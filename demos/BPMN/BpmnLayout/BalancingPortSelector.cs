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
using yWorks.Layout.Graph;
using yWorks.Layout.Grid;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  ///   This port candidate selection tries to balance the edges on each node and distribute them to the four node sides.
  /// </summary>
  /// <remarks>
  ///   To balance the edge distribution it calculates edges that should be on a
  ///   <see cref="HierarchicalLayout.CriticalEdgePriorityDataKey">critical path</see>
  ///   and define the flow of the diagram. Furthermore, it sets
  ///   <see cref="ItemFactory.SetSelectedPortCandidate">selected port candidates</see> on
  ///   the non-flow sides of the nodes.
  /// </remarks>
  internal class BalancingPortSelector : PortCandidateSelector
  {
    // free port candidates that are assigned as selected port candidates
    private static readonly LayoutPortCandidate PortCandidateRight = LayoutPortCandidate.CreateFree(PortSides.Right);
    private static readonly LayoutPortCandidate PortCandidateLeft = LayoutPortCandidate.CreateFree(PortSides.Left);

    // a core optimizer that is executed before this one.
    private readonly IPortCandidateSelector coreSelector;

    private SameLayerData sameLayerData;
    private IMapper<Edge, LaneCrossing> edge2LaneCrossing;
    private IMapper<Node, LaneAlignment> node2LaneAlignment;

    public BalancingPortSelector(IPortCandidateSelector coreSelector) {
      this.coreSelector = coreSelector;
    }

    public override void SelectAfterLayering(LayoutGraph graph, HierarchicalLayoutContext layoutContext) {
      if (coreSelector != null) {
        coreSelector.SelectAfterLayering(graph, layoutContext);
      }
    }

    public override void SelectAfterSequencing(LayoutGraph graph, HierarchicalLayoutContext layoutContext) {
      if (coreSelector != null) {
        coreSelector.SelectAfterSequencing(graph, layoutContext);
      }
      base.SelectAfterSequencing(graph, layoutContext);
    }

    protected override void SelectAfterSequencingAtNode(
        Node node,
        Comparison<Edge> inEdgeOrder,
        Comparison<Edge> outEdgeOrder,
        LayoutGraph graph,
        HierarchicalLayoutContext layoutContext) {
      // this callback isn't used
    }

    protected override SameLayerData InsertSameLayerStructures(
        LayoutGraph graph,
        HierarchicalLayoutContext layoutContext) {
      // store the SameLayerData for later use
      sameLayerData = base.InsertSameLayerStructures(graph, layoutContext);
      return sameLayerData;
    }

    protected override void SelectAfterSequencing(
        Comparison<Edge> inEdgeOrder,
        Comparison<Edge> outEdgeOrder,
        LayoutGraph graph,
        HierarchicalLayoutContext layoutContext) {
      edge2LaneCrossing = new DictionaryMapper<Edge, LaneCrossing>();
      node2LaneAlignment = new DictionaryMapper<Node, LaneAlignment>();

      var criticalEdges = graph.CreateEdgeDataMap<double>();

      // determine whether an edge crosses a swimlane border and if so in which direction
      foreach (var edge in graph.Edges) {
        var originalEdge = GetOriginalEdge(edge, layoutContext);

        // now we have a 'real' edge with valid source and target nodes
        var originalSourceId = GetLaneId(originalEdge.Source, layoutContext);
        var originalTargetId = GetLaneId(originalEdge.Target, layoutContext);
        var crossing = LaneCrossing.None;
        if (originalSourceId != originalTargetId) {
          // check if we need to flip the sides because edge and original edge have different directions
          var flipSides = edge.Source != originalEdge.Source;
          var sourceId = flipSides ? originalTargetId : originalSourceId;
          var targetId = flipSides ? originalSourceId : originalTargetId;

          crossing = sourceId > targetId ? LaneCrossing.ToLeft : LaneCrossing.ToRight;
        }
        edge2LaneCrossing[edge] = crossing;
      }

      // determine basic node alignment
      foreach (var n in graph.Nodes) {
        var alignment = CalculateLaneAlignment(n);
        node2LaneAlignment[n] = alignment;
      }

      foreach (var n in graph.Nodes) {
        // sort the edges with the provided comparer
        n.SortInEdges(inEdgeOrder);
        n.SortOutEdges(outEdgeOrder);

        // calculate 'critical' in- and out-edges whose nodes should be aligned in flow
        var inEdges = n.InEdges;
        var bestInEdge = n.InDegree > 0 ? GetBestFlowEdge(inEdges, layoutContext, graph) : null;
        var outEdges = n.OutEdges;
        var bestOutEdge = n.OutDegree > 0 ? GetBestFlowEdge(outEdges, layoutContext, graph) : null;
        if (bestInEdge != null) {
          criticalEdges[bestInEdge] += 1;
        }
        if (bestOutEdge != null) {
          criticalEdges[bestOutEdge] += 1;
        }
        if (n.Degree <= 4) {
          // should usually be the case and we can distribute each edge to its own side

          // remember which node side is already taken by an in- or out-edge
          var leftTakenByInEdge = false;
          var rightTakenByInEdge = false;
          var leftTakenByOutEdge = false;
          var rightTakenByOutEdge = false;

          if (n.InDegree > 0 && n.OutDegree < 3) {
            // if there are at least three out-edges, we distribute those first, otherwise we start with the in-edges
            var firstInEdge = inEdges.First;
            var lastInEdge = inEdges.Last;
            if (GetLaneCrossing(firstInEdge) == LaneCrossing.ToRight &&
                (n.InDegree > 1 || IsSameLayerEdge(firstInEdge, layoutContext))) {
              // the first in-edge comes from the left and is either a same-layer-edge or there are other in-edges
              SelectLeftSideCandidate(firstInEdge, false, layoutContext);
              leftTakenByInEdge = true;
            }
            if (!leftTakenByInEdge || n.OutDegree < 2) {
              // don't use left and right side for in-edges if there are at least 2 out-edges
              if (GetLaneCrossing(lastInEdge) == LaneCrossing.ToLeft &&
                  (n.InDegree > 1 || IsSameLayerEdge(lastInEdge, layoutContext))) {
                // the last in-edge comes from right and is either
                // a same-layer edge or there are other in-edges
                SelectRightSideCandidate(lastInEdge, false, layoutContext);
                rightTakenByInEdge = true;
              }
            }
          }

          if (n.OutDegree > 0) {
            var firstOutEdge = outEdges.First;
            var lastOutEdge = outEdges.Last;

            if (!leftTakenByInEdge) {
              // the left side is still free
              if (BpmnLayout.IsBoundaryInterrupting(firstOutEdge, graph) ||
                  GetLaneCrossing(firstOutEdge) == LaneCrossing.ToLeft &&
                  (n.OutDegree > 1 || IsSameLayerEdge(firstOutEdge, layoutContext))) {
                // the first out-edge is either boundary interrupting or goes to left and
                // is either a same-layer-edge or there are other out-edges
                SelectLeftSideCandidate(firstOutEdge, true, layoutContext);
                leftTakenByOutEdge = true;
              } else if (rightTakenByInEdge && n.OutDegree >= 2 && !IsSameLayerEdge(outEdges[1], layoutContext)) {
                // the right side is already taken but we have more than one out-edge.
                // if the second out-edge is a same-layer-edge, constraining the firstOutEdge could lead to
                // no in-flow edge
                SelectLeftSideCandidate(firstOutEdge, true, layoutContext);
                leftTakenByOutEdge = true;
              }
            }
            if (!rightTakenByInEdge) {
              // the right side is still free
              if (GetLaneCrossing(lastOutEdge) == LaneCrossing.ToRight &&
                  (n.OutDegree > 1 || IsSameLayerEdge(lastOutEdge, layoutContext))) {
                // the last out-edge goes to the right and
                // is either a same-layer-edge or there are other out-edges
                SelectRightSideCandidate(lastOutEdge, true, layoutContext);
                rightTakenByOutEdge = true;
              } else if (leftTakenByInEdge && n.OutDegree >= 2 &&
                  !IsSameLayerEdge(outEdges[outEdges.Count - 2], layoutContext)) {
                // the left side is already taken, but we have more than one out-edge.
                // if the second last out-edge is a same-layer-edge, constraining the lastOutEdge could lead to
                // no in-flow edge
                SelectRightSideCandidate(lastOutEdge, true, layoutContext);
                rightTakenByOutEdge = true;
              }
            }
          }

          // distribute remaining in-edges
          if (n.InDegree == 2 && !(rightTakenByInEdge || leftTakenByInEdge)) {
            // two in-edges but none distributed, yet
            if (bestInEdge == inEdges.First && !rightTakenByOutEdge) {
              // first in-edge is in-flow edge and right side is still free
              SelectRightSideCandidate(inEdges.Last, false, layoutContext);
              rightTakenByInEdge = true;
            } else if (bestInEdge == inEdges.Last && !leftTakenByOutEdge) {
              // last in-edge is in-flow edge and left side is still free
              SelectLeftSideCandidate(inEdges.First, false, layoutContext);
              leftTakenByInEdge = true;
            }
          } else if (n.InDegree == 3 && !(rightTakenByInEdge && leftTakenByInEdge) &&
              !IsSameLayerEdge(inEdges[1], layoutContext)) {
            // three in-edges but not both sides taken, yet and the middle edge is no same-layer-edge
            if (!rightTakenByOutEdge) {
              // if not already taken, constrain the last in-edge to the right
              SelectRightSideCandidate(inEdges.Last, false, layoutContext);
              rightTakenByInEdge = true;
            }
            if (!leftTakenByOutEdge) {
              // if not already taken, constrain the first in-edge to the left
              SelectLeftSideCandidate(inEdges.First, false, layoutContext);
              leftTakenByInEdge = true;
            }
          }

          // distribute remaining out-edges
          if (n.OutDegree == 2 && !(rightTakenByOutEdge || leftTakenByOutEdge)) {
            // two out-edges but none distributed, yet
            if (bestOutEdge == outEdges.First && !rightTakenByInEdge) {
              // first out-edge is in-flow edge and right side is still free
              SelectRightSideCandidate(outEdges.Last, true, layoutContext);
              rightTakenByOutEdge = true;
            } else if (bestOutEdge == outEdges.Last && !leftTakenByInEdge) {
              // last out-edge is in-flow edge and left side is still free
              SelectLeftSideCandidate(outEdges.First, true, layoutContext);
              leftTakenByOutEdge = true;
            }
          } else if (n.OutDegree == 3 && !(rightTakenByOutEdge && leftTakenByOutEdge) &&
              !IsSameLayerEdge(outEdges[1], layoutContext)) {
            // three out-edges but not both sides taken, yet and the middle edge is no same-layer-edge
            if (!rightTakenByInEdge) {
              // if not already taken, constrain the last out-edge to the right
              SelectRightSideCandidate(outEdges.Last, true, layoutContext);
              rightTakenByOutEdge = true;
            }
            if (!leftTakenByInEdge) {
              // if not already taken, constrain the first out-edge to the left
              SelectLeftSideCandidate(outEdges.First, true, layoutContext);
              leftTakenByOutEdge = true;
            }
          }
        }
      }

      // register the data provider for critical edge paths. It is deregistered again by BpmnLayout itself
      graph.Context.AddItemData(HierarchicalLayout.CriticalEdgePriorityDataKey, criticalEdges);

      sameLayerData = null;
      edge2LaneCrossing = null;
      node2LaneAlignment = null;
    }

    private LaneCrossing GetLaneCrossing(Edge edge) {
      return edge2LaneCrossing[edge];
    }

    private LaneAlignment GetLaneAlignment(Node source) {
      return node2LaneAlignment[source];
    }

    /// <summary>
    ///   Get the <see cref="Edge"/> representing the original edge on the graph.
    /// </summary>
    /// <remarks>
    ///   As the core layout algorithm creates temporary edges for example for same-layer edges and edges spanning
    ///   multiple layers, we need to lookup the original edge of the graph for example as key in data providers.
    /// </remarks>
    private Edge GetOriginalEdge(Edge edge, HierarchicalLayoutContext layoutContext) {
      var originalEdge = sameLayerData.GetOriginalEdge(edge.Source) ??
          // ReSharper disable once ConstantNullCoalescingCondition
          (sameLayerData.GetOriginalEdge(edge.Target) ?? edge);
      var edgeData = layoutContext.GetEdgeContext(originalEdge);
      return edgeData.AssociatedEdge ?? originalEdge;
    }

    /// <summary>
    ///   Returns the best suited edge in <paramref name="edges"/> for use as in-flow edge or <see langword="null"/>
    ///   if no such edge could be found.
    /// </summary>
    private Edge GetBestFlowEdge(IEnumerable<Edge> edges, HierarchicalLayoutContext layoutContext, LayoutGraph graph) {
      var weakCandidates = new List<Edge>();
      var candidates = new List<Edge>();

      foreach (var edge in edges) {
        var originalEdge = GetOriginalEdge(edge, layoutContext);
        if (edge2LaneCrossing[edge] != LaneCrossing.None || BpmnLayout.IsBoundaryInterrupting(originalEdge, graph) ||
            IsSameLayerEdge(originalEdge, layoutContext) || edge.SelfLoop) {
          // an edge should not be aligned if:
          // - it crosses stripe borders
          // - it is boundary interrupting
          // - it is a same-layer edge
          // - it is a self-loop
          continue;
        }
        if (layoutContext.GetEdgeContext(edge).Reversed || !BpmnLayout.IsSequenceFlow(originalEdge, graph)) {
          // it is only a weak candidate if:
          // - it is reversed
          // - it is no sequence flow
          weakCandidates.Add(edge);
        } else {
          candidates.Add(edge);
        }
      }
      if (candidates.Count > 0) {
        // if there are several candidates, choose the one that would keep the LaneAlignment
        // of its source and target node consistent
        candidates.Sort((edge1, edge2) => {
          var ac1 = GetAlignmentConsistency(edge1);
          var ac2 = GetAlignmentConsistency(edge2);
          return ac2 - ac1;
        });
        return candidates[0];
      }
      if (weakCandidates.Count > 0) {
        return weakCandidates[(int) Math.Floor(weakCandidates.Count / 2.0)];
      }
      return null;
    }

    /// <summary>
    ///   Returns how much the <see cref="LaneAlignment"/> of the source and target node is consistent.
    /// </summary>
    /// <remarks>
    ///   The consistency is <c>2</c>, if both nodes have the same alignment.
    ///   It is <c>1</c> if exactly one of the alignments is <see cref="LaneAlignment.None"/>
    ///   and <c>0</c> otherwise.
    /// </remarks>
    private int GetAlignmentConsistency(Edge edge) {
      var sourceLA = GetLaneAlignment(edge.Source);
      var targetLA = GetLaneAlignment(edge.Target);
      var alignmentConsistency = sourceLA == targetLA ? 2 :
          sourceLA == LaneAlignment.None || targetLA == LaneAlignment.None ? 1 : 0;
      return alignmentConsistency;
    }

    /// <summary>
    ///   Sets a <see cref="ItemFactory.SetSelectedPortCandidate">selected left side port candidate</see>
    ///   on <paramref name="source"/> or target side of <paramref name="edge"/>
    /// </summary>
    private static void SelectRightSideCandidate(Edge edge, bool source, HierarchicalLayoutContext layoutContext) {
      if (source) {
        layoutContext.GetEdgeContext(edge)!.SelectedSourcePortCandidate = PortCandidateRight;
      } else {
        layoutContext.GetEdgeContext(edge)!.SelectedTargetPortCandidate = PortCandidateRight;
      }
    }

    /// <summary>
    ///   Sets a <see cref="ItemFactory.SetSelectedPortCandidate">selected right side port candidate</see>
    ///   on <paramref name="source"/> or target side of <paramref name="edge"/>
    /// </summary>
    private static void SelectLeftSideCandidate(Edge edge, bool source, HierarchicalLayoutContext layoutContext) {
      if (source) {
        layoutContext.GetEdgeContext(edge)!.SelectedSourcePortCandidate = PortCandidateLeft;
      } else {
        layoutContext.GetEdgeContext(edge)!.SelectedTargetPortCandidate = PortCandidateLeft;
      }
    }

    /// <summary>
    ///   Returns if the source and target node of the <see cref="GetOriginalEdge">original edge</see> of
    ///   <paramref name="edge"/> are on the same layer.
    /// </summary>
    private bool IsSameLayerEdge(Edge edge, HierarchicalLayoutContext layoutContext) {
      var originalEdge = GetOriginalEdge(edge, layoutContext);
      var sourceNodeData = layoutContext.GetNodeContext(originalEdge.Source);
      var targetNodeData = layoutContext.GetNodeContext(originalEdge.Target);
      return sourceNodeData != null && targetNodeData != null && sourceNodeData.Layer == targetNodeData.Layer;
    }

    /// <summary>
    ///   Determine the alignment of a node in its swimlane depending on the <see cref="LaneCrossing"/>s of its attached
    ///   edges.
    /// </summary>
    private LaneAlignment CalculateLaneAlignment(Node n) {
      var toRightCount = 0;
      var toLeftCount = 0;
      foreach (var edge in n.Edges) {
        var crossing = edge2LaneCrossing[edge];
        if (n == edge.Source) {
          if (crossing == LaneCrossing.ToRight) {
            toRightCount++;
          } else if (crossing == LaneCrossing.ToLeft) {
            toLeftCount++;
          }
        } else {
          if (crossing == LaneCrossing.ToRight) {
            toLeftCount++;
          } else if (crossing == LaneCrossing.ToLeft) {
            toRightCount++;
          }
        }
      }
      if (toLeftCount > toRightCount) {
        return LaneAlignment.Left;
      }
      if (toLeftCount < toRightCount) {
        return LaneAlignment.Right;
      }
      return LaneAlignment.None;
    }

    /// <summary>
    ///   Returns the <see cref="LayoutGrid.Column.Index"/> for <paramref name="node"/>.
    /// </summary>
    private static int GetLaneId(Node node, HierarchicalLayoutContext layoutContext) {
      var nodeData = layoutContext.GetNodeContext(node);
      return nodeData?.Column?.Index ?? -1;
    }

    /// <summary>
    ///   Specifies the alignment of a node in its swimlane.
    /// </summary>
    /// <seealso cref="BalancingPortSelector.CalculateLaneAlignment"/>
    private enum LaneAlignment
    {
      /// <summary>
      ///   The node has no special alignment.
      /// </summary>
      None,

      /// <summary>
      ///   The node is aligned to the left side
      /// </summary>
      Left,

      /// <summary>
      ///   The node is aligned to the right side
      /// </summary>
      Right
    }

    /// <summary>
    ///   Specifies in which direction an edge crosses swimlane borders.
    /// </summary>
    private enum LaneCrossing
    {
      /// <summary>
      ///   The edge doesn't cross a swimlane border.
      /// </summary>
      None,

      /// <summary>
      ///   The edge crosses swimlane borders to the right, so its source node is in a swimlane with a lower
      ///   <see cref="LayoutGrid.Column.Index"/>.
      /// </summary>
      ToRight,

      /// <summary>
      ///   The edge crosses swimlane borders to the left, so its source node is in a swimlane with a higher
      ///   <see cref="LayoutGrid.Column.Index"/>.
      /// </summary>
      ToLeft
    }
  }
}
