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
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;

namespace yWorks.Demo.Collapse;

/// <summary>
///   Provides utility function for collapsing and expanding nodes as well as configuring layout
///   algorithms.
/// </summary>
public sealed class CollapseAndExpandNodes
{
  private readonly GraphControl graphControl;
  private readonly Dictionary<INode, bool> nodeCollapsedMap = [];
  private readonly Dictionary<INode, bool> nodeVisibility = [];

  public CollapseAndExpandNodes(GraphControl graphControl) {
    this.graphControl = graphControl;
  }

  /// <summary>
  ///   Sets the given node's collapsed state.
  /// </summary>
  /// <param name="node">The node whose state is set.</param>
  /// <param name="collapsed">The given node's new state.</param>
  public void SetCollapsed(INode node, bool collapsed) {
    nodeCollapsedMap[node] = collapsed;
  }

  /// <summary>
  ///   Gets the given node's collapsed state.
  /// </summary>
  /// <param name="node">The node whose state is queried.</param>
  public bool IsCollapsed(INode node) => nodeCollapsedMap.TryGetValue(node, out var collapsed) && collapsed;

  /// <summary>
  ///   Sets the given node's visibility.
  /// </summary>
  /// <param name="node">The node whose state is set.</param>
  /// <param name="visible">The given node's new state.</param>
  public void SetNodeVisibility(INode node, bool visible) {
    nodeVisibility[node] = visible;
  }

  /// <summary>
  ///   Gets the given node's visibility.
  /// </summary>
  /// <param name="node">The node whose state is queried.</param>
  public bool GetNodeVisibility(INode node) => nodeVisibility.TryGetValue(node, out var visible) && visible;

  /// <summary>
  ///   Show the children of a collapsed node.
  /// </summary>
  /// <param name="node">The node that should be expanded</param>
  public void Expand(INode node) {
    CollapseExpandImpl(node, false);
  }

  /// <summary>
  ///   Hide the children of an expanded node.
  /// </summary>
  /// <param name="node">The node that should be collapsed</param>
  public void Collapse(INode node) {
    CollapseExpandImpl(node, true);
  }

  /// <summary>
  ///   Collapses or expands the given node.
  /// </summary>
  /// <param name="node">The node whose children will be hidden or shown.</param>
  /// <param name="collapse">
  ///   If true, the given node's children will be hidden;
  ///   otherwise they will be shown.
  /// </param>
  private void CollapseExpandImpl(INode node, bool collapse) {
    SetCollapsed(node, collapse);

    var filteredGraph = (FilteredGraphWrapper) graphControl.Graph!;
    GetDescendants(filteredGraph.WrappedGraph!, node, IsCollapsed).ToList().ForEach(succ => {
      SetNodeVisibility(succ, !collapse);
    });
  }

  /// <summary>
  ///   Returns the descendants of the given node.
  /// </summary>
  /// <param name="graph">The graph.</param>
  /// <param name="node">The node.</param>
  /// <param name="recursionFilter">
  ///   A node predicate that specifies whether
  ///   the recursion should continue for the given node.
  /// </param>
  /// <returns>The descendants of the given node.</returns>
  private static IEnumerable<INode> GetDescendants(IGraph graph, INode node, Func<INode, bool> recursionFilter) {
    var visited = new HashSet<INode>();
    var descendants = new List<INode>();
    var nodes = new Stack<INode>();
    nodes.Push(node);
    while (nodes.Count > 0) {
      foreach (var successor in graph.Successors(nodes.Pop())) {
        if (visited.Add(successor)) {
          descendants.Add(successor);
          if (!recursionFilter(successor)) {
            nodes.Push(successor);
          }
        }
      }
    }
    return descendants;
  }

  /// <summary>
  ///   Moves incremental nodes between their neighbors before expanding for a smooth animation.
  /// </summary>
  /// <param name="incrementalNodes">the nodes that need to be moved.</param>
  private void PrepareSmoothExpandLayoutAnimation(List<INode> incrementalNodes) {
    var graph = graphControl.Graph!;

    // mark the new nodes and place them between their neighbors
    var layout = new PlaceNodesAtBarycenterStage();
    var layoutData = layout.CreateLayoutData(graph);
    layoutData.AffectedNodes.Items = incrementalNodes;

    graph.ApplyLayout(layout, layoutData);
  }

