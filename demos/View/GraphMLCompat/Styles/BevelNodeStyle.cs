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
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace yWorks.Demo.GraphMLCompat.Styles
{
  public class BevelNodeStyle : DelegatingNodeStyle
  {
    private readonly BevelNodeStyleImpl impl = new();
    private INodeStyle wrappedStyle;
    private bool drawShadow;

    public BevelNodeStyle() {
      wrappedStyle = impl;
      Color = Colors.Black;
      Radius = 10;
      Inset = 3;
    }

    public double Inset {
      get => impl.Inset;
      set => impl.Inset = value;
    }

    public double Radius {
      get => impl.Radius;
      set => impl.Radius = value;
    }

    [DefaultValue(typeof(Color), "Black")]
    public Color Color {
      get => impl.Color;
      set => impl.Color = value;
    }

    [DefaultValue(false)]
    public bool DrawShadow {
      get => drawShadow;
      set {
        if (value != drawShadow) {
          drawShadow = value;
          wrappedStyle = value ? new ShadowNodeStyleDecorator(impl) : impl;
        }
      }
    }

    public override object Clone() =>
        new BevelNodeStyle {
            Inset = Inset,
            Radius = Radius,
            Color = Color,
            DrawShadow = DrawShadow
        };

    protected override INodeStyle GetStyle(INode node) => wrappedStyle;
  }

  internal class BevelNodeStyleImpl : INodeStyle
  {
    private readonly BevelNodeStyleRenderer renderer = new();

    public double Inset { get; set; } = 3;

    public double Radius { get; set; } = 10;

    public Color Color { get; set; } = Colors.Black;

    public INodeStyleRenderer Renderer => renderer;

    public virtual object Clone() => MemberwiseClone();
  }

  internal class BevelNodeStyleRenderer : INodeStyleRenderer, IShapeGeometry, IBoundsProvider, IVisibilityTestable,
      IMarqueeTestable, IHitTestable, ILookup, IVisualCreator, ILassoTestable
  {
    private GeneralPath outline;

    public IRectangle Layout => Node.Layout;

    protected BevelNodeStyleImpl Style { get; set; }

    protected INode Node { get; set; }

    protected virtual Color Color => Style.Color;

    protected virtual double Inset => Style.Inset;

    protected virtual double Radius => Style.Radius;

    public virtual IVisualCreator GetVisualCreator(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return VoidVisualCreator.Instance;
    }

    public virtual IBoundsProvider GetBoundsProvider(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return BoundsProviders.Empty;
    }

    public virtual IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return VoidShapeGeometry.Instance;
    }

    public virtual IHitTestable GetHitTestable(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return HitTestables.Never;
    }

    public virtual IMarqueeTestable GetMarqueeTestable(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return MarqueeTestables.Never;
    }

    public ILassoTestable GetLassoTestable(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return LassoTestables.Never;
    }

    public virtual IVisibilityTestable GetVisibilityTestable(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        return this;
      }
      return VisibilityTestables.Never;
    }

    public virtual ILookup GetContext(INode node, INodeStyle style) {
      var theStyle = style as BevelNodeStyleImpl;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        return this;
      }
      return Lookups.Empty;
    }

    public virtual Visual CreateVisual(IRenderContext context) =>
        new BevelNodeStyleVisual().Render(Color, Inset, Layout, Radius);

    public virtual Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      var visual = oldVisual as BevelNodeStyleVisual;
      return visual != null ? visual.Render(Color, Inset, Layout, Radius) : CreateVisual(context);
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
      if (type == typeof(INodeSizeConstraintProvider)) {
        return new MySizeProvider(Style);
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

    private sealed class BevelNodeStyleVisual : VisualGroup
    {
      private readonly MutableRectangle layout = new();
      private Color color;
      private double radius;
      private double inset;

      public BevelNodeStyleVisual() {
        FrameworkElement.SetFlowDirection(this, FlowDirection.LeftToRight);
      }

      internal BevelNodeStyleVisual Render(Color color, double inset, IRectangle rectangle, double radius) {
        if (color == this.color && this.radius == radius && this.inset == inset) {
          if (layout.Width == rectangle.Width && layout.Height == rectangle.Height) {
            this.SetCanvasArrangeRect(rectangle.ToRect());
            return this;
          }
        }

        // something changed.
        this.color = color;
        this.radius = radius;
        this.inset = inset;
        layout.SetShape(rectangle);
        Children.Clear();
        Paint();
        StyleUtils.AdjustStylusHoldEvents(this, false);
        this.SetCanvasArrangeRect(rectangle.ToRect());
        return this;
      }

      private void Paint() {
        double layoutX = 0;
        double layoutY = 0;
        var layoutWidth = layout.Width;
        var layoutHeight = layout.Height;

        {
          var gradientStops = new GradientStopCollection {
              new() {
                  Color = StyleUtils.MixColors(Colors.White, color, 0.75f),
                  Offset = 0
              },
              new() {
                  Color = StyleUtils.MixColors(Colors.White, color, 0.2f),
                  Offset = 0.5
              },
              new() {
                  Color = color,
                  Offset = 0.5
              },
              new() {
                  Color = color,
                  Offset = 1
              }
          };
          var brush = new LinearGradientBrush {
              GradientStops = gradientStops,
              StartPoint = new Point(0, 0),
              EndPoint = new Point(0, 1)
          };
          brush.Freeze();
          var visual = new Rectangle {
              Width = layoutWidth,
              Height = layoutHeight,
              RadiusX = radius,
              RadiusY = radius,
              Fill = brush,
              AllowDrop = true
          };
          visual.SetCanvasArrangeRect(new Rect(layoutX, layoutY, layoutWidth, layoutHeight));
          Add(visual);
        }

        {
          var margin = inset - 1;
          var brush = new SolidColorBrush(StyleUtils.MixColors(color, Colors.White, 0.15f));
          brush.Freeze();
          var visual = new Rectangle {
              Width = Math.Max(0, layoutWidth - 2 * margin),
              Height = Math.Max(0, layoutHeight - 2 * margin),
              RadiusX = Math.Max(0, radius - inset),
              RadiusY = Math.Max(0, radius - inset),
              Stroke = brush,
              StrokeThickness = 2,
              AllowDrop = true
          };
          visual.SetCanvasArrangeRect(new Rect(layoutX + margin, layoutY + margin, visual.Width, visual.Height));
          Add(visual);
        }

        {
          var margin = inset - 0.5;
          var brush = new SolidColorBrush(color);
          brush.Freeze();
          var visual = new Rectangle {
              Width = Math.Max(0, layoutWidth - 2 * margin),
              Height = Math.Max(0, layoutHeight - 2 * margin),
              RadiusX = Math.Max(0, radius - inset),
              RadiusY = Math.Max(0, radius - inset),
              Stroke = brush,
              StrokeThickness = 1,
              AllowDrop = true
          };
          visual.SetCanvasArrangeRect(new Rect(layoutX + margin, layoutY + margin, visual.Width, visual.Height));
          Add(visual);
        }
      }
    }

    private sealed class MySizeProvider : INodeSizeConstraintProvider
    {
      private readonly BevelNodeStyleImpl style;

      internal MySizeProvider(BevelNodeStyleImpl style) {
        this.style = style;
      }

      public SizeD GetMinimumSize() {
        var value = style.Radius * 2 + 2;
        return new SizeD(value, value);
      }

      public SizeD GetMaximumSize() => SizeD.Infinite;

      public RectD GetMinimumEnclosedArea() => RectD.Empty;
    }

    private sealed class MyGroupPaddingProvider : IGroupPaddingProvider
    {
      private readonly BevelNodeStyleImpl style;

      internal MyGroupPaddingProvider(BevelNodeStyleImpl style) {
        this.style = style;
      }

      public InsetsD GetPadding() => new(style.Radius + 1);
    }
  }
}
