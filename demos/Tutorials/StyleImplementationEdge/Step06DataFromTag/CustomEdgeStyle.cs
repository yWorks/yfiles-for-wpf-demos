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

namespace yWorks.Demo.Tutorial.StyleImplementationEdge.Step06DataFromTag
{
  /// <summary>An edge style that uses the load stored in the edge tag as its color.</summary>
  internal sealed class CustomEdgeStyle : EdgeStyleBase<CustomEdgeStyle.CustomEdgeVisual>
  {
    public CustomEdgeStyle(double distance) {
      Distance = distance;
    }

    public CustomEdgeStyle() : this(1) { }

    /// <summary>Gets or sets the distance between the paths.</summary>
    public double Distance { get; set; }

    protected override CustomEdgeVisual CreateVisual(IRenderContext context, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      return new CustomEdgeVisual(croppedPath, Distance, GetLoadColor(edge));
    }

    protected override CustomEdgeVisual UpdateVisual(IRenderContext context, CustomEdgeVisual oldVisual, IEdge edge) {
      var path = GetPath(edge);
      var croppedPath = path == null ? null : CropPath(edge, Arrow.None, Arrow.None, path);
      var pathChanged = croppedPath == null
          ? oldVisual.CroppedPath != null
          : !croppedPath.IsEquivalentTo(oldVisual.CroppedPath);
      var distanceChanged = Distance != oldVisual.Distance;
      var loadColor = GetLoadColor(edge);
      var loadColorChanged = loadColor != oldVisual.LoadColor;

      if (!pathChanged && !distanceChanged && !loadColorChanged) {
        return oldVisual;
      }

      oldVisual.Update(croppedPath, Distance, loadColor);
      return oldVisual;
    }

    /// <summary>Returns the color associated with the load stored in the edge tag.</summary>
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

    /// <summary>The visual that stores the edge path, distance, and tag-derived color.</summary>
    internal sealed class CustomEdgeVisual : DrawingVisual
    {
      public GeneralPath? CroppedPath { get; private set; }

      public double Distance { get; private set; }

      public Color LoadColor { get; private set; }

      public CustomEdgeVisual(GeneralPath? croppedPath, double distance, Color loadColor) {
        Update(croppedPath, distance, loadColor);
      }

      public void Update(GeneralPath? croppedPath, double distance, Color loadColor) {
        CroppedPath = croppedPath;
        Distance = distance;
        LoadColor = loadColor;

        using var drawingContext = RenderOpen();
        if (croppedPath != null) {
          var geometry = croppedPath.CreateGeometry(new Matrix2D(), FillMode.Never);
          geometry.Freeze();
          drawingContext.DrawGeometry(null, CreatePen(Brushes.Black, distance + 2), geometry);
          drawingContext.DrawGeometry(null, CreatePen(new SolidColorBrush(loadColor), distance), geometry);
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
