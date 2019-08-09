/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Markup;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;

namespace Demo.yFiles.Layout.NodeLabeling
{
  [ContentProperty("InnerLabelStyle")]
  public class CityLabelStyle : LabelStyleBase<VisualGroup>
  {
    public CityLabelStyle() : this(new DefaultLabelStyle()) {}

    public CityLabelStyle([NotNull] ILabelStyle innerLabelStyle) {
      if (innerLabelStyle == null) {
        throw new ArgumentNullException("innerLabelStyle");
      }
      InnerLabelStyle = innerLabelStyle;
      ConnectorEdgeStyle = new PolylineEdgeStyle();
      OwnerPortLocation = LabelPortLocation = FreeNodePortLocationModel.NodeCenterAnchored;
    }

    [NotNull]
    public ILabelStyle InnerLabelStyle { get; set; }

    [NotNull]
    public IEdgeStyle ConnectorEdgeStyle { get; set; }

    [NotNull]
    public IPortLocationModelParameter OwnerPortLocation { get; set; }

    [NotNull]
    public IPortLocationModelParameter LabelPortLocation { get; set; }

    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      VisualGroup container = new VisualGroup();

      var connectorEdge = CreateConnectorEdge(label);
      
      // create the connecting edge visual
      var connectorVisual =
        ConnectorEdgeStyle.Renderer.GetVisualCreator(connectorEdge, ConnectorEdgeStyle).CreateVisual(context);

      // and add it to the container
      container.Add(connectorVisual);

      // as well as the label's own core visualization
      var labelVisual = InnerLabelStyle.Renderer.GetVisualCreator(label, InnerLabelStyle).CreateVisual(context);
      container.Add(labelVisual);
      
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup container, ILabel label) {
      var connectorEdge = CreateConnectorEdge(label);

      // create the connecting edge visual
      var connectorVisual =
        ConnectorEdgeStyle.Renderer.GetVisualCreator(connectorEdge, ConnectorEdgeStyle).UpdateVisual(context, container.Children[0]);

      // and udpate it in the container
      container.Children[0] = connectorVisual;

      // as well as the label's own visualization
      var labelVisual = InnerLabelStyle.Renderer.GetVisualCreator(label, InnerLabelStyle).UpdateVisual(context, container.Children[1]);
      container.Children[1] = labelVisual;

      return container;
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      return InnerLabelStyle.Renderer.GetPreferredSize(label, InnerLabelStyle);
    }

    /// <summary>
    /// Overrides base implementation to ensure visibility of the connecting dummy edge.
    /// </summary>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      if (InnerLabelStyle.Renderer.GetVisibilityTestable(label, InnerLabelStyle).IsVisible(context, rectangle)) {
        return true;
      }

      var connectorEdge = CreateConnectorEdge(label);

      // check the connecting edge visual
      return ConnectorEdgeStyle.Renderer.GetVisibilityTestable(connectorEdge, ConnectorEdgeStyle).IsVisible(context, rectangle);
    }

    /// <summary>
    /// Creates the dummy connector edge for the given label.
    /// </summary>
    private IEdge CreateConnectorEdge(ILabel label) {
      // create a dummy node at the location of the label
      var labelNodeDummy = new SimpleNode { Layout = label.GetLayout().GetBounds(), Style = new ShapeNodeStyle() };
      labelNodeDummy.Ports = new ListEnumerable<IPort>(new[] { new SimplePort(labelNodeDummy, LabelPortLocation) });

      // create a connecting edge between the dummy node and the owner of the label
      return new SimpleEdge(new SimplePort(labelNodeDummy, OwnerPortLocation),
                            new SimplePort((IPortOwner)label.Owner, OwnerPortLocation)) { Style = ConnectorEdgeStyle };
    }
  }
}
