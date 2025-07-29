/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Graph.Bpmn.Styles;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Bpmn;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Labeling;
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Layout
{
  /// <summary>
  ///   Specifies custom data for the <see cref="BpmnLayout"/>.
  /// </summary>
  /// <remarks>
  ///   Prepares BPMN layout information provided by the styles for assignment of layout information calculated by
  ///   <see cref="BpmnLayout"/>.
  /// </remarks>
  public class BpmnLayoutData
  {
    private const double MinLabelToLabelDistance = 5;

    /// <summary>
    ///   Determines whether or not start nodes are pulled to the leftmost or topmost layer.
    /// </summary>
    /// <remarks>
    ///   Default value is <see langword="false"/>.
    /// </remarks>
    public virtual bool StartNodesFirst { get; set; }

    /// <summary>
    ///   Determines whether or not message flows have only a weak impact on the layering.
    /// </summary>
    /// <remarks>
    ///   <p>
    ///     Having a weak impact, message flows are more likely to be back edges. This often results in more compact layouts.
    ///   </p>
    ///   <p>
    ///     Default value is <see langword="false"/>.
    ///   </p>
    /// </remarks>
    public virtual bool CompactMessageFlowLayering { get; set; }

    /// <summary>
    ///   The minimum length of edges.
    /// </summary>
    /// <remarks>
    ///   Defaults to <c>20.0</c>.
    /// </remarks>
    public double MinimumEdgeLength { get; set; }

    public BpmnLayoutData() {
      MinimumEdgeLength = 20;
    }

    public LayoutData<INode, IEdge, ILabel, ILabel> Create(IGraph graph, IGraphSelection selection, BpmnLayout layout) {
      var data = graph.CreateGenericLayoutData();
      var hierarchicalLayoutData = layout.HierarchicalLayout.CreateLayoutData(graph);
      var labelData = new GenericLabelingData<INode, IEdge, ILabel, ILabel>();

      // check if only selected elements should be laid out
      var layoutOnlySelection = layout.Scope == Scope.SelectedElements;

      // mark 'flow' edges, i.e. sequence flows, default flows and conditional flows
      data.AddItemCollection(BpmnLayout.SequenceFlowEdgesDataKey).Predicate = IsSequenceFlow;

      // mark boundary interrupting edges for the BalancingPortOptimizer
      data.AddItemCollection(BpmnLayout.BoundaryInterruptingEdgesDataKey).Predicate =
          edge => edge.SourcePort.Style is EventPortStyle;

      // mark conversations, events and gateways so their port locations are adjusted
      data.AddItemCollection(PortLocationAdjuster.AffectedNodesDataKey).Predicate = node =>
          node.Style is ConversationNodeStyle || node.Style is EventNodeStyle || node.Style is GatewayNodeStyle;

      // add margins around nodes with event ports or specific exterior labels so the layout keeps space for the event ports and labels as well
      AddNodeMargins(hierarchicalLayoutData, graph, selection, layoutOnlySelection);

      // add PreferredPlacementDescriptors for labels on sequence, default or conditional flows to place them at source side
      AddEdgeLabelPlacementDescriptors(data);

      // mark nodes, edges and labels as either fixed or affected by the layout and configure port constraints and incremental hints
      MarkFixedAndAffectedItems(data, labelData, hierarchicalLayoutData, selection, layoutOnlySelection);

      // mark associations and message flows as undirected so they have less impact on layering
      hierarchicalLayoutData.EdgeDirectedness.MapperFunction = edge => IsMessageFlow(edge) || IsAssociation(edge) ? 0 : 1;

      // add layer constraints for start events, sub processes and message flows
      AddLayerConstraints(graph, hierarchicalLayoutData);

      // add EdgeLayoutDescriptor to specify minimum edge length for edges
      AddMinimumEdgeLength(MinimumEdgeLength, hierarchicalLayoutData);

      return data.CombineWith(hierarchicalLayoutData).CombineWith(labelData);
    }

    #region Add layer constraints

    private void AddLayerConstraints(
        IGraph graph,
        HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> hierarchicalLayoutData) {
      // use layer constraints via HierarchicalLayoutData
      var layerConstraintData = hierarchicalLayoutData.LayerConstraints;

      foreach (var edge in graph.Edges) {
        if (IsMessageFlow(edge) && !CompactMessageFlowLayering) {
          // message flow layering compaction is disabled, we add a 'weak' same layer constraint, i.e. source node shall be placed at
          // least 0 layers above target node
          layerConstraintData.PlaceInOrder(edge.GetSourceNode(), edge.GetTargetNode(), 0, 1);
        } else if (IsSequenceFlow(edge)) {
          if ((IsSubprocess(edge.GetSourceNode()) && !(edge.SourcePort.Style is EventPortStyle)) ||
              IsSubprocess(edge.GetTargetNode())) {
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
          if (node.Style is EventNodeStyle &&
              ((EventNodeStyle) node.Style).Characteristic == EventCharacteristic.Start && graph.InDegree(node) == 0 &&
              (graph.GetParent(node) == null || graph.GetParent(node).Style is PoolNodeStyle)) {
            layerConstraintData.PlaceAtTop(node);
          }
        }
      }
    }

    private static void AddAboveLayerConstraint(
        LayerConstraintData<INode> layerConstraintData,
        IEdge edge,
        IGraph graph) {
      var sourceNode = edge.GetSourceNode();
      var targetNode = edge.GetTargetNode();

      var sourceNodes = new List<INode>();
      var targetNodes = new List<INode>();
      CollectLeafNodes(graph, sourceNode, sourceNodes);
      CollectLeafNodes(graph, targetNode, targetNodes);
      foreach (var source in sourceNodes) {
        foreach (var target in targetNodes) {
          layerConstraintData.PlaceInOrder(source, target);
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

    private void AddMinimumEdgeLength(
        double minimumEdgeLength,
        HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> hierarchicalLayoutData) {
      // each edge should have a minimum length so that all its labels can be placed on it one
      // after another with a minimum label-to-label distance
      hierarchicalLayoutData.EdgeDescriptors.MapperFunction = edge => {
        var descriptor = new HierarchicalLayoutEdgeDescriptor { RoutingStyleDescriptor = new RoutingStyleDescriptor() };
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
      return activityNodeStyle != null && (activityNodeStyle.ActivityType == ActivityType.SubProcess ||
          activityNodeStyle.ActivityType == ActivityType.EventSubProcess);
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
      return bpmnEdgeStyle.Type == EdgeType.SequenceFlow || bpmnEdgeStyle.Type == EdgeType.DefaultFlow ||
          bpmnEdgeStyle.Type == EdgeType.ConditionalFlow;
    }

    private static bool IsAssociation(IEdge edge) {
      var bpmnEdgeStyle = edge.Style as BpmnEdgeStyle;
      if (bpmnEdgeStyle == null) {
        return false;
      }
      return bpmnEdgeStyle.Type == EdgeType.Association || bpmnEdgeStyle.Type == EdgeType.BidirectedAssociation ||
          bpmnEdgeStyle.Type == EdgeType.DirectedAssociation;
    }

    #endregion

    #region Add Node Margin for Event Ports and exterior node labels

    private static void AddNodeMargins(
        HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> data,
        IGraph graph,
        IGraphSelection selection,
        bool layoutOnlySelection) {
      var nodeMargins = new DictionaryMapper<INode, InsetsD?>();
      foreach (var node in graph.Nodes) {
        var top = 0.0;
        var left = 0.0;
        var bottom = 0.0;
        var right = 0.0;

        // for each port with an EventPortStyle extend the node margin to cover the ports render size
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
            if (IsNodeLabelAffected(graph, selection, label, layoutOnlySelection)) {
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

        nodeMargins[node] = new InsetsD(left, top, right, bottom);
      }
      data.NodeMargins.Mapper = nodeMargins;
    }

    private static bool IsNodeLabelAffected(
        IGraph graph,
        IGraphSelection selection,
        ILabel label,
        bool layoutOnlySelection) {
      var node = label.Owner as INode;
      if (node != null) {
        var isInnerLabel = node.Layout.Contains(label.GetLayout().GetCenter());
        var isPool = node.Style is PoolNodeStyle;
        var isChoreography = node.Style is ChoreographyNodeStyle;
        var isGroupNode = graph.IsGroupNode(node);
        return !isInnerLabel && !isPool && !isChoreography && !isGroupNode &&
            (!layoutOnlySelection || selection.Contains(node));
      }
      return false;
    }

    #endregion

    #region Add PreferredPlacementDescriptors for edge labels

    private static void AddEdgeLabelPlacementDescriptors(GenericLayoutData<INode, IEdge, ILabel, ILabel> data) {
      var atSourcePlacement = new EdgeLabelPreferredPlacement {
          PlacementAlongEdge = LabelAlongEdgePlacements.AtSourcePort,
          EdgeSide = LabelEdgeSides.LeftOfEdge | LabelEdgeSides.RightOfEdge
      };
      data.AddItemMapping(EdgeLabelPreferredPlacement.EdgeLabelPreferredPlacementDataKey).MapperFunction = label => {
        var edgeType = ((BpmnEdgeStyle) ((IEdge) label.Owner).Style).Type;
        if (edgeType == EdgeType.SequenceFlow || edgeType == EdgeType.DefaultFlow ||
            edgeType == EdgeType.ConditionalFlow) {
          // labels on sequence, default and conditional flow edges should be placed at the source side.
          return atSourcePlacement;
        }
        return null;
      };
    }

    #endregion

    #region Mark nodes, edges and labels as fixed or affected

    private static void MarkFixedAndAffectedItems(
        GenericLayoutData<INode, IEdge, ILabel, ILabel> data,
        GenericLabelingData<INode, IEdge, ILabel, ILabel> labelData,
        HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> hierarchicalLayoutData,
        IGraphSelection graphSelection,
        bool layoutOnlySelection) {
      if (layoutOnlySelection) {
        var affectedEdges = Mappers.FromDelegate((IEdge edge) =>
            graphSelection.Contains(edge) || graphSelection.Contains(edge.GetSourceNode()) ||
            graphSelection.Contains(edge.GetTargetNode()));

        // fix ports of unselected edges and edges at event ports
        hierarchicalLayoutData.Ports.SourcePortCandidates.MapperFunction = edge => {
          var side = GetDirection(edge, true);
          return !affectedEdges[edge] || edge.SourcePort.Style is EventPortStyle
              ? new EdgePortCandidates().AddFreeCandidate(side)
              : null;
        };
        hierarchicalLayoutData.Ports.TargetPortCandidates.MapperFunction = edge => {
          var side = GetDirection(edge, false);
          return !affectedEdges[edge] ? new EdgePortCandidates().AddFreeCandidate(side) : null;
        };

        // give core layout hints that selected nodes and edges should be incremental
        hierarchicalLayoutData.IncrementalNodes.Predicate = graphSelection.Contains;
        hierarchicalLayoutData.IncrementalEdges.Predicate = edge => affectedEdges[edge];

        labelData.Scope.NodeLabels.Predicate = label => {
          var node = label.Owner as INode;
          if (node == null) {
            return false;
          }

          var isInnerLabel = node.Layout.Contains(label.GetLayout().GetCenter());
          var isPool = node.Style is PoolNodeStyle;
          var isChoreography = node.Style is ChoreographyNodeStyle;
          return !isInnerLabel && !isPool && !isChoreography && graphSelection.Contains(node);
        };

        labelData.Scope.EdgeLabels.Predicate = label => {
          var edge = label.Owner as IEdge;
          return edge != null && affectedEdges[edge];
        };
      } else {
        // fix source port of edges at event ports
        hierarchicalLayoutData.Ports.SourcePortCandidates.MapperFunction = edge => {
          var side = GetDirection(edge, true);
          return edge.SourcePort.Style is EventPortStyle ? new EdgePortCandidates().AddFreeCandidate(side) : null;
        };

        labelData.Scope.NodeLabels.Predicate = label => {
          var node = label.Owner as INode;
          if (node == null) {
            return false;
          }

          var isInnerLabel = node.Layout.Contains(label.GetLayout().GetCenter());
          var isPool = node.Style is PoolNodeStyle;
          var isChoreography = node.Style is ChoreographyNodeStyle;
          return !isInnerLabel && !isPool && !isChoreography;
        };
        labelData.Scope.EdgeLabels.Predicate = label => label.Owner is IEdge;
      }
    }

    private static PortSides GetDirection([NotNull] IEdge edge, bool atSource) {
      var port = atSource ? edge.SourcePort : edge.TargetPort;
      var node = port.Owner as INode;
      if (node == null) {
        return PortSides.Any;
      }
      var relPortLocation = port.GetLocation() - node.Layout.GetCenter();

      // calculate relative port position scaled by the node size
      var sdx = relPortLocation.X / (node.Layout.Width / 2);
      var sdy = relPortLocation.Y / (node.Layout.Height / 2);

      if (Math.Abs(sdx) > Math.Abs(sdy)) {
        // left or right
        return sdx < 0 ? PortSides.Left : PortSides.Right;
      }
      if (Math.Abs(sdx) < Math.Abs(sdy)) {
        return sdy < 0 ? PortSides.Top : PortSides.Bottom;
      }

      // port is somewhere at the diagonals of the node bounds
      // so we can't decide the port side based on the port location
      // better use the attached segment to decide on the port side
      return GetDirectionFromSegment(edge, atSource);
    }

    private static PortSides GetDirectionFromSegment([NotNull] IEdge edge, bool atSource) {
      var port = atSource ? edge.SourcePort : edge.TargetPort;
      var opposite = atSource ? edge.TargetPort : edge.SourcePort;
      var from = port.GetLocation();

      var to = edge.Bends.Count > 0 ? (atSource ? edge.Bends[0] : edge.Bends.Last()).Location : opposite.GetLocation();

      var dx = to.X - from.X;
      var dy = to.Y - from.Y;
      if (Math.Abs(dx) > Math.Abs(dy)) {
        // left or right
        return dx < 0 ? PortSides.Left : PortSides.Right;
      }
      return dy < 0 ? PortSides.Top : PortSides.Bottom;
    }

    #endregion
  }
}
