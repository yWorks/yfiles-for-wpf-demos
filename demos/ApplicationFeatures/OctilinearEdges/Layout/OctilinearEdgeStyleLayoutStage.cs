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

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   A layout stage that transforms octilinear edge routes to orthogonal edge routes.
  ///   Octilinear edge routes usually consist of bends with a specific angle, which can be transformed to orthogonal
  ///   edge routes by adjusting the bend locations and removing the superfluous bends afterward. The octilinear
  ///   cutting length is stored in the mapper provided by the <see cref="ApplyOctilinearLayout.EdgeBendsCuttingLengthKey"/>.
  /// </summary>
  internal sealed class OctilinearEdgeStyleLayoutStage : LayoutStageBase
  {
    internal OctilinearEdgeStyleLayoutStage(ILayoutAlgorithm coreLayout) : base(coreLayout) { }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      CoreLayout!.ApplyLayout(graph);

      // store the octilinear cutting lengths in the mapper to apply them to the bends afterward
      var edgeBendsMapper = graph.Context.GetItemData(ApplyOctilinearLayout.EdgeBendsCuttingLengthKey);
      if (edgeBendsMapper == null) {
        return;
      }

      foreach (var edge in graph.Edges) {
        var sourcePoint = edge.SourcePortLocation;
        var targetPoint = edge.TargetPortLocation;
        var bends = edge.Bends.ToArray();

        for (var i = 0; i < bends.Length; i++) {
          var bend = bends[i];
          var bendLocation = new PointD(bend.X, bend.Y);

          var prevLocation = i == 0 ? sourcePoint : bends[i - 1].Location;
          var nextLocation = i == bends.Length - 1 ? targetPoint : bends[i + 1].Location;

          var incomingVector = bendLocation - prevLocation;
          var outgoingVector = nextLocation - bendLocation;

          var normalizedDotProduct = NormalizedDotProduct(incomingVector, outgoingVector);

          if (Is90DegreeApart(normalizedDotProduct)) {
            // no need to delete the next bend, just store a cutting length of zero for this bend
            WriteBendCuttingLength(bend, 0);
            continue;
          }

          if (Is135DegreeApart(normalizedDotProduct)) {
            var nextNextPoint = i == bends.Length - 2 ? targetPoint :
                i < bends.Length - 2 ? bends[i + 2].Location : (PointD?) null;

            if (nextNextPoint.HasValue) {
              var nextOutgoingVector = nextNextPoint.Value - nextLocation;

              if (IsSameDirection(NormalizedDotProduct(incomingVector, nextOutgoingVector))) {
                // A "Z" shaped octilinear segment without an extra horizontal/vertical segment.
                // In this case, we need to keep both existing bends to preserve the octilinear shape.
                var nextBend = bends[i + 1];
                if (prevLocation.Y == bendLocation.Y) {
                  // horizontal segment, the bends need to be moved horizontally
                  var meanX = (bendLocation.X + nextLocation.X) * 0.5;
                  var cuttingLength = Math.Abs(meanX - nextLocation.X);
                  bend.X = meanX;
                  nextBend.X = meanX;

                  WriteBendCuttingLength(bend, cuttingLength);
                  WriteBendCuttingLength(nextBend, cuttingLength);
                }
                if (prevLocation.X == bendLocation.X) {
                  // vertical segment, the bends need to be moved vertically
                  var meanY = (bendLocation.Y + nextLocation.Y) * 0.5;
                  var cuttingLength = Math.Abs(meanY - nextLocation.Y);

                  bend.Y = meanY;
                  nextBend.Y = meanY;

                  WriteBendCuttingLength(bend, cuttingLength);
                  WriteBendCuttingLength(nextBend, cuttingLength);
                }
                continue;
              }
            }

            // An octilinear segment that is followed by a horizontal/vertical segment.
            // in this case, move one bend to make it orthogonally, which implicitly makes the other bend collinear.
            double length;
            if (Math.Abs(bendLocation.Y - prevLocation.Y) <= Utils.Eps) {
              length = Math.Abs(bendLocation.X - nextLocation.X);
              bend.X = nextLocation.X;
            } else {
              length = Math.Abs(bendLocation.Y - nextLocation.Y);
              bend.Y = nextLocation.Y;
            }
            WriteBendCuttingLength(bend, length);
          }

          // else 180°, collinear bend location, do nothing, will be removed afterward
        }
      }

      // remove collinear bends now
      foreach (var edge in graph.Edges) {
        var bendCuttingLengths = new List<double>();
        foreach (var bend in edge.Bends.ToArray()) {
          if (bend.Tag is OctilinearBendTag tag) {
            bendCuttingLengths.Add(tag.CuttingLength);
          } else {
            graph.Remove(bend);
          }
        }

        // store the cutting lengths in the output mapper
        edgeBendsMapper[edge] = [.. bendCuttingLengths];
      }
    }

    private static double NormalizedDotProduct(PointD v1, PointD v2) => v1.Normalized.ScalarProduct(v2.Normalized);

    private static bool Is135DegreeApart(double dotProduct) {
      var target = Math.Sqrt(0.5); // -> 1 / Math.Sqrt(2)
      return Math.Abs(dotProduct - target) <= Utils.Eps;
    }

    private static bool Is90DegreeApart(double dotProduct) => Math.Abs(dotProduct) <= Utils.Eps;

    private static bool IsSameDirection(double dotProduct) => dotProduct >= 1 - Utils.Eps;

    private static void WriteBendCuttingLength(Bend bend, double cuttingLength) {
      if (bend.Tag == null) {
        bend.Tag = new OctilinearBendTag { CuttingLength = cuttingLength };
      }
    }
  }
}
