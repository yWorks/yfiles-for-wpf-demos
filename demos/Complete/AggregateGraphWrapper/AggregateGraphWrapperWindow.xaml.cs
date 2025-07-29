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
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Demo.yFiles.Aggregation;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;

namespace Demo.yFiles.Graph.AggregateGraphWrapperDemo
{
  public partial class AggregateGraphWrapperWindow
  {
    #region Helper functions to select nodes to aggregate and create the styles for aggregation nodes

    // selectors for shape and/or color
    private static readonly Func<INode, ShapeNodeShape> ShapeSelector = n => ((ShapeNodeStyle) n.Style).Shape;
    private static readonly Func<INode, Brush> BrushSelector = n => BrushHelper.Instance.Get(((ShapeNodeStyle) n.Style).Brush);

    private static readonly Func<INode, ShapeAndBrush> ShapeAndBrushSelector =
        n => new ShapeAndBrush(ShapeSelector(n), BrushSelector(n));

    private static readonly Pen GrayBorder = new() { Brush = Brushes.DimGray, Thickness = 2 };

    // style factories for aggregation nodes
    private static readonly Func<ShapeNodeShape, INodeStyle> ShapeStyle = shape =>
        new ShapeNodeStyle { Brush = Brushes.FloralWhite, Shape = shape, Pen = GrayBorder };

    private static readonly Func<Brush, INodeStyle> BrushStyle = brush =>
        new ShapeNodeStyle { Brush = brush, Shape = ShapeNodeShape.Ellipse, Pen = GrayBorder };

    private static readonly Func<ShapeAndBrush, INodeStyle> ShapeAndBrushStyle = shapeAndBrush =>
        new ShapeNodeStyle { Brush = shapeAndBrush.brush, Shape = shapeAndBrush.shape, Pen = GrayBorder };

    #endregion

    public void OnLoaded(object source, EventArgs args) {
      // create and configure a new AggregateGraphWrapper
      var aggregateGraph = new AggregateGraphWrapper(graphControl.Graph);

      // set default label text sizes for aggregation labels
      aggregateGraph.AggregationNodeDefaults.Labels.Style = new LabelStyle { TextSize = 28 };
      aggregateGraph.AggregationEdgeDefaults.Labels.Style = new LabelStyle {
          TextSize = 18,
          Shape = LabelShape.Pill,
          BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
          Padding = new InsetsD(4)
      };
      aggregateGraph.AggregationEdgeDefaults.Labels.LayoutParameter = FreeEdgeLabelModel.Instance.CreateParameter();

      // assign it to the graphControl
      graphControl.Graph = aggregateGraph;

      // disable edge cropping, so thick aggregation edges run smoothly into nodes
      Graph.GetDecorator().Ports.EdgePathCropper.Hide();

      // don't create edges in both directions when replacing edges by aggregation edges
      AggregateGraph.EdgeReplacementPolicy = EdgeReplacementPolicy.Undirected;

      RegisterContextMenuCallback();

      RegisterAggregationCallbacks();

      graphControl.ImportFromGraphML("Resources/SampleGraph.graphml");
    }

    #region ContextMenu

    private void RegisterContextMenuCallback() {
      graphViewerInputMode.ContextMenuItems = GraphItemTypes.Node;
      graphViewerInputMode.PopulateItemContextMenu += OnPopulateItemContextMenu;
    }

