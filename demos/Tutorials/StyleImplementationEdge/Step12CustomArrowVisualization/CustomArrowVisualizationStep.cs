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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step12CustomArrowVisualization
{
  /// <summary>Tutorial step 12: implementing a custom arrow visualization.</summary>
  public class CustomArrowVisualizationStep : TutorialStepBase
  {
    public override string Title => "12 Custom Arrow Visualization";

    public override string DescriptionSource => "Step12CustomArrowVisualization.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;

      var bridgeManager = new BridgeManager { CanvasControl = graphControl };
      bridgeManager.AddObstacleProvider(new GraphObstacleProvider());

      DemoStyles.InitTutorialStyles(graph);
      CreateSampleGraph(graph);

      CustomEdgeStyleTutorial.EnableGraphEditing(graphControl);
      CustomEdgeStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateSampleGraph(IGraph graph) {
      var node1 = graph.CreateNode(new RectD(0, 0, 30, 30));
      var node2 = graph.CreateNode(new RectD(0, 50, 30, 30));
      var node3 = graph.CreateNode(new RectD(0, 100, 30, 30));
      var node4 = graph.CreateNode(new RectD(100, 0, 30, 30));
      var node5 = graph.CreateNode(new RectD(100, 50, 30, 30));
      var node6 = graph.CreateNode(new RectD(100, 100, 30, 30));

      var style1 = new CustomEdgeStyle(4, new CustomArrow(4));
      graph.CreateEdge(node1, node4, style1);

      var style2 = new CustomEdgeStyle(2, targetArrow : new CustomArrow(2));
      graph.CreateEdge(node2, node5, style2);

      var style3 = new CustomEdgeStyle(6, new CustomArrow(6), new CustomArrow(6));
      graph.CreateEdge(node3, node6, style3);
    }
  }
}
