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

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Demo.yFiles.Graph.LargeGraphs.Styles.LevelOfDetail;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.Fast
{
  /// <summary>
  ///   A faster edge style.
  /// </summary>
  /// <remarks>
  ///   <para>This style offers three main optimizations compared to the default <see cref="PolylineEdgeStyle" />:</para>
  ///   <list type="number">
  ///     <item>The edge is not clipped at node boundaries.</item>
  ///     <item>Bends are not drawn below a configurable zoom level.</item>
  ///     <item>Edges are hidden completely if they are shorter than a given number of pixels on screen.</item>
  ///   </list>
  ///   <para>
  ///     When an edge has labels bends should not be hidden as long as the edge labels are visible. This is because edge
  ///     labels are attached to the conceptual edge path, which includes bends. If bends are not drawn, edge labels may look
  ///     out of place or even far away from the actually displayed edge path. Using <see cref="LevelOfDetailLabelStyle" /> a
  ///     suitable zoom level at which to display labels can easily be configured.
  ///   </para>
  /// </remarks>
  public class FastEdgeStyle : EdgeStyleBase<Visual>
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="FastEdgeStyle" /> class with default settings.
    /// </summary>
    /// <remarks>By default bends are not drawn below a zoom level of 50 % and edges shorter than 10 pixels are hidden.</remarks>
    public FastEdgeStyle() {
      DrawBendsThreshold = 0.5;
      MinimumEdgeLength = 10;
    }

    /// <summary>
    ///   Gets or sets the minimum zoom level at which bends are drawn.
    /// </summary>
    /// <remarks>Below this zoom level the edge is only drawn as a single line between its source and target ports.</remarks>
    public double DrawBendsThreshold { get; set; }

    /// <summary>
    ///   Gets or sets the minimum length (in pixels on screen) where edges will still be drawn.
    /// </summary>
    /// <remarks>All edges where the distance between source and target port is shorter than this will not be displayed.</remarks>
    public double MinimumEdgeLength { get; set; }

    #region Style

    /// <inheritdoc />
    protected override Visual CreateVisual(IRenderContext context, IEdge edge) {
      PointD source, target;
      GetPoints(edge, out source, out target);
      var zoom = context.Zoom;
      if (!ShouldDrawEdge(source, target, zoom)) {
        return null;
      }

      var drawBends = ShouldDrawBends(zoom);

      // ReSharper disable once UseObjectOrCollectionInitializer
      var path = new Path
      {
        Data = CreateGeometry(source, target, edge, drawBends),
        Stroke = Brushes.Black,
        StrokeThickness = 3
      };

      // Hack to work around WPF bug. Otherwise the path may not appear at the correct location in some cases.
      path.ClipToBounds = false;
      path.Stretch = Stretch.None;
      path.SetCanvasArrangeRect(new Rect(0, 0, double.MaxValue, double.MaxValue));
      path.MinWidth = path.MinHeight = 1;

      path.SetRenderDataCache(new EdgeInfo(source, target, drawBends, GetBendLocations(edge)));
      return path;
    }

    /// <inheritdoc />
    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, IEdge edge) {
      PointD source, target;
      GetPoints(edge, out source, out target);
      var zoom = context.Zoom;

      if (!ShouldDrawEdge(source, target, zoom)) {
        return null;
      }

      var rdc = oldVisual.GetRenderDataCache<EdgeInfo>();

      var oldSource = rdc.source;
      var oldTarget = rdc.target;

      var drawBends = ShouldDrawBends(zoom);
      var bendLocations = GetBendLocations(edge);

      // Did anything change at all? If not, we can just re-use the old visual
      if (source == oldSource &&
          target == oldTarget &&
          drawBends == rdc.drawBends &&
          ArrayEqual(bendLocations, rdc.bendLocations)) {
        return oldVisual;
      }

      // Otherwise re-create the geometry and update the cache
      ((Path) oldVisual).Data = CreateGeometry(source, target, edge, drawBends);
      oldVisual.SetRenderDataCache(new EdgeInfo(source, target, drawBends, bendLocations));
      return oldVisual;
    }

    /// <inheritdoc />
    protected override RectD GetBounds(ICanvasContext context, IEdge edge) {
      var zoom = context.Zoom;
      if (zoom >= DrawBendsThreshold) {
        return base.GetBounds(context, edge);
      }
      PointD source, target;
      GetPoints(edge, out source, out target);
      if (ShouldDrawEdge(source, target, zoom)) {
        return new RectD(source, target);
      }
      return RectD.Empty;
    }

    #endregion

    #region Helper methods

    /// <summary>
    ///   Determines whether the edge should be drawn at all, taking into account the value of the
    ///   <see cref="MinimumEdgeLength" /> property.
    /// </summary>
    /// <param name="source">The source port location.</param>
    /// <param name="target">The target port location.</param>
    /// <param name="zoom">The current zoom level.</param>
    /// <returns>
    ///   <see langword="true" />, if the edge should be drawn, <see langword="false" /> if not.
    /// </returns>
    private bool ShouldDrawEdge(PointD source, PointD target, double zoom) {
      var dx = (source.X - target.X) * zoom;
      var dy = (source.Y - target.Y) * zoom;

      // Minor optimization: Avoid square root
      var distSquared = dx * dx + dy * dy;
      return distSquared >= MinimumEdgeLength * MinimumEdgeLength;
    }


    /// <summary>
    ///   Determines whether bends should be drawn, according to the value of the <see cref="DrawBendsThreshold" /> property.
    /// </summary>
    /// <param name="zoom">The current zoom level.</param>
    /// <returns>
    ///   <see langword="true" />, if bends should be drawn, <see langword="false" /> if not.
    /// </returns>
    private bool ShouldDrawBends(double zoom) {
      return zoom >= DrawBendsThreshold;
    }

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
    ///   Creates the edge path's geometry.
    /// </summary>
    /// <param name="source">The source port location.</param>
    /// <param name="target">The target port location.</param>
    /// <param name="edge">The edge.</param>
    /// <param name="drawBends">Flag to set whether bends are included in the path or not.</param>
    /// <returns>The edge path's geometry.</returns>
    private static StreamGeometry CreateGeometry(PointD source, PointD target, IEdge edge, bool drawBends) {
      var geometry = new StreamGeometry();
      using (var ctx = geometry.Open()) {
        ctx.BeginFigure(source, false, false);
        if (drawBends) {
          foreach (var b in edge.Bends) {
            ctx.LineTo(b.Location.ToPointD(), true, true);
          }
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
    private static PointD[] GetBendLocations(IEdge edge) {
      int count = edge.Bends.Count;
      var points = new PointD[count];
      for (int i = 0; i < count; i++) {
        points[i] = edge.Bends[i].Location.ToPointD();
      }
      return points;
    }

    /// <summary>
    ///   Compares two arrays for equality.
    /// </summary>
    /// <typeparam name="T">The type of the arrays.</typeparam>
    /// <param name="a">The first array.</param>
    /// <param name="b">The second array.</param>
    /// <returns>
    ///   <see langword="true" /> if both arrays have the same length and all elements of one array compare equal to the
    ///   respective element in the other array, <see langword="false" /> otherwise.
    /// </returns>
    private static bool ArrayEqual<T>(T[] a, T[] b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }

      if (a.Length != b.Length) {
        return false;
      }

      // ReSharper disable once LoopCanBeConvertedToQuery
      for (int i = 0; i < a.Length; i++) {
        if (!a[i].Equals(b[i])) {
          return false;
        }
      }
      return true;
    }

    #endregion

    /// <summary>
    ///   Helper structure to keep information about the edge.
    /// </summary>
    private sealed class EdgeInfo
    {
      /// <summary>
      ///   A list of bend locations in the edge.
      /// </summary>
      public readonly PointD[] bendLocations;

      /// <summary>
      ///   A flag determining whether bends should be drawn or not.
      /// </summary>
      public readonly bool drawBends;

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
      ///   locations, whether to draw bends or not and the given list of bend locations.
      /// </summary>
      /// <param name="source">The source port location.</param>
      /// <param name="target">The target port location.</param>
      /// <param name="drawBends">A flag determining whether bends should be drawn or not.</param>
      /// <param name="bendLocations">A list of bend locations.</param>
      public EdgeInfo(PointD source, PointD target, bool drawBends, PointD[] bendLocations) {
        this.source = source;
        this.target = target;
        this.drawBends = drawBends;
        this.bendLocations = bendLocations;
      }
    }
  }
}