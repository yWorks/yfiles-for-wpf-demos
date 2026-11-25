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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Labeling;
using yWorks.Layout.Radial;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;
using yWorks.Utils;

namespace yWorks.Demo.EdgeBundling
{
  /// <summary>Demonstrates edge bundling with several layout algorithms and strategies.</summary>
  [Demo("edge-bundling", "layout", "Edge Bundling",
      Summary = "Shows how edge bundling can be applied for reducing visual cluttering in dense graphs.",
      Tags = ["style", "curve", "layout"], Keywords = [
          "layoutfeatures", "context menu", "organic", "radial", "tree", "radial tree", "balloon", "bundles",
          "bundling", "bezier", "curves", "data analysis"
      ])]
  public partial class EdgeBundlingDemo
  {
    private readonly EdgeBundlingSettings settings = new();
    private GraphViewerInputMode? inputMode;

    /// <summary>
    ///   Holds the component index for each node. It is used to distinguish circles in disconnected
    ///   graphs, where multiple components may contain a circle with the same layout circle ID.
    /// </summary>
    private IMapper<INode, int>? componentsMap;

    /// <summary>Holds whether an edge has to be bundled or not.</summary>
    private readonly Dictionary<IEdge, bool> bundlesMap = [];

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>Creates the demo and binds the toolbar to its settings.</summary>
    public EdgeBundlingDemo() {
      InitializeComponent();
      DataContext = settings;
    }

    /// <summary>Initializes the interaction, graph styles, UI, and selected sample.</summary>
    protected override void Initialize() {
      // Create the input mode and enable hovering over nodes and edges.
      CreateInputMode();

      // set the default styles
      InitializeGraph();

      // wire up the UI
      InitializeUI();

      // load the sample graph and run the layout
      LoadSelectedSample();
    }

    /// <summary>
    ///   Creates the input mode.
    /// </summary>
    private void CreateInputMode() {
      inputMode = new GraphViewerInputMode { ContextMenuItems = GraphItemTypes.Node | GraphItemTypes.Edge };
      inputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Node | GraphItemTypes.Edge;
      inputMode.ItemHoverInputMode.HoverCursor = Cursors.Hand;
      inputMode.ItemHoverInputMode.HoveredItemChanged += OnHoveredItemChanged;

      // Add an event listener that populates the context menu according to the hit elements, or cancels showing a menu.
      inputMode.PopulateItemContextMenu += PopulateContextMenu;
      GraphControl.InputMode = inputMode;
      GraphControl.MaximumZoom = 2;
    }

    /// <summary>Highlights the hovered node or edge and its neighboring graph items.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="args">The event arguments containing the hovered item.</param>
    private void OnHoveredItemChanged(object? sender, HoveredItemChangedEventArgs args) {
      var highlights = GraphControl.Highlights;
      highlights.Clear();
      switch (args.Item) {
        case INode node:
          foreach (var edge in Graph.EdgesAt(node)) {
            highlights.Add(edge);
            highlights.Add(edge.GetSourceNode() == node ? edge.GetTargetNode() : edge.GetSourceNode());
          }
          highlights.Add(node);
          break;
        case IEdge edge:
          highlights.Add(edge);
          highlights.Add(edge.GetSourceNode());
          highlights.Add(edge.GetTargetNode());
          break;
      }
    }

