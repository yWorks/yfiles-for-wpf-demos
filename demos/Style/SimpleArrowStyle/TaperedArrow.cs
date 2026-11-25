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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using MarkupExtension = System.Windows.Markup.MarkupExtension;
using WpfGeometry = System.Windows.Media.Geometry;

namespace yWorks.Demo.SimpleArrowStyle
{
  /// <summary>
  ///   An arrow that appears like the edge tapers to a point.
  /// </summary>
  [GraphML(MarkupExtensionConverter = typeof(TaperedArrowConverter))]
  public sealed class TaperedArrow : IArrow, IVisualCreator, IBoundsProvider
  {
    // these variables hold the state for the flyweight pattern
    // they are populated in GetVisualCreator and used in the implementations of the IVisualCreator interface.
    private PointD anchor = PointD.Origin;
    private PointD direction = PointD.Origin;

    // the un-rotated bounds of the arrow
    private RectD bounds;

    // backing field properties width, length
    private double width;
    private double length;

    // The shape of the arrow's path.
    // We draw in a normalized coordinate system where the edge is horizontal and meets the target at (0,0)
    // The path is just a simple triangle with length and width 1 - the actual adjustment is done by simply scaling everything later.
    // We create a tiny overlap between the edge and the arrow by painting a fraction over the edge.
    // This avoids anti-aliasing artifacts where the edge meets the arrow.
    private static readonly WpfGeometry PathShape = CreatePathShape();

    /// <summary>
    ///   The fill of this arrow
    /// </summary>
    public Brush Fill { get; set; }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TaperedArrow"/> class.
    /// </summary>
    /// <param name="width">The width of the arrow</param>
    /// <param name="length">The length of the arrow</param>
    /// <param name="fill">The color of the arrow</param>
    public TaperedArrow(double width = 2, double length = 0, Brush? fill = null) {
      this.width = width;
      this.length = length;
      Fill = fill ?? Brushes.Black;
      UpdateBounds();
    }

    /// <summary>
    ///   Returns the length of the arrow, i.e. the distance from the arrow's tip to
    ///   the position where the visual representation of the edge's path should begin.
    ///   This is specified by <see cref="IArrow.Length"/>.
    /// </summary>
    public double Length {
      get => length;
      set {
        length = value;
        UpdateBounds();
      }
    }

    /// <summary>
    ///   Returns the thickness of the arrow.
    /// </summary>
    public double Width {
      get => width;
      set {
        width = value;
        UpdateBounds();
      }
    }

    /// <summary>
    ///   Calculates the bounds for the current values of length and width.
    /// </summary>
    private void UpdateBounds() {
      bounds = new RectD(-length * 1.1, -width / 2, length * 1.1, width);
    }

    /// <summary>
    ///   Gets the cropping length associated with this instance.
    ///   Value: Always returns 0
    ///   This value is used by <see cref="IEdgeStyle"/>s to let the
    ///   edge appear to end shortly before its actual target.
    ///   This is specified by <see cref="IArrow.CropLength"/>.
    /// </summary>
    public double CropLength => 0;

    /// <summary>
    ///   Creates the visual for the arrow.
    /// </summary>
    /// <param name="context">The context that contains the information needed to create the visual.</param>
    /// <returns>The arrow visual.</returns>
    /// <remarks>Implements <see cref="IVisualCreator.CreateVisual"/>.</remarks>
    public Visual CreateVisual(IRenderContext context) {
      var visual = new DrawingVisual();
      DrawArrow(visual, Fill);
      CanvasControl.SetRenderDataCache(visual, new TaperedArrowRenderData(Fill));
      UpdateTransform(visual);
      return visual;
    }

    /// <summary>
    ///   Re-renders the arrow using the old visual for performance reasons.
    /// </summary>
    /// <param name="context">The context that contains the information needed to create the visual.</param>
    /// <param name="oldVisual">
    ///   The visual instance that had been returned the last time the <see cref="UpdateVisual"/> method
    ///   was called.
    /// </param>
    /// <returns>The updated visual.</returns>
    /// <remarks>Implements <see cref="IVisualCreator.UpdateVisual"/>.</remarks>
    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is not DrawingVisual visual) {
        return CreateVisual(context);
      }

      if (CanvasControl.GetRenderDataCache(visual) is not TaperedArrowRenderData cache ||
          !ReferenceEquals(Fill, cache.Fill)) {
        DrawArrow(visual, Fill);
        CanvasControl.SetRenderDataCache(visual, new TaperedArrowRenderData(Fill));
      }

