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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step09Visibility
{
  /// <summary>
  ///   A card-style node whose visibility includes the large circle rendered outside its layout.
  /// </summary>
  public sealed class CustomNodeStyle : NodeStyleBase<CustomNodeStyle.CustomNodeVisual>
  {
    protected override CustomNodeVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new CustomNodeVisual();
      UpdateCustomNodeVisual(node, visual);
      return visual;
    }

    protected override CustomNodeVisual UpdateVisual(IRenderContext context, CustomNodeVisual oldVisual, INode node) {
      UpdateCustomNodeVisual(node, oldVisual);
      return oldVisual;
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) {
      // consider the circle, which is twice the size of the node
      var layout = node.Layout;
      var circleDiameter = Math.Max(layout.Width, layout.Height) * 2;
      var circleBounds = RectD.FromCenter(layout.GetCenter(), new SizeD(circleDiameter, circleDiameter));
      return rectangle.Intersects(circleBounds);
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      var layout = node.Layout;
      var hitTestRadius = context.HitTestRadius;

      // Check for the bounding box.
      if (!layout.Contains(location, hitTestRadius)) {
        return false;
      }

      // Check for the upper-right corner, which is empty.
      if (location.X > layout.X + CustomNodeVisual.TabWidth + hitTestRadius &&
          location.Y < layout.Y + CustomNodeVisual.TabHeight - hitTestRadius) {
        return false;
      }

      // All other points are either inside the tab or the rest of the node.
      return true;
    }

    protected override GeneralPath GetOutline(INode node) {
      var layout = node.Layout;
      var topLeft = new PointD(layout.X - 0.5, layout.Y - 0.5);
      var width = layout.Width + 1;
      var height = layout.Height + 1;

      var path = new GeneralPath();
      path.MoveTo(topLeft);
      path.LineTo(topLeft + new PointD(CustomNodeVisual.TabWidth, 0));
      path.LineTo(topLeft + new PointD(CustomNodeVisual.TabWidth, CustomNodeVisual.TabHeight));
      path.LineTo(topLeft + new PointD(width, CustomNodeVisual.TabHeight));
      path.LineTo(topLeft + new PointD(width, height));
      path.LineTo(topLeft + new PointD(0, height));
      path.Close();
      return path;
    }

    protected override bool IsInside(INode node, PointD location) {
      var layout = node.Layout;
      if (!layout.Contains(location)) {
        return false;
      }

      if (location.X > layout.X + CustomNodeVisual.TabWidth && location.Y < layout.Y + CustomNodeVisual.TabHeight) {
        return false;
      }

      return true;
    }

    private static void UpdateCustomNodeVisual(INode node, CustomNodeVisual visual) {
      var layout = node.Layout;
      var pathChanged = visual.UpdatePath(layout.Width, layout.Height);
      visual.UpdateLocation(layout.X, layout.Y);

      var color = Color.FromRgb(11, 113, 137);
      var showBadge = false;
      var title = string.Empty;
      if (node.Tag is NodeData nodeData) {
        if (nodeData.Color != null) {
          color = (Color) ColorConverter.ConvertFromString(nodeData.Color)!;
        }
        showBadge = nodeData.ShowBadge;
        if (nodeData.Title != null) {
          title = nodeData.Title;
        }
      }

      var fillChanged = visual.UpdateFillColor(color);
      var badgeChanged = visual.UpdateBadge(showBadge);
      var titleChanged = visual.UpdateTitle(title);
      if (pathChanged || fillChanged || badgeChanged || titleChanged) {
        visual.Render();
      }
    }

    /// <summary>The visual that renders and caches the circle, card, optional badge, and title.</summary>
    public sealed class CustomNodeVisual : DrawingVisual
    {
      public const double TabWidth = 50;
      public const double TabHeight = 14;

      private static readonly Pen Border = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(51, 51, 51));

      private static readonly Brush VisibilityBrush =
          CustomNodeStyleTutorial.CreateBrush(Color.FromArgb(76, 11, 113, 137));

      private static readonly Brush BadgeFill = CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(108, 159, 68));
      private static readonly Pen BadgeBorder = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(73, 108, 46));
      private static readonly EllipseGeometry BadgeGeometry = CreateBadgeGeometry();
      private static readonly Typeface TitleTypeface = new("Segoe UI");
      private static readonly Brush TitleBrush = CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(238, 238, 238));

      private double width;
      private double height;
      private StreamGeometry? path;
      private EllipseGeometry? visibilityCircle;
      private Color? fillColor;
      private Brush? fillBrush;
      private bool showBadge;
      private string title = string.Empty;
      private readonly TranslateTransform translation = new();

      public CustomNodeVisual() {
        Transform = translation;
      }

      private static EllipseGeometry CreateBadgeGeometry() {
        var geometry = new EllipseGeometry(new Point(0, 0), 8, 8);
        geometry.Freeze();
        return geometry;
      }

      public static StreamGeometry CreatePath(double width, double height) {
        var path = new StreamGeometry();
        using (var pathContext = path.Open()) {
          pathContext.BeginFigure(new Point(0, 0), true, true);
          pathContext.LineTo(new Point(TabWidth, 0), true, false);
          pathContext.LineTo(new Point(TabWidth, TabHeight), true, false);
          pathContext.LineTo(new Point(width, TabHeight), true, false);
          pathContext.LineTo(new Point(width, height), true, false);
          pathContext.LineTo(new Point(0, height), true, false);
        }
        path.Freeze();
        return path;
      }

      public bool UpdatePath(double width, double height) {
        if (path == null || this.width != width || this.height != height) {
          this.width = width;
          this.height = height;
          path = CreatePath(width, height);

          var circleDiameter = Math.Max(width, height) * 2;
          visibilityCircle = new EllipseGeometry(new Point(width * 0.5, height * 0.5), circleDiameter * 0.5,
              circleDiameter * 0.5);
          visibilityCircle.Freeze();
          return true;
        }
        return false;
      }

      public void UpdateLocation(double x, double y) {
        if (translation.X != x) {
          translation.X = x;
        }
        if (translation.Y != y) {
          translation.Y = y;
        }
      }

      public bool UpdateFillColor(Color color) {
        if (fillColor != color) {
          fillColor = color;
          fillBrush = CustomNodeStyleTutorial.CreateBrush(color);
          return true;
        }
        return false;
      }

      public bool UpdateBadge(bool showBadge) {
        if (this.showBadge != showBadge) {
          this.showBadge = showBadge;
          return true;
        }
        return false;
      }

      public bool UpdateTitle(string title) {
        if (this.title != title) {
          this.title = title;
          return true;
        }
        return false;
      }

      public void Render() {
        using var drawingContext = RenderOpen();
        drawingContext.DrawGeometry(VisibilityBrush, null, visibilityCircle);
        drawingContext.DrawGeometry(fillBrush, Border, path!);
        if (showBadge) {
          drawingContext.DrawGeometry(BadgeFill, BadgeBorder, BadgeGeometry);
        }
        RenderTitle(drawingContext);
      }

      private void RenderTitle(DrawingContext drawingContext) {
        if (string.IsNullOrEmpty(title)) {
          return;
        }

        var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var formattedText =
            new FormattedText(title, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, TitleTypeface, 10,
                TitleBrush, pixelsPerDip) {
                MaxTextWidth = TabWidth - 12, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis
            };

        var textY = (TabHeight - formattedText.Height) / 2;
        drawingContext.DrawText(formattedText, new Point(10, textY));
      }
    }
  }
}
