/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Point = System.Windows.Point;

namespace Tutorial.CustomStyles
{
  /// <summary>
  /// This class is an example for a custom edge style based on <see cref="EdgeStyleBase{TVisual}"/>.
  /// </summary>
  /// <remarks>
  /// The <see cref="System.Windows.FrameworkElement"/>s created by this instance are of type <see cref="VisualGroup"/>
  /// so this is used as the generic type parameter.
  /// </remarks>
  public class MySimpleEdgeStyle : EdgeStyleBase<VisualGroup>
  {
    // the default stroke for rendering the path
    private static readonly LinearGradientBrush pathStroke;

    /// <summary>
    /// Creates a <see cref="GeneralPath"/> from the edge's bends
    /// </summary>
    /// <param name="edge">The edge to create the path for.</param>
    /// <returns>A <see cref="GeneralPath"/> following the edge</returns>
    [NotNull]
    protected override GeneralPath GetPath(IEdge edge) {
      // Create a general path from the locations of the ports and the bends of the edge.
      GeneralPath path = new GeneralPath();
      path.MoveTo(GetLocation(edge.SourcePort));
      foreach (var bend in edge.Bends) {
        path.LineTo(bend.Location);
      }
      path.LineTo(GetLocation(edge.TargetPort));

      ////////////////////////////////////////////////////
      //////////////// New in this sample ////////////////
      ////////////////////////////////////////////////////

      // shorten the path in order to provide room for drawing the arrows.
      return base.CropPath(edge, Arrows, Arrows, path);

      ////////////////////////////////////////////////////
    }

    /// <summary>
    /// In contrast to the <see cref="GraphExtensions.GetLocation" /> property of <see cref="IPort" />, this
    /// method returns the static current location of the port.
    /// </summary>
    /// <remarks>
    /// It is recommended to use this method at performance critical places that require no live
    /// view of the port location, like the <see cref="GetPath" /> method of this class.
    /// </remarks>
    /// <param name="port">The port.</param>
    /// <returns>The current location of the given port.</returns>
    private static PointD GetLocation(IPort port) {
      var param = port.LocationParameter;
      return param.Model.GetLocation(port, param);
    }

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MySimpleEdgeStyle"/> class.
    /// </summary>
    public MySimpleEdgeStyle() {
      Arrows = yWorks.Graph.Styles.Arrows.Default;
      PathThickness = 3;
    }

    static MySimpleEdgeStyle() {
      pathStroke = new LinearGradientBrush
                     {
                       GradientStops =
                         {
                           new GradientStop {Color = Color.FromArgb(150, 150, 255, 255), Offset = 1},
                           new GradientStop {Color = Color.FromArgb(200, 0, 130, 180), Offset = 0.5},
                           new GradientStop {Color = Color.FromArgb(150, 200, 255, 255), Offset = 0}
                         },
                       StartPoint = new Point(0, 0),
                       EndPoint = new Point(1, 1),
                       SpreadMethod = GradientSpreadMethod.Repeat,
                     };
      pathStroke.Freeze();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the thickness of the edge
    /// </summary>
    [DefaultValue(3.0d)]
    public double PathThickness { get; set; }

    /// <summary>
    /// Gets or sets the arrows drawn at the beginning and at the end of the edge.
    /// </summary>
    public IArrow Arrows { get; set; }

    #endregion

    #region Rendering

    /// <summary>
    /// Creates the visual for an edge.
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, IEdge edge) {
      // This implementation creates a VisualGroup and uses it for the rendering of the edge.
      var visual = new VisualGroup();
      // Get the necessary data for rendering of the edge
      RenderDataCache cache = CreateRenderDataCache(context, edge);
      // Render the edge
      Render(context, edge, visual, cache);
      return visual;
    }

    /// <summary>
    /// Re-renders the edge using the old visual for performance reasons.
    /// </summary>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IEdge edge) {
      // get the data with which the old visual was created
      RenderDataCache oldCache = oldVisual.GetRenderDataCache<RenderDataCache>();
      // get the data for the new visual
      RenderDataCache newCache = CreateRenderDataCache(context, edge);

      // check if something changed
      if (!newCache.StateEquals(oldCache)) {
        // more than only the path changed - re-render the visual
        oldVisual.Children.Clear();
        Render(context, edge, oldVisual, newCache);
        return oldVisual;
      }

      if (!newCache.PathEquals(oldCache)) {
        // only the path changed - update the old visual
        UpdatePath(context, edge, oldVisual, newCache);
      }

      return oldVisual;
    }

