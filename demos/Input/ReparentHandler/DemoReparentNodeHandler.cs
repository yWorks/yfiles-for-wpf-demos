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
using yWorks.Graph;

namespace yWorks.Demo.ReparentHandler
{
  /// <summary>
  ///   Customized variant of the default <see cref="ReparentNodeHandler"/> that
  ///   determines the possible reparenting operations based on the node's tag.
  /// </summary>
  public sealed class DemoReparentNodeHandler : ReparentNodeHandler
  {
    /// <summary>
    ///   In general, this method determines whether the current gesture that
    ///   can be determined through the context is a reparent gesture. In this
    ///   case, it returns true, if the base implementation returns true or if
    ///   the current node is green.
    /// </summary>
    /// <param name="context">The context that provides information about the user input.</param>
    /// <param name="node">The node that will possibly be reparented.</param>
    /// <returns>Whether this is a reparenting gesture.</returns>
    public override bool IsReparentGesture(IInputModeContext context, INode node) =>
        base.IsReparentGesture(context, node) || node.Tag is "green";

    /// <summary>
    ///   In general, this method determines whether the user may detach the
    ///   given node from its current parent in order to reparent it. In this case,
    ///   it returns false for red nodes.
    /// </summary>
    /// <param name="context">The context that provides information about the user input.</param>
    /// <param name="node">The node that is about to be detached from its current parent.</param>
    /// <returns>Whether the node may be detached and reparented.</returns>
    public override bool ShouldReparent(IInputModeContext context, INode node) => node.Tag is not "red";

    /// <summary>
    ///   In general, this method determines whether the provided node
    ///   may be reparented to the given <paramref name="newParent"/>.
    /// </summary>
    /// <param name="context">The context that provides information about the user input.</param>
    /// <param name="node">The node that will be reparented.</param>
    /// <param name="newParent">The potential new parent.</param>
    /// <returns>Whether <paramref name="newParent"/> is a valid new parent for <paramref name="node"/>.</returns>
    public override bool IsValidParent(IInputModeContext context, INode node, INode? newParent) {
      // Obtain the tag from the designated child
      var nodeTag = node.Tag;
      // and from the designated parent.
      var parentTag = newParent?.Tag;
      if (nodeTag == null) {
        // Newly created nodes or nodes without a tag in general can be reparented freely
        return true;
      }

      // Otherwise allow nodes to be moved only if their tags are the same color
      if (nodeTag is string childTag && parentTag is string groupTag) {
        return childTag == groupTag;
      }

      // Finally, if there is no new parent, this is ok, too
      return newParent == null;
    }
  }
}
