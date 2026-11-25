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
using yWorks.Analysis;
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Grid;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.LayoutWithoutView
{
  [Demo("without-view", "layout", "Layout Without View",
      Summary = "Shows how to use the graph analysis and layout algorithms without a view and without the IGraph API",
      Tags = ["headless", "layout", "analysis"],
      Keywords = ["invisible", "background", "memory", "centrality", "hierarchical"], RelatedDemos = [])]
  public sealed partial class LayoutWithoutViewDemo
  {
    protected override void Initialize() {
      Run();
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<LayoutWithoutViewDemo>();
    }

    public LayoutWithoutViewDemo() {
      InitializeComponent();
    }

    private void Run() {
      // create the graph in memory
      var layoutGraph = new LayoutGraph();

      // define some convenience methods to create elements
      Node CreateNode(double x, double y, double width, double height) =>
          layoutGraph.CreateNode(new RectD(x, y, width, height));

      // build the graph
      var node1 = CreateNode(0, 0, 30, 30);
      layoutGraph.AddLabel(node1, new OrientedRectangle(0, 0, 50, 10));

      var node2 = CreateNode(150, 0, 30, 30);
      layoutGraph.AddLabel(node2, new OrientedRectangle(0, 0, 50, 10));

      var node3 = CreateNode(100, 50, 30, 30);

      var edge = layoutGraph.CreateEdge(node1, node3);
      layoutGraph.AddBend(edge, 50, 20);

      layoutGraph.CreateEdge(node2, node3);
      layoutGraph.AddLabel(layoutGraph.CreateEdge(node3, node1), new OrientedRectangle(0, 0, 60, 20));

      Log("Graph dump before algorithm");
      LogGraph(layoutGraph);

      var centralNode = RunAlgorithm(layoutGraph);
      RunLayout(layoutGraph, centralNode);

      Log("Graph dump after analysis and layout");
      LogGraph(layoutGraph);
    }

    private Node RunAlgorithm(LayoutGraph graph) {
      // create data storage
      var edgeCosts = graph.CreateEdgeDataMap<double>();

      // assign some arbitrary costs
      var edgeCount = graph.Edges.Count;
      var i = 0;
      foreach (var edge in graph.Edges) {
        edgeCosts[edge] = (double) i++ / edgeCount;
      }

      // run the algorithm
      var closenessResult = LayoutGraphAlgorithms.ClosenessCentrality(graph, true, edgeCosts);

      Log("Centrality values");
      foreach (var node in graph.Nodes) {
        Log($" node {node.Index} : {closenessResult[node]}");
      }

      // find the most central node
      var centralNode = graph.Nodes.Select(node => (node, centrality: closenessResult[node]))
          .Aggregate((a, b) => a.centrality > b.centrality ? a : b).node;

      // release resources
      graph.DisposeDataMap(edgeCosts);
      graph.DisposeDataMap(closenessResult);

      Log();

      return centralNode;
    }

    private void RunLayout(LayoutGraph layoutGraph, Node centralNode) {
      // create and configure the layout
      var layout = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.TopToBottom,
          StopDuration = TimeSpan.FromMilliseconds(500)
      };
      layout.DefaultEdgeDescriptor.BackLoopRouting = true;

      // create the partition
      var layoutGrid = new LayoutGrid(2, 1, 0, 5, 30, 30);
      var layoutData = layout.CreateLayoutData(layoutGraph);
      // assign the central node to its own row
      layoutData.LayoutGridData.LayoutGridCellDescriptors.MapperFunction =
          node => layoutGrid.CreateRowSpanDescriptor(node == centralNode ? 0 : 1);

      // and run it
      layoutGraph.ApplyLayout(layout, layoutData);

      var firstRow = layoutGrid.Rows[0];
      var second = layoutGrid.Rows[^1];
      Log("Layout Grid results");
      Log($"First row ({firstRow.Index}): {firstRow.Position}, {firstRow.Height}");
      Log($"Second row ({second.Index}): {second.Position}, {second.Height}");
      Log();
    }

    private void LogGraph(LayoutGraph layoutGraph) {
      void LogOrientedBox(IOrientedRectangle orientedBox) {
        string rotation;
        if (orientedBox.UpY != -1) {
          // rotated
          rotation = $"{orientedBox.UpX}, {orientedBox.UpY}";
        } else {
          rotation = "";
        }
        Log($"    {orientedBox.AnchorX}, {orientedBox.AnchorY}, {orientedBox.Width}, {orientedBox.Height}  {rotation}");
      }

      foreach (var node in layoutGraph.Nodes) {
        Log($"node {node.Index}");
        Log($" layout {node.Layout.X}, {node.Layout.Y}, {node.Layout.Width}, {node.Layout.Height}");

        if (node.Labels.Any()) {
          Log(" labels");
          foreach (var label in node.Labels) {
            LogOrientedBox(label.Layout);
          }
        }
      }

      foreach (var edge in layoutGraph.Edges) {
        Log($"edge {edge.Index}  ({edge.Source.Index} -> {edge.Target.Index})");
        var sourcePortLocation = new PointD(edge.Source.Layout.Center.X - edge.SourcePortLocation.X,
            edge.Source.Layout.Center.Y - edge.SourcePortLocation.Y);
        Log($"  source port offset : {sourcePortLocation.X}, {sourcePortLocation.Y}");
        var targetPortLocation = new PointD(edge.Target.Layout.Center.X - edge.TargetPortLocation.X,
            edge.Target.Layout.Center.Y - edge.TargetPortLocation.Y);
        Log($"  target port offset : {targetPortLocation.X}, {targetPortLocation.Y}");
        var bendIndex = 0;
        foreach (var bend in edge.Bends) {
          Log($"  bend {bendIndex++}: {bend.Location.X}, {bend.Location.Y}");
        }
        if (edge.Labels.Any()) {
          Log("  labels");
          foreach (var label in edge.Labels) {
            LogOrientedBox(label.Layout);
          }
        }
      }
      Log();
    }

    /* Helper element and function to log to the text area */
    private void Log(string? value = null) {
      log.AppendText($"{value ?? string.Empty}{Environment.NewLine}");
    }
  }
}
