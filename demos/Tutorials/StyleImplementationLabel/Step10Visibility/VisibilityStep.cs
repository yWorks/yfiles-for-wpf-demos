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
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step10Visibility
{
  /// <summary>Tutorial step 10: adjusting label visibility for overflowing visuals.</summary>
  public class VisibilityStep : TutorialStepBase
  {
    private readonly Random random = new();
    private DispatcherOperation? initializeOperation;
    private DispatcherTimer? animationTimer;

    public override string Title => "10 Item Visibility";

    public override string DescriptionSource => "Step10Visibility.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      graphControl.HorizontalScrollBarPolicy = ScrollBarVisibility.Disabled;
      graphControl.VerticalScrollBarPolicy = ScrollBarVisibility.Disabled;

      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      InitializeLabelModel(graph);

      var labelStyle = new CustomLabelStyle();
      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.EdgeDefaults.Labels.Style = labelStyle;
      graphControl.GraphModelManager.NodeLabelRenderer = new VisibilityLabelRenderer();
      graphControl.GraphModelManager.EdgeLabelRenderer = new VisibilityLabelRenderer();

      graphControl.InputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false, SelectableItems = GraphItemTypes.None
      };

      initializeOperation = graphControl.Dispatcher.InvokeAsync(() => {
        CreateSampleGraph(graph, graphControl.Viewport);
        // Move the viewport so the animation moves the labels outside and back inside.
        graphControl.UpdateContentBounds();
        StartAnimation(graph);
      }, DispatcherPriority.Loaded);
    }

    protected override void OnCleanUp(GraphControl graphControl, Description description) {
      initializeOperation?.Abort();
      initializeOperation = null;
      animationTimer?.Stop();
      animationTimer = null;
    }

    private static void InitializeLabelModel(IGraph graph) {
      var smartEdgeLabelModel = new SmartEdgeLabelModel();
      graph.EdgeDefaults.Labels.LayoutParameter = smartEdgeLabelModel.CreateParameterFromSource(0, -50);
      var exteriorNodeLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(50) };
      graph.NodeDefaults.Labels.LayoutParameter =
          exteriorNodeLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Top);
    }

    private void CreateSampleGraph(IGraph graph, RectD area) {
      // Scatter nodes at random x positions along the y axis.
      const double extent = 1200;
      const double nodeRadius = 70;
      const double nodeDiameter = nodeRadius * 2;

      for (var y = area.Y - extent; y < area.Height + extent * 2; y += nodeDiameter) {
        var x = area.X + nodeRadius + 50 + random.NextDouble() * (area.Width - nodeDiameter - 100);
        var firstNode = graph.CreateNode(new RectD(x - nodeRadius, y, 30, 30));
        var defaultLabel = graph.AddLabel(firstNode, "Default visibility");
        defaultLabel.Tag = new LabelData(false, "#b91c3b");

        var secondNode = graph.CreateNode(new RectD(x + nodeRadius, y, 30, 30));
        var adjustedLabel = graph.AddLabel(secondNode, "Adjusted visibility");
        adjustedLabel.Tag = new LabelData(true, null);
      }
    }

    private void StartAnimation(IGraph graph) {
      var initialLocations = graph.Nodes.ToDictionary(node => node, node => new PointD(node.Layout.X, node.Layout.Y));
      var stopwatch = Stopwatch.StartNew();

      animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
      animationTimer.Tick += (_, _) => {
        var offset = Math.Sin(stopwatch.Elapsed.TotalSeconds) * 100;
        foreach (var (node, initialLocation) in initialLocations) {
          var layout = node.Layout;
          graph.SetNodeLayout(node,
              new RectD(initialLocation.X, initialLocation.Y + offset, layout.Width, layout.Height));
        }
      };
      animationTimer.Start();
    }

    /// <summary>Delegates label rendering and selects the visibility test from each label's tag.</summary>
    private sealed class VisibilityLabelRenderer : IObjectRenderer
    {
      public IVisualCreator GetVisualCreator(object? renderTag) {
        var label = (ILabel) renderTag!;
        return label.Style.Renderer.GetVisualCreator(label, label.Style);
      }

      public IBoundsProvider GetBoundsProvider(object? renderTag) {
        var label = (ILabel) renderTag!;
        return label.Style.Renderer.GetBoundsProvider(label, label.Style);
      }

      public IVisibilityTestable GetVisibilityTestable(object? renderTag) {
        var label = (ILabel) renderTag!;
        if (label.Tag is LabelData { IsVisibleImplementation: true }) {
          return label.Style.Renderer.GetVisibilityTestable(label, label.Style);
        }
        return new LayoutVisibilityTestable(label);
      }

      public IHitTestable GetHitTestable(object? renderTag) {
        var label = (ILabel) renderTag!;
        return label.Style.Renderer.GetHitTestable(label, label.Style);
      }

      private sealed class LayoutVisibilityTestable(ILabel label) : IVisibilityTestable
      {
        public bool IsVisible(ICanvasContext context, RectD rectangle) {
          return rectangle.Intersects(label.GetLayout(), CustomLabelStyle.Epsilon);
        }
      }
    }
  }
}
