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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Grid;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SimpleLayoutGrid
{
  /// <summary>
  ///   Holds the GraphControl
  /// </summary>
  [Demo("simple-layout-grid", "layout", "Simple Layout Grid", Summary = "Shows how to create a simple layout grid.",
      Tags = ["layout", "layout grid", "hierarchical"], Keywords = ["layoutfeatures", "partition grid", "layout grid"],
      RelatedDemos = [])]
  public sealed partial class SimpleLayoutGridDemo
  {
    /// <summary>
    ///   The Layout Grid
    /// </summary>
    private LayoutGrid? layoutGrid;

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      // Initialize the input mode
      graphControl.InputMode = new GraphViewerInputMode();

      // Initialize the styles for the graph elements
      DemoStyles.InitDemoStyles(graphControl.Graph);

      // Load the graph
      LoadSampleGraph();

      // Create the layout grid
      layoutGrid = CreateLayoutGrid();
      if (layoutGrid is null) {
        return;
      }

      // Initialize the visual creator for the layout grid
      InitializeLayoutGridVisual();

      // Run the layout
      RunLayout();
    }

    /// <summary>
    ///   Creates the layout grid.
    /// </summary>
    /// <returns>The created layout grid</returns>
    private LayoutGrid? CreateLayoutGrid() {
      var graph = graphControl.Graph;

      // find the desired number of rows/columns
      var columnCount = double.NegativeInfinity;
      var rowCount = double.NegativeInfinity;
      foreach (var node in graph.Nodes) {
        if (!graph.IsGroupNode(node)) {
          var nodeData = (NodeData) node.Tag!;
          var columnIndex = nodeData.Column;
          var rowIndex = nodeData.Row;
          // the column/row indices are stored to each node's tag
          node.Tag = new NodeData(rowIndex, columnIndex);
          columnCount = Math.Max(columnIndex, columnCount);
          rowCount = Math.Max(rowIndex, rowCount);
        } else {
          graph.AddLabel(node, "Group");
        }
      }
      columnCount++;
      rowCount++;

      // creates and configures the LayoutGrid
      if (rowCount > 0 && columnCount > 0) {
        var grid = new LayoutGrid((int) rowCount, (int) columnCount);

        foreach (var columnDescriptor in grid.Columns) {
          columnDescriptor.MinimumWidth = 50;
          columnDescriptor.LeftPadding = 10;
          columnDescriptor.RightPadding = 10;
        }

        foreach (var rowDescriptor in grid.Rows) {
          rowDescriptor.MinimumHeight = 50;
          rowDescriptor.TopPadding = 10;
          rowDescriptor.BottomPadding = 10;
        }
        return grid;
      }
      return null;
    }

    /// <summary>
    ///   Arranges the graph with the hierarchical layout algorithm.
    /// </summary>
    private void RunLayout() {
      // configures the layout algorithm
      var layoutAlgorithm = new HierarchicalLayout { NodeDistance = 25 };

      // create the grid cell descriptors
      var layoutData = layoutAlgorithm.CreateLayoutData(graphControl.Graph);
      layoutData.LayoutGridData.LayoutGridCellDescriptors.MapperFunction = node =>
          !graphControl.Graph.IsGroupNode(node)
              ? layoutGrid!.CreateCellDescriptor(((NodeData) node.Tag!).Row, ((NodeData) node.Tag!).Column)
              : null;

      // applies the layout algorithm to the graph
      _ = graphControl.ApplyLayoutAnimated(layoutAlgorithm, TimeSpan.FromSeconds(1), layoutData);
    }

    /// <summary>
    ///   Initializes the visual creator for the layout grid and adds it to the background of the graph
    ///   control.
    /// </summary>
    private void InitializeLayoutGridVisual() {
      // adds the visual object to the canvas
      var layoutGridVisualCreator = new SimpleLayoutGridVisualCreator(layoutGrid!);
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, layoutGridVisualCreator);
    }

    /// <summary>
    ///   Loads the sample graph.
    /// </summary>
    private void LoadSampleGraph() {
      BuildGraphFromJson("graph-data.json", graphControl.Graph, typeof(NodeData));
    }

    [STAThread]
    private static void Main(string[] args) => Run<SimpleLayoutGridDemo>();

    public SimpleLayoutGridDemo() {
      InitializeComponent();
    }

    private sealed record NodeData(int Row, int Column);
  }
}
