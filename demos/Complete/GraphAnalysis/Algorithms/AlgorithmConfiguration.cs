/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Analysis;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{
  /// <summary>
  ///   Base class for algorithm configurations.
  /// </summary>
  /// <remarks>
  ///   Contains code to run a specific algorithm and to display the result.
  ///   Subclasses have to implement <see cref="RunAlgorithm"/>
  ///   to both do the actual calculations and mark the result.
  /// </remarks>
  public abstract class AlgorithmConfiguration
  {
    /// <summary>
    ///   Whether the graph is considered directed.
    /// </summary>
    public bool Directed { get; set; }

    /// <summary>
    ///   Whether the algorithm supports both directed and undirected graphs.
    /// </summary>
    public virtual bool SupportsDirectedAndUndirected => false;

    /// <summary>
    ///   Whether the current algorithm supports edge weights.
    /// </summary>
    public virtual bool SupportsWeights => false;

    /// <summary>
    ///   Whether to use uniform edge weights.
    /// </summary>
    public bool UseUniformWeights { get; set; }

    public DictionaryMapper<INode, bool> IncrementalElements { get; set; }

    public bool EdgeRemoved { get; set; }

    /// <summary>
    ///   Apply the algorithm.
    /// </summary>
    public void Apply(GraphControl graphControl) {
      RunAlgorithm(graphControl.Graph);
    }

    /// <summary>
    ///   Populate the context menu specifically for the current algorithm.
    /// </summary>
    public virtual void PopulateContextMenu(PopulateItemContextMenuEventArgs<IModelItem> args) { }

    /// <summary>
    ///   Run the actual algorithm.
    /// </summary>
    public abstract void RunAlgorithm(IGraph graph);

    /// <summary>
    ///   Calculates the weight for the given edge.
    /// </summary>
    /// <remarks>
    ///   If <see cref="UseUniformWeights"/> is set to <see langword="true"/> 1 is returned.
    ///   If the edge has labels, the double value of the first label is parsed and returned.
    ///   If the label cannot be parsed into a double value, 0 is returned.
    ///   Otherwise, the length of the edge path is returned.
    /// </remarks>
    /// <param name="edge">The edge to calculate the weight for.</param>
    /// <returns>The edge weight.</returns>
    public double GetEdgeWeight(IEdge edge) {
      if (UseUniformWeights) {
        return 1;
      }

      // if edge has at least one label ...
      if (edge.Labels.Any()) {
        double edgeWeight;
        // try to return its value
        return double.TryParse(edge.Labels[0].Text, out edgeWeight) ? edgeWeight : 0;
      }

      // calculate geometric edge length
      return edge.Style.Renderer.GetPathGeometry(edge, edge.Style).GetPath().GetLength();
    }

    /// <summary>
    ///   Generates a set of colors.
    /// </summary>
    /// <param name="gradient">Whether to generate a gradient of blue colors. If <see langword="false"/>, a prepared set of colors is returned.</param>
    /// <param name="count">The number of gradient steps. Is ignored if <paramref name="gradient"/> is false.</param>
    /// <param name="lightToDark">Whether the gradient is generated from light to dark colors.</param>
    protected Color[] GenerateColors(bool gradient, int count = 0, bool lightToDark = false) {
      if (gradient) {
        var colors = new Color[count];
        float stepCount = count - 1;
        var c1 = StyleUtils.StartColor;
        var c2 = StyleUtils.EndColor;

        for (var i = 0; i < count; i++) {
          colors[i] = c1 * ((stepCount - i) / stepCount) + c2 * (i / stepCount);
        }

        if (lightToDark) {
          colors = colors.Reverse().ToArray();
        }

        return colors;
      }
      return StyleUtils.Colors;
    }

    /// <summary>
    ///   Returns the set of components which are associated with the nodes in the <see cref="IncrementalElements"/>.
    /// </summary>
    /// <param name="components">The components to get the affected components from.</param>
    /// <returns>The set of components which are associated with the nodes in the <see cref="IncrementalElements"/>.</returns>
    public ISet<ConnectedComponent> GetAffectedNodeComponents(IMapper<INode, ConnectedComponent> components) {
      var affectedComponents = new HashSet<ConnectedComponent>();

      if (IncrementalElements != null) {
        foreach (var pair in IncrementalElements.Entries) {
          if (components != null) {
            var node = pair.Key;
            if (node != null) {
              var component = components[node];
              affectedComponents.Add(component);
            }
          }
        }
      }
      return affectedComponents;
    }


    public Color DetermineElementColor(
        Color[] colors,
        ConnectedComponent component,
        ISet<ConnectedComponent> affectedComponents,
        Dictionary<ConnectedComponent, Color> color2AffectedComponent,
        ConnectedComponent largestComponent,
        ResultItemCollection<ConnectedComponent> allComponents,
        IGraph graph,
        IModelItem element) {
      var componentId = allComponents.ToList().IndexOf(component);
      if (null == IncrementalElements) {
        return colors[componentId % colors.Length];
      }
      var currentColor = ((Tag) element.Tag).CurrentColor;
      if (affectedComponents.Contains(component)) {
        if (!color2AffectedComponent.ContainsKey(component)) {
          Color l;
          if (largestComponent == component && 1 != component.InducedEdges.Count) {
            l = GenerateMajorColor(component);
          } else {
            l = largestComponent == component && 1 == component.InducedEdges.Count
                ?
                HasValidColorTag(element) ? (Color) element.Tag : GenerateUniqueColor(graph, colors)
                : element is IEdge
                    ? IncrementalElements[((IEdge) element).GetSourceNode()] &&
                    IncrementalElements[((IEdge) element).GetTargetNode()] ? GenerateUniqueColor(graph, colors) :
                    !EdgeRemoved && element.Tag is Tag && currentColor.HasValue ? currentColor.Value :
                    GenerateUniqueColor(graph, colors)
                    : !EdgeRemoved && element.Tag is Tag && currentColor.HasValue
                        ? currentColor.Value
                        : GenerateUniqueColor(graph, colors);
          }
          color2AffectedComponent[component] = l;
        }
        return color2AffectedComponent[component];
      }
      return currentColor.Value;
    }

    /// <summary>
    ///   Gets the largest component, i.e. the component with the largest number of edges and nodes.
    /// </summary>
    /// <param name="affectedComponents">The components to get the largest one from.</param>
    /// <returns>The largest component.</returns>
    protected ConnectedComponent GetLargestComponent(ISet<ConnectedComponent> affectedComponents) {
      ConnectedComponent largest = null;
      var largestCount = 0;
      foreach (var component in affectedComponents) {
        var count = component.Nodes.Count + component.InducedEdges.Count;
        if (count > largestCount) {
          largestCount = count;
          largest = component;
        }
      }
      return largest;
    }

    private Color GenerateMajorColor(ConnectedComponent component) {
      var color2Frequency = new Dictionary<Color, int>();
      // finds the colors of the nodes in the current component
      foreach (var node in component.Nodes) {
        var tag = node.Tag as Tag;
        if (tag != null && tag.CurrentColor.HasValue) {
          var color = tag.CurrentColor.Value;
          int frequency;
          if (color2Frequency.TryGetValue(color, out frequency)) {
            color2Frequency[color] = frequency + 1;
          } else {
            color2Frequency[color] = 1;
          }
        }
      }

      // finds the color with the maximum frequency
      var maxFrequency = 0;
      var colorWithMaxFrequency = Colors.White;
      foreach (var pair in color2Frequency) {
        if (maxFrequency < pair.Value) {
          maxFrequency = pair.Value;
          colorWithMaxFrequency = pair.Key;
        }
      }

      return colorWithMaxFrequency;
    }

    private Color GenerateUniqueColor(IGraph graph, Color[] colors) {
      var existingColors = new HashSet<Color>();

      foreach (var node in graph.Nodes) {
        if (HasValidColorTag(node)) {
          existingColors.Add(((Tag) node.Tag).CurrentColor.Value);
        }
      }

      foreach (var edge in graph.Edges) {
        if (HasValidColorTag(edge)) {
          existingColors.Add(((Tag) edge.Tag).CurrentColor.Value);
        }
      }

      if (existingColors.Count >= colors.Length) {
        return colors[new Random(42).Next(colors.Length)];
      }

      for (var i = 0; i < colors.Length; i++) {
        if (!existingColors.Contains(colors[i])) {
          return colors[i];
        }
      }
      return Colors.White;
    }

    private static bool HasValidColorTag(IModelItem element) {
      return element.Tag is Color;
    }

    /// <summary>
    ///   Resets the styles and tags and removes all markers.
    /// </summary>
    /// <param name="graph">The graph to reset.</param>
    public void ResetGraph(IGraph graph) {
      foreach (var node in graph.Nodes) {
        // reset size
        graph.SetNodeLayout(node, new RectD(node.Layout.GetTopLeft(), graph.NodeDefaults.Size));
        // reset style
        graph.SetStyle(node, graph.NodeDefaults.Style);
        // reset the tag
        node.Tag = new Tag();
        // remove labels
        foreach (var label in node.Labels.Where(l => Equals(l.Tag, "Centrality") || Equals(l.Tag, "ShortestPath")).ToList()) {
          graph.Remove(label);
        }
      }

      var arrow = new Arrow(ArrowType.Stealth, StyleUtils.DefaultPenColor);
      var defaultEdgeStyle = graph.EdgeDefaults.Style as AnalysisPolylineEdgeStyle;
      if (defaultEdgeStyle != null) {
        defaultEdgeStyle.TargetArrow = Directed ? arrow : Arrow.None;

        foreach (var edge in graph.Edges) {
          // reset style
          graph.SetStyle(edge, defaultEdgeStyle);
          // reset the tag
          edge.Tag = new Tag { Directed = SupportsDirectedAndUndirected && Directed };
          // remove labels
          foreach (var label in edge.Labels.Where(l => Equals(l.Tag, "Centrality")).ToList()) {
            graph.Remove(label);
          }
        }
      }
    }
  }

  /// <summary>
  ///   Custom edge style renderer which uses the edge's <see cref="ITagOwner.Tag"/> to determine how to render the edge.
  /// </summary>
  public sealed class AnalysisPolylineEdgeStyle : PathEdgeStyleBase
  {
    private Color? lastArrowColor;
    [CanBeNull]
    private Arrow lastArrow;

    public IArrow TargetArrow { get; set; } = Arrow.None;

    protected override GeneralPath GetPath([NotNull] IEdge edge) => CreatePolylinePath(edge);

    protected override Pen GetPen(IEdge edge) {
      const double thickness = 5;
      if (edge.Tag is Tag tag) {
        if (tag.CurrentColor is Color currentColor) {
          return new Pen(new SolidColorBrush(currentColor), thickness);
        }
        if (tag.GradientValue is double gradientValue) {
          return StyleUtils.GetGradientPen(gradientValue, tag.LightToDark, thickness);
        }
      }

      return StyleUtils.DefaultPen;
    }

    protected override IArrow GetTargetArrow(IEdge edge) {
      if (edge.Tag is Tag { Directed: true } tag) {
        var arrowColor = GetArrowColor(tag);
        if (arrowColor == lastArrowColor) {
          return lastArrow ?? Arrow.None;
        }
        lastArrowColor = arrowColor;
        lastArrow = new Arrow(ArrowType.Triangle, (Brush) new SolidColorBrush(arrowColor).GetAsFrozen(),
            widthScale : 2);
        return lastArrow ?? Arrow.None;
      }
      return TargetArrow;
    }

    private Color GetArrowColor(Tag tag) {
      if (tag.CurrentColor is Color currentColor) {
        return currentColor;
      }
      if (tag.GradientValue is double gradientValue) {
        return StyleUtils.GetGradientColor(gradientValue, tag.LightToDark);
      }
      return StyleUtils.DefaultPenColor;
    }
  }

  /// <summary>
  ///   Custom node style which uses a <see cref="ShapeNodeStyle"/> and the node's <see cref="ITagOwner.Tag"/> to
  ///   determine how to render the node.
  /// </summary>
  public sealed class AnalysisShapeNodeStyle : INodeStyle, INodeStyleRenderer
  {
    private readonly ShapeNodeStyle shapeNodeStyle = new() { Shape = ShapeNodeShape.Ellipse };

    public INodeStyleRenderer Renderer => this;

    private INodeStyle GetConfiguredStyle(INode node) {
      shapeNodeStyle.Brush = GetBrush(node);
      shapeNodeStyle.Pen = GetPen(node);
      return shapeNodeStyle;
    }

    private Brush GetBrush(INode node) {
      var tag = node.Tag as Tag;
      if (tag != null) {
        if (tag.IsSource || tag.IsTarget) {
          return Brushes.White;
        }
        if (tag.CurrentColor != null) {
          return new SolidColorBrush((Color) tag.CurrentColor);
        }
        if (tag.GradientValue != null) {
          return StyleUtils.GetGradientBrush((double) tag.GradientValue, tag.LightToDark);
        }
      }

      return Brushes.LightGray;
    }

    private Pen GetPen(INode node) {
      const double thickness = 5;
      var tag = node.Tag as Tag;
      if (tag != null) {
        if (tag.IsSource && tag.IsTarget) {
          var drawing = new DrawingGroup {
              Children = {
                  new GeometryDrawing(Brushes.YellowGreen, null,
                      Geometry.Parse("M 0,0 L 0.5,0 L 0.5,1 L 1,1 L 1,0.5 L 0,0.5 z")),
                  new GeometryDrawing(Brushes.IndianRed, null,
                      Geometry.Parse("M 0.5,0 L 1,0 L 1,0.5 L 0,0.5 L 0,1 L 0.5,1 z"))
              }
          };
          return new Pen(new DrawingBrush(drawing), thickness);
        }
        if (tag.IsSource) {
          return new Pen(Brushes.YellowGreen, thickness);
        }
        if (tag.IsTarget) {
          return new Pen(Brushes.IndianRed, thickness);
        }
        if (tag.CurrentColor != null) {
          return null;
        }
        if (tag.GradientValue != null) {
          return StyleUtils.GetGradientPen((double) tag.GradientValue, tag.LightToDark);
        }
      }

      return StyleUtils.DefaultPen;
    }

    public object Clone() => this;

    public IShapeGeometry GetShapeGeometry([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetShapeGeometry(node, configuredStyle);
    }

    public IVisualCreator GetVisualCreator([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetVisualCreator(node, configuredStyle);
    }

    public IBoundsProvider GetBoundsProvider([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetBoundsProvider(node, configuredStyle);
    }

    public IVisibilityTestable GetVisibilityTestable([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetVisibilityTestable(node, configuredStyle);
    }

    public IHitTestable GetHitTestable([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetHitTestable(node, configuredStyle);
    }

    public IMarqueeTestable GetMarqueeTestable([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetMarqueeTestable(node, configuredStyle);
    }

    public ILassoTestable GetLassoTestable([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetLassoTestable(node, configuredStyle);
    }

    public ILookup GetContext([NotNull] INode node, [NotNull] INodeStyle style) {
      var configuredStyle = GetConfiguredStyle(node);
      return configuredStyle.Renderer.GetContext(node, configuredStyle);
    }
  }

  /// <summary>
  ///   Used to set different properties to highlight a model item.
  /// </summary>
  /// <remarks>
  ///   Has to be set as <see cref="ITagOwner.Tag"/> of the model item to highlight.
  /// </remarks>
  public class Tag
  {
    public IEnumerable<ColorGroup> ColorGroups { get; set; }

    public double? GradientValue { get; set; }

    public bool IsSource { get; set; }

    public bool IsTarget { get; set; }

    public Color? CurrentColor { get; set; }

    public bool Directed { get; set; }

    public bool LightToDark { get; set; }
  }

  public class ColorGroup
  {
    public int Index { get; }

    public ColorGroup(int index) {
      Index = index;
    }

    public Color Color => StyleUtils.Colors[Index % StyleUtils.Colors.Length];
  }

  internal static class StyleUtils
  {
    internal static readonly Color StartColor = System.Windows.Media.Colors.LightBlue;
    internal static readonly Color EndColor = Color.FromRgb(51, 64, 247);

    internal static readonly Color[] Colors = {
        System.Windows.Media.Colors.RoyalBlue, System.Windows.Media.Colors.Gold, System.Windows.Media.Colors.Crimson,
        System.Windows.Media.Colors.DarkTurquoise, System.Windows.Media.Colors.CornflowerBlue,
        System.Windows.Media.Colors.DarkSlateBlue, System.Windows.Media.Colors.OrangeRed,
        System.Windows.Media.Colors.MediumSlateBlue, System.Windows.Media.Colors.ForestGreen,
        System.Windows.Media.Colors.MediumVioletRed, System.Windows.Media.Colors.DarkCyan,
        System.Windows.Media.Colors.Chocolate, System.Windows.Media.Colors.Orange,
        System.Windows.Media.Colors.LimeGreen, System.Windows.Media.Colors.MediumOrchid
    };

    internal static readonly Color DefaultPenColor = System.Windows.Media.Colors.DarkGray;
    internal static readonly Pen DefaultPen = new(Brushes.DarkGray, 3);

    internal static Color GetGradientColor(double gradientValue, bool lightToDark) {
      var v = (float) Math.Min(1, Math.Max(0, gradientValue));
      var c1 = StartColor;
      var c2 = EndColor;
      return lightToDark ? c1 * (1 - v) + c2 * v : c2 * (1 - v) + c1 * v;
    }

    internal static Brush GetGradientBrush(double gradientValue, bool lightToDark) {
      return new SolidColorBrush(GetGradientColor(gradientValue, lightToDark));
    }

    internal static Pen GetGradientPen(double gradientValue, bool lightToDark, double thickness = 1.0) {
      return new Pen(GetGradientBrush(gradientValue, lightToDark), thickness);
    }
  }
}
