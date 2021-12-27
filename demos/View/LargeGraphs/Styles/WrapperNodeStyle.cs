/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles
{
  /// <summary>
  ///   Simple <see cref="INodeStyle" /> wrapper to simplify changing every node's style.
  /// </summary>
  /// <remarks>
  ///   This class also implements <see cref="INodeStyleRenderer" /> because renderer instances are tightly integrated with
  ///   their styles and we cannot simply return another style's renderer from here.
  /// </remarks>
  public class WrapperNodeStyle : INodeStyle, INodeStyleRenderer
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="WrapperNodeStyle" /> class, wrapping the given
    ///   <see cref="INodeStyle" />.
    /// </summary>
    /// <param name="style">The style to wrap.</param>
    public WrapperNodeStyle(INodeStyle style) {
      Style = style;
    }

    /// <summary>
    ///   Gets or sets the wrapped style.
    /// </summary>
    public INodeStyle Style { get; set; }

    #region INodeStyle

    /// <inheritdoc />
    public object Clone() {
      return MemberwiseClone();
    }

    /// <inheritdoc />
    public INodeStyleRenderer Renderer {
      get { return this; }
    }

    #endregion

    #region INodeStyleRenderer

    /// <inheritdoc />
    public IVisualCreator GetVisualCreator(INode node, INodeStyle style) {
      return Style.Renderer.GetVisualCreator(node, Style);
    }

    /// <inheritdoc />
    public IBoundsProvider GetBoundsProvider(INode node, INodeStyle style) {
      return Style.Renderer.GetBoundsProvider(node, Style);
    }

    /// <inheritdoc />
    public IVisibilityTestable GetVisibilityTestable(INode node, INodeStyle style) {
      return Style.Renderer.GetVisibilityTestable(node, Style);
    }

    /// <inheritdoc />
    public IHitTestable GetHitTestable(INode node, INodeStyle style) {
      return Style.Renderer.GetHitTestable(node, Style);
    }

    /// <inheritdoc />
    public IMarqueeTestable GetMarqueeTestable(INode node, INodeStyle style) {
      return Style.Renderer.GetMarqueeTestable(node, Style);
    }

    /// <inheritdoc />
    public ILookup GetContext(INode node, INodeStyle style) {
      return Style.Renderer.GetContext(node, Style);
    }

    /// <inheritdoc />
    public IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
      return Style.Renderer.GetShapeGeometry(node, Style);
    }

    #endregion
  }
}