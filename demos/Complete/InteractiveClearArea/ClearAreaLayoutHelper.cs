/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Partial;

namespace Demo.yFiles.Graph.InteractiveClearArea
{
  /// <summary>
  /// Performs layout and animation while dragging the rectangular area.
  /// </summary>
  internal class ClearAreaLayoutHelper
  {
    /// <summary>
    /// We use the same <see cref="LayoutGraphAdapter"/> for one drag gesture.
    /// </summary>
    private LayoutExecutor executor;

    /// <summary>
    /// Options to control the layout behavior.
    /// </summary>
    private readonly InteractiveClearAreaDemo.LayoutOptions options;

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
    /// The graph layout copy that stores the original layout before the rectangle has been dragged.
    /// </summary>
    private GivenCoordinatesStageData resetToOriginalGraphStageData;

    /// <summary>
    /// The rectangular area that can be freely moved or resized.
    /// </summary>
    private readonly MutableRectangle clearRect;

    /// <summary>
    /// The layout of the rectangular area at the beginning of the gesture. Used for undo/redo.
    /// </summary>
    private RectD oldClearRect;

    /// <summary>
    /// The <see cref="ILayoutAlgorithm"/> that makes space for the rectangular area.
    /// </summary>
    private ClearAreaLayout clearAreaLayout;

    /// <summary>
    /// The group node we are currently inside.
    /// </summary>
    public INode GroupNode { get; set; }

    /// <summary>
    /// Initializes the helper.
    /// </summary>
    /// <param name="graphControl">The control that displays the graph.</param>
    /// <param name="clearRect">The rectangle the is dragged.</param>
    /// <param name="options">Options to control the layout behavior.</param>
    public ClearAreaLayoutHelper(GraphControl graphControl, MutableRectangle clearRect, InteractiveClearAreaDemo.LayoutOptions options) {
      this.graphControl = graphControl;
      this.clearRect = clearRect;
      this.options = options;
    }

    #region LayoutExecutor configurations

    /// <summary>
    /// Creates a <see cref="GivenCoordinatesStageData"/> that store the layout of nodes and edges.
    /// </summary>
    /// <returns>The <see cref="GivenCoordinatesStageData"/>.</returns>
    private GivenCoordinatesStageData CreateGivenCoordinateStageData() {
      var data = new GivenCoordinatesStageData();
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
    /// A <see cref="LayoutExecutor"/> that is used while dragging the rectangle.
    /// </summary>
    /// <remarks>
    /// First, all nodes and edges are pushed back into place before the drag started. Then space 
    /// is made for the rectangle at its current position. The animation morphs all elements to the 
    /// calculated positions.
    /// </remarks>
    private LayoutExecutor CreateDraggingLayoutExecutor() {
      return new LayoutExecutor(graphControl, CreateDraggingLayout())
      {
        LayoutData = CreateDraggingLayoutData(),
        RunInThread = true,
        Duration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    /// A <see cref="LayoutExecutor"/> that is used after the drag is canceled.
    /// </summary>
    /// <remarks>
    /// All nodes and edges are pushed back into place before the drag started.
    /// </remarks>
    private LayoutExecutor CreateCanceledLayoutExecutor() {
      return new LayoutExecutor(graphControl, new GivenCoordinatesStage())
      {
        LayoutData = resetToOriginalGraphStageData,
        RunInThread = true,
        Duration = TimeSpan.FromMilliseconds(150)
      };
    }

    /// <summary>
    /// Creates a <see cref="ILayoutAlgorithm"/> used while dragging and finishing the gesture. 
    /// </summary>
    private ILayoutAlgorithm CreateDraggingLayout() {
      return new GivenCoordinatesStage(
          clearAreaLayout = new ClearAreaLayout {
              ComponentAssignmentStrategy = options.ComponentAssignmentStrategy,
              ClearAreaStrategy = options.ClearAreaStrategy,
              ConsiderEdges = options.ConsiderEdges
          });
    }

    /// <summary>
    /// Creates a <see cref="LayoutData"/> used while dragging and finishing the gesture.
    /// </summary>
    private LayoutData CreateDraggingLayoutData() {
      return new CompositeLayoutData(
          resetToOriginalGraphStageData,
          new ClearAreaLayoutData {
              AreaGroupNode = {
                  Delegate = node => node == GroupNode
              }
          });
    }

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
    /// Indicates that the gesture has been canceled and the original layout should be restored.
    /// </summary>
    private bool canceled;
    
    /// <summary>
    /// Indicates that the gesture has been finished and the new layout should be applied.
    /// </summary>
    private bool stopped;

    /// <summary>
    /// Creates a single unit to undo and redo the complete reparent gesture.
    /// </summary>
    private ICompoundEdit layoutEdit;

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
        // before the layout run
        OnExecutorStarting();
        // start the layout
        await executor.Start();
        // after the layout run
        OnExecutorFinished();
        // free the executor for the next layout
        layoutIsRunning = false;
        // repeat if another layout has been requested in the meantime
      } while (layoutPending);
    }

    /// <summary>
    /// Prepares the layout execution.
    /// </summary>
    public void InitializeLayout() {
      // prepare undo/redo
      oldClearRect = clearRect.ToRectD();
      layoutEdit = Graph.BeginEdit("Clear Area", "Clear Area");
      
      resetToOriginalGraphStageData = CreateGivenCoordinateStageData();
      executor = CreateDraggingLayoutExecutor();
    }

    /// <summary>
    /// Cancels the current layout calculation.
    /// </summary>
    public void CancelLayout() {
      executor.Stop();
      canceled = true;
      RunLayout();
    }

    /// <summary>
    /// Stops the current layout calculation.
    /// </summary>
    public void StopLayout() {
      executor.Stop();
      stopped = true;
      RunLayout();
    }

    /// <summary>
    /// Called before the a layout run starts.
    /// </summary>
    private void OnExecutorStarting() {
      if (canceled) {
        // use an executor that resets the graph to original layout
        executor = CreateCanceledLayoutExecutor();
      } else {
        clearAreaLayout.Area = clearRect.ToRectD().ToYRectangle();
      }
    }

    /// <summary>
    /// Called after the a layout run finished.
    /// </summary>
    private void OnExecutorFinished() {
      if (canceled) {
        layoutEdit.Cancel();
      } else if (stopped) {
        // finish undo/redo
        // save the layout of the rectangular area before and after the gesture
        var newRect = clearRect.ToRectD();
        var oldRect = oldClearRect;
        graphControl.Graph.AddUndoUnit("Rectangle changed", "Rectangle changed",
            () => clearRect.Reshape(oldRect),
            () => clearRect.Reshape(newRect));

        // add all changes of the complete gesture as one undo/redo unit
        layoutEdit.Commit();
      }
    }

    #endregion

  }
}
