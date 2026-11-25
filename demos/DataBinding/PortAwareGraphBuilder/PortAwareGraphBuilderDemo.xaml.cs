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

namespace yWorks.Demo.DataBinding.PortAwareGraphBuilder
{
  /// <summary>
  ///   This demo shows how to automatically build a graph from business data using
  ///   a customized GraphBuilder which creates node ports based on the node type and
  ///   connects the edges to those ports.
  ///   It also uses GraphBuilder's updateGraph method to modify the existing graph
  ///   to reflect changes in the business data.
  /// </summary>
  [Demo("port-aware-graph-builder", "data-binding", "Port-aware Graph Builder",
      Summary = "Builds a graph using GraphBuilder and connects the items to specific ports.",
      Tags = ["json", "data binding", "layout"], Keywords = ["hierarchical", "ports"], RelatedDemos = [])]
  public sealed partial class PortAwareGraphBuilderDemo
  {
    private GraphBuilder builder = null!;
    private GraphBuilderData graphData = null!;
    private NodesSource<NodeData> nodesSource = null!;
    private EdgesSource<EdgeData> edgesSource = null!;
    private int updateIndex;

    protected override async void Initialize() {
      // We want to keep the unused ports
      graphControl.Graph.NodeDefaults.Ports.AutoCleanUp = false;

      // use the viewer input mode since this demo should not allow interactive graph editing
      graphControl.InputMode = new GraphViewerInputMode();

      // build the graph from data
      graphData = ReadGraphData();
      builder = CreatePortAwareGraphBuilder(graphControl.Graph, graphData.Gates, graphData.Connections);
      builder.BuildGraph();

      // center the graph in the visible area
      await graphControl.FitGraphBounds();

      // arrange the graph using a hierarchical layout algorithm
      await ArrangeGraphAsync(graphControl);
    }

    /// <summary>
    ///   Updates the graph. This reflects changes in the business data while keeping the unchanged items.
    ///   This function uses GraphBuilder's UpdateGraph method to modify the existing graph
    ///   instead of building it anew.
    /// </summary>
    private async Task UpdateGraphAsync(
        GraphControl graphControl,
        List<NodeData> nodesSourceData,
        List<EdgeData> edgesSourceData) {
      var graph = graphControl.Graph;

      // determine which nodes were added while updating the graph
      var newNodes = new List<INode>();
      EventHandler<GraphBuilderItemEventArgs<INode, object>> nodeCreatedListener = (_, evt) => newNodes.Add(evt.Item);
      builder.NodeCreated += nodeCreatedListener;

      // update the graph according the new (but related) data
      // this will remove nodes whose IDs are not in the new data set
      // this will add nodes whose IDs are in the new data set, but are not in the old one
      SetBuilderData(nodesSourceData, edgesSourceData);
      builder.UpdateGraph();

      builder.NodeCreated -= nodeCreatedListener;

      // hide the new items (i.e. the new nodes, the edges connected to the new nodes, their labels
      // and their ports) during the animated layout calculation
      HideItems.HideNodesAndRelatedItems(graph, newNodes);

      await ArrangeGraphAsync(graphControl);

      // after the layout animation has finished, show the previously hidden items
      // this way new items do not seem to be affected by the layout calculation
      // otherwise, new items would appear at the default location (0,0) and then move to their
      // final location during the layout animation
      HideItems.ShowNodesAndRelatedItems(graph, newNodes);
    }

    /// <summary>
    ///   Arranges the graph of the given graph component and applies the new layout in an animated
    ///   fashion.
    /// </summary>
    private async Task ArrangeGraphAsync(GraphControl graphControl) {
      updateButton.IsEnabled = false;
      try {
        var algorithm = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.LeftToRight };

        // arrange the graph with the chosen layout algorithm
        await new LayoutExecutor(graphControl, algorithm) {
            PortPlacementPolicies = { Constant = PortPlacementPolicy.KeepParameter },
            AnimateViewport = true,
            AnimationDuration = TimeSpan.FromSeconds(0.5)
        }.Start();
      } finally {
        updateButton.IsEnabled = true;
      }
    }

