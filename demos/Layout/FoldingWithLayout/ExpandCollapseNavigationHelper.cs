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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Partial;
using yWorks.Layout.Router;
using yWorks.Utils;
using RoutingStyle = yWorks.Layout.Partial.RoutingStyle;

namespace yWorks.Demo.FoldingWithLayout
{
  /// <summary>
  ///   A helper class that customizes the <see cref="NavigationInputMode.GroupExpanding"/>
  ///   and <see cref="NavigationInputMode.GroupCollapsing"/> operations of the
  ///   <see cref="NavigationInputMode"/>.
  ///   - Expanding a group node shifts elements to avoid overlaps.
  ///   - Collapsing a group node additionally fills vacated space to increase compactness.
  /// </summary>
  public class ExpandCollapseNavigationHelper
  {
    private readonly NavigationInputMode navigationInputMode;
    private readonly IFoldingView foldingView;

    /// <summary>
    ///   The layout algorithm applied to the subgraph of a folder node before it is expanded.
    ///   By default, the <see cref="HierarchicalLayout"/> is used.
    ///   If no layout is set, no new layout for the subgraph is calculated.
    /// </summary>
    private readonly ILayoutAlgorithm subgraphLayout;

    private readonly EdgeRouter interEdgeRouter;

    public ExpandCollapseNavigationHelper(NavigationInputMode navigationInputMode, IFoldingView foldingView) {
      this.navigationInputMode = navigationInputMode;
      this.foldingView = foldingView;

      // the default behavior of the navigation input mode is undesirable in our particular case
      navigationInputMode.AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.None;
      navigationInputMode.FitContentAfterGroupActions = false;

      // register handler that moves overlapping elements after expanding a group node
      navigationInputMode.GroupExpanding += OnGroupExpanding;
      navigationInputMode.GroupExpanded += OnGroupExpanded;

      // register handler that fills up free space after collapsing a group node
      navigationInputMode.GroupCollapsing += OnGroupCollapsing;
      navigationInputMode.GroupCollapsed += OnGroupCollapsed;

      subgraphLayout = new HierarchicalLayout { GroupLayeringPolicy = GroupLayeringPolicy.IgnoreGroups };

      interEdgeRouter = new EdgeRouter();
    }

    /// <summary>
    ///   Prepares the layout of the content of the folded group node which is about to expand.
    /// </summary>
    private void OnGroupExpanding(object? sender, InputModeItemEventArgs<INode> e) {
      var groupNode = e.Item;
      var masterGroupNode = foldingView.GetMasterItem(groupNode)!;

      // layout subgraph
      PrepareSubgraph(groupNode, foldingView, masterGroupNode);
    }

    /// <summary>
    ///   Clears the area covered by the expanded group node to avoid overlaps.
    /// </summary>
    private async void OnGroupExpanded(object? sender, InputModeItemEventArgs<INode> e) {
      var groupNode = e.Item;
      // clear the area of the expanded group node from other graph items
      await ClearArea(groupNode);
    }

    /// <summary>
    ///   Layouts the content of the group node with <see cref="subgraphLayout"/> and calls <see cref="CenterSubgraph"/>.
    /// </summary>
    private void PrepareSubgraph(INode node, IFoldingView view, INode masterGroupNode) {
      // We create a temporary second FoldingView where we already expand 'node'.
      // This tempView already has the FolderNodeStates and FoldingEdgeStates applied so we can work directly on this view
      // to get and set correct layout information.
      var tempView = view.Manager.CreateFoldingView(masterGroupNode, n => view.IsExpanded(n));

      // Apply a nice layout to the subgraph.
      if (subgraphLayout != null) {
        tempView.Graph.ApplyLayout(subgraphLayout);
      }
      tempView.LocalRoot = view.LocalRoot;

      // After that the group node is expanded in tempView and the subgraph is moved to its center
      tempView.Expand(masterGroupNode);
      CenterSubgraph(node, view, tempView);
    }

    /// <summary>
    ///   Centers the content of node and returns the bounds of the expanded group node.
    /// </summary>
    /// <param name="node">The node to center.</param>
    /// <param name="foldingView">The folding view.</param>
    /// <param name="tempView">The temporary folding view.</param>
    protected void CenterSubgraph(INode node, IFoldingView foldingView, IFoldingView tempView) {
      var masterGroupNode = foldingView.GetMasterItem(node)!;
      var tempViewGroupNode = tempView.GetViewItem(masterGroupNode)!;
      var tempViewDescendents = tempView.Graph.GetGroupingSupport().GetDescendantsBottomUp(tempViewGroupNode).ToList();

      // calculate the bounds of all descendents
      var bounds = CalculateSubgraphBounds(tempView, tempViewDescendents);

      // move the descendents so the bounds are centered on the 'node' center
      var center = node.Layout.GetCenter();
      var diffVector = center - bounds.GetCenter();
      MoveSubgraph(tempView, tempViewDescendents, diffVector);

      // clear bends of edges attached to the expanded 'node'
      foreach (var edge in foldingView.Graph.EdgesAt(node)) {
        var masterEdge = foldingView.GetMasterItem(edge)!;
        if (masterEdge.GetSourceNode() != masterGroupNode && masterEdge.GetTargetNode() != masterGroupNode) {
          tempView.Graph.ClearBends(tempView.GetViewItem(masterEdge)!);
        }
      }

      // calculate the bounds of the expanded group node
      var groupNodeBounds = bounds.GetTranslated(diffVector);
      var groupPaddingProvider = node.Lookup<IGroupPaddingProvider>();
      if (groupPaddingProvider != null) {
        // respect the padding of the group node
        var padding = groupPaddingProvider.GetPadding();
        groupNodeBounds = groupNodeBounds.GetEnlarged(padding);
      }
      // set the layout of the expanded state
      tempView.Graph.SetNodeLayout(tempViewGroupNode, groupNodeBounds);
    }

