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

using System.Diagnostics;
using System.Windows.Navigation;
using yWorks.Analysis;
using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  public sealed partial class GraphAnalysisDemo
  {
    /// <summary>
    ///   Registers listener to graph changes to update the graph information.
    /// </summary>
    private void InitializeGraphInformation() {
      void OnStructureChanged() {
        QueueApplyAlgorithm();
        if (!changing) {
          UpdateGraphInformation();
        }
      }

      Graph.NodeCreated += (_, _) => OnStructureChanged();
      Graph.NodeRemoved += (_, _) => OnStructureChanged();

      // edge creation
      Graph.EdgeCreated += (_, _) => OnStructureChanged();
      Graph.EdgeRemoved += (_, _) => OnStructureChanged();
      Graph.EdgePortsChanged += (_, _) => OnStructureChanged();

      var engine = Graph.GetUndoEngine();
      if (engine != null) {
        engine.UnitRedone += (_, _) => OnStructureChanged();
        engine.UnitUndone += (_, _) => OnStructureChanged();
      }
    }

    /// <summary>
    ///   Updates the table that holds information about the graph.
    /// </summary>
    private void UpdateGraphInformation() {
      UpdateNumberOfElements();
      UpdateStructureAnalysis();
    }

    /// <summary>
    ///   Returns the number of nodes in the given graph.
    /// </summary>
    private static int GetNodeCount(IGraph graph) => graph.Nodes.Count;

    /// <summary>
    ///   Returns the number of edges in the given graph.
    /// </summary>
    private static int GetEdgeCount(IGraph graph) => graph.Edges.Count;

    /// <summary>
    ///   Checks whether the given graph is acyclic.
    /// </summary>
    private static bool IsAcyclic(GraphStructureAnalyzer analyzer) => analyzer.IsAcyclic(false);

    /// <summary>
    ///   Checks whether the given graph is bipartite.
    /// </summary>
    private static bool IsBipartite(GraphStructureAnalyzer analyzer) => analyzer.IsBipartite();

    /// <summary>
    ///   Checks whether the given graph is connected.
    /// </summary>
    private static bool IsConnected(GraphStructureAnalyzer analyzer) => analyzer.IsConnected();

    /// <summary>
    ///   Checks whether the given graph is biconnected.
    /// </summary>
    private static bool IsBiconnected(GraphStructureAnalyzer analyzer) => analyzer.IsBiconnected();

    /// <summary>
    ///   Checks whether the given graph is strongly connected.
    /// </summary>
    private static bool IsStronglyConnected(GraphStructureAnalyzer analyzer) => analyzer.IsStronglyConnected();

    /// <summary>
    ///   Checks whether the given graph is planar.
    /// </summary>
    private static bool IsPlanar(GraphStructureAnalyzer analyzer) => analyzer.IsPlanar();

    /// <summary>
    ///   Checks whether the given graph is a tree.
    /// </summary>
    private static bool IsTree(GraphStructureAnalyzer analyzer) => analyzer.IsTree(false);

    /// <summary>
    ///   Checks whether the given graph contains self-loop edges.
    /// </summary>
    private static bool HasSelfLoops(GraphStructureAnalyzer analyzer) => analyzer.HasSelfLoops();

    /// <summary>
    ///   Checks whether the given graph has multiple edges between any two nodes.
    /// </summary>
    private static bool HasMultipleEdges(GraphStructureAnalyzer analyzer) => analyzer.HasMultiEdges();

    /// <summary>
    ///   Updates the number of graph elements the panel.
    /// </summary>
    private void UpdateNumberOfElements() {
      settings.NodeCount = GetNodeCount(Graph);
      settings.EdgeCount = GetEdgeCount(Graph);
    }

    /// <summary>
    ///   Updates the results of structure analysis in the panel.
    /// </summary>
    private void UpdateStructureAnalysis() {
      var analyzer = new GraphStructureAnalyzer(Graph);
      settings.Structures = [
          new("Acyclic", IsAcyclic(analyzer), "https://en.wikipedia.org/wiki/Cycle_(graph_theory)"),
          new("Bipartite", IsBipartite(analyzer), "https://en.wikipedia.org/wiki/Bipartite_graph"),
          new("Connected", IsConnected(analyzer), "https://en.wikipedia.org/wiki/Connectivity_(graph_theory)"),
          new("Biconnected", IsBiconnected(analyzer), "https://en.wikipedia.org/wiki/Biconnected_graph"),
          new("Strongly Connected", IsStronglyConnected(analyzer),
              "https://en.wikipedia.org/wiki/Strongly_connected_component"),

          new("Planar", IsPlanar(analyzer), "https://en.wikipedia.org/wiki/Planar_graph"),
          new("Tree", IsTree(analyzer), "https://en.wikipedia.org/wiki/Tree_(graph_theory)"),
          new("Self-Loops", HasSelfLoops(analyzer), "https://en.wikipedia.org/wiki/Loop_(graph_theory)"),
          new("Multiple Edges", HasMultipleEdges(analyzer), "https://en.wikipedia.org/wiki/Multiple_edges")
      ];
    }

    private void OnStructureLink(object sender, RequestNavigateEventArgs e) {
      Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
      e.Handled = true;
    }
  }
}
