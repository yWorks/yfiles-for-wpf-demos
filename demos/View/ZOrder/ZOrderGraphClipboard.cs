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
using System.Linq;
using yWorks.Controls;
using yWorks.Graph;

namespace yWorks.Demo.ZOrder;

/// <summary>
///   A <see cref="GraphClipboard"/> that stores the z-order of cut, copied or duplicated nodes and applies it to their
///   copies.
/// </summary>
public sealed class ZOrderGraphClipboard : GraphClipboard
{
  private readonly ZOrderSupport zOrderSupport;
  private readonly GraphControl graphComponent;
  private readonly Dictionary<INode, int> clipboardZOrders = [];
  private readonly List<INode> newClipboardItems = [];

  public ZOrderGraphClipboard(ZOrderSupport zOrderSupport) {
    this.zOrderSupport = zOrderSupport;
    graphComponent = zOrderSupport.GraphComponent;
    ToClipboardCopier.NodeCopied += OnCopiedToClipboard;
    FromClipboardCopier.NodeCopied += OnCopiedFromClipboard;
    DuplicateCopier.NodeCopied += OnCopiedFromClipboard;

    ItemsCopying += BeforeCopy;
    ItemsCutting += BeforeCopy;
    ItemsDuplicating += BeforeDuplicate;
    ItemsPasting += BeforePaste;
    ItemsPasted += AfterPaste;
    ItemsDuplicated += AfterDuplicate;
  }

  private void OnCopiedToClipboard(object? sender, ItemCopiedEventArgs<INode> args) {
    // transfer relative z-order from original node to the copy in the clipboard graph
    clipboardZOrders[args.Copy] = GetClipboardZOrder(args.Original);
  }

  private void OnCopiedFromClipboard(object? sender, ItemCopiedEventArgs<INode> args) {
    // store new node to use in ArrangeItems
    newClipboardItems.Add(args.Copy);
    // transfer relative z-order from node in the clipboard graph to the new node
    clipboardZOrders[args.Copy] = GetClipboardZOrder(args.Original);
  }

  /// <summary>
  ///   Returns the z-order previously stored for <paramref name="node"/>.
  ///   The z-order stored in <see cref="ZOrderSupport"/> is used as fallback for items currently not in the view.
  /// </summary>
  private int GetClipboardZOrder(INode node) =>
      clipboardZOrders.TryGetValue(node, out var zOrder) ? zOrder : zOrderSupport.GetZOrder(node);

  private void BeforeCopy(object? sender, GraphClipboardEventArgs args) {
    // store the relative z-order for cut or copied items
    StoreInitialZOrder(args.Items.OfType<INode>(), graphComponent.Graph.GetFoldingView());
  }

  private void BeforeDuplicate(object? sender, GraphClipboardEventArgs args) {
    // collect new items in the OnCopiedFromClipboard callbacks
    newClipboardItems.Clear();
    // store the relative z-order for duplicated items
    StoreInitialZOrder(args.Items.OfType<INode>(), graphComponent.Graph.GetFoldingView());
  }

  private void BeforePaste(object? sender, GraphClipboardEventArgs args) {
    // collect new items in the OnCopiedFromClipboard callbacks
    newClipboardItems.Clear();
  }

  private void AfterPaste(object? sender, GraphClipboardEventArgs args) {
    var targetGraph = graphComponent.Graph;
    // set final z-orders of new items depending on their new parent group
    ArrangeItems(newClipboardItems, targetGraph.GetFoldingView());
  }

  private void AfterDuplicate(object? sender, GraphClipboardEventArgs args) {
    var sourceGraph = graphComponent.Graph;
    // set final z-orders of new items depending on their new parent group
    ArrangeItems(newClipboardItems, sourceGraph.GetFoldingView());
  }

  private void StoreInitialZOrder(IEnumerable<INode> sourceItems, IFoldingView? foldingView) {
    // determine the view items involved in the clipboard operation and sort them by their visual z-order
    var items = sourceItems.ToList();
    if (items.Count > 1) {
      items.Sort((node1, node2) => zOrderSupport.Compare(node1, node2));
    }
    clipboardZOrders.Clear();
    for (var i = 0; i < items.Count; i++) {
      // in case of folding store relative z-order for master item as it will be used by the GraphCopier
      var item = foldingView != null ? foldingView.GetMasterItem(items[i]) : items[i];
      if (item != null) {
        clipboardZOrders[item] = i;
      }
    }
  }

  private void ArrangeItems(List<INode> newMasterItems, IFoldingView? foldingView) {
    // sort new items by the relative z-order transferred in OnCopiedFromClipboard
    newMasterItems.Sort((node1, node2) => GetClipboardZOrder(node1).CompareTo(GetClipboardZOrder(node2)));
    var graphModelManager = graphComponent.GraphModelManager;

    // group new nodes by common parent render tree groups of their main render tree element
    var itemsNotInView = new List<INode>();
    var groupToItems = new Dictionary<IRenderTreeGroup, List<INode>>();
    foreach (var masterItem in newMasterItems) {
      var viewItem = foldingView != null ? foldingView.GetViewItem(masterItem) : masterItem;
      if (viewItem == null) {
        // new item is not in view (e.g. child of a collapsed folder node)
        itemsNotInView.Add(masterItem);
      } else {
        var renderTreeElement = graphModelManager.GetMainRenderTreeElement(viewItem);
        if (renderTreeElement?.Parent is not IRenderTreeGroup renderTreeGroup) {
          itemsNotInView.Add(masterItem);
        } else {
          if (!groupToItems.TryGetValue(renderTreeGroup, out var newNodesInGroup)) {
            newNodesInGroup = [];
            groupToItems.Add(renderTreeGroup, newNodesInGroup);
          }
          newNodesInGroup.Add(viewItem);
        }
      }
    }
    // set z-order items not in view just in ascending order
    for (var i = 0; i < itemsNotInView.Count; i++) {
      zOrderSupport.SetZOrder(itemsNotInView[i], i);
    }

    // for each common parent set ascending z-orders for new nodes
    foreach (var pair in groupToItems) {
      var itemsInGroup = pair.Value;
      // find the top-most node that wasn't just added and lookup its z-order
      INode? topNodeNotJustAdded = null;
      var walker = pair.Key.Last;
      while (walker != null) {
        if (graphModelManager.GetModelItem(walker) is INode node && !itemsInGroup.Contains(node)) {
          topNodeNotJustAdded = node;
          break;
        }
        walker = walker.Previous;
      }
      var nextZOrder = topNodeNotJustAdded != null ? GetClipboardZOrder(topNodeNotJustAdded) + 1 : 0;
      // set new z-orders starting from nextZOrder
      foreach (var node in itemsInGroup) {
        zOrderSupport.SetZOrder(node, nextZOrder++);
      }
      // update the view using the new z-orders
      foreach (var node in itemsInGroup) {
        zOrderSupport.Update(node);
      }
    }
  }
}
