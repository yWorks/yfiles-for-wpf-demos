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

using yWorks.Graph;

namespace yWorks.Demo.GraphAnalysis
{
  internal enum NodeType
  {
    Start,
    End
  }

  /// <summary>
  ///   A proper type for the data stored in the <c>tag</c> property of nodes.
  /// </summary>
  /// <param name="Type">The type of the node if it is a start or end node for an algorithm.</param>
  internal sealed record NodeTag(NodeType? Type = null);

  internal static class DemoTypes
  {
    /// <summary>
    ///   Retrieves the item's tag as a properly typed object.
    /// </summary>
    internal static NodeTag? GetTag(IModelItem item) => item.Tag as NodeTag;

    /// <summary>
    ///   Replaces the node's tag with a copy containing the given type.
    ///   A new instance is required for undo to work.
    /// </summary>
    internal static void SetType(INode node, NodeType type) {
      var tag = GetTag(node) ?? new NodeTag();
      if (tag.Type != type) {
        node.Tag = tag with { Type = type };
      }
    }

    /// <summary>
    ///   Resets the type on the tag.
    /// </summary>
    internal static void ResetType(INode node) {
      if (GetTag(node) is { Type: not null } tag) {
        node.Tag = tag with { Type = null };
      }
    }
  }
}
