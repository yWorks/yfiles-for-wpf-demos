/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// A label style for message labels of nodes using a <see cref="ChoreographyNodeStyle"/>.
  /// </summary>
  /// <remarks>
  /// To place labels with this style, <see cref="ChoreographyLabelModel.NorthMessage"/> 
  /// or <see cref="ChoreographyLabelModel.SouthMessage"/> are recommended.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class ChoreographyMessageLabelStyle : ILabelStyle {
    private static readonly ChoreographyMessageLabelStyleRenderer renderer = new ChoreographyMessageLabelStyleRenderer();
    private static readonly BpmnEdgeStyle connectorStyle = new BpmnEdgeStyle { Type = EdgeType.Association };
    private static readonly DefaultLabelStyle textStyle = new DefaultLabelStyle();
    internal static readonly ILabelModelParameter defaultTextPlacement = new ExteriorLabelModel {Insets = new InsetsD(5)}.CreateParameter(ExteriorLabelModel.Position.West);
    private readonly BpmnNodeStyle messageStyle = new BpmnNodeStyle { MinimumSize = BpmnConstants.MessageSize };
    private readonly ConnectedIconLabelStyle delegateStyle;

    /// <summary>
    /// Gets or sets where the text is placed relative to the message icon.
    /// </summary>
    /// <remarks>
    /// The label model parameter has to support <see cref="INode"/>s.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnDefaultValueConverterHolder), "Demo.yFiles.Graph.Bpmn.Styles.ChoreographyMessageLabelStyle.DefaultTextPlacement")]
    public ILabelModelParameter TextPlacement {
      get {
        return delegateStyle != null ? delegateStyle.TextPlacement : null;
      }
      set {
        if (delegateStyle != null) {
          delegateStyle.TextPlacement = value;
        }
      }
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ChoreographyMessageLabelStyle() {

      delegateStyle = new ConnectedIconLabelStyle
      {
        IconSize = BpmnConstants.MessageSize,
        IconStyle = messageStyle,
        TextStyle = textStyle,
        ConnectorStyle = connectorStyle,
        LabelConnectorLocation = FreeNodePortLocationModel.NodeBottomAnchored,
        NodeConnectorLocation = FreeNodePortLocationModel.NodeTopAnchored
      };

      TextPlacement = defaultTextPlacement;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return MemberwiseClone();
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelStyleRenderer Renderer { get { return renderer;  } }

    /// <summary>
    /// An <see cref="ILabelStyleRenderer"/> implementation used by <see cref="ChoreographyMessageLabelStyle"/>.
    /// </summary>
    internal class ChoreographyMessageLabelStyleRenderer : ILabelStyleRenderer, IVisualCreator
    {
      private ILabel item;
      private ILabelStyle style;
      private bool north;
      private Brush messageColor;
      private Pen messageOutline;

      private ILabelStyle GetCurrentStyle(ILabel item, ILabelStyle style) {
        var labelStyle = style as ChoreographyMessageLabelStyle;

        if (labelStyle == null) {
          return VoidLabelStyle.Instance;
        }

        north = true;
        messageColor = BpmnConstants.DefaultInitiatingMessageColor;
        messageOutline = null;
        var node = item.Owner as INode;
        if (node != null) {
          north = item.GetLayout().GetCenter().Y < node.Layout.GetCenter().Y;

          var nodeStyle = node.Style as ChoreographyNodeStyle;
          if (nodeStyle != null) {
            var responseMessage = nodeStyle.InitiatingAtTop ^ north;
            messageColor = responseMessage ? nodeStyle.ResponseColor : nodeStyle.InitiatingColor;
            messageOutline = nodeStyle.messagePen;
          }
        }
        messageOutline = messageOutline ?? (Pen) new Pen(BpmnConstants.DefaultMessageOutline, 1).GetAsFrozen();

        var delegateStyle = labelStyle.delegateStyle;
        delegateStyle.IconStyle = labelStyle.messageStyle;
        labelStyle.messageStyle.Icon = IconFactory.CreateMessage(messageOutline, messageColor);
        delegateStyle.LabelConnectorLocation = north ? FreeNodePortLocationModel.NodeBottomAnchored : FreeNodePortLocationModel.NodeTopAnchored;
        delegateStyle.NodeConnectorLocation = north ? FreeNodePortLocationModel.NodeTopAnchored : FreeNodePortLocationModel.NodeBottomAnchored;
        return delegateStyle;
      }

      /// <inheritdoc/>
      public IVisualCreator GetVisualCreator(ILabel item, ILabelStyle style) {
        this.item = item;
        this.style = style;
        return this;
      }

      /// <inheritdoc/>
      public IBoundsProvider GetBoundsProvider(ILabel item, ILabelStyle style) {
        var delegateStyle = GetCurrentStyle(item, style);
        return delegateStyle.Renderer.GetBoundsProvider(item, delegateStyle);
      }

      /// <inheritdoc/>
      public IVisibilityTestable GetVisibilityTestable(ILabel item, ILabelStyle style) {
        var delegateStyle = GetCurrentStyle(item, style);
        return delegateStyle.Renderer.GetVisibilityTestable(item, delegateStyle);
      }

      /// <inheritdoc/>
      public IHitTestable GetHitTestable(ILabel item, ILabelStyle style) {
        var delegateStyle = GetCurrentStyle(item, style);
        return delegateStyle.Renderer.GetHitTestable(item, delegateStyle);
      }

      /// <inheritdoc/>
      public IMarqueeTestable GetMarqueeTestable(ILabel item, ILabelStyle style) {
        var delegateStyle = GetCurrentStyle(item, style);
        return delegateStyle.Renderer.GetMarqueeTestable(item, delegateStyle);
      }

      /// <inheritdoc/>
      public ILookup GetContext(ILabel item, ILabelStyle style) {
        var delegateStyle = GetCurrentStyle(item, style);
        return delegateStyle.Renderer.GetContext(item, delegateStyle);
      }

      /// <inheritdoc/>
      public SizeD GetPreferredSize(ILabel label, ILabelStyle style) {
        var delegateStyle = GetCurrentStyle(label, style);
        return delegateStyle.Renderer.GetPreferredSize(label, delegateStyle);
      }

      /// <inheritdoc/>
      public Visual CreateVisual(IRenderContext context) {
        var container = new VisualGroup();
        var delegateStyle = GetCurrentStyle(item, style);
        container.Add(delegateStyle.Renderer.GetVisualCreator(item, delegateStyle).CreateVisual(context));
        container.SetRenderDataCache(CreateRenderData());

        return container;
      }

      /// <inheritdoc/>
      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        var container = oldVisual as VisualGroup;
        var cache = container != null ? oldVisual.GetRenderDataCache<RenderData>() : null;
        var newCache = CreateRenderData();
        if (cache == null || !cache.Equals(newCache) || container.Children.Count != 1) {
          return CreateVisual(context);
        }
        var delegateStyle = GetCurrentStyle(item, style);
        var oldDelegateVisual = container.Children[0];
        var newDelegateVisual = delegateStyle.Renderer.GetVisualCreator(item, delegateStyle).UpdateVisual(context, oldDelegateVisual);
        if (oldDelegateVisual != newDelegateVisual) {
          container.Children[0] = newDelegateVisual;
        }
        return container;
      }

      private RenderData CreateRenderData() {
        return new RenderData
        {
          North = north,
          MessageColor = messageColor,
          MessageOutline = messageOutline,
          TextPlacement = ((ChoreographyMessageLabelStyle) style).TextPlacement,
        };
      }

#pragma warning disable 659 // we never yield this class and we are the only ones to call Equals, so we don't need GetHashCode()
      sealed class RenderData
      {
        public ILabelModelParameter TextPlacement { private get; set; }

        public bool North { private get; set; }

        public Brush MessageColor { private get; set; }

        public Pen MessageOutline { private get; set; }

        public override bool Equals(object obj) {
          var other = obj as RenderData;
          if (other == null) {
            return false;
          }
          return TextPlacement == other.TextPlacement &&
                 North == other.North && 
                 Equals(MessageColor, other.MessageColor) &&
                 Equals(MessageOutline, other.MessageOutline);
        }
      }
    }
#pragma warning restore 659
  }
}
