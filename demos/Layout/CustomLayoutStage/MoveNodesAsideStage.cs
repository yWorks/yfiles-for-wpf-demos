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
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Router;
using LayoutNode = yWorks.Layout.Graph.Node;

namespace yWorks.Demo.CustomLayoutStage
{
  internal sealed class MoveNodesAsideStage : LayoutStageBase
  {
    private const double NodeDistance = 15;

    /// <summary>
    ///   A layout stage that excludes nodes with a special tag
    ///   (<see cref="MoveAsideTag.MoveAside"/>) from the core layout
    ///   and instead moves those nodes to the side of the resulting layout.
    ///   This sample demonstrates how to use properties of the original graph to do
    ///   something in the layout (which has a different graph model).
    ///   It also demonstrates how a layout stage works in general that performs its core
    ///   layout only on a part of the original graph. This often also requires that the
    ///   excluded items have to be placed some other way, and also that adjacent edges may
    ///   have to be re-routed to get a consistent result in the end.
    ///   A similar approach is used by layout stages like TreeReductionStage (which
    ///   temporarily removes non-tree edges), HideGroupNodesStage (which pretends that
    ///   no group nodes exist), SubgraphLayoutStage, and others.
    /// </summary>
    public MoveNodesAsideStage(ILayoutAlgorithm coreLayout) : base(coreLayout) { }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      if (CoreLayout is null) {
        return;
      }

      // The simplest way to hide items in the graph temporarily is the LayoutGraphHider,
      // which also remembers the hidden items and can re-show them later.
      var hider = new LayoutGraphHider(graph);
      var asideNodes = new List<LayoutNode>();

      foreach (var node in graph.Nodes.ToArray()) {
        // For each node, get the original node and see whether the tag specifies that it
        // should be handled specially.
        if (node.Tag is MoveAsideTag { MoveAside: true }) {
          // If so, hide the node for now ...
          hider.Hide(node);
          // ... and remember it for later use
          asideNodes.Add(node);
        }
      }

      // Then, run the core layout, which calculates a layout for the remaining graph.
      CoreLayout.ApplyLayout(graph);

      // What's left now is to place the nodes we've hidden earlier.
      // Since we've hidden them from the core layout, they remain at their
      // original location for now.

      // We need the bounding box of the graph without the nodes, to place
      // them correctly.
      var boundingBox = graph.GetBounds();

      // Then we show everything again. We've collected the originally-hidden
      // nodes and edges above, already. We also have to make sure in general
      // that the graph doesn't change its structure during layout. So we always
      // have to undo everything we do to the graph structure.
      hider.UnhideAll();

      // In this case we want to place all of the “aside” nodes in a single column
      // to the right of the rest of the graph. Let's first figure out how tall that
      // column is so we can immediately place them in the correct place.
      var nodeStackHeight = 0.0;
      foreach (var node in asideNodes) {
        nodeStackHeight += node.Layout.Height;
      }
      // Also account for the distance between the nodes
      nodeStackHeight += NodeDistance * (asideNodes.Count - 1);

      // Now we can place the nodes.
      var x = boundingBox.GetMaxX() + NodeDistance;
      var y = boundingBox.GetCenterY() - nodeStackHeight / 2;
      foreach (var node in asideNodes) {
        node.Layout.TopLeft = new PointD(x, y);
        y += node.Layout.Height + NodeDistance;
      }

      // Now we still have to deal with the edges that may have been connected to
      // the nodes we've moved aside. Those still have their original path from
      // before the layout because they've been hidden as well.

      // In this case we simply apply an EdgeRouter to calculate suitable routes
      // to the nodes that we moved aside.
      var edgeRouter = new EdgeRouter();
      // We also have to tell the EdgeRouter which edges to route (in this case,
      // edges at affected nodes, so we tell it the affected nodes)
      var edgeRouterData = edgeRouter.CreateLayoutData(graph);
      edgeRouterData.Scope.IncidentNodes.Source = asideNodes;

      // Finally, EdgeRouter can calculate routes for the remaining edges.
      graph.ApplyLayout(edgeRouter, edgeRouterData);
    }
  }
}
