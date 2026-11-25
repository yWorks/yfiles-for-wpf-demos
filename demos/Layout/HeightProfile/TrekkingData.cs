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
using yWorks.Graph;

namespace yWorks.Demo.HeightProfile
{
  /// <summary>
  ///   Describes a point of the trekking trail.
  /// </summary>
  /// <param name="X">The x-coordinate of the waypoint.</param>
  /// <param name="Y">The y-coordinate of the waypoint.</param>
  /// <param name="Lon">The longitude of the waypoint.</param>
  /// <param name="Lat">The latitude of the waypoint.</param>
  /// <param name="Name">The name of the waypoint.</param>
  /// <param name="Category">The category of the waypoint.</param>
  /// <param name="Type">The type of the waypoint, used for distinguishing the waypoints from the label nodes.</param>
  /// <param name="Icon">An icon used for the description of the waypoint during highlighting.</param>
  /// <param name="Id">The id of the waypoint, used only for the points from which the nodes will be created.</param>
  internal sealed record Waypoint(
      double X,
      double Y,
      double? Lon = null,
      double? Lat = null,
      string? Name = null,
      string? Category = null,
      string? Type = null,
      string? Icon = null,
      int? Id = null);

  /// <summary>
  ///   Describes the dataset used in this demo, which consists of a set of waypoints and a set of points
  ///   that form the trail.
  /// </summary>
  internal sealed record TrekkingData(IReadOnlyList<Waypoint> Waypoints, IReadOnlyList<Waypoint> Trail);

  /// <summary>
  ///   Returns the data associated with each waypoint.
  /// </summary>
  internal static class WaypointData
  {
    public static Waypoint? GetWaypoint(INode node) => node.Tag as Waypoint;
  }
}
