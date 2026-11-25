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

using System.Windows;
using System.Windows.Media;
using yWorks.Geometry;

namespace yWorks.Demo.LabelHandleProvider
{
  /// <summary>Creates oriented rectangle visuals used by the custom label handles.</summary>
  internal static class LabelHandleVisuals
  {
    public static DrawingVisual Create(
        IOrientedRectangle layout,
        bool showRotationDetails,
        Color outlineColor,
        double outlineWidth,
        Color? centerColor = null) {
      var visual = new DrawingVisual {
          Transform = new MatrixTransform(-layout.UpY, layout.UpX, layout.UpX, layout.UpY, layout.AnchorX,
              layout.AnchorY)
      };
      using var drawing = visual.RenderOpen();
      var outline = new Pen(new SolidColorBrush(outlineColor), outlineWidth);
      drawing.DrawRectangle(null, outline, new Rect(0, 0, layout.Width, layout.Height));

      if (showRotationDetails) {
        var halfWidth = layout.Width * 0.5;
        var halfHeight = layout.Height * 0.5;
        drawing.DrawLine(outline, new Point(halfWidth, layout.Height), new Point(halfWidth, layout.Height + 15));
        var centerPen = new Pen(new SolidColorBrush(centerColor ?? Colors.Black), 1);
        drawing.DrawEllipse(null, centerPen, new Point(halfWidth, halfHeight), 4, 4);
      }
      return visual;
    }
  }
}
