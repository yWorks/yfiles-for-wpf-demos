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
using System.Linq;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ShapePortStyle
{
  [Demo("shape-port-style", "style", "Shape Port Style", Summary = "Shows the features of the ShapePortStyle class.",
      Keywords = [
          "library style", "style options", "port shape", "ellipse", "pill", "pentagon", "rectangle", "octagon",
          "diamond", "hexagon", "triangle", "star", "squircle"
      ], Tags = ["style", "ports", "shapes"], RelatedDemos = ["shape-node-style"])]
  public sealed partial class ShapePortStyleDemo
  {
    public ShapePortStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      // Create and configure ports using shape port style
      InitializeGraph(Graph);

      InitializeInteraction(GraphControl);

      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a new shape port style with the provided shape and color.
    /// </summary>
    private static Graph.Styles.ShapePortStyle CreateShapePortStyle(
        ShapeNodeShape portShape,
        Palette colorSet,
        double aspectRatio) =>
        new() {
            Shape = portShape,
            Brush = colorSet.Fill,
            RenderSize = new SizeD(aspectRatio * 40, 40),
            Pen = null
        };

    /// <summary>
    ///   Creates nodes, ports, and edges to demonstrate all port shapes.
    /// </summary>
    /// <param name="graph">The graph in which to create nodes.</param>
    private static void InitializeGraph(IGraph graph) {
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Brush = Themes.PaletteBlue.EdgeLabelFill,
          Pen = null
      };
      graph.EdgeDefaults.Style = new PolylineEdgeStyle { Pen = new Pen(Themes.PaletteBlue.EdgeLabelFill, 3) };

      Palette[] colorSetNames = [
          Themes.PaletteBlue,
          Themes.PaletteOrange,
          Themes.PaletteRed,
          Themes.PaletteGreen,
          Themes.PalettePurple,
          Themes.PaletteLightblue
      ];

      // available port shapes
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

      CreateShapeSamples(graph, shapes, colorSetNames, 0, 1.75);
      CreateShapeSamples(graph, triangles, colorSetNames, 1, 1);
      CreateShapeSamples(graph, polygons, colorSetNames, 2, 1);
      CreateShapeSamples(graph, stars, colorSetNames, 3, 1);
    }

    /// <summary>
    ///   Creates connected nodes whose ports have the given shapes.
    /// </summary>
    /// <param name="graph">The graph to add the nodes to.</param>
    /// <param name="shapes">The shapes to use for the ports.</param>
    /// <param name="colorSets">The color sets to use for the ports.</param>
    /// <param name="row">The row index of the nodes.</param>
    /// <param name="aspectRatio">The aspect ratio of the ports.</param>
    private static void CreateShapeSamples(
        IGraph graph,
        ShapeNodeShape[] shapes,
        Palette[] colorSets,
        int row,
        double aspectRatio) {
      var x = 0.0;
      var y = row * 400.0;

      for (var i = 0; i < shapes.Length; ++i) {
        var colorSet = colorSets[(i + row) % colorSets.Length];

        var node = graph.CreateNode(new RectD(x, y, 290, 200));
        if (i > 0) {
          var sourcePort = graph.Ports.Last();
          var portStyle = CreateShapePortStyle(shapes[i], colorSet, aspectRatio);
          var targetPort = graph.AddPort(node, FreeNodePortLocationModel.Left, portStyle);
          graph.CreateEdge(sourcePort, targetPort);
        }
        if (i < shapes.Length - 1) {
          var portStyle = CreateShapePortStyle(shapes[i], colorSet, aspectRatio);
          graph.AddPort(node, FreeNodePortLocationModel.Right, portStyle);
        }

        var labelStyle = DemoStyles.CreateDemoEdgeLabelStyle(colorSet);
        labelStyle.TextSize *= 2;
        graph.AddLabel(node, shapes[i].ToString(), InteriorNodeLabelModel.Center, labelStyle);

        x += 600.0;
      }
    }

    /// <summary>
    ///   Initializes the interactive behavior for this demo.
    /// </summary>
    private static void InitializeInteraction(GraphControl graphControl) {
      graphControl.InputMode = new GraphEditorInputMode {
          AllowCreateNode = false,
          AllowAddLabel = false,
          AllowEditLabel = false
      };

      // provide port candidates for all existing ports
      graphControl.Graph.GetDecorator().Nodes.PortCandidateProvider
          .AddFactory(node => new CustomPortCandidateProvider(node));

      graphControl.Graph.GetDecorator().Ports.HandleProvider.Hide();
    }

    /// <summary>
    ///   This port candidate provider provides port candidates for the
    ///   ports of a node.
    /// </summary>
    private sealed class CustomPortCandidateProvider : PortCandidateProviderBase
    {
      private readonly INode node;

      /// <summary>
      ///   Creates a new instance of <see cref="CustomPortCandidateProvider"/>.
      /// </summary>
      /// <param name="node">The given node.</param>
      public CustomPortCandidateProvider(INode node) {
        this.node = node;
      }

      /// <summary>
      ///   Returns a list that contains a port candidate for each of the node's
      ///   ports. Each candidate has the same location as the port.
      /// </summary>
      /// <param name="context">The context for which the candidates should be provided</param>
      /// <returns>The candidates for the ports of this node.</returns>
      /// <remarks>Overrides <see cref="PortCandidateProviderBase.GetPortCandidates"/>.</remarks>
      protected override IEnumerable<IPortCandidate> GetPortCandidates(IInputModeContext context) {
        var candidates = new List<IPortCandidate>();
        if (context.GetGraph() != null) {
          foreach (var port in node.Ports) {
            var portCandidate = new PortCandidate(port);
            candidates.Add(portCandidate);
          }
        }
        return candidates;
      }
    }

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    [STAThread]
    public static void Main() {
      Run<ShapePortStyleDemo>();
    }
  }
}
