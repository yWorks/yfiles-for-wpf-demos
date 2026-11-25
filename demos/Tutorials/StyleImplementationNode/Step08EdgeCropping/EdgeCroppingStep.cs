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

using System;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step08EdgeCropping
{
  /// <summary>
  ///   Tutorial step 08: Cropping edges at the node outline.
  /// </summary>
  public class EdgeCroppingStep : TutorialStepBase
  {
    public override string Title => "08 Cropping Edges at the Node Outline";

    public override string DescriptionSource => "Step08EdgeCropping.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      CreateSampleGraph(graphControl.Graph);

      CustomNodeStyleTutorial.EnableGraphEditing(graphControl);
      CustomNodeStyleTutorial.FitGraphBounds(graphControl);
    }

    private static void CreateSampleGraph(IGraph graph) {
      var centerNode = graph.CreateNode(
          new RectD(0, 0, 110, 70), new CustomNodeStyle(), new NodeData("Title", "#b91c3b"));

      var outerNodeStyle = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse, Brush = new SolidColorBrush(Color.FromRgb(204, 204, 204)), Pen = null
      };
      var outerNodeSize = new SizeD(5, 5);

      var edgeColor = Color.FromRgb(191, 212, 219);
      var edgeStyle = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(edgeColor), 1.5), TargetArrow = new Arrow(ArrowType.Triangle, edgeColor)
      };
      graph.EdgeDefaults.Style = edgeStyle;

      // Add nodes in two hemispheres around the main node and then create edges.
      const int count = 31;
      const double radius = 200;
      for (var i = 0; i < count; i++) {
        var layout = centerNode.Layout;
        var radians = (double) i / count * 2 * Math.PI;
        var node = graph.CreateNode(
            RectD.FromCenter(
                new PointD(layout.GetCenterX() - Math.Sin(radians) * radius,
                    layout.GetCenterY() - Math.Cos(radians) * radius), outerNodeSize), outerNodeStyle);
        graph.CreateEdge(node, centerNode);
      }
    }
  }
}
