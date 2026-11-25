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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Filtering
{
  [Demo("filtering", "application-features", "Filtering", Summary = "Shows how to configure graph filtering.",
      Tags = ["filtering", "grouping"],
      Keywords = ["FilteredGraphWrapper", "subset", "predicates", "hide", "hiding", "playground"],
      RelatedDemos = ["filtering-with-folding", "flow-filtering"])]
  public sealed class FilteringDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Predicates to determine which items to hide
      Predicate<INode> nodePredicate = node => node.Tag is not FilteringTag { Filtered: true };
      Predicate<IEdge> edgePredicate = edge => edge.Tag is not FilteringTag { Filtered: true };

      // A filtered graph which wraps the original graph and applies the predicates
      var filteredGraph = new FilteredGraphWrapper(Graph, nodePredicate, edgePredicate);
      // fullGraph is a reference to the original, unfiltered graph wrapped by filteredGraph
      var fullGraph = filteredGraph.WrappedGraph!;

      // Assign the filtered graph to the graph control for display
      GraphControl.Graph = filteredGraph;

      // --- UI Button Setup ---

      // Add a button to filter selected items
      var filterItemsButton = ToolBar.AddButton("Filter Items", () => {
        // Mark the selected items such that the predicates will filter them
        foreach (var item in GraphControl.Selection.Nodes) {
          if (item.Tag is FilteringTag tag) {
            tag.Filtered = true;
          }
        }
        foreach (var item in GraphControl.Selection.Edges) {
          if (item.Tag is FilteringTag tag) {
            tag.Filtered = true;
          }
        }

        // Notify the filtered graph to re-evaluate its predicates and update the view
        filteredGraph.NodePredicateChanged();
        filteredGraph.EdgePredicateChanged();
      });

      // Add a button to reset all filters and show all items
      var resetFilterButton = ToolBar.AddButton("Reset Filter", () => {
        // Access the unfiltered graph to remove the filter mark from all items
        foreach (var item in fullGraph.Nodes) {
          if (item.Tag is FilteringTag tag) {
            tag.Filtered = false;
          }
        }
        foreach (var item in fullGraph.Edges) {
          if (item.Tag is FilteringTag tag) {
            tag.Filtered = false;
          }
        }

        // Notify the filtered graph to re-evaluate its predicates and update the view
        filteredGraph.NodePredicateChanged();
        filteredGraph.EdgePredicateChanged();
      });
      resetFilterButton.IsEnabled = false;
      resetFilterButton.Click += (_, _) => resetFilterButton.IsEnabled = false;
      filterItemsButton.Click += (_, _) => resetFilterButton.IsEnabled = true;

      // Enable / disable the filter button based on the current selection
      void UpdateFilterButtonState() => filterItemsButton.IsEnabled = GraphControl.Selection.Count > 0;
      UpdateFilterButtonState();

      // Add listeners for the current selection to enable / disable the filter button
      GraphControl.Selection.ItemAdded += (_, _) => UpdateFilterButtonState();
      GraphControl.Selection.ItemRemoved += (_, _) => UpdateFilterButtonState();

      // --- Graph Initialization ---

      BuildGraphFromJson("graph-data.json", typeof(FilteringTag), typeof(FilteringTag), typeof(FilteringTag));

      // Enable graph selection for nodes and edges
      GraphControl.InputMode = new GraphEditorInputMode { SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge };

      GraphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      GraphControl.FitGraphBounds();

      // Make sure state tags are on all newly created items
      fullGraph.NodeCreated += (_, args) => args.Item.Tag = new FilteringTag();
      fullGraph.EdgeCreated += (_, args) => args.Item.Tag = new FilteringTag();
    }

    [STAThread]
    private static void Main() => Run<FilteringDemo>();
  }
}
