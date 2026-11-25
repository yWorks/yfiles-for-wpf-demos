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
using System.Windows.Threading;
using yWorks.Analysis;
using yWorks.Graph;
using static yWorks.Demo.GraphAnalysis.DemoTypes;

namespace yWorks.Demo.GraphAnalysis
{
  /// <summary>
  ///   Type containing all the information needed to calculate and display the results.
  ///   It also stores the according sample graph and a description.
  /// </summary>
  internal sealed class Algorithm
  {
    public string Name { get; init; } = null!;

    public string Sample { get; init; } = null!;

    public IReadOnlyList<string> Description { get; init; } = null!;

    public Func<IGraph, AlgorithmConfig, AnalysisResult> Apply { get; init; } = null!;

    public bool DirectedOnly { get; init; }

    public bool SupportsDirectedness { get; init; }

    public bool SupportsEdgeWeights { get; init; }

    public bool NeedsStartNodes { get; init; }

    public bool NeedsEndNodes { get; init; }

    public bool DefaultDirected { get; init; }
  }

  /// <summary>
  ///   Type containing the configuration properties for an analysis algorithm.
  ///   The values are derived from the graph and the toolbar.
  /// </summary>
  internal sealed class AlgorithmConfig
  {
    public bool Directed { get; init; }

    public IReadOnlyDictionary<IEdge, double> EdgeWeights { get; init; } = null!;

    public IReadOnlyList<INode> StartNodes { get; init; } = [];

    public IReadOnlyList<INode> EndNodes { get; init; } = [];
  }

  /// <summary>
  ///   A set of graph analysis algorithms.
  /// </summary>
  internal static class AlgorithmCatalog
  {
    internal static readonly IReadOnlyList<Algorithm> Algorithms = [
        new() {
            Name = "Minimum Spanning Tree",
            Sample = "spanningTreeSample",
            Apply = MinimumSpanningTree.CalculateMinimumSpanningTree,
            Description = MinimumSpanningTree.MinimumSpanningTreeDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = true,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Connected Components",
            Sample = "connectivitySample",
            Apply = Connectivity.CalculateConnectedComponents,
            Description = Connectivity.ConnectedComponentsDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Biconnected Components",
            Sample = "connectivitySample",
            Apply = Connectivity.CalculateBiconnectedComponents,
            Description = Connectivity.BiconnectedComponentsDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Strongly Connected Components",
            Sample = "connectivitySample",
            Apply = Connectivity.CalculateStronglyConnectedComponents,
            Description = Connectivity.StronglyConnectedComponentsDescription,
            DirectedOnly = true,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = true
        },
        new() {
            Name = "Reachability",
            Sample = "connectivitySample",
            Apply = Connectivity.CalculateReachableNodes,
            Description = Connectivity.ReachabilityDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = true,
            NeedsEndNodes = false,
            DefaultDirected = true
        },
        new() {
            Name = "k-Core Components",
            Sample = "kCoreSample",
            Apply = Connectivity.CalculateKCoreComponents,
            Description = Connectivity.KCoreComponentsDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Shortest Paths",
            Sample = "pathsSample",
            Apply = Paths.CalculateShortestPaths,
            Description = Paths.ShortestPathsDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = true,
            NeedsEndNodes = true,
            DefaultDirected = false
        },
        new() {
            Name = "k-Shortest Paths",
            Sample = "substructuresCliquesSample",
            Apply = Paths.CalculateKShortestPaths,
            Description = Paths.KShortestPathsDescription,
            DirectedOnly = true,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = true,
            NeedsEndNodes = true,
            DefaultDirected = true
        },
        new() {
            Name = "All Paths",
            Sample = "pathsSample",
            Apply = Paths.CalculateAllPaths,
            Description = Paths.AllPathsDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = true,
            NeedsEndNodes = true,
            DefaultDirected = false
        },
        new() {
            Name = "All Chains",
            Sample = "pathsSample",
            Apply = Paths.CalculateAllChains,
            Description = Paths.AllChainsDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Single Source Shortest Paths",
            Sample = "pathsSample",
            Apply = Paths.CalculateSingleSourceShortestPaths,
            Description = Paths.SingleSourceShortestPathsDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = true,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Cycles",
            Sample = "cycleSample",
            Apply = Cycles.CalculateCycles,
            Description = Cycles.CyclesDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Degree Centrality",
            Sample = "centralitySample",
            Apply = Centrality.CalculateDegreeCentrality,
            Description = Centrality.DegreeCentralityDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Weight Centrality",
            Sample = "centralitySample",
            Apply = Centrality.CalculateWeightCentrality,
            Description = Centrality.WeightCentralityDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = true,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Graph Centrality",
            Sample = "centralitySample",
            Apply = Centrality.CalculateGraphCentrality,
            Description = Centrality.GraphCentralityDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Node Edge Betweenness Centrality",
            Sample = "centralitySample",
            Apply = Centrality.CalculateNodeEdgeBetweennessCentrality,
            Description = Centrality.NodeEdgeBetweennessCentralityDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Closeness Centrality",
            Sample = "centralitySample",
            Apply = Centrality.CalculateClosenessCentrality,
            Description = Centrality.ClosenessCentralityDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Eigenvector Centrality",
            Sample = "centralitySample",
            Apply = Centrality.CalculateEigenvectorCentrality,
            Description = Centrality.EigenvectorCentralityDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Page Rank",
            Sample = "centralitySample",
            Apply = Centrality.CalculatePageRankCentrality,
            Description = Centrality.PageRankDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = true,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Substructures Chains",
            Sample = "substructuresCyclesSample",
            Apply = Substructures.CalculateChainSubstructures,
            Description = Substructures.ChainsSubstructuresDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Substructures Cycles",
            Sample = "substructuresCyclesSample",
            Apply = Substructures.CalculateCycleSubstructures,
            Description = Substructures.CycleSubstructuresDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Substructures Stars",
            Sample = "substructuresCyclesSample",
            Apply = Substructures.CalculateStarSubstructures,
            Description = Substructures.StarSubstructuresDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Substructures Trees",
            Sample = "substructuresCyclesSample",
            Apply = Substructures.CalculateTreeSubstructures,
            Description = Substructures.TreeSubstructuresDescription,
            DirectedOnly = false,
            SupportsDirectedness = true,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        },
        new() {
            Name = "Substructures Cliques",
            Sample = "substructuresCliquesSample",
            Apply = Substructures.CalculateCliqueSubstructures,
            Description = Substructures.CliquesSubstructuresDescription,
            DirectedOnly = false,
            SupportsDirectedness = false,
            SupportsEdgeWeights = false,
            NeedsStartNodes = false,
            NeedsEndNodes = false,
            DefaultDirected = false
        }
    ];
  }

