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
using yWorks.Graph;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   A selection renderer wrapper for octilinear edges that uses the original selection renderer for the edge's path and
  ///   additionally renders the bends in the center of the octilinear segments.
  /// </summary>
  internal sealed class OctilinearSelectionRenderer : ISelectionRenderer
  {
    private readonly ISelectionRenderer originalSelectionRenderer;

    internal OctilinearSelectionRenderer(ISelectionRenderer originalSelectionRenderer) {
      this.originalSelectionRenderer = originalSelectionRenderer;
    }

    public IBoundsProvider GetBoundsProvider(object? renderTag) =>
        originalSelectionRenderer.GetBoundsProvider(renderTag);

    public IHitTestable GetHitTestable(object? renderTag) => originalSelectionRenderer.GetHitTestable(renderTag);

    public IVisibilityTestable GetVisibilityTestable(object? renderTag) =>
        originalSelectionRenderer.GetVisibilityTestable(renderTag);

    public IVisualCreator GetVisualCreator(object? renderTag) {
      var edge = renderTag as IEdge ??
          throw new ArgumentException("The selection renderer expects an edge tag.", nameof(renderTag));
      return new OctilinearSelectionVisualCreator(edge, originalSelectionRenderer.GetVisualCreator(renderTag));
    }
  }

  /// <summary>
  ///   An <see cref="IVisualCreator"/> that creates the visual representation of the edge selection by using the original
  ///   selection renderer for the edge's path and additional bend indicators in the center of the octilinear segments.
  /// </summary>
  internal sealed class OctilinearSelectionVisualCreator : IVisualCreator
  {
    private readonly IEdge edge;
    private readonly IVisualCreator originalVisualCreator;

    internal OctilinearSelectionVisualCreator(IEdge edge, IVisualCreator originalVisualCreator) {
      this.edge = edge;
      this.originalVisualCreator = originalVisualCreator;
    }

    public Visual CreateVisual(IRenderContext context) {
      var group = new VisualGroup();
      // use the original selection renderer for the edge's path
      var originalVisual = originalVisualCreator.CreateVisual(context);
      if (originalVisual is VisualGroup { Children: { Count: 2 } children } selectionGroup &&
          children[1] is DrawingVisual) {
        // Remove the original selection renderer's bend indicators
        selectionGroup.Children.RemoveAt(1);
      }
      if (originalVisual != null) {
        group.Children.Add(originalVisual);
      }

      // create bend indicators in the center of the octilinear segments
      var bendIndicators = new DrawingVisual();
      using (var drawingContext = bendIndicators.RenderOpen()) {
        foreach (var bend in edge.Bends) {
          var center = Utils.GetLocation(bend) + Utils.GetHandleOffset(bend);
          var size = 3 / context.Zoom;
          var diamond = new StreamGeometry();
          using (var geometryContext = diamond.Open()) {
            geometryContext.BeginFigure(new Point(center.X, center.Y - size), true, true);
            geometryContext.LineTo(new Point(center.X + size, center.Y), true, false);
            geometryContext.LineTo(new Point(center.X, center.Y + size), true, false);
            geometryContext.LineTo(new Point(center.X - size, center.Y), true, false);
          }
          diamond.Freeze();
          drawingContext.DrawGeometry(Brushes.Black, null, diamond);
        }
      }

      group.Children.Add(bendIndicators);
      return group;
    }

    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisual(context);
  }
}
