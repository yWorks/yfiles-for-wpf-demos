/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Layout.IncrementalHierarchicLayout
{
  /// <summary>
  /// Manages and renders the layers
  /// </summary>
  public class LayerVisualCreator : IVisualCreator
  {

    // the bounds of the complete drawing
    private Rect bounds;
    // the list of the dividers (one less than the number of layers)
    private readonly List<float> dividers = new List<float>();
    // the dark brush used for drawing the layers
    private static readonly Brush darkBrush = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xC9, 0x14));
    // the light brush used for drawing the layers
    private static readonly Brush lightBrush = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xE8, 0xA0));

    // updates the layer drawing from the information passed in
    public void UpdateLayers(IGraph graph, IMapper<INode, int> layerMapper) {
      // count the layers
      IEnumerable<INode> nodes;

      if (graph.GetGroupingSupport().HasGroupNodes()) {
        nodes = graph.Nodes.Where(node => graph.GetChildren(node).Count == 0);
      } else {
        nodes = graph.Nodes;
      }

      int layerCount = nodes.Max(node => layerMapper[node]) + 1;

      // calculate min and max values
      int[] mins = new int[layerCount];
      int[] maxs = new int[layerCount];
      for (int i = 0; i < maxs.Length; i++) {
        maxs[i] = Int32.MinValue;
      }

      for (int i = 0; i < mins.Length; i++) {
        mins[i] = Int32.MaxValue;
      }

      double minX = Double.PositiveInfinity;
      double maxX = Double.NegativeInfinity;
      foreach (var node in nodes) {
        mins[layerMapper[node]] = Math.Min(mins[layerMapper[node]], (int)node.Layout.Y);
        maxs[layerMapper[node]] = Math.Max(maxs[layerMapper[node]], (int)node.Layout.GetMaxY());
        minX = Math.Min(minX, node.Layout.X);
        maxX = Math.Max(maxX, node.Layout.GetMaxX());
      }

      // now determine divider locations
      dividers.Clear();
      dividers.Capacity = Math.Max(mins.Length, dividers.Capacity);
      for (int i = 0; i < maxs.Length - 1; i++) {
        dividers.Add((maxs[i] + mins[i + 1]) * 0.5f);
      }

      // determine the bounds of all elements
      const int margin = 10;
      mins[0] -= margin;
      minX -= margin;
      maxX += margin;
      maxs[maxs.Length - 1] += margin;
      if (nodes.GetEnumerator().MoveNext()) {
        mins[0] -= margin;
        minX -= margin;
        maxX += margin;
        maxs[maxs.Length - 1] += margin;
        bounds = new Rect((float)minX, mins[0], (float)(maxX - minX), maxs[maxs.Length - 1] - mins[0]);
      } else {
        bounds = new Rect();
      }
    }

    /// <summary>
    /// Gets the layer at the given location.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns>A positive value if a specific layer is hit, a negative one to indicate that a new layer should
    /// be inserted before layer -(value + 1) - int.MaxValue if no layer has been hit.</returns>
    public int GetLayer(PointD location) {
      // global bounds
      var nbounds = new RectD(bounds.X, bounds.Y - LayerInsets, bounds.Width, bounds.Height + LayerInsets * 2);
      if (location.Y < bounds.Y) {
        // before the first layer
        return -1;
      }
      if (location.Y > bounds.Bottom) {
        // after the last layer
        return -((dividers.Count + 2) + 1);
      }
      // nothing found, 
      if (!nbounds.Contains(location)) {
        return Int32.MaxValue;
      }

      // now search the layer
      double top = bounds.Top;

      int layerCount = 0;
      foreach (var divider in dividers) {
        var layerBounds = new RectD(bounds.X, top, bounds.Width, divider - top);
        if (layerBounds.Contains(location)) {
          return GetLayerIndex(location, layerBounds, layerCount);
        }
        layerCount++;
        top = divider;
      }
      {
        var layerBounds = new RectD(bounds.X, top, bounds.Width, bounds.Bottom - top);
        if (layerBounds.Contains(location)) {
          return GetLayerIndex(location, layerBounds, layerCount);
        }
      }
      // should not really happen...
      return Int32.MaxValue;
    }

    const int LayerInsets = 10;

    // checks a layer and determines if the layer has been clicked near the border
    private int GetLayerIndex(PointD location, RectD layerBounds, int layerIndex) {
      // check if close to top or bottom
      if (location.Y - layerBounds.MinY < LayerInsets) {
        // before current layer
        return -(layerIndex + 1);
      }
      if (layerBounds.MaxY - location.Y < LayerInsets) {
        // after current layer
        return -(layerIndex + 2);
      }
      // in current layer
      return layerIndex;
    }

    /// <summary>
    /// Gets the bounds of a layer by index as specified by <see cref="GetLayer"/>.
    /// </summary>
    /// <param name="layerIndex">Index of the layer.</param>
    /// <returns>The bounds of the layer</returns>
    public RectD GetLayerBounds(int layerIndex) {
      if (layerIndex == Int32.MaxValue) {
        return RectD.Infinite;
      }
      if (layerIndex < 0) {
        // new layer
        int beforeLayer = -(layerIndex + 1);
        if (beforeLayer <= dividers.Count) {
          RectD layerBounds = GetLayerBounds(beforeLayer);
          return new RectD(layerBounds.X, layerBounds.Y - LayerInsets, layerBounds.Width, LayerInsets * 2);
        } else {
          // after last layer
          RectD layerBounds = GetLayerBounds(dividers.Count);
          return new RectD(layerBounds.X, layerBounds.MaxY - LayerInsets, layerBounds.Width, LayerInsets * 2);
        }
      } else {
        var top = layerIndex > 0 ? dividers[layerIndex - 1] : bounds.Top;
        var bottom = layerIndex < dividers.Count ? dividers[layerIndex] : bounds.Bottom;
        return new RectD(bounds.X, top, bounds.Width, bottom - top);
      }
    }

    public Visual CreateVisual(IRenderContext context) {
      return UpdateVisual(context, new VisualGroup());
    }

    public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      VisualGroup cc = oldVisual as VisualGroup;
      if (cc == null) {
        cc = new VisualGroup();
      }

      var y = bounds.Top;
      int count = 0;
      foreach (var divider in dividers) {
        Rectangle rectangle;
        if (cc.Children.Count <= count) {
          cc.Children.Add(rectangle = new Rectangle());
        } else {
          rectangle = (Rectangle) cc.Children[count];
        }
        float bottom = divider;
        rectangle.SetCanvasArrangeRect(new Rect(bounds.Left, y, bounds.Width, bottom - y));
        rectangle.Fill = count%2 == 1 ? lightBrush : darkBrush;
        y = bottom;
        count++;
      }

      {
        Rectangle rectangle;
        if (cc.Children.Count <= count) {
          cc.Children.Add(rectangle = new Rectangle());
        } else {
          rectangle = (Rectangle)cc.Children[count];
        }
        rectangle.SetCanvasArrangeRect(new Rect(bounds.Left, y, bounds.Width, bounds.Bottom - y));
        rectangle.Fill = count % 2 == 1 ? lightBrush : darkBrush;
        count++;
      }

      while (cc.Children.Count > count) {
        cc.Children.RemoveAt(cc.Children.Count - 1);
      }
      return cc;
    }
  }
}
