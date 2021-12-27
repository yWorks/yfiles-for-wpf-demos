/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Layout.PortCandidateDemo
{
  /// <summary>
  /// A very simple implementation of an <see cref="INodeStyle"/>
  /// that delegates the rendering of the style to another node style and adds renderings
  /// for <see cref="PortDescriptor"/> instances.
  /// </summary>
  public class FlowChartNodeStyle : NodeStyleBase<VisualGroup>
  {
    public FlowChartNodeStyle() {
      NodeStyle = new ShapeNodeStyle()
                         {
                           Brush = new LinearGradientBrush
                                     {
                                       StartPoint = new PointD(0, 0),
                                       EndPoint = new PointD(1, 1),
                                       GradientStops = new GradientStopCollection
                                                         {
                                                           new GradientStop(Color.FromRgb(255, 221, 136), 0.3),
                                                           new GradientStop(Color.FromRgb(255, 153, 0), 0.6)
                                                         }
                                     },
                           Pen = new Pen(new SolidColorBrush(Color.FromRgb(255, 153, 0)), 1),

                         };
      PortStyle = new CirclePortStyle();
    }

    private ShapeNodeStyle NodeStyle { get; set; }
    private IPortStyle PortStyle { get; set; }

    protected override GeneralPath GetOutline(INode node) {
      // delegate outline calculation to the NodeStyle
      return NodeStyle.Renderer.GetShapeGeometry(node, NodeStyle).GetOutline();
    }

    private FlowChartType flowChartType;
    public FlowChartType FlowChartType {
      get { return flowChartType; }
      set {
        switch (value) {
          default:
          case FlowChartType.Start:
          case FlowChartType.End:
            NodeStyle.Shape = ShapeNodeShape.Ellipse;
            break;
          case FlowChartType.Operation:
            NodeStyle.Shape = ShapeNodeShape.Rectangle;
            break;
          case FlowChartType.Branch:
            NodeStyle.Shape = ShapeNodeShape.Diamond;
            break;
        }
        flowChartType = value;
      }
    }

    #region Rendering

    /// <summary>
    /// Creates the visual for a node.
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      // This implementation creates a VisualGroup and uses it for the rendering of the node.
      var visual = new VisualGroup();
      // Render the node
      Render(context, node, visual);
      return visual;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      // update the node visualization
      oldVisual.Children[0] = NodeStyle.Renderer.GetVisualCreator(node, NodeStyle).UpdateVisual(context, oldVisual.Children[0]);

      // update the "visual" ports
      int count = 1;
      foreach (var portDescriptor in PortDescriptor.CreatePortDescriptors(FlowChartType)) {
        if (portDescriptor.X != int.MaxValue) {
          var parameter = FreeNodePortLocationModel.Instance.CreateParameter(PointD.Origin, new PointD(portDescriptor.X, portDescriptor.Y));
          var port = new SimplePort(node, parameter) { LookupImplementation = Lookups.Empty };
          // see if we have a visual to update...
          if (count < oldVisual.Children.Count) {
            var oldPortVisual = oldVisual.Children[count];
            oldVisual.Children[count] = PortStyle.Renderer.GetVisualCreator(port, PortStyle).UpdateVisual(context, oldPortVisual);
          } else {
            // no - add a new one
            var portVisual = PortStyle.Renderer.GetVisualCreator(port, PortStyle).CreateVisual(context);
            oldVisual.Children.Add(portVisual);
          }
        }
        count++;
      }
      // see if the number of ports decreased
      while (count < oldVisual.Children.Count) {
        // yes, remove the old visual
        var index = oldVisual.Children.Count - 1;
        oldVisual.Children.RemoveAt(index);
      }
      return oldVisual;
    }

    /// <summary>
    /// Actually creates the visual appearance of a node.
    /// </summary>
    /// <remarks>
    /// This renders the node and the edges to the labels and adds the visuals to the <paramref name="container"/>.
    /// All items are arranged as if the node was located at (0,0). <see cref="CreateVisual"/> and <see cref="UpdateVisual"/>
    /// finally arrange the container so that the drawing is translated into the final position.
    /// </remarks>
    private void Render(IRenderContext context, INode node, VisualGroup container) {
      var innerVisual = NodeStyle.Renderer.GetVisualCreator(node, NodeStyle).CreateVisual(context);
      container.Add(innerVisual);

      // draw "visual" ports
      foreach (var portDescriptor in PortDescriptor.CreatePortDescriptors(FlowChartType)) {
        if (portDescriptor.X != int.MaxValue) {
          var parameter = FreeNodePortLocationModel.Instance.CreateParameter(PointD.Origin, new PointD(portDescriptor.X, portDescriptor.Y));
          var port = new SimplePort(node, parameter) { LookupImplementation = Lookups.Empty };
          var portVisual = PortStyle.Renderer.GetVisualCreator(port, PortStyle).CreateVisual(context);
          container.Children.Add(portVisual);
        }
      }
    }

    #endregion

    /// <summary>
    /// Returns the size of the node based on its tag.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static SizeD GetNodeTypeSize(INode node) {
      return GetNodeTypeSize(((FlowChartNodeStyle) node.Style).FlowChartType);
    }

    /// <summary>
    /// Returns the size a node with this tag would need.
    /// </summary>
    public static SizeD GetNodeTypeSize(FlowChartType type) {
      switch (type) {
        case FlowChartType.Start:
          return new SizeD(30, 30);
        case FlowChartType.Operation:
          return new SizeD(60, 30);
        case FlowChartType.End:
          return new SizeD(30, 30);
        case FlowChartType.Branch:
          return new SizeD(60, 30);
        default: // can't happen
          return new SizeD(30, 30);
      }
    }
  }
}
