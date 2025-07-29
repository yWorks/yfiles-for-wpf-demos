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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;

namespace Demo.yFiles.Graph.Input.SnapLines
{
  /// <summary>
  ///   Interaction logic for SnapLinesWindow
  /// </summary>
  public partial class SnapLinesWindow
  {
    #region static fields

    private const string SnappingConfiguration = "Snapping Configuration";

    private const string CollectSnapLinesGroup = "Collect Snap Lines From";
    private const string CollectNodePairCenterSnapLines = "Centers of Two Nodes";
    private const string CollectNodePairSnapLines = "Same Distance to Two Nodes";
    private const string CollectNodeSnapLines = "Node Borders and Centers";
    private const string CollectEdgeSnapLines = "Edge Segments";
    private const string CollectPortSnapLines = "Port Locations";
    private const string CollectSameSizeSnapLines = "Same Size of Two Nodes";
    private const string OrthogonalMovement = "Orthogonal Movement";

    private const string OrthogonalSnappingGroup = "Support Orthogonal Edge Segments";
    private const string OrthogonalPorts = "Segments at Moved Ports";
    private const string OrthogonalBends = "Segments Next to Bends";
    private const string OrthogonalEdgeCreation = "During Edge Creation";
    private const string OrthogonalEdgeEditing = "During Edge Editing";

    private const string SnappingElementsGroup = "Snapping Elements";
    private const string SnapSegments = "Orthogonal Edge Segments";
    private const string SnapBends = "Bends";
    private const string SnapAdjacentBends = "Adjacent Segments";
    private const string SnapNodes = "Nodes";

    private const string SnappingDistancesGroup = "Snapping Distances";
    private const string NodeToNode = "Node to Node";
    private const string NodeToEdge = "Node to Edge";
    private const string EdgeToEdge = "Edge to Edge";

    private const string GridGroup = "Grid";
    private const string GridHorizontalWidth = "Horizontal Grid Width";
    private const string GridVerticalWidth = "Vertical Grid Width";
    private const string GridSnapDistance = "Grid Snap Distance";
    private const string GridSnapping = "Snap to Grid";

    #endregion

    #region private fields

    private GraphSnapContext snapContext;
    private GridRenderer grid;
    private GridInfo gridInfo;
    private GraphEditorInputMode graphEditorInputMode;

    #endregion

    public SnapLinesWindow() {
      InitializeComponent();
      SetupOptions();
    }

    private void OnLoaded(object source, EventArgs args) {
      // intialize input mode
      graphEditorInputMode = new GraphEditorInputMode();
      GraphControl.InputMode = graphEditorInputMode;

      // initialize grid
      gridInfo = new GridInfo { HorizontalSpacing = 30, VerticalSpacing = 30 };
      grid = new GridRenderer();
      var renderTree = GraphControl.RenderTree;
      renderTree.CreateElement(renderTree.BackgroundGroup, gridInfo, grid);

      snapContext = graphEditorInputMode.SnapContext;
      snapContext.Enabled = true;
      snapContext.NodeGridConstraintProvider = new GridConstraintProvider(gridInfo);
      snapContext.BendGridConstraintProvider = new GridConstraintProvider(gridInfo);

      // initialize current values
      OnSnappingChanged(this, null);
      OnGridHorizontalWidthChanged(this, null);
      OnGridVerticalWidthChanged(this, null);

      GraphControl.Invalidate();
      GraphControl.ZoomChanged += delegate { UpdateGrid(); };
      GraphControl.ViewportChanged += delegate { UpdateGrid(); };

      DemoStyles.InitDemoStyles(GraphControl.Graph, orthogonalEdgeEditing : true);

      InitializeGraph();
    }

    private void InitializeGraph() {
      ReadGraph("Resources\\SnapLineDemo.graphml");
    }

