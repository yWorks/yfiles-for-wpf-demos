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
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Partial;
using yWorks.Utils;

namespace Demo.yFiles.Graph.ComponentDragAndDrop
{
  /// <summary>
  ///   Performs layout and animation during the drag and drop operation.
  /// </summary>
  internal class ClearAreaLayoutHelper
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
    ///   The location of the last drag. Used to move the outline to the current mouse location.
    /// </summary>
    private PointD oldLocation;

    /// <summary>
    ///   The original layout before the drag and drop operation has been started.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> resetToOriginalGraphLayoutData;

    /// <summary>
    ///   The location of the current drag.
    /// </summary>
    public PointD Location { get; set; }

    /// <summary>
    ///   The component that has been created by the drag and drop operation.
    /// </summary>
    public IGraph Component { get; set; }

    /// <summary>
    ///   The <see cref="ILayoutAlgorithm"/> that makes space for the dropped component.
    /// </summary>
    private ClearAreaLayout clearAreaLayout;

    /// <summary>
    ///   Components that should not be modified by the layout.
    /// </summary>
    private readonly DictionaryMapper<INode, object> components;

    #region Initialization

    /// <summary>
    ///   Initializes the helper.
    /// </summary>
    /// <param name="graphControl">The control that displays the graph.</param>
    /// <param name="component">The component that is dragged.</param>
    /// <param name="components">Defines components that should not be separated by the layout algorithm.</param>
    public ClearAreaLayoutHelper(
        GraphControl graphControl,
        IGraph component,
        DictionaryMapper<INode, object> components) {
      this.graphControl = graphControl;
      oldLocation = GetCenter(component);
      Component = component;
      this.components = components;
    }

    /// <summary>
    ///   Returns the center of the <paramref name="graph"/>.
    /// </summary>
    private PointD GetCenter(IGraph graph) {
      var bounds = RectD.Empty;
      foreach (var node in graph.Nodes) {
        bounds += node.Layout.ToRectD();
      }
      return bounds.GetCenter();
    }

    #endregion

    #region Layout executor configurations

    /// <summary>
    ///   Creates a <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> that store the layout of nodes
    ///   and edges.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> CreateGivenCoordinateStageData() {
      var data = new GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel>();
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
    ///   A <see cref="LayoutExecutor"/> that is used during the drag and drop operation.
    /// </summary>
    /// <remarks>
    ///   First, all nodes and edges are pushed back into place before the drag started. Then space
    ///   is made for the component at its current position. The animation morphs all elements to the
    ///   calculated positions.
    /// </remarks>
    private LayoutExecutor CreateDraggingLayoutExecutor() {
      var layout = new GivenCoordinatesLayout(clearAreaLayout = new ClearAreaLayout {
          ClearAreaStrategy = ClearAreaStrategy.PreserveShapes
      });

      var clearAreaLayoutData = clearAreaLayout.CreateLayoutData(graphControl.Graph);
      clearAreaLayoutData.ComponentIds.Mapper = components;
      var layoutData =
          graphControl.Graph.CreateCompositeLayoutData(resetToOriginalGraphLayoutData, clearAreaLayoutData);

      var items = new List<IModelItem>();
      items.AddRange(Component.Nodes);
      items.AddRange(Component.Edges);
      clearAreaLayout.ConfigureAreaOutline(items, 10);

      return new LayoutExecutor(graphControl, layout) {
          AnimationDuration = TimeSpan.FromMilliseconds(150), AnimateViewport = false, LayoutData = layoutData
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag and drop operation has been canceled.
    /// </summary>
    /// <remarks>
    ///   All nodes and edges are pushed back into place before the drag started.
    /// </remarks>
    private LayoutExecutor CreateCanceledLayoutExecutor() {
      return new LayoutExecutor(graphControl, new GivenCoordinatesLayout()) {
          AnimationDuration = TimeSpan.FromMilliseconds(150),
          AnimateViewport = false,
          LayoutData = resetToOriginalGraphLayoutData,
          RunInThread = true
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag and drop operation is finished.
    /// </summary>
    /// <remarks>
    ///   All nodes and edges are pushed back into place before the drag started. Then space is made
    ///   for the component that has been dropped.
    /// </remarks>
    private LayoutExecutor CreateFinishedLayoutExecutor() {
      var clearAreaLayout = new ClearAreaLayout { ClearAreaStrategy = ClearAreaStrategy.PreserveShapes };
      var layout = new GivenCoordinatesLayout(clearAreaLayout);

      var clearAreaLayoutData = clearAreaLayout.CreateLayoutData(graphControl.Graph);
      clearAreaLayoutData.AreaNodes.Source = Component.Nodes;
      clearAreaLayoutData.ComponentIds.Mapper = components;
      var layoutData =
          graphControl.Graph.CreateCompositeLayoutData(resetToOriginalGraphLayoutData, clearAreaLayoutData);

      return new LayoutExecutor(graphControl, layout) {
          AnimationDuration = TimeSpan.FromMilliseconds(150), AnimateViewport = false, LayoutData = layoutData
      };
    }

    #endregion

    #region Layout execution

    /// <summary>
    ///   A lock which prevents re-entrant layout execution.
    /// </summary>
    private bool layoutIsRunning;

    /// <summary>
    ///   Indicates whether a layout run has been requested while running a layout calculation.
    /// </summary>
    private bool layoutPending;

    /// <summary>
    ///   Indicates that the executor has been canceled and the original layout should be restored.
    /// </summary>
    private bool canceled;

    /// <summary>
    ///   Indicates that the final layout should be calculated.
    /// </summary>
    private bool finished;

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
        if (canceled) {
          // reset to original graph layout
          executor = CreateCanceledLayoutExecutor();
        } else if (finished) {
          // calculate the final layout
          executor = CreateFinishedLayoutExecutor();
        } else {
          // update the location of the component's outline
          UpdateOutline();
        }
        await executor.Start();
        // free the executor for the next layout
        layoutIsRunning = false;
        // repeat if another layout has been requested in the meantime
      } while (layoutPending);
    }

    /// <summary>
    ///   Prepares the layout execution.
    /// </summary>
    public void InitializeLayout() {
      resetToOriginalGraphLayoutData = CreateGivenCoordinateStageData();
      executor = CreateDraggingLayoutExecutor();
    }

    /// <summary>
    ///   Cancels the current layout calculation.
    /// </summary>
    public void CancelLayout() {
      executor.Stop();
      canceled = true;
      if (layoutIsRunning) {
        // if layout is running, append another 'cancel' layout run
        layoutPending = true;
      }
    }

    /// <summary>
    ///   Finishes the current layout calculation.
    /// </summary>
    public void FinishLayout() {
      executor.Stop();
      finished = true;
      if (layoutIsRunning) {
        // if layout is running, append another 'finish' layout run
        layoutPending = true;
      }
    }

    #endregion

    #region Outline

    /// <summary>
    ///   Moves the <see cref="ClearAreaLayout.AreaOutline"/> to the current drag location.
    /// </summary>
    private void UpdateOutline() {
      if (Location != oldLocation) {
        var delta = Location - oldLocation;
        clearAreaLayout.MoveAreaOutline(delta);
        oldLocation = Location;
      }
    }

    #endregion
  }
}
