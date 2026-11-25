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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   The SimpleGradientDelegatingEdgeStyle extends DelegatingEdgeStyle to visualize an edge with a color gradient.
  /// </summary>
  public sealed class SimpleGradientDelegatingEdgeStyle : DelegatingEdgeStyle
  {
    /// <summary>Gets the wrapped edge style whose geometry is used to draw the edge.</summary>
    public IEdgeStyle WrappedStyle { get; }

    private readonly Func<IEdge, (string? Source, string? Target)> edgeToColorMapper;

    /// <summary>Gets or sets whether the edge is rendered with the highlight color.</summary>
    public bool IsHighlighted { get; set; }

    private readonly Dictionary<string, LinearGradientBrush> gradients = new();
    private readonly Dictionary<string, LinearGradientBrush> transparentGradients = new();

    /// <summary>
    ///   Instantiates a new SimpleGradientDelegatingEdgeStyle.
    /// </summary>
    /// <param name="wrappedStyle">The IEdgeStyle around which this style wraps.</param>
    /// <param name="edgeToColorMapper">Maps an edge to source and target colors.</param>
    public SimpleGradientDelegatingEdgeStyle(
        IEdgeStyle wrappedStyle,
        Func<IEdge, (string? Source, string? Target)> edgeToColorMapper) {
      WrappedStyle = wrappedStyle;
      this.edgeToColorMapper = edgeToColorMapper;
    }

    /// <summary>
    ///   Creates the custom visual for a gradient edge.
    /// </summary>
    protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
      var visual = new DrawingVisual();
      Draw(context, visual, edge);
      return visual;
    }

    /// <summary>
    ///   Updates a given gradient edge visual.
    /// </summary>
    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, IEdge edge) {
      var visual = (DrawingVisual) oldVisual;
      Draw(context, visual, edge);
      return visual;
    }

    private void Draw(IRenderContext context, DrawingVisual visual, IEdge edge) {
      var source = edge.SourcePort.GetLocation();
      var target = edge.TargetPort.GetLocation();
      var sourceIsUpper = source.Y < target.Y;
      var upperPort = sourceIsUpper ? source : target;
      var lowerPort = sourceIsUpper ? target : source;
      var width = WrappedStyle is PolylineEdgeStyle polylineStyle ? polylineStyle.Pen?.Thickness ?? 10 : 10;
      var (sourceColor, targetColor) = edgeToColorMapper(edge);

      Brush fill;
      if (IsHighlighted) {
        fill = BiofabricRenderUtils.DefaultHighlight;
      } else {
        var biofabric = context.CanvasControl.Lookup<Biofabric>();
        var anythingHighlighted = biofabric?.AnythingHighlighted ?? false;

        if (sourceColor is not null && targetColor is not null && sourceColor != targetColor) {
          var key = sourceIsUpper ? sourceColor + targetColor : targetColor + sourceColor;
          if (!gradients.TryGetValue(key, out var gradient)) {
            var start = BiofabricRenderUtils.CreateBrush(sourceIsUpper ? sourceColor : targetColor, Brushes.Black);
            var end = BiofabricRenderUtils.CreateBrush(sourceIsUpper ? targetColor : sourceColor, Brushes.Black);
            gradient = new LinearGradientBrush {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            gradient.GradientStops.Add(new GradientStop(((SolidColorBrush) start).Color, 0));
            gradient.GradientStops.Add(new GradientStop(((SolidColorBrush) end).Color, 1));
            gradient.Freeze();
            gradients[key] = gradient;
            var transparentGradient = new LinearGradientBrush {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            var gradientStartColor = BiofabricRenderUtils.CreateTransparent(start);
            var gradientEndColor = BiofabricRenderUtils.CreateTransparent(end);
            transparentGradient.GradientStops.Add(new GradientStop(((SolidColorBrush) gradientStartColor).Color, 0));
            transparentGradient.GradientStops.Add(new GradientStop(((SolidColorBrush) gradientEndColor).Color, 1));
            transparentGradient.Freeze();
            transparentGradients[key] = transparentGradient;
          }
          fill = anythingHighlighted ? transparentGradients[key] : gradient;
        } else {
          var brush = BiofabricRenderUtils.CreateBrush(sourceColor ?? targetColor, Brushes.Black);
          fill = anythingHighlighted ? BiofabricRenderUtils.CreateTransparent(brush) : brush;
        }
      }

      using var dc = visual.RenderOpen();
      dc.DrawRectangle(fill, null,
          new Rect(upperPort.X - width / 2, upperPort.Y, width, Math.Max(0, lowerPort.Y - upperPort.Y)));
    }

    /// <summary>
    ///   Gets the style to use for rendering the edge.
    /// </summary>
    protected override IEdgeStyle GetStyle(IEdge edge) => WrappedStyle;
  }
}
