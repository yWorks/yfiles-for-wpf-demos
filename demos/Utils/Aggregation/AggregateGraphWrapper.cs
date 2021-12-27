/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Demo.yFiles.Aggregation
{
  
  /// <summary>
  /// Determines what kind of edges should be created when replacing original edges with aggregation edges in calls to
  /// methods of the <see cref="AggregateGraphWrapper"/>.
  /// </summary>
  public enum EdgeReplacementPolicy
  {
    /// <summary>
    /// Edges will not be replaced by aggregation edges.
    /// </summary> 
    None,

    /// <summary>
    /// During <see cref="AggregateGraphWrapper.Aggregate"/> all edges between any of the aggregated nodes and other
    /// nodes are replaced by a single aggregation edge between the aggregation node and the other node.
    /// </summary>
    /// <remarks>
    /// This means there will be no duplicate edges between any pairs of nodes. The direction of the created edge is not
    /// deterministic.
    /// </remarks>
    Undirected,
    
    /// <summary>
    /// Edges in both directions will be created, resulting in up to two edges between pairs of nodes.
    /// </summary>
    Directed
  }

  /// <summary>
  /// An IGraph implementation that wraps another graph and can replace some of its items by other items.
  /// </summary>
  /// <remarks>
  /// <para>
  /// More precisely, a set of nodes can be aggregated to a new node with the <see cref="Aggregate"/> method.
  /// This will hide the set of nodes and create a new aggregation node while replacing adjacent edges with aggregation edges.
  /// </para>
  /// <para>
  /// Items of the wrapped graph ("original graph") are called <em>original items</em> while the temporary items that
  /// are created for aggregation are called <em>aggregation items</em>.
  /// </para>
  /// <para>
  /// This class implements a concept similar to grouping and folding. The conceptual difference is that with folding
  /// the group nodes remain in the graph while the group is in expanded state. Contrary, with the AggregateGraphWrapper
  /// the aggregation nodes are only in the graph when the nodes are aggregated. The difference in the implementation
  /// is that the AggregateGraphWrapper reuses all original graph items, ensuring reference equality between items of the
  /// wrapped graph and items of the AggregateGraphWrapper.
  /// </para>
  /// <para>
  /// Note that this implementation does not support editing user gestures, e.g. with the <see cref="GraphEditorInputMode"/>.
  /// </para>
  /// <para>
  /// Note also that this instance will register listeners with the wrapped graph instance, so <see cref="Dispose"/> should
  /// be called if this instance is not used any more.
  /// </para>
  /// </remarks>
  public sealed class AggregateGraphWrapper : GraphWrapperBase
  {
    // This implementation combines a filtered graph (for hiding items) and additional aggregation items contained in
    // the aggregationNodes and aggregationEdges lists.
    // Events are forwarded from the wrapped graph to the filtered graph to this graph.
    // Most IGraph methods are overridden and "multiplex" between the filtered graph and the aggregation items.
    private FilteredGraphWrapper filteredGraph;

    // the set of hidden nodes and edges
    private readonly ISet<IModelItem> filteredOriginalNodes = new HashSet<IModelItem>();
    private readonly ISet<AggregationItem> filteredAggregationItems = new HashSet<AggregationItem>();

    private readonly IList<AggregationNode> aggregationNodes = new List<AggregationNode>();
    private readonly IList<AggregationEdge> aggregationEdges = new List<AggregationEdge>();

    // live views of the currently visible items
    private readonly ListEnumerable<INode> nodes;
    private readonly ListEnumerable<IEdge> edges;
    private readonly ListEnumerable<ILabel> labels;
    private readonly ListEnumerable<IPort> ports;

    private INodeDefaults aggregationNodeDefaults;
    private IEdgeDefaults aggregationEdgeDefaults;

    private readonly AggregateLookupDecorator lookupDecorator;

    /// <summary>
    /// Creates a new instance of this graph wrapper.
    /// </summary>
    /// <param name="graph">The graph to be wrapped ("original graph").</param>
    /// <exception cref="ArgumentException">
    /// If the <paramref name="graph"/> is another <see cref="AggregateGraphWrapper"/>
    /// </exception>
    public AggregateGraphWrapper(IGraph graph) : base(graph) {
      if (graph is AggregateGraphWrapper) {
        throw new ArgumentException("Cannot wrap another AggregateGraphWrapper", "graph");
      }

      // the base constructor call sets graph as WrappedGraph
      // this triggers OnGraphChanged where filteredGraph is initialized as FilteredGraphWrapper of graph

      EdgeReplacementPolicy = EdgeReplacementPolicy.Undirected;

      lookupDecorator = new AggregateLookupDecorator(this);

      nodes = new ListEnumerable<INode>(filteredGraph.Nodes.Concat(aggregationNodes.Where(AggregationItemPredicate)));
      edges = new ListEnumerable<IEdge>(filteredGraph.Edges.Concat(aggregationEdges.Where(AggregationItemPredicate)));
      ports = new ListEnumerable<IPort>(nodes.SelectMany(node => node.Ports)
                                             .Concat(edges.SelectMany(edge => edge.Ports)));
      labels = new ListEnumerable<ILabel>(nodes.SelectMany(node => node.Labels)
                                               .Concat(edges.SelectMany(edge => edge.Labels)
                                                            .Concat(ports.SelectMany(port => port.Labels))));
    }

    public override IListEnumerable<INode> Nodes { get { return nodes; } }

    public override IListEnumerable<IEdge> Edges { get { return edges; } }

    public override IListEnumerable<ILabel> Labels { get { return labels; } }

    public override IListEnumerable<IPort> Ports { get { return ports; } }

    /// <summary>
    /// Sets what kind of edges should be created when replacing original edges with aggregation edges.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="Demo.yFiles.Aggregation.EdgeReplacementPolicy.Undirected"/>.
    /// </remarks>
    public EdgeReplacementPolicy EdgeReplacementPolicy { get; set; }

    /// <summary>
    /// Gets or sets the defaults for aggregation nodes.
    /// </summary>
    /// <seealso cref="IGraph.NodeDefaults"/>
    public INodeDefaults AggregationNodeDefaults {
      get {
        if (aggregationNodeDefaults == null) {
          aggregationNodeDefaults = new NodeDefaults();
        }
        return aggregationNodeDefaults;
      }
      set { aggregationNodeDefaults = value; }
    }

    /// <summary>
    /// Gets or sets the defaults for aggregation edges.
    /// </summary>
    /// <remarks>
    /// Used when original edges are automatically replaced by aggregation edges.
    /// </remarks>
    public IEdgeDefaults AggregationEdgeDefaults {
      get {
        if (aggregationEdgeDefaults == null) {
          aggregationEdgeDefaults = new EdgeDefaults();
        }
        return aggregationEdgeDefaults;
      }
      set { aggregationEdgeDefaults = value; }
    }

    /// <summary>
    /// Calls the base method with the <see cref="filteredGraph"/> instead of the passed graph for correct event
    /// forwarding.
    /// </summary>
    protected override void OnGraphChanged(IGraph oldGraph, IGraph newGraph) {
      if (oldGraph == null) {
        filteredGraph = new FilteredGraphWrapper(WrappedGraph, NodePredicate);
        base.OnGraphChanged(null, filteredGraph);
      } else if (newGraph == null) {
        filteredGraph = null;
        base.OnGraphChanged(filteredGraph, null);
      }
    }

    public override void Dispose() {
      filteredGraph.Dispose();
      base.Dispose();
    }

    private bool AggregationItemPredicate(IModelItem item) {
      return !filteredAggregationItems.Contains(item);
    }

    private bool NodePredicate(INode node) {
      return !filteredOriginalNodes.Contains(node);
    }

    #region hide/show

    /// <summary>
    /// Hides the <paramref name="portOwner"/> and all items depending on it and raises the according removed events.
    /// </summary>
    /// <remarks>
    /// For nodes, their labels, ports and adjacent edges are hidden.
    /// For edges, their labels, ports and bends are hidden.
    /// </remarks>
    private void Hide(IPortOwner portOwner) {
      var aggregationNode = portOwner as AggregationNode;
      if (aggregationNode != null) {
        var oldIsGroupNode = IsGroupNode(aggregationNode);
        var oldParent = GetParent(aggregationNode);
        HideAdjacentEdges(aggregationNode);
        filteredAggregationItems.Add(aggregationNode);
        RaiseLabelRemovedEvents(aggregationNode);
        RaisePortRemovedEvents(aggregationNode);
        OnNodeRemoved(new NodeEventArgs(aggregationNode, oldParent, oldIsGroupNode));
        return;
      }

      var aggregationEdge = portOwner as AggregationEdge;
      if (aggregationEdge != null) {
        HideAdjacentEdges(aggregationEdge);
        filteredAggregationItems.Add(aggregationEdge);
        RaiseLabelRemovedEvents(aggregationEdge);
        RaisePortRemovedEvents(aggregationEdge);
        OnEdgeRemoved(new EdgeEventArgs(aggregationEdge));
        return;
      }

      // hide adjacent aggregation edges (which are not hidden by filtered graph)
      foreach (var edge in EdgesAt(portOwner).OfType<AggregationEdge>().ToList()) {
        Hide(edge);
      }
      filteredOriginalNodes.Add(portOwner);
      PredicateChanged(portOwner);
    }

    private void HideAdjacentEdges(AggregationLabelPortOwner portOwner) {
      foreach (var edge in EdgesAt(portOwner).ToList()) {
        Hide(edge);
      }
    }

    /// <summary>
    /// Shows an item, their labels/ports/bends, and their adjacent edges. Raises all necessary events.
    /// </summary>
    private void Show(IPortOwner item) {
      var aggregationNode = item as AggregationNode;
      if (aggregationNode != null) {
        filteredAggregationItems.Remove(aggregationNode);
        OnNodeCreated(new NodeEventArgs(aggregationNode, GetParent(aggregationNode), IsGroupNode(aggregationNode)));
        RaisePortAddedEvents(aggregationNode);
        ShowAdjacentEdges(aggregationNode);
        RaiseLabelAddedEvents(aggregationNode);
        return;
      }

      var aggregationEdge = item as AggregationEdge;
      if (aggregationEdge != null) {
        filteredAggregationItems.Remove(aggregationEdge);
        OnEdgeCreated(new EdgeEventArgs(aggregationEdge));
        RaisePortAddedEvents(aggregationEdge);
        ShowAdjacentEdges(aggregationEdge);
        RaiseLabelAddedEvents(aggregationEdge);
        return;
      }

      filteredOriginalNodes.Remove(item);
      PredicateChanged(item);
      ShowAdjacentEdges(item);
    }

    private void ShowAdjacentEdges(IPortOwner portOwner) {
      // - cannot use EdgesAt() here, since hidden edges are not considered there
      // - aggregation edges are enough, since original edges are managed by FilteredGraphWrapper
      var adjacentEdges = aggregationEdges.Where(edge =>
          portOwner.Ports.Contains(edge.SourcePort) || portOwner.Ports.Contains(edge.TargetPort));
      foreach (var edge in adjacentEdges) {
        if (Ports.Contains(edge.SourcePort) && Ports.Contains(edge.TargetPort)) {
          Show(edge);
        }
      }
    }

    private void RaiseLabelAddedEvents(ILabelOwner labelOwner) {
      foreach (var label in labelOwner.Labels) {
        OnLabelAdded(new LabelEventArgs(label, labelOwner));
      }
    }

    private void RaisePortAddedEvents(IPortOwner portOwner) {
      foreach (var port in portOwner.Ports) {
        OnPortAdded(new PortEventArgs(port, portOwner));
        RaiseLabelAddedEvents(port);
      }
    }

    private void RaiseLabelRemovedEvents(AggregationLabelOwner labelOwner) {
      foreach (var label in labelOwner.Labels) {
        OnLabelRemoved(new LabelEventArgs(label, labelOwner));
      }
    }

    private void RaisePortRemovedEvents(AggregationLabelPortOwner portOwner) {
      foreach (var port in portOwner.Ports) {
        RaiseLabelRemovedEvents(port as AggregationPort);
        OnPortRemoved(new PortEventArgs(port, portOwner));
      }
    }

    private void PredicateChanged(IModelItem item) {
      if (item is INode) {
        filteredGraph.NodePredicateChanged((INode) item);
      } else if (item is IEdge) {
        filteredGraph.EdgePredicateChanged((IEdge) item);
      }
    }

    #endregion

    #region Aggregate/Separate

    /// <summary>
    /// Aggregates the <paramref name="nodes"/> to a new aggregation node.
    /// </summary>
    /// <remarks>
    /// This temporarily removes the <paramref name="nodes"/> together with their labels, ports, and adjacent edges.
    /// Then a new aggregation node is created and replacement edges for all removed edges are created: for each edge
    /// between a node in <paramref name="nodes"/> and a node not in <paramref name="nodes"/> a new edge is created. If
    /// this would lead to multiple edges between the aggregation node and another node, only one edge (or two, see
    /// <see cref="EdgeReplacementPolicy"/>) is created.
    /// </remarks>
    /// <seealso cref="Separate"/>
    /// <param name="nodes">The nodes to be temporarily removed.</param>
    /// <param name="layout">The layout for the new aggregation node or <c>null</c></param>
    /// <param name="style">The style for the new aggregation node or <c>null</c></param>
    /// <param name="tag">The style for the new aggregation node or <c>null</c></param>
    /// <returns>A new aggregation node.</returns>
    /// <exception cref="ArgumentException">Any of the <paramref name="nodes"/> is not in the graph.</exception>
    public INode Aggregate(IListEnumerable<INode> nodes, RectD? layout = null,
        INodeStyle style = null, object tag = null) {
      var badNode = nodes.FirstOrDefault(n => !Contains(n));
      if (badNode != null) {
        throw new ArgumentException("Cannot aggregate node " + badNode + " that is not in this graph.", "nodes");
      }

      var nodeLayout = layout != null
          ? new MutableRectangle(layout)
          : new MutableRectangle(PointD.Origin, AggregationNodeDefaults.Size);
      var nodeStyle = style ?? AggregationNodeDefaults.GetStyleInstance();
      IList<INode> aggregatedNodes = new List<INode>(nodes);
      var aggregationNode = new AggregationNode(this, aggregatedNodes) {
          layout = nodeLayout, Style = nodeStyle, Tag = tag
      };

      var parent = this.GetGroupingSupport().GetNearestCommonAncestor(nodes);
      if (parent != null) {
        aggregationNode.parent = parent;
        var aggregationNodeParent = parent as AggregationNode;
        if (aggregationNodeParent != null) {
          aggregationNodeParent.children.Add(aggregationNode);
        }
      }

      aggregationNodes.Add(aggregationNode);
      OnNodeCreated(new ItemEventArgs<INode>(aggregationNode));

      if (EdgeReplacementPolicy != EdgeReplacementPolicy.None) {
        ReplaceAdjacentEdges(nodes, aggregationNode);
      }

      // hide not until here, so old graph structure is still intact when replacing edges
      foreach (var node in nodes) {
        Hide(node);
      }

      return aggregationNode;
    }

    /// <summary>
    /// Replaces adjacent edges by new aggregation edges. Prevents duplicate edges following <see cref="EdgeReplacementPolicy"/>.
    /// </summary>
    private void ReplaceAdjacentEdges(IListEnumerable<INode> nodes, AggregationNode aggregationNode) {
      var edgesAreDirected = EdgeReplacementPolicy == EdgeReplacementPolicy.Directed;
      var outgoingReplacementEdges = new Dictionary<IPortOwner, AggregationEdge>();
      var incomingReplacementEdges =
          edgesAreDirected ? new Dictionary<IPortOwner, AggregationEdge>() : outgoingReplacementEdges;
      foreach (var node in nodes) {
        ReplaceEdges(AdjacencyTypes.Outgoing, node, aggregationNode, nodes, outgoingReplacementEdges);
        ReplaceEdges(AdjacencyTypes.Incoming, node, aggregationNode, nodes, incomingReplacementEdges);
      }

      // raise edge created events not until here, so the aggregated items are complete
      foreach (var edge in outgoingReplacementEdges.Values) {
        OnEdgeCreated(new EdgeEventArgs(edge));
      }

      if (edgesAreDirected) {
        foreach (var edge in incomingReplacementEdges.Values) {
          OnEdgeCreated(new EdgeEventArgs(edge));
        }
      }
    }

    private void ReplaceEdges(AdjacencyTypes adjacencyType, INode node, IPortOwner aggregationPortOwner,
        IListEnumerable<INode> items,
        IDictionary<IPortOwner, AggregationEdge> replacementEdges) {
      var adjacentEdges = EdgesAt(node, adjacencyType).ToList();
      foreach (var edge in adjacentEdges) {
        var isIncoming = adjacencyType == AdjacencyTypes.Incoming;
        var otherPort = isIncoming ? edge.SourcePort : edge.TargetPort;

        var otherPortOwner = otherPort.Owner;
        if (items.Contains(otherPortOwner)) {
          // don't create aggregation edges for edges between aggregated items
          continue;
        }
        AggregationEdge existingReplacementEdge;
        if (replacementEdges.TryGetValue(otherPortOwner, out existingReplacementEdge)) {
          existingReplacementEdge.aggregatedEdges.Add(edge);
          continue;
        }

        if (edge is AggregationEdge) {
          // otherwise the edge is automatically hidden by filtered graph
          Hide(edge);
        }

        var replacementEdge = ReplaceEdge(edge, aggregationPortOwner, otherPort, isIncoming);
        replacementEdges.Add(otherPortOwner, replacementEdge);
      }
    }

    private AggregationEdge ReplaceEdge(IEdge edge, IPortOwner newPortOwner, IPort otherPort, bool isIncoming) {
      var aggregationPort = AddPort(newPortOwner);
      AggregationEdge replacementEdge;
      if (isIncoming) {
        replacementEdge = CreateAggregationEdge(otherPort, aggregationPort, null);
      } else {
        replacementEdge = CreateAggregationEdge(aggregationPort, otherPort, null);
      }

      replacementEdge.aggregatedEdges.Add(edge);
      return replacementEdge;
    }

    /// <summary>
    /// Separates nodes again that were previously aggregated via <see cref="Aggregate"/>.
    /// </summary>
    /// <remarks>
    /// Removes the aggregation node permanently together with its labels, ports, and adjacent edges. Then inserts the
    /// items that were temporarily removed in <see cref="Aggregate"/> again.
    /// </remarks>
    /// <param name="node">The aggregation node to separate.</param>
    /// <exception cref="ArgumentException">The <paramref name="node"/> is not in the graph or is currently hidden or
    /// is not an aggregation node.</exception>
    public void Separate(INode node) {
      var aggregationNode = node as AggregationNode;
      if (aggregationNode == null) {
        throw new ArgumentException("Cannot separate original node " + node + ".", "node");
      }
      if (!Contains(node)) {
        if (aggregationNodes.Contains(node)) {
          throw new ArgumentException(
              "Cannot separate aggregation node " + node + ", because it is aggregated by another node.", "node");
        }
        throw new ArgumentException("Cannot separate aggregation node " + node + " that is not in this graph.", "node");
      }

      var adjacentEdges = EdgesAt(aggregationNode).ToList();
      foreach (var edge in adjacentEdges) {
        RemoveAggregationEdge((AggregationEdge) edge);
      }

      RemoveAggregationNode(aggregationNode);
      foreach (var aggregatedNode in aggregationNode.aggregatedNodes) {
        Show(aggregatedNode);
      }

      foreach (var edge in adjacentEdges) {
        ShowOrRemoveAggregatedEdges((AggregationEdge) edge);
      }

      if (EdgeReplacementPolicy != EdgeReplacementPolicy.None) {
        ReplaceMissingEdges(aggregationNode);
      }
    }

    private void ShowOrRemoveAggregatedEdges(AggregationEdge aggregationEdge) {
      foreach (var aggregatedEdge in aggregationEdge.aggregatedEdges) {
        var replacedEdge = aggregatedEdge as AggregationEdge;
        if (replacedEdge != null) {
          // manually show aggregated AggregationEdges, other edges are handled by filtered graph
          if (Contains(replacedEdge.SourcePort) && Contains(replacedEdge.TargetPort)) {
            Show(aggregatedEdge);
          } else {
            RemoveAggregationEdge(replacedEdge);
          }
        }
      }
    }

    /// <summary>
    /// When nodes are separated in a different order they were aggregated, we need to create new aggregation edges for the
    /// nodes that were just expanded.
    /// </summary>
    private void ReplaceMissingEdges(AggregationNode aggregationNode) {
      var edgesAreDirected = EdgeReplacementPolicy == EdgeReplacementPolicy.Directed;
      var aggregatedNodes = aggregationNode.aggregatedNodes;
      foreach (var node in aggregatedNodes) {
        var outgoingReplacementEdges = new Dictionary<INode, AggregationEdge>();
        var incomingReplacementEdges =
            edgesAreDirected ? new Dictionary<INode, AggregationEdge>() : outgoingReplacementEdges;
        ReplaceMissingEdges(AdjacencyTypes.Outgoing, node, outgoingReplacementEdges);
        ReplaceMissingEdges(AdjacencyTypes.Incoming, node, incomingReplacementEdges);

        // raise edge created events not until here, so the aggregated items are complete
        foreach (var edge in outgoingReplacementEdges.Values) {
          OnEdgeCreated(new EdgeEventArgs(edge));
        }

        if (edgesAreDirected) {
          foreach (var edge in incomingReplacementEdges.Values) {
            OnEdgeCreated(new EdgeEventArgs(edge));
          }
        }
      }
    }

    private void ReplaceMissingEdges(AdjacencyTypes adjacencyType, INode node,
        IDictionary<INode, AggregationEdge> seenNodes) {
      var isIncoming = adjacencyType == AdjacencyTypes.Incoming;

      IEnumerable<IEdge> edgesAt = aggregationEdges.Where(edge =>
          node.Ports.Contains(isIncoming ? edge.TargetPort : edge.SourcePort));

      if (!IsAggregationItem(node)) {
        edgesAt = edgesAt.Concat(base.EdgesAt(node));
      }

      foreach (var edge in edgesAt.ToList()) {
        if (Contains(edge)) {
          // is already a proper edge
          continue;
        }

        var thisPort = isIncoming ? edge.TargetPort : edge.SourcePort;
        var otherPort = isIncoming ? edge.SourcePort : edge.TargetPort;

        // the node is contained in another aggregation node -> find it
        var otherNode = FindAggregationNode(otherPort.Owner as INode);
        if (otherNode == null || !Contains(otherNode)) {
          continue;
        }

        AggregationEdge aggregationEdge;
        if (seenNodes.TryGetValue(otherNode, out aggregationEdge)) {
          // we already created an edge between this and the other node
          aggregationEdge.aggregatedEdges.Add(edge);
          continue;
        }

        aggregationEdge = ReplaceEdge(edge, otherNode, thisPort, isIncoming);
        seenNodes.Add(otherNode, aggregationEdge);
      }
    }

    private AggregationNode FindAggregationNode(INode node) {
      return aggregationNodes.FirstOrDefault(n => n.aggregatedNodes.Contains(node));
    }

    /// <summary>
    /// Separates all aggregation nodes such that this graph contains exactly the same items as the
    /// <see cref="GraphWrapperBase.WrappedGraph"/>.
    /// </summary>
    public void SeparateAll() {
      do {
        var visibleNodes = aggregationNodes.Where(AggregationItemPredicate).ToList();
        foreach (var aggregationNode in visibleNodes) {
          Separate(aggregationNode);
        }
      } while (aggregationNodes.Count > 0);
    }

    #endregion

    #region get aggregation items

    /// <summary>
    /// Returns <c>true</c> iff the <paramref name="item"/> is an aggregation item and therefore not contained
    /// in the wrapped graph.
    /// </summary>
    /// <remarks>
    /// Does not check if the item is currently <see cref="Contains">contained</see> in the graph or whether
    /// the items was created by this graph instance.
    /// </remarks>
    /// <param name="item">The item to check.</param>
    /// <returns><c>true</c> iff the <paramref name="item"/> is an aggregation item.</returns>
    public bool IsAggregationItem(IModelItem item) {
      return item is AggregationItem;
    }

    /// <summary>
    /// Returns the items that are directly aggregated by the <paramref name="item"/>.
    /// </summary>
    /// <remarks>
    /// In contrast to <see cref="GetAllAggregatedOriginalItems"/> this method returns both original as well as
    /// aggregation items, but only direct descendants in the aggregation hierarchy.
    /// </remarks>
    /// <remarks>
    /// <paramref name="item"/> doesn't need to be <see cref="Contains">contained</see> currently but might be
    /// aggregated in another item.
    /// </remarks>
    /// <param name="item">The aggregation item.</param>
    /// <returns>The items that are aggregated by the <paramref name="item"/>. If an aggregation node is passed, this
    /// will return the aggregated nodes. If an aggregation edge is passed, this will return the edges it replaces.
    /// Otherwise an empty enumerable will be returned. The enumerable may contain both aggregation items as well as
    /// original items.
    /// </returns>
    public IListEnumerable<IModelItem> GetAggregatedItems(IModelItem item) {
      var node = item as AggregationNode;
      if (node != null) {
        return new ListEnumerable<IModelItem>(node.aggregatedNodes);
      }

      var edge = item as AggregationEdge;
      if (edge != null) {
        return new ListEnumerable<IModelItem>(edge.aggregatedEdges);
      }

      return ListEnumerable<IModelItem>.Empty;
    }

    /// <summary>
    /// Returns the (recursively) aggregated original items of the <paramref name="item"/>.
    /// </summary>
    /// <remarks>
    /// In contrast to <see cref="GetAggregatedItems"/> this method returns only original items, but also items
    /// recursively nested in the aggregation hierarchy.
    /// </remarks>
    /// <param name="item">The aggregation item.</param>
    /// <returns>A list of items of the <see cref="GraphWrapperBase.WrappedGraph"/> that is either directly contained
    /// in the <paramref name="item"/> or recursively in any contained aggregation items. This list consists only of
    /// items of the wrapped graph.</returns>
    /// <seealso cref="GetAggregatedItems"/>
    public IListEnumerable<IModelItem> GetAllAggregatedOriginalItems(IModelItem item) {
      var result = new List<IModelItem>();
      var aggregatedItems = GetAggregatedItems(item);
      foreach (var aggregatedItem in aggregatedItems) {
        if (IsAggregationItem(aggregatedItem)) {
          result.AddRange(GetAllAggregatedOriginalItems(aggregatedItem));
        } else {
          result.Add(aggregatedItem);
        }
      }
      return new ListEnumerable<IModelItem>(result);
    }

    #endregion

    #region IGraph API

    /// <summary>
    /// Removes the given item from the graph.
    /// </summary>
    /// <remarks>
    /// If <paramref name="item"/> is an aggregation node or aggregation edge, all aggregated items are removed as well.
    /// </remarks>
    /// <param name="item">The item to remove.</param>
    public override void Remove(IModelItem item) {
      if (!Contains(item)) {
        throw new ArgumentException("Item is not in this graph.", "item");
      }
      RemoveCore(item);
    }

    private void RemoveCore(IModelItem item) {
      var aggregationNode = item as AggregationNode;
      if (aggregationNode != null) {
        if (aggregationNode.graph != this) {
          return;
        }
        RemoveAggregationNode(aggregationNode);
        foreach (var aggregatedNode in aggregationNode.aggregatedNodes) {
          // we can remove the node without checking if it is in the graph
          RemoveCore(aggregatedNode);
        }
        return;
      }

      var aggregationEdge = item as AggregationEdge;
      if (aggregationEdge != null) {
        if (aggregationEdge.graph != this) {
          return;
        }
        RemoveAggregationEdge(aggregationEdge);

        foreach (var aggregatedEdge in aggregationEdge.aggregatedEdges) {
          RemoveCore(aggregatedEdge);
        }

        CleanupPort(aggregationEdge.SourcePort);
        CleanupPort(aggregationEdge.TargetPort);
        return;
      }

      if (item is AggregationBend) {
        RemoveAggregationBend((AggregationBend) item);
      } else if (item is AggregationPort) {
        RemoveAggregationPort((AggregationPort) item);
      } else if (item is AggregationLabel) {
        RemoveAggregationLabel((AggregationLabel) item);
      } else {
        base.Remove(item);
      }
    }

    private void CleanupPort(IPort port) {
      var isAggregationItem = IsAggregationItem(port);
      // check the auto-cleanup policy to apply
      var autoCleanUp = (isAggregationItem ? AggregationNodeDefaults 
          : IsGroupNode(port.Owner as INode) 
              ? WrappedGraph.GroupNodeDefaults 
              : WrappedGraph.NodeDefaults)
                        .Ports.AutoCleanUp;
      if (!autoCleanUp) {
        return;
      }
      var edgesAtPort = aggregationEdges.Count(edge => edge.SourcePort == port || edge.TargetPort == port);
      if (!isAggregationItem) {
        edgesAtPort += base.EdgesAt(port).Count;
      }
      if (edgesAtPort == 0) {
        RemoveCore(port);
      }
    }

    private void RemoveAggregationNode(AggregationNode aggregationNode) {
      foreach (var port in aggregationNode.Ports.ToList()) {
        RemoveAggregationPort((AggregationPort) port);
      }
      foreach (var label in aggregationNode.Labels.ToList()) {
        RemoveAggregationLabel((AggregationLabel) label);
      }
      var oldIsGroupNode = IsGroupNode(aggregationNode);
      var oldParent = GetParent(aggregationNode);
      aggregationNodes.Remove(aggregationNode);
      aggregationNode.graph = null;
      OnNodeRemoved(new NodeEventArgs(aggregationNode, oldParent, oldIsGroupNode));
    }

    private void RemoveAggregationEdge(AggregationEdge aggregationEdge) {
      foreach (var label in aggregationEdge.Labels.ToList()) {
        RemoveAggregationLabel((AggregationLabel) label);
      }
      foreach (var port in aggregationEdge.Ports.ToList()) {
        RemoveAggregationPort((AggregationPort) port);
      }
      foreach (var bend in aggregationEdge.Bends.ToList()) {
        RemoveAggregationBend((AggregationBend) bend);
      }
      aggregationEdges.Remove(aggregationEdge);
      filteredAggregationItems.Remove(aggregationEdge);
      aggregationEdge.graph = null;
      OnEdgeRemoved(new EdgeEventArgs(aggregationEdge));
    }

    private void RemoveAggregationBend(AggregationBend aggregationBend) {
      var bendList = aggregationBend.owner.bends;
      var index = bendList.IndexOf(aggregationBend);
      bendList.Remove(aggregationBend);
      aggregationBend.graph = null;
      OnBendRemoved(new BendEventArgs(aggregationBend, aggregationBend.Owner, index));
    }

    private void RemoveAggregationPort(AggregationPort aggregationPort) {
      foreach (var edge in EdgesAt(aggregationPort).ToList()) {
        RemoveAggregationEdge((AggregationEdge) edge);
      }
      foreach (var label in aggregationPort.Labels.ToList()) {
        RemoveAggregationLabel((AggregationLabel) label);
      }
      aggregationPort.owner.ports.Remove(aggregationPort);
      aggregationPort.graph = null;
      OnPortRemoved(new PortEventArgs(aggregationPort, aggregationPort.Owner));
    }

    private void RemoveAggregationLabel(AggregationLabel aggregationLabel) {
      aggregationLabel.owner.labels.Remove(aggregationLabel);
      aggregationLabel.graph = null;
      OnLabelRemoved(new LabelEventArgs(aggregationLabel, aggregationLabel.Owner));
    }

    public override IListEnumerable<IEdge> EdgesAt(IPortOwner owner, AdjacencyTypes type = AdjacencyTypes.All) {
      if (!Contains(owner)) {
        throw new ArgumentException("Owner is not in this graph", "owner");
      }

      switch (type) {
        case AdjacencyTypes.None:
          return ListEnumerable<IEdge>.Empty;
        case AdjacencyTypes.Incoming:
          return new ListEnumerable<IEdge>(Edges.Where(edge => owner.Ports.Contains(edge.TargetPort)));
        case AdjacencyTypes.Outgoing:
          return new ListEnumerable<IEdge>(Edges.Where(edge => owner.Ports.Contains(edge.SourcePort)));
        default:
        case AdjacencyTypes.All:
          return new ListEnumerable<IEdge>(Edges.Where(edge =>
              owner.Ports.Contains(edge.SourcePort) || owner.Ports.Contains(edge.TargetPort)));
      }
    }

    public override IListEnumerable<IEdge> EdgesAt(IPort port, AdjacencyTypes type = AdjacencyTypes.All) {
      if (!Contains(port)) {
        throw new ArgumentException("Port is not in this graph.", "port");
      }

      switch (type) {
        case AdjacencyTypes.None:
          return ListEnumerable<IEdge>.Empty;
        case AdjacencyTypes.Incoming:
          return new ListEnumerable<IEdge>(Edges.Where(edge => port == edge.TargetPort));
        case AdjacencyTypes.Outgoing:
          return new ListEnumerable<IEdge>(Edges.Where(edge => port == edge.SourcePort));
        default:
        case AdjacencyTypes.All:
          return new ListEnumerable<IEdge>(Edges.Where(edge => port == edge.SourcePort || port == edge.TargetPort));
      }
    }

    public override void SetEdgePorts(IEdge edge, IPort sourcePort, IPort targetPort) {
      if (!Contains(edge)) {
        throw new ArgumentException("Not in Graph", "edge");
      }
      if (!Contains(sourcePort)) {
        throw new ArgumentException("Not in Graph", "sourcePort");
      }
      if (!Contains(targetPort)) {
        throw new ArgumentException("Not in Graph", "targetPort");
      }

      if (edge is AggregationEdge) {
        throw new NotSupportedException("Cannot set ports of aggregation edge " + edge);
      }
      if (sourcePort is AggregationPort) {
        throw new InvalidOperationException("Cannot reconnect original edge to " + sourcePort + ".");
      }
      if (targetPort is AggregationPort) {
        throw new InvalidOperationException("Cannot reconnect original edge to " + targetPort + ".");
      }
      base.SetEdgePorts(edge, sourcePort, targetPort);
    }

    public override bool Contains(IModelItem item) {
      var node = item as AggregationNode;
      if (node != null) {
        return node.graph == this && AggregationItemPredicate(node);
      }
      var edge = item as AggregationEdge;
      if (edge != null) {
        return edge.graph == this && AggregationItemPredicate(edge);
      }
      var port = item as AggregationPort;
      if (port != null) {
        return port.graph == this && Contains(port.Owner);
      }
      var label = item as AggregationLabel;
      if (label != null) {
        return label.graph == this && Contains(label.Owner);
      }
      var bend = item as AggregationBend;
      if (bend != null) {
        return bend.graph == this && Contains(bend.Owner);
      }
      return filteredGraph.Contains(item);
    }

    public override void SetNodeLayout(INode node, RectD layout) {
      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph.", "node");
      }
      if (double.IsNaN(layout.X) || double.IsNaN(layout.Y) || double.IsNaN(layout.Width) ||
          double.IsNaN(layout.Height)) {
        throw new ArgumentException("The layout must not contain a NaN value.", "layout");
      }

      var aggregationNode = node as AggregationNode;
      if (aggregationNode != null) {
        var oldLayout = aggregationNode.Layout.ToRectD();
        aggregationNode.layout.Reshape(layout);
        OnNodeLayoutChanged(aggregationNode, oldLayout);
      } else {
        base.SetNodeLayout(node, layout);
      }
    }

    public override IPort AddPort(IPortOwner owner, IPortLocationModelParameter locationParameter = null,
        IPortStyle style = null,
        object tag = null) {
      if (!Contains(owner)) {
        throw new ArgumentException("Owner is not in this graph.", "owner");
      }
      if (owner is AggregationEdge) {
        throw new ArgumentException("Edge ports are not supported for aggregated edges", "owner");
      }

      var aggregationNode = owner as AggregationNode;
      if (aggregationNode != null) {
        var portLocationParameter =
            locationParameter ?? AggregationNodeDefaults.Ports.GetLocationParameterInstance(owner);
        var portStyle = style ?? AggregationNodeDefaults.Ports.GetStyleInstance(owner);
        var aggregationPort = new AggregationPort(this) {
            owner = aggregationNode, LocationParameter = portLocationParameter, Tag = tag, Style = portStyle
        };
        aggregationNode.ports.Add(aggregationPort);

        OnPortAdded(new ItemEventArgs<IPort>(aggregationPort));
        return aggregationPort;
      }
      return base.AddPort(owner, locationParameter, style, tag);
    }

    public override void SetPortLocationParameter(IPort port, IPortLocationModelParameter locationParameter) {
      if (port.LocationParameter == locationParameter) {
        return;
      }
      if (!Contains(port)) {
        throw new ArgumentException("Port does not belong to this graph.", "port");
      }
      if (locationParameter == null) {
        throw new ArgumentNullException("locationParameter");
      }
      if (!locationParameter.Supports(port.Owner)) {
        throw new ArgumentException("The parameter does not support this port", "locationParameter");
      }

      var aggregationPort = port as AggregationPort;
      if (aggregationPort != null) {
        var oldParameter = port.LocationParameter;
        aggregationPort.LocationParameter = locationParameter;
        OnPortLocationParameterChanged(
            new ItemChangedEventArgs<IPort, IPortLocationModelParameter>(port, oldParameter));
      } else {
        base.SetPortLocationParameter(port, locationParameter);
      }
    }

    public override IBend AddBend(IEdge edge, PointD location, int index = -1) {
      if (!Contains(edge)) {
        throw new ArgumentException("Edge is not in this graph.", "edge");
      }
      if (double.IsNaN(location.X) || double.IsNaN(location.Y)) {
        throw new ArgumentException("The location must not contain a NaN value.", "location");
      }

      var aggregationEdge = edge as AggregationEdge;
      if (aggregationEdge != null) {
        var aggregationBend =
            new AggregationBend(this) { owner = aggregationEdge, location = new MutablePoint(location) };
        var bendList = aggregationEdge.bends;
        if (index < 0) {
          bendList.Add(aggregationBend);
        } else {
          bendList.Insert(index, aggregationBend);
        }
        OnBendAdded(new ItemEventArgs<IBend>(aggregationBend));
        return aggregationBend;
      }
      return base.AddBend(edge, location, index);
    }

    public override void SetBendLocation(IBend bend, PointD location) {
      if (bend.Location.Equals(location)) {
        return;
      }
      if (!Contains(bend)) {
        throw new ArgumentException("Edge is not in this graph.", "bend");
      }
      if (double.IsNaN(location.X) || double.IsNaN(location.Y)) {
        throw new ArgumentException("The location must not contain a NaN value.", "location");
      }

      var aggregationBend = bend as AggregationBend;
      if (aggregationBend != null) {
        var oldLocation = aggregationBend.Location.ToPointD();
        aggregationBend.location.Relocate(location);
        OnBendLocationChanged(bend, oldLocation);
      } else {
        base.SetBendLocation(bend, location);
      }
    }

    public override ILabel AddLabel(ILabelOwner owner, string text, ILabelModelParameter layoutParameter = null,
        ILabelStyle style = null,
        SizeD? preferredSize = null, object tag = null) {
      var labelOwner = owner as AggregationLabelOwner;
      if (labelOwner != null) {
        if (!Contains(owner)) {
          throw new ArgumentException("Owner is not in this graph.", "owner");
        }
        if (preferredSize.HasValue &&
            (double.IsNaN(preferredSize.Value.Width) || double.IsNaN(preferredSize.Value.Height))) {
          throw new ArgumentException("The size must not contain a NaN value.", "preferredSize");
        }

        var labelModelParameter = layoutParameter ?? GetLabelModelParameter(labelOwner);
        var labelStyle = style ?? GetLabelStyle(labelOwner);
        var labelPreferredSize = preferredSize ??
                                 this.CalculateLabelPreferredSize(labelOwner, text, labelModelParameter, labelStyle);

        var aggregationLabel = new AggregationLabel(this) {
            owner = labelOwner,
            Text = text,
            LayoutParameter = labelModelParameter,
            PreferredSize = labelPreferredSize,
            Tag = tag,
            Style = labelStyle
        };

        labelOwner.labels.Add(aggregationLabel);

        OnLabelAdded(new ItemEventArgs<ILabel>(aggregationLabel));
        return aggregationLabel;
      }
      return base.AddLabel(owner, text, layoutParameter, style, preferredSize, tag);
    }

    private ILabelModelParameter GetLabelModelParameter(AggregationLabelOwner owner) {
      if (owner is AggregationNode) {
        return AggregationNodeDefaults.Labels.GetLayoutParameterInstance(owner);
      } else if (owner is AggregationEdge) {
        return AggregationEdgeDefaults.Labels.GetLayoutParameterInstance(owner);
      } else {
        var aggregationPort = owner as AggregationPort;
        if (aggregationPort != null) {
          if (aggregationPort.Owner is INode) {
            return AggregationNodeDefaults.Ports.Labels.GetLayoutParameterInstance(owner);
          } else {
            return AggregationEdgeDefaults.Ports.Labels.GetLayoutParameterInstance(owner);
          }
        }
      }
      // won't happen
      return null;
    }

    private ILabelStyle GetLabelStyle(AggregationLabelOwner owner) {
      if (owner is AggregationNode) {
        return AggregationNodeDefaults.Labels.GetStyleInstance(owner);
      } else if (owner is AggregationEdge) {
        return AggregationEdgeDefaults.Labels.GetStyleInstance(owner);
      } else {
        var aggregationPort = owner as AggregationPort;
        if (aggregationPort != null) {
          if (aggregationPort.Owner is INode) {
            return AggregationNodeDefaults.Ports.Labels.GetStyleInstance(owner);
          } else {
            return AggregationEdgeDefaults.Ports.Labels.GetStyleInstance(owner);
          }
        }
      }
      // won't happen
      return null;
    }

    public override void SetLabelText(ILabel label, string text) {
      if (String.Equals(label.Text, text)) {
        return;
      }
      if (!Contains(label)) {
        throw new ArgumentException("Label is not in this graph.", "label");
      }

      var aggregationLabel = label as AggregationLabel;
      if (aggregationLabel != null) {
        var oldText = label.Text;
        aggregationLabel.Text = text;
        OnLabelTextChanged(new ItemChangedEventArgs<ILabel, string>(label, oldText));
      } else {
        base.SetLabelText(label, text);
      }
    }

    public override void SetLabelPreferredSize(ILabel label, SizeD preferredSize) {
      if (label.PreferredSize.Equals(preferredSize)) {
        return;
      }
      if (!Contains(label)) {
        throw new ArgumentException("Label is not in this graph.", "label");
      }
      if (double.IsNaN(preferredSize.Width) || double.IsNaN(preferredSize.Height)) {
        throw new ArgumentException("The size must not contain a NaN value.", "preferredSize");
      }

      var aggregationLabel = label as AggregationLabel;
      if (aggregationLabel != null) {
        var oldPreferredSize = label.PreferredSize;
        aggregationLabel.PreferredSize = preferredSize;
        OnLabelPreferredSizeChanged(new ItemChangedEventArgs<ILabel, SizeD>(label, oldPreferredSize));
      } else {
        base.SetLabelPreferredSize(label, preferredSize);
      }
    }

    public override void SetLabelLayoutParameter(ILabel label, ILabelModelParameter layoutParameter) {
      if (label.LayoutParameter == layoutParameter) {
        return;
      }
      if (!Contains(label)) {
        throw new ArgumentException("Label does not belong to this graph.", "label");
      }
      if (layoutParameter == null) {
        throw new ArgumentNullException("layoutParameter");
      }
      if (!layoutParameter.Supports(label)) {
        throw new ArgumentException("The parameter does not support the label.", "layoutParameter");
      }

      var aggregationLabel = label as AggregationLabel;
      if (aggregationLabel != null) {
        var oldParameter = label.LayoutParameter;
        aggregationLabel.LayoutParameter = layoutParameter;
        OnLabelLayoutParameterChanged(new ItemChangedEventArgs<ILabel, ILabelModelParameter>(label, oldParameter));
      } else {
        base.SetLabelLayoutParameter(label, layoutParameter);
      }
    }

    public override void SetStyle(INode node, INodeStyle style) {
      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph.", "node");
      }

      var aggregationNode = node as AggregationNode;
      if (aggregationNode != null) {
        if (!ReferenceEquals(aggregationNode.Style, style)) {
          var oldStyle = aggregationNode.Style;
          aggregationNode.Style = style;
          OnNodeStyleChanged(new ItemChangedEventArgs<INode, INodeStyle>(node, oldStyle));
        }
      } else {
        base.SetStyle(node, style);
      }
    }

    public override void SetStyle(ILabel label, ILabelStyle style) {
      if (!Contains(label)) {
        throw new ArgumentException("Label is not in this graph.", "label");
      }

      var aggregationLabel = label as AggregationLabel;
      if (aggregationLabel != null) {
        if (!ReferenceEquals(aggregationLabel.Style, style)) {
          var oldStyle = aggregationLabel.Style;
          aggregationLabel.Style = style;
          OnLabelStyleChanged(new ItemChangedEventArgs<ILabel, ILabelStyle>(label, oldStyle));
        }
      } else {
        base.SetStyle(label, style);
      }
    }

    public override void SetStyle(IEdge edge, IEdgeStyle style) {
      if (!Contains(edge)) {
        throw new ArgumentException("Edge is not in this graph.", "edge");
      }

      var aggregationEdge = edge as AggregationEdge;
      if (aggregationEdge != null) {
        if (!ReferenceEquals(aggregationEdge.Style, style)) {
          var oldStyle = aggregationEdge.Style;
          aggregationEdge.Style = style;
          OnEdgeStyleChanged(new ItemChangedEventArgs<IEdge, IEdgeStyle>(edge, oldStyle));
        }
      } else {
        base.SetStyle(edge, style);
      }
    }

    public override void SetStyle(IPort port, IPortStyle style) {
      if (!Contains(port)) {
        throw new ArgumentException("Port is not in this graph.", "port");
      }

      var aggregationPort = port as AggregationPort;
      if (aggregationPort != null) {
        if (!ReferenceEquals(aggregationPort.Style, style)) {
          var oldStyle = aggregationPort.Style;
          aggregationPort.Style = style;
          OnPortStyleChanged(new ItemChangedEventArgs<IPort, IPortStyle>(port, oldStyle));
        }
      } else {
        base.SetStyle(port, style);
      }
    }

    public override IListEnumerable<INode> GetChildren(INode node) {
      if (node == null) {
        // top-level nodes
        return new ListEnumerable<INode>(Nodes.Where(n => GetParent(n) == null));
      }

      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph.", "node");
      }

      var aggregationNode = node as AggregationNode;
      if (aggregationNode != null) {
        return new ListEnumerable<INode>(aggregationNode.children ?? Enumerable.Empty<INode>());
      }

      return new ListEnumerable<INode>(base.GetChildren(node).Concat(aggregationNodes.Where(an => an.parent == node)));
    }

    public override INode GetParent(INode node) {
      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph.", "node");
      }

      var aggregationNode = node as AggregationNode;
      if (aggregationNode != null) {
        return aggregationNode.parent;
      }

      var aggregationNodeParent =
          aggregationNodes.FirstOrDefault(parent => parent.children != null && parent.children.Contains(node));
      if (aggregationNodeParent != null) {
        return aggregationNodeParent;
      }

      return base.GetParent(node);
    }

    public override void SetParent(INode node, INode parent) {
      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph.", "node");
      }
      if (parent != null && !Contains(parent)) {
        throw new ArgumentException("Parent is not in this graph.", "parent");
      }

      var oldParent = GetParent(node);
      var oldAggregationParent = oldParent as AggregationNode;
      if (oldAggregationParent != null && oldAggregationParent.children != null) {
        oldAggregationParent.children.Remove(node);
      }

      if (node is AggregationNode || parent is AggregationNode) {
        if (!(node is AggregationNode) && !(oldParent is AggregationNode)) {
          // if neither node nor oldParent are AggregationNode, notify WrappedGraph that this relationship is no longer
          // valid
          base.SetParent(node, null);
        }

        if (!IsGroupNode(parent)) {
          SetIsGroupNode(parent, true);
        }

        var aggregationNode = node as AggregationNode;
        if (aggregationNode != null) {
          aggregationNode.parent = node;
        }
        var aggregationParent = parent as AggregationNode;
        if (aggregationParent != null) {
          aggregationParent.children.Add(node);
        }

        OnParentChanged(new NodeEventArgs(node, oldParent, IsGroupNode(node)));
      } else {
        base.SetParent(node, parent);
      }
    }

    public override void SetIsGroupNode(INode node, bool isGroupNode) {
      if (node == null) {
        if (!isGroupNode) {
          throw new InvalidOperationException("Cannot make the root a non-group node.");
        }
        // root stays a group node
        return;
      }
      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph.", "node");
      }

      var aggregationNode = node as AggregationNode;
      if (aggregationNode != null) {
        if (isGroupNode && aggregationNode.children == null) {
          aggregationNode.children = new List<INode>();
          OnIsGroupNodeChanged(new NodeEventArgs(node, GetParent(node), false));
        } else if (!isGroupNode && aggregationNode.children != null) {
          if (aggregationNode.children.Count > 0) {
            throw new InvalidOperationException(
                "Cannot set the type of the node to non-group as long as it has children.");
          }
          aggregationNode.children = null;
          OnIsGroupNodeChanged(new NodeEventArgs(node, GetParent(node), true));
        }
      } else {
        base.SetIsGroupNode(node, isGroupNode);
      }
    }

    public override bool IsGroupNode(INode node) {
      if (node == null) {
        // null represents the root which is always a group node 
        return true;
      }

      if (!Contains(node)) {
        throw new ArgumentException("Node is not in this graph", "node");
      }

      var aggregationNode = node as AggregationNode;
      if (aggregationNode != null) {
        return aggregationNode.children == null;
      }
      return base.IsGroupNode(node);
    }

    public override IEdge CreateEdge(INode source, INode target, IEdgeStyle style = null, object tag = null) {
      if (!Contains(source)) {
        throw new ArgumentException("Cannot create edge from a node that doesn't belong to this graph.", "source");
      }
      if (!Contains(target)) {
        throw new ArgumentException("Cannot create edge to a node that doesn't belong to this graph.", "target");
      }

      if (source is AggregationNode || target is AggregationNode) {
        var sourcePort = AddPort(source);
        var targetPort = AddPort(target);
        return CreateEdge(sourcePort, targetPort, style, tag);
      }
      return base.CreateEdge(source, target, style, tag);
    }

    public override IEdge CreateEdge(IPort sourcePort, IPort targetPort, IEdgeStyle style = null, object tag = null) {
      if (!Contains(sourcePort)) {
        throw new ArgumentException("Cannot create edge from a port that doesn't belong to this graph.", "sourcePort");
      }
      if (!Contains(targetPort)) {
        throw new ArgumentException("Cannot create edge to a port that doesn't belong to this graph.", "targetPort");
      }

      if (sourcePort is AggregationPort || targetPort is AggregationPort) {
        var aggregationEdge = new AggregationEdge(this, new List<IEdge>()) {
            SourcePort = sourcePort,
            TargetPort = targetPort,
            Style = style ?? AggregationEdgeDefaults.GetStyleInstance(),
            Tag = tag
        };
        aggregationEdges.Add(aggregationEdge);
        OnEdgeCreated(new ItemEventArgs<IEdge>(aggregationEdge));
        return aggregationEdge;
      }
      return base.CreateEdge(sourcePort, targetPort, style, tag);
    }

    /// <summary>
    /// Does not raise EdgeCreated event!!
    /// </summary>
    private AggregationEdge CreateAggregationEdge(IPort sourcePort, IPort targetPort, object tag) {
      var aggregationEdge = new AggregationEdge(this, new List<IEdge>()) {
          SourcePort = sourcePort,
          TargetPort = targetPort,
          Style = AggregationEdgeDefaults.GetStyleInstance(),
          Tag = tag
      };

      aggregationEdges.Add(aggregationEdge);
      return aggregationEdge;
    }

    #endregion

    #region ILookupDecorator

    public override object Lookup(Type type) {
      return lookupDecorator.Lookup(type);
    }

    public void AddLookup(IContextLookupChainLink lookup) {
      lookupDecorator.AddLookup(typeof(IGraph), lookup);
    }

    public void RemoveLookup(IContextLookupChainLink lookup) {
      lookupDecorator.RemoveLookup(typeof(IGraph), lookup);
    }

    private object BaseLookup(Type type) {
      return base.Lookup(type);
    }

    private object DelegateLookup(AggregationItem aggregationItem, Type type) {
      return lookupDecorator.DelegateLookup(aggregationItem, type);
    }

    /// <summary>
    /// An ILookupDecorator implementation that contains its own lookup chains.
    /// </summary>
    /// <remarks>
    /// New chain links are added to the chains of this decorator as well as to the decorator of the
    /// <see cref="GraphWrapperBase.WrappedGraph"/>.
    /// </remarks>
    private sealed class AggregateLookupDecorator : ILookup, ILookupDecorator
    {
      private ILookupDecorator wrappedDecorator;
      private readonly AggregateGraphWrapper graph;

      private readonly LookupChain graphLookupChain = new LookupChain();
      private readonly LookupChain nodeLookupChain = new LookupChain();
      private readonly LookupChain edgeLookupChain = new LookupChain();
      private readonly LookupChain bendLookupChain = new LookupChain();
      private readonly LookupChain portLookupChain = new LookupChain();
      private readonly LookupChain labelLookupChain = new LookupChain();

      public AggregateLookupDecorator(AggregateGraphWrapper graph) {
        this.graph = graph;

        graphLookupChain.Add(new GraphFallBackLookup());

        nodeLookupChain.Add(new ItemFallBackLookup());
        nodeLookupChain.Add(new ItemDefaultLookup(DefaultGraph.DefaultNodeLookup));
        nodeLookupChain.Add(new BlockReshapeAndPositionHandlerLookup());

        edgeLookupChain.Add(new ItemFallBackLookup());
        edgeLookupChain.Add(new ItemDefaultLookup(DefaultGraph.DefaultEdgeLookup));

        bendLookupChain.Add(new ItemFallBackLookup());
        bendLookupChain.Add(new ItemDefaultLookup(DefaultGraph.DefaultBendLookup));

        portLookupChain.Add(new ItemFallBackLookup());
        portLookupChain.Add(new ItemDefaultLookup(DefaultGraph.DefaultPortLookup));

        labelLookupChain.Add(new ItemFallBackLookup());
        labelLookupChain.Add(new ItemDefaultLookup(DefaultGraph.DefaultLabelLookup));
      }

      public bool CanDecorate(Type t) {
        if (t == typeof(INode) || t == typeof(IEdge) || t == typeof(IBend) || t == typeof(IPort)
            || t == typeof(ILabel) || t == typeof(IModelItem) || t == typeof(IGraph)) {
          return wrappedDecorator == null || wrappedDecorator.CanDecorate(t);
        }
        return false;
      }

      public void AddLookup(Type t, IContextLookupChainLink lookup) {
        if (t == typeof(INode)) {
          nodeLookupChain.Add(lookup);
        } else if (t == typeof(IEdge)) {
          edgeLookupChain.Add(lookup);
        } else if (t == typeof(IBend)) {
          bendLookupChain.Add(lookup);
        } else if (t == typeof(IPort)) {
          portLookupChain.Add(lookup);
        } else if (t == typeof(ILabel)) {
          labelLookupChain.Add(lookup);
        } else if (t == typeof(IGraph)) {
          graphLookupChain.Add(lookup);
        } else {
          throw new ArgumentException("Cannot decorate type " + t);
        }

        if (wrappedDecorator != null) {
          wrappedDecorator.AddLookup(t, lookup);
        }
      }

      public void RemoveLookup(Type t, IContextLookupChainLink lookup) {
        if (t == typeof(INode)) {
          nodeLookupChain.Remove(lookup);
        } else if (t == typeof(IEdge)) {
          edgeLookupChain.Remove(lookup);
        } else if (t == typeof(IBend)) {
          bendLookupChain.Remove(lookup);
        } else if (t == typeof(IPort)) {
          portLookupChain.Remove(lookup);
        } else if (t == typeof(ILabel)) {
          labelLookupChain.Remove(lookup);
        } else if (t == typeof(IGraph)) {
          graphLookupChain.Remove(lookup);
        }

        if (wrappedDecorator != null) {
          wrappedDecorator.RemoveLookup(t, lookup);
        }
      }

      public object Lookup(Type type) {
        if (type == typeof(ILookupDecorator)) {
          wrappedDecorator = (ILookupDecorator) graph.BaseLookup(type);
          return this;
        }
        if (type == typeof(LookupChain)) {
          return graphLookupChain;
        }

        var lookup = graph.GetLookup();
        if (lookup != null) {
          return lookup.Lookup(type);
        }

        return graphLookupChain.Lookup(graph, type);
      }

      public object DelegateLookup(IModelItem item, Type type) {
        if (item is INode) {
          return nodeLookupChain.Lookup(item, type);
        } else if (item is IEdge) {
          return edgeLookupChain.Lookup(item, type);
        } else if (item is ILabel) {
          return labelLookupChain.Lookup(item, type);
        } else if (item is IBend) {
          return bendLookupChain.Lookup(item, type);
        } else if (item is IPort) {
          return portLookupChain.Lookup(item, type);
        } else {
          return null;
        }
      }
    }

    private sealed class GraphFallBackLookup : ContextLookupChainLinkBase
    {
      public override object Lookup(object item, Type type) {
        return ((AggregateGraphWrapper) item).BaseLookup(type) ?? base.Lookup(item, type);
      }
    }

    private sealed class ItemFallBackLookup : ContextLookupChainLinkBase
    {
      public override object Lookup(object item, Type type) {
        return ((AggregationItem) item).InnerLookup(type) ?? base.Lookup(item, type);
      }
    }

    private sealed class BlockReshapeAndPositionHandlerLookup : ContextLookupChainLinkBase
    {
      public override object Lookup(object item, Type type) {
        // The default implementations of IPositionHandler and IReshapeHandler don't support AggregationNode, which is
        // why moving and reshaping such nodes is not supported by default.
        if (type == typeof(IPositionHandler) || type == typeof(IReshapeHandler)) {
          return null;
        }
        return base.Lookup(item, type);
      }
    }

    private sealed class ItemDefaultLookup : ContextLookupChainLinkBase
    {
      private readonly IContextLookup defaultLookup;

      public ItemDefaultLookup(IContextLookup defaultLookup) {
        this.defaultLookup = defaultLookup;
      }

      public override object Lookup(object item, Type type) {
        return defaultLookup.Lookup(item, type) ?? base.Lookup(item, type);
      }
    }

    private abstract class ContextLookupChainLinkBase : IContextLookupChainLink
    {
      private IContextLookup nextLink;

      public virtual object Lookup(object item, Type type) {
        return nextLink != null ? nextLink.Lookup(item, type) : null;
      }

      public void SetNext(IContextLookup next) {
        this.nextLink = next;
      }
    }

    #endregion

    #region IModelItems

    private void OnTagChanged(IModelItem item, object oldTag) {
      if (item is INode) {
        OnNodeTagChanged(new ItemChangedEventArgs<INode, object>((INode) item, oldTag));
      } else if (item is IEdge) {
        OnEdgeTagChanged(new ItemChangedEventArgs<IEdge, object>((IEdge) item, oldTag));
      } else if (item is ILabel) {
        OnLabelTagChanged(new ItemChangedEventArgs<ILabel, object>((ILabel) item, oldTag));
      } else if (item is IPort) {
        OnPortTagChanged(new ItemChangedEventArgs<IPort, object>((IPort) item, oldTag));
      } else if (item is IBend) {
        OnBendTagChanged(new ItemChangedEventArgs<IBend, object>((IBend) item, oldTag));
      }
    }

    /// <summary>
    /// A simple INode implementation for aggregation nodes.
    /// </summary>
    private sealed class AggregationNode : AggregationLabelPortOwner, INode
    {
      internal readonly IList<INode> aggregatedNodes;
      internal IMutableRectangle layout;
      internal IList<INode> children;
      internal INode parent;

      public AggregationNode(AggregateGraphWrapper graph, IList<INode> aggregatedNodes) : base(graph) {
        this.aggregatedNodes = aggregatedNodes;
      }

      public IRectangle Layout {
        get { return layout; }
      }

      public INodeStyle Style { get; internal set; }

      public override object InnerLookup(Type type) {
        if (type == typeof(INodeStyle)) {
          return Style;
        }
        if (type.IsInstanceOfType(layout)) {
          return layout;
        }
        return base.InnerLookup(type);
      }

      public override string ToString() {
        return Labels.Count > 0
            ? base.ToString()
            : "Aggregation Node (" + aggregatedNodes.Count + ") [" + layout.X + ", " + layout.Y + ", " + layout.Width +
              ", " + layout.Height + "]";
      }
    }

    /// <summary>
    /// A simple IEdge implementation for aggregation edges.
    /// </summary>
    private sealed class AggregationEdge : AggregationLabelPortOwner, IEdge
    {
      internal readonly IList<IEdge> aggregatedEdges;

      internal readonly IList<AggregationBend> bends = new List<AggregationBend>();

      private ListEnumerable<IBend> bendEnumerable;

      public IListEnumerable<IBend> Bends {
        get {
          if (bendEnumerable == null) {
            bendEnumerable = new ListEnumerable<IBend>(bends);
          }
          return bendEnumerable;
        }
      }

      public IPort SourcePort { get; internal set; }

      public IPort TargetPort { get; internal set; }

      public IEdgeStyle Style { get; internal set; }

      public AggregationEdge(AggregateGraphWrapper graph, IList<IEdge> aggregatedEdges) : base(graph) {
        this.aggregatedEdges = aggregatedEdges;
      }

      public override object InnerLookup(Type type) {
        if (type == typeof(IEdgeStyle)) {
          return Style;
        }
        if (type.IsInstanceOfType(bends)) {
          return bends;
        }
        return base.InnerLookup(type);
      }

      public override string ToString() {
        if (labels.Count > 0) {
          return base.ToString();
        } else {
          if ((SourcePort != null && SourcePort.Owner is IEdge) || (TargetPort != null && TargetPort.Owner is IEdge)) {
            return "Aggregation Edge [ At another Edge ]";
          } else {
            return "Aggregation Edge (" + aggregatedEdges.Count + ") [" + SourcePort + " -> " + TargetPort + "]";
          }
        }
      }
    }

    /// <summary>
    /// A simple IBend implementation for bends of <see cref="AggregationEdge"/>s.
    /// </summary>
    private sealed class AggregationBend : AggregationItem, IBend
    {
      internal AggregationEdge owner;
      internal IMutablePoint location;

      public IEdge Owner {
        get { return owner; }
      }

      public IPoint Location {
        get { return location; }
      }

      public AggregationBend(AggregateGraphWrapper graph) : base(graph) { }

      public override object InnerLookup(Type type) {
        if (type.IsInstanceOfType(location)) {
          return location;
        }
        return base.InnerLookup(type);
      }

      public override string ToString() {
        return "Aggregation Bend [" + location.X + ", " + location.Y + "]";
      }
    }

    /// <summary>
    /// A simple IPort implementation for ports of <see cref="AggregationLabelPortOwner"/>.
    /// </summary>
    private sealed class AggregationPort : AggregationLabelOwner, IPort
    {
      internal AggregationLabelPortOwner owner;

      public IPortOwner Owner {
        get { return owner; }
      }

      public IPortStyle Style { get; internal set; }

      public IPortLocationModelParameter LocationParameter { get; internal set; }

      public AggregationPort(AggregateGraphWrapper graph) : base(graph) { }

      public override object InnerLookup(Type type) {
        if (type == typeof(IPortStyle)) {
          return Style;
        }
        if (type == typeof(IPortLocationModelParameter)) {
          return LocationParameter;
        }
        if (type == typeof(IPoint)) {
          return this.GetDynamicLocation();
        }
        return base.InnerLookup(type);
      }

      public override string ToString() {
        var text = "Aggregation Port [";

        try {
          text += "Location: " + this.GetLocation() + "; ";
        } catch (Exception) {
          // ignored
        }
        return text + "Parameter: " + LocationParameter + "; Owner: " + owner + "]";
      }
    }

    /// <summary>
    /// A simple ILabel implementation for labels of <see cref="AggregationLabelOwner"/>.
    /// </summary>
    private sealed class AggregationLabel : AggregationItem, ILabel
    {
      internal AggregationLabelOwner owner;

      public ILabelStyle Style { get; internal set; }

      public SizeD PreferredSize { get; internal set; }

      public ILabelOwner Owner {
        get { return owner; }
      }

      public string Text { get; internal set; }

      public ILabelModelParameter LayoutParameter { get; internal set; }

      public AggregationLabel(AggregateGraphWrapper graph) : base(graph) { }

      public override object InnerLookup(Type type) {
        if (type == typeof(ILabelStyle)) {
          return Style;
        }
        if (type == typeof(ILabelModelParameter)) {
          return LayoutParameter;
        }
        return base.InnerLookup(type);
      }

      public override string ToString() {
        return "Aggregation Label [\"" + Text + "\"; Owner: " + owner + "]";
      }
    }

    private abstract class AggregationLabelPortOwner : AggregationLabelOwner, IPortOwner
    {
      internal readonly IList<IPort> ports = new List<IPort>();
      private IListEnumerable<IPort> enumerable;

      public IListEnumerable<IPort> Ports {
        get {
          if (enumerable == null) {
            enumerable = new ListEnumerable<IPort>(ports);
          }
          return enumerable;
        }
      }

      protected AggregationLabelPortOwner(AggregateGraphWrapper graph) : base(graph) { }
    }

    private abstract class AggregationLabelOwner : AggregationItem, ILabelOwner
    {
      internal readonly IList<ILabel> labels = new List<ILabel>();
      private IListEnumerable<ILabel> enumerable;

      public IListEnumerable<ILabel> Labels {
        get {
          if (enumerable == null) {
            enumerable = new ListEnumerable<ILabel>(labels);
          }
          return enumerable;
        }
      }

      protected AggregationLabelOwner(AggregateGraphWrapper graph) : base(graph) { }

      public override string ToString() {
        return labels.Count > 0 ? labels[0].Text : "ILabelOwner";
      }
    }

    private abstract class AggregationItem : IModelItem
    {
      internal AggregateGraphWrapper graph;
      private object tag;

      protected AggregationItem(AggregateGraphWrapper graph) {
        this.graph = graph;
      }

      public object Tag {
        get { return tag; }
        set {
          var oldTag = tag;
          tag = value;
          if (graph != null) {
            graph.OnTagChanged(this, oldTag);
          }
        }
      }

      public object Lookup(Type type) {
        return graph != null ? graph.DelegateLookup(this, type) : null;
      }

      public virtual object InnerLookup(Type type) {
        if (type.IsInstanceOfType(this)) {
          return this;
        }
        return null;
      }
    }

    #endregion
  }
}
