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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Demo.Base.RandomGraphGenerator;
using Demo.yFiles.Toolkit;
using yWorks.Analysis;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;

namespace Demo.yFiles.Algorithms.ShortestPath
{
  /// <summary>
  /// Demonstrates usage of a yFiles algorithm on an IGraph.
  /// </summary>
  /// <remarks>
  /// This demo dynamically calculates the set of edges that lie on shortest paths
  /// between two sets of nodes (sources and targets).
  /// Geometric edge lengths are used for the weight of the edges, unless there are numeric 
  /// labels attached to them in which case the value of the label text is used. 
  /// Note that negative edge weights will be interpreted as 0.
  /// </remarks>
  public partial class ShortestPathWindow
  {
    #region Fields

    // holds all available layout algorithms by name for the combo box
    private readonly IDictionary<String, ILayoutAlgorithm> layouts = new Dictionary<string, ILayoutAlgorithm>();
    // holds the currently chosen layout algorithm
    private ILayoutAlgorithm currentLayout;

    // the styles to use for source nodes, target nodes, and ordinary nodes
    private INodeStyle defaultNodeStyle, targetNodeStyle, sourceNodeStyle, sourceAndTargetNodeStyle;
    // the style to use for ordinary edges and edges that lie on a shortest path
    private PolylineEdgeStyle defaultEdgeStyle, pathEdgeStyle;
    // the current source nodes
    private List<INode> sourceNodes;
    // the current target nodes
    private List<INode> targetNodes;
    
    // whether to use directed path calculation
    private bool directed;
    
    // for creating sample graphs
    private readonly RandomGraphGenerator randomGraphGenerator;
    
    // the set of the edges that are currently part of the path
    private IEnumerable<IEdge> pathEdges = new HashSet<IEdge>();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortestPathWindow"/> class.
    /// </summary>
    public ShortestPathWindow() {
      InitializeComponent();

      sourceNodes = new List<INode>();
      targetNodes = new List<INode>();

      InitializeInputModes();
      InitializeStyles();
      InitializeGraph();

      // initialize random graph generator
      randomGraphGenerator = new RandomGraphGenerator
                               {
                                 AllowCycles = true,
                                 AllowMultipleEdges = true,
                                 AllowSelfLoops = false,
                                 EdgeCount = 40,
                                 NodeCount = 30
                               };
    }

    #endregion

    #region Initialization

    private void InitializeInputModes() {
      // build an input mode and register for the various events
      // that could change the shortest path calculation
      GraphEditorInputMode editMode = new GraphEditorInputMode();
      // deletion
      editMode.DeletedSelection += async (o, eventArgs) => await CalculateShortestPath();
      // edge creation
      editMode.CreateEdgeInputMode.EdgeCreated += async (sender, args) => await CalculateShortestPath();
      // movement of items
      editMode.MoveInputMode.DragFinished += async (sender, args) => await CalculateShortestPath();
      // resizing of items
      editMode.HandleInputMode.DragFinished += async (sender, args) => await CalculateShortestPath();
      // adding or changing labels
      editMode.LabelAdded += async (sender, args) => await CalculateShortestPath();
      editMode.LabelTextChanged += async (sender, args) => await CalculateShortestPath();

      // allow only numeric label texts
      editMode.ValidateLabelText += EditModeOnValidateLabelText;

      // also prepare a context menu
      editMode.PopulateItemContextMenu += PopulateNodeContextMenu;

      // show weight tooltips
      editMode.ToolTipItems = GraphItemTypes.Edge;
      editMode.QueryItemToolTip += EditModeOnQueryItemToolTip;

      // when the WaitInputMode kicks in, disable buttons which might cause
      // inconsistencies when performed while a layout calculation is running
      editMode.WaitInputMode.WaitingStarted += (sender, args) => {
        this.newGraphButton.IsEnabled = false;
        this.layoutComboBox.IsEnabled = false;
        this.applyLayoutButton.IsEnabled = false;
      };
      editMode.WaitInputMode.WaitingEnded += (sender, args) => {
        this.newGraphButton.IsEnabled = true;
        this.layoutComboBox.IsEnabled = true;
        this.applyLayoutButton.IsEnabled = true;
      };

      graphControl.InputMode = editMode;
    }

