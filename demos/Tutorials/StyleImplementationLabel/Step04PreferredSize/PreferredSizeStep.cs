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

namespace yWorks.Demo.Tutorial.StyleImplementationLabel.Step04PreferredSize
{
  /// <summary>Tutorial step 04: Calculate the preferred size of a label from its text.</summary>
  public class PreferredSizeStep : TutorialStepBase
  {
    public override string Title => "04 Preferred Label Size";

    public override string DescriptionSource => "Step04PreferredSize.description.html";

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
      var node1 = graph.CreateNode();
      graph.AddLabel(node1, "Label size adjusts to long text");

      var node2 = graph.CreateNode(new RectD(100, 250, 30, 30));
      graph.AddLabel(node2, "I'm small");

      var node3 = graph.CreateNode(new RectD(-50, 150, 30, 30));
      graph.AddLabel(node3, "Even line breaks\nwork now!");

      var edge = graph.CreateEdge(node1, node2);
      graph.AddLabel(edge, "Edge labels are adjusted, too");
    }
  }
}
