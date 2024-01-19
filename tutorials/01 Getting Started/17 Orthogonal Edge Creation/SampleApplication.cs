/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;


namespace Tutorial.GettingStarted
{
  /// <summary>
  /// Getting Started - 17 Orthogonal Edge Creation
  /// This demo shows how to enable orthogonal edge creation and editing.
  /// This support is provided through class <see cref="OrthogonalEdgeEditingContext"/> as well as
  /// properties in <see cref="CreateEdgeInputMode"/> and <see cref="GraphEditorInputMode"/>.
  /// </summary>
  public partial class SampleApplication
  {
      ///////////////// New in this Sample ////////////////

    private void orthogonalEdgesButton_Click(object sender, EventArgs e) {
      GraphEditorInputMode geim = graphControl.InputMode as GraphEditorInputMode;
      if (geim != null) {
        // when the button is clicked, we toggle the orthogonal edge mode
        var enableOrthogonalEdges = !geim.OrthogonalEdgeEditingContext.Enabled;
        geim.OrthogonalEdgeEditingContext.Enabled = enableOrthogonalEdges;
      }
    }

    private void EnableOrthogonalEdges()
    {
      GraphEditorInputMode geim = graphControl.InputMode as GraphEditorInputMode;
      if (geim != null) {
        // we create the context for interactively editing existing orthogonal edges
        // in an orthogonal fashion
        geim.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();
        
        var enableOrthogonalEdges = orthogonalEdgesButton.IsChecked ?? false;
        // we enable interactive creation and editing of orthogonal bend paths
        geim.OrthogonalEdgeEditingContext.Enabled = enableOrthogonalEdges;
      }
    }

      //////////////////////////////////////////////////////

    public void OnLoaded(object source, EventArgs args) {

      ConfigureGroupNodeStyles();

      CustomizePortHandling();

      // Configures and enables folding
      EnableFolding();

      // Enable GraphML IO
      EnableGraphMLIO();
      // Configure interaction
      ConfigureInteraction();

      InitializeSnapping();


      // Initializes the grid
      InitializeGrid();

      ///////////////// New in this Sample /////////////////

      // Enables orthogonal edge creation
      EnableOrthogonalEdges();

      //////////////////////////////////////////////////////

      // Configures default label model parameters for newly created graph elements
      SetDefaultLabelParameters();

      // Configures default styles for newly created graph elements
      SetDefaultStyles();

      // Populates the graph
      PopulateGraph();

      // Enables the undo engine (disabled by default)
      EnableUndo();

      // Manages the viewport
      UpdateViewport();
    }

    #region GridSnapping

    private bool gridVisible;
    private GridSnapTypes gridSnapType;
    private GridVisualCreator grid;
    private GridInfo gridInfo;

    public bool GridVisible {
      get { return gridVisible; }
      set {
        gridVisible = value;
        if (grid != null) { grid.Visible = gridVisible; }
        if (snapContext != null) { snapContext.GridSnapType = gridVisible ? GridSnapType : GridSnapTypes.None; }
        graphControl.Invalidate();
      }
    }

    public GridSnapTypes GridSnapType {
      get { return gridSnapType; }
      set {
        gridSnapType = value;
        if (GridVisible) { snapContext.GridSnapType = GridSnapType; }
      }
    }

    private void gridButton_Click(object sender, EventArgs e) {
      GridVisible = !GridVisible;
    }

    private void gridSnapTypeComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      GridSnapType = (GridSnapTypes)gridSnapTypeComboBox.SelectedItem;
    }

    private void InitializeGrid() {
      gridSnapTypeComboBox.Items.Add(GridSnapTypes.None);
      gridSnapTypeComboBox.Items.Add(GridSnapTypes.HorizontalLines);
      gridSnapTypeComboBox.Items.Add(GridSnapTypes.VerticalLines);
      gridSnapTypeComboBox.Items.Add(GridSnapTypes.Lines);
      gridSnapTypeComboBox.Items.Add(GridSnapTypes.GridPoints);
      gridSnapTypeComboBox.Items.Add(GridSnapTypes.All);
      gridSnapTypeComboBox.SelectedIndex = 4;

      // initialize grid
      gridInfo = new GridInfo { HorizontalSpacing = 50, VerticalSpacing = 50 };
      grid = new GridVisualCreator(gridInfo);
      graphControl.BackgroundGroup.AddChild(grid);

      snapContext.NodeGridConstraintProvider = new GridConstraintProvider<INode>(gridInfo);
      snapContext.BendGridConstraintProvider = new GridConstraintProvider<IBend>(gridInfo);

      GridVisible = gridButton.IsChecked ?? false;
      GridSnapType = (GridSnapTypes)gridSnapTypeComboBox.SelectedItem;
    }

