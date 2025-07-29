/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.PreferredLabelPlacement
{
  /// <summary>
  ///   As our demo graph is no tree, we need a
  ///   <see cref="TreeReductionStage"/>.
  /// </summary>
  /// <remarks>
  ///   To layout the non-tree edges and labels we combine an
  ///   <see cref="EdgeRouter"/> for the edges with the
  ///   <see cref="GreedyMISLabeling"/> labeling algorithm for the labels
  /// </remarks>
  internal class NonTreeEdgeRouterStage : LayoutStageBase
  {
    private readonly EdgeRouter nonTreeEdgeRouter;
    private readonly GenericLabeling nonTreeEdgeLabelLayout;

    public NonTreeEdgeRouterStage() {
      var polylineRouter = new EdgeRouter { Rerouting = true };
      polylineRouter.DefaultEdgeDescriptor.EdgeRouterCosts.BendCost = 3;
      polylineRouter.DefaultEdgeDescriptor.EdgeRouterCosts.EdgeCrossingCost = 5;
      polylineRouter.DefaultEdgeDescriptor.RoutingStyle = RoutingStyle.Octilinear;
      nonTreeEdgeRouter = polylineRouter;
      nonTreeEdgeLabelLayout = CreateFastLabeling();
    }

    /// <inheritdoc/>
    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      // first layout the non-tree edges
      nonTreeEdgeRouter.ApplyLayout(graph);

      // the tree reduction stage only prepares a data provider to mark the non-tree edges but we need
      // a provider to mark the labels of all non-tree edges:

      var nonTreeEdgeDp = graph.Context.GetItemData(LayoutKeys.RouteEdgesDataKey);
      var labelingData = nonTreeEdgeLabelLayout.CreateLayoutData(graph);
      labelingData.Scope.EdgeLabels.Predicate = edgeLabel => nonTreeEdgeDp[edgeLabel.Owner];
      graph.ApplyLayout(nonTreeEdgeLabelLayout, labelingData);
    }

    internal static GenericLabeling CreateFastLabeling() {
      return new GenericLabeling { StopDuration = TimeSpan.Zero, Scope = LabelingScope.EdgeLabels };
    }
  }
}
