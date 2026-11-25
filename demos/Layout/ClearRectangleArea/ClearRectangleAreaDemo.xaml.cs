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
using System.Windows.Controls;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout.Partial;
using yWorks.Utils;

namespace yWorks.Demo.ClearRectangleArea
{
  /// <summary>
  ///   Demonstrates how to interactively move graph elements around a rectangle in a given graph layout
  ///   so that the modifications in the graph are minimal.
  /// </summary>
  [Demo("clear-rectangle-area", "layout", "Clear Rectangle Area",
      Summary = "Shows how to automatically keep a user-defined rectangular area clear of graph elements.",
      Tags = ["layout", "interactive layout"], Keywords = [
          "layoutfeatures", "create", "space", "clear area layout", "fill area layout", "adjustment", "interactive"
      ], RelatedDemos = [])]
  public sealed partial class ClearRectangleAreaDemo
  {
    /// <summary>
    ///   The rectangular area used for clearing.
    /// </summary>
    private readonly MutableRectangle clearRect = new(0, 0, 100, 100);

    /// <summary>
    ///   Options to control the layout behavior.
    /// </summary>
    private readonly LayoutOptions options = new(ClearAreaStrategy.PreserveShapes, ComponentAssignmentStrategy.Single,
        false);

    /// <summary>
    ///   The group node we are currently inside.
    /// </summary>
    private INode? groupNode;

    /// <summary>
    ///   Performs layout and animation while dragging the rectangle.
    /// </summary>
    private ClearAreaLayoutHelper? layoutHelper;

    /// <summary>
    ///   An IHitTester to determine the group node we are currently hovering.
    /// </summary>
    private IHitTester? nodeHitTester;

    private bool shiftWasPressed;

    private Dictionary<string, GraphSample> sampleGraphs = new();

    public ClearRectangleAreaDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      InitializeUI();
      InitializeInputModes();
      InitializeGraph();
    }

    /// <summary>
    ///   Syncs combo box selected index with initially loaded sample and options set.
    ///   Also binds actions to toolbars.
    /// </summary>
    private void InitializeUI() {
      AddOption(samplesComboBox, "Hierarchical", "hierarchical");
      AddOption(samplesComboBox, "Grouping", "grouping");
      AddOption(samplesComboBox, "Organic", "organic");
      AddOption(samplesComboBox, "Circular", "circular");
      AddOption(samplesComboBox, "Tree", "tree");
      AddOption(samplesComboBox, "Radial Tree", "radialTree");
      AddOption(samplesComboBox, "Series-Parallel", "seriesParallel");
      AddOption(samplesComboBox, "Components", "components");
      samplesComboBox.SelectedValuePath = nameof(Tag);
      samplesComboBox.SelectedValue = "hierarchical";
      samplesComboBox.SelectionChanged += (_, _) => {
        if (samplesComboBox.SelectedValue is string sampleName) {
          LoadGraph(sampleName);
        }
      };

      AddOption(clearingStrategyComboBox, "Local", ClearAreaStrategy.Local);
      AddOption(clearingStrategyComboBox, "Local Uniform", ClearAreaStrategy.LocalUniform);
      AddOption(clearingStrategyComboBox, "Preserve Shapes", ClearAreaStrategy.PreserveShapes);
      AddOption(clearingStrategyComboBox, "Preserve Shapes Uniform", ClearAreaStrategy.PreserveShapesUniform);
      AddOption(clearingStrategyComboBox, "Global", ClearAreaStrategy.Global);
      clearingStrategyComboBox.SelectedValuePath = nameof(Tag);
      clearingStrategyComboBox.SelectedValue = options.ClearAreaStrategy;
      clearingStrategyComboBox.SelectionChanged += (_, _) => {
        if (clearingStrategyComboBox.SelectedValue is ClearAreaStrategy strategy) {
          options.ClearAreaStrategy = strategy;
        }
      };

      AddOption(componentAssignmentStrategyComboBox, "Single", ComponentAssignmentStrategy.Single);
      AddOption(componentAssignmentStrategyComboBox, "Connected", ComponentAssignmentStrategy.Connected);
      AddOption(componentAssignmentStrategyComboBox, "Clustering", ComponentAssignmentStrategy.Clustering);
      componentAssignmentStrategyComboBox.SelectedValuePath = nameof(Tag);
      componentAssignmentStrategyComboBox.SelectedValue = options.ComponentAssignmentStrategy;
      componentAssignmentStrategyComboBox.SelectionChanged += (_, _) => {
        if (componentAssignmentStrategyComboBox.SelectedValue is ComponentAssignmentStrategy strategy) {
          options.ComponentAssignmentStrategy = strategy;
        }
      };
    }

    private static void AddOption<TValue>(ComboBox comboBox, string label, TValue value) {
      comboBox.Items.Add(new ComboBoxItem {
          Content = label,
          Tag = value
      });
    }

