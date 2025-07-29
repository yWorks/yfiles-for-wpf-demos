/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Utils;

namespace Demo.yFiles.Graph.RelocateSubtree
{
  /// <summary>
  ///   Performs layout and animation while relocating a subtree.
  /// </summary>
  internal class RelocateSubtreeLayoutHelper
  {
    /// <summary>
    ///   We use the same <see cref="LayoutGraphAdapter"/> for one drag gesture.
    /// </summary>
    private LayoutExecutor executor;

    /// <summary>
    ///   The graph that is displayed.
    /// </summary>
    private IGraph Graph => graphControl.Graph;

    /// <summary>
    ///   The control that displays the graph.
    /// </summary>
    private readonly GraphControl graphControl;

    /// <summary>
    ///   The graph layout copy on which the new layout is calculated while the subtree is moving.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> resetToWorkingGraphLayoutData;

    /// <summary>
    ///   The graph layout copy that stores the original layout before the subtree has been moved.
    /// </summary>
    /// <remarks>
    ///   This copy is used the restore the graph when the drag is canceled.
    /// </remarks>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> resetToOriginalGraphLayoutData;

    /// <summary>
    ///   The subgraph that is dragged.
    /// </summary>
    private readonly Subtree subtree;

    /// <summary>
    ///   The render tree element of the edge connecting the subtree with the rest of the graph.
    /// </summary>
    /// <remarks>
    ///   This edge is hidden while the subtree is dragged.
    /// </remarks>
    private IRenderTreeElement renderTreeElementEdge;

    /// <summary>
    ///   Sibling subtrees that should not be modified by the layout.
    /// </summary>
    private readonly DictionaryMapper<INode, object> components = new();

    /// <summary>
    ///   Initializes the helper.
    /// </summary>
    /// <param name="graphControl">The control that displays the graph.</param>
    /// <param name="subtree">The subgraph the is dragged.</param>
    public RelocateSubtreeLayoutHelper(GraphControl graphControl, Subtree subtree) {
      this.graphControl = graphControl;
      this.subtree = subtree;
    }

    #region LayoutExecutor configurations

