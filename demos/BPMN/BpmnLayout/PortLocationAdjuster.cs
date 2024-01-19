/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  /// A layout stage that adjusts the source and target ports at non-regular shaped nodes
  /// so that the edges end inside the node.
  /// </summary>
  /// <remarks>
  /// Only activity nodes are considered to have regular shapes.
  /// </remarks>
  public class PortLocationAdjuster : ILayoutAlgorithm
  {
    /// <summary>
    /// <see cref="yWorks.Algorithms.IDataProvider"/> key used to store if the ports on a node should be adjusted.
    /// </summary>
    public static readonly NodeDpKey<bool> AffectedNodesDpKey = new NodeDpKey<bool>(typeof(PortLocationAdjuster), "com.yworks.yfiles.bpmn.layout.PortLocationAdjuster.AffectedNodesDpKey");

    /// <inheritdoc/>
    public void ApplyLayout(LayoutGraph graph) {
      var affectedNodesDP = graph.GetDataProvider(AffectedNodesDpKey);

      for (IEdgeCursor ec = graph.GetEdgeCursor(); ec.Ok; ec.Next()) {
        Edge e = ec.Edge;
        YPointPath path = graph.GetPath(e);
        //adjust source point
        if (affectedNodesDP == null || affectedNodesDP.GetBool(e.Source)) {
          AdjustPortLocation(graph, e, path, true);
        }
        if (affectedNodesDP == null || affectedNodesDP.GetBool(e.Target)) {
          AdjustPortLocation(graph, e, path, false);
        }
      }
    }

    /// <summary>
    /// Adjusts the edge end points so they don't end outside the shape of the node they are attached to.
    /// </summary>
    private static void AdjustPortLocation(LayoutGraph graph, Edge e, YPointPath path, bool atSource) {
      Node node = atSource ? e.Source : e.Target;
      YPoint pointRel = atSource ? graph.GetSourcePointRel(e) : graph.GetTargetPointRel(e);
      // get offset from the node center to the end of the shape at the node side the edge connects to
      LineSegment segment = path.GetLineSegment(atSource ? 0 : path.Length() - 2);
      double offset = Math.Min(graph.GetWidth(node), graph.GetHeight(node))/2;
      double offsetX = segment.DeltaX > 0 ^ atSource ? -offset : offset;
      double offsetY = segment.DeltaY > 0 ^ atSource ? -offset : offset;
      // if the edge end point is at the center of this side, we use the calculated offset to put the end point on
      // the node bounds, otherwise we prolong the last segment to the center line of the node so it doesn't end
      // outside the node's shape
      YPoint newPortLocation = segment.IsHorizontal
        ? new YPoint(pointRel.Y != 0 ? 0 : offsetX, pointRel.Y)
        : new YPoint(pointRel.X, pointRel.X != 0 ? 0 : offsetY);
      if (atSource) {
        graph.SetSourcePointRel(e, newPortLocation);
      } else {
        graph.SetTargetPointRel(e, newPortLocation);
      }
    }
  }
}