    /// <summary>
    ///   Enables undo/redo support and initializes the default styles.
    /// </summary>
    private void InitializeGraph() {
      graphControl.Graph.SetUndoEngineEnabled(true);
      DemoStyles.InitDemoStyles(graphControl.Graph);
      LoadSampleGraphs();
      LoadGraph("hierarchical");
      graphControl.Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Registers the GraphEditorInputMode as the GraphControl input mode and initializes the
    ///   rectangular area so that it is drawn and can be moved and resized.
    /// </summary>
    private void InitializeInputModes() {
      // Create a GraphEditorInputMode instance.
      var editMode = new GraphEditorInputMode();
      // Install the edit mode into the canvas.
      graphControl.InputMode = editMode;

      // Visualize the clear rectangle.
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.HighlightGroup, clearRect,
          new ClearRectangleRenderer());

      AddClearRectInputModes(editMode);
    }

    /// <summary>
    ///   Adds the input modes that handle the resizing and movement of the rectangular area.
    /// </summary>
    /// <param name="inputMode">The input mode.</param>
    private void AddClearRectInputModes(GraphEditorInputMode inputMode) {
      // Create handles for interactively resizing the rectangle.
      var rectangleHandles = new RectangleReshapeHandleProvider(clearRect) {
          HandlePositions = HandlePositions.Corners,
          MinimumSize = new SizeD(10, 10)
      };

      // Create a mode that deals with the handles.
      var handleInputMode = new HandleInputMode { Priority = 1 };
      inputMode.Add(handleInputMode);

      // Add the handles.
      var inputModeContext = new InputModeContext(inputMode.CreateInputModeContext(), handleInputMode);
      handleInputMode.Handles.Add(rectangleHandles.GetHandle(inputModeContext, HandlePositions.TopRight));
      handleInputMode.Handles.Add(rectangleHandles.GetHandle(inputModeContext, HandlePositions.TopLeft));
      handleInputMode.Handles.Add(rectangleHandles.GetHandle(inputModeContext, HandlePositions.BottomRight));
      handleInputMode.Handles.Add(rectangleHandles.GetHandle(inputModeContext, HandlePositions.BottomLeft));

      // Create a mode that allows for dragging the rectangle at the sides.
      var moveInputMode = new MoveInputMode {
          PositionHandler = new RectanglePositionHandler(clearRect),
          HitTestable = HitTestables.Create((_, location) => clearRect.ToRectD().Contains(location)),
          Priority = 41
      };

      // Handle dragging the rectangle.
      moveInputMode.DragStarting += OnDragStarting;
      moveInputMode.Dragged += OnDragged;
      moveInputMode.DragCanceled += OnDragCanceled;
      moveInputMode.DragFinished += OnDragFinished;

      // Handle resizing the rectangle.
      handleInputMode.DragStarting += OnDragStarting;
      handleInputMode.Dragged += OnDragged;
      handleInputMode.DragCanceled += OnDragCanceled;
      handleInputMode.DragFinished += OnDragFinished;

      // Add the mode to the edit mode.
      inputMode.Add(moveInputMode);
    }

    /// <summary>
    ///   The rectangular area is about to be moved or resized.
    /// </summary>
    private void OnDragStarting(object? sender, InputModeEventArgs e) {
      nodeHitTester = e.Context.Lookup<IHitTester>();
      layoutHelper = new ClearAreaLayoutHelper(graphControl, clearRect, options);
      layoutHelper.InitializeLayout();
      shiftWasPressed = IsShiftPressed();
    }

    /// <summary>
    ///   The rectangular area is currently being moved or resized. For each drag a new layout is
    ///   calculated and applied if the previous one is completed.
    /// </summary>
    private async void OnDragged(object? sender, InputModeEventArgs e) {
      if (IsShiftPressed()) {
        // We do not change the layout now, instead we check if we are hovering a group node.
        // If so, we use that group node inside which the cleared area should be located.
        // In addition, the group node is highlighted to better recognize it.
        if (nodeHitTester != null) {
          var hitGroupNode = GetHitGroupNode(e.Context);
          if (hitGroupNode != groupNode) {
            if (groupNode != null) {
              graphControl.Highlights.Remove(groupNode);
            }
            if (hitGroupNode != null) {
              graphControl.Highlights.Add(hitGroupNode);
            }
            groupNode = hitGroupNode;
          }
        }
      } else {
        if (shiftWasPressed && groupNode != null) {
          // Now we remove the highlight of the group.
          graphControl.Highlights.Remove(groupNode);
        }

        // Invoke the layout calculation and animation.
        layoutHelper!.GroupNode = groupNode;
        await layoutHelper.RunLayoutAsync();
      }
      shiftWasPressed = IsShiftPressed();
    }

