/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.CircleSnapping
{
  /// <summary>
  ///   Renders the visualization for snap lines that bisect the angles of two consecutive sibling nodes.
  /// </summary>
  internal sealed class EqualAngleObjectRenderer : ObjectRendererBase<SnapResult, DrawingVisual>
  {
    public static readonly EqualAngleObjectRenderer Instance = new();

    protected override DrawingVisual CreateVisual(IRenderContext context, SnapResult renderTag) {
      if (renderTag.Item is not INode node || renderTag.Reference is not EqualAngleSnapLine snapReference) {
        return new DrawingVisual();
      }

      var parent = context.WorldToIntermediateCoordinates(snapReference.Parent.Layout.GetCenter());
      var first = context.WorldToIntermediateCoordinates(snapReference.FirstSibling.Layout.GetCenter());
      var second = context.WorldToIntermediateCoordinates(snapReference.SecondSibling.Layout.GetCenter());
      var center = context.WorldToIntermediateCoordinates(snapReference.CenteredPoint);
      var snappedLocation = context.WorldToIntermediateCoordinates(node.Layout.GetCenter());

      var toStart = first - parent;
      var toEnd = second - parent;
      var toSnapped = snappedLocation - parent;
      var shortestDistance = Math.Min(Math.Min(toStart.VectorLength, toEnd.VectorLength), toSnapped.VectorLength);
      var indicatorRadius = shortestDistance * 0.5;
      // the second arc uses a radius increased by 5
      var equalAngleIndicatorRadius = indicatorRadius + 5;

      // To use an arc we need to determine whether the arc is large (> Math.PI) and rendered clockwise.
      // We consider the line orthogonal to the line from p to snappedLocation:
      // If the snappedLocation and first are on different sides of this line, we need a large arc
      var normal = new PointD(toSnapped.Y, -toSnapped.X);
      var largeArc = IsLeft(normal, toSnapped) != IsLeft(normal, toStart);

      // if first is left of the snapped location, we sweep clockwise, otherwise counter-clockwise
      var clockwise = IsLeft(toSnapped, toStart);

      var start = toStart.Normalized;
      var end = toEnd.Normalized;

      var visual = new DrawingVisual();
      using (var dc = visual.RenderOpen()) {
        dc.PushTransform(context.IntermediateTransform);
        var pen = new Pen(Brushes.Gray, 1);
        // draw the three lines from the parent through the siblings and the moved node
        dc.DrawLine(pen, parent.ToPoint(), first.ToPoint());
        dc.DrawLine(pen, parent.ToPoint(), second.ToPoint());
        dc.DrawLine(pen, parent.ToPoint(), center.ToPoint());

        // to indicate the same angle between the three lines, we add double arcs
        // the first arcs shall be placed at half the distance to the closest node (from the parent)
        DrawArc(dc, pen, parent, indicatorRadius, start, end, largeArc, clockwise);
        DrawArc(dc, pen, parent, equalAngleIndicatorRadius, start, end, largeArc, clockwise);
        dc.Pop();
      }
      return visual;
    }

    private static bool IsLeft(PointD p1, PointD p2) => p1.X * -p2.Y - -p1.Y * p2.X > 0;

    private static void DrawArc(
        DrawingContext dc,
        Pen pen,
        PointD center,
        double radius,
        PointD startDirection,
        PointD endDirection,
        bool largeArc,
        bool clockwise) {
      var start = center + startDirection * radius;
      var end = center + endDirection * radius;
      var geometry = new StreamGeometry();
      using (var context = geometry.Open()) {
        context.BeginFigure(start.ToPoint(), false, false);
        context.ArcTo(end.ToPoint(), new Size(radius, radius), 0, largeArc,
            clockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, false);
      }
      geometry.Freeze();
      dc.DrawGeometry(null, pen, geometry);
    }
  }
}
