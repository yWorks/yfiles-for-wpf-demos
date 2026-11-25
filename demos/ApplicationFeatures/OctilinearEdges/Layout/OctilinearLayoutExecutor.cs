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
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Utils;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   A layout executor that animates the cutting lengths of the edges.
  ///   The animation of cutting lengths only works correctly for edges that result in at least the same number of bends
  ///   after the layout algorithm runs. For edges that have fewer bends after in the layout result, none of the bends are
  ///   animated. This is because the original bend count is not known before the layout algorithm runs.
  /// </summary>
  internal sealed class OctilinearLayoutExecutor : LayoutExecutor
  {
    private readonly GraphControl graphControl;

    /// <summary>
    ///   A mapping from edges to the target lengths of their bends. The array index corresponds to the index of the bend in
    ///   the layout result.
    /// </summary>
    internal IMapper<IEdge, double[]> EdgeBendsCuttingLengths { get; set; } = new DictionaryMapper<IEdge, double[]>();

    internal OctilinearLayoutExecutor(GraphControl graphControl, ILayoutAlgorithm layout) : base(graphControl, layout) {
      this.graphControl = graphControl;
    }

    protected override IAnimation CreateLayoutAnimation() {
      var graphAnimation = base.CreateLayoutAnimation();

      // create an animation for the cutting length of each bend
      var cuttingLengthAnimations = new List<IAnimation>();
      foreach (var edge in graphControl.Graph.Edges) {
        var cuttingLengths = EdgeBendsCuttingLengths[edge];
        if (cuttingLengths == null) {
          continue;
        }

        if (edge.Bends.Count == cuttingLengths.Length) {
          for (var i = 0; i < edge.Bends.Count; i++) {
            cuttingLengthAnimations.Add(new CuttingLengthAnimation(graphControl, edge, edge.Bends[i], i, cuttingLengths,
                AnimationDuration));
          }
        } else if (cuttingLengths.Length > 0) {
          // The original bend count differs from the layout result bend count. The missing bends are created in the
          // original graph animation that runs before these bend animations. So after the first animation tick, the
          // bends are available and can be mapped to their target lengths. This is done lazily by the CuttingLengthAnimation
          // if no bend was passed.
          for (var i = 0; i < cuttingLengths.Length; i++) {
            cuttingLengthAnimations.Add(new CuttingLengthAnimation(graphControl, edge, null, i, cuttingLengths,
                AnimationDuration));
          }
        }
      }

      // run the bend animations in parallel to the original graph animation
      return new[] { graphAnimation, cuttingLengthAnimations.CreateParallelAnimation() }.CreateParallelAnimation();
    }
  }
}