      UpdateTransform(visual);
      return visual;
    }

    private static void DrawArrow(DrawingVisual visual, Brush fill) {
      using var drawingContext = visual.RenderOpen();
      drawingContext.DrawGeometry(fill, null, PathShape);
    }

    private void UpdateTransform(DrawingVisual visual) {
      visual.Transform = new MatrixTransform(new Matrix(direction.X * length, direction.Y * length,
          -direction.Y * width, direction.X * width, anchor.X, anchor.Y));
    }

    private static WpfGeometry CreatePathShape() {
      var geometry = new StreamGeometry();
      using (var context = geometry.Open()) {
        context.BeginFigure(new Point(-1.1, -0.5), true, true);
        context.LineTo(new Point(-1, -0.5), true, false);
        context.LineTo(new Point(0, 0), true, false);
        context.LineTo(new Point(-1, 0.5), true, false);
        context.LineTo(new Point(-1.1, 0.5), true, false);
      }

      geometry.Freeze();
      return geometry;
    }

    /// <summary>
    ///   Returns the bounds of the arrow for the current flyweight configuration.
    /// </summary>
    /// <param name="context">The context to calculate the bounds for.</param>
    /// <remarks>Implements <see cref="IBoundsProvider.GetBounds"/>.</remarks>
    public RectD GetBounds(ICanvasContext context) {
      var matrix = new Matrix2D(direction.X, direction.Y, -direction.Y, direction.X, anchor.X, anchor.Y);
      matrix.Scale(length, length);
      return matrix.CalculateTransformedBounds(bounds);
    }

    /// <summary>
    ///   Gets an <see cref="IBoundsProvider"/> implementation that can yield
    ///   this arrow's bounds if painted at the given location using the
    ///   given direction for the given edge.
    ///   The framework calls this method before using the implementation and does not cache the
    ///   returned instance, allowing this arrow to use the flyweight pattern.
    /// </summary>
    /// <param name="edge">The edge this arrow belongs to.</param>
    /// <param name="atSource">Whether this will be the source arrow.</param>
    /// <param name="anchor">The anchor point for the tip of the arrow.</param>
    /// <param name="directionVector">The direction the arrow is pointing in.</param>
    /// <returns>An implementation of <see cref="IBoundsProvider"/> that can subsequently be queried.</returns>
    /// <remarks>Implements <see cref="IArrow.GetBoundsProvider"/>.</remarks>
    public IBoundsProvider GetBoundsProvider(IEdge edge, bool atSource, PointD anchor, PointD directionVector) {
      this.anchor = anchor;
      direction = directionVector;
      return this;
    }

    /// <summary>
    ///   Gets an <see cref="IVisualCreator"/> implementation that will create
    ///   the visual for this arrow at the given location using the given direction.
    /// </summary>
    /// <param name="edge">The edge this arrow belongs to.</param>
    /// <param name="atSource">Whether this will be the source arrow.</param>
    /// <param name="anchor">The anchor point for the tip of the arrow.</param>
    /// <param name="direction">The direction the arrow is pointing in.</param>
    /// <returns>This arrow as a flyweight.</returns>
    /// <remarks>Implements <see cref="IArrow.GetVisualCreator"/>.</remarks>
    public IVisualCreator GetVisualCreator(IEdge edge, bool atSource, PointD anchor, PointD direction) {
      this.anchor = anchor;
      this.direction = direction;
      return this;
    }

    public bool CropAtPort => false;

    private readonly record struct TaperedArrowRenderData(Brush Fill);
  }

  /// <summary>
  ///   A markup extension class used for (de-)serializing the custom arrow style to GraphML.
  /// </summary>
  public sealed class TaperedArrowExtension : MarkupExtension
  {
    public double Width { get; set; } = 2;

    public double Length { get; set; }

    public Brush Fill { get; set; } = Brushes.Black;

    /// <summary>
    ///   Returns an object that is set as the value of the target property for this markup extension.
    /// </summary>
    /// <param name="serviceProvider">The object that provides services for this markup extension.</param>
    public override object ProvideValue(IServiceProvider serviceProvider) => new TaperedArrow(Width, Length, Fill);

    public static TaperedArrowExtension Create(TaperedArrow item) =>
        new() {
            Width = item.Width,
            Length = item.Length,
            Fill = item.Fill
        };
  }

  /// <summary>
  ///   Converts <see cref="TaperedArrow"/> instances to their GraphML markup extension.
  /// </summary>
  public sealed class TaperedArrowConverter : IMarkupExtensionConverter
  {
    public bool CanConvert(IWriteContext context, object value) => value is TaperedArrow;

    public MarkupExtension Convert(IWriteContext context, object value) =>
        TaperedArrowExtension.Create((TaperedArrow) value);
  }
}
