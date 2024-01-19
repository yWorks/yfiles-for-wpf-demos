/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Partial;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;

namespace Demo.yFiles.Graph.OverlapAvoidingEditor
{
  /// <summary>
  /// Calculates a new layout so that there is space at the current position of the moved or resized node.
  /// </summary>
  public class LayoutHelper
  {
    /// <summary>
    /// Performs the layout and the animation.
    /// </summary>
    private LayoutExecutor executor;

    /// <summary>
    /// The graph that is displayed.
    /// </summary>
    private IGraph Graph {
      get { return graphControl.Graph; }
    }

    /// <summary>
    /// The control that displays the graph.
    /// </summary>
    private readonly GraphControl graphControl;

    /// <summary>
    /// The graph layout copy that stores the original layout before the node has been changed.
    /// </summary>
    /// <remarks>
    /// This copy is used to restore the graph when the drag is canceled.
    /// </remarks>
    private GivenCoordinatesStageData resetToOriginalGraphStageData;

    /// <summary>
    /// The node that is moved or resized.
    /// </summary>
    private readonly INode node;

    /// <summary>
    /// The node that is moved and its descendants if the node is a group.
    /// </summary>
    private readonly ISet<INode> nodes;

    /// <summary>
    /// The initial size of the node.
    /// </summary>
    private readonly SizeD oldSize;

    /// <summary>
    /// The state of the current gesture.
    /// </summary>
    /// <remarks>
    /// The state is used to choose the best layout options.
    /// </remarks>
    private ResizeState resizeState = ResizeState.None;

    /// <summary>
    /// The edges which are hidden temporally during the gesture.
    /// </summary>
    private readonly ISet<IEdge> hiddenEdges; 
    
    /// <summary>
    /// Initializes the helper.
    /// </summary>
    /// <param name="graphControl">The control that displays the graph.</param>
    /// <param name="node">The node that is moved.</param>
    public LayoutHelper(GraphControl graphControl, INode node) {
      this.graphControl = graphControl;
      this.node = node;
      this.oldSize = node.Layout.ToSizeD();

      this.nodes = new HashSet<INode> { node };
      if (Graph.IsGroupNode(node)) {
        this.nodes.UnionWith(Graph.GetGroupingSupport().GetDescendants(node));
      }

      hiddenEdges = graphControl.GraphModelManager.EdgeDescriptor is HidingEdgeDescriptor descriptor
          ? descriptor.HiddenEdges
          : new HashSet<IEdge>();
    }

    #region LayoutExecutor configurations
    
    #region Initialize

    /// <summary>
    /// Creates a <see cref="GivenCoordinatesStageData"/> that stores the layout of nodes and edges.
    /// </summary>
    /// <param name="nodePredicate">Determines the nodes to store.</param>
    /// <param name="edgePredicate">Determines the edges to store.</param>
    /// <returns>The <see cref="GivenCoordinatesStageData"/>.</returns>
    private GivenCoordinatesStageData CreateGivenCoordinatesStageData(Predicate<INode> nodePredicate, Predicate<IEdge> edgePredicate) {
      var data = new GivenCoordinatesStageData();
      foreach (var node in Graph.Nodes.Where(n => nodePredicate(n))) {
        data.NodeLocations.Mapper[node] = node.Layout.GetTopLeft();
        data.NodeSizes.Mapper[node] = node.Layout.GetSize();
      }
      foreach (var edge in Graph.Edges.Where(e => edgePredicate(e))) {
        data.EdgePaths.Mapper[edge] = edge.GetPathPoints();
      }
      return data;
    }

    #endregion

    #region Drag and Resize

    /// <summary>
    /// A <see cref="LayoutExecutor"/> that is used while dragging the node.
    /// </summary>
    private LayoutExecutor GetDragLayoutExecutor() {
      var oldResizeState = resizeState;
      resizeState = GetResizeState();
      return (resizeState == oldResizeState && executor != null) 
          ? executor 
          :  new DragLayoutExecutor(graphControl, CreateLayout(resizeState), nodes) {
              LayoutData = CreateLayoutData(resizeState),
              RunInThread = true,
              Duration = TimeSpan.FromMilliseconds(150)
          };
    }

    private ResizeState GetResizeState() {
      var newSize = node.Layout.ToSizeD();
      var anySmaller = newSize.Width < oldSize.Width || newSize.Height < oldSize.Height;
      var anyGreater = newSize.Width > oldSize.Width || newSize.Height > oldSize.Height;
      return anySmaller && anyGreater ? ResizeState.Both
          : anySmaller ? ResizeState.Shrinking
          : anyGreater ? ResizeState.Growing
          : ResizeState.None;
    }

