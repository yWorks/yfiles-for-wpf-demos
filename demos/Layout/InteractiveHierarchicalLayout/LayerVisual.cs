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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.InteractiveHierarchical
{
  /// <summary>
  ///   Visualizes the layers and manages layer regions and contains tests.
  /// </summary>
  internal sealed class LayerVisual : IVisualCreator
  {
    // the bounds of the complete drawing
    private RectD bounds = RectD.Empty;

    // the list of the dividers (one less than the number of layers)
    private readonly List<double> dividers = [];

    // the opacity of the drawing
    private const double opacity = 0.5;

    /// <summary>
    ///   updates the layer drawing from the information passed in
    /// </summary>
    internal void UpdateLayers(IGraph graph, IDictionary<INode, int> layerMapper) {
      var groupingSupport = graph.GetGroupingSupport();
      var nodes = (groupingSupport.HasGroupNodes()
          ? graph.Nodes.Where(node => !graph.IsGroupNode(node) || graph.GetChildren(node).Count == 0)
          : graph.Nodes).ToArray();
      var layerCount = nodes.Length == 0 ? 0 : nodes.Max(node => layerMapper[node]) + 1;
      if (layerCount == 0) {
        bounds = RectD.Empty;
        dividers.Clear();
        return;
      }

      // calculate min and max values
      var mins = Enumerable.Repeat(double.PositiveInfinity, layerCount).ToArray();
      var maxs = Enumerable.Repeat(double.NegativeInfinity, layerCount).ToArray();
      var minX = double.PositiveInfinity;
      var maxX = double.NegativeInfinity;
      foreach (var node in nodes) {
        var layer = layerMapper[node];
        mins[layer] = Math.Min(mins[layer], Math.Floor(node.Layout.Y));
        maxs[layer] = Math.Max(maxs[layer], Math.Floor(node.Layout.GetMaxY()));
        minX = Math.Min(minX, node.Layout.X);
        maxX = Math.Max(maxX, node.Layout.GetMaxX());
      }

      // now determine divider locations
      dividers.Clear();
      for (var i = 0; i < layerCount - 1; i++) {
        dividers.Add((maxs[i] + mins[i + 1]) * 0.5);
      }

      // determine the bounds of all elements
      const double margin = 20;
      mins[0] -= margin;
      minX -= margin;
      maxX += margin;
      maxs[^1] += margin;
      bounds = new RectD(minX, mins[0], maxX - minX, maxs[^1] - mins[0]);
    }

    /// <summary>
    ///   Gets the layer at the given location.
    /// </summary>
    /// <returns>
    ///   A positive value if a specific layer is hit, a negative one to indicate that a new layer should be inserted
    ///   before layer -(value + 1) - int.MaxValue if no layer has been hit.
    /// </returns>
    internal int GetLayer(PointD location) {
      // global bounds
      var globalBounds = new RectD(bounds.X, bounds.Y - LayerInsets, bounds.Width, bounds.Height + LayerInsets * 2);
      if (location.Y < bounds.Y) {
        // before the first layer
        return -1;
      }
      if (location.Y > bounds.GetMaxY()) {
        // after the last layer
        return -(dividers.Count + 3);
      }
      // nothing found,
      if (!globalBounds.Contains(location)) {
        return int.MaxValue;
      }

      // now search the layer
      var top = bounds.Y;
      var layerCount = 0;
      for (var i = 0; i < dividers.Count; i++) {
        var divider = dividers[i];
        var layerBounds = new RectD(bounds.X, top, bounds.Width, divider - top);
        if (layerBounds.Contains(location)) {
          return GetLayerIndex(location, layerBounds, layerCount);
        }
        layerCount++;
        top = divider;
      }
      var lastLayerBounds = new RectD(bounds.X, top, bounds.Width, bounds.GetMaxY() - top);
      if (lastLayerBounds.Contains(location)) {
        return GetLayerIndex(location, lastLayerBounds, layerCount);
      }
      // should not really happen...
      return int.MaxValue;
    }

    /// <summary>
    ///   Gets the bounds of a layer by index as specified by <see cref="GetLayer"/>.
    /// </summary>
    /// <param name="layerIndex">Index of the layer.</param>
    /// <returns>The bounds of the layer.</returns>
    internal RectD GetLayerBounds(int layerIndex) {
      if (layerIndex == int.MaxValue) {
        return bounds.GetEnlarged(20);
      }
      if (layerIndex < 0) {
        // new layer
        var beforeLayer = -(layerIndex + 1);
        if (beforeLayer <= dividers.Count) {
          var layerBounds = GetLayerBounds(beforeLayer);
          return new RectD(layerBounds.X, layerBounds.Y - LayerInsets, layerBounds.Width, LayerInsets * 2);
        }

        // after last layer
        var lastLayer = GetLayerBounds(dividers.Count);
        return new RectD(lastLayer.X, lastLayer.GetMaxY() - LayerInsets, lastLayer.Width, LayerInsets * 2);
      }
      var top = layerIndex > 0 ? dividers[layerIndex - 1] : bounds.Y;
      var bottom = layerIndex < dividers.Count ? dividers[layerIndex] : bounds.GetMaxY();
      return new RectD(bounds.X, top, bounds.Width, bottom - top);
    }

    /// <summary>
    ///   Creates a new visual that emphasizes the hierarchical layers in the graph layout.
    /// </summary>
    public Visual CreateVisual(IRenderContext context) => DrawLayers();

    /// <summary>
    ///   Updates the visual that emphasizes the hierarchical layers in the graph layout.
    /// </summary>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => DrawLayers();

    private DrawingVisual DrawLayers() {
      var visual = new DrawingVisual();
      if (bounds.IsEmpty()) {
        return visual;
      }
      using var drawing = visual.RenderOpen();
      var top = bounds.Y;
      for (var i = 0; i <= dividers.Count; i++) {
        var bottom = i < dividers.Count ? dividers[i] : bounds.GetMaxY();
        var fill = (i % 2 == 1 ? LightFill : DarkFill).Clone();
        fill.Opacity = opacity;
        drawing.DrawRectangle(fill, null, new Rect(bounds.X, top, bounds.Width, bottom - top));
        top = bottom;
      }
      return visual;
    }

    /// <summary>
    ///   checks a layer and determines if the layer has been clicked near the border
    /// </summary>
    private static int GetLayerIndex(PointD location, RectD layerBounds, int layerIndex) {
      // check if close to top or bottom
      if (location.Y - layerBounds.Y < LayerInsets) {
        // before current layer
        return -(layerIndex + 1);
      }
      if (layerBounds.GetMaxY() - location.Y < LayerInsets) {
        // after current layer
        return -(layerIndex + 2);
      }
      // in current layer
      return layerIndex;
    }

    /// <summary>
    ///   the dark fill used for drawing the layers
    /// </summary>
    private static readonly SolidColorBrush DarkFill = BrushFrom("#FFC914");

    /// <summary>
    ///   the light fill used for drawing the layers
    /// </summary>
    private static readonly SolidColorBrush LightFill = BrushFrom("#FFE8A0");

    /// <summary>
    ///   the insets for each layer
    /// </summary>
    private const double LayerInsets = 10;

    private static SolidColorBrush BrushFrom(string color) {
      var brush = (SolidColorBrush) new BrushConverter().ConvertFromString(color)!;
      brush.Freeze();
      return brush;
    }
  }
}
