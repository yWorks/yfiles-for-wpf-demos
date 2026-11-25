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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.ListNode;

/// <summary>
///   Shows how to display a node with sections arranged in rows that can be rearranged interactively.
/// </summary>
[Demo("list-node", "view", "List Node", Summary = "Shows a node which contains re-arrangeable rows.",
    Tags = ["interaction", "style", "table"], Keywords = ["rows", "tables"])]
public sealed class ListNodeDemo : SimpleDemo
{
  /// <summary>
  ///   Set up the graph, sample data, and editing gestures.
  /// </summary>
  protected override void Initialize() {
    ConfigureGraph(Graph);
    CreateSampleGraph(Graph);
    GraphControl.FitGraphBounds();
    InitializeInteraction();
  }

  /// <summary>
  ///   Set up the <see cref="GraphEditorInputMode"/> and customize the editing gestures:
  ///   - do not delete labels and ports
  ///   - create only row containing nodes
  ///   - orthogonal edges
  ///   - register new label texts in the model data
  ///   - set up a special move mode
  ///   - set up a context menu
  /// </summary>
  private void InitializeInteraction() {
    var inputMode = new GraphEditorInputMode {
        AllowEditLabel = true,
        AllowAddLabel = false,
        DeletableItems = GraphItemTypes.Node | GraphItemTypes.Bend | GraphItemTypes.Edge,
        NodeCreator = (_, graph, location, parent) =>
            CreateNode(graph, location, $"Node {graph.Nodes.Count + 1}", parent),
        ClickHitTestOrder = [GraphItemTypes.Label, GraphItemTypes.Edge, GraphItemTypes.Node, GraphItemTypes.All]
    };

    // Prefer edge creation over moving elements, but only if creation starts over a candidate.
    inputMode.CreateEdgeInputMode.Priority = inputMode.MoveUnselectedItemsInputMode.Priority - 1;
    inputMode.CreateEdgeInputMode.StartOverCandidateOnly = true;
    inputMode.CreateEdgeInputMode.EdgeDirectionPolicy = EdgeDirectionPolicy.DetermineFromPortCandidates;

    // Register label changes in the model data.
    inputMode.LabelEdited += (_, args) => {
      var owner = args.Item.Owner;
      if (owner is INode node && node.Tag is NodeInfo nodeInfo) {
        nodeInfo.Name = args.Item.Text;
      } else if (owner is IPort port && port.Tag is RowPortInfo portInfo) {
        if (portInfo.Incoming) {
          portInfo.Row.In = args.Item.Text;
        } else {
          portInfo.Row.Out = args.Item.Text;
        }
      }
    };

    inputMode.DeletedItem += (_, args) => {
      if (args.Details is LabelEventArgs details && details.Owner is IPort port && port.Owner is INode node) {
        var nodeInfo = (NodeInfo) node.Tag!;
        var rowIndex = nodeInfo.Rows.IndexOf(((RowPortInfo) port.Tag!).Row);
        if (rowIndex >= 0) {
          RemoveRow(Graph, node, rowIndex);
        }
      }
    };

    // Reroute edges affected by a drag operation.
    var moveMode = inputMode.MoveUnselectedItemsInputMode;
    moveMode.DragFinished += async (_, _) => {
      var affectedPorts = moveMode.AffectedItems.OfType<IPort>().ToHashSet();
      var affectedEdges = Graph.Edges
          .Where(edge => affectedPorts.Contains(edge.SourcePort) || affectedPorts.Contains(edge.TargetPort)).ToList();
      await RerouteEdges(affectedEdges);
    };

    // Reroute a newly created edge.
    inputMode.CreateEdgeInputMode.EdgeCreated += async (_, args) => await RerouteEdges([args.Item]);

    // Disable the normal move mode and enable moving unselected elements instead.
    inputMode.MoveSelectedItemsInputMode.Enabled = false;
    var oldHitTestable = moveMode.HitTestable;
    // Only start move gestures over the node header or a row containing content.
    moveMode.HitTestable = new RowHitTestable(inputMode, oldHitTestable);
    moveMode.QueryPositionHandler += (_, args) => {
      var node = FindNodeAt(inputMode, args.QueryLocation);
      if (node?.Style is ListNodeStyle style) {
        // Cursor is over a row: return a special position handler.
        var index = style.GetRowIndex(node, args.QueryLocation);
        if (index > -1) {
          // Create a position handler to move the ports at this row.
          args.PositionHandler = new RowPositionHandler(node, index);
        }
      }
      // If the cursor is not over a row, use the default position handler.
    };

    // Do not let the user drag at ports.
    Graph.GetDecorator().Edges.ReconnectionPortCandidateProvider.Hide();

    RegisterContextMenu(inputMode);
    GraphControl.InputMode = inputMode;
  }

