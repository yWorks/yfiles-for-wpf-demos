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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.ClearRectangleArea
{
  /// <summary>
  ///   Creates the visualization for the clear rectangle.
  /// </summary>
  internal sealed class ClearRectangleRenderer : ObjectRendererBase<MutableRectangle, DrawingVisual>
  {
    private static readonly Brush FillBrush = CreateBrush(Color.FromArgb(166, 0, 187, 255));
    private static readonly Pen StrokePen = CreatePen(Color.FromArgb(166, 77, 131, 153));

    protected override DrawingVisual CreateVisual(IRenderContext context, MutableRectangle rectangle) {
      var visual = new DrawingVisual();
      using var drawingContext = visual.RenderOpen();
      drawingContext.DrawRectangle(FillBrush, StrokePen, rectangle.ToRect());
      return visual;
    }

    protected override DrawingVisual UpdateVisual(
        IRenderContext context,
        DrawingVisual oldVisual,
        MutableRectangle rectangle) =>
        CreateVisual(context, rectangle);

    protected override RectD GetBounds(ICanvasContext context, MutableRectangle rectangle) => rectangle.ToRectD();

    protected override bool IsVisible(ICanvasContext context, RectD visibleRectangle, MutableRectangle rectangle) =>
        true;

    protected override bool IsHit(IInputModeContext context, PointD location, MutableRectangle rectangle) => false;

    private static Brush CreateBrush(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }

    private static Pen CreatePen(Color color) {
      var pen = new Pen(CreateBrush(color), 1.5);
      pen.Freeze();
      return pen;
    }
  }
}
