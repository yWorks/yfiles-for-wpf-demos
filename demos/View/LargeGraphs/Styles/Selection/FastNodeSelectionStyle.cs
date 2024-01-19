/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.Selection
{
  /// <summary>
  ///   Node style that is used as a zoom-invariant selection decorator.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This style essentially displays a rectangle and scales its stroke thickness and brush by 1 / zoom level.
  ///     This means that positioning and size considerations can still be done in world coordinates and the rectangle
  ///     doesn't require a series of transformations to end up where it should be. The brush is scaled because the default
  ///     selection decoration uses a pixel checkerboard pattern which would otherwise be scaled with the zoom level.
  ///   </para>
  ///   <para>
  ///     This style caches the scaled stroke brush to avoid creating a new brush for every invocation of
  ///     <seealso cref="UpdateVisual" />. Thus, this style cannot be shared over multiple <see cref="GraphControl" />
  ///     instances because the zoom level might differ. If the stroke brush is simply a solid color the scaling step can be
  ///     omitted.
  ///   </para>
  /// </remarks>
  /// <seealso cref="LargeGraphsWindow.SetSelectionDecorators" />
  public class FastNodeSelectionStyle : NodeStyleBase<Rectangle>
  {
    /// <summary>
    ///   The cached instance of the scaled stroke brush.
    /// </summary>
    private Brush scaledBrush;

    /// <summary>
    ///   The scale at which the cached stroke brush was scaled.
    /// </summary>
    /// <remarks>Scale is 1 / zoom level.</remarks>
    private double scaledBrushScale;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FastNodeSelectionStyle" /> class with the given fill brush and stroke
    ///   pen.
    /// </summary>
    /// <param name="fill">The brush used to fill the selection rectangle.</param>
    /// <param name="stroke">The pen used to draw the rectangle outline.</param>
    public FastNodeSelectionStyle(Brush fill, Pen stroke) {
      Fill = fill;
      Stroke = stroke;
    }

    /// <summary>
    ///   Gets or sets the brush used to fill the selection rectangle.
    /// </summary>
    public Brush Fill { get; set; }

    /// <summary>
    ///   Gets or sets the pen used to draw the rectangle outline.
    /// </summary>
    public Pen Stroke { get; set; }

    #region Style

    /// <inheritdoc />
    protected override Rectangle CreateVisual(IRenderContext context, INode node) {
      var scale = 1 / context.Zoom;
      var layout = GetSelectionBounds(node, scale);
      var rect = CreateRectangle(layout);
      rect.SetRenderDataCache(layout);
      UpdateStroke(rect, scale);
      return rect;
    }

    /// <inheritdoc />
    protected override Rectangle UpdateVisual(IRenderContext context, Rectangle oldVisual, INode node) {
      var scale = 1 / context.Zoom;
      var layout = GetSelectionBounds(node, scale);
      var oldLayout = oldVisual.GetRenderDataCache<RectD>();

      if (layout.GetSize() != oldLayout.GetSize()) {
        // If the size changed we need to update the rectangle size.
        oldVisual.Width = layout.Width;
        oldVisual.Height = layout.Height;
        CanvasControl.SetCanvasControlArrangeRect(oldVisual, layout);
        oldVisual.SetRenderDataCache(layout);
        // ReSharper disable CompareOfFloatsByEqualityOperator
      } else if (layout.X != oldLayout.X || layout.Y != oldLayout.Y) {
        // ReSharper restore CompareOfFloatsByEqualityOperator
        // But if only the position changed, we can get away with just arranging the rectangle in the canvas again.
        CanvasControl.SetCanvasControlArrangeRect(oldVisual, layout);
        oldVisual.SetRenderDataCache(layout);
      }

      // Re-create the cached scaled stroke brush if necessary and set it on the rectangle.
      UpdateStroke(oldVisual, scale);

      return oldVisual;
    }

    #endregion

    /// <summary>
    ///   Creates the rectangle with the necessary properties.
    /// </summary>
    /// <param name="layout">The intended layout for the rectangle.</param>
    /// <returns></returns>
    private Rectangle CreateRectangle(RectD layout) {
      var rect = new Rectangle
      {
        Width = layout.Width,
        Height = layout.Height,
        Fill = Fill,
        Stroke = Stroke.Brush,
        SnapsToDevicePixels = true
      };

      // Disable everything remotely related to anti-aliasing and pretty scaling.
      RenderOptions.SetEdgeMode(rect, EdgeMode.Aliased);
      // The bitmap scaling mode is necessary for the scaled stroke brush not to show moiré.
      RenderOptions.SetBitmapScalingMode(rect, BitmapScalingMode.NearestNeighbor);

      CanvasControl.SetCanvasControlArrangeRect(rect, layout);

      return rect;
    }

    /// <summary>
    ///   Returns the size and position of the selection rectangle around a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="scale">The scale. This is 1 / zoom level.</param>
    /// <returns>The selection rectangle layout, enlarged by the scaled stroke thickness.</returns>
    private RectD GetSelectionBounds(INode node, double scale) {
      var layout = node.Layout.ToRectD().GetEnlarged(Stroke.Thickness * scale);
      return layout;
    }

    /// <summary>
    ///   Re-creates the scaled stroke brush if necessary and sets it on the rectangle.
    /// </summary>
    /// <param name="shape">The shape whose stroke brush will be updated.</param>
    /// <param name="scale">The scale. This is 1 / zoom level.</param>
    private void UpdateStroke(Shape shape, double scale) {
      shape.StrokeThickness = Stroke.Thickness * scale;
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (scale != scaledBrushScale || scaledBrush == null) {
        // If the cached brush no longer matches the scale, re-create it.
        scaledBrush = Stroke.Brush.Clone();
        scaledBrush.Transform = new ScaleTransform(scale, scale);
        scaledBrush.Freeze();
        scaledBrushScale = scale;
      }
      shape.Stroke = scaledBrush;
    }
  }
}
