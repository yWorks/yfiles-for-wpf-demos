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

namespace yWorks.Demo.PositionHandler
{
  /// <summary>
  ///   Shows how to customize and restrict the movement behavior of nodes.
  /// </summary>
  [Demo("position-handler", "input", "Position Handler",
      Summary = "Shows how to customize and restrict the movement behavior of nodes.", Tags = ["interaction"],
      Keywords = ["position handlers", "move"])]
  public sealed partial class PositionHandlerDemo
  {
    public PositionHandlerDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Registers a callback function as decorator that provides a custom
    ///   <see cref="IPositionHandler"/> for each node.
    ///   This callback function is called whenever a node in the graph is queried
    ///   for its <see cref="IPositionHandler"/>. In this case, the <paramref name="node"/>
    ///   parameter will be set to that node and the originalHandler
    ///   parameter will be set to the position handler that would have been returned without
    ///   setting this function as decorator.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="boundaryRectangle">The rectangle that limits the nodes position.</param>
    private static void RegisterPositionHandler(IGraph graph, MutableRectangle boundaryRectangle) {
      graph.GetDecorator().Nodes.PositionHandler.AddWrapperFactory((node, originalHandler) => {
        // Check if it is a known tag and choose the respective implementation.
        // Fallback to the default behavior otherwise.
        if (node.Tag is not string tag || originalHandler == null) {
          return originalHandler;
        }

        return tag switch {
            // This implementation delegates certain behavior to the default implementation
            "orange" => new OrangePositionHandler(boundaryRectangle, node, originalHandler),
            // A simple implementation that prohibits moving
            "red" => new RedPositionHandler(),
            // Implementation that uses two levels of delegation to create a combined behavior
            "blue" => new OrangePositionHandler(boundaryRectangle, node, new GreenPositionHandler(originalHandler)),
            // Another implementation that delegates certain behavior to the default implementation
            "green" => new GreenPositionHandler(originalHandler),
            _ => originalHandler
        };
      });
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;
      ConfigureUserInteraction(graphControl);

      // Create the rectangle that limits the movement of some nodes
      // and add it to the graphControl.
      var boundaryRectangle = new MutableRectangle(20, 20, 480, 400);
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, boundaryRectangle,
          new LimitingRectangleRenderer());

      RegisterPositionHandler(graph, boundaryRectangle);
      CreateSampleGraph(graph);
    }

    /// <summary>
    ///   Restricts interactive editing to selecting and moving nodes for the given graph component.
    /// </summary>
    /// <param name="graphComponent">The demo's graph component.</param>
    private static void ConfigureUserInteraction(GraphControl graphComponent) {
      // Create a default editor input mode
      var graphEditorInputMode = new GraphEditorInputMode {
          // Just for user convenience: disable node, edge creation and clipboard operations,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowClipboardOperations = false,
          // don't show resize handles,
          ShowHandleItems = GraphItemTypes.None
      };

      // Finally, set the input mode to the graph component.
      graphComponent.InputMode = graphEditorInputMode;
    }

    /// <summary>
    ///   Creates the sample graph for this demo.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);

      CreateNode(graph, 100, 100, 100, 30, Themes.PaletteRed, "red", "Unmovable");
      CreateNode(graph, 300, 100, 100, 30, Themes.PaletteGreen, "green", "One Axis");
      CreateNode(graph, 80, 250, 140, 40, Themes.PaletteOrange, "orange", "Limited to Rectangle");
      CreateNode(graph, 280, 250, 140, 40, Themes.PaletteLightblue, "blue", "Limited to Rectangle\nand One Axis");

      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a sample node for this demo.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="x">The node's x-coordinate.</param>
    /// <param name="y">The node's y-coordinate.</param>
    /// <param name="w">The node's width.</param>
    /// <param name="h">The node's height.</param>
    /// <param name="color">The given color set name.</param>
    /// <param name="tag">The tag to identify the position handler.</param>
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
      graph.AddLabel(node, labelText, graph.NodeDefaults.Labels.LayoutParameter,
          DemoStyles.CreateDemoNodeLabelStyle(color));
    }

    [STAThread]
    public static void Main() => Run<PositionHandlerDemo>();
  }
}