    /// <summary>Populates the context menu based on the item under the pointer.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="args">The context-menu event arguments.</param>
    private void PopulateContextMenu(object? sender, PopulateItemContextMenuEventArgs<IModelItem> args) {
      if (args.Handled) {
        return;
      }

      // Prefer nodes when determining which item was hit.
      // Use custom hit testing to prefer nodes over edges at the same location.
      var hit = GraphControl.GraphModelManager.HitElementsAt(args.QueryLocation)
          .FirstOrDefault(item => item is INode or IEdge);
      if (hit is not (IEdge or INode)) {
        return;
      }

      var selectedEdges = new HashSet<IEdge>();
      if (hit is IEdge hitEdge) {
        // update the hit edge and all other possible selected edges
        foreach (var edge in GraphControl.Selection.Edges) {
          selectedEdges.Add(edge);
        }
        selectedEdges.Add(hitEdge);
      } else if (hit is INode hitNode) {
        // update the hit node and all other possible selected nodes and update their adjacent edges
        foreach (var node in GraphControl.Selection.Nodes.Append(hitNode)) {
          foreach (var edge in Graph.EdgesAt(node)) {
            selectedEdges.Add(edge);
          }
        }
      }

      var bundled = selectedEdges.Count(edge => bundlesMap.GetValueOrDefault(edge));
      var unbundled = selectedEdges.Count - bundled;
      if (unbundled > 0) {
        args.Menu.Items.Add(CreateMenuItem(hit is IEdge ? "Bundle Selected Edges" : "Bundle Edges At Selected Nodes",
            () => UpdateBundlingForSelectedEdges(selectedEdges, true)));
      }
      if (bundled > 0) {
        args.Menu.Items.Add(CreateMenuItem(
            hit is IEdge ? "Un-bundle Selected Edges" : "Un-bundle Edges At Selected Nodes",
            () => UpdateBundlingForSelectedEdges(selectedEdges, false)));
      }
      args.ShowMenu = args.Menu.Items.Count > 0;
    }

    /// <summary>Creates a context-menu item that invokes the specified action.</summary>
    /// <param name="label">The text shown in the menu.</param>
    /// <param name="action">The action to invoke when the item is clicked.</param>
    /// <returns>The configured menu item.</returns>
    private static MenuItem CreateMenuItem(string label, Action action) {
      var item = new MenuItem { Header = label };
      item.Click += (_, _) => action();
      return item;
    }

    /// <summary>Enables or disables edge bundling for the specified edges, then reapplies the layout.</summary>
    /// <param name="edges">The edges to update.</param>
    /// <param name="bundled"><see langword="true"/> to bundle the edges; otherwise, <see langword="false"/>.</param>
    private void UpdateBundlingForSelectedEdges(IEnumerable<IEdge> edges, bool bundled) {
      foreach (var edge in edges) {
        bundlesMap[edge] = bundled;
      }
      ApplyLayout();
    }

    /// <summary>Sets the default styles for the graph elements and initializes the highlight.</summary>
    private void InitializeGraph() {
      // Set the default styles for the graph elements.
      Graph.NodeDefaults.Style = new DemoNodeStyle();
      Graph.EdgeDefaults.Style = new ColoredDelegatingEdgeStyle();
      Graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Center;

      // Hide the selection indication because selected edges are highlighted by their edge style.
      var decorator = Graph.GetDecorator();
      decorator.Nodes.SelectionRenderer.Hide();
      decorator.Edges.SelectionRenderer.Hide();
      // Configure the node and edge highlighting.
      decorator.Nodes.HighlightRenderer.AddConstant(new NodeStyleIndicatorRenderer {
          NodeStyle = new DemoNodeStyle(Color.FromRgb(0x67, 0xb7, 0xdc)),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      });
      decorator.Edges.HighlightRenderer.AddConstant(new EdgeStyleIndicatorRenderer {
          EdgeStyle = new DemoEdgeStyle(8, Colors.DarkBlue, Colors.DodgerBlue),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      });

      // When a node is selected, select its adjacent edges as well.
      GraphControl.Selection.ItemAdded += (_, args) => {
        if (args.Item is INode node) {
          GraphControl.Selection.Add(node);
          foreach (var edge in Graph.EdgesAt(node)) {
            GraphControl.Selection.Add(edge);
          }
        }
      };
    }

    /// <summary>Connects the toolbar settings to sample loading, layout, and edge-style updates.</summary>
    private void InitializeUI() {
      settings.SelectedSample = settings.Samples[0];
      settings.SampleChanged += (_, _) => LoadSelectedSample();
      settings.StrengthChanged += (_, _) => ApplyLayout();
      settings.ColorSchemeChanged += (_, _) => UpdateEdgeStyles();
    }

