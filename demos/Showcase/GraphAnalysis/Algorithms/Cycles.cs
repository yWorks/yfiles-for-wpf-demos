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
  internal static class Cycles
  {
    /// <summary>
    ///   Description of the algorithm which determines all cycle edges in the graph.
    /// </summary>
    internal static readonly string[] CyclesDescription = [
        "Finding cycles in a graph is part of analyzing graph structures. This part of the demo shows an " +
        "algorithm that finds edges that belong to a cycle in a graph.",
        "Independent cycles are presented with different colors. Cycles which share common nodes and edges get " +
        "the same color. This algorithm is able to take the direction of edges into account."
    ];

    /// <summary>
    ///   Calculates the cycle edges in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateCycles(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var cycleEdges = new CycleEdges { Directed = config.Directed }.Run(graph).Edges.ToHashSet();
      if (cycleEdges.Count == 0) {
        return results;
      }

      // Find components in the subgraph consisting only of cycle edges, so independent cycles have different colors.
      var result = new ConnectedComponents {
          SubgraphEdges = { Predicate = cycleEdges.Contains },
          SubgraphNodes = { Predicate = node => graph.EdgesAt(node).Any(cycleEdges.Contains) }
      }.Run(graph);
      var i = 0;
      foreach (var component in result.Components) {
        results.MarkGroup(component.Nodes, component.InducedEdges, i++);
      }

      return results;
    }
  }
}
