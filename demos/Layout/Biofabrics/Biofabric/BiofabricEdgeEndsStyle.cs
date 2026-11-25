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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Draws the two endpoint markers on an edge in the biofabric, using the corresponding endpoint colors.
  /// </summary>
  public sealed class BiofabricEdgeEndsStyle : EdgeStyleBase<DrawingVisual>
  {
    /// <summary>Gets the thickness of the edge endpoint markers.</summary>
    public double Thickness { get; }

    /// <summary>Gets the radius of each endpoint marker.</summary>
    public double MarkerRadius { get; }

    /// <summary>Gets the color used for the source endpoint marker.</summary>
    public string? ColorA { get; }

    /// <summary>Gets the color used for the target endpoint marker.</summary>
    public string? ColorB { get; }

    /// <summary>Gets or sets whether both endpoint markers use the highlight color.</summary>
    public bool IsHighlighted { get; set; }

    /// <summary>
    ///   Creates a new BiofabricEdgeEndsStyle.
    /// </summary>
    /// <param name="thickness">The thickness of the edge.</param>
    /// <param name="markerRadius">The radius of the circles attached to the edge's ends.</param>
    /// <param name="colorA">The color at the source node of the edge.</param>
    /// <param name="colorB">The color at the target node of the edge.</param>
    public BiofabricEdgeEndsStyle(double thickness, double markerRadius, string? colorA = null, string? colorB = null) {
      Thickness = thickness;
      MarkerRadius = markerRadius;
      ColorA = colorA;
      ColorB = colorB;
    }

    /// <summary>
    ///   Creates the visual for an edge.
    /// </summary>
    protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
      var visual = new DrawingVisual();
      Draw(context, visual, edge);
      return visual;
    }

    /// <summary>
    ///   Updates the created Biofabric edge visual.
    /// </summary>
    protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, IEdge edge) {
      Draw(context, oldVisual, edge);
      return oldVisual;
    }

    private void Draw(IRenderContext context, DrawingVisual visual, IEdge edge) {
      // Specify target and source points of the edge
      var source = edge.SourcePort.GetLocation();
      var target = edge.TargetPort.GetLocation();
      var fallback = Brushes.Black;
      var biofabric = context.CanvasControl.Lookup<Biofabric>();
      var anythingHighlighted = biofabric?.AnythingHighlighted ?? false;

      var sourceBrush = BiofabricRenderUtils.CreateBrush(ColorA, fallback);
      var targetBrush = BiofabricRenderUtils.CreateBrush(ColorB ?? ColorA, fallback);
      if (IsHighlighted) {
        sourceBrush = BiofabricRenderUtils.DefaultHighlight;
        targetBrush = BiofabricRenderUtils.DefaultHighlight;
      } else if (anythingHighlighted) {
        sourceBrush = BiofabricRenderUtils.CreateTransparent(sourceBrush);
        targetBrush = BiofabricRenderUtils.CreateTransparent(targetBrush);
      }

      using var dc = visual.RenderOpen();
      dc.DrawEllipse(sourceBrush, null, source.ToPoint(), MarkerRadius, MarkerRadius);
      dc.DrawEllipse(targetBrush, null, target.ToPoint(), MarkerRadius, MarkerRadius);
    }

    /// <summary>
    ///   Gets the rectangular bounds of the edge visual.
    /// </summary>
    protected override RectD GetBounds(ICanvasContext context, IEdge edge) =>
        base.GetBounds(context, edge).GetEnlarged(Math.Max(Thickness, MarkerRadius));
  }
}
