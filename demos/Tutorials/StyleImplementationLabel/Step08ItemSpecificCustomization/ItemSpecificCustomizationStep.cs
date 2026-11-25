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

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step08ItemSpecificCustomization
{
  /// <summary>Tutorial step 08: Customize labels based on their tag data.</summary>
  public class ItemSpecificCustomizationStep : TutorialStepBase
  {
    public override string Title => "08 Item-Specific Customization";

    public override string DescriptionSource => "Step08ItemSpecificCustomization.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      CustomLabelStyleTutorial.InitializeLabelModel(graphControl);
      graph.NodeDefaults.Labels.Style = new CustomLabelStyle();

      CreateLabels(graph);

      CustomLabelStyleTutorial.EnableGraphEditing(graphControl);
      CustomLabelStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateLabels(IGraph graph) {
      var node1 = graph.CreateNode(new RectD(0, 0, 30, 30));
      var node2 = graph.CreateNode(new RectD(0, 100, 30, 30));
      var node3 = graph.CreateNode(new RectD(0, 200, 30, 30));
      var node4 = graph.CreateNode(new RectD(0, 300, 30, 30));

      var label1 = graph.AddLabel(node1, "Label with text and icon");
      label1.Tag = new LabelData("resources/search.png", null);
      graph.AdjustLabelPreferredSize(label1);

      var label2 = graph.AddLabel(node2, "");
      label2.Tag = new LabelData("resources/star.png", null);
      graph.AdjustLabelPreferredSize(label2);

      var label3 = graph.AddLabel(node3, "Label with background color and icon");
      label3.Tag = new LabelData("resources/settings.png", "#9DC6D0");
      graph.AdjustLabelPreferredSize(label3);

      graph.AddLabel(node4, "This label has only text");
    }
  }
}
