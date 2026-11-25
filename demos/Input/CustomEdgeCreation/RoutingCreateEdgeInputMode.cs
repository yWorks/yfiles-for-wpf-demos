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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Router;
using yWorks.Utils;

namespace yWorks.Demo.CustomEdgeCreation
{
  /// <summary>
  ///   A custom <see cref="CreateEdgeInputMode"/> that uses a <see cref="RoutingStrategy"/> to choose
  ///   how edges are routed during edge creation gestures.
  /// </summary>
  public sealed class RoutingCreateEdgeInputMode : CreateEdgeInputMode
  {
    private EdgeRouter? edgeRouter;

    // State retained from the start to the end of an edge creation gesture.
    private LayoutGraphAdapter? layoutGraphAdapter;
    private Node? layoutTargetNode;
    private Edge? layoutEdge;
    private IMapper<Edge, EdgePortCandidates>? targetPortCandidates;

    private RoutingStrategy routingStrategy = RoutingStrategy.DefaultOrthogonal;

    /// <summary>Gets or sets the <see cref="EdgeRouter"/> strategy used during edge creation.</summary>
    public RoutingStrategy RoutingStrategy {
      get => routingStrategy;
      set {
        routingStrategy = value;
        edgeRouter = value switch {
            RoutingStrategy.EdgeRouterQuality => new EdgeRouter(),
            // A zero stop duration favors routing speed over route quality.
            RoutingStrategy.EdgeRouterPerformance => new EdgeRouter { StopDuration = TimeSpan.Zero },
            _ => null
        };
        // EdgeRouter handles orthogonal routing in both router modes.
        // The default mode follows the parent mode's orthogonal editing settings.
        OrthogonalEdgeCreation = value == RoutingStrategy.DefaultOrthogonal
            ? OrthogonalEdgeEditingPolicy.Auto
            : OrthogonalEdgeEditingPolicy.Never;
      }
    }

    /// <summary>Creates an instance with the default orthogonal routing strategy.</summary>
    public RoutingCreateEdgeInputMode() {
      OrthogonalEdgeCreation = OrthogonalEdgeEditingPolicy.Auto;
    }

    /// <summary>
    ///   Initializes routing state when a gesture starts, if a layout-based routing strategy is set.
    /// </summary>
    protected override void OnGestureStarted(InputModeEventArgs args) {
      if (RoutingStrategy == RoutingStrategy.DefaultOrthogonal) {
        // Use the default edge creation behavior.
        base.OnGestureStarted(args);
        return;
      }

      // Create a layout graph and a temporary target so the router can consider the active edge.
      layoutGraphAdapter = new LayoutGraphAdapter(Graph);
      var layoutGraph = layoutGraphAdapter.LayoutGraph;
      // Find the source node of the gesture in the layout graph.
      var sourceNode = layoutGraphAdapter.GetLayoutNode(PreviewEdge.GetSourceNode())!;
      // Use a dummy target node for the edge being routed during the gesture.
      layoutTargetNode = layoutGraph.CreateNode(new RectD(0, 0, 1, 1));
      // Create the layout edge that is rerouted during the gesture.
      layoutEdge = layoutGraph.CreateEdge(sourceNode, layoutTargetNode);
      // Keep the source point at the edge creation gesture's start point.
      layoutEdge.SourcePortLocation = PreviewEdge.SourcePort.GetLocation();
      // The target point may be relative to the dummy node to which it is attached.
      layoutEdge.TargetPortLocation = layoutTargetNode.Layout.Center;

      // Route only the temporary edge, and keep its source port fixed at the selected candidate.
      var routeEdges = layoutGraph.CreateEdgeDataMap<bool>();
      routeEdges[layoutEdge] = true;
      layoutGraph.Context.AddItemData(LayoutKeys.RouteEdgesDataKey, routeEdges);

      // Tell the router to keep the source port on its existing side of the node.
      var sourceCandidates = layoutGraph.CreateEdgeDataMap<EdgePortCandidates>();
      sourceCandidates[layoutEdge] = new EdgePortCandidates().AddFixedCandidate(
          GetNodeSide(PreviewEdge.GetSourceNode().Layout, PreviewEdge.SourcePort.GetLocation()));
      layoutGraph.Context.AddItemData(EdgePortCandidates.SourcePortCandidatesDataKey, sourceCandidates);
      // The source port should not be adjusted by the layout.
      targetPortCandidates = layoutGraph.CreateEdgeDataMap<EdgePortCandidates>();
      base.OnGestureStarted(args);
    }

    /// <summary>Recalculates the preview route after each pointer move when routing is enabled.</summary>
    protected override void OnMoved(InputModeEventArgs args) {
      if (RoutingStrategy == RoutingStrategy.DefaultOrthogonal) {
        // Use the default edge creation behavior.
        base.OnMoved(args);
        return;
      }

      // Update the temporary target to match either the hit node or the current pointer location.
      var layoutGraph = layoutGraphAdapter!.LayoutGraph;
      // Clear any target port candidate from the previous move.
      layoutGraph.Context.Remove(EdgePortCandidates.TargetPortCandidatesDataKey);
      var targetPortCandidate = EndPortCandidate;
      var targetNode = PreviewEdge.GetTargetNode();
      var targetPort = PreviewEdge.TargetPort;

      if (targetPortCandidate != null) {
        // Keep the target port location and select the candidate side based on the incoming edge.
        targetPortCandidates![layoutEdge!] = new EdgePortCandidates().AddFixedCandidate(
            GetNodeSide(targetNode.Layout, targetPort.GetLocation()));
        layoutGraph.Context.AddItemData(EdgePortCandidates.TargetPortCandidatesDataKey, targetPortCandidates!);
        // Match the dummy target node's size and location to the actual hit node.
        layoutTargetNode!.Layout.Bounds = targetNode.Layout.ToRectD();
      } else {
        // No node was hit, so place a one-unit dummy node at the drag location.
        layoutTargetNode!.Layout.Bounds = new RectD(DragPoint.X, DragPoint.Y, 1, 1);
      }

      layoutEdge!.TargetPortLocation = targetPort.GetLocation();
      // Apply the chosen route and copy the generated bends to the visible preview edge.
      edgeRouter!.ApplyLayout(layoutGraph);

      PreviewGraph.ClearBends(PreviewEdge);
      foreach (var bend in layoutEdge!.Bends) {
        PreviewGraph.AddBend(PreviewEdge, bend.Location);
      }
      base.OnMoved(args);
    }

    /// <summary>Determines which <see cref="PortSides"/> value corresponds to a point on a node.</summary>
    /// <param name="nodeLayout">The node layout.</param>
    /// <param name="point">The point for which the side is calculated.</param>
    private static PortSides GetNodeSide(IRectangle nodeLayout, PointD point) {
      var deltaX = Math.Abs(nodeLayout.GetCenterX() - point.X) / nodeLayout.Width;
      var deltaY = Math.Abs(nodeLayout.GetCenterY() - point.Y) / nodeLayout.Height;
      if (deltaX == 0 && deltaY == 0) {
        return PortSides.Any;
      }
      if (deltaX > deltaY) {
        return point.X < nodeLayout.GetCenterX() ? PortSides.Left : PortSides.Right;
      }
      return point.Y < nodeLayout.GetCenterY() ? PortSides.Top : PortSides.Bottom;
    }
  }
}
