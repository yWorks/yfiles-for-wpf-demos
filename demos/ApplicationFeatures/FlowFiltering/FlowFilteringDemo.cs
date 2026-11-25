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
using System.Windows.Media;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.FlowFiltering
{
  [Demo("flow-filtering", "application-features", "Flow Filtering",
      Summary = "Shows how to hide/show items within the connecting upstream/downstream flow.",
      Tags = ["highlight", "filter", "flow"], Keywords = [
          "parent", "child", "predecessor", "successor", "neighbor", "neighbour", "hide", "collapse", "interaction",
          "playground"
      ], RelatedDemos = [
          "filtering", "simple-highlight", "collapsible-trees"
      ])]
  public sealed class FlowFilteringDemo : SimpleDemo
  {
    // Define the default flow direction
    private TraversalDirection traversalDirection = TraversalDirection.Successor;
    private FilteredGraphWrapper filteredGraph = null!;

    protected override void Initialize() {
      // --- Flow Filtering ---

      // Populate the graph with data from the JSON dataset
      BuildGraphFromJson("graph-data.json", typeof(NodeTag));

      // Predicate to determine which items to hide
      Predicate<INode> nodePredicate = node => GraphData.GetTag(node)?.Filtered != true;

      // Create a filtered graph which wraps the original graph and applies the predicate
      filteredGraph = new FilteredGraphWrapper(Graph, nodePredicate);

      // Assign the filtered graph to the graph component for display
      GraphControl.Graph = filteredGraph;

      // Hide flow items on node selection
      GraphControl.Selection.ItemAdded += (_, args) => {
        if (args.Item is INode node) {
          ToggleNeighboursVisibility(node, false);
        }
      };

      // Show flow items again when unselected
      GraphControl.Selection.ItemRemoved += (_, args) => {
        if (args.Item is INode node) {
          ToggleNeighboursVisibility(node, true);
        }
      };

      // --- Flow Highlighting ---

      // Configure the input mode for this graph
      var graphViewerInputMode = new GraphViewerInputMode();
      GraphControl.InputMode = graphViewerInputMode;

      // Enable hover effects on nodes
      graphViewerInputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Node;

      // Specify hover effect: add the hovered node and its flow items to the graph's highlight collection
      graphViewerInputMode.ItemHoverInputMode.HoveredItemChanged += (_, args) => {
        GraphControl.Highlights.Clear();
        if (args.Item is INode node) {
          // Highlight the hovered node
          GraphControl.Highlights.Add(node);
          // Highlight the neighbors of the hovered node
          foreach (var neighbor in GetNeighbors(node, filteredGraph).Neighbors) {
            GraphControl.Highlights.Add(neighbor);
          }
        }
      };

      // Define the visual style for highlighted nodes
      Graph.GetDecorator().Nodes.HighlightRenderer.AddConstant(new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.RoundRectangle,
              Brush = new SolidColorBrush(Color.FromRgb(255, 108, 0)),
              Pen = null
          }
      });

      // --- Graph Initialization ---

      // Apply layout and fit the graph bounds
      Graph.ApplyLayout(new HierarchicalLayout());
      GraphControl.FitGraphBounds();

      // Add the flow direction dropdown menu
      ToolBar.AddComboBox("Flow Direction: ", [
          (TraversalDirection.Predecessor, "Upstream"), (TraversalDirection.Successor, "Downstream"),
          (TraversalDirection.Both, "Both Directions"), (TraversalDirection.Undirected, "All Connected")
      ], SetFlowDirection, TraversalDirection.Successor);

      ToolBar.AddButton("Reset Graph", ResetGraph);
    }

    // Use the neighborhood algorithm to obtain neighbors of a given node
    private Neighborhood.Result GetNeighbors(INode node, IGraph graph) {
      var neighborhoodAlgorithm = new Neighborhood {
          StartNodes = { Item = node },
          TraversalDirection = traversalDirection
      };
      return neighborhoodAlgorithm.Run(graph);
    }

    /// <summary>
    ///   Toggles the visibility of items within the connecting flow of the given node.
    /// </summary>
    private void ToggleNeighboursVisibility(INode node, bool visible) {
      // Get the neighbors of the clicked node
      var neighbors = GetNeighbors(node, filteredGraph.WrappedGraph!);
      // Toggle the visibility of each neighbor
      foreach (var neighbor in neighbors.Neighbors) {
        GraphData.SetTag(neighbor, new NodeTag { Filtered = !visible });
      }

      // Notify the filtered graph to re-evaluate its predicates and update the view
      filteredGraph.NodePredicateChanged();
    }

    private void SetFlowDirection(TraversalDirection direction) {
      traversalDirection = direction;
      ResetGraph();
      var selectedNode = GraphControl.Selection.Nodes.FirstOrDefault();
      if (selectedNode != null) {
        ToggleNeighboursVisibility(selectedNode, false);
      }
    }

    private void ResetGraph() {
      // Unfilter all nodes
      foreach (var node in filteredGraph.WrappedGraph!.Nodes) {
        GraphData.SetTag(node, new NodeTag { Filtered = false });
      }

      // Notify the filtered graph to re-evaluate its predicates and update the view
      filteredGraph.NodePredicateChanged();
    }

    [STAThread]
    private static void Main() {
      Run<FlowFilteringDemo>();
    }
  }
}
