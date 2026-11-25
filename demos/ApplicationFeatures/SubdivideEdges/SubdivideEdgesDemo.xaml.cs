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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.SubdivideEdges
{
  /// <summary>
  ///   This demo shows how to use a custom <see cref="NodeDropInputMode"/> for dragging nodes from a separate panel onto an
  ///   edge.
  /// </summary>
  [Demo("subdivide-edges", "application-features", "Subdivide Edges",
      Summary = "Shows how to subdivide an edge when a node is dragged on it.", Tags = ["interaction", "drag and drop"],
      Keywords = ["drop input mode", "create", "subdivide", "split", "drag and drop", "dnd", "insert", "divide"],
      RelatedDemos = [
          "drag-and-drop", "component-drag-and-drop", "graph-drag-and-drop", "custom-drag-and-drop",
          "drag-from-component"
      ])]
  public sealed partial class SubdivideEdgesDemo
  {
    public SubdivideEdgesDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.Graph.SetUndoEngineEnabled(true);

      // configures default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // add a sample graph
      CreateGraph();

      // configure drag and drop
      ConfigureDragAndDrop();
    }

    /// <summary>
    ///   Configures drag and drop for this demo.
    /// </summary>
    private void ConfigureDragAndDrop() {
      // create and configure the input mode
      var inputMode = new GraphEditorInputMode();

      // create and configure the node drop input mode
      var nodeDropInputMode = new SubdivideEdgeDropInputMode();
      // nodes with GroupNodeStyle should be created as group nodes
      nodeDropInputMode.IsGroupNodePredicate = draggedNode => draggedNode.Style is GroupNodeStyle;
      // assign the new node input mode to the graphControl
      inputMode.NodeDropInputMode = nodeDropInputMode;

      graphControl.InputMode = inputMode;

      InitializeDragAndDropPanel();
    }

    /// <summary>
    ///   Initializes the palette of nodes that can be dragged to the graphControl.
    /// </summary>
    private void InitializeDragAndDropPanel() {
      // prepare node styles for the palette
      var defaultNodeStyle = graphControl.Graph.NodeDefaults.Style;
      var otherNodeStyle = DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse, Themes.Palette12);

      var defaultGroupNodeStyle = graphControl.Graph.GroupNodeDefaults.Style;

      var paletteNodes = new INode[] {
          new SimpleNode {
              Layout = new RectD(0, 0, 40, 40),
              Style = (INodeStyle) defaultNodeStyle.Clone()
          },
          new SimpleNode {
              Layout = new RectD(0, 0, 40, 40),
              Style = (INodeStyle) otherNodeStyle.Clone()
          },
          new SimpleNode {
              Layout = new RectD(0, 0, 40, 40),
              Style = (INodeStyle) defaultGroupNodeStyle.Clone()
          }
      };
      new DragAndDropPanel(dndPanel) { MaxItemWidth = 40 }.PopulatePanel(paletteNodes);
    }


    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph, Themes.Palette12, edgeTheme: Themes.Palette12, groupTheme: Themes.Palette12);
    }

    /// <summary>
    ///   Creates a simple sample graph.
    /// </summary>
    private void CreateGraph() {
      var graph = graphControl.Graph;

      var node1 = graph.CreateNode(new PointD(0, 0));
      var node2 = graph.CreateNode(new PointD(200, 0));
      var node3 = graph.CreateNode(new PointD(200, 100));
      var node4 = graph.CreateNode(new PointD(350, 0));

      var group = graph.GroupNodes(node2, node3, node4);
      graph.AddLabel(group, "Group 1");

      graph.AddLabel(graph.CreateEdge(node1, node2), "Label 1");
      var edgeWithBend = graph.CreateEdge(node1, node3);
      graph.AddLabel(edgeWithBend, "Label 2");
      graph.AddBend(edgeWithBend, new PointD(0, 100));
      graph.AddLabel(graph.CreateEdge(node2, node4), "Label 3");

      graphControl.FitGraphBounds();
      graph.GetUndoEngine()?.Clear();
    }

    [STAThread]
    private static void Main() {
      Run<SubdivideEdgesDemo>();
    }
  }
}
