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
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Tree;

namespace yWorks.Demo.Collapse;

internal sealed record TreeNodeTag(int Level, bool Collapsed);

/// <summary>
///   The Collapse demo shows interactive collapsing and expanding of subtrees of a graph.
///   Using the dedicated helper classes for filtering-out parts of a graph and the tree layout
///   algorithm, implementing such an interactive application is easy.
/// </summary>
[Demo("collapsible-trees", "view", "Collapsible Trees",
    Summary = "Shows interactive collapsing and expanding of subtrees of a graph.",
    Tags = ["layout", "interaction", "animation"],
    Keywords = ["hierarchical", "organic", "tree", "balloon", "radial", "filtering", "hide", "collapse"])]
public sealed partial class CollapseDemo
{
  public static readonly RoutedUICommand ToggleChildrenCommand =
      new("Toggle children", nameof(ToggleChildrenCommand), typeof(CollapseDemo));

  /// <summary>
  ///   Utilities for collapsing and expanding nodes.
  /// </summary>
  private CollapseAndExpandNodes collapseAndExpandNodes = null!;

  /// <summary>
  ///   Indicates whether a layout is currently in calculation.
  /// </summary>
  public bool RunningLayout { get; set; }

  /// <summary>
  ///   Runs the demo.
  /// </summary>
  protected override void Initialize() {
    collapseAndExpandNodes = new CollapseAndExpandNodes(graphControl);

    ConfigureUserInteraction();

    graphControl.Graph = CreateGraph(collapseAndExpandNodes);
    graphControl.FitGraphBounds();

    InitializeUI(layoutComboBox, RunLayout);

    _ = RunLayout(null, false);
  }

  /// <summary>
  ///   Creates a filtered graph that displays nodes depending on the corresponding node visibility
  ///   of the demo's collapseAndExpandNodes utility.
  /// </summary>
  /// <returns>a filtered graph.</returns>
  private static FilteredGraphWrapper CreateGraph(CollapseAndExpandNodes collapseAndExpandNodes) {
    // create the graph instance that will hold the complete graph
    var completeGraph = new DefaultGraph();

    // create a new style for inner nodes
    completeGraph.NodeDefaults.Style = new NodeControlNodeStyle("InnerNodeStyleTemplate");
    completeGraph.NodeDefaults.Size = new SizeD(60, 30);
    completeGraph.NodeDefaults.ShareStyleInstance = false;
    completeGraph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Bottom;

    // set a default edge style
    completeGraph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle();

    BuildTree(completeGraph, 5);

    foreach (var node in completeGraph.Nodes) {
      var nodeTag = (TreeNodeTag) node.Tag!;
      // initially, 3 levels are expanded and thus, 4 levels are visible
      node.Tag = nodeTag with { Collapsed = nodeTag.Level > 2 };
      collapseAndExpandNodes.SetCollapsed(node, nodeTag.Level > 2);
      collapseAndExpandNodes.SetNodeVisibility(node, nodeTag.Level < 4);

      // set a different style for inner and leaf nodes
      if (!completeGraph.OutEdgesAt(node).Any()) {
        completeGraph.SetStyle(node, new NodeControlNodeStyle("LeafNodeStyleTemplate"));
      } else {
        completeGraph.SetStyle(node,
            new NodeControlNodeStyle("InnerNodeStyleTemplate") { StyleTag = nodeTag.Level > 2 });
      }
    }

    // Create a filtered graph of the original graph that contains only non-collapsed sub-parts.
    // The predicate methods specify which should be part of the filtered graph.
    return new FilteredGraphWrapper(completeGraph, collapseAndExpandNodes.GetNodeVisibility, _ => true);
  }

  /// <summary>
  ///   Configures user interaction for the demo.
  ///   User interaction is restricted to panning and zooming; the node controls handle collapsing and
  ///   expanding.
  /// </summary>
  private void ConfigureUserInteraction() {
    var graphViewerInputMode = new GraphViewerInputMode {
        SelectableItems = GraphItemTypes.None,
        ClickableItems = GraphItemTypes.None
    };

    graphControl.InputMode = graphViewerInputMode;
    graphControl.SelectionIndicatorManager.Enabled = false;
    graphControl.FocusIndicatorManager.Enabled = false;
    graphControl.HighlightIndicatorManager.Enabled = false;
  }

  private async void ToggleChildrenExecuted(object sender, ExecutedRoutedEventArgs e) {
    if (e.Parameter is not INode node) {
      return;
    }

    e.Handled = true;
    var filteredGraph = (FilteredGraphWrapper) graphControl.Graph;
    if (((TreeNodeTag) node.Tag!).Collapsed) {
      collapseAndExpandNodes.Expand(node);
      filteredGraph.NodePredicateChanged();
      SetNodeCollapsedState(node, false);
      await RunLayout(node, true);
    } else {
      collapseAndExpandNodes.Collapse(node);
      await RunLayout(node, false);
      filteredGraph.NodePredicateChanged();
      SetNodeCollapsedState(node, true);
    }
  }

