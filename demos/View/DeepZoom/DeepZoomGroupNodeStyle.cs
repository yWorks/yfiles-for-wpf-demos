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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DeepZoom;

/// <summary>
///   This group node style creates a visualization of its children if used in the context of folding.
///   The contents are scaled and rendered within the bounds of the node.
/// </summary>
public sealed class DeepZoomGroupNodeStyle : NodeStyleBase<VisualGroup>
{
  private const double ContentRectMargins = 50;
  private const double MinNodeSize = 5;
  private const double MaxZoomChangeThreshold = 3;

  public INodeStyle BackgroundStyle { get; }

  /// <summary>
  ///   Creates a new group node style with the given backgroundStyle.
  /// </summary>
  public DeepZoomGroupNodeStyle(INodeStyle? backgroundStyle = null) {
    BackgroundStyle = backgroundStyle ?? new ShapeNodeStyle();
  }

  /// <summary>
  ///   Creates a visual of the given node. The visual consists of the background of the node and, if
  ///   the node has been zoomed out, an image that shows the content of the node.
  /// </summary>
  protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
    var outerGroup = new VisualGroup();

    var background = CreateBackgroundVisual(node, context);
    outerGroup.Add(background);

    var contents = new VisualGroup();
    outerGroup.Add(contents);

    var contentsBounds = CreateContentsVisual(context, node, contents);
    outerGroup.SetRenderDataCache(new DeepZoomRenderDataCache(context.Zoom, contentsBounds));

