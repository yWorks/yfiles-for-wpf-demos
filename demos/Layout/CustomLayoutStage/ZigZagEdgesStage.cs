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
using yWorks.Layout;
using yWorks.Layout.Graph;

namespace yWorks.Demo.CustomLayoutStage
{
  /// <summary>
  ///   A layout stage that post-processes edge paths produced by the core layout
  ///   and changes all edges into a zig-zag shape, while following the original path.
  /// </summary>
  public sealed class ZigZagEdgesStage : LayoutStageBase
  {
    private const double ZigSize = 5;

    public ZigZagEdgesStage(ILayoutAlgorithm coreLayout) : base(coreLayout) { }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      if (CoreLayout is null) {
        return;
      }

      // Apply the core layout ...
      CoreLayout.ApplyLayout(graph);
      // ... after which we can then change the edge paths
      PostProcessEdgePaths(graph);
    }

    /// <summary>
    ///   Changes the path of each edge into a zig-zag shape.
    /// </summary>
    private static void PostProcessEdgePaths(LayoutGraph graph) {
      foreach (var edge in graph.Edges) {
        List<PointD> path = [
            edge.SourcePortLocation,
            .. edge.Bends.Select(bend => bend.Location),
            edge.TargetPortLocation
        ];

        edge.ResetPath();

        var zigZagPath = GetZigZagPointsForEdge(path);
        edge.SourcePortLocation = zigZagPath[0];
        for (var i = 1; i < zigZagPath.Count - 1; i++) {
          graph.AddBend(edge, zigZagPath[i].X, zigZagPath[i].Y);
        }
        edge.TargetPortLocation = zigZagPath[^1];
      }
    }

    /// <summary>
    ///   Calculates the necessary bend locations to change the edge's path into a zig-zag.
    /// </summary>
    /// <param name="path">The edge path.</param>
    private static List<PointD> GetZigZagPointsForEdge(IReadOnlyList<PointD> path) {
      var list = new List<PointD>();
      for (var i = 0; i < path.Count - 1; i++) {
        var points = GetZigZagPointsForSegment(path[i], path[i + 1]);
        if (i > 0 && points.Count > 0) {
          points.RemoveAt(0);
        }
        list.AddRange(points);
      }
      return list;
    }

    /// <summary>
    ///   Calculates the necessary bend locations to change an edge segment into a zig-zag.
    /// </summary>
    private static List<PointD> GetZigZagPointsForSegment(PointD p1, PointD p2) {
      var length = p1.DistanceTo(p2);
      var result = new List<PointD> { p1 };
      if (length < 2 * ZigSize) {
        result.Add(p2);
        return result;
      }

      // The number of whole v^ we can fit into the segment
      var zigzags = (int) Math.Floor(length / (2 * ZigSize));
      // The length of each v^
      var zigLength = length / zigzags;
      // Create a unit vector along the segment
      var v = new PointD(p2.X - p1.X, p2.Y - p1.Y).Normalized;
      // Create the zig-sized vector perpendicular to the segment
      var zigV = new PointD(v.Y * ZigSize, -v.X * ZigSize);
      var p = p1;
      var vxLength = v.X * zigLength;
      var vyLength = v.Y * zigLength;
      for (var i = 0; i < zigzags; i++) {
        result.Add(TranslatePoint(p, vxLength / 4 + zigV.X, vyLength / 4 + zigV.Y));
        result.Add(TranslatePoint(p, vxLength / 2, vyLength / 2));
        result.Add(TranslatePoint(p, vxLength * 3 / 4 + zigV.X, vyLength * 3 / 4 + zigV.Y));
        p = TranslatePoint(p, vxLength, vyLength);
        result.Add(p);
      }
      return result;
    }

    private static PointD TranslatePoint(PointD p, double dx, double dy) => p + new PointD(dx, dy);
  }
}