  /// <summary>
  ///   Finds the first node whose bounds contain the given location.
  /// </summary>
  private static INode? FindNodeAt(GraphEditorInputMode mode, PointD location) =>
      mode.FindItems(location, [GraphItemTypes.Node]).OfType<INode>().FirstOrDefault();

  /// <summary>
  ///   Creates a node with the custom row-container style.
  /// </summary>
  private INode CreateNode(IGraph graph, PointD location, string label, INode? parent = null) {
    var info = new NodeInfo(label);
    var layout = RectD.FromCenter(location, new SizeD(100, 200));
    var node = graph.CreateNode(parent, layout, new ListNodeStyle(), info);
    graph.AddLabel(node, label, nodeLabelModel.Parameters.First(), CreateNodeLabelStyle());
    return node;
  }

  private static LabelStyle CreateNodeLabelStyle() =>
      new() {
          TextBrush = Brushes.White,
          TextAlignment = TextAlignment.Center,
          VerticalTextAlignment = VerticalAlignment.Center,
          MinimumSize = new SizeD(0, 20)
      };

  /// <summary>
  ///   Creates a new row at the bottom of the node.
  /// </summary>
  private static void AddRow(IGraph graph, INode node, RowInfo rowInfo) {
    var labelStyle = new LabelStyle {
        Padding = new InsetsD(3, 1, 3, 1),
        TextBrush = CreateFrozenBrush(0x0C, 0x31, 0x3A)
    };
    var portStyle = new ShapePortStyle {
        Brush = CreateFrozenBrush(0x2C, 0x4B, 0x52),
        Pen = new Pen(Brushes.Transparent, 0),
        Shape = ShapeNodeShape.Ellipse,
        RenderSize = new SizeD(8, 8)
    };

    var nodeInfo = (NodeInfo) node.Tag!;
    var rowIndex = nodeInfo.Rows.Count;
    var style = (ListNodeStyle) node.Style;
    var model = FreeNodePortLocationModel.Instance;
    if (rowInfo.In is not null) {
      // The row is represented as a port with a label.
      var port = graph.AddPort(node,
          model.CreateParameter(new PointD(0, 0), new PointD(0, style.GetRowCenterY(rowIndex))), portStyle,
          new RowPortInfo(rowInfo, true));
      graph.AddLabel(port, rowInfo.In, new InsideOutsidePortLabelModel().CreateInsideParameter(), labelStyle);
    }
    if (rowInfo.Out is not null) {
      // The row is represented as a port with a label.
      var port = graph.AddPort(node,
          model.CreateParameter(new PointD(1, 0), new PointD(0, style.GetRowCenterY(rowIndex))), portStyle,
          new RowPortInfo(rowInfo, false));
      graph.AddLabel(port, rowInfo.Out, new InsideOutsidePortLabelModel().CreateInsideParameter(), labelStyle);
    }

    // Register the row.
    nodeInfo.Rows.Add(rowInfo);

    // Make sure the new row fits into the node.
    var layout = node.Layout;
    var minimumHeight = style.GetMinimumHeight(nodeInfo);
    if (layout.Height < minimumHeight) {
      graph.SetNodeLayout(node, new RectD(layout.X, layout.Y, layout.Width, minimumHeight));
    }
  }

