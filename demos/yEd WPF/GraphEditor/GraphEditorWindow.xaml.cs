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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Demo.yFiles.GraphEditor.I18N;
using Demo.yFiles.GraphEditor.Input;
using Demo.yFiles.GraphEditor.Properties;
using Demo.yFiles.GraphEditor.Styles;
using Demo.yFiles.GraphEditor.UI;
using Demo.yFiles.GraphEditor.Modules;
using Demo.yFiles.GraphEditor.Modules.Layout;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using Microsoft.Win32;
using yWorks.Controls.Input;
using yWorks.Annotations;
using yWorks.Graph.LabelModels;
using Demo.yFiles.GraphEditor.Option;
using PolylineEdgeStyle = Demo.yFiles.GraphEditor.Styles.PolylineEdgeStyle;
using System.Linq;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Layout;
using yWorks.Utils;
using ArcEdgeStyle = Demo.yFiles.GraphEditor.Styles.ArcEdgeStyle;

namespace Demo.yFiles.GraphEditor
{
  /// <summary>
  /// Demo application that show most features that are currently present in yFiles WPF.
  /// </summary>
  /// <remarks>This demo draws heavily from other demo applications. Features presented include:
  /// - Interactive graph editing (structurally as well as for visual features)
  /// - GraphML I/O
  /// - Automatic graph layout
  /// - Undoability
  /// - Grouping
  /// - Custom functionality through input modes and lookup decoration
  /// - Drag and Drop
  /// </remarks>
  public partial class GraphEditorWindow
  {

    #region Fields

    private readonly string appName;

    /// <summary>
    /// The token for determining the "modified" state - obtained from the <c>UndoEngine</c>
    /// </summary>
    private object lastToken;

    // Determines whether the grid is visible
    private bool gridVisible;
    // The distance of the grid lines/points
    private int gridWidth;

    private GridSnapTypes gridSnapType;

    private string lastOpenedFile;
    // List of files recently used
    private readonly ObservableCollection<string> recentDocuments = new ObservableCollection<string>();

    private GridVisualCreator grid;

    private GraphEditorInputMode editMode;

    private TableEditorInputMode tableEditorInputMode;

    private static Predicate<INode> IsGroupNode;

    /// <summary>
    /// Option Handlers for nodes...
    /// </summary>
    private OptionHandler nodePropertyHandler;
    /// <summary>
    /// labels...
    /// </summary>
    private OptionHandler labelPropertyHandler;
    /// <summary>
    /// ports...
    /// </summary>
    private OptionHandler portPropertyHandler;
    /// <summary>
    /// and edges.
    /// </summary>
    private OptionHandler edgePropertyHandler;

    private readonly ResourceManagerI18NFactory resourceManagerI18NFactory = new ResourceManagerI18NFactory();

