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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step10BridgeSupport
{
  /// <summary>An edge style that supports bridges.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<CustomEdgeStyle.CustomEdgeVisual>
  {
    public CustomEdgeStyle(double distance = 1) {
      Distance = distance;
    }

    public double Distance { get; set; }

    protected override CustomEdgeVisual CreateVisual(IRenderContext context, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      var pathWithBridges = CreatePathWithBridges(croppedPath, context);
      return new CustomEdgeVisual(croppedPath, pathWithBridges, Distance, GetObstacleHash(context));
    }

    protected override CustomEdgeVisual UpdateVisual(IRenderContext context, CustomEdgeVisual oldVisual, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      var obstacleHash = GetObstacleHash(context);
      if (croppedPath == null || !croppedPath.IsEquivalentTo(oldVisual.Path) ||
          obstacleHash != oldVisual.ObstacleHash) {
        var pathWithBridges = CreatePathWithBridges(croppedPath, context);
        oldVisual.UpdatePath(croppedPath, pathWithBridges, obstacleHash);
      }

      if (Distance != oldVisual.Distance) {
        oldVisual.UpdateDistance(Distance);
      }
      return oldVisual;
    }

    protected override bool IsHit(IInputModeContext context, PointD location, IEdge edge) {
      var path = GetPath(edge);
      return path != null && path.PathContains(location, context.HitTestRadius + (Distance + 2) * 0.5);
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, IEdge edge) {
      return rectangle.Intersects(GetBounds(context, edge));
    }

    protected override RectD GetBounds(ICanvasContext context, IEdge edge) {
      var path = GetPath(edge);
      return path == null ? RectD.Empty : path.GetBounds().GetEnlarged((Distance + 2) * 0.5);
    }

    protected override object? Lookup(IEdge edge, Type type) {
      if (type == typeof(IObstacleProvider)) {
        return new EdgeObstacleProvider(this, edge);
      }
      return base.Lookup(edge, type);
    }

    private sealed class EdgeObstacleProvider(CustomEdgeStyle style, IEdge edge) : IObstacleProvider
    {
      public GeneralPath GetObstacles(IRenderContext context) {
        return style.GetPath(edge)!;
      }
    }

    private static int GetObstacleHash(IRenderContext context) {
      var manager = context.Lookup<BridgeManager>();
      return manager != null ? (int) manager.GetObstacleHash(context) : 42;
    }

    private static GeneralPath? CreatePathWithBridges(GeneralPath? path, IRenderContext context) {
      if (path == null) {
        return null;
      }
      var manager = context.Lookup<BridgeManager>();
      return manager != null ? manager.AddBridges(context, path) : path;
    }

    internal sealed class CustomEdgeVisual : DrawingVisual
    {
      private GeneralPath? bridgedPath;

      public GeneralPath? Path { get; private set; }

      public double Distance { get; private set; }

      public int ObstacleHash { get; private set; }

      public CustomEdgeVisual(GeneralPath? path, GeneralPath? pathWithBridges, double distance, int obstacleHash) {
        Path = path;
        bridgedPath = pathWithBridges;
        Distance = distance;
        ObstacleHash = obstacleHash;
        Render();
      }

      public void UpdatePath(GeneralPath? path, GeneralPath? pathWithBridges, int obstacleHash) {
        Path = path;
        bridgedPath = pathWithBridges;
        ObstacleHash = obstacleHash;
        Render();
      }

      public void UpdateDistance(double distance) {
        Distance = distance;
        Render();
      }

      private void Render() {
        using var drawingContext = RenderOpen();
        if (bridgedPath == null) {
          return;
        }

        var geometry = bridgedPath.CreateGeometry(new Matrix2D(), FillMode.Never);
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
    }
  }
}
