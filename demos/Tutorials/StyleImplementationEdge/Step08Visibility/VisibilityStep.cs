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
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step08Visibility
{
  /// <summary>Tutorial step 08: adjusting the visibility test for overflowing edge visuals.</summary>
  public class VisibilityStep : TutorialStepBase
  {
    private DispatcherOperation? initializeOperation;
    private DispatcherTimer? animationTimer;

    public override string Title => "08 Item Visibility";

    public override string DescriptionSource => "Step08Visibility.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      graphControl.GraphModelManager.EdgeRenderer = new VisibilityEdgeRenderer();
      graphControl.HorizontalScrollBarPolicy = ScrollBarVisibility.Disabled;
      graphControl.VerticalScrollBarPolicy = ScrollBarVisibility.Disabled;
      graph.EdgeDefaults.Style = new CustomEdgeStyle();

      graphControl.InputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false, SelectableItems = GraphItemTypes.None
      };
      initializeOperation = graphControl.Dispatcher.InvokeAsync(() => {
        CreateSampleGraph(graph);

        graphControl.FitGraphBounds();
        graphControl.Zoom = 2;
        StartAnimation(graph);
      }, DispatcherPriority.Loaded);
    }

    protected override void OnCleanUp(GraphControl graphControl, Description description) {
      initializeOperation?.Abort();
      initializeOperation = null;
      animationTimer?.Stop();
      animationTimer = null;
    }

    private static void CreateSampleGraph(IGraph graph) {
      for (var i = 0; i < 40; i++) {
        var node1 = graph.CreateNode(new RectD(i * 50, 0, 30, 30));
        var node2 = graph.CreateNode(new RectD(i * 50, 100, 30, 30));
        var node3 = graph.CreateNode(new RectD(i * 50, 200, 30, 30));

        var edge1 = graph.CreateEdge(node1, node2);
        edge1.Tag = new EdgeData(false);

        var edge2 = graph.CreateEdge(node2, node3);
        edge2.Tag = new EdgeData(true);
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
              new RectD(initialLocation.X + offset, initialLocation.Y, layout.Width, layout.Height));
        }
      };
      animationTimer.Start();
    }

    /// <summary>Delegates edge rendering and adjusts the visibility test for selected edges.</summary>
    private sealed class VisibilityEdgeRenderer : IObjectRenderer
    {
      public IVisualCreator GetVisualCreator(object? renderTag) {
        return new EdgeVisualCreator((IEdge) renderTag!);
      }

      public IBoundsProvider GetBoundsProvider(object? renderTag) {
        var edge = (IEdge) renderTag!;
        return edge.Style.Renderer.GetBoundsProvider(edge, edge.Style);
      }

      public IVisibilityTestable GetVisibilityTestable(object? renderTag) {
        var edge = (IEdge) renderTag!;
        if (edge.Tag is EdgeData { IsVisibleImplementation: true }) {
          return edge.Style.Renderer.GetVisibilityTestable(edge, edge.Style);
        }
        return new BoundsVisibilityTestable(edge);
      }

      public IHitTestable GetHitTestable(object? renderTag) {
        var edge = (IEdge) renderTag!;
        return edge.Style.Renderer.GetHitTestable(edge, edge.Style);
      }

      private sealed class BoundsVisibilityTestable(IEdge edge) : IVisibilityTestable
      {
        public bool IsVisible(ICanvasContext context, RectD rectangle) {
          return edge.Style.Renderer.GetBoundsProvider(edge, edge.Style).GetBounds(context).Intersects(rectangle);
        }
      }

      private sealed class EdgeVisualCreator(IEdge edge) : IVisualCreator
      {
        public Visual? CreateVisual(IRenderContext context) {
          var edgeStyle = edge.Style;
          var visual = edgeStyle.Renderer.GetVisualCreator(edge, edgeStyle).CreateVisual(context);
          if (visual is CustomEdgeStyle.CustomEdgeVisual edgeVisual) {
            edgeVisual.UpdateCircleColor(edge.Tag is EdgeData { IsVisibleImplementation: true });
          }
          return visual;
        }

        public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
          return oldVisual;
        }
      }
    }
  }
}
