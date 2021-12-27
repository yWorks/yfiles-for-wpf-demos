/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  /// Also there is a customized button displayed in the label at certain zoom levels that enables editing of the label text.
  /// </summary>
  public class MySimpleLabelStyle : LabelStyleBase<VisualGroup>
  {
    private static readonly SolidColorBrush fillBrush;
    private static readonly Style editButtonStyle;

    private const int HorizontalInset = 4;
    private const int VerticalInset = 2;
    private const int ButtonSize = 16;

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
      editButtonStyle = (Style)Application.Current.Resources["EditLabelButtonStyle"];
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
    /// Creates the visual for a label to be drawn
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      // This implementation creates a VisualGroup and uses it for the rendering of the label.
      var container = new VisualGroup();
      // Get the necessary data for rendering of the label
      RenderDataCache cache = CreateRenderDataCache(context, label, Typeface);
      // Render the label
      var labelLayout = label.GetLayout();
      Render(context, label, container, labelLayout, cache);
      // move container to correct location
      ArrangeByLayout(context, container, labelLayout, true);
      return container;
    }

    /// <summary>
    /// Re-renders the label using the old visual for performance reasons.
    /// </summary>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      // get the data with which the old visual was created
      RenderDataCache oldCache = oldVisual.GetRenderDataCache<RenderDataCache>();
      // get the data for the new visual
      RenderDataCache newCache = CreateRenderDataCache(context, label, Typeface);
      var labelLayout = label.GetLayout();
      if (!oldCache.Equals(newCache)) {
        // something changed - re-render the visual
        oldVisual.Children.Clear();
        Render(context, label, oldVisual, labelLayout, newCache);
      }
      // nothing changed, return the old visual
      // arrange because the layout might have changed
      ArrangeByLayout(context, oldVisual, labelLayout, true);
      return oldVisual;
    }

    /// <summary>
    /// Creates an object containing all necessary data to create a label visual
    /// </summary>
    private RenderDataCache CreateRenderDataCache(IRenderContext context, ILabel label, Typeface typeface) {
      // Visibility of button changes dependent on the zoom level
      Visibility buttonVisibility = context.Zoom >= 1 ? Visibility.Visible : Visibility.Collapsed;
      return new RenderDataCache(label.Text, buttonVisibility, typeface);
    }

    /// <summary>
    /// Creates the visual appearance of a label
    /// </summary>
    private void Render(IRenderContext context, ILabel label, VisualGroup container, IOrientedRectangle labelLayout, RenderDataCache cache) {
      // store information with the visual on how we created it
      container.SetRenderDataCache(cache);

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
        Text = cache.LabelText,
        FontFamily = cache.Typeface.FontFamily,
        FontStretch = cache.Typeface.Stretch,
        FontStyle = cache.Typeface.Style,
        FontWeight = cache.Typeface.Weight,
        Foreground = Brushes.Black,
      };

      textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

      // if edit button is visible align left, otherwise center
      double textPositionLeft = cache.ButtonVisibility == Visibility.Visible
                                  ? HorizontalInset
                                  : (labelLayout.Width - textBlock.DesiredSize.Width) / 2;

      textBlock.SetCanvasArrangeRect(new Rect(textPositionLeft,
                                              (labelLayout.Height - textBlock.DesiredSize.Height) / 2,
                                              textBlock.DesiredSize.Width,
                                              textBlock.DesiredSize.Height
                                       ));
      container.Add(textBlock);

      if (cache.ButtonVisibility == Visibility.Visible) {
        // get style for edit button from XAML resources

        // create edit button
        Button editLabelButton = new Button
        {
          Style = editButtonStyle
        };
        editLabelButton.SetCanvasArrangeRect(new Rect(labelLayout.Width - HorizontalInset - ButtonSize, VerticalInset, ButtonSize, ButtonSize));

        // set button command
        editLabelButton.Command = GraphCommands.EditLabel;
        editLabelButton.CommandParameter = label;
        editLabelButton.CommandTarget = context.CanvasControl;

        container.Add(editLabelButton);
      }
    }


    #endregion

    #region Rendering Helper Methods

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

    #endregion
    
    /// <summary>
    /// Saves the data which is necessary for the creation of a label
    /// </summary>
    private sealed class RenderDataCache
    {
      public string LabelText { get; private set; }
      public Visibility ButtonVisibility { get; private set; }
      public Typeface Typeface { get; private set; }

      public RenderDataCache(String labelText, Visibility buttonVisibility, Typeface typeface) {
        LabelText = labelText;
        ButtonVisibility = buttonVisibility;
        Typeface = typeface;
      }

      /// <summary>
      /// Check if this instance is equal to another RenderDataCache object
      /// </summary>
      public bool Equals(RenderDataCache other) {
        return other.LabelText.Equals(LabelText) && other.ButtonVisibility == ButtonVisibility && other.Typeface.Equals(Typeface);
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(RenderDataCache)) {
          return false;
        }
        return Equals((RenderDataCache)obj);
      }
    }
  }
}