    private void UpdateGrid() {
      // Adjust grid visualization to the snap type
      switch (snapContext.GridSnapType) {
        case GridSnapTypes.None:
          grid.Visible = false;
          break;
        case GridSnapTypes.HorizontalLines:
          grid.Visible = true;
          grid.GridStyle = GridStyle.HorizontalLines;
          break;
        case GridSnapTypes.VerticalLines:
          grid.Visible = true;
          grid.GridStyle = GridStyle.VerticalLines;
          break;
        case GridSnapTypes.Lines:
          grid.Visible = true;
          grid.GridStyle = GridStyle.Lines;
          break;
        case GridSnapTypes.GridPoints:
          grid.Visible = true;
          grid.GridStyle = GridStyle.Dots;
          break;
        case GridSnapTypes.All:
          grid.Visible = true;
          grid.GridStyle = GridStyle.Crosses;
          break;
      }
      GraphControl.Invalidate();
    }

    #region Snapping Options

    public OptionHandler Handler { get; private set; }

    private void SetupOptions() {
      SetupHandler();
      AddEditorControlToForm();
    }

    private void AddEditorControlToForm() {
      editorControl.OptionHandler = Handler;
    }

    private void SetupHandler() {
      Handler = new OptionHandler(SnappingConfiguration);
      var currentGroup = Handler.AddGroup(CollectSnapLinesGroup);
      currentGroup.AddBool(CollectNodePairSnapLines, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(CollectNodePairCenterSnapLines, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(CollectSameSizeSnapLines, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(CollectNodeSnapLines, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(CollectEdgeSnapLines, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(CollectPortSnapLines, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(OrthogonalMovement, false).PropertyChanged += OnSnappingChanged;

      var innerGroup = currentGroup.AddGroup(SnappingDistancesGroup);
      innerGroup.AddDouble(NodeToNode, 50).PropertyChanged += OnSnappingChanged;
      innerGroup.AddDouble(NodeToEdge, 30).PropertyChanged += OnSnappingChanged;
      innerGroup.AddDouble(EdgeToEdge, 40).PropertyChanged += OnSnappingChanged;

      currentGroup = Handler.AddGroup(OrthogonalSnappingGroup);
      currentGroup.AddBool(OrthogonalPorts, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(OrthogonalBends, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(OrthogonalEdgeCreation, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(OrthogonalEdgeEditing, false).PropertyChanged += OnSnappingChanged;

      currentGroup = Handler.AddGroup(SnappingElementsGroup);
      currentGroup.AddBool(SnapNodes, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(SnapBends, true).PropertyChanged += OnSnappingChanged;
      currentGroup.AddBool(SnapSegments, true).PropertyChanged += OnSnappingChanged;

      currentGroup = Handler.AddGroup(GridGroup);
      currentGroup.AddOptionItem(new CollectionOptionItem<GridSnapTypes>(GridSnapping,
          new List<GridSnapTypes> {
              GridSnapTypes.All,
              GridSnapTypes.GridPoints,
              GridSnapTypes.Lines,
              GridSnapTypes.VerticalLines,
              GridSnapTypes.HorizontalLines,
              GridSnapTypes.None
          }, GridSnapTypes.None)).PropertyChanged += OnSnappingChanged;
      currentGroup.AddInt(GridHorizontalWidth, 50).PropertyChanged += OnGridHorizontalWidthChanged;
      currentGroup.AddInt(GridVerticalWidth, 50).PropertyChanged += OnGridVerticalWidthChanged;
      currentGroup.AddInt(GridSnapDistance, 10).PropertyChanged += OnGridSnapDistanceChanged;
    }

    private void OnSnappingChanged(object sender, PropertyChangedEventArgs e) {
      snapContext.CollectNodePairCenterSnapLines =
          (bool) Handler.GetValue(CollectSnapLinesGroup, CollectNodePairCenterSnapLines);
      snapContext.CollectNodePairSnapLines = (bool) Handler.GetValue(CollectSnapLinesGroup, CollectNodePairSnapLines);
      snapContext.CollectNodeSizes = (bool) Handler.GetValue(SnappingElementsGroup, SnapNodes) &&
          (bool) Handler.GetValue(CollectSnapLinesGroup, CollectSameSizeSnapLines);
      snapContext.CollectNodeSnapReferences = (bool) Handler.GetValue(CollectSnapLinesGroup, CollectNodeSnapLines);
      snapContext.CollectEdgeSnapReferences = (bool) Handler.GetValue(CollectSnapLinesGroup, CollectEdgeSnapLines);
      snapContext.CollectPortSnapReferences = (bool) Handler.GetValue(CollectSnapLinesGroup, CollectPortSnapLines);

      snapContext.NodeDistance =
          (double) Handler.GetValue(CollectSnapLinesGroup + "." + SnappingDistancesGroup, NodeToNode);
      snapContext.NodeToEdgeDistance =
          (double) Handler.GetValue(CollectSnapLinesGroup + "." + SnappingDistancesGroup, NodeToEdge);
      snapContext.EdgeDistance =
          (double) Handler.GetValue(CollectSnapLinesGroup + "." + SnappingDistancesGroup, EdgeToEdge);

      var snapOrthogonalMovement = (bool) Handler.GetValue(CollectSnapLinesGroup, OrthogonalMovement)
          ? SnappableItems.OrthogonalMovement
          : SnappableItems.None;

      var snapPortAdjacentSegments = (bool) Handler.GetValue(OrthogonalSnappingGroup, OrthogonalPorts)
          ? SnappableItems.PortAdjacentSegment
          : SnappableItems.None;
      var snapBendAdjacentSegments = (bool) Handler.GetValue(OrthogonalSnappingGroup, OrthogonalBends)
          ? SnappableItems.BendAdjacentSegment
          : SnappableItems.None;

      var snapNodes = (bool) Handler.GetValue(SnappingElementsGroup, SnapNodes)
          ? SnappableItems.Node
          : SnappableItems.None;
      var snapBends = (bool) Handler.GetValue(SnappingElementsGroup, SnapBends)
          ? SnappableItems.Bend
          : SnappableItems.None;
      var snapOrthogonalSegments = (bool) Handler.GetValue(SnappingElementsGroup, SnapSegments)
          ? SnappableItems.OrthogonalSegment
          : SnappableItems.None;
      snapContext.SnappableItems = snapOrthogonalMovement | snapPortAdjacentSegments | snapBendAdjacentSegments |
          snapNodes | snapBends | snapOrthogonalSegments;

      snapContext.GridSnapType = (GridSnapTypes) Handler.GetValue(GridGroup, GridSnapping);

      graphEditorInputMode.OrthogonalEdgeEditingContext.Enabled =
          (bool) Handler.GetValue(OrthogonalSnappingGroup, OrthogonalEdgeEditing);
      graphEditorInputMode.CreateEdgeInputMode.OrthogonalEdgeCreation =
          (bool) Handler.GetValue(OrthogonalSnappingGroup, OrthogonalEdgeCreation)
              ? OrthogonalEdgeEditingPolicy.Always
              : OrthogonalEdgeEditingPolicy.Never;
      UpdateGrid();
    }

    private void OnGridHorizontalWidthChanged(object sender, PropertyChangedEventArgs e) {
      var width = (int) Handler.GetValue(GridGroup, GridHorizontalWidth);
      if (gridInfo != null) {
        gridInfo.HorizontalSpacing = width;
      }
      UpdateGrid();
    }

    private void OnGridVerticalWidthChanged(object sender, PropertyChangedEventArgs e) {
      var width = (int) Handler.GetValue(GridGroup, GridVerticalWidth);
      if (gridInfo != null) {
        gridInfo.VerticalSpacing = width;
      }
      UpdateGrid();
    }

    private void OnGridSnapDistanceChanged(object sender, PropertyChangedEventArgs e) {
      var width = (int) Handler.GetValue(GridGroup, GridSnapDistance);
      snapContext.GridSnapDistance = width;
    }

    #endregion

    #region read initial graph

    /// <summary>
    ///   Reads the graph from a file with the given file name.
    /// </summary>
    protected virtual void ReadGraph(string filename) {
      using (var streamReader = new StreamReader(filename)) {
        ReadGraph(streamReader);
      }
    }

    /// <summary>
    ///   Reads the graph from a given Reader instance.
    /// </summary>
    protected virtual void ReadGraph(TextReader reader) {
      GraphControl.ImportFromGraphML(reader);
    }

    #endregion

    private GraphControl GraphControl => graphControl;
  }
}
