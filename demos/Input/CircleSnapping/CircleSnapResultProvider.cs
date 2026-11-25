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

using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.CircleSnapping
{
  /// <summary>
  ///   Collects snap results for the demo's custom snap references.
  /// </summary>
  internal sealed class CircleSnapResultProvider : NodeSnapResultProvider
  {
    /// <summary>
    ///   Collects snap results for circular snap references.
    /// </summary>
    protected override void CollectCircleSnapResults(
        GraphSnapContext context,
        CollectSnapResultsEventArgs args,
        SnapCircle snapCircle,
        RectD suggestedLayout,
        INode node) {
      if (snapCircle is NodeSnapCircle nodeSnapCircle) {
        var graph = args.Context.GetGraph()!;
        foreach (var inEdge in graph.InEdgesAt(node)) {
          if ((INode) inEdge.SourcePort.Owner == nodeSnapCircle.Node) {
            snapCircle.CollectPointSnapResult(args, node, suggestedLayout.GetCenter(), 10);
          }
        }
      } else {
        base.CollectCircleSnapResults(context, args, snapCircle, suggestedLayout, node);
      }
    }

    /// <summary>
    ///   Collects snap results for snap lines radiating out from a given node.
    /// </summary>
    protected override void CollectLineSnapResults(
        GraphSnapContext context,
        CollectSnapResultsEventArgs args,
        SnapLine snapLine,
        RectD suggestedLayout,
        INode node) {
      if (snapLine is NodeSnapLine nodeSnapLine) {
        var parent = GetParent(args, node);
        if (parent == nodeSnapLine.Node) {
          snapLine.CollectPointSnapResult(args, node, suggestedLayout.GetCenter());
        }
      } else if (snapLine is EqualAngleSnapLine equalAngleSnapLine) {
        var parent = GetParent(args, node);
        if (parent == equalAngleSnapLine.Parent) {
          var result = snapLine.CollectPointSnapResult(args, node, suggestedLayout.GetCenter());
          if (result != null) {
            result.ObjectRenderer = EqualAngleObjectRenderer.Instance;
          }
        }
      } else {
        base.CollectLineSnapResults(context, args, snapLine, suggestedLayout, node);
      }
    }

    private static INode? GetParent(CollectSnapResultsEventArgs args, INode node) =>
        args.Context.GetGraph()!.InEdgesAt(node).FirstOrDefault() is { } edge ? edge.SourcePort.Owner as INode : null;
  }
}
