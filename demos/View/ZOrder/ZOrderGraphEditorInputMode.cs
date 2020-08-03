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
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  /// A <see cref="GraphEditorInputMode" /> that tries to keep the relative z-order of nodes during grouping related
  /// gestures.
  /// </summary>
  public class ZOrderGraphEditorInputMode : GraphEditorInputMode
  {
    public ZOrderGraphEditorInputMode(ZOrderSupport support) {
      AddCommandHandler(Raise, support.Raise);
      AddCommandHandler(Lower, support.Lower);
      AddCommandHandler(ToFront, support.ToFront);
      AddCommandHandler(ToBack, support.ToBack);
    }

    #region Z-Order Commands

    public static readonly RoutedUICommand Raise;
    public static readonly RoutedUICommand Lower;
    public static readonly RoutedUICommand ToFront;
    public static readonly RoutedUICommand ToBack;

    static ZOrderGraphEditorInputMode() {
      Raise = new RoutedUICommand("Raise", "Raise", typeof(ZOrderGraphEditorInputMode));
      Lower = new RoutedUICommand("Lower", "Lower", typeof(ZOrderGraphEditorInputMode));
      ToFront = new RoutedUICommand("ToFront", "ToFront", typeof(ZOrderGraphEditorInputMode));
      ToBack = new RoutedUICommand("ToBack", "ToBack", typeof(ZOrderGraphEditorInputMode));
    }

    /// <summary>
    /// Registers the z-Order commands.
    /// </summary>
    private void AddCommandHandler(RoutedUICommand command, Action<List<INode>> method) {
      KeyboardInputMode.AddCommand(command,
          (sender, args) => {
            var nodes = ResolveParameter(args.Parameter);
            if (nodes != null) {
              method.Invoke(nodes);
              args.Handled = true;
            }
          },
          (sender, args) => {
            var nodes = ResolveParameter(args.Parameter);
            args.CanExecute = nodes != null;
            args.Handled = true;
          });
    }

    private List<INode> ResolveParameter(object parameter) {
      if (parameter == null) {
        if (GraphControl.Selection.SelectedNodes.Count > 0) {
          return GraphControl.Selection.SelectedNodes.ToList();
        }
      } else if (parameter is INode) {
        var nodes = new List<INode>();
        nodes.Add(parameter as INode);
        return nodes;
      } else if (parameter is IEnumerable<IModelItem>) {
        var nodes = ((IEnumerable<IModelItem>) parameter).OfType<INode>().ToList();
        return nodes.Count > 0 ? nodes : null;
      }
      return null;
    }

    #endregion

    #region Grouping operations

    /// <summary>
    /// Removes the selected nodes from their parent while keeping their relative z-Order.
    /// </summary>
    public override void UngroupSelection() {
      // store all selected nodes that have a parent group
      List<INode> nodes = GraphSelection.OfType<INode>().Where(node => Graph.GetParent(node) != null).ToList();

      var zOrderSupport = Graph.Lookup<ZOrderSupport>();
      // sort selected nodes by their current z-order
      nodes.Sort(zOrderSupport);

      // collect top level nodes
      var topLevelNodes = Graph.GetChildren(null).ToList();
      topLevelNodes.Sort(zOrderSupport);

      var newTopLevelNodes = new List<INode>();
      var topLevelIndex = 0;

      INode nextTopLevelNode = null;
      var gs = Graph.GetGroupingSupport();

      foreach (var node in nodes) {
        var topLevelAncestor = gs.GetPathToRoot(node).Last();
        while (topLevelAncestor != nextTopLevelNode) {
          nextTopLevelNode = topLevelNodes[topLevelIndex++];
          newTopLevelNodes.Add(nextTopLevelNode);
        }
        newTopLevelNodes.Add(node);
      }

      for (int i = topLevelIndex; i < topLevelNodes.Count; i++) {
        newTopLevelNodes.Add(topLevelNodes[i]);
      }

      for (var i = 0; i < newTopLevelNodes.Count; i++) {
        zOrderSupport.SetZOrder(newTopLevelNodes[i], i);
      }

      base.UngroupSelection();
    }

    /// <summary>
    /// Adds the selected nodes to a new group node while keeping their relative z-Order.
    /// </summary>
    /// <returns>The newly created group node.</returns>
    public override INode GroupSelection() {
      var zOrderSupport = Graph.Lookup<ZOrderSupport>();

      // get all selected nodes and sort by their current z-order
      List<INode> nodes = GraphSelection.SelectedNodes.ToList();
      nodes.Sort(zOrderSupport);

      // set increasing z-orders
      for (var i = 0; i < nodes.Count; i++) {
        zOrderSupport.SetZOrder(nodes[i], i);
      }
      return base.GroupSelection();
    }

    #endregion

    #region Reparenting
    
    protected override MoveInputMode CreateMoveInputMode() {
      // create a specialized MoveInputMode
      return new ZOrderMoveInputMode(this) {
          Priority = 40,
          SnapContext = this.SnapContext
      };
    }
    
    #endregion
    
    #region DeleteSelection
    
    private HashSet<INode> DeleteSelection_NewParents;
    
    /// <summary>
    /// This method deletes the currently selected elements and keeps the relative z-order of all remaining view items.
    /// </summary>
    public override void DeleteSelection() {
      // collect absolute order of all view items
      var zOrderSupport = Graph.Lookup<ZOrderSupport>();
      var nodes = Graph.Nodes.ToList();
      nodes.Sort(zOrderSupport);
      var absOrder = new Dictionary<INode, int>();
      for (var i = 0; i < nodes.Count; i++) {
        absOrder[nodes[i]] = i;
      }
      // collect new parents in ParentChanged events
      DeleteSelection_NewParents = new HashSet<INode>();
      // before the group node is removed, all its children get reparented so we listen for each ParentChanged event.
      Graph.ParentChanged += DeleteSelection_OnParentChanged;
      base.DeleteSelection();
      Graph.ParentChanged -= DeleteSelection_OnParentChanged;
    
      // for each new parent sort their children in previously stored absolute order
      foreach (var newParent in DeleteSelection_NewParents) {
        if (newParent == null || Graph.Contains(newParent)) {
          // newParent hasn't been removed as well, so sort its children
          var children = Graph.GetChildren(newParent).ToList();
          children.Sort(((node1, node2) => absOrder[node1] - absOrder[node2]));
          zOrderSupport.ArrangeNodes(children);
        }
      }
      DeleteSelection_NewParents = null;
    }
    
    private void DeleteSelection_OnParentChanged(object sender, NodeEventArgs e) {
      var newParent = Graph.GetParent(e.Item);
      DeleteSelection_NewParents.Add(newParent);
    }

    #endregion
  }

  #region ZOrderMoveInputMode
  
  /// <summary>
  ///   A <see cref="MoveInputMode" /> that customizes reparent gestures to keep the relative z-order of reparented nodes.
  /// </summary>
  class ZOrderMoveInputMode : MoveInputMode
  {
    // The parent input mode providing the GraphSelection used to determine the moved nodes
    private readonly GraphEditorInputMode Geim;

    // The graph and ZOrderSupport used for the active move gesture
    private IGraph Graph;
    private ZOrderSupport ZOrderSupport;

    // Moved nodes that might get reparented.
    private List<INode> MovedNodes;

    // A mapping from moved nodes to their original parents
    private readonly Dictionary<INode, INode> oldParents = new Dictionary<INode, INode>();

    // the maximum z-order of the children of a group node
    private readonly Dictionary<INode, int> maxOldZOrder = new Dictionary<INode, int>();
    // the maximum z-order of top-level nodes
    private int maxRootZOrder = Int32.MinValue;

    public ZOrderMoveInputMode(GraphEditorInputMode geim) {
      Geim = geim;
      DragStarting += MoveStarting;
      DragFinished += MoveFinished;
      DragCanceled += MoveCanceled;
    }
    
    #region Initialize fields on MoveStarting

    // Before the move gesture starts, we store all moved nodes, their parents and the maximum z-order of
    // children of their parents
    private void MoveStarting(object sender, InputModeEventArgs e) {
      Graph = e.Context.GetGraph();
      ZOrderSupport = Graph.Lookup<ZOrderSupport>();
      
      // store all selected nodes which might get reparented
      MovedNodes = Geim.GraphSelection.SelectedNodes.ToList();
      // sort this list by their relative z-order
      MovedNodes.Sort(ZOrderSupport);

      // calculate max z-order for all group nodes containing any moved node
      foreach (var node in MovedNodes) {
        var parent = Graph.GetParent(node);
        oldParents[node] = parent;
        GetOrCalculateMaxZOrder(parent);
      }
      // calculate max z-order of top-level nodes
      GetOrCalculateMaxZOrder(null);
    }

    /// <summary>
    /// Returns the maximum z-order of the children of <paramref name="parent"/>.
    /// </summary>
    /// <remarks>
    /// If the maximum z-order isn't stored, yet, it is calculated first.
    /// </remarks>
    private int GetOrCalculateMaxZOrder(INode parent) {
      if (parent == null) {
        // top-level nodes
        if (maxRootZOrder == Int32.MinValue) {
          maxRootZOrder = Graph.GetChildren(null).Select(ZOrderSupport.GetZOrder).Max();
        }
        return maxRootZOrder;
      }
      int maxZOrder;
      if (!maxOldZOrder.TryGetValue(parent, out maxZOrder)) {
        var children = Graph.GetChildren(parent);
        maxZOrder = children.Any() ? children.Select(ZOrderSupport.GetZOrder).Max() : 0;
        maxOldZOrder[parent] = maxZOrder;
      }
      return maxZOrder;
    }
    
    #endregion

    /// <summary>
    /// Returns a new z-order for <paramref name="node"/> in its new <paramref name="parent"/>.
    /// </summary>
    /// <remarks>
    /// As all MovedNodes will be reparented to the same parent, those nodes that had this parent initially will
    /// keep their old z-order. Therefore the new z-order can be calculated by adding the old max z-order of parent's
    /// children to the number of nodes in MovedNodes that were below node and would be reparented as well.
    /// </remarks>
    /// <param name="node">The node to returns the z-order for.</param>
    /// <param name="parent">The new parent of node.</param>
    /// <returns>A new z-order for <paramref name="node"/> in its new <paramref name="parent"/>.</returns>
    public int GetZOrderForNewParent(INode node, INode parent) {
      // start the new z-order one after the old children's maximum
      int newZOrder = GetOrCalculateMaxZOrder(parent) + 1;
      foreach (var movedNode in MovedNodes) {
        if (movedNode == node) {
          return newZOrder;
        }
        if (oldParents[movedNode] != parent) {
          // movedNode would be reparented and was below node, so increase the z-order
          newZOrder++;
        }
      }
      return 0;
    }

    private void MoveFinished(object sender, InputModeEventArgs e) {
      // Apply the temporary z-orders for all reparented nodes
      ZOrderSupport.ApplyTempZOrders();
      Cleanup();
    }

    private void MoveCanceled(object sender, InputModeEventArgs e) {
      // clear temporary z-orders and keep the original ones.
      ZOrderSupport.ClearTempZOrders();
      Cleanup();
    }

    private void Cleanup() {
      MovedNodes = null;
      oldParents.Clear();
      maxOldZOrder.Clear();
      maxRootZOrder = Int32.MinValue;
      Graph = null;
      ZOrderSupport = null;
    }
  }
  
  #endregion
}
