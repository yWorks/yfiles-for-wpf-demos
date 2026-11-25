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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step11AddingArrows
{
  /// <summary>An edge style that renders parallel lines, supports bridges, and displays arrows.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<Visual>
  {
    public CustomEdgeStyle(double distance = 1, IArrow? sourceArrow = null, IArrow? targetArrow = null) {
      Distance = distance;
      SourceArrow = sourceArrow ?? Arrow.None;
      TargetArrow = targetArrow ?? Arrow.None;
    }

    public double Distance { get; set; }

    public IArrow SourceArrow { get; set; }

    public IArrow TargetArrow { get; set; }

    protected override Visual CreateVisual(IRenderContext context, IEdge edge) {
      var visualGroup = new VisualGroup();
      var path = GetPath(edge);
      if (path == null) {
        return visualGroup;
      }

      var croppedPath = CropPath(edge, SourceArrow, TargetArrow, path);
      if (croppedPath == null) {
        return visualGroup;
      }

      var pathWithBridges = CreatePathWithBridges(croppedPath, context);
      var edgeVisual = new CustomEdgeVisual(croppedPath, pathWithBridges, Distance, GetLoadColor(edge),
          GetObstacleHash(context), SourceArrow, TargetArrow);
      visualGroup.Add(edgeVisual);
      AddArrows(context, visualGroup, edge, pathWithBridges, SourceArrow, TargetArrow);
      return visualGroup;
    }

    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, IEdge edge) {
      if (oldVisual is not VisualGroup oldVisualGroup || oldVisualGroup.Children.Count == 0 ||
          oldVisualGroup.Children[0] is not CustomEdgeVisual oldEdgeVisual) {
        return CreateVisual(context, edge);
      }

      var path = GetPath(edge);
      if (path == null) {
        return CreateVisual(context, edge);
      }

      var croppedPath = CropPath(edge, SourceArrow, TargetArrow, path);
      if (croppedPath == null) {
        return CreateVisual(context, edge);
      }

      var obstacleHash = GetObstacleHash(context);
      if (!PathsEqual(croppedPath, oldEdgeVisual.Path) || obstacleHash != oldEdgeVisual.ObstacleHash ||
          !ReferenceEquals(SourceArrow, oldEdgeVisual.SourceArrow) ||
          !ReferenceEquals(TargetArrow, oldEdgeVisual.TargetArrow)) {
        var pathWithBridges = CreatePathWithBridges(croppedPath, context);
        oldEdgeVisual.UpdatePath(croppedPath, pathWithBridges, obstacleHash, SourceArrow, TargetArrow);
        UpdateArrows(context, oldVisualGroup, edge, pathWithBridges, SourceArrow, TargetArrow);
      }

      if (Distance != oldEdgeVisual.Distance) {
        oldEdgeVisual.UpdateDistance(Distance);
      }

      var loadColor = GetLoadColor(edge);
      if (loadColor != oldEdgeVisual.LoadColor) {
        oldEdgeVisual.UpdateLoadColor(loadColor);
      }

      return oldVisualGroup;
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

    private static bool PathsEqual(GeneralPath? path1, GeneralPath? path2) {
      return ReferenceEquals(path1, path2) || path1 != null && path2 != null && path1.IsEquivalentTo(path2);
    }

    private static int GetObstacleHash(IRenderContext context) {
      var manager = context.Lookup<BridgeManager>();
      return manager != null ? (int) manager.GetObstacleHash(context) : 42;
    }

    private static GeneralPath CreatePathWithBridges(GeneralPath path, IRenderContext context) {
      var manager = context.Lookup<BridgeManager>();
      return manager != null ? manager.AddBridges(context, path) : path;
    }

    private static Color GetLoadColor(IEdge edge) {
      if (edge.Tag is EdgeData edgeData) {
        return edgeData.Load switch {
            "Free" => Color.FromRgb(118, 176, 65),
            "Moderate" => Color.FromRgb(255, 201, 20),
            "Overloaded" => Color.FromRgb(255, 108, 0),
            _ => Colors.White
        };
      }
      return Colors.White;
    }

    internal sealed class CustomEdgeVisual : DrawingVisual
    {
      private System.Windows.Media.Geometry? pathGeometry;

      public GeneralPath? Path { get; private set; }

      public double Distance { get; private set; }

      public Color LoadColor { get; private set; }

      public int ObstacleHash { get; private set; }

      public IArrow SourceArrow { get; private set; }

      public IArrow TargetArrow { get; private set; }

      public CustomEdgeVisual(
          GeneralPath path,
          GeneralPath pathWithBridges,
          double distance,
          Color loadColor,
          int obstacleHash,
          IArrow sourceArrow,
          IArrow targetArrow) {
        Path = path;
        pathGeometry = CreateGeometry(pathWithBridges);
        Distance = distance;
        LoadColor = loadColor;
        ObstacleHash = obstacleHash;
        SourceArrow = sourceArrow;
        TargetArrow = targetArrow;
        Render();
      }

      public void UpdatePath(
          GeneralPath path,
          GeneralPath pathWithBridges,
          int obstacleHash,
          IArrow sourceArrow,
          IArrow targetArrow) {
        Path = path;
        pathGeometry = CreateGeometry(pathWithBridges);
        ObstacleHash = obstacleHash;
        SourceArrow = sourceArrow;
        TargetArrow = targetArrow;
        Render();
      }

      public void UpdateDistance(double distance) {
        Distance = distance;
        Render();
      }

      public void UpdateLoadColor(Color loadColor) {
        LoadColor = loadColor;
        Render();
      }

      private static System.Windows.Media.Geometry CreateGeometry(GeneralPath path) {
        var geometry = path.CreateGeometry(new Matrix2D(), FillMode.Never);
        geometry.Freeze();
        return geometry;
      }

      private void Render() {
        using var drawingContext = RenderOpen();
        drawingContext.DrawGeometry(null, CreatePen(Brushes.Black, Distance + 2), pathGeometry);
        drawingContext.DrawGeometry(null, CreatePen(new SolidColorBrush(LoadColor), Distance), pathGeometry);
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
