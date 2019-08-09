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
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="IEdgeStyle"/> implementation representing a connection according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class BpmnEdgeStyle : IEdgeStyle, IArrowOwner
  {

    #region Initialize static fields

    private static readonly IconArrow defaultTargetArrow;
    private static readonly IconArrow defaultSourceArrow;
    private static readonly IconArrow associationArrow;
    private static readonly IconArrow conditionalSourceArrow;
    private static readonly IconArrow messageTargetArrow;
    private static readonly IconArrow messageSourceArrow;

    static BpmnEdgeStyle() {
      defaultTargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.DefaultTarget))
      {
        Bounds = new SizeD(8, 6),
        CropLength = 0,
        Length = 8
      };
      defaultSourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.DefaultSource))
      {
        Bounds = new SizeD(8, 6),
        CropLength = 0,
        Length = 0
      };
      associationArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.Association))
      {
        Bounds = new SizeD(8, 6),
        CropLength = 0,
        Length = 0
      };
      conditionalSourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.ConditionalSource))
      {
        Bounds = new SizeD(16, 8),
        CropLength = 0,
        Length = 16
      };
      messageTargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.MessageTarget))
      {
        Bounds = new SizeD(8, 6),
        CropLength = 0,
        Length = 8
      };
      messageSourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.MessageSource))
      {
        Bounds = new SizeD(6, 6),
        CropLength = 0,
        Length = 6
      };
    }

    #endregion

    #region Properties

    private PolylineEdgeStyle delegateStyle;
    private EdgeType type;
    private double smoothingLength = 20;
    private readonly IEdgeStyleRenderer renderer;
    private IArrow sourceArrow;
    private IArrow targetArrow;
    private Pen pen;
    private readonly Pen doubleLineCenterPen;

    /// <summary>
    /// Gets or sets the edge type of this style. 
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EdgeType.SequenceFlow)]
    public EdgeType Type {
      get { return type; }
      set {
        type = value;
        switch (value) {
          case EdgeType.ConditionalFlow:
            pen = BpmnConstants.Pens.BpmnEdgeStyle;
            sourceArrow = conditionalSourceArrow;
            targetArrow = defaultTargetArrow;
            break;
          case EdgeType.Association:
            pen = BpmnConstants.Pens.AssociationEdgeStyle;
            sourceArrow = Arrows.None;
            targetArrow = Arrows.None;
            break;
          case EdgeType.DirectedAssociation:
            pen = BpmnConstants.Pens.AssociationEdgeStyle;
            sourceArrow = Arrows.None;
            targetArrow = associationArrow;
            break;
          case EdgeType.BidirectedAssociation:
            pen = BpmnConstants.Pens.AssociationEdgeStyle;
            sourceArrow = associationArrow;
            targetArrow = associationArrow;
            break;
          case EdgeType.MessageFlow:
            pen = BpmnConstants.Pens.MessageEdgeStyle;
            sourceArrow = messageSourceArrow;
            targetArrow = messageTargetArrow;
            break;
          case EdgeType.DefaultFlow:
            pen = BpmnConstants.Pens.BpmnEdgeStyle;
            sourceArrow = defaultSourceArrow;
            targetArrow = defaultTargetArrow;
            break;
          case EdgeType.Conversation:
            pen = BpmnConstants.Pens.ConversationDoubleLine;
            sourceArrow = Arrows.None;
            targetArrow = Arrows.None;
            break;
          case EdgeType.SequenceFlow:
          default:
            pen = BpmnConstants.Pens.BpmnEdgeStyle;
            sourceArrow = Arrows.None;
            targetArrow = defaultTargetArrow;
            break;
        }
        UpdateDelegate();
      }
    }

    #endregion

    /// <summary>
    /// Creates a new instance using <see cref="EdgeType.SequenceFlow"/>
    /// </summary>
    public BpmnEdgeStyle() {
      renderer = new BpmnEdgeStyleRenderer();
      doubleLineCenterPen = BpmnConstants.Pens.ConversationCenterLine;
      delegateStyle = new PolylineEdgeStyle();
      Type = EdgeType.SequenceFlow;
    }

    // clone constructor
    private BpmnEdgeStyle(BpmnEdgeStyle other) {
      renderer = other.renderer;
      doubleLineCenterPen = other.doubleLineCenterPen;
      delegateStyle = new PolylineEdgeStyle();
      smoothingLength = other.smoothingLength;
      // setting the type updates all read-only properties
      Type = other.Type;
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return new BpmnEdgeStyle(this);
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IEdgeStyleRenderer Renderer {
      get { return renderer; }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IArrow SourceArrow {
      get { return sourceArrow; }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IArrow TargetArrow {
      get { return targetArrow; }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public Pen Pen {
      get { return pen; }
    }

    /// <summary>
    /// Gets or sets the <see cref="Pen"/> for the center line of a <see cref="EdgeType.Conversation"/>.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    internal Pen DoubleLineCenterPen {
      get { return doubleLineCenterPen; }
    }

    /// <summary>
    /// Gets the smoothing length used for creating smooth bends.
    /// </summary>
    /// <remarks>
    /// A value of <c>0.0d</c> will disable smoothing.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(20.0)]
    public double SmoothingLength {
      get { return smoothingLength; }
      set {
        smoothingLength = value;
        UpdateDelegate();
      }
    }

    private void UpdateDelegate() {
      if (delegateStyle != null) {
        delegateStyle.Pen = Pen;
        delegateStyle.SourceArrow = SourceArrow;
        delegateStyle.TargetArrow = TargetArrow;
        delegateStyle.SmoothingLength = SmoothingLength;
      }
    }

    #region Renderer Class

    /// <summary>
    /// Renderer class used for the <see cref="BpmnEdgeStyle"/>.
    /// </summary>
    private class BpmnEdgeStyleRenderer : IEdgeStyleRenderer, IVisualCreator
    {
      private static PolylineEdgeStyleRenderer delegateRenderer = new PolylineEdgeStyleRenderer();
      private BpmnEdgeStyle style;
      private IEdge edge;

      public IBoundsProvider GetBoundsProvider(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        return delegateRenderer.GetBoundsProvider(edge, this.style.delegateStyle);
      }

      public IPathGeometry GetPathGeometry(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        return delegateRenderer.GetPathGeometry(edge, this.style.delegateStyle);
      }

      public IVisualCreator GetVisualCreator(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        this.edge = edge;
        delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle);
        return this;
      }

      public IVisibilityTestable GetVisibilityTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        return delegateRenderer.GetVisibilityTestable(edge, this.style.delegateStyle);
      }

      public IHitTestable GetHitTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        return delegateRenderer.GetHitTestable(edge, this.style.delegateStyle);
      }

      public IMarqueeTestable GetMarqueeTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        return delegateRenderer.GetMarqueeTestable(edge, this.style.delegateStyle);
      }

      public ILookup GetContext(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle)style;
        return delegateRenderer.GetContext(edge, this.style.delegateStyle);
      }

      /// <inheritdoc/>
      public virtual Visual CreateVisual(IRenderContext context) {
        var container = new VisualGroup();
        if (style.Type != EdgeType.Conversation) {
          container.Add(delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).CreateVisual(context));
        } else {
          container.Add(delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).CreateVisual(context));
          style.delegateStyle.Pen = style.DoubleLineCenterPen;
          container.Add(delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).CreateVisual(context));
          style.delegateStyle.Pen = style.Pen;
        }
        container.SetRenderDataCache<EdgeType>(style.Type);
        return container;
      }

      /// <inheritdoc/>
      public virtual Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        var container = oldVisual as VisualGroup;
        if (container == null) {
          return CreateVisual(context);
        }
        var cachedType = oldVisual.GetRenderDataCache<EdgeType>();
        if (cachedType != style.Type && (cachedType == EdgeType.Conversation || style.Type == EdgeType.Conversation)) {
          return CreateVisual(context);
        }
        if (style.Type != EdgeType.Conversation) {
          Visual firstChild = container.Children[0];
          Visual newFirstChild = delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).UpdateVisual(context, firstChild);
          if (firstChild != newFirstChild) {
            container.Children.Remove(firstChild);
            container.Children.Insert(0, newFirstChild);
          }
        } else {
          Visual firstPath = container.Children[0];
          Visual newFirstPath = delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).UpdateVisual(context, firstPath);
          if (firstPath != newFirstPath) {
            container.Children.Remove(firstPath);
            container.Children.Insert(0, newFirstPath);
          }

          style.delegateStyle.Pen = style.DoubleLineCenterPen;
          Visual secondPath = container.Children[1];
          Visual newSecondPath = delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).UpdateVisual(context, secondPath);
          if (secondPath != newSecondPath) {
            container.Children.Remove(secondPath);
            container.Children.Insert(1, newSecondPath);
          }
          style.delegateStyle.Pen = style.Pen;
        }
        return container;
      }
    }

    #endregion

  }
}
