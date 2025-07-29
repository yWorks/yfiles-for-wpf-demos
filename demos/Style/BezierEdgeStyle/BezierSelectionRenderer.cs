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

using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.BezierEdgeStyle
{
  /// <summary>
  ///   Custom decoration decorator that adds a rendering of the curves control point segments
  /// </summary>
  /// <remarks>
  ///   This implementation adds as a decorator for an existing decorator and just adds the control point rendering on
  ///   top.
  /// </remarks>
  internal sealed class BezierSelectionRenderer : ISelectionRenderer
  {
    /// <summary>
    ///   The style for the control point segments
    /// </summary>
    /// <remarks>
    ///   We just use a polyline edge style with a custom render so that we can reuse most of the existing rendering
    ///   implementations.
    /// </remarks>
    private static readonly IEdgeStyle SelectionDecoratorStyle = new SelectionEdgeStyle();

    private readonly CompositeRenderer renderer;

    /// <summary>
    ///   Create a new instance that decorates the <paramref name="coreImpl"/>
    /// </summary>
    /// <param name="coreImpl">The core indicator that is again decorated by this instance</param>
    public BezierSelectionRenderer([CanBeNull] ISelectionRenderer coreImpl) {
      var decorator = new EdgeStyleIndicatorRenderer {
          EdgeStyle = SelectionDecoratorStyle, ZoomPolicy = StyleIndicatorZoomPolicy.ViewCoordinates
      };
      renderer = new CompositeRenderer(coreImpl, decorator);
    }

    public IVisualCreator GetVisualCreator(object renderTag) {
      renderer.Configure(renderTag);
      return renderer;
    }

    public IBoundsProvider GetBoundsProvider(object renderTag) {
      renderer.Configure(renderTag);
      return renderer;
    }

    public IVisibilityTestable GetVisibilityTestable(object renderTag) {
      renderer.Configure(renderTag);
      return renderer;
    }

    public IHitTestable GetHitTestable(object renderTag) {
      renderer.Configure(renderTag);
      return renderer;
    }

    private sealed class CompositeRenderer : IVisualCreator, IBoundsProvider, IVisibilityTestable, IHitTestable
    {
      private readonly IObjectRenderer first;
      private readonly IObjectRenderer second;
      private object renderTag;

      public CompositeRenderer(IObjectRenderer first, IObjectRenderer second) {
        this.first = first;
        this.second = second;
      }

      public void Configure(object renderTag) {
        this.renderTag = renderTag;
      }

      public Visual CreateVisual(IRenderContext context) {
        var container = new VisualGroup();

        if (first != null) {
          container.Add(first.GetVisualCreator(renderTag).CreateVisual(context));
        }

        if (second != null) {
          container.Add(second.GetVisualCreator(renderTag).CreateVisual(context));
        }

        return container;
      }

      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        if (oldVisual is not VisualGroup container || container.Children.Count != 2) {
          return CreateVisual(context);
        }

        first?.GetVisualCreator(renderTag).UpdateVisual(context, container.Children[0]);
        second?.GetVisualCreator(renderTag).UpdateVisual(context, container.Children[1]);

        return oldVisual;
      }

      public RectD GetBounds(ICanvasContext context) {
        var bounds = new MutableRectangle();
        if (first != null) {
          bounds.Add(first.GetBoundsProvider(renderTag).GetBounds(context));
        }
        if (second != null) {
          bounds.Add(second.GetBoundsProvider(renderTag).GetBounds(context));
        }
        return bounds.ToRectD();
      }

      public bool IsVisible(ICanvasContext context, RectD rectangle) {
        return (first != null && first.GetVisibilityTestable(renderTag).IsVisible(context, rectangle)) ||
            (second != null && second.GetVisibilityTestable(renderTag).IsVisible(context, rectangle));
      }

      public bool IsHit(IInputModeContext context, PointD location) {
        return (first != null && first.GetHitTestable(renderTag).IsHit(context, location)) ||
            (second != null && second.GetHitTestable(renderTag).IsHit(context, location));
      }
    }

    /// <summary>
    ///   Custom edge style that renders a line segment for collinear control point triples
    /// </summary>
    private sealed class SelectionEdgeStyle : PathEdgeStyleBase
    {
      private static readonly Pen SelectionPen =
          (Pen) new Pen(Brushes.LightGray, 1) { DashStyle = DashStyles.Dash }.GetAsFrozen();

      protected override GeneralPath GetPath(IEdge edge) {
        var pathPoints = edge.GetPathPoints();
        var gp = new GeneralPath(pathPoints.Count + 1);
        gp.MoveTo(pathPoints[0]);

        for (var i = 1; i < pathPoints.Count; ++i) {
          if (i % 3 == 2) {
            // Skip to the next triple
            gp.MoveTo(pathPoints[i]);
          } else {
            // Draw a line to the next control pint in the triple
            gp.LineTo(pathPoints[i]);
          }
        }
        return gp;
      }

      protected override GeneralPath GetCroppedPath(IEdge edge) {
        // this style should not be cropped at the adjacent nodes - we want to see the uncropped points
        return GetPath(edge);
      }

      protected override Pen GetPen(IEdge edge) {
        return SelectionPen;
      }
    }
  }
}
