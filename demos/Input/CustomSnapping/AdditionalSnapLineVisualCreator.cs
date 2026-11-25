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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.CustomSnapping
{
  /// <summary>
  ///   A visual creator for orthogonal snap lines.
  /// </summary>
  internal sealed class AdditionalSnapLineVisualCreator : IVisualCreator
  {
    private static readonly Pen LinePen = CreateLinePen();

    public PointD To { get; set; }

    public PointD From { get; set; }

    /// <summary>
    ///   Creates a new instance of <see cref="AdditionalSnapLineVisualCreator"/>.
    /// </summary>
    /// <param name="from">The start point</param>
    /// <param name="to">The end point</param>
    public AdditionalSnapLineVisualCreator(PointD from, PointD to) {
      From = from;
      To = to;
    }

    /// <summary>
    ///   Creates the <see cref="OrthogonalSnapLine"/>s that are displayed by this visual creator.
    ///   Since items should be able to snap from both sides to this line, two snap lines with the same location and
    ///   different <see cref="SnapLineSnapTypes"/>s are created.
    /// </summary>
    public IEnumerable<OrthogonalSnapLine> CreateSnapLines() {
      var lines = new List<OrthogonalSnapLine>();
      if (From.X == To.X) {
        // it's vertical
        var center = PointD.MidPoint(From, To);
        lines.Add(new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Left,
            SnapReferenceVisualizationType.FixedLine, center, From.Y, To.Y, false, 50));
        lines.Add(new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Right,
            SnapReferenceVisualizationType.FixedLine, center, From.Y, To.Y, false, 50));
      } else if (From.Y == To.Y) {
        // it's horizontal
        var center = PointD.MidPoint(From, To);
        lines.Add(new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Top,
            SnapReferenceVisualizationType.FixedLine, center, From.X, To.X, false, 50));
        lines.Add(new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Bottom,
            SnapReferenceVisualizationType.FixedLine, center, From.X, To.X, false, 50));
      }

      return lines;
    }

    /// <summary>
    ///   Creates the visual for the orthogonal snap lines.
    /// </summary>
    public Visual CreateVisual(IRenderContext context) => CreateLineVisual();

    /// <summary>
    ///   Updates a previously created visual.
    /// </summary>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is not DrawingVisual drawingVisual) {
        return CreateVisual(context);
      }

      DrawLine(drawingVisual);
      return drawingVisual;
    }

    private static Pen CreateLinePen() {
      var pen = new Pen(new SolidColorBrush(Color.FromRgb(0xCA, 0x0C, 0x3B)), 2);
      pen.Freeze();
      return pen;
    }

    private DrawingVisual CreateLineVisual() {
      var visual = new DrawingVisual();
      DrawLine(visual);
      return visual;
    }

    private void DrawLine(DrawingVisual visual) {
      using var drawingContext = visual.RenderOpen();
      drawingContext.DrawLine(LinePen, new Point(From.X, From.Y), new Point(To.X, To.Y));
    }
  }
}
