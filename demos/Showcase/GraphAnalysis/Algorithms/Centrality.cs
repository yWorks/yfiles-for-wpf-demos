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
  internal static class Centrality
  {
    /// <summary>
    ///   Description of the algorithm which determines the degree centrality values.
    /// </summary>
    internal static readonly string[] DegreeCentralityDescription = [
        "This part of the demo shows the degree centrality for the nodes of the given graph. This algorithm " +
        "uses the degree of the nodes (incoming and outgoing edges) to determine the centrality value for each " +
        "node.",
        "Larger and darker nodes are more central than small, light ones. The label shows the exact centrality " +
        "value."
    ];

    /// <summary>
    ///   Calculates the degree centrality values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateDegreeCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new DegreeCentrality {
          ConsiderIncomingEdges = true,
          ConsiderOutgoingEdges = true
      }.Run(graph);
      results.SetCentrality(graph.Nodes, node => result.NodeCentrality[node],
          node => result.NormalizedNodeCentrality[node]);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the weight centrality values.
    /// </summary>
    internal static readonly string[] WeightCentralityDescription = [
        "This part of the demo shows the weight centrality for the nodes of the given graph.",
        "Weights for edges must be positive and can be specified using edge labels. The weights of edges " +
        "without labels are their edge lengths. When the algorithm should use Uniform weights all edges have " +
        "equal weights. This demo sums the weights of both incoming and outgoing edges.",
        "Larger and darker nodes are more central than small, light ones. The label shows the exact centrality " +
        "value."
    ];

    /// <summary>
    ///   Calculates the weight centrality values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateWeightCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new WeightCentrality {
          ConsiderIncomingEdges = true,
          ConsiderOutgoingEdges = true,
          Weights = { MapperFunction = edge => config.EdgeWeights[edge] }
      }.Run(graph);
      results.SetCentrality(graph.Nodes, node => result.NodeCentrality[node],
          node => result.NormalizedNodeCentrality[node]);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the graph centrality values.
    /// </summary>
    internal static readonly string[] GraphCentralityDescription = [
        "This part of the demo shows the graph centrality for the nodes of the given graph.",
        "Weights must be positive and can be specified using edge labels. The weights of edges without labels " +
        "are their edge lengths. When the algorithm should use Uniform weights all edges have equal weights. " +
        "This algorithm can take the direction of edges into account.",
        "Note that, for disconnected graphs the centrality values of all nodes will be zero. This also applies " +
        "to directed graphs without a node from which all other nodes are reachable.",
        "Larger and darker nodes are more central than small, light ones. The label shows the exact centrality " +
        "value."
    ];

    /// <summary>
    ///   Calculates the graph centrality values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateGraphCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new GraphCentrality {
          Directed = config.Directed,
          Weights = { MapperFunction = edge => config.EdgeWeights[edge] }
      }.Run(graph);
      results.SetCentrality(graph.Nodes, node => result.NodeCentrality[node],
          node => result.NormalizedNodeCentrality[node]);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the node-edge betweenness centrality values.
    /// </summary>
    internal static readonly string[] NodeEdgeBetweennessCentralityDescription = [
        "This part of the demo shows the node betweenness centrality for the nodes and edges of the given " + "graph.",
        "The edge betweenness centrality is presented by edge labels. Weights must be positive and can be " +
        "specified using edge labels. The weights of edges without labels are their edge lengths. When the " +
        "algorithm should use Uniform weights all edges have equal weights. This algorithm can take the " +
        "direction of edges into account.",
        "Larger and darker nodes are more central than small, light ones. The label shows the exact centrality " +
        "value."
    ];

    /// <summary>
    ///   Calculates the node-edge betweenness centrality values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateNodeEdgeBetweennessCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new BetweennessCentrality {
          Directed = config.Directed,
          Weights = { MapperFunction = edge => config.EdgeWeights[edge] }
      }.Run(graph);
      results.SetCentrality(graph.Nodes, node => result.NodeCentrality[node],
          node => result.NormalizedNodeCentrality[node]);
      foreach (var edge in graph.Edges) {
        results.Gradient[edge] = result.NormalizedEdgeCentrality[edge];
        results.Labels[edge] = result.EdgeCentrality[edge].ToString("F2");
      }

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the closeness centrality values.
    /// </summary>
    internal static readonly string[] ClosenessCentralityDescription = [
        "This part of the demo shows the closeness centrality for the nodes of the given graph.",
        "Weights must be positive and can be specified using edge labels. The weights of edges without labels " +
        "are their edge lengths.",
        "When the algorithm should use Uniform weights all edges have equal weights. This algorithm can take " +
        "the direction of edges into account.",
        "Note that, for disconnected graphs the algorithm will be applied to each connected component " + "separately.",
        "The centrality of all nodes will be zero for directed graphs if there exists a node that is not " +
        "reachable from all other nodes.",
        "Larger and darker nodes are more central than small, light ones. The label shows the exact centrality " +
        "value."
    ];

    /// <summary>
    ///   Calculates the closeness centrality values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateClosenessCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      // Run separately on disconnected components; normalized values avoid very small labels.
      foreach (var component in new ConnectedComponents().Run(graph).Components) {
        var result = new ClosenessCentrality {
            Directed = config.Directed,
            Weights = { MapperFunction = edge => config.EdgeWeights[edge] },
            SubgraphNodes = { Source = component.Nodes }
        }.Run(graph);
        results.SetCentrality(component.Nodes, node => result.NormalizedNodeCentrality[node],
            node => result.NormalizedNodeCentrality[node]);
      }

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the Eigenvector centrality values.
    /// </summary>
    internal static readonly string[] EigenvectorCentralityDescription = [
        "This part of the demo shows the Eigenvector centrality for the nodes of the given graph.",
        "Eigenvector centrality measures the influence of a node in an undirected, unweighted network. " +
        "Connections to other influential nodes increase a node's centrality.",
        "The centrality values are scaled so that the largest centrality value is 1.0.",
        "Larger and darker nodes are more central than small, light ones. The label shows the exact centrality " +
        "value."
    ];

    /// <summary>
    ///   Calculates the Eigenvector centrality values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculateEigenvectorCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new EigenvectorCentrality().Run(graph);
      results.SetCentrality(graph.Nodes, node => result.NodeCentrality[node], node => result.NodeCentrality[node]);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines the page rank values.
    /// </summary>
    internal static readonly string[] PageRankDescription = [
        "This part of the demo shows the PageRank values for the nodes of the given graph.",
        "This algorithm can take the direction of edges into account.",
        "The algorithm starts by initializing the rank value for each node. Then, it uses multiple iterations " +
        "to propagate the rank of a node to its successors. The base factor for each successor is 1. These base " +
        "factors are multiplied by edge weights and node weights if specified.",
        "The final node factors are scaled afterward to sum up to 1, so the overall rank stays the same after " +
        "each iteration. The old rank value of the node multiplied with these scaled factors are then " +
        "distributed to the successors.",
        "After each iteration, the relative change of each node's rank value ((newRank - oldRank)/oldRank) is " +
        "calculated. If all rank value changes are below the specified precision or if the maximal iterations " +
        "are reached, the algorithm stops.",
        "Larger and darker nodes have a higher PageRank value than small, light ones. The label shows the exact " +
        "PageRank value."
    ];

    /// <summary>
    ///   Calculates the page rank values for the given graph.
    /// </summary>
    internal static AnalysisResult CalculatePageRankCentrality(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new PageRank {
          EdgeDirectedness = { Constant = config.Directed ? 1 : 0 },
          EdgeWeights = { MapperFunction = edge => config.EdgeWeights[edge] }
      }.Run(graph);
      var max = graph.Nodes.Select(node => result.PageRank[node]).DefaultIfEmpty(0).Max();
      results.SetCentrality(graph.Nodes, node => result.PageRank[node],
          node => max > 0 ? result.PageRank[node] / max : 0);

      return results;
    }
  }
}
