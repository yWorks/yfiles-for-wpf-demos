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

    #region Initialize static fields

    private static readonly ChoreographyMessageLabelStyleRenderer renderer = new ChoreographyMessageLabelStyleRenderer();
    private static readonly BpmnEdgeStyle connectorStyle;
    private static readonly DefaultLabelStyle textStyle;
    private static readonly ILabelModelParameter defaultTextPlacement;
    private static readonly BpmnNodeStyle initiatingMessageStyle;
    private static readonly BpmnNodeStyle responseMessageStyle;
    private readonly ConnectedIconLabelStyle delegateStyle;

    internal static ILabelModelParameter DefaultTextPlacement {
      get { return defaultTextPlacement; }
    }

    internal static BpmnNodeStyle InitiatingMessageStyle {
      get { return initiatingMessageStyle; }
    }

    internal static BpmnNodeStyle ResponseMessageStyle {
      get { return responseMessageStyle; }
    }

    static ChoreographyMessageLabelStyle() {
      defaultTextPlacement = new ExteriorLabelModel {Insets = new InsetsD(5)}.CreateParameter(ExteriorLabelModel.Position.West);
      initiatingMessageStyle = new BpmnNodeStyle
      {
        Icon = IconFactory.CreateMessage(BpmnConstants.Pens.Message, BpmnConstants.Brushes.ChoreographyInitializingParticipant), 
        MinimumSize = BpmnConstants.Sizes.Message
      };
      responseMessageStyle = new BpmnNodeStyle
      {
        Icon = IconFactory.CreateMessage(BpmnConstants.Pens.Message, BpmnConstants.Brushes.ChoreographyReceivingParticipant), 
        MinimumSize = BpmnConstants.Sizes.Message
      };
      connectorStyle = new BpmnEdgeStyle {Type = EdgeType.Association};
      textStyle = new DefaultLabelStyle();
    }

    #endregion

    #region Properties

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
        return DelegateStyle != null ? DelegateStyle.TextPlacement : null;
      }
      set {
        if (DelegateStyle != null) {
          DelegateStyle.TextPlacement = value;
        }
      }
    }

    internal ConnectedIconLabelStyle DelegateStyle {
      get { return delegateStyle; }
    }

    #endregion

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ChoreographyMessageLabelStyle() {

      delegateStyle = new ConnectedIconLabelStyle
      {
        IconSize = BpmnConstants.Sizes.Message,
        IconStyle = InitiatingMessageStyle,
        TextStyle = textStyle,
        ConnectorStyle = connectorStyle,
        LabelConnectorLocation = FreeNodePortLocationModel.NodeBottomAnchored,
        NodeConnectorLocation = FreeNodePortLocationModel.NodeTopAnchored
      };

      TextPlacement = DefaultTextPlacement;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return MemberwiseClone();
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelStyleRenderer Renderer { get { return renderer;  } }

    #region Renderer Class

    /// <summary>
    /// An <see cref="ILabelStyleRenderer"/> implementation used by <see cref="ChoreographyMessageLabelStyle"/>.
    /// </summary>
    internal class ChoreographyMessageLabelStyleRenderer : ILabelStyleRenderer, IVisualCreator
    {
      private ILabel item;
      private ILabelStyle style;
      private bool north;
      private bool responseMessage;

      private ILabelStyle GetCurrentStyle(ILabel item, ILabelStyle style) {
        var labelStyle = style as ChoreographyMessageLabelStyle;

        if (labelStyle == null) {
          return VoidLabelStyle.Instance;
        }

        north = true;
        responseMessage = false;
        var node = item.Owner as INode;
        if (node != null) {
          north = item.GetLayout().GetCenter().Y < node.Layout.GetCenter().Y;

          ChoreographyNodeStyle nodeStyle = node.Style as ChoreographyNodeStyle;
          if (nodeStyle != null) {
            responseMessage = nodeStyle.InitiatingAtTop ^ north;
          }
        }
          
        var delegateStyle = labelStyle.DelegateStyle;
        delegateStyle.IconStyle = responseMessage ? ResponseMessageStyle : InitiatingMessageStyle;
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
        RenderData cache = container != null ? oldVisual.GetRenderDataCache<RenderData>() : null;
        ILabelStyle delegateStyle = GetCurrentStyle(item, style);
        RenderData newCache = CreateRenderData();
        if (cache == null || !cache.Equals(newCache) || container.Children.Count != 1) {
          return CreateVisual(context);
        }
        Visual oldDelegateVisual = container.Children[0];
        Visual newDelegateVisual = delegateStyle.Renderer.GetVisualCreator(item, delegateStyle).UpdateVisual(context, oldDelegateVisual);
        if (oldDelegateVisual != newDelegateVisual) {
          container.Children[0] = newDelegateVisual;
        }
        return container;
      }

      private RenderData CreateRenderData() {
        return new RenderData
        {
          North = north,
          ResponseMessage = responseMessage,
          TextPlacement = ((ChoreographyMessageLabelStyle) style).TextPlacement
        };
      }

#pragma warning disable 659 // we never yield this class and we are the only ones to call Equals, so we don't need GetHashCode()
      class RenderData
      {
        public ILabelModelParameter TextPlacement { private get; set; }

        public bool North { private get; set; }

        public bool ResponseMessage { private get; set; }

        public override bool Equals(object obj) {
          var other = obj as RenderData;
          if (other == null) {
            return false;
          }
          return TextPlacement == other.TextPlacement && North == other.North && ResponseMessage == other.ResponseMessage;
        }
      }
    }
#pragma warning restore 659

    #endregion

  }
}
