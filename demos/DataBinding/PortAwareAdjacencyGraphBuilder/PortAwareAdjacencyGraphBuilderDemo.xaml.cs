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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.DataBinding.PortAwareAdjacencyGraphBuilder
{
  /// <summary>
  ///   This demo shows how to automatically build a graph from business data using
  ///   a customized AdjacencyGraphBuilder which creates node ports based on the node data and
  ///   connects the edges to those ports.
  ///   It also uses AdjacencyGraphBuilder's UpdateGraph method to modify the existing graph
  ///   to reflect changes in the business data. Also, an incremental layout is applied to arrange
  ///   new elements while keeping the location of the unchanged items as stable as possible.
  /// </summary>
  [Demo("port-aware-adjacency-graph-builder", "data-binding", "Port-aware Adjacency Graph Builder",
      Summary = "Builds a graph using AdjacencyGraphBuilder and connects the items to specific ports.",
      Tags = ["json", "data binding", "layout"], Keywords = ["hierarchical", "ports"], RelatedDemos = [])]
  public sealed partial class PortAwareAdjacencyGraphBuilderDemo
  {
    private AdjacencyGraphBuilder builder = null!;
    private GraphData graphData = null!;
    private AdjacencyNodesSource<NodeData> adjacencyNodesSource = null!;
    private int updateIndex;

    protected override async void Initialize() {
      SetGraphDefaults(graphControl.Graph);

      // use the viewer input mode since this demo should not allow interactive graph editing
      graphControl.InputMode = new GraphViewerInputMode();

      // build the graph from data
      graphData = ReadGraphData();
      builder = CreatePortAwareAdjacencyGraphBuilder(graphControl.Graph, graphData.NodesSource);
      builder.BuildGraph();

      // center graph in the visible area
      await graphControl.FitGraphBounds();

      // arrange the graph using a hierarchical layout algorithm
      await ArrangeGraphAsync(graphControl, [.. graphControl.Graph.Nodes]);
    }

    /// <summary>
    ///   Updates the graph. This reflects changes in the business data while keeping the unchanged items.
    ///   This function uses AdjacencyGraphBuilder's UpdateGraph method to modify the existing graph
    ///   instead of building it anew. After that, it arranges the item in incremental mode
    ///   which keeps the unchanged items as stable as possible.
    /// </summary>
    private async Task UpdateGraphAsync(GraphControl graphControl, List<NodeData> nodesSource) {
      var graph = graphControl.Graph;

      // determine which nodes were added while updating the graph
      var newNodes = new List<INode>();
      EventHandler<GraphBuilderItemEventArgs<INode, object>> nodeCreatedListener = (_, evt) => newNodes.Add(evt.Item);
      builder.NodeCreated += nodeCreatedListener;

      // update the graph according the new (but related) data
      // this will remove nodes whose IDs are not in the new data set
      // this will add nodes whose IDs are in the new data set, but are not in the old one
      builder.SetData(adjacencyNodesSource, nodesSource);
      builder.UpdateGraph();

      builder.NodeCreated -= nodeCreatedListener;

      // hide the new items (i.e. the new nodes, the edges connected to the new nodes, their labels
      // and their ports) during the animated layout calculation
      GraphItemsHider.HideNodesAndRelatedItems(graph, newNodes);

      // arrange the graph: arrange the new nodes while keeping the other nodes as stable as possible
      await ArrangeGraphAsync(graphControl, newNodes);

      // after the layout animation has finished, show the previously hidden items
      // this way new items do not seem to be affected by the layout calculation
      // otherwise, new items would appear at the default location (0,0) and then move to their
      // final location during the layout animation
      GraphItemsHider.ShowNodesAndRelatedItems(graph, newNodes);
    }

    /// <summary>
    ///   Arranges the graph of the given graph component and applies the new layout in an animated
    ///   fashion.
    /// </summary>
    private async Task ArrangeGraphAsync(GraphControl graphControl, ICollection<INode> newNodes) {
      updateButton.IsEnabled = false;
      try {
        var graph = graphControl.Graph;
        // if there are less new nodes than there are nodes in total, calculate an incremental layout
        // i.e., try to keep the positions of the "old" nodes while finding good positions for new nodes
        var arrangeIncrementally = newNodes.Count < graph.Nodes.Count;

        var algorithm = new HierarchicalLayout {
            LayoutOrientation = LayoutOrientation.LeftToRight,
            MinimumLayerDistance = 50,
            FromSketchMode = arrangeIncrementally
        };
        var edgeDescriptor = algorithm.DefaultEdgeDescriptor;
        edgeDescriptor.MinimumFirstSegmentLength = 30;
        edgeDescriptor.MinimumLastSegmentLength = 30;

        var hierarchicalLayoutData = algorithm.CreateLayoutData(graph);
        // specify which nodes are the "new" nodes in the case of an incremental layout calculation
        // i.e. for which nodes the algorithm needs to calculate layer assignment and sequence order
        // for "old" nodes, the algorithm will determine layer and sequence from their current positions
        if (arrangeIncrementally) {
          hierarchicalLayoutData.IncrementalNodes.Items = newNodes;
        }

        // arrange the graph with the chosen layout
        await new LayoutExecutor(graphControl, algorithm) {
            LayoutData = hierarchicalLayoutData,
            PortPlacementPolicies = { Constant = PortPlacementPolicy.KeepParameter },
            AnimateViewport = true,
            AnimationDuration = TimeSpan.FromSeconds(0.5)
        }.Start();
      } finally {
        updateButton.IsEnabled = true;
      }
    }

    /// <summary>
    ///   Initializes style defaults for the graph items.
    /// </summary>
    private static void SetGraphDefaults(IGraph graph) {
      graph.NodeDefaults.Size = new SizeD(100, 160);
      graph.NodeDefaults.Labels.LayoutParameter =
          new InteriorNodeLabelModel { Padding = new InsetsD(5) }.CreateParameter(
              InteriorNodeLabelModel.Position.Bottom);
      graph.NodeDefaults.Labels.ShareLayoutParameterInstance = true;

      // we want to keep the ports
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      graph.NodeDefaults.Ports.Labels.LayoutParameter =
          new InsideOutsidePortLabelModel { Distance = 5 }.CreateInsideParameter();

      var edgeStyle = DemoStyles.CreateDemoEdgeStyle(Themes.PaletteOrange, false, 0);
      edgeStyle.Pen = new Pen(Themes.PaletteOrange.Stroke, 2);
      graph.EdgeDefaults.Style = edgeStyle;
    }

    /// <summary>
    ///   Creates a new instance of a port-aware AdjacencyGraphBuilder.
    ///   The builder uses the given node source.
    /// </summary>
    /// <param name="graph">The graph the builder works on</param>
    /// <param name="nodesSourceData">The data to build the graph from</param>
    private AdjacencyGraphBuilder CreatePortAwareAdjacencyGraphBuilder(IGraph graph, List<NodeData> nodesSourceData) {
      var adjacencyGraphBuilder = new AdjacencyGraphBuilder(graph);

      // Use the port-aware node creator for all node sources
      var nodeCreator = new PortAwareNodeCreator { Defaults = graph.NodeDefaults };
      var labelCreator = nodeCreator.CreateLabelBinding(nodeData => nodeData.Name);
      ConfigureStyles(nodeCreator, labelCreator);

      var nodesSource = adjacencyGraphBuilder.CreateNodesSource(nodesSourceData, nodeData => nodeData.Id);
      nodesSource.NodeCreator = nodeCreator;
      // Keep the source so UpdateGraph can replace its data collection.
      var successorEdgeCreator = new PortAwareEdgeCreator(true, graph.EdgeDefaults, adjacencyGraphBuilder);
      var successorsSource = nodesSource.CreateSuccessorsSource(nodeData => nodeData.Successors ?? [],
          successorEdgeCreator, nodeData => nodeData.Id);
      successorsSource.NodeCreator = nodeCreator;

      // Same for the predecessors source
      var predecessorEdgeCreator = new PortAwareEdgeCreator(false, graph.EdgeDefaults, adjacencyGraphBuilder);
      var predecessorsSource = nodesSource.CreatePredecessorsSource(nodeData => nodeData.Predecessors ?? [],
          predecessorEdgeCreator, nodeData => nodeData.Id);
      predecessorsSource.NodeCreator = nodeCreator;

      // Add recursive successor and predecessor sources
      successorsSource.AddSuccessorsSource(nodeData => nodeData.Successors ?? [], successorsSource,
          successorEdgeCreator);
      successorsSource.AddPredecessorsSource(nodeData => nodeData.Predecessors ?? [], predecessorsSource,
          predecessorEdgeCreator);
      predecessorsSource.AddSuccessorsSource(nodeData => nodeData.Successors ?? [], successorsSource,
          successorEdgeCreator);
      predecessorsSource.AddPredecessorsSource(nodeData => nodeData.Predecessors ?? [], predecessorsSource,
          predecessorEdgeCreator);

      // Save the source used by setData to update this graph later.
      adjacencyNodesSource = nodesSource;
      return adjacencyGraphBuilder;
    }

    /// <summary>
    ///   Demo specific: configures the styles to match the theme.
    /// </summary>
    private static void ConfigureStyles(NodeCreator<NodeData> nodeCreator, LabelCreator<NodeData> labelCreator) {
      nodeCreator.StyleProvider = CreateNodeStyle;
      labelCreator.StyleProvider = CreateLabelStyle;
    }

    /// <summary>
    ///   Demo specific: configures the node style to match the theme
    ///   according to the optional color property of the node data.
    /// </summary>
    private static INodeStyle CreateNodeStyle(NodeData dataItem) =>
        DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.RoundRectangle, GetPalette(dataItem));

    private static INodeStyle CreatePortNodeStyle(NodeData? dataItem) =>
        DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Rectangle, GetPalette(dataItem));

    private static Palette GetPalette(NodeData? dataItem) =>
        dataItem?.Color == "blue" ? Themes.PaletteLightblue : Themes.PaletteOrange;

    /// <summary>
    ///   Demo specific: configures the label style to match the theme
    ///   according to the optional color property of the node data.
    /// </summary>
    private static ILabelStyle CreateLabelStyle(NodeData dataItem) => CreateLabelStyle(dataItem, false);

    private static ILabelStyle CreateLabelStyle(NodeData? dataItem, bool forPortLabels) {
      var palette = GetPalette(dataItem);
      return new LabelStyle {
          TextBrush = palette.Text,
          BackgroundBrush = forPortLabels ? null : palette.NodeLabelFill,
          Padding = new InsetsD(forPortLabels ? 1 : 5),
          Shape = forPortLabels ? LabelShape.Rectangle : LabelShape.RoundRectangle
      };
    }

    /// <summary>
    ///   Reads the initial graph and update data from the embedded JSON resource.
    /// </summary>
    private GraphData ReadGraphData() {
      using var jsonStream = LoadAssemblyResource("graph-builder-data.json");
      return JsonSerializer.Deserialize<GraphData>(jsonStream,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    /// <summary>
    ///   Binds the toolbar button to its functionality.
    /// </summary>
    private async void UpdateButton_OnClick(object sender, RoutedEventArgs e) {
      // build graph from new data
      var data = ++updateIndex % 2 == 1 ? graphData.UpdateNodesSource : graphData.NodesSource;
      await UpdateGraphAsync(graphControl, data);
    }

    public PortAwareAdjacencyGraphBuilderDemo() {
      InitializeComponent();
    }

    [STAThread]
    public static void Main() => Run<PortAwareAdjacencyGraphBuilderDemo>();

    public sealed class GraphData
    {
      public List<NodeData> NodesSource { get; set; } = [];

      public List<NodeData> UpdateNodesSource { get; set; } = [];
    }

    public sealed class NodeData
    {
      /// <summary>
      ///   the ID for the node, which must be unique for all nodes in the graph
      /// </summary>
      public string Id { get; set; } = string.Empty;

      /// <summary>
      ///   the name of the node, to be used as label
      /// </summary>
      public string Name { get; set; } = string.Empty;

      /// <summary>
      ///   the node ports
      /// </summary>
      public List<PortData>? Ports { get; set; }

      /// <summary>
      ///   the ID of the port to connect at the parent
      /// </summary>
      public string? From { get; set; }

      /// <summary>
      ///   the ID of the port to connect at this node
      /// </summary>
      public string? To { get; set; }

      /// <summary>
      ///   the predecessor nodes of this node
      /// </summary>
      public List<NodeData>? Predecessors { get; set; }

      /// <summary>
      ///   the successor nodes of this node
      /// </summary>
      public List<NodeData>? Successors { get; set; }

      /// <summary>
      ///   an optional color schema
      /// </summary>
      public string? Color { get; set; }
    }

    public sealed class PortData
    {
      /// <summary>
      ///   the ID for the port, which must be unique for each owner node
      /// </summary>
      public string Id { get; set; } = string.Empty;

      /// <summary>
      ///   the port location relative to the node layout, x and y values are a ratio between 0 and 1
      /// </summary>
      public double[] Location { get; set; } = [];
    }

    /// <summary>
    ///   A node creator which adds ports as supplied with the node data.
    ///   Overrides CreateNodeCore to add the ports.
    /// </summary>
    private sealed class PortAwareNodeCreator : NodeCreator<NodeData>
    {
      /// <summary>
      ///   Overrides the original node creator to add ports.
      /// </summary>
      protected override INode CreateNodeCore(
          IGraph graph,
          bool groupNode,
          INode? parent,
          RectD layout,
          INodeStyle style,
          object? tag) {
        // this assumes that we don't use a custom tag provider,
        // i.e. the tag contains the node data as provided by the node source array
        var nodeData = tag as NodeData ?? throw new InvalidOperationException("Expected node data as the node tag.");

        // let the base implementation create the node
        var node = base.CreateNodeCore(graph, groupNode, parent, layout, style, tag);

        // add ports
        var ports = GetPorts(nodeData);
        if (ports is not null) {
          foreach (var portData in ports) {
            AddPort(graph, node, portData);
          }
        }
        return node;
      }

      /// <summary>
      ///   Adds a port to the given node.
      ///   The port can be identified by its ID which is set to its tag.
      /// </summary>
      /// <param name="graph">The graph to operate on.</param>
      /// <param name="node">The node to add the port to.</param>
      /// <param name="portData">The port data to create the port for.</param>
      private void AddPort(IGraph graph, INode node, PortData portData) {
        var port = graph.AddPort(node, FreeNodePortLocationModel.Instance.CreateParameter(GetPortLocation(portData)),
            new NodeStylePortStyleAdapter(CreatePortNodeStyle(node.Tag as NodeData)) { RenderSize = new SizeD(8, 8) },
            GetPortId(portData));
        AddPortLabel(graph, port, portData);
      }

      /// <summary>
      ///   Adds a label to the given port.
      /// </summary>
      /// <param name="graph">The graph to operate on.</param>
      /// <param name="port">The port to add the label to.</param>
      /// <param name="portData">The port data to create the label for.</param>
      private static void AddPortLabel(IGraph graph, IPort port, PortData portData) {
        var text = GetPortLabel(portData);
        if (text is not null) {
          graph.AddLabel(port, text, GetParameter(portData), CreateLabelStyle(port.Owner.Tag as NodeData, true));
        }
      }

      /// <summary>
      ///   Updates a given node. This function is called for nodes which already exist in the graph.
      ///   The method itself updates the ports. The actual node update is delegated to the base method.
      /// </summary>
      /// <param name="graph">The graph to operate on.</param>
      /// <param name="node">The node to update.</param>
      /// <param name="parent">The parent node.</param>
      /// <param name="dataItem">The new data item.</param>
      public override void UpdateNode(IGraph graph, INode node, INode? parent, NodeData dataItem) {
        base.UpdateNode(graph, node, parent, dataItem);
        var index = 0;
        var portsToRemove = new List<IPort>();
        var ports = GetPorts(dataItem);
        // synchronize the ports according to the port data:
        // 1. update existing ports for which port data also exists
        //     with ID, location, and label according to the data
        // 2. remove ports for which no data exists
        // 3. add ports for new port data
        foreach (var port in node.Ports) {
          if (ports is not null && index < ports.Count) {
            // update each existing port for which a port data item exists with the new port data
            var portData = ports[index++];
            // update tag and location
            port.Tag = GetPortId(portData);
            graph.SetPortLocationParameter(port,
                FreeNodePortLocationModel.Instance.CreateParameter(GetPortLocation(portData)));
            // if the port has a label update it, otherwise add one
            var oldLabel = port.Labels.FirstOrDefault();
            if (oldLabel is not null) {
              var text = GetPortLabel(portData);
              if (text is not null) {
                graph.SetLabelText(oldLabel, text);
                graph.SetLabelLayoutParameter(oldLabel, GetParameter(portData));
              } else {
                graph.Remove(oldLabel);
              }
            } else {
              AddPortLabel(graph, port, portData);
            }
          } else {
            // mark existing ports for which no port data exists to be removed
            portsToRemove.Add(port);
          }
        }
        // actually remove the marked ports
        if (portsToRemove.Count > 0) {
          foreach (var port in portsToRemove) {
            graph.Remove(port);
          }
        } else if (ports is not null) {
          // if there is port data left, add ports for it
          for (var i = index; i < ports.Count; i++) {
            AddPort(graph, node, ports[i]);
          }
        }
      }

      /* *****************************************************
       * Developers who want to use this adjacency graph builder
       * in their own applications can modify the following
       * methods to adapt to different NodeData and
       * PortData definitions.
       * *****************************************************/

      /// <summary>
      ///   Gets the label placement according to the port data.
      /// </summary>
      private static ILabelModelParameter GetParameter(PortData portData) {
        // whether the port is left or right
        var right = GetPortLocation(portData).X > 0.5;
        return FreePortLabelModel.Instance.CreateParameter(new PointD(right ? -8 : 8, 0),
            new PointD(right ? 1 : 0, 0.5));
      }

      /// <summary>
      ///   Gets the label text to set from the port data.
      ///   Returns null if no label should be set.
      /// </summary>
      private static string? GetPortLabel(PortData portData) => portData.Id;

      /// <summary>
      ///   Gets an array of PortData from the given node data.
      ///   Might be null.
      /// </summary>
      private static List<PortData>? GetPorts(NodeData nodeData) => nodeData.Ports;

      /// <summary>
      ///   Gets the (relative) port location from the port data.
      /// </summary>
      private static PointD GetPortLocation(PortData portData) => new(portData.Location[0], portData.Location[1]);

      /// <summary>
      ///   Gets the ID from the port data.
      /// </summary>
      private static string GetPortId(PortData portData) => portData.Id;
    }

    /// <summary>
    ///   A custom edge creator connects the edges to specified ports.
    ///   Overrides CreateEdgeCore to extract a port ID from the source and target node ID.
    ///   Determines the port with that ID and connects the edge to that port.
    /// </summary>
    private sealed class PortAwareEdgeCreator : EdgeCreator<NodeData>
    {
      private readonly bool successor;
      private readonly AdjacencyGraphBuilder adjacencyGraphBuilder;

      public PortAwareEdgeCreator(bool successor, IEdgeDefaults defaults, AdjacencyGraphBuilder adjacencyGraphBuilder) {
        this.successor = successor;
        Defaults = defaults;
        this.adjacencyGraphBuilder = adjacencyGraphBuilder;
      }

      /// <summary>
      ///   Overrides the original edge creator to connect edges to specified ports.
      /// </summary>
      /// <param name="graph">The graph to operate on.</param>
      /// <param name="source">The source node.</param>
      /// <param name="target">The target node.</param>
      /// <param name="style">The edge style.</param>
      /// <param name="tag">The edge tag (actually the node data of the target node)</param>
      protected override IEdge CreateEdgeCore(IGraph graph, INode source, INode target, IEdgeStyle style, object? tag) {
        var data = (successor ? target.Tag : source.Tag) as NodeData ??
            throw new InvalidOperationException("Expected node data as the endpoint tag.");

        // get the source port: if there is an ID specified (from), get the first with the matching ID
        // if no ID is specified: get the first port
        var sourcePortId = GetSourcePortId(data);
        var sourcePort = sourcePortId is not null
            ? source.Ports.FirstOrDefault(port => Equals(port.Tag, sourcePortId))
            : source.Ports.FirstOrDefault();
        // same for the target port
        var targetPortId = GetTargetPortId(data);
        var targetPort = targetPortId is not null
            ? target.Ports.FirstOrDefault(port => Equals(port.Tag, targetPortId))
            : target.Ports.FirstOrDefault();

        // create the edges between source and target port. if no port is provided, add a default port.
        return graph.CreateEdge(sourcePort ?? graph.AddPort(source), targetPort ?? graph.AddPort(target), style, tag);
      }

      /// <summary>
      ///   Updates a given edge. This function is called for edges which already exist in the graph.
      ///   It delegates to the base implementation for the actual edge update,
      ///   then sets the new ports if needed.
      /// </summary>
      /// <param name="graph">The graph to operate on.</param>
      /// <param name="edge">The edge to update.</param>
      /// <param name="dataItem">The node data of the target node.</param>
      public override void UpdateEdge(IGraph graph, IEdge edge, NodeData dataItem) {
        base.UpdateEdge(graph, edge, dataItem);
        var data =
            adjacencyGraphBuilder.GetDataItem(successor ? edge.GetTargetNode() : edge.GetSourceNode()) as NodeData ??
            throw new InvalidOperationException("Expected node data for the edge endpoint.");

        // If there is no ID or the ID is already the current port ID don't update the port.
        // Otherwise, get the first port at the source node which matches the ID.
        var sourcePortId = GetSourcePortId(data);
        var sourcePort = sourcePortId is not null && !Equals(sourcePortId, edge.SourcePort.Tag)
            ? edge.SourcePort.Owner.Ports.FirstOrDefault(port => Equals(port.Tag, sourcePortId))
            : edge.SourcePort;
        // same for the target port
        var targetPortId = GetTargetPortId(data);
        var targetPort = targetPortId is not null && !Equals(targetPortId, edge.TargetPort.Tag)
            ? edge.TargetPort.Owner.Ports.FirstOrDefault(port => Equals(port.Tag, targetPortId))
            : edge.TargetPort;
        // remember the current source and target ports
        var oldSource = edge.SourcePort;
        var oldTarget = edge.TargetPort;

        // now set the new ports
        graph.SetEdgePorts(edge, sourcePort ?? edge.SourcePort, targetPort ?? edge.TargetPort);

        // if the source or target port has been changed and the old port doesn't have a tag:
        // remove it since it has been auto-generated
        if (oldSource != edge.SourcePort && oldSource.Tag is null) {
          graph.Remove(oldSource);
        }
        if (oldTarget != edge.TargetPort && oldTarget.Tag is null) {
          graph.Remove(oldTarget);
        }
      }

      /* *****************************************************
       * Developers who want to use this adjacency graph builder
       * in their own applications can modify the following
       * methods to adapt to different NodeData and
       * PortData definitions.
       * *****************************************************/

      /// <summary>
      ///   Gets the source port ID from the node data.
      ///   Returns null if the port to connect is not specified.
      /// </summary>
      private static string? GetSourcePortId(NodeData data) => data.From;

      /// <summary>
      ///   Gets the target port ID from the node data.
      ///   Returns null if the port to connect is not specified.
      /// </summary>
      private static string? GetTargetPortId(NodeData data) => data.To;
    }
  }
}
