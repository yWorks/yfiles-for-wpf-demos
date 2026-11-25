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
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.DragFromComponent
{
  /// <summary>
  ///   Shows how to drag nodes from a graph control to other WPF elements.
  /// </summary>
  [Demo("drag-from-component", "input", "Drag From Component",
      Summary = "Shows how to drag graph items out of the graph component to another element.",
      Keywords = ["move", "dnd"], Tags = ["interaction", "drag and drop"],
      RelatedDemos = ["drag-and-drop", "graph-drag-and-drop", "custom-drag-and-drop", "component-drag-and-drop"])]
  public sealed partial class DragFromComponentDemo
  {
    public DragFromComponentDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      // initialize the input modes and the drag and drop targets
      ConfigureInputModes(GraphControl);

      // configure default styles and create a sample graph
      InitializeStyles(Graph);

      // build the graph from the given data set
      BuildGraph(Graph, LoadGraphData());

      // layout and center the graph
      Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      GraphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private static void BuildGraph(IGraph graph, GraphData graphData) {
      var graphBuilder = new GraphBuilder(graph);

      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodeList, item => item.Id);
      nodesSource.NodeCreator.StyleProvider = item => DemoStyles.CreateDemoNodeStyle(GetPalette(item.Label));
      var labelBinding = nodesSource.NodeCreator.CreateLabelBinding(item => item.Label);
      labelBinding.StyleProvider = item => new LabelStyle {
          BackgroundBrush = GetPalette(item.Label).NodeLabelFill,
          TextBrush = GetPalette(item.Label).Text,
          Shape = LabelShape.RoundRectangle,
          Padding = new InsetsD(2, 4, 1, 4)
      };

      graphBuilder.CreateEdgesSource(graphData.EdgeList, item => item.Source, item => item.Target, item => item.Id);

      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Configures node dragging and the graph editor's Shift-modified gestures.
    /// </summary>
    private void ConfigureInputModes(GraphControl graphControl) {
      var mode = new GraphEditorInputMode();

      // edge creation: start with drag + shift down
      // since drag without shift will start dragging a node from the component
      var shiftLeftDown = EventRecognizers.MouseDown.And(EventRecognizers.ShiftIsDown);
      mode.CreateEdgeInputMode.BeginRecognizer = shiftLeftDown;

      // move nodes: start with drag + shift down,
      // so dragging a selected node without shift will start dragging a node from the component
      mode.MoveSelectedItemsInputMode.BeginRecognizer = shiftLeftDown;
      mode.MoveUnselectedItemsInputMode.BeginRecognizer = shiftLeftDown;

      // disable directional constraint editing (the shift key is the default modifier, which
      // conflicts with the gestures above)
      mode.MoveSelectedItemsInputMode.DirectionalConstraintRecognizer = EventRecognizers.Never;
      mode.MoveUnselectedItemsInputMode.DirectionalConstraintRecognizer = EventRecognizers.Never;
      mode.CreateEdgeInputMode.DirectionalConstraintRecognizer = EventRecognizers.Never;

      // configure dragging from the component
      var dragMode = new NodeDragInputMode(graphControl, demoView);
      // lower priority than moveInputMode, so
      // dragging a selected node moves it
      // dragging an unselected node starts drag and drop
      dragMode.Priority = mode.MoveSelectedItemsInputMode.Priority + 1;
      mode.Add(dragMode);

      // configure the drop targets to handle the item drop
      // target #1: the list
      dragMode.AddDropTarget(dropList, node => AddDroppedItemToList(node, dropList));
      // target #2: the trashcan
      dragMode.AddDropTarget(dropTrashcan, node => graphControl.Graph.Remove(node));

      // install the input mode
      graphControl.InputMode = mode;
    }

    /// <summary>
    ///   Adds the given node to the list.
    /// </summary>
    private void AddDroppedItemToList(INode node, Panel list) {
      dropHere.Visibility = Visibility.Collapsed;

      // add a visual which represents the node
      var preview = NodeDragInputMode.CreateNodePreview(node);
      // Let the border receive drops instead of the decorative preview visual.
      preview.IsHitTestVisible = false;
      var item = new Border {
          HorizontalAlignment = HorizontalAlignment.Center,
          Margin = new Thickness(10),
          AllowDrop = true,
          Child = preview
      };
      list.Children.Add(item);
    }

    /// <summary>
    ///   Initializes the defaults for the styles in this demo.
    /// </summary>
    private static void InitializeStyles(IGraph graph) {
      DemoStyles.InitDemoStyles(graph, Themes.PaletteOrange, edgeTheme: Themes.PaletteOrange);

      graph.NodeDefaults.Size = new SizeD(70, 50);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Labels.ShareStyleInstance = false;
    }

    private static Palette GetPalette(string label) =>
        label.StartsWith("Red", StringComparison.Ordinal) ? Themes.Palette14 :
        label.StartsWith("Orange", StringComparison.Ordinal) ? Themes.Palette13 :
        label.StartsWith("Blue", StringComparison.Ordinal) ? Themes.Palette11 : Themes.PaletteOrange;

    private static GraphData LoadGraphData() {
      using var stream = GetResourceStream("Resources.graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(stream)!;
    }

    private static Stream GetResourceStream(string suffix) {
      var assembly = typeof(DragFromComponentDemo).Assembly;
      var resourceName = Array.Find(assembly.GetManifestResourceNames(),
          name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
      return resourceName is not null
          ? assembly.GetManifestResourceStream(resourceName)!
          : throw new InvalidOperationException($"Missing embedded resource ending in '{suffix}'.");
    }

    [STAThread]
    private static void Main() => Run<DragFromComponentDemo>();

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

      [JsonPropertyName("label")]
      public string Label { get; } = string.Empty;
    }

    private sealed class EdgeData
    {
      [JsonPropertyName("id")]
      public int Id { get; init; }

      [JsonPropertyName("source")]
      public int Source { get; init; }

      [JsonPropertyName("target")]
      public int Target { get; init; }
    }
  }
}
