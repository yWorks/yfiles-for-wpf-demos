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

using System.Collections.Generic;
using System.Linq;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Graph;
using yWorks.Utils;
using LayoutEdge = yWorks.Layout.Graph.Edge;
using LayoutEdgeLabel = yWorks.Layout.Graph.EdgeLabel;
using LayoutGraph = yWorks.Layout.Graph.LayoutGraph;
using LayoutNode = yWorks.Layout.Graph.Node;
using LayoutNodeLabel = yWorks.Layout.Graph.NodeLabel;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   A layout stage that adds visual spacing between node groups.
  /// </summary>
  public sealed class CircleGroupSpacerStage : LayoutStageBase
  {
    // Resolved from the layout context so comparison can use the same grouping as the graph.
    private IMapper<LayoutNode, string>? nodeGroupMapper;
    private readonly NodeOrderingFunction<LayoutNode> nodeComparator;
    private readonly NodeGroupOrderingFunction nodeGroupComparator;

    /// <summary>
    ///   Creates a new stage that adds visual spacing between node groups.
    /// </summary>
    /// <param name="layout">The layout to wrap.</param>
    /// <param name="nodeComparator">A function used to compare nodes.</param>
    /// <param name="nodeGroupComparator">A function used to compare node groups.</param>
    public CircleGroupSpacerStage(
        ILayoutAlgorithm layout,
        NodeOrderingFunction<LayoutNode> nodeComparator,
        NodeGroupOrderingFunction nodeGroupComparator) : base(layout) {
      this.nodeComparator = nodeComparator;
      this.nodeGroupComparator = nodeGroupComparator;
    }

    /// <summary>
    ///   Gets the layout-data key used to map each original layout node to its node-group identifier.
    /// </summary>
    public static readonly NodeDataKey<string> NodeGroupDataKey = new("CircleGroupSpacerStage.NodeGroupDataKey");

    /// <summary>
    ///   Applies the layout by inserting spacer nodes between groups, running the core layout,
    ///   and then removing the spacers. This creates visual separation between node groups.
    /// </summary>
    /// <param name="graph">The layout graph to arrange.</param>
    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      // Retrieve the node group mapper from the graph context.
      nodeGroupMapper = graph.Context.GetItemData(NodeGroupDataKey);

      // One temporary node per distinct group creates a gap between adjacent groups in the cycle.
      var groups = new HashSet<string?>(graph.Nodes.Select(node => nodeGroupMapper?[node]));

      // Create invisible spacer nodes - one per group (if there are multiple groups).
      // These spacers create visual gaps between groups in the circular layout.
      var spacerNodes = new List<LayoutNode>();
      if (groups.Count > 1) {
        foreach (var groupName in groups) {
          var spacerNode = graph.CreateNode();
          spacerNode.Tag = new Dictionary<string, object?> {
              ["group"] = groupName ?? "Ungrouped",
              ["label"] = string.Empty
          };
          spacerNodes.Add(spacerNode);
        }
      }

      // Order original nodes and temporary spacers together by group, then order nodes within each group.
      graph.Context.PushLayer(new CircularLayoutData<LayoutNode, LayoutEdge, LayoutNodeLabel, LayoutEdgeLabel> {
          NodeComparison = NestedNodeOrderingFunction
      });

      // Apply the core layout algorithm with the spacer nodes included.
      CoreLayout?.ApplyLayout(graph);

      // Remove the spacer nodes after layout - they've served their purpose.
      foreach (var spacerNode in spacerNodes) {
        graph.Remove(spacerNode);
      }
    }

    /// <summary>
    ///   Compares nodes by group and then by their individual ordering.
    /// </summary>
    /// <param name="nodeA">The first node.</param>
    /// <param name="nodeB">The second node.</param>
    /// <returns>-1 if the first node precedes the second, 1 if it follows, and 0 if equal.</returns>
    private int NestedNodeOrderingFunction(LayoutNode nodeA, LayoutNode nodeB) {
      var groupA = GetGroupName(nodeA);
      var groupB = GetGroupName(nodeB);
      if (groupA != groupB) {
        return nodeGroupComparator(groupA ?? "Ungrouped", groupB ?? "Ungrouped");
      }
      return nodeComparator(nodeA, nodeB);
    }

    private string? GetGroupName(LayoutNode node) {
      // Temporary spacers carry their group in a tag; graph nodes use the caller-provided mapper.
      var mappedGroup = nodeGroupMapper?[node];
      if (mappedGroup is not null) {
        return mappedGroup;
      }
      if (node.Tag is IDictionary<string, object?> tag && tag.TryGetValue("group", out var group)) {
        return group?.ToString();
      }
      return null;
    }
  }
}
