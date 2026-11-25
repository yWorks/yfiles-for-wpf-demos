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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Demo.HierarchicalGridComponents
{
  /// <summary>
  ///   Displays the demo's graph.
  /// </summary>
  [Demo("hierarchical-grid-components", "layout", "Hierarchical Grid Components",
      Summary = "Grid components in the hierarchical layout result in more compact arrangements.",
      Tags = ["layout", "hierarchical", "grid components"],
      Keywords = ["layoutfeatures", "gridcomponents", "bus structures", "orthogonal", "compact"])]
  public sealed partial class HierarchicalGridComponentsDemo
  {
    private readonly Settings settings = new();
    private readonly DispatcherTimer layoutTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private BusPreset lastPreset = BusPreset.Balanced;

    /// <summary>
    ///   State guard to prevent concurrent layout calculations.
    /// </summary>
    private bool layoutRunning;

    /// <summary>
    ///   A collection of colors which is used to distinguish the different buses.
    /// </summary>
    private static readonly Color[] colors = [
        Colors.Crimson,
        Colors.DarkTurquoise,
        Colors.CornflowerBlue,
        Colors.DarkSlateBlue,
        Colors.Gold,
        Colors.MediumSlateBlue,
        Colors.ForestGreen,
        Colors.MediumVioletRed,
        Colors.DarkCyan,
        Colors.Chocolate,
        Colors.LimeGreen,
        Colors.MediumOrchid,
        Colors.RoyalBlue,
        Colors.OrangeRed
    ];

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public HierarchicalGridComponentsDemo() {
      InitializeComponent();
      InitializeUI();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override async void Initialize() {
      ConfigureGraph(Graph);

      // create the demo's sample graph
      LoadGraph(Graph);

      // arrange the sample graph
      await RunLayout();

      // enable interactive editing
      GraphControl.InputMode = new GraphEditorInputMode();
    }

    /// <summary>
    ///   Arranges the demo's graph with <see cref="HierarchicalLayout"/>.
    ///   The hierarchical layout algorithm is configured to route edges in bus structures.
    /// </summary>
    private async Task RunLayout() {
      if (layoutRunning) {
        return;
      }
      layoutRunning = true;
      DisableUI(true);

      var graph = Graph;
      // we collect each star structure with more than 5 incoming/outgoing edges as a bus structure
      var busses = settings.UseGridComponents ? GetBusStructures(graph) : [];

      // clear previous edge highlights
      foreach (var edge in graph.Edges) {
        graph.SetStyle(edge, graph.EdgeDefaults.Style);
      }
      // set new highlights: use a common line color for all edges belonging to the same bus structure
      foreach (var busEdges in busses) {
        HighlightEdges(graph, busEdges);
      }

      // create a configured hierarchical layout instance
      var layout = new HierarchicalLayout { AutomaticEdgeGrouping = true };

      // create layout data that defines bus structures
      var layoutData = layout.CreateLayoutData(graph);
      AddGridComponents(layoutData.GridComponents, busses);

      try {
        // apply the layout
        await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(700), layoutData);
      } finally {
        layoutRunning = false;
        DisableUI(false);
      }
    }

    /// <summary>
    ///   Adds the grid components for the <see cref="HierarchicalLayout"/> based on the current UI state.
    /// </summary>
    /// <param name="gridComponents">The mapping from edges to their grid component descriptors.</param>
    /// <param name="busses">An array of edge lists, each list defining one bus structure.</param>
    private void AddGridComponents(
        ItemCollectionMapping<IEdge, GridComponentDescriptor> gridComponents,
        IReadOnlyList<IEdge>[] busses) {
      foreach (var busEdges in busses) {
        var componentDescriptor = new GridComponentDescriptor();

        // maybe limit the bus structure sizes
        var busSettings = GetBusSettings(settings.Preset, busEdges.Count);
        if (busSettings.HasValue) {
          componentDescriptor.MaximumNodesBeforeBus = busSettings.Value.MaxBeforeBus;
          componentDescriptor.MaximumNodesAfterBus = busSettings.Value.MaxAfterBus;
        }

        // add the bus descriptor to the layout data
        var busCollection = gridComponents.Add(componentDescriptor);

        // specify which edges are part of this bus
        busCollection.Items = [.. busEdges];
      }
    }

    /// <summary>
    ///   Returns settings for bus structure descriptors of <c>HierarchicalLayoutData</c>.
    /// </summary>
    /// <param name="busStylePreset">The desired bus style preset.</param>
    /// <param name="elementCount">The number of elements in the bus that is to be configured.</param>
    private (int MaxBeforeBus, int MaxAfterBus)? GetBusSettings(BusPreset busStylePreset, int elementCount) {
      var beforeBusValue = 0;
      var afterBusValue = 0;
      switch (busStylePreset) {
        case BusPreset.Balanced:
          // the default bus structure setting without further configuration
          return null;
        case BusPreset.Squares: {
          var rowLength = Math.Ceiling(Math.Sqrt(elementCount));
          beforeBusValue = (int) Math.Floor(rowLength / 2);
          afterBusValue = (int) Math.Ceiling(rowLength / 2);
          break;
        }
        case BusPreset.Leaves:
          beforeBusValue = 1;
          afterBusValue = 1;
          break;
        case BusPreset.LeftAligned:
          beforeBusValue = 0;
          afterBusValue = int.MaxValue;
          break;
        case BusPreset.RightAligned:
          beforeBusValue = int.MaxValue;
          afterBusValue = 0;
          break;
        case BusPreset.Custom:
          beforeBusValue = (int) settings.BeforeBusLimit;
          afterBusValue = (int) settings.AfterBusLimit;
          break;
      }
      return (beforeBusValue, afterBusValue);
    }

    /// <summary>
    ///   Returns an array of edge lists. The edges in each list should be grouped into one bus structure.
    ///   This demo identifies those structure by looking for stars that are made up from incoming
    ///   or outgoing edges.
    /// </summary>
    /// <param name="graph">The graph to check for star structures.</param>
    /// <returns>An array of edge lists that should be grouped into a bus.</returns>
    private static IReadOnlyList<IEdge>[] GetBusStructures(IGraph graph) {
      var incomingBusses = graph.Nodes.Where(root => graph.InEdgesAt(root).Count > 5)
          .Select(root => graph.InEdgesAt(root));
      var outgoingBusses = graph.Nodes.Where(root => graph.OutEdgesAt(root).Count > 5)
          .Select(root => graph.OutEdgesAt(root));
      return [.. incomingBusses, .. outgoingBusses];
    }

    /// <summary>
    ///   Highlights the given edges by applying a specific color to each edge.
    /// </summary>
    /// <param name="graph">The graph whose edges will be highlighted.</param>
    /// <param name="edgeList">The edges that will be highlighted.</param>
    private static void HighlightEdges(IGraph graph, IReadOnlyList<IEdge> edgeList) {
      // in this demo we just assign a color based on the number of edges for the bus structure
      var style = NewEdgeStyle(colors[edgeList.Count % colors.Length]);
      foreach (var edge in edgeList) {
        graph.SetStyle(edge, style);
      }
    }

    /// <summary>
    ///   Returns an edge style with the given color.
    /// </summary>
    /// <param name="color">The line color for the created edge style.</param>
    private static PolylineEdgeStyle NewEdgeStyle(Color color) =>
        new() {
            Pen = new Pen(new SolidColorBrush(color), 3),
            TargetArrow = new Arrow(ArrowType.Triangle, color),
            SmoothingLength = 15
        };

    /// <summary>
    ///   Configures default visualizations for the given graph.
    /// </summary>
    /// <param name="graph">The demo's graph.</param>
    private void ConfigureGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph, Themes.Palette58);

      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Size = new SizeD(50, 30);
      graph.EdgeDefaults.Style = NewEdgeStyle(Themes.Palette58.Fill.Color);
      graph.EdgeDefaults.ShareStyleInstance = false;
    }

    /// <summary>
    ///   Creates a sample graph structure from the demo's sample data.
    /// </summary>
    /// <param name="graph">The demo's graph.</param>
    private void LoadGraph(IGraph graph) {
      BuildGraphFromJson("graph-data.json", graph);
    }

    /// <summary>
    ///   Helper function to set a value to a given slider in the UI.
    /// </summary>
    /// <param name="beforeBus">Whether to set the slider for nodes before the bus.</param>
    /// <param name="value">The value to set.</param>
    private void SetSliderValue(bool beforeBus, double value) {
      var sliderValue = double.IsFinite(value) ? value : 20;
      if (beforeBus) {
        settings.BeforeBusLimit = sliderValue;
      } else {
        settings.AfterBusLimit = sliderValue;
      }
    }

    /// <summary>
    ///   Helper function to disable UI during layout animation.
    /// </summary>
    private void DisableUI(bool disabled) {
      settings.IsLayoutRunning = disabled;
    }

    private void ScheduleLayout() {
      layoutTimer.Stop();
      layoutTimer.Start();
    }

    /// <summary>
    ///   Binds actions to the buttons and controls in the toolbar.
    /// </summary>
    private void InitializeUI() {
      demoToolBar.DataContext = settings;
      settings.SettingChanged += (_, _) => ScheduleLayout();
      settings.PropertyChanged += (_, e) => {
        if (e.PropertyName == nameof(Settings.Preset)) {
          if (settings.Preset == BusPreset.Custom) {
            if (lastPreset == BusPreset.LeftAligned) {
              SetSliderValue(false, 5);
            }
            if (lastPreset == BusPreset.RightAligned) {
              SetSliderValue(true, 5);
            }
          }
          lastPreset = settings.Preset;
        }
      };
      layoutTimer.Tick += async (_, _) => {
        layoutTimer.Stop();
        await RunLayout();
      };
    }

    private async void LayoutButtonClick(object sender, RoutedEventArgs e) {
      layoutTimer.Stop();
      await RunLayout();
    }

    [STAThread]
    public static void Main() {
      Run<HierarchicalGridComponentsDemo>();
    }
  }
}
