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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.GeneralPathNodeStyle
{
  /// <summary>
  ///   Shows how to create a variety of star, polygon, and other custom node styles with
  ///   <see cref="GeneralPathNodeStyle"/>.
  /// </summary>
  [Demo("general-path-node-style", "style", "General Path Node Style",
      Summary =
          "Shows how to create a variety of star, polygon, and other custom node styles with GeneralPathNodeStyle.",
      Tags = ["style", "node", "star", "polygon"],
      Keywords = ["styling", "custom", "icon", "factory", "airplane", "computer", "person", "path"])]
  public sealed partial class GeneralPathNodeStyleDemo
  {
    private sealed record CustomNodeStyle(Graph.Styles.GeneralPathNodeStyle Style, string Label);

    public GeneralPathNodeStyleDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // initialize the graph
      var graphControl = this.graphControl;
      graphControl.InputMode = new GraphViewerInputMode();
      var graph = graphControl.Graph;

      // create nodes with different custom shapes
      CreateCustomNodes(graph);

      // center the graph
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates several nodes that demonstrate various custom shapes.
    /// </summary>
    /// <param name="graph">The graph in which to create nodes.</param>
    private static void CreateCustomNodes(IGraph graph) {
      // create several style instances that display various custom shapes
      var customNodeStyles = new List<CustomNodeStyle>();

      // star with 3 points and 0.1 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(3, 0.1, 0, true),
              Brush = Themes.PaletteLightblue.Fill,
              Pen = new Pen(Themes.PaletteLightblue.Stroke, 1)
          }, "Shape: Star\nPoint Count: 3\nOuter-Inner Ratio: 0.1\nStretch to Height: True"));

      // star with 3 points and 1.3 outer-inner ratio, stretched to full bounding box width
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(3, 1.3, 0, false, true),
              Brush = Themes.PaletteGreen.Fill,
              Pen = new Pen(Themes.PaletteGreen.Stroke, 1)
          }, "Shape: Star\nPoint Count: 3\nOuter-Inner Ratio: 1.3\nStretch to Width: True"));

      // star with 4 points and 10 outer-inner ratio, rotated
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(4, 10, 0.1 * Math.PI),
              Brush = Themes.PaletteOrange.Fill,
              Pen = new Pen(Themes.PaletteOrange.Stroke, 1)
          }, "Shape: Star\nPoint Count: 4\nOuter-Inner Ratio: 10\nRotation Angle: 0.1 * π"));

      // star with 5 points and 0.5 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(5, 0.5),
              Brush = Themes.PaletteRed.Fill,
              Pen = new Pen(Themes.PaletteRed.Stroke, 1)
          }, "Shape: Star\nPoint Count: 5\nOuter-Inner Ratio: 0.5"));

      // star with 5 points and 3 outer-inner ratio, stretched to full bounding width
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(5, 3, 0.3 * Math.PI, false, true),
              Brush = Themes.PalettePurple.Fill,
              Pen = new Pen(Themes.PalettePurple.Stroke, 1)
          }, "Shape: Star\nPoint Count: 5\nOuter-Inner Ratio: 3\nRotation 0.3 * π\nStretch to Width: True"));

      // star with 8 points and 1.4 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(8, 1.4),
              Brush = Themes.PaletteBlue.Fill,
              Pen = new Pen(Themes.PaletteBlue.Stroke, 1)
          }, "Shape: Star\nPoint Count: 8\nOuter-Inner Ratio: 1.4"));

      // star with 6 points and 0.1 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(6, 0.1),
              Brush = Themes.PaletteLightblue.Fill,
              Pen = new Pen(Themes.PaletteLightblue.Stroke, 1)
          }, "Shape: Star\nPoint Count: 6\nOuter-Inner Ratio: 0.1"));

      // star with 10 points and 0.5 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(10, 0.5),
              Brush = Themes.PaletteGreen.Fill,
              Pen = new Pen(Themes.PaletteGreen.Stroke, 1)
          }, "Shape: Star\nPoint Count: 10\nOuter-Inner Ratio: 0.5"));

      // star with 10 points and 5 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(10, 5),
              Brush = Themes.PaletteOrange.Fill,
              Pen = new Pen(Themes.PaletteOrange.Stroke, 1)
          }, "Shape: Star\nPoint Count: 10\nOuter-Inner Ratio: 5"));

      // star with 30 points and 0.8 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(30, 0.8),
              Brush = Themes.PaletteRed.Fill,
              Pen = new Pen(Themes.PaletteRed.Stroke, 1)
          }, "Shape: Star\nPoint Count: 30\nOuter-Inner Ratio: 0.8"));

      // star with 30 points and 5 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(30, 5),
              Brush = Themes.PalettePurple.Fill,
              Pen = new Pen(Themes.PalettePurple.Stroke, 1)
          }, "Shape: Star\nPoint Count: 30\nOuter-Inner Ratio: 5"));

      // star with 30 points and 1.2 outer-inner ratio
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreateStarPath(30, 1.2),
              Brush = Themes.PaletteBlue.Fill,
              Pen = new Pen(Themes.PaletteBlue.Stroke, 1)
          }, "Shape: Star\nPoint Count: 30\nOuter-Inner Ratio: 1.2"));

      // polygon with 3 points, stretched to height
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(3, 0, true),
              Brush = Themes.PaletteLightblue.Fill,
              Pen = new Pen(Themes.PaletteLightblue.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 3\nStretch to Height: True"));

      // polygon with 4 points
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(4),
              Brush = Themes.PaletteGreen.Fill,
              Pen = new Pen(Themes.PaletteGreen.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 4"));

      // polygon with 5 points, rotated 0.15 * Pi degrees, stretched to full bounding box height
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(5, 0.15 * Math.PI, true),
              Brush = Themes.PaletteOrange.Fill,
              Pen = new Pen(Themes.PaletteOrange.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 5\nRotation Angle: 0.15 * π\nStretch to Height: True"));

      // polygon with 5 points
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(5),
              Brush = Themes.PaletteRed.Fill,
              Pen = new Pen(Themes.PaletteRed.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 5\nRotation Angle: 0"));

      // polygon with 6 points, stretched to full bounding box width
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(6, 0, false, true),
              Brush = Themes.PalettePurple.Fill,
              Pen = new Pen(Themes.PalettePurple.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 6\nStretch to Width: True"));

      // polygon with 8 points
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(8),
              Brush = Themes.PaletteBlue.Fill,
              Pen = new Pen(Themes.PaletteBlue.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 8"));

      // polygon with 9 points
      customNodeStyles.Add(new CustomNodeStyle(
          new Graph.Styles.GeneralPathNodeStyle {
              Path = Graph.Styles.GeneralPathNodeStyle.CreatePolygonPath(9),
              Brush = Themes.PaletteLightblue.Fill,
              Pen = new Pen(Themes.PaletteLightblue.Stroke, 1)
          }, "Shape: Polygon\nPoint Count: 9"));

      // custom factory icon, facing right
      customNodeStyles.Add(new CustomNodeStyle(CreateFactoryNodeStyle(true, Themes.PaletteGreen),
          "Shape: Factory Icon\nDirection: Right"));

      // custom factory icon, facing left
      customNodeStyles.Add(new CustomNodeStyle(CreateFactoryNodeStyle(false, Themes.PaletteOrange),
          "Shape: Factory Icon\nDirection: Left"));

      // custom airplane icon
      customNodeStyles.Add(new CustomNodeStyle(CreateAirplaneNodeStyle(Themes.PaletteRed), "Shape: Airplane Icon"));

      // custom computer icon
      customNodeStyles.Add(new CustomNodeStyle(CreateComputerNodeStyle(Themes.PalettePurple), "Shape: Computer Icon"));

      // custom person icon
      customNodeStyles.Add(new CustomNodeStyle(CreatePersonNodeStyle(Themes.PaletteBlue), "Shape: Person Icon"));

      // create a node for each style instance
      var x = 0.0;
      var y = 0.0;
      for (var i = 1; i <= customNodeStyles.Count; ++i) {
        const double width = 100;
        const double height = 100;
        var customNodeStyle = customNodeStyles[i - 1];
        // create the node and label
        var node = graph.CreateNode(new RectD(x, y, width, height), customNodeStyle.Style);
        graph.AddLabel(node, customNodeStyle.Label,
            new ExteriorNodeLabelModel { Margins = new InsetsD(30) }.CreateParameter(ExteriorNodeLabelModel.Position
                .Bottom), CreateLabelStyle());
        // display 6 nodes in every row
        if (i % 6 == 0) {
          x = 0;
          y += width + 150;
        } else {
          x += height + 150;
        }
      }
    }

    /// <summary>
    ///   Returns a GeneralPathNodeStyle instance that displays a factory icon.
    /// </summary>
    /// <param name="facingRight">Whether the shape should face right.</param>
    /// <param name="palette">The palette to use for nodes.</param>
    private static Graph.Styles.GeneralPathNodeStyle CreateFactoryNodeStyle(bool facingRight, Palette palette) {
      var generalPath = new GeneralPath();

      if (facingRight) {
        generalPath.MoveTo(0.064, 0);
        generalPath.LineTo(0.17, 0);
        generalPath.LineTo(0.191, 0.625);
        generalPath.LineTo(0.234, 0.625);
        generalPath.LineTo(0.489, 0.475);
        generalPath.LineTo(0.489, 0.625);
        generalPath.LineTo(0.745, 0.475);
        generalPath.LineTo(0.745, 0.625);
        generalPath.LineTo(1, 0.475);
        generalPath.LineTo(1, 1);
        generalPath.LineTo(0, 1);
        generalPath.LineTo(0, 0.625);
        generalPath.LineTo(0.042, 0.625);
        generalPath.Close();
      } else {
        generalPath.MoveTo(0, 0.413);
        generalPath.LineTo(0.261, 0.543);
        generalPath.LineTo(0.261, 0.413);
        generalPath.LineTo(0.522, 0.543);
        generalPath.LineTo(0.522, 0.413);
        generalPath.LineTo(0.783, 0.543);
        generalPath.LineTo(0.826, 0.543);
        generalPath.LineTo(0.848, 0);
        generalPath.LineTo(0.956, 0);
        generalPath.LineTo(0.97, 0.543);
        generalPath.LineTo(1, 0.543);
        generalPath.LineTo(1, 1);
        generalPath.LineTo(0, 1);
        generalPath.Close();
      }

      return new Graph.Styles.GeneralPathNodeStyle(generalPath) {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1)
      };
    }

    /// <summary>
    ///   Returns a GeneralPathNodeStyle instance that displays a person icon.
    /// </summary>
    /// <param name="palette">The palette to use for nodes.</param>
    private static Graph.Styles.GeneralPathNodeStyle CreatePersonNodeStyle(Palette palette) {
      var generalPath = new GeneralPath();

      generalPath.AppendEllipse(new RectD(0.167, 0, 0.667, 0.667), true);
      generalPath.QuadTo(new PointD(0.167, 0.733), new PointD(0, 1));
      generalPath.LineTo(1, 1);
      generalPath.QuadTo(new PointD(0.833, 0.733), new PointD(0.5, 0.667));

      return new Graph.Styles.GeneralPathNodeStyle(generalPath) {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1)
      };
    }

    /// <summary>
    ///   Returns a GeneralPathNodeStyle instance that displays a computer icon.
    /// </summary>
    /// <param name="palette">The palette to use for nodes.</param>
    private static Graph.Styles.GeneralPathNodeStyle CreateComputerNodeStyle(Palette palette) {
      var generalPath = new GeneralPath();

      generalPath.AppendRectangle(new RectD(0, 0.4, 1, 0.5), false);
      generalPath.MoveTo(0.4, 0.9);
      generalPath.QuadTo(new PointD(0.42, 0.967), new PointD(0.38, 1));
      generalPath.LineTo(0.62, 1);
      generalPath.QuadTo(new PointD(0.58, 0.967), new PointD(0.6, 0.9));
      generalPath.Close();
      return new Graph.Styles.GeneralPathNodeStyle(generalPath) {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1)
      };
    }

    /// <summary>
    ///   Returns a GeneralPathNodeStyle instance that displays an airplane icon.
    /// </summary>
    /// <param name="palette">The palette to use for nodes.</param>
    private static Graph.Styles.GeneralPathNodeStyle CreateAirplaneNodeStyle(Palette palette) {
      var generalPath = new GeneralPath();

      generalPath.MoveTo(0.162, 0.662);
      generalPath.LineTo(0.434, 0.529);
      generalPath.LineTo(0.434, 0.397);
      generalPath.QuadTo(new PointD(0.5, 0), new PointD(0.567, 0.397));
      generalPath.LineTo(0.567, 0.529);
      generalPath.LineTo(0.838, 0.662);
      generalPath.LineTo(0.838, 0.706);
      generalPath.LineTo(0.603, 0.662);
      generalPath.LineTo(0.559, 0.662);
      generalPath.LineTo(0.559, 0.868);
      generalPath.LineTo(0.662, 0.926);
      generalPath.LineTo(0.662, 0.956);
      generalPath.LineTo(0.529, 0.926);
      generalPath.LineTo(0.5, 1);
      generalPath.LineTo(0.471, 0.926);
      generalPath.LineTo(0.338, 0.956);
      generalPath.LineTo(0.338, 0.926);
      generalPath.LineTo(0.456, 0.868);
      generalPath.LineTo(0.441, 0.662);
      generalPath.LineTo(0.397, 0.662);
      generalPath.LineTo(0.162, 0.706);
      generalPath.Close();

      return new Graph.Styles.GeneralPathNodeStyle(generalPath) {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1)
      };
    }

    /// <summary>
    ///   Creates the label style.
    /// </summary>
    private static LabelStyle CreateLabelStyle() {
      var style = DemoStyles.CreateDemoNodeLabelStyle(Themes.PaletteLightblue);
      style.TextAlignment = TextAlignment.Left;
      style.Padding = new InsetsD(4, 8, 4, 8);
      style.TextSize = 10;
      return style;
    }

    [STAThread]
    public static void Main() {
      Run<GeneralPathNodeStyleDemo>();
    }
  }
}
