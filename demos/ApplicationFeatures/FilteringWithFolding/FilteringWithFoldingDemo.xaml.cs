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
using System.Text.Json;
using System.Windows;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.FilteringWithFolding
{
  [Demo("filtering-with-folding", "application-features", "Filtering With Folding",
      Summary = "Shows how to configure filtering and folding in the same application.",
      Tags = ["filtering", "grouping"], Keywords = [
          "FilteredGraphWrapper", "subset", "folder", "folding", "predicate", "masterGraph", "wrappedGraph",
          "FoldingManager", "nesting", "folded", "hide"
      ])]
  public sealed partial class FilteringWithFoldingDemo
  {
    public FilteringWithFoldingDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode { SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge };

      // enable filtering and folding for the graph
      var foldingView = EnableFilteringAndFolding();

      // assign the folded graph to the graph control
      graphControl.Graph = foldingView.Graph;

      var fullGraph = ((FilteredGraphWrapper) foldingView.Manager.MasterGraph).WrappedGraph!;

      // make sure state tags are on all created items
      fullGraph.NodeCreated += (_, evt) => evt.Item.Tag = new FilteringTag();
      fullGraph.EdgeCreated += (_, evt) => evt.Item.Tag = new FilteringTag();

      // configures default styles for newly created graph elements
      InitializeGraph(fullGraph);

      // then build the graph with the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      fullGraph.SetUndoEngineEnabled(true);
      // also enable undo for folding operations
      foldingView.EnqueueNavigationalUndoUnits = true;

      // update the reset filter button depending on the current graph state
      fullGraph.GetUndoEngine()!.UnitUndone += (_, _) => UpdateResetButtonState();
      fullGraph.GetUndoEngine()!.UnitRedone += (_, _) => UpdateResetButtonState();

      // bind the buttons to their functionality
      InitializeUI();
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json, typeof(FilteringTag), typeof(FilteringTag), typeof(FilteringTag));
    }

    /// <summary>
    ///   Enables filtering and folding.
    ///   When utilizing both features on the same graph, it is important to apply filtering before folding. Therefore,
    ///   use <see cref="FilteredGraphWrapper"/> to create a filtered view of the graph and after that, use
    ///   <see cref="FoldingManager"/> to create a folding view of the filtered graph.
    /// </summary>
    /// <returns>The folding view that manages the folded graph.</returns>
    private static IFoldingView EnableFilteringAndFolding() {
      // the unfiltered, unfolded master graph
      var fullGraph = new DefaultGraph();

      // set default styles for newly created graph elements
      InitializeGraph(fullGraph);

      // we want to hide items whose tag contains the string 'filtered'
      Predicate<INode> nodePredicate = node => node.Tag is not FilteringTag { Filtered: true };
      Predicate<IEdge> edgePredicate = edge => edge.Tag is not FilteringTag { Filtered: true };

      // create a filtered graph
      var filteredGraph = new FilteredGraphWrapper(fullGraph, nodePredicate, edgePredicate);

      // create a folding manager
      var manager = new FoldingManager(filteredGraph);

      // create a folding view that manages the folded graph
      return manager.CreateFoldingView();
    }

    /// <summary>
    ///   Changes the filtered state of the tag of an edge or node while also adding an undo unit for it.
    /// </summary>
    private void FilterItemWithUndoUnit(INode item, bool state) {
      FilterItemWithUndoUnit((FilteringTag) item.Tag!, state);
    }

    private void FilterItemWithUndoUnit(IEdge item, bool state) {
      FilterItemWithUndoUnit((FilteringTag) item.Tag!, state);
    }

    private void FilterItemWithUndoUnit(FilteringTag tag, bool state) {
      var filteredGraph = (FilteredGraphWrapper) graphControl.Graph.GetFoldingView()!.Manager.MasterGraph;
      var fullGraph = filteredGraph.WrappedGraph!;
      fullGraph.GetUndoEngine()!.AddUnit(new ChangeFilterStateUndoUnit(filteredGraph, tag));
      tag.Filtered = state;
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

    /// <summary>
    ///   Binds the buttons in the toolbar to their functionality.
    /// </summary>
    private void InitializeUI() {
      // adds a listener for the current selection to enable/disable the filter button
      graphControl.Selection.ItemAdded += (_, _) => filterItemsButton.IsEnabled = graphControl.Selection.Count > 0;
      graphControl.Selection.ItemRemoved += (_, _) => filterItemsButton.IsEnabled = graphControl.Selection.Count > 0;
    }

    private void OnFilterItems(object sender, RoutedEventArgs e) {
      // mark the selected items such that the nodePredicate or edgePredicate will filter them
      foreach (var node in graphControl.Selection.Nodes) {
        FilterItemWithUndoUnit(node, true);
      }
      foreach (var edge in graphControl.Selection.Edges) {
        FilterItemWithUndoUnit(edge, true);
      }

      // re-evaluate the filter predicates to actually hide the items
      var filteredGraph = (FilteredGraphWrapper) graphControl.Graph.GetFoldingView()!.Manager.MasterGraph;
      filteredGraph.NodePredicateChanged();
      filteredGraph.EdgePredicateChanged();

      // enable the reset button
      resetFilterButton.IsEnabled = true;
    }

    private void OnResetFilter(object sender, RoutedEventArgs e) {
      // access the unfiltered, unfolded graph to remove the filter mark from all items
      var filteredGraph = (FilteredGraphWrapper) graphControl.Graph.GetFoldingView()!.Manager.MasterGraph;
      var fullGraph = filteredGraph.WrappedGraph!;
      foreach (var node in fullGraph.Nodes) {
        FilterItemWithUndoUnit(node, false);
      }
      foreach (var edge in fullGraph.Edges) {
        FilterItemWithUndoUnit(edge, false);
      }

      // re-evaluate the filter predicates to actually show the items again
      filteredGraph.NodePredicateChanged();
      filteredGraph.EdgePredicateChanged();

      // disable the reset button
      resetFilterButton.IsEnabled = false;
    }

    /// <summary>
    ///   Updates the 'Reset Filter' button state based on the current graph state.
    /// </summary>
    private void UpdateResetButtonState() {
      var filteredGraph = (FilteredGraphWrapper) graphControl.Graph.GetFoldingView()!.Manager.MasterGraph;
      var fullGraph = filteredGraph.WrappedGraph!;
      var hasFilteredItems = fullGraph.Nodes.Any(node => node.Tag is FilteringTag { Filtered: true }) ||
          fullGraph.Edges.Any(edge => edge.Tag is FilteringTag { Filtered: true });
      // set the reset button
      resetFilterButton.IsEnabled = hasFilteredItems;
    }

    /// <summary>
    ///   An undo unit to keep track of the filtered state changes on the graph items.
    /// </summary>
    private sealed class ChangeFilterStateUndoUnit : IUndoUnit
    {
      private readonly FilteredGraphWrapper filteredGraph;
      private readonly FilteringTag tag;
      private readonly bool oldState;
      private bool newState;

      public ChangeFilterStateUndoUnit(FilteredGraphWrapper filteredGraph, FilteringTag tag) {
        this.filteredGraph = filteredGraph;
        // remember the changed object
        this.tag = tag;
        // remember the old value
        oldState = tag.Filtered;
      }

      public string RedoName => "Change Filter State";

      public string UndoName => "Change Filter State";

      public void Undo() {
        // remember the new value for redo
        newState = tag.Filtered;
        // set the old value
        tag.Filtered = oldState;
        filteredGraph.NodePredicateChanged();
        filteredGraph.EdgePredicateChanged();
      }

      public void Redo() {
        // set the new value
        tag.Filtered = newState;
        filteredGraph.NodePredicateChanged();
        filteredGraph.EdgePredicateChanged();
      }

      public bool TryMergeUnit(IUndoUnit unit) => false;

      public bool TryReplaceUnit(IUndoUnit unit) => false;

      public void Dispose() { }
    }

    [STAThread]
    private static void Main() => Run<FilteringWithFoldingDemo>();
  }
}
