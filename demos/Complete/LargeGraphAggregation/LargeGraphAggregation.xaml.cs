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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Demo.yFiles.Aggregation;
using yWorks.Algorithms.Util;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Grouping;
using yWorks.Layout.Tree;
using yWorks.Utils;

namespace Demo.yFiles.Complete.LargeGraphAggregation
{
  /// <summary>
  /// Demo window that shows how to use the smart <see cref="NodeAggregation"/> algorithm together with the
  /// <see cref="AggregateGraphWrapper"/> for drill down exploration of a large graph.
  /// </summary>
  public partial class LargeGraphAggregationWindow : INotifyPropertyChanged
  {
    /// <summary>
    /// Style for aggregation nodes that handles aggregated and separated state as well as placeholder nodes.
    /// </summary>
    private static readonly INodeStyle aggregationNodeStyle =
        new NodeControlNodeStyle("AggregationNodeStyleTemplate") { OutlineShape = new Ellipse() };

    /// <summary>
    /// Style for the artificial hierarchy edges.
    /// </summary>
    private static readonly IEdgeStyle hierarchyEdgeStyle = new BezierEdgeStyle
    {
      Pen = (Pen) new Pen { Brush = (Brush) new SolidColorBrush(Color.FromArgb(35, 0, 0, 0)).GetAsFrozen(), DashStyle = DashStyles.Dash }.GetAsFrozen(),
      TargetArrow = new Arrow
      {
        Type = ArrowType.Simple,
        Pen = (Pen) new Pen { Brush = (Brush) new SolidColorBrush(Color.FromArgb(35, 0, 0, 0)).GetAsFrozen() }.GetAsFrozen()
      }
    };

    /// <summary>
    /// Style for the labels for aggregated nodes.
    /// </summary>
    private static readonly ILabelStyle descendantLabelStyle =
        new DefaultLabelStyle { TextSize = 10, TextBrush = (Brush) new SolidColorBrush(Color.FromArgb(150, 75, 75, 75)).GetAsFrozen() };

    /// <summary>
    /// The number of currently visible original nodes (or their placeholders).
    /// </summary>
    public int VisibleNodes {
      get {
        return aggregationHelper?.VisibleNodes ?? 0;
      }
    }

    /// <summary>
    /// The number of currently visible original edges (or their placeholders).
    /// </summary>
    public int VisibleEdges {
      get {
        return aggregationHelper?.VisibleEdges ?? 0;
      }
    }

    /// <summary>
    /// The label of the <see cref="yWorks.Controls.GraphControl.CurrentItem"/>.
    /// </summary>
    public string CurrentItemLabel {
      get {
        return (GraphControl.CurrentItem as ILabelOwner)?.Labels.FirstOrDefault()?.Text.Replace("\n", " ");
      }
    }

    /// <summary>
    /// The Aggregate of the current item.
    /// </summary>
    public yWorks.Analysis.NodeAggregation.Aggregate CurrentItemAggregate {
      get {
        if (aggregationHelper == null || !(GraphControl.CurrentItem is INode)) {
          return null;
        }

        return aggregationHelper.GetAggregateForNode((INode) GraphControl.CurrentItem);
      }
    }

    /// <summary>
    /// The original graph before aggregation.
    /// </summary>
    public IGraph OriginalGraph { get; set; }

    /// <summary>
    /// Manages the NodeAggregation properties panel on the right sight.
    /// </summary>
    private NodeAggregationConfig NodeAggregationConfig;

    /// <summary>
    /// Encapsulates aggregation and separation methods.
    /// </summary>
    private AggregationHelper aggregationHelper;

    public LargeGraphAggregationWindow() {
      InitializeComponent();

      layoutStyleComboBox.ItemsSource = new[] { "Balloon", "Cactus" };
      layoutStyleComboBox.SelectedIndex = 0;
    }

    private async void OnLoaded(object source, EventArgs e) {
      // initialize properties panel
      InitializePropertiesPanel();

      // initialize node click listener that toggles the aggregation status
      InitializeToggleAggregation();

      InitializeHighlight();

      // disable UI
      SetUiEnabled(false);

      // load sample graph
      OriginalGraph = await LoadGraphMlz("Resources/SampleGraph.graphmlz");

      // run smart aggregation algorithm with default settings and set graph to graphControl
      await RunAggregationAndReplaceGraph(OriginalGraph);

      // notify UI
      OnInfoPanelPropertiesChanged();

      // enable UI again
      SetUiEnabled(true);

      layoutStyleComboBox.SelectionChanged += RunAggregation;
    }

