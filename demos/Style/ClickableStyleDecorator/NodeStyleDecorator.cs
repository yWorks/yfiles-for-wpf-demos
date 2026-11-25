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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ClickableStyleDecorator
{
  /// <summary>
  ///   This node style decorator adds an image in the upper right corner of a given node style.
  /// </summary>
  /// <remarks>
  ///   The <see cref="ImageNodeStyle"/> class is used to render the decoration image.
  ///   This style overrides <see cref="NodeStyleBase{TVisual}.IsVisible(ICanvasContext, RectD, INode)"/> with a custom
  ///   implementation that also
  ///   checks the visibility of the decoration image in addition to calling the implementation of the decorated style.
  ///   Other checks like <see cref="NodeStyleBase{TVisual}.IsHit(IInputModeContext, PointD, INode)"/> and
  ///   <see cref="NodeStyleBase{TVisual}.IsInBox(IInputModeContext, RectD, INode)"/> are
  ///   simply delegated to the base style in order to not make the node selectable by clicking or marquee selecting the
  ///   decoration image part of the visualization. If desired, this feature can be implemented like demonstrated in
  ///   <see cref="NodeStyleDecorator.IsVisible"/>.
  /// </remarks>
  public sealed class NodeStyleDecorator : NodeStyleBase<VisualGroup>
  {
    private readonly INodeStyle baseStyle;
    private readonly ImageNodeStyle imageStyle;
    private readonly SimpleNode dummyDecorationNode = new();

    /// <summary>
    ///   The image used for the decoration.
    /// </summary>
    public ImageSource? Image { get; set; }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="baseStyle">The optional base style.</param>
    /// <param name="image">The image to use for the decoration.</param>
    public NodeStyleDecorator(INodeStyle? baseStyle, ImageSource? image) {
      this.baseStyle = baseStyle ?? new ShapeNodeStyle();
      Image = image;
      imageStyle = new ImageNodeStyle { KeepAspectRatio = true };
      if (image is not null) {
        imageStyle.Image = image;
      }

      // This dummy node is passed to the image node style to render the decoration.
      // Its size is the size of the decoration. Its location is adjusted during each CreateVisual
      // and UpdateVisual.
      dummyDecorationNode.Layout = new RectD(0, 0, 32, 32);
    }

    /// <summary>
    ///   Creates a new visual as combination of the base node visualization and the decoration.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns>The created visual.</returns>
    /// <see cref="NodeStyleBase{TVisual}.CreateVisual(IRenderContext, INode)"/>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      // create the base visualization
      var baseVisual = baseStyle.Renderer.GetVisualCreator(node, baseStyle).CreateVisual(context);
      var group = new VisualGroup();
      group.Add(baseVisual);

      if (Image is null) {
        return group;
      }

      // create the decoration
      imageStyle.Image = Image;
      dummyDecorationNode.Layout = GetDecorationLayout(node.Layout);
      var decorationRenderer = imageStyle.Renderer.GetVisualCreator(dummyDecorationNode, imageStyle);
      var decorationVisual = decorationRenderer.CreateVisual(context);

      // add both to a group
      group.Add(decorationVisual);
      return group;
    }

    /// <summary>
    ///   Updates the provided visual.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">
    ///   The visual that has been created in the call to
    ///   <see cref="NodeStyleBase{TVisual}.CreateVisual(IRenderContext, INode)"/>.
    /// </param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns>The updated visual.</returns>
    /// <see cref="NodeStyleBase{TVisual}.UpdateVisual(IRenderContext, TVisual, INode)"/>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      var expectedChildCount = Image is null ? 1 : 2;
      if (oldVisual.Children.Count != expectedChildCount) {
        return CreateVisual(context, node);
      }

      var children = oldVisual.Children;
      // update the base visual
      var baseVisual = baseStyle.Renderer.GetVisualCreator(node, baseStyle).UpdateVisual(context, children[0]);
      // Check whether the UpdateVisual method created a new element and replace the old one if needed.
      if (!ReferenceEquals(baseVisual, children[0])) {
        children[0] = baseVisual;
      }

      if (Image is null) {
        return oldVisual;
      }

      // update the decoration visual
      // first, check whether the image changed
      if (!ReferenceEquals(Image, imageStyle.Image)) {
        // update the image style for the next UpdateVisual call
        imageStyle.Image = Image;
      }

      dummyDecorationNode.Layout = GetDecorationLayout(node.Layout);
      var decorationRenderer = imageStyle.Renderer.GetVisualCreator(dummyDecorationNode, imageStyle);
      var decorationVisual = decorationRenderer.UpdateVisual(context, children[1]);
      if (!ReferenceEquals(decorationVisual, children[1])) {
        // Check whether the UpdateVisual method created a new element and replace the old one if needed.
        children[1] = decorationVisual;
      }

      // update the image style for the next UpdateVisual call
      return oldVisual;
    }

    /// <summary>
    ///   Returns the bounds of the node and its decoration.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns>The bounds of the visual for the node.</returns>
    protected override RectD GetBounds(ICanvasContext context, INode node) {
      var nodeLayout = node.Layout;
      var nodeBounds = new RectD(nodeLayout.X, nodeLayout.Y, nodeLayout.Width, nodeLayout.Height);
      return Image is null ? nodeBounds : RectD.Add(nodeBounds, GetDecorationLayout(nodeLayout));
    }

    /// <summary>
    ///   Returns whether at least one of the base visualization and the decoration is visible.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="rectangle">The clipping rectangle.</param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if either the base visualization or the decoration is visible.</returns>
    /// <see cref="NodeStyleBase{TVisual}.IsVisible(ICanvasContext, RectD, INode)"/>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) =>
        baseStyle.Renderer.GetVisibilityTestable(node, baseStyle).IsVisible(context, rectangle) ||
        Image is not null && rectangle.Intersects(GetDecorationLayout(node.Layout));

    /// <summary>
    ///   Returns whether the base visualization or the decoration is hit.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="location">The point to test.</param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization or the decoration is hit.</returns>
    /// <see cref="NodeStyleBase{TVisual}.IsHit(IInputModeContext, PointD, INode)"/>
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      // Because we want the decorator to be clickable we need to explicitly check if it was hit.
      var decoratorHit = GetDecorationLayout(node.Layout).Contains(location);
      return baseStyle.Renderer.GetHitTestable(node, baseStyle).IsHit(context, location) || decoratorHit;
    }

    /// <summary>
    ///   Returns whether the base visualization is in the box, we don't want the decoration to be marquee selectable.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="rectangle">The marquee selection box.</param>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization is hit.</returns>
    /// <see cref="NodeStyleBase{TVisual}.IsInBox(IInputModeContext, RectD, INode)"/>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, INode node) =>
        // return only box containment test of baseStyle - we don't want the decoration to be marquee selectable
        baseStyle.Renderer.GetMarqueeTestable(node, baseStyle).IsInBox(context, rectangle);

    /// <summary>
    ///   Gets the intersection of a line with the visual representation of the node.
    /// </summary>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <param name="inner">The coordinates of a point lying inside the shape.</param>
    /// <param name="outer">The coordinates of a point lying outside the shape.</param>
    /// <returns>The intersection point if one has been found; otherwise, <see langword="null"/>.</returns>
    /// <see cref="NodeStyleBase{TVisual}.GetIntersection(INode, PointD, PointD)"/>
    protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) =>
        baseStyle.Renderer.GetShapeGeometry(node, baseStyle).GetIntersection(inner, outer);

    /// <summary>
    ///   Gets the outline of the visual style.
    /// </summary>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <returns>The outline of the visual style.</returns>
    /// <see cref="NodeStyleBase{TVisual}.GetOutline(INode)"/>
    protected override GeneralPath? GetOutline(INode node) =>
        baseStyle.Renderer.GetShapeGeometry(node, baseStyle).GetOutline();

    /// <summary>
    ///   Returns whether the provided point is inside of the base visualization.
    /// </summary>
    /// <param name="node">The node to which this style instance is assigned.</param>
    /// <param name="location">The point to test.</param>
    /// <returns><see langword="true"/> if the provided location is inside of the base visualization.</returns>
    /// <see cref="NodeStyleBase{TVisual}.IsInside(INode, PointD)"/>
    protected override bool IsInside(INode node, PointD location) =>
        // return only inside test of baseStyle
        baseStyle.Renderer.GetShapeGeometry(node, baseStyle).IsInside(location);

    /// <summary>
    ///   Returns the layout of the decoration for the given node layout.
    /// </summary>
    /// <param name="nodeLayout">The layout of the node.</param>
    /// <returns>The layout of the decoration for the given node layout.</returns>
    public RectD GetDecorationLayout(IRectangle nodeLayout) {
      var size = dummyDecorationNode.Layout;
      return new RectD(nodeLayout.X + (nodeLayout.Width - size.Width * 0.5), nodeLayout.Y - size.Height * 0.5,
          size.Width, size.Height);
    }
  }
}
