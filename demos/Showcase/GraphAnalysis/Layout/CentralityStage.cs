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
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  /// <summary>
  ///   A LayoutStage for changing the sizes of the nodes according to their centrality values.
  /// </summary>
  internal sealed class CentralityStage : LayoutStageBase
  {
    internal static readonly NodeDataKey<double?> CentralityKey = new("centrality");

    internal CentralityStage(ILayoutAlgorithm coreLayout) : base(coreLayout) { }

    /// <summary>
    ///   Applies the layout to the given graph.
    /// </summary>
    /// <param name="graph">the given graph</param>
    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      if (CoreLayout == null) {
        return;
      }

      var centrality = graph.Context.GetItemData(CentralityKey);
      // change the node sizes if a centrality algorithm is applied
      var isCentralityAlgorithm = graph.Nodes.Any(node => centrality?[node] != null);
      if (isCentralityAlgorithm) {
        const double mostCentralSize = 100;
        const double leastCentralSize = 30;
        foreach (var node in graph.Nodes) {
          var centralityId = centrality?[node] ?? double.NaN;
          // centrality values are already normalized in [0,1]
          if (double.IsNaN(centralityId)) {
            centralityId = 0;
          }

          var size = Math.Floor(leastCentralSize + centralityId * (mostCentralSize - leastCentralSize));
          node.Layout.Size = new SizeD(size, size);
        }
      } else {
        foreach (var node in graph.Nodes) {
          node.Layout.Size = new SizeD(30, 30);
        }
      }

      // run the core layout
      CoreLayout.ApplyLayout(graph);
    }
  }
}
