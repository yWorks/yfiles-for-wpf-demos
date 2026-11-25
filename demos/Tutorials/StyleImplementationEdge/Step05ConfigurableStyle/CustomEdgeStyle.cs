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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step05ConfigurableStyle
{
  /// <summary>An edge style that renders two parallel lines and can be configured.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<CustomEdgeStyle.CustomEdgeVisual>
  {
    /// <summary>Creates an edge style with the given distance between the paths.</summary>
    public CustomEdgeStyle(double distance) {
      Distance = distance;
    }

    /// <summary>Creates an edge style with the default distance of 1.</summary>
    public CustomEdgeStyle() : this(1) { }

    /// <summary>Gets or sets the distance between the paths.</summary>
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
      var distanceChanged = Distance != oldVisual.Distance;

      if (!pathChanged && !distanceChanged) {
        return oldVisual;
      }

      oldVisual.Update(croppedPath, Distance);
      return oldVisual;
    }

    /// <summary>The visual that stores the edge path and distance used for drawing.</summary>
    internal sealed class CustomEdgeVisual : DrawingVisual
    {
      public GeneralPath? CroppedPath { get; private set; }

      public double Distance { get; private set; }

      public CustomEdgeVisual(GeneralPath? croppedPath, double distance) {
        Update(croppedPath, distance);
      }

      public void Update(GeneralPath? croppedPath, double distance) {
        CroppedPath = croppedPath;
        Distance = distance;

        using var drawingContext = RenderOpen();
        if (croppedPath != null) {
          var geometry = croppedPath.CreateGeometry(new Matrix2D(), FillMode.Never);
          geometry.Freeze();
          drawingContext.DrawGeometry(null, CreatePen(Brushes.Black, distance + 2), geometry);
          drawingContext.DrawGeometry(null, CreatePen(Brushes.White, distance), geometry);
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