    /// <summary>
    ///   Calculates the bounds of the subgraph determined by the subGraphNodes living in tempView.
    /// </summary>
    /// <param name="tempView">The folding view containing the subGraphNodes.</param>
    /// <param name="subGraphNodes">The subgraph nodes to use for bounds calculation.</param>
    private RectD CalculateSubgraphBounds(IFoldingView tempView, IEnumerable<INode> subGraphNodes) {
      // calculate the bounds of all descendents
      var bounds = RectD.Empty;

      foreach (var child in subGraphNodes) {
        bounds += child.Layout.ToRectD();
        // consider node labels that may be external

        foreach (var label in child.Labels) {
          bounds += label.GetLayout().GetBounds();
        }

        foreach (var edge in tempView.Graph.EdgesAt(child)) {
          if (subGraphNodes.Contains(edge.Opposite(child) as INode)) {
            // edge is between nodes in the subgraph, so consider its bends
            foreach (var bend in edge.Bends) {
              bounds += bend.Location.ToPointD();
            }
          }
        }
      }

      return bounds;
    }

    /// <summary>
    ///   Moves the subgraph determined by the subGraphNodes living in tempView by vectorToMove.
    /// </summary>
    /// <param name="tempView">The view containing the subgraph nodes.</param>
    /// <param name="subGraphNodes">The subgraph nodes.</param>
    /// <param name="vectorToMove">The vector to move the subgraph.</param>
    private void MoveSubgraph(IFoldingView tempView, IEnumerable<INode> subGraphNodes, PointD vectorToMove) {
      var movedEdges = new HashSet<IEdge>();

      foreach (var tempViewDescendent in subGraphNodes) {
        tempView.Graph.SetNodeLayout(tempViewDescendent,
            tempViewDescendent.Layout.ToRectD().GetTranslated(vectorToMove));

        foreach (var edge in tempView.Graph.EdgesAt(tempViewDescendent)) {
          if (movedEdges.Add(edge)) {
            foreach (var bend in edge.Bends) {
              tempView.Graph.SetBendLocation(bend, bend.Location.ToPointD() + vectorToMove);
            }
          }
        }
      }
    }

    /// <summary>
    ///   Adjusts the folded node state and the paths of the attached edges to those of the
    ///   expanded group node which is about to collapse.
    /// </summary>
    private void OnGroupCollapsing(object? sender, InputModeItemEventArgs<INode> e) {
      var groupNode = e.Item;

      // adjust the folded node state and attached edges
      PrepareFoldedState(groupNode, foldingView);
    }

    /// <summary>
    ///   Reduces the amount of free space after the group node has been collapsed and is probably smaller.
    /// </summary>
    private async void OnGroupCollapsed(object? sender, InputModeItemEventArgs<INode> e) {
      var groupNode = e.Item;

      var masterGroupNode = foldingView.GetMasterItem(groupNode)!;
      var groupLayout = masterGroupNode.Layout.ToRectD();
      var folderLayout = foldingView.Manager.GetFolderNodeState(masterGroupNode).Layout.ToRectD();

      if (groupLayout.Contains(folderLayout)) {
        await FillArea(groupNode, groupLayout, foldingView);
      } else {
        // the folder node exceeds the area of the group node
        var combinedBounds = groupLayout + folderLayout;
        await ClearAndFillArea(groupNode, combinedBounds, foldingView);
      }
    }

    /// <summary>
    ///   Adjusts the folded node state and the paths of the attached edges to those of the expanded group node.
    /// </summary>
    private void PrepareFoldedState(INode groupNode, IFoldingView view) {
      // use a second folding view where group node can already be collapsed so we can easily adjust its layout and attached edges
      var tempView = view.Manager.CreateFoldingView(view.LocalRoot, node => view.IsExpanded(node));

      var masterGroupNode = view.GetMasterItem(groupNode)!;
      tempView.Collapse(masterGroupNode);

      // move the folder node layout to the center of the expanded group node state
      var tempViewFolderNode = tempView.GetViewItem(masterGroupNode)!;
      tempView.Graph.SetNodeCenter(tempViewFolderNode, groupNode.Layout.GetCenter());

      // transfer path of edges on folder node
      foreach (var tempEdge in tempView.Graph.EdgesAt(tempViewFolderNode)) {
        var masterEdge = tempView.GetMasterItem(tempEdge)!;
        var viewEdge = view.GetViewItem(masterEdge)!;

        tempView.Graph.ClearBends(tempEdge);
        foreach (var bend in viewEdge.Bends) {
          tempView.Graph.AddBend(tempEdge, bend.Location.ToPointD());
        }
      }
    }

