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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step01CreateAPolyline
{
  /// <summary>A basic edge style that draws the edge path as a black polyline.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<DrawingVisual>
  {
    private static readonly Pen EdgePen = Pens.Black;

    protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
      var visual = new DrawingVisual();
      Render(visual, edge);
      return visual;
    }

    protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, IEdge edge) {
      Render(oldVisual, edge);
      return oldVisual;
    }

    private static void Render(DrawingVisual visual, IEdge edge) {
      var path = new StreamGeometry();
      using (var pathContext = path.Open()) {
        var points = edge.GetPathPoints().GetEnumerator();
        if (points.MoveNext()) {
          pathContext.BeginFigure(points.Current.ToPoint(), false, false);
          while (points.MoveNext()) {
            pathContext.LineTo(points.Current.ToPoint(), true, false);
          }
        }
      }
      path.Freeze();

      using var drawingContext = visual.RenderOpen();
      drawingContext.DrawGeometry(null, EdgePen, path);
    }
  }
}
