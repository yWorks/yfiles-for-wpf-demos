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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step12GroupNodeStyleBehavior
{
  /// <summary>Tutorial step 12: customizing group node behavior.</summary>
  public class GroupNodeStyleBehaviorStep : TutorialStepBase
  {
    public override string Title => "12 Group Node Style Behavior";

    public override string DescriptionSource => "Step12GroupNodeStyleBehavior.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;

      CustomNodeStyleTutorial.InitializeTutorialDefaults(graphControl);
      CustomNodeStyleTutorial.EnableGraphEditing(graphControl);

      graph.GroupNodeDefaults.Style = new CustomGroupNodeStyle();
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(
          new PointD(0.5, 1), new PointD(0, 10), new PointD(0.5, 0));

      CreateSampleGraph(graph);

      CustomNodeStyleTutorial.FitGraphBounds(graphControl);
    }

    /// <summary>Creates a sample graph with several group nodes to demonstrate custom behavior.</summary>
    private static void CreateSampleGraph(IGraph graph) {
      // Group 1 demonstrates standard behavior without the size constraint.
      graph.CreateGroupNode(null, new RectD(-50, -200, 100, 50), new Step11GroupNodeStyle.CustomGroupNodeStyle(),
          new Step11GroupNodeStyle.NodeData("Group 1", "#b91c3b"));

      // Group 2 demonstrates the minimum size constraint.
      graph.CreateGroupNode(null, new RectD(-50, -120, 100, 50), null, new NodeData("Group 2"));

      // Group 3 demonstrates custom bounds that include child labels.
      var groupNode = graph.CreateGroupNode();
      groupNode.Tag = new NodeData("Group 3", "#9e7cb5");

      var child1 = graph.CreateNode(groupNode, new RectD(0, -20, 30, 30));
      graph.AddLabel(child1, "Labels are also included in the group nodes");

      var child2 = graph.CreateNode(groupNode, new RectD(50, 60, 30, 30));
      graph.AddLabel(child2, "as a customization");

      graph.CreateNode(groupNode, new RectD(-50, 50, 30, 30));
      graph.AdjustGroupNodeLayout(groupNode);
    }
  }
}
