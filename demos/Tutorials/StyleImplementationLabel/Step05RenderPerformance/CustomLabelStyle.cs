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

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step05RenderPerformance
{
  /// <summary>A custom label style that reuses its visual when the label has not changed.</summary>
  internal sealed class CustomLabelStyle : LabelStyleBase<CustomLabelStyle.CustomLabelVisual>
  {
    private static readonly Typeface Typeface = new("Arial");
    private static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 254, 205));
    private static readonly Pen BorderPen = new(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1);
    private const double Padding = 3;
    private const double FontSize = 12;

    protected override CustomLabelVisual CreateVisual(IRenderContext context, ILabel label) {
      var layout = label.GetLayout();
      var visual = new CustomLabelVisual(label.Text, layout.Width, layout.Height, layout.GetBounds());
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

      // Update the visual if the text, location, or size has changed.
      if (textChanged || sizeChanged || locationChanged) {
        oldVisual.Update(label.Text, layout.Width, layout.Height, bounds, textChanged || sizeChanged);
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
      var lines = text.Replace("\r\n", "\n").Split('\n');
      foreach (var line in lines) {
        // Match StringTokenizer in the Java tutorial, which skips empty lines.
        if (line.Length == 0) {
          continue;
        }
        var formattedText = CreateFormattedText(line, 1.0);
        width = Math.Max(width, formattedText.WidthIncludingTrailingWhitespace);
        height += formattedText.Height;
      }

      return new SizeD(width + Padding * 2, height + Padding * 2);
    }

    private static FormattedText CreateFormattedText(string text, double pixelsPerDip) {
      return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize,
          Brushes.Black, pixelsPerDip);
    }

    /// <summary>The WPF element that draws and caches the label content.</summary>
    internal sealed class CustomLabelVisual : FrameworkElement
    {
      public string Text { get; private set; }

      public RectD Bounds { get; private set; }

      public CustomLabelVisual(string text, double width, double height, RectD bounds) {
        this.Text = text;
        Width = width;
        Height = height;
        Bounds = bounds;
      }

      public void Update(string text, double width, double height, RectD bounds, bool redraw) {
        this.Text = text;
        Width = width;
        Height = height;
        Bounds = bounds;
        if (redraw) {
          InvalidateVisual();
        }
      }

      protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);
        drawingContext.DrawGeometry(BackgroundBrush, BorderPen, CreateBackgroundShape(Width, Height));

        var y = Padding;
        var lines = Text.Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines) {
          if (line.Length == 0) {
            continue;
          }
          var formattedText = CreateFormattedText(line, VisualTreeHelper.GetDpi(this).PixelsPerDip);
          drawingContext.DrawText(formattedText, new Point(Padding, y));
          y += formattedText.Height;
        }
      }
    }

    /// <summary>Creates a simple speech balloon shape.</summary>
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
