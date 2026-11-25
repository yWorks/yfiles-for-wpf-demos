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
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Utils;

namespace yWorks.Demo.HierarchicalNesting
{
  /// <summary>
  ///   A helper class that stores geometrical information before a group node is expanded/collapsed and
  ///   uses that information to apply an incremental layout after the group node is expanded/collapsed.
  /// </summary>
  internal sealed class InteractiveHierarchicalNestingLayout
  {
    private readonly GraphControl graphControl;
    private INode? changedGroupNode;
    private readonly Dictionary<INode, RectD> alternativeGroupBounds = new();
    private readonly Dictionary<IEdge, List<IPoint>> alternativeEdgePaths = new();

    /// <summary>
    ///   Creates and initializes a new instance.
    /// </summary>
    /// <param name="graphControl">The current graph control.</param>
    private InteractiveHierarchicalNestingLayout(GraphControl graphControl) {
      this.graphControl = graphControl;
    }

    /// <summary>
    ///   Initializes the incremental layout that is automatically applied when a group node is collapsed/expanded.
    ///   This function registers the necessary listeners to the input mode and stores all the necessary information
    ///   to keep the layout as stable as possible.
    /// </summary>
    /// <param name="graphControl">The graph control that displays the graph.</param>
    /// <param name="nodeAlignmentPolicy">The group node alignment policy used when collapsing and expanding groups.</param>
    public static void Initialize(
        GraphControl graphControl,
        NodeAlignmentPolicy nodeAlignmentPolicy = NodeAlignmentPolicy.TopRight) {
      var layoutHelper = new InteractiveHierarchicalNestingLayout(graphControl);

      // configure the input mode and set a group node alignment policy that makes sure that the
      // expand/collapse button of the current group node keeps its location
      // note that the corresponding 'anchoring policy' is used for the LayoutAnchoringStage in
      // the incremental layout calculation.
      var inputMode = graphControl.InputMode as GraphViewerInputMode ?? throw new InvalidOperationException();
      var navigationInputMode = inputMode.NavigationInputMode ?? throw new InvalidOperationException();
      navigationInputMode.AutoGroupNodeAlignmentPolicy = nodeAlignmentPolicy;

      // enable folding commands
      navigationInputMode.AllowCollapseGroup = true;
      navigationInputMode.AllowExpandGroup = true;

      // disable FitContent because it interferes with our view port animation setup
      navigationInputMode.FitContentAfterGroupActions = false;

      // register a listener to store all relevant information about the graph
      // before expanding a group node
      navigationInputMode.GroupExpanding += (_, evt) => layoutHelper.BeforeExpandingGroup(evt.Item);
      // register a listener to store all relevant information about the graph
      // before collapsing a group node
      navigationInputMode.GroupCollapsing += (_, evt) => layoutHelper.BeforeCollapsingGroup(evt.Item);

      // register a listener to configure and run the layout with the previously collected information
      // after expanding a group node
      navigationInputMode.GroupExpanded += async (_, evt) => await layoutHelper.AfterGroupStateChanged(evt.Item);
      // register a listener to configure and run the layout with the previously collected information
      // after collapsing a group node
      navigationInputMode.GroupCollapsed += async (_, evt) => await layoutHelper.AfterGroupStateChanged(evt.Item);
    }

    /// <summary>
    ///   Stores information about the layout of a group before expanding the group.
    /// </summary>
    /// <param name="group">The group that will be expanded.</param>
    private void BeforeExpandingGroup(INode group) {
      BeforeGroupStateChanged(group, (graph, node) => graph.EdgesAt(node));
    }

    /// <summary>
    ///   Stores information about the layout of a group before collapsing the group.
    /// </summary>
    /// <param name="group">The group that will be collapsed.</param>
    private void BeforeCollapsingGroup(INode group) {
      BeforeGroupStateChanged(group, GetAffectedEdges);
    }

