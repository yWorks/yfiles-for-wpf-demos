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
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout.Hierarchic;

namespace yWorks.Layout.Bpmn
{

  /// <summary>
  /// This port optimizer tries to balance the edges on each node and distribute them to the four node sides.
  /// </summary>
  /// <remarks>
  /// To balances the edge distribution it calculates edges that should be on a <see cref="HierarchicLayout.CriticalEdgePriorityDpKey">critical path</see>
  /// and define the flow of the diagram. Furthermore it uses <see cref="IItemFactory.SetTemporaryPortConstraint">temporary port constraints</see> on 
  /// the non-flow sides of the nodes.
  /// </remarks>
  internal class BalancingPortOptimizer : PortConstraintOptimizerBase
  {
    // weak port constraints that are assigned as temporary port constraints
    private static readonly PortConstraint portConstraintEast = PortConstraint.Create(PortSide.East);
    private static readonly PortConstraint portConstraintWest = PortConstraint.Create(PortSide.West);

    // a core optimizer that is executed before this one.
    private readonly IPortConstraintOptimizer coreOptimizer;

    private SameLayerData sameLayerData;
    private IEdgeMap edge2LaneCrossing;
    private INodeMap node2LaneAlignment;

    public BalancingPortOptimizer(IPortConstraintOptimizer coreOptimizer) {
      this.coreOptimizer = coreOptimizer;
    }

    public override void OptimizeAfterLayering(LayoutGraph graph, ILayers layers, ILayoutDataProvider ldp, IItemFactory itemFactory) {
      if (coreOptimizer != null) {
        coreOptimizer.OptimizeAfterLayering(graph, layers, ldp, itemFactory);
      }
    }

    public override void OptimizeAfterSequencing(LayoutGraph graph, ILayers layers, ILayoutDataProvider ldp, IItemFactory itemFactory) {
      if (coreOptimizer != null) {
        coreOptimizer.OptimizeAfterSequencing(graph, layers, ldp, itemFactory);
      }
      base.OptimizeAfterSequencing(graph, layers, ldp, itemFactory);
    }

    protected override void OptimizeAfterSequencing(Node node, IComparer<object> inEdgeOrder, 
      IComparer<object> outEdgeOrder, LayoutGraph graph, ILayoutDataProvider ldp, IItemFactory itemFactory) {
      // this callback isn't used
    }

    protected override SameLayerData InsertSameLayerStructures(LayoutGraph graph, ILayers layers, ILayoutDataProvider ldp, IItemFactory itemFactory) {
      // store the SameLayerData for later use
      sameLayerData = base.InsertSameLayerStructures(graph, layers, ldp, itemFactory);
      return sameLayerData;
    }

