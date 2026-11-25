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

namespace yWorks.Demo.SimpleGraphBuilder
{
  [Demo("simple-graph-builder", "data-binding", "Simple Graph Builder",
      Summary = "Automatically builds a graph from JSON business data using GraphBuilder.",
      Tags = ["json", "data binding", "layout"], Keywords = ["hierarchical", "labels", "playground"],
      RelatedDemos = [])]
  public sealed class SimpleGraphBuilderDemo : SimpleDemo
  {
    protected override async void Initialize() {
      // use the viewer input mode since this demo should not allow interactive graph editing
      GraphControl.InputMode = new GraphViewerInputMode();

      Graph.NodeDefaults.Size = new SizeD(260, 60);

      var graphData = ReadGraphData();

      // create the graph builder
      var graphBuilder = new GraphBuilder(Graph);
      // create a source for the nodes of the graph
      // specify the id property of a node object
      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodesSource, nodeData => nodeData.Id);
      // specify the property of a node object that contains its parent group id
      nodesSource.ParentIdProvider = nodeData => nodeData.Group;
      // configure the label binding for the nodes
      nodesSource.NodeCreator.CreateLabelBinding(nodeData => $"{nodeData.Name}\n{nodeData.Position}");

      // create a source for the group nodes of the graph
      // this sample provides explicit objects for the groups
      // specify the id property of a group node object
      var groupsSource = graphBuilder.CreateGroupNodesSource(graphData.GroupsSource, groupData => groupData.Id);
      // specify the property of a group node object that contains its parent group id
      groupsSource.ParentIdProvider = groupData => groupData.ParentGroup!;
      // configure the label binding for the group nodes
      groupsSource.NodeCreator.CreateLabelBinding(groupData => groupData.Id);

      // create a source for the edges of the graph
      graphBuilder.CreateEdgesSource(
          // stores the edges of the graph
          graphData.EdgesSource,
          // specify the property of an edge object that contains the source node's id
          edgeData => edgeData.FromNode,
          // specify the property of an edge object that contains the target node's id
          edgeData => edgeData.ToNode);

      // build the graph from the data
      graphBuilder.BuildGraph();

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
    public static void Main() => Run<SimpleGraphBuilderDemo>();
  }

  public sealed class GraphData
  {
    public List<NodeData> NodesSource { get; set; } = [];

    public List<EdgeData> EdgesSource { get; set; } = [];

    public List<GroupData> GroupsSource { get; set; } = [];
  }

  public sealed class NodeData
  {
    public int Id { get; set; }

    public string Group { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
  }

  public sealed class EdgeData
  {
    public int FromNode { get; set; }

    public int ToNode { get; set; }
  }

  public sealed class GroupData
  {
    public string Id { get; set; } = string.Empty;

    public string? ParentGroup { get; set; }
  }
}