    return outerGroup;
  }

  /// <summary>
  ///   Updates the current visual instead of creating a new one.
  /// </summary>
  protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
    var cache = oldVisual.GetRenderDataCache<DeepZoomRenderDataCache>();

    if (cache.RenderedZoom < context.Zoom / MaxZoomChangeThreshold ||
        cache.RenderedZoom > MaxZoomChangeThreshold * context.Zoom) {
      return CreateVisual(context, node);
    }

    var backgroundGroup = (VisualGroup) oldVisual.Children[0]!;
    var background = backgroundGroup.Children[0]!;
    var updatedBackground = BackgroundStyle.Renderer.GetVisualCreator(node, BackgroundStyle)
        .UpdateVisual(context, background);
    if (!ReferenceEquals(background, updatedBackground)) {
      backgroundGroup.Children[0] = updatedBackground;
    }
    SetBackgroundOpacity(context, backgroundGroup);

    UpdateContentsVisual((VisualGroup) oldVisual.Children[1]!, node, cache.ContentsBounds);

    return oldVisual;
  }

  /// <summary>
  ///   Delegates intersection calculation to the <see cref="BackgroundStyle"/>.
  /// </summary>
  /// <param name="node">The node that has to be tested for intersections.</param>
  /// <param name="inner">The first point of the line that is inside the shape.</param>
  /// <param name="outer">The second point of the line that is outside the shape.</param>
  /// <returns>The coordinates of the intersection point, if an intersection was found.</returns>
  protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) =>
      BackgroundStyle.Renderer.GetShapeGeometry(node, BackgroundStyle).GetIntersection(inner, outer);

  /// <summary>
  ///   Delegates outline calculation to the <see cref="BackgroundStyle"/>.
  /// </summary>
  /// <returns>The outline or null if no outline can be provided.</returns>
  protected override GeneralPath? GetOutline(INode node) =>
      BackgroundStyle.Renderer.GetShapeGeometry(node, BackgroundStyle).GetOutline();

  private Visual CreateBackgroundVisual(INode node, IRenderContext renderContext) {
    var backgroundVisual = BackgroundStyle.Renderer.GetVisualCreator(node, BackgroundStyle).CreateVisual(renderContext);

    var backgroundGroup = new VisualGroup();
    SetBackgroundOpacity(renderContext, backgroundGroup);
    backgroundGroup.Add(backgroundVisual);
    return backgroundGroup;
  }

  private RectD? CreateContentsVisual(IRenderContext renderContext, INode node, VisualGroup contentsGroup) {
    contentsGroup.SetCanvasArrangeRect(new Rect(node.Layout.X, node.Layout.Y, node.Layout.Width, node.Layout.Height));

    // if the group node appears large enough, render the contained graph as static UIElement
    if (renderContext.Zoom * Math.Max(node.Layout.Width, node.Layout.Height) > MinNodeSize) {
      var (contents, contentsBounds) = CreateScaledContentsVisualization(renderContext, node);

      // scale and translate the visual group to fit the displaying node
      contents.SetCanvasArrangeRect(new Rect(0, 0, contentsBounds.Width, contentsBounds.Height));
      contentsGroup.Add(contents);
      contentsGroup.Transform = new MatrixTransform(ComputeTransform(node, contentsBounds));

      return contentsBounds;
    }

    return null;
  }

  /// <summary>
  ///   Creates a static visualization of the contents graph scaled to the container node size.
  /// </summary>
  private (UIElement Contents, RectD ContentsBounds) CreateScaledContentsVisualization(
      IRenderContext renderContext,
      INode containerNode) {
    var graph = renderContext.CanvasControl as GraphControl;
    var foldingView = graph?.Graph.GetFoldingView() ??
        throw new InvalidOperationException("The graph does not provide a folding view.");

    // create a copy of the direct children of this node by using a non-expanded folding view.
    var tempView = foldingView.Manager.CreateFoldingView(foldingView.GetMasterItem(containerNode), _ => false);
    var tempGraphComponent = new GraphControl { Graph = tempView.Graph };
    try {
      tempGraphComponent.UpdateContentBounds(new InsetsD(ContentRectMargins));

      var allBounds = new RectD(0, 0, tempGraphComponent.ContentBounds.Width, tempGraphComponent.ContentBounds.Height);

      // configure a rendering of the groups contents
      var uiElementExport = new UIElementExporter(tempGraphComponent.ContentBounds);

      // By default, the rendering has a zoom of one and the contained nodes are their 'true' sizes in world coordinates.
      // Thus, the image needs to be scaled down to the apparent size of the group node
      var scale = Math.Max(0.00001,
          Math.Min(containerNode.Layout.Width / allBounds.Width, containerNode.Layout.Height / allBounds.Height));
      uiElementExport.Configurator.Zoom = renderContext.Zoom * scale;

      // actually create the UIElement
      var visual = uiElementExport.Export(tempGraphComponent);

      return (visual, allBounds);
    } finally {
      tempView.Dispose();
    }
  }

  private void UpdateContentsVisual(VisualGroup contents, INode node, RectD? contentsBounds) {
    if (contentsBounds is { } bounds) {
      contents.Transform = new MatrixTransform(ComputeTransform(node, bounds));
    }
  }

  private void SetBackgroundOpacity(IRenderContext renderContext, VisualGroup backgroundGroup) {
    backgroundGroup.Opacity = Math.Clamp(4 / renderContext.Zoom - 0.02, 0, 1);
  }

  /// <summary>
  ///   Computes the transform for the rendering of the subgraph to fit the node that displays it.
  /// </summary>
  /// <param name="containerNode">The group node whose contents have to be scaled</param>
  /// <param name="contentsBounds">The actual bounds of the group node</param>
  /// <returns>The transformation matrix</returns>
  private static Matrix ComputeTransform(INode containerNode, RectD contentsBounds) {
    var layout = containerNode.Layout;
    var width = layout.Width;
    var height = layout.Height;
    var scale = Math.Min(width / contentsBounds.Width, height / contentsBounds.Height);
    return new Matrix(scale, 0, 0, scale, (width - contentsBounds.Width * scale) * 0.5 - contentsBounds.X * scale,
        (height - contentsBounds.Height * scale) * 0.5 - contentsBounds.Y * scale);
  }

  private readonly record struct DeepZoomRenderDataCache(double RenderedZoom, RectD? ContentsBounds);
}
