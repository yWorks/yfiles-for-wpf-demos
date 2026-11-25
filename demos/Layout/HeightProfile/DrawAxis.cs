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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Utils;

namespace yWorks.Demo.HeightProfile
{
  internal static class DrawAxis
  {
    /// <summary>
    ///   Determines how much the axis should extend beyond its first and last value ticks.
    /// </summary>
    private const double AxisExtend = 20;

    /// <summary>
    ///   The size of a value tick in an axis.
    /// </summary>
    private const double TickLength = 3;

    /// <summary>
    ///   The desired maximum value for the horizontal axis.
    /// </summary>
    private const double XAxisMax = 55000;

    /// <summary>
    ///   The desired maximum value for the vertical axis.
    /// </summary>
    private const double YAxisMax = 600;

    /// <summary>
    ///   The desired tick scale for the horizontal axis.
    /// </summary>
    private const double XAxisScale = 5;

    /// <summary>
    ///   The desired tick scale for the vertical axis.
    /// </summary>
    private const double YAxisScale = 50;

    /// <summary>
    ///   The desired unit for the horizontal axis.
    /// </summary>
    private const double XAxisUnit = 1000;

    /// <summary>
    ///   The desired unit for the vertical axis.
    /// </summary>
    private const double YAxisUnit = 1;

    /// <summary>
    ///   The desired label for the horizontal axis.
    /// </summary>
    private const string XAxisLabel = "km";

    /// <summary>
    ///   The desired label for the vertical axis.
    /// </summary>
    private const string YAxisLabel = "m";

    /// <summary>
    ///   Adds visuals for the horizontal and vertical axes to the graph control.
    /// </summary>
    public static void Draw(GraphControl graphControl, IReadOnlyList<Waypoint> trail) {
      var axis = new AxisVisual(graphControl, trail);
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.ContentGroup, (ILookup) axis);
    }

