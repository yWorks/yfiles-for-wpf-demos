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

using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step12GraphAnalysis
{
  /// <summary>
  ///   Tutorial step 12: Graph Analysis.
  /// </summary>
  /// <remarks>
  ///   Shows how to run reachability and shortest path analyses and highlight their results.
  /// </remarks>
  public class GraphAnalysisStep : TutorialStepBase
  {
    public override string Title => "12 Graph Analysis";

    public override string DescriptionSource => "Step12GraphAnalysis.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      SetDefaultLabelLayoutParameters(graph);
      ConfigureHighlights(graphControl);
      // Create a GraphEditorInputMode and register it as the main input mode for the GraphControl.
      graphControl.InputMode = new GraphEditorInputMode();

      CreateSampleGraph(graph);

      var initialSelection = graph.Nodes.FirstOrDefault(node =>
          !graph.IsGroupNode(node) && node.Labels.FirstOrDefault()?.Text == "15");
      if (initialSelection != null) {
        graphControl.Selection.Nodes.Add(initialSelection);
      }

      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      AddButtonListener(description, "runReachabilityButton", () => RunReachability(graphControl));
      AddButtonListener(description, "runShortestPathButton", () => RunShortestPath(graphControl));
    }

    /// <summary>Runs a directed reachability analysis from the selected nodes.</summary>
    private static void RunReachability(GraphControl graphControl) {
      var graph = graphControl.Graph;
      var nodes = graph.Nodes.Where(node => !graph.IsGroupNode(node)).ToArray();
      if (nodes.Length == 0) {
        return;
      }

      // First reset the highlights.
      graphControl.Highlights.Clear();

      var selection = graphControl.Selection;
      if (selection.Nodes.Count == 0) {
        selection.Clear();
        selection.Nodes.Add(nodes[0]);
      }

      // Configure and run the algorithm using the selected nodes as start nodes.
      var reachability = new Reachability { Directed = true }; // Consider edge direction
      reachability.StartNodes.Source = selection.Nodes;
      var result = reachability.Run(graph);

      // Highlight all reachable nodes.
      foreach (var node in result.ReachableNodes) {
        graphControl.Highlights.Add(node);
      }
    }

    /// <summary>Runs an undirected shortest path analysis between the selected nodes.</summary>
    private static void RunShortestPath(GraphControl graphControl) {
      // First reset the highlights.
      graphControl.Highlights.Clear();

      var graph = graphControl.Graph;
      var nodes = graph.Nodes.Where(node => !graph.IsGroupNode(node)).ToList();
      if (nodes.Count < 2) {
        return;
      }

      var selection = graphControl.Selection;
      var selectedNodes = selection.Nodes.ToList();
      // Select source and sink node for the path-finding algorithm.
      // Use the first two selected nodes as source and sink if possible.
      var sourceNode = selectedNodes.FirstOrDefault() ?? nodes[0];
      var sinkNode = selectedNodes.Count > 1 ? selectedNodes[1] : nodes[^1];
      selection.Clear();
      selection.Nodes.Add(sourceNode);
      selection.Nodes.Add(sinkNode);

      // Find the shortest path without considering edge direction.
      var shortestPath = new ShortestPath { Directed = false };
      shortestPath.Source.Item = sourceNode;
      shortestPath.Sink.Item = sinkNode;
      // Calculate the cost per edge as the distance between its source and target nodes.
      shortestPath.Costs.MapperFunction = edge =>
          (edge.GetSourceNode().Layout.GetCenter() - edge.GetTargetNode().Layout.GetCenter()).VectorLength;

      var shortestPathResult = shortestPath.Run(graph);

      var pathDistance = shortestPathResult.Distance;
      var pathNodes = shortestPathResult.Path!.Nodes;
      var endNode = shortestPathResult.Path.End;
      var pathEdges = shortestPathResult.Edges;

      if (!double.IsFinite(pathDistance)) {
        return;
      }

      foreach (var node in pathNodes) {
        graphControl.Highlights.Add(node);
      }
      // Highlight all edges that belong to the calculated path.
      foreach (var edge in pathEdges) {
        graphControl.Highlights.Add(edge);
      }
    }

    /// <summary>Creates the sample graph from its embedded JSON resource.</summary>
    private void CreateSampleGraph(IGraph graph) {
      BuildGraphFromJson(graph, "Step12GraphAnalysis.graph-data.json");
    }

    private static void SetDefaultLabelLayoutParameters(IGraph graph) {
      // Place node labels in the node center.
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;

      // Use a rotated layout for edge labels.
      var edgeLabelModel = new SmartEdgeLabelModel { AutoRotation = true };
      graph.EdgeDefaults.Labels.LayoutParameter = edgeLabelModel.CreateParameterFromSource(0, 10.0);
    }

    private static void ConfigureHighlights(GraphControl graphControl) {
      var selection = graphControl.Selection;
      selection.ItemAdded += (_, _) => graphControl.Highlights.Clear();
      selection.ItemRemoved += (_, _) => graphControl.Highlights.Clear();

      var decorator = graphControl.Graph.GetDecorator();
      decorator.Nodes.HighlightRenderer.AddConstant(new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle { Brush = Brushes.Transparent, Pen = new Pen(Brushes.Orange, 3) }
      });
      decorator.Edges.HighlightRenderer.AddConstant(new EdgeStyleIndicatorRenderer {
          EdgeStyle = new PolylineEdgeStyle { Pen = new Pen(Brushes.Orange, 3) }
      });
    }
  }
}
