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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.OrthogonalEdgeEditingCustomization
{
  /// <summary>
  ///   Creates a bend creator that ensures that inner segments of an edge are always
  ///   orthogonal, even if the new bend was created on the non-orthogonal first
  ///   or last segment.
  /// </summary>
  public sealed class BlueBendCreator(IEdge edge, IBendCreator originalBendCreator) : IBendCreator
  {
    /// <summary>
    ///   Creates a new bend at the given location. If this bend is on the first or last segment,
    ///   a second bend is created and placed at a location that ensures that the newly created
    ///   inner segment is orthogonal.
    /// </summary>
    /// <param name="context">The context for which the bend should be created.</param>
    /// <param name="graph">The graph the edge belongs to.</param>
    /// <param name="location">The preferred coordinates of the bend.</param>
    /// <see cref="IBendCreator.CreateBend"/>
    public int CreateBend(IInputModeContext context, IGraph graph, PointD location) {
      var edgePoints = GetEdgePoints(edge);
      var closestSegment = DetermineBendSegmentIndex(edgePoints, location);

      const int firstSegment = 0;
      var lastSegment = edge.Bends.Count;

      // if bend wasn't created in first or last segment, call default action
      var isFirstOrLastSegment = closestSegment == firstSegment || closestSegment == lastSegment;
      if (!isFirstOrLastSegment) {
        return originalBendCreator.CreateBend(context, graph, location);
      }

      // add created bend and another one to make the edge stay orthogonal
      if (closestSegment == -1 || context is null || context.InputMode is not CreateBendInputMode) {
        return -1;
      }

      var editingContext = context.Lookup<OrthogonalEdgeEditingContext>();
      if (editingContext is null) {
        return -1;
      }

      if (closestSegment == firstSegment) {
        var nextPoint = edgePoints[1];
        // get orientation of next edge segment to determine second bend location
        var orientation = editingContext.GetSegmentOrientation(edge, 1);
        graph.AddBend(edge, location, 0);
        if (orientation == SegmentOrientation.Horizontal) {
          graph.AddBend(edge, new PointD(nextPoint.X, location.Y), 1);
        } else if (orientation == SegmentOrientation.Vertical) {
          graph.AddBend(edge, new PointD(location.X, nextPoint.Y), 1);
        }
        return 0;
      }

      if (closestSegment == lastSegment) {
        var prevPoint = edgePoints[edge.Bends.Count];
        // get orientation of next edge segment to determine second bend location
        var orientation = editingContext.GetSegmentOrientation(edge, edge.Bends.Count - 1);
        graph.AddBend(edge, location, edge.Bends.Count);
        if (orientation == SegmentOrientation.Horizontal) {
          graph.AddBend(edge, new PointD(prevPoint.X, location.Y), edge.Bends.Count - 1);
        } else if (orientation == SegmentOrientation.Vertical) {
          graph.AddBend(edge, new PointD(location.X, prevPoint.Y), edge.Bends.Count - 1);
        }
        return edge.Bends.Count - 1;
      }
      return -1;
    }

    /// <summary>
    ///   Determines the index of the segment in which the bend was created.
    /// </summary>
    /// <param name="points">The points of an edge.</param>
    /// <param name="location">The given location.</param>
    private static int DetermineBendSegmentIndex(IReadOnlyList<PointD> points, PointD location) {
      var closestIndex = -1;
      var minDist = double.MaxValue;
      for (var i = 0; i < points.Count - 1; i++) {
        var dist = location.DistanceToSegment(points[i], points[i + 1]);
        if (dist < minDist) {
          closestIndex = i;
          minDist = dist;
        }
      }
      return closestIndex;
    }

    /// <summary>
    ///   Returns a list containing the source port location, the bend locations,
    ///   and the target port location of the given edge.
    /// </summary>
    /// <param name="edge">The given edge.</param>
    private static IReadOnlyList<PointD> GetEdgePoints(IEdge edge) {
      var points = new List<PointD> { edge.SourcePort.GetLocation() };
      foreach (var bend in edge.Bends) {
        points.Add(bend.Location.ToPointD());
      }
      points.Add(edge.TargetPort.GetLocation());
      return points;
    }
  }
}
