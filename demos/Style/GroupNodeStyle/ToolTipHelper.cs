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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.GroupNodeStyle
{
  /// <summary>
  ///   Configures the given input mode to show tool tips for group nodes and folder nodes.
  ///   The tool tips show a description of the corresponding node style configuration.
  /// </summary>
  internal static class ToolTipHelper
  {
    public static void ConfigureToolTips(GraphEditorInputMode inputMode) {
      var toolTipInputMode = inputMode.ToolTipInputMode;
      // Show the tooltip with an additional offset to the pointer location.
      toolTipInputMode.ToolTipLocationOffset = new PointD(30, 30);
      inputMode.ToolTipItems = GraphItemTypes.Node;

      // Register a listener for when a tool tip should be shown.
      inputMode.QueryItemToolTip += (_, evt) => {
        if (evt.Handled) {
          // Tool tip content has already been assigned -> nothing to do.
          return;
        }

        // Use a rich WPF element as tool tip content. Alternatively, a plain string would do as well.
        if (evt.Item is INode node && node.Style is Graph.Styles.GroupNodeStyle) {
          evt.ToolTip = CreateToolTipContent(node);
          // Indicate that the tool tip content has been set.
          evt.Handled = true;
        }
      };
    }

    /// <summary>
    ///   The tool tip may either be a plain string or it can also be a rich WPF element. In this case,
    ///   we show the latter with the properties of the node style.
    /// </summary>
    private static FrameworkElement CreateToolTipContent(INode node) {
      var style = (Graph.Styles.GroupNodeStyle) node.Style;

      var title = new TextBlock {
          Text = "GroupNodeStyle Properties",
          FontWeight = FontWeights.Bold,
          Margin = new Thickness(0, 0, 0, 8)
      };

      var grid = new Grid { Margin = new Thickness(8) };
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
      AddToToolTipGrid(grid, "Folder Icon", style.FolderIcon.ToString());
      AddToToolTipGrid(grid, "Group Icon", style.GroupIcon.ToString());
      AddToToolTipGrid(grid, "Icon Position", style.IconPosition.ToString());
      AddSeparator(grid);
      AddToToolTipGrid(grid, "Tab Position", style.TabPosition.ToString());
      AddToToolTipGrid(grid, "Tab Inset", style.TabPadding);
      AddToToolTipGrid(grid, "Tab Slope", style.TabSlope);
      AddToToolTipGrid(grid, "Tab Width", style.TabWidth);
      AddToToolTipGrid(grid, "Tab Height", style.TabHeight);
      AddToToolTipGrid(grid, "Tab Fill", style.TabBrush);
      AddToToolTipGrid(grid, "Tab Background Fill", style.TabBackgroundBrush);
      AddSeparator(grid);
      AddToToolTipGrid(grid, "Content Area Fill", style.ContentAreaBrush);
      var padding = style.ContentAreaPadding;
      AddToToolTipGrid(grid, "Content Area Insets", $"[{padding.Top} {padding.Right} {padding.Bottom} {padding.Left}]");
      AddSeparator(grid);
      AddToToolTipGrid(grid, "Corner Radius", style.CornerRadius);

      // build the tooltip container
      var container = new StackPanel {
          Background = new SolidColorBrush(Color.FromRgb(0xe0, 0xed, 0xf0)),
          Margin = new Thickness(0)
      };
      container.Children.Add(title);
      container.Children.Add(grid);
      return container;
    }

    /// <summary>
    ///   Adds a property with a given key and value to the grid that shows properties as key-value pairs.
    /// </summary>
    private static void AddToToolTipGrid(Grid grid, string key, object? value) {
      var row = grid.RowDefinitions.Count;
      grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      var keyText = new TextBlock {
          Text = key,
          VerticalAlignment = VerticalAlignment.Center
      };
      Grid.SetRow(keyText, row);
      Grid.SetColumn(keyText, 0);
      grid.Children.Add(keyText);

      FrameworkElement valueElement;
      if (value is Brush brush) {
        valueElement = new Border {
            Background = brush,
            Width = 80,
            Height = 16,
            HorizontalAlignment = HorizontalAlignment.Left
        };
      } else {
        var isNull = value is null;
        valueElement = new TextBlock {
            Text = value?.ToString() ?? "null",
            FontStyle = isNull ? FontStyles.Italic : FontStyles.Normal,
            VerticalAlignment = VerticalAlignment.Center
        };
      }

      Grid.SetRow(valueElement, row);
      Grid.SetColumn(valueElement, 1);
      grid.Children.Add(valueElement);
    }

    /// <summary>
    ///   Adds some extra spacing below the last row in the grid.
    /// </summary>
    private static void AddSeparator(Grid grid) {
      var lastValue = (FrameworkElement) grid.Children[grid.Children.Count - 1];
      lastValue.Margin = new Thickness(0, 0, 0, 8);
      var lastKey = (FrameworkElement) grid.Children[grid.Children.Count - 2];
      lastKey.Margin = new Thickness(0, 0, 0, 8);
    }
  }
}
