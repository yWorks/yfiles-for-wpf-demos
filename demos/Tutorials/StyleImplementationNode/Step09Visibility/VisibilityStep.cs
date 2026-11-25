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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step09Visibility
{
  /// <summary>Tutorial step 09: adjusting the visibility test for overflowing visuals.</summary>
  public class VisibilityStep : TutorialStepBase
  {
    private static readonly Brush OverflowBrush = CreateOverflowBrush();
    private readonly Random random = new();
    private DispatcherTimer? animationTimer;
    private DispatcherOperation? initializeOperation;

    public override string Title => "09 Item Visibility";

    public override string DescriptionSource => "Step09Visibility.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      graphControl.HorizontalScrollBarPolicy = ScrollBarVisibility.Disabled;
      graphControl.VerticalScrollBarPolicy = ScrollBarVisibility.Disabled;

      var graph = graphControl.Graph;
      graph.NodeDefaults.Style = new CustomNodeStyle();
      graphControl.GraphModelManager.NodeRenderer = new VisibilityNodeRenderer();
      initializeOperation = graphControl.Dispatcher.InvokeAsync(() => {
        CreateSampleGraph(graph, graphControl.Viewport);
        // Move viewport so that the animation will move nodes outside and back inside
        graphControl.UpdateContentBounds();
        StartAnimation(graphControl);
      }, DispatcherPriority.Loaded);
    }

    protected override void OnCleanUp(GraphControl graphControl, Description description) {
      initializeOperation?.Abort();
      initializeOperation = null;
      animationTimer?.Stop();
      animationTimer = null;
    }

    private void CreateSampleGraph(IGraph graph, RectD viewport) {
      // Scatter pairs of nodes at random vertical positions.
      const double extent = 1200;
      const double nodeRadius = 130;
      const double nodeDiameter = nodeRadius * 2;

      for (var x = viewport.X - extent; x < viewport.Width + extent * 2; x += nodeDiameter + 50) {
        var availableHeight = Math.Max(0, viewport.Height - nodeDiameter - 100);
        var y = viewport.Y + nodeRadius + 50 + random.NextDouble() * availableHeight;
        var redNodeCenter = new PointD(x, y - nodeRadius);
        var blueNodeCenter = new PointD(x, y + nodeRadius);

        // Red nodes use the step 7 style, which has no custom visibility test.
        graph.CreateNode(RectD.FromCenter(redNodeCenter, new SizeD(120, 70)), new Step07HitTesting.CustomNodeStyle(),
            new Step07HitTesting.NodeData("#ab2346"));

        // Blue nodes use this step's style with an adjusted visibility test.
        graph.CreateNode(RectD.FromCenter(blueNodeCenter, new SizeD(120, 70)), null, new NodeData("#0b7189", true));
      }
    }

    private void StartAnimation(GraphControl graphControl) {
      var graph = graphControl.Graph;
      var initialLocations = graph.Nodes.ToDictionary(node => node, node => new PointD(node.Layout.X, node.Layout.Y));
      var stopwatch = Stopwatch.StartNew();

      animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
      animationTimer.Tick += (_, _) => {
        var offset = Math.Sin(stopwatch.Elapsed.TotalSeconds) * 300;
        foreach (var (node, initialLocation) in initialLocations) {
          var layout = node.Layout;
          graph.SetNodeLayout(node,
              new RectD(initialLocation.X + offset, initialLocation.Y, layout.Width, layout.Height));
        }
      };
      animationTimer.Start();
    }

    private static Brush CreateOverflowBrush() {
      var brush = new SolidColorBrush(Color.FromArgb(60, 127, 0, 0));
      brush.Freeze();
      return brush;
    }

    /// <summary>Delegates node rendering to each style and adds the red overflow circle.</summary>
    private sealed class VisibilityNodeRenderer : IObjectRenderer
    {
      public IVisualCreator GetVisualCreator(object? renderTag) {
        return new NodeVisualCreator((INode) renderTag!);
      }

      public IBoundsProvider GetBoundsProvider(object? renderTag) {
        var node = (INode) renderTag!;
        return node.Style.Renderer.GetBoundsProvider(node, node.Style);
      }

      public IVisibilityTestable GetVisibilityTestable(object? renderTag) {
        var node = (INode) renderTag!;
        if (node.Tag is NodeData { IsVisibleImplementation: true }) {
          return node.Style.Renderer.GetVisibilityTestable(node, node.Style);
        }

        return new LayoutVisibilityTestable(node);
      }

      public IHitTestable GetHitTestable(object? renderTag) {
        var node = (INode) renderTag!;
        return node.Style.Renderer.GetHitTestable(node, node.Style);
      }

      private sealed class LayoutVisibilityTestable(INode node) : IVisibilityTestable
      {
        public bool IsVisible(ICanvasContext context, RectD rectangle) {
          return node.Layout.ToRectD().Intersects(rectangle);
        }
      }

      private sealed class NodeVisualCreator(INode node) : IVisualCreator
      {
        public Visual CreateVisual(IRenderContext context) {
          var nodeVisualCreator = node.Style.Renderer.GetVisualCreator(node, node.Style);
          var nodeVisual = nodeVisualCreator.CreateVisual(context);
          if (node.Tag is NodeData { IsVisibleImplementation: true }) {
            return nodeVisual!;
          }

          var group = new VisualGroup();
          group.Add(CreateOverflowCircle(node.Layout));
          group.Add(nodeVisual);
          return group;
        }

        public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
          return oldVisual;
        }

        private static DrawingVisual CreateOverflowCircle(IRectangle layout) {
          var visual = new DrawingVisual();
          UpdateOverflowCircle(visual, layout);
          return visual;
        }

        private static void UpdateOverflowCircle(DrawingVisual visual, IRectangle layout) {
          var diameter = Math.Max(layout.Width, layout.Height) * 2;
          var circle = new EllipseGeometry(new Point(layout.GetCenterX(), layout.GetCenterY()), diameter * 0.5,
              diameter * 0.5);
          circle.Freeze();
          using var drawingContext = visual.RenderOpen();
          drawingContext.DrawGeometry(OverflowBrush, null, circle);
        }
      }
    }
  }
}