    /// <summary>
    ///   Stores information about the layout of a group before collapsing or expanding the group.
    /// </summary>
    /// <param name="group">The group that will be collapsed or expanded.</param>
    /// <param name="edgesToBackup">The edges whose paths should be stored as well.</param>
    private void BeforeGroupStateChanged(INode group, Func<IGraph, INode, IEnumerable<IEdge>> edgesToBackup) {
      var graph = graphControl.Graph;
      var foldingView = graph.GetFoldingView()!;
      var layout = group.Layout;

      // store the collapsed group node
      changedGroupNode = group;

      // store the group bounds of the collapsed group node before layout
      alternativeGroupBounds.Clear();
      alternativeGroupBounds[foldingView.GetMasterItem(group)!] = layout.ToRectD();

      // store all edge paths that connect to/into the collapsed group before layout
      alternativeEdgePaths.Clear();
      foreach (var edge in edgesToBackup(graph, group)) {
        alternativeEdgePaths[foldingView.GetMasterItem(edge)!] = GetPointList(edge);
      }
    }

    /// <summary>
    ///   Performs an incremental layout on the graph after a group was closed/expanded interactively.
    /// </summary>
    /// <param name="group">The group that was expanded or collapsed.</param>
    private async Task AfterGroupStateChanged(INode group) {
      // store the current locations of nodes and edges to keep them for incremental layout
      var graph = graphControl.Graph;
      var nodesCoordinates = new Dictionary<INode, IPoint>();
      var nodeSizes = new Dictionary<INode, ISize>();
      var edgesCoordinates = new Dictionary<IEdge, IEnumerable<IPoint>>();

      // we store the insets for each group node as we will change the size of the nodes to
      // animate the expand operation
      var padding = new Dictionary<INode, InsetsD>();
      foreach (var node in graph.Nodes) {
        var paddingProvider = node.Lookup<IGroupPaddingProvider>();
        if (paddingProvider != null) {
          padding[node] = paddingProvider.GetPadding();
        }
      }

      var groupingSupport = graph.GetGroupingSupport();
      if (graph.IsGroupNode(group)) {
        // reset the paths and the centers of the child nodes so that morphing looks smoother
        var descendants = groupingSupport.GetDescendants(group);
        var visitedEdges = new HashSet<IEdge>();
        foreach (var childNode in descendants) {
          foreach (var edge in graph.EdgesAt(childNode)) {
            // store path and clear bends afterwards
            if (visitedEdges.Add(edge)) {
              edgesCoordinates[edge] = GetPointList(edge);
              graph.ClearBends(edge);
            }
          }
          // store coordinates and center node afterwards
          var layout = childNode.Layout;
          var layoutRect = layout.ToRectD();
          nodesCoordinates[childNode] = layoutRect.GetTopLeft();
          nodeSizes[childNode] = layoutRect.GetSize();
          graph.SetNodeLayout(childNode, new RectD(group.Layout.ToRectD().GetCenter(), new SizeD(1, 1)));
        }
      }

      // reset the paths of the edges connected to the group node to get smoother layout transitions
      foreach (var edge in graph.EdgesAt(group)) {
        // store path and clear bends afterwards
        edgesCoordinates[edge] = GetPointList(edge);
        graph.ClearBends(edge);
      }

      // we register a new padding provider that holds the old padding of the group nodes, before the resizing
      var chainLink = graph.GetDecorator().Nodes.GroupPaddingProvider.AddFactory(node =>
          new GroupPaddingProvider(padding.TryGetValue(node, out var insets) ? insets : InsetsD.Empty));

      // run the incremental layout
      await ApplyIncrementalLayout(nodesCoordinates, nodeSizes, edgesCoordinates);

      // remove the insets provider
      graph.GetDecorator().Nodes.Remove(chainLink);
    }

