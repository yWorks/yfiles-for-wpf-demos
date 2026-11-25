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
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.BezierEdgeStyle
{
  [Demo("bezier-edge-style", "style", "Bezier Edge Style",
      Summary = "Shows how to use the curved edge style consisting of Bezier splines.",
      Tags = ["style", "curve", "interaction"],
      Keywords = [
          "handles", "indicators", "bends", "create edge input mode", "selection", "labels", "curves", "bezier"
      ])]
  public sealed partial class BezierEdgeStyleDemo
  {
    private readonly BezierSettings settings = new();
    private static readonly BezierEdgeSegmentLabelModel BezierEdgeSegmentLabelModel = new() { AutoSnapping = true };
    private static readonly BezierEdgePathLabelModel BezierEdgePathLabelModel = new() { AutoSnapping = true };
    private static readonly Graph.Styles.BezierEdgeStyle BezierEdgeStyle = CreateBezierEdgeStyle();

    public BezierEdgeStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      GraphControl.InputMode = CreateEditorMode();
      InitializeGraph();
      InitializeUI();
      DataContext = settings;
    }

    /// <summary>Creates the default input mode for the graph control.</summary>
    private GraphEditorInputMode CreateEditorMode() => new BezierGraphEditorInputMode(settings);

    /// <summary>Initializes graph defaults and creates the initial sample graph.</summary>
    private void InitializeGraph() {
      var graph = Graph;

      // We need to provide our own handles for Bezier edge bends.
      RegisterBezierDecorators();

      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = Brushes.LightGray,
          Pen = null
      };
      graph.EdgeDefaults.Style = BezierEdgeStyle;
      graph.EdgeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = Brushes.White,
          BackgroundPen = new Pen(Brushes.Orange, 1),
          Padding = new InsetsD(3),
          TextAlignment = TextAlignment.Center
      };
      graph.EdgeDefaults.Labels.LayoutParameter = BezierEdgePathLabelModel.CreateParameter(0.5);

      graph.SetUndoEngineEnabled(true);
      LoadSample(settings.SelectedSample);
    }

    private void RegisterBezierDecorators() {
      var graph = Graph;
      var decorator = graph.GetDecorator();

      decorator.Bends.Handle.AddWrapperFactory(
          bend => bend.Owner.Style is Graph.Styles.BezierEdgeStyle && bend.Owner.Bends.Count % 3 == 2,
          (bend, handle) => {
            if (handle == null) {
              return null;
            }
            var index = bend.GetIndex();
            if (settings.SmoothSegments) {
              // Smooth editing uses custom handles that synchronize the other handles of the triple.
              return (index % 3) switch {
                  0 or 1 => new OuterControlPointHandle(handle, bend),
                  _ => new InnerControlPointHandle(handle, bend)
              };
            }
            // The custom handle stores whether a point is inner or outer so the renderer can connect the outer points
            // to the inner one.
            return new SimpleControlPointHandle(handle, bend, index % 3 != 2);
          });

      // Override the default bend creator for Bezier edges.
      decorator.Edges.BendCreator.AddWrapperFactory(edge => edge.Style is Graph.Styles.BezierEdgeStyle,
          (edge, coreImplementation) =>
              coreImplementation == null ? null : new BezierBendCreator(edge, coreImplementation));

      // Always show the outer control point handles of selected Bezier edges.
      decorator.Edges.HandleProvider.AddWrapperFactory(edge => edge.Style is Graph.Styles.BezierEdgeStyle,
          (edge, coreImplementation) => new BezierEdgeSelectionHandleProvider(edge, coreImplementation));

      var graphEditorInputMode = GraphControl.InputMode as GraphEditorInputMode ??
          throw new InvalidOperationException("The graph editor input mode is not installed.");
      var handleInputMode = new HandleInputMode {
          // Connect outer control point handles with their inner control points.
          HandlesRenderer = new BezierHandlesRenderer(new HandlesRenderer())
      };

      // Temporarily disable the custom provider while an unselected bend is dragged when other handles are present.
      var draggingBend = false;
      graphEditorInputMode.CreateBendInputMode.Dragging += (_, _) => draggingBend = true;
      handleInputMode.DragFinished += (_, _) => {
        if (draggingBend) {
          draggingBend = false;
          RequeryHandles();
        }
      };
      handleInputMode.DragCanceled += (_, _) => {
        if (draggingBend) {
          draggingBend = false;
          RequeryHandles();
        }
      };

      graphEditorInputMode.HandleInputMode = handleInputMode;

      // Provide the whole control point triplet when its inner bend is selected.
      decorator.Bends.HandleProvider.AddWrapperFactory(
          bend => bend.Owner.Style is Graph.Styles.BezierEdgeStyle && !draggingBend,
          (bend, _) => new BezierBendSelectionHandleProvider(bend, GraphControl.Selection.Edges.Contains(bend.Owner)));
    }

    // Removing bends also affects our handles, so requery to obtain fresh handles.
    private void RequeryHandles() {
      if (GraphControl.InputMode is GraphEditorInputMode graphEditorInputMode) {
        graphEditorInputMode.RequeryHandles();
      }
    }

    /// <summary>Loads a sample graph, its edge bends, ports, and labels.</summary>
    private void LoadSample(SampleKind sample) {
      var graphData = LoadGraphData(sample);
      var graph = Graph;
      graph.Clear();

      var builder = new GraphBuilder(graph);
      var nodesSource = builder.CreateNodesSource(graphData.Nodes, node => node.Id);
      nodesSource.NodeCreator.LayoutProvider = node =>
          new RectD(node.Layout.X, node.Layout.Y, node.Layout.Width, node.Layout.Height);

      var edgesSource =
          builder.CreateEdgesSource(graphData.Edges, edge => edge.Source, edge => edge.Target, edge => edge.Id);
      edgesSource.EdgeCreator.BendsProvider = edge => edge.Bends.Select(bend => new PointD(bend.X, bend.Y));

      var labelCreator = edgesSource.EdgeCreator.CreateLabelsSource(edge => edge.Labels).LabelCreator;
      labelCreator.TextProvider = label => label.Text;
      labelCreator.LayoutParameterProvider = label => {
        if (label.Model == "segment") {
          return label.FromSource
              ? BezierEdgeSegmentLabelModel.CreateParameterFromSource(label.SegmentIndex, label.SegmentRatio,
                  label.Distance)
              : BezierEdgeSegmentLabelModel.CreateParameterFromTarget(label.SegmentIndex, label.SegmentRatio,
                  label.Distance);
        }
        return BezierEdgePathLabelModel.CreateParameter(label.Ratio, label.Distance);
      };

      builder.BuildGraph();
      foreach (var edgeData in graphData.Edges) {
        var edge = builder.GetEdgeForItem(edgeData) ??
            throw new InvalidOperationException("The graph builder did not create an edge for the sample data.");
        graph.SetPortLocation(edge.SourcePort, new PointD(edgeData.SourcePort.X, edgeData.SourcePort.Y));
        graph.SetPortLocation(edge.TargetPort, new PointD(edgeData.TargetPort.X, edgeData.TargetPort.Y));
      }

      GraphControl.FitGraphBounds();
      graph.GetUndoEngine()?.Clear();
    }

    /// <summary>Loads and deserializes the selected embedded sample graph.</summary>
    private GraphData LoadGraphData(SampleKind sample) {
      var resourceName = sample == SampleKind.CircularLayout ? "circle-sample-data.json" : "label-sample-data.json";
      return JsonSerializer.Deserialize<GraphData>(LoadAssemblyResource(resourceName))!;
    }

    private void InitializeUI() {
      settings.SettingChanged += (_, args) => {
        if (args.PropertyName == nameof(BezierSettings.SelectedSample)) {
          LoadSample(settings.SelectedSample);
          return;
        }

        if (args.PropertyName == nameof(BezierSettings.SmoothSegments) &&
            GraphControl.InputMode is BezierGraphEditorInputMode graphEditorInputMode &&
            graphEditorInputMode.CreateEdgeInputMode is BezierCreateEdgeInputMode createEdgeInputMode) {
          createEdgeInputMode.CreateSmoothSplines = settings.SmoothSegments;
          graphEditorInputMode.RequeryHandles();
        }

        if (args.PropertyName == nameof(BezierSettings.AutoRotation)) {
          BezierEdgeSegmentLabelModel.AutoRotation = settings.AutoRotation;
          BezierEdgePathLabelModel.AutoRotation = settings.AutoRotation;
        } else if (args.PropertyName == nameof(BezierSettings.AutoSnapping)) {
          BezierEdgeSegmentLabelModel.AutoSnapping = settings.AutoSnapping;
          BezierEdgePathLabelModel.AutoSnapping = settings.AutoSnapping;
        } else if (args.PropertyName == nameof(BezierSettings.Angle)) {
          var radians = settings.Angle * Math.PI / 180.0;
          BezierEdgeSegmentLabelModel.Angle = radians;
          BezierEdgePathLabelModel.Angle = radians;
        }

        GraphControl.UpdateVisual();
      };
    }

    private static Graph.Styles.BezierEdgeStyle CreateBezierEdgeStyle() {
      var brush = Brushes.RoyalBlue;
      var pen = new Pen(brush, 3);
      pen.Freeze();
      return new Graph.Styles.BezierEdgeStyle {
          Pen = pen,
          TargetArrow = new Arrow(ArrowType.Triangle, brush, pen)
      };
    }

    [STAThread]
    private static void Main() => Run<BezierEdgeStyleDemo>();
  }
}
