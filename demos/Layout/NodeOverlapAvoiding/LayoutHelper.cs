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

namespace yWorks.Demo.NodeOverlapAvoiding
{
  internal interface PendingLayout
  {
    LayoutExecutor OnLayoutStarting();

    void OnLayoutFinished();
  }

  /// <summary>
  ///   Calculates a new layout so that there is space at the current position of the moved or resized node.
  /// </summary>
  internal sealed class LayoutHelper : PendingLayout
  {
    /// <summary>
    ///   Performs the layout and the animation.
    /// </summary>
    private LayoutExecutor? executor;

    private TaskCompletionSource? resolveFinishLayoutTask;

    private IGraph Graph => graphControl.Graph;

    /// <summary>
    ///   The control that displays the graph.
    /// </summary>
    private readonly GraphControl graphControl;

    /// <summary>
    ///   The graph layout copy that stores the original layout before the node has been changed.
    ///   This copy is used to restore the graph when the drag is canceled.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel>? resetToOriginalGraphStageData;

    /// <summary>
    ///   The node that is moved or resized.
    /// </summary>
    private readonly INode node;

    /// <summary>
    ///   The node that is moved and its descendants if the node is a group.
    /// </summary>
    private readonly HashSet<INode> nodes;

    /// <summary>
    ///   The initial size of the node.
    /// </summary>
    private readonly SizeD oldSize;

    /// <summary>
    ///   The state of the current gesture.
    /// </summary>
    private ResizeState resizeState;

    /// <summary>
    ///   The ILayoutAlgorithm used to fill free space when a node shrinks.
    /// </summary>
    private FillAreaLayout? fillLayout;

    /// <summary>
    ///   The current state of the gesture.
    /// </summary>
    private GestureState state;

    /// <summary>
    ///   Initializes the helper.
    /// </summary>
    public LayoutHelper(GraphControl graphControl, INode node) {
      this.graphControl = graphControl;
      this.node = node;
      oldSize = node.Layout.GetSize();
      nodes = [node];
      if (Graph.IsGroupNode(node)) {
        foreach (var descendant in Graph.GetGroupingSupport().GetDescendants(node)) {
          nodes.Add(descendant);
        }
      }
      resizeState = ResizeState.None;
      state = GestureState.Cancelled;
    }

