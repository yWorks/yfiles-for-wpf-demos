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
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Legend;

/// <summary>
///   This demo shows different ways to visualize a legend.
/// </summary>
[Demo("legend", "view", "Legend", Summary = "Shows different ways to display a legend.",
    Tags = ["legend", "node types"])]
public sealed partial class LegendDemo
{
  private OverlayPanelLegend overlayLegend = null!;
  private RenderTreeWorldLegend renderTreeWorldLegend = null!;
  private RenderTreeViewLegend renderTreeViewLegend = null!;
  private GraphControlLegend graphControlLegend = null!;
  private Dictionary<string, LegendBase> legends = [];
  private LegendBase currentLegend = null!;
  private bool initialized;

  public LegendDemo() {
    InitializeComponent();
  }

  protected override void Initialize() {
    // initialize the graph component
    graphControl.ViewportLimiter.Policy = ViewportLimitingPolicy.Unrestricted;

    // disable editing for this demo
    var graphViewerInputMode = new GraphViewerInputMode {
        SelectableItems = GraphItemTypes.None,
        FocusableItems = GraphItemTypes.None,
        MarqueeSelectableItems = GraphItemTypes.None
    };
    graphControl.InputMode = graphViewerInputMode;

    // create different types of legends
    overlayLegend = new OverlayPanelLegend(graphControl, overlayLegendContainer, overlayLegendEntries);
    renderTreeWorldLegend = new RenderTreeWorldLegend(graphControl, graphViewerInputMode);
    renderTreeViewLegend = new RenderTreeViewLegend(graphControl, graphViewerInputMode);
    graphControlLegend = new GraphControlLegend(
        graphControl, graphLegendContainer, graphLegendHeader, legendGraphControl);
    legends = new Dictionary<string, LegendBase> {
        ["overlay"] = overlayLegend,
        ["world"] = renderTreeWorldLegend,
        ["view"] = renderTreeViewLegend,
        ["graphControl"] = graphControlLegend
    };

    // set graph defaults
    InitializeGraph(graphControl.Graph);

    // build the graph from the given data set
    BuildGraphFromJson("graph-data.json", graphControl.Graph);

    // apply styles based on node type
    foreach (var node in graphControl.Graph.Nodes) {
      var nodeType = ParseNodeType((string) node.Tag!);
      graphControl.Graph.SetStyle(node, NodeTypes.CreateStyle(nodeType));
      node.Tag = nodeType;
    }

    // apply a hierarchical layout
    graphControl.Graph.ApplyLayout(new HierarchicalLayout {
        MinimumLayerDistance = 80,
        NodeDistance = 40,
        LayoutOrientation = LayoutOrientation.LeftToRight
    });

    // make sure the graph is centered in the view
    _ = graphControl.FitGraphBounds();

    // start with the overlay panel legend
    currentLegend = overlayLegend;
    initialized = true;
    currentLegend.Show();
    InitializeUI();
  }

  /// <summary>
  ///   Sets graph defaults like style and a highlight decorator.
  /// </summary>
  private static void InitializeGraph(IGraph graph) {
    graph.NodeDefaults.Size = new SizeD(120, 36);
    graph.NodeDefaults.ShareStyleInstance = false;
    graph.NodeDefaults.Labels.Style = new LabelStyle { TextBrush = Brushes.White };
    graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(Themes.PaletteOrange);

    // define a highlight renderer that uses the shape of the node
    var highlightPen = new Pen(new SolidColorBrush(Color.FromRgb(0x0b, 0x71, 0x89)), 3);
    highlightPen.Freeze();
    graph.GetDecorator().Nodes.HighlightRenderer.AddFactory(node => {
      var type = node.Tag is NodeTypeId nodeType ? nodeType : NodeTypeId.Employee;
      return new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              Shape = NodeTypes.Infos[type].Shape ?? ShapeNodeShape.RoundRectangle,
              Brush = Brushes.Transparent,
              Pen = highlightPen
          },
          Margins = new InsetsD(4)
      };
    });
  }

  /// <summary>
  ///   Binds the controls in the toolbar to their functionality.
  /// </summary>
  private void InitializeUI() {
    legendTypeComboBox.SelectedIndex = 0;
    legendVisibilityToggle.IsChecked = true;
    orientationToggle.IsChecked = false;
    viewportFilterToggle.IsChecked = false;
    graphControl.ViewportChanged += OnGraphViewportChanged;
    UpdateViewportFilter();
  }

  private void OnLegendTypeChanged(object sender, SelectionChangedEventArgs args) {
    if (!initialized || legendTypeComboBox.SelectedItem is not ComboBoxItem { Tag: string legendId }) {
      return;
    }

    // hide current legend
    currentLegend.Hide();
    currentLegend = legends[legendId];

    // sync the legend settings
    currentLegend.SetHorizontal(orientationToggle.IsChecked == true);
    currentLegend.SetShowOnlyVisible(viewportFilterToggle.IsChecked == true);

    // show the new legend if legends are enabled
    if (legendVisibilityToggle.IsChecked == true) {
      currentLegend.Show();
    }
  }

  private void OnLegendVisibilityChanged(object sender, RoutedEventArgs args) {
    if (!initialized) {
      return;
    }
    if (legendVisibilityToggle.IsChecked == true) {
      currentLegend.Show();
    } else {
      currentLegend.Hide();
    }
  }

  private void OnOrientationChanged(object sender, RoutedEventArgs args) {
    if (!initialized) {
      return;
    }
    currentLegend.SetHorizontal(orientationToggle.IsChecked == true);
    currentLegend.Refresh();
  }

  private void OnViewportFilterChanged(object sender, RoutedEventArgs args) {
    if (initialized) {
      UpdateViewportFilter();
    }
  }

  private void UpdateViewportFilter() {
    currentLegend.SetShowOnlyVisible(viewportFilterToggle.IsChecked == true);
    currentLegend.Refresh();
  }

  private void OnGraphViewportChanged(object? sender, PropertyChangedEventArgs args) {
    if (viewportFilterToggle.IsChecked == true) {
      RefreshCurrentLegend();
    }
  }

  private void RefreshCurrentLegend() => currentLegend.Refresh();

  private void OnToggleGraphLegend(object sender, RoutedEventArgs args) => graphControlLegend.Toggle();

  private static NodeTypeId ParseNodeType(string value) =>
      NodeTypes.TryParse(value, out var type) ? type : throw new InvalidDataException($"Unknown node type '{value}'.");

  [STAThread]
  private static void Main(string[] args) => Run<LegendDemo>();
}
