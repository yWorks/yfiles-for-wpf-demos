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
  ///   Level-of-detail style for nodes that delegates to different <see cref="INodeStyle" />s depending on the zoom level.
  /// </summary>
  public class LevelOfDetailNodeStyle : NodeStyleBase<VisualGroup>
  {
    /// <summary>The style container.</summary>
    private readonly LevelOfDetailStyleContainer<INodeStyle> styles = new LevelOfDetailStyleContainer<INodeStyle>();

    /// <summary>
    ///   Gets the style container.
    /// </summary>
    /// <remarks>
    ///   Styles have to be added in ascending order of zoom level. This can be initialized with a collection
    ///   initializer.
    /// </remarks>
    public LevelOfDetailStyleContainer<INodeStyle> Styles {
      get { return styles; }
    }

    /// <inheritdoc />
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var style = Styles.GetStyle(context.Zoom);

      var visual = style.Renderer.GetVisualCreator(node, style).CreateVisual(context);
      var cc = new VisualGroup();
      cc.SetRenderDataCache(context.Zoom);
      cc.Add(visual);
      return cc;
    }

    /// <inheritdoc />
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      var v = oldVisual.Children[0];
      var oldZoom = oldVisual.GetRenderDataCache<double>();
      if (Styles.HasSameStyle(context.Zoom, oldZoom)) {
        var style = Styles.GetStyle(oldZoom);
        var newVisual = style.Renderer.GetVisualCreator(node, style).UpdateVisual(context, v);
        if (!ReferenceEquals(v, oldVisual)) {
          oldVisual.Children[0] = newVisual;
        }
      } else {
        var style = Styles.GetStyle(context.Zoom);
        var visual = style.Renderer.GetVisualCreator(node, style).CreateVisual(context);
        oldVisual.Children[0] = visual;
      }
      oldVisual.SetRenderDataCache(context.Zoom);
      return oldVisual;
    }

    #region Delegating methods

    /// <inheritdoc />
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetVisibilityTestable(node, style).IsVisible(context, rectangle);
    }

    /// <inheritdoc />
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetHitTestable(node, style).IsHit(context, location);
    }

    /// <inheritdoc />
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, INode node) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetMarqueeTestable(node, style).IsInBox(context, rectangle);
    }

    /// <inheritdoc />
    protected override RectD GetBounds(ICanvasContext context, INode node) {
      var style = Styles.GetStyle(context.Zoom);
      return style.Renderer.GetBoundsProvider(node, style).GetBounds(context);
    }

    /// <inheritdoc />
    protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) {
      var mostDetailedStyle = Styles.GetStyle(double.PositiveInfinity);
      return mostDetailedStyle.Renderer.GetShapeGeometry(node, mostDetailedStyle).GetIntersection(inner, outer);
    }

    /// <inheritdoc />
    protected override bool IsInside(INode node, PointD location) {
      var mostDetailedStyle = Styles.GetStyle(double.PositiveInfinity);
      return mostDetailedStyle.Renderer.GetShapeGeometry(node, mostDetailedStyle).IsInside(location);
    }

    /// <inheritdoc />
    protected override GeneralPath GetOutline(INode node) {
      var mostDetailedStyle = Styles.GetStyle(double.PositiveInfinity);
      return mostDetailedStyle.Renderer.GetShapeGeometry(node, mostDetailedStyle).GetOutline();
    }

    #endregion
  }
}