    /// <summary>
    /// Provide the properties for nodes, edges, and labels
    /// </summary>
    private DefaultSelectionProvider<INode> nodeSelectionProvider;
    private DefaultSelectionProvider<IEdge> edgeSelectionProvider;
    private DefaultSelectionProvider<ILabel> labelSelectionProvider;
    private DefaultSelectionProvider<IPort> portSelectionProvider;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the <see cref="IsGroupNode"/> predicate.
    /// </summary>
    static GraphEditorWindow() {
      IsGroupNode = IsGroupNodeMethod;
      ExtensionInitializeStatic();
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    static partial void ExtensionInitializeStatic();

    /// <summary>
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public GraphEditorWindow() {
      this.appName = (string) this.TryFindResource("AppName");
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

      saveFileDialog = new SaveFileDialog {AddExtension = true, DefaultExt = "graphml", Filter="GraphML Files|*.graphml|XML Files|*.xml|All files|*.*"};
      openFileDialog = new OpenFileDialog {AddExtension = true, DefaultExt = "graphml", Filter = "GraphML Files|*.graphml|XML Files|*.xml|All files|*.*" };
      InitializeComponent();


      Resources.Add("RecentDocuments", recentDocuments);
      Resources.Add("ZoomLevels", CreateZoomLevelPercentages());
      Resources.Add("SampleFiles", CreateSampleFiles());
      ExtensionInitialize();
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    partial void ExtensionInitialize();

    private static string[] CreateSampleFiles() {
      string s = Environment.CurrentDirectory;
      return Directory.GetFiles(s + "\\Resources\\Samples", "*.graphml");
    }

    private static int[] CreateZoomLevelPercentages() {
      return new[] { 5, 10, 25, 50, 75, 100, 125, 150, 175, 250, 500, 1000 };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl {
      get { return graphControl; }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    public IGraph Graph {
      get { return GraphControl.Graph; }
    }

    /// <summary>
    /// Gets the currently registered IGraphSelection instance from the GraphControl.
    /// </summary>
    public IGraphSelection Selection {
      get { return GraphControl.Selection; }
    }

    /// <summary>
    /// The <see cref="GraphEditorInputMode" /> used for graph editing.
    /// </summary>
    internal GraphEditorInputMode GraphEditorInputMode {
      get { return editMode; }
      set { editMode = value; }
    }

    /// <summary>
    /// Returns the visual representation of the grid.
    /// </summary>
    internal GridVisualCreator Grid {
      get { return grid; }
    }

    /// <summary>
    /// Reflects whether the current document has been modified
    /// </summary>
    private bool DocumentModified {
      get {
        UndoEngine engine = Graph.GetUndoEngine();
        // get the current token from the undo engine
        object currentToken = engine == null ? null : engine.GetToken();
        if (currentToken == null) {
          // end compare it with the last token that has been created by saves/document new
          return lastToken != null;
        } 
        return !currentToken.Equals(lastToken);
      }
    }

    public bool GridVisible {
      get { return gridVisible; }
      set {
        gridVisible = value;
        if (grid != null) {
          grid.Visible = value;
          // turn grid snapping on/off
          GraphSnapContext.GridSnapType = value ? gridSnapType : GridSnapTypes.None;
          toggleGridButton.IsChecked = value;
        }
        GraphControl.Invalidate();
      }
    }

    /// <summary>
    /// Sets the distance between the grid points
    /// </summary>
    public int GridWidth {
      get { return gridWidth; }
      set {
        if (gridWidth != value) {
          gridWidth = value;
          if (grid != null) {
            var nodeGrid = (GridConstraintProvider<INode>)(GraphSnapContext).NodeGridConstraintProvider;
            nodeGrid.GridInfo.HorizontalSpacing = gridWidth;
            nodeGrid.GridInfo.VerticalSpacing = gridWidth;
            var bendGrid = (GridConstraintProvider<IBend>)(GraphSnapContext).BendGridConstraintProvider;
            bendGrid.GridInfo.HorizontalSpacing = gridWidth;
            bendGrid.GridInfo.VerticalSpacing = gridWidth;
            GraphControl.Invalidate();
          }
        }
      }
    }

    public GridSnapTypes GridSnapType {
      get { return gridSnapType; }
      set {
        gridSnapType = value;
        GraphSnapContext.GridSnapType = gridSnapType;
      }
    }

    /// <summary>
    ///  Returns the <see cref="yWorks.Controls.Input.GraphSnapContext"/> of the <see cref="GraphEditorInputMode" />.
    /// </summary>
    internal GraphSnapContext GraphSnapContext {
      get { return (GraphSnapContext)GraphEditorInputMode.SnapContext; }
    }

    /// <summary>
    ///  Returns the <see cref="yWorks.Controls.Input.LabelSnapContext"/> of the <see cref="GraphEditorInputMode" />.
    /// </summary>
    internal LabelSnapContext LabelSnapContext {
      get { return (LabelSnapContext) GraphEditorInputMode.LabelSnapContext; }
    }

    /// <summary>
    ///  Returns the <see cref="OrthogonalEdgeEditingContext"/> of the <see cref="GraphEditorInputMode" />
    /// </summary>
    internal OrthogonalEdgeEditingContext OrthogonalEdgeEditingContext {
      get { return GraphEditorInputMode.OrthogonalEdgeEditingContext; }
    }

    public GraphMLIOHandler IoHandler { get; set; }

    /// <summary>
    /// Gets or sets the last opened file
    /// </summary>
    /// <remarks>Adds a file to list of recent documents when set.</remarks>
    private string LastOpenedFile {
      get { return lastOpenedFile; }
      set {
        if (value.Length > 0 && !recentDocuments.Contains(value)) {
          if (recentDocuments.Count < 5) {
            recentDocuments.Insert(0, value);
          } else {
            recentDocuments.RemoveAt(4);
            recentDocuments.Insert(0, value);
          }
        }
        lastOpenedFile = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="IGraph">MasterGraph</see> of the Graph
    /// </summary>
    public IGraph MasterGraph {
      get {
        return Graph.GetFoldingView().Manager.MasterGraph;
      }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the input modes.
    /// </summary>
    protected virtual void 
      InitializeInputModes() {
      var nodeGrid = new GridConstraintProvider<INode>(40);
      var bendGrid = new GridConstraintProvider<IBend>(40);

      //this is the main input mode that is used for all actual editing
      // we use a customized subclass with specialized marquee selection behavior
      GraphEditorInputMode mode = new MyGraphEditorInputMode {AllowGroupingOperations = true};

      mode.AdjustContentRectPolicy = AdjustContentRectPolicy.Always;

      // when changing focus by clicking the control, we want the graphcontrol 
      // to create a node or select the clicked elements anyway
      mode.ClickInputMode.SwallowFocusClick = false;

      mode.AllowClipboardOperations = true;

      // orthogonal edge editing and creation
      mode.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext
                                            {
                                              Enabled = Settings.Default.OrthogonalEdgesEnabled
                                            };

      mode.CreateEdgeInputMode.PreferredMinimalEdgeDistance = 10;
      mode.CreateEdgeInputMode.NodeBorderWidthRatio = 0.05;
      mode.CreateEdgeInputMode.ResolveSourcePortCandidates = true;
      mode.CreateEdgeInputMode.ResolveTargetPortCandidates = true;
      mode.CreateEdgeInputMode.PortCandidateResolutionRecognizer = EventRecognizers.Always;

      mode.SnapContext = new GraphSnapContext
                      {
                        Enabled = Settings.Default.SnappingEnabled,
                        SnapSegmentsToSnapLines = true,
                        EdgeToEdgeDistance = 10,
                        NodeToEdgeDistance = 15,
                        NodeToNodeDistance = 20,
                        SnapPortAdjacentSegments = true,
                        SnapBendsToSnapLines = true,
                        SnapBendAdjacentSegments = true,
                        GridSnapType = Settings.Default.GridVisible ? Settings.Default.GridSnapType : GridSnapTypes.None,
                        NodeGridConstraintProvider = nodeGrid,
                        BendGridConstraintProvider = bendGrid,
                      };
      mode.LabelSnapContext = new LabelSnapContext
                      {
                        Enabled = Settings.Default.SnappingEnabled,
                        SnapDistance = 15,
                        SnapLineExtension = 100,
                        CollectAllNodeDistanceSnapLines = true,
                        CollectAllEdgeDistanceSnapLines = true
                      };

      // we want to remove superfluous bends after an edge has been created orthogonally
      mode.CreateEdgeInputMode.EdgeCreated += delegate(object sender, EdgeEventArgs args) {
                                                var edgeMode = (CreateEdgeInputMode) sender;
                                                var context = editMode.InputModeContext.Lookup<OrthogonalEdgeEditingContext>();
                                                if (context != null && context.Enabled) {
                                                  var orthogonalEdgeHelper = args.Item.Lookup<IOrthogonalEdgeHelper>();
                                                  if (orthogonalEdgeHelper != null) {
                                                    orthogonalEdgeHelper.CleanUpEdge(edgeMode.InputModeContext, edgeMode.Graph, args.Item);
                                                  }
                                                }
                                              };

      mode.NodeDropInputMode = new GroupNodeDropInputMode();
      mode.NodeDropInputMode.ItemCreated += nodeDropInputMode_NodeCreated;

      mode.Add(new EdgeDropInputMode(typeof(IEdgeStyle), this));

      DropInputMode labelDropInputMode = new DropInputMode(typeof(ILabelStyle));
      labelDropInputMode.DragDropped += labelDropInputMode_DragDropped;
      mode.Add(labelDropInputMode);
      
      var portDropInputMode = mode.PortDropInputMode;
      portDropInputMode.Enabled = true;
      portDropInputMode.UseBestMatchingParameter = true;

      mode.ReparentNodeHandler = new NoTableReparentNodeHandler();

      //Create a new TEIM instance which also allows drag and drop
      tableEditorInputMode = new TableEditorInputMode {
        //Enable drag & drop
        StripeDropInputMode = { Enabled = true },
        //Maximal level for both reparent and drag and drop is 3 for rows and 3 for columns
        ReparentStripeHandler =
          new ReparentStripeHandler { MaxColumnLevel = 3, MaxRowLevel = 3 },
        Priority = mode.HandleInputMode.Priority + 1
      };

      //Add to GEIM - we set the priority higher than for the handle input mode so that handles win if both gestures are possible
      mode.Add(tableEditorInputMode);

      ConfigureContextMenus(mode);

      GraphEditorInputMode = mode;

      // register the mode with the control
      graphControl.InputMode = mode;

      GraphEditorInputMode.AutoRemoveEmptyLabels = Settings.Default.AutomaticallyRemoveEmptyLabels;

      GridWidth = Settings.Default.GridWidth;
      grid = new GridVisualCreator(nodeGrid.GridInfo);
      GraphControl.BackgroundGroup.AddChild(grid);
      GridSnapType = Settings.Default.GridSnapType;
      GridVisible = Settings.Default.GridVisible;

      // configure some additional behavior to the managed view whenever
      // the user collapses or expands a group node:
      // comment out the below property setting to get the default behavior
      // where group nodes are not being relocated upon expand/collapse operations
      GraphEditorInputMode.NavigationInputMode.AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.TopLeft;
    }

    /// <summary>
    /// Slightly customized version of GraphEditorInputMode that has a modified
    /// marquee selection behavior.
    /// </summary>
    internal sealed class MyGraphEditorInputMode : GraphEditorInputMode
    {
      /// <summary>
      /// Alternative Marquee Selection implementation
      /// </summary>
      /// <remarks>
      /// If ctrl is pressed at the end of the marquee selection. All items of the type of
      /// the current selection will be added to the selection.
      /// </remarks>
      protected override void MarqueeSelect(IInputModeContext context, RectD marqueeRectangle, IEnumerable<IModelItem> items, Predicate<IModelItem> predicate) {
        // see what is already selected
        var currentSelection = GraphItemTypesSupport.GetItemTypes(GraphSelection);
        // nothing - default behavior
        if (currentSelection == GraphItemTypes.None) {
          base.MarqueeSelect(context, marqueeRectangle, items, predicate);
          return;
        } else {
          // add all items of the same type to the selection.
          base.MarqueeSelect(context, marqueeRectangle,
              Graph.Nodes.Cast<IModelItem>()
                   .Concat(Graph.Edges)
                   .Concat(Graph.Ports)
                   .Concat(Graph.Labels)
                   .Concat(Graph.GetBends()),
              item => currentSelection.Is(item));
        }
      }
    }

    internal sealed class EdgeDropInputMode: ItemDropInputMode<IEdge>
    {
      private readonly PointD previewNodeOffset = new PointD(20, 10);
      private readonly PointD previewBendOffset = new PointD(0, 10);

      private readonly GraphEditorWindow graphEditorWindow;

      public EdgeDropInputMode(Type expectedType, GraphEditorWindow graphEditorWindow) : base(expectedType) {
        this.graphEditorWindow = graphEditorWindow;
      }

      private IEdgeStyle GetEdgeStyle() {
        return DropData as IEdgeStyle;
      }

      protected override void PopulatePreviewGraph(IGraph previewGraph) {
        IGraph graph = previewGraph;
        graph.NodeDefaults.Style = VoidNodeStyle.Instance;
        var dummyEdge = graph.CreateEdge(
          graph.CreateNode(new RectD(10, 10, 0, 0)),
          graph.CreateNode(new RectD(50, 30, 0, 0)), GetEdgeStyle());
        graph.AddBend(dummyEdge, new PointD(30, 10), 0);
        graph.AddBend(dummyEdge, new PointD(30, 30), 1);
      }

      protected override void UpdatePreview(IGraph previewGraph, PointD dragLocation) {
        previewGraph.SetNodeCenter(previewGraph.Nodes.ElementAt(0), dragLocation - previewNodeOffset);
        previewGraph.SetNodeCenter(previewGraph.Nodes.ElementAt(1), dragLocation + previewNodeOffset);
        var edge = previewGraph.Edges.First();
        previewGraph.ClearBends(edge);
        previewGraph.AddBend(edge, dragLocation - previewBendOffset, 0);
        previewGraph.AddBend(edge, dragLocation + previewBendOffset, 1);
        var canvas = InputModeContext.CanvasControl;
        if (canvas != null) {
          canvas.Invalidate();
        }
      }

      protected override IModelItem GetDropTarget(PointD dragLocation) {
        return graphEditorWindow.GraphEditorInputMode.FindItems(dragLocation, new[]{GraphItemTypes.Node}, item => true).FirstOrDefault() as INode;
      }

      protected override void OnDragDropped(InputModeEventArgs eventArgs) {
        // This method is called when an edge style is dropped onto the canvas.
        PointD location = DropLocation;
        object data = DropData;
        if (data is IEdgeStyle) {
          // Use the dropped edge style for changed/created edges.
          IEdgeStyle style = ((IEdgeStyle)data);
          var graphControl = graphEditorWindow.graphControl;
          // Look for an edge at the drop location.
          GraphModelManager manager = graphControl.GraphModelManager;
          // set the conditional to false in order to allow for dropping an edge onto an existing edge to 
          // change its style
#pragma warning disable 162,429
          IEnumerable<IEdge> ehits = true ?
            Enumerable.Empty<IEdge>() :
            manager.TypedHitElementsAt<IEdge>(location);
#pragma warning restore 162,429

          if (ehits.Any()) {
            // Set the style of the edge at the drop location to the dropped style.
            IEdge edge = ehits.First();
            IEdgeStyle edgeStyle;
            if (style is ArcEdgeStyle || style is yWorks.Graph.Styles.ArcEdgeStyle) {
              edgeStyle = (IEdgeStyle)style.Clone();
              graphControl.Graph.ClearBends(edge);
            } else {
              edgeStyle = style;
            }
            graphControl.Graph.SetStyle(edge, edgeStyle);
          } else {
            // Look for a node at the drop location.
            IEnumerable<INode> hits = manager.TypedHitElementsAt<INode>(location);
            INode node;
            if (hits.Any()) {
              node = hits.First();
            } else {
              // If there is no node at the drop location create a new one.
              node = graphControl.Graph.CreateNode(location);
            }
            // Start the creation of an edge from the node at a suitable port candidate
            // for the drop location with the dropped edge style.
            IPortLocationModelParameter candidateLocation = graphControl.Graph.NodeDefaults.Ports.GetLocationParameterInstance(node);
            DefaultPortCandidate candidate = new DefaultPortCandidate(node, candidateLocation);
            graphEditorWindow.GraphEditorInputMode.CreateEdgeInputMode.DoStartEdgeCreation(candidate, location);
          }
          graphControl.Focus();
        }
        // perform cleanup
        CleanUp();
      }
    }

    internal sealed class GroupNodeDropInputMode : NodeDropInputMode
    {
      public GroupNodeDropInputMode() {
        // determine which nodes should be created as group nodes.
        // reuse the tag at the node for this purpose
        IsGroupNodePredicate = IsGroupNode;
        
        Enabled = true;
      }

      protected override IModelItem GetDropTarget(PointD dragLocation) {
        var draggedNode = DraggedItem;
        if (draggedNode != null && draggedNode.Lookup(typeof (ITable)) != null) {
          // the dragged node has an ITable assigned so it should be top-level
          return null;
        }
        return base.GetDropTarget(dragLocation);
      }
    }

    internal sealed class NoTableReparentNodeHandler : ReparentNodeHandler
    {
      public override bool IsValidParent(IInputModeContext context, INode node, INode newParent) {
        // table nodes shall not become child nodes
        return node.Lookup(typeof (ITable)) == null && base.IsValidParent(context, node, newParent);
      }
    }

    private static bool IsGroupNodeMethod(INode node) {
      // check the node's tag
      return Equals("Group Nodes", node.Tag)
        || node.Lookup(typeof(ITable)) != null;
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoaded(object source, EventArgs e) {
      // initialize the graph
      InitializeGraph();

      InitializeApplication();
    }

    private void InitializeApplication() {
      LoadApplicationSettings(false);

      InitializeInputModes();

      zoomStatusLabel.Content = "Zoom: " + graphControl.Zoom*100 + "%";

      IoHandler = CreateIOHandler();
      this.Text = appName + " - [New File]";

      UndoEngine engine = Graph.GetUndoEngine();
      //we use the undo engine to get notified about general document state changes 
      if (engine != null) {
        engine.PropertyChanged +=
          delegate(object o, PropertyChangedEventArgs args) {
            string propertyName = args.PropertyName;
            if (propertyName.Equals("CanRedo") || propertyName.Equals("CanUndo") || propertyName.Equals("UndoName") ||
                propertyName.Equals("RedoName")) { }
            if (propertyName.Equals("PerformingRedo") || propertyName.Equals("PerformingUndo")) {
              //undo/redo change the structure and possibly other properties of our graph,
              //tree view and option handlers need to be notified of this
              GraphControl.UpdateContentRect();
            }
            undoButton.ToolTip = engine.UndoName;
            redoButton.ToolTip = engine.RedoName;
          };
        //we get a token that can be compared later on to check wether the document is modified
        lastToken = engine.GetToken();
      }

      EventFilter<EventArgs> filter = new EventFilter<EventArgs>(graphControl, TimeSpan.FromMilliseconds(250));
      filter.Event +=
        delegate {
          statusLabel1.Content = "Nodes: " + Graph.Nodes.Count;
          statusLabel2.Content = "Edges: " + Graph.Edges.Count;
        };
      Graph.NodeCreated += filter.OnEvent;
      Graph.NodeRemoved += filter.OnEvent;
      Graph.EdgeCreated += filter.OnEvent;
      Graph.EdgeRemoved += filter.OnEvent;

      Graph.LabelAdded += Graph_LabelChanged;
      Graph.LabelRemoved += Graph_LabelChanged;
      Graph.LabelLayoutParameterChanged += Graph_LabelChanged;
      Graph.LabelStyleChanged += Graph_LabelChanged;
      Graph.LabelPreferredSizeChanged += Graph_LabelChanged;
      Graph.LabelTagChanged += Graph_LabelChanged;
      Graph.LabelTextChanged += Graph_LabelChanged;

      //create option handlers
      CreateOptionHandlers();

      GraphControl.ZoomChanged += GraphControl_ZoomChanged;
      GraphControl.FitContent();

      toggleGridButton.IsChecked = Settings.Default.GridVisible;
      toggleSnaplinesButton.IsChecked = Settings.Default.SnappingEnabled;
      toggleOrthogonalEdgesButton.IsChecked = Settings.Default.OrthogonalEdgesEnabled;

      ExtensionInitializeApplication();
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    partial void ExtensionInitializeApplication();

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      FoldingManager foldingManager = CreateFoldingManager();
      IFoldingView foldingView = foldingManager.CreateFoldingView();

      // we want to add navigational commands (enter/exit group), expand, collapse
      // as undo units
      foldingView.EnqueueNavigationalUndoUnits = true;

      // show the managed View in the GraphControl
      graphControl.Graph = foldingView.Graph;
      IGraph masterGraph = foldingManager.MasterGraph;
      masterGraph.NodeDefaults.Labels.AutoAdjustPreferredSize = true;
      masterGraph.NodeDefaults.Ports.AutoCleanUp = true;
      masterGraph.EdgeDefaults = new YEdEdgeDefaults();
      masterGraph.EdgeDefaults.Labels.AutoAdjustPreferredSize = true;

      masterGraph.NodeDefaults.Ports.Labels.LayoutParameter =
          masterGraph.EdgeDefaults.Ports.Labels.LayoutParameter =
              FreePortLabelModel.Instance.CreateParameter(new PointD(0, 5), new PointD(0.5, 0), PointD.Origin, 0);

      masterGraph.SetUndoEngineEnabled(true);
      //Use the undo support from the graph also for all future table instances
      Table.InstallStaticUndoSupport(Graph);

      var groupNodeDefaults = foldingView.Graph.GroupNodeDefaults;
      groupNodeDefaults.Style = new GroupNodeStyle() {Wrapped = 
        new PanelNodeStyle { Color = Color.FromArgb(255, 207, 226, 248),LabelInsetsColor = Color.FromArgb(255, 166, 202, 240) }};

      masterGraph.NodeDefaults.Style = new BevelNodeStyle {Color = Colors.DarkOrange, Inset = 2, Radius = 5};
      masterGraph.NodeDefaults.Size = new SizeD(50, 30);
      // centered in the node, similar to InteriorLabelModel.Center, but with a smart label placement logic
      masterGraph.NodeDefaults.Labels.LayoutParameter =
        FreeNodeLabelModel.Instance.CreateParameter(new PointD(0.5, 0.5), PointD.Origin, new PointD(0.5, 0.5), PointD.Origin, 0);

      // use a custom poly line style
      masterGraph.EdgeDefaults.Style = new PolylineEdgeStyle() { TargetArrowType = ArrowType.Default};

      // and a simpler labelstyle
      var defaultLabelStyle = new LabelStyle(){FontSize = 10};
      masterGraph.NodeDefaults.Labels.Style = defaultLabelStyle;
      masterGraph.NodeDefaults.Ports.Labels.Style = defaultLabelStyle;
      masterGraph.EdgeDefaults.Labels.Style = defaultLabelStyle;
      masterGraph.EdgeDefaults.Ports.Labels.Style = defaultLabelStyle;

      // disable style sharing
      masterGraph.NodeDefaults.ShareStyleInstance = false;
      masterGraph.EdgeDefaults.ShareStyleInstance = false;
      masterGraph.NodeDefaults.Labels.ShareStyleInstance = false;
      masterGraph.EdgeDefaults.Labels.ShareStyleInstance = false;
      masterGraph.NodeDefaults.Ports.ShareStyleInstance = false;
      masterGraph.EdgeDefaults.Ports.ShareStyleInstance = false;

      // also for group nodes...
      groupNodeDefaults.Labels.Style = new LabelStyle() {FontSize = 10, HorizontalTextAlignment = TextAlignment.Right};
      var labelModel = new InteriorStretchLabelModel { Insets = new InsetsD(15, 1, 1, 1) };
      groupNodeDefaults.Labels.LayoutParameter = labelModel.CreateParameter(InteriorStretchLabelModel.Position.North);

      // set up the default edge label model
      masterGraph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateDefaultParameter();

      // now change some default behavior for certain aspects 
      var graphDecorator = Graph.GetDecorator();
      var nodeDecorator = graphDecorator.NodeDecorator;
      // we use the size of the labels for group node insets
      nodeDecorator.InsetsProviderDecorator.SetImplementationWrapper((node, baseImplementation) => new LabelInsetsProvider(baseImplementation));

      // add custom node size constraints that consider labels that are stretched inside the node
      nodeDecorator.SizeConstraintProviderDecorator.SetImplementationWrapper((item, implementation) => new LabelSizeConstraintProvider(implementation));

      // allow moving of ports and removal of node internal bends
      graphDecorator.EdgeDecorator.OrthogonalEdgeHelperDecorator.SetImplementation(new CustomOrthogonalEdgeHelper());

      // we don't allow moving of edges - instead ports should be moved
      graphDecorator.EdgeDecorator.PositionHandlerDecorator.HideImplementation();

      // we want to move the end of the edge freely if the first or last segment is moved - however constraint to 
      // the node layout 
      graphDecorator.EdgeDecorator.EdgePortHandleProviderDecorator.SetImplementation(new ConstrainedEdgeEndHandleProvider());
      
      // if an edge is selected, its source and target IPort handles should be shown.
      graphDecorator.EdgeDecorator.HandleProviderDecorator.SetFactory(edge => new PortRelocationHandleProvider(null, edge));


      // there are some things that should not be done in the view, but rather
      // in the master graph (the actual model) - 
      // determining port candidates is one such thing, because the view contains dummy
      // ports that should not be used, also folding edges should not be relocatable.
      // The default implementations of the IEdgeReconnectionPortCandidateProvider and IPortCandidateProvider
      // in the implementation of a folding view graph already consider this.
      foldingManager.MasterGraph.GetDecorator().EdgeDecorator.EdgeReconnectionPortCandidateProviderDecorator.SetImplementation(
        EdgeReconnectionPortCandidateProviders.AllNodeCandidates);

      // use our smart port candidate provider implementation to provide the port candidates we would like to have
      foldingManager.MasterGraph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetFactory(
        node => new CustomPortCandidateProvider(node));
      ExtensionInitializeGraph();
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    partial void ExtensionInitializeGraph();

    /// <summary>
    /// Always clone ArcEdgeStyles because their handles should be independent of each other
    /// </summary>
    private sealed class YEdEdgeDefaults : EdgeDefaults
    {
      public override IEdgeStyle GetStyleInstance()
      {
        if (Style is ArcEdgeStyle || Style is yWorks.Graph.Styles.ArcEdgeStyle) {
          return (IEdgeStyle) Style.Clone();
        }
        return base.GetStyleInstance();
      }
    }

    /// <summary>
    /// Factory method that creates the <see cref="FoldingManager"/> instance
    /// to be used by the editor.
    /// </summary>
    protected virtual FoldingManager CreateFoldingManager() {
      // create a manager
      FoldingManager foldingManager = new FoldingManager();
      // assign a customized converter
      DefaultFolderNodeConverter folderNodeConverter = new DefaultFolderNodeConverter();
      folderNodeConverter.FolderNodeSize = new SizeD(40, 30);

      foldingManager.FolderNodeConverter = folderNodeConverter;

      // and a custom one for folding edges
      DefaultFoldingEdgeConverter foldingEdgeConverter = new DefaultFoldingEdgeConverter();
      // since our app is not port centric and instead has exactly one source and target
      // port per edge, that only holds the edge, we will reuse dummy ports for folding edges.
      foldingEdgeConverter.ReuseMasterPorts = false;
      foldingManager.FoldingEdgeConverter = foldingEdgeConverter;

      return foldingManager;
    }

    #endregion

    #region Custom Commands
    
    public static readonly RoutedUICommand PreferencesCommand = new RoutedUICommand("Preferences", "Preferences",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand SetZoomLevelCommand = new RoutedUICommand("Set Zoom Level", "SetZoomLevel",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand RunModuleCommand = new RoutedUICommand("Run Module", "RunModule",
                                                                            typeof(GraphEditorWindow));

    public static readonly RoutedUICommand ToggleGridCommand = new RoutedUICommand("Toggle Grid Display", "ToggleGrid",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ToggleOrthogonalEdgesCommand = new RoutedUICommand("Toggle Orthogonal Edge Handling", "ToggleOrthogonalEdges",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ToggleSnapLinesCommand = new RoutedUICommand("Toggle Snapping", "ToggleSnapLines",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ToggleSelectionModeCommand = new RoutedUICommand("Toggle Selection Mode", "ToggleSelectionMode",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ApplyStyleCommand = new RoutedUICommand("Apply Style", "ApplyStyle",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand SetStyleDefaultCommand = new RoutedUICommand("Set As Default", "SetStyleDefault",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ResetSettingsCommand = new RoutedUICommand("Reset Settings", "ResetSettings",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand OriginalViewCommand = new RoutedUICommand("Original View", "OriginalView",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ExportCommand = new RoutedUICommand("Export", "Export",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "Exit",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand AboutCommand = new RoutedUICommand("About", "About",
                                                                            typeof(GraphEditorWindow));
    public static readonly RoutedUICommand SampleFilesCommand = new RoutedUICommand("Sample Files", "SampleFiles",
                                                                            typeof(GraphEditorWindow));

    private SaveFileDialog saveFileDialog;
    private OpenFileDialog openFileDialog;

    #endregion

    #region Command Handlers

    private void OnSampleFilesExecuted(object sender, ExecutedRoutedEventArgs e) {
      CreateNewDocument();
      string item = e.Parameter as string;
      if (item != null) {
        try {
          ReadGraphML(Graph, item);
          GraphControl.Invalidate();
          GraphControl.UpdateContentRect();
      } catch (Exception exc) {
          MessageBox.Show("Error reading GraphML file: " + exc.Message,
                          "Error", MessageBoxButton.OK);
        }
      }
    }

    private void OnExitExecuted(object sender, ExecutedRoutedEventArgs e) {
      Application.Current.Shutdown();
    }

    #region Layout modules

    /// <summary>
    /// First displays the options dialog and then starts the layout module
    /// </summary>
    /// <param name="module"></param>
    private void StartModule(YModule module) {
      // avoid double registration
      module.ValuesCommitted -= ModuleOnValuesCommitted;
      module.ValuesCommitted += ModuleOnValuesCommitted;
      module.Modal = false;
      module.ShowModule(this);
    }

    private void ModuleOnValuesCommitted(object sender, EventArgs eventArgs) {
      if (moduleRunning) {
        return;
      }
      if (!GraphEditorInputMode.TryStop()) {
        GraphEditorInputMode.Cancel();
      }
      YModule module = (YModule)sender;
      var dictionary = new Dictionary<Type, object>();
      dictionary[typeof(IGraph)] = Graph;
      dictionary[typeof(CanvasControl)] = GraphControl;
      dictionary[typeof(GraphControl)] = GraphControl;
      dictionary[typeof(ISelectionModel<IModelItem>)] = GraphControl.Selection;
      if (module is LayoutModule) {
        moduleRunning = true;
        ((LayoutModule)module).Done += OnLayoutModuleDone;
      }
      ExtensionModuleOnValuesCommitted(module);
      module.Start(Lookups.CreateDictionaryLookup(dictionary));
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    partial void ExtensionModuleOnValuesCommitted(YModule module);

    private void OnLayoutModuleDone(object sender, LayoutEventArgs eventArgs) {
      ((LayoutModule)sender).Done -= OnLayoutModuleDone;
      moduleRunning = false;
      ExtensionOnLayoutModuleDone((LayoutModule)sender);
      if (!eventArgs.Handled && eventArgs.Exception != null) {
        MessageBox.Show(eventArgs.Exception.Message, "Exception during layout",
                        MessageBoxButton.OK);
        eventArgs.Handled = true;
      }
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    partial void ExtensionOnLayoutModuleDone(YModule module);

    #endregion

    private void OnNewExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
    {
      CreateNewDocument();
    }

    private void OnAboutExecuted(object sender, ExecutedRoutedEventArgs e) {
      AboutBox box = new AboutBox();
      box.Owner = this;
      box.ShowDialog();
    }

    private void OnOpenExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs) {

      if (!string.IsNullOrEmpty(LastOpenedFile)) {
        openFileDialog.FileName = LastOpenedFile;
      } else {
        openFileDialog.FileName = string.Empty;
        if (recentDocuments != null && recentDocuments.Count > 0) {
          openFileDialog.InitialDirectory = Path.GetDirectoryName(recentDocuments[0]);
        }
      }
      if (openFileDialog.ShowDialog(this) == true) {
        Load(openFileDialog.FileName);
      }
    }

    private void Load(string fileName) {
      CreateNewDocument();
      try {
        ReadGraphML(GraphControl.Graph, fileName);
        GraphControl.Invalidate();
        GraphControl.UpdateContentRect();
        LastOpenedFile = fileName;
      } catch (Exception exc) {
        MessageBox.Show(this, "Error reading GraphML file: " + exc.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void OnSaveExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs) {
      try {
        if (!string.IsNullOrEmpty(LastOpenedFile)) {
          WriteGraphML(Graph, lastOpenedFile);
        } else {
          if (saveFileDialog.ShowDialog(this) == true) {
            WriteGraphML(Graph, saveFileDialog.FileName);
            LastOpenedFile = saveFileDialog.FileName;
          }
        }
      } catch (Exception exc) {
        MessageBox.Show(this, "Error writing GraphML file: " + exc.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void OnSaveAsExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs) {
      if (!string.IsNullOrEmpty(LastOpenedFile)) {
        saveFileDialog.FileName = LastOpenedFile;
      } else {
        saveFileDialog.FileName = string.Empty;
        if (recentDocuments != null && recentDocuments.Count > 0) {
          saveFileDialog.InitialDirectory = Path.GetDirectoryName(recentDocuments[0]);
        }
      }
      if (saveFileDialog.ShowDialog(this) == true) {
        try {
          WriteGraphML(Graph, saveFileDialog.FileName);
          LastOpenedFile = saveFileDialog.FileName;
        } catch (Exception exc) {
          MessageBox.Show(this, "Error writing GraphML file: " + exc.Message,
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void OnOriginalViewExecuted(object sender, ExecutedRoutedEventArgs e) {
      graphControl.Zoom = 1.0d;
    }

    private void OnPreferencesExecuted(object sender, ExecutedRoutedEventArgs e) {
      DefaultsEditor.EditDefaults(this);
    }

    private void OnResetSettingsExecuted(object sender, ExecutedRoutedEventArgs e) {
      Settings.Default.Reset();
      Settings.Default.Reload();
      LoadApplicationSettings(true);
      GraphEditorInputMode.AutoRemoveEmptyLabels = Settings.Default.AutomaticallyRemoveEmptyLabels;
      GraphSnapContext.GridSnapType = Settings.Default.GridEnabled ? GridSnapTypes.GridPoints : GridSnapTypes.None;
      toggleGridButton.IsChecked = Settings.Default.GridEnabled;
      GridWidth = Settings.Default.GridWidth;
      GridVisible = Settings.Default.GridVisible;
    }

    private void OnExportExecuted(object sender, ExecutedRoutedEventArgs e) {
      // reuse the Image Export demo's window...
      var imageExportWindow = new ImageExport.ImageExportWindow()
                                {
                                  Owner = this,
                                  WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                  Icon = this.Icon,
                                  ShowInTaskbar = false,
                                  Title = "Export Graph",
                                  FlowDirection = graphControl.FlowDirection,
                                };
      // and the prepared functionality to export the contents of the current graphControl
      imageExportWindow.Loaded += (o, args) => imageExportWindow.ShowGraph(graphControl);
      imageExportWindow.ShowDialog();
    }

    private void OnPropertiesExecuted(object sender, ExecutedRoutedEventArgs e) {
      EditProperties();
    }

    private void OnCanExecutePropertiesCommand(object sender, CanExecuteRoutedEventArgs e) {
      IGraphSelection selection = graphControl.Selection;
      e.CanExecute = selection.Count == 1;
      e.Handled = true;
    }

    private void OnToggleGridExecuted(object sender, ExecutedRoutedEventArgs e) {
      GridVisible = !GridVisible;
    }

    private void OnToggleOrthogonalEdgesExecuted(object sender, ExecutedRoutedEventArgs e) {
      var newValue = !OrthogonalEdgeEditingContext.Enabled;
      OrthogonalEdgeEditingContext.Enabled = newValue;
    }

    private void OnToggleSnapLinesExecuted(object sender, ExecutedRoutedEventArgs e) {
      GraphSnapContext.Enabled = !GraphSnapContext.Enabled;
      LabelSnapContext.Enabled = !LabelSnapContext.Enabled;
    }

    private void OnToggleSelectionModeExecuted(object sender, ExecutedRoutedEventArgs e) {
      GraphEditorInputMode.LassoSelectionInputMode.Enabled = !GraphEditorInputMode.LassoSelectionInputMode.Enabled; 
    }

    #endregion

    #region UI Event Handlers

    private void GraphControl_ZoomChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
      zoomStatusLabel.Content = "Zoom: " + Math.Round(graphControl.Zoom*100, 2) + "%";
    }

    /// <summary>
    /// Callback method whenever the content of a selection provider has been changed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void edgeSelectionProvider_SelectionChanged(object source, EventArgs e) {
      //we just rebuild the complete option handler for edge properties in this case
      edgePropertyHandler.BuildFromSelection(edgeSelectionProvider,
                                             Lookups.CreateContextLookupChainLink(GetOptionBuilderCallback));
    }

    /// <summary>
    /// Callback method whenever the content of a selection provider has been changed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void labelSelectionProvider_SelectionChanged(object source, EventArgs e) {
      //we just rebuild the complete option handler for label properties in this case
      labelPropertyHandler.BuildFromSelection(labelSelectionProvider,
                                              Lookups.CreateContextLookupChainLink(GetOptionBuilderCallback));
    }

    /// <summary>
    /// Callback method whenever the content of a selection provider has been changed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void portSelectionProvider_SelectionChanged(object source, EventArgs e) {
      //we just rebuild the complete option handler for port properties in this case
      portPropertyHandler.BuildFromSelection(portSelectionProvider,
                                              Lookups.CreateContextLookupChainLink(GetOptionBuilderCallback));
    }

    /// <summary>
    /// Callback method whenever the content of a selection provider has been changed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void nodeSelectionProvider_SelectionChanged(object source, EventArgs e) {
      //we just rebuild the complete option handler for node properties in this case
      nodePropertyHandler.BuildFromSelection(nodeSelectionProvider,
                                             Lookups.CreateContextLookupChainLink(GetOptionBuilderCallback));
    }

    /// <summary>
    /// Update all necessary objects after a label has changed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void Graph_LabelChanged(object source, ItemEventArgs<ILabel> e) {
      ILabel label = e.Item;
      //update all option handlers
      if (label.Owner is INode) {
        nodeSelectionProvider.UpdatePropertyViews();
      } else if (label.Owner is IEdge) {
        edgeSelectionProvider.UpdatePropertyViews();
      } else if (label.Owner is IPort) {
        portSelectionProvider.UpdatePropertyViews();
      }
      labelSelectionProvider.UpdatePropertyViews();
    }

    private void OnWindowClosing(object sender, CancelEventArgs e) {
      //ask for file save if necessary
      if (DocumentModified) {
        var result = MessageBox.Show(this, DefaultStringResources.ExitConfirmation_UnsavedChanges_Text,
                                              DefaultStringResources.ExitConfirmation_UnsavedChanges_Caption,
                                              MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
        switch (result) {
          case MessageBoxResult.Yes:
            OnSaveExecuted(this, null);
            SaveApplicationSettings();
            break;
          case MessageBoxResult.No:
            SaveApplicationSettings();
            break;
          case MessageBoxResult.Cancel:
            e.Cancel = true;
            break;
        }
      } else {
        SaveApplicationSettings();
        e.Cancel = false;
      }
    }

    #endregion

    #region Option Handlers
    /// <summary>
    /// Create and configure the main option handlers for model item properties
    /// </summary>
    private void CreateOptionHandlers() {
      //initialize default resource bundles
      ResourceManager defaultResources =
        new ResourceManager("Demo.yFiles.GraphEditor.I18N.DefaultOptionI18N",
                            Assembly.GetExecutingAssembly());
      ResourceManager brushResources =
        new ResourceManager("Demo.yFiles.GraphEditor.I18N.BrushPropertiesI18N",
                            Assembly.GetExecutingAssembly());


      var dictionary = new Dictionary<Type, object>();
      dictionary[typeof(IGraph)] = Graph;
      var defaultLookup = Lookups.CreateDictionaryLookup(dictionary);

      this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", defaultResources);
      {
        //create handlers and selection providers
        nodePropertyHandler = new OptionHandler("NODE_PROPERTIES");

        ResourceManager rm =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.NodePropertiesI18N",
                              Assembly.GetExecutingAssembly());
        //bind the resource manager to the option handler, using the given contexts
        //this allows to use the same resource bundle for different option handler structures
        this.resourceManagerI18NFactory.AddResourceManager(nodePropertyHandler.Name, rm);
        this.resourceManagerI18NFactory.AddResourceManager(nodePropertyHandler.Name,
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.NodeStylePropertiesI18N",
                              Assembly.GetExecutingAssembly()));

        //the i18nfactory will also try to strip of the leading "SELECTION_PROPERTIES" prefix
        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", rm);

        //Add general resource managers
        this.resourceManagerI18NFactory.AddResourceManager(nodePropertyHandler.Name, brushResources);

        //nodePropertyHandler.I18nFactory = this.resourceManagerI18NFactory;

        //create a selection provider that manages the translation from model item properties to option handler
        //presentation and vice versa, including managing conflicting values in a selection etc.
        //this selection provider will work on all selected nodes (the delegate would allow further filtering
        nodeSelectionProvider = new DefaultSelectionProvider<INode>(GraphControl.Selection.SelectedNodes,
                                                                    delegate { return true; });
        // we would like to collapse requests so that the handler does not get thrashed
        nodeSelectionProvider.EventCollapseTimeSpan = TimeSpan.FromMilliseconds(300);
        nodeSelectionProvider.InnerLookup = defaultLookup;
        nodeSelectionProvider.ContextLookup = Lookups.CreateContextLookupChainLink(GetPropertyBuilderCallback);
        //when the selection content changes, trigger this action (usually rebuild associated option handlers)
        nodeSelectionProvider.SelectedItemsChanged += nodeSelectionProvider_SelectionChanged;
        //when model item properties have been changed from the "outside" through the selection provider,
        //the GraphControl needs an update
        nodeSelectionProvider.PropertyItemsChanged += delegate {
          GraphControl.Invalidate();
          nodeSelectionProvider_SelectionChanged(GraphControl, null);
        };
      }
      {
        //same for edges
        edgePropertyHandler = new OptionHandler("EDGE_PROPERTIES");
        ResourceManager rm =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.EdgePropertiesI18N",
                              Assembly.GetExecutingAssembly());
        this.resourceManagerI18NFactory.AddResourceManager(edgePropertyHandler.Name, rm);
        this.resourceManagerI18NFactory.AddResourceManager(edgePropertyHandler.Name,
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.EdgeStylePropertiesI18N",
                              Assembly.GetExecutingAssembly()));
        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", rm);

        //Add general resource managers
        this.resourceManagerI18NFactory.AddResourceManager(edgePropertyHandler.Name, brushResources);

        //edgePropertyHandler.I18nFactory = this.resourceManagerI18NFactory;
        edgeSelectionProvider = new DefaultSelectionProvider<IEdge>(GraphControl.Selection.SelectedEdges,
                                                                    delegate { return true; });
        // we would like to collapse requests so that the handler does not get thrashed
        edgeSelectionProvider.EventCollapseTimeSpan = TimeSpan.FromMilliseconds(300);
        edgeSelectionProvider.InnerLookup = defaultLookup;
        edgeSelectionProvider.ContextLookup = Lookups.CreateContextLookupChainLink(GetPropertyBuilderCallback);
        //when the selection content changes, trigger this action (usually rebuild associated option handlers)
        edgeSelectionProvider.SelectedItemsChanged += edgeSelectionProvider_SelectionChanged;
        //when model item properties have been changed from the "outside" through the selection provider,
        //the GraphControl needs an update
        edgeSelectionProvider.PropertyItemsChanged += delegate {
          GraphControl.Invalidate();
          edgeSelectionProvider_SelectionChanged(this, null);
        };
      }
      {
        //and labels
        labelPropertyHandler = new OptionHandler("Label");
        ResourceManager labelProperties =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.LabelPropertiesI18N",
                              Assembly.GetExecutingAssembly());
        this.resourceManagerI18NFactory.AddResourceManager(labelPropertyHandler.Name, labelProperties);
        ResourceManager labelModelProperties =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.LabelModelPropertiesI18N",
                              Assembly.GetExecutingAssembly());
        ResourceManager labelStyleProperties =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.LabelStylePropertiesI18N",
                              Assembly.GetExecutingAssembly());

        this.resourceManagerI18NFactory.AddResourceManager(labelPropertyHandler.Name, labelProperties);
        this.resourceManagerI18NFactory.AddResourceManager(labelPropertyHandler.Name, labelModelProperties);
        this.resourceManagerI18NFactory.AddResourceManager(labelPropertyHandler.Name, labelStyleProperties);

        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", labelProperties);
        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", labelModelProperties);
        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", labelStyleProperties);

        //Add general resource managers
        this.resourceManagerI18NFactory.AddResourceManager(labelPropertyHandler.Name, brushResources);

        resourceManagerI18NFactory.AddResourceManager("CREATE_LABEL", defaultResources);

        //we also want to use this bundle for label properties that are embedded in node and edge properties
        resourceManagerI18NFactory.AddResourceManager(nodePropertyHandler.Name, labelProperties);
        resourceManagerI18NFactory.AddResourceManager(nodePropertyHandler.Name, labelModelProperties);
        resourceManagerI18NFactory.AddResourceManager(nodePropertyHandler.Name, labelStyleProperties);

        resourceManagerI18NFactory.AddResourceManager(edgePropertyHandler.Name, labelProperties);
        resourceManagerI18NFactory.AddResourceManager(edgePropertyHandler.Name, labelModelProperties);
        resourceManagerI18NFactory.AddResourceManager(edgePropertyHandler.Name, labelStyleProperties);

        //labelPropertyHandler.I18nFactory = this.resourceManagerI18NFactory;
        labelSelectionProvider = new DefaultSelectionProvider<ILabel>(GraphControl.Selection.SelectedLabels,
                                                                      delegate { return true; });
        // we would like to collapse requests so that the handler does not get thrashed
        labelSelectionProvider.EventCollapseTimeSpan = TimeSpan.FromMilliseconds(300);
        labelSelectionProvider.InnerLookup = defaultLookup;
        labelSelectionProvider.ContextLookup = Lookups.CreateContextLookupChainLink(GetPropertyBuilderCallback);

        //when the selection content changes, trigger this action (usually rebuild associated option handlers)
        labelSelectionProvider.SelectedItemsChanged += labelSelectionProvider_SelectionChanged;

        //when model item properties have been changed from the "outside" through the selection provider,
        //the GraphControl needs an update
        labelSelectionProvider.PropertyItemsChanged += delegate {
          GraphControl.Invalidate();
          labelSelectionProvider_SelectionChanged(this, null);
        };
      }
      {
        //and ports
        portPropertyHandler = new OptionHandler("Port");
        ResourceManager portProperties =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.PortPropertiesI18N",
                              Assembly.GetExecutingAssembly());
        this.resourceManagerI18NFactory.AddResourceManager(portPropertyHandler.Name, portProperties);
        ResourceManager portStyleProperties =
          new ResourceManager("Demo.yFiles.GraphEditor.I18N.PortStylePropertiesI18N",
                              Assembly.GetExecutingAssembly());

        this.resourceManagerI18NFactory.AddResourceManager(portPropertyHandler.Name, portProperties);
        this.resourceManagerI18NFactory.AddResourceManager(portPropertyHandler.Name, portStyleProperties);

        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", portProperties);
        this.resourceManagerI18NFactory.AddResourceManager("SELECTION_PROPERTIES", portStyleProperties);

        //Add general resource managers
        this.resourceManagerI18NFactory.AddResourceManager(portPropertyHandler.Name, brushResources);

        //labelPropertyHandler.I18nFactory = this.resourceManagerI18NFactory;
        portSelectionProvider = new DefaultSelectionProvider<IPort>(GraphControl.Selection.SelectedPorts,
                                                                      delegate { return true; });
        // we would like to collapse requests so that the handler does not get thrashed
        portSelectionProvider.EventCollapseTimeSpan = TimeSpan.FromMilliseconds(300);
        portSelectionProvider.InnerLookup = defaultLookup;
        portSelectionProvider.ContextLookup = Lookups.CreateContextLookupChainLink(GetPropertyBuilderCallback);

        //when the selection content changes, trigger this action (usually rebuild associated option handlers)
        portSelectionProvider.SelectedItemsChanged += portSelectionProvider_SelectionChanged;

        //when model item properties have been changed from the "outside" through the selection provider,
        //the GraphControl needs an update
        portSelectionProvider.PropertyItemsChanged += delegate {
          GraphControl.Invalidate();
          portSelectionProvider_SelectionChanged(this, null);
        };
      }

      //whenever the selection changes, we want to update the selections (which will in turn trigger
      //option handler updates)
      EventHandler<ItemSelectionChangedEventArgs<IModelItem>> selectionDelegate =
        delegate(object source, ItemSelectionChangedEventArgs<IModelItem> args) {
          if (args.Item is INode) {
            nodeSelectionProvider.UpdatePropertyViews();
          } else if (args.Item is IEdge) {
            edgeSelectionProvider.UpdatePropertyViews();
          } else if (args.Item is ILabel) {
            labelSelectionProvider.UpdatePropertyViews();
          } else if (args.Item is IPort) {
            portSelectionProvider.UpdatePropertyViews();
          } else {
            return;
          }
          FocusPropertyTab();
        };
      GraphControl.Selection.ItemSelectionChanged += selectionDelegate;

      //also detect node movement and resize events if available
      Graph.NodeLayoutChanged += delegate { nodeSelectionProvider.UpdatePropertyViews(); };
      editorControl.OptionHandler = nodePropertyHandler;
    }

    /// <summary>
    /// Just focuses the property tab for the properties of the first item in the selection
    /// </summary>
    private void FocusPropertyTab() {
      if (Selection.Count > 0) {
        //transfer back focus to GraphControl if this had been focused, since
        //SelectTab and friends sometimes grab keyboard focus
        IEnumerator<IModelItem> enumerator = Selection.GetEnumerator();
        enumerator.MoveNext();
        IModelItem item = enumerator.Current;
        if (item is INode) {
          editorControl.OptionHandler = nodePropertyHandler;
        } else if (item is IEdge) {
          editorControl.OptionHandler = edgePropertyHandler;
        } else if (item is ILabel) {
          editorControl.OptionHandler = labelPropertyHandler;
        } else if (item is IPort) {
          editorControl.OptionHandler = portPropertyHandler;
        }
      }
    }

    private static object GetPropertyBuilderCallback(object subject, Type type) {
      if (type == typeof(IPropertyMapBuilder)) {
        if (subject is INode) {
          return new DefaultNodePropertyMapBuilder();
        } else if (subject is ILabel) {
          return new DefaultLabelPropertyMapBuilder();
        } else if (subject is IPort) {
          return new DefaultPortPropertyMapBuilder();
        } else if (subject is IEdge) {
          return new DefaultEdgePropertyMapBuilder();
        } else if (subject is INodeStyle) {
          return new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType());
        } else if (subject is IEdgeStyle) {
          return new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType());
        } else if (subject is ILabelStyle) {
          return new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType());
        } else if (subject is IPortStyle) {
          return new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType());
        }
        return null;
      }
      return null;
    }

    private static IOptionBuilder GetOptionBuilderCallback(object subject, Type type) {
      IOptionBuilder builder = null;
      if (type == typeof(IOptionBuilder)) {
        if (subject is INode) {
          builder = new DefaultNodeOptionBuilder();
        } else if (subject is IEdge) {
          builder = new DefaultEdgeOptionBuilder();
        } else if (subject is ILabel) {
          builder = new DefaultLabelOptionBuilder();
        } else if (subject is IPort) {
          builder = new DefaultPortOptionBuilder();
        } else if (subject is INodeStyle) {
          builder = new AttributeBasedOptionBuilder();
        } else if (subject is IEdgeStyle) {
          builder = new AttributeBasedOptionBuilder();
        } else if (subject is ILabelStyle) {
          builder = new AttributeBasedOptionBuilder();
        } else if (subject is IPortStyle) {
          builder = new AttributeBasedOptionBuilder();
        }
      }
      ExtensionConfigureOptionBuilder(builder, subject, type);
      return builder;
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    static partial void ExtensionConfigureOptionBuilder(IOptionBuilder builder, object subject, Type type);

    private void EditProperties() {
      OptionHandler oh = new OptionHandler("SELECTION_PROPERTIES");
      oh.I18nFactory = resourceManagerI18NFactory;
      if (nodePropertyHandler.Count > 0) {
        if (edgePropertyHandler.Count == 0 && labelPropertyHandler.Count == 0 && portPropertyHandler.Count == 0) {
          oh = nodePropertyHandler;
        } else {
          oh.AddOptionItem(nodePropertyHandler);
        }
      }
      if (edgePropertyHandler.Count > 0) {
        if (nodePropertyHandler.Count == 0 && labelPropertyHandler.Count == 0 && portPropertyHandler.Count == 0) {
          oh = edgePropertyHandler;
        } else {
          oh.AddOptionItem(edgePropertyHandler);
        }
      }
      if (labelPropertyHandler.Count > 0) {
        if (edgePropertyHandler.Count == 0 && nodePropertyHandler.Count == 0 && portPropertyHandler.Count == 0) {
          oh = labelPropertyHandler;
        } else {
          oh.AddOptionItem(labelPropertyHandler);
        }
      }
      if (portPropertyHandler.Count > 0) {
        if (edgePropertyHandler.Count == 0 && nodePropertyHandler.Count == 0 && labelPropertyHandler.Count == 0) {
          oh = portPropertyHandler;
        } else {
          oh.AddOptionItem(portPropertyHandler);
        }
      }
      EditorForm form = new EditorForm() { OptionHandler = oh, IsAutoAdopt = true, IsAutoCommit = false };
      form.Owner = this;
      form.ShowDialog();
    }

    #endregion

    void nodeDropInputMode_NodeCreated(object sender, ItemEventArgs<INode> e) {
      graphControl.Graph.SetStyle(e.Item, (INodeStyle)e.Item.Style.Clone());
    }

    void labelDropInputMode_DragDropped(object sender, EventArgs e) {
      // This method is called when an label style is dropped onto the canvas.
      DropInputMode dom = (DropInputMode)sender;
      PointD location = dom.DropLocation;
      object data = dom.DropData;
      if (data is ILabelStyle) {
        // Use the dropped label style for changed/created edges.
        ILabelStyle style = ((ILabelStyle)data);
        // Look for a label at the drop location.
        ILabel editLabel = null;
        GraphModelManager manager = graphControl.GraphModelManager;
        IEnumerable<ILabel> lhits = manager.TypedHitElementsAt<ILabel>(location);
        if (lhits.Any()) {
          // Set the style of the label at the drop location to the dropped style.
          ILabel label = lhits.First();
          graphControl.Graph.SetStyle(label, style);
          graphControl.Graph.AdjustLabelPreferredSize(label);
        } else {
          // Look for an edge at the drop location.
          IEnumerable<IEdge> ehits = manager.TypedHitElementsAt<IEdge>(location);
          IEnumerable<IPort> phits = manager.TypedHitElementsAt<IPort>(location);
          if (ehits.Any()) {
            // Create a label for the edge at the drop location using the dropped style
            IEdge edge = ehits.First();
            editLabel = graphControl.Graph.AddLabel(edge, "Label", graphControl.Graph.CreateDefaultLabelLayoutParameter(edge), style);
          } else if (phits.Any()) {
            // Create a label for the port at the drop location using the dropped style
            var port = phits.First();
            editLabel = graphControl.Graph.AddLabel(port, "Label", graphControl.Graph.CreateDefaultLabelLayoutParameter(port), style);
          } else {
            // Look for a node at the drop location.
            IEnumerable<INode> hits = manager.TypedHitElementsAt<INode>(location);
            INode node;
            if (hits.Any()) {
              node = hits.First();
            } else {
              // If there is no node at the drop location create a new one.
              node = graphControl.Graph.CreateNode(location);
            }
            // Create a label for the node at the drop location using the dropped style
            editLabel = graphControl.Graph.AddLabel(node, "Label",
              graphControl.Graph.CreateDefaultLabelLayoutParameter(node), style);
          }
        }
        graphControl.Focus();
        if (editLabel != null) {
          graphControl.Dispatcher.BeginInvoke(
            new Action(() => GraphCommands.EditLabel.Execute(editLabel, graphControl)));
        }
      }
    }

    #region Utility Methods

    private bool CreateNewDocument() {
      //ask for file save if necessary
      AskForFileSave();
      // clear the whole graph, not only the current view
      var foldingView = GraphControl.Graph.GetFoldingView();
      foldingView.LocalRoot = null;
      foldingView.Manager.MasterGraph.Clear();
      GraphControl.UpdateContentRect();
      LastOpenedFile = "";
      this.Text = appName + " - [New File]";
      UndoEngine engine = Graph.GetUndoEngine();
      if (engine != null) {
        engine.Clear();
        UpdateModificationState(DocumentModified);
      }
      return true;
    }

    private void AskForFileSave()
    {
      if (DocumentModified) {
        var result = MessageBox.Show(this, DefaultStringResources.ExitConfirmation_UnsavedChanges_Text,
                                              DefaultStringResources.ExitConfirmation_UnsavedChanges_Caption,
                                              MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
        switch (result)
        {
          case MessageBoxResult.Yes:
            OnSaveExecuted(this, null);
            break;
          case MessageBoxResult.No:
            break;
        }
      }
    }

    /// <summary>
    /// Change various GUI elements depending on the modification state of the current document
    /// </summary>
    /// <param name="modified"></param>
    private void UpdateModificationState(bool modified) {
      if (modified) {
        // modified since last operation
        if (!Text.EndsWith("*")) {
          Text += '*';
        }
      } else {
        Text = Text.TrimEnd('*');
      }
    }

    private void SetZoomLevel(double level) {
      GraphControl.Zoom = level;
    }

    private string Text {
      get { return Title; }
      set { Title = value; }
    }

    #endregion

    #region GraphML Utility Methods

    /// <summary>
    /// Set up the I/O handler for <c>GraphML</c> import/export
    /// </summary>
    /// <returns></returns>
    [NotNull]
    private static GraphMLIOHandler CreateIOHandler() {
      return new GraphMLIOHandler();
    }
    
    private void WriteGraphML(IGraph g, string filename) {
      IoHandler.Write(g, filename);
      this.Text = appName + " - " + filename;
      UndoEngine engine = Graph.GetUndoEngine();
      if (engine != null) {
        engine.Clear();
        UpdateModificationState(DocumentModified);
      }
    }

    private void ReadGraphML(IGraph g, string filename) {
      IoHandler.Read(g, filename);

      this.Text = appName + " - " + filename;
      UndoEngine engine = Graph.GetUndoEngine();
      if (engine != null) {
        engine.Clear();
        UpdateModificationState(DocumentModified);
      }
      GraphControl.FitGraphBounds();
    }
    
    private void ReadGraphML(IGraph g, Stream stream) {
      IoHandler.Read(g, stream);

      UndoEngine engine = Graph.GetUndoEngine();
      if (engine != null) {
        engine.Clear();
        UpdateModificationState(DocumentModified);
      }
      GraphControl.FitGraphBounds();
    }

    #endregion

    #region Application settings management

    private void SaveApplicationSettings() {
      if (!string.IsNullOrEmpty(LastOpenedFile)) {
        Settings.Default.LastOpenFile = LastOpenedFile;
      }

      Settings.Default.OrthogonalEdgesEnabled = OrthogonalEdgeEditingContext.Enabled;
      Settings.Default.SnappingEnabled = GraphEditorInputMode.SnapContext.Enabled;

      var stringCollection = new StringCollection();
      stringCollection.AddRange(recentDocuments.ToArray());
      Settings.Default.MruFiles = stringCollection;
      Settings.Default.LastState = WindowState;

      if (WindowState == WindowState.Normal) {
        Settings.Default.MainSize = new Size(ActualWidth, ActualHeight);
        Settings.Default.LastLocation = new System.Windows.Point(Left, Top);
      } else {
        Settings.Default.MainSize = RestoreBounds.Size;
        Settings.Default.LastLocation = RestoreBounds.Location;
      }
      MemoryStream str = new MemoryStream();
      GraphMLIOHandler defaultIOH = new GraphMLIOHandler();
      IGraph g = Graph;
      FilteredGraphWrapper fgw = new FilteredGraphWrapper(g, node => false, edge => false);
      ILookupDecorator decorator = g.Lookup(typeof(ILookupDecorator)) as ILookupDecorator;
      IList<IContextLookupChainLink> links = new List<IContextLookupChainLink>();
      if (decorator != null && decorator.CanDecorate(typeof(IGraph))) {
        links.Add(Lookups.HidingLookupChainLink(typeof(IFoldingView)));
        foreach (var contextLookupChainLink in links) {
          decorator.AddLookup(typeof(IGraph), contextLookupChainLink);  
        }
      }

      defaultIOH.Write(fgw, str);
      str.Flush();
      if (decorator != null && decorator.CanDecorate(typeof(IGraph))) {
        foreach (var contextLookupChainLink in links) {
          decorator.RemoveLookup(typeof (IGraph), contextLookupChainLink);
        }
      }
      str.Position = 0;
      XmlReader reader = XmlReader.Create(str);
      XmlDocument doc = new XmlDocument();
      doc.Load(reader);
      Settings.Default.DefaultGraphSettings = doc;
      str.Close();

      Settings.Default.AutoAdjustContentRectangle = GraphEditorInputMode.AdjustContentRectPolicy == AdjustContentRectPolicy.Always;
      Settings.Default.AutomaticallyRemoveEmptyLabels = GraphEditorInputMode.AutoRemoveEmptyLabels;
      Settings.Default.GridWidth = GridWidth;
      Settings.Default.GridVisible = grid.Visible;
      Settings.Default.GridSnapType = GridSnapType;
      Settings.Default.HitTestRadius = (float)GraphControl.HitTestRadius;

      UndoEngine engine = Graph.GetUndoEngine();
      if (engine != null) {
        Settings.Default.UndoSize = engine.Size;
      }
      Settings.Default.Save();
    }

    private void LoadApplicationSettings(bool resetting) {
      //upgrade if necessary
      if (Settings.Default.UpgradeSettings) {
        Settings.Default.Upgrade();
        Settings.Default.UpgradeSettings = false;
      }
      lastOpenedFile = "";
      recentDocuments.Clear();
      if (Settings.Default.MruFiles != null) {
        var enumerable = Settings.Default.MruFiles.Cast<string>();
        foreach (var mruFile in enumerable) {
          recentDocuments.Add(mruFile);
        }
      }
      Width = Settings.Default.MainSize.Width;
      Height = Settings.Default.MainSize.Height;
      Left = Settings.Default.LastLocation.X;
      Top = Settings.Default.LastLocation.Y;
      WindowState = Settings.Default.LastState;
      
      GraphControl.HitTestRadius = Settings.Default.HitTestRadius;
      UndoEngine engine = Graph.GetUndoEngine();
      if (engine != null) {
        engine.Size = Settings.Default.UndoSize;
      }
    }

    #endregion

    #region Graph context menu creation

    private void ConfigureContextMenus(GraphEditorInputMode mode) {
      mode.ContextMenuInputMode.PopulateMenu += ContextMenuInputMode_PopulateMenu;
    }

    /// <summary>
    /// Partial method used as callback for extension packages.
    /// </summary>
    partial void ExtensionPopulateNodeContextMenu(INode node, PopulateMenuEventArgs e);

    private void ContextMenuInputMode_PopulateMenu(object sender, PopulateMenuEventArgs e) {
      e.Menu.Items.Clear();
      if (graphControl.InputMode != GraphEditorInputMode) {
        PopulateViewContextMenu(e.Menu);
        e.ShowMenu = true;
        return;
      }
      if (GraphControl.Selection.Count > 0) {
        INode node = null;
        if (GraphControl.Selection.SelectedNodes.Count > 0) {
          node = GraphControl.Selection.SelectedNodes.First();
        } else if (GraphControl.Selection.SelectedLabels.Count > 0) {
          node = GraphControl.Selection.SelectedLabels.First().Owner as INode;
        }

        ExtensionPopulateNodeContextMenu(node, e);

        // get stripe
        StripeSubregion stripe = tableEditorInputMode.FindStripe(e.QueryLocation, StripeTypes.All, StripeSubregionTypes.Header);
        if (stripe != null) {
          IHitTester<INode> hitTestEnumerator =
          e.Context.Lookup<IHitTester<INode>>();
          if (hitTestEnumerator != null) {
            IEnumerator<INode> enumerator = hitTestEnumerator.EnumerateHits(e.Context, e.QueryLocation).GetEnumerator();
            if (enumerator.MoveNext()) {
              node = enumerator.Current;
            }
          }

          PopulateStripeContextMenu(e.Menu, stripe.Stripe, node);
        }
        PopulateSelectionContextMenu(e.Menu);
        AddPasteLocation(e);
        e.ShowMenu = true;
        return;
      } else {
        //Get hit(s)
        IHitTester<IModelItem> hitTestEnumerator =
          e.Context.Lookup<IHitTester<IModelItem>>();
        if (hitTestEnumerator != null) {
          IEnumerator<IModelItem> enumerator = hitTestEnumerator.EnumerateHits(e.Context, e.QueryLocation).GetEnumerator();
          if (enumerator.MoveNext()) {
            if (enumerator.Current is INode) {
              //populate node menu
              INode node = (INode)enumerator.Current;
              Selection.SetSelected(node, true);

              ExtensionPopulateNodeContextMenu(node, e);

              // get stripe
              StripeSubregion stripe = tableEditorInputMode.FindStripe(e.QueryLocation, StripeTypes.All, StripeSubregionTypes.Header);
              if (stripe != null) {
                PopulateStripeContextMenu(e.Menu, stripe.Stripe, node);
              }

              PopulateNodeContextMenu(e.Menu);
              AddPasteLocation(e);

              e.ShowMenu = true;
              return;
            } else if (enumerator.Current is IEdge) {
              //populate edge menu
              IEdge edge = (IEdge)enumerator.Current;
              Selection.SetSelected(edge, true);
              PopulateEdgeContextMenu(e.Menu);
              AddPasteLocation(e);
              e.ShowMenu = true;
              return;
            } else if (enumerator.Current is ILabel) {
              ILabel label = (ILabel)enumerator.Current;
              Selection.SetSelected(label, true);
              var node = label.Owner as INode;

              ExtensionPopulateNodeContextMenu(node, e);

              PopulateLabelContextMenu(e.Menu);
              AddPasteLocation(e);
              e.ShowMenu = true;
              return;
            }
          } else {
            //show view and paste
            e.Menu.Items.Add(ApplicationCommands.Paste, GraphControl, e.QueryLocation);
            e.Menu.Items.Add(new Separator());
            PopulateViewContextMenu(e.Menu);
            e.ShowMenu = true;
            return;
          }
        }
      }
    }

    private void AddPasteLocation(PopulateMenuEventArgs args) {
      foreach (var item in args.Menu.Items) {
        var menuItem = item as MenuItem;
        if (menuItem != null && menuItem.Command == ApplicationCommands.Paste) {
          menuItem.CommandParameter = args.QueryLocation;
          break;
        }
      }
    }

    private void PopulateStripeContextMenu(ContextMenu contextMenu, IStripe stripe, INode tableOwner) {
      // add the insert before menu item
      var insertBeforeItem = new MenuItem { Header = "Insert new lane before " + stripe };
      insertBeforeItem.Click += delegate {
        IStripe parent = stripe.GetParentStripe();
        int index = stripe.GetIndex();
        tableEditorInputMode.InsertChild(parent, index);
      };
      contextMenu.Items.Add(insertBeforeItem);
      // add the insert after menu item
      var insertAfterItem = new MenuItem { Header = "Insert new lane after " + stripe };
      insertAfterItem.Click += delegate {
        IStripe parent = stripe.GetParentStripe();
        int index = stripe.GetIndex();
        tableEditorInputMode.InsertChild(parent, index + 1);
      };
      contextMenu.Items.Add(insertAfterItem);

      // add the delete menu item if it isn't the only row/column in the table

      var onlyStripe = false;
      if (stripe is IColumn) {
        var columns = stripe.Table.RootColumn.ChildColumns;
        if (columns.Count() == 1) {
          var firstColumn = columns.First();
          onlyStripe = stripe.Equals(firstColumn) && !firstColumn.ChildColumns.Any();
        }
      } else {
        var rows = stripe.Table.RootRow.ChildRows;
        if (rows.Count() == 1) {
          var firstRow = rows.First();
          onlyStripe = stripe.Equals(firstRow) && !firstRow.ChildRows.Any();
        }
      }

      if (!onlyStripe) {
        var deleteItem = new MenuItem { Header = "Delete lane" };
        deleteItem.Click += delegate { tableEditorInputMode.DeleteStripe(stripe); };
        contextMenu.Items.Add(deleteItem);
      }

      if (tableOwner != null) {
        var insets = stripe.Insets;
        var defaultInsets = stripe is IColumn ? stripe.Table.ColumnDefaults.Insets : stripe.Table.RowDefaults.Insets;

        if (stripe is IColumn) {
          if (insets.Top > defaultInsets.Top) {
            var reduceHeader = new MenuItem { Header = "Reduce header size" };
            reduceHeader.Click += delegate {
              var insetsBefore = stripe.Table.GetAccumulatedInsets();
              stripe.Table.SetStripeInsets(stripe,
                new InsetsD(insets.Left, insets.Top - defaultInsets.Top, insets.Right, insets.Bottom));
              var insetsAfter = stripe.Table.GetAccumulatedInsets();
              var diff = insetsBefore.Top - insetsAfter.Top;
              GraphControl.Graph.SetNodeLayout(tableOwner, new RectD(tableOwner.Layout.X, tableOwner.Layout.Y + diff, tableOwner.Layout.Width, tableOwner.Layout.Height - diff));
            };
            contextMenu.Items.Add(reduceHeader);
          }
          var increaseHeader = new MenuItem { Header = "Increase header size" };
          increaseHeader.Click +=
            delegate {
              var insetsBefore = stripe.Table.GetAccumulatedInsets();
              stripe.Table.SetStripeInsets(stripe,
                new InsetsD(insets.Left, insets.Top + defaultInsets.Top, insets.Right, insets.Bottom));
              var insetsAfter = stripe.Table.GetAccumulatedInsets();
              var diff = insetsBefore.Top - insetsAfter.Top;
              GraphControl.Graph.SetNodeLayout(tableOwner, new RectD(tableOwner.Layout.X, tableOwner.Layout.Y + diff, tableOwner.Layout.Width, tableOwner.Layout.Height - diff));
            };
          contextMenu.Items.Add(increaseHeader);
        } else {
          if (insets.Left > defaultInsets.Left) {
            var reduceHeader = new MenuItem { Header = "Reduce header size" };
            reduceHeader.Click += delegate {
              var insetsBefore = stripe.Table.GetAccumulatedInsets();
              stripe.Table.SetStripeInsets(stripe,
                new InsetsD(insets.Left - defaultInsets.Left, insets.Top, insets.Right, insets.Bottom));
              var insetsAfter = stripe.Table.GetAccumulatedInsets();
              var diff = insetsBefore.Left - insetsAfter.Left;
              GraphControl.Graph.SetNodeLayout(tableOwner, new RectD(tableOwner.Layout.X + diff, tableOwner.Layout.Y, tableOwner.Layout.Width - diff, tableOwner.Layout.Height));
            };
            contextMenu.Items.Add(reduceHeader);
          }
          var increaseHeader = new MenuItem { Header = "Increase header size" };
          increaseHeader.Click +=
            delegate {
              var insetsBefore = stripe.Table.GetAccumulatedInsets();
              stripe.Table.SetStripeInsets(stripe,
                new InsetsD(insets.Left + defaultInsets.Left, insets.Top, insets.Right, insets.Bottom));
              var insetsAfter = stripe.Table.GetAccumulatedInsets();
              var diff = insetsBefore.Left - insetsAfter.Left;
              GraphControl.Graph.SetNodeLayout(tableOwner, new RectD(tableOwner.Layout.X + diff, tableOwner.Layout.Y, tableOwner.Layout.Width - diff, tableOwner.Layout.Height));
            };
          contextMenu.Items.Add(increaseHeader);
        }
      }
      contextMenu.Items.Add(new Separator());
    }

    private ICommand[] editCommands = new ICommand[]
                                        {
                                               ApplicationCommands.Undo, 
                                               ApplicationCommands.Redo, 
                                               null,
                                               ApplicationCommands.Cut,
                                               ApplicationCommands.Copy,
                                               ApplicationCommands.Paste,
                                               ApplicationCommands.Delete,
                                               null,
                                               GraphCommands.DeselectAll,
                                               null,
                                               ApplicationCommands.Properties,
  };

    private ICommand[] hierarchyCommands = new ICommand[]
                                        {
                                               GraphCommands.GroupSelection, 
                                               GraphCommands.UngroupSelection,
                                               null,
                                               GraphCommands.AdjustGroupNodeSize, 
                                               null,
                                               GraphCommands.ExpandGroup, 
                                               GraphCommands.CollapseGroup, 
                                               null,
                                               GraphCommands.EnterGroup,
                                               GraphCommands.ExitGroup,
  };
    private ICommand[] viewCommands = new ICommand[]
                                        {
                                               NavigationCommands.IncreaseZoom, 
                                               NavigationCommands.DecreaseZoom, 
                                               GraphControl.FitGraphBoundsCommand, 
                                               null,
                                               GraphCommands.ExitGroup,
  };

    private Window helpWindow;
    private Window overview;
    private Window hierarchy;
    private bool moduleRunning;

    private void PopulateNodeContextMenu(ContextMenu contextMenu) {
      contextMenu.Items.Add(GraphCommands.EditLabel, GraphControl);
      contextMenu.Items.Add(GraphCommands.AddLabel, GraphControl);
      contextMenu.Items.Add(new Separator());
      CopyItems(contextMenu, editCommands);
      contextMenu.Items.Add(new Separator());
      CopyItems(contextMenu, hierarchyCommands);
    }

    private void PopulateEdgeContextMenu(ContextMenu contextMenu) {
      contextMenu.Items.Add(GraphCommands.EditLabel, GraphControl);
      contextMenu.Items.Add(new Separator());
      CopyItems(contextMenu, editCommands);
    }

    private void PopulateLabelContextMenu(ContextMenu contextMenu) {
      contextMenu.Items.Add(GraphCommands.EditLabel, GraphControl);
      contextMenu.Items.Add(ApplicationCommands.Properties, GraphControl);
      contextMenu.Items.Add(ApplicationCommands.Delete, GraphControl);
    }

    private void PopulateViewContextMenu(ContextMenu contextMenu) {
      CopyItems(contextMenu, viewCommands);
    }

    private void PopulateSelectionContextMenu(ContextMenu contextMenu) {
      CopyItems(contextMenu, editCommands);
      contextMenu.Items.Add(new Separator());
      CopyItems(contextMenu, hierarchyCommands);
    }

    private void CopyItems(ContextMenu menu, IEnumerable<ICommand> commands) {
      foreach (var command in commands) {
        if (command != null) {
          menu.Items.Add(new MenuItem() { Command = command, CommandTarget = graphControl});
        } else {
          menu.Items.Add(new Separator());
        }
      }
    }

    #endregion

    #region Style context menu related methods
    
    private void SetDefaultStyle(object style) {
      if (style is INodeStyle) {
        Graph.NodeDefaults.Style = (INodeStyle)style;
      } else if (style is IEdgeStyle) {
        Graph.EdgeDefaults.Style = (IEdgeStyle)style;
      } 
    }

    private void SetDefaultGroupNodeStyle(INodeStyle style) {
      // if style is group node style, set it as default for the grouped graph
      Graph.GroupNodeDefaults.Style = style;
    }

    private void SetDefaultSize(SizeD size) {
      graphControl.Graph.NodeDefaults.Size = size;
    }

    private void ApplyStyleToSelection(object style)
    {
      if (style is INodeStyle) {
        foreach(var node in GraphControl.Selection.SelectedNodes)
        {
          GraphControl.Graph.SetStyle(node, (INodeStyle)style);
        }
      } else if (style is IEdgeStyle) {
        foreach (var edge in GraphControl.Selection.SelectedEdges) {
          GraphControl.Graph.SetStyle(edge, GraphControl.Graph.EdgeDefaults.GetStyleInstance());
        }
      } else if (style is ILabelStyle) {
        foreach (var label in GraphControl.Selection.SelectedLabels) {
          GraphControl.Graph.SetStyle(label, (ILabelStyle)style);
        }
      } else if (style is IPortStyle) {
        foreach (var port in GraphControl.Selection.SelectedPorts) {
          GraphControl.Graph.SetStyle(port, (IPortStyle)style);
        }
      } 
    }
    #endregion

    private void RecentDocumentMenuClick(object sender, RoutedEventArgs e) {
      var menuItem = e.OriginalSource as MenuItem;
      if (menuItem != null && menuItem.DataContext is string) {
        Load((string) menuItem.DataContext);
      }
    }

    private void ZoomMenuClicked(object sender, RoutedEventArgs e) {
      int percent = (int) ((MenuItem) e.OriginalSource).DataContext;
      graphControl.Zoom = percent/100.0d;
    }

    private void SampleFileMenuClickHandler(object sender, RoutedEventArgs e) {
      if (e.OriginalSource is HeaderedItemsControl && ((HeaderedItemsControl)e.OriginalSource).Header is string) {
        SampleFilesCommand.Execute(((HeaderedItemsControl)e.OriginalSource).Header as string, graphControl);
        e.Handled = true;
      }
    }

    private void OnRunModuleExecuted(object sender, ExecutedRoutedEventArgs e) {
      var module = e.Parameter as YModule;
      if (module != null) {
        StartModule(module);
      }
    }

    private void OnCanRunModuleExecuted(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = e.Parameter is YModule;
      e.Handled = true;
    }

    private void OnHelpExecuted(object sender, ExecutedRoutedEventArgs e) {
      if (this.helpWindow == null) {
        var dataTemplate = FindResource("HelpToolWindow") as DataTemplate;
        var window = dataTemplate.LoadContent() as Window;
        window.Owner = this;
        window.Closed += delegate(object o, EventArgs args) {
                           this.helpWindow = null;
                         };
        window.Show();
        this.helpWindow = window;
      } else {
        this.helpWindow.Focus();
      }
    }
    
    private void OnShowOverview(object sender, RoutedEventArgs e) {
      if (this.overview == null) {
        var dataTemplate = FindResource("OverviewToolWindow") as DataTemplate;
        var window = dataTemplate.LoadContent() as Window;
        window.DataContext = graphControl; 
        window.Owner = this;
        window.FlowDirection = graphControl.FlowDirection;
        window.Closed += delegate {
          this.overview = null;
        };
        window.Show();
        this.overview = window;
      } else {
        this.overview.Focus();
      }
    }

    private void OnCanApplyStyleExecuted(object sender, CanExecuteRoutedEventArgs e) {
      var parameter = e.Parameter;
      if (parameter is INode) {
        e.CanExecute = graphControl.Selection.SelectedNodes.Count > 0;
        e.Handled = true;
        return;
      } else if (parameter is IEdgeStyle) {
        e.CanExecute = graphControl.Selection.SelectedEdges.Count > 0;
        e.Handled = true;
        return;
      } else if (parameter is ILabelStyle) {
        e.CanExecute = graphControl.Selection.SelectedLabels.Count > 0;
        e.Handled = true;
        return;
      }
      e.CanExecute = false;
      e.Handled = true;
    }

    private void OnApplyStyleExecuted(object sender, ExecutedRoutedEventArgs e) {
      var parameter = e.Parameter;
      if (parameter is INode) {
        // check if parameter is group node
        if (IsGroupNode((INode)parameter)) {
          SetDefaultGroupNodeStyle(((INode)parameter).Style);
        } else {
          SetDefaultStyle(((INode)parameter).Style);
          SetDefaultSize(((INode)parameter).Layout.ToSizeD());
        }
        ApplyStyleToSelection(((INode)parameter).Style);
        e.Handled = true;
        return;
      } else if (parameter is IEdgeStyle) {
        SetDefaultStyle((IEdgeStyle)parameter);
        ApplyStyleToSelection((IEdgeStyle)parameter);
        e.Handled = true;
        return;
      } else if (parameter is ILabelStyle) {
        SetDefaultStyle((ILabelStyle)parameter);
        ApplyStyleToSelection((ILabelStyle)parameter);
        e.Handled = true;
        return;
      } else if (parameter is IPortStyle) {
        SetDefaultStyle((IPortStyle)parameter);
        ApplyStyleToSelection((IPortStyle)parameter);
        e.Handled = true;
        return;
      }
      e.Handled = false;
    }

    private void OnSetStyleDefaultExecuted(object sender, ExecutedRoutedEventArgs e) {
      var parameter = e.Parameter;
      if (parameter is INode) {
        // check if parameter is group node
        if (IsGroupNode((INode)parameter)) {
          SetDefaultGroupNodeStyle(((INode)parameter).Style);
        }
        else {
          SetDefaultStyle(((INode)parameter).Style);
          SetDefaultSize(((INode)parameter).Layout.ToSizeD());
        }
        e.Handled = true;
        return;
      } else if (parameter is IEdgeStyle) {
        SetDefaultStyle((IEdgeStyle)parameter);
        e.Handled = true;
        return;
      } else if (parameter is ILabelStyle) {
        SetDefaultStyle((ILabelStyle)parameter);
        e.Handled = true;
        return;
      } else if (parameter is IPortStyle) {
        SetDefaultStyle((IPortStyle)parameter);
        e.Handled = true;
        return;
      }
      e.Handled = false;
    }

    private void OnPrintPreviewExecuted(object sender, ExecutedRoutedEventArgs e) {
      // reuse the Print demo's window...
      var printingWindow = new Printing.PrintingWindow()
      {
        Owner = this,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Icon = this.Icon,
        ShowInTaskbar = false,
        Title = "Print Graph",
        FlowDirection = graphControl.FlowDirection,
      };
      // and the prepared functionality to export the contents of the current graphControl
      printingWindow.Loaded += (o, args) => printingWindow.ShowGraph(graphControl);
      printingWindow.ShowDialog();
    }
  }
}
