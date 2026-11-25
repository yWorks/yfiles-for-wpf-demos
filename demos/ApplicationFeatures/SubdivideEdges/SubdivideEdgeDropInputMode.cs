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

using System.Collections.Generic;
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.SubdivideEdges
{
  /// <summary>
  ///   This <see cref="NodeDropInputMode"/> allows for dropping a node onto an edge. More precisely, when a node
  ///   is dropped onto an edge, a new node is created and the edge is split in two edges such that:
  ///   - The source of the first edge is the original source of the split edge and the target is the newly created node.
  ///   - The source of second edge is the newly created node and the target is the original target of the split edge.
  ///   Note that, only edges that are not in folding state are considered to be valid.
  /// </summary>
  public sealed class SubdivideEdgeDropInputMode : NodeDropInputMode
  {
    private readonly record struct SplitSegment(int Index, PointD Projection);

    /// <summary>
    ///   Returns the drop target at the specified location.
    /// </summary>
    /// <param name="dragLocation">The location to return the drop target for.</param>
    protected override IModelItem? GetDropTarget(PointD dragLocation) {
      // return the edge as hit item as well
      IModelItem? hitItem = null;

      var parentContext = ParentInputModeContext!;
      if (parentContext.CanvasControl.InputMode is GraphEditorInputMode parentMode) {
        var foldingView = ((GraphControl) parentContext.CanvasControl).Graph.GetFoldingView();
        // exclude edges that are in folding state
        hitItem = parentMode.FindItems(dragLocation, [GraphItemTypes.Edge],
            item => foldingView == null || !foldingView.IsInFoldingState(item)).FirstOrDefault();
      }

      return hitItem ?? base.GetDropTarget(dragLocation);
    }

    /// <summary>
    ///   Creates the node in the graph after it has been dropped.
    /// </summary>
    /// <param name="context">The context for which the node should be created.</param>
    /// <param name="graph">The graph in which to create the item.</param>
    /// <param name="node">The node that was dragged and should therefore be created.</param>
    /// <param name="dropTarget">The model item on which the node is dropped.</param>
    /// <param name="layout">The bounds of the new node.</param>
    protected override INode CreateNode(
        IInputModeContext context,
        IGraph graph,
        INode node,
        IModelItem? dropTarget,
        RectD layout) {
      // The super implementation of CreateNode automatically reparents the created node correctly,
      // if the dropTarget is a group node. Therefore, if the node is dropped onto an edge that is
      // located on top of a group node, provide this group node as dropTarget.
      if (dropTarget is IEdge) {
        var parentMode = context.CanvasControl.InputMode as GraphEditorInputMode;
        var groupAtDropLocation = parentMode
            ?.FindItems(context, DropLocation, [GraphItemTypes.Node], item => graph.IsGroupNode((INode) item))
            .FirstOrDefault();

        if (groupAtDropLocation is INode groupNode) {
          dropTarget = groupNode;
        }
      }

      return base.CreateNode(context, graph, node, dropTarget!, layout)!;
    }

    /// <summary>
    ///   Splits the edge if the node is dropped on an edge.
    /// </summary>
    protected override void OnItemCreated(InputModeItemEventArgs<INode> evt) {
      var newNode = evt.Item;
      if (DropTarget is not IEdge edge) {
        // trigger the default event if the dropped target is not an edge
        base.OnItemCreated(evt);
        return;
      }

      var targetGraph = ((GraphControl) ParentInputModeContext!.CanvasControl).Graph;
      var size = edge.Bends.Count;
      var droppedNodeCodeCenter = newNode.Layout.GetCenter();
      var splitEdge = edge;
      if (edge.IsSelfLoop() && (size == 0 || size == 1)) {
        splitEdge = GetDummyEdgeFromPath(edge);
      }

      // get the closest segment to the node center and create the ports for the new edges
      var closestSegment = GetClosestSegment(droppedNodeCodeCenter, splitEdge);
      var newPort1 = targetGraph.AddPort(newNode, closestSegment.Projection);
      var newPort2 = targetGraph.AddPort(newNode, closestSegment.Projection);
      // for the new edges, we use the style of the original edge
      var newEdge1 = targetGraph.CreateEdge(edge.SourcePort, newPort1, (IEdgeStyle) edge.Style.Clone());
      var newEdge2 = targetGraph.CreateEdge(newPort2, edge.TargetPort, (IEdgeStyle) edge.Style.Clone());
      var idx = 0;
      foreach (var bend in splitEdge.Bends) {
        if (idx < closestSegment.Index) {
          targetGraph.AddBend(newEdge1, bend.Location.ToPointD());
        } else {
          targetGraph.AddBend(newEdge2, bend.Location.ToPointD());
        }
        ++idx;
      }

      // copy the labels of the original edge to the newly created edges
      foreach (var label in edge.Labels) {
        targetGraph.AddLabel(newEdge1, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
        targetGraph.AddLabel(newEdge2, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
      }

      // remove the original edge from the graph
      targetGraph.Remove(edge);

      // trigger the default event
      base.OnItemCreated(evt);
    }

    /// <summary>
    ///   Returns the closest segment of the edge from the center of the node.
    /// </summary>
    /// <returns>closest segment index</returns>
    private SplitSegment GetClosestSegment(PointD point, IEdge edge) {
      // if the edge has no bends, use the projection of the node center to the edge line
      var sourcePortLocation = edge.SourcePort.GetLocation();
      var targetPortLocation = edge.TargetPort.GetLocation();
      if (edge.Bends.Count == 0 && !edge.IsSelfLoop()) {
        return new SplitSegment(0, point.GetProjectionOnSegment(sourcePortLocation, targetPortLocation));
      }

      var minDistance = double.PositiveInfinity;
      var closestSegmentIndex = -1;
      PointD? closestProjection = null;

      var lastPoint = sourcePortLocation;
      var i = 0;
      foreach (var bend in edge.Bends.Concat([new SimpleBend(edge, targetPortLocation)])) {
        var currentPoint = bend.Location.ToPointD();
        var projectionOnSegment = point.GetProjectionOnSegment(lastPoint, currentPoint);
        var distance = point.DistanceTo(projectionOnSegment);
        if (distance < minDistance) {
          minDistance = distance;
          closestSegmentIndex = i;
          closestProjection = projectionOnSegment;
        }

        lastPoint = currentPoint;
        ++i;
      }

      return new SplitSegment(closestSegmentIndex, closestProjection!.Value);
    }

    /// <summary>
    ///   Creates a dummy edge for the given self-loop edge. The new edge will take the bends from the edge style renderer.
    /// </summary>
    private SimpleEdge GetDummyEdgeFromPath(IEdge edge) {
      var previewEdge = new SimpleEdge(edge.SourcePort, edge.TargetPort);

      var path = edge.Style.Renderer.GetPathGeometry(edge, edge.Style).GetPath()!;
      var pathCursor = path.CreateCursor();
      var bends = new List<IBend>();
      while (pathCursor.MoveNext()) {
        bends.Add(new SimpleBend(previewEdge, pathCursor.CurrentEndPoint));
      }
      previewEdge.Bends = bends;

      return previewEdge;
    }
  }
}