  public sealed partial class GraphAnalysisDemo
  {
    private bool changing;
    private bool refreshQueued;
    private AnalysisResult? result;

    /// <summary>
    ///   Resets the node types.
    /// </summary>
    private void ResetTypes() {
      foreach (var node in Graph.Nodes) {
        ResetType(node);
      }
    }

    /// <summary>
    ///   Applies the current algorithm with the correct configuration to the given graph.
    /// </summary>
    private void ApplyAlgorithm() {
      var currentAlgorithm = GetCurrentAlgorithm();
      if (changing || currentAlgorithm == null) {
        return;
      }

      changing = true;
      try {
        ResetGraph();

        var config = new AlgorithmConfig {
            Directed = UseDirectedEdges(),
            EdgeWeights = GetEdgeWeights(),
            StartNodes = GetStartNodes(),
            EndNodes = GetEndNodes()
        };
        result = Graph.Nodes.Any() ? currentAlgorithm.Apply(Graph, config) : new AnalysisResult();

        RenderResults();

        graphControl.Invalidate();
        componentSwitcher?.Invalidate();
      } finally {
        changing = false;
      }
    }

    /// <summary>
    ///   Resets the results of the previous algorithm.
    ///   Removes all result-labels and resets derived item data.
    /// </summary>
    private void ResetGraph() {
      using var presentation = ((AnalysisGraph) Graph).SuspendRecording();
      result = null;
      componentSwitcher?.Reset();
      foreach (var label in Graph.GetNodeLabels().Concat(Graph.GetEdgeLabels().Where(l => !Equals(l.Tag, "weight")))
          .ToArray()) {
        Graph.Remove(label);
      }
    }

    /// <summary>
    ///   Returns a map containing a weight value for each edge.
    /// </summary>
    private Dictionary<IEdge, double> GetEdgeWeights() {
      return Graph.Edges.ToDictionary(edge => edge, edge => {
        if (UseUniformEdgeWeights()) {
          return 1;
        }

        var text = edge.Labels.FirstOrDefault(l => Equals(l.Tag, "weight"))?.Text;
        return double.TryParse(text, out var n) && n > 0 ? n : 1;
      });
    }

