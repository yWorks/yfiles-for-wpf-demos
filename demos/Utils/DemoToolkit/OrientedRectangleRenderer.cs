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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;

namespace yWorks.Demo
{
  /// <summary>
  ///   A utility class for anything that needs to render a rotated rectangle, e.g., a selection, focus or
  ///   highlight visualization for rotatable items.
  /// </summary>
  /// <remarks>
  ///   This is a convenience implementation that takes care of the transformation and zoom-invariant rendering.
  /// </remarks>
  /// <typeparam name="TRenderTag">
  ///   The type of the <see cref="IRenderTreeElement.Tag">render tag</see> that is rendered. This class will
  ///   cast the render tag to this type before passing it to its methods.
  /// </typeparam>
  public class OrientedRectangleRenderer<TRenderTag> : ObjectRendererBase<TRenderTag, VisualGroup>, ISelectionRenderer,
      IFocusRenderer, IHighlightRenderer
  {
    public Pen Pen { get; set; } = Pens.Black;

    public Brush Brush { get; set; } = Brushes.Transparent;

    public Func<TRenderTag, IOrientedRectangle> LayoutProvider { get; }

    protected readonly bool useViewCoordinates;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="layoutProvider">A callback providing the layout for the item.</param>
    /// <param name="useViewCoordinates">
    ///   Gets or sets a value indicating whether the visualization is drawn in
    ///   view coordinates or intermediate coordinates.
    /// </param>
    public OrientedRectangleRenderer(
        Func<TRenderTag, IOrientedRectangle> layoutProvider,
        bool useViewCoordinates = true) {
      LayoutProvider = layoutProvider;
      this.useViewCoordinates = useViewCoordinates;
    }

    /// <summary>
    ///   Creates the visual that visualizes the renderItem.
    /// </summary>
    /// <param name="context">The current render context.</param>
    /// <param name="size">The size used for the visualization.</param>
    /// <param name="renderTag">The tag that is rendered</param>
    /// <returns>A visual that visualizes the renderItem.</returns>
    protected virtual Visual CreateIndicatorElement(IRenderContext context, ISize size, TRenderTag renderTag) {
      var rectangle = new Rectangle {
          Width = size.Width,
          Height = size.Height,
          Fill = Brush,
          AllowDrop = true
      };
      rectangle.SetPen(Pen);
      var rect = new Rect(new Point(0, 0), size.ToSize());
      rectangle.SetCanvasArrangeRect(rect);
      rectangle.SetRenderDataCache(rect);
      return rectangle;
    }

    /// <summary>
    ///   Updates the visual element that visualizes the renderItem.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="size">The size of the visualization.</param>
    /// <param name="renderTag">The item to render.</param>
    /// <param name="oldVisual">The visual element to update.</param>
    /// <returns>The updated visual.</returns>
    protected virtual Visual UpdateIndicatorElement(
        IRenderContext context,
        ISize size,
        TRenderTag renderTag,
        Visual oldVisual) {
      if (oldVisual is Rectangle rectangle) {
        var w = size.Width;
        var h = size.Height;

        var cachedLayout = rectangle.GetRenderDataCache<Rect>();
        var widthChanged = w != cachedLayout.Width;
        var heightChanged = h != cachedLayout.Height;
        if (widthChanged || heightChanged) {
          if (widthChanged) {
            rectangle.Width = w;
          }
          if (heightChanged) {
            rectangle.Height = h;
          }
          var rect = new Rect(new Point(0, 0), size.ToSize());
          rectangle.SetCanvasArrangeRect(rect);
          rectangle.SetRenderDataCache(rect);
        }
        rectangle.Fill = Brush;
        rectangle.SetPen(Pen);

        return rectangle;
      }
      context.ChildVisualRemoved(oldVisual);
      return CreateIndicatorElement(context, size, renderTag);
    }

    protected override VisualGroup CreateVisual(IRenderContext context, TRenderTag renderTag) {
      var g = new VisualGroup();
      if (renderTag == null) {
        return g;
      }

      var layout = LayoutProvider(renderTag);
      var intermediateLayout = useViewCoordinates ? GetIntermediateLayout(context, layout) : layout;

      var wrappedEl = CreateIndicatorElement(context, intermediateLayout.ToSizeD(), renderTag);

      if (wrappedEl != null) {
        g.Add(wrappedEl);
        UpdateTransform(context, intermediateLayout, g);
      }
      return g;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, TRenderTag renderTag) {
      if (renderTag == null) {
        return CreateVisual(context, renderTag);
      }

      var layout = LayoutProvider(renderTag);
      var intermediateLayout = useViewCoordinates ? GetIntermediateLayout(context, layout) : layout;
      var oldWrappedEl = oldVisual.Children.Count == 1 ? oldVisual.Children[0] : null;

      var wrappedEl = oldWrappedEl != null
          ? UpdateIndicatorElement(context, intermediateLayout.ToSizeD(), renderTag, oldWrappedEl)
          : CreateIndicatorElement(context, intermediateLayout.ToSizeD(), renderTag);
      if (wrappedEl != oldWrappedEl) {
        if (oldWrappedEl != null) {
          oldVisual.Remove(oldWrappedEl);
        }
        if (wrappedEl != null) {
          oldVisual.Add(wrappedEl);
        }
      }
      UpdateTransform(context, intermediateLayout, oldVisual);
      return oldVisual;
    }

    private static IOrientedRectangle GetIntermediateLayout(IRenderContext context, IOrientedRectangle layout) {
      var anchorX = layout.AnchorX;
      var anchorY = layout.AnchorY;
      var upX = layout.UpX;
      var upY = layout.UpY;
      var topLeft = new PointD(anchorX + upX * layout.Height, anchorY + upY * layout.Height);
      var bottomRight = new PointD(anchorX + -upY * layout.Width, anchorY + upX * layout.Width);
      var anchor = context.WorldToIntermediateCoordinates(new PointD(anchorX, anchorY));
      topLeft = context.WorldToIntermediateCoordinates(topLeft);
      bottomRight = context.WorldToIntermediateCoordinates(bottomRight);

      var or = new OrientedRectangle();
      or.SetUpVector(PointD.Subtract(topLeft, anchor).Normalized);
      or.AnchorX = anchor.X;
      or.AnchorY = anchor.Y;
      or.Width = anchor.DistanceTo(bottomRight);
      or.Height = anchor.DistanceTo(topLeft);
      return or;
    }

    private void UpdateTransform(IRenderContext context, IOrientedRectangle intermediateLayout, VisualGroup g) {
      Transform layoutTransform = new MatrixTransform(-intermediateLayout.UpY, intermediateLayout.UpX,
          intermediateLayout.UpX, intermediateLayout.UpY, intermediateLayout.AnchorX, intermediateLayout.AnchorY);
      if (useViewCoordinates) {
        layoutTransform = new TransformGroup {
            Children = {
                layoutTransform,
                context.IntermediateTransform
            }
        };
      }
      g.Transform = layoutTransform;
    }
  }
}
