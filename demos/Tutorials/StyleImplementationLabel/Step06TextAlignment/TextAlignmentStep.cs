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

using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step06TextAlignment
{
  /// <summary>Tutorial step 06: Add horizontal and vertical text alignment to the label style.</summary>
  public class TextAlignmentStep : TutorialStepBase
  {
    public override string Title => "06 Text Alignment";

    public override string DescriptionSource => "Step06TextAlignment.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      CustomLabelStyleTutorial.InitializeLabelModel(graphControl);

      var style = new CustomLabelStyle();
      graph.NodeDefaults.Labels.Style = style;
      graph.EdgeDefaults.Labels.Style = style;

      CreateSampleGraph(graph);

      CustomLabelStyleTutorial.EnableGraphEditing(graphControl);
      CustomLabelStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateSampleGraph(IGraph graph) {
      const double distance = 120;
      AddLabel(graph, new RectD(0, 0, 30, 30), "Top Left", CustomLabelStyle.HorizontalAlignment.START,
          CustomLabelStyle.VerticalAlignment.TOP);
      AddLabel(graph, new RectD(distance, 0, 30, 30), "Top Center", CustomLabelStyle.HorizontalAlignment.MIDDLE,
          CustomLabelStyle.VerticalAlignment.TOP);
      AddLabel(graph, new RectD(2 * distance, 0, 30, 30), "Top Right", CustomLabelStyle.HorizontalAlignment.END,
          CustomLabelStyle.VerticalAlignment.TOP);
      AddLabel(graph, new RectD(0, distance, 30, 30), "Center Left", CustomLabelStyle.HorizontalAlignment.START,
          CustomLabelStyle.VerticalAlignment.CENTER);
      AddLabel(graph, new RectD(distance, distance, 30, 30), "Centered", CustomLabelStyle.HorizontalAlignment.MIDDLE,
          CustomLabelStyle.VerticalAlignment.CENTER);
      AddLabel(graph, new RectD(2 * distance, distance, 30, 30), "Center Right",
          CustomLabelStyle.HorizontalAlignment.END, CustomLabelStyle.VerticalAlignment.CENTER);
      AddLabel(graph, new RectD(0, 2 * distance, 30, 30), "Bottom Left", CustomLabelStyle.HorizontalAlignment.START,
          CustomLabelStyle.VerticalAlignment.BOTTOM);
      AddLabel(graph, new RectD(distance, 2 * distance, 30, 30), "Bottom Center",
          CustomLabelStyle.HorizontalAlignment.MIDDLE, CustomLabelStyle.VerticalAlignment.BOTTOM);
      AddLabel(graph, new RectD(2 * distance, 2 * distance, 30, 30), "Bottom Right",
          CustomLabelStyle.HorizontalAlignment.END, CustomLabelStyle.VerticalAlignment.BOTTOM);
    }

    private static void AddLabel(
        IGraph graph,
        RectD nodeLayout,
        string text,
        CustomLabelStyle.HorizontalAlignment horizontalAlignment,
        CustomLabelStyle.VerticalAlignment verticalAlignment) {
      var node = graph.CreateNode(nodeLayout);
      graph.AddLabel(node, text, null, new CustomLabelStyle(horizontalAlignment, verticalAlignment));
    }
  }
}
