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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.GraphMLCompat.Styles
{
  public class ShinyPlateNodeStyle : DelegatingNodeStyle
  {
    private readonly ShinyPlateNodeStyleImpl impl = new();
    private INodeStyle wrappedStyle;
    private bool drawShadow = true;

    public ShinyPlateNodeStyle() {
      wrappedStyle = new ShadowNodeStyleDecorator(impl);
      Brush = Brushes.Black;
      Pen = null;
      Radius = 5;
      Insets = new(5);
    }

    public Brush Brush {
      get => impl.Brush;
      set => impl.Brush = value;
    }

    [DefaultValue(null)]
    public Pen Pen {
      get => impl.Pen;
      set => impl.Pen = value;
    }

    [DefaultValue(5d)]
    public double Radius {
      get => impl.Radius;
      set => impl.Radius = value;
    }

    [DefaultValue(true)]
    public bool DrawShadow {
      get => drawShadow;
      set {
        if (value != drawShadow) {
          drawShadow = value;
          wrappedStyle = value ? new ShadowNodeStyleDecorator(impl) : impl;
        }
      }
    }

    [DefaultValue(typeof(InsetsD), "5,5,5,5")]
    public InsetsD Insets {
      get => impl.Insets;
      set => impl.Insets = value;
    }

    public override object Clone() =>
        new ShinyPlateNodeStyle {
            DrawShadow = DrawShadow,
            Brush = Brush,
            Insets = Insets,
            Radius = Radius,
            Pen = Pen
        };

    protected override INodeStyle GetStyle(INode node) => wrappedStyle;
  }

  internal class ShinyPlateNodeStyleImpl : INodeStyle
  {
    private readonly ShinyPlateNodeStyleRenderer renderer = new();

    public Brush Brush { get; set; } = Brushes.Black;

    public Pen Pen { get; set; }

    public double Radius { get; set; } = 5;

    public InsetsD Insets { get; set; } = new(5);

    public INodeStyleRenderer Renderer => renderer;

    /// <inheritdoc/>
    public virtual object Clone() => MemberwiseClone();
  }

  internal class ShinyPlateNodeStyleRenderer : INodeStyleRenderer, IShapeGeometry, IBoundsProvider, IVisibilityTestable,
      IMarqueeTestable, IHitTestable, ILookup, IVisualCreator, ILassoTestable
  {
    private GeneralPath outline;

    public IRectangle Layout => Node.Layout;

    protected ShinyPlateNodeStyleImpl Style { get; set; }

    protected INode Node { get; set; }

    protected virtual Brush Brush => Style.Brush;

    protected virtual Pen Pen => Style.Pen;

    protected virtual double Radius => Style.Radius;

    public virtual IVisualCreator GetVisualCreator(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return VoidVisualCreator.Instance;
    }

    public virtual IBoundsProvider GetBoundsProvider(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return BoundsProviders.Empty;
    }

    public virtual IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return VoidShapeGeometry.Instance;
    }

    public virtual IHitTestable GetHitTestable(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return HitTestables.Never;
    }

    public virtual IMarqueeTestable GetMarqueeTestable(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return MarqueeTestables.Never;
    }

    public ILassoTestable GetLassoTestable(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return LassoTestables.Never;
    }

    public virtual IVisibilityTestable GetVisibilityTestable(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        return this;
      }
      return VisibilityTestables.Never;
    }

    public virtual ILookup GetContext(INode node, INodeStyle style) {
      var theStyle = style as ShinyPlateNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        return this;
      }
      return Lookups.Empty;
    }

    public virtual Visual CreateVisual(IRenderContext context) =>
        new ShinyPlateNodeStyleVisual().Render(Brush, Pen, Layout, Radius);

    public virtual Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      var visual = oldVisual as ShinyPlateNodeStyleVisual;
      if (visual != null) {
        return visual.Render(Brush, Pen, Layout, Radius);
      }
      return CreateVisual(context);
    }

    public virtual RectD GetBounds(ICanvasContext context) => Layout.ToRectD();

    public virtual GeneralPath GetOutline() {
      if (outline != null) {
        return outline;
      }
      var layout = Layout;
      return outline = StyleUtils.CreateRoundRectanglePath(layout.X, layout.Y, layout.Width, layout.Height, Radius);
    }

    public virtual PointD? GetIntersection(PointD inner, PointD outer) =>
        StyleUtils.FindLineIntersectionWithRoundRect(inner, outer, Layout, Radius);

    public virtual bool IsInside(PointD location) => StyleUtils.RoundRectContains(location, Layout, Radius);

    public virtual bool IsHit(IInputModeContext context, PointD location) =>
        StyleUtils.RoundRectIsHit(location, Layout, Radius, context.HitTestRadius);

    public virtual bool IsInBox(IInputModeContext context, RectD rectangle) => rectangle.Intersects(Layout.ToRectD());

    public bool IsInPath(IInputModeContext context, GeneralPath lassoPath) =>
        lassoPath.AreaIntersects(GetOutline()!, context.HitTestRadius);

    public virtual bool IsVisible(ICanvasContext context, RectD rectangle) => rectangle.Intersects(Layout.ToRectD());

    public virtual object Lookup(Type type) {
      if (type == typeof(IGroupPaddingProvider)) {
        return new MyGroupPaddingProvider(Style);
      }
      if (type.IsInstanceOfType(this)) {
        Configure();
        return this;
      }
      return null;
    }

    protected virtual void Configure() {
      outline = null;
    }

    private sealed class MyGroupPaddingProvider : IGroupPaddingProvider
    {
      private readonly ShinyPlateNodeStyleImpl style;

      internal MyGroupPaddingProvider(ShinyPlateNodeStyleImpl style) {
        this.style = style;
      }

      public InsetsD GetPadding() => style.Insets;
    }


    private sealed class ShinyPlateNodeStyleVisual : VisualGroup
    {
      private readonly MutableRectangle layout = new();
      private Brush brush;
      private Pen pen;
      private double radius;

      public ShinyPlateNodeStyleVisual() {
        FrameworkElement.SetFlowDirection(this, FlowDirection.LeftToRight);
      }

      internal ShinyPlateNodeStyleVisual Render(Brush brush, Pen pen, IRectangle rectangle, double radius) {
        var arrangeRect = rectangle.ToRectD();
        if (brush == this.brush && this.pen == pen && this.radius == radius) {
          if (layout.Width == rectangle.Width && layout.Height == rectangle.Height) {
            ((UIElement) Children[0])!.SetCanvasArrangeRect(arrangeRect.ToRect());
            ((UIElement) Children[1])!.SetCanvasArrangeRect(arrangeRect.ToRect());
            return this;
          }
        }

        // something changed.
        this.pen = pen;
        this.brush = brush;
        this.radius = radius;
        layout.SetShape(rectangle);
        Children.Clear();
        Paint();
        ((UIElement) Children[0])!.SetCanvasArrangeRect(arrangeRect.ToRect());
        ((UIElement) Children[1])!.SetCanvasArrangeRect(arrangeRect.ToRect());
        StyleUtils.AdjustStylusHoldEvents(this, false);
        return this;
      }

      private void Paint() {
        double layoutX = 0;
        double layoutY = 0;
        var layoutWidth = layout.Width;
        var layoutHeight = layout.Height;

        var rectangle = new Rectangle {
            AllowDrop = true,
            Width = layoutWidth,
            Height = layoutHeight
        };
        rectangle.RadiusX = rectangle.RadiusY = radius;
        rectangle.Fill = brush;
        rectangle.SetPen(pen);

        layoutWidth = Math.Min(layoutWidth, Math.Max(0.65 * layoutWidth, radius * 2));
        layoutHeight = Math.Min(layoutHeight, Math.Max(0.8 * layoutHeight, radius * 2));

        radius = Math.Min(radius, Math.Min(layoutWidth * 0.5, layoutHeight * 0.5));
        var r2 = Math.Min(radius * 0.75, Math.Min(layoutWidth * 0.4, layoutHeight * 0.4));

        var gp = new GeneralPath(12);
        gp.MoveTo(layoutX + radius - r2, layoutY + radius);
        gp.QuadTo(layoutX + radius - r2, layoutY + radius - r2, layoutX + radius, layoutY + radius - r2);
        gp.LineTo(layoutX + layoutWidth - radius, layoutY + radius - r2);
        gp.QuadTo(layoutX + layoutWidth - radius + r2, layoutY + radius - r2, layoutX + layoutWidth - radius + r2,
            layoutY + radius);
        gp.QuadTo(layoutX + layoutWidth - radius + r2, layoutY + radius + r2, layoutX + layoutWidth * 0.5 + r2,
            layoutY + layoutHeight * 0.5 + r2);
        gp.QuadTo(layoutX + radius + r2, layoutY + layoutHeight - radius + r2, layoutX + radius,
            layoutY + layoutHeight - radius + r2);
        gp.QuadTo(layoutX + radius - r2, layoutY + layoutHeight - radius + r2, layoutX + radius - r2,
            layoutY + layoutHeight - radius);

        var w = layoutWidth - r2 - r2;
        var h = layoutHeight - r2 - r2;
        var l2 = w * w + h * h;
        var alpha = Math.Atan(w * w / l2 / (h * h / l2)) * 360 / (2 * Math.PI);

        var gradientStops = new GradientStopCollection {
            new() {
                Color = Color.FromArgb(220, 255, 255, 255),
                Offset = 0
            },
            new() {
                Color = Color.FromArgb(20, 255, 255, 255),
                Offset = 0.8
            }
        };
        gradientStops.Freeze();
        var gradientBrush = new LinearGradientBrush(gradientStops, alpha) { SpreadMethod = GradientSpreadMethod.Pad };
        gradientBrush.Freeze();

        var path = gp.CreatePath(gradientBrush, null, null, FillMode.Always);
        path.AllowDrop = false;
        Add(rectangle);
        Add(path);
      }
    }
  }
}
