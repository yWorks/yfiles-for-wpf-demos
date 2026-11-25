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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step09ItemBounds
{
  /// <summary>An edge style that includes the circle in the edge's render bounds.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<CustomEdgeStyle.CustomEdgeVisual>
  {
    private const double CircleRadius = 20;

    public CustomEdgeStyle(double distance = 1) {
      Distance = distance;
    }

    public double Distance { get; set; }

    protected override CustomEdgeVisual CreateVisual(IRenderContext context, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      return new CustomEdgeVisual(croppedPath, Distance);
    }

    protected override CustomEdgeVisual UpdateVisual(IRenderContext context, CustomEdgeVisual oldVisual, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      var pathChanged = croppedPath == null
          ? oldVisual.CroppedPath != null
          : !croppedPath.IsEquivalentTo(oldVisual.CroppedPath);
      if (pathChanged || Distance != oldVisual.Distance) {
        oldVisual.Update(croppedPath, Distance);
      }
      return oldVisual;
    }

    protected override RectD GetBounds(ICanvasContext context, IEdge edge) {
      var path = GetPath(edge);
      if (path == null) {
        return RectD.Empty;
      }

      var thickness = Distance + 2;
      var edgeBounds = path.GetBounds().GetEnlarged(thickness * 0.5);
      var circleBounds = RectD.FromCenter(path.GetPoint(0.5), new SizeD(CircleRadius * 2, CircleRadius * 2));
      return edgeBounds + circleBounds;
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, IEdge edge) {
      return rectangle.Intersects(GetBounds(context, edge));
    }

    internal sealed class CustomEdgeVisual : DrawingVisual
    {
      private static readonly Brush CircleBrush = CreateBrush(Color.FromArgb(76, 11, 113, 137));

      public GeneralPath? CroppedPath { get; private set; }

      public double Distance { get; private set; }

      public CustomEdgeVisual(GeneralPath? path, double distance) {
        Update(path, distance);
      }

      public void Update(GeneralPath? path, double distance) {
        CroppedPath = path;
        Distance = distance;
        Render();
      }

      private void Render() {
        using var drawingContext = RenderOpen();
        if (CroppedPath == null) {
          return;
        }

        var center = CroppedPath.GetPoint(0.5);
        drawingContext.DrawEllipse(CircleBrush, null, new Point(center.X, center.Y), CircleRadius, CircleRadius);

        var geometry = CroppedPath.CreateGeometry(new Matrix2D(), FillMode.Never);
        geometry.Freeze();
        drawingContext.DrawGeometry(null, CreatePen(Brushes.Black, Distance + 2), geometry);
        drawingContext.DrawGeometry(null, CreatePen(Brushes.White, Distance), geometry);
      }

      private static Pen CreatePen(Brush brush, double width) {
        var pen = new Pen(brush, width) {
            StartLineCap = PenLineCap.Flat, EndLineCap = PenLineCap.Flat, LineJoin = PenLineJoin.Miter
        };
        pen.Freeze();
        return pen;
      }

      private static Brush CreateBrush(Color color) {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
      }
    }
  }
}
