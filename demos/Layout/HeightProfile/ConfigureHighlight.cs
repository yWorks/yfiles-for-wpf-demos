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
using yWorks.Graph.Styles;

namespace yWorks.Demo.HeightProfile
{
  internal static class ConfigureHighlight
  {
    /// <summary>
    ///   Configures the highlighting style for a hovered node and adds its description icon if it has one.
    /// </summary>
    public static void Initialize(GraphControl graphControl) {
      // defines the highlight style for the nodes based on their type
      graphControl.Graph.GetDecorator().Nodes.HighlightRenderer.AddFactory(node => new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              Brush = Brushes.Transparent,
              Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 1),
              Shape = WaypointData.GetWaypoint(node)?.Type == "label"
                  ? ShapeNodeShape.RoundRectangle
                  : ShapeNodeShape.Ellipse
          },
          Margins = new InsetsD(2),
          ZoomPolicy = StyleIndicatorZoomPolicy.Mixed
      });

      // configure hover input mode to highlight hovered nodes and to show the icon associated with
      // the hovered node
      var inputMode = graphControl.InputMode as GraphViewerInputMode ??
          throw new InvalidOperationException("The graph control requires a GraphViewerInputMode.");
      inputMode.ItemHoverInputMode.Enabled = true;
      inputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Node;
      inputMode.ItemHoverInputMode.HoveredItemChanged += (_, evt) => {
        var hoveredItem = evt.Item;
        // clear previous highlights and remove the description icon if there was one
        var highlights = graphControl.Highlights;
        highlights.Clear();
        DrawIconDescription.Remove(graphControl);

        if (hoveredItem is INode node) {
          // highlight the node and show its description icon if there is one
          highlights.Add(node);
          DrawIconDescription.Add(graphControl, node);
        }
      };
    }
  }
}
