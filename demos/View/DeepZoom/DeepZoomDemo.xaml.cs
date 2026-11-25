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
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DeepZoom;

/// <summary>
///   Interaction logic for DeepZoomDemo.xaml
/// </summary>
[Demo("deep-zoom", "view", "Deep Zoom",
    Summary = "Seamlessly zoom into the contents of deeply nested group nodes, similar to \"deep zoom\" for images",
    Tags = ["view", "styles", "groups"], Keywords = ["group", "group style", "viewport", "folding", "zoom", "content"])]
public sealed partial class DeepZoomDemo
{
  private GraphControl graphComponent = null!;
  private IFoldingView foldingView = null!;

  /// <summary>
  ///   Bootstraps the demo.
  /// </summary>
  protected override void Initialize() {
    graphComponent = graphControl;

    // hide the scrollbars
    graphComponent.HorizontalScrollBarPolicy = ScrollBarVisibility.Hidden;
    graphComponent.VerticalScrollBarPolicy = ScrollBarVisibility.Hidden;

    InitializeGraphStyles(graphComponent);
    LoadSampleGraph();

    // Enable a managed folding view on the graph, instead of displaying all elements
    foldingView = EnableFolding(graphComponent);

    // apply different layouts to the individual layers
    DeepZoomLayout.ApplyDeepZoomLayout(foldingView);

    // initialize the input mode
    graphComponent.InputMode = new GraphViewerInputMode();

    // attach a viewport listener that adjusts the viewport and visible graph depending on zoom level
    DeepZoomUpdate.InitializeDeepZoom(graphComponent, foldingView);

    _ = graphComponent.FitGraphBounds();
  }

  /// <summary>
  ///   Registers a special click listener to the "fit content" button.
  /// </summary>
  private void OnFitGraphBoundsClick(object sender, RoutedEventArgs e) {
    DeepZoomUpdate.FitContent(graphComponent, foldingView);
  }

  /// <summary>
  ///   Sets a custom node style for the group nodes of the graph and
  ///   initializes the styles for the normal nodes.
  /// </summary>
  private static void InitializeGraphStyles(GraphControl graphComponent) {
    var graph = graphComponent.Graph;

    graph.GroupNodeDefaults.Style = new DeepZoomGroupNodeStyle(new ShapeNodeStyle {
        Brush = Brushes.White,
        Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x99, 0x6d, 0x4d)), 2.5),
        Shape = ShapeNodeShape.RoundRectangle
    });
    graph.GroupNodeDefaults.ShareStyleInstance = false;
    graph.GroupNodeDefaults.Size = new SizeD(50, 50);

    graph.NodeDefaults.Size = new SizeD(50, 50);
    graph.NodeDefaults.ShareStyleInstance = false;
    graph.NodeDefaults.Style = DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.RoundRectangle);
  }

  /// <summary>
  ///   Enables folding - changes the graphComponent's graph to a managed view
  ///   that provides the actual collapse/expand state.
  /// </summary>
  private static IFoldingView EnableFolding(GraphControl graphComponent) {
    // Creates the folding manager
    var foldingManager = new FoldingManager(graphComponent.Graph);
    // all group node are collapsed at startup
    var foldingView = foldingManager.CreateFoldingView(null, _ => false);
    graphComponent.Graph = foldingView.Graph;
    return foldingView;
  }

  [STAThread]
  private static void Main(string[] args) {
    Run<DeepZoomDemo>();
  }

  public DeepZoomDemo() {
    InitializeComponent();
  }

  public void LoadSampleGraph() {
    var graph = graphComponent.Graph;

    using var stream = LoadAssemblyResource("DeepZoomGraph.json") ??
        throw new InvalidOperationException("The sample graph could not be loaded.");
    var sampleData =
        JsonSerializer.Deserialize<DeepZoomData>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ??
        throw new InvalidOperationException("The sample graph could not be loaded.");

    var graphBuilder = new GraphBuilder(graph);

    // configure node creation
    var nodesSource = graphBuilder.CreateNodesSource(sampleData.Nodes, item => item.Id);
    nodesSource.ParentIdProvider = item => item.ParentId;
    nodesSource.NodeCreator.StyleProvider = item => new ShapeNodeStyle {
        Brush = CreateBrush(item.Fill),
        Pen = new Pen(CreateBrush(item.Stroke), 1.5),
        Shape = ShapeNodeShape.RoundRectangle
    };

    // configure group node creation
    var groupNodesSource = graphBuilder.CreateGroupNodesSource(sampleData.GroupNodes, item => item.Id);
    groupNodesSource.ParentIdProvider = item => item.ParentId;
    groupNodesSource.NodeCreator.StyleProvider = item => new DeepZoomGroupNodeStyle(new ShapeNodeStyle {
        Brush = CreateBrush(item.Fill),
        Pen = new Pen(CreateBrush(item.Stroke), 2.5),
        Shape = ShapeNodeShape.RoundRectangle
    });

    // configure edge creation
    var edgesSource = graphBuilder.CreateEdgesSource(sampleData.Edges, item => item.From, item => item.To);
    edgesSource.EdgeCreator.StyleProvider = item => {
      var brush = CreateBrush(item.Color);
      return new PolylineEdgeStyle {
          Pen = new Pen(brush, 1.5),
          TargetArrow = new Arrow(ArrowType.Triangle, ((SolidColorBrush) brush).Color)
      };
    };

    // actually create the graph
    graphBuilder.BuildGraph();
  }

  private static Brush CreateBrush(string color) => (Brush) new BrushConverter().ConvertFromString(color)!;
}