    // show the weight of the edge as a tooltip
    private void EditModeOnQueryItemToolTip(object sender, QueryItemToolTipEventArgs<IModelItem> queryItemToolTipEventArgs) {
      var edge = queryItemToolTipEventArgs.Item as IEdge;
      if (edge != null) {
        queryItemToolTipEventArgs.ToolTip = "Weight = " + GetEdgeCost(edge).ToString("F");
      }
    }

    // allow only empty labels (for deletion) and labels containing a floating point value
    private void EditModeOnValidateLabelText(object sender, LabelTextValidatingEventArgs labelTextValidatingEventArgs) {
      labelTextValidatingEventArgs.NewText = labelTextValidatingEventArgs.NewText.Trim();
      if (labelTextValidatingEventArgs.NewText.Length == 0) {
        return;
      }
      double result;
      if (!(Double.TryParse(labelTextValidatingEventArgs.NewText, NumberStyles.Float, Thread.CurrentThread.CurrentUICulture, out result) && result >= 0)) {
        labelTextValidatingEventArgs.Cancel = true;
      }
    }

    /// <summary>
    /// Populates the context menu for nodes.
    /// </summary>
    private void PopulateNodeContextMenu(object sender, PopulateItemContextMenuEventArgs<IModelItem> e) {
      var node = e.Item as INode;
      var selection = graphControl.Selection.SelectedNodes;
      if (node != null) {
        if (!selection.IsSelected(node)) {
          selection.Clear();
          selection.SetSelected(node, true);
          graphControl.CurrentItem = node;
        }
      }
      if (selection.Count > 0) {
        var markAsSourceItem = new MenuItem {Header = "Mark as Source"};
        markAsSourceItem.Click += async (o, args) => await MarkAsSource(selection.ToList());
        e.Menu.Items.Add(markAsSourceItem);
        var markAsTargetItem = new MenuItem {Header = "Mark as Target"};
        markAsTargetItem.Click += async (o, args) => await MarkAsTarget(selection.ToList());
        e.Menu.Items.Add(markAsTargetItem);
        // check if one or more of the selected nodes are already marked as source or target
        bool marked = false;
        foreach (INode n in selection) {
          if (sourceNodes.Contains(n) || targetNodes.Contains(n)) {
            marked = true;
            break;
          }
        }
        if (marked) {
          // add the 'Remove Mark' item
          var removeMarkItem = new MenuItem { Header = "Remove Mark" };
          removeMarkItem.Click += async delegate {
              List<INode> sn = sourceNodes.ToList();
              sn.RemoveAll((node1 => selection.IsSelected(node1)));
              await MarkAsSource(sn);
              List<INode> tn = targetNodes.ToList();
              tn.RemoveAll((node1 => selection.IsSelected(node1)));
              await MarkAsTarget(tn);
            };
          e.Menu.Items.Add(removeMarkItem);
        }
      }
      e.Handled = true;
    }