    /// <summary>
    ///   Creates a <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> that stores the layout of nodes
    ///   and edges.
    /// </summary>
    /// <param name="nodePredicate">Determines the nodes to store.</param>
    /// <param name="edgePredicate">Determines the edges to store.</param>
    /// <returns>The <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/>.</returns>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> CreateGivenCoordinatesLayoutData(
        Predicate<INode> nodePredicate,
        Predicate<IEdge> edgePredicate) {
      var data = new GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel>();
      foreach (var node in Graph.Nodes.Where(n => nodePredicate(n))) {
        data.NodeLocations.Mapper[node] = node.Layout.GetTopLeft();
        data.NodeSizes.Mapper[node] = node.Layout.GetSize();
      }
      foreach (var edge in Graph.Edges.Where(e => edgePredicate(e))) {
        data.EdgePaths.Mapper[edge] = edge.GetPathPoints();
      }
      return data;
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used when dragging the subtree starts.
    /// </summary>
    /// <remarks>
    ///   When the drag begins, the <see cref="FillAreaLayout"/> fills up the space that was covered by the subtree.
    ///   This state is the initial layout for the <see cref="ClearAreaLayout"/> during the drag.
    /// </remarks>
    private LayoutExecutor CreateInitializingLayoutExecutor() {
      var layout = new FillAreaLayout { LayoutOrientation = PartialLayoutOrientation.TopToBottom, Spacing = 50 };
      layout.ConfigureAreaOutline(subtree.Nodes);

      var layoutData = layout.CreateLayoutData(graphControl.Graph);
      layoutData.ComponentIds.Mapper = components;

      // the FillAreaLayout is only applied to the part of the tree that does not belong to the subtree
      IGraph filteredGraph = new FilteredGraphWrapper(graphControl.Graph, n => !subtree.Nodes.Contains(n));
      return new LayoutExecutor(graphControl, filteredGraph, layout) {
          LayoutData = layoutData, AnimateViewport = false, RunInThread = true, AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used while dragging the subtree.
    /// </summary>
    /// <remarks>
    ///   First, all nodes and edges are pushed back into place before the drag started, except the
    ///   elements of the subtree. Then space is made for the subtree at its current position. The
    ///   animation morphs all elements, except those in the subtree, to the calculated positions.
    /// </remarks>
    private LayoutExecutor CreateDraggingLayoutExecutor() {
      return new DraggingLayoutExecutor(graphControl, CreateClearAreaLayout(), subtree.Nodes) {
          LayoutData = CreateClearAreaLayoutData(), RunInThread = true, AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag is finished.
    /// </summary>
    /// <remarks>
    ///   First, all nodes and edges are pushed back into place before the drag started, except the
    ///   element of the subtree. Then space is made for the subtree at its current position. The
    ///   animation morphs all elements to the calculated positions.
    /// </remarks>
    private LayoutExecutor CreateFinishedLayoutExecutor() {
      return new LayoutExecutor(graphControl, CreateClearAreaLayout()) {
          LayoutData = CreateClearAreaLayoutData(), AnimateViewport = false, AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag is canceled.
    /// </summary>
    /// <remarks>
    ///   All nodes and edges are pushed back into place before the drag started.
    /// </remarks>
    private LayoutExecutor CreateCanceledLayoutExecutor() {
      return new LayoutExecutor(graphControl, new GivenCoordinatesLayout()) {
          LayoutData = resetToOriginalGraphLayoutData, AnimateViewport = false, RunInThread = true, AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    ///   Creates a <see cref="LayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> used while dragging and finishing the gesture.
    /// </summary>
    private LayoutData<INode, IEdge, ILabel, ILabel> CreateClearAreaLayoutData() {
      // force the router to let edges leave the nodes at the center of the bottom side
      // and to let enter the nodes in the center of the top side
      var sourcePortCandidate = new EdgePortCandidates().AddFixedCandidate(PortSides.Bottom, PointD.Origin);
      var targetPortCandidate = new EdgePortCandidates().AddFixedCandidate(PortSides.Top, PointD.Origin);

      return new CompositeLayoutData<INode, IEdge, ILabel, ILabel>(resetToWorkingGraphLayoutData,
          new ClearAreaLayoutData<INode, IEdge, ILabel, ILabel> {
              AreaNodes = { Source = subtree.Nodes },
              ComponentIds = { Mapper = components },
              Ports = {
                  SourcePortCandidates = { Constant = sourcePortCandidate },
                  TargetPortCandidates = { Constant = targetPortCandidate }
              }
          });
    }

    /// <summary>
    ///   Creates an <see cref="ILayoutAlgorithm"/> used while dragging and finishing the gesture.
    /// </summary>
    private ILayoutAlgorithm CreateClearAreaLayout() {
      return new GivenCoordinatesLayout(new ClearAreaLayout {
          ClearAreaStrategy = ClearAreaStrategy.PreserveShapes, LayoutOrientation = PartialLayoutOrientation.TopToBottom
      });
    }

    /// <summary>
    ///   Calculates the layout for the whole graph but only animates the part that does not belong
    ///   to the subtree.
    /// </summary>
    private sealed class DraggingLayoutExecutor : LayoutExecutor
    {
      /// <summary>
      ///   The graph that contains all elements except the subgraph.
      /// </summary>
      /// <remarks>
      ///   This is the part of the graph that is morphed after a new layout has been calculated.
      /// </remarks>
      private readonly FilteredGraphWrapper filteredGraph;

      public DraggingLayoutExecutor(GraphControl graphControl, ILayoutAlgorithm layout, ICollection<INode> nodes) :
          base(graphControl, layout) {
        filteredGraph = new FilteredGraphWrapper(graphControl.Graph, n => !nodes.Contains(n));
        AnimateViewport = false;
      }

      /// <summary>
      ///   Creates an <see cref="IAnimation"/> that morphs all graph elements except the subgraph to
      ///   the new layout.
      /// </summary>
      protected override IAnimation CreateLayoutAnimation() {
        return filteredGraph.CreateLayoutAnimation(Adapter, AnimationDuration);
      }
    }

    #endregion

    #region Layout Execution

    /// <summary>
    ///   A lock which prevents re-entrant layout execution.
    /// </summary>
    private bool layoutIsRunning;

    /// <summary>
    ///   Indicates whether a layout run has been requested while a layout calculation is running.
    /// </summary>
    private bool layoutPending;

    /// <summary>
    ///   The current state of the gesture.
    /// </summary>
    private GestureState state;

    /// <summary>
    ///   Starts a layout calculation if none is already running.
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

    public void InitializeLayout() {
      state = GestureState.Initializing;
      RunLayout();
    }


    /// <summary>
    ///   Cancels the current layout calculation.
    /// </summary>
    public void CancelLayout() {
      executor.Stop();
      state = GestureState.Cancelling;
      RunLayout();
    }

    /// <summary>
    ///   Stops the current layout calculation.
    /// </summary>
    public void StopLayout() {
      executor.Stop();
      state = GestureState.Finishing;
      RunLayout();
    }

    /// <summary>
    ///   Called before the a layout run starts.
    /// </summary>
    private void OnLayoutStarting() {
      switch (state) {
        case GestureState.Initializing:
          resetToOriginalGraphLayoutData = CreateGivenCoordinatesLayoutData(n => true, e => true);
          executor = CreateInitializingLayoutExecutor();

          // highlight the parent of the current subtree and make the connection to the root invisible
          if (subtree.Parent != null) {
            renderTreeElementEdge = graphControl.GraphModelManager.GetRenderTreeElement(subtree.ParentToRootEdge);
            renderTreeElementEdge.Visible = false;

            subtree.NewParent = subtree.Parent;
            UpdateComponents();
            graphControl.Highlights.Add(subtree.NewParent);
          }
          break;
        case GestureState.Cancelling:
          // make root edge visible
          if (renderTreeElementEdge != null) {
            renderTreeElementEdge.Visible = true;
          }
          // remove highlight
          if (subtree.NewParent != null) {
            graphControl.Highlights.Remove(subtree.NewParent);
          }
          // reset to original graph layout
          executor = CreateCanceledLayoutExecutor();
          break;
        case GestureState.Finishing:
          // before the last run starts, we have to reparent the subtree
          if (ApplyNewParent()) {
            renderTreeElementEdge = graphControl.GraphModelManager.GetRenderTreeElement(subtree.ParentToRootEdge);
            renderTreeElementEdge.Visible = false;
          }

          // last layout run also includes subtree
          executor = CreateFinishedLayoutExecutor();

          // remove highlight
          if (subtree.NewParent != null) {
            graphControl.Highlights.Remove(subtree.NewParent);
          }
          state = GestureState.Finished;
          break;
      }
    }

    /// <summary>
    ///   Called after the a layout run finished.
    /// </summary>
    private void OnLayoutFinished() {
      switch (state) {
        case GestureState.Initializing:
          resetToWorkingGraphLayoutData =
              CreateGivenCoordinatesLayoutData(n => !subtree.Nodes.Contains(n), e => !subtree.Edges.Contains(e));
          executor = CreateDraggingLayoutExecutor();
          state = GestureState.Dragging;
          break;
        case GestureState.Dragging:
          // check for a new parent candidate
          UpdateNewParent();
          break;
        case GestureState.Finished:
          // make root edge visible
          if (subtree.Parent == subtree.NewParent && renderTreeElementEdge != null) {
            renderTreeElementEdge.Visible = true;
          }
          break;
      }
    }

    #endregion

    #region Reparenting

    /// <summary>
    ///   Checks if the moved subtree root is near another parent.
    /// </summary>
    private void UpdateNewParent() {
      // If so, we use that node as new parent of the subtree.
      var candidate = GetParentCandidate();
      if (candidate != subtree.NewParent) {
        if (subtree.NewParent != null) {
          graphControl.Highlights.Remove(subtree.NewParent);
        }

        if (candidate != null) {
          graphControl.Highlights.Add(candidate);
        }
        subtree.NewParent = candidate;
        UpdateComponents();
      }
    }

    /// <summary>
    ///   Creates a mapping to specify the components which should not be modified by <see cref="ClearAreaLayout"/>.
    /// </summary>
    private void UpdateComponents() {
      components.Clear();
      if (subtree.NewParent != null) {
        foreach (var edge in Graph.OutEdgesAt(subtree.NewParent)) {
          var siblingSubtree = new Subtree(Graph, edge.GetTargetNode());
          foreach (var node in siblingSubtree.Nodes) {
            components[node] = siblingSubtree;
          }
        }
      }
    }

    /// <summary>
    ///   Determines the node that is nearest to the subtree root.
    /// </summary>
    private INode GetParentCandidate() {
      var nodesOnTop = new List<INode>();
      var rootY = subtree.Root.Layout.Y;
      var maxY = double.NegativeInfinity;
      foreach (var node in Graph.Nodes) {
        if (!subtree.Nodes.Contains(node) && node.Layout.GetMaxY() < rootY) {
          maxY = Math.Max(maxY, node.Layout.GetMaxY());
          nodesOnTop.Add(node);
        }
      }

      var minDist = double.PositiveInfinity;
      INode result = null;
      var rootCenter = subtree.Root.Layout.GetCenter();
      foreach (var node in nodesOnTop) {
        if (node.Layout.GetMaxY() > maxY - 30) {
          var dist = rootCenter.DistanceTo(node.Layout.GetCenter());
          if (dist < minDist) {
            minDist = dist;
            result = node;
          }
        }
      }
      return result;
    }

    /// <summary>
    ///   Relocates the edge to the subtree root to the new parent node.
    /// </summary>
    /// <returns>true if the root has a new parent.</returns>
    private bool ApplyNewParent() {
      var reparented = false;
      if (subtree.Parent != subtree.NewParent) {
        if (subtree.Parent != null) {
          Graph.Remove(subtree.ParentToRootEdge);
        }
        if (subtree.NewParent != null) {
          Graph.CreateEdge(subtree.NewParent, subtree.Root);
          reparented = true;
        }
        subtree.Parent = subtree.NewParent;
      }
      return reparented;
    }

    #endregion

    /// <summary>
    ///   The states of the gesture.
    /// </summary>
    private enum GestureState
    {
      Initializing,
      Dragging,
      Cancelling,
      Finishing,
      Finished
    }
  }
}
