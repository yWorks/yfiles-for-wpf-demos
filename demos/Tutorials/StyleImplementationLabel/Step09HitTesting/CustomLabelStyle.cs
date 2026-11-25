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
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using LabelData = yWorks.Demo.Tutorial.StyleImplementationLabel.Step08ItemSpecificCustomization.LabelData;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step09HitTesting
{
  /// <summary>A custom label style that renders a speech balloon and customizes hit-testing.</summary>
  internal sealed class CustomLabelStyle : LabelStyleBase<CustomLabelStyle.CustomLabelVisual>
  {
    private static readonly Typeface Typeface = new("Arial");
    private static readonly Brush DefaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 254, 205));
    private static readonly Pen BorderPen = new(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1);
    private static readonly Dictionary<string, BitmapImage> ImageCache = new(StringComparer.Ordinal);
    private const double Padding = 3;
    private const double FontSize = 12;
    private const double IconSize = 16;
    private const double TailWidth = 10;
    private const double TailHeight = 20;

    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      var labelLayout = label.GetLayout();
      // First check whether the label layout is hit.
      if (labelLayout.Contains(location, context.HitTestRadius)) {
        return true;
      }

      // Convert the hit location to the label's unrotated coordinate system and subtract the tail position.
      var topLeftX = labelLayout.AnchorX + labelLayout.UpX * labelLayout.Height;
      var topLeftY = labelLayout.AnchorY + labelLayout.UpY * labelLayout.Height;
      var dx = location.X - topLeftX;
      var dy = location.Y - topLeftY;
      var x = -labelLayout.UpY * dx + labelLayout.UpX * dy - labelLayout.Width * 0.5;
      var y = -labelLayout.UpX * dx - labelLayout.UpY * dy - labelLayout.Height;

      // Check the rectangular tail bounds, then the triangular tail itself.
      var tailBounds = new RectD(0, 0, TailWidth, TailHeight);
      if (!tailBounds.Contains(new PointD(x, y), context.HitTestRadius)) {
        return false;
      }

      var tailHeightAtLocationX = TailHeight * ((TailWidth - x) / TailWidth);
      return y <= tailHeightAtLocationX + context.HitTestRadius;
    }

    protected override CustomLabelVisual CreateVisual(IRenderContext context, ILabel label) {
      var layout = label.GetLayout();
      var bounds = layout.GetBounds();
      var visual = new CustomLabelVisual(label.Text, layout.Width, layout.Height, bounds, label.Tag as LabelData);
      ArrangeByLayout(context, visual, layout, true);
      return visual;
    }

    protected override CustomLabelVisual UpdateVisual(
        IRenderContext context,
        CustomLabelVisual oldVisual,
        ILabel label) {
      var layout = label.GetLayout();
      var bounds = layout.GetBounds();
      var labelData = label.Tag as LabelData;
      var textChanged = label.Text != oldVisual.Text;
      var sizeChanged = layout.Width != oldVisual.Width || layout.Height != oldVisual.Height ||
          bounds.Width != oldVisual.Bounds.Width || bounds.Height != oldVisual.Bounds.Height;
      var locationChanged = bounds.X != oldVisual.Bounds.X || bounds.Y != oldVisual.Bounds.Y;
      var tagChanged = !Equals(labelData, oldVisual.LabelData);

      if (textChanged || sizeChanged || locationChanged || tagChanged) {
        oldVisual.Update(label.Text, layout.Width, layout.Height, bounds, labelData,
            textChanged || sizeChanged || tagChanged);
        ArrangeByLayout(context, oldVisual, layout, true);
      }
      return oldVisual;
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      var text = label.Text;
      var width = 0.0;
      var height = 0.0;

      if (text.Length > 0) {
        foreach (var line in text.Replace("\r\n", "\n").Split('\n')) {
          if (line.Length == 0) {
            continue;
          }
          var formattedText = CreateFormattedText(line, 1.0);
          width = Math.Max(width, formattedText.WidthIncludingTrailingWhitespace);
          height += formattedText.Height;
        }
      }

      if (label.Tag is LabelData { IconUrl: not null }) {
        width += IconSize + (width > 0 ? Padding : 0);
        height = Math.Max(height, IconSize);
      }

      return new SizeD(width + Padding * 2, height + Padding * 2);
    }

    private static FormattedText CreateFormattedText(string text, double pixelsPerDip) {
      return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize,
          Brushes.Black, pixelsPerDip);
    }

    private static BitmapImage GetImage(string iconUrl) {
      if (ImageCache.TryGetValue(iconUrl, out var image)) {
        return image;
      }
      var resourceSuffix = iconUrl.Replace('/', '.');
      var assembly = typeof(CustomLabelStyleTutorial).Assembly;
      image = TutorialStepBase.LoadImageResource(resourceSuffix, assembly);
      ImageCache.Add(iconUrl, image);
      return image;
    }

    /// <summary>The WPF element that draws and caches the label content.</summary>
    internal sealed class CustomLabelVisual : FrameworkElement
    {
      private BitmapImage? icon;

      public string Text { get; private set; }

      public RectD Bounds { get; private set; }

      public LabelData? LabelData { get; private set; }

      public CustomLabelVisual(string text, double width, double height, RectD bounds, LabelData? labelData) {
        this.Text = text;
        Width = width;
        Height = height;
        Bounds = bounds;
        UpdateLabelData(labelData);
      }

      public void Update(string text, double width, double height, RectD bounds, LabelData? labelData, bool redraw) {
        this.Text = text;
        Width = width;
        Height = height;
        Bounds = bounds;
        UpdateLabelData(labelData);
        if (redraw) {
          InvalidateVisual();
        }
      }

      protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);
        var backgroundBrush = DefaultBackgroundBrush;
        if (LabelData?.BackgroundColor is { } backgroundColor) {
          backgroundBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(backgroundColor)!);
        }
        drawingContext.DrawGeometry(backgroundBrush, BorderPen, CreateBackgroundShape(Width, Height));

        if (icon != null) {
          drawingContext.DrawImage(icon, new Rect(Width - IconSize - Padding, Padding, IconSize, IconSize));
        }

        var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var lines = Text.Replace("\r\n", "\n").Split('\n');
        var totalHeight = 0.0;
        foreach (var line in lines) {
          if (line.Length > 0) {
            totalHeight += CreateFormattedText(line, pixelsPerDip).Height;
          }
        }

        var y = (Height - totalHeight) * 0.5;
        foreach (var line in lines) {
          if (line.Length == 0) {
            continue;
          }
          var formattedText = CreateFormattedText(line, pixelsPerDip);
          drawingContext.DrawText(formattedText, new Point(Padding, y));
          y += formattedText.Height;
        }
      }

      private void UpdateLabelData(LabelData? data) {
        LabelData = data;
        icon = data?.IconUrl is { } iconUrl ? GetImage(iconUrl) : null;
      }

      public static StreamGeometry CreateBackgroundShape(double width, double height) {
        var path = new StreamGeometry();
        using (var pathContext = path.Open()) {
          pathContext.BeginFigure(new Point(0, 0), true, true);
          pathContext.LineTo(new Point(width, 0), true, false);
          pathContext.LineTo(new Point(width, height), true, false);
          pathContext.LineTo(new Point(width * 0.5 + TailWidth, height), true, false);
          pathContext.LineTo(new Point(width * 0.5, height + TailHeight), true, false);
          pathContext.LineTo(new Point(width * 0.5, height), true, false);
          pathContext.LineTo(new Point(0, height), true, false);
        }
        path.Freeze();
        return path;
      }
    }
  }
}
