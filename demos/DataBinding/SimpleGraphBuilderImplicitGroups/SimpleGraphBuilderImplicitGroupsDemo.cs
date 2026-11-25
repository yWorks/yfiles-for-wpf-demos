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

namespace yWorks.Demo.SimpleGraphBuilderImplicitGroups
{
  [Demo("simple-graph-builder-implicit-groups", "data-binding", "Simple Graph Builder With Implicit Groups",
      Summary = "Automatically builds a graph from JSON business data with implicit groups using GraphBuilder.",
      Tags = ["json", "data binding", "layout"], Keywords = ["hierarchical", "labels", "playground"],
      RelatedDemos = [])]
  public sealed class SimpleGraphBuilderImplicitGroupsDemo : SimpleDemo
  {
    protected override async void Initialize() {
      // use the viewer input mode since this demo should not allow interactive graph editing
      GraphControl.InputMode = new GraphViewerInputMode();

      Graph.NodeDefaults.Size = new SizeD(260, 60);

      var graphData = ReadGraphData();

      // create the graph builder
      var graphBuilder = new GraphBuilder(Graph);

      // in this sample, the top-level objects are the groups that have
      // a list of children (employees) as well as an additional location
      // attribute which specifies the group's parent node
      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodesSource, group => group.Id);

      // the children of each group are defined directly in the data
      var childSource = nodesSource.CreateChildNodesSource(
          // specify how to retrieve the children of each group
          group => group.Members,
          // specify how the child nodes are identified globally
          employee => employee.Id);
      // the groups are additionally grouped again by location
      var parentSource = nodesSource.CreateParentNodesSource(group => group.Location);

      // configure the labels for the groups
      nodesSource.NodeCreator.CreateLabelBinding(group => group.Id);
      // configure the labels for the child nodes
      childSource.NodeCreator.CreateLabelBinding(employee => $"{employee.Name}\n{employee.Position}");
      // configure the labels for the location nodes
      parentSource.NodeCreator.CreateLabelBinding(location => location);

      // set up reasonable defaults for the styles.
      // since the entities in the nodesSource and the parentsSource are both group nodes,
      // both use a group node style
      nodesSource.NodeCreator.Defaults.Style = parentSource.NodeCreator.Defaults.Style = Graph.GroupNodeDefaults.Style;
      // use the group node label defaults for the group nodes
      nodesSource.NodeCreator.Defaults.Labels = parentSource.NodeCreator.Defaults.Labels =
          Graph.GroupNodeDefaults.Labels;
      // the nodes in the childSource are styled with the node default
      childSource.NodeCreator.Defaults.Style = Graph.NodeDefaults.Style;
      childSource.NodeCreator.Defaults.Labels = Graph.NodeDefaults.Labels;

      // create a source for the edges of the graph
      graphBuilder.CreateEdgesSource(graphData.EdgesSource,
          // specify the property of an edge object that contains the source node's id
          edge => edge.FromNode,
          // specify the property of an edge object that contains the target node's id
          edge => edge.ToNode);

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
    public static void Main() => Run<SimpleGraphBuilderImplicitGroupsDemo>();
  }

  public sealed class GraphData
  {
    public List<GroupData> NodesSource { get; set; } = [];

    public List<EdgeData> EdgesSource { get; set; } = [];
  }

  public sealed class GroupData
  {
    public string Id { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public List<EmployeeData> Members { get; set; } = [];
  }

  public sealed class EmployeeData
  {
    public int Id { get; set; }

    public string Position { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
  }

  public sealed class EdgeData
  {
    public int FromNode { get; set; }

    public int ToNode { get; set; }
  }
}
