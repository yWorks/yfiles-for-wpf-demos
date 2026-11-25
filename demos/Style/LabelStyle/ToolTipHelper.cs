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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.LabelStyle
{
  /// <summary>
  ///   Configures the given input mode to show tool tips for labels.
  ///   The tool tips show a description of the corresponding label's configuration.
  /// </summary>
  internal static class ToolTipHelper
  {
    public static void ConfigureToolTips(GraphEditorInputMode inputMode) {
      var toolTipInputMode = inputMode.ToolTipInputMode;
      // Show the tooltip faster when hovering an item.
      toolTipInputMode.Delay = TimeSpan.FromMilliseconds(50);

      // Register a listener for when a tool tip should be shown.
      inputMode.QueryItemToolTip += (_, evt) => {
        if (evt.Handled) {
          // Tool tip content has already been assigned -> nothing to do.
          return;
        }

        // Use a rich WPF element as tool tip content. Alternatively, a plain string would do as well.
        if (evt.Item is ILabel label) {
          evt.ToolTip = CreateToolTipContent(label);
          // Indicate that the tool tip content has been set.
          evt.Handled = true;
        }
      };
    }

    /// <summary>
    ///   The tool tip may either be a plain string or it can also be a rich WPF element. In this case, we
    ///   show the latter. We just extract the first label text from the given item and show it as
    ///   tool tip.
    /// </summary>
    private static FrameworkElement CreateToolTipContent(ILabel label) {
      var style = (Graph.Styles.LabelStyle) label.Style;

      var title = new TextBlock {
          Text = $"Properties of \"{label.Text}\"",
          FontWeight = FontWeights.SemiBold,
          Margin = new Thickness(0, 3, 0, 5)
      };
      var grid1 = CreateToolTipGrid();
      AddToToolTipGrid(grid1, "Shape", style.Shape.ToString());
      AddToToolTipGrid(grid1, "Background Fill", style.BackgroundBrush!);
      AddToToolTipGrid(grid1, "Insets",
          $"[{style.Padding.Top}, {style.Padding.Right}, {style.Padding.Bottom}, {style.Padding.Left}]");

      var textTitle = new TextBlock {
          Text = "Text properties",
          FontWeight = FontWeights.SemiBold,
          Margin = new Thickness(0, 3, 0, 5)
      };
      var grid2 = CreateToolTipGrid();
      AddToToolTipGrid(grid2, "Font", $"{style.Typeface.FontFamily.Source}, {style.TextSize}");
      AddToToolTipGrid(grid2, "Text Color", style.TextBrush);
      AddToToolTipGrid(grid2, "Horizontal Alignment", style.TextAlignment.ToString());
      AddToToolTipGrid(grid2, "Vertical Alignment", style.VerticalTextAlignment.ToString());
      AddToToolTipGrid(grid2, "Wrapping", GetWrappingName(style));

      // build the tooltip container
      var toolTipContent = new StackPanel();
      toolTipContent.Children.Add(title);
      toolTipContent.Children.Add(grid1);
      toolTipContent.Children.Add(textTitle);
      toolTipContent.Children.Add(grid2);
      return new Border {
          Background = new SolidColorBrush(Color.FromRgb(0xe0, 0xed, 0xf0)),
          Padding = new Thickness(5),
          Child = toolTipContent
      };
    }

    private static Grid CreateToolTipGrid() {
      var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
      return grid;
    }

    private static string GetWrappingName(Graph.Styles.LabelStyle style) {
      if (style.TextWrapping == TextWrapping.Wrap && style.TextTrimming == TextTrimming.WordEllipsis) {
        return "WrapWordEllipsis";
      }
      if (style.TextWrapping == TextWrapping.NoWrap && style.ClipText) {
        return "Clip";
      }
      return "None";
    }

    /// <summary>
    ///   Adds a property with a given key and value to the grid that shows properties
    ///   as key-value pairs.
    /// </summary>
    private static void AddToToolTipGrid(Grid grid, string key, object value) {
      var rowIndex = grid.RowDefinitions.Count;
      grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      var keyText = new TextBlock {
          Text = key,
          VerticalAlignment = VerticalAlignment.Center,
          Margin = new Thickness(0, 0, 8, 2)
      };
      Grid.SetRow(keyText, rowIndex);
      Grid.SetColumn(keyText, 0);
      grid.Children.Add(keyText);

      FrameworkElement valueElement = value is Brush brush
          ? new Border {
              Background = brush,
              Width = 80,
              Height = 16,
              HorizontalAlignment = HorizontalAlignment.Left,
              Margin = new Thickness(0, 0, 0, 2)
          }
          : new TextBlock {
              Text = value.ToString(),
              VerticalAlignment = VerticalAlignment.Center,
              Margin = new Thickness(0, 0, 0, 2)
          };
      Grid.SetRow(valueElement, rowIndex);
      Grid.SetColumn(valueElement, 1);
      grid.Children.Add(valueElement);
    }
  }
}
