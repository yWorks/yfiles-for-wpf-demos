/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;
using yWorks.Utils;
using yWorks.Layout.Hierarchic;


namespace Tutorial.GettingStarted
{

  /// <summary>
  /// Getting Started - 14 Automatic Graph Layout
  /// This demo shows how to use the layout algorithms in yFiles WPF to automatically
  /// place the graph elements.
  /// </summary>
  /// <remarks>To use the layout algorithms you have to reference the following
  /// assemblies in addition to <c>yWorks.yFilesWPF.Viewer</c>:
  /// <list type="bullet">
  /// <item><c>yWorks.yFilesWPF.Algorithms</c>: This assembly contains the actual layout
  /// and analysis algorithms.</item>
  /// <item><c>yWorks.yFilesWPF.Adapter</c>: This class is necessary to use the layout and
  /// analysis algorithms with the object model in the
  /// yFiles WPF viewer part.</item>
  /// </list>
  /// Note that this sample loads a sample graph from a file instead of creating it
  /// programmatically, since the graphs from the previous examples
  /// are not really suited for automatic layout.
  /// </remarks>
  public partial class SampleApplication
  {

    public void OnLoaded(object source, EventArgs args) {

      ConfigureGroupNodeStyles();

      // Customizes the port handling
      // we leave this one disabled for this demo
      // since this does not make sense for the kind of
      // automatic layout that has been implemented in this demo
      //CustomizePortHandling();

      // Configure interaction
      ConfigureInteraction();

      // Sets up the data binding after folding has been enabled.
      // Order is important here, since we want to subscribe to events on the master graph
      EnableDataBinding();

      // From now on, everything can be done on the actual managed view instance
      EnableFolding();

      // Enables GraphML IO
      EnableGraphMLIO();

      // Displays tooltips for the stored data items, so that something is visible to the user
      SetupTooltips();

      // Add a context menu to nodes
      SetupContextMenu();

      // Makes the data in the mapper persistent.
      EnableDataPersistence();

      // Configures default label model parameters for newly created graph elements
      SetDefaultLabelParameters();

      // Configures default styles for newly created graph elements
      SetDefaultStyles();

      ///////////////// New in this Sample /////////////////

      // Reads the initial graph from a resource file instead of creating it programmatically
      ReadSampleGraph();

      //////////////////////////////////////////////////////

      // Enables the undo engine (disabled by default)
      EnableUndo();

      // Manages the viewport
      UpdateViewport();
    }

      ///////////////// New in this Sample /////////////////

    private async void LayoutButton_Click(object sender, EventArgs e)
    {
      // Uses the convenience class LayoutExtensions to perform the layout in a background thread,
      // animate it, manage undo and adjust the content rectangle in one simple step.
      await graphControl.MorphLayout(new HierarchicLayout(), TimeSpan.FromSeconds(2), null);

      // Alternatively we can do this more explicitly like so, without animation and without background threads
      // that supports undo by the way
//      using (ICompoundEdit edit = manager.MasterGraph.BeginEdit("Do Layout", "Undo Layout")) {
//        // Applies a layout in a single method call using an extension method
//        new HierarchicLayout().DoLayout(Graph);
//
//        // Refits the graph bounds, which we have probably changed with our layout invocation.
//        graphControl.FitGraphBounds();
//      }
    }

    /// <summary>
    /// Reads a sample graph from an embedded resource, using <see cref="GraphMLIOHandler"/>
    /// </summary>
    private void ReadSampleGraph() {
      graphControl.ImportFromGraphML("layout-sample.graphml");
    }

    #region Persist bound data to GraphML

