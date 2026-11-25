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
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using PreviousLabelData = yWorks.Demo.Tutorial.StyleImplementationLabel.Step08ItemSpecificCustomization.LabelData;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step09HitTesting
{
  /// <summary>Tutorial step 09: customizing label hit-testing.</summary>
  public class HitTestingStep : TutorialStepBase
  {
    private static readonly Brush HighlightBrush = CreateHighlightBrush();
    private IRenderTreeElement? currentHighlight;
    private DispatcherTimer? touchTimer;

    public override string Title => "09 Customizing Hit-Testing";

    public override string DescriptionSource => "Step09HitTesting.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      InitializeLabelModel(graph);

      graphControl.GraphModelManager.NodeLabelRenderer = new IsHitLabelStyleRenderer();
      graphControl.GraphModelManager.EdgeLabelRenderer = new IsHitLabelStyleRenderer();

      var labelStyle = new CustomLabelStyle();
      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.EdgeDefaults.Labels.Style = labelStyle;

      CreateSampleGraph(graph);

      var inputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false, SelectableItems = GraphItemTypes.None
      };
      graphControl.InputMode = inputMode;
      AddHoverEffect(graphControl, inputMode);

      CustomLabelStyleTutorial.FitGraphBounds(graphControl);
    }

    protected override void OnCleanUp(GraphControl graphControl, Description description) {
      touchTimer?.Stop();
      touchTimer = null;
      RemoveCurrentHighlight(graphControl);
    }

    private static void InitializeLabelModel(IGraph graph) {
      var smartEdgeLabelModel = new SmartEdgeLabelModel();
      graph.EdgeDefaults.Labels.LayoutParameter = smartEdgeLabelModel.CreateParameterFromSource(0, -20);
      var exteriorNodeLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(20) };
      graph.NodeDefaults.Labels.LayoutParameter =
          exteriorNodeLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Top);
    }

    private void AddHoverEffect(GraphControl graphControl, GraphEditorInputMode inputMode) {
      var hoverInputMode = inputMode.ItemHoverInputMode;
      hoverInputMode.HoverItems = GraphItemTypes.Label;
      hoverInputMode.HoveredItemChanged += (_, args) => {
        touchTimer?.Stop();
        touchTimer = null;
        RemoveCurrentHighlight(graphControl);
        if (args.Item is ILabel label) {
          currentHighlight = AddHighlight(graphControl, label);
        }
      };

      inputMode.ItemClicked += (_, args) => {
        if (args.PointerType != PointerType.Touch || args.Item is not ILabel label) {
          return;
        }

        touchTimer?.Stop();
        RemoveCurrentHighlight(graphControl);
        var highlight = AddHighlight(graphControl, label);
        currentHighlight = highlight;

        touchTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        touchTimer.Tick += (_, _) => {
          touchTimer.Stop();
          if (ReferenceEquals(currentHighlight, highlight)) {
            RemoveCurrentHighlight(graphControl);
          }
        };
        touchTimer.Start();
      };
    }

    private static IRenderTreeElement AddHighlight(GraphControl graphControl, ILabel label) {
      return graphControl.RenderTree.CreateElement(graphControl.RenderTree.InputModeGroup,
          new LabelHighlightVisualCreator(label));
    }

    private void RemoveCurrentHighlight(GraphControl graphControl) {
      if (currentHighlight != null) {
        graphControl.RenderTree.Remove(currentHighlight);
        currentHighlight = null;
      }
    }

    private static void CreateSampleGraph(IGraph graph) {
      var oldNode1 = graph.CreateNode(new RectD(-200, 0, 30, 30));
      var oldLabel1 = graph.AddLabel(oldNode1, "Old Label Style");
      oldLabel1.Tag = new PreviousLabelData(null, "#b91c3b");

      var oldNode2 = graph.CreateNode(new RectD(-100, 200, 30, 30));
      var oldLabel2 = graph.AddLabel(oldNode2, "Old Label Style");
      oldLabel2.Tag = new PreviousLabelData(null, "#b91c3b");

      var oldEdge = graph.CreateEdge(oldNode1, oldNode2);
      var oldEdgeLabel = graph.AddLabel(oldEdge, "Old Label Style");
      oldEdgeLabel.Tag = new PreviousLabelData(null, "#b91c3b");

      var node1 = graph.CreateNode();
      var label1 = graph.AddLabel(node1, "Node Label 1");
      label1.Tag = new LabelHitTestData(true);

      var node2 = graph.CreateNode(new RectD(100, 200, 30, 30));
      var label2 = graph.AddLabel(node2, "Node Label 2");
      label2.Tag = new LabelHitTestData(true);

      var edge = graph.CreateEdge(node1, node2);
      var edgeLabel = graph.AddLabel(edge, "Edge Label");
      edgeLabel.Tag = new LabelHitTestData(true);
    }

    private static Brush CreateHighlightBrush() {
      var brush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
      brush.Freeze();
      return brush;
    }

    private sealed record LabelHitTestData(bool IsHitImplementation);

    private sealed class IsHitLabelStyleRenderer : IObjectRenderer
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
        return label.Style.Renderer.GetVisibilityTestable(label, label.Style);
      }

      public IHitTestable GetHitTestable(object? renderTag) {
        var label = (ILabel) renderTag!;
        if (label.Tag is LabelHitTestData { IsHitImplementation: true }) {
          return label.Style.Renderer.GetHitTestable(label, label.Style);
        }
        return new LayoutHitTestable(label);
      }

      private sealed class LayoutHitTestable(ILabel label) : IHitTestable
      {
        public bool IsHit(IInputModeContext context, PointD location) {
          return label.GetLayout().Contains(location, context.HitTestRadius);
        }
      }
    }

    private sealed class LabelHighlightVisualCreator(ILabel label) : IVisualCreator
    {
      public Visual CreateVisual(IRenderContext context) {
        var layout = label.GetLayout();
        var visual = new DrawingVisual();
        var transform = new Matrix(-layout.UpY, layout.UpX, -layout.UpX, -layout.UpY,
            layout.AnchorX + layout.UpX * layout.Height, layout.AnchorY + layout.UpY * layout.Height);
        using var drawingContext = visual.RenderOpen();
        drawingContext.PushTransform(new MatrixTransform(transform));
        if (label.Style is CustomLabelStyle && label.Tag is LabelHitTestData { IsHitImplementation: true }) {
          drawingContext.DrawGeometry(HighlightBrush, null,
              CustomLabelStyle.CustomLabelVisual.CreateBackgroundShape(layout.Width, layout.Height));
        } else {
          drawingContext.DrawRectangle(HighlightBrush, null, new Rect(0, 0, layout.Width, layout.Height));
        }
        drawingContext.Pop();
        return visual;
      }

      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
        return CreateVisual(context);
      }
    }
  }
}
