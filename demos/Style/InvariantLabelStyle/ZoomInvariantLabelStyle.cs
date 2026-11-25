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
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.InvariantLabelStyle
{
  /// <summary>
  ///   A label style that renders labels at the same size regardless of the zoom level.
  /// </summary>
  /// <remarks>
  ///   The style is implemented as a wrapper for an existing label style.
  ///   Note that due to the way this class caches the layout for the visualization of the selection,
  ///   its instances shouldn't be shared between graph items.
  /// </remarks>
  public abstract class ZoomInvariantLabelStyleBase : LabelStyleBase<VisualGroup>
  {
    private static readonly Pen SelectionPen = CreateSelectionPen();

    public ILabelStyle InnerLabelStyle { get; }

    // These mutable helper objects follow the source style: a label centered at the origin is rendered
    // through a proxy context, then moved and scaled into the original label's position.
    private OrientedRectangle dummyLabelLayout = new();
    private OrientedRectangle dummyLabelBounds = new();
    private SimpleLabel dummyLabel;

    public double ZoomThreshold { get; }

    /// <summary>
    ///   Instantiates a new label style.
    /// </summary>
    /// <param name="innerLabelStyle">The style of the label.</param>
    /// <param name="zoomThreshold">The threshold that controls the label's size.</param>
    protected ZoomInvariantLabelStyleBase(ILabelStyle innerLabelStyle, double zoomThreshold) {
      InnerLabelStyle = innerLabelStyle ?? throw new ArgumentNullException(nameof(innerLabelStyle));
      ZoomThreshold = zoomThreshold;
      dummyLabel = CreateDummyLabel(dummyLabelLayout);
    }

    /// <summary>
    ///   Returns the preferred size calculated by the inner style.
    /// </summary>
    /// <param name="label">The label which will be styled.</param>
    protected override SizeD GetPreferredSize(ILabel label) =>
        InnerLabelStyle.Renderer.GetPreferredSize(label, InnerLabelStyle);

    /// <summary>
    ///   Determines the scale factor for the given label and zoom level.
    /// </summary>
    /// <param name="label">The current label which will be styled.</param>
    /// <param name="zoom">The current zoom level.</param>
    public virtual double GetScaleForZoom(ILabel label, double zoom) =>
        // The base implementation does nothing.
        1;

    /// <summary>
    ///   Creates the visual for the label.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      UpdateDummyLabel(context, label);

      var group = new VisualGroup();
      var scale = GetScaleForZoom(label, context.Zoom);
      var outerTransform = CreateOuterTransform(label, scale, out var inverseOuterTransform);
      var innerContext = new DummyContext(context, scale * context.Zoom, inverseOuterTransform);
      var creator = InnerLabelStyle.Renderer.GetVisualCreator(dummyLabel, InnerLabelStyle);
      group.Add(creator.CreateVisual(innerContext));
      group.Transform = outerTransform;
      return group;
    }

    /// <summary>
    ///   Updates the visual previously created by <see cref="CreateVisual"/>.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual that was created by <see cref="CreateVisual"/>.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      UpdateDummyLabel(context, label);
      if (oldVisual.Children.Count != 1) {
        return CreateVisual(context, label);
      }

      var scale = GetScaleForZoom(label, context.Zoom);
      var outerTransform = CreateOuterTransform(label, scale, out var inverseOuterTransform);
      var innerContext = new DummyContext(context, scale * context.Zoom, inverseOuterTransform);
      var creator = InnerLabelStyle.Renderer.GetVisualCreator(dummyLabel, InnerLabelStyle);
      var oldChild = oldVisual.Children[0];
      var updatedChild = creator.UpdateVisual(innerContext, oldChild);
      if (!ReferenceEquals(updatedChild, oldChild)) {
        oldVisual.Children[0] = updatedChild;
      }

      oldVisual.Transform = outerTransform;
      return oldVisual;
    }

    private static SimpleLabel CreateDummyLabel(OrientedRectangle layout) =>
        new() { LayoutParameter = FreeLabelModel.Instance.CreateDynamic(layout) };

    /// <summary>
    ///   Updates the internal label to match the given original label.
    /// </summary>
    private void UpdateDummyLabel(ICanvasContext context, ILabel original) {
      dummyLabel.Owner = original.Owner;
      dummyLabel.Style = original.Style;
      dummyLabel.Tag = original.Tag;
      dummyLabel.Text = original.Text;

      var originalLayout = original.GetLayout();
      dummyLabelLayout.SetShape(originalLayout);
      dummyLabelLayout.SetCenter(0, 0);
      dummyLabel.PreferredSize = dummyLabelLayout.GetSize();

      var scale = GetScaleForZoom(original, context.Zoom);
      dummyLabelBounds.SetUpVector(originalLayout.GetUpVector());
      dummyLabelBounds.Width = originalLayout.Width * scale;
      dummyLabelBounds.Height = originalLayout.Height * scale;
      dummyLabelBounds.SetCenter(originalLayout.GetCenter());
    }

    private static Transform CreateOuterTransform(ILabel label, double scale, out Transform inverseTransform) {
      // This is the source matrix: its translation is the label center in world coordinates.
      var center = label.GetLayout().GetCenter();
      var matrix = new Matrix(scale, 0, 0, scale, center.X, center.Y);
      var inverse = matrix;
      inverse.Invert();
      inverseTransform = new MatrixTransform(inverse);
      return new MatrixTransform(matrix);
    }

    private RectD ScaleBounds(RectD bounds, ILabel label, double zoom) {
      var scale = GetScaleForZoom(label, zoom);
      var center = bounds.GetCenter();
      var scaledWidth = bounds.Width * scale;
      var scaledHeight = bounds.Height * scale;
      return new RectD(center.X + (bounds.X - center.X) * scale, center.Y + (bounds.Y - center.Y) * scale, scaledWidth,
          scaledHeight);
    }

    /// <summary>
    ///   Creates a new copy of this instance and gives it a separate cached selection layout.
    /// </summary>
    public override object Clone() {
      var clone = (ZoomInvariantLabelStyleBase) base.Clone();
      clone.dummyLabelLayout = new OrientedRectangle();
      clone.dummyLabelBounds = new OrientedRectangle();
      clone.dummyLabel = CreateDummyLabel(clone.dummyLabelLayout);
      return clone;
    }

    /// <summary>
    ///   Gets the bounds of the visual for the label in the given context.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      UpdateDummyLabel(context, label);
      var bounds = InnerLabelStyle.Renderer.GetBoundsProvider(label, InnerLabelStyle).GetBounds(context);
      return ScaleBounds(bounds, label, context.Zoom);
    }

    /// <summary>
    ///   Determines whether the visualization for the specified label is visible in the context.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="rectangle">The clipping rectangle.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) =>
        GetBounds(context, label).Intersects(rectangle);

    /// <summary>
    ///   Determines whether the visual representation of the label has been hit at the given location.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="location">The point to test.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      UpdateDummyLabel(context, label);
      return dummyLabelBounds.Contains(location, 0.001);
    }

    /// <summary>
    ///   Determines whether the visualization for the specified label is included in the marquee selection.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="rectangle">The marquee selection box.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) =>
        GetBounds(context, label).Intersects(rectangle);

    /// <summary>
    ///   Returns an adjusted selection rectangle for the resized label styles.
    /// </summary>
    protected override object Lookup(ILabel label, Type type) {
      if (type == typeof(ISelectionRenderer)) {
        return new OrientedRectangleRenderer<ILabel>(_ => dummyLabelBounds) { Pen = SelectionPen };
      }
      return InnerLabelStyle.Renderer.GetContext(label, InnerLabelStyle).Lookup(type)!;
    }

    private static Pen CreateSelectionPen() {
      var pen = new Pen(new SolidColorBrush(Color.FromRgb(56, 67, 79)), 2);
      pen.Freeze();
      return pen;
    }
  }

  public sealed class ZoomInvariantBelowThresholdLabelStyle : ZoomInvariantLabelStyleBase
  {
    /// <summary>
    ///   Stops the label from getting smaller in view coordinates if the zoom is smaller than the threshold.
    /// </summary>
    public ZoomInvariantBelowThresholdLabelStyle(ILabelStyle innerLabelStyle, double zoomThreshold) : base(
        innerLabelStyle, zoomThreshold) { }

    public override double GetScaleForZoom(ILabel label, double zoom) {
      if (zoom > ZoomThreshold) {
        return 1;
      }
      return ZoomThreshold / zoom;
    }
  }

  public sealed class ZoomInvariantAboveThresholdLabelStyle : ZoomInvariantLabelStyleBase
  {
    /// <summary>
    ///   Stops the label from getting larger in view coordinates if the zoom is greater than the threshold.
    /// </summary>
    public ZoomInvariantAboveThresholdLabelStyle(ILabelStyle innerLabelStyle, double zoomThreshold) : base(
        innerLabelStyle, zoomThreshold) { }

    public override double GetScaleForZoom(ILabel label, double zoom) {
      if (zoom < ZoomThreshold) {
        return 1;
      }
      return ZoomThreshold / zoom;
    }
  }

  public sealed class ZoomInvariantOutsideRangeLabelStyle : ZoomInvariantLabelStyleBase
  {
    public double MaxScale { get; }

    public ZoomInvariantOutsideRangeLabelStyle(ILabelStyle innerLabelStyle, double zoomThreshold, double maxScale) :
        base(innerLabelStyle, zoomThreshold) {
      MaxScale = maxScale;
    }

    /// <summary>
    ///   Stops the label from getting smaller below the threshold and from getting larger above the maximum scale.
    /// </summary>
    public override double GetScaleForZoom(ILabel label, double zoom) {
      if (zoom < ZoomThreshold) {
        return ZoomThreshold / zoom;
      }
      if (zoom > MaxScale) {
        return MaxScale / zoom;
      }
      return 1;
    }
  }

  public sealed class FitOwnerLabelStyle : ZoomInvariantLabelStyleBase
  {
    public FitOwnerLabelStyle(ILabelStyle innerLabelStyle) : base(innerLabelStyle, 1) { }

    /// <summary>
    ///   Scales the label to fit its owner.
    /// </summary>
    public override double GetScaleForZoom(ILabel label, double zoom) {
      var labelWidth = label.GetLayout().Width;
      var ratio = 1.0;
      if (label.Owner is INode node) {
        ratio = Math.Min(1, node.Layout.Width / labelWidth);
      } else if (label.Owner is IEdge edge) {
        var edgeLength = edge.SourcePort.GetLocation().DistanceTo(edge.TargetPort.GetLocation());
        ratio = Math.Min(1, edgeLength / labelWidth);
      }
      return ratio;
    }
  }

  /// <summary>
  ///   Adapts the render context so the wrapped style renders the origin-centered dummy label as the
  ///   original label, while observing the zoom adjusted for the invariant scale. It composes the
  ///   inverse outer transform into each render transform, matching the source proxy context.
  /// </summary>
  internal sealed class DummyContext : IRenderContext
  {
    private readonly IRenderContext innerContext;

    public DummyContext(IRenderContext innerContext, double zoom, Transform inverseOuterTransform) {
      this.innerContext = innerContext;
      Zoom = zoom;
      ViewTransform = Append(innerContext.ViewTransform, inverseOuterTransform);
      IntermediateTransform = Append(innerContext.IntermediateTransform, inverseOuterTransform);
      Projection = Append(innerContext.Projection, inverseOuterTransform);
    }

    public CanvasControl CanvasControl => innerContext.CanvasControl;

    public RectD Clip => innerContext.Clip;

    public double HitTestRadius => innerContext.HitTestRadius;

    public double Zoom { get; }

    public Transform ViewTransform { get; }

    public Transform IntermediateTransform { get; }

    public Transform Projection { get; }

    public PointD WorldToViewCoordinates(PointD worldPoint) => Transform(ViewTransform, worldPoint);

    public PointD WorldToIntermediateCoordinates(PointD worldPoint) => Transform(IntermediateTransform, worldPoint);

    public PointD IntermediateToViewCoordinates(PointD intermediatePoint) => Transform(Projection, intermediatePoint);

    public object Lookup(Type type) => innerContext.Lookup(type)!;

    private static Transform Append(Transform transform, Transform appended) =>
        new TransformGroup {
            Children = {
                transform,
                appended
            }
        };

    private static PointD Transform(Transform transform, PointD point) {
      var transformed = transform.Transform(new Point(point.X, point.Y));
      return new PointD(transformed.X, transformed.Y);
    }
  }
}
