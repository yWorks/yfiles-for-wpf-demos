/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Xml;
using System.Xml.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.GraphML;
using yWorks.Utils;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  /// An utility class to add z-order consistency for nodes to a <see cref="yWorks.Controls.GraphControl"/>.
  /// </summary>
  public class ZOrderSupport : IComparer<INode>
  {
    private readonly IFoldingView FoldingView;
    internal readonly IGraph MasterGraph;
    private readonly GroupingSupport MasterGroupingSupport;

    private readonly IDictionary<INode, int> zOrders = new Dictionary<INode, int>();
    private readonly IDictionary<INode, int> tempZOrders = new Dictionary<INode, int>();
    private readonly IDictionary<INode, INode> tempParents = new Dictionary<INode, INode>();

    /// <summary>
    /// The <see cref="yWorks.Controls.GraphControl"/> to work on.
    /// </summary>
    public GraphControl GraphControl { get; }
    
    /// <summary>
    /// A flag indicating if nodes added to the master group should get a z-order assigned. 
    /// </summary>
    public bool AddZOrderForNewNodes { get; set; }

    /// <summary>
    /// Creates a new instance and installs it on the given <see cref="GraphControl"/>.
    /// </summary>
    /// <param name="graphControl">The control to install the z-order support on.</param>
    public ZOrderSupport(GraphControl graphControl) {
      GraphControl = graphControl;

      // make this ZOrderSupport available via lookup of the view graph and master graph
      AddZOrderSupportLookup(graphControl.Graph, this);
      FoldingView = graphControl.Graph.GetFoldingView();
      MasterGraph = FoldingView != null ? FoldingView.Manager.MasterGraph : graphControl.Graph;
      MasterGroupingSupport = MasterGraph.GetGroupingSupport();
      if (FoldingView != null) {
        AddZOrderSupportLookup(MasterGraph, this);
      }

      // use this ZOrderSupport as node comparer for the visualization
      graphControl.GraphModelManager.NodeManager.Comparer = this;
      // The ItemModelManager.Comparer needs the user objects to be accessible from the main canvas objects
      graphControl.GraphModelManager.ProvideUserObjectOnMainCanvasObject = true;

      // keep labels at their owners for this demo
      graphControl.GraphModelManager.LabelLayerPolicy = LabelLayerPolicy.AtOwner;

      // configure the GraphMLIOHandler to support writing and parsing node z-orders to/from GraphML
      ConfigureGraphMLIOHandler(graphControl.GraphMLIOHandler);
      
      // configure the edit mode to keep z-order consistent during grouping/folding/reparenting gestures
      var geim = new GraphEditorInputMode();
      ConfigureInputMode(geim);
      graphControl.InputMode = geim;
      graphControl.Graph.GetDecorator().NodeDecorator.PositionHandlerDecorator.SetFactory(node => new ZOrderNodePositionHandler(node));

      // configure the clipboard that transfers the relative z-order of copied/cut nodes
      ConfigureGraphClipboard(graphControl.Clipboard);
      
      // listen for new nodes to assign an initial z-order
      MasterGraph.NodeCreated += OnNodeCreated;
      AddZOrderForNewNodes = true;
    }

    /// <summary>
    /// Decorates the lookup of <paramref name="graph"/> to provide the <paramref name="zOrderSupport"/>.
    /// </summary>
    private static void AddZOrderSupportLookup(IGraph graph, ZOrderSupport zOrderSupport) {
      var graphLookupDecorator = graph.Lookup<ILookupDecorator>();
      new LookupDecorator<IGraph, ZOrderSupport>(graphLookupDecorator, true, true).SetImplementation(zOrderSupport);
    }

    #region Comparer implementation
    
    public int Compare(INode x, INode y) {
      var masterX = GetMasterNode(x);
      var masterY = GetMasterNode(y);

      // the stored z-order is a partial order between children of the same parent
      var parentX = GetParent(masterX);
      var parentY = GetParent(masterY);
      if (parentX == parentY) {
        // for a common parent we can just compare the z-order values
        return ZOrderOf(masterX) - ZOrderOf(masterY);
      }
      if (MasterGroupingSupport.IsDescendant(masterX, masterY)) {
        return 1;
      }
      if (MasterGroupingSupport.IsDescendant(masterY, masterX)) {
        return -1;
      }
      // there is no common parent so find the ancestors that have a common parent
      var nca = MasterGroupingSupport.GetNearestCommonAncestor(masterX, masterY);
      var pathToRootX = MasterGroupingSupport.GetPathToRoot(masterX);
      var pathToRootY = MasterGroupingSupport.GetPathToRoot(masterY);

      var ancesterX = nca == null ? pathToRootX.Last() : pathToRootX[pathToRootX.IndexOf(nca) - 1];
      var ancesterY = nca == null ? pathToRootY.Last() : pathToRootY[pathToRootY.IndexOf(nca) - 1];
      // for these ancestors we can now compare the z-order values
      return ZOrderOf(ancesterX) - ZOrderOf(ancesterY);
    }
    
    #endregion

    #region Basic z-order operations
    
    /// <summary>
    /// Gets the z-order value stored for <paramref name="node"/>.
    /// </summary>
    public int GetZOrder(INode node) {
      return ZOrderOf(GetMasterNode(node));
    }
    
    /// <summary>
    /// Sets the new z-order value stored for <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="IUndoUnit"/> for the changed z-order is added as well if undo is
    /// <see cref="GraphExtensions.IsUndoEngineEnabled">enabled</see>.
    /// </remarks>
    public void SetZOrder(INode key, int newZOrder) {
      var master = GetMasterNode(key);
      int oldZOrder = 0;
      zOrders.TryGetValue(master, out oldZOrder);
      if (oldZOrder != newZOrder) {
        if (MasterGraph.IsUndoEngineEnabled()) {
          MasterGraph.AddUndoUnit("Undo Z-order Change", "Redo Z-order Change",
              () => {
                SetZOrderCore(master, oldZOrder);
                Update(master);
              },() => {
                SetZOrderCore(master, newZOrder);
                Update(master);
              });
        }
        SetZOrderCore(master, newZOrder);
        OnZOrderChanged(key, newZOrder, oldZOrder);
      }
    }

    internal void SetZOrderCore(INode master, int data) {
      zOrders[master] = data;
    }
    
    /// <summary>
    /// Arranges the <paramref name="node"/> according to its <see cref="GetZOrder">z-order</see>.
    /// </summary>
    public void Update(INode node) {
      // the update call triggers a new installation of the node visualization that considers the z-order
      GraphControl.GraphModelManager.Update(GetViewNode(node));
    }
    
    /// <summary>
    /// Sets new ascending z-orders for <paramref name="viewNodes"/> starting from <paramref name="zOrder"/>
    /// and sorts their <see cref="GraphModelManager.GetMainCanvasObject">canvas objects</see> as well.
    /// </summary>
    /// <param name="viewNodes">The children of one group node in their new z-order.</param>
    /// <param name="zOrder">The z-order to start the new z-orders from.</param>
    public void ArrangeNodes(IEnumerable<INode> viewNodes, int zOrder = 0) {
      ICanvasObject prev = null;
      foreach (var node in viewNodes) {
        SetZOrder(GetMasterNode(node), zOrder++);
        var canvasObject = GraphControl.GraphModelManager.GetMainCanvasObject(node);
        if (prev == null) {
          canvasObject.ToBack();
        } else {
          canvasObject.Above(prev);
        }
        prev = canvasObject;
      }
    }
    
    #endregion
    
    #region Z-Order Changed Event
    
    private void OnZOrderChanged(INode node, int newValue, int oldValue = 0) {
      if (ZOrderChanged != null) {
        ZOrderChanged(this, new ZOrderChangedEventArgs(node, newValue, oldValue));
      }
    }

    /// <summary>
    /// Occurs when the z-order of a a node has been changed.
    /// </summary>
    public event EventHandler<ZOrderChangedEventArgs> ZOrderChanged;
    
    #endregion
    
    #region Support for temporary z-orders

    /// <summary>
    /// Sets a temporary z-order for <paramref name="node"/> for a temporary <paramref name="tempParent">parent</paramref>.
    /// </summary>
    public void SetTempZOrder(INode node, INode tempParent, int newZOrder, bool force = false) {
      var master = GetMasterNode(node);
      if (force || GetZOrder(master) != newZOrder) {
        tempZOrders[master] = newZOrder;
      }
      var masterParent = tempParent != null ? GetMasterNode(tempParent) : FoldingView?.LocalRoot;
      tempParents[master] = masterParent;
    }

    /// <summary>
    /// Sets normalized z-orders for all children of <paramref name="parent"/> and recurses into child group nodes.
    /// </summary>
    /// <param name="parent">The parent of the child nodes to normalize.</param>
    public void SetTempNormalizedZOrders(INode parent = null) {
      var children = GraphControl.Graph.GetChildren(parent).ToList();
      children.Sort(this);
      int zOrder = 0;
      foreach (var child in children) {
        SetTempZOrder(child, parent, zOrder++);
        if (GraphControl.Graph.IsGroupNode(child)) {
          SetTempNormalizedZOrders(child);
        }
      }
    }
    
    /// <summary>
    /// Removes a temporary z-order for <paramref name="node"/> that has been set previously via <see cref="SetTempZOrder"/>.
    /// </summary>
    public void RemoveTempZOrder(INode node) {
      var master = GetMasterNode(node);
      tempZOrders.Remove(master);
      tempParents.Remove(master);
    }
    
    /// <summary>
    /// Transfers all temporary z-orders that have been set previously via <see cref="SetTempZOrder"/>.
    /// </summary>
    public void ApplyTempZOrders(bool update = false) {
      foreach (var keyValuePair in tempZOrders) {
        SetZOrder(keyValuePair.Key, keyValuePair.Value);
        if (update) {
          Update(keyValuePair.Key);
        }
      }
      ClearTempZOrders();
    }
    
    /// <summary>
    /// Removes all temporary z-orders that has been set previously via <see cref="SetTempZOrder"/>.
    /// </summary>
    public void ClearTempZOrders() {
      tempZOrders.Clear();
      tempParents.Clear();
    }
    
    #endregion

    #region Commands

    public static readonly RoutedUICommand RaiseCommand;
    public static readonly RoutedUICommand LowerCommand;
    public static readonly RoutedUICommand ToFrontCommand;
    public static readonly RoutedUICommand ToBackCommand;

    static ZOrderSupport() {
      RaiseCommand = new RoutedUICommand("Raise", "Raise", typeof(ZOrderSupport));
      LowerCommand = new RoutedUICommand("Lower", "Lower", typeof(ZOrderSupport));
      ToFrontCommand = new RoutedUICommand("ToFront", "ToFront", typeof(ZOrderSupport));
      ToBackCommand = new RoutedUICommand("ToBack", "ToBack", typeof(ZOrderSupport));
    }

    /// <summary>
    /// Registers the z-Order commands.
    /// </summary>
    private void AddCommandHandler(RoutedUICommand command, Action<List<INode>> method, GraphEditorInputMode inputMode) {
      inputMode.KeyboardInputMode.AddCommand(command,
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
    
    /// <summary>
    /// Raises the given <paramref name="nodes"/> above their successor.
    /// </summary>
    /// <param name="nodes">The nodes to raise.</param>
    public void Raise(List<INode> nodes) {
      nodes.Sort(this);

      INode prev = null;
      for (int i = nodes.Count - 1; i >= 0; i--) {
        var node = nodes[i];
        var co = GraphControl.GraphModelManager.GetMainCanvasObject(node);
        var nextCO = co.Next;
        if (nextCO != null) {
          var nextNode = GraphControl.GraphModelManager.GetModelItem(nextCO) as INode;
          if (nextNode != null && nextNode != prev) {
            SwapZOrder(node, nextNode);
            GraphControl.GraphModelManager.Update(node);
          }
        }
        prev = node;
      }
    }

    /// <summary>
    /// Lowers the given <paramref name="nodes"/> below their successor.
    /// </summary>
    /// <param name="nodes">The nodes to lower.</param>
    public void Lower(List<INode> nodes) {
      nodes.Sort(this);

      INode prev = null;
      foreach (var node in nodes) {
        var co = GraphControl.GraphModelManager.GetMainCanvasObject(node);
        var prevCO = co.Previous;
        if (prevCO != null) {
          var prevNode = GraphControl.GraphModelManager.GetModelItem(prevCO) as INode;
          if (prevNode != null && prevNode != prev) {
            SwapZOrder(node, prevNode);
            GraphControl.GraphModelManager.Update(node);
          }
        }
        prev = node;
      }
    }

    /// <summary>
    /// Moves the given nodes to front in their parent group.
    /// </summary>
    /// <remarks>
    /// The order of the nodes to each other is kept stable.
    /// </remarks>
    /// <param name="nodes">The nodes to move to front.</param>
    public void ToFront(List<INode> nodes) {
      foreach (var grouping in nodes.GroupBy(node => GraphControl.Graph.GetParent(node))) {
        var groupNode = grouping.Key;
        var toFrontChildren = grouping.ToList();
        var allChildren = GraphControl.Graph.GetChildren(groupNode).ToList();
        if (toFrontChildren.Count < allChildren.Count) {
          allChildren.Sort(this);
          toFrontChildren.Sort(this);
          allChildren.RemoveAll(node => toFrontChildren.Contains(node));
          var last = allChildren.Last();
          var zOrder = GetZOrder(last) + 1;
          foreach (var node in toFrontChildren) {
            SetZOrder(node, zOrder++);
            Update(node);
          }
        }
      }
    }

    /// <summary>
    /// Moves the given nodes to back in their parent group.
    /// </summary>
    /// <remarks>
    /// The order of the nodes to each other is kept stable.
    /// </remarks>
    /// <param name="nodes">The nodes to move to back.</param>
    public void ToBack(List<INode> nodes) {
      foreach (var grouping in nodes.GroupBy(node => GraphControl.Graph.GetParent(node))) {
        var groupNode = grouping.Key;
        var toBackChildren = grouping.ToList();
        var allChildren = GraphControl.Graph.GetChildren(groupNode).ToList();
        if (toBackChildren.Count < allChildren.Count) {
          allChildren.Sort(this);
          toBackChildren.Sort(this);
          allChildren.RemoveAll(node => toBackChildren.Contains(node));
          var first = allChildren[0];
          var zOrder = GetZOrder(first) - 1;

          for (int i = toBackChildren.Count - 1; i >= 0; i--) {
            var node = toBackChildren[i];
            SetZOrder(node, zOrder--);
            Update(node);
          }
        }
      }
    }
    
    #endregion

    #region NodeCreation callback
    
    /// <summary>
    /// Has to be called after a node has been created to register its z-Order.
    /// </summary>
    private void OnNodeCreated(object sender, ItemEventArgs<INode> e) {
      var undoEngine = GraphControl.Graph.GetUndoEngine();
      if (!AddZOrderForNewNodes || undoEngine.PerformingUndo || undoEngine.PerformingRedo) {
        // don't assign z-orders during undo/redo automatically
        return;
      }
      var newNode = e.Item;
      var parent = GetParent(newNode);
      var newZOrder = MasterGraph.GetChildren(parent).Select(GetZOrder).Max() + 1;
      SetZOrderCore(newNode, newZOrder);
      Update(newNode);
    }
    
    #endregion

    #region Utility methods
    
    /// <summary>
    /// Resets the z-Order.
    /// </summary>
    public void Clear() {
      zOrders.Clear();
      tempZOrders.Clear();
      tempParents.Clear();
      maxRootZOrder = Int32.MinValue;
    }

    /// <summary>
    /// Returns the master item of <paramref name="node"/> if folding is enabled and <paramref name="node"/> itself otherwise.
    /// </summary>
    internal INode GetMasterNode(INode node) {
      var foldingView = GraphControl.Graph.GetFoldingView();
      if (foldingView != null) {
        if (foldingView.Manager.MasterGraph.Contains(node)) {
          // node already part of the master graph
          return node;
        } else {
          // return master of node
          return foldingView.GetMasterItem(node);
        }
      }
      // no folding enabled
      return node;
    }

    /// <summary>
    /// Returns the view item of <paramref name="node"/> if folding is enabled and <paramref name="node"/> itself otherwise.
    /// </summary>
    internal INode GetViewNode(INode node) {
      if (GraphControl.Graph.Contains(node)) {
        // node is already a view node
        return node;
      }
      var foldingView = GraphControl.Graph.GetFoldingView();
      if (foldingView != null && foldingView.Manager.MasterGraph.Contains(node)) {
        // return the view node of node
        return foldingView.GetViewItem(node);
      }
      return null;
    }

    /// <summary>
    /// Gets the parent of a node, including a temporary parent during an input gesture.
    /// </summary>
    /// <param name="masterNode">The node to get the parent for. Must be a master node.</param>
    /// <returns>The parent of the <paramref name="masterNode"/>. Taken from the master graph.</returns>
    internal INode GetParent(INode masterNode) {
      INode parent;
      if (tempParents.TryGetValue(masterNode, out parent)) {
        // temporary parent has precedence over structural parent
        return parent;
      }
      return MasterGraph.GetParent(masterNode);
    }

    /// <summary>
    /// Gets the z-Order value of the given <paramref name="node"/>.
    /// </summary>
    /// <remarks>
    /// If the node has a temporary z-Order the temporary order is returned.
    /// </remarks>
    /// <param name="node">The node to get the z-Order for.</param>
    /// <returns>The z-Order as an integer index.</returns>
    private int ZOrderOf(INode node) {
      int zOrder;
      if (tempZOrders.TryGetValue(node, out zOrder)) {
        // temporary z-order has precedence over basic z-order
        return zOrder;
      }
      if (zOrders.TryGetValue(node, out zOrder)) {
        return zOrder;
      }
      return 0;
    }

    /// <summary>
    /// Swaps the z-Order of the given nodes.
    /// </summary>
    private void SwapZOrder(INode node1, INode node2) {
      var zOrder1 = GetZOrder(node1);
      var zOrder2 = GetZOrder(node2);
      SetZOrder(node1, zOrder2);
      SetZOrder(node2, zOrder1);
    }

    #endregion

    #region Clipboard

    private readonly Dictionary<INode, int> clipboardZOrders = new Dictionary<INode, int>();
    private readonly List<INode> newClipboardItems = new List<INode>();
    private GraphClipboard clipboard;

    public void ConfigureGraphClipboard(GraphClipboard clipboard) {
      this.clipboard = clipboard;
      // copy z-order to item copied to clipboard 
      clipboard.ToClipboardCopier.NodeCopied += OnItemCopiedToClipboard;

      // copy z-order to item copied to graph and collect those copied items
      clipboard.FromClipboardCopier.NodeCopied += OnItemCopiedFromClipboard;
      clipboard.DuplicateCopier.NodeCopied += OnItemCopiedFromClipboard;

      clipboard.ElementsCutting += BeforeCut;
      clipboard.ElementsCopying += BeforeCopy;
      clipboard.ElementsPasting += BeforePaste;
      clipboard.ElementsDuplicating += BeforeDuplicate;
      clipboard.ElementsPasted += AfterPaste;
      clipboard.ElementsDuplicated += AfterDuplicate;
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
    /// Returns the z-order previously stored for the <paramref name="node"/>.
    /// </summary>
    /// <remarks>
    /// The z-order stored in the <see cref="ZOrderSupport"/> is used as fallback for items currently not in the view. 
    /// </remarks>
    /// <param name="node">The item to return the z-order for.</param>
    /// <returns>The z-order of the item.</returns>
    private int GetClipboardZOrder(INode node) {
      int zOrder;
      if (clipboardZOrders.TryGetValue(node, out zOrder)) {
        return zOrder;
      }
      return GetZOrder(node);
    }

    private void BeforeCut(object sender, EventArgs eventArgs) {
      // store the relative z-order for cut or copied items
      StoreInitialZOrder(GraphControl.Graph, clipboard.CreateDefaultCutFilter(GraphControl.Selection, GraphControl.Graph));
    }   
    
    private void BeforeCopy(object sender, EventArgs eventArgs) {
      // store the relative z-order for cut or copied items
      StoreInitialZOrder(GraphControl.Graph, clipboard.CreateDefaultCopyFilter(GraphControl.Selection, GraphControl.Graph));
    }   

    private void BeforePaste(object sender, EventArgs eventArgs) {
      // collect new items in the OnCopiedFromClipboard callbacks
      newClipboardItems.Clear();
    }

    private void AfterPaste(object sender, EventArgs eventArgs) {
      var targetGraph = GraphControl.Graph;
      // set final z-orders of newItems depending on their new parent group
      ArrangeItems(newClipboardItems, targetGraph.GetFoldingView());
    }

    private void BeforeDuplicate(object sender, EventArgs eventArgs) {
      // store the relative z-order for duplicated items
      StoreInitialZOrder(GraphControl.Graph, clipboard.CreateDefaultDuplicateFilter(GraphControl.Selection, GraphControl.Graph));
      // collect new items in the OnCopiedFromClipboard callbacks
      newClipboardItems.Clear();
    }

    private void AfterDuplicate(object sender, EventArgs eventArgs) {
      var sourceGraph = GraphControl.Graph;
      // set final z-orders of newItems depending on their new parent group
      ArrangeItems(newClipboardItems, sourceGraph.GetFoldingView());
    }

    private void StoreInitialZOrder(IGraph sourceGraph, Predicate<IModelItem> filter) {
      // determine the view items involved in the clipboard operation and sort them by their visual z-order
      var items = sourceGraph.Nodes.Where(node => filter(node)).ToList();
      if (items.Count > 1) {
        items.Sort(this);
      }
      clipboardZOrders.Clear();
      var foldingView = sourceGraph.GetFoldingView();
      for (int i = 0; i < items.Count; i++) {
        // in case of folding store relative z-order for master item as it will be used by the GraphCopier
        var item = foldingView != null ? foldingView.GetMasterItem(items[i]) : items[i];
        clipboardZOrders[item] = i;
      }
    }

    private void ArrangeItems(List<INode> newMasterItems, IFoldingView foldingView) {
      // sort new items by the relative z-order transferred in OnCopiedFromClipboard
      newMasterItems.Sort((node1, node2) => GetClipboardZOrder(node1) - GetClipboardZOrder(node2));
      var gmm = GraphControl.GraphModelManager;

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
        SetZOrder(itemsNotInView[i], i);
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
        var nextZOrder = topNodeNotJustAdded != null ? GetClipboardZOrder(topNodeNotJustAdded) + 1 : 0;

        // set new z-orders starting from nextZOrder
        foreach (var node in itemsInGroup) {
          SetZOrder(node, nextZOrder++);
        }
        // update the view using the new z-orders
        foreach (var node in itemsInGroup) {
          Update(node);
        }
      }
    }

    #endregion

    #region Configure Input Mode

    private GraphEditorInputMode inputMode;
    public void ConfigureInputMode(GraphEditorInputMode inputMode) {
      AddCommandHandler(RaiseCommand, Raise, inputMode);
      AddCommandHandler(LowerCommand, Lower, inputMode);
      AddCommandHandler(ToFrontCommand, ToFront, inputMode);
      AddCommandHandler(ToBackCommand, ToBack, inputMode);
      inputMode.DeletingSelection += BeforeDeleteSelection;
      inputMode.DeletedSelection += AfterDeleteSelection;
      inputMode.GroupingSelection += BeforeGrouping;
      inputMode.UngroupingSelection += BeforeUngrouping;
      inputMode.ReparentNodeHandler = new ZOrderReparentHandler(inputMode.ReparentNodeHandler, this);
      ConfigureMoveInputMode(inputMode);
      this.inputMode = inputMode;
    }

    #endregion
    
    #region Grouping operations

    private void BeforeGrouping(object sender, SelectionEventArgs<IModelItem> e) {
      // get all selected nodes and sort by their current z-order
      List<INode> nodes = ((IGraphSelection)e.Selection).SelectedNodes.ToList();
      nodes.Sort(this);

      // set increasing z-orders
      for (var i = 0; i < nodes.Count; i++) {
        SetZOrder(nodes[i], i);
      }
    }

    private void BeforeUngrouping(object sender, SelectionEventArgs<IModelItem> e) {
      var graph = GraphControl.Graph;
      // store all selected nodes that have a parent group
      List<INode> nodes = e.Selection.OfType<INode>().Where(node => graph.GetParent(node) != null).ToList();

      var zOrderSupport = graph.Lookup<ZOrderSupport>();
      // sort selected nodes by their current z-order
      nodes.Sort(zOrderSupport);

      // collect top level nodes
      var topLevelNodes = graph.GetChildren(null).ToList();
      topLevelNodes.Sort(zOrderSupport);

      var newTopLevelNodes = new List<INode>();
      var topLevelIndex = 0;

      INode nextTopLevelNode = null;
      var gs = graph.GetGroupingSupport();

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
    }

    #endregion
    
    #region DeleteSelection
    
    private HashSet<INode> deleteSelectionNewParents;
    private Dictionary<INode, int> absOrder;
    
    private void BeforeDeleteSelection(object sender, SelectionEventArgs<IModelItem> e) {
      var graph = GraphControl.Graph;
      // collect absolute order of all view items
      var zOrderSupport = graph.Lookup<ZOrderSupport>();
      var nodes = graph.Nodes.ToList();
      nodes.Sort(zOrderSupport);
      absOrder = new Dictionary<INode, int>();
      for (var i = 0; i < nodes.Count; i++) {
        absOrder[nodes[i]] = i;
      }
      // collect new parents in ParentChanged events
      deleteSelectionNewParents = new HashSet<INode>();
      // before the group node is removed, all its children get reparented so we listen for each ParentChanged event.
      graph.ParentChanged += DeleteSelection_OnParentChanged;
    }

    private void AfterDeleteSelection(object sender, SelectionEventArgs<IModelItem> e) {
      var graph = GraphControl.Graph;
      graph.ParentChanged -= DeleteSelection_OnParentChanged;
    
      // for each new parent sort their children in previously stored absolute order
      foreach (var newParent in deleteSelectionNewParents) {
        if (newParent == null || graph.Contains(newParent)) {
          // newParent hasn't been removed as well, so sort its children
          var children = graph.GetChildren(newParent).ToList();
          children.Sort(((node1, node2) => absOrder[node1] - absOrder[node2]));
          ArrangeNodes(children);
        }
      }
      deleteSelectionNewParents = null;
    }

    private void DeleteSelection_OnParentChanged(object sender, NodeEventArgs e) {
      var newParent = GraphControl.Graph.GetParent(e.Item);
      deleteSelectionNewParents.Add(newParent);
    }

    #endregion

    #region MoveInputMode

    // Moved nodes that might get reparented.
    private List<INode> MovedNodes;

    // A mapping from moved nodes to their original parents
    private readonly Dictionary<INode, INode> oldParents = new Dictionary<INode, INode>();

    // the maximum z-order of the children of a group node
    private readonly Dictionary<INode, int> maxOldZOrder = new Dictionary<INode, int>();
    // the maximum z-order of top-level nodes
    private int maxRootZOrder = Int32.MinValue;

    public void ConfigureMoveInputMode(GraphEditorInputMode geim) {
      geim.MoveInputMode.DragStarting += MoveStarting;
      geim.MoveInputMode.DragFinished += MoveFinished;
      geim.MoveInputMode.DragCanceled += MoveCanceled;
    }

    #region Initialize fields on MoveStarting

    // Before the move gesture starts, we store all moved nodes, their parents and the maximum z-order of
    // children of their parents
    private void MoveStarting(object sender, InputModeEventArgs e) {
      // store all selected nodes which might get reparented
      MovedNodes = inputMode.GraphSelection.SelectedNodes.ToList();
      // sort this list by their relative z-order
      MovedNodes.Sort(this);

      // calculate max z-order for all group nodes containing any moved node
      foreach (var node in MovedNodes) {
        var parent = GraphControl.Graph.GetParent(node);
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
      var graph = GraphControl.Graph;
      if (parent == null) {
        // top-level nodes
        if (maxRootZOrder == Int32.MinValue && graph.Nodes.Count > 0) {
          maxRootZOrder = graph.GetChildren(null).Select(GetZOrder).Max();
        }
        return maxRootZOrder;
      }
      int maxZOrder;
      if (!maxOldZOrder.TryGetValue(parent, out maxZOrder)) {
        var children = graph.GetChildren(parent);
        maxZOrder = children.Any() ? children.Select(GetZOrder).Max() : 0;
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
      ApplyTempZOrders();
      Cleanup();
    }

    private void MoveCanceled(object sender, InputModeEventArgs e) {
      // clear temporary z-orders and keep the original ones.
      ClearTempZOrders();
      Cleanup();
    }

    private void Cleanup() {
      MovedNodes = null;
      oldParents.Clear();
      maxOldZOrder.Clear();
      maxRootZOrder = Int32.MinValue;
    }

      
    #endregion

    #region GraphMLIOHandler

    public void ConfigureGraphMLIOHandler(GraphMLIOHandler ioHandler) {
      var zOrderKeyDefinitionFound = false;
      int maxExistingZOrder = int.MinValue;

      ioHandler.QueryOutputHandlers += (o, evt) => {
        if (evt.Scope == KeyScope.Node) {
          evt.AddOutputHandler(new ZOrderOutputHandler(this));
        }
      };

      ioHandler.QueryInputHandlers += (o, evt) => {
        if (!evt.Handled &&
            GraphMLIOHandler.MatchesScope(evt.KeyDefinition, KeyScope.Node) &&
            GraphMLIOHandler.MatchesName(evt.KeyDefinition, ZOrderOutputHandler.ZOrderKeyName)) {
          zOrderKeyDefinitionFound = true;
          evt.AddInputHandler(new ZOrderInputHandler(this));
          evt.Handled = true;
        }
      };

      ioHandler.Parsing += (sender, evt) => {
        if (ioHandler.ClearGraphBeforeRead) {
          // clear old z-orders of old graph
          Clear();
        } else {
          maxExistingZOrder = GetOrCalculateMaxZOrder(null);
          ClearTempZOrders();
        }
        AddZOrderForNewNodes = false;
        zOrderKeyDefinitionFound = false;
      };

      ioHandler.Parsed += (sender, evt) => {
        // enable automatic z-order creation for new nodes again
        AddZOrderForNewNodes = true;
        if (!zOrderKeyDefinitionFound) {
          // no z-orders were stored in the GraphML so initialize the nodes in the view
          SetTempNormalizedZOrders(null);
        } else if (!ioHandler.ClearGraphBeforeRead) {
          AppendTempZOrdersToExisting(maxExistingZOrder);
        }
        ApplyTempZOrders(true);
      };
    }

    private void AppendTempZOrdersToExisting(int maxExistingZOrder) {
      if (maxExistingZOrder == int.MinValue) {
        // no nodes in the graph, yet
        return;
      }
      int minNewZOrder = tempZOrders.Values.Min();
      int delta = maxExistingZOrder - minNewZOrder + 1;
      foreach (var key in tempZOrders.Keys.Where(n => MasterGraph.GetParent(n) == null).ToList()) {
        tempZOrders[key] = tempZOrders[key] + delta;
      }
    }


    /// <summary>
    /// An <see cref="IOutputHandler"/> that writes the z-order of nodes, edges and ports.
    /// </summary>
    private class ZOrderOutputHandler : OutputHandlerBase<INode, int>
    {
      public const string ZOrderKeyName = "zOrder";
    
      /// <summary>
      /// The namespace URI for z-order extensions to GraphML.
      /// </summary>
      /// <remarks>This field has the constant value <c>http://www.yworks.com/xml/yfiles-z-order/1.0</c></remarks>
      public const string ZOrderNS = "http://www.yworks.com/xml/yfiles-z-order/1.0";

      private readonly ZOrderSupport zOrderSupport;
    
      public ZOrderOutputHandler(ZOrderSupport zOrderSupport) : base(KeyScope.Node, ZOrderKeyName, KeyType.Int) {
        DefaultValue = 0;
        WriteKeyDefault = false;
        Precedence = WritePrecedence.BeforeChildren;
        SetKeyDefinitionUri(ZOrderNS + "/" + ZOrderKeyName);
        this.zOrderSupport = zOrderSupport;
      }

      protected override void WriteValueCore(IWriteContext context, int data) {
        context.Writer.WriteString(XmlConvert.ToString(data));
      }

      protected override int GetValue(IWriteContext context, INode key) {
        return zOrderSupport.GetZOrder(key);
      }
    }

    /// <summary>
    /// An <see cref="IInputHandler"/> that reads the z-order of nodes, edges and ports.
    /// </summary>
    private class ZOrderInputHandler : InputHandlerBase<INode, int>
    {
      private readonly ZOrderSupport zOrderSupport;

      public ZOrderInputHandler(ZOrderSupport zOrderSupport) {
        this.zOrderSupport = zOrderSupport;
      }
      protected override int ParseDataCore(IParseContext context, XObject node) {
        var zOrder = XmlConvert.ToInt32(((XElement) node).Value);
        return zOrder;
      }

      protected override void SetValue(IParseContext context, INode key, int data) {
        if (key != null) {
          zOrderSupport.SetTempZOrder(key, null, data, true);
        }
      }
      
      public override void ApplyDefault(IParseContext context) {
        var key= context.GetCurrent<INode>();
        SetValue(context, key, 0);
      }
    }

    #endregion
  }
}