    /// <summary>
    /// Updates the edge path data as well as the arrow positions of the visuals stored in <param name="container" />.
    /// </summary>
    private void UpdatePath(IRenderContext context, IEdge edge, VisualGroup container, RenderDataCache cache) {
      // The first child must be a path - else re-create the container from scratch
      if (container.Children.Count == 0 || !(container.Children[0] is Path)) {
        container.Children.Clear();
        Render(context, edge, container, cache);
        return;
      }

      // store information with the visual on how we created it
      container.SetRenderDataCache(cache);

      // update the path
      GeneralPath gp = cache.GeneralPath;
      Path path = (Path) container.Children[0];
      path.Data = CreateGeometry(gp);

      // update the arrows
      base.UpdateArrows(context, container, edge, gp, cache.Arrows, cache.Arrows);
    }

    /// <summary>
    /// Creates an object containing all necessary data to create an edge visual
    /// </summary>
    private RenderDataCache CreateRenderDataCache(IRenderContext context, IEdge edge) {
      return new RenderDataCache(PathThickness, GetPath(edge), Arrows);
    }

    /// <summary>
    /// Creates the visual appearance of an edge
    /// </summary>
    private void Render(IRenderContext context, IEdge edge, VisualGroup container, RenderDataCache cache) {
      // store information with the visual on how we created it
      container.SetRenderDataCache(cache);

      GeneralPath gp = cache.GeneralPath;
      Path path = new Path
      {
        // convince WPF to render the path even if all coordinates are negative 
        Stretch = Stretch.None,
        MinWidth = 1,
        MinHeight = 1,
        Data = CreateGeometry(gp)
      };

      // set the stroke
      path.Stroke = pathStroke;

      path.StrokeThickness = cache.PathThickness;
      path.StrokeLineJoin = PenLineJoin.Round;
      container.Add(path);

      // add the arrows to the container
      base.AddArrows(context, container, edge, gp, cache.Arrows, cache.Arrows);
    }

    /// <summary>
    /// Creates the geometry for the path from the given GeneralPath.
    /// </summary>
    private Geometry CreateGeometry(GeneralPath gp) {
      PolyLineSegment pl = new PolyLineSegment();
      PathFigure figure = new PathFigure { Segments = { pl } };
      
      // create path
      if (gp != null) {
        var cursor = gp.CreateCursor();
        if (cursor.MoveNext()) {
          figure.StartPoint = cursor.CurrentEndPoint;
        }
        // loop all bends of the edge
        while (cursor.MoveNext()) {
          pl.Points.Add(cursor.CurrentEndPoint);
        }
      }

      return new PathGeometry { Figures = { figure } };
    }

    #endregion

    #region Rendering Helper Methods

    /// <summary>
    /// Determines whether the visual representation of the edge has been hit at the given location.
    /// Overridden method to include the <see cref="PathThickness"/> and the HitTestRadius specified in the context
    /// in the calculation.
    /// </summary>
    protected override bool IsHit(IInputModeContext context, PointD location, IEdge edge) {
      // Use the convenience method in GeneralPath
      return GetPath(edge).PathContains(location, context.HitTestRadius + PathThickness * 0.5d);
    }

    #endregion
    
    /// <summary>
    /// This implementation of the look up provides a custom implementation of the 
    /// <see cref="ISelectionIndicatorInstaller"/> interface that better suits to this style.
    /// </summary>
    protected override object Lookup(IEdge edge, Type type) {
      if (type == typeof(ISelectionIndicatorInstaller)) {
        return new MySelectionIndicatorInstaller();
      } else {
        return base.Lookup(edge, type);
      }
    }

    /// <summary>
    /// This customized <see cref="ISelectionIndicatorInstaller"/> overrides the
    /// pen property to be <see langword="null"/>, so that no edge path is rendered if the edge is selected.
    /// </summary>
    private sealed class MySelectionIndicatorInstaller : EdgeSelectionIndicatorInstaller
    {
      protected override Pen GetPen(CanvasControl canvas, IEdge edge) {
        return null;
      }
    }

    /// <summary>
    /// Saves the data which is necessary for the creation of an edge
    /// </summary>
    private sealed class RenderDataCache
    {
      public RenderDataCache(double pathThickness, GeneralPath generalPath, IArrow arrows) {
        this.PathThickness = pathThickness;
        this.GeneralPath = generalPath;
        this.Arrows = arrows;
      }

      public double PathThickness { get; private set; }
      public GeneralPath GeneralPath { get; private set; }
      public IArrow Arrows { get; private set; }

      public override bool Equals(object obj) {
        var cache = obj as RenderDataCache;
        return cache != null && PathEquals(cache) && StateEquals(cache);
      }

      /// <summary>
      /// Check if the path thickness and the arrows of this instance 
      /// are equal to another <see cref="RenderDataCache"/>'s properties.
      /// </summary>
      public bool StateEquals(RenderDataCache other) {
        return other.PathThickness == PathThickness && Equals(other.Arrows, Arrows);
      }

      /// <summary>
      /// Check if the path of this instance is equals to another <see cref="RenderDataCache"/>'s path.
      /// </summary>
      public bool PathEquals(RenderDataCache other) {
        return other.GeneralPath.IsEquivalentTo(GeneralPath);
      }
    }
  }
}
