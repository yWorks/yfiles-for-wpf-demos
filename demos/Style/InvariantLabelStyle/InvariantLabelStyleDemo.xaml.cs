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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.InvariantLabelStyle
{
  /// <summary>
  ///   Shows label styles that are independent of the zoom level.
  /// </summary>
  [Demo("invariant-label-style", "style", "Zoom-invariant Label Style",
      Summary = "Shows label styles that are independent of the zoom level.", Tags = ["style", "label", "zoom"],
      Keywords = ["size", "fit"])]
  public sealed partial class InvariantLabelStyleDemo
  {
    private LabelStyle baseLabelStyle = null!;

    public InvariantLabelStyleDemo() {
      InitializeComponent();
    }

    private IGraph Graph => graphControl.Graph;

    protected override void Initialize() {
      graphControl.InputMode = new GraphViewerInputMode {
          SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge | GraphItemTypes.Label
      };

      DemoStyles.InitDemoStyles(Graph);
      // For the general appearance of a label, we use the common demo defaults set above.
      baseLabelStyle = Graph.NodeDefaults.Labels.Style as LabelStyle ??
          throw new InvalidOperationException("The demo label style must be a LabelStyle.");

      // Initially, use FIXED_BELOW_THRESHOLD mode.
      SetLabelStyle(LabelScalingMode.FixedBelowThreshold, GetZoomThresholds());

      // Build the graph from the given data set.
      BuildGraph(Graph, LoadGraphData());

      // Layout and center the graph.
      Graph.ApplyLayout(new HierarchicalLayout {
          MinimumLayerDistance = 100,
          EdgeDistance = 100,
          NodeToEdgeDistance = 100
      });
      graphControl.FitGraphBounds();

      // Enable undo after the initial graph was populated since we don't want to allow undoing that.
      Graph.SetUndoEngineEnabled(true);

      // Instances of ZoomInvariantLabelStyleBase should not be shared between graph items.
      Graph.NodeDefaults.Labels.ShareStyleInstance = false;
      Graph.EdgeDefaults.Labels.ShareStyleInstance = false;

      InitializeUI();
      graphControl.ZoomChanged += (_, _) => UpdateZoomLevel();
      UpdateZoomLevel();
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph, GraphData graphData) {
      var graphBuilder = new GraphBuilder(graph);

      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodeList, item => item.Id);
      nodesSource.NodeCreator.CreateLabelBinding(item => item.Label);
      nodesSource.NodeCreator.LayoutProvider = item => new RectD(0, 0, item.Tag == "level 1" ? 100 : 30, 70);
      nodesSource.NodeCreator.TagProvider = item => item.Tag;

      var edgesSource = graphBuilder.CreateEdgesSource(graphData.EdgeList, item => item.Source, item => item.Target);
      edgesSource.EdgeCreator.CreateLabelBinding(item => item.Label);

      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Loads and deserializes the sample graph data embedded as <c>graph-data.json</c>.
    /// </summary>
    private GraphData LoadGraphData() =>
        JsonSerializer.Deserialize<GraphData>(LoadAssemblyResource("graph-data.json")) ??
        throw new InvalidOperationException("The sample graph data could not be loaded.");

    /// <summary>
    ///   Sets a new label style for the given mode to all labels in the graph.
    /// </summary>
    private void SetLabelStyle(LabelScalingMode mode, (double lowerThreshold, double upperThreshold) thresholds) {
      var nodeStyle = CreateLabelStyle(mode, thresholds);
      var edgeStyle = CreateLabelStyle(mode, thresholds);

      Graph.NodeDefaults.Labels.Style = nodeStyle;
      Graph.EdgeDefaults.Labels.Style = edgeStyle;
      // To make sure that the selection rectangle has the correct label bounds, the label style should not be shared.
      Graph.NodeDefaults.Labels.ShareStyleInstance = false;
      Graph.EdgeDefaults.Labels.ShareStyleInstance = false;

      foreach (var label in Graph.Labels) {
        Graph.SetStyle(label, CreateLabelStyle(mode, thresholds));
      }
      graphControl.Invalidate();
    }

    /// <summary>
    ///   Creates a new label style for the selected mode and base style.
    /// </summary>
    private ILabelStyle CreateLabelStyle(
        LabelScalingMode mode,
        (double lowerThreshold, double upperThreshold) thresholds) {
      return mode switch {
          LabelScalingMode.FixedAboveThreshold => new ZoomInvariantAboveThresholdLabelStyle(baseLabelStyle,
              thresholds.lowerThreshold),
          LabelScalingMode.FixedBelowThreshold => new ZoomInvariantBelowThresholdLabelStyle(baseLabelStyle,
              thresholds.lowerThreshold),
          LabelScalingMode.InvariantOutsideRange => new ZoomInvariantOutsideRangeLabelStyle(baseLabelStyle,
              thresholds.lowerThreshold, thresholds.upperThreshold),
          LabelScalingMode.FitOwner => new FitOwnerLabelStyle(baseLabelStyle),
          _ => baseLabelStyle
      };
    }

    /// <summary>
    ///   Wires up the user interface.
    /// </summary>
    private void InitializeUI() {
      modeChooserBox.ItemsSource = new[] {
          new LabelModeOption(LabelScalingMode.FixedAboveThreshold, "Fixed above zoom threshold"),
          new LabelModeOption(LabelScalingMode.FixedBelowThreshold, "Fixed below zoom threshold"),
          new LabelModeOption(LabelScalingMode.InvariantOutsideRange, "Fixed when outside specified range"),
          new LabelModeOption(LabelScalingMode.FitOwner, "Fit into the label's owner"),
          new LabelModeOption(LabelScalingMode.Default, "Default behaviour")
      };
      modeChooserBox.SelectedIndex = 1;
      modeChooserBox.SelectionChanged += (_, _) => {
        if (modeChooserBox.SelectedItem is not LabelModeOption option) {
          return;
        }

        SetLabelStyle(option.Mode, GetZoomThresholds());

        // Hide the threshold controls if not applicable for the selected zoom style.
        zoomThresholdControls.Visibility = option.Mode is LabelScalingMode.Default or LabelScalingMode.FitOwner
            ? Visibility.Collapsed
            : Visibility.Visible;

        // Hide the max scale controls if not applicable for the selected zoom style.
        maxScaleControls.Visibility = option.Mode == LabelScalingMode.InvariantOutsideRange
            ? Visibility.Visible
            : Visibility.Collapsed;
      };

      // Adds an event handler to the threshold slider.
      zoomThresholdSlider.ValueChanged += (_, _) => {
        if (modeChooserBox.SelectedItem is LabelModeOption option) {
          SetLabelStyle(option.Mode, GetZoomThresholds());
        }
      };

      maxScaleSlider.ValueChanged += (_, _) => {
        if (modeChooserBox.SelectedItem is LabelModeOption option) {
          SetLabelStyle(option.Mode, GetZoomThresholds());
        }
      };
    }

    private (double lowerThreshold, double upperThreshold) GetZoomThresholds() =>
        (zoomThresholdSlider.Value, maxScaleSlider.Value);

    /// <summary>
    ///   Shows the current level of zoom in the toolbar.
    /// </summary>
    private void UpdateZoomLevel() {
      zoomLevel.Content = graphControl.Zoom.ToString("0.00");
    }

    private enum LabelScalingMode
    {
      FixedAboveThreshold,
      FixedBelowThreshold,
      InvariantOutsideRange,
      FitOwner,
      Default
    }

    private sealed record LabelModeOption(LabelScalingMode Mode, string DisplayName)
    {
      public override string ToString() => DisplayName;
    }

    private sealed class GraphData
    {
      [JsonPropertyName("nodeList")]
      public List<NodeData> NodeList { get; set; } = [];

      [JsonPropertyName("edgeList")]
      public List<EdgeData> EdgeList { get; set; } = [];
    }

    private sealed class NodeData
    {
      [JsonPropertyName("id")]
      public int Id { get; set; }

      [JsonPropertyName("label")]
      public string Label { get; } = "";

      [JsonPropertyName("tag")]
      public string Tag { get; } = "";
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
      public string Label { get; } = "";
    }

    [STAThread]
    public static void Main() {
      Run<InvariantLabelStyleDemo>();
    }
  }
}
