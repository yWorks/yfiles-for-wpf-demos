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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  /// <summary>
  /// An edge label model that allows placement of labels at a set of continuous positions
  /// along both sides of an edge or directly on the edge path.
  /// </summary>
  /// <remarks>
  /// The set of positions can be influenced by specifying the density value that controls
  /// the spacing between adjacent label positions.
  /// Furthermore, it's possible to specify distance values that control the distance
  /// between label and edge and between label and nodes.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class RotatedSliderEdgeLabelModel : ILabelModel, ILabelModelParameterProvider, ILabelModelParameterFinder
  {
    private const double LabelNodeDistance = 5;

    internal const double Eps = 0.0001;

    private const double EpsSegmentEnd = 0.000001;

    private const sbyte NodeSideTop = 0;

    private const sbyte NodeSideBottom = 1;

    private const sbyte NodeSideLeft = 2;

    private const sbyte NodeSideRight = 3;

    private readonly RotatedSliderParameter defaultParameter;

    /// <summary>Returns a new instance of RotatedSliderEdgeLabelModel.</summary>
    public RotatedSliderEdgeLabelModel() : this(0, 0, true, true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RotatedSliderEdgeLabelModel"/> class.
    /// </summary>
    /// <param name="distance">the distance to the edge. Set to 0 to create a centered model.</param>
    /// <param name="angle">the angle of the label's rotation in radians.</param>
    /// <param name="distanceRelativeToEdge">if set to <c>true</c> distance is interpreted relative to edge.</param>
    /// <param name="autoRotationEnabled">if set to <c>true</c> auto rotation is enabled.</param>
    public RotatedSliderEdgeLabelModel(double distance, double angle, bool distanceRelativeToEdge,
                                       bool autoRotationEnabled) {
      Distance = distance;
      DistanceRelativeToEdge = distanceRelativeToEdge;
      Angle = angle;
      AutoRotationEnabled = autoRotationEnabled;
      defaultParameter = new RotatedSliderParameter(this, 0, 0.0);
    }

    /// <summary>
    /// Specifies whether the distance to the edge is interpreted relatively to the edge's path.
    /// </summary>
    /// <value>
    /// <c>true</c> if distance to the edge is interpreted relatively to the edge's path; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// If enabled, the label is placed
    /// to the left of the edge segment (relative to the segment direction) if
    /// <see cref="Distance" /> is less than <c>0</c> and to the right of the
    /// edge segment if <see cref="Distance" /> is greater than <c>0</c>.
    /// <para>
    /// Otherwise, the label is placed below the edge segment (in
    /// geometric sense) if <see cref="Distance" /> is less than <c>0</c> and
    /// above the edge segment if <see cref="Distance" /> is greater than
    /// <c>0</c>.
    /// </para>
    /// <para>
    /// The default value is <see langword="true" />.
    /// </para>
    /// </remarks>
    /// <seealso cref="RotatedSliderEdgeLabelModel.Distance" />
    [DefaultValue(true)]
    public bool DistanceRelativeToEdge { get; set; }

    /// <summary>
    /// Specifies the distance between the label box and the edge path.
    /// </summary>
    /// <value>
    /// The distance between the label box and the edge path.
    /// </value>
    /// <remarks>
    /// The interpretation of positive/negative values depends on the property
    /// <see cref="RotatedSliderEdgeLabelModel.DistanceRelativeToEdge" />.
    /// </remarks>
    /// <seealso cref="RotatedSliderEdgeLabelModel.DistanceRelativeToEdge" />
    [DefaultValue(0.0d)]
    public double Distance { get; set; }

    /// <summary>
    /// Specifies whether edge labels are automatically rotated according to the angle of
    /// the corresponding reference edge segment.
    /// </summary>
    /// <value>
    /// <c>true</c> if edge labels are automatically rotated; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// By default, this feature is enabled.
    /// </remarks>
    [DefaultValue(true)]
    public bool AutoRotationEnabled { get; set; }

    /// <summary>
    /// Specifies the rotation angle of all labels with this model.
    /// </summary>
    /// <value>
    /// The rotation angle of all labels with this model.
    /// </value>
    [DefaultValue(0.0d)]
    public double Angle { get; set; }

    ///<inheritdoc/>
    public object Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider)) {
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        return this;
      }
      return null;
    }

    /// <summary>
    /// A model parameter that encodes the default position of this model's
    /// allowed edge label positions.
    /// </summary>
    /// <remarks>
    /// Returns a model parameter that encodes the default position of this model's
    /// allowed edge label positions.
    /// </remarks>
    public ILabelModelParameter CreateDefaultParameter() {
      return defaultParameter;
    }

    ///<inheritdoc/>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      RotatedSliderParameter rsp = parameter as RotatedSliderParameter ??
                                   (RotatedSliderParameter) CreateDefaultParameter();
      int segmentNo = rsp.Segment;
      double ratio = rsp.Ratio;
      OrientedRectangle bounds = new OrientedRectangle(0, 0, label.PreferredSize.Width, label.PreferredSize.Height);
      bounds.Angle = Angle;

      IEdge edge = (IEdge) label.Owner;
      INode sourceNode = edge.SourcePort.Owner as INode;
      INode targetNode = edge.TargetPort.Owner as INode;
      if (sourceNode == null || targetNode == null) {
        throw new NullReferenceException("Source or target node is null!");
      }
      var sourceNodeLayout = sourceNode.Layout.ToRectD();
      var targetNodeLayout = targetNode.Layout.ToRectD();

      // get edge path
      var generalPath = edge.Style.Renderer.GetPathGeometry(edge, edge.Style).GetPath();
      PointD[] path = GetPathPoints(generalPath);
      //check path
      if (path.Length < 2 || (path.Length == 2 && path[0].DistanceTo(path[path.Length - 1]) < Eps)) {
        if (path.Length < 2) {
          RectD bBox = bounds.GetBounds();
          bounds.SetCenter(new PointD(sourceNodeLayout.X + bBox.Width * 0.5, sourceNodeLayout.Y + bBox.Height * 0.5));
        } else {
          bounds.SetCenter(path[0]);
        }
        return bounds;
      }
      // get interesting line segment
      int index = (segmentNo < 0) ? (path.Length - 1 + segmentNo) : segmentNo;
      LineSegment segment = GetLineSegment(path, index);

      if (segment == null) {
        RectD bBox = bounds.GetBounds();
        bounds.SetCenter(new PointD(sourceNodeLayout.X + bBox.Width * 0.5, sourceNodeLayout.Y + bBox.Height * 0.5));
        return bounds;
      }
      PointD p1 = segment.FirstEndPoint;
      PointD p2 = segment.SecondEndPoint;
      if (segment.GetLength() == 0) {
        // something bad happened, try to fix it!
        var spl = edge.SourcePort.GetLocation();
        var tpl = edge.TargetPort.GetLocation();
        double dx = sourceNodeLayout.X + sourceNodeLayout.Width * 0.5d + spl.X -
                    (targetNodeLayout.X + targetNodeLayout.Width * 0.5d + tpl.X);
        double dy = sourceNodeLayout.Y + sourceNodeLayout.Height * 0.5d + spl.Y -
                    (targetNodeLayout.Y + targetNodeLayout.Height * 0.5d + tpl.Y);
        if (dx == 0 && dy == 0) {
          // something even worse happened, try to fix it!
          p2 = new PointD(p1.X + EpsSegmentEnd, p1.Y);
        } else {
          double dl = Math.Sqrt(dx * dx + dy * dy);
          segment.SecondEndPoint = p2 = new PointD(p1.X + EpsSegmentEnd * dx / dl, p1.Y + EpsSegmentEnd * dy / dl);
        }
      }
      //determine rotation angle
      if (AutoRotationEnabled) {
        bounds.Angle = CalculateRotationAngle(segment.ToVector(), Angle);
      }

      var absolutePlacement = 0 > ratio || ratio > 1;

      //get oriented box representing candidate at ratio 0
      OrientedRectangle boundsR0 = new OrientedRectangle(bounds);
      PlaceAtPoint(boundsR0, segment, p1, Distance);
      if (index == 0 && DoIntersect(sourceNodeLayout, boundsR0)) {
        // placement depends on source node
        PlaceAtSource(boundsR0, segment, sourceNodeLayout, Distance);
      }
      //get oriented box representing candidate at ratio 1
      OrientedRectangle boundsR1 = new OrientedRectangle(bounds);
      PlaceAtPoint(boundsR1, segment, p2, Distance);
      if (index >= path.Length - 2 && DoIntersect(targetNodeLayout, boundsR1)) {
        // placement depends on target node
        PlaceAtTarget(boundsR1, segment, targetNodeLayout, Distance);
      }
      //get oriented box representing candidate at ratio "ratio"
      PointD anchor;
      if (absolutePlacement) {
        // not between 0 and 1: absolute length
        var ddx = boundsR1.AnchorX - boundsR0.AnchorX;
        var ddy = boundsR1.AnchorY - boundsR0.AnchorY;
        var segLength = Math.Sqrt(ddx * ddx + ddy * ddy);
        if (segLength < 1) {
          var vec = segment.ToVector();
          ddx = vec.X;
          ddy = vec.Y;
          segLength = Math.Sqrt(ddx * ddx + ddy * ddy);
        }
        double x, y;
        if (ratio < 0) {
          var normalizedRatio = segLength != 0 ? ratio / segLength : 0;
          x = boundsR0.AnchorX + normalizedRatio * ddx;
          y = boundsR0.AnchorY + normalizedRatio * ddy;
        } else {
          var normalizedRatio = segLength != 0 ? (ratio - 1) / segLength : 0;
          x = boundsR1.AnchorX + normalizedRatio * ddx;
          y = boundsR1.AnchorY + normalizedRatio * ddy;
        }
        anchor = new PointD(x, y);
      } else {
        anchor = new PointD(boundsR0.AnchorX * (1 - ratio) + boundsR1.AnchorX * ratio,
          boundsR0.AnchorY * (1 - ratio) + boundsR1.AnchorY * ratio);
      }
      bounds.Anchor = anchor;
      return bounds;
    }

    /// <inheritdoc/>
    IEnumerable<ILabelModelParameter> ILabelModelParameterProvider.GetParameters(ILabel label, ILabelModel model) {
      List<ILabelModelParameter> candList = new List<ILabelModelParameter>();
      IEdge edge = (IEdge) label.Owner;
      var sourceNodeLayout = GetNodeLayout(edge.SourcePort);
      var targetNodeLayout = GetNodeLayout(edge.TargetPort);

      // get edge path
      var generalPath = edge.Style.Renderer.GetPathGeometry(edge, edge.Style).GetPath();
      // get array of path points
      PointD[] path = GetPathPoints(generalPath);

      // if something is wrong with the path generate one trivial position)
      if (path.Length < 2 || (path.Length == 2 && path[0].DistanceTo(path[1]) < Eps)) {
        candList.Add(CreateDefaultParameter());
        return candList;
      }

      var boundsR0 = new OrientedRectangle(0, 0, 0, 0);
      var boundsR1 = new OrientedRectangle(0, 0, 0, 0);

      // iterate over all edge segments
      for (int i = 0; i < path.Length - 1; i++) {
        PointD p1 = path[i];
        PointD p2 = path[i + 1];
        SetFirstAndLastBoxOnSegment(p1, p2, i == 0, i == (path.Length - 2), label, sourceNodeLayout, targetNodeLayout,
          boundsR0, boundsR1);

        //determine rotation angle
        LineSegment segment = new LineSegment(p1, p2);
        int segmentId = (i > (path.Length - 2) / 2) ? (i + 1 - path.Length) : i;
        bool isSingleSegment = path.Length == 2;

        RotatedSliderParameter paramR0 = new RotatedSliderParameter(this, segmentId, 0);
        candList.Add(paramR0);

        RotatedSliderParameter paramR1;
        // in case of edges with only one segment, treat second parameter as 'from target'
        if (isSingleSegment) {
          paramR1 = new RotatedSliderParameter(this, -1, 1);
        } else {
          paramR1 = new RotatedSliderParameter(this, segmentId, 1);
        }
        candList.Add(paramR1);
        AddIntermediateCandidates(candList, boundsR0, boundsR1, segment, segmentId, isSingleSegment);
      }

      return candList;
    }

    /// <summary>
    /// Gets an array of <see cref="PointD">points</see> representing the path points of the <see cref="GeneralPath" />.
    /// </summary>
    internal static PointD[] GetPathPoints(GeneralPath path) {
      if (path == null) {
        return new PointD[0];
      }

      List<PointD> points = new List<PointD>();

      var pathCursor = path.CreateCursor();
      double[] coords = new double[6];
      double moveX = 0, moveY = 0;
      while (pathCursor.MoveNext()) {
        PathType current = pathCursor.GetCurrent(coords);
        double lastX;
        double lastY;
        switch (current) {
          case PathType.MoveTo:
            lastX = moveX = coords[0];
            lastY = moveY = coords[1];
            break;
          case PathType.LineTo:
            lastX = coords[0];
            lastY = coords[1];
            break;
          case PathType.QuadTo:
            lastX = coords[2];
            lastY = coords[3];
            break;
          case PathType.CubicTo:
            lastX = coords[4];
            lastY = coords[5];
            break;
          case PathType.Close:
            lastX = moveX;
            lastY = moveY;
            break;
          default:
            throw new Exception();
        }
        points.Add(new PointD(lastX, lastY));
      }
      return points.ToArray();
    }

    /// <summary>
    /// Returns an empty context.
    /// </summary>
    /// <param name="label">The label to use in the context.</param>
    /// <param name="parameter">The parameter to use for the label in the context.</param>
    /// <returns>An empty context.</returns>
    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    private void PlaceAtPoint(OrientedRectangle rect, LineSegment segment, PointD p, double dist) {
      PointD segmentVector = segment.ToVector();
      if (IsSideSliderModel() == false) {
        rect.SetCenter(new PointD(p.X, p.Y));
      } else {
        //calc offset vector
        PointD offsetVec = OrthoNormal(segmentVector);
        if (DistanceRelativeToEdge) {
          if (IsNegative(dist)) {
            offsetVec = new PointD(-offsetVec.X, -offsetVec.Y);
          }
        } else {
          double offsetAngle = CalculateAngle(ZeroVector, offsetVec);
          if (offsetAngle == 2 * Math.PI) {
            offsetAngle = 0;
          }
          if ((offsetAngle >= Math.PI && IsNegative(dist)) || (offsetAngle < Math.PI && IsPositive(dist))) {
            offsetVec = new PointD(-offsetVec.X, -offsetVec.Y);
          }
        }
        //calculate initial position
        offsetVec *= Math.Abs(dist) + rect.Width + rect.Height;
        PointD rectCenter = p + offsetVec;
        rect.SetCenter(new PointD(rectCenter.X, rectCenter.Y));
        //calc distance to segment
        PointD[] corners = GetCorners(rect);
        double minDist = double.MaxValue;
        for (int i = 0; i < corners.Length; i++) {
          minDist = Math.Min(minDist, DistanceToLine(corners[i], segment.FirstEndPoint, segment.SecondEndPoint));
        }
        //adjust distance to node
        PointD corrVec = new PointD(-offsetVec.X, -offsetVec.Y).Normalized;
        corrVec *= minDist - Math.Abs(dist);
        rectCenter = rectCenter + corrVec;
        rect.SetCenter(new PointD(rectCenter.X, rectCenter.Y));
      }
    }

    private double DistanceToLine(PointD p, PointD l1, PointD l2) {
      double dx = l2.X - l1.X;
      double dy = l2.Y - l1.Y;

      double tdx = p.X - l1.X;
      double tdy = p.Y - l1.Y;

      //calc projection length
      double tmp = tdx * dx + tdy * dy;
      double squaredProjLength = tmp * tmp / (dx * dx + dy * dy);
      double squaredLength = tdx * tdx + tdy * tdy - squaredProjLength;
      return squaredLength < 0 ? 0 : Math.Sqrt(squaredLength);
    }


    private void PlaceAtTarget(OrientedRectangle rect, LineSegment segment, IRectangle nodeLayout, double dist) {
      PlaceAtEndpoint(rect, segment.SecondEndPoint, segment.FirstEndPoint, nodeLayout, false, dist);
    }

    private void PlaceAtSource(OrientedRectangle rect, LineSegment segment, IRectangle nodeLayout, double dist) {
      PlaceAtEndpoint(rect, segment.FirstEndPoint, segment.SecondEndPoint, nodeLayout, true, dist);
    }

    private void PlaceAtEndpoint(OrientedRectangle bounds, PointD p1, PointD p2, IRectangle nodeLayout, bool atSource,
                                 double dist) {
      RectD nodeBox = nodeLayout.ToRectD();
      //the bounds of the endpoint
      LineSegment lineSegment = new LineSegment(p1, p2);
      sbyte side = DetermineNodeSide(p1, p2);
      //the side where to place the label
      PointD anchor = PlaceOnSide(side, lineSegment, bounds, nodeBox, atSource, dist);
      bounds.Anchor = anchor;
      double distToNode = GetDistance(nodeBox, bounds);
      //distance is used to determine the quality of the label position
      RectD rectBox = bounds.GetBounds();
      bool tryAlternative = (distToNode < 0.5 * LabelNodeDistance || distToNode > 1.2 * LabelNodeDistance) &&
                            // label is not good (too close or too far)
                            (((side == NodeSideTop || side == NodeSideBottom) && Math.Abs(p1.X - p2.X) > Eps &&
                              // edge ends above or below
                              (p2.X < nodeBox.X - LabelNodeDistance - rectBox.Width * 0.5 || // edge ends to the left 
                               p2.X > (nodeBox.X + nodeBox.Width + LabelNodeDistance + rectBox.Width * 0.5))) ||
                             // or to the right
                             ((side == NodeSideLeft || side == NodeSideRight) && Math.Abs(p1.Y - p2.Y) > Eps &&
                              // at the sides
                              (p2.Y < nodeBox.Y - LabelNodeDistance - rectBox.Height * 0.5 || // above
                               p2.Y > (nodeBox.Y + nodeBox.Height + LabelNodeDistance + rectBox.Height * 0.5))));
      // below
      //enough place for the alternative candidate?
      if (tryAlternative) {
        //try to place label at another side
        sbyte alternativeSide = DetermineAlternativeNodeSide(p1, p2);
        PointD anchorAlternative = PlaceOnSide(alternativeSide, lineSegment, bounds, nodeBox, atSource, dist);
        bounds.Anchor = anchorAlternative; // try alternative
        double distToNodeAlternative = GetDistance(nodeBox, bounds); // measure alternative
        // see if the situation got worse
        if (distToNodeAlternative < 0.5 * LabelNodeDistance ||
            // if the alternative is closer than a min distance, discard it
            (distToNodeAlternative > distToNode && distToNode > 0.5 * LabelNodeDistance)) {
          // or the first was too far away, and the alternative is even farther away 
          // => distToNode > 1.2 * LabelNodeDistance
          bounds.Anchor = anchor; // reset to previous (non-alternative)
        }
      }
    }

    #region distance calculation

    /// <summary>
    /// Calculates the distance between a <see cref="RectD">rectangle</see> and an 
    /// <see cref="OrientedRectangle">oriented rectangle</see>.
    /// </summary>
    /// <param name="r">the rectangle</param>
    /// <param name="orientedRect">the oriented rectangle</param>
    /// <returns>the distance</returns>
    private static double GetDistance(RectD r, IOrientedRectangle orientedRect) {
      if (IsParaxial(orientedRect)) {
        return DistanceToRect(r, GetBounds(orientedRect));
      }
      return r.Intersects(orientedRect, Eps) ? 0.0 : GetDistance(r, GetCorners(orientedRect));
    }

    private static bool IsParaxial(IOrientedRectangle rect) {
      // ReSharper disable CompareOfFloatsByEqualityOperator
      return (rect.UpX == 0 && (rect.UpY == -1 || rect.UpY == 1))
             || (rect.UpY == 0 && (rect.UpX == -1 || rect.UpX == 1));
      // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    private static double DistanceToRect(RectD r1, RectD r2) {
      if (r1.Intersects(r2)) {
        return 0.0;
      }
      var distVertical = OrthogonalDistanceTo(r1, r2, false);
      var distHorizontal = OrthogonalDistanceTo(r1, r2, true);
      return Math.Sqrt(distVertical * distVertical + distHorizontal * distHorizontal);
    }

    private static double OrthogonalDistanceTo(RectD rect1, RectD rect2, bool horizontal) {
      double rect1Min = horizontal ? rect1.X : rect1.Y;
      double rect1Max = horizontal ? rect1.X + rect1.Width : rect1.Y + rect1.Height;
      double rect2Min = horizontal ? rect2.X : rect2.Y;
      double rect2Max = horizontal ? rect2.X + rect2.Width : rect2.Y + rect2.Height;
      if (rect2Max < rect1Min) {
        // complete rectangle at lower coordinate
        return rect2Max - rect1Min;
      } else if (rect1Max < rect2Min) {
        // complete rectangle at higher coordinate
        return rect2Min - rect1Max;
      } else {
        // intersection of elements
        return 0.0;
      }
    }

    private static RectD GetBounds(IOrientedRectangle rect) {
      if (rect.UpX == 0 && rect.UpY == -1) {
        return new RectD(rect.AnchorX, rect.AnchorY - rect.Height, rect.Width, rect.Height);
      } else if (rect.UpX == 0 && rect.UpY == 1) {
        return new RectD(rect.AnchorX - rect.Width, rect.AnchorY, rect.Width, rect.Height);
      } else if (rect.UpX == 1 && rect.UpY == 0) {
        return new RectD(rect.AnchorX, rect.AnchorY, rect.Height, rect.Width);
      } else if (rect.UpX == -1 && rect.UpY == 0) {
        return new RectD(rect.AnchorX - rect.Height, rect.AnchorY - rect.Width, rect.Height, rect.Width);
      } else {
        return rect.GetBounds();
      }
    }

    private static double GetDistance(RectD r, PointD[] polygon) {
      PointD upperLeft = r.GetTopLeft();
      PointD lowerLeft = r.GetBottomLeft();
      PointD lowerRight = r.GetBottomRight();
      PointD upperRight = r.GetTopRight();
      LineSegment[] borderSegments =
      {
        new LineSegment(upperLeft, lowerLeft), new LineSegment(upperRight, lowerRight),
        new LineSegment(upperLeft, upperRight), new LineSegment(lowerLeft, lowerRight)
      };
      double dist = double.MaxValue;
      for (int i = 0; i < polygon.Length; i++) {
        LineSegment line = new LineSegment(polygon[i], polygon[(i + 1) % polygon.Length]);
        for (int j = 0; j < borderSegments.Length; j++) {
          dist = Math.Min(dist, line.GetDistance(borderSegments[j]));
        }
      }
      return dist;
    }

    #endregion

    private void AddIntermediateCandidates(List<ILabelModelParameter> candList, OrientedRectangle boundsR0,
                                           OrientedRectangle boundsR1, LineSegment edgeSegment, int segmentId,
                                           bool singleSegment) {
      //determine the two segments of boundsR0 that are adjacent to the given edge segment
      PointD[] boundsR0Corner = GetCorners(boundsR0);
      int closestPointR0Index = edgeSegment.GetClosestPointIndex(boundsR0Corner);
      PointD otherPoint1 = (closestPointR0Index > 0) ? boundsR0Corner[closestPointR0Index - 1] : boundsR0Corner[3];
      LineSegment boundsR0Segment1 = new LineSegment(boundsR0Corner[closestPointR0Index], otherPoint1);
      PointD otherPoint2 = (closestPointR0Index < 3) ? boundsR0Corner[closestPointR0Index + 1] : boundsR0Corner[0];
      LineSegment boundsR0Segment2 = new LineSegment(boundsR0Corner[closestPointR0Index], otherPoint2);

      //check on which of both segments of boundsR0 (boundsR1) adjacent to the edge segment consecutive candidates touch each other
      PointD[] boundsR1Corner = GetCorners(boundsR1);

      PointD vectorSegment1 = boundsR0Segment1.ToVector();
      LineSegment segment1R0 = new LineSegment(boundsR0Corner[closestPointR0Index],
        boundsR0Corner[closestPointR0Index] + vectorSegment1);
      LineSegment segment1R1 = new LineSegment(boundsR1Corner[closestPointR0Index],
        boundsR1Corner[closestPointR0Index] + vectorSegment1);
      LineSegment orthogonalToSegment1 = new LineSegment(boundsR0Corner[closestPointR0Index],
        boundsR0Corner[closestPointR0Index] + OrthoNormal(vectorSegment1));
      PointD segment1R0Crossing;
      FindLineIntersection(segment1R0, orthogonalToSegment1, out segment1R0Crossing);
      PointD segment1R1Crossing;
      FindLineIntersection(segment1R1, orthogonalToSegment1, out segment1R1Crossing);
      double distSegment1 = segment1R0Crossing.DistanceTo(segment1R1Crossing);
      int intermediateCandidatesCountSegment1 =
          (int) Math.Floor((distSegment1 - boundsR0Segment2.GetLength()) / boundsR0Segment2.GetLength());

      PointD vectorSegment2 = boundsR0Segment2.ToVector();
      LineSegment segment2R0 = new LineSegment(boundsR0Corner[closestPointR0Index],
        boundsR0Corner[closestPointR0Index] + vectorSegment2);
      LineSegment segment2R1 = new LineSegment(boundsR1Corner[closestPointR0Index],
        boundsR1Corner[closestPointR0Index] + vectorSegment2);
      LineSegment orthogonalToSegment2 = new LineSegment(boundsR0Corner[closestPointR0Index],
        boundsR0Corner[closestPointR0Index] + OrthoNormal(vectorSegment2));
      PointD segment2R0Crossing;
      FindLineIntersection(segment2R0, orthogonalToSegment2, out segment2R0Crossing);
      PointD segment2R1Crossing;
      FindLineIntersection(segment2R1, orthogonalToSegment2, out segment2R1Crossing);
      double distSegment2 = segment2R0Crossing.DistanceTo(segment2R1Crossing);

      int intermediateCandidatesCountSegment2 =
          (int) Math.Floor((distSegment2 - boundsR0Segment1.GetLength()) / boundsR0Segment1.GetLength());
      bool useFirstSegment = (intermediateCandidatesCountSegment1 >= intermediateCandidatesCountSegment2);
      double dist = useFirstSegment ? distSegment1 : distSegment2;

      LineSegment segment = useFirstSegment ? boundsR0Segment1 : boundsR0Segment2;
      LineSegment otherSegment = useFirstSegment ? boundsR0Segment2 : boundsR0Segment1;

      int intermediateCandidatesCount = useFirstSegment
        ? intermediateCandidatesCountSegment1
        : intermediateCandidatesCountSegment2;
      //distance between two adjacent candidates
      double candidateDist = (dist - (intermediateCandidatesCount + 1) * otherSegment.GetLength()) /
                             (intermediateCandidatesCount + 1);
      //calculate offset vector (we obtain the anchor of a candidate by adding this vector to the anchor of the previous candidate)
      PointD offsetVec = edgeSegment.ToVector().Normalized;
      //points to the location of the next candidate (next with respect to the vector's direction)
      PointD otherSegmentVec = otherSegment.ToVector().Normalized;
      otherSegmentVec *= candidateDist + otherSegment.GetLength();
      PointD boundsR0Anchor = boundsR0.Anchor.ToPointD();
      PointD tmpLocation = boundsR0Anchor + otherSegmentVec;
      //we have to project this location on the line which is parallel to the edge segment and contains the anchor point of boundsR0
      PointD nextAnchor;
      LineSegment projLine = new LineSegment(boundsR0Anchor, boundsR0Anchor + edgeSegment.ToVector());
      LineSegment orthoTmpLocationLine = new LineSegment(tmpLocation, tmpLocation + segment.ToVector());
      bool nextAnchorFound = FindLineIntersection(orthoTmpLocationLine, projLine, out nextAnchor);
      if (!nextAnchorFound) {
        nextAnchor = tmpLocation;
      }
      offsetVec *= boundsR0Anchor.DistanceTo(nextAnchor);
      //now, we can place the intermediate candidates
      PointD lastAnchor = boundsR0Anchor;
      double boundsR0R1Dist = boundsR0.Anchor.DistanceTo(boundsR1.Anchor);
      for (int i = 0; i < intermediateCandidatesCount; i++) {
        OrientedRectangle bounds = new OrientedRectangle(boundsR0);
        PointD anchor = lastAnchor + offsetVec;
        double boundsR0BoundsDist = boundsR0.Anchor.DistanceTo(anchor);
        bounds.Anchor = anchor;
        RotatedSliderParameter param;
        if (singleSegment && i > intermediateCandidatesCount * 0.5d) {
          param = new RotatedSliderParameter(this, -1, boundsR0BoundsDist / boundsR0R1Dist);
        } else {
          param = new RotatedSliderParameter(this, segmentId, boundsR0BoundsDist / boundsR0R1Dist);
        }
        candList.Add(param);
        lastAnchor = anchor;
      }
    }

    /// <summary>
    /// Finds the intersection point between two segments that 
    /// are treated as if they were infinite straight lines.
    /// </summary>
    /// /// <param name="segment1">The first segment.</param>
    /// /// <param name="segment2">The second segment.</param>
    /// <param name="intersectionPoint">The intersection point, if an intersection has been found, or (0/0).</param>
    /// <returns><see langword="true"/> if and intersection point has been found.</returns>
    private static bool FindLineIntersection(LineSegment segment1, LineSegment segment2, out PointD intersectionPoint) {
      return FindLineIntersection(segment1.FirstEndPoint, segment1.SecondEndPoint, segment2.FirstEndPoint,
        segment2.SecondEndPoint - segment2.FirstEndPoint, out intersectionPoint);
    }

    /// <summary>
    /// Finds the intersection point between two infinite straight lines, 
    /// one defined by two points, one defined by an anchor point and a 
    /// direction vector.
    /// </summary>
    /// <param name="l1">The first point of the first line.</param>
    /// <param name="l2">The second point of the first line.</param>
    /// <param name="anchor">The anchor point of the second line.</param>
    /// <param name="rayDirection">The direction vector of the second line.</param>
    /// <param name="intersectionPoint">The intersection point, if an intersection has been found, or (0/0).</param>
    /// <returns></returns>
    private static bool FindLineIntersection(PointD l1, PointD l2, PointD anchor, PointD rayDirection,
                                             out PointD intersectionPoint) {
      double lx1 = l1.X;
      double ly1 = l1.Y;
      double lx2 = l2.X;
      double ly2 = l2.Y;
      double anchorX = anchor.X;
      double anchorY = anchor.Y;
      double rayX = rayDirection.X;
      double rayY = rayDirection.Y;

      double dx1 = lx2 - lx1;
      double dy1 = ly2 - ly1;
      double denominator = ((rayY * dx1) - (rayX * dy1));
      if (denominator != 0) {
        double b = ((dx1 * (ly1 - anchorY)) - (dy1 * (lx1 - anchorX))) / denominator;
        double ix = anchorX + rayX * b;
        double iy = anchorY + rayY * b;
        intersectionPoint = new PointD(ix, iy);
        return true;
      }
      intersectionPoint = PointD.Origin;
      return false;
    }

    private static sbyte DetermineAlternativeNodeSide(PointD segmentStartPoint, PointD segmentEndPoint) {
      PointD vec = segmentEndPoint - segmentStartPoint;
      return Math.Abs(vec.X) > Math.Abs(vec.Y)
        ? (vec.Y > 0 ? NodeSideBottom : NodeSideTop)
        : (vec.X < 0 ? NodeSideLeft : NodeSideRight);
    }

    private PointD PlaceOnSide(sbyte side, LineSegment lineSegment, OrientedRectangle rectOrig, RectD nodeLayout,
                               bool atSource, double dist) {
      bool isCenterSlider = !IsSideSliderModel();
      OrientedRectangle rectClone = new OrientedRectangle(rectOrig);
      PointD p1 = lineSegment.FirstEndPoint;
      PointD p2 = lineSegment.SecondEndPoint;
      PointD lineSegmentVector = lineSegment.ToVector();
      double segmentAngle = CalculateAngle(lineSegmentVector, ZeroVector);
      RectD bBox = rectClone.GetBounds();
      if (side == NodeSideTop) {
        bool leftOfEdge;
        //whether the label should be placed to the left of the edge segment (in geometric sense)
        if (DistanceRelativeToEdge) {
          leftOfEdge = (atSource && IsNegative(dist)) || (!atSource && IsPositive(dist));
        } else {
          //if true, place label to the left of segment p1, p2
          leftOfEdge = (IsPositive(dist) && segmentAngle <= Math.PI * 0.5) ||
                       (IsNegative(dist) && segmentAngle > Math.PI * 0.5);
        }
        double desiredCenterY = nodeLayout.Y - LabelNodeDistance - bBox.Height * 0.5;
        if (desiredCenterY < p2.Y) {
          desiredCenterY = p2.Y;
        }
        //candidate should be placed near the segment
        double initialXPos = leftOfEdge ? Math.Min(p1.X, p2.X) - bBox.Width : Math.Max(p1.X, p2.X) + bBox.Width;
        rectClone.SetCenter(new PointD(initialXPos, desiredCenterY));
        //initial placement of rect
        //calc placement
        PointD closestPoint = isCenterSlider
          ? rectClone.GetCenter()
          : lineSegment.GetClosestPoint(GetCorners(rectClone));
        PointD intersectionPoint;
        FindLineIntersection(p1, p2, closestPoint, new PointD(1, 0), out intersectionPoint);
        rectClone.MoveBy(new PointD(intersectionPoint.X - closestPoint.X, 0));
        if (!isCenterSlider) {
          //include edge to label distance
          double distanceOffset = CalcProjDistance(lineSegment, new PointD(1, 0), Math.Abs(dist));
          if (!double.IsInfinity(distanceOffset)) {
            if (leftOfEdge) {
              distanceOffset = -distanceOffset;
            }
            rectClone.MoveBy(new PointD(distanceOffset, 0));
          }
        }
      } else {
        if (side == NodeSideBottom) {
          bool leftOfEdge;
          //whether the label should be placed to the left of the edge segment (in geometric sense)
          if (DistanceRelativeToEdge) {
            leftOfEdge = (atSource && IsPositive(dist)) || (!atSource && IsNegative(dist));
          } else {
            //if true, place label to the left of segment p1, p2
            leftOfEdge = (IsPositive(dist) && segmentAngle <= Math.PI * 1.5) ||
                         (IsNegative(dist) && segmentAngle > Math.PI * 1.5);
          }
          double desiredCenterY = nodeLayout.Y + nodeLayout.Height + LabelNodeDistance + bBox.Height * 0.5;
          if (desiredCenterY > p2.Y) {
            desiredCenterY = p2.Y;
          }
          //candidate should be placed near the segment
          double initialXPos = leftOfEdge ? Math.Min(p1.X, p2.X) - bBox.Width : Math.Max(p1.X, p2.X) + bBox.Width;
          rectClone.SetCenter(new PointD(initialXPos, desiredCenterY));
          //initial placement of rect
          //calc placement
          PointD closestPoint = isCenterSlider
            ? rectClone.GetCenter()
            : lineSegment.GetClosestPoint(GetCorners(rectClone));
          PointD intersectionPoint;
          FindLineIntersection(p1, p2, closestPoint, new PointD(1, 0), out intersectionPoint);
          rectClone.MoveBy(new PointD(intersectionPoint.X - closestPoint.X, 0));
          if (!isCenterSlider) {
            //include edge to label distance
            double distanceOffset = CalcProjDistance(lineSegment, new PointD(1, 0), Math.Abs(dist));
            if (!double.IsInfinity(distanceOffset)) {
              if (!leftOfEdge) {
                distanceOffset = -distanceOffset;
              }
              rectClone.MoveBy(new PointD(distanceOffset, 0));
            }
          }
        } else {
          if (side == NodeSideLeft) {
            bool belowEdge;
            //whether the label should be placed below the edge segment (in geometric sense)
            if (DistanceRelativeToEdge) {
              belowEdge = (atSource && IsNegative(dist)) || (!atSource && IsPositive(dist));
            } else {
              belowEdge = IsNegative(dist);
            }
            double desiredCenterX = nodeLayout.X - LabelNodeDistance - bBox.Width * 0.5;
            if (desiredCenterX < p2.X) {
              desiredCenterX = p2.X;
            }
            double initialYPos = belowEdge ? Math.Max(p1.Y, p2.Y) + bBox.Height : Math.Min(p1.Y, p2.Y) - bBox.Height;
            rectClone.SetCenter(new PointD(desiredCenterX, initialYPos));
            //initial placement of rect
            //calc placement
            PointD closestPoint = isCenterSlider
              ? rectClone.GetCenter()
              : lineSegment.GetClosestPoint(GetCorners(rectClone));
            PointD intersectionPoint;
            FindLineIntersection(p1, p2, closestPoint, new PointD(0, 1), out intersectionPoint);
            rectClone.MoveBy(new PointD(0, intersectionPoint.Y - closestPoint.Y));
            if (!isCenterSlider) {
              //include edge to label distance
              double distanceOffset = CalcProjDistance(lineSegment, new PointD(0, 1), Math.Abs(dist));
              if (!double.IsInfinity(distanceOffset)) {
                if (belowEdge) {
                  distanceOffset = -distanceOffset;
                }
                rectClone.MoveBy(new PointD(0, distanceOffset));
              }
            }
          } else {
            if (side == NodeSideRight) {
              bool belowEdge;
              //whether the label should be placed below the edge segment (in geometric sense)
              if (DistanceRelativeToEdge) {
                belowEdge = (atSource && IsPositive(dist)) || (!atSource && IsNegative(dist));
              } else {
                belowEdge = IsNegative(dist);
              }
              double desiredCenterX = nodeLayout.X + nodeLayout.Width + LabelNodeDistance + bBox.Width * 0.5;
              if (desiredCenterX > p2.X) {
                desiredCenterX = p2.X;
              }
              double initialYPos = belowEdge ? Math.Max(p1.Y, p2.Y) + bBox.Height : Math.Min(p1.Y, p2.Y) - bBox.Height;
              rectClone.SetCenter(new PointD(desiredCenterX, initialYPos));
              //initial placement of rect
              //calc placement
              PointD closestPoint = isCenterSlider
                ? rectClone.GetCenter()
                : lineSegment.GetClosestPoint(GetCorners(rectClone));
              PointD intersectionPoint;
              FindLineIntersection(p1, p2, closestPoint, new PointD(0, 1), out intersectionPoint);
              rectClone.MoveBy(new PointD(0, intersectionPoint.Y - closestPoint.Y));
              if (!isCenterSlider) {
                //include edge to label distance
                double distanceOffset = CalcProjDistance(lineSegment, new PointD(0, 1), Math.Abs(dist));
                if (!double.IsInfinity(distanceOffset)) {
                  if (!belowEdge) {
                    distanceOffset = -distanceOffset;
                  }
                  rectClone.MoveBy(new PointD(0, distanceOffset));
                }
              }
            }
          }
        }
      }
      //guarantee that the source/target candidates do not pass the center position
      if (rectClone.GetCenter().DistanceTo(p1) > rectClone.GetCenter().DistanceTo(p2)) {
        PointD center = new PointD((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        if (atSource) {
          PlaceAtPoint(rectClone, lineSegment, center, dist);
        } else {
          LineSegment original = new LineSegment(lineSegment.SecondEndPoint, lineSegment.FirstEndPoint);
          //current segment was reversed
          PlaceAtPoint(rectClone, original, center, dist);
        }
      }
      return rectClone.Anchor.ToPointD();
    }

    private static double CalcProjDistance(LineSegment edgeSegment, PointD alignSegment, double dist) {
      double segmentAngle = CalculateAngle(edgeSegment.ToVector(), alignSegment);
      if (segmentAngle == Math.PI * 0.5) {
        return dist;
      } else {
        if (segmentAngle > Math.PI * 0.5) {
          segmentAngle = Math.PI - segmentAngle;
        }
      }
      return dist / Math.Sin(segmentAngle);
    }

    private static sbyte DetermineNodeSide(PointD p1, PointD p2) {
      PointD vec = p2 - p1;
      if (Math.Abs(vec.X) > Math.Abs(vec.Y)) {
        return vec.X > 0 ? NodeSideRight : NodeSideLeft;
      } else {
        return vec.Y < 0 ? NodeSideTop : NodeSideBottom;
      }
    }

    private static bool RightOf(PointD v1, PointD v2) {
      return (v1.X * v2.Y - v1.Y * v2.X > 0);
    }

    private static PointD ZeroVector {
      get { return new PointD(1, 0); }
    }

    private static double CalculateRotationAngle(PointD vector, double angle) {
      return NormalizeAngle(CalculateAngle(vector, ZeroVector) - angle);
    }

    /// <summary>
    /// Calculates the angle between two vectors in radians.
    /// </summary>
    private static double CalculateAngle(PointD v1, PointD v2) {
      double cosA = v1.ScalarProduct(v2) / (v1.VectorLength * v2.VectorLength);
      // due to rounding errors the above calculated value cosA might be out of
      // the range of theoretically possible (and for Math.acos(double) required)
      // value range [-1, 1]
      double a;
      if (cosA > 1) {
        a = Math.Acos(1);
      } else {
        if (cosA < -1) {
          a = Math.Acos(-1);
        } else {
          a = Math.Acos((Math.Min(1, cosA)));
        }
      }
      if (!RightOf(v1, v2)) {
        a = 2 * Math.PI - a;
      }
      return a;
    }

    /// <summary>
    /// Calculates the orthogonal vector.
    /// </summary>
    internal static PointD OrthoNormal(PointD vector) {
      double length = vector.VectorLength;
      return new PointD(-vector.Y / length, vector.X / length);
    }

    /// <summary>
    /// Calculates the corner points of a oriented rectangle
    /// </summary>
    private static PointD[] GetCorners(IOrientedRectangle rect) {
      double w = rect.Width;
      double h = rect.Height;
      double x1 = rect.AnchorX;
      double y1 = rect.AnchorY;
      double upX = rect.UpX;
      double upY = rect.UpY;
      double x2 = x1 + upX * h;
      double y2 = y1 + upY * h;
      double x3 = x2 - upY * w;
      double y3 = y2 + upX * w;
      double x4 = x1 - upY * w;
      double y4 = y1 + upX * w;
      return new[] { new PointD(x1, y1), new PointD(x2, y2), new PointD(x3, y3), new PointD(x4, y4) };
    }

    /// <summary>
    /// Checks if a value is positive.
    /// </summary>
    private static bool IsPositive(double value) {
      return value > 0;
    }

    /// <summary>
    /// Checks is a value is negative, including -0.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsNegative(double value) {
      return (value < 0);
    }

    /// <summary>
    /// Normalizes an angle
    /// </summary>
    private static double NormalizeAngle(double angle) {
      if (angle < 0) {
        angle += 2 * Math.PI;
      }
      while (angle > 2 * Math.PI) {
        angle -= 2 * Math.PI;
      }
      return angle;
    }

    /// <summary>
    /// <see langword="true"/> if this label model is a side slider, <see langword="false"/> if it is a center slider
    /// </summary>
    /// <returns></returns>
    private bool IsSideSliderModel() {
      return Distance != 0;
    }

    [CanBeNull]
    private static LineSegment GetLineSegment(PointD[] linePoints, int index) {
      if (index + 1 >= linePoints.Length) {
        index = linePoints.Length - 2;
      } else if (index < 0) {
        index = 0;
      }
      if (index + 1 < linePoints.Length) {
        return new LineSegment(linePoints[index], linePoints[index + 1]);
      }
      return null;
    }

    #region LineSegment

    /// <summary>
    /// A simple class representing a line segment
    /// </summary>
    private sealed class LineSegment
    {
      public LineSegment(PointD startPoint, PointD endPoint) {
        FirstEndPoint = startPoint;
        SecondEndPoint = endPoint;
      }

      internal PointD FirstEndPoint { get; set; }

      internal PointD SecondEndPoint { get; set; }

      public double GetLength() {
        return FirstEndPoint.DistanceTo(SecondEndPoint);
      }

      public PointD ToVector() {
        return SecondEndPoint - FirstEndPoint;
      }

      public double GetDistance(LineSegment otherSegment) {
        if (Intersects(otherSegment)) {
          return 0.0;
        }
        double distance = otherSegment.FirstEndPoint.DistanceToSegment(FirstEndPoint, SecondEndPoint);
        distance = Math.Min(distance, otherSegment.SecondEndPoint.DistanceToSegment(FirstEndPoint, SecondEndPoint));
        distance = Math.Min(distance,
          FirstEndPoint.DistanceToSegment(otherSegment.FirstEndPoint, otherSegment.SecondEndPoint));
        distance = Math.Min(distance,
          SecondEndPoint.DistanceToSegment(otherSegment.FirstEndPoint, otherSegment.SecondEndPoint));
        return distance;
      }

      private bool Intersects(LineSegment otherSegment) {
        return
            !double.IsPositiveInfinity(FindLineSegmentIntersection(FirstEndPoint.X, FirstEndPoint.Y, SecondEndPoint.X,
              SecondEndPoint.Y,
              otherSegment.FirstEndPoint.X, otherSegment.FirstEndPoint.Y, otherSegment.SecondEndPoint.X,
              otherSegment.SecondEndPoint.Y));
      }

      private static double FindLineSegmentIntersection(double l1x1, double l1y1, double l1x2, double l1y2, double l2x1,
                                                        double l2y1, double l2x2, double l2y2) {
        double dx1 = l1x2 - l1x1;
        double dy1 = l1y2 - l1y1;
        double dx2 = l2x2 - l2x1;
        double dy2 = l2y2 - l2y1;
        double denominator = ((dy2 * dx1) - (dx2 * dy1));
        if (denominator != 0) {
          double a = ((dx2 * (l1y1 - l2y1)) - (dy2 * (l1x1 - l2x1))) / denominator;
          if (a >= 0 && a <= 1) {
            double b = ((dx1 * (l1y1 - l2y1)) - (dy1 * (l1x1 - l2x1))) / denominator;
            if (b >= 0 && b <= 1) {
              return b;
            }
          }
        }
        return Double.PositiveInfinity;
      }

      public int GetClosestPointIndex(PointD[] corners) {
        double minDist = double.MaxValue;
        int bestIndex = -1;
        for (int i = 0; i < corners.Length; i++) {
          double dist = corners[i].DistanceToSegment(FirstEndPoint, SecondEndPoint);
          if (dist < minDist) {
            bestIndex = i;
            minDist = dist;
          }
        }
        return bestIndex;
      }

      public PointD GetClosestPoint(PointD[] corners) {
        return corners[GetClosestPointIndex(corners)];
      }
    }

    #endregion

    /// <summary>
    /// Creates a parameter that measures the provided segment index from the source side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the source side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment. A ratio of 
    /// 0.0 will place the label at the source side of the segment, a ratio of 1.0 at the target
    /// side. Ratios &lt; 0.0 or > 1.0 will be interpreted as absolute values in world coordinates.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    [NotNull]
    public ILabelModelParameter CreateParameterFromSource(int segmentIndex, double segmentRatio) {
      return new RotatedSliderParameter(this, segmentIndex, segmentRatio);
    }

    /// <summary>
    /// Creates a parameter that measures the provided segment index from the target side of the edge path.
    /// </summary>
    /// <param name="segmentIndex">The zero-based index of the segment beginning from the target side.</param>
    /// <param name="segmentRatio">The ratio at which to place the label at the segment. A ratio of 
    /// 0.0 will place the label at the target side of the segment, a ratio of 1.0 at the source
    /// side. Ratios &lt; 0.0 or > 1.0 will be interpreted as absolute values in world coordinates.</param>
    /// <returns>A label parameter that describes the provided parameters for this model instance.</returns>
    [NotNull]
    public ILabelModelParameter CreateParameterFromTarget(int segmentIndex, double segmentRatio) {
      return new RotatedSliderParameter(this, -1 - segmentIndex, 1 - segmentRatio);
    }

    internal sealed class RotatedSliderParameter : ILabelModelParameter, IMarkupExtensionConverter
    {
      private readonly RotatedSliderEdgeLabelModel model;

      private readonly int segment;

      private readonly double ratio;

      public RotatedSliderParameter(RotatedSliderEdgeLabelModel model, int segment, double ratio) {
        this.model = model;
        this.segment = segment;
        this.ratio = ratio;
      }

      public int Segment {
        get { return segment; }
      }

      public double Ratio {
        get { return ratio; }
      }

      public ILabelModel Model {
        get { return model; }
      }

      public object Clone() {
        return MemberwiseClone();
      }

      public bool Supports(ILabel label) {
        return label.Owner is IEdge;
      }

      #region Implementation of IMarkupExtensionConverter

      public bool CanConvert(IWriteContext context, object value) {
        return true;
      }

      public MarkupExtension Convert(IWriteContext context, object value) {
        if (Segment < 0) {
          return new RotatedSliderLabelModelParameterExtension
          {
            Location = SliderParameterLocation.FromTarget,
            SegmentIndex = -1 - Segment,
            SegmentRatio = 1 - ratio,
            Model = model
          };
        } else {
          return new RotatedSliderLabelModelParameterExtension
          {
            Location = SliderParameterLocation.FromSource,
            SegmentIndex = Segment,
            SegmentRatio = ratio,
            Model = model
          };
        }
      }

      #endregion
    }

    ILabelModelParameter ILabelModelParameterFinder.FindBestParameter(ILabel label, ILabelModel model,
                                                                      IOrientedRectangle labelLayout) {
      if (!(label.Owner is IEdge)) {
        throw new ArgumentException("RotatedSliderEdgeLabelModel.findBestParameter() can only handle edge labels.");
      }
      if (!(model is RotatedSliderEdgeLabelModel)) {
        throw new ArgumentException(
          "RotatedSliderEdgeLabelModel.findBestParameter() can only handle RotatedSliderEdgeLabelModel.");
      }

      RotatedSliderEdgeLabelModel sliderModel = (RotatedSliderEdgeLabelModel) model;

      var edge = (IEdge) label.Owner;
      RectD sourceNodeLayout = GetNodeLayout(edge.SourcePort);
      RectD targetNodeLayout = GetNodeLayout(edge.TargetPort);

      // get edge path
      GeneralPath generalPath = edge.Style.Renderer.GetPathGeometry(edge, edge.Style).GetPath();
      // get array of path points
      PointD[] path = GetPathPoints(generalPath);

      // if something is wrong with the path, generate one trivial position
      if (path.Length < 2 || (path.Length == 2 && path[0].DistanceTo(path[1]) < Eps)) {
        return sliderModel.CreateDefaultParameter();
      }

      var labelCenter = labelLayout.GetCenter();
      var bestQuality = double.MaxValue;
      var bestRatio = 0.0;
      var bestIndex = 0;
      var boundsR0 = new OrientedRectangle(0, 0, 0, 0);
      var boundsR1 = new OrientedRectangle(0, 0, 0, 0);

      // iterate over all edge segments
      for (var i = 0; i < path.Length - 1; i++) {
        sliderModel.SetFirstAndLastBoxOnSegment(path[i], path[i + 1], i == 0, i == (path.Length - 2), label,
          sourceNodeLayout, targetNodeLayout, boundsR0, boundsR1);

        var p1 = boundsR0.GetCenter();
        var p2 = boundsR1.GetCenter();

        double r;
        double distanceToLine;
        CalculateRatioAndDistance(labelCenter, path, i, p1, p2, out r, out distanceToLine);

        var quality = CalculateQuality(r, distanceToLine);
        if (!(quality < bestQuality)) {
          continue;
        }

        bestQuality = quality;
        bestRatio = r;
        // for segments closer to the target create a parameter "from target"
        if ((i > (path.Length - 2) / 2) || (i == 0 && r > 1)) {
          bestIndex = (i + 1 - path.Length);
        } else {
          bestIndex = i;
        }
      }

      return bestIndex < 0
        ? sliderModel.CreateParameterFromTarget(-bestIndex - 1, 1 - bestRatio)
        : sliderModel.CreateParameterFromSource(bestIndex, bestRatio);
    }

    /// <summary>
    /// Creates the source or target node layout of the port's owner, or a fallback if no owner exists.
    /// </summary>
    /// <param name="port">The port.</param>
    private static RectD GetNodeLayout(IPort port) {
      var sourceNode = port.Owner as INode;
      if (sourceNode != null) {
        return sourceNode.Layout.ToRectD();
      }
      var location = port.GetLocation();
      return new RectD(location.X - 0.5, location.Y - 0.5, 1, 1);
    }

    internal virtual void CalculateRatioAndDistance(PointD lc, PointD[] path, int i, PointD p1, PointD p2,
                                                    out double r, out double distanceToLine) {
      var direction = p2 - p1;
      // lx/ly are the absolute coordinates of the projection of the center on the line
      double lx, ly;
      if (direction.SquaredVectorLength == 0) {
        // we have no direction vector - absolute ratio
        direction = (path[i + 1] - path[i]).Normalized;
        var project = lc.GetProjectionOnLine(p1, direction);
        lx = project.X;
        ly = project.Y;
        distanceToLine = (lc - project).SquaredVectorLength;
        var dx = lx - p1.X;
        var edx = direction.X;
        var dy = ly - p1.Y;
        var edy = direction.Y;
        // can never happen?
        if (edx == 0 && edy == 0) {
          edx = 1;
          edy = 0;
        }
        if (Math.Abs(edy) > Math.Abs(edx)) {
          r = dy / edy;
        } else if (Math.Abs(edx) > 0) {
          r = dx / edx;
        } else {
          r = (dx / edx + dy / edy) / 2;
        }
        if (r > 0) {
          // trigger absolute calculation
          r += 1.0d;
        }
        if (r == 0) {
          r = -0.00001;
        }
      } else {
        var project = lc.GetProjectionOnLine(p1, direction);
        lx = project.X;
        ly = project.Y;
        distanceToLine = (lc - project).SquaredVectorLength;
        var dx = lx - p1.X;
        var edx = direction.X;
        var dy = ly - p1.Y;
        var edy = direction.Y;
        // can never happen?
        if (edx == 0 && edy == 0) {
          edx = 1;
          edy = 0;
        }

        if (Math.Abs(edy) > Math.Abs(edx)) {
          r = dy / edy;
        } else if (Math.Abs(edx) > 0) {
          r = dx / edx;
        } else {
          r = (dx / edx + dy / edy) / 2;
        }
      }

      r = ModifyAbsoluteRatios(lx, ly, p1, p2, r);
    }

    private static double ModifyAbsoluteRatios(double lx, double ly, PointD p1, PointD p2, double r) {
      if (r < 0) {
        // ratio < 0 is interpreted absolutely:
        var dx = p1.X - lx;
        var dy = p1.Y - ly;
        // but we have to make sure this absolute value is < 0
        r = Math.Min(-Math.Sqrt(dx * dx + dy * dy), -0.0000001);
      } else if (r > 1) {
        // ratio > 1 is interpreted absolutely:
        var dx = p2.X - lx;
        var dy = p2.Y - ly;
        // but we have to make sure this absolute value is > 1
        r = Math.Max(Math.Sqrt(dx * dx + dy * dy), 1.0000001);
      }
      return r;
    }

    private double CalculateQuality(double ratio,
                                    double distanceToLine) {
      var rq = ratio < 0 ? -ratio + 1 : ratio;
      return rq < 1
        ? distanceToLine + Math.Abs(rq - 0.5)
        : Math.Sqrt(distanceToLine * distanceToLine + (rq - 1) * (rq - 1));
    }

    internal void SetFirstAndLastBoxOnSegment(
      PointD p1, PointD p2, bool isFirst, bool isLast, ILabel label, RectD sourceNodeLayout,
      RectD targetNodeLayout, OrientedRectangle boundsR0, OrientedRectangle boundsR1) {
      var segment = new LineSegment(p1, p2);
      var edge = (IEdge) label.Owner;
      if (segment.GetLength() == 0) {
        // something went wrong, fix it
        var spl = edge.SourcePort.GetLocation();
        var tpl = edge.TargetPort.GetLocation();
        var dx = spl.X - tpl.X;
        var dy = spl.Y - tpl.Y;
        if (dx == 0 && dy == 0) {
          // something even worse happened, try to fix it!
          p2 = new PointD(p1.X + Eps, p1.Y);
        } else {
          var dl = Math.Sqrt(dx * dx + dy * dy);
          segment.SecondEndPoint = p2 = new PointD(p1.X + Eps * dx / dl, p1.Y + Eps * dy / dl);
        }
      }

      // determine rotation angle
      var rotationAngle = Angle;
      if (AutoRotationEnabled) {
        rotationAngle = CalculateRotationAngle(segment.ToVector(), Angle);
      }
      // segment index used for the model parameter
      var dist = Distance;
      boundsR0.Size = label.PreferredSize;
      boundsR1.Size = label.PreferredSize;

      // represents label candidate at ratio 0
      if (rotationAngle != 0) {
        boundsR0.Angle = rotationAngle;
      }
      PlaceAtPoint(boundsR0, segment, p1, dist);
      if (isFirst && DoIntersect(sourceNodeLayout, boundsR0)) {
        // placement depends on source node
        PlaceAtSource(boundsR0, segment, sourceNodeLayout, dist);
      }
      // represents label candidate at ratio 1
      boundsR1.Angle = rotationAngle;
      PlaceAtPoint(boundsR1, segment, p2, dist);
      if (isLast && DoIntersect(targetNodeLayout, boundsR1)) {
        // placement depends on target node
        PlaceAtTarget(boundsR1, segment, targetNodeLayout, dist);
      }
    }

    private static bool DoIntersect(RectD nodeLayout, IOrientedRectangle labelBox) {
      return nodeLayout.Intersects(labelBox, LabelNodeDistance);
    }
  }
}