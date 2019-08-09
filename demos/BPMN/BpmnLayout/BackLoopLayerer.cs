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
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout.Hierarchic;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  /// A layerer stage that pulls back loop components to earlier layers to reduce the spanned layers of back edges.
  /// </summary>
  /// <remarks>
  /// A back loop component is a set of connected nodes satisfying the following rules:
  /// <ul>
  /// <li>the set contains no sink node, i.e. no node with out degree 0</li>
  /// <li>all outgoing edges to nodes outside of this set are back edges.</li>
  /// </ul>
  /// </remarks>
  internal class BackLoopLayerer : ConstraintIncrementalLayerer
  {
    /// <summary>
    /// The state of a node while calculating those nodes on a back loop that might be pulled
    /// to a lower layer.
    /// </summary>
    private enum NodeState
    {
      Fixed,

      BackLooping,

      BackLoopingCandidate
    }

    private NodeState[] nodeStates;
    private int[] currentLayers;

    /// <summary>
    /// Creates a new instance with the specified core layerer.
    /// </summary>
    public BackLoopLayerer() : base(new TopologicalLayerer()) {
      AllowSameLayerEdges = true;
    }

    /// <inheritdoc/>
    public override void AssignLayers(LayoutGraph graph, ILayers layers, ILayoutDataProvider ldp) {
      // get core layer assignment
      base.AssignLayers(graph, layers, ldp);

      // Hide all edges that are no sequence flows
      var graphHider = new LayoutGraphHider(graph);
      foreach (var edge in graph.GetEdgeArray()) {
        if (!BpmnLayout.IsSequenceFlow(edge, graph)) { 
          graphHider.Hide(edge);
        }
      }

      // determine current layer of all nodes
      currentLayers = new int[graph.NodeCount];
      for (int i = 0; i < layers.Size(); i++) {
        for (INodeCursor nc = layers.GetLayer(i).List.Nodes(); nc.Ok; nc.Next()) {
          currentLayers[nc.Node.Index] = i;
        }
      }

      // mark nodes on a back-loop and candidates that may be on a back loop if other back-loop nodes are reassigned
      nodeStates = new NodeState[graph.NodeCount];
      NodeList candidates = new NodeList();
      NodeList backLoopNodes = new NodeList();
      for (int i = layers.Size() - 1; i >= 0; i--) {
        // check from last to first layer to detect candidates as well
        NodeList nodes = layers.GetLayer(i).List;
        UpdateNodeStates(nodes, backLoopNodes, candidates);
      }

      // swap layer for back-loop nodes
      while (backLoopNodes.Count > 0) {
        for (INodeCursor nc = backLoopNodes.Nodes(); nc.Ok; nc.Next()) {
          Node node = nc.Node;
          int currentLayer = currentLayers[node.Index];
          // the target layer is the next layer after the highest fixed target node layer
          int targetLayer = 0;
          for (Edge edge = node.FirstOutEdge; edge != null; edge = edge.NextOutEdge) {
            int targetNodeIndex = edge.Target.Index;
            if (nodeStates[targetNodeIndex] == NodeState.Fixed) {
              targetLayer = Math.Max(targetLayer, currentLayers[targetNodeIndex] + 1);
            }
          }
          if (targetLayer == 0) {
            // no fixed target found, so all targets must be candidates
            // -> we skip the node as we don't know where the candidates will be placed at the end
            continue;
          }
          if (targetLayer < currentLayer) {
            layers.GetLayer(currentLayer).Remove(node);
            layers.GetLayer(targetLayer).Add(node);
            currentLayers[node.Index] = targetLayer;
            nodeStates[node.Index] = NodeState.Fixed;
          }
        }
        backLoopNodes.Clear();

        // update states of the candidates
        candidates = UpdateNodeStates(candidates, backLoopNodes, new NodeList());
      }

      // remove empty layers
      for (int i = layers.Size() - 1; i >= 0; i--) {
        if (layers.GetLayer(i).List.Count == 0) {
          layers.Remove(i);
        }
      }

      // cleanup
      graphHider.UnhideAll();
      nodeStates = null;
      currentLayers = null;
    }

    private NodeList UpdateNodeStates(NodeList nodes, NodeList backLoopNodes, NodeList candidates) {
      for (INodeCursor nc = nodes.Nodes(); nc.Ok; nc.Next()) {
        Node node = nc.Node;
        NodeState nodeState = GetNodeState(node);
        switch (nodeState) {
          case NodeState.BackLooping:
            backLoopNodes.AddFirst(node);
            break;
          case NodeState.BackLoopingCandidate:
            candidates.AddFirst(node);
            break;
        }
        nodeStates[node.Index] = nodeState;
      }
      return candidates;
    }

    private NodeState GetNodeState(Node node) {
      int nodeLayer = currentLayers[node.Index];
      if (nodeLayer == 0) {
        // nodes in the first layer can't have any back edges
        return NodeState.Fixed;
      }
      NodeState nodeState = NodeState.Fixed;
      for (Edge edge = node.FirstOutEdge; edge != null; edge = edge.NextOutEdge) {
        int targetIndex = edge.Target.Index;
        if (currentLayers[targetIndex] >= nodeLayer) {
          // no back-looping edge...
          if (nodeStates[targetIndex] == NodeState.BackLooping || nodeStates[targetIndex] == NodeState.BackLoopingCandidate) {
            // ...but target is back-looping, so this one might be as well
            nodeState = NodeState.BackLoopingCandidate;
          } else {
            // ... and target is fixed -> this node is fixed as well.
            nodeState = NodeState.Fixed;
            break;
          }
        } else {
          if (nodeState == NodeState.Fixed) {
            // no back looping candidate -> back-looping
            nodeState = NodeState.BackLooping;
          }
        }
      }
      return nodeState;
    }
  }
}