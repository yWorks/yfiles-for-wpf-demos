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

using System;
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
    private readonly PolylineEdgeStyle delegateStyle;
    private EdgeType type;
    private readonly IEdgeStyleRenderer renderer = new BpmnEdgeStyleRenderer();

    /// <summary>
    /// Gets or sets the edge type of this style. 
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EdgeType.SequenceFlow)]
    public EdgeType Type {
      get { return type; }
      set {
        type = value;
        UpdatePen(Color);
        UpdateArrow(value);
      }
    }

    /// <summary>
    /// Gets or sets the stroke color of the edge.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "EdgeDefaultColor")]
    public Brush Color {
      get { return delegateStyle.Pen.Brush; }
      set {
        if (!Equals(delegateStyle.Pen.Brush, value)) {
          UpdatePen(value);
          UpdateArrow(Type);
        }
      }
    }

    private Pen innerPen;

    /// <summary>
    /// Gets or sets the inner stroke color of the edge when <see cref="Type"/> is <see cref="EdgeType.Conversation"/>.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "EdgeDefaultInnerColor")]
    public Brush InnerColor {
      get { return innerPen.Brush; }
      set {
        if (innerPen == null || !Equals(innerPen.Brush, value)) {
          innerPen = (Pen) new Pen(value, 1) { LineJoin = PenLineJoin.Round }.GetAsFrozen();
        }
      }
    }

    /// <summary>
    /// Creates a new instance using <see cref="EdgeType.SequenceFlow"/>
    /// </summary>
    public BpmnEdgeStyle() {
      delegateStyle = new PolylineEdgeStyle { SmoothingLength = 20 };
      // Setting the type also initializes the pen and arrows correctly
      Type = EdgeType.SequenceFlow;
      Color = BpmnConstants.EdgeDefaultColor;
      InnerColor = BpmnConstants.EdgeDefaultInnerColor;
    }

    // clone constructor
    private BpmnEdgeStyle(BpmnEdgeStyle other) {
      renderer = other.renderer;
      innerPen = other.innerPen;
      // We need to clone the wrapped style since our properties just delegate there
      delegateStyle = (PolylineEdgeStyle) other.delegateStyle.Clone();
      // setting the type updates all read-only properties
      Type = other.Type;
      innerPen = other.innerPen;
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
      get { return delegateStyle.SourceArrow; }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IArrow TargetArrow {
      get { return delegateStyle.TargetArrow; }
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
      get { return delegateStyle.SmoothingLength; }
      set {
        delegateStyle.SmoothingLength = value;
      }
    }

    private void UpdatePen(Brush brush) {
      Pen result;
      switch (Type) {
        case EdgeType.ConditionalFlow:
        case EdgeType.DefaultFlow:
        case EdgeType.SequenceFlow:
        default:
          result = new Pen(brush, 1);
          break;
        case EdgeType.Association:
        case EdgeType.DirectedAssociation:
        case EdgeType.BidirectedAssociation:
          result = new Pen { Brush = brush, DashStyle = DashStyles.Dot, DashCap = PenLineCap.Round };
          break;
        case EdgeType.MessageFlow:
          result = new Pen { Brush = brush, DashStyle = DashStyles.Dash };
          break;
        case EdgeType.Conversation:
          result = new Pen { Brush = brush, Thickness = 3, LineJoin = PenLineJoin.Round };
          break;
      }
      delegateStyle.Pen = (Pen) result.GetAsFrozen();
    }

    private void UpdateArrow(EdgeType type) {
      switch (type) {
        case EdgeType.ConditionalFlow:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.ConditionalSource, Color)) {
              Bounds = new SizeD(16, 8), CropLength = 0, Length = 16
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.DefaultTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
        case EdgeType.Association:
          delegateStyle.SourceArrow = Arrows.None;
          delegateStyle.TargetArrow = Arrows.None;
          break;
        case EdgeType.DirectedAssociation:
          delegateStyle.SourceArrow = Arrows.None;
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.Association, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          break;
        case EdgeType.BidirectedAssociation:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.Association, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.Association, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          break;
        case EdgeType.MessageFlow:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.MessageSource, Color)) {
              Bounds = new SizeD(6, 6), CropLength = 0, Length = 6
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.MessageTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
        case EdgeType.DefaultFlow:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.DefaultSource, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.DefaultTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
        case EdgeType.Conversation:
          delegateStyle.SourceArrow = Arrows.None;
          delegateStyle.TargetArrow = Arrows.None;
          break;
        case EdgeType.SequenceFlow:
        default:
          delegateStyle.SourceArrow = Arrows.None;
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(ArrowType.DefaultTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
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
          var oldPen = style.delegateStyle.Pen;
          style.delegateStyle.Pen = style.innerPen;
          container.Add(delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).CreateVisual(context));
          style.delegateStyle.Pen = oldPen;
        }
        container.SetRenderDataCache(style.Type);
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
          var firstChild = container.Children[0];
          var newFirstChild = delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).UpdateVisual(context, firstChild);
          if (firstChild != newFirstChild) {
            container.Children[0] = firstChild;
          }
        } else {
          var firstPath = container.Children[0];
          var newFirstPath = delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).UpdateVisual(context, firstPath);
          if (firstPath != newFirstPath) {
            container.Children[0] = firstPath;
          }

          var oldPen = style.delegateStyle.Pen;
          style.delegateStyle.Pen = style.innerPen;
          var secondPath = container.Children[1];
          var newSecondPath = delegateRenderer.GetVisualCreator(this.edge, this.style.delegateStyle).UpdateVisual(context, secondPath);
          if (secondPath != newSecondPath) {
            container.Children[1] = secondPath;
          }
          style.delegateStyle.Pen = oldPen;
        }
        return container;
      }
    }

    #endregion
  }
}
