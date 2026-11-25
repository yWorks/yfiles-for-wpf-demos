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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step11GroupNodeStyle
{
  /// <summary>Tutorial step 11: implementing a custom style for group nodes.</summary>
  public class GroupNodeStyleStep : TutorialStepBase
  {
    public override string Title => "11 Group Node Style";

    public override string DescriptionSource => "Step11GroupNodeStyle.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;

      CustomNodeStyleTutorial.InitializeTutorialDefaults(graphControl);
      CustomNodeStyleTutorial.EnableGraphEditing(graphControl);

      // Use the custom group node style for all group nodes.
      graph.GroupNodeDefaults.Style = new CustomGroupNodeStyle();

      CreateSampleGraph(graph);

      CustomNodeStyleTutorial.FitGraphBounds(graphControl);
    }

    /// <summary>Creates a group node with three child nodes.</summary>
    private static void CreateSampleGraph(IGraph graph) {
      var groupNode = graph.CreateGroupNode();
      groupNode.Tag = new NodeData("Group", "#9e7cb5");

      graph.CreateNode(groupNode, new RectD(5, 0, 30, 30));
      graph.CreateNode(groupNode, new RectD(50, 20, 30, 30));
      graph.CreateNode(groupNode, new RectD(20, 50, 30, 30));

      graph.AdjustGroupNodeLayout(groupNode);
    }
  }
}
