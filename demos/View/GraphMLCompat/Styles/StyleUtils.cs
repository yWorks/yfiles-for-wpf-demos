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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Geometry;

namespace yWorks.Demo.GraphMLCompat.Styles
{
  internal static class StyleUtils
  {
    internal static void AdjustStylusHoldEvents(Visual element, bool enabled) {
      Stylus.SetIsPressAndHoldEnabled(element, enabled);

      var childCount = VisualTreeHelper.GetChildrenCount(element);
      for (var i = 0; i < childCount; ++i) {
        var child = VisualTreeHelper.GetChild(element, i) as Visual;
        if (child != null) {
          AdjustStylusHoldEvents(child, enabled);
        }
      }
    }

    internal static PointD? FindLineIntersectionWithRoundRect(
        PointD inner,
        PointD outer,
        IRectangle rect,
        double rectArcRadius) {
      var layout = rect.ToRectD();

      var intersection = layout.FindLineIntersection(inner, outer);

      // no intersection with the rectangle - we should have an intersection unless outer is inside the rectangle but outside the round rect.
      if (intersection != null) {
        var i = intersection.Value;
        if ((i.X >= layout.X + rectArcRadius && i.X <= layout.X + layout.Width - rectArcRadius) ||
            (i.Y >= layout.Y + rectArcRadius && i.Y <= layout.Y + layout.Height - rectArcRadius)) {
          // trivial case - line intersects rectangular part
          return i;
        }

        // see if we have perfect circles in the corners
        if (layout.Width >= rectArcRadius * 2 && layout.Height >= rectArcRadius * 2) {
          // no trivial case but perfect circles - find out quadrant
          var left = i.X < layout.X + layout.Width * 0.5;
          var top = i.Y < layout.Y + layout.Height * 0.5;

          // determine center of circle
          var cx = left ? layout.X + rectArcRadius : layout.X + layout.Width - rectArcRadius;
          var cy = top ? layout.Y + rectArcRadius : layout.Y + layout.Height - rectArcRadius;

          var dx = outer.X - inner.X;
          var dy = outer.Y - inner.Y;

          var innerX = inner.X - cx;
          var innerY = inner.Y - cy;

          var outerX = innerX + dx;
          var outerY = innerY + dy;

          var r = rectArcRadius;

          var dr2 = dx * dx + dy * dy;
          var D = innerX * outerY - outerX * innerY;

          var disc = r * r * dr2 - D * D;

          // calculate intersections
          if (disc >= 0) {
            var sqrtDisc = Math.Sqrt(disc);
            return new PointD(cx + (D * dy + dx * sqrtDisc) / dr2, cy + (-D * dx + dy * sqrtDisc) / dr2);
          } // else fall through to old code for edge cases
        }
      }
      // fall back to complex path code
      var t = CreateRoundRectanglePath(layout.X, layout.Y, layout.Width, layout.Height, rectArcRadius)
          .FindLineIntersection(inner, outer);
      if (t < double.PositiveInfinity) {
        return new PointD(inner.X + (outer.X - inner.X) * t, inner.Y + (outer.Y - inner.Y) * t);
      }
      return null;
    }

    internal static bool RoundRectContains(PointD i, IRectangle rectangle, double rectArcRadius) {
      var layoutX = rectangle.X;
      var layoutY = rectangle.Y;
      var layoutW = rectangle.Width;
      var layoutH = rectangle.Height;
      if (i.X < layoutX || i.Y < layoutY || i.X > layoutX + layoutW || i.Y > layoutY + layoutH) {
        // definitely no
        return false;
      }

      // see if x fits
      if (i.X >= layoutX + rectArcRadius && i.X <= layoutX + layoutW - rectArcRadius) {
        return true;
      }

      // see if y fits
      if (i.Y >= layoutY + rectArcRadius && i.Y <= layoutY + layoutH - rectArcRadius) {
        return true;
      }

      // see if we have perfect circles in the corners
      if (layoutW >= rectArcRadius * 2 && layoutH >= rectArcRadius * 2) {
        // no trivial case but perfect circles - find out quadrant
        // determine center of circle
        var dx = i.X - (i.X < layoutX + layoutW * 0.5 ? layoutX + rectArcRadius : layoutX + layoutW - rectArcRadius);
        var dy = i.Y - (i.Y < layoutY + layoutH * 0.5 ? layoutY + rectArcRadius : layoutY + layoutH - rectArcRadius);

        return dx * dx + dy * dy <= rectArcRadius * rectArcRadius;
      }

      // fallback code:
      return CreateRoundRectanglePath(layoutX, layoutY, layoutW, layoutH, rectArcRadius).AreaContains(i);
    }