    /// <summary>
    /// Enables/disables the UI.
    /// </summary>
    private void SetUiEnabled(bool isEnabled) {
      graphControl.IsEnabled = isEnabled;
      ToolBar.IsEnabled = isEnabled;
      SidePanel.IsEnabled = isEnabled;

      graphLoadingBar.IsIndeterminate = !isEnabled;
      graphLoadingBar.Visibility = isEnabled ? Visibility.Hidden : Visibility.Visible;

      Window.Cursor = isEnabled ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Wait;
      SwitchViewButton.IsEnabled = isEnabled && !graphControl.Graph.Nodes.All(aggregationHelper.AggregateGraph.IsAggregationItem);
    }

    /// <summary>
    /// Asynchronously loads a compressed graphmlz file.
    /// </summary>
    private static async Task<IGraph> LoadGraphMlz(string filepath) {
      return await Task.Run(() => {
        var graph = new DefaultGraph();
        var ioh = new GraphMLIOHandler();
        using (var stream = new GZipStream(File.OpenRead(filepath), CompressionMode.Decompress)) {
          ioh.Read(graph, stream);
        }
        return graph;
      });
    }


    /// <summary>
    /// Initializes the property panel for the <see cref="NodeAggregation"/> settings.
    /// </summary>
    private void InitializePropertiesPanel() {
      NodeAggregationConfig = new NodeAggregationConfig();
      Editor.Configuration = NodeAggregationConfig;
    }

    /// <summary>
    /// Registers a listener to the <see cref="GraphInputMode.ItemClicked"/> event that toggles the aggregation of a node,
    /// runs a layout and sets the current item.
    /// </summary>
    private void InitializeToggleAggregation() {
      graphViewerInputMode.ClickableItems = GraphItemTypes.Node;
      graphViewerInputMode.ItemClicked += async (sender, args) => {
        // prevent default behavior, which would select nodes that are no longer in the graph
        args.Handled = true;

        var node = (INode) args.Item;
        if (!AggregateGraph.IsAggregationItem(node)) {
          // is an original node -> only set current item
          GraphControl.CurrentItem = node;
          OnInfoPanelPropertiesChanged();
          return;
        }

        // toggle the aggregation
        var affectedNodes = aggregationHelper.ToggleAggregation(node);

        // set the current item to the new aggregation node (which is the first in the list)
        GraphControl.CurrentItem = affectedNodes[0];

        // notify UI
        OnInfoPanelPropertiesChanged();

        // run layout
        await RunLayoutOnHierarchyView(affectedNodes);
        SwitchViewButton.IsEnabled = !graphControl.Graph.Nodes.All(aggregationHelper.AggregateGraph.IsAggregationItem);
      };
    }

    #region highlight

    private void InitializeHighlight() {
      // we want to get reports of the mouse being hovered over nodes and edges
      // first enable queries
      graphViewerInputMode.ItemHoverInputMode.Enabled = true;
      // set the items to be reported
      graphViewerInputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Edge | GraphItemTypes.Node;
      // if there are other items (most importantly labels) in front of edges or nodes
      // they should be discarded, rather than be reported as "null"
      graphViewerInputMode.ItemHoverInputMode.DiscardInvalidItems = false;
      // whenever the currently hovered item changes call our method
      graphViewerInputMode.ItemHoverInputMode.HoveredItemChanged += OnHoveredItemChanged;
    }

    private void OnHoveredItemChanged(object sender, HoveredItemChangedEventArgs e) {
      // first remove previous highlights
      graphControl.HighlightIndicatorManager.ClearHighlights();
      // then see where we are hovering over, now
      var newItem = e.Item;
      if (newItem != null) {
        // we highlight the item itself
        var node = newItem as INode;
        var edge = newItem as IEdge;
        if (node != null && aggregationHelper.IsOriginalNodeOrPlaceHolder(node)) {
          AddHighlight(node);
          // and if it's a node, we highlight all adjacent edges, too
          foreach (var adjacentEdge in graphControl.Graph.EdgesAt(node)) {
            if (aggregationHelper.IsHierarchyEdge(adjacentEdge)) {
              continue;
            }
            AddHighlight(adjacentEdge);
            AddHighlight((INode) adjacentEdge.Opposite(node));
          }
        } else if (edge != null && !aggregationHelper.IsHierarchyEdge(edge)) {
          AddHighlight(edge);
          // if it's an edge - we highlight the adjacent nodes
          AddHighlight(edge.GetSourceNode());
          AddHighlight(edge.GetTargetNode());
        }
      }
    }

