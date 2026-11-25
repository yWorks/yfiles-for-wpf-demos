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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step12CustomArrowVisualization
{
  /// <summary>An arrow visualization that fits the parallel-line edge style.</summary>
  internal sealed class CustomArrow : IArrow, IVisualCreator, IBoundsProvider
  {
    private readonly double distance;
    private PointD anchor = PointD.Origin;
    private PointD direction = PointD.Origin;
    private GeneralPath? arrowPath;

    public CustomArrow(double distance) {
      this.distance = distance;
    }

    public double Length => distance * 2;

    public double CropLength => 1;

    public bool CropAtPort => false;

    public IVisualCreator GetVisualCreator(IEdge edge, bool atSource, PointD anchor, PointD direction) {
      this.anchor = anchor;
      this.direction = direction;
      return this;
    }

    public Visual CreateVisual(IRenderContext context) {
      arrowPath ??= CreateArrowPath(distance);
      var geometry = arrowPath.CreateGeometry(new Matrix2D());
      geometry.Freeze();
      return new ArrowVisual(geometry, anchor, direction);
    }

    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
      if (oldVisual is ArrowVisual arrowVisual) {
        arrowVisual.Update(anchor, direction);
        return arrowVisual;
      }
      return CreateVisual(context);
    }

    public IBoundsProvider GetBoundsProvider(IEdge edge, bool atSource, PointD anchor, PointD direction) {
      this.anchor = anchor;
      this.direction = direction;
      return this;
    }

    public RectD GetBounds(ICanvasContext context) {
      var path = CreateArrowPath(distance);
      path.Transform(CreateTransform(anchor, direction));
      return path.GetBounds();
    }

    private static GeneralPath CreateArrowPath(double distance) {
      var path = new GeneralPath();
      path.MoveTo(distance * 2 + 1, distance * 0.5);
      path.LineTo(distance * 2 + 1, distance + 1);
      path.LineTo(0, 0);
      path.LineTo(distance * 2 + 1, -distance - 1);
      path.LineTo(distance * 2 + 1, -distance * 0.5);
      return path;
    }

    private static Matrix2D CreateTransform(PointD anchor, PointD direction) {
      return new Matrix2D(-direction.X, -direction.Y, direction.Y, -direction.X, anchor.X, anchor.Y);
    }

    private sealed class ArrowVisual : DrawingVisual
    {
      private readonly System.Windows.Media.Geometry geometry;

      public ArrowVisual(System.Windows.Media.Geometry geometry, PointD anchor, PointD direction) {
        this.geometry = geometry;
        Update(anchor, direction);
        using var drawingContext = RenderOpen();
        drawingContext.DrawGeometry(Brushes.White, Pens.Black, geometry);
      }

      public void Update(PointD anchor, PointD direction) {
        var transform = new MatrixTransform(new Matrix(
            -direction.X, -direction.Y, direction.Y, -direction.X, anchor.X, anchor.Y));
        transform.Freeze();
        Transform = transform;
      }
    }
  }
}
