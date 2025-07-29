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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Demo.yFiles.IO.GraphML.Compat.Styles
{
  public class PanelNodeStyle : INodeStyle
  {
    private readonly PanelNodeStyleRenderer renderer = new();

    [DefaultValue(typeof(Color), "Black")]
    public Color Color { get; set; } = Colors.Black;

    [DefaultValue(typeof(Color), "LightGray")]
    public Color LabelInsetsColor { get; set; } = Colors.LightGray;

    [DefaultValue(typeof(InsetsD), "5, 5, 5, 5")]
    public InsetsD Insets { get; set; } = new(5);

    public INodeStyleRenderer Renderer => renderer;

    public virtual object Clone() {
      return MemberwiseClone();
    }
  }

  internal class PanelNodeStyleRenderer : INodeStyleRenderer, IShapeGeometry, IBoundsProvider, IVisibilityTestable,
      IMarqueeTestable, IHitTestable, ILookup, IVisualCreator, ILassoTestable
  {
    public IRectangle Layout => Node.Layout;

    protected PanelNodeStyle Style { get; set; }

    protected INode Node { get; set; }

    protected virtual Color Color => Style.Color;

    public virtual IVisualCreator GetVisualCreator(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return VoidVisualCreator.Instance;
    }

    public virtual IBoundsProvider GetBoundsProvider(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return BoundsProviders.Empty;
    }

    public virtual IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return VoidShapeGeometry.Instance;
    }

    public virtual IHitTestable GetHitTestable(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return HitTestables.Never;
    }

    public virtual IMarqueeTestable GetMarqueeTestable(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return MarqueeTestables.Never;
    }

    public ILassoTestable GetLassoTestable(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        Configure();
        return this;
      }
      return LassoTestables.Never;
    }

    public virtual IVisibilityTestable GetVisibilityTestable(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        return this;
      }
      return VisibilityTestables.Never;
    }

    public virtual ILookup GetContext(INode node, INodeStyle style) {
      var theStyle = style as PanelNodeStyle;
      if (theStyle != null) {
        Style = theStyle;
        Node = node;
        return this;
      }
      return Lookups.Empty;
    }

    public virtual Visual CreateVisual(IRenderContext context) {
      if (Layout.Width <= 0 || Layout.Height <= 0) {
        return null;
      }
      return new PanelNodeStyleVisual().Render(Color, Style.LabelInsetsColor, Layout, CalculatePadding());
    }

    public virtual Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      if (Layout.Width <= 0 || Layout.Height <= 0) {
        return null;
      }
      var visual = oldVisual as PanelNodeStyleVisual;
      if (visual != null) {
        return visual.Render(Color, Style.LabelInsetsColor, Layout, CalculatePadding());
      }
      return CreateVisual(context);
    }

    private InsetsD CalculatePadding() {
      var padding = InsetsD.Empty;
      if (Node.Labels.Count > 0 && Style.LabelInsetsColor != Colors.Transparent) {
        foreach (var label in Node.Labels) {
          var provider = label.LayoutParameter.Model.GetContext(label).Lookup<IGroupPaddingProvider>();
          if (provider != null) {
            padding = padding.CreateUnion(provider.GetPadding());
          }
        }
      }
      return padding;
    }

    public virtual RectD GetBounds(ICanvasContext context) {
      return Layout.ToRectD();
    }

    public virtual GeneralPath GetOutline() {
      var path = new GeneralPath(4);
      path.AppendRectangle(Node.Layout, false);
      return path;
    }

    public virtual PointD? GetIntersection(PointD inner, PointD outer) {
      var l = Layout.ToRectD();
      if (!l.Contains(inner)) {
        return inner;
      }
      if (l.Contains(outer)) {
        return null;
      }

      return l.FindLineIntersection(inner, outer);
    }

    public virtual bool IsInside(PointD location) {
      return Layout.ToRectD().Contains(location);
    }

    public virtual bool IsHit(IInputModeContext context, PointD location) {
      var layout = Layout;
      var x = layout.X;
      var y = layout.Y;
      var w = layout.Width;
      var h = layout.Height;
      var px = location.X;
      var py = location.Y;
      var epsilon = context.HitTestRadius;

      if (w < 0 || h < 0) {
        return false;
      }
      if (px < x - epsilon || py < y - epsilon) {
        return false;
      }
      if ((!double.IsPositiveInfinity(w) && w + 2 * epsilon < px - (x - epsilon)) ||
          (!double.IsPositiveInfinity(h) && h + 2 * epsilon < py - (y - epsilon))) {
        return false;
      }
      return true;
    }

    public virtual bool IsInBox(IInputModeContext context, RectD rectangle) {
      return rectangle.Intersects(Layout.ToRectD());
    }

    public bool IsInPath(IInputModeContext context, GeneralPath lassoPath) {
      return lassoPath.AreaIntersects(GetOutline()!, context.HitTestRadius);
    }

    public virtual bool IsVisible(ICanvasContext context, RectD rectangle) {
      return rectangle.Intersects(Layout.ToRectD());
    }

    /// <inheritdoc/>
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

    protected virtual void Configure() { }

    private sealed class MyGroupPaddingProvider : IGroupPaddingProvider
    {
      private readonly PanelNodeStyle style;

      internal MyGroupPaddingProvider(PanelNodeStyle style) {
        this.style = style;
      }

      public InsetsD GetPadding() {
        return style.Insets;
      }
    }

    internal sealed class PanelNodeStyleVisual : VisualGroup
    {
      private Color color;
      private RectD layout;
      private InsetsD padding;
      private Color labelColor;

      public PanelNodeStyleVisual() {
        FrameworkElement.SetFlowDirection(this, FlowDirection.LeftToRight);
      }

      internal PanelNodeStyleVisual Render(Color color, Color labelColor, IRectangle rectangle, InsetsD labelPadding) {
        if (labelColor == Colors.Transparent) {
          labelPadding = InsetsD.Empty;
        }
        if (color == this.color && labelColor == this.labelColor) {
          if (layout.Width == rectangle.Width && layout.Height == rectangle.Height) {
            if (padding == labelPadding) {
              this.SetCanvasArrangeRect(rectangle.ToRect());
              return this;
            }
          }
        }

        // something changed.
        this.color = color;
        this.labelColor = labelColor;
        layout = rectangle.ToRectD();
        padding = labelPadding;
        Children.Clear();

        Paint();
        StyleUtils.AdjustStylusHoldEvents(this, false);
        return this;
      }

      private void Paint() {
        var gradient = CreateGradient(StyleUtils.MixColors(Colors.White, color, 0.75f), color, new Point(0, 0),
            new Point(0, 1));

        var lineGradient = CreateGradient(StyleUtils.MixColors(color, Colors.Black, 0.8f), color, new Point(0, 0),
            new Point(1, 1));

        var pen = new Pen(lineGradient, 1);
        pen.Freeze();

        var rectangle =
            new Rectangle { AllowDrop = true, Width = layout.Width, Height = layout.Height, Fill = gradient };

        const double layoutX = 0.0;
        const double layoutY = 0.0;
        rectangle.SetCanvasArrangeRect(new Rect(layoutX, layoutY, layout.Width, layout.Height));

        var gp = new GeneralPath(4);
        gp.MoveTo(layoutX, layoutY);
        gp.LineTo(layoutX + layout.Width, layoutY);
        gp.LineTo(layoutX + layout.Width, layoutY + layout.Height);
        gp.LineTo(layoutX, layoutY + layout.Height);
        gp.Close();

        var path = gp.CreatePath(null, pen, null, FillMode.Never);

        Add(rectangle);

        if (!padding.IsEmpty && labelColor != Colors.Transparent) {
          var top = layoutY;
          var bottom = layoutY + layout.Height;
          var backgroundBrush = new SolidColorBrush(labelColor);
          backgroundBrush.Freeze();

          if (padding.Top > 0) {
            top = layoutY + padding.Top;
            AddRectangle(backgroundBrush, layoutX, layoutY, layout.Width, padding.Top);
          }
          if (padding.Bottom > 0) {
            bottom = layoutY + layout.Height - padding.Bottom;
            AddRectangle(backgroundBrush, layoutX, bottom, layout.Width, padding.Bottom);
          }
          if (padding.Left > 0 && bottom - top > 0) {
            AddRectangle(backgroundBrush, layoutX, top, padding.Left, bottom - top);
          }
          if (padding.Right > 0 && bottom - top > 0) {
            AddRectangle(backgroundBrush, layoutX + layout.Width - padding.Right, top, padding.Right, bottom - top);
          }

          if (padding.Top > 0) {
            AddLine(pen, layoutX, top, layoutX + layout.Width, top);
          }
          if (padding.Bottom > 0) {
            AddLine(pen, layoutX, bottom, layoutX + layout.Width, bottom);
          }
          if (padding.Left > 0) {
            AddLine(pen, layoutX + padding.Left, top, layoutX + padding.Left, bottom);
          }
        }

        Add(path);
        this.SetCanvasArrangeRect(layout.ToRect());
      }

      private void AddLine(Pen pen, double x1, double y1, double x2, double y2) {
        var line = new GeneralPath(2);
        line.MoveTo(x1, y1);
        line.LineTo(x2, y2);
        Add(line.CreatePath(null, pen, null, FillMode.Never));
      }

      private void AddRectangle(Brush brush, double x, double y, double w, double h) {
        var rectangle = new Rectangle { AllowDrop = true, Width = w, Height = h, Fill = brush };
        rectangle.SetCanvasArrangeRect(new Rect(x, y, w, h));
        Add(rectangle);
      }

      private static LinearGradientBrush CreateGradient(Color color1, Color color2, Point point1, Point point2) {
        var stop1 = new GradientStop(color1, 0);
        stop1.Freeze();
        var stop2 = new GradientStop(color2, 1);
        stop2.Freeze();
        var gradientStops = new GradientStopCollection(2) { stop1, stop2 };
        gradientStops.Freeze();
        var brush = new LinearGradientBrush(gradientStops, point1, point2);
        brush.Freeze();
        return brush;
      }
    }
  }
}
