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
using yWorks.Graph.PortLocationModels;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step03ManagingViewport
{
  /// <summary>
  ///   Tutorial step 03: Managing the Viewport.
  /// </summary>
  /// <remarks>
  ///   Shows how to work with the viewport of a <see cref="GraphControl"/>:
  ///   fitting the graph, resetting zoom/viewpoint, and animated fitting.
  ///   Three buttons in the description pane trigger the respective viewport operations.
  /// </remarks>
  public class ManagingViewportStep : TutorialStepBase
  {
    public override string Title => "03 Managing the Viewport";

    public override string DescriptionSource => "Step03ManagingViewport.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      DemoStyles.InitTutorialStyles(graphControl.Graph);
      CreateSampleGraph(graphControl.Graph);

      AddButtonListener(description, "fitGraphButton", () => FitGraphBounds(graphControl));
      AddButtonListener(description, "resetButton", () => ResetViewport(graphControl));
      AddButtonListener(description, "fitGraphAnimatedButton", () => FitGraphAnimated(graphControl));
    }

    /// <summary>Updates the content bounds and fits the graph into the viewport.</summary>
    private static void FitGraphBounds(GraphControl graphControl) {
      graphControl.FitGraphBounds();
    }

    /// <summary>Resets the viewport to zoom 1 at the origin.</summary>
    private static void ResetViewport(GraphControl graphControl) {
      graphControl.Zoom = 1;
      graphControl.ViewPoint = PointD.Origin;
    }

    /// <summary>Fits the graph into the viewport using an animated transition.</summary>
    private static void FitGraphAnimated(GraphControl graphControl) {
      _ = graphControl.FitGraphBounds(InsetsD.Empty, true);
    }

    /// <summary>
    ///   Creates a sample graph. One node is placed far outside the initial viewport
    ///   to demonstrate that <see cref="GraphControl.FitGraphBounds()"/> brings it into view.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph) {
      var node1 = graph.CreateNode(new PointD(30, 30));
      var node2 = graph.CreateNode(new PointD(150, 30));
      var node3 = graph.CreateNode(new RectD(230, 200, 60, 30));

      graph.CreateEdge(node1, node2);
      var edgeWithBend = graph.CreateEdge(node2, node3);

      graph.AddBend(edgeWithBend, new PointD(260, 30));

      var port1AtNode1 = graph.AddPort(node1, FreeNodePortLocationModel.Center);

      var port1AtNode3 = graph.AddPort(node3, new PointD(node3.Layout.X, node3.Layout.GetCenterY()));

      var edgeAtPorts = graph.CreateEdge(port1AtNode1, port1AtNode3);

      graph.AddLabel(node1, "1");
      graph.AddLabel(node2, "n2");
      graph.AddLabel(node3, "n3");
      graph.AddLabel(edgeAtPorts, "Edge at Ports");

      // This node is intentionally placed far outside the initial viewport
      // so that the fitGraphBounds demo is meaningful.
      graph.AddLabel(graph.CreateNode(new PointD(-500, -500)), "Outside initial viewport",
          ExteriorNodeLabelModel.Bottom);
    }
  }
}
