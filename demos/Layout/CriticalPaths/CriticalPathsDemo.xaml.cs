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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Tree;

namespace yWorks.Demo.CriticalPaths
{
  /// <summary>
  ///   Demonstrates how hierarchical and tree layouts emphasize critical paths.
  /// </summary>
  [Demo("critical-paths", "layout", "Critical Paths",
      Summary =
          "The hierarchical and tree layout styles can emphasize critical (important) paths by aligning their nodes.",
      Tags = ["layout", "hierarchical", "tree"], Keywords = ["layoutfeatures", "data analysis"])]
  public partial class CriticalPathsDemo
  {
    private bool layoutRunning;
    private string layoutStyle = "hierarchical";
    private GraphViewerInputMode inputMode = null!;
    private PriorityPanel priorityPanel = null!;

    public CriticalPathsDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      InitializeInputMode();
      InitializePriorityPanel();
      InitializeUI();
      LoadGraph(layoutStyle);
      _ = RunLayoutAsync(true);
    }

    /// <summary>
    ///   Loads the sample graph which initially provides some priorities.
    /// </summary>
    private void LoadGraph(string sample) {
      priorityPanel.CurrentItems = null;
      Graph.Clear();
      Graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle(Themes.Palette44);
      Graph.EdgeDefaults.Style = new PolylineEdgeStyle();
      Graph.EdgeDefaults.ShareStyleInstance = false;

      var data = LoadGraphData(sample);
      var builder = new GraphBuilder(Graph);
      builder.CreateNodesSource(data.Nodes, node => node.Id);
      builder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target);
      builder.BuildGraph();

