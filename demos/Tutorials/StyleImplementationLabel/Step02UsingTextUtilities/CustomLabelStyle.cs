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

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step02UsingTextUtilities
{
  /// <summary>A custom label style that places text using WPF text utilities.</summary>
  internal sealed class CustomLabelStyle : LabelStyleBase<CustomLabelStyle.CustomLabelVisual>
  {
    private static readonly Typeface Typeface = new("Arial");
    private const double Padding = 3;

    protected override CustomLabelVisual CreateVisual(IRenderContext context, ILabel label) {
      var layout = label.GetLayout();
      var visual = new CustomLabelVisual(label.Text, layout.Width, layout.Height);
      ArrangeByLayout(context, visual, layout, true);
      return visual;
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      return new SizeD(80, 14);
    }

    /// <summary>The WPF element that draws the text inside the arranged label layout.</summary>
    internal sealed class CustomLabelVisual : FrameworkElement
    {
      private readonly string text;

      public CustomLabelVisual(string text, double width, double height) {
        this.text = text;
        Width = width;
        Height = height;
      }

      protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);
        var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface,
            12, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        var textY = Height - formattedText.Height;
        drawingContext.DrawText(formattedText, new Point(Padding, textY));
      }
    }
  }
}
