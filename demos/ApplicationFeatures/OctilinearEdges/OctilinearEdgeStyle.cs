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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   An edge style that draws orthogonal edges in an octilinear style. For this purpose, 90-degree
  ///   corners are cut with a 45-degree segment (called octilinear segment in this context). This
  ///   segment's length is controlled by property <see cref="PreferredOctilinearSegmentLength"/>. The
  ///   implementation of this style calculates with the cutting length, which is the distance between
  ///   the cut-off and the actual bend location. This cutting length is stored in the bend's tag.
  /// </summary>
  public sealed class OctilinearEdgeStyle : PathEdgeStyleBase
  {
    public OctilinearEdgeStyle() {
      Pen = Pens.Black;
      TargetArrow = Arrow.None;
      PreferredOctilinearSegmentLength = 0;
    }

    public Pen Pen { get; set; }

    public IArrow TargetArrow { get; set; }

    /// <summary>
    ///   Gets or sets the preferred length of the octilinear segments. The actual segment lengths might
    ///   be smaller due to the graph's layout. Once constrained, segments keep their current length on
    ///   graph changes, but new bends will use this property.
    /// </summary>
    public double PreferredOctilinearSegmentLength { get; set; }

    protected override Pen GetPen(IEdge edge) => Pen;

    protected override IArrow GetTargetArrow(IEdge edge) => TargetArrow;

    /// <summary>
    ///   Overridden to register custom implementations for <see cref="IOrthogonalEdgeHelper"/>
    ///   and <see cref="IBendSelectionTester"/>.
    /// </summary>
    protected override object? Lookup(IEdge edge, Type type) {
      if (type == typeof(IOrthogonalEdgeHelper)) {
        return new OrthogonalEdgeHelper(edge);
      }

      if (type == typeof(IBendSelectionTester)) {
        return new OctilinearBendSelectionTester(edge);
      }

      return base.Lookup(edge, type);
    }

    /// <summary>
    ///   Creates the visual path for the edge using octilinear segments.
    /// </summary>
    protected override GeneralPath GetPath(IEdge edge) {
      // create a new GeneralPath with the edge points
      var generalPath = new GeneralPath();
      var points = GetEdgePoints(edge);
      generalPath.MoveTo(points[0]);
      for (var i = 1; i < points.Count; i++) {
        generalPath.LineTo(points[i]);
      }
      return generalPath;
    }

    /// <summary>
    ///   Calculates the visual points that define the octilinear path of the edge.
    ///   This includes calculating the intermediate points for the diagonal segments
    ///   based on the current cutting length of each bend.
    /// </summary>
    private static List<PointD> GetEdgePoints(IEdge edge) {
      var points = new List<PointD> { edge.SourcePort.GetLocation() };

      IModelItem prevItem = edge.SourcePort;

      for (var i = 0; i < edge.Bends.Count; i++) {
        var bend = edge.Bends[i];
        IModelItem nextItem = i == edge.Bends.Count - 1 ? edge.TargetPort : edge.Bends[i + 1];

        var prevPoint = Utils.GetLocation(prevItem);
        var bendPoint = Utils.GetLocation(bend);
        var nextPoint = Utils.GetLocation(nextItem);

        var vectorToPrev = (prevPoint - bendPoint).Normalized;
        var vectorToNext = (nextPoint - bendPoint).Normalized;

        var cuttingLength = Utils.GetVisualCuttingLengthForItems(prevItem, bend, nextItem);

        points.Add(bendPoint + vectorToPrev.Normalized * cuttingLength);
        points.Add(bendPoint + vectorToNext.Normalized * cuttingLength);

        prevItem = bend;
      }

      points.Add(edge.TargetPort.GetLocation());
      return points;
    }
  }
}
