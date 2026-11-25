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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Demo.GraphAnalysis.UI;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using static yWorks.Demo.GraphAnalysis.DemoTypes;

namespace yWorks.Demo.GraphAnalysis
{
  [Demo("graph-analysis", "showcase", "Graph Analysis",
      Summary =
          "Showcases yFiles' algorithms that help analyzing the structure of a graph, such as shortest paths, flows, centrality measures",
      Tags = ["analysis", "graph structure"], Keywords = [
          "context menu", "centrality", "connectivity", "reachability", "cycles", "minimum spanning tree", "mst",
          "shortest paths", "data analysis", "biconnected", "strongly connected components", "k-core", "chains",
          "single-source", "degree centrality", "closeness centrality", "eigenvector centrality", "weights",
          "node edge betweenness centrality", "page rank", "substructures", "stars", "trees", "cliques", "directed",
          "direction"
      ], RelatedDemos = ["large-graphs", "layout-styles"])]
  public sealed partial class GraphAnalysisDemo
  {
    private ComponentSwitchingInputMode? componentSwitcher;

    private IGraph Graph => graphControl.Graph;

    public GraphAnalysisDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Main function for running the Graph Analysis demo.
    /// </summary>
    protected override void Initialize() {
      settings = new Settings();

      LoadGraphSamples();

      graphControl.Graph = new AnalysisGraph();
      // configure the interaction for this demo
      graphControl.InputMode = CreateEditorMode();

      // enable undo support
      Graph.SetUndoEngineEnabled(true);

      // initialize the styles for nodes and edges.
      InitializeStyles();

      // create the toolbar's elements and binds them to the appropriate commands
      InitializeToolbar();
      // register the listeners for notifying graph structural changes to update the
      // graph information displayed in the right panel
      InitializeGraphInformation();

      settings.Sample = settings.Algorithms[0];
    }

    private void LoadGraphSamples() {
      var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
      using (var stream = LoadAssemblyResource("samples.json")) {
        samples = JsonSerializer.Deserialize<Dictionary<string, int[][]>>(stream, options)!;
      }
    }

    /// <summary>
    ///   Initializes the graph instance and set default styles.
    /// </summary>
    private void InitializeStyles() {
      var graph = Graph;

      // set the default style for the nodes
      graph.NodeDefaults.Style = new TagColoredShapeNodeStyle(this);
      graph.NodeDefaults.Labels.Style = new LabelStyle {
          TextSize = 10,
          TextBrush = Brushes.White
      };

      // initialize the selection style for nodes
      graph.GetDecorator().Nodes.SelectionRenderer.AddConstant(new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.Ellipse,
              Pen = new Pen(Brushes.Gray, 5),
              Brush = null
          },
          Margins = new InsetsD(5)
      });

      // disable focus decoration
      graphControl.FocusIndicatorManager.Enabled = false;

      // set the default style for the edges
      graph.EdgeDefaults.Style = new TagColoredPolylineEdgeStyle(this);

      // set the style and the label model for the edge labels
      graph.EdgeDefaults.Labels.LayoutParameter = FreeEdgeLabelModel.Instance.CreateParameter();
      graph.EdgeDefaults.Labels.Style = new LabelStyle {
          TextSize = 10,
          TextBrush = Brushes.Gray
      };

      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Center;
    }

    /// <summary>
    ///   Configures the interaction for this demo.
    /// </summary>
    private GraphEditorInputMode CreateEditorMode() {
      // configure interaction
      var inputMode = new GraphEditorInputMode {
          AllowAddLabel = false,
          AllowCreateBend = false,
          DeletableItems = GraphItemTypes.Edge | GraphItemTypes.Node,
          ShowHandleItems = GraphItemTypes.None,
          ContextMenuItems = GraphItemTypes.Node
      };

      // allow to interactively switch the displayed component
      componentSwitcher = new ComponentSwitchingInputMode(graphControl, () => result,
          node => GetTag(node)?.Type is NodeType.Start or NodeType.End, id => colors[id % colors.Length],
          OnHighlightComponent);

      // deletion
      inputMode.DeletingSelection += (_, _) => {
        // Derived labels must not become part of the deletion's undo unit.
        ResetGraph();
      };
      inputMode.DeletedSelection += async (_, _) => {
        ApplyAlgorithm();
        await RunLayout([], false);
      };

      // edge creation
      inputMode.CreateEdgeInputMode.EdgeCreated += async (_, e) => {
        var edge = e.Item;

        if (settings.WeightIndex == 1) {
          Graph.AddLabel(edge, "1", tag: "weight");
        }

        ApplyAlgorithm();
        var incrementalNodes = new[] { edge.GetSourceNode(), edge.GetTargetNode() };
        await RunLayout(incrementalNodes, false);
      };

      async Task OnDragFinished(MoveInputMode inputModeMove) {
        var affectedNodes = inputModeMove.AffectedItems.OfType<INode>().ToArray();
        if (affectedNodes.Length < Graph.Nodes.Count) {
          ApplyAlgorithm();
          await RunLayout(affectedNodes, false);
        }
      }

      inputMode.MoveSelectedItemsInputMode.DragFinished +=
          async (_, _) => await OnDragFinished(inputMode.MoveSelectedItemsInputMode);
      inputMode.MoveUnselectedItemsInputMode.DragFinished +=
          async (_, _) => await OnDragFinished(inputMode.MoveUnselectedItemsInputMode);

      // run the algorithm on node creation, edge port changes or label text changes
      Graph.EdgePortsChanged += (_, _) => QueueApplyAlgorithm();

      Graph.NodeCreated += (_, _) => QueueApplyAlgorithm();

      inputMode.LabelEdited += (_, _) => QueueApplyAlgorithm();
      Graph.LabelTextChanged += (_, _) => QueueApplyAlgorithm();

      inputMode.EditLabelPredicate = item => item is ILabel label && Equals(label.Tag, "weight");
      inputMode.EditLabelInputMode.ValidateLabelText += (_, args) => {
        // labels must contain only positive numbers
        if (!double.TryParse(args.NewText, out var value) || !double.IsFinite(value) || value <= 0) {
          args.Cancel = true;
        }
      };

      // add the context menu
      InitializeContextMenu(inputMode);
      return inputMode;
    }

    [STAThread]
    public static void Main() {
      Run<GraphAnalysisDemo>();
    }
  }
}
