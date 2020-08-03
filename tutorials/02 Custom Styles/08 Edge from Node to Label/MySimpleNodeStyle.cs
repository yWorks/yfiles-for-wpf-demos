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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;
using Point = System.Windows.Point;

namespace Tutorial.CustomStyles
{
  /// <summary>
  /// A very simple implementation of an <see cref="INodeStyle"/>
  /// that uses the convenience class <see cref="NodeStyleBase{TVisual}"/>
  /// as the base class.
  /// </summary>
  public class MySimpleNodeStyle : NodeStyleBase<VisualGroup>
  {
    // bitmap to pre-render the drop shadow
    private static readonly RenderTargetBitmap dropShadow;

    ////////////////////////////////////////////////////
    //////////////// New in this sample ////////////////
    ////////////////////////////////////////////////////

    /// <summary>
    /// Draws the edge-like connectors from a node to its labels
    /// </summary>
    private void RenderLabelEdges(IRenderContext context, INode node, VisualGroup container, RenderDataCache cache) {
      if (node.Labels.Count > 0) {
        // Create a SimpleEdge which will be used as a dummy for the rendering
        SimpleEdge simpleEdge = new SimpleEdge(null, null);
        // Assign the style
        simpleEdge.Style = new PolylineEdgeStyle();

        // Create a SimpleNode which provides the source port for the edge but won't be drawn itself
        SimpleNode sourceDummyNode = new SimpleNode { Layout = new RectD(0, 0, node.Layout.Width, node.Layout.Height), Style = node.Style };


        // Set source port to the port of the node using a dummy node that is located at the origin.
        simpleEdge.SourcePort = new SimplePort(sourceDummyNode, FreeNodePortLocationModel.NodeCenterAnchored);

        // Create a SimpleNode which provides the target port for the edge but won't be drawn itself
        SimpleNode targetDummyNode = new SimpleNode();

        // Create port on targetDummynode for the label target
        simpleEdge.TargetPort = new SimplePort(targetDummyNode, FreeNodePortLocationModel.NodeCenterAnchored);

        // Render one edge for each label
        foreach (PointD labelLocation in cache.LabelLocations) {
          // move the dummy node to the location of the label
          targetDummyNode.Layout = new MutableRectangle(labelLocation, SizeD.Zero);

          // now create the visual using the style interface:
          IEdgeStyleRenderer renderer = simpleEdge.Style.Renderer;
          IVisualCreator creator = renderer.GetVisualCreator(simpleEdge, simpleEdge.Style);
          Visual element = creator.CreateVisual(context);
          if (element != null) {
            container.Add(element);
          }
        }
      }
    }

    ////////////////////////////////////////////////////

    #region Node Color

    /// <summary>
    /// Gets or sets the fill color of the node.
    /// </summary>
    [DefaultValue(typeof(Color), "#C80082B4")]
    public Color NodeColor { get; set; }

    /// <summary>
    /// Determines the color to use for filling the node.
    /// </summary>
    /// <remarks>
    /// This implementation uses the <see cref="NodeColor"/> property unless
    /// the <see cref="ITagOwner.Tag"/> of the <see cref="INode"/> is of type <see cref="Color"/>, 
    /// in which case that color overrides this style's setting.
    /// </remarks>
    /// <param name="node">The node to determine the color for.</param>
    /// <returns>The color for filling the node.</returns>
    protected virtual Color GetNodeColor(INode node) {
      // the color can be obtained from the "business data" that can be associated with
      // each node, or use the value from this instance.
      return node.Tag is Color ? (Color)node.Tag : NodeColor;
    }

    #endregion

    #region Constructor

    public MySimpleNodeStyle() {
      NodeColor = Color.FromArgb(0xc8, 0x00, 0x82, 0xb4);
    }

    static MySimpleNodeStyle() {
      // Render the drop shadow

      dropShadow = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Default);
      Ellipse ellipse = new Ellipse
      {
        Opacity = 0.2,
        Fill = new SolidColorBrush(Colors.Black),
        Width = 32,
        Height = 32,
        Effect = new BlurEffect { Radius = 4 },
      };
      ellipse.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
      ellipse.Arrange(new Rect(16, 16, 32, 32));
      ellipse.LayoutTransform = new TranslateTransform { X = 16, Y = 16 };
      dropShadow.Render(ellipse);
    }

    #endregion

    #region Rendering

