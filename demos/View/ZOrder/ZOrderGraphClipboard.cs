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

using System.Collections.Generic;
using System.Linq;
using yWorks.Controls;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  ///   A <see cref="GraphClipboard"/> that tries to store the z-order of cut, copied, or duplicated nodes and to apply
  ///   them for the corresponding duplicated or pasted nodes.
  /// </summary>
  public class ZOrderGraphClipboard : GraphClipboard
  {
    public ZOrderSupport ZOrderSupport { get; }

    public GraphControl GraphControl => ZOrderSupport.GraphControl;

    private readonly Dictionary<INode, int> clipboardZOrders = new();
    private readonly List<INode> newClipboardItems = new();

    public ZOrderGraphClipboard(ZOrderSupport zOrderSupport) {
      ZOrderSupport = zOrderSupport;
      // copy z-order to item copied to clipboard
      ToClipboardCopier.NodeCopied += OnItemCopiedToClipboard;

      // copy z-order to item copied to graph and collect those copied items
      FromClipboardCopier.NodeCopied += OnItemCopiedFromClipboard;
      DuplicateCopier.NodeCopied += OnItemCopiedFromClipboard;

      ItemsPasting += BeforePaste;
      ItemsPasted += AfterPaste;
      ItemsDuplicated += AfterDuplicate;
    }

    private void OnItemCopiedToClipboard(object sender, ItemCopiedEventArgs<INode> e) {
      // transfer relative z-order from original node to the copy in the clipboard graph
      clipboardZOrders[e.Copy] = GetClipboardZOrder(e.Original);
    }

    private void OnItemCopiedFromClipboard(object sender, ItemCopiedEventArgs<INode> e) {
      // store new node to use in ArrangeItems
      newClipboardItems.Add(e.Copy);
      // transfer relative z-order from node in the clipboard graph to the new node
      clipboardZOrders[e.Copy] = GetClipboardZOrder(e.Original);
    }

    /// <summary>
    ///   Returns the z-order previously stored for the <paramref name="node"/>.
    /// </summary>
    /// <remarks>
    ///   The z-order stored in the <see cref="ZOrderSupport"/> is used as fallback for items currently not in the view.
    /// </remarks>
    /// <param name="node">The item to return the z-order for.</param>
    /// <returns>The z-order of the item.</returns>
    private int GetClipboardZOrder(INode node) {
      if (clipboardZOrders.TryGetValue(node, out var zOrder)) {
        return zOrder;
      }
      return ZOrderSupport.GetZOrder(node);
    }

    protected override void Copy(
        ClipboardOperationContext copyContext,
        INode targetRootNode,
        ItemCopiedCallback itemCopiedCallback) {
      // store the relative z-order for cut or copied items
      StoreInitialZOrder(copyContext);
      base.Copy(copyContext, targetRootNode, itemCopiedCallback);
    }

    protected override void Duplicate(
        ClipboardOperationContext duplicateContext,
        ItemCopiedCallback itemDuplicatedCallback) {
      // collect new items in the OnItemCopiedFromClipboard callbacks
      newClipboardItems.Clear();
      // store the relative z-order for duplicated items
      StoreInitialZOrder(duplicateContext);
      base.Duplicate(duplicateContext, itemDuplicatedCallback);
    }

    private void BeforePaste(object sender, GraphClipboardEventArgs eventArgs) {
      // collect new items in the OnCopiedFromClipboard callbacks
      newClipboardItems.Clear();
    }

    private void AfterPaste(object sender, GraphClipboardEventArgs eventArgs) {
      var targetGraph = GraphControl.Graph;
      // set final z-orders of newItems depending on their new parent group
      ArrangeItems(newClipboardItems, targetGraph.GetFoldingView());
    }

    private void AfterDuplicate(object sender, GraphClipboardEventArgs eventArgs) {
      var sourceGraph = GraphControl.Graph;
      // set final z-orders of newItems depending on their new parent group
      ArrangeItems(newClipboardItems, sourceGraph.GetFoldingView());
    }

    private void StoreInitialZOrder(ClipboardOperationContext copyContext) {
      // determine the view items involved in the clipboard operation and sort them by their visual z-order
      var items = copyContext.AllViewItems.OfType<INode>().ToList();
      if (items.Count > 1) {
        items.Sort(ZOrderSupport);
      }
      clipboardZOrders.Clear();
      var foldingView = copyContext.SourceFoldingView;
      for (var i = 0; i < items.Count; i++) {
        // in case of folding store relative z-order for master item as it will be used by the GraphCopier
        var item = foldingView != null ? foldingView.GetMasterItem(items[i]) : items[i];
        clipboardZOrders[item] = i;
      }
    }

    private void ArrangeItems(List<INode> newMasterItems, IFoldingView foldingView) {
      // sort new items by the relative z-order transferred in OnCopiedFromClipboard
      newMasterItems.Sort((node1, node2) => GetClipboardZOrder(node1) - GetClipboardZOrder(node2));
      var gmm = GraphControl.GraphModelManager;

      // group new nodes by common parent render tree groups of their main render tree elements
      var itemsNotInView = new List<INode>();
      var groupToItems = new Dictionary<IRenderTreeGroup, List<INode>>();
      foreach (var masterItem in newMasterItems) {
        var viewItem = foldingView != null ? foldingView.GetViewItem(masterItem) : masterItem;
        if (viewItem == null) {
          // new item is not in view (e.g. child of a collapsed folder node)
          itemsNotInView.Add(masterItem);
        } else {
          var rte = gmm.GetMainRenderTreeElement(viewItem);
          if (rte == null) {
            itemsNotInView.Add(masterItem);
          } else {
            var rteParent = rte.Parent;
            List<INode> newNodesInGroup;
            if (!groupToItems.TryGetValue(rteParent, out newNodesInGroup)) {
              newNodesInGroup = new List<INode>();
              groupToItems[rteParent] = newNodesInGroup;
            }
            newNodesInGroup.Add(viewItem);
          }
        }
      }
      // set z-order items not in view just in ascending order
      for (var i = 0; i < itemsNotInView.Count; i++) {
        ZOrderSupport.SetZOrder(itemsNotInView[i], i);
      }

      // for each common parent set ascending z-orders for new nodes
      foreach (var groupItemsPair in groupToItems) {
        var itemsInGroup = groupItemsPair.Value;

        // find the top-most node that was not just added and lookup its z-order
        INode topNodeNotJustAdded = null;
        var walker = groupItemsPair.Key.Last;
        while (walker != null) {
          var node = gmm.GetModelItem(walker) as INode;
          if (node != null && !itemsInGroup.Contains(node)) {
            topNodeNotJustAdded = node;
            break;
          }
          walker = walker.Previous;
        }
        var nextZOrder = topNodeNotJustAdded != null ? GetClipboardZOrder(topNodeNotJustAdded) + 1 : 0;

        // set new z-orders starting from nextZOrder
        foreach (var node in itemsInGroup) {
          ZOrderSupport.SetZOrder(node, nextZOrder++);
        }
        // update the view using the new z-orders
        foreach (var node in itemsInGroup) {
          ZOrderSupport.Update(node);
        }
      }
    }
  }
}