    /// <summary>
    /// Initializes the styles to use for the graph.
    /// </summary>
    private void InitializeStyles() {
      defaultNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteOrange);
      sourceNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteGreen);
      targetNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteRed);
      sourceAndTargetNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteBlue);
      defaultEdgeStyle = DemoStyles.CreateDemoEdgeStyle();
      pathEdgeStyle = new PolylineEdgeStyle { Pen = new Pen(Brushes.Red, 4.0f) };
    }

    /// <summary>
    /// Initializes the graph defaults.
    /// </summary>
    private void InitializeGraph() {
      graphControl.Graph.NodeDefaults.Style = defaultNodeStyle;
      graphControl.Graph.NodeDefaults.Size = new SizeD(30, 30);
      graphControl.Graph.EdgeDefaults.Style = defaultEdgeStyle;
      graphControl.Graph.EdgeDefaults.Labels.Style = DemoStyles.CreateDemoEdgeLabelStyle();
    }

    private async void OnLoaded(object sender, EventArgs e) {
      PopulateLayoutComboBox();
      await GenerateGraph();

      directedComboBox.SelectedIndex = 1;
      layoutComboBox.SelectedIndex = 1;
      currentLayout = layouts[(string) layoutComboBox.SelectedItem];
    }

    #endregion

    #region Menu Handlers

    private async void MarkSourceMenuItem_Click(object sender, EventArgs e) {
      await MarkAsSource(graphControl.Selection.SelectedNodes.ToList());
    }

    private async void MarkTargetMenuItem_Click(object sender, EventArgs e) {
      await MarkAsTarget(graphControl.Selection.SelectedNodes.ToList());
    }

    private async void directedComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      directed = directedComboBox.SelectedIndex == 0 ? true : false;
      UpdateArrow(defaultEdgeStyle, directed);
      UpdateArrow(pathEdgeStyle, directed);
      await CalculateShortestPath();
    }

    private static void UpdateArrow(PolylineEdgeStyle style, bool directed) {
      if (directed) {
        var demoArrow = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: true).TargetArrow as Arrow;
        var prototype = demoArrow ?? new Arrow() { Type = ArrowType.None };
        style.TargetArrow = new Arrow() {
          Brush = style.Pen.Brush,
          Scale = prototype.Scale,
          Type = prototype.Type
        };
      } else {
        style.TargetArrow = Arrows.None;        
      }
    }

    private async void newGraphButton_Click(object sender, EventArgs e) {
      await GenerateGraph();
    }

    #endregion

    #region Source and Target Node

    /// <summary>
    /// Marks the list of nodes as source nodes.
    /// </summary>
    private async Task MarkAsSource(List<INode> nodes) {
      // Reset style of old source nodes
      foreach (INode sourceNode in sourceNodes) {
        if (graphControl.Graph.Contains(sourceNode)) {
          graphControl.Graph.SetStyle(sourceNode, defaultNodeStyle);
        }
      }
      sourceNodes = nodes;

      SetStyles();
      await CalculateShortestPath();
    }

    /// <summary>
    /// Marks the list of nodes as target nodes.
    /// </summary>
    private async Task MarkAsTarget(List<INode> nodes) {
      // Reset style of old target nodes
      foreach (INode targetNode in targetNodes) {
        if (graphControl.Graph.Contains(targetNode)) {
          graphControl.Graph.SetStyle(targetNode, defaultNodeStyle);
        }
      }
      targetNodes = nodes;

      SetStyles();
      await CalculateShortestPath();
    }

    /// <summary>
    /// Sets the node styles for source and target nodes
    /// </summary>
    private void SetStyles() {
      // set target node styles
      foreach (INode targetNode in targetNodes) {
        graphControl.Graph.SetStyle(targetNode, targetNodeStyle);
      }
      
      // set source node styles
      foreach (INode sourceNode in sourceNodes) {
        // check for nodes which are both - source and target
        if (targetNodes.Contains(sourceNode)) {
          graphControl.Graph.SetStyle(sourceNode, sourceAndTargetNodeStyle);
        } else {
          graphControl.Graph.SetStyle(sourceNode, sourceNodeStyle);
        }
      }
    }

    #endregion

    #region Layout

    /// <summary>
    /// Populates the layout combo box.
    /// </summary>
    private void PopulateLayoutComboBox() {
      layouts["Hierarchic Layout"] = new HierarchicLayout();
      layouts["Organic Layout"] = new OrganicLayout { MinimumNodeDistance = 40 };
      layouts["Orthogonal Layout"] = new OrthogonalLayout();

      layoutComboBox.ItemsSource = layouts.Keys.ToArray();
    }

    /// <summary>
    /// Applies the layout and recalculates the shortest paths.
    /// </summary>
    private async Task ApplyLayout() {
      if (currentLayout != null) {
        await graphControl.MorphLayout(currentLayout, TimeSpan.FromSeconds(1));
        await CalculateShortestPath();
      }
    }

    /// <summary>
    /// Handles the Click event of the applyLayoutButton control.
    /// </summary>
    private async void applyLayoutButton_Click(object sender, EventArgs e) {
      await ApplyLayout();
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the layoutComboBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private async void layoutComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      currentLayout = layouts[(string) layoutComboBox.SelectedItem];
      await ApplyLayout();
    }

    #endregion

    #region Graph Generation

    /// <summary>
    /// Generates a new graph, applies the layout and recalculates the shortest paths.
    /// </summary>
    private async Task GenerateGraph() {
      graphControl.Graph.Clear();
      randomGraphGenerator.Generate(graphControl.Graph);
      // center the graph to prevent the initial layout fading in from the top left corner
      await graphControl.FitGraphBounds();
      await ApplyLayout();
    }

    #endregion

    #region Shortest Path Calculation

    /// <summary>
    /// Calculates the shortest paths from a set of source nodes 
    /// to a set of target nodes and marks it.
    /// </summary>
    /// <remarks>This is the implementation for a list of source and target nodes.</remarks>
    private async Task CalculateShortestPath() {

      // reset old path edges
      foreach (IEdge edge in pathEdges) {
        if (graphControl.Graph.Contains(edge)) {
          graphControl.Graph.SetStyle(edge, defaultEdgeStyle);
        }
      }
      // remove deleted nodes 
      foreach (var sourceNode in sourceNodes.ToArray()) {
        if (!graphControl.Graph.Contains(sourceNode)) {
          sourceNodes.Remove(sourceNode);
        }
      }
      foreach (var targetNode in targetNodes.ToArray()) {
        if (!graphControl.Graph.Contains(targetNode)) {
          targetNodes.Remove(targetNode);
        }
      }

      ((GraphEditorInputMode) graphControl.InputMode).Waiting = true;
      var allPaths = await new AllPairsShortestPaths {
          Costs = {Delegate = GetEdgeCost},
          Directed = directed,
          Sources = {Source = sourceNodes},
          Sinks = {Source = targetNodes}
      }.RunAsync(graphControl.Graph);

      pathEdges = allPaths.Paths.SelectMany(p=>p.Edges);
      
      // mark path with path style
      foreach (IEdge edge in pathEdges) {
        if (graphControl.Graph.Contains(edge)) {
          graphControl.Graph.SetStyle(edge, pathEdgeStyle);
        }
      }
      ((GraphEditorInputMode) graphControl.InputMode).Waiting = false;
      graphControl.Invalidate();
    }

    /// <summary>
    /// Callback that gets the edge cost for a given edge.
    /// </summary>
    /// <param name="edge">The edge.</param>
    /// <returns>The cost of the edge</returns>
    private double GetEdgeCost(IEdge edge) {
      // if edge has at least one label...
      if (edge.Labels.Count > 0) {
        // ..try to return it's value
        double length;
        if (Double.TryParse(edge.Labels[0].Text, NumberStyles.Float, Thread.CurrentThread.CurrentUICulture, out length)) {
          return Math.Max(length, 0);
        }
      }

      // calculate geometric edge length
      PointD[] edgePoints = new PointD[edge.Bends.Count + 2];

      edgePoints[0] = edge.SourcePort.GetLocation();
      edgePoints[edge.Bends.Count + 1] = edge.TargetPort.GetLocation();

      for (int i = 0; i < edge.Bends.Count; i++) {
        edgePoints[i + 1] = edge.Bends[i].Location.ToPointD();
      }

      double totalEdgeLength = 0;
      for (int i = 0; i < edgePoints.Length - 1; i++) {
        totalEdgeLength += edgePoints[i].DistanceTo(edgePoints[i + 1]);
      }
      return totalEdgeLength;
    }

    #endregion

    /// <summary>
    /// Handles the Click event of the setLabelValueButton control.
    /// </summary>
    /// <remarks>Shows a dialog that can be used to specify a numeric label for all edges.</remarks>
    private async void setLabelValueButton_Click(object sender, EventArgs e) {
      LabelValueDialog labelValueForm = new LabelValueDialog();
      if (labelValueForm.ShowDialog() == true){
        double i = labelValueForm.Value;
        foreach (IEdge edge in graphControl.Graph.Edges) {
          if (edge.Labels.Count > 0) {
            graphControl.Graph.SetLabelText(edge.Labels[0], i.ToString(Thread.CurrentThread.CurrentUICulture));
          } else {
            graphControl.Graph.AddLabel(edge, String.Empty + i);
          }
        }
        await CalculateShortestPath();
      }
    }

    /// <summary>
    /// Handles the Click event of the deleteLabelsButton control.
    /// </summary>
    private async void deleteLabelsButton_Click(object sender, EventArgs e) {
      foreach (IEdge edge in graphControl.Graph.Edges.ToArray()) {
        foreach (ILabel label in edge.Labels.ToArray()) {
          graphControl.Graph.Remove(label);
        }
      }
      await CalculateShortestPath();
    }
  }

}
