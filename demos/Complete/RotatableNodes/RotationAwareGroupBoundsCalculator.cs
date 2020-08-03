/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Calculates group bounds taking the rotated layout for nodes which <see cref="RotatableNodeStyleDecorator">support rotation</see>.
  /// </summary>
  class RotationAwareGroupBoundsCalculator : IGroupBoundsCalculator {
    
    /// <summary>
    /// Calculates the minimum bounds for the given group node to enclose all its children plus insets.
    /// </summary>
    public RectD CalculateBounds(IGraph graph, INode groupNode) {
      var bounds = RectD.Empty;
      foreach (var node in graph.GetChildren(groupNode)) {
        var rotatableWrapper = node.Style as RotatableNodeStyleDecorator;
        if (rotatableWrapper != null) {
          // if the node supports rotation: add the outer bounds of the rotated layout
          bounds += rotatableWrapper.GetRotatedLayout(node).GetBounds();
        } else {
          // in all other cases: add the node's layout
          bounds += node.Layout.ToRectD();
        }
      }
      // if we have content: add insets
      return bounds.IsEmpty ? bounds : bounds.GetEnlarged(GetInsets(groupNode));
    }

    /// <summary>
    /// Returns insets to add to apply to the given groupNode.
    /// </summary>
    private static InsetsD GetInsets(INode groupNode) {
      var provider = groupNode.Lookup<INodeInsetsProvider>();
      if (provider != null) {
        // get the insets from the node's insets provider if there is one
        return provider.GetInsets(groupNode);
      }
      // otherwise add 5 to each border
      return new InsetsD(5);
    }
  }
}