    #endregion

    #region Snapping

    private bool snappingEnabled;
    private GraphSnapContext snapContext;

    public bool SnappingEnabled {
      get { return snappingEnabled; }
      set {
        snappingEnabled = value;
        if (snapContext != null) {
          snapContext.Enabled = snappingEnabled;
        }
      }
    }

    private void snappingButton_Click(object sender, EventArgs e) {
      SnappingEnabled = !SnappingEnabled;
    }

    private void InitializeSnapping() {
      // Initialize snapping state
      SnappingEnabled = snappingButton.IsChecked ?? false;

      // Initialize SnapContext
      GraphEditorInputMode geim = graphControl.InputMode as GraphEditorInputMode;
      if (geim != null) {
        snapContext = new GraphSnapContext
        {
          Enabled = SnappingEnabled,
        };
        geim.SnapContext = snapContext;
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
      // GroupNodeStyle is a style especially suited to group nodes
      var groupNodeDefaults = Graph.GroupNodeDefaults;
      groupNodeDefaults.Style = new GroupNodeStyle
      {
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
        ContentAreaInsets = new InsetsD(8),
        HitTransparentContentArea = true
      };

      // Sets a label style with right-aligned text
      groupNodeDefaults.Labels.Style = new DefaultLabelStyle
      {
        TextAlignment = TextAlignment.Right,
        TextBrush = Brushes.White
      };

      // Places the label inside of the tab.
      groupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateDefaultParameter();
    }

    /// <summary>
    /// Shows how to create group nodes programmatically.
    /// </summary>
    /// <remarks>Creates two nodes and puts them into a group node.</remarks>
    private INode CreateGroupNodes(params INode[] childNodes) {
      //Creates a group node that encloses the given child nodes
      INode groupNode = Graph.GroupNodes(childNodes);

      // Creates a label for the group node 
      Graph.AddLabel(groupNode, "Group 1");

      // Adjusts the layout of the group nodes
      Graph.AdjustGroupNodeLayout(groupNode);
      return groupNode;
    }

    #endregion

    #region Custom Lookup Chain Link for port candidates

    /// <summary>
    /// Configure custom port handling with the help of <see cref="ILookup"/>
    /// </summary>
    private void CustomizePortHandling() {
      // Sets auto cleanup to false, since we don't want to remove unoccupied ports.
      Graph.NodeDefaults.Ports.AutoCleanUp = false;

      // First we create a GraphDecorator from the IGraph.
      // GraphDecorator is a utility class that aids in decorating model items from a graph instance.

      // Here, we call NodeDecorator.PortCandidateProviderDecorator
      // to access the lookup decorator for ports - the thing we want to change.

      // One way to decorate the graph is to use the factory design pattern.
      // We set the factory to a lambda expression which
      // returns instances that implement the IPortCandidateProvider interface.

      // Here we can create a CompositePortCandidateProvider that combines various port candidate providers.
      // The ExistingPortsCandidateProvider provides port candidates at the locations of the already existing ports.
      // The NodeCenterPortCandidateProvider provides a single port candidate at the center of the node.
      // The ShapeGeometryPortCandidateProvider provides several port candidates based on the shape of the node.
      Graph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetFactory(
        node => PortCandidateProviders.Combine(
          PortCandidateProviders.FromExistingPorts(node),
          PortCandidateProviders.FromNodeCenter(node),
          PortCandidateProviders.FromShapeGeometry(node)));

      // To modify the existing lookup for a graph element, typically we decorate it with the help
      // of one of graph's Get...Decorator() extension methods,
      // which allows to dynamically insert custom implementations for the specified types.
      // Doing this can be seen as dynamically subclassing
      // the class in question (the INode implementation in this case), but only
      // for the node instances that live in the graph in question and then
      // overriding just their Lookup(Type) method. The only difference to traditional
      // subclassing is that you get the "this" passed in as a parameter.
      // Doing this more than once is like subclassing more and more, so the order matters.
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

    #region Sample graph creation
    /// <summary>
    /// Creates a sample graph and introduces all important graph elements present in
    /// yFiles WPF. Additionally, this method now overrides the label placement for some specific labels.
    /// </summary>
    private void PopulateGraph() {
      #region Sample Graph creation

      // Creates two nodes with the default node size
      // The location is specified for the _center_
      INode node1 = Graph.CreateNode(new PointD(50, 50));
      INode node2 = Graph.CreateNode(new PointD(150, 50));
      // Creates a third node with a different size of 80x40
      // In this case, the location of (360,280) describes the _upper left_
      // corner of the node bounds
      INode node3 = Graph.CreateNode(new RectD(260, 180, 80, 40));
      
      // Creates some edges between the nodes
      IEdge edge1 = Graph.CreateEdge(node1, node2);
      IEdge edge2 = Graph.CreateEdge(node2, node3);

      // Creates the first bend for edge2 at (400, 50)
      IBend bend1 = Graph.AddBend(edge2, new PointD(300, 50));

      // Actually, edges connect "ports", not nodes directly.
      // If necessary, you can manually create ports at nodes
      // and let the edges connect to these.
      // Creates a port in the center of the node layout
      IPort port1AtNode1 = Graph.AddPort(node1, FreeNodePortLocationModel.NodeCenterAnchored);

      // Creates a port at the middle of the left border
      // Note to use absolute locations when placing ports using PointD.
      IPort port1AtNode3 = Graph.AddPort(node3, new PointD(node3.Layout.X, node3.Layout.GetCenter().Y));

      // Creates an edge that connects these specific ports

      ///////////////// New in this Sample /////////////////

      // NOTE: the non-orthogonal edge is edited as if orthogonal edge editing was switched off.
      // Once one or more bends are added in a way that the edge becomes orthogonal
      // orthogonal edge editing is enabled for this edge, too.
      // Also note: if some segments of the edge are orthogonal and some are not
      // orthogonal edge editing is enabled for the orthogonal segments.

      //////////////////////////////////////////////////////
      IEdge edgeAtPorts = Graph.CreateEdge(port1AtNode1, port1AtNode3);

      // Adds labels to several graph elements
      Graph.AddLabel(node1, "Node 1");
      Graph.AddLabel(node2, "Node 2");
      Graph.AddLabel(node3, "Node 3");
      Graph.AddLabel(edgeAtPorts, "Not Orthogonal");

      // Add some more elements to have a larger graph to edit
      var n4 = Graph.CreateNode(new PointD(50, -50));
      Graph.AddLabel(n4, "Node 4");
      var n5 = Graph.CreateNode(new PointD(50, -150));
      Graph.AddLabel(n5, "Node 5");
      var n6 = Graph.CreateNode(new PointD(-50, -50));
      Graph.AddLabel(n6, "Node 6");
      var n7 = Graph.CreateNode(new PointD(-50, -150));
      Graph.AddLabel(n7, "Node 7");
      var n8 = Graph.CreateNode(new PointD(150, -50));
      Graph.AddLabel(n8, "Node 8");

      Graph.CreateEdge(n4, node1);
      Graph.CreateEdge(n5, n4);
      Graph.CreateEdge(n7, n6);
      var e6_1 = Graph.CreateEdge(n6, node1);
      Graph.AddBend(e6_1, new PointD(-50, 50), 0);

      // Creates a group node programmatically which groups the child nodes n4, n5, and n8
      var groupNode = CreateGroupNodes(n4, n5, n8);
      // creates an edge between the group node and node 2
      var eg_2 = Graph.CreateEdge(groupNode, node2);
      Graph.AddBend(eg_2, new PointD(100, 0), 0);
      Graph.AddBend(eg_2, new PointD(150, 0), 1);

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
      // Sets this style as the default for all nodes that don't have another
      // style assigned explicitly
      Graph.NodeDefaults.Style = new ShapeNodeStyle
      {
        Shape = ShapeNodeShape.RoundRectangle,
        Brush = new SolidColorBrush(Color.FromRgb(255, 108, 0)),
        Pen = new Pen(new SolidColorBrush(Color.FromRgb(102, 43, 0)), 1.5)
      };

      #endregion

      #region Default Edge Style
      // Sets the default style for edges:
      // Creates a PolylineEdgeStyle which will be used as default for all edges
      // that don't have another style assigned explicitly
      var defaultEdgeStyle = new PolylineEdgeStyle
      {
        Pen = new Pen(new SolidColorBrush(Color.FromRgb(102, 43, 0)), 1.5),
        TargetArrow = new Arrow
        {
          Type = ArrowType.Triangle,
          Brush = new SolidColorBrush(Color.FromRgb(102, 43, 0))
        }
      };

      // Sets the defined edge style as the default for all edges that don't have
      // another style assigned explicitly:
      Graph.EdgeDefaults.Style = defaultEdgeStyle;
      #endregion

      #region Default Label Styles
      // Sets the default style for labels
      // Creates a label style with the label text color set to dark red
      ILabelStyle defaultLabelStyle = new DefaultLabelStyle
      {
        Typeface = new Typeface("Tahoma"),
        TextSize = 12,
        TextBrush = Brushes.Black
      };

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
