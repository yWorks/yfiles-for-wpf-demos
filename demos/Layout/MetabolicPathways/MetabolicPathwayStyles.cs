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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.MetabolicPathways
{
  /// <summary>
  ///   Provides the styles used for the nodes, edges, and labels of the graph.
  /// </summary>
  internal static class MetabolicPathwayStyles
  {
    /// <summary>
    ///   A mapping between node types and colors.
    /// </summary>
    private static readonly IReadOnlyDictionary<MetabolicPathwayData.NodeType, Palette> Palettes =
        new Dictionary<MetabolicPathwayData.NodeType, Palette> {
            [MetabolicPathwayData.NodeType.Product] = Themes.Palette54,
            [MetabolicPathwayData.NodeType.Reactant] = Themes.Palette56,
            [MetabolicPathwayData.NodeType.Enzyme] = Themes.Palette59,
            [MetabolicPathwayData.NodeType.Reaction] = new("#0b7189", "black", "#9dc6d0", "#9f9ea9", "#0e0e28"),
            [MetabolicPathwayData.NodeType.CoReactant] = Themes.Palette72,
            [MetabolicPathwayData.NodeType.Other] = Themes.Palette510
        };

    private static readonly Brush EdgeBrush = CreateBrush("#0b7189");

    private static readonly LabelStyle LabelStyle = new() {
        Shape = LabelShape.RoundRectangle,
        Padding = new InsetsD(1),
        VerticalTextAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Center,
        TextSize = 14,
        Typeface =
            new Typeface(new FontFamily("sans-serif"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
        TextWrapping = TextWrapping.NoWrap,
        BackgroundBrush = CreateBrush("Transparent"),
        BackgroundPen = null
    };

    /// <summary>
    ///   Initializes the default styles for the nodes and edges of the graph.
    /// </summary>
    internal static void InitializeDefaultStyles(IGraph graph) {
      graph.NodeDefaults.Size = new SizeD(190, 60);
      graph.EdgeDefaults.Style = CreateArrowStyle(ArrowStyleShape.Arrow, 2);
      graph.NodeDefaults.Labels.Style = LabelStyle;
    }

    /// <summary>
    ///   Updates the node and edge styles based on the node types.
    /// </summary>
    internal static void UpdateStyles(IGraph graph) {
      foreach (var node in graph.Nodes) {
        var type = MetabolicPathwayData.GetType(node);
        var palette = Palettes[type];
        INodeStyle style = type != MetabolicPathwayData.NodeType.Reaction
            ? new RectangleNodeStyle {
                Brush = palette.NodeLabelFill,
                Pen = new Pen(palette.Fill, 2)
            }
            : new ShapeNodeStyle {
                Shape = ShapeNodeShape.Ellipse,
                Brush = palette.NodeLabelFill,
                Pen = new Pen(EdgeBrush, 2)
            };
        graph.SetStyle(node, style);

        if (node.Labels.Count > 0) {
          var label = node.Labels.First();
          graph.SetStyle(label, LabelStyle);
          if (type is MetabolicPathwayData.NodeType.CoReactant or MetabolicPathwayData.NodeType.Other) {
            graph.SetLabelLayoutParameter(label, ExteriorNodeLabelModel.Top);
          }
        }

        if (type == MetabolicPathwayData.NodeType.Reaction) {
          graph.SetNodeLayout(node, new RectD(0, 0, 40, 40));
        } else if (type is MetabolicPathwayData.NodeType.CoReactant or MetabolicPathwayData.NodeType.Other) {
          graph.SetNodeLayout(node, new RectD(0, 0, 20, 20));
        }
      }

      var visited = new HashSet<IEdge>();
      foreach (var edge in graph.Edges.ToArray()) {
        if (!visited.Add(edge)) {
          continue;
        }

        var source = edge.GetSourceNode();
        var target = edge.GetTargetNode();
        var sourceType = MetabolicPathwayData.GetType(source);
        var targetType = MetabolicPathwayData.GetType(target);
        var parallelEdge = graph.GetEdge(target, source);
        if (parallelEdge != null && visited.Add(parallelEdge)) {
          graph.Remove(parallelEdge);
          graph.SetStyle(edge, CreateArrowStyle(ArrowStyleShape.DoubleArrow, 2));
        }

        if (sourceType == MetabolicPathwayData.NodeType.CoReactant ||
            targetType == MetabolicPathwayData.NodeType.CoReactant) {
          graph.SetStyle(edge, GetArcEdgeStyle(graph, edge));
        }
      }
    }

    /// <summary>
    ///   Returns an arc-edge style for edges attached to co-reactants by calculating the appropriate edge height.
    /// </summary>
    internal static IEdgeStyle GetArcEdgeStyle(IGraph graph, IEdge edge) {
      var hasArrow = MetabolicPathwayData.GetType(edge.GetSourceNode()) != MetabolicPathwayData.NodeType.CoReactant;
      return new ArcEdgeStyle {
          Height = GetArcHeight(graph, edge),
          FixedHeight = true,
          Pen = new Pen(EdgeBrush, 3) { DashStyle = DashStyles.Dash },
          TargetArrow =
              hasArrow ? new Arrow(brush: EdgeBrush, type: ArrowType.Triangle, pen: new Pen(EdgeBrush, 3)) : Arrow.None
      };
    }

    /// <summary>
    ///   Calculates the height for the arc edge based on the position of its source/target.
    /// </summary>
    private static double GetArcHeight(IGraph graph, IEdge edge) {
      var source = edge.GetSourceNode();
      var target = edge.GetTargetNode();
      var reaction = MetabolicPathwayData.GetType(source) == MetabolicPathwayData.NodeType.Reaction ? source : target;
      var coReactantCount = graph.Neighbors(reaction).Count(neighbor =>
          MetabolicPathwayData.GetType(neighbor) == MetabolicPathwayData.NodeType.CoReactant &&
          neighbor.Layout.X > reaction.Layout.X);
      return coReactantCount > 1 ? -40 : 40;
    }

    /// <summary>
    ///   Converts the HTML labels to readable plain text and keeps explicit line breaks.
    /// </summary>
    internal static string FormatLabel(string text) {
      var withBreaks = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
      var withoutMarkup = Regex.Replace(withBreaks, "<[^>]+>", "");
      return WebUtility.HtmlDecode(withoutMarkup);
    }

    private static ArrowEdgeStyle CreateArrowStyle(ArrowStyleShape shape, double thickness) =>
        new() {
            Shape = shape,
            Thickness = thickness,
            Pen = new Pen(EdgeBrush, thickness),
            Brush = EdgeBrush
        };

    private static Brush CreateBrush(string color) =>
        new SolidColorBrush((Color) ColorConverter.ConvertFromString(color));
  }
}
