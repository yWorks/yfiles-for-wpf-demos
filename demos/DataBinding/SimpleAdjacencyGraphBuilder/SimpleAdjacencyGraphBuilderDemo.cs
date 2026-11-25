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
using System.Text.Json;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SimpleAdjacencyGraphBuilder
{
  [Demo("simple-adjacency-graph-builder", "data-binding", "Simple Adjacency Graph Builder",
      Summary = "Automatically builds a graph from JSON business data using AdjacencyGraphBuilder.",
      Tags = ["json", "data binding", "layout"], Keywords = ["hierarchical", "labels", "playground"],
      RelatedDemos = [])]
  public sealed class SimpleAdjacencyGraphBuilderDemo : SimpleDemo
  {
    protected override async void Initialize() {
      // use the viewer input mode since this demo should not allow interactive graph editing
      GraphControl.InputMode = new GraphViewerInputMode();

      Graph.NodeDefaults.Size = new SizeD(260, 60);

      var graphData = ReadGraphData();

      // create the graph builder
      var adjacencyGraphBuilder = new AdjacencyGraphBuilder(Graph);
      // create a source for the nodes of the graph
      var adjacencyNodesSource = adjacencyGraphBuilder.CreateNodesSource(graphData.NodesSource);
      // configure the label binding for the nodes
      adjacencyNodesSource.NodeCreator.CreateLabelBinding(nodeData => $"{nodeData.Name}\n{nodeData.Position}");

      // configure the successor nodes
      adjacencyNodesSource.AddSuccessorsSource(nodeData => nodeData.Children, adjacencyNodesSource,
          new EdgeCreator<NodeData> { Defaults = Graph.EdgeDefaults });

      // build the graph from the data
      adjacencyGraphBuilder.BuildGraph();

      // make sure the graph is centered in the view before arranging it
      await GraphControl.FitGraphBounds();

      // configure a layout algorithm
      var algorithm = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.LeftToRight };

      // arrange the graph with the chosen layout algorithm
      await GraphControl.ApplyLayoutAnimated(algorithm, TimeSpan.FromSeconds(1));
    }

    private GraphData ReadGraphData() {
      using var jsonStream = LoadAssemblyResource("graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(jsonStream,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    [STAThread]
    public static void Main() => Run<SimpleAdjacencyGraphBuilderDemo>();
  }

  public sealed class GraphData
  {
    public List<NodeData> NodesSource { get; set; } = [];
  }

  public sealed class NodeData
  {
    public string Position { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<NodeData> Children { get; set; } = [];
  }
}
