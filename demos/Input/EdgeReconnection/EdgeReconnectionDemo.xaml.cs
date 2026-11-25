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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.EdgeReconnection
{
  /// <summary>
  ///   This demo shows how the reconnection of edge ports can be customized and restricted.
  /// </summary>
  [Demo("edge-reconnection", "input", "Edge Reconnection",
      Summary = "Shows how the reconnection of edge ports can be customized and restricted.", Tags = ["interaction"],
      Keywords = ["port candidate providers", "ports"])]
  public sealed partial class EdgeReconnectionDemo
  {
    public EdgeReconnectionDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Registers a callback function as decorator that provides a custom
    ///   <see cref="IEdgeReconnectionPortCandidateProvider"/> for each edge.
    ///   This callback function is called whenever an edge in the graph is queried
    ///   for its <see cref="IEdgeReconnectionPortCandidateProvider"/>. In this case, the 'edge'
    ///   parameter will be set to that edge.
    /// </summary>
    /// <param name="graph">The given graph</param>
    private static void RegisterEdgePortCandidateProvider(IGraph graph) {
      var edgeDecorator = graph.GetDecorator().Edges;
      edgeDecorator.ReconnectionPortCandidateProvider.AddFactory(edge => {
        // obtain the tag from the edge
        var edgeTag = edge.Tag;

        // Check if it is a known tag and choose the respective implementation
        if (edgeTag is not string tag) {
          return null;
        }
        return tag switch {
            "red" => new RedEdgePortCandidateProvider(edge),
            "orange" => new OrangeEdgePortCandidateProvider(edge),
            "blue" => new BlueEdgePortCandidateProvider(),
            "green" => new GreenEdgePortCandidateProvider(),
            // otherwise revert to default behavior
            _ => null
        };
      });
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;

      // Disable automatic cleanup of unconnected ports since some nodes have a predefined set of ports
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      // Create a default editor input mode
      var graphEditorInputMode = new GraphEditorInputMode {
          // Just for user convenience: disable node/edge creation and clipboard operations
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowClipboardOperations = false,
          SelectableItems = GraphItemTypes.All & ~GraphItemTypes.Port
      };
      // and enable the undo feature.
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph component.
      graphControl.InputMode = graphEditorInputMode;

      // Set a port style that makes the pre-defined ports visible
      graph.NodeDefaults.Ports.Style = new ShapePortStyle { Shape = ShapeNodeShape.Ellipse };

      RegisterEdgePortCandidateProvider(graph);

      CreateSampleGraph(graphControl);

      //center graph
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates the sample graph of this demo.
    /// </summary>
    /// <param name="graphControl">The given graphControl</param>
    private static void CreateSampleGraph(GraphControl graphControl) {
      var graph = graphControl.Graph;
      var blackPortStyle = new ShapePortStyle { Shape = ShapeNodeShape.Ellipse };
      CreateSubgraph(graph, Themes.PaletteRed, "red", 0);
      CreateSubgraph(graph, Themes.PaletteOrange, "orange", 200);
      CreateSubgraph(graph, Themes.PaletteGreen, "green", 600);

      // the blue nodes have some additional ports besides the ones used by the edge
      var nodes = CreateSubgraph(graph, Themes.PaletteLightblue, "blue", 400);
      graph.AddPort(nodes[0], FreeNodePortLocationModel.Instance.CreateParameter(new PointD(1.0, 0.2)), blackPortStyle);
      graph.AddPort(nodes[0], FreeNodePortLocationModel.Instance.CreateParameter(new PointD(1.0, 0.8)), blackPortStyle);

      var candidateProvider = PortCandidateProviders.FromShapeGeometry(nodes[2], 0, 0.25, 0.5, 0.75);
      candidateProvider.Style = blackPortStyle;
      var candidates = candidateProvider.GetSourcePortCandidates(graphControl.InputModeContext);
      foreach (var portCandidate in candidates) {
        if (portCandidate.Validity != PortCandidateValidity.Dynamic) {
          portCandidate.CreatePort(graphControl.InputModeContext);
        }
      }

      // clear undo after initial graph loading
      graph.GetUndoEngine()!.Clear();
    }

    /// <summary>
    ///   Creates new graph items in the given graph using the given color set.
    /// </summary>
    /// <param name="graph">The graph instance in which to create sample items.</param>
    /// <param name="colorSet">The color set to use for the new sample items.</param>
    /// <param name="tag">The tag for new nodes created by this method.</param>
    /// <param name="yOffset">An y-coordinate offset for new nodes created by this method.</param>
    private static INode[] CreateSubgraph(IGraph graph, Palette colorSet, string tag, double yOffset) {
      var nodeStyle = DemoStyles.CreateDemoNodeStyle(colorSet);

      var n1 = graph.CreateNode(new RectD(100, 100 + yOffset, 60, 60), nodeStyle, tag);
      var n2 = graph.CreateNode(new RectD(500, 100 + yOffset, 60, 60), nodeStyle, tag);
      var n3 = graph.CreateNode(new RectD(300, 160 + yOffset, 60, 60), nodeStyle, tag);

      var edgeStyle = DemoStyles.CreateDemoEdgeStyle(colorSet, false);
      graph.CreateEdge(n1, n2, edgeStyle, tag);
      return [n1, n2, n3];
    }

    [STAThread]
    public static void Main() => Run<EdgeReconnectionDemo>();
  }
}