    private void AddHighlight(INode node) {
      if (aggregationHelper.IsOriginalNodeOrPlaceHolder(node)) {
        graphControl.HighlightIndicatorManager.AddHighlight(node);
      }
    }

    private void AddHighlight(IEdge edge) {
      if (!aggregationHelper.IsHierarchyEdge(edge)) {
        graphControl.HighlightIndicatorManager.AddHighlight(edge);
      }
    }

    private void InitializeHighlightStyles() {
      // we want to create a non-default nice highlight styling
      // for the hover highlight, create semi transparent orange stroke first
      var orangeRed = Colors.Orange;
      var orangePen = new Pen((Brush) new SolidColorBrush(Color.FromArgb(220, orangeRed.R, orangeRed.G, orangeRed.B)).GetAsFrozen(), 3);
      // freeze it for slightly improved performance
      orangePen.Freeze();

      // hide the default selection and focus indicator
      graphControl.SelectionIndicatorManager = new GraphSelectionIndicatorManager() { NodeStyle = VoidNodeStyle.Instance };
      graphControl.FocusIndicatorManager = new GraphFocusIndicatorManager { NodeStyle = VoidNodeStyle.Instance };

      // nodes should be given a rectangular orange rectangle highlight shape
      var highlightShape = new ShapeNodeStyle { Shape = ShapeNodeShape.Ellipse, Pen = orangePen, Brush = null };
      // that should be slightly larger than the real node
      var nodeStyleHighlight = new IndicatorNodeStyleDecorator(highlightShape) { Padding = new InsetsD(5) };

      // a similar style for the edges, however cropped by the highlight's insets
      var dummyCroppingArrow = new Arrow { Type = ArrowType.None, CropLength = 5 };
      var edgeStyle = new BezierEdgeStyle
      {
        Pen = orangePen,
        TargetArrow = dummyCroppingArrow,
        SourceArrow = dummyCroppingArrow,
      };
      var edgeStyleHighlight = new IndicatorEdgeStyleDecorator(edgeStyle) ;
      graphControl.HighlightIndicatorManager = new GraphHighlightIndicatorManager {
          NodeStyle = nodeStyleHighlight, EdgeStyle = edgeStyleHighlight
      };
    }

    #endregion

    /// <summary>
    /// Runs the smart <see cref="NodeAggregation"/> algorithm with the settings from the properties panel.
    /// </summary>
    private async void RunAggregation(object sender, RoutedEventArgs e) {
      SetUiEnabled(false);
      GraphControl.Graph = new DefaultGraph();
      AggregateGraph.Dispose();
      await RunAggregationAndReplaceGraph(OriginalGraph);
      SetUiEnabled(true);

      OnInfoPanelPropertiesChanged();
    }

    /// <summary>
    /// Switches between the view with hierarchy nodes and without and runs an appropriate layout.
    /// </summary>
    private async void SwitchViewButtonClick(object sender, RoutedEventArgs e) {
      if (GraphControl.Graph is AggregateGraphWrapper) {
        SwitchViewButton.Content = "Switch to Hierarchy View";
        GraphControl.Graph = CreateFilteredView();
        await RunCircularLayout();
      } else {
        SwitchViewButton.Content = "Switch to Filtered View";
        GraphControl.Graph = aggregationHelper.AggregateGraph;
        await RunLayoutOnHierarchyView();
      }
    }

    /// <summary>
    /// Switches the visibility of hierarchy edges by setting either the default hierarchy edge
    /// style or a void edge style that does not visualize the edge.
    /// </summary>
    private void SwitchHierarchyEdgeVisibility(IGraph graph, bool visible) {
      foreach (var edge in graph.Edges) {
        if (aggregationHelper.IsHierarchyEdge(edge)) {
          if (visible && edge.Style != hierarchyEdgeStyle) {
            graph.SetStyle(edge, hierarchyEdgeStyle);
          } else if (!visible && edge.Style != VoidEdgeStyle.Instance) {
            graph.SetStyle(edge, VoidEdgeStyle.Instance);
          }
        }
      }
    }