    /// <summary>Loads the selected sample graph and runs its initial layout.</summary>
    private void LoadSelectedSample() {
      if (settings.SelectedSample is not { } sample || !IsLoaded) {
        return;
      }
      settings.IsEnabled = false;
      settings.BundlingStrength = sample.DefaultBundlingStrength;
      Graph.Clear();
      // Load and parse the sample graph from its embedded JSON resource.
      BuildGraphFromJson(sample.ResourceName, Graph);
      bundlesMap.Clear();
      foreach (var edge in Graph.Edges) {
        bundlesMap[edge] = true;
      }
      CalculateConnectedComponents();
      ApplyLayout();
    }

    /// <summary>Runs the layout matching the selected sample and toolbar settings.</summary>
    private void ApplyLayout() {
      if (settings.SelectedSample is not { } sample || Graph.Nodes.Count == 0) {
        return;
      }
      settings.IsEnabled = false;
      var algorithm = CreateLayout(sample.Id, out var layoutData);
      // Append curve fitting to approximate bundled edge paths with cubic Bezier curves.
      var curveFitting = new CurveFittingStage {
          CoreLayout = algorithm,
          MaximumError = 1
      };
      // Apply the layout synchronously and update the circular node sectors when applicable.
      Graph.ApplyLayout(curveFitting, layoutData);
      if (sample.Id is "single-cycle-layout" or "circular-layout" &&
          layoutData is CircularLayoutData<INode, IEdge, ILabel, ILabel> circularData) {
        UpdateNodeInformation(circularData);
      }
      GraphControl.FitGraphBounds();
      settings.IsEnabled = true;
    }

    /// <summary>Creates the layout algorithm and matching layout data for the selected sample.</summary>
    /// <param name="sampleId">The identifier of the selected layout sample.</param>
    /// <param name="layoutData">The layout data configured for edge bundling.</param>
    /// <returns>The layout algorithm for the selected sample.</returns>
    private ILayoutAlgorithm CreateLayout(string sampleId, out LayoutData<INode, IEdge, ILabel, ILabel> layoutData) {
      switch (sampleId) {
        case "single-cycle-layout":
        case "circular-layout": {
          var layout = CreateCircularLayout(sampleId == "single-cycle-layout");
          var data = layout.CreateLayoutData(Graph);
          data.EdgeBundleDescriptors.MapperFunction = CreateEdgeBundleDescriptor;
          layoutData = data;
          return layout;
        }
        case "radial-layout": {
          var layout = new RadialLayout();
          ConfigureEdgeBundling(layout);
          var data = layout.CreateLayoutData(Graph);
          data.EdgeBundleDescriptors.MapperFunction = CreateEdgeBundleDescriptor;
          layoutData = data;
          return layout;
        }
        case "radial-tree-layout": {
          var layout = CreateRadialTreeLayout();
          var data = layout.TreeReductionStage.CreateLayoutData(Graph);
          data.EdgeBundleDescriptors.MapperFunction = CreateEdgeBundleDescriptor;
          layoutData = data;
          return layout;
        }
        case "tree-layout": {
          var layout = CreateTreeLayout();
          var data = layout.TreeReductionStage.CreateLayoutData(Graph);
          data.EdgeBundleDescriptors.MapperFunction = CreateEdgeBundleDescriptor;
          layoutData = data;
          return layout;
        }
        default: {
          var strategy = sampleId switch {
              "voronoi" => BundledEdgeRouterStrategy.Voronoi,
              "force-directed" => BundledEdgeRouterStrategy.ForceDirected,
              _ => BundledEdgeRouterStrategy.Spanner
          };
          var router = new BundledEdgeRouter { Strategy = strategy };
          ConfigureEdgeBundling(router);
          layoutData = new BundledEdgeRouterData<INode, IEdge, ILabel, ILabel> {
              EdgeBundleDescriptors = { MapperFunction = CreateEdgeBundleDescriptor }
          };
          return new GenericLabeling {
              CoreLayout = router,
              Scope = LabelingScope.NodeLabels,
              QualityTimeRatio = 0.6
          };
        }
      }
    }

