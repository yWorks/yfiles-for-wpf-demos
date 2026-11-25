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
using yWorks.Utils;

namespace yWorks.Demo.ComponentDragAndDrop
{
  /// <summary>
  ///   Performs layout and animation during the drag and drop operation.
  /// </summary>
  internal sealed class ClearAreaLayoutHelper
  {
    /// <summary>
    ///   We use the same <see cref="LayoutGraphAdapter"/> for one drag gesture.
    /// </summary>
    private LayoutExecutor executor = null!;

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
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> resetToOriginalGraphStageData = null!;

    /// <summary>
    ///   The location of the current drag.
    /// </summary>
    public PointD Location { get; set; }

    /// <summary>
    ///   The component that has been created by the drag and drop operation.
    /// </summary>
    public IEnumerable<INode> Component { get; set; }

    /// <summary>
    ///   The <see cref="ILayoutAlgorithm"/> that makes space for the dropped component.
    /// </summary>
    private ClearAreaLayout? clearAreaLayout;

    /// <summary>
    ///   Components that should not be modified by the layout.
    /// </summary>
    private readonly bool keepComponents;

    /// <summary>
    ///   The graph that is displayed.
    /// </summary>
    private IGraph Graph => graphControl.Graph;

    /// <summary>
    ///   Initializes the helper.
    /// </summary>
    /// <param name="graphControl">The control that displays the graph.</param>
    /// <param name="component">The component the is dragged.</param>
    /// <param name="keepComponents">Defines whether or not components should not be separated by the layout algorithm.</param>
    public ClearAreaLayoutHelper(GraphControl graphControl, IEnumerable<INode> component, bool keepComponents) {
      this.graphControl = graphControl;
      oldLocation = GetCenter(component);
      Component = component;
      this.keepComponents = keepComponents;

      Location = oldLocation;
    }

    /// <summary>
    ///   Returns the center of the <see cref="Graph"/>.
    /// </summary>
    private PointD GetCenter(IEnumerable<INode> nodes) => GetRect(nodes).GetCenter();

    /// <summary>
    ///   Returns the rectangle enclosing the given nodes.
    /// </summary>
    private RectD GetRect(IEnumerable<INode> nodes) {
      var bounds = RectD.Empty;
      foreach (var node in nodes) {
        bounds = RectD.Add(bounds, node.Layout.ToRectD());
      }

      return bounds;
    }

    /// <summary>
    ///   Creates a <see cref="GivenCoordinatesLayoutData{TNode,TEdge,TNodeLabel,TEdgeLabel}"/> that stores the layout of nodes
    ///   and edges.
    /// </summary>
    private GivenCoordinatesLayoutData<INode, IEdge, ILabel, ILabel> CreateGivenCoordinateStageData() {
      var givenCoordinatesStageData = new GivenCoordinatesLayout().CreateLayoutData(Graph);

      // store the initial coordinates and sizes of all nodes and bends not related to the current component
      foreach (var node in Graph.Nodes.Where(node => !Component.Contains(node))) {
        givenCoordinatesStageData.NodeLocations.Mapper[node] = node.Layout.GetTopLeft();
        givenCoordinatesStageData.NodeSizes.Mapper[node] = node.Layout.ToSizeD();
      }

      foreach (var edge in Graph.Edges.Where(edge =>
          !Component.Contains(edge.SourcePort.Owner) || !Component.Contains(edge.TargetPort.Owner))) {
        givenCoordinatesStageData.EdgePaths.Mapper[edge] = GetEdgePath(edge);
      }

      return givenCoordinatesStageData;
    }

    /// <summary>
    ///   Gets the edge path including the source and target ports.
    /// </summary>
    private static List<IPoint> GetEdgePath(IEdge edge) {
      List<IPoint> points = [
          edge.SourcePort.GetLocation(),
          .. edge.Bends.Select(bend => bend.Location),
          edge.TargetPort.GetLocation()
      ];
      return points;
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used during the drag and drop operation.
    ///   First, all nodes and edges are pushed back into place before the drag started. Then space
    ///   is made for the component at its current position. The animation morphs all elements to the
    ///   calculated positions.
    /// </summary>
    private LayoutExecutor CreateDraggingLayoutExecutor() {
      var clearAreaLayout = new ClearAreaLayout { ClearAreaStrategy = ClearAreaStrategy.PreserveShapes };

      clearAreaLayout.ConfigureAreaOutline(Component, 10);
      var layout = new GivenCoordinatesLayout(clearAreaLayout);
      this.clearAreaLayout = clearAreaLayout;

      var clearAreaLayoutData = clearAreaLayout.CreateLayoutData(Graph);
      clearAreaLayoutData.ComponentIds.MapperFunction = node => keepComponents ? node.Tag! : null!;
      var layoutData = Graph.CreateCompositeLayoutData(resetToOriginalGraphStageData, clearAreaLayoutData);
      var graphWithoutComponent = new FilteredGraphWrapper(Graph, node => !Component.Contains(node), _ => true);

      return new LayoutExecutor(graphControl, graphWithoutComponent, layout) {
          LayoutData = layoutData,
          AnimationDuration = TimeSpan.FromMilliseconds(150),
          AnimateViewport = false
      };
    }

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag and drop operation has been
    ///   canceled. All nodes and edges are pushed back into place before the drag started.
    /// </summary>
    private LayoutExecutor CreateCanceledLayoutExecutor() =>
        new(graphControl, new GivenCoordinatesLayout()) {
            LayoutData = resetToOriginalGraphStageData,
            AnimationDuration = TimeSpan.FromMilliseconds(150)
        };

    /// <summary>
    ///   A <see cref="LayoutExecutor"/> that is used after the drag and drop operation is finished.
    ///   All nodes and edges are pushed back into place before the drag started. Then space is made
    ///   for the component that has been dropped.
    /// </summary>
    private LayoutExecutor CreateFinishedLayoutExecutor() {
      var clearAreaLayout = new ClearAreaLayout { ClearAreaStrategy = ClearAreaStrategy.PreserveShapes };
      var layout = new GivenCoordinatesLayout(clearAreaLayout);
      var clearAreaLayoutData = clearAreaLayout.CreateLayoutData(Graph);
      clearAreaLayoutData.AreaNodes.Mapper = new DictionaryMapper<INode, bool>();
      foreach (var node in Component) {
        clearAreaLayoutData.AreaNodes.Mapper[node] = true;
      }
      clearAreaLayoutData.ComponentIds.MapperFunction = node => keepComponents ? node.Tag! : null!;
      var layoutData = Graph.CreateCompositeLayoutData(resetToOriginalGraphStageData, clearAreaLayoutData);

      return new LayoutExecutor(graphControl, layout) {
          LayoutData = layoutData,
          AnimationDuration = TimeSpan.FromMilliseconds(150)
      };
    }

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
        if (canceled) {
          // reset to original graph layout
          executor = CreateCanceledLayoutExecutor();
        } else if (finished) {
          // calculate the final layout
          executor = CreateFinishedLayoutExecutor();
        } else {
          // update the location of the components outline
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
      resetToOriginalGraphStageData = CreateGivenCoordinateStageData();
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

    /// <summary>
    ///   Moves the <see cref="ClearAreaLayout.AreaOutline"/> to the current drag location.
    /// </summary>
    private void UpdateOutline() {
      if (Location != oldLocation) {
        var delta = Location - oldLocation;
        clearAreaLayout!.MoveAreaOutline(delta);
        oldLocation = Location;
      }
    }
  }
}