    /// <summary>
    ///   Creates the visualization for the axes, draws the corresponding ticks, and their labels.
    /// </summary>
    private sealed class AxisVisual(GraphControl graphControl, IReadOnlyList<Waypoint> trail)
        : IVisualCreator, IBoundsProvider, ILookup
    {
      private static readonly Typeface AxisTypeface = new("Roboto,sans-serif");
      private static readonly Brush BlackBrush = Brushes.Black;
      private static readonly Pen AxisPen = new(BlackBrush, 1);

      public Visual CreateVisual(IRenderContext context) {
        var visual = new DrawingVisual();
        using (var drawingContext = visual.RenderOpen()) {
          var (maxX, maxY) = ScaleData.GetMax(trail);
          var horizontalAxisLength = XAxisMax * ScaleData.ScaledMaxX / maxX;
          var verticalAxisLength = YAxisMax * ScaleData.ScaledMaxY / maxY;

          AddLine(drawingContext, -AxisExtend, horizontalAxisLength + 2 * AxisExtend, 0, 0);
          AddLine(drawingContext, 0, 0, AxisExtend, -verticalAxisLength - AxisExtend);

          DrawTicksOnHorizontalAxis(drawingContext, horizontalAxisLength);
          DrawTicksOnVerticalAxis(drawingContext, verticalAxisLength);
        }
        return visual;
      }

      /// <summary>
      ///   Draws the horizontal axis ticks and their labels.
      /// </summary>
      private static void DrawTicksOnHorizontalAxis(DrawingContext drawingContext, double axisLength) {
        var tickNumber = GetTickNumber(XAxisMax, XAxisUnit, XAxisScale);
        for (var i = 0; i <= tickNumber; i++) {
          // find the position of the tick and draw it with its label
          var tickPosition = i * (axisLength / tickNumber);
          AddLine(drawingContext, tickPosition, tickPosition, -TickLength, TickLength);
          if (i != 0) {
            AddTickLabel(drawingContext, (XAxisScale * i).ToString(CultureInfo.InvariantCulture), tickPosition,
                -TickLength, true);
          }
        }

        // draw the last tick of the axis and add a label with the axis unit
        AddLine(drawingContext, axisLength + 2 * AxisExtend, axisLength + 2 * AxisExtend, -TickLength, TickLength);
        AddLabel(drawingContext, $"({XAxisLabel})", axisLength + 3.5 * AxisExtend, 0);
      }

      /// <summary>
      ///   Draws the vertical axis ticks and their labels.
      /// </summary>
      private static void DrawTicksOnVerticalAxis(DrawingContext drawingContext, double axisLength) {
        var tickNumber = GetTickNumber(YAxisMax, YAxisUnit, YAxisScale);
        for (var i = 0; i <= tickNumber; i++) {
          // find the position of the tick and draw it with its label
          var tickPosition = i * (axisLength / tickNumber);
          AddLine(drawingContext, -TickLength, TickLength, -tickPosition, -tickPosition);
          if (i != 0) {
            AddTickLabel(drawingContext, (YAxisScale * i).ToString(CultureInfo.InvariantCulture), -TickLength,
                -tickPosition, false);
          }
        }

        // draw the last tick of the axis and add a label with the axis unit
        AddLine(drawingContext, -TickLength, TickLength, -axisLength - AxisExtend, -axisLength - AxisExtend);
        AddLabel(drawingContext, $"({YAxisLabel})", -AxisExtend, -axisLength - 2 * AxisExtend);
      }

      /// <summary>
      ///   Returns the number of necessary ticks based on the length of the given axis, its unit, and scale.
      /// </summary>
      private static double GetTickNumber(double axisLength, double unit, double scale) => axisLength / unit / scale;

      /// <summary>
      ///   Adds a line for the given coordinates.
      /// </summary>
      private static void AddLine(DrawingContext drawingContext, double x1, double x2, double y1, double y2) =>
          drawingContext.DrawLine(AxisPen, new Point(x1, y1), new Point(x2, y2));

      /// <summary>
      ///   Adds a tick label at the given location.
      /// </summary>
      private static void AddTickLabel(
          DrawingContext drawingContext,
          string labelText,
          double x,
          double y,
          bool horizontalAxis) {
        var text = CreateFormattedText(labelText);
        AddLabel(drawingContext, labelText, x - (horizontalAxis ? 0 : text.Width - 5),
            y + (horizontalAxis ? text.Height + 5 : 0));
      }

      /// <summary>
      ///   Adds a text element at the given location.
      /// </summary>
      private static void AddLabel(DrawingContext drawingContext, string textContent, double x, double y) {
        var text = CreateFormattedText(textContent);
        drawingContext.DrawText(text, new Point(x - text.Width * 0.5, y - text.Height * 0.5));
      }

      private static FormattedText CreateFormattedText(string text) =>
          new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, AxisTypeface, 14, BlackBrush, 1);

      /// <summary>
      ///   Returns the bounds of this visual so that the size of the axis is also considered
      ///   when fit graph bounds is called.
      /// </summary>
      public RectD GetBounds(ICanvasContext context) {
        // calculate the bounding box that contains all nodes
        var nodes = graphControl.Graph.Nodes.ToArray();
        var minX = nodes.Min(node => node.Layout.X);
        var minY = nodes.Min(node => node.Layout.Y);
        var maxX = nodes.Max(node => node.Layout.X);
        var maxY = nodes.Max(node => node.Layout.Y);
        return new RectD(minX, minY, maxX - minX + 150, // add some offset to fit the axis' labels (approximately)
            maxY - minY + 300                           // add some offset to fit the axis' labels (approximately)
        );
      }

      /// <summary>
      ///   Delegates to <see cref="CreateVisual"/>.
      /// </summary>
      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisual(context);

      public object? Lookup(Type type) => type.IsInstanceOfType(this) ? this : null;
    }
  }
}