    /// <summary>Returns a descriptor that excludes an explicitly unbundled edge, or <see langword="null"/> for the default.</summary>
    /// <param name="edge">The edge whose bundling descriptor is requested.</param>
    /// <returns>A descriptor with bundling disabled, or <see langword="null"/> to use the default descriptor.</returns>
    private EdgeBundleDescriptor? CreateEdgeBundleDescriptor(IEdge edge) =>
        bundlesMap.GetValueOrDefault(edge) ? null : new EdgeBundleDescriptor { Bundled = false };

    /// <summary>Creates and configures the circular layout algorithm.</summary>
    /// <param name="singleCycle"><see langword="true"/> to arrange nodes in a single cycle.</param>
    /// <returns>The configured circular layout algorithm.</returns>
    private CircularLayout CreateCircularLayout(bool singleCycle) {
      var layout = new CircularLayout();
      if (singleCycle) {
        layout.PartitioningPolicy = PartitioningPolicy.SingleCycle;
        layout.PartitionDescriptor.MinimumNodeDistance = 0;
      }
      ConfigureEdgeBundling(layout);
      return layout;
    }

    /// <summary>Creates and configures the radial tree layout algorithm.</summary>
    /// <returns>The configured radial tree layout algorithm.</returns>
    private RadialTreeLayout CreateRadialTreeLayout() {
      var layout = new RadialTreeLayout();
      CreateTreeReductionStage(layout.TreeReductionStage);
      return layout;
    }

    /// <summary>Creates and configures the tree layout algorithm.</summary>
    /// <returns>The configured tree layout algorithm.</returns>
    private TreeLayout CreateTreeLayout() {
      var layout = new TreeLayout();
      if (layout.DefaultSubtreePlacer is SingleLayerSubtreePlacer placer) {
        placer.EdgeRoutingStyle = SingleLayerSubtreePlacerRoutingStyle.StraightLine;
      }
      CreateTreeReductionStage(layout.TreeReductionStage);
      return layout;
    }

    /// <summary>Configures the non-tree edge router and edge bundling for a tree reduction stage.</summary>
    /// <param name="stage">The tree reduction stage to configure.</param>
    private void CreateTreeReductionStage(TreeReductionStage stage) {
      stage.NonTreeEdgeRouter = new OrganicEdgeRouter();
      ConfigureEdgeBundling(stage);
    }

    /// <summary>Configures the bundling strength and default descriptor for a bundled edge router.</summary>
    /// <param name="layout">The bundled edge router to configure.</param>
    private void ConfigureEdgeBundling(BundledEdgeRouter layout) {
      // Configure the bundling strength and enable bundling by default.
      layout.EdgeBundling.BundlingStrength = settings.BundlingStrength;
      layout.EdgeBundling.DefaultBundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
    }

    /// <summary>Configures the bundling strength and default descriptor for a circular layout.</summary>
    /// <param name="layout">The circular layout to configure.</param>
    private void ConfigureEdgeBundling(CircularLayout layout) {
      layout.EdgeBundling.BundlingStrength = settings.BundlingStrength;
      layout.EdgeBundling.DefaultBundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
    }

    /// <summary>Configures the bundling strength and default descriptor for a radial layout.</summary>
    /// <param name="layout">The radial layout to configure.</param>
    private void ConfigureEdgeBundling(RadialLayout layout) {
      layout.EdgeBundling.BundlingStrength = settings.BundlingStrength;
      layout.EdgeBundling.DefaultBundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
    }