    /// <summary>
    /// Register input and output handlers that store the data in the mapper as GraphMLAttributes resp. can read them back.
    /// </summary>
    private void EnableDataPersistence() {
      // We get the IO handler that is used by the GraphControl for serialization and deserialization.
      var ioh = graphControl.GraphMLIOHandler;

      IMapperRegistry registry = manager.MasterGraph.MapperRegistry;
      IMapper<INode, DateTime> dateMapper = registry.GetMapper<INode, DateTime>(DateTimeMapperKey);
      if (dateMapper != null) {
        // The OutputHandler just stores the string value of the attribute
        // We need to provide the symbolic name of the attribute in the GraphML file, the data source as an IMapper and the
        // GraphML type of the attribute
        ioh.AddOutputMapper(DateTimeMapperKey, null, dateMapper, delegate(object args, HandleSerializationEventArgs e) {
          if (e.Item is DateTime) {
            e.Writer.WriteString(((DateTime)e.Item).ToString(CultureInfo.InvariantCulture));
          }
          e.Handled = true;
        }, KeyType.String);

        // To read back a DateTime value from a string GraphML attribute, we have to provide an additional (very simple...) callback method.
        ioh.AddInputMapper((elem) => GraphMLIOHandler.MatchesName(elem, DateTimeMapperKey) && GraphMLIOHandler.MatchesType(elem, KeyType.String), dateMapper,
                           delegate(object args, HandleDeserializationEventArgs e) {
                             //The actual value is a text node that can be retrieved from the event
                             try {
                               DateTime dateTime = DateTime.Parse(e.XmlNode.ToString(), CultureInfo.InvariantCulture);
                               e.Result = dateTime;
                             } catch (Exception exception) {
                               Console.WriteLine(exception);
                               e.Result = DateTime.Now;
                             }
                           });
      }
    }

    #endregion

    #region Bind data to graph elements

    /// <summary>
    /// Symbolic name for the mapper that allows transparent access to the correct implementation even across
    /// wrapped graphs.
    /// </summary>
    private const string DateTimeMapperKey = "DateTimeMapperKey";

    /// <summary>
    /// Sets up simple data binding - creates an IMapper, registers it and subscribe to the node creation event
    /// on the master graph.
    /// </summary>
    private void EnableDataBinding() {
      // Creates a specialized IMapper instance, and registers it under a symbolic name.
      // Uses WeakDictionaryMapper so that in case a node is removed from the graph,
      // we don't have to care about the reference through the IMapper.

      IMapper<INode, DateTime> dateMapper = Graph.MapperRegistry.CreateWeakMapper<INode, DateTime>(DateTimeMapperKey);
      
      //Subscribes to the node creation event to record the node creation time.
      //Note that since this event is triggered after undo/redo, the time will
      //be updated when re-doing node creations and undoing node deletions.
      //If this is unwanted behavior, you can customize the node creation itself
      //to associate this data with the element at the time of its initial creation,
      //e.g. by listening to the NodeCreated event of GraphEditorInputMode, see below
      Graph.NodeCreated += delegate(object source, ItemEventArgs<INode> eventArgs) {
        // Stores the current time as node creation time.
        dateMapper[eventArgs.Item] = DateTime.Now;
      };

      // Alternatively (or in addition) we could use the event for
      // interactive node creation as follows, provided that the input mode
      // for the graph control is already set.
      //                      ((GraphEditorInputMode)graphControl.InputMode).NodeCreated +=
      //                        delegate(object sender, ItemEventArgs<INode> e) {
      //                          IFoldingView foldingView = Graph.GetFoldingView();
      //                          if (foldingView != null) {
      //                            // Store the data at the master graph as in the original approach.
      //                            INode master = foldingView.GetMaster(e.Item);
      //                            dateMapper[master] = DateTime.Now;
      //                          }
      //                        };
    }

    #endregion

    #region Configure dynamic tooltips