    protected override void OptimizeAfterSequencing(IComparer<object> inEdgeOrder, IComparer<object> outEdgeOrder, LayoutGraph graph, ILayers layers,
      ILayoutDataProvider ldp, IItemFactory itemFactory) {
      edge2LaneCrossing = Maps.CreateHashedEdgeMap();
      node2LaneAlignment = Maps.CreateHashedNodeMap();

      var criticalEdges = Maps.CreateHashedEdgeMap();

      // determine whether an edge crosses a swim lane border and if so in which direction
      foreach (var edge in graph.Edges) {
        var originalEdge = GetOriginalEdge(edge, ldp);

        // now we have a 'real' edge with valid valid source and target nodes
        var originalSourceId = GetLaneId(originalEdge.Source, ldp);
        var originalTargetId = GetLaneId(originalEdge.Target, ldp);
        LaneCrossing crossing = LaneCrossing.None;
        if (originalSourceId != originalTargetId) {
          // check if we need to flip the sides because edge and original edge have different directions
          var flipSides = edge.Source != originalEdge.Source;
          var sourceId = flipSides ? originalTargetId : originalSourceId;
          var targetId = flipSides ? originalSourceId : originalTargetId;

          crossing = sourceId > targetId ? LaneCrossing.ToWest : LaneCrossing.ToEast;
        }
        edge2LaneCrossing.Set(edge, crossing);
      }

      // determine basic node alignment
      foreach (var n in graph.Nodes) {
        LaneAlignment alignment = CalculateLaneAlignment(n);
        node2LaneAlignment.Set(n, alignment);
      }

      foreach (var n in graph.Nodes) {
        // sort the edges with the provided comparer
        n.SortInEdges(inEdgeOrder);
        n.SortOutEdges(outEdgeOrder);

        // calculate 'critical' in and out-edges whose nodes should be aligned in flow
        var bestInEdge = n.InDegree > 0 ? GetBestFlowEdge(n.InEdges, ldp, graph) : null;
        var bestOutEdge = n.OutDegree > 0 ? GetBestFlowEdge(n.OutEdges, ldp, graph) : null;
        if (bestInEdge != null) {
          criticalEdges.SetDouble(bestInEdge, criticalEdges.GetDouble(bestInEdge) + 0.5);
        }
        if (bestOutEdge != null) {
          criticalEdges.SetDouble(bestOutEdge, criticalEdges.GetDouble(bestOutEdge) + 0.5);
        }
        if (n.Degree <= 4) {
          // should usually be the case and we can distribute each edge to its own side
          
          // remember which node side is already taken by an in- or out-edge
          bool westTakenByInEdge = false;
          bool eastTakenByInEdge = false;
          bool westTakenByOutEdge = false;
          bool eastTakenByOutEdge = false;

          if (n.InDegree > 0 && n.OutDegree < 3) {
            // if there are at least three out-edges, we distribute those first, otherwise we start with the in-edges

            var firstInEdge = n.FirstInEdge;
            var lastInEdge = n.LastInEdge;
            if (GetLaneCrossing(firstInEdge) == LaneCrossing.ToEast
              && (n.InDegree > 1 || IsSameLayerEdge(firstInEdge, ldp))) {
              // the first in-edge comes from west and is either a same layer edge or there are other in-edges
              ConstrainWest(firstInEdge, false, itemFactory);
              westTakenByInEdge = true;
            }
            if (!westTakenByInEdge || n.OutDegree < 2) {
              // don't use west and east side for in-edges if there are at least 2 out-edges
              if (GetLaneCrossing(lastInEdge) == LaneCrossing.ToWest 
                  && (n.InDegree > 1 || IsSameLayerEdge(lastInEdge, ldp))) {
                // the last in-edge comes from east and is either
                // a same-layer edge or there are other in-edges
                ConstrainEast(lastInEdge, false, itemFactory);
                eastTakenByInEdge = true;
              }
            }
          }

          if (n.OutDegree > 0) {
            var firstOutEdge = n.FirstOutEdge;
            var lastOutEdge = n.LastOutEdge;

            if (!westTakenByInEdge) {
              // the west side is still free
              if (BpmnLayout.IsBoundaryInterrupting(firstOutEdge, graph) 
                || (GetLaneCrossing(firstOutEdge) == LaneCrossing.ToWest) 
                    && (n.OutDegree > 1 || IsSameLayerEdge(firstOutEdge, ldp))) {
                // the first out-edge is either boundary interrupting or goes to west and 
                // is either a same layer edge or there are other out-edges
                ConstrainWest(firstOutEdge, true, itemFactory);
                westTakenByOutEdge = true;
              } else if (eastTakenByInEdge && n.OutDegree >= 2 && !IsSameLayerEdge(firstOutEdge.NextOutEdge, ldp)) {
                // the east side is already taken but we have more then one out edge.
                // if the second out edge is a same layer edge, constraining the firstOutEdge could lead to
                // no in-flow edge
                ConstrainWest(firstOutEdge, true, itemFactory);
                westTakenByOutEdge = true;
              }
            }
            if (!eastTakenByInEdge) {
              // the east side is still free
              if (GetLaneCrossing(lastOutEdge) == LaneCrossing.ToEast
                && (n.OutDegree > 1 || IsSameLayerEdge(lastOutEdge, ldp))) {
                // the last out-edge goes to east and 
                // is either a same layer edge or there are other out-edges
                ConstrainEast(lastOutEdge, true, itemFactory);
                eastTakenByOutEdge = true;
              } else if (westTakenByInEdge && n.OutDegree >= 2 && !IsSameLayerEdge(lastOutEdge.PrevOutEdge, ldp)) {
                // the west side is already taken but we have more then one out edge.
                // if the second last out edge is a same layer edge, constraining the lastOutEdge could lead to
                // no in-flow edge
                ConstrainEast(lastOutEdge, true, itemFactory);
                eastTakenByOutEdge = true;
              }
            }
          }

          // distribute remaining in-edges
          if (n.InDegree == 2
              && !(eastTakenByInEdge || westTakenByInEdge)) {
            // two in-edges but none distributed, yet
            if (bestInEdge == n.FirstInEdge && !eastTakenByOutEdge) {
              // first in-edge is in-flow edge and east side is still free
              ConstrainEast(n.LastInEdge, false, itemFactory);
              eastTakenByInEdge = true;
            } else if (bestInEdge == n.LastInEdge && !westTakenByOutEdge) {
              // last in-edge is in-flow edge and west side is still free
              ConstrainWest(n.FirstInEdge, false, itemFactory);
              westTakenByInEdge = true;
            }
          } else if (n.InDegree == 3 
            && !(eastTakenByInEdge && westTakenByInEdge) 
            && !(IsSameLayerEdge(n.FirstInEdge.NextInEdge, ldp))) {
            // three in-edges but not both sides taken, yet and the middle edge is no same layer edge
            if (!eastTakenByOutEdge) {
              // if not already taken, constraint the last in-edge to east
              ConstrainEast(n.LastInEdge, false, itemFactory);
              eastTakenByInEdge = true;
            } 
            if (!westTakenByOutEdge) {
              // if not already taken, constraint the first in-edge to west
              ConstrainWest(n.FirstInEdge, false, itemFactory);
              westTakenByInEdge = true;
            }
          }

          // distribute remaining out-edges
          if (n.OutDegree == 2 && !(eastTakenByOutEdge || westTakenByOutEdge)) {
            // two out-edges but none distributed, yet
            if (bestOutEdge == n.FirstOutEdge && !eastTakenByInEdge) {
              // first out-edge is in-flow edge and east side is still free
              ConstrainEast(n.LastOutEdge, true, itemFactory);
              eastTakenByOutEdge = true;
            } else if (bestOutEdge == n.LastOutEdge && !westTakenByInEdge) {
              // last out-edge is in-flow edge and west side is still free
              ConstrainWest(n.FirstOutEdge, true, itemFactory);
              westTakenByOutEdge = true;
            }
          } else if (n.OutDegree == 3 
            && !(eastTakenByOutEdge && westTakenByOutEdge) 
            && !(IsSameLayerEdge(n.FirstOutEdge.NextOutEdge, ldp))) {
            // three out-edges but not both sides taken, yet and the middle edge is no same layer edge
            if (!eastTakenByInEdge) {
              // if not already taken, constraint the last out-edge to east
              ConstrainEast(n.LastOutEdge, true, itemFactory);
              eastTakenByOutEdge = true;
            }
            if (!westTakenByInEdge) {
              // if not already taken, constraint the first out-edge to west
              ConstrainWest(n.FirstOutEdge, true, itemFactory);
              westTakenByOutEdge = true;
            }
          }
        }
      }

      // register the data provider for critical edge paths. It is deregistered again by BpmnLayout itself
      graph.AddDataProvider(HierarchicLayout.CriticalEdgePriorityDpKey, criticalEdges);

      sameLayerData = null;
      edge2LaneCrossing = null;
      node2LaneAlignment = null;
    }

