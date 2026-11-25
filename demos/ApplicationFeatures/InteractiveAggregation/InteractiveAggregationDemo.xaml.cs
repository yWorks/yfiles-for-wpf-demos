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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Demo.Aggregation;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;

namespace yWorks.Demo.InteractiveAggregation
{
  /// <summary>
  ///   Shows how to analyze a graph by interactively aggregating nodes with common properties.
  /// </summary>
  [Demo("interactive-aggregation", "application-features", "Interactive Aggregation",
      Summary = "Shows how to analyze a graph by interactively aggregating nodes with common properties.",
      Keywords = ["aggregation graph wrapper", "context menu", "clusters", "data analysis"],
      Tags = ["large graph", "exploration"])]
  public sealed partial class InteractiveAggregationDemo
  {
    private static readonly Color BlueFillColor = Color.FromRgb(0x67, 0xB7, 0xDC);
    private static readonly Color GreenFillColor = Color.FromRgb(0x17, 0x7E, 0x89);
    private static readonly Color PurpleFillColor = Color.FromRgb(0xAA, 0x45, 0x86);
    private static readonly Color BlueBorderColor = Color.FromRgb(0x61, 0x79, 0x84);
    private static readonly Color GreenBorderColor = Color.FromRgb(0x30, 0x4F, 0x52);
    private static readonly Color PurpleBorderColor = Color.FromRgb(0x66, 0x48, 0x5B);
    private static readonly SolidColorBrush GrayBorderBrush = new(Color.FromRgb(0x77, 0x77, 0x6E));
    private static readonly Pen GrayBorder = new(GrayBorderBrush, 2);
    private AggregateGraphWrapper aggregateGraph = null!;

