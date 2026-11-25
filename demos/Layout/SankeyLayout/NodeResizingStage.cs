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
using System.Collections.Generic;
using System.Linq;
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SankeyLayout
{
  public class NodeResizingStage : LayoutStageBase
  {
    private readonly ILayoutAlgorithm layout;

    public NodeResizingStage(ILayoutAlgorithm layout) : base(layout) {
      this.layout = layout;
      LayoutOrientation = LayoutOrientation.LeftToRight;
      PortBorderGapRatio = 0;
      MinimumPortDistance = 0;
    }

    public LayoutOrientation LayoutOrientation { get; set; }

    public double PortBorderGapRatio { get; set; }

    public double MinimumPortDistance { get; set; }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      foreach (var node in graph.Nodes) {
        AdjustNodeSize(node, graph);
      }

      // run the core layout
      CoreLayout?.ApplyLayout(graph);
    }

    private void AdjustNodeSize(Node node, LayoutGraph graph) {
      double width = 60;
      double height = 40;

      var leftEdgeSpace = CalcRequiredSpace(node.InEdges, graph);
      var rightEdgeSpace = CalcRequiredSpace(node.OutEdges, graph);
      if (LayoutOrientation == LayoutOrientation.TopToBottom || LayoutOrientation == LayoutOrientation.BottomToTop) {
        // we have to enlarge the width such that the in-/out-edges can be placed side by side without overlaps
        width = Math.Max(width, leftEdgeSpace);
        width = Math.Max(width, rightEdgeSpace);
      } else {
        // we have to enlarge the height such that the in-/out-edges can be placed side by side without overlaps
        height = Math.Max(height, leftEdgeSpace);
        height = Math.Max(height, rightEdgeSpace);
      }

      // adjust size for edges with strong port constraints
      var edgeThicknessDP = graph.Context.GetItemData(HierarchicalLayout.EdgeThicknessDataKey);
      if (edgeThicknessDP != null) {
        foreach (var edge in node.Edges) {
          var thickness = edgeThicknessDP[edge];

          var spc = GetFirstPortCandidate(edge, true);
          if (edge.Source == node && spc is
              { Type: PortCandidateType.FixedOffset or PortCandidateType.FixedFromSketch }) {
            var sourcePoint = spc.Type == PortCandidateType.FixedFromSketch ? edge.SourcePortOffset : spc.Offset;
            width = Math.Max(width, Math.Abs(sourcePoint.X) * 2 + thickness);
            height = Math.Max(height, Math.Abs(sourcePoint.Y) * 2 + thickness);
          }

          var tpc = GetFirstPortCandidate(edge, false);
          if (edge.Target == node && tpc is
              { Type: PortCandidateType.FixedOffset or PortCandidateType.FixedFromSketch }) {
            var targetPoint = tpc.Type == PortCandidateType.FixedFromSketch ? edge.TargetPortOffset : tpc.Offset;
            width = Math.Max(width, Math.Abs(targetPoint.X) * 2 + thickness);
            height = Math.Max(height, Math.Abs(targetPoint.Y) * 2 + thickness);
          }
        }
      }
      node.Layout.Size = new SizeD(width, height);
    }

    private static LayoutPortCandidate GetFirstPortCandidate(Edge e, bool atSource) {
      var dp = e.Graph!.Context.GetItemData(atSource
          ? EdgePortCandidates.SourcePortCandidatesDataKey
          : EdgePortCandidates.TargetPortCandidatesDataKey);
      return dp?[e]?.Candidates.FirstOrDefault();
    }

    private double CalcRequiredSpace(IEnumerable<Edge> edges, LayoutGraph graph) {
      double requiredSpace = 0;
      var edgeThicknessDP = graph.Context.GetItemData(HierarchicalLayout.EdgeThicknessDataKey);
      var count = 0;
      foreach (var edge in edges) {
        var thickness = edgeThicknessDP?[edge] ?? 0;
        requiredSpace += Math.Max(thickness, 1);
        count++;
      }
      requiredSpace += (count - 1) * MinimumPortDistance;
      requiredSpace += 2 * PortBorderGapRatio * MinimumPortDistance;
      return requiredSpace;
    }
  }
}
