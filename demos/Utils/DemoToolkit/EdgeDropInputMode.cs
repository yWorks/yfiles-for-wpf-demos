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
using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo
{
  /// <summary>
  ///   Displays a dragged edge preview and starts edge creation when an edge is dropped onto the canvas.
  /// </summary>
  /// <remarks>
  ///   Add this mode to a <see cref="GraphEditorInputMode"/> using <see cref="GraphEditorInputMode.Add"/> and transfer the
  ///   dragged edge as <see cref="IEdge"/> data. Dropping on an existing edge applies the dragged edge's style to it.
  /// </remarks>
  public sealed class EdgeDropInputMode : ItemDropInputMode<IEdge>
  {
    /// <summary>
    ///   The offset between the drag location and the preview edge's endpoint nodes.
    /// </summary>
    private static readonly PointD PreviewNodeOffset = new(20, 10);

    /// <summary>
    ///   The offset between the drag location and the preview edge's bends.
    /// </summary>
    private static readonly PointD PreviewBendOffset = new(0, 10);

    private GraphEditorInputMode? graphEditorInputMode;
    private IEdgeStyle? originalEdgeDefaultStyle;

    /// <summary>
    ///   Creates an input mode that accepts <see cref="IEdge"/> data and displays an edge preview while it is dragged.
    /// </summary>
    public EdgeDropInputMode() : base(typeof(IEdge)) {
      ItemCreator = CreateDroppedEdge;
    }

    /// <summary>
    ///   Installs this input mode and subscribes to edge-creation completion and cancellation events.
    /// </summary>
    /// <param name="context">The context in which this input mode is installed.</param>
    /// <param name="concurrencyController">The controller used to manage input-mode concurrency.</param>
    /// <exception cref="InvalidOperationException">
    ///   The input mode is not being installed as part of a <see cref="GraphEditorInputMode"/>.
    /// </exception>
    public override void Install(IInputModeContext context, ConcurrencyController concurrencyController) {
      var editorInputMode = context.InputMode as GraphEditorInputMode ??
          throw new InvalidOperationException(
              $"{nameof(EdgeDropInputMode)} must be added to a {nameof(GraphEditorInputMode)}.");
      graphEditorInputMode = editorInputMode;
      base.Install(context, concurrencyController);

      editorInputMode.CreateEdgeInputMode.GestureFinished += OnEdgeCreationFinished;
      editorInputMode.CreateEdgeInputMode.GestureCanceled += OnEdgeCreationCanceled;
    }

    /// <summary>
    ///   Uninstalls this input mode, removes its event handlers, and restores the default edge style if necessary.
    /// </summary>
    /// <param name="context">The context from which this input mode is uninstalled.</param>
    public override void Uninstall(IInputModeContext context) {
      if (graphEditorInputMode != null) {
        graphEditorInputMode.CreateEdgeInputMode.GestureFinished -= OnEdgeCreationFinished;
        graphEditorInputMode.CreateEdgeInputMode.GestureCanceled -= OnEdgeCreationCanceled;
        RestoreEdgeDefaultStyle();
        graphEditorInputMode = null;
      }
      base.Uninstall(context);
    }

    /// <summary>
    ///   Populates the preview graph with a representation of the dragged edge.
    /// </summary>
    /// <param name="previewGraph">The graph to populate with the edge preview.</param>
    protected override void PopulatePreviewGraph(IGraph previewGraph) {
      if (DraggedItem == null) {
        return;
      }

      // Use invisible endpoint nodes so that only the dragged edge is visible in the preview.
      previewGraph.NodeDefaults.Style = VoidNodeStyle.Instance;
      var previewEdge = previewGraph.CreateEdge(previewGraph.CreateNode(new RectD(10, 10, 0, 0)),
          previewGraph.CreateNode(new RectD(50, 30, 0, 0)), DraggedItem.Style);
      previewGraph.AddBend(previewEdge, new PointD(30, 10));
      previewGraph.AddBend(previewEdge, new PointD(30, 30));
    }

    /// <summary>
    ///   Moves the preview edge's endpoint nodes and bends to follow the current drag location.
    /// </summary>
    /// <param name="previewGraph">The graph containing the preview edge.</param>
    /// <param name="dragLocation">The current location of the dragged edge.</param>
    protected override void UpdatePreview(IGraph previewGraph, PointD dragLocation) {
      var sourceNode = previewGraph.Nodes.FirstOrDefault();
      var targetNode = previewGraph.Nodes.Skip(1).FirstOrDefault();
      var edge = previewGraph.Edges.FirstOrDefault();
      if (sourceNode == null || targetNode == null || edge == null) {
        return;
      }

      // Keep the preview's endpoint nodes and bends positioned around the current drag location.
      previewGraph.SetNodeCenter(sourceNode, dragLocation - PreviewNodeOffset);
      previewGraph.SetNodeCenter(targetNode, dragLocation + PreviewNodeOffset);
      previewGraph.ClearBends(edge);
      previewGraph.AddBend(edge, dragLocation - PreviewBendOffset, 0);
      previewGraph.AddBend(edge, dragLocation + PreviewBendOffset, 1);
      ParentInputModeContext?.CanvasControl?.Invalidate();
    }

    /// <summary>
    ///   Gets the node or edge at the specified location to use as the drop target.
    /// </summary>
    /// <param name="dragLocation">The location to search for a drop target.</param>
    /// <returns>The first node or edge found at the location, or <see langword="null"/> if none is found.</returns>
    protected override IModelItem? GetDropTarget(PointD dragLocation) =>
        // Nodes and edges are the only supported drop targets.
        graphEditorInputMode?.FindItems(dragLocation, [GraphItemTypes.Node, GraphItemTypes.Edge]).FirstOrDefault();

    /// <summary>
    ///   Applies the dragged edge's style to an edge drop target or starts creating an edge from the drop location.
    /// </summary>
    /// <param name="context">The input-mode context for the drop operation.</param>
    /// <param name="graph">The graph that receives the drop operation.</param>
    /// <param name="draggedEdge">The edge being dragged.</param>
    /// <param name="dropTarget">The node or edge under the drop location, if any.</param>
    /// <param name="dropLocation">The location at which the edge is dropped.</param>
    /// <returns><see langword="null"/>, since edge creation is handled by the edge creation input mode.</returns>
    private IEdge? CreateDroppedEdge(
        IInputModeContext context,
        IGraph graph,
        IEdge draggedEdge,
        IModelItem? dropTarget,
        PointD dropLocation) {
      // Use the dragged edge's style for any edge changed or created by this drop.
      var style = draggedEdge.Style;
      if (dropTarget is IEdge edge) {
        // Apply the dropped edge's style to the edge under the drop location.
        graph.SetStyle(edge, style);
      } else {
        // Use the node under the drop location, or create a node if the location is empty.
        var node = dropTarget as INode ?? graph.CreateNode(dropLocation);
        var candidateLocation = graph.NodeDefaults.Ports.GetLocationParameterInstance(node);
        var candidate = new PortCandidate(node, candidateLocation);
        var createEdgeInputMode = graphEditorInputMode!.CreateEdgeInputMode;

        // Save the current default style so it can be restored when edge creation finishes or is canceled.
        originalEdgeDefaultStyle = createEdgeInputMode.EdgeDefaults.Style;
        // Apply the dropped style to this edge creation gesture and its preview.
        createEdgeInputMode.EdgeDefaults.Style = style;
        createEdgeInputMode.PreviewGraph.SetStyle(createEdgeInputMode.PreviewEdge, style);
        _ = createEdgeInputMode.StartEdgeCreation(candidate, dropLocation);
      }

      // Return focus to the canvas so the user can continue the edge creation gesture.
      context.CanvasControl.Focus();
      return null;
    }

    /// <summary>
    ///   Restores the edge creation input mode's default style after edge creation completes.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="eventArgs">The event data.</param>
    private void OnEdgeCreationFinished(object? sender, InputModeEventArgs eventArgs) {
      RestoreEdgeDefaultStyle();
    }

    /// <summary>
    ///   Restores the edge creation input mode's default style after edge creation is canceled.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="eventArgs">The event data.</param>
    private void OnEdgeCreationCanceled(object? sender, InputModeEventArgs eventArgs) {
      RestoreEdgeDefaultStyle();
    }

    /// <summary>
    ///   Restores the edge creation input mode's original default style, if one was saved.
    /// </summary>
    private void RestoreEdgeDefaultStyle() {
      if (originalEdgeDefaultStyle != null && graphEditorInputMode != null) {
        graphEditorInputMode.CreateEdgeInputMode.EdgeDefaults.Style = originalEdgeDefaultStyle;
        originalEdgeDefaultStyle = null;
      }
    }
  }
}