    private LaneCrossing GetLaneCrossing(Edge edge) {
      return (LaneCrossing) edge2LaneCrossing.Get(edge);
    }

    private LaneAlignment GetLaneAlignment(Node source) {
      return (LaneAlignment)node2LaneAlignment.Get(source);
    }

    /// <summary>
    /// Get the <see cref="Edge"/> representing the original edge on the graph.
    /// </summary>
    /// <remarks>
    /// As the core layout algorithm creates temporary edges for example for same-layer edges and edges spanning
    /// multiple layers, we need to lookup the original edge of the graph for example as key in data providers.
    /// </remarks>
    private Edge GetOriginalEdge(Edge edge, ILayoutDataProvider ldp) {
      var originalEdge = sameLayerData.GetOriginalEdge(edge.Source) ??
                         (sameLayerData.GetOriginalEdge(edge.Target) ?? edge);
      var edgeData = ldp.GetEdgeData(originalEdge);
      return edgeData.AssociatedEdge ?? originalEdge;
    }

    /// <summary>
    /// Returns the best suited edge in <paramref name="edges"/> for use as in-flow edge or <see langword="null"/>
    /// if no such edge could be found.
    /// </summary>
    private Edge GetBestFlowEdge(IEnumerable<Edge> edges, ILayoutDataProvider ldp, LayoutGraph graph) {
      List<Edge> weakCandidates = new List<Edge>();
      List<Edge> candidates = new List<Edge>();

      foreach (var edge in edges) {
        var originalEdge = GetOriginalEdge(edge, ldp);
        if ((LaneCrossing) edge2LaneCrossing.Get(edge) != LaneCrossing.None
            || BpmnLayout.IsBoundaryInterrupting(originalEdge, graph)
            || IsSameLayerEdge(originalEdge, ldp)
            || edge.SelfLoop) {
          // an edge should not be aligned if:
          // - it crosses stripe borders
          // - it is boundary interrupting
          // - it is a same-layer edge
          // - it is a self-loop
          continue;
        }
        if (ldp.GetEdgeData(edge).Reversed
          || !BpmnLayout.IsSequenceFlow(originalEdge, graph)) {
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
        return weakCandidates[(int)Math.Floor(weakCandidates.Count / 2.0)];
      }
      return null;
    }

    /// <summary>
    /// Returns how much the <see cref="LaneAlignment"/> of the source and target node is consistent.
    /// </summary>
    /// <remarks>
    /// The consistency is <c>2</c>, if both nodes have the same alignment. 
    /// It is <c>1</c> if exactly one of the alignments is <see cref="LaneAlignment.None"/>
    /// and <c>0</c> otherwise.
    /// </remarks>
    private int GetAlignmentConsistency(Edge edge) {
      var sourceLA = GetLaneAlignment(edge.Source);
      var targetLA = GetLaneAlignment(edge.Target);
      int alignmentConsistency = sourceLA == targetLA ? 2
        : (sourceLA == LaneAlignment.None || targetLA == LaneAlignment.None) ? 1 
        : 0;
      return alignmentConsistency;
    }

    /// <summary>
    /// Sets a <see cref="IItemFactory.SetTemporaryPortConstraint">temporary east port constraint</see>
    /// on <paramref name="source"/> or target side of <paramref name="edge"/>
    /// </summary>
    private static void ConstrainEast(Edge edge, bool source, IItemFactory itemFactory) {
      itemFactory.SetTemporaryPortConstraint(edge, source, portConstraintEast);
    }

    /// <summary>
    /// Sets a <see cref="IItemFactory.SetTemporaryPortConstraint">temporary west port constraint</see>
    /// on <paramref name="source"/> or target side of <paramref name="edge"/>
    /// </summary>
    private static void ConstrainWest(Edge edge, bool source, IItemFactory itemFactory) {
      itemFactory.SetTemporaryPortConstraint(edge, source, portConstraintWest);
    }

    /// <summary>
    /// Returns if the source and target node of the <see cref="GetOriginalEdge">original edge</see> of <paramref name="edge"/>
    /// are on the same layer.
    /// </summary>
    private bool IsSameLayerEdge(Edge edge, ILayoutDataProvider ldp) {
      var originalEdge = GetOriginalEdge(edge, ldp);
      var sourceNodeData = ldp.GetNodeData(originalEdge.Source);
      var targetNodeData = ldp.GetNodeData(originalEdge.Target);
      return sourceNodeData != null && targetNodeData != null && (sourceNodeData.Layer == targetNodeData.Layer);
    }

    /// <summary>
    /// Determine the alignment of a node in its swim lane depending on the <see cref="LaneCrossing"/>s of its attached edges.
    /// </summary>
    private LaneAlignment CalculateLaneAlignment(Node n) {
      int toRightCount = 0;
      int toLeftCount = 0;
      foreach (var edge in n.Edges) {
        var crossing = (LaneCrossing) edge2LaneCrossing.Get(edge);
        if (n == edge.Source) {
          if (crossing == LaneCrossing.ToEast) {
            toRightCount++;
          } else if (crossing == LaneCrossing.ToWest) {
            toLeftCount++;
          }
        } else {
          if (crossing == LaneCrossing.ToEast) {
            toLeftCount++;
          } else if (crossing == LaneCrossing.ToWest) {
            toRightCount++;
          }
        }
      }
      if (toLeftCount > toRightCount) {
        return LaneAlignment.Left;
      } else if (toLeftCount < toRightCount) {
        return LaneAlignment.Right;
      } else  {
        return LaneAlignment.None;
      }
    }

    /// <summary>
    /// Returns the <see cref="SwimlaneDescriptor.ComputedLaneIndex"/> for <paramref name="node"/>.
    /// </summary>
    private static int GetLaneId(Node node, ILayoutDataProvider ldp) {
      var nodeData = ldp.GetNodeData(node);
      SwimlaneDescriptor laneDesc = nodeData != null ? nodeData.SwimLaneDescriptor : null;
      return laneDesc != null ? laneDesc.ComputedLaneIndex : -1;
    }

    /// <summary>
    /// Specifies the alignment of a node in its swimlane.
    /// </summary>
    /// <seealso cref="BalancingPortOptimizer.CalculateLaneAlignment"/>
    private enum LaneAlignment
    {
      /// <summary>
      /// The node has no special alignment.
      /// </summary>
      None,

      /// <summary>
      /// The node is aligned to the left side
      /// </summary>
      Left,

      /// <summary>
      /// The node is aligned to the right side
      /// </summary>
      Right
    }

    /// <summary>
    /// Specifies in which direction an edge crosses swim lane borders.
    /// </summary>
    private enum LaneCrossing
    {
      /// <summary>
      /// The edge doesn't cross a swimlane border.
      /// </summary>
      None,

      /// <summary>
      /// The edge crosses swimlane borders to the east, so its source node is in a swim lane with a lower <see cref="SwimlaneDescriptor.ComputedLaneIndex"/>.
      /// </summary>
      ToEast,

      /// <summary>
      /// The edge crosses swimlane borders to the west, so its source node is in a swim lane with a higher <see cref="SwimlaneDescriptor.ComputedLaneIndex"/>.
      /// </summary>
      ToWest
    }
  }
}