    /// <summary>
    /// Creates a new <see cref="FilteredGraphWrapper"/> that shows the currently visible original nodes.
    /// </summary>
    /// <remarks>
    /// Nodes without currently visible edges are also filtered out.
    /// </remarks>
    private FilteredGraphWrapper CreateFilteredView() {
      // create a new FilteredGraphWrapper that filters the original graph and shows only the currently visible nodes
      var filteredGraph = new FilteredGraphWrapper(
        OriginalGraph,
          node => {
            node = aggregationHelper.GetPlaceholder(node);
            var aggregateGraph = aggregationHelper.AggregateGraph;
            return aggregateGraph.Contains(node);
          }
        );

      // set the node layouts for a smooth transition
      foreach (var node in filteredGraph.Nodes) {
        filteredGraph.SetNodeLayout(node, aggregationHelper.GetPlaceholder(node).Layout.ToRectD());
      }

      // reset any rotated node labels
      foreach (var label in filteredGraph.GetNodeLabels()) {
        filteredGraph.SetLabelLayoutParameter(label, FreeNodeLabelModel.Instance.CreateDefaultParameter());
      }

      return filteredGraph;
    }

    /// <summary>
    /// Creates a new <see cref="AggregateGraphWrapper"/> and runs the aggregation algorithm.
    /// </summary>
    private async Task RunAggregationAndReplaceGraph(IGraph originalGraph) {
      var aggregateGraph = new AggregateGraphWrapper(originalGraph) { EdgeReplacementPolicy = EdgeReplacementPolicy.None };
      await ApplyAggregation(originalGraph, aggregateGraph);
      GraphControl.Graph = aggregateGraph;

      // initializes the highlight styles of the graphControl's current graph
      InitializeHighlightStyles();

      await RunLayoutOnHierarchyView();
    }


    /// <summary>
    /// Asynchronously runs the <see cref="NodeAggregation"/> algorithm with the settings from the properties panel.
    /// </summary>
    /// <remarks>
    /// Afterwards, the <see cref="NodeAggregation.Result"/> is applied to the <paramref name="aggregateGraph"/>.
    /// </remarks>
    private async Task ApplyAggregation(IGraph originalGraph, AggregateGraphWrapper aggregateGraph) {
      await Task.Run(() => {
        var nodeAggregation = NodeAggregationConfig.CreateConfiguredAggregation();
        var aggregationResult = nodeAggregation.Run(originalGraph);

        aggregationHelper = new AggregationHelper(aggregationResult, aggregateGraph)
        {
          AggregationNodeStyle = aggregationNodeStyle,
          HierarchyEdgeStyle = hierarchyEdgeStyle,
          DescendantLabelStyle = descendantLabelStyle
        };
        aggregationHelper.Separate(aggregationHelper.AggregateRecursively(aggregationResult.Root));
      });
    }

    #region layout

    /// <summary>
    /// Runs a layout on the hierarchy view.
    /// </summary>
    private Task RunLayoutOnHierarchyView(IListEnumerable<INode> affectedNodes = null) {
      return "Cactus".Equals(layoutStyleComboBox.SelectedItem)
        ? RunCactusLayout(affectedNodes)
        : RunBalloonLayout(affectedNodes);
    }

    /// <summary>
    /// Runs a balloon layout where the hierarchy edges are the tree edges and original edges are bundled.
    /// </summary>
    private async Task RunBalloonLayout(IListEnumerable<INode> affectedNodes = null) {
      SwitchHierarchyEdgeVisibility(GraphControl.Graph, true);

      // create the balloon layout
      var layout = new BalloonLayout
      {
        IntegratedNodeLabeling = true,
        NodeLabelingPolicy = NodeLabelingPolicy.RayLikeLeaves,
        FromSketchMode = true,
        CompactnessFactor = 0.1,
        AllowOverlaps = true
      };

      // prepend a TreeReduction stage with the hierarchy edges as tree edges
      var treeReductionStage = new TreeReductionStage();
      treeReductionStage.NonTreeEdgeRouter = treeReductionStage.CreateStraightLineRouter();
      treeReductionStage.EdgeBundling.BundlingStrength = 1;
      layout.PrependStage(treeReductionStage);
      var nonTreeEdges = GraphControl.Graph.Edges.Where(e => !aggregationHelper.IsHierarchyEdge(e)).ToList();

      // mark all other edges to be bundled
      var treeReductionStageData = new TreeReductionStageData
      {
          NonTreeEdges = { Items = nonTreeEdges },
          EdgeBundleDescriptors = new ItemMapping<IEdge, EdgeBundleDescriptor>(edge =>
              new EdgeBundleDescriptor { Bundled = nonTreeEdges.Contains(edge), BezierFitting = true })
      };

      // create a layout executor that also zooms to all nodes that were affected by the last operation
      var layoutExecutor =
          new ZoomToNodesLayoutExecutor(affectedNodes ?? ListEnumerable<INode>.Empty, GraphControl, layout)
          {
            Duration = TimeSpan.FromSeconds(0.5),
            AnimateViewport = true,
            EasedAnimation = true,
            LayoutData = treeReductionStageData
          };
      await layoutExecutor.Start();
    }

