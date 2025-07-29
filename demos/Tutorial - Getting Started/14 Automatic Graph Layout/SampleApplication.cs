/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace Tutorial.GettingStarted
{
  /// <summary>
  ///   Getting Started - 14 Automatic Graph Layout
  ///   This demo shows how to use the layout algorithms in yFiles WPF to automatically
  ///   place the graph elements.
  /// </summary>
  /// <remarks>
  ///   To use the layout algorithms you have to reference the following
  ///   assemblies in addition to <c>yFilesViewer</c>:
  ///   <list type="bullet">
  ///     <item>
  ///       <c>yFilesAlgorithms</c>: This assembly contains the actual layout
  ///       and analysis algorithms.
  ///     </item>
  ///     <item>
  ///       <c>yFilesAdapter</c>: This class is necessary to use the layout and
  ///       analysis algorithms with the object model in the
  ///       yFiles WPF viewer part.
  ///     </item>
  ///   </list>
  ///   Note that this sample loads a sample graph from a file instead of creating it
  ///   programmatically, since the graphs from the previous examples
  ///   are not really suited for automatic layout.
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

    private async void LayoutButton_Click(object sender, EventArgs e) {
      // Uses the convenience class LayoutExtensions to perform the layout in a background thread,
      // animate it, manage undo and adjust the content bounds in one simple step.
      await graphControl.ApplyLayoutAnimated(new HierarchicalLayout(), TimeSpan.FromSeconds(2));

      // Alternatively we can do this more explicitly like so, without animation and without background threads
      // that supports undo by the way
      //      using (ICompoundEdit edit = manager.MasterGraph.BeginEdit("Do Layout", "Undo Layout")) {
      //        // Applies a layout in a single method call using an extension method
      //        new HierarchicalLayout().DoLayout(Graph);
      //
      //        // Refits the graph bounds, which we have probably changed with our layout invocation.
      //        graphControl.FitGraphBounds();
      //      }
    }

    /// <summary>
    ///   Reads a sample graph from an embedded resource, using <see cref="GraphMLIOHandler"/>
    /// </summary>
    private void ReadSampleGraph() {
      graphControl.ImportFromGraphML("layout-sample.graphml");
    }

    //////////////////////////////////////////////////////

    #region Persist bound data to GraphML

    /// <summary>
    ///   Register input and output handlers that store the data in the mapper as <see cref="GraphMLAttribute"/>s resp.
    ///   can read them back.
    /// </summary>
    private void EnableDataPersistence() {
      // We get the IO handler that is used by the GraphControl for serialization and deserialization.
      var ioh = graphControl.GraphMLIOHandler;

      if (dateMapper != null) {
        // The OutputHandler just stores the string value of the attribute
        // We need to provide the symbolic name of the attribute in the GraphML file, the data source as an IMapper and the
        // GraphML type of the attribute
        ioh.AddOutputMapper("CreationTime", null, dateMapper,
            (context, item, _) => { context.Writer.WriteString(item.ToString(CultureInfo.InvariantCulture)); },
            KeyType.String);

        // To read back a DateTime value from a string GraphML attribute, we have to provide an additional (very simple...) callback method.
        ioh.AddInputMapper(
            elem => GraphMLIOHandler.MatchesName(elem, "CreationTime") &&
                GraphMLIOHandler.MatchesType(elem, KeyType.String), dateMapper, (_, node, _) => {
              // The actual value is a text node that can be retrieved from the event
              try {
                return DateTime.Parse(node.ToString(), CultureInfo.InvariantCulture);
              } catch (Exception exception) {
                Console.WriteLine(exception);
                return DateTime.Now;
              }
            });
      }
    }

    #endregion

    #region Bind data to graph elements

    /// <summary>
    ///   Mapper to store the creation time for each node.
    /// </summary>
    private IMapper<INode, DateTime> dateMapper;

    /// <summary>
    ///   Sets up simple data binding - creates an IMapper, registers it and subscribe to the node creation event
    ///   on the master graph.
    /// </summary>
    private void EnableDataBinding() {
      // Creates a specialized IMapper instance.
      // This uses weak references so that in case a node is removed from the graph,
      // we don't have to care about the reference through the IMapper.
      dateMapper = new WeakDictionaryMapper<INode, DateTime>();

      // Subscribes to the node creation event to record the node creation time.
      // Note that since this event is triggered after undo/redo, the time will
      // be updated when re-doing node creations and undoing node deletions.
      // If this is unwanted behavior, you can customize the node creation itself
      // to associate this data with the element at the time of its initial creation,
      // e.g. by listening to the NodeCreated event of GraphEditorInputMode, see below
      Graph.NodeCreated += (_, e) => {
        // Stores the current time as node creation time.
        dateMapper[e.Item] = DateTime.Now;
      };

      // Alternatively (or in addition) we could use the event for
      // interactive node creation as follows, provided that the input mode
      // for the graph control is already set.
      //((GraphEditorInputMode) graphControl.InputMode).NodeCreated +=
      //  (_, e) => {
      //    // Store the data at the master graph as in the original approach.
      //    INode master = Graph.GetFoldingView()?.GetMasterItem(e.Item) ?? e.Item;
      //    dateMapper[master] = DateTime.Now;
      //  };
    }

    #endregion

    #region Configure dynamic tooltips

    /// <summary>
    ///   Setup tooltips that return the value that is stored in the mapper.
    /// </summary>
    /// <remarks>
    ///   Dynamic tooltips are implemented by adding a tooltip provider as an event handler for the
    ///   <see cref="GraphEditorInputMode.QueryItemToolTip"/> event of the GraphEditorInputMode using the
    ///   <see cref="QueryItemToolTipEventArgs{TModelItem}"/> parameter. This parameter provides three relevant
    ///   properties:
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         The <see cref="QueryToolTipEventArgs.Handled"/> property is a flag which indicates
    ///         whether the tooltip was already set by one of possibly several tooltip providers.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         The <see cref="QueryItemToolTipEventArgs{TModelItem}.Item"/> property contains the <see cref="IModelItem"/>
    ///         the mouse hovers over.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         The tooltip is set by setting the <see cref="QueryToolTipEventArgs.ToolTip"/>
    ///         property.
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    private void SetupTooltips() {
      if (graphControl.InputMode is GraphInputMode gim) {
        gim.ToolTipItems = GraphItemTypes.Node;
        gim.QueryItemToolTip += (_, e) => {
          if (e.Handled) {
            // A tooltip has already been assigned -> nothing to do.
            return;
          }
          if (e.Item is not INode node) {
            return;
          }
          // Since the node the user interacts with is an instance on the managed view,
          // rather than on the master graph to which we've bound our data,
          // we have to fetch the information with the master node since that's what
          // we stored it at. If we were not using folding, this step would be unnecessary
          // and we could use the mapper instance directly on the original nodes.
          if (dateMapper != null) {
            // Set the tooltip.
            var master = Graph.GetFoldingView()?.GetMasterItem(node) ?? node;
            e.ToolTip = dateMapper[master].ToString();
          }
        };
      }
    }

    /// <summary>
    ///   Adds a context menu for nodes
    /// </summary>
    private void SetupContextMenu() {
      if (graphControl.InputMode is GraphInputMode gim) {
        gim.ContextMenuItems = GraphItemTypes.Node;
        gim.PopulateItemContextMenu += (sender, e) => {
          if (e.Item is INode node) {
            // add a context menu entry
            var menuItem = new MenuItem { Header = "Set to now" };
            menuItem.Click += (o, args) => SetToNow(node);
            e.Menu.Items.Add(menuItem);
            e.Handled = true;
          }
        };
      }
    }

    /// <summary>
    ///   Sets the mapped <see cref="DateTime"/> object of a node to the current value.
    /// </summary>
    /// <param name="node">The node to map the <see cref="DateTime"/> instance to.</param>
    private void SetToNow(INode node) {
      if (dateMapper != null) {
        var master = Graph.GetFoldingView()?.GetMasterItem(node) ?? node;
        dateMapper[master] = DateTime.Now;
      }
    }

    #endregion

    #region Configure Folding

    private FoldingManager manager;

    /// <summary>
    ///   Enables folding - changes the GraphControl's graph to a managed view
    ///   that provides the actual collapse/expand state.
    /// </summary>
    private void EnableFolding() {
      // Creates the folding manager and sets its master graph to
      // the single graph that has served for all purposes up to this point
      manager = new FoldingManager(Graph);
      // Creates a managed view from the master graph and
      // replaces the existing graph view with a managed view
      graphControl.Graph = manager.CreateFoldingView().Graph;
    }

    #endregion

    #region Customized Undo for Folding

    /// <summary>
    ///   Enables the Undo functionality.
    /// </summary>
    private void EnableUndo() {
      // Enables undo on the folding manager's master graph instead of on the managed view.
      manager.MasterGraph.SetUndoEngineEnabled(true);
    }

    #endregion

    #region Configure grouping

    /// <summary>
    ///   Configures the default style for group nodes.
    /// </summary>
    private void ConfigureGroupNodeStyles() {
      // GroupNodeStyle is a style especially suited to group nodes
      var groupNodeDefaults = Graph.GroupNodeDefaults;
      groupNodeDefaults.Style = new GroupNodeStyle {
          GroupIcon = GroupNodeStyleIconType.ChevronDown,
          FolderIcon = GroupNodeStyleIconType.ChevronUp,
          IconSize = 14,
          IconBackgroundShape = GroupNodeStyleIconBackgroundShape.Circle,
          IconForegroundBrush = Brushes.White,
          TabBrush = new SolidColorBrush(Color.FromRgb(0x24, 0x22, 0x65)),
          TabPosition = GroupNodeStyleTabPosition.TopTrailing,
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x24, 0x22, 0x65)), 2),
          CornerRadius = 8,
          TabWidth = 70,
          ContentAreaPadding = new InsetsD(8),
          HitTransparentContentArea = true
      };

      // Sets a label style with right-aligned text
      groupNodeDefaults.Labels.Style =
          new LabelStyle { TextAlignment = TextAlignment.Right, TextBrush = Brushes.White };

      // Places the label inside the tab.
      groupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabParameter();
    }

    #endregion

    #region Enable command bindings for GraphML I/O

    /// <summary>
    ///   Enables GraphML I/O command bindings.
    /// </summary>
    private void EnableGraphMLIO() {
      // Usually, this would be done in XAML, we just show it here for convenience
      graphControl.FileOperationsEnabled = true;
    }

    #endregion

    #region InputMode creation and configuration

    /// <summary>
    ///   Configure basic interaction.
    /// </summary>
    /// <remarks>
    ///   Interaction is handled by so called InputModes. <see cref="GraphEditorInputMode"/> is the main
    ///   InputMode that already provides a large number of graph interaction possibilities, such as moving, deleting,
    ///   creating,
    ///   resizing graph elements. Note that to create or edit a label, just press F2. Also, try to move a label around and see
    ///   what happens
    /// </remarks>
    private void ConfigureInteraction() {
      // Creates a new GraphEditorInputMode instance and registers it as the main
      // input mode for the graphControl
      graphControl.InputMode = new GraphEditorInputMode();
    }

    #endregion

    #region Default label model parameters

    /// <summary>
    ///   Set up default label model parameters for graph elements.
    /// </summary>
    /// <remarks>
    ///   Label model parameters control the actual label placement as well as the available
    ///   placement candidates when moving the label interactively.
    /// </remarks>
    private void SetDefaultLabelParameters() {
      #region Default node label model parameter

      // For node labels, the default is a label position at the node center
      // Let's keep the default. Here is how to set it manually
      Graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;

      #endregion

      #region Default edge label parameter

      // For edge labels, the default is a label that is rotated to match the associated edge segment
      // We'll start by creating a model that is similar to the default:
      var edgeSegmentLabelModel = new EdgeSegmentLabelModel();
      // However, by default, the rotated label is centered on the edge path.
      // Let's move the label off of the path:
      edgeSegmentLabelModel.Distance = 10;
      // Finally, we can set this label model as the default for edge labels using a location at the center of the first segment
      Graph.EdgeDefaults.Labels.LayoutParameter =
          edgeSegmentLabelModel.CreateParameterFromSource(0, 0.5, EdgeSides.RightOfEdge);

      #endregion
    }

    #endregion

    #region Default style setup

    /// <summary>
    ///   Sets up default styles for graph elements.
    /// </summary>
    /// <remarks>
    ///   Default styles apply only to elements created after the default style has been set,
    ///   so typically, you'd set these as early as possible in your application.
    /// </remarks>
    private void SetDefaultStyles() {
      #region Default Node Style

      // Sets this style as the default for all nodes that don't have another
      // style assigned explicitly
      Graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.RoundRectangle,
          Brush = new SolidColorBrush(Color.FromRgb(255, 108, 0)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(102, 43, 0)), 1.5)
      };

      #endregion

      #region Default Edge Style

      // Sets the default style for edges:
      // Creates a PolylineEdgeStyle which will be used as default for all edges
      // that don't have another style assigned explicitly
      var defaultEdgeStyle = new PolylineEdgeStyle {
          Pen = (Pen) new Pen(new SolidColorBrush(Color.FromRgb(102, 43, 0)), 1.5).GetAsFrozen(),
          TargetArrow = new Arrow(ArrowType.Triangle,
              (Brush) new SolidColorBrush(Color.FromRgb(102, 43, 0)).GetAsFrozen())
      };

      // Sets the defined edge style as the default for all edges that don't have
      // another style assigned explicitly:
      Graph.EdgeDefaults.Style = defaultEdgeStyle;

      #endregion

      #region Default Label Styles

      // Sets the default style for labels
      // Creates a label style with the label text color set to dark red
      ILabelStyle defaultLabelStyle =
          new LabelStyle { Typeface = new Typeface("Tahoma"), TextSize = 12, TextBrush = Brushes.Black };

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
    ///   Updates the content bounds to encompass all existing graph elements.
    /// </summary>
    /// <remarks>
    ///   If you create your graph elements programmatically, the content bounds
    ///   (i.e. the rectangle in <b>world coordinates</b>
    ///   that encloses the graph) are <b>not</b> updated automatically to enclose these elements.
    ///   Typically, this manifests in wrong/missing scrollbars, incorrect <see cref="GraphOverviewControl"/>
    ///   behavior and the like.
    ///   <para>
    ///     This method demonstrates several ways to update the content bounds, with or without adjusting the zoom level
    ///     to show the whole graph in the view.
    ///   </para>
    ///   <para>
    ///     Note that updating the content bounds only does not change the current Viewport (i.e. the world coordinate
    ///     rectangle that corresponds to the currently visible area in view coordinates)
    ///   </para>
    ///   <para>
    ///     Uncomment various combinations of lines in this method and observe the different effects.
    ///   </para>
    ///   <para>
    ///     The following demos in this tutorial will assume that you've called <c>graphControl.FitGraphBounds();</c>
    ///     in this method.
    ///   </para>
    /// </remarks>
    private void UpdateViewport() {
      // Uncomment the following line to update the content bounds
      // to include all graph elements
      // This should result in correct scrolling behavior:

      //graphControl.UpdateContentBounds();

      // Additionally, we can also set the zoom level so that the
      // content bounds fit exactly into the viewport area:
      // Uncomment this line in addition to UpdateContentBounds:
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

    public IGraph Graph => graphControl.Graph;

    #endregion

    #region Constructor

    public SampleApplication() {
      InitializeComponent();
    }

    #endregion
  }
}
