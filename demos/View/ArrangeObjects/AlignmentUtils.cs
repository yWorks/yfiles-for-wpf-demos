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
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.ArrangeObjects
{
  /*
   * Provides utility functions for aligning nodes, i.e.
   *   alignTop               - nodes will get the same y-coordinate
   *   alignLeft              - nodes will get the same x-coordinate
   *   alignBottom            - nodes will get the same maximum y-coordinate
   *   alignRight             - nodes will get the same maximum x-coordinate
   *   alignHorizontally      - node centers will get the same x-coordinate
   *   alignVertically        - node centers will get the same y-coordinate and distributing nodes, i.e.
   *   distributeHorizontally - nodes will be distributed along x-axis
   *   distributeVertically   - nodes will be distributed along y-axis
   *
   * For simplicity's sake, group nodes are currently not supported.
   */
  internal static class AlignmentUtils
  {
    private enum Coordinate
    {
      X,
      Y
    }

    /// <summary>
    ///   Aligns the given nodes such that all nodes have the same y-coordinate.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    public static void AlignTop(IGraph graph, IEnumerable<INode> nodes) {
      if (HasAtLeastTwoNodes(nodes)) {
        AlignMinCoordImpl(graph, nodes, Coordinate.Y);
      }
    }

    /// <summary>
    ///   Aligns the given nodes such that all nodes have the same x-coordinate.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    public static void AlignLeft(IGraph graph, IEnumerable<INode> nodes) {
      if (HasAtLeastTwoNodes(nodes)) {
        AlignMinCoordImpl(graph, nodes, Coordinate.X);
      }
    }

    /// <summary>
    ///   Aligns the given nodes such that y + height is the same value for all nodes.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    public static void AlignBottom(IGraph graph, IEnumerable<INode> nodes) {
      if (HasAtLeastTwoNodes(nodes)) {
        AlignMaxCoordImpl(graph, nodes, Coordinate.Y);
      }
    }

    /// <summary>
    ///   Aligns the given nodes such that x + width is the same value for all nodes.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    public static void AlignRight(IGraph graph, IEnumerable<INode> nodes) {
      if (HasAtLeastTwoNodes(nodes)) {
        AlignMaxCoordImpl(graph, nodes, Coordinate.X);
      }
    }

    /// <summary>
    ///   Aligns the given nodes such that the centers of the nodes have the same x-coordinate.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    public static void AlignHorizontally(IGraph graph, IEnumerable<INode> nodes) {
      if (HasAtLeastTwoNodes(nodes)) {
        AlignCenterCoordImpl(graph, nodes, Coordinate.X);
      }
    }

    /// <summary>
    ///   Aligns the given nodes such that the centers of the nodes have the same y-coordinate.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    public static void AlignVertically(IGraph graph, IEnumerable<INode> nodes) {
      if (HasAtLeastTwoNodes(nodes)) {
        AlignCenterCoordImpl(graph, nodes, Coordinate.Y);
      }
    }

    /// <summary>
    ///   Aligns the given nodes by the given coordinate property.
    ///   Used for top and left aligning nodes.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    /// <param name="coordinate">The coordinate property to use for positioning the nodes.</param>
    private static void AlignMinCoordImpl(IGraph graph, IEnumerable<INode> nodes, Coordinate coordinate) {
      if (!nodes.Any()) {
        return;
      }

      var edit = graph.BeginEdit("Align Nodes", "Align Nodes");

      var min = nodes.Min(node => GetCoordinate(node.Layout, coordinate));
      foreach (var node in nodes) {
        UpdateLayout(graph, node, coordinate, min);
      }

      edit.Commit();
    }

    /// <summary>
    ///   Aligns the given nodes by the given coordinate and size properties.
    ///   Used for bottom and right aligning nodes.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    /// <param name="coordinate">The coordinate property to use for positioning the nodes.</param>
    private static void AlignMaxCoordImpl(IGraph graph, IEnumerable<INode> nodes, Coordinate coordinate) {
      if (!nodes.Any()) {
        return;
      }

      var edit = graph.BeginEdit("Align Nodes", "Align Nodes");

      var max = nodes.Max(node => GetCoordinate(node.Layout, coordinate) + GetSize(node.Layout, coordinate));
      // Technically, the initial value should be
      //   nodes.First().Layout.X + nodes.First().Layout.Width (or the corresponding y and height)
      // but since the reduction calculates a maximum and nodes must have non-negative width and height,
      // it does not matter for the result.
      foreach (var node in nodes) {
        UpdateLayout(graph, node, coordinate, max - GetSize(node.Layout, coordinate));
      }

      edit.Commit();
    }

    /// <summary>
    ///   Aligns the given nodes by the center of the given coordinate and size properties.
    ///   Used for vertical and horizontal aligning nodes.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    /// <param name="coordinate">The coordinate property to use for positioning the nodes.</param>
    private static void AlignCenterCoordImpl(IGraph graph, IEnumerable<INode> nodes, Coordinate coordinate) {
      if (!nodes.Any()) {
        return;
      }

      var edit = graph.BeginEdit("Align Nodes", "Align Nodes");

      var largestNode = nodes.MaxBy(node => GetSize(node.Layout, coordinate))!;
      var center = GetCenter(largestNode.Layout, coordinate);
      foreach (var node in nodes) {
        UpdateLayout(graph, node, coordinate, center - GetSize(node.Layout, coordinate) * 0.5);
      }

      edit.Commit();
    }

    /// <summary>
    ///   Distributes the given nodes along the x-axis. I.e. the given nodes are repositioned such that
    ///   they do not overlap horizontally. (Vertical overlaps are not removed, though.)
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    /// <param name="minimumSpacing">The minimum distance between distributed nodes.</param>
    public static void DistributeHorizontally(IGraph graph, IEnumerable<INode> nodes, double minimumSpacing = 0) {
      if (HasAtLeastTwoNodes(nodes)) {
        DistributeImpl(graph, nodes, minimumSpacing, Coordinate.X);
      }
    }

    /// <summary>
    ///   Distributes the given nodes along the y-axis. I.e. the given nodes are repositioned such that
    ///   they do not overlap vertically. (Horizontal overlaps are not removed, though.)
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    /// <param name="minimumSpacing">The minimum distance between distributed nodes.</param>
    public static void DistributeVertically(IGraph graph, IEnumerable<INode> nodes, double minimumSpacing = 0) {
      if (HasAtLeastTwoNodes(nodes)) {
        DistributeImpl(graph, nodes, minimumSpacing, Coordinate.Y);
      }
    }

    /// <summary>
    ///   Distributes the given nodes along the given coordinate axis.
    /// </summary>
    /// <param name="graph">The graph that contains the given nodes.</param>
    /// <param name="nodes">The nodes to be repositioned.</param>
    /// <param name="minimumSpacing">The minimum distance between distributed nodes.</param>
    /// <param name="coordinate">The coordinate property to use for positioning the nodes.</param>
    private static void DistributeImpl(
        IGraph graph,
        IEnumerable<INode> nodes,
        double minimumSpacing,
        Coordinate coordinate) {
      if (!nodes.Any()) {
        return;
      }

      var edit = graph.BeginEdit("Distribute Nodes", "Distribute Nodes");

      var array = nodes.ToList();
      var firstLayout = array[0].Layout;
      var min = GetCoordinate(firstLayout, coordinate);
      var max = min + GetSize(firstLayout, coordinate);
      var occupied = 0.0;

      foreach (var node in array) {
        var layout = node.Layout;

        min = Math.Min(min, GetCoordinate(layout, coordinate));
        max = Math.Max(max, GetCoordinate(layout, coordinate) + GetSize(layout, coordinate));
        occupied += GetSize(layout, coordinate);
      }

      var dist = Math.Max(0, minimumSpacing);
      var availableSpace = max - min - occupied;
      var spacingInBetween = availableSpace > dist ? availableSpace / (array.Count - 1) : dist;

      array = array.OrderBy(node => GetCenter(node.Layout, coordinate)).ToList();

      var pos = GetCoordinate(array[0].Layout, coordinate);
      foreach (var node in array) {
        UpdateLayout(graph, node, coordinate, pos);
        pos += GetSize(node.Layout, coordinate) + spacingInBetween;
      }

      edit.Commit();
    }

    /// <summary>
    ///   Determines if the given enumerable has at least two elements.
    /// </summary>
    /// <param name="nodes">The enumerable to check.</param>
    private static bool HasAtLeastTwoNodes(IEnumerable<INode> nodes) => nodes.Take(2).Count() == 2;

    /// <summary>
    ///   Sets the given value for the given coordinate of the given node.
    /// </summary>
    /// <param name="graph">The graph that contains the given node.</param>
    /// <param name="node">The node whose coordinate is set.</param>
    /// <param name="coordinate">The coordinate property to set.</param>
    /// <param name="value">The new value to set for the given coordinate.</param>
    private static void UpdateLayout(IGraph graph, INode node, Coordinate coordinate, double value) {
      var layout = node.Layout;
      var updatedLayout = coordinate == Coordinate.X
          ? new RectD(value, layout.Y, layout.Width, layout.Height)
          : new RectD(layout.X, value, layout.Width, layout.Height);
      graph.SetNodeLayout(node, updatedLayout);
    }

    private static double GetCoordinate(IRectangle layout, Coordinate coordinate) =>
        coordinate == Coordinate.X ? layout.X : layout.Y;

    private static double GetSize(IRectangle layout, Coordinate coordinate) =>
        coordinate == Coordinate.X ? layout.Width : layout.Height;

    private static double GetCenter(IRectangle layout, Coordinate coordinate) =>
        coordinate == Coordinate.X ? layout.GetCenterX() : layout.GetCenterY();
  }
}
