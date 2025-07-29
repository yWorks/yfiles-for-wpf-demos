/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Styles
{
  /// <summary>
  ///   An <see cref="IEdgeStyle"/> implementation representing a connection according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public sealed class BpmnEdgeStyle : IEdgeStyle, IArrowOwner
  {
    private readonly PolylineEdgeStyle delegateStyle;
    private EdgeType type;

    /// <summary>
    ///   Gets or sets the edge type of this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EdgeType.SequenceFlow)]
    public EdgeType Type {
      get => type;
      set {
        type = value;
        UpdatePen(Color);
        UpdateArrow(value);
      }
    }

    /// <summary>
    ///   Gets or sets the stroke color of the edge.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "EdgeDefaultColor")]
    public Brush Color {
      get => delegateStyle.Pen.Brush;
      set {
        if (!Equals(delegateStyle.Pen.Brush, value)) {
          UpdatePen(value);
          UpdateArrow(Type);
        }
      }
    }

    private Pen innerPen;

    /// <summary>
    ///   Gets or sets the inner stroke color of the edge when <see cref="Type"/> is <see cref="EdgeType.Conversation"/>.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "EdgeDefaultInnerColor")]
    public Brush InnerColor {
      get => innerPen.Brush;
      set {
        if (innerPen == null || !Equals(innerPen.Brush, value)) {
          innerPen = (Pen) new Pen(value, 1) { LineJoin = PenLineJoin.Round }.GetAsFrozen();
        }
      }
    }

    /// <summary>
    ///   Creates a new instance using <see cref="EdgeType.SequenceFlow"/>
    /// </summary>
    public BpmnEdgeStyle() {
      // activate OrthogonalEditing for orthogonal edge creation and editing
      delegateStyle = new PolylineEdgeStyle { SmoothingLength = 20, OrthogonalEditing = true };
      // Setting the type also initializes the pen and arrows correctly
      Type = EdgeType.SequenceFlow;
      Color = BpmnConstants.EdgeDefaultColor;
      InnerColor = BpmnConstants.EdgeDefaultInnerColor;
    }

    // clone constructor
    private BpmnEdgeStyle(BpmnEdgeStyle other) {
      Renderer = other.Renderer;
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
    public IEdgeStyleRenderer Renderer { get; } = new BpmnEdgeStyleRenderer();

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IArrow SourceArrow => delegateStyle.SourceArrow;

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IArrow TargetArrow => delegateStyle.TargetArrow;

    /// <summary>
    ///   Gets the smoothing length used for creating smooth bends.
    /// </summary>
    /// <remarks>
    ///   A value of <c>0.0d</c> will disable smoothing.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(20.0)]
    public double SmoothingLength {
      get => delegateStyle.SmoothingLength;
      set => delegateStyle.SmoothingLength = value;
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
          delegateStyle.SourceArrow =
              new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.ConditionalSource, Color)) {
                  Bounds = new SizeD(16, 8), CropLength = 0, Length = 16
              };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.DefaultTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
        case EdgeType.Association:
          delegateStyle.SourceArrow = Arrow.None;
          delegateStyle.TargetArrow = Arrow.None;
          break;
        case EdgeType.DirectedAssociation:
          delegateStyle.SourceArrow = Arrow.None;
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.Association, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          break;
        case EdgeType.BidirectedAssociation:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.Association, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.Association, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          break;
        case EdgeType.MessageFlow:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.MessageSource, Color)) {
              Bounds = new SizeD(6, 6), CropLength = 0, Length = 6
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.MessageTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
        case EdgeType.DefaultFlow:
          delegateStyle.SourceArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.DefaultSource, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 0
          };
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.DefaultTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
        case EdgeType.Conversation:
          delegateStyle.SourceArrow = Arrow.None;
          delegateStyle.TargetArrow = Arrow.None;
          break;
        case EdgeType.SequenceFlow:
        default:
          delegateStyle.SourceArrow = Arrow.None;
          delegateStyle.TargetArrow = new IconArrow(IconFactory.CreateArrowIcon(BpmnArrowType.DefaultTarget, Color)) {
              Bounds = new SizeD(8, 6), CropLength = 0, Length = 8
          };
          break;
      }
    }

    #region Renderer Class

    /// <summary>
    ///   Renderer class used for the <see cref="BpmnEdgeStyle"/>.
    /// </summary>
    private sealed class BpmnEdgeStyleRenderer : IEdgeStyleRenderer, IVisualCreator
    {
      private BpmnEdgeStyle style;
      private IEdge edge;

      private PolylineEdgeStyle DelegateStyle => style.delegateStyle;

      public IBoundsProvider GetBoundsProvider(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetBoundsProvider(edge, DelegateStyle);
      }

      public IPathGeometry GetPathGeometry(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetPathGeometry(edge, DelegateStyle);
      }

      public IVisualCreator GetVisualCreator(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        this.edge = edge;
        DelegateStyle.Renderer.GetVisualCreator(this.edge, DelegateStyle);
        return this;
      }

      public IVisibilityTestable GetVisibilityTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetVisibilityTestable(edge, DelegateStyle);
      }

      public IHitTestable GetHitTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetHitTestable(edge, DelegateStyle);
      }

      public IMarqueeTestable GetMarqueeTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetMarqueeTestable(edge, DelegateStyle);
      }

      public ILassoTestable GetLassoTestable(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetLassoTestable(edge, DelegateStyle);
      }

      public ILookup GetContext(IEdge edge, IEdgeStyle style) {
        this.style = (BpmnEdgeStyle) style;
        return DelegateStyle.Renderer.GetContext(edge, DelegateStyle);
      }

      /// <inheritdoc/>
      public Visual CreateVisual(IRenderContext context) {
        var container = new VisualGroup();
        var delegateStyle = DelegateStyle;
        if (style.Type != EdgeType.Conversation) {
          container.Add(delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle).CreateVisual(context));
        } else {
          container.Add(delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle).CreateVisual(context));
          var oldPen = delegateStyle.Pen;
          delegateStyle.Pen = style.innerPen;
          container.Add(delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle).CreateVisual(context));
          delegateStyle.Pen = oldPen;
        }
        container.SetRenderDataCache(style.Type);
        return container;
      }

      /// <inheritdoc/>
      public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
        var container = oldVisual as VisualGroup;
        if (container == null) {
          return CreateVisual(context);
        }
        var cachedType = oldVisual.GetRenderDataCache<EdgeType>();
        if (cachedType != style.Type && (cachedType == EdgeType.Conversation || style.Type == EdgeType.Conversation)) {
          return CreateVisual(context);
        }
        var delegateStyle = DelegateStyle;
        if (style.Type != EdgeType.Conversation) {
          var firstChild = container.Children[0];
          var newFirstChild = delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle)
              .UpdateVisual(context, firstChild);
          if (firstChild != newFirstChild) {
            container.Children[0] = firstChild;
          }
        } else {
          var firstPath = container.Children[0];
          var newFirstPath = delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle)
              .UpdateVisual(context, firstPath);
          if (firstPath != newFirstPath) {
            container.Children[0] = firstPath;
          }

          var oldPen = delegateStyle.Pen;
          delegateStyle.Pen = style.innerPen;
          var secondPath = container.Children[1];
          var newSecondPath = delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle)
              .UpdateVisual(context, secondPath);
          if (secondPath != newSecondPath) {
            container.Children[1] = secondPath;
          }
          delegateStyle.Pen = oldPen;
        }
        return container;
      }
    }

    #endregion
  }
}
