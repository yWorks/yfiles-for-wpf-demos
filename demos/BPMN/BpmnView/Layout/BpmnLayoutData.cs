/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Graph.Bpmn.Styles;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Bpmn;
using yWorks.Layout.Hierarchic;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Layout
{
  /// <summary>
  /// Specifies custom data for the <see cref="BpmnLayout"/>.
  /// </summary>
  /// <remarks>
  /// Prepares BPMN layout information provided by the styles for assignment of layout information calculated by <see cref="BpmnLayout"/>.
  /// </remarks>
  public class BpmnLayoutData : HierarchicLayoutData
  {
    private const double MinLabelToLabelDistance = 5;

    /// <summary>
    /// Determines whether or not start node are pulled to the leftmost or topmost layer.</summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    public virtual bool StartNodesFirst { get; set; }

    /// <summary>
    /// Determines whether or not message flows have only weak impact on the layering.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Having weak impact, message flows are more likely to be back edges. This often results in more compact layouts.
    /// </p>
    /// <p>
    /// Default value is <see langword="false"/>.
    /// </p>
    /// </remarks>
    public virtual bool CompactMessageFlowLayering { get; set; }

    /// <summary>
    /// The minimum length of edges.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>20.0</c>.
    /// </remarks>
    public double MinimumEdgeLength { get; set; }

    public BpmnLayoutData() {
      MinimumEdgeLength = 20;
    }

    protected override void Apply(LayoutGraphAdapter adapter, ILayoutAlgorithm layout, CopiedLayoutGraph layoutGraph) {
      var graph = adapter.AdaptedGraph;

      // check if only selected elements should be laid out
      var layoutOnlySelection = layout is BpmnLayout && ((BpmnLayout) layout).Scope == Scope.SelectedElements;

      // mark 'flow' edges, i.e. sequence flows, default flows and conditional flows
      adapter.AddDataProvider(BpmnLayout.SequenceFlowEdgesDpKey, Mappers.FromDelegate<IEdge, bool>(IsSequenceFlow));

      // mark boundary interrupting edges for the BalancingPortOptimizer
      adapter.AddDataProvider(BpmnLayout.BoundaryInterruptingEdgesDpKey, Mappers.FromDelegate((IEdge edge) => edge.SourcePort.Style is EventPortStyle));

      // mark conversations, events and gateways so their port locations are adjusted
      adapter.AddDataProvider(PortLocationAdjuster.AffectedNodesDpKey,
        Mappers.FromDelegate((INode node) => (node.Style is ConversationNodeStyle || node.Style is EventNodeStyle || node.Style is GatewayNodeStyle)));

      // add NodeHalos around nodes with event ports or specific exterior labels so the layout keeps space for the event ports and labels as well
      AddNodeHalos(adapter, graph, layoutOnlySelection);

      // add PreferredPlacementDescriptors for labels on sequence, default or conditional flows to place them at source side
      AddEdgeLabelPlacementDescriptors(adapter);

      // mark nodes, edges and labels as either fixed or affected by the layout and configure port constraints and incremental hints
      MarkFixedAndAffectedItems(adapter, layoutOnlySelection);

      // mark associations and message flows as undirected so they have less impact on layering
      EdgeDirectedness.Delegate = edge => (IsMessageFlow(edge) || IsAssociation(edge)) ? 0 : 1;

      // add layer constraints for start events, sub processes and message flows
      AddLayerConstraints(graph);

      // add EdgeLayoutDescriptor to specify minimum edge length for edges
      AddMinimumEdgeLength(MinimumEdgeLength);

      base.Apply(adapter, layout, layoutGraph);
    }

    #region Add layer constraints

    private void AddLayerConstraints(IGraph graph) {
      // use layer constraints via HierarchicLayoutData
      var layerConstraintData = LayerConstraints;

      foreach (var edge in graph.Edges) {
        if (IsMessageFlow(edge) && !CompactMessageFlowLayering) {
          // message flow layering compaction is disabled, we add a 'weak' same layer constraint, i.e. source node shall be placed at
          // least 0 layers above target node
          layerConstraintData.PlaceAbove(edge.GetTargetNode(), edge.GetSourceNode(), 0, 1);
        } else if (IsSequenceFlow(edge)) {
          if ((IsSubprocess(edge.GetSourceNode()) && !(edge.SourcePort.Style is EventPortStyle))
              || IsSubprocess(edge.GetTargetNode())) {
            // For edges to or from a subprocess that are not attached to an (interrupting) event port, the flow should be considered.
            // If the subprocess is a group node, any constraints to it are ignored so we have to add the constraints to the content nodes
            // of the subprocess
            AddAboveLayerConstraint(layerConstraintData, edge, graph);
          }
        }
      }

      // if start events should be pulled to the first layer, add PlaceNodeAtTop constraint.
      if (StartNodesFirst) {
        foreach (var node in graph.Nodes) {
          if (node.Style is EventNodeStyle
              && ((EventNodeStyle) node.Style).Characteristic == EventCharacteristic.Start
              && graph.InDegree(node) == 0
              && (graph.GetParent(node) == null || graph.GetParent(node).Style is PoolNodeStyle)) {
            layerConstraintData.PlaceAtTop(node);
          }
        }
      }
    }

    private static void AddAboveLayerConstraint(LayerConstraintData layerConstraintData, IEdge edge, IGraph graph) {
      var sourceNode = edge.GetSourceNode();
      var targetNode = edge.GetTargetNode();

      var sourceNodes = new List<INode>();
      var targetNodes = new List<INode>();
      CollectLeafNodes(graph, sourceNode, sourceNodes);
      CollectLeafNodes(graph, targetNode, targetNodes);
      foreach (var source in sourceNodes) {
        foreach (var target in targetNodes) {
          layerConstraintData.PlaceAbove(target, source);
        }
      }
    }

    private static void CollectLeafNodes(IGraph graph, INode node, ICollection<INode> leafNodes) {
      var children = graph.GetChildren(node);
      if (children.Count > 0) {
        foreach (var child in children) {
          CollectLeafNodes(graph, child, leafNodes);
        }
      } else {
        leafNodes.Add(node);
      }
    }

    #endregion

    #region Add minimum edge length

    private void AddMinimumEdgeLength(double minimumEdgeLength) {
      // each edge should have a minimum length so that all its labels can be placed on it one
      // after another with a minimum label-to-label distance
      EdgeLayoutDescriptors.Delegate = edge => {
        var descriptor = new EdgeLayoutDescriptor {
          RoutingStyle = new RoutingStyle(EdgeRoutingStyle.Orthogonal)
        };
        double minLength = 0;
        foreach (var label in edge.Labels) {
          var labelSize = label.GetLayout().GetBounds();
          minLength += Math.Max(labelSize.Width, labelSize.Height);
        }
        if (edge.Labels.Count > 1) {
          minLength += (edge.Labels.Count - 1) * MinLabelToLabelDistance;
        }
        descriptor.MinimumLength = Math.Max(minLength, minimumEdgeLength);
        return descriptor;
      };
    }

    #endregion

    #region Utility method to check the type of nodes or edges

    private static bool IsSubprocess(INode node) {
      var activityNodeStyle = node.Style as ActivityNodeStyle;
      return activityNodeStyle != null && (activityNodeStyle.ActivityType == ActivityType.SubProcess || activityNodeStyle.ActivityType == ActivityType.EventSubProcess);
    }

    private static bool IsMessageFlow(IEdge edge) {
      var bpmnEdgeStyle = edge.Style as BpmnEdgeStyle;
      return bpmnEdgeStyle != null && bpmnEdgeStyle.Type == EdgeType.MessageFlow;
    }

    private static bool IsSequenceFlow(IEdge edge) {
      var bpmnEdgeStyle = edge.Style as BpmnEdgeStyle;
      if (bpmnEdgeStyle == null) {
        return false;
      }
      return bpmnEdgeStyle.Type == EdgeType.SequenceFlow 
        || bpmnEdgeStyle.Type == EdgeType.DefaultFlow 
        || bpmnEdgeStyle.Type == EdgeType.ConditionalFlow;
    }

    private static bool IsAssociation(IEdge edge) {
      var bpmnEdgeStyle = edge.Style as BpmnEdgeStyle;
      if (bpmnEdgeStyle == null) {
        return false;
      }
      return bpmnEdgeStyle.Type == EdgeType.Association 
        || bpmnEdgeStyle.Type == EdgeType.BidirectedAssociation 
        || bpmnEdgeStyle.Type == EdgeType.DirectedAssociation;
    }

    #endregion

    #region Add Node Halos for Event Ports and exterior node labels

    private static void AddNodeHalos(LayoutGraphAdapter adapter, IGraph graph, bool layoutOnlySelection) {
      var nodeHalos = new DictionaryMapper<INode, NodeHalo>();
      foreach (var node in graph.Nodes) {
        var top = 0.0;
        var left = 0.0;
        var bottom = 0.0;
        var right = 0.0;

        // for each port with an EventPortStyle extend the node halo to cover the ports render size
        foreach (var port in node.Ports) {
          var eventPortStyle = port.Style as EventPortStyle;
          if (eventPortStyle != null) {
            var renderSize = eventPortStyle.RenderSize;
            var location = port.GetLocation();
            top = Math.Max(top, node.Layout.Y - location.Y - renderSize.Height / 2);
            left = Math.Max(left, node.Layout.X - location.X - renderSize.Width / 2);
            bottom = Math.Max(bottom, location.Y + renderSize.Height / 2 - node.Layout.GetMaxY());
            right = Math.Max(right, location.X + renderSize.Width / 2 - node.Layout.GetMaxX());
          }
        }

        // for each node without incoming or outgoing edges reserve space for laid out exterior labels
        if (graph.InDegree(node) == 0 || graph.OutDegree(node) == 0) {
          foreach (var label in node.Labels) {
            if (IsNodeLabelAffected(label, adapter, layoutOnlySelection)) {
              var labelBounds = label.GetLayout().GetBounds();
              if (graph.InDegree(node) == 0) {
                left = Math.Max(left, labelBounds.Width);
                top = Math.Max(top, labelBounds.Height);
              }
              if (graph.OutDegree(node) == 0) {
                right = Math.Max(right, labelBounds.Width);
                bottom = Math.Max(bottom, labelBounds.Height);
              }
            }
          }
        }

        nodeHalos[node] = NodeHalo.Create(top, left, bottom, right);
      }
      adapter.AddDataProvider(NodeHalo.NodeHaloDpKey, nodeHalos);
    }

    private static bool IsNodeLabelAffected(ILabel label, LayoutGraphAdapter adapter, bool layoutOnlySelection) {
      var node = label.Owner as INode;
      if (node != null) {
        var isInnerLabel = node.Layout.Contains(label.GetLayout().GetCenter());
        bool isPool = node.Style is PoolNodeStyle;
        bool isChoreography = node.Style is ChoreographyNodeStyle;
        var isGroupNode = adapter.AdaptedGraph.IsGroupNode(node);
        return !isInnerLabel && !isPool && !isChoreography && !isGroupNode && (!layoutOnlySelection || adapter.SelectionModel.IsSelected(node));
      }
      return false;
    }

    #endregion

    #region Add PreferredPlacementDescriptors for edge labels

    private static void AddEdgeLabelPlacementDescriptors(LayoutGraphAdapter adapter) {
      var atSourceDescriptor = new PreferredPlacementDescriptor {
        PlaceAlongEdge = LabelPlacements.AtSourcePort,
        SideOfEdge = LabelPlacements.LeftOfEdge | LabelPlacements.RightOfEdge,
      };
      adapter.AddDataProvider(LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey,
        Mappers.FromDelegate((ILabel label) => {
          var edgeType = ((BpmnEdgeStyle)((IEdge)label.Owner).Style).Type;
          if (edgeType == EdgeType.SequenceFlow || edgeType == EdgeType.DefaultFlow || edgeType == EdgeType.ConditionalFlow) {
            // labels on sequence, default and conditional flow edges should be placed at the source side.
            return atSourceDescriptor;
          }
          return null;
        }));
    }

    #endregion

    #region Mark nodes, edges and labels as fixed or affected

    private void MarkFixedAndAffectedItems(LayoutGraphAdapter adapter, bool layoutOnlySelection) {
      if (layoutOnlySelection) {
        var affectedEdges = Mappers.FromDelegate((IEdge edge) =>
          adapter.SelectionModel.IsSelected(edge)
          || adapter.SelectionModel.IsSelected(edge.GetSourceNode()) ||
          adapter.SelectionModel.IsSelected(edge.GetTargetNode()));
        adapter.AddDataProvider(LayoutKeys.AffectedEdgesDpKey, affectedEdges);

        // fix ports of unselected edges and edges at event ports
        adapter.AddDataProvider(PortConstraintKeys.SourcePortConstraintDpKey, Mappers.FromDelegate<IEdge, PortConstraint>(
          edge =>
            (!affectedEdges[edge] || edge.SourcePort.Style is EventPortStyle)
              ? PortConstraint.Create(GetSide(edge, true))
              : null));
        adapter.AddDataProvider(PortConstraintKeys.TargetPortConstraintDpKey, Mappers.FromDelegate<IEdge, PortConstraint>(
          edge => !affectedEdges[edge] ? PortConstraint.Create(GetSide(edge, false)) : null));

        // give core layout hints that selected nodes and edges should be incremental
        IncrementalHints.ContextDelegate = (item, factory) => {
          if (item is INode && adapter.SelectionModel.IsSelected(item)) {
            return factory.CreateLayerIncrementallyHint(item);
          } else if (item is IEdge && affectedEdges[(IEdge)item]) {
            return factory.CreateSequenceIncrementallyHint(item);
          }
          return null;
        };
        adapter.AddDataProvider(BpmnLayout.AffectedLabelsDpKey, Mappers.FromDelegate<ILabel, bool>(label => {
          var edge = label.Owner as IEdge;
          if (edge != null) {
            return affectedEdges[edge];
          }
          var node = label.Owner as INode;
          if (node != null) {
            var isInnerLabel = node.Layout.Contains(label.GetLayout().GetCenter());
            bool isPool = node.Style is PoolNodeStyle;
            bool isChoreography = node.Style is ChoreographyNodeStyle;
            return !isInnerLabel && !isPool && !isChoreography && adapter.SelectionModel.IsSelected(node);
          }
          return false;
        }));
      } else {
        // fix source port of edges at event ports
        adapter.AddDataProvider(PortConstraintKeys.SourcePortConstraintDpKey, Mappers.FromDelegate<IEdge, PortConstraint>(
          edge => edge.SourcePort.Style is EventPortStyle ? PortConstraint.Create(GetSide(edge, true)) : null));

        adapter.AddDataProvider(BpmnLayout.AffectedLabelsDpKey, Mappers.FromDelegate<ILabel, bool>(label => {
          if (label.Owner is IEdge) {
            return true;
          }
          var node = label.Owner as INode;
          if (node != null) {
            var isInnerLabel = node.Layout.Contains(label.GetLayout().GetCenter());
            bool isPool = node.Style is PoolNodeStyle;
            bool isChoreography = node.Style is ChoreographyNodeStyle;
            return !isInnerLabel && !isPool && !isChoreography;
          }
          return false;
        }));
      }
    }

    private static PortSide GetSide([NotNull] IEdge edge, bool atSource) {
      var port = atSource ? edge.SourcePort : edge.TargetPort;
      var node = port.Owner as INode;
      if (node == null) {
        return PortSide.Any;
      }
      var relPortLocation = port.GetLocation() - node.Layout.GetCenter();

      // calculate relative port position scaled by the node size
      var sdx = relPortLocation.X / (node.Layout.Width / 2);
      var sdy = relPortLocation.Y / (node.Layout.Height / 2);

      if (Math.Abs(sdx) > Math.Abs(sdy)) {
        // east or west
        return sdx < 0 ? PortSide.West : PortSide.East;
      } else if (Math.Abs(sdx) < Math.Abs(sdy)) {
        return sdy < 0 ? PortSide.North : PortSide.South;
      }

      // port is somewhere at the diagonals of the node bounds
      // so we can't decide the port side based on the port location
      // better use the attached segment to decide on the port side
      return GetSideFromSegment(edge, atSource);
    }

    private static PortSide GetSideFromSegment([NotNull] IEdge edge, bool atSource) {
      var port = atSource ? edge.SourcePort : edge.TargetPort;
      var opposite = atSource ? edge.TargetPort : edge.SourcePort;
      var from = port.GetLocation();

      var to = edge.Bends.Count > 0
        ? (atSource ? edge.Bends[0] : edge.Bends.Last()).Location
        : opposite.GetLocation();

      var dx = to.X - from.X;
      var dy = to.Y - from.Y;
      if (Math.Abs(dx) > Math.Abs(dy)) {
        // east or west
        return dx < 0 ? PortSide.West : PortSide.East;
      } else {
        return dy < 0 ? PortSide.North : PortSide.South;
      }
    }

    #endregion
  }
}