    /// <summary>
    /// Creates a layout algorithm suiting the <paramref name="resizeState"/>.
    /// </summary>
    /// <param name="resizeState">The current state of the gesture</param>
    /// <returns>A layout algorithm suiting the <paramref name="resizeState"/>.</returns>
    private ILayoutAlgorithm CreateLayout(ResizeState resizeState) {
      var sequentialLayout = new SequentialLayout();
      if (resizeState == ResizeState.Shrinking) {
        // fill the free space of the shrunken node
        fillLayout = new FillAreaLayout { ComponentAssignmentStrategy = ComponentAssignmentStrategy.Single };
        sequentialLayout.AppendLayout(fillLayout);
        if (this.state == GestureState.Finishing) {
          // only route edges for the final layout
          sequentialLayout.AppendLayout( new EdgeRouter {Scope = Scope.RouteEdgesAtAffectedNodes});
        }
      } else {
        if (resizeState == ResizeState.Both) {
          // fill the free space of the resized node
          fillLayout = new FillAreaLayout { ComponentAssignmentStrategy = ComponentAssignmentStrategy.Single };
          sequentialLayout.AppendLayout(fillLayout);
        }
        // clear the space of the moved/enlarged node
        sequentialLayout.AppendLayout(new ClearAreaLayout {
            ComponentAssignmentStrategy = ComponentAssignmentStrategy.Single,
            ClearAreaStrategy = ClearAreaStrategy.Local,
            ConsiderEdges = true
        });
      }
      return new GivenCoordinatesStage(sequentialLayout);
    }

    /// <summary>
    /// Creates a layout data suiting the <paramref name="resizeState"/>.
    /// </summary>
    /// <param name="resizeState">The current state of the gesture</param>
    /// <returns>A layout data suiting the <paramref name="resizeState"/>.</returns>
    private LayoutData CreateLayoutData(ResizeState resizeState) {
      var layoutData = new CompositeLayoutData(resetToOriginalGraphStageData);
      if (resizeState == ResizeState.Shrinking) {
        layoutData.Items.Add(new FillAreaLayoutData { FixedNodes = { Items = nodes } });
        if (state == GestureState.Finishing) {
          // only route edges for the final layout
          layoutData.Items.Add(new EdgeRouterData { AffectedNodes = { Items = nodes } });
        }
      } else {
        if (resizeState == ResizeState.Both) {
          layoutData.Items.Add(new FillAreaLayoutData { FixedNodes = { Items = nodes } });
        }
        layoutData.Items.Add(new ClearAreaLayoutData { AreaNodes = {Items = nodes} });
      }
      return layoutData;
    }

    /// <summary>
    /// Calculates the layout for the whole graph but only animates the part that does not belong 
    /// to node and its descendants.
    /// </summary>
    private sealed class DragLayoutExecutor : LayoutExecutor
    {
      /// <summary>
      /// The graph that contains all elements except the subgraph.
      /// </summary>
      /// <remarks>
      /// This is the part of the graph that is morphed after a new layout has been calculated.
      /// </remarks>
      private readonly FilteredGraphWrapper filteredGraph;

      public DragLayoutExecutor(GraphControl graphControl, ILayoutAlgorithm layout, ICollection<INode> nodes) : base(graphControl, layout) {
        filteredGraph = new FilteredGraphWrapper(graphControl.Graph, n => !nodes.Contains(n));
      }

      /// <summary>
      /// Creates an <see cref="IAnimation"/> that morphs all graph elements except the node and its descendants to 
      /// the new layout.
      /// </summary>
      protected override IAnimation CreateMorphAnimation() {
        return filteredGraph.CreateLayoutAnimation(LayoutGraph, Duration);
      }
    }

    #endregion
    
    #region Cancel

    /// <summary>
    /// A <see cref="LayoutExecutor"/> that is used after the drag is canceled.
    /// </summary>
    /// <remarks>
    /// All nodes and edges are pushed back into place before the drag started.
    /// </remarks>
    private LayoutExecutor CreateCancelLayoutExecutor() {
      return new LayoutExecutor(graphControl, new GivenCoordinatesStage()) {
        LayoutData = resetToOriginalGraphStageData,
        RunInThread = true,
        Duration = TimeSpan.FromMilliseconds(150)
      };
    }
    
    #endregion
    
    #region Finish
    
