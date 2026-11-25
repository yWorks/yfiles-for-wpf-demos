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

using yWorks.Analysis;
using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  internal static class Substructures
  {
    /// <summary>
    ///   Description of the algorithm which determines chain substructures in the graph.
    /// </summary>
    internal static readonly string[] ChainsSubstructuresDescription = [
        "This part of the demo shows chain substructures in the graph.",
        "A chain is a simple path consisting of nodes connected with edges.",
        "If the algorithm is configured to take edge directedness into account, all edges along the path must " +
        "point into the same direction, i.e. if the edge directions alter along the path, the path is not " +
        "recognized as a chain substructure."
    ];

    /// <summary>
    ///   Calculates the chain substructures in the graph in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateChainSubstructures(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      results.MarkSubstructures(new ChainSubstructures {
          MinimumSize = 2,
          EdgeDirectedness = { Constant = config.Directed ? 1 : 0 }
      }.Run(graph).Chains);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines cycle substructures in the graph.
    /// </summary>
    internal static readonly string[] CycleSubstructuresDescription = [
        "This part of the demo shows cycles substructures in the graph.",
        "A cycle is a simple path in which the first and last nodes are identical. The algorithm only considers " +
        "cycles in which exactly one edge connects a cycle node with the rest of the graph, i.e. isolated " + "cycles.",
        "If the algorithm is configured to take edge directedness into account all edges along the path must " +
        "point into the same direction, i.e. if the edge directions alter along the path, the path is not " +
        "recognized as a cycle substructure."
    ];

    /// <summary>
    ///   Calculates the cycle substructures in the graph in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateCycleSubstructures(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      results.MarkSubstructures(new CycleSubstructures {
          MinimumSize = 2,
          EdgeDirectedness = { Constant = config.Directed ? 1 : 0 }
      }.Run(graph).Cycles);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines star substructures in the graph.
    /// </summary>
    internal static readonly string[] StarSubstructuresDescription = [
        "This part of the demo shows star substructures in the graph.",
        "A star consists of a root that is connected to multiple nodes with degree one.",
        "If edge directedness is taken into account, all edges must either point away from or towards the root " +
        "node. If for a root node there are differing edge directions present the algorithm returns the largest " +
        "star for the root node."
    ];

    /// <summary>
    ///   Calculates the star substructures in the graph in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateStarSubstructures(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      results.MarkSubstructures(new StarSubstructures {
          MinimumSize = 3,
          EdgeDirectedness = { Constant = config.Directed ? 1 : 0 }
      }.Run(graph).Stars);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines tree substructures in the graph.
    /// </summary>
    internal static readonly string[] TreeSubstructuresDescription = [
        "This part of the demo shows tree substructures in the graph.",
        "The root of a tree is the only node which may have non-tree edges.",
        "If edge directedness is taken into account, all edges along the tree substructure must point into the " +
        "same direction, i.e. from the root to the leafs."
    ];

    /// <summary>
    ///   Calculates the tree substructures in the graph in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateTreeSubstructures(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      results.MarkSubstructures(new TreeSubstructures {
          MinimumSize = 2,
          EdgeDirectedness = { Constant = config.Directed ? 1 : 0 }
      }.Run(graph).Trees);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which determines clique substructures in the graph.
    /// </summary>
    internal static readonly string[] CliquesSubstructuresDescription = [
        "This part of the demo shows cliques substructures in the graph.",
        "A clique is a subset of nodes that are all adjacent (i.e. connected) to each other.",
        "Note that finding a maximum clique is NP-hard. Hence, only a simple heuristic approach that does not " +
        "guarantee to find all/the largest clique(s) is used.",
        "A node can only be contained in a single clique, i.e. the returned cliques are non-overlapping."
    ];

    /// <summary>
    ///   Calculates the clique substructures in the graph in the given graph.
    /// </summary>
    internal static AnalysisResult CalculateCliqueSubstructures(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      results.MarkSubstructures(new CliqueSubstructures { MinimumSize = 2 }.Run(graph).Cliques);

      return results;
    }
  }
}
