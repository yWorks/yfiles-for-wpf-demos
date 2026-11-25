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
using yWorks.Analysis;
using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  internal static class Paths
  {
    /// <summary>
    ///   Description of the algorithm which determines the shortest paths between nodes.
    /// </summary>
    internal static readonly string[] ShortestPathsDescription = [
        "This part of the demo highlights the shortest/cheapest path between nodes that can be marked using the " +
        "Context Menu. It is possible to mark multiple sources/targets. Which path should be focused can be " +
        "specified at the nodes that belong to several paths.",
        "Costs can be specified using edge labels. The cost of edges without labels is their edge length. When " +
        "the algorithm should use Uniform costs all edges are treated the same. For the sake of simplicity, in " +
        "this demo we allow only positive edge-costs.",
        "This algorithm can take the direction of edges into account."
    ];

    /// <summary>
    ///   Calculates the shortest paths between start and end nodes.
    /// </summary>
    internal static AnalysisResult CalculateShortestPaths(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();
      var source = config.StartNodes.FirstOrDefault() ?? graph.Nodes.First();
      var sink = config.EndNodes.FirstOrDefault() ?? graph.Nodes.Last();

      var starts = config.StartNodes.Count > 0 ? config.StartNodes : new List<INode> { source };
      var ends = config.EndNodes.Count > 0 ? config.EndNodes : new List<INode> { sink };
      if (starts.Count == 1 && ends.Count == 1) {
        var path = new ShortestPath {
            Directed = config.Directed,
            Source = { Item = starts[0] },
            Sink = { Item = ends[0] },
            Costs = { MapperFunction = edge => config.EdgeWeights[edge] }
        }.Run(graph);
        results.MarkGroup(path.Nodes, path.Edges, 0);
        return results;
      }

      var result = new AllPairsShortestPaths {
          Directed = config.Directed,
          Costs = { MapperFunction = edge => config.EdgeWeights[edge] },
          Sources = { Source = starts },
          Sinks = { Source = ends }
      }.Run(graph);
      results.MarkPaths(result.Paths);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the k shortest paths between nodes.
    /// </summary>
    internal static readonly string[] KShortestPathsDescription = [
        "This part of the demo highlights the k shortest path between nodes that can be marked using the " +
        "Context Menu. The k value is set to 3 for this sample.",
        "The paths are ranked by their total cost. This ranking is shown in the labels of the nodes. Costs are " +
        "specified in the edge labels when the Non-uniform Edge Weights option is selected. Otherwise, the " +
        "Uniform costs option treats all edges equally.",
        "Currently, the k-shortest path algorithm only supports directed graphs. It may return fewer than k " +
        "paths if there are no more distinct paths between the nodes. As the simplePaths parameter is set to be " +
        "true, only paths where no node is repeated are returned."
    ];

    /// <summary>
    ///   Calculates the k shortest paths between start and end nodes.
    /// </summary>
    internal static AnalysisResult CalculateKShortestPaths(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();
      var source = config.StartNodes.FirstOrDefault() ?? graph.Nodes.First();
      var sink = config.EndNodes.FirstOrDefault() ?? graph.Nodes.Last();

      // The k-shortest paths algorithm uses directed edges and returns simple paths.
      var result = new KShortestPaths {
          K = 3,
          SimplePaths = true,
          Source = { Item = source },
          Sink = { Item = sink },
          Costs = { MapperFunction = edge => config.EdgeWeights[edge] }
      }.Run(graph);
      // Mark each path and add its ranking as a node label.
      results.MarkPaths(result.Paths, true);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines all paths between nodes.
    /// </summary>
    internal static readonly string[] AllPathsDescription = [
        "This part of the demo highlights all paths between two nodes that can be marked using the Context " + "Menu.",
        "The paths may share some parts and therefore can overlap. Which path is in focus can be specified at " +
        "the nodes which belong to several paths.",
        "This algorithm can take the direction of edges into account."
    ];

    /// <summary>
    ///   Calculates all paths between start and edge nodes.
    /// </summary>
    internal static AnalysisResult CalculateAllPaths(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();
      var source = config.StartNodes.FirstOrDefault() ?? graph.Nodes.First();
      var sink = config.EndNodes.FirstOrDefault() ?? graph.Nodes.Last();

      var result = new Analysis.Paths {
          Directed = config.Directed,
          StartNodes = { Source = config.StartNodes.Count > 0 ? config.StartNodes : new List<INode> { source } },
          EndNodes = { Source = config.EndNodes.Count > 0 ? config.EndNodes : new List<INode> { sink } }
      }.Run(graph);
      results.MarkPaths(result.Paths);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines all chains in the graph.
    /// </summary>
    internal static readonly string[] AllChainsDescription = [
        "This part of the demo highlights all chains in the graph. Chains contain only nodes with degree 2 as " +
        "well as their start and end node.",
        "Which path is focused in case a node belongs to several chains can be specified at these nodes.",
        "This algorithm can take the direction of edges into account."
    ];

    /// <summary>
    ///   Calculates all chains in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateAllChains(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      results.MarkPaths(new Chains { Directed = config.Directed }.Run(graph).Chains);

      return results;
    }

    /// <summary>
    ///   Description of all shortest paths from a single source to all other nodes.
    /// </summary>
    internal static readonly string[] SingleSourceShortestPathsDescription = [
        "This part of the demo marks the shortest paths from a single source to all other reachable nodes in " +
        "the graph.",
        "The gradient of node and edge colors represents the distance from the source. The source can be marked " +
        "using the Context Menu.",
        "Costs can be specified using edge labels. The cost of edges without labels is their edge length. When " +
        "the algorithm should Uniform costs all edges are treated the same. For the sake of simplicity, in this " +
        "demo we allow only positive edge-costs.",
        "This algorithm can take the direction of edges into account."
    ];

    /// <summary>
    ///   Calculates all shortest paths from a single source to all other nodes.
    /// </summary>
    internal static AnalysisResult CalculateSingleSourceShortestPaths(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();
      var source = config.StartNodes.FirstOrDefault() ?? graph.Nodes.First();

      var result = new SingleSourceShortestPaths {
          Directed = config.Directed,
          Source = { Item = source },
          Sinks = { Source = graph.Nodes },
          Costs = { MapperFunction = edge => config.EdgeWeights[edge] }
      }.Run(graph);
      // Determine the longest path length to scale the distances used for the color gradient.
      var longest = result.Paths.Select(path => path.Distance).DefaultIfEmpty(0).Max();
      var i = 0;
      foreach (var path in result.Paths.OrderBy(path => path.Nodes.Count)) {
        foreach (var node in path.Nodes) {
          results.MarkItem(node, i);
          var distance = longest > 0 ? result.Distances[node] / longest : 0;
          results.Gradient[node] = distance;
          var edge = result.Predecessors[node];
          if (edge != null) {
            results.MarkItem(edge, i);
            results.Gradient[edge] = distance;
          }
        }

        i++;
      }

      return results;
    }
  }
}
