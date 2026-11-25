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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.EdgeBundling
{
  /// <summary>
  ///   Draws the edges with cubic Bezier curves and gradient colors from the source to the target node.
  /// </summary>
  public sealed class DemoEdgeStyle : EdgeStyleBase<DrawingVisual>
  {
    public double PathThickness { get; }

    public Color StartColor { get; }

    public Color EndColor { get; }

    public DemoEdgeStyle(double pathThickness = 2, Color? startColor = null, Color? endColor = null) {
      PathThickness = pathThickness;
      StartColor = startColor ?? Color.FromRgb(0, 0, 139);
      EndColor = endColor ?? Color.FromRgb(135, 206, 235);
    }

    protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
      var visual = new DrawingVisual();
      Render(context, visual, edge);
      return visual;
    }

    protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, IEdge edge) {
      Render(context, oldVisual, edge);
      return oldVisual;
    }

    private void Render(IRenderContext context, DrawingVisual visual, IEdge edge) {
      var points = GetControlPoints(edge);
      var graphControl = context.CanvasControl as GraphControl;
      var selected = graphControl?.Selection.Contains(edge) == true;
      var width = selected ? 5 : PathThickness;
      var colors = GenerateColors(selected ? Colors.Red : StartColor, selected ? Colors.Gold : EndColor);
      using var drawing = visual.RenderOpen();

      if (edge.Bends.Count > 1) {
        // Draw each pairwise Bezier segment with a different color from the gradient.
        for (var i = 0; i < points.Count - 3; i += 3) {
          var colorIndex = Math.Clamp((int) Math.Floor(i * (colors.Count - 1d) / (points.Count - 1)), 0,
              colors.Count - 1);
          var geometry = new StreamGeometry();
          using (var path = geometry.Open()) {
            path.BeginFigure(points[i].ToPoint(), false, false);
            path.BezierTo(points[i + 1].ToPoint(), points[i + 2].ToPoint(), points[i + 3].ToPoint(), true, false);
          }

          geometry.Freeze();
          drawing.DrawGeometry(null, CreatePen(colors[colorIndex], width), geometry);
        }
      } else {
        // Split straight paths into colored line segments so the source and target colors remain visible.
        var linePoints = points.ToList();
        if (linePoints.Count == 2) {
          linePoints.Insert(1,
              new PointD((linePoints[0].X + linePoints[1].X) / 2, (linePoints[0].Y + linePoints[1].Y) / 2));
        }
        for (var i = 1; i < linePoints.Count; i++) {
          var colorIndex = Math.Clamp((int) Math.Floor(i * (colors.Count - 1d) / (linePoints.Count - 1)), 0,
              colors.Count - 1);
          drawing.DrawLine(CreatePen(colors[colorIndex], width), linePoints[i - 1].ToPoint(), linePoints[i].ToPoint());
        }
      }
    }

    protected override GeneralPath GetPath(IEdge edge) {
      var points = GetControlPoints(edge);
      var path = new GeneralPath();
      path.MoveTo(points[0]);
      if (edge.Bends.Count > 1) {
        for (var i = 0; i < points.Count - 3; i += 3) {
          path.CubicTo(points[i + 1], points[i + 2], points[i + 3]);
        }
      } else {
        foreach (var bend in edge.Bends) {
          path.LineTo(bend.Location);
        }
        path.LineTo(edge.TargetPort.GetLocation());
      }

      return path;
    }

    private static List<Color> GenerateColors(Color start, Color end) {
      const int count = 25;
      var colors = new List<Color>(count);
      for (var i = 0; i < count; i++) {
        var t = i / (double) (count - 1);
        colors.Add(Color.FromArgb((byte) (start.A * (1 - t) + end.A * t), (byte) (start.R * (1 - t) + end.R * t),
            (byte) (start.G * (1 - t) + end.G * t), (byte) (start.B * (1 - t) + end.B * t)));
      }

      colors.Reverse();
      return colors;
    }

    private static List<PointD> GetControlPoints(IEdge edge) {
      List<PointD> points = [
          edge.SourcePort.GetLocation(),
          .. edge.Bends.Select(bend => bend.Location.ToPointD()),
          edge.TargetPort.GetLocation()
      ];
      if (edge.Bends.Count > 1) {
        var target = edge.TargetPort.GetLocation();
        if (points.Count % 3 == 0) {
          points.Add(target);
        } else if (points.Count % 3 == 2) {
          points.Add(target);
          points.Add(target);
        }
      }

      return points;
    }

    private static Pen CreatePen(Color color, double width) {
      var brush = new SolidColorBrush(Color.FromArgb(102, color.R, color.G, color.B));
      brush.Freeze();
      var pen = new Pen(brush, width) {
          StartLineCap = PenLineCap.Round,
          EndLineCap = PenLineCap.Round,
          LineJoin = PenLineJoin.Round
      };
      pen.Freeze();
      return pen;
    }
  }

  /// <summary>Colors Bezier edges according to the angle between their source and target ports.</summary>
  public sealed class ColoredDelegatingEdgeStyle : DelegatingEdgeStyle
  {
    private readonly BezierEdgeStyle delegatingStyle = new() {
        SourceArrow = new Arrow(ArrowType.None, cropAtPort: true),
        TargetArrow = new Arrow(ArrowType.None, cropAtPort: true)
    };

    private static readonly Color[] Palette = [
        ColorFromHex("#f4ba26"), ColorFromHex("#e05d29"), ColorFromHex("#d62329"), ColorFromHex("#d90f7d"),
        ColorFromHex("#b00d7a"), ColorFromHex("#7d3199"), ColorFromHex("#56168c"), ColorFromHex("#0b58a9"),
        ColorFromHex("#0796d4"), ColorFromHex("#00ccff"), ColorFromHex("#04bec8"), ColorFromHex("#05e4d9"),
        ColorFromHex("#06dc9f"), ColorFromHex("#01c432"), ColorFromHex("#a8cf1c"), ColorFromHex("#ded000")
    ];

    protected override IEdgeStyle GetStyle(IEdge edge) {
      // Determine the edge color from the angle between its source and target ports.
      delegatingStyle.Pen = GetPen(edge);
      return delegatingStyle;
    }

    private static Pen? GetPen(IEdge edge) {
      var source = edge.SourcePort.GetLocation();
      var target = edge.TargetPort.GetLocation();
      var dx = source.X - target.X;
      var dy = source.Y - target.Y;
      if (dx == 0 && dy == 0) {
        return null;
      }
      var angle = Math.Atan2(dy, dx) + Math.PI / 2;
      var colorAngle = Math.PI / Palette.Length;
      var index = Math.Clamp((int) Math.Floor(angle / colorAngle), 0, Palette.Length - 1);
      var brush = new SolidColorBrush(Color.FromArgb(128, Palette[index].R, Palette[index].G, Palette[index].B));
      brush.Freeze();
      var pen = new Pen(brush, 0.5);
      pen.Freeze();
      return pen;
    }

    private static Color ColorFromHex(string value) => (Color) ColorConverter.ConvertFromString(value)!;
  }

  /// <summary>Draws nodes as circular sectors when they are members of a circular partition.</summary>
  public sealed class DemoNodeStyle : NodeStyleBase<DrawingVisual>
  {
    public Color Color { get; }

    public double Thickness { get; }

    public DemoNodeStyle(Color? color = null, double thickness = 20) {
      Color = color ?? ColorFromHex("#0b7189");
      Thickness = thickness;
    }

    protected override DrawingVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new DrawingVisual();
      Render(context, visual, node);
      return visual;
    }

    protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, INode node) {
      Render(context, oldVisual, node);
      return oldVisual;
    }

    private void Render(IRenderContext context, DrawingVisual visual, INode node) {
      var layout = node.Layout;
      var center = layout.GetCenter();
      var selected = context.CanvasControl is GraphControl control && control.Selection.Contains(node);
      var fill = new SolidColorBrush(selected ? Colors.Red : Color);
      fill.Freeze();
      using var drawing = visual.RenderOpen();
      if (node.Tag is CircleNodeInfo info && info.CircleNodeSize > 2) {
        // Render an annular sector for nodes that belong to a circular partition.
        var delta = center - info.Center;
        var currentAngle = Math.Atan2(delta.Y, delta.X);
        var radius = delta.VectorLength;
        var innerRadius = radius - Thickness * 0.5;
        var outerRadius = radius + Thickness * 0.5;
        var halfAngle = Math.PI / info.CircleNodeSize;
        var startInner = PointAt(info.Center, innerRadius, currentAngle - halfAngle);
        var endInner = PointAt(info.Center, innerRadius, currentAngle + halfAngle);
        var endOuter = PointAt(info.Center, outerRadius, currentAngle + halfAngle);
        var startOuter = PointAt(info.Center, outerRadius, currentAngle - halfAngle);
        var geometry = new StreamGeometry();
        using (var path = geometry.Open()) {
          path.BeginFigure(startInner.ToPoint(), true, true);
          path.ArcTo(endInner.ToPoint(), new Size(innerRadius, innerRadius), 0, halfAngle * 2 > Math.PI,
              SweepDirection.Clockwise, true, false);
          path.LineTo(endOuter.ToPoint(), true, false);
          path.ArcTo(startOuter.ToPoint(), new Size(outerRadius, outerRadius), 0, halfAngle * 2 > Math.PI,
              SweepDirection.Counterclockwise, true, false);
        }

        geometry.Freeze();
        drawing.DrawGeometry(fill, Pens.White, geometry);
      } else {
        drawing.DrawEllipse(fill, Pens.White, new Point(center.X, center.Y), layout.Width / 2, layout.Height / 2);
      }
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      if (node.Tag is CircleNodeInfo info && info.CircleNodeSize > 2) {
        var center = node.Layout.GetCenter();
        var delta = center - info.Center;
        var radius = delta.VectorLength;
        var angle = Math.PI / info.CircleNodeSize;
        var inner = radius - Thickness / 2 - context.HitTestRadius;
        var outer = radius + Thickness / 2 + context.HitTestRadius;
        var hitDelta = location - info.Center;
        var hitRadius = hitDelta.VectorLength;
        if (hitRadius < inner || hitRadius > outer || hitRadius == 0 || radius == 0) {
          return false;
        }
        var cosine = (delta.X * hitDelta.X + delta.Y * hitDelta.Y) / (radius * hitRadius);
        return Math.Acos(Math.Clamp(cosine, -1, 1)) < angle;
      }

      var layout = node.Layout;
      var x = (location.X - layout.X - layout.Width / 2) / (layout.Width / 2);
      var y = (location.Y - layout.Y - layout.Height / 2) / (layout.Height / 2);
      var tolerance = context.HitTestRadius / Math.Max(1, Math.Min(layout.Width, layout.Height) / 2);
      return x * x + y * y <= (1 + tolerance) * (1 + tolerance);
    }

    private static PointD PointAt(PointD center, double radius, double angle) =>
        new(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);

    private static Color ColorFromHex(string value) => (Color) ColorConverter.ConvertFromString(value)!;
  }

  internal sealed record CircleNodeInfo(int CircleNodeSize, PointD Center);
}
