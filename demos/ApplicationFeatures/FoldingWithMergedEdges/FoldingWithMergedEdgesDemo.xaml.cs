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
using System.Text.Json;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.FoldingWithMergedEdges
{
  /// <summary>
  ///   Shows how to enable the merging of edges when collapsing group nodes.
  /// </summary>
  [Demo("folding-with-merged-edges", "application-features", "Folding With Merged Edges",
      Summary = "Shows how to enable the merging of edges when collapsing group nodes.", Tags = ["folding", "grouping"],
      Keywords = [
          "masterGraph", "wrappedGraph", "FoldingManager", "MergingFoldingEdgeConverter", "expand", "nesting", "hide",
          "collapse"
      ])]
  public sealed partial class FoldingWithMergedEdgesDemo
  {
    public FoldingWithMergedEdgesDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode();

      // enable folding for the graph
      var foldingView = EnableFolding();

      // assign the folded graph to the graph control
      graphControl.Graph = foldingView.Graph;

      // configures default styles for newly created graph elements
      InitializeGraph(foldingView.Manager.MasterGraph);

      // build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      foldingView.Manager.MasterGraph.SetUndoEngineEnabled(true);
      // also enable undo for folding operations
      foldingView.EnqueueNavigationalUndoUnits = true;
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Enables merging of folding edges. Adds labels with the number of merged edges.
    /// </summary>
    private sealed class LabeledMergingFoldingEdgeConverter : MergingFoldingEdgeConverter
    {
      public override void InitializeFoldingEdgeState(
          FoldingEdgeState state,
          IFoldingView foldingView,
          IEdge foldingEdge,
          IReadOnlyList<IEdge> masterEdges) {
        UpdateEdgeCountLabel(state, masterEdges);
        base.InitializeFoldingEdgeState(state, foldingView, foldingEdge, masterEdges);
      }

      public override void UpdateFoldingEdgeState(
          FoldingEdgeState state,
          IFoldingView foldingView,
          IEdge foldingEdge,
          IReadOnlyList<IEdge> masterEdges) {
        UpdateEdgeCountLabel(state, masterEdges);
        base.UpdateFoldingEdgeState(state, foldingView, foldingEdge, masterEdges);
      }

      /// <summary>
      ///   Adds or updates the label showing the number of merged edges.
      /// </summary>
      private static void UpdateEdgeCountLabel(FoldingEdgeState state, IReadOnlyList<IEdge> masterEdges) {
        var labelText = $"E: {masterEdges.Count}";

        var label = state.Labels.FirstOrDefault();
        if (label != null) {
          label.Text = labelText;
        } else {
          state.AddLabel(labelText, NinePositionsEdgeLabelModel.CenterCentered);
        }
      }
    }

    /// <summary>
    ///   Enables folding.
    /// </summary>
    /// <returns>The folding view that manages the folded graph.</returns>
    private static IFoldingView EnableFolding() {
      var masterGraph = new DefaultGraph();

      // set default styles for newly created graph elements
      InitializeGraph(masterGraph);

      // Creates the folding manager
      var manager = new FoldingManager(masterGraph);
      manager.FoldingEdgeConverter = new LabeledMergingFoldingEdgeConverter();
      // Creates a folding view that manages the folded graph
      return manager.CreateFoldingView();
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph, Themes.Palette12, edgeTheme: Themes.Palette12, groupTheme: Themes.Palette12,
          foldingEnabled: true);
    }

    [STAThread]
    private static void Main() {
      Run<FoldingWithMergedEdgesDemo>();
    }
  }
}
