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
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Demo.InteractiveHierarchical
{
  /// <summary>
  ///   Sample that interactively demonstrates the usage of <see cref="HierarchicalLayout"/>.
  ///   This demo shows how to incrementally add nodes and edges and dynamically assign port candidates.
  ///   Create new nodes and observe how they are inserted into the drawing near the place they have been created.
  ///   Create new edges and watch the routes being calculated immediately.
  ///   Drag the first and last bend of an edge to interactively assign or reset port candidates.
  ///   Use the context menu to reroute selected edges or optimize selected nodes locations.
  /// </summary>
  [Demo("interactive-hierarchical-layout", "layout", "Interactive Hierarchical Layout",
      Summary =
          "The from sketch mode of the hierarchical layout style can incrementally fit new nodes and edges into the existing drawing.",
      Tags = ["layout", "hierarchical"],
      Keywords = ["layoutfeatures", "ports", "background", "incremental", "from sketch"])]
  public sealed partial class InteractiveHierarchicalLayoutDemo
  {
    private readonly Settings settings = new();
    private GraphEditorInputMode editorInputMode = null!;
    private LayerVisual layerVisual = null!;
    private Dictionary<INode, int> layerMapper = null!;
    private readonly Dictionary<INode, int> newLayerMapper = [];
    private readonly Dictionary<IEdge, EdgePortCandidates> sourcePortCandidates = [];
    private readonly Dictionary<IEdge, EdgePortCandidates> targetPortCandidates = [];
    private readonly HashSet<INode> incrementalNodes = [];
    private readonly HashSet<IEdge> incrementalEdges = [];
    private bool layouting;

    public InteractiveHierarchicalLayoutDemo() {
      InitializeComponent();
      DataContext = settings;
    }

    [STAThread]
    private static void Main() => Run<InteractiveHierarchicalLayoutDemo>();

    protected override async void Initialize() {
      // initialize the input mode
      InitializeInputModes();
      // initialize the graph
      InitializeGraph();

      // then build the graph with the given data set
      BuildGraphFromJson("graph-data.json", graphControl.Graph);

      // calculate the initial layout
      var hierarchicalLayout = new HierarchicalLayout { GroupLayeringPolicy = GroupLayeringPolicy.IgnoreGroups };
      var layoutData = hierarchicalLayout.CreateLayoutData(graphControl.Graph);
      graphControl.Graph.ApplyLayout(hierarchicalLayout, layoutData);

      // and update the layer visualization
      layerMapper = CopyLayerIndices(layoutData.LayerIndicesResult, graphControl.Graph.Nodes);
      layerVisual.UpdateLayers(graphControl.Graph, layerMapper);

      await graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private static Dictionary<INode, int> CopyLayerIndices(IMapper<INode, int?> indices, IEnumerable<INode> nodes) {
      return nodes.ToDictionary(node => node, node => indices[node] ?? 0);
    }

    /// <summary>
    ///   Calls <see cref="CreateEditorMode"/> and registers the result as the graph control's input mode.
    /// </summary>
    private void InitializeInputModes() {
      // create the interaction mode
      editorInputMode = CreateEditorMode();
      graphControl.InputMode = editorInputMode;

      // display the layers
      layerVisual = new LayerVisual();
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, layerVisual);
    }

    /// <summary>
    ///   Creates the default input mode for the <see cref="GraphControl"/> and a
    ///   <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>A specialized <see cref="GraphEditorInputMode"/> instance.</returns>
    private GraphEditorInputMode CreateEditorMode() {
      var mode = new GraphEditorInputMode { ContextMenuItems = GraphItemTypes.Node | GraphItemTypes.Edge };
      // creating bends does not make sense because the routing is calculated
      // immediately after the creation.
      mode.CreateEdgeInputMode.AllowCreateBend = false;

      // register hooks whenever something is dragged or resized
      mode.HandleInputMode.DragFinished += HandleLayoutChanged;
      mode.MoveSelectedItemsInputMode.DragFinished += HandleLayoutChanged;
      mode.MoveUnselectedItemsInputMode.DragFinished += HandleLayoutChanged;
      // ... and when new nodes are created interactively
      mode.NodeCreated += (_, args) => {
        var node = args.Item;
        var newLayer = layerVisual.GetLayer(node.Layout.GetCenter());
        newLayerMapper[node] = newLayer;
        _ = RecalculateLayout();
      };
      // ... or edges
      mode.CreateEdgeInputMode.EdgeCreated += (_, args) => {
        incrementalEdges.Add(args.Item);
        _ = RecalculateLayout();
      };

      // Add an event listener that populates the context menu according to the hit elements, or cancels showing a menu.
      mode.PopulateItemContextMenu += PopulateItemContextMenu;

      mode.SnapContext = new GraphSnapContext {
          SnappableItems = SnappableItems.Node,
          CollectNodePairSnapLines = false
      };

      return mode;
    }

    private void HandleLayoutChanged(object? sender, InputModeEventArgs args) => _ = RecalculateLayout();

    private void PopulateItemContextMenu(object? sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      // see if it's a node but not a not empty group node
      var item = args.Item;
      if (item is INode node && !graphControl.Graph.IsGroupNode(node)) {
        // see if it's already selected
        var selectedNodes = graphControl.Selection.Nodes;
        if (!selectedNodes.Contains(node)) {
          // no - make it the only selected node
          selectedNodes.Clear();
        }
        // make sure the node is selected
        selectedNodes.Add(node);
        graphControl.CurrentItem = node;
        // mark all selected nodes for incremental layout
        var menuItem = new MenuItem { Header = "Reinsert Incrementally" };
        menuItem.Click += (_, _) => {
          incrementalNodes.UnionWith(selectedNodes);
          _ = RecalculateLayout();
        };
        args.Menu.Items.Add(menuItem);
        args.Handled = true;
        args.ShowMenu = true;
      } else if (item is IEdge edge) {
        // update selection state
        var selectedEdges = graphControl.Selection.Edges;
        if (!selectedEdges.Contains(edge)) {
          selectedEdges.Clear();
        }
        selectedEdges.Add(edge);
        graphControl.CurrentItem = edge;
        // and offer option to reroute selected edges
        var menuItem = new MenuItem { Header = "Reroute" };
        menuItem.Click += (_, _) => {
          incrementalEdges.UnionWith(selectedEdges);
          _ = RecalculateLayout();
        };
        args.Menu.Items.Add(menuItem);
        args.Handled = true;
        args.ShowMenu = true;
      }
    }

    /// <summary>
    ///   Core method that recalculates and updates the layout.
    /// </summary>
    private async Task RecalculateLayout() {
      // make sure we are not re-entrant
      if (layouting) {
        return;
      }
      layouting = true;
      // disable the input mode during the layout calculation
      editorInputMode.Enabled = false;

      var movedNodes = new HashSet<INode>(newLayerMapper.Keys);

      // update the layers for moved nodes
      UpdateMovedNodes();

      var layout = new HierarchicalLayout {
          GroupLayeringPolicy = GroupLayeringPolicy.IgnoreGroups,
          FromSketchMode = true
      };
      layout.Core.FixedElementsLayerAssigner = new GivenLayersAssigner();
      var layoutData = layout.CreateLayoutData(graphControl.Graph);
      layoutData.GivenLayersIndices.Dictionary = layerMapper;
      layoutData.Ports.SourcePortCandidates.Dictionary = sourcePortCandidates;
      layoutData.Ports.TargetPortCandidates.Dictionary = targetPortCandidates;
      // specify incremental node hints for nodes
      layoutData.IncrementalNodeHints.MapperFunction = node =>
          incrementalNodes.Contains(node)
              ? IncrementalNodeHint.Incremental
              : GetSelectedNodeHint(movedNodes.Contains(node));
      // specify the edges to rearrange
      layoutData.IncrementalEdges.Items = incrementalEdges;

      // apply the layout
      try {
        await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(1), layoutData);
        layerMapper = CopyLayerIndices(layoutData.LayerIndicesResult, graphControl.Graph.Nodes);
        layerVisual.UpdateLayers(graphControl.Graph, layerMapper);
      } finally {
        layouting = false;
        editorInputMode.Enabled = true;

        // forget the incremental nodes and edges for the next run
        incrementalNodes.Clear();
        incrementalEdges.Clear();
      }
    }

    private IncrementalNodeHint GetSelectedNodeHint(bool moved) =>
        settings.SelectedNodeHint switch {
            NodeHintMode.KeepOrder => IncrementalNodeHint.KeepRelativeOrder,
            NodeHintMode.FromSketch => IncrementalNodeHint.FromSketch,
            NodeHintMode.ExactCoordinates => IncrementalNodeHint.ExactCoordinates,
            NodeHintMode.MovedFromSketchOthersFixed => moved
                ? IncrementalNodeHint.FromSketch
                : IncrementalNodeHint.ExactCoordinates,
            _ => IncrementalNodeHint.FromSketch
        };

    /// <summary>
    ///   Updates the layers for moved nodes.
    /// </summary>
    private void UpdateMovedNodes() {
      if (newLayerMapper.Count > 0) {
        // spread out existing layers
        foreach (var node in graphControl.Graph.Nodes) {
          if (layerMapper.TryGetValue(node, out var layerIndex)) {
            layerMapper[node] = layerIndex * 2;
          }
        }
        foreach (var (node, newLayerIndex) in newLayerMapper) {
          // if a node has been moved, reinsert the adjacent edges incrementally and not from sketch
          incrementalEdges.UnionWith(graphControl.Graph.EdgesAt(node));
          if (newLayerIndex == int.MaxValue) {
            // the node has been dragged outside - mark it as incremental
            incrementalNodes.Add(node);
          } else if (newLayerIndex < 0) {
            var beforeLayer = -(newLayerIndex + 1);
            layerMapper[node] = beforeLayer * 2 - 1;
          } else {
            layerMapper[node] = newLayerIndex * 2;
          }
        }
        newLayerMapper.Clear();
      }
    }

    /// <summary>
    ///   Initializes the graph instance setting default styles
    ///   and creating a small sample graph.
    /// </summary>
    private void InitializeGraph() {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Size = new SizeD(60, 30);

      // set some nice defaults
      DemoStyles.InitDemoStyles(graph, Themes.Palette21, edgeTheme: Themes.Palette21);

      // create mappers for the layers
      layerMapper = new Dictionary<INode, int>();

      // register a custom PositionHandler for the nodes.
      // this enables interactive layer reassignment with layer preview
      graph.GetDecorator().Nodes.PositionHandler.AddWrapperFactory(node => !graph.IsGroupNode(node),
          (node, positionHandler) => positionHandler == null
              ? null
              : new LayerPositionHandler(positionHandler, layerVisual, node, newLayerMapper));

      // register custom handles for the first and last bends of an edge
      // this enables interactive port candidate assignment.
      graph.GetDecorator().Bends.Handle.AddWrapperFactory(
          bend => bend.Owner.Bends.First() == bend || bend.Owner.Bends.Last() == bend,
          (bend, baseHandle) => baseHandle == null
              ? null
              : new PortCandidateBendHandle(baseHandle, bend.Owner.Bends.First() == bend, bend,
                  bend.Owner.Bends.First() == bend ? sourcePortCandidates : targetPortCandidates));
    }
  }
}
