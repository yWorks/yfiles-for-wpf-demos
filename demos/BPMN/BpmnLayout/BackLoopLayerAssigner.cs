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
using yWorks.Layout.Graph;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  ///   A layer assignment stage that pulls back-loop components to earlier layers to reduce the spanned layers of
  ///   back edges.
  /// </summary>
  /// <remarks>
  ///   A back-loop component is a set of connected nodes satisfying the following rules:
  ///   <list type="bullet">
  ///     <item>the set contains no sink node, i.e. no node with out-degree 0</item>
  ///     <item>all outgoing edges to nodes outside of this set are back edges.</item>
  ///   </list>
  /// </remarks>
  internal class BackLoopLayerAssigner : ConstraintIncrementalLayerAssigner
  {
    /// <summary>
    ///   The state of a node while calculating those nodes on a back-loop that might be pulled
    ///   to a lower layer.
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
    ///   Creates a new instance with the specified core layerer.
    /// </summary>
    public BackLoopLayerAssigner() : base(new TopologicalLayerAssigner()) {
      AllowSameLayerEdges = true;
    }

    /// <inheritdoc/>
    public override void AssignLayers(LayoutGraph graph, HierarchicalLayoutContext layoutContext) {
      // get core layer assignment
      base.AssignLayers(graph, layoutContext);
      var layers = layoutContext.Layers;

      // Hide all edges that are no sequence flows
      var graphHider = new LayoutGraphHider(graph);
      foreach (var edge in graph.Edges.ToArray()) {
        if (!BpmnLayout.IsSequenceFlow(edge, graph)) {
          graphHider.Hide(edge);
        }
      }

      // determine current layer of all nodes
      currentLayers = new int[graph.Nodes.Count];
      for (var i = 0; i < layers.Count; i++) {
        foreach (var node in layers[i].Nodes) {
          currentLayers[node.Index] = i;
        }
      }

      // mark nodes on a back-loop and candidates that may be on a back loop if other back-loop nodes are reassigned
      nodeStates = new NodeState[graph.Nodes.Count];
      var candidates = new YList<Node>();
      var backLoopNodes = new YList<Node>();
      for (var i = layers.Count - 1; i >= 0; i--) {
        // check from last to first layer to detect candidates as well
        var nodes = layers[i].Nodes;
        UpdateNodeStates(nodes, backLoopNodes, candidates);
      }

      // swap layer for back-loop nodes
      while (backLoopNodes.Count > 0) {
        foreach (var node in backLoopNodes) {
          var currentLayer = currentLayers[node.Index];
          // the target layer is the next layer after the highest fixed target node layer
          var targetLayer = 0;
          foreach (var edge in node.OutEdges) {
            var targetNodeIndex = edge.Target.Index;
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
            layers[currentLayer].Remove(node);
            layers[targetLayer].Add(node);
            currentLayers[node.Index] = targetLayer;
            nodeStates[node.Index] = NodeState.Fixed;
          }
        }
        backLoopNodes.Clear();

        // update states of the candidates
        candidates = UpdateNodeStates(candidates, backLoopNodes, new YList<Node>());
      }

      // remove empty layers
      for (var i = layers.Count - 1; i >= 0; i--) {
        if (layers[i].Nodes.Count == 0) {
          layoutContext.RemoveLayer(i);
        }
      }

      // cleanup
      graphHider.UnhideAll();
      nodeStates = null;
      currentLayers = null;
    }

    private YList<Node> UpdateNodeStates(YList<Node> nodes, YList<Node> backLoopNodes, YList<Node> candidates) {
      foreach (var node in nodes) {
        var nodeState = GetNodeState(node);
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
      var nodeLayer = currentLayers[node.Index];
      if (nodeLayer == 0) {
        // nodes in the first layer can't have any back edges
        return NodeState.Fixed;
      }
      var nodeState = NodeState.Fixed;
      foreach (var edge in node.OutEdges) {
        var targetIndex = edge.Target.Index;
        if (currentLayers[targetIndex] >= nodeLayer) {
          // no back-looping edge...
          if (nodeStates[targetIndex] == NodeState.BackLooping ||
              nodeStates[targetIndex] == NodeState.BackLoopingCandidate) {
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
