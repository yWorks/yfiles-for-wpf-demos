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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Partial;
using yWorks.Layout.Router;

namespace yWorks.Demo.InteractiveGraphRestructuring
{
  /// <summary>
  ///   Performs the layout and animation while relocating a subtree.
  /// </summary>
  internal sealed class RelocateSubtreeLayoutHelper
  {
    /// <summary>
    ///   The layout executor that will run the layout.
    /// </summary>
    private LayoutExecutor? executor;

    /// <summary>
    ///   The component that displays the graph.
    /// </summary>
    private readonly GraphControl graphControl;

    /// <summary>
    ///   The graph layout copy on which the new layout is calculated while the subtree is moving.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel>? resetToWorkingGraphStageData;

    /// <summary>
    ///   The graph layout copy that stores the original layout before the subtree has been moved.
    ///   This copy is used to restore the graph when the drag is canceled.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel>? resetToOriginalGraphStageData;

    /// <summary>
    ///   The subgraph that is dragged.
    /// </summary>
    private readonly Subtree subtree;

    /// <summary>
    ///   The render tree element of the edge connecting the subtree with the rest of the graph.
    ///   This edge is hidden while the subtree is dragged.
    /// </summary>
    private IRenderTreeElement? renderTreeElementEdge;

    /// <summary>
    ///   Sibling subtrees that should not be modified by the layout.
    /// </summary>
    private readonly Dictionary<INode, object> components = [];

    /// <summary>
    ///   A lock which prevents re-entrant layout execution.
    /// </summary>
    private bool layoutIsRunning;

    /// <summary>
    ///   Indicates whether a layout run has been requested while a layout calculation is running.
    /// </summary>
    private bool layoutPending;

    /// <summary>
    ///   The executor has been stopped. The final layout for the complete tree including the edge to
    ///   the dragged subtree shall be calculated.
    /// </summary>
    private bool stopped;

    private bool canceled;
    private bool initializing;

    /// <summary>
    ///   Creates a new instance of <see cref="RelocateSubtreeLayoutHelper"/>.
    /// </summary>
    /// <param name="graphControl">The given graphControl</param>
    /// <param name="subtree">The subtree to be moved</param>
    public RelocateSubtreeLayoutHelper(GraphControl graphControl, Subtree subtree) {
      this.graphControl = graphControl;
      this.subtree = subtree;
    }

    /// <summary>
    ///   Starts a layout calculation if none is already running.
    /// </summary>
    public void RunLayout() {
      if (layoutIsRunning) {
        // if another layout is running: request a new layout and exit
        layoutPending = true;
        return;
      }
      _ = RunLayoutCore();
    }

    private async Task RunLayoutCore() {
      do {
        // prevent other layouts from running
        layoutIsRunning = true;
        // clear the pending flag: the requested layout will run now
        layoutPending = false;
        // start the layout
        OnLayoutStarting();
        await executor!.Start();
        OnLayoutFinished();
        // free the executor for the next layout
        layoutIsRunning = false;
        // repeat if another layout has been requested in the meantime
      } while (layoutPending);
    }

    /// <summary>
    ///   Initializes the current layout calculation.
    /// </summary>
    public void InitializeLayout() {
      initializing = true;
      RunLayout();
    }

    /// <summary>
    ///   Cancels the current layout calculation.
    /// </summary>
    public void CancelLayout() {
      executor?.Stop();
      canceled = true;
      RunLayout();
    }

    /// <summary>
    ///   Stops the current layout calculation.
    /// </summary>
    public void StopLayout() {
      executor?.Stop();
      stopped = true;
      RunLayout();
    }

