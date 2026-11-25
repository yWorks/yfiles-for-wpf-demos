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
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using static yWorks.Demo.GraphAnalysis.DemoTypes;

namespace yWorks.Demo.GraphAnalysis
{
  public sealed partial class GraphAnalysisDemo
  {
    /// <summary>
    ///   An extended node which chooses the colors of the node's
    ///   stroke and fill depending on the information in the tag and the analysis results.
    /// </summary>
    private sealed class TagColoredShapeNodeStyle : DelegatingNodeStyle
    {
      private readonly GraphAnalysisDemo demo;
      private readonly ShapeNodeStyle delegatingStyle = new() { Shape = ShapeNodeShape.Ellipse };

      public TagColoredShapeNodeStyle(GraphAnalysisDemo demo) {
        this.demo = demo;
      }

      protected override INodeStyle GetStyle(INode node) {
        delegatingStyle.Brush = GetFill(node);
        delegatingStyle.Pen = GetStroke(node);
        return delegatingStyle;
      }

      private Brush GetFill(INode node) {
        var tag = GetTag(node);
        if (tag?.Type is NodeType.Start or NodeType.End) {
          return Brushes.White;
        }

        var componentColor = demo.GetDisplayedComponentColor(node);
        return componentColor ?? Brushes.LightGray;
      }

      private Pen? GetStroke(INode node) {
        var tag = GetTag(node);
        var highlighted = demo.IsHighlighted(node);
        var highlightedStrokeWidth = highlighted ? 6 : 4;

        // first check if the node is a start or end node for the algorithm
        if (tag?.Type == NodeType.Start) {
          return GetPen(Brushes.YellowGreen, highlightedStrokeWidth);
        }

        if (tag?.Type == NodeType.End) {
          return GetPen(Brushes.IndianRed, highlightedStrokeWidth);
        }

        var componentColor = demo.GetDisplayedComponentColor(node);
        if (componentColor == null) {
          // if the node doesn't belong to a component, render it grey
          return GetPen(Brushes.DarkGray, 3);
        }

        return highlighted ? GetPen(componentColor, highlightedStrokeWidth) : null;
      }
    }

    /// <summary>
    ///   An extended <see cref="PolylineEdgeStyle"/> which chooses the color and width of the edge's
    ///   stroke depending on the analysis results.
    /// </summary>
    private sealed class TagColoredPolylineEdgeStyle : DelegatingEdgeStyle
    {
      private static readonly Dictionary<(Brush Color, int StrokeWidth), IArrow> arrowCache = new();
      private readonly GraphAnalysisDemo demo;
      private readonly PolylineEdgeStyle delegatingStyle = new();

      public TagColoredPolylineEdgeStyle(GraphAnalysisDemo demo) {
        this.demo = demo;
      }

      protected override IEdgeStyle GetStyle(IEdge edge) {
        delegatingStyle.Pen = GetStroke(edge);
        delegatingStyle.TargetArrow = GetTargetArrow(edge);
        return delegatingStyle;
      }

      private Pen GetStroke(IEdge edge) {
        var componentColor = demo.GetDisplayedComponentColor(edge);
        if (componentColor == null) {
          // if the edge doesn't belong to a component, render it grey
          return GetPen(Brushes.DarkGray, 3);
        }

        return GetPen(componentColor, demo.IsHighlighted(edge) ? 8 : 5);
      }

      private IArrow GetTargetArrow(IEdge edge) {
        var directed = demo.UseDirectedEdges();
        if (!directed) {
          return Arrow.None;
        }

        var highlighted = demo.IsHighlighted(edge);
        var componentColor = demo.GetDisplayedComponentColor(edge) ?? Brushes.DarkGray;
        var strokeWidth = highlighted ? 5 : 1;
        return GetArrow(componentColor, strokeWidth);
      }

      /// <summary>
      ///   Returns an arrow with the given color and stroke width. Either returns
      ///   a cached version if such an arrow already exists or creates and caches
      ///   a new instance.
      /// </summary>
      private static IArrow GetArrow(Brush color, int strokeWidth) {
        var arrowMapKey = (color, strokeWidth);
        if (!arrowCache.TryGetValue(arrowMapKey, out var arrow)) {
          arrow = new Arrow(ArrowType.Triangle, color, GetPen(color, strokeWidth), widthScale: 1.5);
          arrowCache[arrowMapKey] = arrow;
        }

        return arrow;
      }
    }

    /// <summary>
    ///   Checks whether the given item is part of a highlighted component.
    /// </summary>
    private bool IsHighlighted(IModelItem item) =>
        highlightedComponent is { } component && result != null &&
        result.Components.TryGetValue(item, out var components) && components.Contains(component);

    /// <summary>
    ///   Returns the color for the displayed component of the given item.
    /// </summary>
    private Brush? GetDisplayedComponentColor(IModelItem item) {
      if (result == null) {
        return null;
      }

      if (item is INode node && result.Centrality.TryGetValue(node, out var centrality)) {
        return GradientColor(centrality);
      }

      if (result.Gradient.TryGetValue(item, out var gradient)) {
        return GradientColor(gradient);
      }

      if (result.Components.TryGetValue(item, out var ids) && ids.Count > 0) {
        return colors[
            (highlightedComponent is { } component && ids.Contains(component) ? component : ids[0]) % colors.Length];
      }

      return null;
    }

    /// <summary>
    ///   A set of colors that can be applied to components of nodes and edges.
    /// </summary>
    private readonly Brush[] colors = [
        .. new[] {
            "#4281a4",
            "#ff6c00",
            "#56926e",
            "#db3a34",
            "#242265",
            "#f0c808",
            "#6c4f77",
            "#e0e04f",
            "#2d4d3a",
            "#6dbc8d",
            "#ffc914",
            "#76b041",
            "#ff6c00",
            "#17bebb",
            "#76b041"
        }.Select(value => (Brush) new BrushConverter().ConvertFromString(value)!)
    ];

    private static readonly Dictionary<Color, Brush> gradientBrushCache = new();
    private static readonly Dictionary<(Brush Color, double Width), Pen> penCache = new();
    private int? highlightedComponent;

    private static readonly LabelStyle CentralityEdgeLabelStyle = new() {
        TextSize = 10,
        TextBrush = Brushes.Black,
        BackgroundBrush = Brushes.White,
        BackgroundPen = new Pen((Brush) new BrushConverter().ConvertFromString("#17bebb")!, 2),
        Padding = new InsetsD(5, 3, 5, 3),
        AutoFlip = false
    };

    /// <summary>
    ///   Highlights all nodes and edges belonging to a particular component.
    /// </summary>
    private void OnHighlightComponent(int? component) {
      highlightedComponent = component;
      graphControl.Invalidate();
    }

    private static Pen GetPen(Brush color, double width) {
      var key = (color, width);
      if (!penCache.TryGetValue(key, out var pen)) {
        pen = new Pen(color, width);
        pen.Freeze();
        penCache[key] = pen;
      }

      return pen;
    }

    // Blend the source demo's dark blue and turquoise gradient endpoints.
    private static Brush GradientColor(double value) {
      var t = double.IsFinite(value) ? Math.Clamp(value, 0, 1) : 0;
      var color = Color.FromRgb((byte) Math.Round(36 - 13 * t), (byte) Math.Round(34 + 156 * t),
          (byte) Math.Round(101 + 86 * t));
      if (!gradientBrushCache.TryGetValue(color, out var brush)) {
        brush = new SolidColorBrush(color);
        brush.Freeze();
        gradientBrushCache[color] = brush;
      }

      return brush;
    }
  }
}
