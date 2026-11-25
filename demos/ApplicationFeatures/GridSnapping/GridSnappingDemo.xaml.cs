/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Text.Json;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.GridSnapping
{
  /// <summary>
  ///   This demo shows the grid snapping feature which enables snapping graph items to grid points.
  ///   Grid snapping not only works for moving or resizing nodes, but also for creating edges and adding
  ///   and moving bends.
  /// </summary>
  [Demo("grid-snapping", "application-features", "Grid Snapping",
      Summary = "Shows how to enable grid snapping during interactive changes.",
      Tags = ["grid", "interaction", "snapping"],
      Keywords = ["GraphSnapContext", "GridSnapTypes", "GridVisualCreator", "align", "visuals", "interactive"])]
  public sealed partial class GridSnappingDemo
  {
    /// <summary>
    ///   Visualizes the grid.
    /// </summary>
    private GridRenderer grid = null!;

    public GridSnappingDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // initialize graph control
      graphControl.InputMode = new GraphEditorInputMode();

      // configures default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // then build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout {
          MinimumLayerDistance = 70,
          NodeDistance = 70
      });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);

      // enable snapping and create the grid
      InitializeSnapping();
      InitializeGrid();

      // bind the buttons to their functionality
      InitializeUI();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Initializes snapping for labels and other graph items. The default snapping behavior can easily
    ///   be enabled by setting the according snap context. Those snap contexts provide many options to
    ///   fine-tune their behavior; in this case, we use it to make the items snap only to the given grid
    ///   but not to other graph items. Please see the documentation of <see cref="GraphSnapContext"/> for more information.
    /// </summary>
    private void InitializeSnapping() {
      var geim = (GraphEditorInputMode) graphControl.InputMode!;
      geim.SnapContext = new GraphSnapContext {
          // disable some default snapping behavior such that the graph items only snap to the grid and nowhere else
          SnappableItems = SnappableItems.None,
          GridSnappableItems = GridSnappableItems.All
      };
    }

    /// <summary>
    ///   Initializes the <see cref="GridInfo"/> which is the actual grid to which items can snap.
    /// </summary>
    private void InitializeGrid() {
      // Initializes GridInfo which holds the basic information about the grid
      // Sets horizontal and vertical space between grid lines
      var gridInfo = new GridInfo(50, 50, PointD.Origin);

      // Creates grid visualization and adds it to graphControl
      grid = new GridRenderer {
          GridStyle = GridStyle.Lines,
          Pen = new Pen(new SolidColorBrush(Colors.Gray), 1)
      };
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, gridInfo, grid);

      // Sets constraint provider to make nodes and bends snap to grid
      var graphSnapContext = ((GraphEditorInputMode) graphControl.InputMode!).SnapContext;
      graphSnapContext.NodeGridConstraintProvider = new GridConstraintProvider(gridInfo);
      graphSnapContext.BendGridConstraintProvider = new GridConstraintProvider(gridInfo);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph);

      // set sizes and locations specific for this demo
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(0.5, EdgeSides.BelowEdge);
    }

    /// <summary>
    ///   Binds the buttons in the toolbar to their functionality.
    /// </summary>
    private void InitializeUI() {
      var settings = new GridSnappingSettings();
      settingsPanel.DataContext = settings;
      gridVisibilityToggle.DataContext = settings;

      settings.SettingChanged += (_, _) => ApplyGridSettings(settings);

      ApplyGridSettings(settings);
    }

    private void ApplyGridSettings(GridSnappingSettings settings) {
      var graphSnapContext = ((GraphEditorInputMode) graphControl.InputMode!).SnapContext;
      graphSnapContext.GridSnapType = settings.GridSnapType;

      grid.GridStyle = settings.GridStyle;

      // make sure the grid is at least 2 pixels thick when 'Dots' is selected
      var thickness = grid.GridStyle == GridStyle.Dots ? Math.Max(2, settings.Thickness) : settings.Thickness;
      grid.Pen = new Pen(new SolidColorBrush(settings.GridColor), thickness);

      grid.Visible = settings.ShowGrid;

      graphControl.Invalidate(); // triggers repaint
    }

    [STAThread]
    public static void Main() {
      Run<GridSnappingDemo>();
    }
  }
}
