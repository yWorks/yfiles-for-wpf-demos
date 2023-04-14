/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using Demo.yFiles.Toolkit;

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
    /// <seealso cref="SetDefaultStyles"/>
    protected virtual void OnLoaded(object source, EventArgs e) {
      // Configure and enable folding
      var foldingManager = new FoldingManager();
      var foldingView = foldingManager.CreateFoldingView();
      foldingView.EnqueueNavigationalUndoUnits = true;
      var graph = foldingView.Graph;
      GraphControl.Graph = graph;

      // Setup the default styles for the graph
      SetDefaultStyles(graph);

      // initialize the grid for grid snapping
      InitializeGrid();

      // Specify a configured input mode that enables graph editing
      GraphControl.InputMode = CreateEditorMode();

      // Create a sample graph
      CreateSampleGraph(graph);

      GraphControl.FitGraphBounds();

      // Enable the undo engine on the master graph
      foldingManager.MasterGraph.SetUndoEngineEnabled(true);
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

      return mode;
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void SetDefaultStyles(IGraph graph) {
      // Assign the default demo styles
      DemoStyles.InitDemoStyles(graph, foldingEnabled: true);

      // Set the default node label position to centered below the node with the FreeNodeLabelModel that supports label snapping
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(
          new PointD(0.5, 1.0), new PointD(0, 10), new PointD(0.5, 0.0), new PointD(0, 0), 0);

      // Set the default edge label position with the SmartEdgeLabelModel that supports label snapping
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0, 0, 0.5);
    }

    /// <summary>
    /// Creates the initial graph.
    /// </summary>
    /// <param name="graph"></param>
    private void CreateSampleGraph(IGraph graph) {
      graph.Clear();

      var n1 = graph.CreateNode(new Rect(126, 0, 30, 30));
      var n2 = graph.CreateNode(new Rect(126, 72, 30, 30));
      var n3 = graph.CreateNode(new Rect(75, 147, 30, 30));
      var n4 = graph.CreateNode(new Rect(177.5, 147, 30, 30));
      var n5 = graph.CreateNode(new Rect(110, 249, 30, 30));
      var n6 = graph.CreateNode(new Rect(177.5, 249, 30, 30));
      var n7 = graph.CreateNode(new Rect(110, 299, 30, 30));
      var n8 = graph.CreateNode(new Rect(177.5, 299, 30, 30));
      var n9 = graph.CreateNode(new Rect(110, 359, 30, 30));
      var n10 = graph.CreateNode(new Rect(47.5, 299, 30, 30));
      var n11 = graph.CreateNode(new Rect(20, 440, 30, 30));
      var n12 = graph.CreateNode(new Rect(110, 440, 30, 30));
      var n13 = graph.CreateNode(new Rect(20, 515, 30, 30));
      var n14 = graph.CreateNode(new Rect(80, 515, 30, 30));
      var n15 = graph.CreateNode(new Rect(140, 515, 30, 30));
      var n16 = graph.CreateNode(new Rect(20, 569, 30, 30));

      var group1 = graph.CreateGroupNode(null, new Rect(25, 45, 202.5, 353));
      graph.AddLabel(group1, "Group 1");
      graph.GroupNodes(group1, new[] { n2, n3, n4, n9, n10 });

      var group2 = graph.CreateGroupNode(group1, new Rect(98, 222, 119.5, 116));
      graph.AddLabel(group2, "Group 2");
      graph.GroupNodes(group2, new[] { n5, n6, n7, n8 });

      var group3 = graph.CreateGroupNode(null, new Rect(10, 413, 170, 141));
      graph.AddLabel(group3, "Group 3");
      graph.GroupNodes(group3, new[] { n11, n12, n13, n14, n15 });

      graph.CreateEdge(n1, n2);
      graph.CreateEdge(n2, n3);
      graph.CreateEdge(n2, n4);
      graph.CreateEdge(n3, n5);
      graph.CreateEdge(n3, n10);
      graph.CreateEdge(n5, n7);
      graph.CreateEdge(n7, n9);
      graph.CreateEdge(n4, n6);
      graph.CreateEdge(n6, n8);
      graph.CreateEdge(n10, n11);
      graph.CreateEdge(n10, n12);
      graph.CreateEdge(n11, n13);
      graph.CreateEdge(n13, n16);
      graph.CreateEdge(n12, n14);
      graph.CreateEdge(n12, n15);
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
      if (graphControl.ContentRect.IsEmpty) {
        MessageBox.Show("Canvas is empty.", "Image Export");
        return;
      }
      var dialog = new SaveFileDialog { Filter = "JPEG Files|*.jpg;*.jpeg;*.jpe" };
      if (dialog.ShowDialog(this) == true) {
        graphControl.ExportToBitmap(dialog.FileName, "image/jpeg");
      }
    }
  }
}

