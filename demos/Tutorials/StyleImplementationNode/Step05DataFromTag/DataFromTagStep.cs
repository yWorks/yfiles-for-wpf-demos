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
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step05DataFromTag
{
  /// <summary>
  ///   Tutorial step 05: Data from tag.
  /// </summary>
  public class DataFromTagStep : TutorialStepBase
  {
    public override string Title => "05 Data From Tag";

    public override string DescriptionSource => "Step05DataFromTag.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      CustomNodeStyleTutorial.InitializeTutorialDefaults(graphControl);

      var graph = graphControl.Graph;
      graph.NodeDefaults.Style = new CustomNodeStyle();
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(
          new PointD(0.5, 1), new PointD(0, 10), new PointD(0.5, 0));

      CreateSampleGraph(graph);

      CustomNodeStyleTutorial.EnableGraphEditing(graphControl);
      CustomNodeStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateSampleGraph(IGraph graph) {
      var node1 = graph.CreateNode(new RectD(0, 0, 100, 70), null, new NodeData("#b91c3b"));
      graph.AddLabel(node1, "Red");

      var node2 = graph.CreateNode(new RectD(140, 0, 100, 70), null, new NodeData("#9e7cb5", true));
      graph.AddLabel(node2, "Purple with badge");

      var node3 = graph.CreateNode(new RectD(70, 150, 100, 70));
      graph.AddLabel(node3, "Without tag");
    }
  }
}
