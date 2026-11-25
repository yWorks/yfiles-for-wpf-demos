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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step07HitTesting
{
  /// <summary>
  ///   Tutorial step 07: Customizing hit-testing.
  /// </summary>
  public class HitTestingStep : TutorialStepBase
  {
    private static readonly Brush HighlightBrush = CreateHighlightBrush();

    public override string Title => "07 Customizing Hit-Testing";

    public override string DescriptionSource => "Step07HitTesting.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      CustomNodeStyleTutorial.InitializeTutorialDefaults(graphControl);

      var graph = graphControl.Graph;
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Bottom;
      CreateSampleGraph(graph);

      var inputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false, SelectableItems = GraphItemTypes.None
      };
      graphControl.InputMode = inputMode;
      AddHoverEffect(graphControl, inputMode);

      CustomNodeStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateSampleGraph(IGraph graph) {
      // The red node uses the default hit-test from step 6.
      var defaultHitTestNode = graph.CreateNode(new RectD(0, 0, 100, 70), new Step06RenderText.CustomNodeStyle(),
          new Step06RenderText.NodeData("#b91c3b"));
      graph.AddLabel(defaultHitTestNode, "Without custom hit test");

      // The blue node uses the customized hit-test.
      var customHitTestNode = graph.CreateNode(
          new RectD(140, 0, 100, 70), new CustomNodeStyle(), new NodeData("#0b7189"));
      graph.AddLabel(customHitTestNode, "With custom hit test");
    }

    private static void AddHoverEffect(GraphControl graphControl, GraphEditorInputMode inputMode) {
      var hoverInputMode = inputMode.ItemHoverInputMode;
      hoverInputMode.HoverItems = GraphItemTypes.Node;

      IRenderTreeElement? currentHighlight = null;
      DispatcherTimer? touchTimer = null;

      void RemoveCurrentHighlight() {
        if (currentHighlight != null) {
          graphControl.RenderTree.Remove(currentHighlight);
          currentHighlight = null;
        }
      }

      hoverInputMode.HoveredItemChanged += (_, args) => {
        touchTimer?.Stop();
        touchTimer = null;
        RemoveCurrentHighlight();
        if (args.Item is INode node) {
          currentHighlight = AddHighlight(graphControl, node);
        }
      };

      inputMode.ItemClicked += (_, args) => {
        if (args.PointerType != PointerType.Touch || args.Item is not INode node) {
          return;
        }

        touchTimer?.Stop();
        RemoveCurrentHighlight();
        var highlight = AddHighlight(graphControl, node);
        currentHighlight = highlight;

        touchTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        touchTimer.Tick += (_, _) => {
          touchTimer.Stop();
          if (ReferenceEquals(currentHighlight, highlight)) {
            RemoveCurrentHighlight();
          }
        };
        touchTimer.Start();
      };
    }

    private static IRenderTreeElement AddHighlight(GraphControl graphControl, INode node) {
      return graphControl.RenderTree.CreateElement(graphControl.RenderTree.InputModeGroup,
          new NodeHighlightVisualCreator(node));
    }

    private static Brush CreateHighlightBrush() {
      var brush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
      brush.Freeze();
      return brush;
    }

    private sealed class NodeHighlightVisualCreator(INode node) : IVisualCreator
    {
      public Visual CreateVisual(IRenderContext context) {
        var visual = new DrawingVisual();
        var layout = node.Layout;
        using var drawingContext = visual.RenderOpen();
        drawingContext.PushTransform(new TranslateTransform(layout.X, layout.Y));
        if (node.Style is CustomNodeStyle) {
          var path = CustomNodeStyle.CustomNodeVisual.CreatePath(layout.Width, layout.Height);
          drawingContext.DrawGeometry(HighlightBrush, null, path);
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
