/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;

namespace Demo.yFiles.ImageExport
{
  /// <summary>
  ///   Renders a rectangle in view coordinates with the given <see cref="Pen"/> and <see cref="Brush"/>.
  /// </summary>
  public sealed class ExportRectangleRenderer : ObjectRendererBase<IRectangle, VisualGroup>
  {
    private readonly Pen pen;
    private readonly Brush brush;

    public ExportRectangleRenderer(Pen pen, Brush brush = null) {
      this.pen = pen;
      this.brush = brush;
    }

    protected override VisualGroup CreateVisual(IRenderContext context, IRectangle layout) {
      var rect = new Rectangle { Fill = brush, Stroke = pen.Brush, StrokeThickness = pen.Thickness };
      var container = new VisualGroup { Children = { rect } };

      Transform(context, container, rect, layout);

      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IRectangle layout) {
      if (oldVisual.Children.Count != 1 || oldVisual.Children[0] is not Rectangle rect) {
        return CreateVisual(context, layout);
      }

      Transform(context, oldVisual, rect, layout);

      return oldVisual;
    }

    private static void Transform(IRenderContext context, VisualGroup container, UIElement rect, IRectangle layout) {
      container.Transform = context.ViewTransform;
      var viewLayout = new Rect(context.WorldToViewCoordinates(layout.GetTopLeft()).ToPoint(),
          context.WorldToViewCoordinates(layout.GetBottomRight()).ToPoint());
      CanvasControl.SetCanvasControlArrangeRect(rect, viewLayout);
    }
  }
}