    public InteractiveAggregationDemo() {
      InitializeComponent();
      Unloaded += (_, _) => aggregateGraph?.Dispose();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override async void Initialize() {
      // Initialize the demo styles
      DemoStyles.InitDemoStyles(Graph);
      Graph.NodeDefaults.Size = new SizeD(40, 40);

      // Build and layout the graph with the given data set
      BuildGraph(Graph);
      await RunLayoutAsync();

      // Finally, enable the undo engine. This prevents undoing of the graph creation
      Graph.SetUndoEngineEnabled(true);

      // Create and configure a new AggregateGraphWrapper
      aggregateGraph = new AggregateGraphWrapper(Graph);

      // Set default label text sizes for aggregation labels
      aggregateGraph.AggregationNodeDefaults.Labels.Style = new LabelStyle { TextSize = 28 };
      aggregateGraph.AggregationEdgeDefaults.Labels.Style = new LabelStyle {
          TextSize = 18,
          Shape = LabelShape.Pill,
          BackgroundBrush = new SolidColorBrush(Color.FromArgb(204, 255, 255, 255)),
          Padding = new InsetsD(4)
      };

      // Assign it to the graph control
      GraphControl.Graph = aggregateGraph;

      // Disable edge cropping, so thick aggregation edges run smoothly into nodes
      GraphControl.Graph.GetDecorator().Ports.EdgePathCropper.Hide();

      // Don't create edges in both directions when replacing edges by aggregation edges
      aggregateGraph.EdgeReplacementPolicy = EdgeReplacementPolicy.Undirected;

      ConfigureContextMenu();
      RegisterAggregationCallbacks();
      await GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Initializes the context menu for graph nodes and the canvas background.
    /// </summary>
    private void ConfigureContextMenu() {
      var inputMode = new GraphViewerInputMode { ContextMenuItems = GraphItemTypes.Node };
      // Populate the context menu according to the hit item, or cancel showing a menu.
      inputMode.PopulateItemContextMenu += (_, args) => PopulateContextMenu(args);
      GraphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Populates the context menu with actions for the clicked node or canvas.
    /// </summary>
    private void PopulateContextMenu(PopulateItemContextMenuEventArgs<IModelItem> args) {
      // First update the selection
      var node = args.Item as INode;
      // If the cursor is over a node select it, else clear selection
      UpdateSelection(node);

      // Populate the context menu items
      var menu = args.Menu;
      var selectedNodes = GraphControl.Selection.Nodes.ToList();
      if (selectedNodes.Count > 0) {
        // Only allow aggregation operations on nodes that are not aggregation nodes already
        var aggregateAllowed = selectedNodes.Any(n => !aggregateGraph.IsAggregationItem(n));
        if (aggregateAllowed) {
          // Add aggregation menu items
          AddMenuItem(menu, "Aggregate Nodes with Same Shape",
              () => AggregateSame(selectedNodes, ShapeSelector, CreateShapeStyle));
          AddMenuItem(menu, "Aggregate Nodes with Same Color",
              () => AggregateSame(selectedNodes, FillColorSelector, CreateFillStyle));
          AddMenuItem(menu, "Aggregate Nodes with Same Shape & Color",
              () => AggregateSame(selectedNodes, ShapeAndFillSelector, CreateShapeAndFillStyle));
        }

        var separateAllowed = selectedNodes.Any(aggregateGraph.IsAggregationItem);
        if (separateAllowed) {
          AddMenuItem(menu, "Separate", () => Separate(selectedNodes));
        }
      } else {
        // Add generic aggregate / separate menu items
        AddMenuItem(menu, "Aggregate All Nodes by Shape", () => AggregateAll(ShapeSelector, CreateShapeStyle));
        AddMenuItem(menu, "Aggregate All Nodes by Color", () => AggregateAll(FillColorSelector, CreateFillStyle));
        AddMenuItem(menu, "Aggregate All Nodes by Shape & Color",
            () => AggregateAll(ShapeAndFillSelector, CreateShapeAndFillStyle));

        var separateAllowed = Graph.Nodes.Any(aggregateGraph.IsAggregationItem);
        if (separateAllowed) {
          AddMenuItem(menu, "Separate All", () => {
            aggregateGraph.SeparateAll();
            _ = RunLayoutAsync();
          });
        }
      }

      if (menu.Items.Count > 0) {
        args.ShowMenu = true;
        args.Handled = true;
      }
    }

    private static void AddMenuItem(ContextMenu menu, string header, Action action) {
      var menuItem = new MenuItem { Header = header };
      menuItem.Click += (_, _) => action();
      menu.Items.Add(menuItem);
    }

    /// <summary>
    ///   Updates the node selection state when the context menu is opened on the node.
    ///   If the node is null, the selection is cleared.
    ///   If the node is already selected, the existing multi-selection is kept.
    /// </summary>
    /// <param name="node">The node to consider for the selection state.</param>
    private void UpdateSelection(INode? node) {
      // See if no node was hit
      if (node == null) {
        // Clear the whole selection
        GraphControl.Selection.Clear();
      } else if (!GraphControl.Selection.Nodes.Contains(node)) {
        // The node was not selected, so clear the remaining selection
        GraphControl.Selection.Clear();
        // Select the node
        GraphControl.Selection.Nodes.Add(node);
        // Also update the current item
        GraphControl.CurrentItem = node;
      }
    }

    private void RegisterAggregationCallbacks() {
      Graph.NodeCreated += (_, args) => {
        if (aggregateGraph.IsAggregationItem(args.Item)) {
          // Add a label with the number of aggregated items to the new aggregation node
          Graph.AddLabel(args.Item,
              aggregateGraph.GetAggregatedItems(args.Item).Count.ToString(CultureInfo.InvariantCulture),
              InteriorNodeLabelModel.Center);
        }
      };

      Graph.EdgeCreated += (_, args) => {
        var edge = args.Item;
        if (!aggregateGraph.IsAggregationItem(edge)) {
          return;
        }

        // Add a label with the number of all original aggregated edges represented by the new aggregation edge
        var aggregatedEdgesCount = aggregateGraph.GetAllAggregatedOriginalItems(edge).Count;

        // Set the thickness to the number of aggregated edges
        Graph.SetStyle(edge, new PolylineEdgeStyle { Pen = new Pen(Brushes.Gray, Math.Max(2, aggregatedEdgesCount)) });

        if (aggregatedEdgesCount > 1) {
          Graph.AddLabel(edge, aggregatedEdgesCount.ToString(CultureInfo.InvariantCulture),
              NinePositionsEdgeLabelModel.CenterAbove);
        }
      };
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      var jsonDocument = JsonDocument.Parse(LoadAssemblyResource("graph-data.json"));
      BuildGraphHelper.BuildGraph(graph, jsonDocument, typeof(NodeData));
      foreach (var node in graph.Nodes) {
        graph.SetStyle(node, GetNodeStyle(((NodeData) node.Tag!).Tag));
      }
    }

    private static ShapeNodeStyle GetNodeStyle(string tag) {
      var (theme, borderColor) = tag.StartsWith("blue", StringComparison.Ordinal)
          ?
          (Themes.Palette31, BlueBorderColor)
          : tag.StartsWith("green", StringComparison.Ordinal)
              ? (Themes.Palette48, GreenBorderColor)
              : (Themes.Palette47, PurpleBorderColor);
      var shape = tag.EndsWith("Rectangle", StringComparison.Ordinal) ? ShapeNodeShape.Rectangle :
          tag.EndsWith("Octagon", StringComparison.Ordinal) ? ShapeNodeShape.Octagon : ShapeNodeShape.Diamond;

      var style = DemoStyles.CreateDemoShapeNodeStyle(shape, theme);
      style.Pen = new Pen(new SolidColorBrush(borderColor), 1);
      return style;
    }

    // Selectors for shape and/or color
    private static ShapeNodeShape ShapeSelector(INode node) => ((ShapeNodeStyle) node.Style).Shape;

    private static Color FillColorSelector(INode node) {
      if (((ShapeNodeStyle) node.Style).Brush is SolidColorBrush brush) {
        return brush.Color;
      }

      throw new InvalidOperationException("The node style does not have a solid fill.");
    }

    private static ShapeAndFill ShapeAndFillSelector(INode node) => new(ShapeSelector(node), FillColorSelector(node));

    // Style factories for aggregation nodes
    private static ShapeNodeStyle CreateShapeStyle(ShapeNodeShape shape) =>
        CreateAggregationNodeStyle(shape, Themes.Palette44);

    private static ShapeNodeStyle CreateFillStyle(Color fillColor) =>
        CreateAggregationNodeStyle(ShapeNodeShape.Ellipse, GetThemeForFill(fillColor));

    private static ShapeNodeStyle CreateShapeAndFillStyle(ShapeAndFill shapeAndFill) =>
        CreateAggregationNodeStyle(shapeAndFill.Shape, GetThemeForFill(shapeAndFill.FillColor));

    private static ShapeNodeStyle CreateAggregationNodeStyle(ShapeNodeShape shape, Palette theme) {
      var style = DemoStyles.CreateDemoShapeNodeStyle(shape, theme);
      style.Pen = GrayBorder;
      return style;
    }

    private static Palette GetThemeForFill(Color fillColor) {
      if (fillColor == BlueFillColor) {
        return Themes.Palette31;
      }

      if (fillColor == GreenFillColor) {
        return Themes.Palette48;
      }

      if (fillColor == PurpleFillColor) {
        return Themes.Palette47;
      }

      throw new InvalidOperationException("The node fill color does not have a matching theme.");
    }

    /// <summary>
    ///   For all passed nodes, aggregates all nodes that match the given node by the selector.
    ///   After the aggregation a layout calculation is run.
    /// </summary>
    /// <param name="nodes">The nodes to aggregate by.</param>
    /// <param name="selector">The selector function to use for aggregation.</param>
    /// <param name="styleFactory">The style factory to use for the aggregation node.</param>
    private void AggregateSame<TKey>(
        IEnumerable<INode> nodes,
        Func<INode, TKey> selector,
        Func<TKey, ShapeNodeStyle> styleFactory) {
      // Get one representative of each kind of node (determined by the selector) ignoring aggregation nodes
      var distinctNodes = nodes.Where(node => !aggregateGraph.IsAggregationItem(node)).GroupBy(selector)
          .Select(group => group.First()).ToList();

      foreach (var node in distinctNodes) {
        // Aggregate all nodes of the same kind as the representative node
        var nodesOfSameKind = CollectNodesOfSameKind(node, selector);
        Aggregate(nodesOfSameKind, selector(node), styleFactory);
      }

      _ = RunLayoutAsync();
    }

    /// <summary>
    ///   Collects all un-aggregated nodes that match the kind of node by the selector.
    /// </summary>
    /// <param name="node">The node to match.</param>
    /// <param name="selector">The selector function to use for the node matching.</param>
    private List<INode> CollectNodesOfSameKind<TKey>(INode node, Func<INode, TKey> selector) {
      var nodeKind = selector(node);
      return [
          .. Graph.Nodes.Where(candidate => !aggregateGraph.IsAggregationItem(candidate)).Where(candidate =>
              EqualityComparer<TKey>.Default.Equals(selector(candidate), nodeKind))
      ];
    }

    /// <summary>
    ///   Aggregates all nodes of the original graph by the selector and runs the layout.
    ///   Before aggregating the nodes, all existing aggregations are separated.
    ///   <see cref="AggregateGraphWrapper.SeparateAll"/>
    /// </summary>
    private void AggregateAll<TKey>(Func<INode, TKey> selector, Func<TKey, ShapeNodeStyle> styleFactory) {
      // Separate all previous aggregations first
      aggregateGraph.SeparateAll();

      foreach (var group in Graph.Nodes.GroupBy(selector)) {
        Aggregate([.. group], group.Key, styleFactory);
      }

      _ = RunLayoutAsync();
    }

    /// <summary>
    ///   Aggregates the nodes to a new aggregation node. Creation callbacks add labels with the
    ///   number of aggregated nodes and represented edges.
    /// </summary>
    /// <param name="nodes">The nodes to aggregate.</param>
    /// <param name="key">The key to use for the aggregation.</param>
    /// <param name="styleFactory">The style factory to use for the aggregation node.</param>
    private void Aggregate<TKey>(IReadOnlyList<INode> nodes, TKey key, Func<TKey, ShapeNodeStyle> styleFactory) {
      var size = Math.Sqrt(Graph.NodeDefaults.Size.Width * nodes.Count) * 5;
      var layout = new RectD(PointD.Origin, new SizeD(size, size));
      aggregateGraph.Aggregate(nodes, layout, styleFactory(key));
    }

    /// <summary>
    ///   Separates all given aggregation nodes and runs the layout afterward.
    /// </summary>
    /// <param name="nodes">The nodes to separate.</param>
    private void Separate(IEnumerable<INode> nodes) {
      foreach (var node in nodes) {
        if (aggregateGraph.IsAggregationItem(node)) {
          aggregateGraph.Separate(node);
        }
      }

      _ = RunLayoutAsync();
    }

    /// <summary>
    ///   Runs an organic layout with edge labeling.
    /// </summary>
    private async Task RunLayoutAsync() {
      var layout = new OrganicLayout {
          DefaultMinimumNodeDistance = 80,
          DefaultPreferredEdgeLength = 100,
          GenericLabeling = {
              Enabled = true,
              Scope = LabelingScope.EdgeLabels,
              DefaultEdgeLabelingCosts = { AmbiguousPlacementCost = 1.0 }
          }
      };

      await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///   Helper value for aggregation by shape and fill.
    /// </summary>
    private sealed record ShapeAndFill(ShapeNodeShape Shape, Color FillColor);

    [STAThread]
    private static void Main() {
      Run<InteractiveAggregationDemo>();
    }
  }

  internal sealed class NodeData
  {
    public string Tag { get; set; } = null!;
  }
}
