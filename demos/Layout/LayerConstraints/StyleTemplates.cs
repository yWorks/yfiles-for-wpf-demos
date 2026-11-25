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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LayerConstraints
{
  internal static class StyleTemplates
  {
    public static INodeStyle ConstraintNodeStyle { get; } = new ConstraintNodeStyleImpl();

    private static Color GetBackgroundColor(int value) {
      switch (value) {
        case 0:
          return Colors.YellowGreen;
        case 7:
          return Colors.IndianRed;
        default: {
          var component = (byte) Math.Round(value * 255.0 / 7);
          return Color.FromRgb(component, component, 255);
        }
      }
    }

    private static Brush GetTextColor(int value) => value is 0 or 3 ? Brushes.Black : Brushes.White;

    private sealed class ConstraintNodeStyleImpl : NodeStyleBase<DrawingVisual>
    {
      protected override DrawingVisual CreateVisual(IRenderContext context, INode node) {
        var visual = new DrawingVisual();
        Render(visual, node);
        return visual;
      }

      protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, INode node) {
        Render(oldVisual, node);
        return oldVisual;
      }

      private static void Render(DrawingVisual visual, INode node) {
        var layout = node.Layout;
        var data = node.Tag as LayerConstraintsDemo.LayerConstraintsData;
        using var drawing = visual.RenderOpen();

        if (data?.Constraints == true) {
          var background = new SolidColorBrush(GetBackgroundColor(data.Value));
          background.Freeze();
          drawing.DrawRoundedRectangle(background, null, new Rect(layout.X, layout.Y, layout.Width, layout.Height), 4,
              4);

          var text = data.Value switch {
              0 => "First",
              7 => "Last",
              _ => data.Value.ToString(CultureInfo.InvariantCulture)
          };
          var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
              new Typeface("Arial"), 22, GetTextColor(data.Value), 1);
          drawing.DrawText(formattedText,
              new Point(layout.X + (layout.Width - formattedText.Width) * 0.5, layout.Y + 5));

          DrawEnabledButtons(drawing, layout);
        } else {
          drawing.DrawRoundedRectangle(Brushes.LightGray, null,
              new Rect(layout.X, layout.Y, layout.Width, layout.Height), 4, 4);
          DrawDisabledButton(drawing, layout);
        }
      }

      private static void DrawEnabledButtons(DrawingContext drawing, IRectangle layout) {
        var y = layout.Y + 38;
        drawing.DrawEllipse(Brushes.White, null, new Point(layout.X + 12, y), 8, 8);
        drawing.DrawEllipse(Brushes.White, null, new Point(layout.X + 30, y), 8, 8);
        drawing.DrawEllipse(Brushes.White, null, new Point(layout.X + 48, y), 8, 8);

        var grayPen = new Pen(Brushes.Gray, 2);
        grayPen.Freeze();
        drawing.DrawLine(grayPen, new Point(layout.X + 7, y), new Point(layout.X + 17, y));
        drawing.DrawLine(grayPen, new Point(layout.X + 43, y), new Point(layout.X + 53, y));
        drawing.DrawLine(grayPen, new Point(layout.X + 48, y - 5), new Point(layout.X + 48, y + 5));

        var lockPen = new Pen(Brushes.Gray, 1.5);
        lockPen.Freeze();
        drawing.DrawEllipse(null, lockPen, new Point(layout.X + 30, y - 2), 2, 3);
        drawing.DrawRoundedRectangle(Brushes.Gray, null, new Rect(layout.X + 25.5, y - 2, 9, 7), 1, 1);
        drawing.DrawLine(new Pen(Brushes.White, 1), new Point(layout.X + 30, y + 1), new Point(layout.X + 30, y + 3));
        drawing.DrawEllipse(Brushes.White, null, new Point(layout.X + 30, y + 1), 1, 1);
      }

      private static void DrawDisabledButton(DrawingContext drawing, IRectangle layout) {
        var center = new Point(layout.X + layout.Width * 0.5, layout.Y + layout.Height * 0.5);
        drawing.DrawEllipse(Brushes.White, null, center, 15, 15);

        var grayPen = new Pen(Brushes.Gray, 2);
        grayPen.Freeze();
        drawing.DrawEllipse(null, grayPen, new Point(center.X, center.Y - 4), 4, 6);
        drawing.DrawRoundedRectangle(Brushes.Gray, null, new Rect(center.X - 9, center.Y - 5, 18, 14), 2, 2);
        var whitePen = new Pen(Brushes.White, 2);
        whitePen.Freeze();
        drawing.DrawLine(whitePen, new Point(center.X, center.Y), new Point(center.X, center.Y + 5));
        drawing.DrawEllipse(Brushes.White, null, new Point(center.X, center.Y), 2, 2);
      }
    }
  }
}
