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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.HeightProfile
{
  internal static class InitializeStyles
  {
    public static readonly SizeD LabelNodeSize = new(130, 50);

    /// <summary>
    ///   Configures the default node size and style for the waypoints.
    /// </summary>
    public static void Initialize(IGraph graph) {
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x6C, 0x00)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 1)
      };
      graph.NodeDefaults.Size = new SizeD(5, 5);
    }

    /// <summary>
    ///   Style for the edges that connect the waypoints with the associated label nodes.
    /// </summary>
    public static PolylineEdgeStyle CreateLeaderEdgeStyle() =>
        new() {
            Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 1) { DashStyle = DashStyles.Dash }
        };

    /// <summary>
    ///   Returns the icon used instead of text for the given waypoint when one is available.
    /// </summary>
    public static string? GetIcon(Waypoint value) =>
        value.Category switch {
            "yWorks" or "start" or "finish" or "picnic" or "camping" or "golf" or "observation-point" or "grill"
                or "parking" or "swimming-pool" => $"resources/icons/{value.Category}.svg",
            _ => null
        };
  }
}
