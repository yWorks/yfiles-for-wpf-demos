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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;

namespace yWorks.Demo.ViewportLimiter
{
  internal sealed class RectangleVisualCreator : IVisualCreator
  {
    private readonly string title;
    private readonly bool titleAtTop;
    private readonly Brush brush;
    private readonly Pen pen;
    private readonly Func<RectD?> boundsProvider;
    private readonly bool renderInViewCoordinates;

    public RectangleVisualCreator(
        string title,
        bool titleAtTop,
        Brush brush,
        Pen pen,
        Func<RectD?> boundsProvider,
        bool renderInViewCoordinates = false) {
      this.title = title;
      this.titleAtTop = titleAtTop;
      this.brush = brush;
      this.pen = pen;
      this.boundsProvider = boundsProvider;
      this.renderInViewCoordinates = renderInViewCoordinates;
    }

    /// <summary>
    ///   Creates the visual that renders the content bounds.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used.</param>
    /// <returns>The visual for the background.</returns>
    public Visual CreateVisual(IRenderContext context) {
      var groupVisual = new VisualGroup();
      var rectangleVisual = new Rectangle { Fill = brush };
      rectangleVisual.SetPen(pen);
      var textVisual = new DrawingVisual();

      groupVisual.Add(rectangleVisual);
      groupVisual.Add(textVisual);
      return UpdateVisual(context, groupVisual);
    }

    /// <summary>
    ///   Updates the content bounds visual.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used.</param>
    /// <param name="oldVisual">The old visual.</param>
    /// <returns>The visual that renders the content bounds.</returns>
    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is not VisualGroup group || group.Children.Count != 2) {
        return CreateVisual(context);
      }

      var rect = boundsProvider();
      if (!rect.HasValue) {
        return new VisualGroup();
      }

      var bounds = new RectD(rect.Value.X, rect.Value.Y, rect.Value.Width > 0 ? rect.Value.Width : 1,
          rect.Value.Height > 0 ? rect.Value.Height : 1);

      if (renderInViewCoordinates) {
        group.Transform = context.ViewTransform;
      }

      var rectangle = (Rectangle) group.Children[0]!;
      rectangle.Width = bounds.Width;
      rectangle.Height = bounds.Height;
      rectangle.SetCanvasArrangeRect(bounds.ToRect());

      var text = (DrawingVisual) group.Children[1]!;
      using (var drawingContext = text.RenderOpen()) {
        drawingContext.DrawText(
            new FormattedText(title, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12,
                pen.Brush), new Point(bounds.X + 10, titleAtTop ? bounds.Y + 10 : bounds.Y + bounds.Height - 20));
      }

      return oldVisual;
    }
  }
}
