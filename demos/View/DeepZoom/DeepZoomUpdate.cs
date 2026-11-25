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
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.DeepZoom;

internal static class DeepZoomUpdate
{
  private const double ContentRectMargins = 50;

  /// <summary>
  ///   Registers a viewport listener, that checks if the viewport is zoomed in sufficiently close on a
  ///   group node so that it's (virtual) children need to be added to the graph that is actually
  ///   displayed and vice versa.
  ///   To avoid changing zoom and viewpoint in a listener to these changes the actual work
  ///   is deferred.
  /// </summary>
  public static void InitializeDeepZoom(GraphControl graphComponent, IFoldingView foldingView) {
    graphComponent.ViewportChanged += (_, _) => UpdateDeepZoomOnViewportChanged(graphComponent, foldingView);
  }

  /// <summary>
  ///   Exits all groups and sets the zoom to 1.
  /// </summary>
  public static void ZoomToOriginal(GraphControl graphComponent, IFoldingView foldingView) {
    foldingView.LocalRoot = null!;
    graphComponent.Zoom = 1;
  }

  /// <summary>
  ///   Exits all groups and calls <see cref="GraphControl.FitGraphBounds"/>.
  /// </summary>
  public static void FitContent(GraphControl graphComponent, IFoldingView foldingView) {
    foldingView.LocalRoot = null!;
    graphComponent.UpdateContentBounds(new InsetsD(ContentRectMargins));
    _ = graphComponent.FitGraphBounds();
  }

  // mutex that ensures viewport modifications occur one at time
  private static bool changing;

