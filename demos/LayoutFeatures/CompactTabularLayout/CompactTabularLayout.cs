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
using System.Linq;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Grid;
using yWorks.Layout.Router;
using yWorks.Layout.Tabular;

namespace yWorks.Demo.CompactTabularLayout
{
  /// <summary>
  ///   Configures and applies the <see cref="TabularLayout"/> algorithm for compact graph drawings.
  /// </summary>
  [Demo("layout-compact-tabular-layout", "layout-features", "Compact Tabular Layout",
      Summary = "Shows how to configure the tabular layout to create compact drawings",
      Tags = ["compact", "tabular", "router"],
      Keywords = ["layout", "compact", "table", "tabular", "edgerouter", "aspect ratio", "playground"])]
  public class CompactTabularLayoutDemo : SimpleDemo
  {
    protected override void Initialize() {
      BuildGraphFromJson("sample.json");

      // Run an initial layout without aspect ratio preservation
      ApplyCompactTabularLayout(TabularLayoutMode.AutoSize);

      // Add a toggle button to switch aspect ratio preservation on/off
      ToolBar.AddToggleButton("Preserve Aspect Ratio", pressed => {
        var tabularLayoutMode = pressed ? TabularLayoutMode.FixedSize : TabularLayoutMode.AutoSize;
        ApplyCompactTabularLayout(tabularLayoutMode);
      });
    }

    /// <summary>
    ///   Configures and applies the <see cref="TabularLayout"/> algorithm with the given mode.
    /// </summary>
    private void ApplyCompactTabularLayout(TabularLayoutMode tabularLayoutMode) {
      // Initialize tabular layout with specified mode
      var tabularLayout = new TabularLayout {
          ConsiderEdges = true, // Minimize overall edge lengths
          LayoutMode = tabularLayoutMode
      };

      // Post-process with EdgeRouter for orthogonal paths (tabular layout supports straight lines only)
      var layout = new EdgeRouter(tabularLayout);

      // Calculate layout grid based on the mode for compact node arrangement
      var layoutGrid = CalculateGrid(tabularLayoutMode);

      // Set up layout data for data-driven features like the layout grid
      var layoutData = tabularLayout.CreateLayoutData(Graph);
      layoutData.LayoutGridData.Grid = layoutGrid;
      layoutData.LayoutGridData.LayoutGridCellDescriptors.MapperFunction =
          _ => layoutGrid.CreateDynamicCellDescriptor();

      GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(0.5), layoutData);
    }

    /// <summary>
    ///   Computes the layout grid based on whether to preserve the aspect ratio of the GraphControl's width/height.
    /// </summary>
    private LayoutGrid CalculateGrid(TabularLayoutMode tabularLayoutMode) {
      LayoutGrid layoutGrid;
      if (tabularLayoutMode == TabularLayoutMode.FixedSize) {
        // Get the aspect ratio of the GraphControl's width/height
        var aspectRatio = GraphControl.InnerSize.Width / GraphControl.InnerSize.Height;
        // Preserve aspect ratio by calculating rows and columns needed for all nodes
        var nodeCount = Graph.Nodes.Count;
        var rowCount = (int) Math.Ceiling(Math.Sqrt(nodeCount / aspectRatio));
        var columnCount = (int) Math.Ceiling((double) nodeCount / rowCount);
        layoutGrid = new LayoutGrid(rowCount, columnCount);
      } else {
        // Automatically assign table size without a specific grid
        layoutGrid = new LayoutGrid(1, 1);
      }

      // Determine max node dimensions for uniform grid sizing
      var maxWidth = Graph.Nodes.Max(node => node.Layout.Width);
      var maxHeight = Graph.Nodes.Max(node => node.Layout.Height);
      foreach (var row in layoutGrid.Rows) {
        row.MinimumHeight = maxHeight + 20;
      }
      foreach (var column in layoutGrid.Columns) {
        column.MinimumWidth = maxWidth + 20;
      }
      return layoutGrid;
    }

    [STAThread]
    private static void Main() => Run<CompactTabularLayoutDemo>();
  }
}
