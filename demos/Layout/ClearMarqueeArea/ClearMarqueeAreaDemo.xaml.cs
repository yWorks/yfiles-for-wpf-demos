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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout.Partial;
using yWorks.Utils;

namespace yWorks.Demo.ClearMarqueeArea
{
  /// <summary>
  ///   Demonstrates how to interactively make space for a marquee rectangle in a graph layout.
  /// </summary>
  [Demo("clear-marquee-area", "layout", "Clear Marquee Area",
      Summary = "Shows how to automatically keep a marquee area clear of graph elements.",
      Tags = ["layout", "interactive layout"], Keywords = [
          "layoutfeatures", "create", "space", "clear area layout", "fill area layout", "adjustment", "interactive"
      ], RelatedDemos = [])]
  public sealed partial class ClearMarqueeAreaDemo
  {
    private ClearAreaLayoutHelper? layoutHelper;
    private ComponentAssignmentStrategy componentAssignmentStrategy = ComponentAssignmentStrategy.Single;
    private ClearAreaStrategy clearAreaStrategy = ClearAreaStrategy.PreserveShapes;
    private Dictionary<string, GraphSample> sampleGraphs = new();

    protected override void Initialize() {
      InitializeInputModes();
      DemoStyles.InitDemoStyles(graphControl.Graph);
      LoadSampleGraphs();
      LoadGraph("hierarchical");

      graphControl.Graph.GetUndoEngine()?.Clear();

      // Bind the controls to their actions.
      InitializeUI();
    }

    /// <summary>
    ///   Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    ///   and initializes the marquee input mode that clears the area of the marquee rectangle.
    /// </summary>
    private void InitializeInputModes() {
      // Enable undo/redo support.
      graphControl.Graph.SetUndoEngineEnabled(true);

      // Create an input mode to edit graphs.
      var editMode = new GraphEditorInputMode();

      // Create an input mode to clear the area of a marquee rectangle using the right mouse button.
      var marqueeClearInputMode = new MarqueeSelectionInputMode {
          MarqueeRenderer = new ClearRectangleRenderer(),
          BeginRecognizer = (_, evt) => evt is PointerEventArgs {
              Buttons: PointerButtons.MouseRight, EventType: PointerEventType.Down
          },
          MoveRecognizer = (_, evt) => evt is PointerEventArgs { EventType: PointerEventType.Drag },
          FinishRecognizer = (_, evt) => evt is PointerEventArgs {
              ChangedButtons: PointerButtons.MouseRight, EventType: PointerEventType.Up
          },
          CancelRecognizer = (source, evt) => EventRecognizers.EscapeDown(source, evt) || evt is PointerEventArgs {
              EventType: PointerEventType.DragCaptureLost
          },
          UseViewCoordinates = false
      };

      // Handle dragging the marquee.
      marqueeClearInputMode.DragStarting += OnDragStarting;
      marqueeClearInputMode.Dragged += OnDragged;
      marqueeClearInputMode.DragCanceled += OnDragCanceled;
      marqueeClearInputMode.DragFinished += OnDragFinished;

      // Add this mode to the edit mode.
      editMode.Add(marqueeClearInputMode);

      // Install the edit mode into the canvas.
      graphControl.InputMode = editMode;
    }

    /// <summary>
    ///   A renderer for the blue marquee rectangle.
    /// </summary>
    private sealed class ClearRectangleRenderer : ObjectRendererBase<MarqueeRenderTag, DrawingVisual>
    {
      private static readonly Brush FillBrush = CreateBrush(Color.FromArgb(166, 0, 187, 255));
      private static readonly Pen StrokePen = CreatePen(Color.FromArgb(166, 77, 131, 153));

      protected override DrawingVisual CreateVisual(IRenderContext context, MarqueeRenderTag renderTag) {
        var visual = new DrawingVisual();
        using var drawingContext = visual.RenderOpen();
        var bounds = renderTag.SelectionRectangle;
        drawingContext.DrawRectangle(FillBrush, StrokePen, bounds.ToRect());
        return visual;
      }

      protected override DrawingVisual UpdateVisual(
          IRenderContext context,
          DrawingVisual oldVisual,
          MarqueeRenderTag renderTag) =>
          CreateVisual(context, renderTag);

      private static Brush CreateBrush(Color color) {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
      }

      private static Pen CreatePen(Color color) {
        var pen = new Pen(CreateBrush(color), 1.5);
        pen.Freeze();
        return pen;
      }
    }

    /// <summary>
    ///   The marquee rectangle is about to be dragged.
    /// </summary>
    private void OnDragStarting(object? sender, MarqueeSelectionEventArgs e) {
      var hitGroupNode = GetHitGroupNode(e.Context, e.Context.CanvasControl!.LastPointerEvent.Location);
      layoutHelper = new ClearAreaLayoutHelper(graphControl, e.Rectangle, hitGroupNode, componentAssignmentStrategy,
          clearAreaStrategy);
      layoutHelper.InitializeLayout();
    }

