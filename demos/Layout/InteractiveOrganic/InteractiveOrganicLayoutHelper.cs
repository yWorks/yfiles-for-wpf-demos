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
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Organic;

namespace yWorks.Demo.InteractiveOrganic
{
  /// <summary>
  ///   Helper class for the <see cref="InteractiveOrganicLayout"/> that provides a way to update locations of items in
  ///   an <see cref="IGraph"/> instance based on the <see cref="InteractiveOrganicNodeHandle"/>s provided by the
  ///   layout algorithm.
  /// </summary>
  public sealed class InteractiveOrganicLayoutHelper
  {
    private readonly IGraph graph;
    private readonly GraphControl graphControl;

    private InteractiveOrganicLayout layout = null!;
    private readonly InteractiveOrganicLayoutData layoutData = new();
    private Task? cancelTask;

    /// <summary>
    ///   Instantiates a new helper using the given graph control and graph.
    /// </summary>
    public InteractiveOrganicLayoutHelper(GraphControl graphControl, IGraph graph) {
      this.graphControl = graphControl;
      this.graph = graph;
    }

    /// <summary>
    ///   Creates a new <see cref="InteractiveOrganicLayout"/> instance and starts it in a new thread.
    /// </summary>
    public void StartLayout() {
      // create the layout
      var organicLayout = new InteractiveOrganicLayout { StopDuration = TimeSpan.FromMilliseconds(2000) };

      // run the layout in a separate thread
      var layoutExecutor = new LayoutExecutor(graphControl, organicLayout) {
          LayoutData = layoutData,
          AnimateViewport = false,
          RunInThread = true,
          AllowUserInteraction = true
      };
      layout = organicLayout;

      // do not await, since this will be canceled in many cases and then throw an abortion exception
      layoutExecutor.Start();
    }

    /// <summary>
    ///   Starts an animator that calls <see cref="InteractiveOrganicLayoutData.UpdateNodeCenters"/>
    ///   to smoothly animate position changes of items.
    /// </summary>
    public void StartAnimator() {
      // use an animator that animates an infinite animation
      var animator = new Animator(graphControl) {
          AutoInvalidation = false,
          AnimationPriority = DispatcherPriority.Input
      };
      animator.Animate(delegate {
        if (layoutData.UpdateNodeCenters(graph) > 0) {
          graphControl.UpdateVisual();
        }
      }, TimeSpan.MaxValue);
    }

    /// <summary>
    ///   Notifies that the given new node was added to the <see cref="IGraph"/>.
    /// </summary>
    /// <remarks>
    ///   The node will be nailed down by assigning a high inertia and low stress value to it.
    /// </remarks>
    /// <param name="node">The new node</param>
    public void AddNode(INode node) {
      // we nail down all newly created nodes
      var nodeHandle = layoutData.NodeHandles[node]!;
      nodeHandle.Inertia = 1;
      nodeHandle.Stress = 0;
    }

    /// <summary>
    ///   Fixes a given node by increasing the heat of the neighbor nodes and optionally setting the inertia
    ///   and stress of the node itself to the given values.
    /// </summary>
    /// <param name="node">The node to update</param>
    /// <param name="inertia">The inertia value to set for the node</param>
    /// <param name="stress">The stress value to set for the node</param>
    public void FixNode(INode node, double inertia = -1, double stress = -1) {
      // Update the position of the node in the LayoutGraph to match the one in the IGraph
      var nodeHandle = layoutData.NodeHandles[node]!;
      nodeHandle.SetCenter(node.Layout.GetCenterX(), node.Layout.GetCenterY());
      // Actually, the node itself is fixed at the start of a drag gesture
      if (inertia >= 0) {
        nodeHandle.Inertia = inertia;
      }
      if (stress >= 0) {
        nodeHandle.Stress = stress;
      }
      // Increasing has the effect that the layout will consider this node as not completely placed...
      // In this case, the node itself is fixed, but it's neighbors will wake up
      IncreaseHeat(node, 0.5);
    }

