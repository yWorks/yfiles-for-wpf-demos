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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   The renderer in charge of visually representing an edge group in the biofabric.
  /// </summary>
  public sealed class EdgeGroupRenderer : ObjectRendererBase<EdgeGroupRendererRenderTag, DrawingVisual>
  {
    private readonly double offset;

    /// <summary>Gets the height of the edge-group bar and its marker.</summary>
    public double Thickness { get; }

    /// <summary>Gets or sets the edge-group color as a color string.</summary>
    public string? Color { get; set; }

    /// <summary>Gets or sets whether the group renderer is highlighted.</summary>
    public bool IsHighlighted { get; set; }

    /// <summary>Gets or sets whether another group renderer is highlighted.</summary>
    public bool OtherIsHighlighted { get; set; }

    /// <summary>
    ///   Creates a new biofabric edge group renderer instance.
    /// </summary>
    /// <param name="y">The offset, i.e., lowest y-coordinate, of a biofabric's edge group visual.</param>
    /// <param name="thickness">The height of the edge group bar.</param>
    /// <param name="color">The color hex of the edge group visual.</param>
    public EdgeGroupRenderer(double y, double thickness, string? color = null) {
      offset = y;
      Thickness = thickness;
      Color = color;
    }

    public void ResetHighlight() {
      IsHighlighted = false;
      OtherIsHighlighted = false;
    }

    /// <summary>
    ///   Creates a new biofabric edge group visual.
    /// </summary>
    /// <param name="context">The visualization's render context.</param>
    /// <param name="renderTag">The tag containing the edge group's name and edge data.</param>
    /// <returns>A new edge group visual.</returns>
    protected override DrawingVisual CreateVisual(IRenderContext context, EdgeGroupRendererRenderTag renderTag) {
      var visual = new DrawingVisual();
      Draw(context, visual, renderTag);
      return visual;
    }

    /// <summary>Updates an existing visual to reflect the current edge-group data and state.</summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual previously created for this group.</param>
    /// <param name="renderTag">The group name and edge-group data to render.</param>
    /// <returns>The updated visual.</returns>
    protected override DrawingVisual UpdateVisual(
        IRenderContext context,
        DrawingVisual oldVisual,
        EdgeGroupRendererRenderTag renderTag) {
      Draw(context, oldVisual, renderTag);
      return oldVisual;
    }

    private void Draw(IRenderContext context, DrawingVisual visual, EdgeGroupRendererRenderTag renderTag) {
      // Extract the edge group and its max/min bounds
      var edgeGroup = renderTag.EdgeGroup;
      var (min, max) = CalculateMinMax(edgeGroup.Edges);
      var centerX = (max + min) / 2;
      var color = BiofabricRenderUtils.CreateBrush(Color, Brushes.Black);
      var highlight = IsHighlighted || edgeGroup.Highlighted;
      var currentColor = highlight ? BiofabricRenderUtils.DefaultHighlight :
          edgeGroup.OtherHighlighted ? BiofabricRenderUtils.CreateTransparent(color) : color;

      var background = BiofabricRenderUtils.DefaultBackground;

      using var dc = visual.RenderOpen();
      // Draw the rectangle spanning the width of the edge group.
      var rectangle = new Rect(min, offset, Math.Max(0, max - min), Thickness);
      dc.DrawRoundedRectangle(currentColor, BiofabricRenderUtils.CreatePen(currentColor, 1), rectangle, Thickness / 2,
          Thickness / 2);

      // Draw the edge group's padding between its circle and rectangle.
      var centerY = offset + 0.5 * Thickness;
      dc.DrawEllipse(background, null, new Point(centerX, centerY), Thickness * 3, Thickness * 3);

      // Draw the edge group's circle indicating its center and collapsed state.
      var circleBrush = edgeGroup.Collapsed ? currentColor : background;
      var circleColor = currentColor;
      dc.DrawEllipse(circleBrush, BiofabricRenderUtils.CreatePen(circleColor, Thickness), new Point(centerX, centerY),
          Thickness, Thickness);

      // Draw the edge group's label/ID.
      var textBrush = currentColor;
      var text = BiofabricRenderUtils.FormatText(renderTag.GroupName, 3 * Thickness, textBrush);
      text.TextAlignment = TextAlignment.Center;
      dc.DrawText(text, new Point(centerX, offset - Thickness * 3 - text.Height));
    }

    private static (double Min, double Max) CalculateMinMax(IEnumerable<IEdge> edges) {
      var locations = edges.Select(edge => edge.SourcePort.GetLocation().X).ToArray();
      return locations.Length == 0 ? (0, 0) : (locations.Min(), locations.Max());
    }

    /// <summary>
    ///   Calculate the rectangular bounds of the edge group's visual elements.
    /// </summary>
    protected override RectD GetBounds(ICanvasContext context, EdgeGroupRendererRenderTag renderTag) {
      var text = BiofabricRenderUtils.FormatText(renderTag.GroupName, 3 * Thickness, Brushes.White);
      var (min, max) = CalculateMinMax(renderTag.EdgeGroup.Edges);
      var width = Math.Max(max - min, text.Width);
      return new RectD(max - min > text.Width ? min : min - 0.5 * text.Width, offset - Thickness * 5.5, width,
          Thickness * 7.5);
    }

    /// <summary>
    ///   Check whether the given coordinate pair falls within the group's rectangular bounds.
    /// </summary>
    protected override bool IsHit(IInputModeContext context, PointD location, EdgeGroupRendererRenderTag renderTag) =>
        GetBounds(context, renderTag).Contains(location);
  }
}
