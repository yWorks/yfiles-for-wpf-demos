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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.Legend;

/// <summary>
///   Legend built from a UIElement export of a graph, added to the render tree of the graph control,
///   rendered in view coordinates.
/// </summary>
internal sealed class RenderTreeViewLegend : LegendBase
{
  private const double LegendMargin = 16;
  private const double MinLegendScale = 0.5;
  private IRenderTreeElement? legendElement;
  private readonly RenderTreeLegendPosition legendPosition;

  public RenderTreeViewLegend(GraphControl graphControl, GraphViewerInputMode inputMode) : base(graphControl) {
    // create a custom position handler used for dragging the legend
    legendPosition = new RenderTreeLegendPosition(UpdateLegendPosition);
    // add a custom legend move input mode to the main input mode
    inputMode.Add(new RenderTreeLegendMoveInputMode(graphControl, GetLegendBounds, () => legendPosition.Position,
        legendPosition.SetPosition, true));
    graphControl.SizeChanged += OnGraphControlSizeChanged;
  }

  protected override void OnHide() {
    if (legendElement is not null) {
      graphControl.RenderTree.Remove(legendElement);
      legendElement = null;
    }
  }

  protected override void RefreshCore(IReadOnlyList<NodeTypeId> visibleTypes) {
    // create the WPF visual
    var (legendVisual, size) = CreateLegendVisual(visibleTypes);
    var layout = CalculateLegendLayout(size);
    var position = legendPosition.SetDefaultPosition(layout.Position);
    var renderTag = new LegendRenderTag(
        legendVisual, size, position, layout.Scale, true, legendPosition.IsManualPosition);

    if (legendElement is null) {
      // if there is no render tree element yet, add one with its dedicated renderer
      legendElement = graphControl.RenderTree.CreateElement(graphControl.RenderTree.ForegroundGroup, renderTag,
          new RenderTreeLegendRenderer(UpdateLegendLayout));
    } else {
      // if there is already a render tree element, update its tag and invalidate the graph control
      legendElement.Tag = renderTag;
      graphControl.Invalidate();
    }
  }

  /// <summary>
  ///   Returns the render tag of the legend including its visual, size, position, and scale.
  /// </summary>
  private LegendRenderTag? GetRenderTag() => legendElement?.Tag as LegendRenderTag;

  /// <summary>
  ///   Returns the bounds of the legend used for hit testing in the move input mode.
  /// </summary>
  private RectD? GetLegendBounds() {
    if (GetRenderTag() is not { } tag) {
      return null;
    }
    return new RectD(tag.Position.X, tag.Position.Y, tag.Size.Width * tag.Scale, tag.Size.Height * tag.Scale);
  }

  /// <summary>
  ///   Updates the legend position and invalidates the graph control.
  /// </summary>
  private void UpdateLegendPosition(PointD position) {
    if (GetRenderTag() is { } tag && legendElement is not null) {
      legendElement.Tag = tag with {
          Position = position,
          HasCustomPosition = true
      };
      graphControl.Invalidate();
    }
  }

  /// <summary>
  ///   Updates the legend layout when the graph control size changes.
  /// </summary>
  private LegendRenderTag UpdateLegendLayout(LegendRenderTag renderTag) {
    var layout = CalculateLegendLayout(renderTag.Size);
    var position = legendPosition.SetDefaultPosition(layout.Position);
    return renderTag with {
        Position = position,
        Scale = layout.Scale
    };
  }

  private void OnGraphControlSizeChanged(object sender, SizeChangedEventArgs args) {
    if (GetRenderTag() is { } tag && legendElement is not null) {
      legendElement.Tag = UpdateLegendLayout(tag);
      graphControl.Invalidate();
    }
  }

  /// <summary>
  ///   Calculates the legend size and position considering the current orientation.
  ///   Potentially scales the legend down to fit inside the graph control.
  /// </summary>
  private (PointD Position, double Scale) CalculateLegendLayout(SizeD size) {
    var graphWidth = Math.Max(1, graphControl.ActualWidth);
    var graphHeight = Math.Max(1, graphControl.ActualHeight);
    var availableWidth = Horizontal ? graphWidth - 2 * LegendMargin : graphWidth - LegendMargin;
    var availableHeight = Horizontal ? graphHeight - LegendMargin : graphHeight - 2 * LegendMargin;
    var scale = Math.Max(MinLegendScale,
        Math.Min(1, Math.Min(availableWidth / size.Width, availableHeight / size.Height)));
    var scaledWidth = size.Width * scale;
    var scaledHeight = size.Height * scale;
    var defaultPosition = new PointD(
        Horizontal ? (graphWidth - scaledWidth) / 2 : graphWidth - scaledWidth - LegendMargin,
        Horizontal ? graphHeight - scaledHeight - LegendMargin : (graphHeight - scaledHeight) / 2);
    return (defaultPosition, scale);
  }
}
