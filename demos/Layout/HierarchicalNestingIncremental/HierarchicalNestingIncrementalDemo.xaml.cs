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
using System.Text.Json.Serialization;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalNestingIncremental
{
  /// <summary>
  ///   This demo shows how to nicely lay out the newly loaded nodes when expanding folded groups in a graph.
  /// </summary>
  [Demo("hierarchical-nesting-incremental", "layout", "Hierarchical Nesting (Incremental)",
      Summary = "Shows how to nicely lay out newly loaded nodes when expanding folded groups.",
      Tags = ["layout", "hierarchical", "incremental"],
      Keywords = [
          "overview", "fold", "hide", "group", "dynamic", "load", "collapse", "from sketch", "interactive", "data",
          "padding"
      ])]
  public sealed partial class HierarchicalNestingIncrementalDemo
  {
    private GraphBuilder builder = null!;
    private NodesSource<NodeData> nodesSource = null!;
    private NodesSource<NodeData> groupNodesSource = null!;
    private GraphData graphData = null!;

    /// <summary>
    ///   Initializes folding, the overview, and the sample graph.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphViewerInputMode {
          FocusableItems = GraphItemTypes.None,
          SelectableItems = GraphItemTypes.None
      };

      // set up folding for this graph component
      var foldingManager = new FoldingManager();
      // collapse all folders in the beginning
      var foldingView = foldingManager.CreateFoldingView(isExpanded: _ => false);
      graphControl.Graph = foldingView.Graph;

      // managing the appearance of folder nodes
      foldingManager.FolderNodeConverter = new FolderNodeConverter {
          FolderNodeDefaults = {
              CopyLabels = true,
              Size = new SizeD(110, 60)
          }
      };

      // load the sample graph
      LoadSampleGraph(foldingManager);

      // set up the layout which is automatically applied when a group node is collapsed/expanded
      InteractiveHierarchicalNestingLayout.Initialize(graphControl, NodeAlignmentPolicy.TopRight,
          AddGroupDescendantsToGraph);
    }

    /// <summary>
    ///   Loads the sample graph from JSON data and applies an initial layout.
    /// </summary>
    private void LoadSampleGraph(FoldingManager foldingManager) {
      graphData = JsonSerializer.Deserialize<GraphData>(LoadAssemblyResource("graph-data.json")) ??
          throw new InvalidOperationException("The graph data resource is empty.");

      // use the default demo styles for the sample graph items
      DemoStyles.InitDemoStyles(foldingManager.MasterGraph, foldingEnabled: true);

      // use the main graph to build the unfolded graph from the data
      var mainGraph = foldingManager.MasterGraph;
      builder = new GraphBuilder(mainGraph);
      nodesSource = builder.CreateNodesSource(graphData.Nodes.Where(node => !node.IsGroup), node => node.Id);
      nodesSource.ParentIdProvider = node => node.Parent;
      nodesSource.NodeCreator.TagProvider = node => node;

      groupNodesSource = builder.CreateGroupNodesSource(graphData.Nodes.Where(node => node.IsGroup), group => group.Id);
      groupNodesSource.ParentIdProvider = group => group.Parent;
      groupNodesSource.NodeCreator.TagProvider = group => group;
      groupNodesSource.NodeCreator.CreateLabelBinding(group => group.Labels);

      builder.CreateEdgesSource(graphData.Edges, edge => edge.Source, edge => edge.Target);
      builder.BuildGraph();

      // no layout information available, yet
      // so we come up with an initial layout for the complete, expanded graph.
      graphControl.Graph.ApplyLayout(new HierarchicalLayout());

      // center the arranged graph in the visible area
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Retrieves data of all child nodes from a group node and adds them to the graph using the graph builder.
    /// </summary>
    private void AddGroupDescendantsToGraph(INode groupNode) {
      if (groupNode.Tag is not NodeData groupData) {
        return;
      }

      // append all child nodes of the given group node to the current nodes in graph
      builder.SetData(nodesSource,
          graphData.Nodes.Where(node => !node.IsGroup).Where(node => node.Parent == groupData.Id).Concat(graphControl
              .Graph.Nodes.Select(node => node.Tag).OfType<NodeData>().Where(node => node.Labels is null)));
      // append all child group nodes of the given group node to the current group nodes in graph
      builder.SetData(groupNodesSource,
          graphData.Nodes.Where(node => node.IsGroup).Where(group => group.Parent == groupData.Id).Concat(graphControl
              .Graph.Nodes.Select(node => node.Tag).OfType<NodeData>().Where(node => node.Labels is not null)));
      // update the graph
      builder.UpdateGraph();
    }

    private sealed class GraphData
    {
      [JsonPropertyName("nodes")]
      public List<NodeData> Nodes { get; init; } = [];

      [JsonPropertyName("edges")]
      public List<EdgeData> Edges { get; init; } = [];
    }

    private sealed class NodeData
    {
      [JsonPropertyName("id")]
      public string Id { get; } = "";

      [JsonPropertyName("parent")]
      public string? Parent { get; init; }

      [JsonPropertyName("labels")]
      public string? Labels { get; init; }

      [JsonPropertyName("isGroup")]
      public bool IsGroup { get; init; }
    }

    private sealed class EdgeData
    {
      [JsonPropertyName("source")]
      public string Source { get; } = "";

      [JsonPropertyName("target")]
      public string Target { get; } = "";
    }

    [STAThread]
    private static void Main(string[] args) => Run<HierarchicalNestingIncrementalDemo>();

    public HierarchicalNestingIncrementalDemo() {
      InitializeComponent();
    }
  }
}
