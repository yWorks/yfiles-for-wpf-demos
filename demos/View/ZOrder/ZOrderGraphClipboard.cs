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

using System;
using System.Collections.Generic;
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  /// A <see cref="GraphClipboard"/> that keeps the relative z-order between cut/copied/duplicated nodes when
  /// pasting/inserting them to the graph again. 
  /// </summary>
  public class ZOrderGraphClipboard : GraphClipboard
  {
    private readonly ZOrderSupport Support;

    private readonly Dictionary<INode, int> zOrders = new Dictionary<INode, int>();
    private readonly List<INode> newItems = new List<INode>();

    public ZOrderGraphClipboard(ZOrderSupport support) {
      this.Support = support;
      // copy z-order to item copied to clipboard 
      this.ToClipboardCopier.NodeCopied += OnCopiedToClipboard;

      // copy z-order to item copied to graph and collect those copied items
      this.FromClipboardCopier.NodeCopied += OnCopiedFromClipboard;
      this.DuplicateCopier.NodeCopied += OnCopiedFromClipboard;
    }

    private void OnCopiedToClipboard(object sender, ItemCopiedEventArgs<INode> e) {
      // transfer relative z-order from original node to the copy in the clipboard graph
      zOrders[e.Copy] = GetZOrder(e.Original);
    }
    
    private void OnCopiedFromClipboard(object sender, ItemCopiedEventArgs<INode> e) {
      // store new node to use in ArrangeItems
      newItems.Add(e.Copy);
      // transfer relative z-order from node in the clipboard graph to the new node
      zOrders[e.Copy] = GetZOrder(e.Original);
    }

    /// <summary>
    /// Returns the z-order previously stored for the <paramref name="node"/>.
    /// </summary>
    /// <remarks>
    /// The z-order stored in the <see cref="ZOrderSupport"/> is used as fallback for items currently not in the view. 
    /// </remarks>
    /// <param name="node">The item to return the z-order for.</param>
    /// <returns>The z-order of the item.</returns>
    private int GetZOrder(INode node) {
      int zOrder;
      if (zOrders.TryGetValue(node, out zOrder)) {
        return zOrder;
      }
      return Support.GetZOrder(node);
    }

    public override void Cut(IGraph sourceGraph, Predicate<IModelItem> filter) {
      // store the relative z-order for cut items
      StoreInitialZOrder(sourceGraph, filter);
      base.Cut(sourceGraph, filter);
    }

    public override void Copy(IGraph sourceGraph, Predicate<IModelItem> filter) {
      // store the relative z-order for copied items
      StoreInitialZOrder(sourceGraph, filter);
      base.Copy(sourceGraph, filter);
    }

    public override void Paste(IInputModeContext context, IGraph targetGraph, Predicate<IModelItem> filter = null, ElementCopiedCallback elementPasted = null, Predicate<IModelItem> targetFilter = null) {
      // collect new items in the OnCopiedFromClipboard callbacks
      newItems.Clear();
      base.Paste(context, targetGraph, filter, elementPasted, targetFilter);
      
      // set final z-orders of newItems depending on their new parent group
      ArrangeItems(newItems, targetGraph.GetFoldingView());
    }

    public override void Duplicate(IInputModeContext context, IGraph sourceGraph, Predicate<IModelItem> filter, ElementCopiedCallback elementDuplicated = null) {
      // store the relative z-order for duplicated items
      StoreInitialZOrder(sourceGraph, filter);
      // collect new items in the OnCopiedFromClipboard callbacks
      newItems.Clear();
      base.Duplicate(context, sourceGraph, filter, elementDuplicated);
      
      // set final z-orders of newItems depending on their new parent group
      ArrangeItems(newItems, sourceGraph.GetFoldingView());
    }

    private void StoreInitialZOrder(IGraph sourceGraph, Predicate<IModelItem> filter) {
      // determine the view items involved in the clipboard operation and sort them by their visual z-order
      var items = sourceGraph.Nodes.Where(node => filter(node)).ToList();
      if (items.Count > 1) {
        items.Sort(Support);
      }
      zOrders.Clear();
      var foldingView = sourceGraph.GetFoldingView();
      for (int i = 0; i < items.Count; i++) {
        // in case of folding store relative z-order for master item as it will be used by the GraphCopier
        var item = foldingView != null ? foldingView.GetMasterItem(items[i]) : items[i];
        zOrders[item] = i;
      }
    }

    private void ArrangeItems(List<INode> newMasterItems, IFoldingView foldingView) {
      // sort new items by the relative z-order transferred in OnCopiedFromClipboard
      newMasterItems.Sort((node1, node2) => GetZOrder(node1) - GetZOrder(node2));
      var gmm = Support.GraphControl.GraphModelManager;

      // group new nodes by common parent canvas object groups of their main canvas objects
      var itemsNotInView = new List<INode>();
      var groupToItems = new Dictionary<ICanvasObjectGroup, List<INode>>();
      foreach (var masterItem in newMasterItems) {
        var viewItem = foldingView != null ? foldingView.GetViewItem(masterItem) : masterItem;
        if (viewItem == null) {
          // new item is not in view (e.g. child of a collapsed folder node)
          itemsNotInView.Add(masterItem);
        } else {
          var co = gmm.GetMainCanvasObject(viewItem);
          if (co == null) {
            itemsNotInView.Add(masterItem);
          } else {
            var coGroup = co.Group;
            List<INode> newNodesInGroup;
            if (!groupToItems.TryGetValue(coGroup, out newNodesInGroup)) {
              newNodesInGroup = new List<INode>();
              groupToItems[coGroup] = newNodesInGroup;
            }
            newNodesInGroup.Add(viewItem);
          }
        }
      }
      // set z-order items not in view just in ascending order
      for (var i = 0; i < itemsNotInView.Count; i++) {
        Support.SetZOrder(itemsNotInView[i], i);
      }

      // for each common parent set ascending z-orders for new nodes
      foreach (var groupItemsPair in groupToItems) {
        var itemsInGroup = groupItemsPair.Value;
        
        // find the top-most node that wasn't just added and lookup its z-order
        INode topNodeNotJustAdded = null;
        ICanvasObject walker = groupItemsPair.Key.Last;
        while (walker != null) {
          var node = gmm.GetModelItem(walker) as INode;
          if (node != null && !itemsInGroup.Contains(node)) {
            topNodeNotJustAdded = node;
            break;
          }
          walker = walker.Previous;
        }
        var nextZOrder = topNodeNotJustAdded != null ? GetZOrder(topNodeNotJustAdded) + 1 : 0;

        // set new z-orders starting from nextZOrder
        foreach (var node in itemsInGroup) {
          Support.SetZOrder(node, nextZOrder++);
        }
        // update the view using the new z-orders
        foreach (var node in itemsInGroup) {
          Support.Update(node);
        }
      }
    }
  }
}
