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

using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step09ItemBounds
{
  /// <summary>Tutorial step 09: including overflowing edge visuals in render bounds.</summary>
  public class ItemBoundsStep : TutorialStepBase
  {
    public override string Title => "09 Item Bounds";

    public override string DescriptionSource => "Step09ItemBounds.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);

      var oldEdgeStyle = new Step08Visibility.CustomEdgeStyle(2);
      var edgeStyle = new CustomEdgeStyle(2);
      CreateSampleGraph(graph, oldEdgeStyle, edgeStyle);

      CustomEdgeStyleTutorial.FitGraphBounds(graphControl);

      graphControl.RenderTree.CreateElement(graphControl.RenderTree.InputModeGroup,
          new BoundsVisualCreator(graphControl));
    }

    private static void CreateSampleGraph(IGraph graph, IEdgeStyle oldEdgeStyle, IEdgeStyle edgeStyle) {
      var node1 = graph.CreateNode(new RectD(50, 0, 30, 30));
      var node2 = graph.CreateNode(new RectD(200, 0, 30, 30));
      graph.CreateEdge(node1, node2, oldEdgeStyle);

      var node3 = graph.CreateNode(new RectD(50, 100, 30, 30));
      var node4 = graph.CreateNode(new RectD(200, 100, 30, 30));
      graph.CreateEdge(node3, node4, edgeStyle);
    }

    /// <summary>Draws the render bounds of all edges.</summary>
    private sealed class BoundsVisualCreator(GraphControl graphControl) : IVisualCreator
    {
      public Visual CreateVisual(IRenderContext context) {
        var visual = new DrawingVisual();
        UpdateBoundsVisual(context, visual);
        return visual;
      }

      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
        if (oldVisual is DrawingVisual visual) {
          UpdateBoundsVisual(context, visual);
          return visual;
        }
        return CreateVisual(context);
      }

      private void UpdateBoundsVisual(IRenderContext context, DrawingVisual visual) {
        using var drawingContext = visual.RenderOpen();
        foreach (var edge in graphControl.Graph.Edges) {
          var boundsProvider = edge.Style.Renderer.GetBoundsProvider(edge, edge.Style);
          var bounds = boundsProvider.GetBounds(context);
          drawingContext.DrawRectangle(null, Pens.Red, new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height));
        }
      }
    }
  }
}