    /// <summary>
    ///   Moving or resizing the rectangular area has been canceled. The state before the gesture
    ///   must be restored.
    /// </summary>
    private async void OnDragCanceled(object? sender, InputModeEventArgs e) {
      await layoutHelper!.CancelLayoutAsync();
      ClearGroupHighlight();
      groupNode = null;
    }

    /// <summary>
    ///   Moving or resizing the rectangular area has been finished. We execute the layout to the
    ///   final state.
    /// </summary>
    private async void OnDragFinished(object? sender, InputModeEventArgs e) {
      await layoutHelper!.StopLayoutAsync();
      ClearGroupHighlight();
      groupNode = null;
    }

    private void ClearGroupHighlight() {
      if (groupNode != null) {
        graphControl.Highlights.Remove(groupNode);
      }
    }

    /// <summary>
    ///   Determines the group node over which the mouse is currently hovering. If there is no group
    ///   node, null is returned.
    /// </summary>
    private INode? GetHitGroupNode(IInputModeContext context) {
      if (nodeHitTester == null) {
        return null;
      }
      var hits = nodeHitTester.EnumerateHits(context, context.CanvasControl!.LastPointerEvent.Location,
          GraphItemTypes.Node);
      return hits.OfType<INode>().FirstOrDefault(graphControl.Graph.IsGroupNode);
    }

    /// <summary>
    ///   Determines whether ModifierKeys.Shift is currently pressed.
    /// </summary>
    private static bool IsShiftPressed() => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

    /// <summary>
    ///   Loads the sample graph associated with the given name.
    /// </summary>
    private void LoadGraph(string sampleName) {
      var data = sampleGraphs[sampleName];
      var graph = graphControl.Graph;
      graph.Clear();

      var defaultNodeSize = graph.NodeDefaults.Size;
      var builder = new GraphBuilder(graph);
      var nodesSource = builder.CreateNodesSource(data.Nodes, node => node.Id);
      nodesSource.ParentIdProvider = node => node.ParentId;
      nodesSource.NodeCreator.LayoutProvider = node =>
          new RectD(node.X, node.Y, defaultNodeSize.Width, defaultNodeSize.Height);

      var groupStyle = DemoStyles.CreateDemoGroupStyle();
      // Set HitTransparentContentArea to false so group nodes are properly hit in GetHitGroupNode.
      groupStyle.HitTransparentContentArea = false;
      graph.GroupNodeDefaults.Style = groupStyle;
      if (data.Groups is { } groups) {
        var groupNodesSource = builder.CreateGroupNodesSource(groups, group => group.Id);
        groupNodesSource.ParentIdProvider = group => group.ParentId;
        groupNodesSource.NodeCreator.LayoutProvider = group => new RectD(group.X, group.Y, group.Width, group.Height);
      }

      builder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target, edge => edge.Id);
      builder.BuildGraph();

      foreach (var edgeData in data.Edges) {
        var edge = builder.GetEdgeForItem(edgeData)!;
        if (edgeData.SourcePort is { } sourcePort) {
          graph.SetPortLocation(edge.SourcePort, new PointD(sourcePort.X, sourcePort.Y));
        }
        if (edgeData.TargetPort is { } targetPort) {
          graph.SetPortLocation(edge.TargetPort, new PointD(targetPort.X, targetPort.Y));
        }
        foreach (var bend in edgeData.Bends) {
          graph.AddBend(edge, new PointD(bend.X, bend.Y));
        }
      }

      graphControl.FitGraphBounds();
      graph.GetUndoEngine()?.Clear();

      // Move the clear rectangle to the default initial position.
      clearRect.X = 0;
      clearRect.Y = 0;
    }

    /// <summary>
    ///   Loads the sample graph data embedded in the project.
    /// </summary>
    private void LoadSampleGraphs() {
      using var stream = LoadAssemblyResource("SampleData.json");
      sampleGraphs =
          JsonSerializer.Deserialize<Dictionary<string, GraphSample>>(stream,
              new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
          throw new InvalidOperationException("Could not load the embedded sample graph data.");
    }

    [STAThread]
    public static void Main(string[] args) => Run<ClearRectangleAreaDemo>();

    private sealed class GraphSample
    {
      public List<NodeData> Nodes { get; } = [];

      public List<GroupNodeData>? Groups { get; init; }

      public List<EdgeData> Edges { get; } = [];
    }

    private class NodeData
    {
      public int Id { get; init; }

      public double X { get; init; }

      public double Y { get; init; }

      public int? ParentId { get; init; }
    }

    private sealed class GroupNodeData : NodeData
    {
      public double Width { get; init; }

      public double Height { get; init; }
    }

    private sealed class EdgeData
    {
      public int Id { get; init; }

      public int Source { get; init; }

      public int Target { get; init; }

      public PointData? SourcePort { get; init; }

      public PointData? TargetPort { get; init; }

      public List<PointData> Bends { get; } = [];
    }

    private sealed class PointData
    {
      public double X { get; init; }

      public double Y { get; init; }
    }
  }
}