    /// <summary>
    ///   Creates a <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/>
    ///   that stores the layout of nodes and edges.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> CreateGivenCoordinatesLayoutData(
        Func<INode, bool> nodePredicate,
        Func<IEdge, bool> edgePredicate) {
      var data = new GivenCoordinatesLayout().CreateLayoutData(Graph);
      foreach (var graphNode in Graph.Nodes.Where(nodePredicate)) {
        data.NodeLocations.Mapper[graphNode] = graphNode.Layout.GetTopLeft();
        data.NodeSizes.Mapper[graphNode] = graphNode.Layout.GetSize();
      }
      foreach (var edge in Graph.Edges.Where(edgePredicate)) {
        data.EdgePaths.Mapper[edge] = edge.GetPathPoints();
      }
      return data;
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used while dragging the node.
    /// </summary>
    private LayoutExecutor GetDragLayoutExecutor() {
      var oldResizeState = resizeState;
      resizeState = GetResizeState();
      return resizeState == oldResizeState && executor != null
          ? executor
          : new DragLayoutExecutor(graphControl, CreateLayout(resizeState), nodes) {
              LayoutData = CreateLayoutData(resizeState),
              AnimationDuration = TimeSpan.FromMilliseconds(150)
          };
    }

    private ResizeState GetResizeState() {
      var newSize = node.Layout.GetSize();
      var anySmaller = newSize.Width < oldSize.Width || newSize.Height < oldSize.Height;
      var anyGreater = newSize.Width > oldSize.Width || newSize.Height > oldSize.Height;
      return anySmaller && anyGreater ? ResizeState.Both :
          anySmaller ? ResizeState.Shrinking :
          anyGreater ? ResizeState.Growing : ResizeState.None;
    }

    /// <summary>
    ///   Creates a layout algorithm suiting the resize state.
    /// </summary>
    private ILayoutAlgorithm CreateLayout(ResizeState currentResizeState) {
      var sequentialLayout = new SequentialLayout();
      if (currentResizeState == ResizeState.Shrinking) {
        // Fill the free space of the shrunken node.
        fillLayout = new FillAreaLayout { ComponentAssignmentStrategy = ComponentAssignmentStrategy.Single };
        sequentialLayout.Layouts.Add(fillLayout);
        if (state == GestureState.Finishing) {
          // Only route edges for the final layout.
          sequentialLayout.Layouts.Add(new EdgeRouter());
        }
      } else {
        if (currentResizeState == ResizeState.Both) {
          // Fill the free space of the resized node.
          fillLayout = new FillAreaLayout { ComponentAssignmentStrategy = ComponentAssignmentStrategy.Single };
          sequentialLayout.Layouts.Add(fillLayout);
        }

        // Clear the space of the moved or enlarged node.
        sequentialLayout.Layouts.Add(new ClearAreaLayout {
            ComponentAssignmentStrategy = ComponentAssignmentStrategy.Single,
            ClearAreaStrategy = ClearAreaStrategy.Local,
            ConsiderEdges = true
        });
      }
      return new GivenCoordinatesLayout(sequentialLayout);
    }

    /// <summary>
    ///   Creates a layout data object suiting the resize state.
    /// </summary>
    private LayoutData<INode, IEdge, ILabel, ILabel> CreateLayoutData(ResizeState currentResizeState) {
      LayoutData<INode, IEdge, ILabel, ILabel> layoutData = resetToOriginalGraphStageData!;
      if (currentResizeState == ResizeState.Shrinking) {
        var fillAreaLayout = new FillAreaLayout();
        var fillAreaLayoutData = fillAreaLayout.CreateLayoutData(Graph);
        fillAreaLayoutData.FixedNodes.Source = nodes;
        layoutData = layoutData.CombineWith(fillAreaLayoutData);
        if (state == GestureState.Finishing) {
          var edgeRouter = new EdgeRouter();
          var edgeRouterData = edgeRouter.CreateLayoutData(Graph);
          edgeRouterData.Scope.IncidentNodes.Items = nodes;
          // Only route edges for the final layout.
          layoutData = layoutData.CombineWith(edgeRouterData);
        }
      } else {
        if (currentResizeState == ResizeState.Both) {
          var fillAreaLayout = new FillAreaLayout();
          var fillAreaLayoutData = fillAreaLayout.CreateLayoutData(Graph);
          fillAreaLayoutData.FixedNodes.Source = nodes;
          layoutData = layoutData.CombineWith(fillAreaLayoutData);
        }
        var clearAreaLayout = new ClearAreaLayout();
        var clearAreaLayoutData = clearAreaLayout.CreateLayoutData(Graph);
        clearAreaLayoutData.AreaNodes.Source = nodes;
        layoutData = layoutData.CombineWith(clearAreaLayoutData);
      }
      return layoutData;
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag is canceled.
    ///   All nodes and edges are pushed back into place before the drag started.
    /// </summary>
    private LayoutExecutor CreateCancelLayoutExecutor() =>
        new(graphControl, new GivenCoordinatesLayout()) {
            LayoutData = resetToOriginalGraphStageData!,
            AnimationDuration = TimeSpan.FromMilliseconds(150)
        };

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag is finished.
    ///   First, all nodes and edges are pushed back into place before the drag started, except the
    ///   node and its descendants. Then space is made for the node and its descendants in its current
    ///   position. The animation morphs all elements to the calculated positions.
    /// </summary>
    private LayoutExecutor CreateFinishLayoutExecutor() {
      var currentResizeState = GetResizeState();
      return new LayoutExecutor(graphControl, CreateLayout(currentResizeState)) {
          LayoutData = CreateLayoutData(currentResizeState),
          AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    ///   Starts a layout calculation if none is already running.
    /// </summary>
    public Task RunLayoutAsync() => LayoutRunner.Instance.RunLayoutAsync(this);

    /// <summary>
    ///   Initializes the layout calculation.
    /// </summary>
    public void InitializeLayout() {
      resetToOriginalGraphStageData = CreateGivenCoordinatesLayoutData(
          graphNode => !nodes.Contains(graphNode), edge => !IsSubgraphEdge(edge));
      executor = GetDragLayoutExecutor();
      state = GestureState.Dragging;
    }

    /// <summary>
    ///   Determines whether both source and target node of the given edge is part of the node set.
    /// </summary>
    private bool IsSubgraphEdge(IEdge edge) =>
        nodes.Contains(edge.GetSourceNode()) && nodes.Contains(edge.GetTargetNode());

    /// <summary>
    ///   Cancels the current layout calculation.
    /// </summary>
    public async Task CancelLayoutAsync() {
      state = GestureState.Cancelling;
      var completion = new TaskCompletionSource();
      resolveFinishLayoutTask = completion;
      await RunLayoutAsync();
      await completion.Task;
    }

    /// <summary>
    ///   Stops the current layout calculation.
    /// </summary>
    public async Task FinishLayoutAsync() {
      state = GestureState.Finishing;
      var completion = new TaskCompletionSource();
      resolveFinishLayoutTask = completion;
      await RunLayoutAsync();
      await completion.Task;
    }

    /// <summary>
    ///   Runs a layout immediately.
    /// </summary>
    public async Task LayoutImmediatelyAsync() {
      resetToOriginalGraphStageData = CreateGivenCoordinatesLayoutData(_ => false, _ => false);
      state = GestureState.Finishing;
      await RunLayoutAsync();
    }

    /// <summary>
    ///   Called before a layout run starts.
    /// </summary>
    public LayoutExecutor OnLayoutStarting() {
      switch (state) {
        case GestureState.Dragging:
          executor = GetDragLayoutExecutor();
          fillLayout?.ConfigureAreaOutline(nodes);
          break;
        case GestureState.Cancelling:
          executor = CreateCancelLayoutExecutor();
          state = GestureState.Cancelled;
          break;
        case GestureState.Finishing:
          executor = CreateFinishLayoutExecutor();
          fillLayout?.ConfigureAreaOutline(nodes);
          state = GestureState.Finished;
          break;
      }
      return executor!;
    }

    /// <summary>
    ///   Called after a layout run finished.
    /// </summary>
    public void OnLayoutFinished() {
      if (state is GestureState.Cancelled or GestureState.Finished) {
        resolveFinishLayoutTask?.TrySetResult();
      }
    }

    /// <summary>
    ///   Calculates the layout for the whole graph but only animates the part that does not belong
    ///   to the node and its descendants.
    /// </summary>
    private sealed class DragLayoutExecutor : LayoutExecutor
    {
      /// <summary>
      ///   The graph that contains all elements except the subgraph.
      ///   This is the part of the graph that is morphed after a new layout has been calculated.
      /// </summary>
      private readonly FilteredGraphWrapper filteredGraph;

      public DragLayoutExecutor(GraphControl graphControl, ILayoutAlgorithm layout, HashSet<INode> nodes) : base(
          graphControl, layout) {
        AnimateViewport = false;
        filteredGraph = new FilteredGraphWrapper(
            graphControl.Graph, graphNode => !nodes.Contains(graphNode), _ => true);
      }

      /// <summary>
      ///   Creates an <see cref="IAnimation"/> that morphs all graph elements except the node and
      ///   its descendants to the new layout.
      /// </summary>
      protected override IAnimation CreateLayoutAnimation() =>
          filteredGraph.CreateLayoutAnimation(Adapter!, AnimationDuration);
    }

    /// <summary>
    ///   Singleton class that ensures no two layouts are running at the same time.
    /// </summary>
    private sealed class LayoutRunner
    {
      /// <summary>
      ///   A lock which prevents re-entrant layout execution.
      /// </summary>
      private bool layoutIsRunning;

      /// <summary>
      ///   Stores the pending layout. Only one layout can be pending at a time.
      /// </summary>
      private PendingLayout? pendingLayout;

      private LayoutRunner() { }

      private static LayoutRunner? instance;

      /// <summary>
      ///   Returns the singleton instance of this class.
      /// </summary>
      public static LayoutRunner Instance => instance ??= new LayoutRunner();

      /// <summary>
      ///   Starts a layout calculation if none is already running.
      /// </summary>
      public async Task RunLayoutAsync(PendingLayout layout) {
        pendingLayout = layout;
        if (layoutIsRunning) {
          // If another layout is running, request a new layout and exit.
          return;
        }
        do {
          // Prevent other layouts from running.
          layoutIsRunning = true;
          // Clear the pending flag; the requested layout will run now.
          var currentLayout = pendingLayout!;
          pendingLayout = null;
          // Start the layout.
          var currentExecutor = currentLayout.OnLayoutStarting();
          await currentExecutor.Start();
          currentLayout.OnLayoutFinished();
          // Free the executor for the next layout.
          layoutIsRunning = false;
          // Repeat if another layout has been requested in the meantime.
        } while (pendingLayout != null);
      }
    }

    /// <summary>
    ///   The states of the gesture.
    /// </summary>
    private enum GestureState
    {
      Dragging,
      Cancelling,
      Cancelled,
      Finishing,
      Finished
    }

    /// <summary>
    ///   The states of the size change of the modified node.
    /// </summary>
    private enum ResizeState
    {
      None,
      Growing,
      Shrinking,
      Both
    }
  }
}
