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
using yWorks.Utils;

namespace yWorks.Demo.ReshapeHandleProviderConfiguration
{
  /// <summary>
  ///   Shows how resizing of nodes can be customized.
  /// </summary>
  [Demo("reshape-handle-provider-configuration", "input", "Reshape Handle Provider Configuration",
      Summary = "Shows how resizing of nodes can be customized.", Tags = ["interaction", "resizing"],
      Keywords = ["handles", "reshape", "size", "scale"])]
  public sealed partial class ReshapeHandleProviderConfigurationDemo
  {
    private static ApplicationState applicationState = null!;

    public ReshapeHandleProviderConfigurationDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Registers a callback function as a decorator that provides a customized
    ///   <see cref="IReshapeHandleProvider"/> for each node.
    ///   This callback function is called whenever a node in the graph is queried
    ///   for its <see cref="IReshapeHandleProvider"/>.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="boundaryRectangle">The rectangle that limits the node's size.</param>
    private static void RegisterReshapeHandleProvider(IGraph graph, RectD boundaryRectangle) {
      var nodeDecorator = graph.GetDecorator().Nodes;

      // deactivate reshape handling for the red node
      nodeDecorator.ReshapeHandleProvider.Hide(node => Equals(node.Tag, "red"));

      // return customized reshape handle provider for the orange, blue and green node
      nodeDecorator.ReshapeHandleProvider.AddFactory(
          node => Equals(node.Tag, "orange") || Equals(node.Tag, "blue") || Equals(node.Tag, "green") ||
              Equals(node.Tag, "purple") || Equals(node.Tag, "darkblue") || Equals(node.Tag, "gold"), node => {
            // Obtain the tag from the node
            var nodeTag = (string) node.Tag!;

            // Create a default reshape handle provider for nodes
            var reshapeHandler = node.Lookup<IReshapeHandler>()!;
            var provider = new NodeReshapeHandleProvider(node, reshapeHandler, HandlePositions.Border);

            // Customize the handle provider depending on the node's color
            if (nodeTag == "orange") {
              // Restrict the node bounds to the boundaryRectangle
              provider.MaximumBoundingArea = boundaryRectangle;
            } else if (nodeTag == "green") {
              // Show only handles at the corners and always use aspect ratio resizing
              provider.HandlePositions = HandlePositions.Corners;
              provider.RatioReshapeRecognizer = EventRecognizers.Always;
            } else if (nodeTag == "blue") {
              // Restrict the node bounds to the boundaryRectangle and
              // show only handles at the corners and always use aspect ratio resizing
              provider.MaximumBoundingArea = boundaryRectangle;
              provider.HandlePositions = HandlePositions.Corners;
              provider.RatioReshapeRecognizer = EventRecognizers.Always;
            } else if (nodeTag == "purple") {
              return new PurpleNodeReshapeHandleProvider(node, reshapeHandler);
            } else if (nodeTag == "darkblue") {
              provider.HandlePositions = HandlePositions.BottomRight;
              provider.CenterReshapeRecognizer = EventRecognizers.Always;
            } else if (nodeTag == "gold") {
              return new ClickableNodeReshapeHandleProvider(node, reshapeHandler, applicationState);
            }
            return provider;
          });
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;

      // Create a default editor input mode
      var graphEditorInputMode = new GraphEditorInputMode {
          // Just for user convenience: disable node, edge creation and clipboard operations,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowClipboardOperations = false,
          MovableSelectedItems = GraphItemTypes.None,
          MovableUnselectedItems = GraphItemTypes.None
      };

      applicationState = new ApplicationState(graphEditorInputMode, graph, true);

      // and enable the undo feature.
      graph.SetUndoEngineEnabled(true);

      // Finally, set the input mode to the graph control.
      graphControl.InputMode = graphEditorInputMode;

      // Create the rectangle that limits the node size of some nodes
      // and add it to the graphControl.
      var boundaryRectangle = new MutableRectangle(20, 20, 480, 550);
      var renderTree = graphControl.RenderTree;
      renderTree.CreateElement(renderTree.BackgroundGroup, boundaryRectangle, new LimitingRectangleRenderer());

      RegisterReshapeHandleProvider(graph, boundaryRectangle.ToRectD());
      CreateSampleGraph(graphControl);
    }

    /// <summary>
    ///   Creates the sample graph of this demo.
    /// </summary>
    private static void CreateSampleGraph(GraphControl graphComponent) {
      var graph = graphComponent.Graph;
      DemoStyles.InitDemoStyles(graph);

      CreateNode(graph, 80, 100, 140, 30, Themes.PaletteRed, "red", "Fixed size");
      CreateNode(graph, 300, 100, 140, 30, Themes.PaletteGreen, "green", "Keep aspect ratio");
      CreateNode(graph, 80, 200, 140, 50, Themes.PaletteBlue, "darkblue", "Keep center");
      CreateNode(graph, 300, 200, 140, 50, Themes.PalettePurple, "purple", "Keep aspect ratio\nat corners");
      CreateNode(graph, 80, 310, 140, 30, Themes.PaletteOrange, "orange", "Limited to rectangle");
      CreateNode(graph, 300, 300, 140, 50, Themes.PaletteLightblue, "blue",
          "Limited to rectangle\nand keep aspect ratio");
      CreateNode(graph, 80, 400, 140, 70, Themes.Palette510, "gold", "Keep aspect ratio\ndepending on state:\n keep");

      //center graph
      graphComponent.FitGraphBounds();

      // clear undo after initial graph loading
      graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Creates a sample node for this demo.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="x">The node's x-coordinate.</param>
    /// <param name="y">The node's y-coordinate.</param>
    /// <param name="w">The node's width.</param>
    /// <param name="h">The node's height.</param>
    /// <param name="color">The color palette that defines the node color.</param>
    /// <param name="tag">The tag to identify the reshape handler.</param>
    /// <param name="labelText">The node's label text.</param>
    private static void CreateNode(
        IGraph graph,
        double x,
        double y,
        double w,
        double h,
        Palette color,
        string tag,
        string labelText) {
      var node = graph.CreateNode(new RectD(x, y, w, h), DemoStyles.CreateDemoNodeStyle(color), tag);
      graph.AddLabel(node, labelText, style: DemoStyles.CreateDemoNodeLabelStyle(color));
    }

    [STAThread]
    public static void Main() => Run<ReshapeHandleProviderConfigurationDemo>();
  }
}
