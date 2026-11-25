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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.NodeLabeling
{
  /// <summary>
  ///   This class is an example for a custom style based on the <see cref="LabelStyleBase{TVisual}"/>.
  ///   For each label, an edge is created that connects the label to its owner node.
  /// </summary>
  internal sealed class CityLabelStyle : LabelStyleBase<VisualGroup>
  {
    private readonly PolylineEdgeStyle connectorEdgeStyle = new() {
        Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)), 1)
    };

    private readonly FreeNodePortLocationModel.Parameter ownerPortLocation = FreeNodePortLocationModel.Center;
    private readonly FreeNodePortLocationModel.Parameter labelPortLocation = FreeNodePortLocationModel.Center;

    /// <summary>
    ///   The style that will be used for the label rendering
    /// </summary>
    public ILabelStyle InnerLabelStyle { get; }

    /// <summary>
    ///   Initializes a new instance of CityLabelStyle.
    /// </summary>
    public CityLabelStyle(ILabelStyle innerLabelStyle) {
      InnerLabelStyle = innerLabelStyle;
    }

    /// <summary>
    ///   Creates the visual for a label to be drawn.
    /// </summary>
    /// <param name="context">The render context</param>
    /// <param name="label">The label to be rendered</param>
    /// <returns>The visual for a label to be drawn.</returns>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      // create a visual group
      var group = new VisualGroup();

      var connectorEdge = CreateConnectorEdge(label);
      var connectorVisual = connectorEdgeStyle.Renderer.GetVisualCreator(connectorEdge, connectorEdgeStyle)
          .CreateVisual(context);
      group.Add(connectorVisual);

      var labelVisual = InnerLabelStyle.Renderer.GetVisualCreator(label, InnerLabelStyle).CreateVisual(context);
      group.Add(labelVisual);

      return group;
    }

    /// <summary>
    ///   Re-renders the label using the old visual for performance reasons.
    /// </summary>
    /// <param name="context">The render context</param>
    /// <param name="oldVisual">The old visual</param>
    /// <param name="label">The label to be rendered</param>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      // check whether the elements are as expected
      if (oldVisual.Children.Count != 2) {
        return CreateVisual(context, label);
      }
      var connectorEdge = CreateConnectorEdge(label);
      var connectorVisual = connectorEdgeStyle.Renderer.GetVisualCreator(connectorEdge, connectorEdgeStyle)
          .UpdateVisual(context, oldVisual.Children[0]);
      oldVisual.Children[0] = connectorVisual;

      var labelVisual = InnerLabelStyle.Renderer.GetVisualCreator(label, InnerLabelStyle)
          .UpdateVisual(context, oldVisual.Children[1]);
      oldVisual.Children[1] = labelVisual;

      return oldVisual;
    }

    /// <summary>
    ///   Creates the edge that connects the label with its owner node.
    /// </summary>
    /// <param name="label">The given label</param>
    /// <returns>The edge that connects the label with its owner node</returns>
    private SimpleEdge CreateConnectorEdge(ILabel label) {
      // create a dummy node at the location of the label
      var labelNodeDummy = new SimpleNode {
          Layout = label.GetLayout().GetBounds(),
          Style = new ShapeNodeStyle()
      };

      var simpleEdge = new SimpleEdge {
          Style = connectorEdgeStyle,
          SourcePort = new SimplePort(labelNodeDummy, labelPortLocation),
          TargetPort = new SimplePort((INode) label.Owner, ownerPortLocation)
      };
      return simpleEdge;
    }

    /// <summary>
    ///   Returns the preferred size of the given label.
    /// </summary>
    /// <param name="label">The given label</param>
    /// <returns>The preferred size of the given label</returns>
    protected override SizeD GetPreferredSize(ILabel label) =>
        InnerLabelStyle.Renderer.GetPreferredSize(label, InnerLabelStyle);

    /// <summary>
    ///   Determines whether the visualization for the specified label is visible in the context.
    /// </summary>
    /// <param name="context">The canvas context</param>
    /// <param name="rectangle">The clipping rectangle</param>
    /// <param name="label">The given label</param>
    /// <returns>True if the visualization is visible, false otherwise</returns>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      var isInnerLabelVisible = InnerLabelStyle.Renderer.GetVisibilityTestable(label, InnerLabelStyle)
          .IsVisible(context, rectangle);
      if (isInnerLabelVisible) {
        return true;
      }

      var connectorEdge = CreateConnectorEdge(label);
      return connectorEdgeStyle.Renderer.GetVisibilityTestable(connectorEdge, connectorEdgeStyle)
          .IsVisible(context, rectangle);
    }
  }
}
