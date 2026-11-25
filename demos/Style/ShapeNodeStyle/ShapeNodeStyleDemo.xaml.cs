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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ShapeNodeStyle
{
  [Demo("shape-node-style", "style", "Shape Node Style", Summary = "Shows the features of the ShapeNodeStyle class.",
      Tags = ["style", "nodes", "shapes"], Keywords = [
          "library style", "style options", "node shape", "ellipse", "pill", "pentagon",
          "rectangle", "octagon", "diamond", "hexagon", "triangle", "star", "squircle"
      ], RelatedDemos = [
          "rectangle-node-style", "group-node-style", "arrow-node-style",
          "tutorial-style-implementation-node-create-a-rectangle"
      ])]
  public sealed partial class ShapeNodeStyleDemo
  {
    public ShapeNodeStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      InitializeStyleDefaults(Graph);

      // Create and configure nodes using shape node style
      CreateSampleNodes(Graph);

      ConfigureInteraction(GraphControl);
      GraphControl.FitGraphBounds();
      InitializeUI();
    }

    /// <summary>
    ///   Creates several nodes for each <see cref="ShapeNodeShape"/> value.
    /// </summary>
    /// <param name="graph">The graph to add the nodes to.</param>
    private void CreateSampleNodes(IGraph graph) {
      // Create the various shape samples
      ShapeNodeShape[] shapes = [
          ShapeNodeShape.Rectangle,
          ShapeNodeShape.RoundRectangle,
          ShapeNodeShape.Pill,
          ShapeNodeShape.Squircle,
          ShapeNodeShape.Ellipse
      ];
      ShapeNodeShape[] triangles = [
          ShapeNodeShape.Triangle,
          ShapeNodeShape.TrianglePointingRight,
          ShapeNodeShape.TrianglePointingDown,
          ShapeNodeShape.TrianglePointingLeft
      ];
      ShapeNodeShape[] polygons = [
          ShapeNodeShape.Diamond,
          ShapeNodeShape.Pentagon,
          ShapeNodeShape.Hexagon,
          ShapeNodeShape.HexagonStanding,
          ShapeNodeShape.Octagon,
          ShapeNodeShape.OctagonStanding
      ];
      ShapeNodeShape[] stars = [ShapeNodeShape.Star5, ShapeNodeShape.Star6, ShapeNodeShape.Star8];
      CreateShapeSamples(shapes, 0, graph);
      CreateShapeSamples(triangles, 1, graph);
      CreateShapeSamples(polygons, 2, graph);
      CreateShapeSamples(stars, 3, graph);
    }

    /// <summary>
    ///   Creates sample nodes for the given shapes.
    /// </summary>
    /// <param name="shapes">The shapes to use.</param>
    /// <param name="column">The column index of the nodes.</param>
    /// <param name="graph">The graph to add the nodes to.</param>
    private void CreateShapeSamples(ShapeNodeShape[] shapes, int column, IGraph graph) {
      const double size1 = 45;
      const double size2 = 90;

      // Define colors for distinguishing the three different aspect ratios used below
      var palette1 = Themes.Palette54;
      var palette2 = Themes.Palette56;
      var palette3 = Themes.Palette510;

      for (var i = 0; i < shapes.Length; i++) {
        // Create a green node with aspect ratio 1:1
        var n1 = graph.CreateNode(new RectD(column * 350 - size1 / 2, i * 200 - size1 / 2, size1, size1),
            DemoStyles.CreateDemoShapeNodeStyle(shapes[i], palette1));

        // Create a blue node with aspect ratio 2:1
        var n2 = graph.CreateNode(new RectD(column * 350 + 100 - size2 / 2, i * 200 - size1 / 2, size2, size1),
            DemoStyles.CreateDemoShapeNodeStyle(shapes[i], palette2));
        graph.AddLabel(n2, shapes[i].ToString());

        // Create a yellow node with aspect ratio 1:2
        var n3 = graph.CreateNode(new RectD(column * 350 + 200 - size1 / 2, i * 200 - size2 / 2, size1, size2),
            DemoStyles.CreateDemoShapeNodeStyle(shapes[i], palette3));

        graph.CreateEdge(n1, n2);
        graph.CreateEdge(n2, n3);
      }
    }

    /// <summary>
    ///   Initializes the style defaults for labels and edges.
    /// </summary>
    private void InitializeStyleDefaults(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);

      // All node labels share the same style and label model parameter
      var labelStyle = DemoStyles.CreateDemoNodeLabelStyle(Themes.Palette58);
      labelStyle.Typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal,
          FontStretches.Normal);
      labelStyle.TextSize = 24;
      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(
          new PointD(0.5, 0), new PointD(0, -50), new PointD(0.5, 0.5));

      // Edges share the same style as well, they are not important in this demo
      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(Themes.Palette56);
    }

    /// <summary>
    ///   Configures the interactive behavior for this demo.
    /// </summary>
    private void ConfigureInteraction(GraphControl graphControl) {
      graphControl.InputMode = new GraphEditorInputMode {
          AllowGroupingOperations = false,
          MarqueeSelectableItems = GraphItemTypes.Node,
          AllowCreateBend = false,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowAddLabel = false,
          AllowEditLabel = false,
          DeletableItems = GraphItemTypes.None,
          MovableSelectedItems = GraphItemTypes.Node
      };
    }

    /// <summary>
    ///   Binds actions to the demo's UI controls.
    /// </summary>
    private void InitializeUI() {
      IntrinsicAspectRatioToggle.Checked += IntrinsicAspectRatioToggle_Changed;
      IntrinsicAspectRatioToggle.Unchecked += IntrinsicAspectRatioToggle_Changed;
    }

    private void IntrinsicAspectRatioToggle_Changed(object sender, RoutedEventArgs e) {
      // Change the keep-intrinsic-aspect-ratio behavior of the nodes depending on the state of the
      // corresponding toggle button
      var keepAspectRatio = IntrinsicAspectRatioToggle.IsChecked == true;
      foreach (var node in Graph.Nodes) {
        var shapeNodeStyle = (Graph.Styles.ShapeNodeStyle) node.Style;
        shapeNodeStyle.KeepIntrinsicAspectRatio = keepAspectRatio;
      }

      // Force the graph component to repaint itself
      // This ensures the visual effects of changing ShapeNodeStyle's keep-intrinsic-aspect-ratio
      // behavior is immediately visible
      GraphControl.Invalidate();
    }

    [STAThread]
    public static void Main() {
      Run<ShapeNodeStyleDemo>();
    }
  }
}
