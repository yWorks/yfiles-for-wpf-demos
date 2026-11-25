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
using yWorks.Graph.Styles;

namespace yWorks.Demo.ComponentDragAndDrop
{
  [Demo("component-drag-and-drop", "input", "Component Drag and Drop",
      Summary = "A demo that shows how to clear space for a dropped component in an existing layout.",
      Keywords = [
          "layoutfeatures", "drop input mode", "create", "space", "clear area layout", "fill area layout",
          "drag and drop", "palette", "dnd", "interactive"
      ], Tags = ["layout", "component"],
      RelatedDemos = ["drag-and-drop", "graph-drag-and-drop", "custom-drag-and-drop", "component-drag-and-drop"])]
  public sealed partial class ComponentDragAndDropDemo
  {
    private readonly Settings settings = new();

    private GraphDropInputMode componentDropInputMode = null!;

    /// <summary>
    ///   Performs layout and animation during the drag and drop operation.
    /// </summary>
    private ClearAreaLayoutHelper? layoutHelper;

    /// <summary>
    ///   Determines whether components are kept during drag.
    /// </summary>
    private bool KeepComponents => settings.KeepComponents;

    /// <summary>
    ///   Counts the components and is used as component id.
    /// </summary>
    private int componentCount;

    private bool deleting;

    public ComponentDragAndDropDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      InitializeGraph();
      InitializeInputModes();
      InitializePalette();
      LoadSampleGraph();
      InitializeUI();
    }

    /// <summary>
    ///   Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    ///   and initializes the input mode for dropping components.
    /// </summary>
    private void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode();
      // add newly created nodes to their only component
      graphEditorInputMode.NodeCreated += (_, args) => args.Item.Tag = componentCount++;
      // change color of new edges to the color of the component if source and target belong to the same component
      graphEditorInputMode.CreateEdgeInputMode.EdgeCreated += (_, args) => {
        var edge = args.Item;
        if (edge.SourcePort.Owner is INode sourceNode && edge.TargetPort.Owner is INode targetNode &&
            sourceNode.Tag is int sourceComponent && targetNode.Tag is int targetComponent &&
            sourceComponent == targetComponent && sourceNode.Style is ShapeNodeStyle nodeStyle) {
          Graph.SetStyle(edge, new PolylineEdgeStyle { Pen = new Pen(nodeStyle.Brush, 5) });
        }
      };

      graphEditorInputMode.ItemsDuplicated += (_, args) => UpdateDuplicatedComponent(args.Items);
      graphEditorInputMode.ItemsPasted += (_, args) => UpdateDuplicatedComponent(args.Items);

      // configure the move input mode to drag components
      // both moving of selected and unselected nodes can be supported,
      // so both moveSelectedItemsInputMode and moveUnselectedItemsInputMode have to be configured
      ConfigureMoveInputMode(graphEditorInputMode.MoveSelectedItemsInputMode);
      ConfigureMoveInputMode(graphEditorInputMode.MoveUnselectedItemsInputMode);

      componentDropInputMode = new GraphDropInputMode();
      componentDropInputMode.DragEntered += (_, _) => {
        if (componentDropInputMode.DropData is IGraph droppedGraph) {
          layoutHelper = new ClearAreaLayoutHelper(GraphControl, droppedGraph.Nodes, KeepComponents);
          layoutHelper.InitializeLayout();
        }
      };
      componentDropInputMode.DragOver += (_, _) => {
        if (layoutHelper is not null) {
          var location = componentDropInputMode.PointerPosition;
          layoutHelper.Location = new PointD(location.X, location.Y);
          layoutHelper.RunLayout();
        }
      };
      componentDropInputMode.ItemCreated += (_, args) => {
        if (layoutHelper is null) {
          return;
        }

        layoutHelper.Location = componentDropInputMode.DropLocation;
        layoutHelper.Component = [.. args.Item.Nodes];
        // specify the dropped nodes as a single component
        foreach (var node in layoutHelper.Component) {
          node.Tag = componentCount;
        }

        componentCount++;
        layoutHelper.FinishLayout();
      };
      componentDropInputMode.DragLeft += (_, _) => layoutHelper?.CancelLayout();
      graphEditorInputMode.Add(componentDropInputMode);

      graphEditorInputMode.DeletingSelection += (_, _) => deleting = true;
      graphEditorInputMode.DeletedSelection += (_, _) => deleting = false;

      // select the whole component when selecting a graph element
      GraphControl.Selection.ItemAdded += (_, args) => {
        if (!deleting) {
          OnSelectionChanged(args.Item, true);
        }
      };
      GraphControl.Selection.ItemRemoved += (_, args) => {
        if (!deleting) {
          OnSelectionChanged(args.Item, false);
        }
      };

      GraphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    ///   Updates the selected whenever an item is being selected or deselected.
    /// </summary>
    private void OnSelectionChanged(IModelItem item, bool selected) {
      var changedNode = item switch {
          INode node => node,
          IEdge edge when edge.GetSourceNode().Tag is int sourceComponent &&
              edge.GetTargetNode().Tag is int targetComponent &&
              sourceComponent == targetComponent => edge.GetSourceNode(),
          _ => null
      };

      if (changedNode is null || changedNode.Tag is not int componentId) {
        return;
      }

      var component = Graph.Nodes.Where(node => Equals(node.Tag, componentId)).ToList();
      foreach (var node in component) {
        SetSelected(node, selected);

        foreach (var edge in Graph.EdgesAt(node)) {
          if (edge.SourcePort.Owner is INode sourceNode && edge.TargetPort.Owner is INode targetNode &&
              component.Contains(sourceNode) && component.Contains(targetNode)) {
            foreach (var bend in edge.Bends) {
              SetSelected(bend, selected);
            }

            SetSelected(edge, selected);
          }
        }
      }
    }

    /// <summary>
    ///   Adds the listeners for drag operations on the given input mode.
    ///   This demo supports moving of both selected and unselected nodes.
    /// </summary>
    private void ConfigureMoveInputMode(MoveInputMode moveInputMode) {
      moveInputMode.DragStarted += (_, _) => OnDragStarted(moveInputMode);
      moveInputMode.Dragged += (_, _) => OnDragged();
      moveInputMode.DragFinished += (_, _) => DragFinished(moveInputMode);
      moveInputMode.DragCanceled += (_, _) => DragCanceled();
    }

    /// <summary>
    ///   Selected or deselects the given item.
    /// </summary>
    private void SetSelected(IModelItem item, bool selected) {
      if (selected) {
        GraphControl.Selection.Add(item);
      } else {
        GraphControl.Selection.Remove(item);
      }
    }

    /// <summary>
    ///   Initializes the layout when a node has been moved.
    /// </summary>
    private void OnDragStarted(MoveInputMode moveInputMode) {
      var component = moveInputMode.AffectedItems.OfType<INode>().ToList();
      layoutHelper = new ClearAreaLayoutHelper(GraphControl, component, KeepComponents);
      layoutHelper.InitializeLayout();
    }

    /// <summary>
    ///   Updates the layout when a node has been moved.
    /// </summary>
    private void OnDragged() {
      var location = GraphControl.LastPointerEvent.Location;
      layoutHelper?.Location = new PointD(location.X, location.Y);
      layoutHelper?.RunLayout();
    }

    /// <summary>
    ///   Finalizes the component after a node movement.
    /// </summary>
    private void DragFinished(MoveInputMode moveInputMode) {
      var location = GraphControl.LastPointerEvent.Location;
      layoutHelper?.Location = new PointD(location.X, location.Y);
      layoutHelper?.Component = [.. moveInputMode.AffectedItems.OfType<INode>()];
      layoutHelper?.FinishLayout();
    }

    /// <summary>
    ///   Cancels the layout when the drag operation has been canceled.
    /// </summary>
    private void DragCanceled() => layoutHelper?.CancelLayout();

    /// <summary>
    ///   Updates the graph after a component was added using clipboard operations
    /// </summary>
    private void UpdateDuplicatedComponent(IEnumerable<IModelItem> items) {
      var nodes = items.OfType<INode>().ToList();
      if (nodes.Count == 0) {
        return;
      }

      foreach (var node in nodes) {
        node.Tag = componentCount;
      }

      UpdateGraph();
      componentCount++;
    }

    private void UpdateGraph() {
      var component = Graph.Nodes.Where(node => Equals(node.Tag, componentCount)).ToList();
      layoutHelper = new ClearAreaLayoutHelper(GraphControl, component, KeepComponents);
      layoutHelper.InitializeLayout();
      layoutHelper.RunLayout();

      // update the selection of the new component
      GraphControl.Selection.Clear();
      foreach (var node in Graph.Nodes) {
        SetSelected(node, Equals(node.Tag, componentCount));
      }
    }

    /// <summary>
    ///   Initializes the default styles.
    /// </summary>
    private void InitializeGraph() {
      Graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = new SolidColorBrush(Color.FromRgb(0xc1, 0xc1, 0xc1)),
          Pen = null
      };

      Graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0xc1, 0xc1, 0xc1)), 5)
      };
    }

    /// <summary>
    ///   Populates the palette with the graphs stored in the resources folder.
    /// </summary>
    private void InitializePalette() {
      using var stream = LoadAssemblyResource("PaletteComponents.json");
      var components = JsonSerializer.Deserialize<List<PaletteComponentData>>(stream) ??
          throw new InvalidOperationException("The palette components could not be loaded.");

      foreach (var component in components) {
        var componentGraph = GetComponentGraph(component);
        palettePanel.Children.Add(CreatePaletteEntry(componentGraph));
      }
    }

    /// <summary>
    ///   Creates a WPF element that visualizes the given graph in the drag and drop panel.
    /// </summary>
    private Border CreatePaletteEntry(IGraph graph) {
      var entry = new Border {
          Child = new Viewbox {
              Child = CreateComponentVisual(graph),
              Stretch = Stretch.None,
              HorizontalAlignment = HorizontalAlignment.Center
          },
          BorderBrush = Brushes.LightGray,
          BorderThickness = new Thickness(1),
          Background = Brushes.White,
          Margin = new Thickness(10, 10, 10, 10),
          Height = 150,
          HorizontalAlignment = HorizontalAlignment.Stretch,
          Cursor = Cursors.Hand,
          Tag = graph
      };

      entry.PreviewMouseMove += (sender, args) => {
        if (args.LeftButton != MouseButtonState.Pressed || sender is not Border { Tag: IGraph draggedGraph } item) {
          return;
        }

        GraphDropInputMode.StartDrag(item, draggedGraph);
        args.Handled = true;
      };

      return entry;
    }

    /// <summary>
    ///   Builds a graph from the given component.
    /// </summary>
    private IGraph GetComponentGraph(PaletteComponentData component) {
      var componentGraph = new DefaultGraph();
      var brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(component.Color)!);
      componentGraph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Pen = new Pen(brush, 1),
          Brush = brush
      };
      componentGraph.EdgeDefaults.Style = new PolylineEdgeStyle { Pen = new Pen(brush, 5) };

      var defaultNodeSize = componentGraph.NodeDefaults.Size;
      var builder = new GraphBuilder(componentGraph);
      var nodesSource = builder.CreateNodesSource(component.NodeData, item => item.Id);
      nodesSource.NodeCreator.LayoutProvider = item =>
          new RectD(item.X, item.Y, defaultNodeSize.Width, defaultNodeSize.Height);
      builder.CreateEdgesSource(component.EdgeData, item => item.Source, item => item.Target, item => item.Id);
      builder.BuildGraph();

      return componentGraph;
    }

    /// <summary>
    ///   Creates a <see cref="UIElement"/> for the given graph.
    /// </summary>
    private static UIElement CreateComponentVisual(IGraph graph) {
      var exportControl = new GraphControl { Graph = graph };
      exportControl.UpdateContentBounds(new InsetsD(5));

      var visualExport = new UIElementExporter(exportControl.ContentBounds);
      var visual = visualExport.Export(exportControl);

      return visual;
    }

    /// <summary>
    ///   Loads the initial graph.
    /// </summary>
    private void LoadSampleGraph() {
      using var stream = LoadAssemblyResource("SampleGraph.json");
      var graphData = JsonSerializer.Deserialize<GraphData>(stream) ??
          throw new InvalidOperationException("The sample graph could not be loaded.");

      var defaultNodeSize = Graph.NodeDefaults.Size;
      var builder = new GraphBuilder(Graph);
      var nodesSource = builder.CreateNodesSource(graphData.NodeData, item => item.Id);
      nodesSource.NodeCreator.LayoutProvider = item =>
          new RectD(item.X, item.Y, defaultNodeSize.Width, defaultNodeSize.Height);
      builder.CreateEdgesSource(graphData.EdgeData, item => item.Source, item => item.Target, item => item.Id);
      builder.BuildGraph();

      GraphControl.FitGraphBounds();

      foreach (var node in Graph.Nodes) {
        node.Tag = componentCount;
      }

      componentCount++;
    }

    /// <summary>
    ///   Registers actions for the items in the toolbar.
    /// </summary>
    private void InitializeUI() {
      keepComponentsCheckBox.DataContext = settings;
    }

    private sealed class NodeData
    {
      [JsonPropertyName("id")]
      public int Id { get; set; }

      [JsonPropertyName("x")]
      public double X { get; set; }

      [JsonPropertyName("y")]
      public double Y { get; set; }
    }

    private sealed class EdgeData
    {
      [JsonPropertyName("id")]
      public int Id { get; set; }

      [JsonPropertyName("source")]
      public int Source { get; set; }

      [JsonPropertyName("target")]
      public int Target { get; set; }
    }

    private sealed class GraphData
    {
      [JsonPropertyName("nodeData")]
      public List<NodeData> NodeData { get; set; } = [];

      [JsonPropertyName("edgeData")]
      public List<EdgeData> EdgeData { get; set; } = [];
    }

    private sealed class PaletteComponentData
    {
      [JsonPropertyName("nodeData")]
      public List<NodeData> NodeData { get; } = [];

      [JsonPropertyName("edgeData")]
      public List<EdgeData> EdgeData { get; } = [];

      [JsonPropertyName("color")]
      public string Color { get; } = string.Empty;
    }

    [STAThread]
    private static void Main() => Run<ComponentDragAndDropDemo>();
  }
}