  /// <summary>
  ///   Removes a row. The caller must provide a valid row index.
  /// </summary>
  private static void RemoveRow(IGraph graph, INode node, int rowIndex) {
    // Remove its ports and row info.
    var nodeInfo = (NodeInfo) node.Tag!;
    var rowInfo = nodeInfo.Rows[rowIndex];
    foreach (var port in node.Ports.Where(port => ReferenceEquals(((RowPortInfo) port.Tag!).Row, rowInfo)).ToList()) {
      graph.Remove(port);
    }
    nodeInfo.Rows.RemoveAt(rowIndex);

    // Update subsequent rows.
    for (var i = rowIndex; i < nodeInfo.Rows.Count; i++) {
      var row = nodeInfo.Rows[i];
      foreach (var port in node.Ports.Where(port => ReferenceEquals(((RowPortInfo) port.Tag!).Row, row)).ToList()) {
        var portInfo = (RowPortInfo) port.Tag!;
        graph.SetPortLocationParameter(port,
            CreatePortLocationParameter(i, portInfo.Incoming, (ListNodeStyle) node.Style));

        // Keep adjacent edges orthogonal.
        var edge = graph.Edges.FirstOrDefault(edge => edge.SourcePort == port || edge.TargetPort == port);
        if (edge is null) {
          continue;
        }
        var bend = portInfo.Incoming ? edge.Bends.LastOrDefault() : edge.Bends.FirstOrDefault();
        if (bend is not null) {
          graph.SetBendLocation(bend, new PointD(bend.Location.X, port.GetLocation().Y));
        } else {
          var source = edge.SourcePort.GetLocation();
          var target = edge.TargetPort.GetLocation();
          var x = source.X + (target.X - source.X) / 2;
          graph.AddBend(edge, new PointD(x, source.Y));
          graph.AddBend(edge, new PointD(x, target.Y));
        }
      }
    }
  }

  /// <summary>
  ///   Creates a port location parameter for the row with the given index that is anchored at the
  ///   owner nodes top left or top right corner.
  /// </summary>
  /// <param name="rowIndex">The index of the row to which the port belongs.</param>
  /// <param name="incoming">
  ///   If <see langword="true"/> the port will be anchored that the owner node's top left
  ///   corner; otherwise it will be anchored in the top right corner.
  /// </param>
  /// <param name="style">The owner node's node style.</param>
  private static IPortLocationModelParameter
      CreatePortLocationParameter(int rowIndex, bool incoming, ListNodeStyle style) =>
      FreeNodePortLocationModel.Instance.CreateParameter(new PointD(incoming ? 0 : 1, 0),
          new PointD(0, style.GetRowCenterY(rowIndex)));

  /// <summary>
  ///   Configures default styles and behavior for the graph.
  /// </summary>
  private static void ConfigureGraph(IGraph graph) {
    var edgeBrush = CreateFrozenBrush(0x2C, 0x4B, 0x52);
    var edgePen = new Pen(edgeBrush, 3) {
        StartLineCap = PenLineCap.Round,
        EndLineCap = PenLineCap.Round
    };
    edgePen.Freeze();
    graph.EdgeDefaults.Style = new PolylineEdgeStyle {
        Pen = edgePen,
        SourceArrow = Arrow.None,
        TargetArrow =
            new Arrow(ArrowType.Stealth, edgeBrush, lengthScale: 1.5, widthScale: 1.5, cropLength: 6, cropAtPort: true),
        SmoothingLength = 50,
        OrthogonalEditing = true
    };
    graph.EdgeDefaults.Labels.Style = new LabelStyle {
        TextBrush = Brushes.Black,
        BackgroundPen = new Pen(Brushes.Black, 1),
        BackgroundBrush = Brushes.White,
        Padding = new InsetsD(5)
    };

    // Do not remove ports when their adjacent edges are removed.
    graph.NodeDefaults.Ports.AutoCleanUp = false;

    // Disallow moving the main node label.
    var compositeLabelModel = nodeLabelModel;
    graph.NodeDefaults.Labels.LayoutParameter = compositeLabelModel.Parameters.First();
  }

  private static readonly CompositeLabelModel nodeLabelModel = CreateNodeLabelModel();

  private static CompositeLabelModel CreateNodeLabelModel() {
    var model = new CompositeLabelModel();
    model.AddParameter(StretchNodeLabelModel.Top);
    return model;
  }