    /// <summary>
    ///   Applies the incremental layout after each expanding and collapsing.
    /// </summary>
    /// <param name="nodesCoordinates">The coordinates of the nodes before the layout.</param>
    /// <param name="nodeSizes">The sizes of the nodes before the layout.</param>
    /// <param name="edgesCoordinates">The coordinates of the edges before the layout.</param>
    private async Task ApplyIncrementalLayout(
        Dictionary<INode, IPoint> nodesCoordinates,
        Dictionary<INode, ISize> nodeSizes,
        Dictionary<IEdge, IEnumerable<IPoint>> edgesCoordinates) {
      // configure hierarchical layout for a most stable outcome
      var layout = new HierarchicalLayout { FromSketchMode = true };
      layout.DefaultEdgeDescriptor.RecursiveEdgePolicy = RecursiveEdgePolicy.Directed;

      // the LayoutAnchoringStage is used to make sure that the expanded/collapsed group stays at their location
      // note that an input mode with the corresponding 'group node alignment policy' is used, too
      var anchorNodeLayout = new LayoutAnchoringStage(layout);

      var graph = graphControl.Graph;
      var foldingView = graph.GetFoldingView()!;

      // configure features of the layout algorithms that require node- and/or edge-specific information
      // such as alternative paths for specific edges or which node should stay fixed
      var hierarchicalLayoutData = layout.CreateLayoutData(graph);
      hierarchicalLayoutData.AlternativeGroupBounds.MapperFunction = node => {
        var masterNode = foldingView.GetMasterItem(node)!;
        return alternativeGroupBounds.TryGetValue(masterNode, out var bounds) ? bounds : null;
      };
      hierarchicalLayoutData.AlternativeEdgePaths.MapperFunction = edge => {
        var masterEdge = foldingView.GetMasterItem(edge)!;
        return alternativeEdgePaths.TryGetValue(masterEdge, out var path) ? path : null;
      };
      // mark folder nodes to treat them differently than normal nodes during layout
      hierarchicalLayoutData.FolderNodes.Predicate = node => !foldingView.IsExpanded(node);

      var anchoringLayoutData = anchorNodeLayout.CreateLayoutData(graph);
      anchoringLayoutData.NodeAnchoringPolicies.MapperFunction = node =>
          node == changedGroupNode ? LayoutAnchoringPolicy.Center : LayoutAnchoringPolicy.None;

      var givenCoordinatesLayout = new GivenCoordinatesLayout(anchorNodeLayout);
      var givenCoordinatesLayoutData = givenCoordinatesLayout.CreateLayoutData(graph);
      givenCoordinatesLayoutData.NodeLocations.Dictionary = nodesCoordinates;
      givenCoordinatesLayoutData.NodeSizes.Dictionary = nodeSizes;
      givenCoordinatesLayoutData.EdgePaths.Dictionary = edgesCoordinates;

      var layoutData = graph.CreateCompositeLayoutData(
          hierarchicalLayoutData, anchoringLayoutData, givenCoordinatesLayoutData);

      // the GivenCoordinatesLayout will move the nodes to their previous locations
      // to be able to run an incremental layout, all nodes inside a group node were placed at
      // the same location.
      await graphControl.ApplyLayoutAnimated(givenCoordinatesLayout, TimeSpan.FromSeconds(0.5), layoutData);
      graphControl.UpdateContentBounds();
    }

    /// <summary>
    ///   Retrieves the affected edges when a group node is collapsed.
    ///   Edges are affected when they connect to the group node directly or to a descendant of the group node.
    /// </summary>
    /// <param name="graph">The graph to which the edges belong.</param>
    /// <param name="group">The group node which is collapsed.</param>
    /// <returns>An array of all affected edges.</returns>
    private IEnumerable<IEdge> GetAffectedEdges(IGraph graph, INode group) {
      // collect all edges that connect to the group node.
      var crossingEdges = graph.EdgesAt(group).ToList();

      // collect all edges that cross the group node's border.
      var groupingSupport = graph.GetGroupingSupport();
      var descendants = groupingSupport.GetDescendants(group);
      var visitedEdges = new HashSet<IEdge>();
      foreach (var descendant in descendants) {
        foreach (var edge in graph.EdgesAt(descendant)) {
          if (visitedEdges.Add(edge) && !groupingSupport.IsDescendant((INode) edge.Opposite(descendant), group)) {
            crossingEdges.Add(edge);
          }
        }
      }

      return crossingEdges;
    }

    /// <summary>
    ///   Returns the control points of the given edge.
    ///   The control points of an edge are its source port location, its bend locations, and its target
    ///   port location.
    /// </summary>
    /// <param name="edge">The edge whose control points are collected.</param>
    private static List<IPoint> GetPointList(IEdge edge) {
      var points = new List<IPoint> { edge.SourcePort.GetLocation() };
      foreach (var bend in edge.Bends) {
        points.Add(bend.Location);
      }
      points.Add(edge.TargetPort.GetLocation());
      return points;
    }
  }
}
