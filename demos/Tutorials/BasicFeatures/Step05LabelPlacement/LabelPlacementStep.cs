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

using System.Linq;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step05LabelPlacement
{
  /// <summary>
  ///   Tutorial step 05: Label Placement.
  /// </summary>
  /// <remarks>
  ///   Shows how to control label placement using label model parameters,
  ///   and how to change placement at runtime.
  /// </remarks>
  public class LabelPlacementStep : TutorialStepBase
  {
    public override string Title => "05 Label Placement";

    public override string DescriptionSource => "Step05LabelPlacement.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      SetDefaultLabelLayoutParameters(graph);
      CreateSampleGraph(graph);
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      AddButtonListener(description, "changeLayoutParametersButton", () => ChangeLabelLayoutParameters(graph));
      AddButtonListener(description, "resetButton", () => ResetLabelLayoutParameters(graph));
    }

    private static void CreateSampleGraph(IGraph graph) {
      var node1 = graph.CreateNode(new RectD(30, 10, 40, 40));
      var node2 = graph.CreateNode(new RectD(120, 10, 60, 40));
      var node3 = graph.CreateNode(new RectD(230, 200, 60, 30));

      graph.CreateEdge(node1, node2);
      var edge2 = graph.CreateEdge(node2, node3);

      graph.AddBend(edge2, new PointD(260, 30));

      var port1AtNode1 = graph.AddPort(node1, FreeNodePortLocationModel.Center);

      var port1AtNode3 = graph.AddPort(node3, new PointD(node3.Layout.X, node3.Layout.GetCenterY()));

      var edgeAtPorts = graph.CreateEdge(port1AtNode1, port1AtNode3);

      // Add labels to several graph elements
      graph.AddLabel(node1, "n1");
      graph.AddLabel(node2, "n2");
      graph.AddLabel(node3, "n3");
      graph.AddLabel(edgeAtPorts, "Edge at Ports");
    }

    /// <summary>Configures the default label placement.</summary>
    private static void SetDefaultLabelLayoutParameters(IGraph graph) {
      // Place node labels in the node center
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;

      // Use a rotated layout for edge labels
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0, 10.0);
    }

    /// <summary>Changes the placement of some labels.</summary>
    private static void ChangeLabelLayoutParameters(IGraph graph) {
      var nodeLabels = graph.GetNodeLabels().ToList();
      var edgeLabels = graph.GetEdgeLabels().ToList();
      if (nodeLabels.Count < 3 || edgeLabels.Count == 0) {
        return;
      }
      var label1 = nodeLabels[1];
      var label2 = nodeLabels[2];
      var edgeLabel = edgeLabels[0];

      // StretchNodeLabelModel stretches the label width or height to match the node size
      var interiorStretchModel = new StretchNodeLabelModel { Padding = new InsetsD(3) };
      graph.SetLabelLayoutParameter(label1, interiorStretchModel.CreateParameter(StretchNodeLabelModel.Position.Top));

      // ExteriorNodeLabelModel places the label in discrete positions outside the node bounds
      var exteriorLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(10) };
      graph.SetLabelLayoutParameter(label2, exteriorLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Bottom));

      // NinePositionsEdgeLabelModel provides a set of 9 predefined locations on an edge
      graph.SetLabelLayoutParameter(edgeLabel, NinePositionsEdgeLabelModel.CenterAbove);
    }

    /// <summary>Resets each label layout to the graph defaults.</summary>
    private static void ResetLabelLayoutParameters(IGraph graph) {
      foreach (var label in graph.GetNodeLabels().ToList()) {
        graph.SetLabelLayoutParameter(label, graph.NodeDefaults.Labels.LayoutParameter);
      }
      foreach (var label in graph.GetEdgeLabels().ToList()) {
        graph.SetLabelLayoutParameter(label, graph.EdgeDefaults.Labels.LayoutParameter);
      }
    }
  }
}
