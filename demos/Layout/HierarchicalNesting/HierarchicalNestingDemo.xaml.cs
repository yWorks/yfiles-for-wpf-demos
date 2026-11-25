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
using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalNesting
{
  /// <summary>
  ///   This demo shows how to nicely expand and collapse sub-graphs organized in groups.
  /// </summary>
  [Demo("hierarchical-nesting", "layout", "Hierarchical Nesting",
      Summary = "The hierarchical layout nicely expands and collapses sub-graphs organized in groups.",
      Tags = ["layout", "hierarchical", "animation"],
      Keywords = ["overview", "folding", "hide", "grouping", "from sketch", "padding"], RelatedDemos = [])]
  public sealed partial class HierarchicalNestingDemo
  {
    /// <summary>
    ///   Initializes folding, the overview, and the sample graph.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphViewerInputMode {
          FocusableItems = GraphItemTypes.None,
          SelectableItems = GraphItemTypes.None
      };

      // set up folding for this graph component
      var foldingManager = new FoldingManager();
      var foldingView = foldingManager.CreateFoldingView();
      graphControl.Graph = foldingView.Graph;

      // managing the appearance of folder nodes
      foldingManager.FolderNodeConverter = new FolderNodeConverter {
          FolderNodeDefaults = {
              CopyLabels = true,
              Size = new SizeD(110, 60)
          }
      };

      // set up the layout which is automatically applied when a group node is collapsed/expanded
      InteractiveHierarchicalNestingLayout.Initialize(graphControl);

      // import the sample
      LoadSampleGraph(foldingManager, foldingView);
    }

    /// <summary>
    ///   Create the hierarchical layout that we will be using initially.
    /// </summary>
    /// <param name="fromSketchMode">Whether this is for the first run or the incremental run.</param>
    private static HierarchicalLayout CreateHierarchicalLayout(bool fromSketchMode = false) =>
        new() {
            FromSketchMode = fromSketchMode,
            DefaultEdgeDescriptor = { RecursiveEdgePolicy = RecursiveEdgePolicy.Directed }
        };

    /// <summary>
    ///   Loads the sample graph from JSON data and applies an initial layout.
    /// </summary>
    private void LoadSampleGraph(FoldingManager foldingManager, IFoldingView foldingView) {
      // use the default demo styles for the sample graph items
      DemoStyles.InitDemoStyles(foldingManager.MasterGraph, foldingEnabled: true);

      // build the graph from the JSON data
      BuildGraph(foldingManager.MasterGraph, foldingView);

      // Apply an initial layout with a configuration that fits the interactive layout when
      // expanding/collapsing group nodes.
      var hierarchicalLayout = CreateHierarchicalLayout(true);
      graphControl.Graph.ApplyLayout(hierarchicalLayout);

      // center the arranged graph in the visible area
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Builds the graph using the JSON Data.
    ///   After building the graph a hierarchical layout is applied.
    /// </summary>
    /// <param name="graph">The folding view's master graph.</param>
    /// <param name="foldingView">The folding view used to collapse groups.</param>
    private void BuildGraph(IGraph graph, IFoldingView foldingView) {
      // use the main graph to build the unfolded graph from the data
      BuildGraphFromJson("graph-data.json", graph, groupNodeTagType: typeof(GroupData));

      // no layout information available, yet
      // so we come up with an initial layout for the complete, expanded graph.
      graph.ApplyLayout(CreateHierarchicalLayout());

      // collapsing the groups whose tags are collapsed
      var viewGraph = foldingView.Graph;
      viewGraph.Nodes.Where(node => viewGraph.IsGroupNode(node) && node.Tag is GroupData { Collapsed: true }).ToList()
          .ForEach(foldingView.Collapse);
    }

    private sealed record GroupData
    {
      public bool Collapsed { get; set; }
    }

    [STAThread]
    private static void Main(string[] args) => Run<HierarchicalNestingDemo>();

    public HierarchicalNestingDemo() {
      InitializeComponent();
    }
  }
}
