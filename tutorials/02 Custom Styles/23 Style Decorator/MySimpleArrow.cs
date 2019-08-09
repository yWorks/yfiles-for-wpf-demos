/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.ComponentModel;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Tutorial.CustomStyles
{
  public class MySimpleArrow : IArrow, IVisualCreator, IBoundsProvider
  {
    // these variables hold the state for the flyweight pattern
    // they are populated in GetPaintable and used in the implementations of the IVisualCreator interface.
    private PointD anchor;
    private PointD direction;
    private static readonly LinearGradientBrush pathFill;
    private PathFigure arrowFigure;
    private double thickness;

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MySimpleArrow"/> class.
    /// </summary>
    public MySimpleArrow() {
      //////////////// New in this sample ////////////////
      this.Thickness = 2.0d;
    }

    static MySimpleArrow() {
      pathFill = new LinearGradientBrush
      {
        StartPoint = new Point(0, 0),
        EndPoint = new Point(0, 1),
        SpreadMethod = GradientSpreadMethod.Repeat,
        GradientStops =
                       {
                         new GradientStop {Color = Color.FromArgb(255, 180, 180, 180), Offset = 0},
                         new GradientStop {Color = Color.FromArgb(255, 50, 50, 50), Offset = 0.5},
                         new GradientStop {Color = Color.FromArgb(255, 150, 150, 150), Offset = 1}
                       }
      };
      pathFill.Freeze();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns the length of the arrow, i.e. the distance from the arrow's tip to
    /// the position where the visual representation of the edge's path should begin.
    /// </summary>
    /// <value>Always returns 0</value>
    public double Length {
      get { return 0; }
    }

    /// <summary>
    /// Gets the cropping length associated with this instance.
    /// </summary>
    /// <value>Always returns 1</value>
    /// <remarks>
    /// This value is used by <see cref="IEdgeStyle"/>s to let the
    /// edge appear to end shortly before its actual target.
    /// </remarks>
    public double CropLength {
      get { return 1; }
    }

    /// <summary>
    /// Gets or sets the thickness of the arrow
    /// </summary>
    [DefaultValue(2.0d)]
    public double Thickness { get; set; }

    #endregion

    #region IArrow Members

    /// <summary>
    /// Gets an <see cref="IVisualCreator"/> implementation that will create
    /// the <see cref="FrameworkElement"/> for this arrow
    /// at the given location using the given direction for the given edge.
    /// </summary>
    /// <param name="edge">the edge this arrow belongs to</param>
    /// <param name="atSource">whether this will be the source arrow</param>
    /// <param name="anchor">the anchor point for the tip of the arrow</param>
    /// <param name="direction">the direction the arrow is pointing in</param>
    /// <returns>
    /// Itself as a flyweight.
    /// </returns>
    public IVisualCreator GetVisualCreator(IEdge edge, bool atSource, PointD anchor, PointD direction) {
      ConfigureThickness(edge);
      this.anchor = anchor;
      this.direction = direction;
      return this;
    }

    /// <summary>
    /// Gets an <see cref="IBoundsProvider"/> implementation that can yield
    /// this arrow's bounds if painted at the given location using the
    /// given direction for the given edge.
    /// </summary>
    /// <param name="edge">the edge this arrow belongs to</param>
    /// <param name="atSource">whether this will be the source arrow</param>
    /// <param name="anchor">the anchor point for the tip of the arrow</param>
    /// <param name="directionVector">the direction the arrow is pointing in</param>
    /// <returns>
    /// an implementation of the <see cref="IBoundsProvider"/> interface that can
    /// subsequently be used to query the bounds. Clients will always call
    /// this method before using the implementation and may not cache the instance returned.
    /// This allows for applying the flyweight design pattern to implementations.
    /// </returns>
    public IBoundsProvider GetBoundsProvider(IEdge edge, bool atSource, PointD anchor, PointD directionVector) {
      // Get the edge's thickness
      MySimpleEdgeStyle style = edge.Style as MySimpleEdgeStyle;

      //////////////// New in this sample ////////////////

      if (style != null) {
        thickness = style.PathThickness;
      } else {
        thickness = Thickness;
      }
      this.anchor = anchor;
      this.direction = directionVector;
      return this;
    }

    #endregion

    #region Rendering

    /// <summary>
    /// This method is called by the framework to create a <see cref="FrameworkElement"/>
    /// that will be included into the <see cref="IRenderContext"/>.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used.</param>
    /// <returns>
    /// The arrow visual to include in the canvas object visual tree./>.
    /// </returns>
    /// <seealso cref="UpdateVisual"/>
    Visual IVisualCreator.CreateVisual(IRenderContext context) {
      // Create a new Path to draw the arrow
      if (arrowFigure == null) {
        arrowFigure = new PathFigure
        {
          StartPoint = new Point(-7, -thickness / 2),
          Segments =
                            {
                              new LineSegment {Point = new Point(-7, thickness/2)},
                              new BezierSegment
                                {
                                  Point1 = new Point(-5, thickness/2),
                                  Point2 = new Point(-1.5, thickness/2),
                                  Point3 = new Point(1, thickness*1.666)
                                },
                              new BezierSegment
                                {
                                  Point1 = new Point(0, thickness*0.833),
                                  Point2 = new Point(0, -thickness*0.833),
                                  Point3 = new Point(1, -thickness*1.666)
                                },
                              new BezierSegment
                                {
                                  Point1 = new Point(-1.5, -thickness/2),
                                  Point2 = new Point(-5, -thickness/2),
                                  Point3 = new Point(-7, -thickness/2)
                                }
                            }
        };
        arrowFigure.Freeze();
      }

      Path p = new Path
      {
        // set Stretch, MinWidth and MinHeight so Path gets drawn in negative coordinate range
        Stretch = Stretch.None,
        MinWidth = 1,
        MinHeight = 1,
        Fill = pathFill,
        // Draw arrow outline
        Data = new PathGeometry
        {
          Figures = { arrowFigure }
        }
      };

      // Remember thickness for update
      p.SetRenderDataCache(thickness);

      // Rotate arrow and move it to correct position
      p.RenderTransform = new MatrixTransform
      {
        Matrix =
          new Matrix(direction.X, direction.Y, -direction.Y, direction.X, anchor.X, anchor.Y)
      };
      return p;
    }


    /// <summary>
    /// This method updates or replaces a previously created <see cref="FrameworkElement"/> for inclusion
    /// in the <see cref="IRenderContext"/>.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used in.</param>
    /// <param name="oldVisual">The visual instance that had been returned the last time the
    /// <see cref="IVisualCreator.CreateVisual"/> method was called on this instance.</param>
    /// <returns>
    /// 	<paramref name="oldVisual"/>, if this instance modified the visual, or a new visual that should replace the
    /// existing one in the canvas object visual tree.
    /// </returns>
    /// <remarks>
    /// The <see cref="CanvasControl"/> uses this method to give implementations a chance to
    /// update an existing Visual that has previously been created by the same instance during a call
    /// to <see cref="IVisualCreator.CreateVisual"/>. Implementation may update the <paramref name="oldVisual"/>
    /// and return that same reference, or create a new visual and return the new instance or <see langword="null"/>.
    /// </remarks>
    /// <seealso cref="IVisualCreator.CreateVisual"/>
    /// <seealso cref="ICanvasObjectDescriptor"/>
    /// <seealso cref="CanvasControl"/>
    public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      // get thickness of old arrow
      double oldThickness = oldVisual.GetRenderDataCache<double>();
      // if thickness has changed
      if (oldThickness != thickness) {
        // re-render arrow
        return ((IVisualCreator)this).CreateVisual(context);
      } else {
        Path p = oldVisual as Path;
        if (p != null) {
          ((MatrixTransform)p.RenderTransform).Matrix = new Matrix(direction.X, direction.Y, -direction.Y, direction.X, anchor.X, anchor.Y);
          return p;
        }
        return ((IVisualCreator)this).CreateVisual(context);
      }
    }

    #endregion

    #region Rendering Helper Methods

    /// <summary>
    /// Returns the bounds of the arrow for the current flyweight configuration.
    /// </summary>
    RectD IBoundsProvider.GetBounds(ICanvasContext context) {
      return new RectD(anchor.X - 8 - thickness, anchor.Y - 8 - thickness, 16 + thickness * 2, 16 + thickness * 2);
    }

    #endregion

    /// <summary>
    /// Configures the thickness to use for the next visual creation.
    /// </summary>
    /// <param name="edge">The edge to read the thickness from.</param>
    private void ConfigureThickness(IEdge edge) {
      // Get the edge's thickness
      MySimpleEdgeStyle style = edge.Style as MySimpleEdgeStyle;
      double oldThickness = thickness;
      if (style != null) {
        thickness = style.PathThickness;
      } else {
        thickness = Thickness;
      }
      // see if the old arrow figure needs to be invalidated...
      if (thickness != oldThickness) {
        arrowFigure = null;
      }
    }

  }
}