    /// <summary>
    ///   Fills the context menu with menu items based on the clicked node.
    /// </summary>
    private void OnPopulateItemContextMenu(object sender, PopulateItemContextMenuEventArgs<IModelItem> e) {
      // first update the selection
      var node = e.Item as INode;
      // if the cursor is over a node select it, else clear selection
      UpdateSelection(node);

      // Create the context menu items
      var selectedNodes = graphControl.Selection.Nodes;
      if (selectedNodes.Count > 0) {
        // only allow aggregation operations on nodes that are not aggregation nodes already
        var aggregateAllowed = selectedNodes.Any(n => !AggregateGraph.IsAggregationItem(n));

        var aggregateByShape =
            new MenuItem { Header = "Aggregate Nodes with Same Shape", IsEnabled = aggregateAllowed };
        aggregateByShape.Click += (o, args) => AggregateSame(selectedNodes.ToList(), ShapeSelector, ShapeStyle);
        e.Menu.Items.Add(aggregateByShape);

        var aggregateByColor =
            new MenuItem { Header = "Aggregate Nodes with Same Color", IsEnabled = aggregateAllowed };
        aggregateByColor.Click += (o, args) => AggregateSame(selectedNodes.ToList(), BrushSelector, BrushStyle);
        e.Menu.Items.Add(aggregateByColor);

        var aggregateByShapeAndColor = new MenuItem {
            Header = "Aggregate Nodes with Same Shape & Color", IsEnabled = aggregateAllowed
        };
        aggregateByShapeAndColor.Click += (o, args) =>
            AggregateSame(selectedNodes.ToList(), ShapeAndBrushSelector, ShapeAndBrushStyle);
        e.Menu.Items.Add(aggregateByShapeAndColor);

        var separateAllowed = selectedNodes.Any(n => AggregateGraph.IsAggregationItem(n));
        var separate = new MenuItem { Header = "Separate", IsEnabled = separateAllowed };
        separate.Click += (o, args) => Separate(selectedNodes.ToList());
        e.Menu.Items.Add(separate);
      } else {
        var aggregateByShape = new MenuItem { Header = "Aggregate All Nodes by Shape" };
        aggregateByShape.Click += (o, args) => AggregateAll(ShapeSelector, ShapeStyle);
        e.Menu.Items.Add(aggregateByShape);

        var aggregateByColor = new MenuItem { Header = "Aggregate All Nodes by Color" };
        aggregateByColor.Click += (o, args) => AggregateAll(BrushSelector, BrushStyle);
        e.Menu.Items.Add(aggregateByColor);

        var aggregateByShapeAndColor = new MenuItem { Header = "Aggregate All Nodes by Shape & Color" };
        aggregateByShapeAndColor.Click += (o, args) => AggregateAll(ShapeAndBrushSelector, ShapeAndBrushStyle);
        e.Menu.Items.Add(aggregateByShapeAndColor);

        var separateAllowed = Graph.Nodes.Any(n => AggregateGraph.IsAggregationItem(n));
        var separateAll = new MenuItem { Header = "Separate All", IsEnabled = separateAllowed };
        separateAll.Click += (o, args) => {
          AggregateGraph.SeparateToOriginalItems();
          RunLayout();
        };
        e.Menu.Items.Add(separateAll);
      }

      e.ShowMenu = true;
      e.Handled = true;
    }

    /// <summary>
    ///   Updates the node selection state when the context menu is opened on <paramref name="node"/>.
    /// </summary>
    /// <remarks>
    ///   If <paramref name="node"/> is <see langword="null"/>, the selection is cleared.
    ///   If <paramref name="node"/> is already selected, the selection keeps unchanged, otherwise the selection
    ///   is cleared and <paramref name="node"/> is selected.
    /// </remarks>
    /// <param name="node">The node or <see langword="null"/>.</param>
    private void UpdateSelection(INode node) {
      // see if no node was hit
      if (node == null) {
        // clear the whole selection
        graphControl.Selection.Clear();
      } else {
        // see if the node was selected, already and keep the selection in this case
        if (!graphControl.Selection.Nodes.Contains(node)) {
          // no - clear the remaining selection
          graphControl.Selection.Clear();
          // select the node
          graphControl.Selection.Nodes.Add(node);
          // also update the current item
          graphControl.CurrentItem = node;
        }
      }
    }

    #endregion

    #region Aggregation

    private void RegisterAggregationCallbacks() {
      Graph.NodeCreated += (sender, args) => {
        if (AggregateGraph.IsAggregationItem(args.Item)) {
          // add a label with the number of aggregated items to the new aggregation node
          Graph.AddLabel(args.Item, AggregateGraph.GetAggregatedItems(args.Item).Count.ToString());
        }
      };

      Graph.EdgeCreated += (sender, args) => {
        var edge = args.Item;
        if (!AggregateGraph.IsAggregationItem(edge)) {
          return;
        }

        // add a label with the number of all original aggregated edges represented by the new aggregation edge
        var aggregatedEdgesCount = AggregateGraph.GetAllAggregatedOriginalItems(edge).Count;
        if (aggregatedEdgesCount > 1) {
          Graph.AddLabel(edge, aggregatedEdgesCount.ToString());
        }

        // set the thickness to the number of aggregated edges
        Graph.SetStyle(edge,
            new PolylineEdgeStyle {
                Pen = new Pen { Thickness = Math.Max(2, aggregatedEdgesCount), Brush = Brushes.Gray }
            });
      };
    }

    /// <summary>
    ///   For all passed nodes, aggregates all nodes that match the given node by the selector.
    /// </summary>
    /// <remarks>
    ///   After the aggregation a layout calculation is run.
    /// </remarks>
    private void AggregateSame<TKey>(
        IList<INode> nodes,
        Func<INode, TKey> selector,
        Func<TKey, INodeStyle> styleFactory) {
      // get one representative of each kind of node (determined by the selector) ignoring aggregation nodes
      IList<INode> distinctNodes = nodes.Where(n => !AggregateGraph.IsAggregationItem(n)).GroupBy(selector)
          .Select(g => g.First()).ToList();
      foreach (var node in distinctNodes) {
        // aggregate all nodes of the same kind as the representing node
        var nodesOfSameKind = CollectNodesOfSameKind(node, selector);
        Aggregate(nodesOfSameKind, selector(node), styleFactory);
      }
      RunLayout();
    }