      foreach (var edge in Graph.Edges) {
        SetPriority(edge, GetPriority(edge));
      }
    }

    /// <summary>
    ///   Loads and deserializes one of the embedded sample graph files.
    /// </summary>
    private GraphData LoadGraphData(string sample) {
      var resourceName = sample == "hierarchical" ? "hierarchical-graph-data.json" : "tree-graph-data.json";
      using var stream = LoadAssemblyResource(resourceName);
      return JsonSerializer.Deserialize<GraphData>(stream) ??
          throw new InvalidOperationException($"Could not deserialize {resourceName}.");
    }

    /// <summary>
    ///   Specifies the priority of the given edge.
    /// </summary>
    private void SetPriority(IEdge edge, int priority) {
      if (edge.Tag is EdgeData data) {
        data.Priority = priority;
      }
      (edge.Style as PolylineEdgeStyle)!.Pen = GetPen(priority);
    }

    /// <summary>
    ///   Updates the stroke color and thickness according to the given priority.
    /// </summary>
    private static Pen GetPen(int priority) {
      var (color, thickness) = priority switch {
          1 => (Color.FromRgb(0x33, 0x66, 0x99), 3.0),
          2 => (Color.FromRgb(0x56, 0x92, 0x6e), 3.0),
          3 => (Color.FromRgb(0xf0, 0xc8, 0x08), 3.0),
          4 => (Color.FromRgb(0xff, 0x6c, 0x00), 3.0),
          5 => (Color.FromRgb(0xdb, 0x3a, 0x34), 3.0),
          _ => (Color.FromRgb(0xc7, 0xc7, 0xa6), 1.0)
      };
      return new Pen(new SolidColorBrush(color), thickness);
    }

    private static int GetPriority(IModelItem item) =>
        item.Tag switch {
            EdgeData edge => edge.Priority,
            NodeData node => node.Priority,
            _ => 0
        };

    /// <summary>
    ///   Applies a hierarchical layout considering the edge priorities.
    /// </summary>
    private async Task RunLayoutAsync(bool fitGraphBounds = false) {
      if (layoutRunning) {
        return;
      }
      layoutRunning = true;
      SetUIDisabled(true);

      try {
        if (layoutStyle == "hierarchical") {
          var (layout, layoutData) = ConfigureHierarchicalLayout();
          await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(700), layoutData);
        } else {
          var (layout, layoutData) = ConfigureTreeLayout();
          await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(700), layoutData);
        }

        if (fitGraphBounds) {
          await GraphControl.FitGraphBounds();
        }
      } finally {
        layoutRunning = false;
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Returns a configured hierarchical layout considering the edge priorities.
    /// </summary>
    private (HierarchicalLayout layout, HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> layoutData)
        ConfigureHierarchicalLayout() {
      var layout = new HierarchicalLayout {
          DefaultEdgeDescriptor = {
              MinimumFirstSegmentLength = 30,
              MinimumLastSegmentLength = 30
          }
      };

      var layoutData = layout.CreateLayoutData(Graph);
      // Define priorities for edges on critical paths
      layoutData.CriticalEdgePriorities.MapperFunction = edge => GetPriority(edge);

      // Use the edge crossing costs to avoid crossings of different critical paths,
      // when the priority of the edge is high then the probability of crossing is low.
      layoutData.EdgeCrossingCosts.MapperFunction = edge => GetPriority(edge) + 1;

      return (layout, layoutData);
    }

    /// <summary>
    ///   Returns a configured tree layout considering the edge priorities.
    /// </summary>
    private (TreeLayout layout, TreeLayoutData<INode, IEdge, ILabel, ILabel> layoutData) ConfigureTreeLayout() {
      var layout = new TreeLayout();
      var levelAlignedSubtreePlacer = new LevelAlignedSubtreePlacer {
          LayerSpacing = 60,
          Spacing = 30
      };
      layout.DefaultSubtreePlacer = levelAlignedSubtreePlacer;

      var layoutData = layout.CreateLayoutData(Graph);
      // Define priorities for edges on critical paths
      layoutData.CriticalEdgePriorities.MapperFunction = edge => GetPriority(edge);

      return (layout, layoutData);
    }

    /// <summary>
    ///   Marks random upstream paths from leaf nodes to generate random long paths.
    /// </summary>
    private async Task MarkRandomPredecessorsPathsAsync() {
      if (layoutRunning) {
        return;
      }

      var leaves = Graph.Nodes.Where(node => Graph.OutEdgesAt(node).Count == 0).ToArray();

      // clear priorities
      foreach (var edge in Graph.Edges) {
        SetPriority(edge, 0);
      }

      // mark the upstream path of random leaf nodes
      var randomNodeCount = Math.Min(10, leaves.Length);
      while (randomNodeCount > 0) {
        randomNodeCount--;
        var rndNodeIdx = Random.Shared.Next(leaves.Length);
        var rndPriority = Random.Shared.Next(1, 6);
        MarkPredecessorsPath(leaves[rndNodeIdx], rndPriority);
      }

      await RunLayoutAsync();
    }

    /// <summary>
    ///   Marks the upstream path from a given node.
    /// </summary>
    private void MarkPredecessorsPath(INode node, int priority) {
      var incomingEdges = Graph.InEdgesAt(node);
      while (incomingEdges.Count > 0) {
        var edge = incomingEdges[0];
        if (GetPriority(edge) > priority) {
          // stop upstream path when a higher priority is found
          break;
        }
        SetPriority(edge, priority);
        incomingEdges = Graph.InEdgesAt(edge.GetSourceNode());
      }
    }

    /// <summary>
    ///   Clears all edge priorities and reapplies the layout.
    /// </summary>
    private async Task ClearPrioritiesAsync() {
      foreach (var edge in Graph.Edges) {
        SetPriority(edge, 0);
      }

      await RunLayoutAsync();
    }

    /// <summary>
    ///   Initializes a <see cref="GraphViewerInputMode"/> that enables element selection and tool tips.
    /// </summary>
    private void InitializeInputMode() {
      inputMode = new GraphViewerInputMode {
          SelectableItems = GraphItemTypes.Edge | GraphItemTypes.Node,
          ToolTipItems = GraphItemTypes.Edge
      };
      inputMode.QueryItemToolTip += (_, evt) => {
        if (!evt.Handled && evt.Item is IEdge edge) {
          evt.ToolTip = $"Priority: {GetPriority(edge)}";
          evt.Handled = true;
        }
      };
      GraphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Initializes the <see cref="PriorityPanel"/>.
    /// </summary>
    private void InitializePriorityPanel() {
      priorityPanel = new PriorityPanel(GraphControl, priorityCanvas);
      priorityPanel.ItemPriorityChanged += (item, newPriority, _) => {
        if (item is IEdge edge) {
          SetPriority(edge, newPriority);
        } else if (item is INode node) {
          MarkPredecessorsPath(node, newPriority);
        }
        GraphControl.Selection.Clear();
      };
      priorityPanel.PriorityChanged += () => _ = RunLayoutAsync();

      GraphControl.Selection.ItemAdded += (_, evt) => {
        if (evt.Item is INode) {
          priorityPanel.CurrentItems = [.. GraphControl.Selection.Nodes];
        } else if (evt.Item is IEdge) {
          priorityPanel.CurrentItems = [.. GraphControl.Selection.Edges];
        }
      };
      GraphControl.Selection.ItemRemoved += (_, _) => {
        if (GraphControl.Selection.Count == 0) {
          priorityPanel.CurrentItems = null;
        }
      };
    }

    private void InitializeUI() {
      layoutComboBox.SelectionChanged += async (_, _) => {
        layoutStyle = layoutComboBox.SelectedIndex == 0 ? "hierarchical" : "tree";
        LoadGraph(layoutStyle);
        await RunLayoutAsync(true);
      };
    }

    private async void OnRandomPathsClick(object sender, RoutedEventArgs e) {
      await MarkRandomPredecessorsPathsAsync();
    }

    private async void OnClearPrioritiesClick(object sender, RoutedEventArgs e) {
      await ClearPrioritiesAsync();
    }

    private void SetUIDisabled(bool disabled) {
      randomPathsButton.IsEnabled = !disabled;
      clearPrioritiesButton.IsEnabled = !disabled;
      layoutComboBox.IsEnabled = !disabled;
      inputMode.WaitInputMode.Waiting = disabled;
    }

    [STAThread]
    public static void Main() {
      Run<CriticalPathsDemo>();
    }
  }
}
