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
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Graph.SimpleEditor
{
  /// <summary>
  /// Simple demo that hosts a <see cref="GraphControl"/>
  /// which enables graph editing via the default <see cref="GraphEditorInputMode"/> 
  /// input mode for editing graphs.
  /// </summary>
  /// <remarks>
  /// This demo also supports grouped graphs. Selected nodes can be grouped 
  /// in so-called group nodes using CTRL-G and again be ungrouped using CTRL-U. 
  /// To move sets of nodes into and out of group nodes using the mouse, hold down 
  /// the SHIFT key while dragging.
  /// <para>
  /// Apart from graph editing, the demo demonstrates various basic features that are already
  /// present on GraphControl (either as predefined commands or as simple method calls),
  /// for example load/save/export.
  /// </para>
  /// <para>
  /// In addition to the GraphControl itself, the demo also shows how to use the GraphOverviewControl.
  /// </para>
  /// </remarks>
  public partial class SimpleEditorWindow
  {
    private readonly GridInfo gridInfo = new GridInfo(50);
    private GridVisualCreator grid;

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public SimpleEditorWindow() {
      InitializeComponent();
    }

    /// <summary>
    /// Initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoaded(object source, EventArgs e) {
      // initialize the graph
      InitializeGraph();

      // initialize the grid for grid snapping
      InitializeGrid();

      // initialize the input mode
      graphControl.InputMode = CreateEditorMode();

      GraphControl.FitGraphBounds();
    }

    /// <summary>
    /// Creates a pre-configured <see cref="GraphSnapContext"/> for this demo.
    /// </summary>
    protected virtual GraphSnapContext CreateGraphSnapContext() {
      return new GraphSnapContext
      {
        Enabled = false,
        GridSnapType = GridSnapTypes.None,
        NodeGridConstraintProvider = new GridConstraintProvider<INode>(gridInfo),
        BendGridConstraintProvider = new GridConstraintProvider<IBend>(gridInfo),
        PortGridConstraintProvider = new GridConstraintProvider<IPort>(gridInfo)
      };
    }

    /// <summary>
    /// Creates a pre-configured <see cref="LabelSnapContext"/> for this demo.
    /// </summary>
    protected virtual LabelSnapContext CreateLabelSnapContext() {
      return new LabelSnapContext
      {
        Enabled = false,
        SnapDistance = 15,
        SnapLineExtension = 100
      };
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl, a <see cref="GraphEditorInputMode" />.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance and configures snapping and orthogonal edge editing</returns>
    protected virtual IInputMode CreateEditorMode() {
      var mode = new GraphEditorInputMode
      {
        AllowGroupingOperations = true,
        SnapContext = CreateGraphSnapContext(),
        LabelSnapContext = CreateLabelSnapContext(),
        OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext { Enabled = false },
      };

      // make bend creation more important than moving of selected edges
      // this has the effect that dragging a selected edge (not its bends)
      // will create a new bend instead of moving all bends
      // This is especially nicer in conjunction with orthogonal
      // edge editing because this creates additional bends every time
      // the edge is moved otherwise
      mode.CreateBendInputMode.Priority = mode.MoveInputMode.Priority - 1;

      return mode;
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      // Enable folding
      IFoldingView view = new FoldingManager().CreateFoldingView();
      graphControl.Graph = view.Graph;

      // Get the master graph instance and enable undoability support.
      view.Manager.MasterGraph.SetUndoEngineEnabled(true);

      #region Configure grouping

      // get a hold of the group node defaults
      var groupNodeDefaults = view.Graph.GroupNodeDefaults;

      // configure the group node style.
      //PanelNodeStyle is a nice style especially suited for group nodes
      Color groupNodeColor = Color.FromArgb(255, 214, 229, 248);
      groupNodeDefaults.Style = new CollapsibleNodeStyleDecorator(new PanelNodeStyle
      {
        Color = groupNodeColor,
        Insets = new InsetsD(5, 20, 5, 5),
        LabelInsetsColor = groupNodeColor,
      });

      // Set a different label style and parameter
      groupNodeDefaults.Labels.Style = new DefaultLabelStyle
      {
        TextAlignment = TextAlignment.Left
      };
      var labelModel = new InteriorStretchLabelModel() { Insets = new InsetsD(15, 1, 1, 1) };
      var param = labelModel.CreateParameter(InteriorStretchLabelModel.Position.North);
      groupNodeDefaults.Labels.LayoutParameter = param;

      #endregion

      #region Configure Graph defaults

      // Set the default node style
      Graph.NodeDefaults.Style = new ShinyPlateNodeStyle { Brush = Brushes.Orange };

      // Set the default node label position to centered below the node with the FreeNodeLabelModel that supports label snapping
      Graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(
          new PointD(0.5, 1.0), new PointD(0, 10), new PointD(0.5, 0.0), new PointD(0, 0), 0);

      // Set the default edge label position with the SmartEdgeLabelModel that supports label snapping
      Graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0, 0, 0.5);

      #endregion

    }

    /// <summary>
    /// Initializes the visualization of the grid feature.
    /// </summary>
    protected virtual void InitializeGrid() {
      grid = new GridVisualCreator(gridInfo);
      GraphControl.BackgroundGroup.AddChild(grid, CanvasObjectDescriptors.AlwaysDirtyInstance);
      // disable the grid by default
      grid.Visible = false;
    }

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl
    {
      get { return graphControl; }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    public IGraph Graph
    {
      get { return GraphControl.Graph; }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    private void exportImageButton_Click(object sender, EventArgs e) {
      ExportImage();
    }

    private void newButton_Click(object sender, EventArgs e) {
      ClearGraph();
    }

    private void snappingButton_Click(object sender, EventArgs e) {
      ((GraphEditorInputMode) graphControl.InputMode).SnapContext.Enabled = snappingButton.IsChecked == true;
      ((GraphEditorInputMode) graphControl.InputMode).LabelSnapContext.Enabled = snappingButton.IsChecked == true;
    }

    private void orthogonalEditingButton_Click(object sender, EventArgs e) {
      var graphEditorInputMode = ((GraphEditorInputMode) graphControl.InputMode);
      var orthogonalEdges = orthogonalEditingButton.IsChecked == true;
      graphEditorInputMode.OrthogonalEdgeEditingContext.Enabled = orthogonalEdges;
    }

    private void gridButton_Click(object sender, EventArgs e) {
      var gridEnabled = gridButton.IsChecked == true;
      var graphEditorInputMode = (GraphEditorInputMode) graphControl.InputMode;
      var snapContext = (GraphSnapContext) graphEditorInputMode.SnapContext;
      snapContext.GridSnapType = gridEnabled ? GridSnapTypes.All : GridSnapTypes.None;
      grid.Visible = gridEnabled;
      if (gridEnabled) {
        snappingButton.IsChecked = true;
        snappingButton_Click(null, null);
      }
      GraphControl.Invalidate();
    }

    private void ClearGraph() {
      graphControl.Graph.Clear();
    }

    private void ExportImage() {
      graphControl.UpdateContentRect(new InsetsD(5, 5, 5, 5));
      SaveFileDialog dialog = new SaveFileDialog();
      dialog.Filter = "JPEG Files|*.jpg;*.jpeg;*.jpe";
      if (dialog.ShowDialog(this) == true) {
        graphControl.ExportToBitmap(dialog.FileName, "image/jpeg");
      }
    }
  }
}
