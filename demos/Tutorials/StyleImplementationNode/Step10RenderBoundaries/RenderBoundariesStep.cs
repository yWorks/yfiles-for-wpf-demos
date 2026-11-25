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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step10RenderBoundaries
{
  /// <summary>Tutorial step 10: including overflowing visuals in item bounds.</summary>
  public class RenderBoundariesStep : TutorialStepBase
  {
    private static readonly Pen BoundsPen = CreateBoundsPen();

    public override string Title => "10 Render Boundaries";

    public override string DescriptionSource => "Step10RenderBoundaries.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Style = new CustomNodeStyle();
      graph.CreateNode(new RectD(0, 0, 100, 70), new CustomNodeStyle(), new NodeData(true));

      CustomNodeStyleTutorial.EnableGraphEditing(graphControl);
      CustomNodeStyleTutorial.FitGraphBounds(graphControl);

      graphControl.RenderTree.CreateElement(graphControl.RenderTree.InputModeGroup,
          new BoundsVisualCreator(graphControl));
    }

    private static Pen CreateBoundsPen() {
      var pen = new Pen(Brushes.Red, 1);
      pen.Freeze();
      return pen;
    }

    /// <summary>Draws the combined render bounds of all nodes.</summary>
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
        var graph = graphControl.Graph;
        var bounds = RectD.Empty;
        foreach (var node in graph.Nodes) {
          var boundsProvider = node.Style.Renderer.GetBoundsProvider(node, node.Style);
          bounds += boundsProvider.GetBounds(context).GetEnlarged(2);
        }

        using var drawingContext = visual.RenderOpen();
        drawingContext.DrawRectangle(null, BoundsPen, new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height));
      }
    }
  }
}
