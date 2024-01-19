/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Size = System.Windows.Size;

namespace Tutorial.CustomStyles
{
  /// <summary>
  /// This class is an example for a custom style based on the <see cref="LabelStyleBase{TVisual}"/>.
  /// The typeface for the label text can be set. The label text is drawn with black letters inside a blue rounded rectangle.
  /// </summary>
  public class MySimpleLabelStyle : LabelStyleBase<VisualGroup>
  {
    private static readonly SolidColorBrush fillBrush;

    private const int HorizontalInset = 4;
    private const int VerticalInset = 2;
    private const int ButtonSize = 16;

    ////////////////////////////////////////////////////
    //////////////// New in this sample ////////////////
    ////////////////////////////////////////////////////

    /// <summary>
    /// Calculates the preferred size for the given label if this style is used for the rendering.
    /// </summary>
    /// <remarks>
    /// The size is calculated from the label's text.
    /// </remarks>
    protected override SizeD GetPreferredSize(ILabel label) {
      // return size of the text block plus some space for the button
      TextBlock tb = new TextBlock
      {
        Text = label.Text,
        FontFamily = Typeface.FontFamily,
        FontStretch = Typeface.Stretch,
        FontStyle = Typeface.Style,
        FontWeight = Typeface.Weight
      };

      // first measure
      tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      if (tb.IsMeasureValid) {
        // then use the desired size - plus rounding and insets, as well as space for button
        return new SizeD(
          Math.Ceiling(0.5d + tb.DesiredSize.Width) + HorizontalInset * 3 + ButtonSize,
          2 * VerticalInset + Math.Max(ButtonSize, Math.Ceiling(0.5d + tb.DesiredSize.Height)));
      } else {
        return new SizeD(HorizontalInset * 3 + ButtonSize + 50, VerticalInset * 2 + ButtonSize);
      }
    }

    ////////////////////////////////////////////////////

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MySimpleLabelStyle"/> class using the "Arial" typeface.
    /// </summary>
    public MySimpleLabelStyle() {
      Typeface = new Typeface("Arial");
    }

    static MySimpleLabelStyle() {
      fillBrush = new SolidColorBrush(Color.FromArgb(255, 155, 226, 255));
      fillBrush.Freeze();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the typeface used for rendering the label text.
    /// </summary>
    public Typeface Typeface { get; set; }

    #endregion

    #region Rendering

    /// <summary>
    /// Creates the visual appearance of a label
    /// </summary>
    private void Render(IRenderContext context, ILabel label, VisualGroup container, IOrientedRectangle labelLayout) {

      // background rectangle
      System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
      {
        Width = labelLayout.Width,
        Height = labelLayout.Height,
        RadiusX = labelLayout.Width / 10,
        RadiusY = labelLayout.Height / 10,
        Stroke = Brushes.SkyBlue,
        Fill = fillBrush,
        StrokeThickness = 1
      };
      container.Add(rect);

      // TextBlock with label text
      TextBlock textBlock = new TextBlock
      {
        Text = label.Text,
        FontFamily = Typeface.FontFamily,
        FontStretch = Typeface.Stretch,
        FontStyle = Typeface.Style,
        FontWeight = Typeface.Weight,
        Foreground = Brushes.Black,
      };

      textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

      // if edit button is visible align left, otherwise center
      double textPositionLeft = (labelLayout.Width - textBlock.DesiredSize.Width) / 2;

      textBlock.SetCanvasArrangeRect(new Rect(textPositionLeft,
                                              (labelLayout.Height - textBlock.DesiredSize.Height) / 2,
                                              textBlock.DesiredSize.Width,
                                              textBlock.DesiredSize.Height
                                       ));
      container.Add(textBlock);
    }

    /// <summary>
    /// Creates the visual for a label to be drawn
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      // This implementation creates a VisualGroup and uses it for the rendering of the label.
      var container = new VisualGroup();
      // Render the label
      var labelLayout = label.GetLayout();
      Render(context, label, container, labelLayout);
      // move container to correct location
      // We delegate to LabelStyleBase's ArrangeByLayout method to position the container.
      // This method sets the layout properly and applies auto flipping to the text if necessary.
      ArrangeByLayout(context, container, labelLayout, true);
      return container;
    }

    #endregion

  }
}
