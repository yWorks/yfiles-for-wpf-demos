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
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   The renderer in charge of visually representing a node group in the biofabric.
  /// </summary>
  public sealed class NodeGroupRenderer : ObjectRendererBase<NodeGroupRendererRenderTag, DrawingVisual>
  {
    private readonly double offset;

    /// <summary>Gets the width of the node-group bar and its marker.</summary>
    public double Thickness { get; }

    /// <summary>Gets or sets the node-group color as a color string.</summary>
    public string? Color { get; set; }

    /// <summary>Gets or sets whether the group renderer is highlighted.</summary>
    public bool IsHighlighted { get; set; }

    /// <summary>Gets or sets whether another group renderer is highlighted.</summary>
    public bool OtherIsHighlighted { get; set; }

    /// <summary>
    ///   Creates a new biofabric node group renderer instance.
    /// </summary>
    /// <param name="x">The offset, i.e., right-most x-coordinate, of a biofabric's node group visual.</param>
    /// <param name="thickness">The width of the node group bar.</param>
    /// <param name="color">The color hex of the edge group visual.</param>
    public NodeGroupRenderer(double x, double thickness, string? color = null) {
      offset = x;
      Thickness = thickness;
      Color = color;
    }

    public void ResetHighlight() {
      IsHighlighted = false;
      OtherIsHighlighted = false;
    }

    /// <summary>
    ///   Creates a new biofabric node group visual.
    /// </summary>
    /// <param name="context">The visualization's render context.</param>
    /// <param name="renderTag">The tag containing the node group's name and node data.</param>
    /// <returns>A new node group visual.</returns>
    protected override DrawingVisual CreateVisual(IRenderContext context, NodeGroupRendererRenderTag renderTag) {
      var visual = new DrawingVisual();
      Draw(context, visual, renderTag);
      return visual;
    }

    /// <summary>Updates an existing visual to reflect the current node-group data and state.</summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual previously created for this group.</param>
    /// <param name="renderTag">The group name and node-group data to render.</param>
    /// <returns>The updated visual.</returns>
    protected override DrawingVisual UpdateVisual(
        IRenderContext context,
        DrawingVisual oldVisual,
        NodeGroupRendererRenderTag renderTag) {
      Draw(context, oldVisual, renderTag);
      return oldVisual;
    }

    private void Draw(IRenderContext context, DrawingVisual visual, NodeGroupRendererRenderTag renderTag) {
      // Extract the node group's nodes and collapsed state
      var group = renderTag.NodeGroup;
      var bounds = BiofabricRenderUtils.BoundsOfNodes(group.Nodes, context);
      var min = bounds.IsEmpty() ? 0 : bounds.Y;
      var max = bounds.IsEmpty() ? 0 : bounds.GetMaxY();
      var centerY = (max + min) / 2;
      var color = BiofabricRenderUtils.CreateBrush(Color, Brushes.Black);
      var highlight = IsHighlighted || group.Highlighted;
      var currentColor = highlight ? BiofabricRenderUtils.DefaultHighlight :
          group.OtherHighlighted ? BiofabricRenderUtils.CreateTransparent(color) : color;
      var background = BiofabricRenderUtils.DefaultBackground;

      using var dc = visual.RenderOpen();
      // Draw the rectangle spanning the height of the node group.
      var rectangle = new Rect(offset, min, Thickness, Math.Max(0, max - min));
      dc.DrawRoundedRectangle(currentColor, BiofabricRenderUtils.CreatePen(currentColor, 1), rectangle, Thickness / 2,
          Thickness / 2);

      // Draw the node group's padding between its circle and rectangle.
      var centerX = offset + 0.5 * Thickness;
      dc.DrawEllipse(background, null, new Point(centerX, centerY), Thickness * 3, Thickness * 3);

      // Draw the node group's circle indicating its center and collapsed state.
      var circleBrush = group.Collapsed ? color : background;
      var circleColor = currentColor;
      dc.DrawEllipse(circleBrush, BiofabricRenderUtils.CreatePen(circleColor, Thickness), new Point(centerX, centerY),
          Thickness, Thickness);

      // Draw the node group's label/ID.
      var textBrush = currentColor;
      var text = BiofabricRenderUtils.FormatText(renderTag.GroupName, 3 * Thickness, textBrush);
      text.TextAlignment = TextAlignment.Right;
      text.Trimming = TextTrimming.None;
      dc.DrawText(text, new Point(offset - Thickness * 3, centerY - text.Height / 2));
    }

    /// <summary>
    ///   Check whether the given coordinate pair falls within the group's rectangular bounds.
    /// </summary>
    protected override bool IsHit(IInputModeContext context, PointD location, NodeGroupRendererRenderTag renderTag) =>
        GetBounds(context, renderTag).Contains(location);

    /// <summary>
    ///   Calculate the rectangular bounds of the node group's visual elements.
    /// </summary>
    protected override RectD GetBounds(ICanvasContext context, NodeGroupRendererRenderTag renderTag) {
      var text = BiofabricRenderUtils.FormatText(renderTag.GroupName, 3 * Thickness, Brushes.White);
      var bounds = BiofabricRenderUtils.BoundsOfNodes(renderTag.NodeGroup.Nodes, context);
      var min = bounds.IsEmpty() ? 0 : bounds.Y;
      var max = bounds.IsEmpty() ? 0 : bounds.GetMaxY();
      var halfTextHeight = text.Height / 2;
      var top = max - min < text.Height ? min - halfTextHeight : min;
      var bottom = max - min < text.Height ? max + halfTextHeight : max;
      return new RectD(offset - Thickness * 3 - text.Width, top,
          offset + Thickness * 2.5 - (offset - Thickness * 3 - text.Width), bottom - top);
    }
  }
}
