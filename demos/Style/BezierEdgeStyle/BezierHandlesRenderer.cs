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

using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.BezierEdgeStyle
{
  /// <summary>
  ///   Renders lines from the other control points to their inner control point. This wraps the default handles
  ///   renderer and adds the lines for the outer control points.
  /// </summary>
  public sealed class BezierHandlesRenderer : ObjectRendererBase<HandlesRenderTag, VisualGroup>
  {
    private readonly IObjectRenderer coreRenderer;
    private static readonly Pen SelectionPen = CreateSelectionPen();

    public BezierHandlesRenderer(IObjectRenderer coreRenderer) {
      this.coreRenderer = coreRenderer;
    }

    protected override VisualGroup CreateVisual(IRenderContext context, HandlesRenderTag renderTag) {
      var mainGroup = new VisualGroup();
      var controlPointLinesGroup = new VisualGroup();
      mainGroup.Add(controlPointLinesGroup);
      mainGroup.Add(coreRenderer.GetVisualCreator(renderTag).CreateVisual(context));
      return UpdateVisual(context, mainGroup, renderTag);
    }

    protected override VisualGroup UpdateVisual(
        IRenderContext context,
        VisualGroup oldVisual,
        HandlesRenderTag renderTag) {
      if (oldVisual.Children.Count == 2 && oldVisual.Children[0] is VisualGroup controlPointLinesGroup) {
        controlPointLinesGroup.Children.Clear();
        foreach (var handle in renderTag.Handles) {
          var bend = handle switch {
              OuterControlPointHandle outer => outer.Bend,
              SimpleControlPointHandle simple when simple.IsOuterControlPoint => simple.Bend,
              _ => null
          };
          if (bend == null) {
            continue;
          }

          var middleBendLocation = GetMiddleBendLocation(bend);
          var handleLocation = new PointD(handle.Location.X, handle.Location.Y);
          var center = context.WorldToViewCoordinates(handleLocation);
          var otherPoint = context.WorldToViewCoordinates(middleBendLocation);
          var line = new DrawingVisual();
          using (var drawingContext = line.RenderOpen()) {
            drawingContext.DrawLine(SelectionPen, new Point(center.X, center.Y), new Point(otherPoint.X, otherPoint.Y));
          }
          controlPointLinesGroup.Add(line);
        }
        controlPointLinesGroup.Transform = context.ViewTransform;

        var oldHandles = oldVisual.Children[1];
        var updatedHandles = coreRenderer.GetVisualCreator(renderTag).UpdateVisual(context, oldHandles);
        if (!ReferenceEquals(updatedHandles, oldHandles)) {
          oldVisual.Children[1] = updatedHandles;
        }
        return oldVisual;
      }

      // If something changed, rebuild the visual structure.
      context.ChildVisualRemoved(oldVisual);
      return CreateVisual(context, renderTag);
    }

    /// <summary>Returns the location of the inner control point for the given bend.</summary>
    private static PointD GetMiddleBendLocation(IBend bend) {
      var bendIndex = bend.GetIndex();
      var middleIndexDifference = (bendIndex % 3) switch {
          0 => -1, // First bend belongs to source port; third of triplet to previous bend.
          1 => 1,  // Last bend belongs to target port; first of triplet to next bend.
          2 => 0,  // Middle bend in triplet.
          _ => 0
      };

      // +1 because path points begin with the source port.
      var pathPoints = bend.Owner.GetPathPoints();
      var point = pathPoints[bendIndex + 1 + middleIndexDifference];
      return point is PointD pointD ? pointD : new PointD(point.X, point.Y);
    }

    private static Pen CreateSelectionPen() {
      var pen = new Pen(Brushes.LightGray, 2) { DashStyle = DashStyles.Dash };
      pen.Freeze();
      return pen;
    }
  }
}
