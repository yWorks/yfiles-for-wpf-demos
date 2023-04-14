/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.Selection
{
  /// <summary>
  ///   Edge style that is used as a zoom-invariant selection decorator.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This style essentially displays a path along the edge and scales its stroke thickness and brush by 1 / zoom level.
  ///     This means that positioning considerations can still be done in world coordinates and the path doesn't require a
  ///     series of transformations to end up where it should be. The brush is scaled because the default selection
  ///     decoration uses a pixel checkerboard pattern which would otherwise be scaled with the zoom level.
  ///   </para>
  ///   <para>
  ///     This style caches the scaled stroke brush to avoid creating a new brush for every invocation of
  ///     <seealso cref="UpdateVisual" />. Thus, this style cannot be shared over multiple <see cref="GraphControl" />
  ///     instances because the zoom level might differ. If the stroke brush is simply a solid color the scaling step can be
  ///     omitted.
  ///   </para>
  /// </remarks>
  /// <seealso cref="LargeGraphsWindow.SetSelectionDecorators" />
  public class FastEdgeSelectionStyle : EdgeStyleBase<Path>
  {
    /// <summary>
    ///   The cached instance of the scaled stroke brush.
    /// </summary>
    private Brush scaledBrush;

    /// <summary>
    ///   The scale at which the cached stroke brush was scaled.
    /// </summary>
    /// <remarks>Scale is 1 / zoom level.</remarks>
    private double scaledBrushScale;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FastEdgeSelectionStyle" /> class with the given stroke pen.
    /// </summary>
    /// <param name="stroke">The pen used to draw the path.</param>
    public FastEdgeSelectionStyle(Pen stroke) {
      Stroke = stroke;
    }

    /// <summary>
    ///   Gets or sets the pen used to draw the rectangle outline.
    /// </summary>
    public Pen Stroke { get; set; }

    #region Style

    /// <inheritdoc />
    protected override Path CreateVisual(IRenderContext context, IEdge edge) {
      var scale = 1 / context.Zoom;
      PointD n1, n2;
      GetPoints(edge, out n1, out n2);

      PointD n3, n4;
      GetPoints(edge, out n3, out n4);

      // ReSharper disable once UseObjectOrCollectionInitializer
      var path = new Path
      {
        Data = CreateGeometry(n3, n4, edge),
        Stroke = Brushes.Black,
        StrokeThickness = 3,
      };

      // Hack to work around WPF bug. Otherwise the path may not appear at the correct location in some cases.
      path.ClipToBounds = false;
      path.Stretch = Stretch.None;
      path.SetCanvasArrangeRect(new Rect(0, 0, double.MaxValue, double.MaxValue));
      path.MinWidth = path.MinHeight = 1;

      // The bitmap scaling mode is necessary for the scaled stroke brush not to show moiré.
      RenderOptions.SetBitmapScalingMode(path, BitmapScalingMode.NearestNeighbor);

      path.SetRenderDataCache(new EdgeInfo(n1, n2, GetBendLocations(edge)));

      UpdateStroke(path, scale);
      return path;
    }

    /// <inheritdoc />
    protected override Path UpdateVisual(IRenderContext context, Path oldVisual, IEdge edge) {
      var scale = 1 / context.Zoom;
      PointD source, target;
      GetPoints(edge, out source, out target);

      var rdc = oldVisual.GetRenderDataCache<EdgeInfo>();

      var oldSource = rdc.source;
      var oldTarget = rdc.target;

      var bendLocations = GetBendLocations(edge);

      UpdateStroke(oldVisual, scale);

      if (source == oldSource && target == oldTarget && bendLocations.SequenceEqual(rdc.bendLocations)) {
        return oldVisual;
      }

      oldVisual.Data = CreateGeometry(source, target, edge);
      oldVisual.SetRenderDataCache(new EdgeInfo(source, target, bendLocations));
      return oldVisual;
    }

    #endregion

    /// <summary>
    ///   Gets the locations of the source and target port as <see cref="PointD" />s.
    /// </summary>
    /// <param name="edge">The edge.</param>
    /// <param name="source">A variable to store the source port's location.</param>
    /// <param name="target">A variable to store the target port's location.</param>
    private static void GetPoints(IEdge edge, out PointD source, out PointD target) {
      source = GetLocation(edge.SourcePort);
      target = GetLocation(edge.TargetPort);
    }

    /// <summary>
    ///   Gets the location of a port.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns>The port's location.</returns>
    private static PointD GetLocation(IPort port) {
      var param = port.LocationParameter;
      return param.Model.GetLocation(port, param);
    }

    /// <summary>
    ///   Creates the selection path's geometry.
    /// </summary>
    /// <param name="source">The source port location.</param>
    /// <param name="target">The target port location.</param>
    /// <param name="edge">The edge.</param>
    /// <returns>The selection path's geometry.</returns>
    private static StreamGeometry CreateGeometry(PointD source, PointD target, IEdge edge) {
      var geometry = new StreamGeometry();

      using (var ctx = geometry.Open()) {
        ctx.BeginFigure(source, false, false);
        foreach (var b in edge.Bends) {
          ctx.LineTo(b.Location.ToPointD(), true, true);
        }
        ctx.LineTo(target, true, false);
      }
      geometry.Freeze();
      return geometry;
    }

    /// <summary>
    ///   Gets a list of bend locations from an edge.
    /// </summary>
    /// <param name="edge">The edge.</param>
    /// <returns>A list of the edge's bend locations, or an empty list if there are no bends.</returns>
    private static List<PointD> GetBendLocations(IEdge edge) {
      return edge.Bends.Select(b => b.Location.ToPointD()).ToList();
    }

    /// <summary>
    ///   Re-creates the scaled stroke brush if necessary and sets it on the rectangle.
    /// </summary>
    /// <param name="shape">The shape whose stroke brush will be updated.</param>
    /// <param name="scale">The scale. This is 1 / zoom level.</param>
    private void UpdateStroke(Shape shape, double scale) {
      shape.StrokeThickness = Stroke.Thickness * scale;
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (scale != scaledBrushScale || scaledBrush == null) {
        // If the cached brush no longer matches the scale, re-create it.
        scaledBrush = Stroke.Brush.Clone();
        scaledBrush.Transform = new ScaleTransform(scale, scale);
        scaledBrush.Freeze();
        scaledBrushScale = scale;
      }
      shape.Stroke = scaledBrush;
    }

    /// <summary>
    ///   Helper structure to keep information about the edge.
    /// </summary>
    private sealed class EdgeInfo
    {
      /// <summary>
      ///   A list of bend locations in the edge.
      /// </summary>
      public readonly List<PointD> bendLocations;
      /// <summary>
      ///   The source port location.
      /// </summary>
      public readonly PointD source;
      /// <summary>
      ///   The target port location.
      /// </summary>
      public readonly PointD target;

      /// <summary>
      ///   Initializes a new instance of the <see cref="EdgeInfo" /> structure, using the given source and target port
      ///   locations, and the given list of bend locations.
      /// </summary>
      /// <param name="source">The source port location.</param>
      /// <param name="target">The target port location.</param>
      /// <param name="bendLocations">A list of bend locations.</param>
      public EdgeInfo(PointD source, PointD target, List<PointD> bendLocations) {
        this.source = source;
        this.target = target;
        this.bendLocations = bendLocations;
      }
    }
  }
}