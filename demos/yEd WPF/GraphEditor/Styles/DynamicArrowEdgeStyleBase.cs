/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Styles
{
  public abstract class DynamicArrowEdgeStyleBase : IEdgeStyle, IArrowOwner{
    private Arrow sourceArrow;
    private Arrow targetArrow;
    private Pen pen;
    private readonly IEdgeStyleRenderer renderer;

    protected DynamicArrowEdgeStyleBase() {
      sourceArrow = new Arrow() {Type = ArrowType.None};
      targetArrow = new Arrow() {Type = ArrowType.Default};
      Pen = Pens.Black;
      renderer = new DynamicArrowEdgeStyleRenderer();
    }

    [DisplayName("Source Arrow")]
    [DefaultValue(ArrowType.None)]
    public ArrowType SourceArrowType {
      get { return sourceArrow.Type; }
      set {
        sourceArrow = UpdateArrow(value);
        OnArrowsChanged();
      }
    }

    [DisplayName("Target Arrow")]
    [DefaultValue(ArrowType.Default)]
    public ArrowType TargetArrowType {
      get { return targetArrow.Type; }
      set {
        targetArrow = UpdateArrow(value);
        OnArrowsChanged();
      }
    }

    public IArrow SourceArrow {
      get { return sourceArrow; }
    }

    public IArrow TargetArrow {
      get { return targetArrow; }
    }

    public IEdgeStyleRenderer Renderer {
      get { return renderer; }
    }

    protected abstract IEdgeStyle GetDelegateStyle();

    [NotNull,DisplayName("Line")]
    public Pen Pen {
      get { return pen; }
      set {
        pen = value;
        OnPenChanged();
      }
    }

    protected virtual void OnPenChanged() {
      sourceArrow = UpdateArrow(SourceArrowType);
      targetArrow = UpdateArrow(TargetArrowType);
      OnArrowsChanged();
    }

    protected virtual void OnArrowsChanged() {}

    private Arrow UpdateArrow(ArrowType arrowType) {
      Arrow arrow = new Arrow() {Type = arrowType};
      switch (arrowType) {
        case ArrowType.None:
          break;
        case ArrowType.Default:
        case ArrowType.Short:
        case ArrowType.Diamond:
        case ArrowType.Circle:
        case ArrowType.Triangle:
          arrow.Pen = null;
          arrow.Brush = pen.Brush;
          break;
        case ArrowType.Cross:
        case ArrowType.Simple:
          arrow.Pen = Pen;
          arrow.Brush = null;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      arrow.Scale = 1 + (pen.Thickness - 1) / 4d;
      return arrow;
    }

    public virtual object Clone() {
      var clone = (DynamicArrowEdgeStyleBase) base.MemberwiseClone();
      clone.sourceArrow = (Arrow) ((ICloneable)sourceArrow).Clone();
      clone.targetArrow = (Arrow) ((ICloneable)targetArrow).Clone();
      clone.OnArrowsChanged();
      return clone;
    }

    private class DynamicArrowEdgeStyleRenderer : IEdgeStyleRenderer
    {
      public IVisualCreator GetVisualCreator(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetVisualCreator(edge, delegateStyle);
      }

      public IBoundsProvider GetBoundsProvider(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetBoundsProvider(edge, delegateStyle);
      }

      public IMarqueeTestable GetMarqueeTestable(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetMarqueeTestable(edge, delegateStyle);
      }

      public IVisibilityTestable GetVisibilityTestable(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetVisibilityTestable(edge, delegateStyle);
      }

      public ILookup GetContext(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetContext(edge, delegateStyle);
      }

      public IPathGeometry GetPathGeometry(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetPathGeometry(edge, delegateStyle);
      }

      public IHitTestable GetHitTestable(IEdge edge, IEdgeStyle style) {
        var delegateStyle = ((DynamicArrowEdgeStyleBase)style).GetDelegateStyle();
        return delegateStyle.Renderer.GetHitTestable(edge, delegateStyle);
      }
    }
  }
}