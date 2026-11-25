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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Legend;

/// <summary>
///   Base class for shared functionality of all legends.
/// </summary>
internal abstract class LegendBase
{
  private static readonly GraphControl IconGraphControl = new();
  private string? lastVisibleTypes;
  private bool wasHorizontal;
  private bool onlyVisible;

  protected readonly GraphControl graphControl;

  protected bool Horizontal { get; private set; }

  protected bool IsVisible { get; private set; }

  protected LegendBase(GraphControl graphControl) {
    this.graphControl = graphControl;
  }

  /// <summary>
  ///   sets whether the legend entries should be displayed horizontally.
  /// </summary>
  public void SetHorizontal(bool horizontal) {
    Horizontal = horizontal;
  }

  /// <summary>
  ///   Sets whether only nodes visible in the current viewport should be considered for the legend.
  /// </summary>
  public void SetShowOnlyVisible(bool visible) {
    onlyVisible = visible;
  }

  /// <summary>
  ///   Shows the legend.
  /// </summary>
  public void Show() {
    IsVisible = true;
    OnShow();
    Refresh();
  }

  /// <summary>
  ///   Hides the legend.
  /// </summary>
  public void Hide() {
    IsVisible = false;
    OnHide();
    // reset the cached visible node types which causes the next refresh to update the legend
    lastVisibleTypes = null;
  }

  /// <summary>
  ///   Refreshes the legend if the visible node types or the orientation have changed.
  /// </summary>
  public void Refresh(bool forceRefresh = false) {
    if (!IsVisible) {
      return;
    }

    var nodeTypesInGraph = GetNodeTypesInGraph();
    var typesKey = string.Join(",", nodeTypesInGraph);
    if (typesKey == lastVisibleTypes && wasHorizontal == Horizontal && !forceRefresh) {
      // nothing relevant to the legend has changed
      return;
    }

    // let the actual legend create itself
    RefreshCore(nodeTypesInGraph);
    lastVisibleTypes = typesKey;
    wasHorizontal = Horizontal;
  }

  /// <summary>
  ///   Derived classes implement this to update the legend visuals.
  /// </summary>
  protected abstract void RefreshCore(IReadOnlyList<NodeTypeId> visibleTypes);

  /// <summary>
  ///   Hook for derived classes to perform custom work when the legend is shown.
  /// </summary>
  protected virtual void OnShow() { }

  /// <summary>
  ///   Hook for derived classes to perform custom work when the legend is hidden.
  /// </summary>
  protected virtual void OnHide() { }

  /// <summary>
  ///   Returns the set of node types that are present in the graph, considering the "show only visible" option.
  /// </summary>
  private IReadOnlyList<NodeTypeId> GetNodeTypesInGraph() {
    var presentTypes = new HashSet<NodeTypeId>();
    foreach (var node in graphControl.Graph.Nodes) {
      if (node.Tag is NodeTypeId type && (!onlyVisible ||
          graphControl.Viewport.Intersects(new RectD(node.Layout.X, node.Layout.Y, node.Layout.Width,
              node.Layout.Height)))) {
        presentTypes.Add(type);
      }
    }
    // sort legend entries alphabetically
    return presentTypes.OrderBy(type => NodeTypes.Infos[type].Name, StringComparer.CurrentCulture).ToArray();
  }

  /// <summary>
  ///   Creates a visual representation of the legend using WPF elements exported from a hidden graph.
  /// </summary>
  protected (Border LegendVisual, SizeD Size) CreateLegendVisual(IReadOnlyList<NodeTypeId> nodeTypes) {
    var entries = new StackPanel {
        Orientation = Horizontal ? Orientation.Horizontal : Orientation.Vertical,
        Margin = new Thickness(14)
    };

    foreach (var type in nodeTypes) {
      var entry = new StackPanel {
          Orientation = Orientation.Horizontal,
          Margin = Horizontal ? new Thickness(0, 0, 14, 0) : new Thickness(0, 0, 0, 8),
          VerticalAlignment = VerticalAlignment.Center
      };
      var icon = CreateNodeTypeVisual(NodeTypes.CreateStyle(type));
      entry.Children.Add(icon);
      entry.Children.Add(new TextBlock {
          Text = NodeTypes.Infos[type].Name,
          Foreground = Brushes.White,
          FontSize = 16,
          VerticalAlignment = VerticalAlignment.Center,
          Margin = new Thickness(8, 0, 0, 0)
      });
      entries.Children.Add(entry);
    }

    var content = new StackPanel();
    content.Children.Add(new TextBlock {
        Text = "Legend",
        Foreground = Brushes.White,
        FontSize = 18,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(14, 12, 14, 0)
    });
    content.Children.Add(entries);
    content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
    var width = Horizontal ? Math.Max(220, content.DesiredSize.Width) : 220;
    var height = Math.Max(Horizontal ? 92 : 120, content.DesiredSize.Height);
    var border = new Border {
        Width = width,
        Height = height,
        Padding = new Thickness(0),
        Background = new SolidColorBrush(Color.FromArgb(242, 56, 83, 107)),
        CornerRadius = new CornerRadius(14)
    };
    border.Child = content;
    border.Measure(new Size(width, height));
    border.Arrange(new Rect(0, 0, width, height));
    border.SetCanvasArrangeRect(new Rect(0, 0, width, height));
    return (border, new SizeD(width, height));
  }

  /// <summary>
  ///   Adds a node and a label for each of the given node types to the given graph.
  /// </summary>
  protected void AddLegendEntries(IReadOnlyList<NodeTypeId> nodeTypes, IGraph graph) {
    var x = 0.0;
    var y = 0.0;
    foreach (var type in nodeTypes) {
      var node = graph.CreateNode(new RectD(x, y, NodeTypes.LegendNodeWidth, NodeTypes.LegendNodeHeight),
          NodeTypes.CreateStyle(type));
      var label = graph.AddLabel(node, NodeTypes.Infos[type].Name, ExteriorNodeLabelModel.Right,
          new LabelStyle {
              VerticalTextAlignment = VerticalAlignment.Center,
              TextSize = 16,
              TextBrush = Brushes.White,
              Padding = new InsetsD(10, 0, 0, 0)
          });
      if (Horizontal) {
        x += NodeTypes.LegendNodeWidth + label.PreferredSize.Width + 24;
      } else {
        y += 50;
      }
    }
  }

  /// <summary>
  ///   Exports a single node style as a WPF element for the panel legend.
  /// </summary>
  protected static UIElement CreateNodeTypeVisual(INodeStyle style) {
    var graph = IconGraphControl.Graph;
    graph.Clear();
    graph.CreateNode(new RectD(0, 0, NodeTypes.LegendNodeWidth, NodeTypes.LegendNodeHeight), style);
    IconGraphControl.UpdateContentBounds(new InsetsD(4));
    var exporter = new UIElementExporter(IconGraphControl.ContentBounds) { Background = Brushes.Transparent };
    return exporter.Export(IconGraphControl);
  }
}