    private async Task RunCircularLayout() {
      await GraphControl.MorphLayout(
          new CircularLayout { BalloonLayout = { InterleavedMode = InterleavedMode.AllNodes } },
          TimeSpan.FromSeconds(0.5));
    }

    /// <summary>
    /// Runs a {@link CactusGroupLayout} where the hierarchy edges are not shown but the hierarchy is
    /// visualized by placing child nodes along the circular border of the parent node.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The original edges are drawn in a bundled style.
    /// </para>
    /// <para>
    /// The approach uses {@link TemporaryGroupNodeInsertionStage} and some custom layout stage code
    /// to temporarily represent inner tree nodes (which have successors) as group nodes with children.
    /// This is required because the cactus layout works on hierarchical grouping structures as
    /// input and not on tree graph structures.
    /// </para>
    /// </remarks>
    private async Task RunCactusLayout(IListEnumerable<INode> affectedNodes = null) {
      var graph = graphControl.Graph;
      SwitchHierarchyEdgeVisibility(graph, false);

      // collect hierarchy tree nodes that have children, which must temporarily be modeled as a groups
      var innerTreeNodes = new List<INode>();
      var innerTreeNode2Descriptor = new Dictionary<INode, TemporaryGroupDescriptor>();
      foreach (var node in graph.Nodes) {
        if (graph.OutEdgesAt(node).Any(e => aggregationHelper.IsHierarchyEdge(e))) {
          innerTreeNodes.Add(node);
          innerTreeNode2Descriptor[node] = new TemporaryGroupDescriptor();
        }
      }

      // prepare the layout data for the TemporaryGroupNodeInsertionStage
      var tmpGroupStageData = new TemporaryGroupNodeInsertionData();
      foreach (var treeNode in innerTreeNodes) {
        var descriptor = innerTreeNode2Descriptor[treeNode];

        // register a temporary group for each inner tree node...
        var temporaryGroup = tmpGroupStageData.TemporaryGroups.Add(descriptor);
        // ... members of the group are all the successor nodes
        temporaryGroup.Source = graph
          .OutEdgesAt(treeNode)
          .Where(e => aggregationHelper.IsHierarchyEdge(e))
          .Select(e => e.GetTargetNode());

        // if the tree node has a parent too, then the parent descriptor must be setup as well
        var inEdge = graph
          .InEdgesAt(treeNode)
          .Where(e => aggregationHelper.IsHierarchyEdge(e))
          .FirstOrDefault();
        if (inEdge != null) {
          TemporaryGroupDescriptor parentGroupDescriptor;
          if (innerTreeNode2Descriptor.TryGetValue(inEdge.GetSourceNode(), out parentGroupDescriptor)) {
            descriptor.Parent = parentGroupDescriptor;
          }
        }
      }

      // create a mapper that maps from the inner tree node to the temporary group descriptor,
      // which is necessary that the custom layout stages know
      var innerNodesMapper = graph.MapperRegistry.CreateDelegateMapper<INode, TemporaryGroupDescriptor>(
        "INNER_TREE_NODES",
        n => {
          TemporaryGroupDescriptor descriptor;
          if (innerTreeNode2Descriptor.TryGetValue(n, out descriptor)) {
            return descriptor;
          }
          return null;
        }
      );

      // create a layout executor that also zooms to all nodes that were affected by the last operation
      var layoutExecutor = new ZoomToNodesLayoutExecutor(
        affectedNodes ?? ListEnumerable<INode>.Empty,
        graphControl,
        new CustomCactusLayoutStage()
      )
      {
        Duration = TimeSpan.FromSeconds(0.5),
        AnimateViewport = true,
        EasedAnimation = true,
        LayoutData = tmpGroupStageData
      };
      await layoutExecutor.Start();

      // clean-up the mapper registered earlier
      graph.MapperRegistry.RemoveMapper(innerNodesMapper);
    }

