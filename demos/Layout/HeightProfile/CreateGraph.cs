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
using System.Globalization;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.HeightProfile
{
  internal static class CreateGraph
  {
    /// <summary>
    ///   Creates the graph from the trekking dataset.
    ///   For each data item, a waypoint node and an associated label node are created.
    /// </summary>
    public static void Initialize(IGraph graph, TrekkingData data, IReadOnlyDictionary<string, ImageSource> icons) {
      // use the default node size as size for waypoint nodes
      var width = graph.NodeDefaults.Size.Width;
      var height = graph.NodeDefaults.Size.Height;

      // get the min/max values from the dataset to be able to scale the data
      var (maxX, maxY) = ScaleData.GetMax(data.Trail);
      var leaderEdgeStyle = InitializeStyles.CreateLeaderEdgeStyle();

      foreach (var waypointData in data.Waypoints) {
        var xPos = ScaleData.ScalePoint(waypointData.X, maxX, ScaleData.ScaledMaxX);
        var yPos = -ScaleData.ScalePoint(waypointData.Y, maxY, ScaleData.ScaledMaxY);

        var waypoint = graph.CreateNode(new RectD(xPos - width * 0.5, yPos - height * 0.5, width, height),
            tag: waypointData with { Type = "waypoint" });

        // create the label node and define its tag based on the tag of the associated waypoint
        var labelNode = graph.CreateNode(
            new RectD(0, 0, InitializeStyles.LabelNodeSize.Width, InitializeStyles.LabelNodeSize.Height),
            tag: waypointData with { Type = "label" });

        // set the style of the label node and ...
        graph.SetStyle(labelNode, new ShapeNodeStyle {
            Shape = ShapeNodeShape.RoundRectangle,
            Brush = new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
            Pen = null
        });
        graph.AddLabel(labelNode, $"{waypointData.Y.ToString(CultureInfo.InvariantCulture)} m",
            InteriorNodeLabelModel.Bottom);
        var icon = InitializeStyles.GetIcon(waypointData);
        if (icon is not null && icons.TryGetValue(icon, out var iconImage)) {
          var maxIconSize = waypointData.Category == "yWorks"
              ? new SizeD(InitializeStyles.LabelNodeSize.Width * 0.6, InitializeStyles.LabelNodeSize.Height * 0.6)
              : new SizeD(InitializeStyles.LabelNodeSize.Height * 0.6, InitializeStyles.LabelNodeSize.Height * 0.6);
          var iconScale = Math.Min(maxIconSize.Width / iconImage.Width, maxIconSize.Height / iconImage.Height);
          var iconSize = new SizeD(iconImage.Width * iconScale, iconImage.Height * iconScale);
          var iconLabelStyle = new IconLabelStyle {
              Icon = iconImage,
              IconPlacement = InteriorNodeLabelModel.Top,
              IconSize = iconSize,
              WrappedStyle = new LabelStyle {
                  Typeface = new Typeface("sans-serif"),
                  TextSize = 16,
                  TextBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x66, 0x99))
              }
          };
          graph.AddLabel(labelNode, string.Empty, InteriorNodeLabelModel.Top, iconLabelStyle);
        } else {
          graph.AddLabel(labelNode, waypointData.Name ?? string.Empty, InteriorNodeLabelModel.Top,
              new LabelStyle {
                  Typeface = new Typeface("sans-serif"),
                  TextSize = 16,
                  TextBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x66, 0x99))
              });
        }

        // ... create an edge between the waypoint and the label node
        graph.CreateEdge(waypoint, labelNode, leaderEdgeStyle);
      }
    }
  }
}
