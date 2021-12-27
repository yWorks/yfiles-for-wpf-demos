/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace yWorks.Layout.Bpmn
{
  /// <summary>
  /// A profit model for exterior node labels that prefers node sides that are far away
  /// from incoming or outgoing edges.
  /// </summary>
  internal class BpmnLabelProfitModel : IProfitModel
  {
    private readonly LayoutGraph graph;

    public BpmnLabelProfitModel(LayoutGraph graph) {
      this.graph = graph;
    }

    /// <inheritdoc/>
    public virtual double GetProfit(LabelCandidate candidate) {
      if (candidate.Owner is IEdgeLabelLayout) {
        return 1;
      }
      double profit = 0;
      INodeLabelLayout nl = (INodeLabelLayout) candidate.Owner;
      var node = graph.GetOwner(nl);
      var nodeLayout = graph.GetLayout(node);
      var candidateLayout = candidate.BoundingBox;
      
      var isLeft = candidateLayout.X + candidateLayout.Width/2 < nodeLayout.X;
      var isRight = candidateLayout.X + candidateLayout.Width/2 > (nodeLayout.X + nodeLayout.Width);
      var isTop = candidateLayout.Y + candidateLayout.Height/2 < nodeLayout.Y;
      var isBottom = candidateLayout.Y + candidateLayout.Height/2 > (nodeLayout.Y + nodeLayout.Height);

      var horizontalCenter = !isLeft && !isRight;
      var verticalCenter = !isTop && !isBottom;
      if (horizontalCenter && verticalCenter) {
        // candidate is in center -> don't use
        return 0;
      } else if (horizontalCenter || verticalCenter) {
        profit = 0.95;
      } else {
        // diagonal candidates get a bit less profit
        profit = 0.9;
      }
      foreach (var edge in node.Edges) {
        var portLocation = edge.Source == node ? graph.GetSourcePointRel(edge) : graph.GetTargetPointRel(edge);
        if (Math.Abs(portLocation.X) > Math.Abs(portLocation.Y)) {
          // edge at left or right
          if (portLocation.X < 0 && isLeft || portLocation.X > 0 && isRight) {
            if (isTop || isBottom) {
              profit -= 0.03;
            } else {
              // edge at same side as candidate
              profit -= 0.2;
            }
          } else if (horizontalCenter) {
            // candidate is close to the edge but not on the same side
            profit -= 0.01;
          }
        } else {
          // edge at top or bottom
          if (portLocation.Y < 0 && isTop || portLocation.Y > 0 && isBottom) {
            if (isLeft || isRight) {
              profit -= 0.03;
            } else {
              profit -= 0.2;
            }
          } else if (verticalCenter) {
            // candidate is close to the edge but not on the same side
            profit -= 0.01;
          }
        }
      }

      return Math.Max(0, profit);
    }
  }
}