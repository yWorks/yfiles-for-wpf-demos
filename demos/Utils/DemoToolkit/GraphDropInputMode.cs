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
using System.Windows;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo
{
  /// <summary>
  ///   An <see cref="ItemDropInputMode{IGraph}"/> specialized to drag and drop <see cref="IGraph"/> graphs.
  ///   It can visualize a preview of the dragged graph during the drag operation, and supports snapping
  ///   via the <see cref="ItemDropInputMode{IGraph}.SnapContext"/> and highlighting of the drop target
  ///   via a highlight indicator manager. In addition, it supports dragging nodes into groups and,
  ///   optionally, collapsed folders.
  /// </summary>
  public sealed class GraphDropInputMode : ItemDropInputMode<IGraph>
  {
    /// <summary>
    ///   Gets or sets a value indicating whether graphs can be dropped on collapsed folder nodes.
    ///   If this property is set to <see langword="true"/>, dropping a graph on collapsed folder nodes
    ///   will create the graph inside the folder node in the master graph. In that case the
    ///   <see cref="ItemDropInputMode{IGraph}.ItemCreated"/> event will yield the items of the graph in
    ///   the <see cref="FoldingManager.MasterGraph"/>. The items of the graph will not be contained in
    ///   the currently visible graph. By default this feature is disabled.
    /// </summary>
    public bool AllowFolderNodeAsParent { get; set; }

    // The center of the preview graph.
    private PointD center;

    // The latest filtered graph.
    private FilteredGraphWrapper? graphWrapper;

    /// <summary>
    ///   Constructs a new instance of class <see cref="GraphDropInputMode"/>.
    /// </summary>
    public GraphDropInputMode() : base(typeof(IGraph)) {
      ItemCreator = CreateGraph;
      center = new PointD(0, 0);
      AllowFolderNodeAsParent = false;
    }

    /// <summary>
    ///   Creates the dragged graph in the target graph after the dragged graph has been dropped.
    ///   This method is called by the <see cref="ItemDropInputMode{IGraph}.ItemCreator"/> that is set
    ///   as default on this class.
    /// </summary>
    /// <param name="context">The context for which the graph should be created.</param>
    /// <param name="graph">The target graph in which to create the dragged graph.</param>
    /// <param name="draggedGraph">
    ///   The graph that was dragged and should therefore be created. The items of the graph will be
    ///   copied into the graph of the <see cref="GraphControl"/>.
    /// </param>
    /// <param name="dropTarget">The item on which the graph is dropped.</param>
    /// <param name="dropLocation">The location where the graph has been dropped.</param>
    /// <returns>A newly created graph.</returns>
    private IGraph? CreateGraph(
        IInputModeContext context,
        IGraph graph,
        IGraph draggedGraph,
        IModelItem? dropTarget,
        PointD dropLocation) {
      // move the graph to the drop location
      var delta = dropLocation - GetCenter(draggedGraph);
      Move(draggedGraph, delta);

      // create the graph and collect the dropped nodes
      var droppedNodes = new HashSet<INode>();

      var targetGraph = graph;
      INode? parent = null;
      if (dropTarget is INode dropTargetNode) {
        if (graph.IsGroupNode(dropTargetNode)) {
          parent = dropTargetNode;
        } else {
          var foldingView = graph.GetFoldingView()!;
          targetGraph = foldingView.Manager.MasterGraph;
          parent = foldingView.GetMasterItem(dropTargetNode);
        }
      }

      new GraphCopier().Copy(draggedGraph, targetGraph, (IEnumerable<IModelItem>?) null, parent, new PointD(0, 0),
          (_, copy) => {
            if (copy is INode node) {
              droppedNodes.Add(node);
            }
          });

      if (parent is not null) {
        targetGraph.GetGroupingSupport().EnlargeGroupNode(parent, true);
      }

      // dispose any previous created graph instances to avoid memory leaks
      graphWrapper?.Dispose();

      // return the dropped graph
      graphWrapper = new FilteredGraphWrapper(targetGraph, node => droppedNodes.Contains(node));
      // update the content bounds of the target graph component
      ParentInputModeContext?.CanvasControl.UpdateContentBounds();
      return graphWrapper;
    }

    /// <summary>
    ///   Fills the specified graph that is used to preview the dragged graph.
    /// </summary>
    /// <param name="previewGraph">The preview graph to fill.</param>
    protected override void PopulatePreviewGraph(IGraph previewGraph) {
      var draggedGraph = DraggedItem;
      if (draggedGraph is null) {
        return;
      }

      // copy the dragged items into the preview graph
      new GraphCopier().Copy(draggedGraph, previewGraph);
      center = GetCenter(previewGraph);
    }

    /// <summary>
    ///   Updates the preview graph so the dragged graph is displayed at the specified drag location.
    /// </summary>
    /// <param name="previewGraph">The graph to update.</param>
    /// <param name="dragLocation">The current drag location.</param>
    protected override void UpdatePreview(IGraph previewGraph, PointD dragLocation) {
      // move the preview graph to the drag location
      var delta = dragLocation - center;
      Move(previewGraph, delta);
      center = dragLocation;
      ParentInputModeContext?.CanvasControl.Invalidate();
    }

    /// <summary>
    ///   Returns the center of the given graph.
    /// </summary>
    private static PointD GetCenter(IGraph graph) =>
        graph.Nodes.Select(node => node.Layout.ToRectD()).Aggregate(RectD.Empty, RectD.Add).GetCenter();

    /// <summary>
    ///   Moves the given graph by the given delta.
    /// </summary>
    private static void Move(IGraph graph, PointD delta) {
      foreach (var node in graph.Nodes) {
        graph.SetNodeLayout(node, node.Layout.ToRectD().GetTranslated(delta));
      }

      foreach (var bend in graph.Edges.SelectMany(edge => edge.Bends)) {
        graph.SetBendLocation(bend, bend.Location.ToPointD() + delta);
      }
    }

    protected override IModelItem? GetDropTarget(PointD dragLocation) {
      var validDrag = LastDragEventArgs is null || LastDragEventArgs.Effects != DragDropEffects.None;
      if (ParentInputModeContext is not { } context || !validDrag) {
        return null;
      }

      var graph = context.GetGraph();
      if (graph is null) {
        return null;
      }

      return GetDropTargetParentNode(context, graph, dragLocation);
    }

    /// <summary>
    ///   Returns a valid parent node at the current mouse location to drop the dragged graph onto or null.
    /// </summary>
    private IModelItem? GetDropTargetParentNode(IInputModeContext context, IGraph graph, PointD dragLocation) {
      var hitTester = context.Lookup(typeof(IHitTester)) as IHitTester;
      // hit testing needs to be done with a context whose parent input mode is this mode,
      return hitTester?.EnumerateHits(CreateInputModeContext(), dragLocation, GraphItemTypes.Node).OfType<INode>()
          .FirstOrDefault(node => IsValidDropTargetParentNode(graph, node));
    }

    /// <summary>
    ///   Checks whether the given node is a valid parent node to drop the dragged graph onto.
    /// </summary>
    private bool IsValidDropTargetParentNode(IGraph graph, INode node) =>
        graph.Contains(node) && (graph.IsGroupNode(node) || (AllowFolderNodeAsParent && IsFolderNode(graph, node)));

    /// <summary>
    ///   Checks whether the given node is a folder node.
    /// </summary>
    private static bool IsFolderNode(IGraph graph, INode node) {
      var isGroupNodeInViewGraph = graph.IsGroupNode(node);
      var foldingView = graph.GetFoldingView();
      var masterNode = foldingView?.GetMasterItem(node);
      var isGroupNodeInMasterGraph = masterNode is not null && foldingView!.Manager.MasterGraph.IsGroupNode(masterNode);
      return !isGroupNodeInViewGraph && isGroupNodeInMasterGraph;
    }

    protected override void CollectSnapResults(object source, CollectSnapResultsEventArgs evt) {
      if (PreviewGraph is null || DraggedItem is null || source is not GraphSnapContext snapContext) {
        return;
      }

      foreach (var node in PreviewGraph.Nodes) {
        var suggestedLayout = GetNodeLayoutAt(node, evt.NewLocation);
        var provider = node.Lookup(typeof(INodeSnapResultProvider)) as INodeSnapResultProvider ??
            NodeSnapResultProvider.Instance;
        provider.CollectSnapResults(snapContext, evt, suggestedLayout, node);
      }
    }

    /// <summary>
    ///   Gets the layout of a given node at the given location.
    /// </summary>
    private RectD GetNodeLayoutAt(INode node, PointD location) {
      var delta = location - center;
      return node.Layout.ToRectD().GetTranslated(delta);
    }

    /// <summary>
    ///   Initializes the information for a drag and drop operation from the graph palette and starts the operation.
    /// </summary>
    public static DragDropEffects StartDrag(
        FrameworkElement dragSource,
        IGraph graph,
        DragDropEffects dragDropEffects = DragDropEffects.Copy) {
      var data = new DataObject(typeof(IGraph), graph);
      return DragDrop.DoDragDrop(dragSource, data, dragDropEffects);
    }
  }
}
