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
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SimpleDragAndDrop
{
  [Demo("simple-drag-and-drop", "application-features", "Simple Drag And Drop",
      Summary = "Shows how to enable dragging nodes from a panel and drop them into the graph component.",
      Tags = ["interaction", "drag and drop"], Keywords = ["DropInputMode", "move", "events", "input mode", "dnd"],
      RelatedDemos = [
          "drag-and-drop", "graph-drag-and-drop", "custom-drag-and-drop", "drag-from-component"
      ])]
  public sealed partial class SimpleDragAndDropDemo
  {
    public SimpleDragAndDropDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // Initialize the graph control.
      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      // Configure default styles for newly created graph elements.
      InitializeGraph(graphControl.Graph);

      // Build the graph from the given data set.
      BuildGraph(graphControl.Graph);

      // Layout and center the graph.
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // Enable the undo engine now to prevent undoing the graph creation.
      graphControl.Graph.SetUndoEngineEnabled(true);

      // Configure drag and drop.
      ConfigureDragAndDrop(inputMode, graphControl, palettePanel);
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var jsonDocument = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, jsonDocument);
    }

    /// <summary>
    ///   Configures drag and drop for this demo.
    /// </summary>
    private static void ConfigureDragAndDrop(
        GraphEditorInputMode inputMode,
        GraphControl graphControl,
        SideBar palettePanel) {
      // Obtain the input mode for handling dropped nodes from the GraphEditorInputMode.
      var nodeDropInputMode = inputMode.NodeDropInputMode;
      // By default the mode available in GraphEditorInputMode is disabled, so first enable it.
      nodeDropInputMode.Enabled = true;
      // Certain nodes should be created as group nodes. In this case, we distinguish them by their style.
      nodeDropInputMode.IsGroupNodePredicate = draggedNode => draggedNode.Style is GroupNodeStyle;
      // When dragging the node within the GraphControl, we want to show a preview of that node.
      nodeDropInputMode.ShowPreview = true;

      InitializeDragAndDropPanel(graphControl, palettePanel);
    }

    /// <summary>
    ///   Initializes the palette of nodes that can be dragged to the graph control.
    /// </summary>
    private static void InitializeDragAndDropPanel(GraphControl graphControl, SideBar palettePanel) {
      // Prepare node styles for the palette.
      var nodeStyles = new[] {
          graphControl.Graph.NodeDefaults.Style,
          DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse),
          graphControl.Graph.GroupNodeDefaults.Style
      };

      // Add a visual for each node style to the palette.
      // The panel exports visuals using a separate GraphControl and UIElementExporter.
      // It clones each preview node's style and starts a native drag carrying that node as data.
      var panel = new DragAndDropPanel(palettePanel);
      // Create preview nodes with which the GraphControl can render previews during drag gestures.
      panel.PopulatePanel(nodeStyles.Select(style => (IModelItem) new SimpleNode {
          Layout = new RectD(0, 0, 40, 40),
          Style = style
      }));
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // Set styles for this demo.
      DemoStyles.InitDemoStyles(graph);

      // Set the style, label style and label parameter for group nodes.
      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabBrush = new SolidColorBrush(Color.FromRgb(0x46, 0xa8, 0xd5)),
          TabPosition = GroupNodeStyleTabPosition.TopLeading,
          ContentAreaBrush = new SolidColorBrush(Color.FromRgb(0xb5, 0xdc, 0xee))
      };
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          TextAlignment = TextAlignment.Left,
          TextBrush = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee))
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabParameter();

      // Set sizes and locations specific to this demo.
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    private void OnNewExecuted(object sender, ExecutedRoutedEventArgs e) {
      graphControl.Graph.Clear();
      graphControl.Graph.GetUndoEngine()?.Clear();
      graphControl.FitGraphBounds();
      e.Handled = true;
    }

    [STAThread]
    public static void Main() {
      Run<SimpleDragAndDropDemo>();
    }
  }
}
