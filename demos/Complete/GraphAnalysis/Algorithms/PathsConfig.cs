/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using Paths = yWorks.Analysis.Paths;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{
  /// <summary>
  /// Configuration which solves several path problems.
  /// </summary>
  public class PathsConfig : AlgorithmConfiguration
  {
    private readonly PathsMode algorithmType;
    private List<INode> markedSources;
    private List<INode> markedTargets;

    public PathsConfig(PathsMode algorithmType) {
      this.algorithmType = algorithmType;
    }

    public override bool SupportsDirectedAndUndirected {
      get { return true; }
    }

    public override bool SupportsWeights {
      get {
        return algorithmType == PathsMode.ShortestPaths ||
               algorithmType == PathsMode.SingleSource;
      }
    }

    public override void RunAlgorithm(IGraph graph) {
      // reset the graph to remove all previous markings
      ResetGraph(graph);

      if (graph.Nodes.Any()) {
        if (markedSources == null || !NodesInGraph(markedSources, graph)) {
          // choose a source for the path if there isn't one already
          markedSources = new List<INode> {graph.Nodes.First()};
        }
        if (markedTargets == null || !NodesInGraph(markedTargets, graph)) {
          // choose a target for the path if there isn't one already
          markedTargets = new List<INode> {graph.Nodes.Last()};
        }

        // apply one of the path algorithms
        switch (algorithmType) {
          case PathsMode.ShortestPaths:
            CalculateShortestPath(markedSources, markedTargets, graph);
            break;
          case PathsMode.AllPaths:
            CalculateAllPaths(markedSources[0], markedTargets[0], graph);
            break;
          case PathsMode.AllChains:
            CalculateAllChains(graph);
            break;
          case PathsMode.SingleSource:
            markedTargets.Clear();
            CalculateSingleSource(graph, markedSources[0]);
            break;
          default:
            CalculateShortestPath(markedSources, markedTargets, graph);
            break;
        }
      }
    }

    private void CalculateShortestPath(List<INode> sources, List<INode> targets, IGraph graph) {
      if (sources != null && targets != null) {

        // run algorithm
        var result = new AllPairsShortestPaths {
            Directed = Directed,
            Costs = {Delegate = GetEdgeWeight},
            Sources = {Source = sources},
            Sinks = {Source = targets}
        }.Run(graph);

        // mark the resulting paths
        MarkPaths(result.Paths, sources, targets);
      }
    }

    private void CalculateAllPaths(INode source, INode target, IGraph graph) {
      if (source != null && target != null) {

        // run algorithm
        var result = new Paths {
          Directed = this.Directed,
          StartNodes = {Item = source},
          EndNodes = {Item = target}
        }.Run(graph);
        // mark the resulting paths
        MarkPaths(result.Paths, markedSources, markedTargets);
      }
    }

    private void CalculateAllChains(IGraph graph) {
      // add resulting edges to set
      var result = new Chains {
          Directed = Directed
      }.Run(graph);
      // mark the resulting paths
      MarkPaths(result.Chains, markedSources, markedTargets);
    }

    private void CalculateSingleSource(IGraph graph, INode source) {
      // there cannot be any negative cost cycles because of the implementation of GetEdgeWeight
      var result = new SingleSourceShortestPaths {
          Source = {Item = source},
          Sinks = {Source = graph.Nodes},
          Directed = Directed,
          Costs = {Delegate = GetEdgeWeight}
      }.Run(graph);

      // add a gradient to indicate the distance of the nodes to the source
      var maxDistance = result.Distances.Values.Where(dist => !double.IsPositiveInfinity(dist)).Max();

      foreach (var node in graph.Nodes) {
        var v = result.Distances[node] / maxDistance;
        node.Tag = new Tag {GradientValue = v};
        var edgeToNode = result.Predecessors[node];
        if (edgeToNode != null) {
          edgeToNode.Tag = new Tag {GradientValue = v};
        }
      }

      ((Tag) source.Tag).IsSource = true;
    }

    /// <summary>
    /// Marks the given paths.
    /// </summary>
    /// <remarks>
    /// Also highlights the source and target nodes.
    /// </remarks>
    private void MarkPaths(ResultItemCollection<Path> paths, List<INode> sources, List<INode> targets) {
      // Mapping from items to their list of color groups
      var item2ColorGroups = new Dictionary<IModelItem, List<ColorGroup>>();

      // Prepare the item to color groups mapping
      int i = 0;
      foreach (var path in paths) {
        var group = new ColorGroup(i++);

        foreach (var item in path.Nodes.Cast<IModelItem>().Concat(path.Edges)) {
          List<ColorGroup> list;
          if (!item2ColorGroups.TryGetValue(item, out list)) {
            list = new List<ColorGroup>();
            item2ColorGroups[item] = list;
          }
          list.Add(group);
        }
      }

      // Set the initial tags for all paths
      foreach (var pair in item2ColorGroups) {
        pair.Key.Tag = new Tag {
            ColorGroups = pair.Value,
            CurrentColor = pair.Value.First().Color
        };
      }
      MarkSourceAndTargetNodes(sources, targets);
    }

    /// <summary>
    /// Marks the source and target nodes.
    /// </summary>
    private void MarkSourceAndTargetNodes(List<INode> sources, List<INode> targets) {
      if (algorithmType == PathsMode.AllChains) {
        return;
      }
      // mark source and target of the paths
      foreach (var node in sources) {
        ((Tag) node.Tag).IsSource = true;
      }
      if (algorithmType == PathsMode.SingleSource) {
        return;
      }
      foreach (var node in targets) {
        ((Tag) node.Tag).IsTarget = true;
      }
    }

    /// <summary>
    /// Supports a custom context menu.
    /// </summary>
    public override void PopulateContextMenu(PopulateItemContextMenuEventArgs<IModelItem> args) {
      var graph = args.Context.GetGraph();
      var contextMenu = args.Menu;
      var graphComponent = (GraphControl) args.Context.CanvasControl;
      var item = args.Item as INode;
      if (algorithmType == PathsMode.ShortestPaths) {
        if (item != null) {
          UpdateSelection(item, graphComponent);
        }
        var selectedNodes = graphComponent.Selection.SelectedNodes;
        if (selectedNodes.Count > 0) {
          var markAsSource = new MenuItem {Header = "Mark as Source"};
          markAsSource.Click += (sender, eventArgs) => {
            markedSources = selectedNodes.ToList();
            RunAlgorithm(graph);
          };
          contextMenu.Items.Add(markAsSource);
          var markAsTarget = new MenuItem { Header = "Mark as Target" };
          markAsTarget.Click += (sender, eventArgs) => {
            markedTargets = selectedNodes.ToList();
            RunAlgorithm(graph);
          };
          contextMenu.Items.Add(markAsTarget);
        }
      } else if (algorithmType != PathsMode.AllChains) {
        if (item != null) {
          var markAsSource = new MenuItem { Header = "Mark as Source" };
          markAsSource.Click += (sender, eventArgs) => {
            markedSources = new []{item}.ToList();
            RunAlgorithm(graph);
          };
          contextMenu.Items.Add(markAsSource);
          if (algorithmType != PathsMode.SingleSource) {
            var markAsTarget = new MenuItem { Header = "Mark as Target" };
            markAsTarget.Click += (sender, eventArgs) => {
              markedTargets = new[] { item }.ToList();
              RunAlgorithm(graph);
            };
            contextMenu.Items.Add(markAsTarget);
          }
        }
      }
    }

    private void UpdateSelection(INode node, GraphControl graphControl) {
      if (node == null) {
        graphControl.Selection.Clear();
      } else if (!graphControl.Selection.IsSelected(node)) {
        graphControl.Selection.Clear();
        graphControl.Selection.SetSelected(node, true);
        graphControl.CurrentItem = node;
      }
    }

    private bool NodesInGraph(IEnumerable<INode> nodes, IGraph graph) {
      foreach (var node in nodes) {
        if (!graph.Contains(node)) {
          return false;
        }
      }
      return true;
    }
  }

  /// <summary>
  /// Defines the path problem to solve.
  /// </summary>
  public enum PathsMode
  {
    /// <summary>
    /// Calculates the shortest path between one or more source nodes and one or more target nodes.
    /// </summary>
    ShortestPaths,
    /// <summary>
    /// Calculates all paths between a source and a target node.
    /// </summary>
    AllPaths,
    AllChains,
    SingleSource
  }
}