    /// <summary>
    ///   Creates a new instance of a port-aware graph builder.
    ///   The builder uses the given node and edge sources.
    /// </summary>
    /// <param name="graph">The graph the builder works on</param>
    /// <param name="sampleNodes">The data to build the nodes from</param>
    /// <param name="sampleEdges">The data to build the edges from</param>
    private GraphBuilder CreatePortAwareGraphBuilder(
        IGraph graph,
        List<NodeData> sampleNodes,
        List<EdgeData> sampleEdges) {
      var graphBuilder = new GraphBuilder(graph);

      nodesSource = graphBuilder.CreateNodesSource(sampleNodes, data => data.Id);
      // change the node creator but keep the original's defaults
      var nodeCreator = new TypeAwareNodeCreator { Defaults = nodesSource.NodeCreator.Defaults };
      nodeCreator.StyleProvider = GetNodeStyle;
      nodesSource.NodeCreator = nodeCreator;
      nodesSource.NodeCreator.Defaults.Size = new SizeD(100, 50);

      edgesSource = graphBuilder.CreateEdgesSource(sampleEdges, data => GetNodeId(data.From),
          data => GetNodeId(data.To), data => data.Id);
      edgesSource.EdgeCreator = new PortAwareEdgeCreator { Defaults = edgesSource.EdgeCreator.Defaults };
      edgesSource.EdgeCreator.Defaults.Style = new PolylineEdgeStyle { Pen = new Pen(Brushes.Black, 2) };
      return graphBuilder;
    }

    /// <summary>
    ///   Update new nodes and edges data for the builder's sources.
    /// </summary>
    /// <param name="nodesSourceData">The new node data to set.</param>
    /// <param name="edgesSourceData">The new edge data to set.</param>
    private void SetBuilderData(List<NodeData> nodesSourceData, List<EdgeData> edgesSourceData) {
      builder.SetData(nodesSource, nodesSourceData);
      builder.SetData(edgesSource, edgesSourceData);
    }