    /// <summary>
    /// Setup tooltips that return the value that is stored in the mapper.
    /// </summary>
    /// <remarks>
    /// Dynamic tooltips are implemented by adding a tooltip provider as an event handler for the
    /// <see cref="GraphEditorInputMode.QueryItemToolTip" /> event of the GraphEditorInputMode using the
    /// <see cref="QueryItemToolTipEventArgs{T}" /> parameter. This parameter provides three relevant
    /// properties:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The <see cref="ToolTipQueryEventArgs.Handled" /> property is a flag which indicates
    /// whether the tooltip was already set by one of possibly several tooltip providers.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <see cref="QueryItemToolTipEventArgs{T}.Item" /> property contains the <see cref="IModelItem" />
    /// the mouse hovers over.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The tooltip is set by setting the <see cref="ToolTipQueryEventArgs.ToolTip" />
    /// property.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    private void SetupTooltips() {
      GraphEditorInputMode geim = graphControl.InputMode as GraphEditorInputMode;
      if (geim != null) {
        geim.ToolTipItems = GraphItemTypes.Node;
        geim.QueryItemToolTip +=
          delegate(object src, QueryItemToolTipEventArgs<IModelItem> eventArgs) {
            if (eventArgs.Handled) {
              // A tooltip has already been assigned -> nothing to do.
              return;
            }
            INode hitNode = eventArgs.Item as INode;
            if (hitNode == null) {
              return;
            }
            // Since the node the user interacts with is an instance on the managed view,
            // rather than on the master graph to which we've bound our data,
            // we retrieve the mapper indirectly through its symbolic name.
            // The folding framework automagically returns an IMapper instance that translates
            // to the original elements. If we were not using folding, this step would be unnecessary
            // and we could use the mapper instance directly on the original nodes.
            IMapperRegistry registry = Graph.MapperRegistry;
            IMapper<INode, DateTime> dateMapper =
              registry.GetMapper<INode, DateTime>(DateTimeMapperKey);
            if (dateMapper != null) {
              //Found a suitable mapper.
              //Finds out if a node is under the current location.
              // Set the tooltip.
              eventArgs.ToolTip = dateMapper[hitNode].ToString();

              // Indicate that the tooltip has been set.
              eventArgs.Handled = true;
            }
          };

        // Add a little offset to the tooltip such that it is not obscured by the mouse pointer.
        geim.MouseHoverInputMode.ToolTipLocationOffset = new PointD(20, 20);
      }
    }

    /// <summary>
    /// Adds a context menu for nodes
    /// </summary>
    private void SetupContextMenu() {
      GraphEditorInputMode mode = graphControl.InputMode as GraphEditorInputMode;
      if (mode != null) {
        mode.ContextMenuItems = GraphItemTypes.Node;
        mode.PopulateItemContextMenu += (sender, e) =>
        {
          var node = e.Item as INode;
          if (node != null) {
            // add a context menu entry
            var menuItem = new MenuItem(){Header = "Set to now"};
            menuItem.Click += (o, args) => SetToNow(node);
            e.Menu.Items.Add(menuItem);
          }
        }; 
      }
    }

    /// <summary>
    /// Sets the mapped DateTime object of a node to the current value.
    /// </summary>
    /// <param name="node">The node to map the DateTime instance to.</param>
    private void SetToNow(INode node) {
      IMapper<INode, DateTime> mapper = Graph.MapperRegistry.GetMapper<INode, DateTime>(DateTimeMapperKey);
      if (mapper != null) {
        mapper[node] = DateTime.Now;
      }
    }

    #endregion

    #region Configure Folding

    private FoldingManager manager;

    /// <summary>
    /// Enables folding - changes the GraphControl's graph to a managed view
    /// that provides the actual collapse/expand state.
    /// </summary>
    private void EnableFolding() {
      // Creates the folding manager and sets its master graph to
      // the single graph that has served for all purposes up to this point
      manager = new FoldingManager(Graph);
      // Creates a managed view from the master graph and 
      // replaces the existing graph view with a managed view
      graphControl.Graph = manager.CreateFoldingView().Graph;
      // Change the default style for group nodes. We use <see cref="CollapsibleNodeStyleDecorator"/> 
      // to wrap the <see cref="PanelNodeStyle"/> from the last demo, since we want to have nice
      // +/- buttons for collapse/expand. Note that if you haven't defined
      // a custom group node style, you don't have to do anything at all, since
      // <see cref="FoldingManager"/> already
      // provides such a decorated group node style by default.
      Graph.GroupNodeDefaults.Style = new CollapsibleNodeStyleDecorator(Graph.GroupNodeDefaults.Style);
    }


