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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Partial;
using yWorks.Utils;
using RoutingStyle = yWorks.Layout.Partial.RoutingStyle;

namespace yWorks.Demo.PartialLayout
{
  /// <summary>
  ///   Shows how to integrate new graph elements into an existing graph layout.
  /// </summary>
  [Demo("partial-layout", "layout", "Partial Layout",
      Summary = "Shows how to integrate new graph elements into an existing graph layout.",
      Tags = ["layout", "incremental", "partial"],
      Keywords = ["layoutfeatures", "hierarchical", "orthogonal", "organic", "circular", "curves"],
      RelatedDemos = ["hierarchical-exact-coordinates"])]
  public sealed partial class PartialLayoutDemo
  {
    private readonly Settings settings = new();
    private DictionaryMapper<INode, bool> partialNodesMapper = new() { DefaultValue = true };
    private DictionaryMapper<IEdge, bool> partialEdgesMapper = new() { DefaultValue = true };

    private GraphEditorInputMode? graphEditorInputMode;
    private FoldingManager foldingManager = null!;
    private IFoldingView foldingView = null!;

    private INodeStyle partialNodeStyle = null!;
    private INodeStyle partialGroupStyle = null!;
    private IEdgeStyle partialEdgeStyle = null!;
    private INodeStyle fixedNodeStyle = null!;
    private INodeStyle fixedGroupNodeStyle = null!;
    private IEdgeStyle fixedEdgeStyle = null!;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public PartialLayoutDemo() {
      InitializeComponent();
      interactionPanel.DataContext = settings;
      settings.PropertyChanged += Settings_PropertyChanged;
    }

    protected override void Initialize() {
      // initialize the GraphControl with folding support
      foldingManager = new FoldingManager();
      foldingView = foldingManager.CreateFoldingView();
      GraphControl.Graph = foldingView.Graph;

      // initialize default styles
      InitializeGraph();

      // initialize interactive behavior
      InitializeInputModes();

      // bind toolbar buttons to actions
      sampleComboBox.SelectedIndex = 0;
      sampleComboBox.SelectionChanged += OnSampleChanged;

      // load the first scenario
      LoadScenario("hierarchical");
    }

