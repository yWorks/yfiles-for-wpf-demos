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

namespace yWorks.Demo.PortCandidateProvider
{
  /// <summary>
  ///   This demo shows how edge creation can be customized by implementing the
  ///   <see cref="IPortCandidateProvider"/> interface.
  /// </summary>
  [Demo("port-candidate-provider", "input", "Port Candidate Provider",
      Summary = "Shows how edge creation can be customized.", Tags = ["interaction", "port"],
      Keywords = ["port candidate providers"])]
  public sealed partial class PortCandidateProviderDemo
  {
    public PortCandidateProviderDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;

      // Disable automatic cleanup of unconnected ports since some nodes have a predefined set of ports
      graph.NodeDefaults.Ports.AutoCleanUp = false;
      graph.NodeDefaults.Ports.Style = new ShapePortStyle { Shape = ShapeNodeShape.Ellipse };

      // Initialize basic demo styles
      DemoStyles.InitDemoStyles(graph);

      // Create a default editor input mode and configure it
      var graphEditorInputMode = new GraphEditorInputMode {
          // Just for user convenience: disable node creation and clipboard operations
          AllowCreateNode = false,
          AllowClipboardOperations = false,
          SelectableItems = GraphItemTypes.All & ~GraphItemTypes.Port
      };
      graphEditorInputMode.CreateEdgeInputMode.UseHitItemsCandidatesOnly = true;

      // and enable the undo feature.
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph component.
      graphControl.InputMode = graphEditorInputMode;
      RegisterPortCandidateProvider(graph);

      // crop the edge path at the port and not at the node bounds
      graph.GetDecorator().Ports.EdgePathCropper.AddConstant(new EdgePathCropper {
          CropAtPort = true,
          ExtraCropLength = 3
      });

      // draw edges in front of the nodes
      graphControl.GraphModelManager.EdgeGroup.ToFront();
      graphControl.GraphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.Nodes;

      // create the graph
      CreateSampleGraph(graphControl);
      graphControl.UpdateContentBounds();
    }

    /// <summary>
    ///   Registers a callback function as decorator that provides a custom
    ///   <see cref="IPortCandidateProvider"/> for each node.
    ///   This callback function is called whenever a node in the graph is queried
    ///   for its <see cref="IPortCandidateProvider"/>. In this case, the 'node'
    ///   parameter will be assigned that node.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    private static void RegisterPortCandidateProvider(IGraph graph) {
      graph.GetDecorator().Nodes.PortCandidateProvider.AddFactory(node => {
        // Obtain the tag from the node
        var nodeTag = node.Tag;

        // Check if it is a known tag and choose the respective implementation
        if (nodeTag is not string tag) {
          return null;
        }
        return tag switch {
            "red" => new RedPortCandidateProvider(node),
            "blue" => new BluePortCandidateProvider(node),
            "green" => new GreenPortCandidateProvider(node),
            "orange" => new OrangePortCandidateProvider(node),
            "purple" => new PurplePortCandidateProvider(node),
            // otherwise revert to default behavior
            _ => null
        };
      });
    }

    /// <summary>
    ///   Creates the sample graph for this demo.
    /// </summary>
    /// <param name="graphControl">The given graphControl.</param>
    private static void CreateSampleGraph(GraphControl graphControl) {
      var graph = graphControl.Graph;

      CreateNode(graph, 100, 100, 80, 30, Themes.PaletteRed, "red", "No Edge");
      CreateNode(graph, 350, 200, 80, 30, Themes.PaletteRed, "red", "No Edge");
      CreateNode(graph, 350, 100, 80, 30, Themes.PaletteGreen, "green", "Green Only");
      CreateNode(graph, 100, 200, 80, 30, Themes.PaletteGreen, "green", "Green Only");

      var purpleNode = graph.CreateNode(new RectD(350, 470, 110, 110),
          DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Octagon, Themes.PalettePurple), "purple");
      graph.AddLabel(purpleNode, "Dynamic Ports on\nShape Boundary", graph.NodeDefaults.Labels.LayoutParameter,
          DemoStyles.CreateDemoNodeLabelStyle(Themes.PalettePurple));

      var blue1 = CreateNode(graph, 100, 300, 80, 30, Themes.PaletteLightblue, "blue", "One   Port");
      graph.AddPort(blue1, FreeNodePortLocationModel.Center);

      var blue2 = CreateNode(graph, 350, 300, 100, 100, Themes.PaletteLightblue, "blue", "Many Ports");
      var portCandidateProvider = PortCandidateProviders.FromShapeGeometry(blue2, 0, 0.25, 0.5, 0.75);
      var candidates = portCandidateProvider.GetSourcePortCandidates(graphControl.InputModeContext);
      foreach (var portCandidate in candidates) {
        if (portCandidate.Validity != PortCandidateValidity.Dynamic) {
          portCandidate.CreatePort(graphControl.InputModeContext);
        }
      }

      // The orange node
      CreateNode(graph, 100, 400, 100, 100, Themes.PaletteOrange, "orange", "Dynamic Ports");

      //center graph
      graphControl.FitGraphBounds();
      // clear undo after initial graph loading
      graph.GetUndoEngine()!.Clear();
    }

    /// <summary>
    ///   Creates a sample node for the given color set and adds its label.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="x">The node's x-coordinate.</param>
    /// <param name="y">The node's y-coordinate.</param>
    /// <param name="w">The node's width.</param>
    /// <param name="h">The node's height.</param>
    /// <param name="colorSet">The color set to use.</param>
    /// <param name="tag">The tag to identify the port candidate provider.</param>
    /// <param name="labelText">The nodes label's text.</param>
    private static INode CreateNode(
        IGraph graph,
        double x,
        double y,
        double w,
        double h,
        Palette colorSet,
        string tag,
        string labelText) {
      var node = graph.CreateNode(new RectD(x, y, w, h), DemoStyles.CreateDemoNodeStyle(colorSet), tag);
      graph.AddLabel(node, labelText, graph.NodeDefaults.Labels.LayoutParameter,
          DemoStyles.CreateDemoNodeLabelStyle(colorSet));
      return node;
    }

    [STAThread]
    public static void Main() => Run<PortCandidateProviderDemo>();
  }
}
