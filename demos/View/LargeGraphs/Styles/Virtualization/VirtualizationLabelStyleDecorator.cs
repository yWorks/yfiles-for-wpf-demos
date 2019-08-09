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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.Virtualization
{
  /// <summary>
  ///   Style decorator that wraps a label style and turns off virtualization below a configurable zoom level.
  /// </summary>
  /// <remarks>
  ///   This implementation delegates everything to the wrapped style except the visibility test, which always returns
  ///   <see langword="true" /> below the configured zoom level.
  /// </remarks>
  public class VirtualizationLabelStyleDecorator : LabelStyleBase<Visual>
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="VirtualizationLabelStyleDecorator" /> class with the given
    ///   virtualization threshold and wrapped style.
    /// </summary>
    /// <param name="threshold">The zoom level below which virtualization is turned off.</param>
    /// <param name="style">The wrapped style.</param>
    public VirtualizationLabelStyleDecorator(double threshold, ILabelStyle style) {
      Style = style;
      Threshold = threshold;
    }

    /// <summary>
    ///   Gets or sets the wrapped style.
    /// </summary>
    public ILabelStyle Style { get; set; }

    /// <summary>
    ///   Gets or sets the zoom level below which virtualization should be turned off.
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    ///   Determines whether the visualization for the specified label is visible in the context.
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
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>
    ///   <see langword="true" /> if either the zoom level is below <see cref="Threshold" /> or the label is visible in the
    ///   clipping rectangle; <see langword="false" /> otherwise.
    /// </returns>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      if (context.Zoom < Threshold) {
        // Returning true here causes virtualization to be turned off. That is, all elements are always in the visual tree.
        return true;
      }
      return Style.Renderer.GetVisibilityTestable(label, Style).IsVisible(context, rectangle);
    }

    #region Delegating methods

    /// <inheritdoc />
    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      return Style.Renderer.GetBoundsProvider(label, Style).GetBounds(context);
    }

    /// <inheritdoc />
    protected override Visual CreateVisual(IRenderContext context, ILabel label) {
      return Style.Renderer.GetVisualCreator(label, Style).CreateVisual(context);
    }

    /// <inheritdoc />
    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, ILabel label) {
      return Style.Renderer.GetVisualCreator(label, Style).UpdateVisual(context, oldVisual);
    }

    /// <inheritdoc />
    protected override SizeD GetPreferredSize(ILabel label) {
      return Style.Renderer.GetPreferredSize(label, Style);
    }

    /// <inheritdoc />
    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      return Style.Renderer.GetHitTestable(label, Style).IsHit(context, location);
    }

    /// <inheritdoc />
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) {
      return Style.Renderer.GetMarqueeTestable(label, Style).IsInBox(context, rectangle);
    }

    #endregion
  }
}