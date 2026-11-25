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
using System.Windows;
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.TableEditor
{
  [Demo("table-editor", "application-features", "Table Editor",
      Summary = "Shows how to edit tables, rows, columns, and the graph elements they contain.",
      Keywords = ["table", "rows", "columns", "stripes", "pool", "drag-and-drop"], Tags = ["tables", "interaction"],
      RelatedDemos = ["table-editor", "drag-and-drop"])]
  public sealed partial class TableEditorDemo
  {
    private TableEditorInputMode tableInputMode = null!;
    private bool layoutRunning;

    public TableEditorDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Bootstraps this demo.
    /// </summary>
    protected override void Initialize() {
      // initialize the input mode
      var editorMode = CreateEditorMode();
      GraphControl.InputMode = editorMode;

      // use two finger panning to allow easier editing with touch gestures
      ConfigureTwoPointerPanning(editorMode);

      // initialize default styles
      DemoStyles.InitDemoStyles(Graph, orthogonalEdgeEditing: true);
      Graph.GroupNodeDefaults.Style = DragAndDropSupport.CreateGroupNodeStyle();

      // configures the drag and drop panel
      DragAndDropSupport.ConfigureDndPanel(dndPanel);

      // Enable general undo support
      Graph.SetUndoEngineEnabled(true);

      // configures the table editor input mode and context menu
      ConfigureTableEditing(editorMode);
      ConfigureContextMenu(editorMode);

      // enable loading and saving, and load a sample graph
      CreateGraph();
    }

    /// <summary>
    ///   Reads the default sample graph.
    /// </summary>
    private void CreateGraph() {
      var graphMLSupport = new GraphMLSupport(GraphControl);
      graphMLSupport.RegisterOpenSaveCommandBindings();

      // enable serialization of the table and demo styles - without a namespace mapping, serialization will fail
      TableStyles.RegisterGraphMLTypes(graphMLSupport.GraphMLIOHandler);

      using (var stream = LoadAssemblyResource("sample.graphml")) {
        graphMLSupport.GraphMLIOHandler.Read(Graph, stream);
      }
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates the editor input mode for this demo.
    /// </summary>
    private GraphEditorInputMode CreateEditorMode() =>
        new() {
            AllowCreateNode = false,
            ContextMenuItems = GraphItemTypes.Node,
            NodeDropInputMode = DragAndDropSupport.ConfigureDndInputMode(Graph)
        };

    private static void ConfigureTwoPointerPanning(GraphEditorInputMode editorMode) {
      // start marquee selection on long press to allow other gestures to start on a simple single press
      editorMode.MarqueeSelectionInputMode.BeginRecognizerTouch = EventRecognizers.TouchPrimaryLongPress;

      // set gestures to an immediate touch-down recognizer instead of the long-press recognizer
      editorMode.MoveSelectedItemsInputMode.BeginRecognizerTouch = EventRecognizers.TouchPrimaryDown;
      editorMode.CreateEdgeInputMode.BeginRecognizerTouch = EventRecognizers.TouchPrimaryDown;
      editorMode.CreateBendInputMode.BeginRecognizerTouch = EventRecognizers.TouchPrimaryDown;
      editorMode.HandleInputMode.BeginRecognizerTouch = EventRecognizers.TouchPrimaryDown;
      editorMode.MoveUnselectedItemsInputMode.BeginRecognizerTouch = EventRecognizers.TouchPrimaryDown;

      // make sure that starting the input modes above has higher priority than moving the viewport
      editorMode.MoveViewportInputMode.Priority = editorMode.MarqueeSelectionInputMode.Priority - 1;
      editorMode.MoveViewportInputMode.AllowSinglePointerMovement = false;
    }

    /// <summary>
    ///   Configures table editing specific parts.
    /// </summary>
    private void ConfigureTableEditing(GraphEditorInputMode editorMode) {
      // use the undo support from the graph also for all future table instances
      Table.InstallStaticUndoSupport(Graph);

      // provide no candidates for edge creation at pool nodes - this effectively disables
      // edge creations for those nodes
      Graph.GetDecorator().Nodes.PortCandidateProvider.AddConstant(node => node.Lookup(typeof(ITable)) is ITable,
          PortCandidateProviders.NoCandidates);

      // customize marquee selection handling for pool nodes
      Graph.GetDecorator().Nodes.MarqueeTestable.AddFactory(node => node.Lookup(typeof(ITable)) is ITable,
          // the marquee testable for pool nodes. The pool node should only be selected by marquee, if the entire bounds are
          // within the marquee.
          node => new TableNodeMarqueeTestable(node));

      var stripeReparentHandler = new ReparentStripeHandler {
          MaxColumnLevel = 2,
          MaxRowLevel = 2
      };

      // create a new TEIM instance which also allows drag and drop
      tableInputMode = new TableEditorInputMode {
          ReparentStripeHandler = stripeReparentHandler,
          Priority = editorMode.HandleInputMode.Priority + 1,
          StripeDropInputMode = { Enabled = true }
      };

      // add to GEIM
      editorMode.Add(tableInputMode);

      // tooltip for tables. We show only tool tips for stripe headers in this demo.
      editorMode.ToolTipItems = GraphItemTypes.Node;
      editorMode.QueryItemToolTip += (_, args) => {
        if (args.Handled) {
          return;
        }
        var stripeHit = tableInputMode.FindStripe(args.QueryLocation, StripeTypes.All, StripeSubregionTypes.Header);
        if (stripeHit != null) {
          args.ToolTip = stripeHit.Stripe.ToString();
          args.Handled = true;
        }
      };

      // register custom reparent handler that prevents reparenting of table nodes (i.e. they may only appear on root
      // level)
      editorMode.ReparentNodeHandler = new MyReparentHandler(editorMode.ReparentNodeHandler);

      // prevent re-parenting of tables into tables by copy & paste
      var clipboard = new GraphClipboard {
          ParentNodeDetection = GraphClipboard.ParentNodeDetectionModes.PreviousParent
      };
      GraphControl.Clipboard = clipboard;
    }

    /// <summary>
    ///   Initializes the context menu.
    /// </summary>
    private void ConfigureContextMenu(GraphEditorInputMode editorMode) {
      // add an event listener that populates the context menu according to the hit elements, or cancels showing a menu.
      editorMode.PopulateItemContextMenu += (_, args) => PopulateContextMenu(args);
    }

    /// <summary>
    ///   Event handler for the context menu.
    /// </summary>
    private void PopulateContextMenu(PopulateItemContextMenuEventArgs<IModelItem> args) {
      if (args.Handled) {
        return;
      }

      var stripeHit = tableInputMode.FindStripe(args.QueryLocation, StripeTypes.All, StripeSubregionTypes.Header);
      if (stripeHit != null) {
        var stripe = stripeHit.Stripe;
        var menu = args.Menu;
        menu.Items.Clear();
        menu.Items.Add(CreateMenuItem($"Delete {stripe}", (_, _) => tableInputMode.DeleteStripe(stripe)));

        menu.Items.Add(CreateMenuItem($"Insert new stripe before {stripe}",
            (_, _) => tableInputMode.InsertChild(stripe.GetParentStripe()!, stripe.GetIndex())));
        menu.Items.Add(CreateMenuItem($"Insert new stripe after {stripe}",
            (_, _) => tableInputMode.InsertChild(stripe.GetParentStripe()!, stripe.GetIndex() + 1)));
        args.ShowMenu = true;
        return;
      }

      if (args.Item is INode node && node.Lookup(typeof(ITable)) is ITable) {
        args.Menu.Items.Clear();
        args.Menu.Items.Add(CreateMenuItem($"ContextMenu for {node}", (_, _) => { }));
        args.ShowMenu = true;
      }
    }

    private static MenuItem CreateMenuItem(string header, RoutedEventHandler handler) {
      var item = new MenuItem { Header = header };
      item.Click += handler;
      return item;
    }

    /// <summary>
    ///   Perform a hierarchical layout that also configures the tables.
    ///   Table support is automatically enabled in <see cref="LayoutExecutor"/>. The layout will:
    ///   - Arrange all leaf nodes in a hierarchical layout inside their respective table cells
    ///   - Resize all table cells to encompass their child nodes. Optionally,
    ///   <see cref="TableLayoutConfigurator.Compaction"/> allows to shrink table cells, otherwise, table cells
    ///   can only grow.
    /// </summary>
    private async void OnLayoutClick(object sender, RoutedEventArgs e) {
      if (layoutRunning) {
        return;
      }

      layoutRunning = true;
      var layoutButton = sender as Button;
      layoutButton?.IsEnabled = false;
      try {
        var layout = new HierarchicalLayout {
            LayoutOrientation = LayoutOrientation.LeftToRight,
            GroupLayeringPolicy = GroupLayeringPolicy.IgnoreGroups,
            ComponentLayout = { Enabled = false }
        };
        // we use Layout executor convenience method that already sets up the whole layout pipeline correctly
        var executor = new LayoutExecutor(GraphControl, layout) {
            ConfigureTableLayout = true,
            AnimationDuration = TimeSpan.FromMilliseconds(500),
            AnimateViewport = true,
            TableLayoutConfigurator = { // table cells may only grow by an automatic layout.
                Compaction = false
            }
        };
        await executor.Start();
      } finally {
        layoutRunning = false;
        layoutButton?.IsEnabled = true;
      }
    }

    [STAThread]
    private static void Main() => Run<TableEditorDemo>();
  }
}