  private static void UpdateDeepZoomOnViewportChanged(GraphControl graphComponent, IFoldingView foldingView) {
    if (changing) {
      return;
    }

    // defer update
    _ = graphComponent.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
      changing = true;
      try {
        UpdateGraphComponentOnViewportChange(graphComponent, foldingView);
      } finally {
        changing = false;
      }
    }));
  }

  private static void UpdateGraphComponentOnViewportChange(GraphControl graphComponent, IFoldingView foldingView) {
    var viewport = graphComponent.Viewport;
    var masterGraph = foldingView.Manager.MasterGraph;

    // finds a group node that is not yet entered but is larger than the current viewport
    var groupNode = FindGroupNodeToEnter(graphComponent, foldingView, masterGraph, viewport);

    // If such a node exists, nothing but the contents of this group node are visible.
    // Thus, its contents are now the "real" graph, and everything that is hierarchically
    // above this group node needs to be hidden.
    if (groupNode is not null) {
      EnterGroup(graphComponent, foldingView, groupNode);
    } else {
      // If no such node exists check whether the whole graph is contained in the
      // viewport and whether a local root has been set. In this case, the graph
      // is being zoomed out, and the parent needs to be made visible again.
      var graphBounds = graphComponent.ContentBounds;
      if (foldingView.LocalRoot is not null && Encloses(viewport, graphBounds)) {
        ExitGroup(graphComponent, masterGraph, foldingView, graphBounds, foldingView.LocalRoot);
      }
    }

    LimitMinimumZoom(graphComponent, foldingView);
    LimitMaximumZoom(graphComponent, masterGraph, foldingView);
  }

  /// <summary>
  ///   Finds a group node that is not yet entered but is larger than the current viewport.
  /// </summary>
  private static INode? FindGroupNodeToEnter(
      GraphControl graphComponent,
      IFoldingView foldingView,
      IGraph masterGraph,
      RectD viewport) {
    return graphComponent.Graph.Nodes.FirstOrDefault(node => {
      var nodeLayout = node.Layout.ToRectD();
      var masterNode = foldingView.GetMasterItem(node);
      return masterGraph.IsGroupNode(masterNode) && masterGraph.GetChildren(masterNode).Any() &&
          !foldingView.IsExpanded(node) && Encloses(nodeLayout, viewport);
    });
  }

  private static bool Encloses(RectD outer, RectD inner) => outer.Contains(inner);

  /// <summary>
  ///   Enters a group and seamlessly updates the <see cref="GraphControl.Zoom"/> and
  ///   <see cref="GraphControl.ViewPoint"/> for the group contents.
  ///   For this, all graph items on the same hierarchy level as the groupNode are removed from the
  ///   view graph and the group contents are added. Afterwards, the zoom and viewPoint are updated
  ///   such that the new graph appears at the same location as the preview rendered by the
  ///   <see cref="DeepZoomGroupNodeStyle"/>.
  /// </summary>
  private static void EnterGroup(GraphControl graphComponent, IFoldingView foldingView, INode groupNode) {
    var groupLayout = groupNode.Layout;
    var groupNodeCenterInView = graphComponent.WorldToViewCoordinates(groupLayout.GetCenter());

    // Setting the local root to this group node effectively removes nodes higher up
    // in the hierarchy from the view graph.
    SetLocalRoot(graphComponent, foldingView, foldingView.GetMasterItem(groupNode));

    var graphBounds = graphComponent.ContentBounds;
    // match the zoom factor of how large the group node contents appeared to
    // the actual size of the contents
    var oldScale = Math.Min(groupLayout.Width / graphBounds.Width, groupLayout.Height / graphBounds.Height);
    var oldZoom = graphComponent.Zoom;
    graphComponent.Zoom = oldZoom * oldScale;

    // Modify the viewPoint as well so that the center of the group node into which we have zoomed in
    // coincides with the center of the graph we are now presenting as the group nodes contents.
    var graphCenterInView = graphComponent.WorldToViewCoordinates(graphBounds.GetCenter());
    var delta = (graphCenterInView - groupNodeCenterInView) * (1 / graphComponent.Zoom);
    graphComponent.ViewPoint += delta;
  }

  /// <summary>
  ///   Exits a group and seamlessly updates the <see cref="GraphControl.Zoom"/> and
  ///   <see cref="GraphControl.ViewPoint"/> for the outer graph.
  ///   For this, all children of the groupNode are removed from the view graph and items
  ///   at the groupNode's hierarchy level are added. Afterwards, the zoom and viewPoint are
  ///   updated such that the preview rendered by <see cref="DeepZoomGroupNodeStyle"/> inside
  ///   the groupNode appears at the same location as the old graph.
  /// </summary>
  private static void ExitGroup(
      GraphControl graphComponent,
      IGraph masterGraph,
      IFoldingView foldingView,
      RectD graphBounds,
      INode oldRoot) {
    var graphCenterInView = graphComponent.WorldToViewCoordinates(graphBounds.GetCenter());

    var oldZoom = graphComponent.Zoom;

    // switch the localRoot so that the viewGraph displays the parent group node.
    SetLocalRoot(graphComponent, foldingView, masterGraph.GetParent(oldRoot));

    var groupNode = foldingView.GetViewItem(oldRoot) ??
        throw new InvalidOperationException("The group node is not present in the folding view.");
    var groupLayout = groupNode.Layout;
    var newScale = Math.Min(groupLayout.Width / graphBounds.Width, groupLayout.Height / graphBounds.Height);
    graphComponent.Zoom = oldZoom / newScale;

    // likewise, adjust the viewPoint so that the center of the groups' contents
    // coincide with the center of the group node
    var groupNodeCenterInView = graphComponent.WorldToViewCoordinates(groupLayout.GetCenter());
    var delta = (groupNodeCenterInView - graphCenterInView) * (1 / graphComponent.Zoom);
    graphComponent.ViewPoint += delta;
  }

  /// <summary>
  ///   Sets the new local root and updates the <see cref="GraphControl.ContentBounds"/>.
  /// </summary>
  private static void SetLocalRoot(GraphControl graphComponent, IFoldingView foldingView, INode? newRoot) {
    foldingView.LocalRoot = newRoot!;
    // after programmatically changing the graph, the content rect has to be updated manually
    graphComponent.UpdateContentBounds(new InsetsD(ContentRectMargins));
  }

  private static void LimitMinimumZoom(GraphControl graphComponent, IFoldingView foldingView) {
    // limit zoom if at minimum or maximum depth of hierarchy
    if (foldingView.LocalRoot is not null) {
      // unrestricted zooming out
      graphComponent.MinimumZoom = 0.0000001;
    } else {
      // no local root, so we're maximally zoomed out
      graphComponent.MinimumZoom = 0.3;
    }
  }

  private static void LimitMaximumZoom(GraphControl graphComponent, IGraph masterGraph, IFoldingView foldingView) {
    // if the current root node has no group grandchildren, limit zooming further in
    if (HasGrandChildren(masterGraph, foldingView.LocalRoot)) {
      // there are further group nodes one may zoom 'into', un-restrict zooming in.
      graphComponent.MaximumZoom = 10000000;
    } else {
      // no further children, limit zooming in
      graphComponent.MaximumZoom = 4;
    }
  }

  private static bool HasGrandChildren(IGraph masterGraph, INode? groupNode) {
    return masterGraph.GetChildren(groupNode!).Where(child => masterGraph.IsGroupNode(child))
        .SelectMany(child => masterGraph.GetChildren(child)).Any();
  }
}
