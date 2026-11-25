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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;

namespace yWorks.Demo.RectangularIndicator
{
  internal sealed class RectangleRenderer : ObjectRendererBase<IRectangle, VisualGroup>
  {
    protected override VisualGroup CreateVisual(IRenderContext context, IRectangle renderTag) {
      var rectangle = new Rectangle {
          Fill = Brushes.Transparent,
          Stroke = Brushes.Black,
          StrokeThickness = 1
      };
      return UpdateVisual(context, new VisualGroup { Children = { rectangle } }, renderTag);
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IRectangle renderTag) {
      if (oldVisual.Children[0] is Rectangle rectangle) {
        var bounds = new RectD(context.WorldToViewCoordinates(new PointD(renderTag.X, renderTag.Y)),
            context.WorldToViewCoordinates(new PointD(renderTag.X + renderTag.Width, renderTag.Y + renderTag.Height)));
        rectangle.Width = bounds.Width;
        rectangle.Height = bounds.Height;
        rectangle.SetCanvasArrangeRect(bounds.ToRect());
        oldVisual.Transform = context.ViewTransform;
        return oldVisual;
      }
      return CreateVisual(context, renderTag);
    }
  }
}
