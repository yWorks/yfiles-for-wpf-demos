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
using yWorks.Annotations;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Graph.BezierEdgeStyle
{
  /// <summary>
  /// Custom bend creator for bezier edges
  /// </summary>
  /// <remarks>This implementation always creates collinear triples of bends since the bezier edge model expects this.
  /// In addition, the new bends and the neighboring bends are positioned so that the curve shape stays constant.</remarks>
  internal class BezierBendCreator : IBendCreator
  {
    /// <summary>
    /// Fallback for bend creation if the existing model is not consistent
    /// </summary>
    private static readonly IBendCreator fallBackCreator = new DefaultBendCreator();

    /// <summary>
    /// If the existing number of bends is 2 mod 3 (i.e. the bends are consistent with
    /// what the bezier style expects),
    /// this implementation creates a triple of collinear bends and adjust the neighboring bends
    /// in a way that the shape of the curve is not changed initially and returns the middle bend.
    /// If there are no bends at all, it creates a triple plus two initial and final control bends, all of them collinear.
    /// Otherwise, the fallback bend creator is used to create a bend with its default strategy.
    /// </summary>
    /// <returns>The index of middle bend of a control point triple if such a triple was created,
    /// or the index of the newly created single bend.</returns>
    public int CreateBend(IInputModeContext context, IGraph graph, IEdge edge, PointD location) {
      switch (edge.Bends.Count) {
        case 0:
          var spl = edge.SourcePort.GetLocation();
          var tpl = edge.TargetPort.GetLocation();

          //a single linear segment... we just insert 5 collinear bends adjusted to the angle of the linear segment,
          //approximately evenly spaced
          graph.AddBend(edge, (location - spl) / 4 + spl, 0);
          graph.AddBend(edge, -(location - spl) / 4 + location, 1);
          graph.AddBend(edge, location, 2);
          graph.AddBend(edge, (location - spl) / 4 + location, 3);
          graph.AddBend(edge, location + (tpl - location) * 3 / 4, 4);
          return 2;
        case 1:
          //Use the default strategy to insert a single bend at the correct index
          return fallBackCreator.CreateBend(context, graph, edge, location);
        default:
          var pathPoints = edge.GetPathPoints();
          if (pathPoints.Count % 3 == 1) {
            //Consistent number of existing points
            //Try to insert a smooth bend
            //I.e. a triple of three collinear bends and adjust the neighbor bends

            //Various quality measures and counters
            var segmentIndex = 0;
            var pathCounter = 0;
            var bestDistanceSqr = Double.PositiveInfinity;
            double bestRatio = Double.NaN;

            //The index of the segment where we want to create the bend in the end
            int bestIndex = -1;

            //Find the best segment
            while (pathCounter + 3 < pathPoints.Count) {
              //Get the control points defining the current segment
              var cp0 = pathPoints[pathCounter++];
              var cp1 = pathPoints[pathCounter++];
              var cp2 = pathPoints[pathCounter++];
              //Consecutive segments share the last/first control point! So we may not advance the counter here
              var cp3 = pathPoints[pathCounter];
              //Shift a cubic segment
              //
              //Here we assume that the path is actually composed of cubic segments, only.
              //Alternatively, we could inspect the actual path created by the edge renderer - this would also
              //allow to deal with intermediate non cubic segments, but we'd have to associate those
              //path segments somehow with the correct bends again, so again this would be tied to the actual
              //renderer implementation.
              var fragment = new GeneralPath(2);
              fragment.MoveTo(cp0);
              fragment.CubicTo(cp1, cp2, cp3);

              //Try to find the projection onto the fragment
              var ratio = fragment.GetProjection(location, 0);
              if (ratio.HasValue) {
                //Actually found a projection ratio
                //Determine the point on the curve - the tangent provides this
                var tangent = fragment.GetTangent(0, ratio.Value);
                if (tangent.HasValue) {
                  //There actually is a tangent
                  var d = (location - tangent.Value.Point).SquaredVectorLength;
                  //Is this the best distance?
                  if (d < bestDistanceSqr) {
                    bestDistanceSqr = d;
                    //Remember ratio (needed to split the curve)
                    bestRatio = ratio.Value;
                    //and the index, of course
                    bestIndex = segmentIndex;
                  }
                }
              }
              ++segmentIndex;
            }
            if (bestIndex != -1) {
              //Actually found a segment
              //For the drag, we want to move the middle bend
              return CreateBends(graph, edge, bestIndex, bestRatio, pathPoints).GetIndex();
            }
            //No best segment found (for whatever reason) - we don't want to create a bend so that we don't mess up anything
            return -1;
          } else {
            //No consistent number of bends - just insert a single bend
            //We could also see whether we actually would have a cubic segment on the path, and treat that differently
            //However, why bother - just create the edge with a correct number of points instead
            return fallBackCreator.CreateBend(context, graph, edge, location);
          }
      }
    }

    /// <summary>
    /// Create a triple of control bends and adjust the neighboring bends
    /// </summary>
    /// <param name="graph">The graph where the bends are created</param>
    /// <param name="edge">The edge where the bends are created</param>
    /// <param name="segmentIndex">The segment index</param>
    /// <param name="ratio">The ratio on the segment</param>
    /// <param name="pathPoints">The existing control points</param>
    /// <returns>The middle bend of a control point triple</returns>
    [NotNull]
    private static IBend CreateBends([NotNull] IGraph graph, [NotNull] IEdge edge, int segmentIndex, double ratio,
                                     [NotNull] IListEnumerable<IPoint> pathPoints) {
      //Create 3 bends and adjust the neighbors
      //The first bend we need to touch is at startIndex
      var startIndex = segmentIndex * 3;

      //This holds the new coordinates left and right of the split point
      //We don't actually need all of them, but this keeps the algorithm more straightforward.
      var left = new PointD[4];
      var right = new PointD[4];

      //Determine the new control points to cleanly split the curve

      GetCubicSplitPoints(ratio,
          new[] {
              pathPoints[startIndex].ToPointD(), pathPoints[startIndex + 1].ToPointD(),
              pathPoints[startIndex + 2].ToPointD(), pathPoints[startIndex + 3].ToPointD()
          }, left, right);

      //Previous control point - does always exist as a bend, given our precondition
      var previousBend = edge.Bends[startIndex];
      //Next control point - also always exists given the precondition for bend counts (i.e. there have to be at least two)
      var nextBend = edge.Bends[startIndex + 1];

      //We create the three new bends between previous bend and next bend and adjust these two.
      //We don't have to adjust more bends, since we just have a cubic curve.
      IBend bendToMove;
      var engine = graph.GetUndoEngine();

      //Wrap everything into a single compound edit, so that everything can be undone in a single unit
      using (var edit = graph.BeginEdit("Create Bezier Bend", "Create Bezier Bend")) {
        try {
          //Adjust the previous bend - given the split algorithm, its coordinate is in left[1]
          //(left[0] is actually kept unchanged from the initial value)
          var oldPrevLocation = previousBend.Location.ToPointD();
          var newPrevLocation = left[1];
          graph.SetBendLocation(previousBend, newPrevLocation);
          // Add unit to engine
          graph.AddUndoUnit("Set bend location", "Set bend location",
              () => graph.SetBendLocation(previousBend, oldPrevLocation),
              () => graph.SetBendLocation(previousBend, newPrevLocation));

          //Insert the new triple, using the values from left and right in order
          graph.AddBend(edge, left[2], startIndex + 1);
          bendToMove = graph.AddBend(edge, left[3], startIndex + 2);
          //right[0] == left[3], so right[1] is the next new control point
          graph.AddBend(edge, right[1], startIndex + 3);

          //Adjust the next bend
          var oldNextLocation = nextBend.Location.ToPointD();
          var newNextLocation = right[2];
          graph.SetBendLocation(nextBend, newNextLocation);
          // Add unit to engine
          graph.AddUndoUnit("Set bend location", "Set bend location",
              () => graph.SetBendLocation(nextBend, oldNextLocation),
              () => graph.SetBendLocation(nextBend, newNextLocation));

        } catch {
          //Cancel the edit in case anything goes wrong.
          edit.Cancel();
          throw;
        }
      }

      return bendToMove;
    }


    /// <summary>
    /// For an array of <paramref name="controlPoints"/> defining a cubic segment
    /// and a given <paramref name="ratio"/> on the segment, populate the <paramref name="left"/>
    /// and <paramref name="right"/> arrays with new control points so that the cubic can be
    /// split smoothly at the <paramref name="ratio"/>
    /// </summary>
    private static void GetCubicSplitPoints(double ratio, [NotNull] PointD[] controlPoints, [NotNull] PointD[] left,
                                            [NotNull] PointD[] right) {
      //Determine the new control points
      //Based on de Casteljau's algorithm, but iterations unrolled, since we have a fixed curve order
      var c_1_1 = ((1 - ratio) * controlPoints[1] + ratio * controlPoints[2]);

      left[0] = controlPoints[0];
      right[3] = controlPoints[3];
      left[1] = (1 - ratio) * left[0] + ratio * controlPoints[1];
      right[2] = (1 - ratio) * controlPoints[2] + ratio * right[3];

      left[2] = (1 - ratio) * left[1] + ratio * (c_1_1);
      right[1] = (1 - ratio) * c_1_1 + ratio * right[2];

      right[0] = left[3] = (1 - ratio) * left[2] + ratio * right[1];
    }
  }
}
