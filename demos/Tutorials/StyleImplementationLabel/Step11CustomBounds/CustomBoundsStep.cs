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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using YFilesPens = yWorks.Controls.Pens;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step11CustomBounds
{
  /// <summary>Tutorial step 11: including overflowing label visuals in their bounds.</summary>
  public class CustomBoundsStep : TutorialStepBase
  {
    public override string Title => "11 Custom Bounds";

    public override string DescriptionSource => "Step11CustomBounds.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      CustomLabelStyleTutorial.InitializeLabelModel(graphControl);

      var labelStyle = new CustomLabelStyle();
      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.EdgeDefaults.Labels.Style = labelStyle;
      graphControl.GraphModelManager.NodeLabelRenderer = new CustomBoundsLabelRenderer();
      graphControl.GraphModelManager.EdgeLabelRenderer = new CustomBoundsLabelRenderer();

      CustomLabelStyleTutorial.CreateSimpleGraph(graph);

      CustomLabelStyleTutorial.EnableGraphEditing(graphControl);
      CustomLabelStyleTutorial.FitGraphBounds(graphControl);
    }

    /// <summary>Draws the label style's render bounds and delegates its other renderer functions.</summary>
    private sealed class CustomBoundsLabelRenderer : IObjectRenderer
    {
      public IVisualCreator GetVisualCreator(object? renderTag) {
        return new LabelVisualCreator((ILabel) renderTag!);
      }

      public IBoundsProvider GetBoundsProvider(object? renderTag) {
        var label = (ILabel) renderTag!;
        return label.Style.Renderer.GetBoundsProvider(label, label.Style);
      }

      public IVisibilityTestable GetVisibilityTestable(object? renderTag) {
        var label = (ILabel) renderTag!;
        return label.Style.Renderer.GetVisibilityTestable(label, label.Style);
      }

      public IHitTestable GetHitTestable(object? renderTag) {
        var label = (ILabel) renderTag!;
        return label.Style.Renderer.GetHitTestable(label, label.Style);
      }
    }

    private sealed class LabelVisualCreator(ILabel label) : IVisualCreator
    {
      public Visual CreateVisual(IRenderContext context) {
        var renderer = label.Style.Renderer;
        var labelVisual = renderer.GetVisualCreator(label, label.Style).CreateVisual(context);
        var bounds = renderer.GetBoundsProvider(label, label.Style).GetBounds(context);

        var visualGroup = new VisualGroup();
        visualGroup.Add(labelVisual!);
        visualGroup.Add(CreateBoundsVisual(bounds));
        return visualGroup;
      }

      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
        return oldVisual;
      }

      private static DrawingVisual CreateBoundsVisual(RectD bounds) {
        var visual = new DrawingVisual();
        using var drawingContext = visual.RenderOpen();
        drawingContext.DrawRectangle(null, YFilesPens.Red, new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height));
        return visual;
      }
    }
  }
}
