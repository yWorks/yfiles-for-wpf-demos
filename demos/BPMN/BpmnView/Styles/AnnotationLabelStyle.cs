/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  /// A label style for annotations according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class AnnotationLabelStyle : ILabelStyle {

    #region Initialize static fields

    private static readonly AnnotationLabelStyleRenderer renderer = new AnnotationLabelStyleRenderer();
    private readonly BpmnEdgeStyle connectorStyle = new BpmnEdgeStyle { Type = EdgeType.Association };
    private static readonly DefaultLabelStyle textStyle = new DefaultLabelStyle();
    private readonly AnnotationNodeStyle leftAnnotationStyle = new AnnotationNodeStyle { Left = true };
    private readonly AnnotationNodeStyle rightAnnotationStyle = new AnnotationNodeStyle { Left = false };

    #endregion

    #region Properties

    private double insets = 5.0;

    /// <summary>
    /// Gets or sets the insets around the text.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(5.0)]
    public double Insets {
      get { return insets; }
      set { insets = value; }
    }

    private ConnectedIconLabelStyle delegateStyle;
    internal ConnectedIconLabelStyle DelegateStyle {
      get { return delegateStyle; }
    }

    /// <summary>
    /// Gets or sets the background color of the annotation.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "AnnotationDefaultBackground")]
    public Brush Background {
      get { return leftAnnotationStyle.Background; }
      set {
        if (leftAnnotationStyle.Background != value) {
          leftAnnotationStyle.Background = value;
          rightAnnotationStyle.Background = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the outline color of the annotation.
    /// </summary>
    /// <remarks>
    /// This also influences the color of the line to the annotated element.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "AnnotationDefaultOutline")]
    public Brush Outline {
      get { return leftAnnotationStyle.Outline; }
      set {
        if (leftAnnotationStyle.Outline != value) {
          leftAnnotationStyle.Outline = value;
          rightAnnotationStyle.Outline = value;
          connectorStyle.Color = value;
        }
      }
    }

    #endregion

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public AnnotationLabelStyle() {
      delegateStyle = new ConnectedIconLabelStyle {
        IconStyle = leftAnnotationStyle,
        TextStyle = textStyle,
        TextPlacement = InteriorLabelModel.Center,
        ConnectorStyle = connectorStyle,
        LabelConnectorLocation = FreeNodePortLocationModel.NodeLeftAnchored,
        NodeConnectorLocation = FreeNodePortLocationModel.NodeCenterAnchored
      };
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return new AnnotationLabelStyle {
          Insets = Insets,
          Background = Background,
          Outline = Outline,
      };
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public ILabelStyleRenderer Renderer { get { return renderer; } }

    #region Renderer Class

    /// <summary>
    /// An <see cref="ILabelStyleRenderer"/> implementation used by <see cref="AnnotationLabelStyle"/>.
    /// </summary>
    private sealed class AnnotationLabelStyleRenderer : ILabelStyleRenderer, IVisualCreator {

      private ILabel label;
      private ILabelStyle labelStyle;
      private bool left;
      private double insets;

      private ILabelStyle GetCurrentStyle(ILabel item, ILabelStyle style) {
        var annotationLabelStyle = style as AnnotationLabelStyle;
        var nodeOwner = item.Owner as INode;
        if (annotationLabelStyle == null || nodeOwner == null) {
          return VoidLabelStyle.Instance;
        }

        left = item.GetLayout().GetCenter().X > nodeOwner.Layout.GetCenter().X;
        insets = annotationLabelStyle.Insets;

        var delegateStyle = annotationLabelStyle.DelegateStyle;
        delegateStyle.IconStyle = left ? annotationLabelStyle.leftAnnotationStyle : annotationLabelStyle.rightAnnotationStyle;
        delegateStyle.LabelConnectorLocation = left ? FreeNodePortLocationModel.NodeLeftAnchored : FreeNodePortLocationModel.NodeRightAnchored;
        return delegateStyle;
      }

      /// <inheritdoc/>
      public IVisualCreator GetVisualCreator(ILabel item, ILabelStyle style) {
        label = item;
        labelStyle = style;
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
        var preferredTextSize = textStyle.Renderer.GetPreferredSize(label, textStyle);
        var insets = ((AnnotationLabelStyle) style).Insets;
        return new SizeD(2 * insets + preferredTextSize.Width, 2 * insets + preferredTextSize.Height);
      }

      /// <inheritdoc/>
      public Visual CreateVisual(IRenderContext context) {
        var container = new VisualGroup();
        var delegateStyle = GetCurrentStyle(label, labelStyle);
        container.Add(delegateStyle.Renderer.GetVisualCreator(label, delegateStyle).CreateVisual(context));
        container.SetRenderDataCache(CreateRenderData());

        return container;
      }

      /// <inheritdoc/>
      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        var container = oldVisual as VisualGroup;
        var cache = container != null ? oldVisual.GetRenderDataCache<RenderData>() : null;
        var delegateStyle = GetCurrentStyle(label, labelStyle);
        var newCache = CreateRenderData();
        if (cache == null || !cache.Equals(newCache) || container.Children.Count != 1) {
          return CreateVisual(context);
        }
        var oldDelegateVisual = container.Children[0];
        var newDelegateVisual = delegateStyle.Renderer.GetVisualCreator(label, delegateStyle).UpdateVisual(context, oldDelegateVisual);
        if (oldDelegateVisual != newDelegateVisual) {
          container.Children[0] = newDelegateVisual;
        }
        return container;
      }

      private RenderData CreateRenderData() {
        return new RenderData {
          Left = left,
          Insets = insets
        };
      }

#pragma warning disable 659 // we never yield this class and we are the only ones to call Equals, so we don't need GetHashCode()
      sealed class RenderData
      {
        public bool Left { private get; set; }
        public double Insets { private get; set; }

        public override bool Equals(object obj) {
          var other = obj as RenderData;
          if (other == null) {
            return false;
          }
          return Left == other.Left && Insets == other.Insets;
        }
      }
    }
#pragma warning restore 659

    #endregion

  }
}



