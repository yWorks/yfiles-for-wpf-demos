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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Hierarchical;
using LayoutEdge = yWorks.Layout.Graph.Edge;
using LayoutGraphItem = yWorks.Layout.Graph.LayoutGraphItem;
using LayoutNode = yWorks.Layout.Graph.Node;
using LayoutNodeLabel = yWorks.Layout.Graph.NodeLabel;
using LayoutEdgeLabel = yWorks.Layout.Graph.EdgeLabel;

namespace yWorks.Demo.CustomLayoutStage
{
  /// <summary>
  ///   A layout stage that tries to align a specified set of nodes by using critical edge
  ///   priorities when the core layout is <see cref="HierarchicalLayout"/>.
  ///   This sample shows how to define a custom <see cref="yWorks.Utils.IMapper{K,V}"/> which is used to define
  ///   data for this layout stage. In this case, the data is boolean and defines whether
  ///   a node is part of the alignment nodes set.
  ///   Furthermore, it demonstrates how a graph can be pre-processed before the core layout and
  ///   post-processed (restored) afterward. In the pre-processing step, dummy elements (in this
  ///   case edges) are inserted to realize the alignment and have to be marked as critical using
  ///   <see cref="HierarchicalLayoutData{TNode,TEdge,TNodeEdge,TNodeLabel,TEdgeLabel}.CriticalEdgePriorities"/>.
  ///   In the restoration step, the dummy elements are removed.
  ///   This general approach (pre-processing, core layout, post-processing) can be used for many
  ///   scenarios where a custom layout stage is required.
  ///   Inserting temporary elements is an effective way to influence the layout result and customize
  ///   them to specific requirements. Keeping temporary elements only inside the layout graph and
  ///   using a custom stage has the huge advantages that no additional view graph (<see cref="IGraph"/>)
  ///   elements must be created and that only a single layout run on the view graph is required,
  ///   thus, not affecting the layout animation.
  /// </summary>
  public sealed class AlignmentStage : LayoutStageBase
  {
    /// <summary>
    ///   Key to register a <see cref="yWorks.Utils.IMapper{K,V}"/> with the input graph where the nodes that should
    ///   be aligned are marked.
    /// </summary>
    public static readonly NodeDataKey<bool> AlignedNodesDataKey = new("AlignmentStage.AlignedNodesDataKey");

    public AlignmentStage(ILayoutAlgorithm coreLayout) : base(coreLayout) { }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      // Calculate an initial layout. This is needed so that the nodes to align
      // already have their proper vertical order as defined by the core layout.
      CoreLayout?.ApplyLayout(graph);

      if (CoreLayout is not HierarchicalLayout hierarchicalLayout) {
        // This stage is meant to be used with a hierarchical layout as its core layout algorithm
        // because it requires its critical path feature to realize the node alignment
        return;
      }

      // Pre-processing: insert temporary alignment edges
      var alignmentEdges = InsertAlignmentEdges(graph);

      // Use the layout data to mark the alignmentEdges edges as critical
      var layoutData = CreateLayoutData(graph, hierarchicalLayout, alignmentEdges);

      // Execute the hierarchical layout (core layout)
      graph.ApplyLayout(hierarchicalLayout, layoutData);

      // Post-processing: remove the temporary alignment edges
      RestoreGraph(graph, alignmentEdges);
    }

    /// <summary>
    ///   Marks the edges that have to be aligned as critical using the layout data.
    /// </summary>
    private static HierarchicalLayoutData<LayoutNode, LayoutEdge, LayoutGraphItem, LayoutNodeLabel, LayoutEdgeLabel>
        CreateLayoutData(LayoutGraph graph, HierarchicalLayout hierarchicalLayout, List<LayoutEdge> alignmentEdges) {
      // Query the data map defining critical edge priorities and 'wrap' it.
      // This is an important step to allow the fact that critical edge priorities are defined
      // for edges other than the new alignment edges too.
      // Without considering the original data provider, the original data would be ignored/overwritten.
      var originalPriorities = graph.Context.GetItemData(HierarchicalLayout.CriticalEdgePriorityDataKey);

      var alignmentEdgesSet = new HashSet<LayoutEdge>(alignmentEdges);
      var layoutData = hierarchicalLayout.CreateLayoutData(graph);
      layoutData.CriticalEdgePriorities.MapperFunction = edge => {
        if (alignmentEdgesSet.Contains(edge)) {
          // An inserted edge gets a large criticality priority which means that it is important
          // that it should be straight,
          // yielding that the two adjacent nodes have to be aligned
          return 100;
        }
        // For all other edges, we query the priority from the original provider
        return originalPriorities?[edge] ?? 0;
      };
      return layoutData;
    }

    /// <summary>
    ///   Prepares the graph by introducing temporary alignment edges between nodes marked as
    ///   alignment nodes in the data map registered with the key <see cref="AlignedNodesDataKey"/>.
    /// </summary>
    private static List<LayoutEdge> InsertAlignmentEdges(LayoutGraph graph) {
      var dataMap = graph.Context.GetItemData(AlignedNodesDataKey);
      if (dataMap is null) {
        // If no provider is registered, there is nothing to do
        return [];
      }

      var orderedAlignmentNodes = graph.Nodes
          // Handle nodes where the data map indicates that it is an alignment node
          .Where(node => dataMap[node])
          // Order them by y-coordinates
          .OrderBy(node => node.Layout.Y).ToArray();

      var alignedEdges = new List<LayoutEdge>();
      for (var i = 1; i < orderedAlignmentNodes.Length; i++) {
        // Insert an edge between two consecutive alignment nodes. Since we performed the core
        // layout initially already, we can assume that in the alignment nodes have y-coordinates that
        // are consistent with the layering the hierarchical layout (core layout) will produce.
        var edge = graph.CreateEdge(orderedAlignmentNodes[i - 1], orderedAlignmentNodes[i]);
        // Remember the inserted edges because later we need to remove them
        alignedEdges.Add(edge);
      }
      return alignedEdges;
    }

    /// <summary>
    ///   Restores the graph by removing the temporarily inserted alignment edges.
    /// </summary>
    private static void RestoreGraph(LayoutGraph graph, List<LayoutEdge> alignmentEdges) {
      if (alignmentEdges.Count == 0) {
        // Nothing to do
        return;
      }

      // Remove temporary edges
      foreach (var alignmentEdge in alignmentEdges) {
        graph.Remove(alignmentEdge);
      }
    }
  }
}
