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
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Organic;

namespace yWorks.Demo.HeatMap
{
  [Demo("heat-map", "application-features", "Heat Map", Summary = "Shows how to display a heat map on a graph.",
      Tags = ["background", "animation"],
      Keywords = [
          "intensity", "density", "hotspot", "temperature", "thermal", "concentration", "activity", "engagement",
          "popularity", "risk", "renderer", "gradient", "heatmap", "playground"
      ])]
  public sealed partial class HeatMapDemo : IDisposable
  {
    private readonly Random random = new();
    private readonly Stopwatch animationTime = new();
    private Animator? animator;
    private HeatMapRenderer? heatMapRenderer;
    private double elapsedMilliseconds;

    public HeatMapDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      InitializeInteraction(graphControl);

      // Configures default styles for newly created graph elements.
      InitializeStyles(graphControl.Graph);

      // Builds the graph from the json data.
      BuildGraph(graphControl.Graph);

      // Layout and center the graph.
      graphControl.Graph.ApplyLayout(new OrganicLayout());
      graphControl.UpdateContentBounds();
      graphControl.ZoomTo(graphControl.ContentBounds.GetEnlarged(100));

      // Enable undo after initial population so the initial graph cannot be undone.
      graphControl.Graph.SetUndoEngineEnabled(true);

      // Initialize the heat map and start the animation.
      var getElapsedTime = StartAnimation(graphControl);
      var nodeHeatProvider = CreateHeatProvider<INode>(getElapsedTime);
      var edgeHeatProvider = CreateHeatProvider<IEdge>(getElapsedTime);
      heatMapRenderer = InitializeHeatMap(nodeHeatProvider, edgeHeatProvider);

      InitializeUI();
    }

    /// <summary>
    ///   Starts an infinite animation and returns a function that returns elapsed milliseconds.
    /// </summary>
    private Func<double> StartAnimation(CanvasControl canvasComponent) {
      animationTime.Start();
      animator = new Animator(canvasComponent) { AllowUserInteraction = true };
      _ = animator.Animate(_ => elapsedMilliseconds = animationTime.Elapsed.TotalMilliseconds, TimeSpan.MaxValue,
          CancellationToken.None);
      return () => elapsedMilliseconds;
    }

    /// <summary>
    ///   Creates the heat function that returns the heat value for an item at the current time.
    /// </summary>
    private static HeatProvider<TItem> CreateHeatProvider<TItem>(Func<double> getElapsedTime) where TItem : IModelItem {
      return item => item.Tag is ItemTag tag ? tag.GetHeat(getElapsedTime()) : 0;
    }

    /// <summary>
    ///   Initializes the HeatMapRenderer and adds it to the background group of the render tree.
    /// </summary>
    private HeatMapRenderer InitializeHeatMap(
        HeatProvider<INode> nodeHeatProvider,
        HeatProvider<IEdge> edgeHeatProvider) {
      var renderer = new HeatMapRenderer(nodeHeatProvider, edgeHeatProvider);
      var renderTree = graphControl.RenderTree;
      renderTree.CreateElement(renderTree.BackgroundGroup, graphControl.Graph, renderer);
      return renderer;
    }

    /// <summary>
    ///   Creates a tag with a random oscillator function for the heat map.
    /// </summary>
    private ItemTag CreateRandomItemTag() => new(CreateRandomOscillator());

    /// <summary>
    ///   Creates a random oscillator function that oscillates between zero and one.
    /// </summary>
    private Func<double, double> CreateRandomOscillator() {
      var period = 500 + (random.NextDouble() * 2000);
      var amplitude = 0.8 + (random.NextDouble() * 0.2);
      var phase = random.NextDouble() * 2500;
      return milliseconds => Math.Max(0, amplitude * Math.Sin((milliseconds + phase) / period));
    }

    /// <summary>
    ///   Initializes the UI elements for the demo.
    /// </summary>
    private void InitializeUI() {
      gradientComboBox.SelectedIndex = 0;
      backgroundComboBox.SelectedIndex = 0;
    }

    /// <summary>
    ///   Creates nodes and edges from the data and adds heat providers to their tags.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      BuildGraphFromJson("graph-data.json", graph);
      foreach (var node in graph.Nodes) {
        node.Tag = CreateRandomItemTag();
      }
      foreach (var edge in graph.Edges) {
        edge.Tag = CreateRandomItemTag();
      }
    }

    /// <summary>
    ///   Initializes the graph defaults used for newly created elements.
    /// </summary>
    private static void InitializeStyles(IGraph graph) {
      var fill = new SolidColorBrush(Color.FromRgb(0xc1, 0xc1, 0xc1));
      var stroke = new Pen(new SolidColorBrush(Color.FromRgb(0x4d, 0x4d, 0x4d)), 1.5);
      fill.Freeze();
      stroke.Freeze();

      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = fill,
          Pen = stroke
      };
      graph.EdgeDefaults.Style = new PolylineEdgeStyle { Pen = stroke };
    }

    /// <summary>
    ///   Adds event listeners that assign a heat function to interactively created items.
    /// </summary>
    private void InitializeInteraction(GraphControl component) {
      var graphEditorInputMode = new GraphEditorInputMode();
      component.InputMode = graphEditorInputMode;
      graphEditorInputMode.NodeCreated += (_, args) => args.Item.Tag = CreateRandomItemTag();
      graphEditorInputMode.CreateEdgeInputMode.EdgeCreated += (_, args) => args.Item.Tag = CreateRandomItemTag();
    }

    private void GradientChanged(object sender, SelectionChangedEventArgs e) {
      if (heatMapRenderer != null && gradientComboBox.SelectedItem is ComboBoxItem {
              Tag: IEnumerable<GradientStop> gradient
          }) {
        heatMapRenderer.Gradient = gradient;
        graphControl.Invalidate();
      }
    }

    private void BackgroundChanged(object sender, SelectionChangedEventArgs e) {
      if (backgroundComboBox.SelectedItem is ComboBoxItem { Tag: Brush background }) {
        graphControl.Background = background;
      }
    }

    [STAThread]
    private static void Main() => Run<HeatMapDemo>();

    public void Dispose() {
      animator?.Stop();
      animationTime.Stop();
    }

    private sealed record ItemTag(Func<double, double> GetHeat);
  }
}
