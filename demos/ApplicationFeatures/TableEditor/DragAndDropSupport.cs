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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.TableEditor
{
  /// <summary>
  ///   Creates the drag-and-drop input mode and palette for table nodes, stripes, and regular nodes.
  /// </summary>
  internal static class DragAndDropSupport
  {
    /// <summary>
    ///   Configures drag and drop interaction that considers dropping nodes on table nodes.
    /// </summary>
    public static TableNodeDropInputMode ConfigureDndInputMode(IGraph graph) {
      var inputMode = new TableNodeDropInputMode {
          Enabled = true,
          ShowPreview = true,
          // tables and tagged nodes should be created as group nodes
          IsGroupNodePredicate = node => GetTable(node) != null || Equals(node.Tag, "GroupNode")
      };

      inputMode.IsValidParentPredicate = parent => {
        var draggedNode = inputMode.CurrentDraggedItem;
        if (draggedNode != null && GetTable(draggedNode) != null && parent != null && graph.IsGroupNode(parent)) {
          // this node has a table associated - disallow dragging into a group node.
          return false;
        }
        return graph.IsGroupNode(parent);
      };
      return inputMode;
    }

    /// <summary>
    ///   Configures the drag and drop panel.
    /// </summary>
    public static void ConfigureDndPanel(ContentControl container) {
      new DragAndDropPanel(container) { MaxItemWidth = 160 }.PopulatePanel(CreateDndPanelNodes());
    }

    /// <summary>
    ///   Creates the nodes that provide the visualizations for the style panel.
    /// </summary>
    private static SimpleNode[] CreateDndPanelNodes() {
      var nodes = new List<SimpleNode>();
      // dummy table that serves to hold only a sample row
      var rowSampleTable = new Table();
      // dummy table that serves to hold only a sample column
      var columnSampleTable = new Table();

      // configure the defaults for the row sample table
      rowSampleTable.RowDefaults.Style = new NodeStyleStripeStyleAdapter(new DemoStripeStyle());

      // create the sample row
      var row = rowSampleTable.CreateRow(rowSampleTable.RootRow);
      // create an invisible sample column in this table so that we will see something.
      var rowSampleColumn = rowSampleTable.CreateColumn(rowSampleTable.RootColumn, 160, 50, null,
          new NodeStyleStripeStyleAdapter(VoidNodeStyle.Instance));

      // the sample row uses empty padding
      rowSampleTable.SetStripePadding(rowSampleColumn, InsetsD.Empty);

      var labelStyle = new LabelStyle {
          TextBrush = Brushes.White,
          TextAlignment = TextAlignment.Center,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      rowSampleTable.AddLabel(row, "Row", StretchStripeLabelModel.Left, labelStyle);

      // create an invisible row in this table so that we will see something.
      var columnSampleRow = columnSampleTable.CreateRow(columnSampleTable.RootRow, 160, 50, InsetsD.Empty,
          new NodeStyleStripeStyleAdapter(VoidNodeStyle.Instance));
      var column = columnSampleTable.CreateColumn(columnSampleTable.RootColumn, 160, 50, null,
          new NodeStyleStripeStyleAdapter(new DemoStripeStyle()));
      columnSampleTable.SetStripePadding(columnSampleRow, InsetsD.Empty);
      columnSampleTable.AddLabel(column, "Column", StretchStripeLabelModel.Top, labelStyle);

      // table for a complete sample table node
      var sampleTable = new Table { Padding = InsetsD.Empty };
      sampleTable.RowDefaults.MinimumSize = 50;
      sampleTable.ColumnDefaults.MinimumSize = 50;

      // setup defaults for the complete sample table
      // we use a custom style that alternates the stripe colors and uses a special style for all parent stripes.
      sampleTable.RowDefaults.Style = new NodeStyleStripeStyleAdapter(new DemoStripeStyle());
      sampleTable.RowDefaults.Labels.Style = labelStyle;
      // the style for the columns is simpler, we use a node control style that only points the header padding.
      sampleTable.ColumnDefaults.Style = new NodeStyleStripeStyleAdapter(new DemoStripeStyle());
      sampleTable.ColumnDefaults.Labels.Style = labelStyle;

      // create a row and a column in the sample table
      sampleTable.CreateGrid(1, 1);
      var sampleColumn = sampleTable.RootColumn.ChildColumns.First();
      sampleTable.SetSize(sampleColumn, sampleColumn.Size * 2);

      // bind the table to a dummy node which is used for drag & drop
      // binding the table is performed through a TableNodeStyle instance.
      // among other things, this also makes the table instance available in the node's lookup
      // add the sample node for the table
      var sampleNode = CreateTablePaletteNode(sampleTable);
      nodes.Add(sampleNode);

      // add sample rows and columns
      var columnNode = CreateTablePaletteNode(columnSampleTable, column);
      nodes.Add(columnNode);

      // add sample rows and columns
      var rowNode = CreateTablePaletteNode(rowSampleTable, row);
      nodes.Add(rowNode);

      // add normal sample leaf and group nodes
      nodes.Add(new SimpleNode {
          Layout = new RectD(0, 0, 80, 50),
          Style = DemoStyles.CreateDemoNodeStyle()
      });

      nodes.Add(new SimpleNode {
          Layout = new RectD(0, 0, 120, 70),
          Style = CreateGroupNodeStyle(),
          Tag = "GroupNode"
      });
      return [.. nodes];
    }

    private static SimpleNode CreateTablePaletteNode(Table table, object? tag = null) =>
        new() {
            Layout = table.Layout.ToRectD(),
            Style = new DemoTableStyle(table),
            Tag = tag
        };

    private static ITable? GetTable(INode node) => node.Lookup(typeof(ITable)) as ITable;

    /// <summary>
    ///   Creates a style that visualizes nodes as dash-dotted outline only.
    /// </summary>
    public static INodeStyle CreateGroupNodeStyle() =>
        new ShapeNodeStyle {
            Shape = ShapeNodeShape.RoundRectangle,
            Brush = Brushes.Transparent,
            Pen = new Pen(Brushes.Black, 1) {
                DashStyle = DashStyles.DashDot,
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square
            }
        };
  }
}