  /// <summary>
  ///   Creates the initial sample graph.
  /// </summary>
  private static void CreateSampleGraph(IGraph graph) {
    // Create the sample graph.
    var n1 = CreateSampleNode(graph, new PointD(0, 0), "Node 1", [
        new RowInfo("in 0", "out 0"),
        new RowInfo(null, "out 1"),
        new RowInfo("in 2", "out 2"),
        new RowInfo(null, "out 3")
    ]);
    var n2 = CreateSampleNode(graph, new PointD(400, 0), "Node 2", [
        new RowInfo("in 0", null),
        new RowInfo("in 1", "out 1"),
        new RowInfo("in 2", null),
        new RowInfo("in 3", "out 3")
    ]);

    var out0 = n1.Ports.ElementAt(1);
    var in0 = n2.Ports.ElementAt(0);
    var out1 = n1.Ports.ElementAt(2);
    var in2 = n2.Ports.ElementAt(4);
    var edge1 = graph.CreateEdge(out0, in0);
    graph.AddBends(edge1, [new PointD(150, out0.GetLocation().Y), new PointD(150, in0.GetLocation().Y)]);
    var edge2 = graph.CreateEdge(out1, in2);
    graph.AddBends(edge2, [new PointD(250, out1.GetLocation().Y), new PointD(250, in2.GetLocation().Y)]);
  }

  private static INode CreateSampleNode(IGraph graph, PointD location, string label, IEnumerable<RowInfo> rows) {
    var info = new NodeInfo(label);
    var node = graph.CreateNode(new RectD(location.X - 50, location.Y - 100, 100, 200), new ListNodeStyle(), info);
    graph.AddLabel(node, label, nodeLabelModel.Parameters.First(), CreateNodeLabelStyle());
    foreach (var row in rows) {
      AddRow(graph, node, row);
    }
    return node;
  }

  /// <summary>
  ///   Registers a context menu for adding and removing rows.
  /// </summary>
  private void RegisterContextMenu(GraphEditorInputMode inputMode) {
    inputMode.ContextMenuItems = GraphItemTypes.Node;
    inputMode.PopulateItemContextMenu += (_, args) => {
      if (args.Handled || args.Item is not INode node || node.Style is not ListNodeStyle style) {
        return;
      }

      var rowIndex = style.GetRowIndex(node, args.QueryLocation);
      args.Menu.Items.Clear();
      args.Menu.Items.Add(CreateMenuItem("Add input",
          () => AddRow(Graph, node, new RowInfo($"in {node.Ports.Count}", null))));
      args.Menu.Items.Add(CreateMenuItem("Add output",
          () => AddRow(Graph, node, new RowInfo(null, $"out {node.Ports.Count}"))));
      args.Menu.Items.Add(CreateMenuItem("Add input/output",
          () => AddRow(Graph, node, new RowInfo($"in {node.Ports.Count}", $"out {node.Ports.Count}"))));
      if (rowIndex >= 0) {
        args.Menu.Items.Add(CreateMenuItem("Remove row", () => RemoveRow(Graph, node, rowIndex)));
      }

      args.ShowMenu = true;
      args.Handled = true;
    };
  }

  private static MenuItem CreateMenuItem(string label, Action action) {
    var menuItem = new MenuItem { Header = label };
    menuItem.Click += (_, _) => action();
    return menuItem;
  }

  /// <summary>
  ///   Reroutes edges affected by edge creation or a drag operation.
  /// </summary>
  private async Task RerouteEdges(IReadOnlyList<IEdge> affectedEdges) {
    if (affectedEdges.Count == 0) {
      return;
    }
    var router = new EdgeRouter();
    var data = router.CreateLayoutData(Graph);
    data.Scope.Edges.Source = affectedEdges;
    var executor = new LayoutExecutor(GraphControl, router) {
        LayoutData = data,
        AnimateViewport = false,
        AnimationDuration = TimeSpan.FromMilliseconds(500),
        PortPlacementPolicies = { MapperFunction = _ => PortPlacementPolicy.KeepParameter }
    };
    await executor.Start();
  }

  private static Brush CreateFrozenBrush(byte red, byte green, byte blue) {
    var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
    brush.Freeze();
    return brush;
  }

  [STAThread]
  private static void Main() => Run<ListNodeDemo>();
}

/// <summary>
///   Model data for a node: its name, rows, and the row currently being dragged.
/// </summary>
public sealed class NodeInfo(string name)
{
  public string Name { get; set; } = name;

  public List<RowInfo> Rows { get; } = [];

  public int? DraggingIndex { get; set; }
}

/// <summary>
///   Model data for a row and its optional incoming and outgoing labels.
/// </summary>
public sealed class RowInfo(string? incoming, string? outgoing)
{
  public string? In { get; set; } = incoming;

  public string? Out { get; set; } = outgoing;
}

/// <summary>
///   Identifies the row represented by a port and the direction it supports.
/// </summary>
public sealed record RowPortInfo(RowInfo Row, bool Incoming);