    /// <summary>
    ///   Collects all un-aggregated nodes that match the kind of <paramref name="node"/> by the selector.
    /// </summary>
    private IReadOnlyList<INode> CollectNodesOfSameKind<TKey>(INode node, Func<INode, TKey> selector) {
      var nodeKind = selector(node);
      return Graph.Nodes.Where(n => !AggregateGraph.IsAggregationItem(n)).Where(n => selector(n).Equals(nodeKind))
          .ToList();
    }

    /// <summary>
    ///   Aggregates all nodes of the original graph by the selector and runs the layout.
    /// </summary>
    /// <remarks>
    ///   Before aggregating the nodes, all existing aggregations are
    ///   <see cref="AggregateGraphWrapper.SeparateToOriginalItems">separated</see>.
    /// </remarks>
    private void AggregateAll<TKey>(Func<INode, TKey> selector, Func<TKey, INodeStyle> styleFactory) {
      AggregateGraph.SeparateToOriginalItems();

      foreach (var grouping in Graph.Nodes.GroupBy(selector).ToList()) {
        Aggregate(grouping.ToList(), grouping.Key, styleFactory);
      }

      RunLayout();
    }

    /// <summary>
    ///   Aggregates the nodes to a new aggregation node.
    /// </summary>
    /// <remarks>
    ///   Adds a label with the number of aggregated nodes and adds labels
    ///   to all created aggregation edges with the number of replaced original edges.
    /// </remarks>
    private void Aggregate<TKey>(IReadOnlyList<INode> nodes, TKey key, Func<TKey, INodeStyle> styleFactory) {
      var size = Math.Sqrt(Graph.NodeDefaults.Size.Width * nodes.Count) * 5;
      var layout = RectD.FromCenter(PointD.Origin, new SizeD(size, size));
      AggregateGraph.Aggregate(nodes, layout, styleFactory(key));
    }

    /// <summary>
    ///   Separates all <paramref name="nodes"/> and runs the layout afterward.
    /// </summary>
    private void Separate(IEnumerable<INode> nodes) {
      foreach (var child in nodes) {
        if (AggregateGraph.IsAggregationItem(child)) {
          AggregateGraph.Separate(child);
        }
      }
      RunLayout();
    }

    /// <summary>
    ///   Helper struct for aggregation by shape and color.
    /// </summary>
    private readonly struct ShapeAndBrush
    {
      public readonly ShapeNodeShape shape;
      public readonly Brush brush;

      public ShapeAndBrush(ShapeNodeShape shape, Brush brush) {
        this.shape = shape;
        this.brush = brush;
      }
    }

    /// <summary>
    ///    Provides a unique <see cref="SolidColorBrush"/> representative for its color to help with comparing
    ///    solid color brushes even though <see cref="Brush"/> (and <see cref="SolidColorBrush"/>) have no dedicated
    ///    <see cref="Object.Equals(object)"/> implementation.
    /// </summary>
    private sealed class BrushHelper
    {
      internal static readonly BrushHelper Instance = new BrushHelper();

      private readonly Dictionary<Color, Brush> cache = new Dictionary<Color, Brush>();

      private BrushHelper() {}

      internal Brush Get(Brush brush) {
        if (brush is not SolidColorBrush scb) {
          return brush;
        }
        if (cache.TryGetValue(scb.Color, out var representative)) {
          return representative;
        }
        cache[scb.Color] = scb;
        return scb;
      }
    }

    #endregion

    #region Layout

    /// <summary>
    ///   Runs an organic layout with edge labeling.
    /// </summary>
    private void RunLayout() {
      var layout = new OrganicLayout {
          DefaultMinimumNodeDistance = 80,
          DefaultPreferredEdgeLength = 100,
          GenericLabeling = {
              Enabled = true,
              Scope = LabelingScope.EdgeLabels,
              DefaultEdgeLabelingCosts = { AmbiguousPlacementCost = 1.0 }
          }
      };
      graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Convenience Properties

    public IGraph Graph => graphControl.Graph;

    public AggregateGraphWrapper AggregateGraph => (AggregateGraphWrapper) graphControl.Graph;

    #endregion

    #region Constructor

    public AggregateGraphWrapperWindow() {
      InitializeComponent();
    }

    #endregion
  }
}
