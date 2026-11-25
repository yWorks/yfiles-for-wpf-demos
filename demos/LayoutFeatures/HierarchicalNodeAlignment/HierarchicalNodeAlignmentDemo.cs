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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalNodeAlignment
{
  [Demo("layout-hierarchical-node-alignment", "layout-features", "Hierarchical Layout with Node Alignment",
      Summary = "Shows how to align a set of nodes with hierarchical layout.", Tags = ["hierarchical", "alignment"],
      Keywords = ["layout", "alignment", "critical", "paths", "playground"])]
  public class HierarchicalNodeAlignmentDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // Find edges forming the longest path (requires an acyclic graph)
      var longestPathAlgorithm = new LongestPath();
      longestPathAlgorithm.SubgraphEdges.Excludes.Source = GetCycleEdges(Graph);
      var longestPathEdges = new HashSet<IEdge>(longestPathAlgorithm.Run(Graph).Edges);

      // Prioritize edges on the longest path for layout
      var layout = new HierarchicalLayout();
      var layoutData = layout.CreateLayoutData(Graph);
      layoutData.CriticalEdgePriorities.MapperFunction = edge => longestPathEdges.Contains(edge) ? 10 : 1;

      // Apply hierarchical layout with prioritized edges
      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Returns edges that create cycles in the graph.
    /// </summary>
    private static IEnumerable<IEdge> GetCycleEdges(IGraph graph) {
      // Find feedback edges and self-loops
      var feedbackEdgeSetResult = new FeedbackEdgeSet().Run(graph);
      return graph.Edges.Where(edge => feedbackEdgeSetResult.FeedbackEdgeSet.Contains(edge) || edge.IsSelfLoop());
    }

    [STAThread]
    private static void Main() => Run<HierarchicalNodeAlignmentDemo>();
  }
}
