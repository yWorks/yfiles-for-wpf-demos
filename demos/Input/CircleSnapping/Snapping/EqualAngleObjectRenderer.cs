/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Input.CircleSnapping.Snapping
{
  /// <summary>
  ///   Renders the visualization for snap lines that are bisecting the angles of two consecutive sibling nodes.
  /// </summary>
  public sealed class EqualAngleObjectRenderer : ObjectRendererBase<SnapResult, EqualAngleVisual>
  {
    public static readonly EqualAngleObjectRenderer Instance = new();

    protected override EqualAngleVisual CreateVisual(IRenderContext context, SnapResult snapResult) {
      // determine the pen to render with
      var pen = context.CanvasControl.TryFindResource(SnapReference.SnapReferencePenKey) as Pen ?? Pens.Gray;
      return snapResult.Reference is EqualAngleSnapLine ? new EqualAngleVisual(context, snapResult, pen) : null;
    }
  }

  public sealed class EqualAngleVisual : VisualGroup
  {
    public EqualAngleVisual(IRenderContext context, SnapResult snapResult, Pen pen) {
      if (snapResult.Reference is not EqualAngleSnapLine snapReference || snapResult.Item is not INode node) {
        return;
      }
      // render the lines and arcs in view coordinates
      RenderTransform = context.IntermediateTransform;

      // convert the utility points for the lines and arcs to view coordinates
      var p = ToView(context, snapReference.Parent.Layout.GetCenter());
      var s1 = ToView(context, snapReference.FirstSibling.Layout.GetCenter());
      var s2 = ToView(context, snapReference.SecondSibling.Layout.GetCenter());
      var c = ToView(context, snapReference.CenteredPoint);
      var snappedLocation = ToView(context, node.Layout.GetCenter());

      // add the three lines from the parent through the siblings and the moved node
      var line1 = new Line() { X1 = p.X, Y1 = p.Y, X2 = s1.X, Y2 = s1.Y };
      line1.SetPen(pen);
      Add(line1);

      var line2 = new Line() { X1 = p.X, Y1 = p.Y, X2 = s2.X, Y2 = s2.Y };
      line2.SetPen(pen);
      Add(line2);

      var line3 = new Line() { X1 = p.X, Y1 = p.Y, X2 = c.X, Y2 = c.Y };
      line3.SetPen(pen);
      Add(line3);

      // to indicate the same angle between the three lines, we add double arcs
      // the first arcs shall be placed at half the distance to the closest node (from the parent)
      var toStart = s1 - p;
      var toEnd = s2 - p;
      var toSnapped = snappedLocation - p;
      var shortestDistance = Math.Min(Math.Min(toStart.Length, toEnd.Length), toSnapped.Length);
      var indicatorRadius = shortestDistance * 0.5;
      // the second arc uses a radius increased by 5
      var equalAngleIndicatorRadius = indicatorRadius + 5;

      // to use an ArcSegment we need to determine whether the arc is large (> Math.PI) and rendered clockwise
      // we consider the line orthogonal to the line from p to snappedLocation:
      // if the snappedLocation and s1 are on different sides of this line, we need a large arc
      var isLargeArc = IsLeft(0, 0, toSnapped.Y, -toSnapped.X, toSnapped.X, toSnapped.Y) !=
          IsLeft(0, 0, toSnapped.Y, -toSnapped.X, toStart.X, toStart.Y);

      // if s1 is left of the snapped location, we sweep clockwise, otherwise counter-clockwise
      var clockwise = IsLeft(0, 0, toSnapped.X, toSnapped.Y, toStart.X, toStart.Y);
      var sweepDirection = clockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

      toStart.Normalize();
      toEnd.Normalize();
      AddArc(pen, p, indicatorRadius, toStart, toEnd, isLargeArc, sweepDirection);
      AddArc(pen, p, equalAngleIndicatorRadius, toStart, toEnd, isLargeArc, sweepDirection);
    }

    private void AddArc(
        Pen pen,
        Point center,
        double radius,
        Vector toStart,
        Vector toEnd,
        bool isLargeArc,
        SweepDirection sweepDirection) {
      // calculate the start and end location of the arc considering the given radius
      var start = center + (toStart * radius);
      var end = center + (toEnd * radius);
      var arc = new Path {
          Data = new PathGeometry {
              Figures = new PathFigureCollection(new[] {
                  new PathFigure(start,
                      new PathSegment[] {
                          new ArcSegment(end, new Size(radius, radius), 0, isLargeArc, sweepDirection, true)
                      }, false)
              })
          }
      };
      arc.SetPen(pen);
      Add(arc);
    }

    public bool IsLeft(double fromX, double fromY, double toX, double toY, double pX, double pY) {
      return (toX - fromX) * (-pY + fromY) - (-toY + fromY) * (pX - fromX) > 0;
    }

    internal static Point ToView(IRenderContext context, PointD worldPoint) {
      return context.WorldToIntermediateCoordinates(worldPoint).ToPoint();
    }
  }
}
