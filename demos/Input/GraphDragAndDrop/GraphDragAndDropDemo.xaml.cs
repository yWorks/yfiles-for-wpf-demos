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
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.GraphDragAndDrop
{
  [Demo("graph-drag-and-drop", "input", "Graph Drag and Drop",
      Summary = "Shows drag and drop of graphs consisting of multiple items.",
      Keywords = ["palette", "graphs", "groups", "dnd"], Tags = ["interaction", "drag and drop"],
      RelatedDemos = ["drag-and-drop", "drag-from-component", "custom-drag-and-drop", "component-drag-and-drop"])]
  public sealed partial class GraphDragAndDropDemo
  {
    private readonly Settings settings = new();

    private GraphDropInputMode graphDropInputMode = null!;

    public GraphDragAndDropDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      // enable the option to expand and collapse group nodes
      var masterGraph = new DefaultGraph();
      var manager = new FoldingManager(masterGraph);
      GraphControl.Graph = manager.CreateFoldingView().Graph;

      // enable undo support
      masterGraph.SetUndoEngineEnabled(true);

      InitializeInputModes();

      InitializeStyles(Graph);

      InitializePalette();

      InitializeUI();
    }

    /// <summary>
    ///   Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    ///   and initializes the input mode for dropping graphs.
    /// </summary>
    private void InitializeInputModes() {
      // configure graph editor with snapping enabled
      var graphEditorInputMode = new GraphEditorInputMode {
          SnapContext = new GraphSnapContext {
              SnappableItems = SnappableItems.Node | SnappableItems.Edge,
              NodeDistance = 30,
              NodeToEdgeDistance = 20,
              SnapDistance = 10,
              GridSnapType = GridSnapTypes.All
          }
      };

      // Fix the top right location of a group node when toggling collapse/expand
      graphEditorInputMode.NavigationInputMode.AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.TopRight;

      // add the input mode to drop graphs
      graphDropInputMode = new GraphDropInputMode {
          SnappingEnabled = true,
          ShowPreview = true,
          AllowFolderNodeAsParent = false
      };
      graphEditorInputMode.Add(graphDropInputMode);

      GraphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    ///   Populates the palette with the graphs stored in the <c>Resources</c> folder.
    /// </summary>
    private void InitializePalette() {
      var graphDataList = LoadGraphDataList();

      // add for each graph a panel element to the palette element
      foreach (var graphData in graphDataList) {
        var graph = ToGraph(graphData);
        var paletteEntry = CreatePaletteEntry(graph);
        palettePanel.Children.Add(paletteEntry);
      }
    }

    /// <summary>
    ///   Creates a WPF element that visualizes the given graph in the drag and drop panel.
    /// </summary>
    private Border CreatePaletteEntry(IGraph graph) {
      // create a border that holds the graph preview
      var paletteEntry = new Border {
          Child = new Viewbox {
              Child = ToPreview(graph),
              Stretch = Stretch.None,
              HorizontalAlignment = HorizontalAlignment.Center
          },
          BorderBrush = Brushes.LightGray,
          BorderThickness = new Thickness(1),
          Background = Brushes.White,
          Margin = new Thickness(4),
          HorizontalAlignment = HorizontalAlignment.Stretch,
          Cursor = Cursors.Hand,
          Tag = graph
      };

      paletteEntry.PreviewMouseMove += (sender, args) => {
        if (args.LeftButton != MouseButtonState.Pressed || sender is not Border { Tag: IGraph draggedGraph } item) {
          return;
        }

        var data = new DataObject();
        data.SetData(typeof(IGraph), draggedGraph);
        DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
        args.Handled = true;
      };

      return paletteEntry;
    }

    private List<GraphData> LoadGraphDataList() {
      using var stream = LoadAssemblyResource("graphs.json");
      return JsonSerializer.Deserialize<List<GraphData>>(stream) ??
          throw new InvalidOperationException("The graph data could not be loaded.");
    }

    private sealed class NodeData
    {
      [JsonPropertyName("id")]
      public int Id { get; set; }

      [JsonPropertyName("layout")]
      public NodeLayoutData Layout { get; } = new();

      [JsonPropertyName("label")]
      public string Label { get; } = string.Empty;

      [JsonPropertyName("parent")]
      public int? Parent { get; set; }
    }

    private sealed class EdgeData
    {
      [JsonPropertyName("id")]
      public int Id { get; set; }

      [JsonPropertyName("source")]
      public int Source { get; set; }

      [JsonPropertyName("target")]
      public int Target { get; set; }

      [JsonPropertyName("label")]
      public EdgeLabelData Label { get; } = new();

      [JsonPropertyName("bends")]
      public List<BendData>? Bends { get; set; }
    }

    private sealed class GraphData
    {
      [JsonPropertyName("nodes")]
      public List<NodeData> Nodes { get; } = [];

      [JsonPropertyName("groups")]
      public List<NodeData> Groups { get; } = [];

      [JsonPropertyName("edges")]
      public List<EdgeData> Edges { get; } = [];
    }

    private sealed class NodeLayoutData
    {
      [JsonPropertyName("x")]
      public double X { get; set; }

      [JsonPropertyName("y")]
      public double Y { get; set; }

      [JsonPropertyName("width")]
      public double Width { get; set; }

      [JsonPropertyName("height")]
      public double Height { get; set; }
    }

    private sealed class EdgeLabelData
    {
      [JsonPropertyName("text")]
      public string Text { get; } = string.Empty;

      [JsonPropertyName("ratio")]
      public double Ratio { get; } = 0.5;
    }

    private sealed class BendData
    {
      [JsonPropertyName("x")]
      public double X { get; set; }

      [JsonPropertyName("y")]
      public double Y { get; set; }
    }

    /// <summary>
    ///   Builds a graph from the given graph data.
    /// </summary>
    private static IGraph ToGraph(GraphData graphData) {
      // enable the option to expand and collapse group nodes
      var masterGraph = new DefaultGraph();
      var manager = new FoldingManager(masterGraph);
      var graph = manager.CreateFoldingView().Graph;
      InitializeStyles(graph);

      var builder = new GraphBuilder(graph);
      var groupNodesSource = builder.CreateGroupNodesSource(graphData.Groups, item => item.Id);
      groupNodesSource.NodeCreator.LayoutProvider = item =>
          new RectD(item.Layout.X, item.Layout.Y, item.Layout.Width, item.Layout.Height);
      groupNodesSource.NodeCreator.CreateLabelBinding(item => item.Label);

      var nodesSource = builder.CreateNodesSource(graphData.Nodes, item => item.Id);
      nodesSource.ParentIdProvider = item => item.Parent.HasValue ? item.Parent.Value : null!;
      nodesSource.NodeCreator.LayoutProvider = item =>
          new RectD(item.Layout.X, item.Layout.Y, item.Layout.Width, item.Layout.Height);
      nodesSource.NodeCreator.CreateLabelBinding(item => item.Label);

      var edgesSource = builder.CreateEdgesSource(graphData.Edges, item => item.Source, item => item.Target,
          item => item.Id);
      edgesSource.EdgeCreator.BendsProvider = item => item.Bends?.Select(bend => new PointD(bend.X, bend.Y)) ?? [];
      var labelCreator = edgesSource.EdgeCreator.CreateLabelBinding(item => item.Label.Text);
      labelCreator.LayoutParameterProvider = item => CreateLabelParameter(item.Label.Ratio);

      return builder.BuildGraph();
    }

    /// <summary>
    ///   Creates an edge label parameter for placing a label along the path of an edge.
    /// </summary>
    /// <param name="ratio">
    ///   The ratio at which to place the label at the edge path. A ratio of 0.0 will place the label at
    ///   the source side of the edge path, a ratio of 1.0 at the target side. The default is 0.5.
    /// </param>
    private static ILabelModelParameter CreateLabelParameter(double ratio) =>
        new EdgePathLabelModel().CreateRatioParameter(ratio, EdgeSides.AboveEdge);

    /// <summary>
    ///   Creates an <see cref="UIElement"/> for the given graph.
    /// </summary>
    private static UIElement ToPreview(IGraph graph) {
      var previewControl = new GraphControl { Graph = graph };
      previewControl.FitGraphBounds();
      var bounds = previewControl.ContentBounds;
      if (bounds.Width <= 0 || bounds.Height <= 0) {
        bounds = new RectD(0, 0, 40, 40);
      }

      var exportBounds = bounds.GetEnlarged(new InsetsD(10, 10, 10, 10));
      const double maxItemWidth = 190;
      const double maxItemHeight = 120;
      var scale = Math.Min(1, Math.Min(maxItemWidth / exportBounds.Width, maxItemHeight / exportBounds.Height));
      var configurator = new ContextConfigurator(exportBounds) {
          Scale = scale,
          WorldBounds = exportBounds
      };
      return new UIElementExporter(configurator).Export(previewControl);
    }

    /// <summary>
    ///   Binds actions for the demo-specific items in the toolbar.
    /// </summary>
    private void InitializeUI() {
      previewCheckBox.DataContext = settings;
      snappingCheckBox.DataContext = settings;
      foldersAsParentsCheckBox.DataContext = settings;

      settings.PropertyChanged += (_, evt) => {
        switch (evt.PropertyName) {
          case nameof(Settings.ShowPreview):
            graphDropInputMode.ShowPreview = settings.ShowPreview;
            break;
          case nameof(Settings.SnappingEnabled):
            graphDropInputMode.SnappingEnabled = settings.SnappingEnabled;
            break;
          case nameof(Settings.AllowFolderNodeAsParent):
            graphDropInputMode.AllowFolderNodeAsParent = settings.AllowFolderNodeAsParent;
            break;
        }
      };
    }

    /// <summary>
    ///   Initializes the styles used for the graph elements of this demo.
    /// </summary>
    private static void InitializeStyles(IGraph graph) {
      DemoStyles.InitDemoStyles(graph, Themes.Palette48, foldingEnabled: true);
      var grayPen = new Pen(Brushes.Gray, 1.5);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          SmoothingLength = 8,
          Pen = grayPen,
          TargetArrow = new Arrow(ArrowType.Triangle, Brushes.Gray, grayPen, 0.5, 0.5)
      };

      graph.EdgeDefaults.Labels.Style = new LabelStyle {
          Shape = LabelShape.RoundRectangle,
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xcd, 0xce, 0xd2)),
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Center,
          Padding = new InsetsD(2, 4, 1, 4)
      };
    }

    [STAThread]
    private static void Main() => Run<GraphDragAndDropDemo>();
  }
}
