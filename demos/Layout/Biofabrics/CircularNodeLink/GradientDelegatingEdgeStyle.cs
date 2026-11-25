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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   A delegating edge style that flattens its path and draws color-interpolated segments.
  /// </summary>
  public sealed class GradientDelegatingEdgeStyle : DelegatingEdgeStyle
  {
    /// <summary>Gets the edge style that supplies the path to render.</summary>
    public IEdgeStyle WrappedStyle { get; }

    /// <summary>Gets the color applied at the source end of the edge.</summary>
    public string StartColor { get; }

    /// <summary>Gets the color applied at the target end of the edge.</summary>
    public string EndColor { get; }

    /// <summary>Gets the maximum target length of the line segments used to approximate the path.</summary>
    public double SubdivisionTargetLength { get; }

    /// <summary>
    ///   Gets or sets an optional brush that replaces the interpolated colors, for example while highlighting.
    /// </summary>
    public Brush? HighlightBrush { get; set; }

    /// <summary>
    ///   Creates a delegating edge style that interpolates colors along the wrapped edge path.
    /// </summary>
    /// <param name="wrappedStyle">The style that provides the edge path.</param>
    /// <param name="startColor">The color at the source end of the edge.</param>
    /// <param name="endColor">The color at the target end of the edge.</param>
    /// <param name="subdivisionTargetLength">The maximum target length for path segments.</param>
    public GradientDelegatingEdgeStyle(
        IEdgeStyle wrappedStyle,
        string startColor,
        string endColor,
        double subdivisionTargetLength = 40) {
      WrappedStyle = wrappedStyle;
      StartColor = startColor;
      EndColor = endColor;
      SubdivisionTargetLength = subdivisionTargetLength;
    }

    /// <summary>Creates a visual by drawing the wrapped path as short, color-interpolated lines.</summary>
    protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
      var visual = new DrawingVisual();
      Draw(context, visual, edge);
      return visual;
    }

    /// <summary>Updates the existing drawing visual to reflect the current edge path and colors.</summary>
    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, IEdge edge) {
      var visual = (DrawingVisual) oldVisual;
      Draw(context, visual, edge);
      return visual;
    }

    private void Draw(IRenderContext context, DrawingVisual visual, IEdge edge) {
      // Get the path produced by the wrapped edge style and flatten its curves into short segments.
      var path = WrappedStyle.Renderer.GetPathGeometry(edge, WrappedStyle).GetPath();
      if (path is null) {
        return;
      }

      var points = FlattenGeneralPathToSegments(path, 0.5, SubdivisionTargetLength);
      if (points.Count < 2) {
        return;
      }

      // Match the line width of the wrapped style where it is available.
      var width = WrappedStyle switch {
          BezierEdgeStyle bezier => bezier.Pen?.Thickness ?? 2,
          PolylineEdgeStyle polyline => polyline.Pen?.Thickness ?? 2,
          _ => 2
      };
      var startColor = ParseColor(StartColor);
      var endColor = ParseColor(EndColor);

      using var dc = visual.RenderOpen();
      // Draw each consecutive pair of flattened points with the color at that position on the edge.
      for (var i = 1; i < points.Count; i++) {
        // Interpolate between the two endpoint colors along the flattened path.
        var colorPosition = (double) i / (points.Count - 1);
        Brush brush;
        if (HighlightBrush is not null) {
          brush = HighlightBrush;
        } else {
          var color = Interpolate(startColor, endColor, colorPosition);
          brush = new SolidColorBrush(color);
        }
        brush.Freeze();
        var pen = new Pen(brush, width) {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();
        dc.DrawLine(pen, points[i - 1].ToPoint(), points[i].ToPoint());
      }
    }

    /// <summary>Returns the wrapped style so the base class can preserve its style semantics.</summary>
    protected override IEdgeStyle GetStyle(IEdge edge) => WrappedStyle;

    /// <summary>
    ///   Converts the path into short line segments, subdividing curved segments until they meet
    ///   both the flatness and maximum-length thresholds.
    /// </summary>
    /// <param name="edgePath">The path to flatten.</param>
    /// <param name="flatnessThreshold">The maximum distance from a curve control point to its approximating chord.</param>
    /// <param name="sizeThreshold">The maximum target length for each resulting line segment.</param>
    /// <returns>The ordered points that define the flattened path.</returns>
    private static List<PointD> FlattenGeneralPathToSegments(
        GeneralPath edgePath,
        double flatnessThreshold,
        double sizeThreshold) {
      // Path cursor coordinates contain up to three point pairs for a cubic curve.
      var coordinates = new double[6];
      var current = new PointD(0, 0);
      var subpathStart = current;
      var lineSegments = new List<PointD>();
      var cursor = edgePath.CreateCursor();
      while (cursor.MoveNext()) {
        var type = cursor.PathType;
        cursor.GetCurrent(coordinates);
        switch (type) {
          case PathType.MoveTo:
            current = new PointD(coordinates[0], coordinates[1]);
            subpathStart = current;
            lineSegments.Add(current);
            break;
          case PathType.LineTo: {
            // Divide long straight segments too, so the gradient advances smoothly along the path.
            var target = new PointD(coordinates[0], coordinates[1]);
            AddLinePoints(lineSegments, current, target, sizeThreshold);
            current = target;
            break;
          }
          case PathType.Close:
            // Close the current subpath to its initial point.
            AddLinePoints(lineSegments, current, subpathStart, sizeThreshold);
            current = subpathStart;
            break;
          case PathType.CubicTo: {
            var p1 = new PointD(coordinates[0], coordinates[1]);
            var p2 = new PointD(coordinates[2], coordinates[3]);
            var p3 = new PointD(coordinates[4], coordinates[5]);
            // Recursively approximate a cubic curve by its sufficiently flat, short chords.
            SubdivideCubic(current, p1, p2, p3, flatnessThreshold, sizeThreshold, lineSegments);
            current = p3;
            break;
          }
          case PathType.QuadTo: {
            var control = new PointD(coordinates[0], coordinates[1]);
            var target = new PointD(coordinates[2], coordinates[3]);
            // A quadratic curve is represented as a cubic with the same control point twice.
            SubdivideCubic(current, control, control, target, flatnessThreshold, sizeThreshold, lineSegments);
            current = target;
            break;
          }
        }
      }
      return lineSegments;
    }

    private static void AddLinePoints(List<PointD> points, PointD start, PointD end, double sizeThreshold) {
      var distance = start.DistanceTo(end);
      if (distance > sizeThreshold) {
        // Insert evenly spaced intermediate points while retaining the original endpoint.
        var divisions = (int) Math.Ceiling(distance / sizeThreshold);
        for (var i = 1; i < divisions; i++) {
          var t = (double) i / divisions;
          points.Add(start + (end - start) * t);
        }
      }
      points.Add(end);
    }

    private static void SubdivideCubic(
        PointD start,
        PointD cp1,
        PointD cp2,
        PointD end,
        double flatnessThreshold,
        double lengthThreshold,
        List<PointD> points,
        int depth = 0) {
      // Measure how far the control points bend away from the chord connecting the endpoints.
      var flatness = Math.Max(cp1.DistanceToSegment(start, end), cp2.DistanceToSegment(start, end));
      if ((flatness > flatnessThreshold || start.DistanceTo(end) > lengthThreshold) && depth < 18) {
        // Split the Bezier at t = 0.5 using De Casteljau's construction, then process both halves.
        var p01 = Midpoint(start, cp1);
        var p12 = Midpoint(cp1, cp2);
        var p23 = Midpoint(cp2, end);
        var p012 = Midpoint(p01, p12);
        var p123 = Midpoint(p12, p23);
        var middle = Midpoint(p012, p123);
        SubdivideCubic(start, p01, p012, middle, flatnessThreshold, lengthThreshold, points, depth + 1);
        SubdivideCubic(middle, p123, p23, end, flatnessThreshold, lengthThreshold, points, depth + 1);
      } else {
        // The chord meets both limits (or reached the recursion cap); append its endpoint.
        points.Add(end);
      }
    }

    /// <summary>Returns the midpoint used by De Casteljau subdivision.</summary>
    private static PointD Midpoint(PointD a, PointD b) => new((a.X + b.X) / 2, (a.Y + b.Y) / 2);

    /// <summary>Parses a WPF color string, such as a named color or hexadecimal value.</summary>
    private static Color ParseColor(string value) => (Color) ColorConverter.ConvertFromString(value)!;

    /// <summary>Linearly interpolates the alpha and red, green, and blue channels between two colors.</summary>
    private static Color Interpolate(Color first, Color second, double position) =>
        Color.FromArgb((byte) (first.A + (second.A - first.A) * position),
            (byte) (first.R + (second.R - first.R) * position), (byte) (first.G + (second.G - first.G) * position),
            (byte) (first.B + (second.B - first.B) * position));
  }
}
