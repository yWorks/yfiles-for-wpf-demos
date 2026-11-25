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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.Legend;

internal sealed record LegendRenderTag(
    Border LegendVisual,
    SizeD Size,
    PointD Position,
    double Scale,
    bool IsViewCoordinates,
    bool HasCustomPosition);

internal sealed class RenderTreeLegendRenderer : IObjectRenderer, IVisualCreator, IBoundsProvider, IHitTestable,
    IVisibilityTestable
{
  private readonly Func<LegendRenderTag, LegendRenderTag>? updateLegendLayout;
  private LegendRenderTag? renderTag;

  public RenderTreeLegendRenderer(Func<LegendRenderTag, LegendRenderTag>? updateLegendLayout = null) {
    this.updateLegendLayout = updateLegendLayout;
  }

  public IBoundsProvider GetBoundsProvider(object? renderTag) {
    this.renderTag = renderTag as LegendRenderTag;
    return this;
  }

  public IHitTestable GetHitTestable(object? renderTag) => HitTestables.Never;

  public IVisibilityTestable GetVisibilityTestable(object? renderTag) => VisibilityTestables.Always;

  public IVisualCreator GetVisualCreator(object? renderTag) {
    this.renderTag = renderTag as LegendRenderTag;
    return this;
  }

  public Visual CreateVisual(IRenderContext context) {
    var tag = renderTag;
    if (tag is null) {
      return new VisualGroup();
    }
    tag = UpdateViewLayout(tag);
    renderTag = tag;
    UpdateLegendElement(tag);
    var group = new VisualGroup { Transform = tag.IsViewCoordinates ? context.ViewTransform : Transform.Identity };
    group.Add(tag.LegendVisual);
    return group;
  }

  public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
    var tag = renderTag;
    if (tag is null || oldVisual is not VisualGroup group || group.Children.Count == 0) {
      return CreateVisual(context);
    }
    tag = UpdateViewLayout(tag);
    renderTag = tag;
    UpdateLegendElement(tag);
    group.Transform = tag.IsViewCoordinates ? context.ViewTransform : Transform.Identity;
    if (!ReferenceEquals(group.Children[0], tag.LegendVisual)) {
      group.Children[0] = tag.LegendVisual;
    }
    return group;
  }

  public RectD GetBounds(ICanvasContext context) =>
      renderTag is { IsViewCoordinates: false } tag
          ? new RectD(tag.Position.X, tag.Position.Y, tag.Size.Width, tag.Size.Height)
          : RectD.Infinite;

  public bool IsHit(IInputModeContext context, PointD location) => false;

  public bool IsVisible(ICanvasContext context, RectD rectangle) => true;

  private LegendRenderTag UpdateViewLayout(LegendRenderTag tag) =>
      tag.IsViewCoordinates && !tag.HasCustomPosition && updateLegendLayout is not null ? updateLegendLayout(tag) : tag;

  private static void UpdateLegendElement(LegendRenderTag tag) {
    tag.LegendVisual.Width = tag.Size.Width;
    tag.LegendVisual.Height = tag.Size.Height;
    tag.LegendVisual.RenderTransform = new ScaleTransform(tag.Scale, tag.Scale);
    tag.LegendVisual.RenderTransformOrigin = new Point(0, 0);
    tag.LegendVisual.SetCanvasArrangeRect(new Rect(tag.Position.X, tag.Position.Y, tag.Size.Width, tag.Size.Height));
  }
}
