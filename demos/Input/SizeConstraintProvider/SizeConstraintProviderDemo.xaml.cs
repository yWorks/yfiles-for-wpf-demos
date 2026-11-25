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

namespace yWorks.Demo.SizeConstraintProvider
{
  /// <summary>
  ///   Shows how resizing of nodes can be restricted.
  /// </summary>
  [Demo("size-constraint-provider", "input", "Size Constraint Provider",
      Summary = "Shows how resizing of nodes can be restricted.", Tags = ["interaction"],
      Keywords = ["size constraint providers", "handles"], RelatedDemos = [])]
  public sealed partial class SizeConstraintProviderDemo
  {
    public SizeConstraintProviderDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Registers a callback function as decorator that provides a custom
    ///   <see cref="INodeSizeConstraintProvider"/> for each node.
    ///   This callback function is called whenever a node in the graph is queried
    ///   for its <see cref="INodeSizeConstraintProvider"/>. In this case, node
    ///   parameter will be set to that node.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="boundaryRectangle">The rectangle that limits the node's size.</param>
    private static void RegisterSizeConstraintProvider(IGraph graph, MutableRectangle boundaryRectangle) {
      var nodeDecorator = graph.GetDecorator().Nodes;
      nodeDecorator.SizeConstraintProvider.AddFactory(node => {
        // Obtain the tag from the node.
        var nodeTag = node.Tag as string;
        // Check if it is a known tag and choose the respective implementation.
        // Fallback to the default behavior otherwise.
        if (nodeTag == "blue") {
          return new BlueSizeConstraintProvider(node);
        }
        if (nodeTag == "green") {
          return new GreenSizeConstraintProvider(node);
        }
        if (nodeTag == "orange") {
          return new NodeSizeConstraintProvider(new SizeD(50, 50), new SizeD(300, 300), boundaryRectangle);
        }
        return null;
      });
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;

      // Create a default editor input mode.
      graphControl.InputMode = new GraphEditorInputMode {
          // Just for user convenience: disable node, edge creation and clipboard operations,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowClipboardOperations = false,
          MovableSelectedItems = GraphItemTypes.None
      };

      // Create the rectangle that limits the node's size
      // and add it to the graphControl.
      var boundaryRectangle = new MutableRectangle(210, 350, 30, 30);
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.HighlightGroup, boundaryRectangle,
          new LimitingRectangleRenderer());

      // Register size constraint providers that are the main subject of this demo.
      RegisterSizeConstraintProvider(graph, boundaryRectangle);

      CreateSampleGraph(graphControl);

      // Enable undo and redo.
      graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Creates the demo's sample graph.
    /// </summary>
    private void CreateSampleGraph(GraphControl graphComponent) {
      var graph = graphComponent.Graph;
      DemoStyles.InitDemoStyles(graph);

      CreateNode(graph, 100, 100, 100, 60, Themes.PaletteLightblue, "blue", "Never Shrink\n(Max 3x)");
      CreateNode(graph, 300, 100, 160, 30, Themes.PaletteLightblue, "blue", "Never Shrink (Max 3x)");
      CreateNode(graph, 100, 215, 100, 30, Themes.PaletteGreen, "green", "Enclose Label");
      CreateNode(graph, 300, 200, 140, 80, Themes.PaletteGreen, "green", "Enclose Label,\nEven Large Ones");
      CreateNode(graph, 200, 340, 140, 140, Themes.PaletteOrange, "orange", "Encompass Rectangle,\nMin and Max Size");

      // Center the graph.
      graphComponent.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a sample node for this demo.
    /// </summary>
    /// <param name="graph">The given graph.</param>
    /// <param name="x">The node's x-coordinate.</param>
    /// <param name="y">The node's y-coordinate.</param>
    /// <param name="w">The node's width.</param>
    /// <param name="h">The node's height.</param>
    /// <param name="color">The color set to use for the new node and its label.</param>
    /// <param name="tag">The tag to identify the size constraint provider.</param>
    /// <param name="labelText">The node label's text.</param>
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
    private static void Main(string[] args) {
      Run<SizeConstraintProviderDemo>();
    }
  }
}