  private static void SetNodeCollapsedState(INode node, bool collapsed) {
    node.Tag = (TreeNodeTag) node.Tag! with { Collapsed = collapsed };
    ((NodeControlNodeStyle) node.Style).StyleTag = collapsed;
  }

  /// <summary>
  ///   Creates a new radial tree layout algorithm instance.
  /// </summary>
  private static RadialTreeLayout NewRadialTreeLayout() =>
      new() {
          ChildOrderingPolicy = ChildOrderingPolicy.FromSketch,
          AllowOverlaps = true
      };

  /// <summary>
  ///   Creates a new organic layout algorithm instance.
  /// </summary>
  private static OrganicLayout NewOrganicLayout() =>
      new() {
          DefaultMinimumNodeDistance = 100,
          DefaultPreferredEdgeLength = 80,
          Deterministic = true,
          AllowNodeOverlaps = true
      };

  /// <summary>
  ///   Creates a new tree layout algorithm instance.
  /// </summary>
  private static TreeLayout NewTreeLayout() => new();

  /// <summary>
  ///   Creates a new hierarchical layout algorithm instance.
  /// </summary>
  private static HierarchicalLayout NewHierarchicalLayout() => new();

  /// <summary>
  ///   Creates a new layout algorithm instance.
  /// </summary>
  /// <param name="algorithmName">the name of the layout algorithm that will be created.</param>
  private static ILayoutAlgorithm NewLayoutAlgorithm(string algorithmName) {
    ILayoutAlgorithm algorithm = algorithmName switch {
        "radial-tree" => NewRadialTreeLayout(),
        "organic" => NewOrganicLayout(),
        "hierarchical" => NewHierarchicalLayout(),
        _ => NewTreeLayout()
    };

    // For a nice layout animation, we use PlaceNodesAtBarycenterStage to make sure new nodes
    // appear at the position of their parent
    var layoutStages = algorithm switch {
        TreeLayout treeLayout => treeLayout.LayoutStages,
        RadialTreeLayout radialTreeLayout => radialTreeLayout.LayoutStages,
        OrganicLayout organicLayout => organicLayout.LayoutStages,
        HierarchicalLayout hierarchicalLayout => hierarchicalLayout.LayoutStages,
        _ => throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, "Unsupported layout algorithm")
    };
    layoutStages.Prepend(new PlaceNodesAtBarycenterStage());
    layoutStages.Prepend(new LayoutAnchoringStage());
    return algorithm;
  }

  /// <summary>
  ///   Applies a new layout to the current graph.
  /// </summary>
  /// <param name="toggledNode">
  ///   The children of this node are laid out as incremental items.
  ///   Without a toggled node, a 'from scratch' layout is calculated.
  /// </param>
  /// <param name="expand">Whether this is part of an expand or a collapse action.</param>
  private async Task RunLayout(INode? toggledNode, bool expand) {
    if (RunningLayout) {
      return;
    }
    RunningLayout = true;

    layoutComboBox.IsEnabled = false;
    try {
      var currentLayout = NewLayoutAlgorithm(GetSelectedLayoutName(layoutComboBox));
      var currentLayoutData = graphControl.Graph.CreateCompositeLayoutData();

      collapseAndExpandNodes.ConfigureLayout(toggledNode, expand, currentLayoutData, currentLayout);

      await graphControl.ApplyLayoutAnimated(currentLayout, TimeSpan.FromMilliseconds(300), currentLayoutData,
          cancellationToken: default);
    } finally {
      RunningLayout = false;
      layoutComboBox.IsEnabled = true;
    }
  }

  /// <summary>
  ///   Builds a sample tree graph.
  /// </summary>
  private static void BuildTree(IGraph graph, int levelCount) {
    var root = graph.CreateNode(new RectD(0, 0, 60, 30), tag: new TreeNodeTag(0, true));
    AddChildren(graph, root, 3, levelCount);
  }

  /// <summary>
  ///   Adds children to the given root node.
  /// </summary>
  private static void AddChildren(IGraph graph, INode root, int childrenCount, int levelCount) {
    var level = ((TreeNodeTag) root.Tag!).Level + 1;
    if (level >= levelCount) {
      return;
    }
    for (var i = 0; i < childrenCount; ++i) {
      var child = graph.CreateNode(new RectD(0, 0, 60, 30), tag: new TreeNodeTag(level, true));
      graph.CreateEdge(root, child);
      AddChildren(graph, child, Random.Shared.Next(1, 5), levelCount);
    }
  }

  /// <summary>
  ///   Binds actions to the buttons in the demo's toolbar.
  /// </summary>
  private static void InitializeUI(ComboBox layoutComboBox, Func<INode?, bool, Task> runLayout) {
    layoutComboBox.SelectionChanged += async (_, _) => await runLayout(null, false);
  }

  private static string GetSelectedLayoutName(ComboBox layoutComboBox) =>
      ((ComboBoxItem) layoutComboBox.SelectedItem).Tag?.ToString() ?? "tree";

  [STAThread]
  private static void Main() => Run<CollapseDemo>();

  public CollapseDemo() {
    InitializeComponent();
  }
}
