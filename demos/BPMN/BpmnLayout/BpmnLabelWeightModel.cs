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
using System.Collections.Generic;
using yWorks.Geometry;
using yWorks.Layout.Graph;
using yWorks.Layout.Labeling;

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  ///   A weight model for exterior node labels that prefers node sides that are far away
  ///   from incoming or outgoing edges.
  /// </summary>
  public static class BpmnLabelWeightModel
  {
    private const double CustomWeightRatio = 0.15;

    public static void NodeLabelWeightDelegate(IEnumerable<LabelCandidate> candidates, NodeLabel label) {
      foreach (var candidate in candidates) {
        var weight = candidate.Weight;
        var customWeight = GetWeight(label, candidate);
        candidate.Weight = ((1 - CustomWeightRatio) * weight) + (CustomWeightRatio * customWeight);
      }
    }

    /// <summary>
    ///   Returns the weight for placing a node label using the given <see cref="LabelCandidate"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Higher weight means better candidates. Hence, there is a higher probability that the candidate is chosen by a
    ///     labeling algorithm.
    ///   </para>
    ///   <para>
    ///     Weights need to have a value between <c>0</c> and <c>1</c>.
    ///   </para>
    /// </remarks>
    /// <param name="label">The owner label of the candidate</param>
    /// <param name="candidate">The candidate</param>
    /// <returns>The weight of the candidate</returns>
    public static double GetWeight(NodeLabel label, LabelCandidate candidate) {
      double weight = 0;
      var node = label.Owner;
      var nodeLayout = node.Layout;
      var candidateLayout = Bounds(candidate.Layout);

      var isLeft = candidateLayout.X + candidateLayout.Width / 2 < nodeLayout.X;
      var isRight = candidateLayout.X + candidateLayout.Width / 2 > nodeLayout.X + nodeLayout.Width;
      var isTop = candidateLayout.Y + candidateLayout.Height / 2 < nodeLayout.Y;
      var isBottom = candidateLayout.Y + candidateLayout.Height / 2 > nodeLayout.Y + nodeLayout.Height;

      var horizontalCenter = !isLeft && !isRight;
      var verticalCenter = !isTop && !isBottom;
      if (horizontalCenter && verticalCenter) {
        // candidate is in center -> don't use
        return 0;
      }
      if (horizontalCenter || verticalCenter) {
        weight = 0.95;
      } else {
        // diagonal candidates get a bit less weight
        weight = 0.9;
      }
      foreach (var edge in node.Edges) {
        var portLocation = edge.Source == node ? edge.SourcePortOffset : edge.TargetPortOffset;
        if (Math.Abs(portLocation.X) > Math.Abs(portLocation.Y)) {
          // edge at left or right
          if (portLocation.X < 0 && isLeft || portLocation.X > 0 && isRight) {
            if (isTop || isBottom) {
              weight -= 0.03;
            } else {
              // edge at same side as candidate
              weight -= 0.2;
            }
          } else if (horizontalCenter) {
            // candidate is close to the edge but not on the same side
            weight -= 0.01;
          }
        } else {
          // edge at top or bottom
          if (portLocation.Y < 0 && isTop || portLocation.Y > 0 && isBottom) {
            if (isLeft || isRight) {
              weight -= 0.03;
            } else {
              weight -= 0.2;
            }
          } else if (verticalCenter) {
            // candidate is close to the edge but not on the same side
            weight -= 0.01;
          }
        }
      }

      return Math.Max(0, weight);
    }

    public static RectD Bounds(IOrientedRectangle rectangle) {
      var width = rectangle.Width;
      var height = rectangle.Height;

      var x1 = rectangle.AnchorX;
      var y1 = rectangle.AnchorY;

      var upX = rectangle.UpX;
      var upY = rectangle.UpY;

      var x2 = x1 + upX * height;
      var y2 = y1 + upY * height;

      var x3 = x1 + upX * height - upY * width;
      var y3 = y1 + upY * height + upX * width;

      var x4 = x1 - upY * width;
      var y4 = y1 + upX * width;

      var minX = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
      var maxX = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
      var minY = Math.Min(Math.Min(y1, y2), Math.Min(y3, y4));
      var maxY = Math.Max(Math.Max(y1, y2), Math.Max(y3, y4));

      return new RectD(minX, minY, maxX - minX, maxY - minY);
    }
  }
}