    /// <summary>
    ///   Called before the layout run starts.
    /// </summary>
    private void OnLayoutStarting() {
      var highlights = graphControl.Highlights;
      if (initializing) {
        resetToOriginalGraphStageData = CreateGivenCoordinatesLayoutData(_ => true, _ => true);
        executor = CreateInitializingLayoutExecutor();

        // highlight the parent of the current subtree and make the connection to the root invisible
        if (subtree.Parent is not null && subtree.ParentToRootEdge is { } parentEdge) {
          renderTreeElementEdge = graphControl.GraphModelManager.GetRenderTreeElement(parentEdge);
          if (renderTreeElementEdge is not null) {
            renderTreeElementEdge.Visible = false;
          }
          subtree.NewParent = subtree.Parent;
          UpdateComponents();
          highlights.Add(subtree.NewParent);
        }
      } else if (stopped) {
        // before the last run starts, we have to re-parent the subtree
        ApplyNewParent();
        // last layout run also includes subtree
        executor = CreateFinishedLayoutExecutor();
        if (subtree.NewParent is not null) {
          // remove highlight
          highlights.Remove(subtree.NewParent);
        }
        // make root edge visible
        if (subtree.Parent == subtree.NewParent && renderTreeElementEdge is not null) {
          renderTreeElementEdge.Visible = true;
        }
      } else if (canceled) {
        // make root edge visible
        if (renderTreeElementEdge is not null) {
          renderTreeElementEdge.Visible = true;
        }
        if (subtree.NewParent is not null) {
          // remove highlight
          highlights.Remove(subtree.NewParent);
        }
        // reset to original graph layout
        executor = CreateCanceledLayoutExecutor();
      }
    }

    /// <summary>
    ///   Called after the layout run finished.
    /// </summary>
    private void OnLayoutFinished() {
      if (initializing) {
        resetToWorkingGraphStageData = CreateGivenCoordinatesLayoutData(node => !subtree.Nodes.Contains(node),
            edge => !subtree.Edges.Contains(edge));
        executor = CreateDraggingLayoutExecutor();
        initializing = false;
      } else if (!stopped && !canceled) {
        // check for a new parent candidate
        UpdateNewParent();
      }
    }

    /// <summary>
    ///   Checks if the moved subtree root is near another parent.
    /// </summary>
    private void UpdateNewParent() {
      var candidate = GetParentCandidate();
      if (candidate == subtree.NewParent) {
        return;
      }
      // If so, we use that node as new parent of the subtree.
      if (subtree.NewParent is not null) {
        graphControl.Highlights.Remove(subtree.NewParent);
      }
      if (candidate is not null) {
        graphControl.Highlights.Add(candidate);
      }
      subtree.NewParent = candidate;
      UpdateComponents();
    }

    /// <summary>
    ///   Creates a mapping to specify the components which should not be modified by the
    ///   <see cref="ClearAreaLayout"/>.
    /// </summary>
    private void UpdateComponents() {
      components.Clear();
      if (subtree.NewParent is null) {
        return;
      }
      foreach (var edge in graphControl.Graph.OutEdgesAt(subtree.NewParent)) {
        var siblingSubtree = new Subtree(graphControl.Graph, edge.GetTargetNode());
        foreach (var node in siblingSubtree.Nodes) {
          components[node] = siblingSubtree;
        }
      }
    }

    /// <summary>
    ///   Determines the node that is nearest to the subtree root.
    /// </summary>
    private INode? GetParentCandidate() {
      var graph = graphControl.Graph;
      var rootY = subtree.Root.Layout.Y;
      var maxY = double.NegativeInfinity;
      var nodesOnTop = new List<INode>();
      foreach (var node in graph.Nodes) {
        var nodeMaxY = node.Layout.GetMaxY();
        if (!subtree.Nodes.Contains(node) && nodeMaxY < rootY) {
          maxY = Math.Max(maxY, nodeMaxY);
          nodesOnTop.Add(node);
        }
      }

      var minimumDistance = double.PositiveInfinity;
      INode? result = null;
      var rootCenter = subtree.Root.Layout.GetCenter();
      foreach (var node in nodesOnTop) {
        if (node.Layout.Y + node.Layout.Height > maxY - 30) {
          var distance = rootCenter.DistanceTo(node.Layout.GetCenter());
          if (distance < minimumDistance) {
            minimumDistance = distance;
            result = node;
          }
        }
      }
      return result;
    }

