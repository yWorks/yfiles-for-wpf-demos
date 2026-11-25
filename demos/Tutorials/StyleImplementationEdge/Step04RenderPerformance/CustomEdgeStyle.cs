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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step04RenderPerformance
{
  /// <summary>An edge style that reuses its visual when the edge path has not changed.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<CustomEdgeStyle.CustomEdgeVisual>
  {
    protected override CustomEdgeVisual CreateVisual(IRenderContext context, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      return new CustomEdgeVisual(croppedPath);
    }

    protected override CustomEdgeVisual UpdateVisual(IRenderContext context, CustomEdgeVisual oldVisual, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);

      // If the path has not changed, we can return the old visual
      if (croppedPath != null && croppedPath.IsEquivalentTo(oldVisual.Path)) {
        return oldVisual;
      }

      // Otherwise, we update the visual's path
      oldVisual.UpdatePath(croppedPath);
      return oldVisual;
    }

    /// <summary>The visual that stores the last path used to draw the edge.</summary>
    internal sealed class CustomEdgeVisual : DrawingVisual
    {
      private static readonly Pen OuterPen = CreatePen(Brushes.Black, 4);
      private static readonly Pen InnerPen = CreatePen(Brushes.White, 2);

      public GeneralPath? Path { get; private set; }

      public CustomEdgeVisual(GeneralPath? croppedPath) {
        UpdatePath(croppedPath);
      }

      public void UpdatePath(GeneralPath? croppedPath) {
        Path = croppedPath;
        using var drawingContext = RenderOpen();
        if (croppedPath != null) {
          var geometry = croppedPath.CreateGeometry(new Matrix2D(), FillMode.Never);
          geometry.Freeze();
          drawingContext.DrawGeometry(null, OuterPen, geometry);
          drawingContext.DrawGeometry(null, InnerPen, geometry);
        }
      }

      private static Pen CreatePen(Brush brush, double width) {
        var pen = new Pen(brush, width) {
            StartLineCap = PenLineCap.Flat, EndLineCap = PenLineCap.Flat, LineJoin = PenLineJoin.Miter
        };
        pen.Freeze();
        return pen;
      }
    }
  }
}