    /// <summary>
    ///   Reads the initial graph and update data from the embedded JSON resource.
    /// </summary>
    private GraphBuilderData ReadGraphData() {
      using var jsonStream = LoadAssemblyResource("graph-builder-data.json");
      return JsonSerializer.Deserialize<GraphBuilderData>(jsonStream,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    /// <summary>
    ///   Extracts the node ID from the source or target ID of an edge.
    /// </summary>
    /// <param name="fullId">The full ID.</param>
    private static string GetNodeId(string fullId) =>
        // here the ID is 'nodeId;portId'
        fullId.Split(';')[0];

    private static INodeStyle GetNodeStyle(NodeData nodeData) {
      return nodeData.Type switch {
          "or" => new OrNodeStyle(false, "#A49966", "#625F50", "#FFFFFF"),
          "nor" => new OrNodeStyle(true, "#C7C7A6", "#77776E", "#000000"),
          "and" => new AndGateNodeStyle(false, "#363020", "#201F1A", "#FFFFFF"),
          "nand" => new AndGateNodeStyle(true, "#605C4E", "#3A3834", "#FFFFFF"),
          "xor" => new XOrNodeStyle(false, "#A4778B", "#62555B", "#FFFFFF"),
          "xnor" => new XOrNodeStyle(true, "#AA4586", "#66485B", "#FFFFFF"),
          "not" => new NotNodeStyle("#177e89", "#093237", "#FFFFFF"),
          _ => new ShapeNodeStyle()
      };
    }

    /// <summary>
    ///   Binds the toolbar button to its functionality.
    /// </summary>
    private async void UpdateButton_OnClick(object sender, RoutedEventArgs e) {
      // build graph from new data
      var update = ++updateIndex % 2 == 1;
      var nodeData = update ? graphData.UpdateGates : graphData.Gates;
      var edgeData = update ? graphData.UpdateConnections : graphData.Connections;
      await UpdateGraphAsync(graphControl, nodeData, edgeData);
    }

    public PortAwareGraphBuilderDemo() {
      InitializeComponent();
    }

    [STAThread]
    public static void Main() => Run<PortAwareGraphBuilderDemo>();

    /// <summary>
    ///   A node creator which adds ports according to the node's type.
    ///   Overrides CreateNodeCore to add ports according to the node's type.
    /// </summary>
    private sealed class TypeAwareNodeCreator : NodeCreator<NodeData>
    {
      /// <summary>
      ///   Overrides the original node creator to add ports
      ///   according to the node type.
      ///   Note that the given style is already chosen for the node type
      ///   since the style provider is set by the builder.
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

        // add ports according to their type
        foreach (var portData in GetPorts(nodeData)) {
          AddPort(graph, node, portData);
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
      private static void AddPort(IGraph graph, INode node, PortData portData) {
        var port = graph.AddPort(node, FreeNodePortLocationModel.Instance.CreateParameter(GetPortLocation(portData)),
            GetPortStyle(portData), GetPortId(portData));
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
          graph.AddLabel(port, text, GetParameter(portData));
        }
      }

      /// <summary>
      ///   Updates a given node. This function is called for nodes which already exist in the graph.
      ///   The method itself updates the ports and style. The actual node update is delegated to the base method.
      /// </summary>
      /// <param name="graph">The graph that contains the node.</param>
      /// <param name="node">The node to update.</param>
      /// <param name="parent">The parent node.</param>
      /// <param name="dataItem">The new data item.</param>
      public override void UpdateNode(IGraph graph, INode node, INode? parent, NodeData dataItem) {
        base.UpdateNode(graph, node, parent, dataItem);
        graph.SetStyle(node, GetNodeStyle(dataItem));
        var index = 0;
        var portsToRemove = new List<IPort>();
        var ports = GetPorts(dataItem);
        // synchronize the ports according to the port data:
        // 1. update existing ports for which port data also exists
        //     with ID, location, and label according to the data
        // 2. remove ports for which no data exists
        // 3. add ports for new port data
        foreach (var port in node.Ports) {
          if (index < ports.Count) {
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
            // mark existing ports for which no data exists to be removed
            portsToRemove.Add(port);
          }
        }
        // actually remove the marked ports
        if (portsToRemove.Count > 0) {
          foreach (var port in portsToRemove) {
            graph.Remove(port);
          }
        } else {
          // if there is port data left, add ports for it
          for (var i = index; i < ports.Count; i++) {
            AddPort(graph, node, ports[i]);
          }
        }
      }

      /* *****************************************************
       * Developers who want to use this graph builder
       * in their own applications can modify the following
       * methods to adapt to different node data (Gate) and
       * port data (Pin) definitions.
       * *****************************************************/

      /// <summary>
      ///   Gets an array of PortData from the given node data.
      ///   The port data is created according to the node type.
      /// </summary>
      private static List<PortData> GetPorts(NodeData nodeData) {
        return nodeData.Type switch {
            "or" or "nor" or "and" or "nand" or "xor" or "xnor" => [
                new PortData("in0", "in0", new PointD(0, 0.25)),
                new PortData("in1", "in1", new PointD(0, 0.75)),
                new PortData("out0", "out", new PointD(1, 0.5), true)
            ],
            _ => [
                new PortData("in0", "in", new PointD(0, 0.5)),
                new PortData("out0", "out", new PointD(1, 0.5), true)
            ]
        };
      }

      /// <summary>
      ///   Gets the (relative) port location from the port data.
      /// </summary>
      private static PointD GetPortLocation(PortData pin) => pin.Location;

      /// <summary>
      ///   Gets the ID from the port data.
      /// </summary>
      private static string GetPortId(PortData pin) => pin.Id;

      /// <summary>
      ///   Gets the port style from the port data.
      /// </summary>
      private static IPortStyle GetPortStyle(PortData pin) =>
          // the ports are invisible
          VoidPortStyle.Instance;

      /// <summary>
      ///   Gets the port label location parameter.
      /// </summary>
      private static ILabelModelParameter GetParameter(PortData pin) =>
          FreePortLabelModel.Instance.CreateParameter(new PointD(0, -5), new PointD(pin.RightSide ? 0.8 : 0.5, 1));

      /// <summary>
      ///   Gets the port label text.
      ///   Might be null if no label should be displayed.
      /// </summary>
      private static string? GetPortLabel(PortData pin) => pin.Name;
    }

    /// <summary>
    ///   A custom edge creator connects the edges to specified ports.
    ///   Overrides CreateEdgeCore to extract a port ID from the source and target node ID.
    ///   Determines the port with that ID and connects the edge to that port.
    /// </summary>
    private sealed class PortAwareEdgeCreator : EdgeCreator<EdgeData>
    {
      /// <summary>
      ///   Overrides the original edge creator to connect edges to specified ports.
      /// </summary>
      /// <param name="graph">The graph to operate on.</param>
      /// <param name="source">The source node.</param>
      /// <param name="target">The target node.</param>
      /// <param name="style">The edge style.</param>
      /// <param name="tag">The edge tag (actually the edge data of the edge source array)</param>
      protected override IEdge CreateEdgeCore(IGraph graph, INode source, INode target, IEdgeStyle style, object? tag) {
        var edgeData = tag as EdgeData ?? throw new InvalidOperationException("Expected edge data as the edge tag.");
        // get the source port: if there is an ID specified (from), get the first with the matching ID
        // if no ID is specified: get the first port
        var sourcePortId = GetSourcePortId(edgeData);
        var sourcePort = !string.IsNullOrEmpty(sourcePortId)
            ? source.Ports.FirstOrDefault(port => Equals(port.Tag, sourcePortId))
            : source.Ports.FirstOrDefault();
        var targetPortId = GetTargetPortId(edgeData);
        var targetPort = !string.IsNullOrEmpty(targetPortId)
            ? target.Ports.FirstOrDefault(port => Equals(port.Tag, targetPortId))
            : target.Ports.FirstOrDefault();

        // create the edges between source and target port. If no port is provided add a default port.
        return graph.CreateEdge(sourcePort ?? graph.AddPort(source), targetPort ?? graph.AddPort(target), style, tag);
      }

      /// <summary>
      ///   Updates a given edge. This function is called for edges which already exist in the graph.
      ///   Delegates to the base implementation for the actual edge update,
      ///   then sets the new ports if needed.
      /// </summary>
      /// <param name="graph">The graph that contains the edge.</param>
      /// <param name="edge">The edge to update.</param>
      /// <param name="dataItem">The edge data of the edge to update.</param>
      public override void UpdateEdge(IGraph graph, IEdge edge, EdgeData dataItem) {
        base.UpdateEdge(graph, edge, dataItem);

        // If there is no ID or the ID is already the current port ID don't update the port.
        // Otherwise, get the first port at the source node which matches the ID.
        var sourcePortId = GetSourcePortId(dataItem);
        var sourcePort = !string.IsNullOrEmpty(sourcePortId) && !Equals(sourcePortId, edge.SourcePort.Tag)
            ? edge.SourcePort.Owner.Ports.FirstOrDefault(port => Equals(port.Tag, sourcePortId))
            : edge.SourcePort;
        // same for the target port
        var targetPortId = GetTargetPortId(dataItem);
        var targetPort = !string.IsNullOrEmpty(targetPortId) && !Equals(targetPortId, edge.TargetPort.Tag)
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
       * Developers who want to use this graph builder
       * in their own applications can modify the following
       * methods to adapt to different edge data definitions.
       * *****************************************************/

      /// <summary>
      ///   Gets the source port ID from the edge data.
      ///   Returns null if the port to connect is not specified.
      /// </summary>
      private static string? GetSourcePortId(EdgeData data) => GetPortId(data.From);

      /// <summary>
      ///   Gets the target port ID from the edge data.
      ///   Returns null if the port to connect is not specified.
      /// </summary>
      private static string? GetTargetPortId(EdgeData data) => GetPortId(data.To);

      private static string? GetPortId(string fullId) {
        var parts = fullId.Split(';');
        return parts.Length > 1 ? parts[1] : null;
      }
    }

    /// <summary>
    ///   internally used port data
    /// </summary>
    private sealed record PortData(string Id, string? Name, PointD Location, bool RightSide = false);
  }
}
