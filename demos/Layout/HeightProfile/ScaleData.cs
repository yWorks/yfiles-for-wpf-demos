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
using System.Windows;
using yWorks.Geometry;

namespace yWorks.Demo.HeightProfile
{
  internal static class ScaleData
  {
    /// <summary>
    ///   The maximum coordinate of the horizontal axis.
    /// </summary>
    public static readonly double ScaledMaxX = Math.Max(
        Math.Max(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight) * 0.9, 300);

    /// <summary>
    ///   The maximum coordinate of the vertical axis.
    /// </summary>
    public static readonly double ScaledMaxY = Math.Max(
        Math.Min(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight) * 0.4, 100);

    /// <summary>
    ///   Scales the given dataset.
    /// </summary>
    public static IReadOnlyList<PointD> ScaleDataPoints(IReadOnlyList<Waypoint> trail) {
      var (maxX, maxY) = GetMax(trail);
      return [
          .. trail.Select(point =>
                  new PointD(ScalePoint(point.X, maxX, ScaledMaxX), -ScalePoint(point.Y, maxY, ScaledMaxY)))
              .OrderBy(point => point.X)
      ];
    }

    /// <summary>
    ///   Returns the maximum values of the x- and y-coordinates of the trail's points.
    /// </summary>
    public static (double MaxX, double MaxY) GetMax(IReadOnlyList<Waypoint> trail) =>
        (trail.Max(point => point.X), trail.Max(point => point.Y));

    /// <summary>
    ///   Scales the given point.
    /// </summary>
    /// <param name="location">The location of the point.</param>
    /// <param name="max">The maximum coordinate of the axis.</param>
    /// <param name="scaledMax">The maximum scaled coordinate.</param>
    public static double ScalePoint(double location, double max, double scaledMax) =>
        Math.Floor(location * (scaledMax / max));
  }
}
