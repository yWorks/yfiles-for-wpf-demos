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

using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LevelOfDetailStyle
{
  /// <summary>
  ///   Selects one of three node styles according to the current zoom level.
  /// </summary>
  public sealed class LevelOfDetailNodeStyle : NodeStyleBase<VisualGroup>
  {
    private readonly INodeStyle detailStyle;
    private readonly INodeStyle intermediateStyle;
    private readonly INodeStyle overviewStyle;

    public double DetailThreshold { get; }

    public double IntermediateThreshold { get; }

    /// <summary>
    ///   Creates a node style which combines three node styles for different zoom levels.
    /// </summary>
    public LevelOfDetailNodeStyle(INodeStyle detailStyle, INodeStyle intermediateStyle, INodeStyle overviewStyle) {
      this.detailStyle = detailStyle;
      this.intermediateStyle = intermediateStyle;
      this.overviewStyle = overviewStyle;
      DetailThreshold = 0.7;
      IntermediateThreshold = 0.4;
    }

    protected override VisualGroup? CreateVisual(IRenderContext context, INode node) {
      var style = GetStyleForZoom(context.Zoom);
      var group = new VisualGroup();
      group.Add(style.Renderer.GetVisualCreator(node, style).CreateVisual(context));
      group.SetRenderDataCache(new NodeVisualCache(style));
      return group;
    }

    protected override VisualGroup? UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      if (oldVisual is null) {
        return CreateVisual(context, node);
      }

      var cache = oldVisual.GetRenderDataCache<NodeVisualCache>();
      var style = GetStyleForZoom(context.Zoom);
      if (cache.Style != style) {
        return CreateVisual(context, node);
      }

      var visualCreator = style.Renderer.GetVisualCreator(node, style);
      oldVisual.Children[0] = visualCreator.UpdateVisual(context, oldVisual.Children[0]);
      return oldVisual;
    }

    private INodeStyle GetStyleForZoom(double zoom) =>
        zoom >= DetailThreshold ? detailStyle : zoom >= IntermediateThreshold ? intermediateStyle : overviewStyle;

    private readonly record struct NodeVisualCache(INodeStyle Style);
  }
}