    /// <summary>
    ///   The marquee rectangle is currently dragged. For each drag a new layout is calculated and
    ///   applied if the previous one is completed.
    /// </summary>
    private async void OnDragged(object? sender, MarqueeSelectionEventArgs e) {
      layoutHelper!.ClearRectangle = e.Rectangle;
      await layoutHelper.RunLayoutAsync();
    }

    /// <summary>
    ///   Dragging the marquee rectangle has been canceled, so the state before the gesture must be restored.
    /// </summary>
    private async void OnDragCanceled(object? sender, MarqueeSelectionEventArgs e) {
      layoutHelper!.ClearRectangle = e.Rectangle;
      await layoutHelper.CancelLayoutAsync();
    }

    /// <summary>
    ///   Dragging the marquee rectangle has been finished, so we execute the layout with the final rectangle.
    /// </summary>
    private async void OnDragFinished(object? sender, MarqueeSelectionEventArgs e) {
      layoutHelper!.ClearRectangle = e.Rectangle;
      await layoutHelper.StopLayoutAsync();
    }

    /// <summary>
    ///   Returns the group node at the given location. If there is no group node, <see langword="null"/> is returned.
    /// </summary>
    private INode? GetHitGroupNode(IInputModeContext context, PointD location) {
      var hits = context.Lookup<IHitTester>()!.EnumerateHits(context, location, GraphItemTypes.Node);
      return hits.OfType<INode>().FirstOrDefault(graphControl.Graph.IsGroupNode);
    }

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
    }

    /// <summary>
    ///   Registers actions for the items in the toolbar.
    /// </summary>
    private void InitializeUI() {
      AddOption(sampleGraphsComboBox, "Hierarchical", "hierarchical");
      AddOption(sampleGraphsComboBox, "Grouping", "grouping");
      AddOption(sampleGraphsComboBox, "Organic", "organic");
      AddOption(sampleGraphsComboBox, "Orthogonal", "orthogonal");
      AddOption(sampleGraphsComboBox, "Circular", "circular");
      AddOption(sampleGraphsComboBox, "Tree", "tree");
      AddOption(sampleGraphsComboBox, "Radial Tree", "radialTree");
      AddOption(sampleGraphsComboBox, "Series-Parallel", "seriesParallel");
      AddOption(sampleGraphsComboBox, "Components", "components");
      sampleGraphsComboBox.SelectedValuePath = nameof(Tag);
      sampleGraphsComboBox.SelectedValue = "hierarchical";
      sampleGraphsComboBox.SelectionChanged += (_, _) => {
        if (sampleGraphsComboBox.SelectedValue is string selectedSample) {
          LoadGraph(selectedSample);
        }
      };

      AddOption(componentAssignmentStrategiesComboBox, "Single", ComponentAssignmentStrategy.Single);
      AddOption(componentAssignmentStrategiesComboBox, "Connected", ComponentAssignmentStrategy.Connected);
      AddOption(componentAssignmentStrategiesComboBox, "Clustering", ComponentAssignmentStrategy.Clustering);
      componentAssignmentStrategiesComboBox.SelectedValuePath = nameof(Tag);
      componentAssignmentStrategiesComboBox.SelectedValue = componentAssignmentStrategy;
      componentAssignmentStrategiesComboBox.SelectionChanged += (_, _) => {
        if (componentAssignmentStrategiesComboBox.SelectedValue is ComponentAssignmentStrategy selectedStrategy) {
          componentAssignmentStrategy = selectedStrategy;
        }
      };

      AddOption(clearAreaStrategiesComboBox, "Global", ClearAreaStrategy.Global);
      AddOption(clearAreaStrategiesComboBox, "Local", ClearAreaStrategy.Local);
      AddOption(clearAreaStrategiesComboBox, "Local Uniform", ClearAreaStrategy.LocalUniform);
      AddOption(clearAreaStrategiesComboBox, "Preserve Shapes", ClearAreaStrategy.PreserveShapes);
      AddOption(clearAreaStrategiesComboBox, "Preserve Shapes Uniform", ClearAreaStrategy.PreserveShapesUniform);
      clearAreaStrategiesComboBox.SelectedValuePath = nameof(Tag);
      clearAreaStrategiesComboBox.SelectedValue = clearAreaStrategy;

      clearAreaStrategiesComboBox.SelectionChanged += (_, _) => {
        if (clearAreaStrategiesComboBox.SelectedValue is ClearAreaStrategy selectedStrategy) {
          clearAreaStrategy = selectedStrategy;
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
    private static void Main(string[] args) => Run<ClearMarqueeAreaDemo>();

    public ClearMarqueeAreaDemo() {
      InitializeComponent();
    }

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