    /// <summary>
    ///   Clears the area of the groupNode of graph items other than the group node and its descendants.
    /// </summary>
    /// <param name="groupNode">The group node whose area should be cleared.</param>
    protected Task ClearArea(INode groupNode) {
      // configure ClearAreaLayout to make a clear area using the bounds of the group node
      var layout = CreateClearAreaLayout();
      var layoutData = CreateClearAreaLayoutData(groupNode);
      return RunLayout(layout, layoutData);
    }

    /// <summary>
    ///   Reduces the empty space in area by moving outer nodes closer to and into it.
    /// </summary>
    protected Task FillArea(INode groupNode, RectD area, IFoldingView view) {
      var layout = CreateFillAreaLayout(area);
      var layoutData = CreateFillAreaLayoutData(groupNode, view);
      return RunLayout(layout, layoutData);
    }

    /// <summary>
    ///   Clears the area of the groupNode layout of graph items other than the group node and its
    ///   descendants and reduces empty space afterwards.
    /// </summary>
    protected Task ClearAndFillArea(INode groupNode, RectD area, IFoldingView view) =>
        RunLayout(new SequentialLayout(CreateClearAreaLayout(), CreateFillAreaLayout(area)),
            CreateClearAreaLayoutData(groupNode).CombineWith(CreateFillAreaLayoutData(groupNode, view)));

    /// <summary>
    ///   Creates a <see cref="ClearAreaLayout"/> that clears the area of an expanded group node by
    ///   moving intersecting elements. The location of the group node and its descendants are not
    ///   changed. The group node is specified in <see cref="CreateClearAreaLayoutData"/>.
    /// </summary>
    private static ILayoutAlgorithm CreateClearAreaLayout() =>
        new ClearAreaLayout {
            Spacing = 20,
            NodeLabelPlacement = NodeLabelPlacement.Ignore,
            EdgeRoutingStyle = RoutingStyle.Orthogonal
        };

    /// <summary>
    ///   Creates a <see cref="ClearAreaLayoutData{TNode, TEdge, TNodeLabel, TEdgeLabel}"/> that specifies that the area
    ///   covered by
    ///   groupNode must be cleared.
    /// </summary>
    private LayoutData<INode, IEdge, ILabel, ILabel> CreateClearAreaLayoutData(INode groupNode) {
      var layout = new ClearAreaLayout();
      var layoutData = layout.CreateLayoutData(foldingView.Graph);
      layoutData.AreaNodes.Source = [groupNode];
      return layoutData;
    }

    /// <summary>
    ///   Creates an <see cref="ILayoutAlgorithm"/> that fills the area covered by the expanded group node
    ///   (that is now collapsed and probably smaller) with other graph elements by moving nearby elements towards it.
    ///   In addition, it re-routes the adjacent edges of the group node.
    /// </summary>
    /// <param name="area">The area to be filled.</param>
    private ILayoutAlgorithm CreateFillAreaLayout(RectD area) {
      var fillAreaLayout = new FillAreaLayout {
          Area = area,
          Spacing = 20
      };
      return new SequentialLayout(interEdgeRouter, fillAreaLayout);
    }

    /// <summary>
    ///   Creates a <see cref="LayoutData{TNode, TEdge, TNodeLabel, TEdgeLabel}"/> which specifies that
    ///   <see cref="CreateFillAreaLayout"/> should keep the location of the collapsed group node fixed and that the
    ///   adjacent edges of the group should be re-routed.
    /// </summary>
    private LayoutData<INode, IEdge, ILabel, ILabel> CreateFillAreaLayoutData(INode groupNode, IFoldingView view) {
      var edgeRouterData = interEdgeRouter.CreateLayoutData(view.Graph);
      edgeRouterData.Scope.Edges.Source = [.. view.Graph.EdgesAt(groupNode)];

      var fillAreaLayout = new FillAreaLayout();
      var fillAreaLayoutData = fillAreaLayout.CreateLayoutData(view.Graph);
      fillAreaLayoutData.FixedNodes.Source = [groupNode];
      return fillAreaLayoutData.CombineWith(edgeRouterData);
    }

    private async Task RunLayout(ILayoutAlgorithm layout, LayoutData<INode, IEdge, ILabel, ILabel> layoutData) {
      await navigationInputMode.GraphControl!.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(150), layoutData);
    }
  }
}
