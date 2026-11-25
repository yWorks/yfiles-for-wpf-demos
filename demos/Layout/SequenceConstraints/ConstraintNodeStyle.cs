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

namespace yWorks.Demo.SequenceConstraints
{
  /// <summary>
  ///   Renders the sequence value and the constraint controls inside each node.
  /// </summary>
  internal sealed class ConstraintNodeStyle : NodeStyleBase<ConstraintNodeStyle.ConstraintNodeVisual>
  {
    protected override ConstraintNodeVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new ConstraintNodeVisual();
      UpdateConstraintNodeVisual(node, visual);
      return visual;
    }

    protected override ConstraintNodeVisual UpdateVisual(
        IRenderContext context,
        ConstraintNodeVisual oldVisual,
        INode node) {
      UpdateConstraintNodeVisual(node, oldVisual);
      return oldVisual;
    }

    private static void UpdateConstraintNodeVisual(INode node, ConstraintNodeVisual visual) {
      var layout = node.Layout;
      visual.UpdateLocation(layout.X, layout.Y);
      visual.Render(layout.Width, layout.Height, node.Tag as SequenceConstraintsData);
    }

    /// <summary>The visual that renders the constraint controls and their current value.</summary>
    internal sealed class ConstraintNodeVisual : DrawingVisual
    {
      private static readonly Brush DisabledBrush = CreateBrush(Colors.LightGray);
      private static readonly Brush WhiteBrush = CreateBrush(Colors.White);
      private readonly TranslateTransform translation = new();

      public ConstraintNodeVisual() {
        Transform = translation;
      }

      public void UpdateLocation(double x, double y) {
        if (translation.X != x) {
          translation.X = x;
        }
        if (translation.Y != y) {
          translation.Y = y;
        }
      }

      public void Render(double width, double height, SequenceConstraintsData? data) {
        using var drawingContext = RenderOpen();
        if (data is not { Constraints: true }) {
          drawingContext.DrawRoundedRectangle(DisabledBrush, null, new Rect(0, 0, width, height), 4, 4);
          DrawDisabledButton(drawingContext, width, height);
          return;
        }

        drawingContext.DrawRoundedRectangle(GetBackgroundBrush(data.Value), null, new Rect(0, 0, width, height), 4, 4);
        DrawValue(drawingContext, width, height, data.Value);

        // Draw the controls at the bottom of the node.
        DrawEnabledButtons(drawingContext, height);
      }

      private void DrawValue(DrawingContext drawingContext, double width, double height, int value) {
        var text = value == 0 ? "First" : value == 7 ? "Last" : value.ToString(CultureInfo.InvariantCulture);
        var foreground = value is 0 or 3 ? Brushes.Black : Brushes.White;
        var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Arial"), 22, foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        drawingContext.DrawText(formattedText, new Point((width - formattedText.Width) / 2, 5));
      }

      private static void DrawEnabledButtons(DrawingContext drawingContext, double height) {
        var y = height - 12;
        drawingContext.DrawEllipse(WhiteBrush, null, new Point(12, y), 8, 8);
        drawingContext.DrawEllipse(WhiteBrush, null, new Point(30, y), 8, 8);
        drawingContext.DrawEllipse(WhiteBrush, null, new Point(48, y), 8, 8);

        var grayPen = CreatePen(Colors.Gray, 2);
        drawingContext.DrawLine(grayPen, new Point(7, y), new Point(17, y));
        drawingContext.DrawLine(grayPen, new Point(43, y), new Point(53, y));
        drawingContext.DrawLine(grayPen, new Point(48, y - 5), new Point(48, y + 5));

        var lockPen = CreatePen(Colors.Gray, 1.5);
        drawingContext.DrawEllipse(null, lockPen, new Point(30, y - 2), 2, 3);
        drawingContext.DrawRoundedRectangle(Brushes.Gray, null, new Rect(25.5, y - 2, 9, 7), 1, 1);
        drawingContext.DrawLine(CreatePen(Colors.White, 1), new Point(30, y + 1), new Point(30, y + 3));
        drawingContext.DrawEllipse(Brushes.White, null, new Point(30, y + 1), 1, 1);
      }

      private static void DrawDisabledButton(DrawingContext drawingContext, double width, double height) {
        var center = new Point(width * 0.5, height * 0.5);
        drawingContext.DrawEllipse(WhiteBrush, null, center, 15, 15);

        var grayPen = CreatePen(Colors.Gray, 2);
        drawingContext.DrawEllipse(null, grayPen, new Point(center.X, center.Y - 4), 4, 6);
        drawingContext.DrawRoundedRectangle(Brushes.Gray, null, new Rect(center.X - 9, center.Y - 5, 18, 14), 2, 2);
        drawingContext.DrawLine(CreatePen(Colors.White, 2), new Point(center.X, center.Y),
            new Point(center.X, center.Y + 5));
        drawingContext.DrawEllipse(Brushes.White, null, new Point(center.X, center.Y), 2, 2);
      }

      private static Brush GetBackgroundBrush(int value) {
        if (value == 0) {
          return Brushes.YellowGreen;
        }
        if (value == 7) {
          return Brushes.IndianRed;
        }

        var channel = (byte) Math.Round(value * 255d / 7, MidpointRounding.AwayFromZero);
        return CreateBrush(Color.FromRgb(channel, channel, byte.MaxValue));
      }

      private static Brush CreateBrush(Color color) {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
      }

      private static Pen CreatePen(Color color, double thickness) {
        var pen = new Pen(CreateBrush(color), thickness);
        pen.Freeze();
        return pen;
      }
    }
  }
}
