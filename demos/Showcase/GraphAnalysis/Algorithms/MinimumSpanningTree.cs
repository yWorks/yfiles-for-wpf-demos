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

using System.Linq;
using yWorks.Analysis;
using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  internal static class MinimumSpanningTree
  {
    /// <summary>
    ///   Description of the algorithm which determines the minimum spanning tree of a graph.
    /// </summary>
    internal static readonly string[] MinimumSpanningTreeDescription = [
        "Finding the minimum spanning tree in a graph is part of analyzing graph structures.",
        "Which edges are included in the minimum spanning tree can be influenced with costs. Edges with lower " +
        "costs are more likely kept in the tree.",
        "Costs can be specified using edge labels. The cost of edges without labels is their edge length. When " +
        "the algorithm should use Uniform costs all edges are treated the same. For the sake of simplicity, in " +
        "this demo we allow only positive edge-costs."
    ];

    /// <summary>
    ///   Calculates the minimum spanning tree for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateMinimumSpanningTree(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      if (!graph.Edges.Any()) {
        return results;
      }

      // Calculate the minimum spanning tree or forest, then mark its edges and nodes.
      var result = new SpanningTree { Costs = { MapperFunction = edge => config.EdgeWeights[edge] } }.Run(graph);
      results.MarkGroup(graph.Nodes, result.Edges, 0);

      return results;
    }
  }
}