    /// <summary>
    /// A layout stage that configures and applies the {@link CactusGroupLayout} and uses further
    /// stages to make the input suitable for it.
    /// </summary>
    private class CustomCactusLayoutStage : LayoutStageBase
    {
      public override void ApplyLayout(LayoutGraph graph) {
        if (graph.NodeCount < 2) {
          // single node or no node: nothing to do for the layout
          return;
        }

        // configure the cactus group layout
        var cactus = new CactusGroupLayout()
        {
          FromSketchMode = true,
          PreferredRootWedge = 360,
          IntegratedNodeLabeling = true,
          NodeLabelingPolicy = NodeLabelingPolicy.RayLikeLeaves
        };
        // ... configure bundling
        cactus.EdgeBundling.DefaultBundleDescriptor.Bundled = true;
        cactus.EdgeBundling.DefaultBundleDescriptor.BezierFitting = true;

        // ... configure the parent-child overlap ratio so that they are allowed to overlap a bit
        graph.AddDataProvider(
          CactusGroupLayout.ParentOverlapRatioDpKey,
          DataProviders.CreateConstantDataProvider(0.5)
        );

        // apply the cactus group layout with temporary groups
        new TemporaryGroupNodeInsertionStage(new TemporaryGroupCustomizationStage(cactus)).ApplyLayout(
          graph
        );

        // clean-up
        graph.RemoveDataProvider(CactusGroupLayout.ParentOverlapRatioDpKey);
      }
    }

    /**
 * A layout stage that prepares the temporary group nodes inserted by
 * {@link TemporaryGroupDescriptor} for the {@link CactusGroupLayout} algorithm.
 */
    private class TemporaryGroupCustomizationStage : LayoutStageBase
    {
      public TemporaryGroupCustomizationStage(CactusGroupLayout cactus) : base(cactus) {
      }

      public override void ApplyLayout(LayoutGraph graph) {
        var innerNodesDp = graph.GetDataProvider("INNER_TREE_NODES");
        var isInsertedTmpGroupDp = graph.GetDataProvider(
          TemporaryGroupNodeInsertionStage.InsertedGroupNodeDpKey
        );
        var childNode2DescriptorDp = graph.GetDataProvider(
          TemporaryGroupNodeInsertionStage.TemporaryGroupDescriptorDpKey
        );
        var grouping = new yWorks.Layout.Grouping.GroupingSupport(graph);

        // collect the temporary group nodes inserted by TemporaryGroupNodeInsertionStage earlier
        // and the respective original tree node
        var temporaryGroups = graph.Nodes
          .Where(n => isInsertedTmpGroupDp.GetBool(n))
          .Select(tmpGroup => {
            var child = grouping.GetChildren(tmpGroup).FirstNode();

            var originalTreeNode = graph.Nodes.First(
              n => innerNodesDp.Get(n) == childNode2DescriptorDp.Get(child)
            );
            return new { Group = tmpGroup, TreeNode = originalTreeNode };
          })
          .ToArray();
        grouping.Dispose();

        // pre-processing: transfer data from original tree node to temporary group node
        foreach (var group in temporaryGroups) {
          var groupNode = group.Group;
          var treeNode = group.TreeNode;

          // transfer sketch (size and location) from the original tree node to the temporary group
          graph.SetSize(groupNode, graph.GetSize(treeNode));
          graph.SetCenter(groupNode, graph.GetCenter(treeNode));

          // change edges such that they connect to the group node (if there are any)
          foreach (var edge in treeNode.Edges.ToArray()) {
            var atSource = edge.Source == treeNode;
            var other = edge.Opposite(treeNode);
            graph.ChangeEdge(edge, atSource ? groupNode : other, atSource ? other : groupNode);
          }
        }

        // now hide all the tree node, they are now modeled by the inserted temporary group nodes
        var treeNodeHider = new LayoutGraphHider(graph);
        foreach (var group in temporaryGroups) {
          treeNodeHider.Hide(group.TreeNode);
        }

        // apply the cactus layout
        base.ApplyLayoutCore(graph);

        // un-hide all the hidden tree nodes
        treeNodeHider.UnhideAll();

        // post-processing: transfer from temporary group node to original tree node
        foreach (var group in temporaryGroups) {
          var groupNode = group.Group;
          var treeNode = group.TreeNode;

          // transfer size and location
          graph.SetSize(treeNode, graph.GetSize(groupNode));
          graph.SetLocation(treeNode, graph.GetLocation(groupNode));

          // change edges such that they again connect to the original tree nodes
          foreach (var edge in groupNode.Edges.ToArray()) {
            var atSource = edge.Source == groupNode;
            var other = edge.Opposite(groupNode);
            graph.ChangeEdge(edge, atSource ? treeNode : other, atSource ? other : treeNode);
          }
        }
      }
    }

