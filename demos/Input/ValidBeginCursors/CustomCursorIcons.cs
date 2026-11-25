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
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace yWorks.Demo.ValidBeginCursors
{
  public static class CustomCursorIcons
  {
    private const int CursorSize = 32;
    private static readonly List<MemoryStream> CursorStreams = [];
    private static readonly Pen OutlinePen = new(Brushes.Black, 1.4) { LineJoin = PenLineJoin.Round };
    private static readonly Pen LightPen = new(Brushes.White, 3.2) { LineJoin = PenLineJoin.Round };

    public static readonly Cursor CreateEdge = CreateCursor(DrawCreateEdge);
    public static readonly Cursor CrosshairPlus = CreateCursor(context => DrawCrosshair(context, true), 15, 15);
    public static readonly Cursor CrosshairMinus = CreateCursor(context => DrawCrosshair(context, false), 15, 15);
    public static readonly Cursor Lasso = CreateCursor(context => DrawLasso(context, null), 16, 13);
    public static readonly Cursor LassoPlus = CreateCursor(context => DrawLasso(context, true), 16, 13);
    public static readonly Cursor LassoMinus = CreateCursor(context => DrawLasso(context, false), 16, 13);

    private static Cursor CreateCursor(Action<DrawingContext> draw, int hotspotX = 2, int hotspotY = 2) {
      var visual = new DrawingVisual();
      using (var context = visual.RenderOpen()) {
        draw(context);
      }

      var bitmap = new RenderTargetBitmap(CursorSize, CursorSize, 96, 96, PixelFormats.Pbgra32);
      bitmap.Render(visual);
      var pixels = new byte[CursorSize * CursorSize * 4];
      bitmap.CopyPixels(pixels, CursorSize * 4, 0);
      var mask = new byte[CursorSize * 4 * CursorSize / 8];
      for (var y = 0; y < CursorSize; y++) {
        for (var x = 0; x < CursorSize; x++) {
          if (pixels[(y * CursorSize + x) * 4 + 3] == 0) {
            mask[(CursorSize - 1 - y) * 4 + x / 8] |= (byte) (1 << (7 - x % 8));
          }
        }
      }

      using var imageStream = new MemoryStream();
      using (var writer = new BinaryWriter(imageStream, Encoding.UTF8, true)) {
        writer.Write(40);
        writer.Write(CursorSize);
        writer.Write(CursorSize * 2);
        writer.Write((short) 1);
        writer.Write((short) 32);
        writer.Write(0);
        writer.Write(pixels.Length + mask.Length);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        for (var y = CursorSize - 1; y >= 0; y--) {
          for (var x = 0; x < CursorSize; x++) {
            var offset = (y * CursorSize + x) * 4;
            writer.Write(pixels[offset]);
            writer.Write(pixels[offset + 1]);
            writer.Write(pixels[offset + 2]);
            writer.Write(pixels[offset + 3]);
          }
        }
        writer.Write(mask);
      }

      var cursorStream = new MemoryStream();
      using (var writer = new BinaryWriter(cursorStream, Encoding.UTF8, true)) {
        writer.Write((short) 0);
        writer.Write((short) 2);
        writer.Write((short) 1);
        writer.Write((byte) CursorSize);
        writer.Write((byte) CursorSize);
        writer.Write((byte) 0);
        writer.Write((byte) 0);
        writer.Write((short) hotspotX);
        writer.Write((short) hotspotY);
        writer.Write((uint) imageStream.Length);
        writer.Write(22);
        writer.Write(imageStream.ToArray());
      }
      cursorStream.Position = 0;
      CursorStreams.Add(cursorStream);
      return new Cursor(cursorStream);
    }

    private static void DrawCreateEdge(DrawingContext context) {
      DrawArrow(context);
      context.DrawLine(LightPen, new Point(13, 23), new Point(27, 23));
      context.DrawLine(OutlinePen, new Point(13, 23), new Point(27, 23));
      context.DrawRectangle(Brushes.White, OutlinePen, new Rect(23, 19, 7, 7));
    }

    private static void DrawCrosshair(DrawingContext context, bool add) {
      context.DrawLine(LightPen, new Point(15, 2), new Point(15, 29));
      context.DrawLine(LightPen, new Point(2, 15), new Point(29, 15));
      context.DrawLine(OutlinePen, new Point(15, 2), new Point(15, 29));
      context.DrawLine(OutlinePen, new Point(2, 15), new Point(29, 15));
      DrawBadge(context, add);
    }

    private static void DrawLasso(DrawingContext context, bool? add) {
      context.DrawEllipse(null, LightPen, new Point(16, 13), 11, 8);
      context.DrawEllipse(null, OutlinePen, new Point(16, 13), 11, 8);
      context.DrawLine(LightPen, new Point(8, 19), new Point(4, 27));
      context.DrawLine(OutlinePen, new Point(8, 19), new Point(4, 27));
      DrawLassoArrow(context);
      if (add.HasValue) {
        DrawBadge(context, add.Value);
      }
    }

    private static void DrawLassoArrow(DrawingContext context) {
      var geometry = System.Windows.Media.Geometry.Parse("M 16,13 L 16,23 L 19,20 L 22,27 L 25,25 L 22,18 L 26,18 Z");
      context.DrawGeometry(Brushes.White, OutlinePen, geometry);
    }

    private static void DrawArrow(DrawingContext context, double offsetX = 0, double offsetY = 0) {
      var geometry = System.Windows.Media.Geometry.Parse("M 2,1 L 2,22 L 8,16 L 12,27 L 16,25 L 11,14 L 20,14 Z")
          .Clone();
      geometry.Transform = new TranslateTransform(offsetX, offsetY);
      context.DrawGeometry(Brushes.White, OutlinePen, geometry);
    }

    private static void DrawBadge(DrawingContext context, bool add) {
      context.DrawEllipse(Brushes.White, OutlinePen, new Point(25, 25), 6, 6);
      context.DrawLine(OutlinePen, new Point(21, 25), new Point(29, 25));
      if (add) {
        context.DrawLine(OutlinePen, new Point(25, 21), new Point(25, 29));
      }
    }
  }
}
