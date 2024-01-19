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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Organic;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.Tree;
using Scope = yWorks.Layout.Router.Scope;

namespace Demo.yFiles.Graph.NodeTypes
{
  /// <summary>
  /// A demo that shows how different layout algorithms handle nodes with types.
  /// </summary>
  public partial class NodeTypesDemo
  {
    // node visualizations for the different node types
    private static readonly ShapeNodeStyle[] nodeStyles = {
        new ShapeNodeStyle {
            Shape = ShapeNodeShape.RoundRectangle,
            Brush = new SolidColorBrush(Color.FromRgb(0x17, 0xbe, 0xbb)),
            Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x40, 0x72, 0x71)), 1.5)
        },
        new ShapeNodeStyle {
            Shape = ShapeNodeShape.RoundRectangle,
            Brush = new SolidColorBrush(Color.FromRgb(0xff, 0xc9, 0x14)),
            Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x99, 0x89, 0x53)), 1.5)
        },
        new ShapeNodeStyle {
            Shape = ShapeNodeShape.RoundRectangle,
            Brush = new SolidColorBrush(Color.FromRgb(0xff, 0x6c, 0x00)),
            Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)), 1.5)
        }
    };

    // edge visualizations for directed and undirected edges
    private static readonly IEdgeStyle directedEdgeStyle = DemoStyles.CreateDemoEdgeStyle();
    private static readonly IEdgeStyle undirectedEdgeStyle = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: false);

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    private GraphControl GraphControl {
      get { return graphControl; }
    }

    #region Initialization

    /// <summary>
    /// Wires up the UI components and initializes the layout options.
    /// </summary>
    public NodeTypesDemo() {
      InitializeComponent();

      SampleGraphComboBox.ItemsSource = new[] {
          CreateHierarchicSample(), 
          CreateOrganicSample(), 
          CreateTreeSample(), 
          CreateCircularSample(),
          CreateComponentSample()
      };
    }

    /// <summary>
    /// Initializes the graph and the input mode.
    /// </summary>
    private void OnLoad(object sender, EventArgs e) {
      InitializeInputModes();
      InitializeGraph(GraphControl.Graph);
      SampleGraphComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Initializes the edit mode and the context menu.
    /// </summary>
    private void InitializeInputModes() {
      var editMode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge,
          ContextMenuItems = GraphItemTypes.Node,
          NodeCreator = (context, graph, location, parent) => {
            var node = graph.CreateNode(location);
            node.Tag = 0;
            graph.SetStyle(node, GetNodeStyle(node));
            return node;
          }
      };

      editMode.PopulateItemContextMenu += OnPopulateItemContextMenu;
      
      GraphControl.InputMode = editMode;
    }

    /// <summary>
    /// Opens a context menu to change the type of a node.
    /// </summary>
    private void OnPopulateItemContextMenu(object sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      if (args.Item is INode) {
        // Select node if not already selected
        if (!graphControl.Selection.IsSelected(args.Item)) {
          graphControl.Selection.Clear();
          graphControl.Selection.SetSelected(args.Item, true);
        }

        for (int type = 0; type < nodeStyles.Length; type++) {
          var nodeStyle = nodeStyles[type];
          var menuItem = new MenuItem
          {
            Header = new Rectangle
            {
              Fill = nodeStyle.Brush,
              Width = 50,
              Height = 20,
              Stroke = nodeStyle.Pen.Brush,
              Margin = new Thickness(5)
            }
          };
          var type1 = type;
          menuItem.Click += async (o, eventArgs) => {
            foreach (var node in graphControl.Selection.SelectedNodes) {
              node.Tag = type1;
              GraphControl.Graph.SetStyle(node, nodeStyle);
            }
            await ApplyLayout(true);
          };
          args.Menu.Items.Add(menuItem);
        }
      }
    }

    /// <summary>
    /// Configures the defaults for the graph.
    /// </summary>
    private void InitializeGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Size = new SizeD(40, 40);
      
      graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    /// Loads a sample graph and adapts the visualization of the nodes to their types and of the
    /// edges to their directions.
    /// </summary>
    private async Task LoadSample() {
      var selectedSample = (Sample) SampleGraphComboBox.SelectedItem;
      GraphControl.ImportFromGraphML("Resources\\" + selectedSample.File + ".graphml");

      var graph = GraphControl.Graph;
      foreach (var node in graph.Nodes) {
        graph.SetStyle(node, GetNodeStyle(node));
      }
      foreach (var edge in graph.Edges) {
        graph.SetStyle(edge, GetEdgeStyle(selectedSample.IsDirected));
      }
      
      await ApplyLayout(false);
      await GraphControl.FitGraphBounds();
    }
    
    #endregion

    #region Node type handling

    /// <summary>
    /// Gets the type of the given node from its tag.
    /// </summary>
    private int GetNodeType(INode node) {
      // The implementation of this demo assumes that on the INode.Tag a type property
      // (a number) exists. Note though that for the layout's node type feature arbitrary objects
      // from arbitrary sources may be used. 
      return node.Tag is int ? (int) node.Tag : 0;
    }

    /// <summary>
    /// Determines the visualization of a node based on its type.
    /// </summary>
    private ShapeNodeStyle GetNodeStyle(INode node) {
      var type = GetNodeType(node);
      return nodeStyles[type];
    }

    /// <summary>
    /// Determines the visualization of an edge based on its direction.
    /// </summary>
    private IEdgeStyle GetEdgeStyle(bool directed) {
      return directed ? directedEdgeStyle : undirectedEdgeStyle;
    }
    
    #endregion

    #region Sample creation

    /// <summary>
    /// Creates and configures the <see cref="HierarchicLayout"/> and the <see cref="HierarchicLayoutData"/>
    /// such that node types are considered.
    /// </summary>
    private Sample CreateHierarchicSample() {
      // create hierarchic layout - no further settings on the algorithm necessary to support types
      var layout = new HierarchicLayout();

      // the node types are specified as delegate on the nodeTypes property of the layout data
      var layoutData = new HierarchicLayoutData { NodeTypes = { Delegate = node => GetNodeType(node) } };

      return new Sample {
          Name = "Hierarchic",
          File = "hierarchic",
          Layout = layout,
          LayoutData = layoutData,
          IsDirected = true
      };
    }

    /// <summary>
    /// Creates and configures the <see cref="OrganicLayout"/> and the <see cref="OrganicLayoutData"/>
    /// such that node types are considered.
    /// </summary>
    private Sample CreateOrganicSample() {
      // create an organic layout wrapped by an organic edge router
      var layout = new OrganicEdgeRouter(
          // to consider node types, substructures handling (stars, parallel structures and cycles)
          // on the organic layout is enabled - otherwise types have no influence
          new OrganicLayout {
              Deterministic = true,
              ConsiderNodeSizes = true,
              MinimumNodeDistance = 30,
              StarSubstructureStyle = yWorks.Layout.Organic.StarSubstructureStyle.Circular,
              StarSubstructureTypeSeparation = false,
              ParallelSubstructureStyle = ParallelSubstructureStyle.Rectangular,
              ParallelSubstructureTypeSeparation = false,
              CycleSubstructureStyle = CycleSubstructureStyle.Circular
          });

      // the node types are specified as delegate on the nodeTypes property of the layout data
      var layoutData = new OrganicLayoutData { NodeTypes = { Delegate = node => GetNodeType(node) } };

      return new Sample {
          Name = "Organic",
          File = "organic",
          Layout = layout,
          LayoutData = layoutData,
          IsDirected = false
      };
    }

    /// <summary>
    /// Creates and configures the <see cref="TreeLayout"/> and the <see cref="TreeLayoutData"/>
    /// such that node types are considered.
    /// </summary>
    private Sample CreateTreeSample() {
      // create a tree layout including a reduction stage to support non-tree graphs too
      var layout = new TreeLayout { DefaultNodePlacer = new CompactNodePlacer() };
      var edgeRouter = new EdgeRouter { Scope = Scope.RouteAffectedEdges };
      var reductionStage = new TreeReductionStage {
          NonTreeEdgeRouter = edgeRouter, NonTreeEdgeSelectionKey = edgeRouter.AffectedEdgesDpKey
      };
      layout.PrependStage(reductionStage);

      // the node types are specified as delegate on the nodeTypes property of the layout data
      var layoutData = new TreeLayoutData { NodeTypes = { Delegate = node => GetNodeType(node) } };

      return new Sample {
          Name = "Tree",
          File = "tree",
          Layout = layout,
          LayoutData = layoutData,
          IsDirected = true
      };
    }

    /// <summary>
    /// Creates and configures the <see cref="CircularLayout"/> and the <see cref="CircularLayoutData"/>
    /// such that node types are considered.
    /// </summary>
    private Sample CreateCircularSample() {
      // create a circular layout and specify the NodeTypeAwareSequencer as sequencer responsible
      // for the ordering on the circle - this is necessary to support node types
      var layout = new CircularLayout { SingleCycleLayout = { NodeSequencer = new NodeTypeAwareSequencer() } };

      // the node types are specified as delegate on the nodeTypes property of the layout data
      var layoutData = new CircularLayoutData { NodeTypes = { Delegate = node => GetNodeType(node) } };

      return new Sample {
          Name = "Circular",
          File = "circular",
          Layout = layout,
          LayoutData = layoutData,
          IsDirected = false
      };
    }

    /// <summary>
    /// Creates and configures the <see cref="ComponentLayout"/> and the <see cref="ComponentLayoutData"/>
    /// such that node types are considered.
    /// </summary>
    private Sample CreateComponentSample() {
      // create a component layout with default settings
      var layout = new ComponentLayout();

      // note that with the default component arrangement style the types of nodes have an influence
      // already - however, if in a row only components with nodes of the same type should be
      // allowed, this can be achieved by specifying the style as follows:
      // layout.Style = ComponentArrangementStyles.MultiRowsTypeSeparated

      // the node types are specified as delegate on the nodeTypes property of the layout data
      var layoutData = new ComponentLayoutData { NodeTypes = { Delegate = node => GetNodeType(node) } };

      return new Sample {
          Name = "Component",
          File = "component",
          Layout = layout,
          LayoutData = layoutData,
          IsDirected = false
      };
    }
    
    #endregion

    #region Layout calculation

    /// <summary>
    /// Calculates a layout taking the node types into account.
    /// </summary>
    private async Task ApplyLayout(bool animate) {
      var sample = (Sample) SampleGraphComboBox.SelectedItem;
      var considerTypes = ConsiderTypes.IsChecked == true;

      var layout = sample.Layout;
      var layoutData = considerTypes ? sample.LayoutData : null;

      var layoutExecutor = new LayoutExecutor(graphControl, layout)
      {
        LayoutData = layoutData,
        Duration = animate ? TimeSpan.FromMilliseconds(700) : TimeSpan.Zero,
        PortAdjustmentPolicy = PortAdjustmentPolicy.Always,
        RunInThread = false,
        AnimateViewport = true
      };

      EnableUi(false);
      await layoutExecutor.Start();
      EnableUi(true);
    }

    #endregion

    #region User interface event handling

    /// <summary>
    /// Loads a new sample.
    /// </summary>
    private async void OnSampleChanged(object sender, SelectionChangedEventArgs e) {
      await LoadSample();
    }

    /// <summary>
    /// Loads the previous sample.
    /// </summary>
    private void LoadPreviousSampleGraph(object sender, RoutedEventArgs e) {
      var index = SampleGraphComboBox.SelectedIndex;
      int sampleCount = SampleGraphComboBox.Items.Count;
      int newIndex = index > 0 ? index - 1 : sampleCount - 1;
      SampleGraphComboBox.SelectedIndex = newIndex;
    }

    /// <summary>
    /// Loads the next sample.
    /// </summary>
    private void LoadNextSampleGraph(object sender, RoutedEventArgs e) {
      var index = SampleGraphComboBox.SelectedIndex;
      int sampleCount = SampleGraphComboBox.Items.Count;
      int newIndex = index < sampleCount - 1 ? index + 1 : 0;
      SampleGraphComboBox.SelectedIndex = newIndex;
    }

    /// <summary>
    /// Invokes the layout calculation.
    /// </summary>
    private async void OnLayoutClicked(object sender, RoutedEventArgs e) {
      await ApplyLayout(true);
    }

    /// <summary>
    /// Invokes the layout calculation.
    /// </summary>
    private async void OnConsiderTypesClicked(object sender, RoutedEventArgs e) {
      await ApplyLayout(true);
    }

    /// <summary>
    /// Activates or deactivates the UI elements for changing the sample.
    /// </summary>
    private void EnableUi(bool enable) {
      LayoutButton.IsEnabled = enable;
      SampleGraphComboBox.IsEnabled = enable;
      PreviousSampleButton.IsEnabled = enable;
      NextSampleButton.IsEnabled = enable;
      ConsiderTypes.IsEnabled = enable;
    }

    #endregion

    /// <summary>
    /// Contains all information about a sample.
    /// </summary>
    sealed class Sample
    {
      public string Name { get; set; }

      public string File { get; set; }

      public ILayoutAlgorithm Layout { get; set; }

      public LayoutData LayoutData { get; set; }

      public bool IsDirected { get; set; }

      public override string ToString() {
        return Name;
      }
    }
  }
}
