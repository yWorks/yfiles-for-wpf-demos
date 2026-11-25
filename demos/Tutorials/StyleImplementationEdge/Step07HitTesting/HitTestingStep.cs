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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step07HitTesting
{
  /// <summary>Tutorial step 07: Customizing hit-testing.</summary>
  public class HitTestingStep : TutorialStepBase
  {
    public override string Title => "07 Customizing Hit-Testing";

    public override string DescriptionSource => "Step07HitTesting.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Bottom;

      CreateSampleGraph(graph);

      var inputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false, SelectableItems = GraphItemTypes.None
      };
      graphControl.InputMode = inputMode;
      AddHoverEffect(graphControl, inputMode);

      CustomEdgeStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateSampleGraph(IGraph graph) {
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.RoundRectangle,
          Brush = CreateBrush(Color.FromRgb(11, 113, 137)),
          Pen = CreatePen(Color.FromRgb(4, 45, 55))
      };

      var node1 = graph.CreateNode(new RectD(0, 0, 30, 30));
      var node2 = graph.CreateNode(new RectD(0, 200, 30, 30));
      var node3 = graph.CreateNode(new RectD(100, 0, 30, 30));
      var node4 = graph.CreateNode(new RectD(100, 200, 30, 30));

      // This edge uses the default hit-test from the previous step.
      graph.CreateEdge(node1, node2, new Step06DataFromTag.CustomEdgeStyle(5));
      // This edge uses the customized hit-test.
      graph.CreateEdge(node3, node4, new CustomEdgeStyle(5));

      graph.AddLabel(node2, "Default hit-test");
      graph.AddLabel(node4, "Custom hit-test");
    }

    private static void AddHoverEffect(GraphControl graphControl, GraphEditorInputMode inputMode) {
      graphControl.Graph.GetDecorator().Edges.HighlightRenderer.AddFactory(edge => {
        var distance = edge.Style is CustomEdgeStyle customEdgeStyle ? customEdgeStyle.Distance : 1;
        var indicatorRenderer = new EdgeStyleIndicatorRenderer {
            ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates,
            EdgeStyle = new PolylineEdgeStyle { Pen = CreatePen(Color.FromRgb(171, 35, 70), distance) }
        };
        return indicatorRenderer;
      });

      var highlights = graphControl.Highlights;
      var hoverInputMode = inputMode.ItemHoverInputMode;
      hoverInputMode.HoverItems = GraphItemTypes.Edge;
      hoverInputMode.HoveredItemChanged += (_, args) => {
        if (args.OldItem != null) {
          highlights.Remove(args.OldItem);
        }
        if (args.Item != null) {
          highlights.Add(args.Item);
        }
      };
    }

    private static Brush CreateBrush(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }

    private static Pen CreatePen(Color color, double width = 1) {
      var pen = new Pen(CreateBrush(color), width);
      pen.Freeze();
      return pen;
    }
  }
}
