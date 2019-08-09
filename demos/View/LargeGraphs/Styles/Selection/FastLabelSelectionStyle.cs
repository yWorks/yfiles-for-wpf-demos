/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  ///   Label style that is used as a zoom-invariant selection decorator.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This style essentially displays a rotated rectangle and scales its stroke thickness by 1 / zoom level. This means that positioning and size considerations can still be done in world coordinates and the rectangle doesn't require a series of transformations to end up where it should be.
  ///   </para>
  ///   <para>
  ///     Since the default label selection decorator uses a solid color instead of a pattern, this style doesn't scale or cache its stroke brush, unlike <see cref="FastEdgeSelectionStyle"/> or <see cref="FastNodeSelectionStyle"/>.
  ///   </para>
  /// </remarks>
  /// <seealso cref="LargeGraphsWindow.SetSelectionDecorators" />
  public class FastLabelSelectionStyle : LabelStyleBase<Path>
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="FastLabelSelectionStyle" /> class with the given fill brush and stroke pen.
    /// </summary>
    /// <param name="fill">The brush used to fill the selection rectangle.</param>
    /// <param name="stroke">The pen used to draw the rectangle outline.</param>
    public FastLabelSelectionStyle(Brush fill, Pen stroke) {
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
    protected override Path CreateVisual(IRenderContext context, ILabel label) {
      var scale = 1 / context.Zoom;
      var layout = GetSelectionBounds(label, scale);

      // ReSharper disable once UseObjectOrCollectionInitializer
      var path = new Path
      {
        Data = CreateGeometry(layout),
        Fill = Fill,
        Stroke = Stroke.Brush,
        StrokeThickness = Stroke.Thickness * scale,
        SnapsToDevicePixels = true
      };

      // Hack to work around WPF bug. Otherwise the path may not appear at the correct location in some cases.
      path.ClipToBounds = false;
      path.Stretch = Stretch.None;
      path.SetCanvasArrangeRect(new Rect(0, 0, double.MaxValue, double.MaxValue));
      path.MinWidth = path.MinHeight = 1;

      path.SetRenderDataCache(layout);
      return path;
    }

    /// <inheritdoc />
    protected override Path UpdateVisual(IRenderContext context, Path oldVisual, ILabel label) {
      var scale = 1 / context.Zoom;
      var layout = GetSelectionBounds(label, scale);
      var oldLayout = oldVisual.GetRenderDataCache<IOrientedRectangle>();

      // ReSharper disable CompareOfFloatsByEqualityOperator
      if (!AreEqual(layout, oldLayout)) {
        // ReSharper restore CompareOfFloatsByEqualityOperator
        oldVisual.Data = CreateGeometry(layout);
        oldVisual.SetRenderDataCache(layout);
      }
      oldVisual.StrokeThickness = Stroke.Thickness * scale;
      return oldVisual;
    }

    /// <inheritdoc />
    protected override SizeD GetPreferredSize(ILabel label) {
      return label.GetLayout().GetSize();
    }

    #endregion

    /// <summary>
    ///   Creates the geometry for the selection rectangle.
    /// </summary>
    /// <param name="layout">The label's layout.</param>
    /// <returns>The selection rectangle geometry.</returns>
    private Geometry CreateGeometry(IOrientedRectangle layout) {
      var up = layout.GetUp();
      var anchor = layout.GetAnchorLocation();
      var right = new PointD(-up.Y, up.X);

      var g = new StreamGeometry();
      using (var ctx = g.Open()) {
        ctx.BeginFigure(anchor, Fill != null, true);
        ctx.LineTo(anchor + right * layout.Width, true, false);
        ctx.LineTo(anchor + right * layout.Width + up * layout.Height, true, false);
        ctx.LineTo(anchor + up * layout.Height, true, false);
        ctx.LineTo(anchor, true, false);
      }
      return g;
    }

    /// <summary>
    ///   Returns an <see cref="IOrientedRectangle"/> representing the selection rectangle around the label.
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="scale">The scale. This is 1 / zoom level.</param>
    /// <returns>The selection rectangle, enlarged by the scaled stroke thickness.</returns>
    private IOrientedRectangle GetSelectionBounds(ILabel label, double scale) {
      var layout = label.GetLayout();
      // Normally I'd say scale / 2 would be correct here, but scale / 4 reproduces the default label selection exactly.
      var amount = Stroke.Thickness * scale / 4;
      var up = layout.GetUp();
      var anchor = layout.GetAnchorLocation();
      var right = new PointD(-up.Y, up.X);
      var width = layout.Width;
      var height = layout.Height;

      var newAnchor = anchor - up * amount - right * amount;
      var newWidth = width + amount * 2;
      var newHeight = height + amount * 2;

      return new OrientedRectangle(newAnchor.X, newAnchor.Y, newWidth, newHeight, up.X, up.Y);
    }

    /// <summary>
    ///   Helper method to determine whether two <see cref="IOrientedRectangle" />s are equal.
    /// </summary>
    /// <param name="rect1">The first rectangle to compare.</param>
    /// <param name="rect2">The second rectangle to compare.</param>
    /// <returns><see langword="true" /> if both rectangles are equal, <see langword="false" /> otherwise.</returns>
    /// <remarks>
    ///   Two oriented rectangles are considered equal if their anchor location, up vector, width, and height are equal.
    /// </remarks>
    private static bool AreEqual(IOrientedRectangle rect1, IOrientedRectangle rect2) {
      // ReSharper disable CompareOfFloatsByEqualityOperator
      return rect1.AnchorX == rect2.AnchorX ||
             rect1.AnchorY == rect2.AnchorY ||
             rect1.UpX == rect2.UpX ||
             rect1.UpY == rect2.UpY ||
             rect1.Width == rect2.Width ||
             rect1.Height == rect2.Height;
      // ReSharper restore CompareOfFloatsByEqualityOperator
    }
  }
}