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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DataBinding.PortAwareGraphBuilder
{
  /// <summary>
  ///   The base class for the logic gate node style. This handles the creation
  ///   and update of the node visuals.
  /// </summary>
  internal abstract class GateNodeStyle : NodeStyleBase<DrawingVisual>
  {
    private readonly Brush fill;
    private readonly Brush labelBrush;
    private readonly Pen border;

    protected GateNodeStyle(string fillColor, string strokeColor, string labelColor) {
      fill = CreateBrush(fillColor);
      labelBrush = CreateBrush(labelColor);
      border = CreatePen(strokeColor);
    }

    protected abstract void Render(DrawingContext drawingContext, double width, double height, DrawingVisual visual);

    protected override DrawingVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new DrawingVisual();
      RenderVisual(visual, node);
      return visual;
    }

    /// <summary>
    ///   Re-renders the node using the old visual for performance reasons.
    /// </summary>
    /// <param name="context">The render context</param>
    /// <param name="oldVisual">The old visual</param>
    /// <param name="node">The node to which this style instance is assigned</param>
    protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, INode node) {
      RenderVisual(oldVisual, node);
      return oldVisual;
    }

    private void RenderVisual(DrawingVisual visual, INode node) {
      var layout = node.Layout;
      visual.Transform = new TranslateTransform(layout.X, layout.Y);
      using var drawingContext = visual.RenderOpen();
      Render(drawingContext, layout.Width, layout.Height, visual);
    }

    protected void DrawShape(DrawingContext drawingContext, Action<StreamGeometryContext> buildPath) {
      drawingContext.DrawGeometry(fill, border, CreateGeometry(buildPath));
    }

    protected static void DrawLine(DrawingContext drawingContext, Pen pen, Action<StreamGeometryContext> buildPath) {
      drawingContext.DrawGeometry(null, pen, CreateGeometry(buildPath));
    }

    protected static void DrawBubble(DrawingContext drawingContext, double centerX, double centerY, double radius) {
      drawingContext.DrawEllipse(Brushes.White, CreatePen("black"), new Point(centerX, centerY), radius, radius);
    }

    protected void DrawLabel(
        DrawingContext drawingContext,
        DrawingVisual visual,
        string text,
        double width,
        double height,
        double horizontalPosition = 0.5) {
      var fontSize = Math.Floor(height * 0.25);
      var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
          new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), fontSize,
          labelBrush, VisualTreeHelper.GetDpi(visual).PixelsPerDip);
      drawingContext.DrawText(formattedText,
          new Point((width - formattedText.Width) * horizontalPosition, (height - formattedText.Height) / 2));
    }

    private static StreamGeometry CreateGeometry(Action<StreamGeometryContext> buildPath) {
      var geometry = new StreamGeometry();
      using (var context = geometry.Open()) {
        buildPath(context);
      }
      geometry.Freeze();
      return geometry;
    }

    protected static Pen CreatePen(string color) {
      var pen = new Pen(CreateBrush(color), 2) { LineJoin = PenLineJoin.Round };
      pen.Freeze();
      return pen;
    }

    private static Brush CreateBrush(string color) {
      var brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(color)!);
      brush.Freeze();
      return brush;
    }

    /// <summary>
    ///   Calculates a point on the Bézier cubic curve based on the given t value.
    /// </summary>
    /// <param name="t">The parametric value t in [0,1]</param>
    /// <param name="firstPoint">The first point of the curve</param>
    /// <param name="endPoint">The end point of the curve</param>
    /// <param name="c1">The first control point of the curve</param>
    /// <param name="c2">The second control point of the curve</param>
    /// <returns>The calculated x coordinate of the point on the cubic Bézier curve</returns>
    protected static double GetPointOnCurve(double t, Point firstPoint, Point endPoint, Point c1, Point c2) =>
        Math.Pow(1 - t, 3) * firstPoint.X + 3 * Math.Pow(1 - t, 2) * t * c1.X + 3 * (1 - t) * t * t * c2.X +
        t * t * t * endPoint.X;
  }

  /// <summary>
  ///   Node style implementation which renders an AND or NAND gate.
  /// </summary>
  internal sealed class AndGateNodeStyle : GateNodeStyle
  {
    private readonly bool isNegated;

    public AndGateNodeStyle(bool negated, string fillColor, string strokeColor, string labelColor) : base(fillColor,
        strokeColor, labelColor) {
      isNegated = negated;
    }

    protected override void Render(DrawingContext drawingContext, double width, double height, DrawingVisual visual) {
      var x1 = width * 0.15;
      var x2 = width * 0.6;
      var y1 = height * 0.25;
      var y2 = height * 0.5;
      var y3 = height * 0.75;

      var firstPoint = new Point(x2, 0);
      var endPoint = new Point(x2, height);
      var c1 = new Point(firstPoint.X + width * 0.3, firstPoint.Y + height * 0.1);
      var c2 = new Point(endPoint.X + width * 0.3, endPoint.Y - height * 0.1);
      var extremaX = GetPointOnCurve(0.5, firstPoint, endPoint, c1, c2);

      DrawShape(drawingContext, context => {
        context.BeginFigure(new Point(x1, 0), true, true);
        context.LineTo(new Point(x2, 0), true, false);
        context.BezierTo(c1, c2, endPoint, true, false);
        context.LineTo(new Point(x1, height), true, false);
      });

      var outputX = isNegated ? extremaX + 2 * width * 0.03 : extremaX;
      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(outputX, y2), false, false);
        context.LineTo(new Point(width, y2), true, false);
      });
      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(0, y1), false, false);
        context.LineTo(new Point(x1, y1), true, false);
        context.BeginFigure(new Point(x1, y3), false, false);
        context.LineTo(new Point(0, y3), true, false);
      });

      if (isNegated) {
        DrawBubble(drawingContext, extremaX + width * 0.03, height * 0.5, width * 0.03);
      }
      DrawLabel(drawingContext, visual, isNegated ? "NAND" : "AND", width, height);
    }
  }

  /// <summary>
  ///   Node style implementation which renders a NOT gate.
  /// </summary>
  internal sealed class NotNodeStyle : GateNodeStyle
  {
    public NotNodeStyle(string fillColor, string strokeColor, string labelColor) : base(fillColor, strokeColor,
        labelColor) { }

    protected override void Render(DrawingContext drawingContext, double width, double height, DrawingVisual visual) {
      var x1 = width * 0.2;
      var x2 = width * 0.7;
      DrawShape(drawingContext, context => {
        context.BeginFigure(new Point(x1, 0), true, true);
        context.LineTo(new Point(x2, height * 0.5), true, false);
        context.LineTo(new Point(x1, height), true, false);
      });
      DrawBubble(drawingContext, x2 + width * 0.03, height * 0.5, width * 0.03);

      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(x2 + 2 * width * 0.03, height * 0.5), false, false);
        context.LineTo(new Point(width, height * 0.5), true, false);
      });
      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(0, height * 0.5), false, false);
        context.LineTo(new Point(x1, height * 0.5), true, false);
      });
      DrawLabel(drawingContext, visual, "NOT", width, height, 0.33);
    }
  }

  /// <summary>
  ///   Node style implementation which renders an OR or NOR gate.
  /// </summary>
  internal sealed class OrNodeStyle : GateNodeStyle
  {
    private readonly bool isNegated;

    public OrNodeStyle(bool negated, string fillColor, string strokeColor, string labelColor) : base(fillColor,
        strokeColor, labelColor) {
      isNegated = negated;
    }

    protected override void Render(DrawingContext drawingContext, double width, double height, DrawingVisual visual) {
      var x1 = width * 0.15;
      var x2 = width * 0.8;
      var y1 = height * 0.25;
      var y2 = height * 0.5;
      var y3 = height * 0.75;
      var firstPoint = new Point(x1, 0);
      var endPoint = new Point(x2, y2);
      var c1 = new Point(firstPoint.X + (endPoint.X - firstPoint.X) * 0.5, firstPoint.Y);
      var c2 = new Point(endPoint.X - (endPoint.X - firstPoint.X) * 0.5, endPoint.Y - height * 0.5);
      var outerStart = new Point(x1, height);
      var outerEnd = new Point(x1, 0);
      var outerC1 = new Point(outerStart.X + width * 0.1, outerStart.Y - height * 0.1);
      var outerC2 = new Point(outerEnd.X + width * 0.1, outerEnd.Y + height * 0.1);

      DrawShape(drawingContext, context => {
        context.BeginFigure(firstPoint, true, true);
        context.BezierTo(c1, c2, endPoint, true, false);
        context.BezierTo(new Point(endPoint.X + (outerStart.X - endPoint.X) * 0.5, endPoint.Y + height * 0.5),
            new Point(outerStart.X - (outerStart.X - endPoint.X) * 0.5, outerStart.Y), outerStart, true, false);
        context.BezierTo(outerC1, outerC2, outerEnd, true, false);
      });

      var x11 = GetPointOnCurve(0.3, outerStart, outerEnd, outerC1, outerC2);
      var x21 = GetPointOnCurve(0.7, outerStart, outerEnd, outerC1, outerC2);
      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(0, y1), false, false);
        context.LineTo(new Point(x11, y1), true, false);
        context.BeginFigure(new Point(0, y3), false, false);
        context.LineTo(new Point(x21, y3), true, false);
      });
      DrawLine(drawingContext, CreatePen("black"), context => {
        var outputX = isNegated ? x2 + 2 * width * 0.03 : x2;
        context.BeginFigure(new Point(outputX, y2), false, false);
        context.LineTo(new Point(width, y2), true, false);
      });
      if (isNegated) {
        DrawBubble(drawingContext, x2 + width * 0.03, y2, width * 0.03);
      }
      DrawLabel(drawingContext, visual, isNegated ? "NOR" : "OR", width, height);
    }
  }

  /// <summary>
  ///   Node style implementation which renders an XOR or XNOR gate.
  /// </summary>
  internal sealed class XOrNodeStyle : GateNodeStyle
  {
    private readonly bool isNegated;

    public XOrNodeStyle(bool negated, string fillColor, string strokeColor, string labelColor) : base(fillColor,
        strokeColor, labelColor) {
      isNegated = negated;
    }

    protected override void Render(DrawingContext drawingContext, double width, double height, DrawingVisual visual) {
      var x1 = width * 0.18;
      var x2 = width * 0.25;
      var x3 = width * 0.8;
      var y1 = height * 0.25;
      var y2 = height * 0.5;
      var y3 = height * 0.75;
      var firstPoint = new Point(x2, 0);
      var endPoint = new Point(x3, y2);
      var c1 = new Point(firstPoint.X + (endPoint.X - firstPoint.X) * 0.5, firstPoint.Y);
      var c2 = new Point(endPoint.X - (endPoint.X - firstPoint.X) * 0.5, endPoint.Y - height * 0.5);
      var outerStart = new Point(x2, height);
      var outerEnd = new Point(x2, 0);
      var outerC1 = new Point(outerStart.X + width * 0.1, outerStart.Y - height * 0.1);
      var outerC2 = new Point(outerEnd.X + width * 0.1, outerEnd.Y + height * 0.1);

      DrawShape(drawingContext, context => {
        context.BeginFigure(firstPoint, true, true);
        context.BezierTo(c1, c2, endPoint, true, false);
        context.BezierTo(new Point(endPoint.X + (outerStart.X - endPoint.X) * 0.5, endPoint.Y + height * 0.5),
            new Point(outerStart.X - (outerStart.X - endPoint.X) * 0.5, outerStart.Y), outerStart, true, false);
        context.BezierTo(outerC1, outerC2, outerEnd, true, false);
      });
      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(x1, height), false, false);
        context.BezierTo(new Point(x1 + width * 0.1, height - height * 0.1), new Point(x1 + width * 0.1, height * 0.1),
            new Point(x1, 0), true, false);
      });

      var x11 = GetPointOnCurve(0.3, new Point(x1, height), new Point(x1, 0),
          new Point(x1 + width * 0.1, height - height * 0.1), new Point(x1 + width * 0.1, height * 0.1));
      var x21 = GetPointOnCurve(0.7, new Point(x1, height), new Point(x1, 0),
          new Point(x1 + width * 0.1, height - height * 0.1), new Point(x1 + width * 0.1, height * 0.1));
      DrawLine(drawingContext, CreatePen("black"), context => {
        context.BeginFigure(new Point(0, y1), false, false);
        context.LineTo(new Point(x11, y1), true, false);
        context.BeginFigure(new Point(0, y3), false, false);
        context.LineTo(new Point(x21, y3), true, false);
      });
      DrawLine(drawingContext, CreatePen("black"), context => {
        var outputX = isNegated ? x3 + 2 * width * 0.03 : x3;
        context.BeginFigure(new Point(outputX, y2), false, false);
        context.LineTo(new Point(width, y2), true, false);
      });
      if (isNegated) {
        DrawBubble(drawingContext, x3 + width * 0.03, y2, width * 0.03);
      }
      DrawLabel(drawingContext, visual, isNegated ? "XNOR" : "XOR", width, height);
    }
  }
}
