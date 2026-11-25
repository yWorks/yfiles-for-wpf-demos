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

using System.Collections.Generic;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.Legend;

/// <summary>
///   Legend built from a UIElement export of a graph and added to the render tree of the graph control.
/// </summary>
internal sealed class RenderTreeWorldLegend : LegendBase
{
  private IRenderTreeElement? legendElement;
  private readonly RenderTreeLegendPosition legendPosition;

  public RenderTreeWorldLegend(GraphControl graphControl, GraphViewerInputMode inputMode) : base(graphControl) {
    // create a custom position handler used for dragging the legend
    legendPosition = new RenderTreeLegendPosition(UpdateLegendPosition);
    // add a custom legend move input mode to the main input mode
    inputMode.Add(new RenderTreeLegendMoveInputMode(graphControl, GetLegendBounds, () => legendPosition.Position,
        legendPosition.SetPosition, false));
  }

  protected override void OnHide() {
    if (legendElement is not null) {
      graphControl.RenderTree.Remove(legendElement);
      graphControl.UpdateContentBounds();
      legendElement = null;
    }
  }

  protected override void RefreshCore(IReadOnlyList<NodeTypeId> visibleTypes) {
    if (legendElement is not null) {
      // remove the old legend from the render tree and reset the content bounds
      graphControl.RenderTree.Remove(legendElement);
      graphControl.UpdateContentBounds();
    }

    // calculate the position of the legend (top right or bottom left)
    var contentBounds = graphControl.ContentBounds;
    var legendAnchor = Horizontal ? contentBounds.GetBottomLeft() : contentBounds.GetTopRight();
    const double legendOffset = 60;
    var position = legendPosition.SetDefaultPosition(new PointD(
        Horizontal ? legendAnchor.X : legendAnchor.X + legendOffset,
        Horizontal ? legendAnchor.Y + legendOffset : legendAnchor.Y));

    // create the WPF visual
    var (legendVisual, size) = CreateLegendVisual(visibleTypes);
    var renderTag = new LegendRenderTag(legendVisual, size, position, 1, false, legendPosition.IsManualPosition);

    // Add the visual to the content group where the graph lives too. This way its bounds are
    // considered when fitting the graph bounds.
    legendElement = graphControl.RenderTree.CreateElement(graphControl.RenderTree.ContentGroup, renderTag,
        new RenderTreeLegendRenderer());
    // Update to include the legend visual in the content bounds
    graphControl.UpdateContentBounds();
  }

  /// <summary>
  ///   Returns the bounds of the legend used for hit testing in the move input mode.
  /// </summary>
  private RectD? GetLegendBounds() {
    if (legendElement?.Tag is LegendRenderTag tag) {
      return new RectD(tag.Position.X, tag.Position.Y, tag.Size.Width, tag.Size.Height);
    }
    return null;
  }

  /// <summary>
  ///   Updates the legend position and invalidates the graph control.
  /// </summary>
  private void UpdateLegendPosition(PointD position) {
    if (legendElement?.Tag is LegendRenderTag renderTag) {
      legendElement.Tag = renderTag with {
          Position = position,
          HasCustomPosition = true
      };
      graphControl.Invalidate();
    }
  }
}
