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
using yWorks.Controls;
using yWorks.Layout.Grid;

namespace yWorks.Demo.SimpleLayoutGrid
{
  /// <summary>
  ///   Visualizes the layout grid that has been used in the layout.
  ///   Each grid cell is visualized as a rectangle.
  /// </summary>
  public sealed class SimpleLayoutGridVisualCreator : IVisualCreator
  {
    private static readonly Brush GridFill = CreateGridFill();
    private static readonly Pen GridPen = CreateGridPen();

    /// <summary>
    ///   The layout grid to be visualized
    /// </summary>
    private readonly LayoutGrid grid;

    /// <summary>
    ///   Creates a new instance of LayoutGridVisualCreator.
    /// </summary>
    /// <param name="grid">The layout grid to be visualized</param>
    public SimpleLayoutGridVisualCreator(LayoutGrid grid) {
      this.grid = grid;
    }

    /// <summary>
    ///   Creates the visual for the given layout grid.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used</param>
    /// <returns>The visual</returns>
    public Visual CreateVisual(IRenderContext context) {
      var visual = new DrawingVisual();
      UpdateVisualContent(visual);
      return visual;
    }

    /// <summary>
    ///   Updates the visual for the given layout grid. In particular, method <see cref="CreateVisual"/> is called.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used</param>
    /// <param name="oldVisual">
    ///   The visual instance that had been returned the last time the create visual
    ///   method was called on this instance
    /// </param>
    /// <returns>The updated visual</returns>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is not DrawingVisual drawingVisual) {
        return CreateVisual(context);
      }
      UpdateVisualContent(drawingVisual);
      return drawingVisual;
    }

    private void UpdateVisualContent(DrawingVisual visual) {
      using var drawingContext = visual.RenderOpen();
      foreach (var row in grid.Rows) {
        foreach (var column in grid.Columns) {
          var x = column.Position;
          var y = row.Position;
          var height = row.Height;
          var width = column.Width;
          drawingContext.DrawRectangle(GridFill, GridPen, new Rect(x, y, width, height));
        }
      }
    }

    private static Brush CreateGridFill() {
      var brush = new SolidColorBrush(Color.FromRgb(0xDA, 0xD5, 0xC1));
      brush.Freeze();
      return brush;
    }

    private static Pen CreateGridPen() {
      var pen = new Pen(Brushes.White, 1);
      pen.Freeze();
      return pen;
    }
  }
}
