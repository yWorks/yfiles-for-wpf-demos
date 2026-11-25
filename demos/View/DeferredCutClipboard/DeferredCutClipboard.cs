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
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.ClipboardDeferredCut
{
  /// <summary>
  ///   A special clipboard implementation which doesn't cut elements immediately.
  /// </summary>
  /// <remarks>
  ///   Instead, the elements are stored as "to be cut" and actually removed
  ///   when they are finally pasted.
  ///   Changes in clipboard before they are pasted removes them from the list.
  ///   Note that this implementation supports copying from multiple graphs.
  /// </remarks>
  public sealed class DeferredCutClipboard : GraphClipboard
  {
    /// <summary>
    ///   The collection of elements to cut.
    /// </summary>
    /// <remarks>
    ///   To support multiple source graphs this is implemented
    ///   as a map which maps elements to their source graph.
    /// </remarks>
    private readonly Dictionary<IModelItem, IGraph> itemsToBeCut = [];

    /// <summary>
    ///   Whether the given element is marked as "to be cut".
    /// </summary>
    public bool IsToBeCut(IModelItem item) => itemsToBeCut.ContainsKey(item);

    /// <summary>
    ///   This override sets the PasteOffset of the clipboard to (15,15)
    ///   instead of the default which doesn't move the pasted elements.
    /// </summary>
    /// <remarks>
    ///   Overrides the default cut implementation.
    ///   This is mainly for demonstration purposes, otherwise
    ///   a cut with a subsequent paste would not be visible.
    /// </remarks>
    public override void Cut(IGraph sourceGraph, IEnumerable<IModelItem>? itemsToCut) {
      base.Cut(sourceGraph, itemsToCut);
      PasteOffset = new PointD(15, 15);
    }

    /// <summary>
    ///   This method is called by Cut to remove the element from the source graph.
    /// </summary>
    /// <remarks>
    ///   It is overridden to do nothing, since the actual removal will happen in
    ///   <see cref="OnItemPasted"/>.
    /// </remarks>
    protected override void RemoveItems(IGraph graph, IEnumerable<IModelItem> itemsToRemove) {
      // don't remove anything, instead remember the cut elements in OnItemCut
    }

    /// <summary>
    ///   The method which actually copies elements from one graph to another.
    ///   Invoked by both Cut and Copy.
    /// </summary>
    /// <remarks>
    ///   Overridden to clear the <see cref="itemsToBeCut"/> before the actual copying.
    /// </remarks>
    protected override void Copy(
        ClipboardOperationContext copyContext,
        INode? targetRootNode,
        ItemCopiedCallback? itemCopiedCallback) {
      itemsToBeCut.Clear();
      base.Copy(copyContext, targetRootNode, itemCopiedCallback);
    }

    /// <summary>
    ///   Called for each element after being cut and
    ///   adds the element to the <see cref="itemsToBeCut"/>.
    /// </summary>
    protected override void OnItemCut(ClipboardOperationContext context, IModelItem original, IModelItem copy) {
      itemsToBeCut[original] = context.SourceGraph;
    }

    /// <summary>
    ///   Called for each element after being pasted and
    ///   removes the original if the original is in the <see cref="itemsToBeCut"/> collection.
    /// </summary>
    protected override void OnItemPasted(ClipboardOperationContext context, IModelItem original, IModelItem copy) {
      var sourceItem = IdProvider.GetItem(ClipboardContext!, GetId(original));
      if (sourceItem != null && itemsToBeCut.TryGetValue(sourceItem, out var sourceGraph) &&
          sourceGraph.Contains(sourceItem)) {
        sourceGraph.Remove(sourceItem);
      }
    }
  }
}
