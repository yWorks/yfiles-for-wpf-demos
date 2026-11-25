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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.BusRouting
{
  /// <summary>
  ///   Demonstrates how to create bus structures using the <see cref="EdgeRouter"/>.
  /// </summary>
  [Demo("bus-routing", "layout", "Bus Routing", Summary = "Shows how to group edges in bus structures.",
      Tags = ["layout", "edge routing", "bus"],
      Keywords = ["layoutfeatures", "edge router", "edge groups", "bus structures"])]
  public sealed partial class BusRoutingDemo
  {
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    ///   Provides different colors for new edge buses in this demo.
    /// </summary>
    private sealed class ColorUtil
    {
      private readonly List<Color> fills = CreateFills();
      private int index;

      /// <summary>
      ///   Returns a color not yet in use.
      /// </summary>
      public Color NextColor() {
        if (index >= fills.Count) {
          var r = (byte) Random.Shared.Next(150);
          var g = (byte) Random.Shared.Next(150);
          var b = (byte) Random.Shared.Next(150);
          fills.Add(Color.FromRgb(r, g, b));
        }

        return fills[index++];
      }

      /// <summary>
      ///   Returns the colors already in use.
      /// </summary>
      public IEnumerable<Color> UsedColors() => fills.Take(index);

      /// <summary>
      ///   Creates an initial set of colors for new edges.
      /// </summary>
      private static List<Color> CreateFills() => [
          Color.FromRgb(0xAB, 0x23, 0x46), // sample graph edge color
          Color.FromRgb(0x66, 0x2b, 0x00), // sample graph edge color
          Color.FromRgb(0x0B, 0x71, 0x89), // sample graph edge color
          Color.FromRgb(0x11, 0x1D, 0x4A),
          Color.FromRgb(0x17, 0xBE, 0xBB),
          Color.FromRgb(0xFF, 0xC9, 0x14),
          Color.FromRgb(0xFF, 0x6C, 0x00),
          Color.FromRgb(0x2E, 0x28, 0x2A),
          Color.FromRgb(0x76, 0xB0, 0x41)
      ];
    }

    private readonly ColorUtil colorUtil = new();
    private bool layoutRunning;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public BusRoutingDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override async void Initialize() {
      ConfigureUserInteraction();
      ConfigureGraph();
      LoadGraph(Graph);

      await GraphControl.FitGraphBounds(animated: false);
      await RouteEdges();
    }

    /// <summary>
    ///   Configures the demo's user interaction.
    /// </summary>
    private void ConfigureUserInteraction() {
      // Create an input mode for interactive graph editing.
      var mode = new GraphEditorInputMode {
          // Disable interactive edge creation.
          AllowCreateEdge = false,
          // Restrict marquee selection to nodes.
          MarqueeSelectableItems = GraphItemTypes.Node,
          // Disable interactive node resizing.
          ShowHandleItems = GraphItemTypes.Bend | GraphItemTypes.Edge,
          // Turn on default snapping for graph elements.
          SnapContext = new GraphSnapContext()
      };

      GraphControl.InputMode = mode;
    }

    /// <summary>
    ///   Configures default visualizations for the given graph.
    /// </summary>
    private void ConfigureGraph() {
      DemoStyles.InitDemoStyles(Graph, orthogonalEdgeEditing: true);

      // Increase the default node size to 50x50 pixels.
      Graph.NodeDefaults.Size = new SizeD(50, 50);
    }

    /// <summary>
    ///   Creates a sample graph structure from the demo's sample data.
    ///   The sample graph has edges in three different colors (red, brown, and blue)
    ///   and thus starts out with three edge buses.
    /// </summary>
    private void LoadGraph(IGraph graph) {
      // The style to be used for red, brown, and blue edges.
      var red = NewEdgeStyle();
      var brown = NewEdgeStyle();
      var blue = NewEdgeStyle();

      // The demo's sample graph data.
      var edgeTagType = typeof(EdgeData);
      BuildGraphFromJson("graph-data.json", graph, null, null, edgeTagType);

      // assign edge styles depending on the edge's color tag
      foreach (var edge in graph.Edges) {
        graph.SetStyle(edge, ((EdgeData) edge.Tag!).Tag.Color switch {
            "#AB2346" => red,
            "#662b00" => brown,
            _ => blue
        });
      }
    }

    /// <summary>
    ///   Triggers a new layout calculation for the edges associated to the given scope.
    /// </summary>
    /// <param name="edgesToRoute">
    ///   Determines which edges to route. If provided, only these edges are routed; otherwise all edges are routed.
    /// </param>
    private async Task RouteEdges(IReadOnlyList<IEdge>? edgesToRoute = null) {
      // Prevent concurrent layout calculations or animations.
      if (layoutRunning) {
        return;
      }

      layoutRunning = true;
      DisableUI(true);

      try {
        await RouteEdgesCore(edgesToRoute);
      } finally {
        layoutRunning = false;
        DisableUI(false);
      }
    }

    /// <summary>
    ///   Routes the edges associated to the given scope and animates the result.
    /// </summary>
    /// <param name="edgesToRoute">
    ///   Determines which edges to route. If provided, only these edges are routed; otherwise all edges are routed.
    /// </param>
    private async Task RouteEdgesCore(IReadOnlyList<IEdge>? edgesToRoute) {
      // Configure bus structures.
      var algorithm = new EdgeRouter();
      var layoutData = algorithm.CreateLayoutData(Graph);

      if (edgesToRoute is { Count: > 0 }) {
        // The affected edges are all the edges created in the last Connect Nodes action.
        // Mark those edges for routing ...
        layoutData.Scope.Edges.Source = edgesToRoute;
        // ... and assign those edges to a new edge bus.
        layoutData.Buses.Add(new BusDescriptor { MultipleBackboneSegments = false }).Items = [.. edgesToRoute];
      } else {
        // The affected edges are all edges.

        // Assign each edge to a bus depending on the edge's color.
        foreach (var color in colorUtil.UsedColors()) {
          var edgesOfColor = Graph.Edges.Where(edge => {
            var edgeStyle = edge.Style as PolylineEdgeStyle;
            return edgeStyle?.Pen?.Brush is SolidColorBrush edgeBrush && edgeBrush.Color == color;
          }).ToArray();
          layoutData.Buses.Add(new BusDescriptor { MultipleBackboneSegments = false }).Items = edgesOfColor;
        }
      }

      // Calculate the new edge routes and apply the result in an animated fashion.
      await GraphControl.ApplyLayoutAnimated(algorithm, layoutData: layoutData);
    }

    /// <summary>
    ///   Connects all the currently selected nodes and routes the new edges.
    ///   All new edges have the same color and thus are assigned to the same edge bus.
    /// </summary>
    private async Task ConnectNodes() {
      // Prevent previously created edges from being routed as well.
      var edgesToRoute = new List<IEdge>();

      // Determine the currently selected nodes.
      var nodes = GraphControl.Selection.Nodes.ToArray();
      var n = nodes.Length;
      if (n < 1) {
        return;
      }

      // Create a new style with a color not yet in use.
      var style = NewEdgeStyle();

      // Connect the selected nodes and assign each new edge the new style.
      for (var i = 0; i < n; ++i) {
        for (var j = i + 1; j < n; ++j) {
          edgesToRoute.Add(Graph.CreateEdge(nodes[i], nodes[j], style));
        }
      }

      // Route the new edges and only the new edges.
      await RouteEdges(edgesToRoute);
    }

    /// <summary>
    ///   Creates a new edge style with a color not yet in use.
    /// </summary>
    private PolylineEdgeStyle NewEdgeStyle() =>
        new() {
            Pen = new Pen(new SolidColorBrush(colorUtil.NextColor()), 2),
            // Since EdgeRouter creates and works with orthogonal edge routes,
            // ensure interactive edge editing keeps edges orthogonal.
            OrthogonalEditing = true
        };

    /// <summary>
    ///   Helper function to disable UI during layout animation.
    /// </summary>
    private void DisableUI(bool disabled) {
      connectButton.IsEnabled = !disabled;
      routeButton.IsEnabled = !disabled;
    }

    private async void OnRouteClicked(object sender, RoutedEventArgs args) {
      await RouteEdges();
    }

    private async void OnConnectClicked(object sender, RoutedEventArgs args) {
      await ConnectNodes();
    }

    private sealed record EdgeData(EdgeTagData Tag);

    private sealed record EdgeTagData(string Color);

    [STAThread]
    private static void Main() => Run<BusRoutingDemo>();
  }
}
