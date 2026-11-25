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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.EdgeRouterGrouping
{
  /// <summary>
  ///   Demonstrates edge and port grouping with the <see cref="EdgeRouter"/>.
  /// </summary>
  [Demo("edge-router-grouping", "layout", "EdgeRouter Grouping",
      Summary = "The EdgeRouter can group the paths or ports of edges.", Tags = ["layout", "edge routing"], Keywords = [
          "layoutfeatures", "edge groups", "port groups", "edge router", "polyline", "ports", "context menu", "bridges",
          "data analysis"
      ])]
  public sealed partial class EdgeRouterGroupingDemo
  {
    private GraphEditorInputMode? inputMode;
    private bool portGroupMode;
    private bool uiInitialized;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public EdgeRouterGroupingDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      ConfigureInteraction();
      ConfigureGraph();
      InitializeUI();
      await CreateSampleGraphAsync();
    }

    /// <summary>
    ///   Applies the <see cref="EdgeRouter"/> to the current graph including the edge/port group information in the edges'
    ///   tags.
    /// </summary>
    private async Task RunEdgeRouterAsync() {
      SetUIDisabled(true);
      try {
        var layout = new EdgeRouter();
        var layoutData = layout.CreateLayoutData(Graph);

        if (portGroupMode) {
          layoutData.Ports.SourcePortGroupIds.MapperFunction = edge => (edge.Tag as EdgeTag)?.SourceGroupId;
          layoutData.Ports.TargetPortGroupIds.MapperFunction = edge => (edge.Tag as EdgeTag)?.TargetGroupId;
        } else {
          layoutData.SourceGroupIds.MapperFunction = edge => (edge.Tag as EdgeTag)?.SourceGroupId;
          layoutData.TargetGroupIds.MapperFunction = edge => (edge.Tag as EdgeTag)?.TargetGroupId;
        }

        await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(700), layoutData);
      } finally {
        SetUIDisabled(false);
      }
    }

    private void ConfigureGraph() {
      Graph.GetDecorator().Edges.SelectionRenderer.AddConstant(new EdgeStyleIndicatorRenderer {
          EdgeStyle = new HighlightEdgeStyle(),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      });

      var bridgeManager = new BridgeManager {
          CanvasControl = GraphControl,
          DefaultBridgeCrossingStyle = BridgeCrossingStyle.Gap
      };
      bridgeManager.AddObstacleProvider(new GraphObstacleProvider());
    }

    private async Task CreateSampleGraphAsync() {
      Graph.Clear();
      DemoStyles.InitDemoStyles(Graph, extraCropLength: 0);
      Graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle(Themes.Palette44);
      Graph.NodeDefaults.Size = new SizeD(50, 30);
      var defaultEdgeBrush = new SolidColorBrush(ColorFromHex("#BBBBBB"));
      Graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(defaultEdgeBrush, 3),
          TargetArrow = new Arrow(brush: defaultEdgeBrush, type: ArrowType.Triangle),
          SmoothingLength = 15
      };
      Graph.EdgeDefaults.ShareStyleInstance = false;

      BuildGraphFromJson("graph-data.json", Graph, edgeTagType: typeof(EdgeData));

      foreach (var edge in Graph.Edges) {
        edge.Tag = ((EdgeData) edge.Tag!).GroupIds;
        UpdateStyles(edge);
      }

      await GraphControl.FitGraphBounds();
      await RunEdgeRouterAsync();
    }

    /// <summary>
    ///   Updates the selection when an item is right-clicked for a context menu.
    /// </summary>
    private void UpdateSelection(IModelItem? item) {
      var selection = GraphControl.Selection;
      if (item is null) {
        return;
      }
      if (!selection.Contains(item)) {
        selection.Clear();
        selection.Add(item);
      } else {
        if (item is IEdge) {
          selection.Nodes.Clear();
        } else {
          selection.Edges.Clear();
        }
        selection.Add(item);
      }
    }

    /// <summary>
    ///   Groups the given edges according to the type. In case <paramref name="override"/> is true, a new tag is set.
    /// </summary>
    private async Task GroupEdgesAsync(GroupingType type, IEnumerable<IEdge> edges, bool @override) {
      var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      foreach (var edge in edges) {
        var tag = new EdgeTag();
        switch (type) {
          case GroupingType.Source:
            tag.SourceGroupId = $"s {id} {HashCode(edge.GetSourceNode())}";
            break;
          case GroupingType.Target:
            tag.TargetGroupId = $"t {id} {HashCode(edge.GetTargetNode())}";
            break;
          case GroupingType.SourceAndTarget:
            tag.SourceGroupId = $"s {id} {HashCode(edge.GetSourceNode())}";
            tag.TargetGroupId = $"t {id} {HashCode(edge.GetTargetNode())}";
            break;
        }

        if (edge.Tag is not EdgeTag current || @override || type is GroupingType.Ungroup) {
          edge.Tag = tag;
        } else {
          current.SourceGroupId = tag.SourceGroupId ?? current.SourceGroupId;
          current.TargetGroupId = tag.TargetGroupId ?? current.TargetGroupId;
        }
        UpdateStyles(edge);
      }

      await RunEdgeRouterAsync();
    }

    /// <summary>
    ///   Returns the hash code of the given node.
    /// </summary>
    private static int HashCode(INode node) => node.GetHashCode();

    /// <summary>
    ///   Updates the styles to distinguish the different types of edge/port grouping.
    /// </summary>
    private void UpdateStyles(IEdge edge) {
      if (edge.Tag is not EdgeTag tag) {
        return; // the ungrouped edge default
      }

      var color = ColorFromHex("#BBBBBB");
      if (portGroupMode) {
        if (tag.SourceGroupId is not null && tag.TargetGroupId is not null) {
          color = ColorFromHex("#FFA500");
        } else if (tag.SourceGroupId is not null) {
          color = ColorFromHex("#DC143C");
        } else if (tag.TargetGroupId is not null) {
          color = ColorFromHex("#D2691E");
        }
      } else if (tag.SourceGroupId is not null && tag.TargetGroupId is not null) {
        color = ColorFromHex("#6495ED");
      } else if (tag.SourceGroupId is not null) {
        color = ColorFromHex("#4169E1");
      } else if (tag.TargetGroupId is not null) {
        color = ColorFromHex("#483D8B");
      }

      var brush = new SolidColorBrush(color);
      var portStyle = new ShapePortStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = brush,
          Pen = null,
          RenderSize = new SizeD(7, 7)
      };
      Graph.SetStyle(edge.SourcePort, tag.SourceGroupId is not null ? portStyle : VoidPortStyle.Instance);
      Graph.SetStyle(edge.TargetPort, tag.TargetGroupId is not null ? portStyle : VoidPortStyle.Instance);

      Graph.SetStyle(edge, new PolylineEdgeStyle {
          Pen = new Pen(brush, 3),
          TargetArrow = new Arrow(
              brush: brush, pen: new Pen(brush, 1), type: ArrowType.Triangle,
              cropLength: tag.TargetGroupId is not null ? 4 : 0.5),
          SmoothingLength = 15
      });
    }

    private static Color ColorFromHex(string value) => (Color) ColorConverter.ConvertFromString(value)!;

    /// <summary>
    ///   Sets a <see cref="GraphEditorInputMode"/> and initializes the context menu.
    /// </summary>
    private void ConfigureInteraction() {
      inputMode = new GraphEditorInputMode { SelectableItems = GraphItemTypes.Edge | GraphItemTypes.Node };
      inputMode.ContextMenuInputMode.ClearMenu = true;
      inputMode.PopulateItemContextMenu += PopulateItemContextMenu;
      GraphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Adds menu items to the context menu depending on what type of graph element was hit.
    /// </summary>
    private void PopulateItemContextMenu(object? sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      if (args.Handled) {
        return;
      }

      var item = args.Item;
      var selection = GraphControl.Selection;
      if (item is null && selection.Edges.Count > 0) {
        item = selection.Edges.First();
      }
      UpdateSelection(item);

      var menuItems = new List<(string Label, Func<Task> Action, Color? Accent)>();
      if (item is IEdge) {
        var selectedEdges = selection.Edges.ToArray();
        if (portGroupMode) {
          menuItems.Add(("Source Port Group", () => GroupEdgesAsync(GroupingType.Source, selectedEdges, true),
              ColorFromHex("#DC143C")));
          menuItems.Add(("Target Port Group", () => GroupEdgesAsync(GroupingType.Target, selectedEdges, true),
              ColorFromHex("#D2691E")));
          menuItems.Add(("Source and Target Port Group",
              () => GroupEdgesAsync(GroupingType.SourceAndTarget, selectedEdges, true), ColorFromHex("#FFA500")));
        } else {
          menuItems.Add(("Source Group", () => GroupEdgesAsync(GroupingType.Source, selectedEdges, true),
              ColorFromHex("#4169E1")));
          menuItems.Add(("Target Group", () => GroupEdgesAsync(GroupingType.Target, selectedEdges, true),
              ColorFromHex("#483D8B")));
          menuItems.Add(("Source and Target Group",
              () => GroupEdgesAsync(GroupingType.SourceAndTarget, selectedEdges, true), ColorFromHex("#6495ED")));
        }
        menuItems.Add(("Ungroup", () => GroupEdgesAsync(GroupingType.Ungroup, selectedEdges, true), null));
      } else if (item is INode) {
        var outgoingEdges = new HashSet<IEdge>();
        var incomingEdges = new HashSet<IEdge>();
        var incidentEdges = new HashSet<IEdge>();
        foreach (var node in selection.Nodes) {
          foreach (var edge in Graph.EdgesAt(node)) {
            incidentEdges.Add(edge);
            if (edge.GetSourceNode() == node) {
              outgoingEdges.Add(edge);
            }
            if (edge.GetTargetNode() == node) {
              incomingEdges.Add(edge);
            }
          }
        }
        if (portGroupMode) {
          menuItems.Add(("Port Group Outgoing Edges", () => GroupEdgesAsync(GroupingType.Source, outgoingEdges, false),
              ColorFromHex("#DC143C")));
          menuItems.Add(("Port Group Incoming Edges", () => GroupEdgesAsync(GroupingType.Target, incomingEdges, false),
              ColorFromHex("#D2691E")));
          menuItems.Add(("Port Group Incident Edges", async () => {
            await GroupEdgesAsync(GroupingType.Source, outgoingEdges, false);
            await GroupEdgesAsync(GroupingType.Target, incomingEdges, false);
          }, ColorFromHex("#FFA500")));
        } else {
          menuItems.Add(("Group Outgoing Edges", () => GroupEdgesAsync(GroupingType.Source, outgoingEdges, false),
              ColorFromHex("#4169E1")));
          menuItems.Add(("Group Incoming Edges", () => GroupEdgesAsync(GroupingType.Target, incomingEdges, false),
              ColorFromHex("#483D8B")));
          menuItems.Add(("Group Incident Edges",
              () => GroupEdgesAsync(GroupingType.SourceAndTarget, incidentEdges, false), ColorFromHex("#6495ED")));
        }
        menuItems.Add(("Ungroup Incident Edges", () => GroupEdgesAsync(GroupingType.Ungroup, incidentEdges, false),
            null));
      } else {
        var allEdges = Graph.Edges.ToArray();
        if (portGroupMode) {
          menuItems.Add(("Source Port Group All Edges", () => GroupEdgesAsync(GroupingType.Source, allEdges, true),
              ColorFromHex("#DC143C")));
          menuItems.Add(("Target Port Group All Edges", () => GroupEdgesAsync(GroupingType.Target, allEdges, true),
              ColorFromHex("#D2691E")));
          menuItems.Add(("Source and Target Port Group All Edges",
              () => GroupEdgesAsync(GroupingType.SourceAndTarget, allEdges, true), ColorFromHex("#FFA500")));
        } else {
          menuItems.Add(("Source Group All Edges", () => GroupEdgesAsync(GroupingType.Source, allEdges, true),
              ColorFromHex("#4169E1")));
          menuItems.Add(("Target Group All Edges", () => GroupEdgesAsync(GroupingType.Target, allEdges, true),
              ColorFromHex("#483D8B")));
          menuItems.Add(("Source and Target Group All Edges",
              () => GroupEdgesAsync(GroupingType.SourceAndTarget, allEdges, true), ColorFromHex("#6495ED")));
        }
        menuItems.Add(("Ungroup All Edges", () => GroupEdgesAsync(GroupingType.Ungroup, allEdges, true), null));
      }

      args.Menu.Items.Clear();
      foreach (var (label, action, accent) in menuItems) {
        args.Menu.Items.Add(CreateMenuItem(label, action, accent));
      }
      args.ShowMenu = args.Menu.Items.Count > 0;
      args.Handled = args.ShowMenu;
    }

    private static MenuItem CreateMenuItem(string label, Func<Task> action, Color? accent) {
      var header = new DockPanel();
      if (accent is { } color) {
        header.Children.Add(new Border {
            Width = 7,
            Margin = new Thickness(0, 0, 8, 0),
            Background = new SolidColorBrush(color)
        });
      }
      header.Children.Add(new TextBlock {
          Text = label,
          VerticalAlignment = VerticalAlignment.Center
      });

      var item = new MenuItem { Header = header };
      item.Click += async (_, _) => await action();
      return item;
    }

    /// <summary>
    ///   Binds the various actions to the buttons in the toolbar.
    /// </summary>
    private void InitializeUI() {
      groupModeComboBox.SelectedIndex = 0;
      resetButton.Click += async (_, _) => await CreateSampleGraphAsync();
      layoutButton.Click += async (_, _) => await RunEdgeRouterAsync();
      groupModeComboBox.SelectionChanged += async (_, _) => {
        if (!uiInitialized || groupModeComboBox.SelectedIndex < 0) {
          return;
        }
        portGroupMode = groupModeComboBox.SelectedIndex == 1;
        foreach (var edge in Graph.Edges) {
          UpdateStyles(edge);
        }
        await RunEdgeRouterAsync();
      };
      uiInitialized = true;
    }

    /// <summary>
    ///   Disables the UI while the layout is running.
    /// </summary>
    /// <param name="disabled">True if the controls should be disabled; false otherwise.</param>
    private void SetUIDisabled(bool disabled) {
      resetButton.IsEnabled = !disabled;
      layoutButton.IsEnabled = !disabled;
      groupModeComboBox.IsEnabled = !disabled;
      if (inputMode is not null) {
        inputMode.Enabled = !disabled;
      }
    }

    /// <summary>
    ///   An edge style to draw the selection highlight below the edge.
    /// </summary>
    private sealed class HighlightEdgeStyle : EdgeStyleBase<DrawingVisual>
    {
      protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
        var visual = new DrawingVisual();
        Render(context, visual, edge);
        return visual;
      }

      protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, IEdge edge) {
        Render(context, oldVisual, edge);
        return oldVisual;
      }

      private void Render(IRenderContext context, DrawingVisual visual, IEdge edge) {
        if (edge.Style is not PolylineEdgeStyle style || style.Pen?.Brush is not SolidColorBrush edgeBrush) {
          return;
        }

        var edgePath = GetPath(edge);
        if (edgePath is null) {
          return;
        }
        var croppedPath = CropPath(edge, style.SourceArrow, style.TargetArrow, edgePath);
        if (croppedPath is null) {
          return;
        }
        var path = croppedPath.CreateSmoothedPath(15);
        var highlightBrush = new SolidColorBrush(edgeBrush.Color) { Opacity = 0.75 };
        var highlightPen = new Pen(highlightBrush, 6);
        using var drawing = visual.RenderOpen();
        path.Render(drawing, null!, highlightPen);
      }
    }

    private sealed record EdgeData(EdgeTag GroupIds);

    private sealed class EdgeTag
    {
      public string? SourceGroupId { get; set; }

      public string? TargetGroupId { get; set; }

      public string? SourcePortGroupId { get; set; }

      public string? TargetPortGroupId { get; set; }
    }

    private enum GroupingType
    {
      Source,
      Target,
      SourceAndTarget,
      Ungroup
    }

    [STAThread]
    private static void Main() => Run<EdgeRouterGroupingDemo>();
  }
}