    /// <summary>
    ///   Relocates the edge to the subtree root to the new parent node.
    /// </summary>
    private void ApplyNewParent() {
      var graph = graphControl.Graph;
      if (subtree.Parent == subtree.NewParent) {
        return;
      }
      if (subtree.ParentToRootEdge is { } parentEdge) {
        graph.Remove(parentEdge);
      }
      if (subtree.NewParent is not null) {
        graph.CreateEdge(subtree.NewParent, subtree.Root);
      }
      subtree.SetParent(subtree.NewParent);
    }

    /// <summary>
    ///   Creates a <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> that stores the layout of nodes
    ///   and edges.
    /// </summary>
    /// <param name="nodePredicate">Determines the nodes to store</param>
    /// <param name="edgePredicate">Determines the edges to store</param>
    /// <returns>The <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/></returns>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> CreateGivenCoordinatesLayoutData(
        Func<INode, bool> nodePredicate,
        Func<IEdge, bool> edgePredicate) {
      var graph = graphControl.Graph;
      var layoutData = new GivenCoordinatesLayout().CreateLayoutData(graph);
      foreach (var node in graph.Nodes.Where(nodePredicate)) {
        layoutData.NodeLocations.Mapper[node] = node.Layout.GetTopLeft();
        layoutData.NodeSizes.Mapper[node] = node.Layout.GetSize();
      }
      foreach (var edge in graph.Edges.Where(edgePredicate)) {
        layoutData.EdgePaths.Mapper[edge] = edge.GetPathPoints();
      }
      return layoutData;
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used when the subtree dragging starts.
    ///   When the drag begins, the <see cref="FillAreaLayout"/> fills up the space that was covered
    ///   by the subtree. This state is the initial layout for the <see cref="ClearAreaLayout"/>
    ///   during the drag.
    /// </summary>
    private LayoutExecutor CreateInitializingLayoutExecutor() {
      var graph = graphControl.Graph;
      var filteredGraph = new FilteredGraphWrapper(graph, node => !subtree.Nodes.Contains(node), _ => true);
      var fillAreaLayout = new FillAreaLayout {
          Area = subtree.Bounds,
          LayoutOrientation = PartialLayoutOrientation.TopToBottom,
          Spacing = 50
      };
      var layoutData = fillAreaLayout.CreateLayoutData(graph);
      layoutData.ComponentIds.MapperFunction = node =>
          components.TryGetValue(node, out var component) ? component : null!;
      return new LayoutExecutor(graphControl, filteredGraph, fillAreaLayout) {
          LayoutData = layoutData,
          AnimateViewport = false,
          AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used while dragging the subtree. First, all nodes
    ///   and edges are pushed back into place before the drag started, except the elements of the
    ///   subtree. Then space is made for the subtree at its current position. The animation morphs
    ///   all elements, except those in the subtree, to the calculated positions.
    /// </summary>
    private LayoutExecutor CreateDraggingLayoutExecutor() {
      var draggingLayoutExecutor = new DraggingLayoutExecutor(graphControl, CreateClearAreaLayout(true), subtree.Nodes);
      draggingLayoutExecutor.LayoutData = CreateClearAreaLayoutData();
      draggingLayoutExecutor.AnimationDuration = TimeSpan.FromMilliseconds(150);
      return draggingLayoutExecutor;
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag is finished. First, all nodes
    ///   and edges are pushed back into place before the drag started, except the element of the
    ///   subtree. Then space is made for the subtree at its current position. The animation morphs
    ///   all elements to the calculated positions.
    /// </summary>
    private LayoutExecutor CreateFinishedLayoutExecutor() =>
        new(graphControl, CreateClearAreaLayout(false)) {
            AnimationDuration = TimeSpan.FromMilliseconds(150),
            AnimateViewport = false,
            LayoutData = CreateClearAreaLayoutData()
        };

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag is canceled. All nodes and edges
    ///   are pushed back into place before the drag started.
    /// </summary>
    private LayoutExecutor CreateCanceledLayoutExecutor() =>
        new(graphControl, new GivenCoordinatesLayout()) {
            LayoutData = resetToOriginalGraphStageData!,
            AnimationDuration = TimeSpan.FromMilliseconds(150)
        };

    /// <summary>
    ///   Creates the <see cref="ILayoutAlgorithm"/> used while dragging and finishing the gesture.
    /// </summary>
    private static ILayoutAlgorithm CreateClearAreaLayout(bool dragging) {
      var clearAreaLayout = new ClearAreaLayout {
          ClearAreaStrategy = ClearAreaStrategy.PreserveShapes,
          LayoutOrientation = PartialLayoutOrientation.TopToBottom
      };
      if (dragging) {
        clearAreaLayout.EdgeRouter = new StraightLineEdgeRouter();
      }
      return new GivenCoordinatesLayout(clearAreaLayout);
    }

    /// <summary>
    ///   Creates the <see cref="LayoutData{TNode, TEdge, TNodeLabel, TEdgeLabel}"/> used while
    ///   dragging and finishing the gesture.
    /// </summary>
    private LayoutData<INode, IEdge, ILabel, ILabel> CreateClearAreaLayoutData() {
      var clearAreaLayoutData = new ClearAreaLayout().CreateLayoutData(graphControl.Graph);
      clearAreaLayoutData.AreaNodes.Predicate = node => subtree.Nodes.Contains(node);
      clearAreaLayoutData.ComponentIds.MapperFunction = node =>
          components.TryGetValue(node, out var component) ? component : null!;
      clearAreaLayoutData.Ports.SourcePortCandidates.MapperFunction = _ =>
          // force the router to let edges leave the nodes at the center of the bottom side
          // and to let enter the nodes in the center of the top side
          new EdgePortCandidates().AddFixedCandidate(PortSides.Bottom, PointD.Origin);
      clearAreaLayoutData.Ports.TargetPortCandidates.MapperFunction = _ =>
          new EdgePortCandidates().AddFixedCandidate(PortSides.Top, PointD.Origin);
      return resetToWorkingGraphStageData!.CombineWith(clearAreaLayoutData);
    }

    /// <summary>
    ///   A class that calculates the layout for the whole graph but animates only the part that does
    ///   not belong to the subtree.
    /// </summary>
    private sealed class DraggingLayoutExecutor : LayoutExecutor
    {
      /// <summary>
      ///   The graph that contains all elements except the subgraph. This is the part of the graph
      ///   that is morphed after a new layout has been calculated.
      /// </summary>
      private readonly FilteredGraphWrapper filteredGraph;

      /// <summary>
      ///   Creates a new instance of <see cref="DraggingLayoutExecutor"/>.
      /// </summary>
      /// <param name="graphControl">The given graphControl</param>
      /// <param name="layout">The layout algorithm to apply</param>
      /// <param name="nodes">The subgraph on which the layout will be applied</param>
      public DraggingLayoutExecutor(GraphControl graphControl, ILayoutAlgorithm layout, IEnumerable<INode> nodes) :
          base(graphControl, layout) {
        AnimateViewport = false;
        var nodeSet = new HashSet<INode>(nodes);
        filteredGraph = new FilteredGraphWrapper(graphControl.Graph, node => !nodeSet.Contains(node), _ => true);
      }

      /// <summary>
      ///   Creates an <see cref="IAnimation"/> that morphs all graph elements except the subgraph to
      ///   the new layout.
      /// </summary>
      protected override IAnimation CreateLayoutAnimation() =>
          filteredGraph.CreateLayoutAnimation(Adapter!, AnimationDuration);
    }
  }
}
