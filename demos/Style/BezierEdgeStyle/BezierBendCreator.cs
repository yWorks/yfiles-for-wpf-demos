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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.BezierEdgeStyle
{
  /// <summary>
  ///   Custom bend creator for Bézier edges. This implementation always creates collinear triples of bends since the
  ///   Bézier edge model expects this. The new bends and neighboring bends are positioned so that the curve shape stays
  ///   constant.
  /// </summary>
  public sealed class BezierBendCreator : IBendCreator
  {
    private readonly IBendCreator originalBendCreator;
    private readonly IEdge edge;

    public BezierBendCreator(IEdge edge, IBendCreator originalBendCreator) {
      this.edge = edge;
      this.originalBendCreator = originalBendCreator;
    }

    /// <summary>
    ///   If the existing number of bends is 2 mod 3, this implementation creates a triple of collinear bends and adjusts
    ///   neighboring bends so that the curve shape is initially unchanged, then returns the middle bend. If there are no
    ///   bends, it creates a triple plus two initial and final control bends, all of them collinear. Otherwise, the
    ///   fallback bend creator is used.
    /// </summary>
    public int CreateBend(IInputModeContext context, IGraph graph, PointD location) {
      switch (edge.Bends.Count) {
        case 0: {
          var sourcePortLocation = edge.SourcePort.GetLocation();
          var targetPortLocation = edge.TargetPort.GetLocation();

          // A single linear segment: insert five collinear bends adjusted to the angle of the segment, approximately evenly spaced.
          graph.AddBend(edge, sourcePortLocation + (location - sourcePortLocation) * 0.25, 0);
          graph.AddBend(edge, location - (location - sourcePortLocation) * 0.25, 1);
          graph.AddBend(edge, location, 2);
          graph.AddBend(edge, location + (location - sourcePortLocation) * 0.25, 3);
          graph.AddBend(edge, location + (targetPortLocation - location) * 0.75, 4);
          return 2;
        }
        case 1:
          // Use the default strategy to insert a single bend at the correct index.
          return originalBendCreator.CreateBend(context, graph, location);
        default: {
          var pathPoints = edge.GetPathPoints();
          if (pathPoints.Count % 3 != 1) {
            // There is no consistent number of bends; create a single bend instead.
            // We could check whether the path actually contains a cubic segment, but that would tie this code to the renderer.
            return originalBendCreator.CreateBend(context, graph, location);
          }

          // The number of points is consistent. Try to insert a smooth bend by splitting the closest cubic segment.
          var segmentIndex = 0;
          var pathCounter = 0;
          var bestDistanceSquared = double.PositiveInfinity;
          var bestRatio = double.NaN;
          var bestIndex = -1;

          while (pathCounter + 3 < pathPoints.Count) {
            // Get the control points defining the current segment.
            var cp0 = pathPoints[pathCounter++].ToPointD();
            var cp1 = pathPoints[pathCounter++].ToPointD();
            var cp2 = pathPoints[pathCounter++].ToPointD();
            // Consecutive segments share the last/first control point, so do not advance the counter here.
            var cp3 = pathPoints[pathCounter].ToPointD();

            // Shift a cubic segment. Here we assume that the path is actually composed of cubic segments only.
            // Inspecting the rendered edge would also require associating path segments with bends and would tie this to the renderer.
            var fragment = new GeneralPath(2);
            fragment.MoveTo(cp0);
            fragment.CubicTo(cp1, cp2, cp3);

            // Try to find the projection onto the fragment.
            var projectedRatio = fragment.GetProjection(location, 0);
            if (projectedRatio is double ratio && fragment.GetTangent(0, ratio) is Tangent tangent) {
              // Determine the point on the curve; the tangent provides this.
              var distanceSquared = (location - tangent.Point).SquaredVectorLength;
              if (distanceSquared < bestDistanceSquared) {
                bestDistanceSquared = distanceSquared;
                bestRatio = ratio;
                bestIndex = segmentIndex;
              }
            }
            ++segmentIndex;
          }

          if (bestIndex != -1) {
            // For the drag, move the middle bend of the inserted triple.
            return CreateBends(graph, bestIndex, bestRatio, pathPoints).GetIndex();
          }
          // No segment was found, so do not create a bend that would distort the curve.
          return -1;
        }
      }
    }

    /// <summary>Creates a triple of control bends and adjusts the neighboring bends.</summary>
    private IBend CreateBends(IGraph graph, int segmentIndex, double ratio, IReadOnlyList<IPoint> pathPoints) {
      // The first bend we need to touch is at startIndex.
      var startIndex = segmentIndex * 3;

      // These arrays hold the new coordinates to the left and right of the split point.
      var left = new PointD[4];
      var right = new PointD[4];

      // Determine the new control points to cleanly split the curve.
      GetCubicSplitPoints(ratio, [
          pathPoints[startIndex].ToPointD(), pathPoints[startIndex + 1].ToPointD(),
          pathPoints[startIndex + 2].ToPointD(), pathPoints[startIndex + 3].ToPointD()
      ], left, right);

      // Previous and next control points exist because of the bend count precondition.
      var previousBend = edge.Bends[startIndex];
      var nextBend = edge.Bends[startIndex + 1];
      var engine = graph.GetUndoEngine();

      // Wrap all changes into a single compound edit so they can be undone in one step.
      var edit = graph.BeginEdit("Create Bezier Bend", "Create Bezier Bend");
      try {
        // Adjust the previous bend; left[0] stays unchanged and left[1] is its new coordinate.
        var oldLocation = previousBend.Location.ToPointD();
        graph.SetBendLocation(previousBend, left[1]);
        engine?.AddUnit(new BendLocationUndoUnit(graph, previousBend, oldLocation));

        // Insert the new triple, using the values from left and right in order.
        graph.AddBend(edge, left[2], startIndex + 1);
        var bendToMove = graph.AddBend(edge, left[3], startIndex + 2);
        // right[0] equals left[3], so right[1] is the next new control point.
        graph.AddBend(edge, right[1], startIndex + 3);

        // Adjust the next bend.
        oldLocation = nextBend.Location.ToPointD();
        graph.SetBendLocation(nextBend, right[2]);
        engine?.AddUnit(new BendLocationUndoUnit(graph, nextBend, oldLocation));
        edit.Commit();
        return bendToMove;
      } catch (Exception exception) {
        // Cancel the edit in case everything goes wrong.
        edit.Cancel();
        throw new InvalidOperationException("Bend creation failed", exception);
      }
    }

    /// <summary>
    ///   For an array of control points defining a cubic segment and a given ratio, populates the left and right arrays
    ///   with new control points so the cubic can be split smoothly at that ratio.
    /// </summary>
    private static void GetCubicSplitPoints(double ratio, PointD[] controlPoints, PointD[] left, PointD[] right) {
      // Determine the new control points using de Casteljau's algorithm, unrolled for the fixed curve order.
      var c11 = controlPoints[1] * (1 - ratio) + controlPoints[2] * ratio;

      left[0] = controlPoints[0];
      right[3] = controlPoints[3];
      left[1] = left[0] * (1 - ratio) + controlPoints[1] * ratio;
      right[2] = controlPoints[2] * (1 - ratio) + right[3] * ratio;

      left[2] = left[1] * (1 - ratio) + c11 * ratio;
      right[1] = c11 * (1 - ratio) + right[2] * ratio;

      right[0] = left[3] = left[2] * (1 - ratio) + right[1] * ratio;
    }
  }

  /// <summary>Custom undo unit for bend location changes.</summary>
  internal sealed class BendLocationUndoUnit : IUndoUnit
  {
    private readonly PointD oldValue;
    private readonly IGraph graph;
    private readonly IBend bend;
    private readonly PointD newValue;

    internal BendLocationUndoUnit(IGraph graph, IBend bend, PointD oldValue) {
      this.graph = graph;
      this.bend = bend;
      this.oldValue = oldValue;
      newValue = bend.Location.ToPointD();
    }

    public string UndoName => "Set bend location";

    public string RedoName => "Set bend location";

    public void Undo() => graph.SetBendLocation(bend, oldValue);

    public void Redo() => graph.SetBendLocation(bend, newValue);

    public bool TryMergeUnit(IUndoUnit unit) => false;

    public bool TryReplaceUnit(IUndoUnit unit) => false;

    public void Dispose() { }
  }
}