  /// <summary>
  ///   Configures a new layout for the current graph.
  ///   Incremental nodes are moved between their neighbors before expanding for a smooth animation.
  /// </summary>
  /// <param name="toggledNode">
  ///   The children of this node are laid out as incremental items.
  ///   Without a toggled node, a 'from scratch' layout is calculated.
  /// </param>
  /// <param name="expand">Whether this is part of an expand or a collapse action.</param>
  /// <param name="currentLayoutData">
  ///   Additional configuration data for the given layout algorithm
  ///   and the demo's graph.
  /// </param>
  /// <param name="currentLayout">The layout algorithm to arrange the demo's graph.</param>
  public void ConfigureLayout(
      INode? toggledNode,
      bool expand,
      CompositeLayoutData<INode, IEdge, ILabel, ILabel> currentLayoutData,
      ILayoutAlgorithm currentLayout) {
    var graph = graphControl.Graph!;
    if (toggledNode is not null) {
      // keep the clicked node at its location
      var anchoringStage = new LayoutAnchoringStage();
      var anchoringData = anchoringStage.CreateLayoutData(graph);
      anchoringData.NodeAnchoringPolicies.MapperFunction = node =>
          node == toggledNode ? LayoutAnchoringPolicy.Center : LayoutAnchoringPolicy.None;
      currentLayoutData.Items.Add(anchoringData);

      var incrementalNodes = GetDescendants(graph, toggledNode, _ => false).ToList();

      var toggledNodeRenderElement = graphControl.GraphModelManager.GetMainRenderTreeElement(toggledNode);
      foreach (var node in incrementalNodes) {
        var renderElement = graphControl.GraphModelManager.GetMainRenderTreeElement(node);
        if (renderElement is not null && toggledNodeRenderElement is not null) {
          graphControl.GraphModelManager.Lower(node);
        }
      }

      if (expand) {
        // move the incremental nodes between their neighbors before expanding for a smooth animation
        PrepareSmoothExpandLayoutAnimation(incrementalNodes);
      } else {
        // configure PlaceNodesAtBarycenterStage for a smooth animation
        var barycenterStage = new PlaceNodesAtBarycenterStage();
        var barycenterData = barycenterStage.CreateLayoutData(graph);
        barycenterData.AffectedNodes.Items = incrementalNodes;
        currentLayoutData.Items.Add(barycenterData);
      }

      if (currentLayout is OrganicLayout organicLayout) {
        organicLayout.CompactnessFactor = 0.7;
        organicLayout.DefaultPreferredEdgeLength = 60;

        organicLayout.AllowNodeOverlaps = false;
        organicLayout.DefaultMinimumNodeDistance = 10;
        organicLayout.QualityTimeRatio = 1;
        organicLayout.StopDuration = TimeSpan.FromMilliseconds(1000 + graph.Nodes.Count * 50);

        var bfs = new Bfs { TraversalDirection = TraversalDirection.Both };
        bfs.CoreNodes.Items = [.. incrementalNodes, toggledNode];
        var layerIds = bfs.Run(graph).NodeLayerIds;

        var organicLayoutData = organicLayout.CreateLayoutData(graph);
        organicLayoutData.NodeInertia.MapperFunction = node => 1 - 1.0 / (layerIds[node] + 1);
        organicLayoutData.NodeStress.MapperFunction = node => 1.0 / (layerIds[node] + 1);
        currentLayoutData.Items.Add(organicLayoutData);
      } else if (currentLayout is HierarchicalLayout hierarchicalLayout) {
        hierarchicalLayout.FromSketchMode = true;

        var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(graph);
        hierarchicalLayoutData.IncrementalNodes.Items = incrementalNodes;
        currentLayoutData.Items.Add(hierarchicalLayoutData);
      }
    } else if (currentLayout is HierarchicalLayout hierarchicalLayout) {
      hierarchicalLayout.FromSketchMode = false;
    }
  }
}
