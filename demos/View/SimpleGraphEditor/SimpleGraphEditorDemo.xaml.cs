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
using System.Windows.Controls;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SimpleGraphEditor
{
  /// <summary>
  ///   Demonstrates graph editing with folding, snapping, orthogonal edge editing, and an overview.
  /// </summary>
  [Demo("graph-editor", "view", "Graph Editor", Summary = "Shows the graph editing features of the graph component.",
      Tags = ["interaction"], Keywords = ["context menu", "groups", "folding", "overview", "Labels"])]
  public sealed partial class SimpleGraphEditorDemo
  {
    private FoldingManager foldingManager = null!;
    private IFoldingView foldingView = null!;
    private GraphEditorInputMode editorMode = null!;
    private GraphSnapContext snapContext = null!;
    private OrthogonalEdgeEditingContext orthogonalEdgeEditingContext = null!;
    private GraphMLSupport graphMLSupport = null!;
    private readonly GraphEditorSettings settings = new();

    public SimpleGraphEditorDemo() {
      InitializeComponent();
      DataContext = settings;
    }

    /// <summary>
    ///   Called just before showing the demo.
    ///   This method initializes the graph, the overview, and the input mode.
    /// </summary>
    protected override void Initialize() {
      EnableFolding();

      // Set up the default styles for the graph
      SetDefaultStyles(foldingManager.MasterGraph);

      // Specify a configured input mode that enables graph editing
      editorMode = CreateEditorMode();
      graphControl.InputMode = editorMode;

      // build the graph from the given data set
      BuildGraphFromJson("graph-data.json", graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout());
      graphControl.FitGraphBounds();

      // Enable the undo engine on the master graph
      foldingManager.MasterGraph.SetUndoEngineEnabled(true);
      // Also enable undo for folding operations
      foldingView.EnqueueNavigationalUndoUnits = true;

      InitializeSettings();

      graphMLSupport = new GraphMLSupport(graphControl);
      graphMLSupport.RegisterOpenSaveCommandBindings();
      graphControl.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewGraph, CanExecuteNewGraph));
    }

    /// <summary>
    ///   Enables folding by displaying a managed view of the master graph.
    /// </summary>
    private void EnableFolding() {
      foldingManager = new FoldingManager();
      foldingView = foldingManager.CreateFoldingView();
      graphControl.Graph = foldingView.Graph;
    }

    /// <summary>
    ///   Creates the editor input mode for this demo.
    /// </summary>
    private GraphEditorInputMode CreateEditorMode() {
      snapContext = new GraphSnapContext { Enabled = false };
      orthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext { Enabled = false };
      var mode = new GraphEditorInputMode {
          SnapContext = snapContext,
          OrthogonalEdgeEditingContext = orthogonalEdgeEditingContext
      };
      mode.EditLabelInputMode.TextEditorInputMode.TextBoxPlacementPolicy = TextBoxPlacementPolicy.MoveTextBox;
      // Fix the top right location of a group node when toggling collapse/expand
      mode.NavigationInputMode.AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.TopRight;

      // Make bend creation more important than moving of selected edges.
      // As a result, dragging a selected edge (not its bends) will create a new bend instead of moving all bends.
      // This is especially nicer in conjunction with orthogonal edge editing because this would create additional bends
      // every time the edge is moved otherwise
      mode.CreateBendInputMode.Priority = mode.MoveSelectedItemsInputMode.Priority - 1;

      // Add a context menu that is populated according to the hit elements.
      mode.ContextMenuInputMode.PopulateMenu += PopulateContextMenu;
      return mode;
    }

    /// <summary>
    ///   Sets default styles to the graph.
    /// </summary>
    private static void SetDefaultStyles(IGraph graph) {
      // Assign the default demo styles
      DemoStyles.InitDemoStyles(graph, foldingEnabled: true, orthogonalEdgeEditing: true);

      // Set the default node label position to centered below the node with the FreeNodeLabelModel that supports label
      // snapping
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(new PointD(0.5, 1.0),
          new PointD(0, 10), new PointD(0.5, 0.0), new PointD(0, 0));

      // Set the default edge label position with the SmartEdgeLabelModel that supports label snapping
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0);
    }

    private void InitializeSettings() {
      settings.PropertyChanged += (_, args) => {
        if (args.PropertyName == nameof(GraphEditorSettings.IsSnappingEnabled)) {
          snapContext.Enabled = settings.IsSnappingEnabled;
        } else if (args.PropertyName == nameof(GraphEditorSettings.IsOrthogonalEditingEnabled)) {
          orthogonalEdgeEditingContext.Enabled = settings.IsOrthogonalEditingEnabled;
        }
      };
    }

    private void NewGraph(object sender, ExecutedRoutedEventArgs args) {
      foldingManager.MasterGraph.Clear();
      foldingManager.MasterGraph.GetUndoEngine()?.Clear();
      graphControl.FitGraphBounds();
      args.Handled = true;
    }

    private static void CanExecuteNewGraph(object sender, CanExecuteRoutedEventArgs args) {
      args.CanExecute = true;
      args.Handled = true;
    }

    private void PopulateContextMenu(object? sender, PopulateMenuEventArgs args) {
      if (args.Handled) {
        return;
      }

      // In this demo, use custom hit testing to prefer nodes.
      var hits = graphControl.GraphModelManager.HitElementsAt(args.QueryLocation).ToList();

      // Check whether a node was hit. If it was, prefer it over edges.
      var hit = hits.OfType<INode>().FirstOrDefault() ?? hits.FirstOrDefault();
      var menuItems = new List<MenuItem>();
      var graphSelection = graphControl.Selection;
      if (hit is INode node) {
        // If a node or an edge is hit: provide 'Select All Nodes' or 'Select All Edges', respectively, and select the hit item.
        menuItems.Add(CreateMenuItem("Select All Nodes", SelectAllNodes));
        if (!graphSelection.Nodes.Contains(node)) {
          graphSelection.Clear();
        }
        graphSelection.Nodes.Add(node);
      } else if (hit is IEdge edge) {
        menuItems.Add(CreateMenuItem("Select All Edges", SelectAllEdges));
        if (!graphSelection.Edges.Contains(edge)) {
          graphSelection.Clear();
        }
        graphSelection.Edges.Add(edge);
      } else {
        // If another type of item or the empty canvas is hit: provide 'Select All'
        menuItems.Add(CreateMenuItem("Select All", SelectAll));
      }

      var inputMode = editorMode;
      // If one or more nodes are selected: add options to cut and copy.
      if (graphSelection.Nodes.Count > 0) {
        menuItems.Add(CreateMenuItem("Cut", inputMode.Cut));
        menuItems.Add(CreateMenuItem("Copy", inputMode.Copy));
      }
      if (!graphControl.Clipboard.IsEmpty) {
        // The clipboard is not empty: add an option to paste at the queried location.
        var queryLocation = args.QueryLocation;
        menuItems.Add(CreateMenuItem("Paste", () => inputMode.PasteAtLocation(queryLocation)));
      }

      args.Menu.Items.Clear();
      foreach (var menuItem in menuItems) {
        args.Menu.Items.Add(menuItem);
      }
      args.ShowMenu = menuItems.Count > 0;
      args.Handled = true;
    }

    private static MenuItem CreateMenuItem(string label, Action action) {
      var menuItem = new MenuItem { Header = label };
      menuItem.Click += (_, _) => action();
      return menuItem;
    }

    private void SelectAllNodes() {
      graphControl.Selection.Clear();
      foreach (var node in graphControl.Graph.Nodes) {
        graphControl.Selection.Nodes.Add(node);
      }
    }

    private void SelectAllEdges() {
      graphControl.Selection.Clear();
      foreach (var edge in graphControl.Graph.Edges) {
        graphControl.Selection.Edges.Add(edge);
      }
    }

    private void SelectAll() {
      foreach (var node in graphControl.Graph.Nodes) {
        graphControl.Selection.Nodes.Add(node);
      }
      foreach (var edge in graphControl.Graph.Edges) {
        graphControl.Selection.Edges.Add(edge);
      }
    }

    [STAThread]
    private static void Main() => Run<SimpleGraphEditorDemo>();
  }
}
