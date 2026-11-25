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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step02CropThePolyline
{
  /// <summary>An edge style that crops its path at the adjacent node outlines.</summary>
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

    private void Render(DrawingVisual visual, IEdge edge) {
      var generalPath = GetPath(edge);
      using var drawingContext = visual.RenderOpen();
      if (generalPath != null) {
        var croppedGeneralPath = CropPath(edge, Arrow.None, Arrow.None, generalPath);
        if (croppedGeneralPath != null) {
          var path = croppedGeneralPath.CreateGeometry(new Matrix2D(), FillMode.Never);
          path.Freeze();
          drawingContext.DrawGeometry(null, EdgePen, path);
        }
      }
    }
  }
}
