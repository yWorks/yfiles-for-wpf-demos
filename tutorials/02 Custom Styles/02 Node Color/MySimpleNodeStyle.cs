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
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Point = System.Windows.Point;

namespace Tutorial.CustomStyles
{
  /// <summary>
  /// A very simple implementation of an <see cref="INodeStyle"/>
  /// that uses the convenience class <see cref="NodeStyleBase{TVisual}"/>
  /// as the base class. This step shows how to add properties to the style.
  /// Also it shows how to use data bound to the node's tag.
  /// </summary>
  public class MySimpleNodeStyle : NodeStyleBase<VisualGroup>
  {
    ////////////////////////////////////////////////////
    //////////////// New in this sample ////////////////
    ////////////////////////////////////////////////////

    public MySimpleNodeStyle() {
      NodeColor = Color.FromArgb(0xc8, 0x00, 0x82, 0xb4);
    }

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

    ////////////////////////////////////////////////////

    /// <summary>
    /// Creates the visual for a node.
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      // This implementation creates a VisualGroup and uses it for the rendering of the node.
      var visual = new VisualGroup();
      // Render the node
      Render(context, node, visual);
      // set the location
      visual.SetCanvasArrangeRect(node.Layout.ToRectD());
      return visual;
    }

    /// <summary>
    /// Actually creates the visual appearance of a node.
    /// </summary>
    /// <remarks>
    /// This renders the node and the edges to the labels and adds the visuals to the <paramref name="container"/>.
    /// All items are arranged as if the node was located at (0,0). <see cref="CreateVisual"/> and
    /// <see cref="IVisualCreator.UpdateVisual"/> finally arrange the container so that the drawing is translated into
    /// the final position.
    /// </remarks>
    private void Render(IRenderContext context, INode node, VisualGroup container) {

      ////////////////////////////////////////////////////
      //////////////// New in this sample ////////////////
      ////////////////////////////////////////////////////

      // determine the color to use for the rendering
      Color color = GetNodeColor(node);

      ////////////////////////////////////////////////////

      // the size of node
      SizeD nodeSize = node.Layout.ToSizeD();

      Ellipse shape = new Ellipse
      {
        Width = nodeSize.Width,
        Height = nodeSize.Height,
        Effect = new DropShadowEffect() { BlurRadius = 3, Color = Colors.Black, Opacity = 0.2, ShadowDepth = 3 }
      };

      // max and min needed for reflection effect calculation
      double max = Math.Max(nodeSize.Width, nodeSize.Height);
      double min = Math.Min(nodeSize.Width, nodeSize.Height);

      ////////////////////////////////////////////////////
      //////////////// New in this sample ////////////////
      ////////////////////////////////////////////////////

      // Create Background gradient from specified background color
      // NEW: create the gradient dynamically, based on the node color
      shape.Fill = new LinearGradientBrush
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

      ////////////////////////////////////////////////////

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
  }
}
