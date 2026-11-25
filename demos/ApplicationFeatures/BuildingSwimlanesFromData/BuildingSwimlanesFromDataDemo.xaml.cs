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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Demo.BuildingSwimlanesFromData
{
  [Demo("building-swimlanes-from-data", "application-features", "Building Swimlanes From Data",
      Summary = "Shows how to build a graph with swimlanes from data in JSON format.",
      Keywords = ["read", "write", "input", "files", "data base"], Tags = ["json", "i/o", "table"])]
  public sealed partial class BuildingSwimlanesFromDataDemo
  {
    private IGraph Graph => graphControl.Graph;

    public BuildingSwimlanesFromDataDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode { AllowGroupSelection = false };

      // Enable general undo support.
      Graph.SetUndoEngineEnabled(true);

      // Configures the table editor input mode
      ConfigureTableEditing(graphControl);

      // Configures default styles for newly created graph elements
      InitializeGraph(Graph);

      LoadGraph();
    }

    private void LoadGraph() {
      using var stream = LoadAssemblyResource("graph-data.json");
      var data = JsonSerializer.Deserialize<GraphData>(stream, GraphData.JsonOptions)!;
      BuildGraph(Graph, data);

      // Automatically layout the swimlanes. The HierarchicalLayout respects the node to cell assignment based on the
      // node's center position.
      ApplyInitialLayout(Graph);
      graphControl.FitGraphBounds();
      // Finally, clear the undo engine to prevent undoing of the graph creation.
      Graph.GetUndoEngine()!.Clear();
    }

    private static void ConfigureTableEditing(GraphControl graphControl) {
      var graphEditorInputMode = (graphControl.InputMode as GraphEditorInputMode)!;

      // Use the undo support from the graph also for all future table instances.
      Table.InstallStaticUndoSupport(graphControl.Graph);

      var reparentStripeHandler = new ReparentStripeHandler();
      reparentStripeHandler.MaxColumnLevel = 1;
      reparentStripeHandler.MaxRowLevel = 1;

      // Create a table editor input mode which also allows drag and drop.
      var tableInputMode = new TableEditorInputMode {
          ReparentStripeHandler = reparentStripeHandler,
          // Use a higher priority value than handle input mode so that handles win if both gestures are possible.
          Priority = graphEditorInputMode.HandleInputMode.Priority + 1
      };
      tableInputMode.StripeDropInputMode.Enabled = true;

      // Add to GEIM
      graphEditorInputMode.Add(tableInputMode);

      // Prevent reparenting of tables into tables by copy and paste.
      graphControl.Clipboard = new GraphClipboard {
          ParentNodeDetection = GraphClipboard.ParentNodeDetectionModes.PreviousParent
      };

      // Prevent selection of the table node.
      graphEditorInputMode.SelectablePredicate = item => item is not INode node || node.Lookup<ITable>() == null;
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    ///   Iteration and the applied information depend on the input structure, but the general approach
    ///   is always to call graph item factory methods such as CreateNode, CreateGroupNode, CreateEdge,
    ///   and AddLabel, and other IGraph functions to apply the data to the graph model.
    /// </summary>
    private static void BuildGraph(IGraph graph, GraphData graphData) {
      // Store lanes and nodes references.
      // It will be easier to assign lanes or connect them with edges afterward.
      var nodes = new Dictionary<string, INode>();
      var lanes = new Dictionary<string, IColumn>();

      // Swimlanes are a special application of Tables, either one row with several columns (i.e. vertical swimlanes), or
      // one column with several rows (i.e. horizontal swimlanes).
      // In this case we go with the vertical swimlanes. Therefore, we create a Table with one row and multiple columns,
      // depending on the input data.
      var table = new Table();

      // Configure the row style, i.e. the container for the swimlanes. In this case, they should not be rendered at all,
      // since we are creating a vertical swimlane. However, in the general case of Tables, we could use a
      // semi-transparent style here to create overlapping cell colors.
      table.RowDefaults.Padding = new InsetsD(0, 10, 0, 0);
      table.RowDefaults.Style = VoidStripeStyle.Instance;

      // Configure the column style, i.e. the actual swimlanes.
      table.ColumnDefaults.Padding = new InsetsD(10, 30, 10, 10);
      table.ColumnDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0)),
          BackgroundPen = new Pen(Brushes.Black, 1),
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Center,
          TextSize = 16,
          Padding = new InsetsD(5, 3, 5, 3)
      };
      table.ColumnDefaults.Size = 200;
      table.ColumnDefaults.Style = new NodeStyleStripeStyleAdapter(new ShapeNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0xc4, 0xd7, 0xed)),
          Pen = new Pen(Brushes.Black, 1),
          Shape = ShapeNodeShape.Rectangle
      });

      // The second column style is used for even-odd styling of the columns.
      var alternateColumnStyle = new NodeStyleStripeStyleAdapter(new ShapeNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0xab, 0xc8, 0xe2)),
          Pen = new Pen(Brushes.Black, 1),
          Shape = ShapeNodeShape.Rectangle
      });

      // Create the single container row.
      var row = table.CreateRow();

      // Iterate the lanes data and create the according lanes.
      for (var i = 0; i < graphData.LanesSource.Count; i++) {
        var laneData = graphData.LanesSource[i];
        var column = table.CreateColumn(style: i % 2 == 0 ? table.ColumnDefaults.Style : alternateColumnStyle,
            tag: laneData);
        table.AddLabel(column, string.IsNullOrEmpty(laneData.Label) ? laneData.Id : laneData.Label);

        // We store the lane index to easily assign nodes to them later.
        lanes.Add(laneData.Id, column);
      }

      // Create a top-level group node and bind, via the TableNodeStyle, the table to it.
      var tableStyle = new TableNodeStyle(table);
      var tableNode = graph.CreateGroupNode(layout: table.Layout.ToRectD(), style: tableStyle);

      // Iterate the node data and create the according nodes.
      foreach (var nodeData in graphData.NodesSource) {
        var size = nodeData.Size ?? [50, 50];
        var node = graph.CreateNode(new RectD(0, 0, size[0], size[1]), tag: nodeData);
        if (nodeData.Label != null) {
          graph.AddLabel(node, nodeData.Label);
        }
        if (!string.IsNullOrEmpty(nodeData.Fill)) {
          // If the node data specifies an individual fill color, adjust the style.
          var style = (RectangleNodeStyle) graph.NodeDefaults.Style.Clone();
          style.Brush = (Brush) new BrushConverter().ConvertFromInvariantString(nodeData.Fill)!;
          graph.SetStyle(node, style);
        }
        nodes.Add(nodeData.Id, node);

        if (!string.IsNullOrEmpty(nodeData.Lane)) {
          // Nodes are assigned to lanes based on their center location. We could either place them manually by getting
          // the respective lane's bounds, or we can use a helper function to place nodes in specific cells. In case of
          // manually, placing the nodes, don't forget to also reparent the nodes to the table group, which is also done
          // by the helper function.
          var cell = table.GetCellBounds(lanes[nodeData.Lane], row);
          var center = cell.GetCenter();
          graph.SetNodeCenter(node,
              new PointD(tableNode.Layout.X + center.X - table.Layout.X,
                  tableNode.Layout.Y + center.Y - table.Layout.Y));
          graph.SetParent(node, tableNode);
        }
      }

      // Iterate the edge data and create the according edges.
      foreach (var edgeData in graphData.EdgesSource) {
        // Note that nodes and groups need to have disjoint sets of ids, otherwise it is impossible to determine
        // which node is the correct source/target.
        var edge = graph.CreateEdge(nodes[edgeData.From], nodes[edgeData.To], tag: edgeData);
        if (edgeData.Label != null) {
          graph.AddLabel(edge, edgeData.Label);
        }
      }
    }

    /// <summary>
    ///   Serializes the graph to JSON.
    /// </summary>
    private static GraphData WriteToJson(IGraph graph) {
      var output = new GraphData();

      // Find the table, we assume there is only one
      var tableNode = graph.Nodes.FirstOrDefault(node => node.Lookup<ITable>() != null);
      var table = tableNode?.Lookup<ITable>();

      var columns = table?.RootColumn.ChildColumns.ToList() ?? [];
      for (var i = 0; i < columns.Count; i++) {
        output.LanesSource.Add(new LaneData {
            Id = $"lane{i}",
            Label = columns[i].Labels.FirstOrDefault()?.Text
        });
      }

      // Serialize nodes with their swimlane information.
      var nodeIds = new Dictionary<INode, string>();
      foreach (var node in graph.Nodes) {
        // Skip the table node.
        if (node == tableNode) {
          continue;
        }
        // Save IDs to easily create edgesSource afterward.
        var id = nodeIds.Count.ToString(CultureInfo.InvariantCulture);
        nodeIds.Add(node, id);
        var data = new NodeData {
            Id = id,
            Size = [node.Layout.Width, node.Layout.Height],
            Fill = node.Style is RectangleNodeStyle { Brush: SolidColorBrush brush } ? brush.Color.ToString() : null,
            Label = node.Labels.FirstOrDefault()?.Text
        };
        output.NodesSource.Add(data);

        // Store the lane of the node from its current center, not its original JSON tag.
        if (table != null && table.FindColumn(tableNode!, node.Layout.GetCenter()) is { } column) {
          data.Lane = $"lane{columns.IndexOf(column)}";
        }
      }

      // Serialize edges.
      foreach (var edge in graph.Edges) {
        output.EdgesSource.Add(new EdgeData {
            From = nodeIds[edge.GetSourceNode()!],
            To = nodeIds[edge.GetTargetNode()!],
            Label = edge.Labels.FirstOrDefault()?.Text
        });
      }
      return output;
    }

    /// <summary>
    ///   HierarchicalLayout respects node-to-cell assignments by considering node locations relative
    ///   to the swimlane bounds.
    /// </summary>
    private static HierarchicalLayout CreateLayout() {
      var layout = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.TopToBottom };
      layout.ComponentLayout.Enabled = true;
      return layout;
    }

    private static void ApplyInitialLayout(IGraph graph) {
      var configurator = new TableLayoutConfigurator();
      var data = configurator.Prepare(graph);
      try {
        graph.ApplyLayout(CreateLayout(), data);
        configurator.Restore(graph);
      } finally {
        configurator.CleanUp(graph);
      }
    }

    private async void RunLayout(object sender, RoutedEventArgs e) {
      layoutButton.IsEnabled = false;
      try {
        await RunLayoutAnimated();
      } catch (Exception exception) {
        MessageBox.Show(exception.Message, "Layout failed", MessageBoxButton.OK, MessageBoxImage.Error);
      } finally {
        layoutButton.IsEnabled = true;
      }
    }

    private Task RunLayoutAnimated() {
      // LayoutExecutor sets up the whole layout pipeline, including table layout.
      var executor = new LayoutExecutor(graphControl, CreateLayout()) {
          // Table layout is enabled by default; keep it explicit here.
          ConfigureTableLayout = true,
          AnimationDuration = TimeSpan.FromSeconds(1),
          AnimateViewport = true
      };
      // Apply the layout.
      return executor.Start();
    }

    /// <summary>
    ///   Initializes the defaults for styling in this demo.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      // Set styles for this demo.
      DemoStyles.InitDemoStyles(graph, orthogonalEdgeEditing: true);
      // Set sizes and locations specific to this demo.
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    /// <summary>
    ///   Saves the graph as JSON when the toolbar button is clicked.
    /// </summary>
    private void SaveToJson(object sender, RoutedEventArgs e) {
      var dialog = new SaveFileDialog {
          FileName = "graph.json",
          DefaultExt = ".json",
          Filter = "JSON files (*.json)|*.json"
      };
      if (dialog.ShowDialog(Window.GetWindow(this)) != true) {
        return;
      }
      try {
        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(WriteToJson(Graph), GraphData.JsonOptions));
      } catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) {
        MessageBox.Show(exception.Message, "Could not save JSON", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    [STAThread]
    private static void Main() {
      Run<BuildingSwimlanesFromDataDemo>();
    }
  }
}