    #endregion

    #region Customized Undo for Folding

    /// <summary>
    /// Enables the Undo functionality.
    /// </summary>
    private void EnableUndo() {
      // Enables undo on the folding manager's master graph instead of on the managed view.
      manager.MasterGraph.SetUndoEngineEnabled(true);
    }

    #endregion

    #region Configure grouping

    /// <summary>
    /// Configures the default style for group nodes.
    /// </summary>
    private void ConfigureGroupNodeStyles() {
      // PanelNodeStyle is a style especially suited to group nodes
      // Creates a panel with a light blue background
      Color groupNodeColor = Color.FromArgb(255, 214, 229, 248);
      var groupNodeDefaults = Graph.GroupNodeDefaults;
      groupNodeDefaults.Style = new PanelNodeStyle {
        Color = groupNodeColor,
        // Specifies insets that provide space for a label at the top
        // For a solution how to determine these insets automatically, please
        // see the yEd WPF demo application.
        Insets = new InsetsD(5, 18, 5, 5),
        LabelInsetsColor = groupNodeColor,
      };

      // Sets a label style with right-aligned text
      groupNodeDefaults.Labels.Style = new DefaultLabelStyle { TextAlignment = TextAlignment.Right };

      // Places the label at the top inside of the panel.
      // For PanelNodeStyle, InteriorStretchLabelModel is usually the most appropriate label model
      groupNodeDefaults.Labels.LayoutParameter = InteriorStretchLabelModel.North;
    }

    #endregion

    #region Enable command bindings for GraphML I/O

    /// <summary>
    /// Enables GraphML I/O command bindings.
    /// </summary>
    private void EnableGraphMLIO() {
      // Usually, this would be done in XAML, we just show it here for convenience
      graphControl.FileOperationsEnabled = true;
    }

    #endregion

    #region InputMode creation and configuration

    /// <summary>
    /// Configure basic interaction.
    /// </summary>
    /// <remarks>Interaction is handled by so called InputModes. <see cref="GraphEditorInputMode"/> is the main
    /// InputMode that already provides a large number of graph interaction possibilities, such as moving, deleting, creating,
    /// resizing graph elements. Note that to create or edit a label, just press F2. Also, try to move a label around and see what happens
    /// </remarks>
    private void ConfigureInteraction() {
      // Creates a new GraphEditorInputMode instance and registers it as the main
      // input mode for the graphControl
      var inputMode = new GraphEditorInputMode();
      // enable grouping operations on the input mode
      inputMode.AllowGroupingOperations = true;
      graphControl.InputMode = inputMode;
    }

    #endregion

    #region Default label model parameters

    /// <summary>
    /// Set up default label model parameters for graph elements.
    /// </summary>
    /// <remarks>
    /// Label model parameters control the actual label placement as well as the available
    /// placement candidates when moving the label interactively.
    /// </remarks>
    private void SetDefaultLabelParameters() {
      #region Default node label model parameter

      // For node labels, the default is a label position at the node center
      // Let's keep the default.  Here is how to set it manually
      Graph.NodeDefaults.Labels.LayoutParameter = InteriorLabelModel.Center;

      #endregion

      #region Default edge label parameter

      // For edge labels, the default is a label that is rotated to match the associated edge segment
      // We'll start by creating a model that is similar to the default:
      EdgeSegmentLabelModel edgeSegmentLabelModel = new EdgeSegmentLabelModel();
      // However, by default, the rotated label is centered on the edge path.
      // Let's move the label off of the path:
      edgeSegmentLabelModel.Distance = 10;
      // Finally, we can set this label model as the default for edge labels using a location at the center of the first segment
      Graph.EdgeDefaults.Labels.LayoutParameter = edgeSegmentLabelModel.CreateParameterFromSource(0, 0.5, EdgeSides.RightOfEdge);

      #endregion
    }

    #endregion

    #region Default style setup

