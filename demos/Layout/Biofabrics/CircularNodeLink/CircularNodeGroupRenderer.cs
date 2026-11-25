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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Renders node groups as arc segments around the circular layout.
  /// </summary>
  public sealed class CircularNodeGroupRenderer : ObjectRendererBase<CircularNodeGroupRenderTag, DrawingVisual>
  {
    /// <summary>Gets the stroke width of the group arc.</summary>
    public double Thickness { get; }

    /// <summary>Gets the distance between the outer node circle and the group arc.</summary>
    public double RadiusOffset { get; }

    /// <summary>Gets or sets the optional color used for the group arc and its label.</summary>
    public string? Color { get; set; }

    /// <summary>Gets or sets the center of the circular layout in world coordinates.</summary>
    public PointD Center { get; set; } = new(0, 0);

    /// <summary>
    ///   Creates a new circular node group renderer.
    /// </summary>
    /// <param name="thickness">The stroke width of the group arc.</param>
    /// <param name="radiusOffset">The distance from the node circle to the group arc.</param>
    /// <param name="color">Optional color for the group arc and text.</param>
    public CircularNodeGroupRenderer(double thickness, double radiusOffset, string? color = null) {
      Thickness = thickness;
      RadiusOffset = radiusOffset;
      Color = color;
    }

    /// <summary>Creates the arc and group label for the supplied node group.</summary>
    protected override DrawingVisual CreateVisual(IRenderContext context, CircularNodeGroupRenderTag renderTag) {
      var visual = new DrawingVisual();
      Draw(context, visual, renderTag);
      return visual;
    }

    protected override DrawingVisual UpdateVisual(
        IRenderContext context,
        DrawingVisual oldVisual,
        CircularNodeGroupRenderTag renderTag) {
      // Redraw the arc and label when the render tree requests an update.
      Draw(context, oldVisual, renderTag);
      return oldVisual;
    }

    private void Draw(IRenderContext context, DrawingVisual visual, CircularNodeGroupRenderTag renderTag) {
      var nodes = renderTag.NodeGroup.Nodes;
      if (nodes.Count == 0) {
        return;
      }
      // The groups are ordered along the circle, so the first and last members delimit their arc.
      var radius = nodes[0].Layout.GetCenter().DistanceTo(Center) + RadiusOffset;
      var (start, end) = GetArcEndpoints(nodes, radius);
      var startAngle = Math.Atan2(start.Y - Center.Y, start.X - Center.X);
      var endAngle = Math.Atan2(end.Y - Center.Y, end.X - Center.X);
      var clockwiseSweep = Normalize(endAngle - startAngle);
      var largeArc = nodes.Count > 1 && renderTag.LargeGroup;

      // Draw the group boundary as one circular arc between the first and last member directions.
      var geometry = new StreamGeometry();
      using (var geometryContext = geometry.Open()) {
        geometryContext.BeginFigure(start.ToPoint(), false, false);
        geometryContext.ArcTo(end.ToPoint(), new Size(radius, radius), 0, largeArc, SweepDirection.Clockwise, true,
            false);
      }
      geometry.Freeze();

      var highlight = renderTag.NodeGroup.Highlighted;
      var brush = highlight
          ? BiofabricRenderUtils.DefaultHighlight
          : BiofabricRenderUtils.CreateBrush(Color, BiofabricRenderUtils.DefaultLabelBackground);
      var pen = BiofabricRenderUtils.CreatePen(brush, Thickness);
      var label = BiofabricRenderUtils.FormatText(renderTag.GroupName, 4 * Thickness, brush);
      label.TextAlignment = TextAlignment.Center;

      // Position the group label at the middle of the arc and align it with the tangent.
      var selectedSweep = largeArc ? clockwiseSweep <= Math.PI ? 2 * Math.PI - clockwiseSweep : clockwiseSweep :
          clockwiseSweep <= Math.PI ? clockwiseSweep : 2 * Math.PI - clockwiseSweep;
      var midAngle = startAngle + selectedSweep / 2;
      var labelRadius = radius + 40;
      var anchor = new Point(Center.X + labelRadius * Math.Cos(midAngle), Center.Y + labelRadius * Math.Sin(midAngle));
      var tangentDegrees = (midAngle + Math.PI / 2) * 180 / Math.PI;

      // Apply the world-to-view transform once to the arc and its label.
      using var dc = visual.RenderOpen();
      dc.DrawGeometry(null, pen, geometry);
      dc.PushTransform(new RotateTransform(tangentDegrees, anchor.X, anchor.Y));
      dc.DrawText(label, new Point(anchor.X, anchor.Y));
      dc.Pop();
    }

    /// <summary>Tests whether a world-coordinate point falls within the visible group arc's hit tolerance.</summary>
    protected override bool IsHit(IInputModeContext context, PointD location, CircularNodeGroupRenderTag renderTag) {
      var nodes = renderTag.NodeGroup.Nodes;
      if (nodes.Count == 0) {
        return false;
      }
      var radius = nodes[0].Layout.GetCenter().DistanceTo(Center) + RadiusOffset;
      var (start, end) = GetArcEndpoints(nodes, radius);
      return IsPointOnArc(start, end, location, radius, Center, Math.Max(2, radius * 0.05), renderTag.LargeGroup);
    }

    /// <summary>Returns conservative bounds that include both the group arc and its label.</summary>
    protected override RectD GetBounds(ICanvasContext context, CircularNodeGroupRenderTag renderTag) {
      if (renderTag.NodeGroup.Nodes.Count == 0) {
        return new RectD(Center.X, Center.Y, 0, 0);
      }
      var text = BiofabricRenderUtils.FormatText(renderTag.GroupName, 4 * Thickness, Brushes.White);
      var radius = renderTag.NodeGroup.Nodes[0].Layout.GetCenter().DistanceTo(Center) + RadiusOffset + text.Height;
      return new RectD(Center.X - radius, Center.Y - radius, 2 * radius, 2 * radius);
    }

    /// <summary>
    ///   Determines whether a point is close to the circle and falls within the selected arc sweep.
    /// </summary>
    /// <param name="a">The start point of the arc.</param>
    /// <param name="b">The end point of the arc.</param>
    /// <param name="testPoint">The point to test.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="center">The center of the circle.</param>
    /// <param name="epsilon">The radial tolerance for the hit test.</param>
    /// <param name="largeGroup">Whether the arc represents the larger of the two possible sweeps.</param>
    /// <returns><see langword="true"/> if the point lies on the arc within the given tolerance.</returns>
    private static bool IsPointOnArc(
        PointD a,
        PointD b,
        PointD testPoint,
        double radius,
        PointD center,
        double epsilon,
        bool largeGroup) {
      // Reject points that are not close to the circle before comparing their angles.
      if (Math.Abs(center.DistanceTo(testPoint) - radius) > epsilon) {
        return false;
      }

      var angleA = Math.Atan2(a.Y - center.Y, a.X - center.X);
      var angleB = Math.Atan2(b.Y - center.Y, b.X - center.X);
      var angleP = Math.Atan2(testPoint.Y - center.Y, testPoint.X - center.X);
      var directionSweep = Normalize(angleB - angleA);
      // The endpoints allow either the short or long arc; choose according to the group's size.
      var sweep = largeGroup ? directionSweep <= Math.PI ? 2 * Math.PI - directionSweep : directionSweep :
          directionSweep <= Math.PI ? directionSweep : 2 * Math.PI - directionSweep;
      var target = Normalize(angleP - angleA);
      return target <= sweep;
    }

    /// <summary>Gets the points on the group circle aligned with its first and last nodes.</summary>
    /// <param name="nodes">The ordered nodes in the group.</param>
    /// <param name="radius">The radius of the group arc.</param>
    /// <returns>The start and end points of the arc.</returns>
    private (PointD Start, PointD End) GetArcEndpoints(IReadOnlyList<INode> nodes, double radius) {
      var first = nodes[0].Layout.GetCenter();
      if (nodes.Count == 1) {
        // Give a one-node group a small visible arc around its node direction.
        var nodeAngle = Math.Atan2(first.Y - Center.Y, first.X - Center.X);
        const double spreadAngle = 0.1;
        return (AtAngle(nodeAngle - spreadAngle, radius), AtAngle(nodeAngle + spreadAngle, radius));
      }
      return (AtAngle(Math.Atan2(first.Y - Center.Y, first.X - Center.X), radius),
          AtAngle(Math.Atan2(nodes[^1].Layout.GetCenter().Y - Center.Y, nodes[^1].Layout.GetCenter().X - Center.X),
              radius));
    }

    /// <summary>Returns the point at the specified angle and radius around <see cref="Center"/>.</summary>
    private PointD AtAngle(double angle, double radius) =>
        new(Center.X + radius * Math.Cos(angle), Center.Y + radius * Math.Sin(angle));

    /// <summary>Normalizes an angle to the range from zero, inclusive, to 2π, exclusive.</summary>
    private static double Normalize(double angle) => (angle % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);
  }

  /// <summary>Associates a node-group arc visual with its group data and arc-size selection.</summary>
  /// <param name="GroupName">The identifier displayed beside the group arc.</param>
  /// <param name="NodeGroup">The group whose nodes determine the arc endpoints.</param>
  /// <param name="LargeGroup">Whether the group arc uses the larger sweep between its endpoints.</param>
  public sealed record CircularNodeGroupRenderTag(string GroupName, NodeGroup NodeGroup, bool LargeGroup);
}
