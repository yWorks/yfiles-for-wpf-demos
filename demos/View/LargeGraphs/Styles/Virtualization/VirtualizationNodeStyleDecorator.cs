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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.Virtualization
{
  /// <summary>
  ///   Style decorator that wraps a node style and turns off virtualization below a configurable zoom level.
  /// </summary>
  /// <remarks>
  ///   This implementation delegates everything to the wrapped style except the visibility test, which always returns
  ///   <see langword="true" /> below the configured zoom level.
  /// </remarks>
  public class VirtualizationNodeStyleDecorator : NodeStyleBase<Visual>
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="VirtualizationNodeStyleDecorator" /> class with the given virtualization
    ///   threshold and wrapped style.
    /// </summary>
    /// <param name="threshold">The zoom level below which virtualization is turned off.</param>
    /// <param name="style">The wrapped style.</param>
    public VirtualizationNodeStyleDecorator(double threshold, INodeStyle style) {
      Style = style;
      Threshold = threshold;
    }

    /// <summary>
    ///   Gets or sets the wrapped style.
    /// </summary>
    public INodeStyle Style { get; set; }

    /// <summary>
    ///   Gets or sets the zoom level below which virtualization should be turned off.
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    ///   Determines whether the visualization for the specified node is visible in the context.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This method is called in response to a <see cref="IVisibilityTestable.IsVisible" />
    ///     call to the instance that has been queried from the <see cref="Renderer" />.
    ///   </para>
    ///   <para>
    ///     This implementation returns <see langword="true" /> if the zoom level is below <see cref="Threshold" /> and
    ///     otherwise delegates to the wrapped style.
    ///   </para>
    /// </remarks>
    /// <param name="context">The canvas context.</param>
    /// <param name="rectangle">The clipping rectangle.</param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns>
    ///   <see langword="true" /> if either the zoom level is below <see cref="Threshold" /> or the node is visible in the
    ///   clipping rectangle; <see langword="false" /> otherwise.
    /// </returns>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) {
      if (context.Zoom < Threshold) {
        // Returning true here causes virtualization to be turned off. That is, all elements are always in the visual tree.
        return true;
      }
      return Style.Renderer.GetVisibilityTestable(node, Style).IsVisible(context, rectangle);
    }

    #region Delegating methods

    /// <inheritdoc />
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      return Style.Renderer.GetHitTestable(node, Style).IsHit(context, location);
    }

    /// <inheritdoc />
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, INode node) {
      return Style.Renderer.GetMarqueeTestable(node, Style).IsInBox(context, rectangle);
    }

    /// <inheritdoc />
    protected override bool IsInside(INode node, PointD location) {
      return Style.Renderer.GetShapeGeometry(node, Style).IsInside(location);
    }

    /// <inheritdoc />
    protected override GeneralPath GetOutline(INode node) {
      return Style.Renderer.GetShapeGeometry(node, Style).GetOutline();
    }

    /// <inheritdoc />
    protected override Visual CreateVisual(IRenderContext context, INode node) {
      return Style.Renderer.GetVisualCreator(node, Style).CreateVisual(context);
    }

    /// <inheritdoc />
    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, INode node) {
      return Style.Renderer.GetVisualCreator(node, Style).UpdateVisual(context, oldVisual);
    }

    /// <inheritdoc />
    protected override RectD GetBounds(ICanvasContext context, INode node) {
      return Style.Renderer.GetBoundsProvider(node, Style).GetBounds(context);
    }

    #endregion
  }
}