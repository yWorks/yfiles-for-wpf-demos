/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  /// An utility class to add z-order consistency for nodes to a <see cref="yWorks.Controls.GraphControl"/>.
  /// </summary>
  public class ZOrderSupport : IComparer<INode>
  {
    private readonly IFoldingView FoldingView;
    private readonly IGraph MasterGraph;
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
      MasterGraph = FoldingView.Manager.MasterGraph;
      MasterGroupingSupport = MasterGraph.GetGroupingSupport();
      AddZOrderSupportLookup(MasterGraph, this);
      
      // use this ZOrderSupport as node comparer for the visualization
      graphControl.GraphModelManager = new ZOrderGraphModelManager(graphControl, this);
      // keep labels at their owners for this demo
      graphControl.GraphModelManager.LabelLayerPolicy = LabelLayerPolicy.AtOwner;

      // set a custom GraphMLIOHandler that supports writing and parsing node z-orders to/from GraphML
      graphControl.GraphMLIOHandler = new ZOrderGraphMLIOHandler();
      
      // use a custom edit mode to keep z-order consistent during grouping/folding/reparenting gestures
      graphControl.InputMode = new ZOrderGraphEditorInputMode(this);
      graphControl.Graph.GetDecorator().NodeDecorator.PositionHandlerDecorator.SetFactory(node => new ZOrderNodePositionHandler(node));

      // use a custom clipboard that transfers the relative z-order of copied/cut nodes
      graphControl.Clipboard = new ZOrderGraphClipboard(this);
      
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
      var masterX = MasterOf(x);
      var masterY = MasterOf(y);

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
      return ZOrderOf(MasterOf(node));
    }
    
    /// <summary>
    /// Sets the new z-order value stored for <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="IUndoUnit"/> for the changed z-order is added as well if undo is
    /// <see cref="GraphExtensions.IsUndoEngineEnabled">enabled</see>.
    /// </remarks>
    public void SetZOrder(INode key, int newZOrder) {
      var master = MasterOf(key);
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
      GraphControl.GraphModelManager.Update(ViewOf(node));
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
        SetZOrder(MasterOf(node), zOrder++);
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
    public void SetTempZOrder(INode node, INode tempParent, int newZOrder) {
      var master = MasterOf(node);
      var oldZOrder = GetZOrder(master);
      if (oldZOrder != newZOrder) {
        tempZOrders[master] = newZOrder;
      }
      var masterParent = tempParent != null ? MasterOf(tempParent) : FoldingView.LocalRoot;
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
      var master = MasterOf(node);
      tempZOrders.Remove(master);
      tempParents.Remove(master);
    }
    
    /// <summary>
    /// Transfers all temporary z-orders that have been set previously via <see cref="SetTempZOrder"/>.
    /// </summary>
    public void ApplyTempZOrders() {
      foreach (var keyValuePair in tempZOrders) {
        SetZOrder(keyValuePair.Key, keyValuePair.Value);
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

    #region Z-order command methods
    
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
    }

    /// <summary>
    /// Returns the master item of <paramref name="node"/> if folding is enabled and <paramref name="node"/> itself otherwise.
    /// </summary>
    private INode MasterOf(INode node) {
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
    private INode ViewOf(INode node) {
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
    private INode GetParent(INode masterNode) {
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
  }
}