    /// <summary>
    /// A <see cref="LayoutExecutor"/> that is used after the drag is finished.
    /// </summary>
    /// <remarks>
    /// First, all nodes and edges are pushed back into place before the drag started, except the 
    /// node and its descendants. Then space is made for the node and its descendants at its
    /// current position. The animation morphs all elements to the calculated positions.
    /// </remarks>
    private LayoutExecutor CreateFinishLayoutExecutor() {
      var state = GetResizeState();
      return new LayoutExecutor(graphControl, CreateLayout(state)) {
          LayoutData = CreateLayoutData(state),
          RunInThread = true,
          Duration = TimeSpan.FromMilliseconds(150)
      };
    }

    #endregion

    #endregion

    #region Layout Execution

    /// <summary>
    /// A lock which prevents re-entrant layout execution.
    /// </summary>
    private bool layoutIsRunning;

    /// <summary>
    /// Indicates whether a layout run has been requested while running a layout calculation.
    /// </summary>
    private bool layoutPending;

    /// <summary>
    /// The current state of the gesture.
    /// </summary>
    private GestureState state;

    /// <summary>
    /// The <see cref="FillAreaLayout"/> used for <see cref="ResizeState.Growing"/> and <see cref="ResizeState.Both"/>.
    /// </summary>
    private FillAreaLayout fillLayout;

    /// <summary>
    /// Starts a layout calculation if none is already running.
    /// </summary>
    public async void RunLayout() {
      if (layoutIsRunning) {
        // if another layout is running: request a new layout and exit
        layoutPending = true;
        return;
      }
      do {
        // prevent other layouts from running
        layoutIsRunning = true;
        // clear the pending flag: the requested layout will run now
        layoutPending = false;
        // start the layout
        OnLayoutStarting();
        await executor.Start();
        OnLayoutFinished();
        // free the executor for the next layout
        layoutIsRunning = false;
        // repeat if another layout has been requested in the meantime
      } while (layoutPending);
    }

    /// <summary>
    /// Initializes the layout calculation.
    /// </summary>
    public void InitializeLayout() {
      resetToOriginalGraphStageData = CreateGivenCoordinatesStageData(n => !nodes.Contains(n), e => !IsSubgraphEdge(e));
      
      // hide edge path for any edge between a node in 'nodes' and a node not in 'nodes' 
      HideInterEdges();
      
      executor = GetDragLayoutExecutor();
      state = GestureState.Dragging;
    }

    /// <summary>
    /// Hides the inter-edges.
    /// </summary>
    private void HideInterEdges() {
      foreach (var edge in Graph.Edges) {
        if (IsInterEdge(edge)) {
          hiddenEdges.Add(edge);
        }
      }
    }

    /// <summary>
    /// Un-hides the inter-edges.
    /// </summary>
    private void UnhideInterEdges() {
      hiddenEdges.Clear();
    }

    /// <summary>
    /// Determines whether source or target node of the given edge is part of <see cref="nodes"/>.
    /// </summary>
    private bool IsInterEdge(IEdge edge) {
      var sourceInNodes = nodes.Contains(edge.GetSourceNode());
      var targetInNodes = nodes.Contains(edge.GetTargetNode());
      return sourceInNodes && !targetInNodes ||
             targetInNodes && !sourceInNodes;
    }

    /// <summary>
    /// Determines whether both source and target node of the given edge is part of <see cref="nodes"/>.
    /// </summary>
    private bool IsSubgraphEdge(IEdge edge) {
      return nodes.Contains(edge.GetSourceNode()) && 
             nodes.Contains(edge.GetTargetNode());
    }

    /// <summary>
    /// Cancels the current layout calculation.
    /// </summary>
    public void CancelLayout() {
      state = GestureState.Cancelling;
      RunLayout();
    }

    /// <summary>
    /// Stops the current layout calculation.
    /// </summary>
    public void FinishLayout() {
      state = GestureState.Finishing;
      RunLayout();
    }

    /// <summary>
    /// Run a layout immediately.
    /// </summary>
    public void LayoutImmediately() {
      resetToOriginalGraphStageData = CreateGivenCoordinatesStageData(n => false, e => false);
      state = GestureState.Finishing;
      RunLayout();
    }

    /// <summary>
    /// Called before the a layout run starts.
    /// </summary>
    private void OnLayoutStarting() {
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
    }

    /// <summary>
    /// Called after the a layout run finished.
    /// </summary>
    private void OnLayoutFinished() {
      switch (state) {
        case GestureState.Cancelled:
        case GestureState.Finished:
          UnhideInterEdges();
          break;
      }
    }

    #endregion

    /// <summary>
    /// The states of the gesture.
    /// </summary>
    enum GestureState {
      Dragging, Cancelling, Cancelled, Finishing, Finished
    }

    /// <summary>
    /// The states of the size change of the modified node.
    /// </summary>
    enum ResizeState {
      None, Growing, Shrinking, Both
    }
  }
}
