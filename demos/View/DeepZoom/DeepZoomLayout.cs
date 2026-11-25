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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;

namespace yWorks.Demo.DeepZoom;

internal static class DeepZoomLayout
{
  private static readonly HierarchicalLayout HierarchicalLayout = new();
  private static readonly OrganicLayout OrganicLayout = new() { DefaultMinimumNodeDistance = 80 };
  private static readonly CircularLayout CircularLayout = new();

  private static readonly ILayoutAlgorithm[] Layouts = [
      HierarchicalLayout,
      HierarchicalLayout,
      OrganicLayout,
      HierarchicalLayout,
      CircularLayout,
      OrganicLayout,
      CircularLayout,
      OrganicLayout,
      HierarchicalLayout
  ];

  /// <summary>
  ///   Applies different layouts to each of the group layers.
  ///   Each layer is considered its own graph and consequently, all layers are arranged
  ///   on top of each other in the master graph. However, with the deep zoom feature,
  ///   only a single layer is visible at a time.
  /// </summary>
  /// <param name="foldingView">The top level folding view</param>
  public static void ApplyDeepZoomLayout(IFoldingView foldingView) {
    var foldingManager = foldingView.Manager;
    var masterGraph = foldingManager.MasterGraph;

    var queue = new Queue<(INode? Root, int Layer)>();
    queue.Enqueue((foldingView.LocalRoot, 0));
    while (queue.Count > 0) {
      var (root, layer) = queue.Dequeue();
      ApplyLayout(foldingManager, root, layer);

      var childGroups = masterGraph.GetChildren(root).Where(child => masterGraph.IsGroupNode(child));
      foreach (var childGroup in childGroups) {
        queue.Enqueue((childGroup, layer + 1));
      }
    }
  }

  private static void ApplyLayout(FoldingManager foldingManager, INode? root, int layer) {
    // create a temporary view of the graph inside the given root with all child groups collapsed
    var localFoldingView = foldingManager.CreateFoldingView(root, _ => false);
    try {
      localFoldingView.Graph.ApplyLayout(GetLayoutForLayer(layer));
    } finally {
      localFoldingView.Dispose();
    }
  }

  private static ILayoutAlgorithm GetLayoutForLayer(int layer) =>
      layer < Layouts.Length ? Layouts[layer] : HierarchicalLayout;
}
