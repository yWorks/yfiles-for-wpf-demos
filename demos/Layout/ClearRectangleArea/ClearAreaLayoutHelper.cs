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
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Partial;

namespace yWorks.Demo.ClearRectangleArea
{
  /// <summary>
  ///   Performs layout and animation while dragging the marquee rectangle.
  /// </summary>
  internal sealed class ClearAreaLayoutHelper
  {
    /// <summary>
    ///   Performs the layout and the animation.
    /// </summary>
    private LayoutExecutor? executor;

    /// <summary>
    ///   Options to control the layout behavior.
    /// </summary>
    private readonly LayoutOptions options;

    /// <summary>
    ///   The control that displays the graph.
    /// </summary>
    private readonly GraphControl graphControl;

    /// <summary>
    ///   The graph that is displayed.
    /// </summary>
    private IGraph Graph => graphControl.Graph;

    /// <summary>
    ///   The graph layout copy that stores the original layout before the clear rectangle has been dragged.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel>? resetToOriginalGraphStageData;

    /// <summary>
    ///   The rectangular area that can be freely moved or resized.
    /// </summary>
    private readonly MutableRectangle clearRect;

    /// <summary>
    ///   The layout of the rectangular area at the beginning of the gesture. Used for undo/redo.
    /// </summary>
    private RectD? oldClearRect;

    /// <summary>
    ///   The ILayoutAlgorithm that makes space for the rectangular area.
    /// </summary>
    private ClearAreaLayout? clearAreaLayout;

    /// <summary>
    ///   The group node we are currently inside.
    /// </summary>
    public INode? GroupNode { get; set; }

    /// <summary>
    ///   Initializes the helper.
    /// </summary>
    /// <param name="graphControl">The component that displays the graph.</param>
    /// <param name="clearRect">The rectangle that is dragged.</param>
    /// <param name="options">Options to control the layout behavior.</param>
    public ClearAreaLayoutHelper(GraphControl graphControl, MutableRectangle clearRect, LayoutOptions options) {
      this.graphControl = graphControl;
      this.clearRect = clearRect;
      this.options = options;
    }

    // --------------------------------------------------------------- LayoutExecutor configurations

    /// <summary>
    ///   Creates a GivenCoordinatesLayoutData that stores the layout of nodes and edges.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> CreateGivenCoordinateStageData() {
      var data = new GivenCoordinatesLayout().CreateLayoutData(Graph);

      foreach (var node in Graph.Nodes) {
        data.NodeLocations.Mapper[node] = node.Layout.GetTopLeft();
        data.NodeSizes.Mapper[node] = node.Layout.GetSize();
      }

      foreach (var edge in Graph.Edges) {
        data.EdgePaths.Mapper[edge] = edge.GetPathPoints();
      }

      return data;
    }

    /// <summary>
    ///   A LayoutExecutor that is used while dragging the rectangle.
    ///   First, all nodes and edges are pushed back into place before the drag started. Then space
    ///   is made for the rectangle at its current position. The animation morphs all elements to the
    ///   calculated positions.
    /// </summary>
    private LayoutExecutor CreateDraggingLayoutExecutor() =>
        new(graphControl, CreateDraggingLayout()) {
            LayoutData = CreateDraggingLayoutData(),
            AnimationDuration = TimeSpan.FromMilliseconds(150),
            AnimateViewport = false
        };

    /// <summary>
    ///   A LayoutExecutor that is used after the drag is canceled.
    ///   All nodes and edges are pushed back into place before the drag started.
    /// </summary>
    private LayoutExecutor CreateCanceledLayoutExecutor() =>
        new(graphControl, new GivenCoordinatesLayout()) {
            LayoutData = resetToOriginalGraphStageData!,
            AnimationDuration = TimeSpan.FromMilliseconds(150)
        };

    /// <summary>
    ///   Creates an ILayoutAlgorithm used while dragging and finishing the gesture.
    /// </summary>
    private ILayoutAlgorithm CreateDraggingLayout() {
      clearAreaLayout = new ClearAreaLayout {
          ComponentAssignmentStrategy = options.ComponentAssignmentStrategy,
          ClearAreaStrategy = options.ClearAreaStrategy,
          ConsiderEdges = options.ConsiderEdges
      };
      return new GivenCoordinatesLayout(clearAreaLayout);
    }

