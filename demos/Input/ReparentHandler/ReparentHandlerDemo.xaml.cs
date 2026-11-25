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

namespace yWorks.Demo.ReparentHandler
{
  /// <summary>
  ///   This demo shows how reparenting of nodes can be customized.
  /// </summary>
  [Demo("reparent-handler", "input", "Reparent Handler", Summary = "Shows how reparenting of nodes can be customized.",
      Tags = ["interaction", "grouping"], Keywords = ["re-parent handlers"])]
  public sealed partial class ReparentHandlerDemo
  {
    public ReparentHandlerDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      // initialize the GraphComponent
      var graph = graphControl.Graph;

      // create a default editor input mode and configure it
      var graphEditorInputMode = new GraphEditorInputMode {
          // assign the custom reparent handler of this demo
          ReparentNodeHandler = new DemoReparentNodeHandler(),
          // Just for user convenience: disable edge creation, ...
          AllowCreateEdge = false,
          // ... node creation, and ...
          AllowCreateNode = false,
          // ... group node creation as well as ...
          AllowGroupSelection = false,
          // ... clipboard operations
          AllowClipboardOperations = false
      };

      // enable the undo feature.
      graph.SetUndoEngineEnabled(true);

      // initialize the default style of the nodes and edges
      DemoStyles.InitDemoStyles(graph);

      // Finally, assign the configured input mode to the graph component.
      graphControl.InputMode = graphEditorInputMode;

      CreateSampleGraph(graphControl);
    }

    /// <summary>
    ///   Creates the demo's sample graph.
    /// </summary>
    private void CreateSampleGraph(GraphControl graphControl) {
      var graph = graphControl.Graph;

      // create some group nodes ...
      var group1 = CreateGroupNode(graph, 100, 100, Themes.PaletteLightblue, "blue", "Only Blue Children");
      var group2 = CreateGroupNode(graph, 160, 130, Themes.PaletteLightblue, "blue", "Only Blue Children");
      var greenGroup = CreateGroupNode(graph, 100, 350, Themes.PaletteGreen, "green", "Only Green Children");
      CreateGroupNode(graph, 400, 350, Themes.PaletteGreen, "green", "Only Green Children");

      // ... and some regular nodes
      var blueNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteLightblue);
      var greenNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteGreen);
      var redNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteRed);

      var blueNode = graph.CreateNode(new RectD(110, 130, 30, 30), blueNodeStyle, "blue");
      var greenNode = graph.CreateNode(new RectD(130, 380, 30, 30), greenNodeStyle, "green");
      graph.CreateNode(new RectD(400, 100, 30, 30), redNodeStyle, "red");
      graph.CreateNode(new RectD(500, 100, 30, 30), greenNodeStyle, "green");
      graph.CreateNode(new RectD(400, 200, 30, 30), blueNodeStyle, "blue");
      graph.CreateNode(new RectD(500, 200, 30, 30), redNodeStyle, "red");

      graph.GroupNodes(group1, [blueNode, group2]);
      graph.GroupNodes(greenGroup, [greenNode]);

      // ensure that the outer blue group completely contains its inner group
      graph.SetNodeLayout(group1, new RectD(100, 100, 200, 150));
      // uncomment the following line to adjust the bounds of the outer blue group automatically
      // graph.AdjustGroupNodeLayout(group1);
      graphControl.FitGraphBounds();
      // clear undo after initial graph creation
      graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Creates a group node for the sample graph with a specific styling.
    /// </summary>
    /// <param name="graph">The given graph</param>
    /// <param name="x">The node's x-coordinate</param>
    /// <param name="y">The node's y-coordinate</param>
    /// <param name="color">The color set that defines the node's styling</param>
    /// <param name="tag">The tag to identify the reparent handler</param>
    /// <param name="labelText">The node's label text</param>
    private static INode CreateGroupNode(
        IGraph graph,
        double x,
        double y,
        Palette color,
        string tag,
        string labelText) {
      var groupNode = graph.CreateGroupNode(layout: new RectD(x, y, 130, 100),
          style: DemoStyles.CreateDemoGroupStyle(color), tag: tag);
      graph.AddLabel(groupNode, labelText, style: DemoStyles.CreateDemoGroupLabelStyle(color));

      return groupNode;
    }

    [STAThread]
    public static void Main() => Run<ReparentHandlerDemo>();
  }
}
