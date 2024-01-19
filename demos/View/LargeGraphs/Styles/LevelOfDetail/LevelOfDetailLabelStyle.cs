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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.LevelOfDetail
{
  /// <summary>
  ///   Level-of-detail style for labels that delegates to different <see cref="ILabelStyle" />s depending on the zoom level.
  /// </summary>
  public class LevelOfDetailLabelStyle : LabelStyleBase<VisualGroup>
  {
    /// <summary>The style container.</summary>
    private readonly LevelOfDetailStyleContainer<ILabelStyle> styles = new LevelOfDetailStyleContainer<ILabelStyle>();

    /// <summary>
    ///   Gets the style container.
    /// </summary>
    /// <remarks>
    ///   Styles have to be added in ascending order of zoom level. This can be initialized with a collection
    ///   initializer.
    /// </remarks>
    public LevelOfDetailStyleContainer<ILabelStyle> Styles {
      get { return styles; }
    }

    /// <inheritdoc />
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      var style = Styles.GetStyle(context.Zoom);

      var visual = style.Renderer.GetVisualCreator(label, style).CreateVisual(context);
      var cc = new VisualGroup();
      cc.SetRenderDataCache(context.Zoom);
      cc.Add(visual);
      return cc;
    }

    /// <inheritdoc />
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      var v = oldVisual.Children[0];
      var oldZoom = oldVisual.GetRenderDataCache<double>();
      if (Styles.HasSameStyle(context.Zoom, oldZoom)) {
        var style = Styles.GetStyle(oldZoom);
        var newVisual = style.Renderer.GetVisualCreator(label, style).UpdateVisual(context, v);
        if (!ReferenceEquals(v, newVisual)) {
          oldVisual.Children[0] = newVisual;
        }
      } else {
        var style = Styles.GetStyle(context.Zoom);
        var visual = style.Renderer.GetVisualCreator(label, style).CreateVisual(context);
        oldVisual.Children[0] = visual;
      }
      oldVisual.SetRenderDataCache(context.Zoom);
      return oldVisual;
    }

    /// <inheritdoc />
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetVisibilityTestable(label, style).IsVisible(context, rectangle);
    }

    /// <inheritdoc />
    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetBoundsProvider(label, style).GetBounds(context);
    }

    /// <summary>
    ///   Returns the preferred <see cref="SizeD">size</see> of the label.
    /// </summary>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The preferred size.</returns>
    /// <remarks>
    ///   This level-of-detail style assumes that the most-detailed style has the most correct preferred size. In most
    ///   cases that would simply be a <see cref="DefaultLabelStyle" /> instance.
    /// </remarks>
    protected override SizeD GetPreferredSize(ILabel label) {
      var mostDetailedStyle = Styles.GetStyle(double.PositiveInfinity);
      return mostDetailedStyle.Renderer.GetPreferredSize(label, mostDetailedStyle);
    }

    /// <inheritdoc />
    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetHitTestable(label, style).IsHit(context, location);
    }

    /// <inheritdoc />
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetMarqueeTestable(label, style).IsInBox(context, rectangle);
    }
  }
}
