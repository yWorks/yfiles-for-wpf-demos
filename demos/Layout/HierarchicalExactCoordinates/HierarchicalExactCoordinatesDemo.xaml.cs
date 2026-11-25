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
using System.Threading.Tasks;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalExactCoordinates
{
  [Demo("hierarchical-exact-coordinates", "layout", "Hierarchical Layout with Exact Coordinates",
      Summary = "Shows how to configure hierarchical layout from sketch with exact coordinates.",
      Tags = ["hierarchical", "from-sketch"],
      Keywords = ["layout", "subset", "nodes", "set", "node-set", "partial", "mental map", "fixed", "incremental"],
      RelatedDemos = ["partial-layout"])]
  public sealed partial class HierarchicalExactCoordinatesDemo
  {
    private INodeStyle incrementalNodeStyle = null!;
    private IEdgeStyle incrementalEdgeStyle = null!;
    private INodeStyle fixedNodeStyle = null!;
    private IEdgeStyle fixedEdgeStyle = null!;

    public HierarchicalExactCoordinatesDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      // Initialize default styles
      InitializeGraph();

      // Initialize interactive behavior
      InitializeInputModes();

      // Bind toolbar buttons to actions
      InitializeUI();

      // Load the graph
      await LoadGraphAsync();

      // Enable the undo engine
      graphControl.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Runs a hierarchical layout considering the incremental/fixed elements.
    /// </summary>
    private async Task RunLayoutAsync() {
      SetUIDisabled(true);

      // Create a hierarchical layout with fromSketchMode enabled
      var hierarchicalLayout = new HierarchicalLayout { FromSketchMode = true };

      // Configure the layout data to integrate the incremental nodes while the
      // remaining nodes should keep their coordinates as much as possible.
      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(graphControl.Graph);
      hierarchicalLayoutData.IncrementalNodeHints.MapperFunction = node =>
          IsFixed(node) ? IncrementalNodeHint.ExactCoordinates : IncrementalNodeHint.Incremental;

      // Run layout algorithm
      try {
        await graphControl.ApplyLayoutAnimated(hierarchicalLayout, TimeSpan.FromMilliseconds(500),
            hierarchicalLayoutData);
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Sets the default styles for the new graph elements.
    /// </summary>
    private void InitializeGraph() {
      // Initialize styles
      incrementalNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteOrange);
      incrementalEdgeStyle = DemoStyles.CreateDemoEdgeStyle(Themes.PaletteOrange);
      fixedNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.Palette58);
      fixedEdgeStyle = DemoStyles.CreateDemoEdgeStyle(Themes.Palette58);

      var graph = graphControl.Graph;
      graph.NodeDefaults.Size = new SizeD(60, 30);
      graph.NodeDefaults.Style = incrementalNodeStyle;
      graph.EdgeDefaults.Style = incrementalEdgeStyle;
    }

    /// <summary>
    ///   Configures input modes to interact with the graph structure.
    /// </summary>
    private void InitializeInputModes() {
      var inputMode = new GraphEditorInputMode {
          AllowEditLabel = false,
          AllowGroupingOperations = false,
          ShowHandleItems = GraphItemTypes.All & ~GraphItemTypes.Node
      };
      inputMode.ItemDoubleClicked += (_, evt) => {
        // A graph element was double-clicked => toggle its fixed/incremental state
        SetFixed(evt.Item, !IsFixed(evt.Item));
      };
      // Mark the node as non-fixed
      inputMode.NodeCreated += (_, evt) => SetFixed(evt.Item, false);
      inputMode.CreateEdgeInputMode.EdgeCreated += (_, evt) => SetFixed(evt.Item, false);
      graphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Sets the given item as fixed or movable and changes its color to indicate its new state.
    /// </summary>
    private void SetFixed(IModelItem item, bool fixedItem) {
      item.Tag = fixedItem ? "fixed" : "incremental";
      UpdateStyle(item);
    }

    /// <summary>
    ///   Returns if a given item is considered fixed or shall be rearranged by the layout algorithm.
    ///   Note that an edge always gets rerouted if any of its end nodes may be moved.
    /// </summary>
    private static bool IsFixed(IModelItem item) => item.Tag as string == "fixed";

    /// <summary>
    ///   Updates the style of the given item when the incremental/fixed state has changed.
    /// </summary>
    private void UpdateStyle(IModelItem item) {
      var graph = graphControl.Graph;
      if (item is INode node) {
        graph.SetStyle(node, IsFixed(node) ? fixedNodeStyle : incrementalNodeStyle);
      } else if (item is IEdge edge) {
        graph.SetStyle(edge, IsFixed(edge) ? fixedEdgeStyle : incrementalEdgeStyle);
      }
    }

    /// <summary>
    ///   Updates the incremental/fixed state of all graph elements that are currently selected.
    /// </summary>
    private void SetSelectionFixed(bool fixedItem) {
      var selection = graphControl.Selection;
      foreach (var node in selection.Nodes) {
        SetFixed(node, fixedItem);
      }
      foreach (var edge in selection.Edges) {
        SetFixed(edge, fixedItem);
      }
    }

    /// <summary>
    ///   Binds actions to the buttons in the toolbar.
    /// </summary>
    private void InitializeUI() {
      refreshButton.Click += async (_, _) => await LoadGraphAsync();
      lockSelectionButton.Click += (_, _) => SetSelectionFixed(true);
      unlockSelectionButton.Click += (_, _) => SetSelectionFixed(false);
      layoutButton.Click += async (_, _) => await RunLayoutAsync();
    }

    /// <summary>
    ///   Creates the sample graph for this demo.
    /// </summary>
    private async Task LoadGraphAsync() {
      var graph = graphControl.Graph;
      // The graph builder sets edge port locations and simplifies the edge tags to fixed/incremental values.
      BuildGraphFromJson("graph-data.json", graph);

      foreach (var node in graph.Nodes) {
        UpdateStyle(node);
      }
      foreach (var edge in graph.Edges) {
        UpdateStyle(edge);
      }

      await graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Enables/disables the buttons in the toolbar and the input mode. This is used for managing the toolbar during
    ///   layout calculation.
    /// </summary>
    private void SetUIDisabled(bool disabled) {
      lockSelectionButton.IsEnabled = !disabled;
      unlockSelectionButton.IsEnabled = !disabled;
      refreshButton.IsEnabled = !disabled;
      layoutButton.IsEnabled = !disabled;
    }

    [STAThread]
    private static void Main() => Run<HierarchicalExactCoordinatesDemo>();
  }
}