    /// <summary>
    ///   Retrieves all nodes that are marked as start nodes.
    /// </summary>
    private List<INode> GetStartNodes() {
      var currentAlgorithm = GetCurrentAlgorithm();
      var needsStartNodes = currentAlgorithm?.NeedsStartNodes ?? false;
      if (!needsStartNodes || Graph.Nodes.Count == 0) {
        return [];
      }

      var startNodes = Graph.Nodes.Where(node => GetTag(node)?.Type == NodeType.Start).ToList();
      if (startNodes.Count == 0) {
        // Keep marked end nodes when choosing a default start.
        var startNode = Graph.Nodes.FirstOrDefault(node => GetTag(node)?.Type != NodeType.End) ?? Graph.Nodes.First();
        if (GetTag(startNode)?.Type != NodeType.End) {
          SetType(startNode, NodeType.Start);
        }

        startNodes.Add(startNode);
      }

      return startNodes;
    }

    /// <summary>
    ///   Retrieves all nodes that are marked as end nodes.
    /// </summary>
    private List<INode> GetEndNodes() {
      if (GetCurrentAlgorithm()?.NeedsEndNodes != true || Graph.Nodes.Count == 0) {
        return [];
      }

      var endNodes = Graph.Nodes.Where(node => GetTag(node)?.Type == NodeType.End).ToList();
      if (endNodes.Count == 0) {
        // Keep marked start nodes when choosing a default end.
        var endNode = Graph.Nodes.LastOrDefault(node => GetTag(node)?.Type != NodeType.Start) ?? Graph.Nodes.Last();
        var tag = GetTag(endNode);
        if (tag?.Type != NodeType.Start) {
          SetType(endNode, NodeType.End);
        }

        endNodes.Add(endNode);
      }

      return endNodes;
    }

    private void RenderResults() {
      if (result == null) {
        return;
      }

      using var presentation = ((AnalysisGraph) Graph).SuspendRecording();
      foreach (var (item, text) in result.Labels) {
        if (item is ILabelOwner owner) {
          Graph.AddLabel(owner, text, style: item is IEdge ? CentralityEdgeLabelStyle : null,
              tag: item is IEdge ? "centrality" : "result");
        }
      }
    }

    private void QueueApplyAlgorithm() {
      if (changing || refreshQueued) {
        return;
      }

      refreshQueued = true;
      Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
        refreshQueued = false;
        ApplyAlgorithm();
      }));
    }
  }

  /// <summary>
  ///   Stores the derived results used to display an analysis.
  /// </summary>
  internal sealed class AnalysisResult
  {
    // Items can belong to several components, such as overlapping paths or k-cores.
    internal readonly Dictionary<IModelItem, List<int>> Components = new();
    internal readonly Dictionary<INode, double> Centrality = new();
    internal readonly Dictionary<IModelItem, double> Gradient = new();
    internal readonly Dictionary<IModelItem, string> Labels = new();
  }

  /// <summary>
  ///   Shared helpers for marking and displaying analysis results.
  /// </summary>
  internal static class Algorithms
  {
    /// <summary>
    ///   Adds a component membership to a node or edge.
    /// </summary>
    internal static void MarkItem(this AnalysisResult results, IModelItem item, int component) {
      if (!results.Components.TryGetValue(item, out var ids)) {
        results.Components[item] = ids = [];
      }

      if (!ids.Contains(component)) {
        ids.Add(component);
      }
    }

    internal static void MarkGroup(
        this AnalysisResult results,
        IEnumerable<INode> nodes,
        IEnumerable<IEdge> edges,
        int id) {
      foreach (var node in nodes) {
        results.MarkItem(node, id);
      }

      foreach (var edge in edges) {
        results.MarkItem(edge, id);
      }
    }

    // A path can overlap others, so each of its items retains all component memberships.
    internal static void MarkPaths(this AnalysisResult results, IEnumerable<Path> paths, bool rank = false) {
      var i = 0;
      foreach (var path in paths) {
        results.MarkGroup(path.Nodes, path.Edges, i);
        if (rank) {
          foreach (var node in path.Nodes) {
            results.Labels.TryAdd(node, (i + 1).ToString());
          }
        }

        i++;
      }
    }

    internal static void MarkSubstructures(this AnalysisResult results, IEnumerable<SubstructureItems> structures) {
      var i = 0;
      foreach (var structure in structures) {
        results.MarkGroup(structure.Nodes, structure.Edges, i++);
      }
    }

    /// <summary>
    ///   Stores centrality values and the text to display on each node.
    /// </summary>
    internal static void SetCentrality(
        this AnalysisResult results,
        IEnumerable<INode> nodes,
        Func<INode, double> values,
        Func<INode, double> normalized) {
      foreach (var node in nodes) {
        var value = values(node);
        results.Centrality[node] = double.IsFinite(normalized(node)) ? Math.Clamp(normalized(node), 0, 1) : 0;
        results.Labels[node] = double.IsFinite(value) ? value.ToString("F2") : "Inf";
      }
    }
  }
}
