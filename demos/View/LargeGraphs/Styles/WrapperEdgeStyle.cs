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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles
{
  /// <summary>
  ///   Simple <see cref="IEdgeStyle" /> wrapper to simplify changing every edge's style.
  /// </summary>
  /// <remarks>
  ///   This class also implements <see cref="IEdgeStyleRenderer" /> because renderer instances are tightly integrated with
  ///   their styles and we cannot simply return another style's renderer from here.
  /// </remarks>
  public class WrapperEdgeStyle : IEdgeStyle, IEdgeStyleRenderer
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="WrapperEdgeStyle" /> class, wrapping the given
    ///   <see cref="IEdgeStyle" />.
    /// </summary>
    /// <param name="style">The style to wrap.</param>
    public WrapperEdgeStyle(IEdgeStyle style) {
      Style = style;
    }

    /// <summary>
    ///   Gets or sets the wrapped style.
    /// </summary>
    public IEdgeStyle Style { get; set; }

    #region IEdgeStyle

    /// <inheritdoc />
    public object Clone() {
      return MemberwiseClone();
    }

    /// <inheritdoc />
    public IEdgeStyleRenderer Renderer {
      get { return this; }
    }

    #endregion

    #region IEdgeStyleRenderer

    /// <inheritdoc />
    public IVisualCreator GetVisualCreator(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetVisualCreator(edge, Style);
    }

    /// <inheritdoc />
    public IBoundsProvider GetBoundsProvider(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetBoundsProvider(edge, Style);
    }

    /// <inheritdoc />
    public IVisibilityTestable GetVisibilityTestable(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetVisibilityTestable(edge, Style);
    }

    /// <inheritdoc />
    public IHitTestable GetHitTestable(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetHitTestable(edge, Style);
    }

    /// <inheritdoc />
    public IMarqueeTestable GetMarqueeTestable(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetMarqueeTestable(edge, Style);
    }

    /// <inheritdoc />
    public ILookup GetContext(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetContext(edge, Style);
    }

    /// <inheritdoc />
    public IPathGeometry GetPathGeometry(IEdge edge, IEdgeStyle style) {
      return Style.Renderer.GetPathGeometry(edge, Style);
    }

    #endregion
  }
}