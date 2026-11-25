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

namespace yWorks.Demo.OrthogonalEdgeEditingCustomization
{
  /// <summary>
  ///   Shows how to customize orthogonal edge editing.
  /// </summary>
  [Demo("orthogonal-edge-editing-customization", "input", "Orthogonal Edge Editing Customization",
      Summary = "Shows how to customize orthogonal edge editing.", Tags = ["interaction"],
      Keywords = ["orthogonal edges", "move"], RelatedDemos = [])]
  public sealed partial class OrthogonalEdgeEditingCustomizationDemo
  {
    public OrthogonalEdgeEditingCustomizationDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Registers different IOrthogonalEdgeHelpers to demonstrate various custom behaviour.
    /// </summary>
    /// <param name="graph">The given graph</param>
    private static void RegisterOrthogonalEdgeHelperDecorators(IGraph graph) {
      var edgeDecorator = graph.GetDecorator().Edges;

      // Add different IOrthogonalEdgeHelpers to demonstrate various custom behavior
      edgeDecorator.OrthogonalEdgeHelper.AddFactory(edge => Equals(edge.Tag, "red"),
          edge => new RedOrthogonalEdgeHelper(edge));

      // Green edges have the regular orthogonal editing behavior and therefore,
      // don't need a custom implementation
      edgeDecorator.OrthogonalEdgeHelper.AddFactory(edge => Equals(edge.Tag, "green"),
          edge => new OrthogonalEdgeHelper(edge));

      edgeDecorator.OrthogonalEdgeHelper.AddFactory(edge => Equals(edge.Tag, "purple"),
          edge => new PurpleOrthogonalEdgeHelper(edge));

      edgeDecorator.OrthogonalEdgeHelper.AddFactory(edge => Equals(edge.Tag, "orange"),
          edge => new OrangeOrthogonalEdgeHelper(edge));

      edgeDecorator.OrthogonalEdgeHelper.AddFactory(edge => Equals(edge.Tag, "yellow"),
          edge => new YellowOrthogonalEdgeHelper(edge));

      edgeDecorator.OrthogonalEdgeHelper.AddFactory(edge => Equals(edge.Tag, "blue"),
          edge => new BlueOrthogonalEdgeHelper(edge));

      // Disable moving of the complete edge for orthogonal edges since this would create way too many bends
      edgeDecorator.PositionHandler.Hide(edge =>
          Equals(edge.Tag, "orange") || Equals(edge.Tag, "yellow") || Equals(edge.Tag, "green") ||
          Equals(edge.Tag, "purple"));

      // Add a custom BendCreator for blue edges that ensures orthogonality
      // if a bend is added to the first or last (non-orthogonal) segment
      edgeDecorator.BendCreator.AddWrapperFactory(edge => Equals(edge.Tag, "blue"),
          (edge, originalBendCreator) =>
              originalBendCreator != null ? new BlueBendCreator(edge, originalBendCreator) : null);

      // Add a custom EdgePortHandleProvider to make the handles of an
      // orange edge move within the bounds of the node
      edgeDecorator.PortHandleProvider.AddWrapperFactory(edge => Equals(edge.Tag, "orange"),
          (edge, _) => new PortLookupEdgePortHandleProvider(edge));
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;

      // Create a default editor input mode
      var graphEditorInputMode = new GraphEditorInputMode {
          // Just for user convenience: disable node, edge creation and clipboard operations
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowClipboardOperations = false,
          // disable deleting items
          DeletableItems = GraphItemTypes.None,
          // enable snapping for edges only
          SnapContext = new GraphSnapContext {
              SnappableItems = SnappableItems.Edge,
              CollectNodePairCenterSnapLines = false,
              CollectNodePairSnapLines = false,
              CollectNodePairSegmentSnapLines = false,
              CollectNodeSizes = false
          }
      };

      // and enable the undo feature.
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph component.
      graphControl.InputMode = graphEditorInputMode;

      // Disable auto-cleanup of ports since the purple nodes have explicit ports
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      // Create and register the edge decorations
      RegisterOrthogonalEdgeHelperDecorators(graph);

      CreateSampleGraph(graph, graphControl);
      //center graph
    }