    /// <summary>
    /// Set up default styles for graph elements.
    /// </summary>
    /// <remarks>
    /// Default styles apply only to elements created after the default style has been set,
    /// so typically, you'd set these as early as possible in your application.
    /// </remarks>
    private void SetDefaultStyles() {
      
      #region Default Node Style
      // Sets the default style for nodes
      // Creates a nice ShinyPlateNodeStyle instance, using an orange Brush.
      INodeStyle defaultNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)) };

      // Sets this style as the default for all nodes that don't have another
      // style assigned explicitly
      Graph.NodeDefaults.Style = defaultNodeStyle;

      #endregion

      #region Default Edge Style
      // Sets the default style for edges:
      // Creates an edge style that will apply a gray pen with thickness 1
      // to the entire line using PolyLineEdgeStyle,
      // which draws a polyline determined by the edge's control points (bends)
      var defaultEdgeStyle = new PolylineEdgeStyle { Pen = Pens.Gray };

      // Sets the source and target arrows on the edge style instance
      // (Actually: no source arrow)
      // Note that IEdgeStyle itself does not have these properties
      // Also note that by default there are no arrows
      defaultEdgeStyle.TargetArrow = Arrows.Default;

      // Sets the defined edge style as the default for all edges that don't have
      // another style assigned explicitly:
      Graph.EdgeDefaults.Style = defaultEdgeStyle;
      #endregion

      #region Default Label Styles
      // Sets the default style for labels
      // Creates a label style with the label text color set to dark red
      ILabelStyle defaultLabelStyle = new DefaultLabelStyle { Typeface = new Typeface("Tahoma"), TextSize = 12, TextBrush = Brushes.DarkRed };

      // Sets the defined style as the default for both edge and node labels:
      Graph.EdgeDefaults.Labels.Style = Graph.NodeDefaults.Labels.Style = defaultLabelStyle;

      #endregion

      #region Default Node size
      // Sets the default size explicitly to 40x40
      Graph.NodeDefaults.Size = new SizeD(40, 40);

      #endregion

    }

    #endregion

    #region Viewport handling

    /// <summary>
    /// Updates the content rectangle to encompass all existing graph elements.
    /// </summary>
    /// <remarks>If you create your graph elements programmatically, the content rectangle 
    /// (i.e. the rectangle in <b>world coordinates</b>
    /// that encloses the graph) is <b>not</b> updated automatically to enclose these elements. 
    /// Typically, this manifests in wrong/missing scrollbars, incorrect <see cref="GraphOverviewControl"/> 
    /// behavior and the like.
    /// <para>
    /// This method demonstrates several ways to update the content rectangle, with or without adjusting the zoom level 
    /// to show the whole graph in the view.
    /// </para>
    /// <para>
    /// Note that updating the content rectangle only does not change the current Viewport (i.e. the world coordinate rectangle that
    /// corresponds to the currently visible area in view coordinates)
    /// </para>
    /// <para>
    /// Uncomment various combinations of lines in this method and observe the different effects.
    /// </para>
    /// <para>The following demos in this tutorial will assume that you've called <c>graphControl.FitGraphBounds();</c>
    /// in this method.</para>
    /// </remarks>
    private void UpdateViewport() {
      // Uncomment the following line to update the content rectangle 
      // to include all graph elements
      // This should result in correct scrolling behavior:

      //graphControl.UpdateContentRect();

      // Additionally, we can also set the zoom level so that the
      // content rectangle fits exactly into the viewport area:
      // Uncomment this line in addition to UpdateContentRect:
      // Note that this changes the zoom level (i.e. the graph elements will look smaller)

      //graphControl.FitContent();

      // The sequence above is equivalent to just calling:
      graphControl.FitGraphBounds();
    }

    #endregion

    #region Standard Event handlers

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion

    #region Convenience Properties

    public IGraph Graph {
      get { return graphControl.Graph; }
    }

    #endregion

    #region Constructor
    public SampleApplication() {
      InitializeComponent();
    }

    #endregion
  }
}