    /// <summary>Configures the bundling strength and default descriptor for a tree reduction stage.</summary>
    /// <param name="layout">The tree reduction stage to configure.</param>
    private void ConfigureEdgeBundling(TreeReductionStage layout) {
      layout.EdgeBundling.BundlingStrength = settings.BundlingStrength;
      layout.EdgeBundling.DefaultBundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
    }

    /// <summary>Updates circle membership data used by the circular-sector node style.</summary>
    /// <param name="layoutData">The layout data containing the circle identifiers for the nodes.</param>
    private void UpdateNodeInformation(CircularLayoutData<INode, IEdge, ILabel, ILabel> layoutData) {
      // Store the nodes that belong to each circle. Include the connected component in the key because
      // disconnected components can have circles with the same layout circle ID.
      var circleNodes = new Dictionary<string, List<INode>>();
      var circleCenters = new Dictionary<string, PointD>();
      foreach (var node in Graph.Nodes) {
        var circleId = layoutData.CircleIdsResult[node];
        var componentId = componentsMap![node];
        var id = $"{circleId} {componentId}";
        if (!circleNodes.TryGetValue(id, out var nodes)) {
          circleNodes[id] = nodes = [];
        }
        nodes.Add(node);
      }
      // Calculate the center of each circle from its first three nodes.
      foreach (var (circleId, nodes) in circleNodes) {
        if (nodes.Count > 2) {
          circleCenters[circleId] = CalculateCircleCenter(nodes);
        }
      }
      // Store each node's circle identifier, center, and circle size in its tag for the sector node style.
      foreach (var node in Graph.Nodes) {
        var circleId = layoutData.CircleIdsResult[node];
        var id = $"{circleId} {componentsMap![node]}";
        node.Tag = circleCenters.TryGetValue(id, out var center)
            ? new CircleNodeInfo(circleNodes[id].Count, center)
            : new CircleNodeInfo(0, PointD.Origin);
      }
      GraphControl.Invalidate();
    }

    /// <summary>Calculates the center of the circle passing through the first three nodes.</summary>
    /// <param name="nodes">A list whose first three nodes define the circle.</param>
    /// <returns>The center coordinates of the circle.</returns>
    private static PointD CalculateCircleCenter(IReadOnlyList<INode> nodes) {
      var p1 = nodes[0].Layout.GetCenter();
      var p2 = nodes[1].Layout.GetCenter();
      var p3 = nodes[2].Layout.GetCenter();
      var idet = 2 * (p1.X * p2.Y - p2.X * p1.Y - p1.X * p3.Y + p3.X * p1.Y + p2.X * p3.Y - p3.X * p2.Y);
      var a = p1.X * p1.X + p1.Y * p1.Y;
      var b = p2.X * p2.X + p2.Y * p2.Y;
      var c = p3.X * p3.X + p3.Y * p3.Y;
      var centerX = (a * (p2.Y - p3.Y) + b * (p3.Y - p1.Y) + c * (p1.Y - p2.Y)) / idet;
      var centerY = (a * (p3.X - p2.X) + b * (p1.X - p3.X) + c * (p2.X - p1.X)) / idet;
      return new PointD(centerX, centerY);
    }

    /// <summary>Calculates connected components when the selected circular layout uses component-specific circles.</summary>
    private void CalculateConnectedComponents() {
      if (settings.SelectedSample?.Id is "single-cycle-layout" or "circular-layout") {
        componentsMap = new ConnectedComponents().Run(Graph).NodeComponentIds;
      } else {
        componentsMap = null;
      }
    }

    /// <summary>Updates the default edge style and applies it to all existing edges.</summary>
    private void UpdateEdgeStyles() {
      IEdgeStyle style = settings.UseDirectionalColors ? new ColoredDelegatingEdgeStyle() : new DemoEdgeStyle();
      Graph.EdgeDefaults.Style = style;
      foreach (var edge in Graph.Edges) {
        Graph.SetStyle(edge, style);
      }
    }

    /// <summary>Runs the edge bundling demo.</summary>
    [STAThread]
    public static void Main() => Run<EdgeBundlingDemo>();
  }
}
