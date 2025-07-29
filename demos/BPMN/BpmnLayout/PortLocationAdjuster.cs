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
using yWorks.Geometry;
using yWorks.Layout.Graph;
using yWorks.Utils;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  ///   A layout stage that adjusts the source and target ports at non-regularly shaped nodes
  ///   so that the edges end inside the node.
  /// </summary>
  /// <remarks>
  ///   Only activity nodes are considered to have regular shapes.
  /// </remarks>
  public class PortLocationAdjuster : ILayoutAlgorithm
  {
    /// <summary>
    ///   <see cref="IMapper{K,V}"/> key used to store if the ports on a node should be adjusted.
    /// </summary>
    public static readonly NodeDataKey<bool> AffectedNodesDataKey =
        new("yWorks.Layout.Bpmn.PortLocationAdjuster.AffectedNodesDataKey");

    /// <summary>
    ///   Represents a small tolerance value used for floating-point comparisons.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The <c>Epsilon</c> constant is used to account for minor inaccuracies in floating-point arithmetic when performing
    ///     geometric calculations involving line segments. It provides a threshold for determining approximate equality or
    ///     intersections, helping to mitigate precision issues that can arise from the limitations of floating-point
    ///     representation.
    ///   </para>
    ///   <para>
    ///     This constant is typically used in methods that involve checking for intersections, containment, or equality of
    ///     line segments where exact precision is not feasible. The default value of <c>1e-8</c> is chosen to balance
    ///     precision with practical considerations of computational accuracy.
    ///   </para>
    /// </remarks>
    private const double Epsilon = 1e-8;

    /// <inheritdoc/>
    public virtual void ApplyLayout(LayoutGraph graph) {
      var affectedNodesDP = graph.Context.GetItemData(AffectedNodesDataKey);

      foreach (var e in graph.Edges) {
        // adjust source point
        if (affectedNodesDP == null || affectedNodesDP[e.Source]) {
          AdjustPortLocation(e, true);
        }
        if (affectedNodesDP == null || affectedNodesDP[e.Target]) {
          AdjustPortLocation(e, false);
        }
      }
    }

    /// <summary>
    ///   Adjusts the edge end points so they don't end outside the shape of the node they are attached to.
    /// </summary>
    private static void AdjustPortLocation(Edge e, bool atSource) {
      var node = atSource ? e.Source : e.Target;
      var pointRel = atSource ? e.SourcePortOffset : e.TargetPortOffset;
      // get offset from the node center to the end of the shape at the node side the edge connects to
      var points = e.PathPoints.ToArray();
      var p1 = atSource ? points[0].Location : points[points.Length - 2].Location;
      var p2 = atSource ? points[1].Location : points[points.Length - 1].Location;
      var segment = new LineSegment(p1, p2);
      var offset = Math.Min(node.Layout.Width, node.Layout.Height) / 2;
      var offsetX = segment.DeltaX > 0 ^ atSource ? -offset : offset;
      var offsetY = segment.DeltaY > 0 ^ atSource ? -offset : offset;
      // if the edge end point is at the center of this side, we use the calculated offset to put the end point on
      // the node bounds, otherwise we prolong the last segment to the center line of the node so it doesn't end
      // outside the node's shape
      var newPortLocation = segment.IsHorizontal(Epsilon)
          ? new PointD(pointRel.Y != 0 ? 0 : offsetX, pointRel.Y)
          : new PointD(pointRel.X, pointRel.X != 0 ? 0 : offsetY);
      if (atSource) {
        e.SourcePortOffset = newPortLocation;
      } else {
        e.TargetPortOffset = newPortLocation;
      }
    }
  }
}
