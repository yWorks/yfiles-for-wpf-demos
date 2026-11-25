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
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Organic;

namespace yWorks.Demo.StyleDecorators
{
  /// <summary>
  ///   Shows how to create styles for nodes, edges, and labels that wrap existing styles and add visual decorations.
  /// </summary>
  [Demo("style-decorators", "style", "Style Decorators",
      Summary =
          "Shows how to create styles for nodes, edges, and labels that wrap existing styles and add visual decorators.",
      Tags = ["style", "decorators"], Keywords = ["ports"])]
  public sealed partial class StyleDecoratorsDemo
  {
    private static readonly INodeStyle baseStyle = new ShapeNodeStyle {
        Brush = new SolidColorBrush(Color.FromRgb(0x46, 0xA8, 0xD5)),
        Pen = null,
        Shape = ShapeNodeShape.Rectangle
    };

    public StyleDecoratorsDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      GraphControl.InputMode = CreateInputMode();
      ConfigureGraph(Graph);
      LoadGraph(GraphControl);
    }

    private void LoadGraph(GraphControl graphControl) {
      // build the graph from the given data set
      BuildGraph(graphControl.Graph, LoadGraphData());

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new OrganicLayout { DefaultMinimumNodeDistance = 100 });
      graphControl.FitGraphBounds();

      // add some bends
      foreach (var edge in graphControl.Graph.Edges) {
        var sourcePort = edge.SourcePort;
        var targetPort = edge.TargetPort;
        graphControl.Graph.AddBend(edge, PointD.MidPoint(sourcePort.GetLocation(), targetPort.GetLocation()));
      }

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph, GraphData graphData) {
      var graphBuilder = new GraphBuilder(graph);

      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodeList, item => item.Id);
      nodesSource.NodeCreator.StyleProvider = item => item.Tag is { } tag
          ? new NodeStyleDecorator(baseStyle, LoadImage(tag))
          : graph.NodeDefaults.GetStyleInstance();
      nodesSource.NodeCreator.CreateLabelBinding(item => item.Label!);

      var edgesSource = graphBuilder.CreateEdgesSource(graphData.EdgeList, item => item.Source, item => item.Target,
          item => item.Id);
      edgesSource.EdgeCreator.TagProvider = item => item.Tag!;

      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Creates an input mode that supports interactive editing like e.g., creating new nodes and edges or
    ///   editing labels.
    /// </summary>
    private static GraphEditorInputMode CreateInputMode() {
      var geim = new GraphEditorInputMode { AllowEditLabel = true };

      // set a random traffic value to edges created interactively
      geim.CreateEdgeInputMode.EdgeCreationStarted += (_, evt) => {
        evt.Item.Tag = Random.Shared.Next(4) switch {
            0 => "TRAFFIC_VERY_HIGH",
            1 => "TRAFFIC_HIGH",
            2 => "TRAFFIC_NORMAL",
            _ => "TRAFFIC_LOW"
        };
      };

      return geim;
    }

    /// <summary>
    ///   Configures default styles for nodes and edges.
    /// </summary>
    private void ConfigureGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);

      graph.NodeDefaults.Style = new NodeStyleDecorator(baseStyle, LoadImage("workstation"));
      graph.NodeDefaults.Size = new SizeD(80, 40);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.EdgeDefaults.Style = new EdgeStyleDecorator(new ShapePortStyle {
          Brush = Brushes.LightGray,
          Pen = null,
          Shape = ShapeNodeShape.Ellipse,
          RenderSize = new SizeD(5, 5)
      });
      graph.NodeDefaults.Labels.Style = new LabelStyleDecorator(new LabelStyle {
          TextBrush = new SolidColorBrush(Color.FromRgb(0x22, 0x45, 0x56)),
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xB4, 0xDB, 0xED))
      });
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;

      graph.EdgeDefaults.Labels.Style = new LabelStyleDecorator(new LabelStyle());
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0);
      graph.EdgeDefaults.ShareStyleInstance = false;
    }

    private ImageSource LoadImage(string name) {
      var resourceName = name.ToLowerInvariant() switch {
          "printer" => "PrinterIcon",
          "scanner" => "ScannerIcon",
          "switch" => "SwitchIcon",
          "workstation" => "WorkstationIcon",
          _ => throw new InvalidOperationException($"Unknown device icon '{name}'.")
      };
      return (ImageSource) FindResource(resourceName);
    }

    private void OnReloadClick(object sender, RoutedEventArgs args) {
      GraphControl.Graph.Clear();
      LoadGraph(GraphControl);
    }

    private GraphData LoadGraphData() {
      using var stream = GetResourceStream("Resources.graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(stream) ??
          throw new InvalidOperationException("The sample graph data could not be loaded.");
    }

    private static Stream GetResourceStream(string suffix) {
      var assembly = typeof(StyleDecoratorsDemo).Assembly;
      var resourceName = Array.Find(assembly.GetManifestResourceNames(),
          name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
      return resourceName is not null
          ? assembly.GetManifestResourceStream(resourceName)!
          : throw new InvalidOperationException($"Missing embedded resource ending in '{suffix}'.");
    }

    private sealed class GraphData
    {
      [JsonPropertyName("nodeList")]
      public NodeData[] NodeList { get; init; } = [];

      [JsonPropertyName("edgeList")]
      public EdgeData[] EdgeList { get; init; } = [];
    }

    private sealed class NodeData
    {
      [JsonPropertyName("id")]
      public int Id { get; init; }

      [JsonPropertyName("tag")]
      public string? Tag { get; init; }

      [JsonPropertyName("label")]
      public string? Label { get; init; }
    }

    private sealed class EdgeData
    {
      [JsonPropertyName("id")]
      public int Id { get; init; }

      [JsonPropertyName("source")]
      public int Source { get; init; }

      [JsonPropertyName("target")]
      public int Target { get; init; }

      [JsonPropertyName("tag")]
      public string? Tag { get; init; }
    }

    [STAThread]
    public static void Main() {
      Run<StyleDecoratorsDemo>();
    }
  }
}
