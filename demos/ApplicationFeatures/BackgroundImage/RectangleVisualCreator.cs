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

namespace yWorks.Demo.BackgroundImage
{
  public sealed class RectangleVisualCreator : IVisualCreator
  {
    private static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x99, 0xcc));
    private readonly CanvasControl canvasControl;

    public RectangleVisualCreator(CanvasControl canvasControl) {
      this.canvasControl = canvasControl;
    }

    /// <summary>
    ///   Creates the visual for the background.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used.</param>
    /// <returns>The visual for the background.</returns>
    public Visual CreateVisual(IRenderContext context) =>
        UpdateVisual(context, new Rectangle {
            Fill = BackgroundBrush,
            IsHitTestVisible = false
        });

    /// <summary>
    ///   Updates the visual for the background.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used.</param>
    /// <param name="oldVisual">The old visual.</param>
    /// <returns>The visual for the background.</returns>
    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is not Rectangle rectangle) {
        return CreateVisual(context);
      }
      var contentBounds = canvasControl.ContentBounds;
      var bounds = (contentBounds.IsEmpty() ? new RectD(0, 0, 0, 0) : contentBounds).GetEnlarged(20);
      rectangle.Width = bounds.Width;
      rectangle.Height = bounds.Height;
      rectangle.SetCanvasArrangeRect(bounds.ToRect());
      return rectangle;
    }
  }
}
