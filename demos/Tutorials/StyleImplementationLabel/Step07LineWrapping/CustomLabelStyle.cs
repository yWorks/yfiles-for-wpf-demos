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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step07LineWrapping
{
  /// <summary>A custom label style that supports automatic line wrapping.</summary>
  internal sealed class CustomLabelStyle : LabelStyleBase<CustomLabelStyle.CustomLabelVisual>
  {
    private static readonly Typeface Typeface = new("Arial");
    private static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 254, 205));
    private static readonly Pen BorderPen = new(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1);
    private const double Padding = 3;
    private const double FontSize = 12;

    private readonly bool wrapping;
    private readonly SizeD maxSize;

    public CustomLabelStyle(bool wrapping, SizeD maxSize) {
      this.wrapping = wrapping;
      this.maxSize = maxSize;
    }

    public CustomLabelStyle() : this(false, SizeD.Infinite) { }

    protected override CustomLabelVisual CreateVisual(IRenderContext context, ILabel label) {
      var layout = label.GetLayout();
      var bounds = layout.GetBounds();
      var visual = new CustomLabelVisual(label.Text, layout.Width, layout.Height, bounds, wrapping, maxSize);
      ArrangeByLayout(context, visual, layout, true);
      return visual;
    }

    protected override CustomLabelVisual UpdateVisual(
        IRenderContext context,
        CustomLabelVisual oldVisual,
        ILabel label) {
      var layout = label.GetLayout();
      var bounds = layout.GetBounds();
      var textChanged = label.Text != oldVisual.Text;
      var sizeChanged = layout.Width != oldVisual.Width || layout.Height != oldVisual.Height ||
          bounds.Width != oldVisual.Bounds.Width || bounds.Height != oldVisual.Bounds.Height;
      var locationChanged = bounds.X != oldVisual.Bounds.X || bounds.Y != oldVisual.Bounds.Y;
      var wrappingChanged = wrapping != oldVisual.Wrapping || !maxSize.Equals(oldVisual.MaxSize);

      if (textChanged || sizeChanged || locationChanged || wrappingChanged) {
        oldVisual.Update(label.Text, layout.Width, layout.Height, bounds, wrapping, maxSize,
            textChanged || sizeChanged || wrappingChanged);
        ArrangeByLayout(context, oldVisual, layout, true);
      }
      return oldVisual;
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      var text = label.Text;
      if (text.Length == 0) {
        return new SizeD(Padding * 2, Padding * 2);
      }

      var width = 0.0;
      var height = 0.0;
      foreach (var line in text.Replace("\r\n", "\n").Split('\n')) {
        if (line.Length == 0) {
          continue;
        }
        var formattedText = CreateFormattedText(line, 1.0, wrapping);
        width = Math.Max(width, formattedText.WidthIncludingTrailingWhitespace);
        height += formattedText.Height;
      }

      return new SizeD(width + Padding * 2, height + Padding * 2);
    }

    private FormattedText CreateFormattedText(string text, double pixelsPerDip, bool enableWrapping) {
      var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface,
          FontSize, Brushes.Black, pixelsPerDip);
      if (enableWrapping && maxSize.Width > 0 && !double.IsInfinity(maxSize.Width)) {
        formattedText.MaxTextWidth = maxSize.Width;
      }
      return formattedText;
    }

    /// <summary>The WPF element that draws and caches the label content.</summary>
    internal sealed class CustomLabelVisual : FrameworkElement
    {
      public string Text { get; private set; }

      public RectD Bounds { get; private set; }

      public bool Wrapping { get; private set; }

      public SizeD MaxSize { get; private set; }

      public CustomLabelVisual(string text, double width, double height, RectD bounds, bool wrapping, SizeD maxSize) {
        this.Text = text;
        Width = width;
        Height = height;
        Bounds = bounds;
        Wrapping = wrapping;
        MaxSize = maxSize;
      }

      public void Update(
          string text,
          double width,
          double height,
          RectD bounds,
          bool wrapping,
          SizeD maxSize,
          bool redraw) {
        this.Text = text;
        Width = width;
        Height = height;
        Bounds = bounds;
        Wrapping = wrapping;
        MaxSize = maxSize;
        if (redraw) {
          InvalidateVisual();
        }
      }

      protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);
        drawingContext.DrawGeometry(BackgroundBrush, BorderPen, CreateBackgroundShape(Width, Height));

        var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var y = Padding;
        foreach (var line in Text.Replace("\r\n", "\n").Split('\n')) {
          if (line.Length == 0) {
            continue;
          }
          var formattedText = CreateFormattedText(line, pixelsPerDip, Wrapping);
          drawingContext.DrawText(formattedText, new Point(Padding, y));
          y += formattedText.Height;
        }
      }

      private FormattedText CreateFormattedText(string text, double pixelsPerDip, bool enableWrapping) {
        var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface,
            FontSize, Brushes.Black, pixelsPerDip);
        if (enableWrapping && MaxSize.Width > 0 && !double.IsInfinity(MaxSize.Width)) {
          formattedText.MaxTextWidth = MaxSize.Width;
        }
        return formattedText;
      }

      private static StreamGeometry CreateBackgroundShape(double width, double height) {
        var path = new StreamGeometry();
        using (var pathContext = path.Open()) {
          pathContext.BeginFigure(new Point(0, 0), true, true);
          pathContext.LineTo(new Point(width, 0), true, false);
          pathContext.LineTo(new Point(width, height), true, false);
          pathContext.LineTo(new Point(width * 0.5 + 5, height), true, false);
          pathContext.LineTo(new Point(width * 0.5, height + 5), true, false);
          pathContext.LineTo(new Point(width * 0.5 - 5, height), true, false);
          pathContext.LineTo(new Point(0, height), true, false);
        }
        path.Freeze();
        return path;
      }
    }
  }
}