    /// <summary>
    /// A LayoutExecutor that modifies the viewport animation to zoom to a list of nodes.
    /// </summary>
    sealed class ZoomToNodesLayoutExecutor : LayoutExecutor
    {
      private readonly IListEnumerable<INode> nodes;

      public ZoomToNodesLayoutExecutor(IListEnumerable<INode> nodes, GraphControl graphControl, ILayoutAlgorithm layout)
          : base(graphControl, layout) {
        this.nodes = nodes;
      }

      protected override IAnimation CreateViewportAnimation(RectD targetBounds) {
        if (nodes.Count == 0) {
          return base.CreateViewportAnimation(targetBounds);
        }

        if (!nodes.All(node => Graph.Contains(node))) {
          throw new InvalidOperationException("Cannot zoom to nodes that are not in the graph");
        }

        var layoutGraph = this.LayoutGraph;
        var bounds = nodes
                     .Select(node => layoutGraph.GetBoundingBox(layoutGraph.GetCopiedNode(node)))
                     .Aggregate(RectD.Empty, (rect1, rect2) => rect1 + rect2.ToRectD());

        var viewportAnimation =
            new ViewportAnimation(this.GraphControl, bounds, this.Duration)
            {
              // The insets are not symmetric since we must compensate the space of the scrollbars
              TargetViewMargins = new InsetsD(20, 20, 36, 36),
              MaximumTargetZoom = 1
            };
        return viewportAnimation;
      }
    }

    #endregion

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    private GraphControl GraphControl {
      get { return graphControl; }
    }

    /// <summary>
    /// Returns the AggregateGraph.
    /// </summary>
    public AggregateGraphWrapper AggregateGraph {
      get { return aggregationHelper?.AggregateGraph; }
    }

    #region INotifyPropertyChanged

    /// <summary>
    /// Updates the info panels.
    /// </summary>
    private void OnInfoPanelPropertiesChanged() {
      OnPropertyChanged("VisibleNodes");
      OnPropertyChanged("VisibleEdges");
      OnPropertyChanged("OriginalGraph");
      OnPropertyChanged("CurrentItemLabel");
      OnPropertyChanged("CurrentItemAggregate");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }

  #region style converters

  /// <summary>
  /// A value converter that converts the <see cref="AggregationNodeInfo"/> in a node's tag to a respective fill color.
  /// </summary>
  /// <remarks>
  /// If the node is a placeholder node, the fill color of this node is used. Otherwise one of the colors in the parameter
  /// is used depending on the <see cref="AggregationNodeInfo.IsAggregated"/> property.
  /// </remarks>
  public class FillConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == null || parameter == null) {
        return null;
      }

      var aggregationInfo = (AggregationNodeInfo) value;
      if (aggregationInfo.Aggregate.Node != null) {
        return ((ShapeNodeStyle) aggregationInfo.Aggregate.Node.Style).Brush;
      }

      var defaultColors = ((string) parameter).Split('|');
      return new BrushConverter().ConvertFrom(aggregationInfo.IsAggregated ? defaultColors[0] : defaultColors[1]);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
  }

  /// <summary>
  /// A value converter that converts the <see cref="AggregationNodeInfo"/> in a node's tag to a respective stroke color.
  /// </summary>
  /// <remarks>
  /// If the node is a placeholder node, the stroke color of this node is used. Otherwise the color in the parameter is
  /// used.
  /// </remarks>
  public class StrokeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == null || parameter == null) {
        return null;
      }

      var aggregationInfo = (AggregationNodeInfo) value;
      if (aggregationInfo.Aggregate.Node != null) {
        return ((ShapeNodeStyle) aggregationInfo.Aggregate.Node.Style).Pen.Brush;
      }

      return new BrushConverter().ConvertFrom(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
  }

  #endregion
}
