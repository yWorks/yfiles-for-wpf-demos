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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.StyleDecorators
{
  /// <summary>
  ///   This edge style decorator shows how to decorate an edge style with bends that are rendered by a port style.
  /// </summary>
  /// <remarks>
  ///   This implementation wraps <see cref="PolylineEdgeStyle"/>.
  ///   The <see cref="PolylineEdgeStyle.Pen"/> of the wrapped style is modified based on the value stored
  ///   in the edge's tag. An arbitrary <see cref="IPortStyle"/> port style, that can be set in the
  ///   constructor, is used to render the edge's bends.
  /// </remarks>
  public sealed class EdgeStyleDecorator : EdgeStyleBase<VisualGroup>
  {
    private readonly IPortStyle bendStyle;
    private readonly PolylineEdgeStyle baseStyle;

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="bendStyle">A port style that is used to draw the bends.</param>
    public EdgeStyleDecorator(IPortStyle bendStyle) {
      this.bendStyle = bendStyle;
      baseStyle = new PolylineEdgeStyle { SmoothingLength = 5.0 };
    }

    /// <summary>
    ///   Creates a new visual as combination of the base edge visualization and the bend visualizations.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="edge">The edge to which this style instance is assigned.</param>
    /// <returns>The created visual.</returns>
    /// <see cref="EdgeStyleBase{TVisual}.CreateVisual(IRenderContext, IEdge)"/>
    protected override VisualGroup CreateVisual(IRenderContext context, IEdge edge) {
      // create container
      var group = new VisualGroup();

      // delegate rendering
      var tag = edge.Tag as string;
      var pen = GetPen(tag);
      baseStyle.Pen = pen;
      var baseVisual = baseStyle.Renderer.GetVisualCreator(edge, baseStyle).CreateVisual(context);
      group.Add(baseVisual!);

      var bendGroup = new VisualGroup();
      group.Add(bendGroup);
      RenderBends(context, bendGroup, edge, pen);
      group.SetRenderDataCache(new EdgeVisualCache(tag, pen));
      return group;
    }

    /// <summary>
    ///   Updates the provided visual.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">
    ///   The visual that has been created in the call to
    ///   <see cref="EdgeStyleBase{TVisual}.CreateVisual(IRenderContext, IEdge)"/>.
    /// </param>
    /// <param name="edge">The edge to which this style instance is assigned.</param>
    /// <returns>The updated visual.</returns>
    /// <see cref="EdgeStyleBase{TVisual}.UpdateVisual(IRenderContext, TVisual, IEdge)"/>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IEdge edge) {
      // check whether the elements are as expected
      if (oldVisual.Children.Count != 2 || oldVisual.Children[1] is not VisualGroup bendGroup) {
        return CreateVisual(context, edge);
      }

      var cache = oldVisual.GetRenderDataCache<EdgeVisualCache>();
      var tag = edge.Tag as string;
      var pen = cache.Tag == tag ? cache.Pen : GetPen(tag);

      var baseVisual = oldVisual.Children[0];
      // delegate update
      baseStyle.Pen = pen;
      baseVisual = baseStyle.Renderer.GetVisualCreator(edge, baseStyle).UpdateVisual(context, baseVisual);
      if (!ReferenceEquals(oldVisual.Children[0], baseVisual)) {
        oldVisual.Children[0] = baseVisual!;
      }

      RenderBends(context, bendGroup, edge, pen);
      oldVisual.SetRenderDataCache(new EdgeVisualCache(tag, pen));
      return oldVisual;
    }

    /// <summary>
    ///   Renders the edge's bends, using <see cref="bendStyle"/> and dummy ports.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="group">The group element.</param>
    /// <param name="edge">The edge.</param>
    private void RenderBends(IRenderContext context, VisualGroup group, IEdge edge, Pen pen) {
      var bends = edge.Bends;

      // remove surplus visuals
      while (group.Children.Count > bends.Count) {
        // remove last child
        group.Remove(group.Children[^1]);
      }

      var bendStyle = this.bendStyle;
      if (bendStyle is ShapePortStyle shapePortStyle) {
        var diameter = pen.Thickness * 2;
        shapePortStyle.RenderSize = new SizeD(diameter, diameter);
        shapePortStyle.Brush = pen.Brush;
      }

      // update existing bend visuals
      var dummyPort = new SimplePort(edge);
      var portLocationModel = BendAnchoredPortLocationModel.Instance;
      for (var i = 0; i < group.Children.Count; i++) {
        // place the dummy port at the bend's location
        dummyPort.LocationParameter = portLocationModel.CreateParameterFromSource(i);

        // update the dummy port visual
        var visual = bendStyle.Renderer.GetVisualCreator(dummyPort, bendStyle).UpdateVisual(context, group.Children[i]);
        // switch instances if necessary
        if (!ReferenceEquals(group.Children[i], visual)) {
          group.Children[i] = visual!;
        }
      }

      // add missing visuals
      for (var i = group.Children.Count; i < bends.Count; i++) {
        // place the dummy port at the bend's location
        dummyPort.LocationParameter = portLocationModel.CreateParameterFromSource(i);

        // render the dummy port visual
        var bendVisual = bendStyle.Renderer.GetVisualCreator(dummyPort, bendStyle).CreateVisual(context);
        group.Add(bendVisual!);
      }
    }

    /// <summary>
    ///   Returns a pen for the provided data.
    /// </summary>
    /// <param name="data">A custom data object.</param>
    /// <returns>The pen for the provided data.</returns>
    private static Pen GetPen(string? data) {
      return data switch {
          "TRAFFIC_VERY_HIGH" => new Pen(new SolidColorBrush(Color.FromRgb(0xDB, 0x3A, 0x34)), 3.5),
          "TRAFFIC_HIGH" => new Pen(new SolidColorBrush(Color.FromRgb(0xFF, 0x6C, 0x00)), 2.5),
          "TRAFFIC_LOW" => new Pen(new SolidColorBrush(Color.FromRgb(0xC1, 0xC1, 0xC1)), 1.5),
          _ => new Pen(Brushes.Black, 1.5)
      };
    }

    /// <summary>
    ///   Returns the bounds provided by the base style for the edge.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="edge">The edge to which this style instance is assigned.</param>
    /// <returns>The visual bounds.</returns>
    /// <see cref="EdgeStyleBase{TVisual}.GetBounds(ICanvasContext, IEdge)"/>
    protected override RectD GetBounds(ICanvasContext context, IEdge edge) =>
        baseStyle.Renderer.GetBoundsProvider(edge, baseStyle).GetBounds(context);

    /// <summary>
    ///   Returns whether the base visualization for the specified edge is visible.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="rectangle">The clipping rectangle.</param>
    /// <param name="edge">The edge to which this style instance is assigned.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified edge is visible in the clipping rectangle;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <see cref="EdgeStyleBase{TVisual}.IsVisible(ICanvasContext, RectD, IEdge)"/>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, IEdge edge) =>
        baseStyle.Renderer.GetVisibilityTestable(edge, baseStyle).IsVisible(context, rectangle);

    /// <summary>
    ///   Returns whether the base visualization is hit.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="location">The point to test.</param>
    /// <param name="edge">The edge to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization is hit.</returns>
    /// <see cref="EdgeStyleBase{TVisual}.IsHit(IInputModeContext, PointD, IEdge)"/>
    protected override bool IsHit(IInputModeContext context, PointD location, IEdge edge) =>
        baseStyle.Renderer.GetHitTestable(edge, baseStyle).IsHit(context, location);

    /// <summary>
    ///   Returns whether the base visualization is in the box.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="rectangle">The marquee selection box.</param>
    /// <param name="edge">The edge to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization is in the box.</returns>
    /// <see cref="EdgeStyleBase{TVisual}.IsInBox(IInputModeContext, RectD, IEdge)"/>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, IEdge edge) =>
        // return only box containment test of baseStyle - we don't want the decoration to be marquee selectable
        baseStyle.Renderer.GetMarqueeTestable(edge, baseStyle).IsInBox(context, rectangle);

    /// <summary>
    ///   Delegates the lookup to the base style.
    /// </summary>
    /// <param name="edge">The edge to use for the context lookup.</param>
    /// <param name="type">The type to query.</param>
    /// <returns>An implementation of the <paramref name="type"/> or <see langword="null"/>.</returns>
    /// <see cref="EdgeStyleBase{TVisual}.Lookup(IEdge, System.Type)"/>
    protected override object? Lookup(IEdge edge, Type type) =>
        baseStyle.Renderer.GetContext(edge, baseStyle).Lookup(type);

    private readonly record struct EdgeVisualCache(string? Tag, Pen Pen);
  }
}