    /// <summary>
    ///   Creates a LayoutData used while dragging and finishing the gesture.
    /// </summary>
    private LayoutData<INode, IEdge, ILabel, ILabel> CreateDraggingLayoutData() {
      var clearAreaLayoutData = new ClearAreaLayout().CreateLayoutData(Graph);
      clearAreaLayoutData.AreaGroupNode.Predicate = node => node == GroupNode;
      return resetToOriginalGraphStageData!.CombineWith(clearAreaLayoutData);
    }

    // ---------------------------------------------------------------------------- Layout Execution

    /// <summary>
    ///   A lock which prevents re-entrant layout execution.
    /// </summary>
    private bool layoutIsRunning;

    /// <summary>
    ///   Indicates whether a layout run has been requested while running a layout calculation.
    /// </summary>
    private bool layoutPending;

    /// <summary>
    ///   Indicates that the gesture has been canceled and the original layout should be restored.
    /// </summary>
    private bool canceled;

    /// <summary>
    ///   Indicates that the gesture has been finished and the new layout should be applied.
    /// </summary>
    private bool stopped;

    /// <summary>
    ///   Creates a single unit to undo and redo the complete gesture.
    /// </summary>
    private ICompoundEdit? layoutEdit;

    private Task? layoutLoopTask;

    /// <summary>
    ///   Starts a layout calculation if none is already running.
    /// </summary>
    public Task RunLayoutAsync() {
      if (layoutIsRunning) {
        // If another layout is running: request a new layout and exit.
        layoutPending = true;
        return layoutLoopTask ?? Task.CompletedTask;
      }
      layoutLoopTask = RunLayoutLoopAsync();
      return layoutLoopTask;
    }

    private async Task RunLayoutLoopAsync() {
      do {
        // Prevent other layouts from running.
        layoutIsRunning = true;
        // Clear the pending flag: the requested layout will run now.
        layoutPending = false;
        // Before the layout run.
        OnExecutorStarting();
        await executor!.Start();
        // After the layout run.
        OnExecutorFinished();
        // Free the executor for the next layout.
        layoutIsRunning = false;
        // Repeat if another layout has been requested in the meantime.
      } while (layoutPending);
    }

    /// <summary>
    ///   Prepares the layout execution.
    /// </summary>
    public void InitializeLayout() {
      oldClearRect = clearRect.ToRectD();
      layoutEdit = Graph.BeginEdit("Clear Area", "Clear Area");
      resetToOriginalGraphStageData = CreateGivenCoordinateStageData();
      executor = CreateDraggingLayoutExecutor();
    }

    /// <summary>
    ///   Cancels the current layout calculation.
    /// </summary>
    public async Task CancelLayoutAsync() {
      executor?.Stop();
      if (layoutLoopTask is { IsCompleted: false } activeLayout) {
        await activeLayout;
      }
      canceled = true;
      await RunLayoutAsync();
    }

    /// <summary>
    ///   Stops the current layout calculation.
    /// </summary>
    public async Task StopLayoutAsync() {
      executor?.Stop();
      if (layoutLoopTask is { IsCompleted: false } activeLayout) {
        await activeLayout;
      }
      stopped = true;
      await RunLayoutAsync();
    }

    /// <summary>
    ///   Called before a layout run starts.
    /// </summary>
    private void OnExecutorStarting() {
      if (canceled) {
        // Use an executor that resets the graph to its original layout.
        executor = CreateCanceledLayoutExecutor();
      } else {
        clearAreaLayout!.Area = clearRect.ToRectD();
      }
    }

    /// <summary>
    ///   Called after a layout run finished.
    /// </summary>
    private void OnExecutorFinished() {
      if (canceled) {
        layoutEdit?.Cancel();
      } else if (stopped) {
        // Finish undo/redo and save the layout of the rectangle before and after the gesture.
        var newRect = clearRect.ToRectD();
        var oldRect = oldClearRect;
        Graph.AddUndoUnit("Rectangle changed", "Rectangle changed", () => {
          if (oldRect is { } previousRectangle) {
            clearRect.SetShape(previousRectangle);
          }
        }, () => clearRect.SetShape(newRect));

        // Add all changes of the complete gesture as one undo/redo unit.
        layoutEdit?.Commit();
      }
    }
  }
}