    /// <summary>
    /// Creates the visual for a node.
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      // This implementation creates a VisualGroup and uses it for the rendering of the node.
      var visual = new VisualGroup();
      // Get the necessary data for rendering of the edge
      RenderDataCache cache = CreateRenderDataCache(context, node);
      // Render the node
      Render(context, node, visual, cache);
      // set the location
      visual.SetCanvasArrangeRect(node.Layout.ToRectD());
      return visual;
    }

    /// <summary>
    /// Re-renders the node using the old visual for performance reasons.
    /// </summary>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      // get the data with which the oldvisual was created
      RenderDataCache oldCache = oldVisual.GetRenderDataCache<RenderDataCache>();
      // get the data for the new visual
      RenderDataCache newCache = CreateRenderDataCache(context, node);

      // check if something changed except for the location of the node
      if (!newCache.Equals(oldCache)) {
        // something changed - re-render the visual
        oldVisual.Children.Clear();
        Render(context, node, oldVisual, newCache);
      }
      // make sure that the location is up to date
      oldVisual.SetCanvasArrangeRect(node.Layout.ToRectD());
      return oldVisual;
    }

    /// <summary>
    /// Creates an object containing all necessary data to create a visual for the node
    /// </summary>
    private RenderDataCache CreateRenderDataCache(IRenderContext context, INode node) {
      // If Tag is set to a Color, use it as background color of the node
      Color c = (node.Tag is Color) ? (Color)node.Tag : Color.FromArgb(200, 0, 130, 180);

      List<PointD> labelLocations = new List<PointD>();
      // Remember center points of labels to draw label edges, relative the node's top left corner
      foreach (ILabel label in node.Labels) {
        PointD labelCenter = label.GetLayout().GetCenter();
        labelLocations.Add(labelCenter - node.Layout.GetTopLeft());
      }
      return new RenderDataCache(c, node.Layout.ToSizeD(), labelLocations);
    }

    /// <summary>
    /// Actually creates the visual appearance of a node given the values provided by <see cref="RenderDataCache"/>.
    /// </summary>
    /// <remarks>
    /// This renders the node and the edges to the labels and adds the visuals to the <paramref name="container"/>.
    /// All items are arranged as if the node was located at (0,0). <see cref="CreateVisual"/> and <see cref="UpdateVisual"/>
    /// finally arrange the container so that the drawing is translated into the final position.
    /// </remarks>
    private void Render(IRenderContext context, INode node, VisualGroup container, RenderDataCache cache) {
      // store information with the visual on how we created it
      container.SetRenderDataCache(cache);

      // draw the dropshadow
      DrawShadow(container, cache.Size);
      // draw edges to node labels
      RenderLabelEdges(context, node, container, cache);

      // determine the color to use for the rendering
      Color color = GetNodeColor(node);

      // the size of node
      SizeD nodeSize = cache.Size;

      Ellipse shape = new Ellipse
      {
        Width = nodeSize.Width,
        Height = nodeSize.Height
      };

      // max and min needed for reflection effect calculation
      double max = Math.Max(nodeSize.Width, nodeSize.Height);
      double min = Math.Min(nodeSize.Width, nodeSize.Height);
      // Create Background gradient from specified background color
      shape.Fill = new LinearGradientBrush()
      {
        GradientStops =
                        {
                          new GradientStop
                            {
                              Color =
                                Color.FromArgb((byte) Math.Max(0, color.A - 50),
                                               (byte) Math.Min(255, color.R*1.7),
                                               (byte) Math.Min(255, color.G*1.7),
                                               (byte) Math.Min(255, color.B*1.7)),
                              Offset = 1
                            },
                          new GradientStop {Color = color, Offset = 0.5},
                          new GradientStop
                            {
                              Color =
                                Color.FromArgb((byte) Math.Max(0, color.A - 50),
                                               (byte) Math.Min(255, color.R*1.4),
                                               (byte) Math.Min(255, color.G*1.4),
                                               (byte) Math.Min(255, color.B*1.4)),
                              Offset = 0
                            }
                        },
        StartPoint = new Point(0, 0),
        EndPoint = new Point(0.5 / (nodeSize.Width / max), 1 / (nodeSize.Height / max)),
        SpreadMethod = GradientSpreadMethod.Pad
      };

      // Create light reflection effects
      Ellipse reflection1 = new Ellipse
      {
        Width = min / 10,
        Height = min / 10,
        Fill = Brushes.White
      };
      Ellipse reflection2 = new Ellipse
      {
        Width = min / 7,
        Height = min / 7,
        Fill = Brushes.AliceBlue
      };

      PathGeometry reflection3 = new PathGeometry();
      PathFigure figure = new PathFigure();
      Point startPoint = new Point(nodeSize.Width / 2.5, nodeSize.Height / 10 * 9);
      Point endPoint = new Point(nodeSize.Width / 10 * 9, nodeSize.Height / 2.5);
      Point ctrlPoint1 = new Point(startPoint.X + (endPoint.X - startPoint.X) / 2, nodeSize.Height);
      Point ctrlPoint2 = new Point(nodeSize.Width, startPoint.Y + (endPoint.Y - startPoint.Y) / 2);
      Point ctrlPoint3 = new Point(ctrlPoint1.X, ctrlPoint1.Y - nodeSize.Height / 10);
      Point ctrlPoint4 = new Point(ctrlPoint2.X - nodeSize.Width / 10, ctrlPoint2.Y);

      figure.StartPoint = startPoint;
      reflection3.Figures.Add(figure);
      figure.Segments.Add(new BezierSegment { Point1 = ctrlPoint1, Point2 = ctrlPoint2, Point3 = endPoint });
      figure.Segments.Add(new BezierSegment { Point1 = ctrlPoint4, Point2 = ctrlPoint3, Point3 = startPoint });
      figure.IsFilled = true;
      Path p = new Path();
      p.Data = reflection3;
      p.Fill = Brushes.AliceBlue;

      figure.Freeze();

      // place the reflections
      reflection1.SetCanvasArrangeRect(new Rect(nodeSize.Width / 5, nodeSize.Height / 5,
                                               min / 10, min / 10));
      reflection2.SetCanvasArrangeRect(new Rect(nodeSize.Width / 4.9, nodeSize.Height / 4.9,
                                                min / 7, min / 7));
      // and add all to the container for the node
      container.Children.Add(shape);
      container.Children.Add(reflection2);
      container.Children.Add(reflection1);
      container.Children.Add(p);
    }

    /// <summary>
    /// Draws the pre-rendered dropshadow image at the given size.
    /// </summary>
    private void DrawShadow(VisualGroup visual, SizeD size) {
      const int tileSize = 32;
      const int tileSize2 = 16;
      const int offsetY = 2;
      const int offsetX = 2;

      double xScaleFactor = size.Width / tileSize;
      double yScaleFactor = size.Height / tileSize;

      Image imageCompleteImage = new Image { Source = dropShadow, Stretch = Stretch.Fill, IsHitTestVisible = false };
      imageCompleteImage.SetCanvasArrangeRect(new Rect(offsetX - tileSize2 * xScaleFactor,
                                                       offsetY - tileSize2 * yScaleFactor,
                                                       tileSize * 2 * xScaleFactor, tileSize * 2 * yScaleFactor));
      visual.Add(imageCompleteImage);
    }

    #endregion

    #region Rendering Helper Methods

    /// <summary>
    /// Gets the outline of the node, an ellipse in this case
    /// </summary>
    /// <remarks>
    /// This allows for correct edge path intersection calculation, among others.
    /// </remarks>
    protected override GeneralPath GetOutline(INode node) {
      var rect = node.Layout.ToRectD();
      var outline = new GeneralPath();
      outline.AppendEllipse(rect, false);
      return outline;
    }

    /// <summary>
    /// Get the bounding box of the node
    /// </summary>
    /// <remarks>
    /// This is used for bounding box calculations and includes the visual shadow.
    /// </remarks>
    protected override RectD GetBounds(ICanvasContext context, INode node) {
      RectD bounds = node.Layout.ToRectD();
      // expand bounds to include dropshadow
      return bounds + new InsetsD(0, 0, 3, 3);
    }

    /// <summary>
    /// Hit test which considers HitTestRadius specified in CanvasContext
    /// </summary>
    /// <returns>True if p is inside node.</returns>
    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      return GeomUtilities.EllipseContains(node.Layout.ToRectD(), location, context.HitTestRadius);
    }

    /// <summary>  
    /// Checks if a node is inside a certain box. Considers HitTestRadius.
    /// </summary>
    /// <returns>True if the box intersects the elliptical shape of the node. Also true if box lies completely inside node.</returns>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, INode node) {

      // early exit if not even the bounds are contained in the box
      if (!base.IsInBox(context, rectangle, node)) {
        return false;
      }

      double eps = context.HitTestRadius;

      var outline = GetOutline(node);
      if (outline == null) return (rectangle.Contains(node.Layout.ToRectD().TopLeft) && rectangle.Contains(node.Layout.ToRectD().BottomRight));

      if (outline.Intersects(rectangle, eps)) {
        return true;
      }
      if (outline.PathContains(rectangle.TopLeft, eps) && outline.PathContains(rectangle.BottomRight, eps)) {
        return true;
      }
      return (rectangle.Contains(node.Layout.ToRectD().TopLeft) && rectangle.Contains(node.Layout.ToRectD().BottomRight));
    }

    /// <summary>
    /// Exact geometric check whether a point p lies inside the node. This is important for intersection calculation, among others.
    /// </summary>
    protected override bool IsInside(INode node, PointD location) {
      return GeomUtilities.EllipseContains(node.Layout.ToRectD(), location, 0);
    }

    #endregion

    /// <summary>
    /// Saves the data which is necessary for the creation of a node
    /// </summary>
    private sealed class RenderDataCache
    {
      public Color Color { get; private set; }
      public SizeD Size { get; private set; }
      // Center points of the node's labels relative to the node's top left corner
      public List<PointD> LabelLocations { get; private set; }

      public RenderDataCache(Color color, SizeD size, List<PointD> labelLocations) {
        Color = color;
        Size = size;
        LabelLocations = labelLocations;
      }

      public bool Equals(RenderDataCache other) {
        return other.Color == Color && other.Size.Equals(Size) &&
               ListsAreEqual(LabelLocations, other.LabelLocations);
      }

      /// <summary>
      /// Helper method to decide if two lists are equals
      /// </summary>
      private static bool ListsAreEqual<T>(List<T> list1, List<T> list2) {
        if (list1.Count != list2.Count) {
          return false;
        }
        for (int i = 0; i < list1.Count; i++) {
          if (!Equals(list1[i], list2[i])) {
            return false;
          }
        }
        return true;
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(RenderDataCache)) {
          return false;
        }
        return Equals((RenderDataCache)obj);
      }
    }
  }
}
