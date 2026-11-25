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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.TabularGroups
{
  internal static class HierarchicalLayoutTabularGroups
  {
    /// <summary>
    ///   Creates a <see cref="HierarchicalLayout"/> and the respective
    ///   <see cref="HierarchicalLayoutData{TNode, TEdge, TNodeEdge, TNodeLabel, TEdgeLabel}"/> so that all groups
    ///   contained in the graph are treated as tabular group nodes.
    /// </summary>
    /// <param name="graph">The graph whose group nodes are marked as tabular groups.</param>
    /// <param name="sorted">Whether the tabular group child nodes should be sorted by their label text.</param>
    /// <param name="childDistance">The distance between tabular group child nodes.</param>
    /// <returns>The configured hierarchical layout algorithm and its layout data.</returns>
    public static (HierarchicalLayout Layout, HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel>
        LayoutData) CreateTabularGroupsHierarchicalLayout(IGraph graph, bool sorted = false, double childDistance = 0) {
      var layout = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
          MinimumLayerDistance = 50
      };

      // Create layout data and mark all group nodes as tabular groups.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.TabularGroups.Predicate = graph.IsGroupNode;

      // Configure the order of tabular group children, if desired.
      if (sorted) {
        layoutData.TabularGroupChildComparisons.Constant = (node1, node2) => {
          var label1 = node1.Labels.Count > 0 ? node1.Labels[0] : null;
          var label2 = node2.Labels.Count > 0 ? node2.Labels[0] : null;
          if (label1 is not null && label2 is not null) {
            // Compare label text in reverse because a left-to-right layout orders nodes within a layer from bottom to top.
            return string.Compare(label2.Text, label1.Text, StringComparison.CurrentCulture);
          }
          return 0;
        };
      }

      // Specify the distance between tabular group child nodes.
      layout.DefaultNodeDescriptor.TabularGroupChildDistance = childDistance;

      return (layout, layoutData);
    }

    /// <summary>
    ///   Returns a <see cref="HierarchicalLayout"/> with a left-to-right layout orientation.
    /// </summary>
    /// <returns>The configured hierarchical layout algorithm.</returns>
    public static HierarchicalLayout CreateDefaultHierarchicalLayout() =>
        new() {
            LayoutOrientation = LayoutOrientation.LeftToRight,
            MinimumLayerDistance = 50
        };
  }
}
