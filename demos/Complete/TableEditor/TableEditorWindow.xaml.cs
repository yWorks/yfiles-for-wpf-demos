/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Graph.TableEditor.Style;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace Demo.yFiles.Graph.TableEditor
{
  /// <summary>
  /// Interaction logic for <c>TableNodeStyleWindow.xaml</c>.
  /// </summary>
  /// <remarks>This demo configures an instance of <see cref="TableEditorInputMode"/> that is used to interactively modify the tables, as well as several child modes of <see cref="GraphEditorInputMode"/> that
  /// handle context menus and tool tips. Additionally, it shows how to perform a hierarchic layout that automatically respects the table structure. Please see the demo description for further information.</remarks>
  public partial class TableEditorWindow
  {
    private TableEditorInputMode tableEditorInputMode;
    private GraphEditorInputMode graphEditorInputMode;

    /// <summary>
    /// The default style for normal group nodes
    /// </summary>
    private readonly ShapeNodeStyle defaultGroupNodeStyle = new ShapeNodeStyle()
                                                              {
                                                                Shape = ShapeNodeShape.RoundRectangle,
                                                                Brush = Brushes.Transparent,
                                                                Pen =
                                                                  new Pen(Brushes.Black, 1)
                                                                    {DashStyle = DashStyles.DashDot}
                                                              };

    /// <summary>
    /// The default style for normal nodes
    /// </summary>
    private readonly ShinyPlateNodeStyle defaultNormalNodeStyle = new ShinyPlateNodeStyle { Brush = Brushes.Orange, Radius = 0};

    /// <summary>
    /// The default size for normal nodes
    /// </summary>
    private readonly SizeD defaultNodeSize = new SizeD(80, 50);
    
    public TableEditorWindow() {
      InitializeComponent();
      PopulateDnDList();
      ConfigureInputModes();
    }

    /// <summary>
    /// Configure the main input mode.
    /// </summary>
    /// <remarks>Creates a <see cref="GraphEditorInputMode"/> instance.</remarks>
    private void ConfigureInputModes() {
      graphEditorInputMode = new TableGraphEditorInputMode
                               {
                                 // enable grouping
                                 AllowGroupingOperations = true,
                                 //We want orthogonal edge editing/creation
                                 OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext(),
                                 //Activate drag 'n' drop from the style palette
                                 NodeDropInputMode = new MyNodeDropInputMode
                                                       {
                                                         ShowPreview = true,
                                                         Enabled = true,
                                                         //We identify the group nodes during a drag by either a custom tag or if they have a table associated.
                                                         IsGroupNodePredicate =
                                                           draggedNode =>
                                                           draggedNode.Lookup<ITable>() != null ||
                                                           (string) draggedNode.Tag == "GroupNode"
                                                       },
                                 //But disable node creation on click
                                 AllowCreateNode = false
                               };
      //Register custom reparent handler that prevents reparenting of table nodes (i.e. they may only appear on root level)
      graphEditorInputMode.ReparentNodeHandler = new MyReparentHandler(graphEditorInputMode.ReparentNodeHandler);
      ConfigureTableEditing();

      graphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    /// Configures table editing specific parts.
    /// </summary>
    private void ConfigureTableEditing() {
      //Create a new TEIM instance which also allows drag and drop
      tableEditorInputMode = new TableEditorInputMode
                               {
                                 //Enable drag & drop
                                 StripeDropInputMode = {Enabled = true},
                                 //Maximal level for both reparent and drag and drop is 2
                                 ReparentStripeHandler =
                                   new ReparentStripeHandler {MaxColumnLevel = 2, MaxRowLevel = 2},
                                 //Set the priority higher than for the handle input mode so that handles win if both gestures are possible
                                 Priority = graphEditorInputMode.HandleInputMode.Priority + 1
                               };
      //Add to GEIM
      graphEditorInputMode.Add(tableEditorInputMode);

      //Tooltip and context menu stuff for tables
      graphEditorInputMode.ContextMenuItems = GraphItemTypes.Node;
      graphEditorInputMode.PopulateItemContextMenu += graphEditorInputMode_PopulateItemContextMenu;
      graphEditorInputMode.PopulateItemContextMenu += graphEditorInputMode_PopulateNodeContextMenu;
      graphEditorInputMode.MouseHoverInputMode.QueryToolTip += MouseHoverInputMode_QueryToolTip;
    }

    /// <summary>
    /// Event handler for tool tips over a stripe header
    /// </summary>
    /// <remarks>We show only tool tips for stripe headers in this demo.</remarks>
    private void MouseHoverInputMode_QueryToolTip(object sender, ToolTipQueryEventArgs e) {
      if (!e.Handled) {
        StripeSubregion stripe = GetStripe(e.QueryLocation);
        if (stripe != null) {
          e.ToolTip = stripe.Stripe.ToString();
          e.Handled = true;
        }
      }
    }

    /// <summary>
    /// Event handler for the context menu for stripe header
    /// </summary>
    /// <remarks>We show only a simple context menu that demonstrates the <see cref="TableEditorInputMode.InsertChild"/> convenience method.</remarks>
    private void graphEditorInputMode_PopulateItemContextMenu(object sender,
                                                              PopulateItemContextMenuEventArgs<IModelItem> e) {
      if (!e.Handled) {
        StripeSubregion stripe = GetStripe(e.QueryLocation);
        if (stripe != null) {
          var deleteItem = new MenuItem { Header = "Delete " + stripe.Stripe, Command = ApplicationCommands.Delete, CommandParameter = stripe.Stripe};
          e.Menu.Items.Add(deleteItem);
          var insertBeforeItem = new MenuItem {Header = "Insert new stripe before " + stripe.Stripe};
          insertBeforeItem.Click += delegate {
                             IStripe parent = stripe.Stripe.GetParentStripe();
                             int index = stripe.Stripe.GetIndex();
                             tableEditorInputMode.InsertChild(parent, index);
                           };
          e.Menu.Items.Add(insertBeforeItem);
          var insertAfterItem = new MenuItem {Header = "Insert new stripe after " + stripe.Stripe};
          insertAfterItem.Click += delegate {
                              IStripe parent = stripe.Stripe.GetParentStripe();
                              int index = stripe.Stripe.GetIndex();
                              tableEditorInputMode.InsertChild(parent, index + 1);
                            };
          e.Menu.Items.Add(insertAfterItem);
          e.Handled = true;
        }
      }
    }

    /// <summary>
    /// Event handler for the context menu other hits on a node.
    /// </summary>
    /// <remarks>We show only a dummy context menu to demonstrate the basic principle.</remarks>
    private void graphEditorInputMode_PopulateNodeContextMenu(object sender,
                                                              PopulateItemContextMenuEventArgs<IModelItem> e) {
      if (!e.Handled) {
        IModelItem tableNode =
          graphEditorInputMode.FindItems(e.Context, e.QueryLocation, new[] {GraphItemTypes.Node}, item => item.Lookup<ITable>() != null).FirstOrDefault();
        if (tableNode != null) {
          var cutItem = new MenuItem
                          {
                            Header = "ContextMenu for " + tableNode
                          };
          e.Menu.Items.Add(cutItem);
          e.Handled = true;
        }
      }
    }

    /// <summary>
    /// Helper method that uses <see cref="TableEditorInputMode.FindStripe(PointD,StripeTypes,StripeSubregionTypes)"/>
    /// to retrieve a stripe at a certain location.
    /// </summary>
    private StripeSubregion GetStripe(PointD location) {
      return tableEditorInputMode.FindStripe(location, StripeTypes.All, StripeSubregionTypes.Header);
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeGraph"/>
    protected void OnLoaded(object src, RoutedEventArgs e) {
      // initialize the graph
      InitializeGraph();
    }


    private void InitializeGraph() {
      Graph.NodeDefaults.Style = defaultNormalNodeStyle;
      Graph.NodeDefaults.Size = defaultNodeSize;
      var groupNodeDefaults = Graph.GroupNodeDefaults;
      groupNodeDefaults.Style = defaultGroupNodeStyle;
      groupNodeDefaults.Size = defaultNodeSize;

      //We load a sample graph
      graphControl.ImportFromGraphML("Resources\\sample.graphml");
      graphControl.FitGraphBounds();

      //Configure Undo...
      //Enable general undo support
      graphControl.Graph.SetUndoEngineEnabled(true);
      //Use the undo support from the graph also for all future table instances
      Table.InstallStaticUndoSupport(Graph);

      // provide no candidates for edge creation at pool nodes - this effectively disables
      // edge creations for those nodes
      Graph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetImplementation(
        node => node.Lookup<ITable>() != null, PortCandidateProviders.NoCandidates);

      // customize marquee selection handling for pool nodes
      Graph.GetDecorator().NodeDecorator.MarqueeTestableDecorator.SetFactory(
        node => node.Lookup<ITable>() != null, node => new PoolNodeMarqueeTestable(node.Layout));
    }

    private class PoolNodeMarqueeTestable : IMarqueeTestable {
      private readonly IRectangle rectangle;

      public PoolNodeMarqueeTestable(IRectangle rectangle) {
        this.rectangle = rectangle;
      }

      public bool IsInBox(IInputModeContext context, RectD rectangle) {
        return rectangle.Contains(rectangle.GetTopLeft()) && rectangle.Contains(rectangle.GetBottomRight());
      }
    }


    public IGraph Graph {
      get { return graphControl.Graph; }
    }

    /// <summary>
    /// Perform a hierarchic layout that also configures the tables
    /// </summary>
    /// <remarks>Table support is automatically enabled in <see cref="LayoutExecutor"/>. The layout will:
    /// <list type="bullet">
    /// <item>
    /// Arrange all leaf nodes in a hierarchic layout inside their respective table cells
    /// </item>
    /// <item>Resize all table cells to encompass their child nodes. Optionally,
    /// <see cref="TableLayoutConfigurator.Compaction"/> allows to shrink table cells, other wise, table cells
    /// can only grow.</item>
    /// </list>
    /// </remarks>
    private void LayoutButton_Click(object sender, RoutedEventArgs e) {
        var hl = new HierarchicLayout()
                    {
                      ComponentLayoutEnabled = false,
                      LayoutOrientation = LayoutOrientation.LeftToRight,
                      OrthogonalRouting = true,
                      RecursiveGroupLayering = false
                    };
        ((SimplexNodePlacer) hl.NodePlacer).BarycenterMode = true;

        //We use Layout executor convenience method that already sets up the whole layout pipeline correctly
        var layoutExecutor = new LayoutExecutor(graphControl, hl)
                               {
                                 //Table layout is enabled by default already...
                                 ConfigureTableLayout = true,
                                 Duration = TimeSpan.FromMilliseconds(500),
                                 AnimateViewport = true,
                                 UpdateContentRect = true,
                                 RunInThread = true,
                                 //Table cells may only grow by an automatic layout.
                                 TableLayoutConfigurator = {Compaction = false}
                               };
        layoutExecutor.Start();
    }

    /// <summary>
    /// Exit the demo
    /// </summary>
    private void ExitMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #region drag and drop

    /// <summary>
    /// Creates rows, columns and tables programmatically to populate the list view where the items
    /// are dragged from.
    /// </summary>
    private void PopulateDnDList() {
      //Dummy table that serves to hold only a sample row
      ITable rowSampleTable = new Table();
      //Dummy table that serves to hold only a sample column
      ITable columnSampleTable = new Table();

      //Configure the defaults for the row sample table
      //We use a stripe control style and pass the style specific instance b a custom messenger object (e.g. StripeDescriptor)
      rowSampleTable.RowDefaults.Style = new StripeControlStripeStyle("RowStyle")
      {
        StyleTag = new StripeDescriptor
        {
          BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 171, 200, 226)),
          InsetBrush = new SolidColorBrush(Color.FromArgb(255, 240, 248, 255))
        }
      };

      //Create the sample row
      var rowSampleRow = rowSampleTable.CreateRow();
      //Create an invisible sample column in this table so that we will see something.
      var rowSampleColumn = rowSampleTable.CreateColumn(200d);
      rowSampleTable.SetStyle(rowSampleColumn, VoidStripeStyle.Instance);
      //The sample row uses empty insets
      rowSampleTable.SetStripeInsets(rowSampleColumn, new InsetsD());
      rowSampleTable.AddLabel(rowSampleRow, "Row");


      var columnSampleRow = columnSampleTable.CreateRow(200);
      columnSampleTable.SetStyle(columnSampleRow, VoidStripeStyle.Instance);
      var columnSampleColumn = columnSampleTable.CreateColumn(200d);
      columnSampleTable.SetStyle(columnSampleColumn, new StripeControlStripeStyle("ColumnStyle")
      {
        StyleTag = new StripeDescriptor
        {
          BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 171, 200, 226)),
          InsetBrush = new SolidColorBrush(Color.FromArgb(255, 240, 248, 255))
        }
      });
      columnSampleTable.SetStripeInsets(columnSampleRow, new InsetsD());
      columnSampleTable.AddLabel(columnSampleColumn, "Column");

      //Table for a complete sample table node
      Table sampleTable = new Table() {Insets = new InsetsD(0, 30, 0, 0)};
      //Configure the defaults for the row sample table
      sampleTable.ColumnDefaults.MinimumSize = sampleTable.RowDefaults.MinimumSize = 50;

      //Setup defaults for the complete sample table
      //We use a custom style that alternates the stripe colors and uses a special style for all parent stripes.
      sampleTable.RowDefaults.Style = new AlternatingLeafStripeStyle()
                                        {
                                          EvenLeafDescriptor = new StripeDescriptor()
                                                                 {
                                                                   BackgroundBrush =
                                                                     new SolidColorBrush(
                                                                     Color.FromArgb(255, 196, 215, 237)),
                                                                   InsetBrush =
                                                                     new SolidColorBrush(
                                                                     Color.FromArgb(255, 196, 215, 237))
                                                                 },
                                          OddLeafDescriptor = new StripeDescriptor()
                                                                {
                                                                  BackgroundBrush =
                                                                    new SolidColorBrush(
                                                                    Color.FromArgb(255, 171, 200, 226)),
                                                                  InsetBrush =
                                                                    new SolidColorBrush(
                                                                    Color.FromArgb(255, 171, 200, 226))
                                                                },
                                          ParentDescriptor = new StripeDescriptor()
                                                               {
                                                                 BackgroundBrush =
                                                                   new SolidColorBrush(Color.FromArgb(255, 113, 146, 178)),
                                                                 InsetBrush =
                                                                   new SolidColorBrush(Color.FromArgb(255, 113, 146, 178))
                                                               }
                                        };

      //The style for the columns is simpler, we use a stripe control style that only points the header insets.
      sampleTable.ColumnDefaults.Style = columnSampleTable.ColumnDefaults.Style = new StripeControlStripeStyle("ColumnStyle")
      {
        StyleTag =
          new StripeDescriptor
          {
            BackgroundBrush = Brushes.Transparent,
            InsetBrush = new SolidColorBrush(Color.FromArgb(255, 113, 146, 178))
          }
      };
      //Create a row and a column in the sample table
      sampleTable.CreateGrid(1, 1);
      //Use twice the default width for this sample column (looks nicer in the preview...)
      sampleTable.SetSize(sampleTable.Columns.First(), sampleTable.Columns.First().GetActualSize()*2);
      //Bind the table to a dummy node which is used for drag & drop
      //Binding the table is performed through a TableNodeStyle instance.
      //Among other things, this also makes the table instance available in the node's lookup (use INode.Lookup<ITable>()...)
      SimpleNode tableNode = new SimpleNode
                               {
                                 Style =
                                   new yWorks.Graph.Styles.TableNodeStyle(sampleTable)
                                     {
                                       TableRenderingOrder = TableRenderingOrder.RowsFirst,
                                       BackgroundStyle =
                                         new ShapeNodeStyle
                                           {Brush = new SolidColorBrush(Color.FromArgb(255, 236, 245, 255))}
                                     },
                                 Layout = new MutableRectangle(sampleTable.Layout)
                               };

      //Add the sample node for the table
      styleListBox.Items.Add(tableNode);

      //Add sample rows and columns
      //We use dummy nodes to hold the associated stripe instances - this makes the style panel easier to use
      SimpleNode columnSampleNode = new SimpleNode
                                      {
                                        Style =
                                          new yWorks.Graph.Styles.TableNodeStyle(columnSampleTable),
                                        Layout = new MutableRectangle(columnSampleTable.Layout),
                                        Tag = columnSampleTable.RootColumn.ChildColumns.First()
                                      };

      styleListBox.Items.Add(columnSampleNode);

      //Add sample rows and columns
      //We use dummy nodes to hold the associated stripe instances - this makes the style panel easier to use
      SimpleNode rowSampleNode = new SimpleNode
                                   {
                                     Style =
                                       new yWorks.Graph.Styles.TableNodeStyle(rowSampleTable),
                                     Layout = new MutableRectangle(rowSampleTable.Layout),
                                     Tag = rowSampleTable.RootRow.ChildRows.First()
                                   };
      styleListBox.Items.Add(rowSampleNode);

      //Add normal sample leaf and group nodes
      SimpleNode normalNode = new SimpleNode
                                {
                                  Style = defaultNormalNodeStyle,
                                  Layout = new MutableRectangle(PointD.Origin, defaultNodeSize),
                                };
      styleListBox.Items.Add(normalNode);

      SimpleNode groupNode = new SimpleNode
                               {
                                 Style = defaultGroupNodeStyle,
                                 Layout = new MutableRectangle(PointD.Origin, defaultNodeSize),
                                 //We set a custom tag that identifies this node as group node.
                                 Tag = "GroupNode"
                               };
      styleListBox.Items.Add(groupNode);
    }

    #endregion

    /// <summary>
    /// Custom <see cref="NodeDropInputMode"/> that disallows creating a table node inside of a group node (especially inside of another table node)
    /// </summary>
    private class MyNodeDropInputMode : NodeDropInputMode
    {
      protected override IModelItem GetDropTarget(PointD location) {
        //Ok, this node has a table associated - disallow dragging into a group node.
        var draggedNode = DraggedItem;
        if (draggedNode != null && draggedNode.Lookup<ITable>() != null) {
          return null;
        }
        return base.GetDropTarget(location);
      }
    }

    /// <summary>
    /// Custom <see cref="IReparentNodeHandler"/> that disallows reparenting a table node
    /// </summary>
    private class MyReparentHandler : IReparentNodeHandler
    {
      private readonly IReparentNodeHandler coreHandler;

      public MyReparentHandler(IReparentNodeHandler coreHandler) {
        this.coreHandler = coreHandler;
      }

      public bool IsReparentGesture(IInputModeContext context, INode node) {
        return coreHandler.IsReparentGesture(context, node);
      }

      public bool ShouldReparent(IInputModeContext context, INode node) {
        //Ok, this node has a table associated - disallow dragging into a group node.
        if (node.Lookup<ITable>() != null) {
          return false;
        }
        return coreHandler.ShouldReparent(context, node);
      }

      public bool IsValidParent(IInputModeContext context, INode node, INode newParent) {
        return coreHandler.IsValidParent(context, node, newParent);
      }

      public void Reparent(IInputModeContext context, INode node, INode newParent) {
        coreHandler.Reparent(context, node, newParent);
      }
    }

    public class TableGraphEditorInputMode : GraphEditorInputMode
    {
      /// <summary>
      /// Disallows the click selection of table nodes
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      protected override bool ShouldClickSelect(IModelItem item) {
        if (item.Lookup<ITable>() != null) {
          return false;
        } else {
          return base.ShouldClickSelect(item);
        }
      }
    }
  }
}
