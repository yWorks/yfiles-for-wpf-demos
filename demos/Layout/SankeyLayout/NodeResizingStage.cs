/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Algorithms;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace SankeyLayout
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

    public override void ApplyLayout(LayoutGraph graph) {
      foreach (var node in graph.Nodes) {
        AdjustNodeSize(node, graph);
      }

      // run the core layout
      ApplyLayoutCore(graph);
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
      var edgeThicknessDP = graph.GetDataProvider(HierarchicLayout.EdgeThicknessDpKey);
      if (edgeThicknessDP != null) {
        foreach (var edge in node.Edges) {
          var thickness = edgeThicknessDP.GetDouble(edge);

          var spc = PortConstraint.GetSPC(graph, edge);
          if (edge.Source == node && spc != null && spc.Strong) {
            var sourcePoint = graph.GetSourcePointRel(edge);
            width = Math.Max(width, Math.Abs(sourcePoint.X) * 2 + thickness);
            height = Math.Max(height, Math.Abs(sourcePoint.Y) * 2 + thickness);
          }

          var tpc = PortConstraint.GetTPC(graph, edge);
          if (edge.Target == node && tpc != null && tpc.Strong) {
            var targetPoint = graph.GetTargetPointRel(edge);
            width = Math.Max(width, Math.Abs(targetPoint.X) * 2 + thickness);
            height = Math.Max(height, Math.Abs(targetPoint.Y) * 2 + thickness);
          }
        }
      }
      graph.SetSize(node, width, height);
    }

    private double CalcRequiredSpace(IEnumerable<Edge> edges, LayoutGraph graph) {
      double requiredSpace = 0;
      var edgeThicknessDP = graph.GetDataProvider(HierarchicLayout.EdgeThicknessDpKey);
      var count = 0;
      foreach (var edge in edges) {
        var thickness = edgeThicknessDP?.GetDouble(edge) ?? 0;
        requiredSpace += Math.Max(thickness, 1);
        count++;
      }
      requiredSpace += (count - 1) * MinimumPortDistance;
      requiredSpace += 2 * PortBorderGapRatio * MinimumPortDistance;
      return requiredSpace;
    }
  }
}