    /// <summary>
    ///   Runs a partial layout considering all selected options and partial/fixed nodes.
    /// </summary>
    private async Task RunLayoutAsync() {
      if (settings.IsBusy) {
        return;
      }

      SetUIDisabled(true);

      // configure layout
      var partialLayout = new Layout.Partial.PartialLayout(GetSubgraphLayout()) {
          ComponentAssignmentStrategy = GetComponentAssignmentStrategy(),
          SubgraphPlacement = GetSubgraphPlacement(),
          EdgeRoutingStyle = GetEdgeRoutingStyle(),
          LayoutOrientation = GetLayoutOrientation(),
          MinimumNodeDistance = double.IsNaN(settings.MinimumNodeDistance) ? 0 : settings.MinimumNodeDistance,
          AllowMirroring = settings.AllowMirroring,
          ConsiderNodeAlignment = settings.NodeSnapping
      };

      // mark partial elements for the layout algorithm
      var partialLayoutData = partialLayout.CreateLayoutData(Graph);
      partialLayoutData.Scope.Nodes.Predicate = node => !IsFixed(node);
      partialLayoutData.Scope.Edges.Predicate = edge => !IsFixed(edge);

      // run layout algorithm
      try {
        await GraphControl.ApplyLayoutAnimated(partialLayout, TimeSpan.FromMilliseconds(500), partialLayoutData);
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Retrieves the selected layout for partial components.
    /// </summary>
    private ILayoutAlgorithm GetSubgraphLayout() {
      var distance = double.IsNaN(settings.MinimumNodeDistance) ? 0 : settings.MinimumNodeDistance;
      switch (settings.SubgraphLayout) {
        case SubgraphLayoutKind.Hierarchical:
          return new HierarchicalLayout {
              MinimumLayerDistance = distance,
              NodeDistance = distance
          };
        case SubgraphLayoutKind.Orthogonal:
          return new OrthogonalLayout { GridSpacing = (int) Math.Round(distance) };
        case SubgraphLayoutKind.Organic:
          return new OrganicLayout { DefaultMinimumNodeDistance = distance };
        case SubgraphLayoutKind.Circular:
          var circularLayout = new CircularLayout();
          circularLayout.PartitionDescriptor.MinimumNodeDistance = distance;
          circularLayout.BackboneLayout.MinimumNodeDistance = distance;
          return circularLayout;
        default:
          return new HierarchicalLayout {
              MinimumLayerDistance = distance,
              NodeDistance = distance
          };
      }
    }

    /**
     * Retrieves the assignment strategy, either single nodes or components.
     */
    private ComponentAssignmentStrategy GetComponentAssignmentStrategy() => settings.ComponentAssignment;

    /**
     * Retrieves the positioning strategy, either nodes are placed close to the barycenter of their neighbors or their
     * initial location.
     */
    private SubgraphPlacement GetSubgraphPlacement() => settings.SubgraphPlacement;

    /**
     * Retrieves the edge routing strategy for partial edges and edges connected to partial nodes.
     */
    private RoutingStyle GetEdgeRoutingStyle() => settings.EdgeRoutingStyle;

    /**
     * Retrieves the layout orientation for partial components.
     */
    private PartialLayoutOrientation GetLayoutOrientation() => settings.LayoutOrientation;

    /**
     * Activates folding, sets the defaults for new graph elements and registers mappers
     */
    private void InitializeGraph() {
      // initialize styles
      partialNodeStyle = CreateNodeStyle(true);
      partialGroupStyle = CreateGroupNodeStyle(true);
      partialEdgeStyle = CreateEdgeStyle(true);
      fixedNodeStyle = CreateNodeStyle(false);
      fixedGroupNodeStyle = CreateGroupNodeStyle(false);
      fixedEdgeStyle = CreateEdgeStyle(false);

      var masterGraph = foldingManager.MasterGraph;
      GraphControl.NavigationCommandsEnabled = true;

      masterGraph.NodeDefaults.Size = new SizeD(60, 30);
      masterGraph.NodeDefaults.Style = partialNodeStyle;
      masterGraph.EdgeDefaults.Style = partialEdgeStyle;

      masterGraph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabBackgroundParameter();
      masterGraph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          TextAlignment = TextAlignment.Left,
          TextBrush = Brushes.White
      };
      masterGraph.GroupNodeDefaults.Style = partialGroupStyle;

      // Create and register mappers that specify partial graph elements
      partialNodesMapper.DefaultValue = true;
      partialEdgesMapper.DefaultValue = true;
    }

    /// <summary>Creates a new style instance for nodes in this demo.</summary>
    /// <param name="partial">Whether the node is partial or fixed.</param>
    private static INodeStyle CreateNodeStyle(bool partial) =>
        DemoStyles.CreateDemoNodeStyle(partial ? Themes.PaletteOrange : Themes.Palette58);

    /// <summary>Creates a new style instance for group nodes in this demo.</summary>
    /// <param name="partial">Whether the node is partial or fixed.</param>
    private static INodeStyle CreateGroupNodeStyle(bool partial) =>
        DemoStyles.CreateDemoGroupStyle(partial ? Themes.Palette12 : Themes.Palette58, true);

    /// <summary>Creates a new style instance for edges in this demo.</summary>
    /// <param name="partial">Whether the edge is partial or fixed.</param>
    private static IEdgeStyle CreateEdgeStyle(bool partial) {
      var color = partial ? Color.FromRgb(0xff, 0x6c, 0x00) : Color.FromRgb(0x4d, 0x4d, 0x4d);
      var brush = new SolidColorBrush(color);
      var pen = new Pen(brush, 1.5);
      return new PolylineEdgeStyle {
          Pen = pen,
          TargetArrow = new Arrow(ArrowType.Triangle, brush, pen, 0.75, 0.75, 0, false)
      };
    }

    /**
     * Configures input modes to interact with the graph structure.
     */
    private void InitializeInputModes() {
      graphEditorInputMode = new GraphEditorInputMode { AllowEditLabel = false };
      graphEditorInputMode.ItemDoubleClicked += (_, evt) => {
        // a graph element was double-clicked => toggle its fixed/partial state
        SetFixed(evt.Item, !IsFixed(evt.Item));
      };
      // add a label to newly created nodes and mark the node as non-fixed
      graphEditorInputMode.NodeCreated += (_, evt) => {
        var node = evt.Item;
        if (Graph.IsGroupNode(node)) {
          Graph.AddLabel(node, "Group");
        } else {
          Graph.AddLabel(node, Graph.Nodes.Count.ToString());
        }
        SetFixed(node, false);
      };
      graphEditorInputMode.CreateEdgeInputMode.EdgeCreated += (_, evt) => SetFixed(evt.Item, false);
      graphEditorInputMode.NavigationInputMode.GroupCollapsed += (_, evt) => {
        var group = evt.Item;
        UpdateStyle(group, IsFixed(group));
      };
      graphEditorInputMode.NavigationInputMode.GroupExpanded += (_, evt) => {
        var group = evt.Item;
        UpdateStyle(group, IsFixed(group));
      };
      GraphControl.InputMode = graphEditorInputMode;
    }

    /**
     * Sets the given item as fixed or movable and changes its color to indicate its new state.
     */
    private void SetFixed(IModelItem item, bool fixedItem) {
      var masterItem = GetMasterItem(item);
      if (masterItem is INode node) {
        partialNodesMapper[node] = !fixedItem;
        UpdateStyle(item, fixedItem);
      } else if (masterItem is IEdge edge) {
        partialEdgesMapper[edge] = !fixedItem;
        UpdateStyle(item, fixedItem);
      }
    }

    /**
     * Returns if a given item is considered fixed or shall be rearranged by the layout algorithm.
     * Note that an edge always gets rerouted if any of its end nodes may be moved.
     */
    private bool IsFixed(IModelItem item) {
      var masterItem = GetMasterItem(item);
      if (masterItem is INode node) {
        return !partialNodesMapper[node];
      }
      if (masterItem is IEdge edge) {
        return !partialEdgesMapper[edge];
      }
      return false;
    }

    /**
     * Returns the master item for the given item.
     * Since folding is supported in this demo, partial/fixed states are stored for the master items to stay consistent
     * when expanding/collapsing group nodes.
     */
    private IModelItem? GetMasterItem(IModelItem item) {
      var masterGraph = foldingView.Manager.MasterGraph;
      if (masterGraph.Contains(item)) {
        return item;
      }
      if (Graph.Contains(item)) {
        return foldingView.GetMasterItem(item);
      }
      return null;
    }

    /**
     * Updates the style of the given item when the partial/fixed state has changed.
     */
    private void UpdateStyle(IModelItem item, bool fixedItem) {
      var graph = Graph;
      if (item is INode node) {
        var masterNode = GetMasterItem(node) as INode;
        var isGroupNode = masterNode != null && foldingView.Manager.MasterGraph.IsGroupNode(masterNode);
        graph.SetStyle(node,
            isGroupNode ? fixedItem ? fixedGroupNodeStyle : partialGroupStyle :
            fixedItem ? fixedNodeStyle : partialNodeStyle);
      } else if (item is IEdge edge) {
        graph.SetStyle(edge, fixedItem ? fixedEdgeStyle : partialEdgeStyle);
      }
    }

    /**
     * Updates the partial/fixed state of all graph elements that are currently selected.
     */
    private void SetSelectionFixed(bool fixedItem) {
      foreach (var node in GraphControl.Selection.Nodes) {
        SetFixed(node, fixedItem);
      }
      foreach (var edge in GraphControl.Selection.Edges) {
        SetFixed(edge, fixedItem);
      }
    }

    /**
     * Loads one of four scenarios that come with a sample graph and a layout configuration.
     */
    private void LoadScenario(string sample) {
      partialNodesMapper = new DictionaryMapper<INode, bool> { DefaultValue = true };
      partialEdgesMapper = new DictionaryMapper<IEdge, bool> { DefaultValue = true };

      var ioHandler = new GraphMLIOHandler { ClearGraphBeforeRead = true };
      ioHandler.AddInputMapper("yWorks.Layout.Partial.PartialLayout.NodeScopeDataKey", partialNodesMapper);
      ioHandler.AddInputMapper("yWorks.Layout.Partial.PartialLayout.EdgeScopeDataKey", partialEdgesMapper);

      switch (sample) {
        default:
        case "hierarchical":
          SetOptions(SubgraphLayoutKind.Hierarchical, ComponentAssignmentStrategy.Connected,
              SubgraphPlacement.Barycenter, RoutingStyle.Orthogonal, PartialLayoutOrientation.TopToBottom, 5, true,
              true);
          break;
        case "orthogonal":
          SetOptions(SubgraphLayoutKind.Orthogonal, ComponentAssignmentStrategy.Single, SubgraphPlacement.Barycenter,
              RoutingStyle.Orthogonal, PartialLayoutOrientation.None, 20, false, true);
          break;
        case "organic":
          SetOptions(SubgraphLayoutKind.Organic, ComponentAssignmentStrategy.Single, SubgraphPlacement.Barycenter,
              RoutingStyle.Automatic, PartialLayoutOrientation.None, 30, true, false);
          break;
        case "circular":
          SetOptions(SubgraphLayoutKind.Circular, ComponentAssignmentStrategy.Connected, SubgraphPlacement.Barycenter,
              RoutingStyle.Automatic, PartialLayoutOrientation.None, 10, true, false);
          break;
      }

      using var stream = LoadAssemblyResource($"{sample}.graphml");
      ioHandler.Read(Graph, stream);
      foreach (var node in Graph.Nodes) {
        UpdateStyle(node, IsFixed(node));
      }
      foreach (var edge in Graph.Edges) {
        UpdateStyle(edge, IsFixed(edge));
      }
      GraphControl.FitGraphBounds();
    }

    /**
     * Update the options according to the current scenario.
     */
    private void SetOptions(
        SubgraphLayoutKind subgraphLayout,
        ComponentAssignmentStrategy componentAssignmentStrategy,
        SubgraphPlacement subgraphPlacement,
        RoutingStyle edgeRoutingStrategy,
        PartialLayoutOrientation layoutOrientation,
        double minimumNodeDistance,
        bool allowMirroring,
        bool nodeSnapping) {
      settings.SetOptions(subgraphLayout, componentAssignmentStrategy, subgraphPlacement, edgeRoutingStrategy,
          layoutOrientation, minimumNodeDistance, allowMirroring, nodeSnapping);
    }

    /**
     * Enables/disables the toolbar and interaction panel during layout calculation.
     */
    private void SetUIDisabled(bool disabled) {
      settings.IsBusy = disabled;
      if (graphEditorInputMode is not null) {
        graphEditorInputMode.WaitInputMode.Waiting = disabled;
      }
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof(Settings.IsBusy)) {
        interactionPanel.IsEnabled = !settings.IsBusy;
        demoToolBar.IsEnabled = !settings.IsBusy;
      }
    }

    private void OnLockSelection(object sender, RoutedEventArgs e) => SetSelectionFixed(true);

    private void OnUnlockSelection(object sender, RoutedEventArgs e) => SetSelectionFixed(false);

    private void OnRefresh(object sender, RoutedEventArgs e) => LoadScenario(GetSelectedSample());

    private void OnSampleChanged(object sender, SelectionChangedEventArgs e) => LoadScenario(GetSelectedSample());

    private async void OnLayout(object sender, RoutedEventArgs e) => await RunLayoutAsync();

    private string GetSelectedSample() => sampleComboBox.SelectedValue as string ?? "hierarchical";

    [STAThread]
    private static void Main() => Run<PartialLayoutDemo>();
  }
}
