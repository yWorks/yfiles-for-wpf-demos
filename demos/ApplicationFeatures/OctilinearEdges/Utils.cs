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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.OctilinearEdges
{
  internal enum Corner
  {
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
  }

  public sealed class OctilinearBendTag
  {
    public double CuttingLength { get; set; }
  }

  internal static class Utils
  {
    internal const double Eps = 1e-6;

    /// <summary>
    ///   Creates a hierarchical layout with an octilinear routing style.
    /// </summary>
    internal static HierarchicalLayout CreateHierarchicalLayout(bool fromSketch = false) =>
        new() {
            FromSketchMode = fromSketch,
            NodeDistance = 150,
            NodeToEdgeDistance = 70,
            DefaultEdgeDescriptor = new HierarchicalLayoutEdgeDescriptor {
                MinimumFirstSegmentLength = 30,
                MinimumLastSegmentLength = 30,
                MinimumOctilinearSegmentLength = 30,
                RoutingStyleDescriptor = new RoutingStyleDescriptor(RoutingStyle.Octilinear)
            }
        };

    /// <summary>
    ///   Calculates the cutting length for a given octilinear segment length. The cutting length is the
    ///   side of the triangle with the bend as tip and with the octilinear segment as hypotenuse
    /// </summary>
    internal static double SegmentLengthToCuttingLength(double segmentLength) =>
        Math.Sqrt(0.5 * Math.Pow(segmentLength, 2));

    /// <summary>
    ///   Returns the cutting length of the given bend or port stored in the item's tag or the preferred
    ///   length set on the style if no tag is present.
    /// </summary>
    internal static double GetCuttingLength(IModelItem item) {
      if (item is not IBend bend) {
        return 0;
      }

      if (IsOctilinearBendTag(bend.Tag)) {
        return ((OctilinearBendTag) bend.Tag!).CuttingLength;
      }
      if (bend.Owner.Style is OctilinearEdgeStyle style) {
        return SegmentLengthToCuttingLength(style.PreferredOctilinearSegmentLength);
      }

      return 0;
    }

    /// <summary>
    ///   Returns the previous bend or the source port of the given bend.
    /// </summary>
    internal static IModelItem GetPreviousEdgeItem(IBend bend) {
      var bendIndex = bend.GetIndex();
      return bendIndex == 0 ? bend.Owner.SourcePort : bend.Owner.Bends[bendIndex - 1];
    }

    /// <summary>
    ///   Returns the next bend or the target port of the given bend.
    /// </summary>
    internal static IModelItem GetNextEdgeItem(IBend bend) {
      var bendIndex = bend.GetIndex();
      return bendIndex == bend.Owner.Bends.Count - 1 ? bend.Owner.TargetPort : bend.Owner.Bends[bendIndex + 1];
    }

    /// <summary>
    ///   Returns the location of a bend or port as a point.
    /// </summary>
    internal static PointD GetLocation(IModelItem item) {
      return item switch {
          IBend bend => new PointD(bend.Location.X, bend.Location.Y),
          IPort port => port.GetLocation(),
          _ => throw new ArgumentException("Expected an edge bend or port.", nameof(item))
      };
    }

    /// <summary>
    ///   Calculates the current cutting length of the given bend or port based on the edge's geometry.
    /// </summary>
    internal static double GetVisualCuttingLength(IModelItem item) {
      if (item is not IBend bend) {
        return 0;
      }

      var prevItem = GetPreviousEdgeItem(bend);
      var nextItem = GetNextEdgeItem(bend);
      return GetVisualCuttingLengthForItems(prevItem, bend, nextItem);
    }

    /// <summary>
    ///   Calculates the current cutting length of the given bend or port based on the edge's geometry.
    /// </summary>
    internal static double GetVisualCuttingLengthForItems(IModelItem prevItem, IBend bend, IModelItem nextItem) {
      var bendPoint = GetLocation(bend);
      var prevPoint = GetLocation(prevItem);
      var nextPoint = GetLocation(nextItem);

      var vectorToPrev = prevPoint - bendPoint;
      var vectorToNext = nextPoint - bendPoint;
      var prevBendDist = vectorToPrev.VectorLength;
      var nextBendDist = vectorToNext.VectorLength;

      var bendCuttingLength = GetCuttingLength(bend);
      var prevCuttingLength = GetCuttingLength(prevItem);
      var nextCuttingLength = GetCuttingLength(nextItem);

      // The available space between neighboring bends is split regarding the ratio of their cutting length
      var prevMaxFraction = bendCuttingLength == 0 ? 0 : bendCuttingLength / (bendCuttingLength + prevCuttingLength);
      var nextMaxFraction = bendCuttingLength == 0 ? 0 : bendCuttingLength / (bendCuttingLength + nextCuttingLength);

      var prevMax = prevMaxFraction * prevBendDist;
      var nextMax = nextMaxFraction * nextBendDist;

      return Math.Min(bendCuttingLength, Math.Min(prevMax, nextMax));
    }

    /// <summary>
    ///   Calculates the maximum cutting length of the given bend based on the edge's geometry.
    /// </summary>
    internal static double GetMaxCuttingLength(IBend bend) {
      var edge = bend.Owner;
      var bendIndex = bend.GetIndex();

      IModelItem prevItem = bendIndex == 0 ? edge.SourcePort : edge.Bends[bendIndex - 1];
      IModelItem nextItem = bendIndex == edge.Bends.Count - 1 ? edge.TargetPort : edge.Bends[bendIndex + 1];

      var bendPoint = bend.Location.ToPointD();
      var prevPoint = GetLocation(prevItem);
      var nextPoint = GetLocation(nextItem);

      var prevCuttingLength = GetVisualCuttingLength(prevItem);
      var nextCuttingLength = GetVisualCuttingLength(nextItem);

      var vectorToPrev = prevPoint - bendPoint;
      var vectorToNext = nextPoint - bendPoint;

      return Math.Min(vectorToPrev.VectorLength - prevCuttingLength, vectorToNext.VectorLength - nextCuttingLength);
    }

    /// <summary>
    ///   Writes the given cutting length in the bend's tag.
    /// </summary>
    /// <param name="bend">The bend for which the cutting length should be updated.</param>
    /// <param name="cuttingLength">The new cutting length of the bend.</param>
    internal static void UpdateCuttingLength(IBend? bend, double? cuttingLength) {
      if (bend != null && cuttingLength.HasValue) {
        bend.Tag = new OctilinearBendTag { CuttingLength = cuttingLength.Value };
      }
    }

    private static bool IsOctilinearBendTag(object? tag) => tag is OctilinearBendTag;

    /// <summary>
    ///   Calculates the offset of the handle to its given bend based on the current cutting length.
    /// </summary>
    internal static PointD GetHandleOffset(IBend bend) {
      var edge = bend.Owner;
      var bendIndex = bend.GetIndex();

      IModelItem prevItem = bendIndex == 0 ? edge.SourcePort : edge.Bends[bendIndex - 1];
      IModelItem nextItem = bendIndex == edge.Bends.Count - 1 ? edge.TargetPort : edge.Bends[bendIndex + 1];

      var bendPoint = bend.Location.ToPointD();
      var prevPoint = GetLocation(prevItem);
      var nextPoint = GetLocation(nextItem);

      var vectorToPrev = prevPoint - bendPoint;
      var vectorToNext = nextPoint - bendPoint;

      var cuttingLength = GetVisualCuttingLengthForItems(prevItem, bend, nextItem);

      return vectorToPrev.Normalized * (0.5 * cuttingLength) + vectorToNext.Normalized * (0.5 * cuttingLength);
    }

    /// <summary>
    ///   Calculates the corner type of the given bend based on the edge's geometry.
    /// </summary>
    internal static Corner GetCornerType(IBend bend) {
      var edge = bend.Owner;
      var bendIndex = bend.GetIndex();

      IModelItem prevItem = bendIndex == 0 ? edge.SourcePort : edge.Bends[bendIndex - 1]!;
      IModelItem nextItem = bendIndex == edge.Bends.Count - 1 ? edge.TargetPort! : edge.Bends[bendIndex + 1]!;

      var bendPoint = bend.Location.ToPointD();
      var prevPoint = GetLocation(prevItem);
      var nextPoint = GetLocation(nextItem);

      var vectorToPrev = prevPoint - bendPoint;
      var vectorToNext = nextPoint - bendPoint;

      if (vectorToPrev.Y > Eps || vectorToNext.Y > Eps) {
        if (vectorToPrev.X > Eps || vectorToNext.X > Eps) {
          return Corner.TopLeft;
        }
        return Corner.TopRight;
      }
      if (vectorToPrev.X > Eps || vectorToNext.X > Eps) {
        return Corner.BottomLeft;
      }
      return Corner.BottomRight;
    }
  }
}