    internal static bool RoundRectIsHit(PointD i, IRectangle rectangle, double rectArcRadius, double eps) {
      var layoutX = rectangle.X;
      var layoutY = rectangle.Y;
      var layoutW = rectangle.Width;
      var layoutH = rectangle.Height;
      if (i.X < layoutX - eps || i.Y < layoutY - eps || i.X > layoutX + layoutW + eps ||
          i.Y > layoutY + layoutH + eps) {
        // definitely no
        return false;
      }

      // see if x fits
      if (i.X >= layoutX + rectArcRadius && i.X <= layoutX + layoutW - rectArcRadius) {
        return true;
      }

      // see if y fits
      if (i.Y >= layoutY + rectArcRadius && i.Y <= layoutY + layoutH - rectArcRadius) {
        return true;
      }

      // see if we have perfect circles in the corners
      if (layoutW >= rectArcRadius * 2 && layoutH >= rectArcRadius * 2) {
        // no trivial case but perfect circles - find out quadrant
        // determine center of circle
        var dx = i.X - (i.X < layoutX + layoutW * 0.5 ? layoutX + rectArcRadius : layoutX + layoutW - rectArcRadius);
        var dy = i.Y - (i.Y < layoutY + layoutH * 0.5 ? layoutY + rectArcRadius : layoutY + layoutH - rectArcRadius);
        return dx * dx + dy * dy <= (rectArcRadius + eps) * (rectArcRadius + eps);
      }

      // fallback code:
      return CreateRoundRectanglePath(layoutX, layoutY, layoutW, layoutH, rectArcRadius).AreaContains(i, eps);
    }

    private static readonly double BezierArcApproximation = 0.552284749830794; // (Math.Sqrt(2) - 1) * 4 / 3

    internal static GeneralPath CreateRoundRectanglePath(
        double x,
        double y,
        double width,
        double height,
        double radius) {
      var roundRect = new GeneralPath();
      var arcX = Math.Min(width * 0.5, radius);
      var arcY = Math.Min(height * 0.5, radius);

      var cx = (1 - BezierArcApproximation) * arcX;
      var cy = (1 - BezierArcApproximation) * arcY;

      roundRect.MoveTo(x, y + arcY);
      roundRect.CubicTo(x, y + cy, x + cx, y, x + arcX, y);
      roundRect.LineTo(x + width - arcX, y);
      roundRect.CubicTo(x + width - cx, y, x + width, y + cy, x + width, y + arcY);
      roundRect.LineTo(x + width, y + height - arcY);
      roundRect.CubicTo(x + width, y + height - cy, x + width - cx, y + height, x + width - arcX, y + height);
      roundRect.LineTo(x + arcX, y + height);
      roundRect.CubicTo(x + cx, y + height, x, y + height - cy, x, y + height - arcY);
      roundRect.Close();
      return roundRect;
    }


    internal static Color MixColors(Color color0, Color color1, double ratio) {
      var iratio = 1 - ratio;
      var a = color0.A * ratio + color1.A * iratio;
      var r = color0.R * ratio + color1.R * iratio;
      var g = color0.G * ratio + color1.G * iratio;
      var b = color0.B * ratio + color1.B * iratio;
      return Color.FromArgb((byte) Math.Round(a), (byte) Math.Round(r), (byte) Math.Round(g), (byte) Math.Round(b));
    }
  }
}
