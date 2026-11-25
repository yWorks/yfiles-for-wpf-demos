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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step06DataFromTag
{
  /// <summary>Tutorial step 06: Using data from the edge tag.</summary>
  public class DataFromTagStep : TutorialStepBase
  {
    public override string Title => "06 Data From Tag";

    public override string DescriptionSource => "Step06DataFromTag.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);

      var node1 = graph.CreateNode(new RectD(50, 0, 30, 30));
      var node2 = graph.CreateNode(new RectD(100, 100, 30, 30));
      var node3 = graph.CreateNode(new RectD(0, 200, 30, 30));

      var style1 = new CustomEdgeStyle(1);
      var style2 = new CustomEdgeStyle(3);
      var style3 = new CustomEdgeStyle(5);

      var edge1 = graph.CreateEdge(node1, node2, style1);
      graph.AddBend(edge1, new PointD(115, 60));

      var edge2 = graph.CreateEdge(node2, node3, style2);

      var edge3 = graph.CreateEdge(node3, node1, style3);
      graph.AddBend(edge3, new PointD(15, 120));

      edge1.Tag = new EdgeData("Free");
      edge2.Tag = new EdgeData("Moderate");
      edge3.Tag = new EdgeData("Overloaded");

      CustomEdgeStyleTutorial.EnableGraphEditing(graphControl);
      CustomEdgeStyleTutorial.FitGraphBounds(graphControl);
    }
  }
}
