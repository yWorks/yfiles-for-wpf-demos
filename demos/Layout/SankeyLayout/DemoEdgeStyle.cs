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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.SankeyLayout
{
  /// <summary>
  ///   Renders a path between Sankey nodes.
  /// </summary>
  /// <remarks>
  ///   Incorporates the thickness which has been saved into the node tag.
  /// </remarks>
  public sealed class DemoEdgeStyle : IEdgeStyle, IEdgeStyleRenderer
  {
    private readonly BezierEdgeStyle style = new();
    private readonly IGraphSelection selection;
    private readonly bool highlight;

    public DemoEdgeStyle(IGraphSelection selection, bool highlight) {
      this.selection = selection;
      this.highlight = highlight;
    }

    private void Configure(IEdge edge) {
      var tag = (CustomTag) edge.Tag;
      if (tag == null) {
        style.Pen = Pens.Black;
        return;
      }
      var thickness = tag.thickness;
      var color = tag.brush.Color;
      var selected = selection.Contains(edge);
      color.A = (byte) (selected ? 127 : 204);

      style.Pen = new Pen(tag.brush, highlight ? Math.Max(thickness + 2, 2) : thickness);
    }

    #region IEdgeStyle

    /// <inheritdoc/>
    public object Clone() => MemberwiseClone();

    /// <inheritdoc/>
    public IEdgeStyleRenderer Renderer => this;

    #endregion

    #region IEdgeStyleRenderer

    /// <inheritdoc/>
    public IVisualCreator GetVisualCreator(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetVisualCreator(edge, this.style);
    }

    /// <inheritdoc/>
    public IBoundsProvider GetBoundsProvider(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetBoundsProvider(edge, this.style);
    }

    /// <inheritdoc/>
    public IVisibilityTestable GetVisibilityTestable(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetVisibilityTestable(edge, this.style);
    }

    /// <inheritdoc/>
    public IHitTestable GetHitTestable(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetHitTestable(edge, this.style);
    }

    /// <inheritdoc/>
    public IMarqueeTestable GetMarqueeTestable(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetMarqueeTestable(edge, this.style);
    }

    /// <inheritdoc/>
    public ILassoTestable GetLassoTestable(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetLassoTestable(edge, this.style);
    }

    /// <inheritdoc/>
    public ILookup GetContext(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetContext(edge, this.style);
    }

    /// <inheritdoc/>
    public IPathGeometry GetPathGeometry(IEdge edge, IEdgeStyle style) {
      Configure(edge);
      return this.style.Renderer.GetPathGeometry(edge, this.style);
    }

    #endregion
  }
}
