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
  /// <summary>
  ///   Calculates the connectivity analyses for the current graph.
  /// </summary>
  internal static class Connectivity
  {
    /// <summary>
    ///   Description of the algorithm which finds connected components.
    /// </summary>
    internal static readonly string[] ConnectedComponentsDescription = [
        "This part of the demo shows the connected components of the given graph.",
        "Nodes and edges that belong to the same component share the same color."
    ];

    /// <summary>
    ///   Calculates the connected components of the given graph.
    /// </summary>
    internal static AnalysisResult CalculateConnectedComponents(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var i = 0;
      foreach (var component in new ConnectedComponents().Run(graph).Components) {
        results.MarkGroup(component.Nodes, component.InducedEdges, i++);
      }

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which finds biconnected components.
    /// </summary>
    internal static readonly string[] BiconnectedComponentsDescription = [
        "This part of the demo shows the biconnected components of the given graph.",
        "Nodes and edges that belong to the same component share the same color. Articulation points present " +
        "all colors of the corresponding components. A component can be brought to focus by selecting the color " +
        "at an articulation point."
    ];

    /// <summary>
    ///   Calculates the biconnected components of the given graph.
    /// </summary>
    internal static AnalysisResult CalculateBiconnectedComponents(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var i = 0;
      foreach (var component in new BiconnectedComponents().Run(graph).Components) {
        results.MarkGroup(component.Nodes, component.Edges, i++);
      }

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which finds strongly connected components.
    /// </summary>
    internal static readonly string[] StronglyConnectedComponentsDescription = [
        "This part of the demo shows the strongly connected components of the given graph.",
        "Nodes and edges that belong to the same component share the same color."
    ];

    /// <summary>
    ///   Calculates the strongly connected components of the given graph.
    /// </summary>
    internal static AnalysisResult CalculateStronglyConnectedComponents(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var i = 0;
      foreach (var component in new StronglyConnectedComponents().Run(graph).Components) {
        results.MarkGroup(component.Nodes, component.InducedEdges, i++);
      }

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which finds all reachable nodes from a marked node.
    /// </summary>
    internal static readonly string[] ReachabilityDescription = [
        "This part of the demo highlights the set of nodes that are reachable in the given graph when starting " +
        "from the marked source. The source can be marked using the Context Menu. If no node is marked as the " +
        "source, a random node will be selected.",
        "The algorithm can take the direction of edges into account."
    ];

    /// <summary>
    ///   Calculates the nodes reachable from the marked node.
    /// </summary>
    internal static AnalysisResult CalculateReachableNodes(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();
      var source = config.StartNodes.FirstOrDefault() ?? graph.Nodes.First();

      var result = new Reachability {
          Directed = config.Directed,
          StartNodes = { Item = source }
      }.Run(graph);
      results.MarkGroup(result.ReachableNodes,
          graph.Edges.Where(edge =>
              result.IsReachable(edge.GetSourceNode()) && result.IsReachable(edge.GetTargetNode())), 0);

      return results;
    }

    /// <summary>
    ///   Description of the algorithm which finds k-core components.
    /// </summary>
    internal static readonly string[] KCoreComponentsDescription = [
        "This part of the demo shows the k-core of the given graph.",
        "The k-core of an undirected input graph consists of the subgraph components where each node has at " +
        "least degree k.",
        "Nodes and edges can be members of multiple k-cores. Choose a nodes' color to highlight the k-core " +
        "related to this color.",
        "The number in every node represents the highest k-Core the node belongs to.",
        "The k-Core for k=0 is not visualized in this demo, as every node is a member."
    ];

    /// <summary>
    ///   Calculates the k-core components of the given graph.
    /// </summary>
    internal static AnalysisResult CalculateKCoreComponents(IGraph graph, AlgorithmConfig config) {
      var results = new AnalysisResult();

      var result = new KCoreComponents().Run(graph);
      if (result.MaximumK == 0) {
        return results;
      }

      for (var k = result.MaximumK; k > 0; k--) {
        var core = result.GetKCore(k).ToHashSet();
        results.MarkGroup(core,
            graph.Edges.Where(edge => core.Contains(edge.GetSourceNode()) && core.Contains(edge.GetTargetNode())), k);
      }

      foreach (var node in graph.Nodes) {
        results.Labels[node] = result.KCores[node].ToString();
      }

      return results;
    }
  }
}
