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
using yWorks.Graph.PortLocationModels;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step02GraphElementCreation
{
  /// <summary>
  ///   Tutorial step 02: Creating Graph Elements.
  /// </summary>
  /// <remarks>
  ///   Shows how to create nodes, edges, bends, ports, and labels using the low-level
  ///   <see cref="IGraph"/> API.
  /// </remarks>
  public class GraphElementCreationStep : TutorialStepBase
  {
    public override string Title => "02 Creating Graph Elements";

    public override string DescriptionSource => "Step02GraphElementCreation.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      DemoStyles.InitTutorialStyles(graphControl.Graph);
      GraphElementCreation(graphControl.Graph);
      // FitGraphBounds is intentionally omitted here: the next step (03 Managing the Viewport)
      // introduces viewport management and explains how to fit the graph bounds.
    }

    private static void GraphElementCreation(IGraph graph) {
      // Create two nodes with the default node size.
      // The location specifies the node center.
      var node1 = graph.CreateNode();
      var node2 = graph.CreateNode(new PointD(150, 15));

      // Create a third node with a different size of 60x30.
      // In this case, the X/Y values define the upper left corner of the node bounds.
      var node3 = graph.CreateNode(new RectD(230, 200, 60, 30));

      // Create edges between the nodes
      graph.CreateEdge(node1, node2);
      var edgeWithBend = graph.CreateEdge(node2, node3);

      // Create a bend
      graph.AddBend(edgeWithBend, new PointD(260, 15));

      // Actually, edges connect "ports", not nodes directly.
      // If necessary, you can manually create ports at nodes
      // and let the edges connect to these.
      // Create a port in the center of the node layout
      var port1AtNode1 = graph.AddPort(node1, FreeNodePortLocationModel.Center);

      // Create a port at the middle of the left border.
      // The location is interpreted as absolute coordinates.
      var layout = node3.Layout;
      var port1AtNode3 = graph.AddPort(node3, new PointD(layout.X, layout.GetCenterY()));

      // Create an edge that connects the two ports
      var edgeAtPorts = graph.CreateEdge(port1AtNode1, port1AtNode3);

      // Add labels to several graph elements
      graph.AddLabel(node1, "n1");
      graph.AddLabel(node2, "n2");
      graph.AddLabel(node3, "n3");
      graph.AddLabel(edgeAtPorts, "Edge at Ports");
    }
  }
}