    /// <summary>Schedules an update of the inertia for the given node.</summary>
    /// <remarks>
    ///   <para>
    ///     The inertia is defined to be a value from the interval <c>[0,1]</c>.
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <c>1.0</c>: The node will not move.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <c>0.5</c>: The node will only move half as far as it would with an inertia of <c>0.0</c>.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <c>0.0</c>: The node will move as fast as possible.
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <see cref="InteractiveOrganicNodeHandle.Inertia"/>
    public void SetInertia(INode node, double value) {
      layoutData.NodeHandles[node]!.Inertia = value;
    }

    /// <summary>Schedules an update of the stress for the given node.</summary>
    /// <remarks>
    ///   <para>
    ///     The stress value indicates how far a node will possibly move. The higher the stress of a node is,
    ///     the farther it may move.
    ///   </para>
    ///   <para>
    ///     The stress value is defined to be a value from the interval <c>[0,1]</c>.
    ///   </para>
    /// </remarks>
    public void IncreaseStress(INode node, double value = 1) {
      var nodeHandle = layoutData.NodeHandles[node]!;
      nodeHandle.Stress = Math.Min(1, nodeHandle.Stress + value);
    }

    /// <summary>
    ///   Notifies that the given node was moved, updating the center position of the respective
    ///   <see cref="InteractiveOrganicNodeHandle"/> and increasing the heat (see <see cref="IncreaseHeat"/>).
    /// </summary>
    /// <param name="node">The node that is moving</param>
    public void MoveNode(INode node) {
      var handle = layoutData.NodeHandles[node]!;
      // Update the position of the node in the LayoutGraph to match the one in the IGraph
      handle.SetCenter(node.Layout.GetCenterX(), node.Layout.GetCenterY());
      // Increasing the heat has the effect that the layout will consider these nodes as not completely placed...
      IncreaseHeat(node, 0.05);
    }

    /// <summary>
    ///   Increases the heat of the neighbors of a given node by a given value.
    /// </summary>
    /// <remarks>
    ///   This will make the layout move the neighbor nodes more quickly.
    /// </remarks>
    public void IncreaseHeat(INode copiedNode, double delta) {
      // Increase Heat of neighbors
      foreach (var neighbor in graph.Neighbors(copiedNode)) {
        var nodeHandle = layoutData.NodeHandles[neighbor]!;
        nodeHandle.Stress = Math.Min(1, nodeHandle.Stress + delta);
      }
    }

    /// <summary>Schedules an update for the preferred length of the given edge.</summary>
    /// <param name="edge">The edge whose preferred length should be updated</param>
    /// <param name="value">The new preferred edge length</param>
    /// <see cref="InteractiveOrganicEdgeHandle.PreferredEdgeLength"/>
    public void SetPreferredLength(IEdge edge, double value) {
      layoutData.EdgeHandles[edge]!.PreferredEdgeLength = value;
    }

    /// <summary>
    ///   Delegates to <see cref="InteractiveOrganicLayout.WakeUp"/> and so wakes up the algorithm
    ///   with the effect that it will restart/continue the layout calculation.
    /// </summary>
    public void WakeUp() {
      layout?.WakeUp();
    }

    /// <summary>
    ///   Convenience method that can be called whenever the structure of the <see cref="IGraph"/> was changed to
    ///   synchronize with the layout algorithm.
    /// </summary>
    /// <remarks>
    ///   This method will first cancel the currently running layout and wait for it, then it will restart a new layout
    ///   calculation now taking into account structure changes to the underlying displayed <see cref="IGraph"/>.
    /// </remarks>
    public async void Synchronize() {
      if (cancelTask != null) {
        return;
      }

      cancelTask = layout.Cancel();
      await cancelTask;
      cancelTask = null;

      WarmupNodes();
      StartLayout();
    }

    /// <summary>
    ///   Warms up all nodes in the graph by increasing their stress value in the force-directed layout, which
    ///   has the effect that they want to move more.
    /// </summary>
    public void WarmupNodes() {
      foreach (var node in graph.Nodes) {
        var handle = layoutData.NodeHandles[node]!;
        handle.Stress = Math.Max(0.5, handle.Stress);
      }
    }
  }
}