    /// <summary>
    ///   Creates the sample graph of this demo.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph, GraphControl graphControl) {
      CreateSubgraph(graph, 0, Themes.PaletteRed, "red");
      CreateSubgraph(graph, 110, Themes.PaletteGreen, "green");
      CreateSubgraph(graph, 220, Themes.PalettePurple, "purple", true);
      CreateSubgraph(graph, 330, Themes.PaletteOrange, "orange");
      CreateSubgraph(graph, 440, Themes.Palette510, "yellow");

      // The blue edge has more bends than the other edges
      var blueEdge = CreateSubgraph(graph, 550, Themes.PaletteLightblue, "blue");
      graph.Remove(blueEdge.Bends[1]);
      graph.Remove(blueEdge.Bends[0]);
      graph.AddBend(blueEdge, new PointD(220, blueEdge.SourcePort.GetLocation().Y - 30));
      graph.AddBend(blueEdge, new PointD(300, blueEdge.SourcePort.GetLocation().Y - 30));
      graph.AddBend(blueEdge, new PointD(300, blueEdge.TargetPort.GetLocation().Y + 30));
      graph.AddBend(blueEdge, new PointD(380, blueEdge.TargetPort.GetLocation().Y + 30));

      graphControl.FitGraphBounds();
      // clear undo after initial graph loading
      graph.GetUndoEngine()!.Clear();
    }

    /// <summary>
    ///   Creates the sample graph of the given color with two nodes and a single edge.
    /// </summary>
    private static IEdge CreateSubgraph(
        IGraph graph,
        double yOffset,
        Palette colorSet,
        string tag,
        bool createPorts = false) {
      // Create two nodes
      var nodeStyle = DemoStyles.CreateDemoNodeStyle(colorSet);

      var n1 = graph.CreateNode(new RectD(110, 100 + yOffset, 40, 40), nodeStyle, tag);
      var n2 = graph.CreateNode(new RectD(450, 130 + yOffset, 40, 40), nodeStyle, tag);

      // Create an edge, either between the two nodes or between their ports
      IEdge edge;
      var edgeStyle = DemoStyles.CreateDemoEdgeStyle(colorSet, false, orthogonalEditing: true);

      if (!createPorts) {
        edge = graph.CreateEdge(n1, n2, edgeStyle, tag);
      } else {
        var p1 = CreateSamplePorts(graph, n1, true);
        var p2 = CreateSamplePorts(graph, n2, false);
        edge = graph.CreateEdge(p1[1], p2[2], edgeStyle, tag);
      }

      // Add bends that create a vertical segment in the middle of the edge
      var sourcePortLocation = edge.SourcePort.GetLocation();
      var targetPortLocation = edge.TargetPort.GetLocation();
      var x = (sourcePortLocation.X + targetPortLocation.X) / 2;
      graph.AddBend(edge, new PointD(x, sourcePortLocation.Y));
      graph.AddBend(edge, new PointD(x, targetPortLocation.Y));

      return edge;
    }

    /// <summary>
    ///   Adds some ports to the given node.
    /// </summary>
    private static IPort[] CreateSamplePorts(IGraph graph, INode node, bool toEastSide) {
      var nodeScaledPortLocationModel = FreeNodePortLocationModel.Instance;
      var x = toEastSide ? 0.9 : 0.1;
      return [
          graph.AddPort(node, nodeScaledPortLocationModel.CreateParameter(new PointD(x, 0.05))),
          graph.AddPort(node, nodeScaledPortLocationModel.CreateParameter(new PointD(x, 0.35))),
          graph.AddPort(node, nodeScaledPortLocationModel.CreateParameter(new PointD(x, 0.65))),
          graph.AddPort(node, nodeScaledPortLocationModel.CreateParameter(new PointD(x, 0.95)))
      ];
    }

    [STAThread]
    public static void Main() => Run<OrthogonalEdgeEditingCustomizationDemo>();
  }
}
