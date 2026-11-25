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

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.CustomSnapping
{
  /// <summary>
  ///   Customizes the grid snapping behavior of <see cref="NodeSnapResultProvider"/> by providing snap results for
  ///   each point of the node's shape path instead of the node's center.
  /// </summary>
  internal sealed class ShapeBasedGridNodeSnapResultProvider : NodeSnapResultProvider
  {
    /// <summary>
    ///   Collects snap results that snap the node to a grid and adds them to the argument.
    /// </summary>
    /// <param name="context">The context in which the snapping is performed</param>
    /// <param name="args">The arguments to add the results to</param>
    /// <param name="snapGrid">The grid snap reference with information that may be useful for visualization</param>
    /// <param name="suggestedLayout">The layout of the node if it would move without snapping</param>
    /// <param name="node">The node that is currently being processed</param>
    protected override void CollectGridSnapResults(
        GraphSnapContext context,
        CollectSnapResultsEventArgs args,
        SnapGrid snapGrid,
        RectD suggestedLayout,
        INode node) {
      // node.Layout isn't updated, yet, so we have to calculate the delta between the new suggested layout and the
      // current node.Layout
      var delta = suggestedLayout.GetTopLeft() - node.Layout.GetTopLeft();

      // get outline of the shape and iterate over its path point
      var geometry = node.Style.Renderer.GetShapeGeometry(node, node.Style);
      var outline = geometry.GetOutline();
      if (outline is null) {
        return;
      }

      var cursor = outline.CreateCursor();
      while (cursor.MoveNext()) {
        // ignore PathType.Close as we had the path point as first point
        // and cursor.CurrentEndPoint is always (0, 0) for PathType.Close
        if (cursor.PathType != PathType.Close) {
          // adjust path point by the delta calculated above and add an according SnapResult
          var endPoint = cursor.CurrentEndPoint + delta;
          CollectGridSnapResult(context, args, snapGrid, endPoint, node);
        }
      }
    }
  }
}
