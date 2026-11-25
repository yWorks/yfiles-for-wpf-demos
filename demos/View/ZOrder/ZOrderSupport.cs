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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.GraphML;

namespace yWorks.Demo.ZOrder;

/// <summary>
///   An utility class to add z-order consistency for nodes to a <see cref="GraphControl"/>.
/// </summary>
public sealed class ZOrderSupport
{
  private readonly List<Action<object, ZIndexChangedEventArgs>> zOrderChangedListeners = [];

  // initialize z-order and temporary state maps
  private readonly Dictionary<INode, int> zOrders = [];
  private readonly Dictionary<INode, int> tempZOrders = [];
  private readonly Dictionary<INode, INode?> tempParents = [];
  private readonly Dictionary<INode, INode?> oldParents = [];
  private readonly Dictionary<INode, int> maxOldZOrders = [];
  private readonly Dictionary<INode, int> absoluteOrder = [];
  private readonly HashSet<INode?> deleteSelectionNewParents = [];
  private GraphEditorInputMode inputMode = null!;
  private GroupingSupport masterGroupingSupport = null!;
  private IFoldingView? foldingView;
  private int maxRootZOrder = int.MinValue;
  private List<INode> movedNodes = [];

  /// <summary>
  ///   A flag indicating if nodes added to the master group should get a z-order assigned.
  /// </summary>
  private bool addZOrderForNewNodes = true;

  public GraphControl GraphComponent { get; }

  public IGraph MasterGraph { get; private set; } = null!;

  /// <summary>
  ///   Creates a new instance and installs it on the given <see cref="GraphComponent"/>.
  /// </summary>
  public ZOrderSupport(GraphControl graphComponent) {
    this.GraphComponent = graphComponent;
    InitializeGraphComponent(graphComponent, true);
  }

  private void InitializeGraphComponent(GraphControl component, bool addInputMode) {
    foldingView = component.Graph.GetFoldingView();
    MasterGraph = foldingView?.Manager.MasterGraph ?? component.Graph;
    masterGroupingSupport = new GroupingSupport(MasterGraph);

    // use this ZOrderSupport as node comparer for the visualization
    component.GraphModelManager.NodeManager.Comparer = Compare;
    // The ItemModelManager.Comparer needs the user objects to be accessible from the main render tree elements
    component.GraphModelManager.ProvideRenderTagOnMainRenderTreeElement = true;
    // keep labels at their owners for this demo
    component.GraphModelManager.NodeLabelLayerPolicy = LabelLayerPolicy.AtOwner;

    component.Graph.GetDecorator().Nodes.PositionHandler.AddFactory(node => new ZOrderNodePositionHandler(node, this));

    if (addInputMode) {
      // configure the edit mode to keep z-order consistent during grouping/folding/reparenting gestures
      inputMode = new GraphEditorInputMode();
      ConfigureInputMode(inputMode);
      component.InputMode = inputMode;
    }

    // configure the clipboard that transfers the relative z-order of copied/cut nodes
    component.Clipboard = new ZOrderGraphClipboard(this);

    // listen for new nodes to assign an initial z-order
    MasterGraph.NodeCreated += (_, args) => OnNodeCreated(args.Item);
  }

  public void ConfigureGraphMLIOHandler(GraphMLIOHandler ioHandler) {
    var zOrderKeyDefinitionFound = false;
    var maxExistingZOrder = int.MinValue;

    ioHandler.QueryOutputHandlers += (_, args) => {
      if (args.Scope == KeyScope.Node) {
        args.AddOutputHandler(new ZOrderOutputHandler(this));
      }
    };

    ioHandler.QueryInputHandlers += (_, args) => {
      if (!args.Handled && GraphMLIOHandler.MatchesScope(args.KeyDefinition, KeyScope.Node) &&
          GraphMLIOHandler.MatchesName(args.KeyDefinition, ZOrderOutputHandler.ZOrderKeyName)) {
        zOrderKeyDefinitionFound = true;
        args.AddInputHandler(new ZOrderInputHandler(this));
        args.Handled = true;
      }
    };

    ioHandler.Parsing += (_, _) => {
      // clear old z-orders of old graph
      if (ioHandler.ClearGraphBeforeRead) {
        Clear();
      } else {
        maxExistingZOrder = GetOrCalculateMaxZOrder(null);
        ClearTempZOrders();
      }

      addZOrderForNewNodes = false;
      zOrderKeyDefinitionFound = false;
    };

    ioHandler.Parsed += (_, _) => {
      // enable automatic z-order creation for new nodes again
      addZOrderForNewNodes = true;
      if (!zOrderKeyDefinitionFound) {
        // no z-orders were stored in the GraphML so initialize the nodes in the view
        SetTempNormalizedZOrders(null);
      } else if (!ioHandler.ClearGraphBeforeRead) {
        AppendTempZOrdersToExisting(maxExistingZOrder);
      }

      ApplyTempZOrders();
    };
  }

  private void AppendTempZOrdersToExisting(int maxExistingZOrder) {
    if (maxExistingZOrder == int.MinValue) {
      // no nodes in the graph, yet
      return;
    }

    var minimumNewZOrder = tempZOrders.Values.Min();
    var delta = maxExistingZOrder - minimumNewZOrder + 1;
    foreach (var node in tempZOrders.Keys.Where(node => MasterGraph.GetParent(node) == null).ToArray()) {
      tempZOrders[node] += delta;
    }
  }

  public int Compare(INode x, INode y) {
    var masterX = GetMasterNode(x);
    var masterY = GetMasterNode(y);

    // the stored z-order is a partial order between children of the same parent
    var parentX = GetParent(masterX);
    var parentY = GetParent(masterY);
    if (ReferenceEquals(parentX, parentY)) {
      // for a common parent we can just compare the z-order values
      return ZOrderOf(masterX).CompareTo(ZOrderOf(masterY));
    }

    if (masterGroupingSupport.IsDescendant(masterX, masterY)) {
      return 1;
    }

    if (masterGroupingSupport.IsDescendant(masterY, masterX)) {
      return -1;
    }

    // there is no common parent so find the ancestors that have a common parent
    var commonAncestor = masterGroupingSupport.GetNearestCommonAncestor(masterX, masterY);
    var pathToRootX = masterGroupingSupport.GetAncestors(masterX).ToList();
    var pathToRootY = masterGroupingSupport.GetAncestors(masterY).ToList();
    var ancestorX = commonAncestor == null ? pathToRootX[^1] : pathToRootX[pathToRootX.IndexOf(commonAncestor) - 1];
    var ancestorY = commonAncestor == null ? pathToRootY[^1] : pathToRootY[pathToRootY.IndexOf(commonAncestor) - 1];
    // for these ancestors we can now compare the z-order values
    return ZOrderOf(ancestorX).CompareTo(ZOrderOf(ancestorY));
  }

  /// <summary>
  ///   Gets the z-order value stored for <paramref name="node"/>.
  /// </summary>
  public int GetZOrder(INode node) => ZOrderOf(GetMasterNode(node));

  /// <summary>
  ///   Sets the new z-order value stored for <paramref name="node"/>.
  ///   An undo unit for the changed z-order is added as well if undo is enabled.
  /// </summary>
  public void SetZOrder(INode node, int newZOrder) {
    var master = GetMasterNode(node);
    var oldZOrder = zOrders.GetValueOrDefault(master);
    if (oldZOrder == newZOrder) {
      return;
    }

    if (MasterGraph.IsUndoEngineEnabled()) {
      MasterGraph.AddUndoUnit("Undo z-Order Change", "Redo z-Order Change", () => {
        zOrders[master] = oldZOrder;
        Update(master);
      }, () => {
        zOrders[master] = newZOrder;
        Update(master);
      });
    }

    zOrders[master] = newZOrder;
    OnZIndexChanged(node, newZOrder, oldZOrder);
  }

  private void OnZIndexChanged(IModelItem item, int newValue, int? oldValue) {
    var eventArgs = new ZIndexChangedEventArgs(item, newValue, oldValue);
    foreach (var listener in zOrderChangedListeners) {
      listener(this, eventArgs);
    }
  }

  public void AddZIndexChangedListener(Action<object, ZIndexChangedEventArgs> listener) =>
      zOrderChangedListeners.Add(listener);

  public void RemoveZIndexChangedListener(Action<object, ZIndexChangedEventArgs> listener) =>
      zOrderChangedListeners.Remove(listener);

  /// <summary>
  ///   Arranges the <paramref name="node"/> according to its z-order.
  /// </summary>
  public void Update(INode node) {
    // the update call triggers a new installation of the node visualization that considers the z-order
    var viewNode = GetViewNode(node);
    if (viewNode != null) {
      GraphComponent.GraphModelManager.Update(viewNode);
    }
  }

  /// <summary>
  ///   Sets new ascending z-orders for <paramref name="viewNodes"/> starting from <paramref name="zOrder"/> and sorts their
  ///   render tree elements as well.
  /// </summary>
  public void ArrangeNodes(IEnumerable<INode> viewNodes, int zOrder) {
    IRenderTreeElement? previous = null;
    foreach (var node in viewNodes) {
      SetZOrder(GetMasterNode(node), zOrder++);
      var renderTreeElement = GraphComponent.GraphModelManager.GetMainRenderTreeElement(node);
      if (renderTreeElement == null) {
        continue;
      }

      if (previous == null) {
        renderTreeElement.ToBack();
      } else {
        renderTreeElement.Above(previous);
      }

      previous = renderTreeElement;
    }
  }

  /// <summary>
  ///   Sets a temporary z-order for <paramref name="node"/> under a temporary parent.
  /// </summary>
  public void SetTempZOrder(INode node, INode? tempParent, int newZOrder, bool force = false) {
    var master = GetMasterNode(node);
    if (force || GetZOrder(master) != newZOrder) {
      tempZOrders[master] = newZOrder;
    }

    var masterParent = tempParent != null ? GetMasterNode(tempParent) : foldingView?.LocalRoot;
    tempParents[master] = masterParent;
  }

  /// <summary>
  ///   Sets normalized z-orders for all children of <paramref name="parent"/> and recurses into child group nodes.
  /// </summary>
  public void SetTempNormalizedZOrders(INode? parent) {
    var children = GraphComponent.Graph.GetChildren(parent).ToList();
    children.Sort(Compare);
    var zOrder = 0;
    foreach (var child in children) {
      SetTempZOrder(child, parent, zOrder++);
      if (GraphComponent.Graph.IsGroupNode(child)) {
        SetTempNormalizedZOrders(child);
      }
    }
  }

  /// <summary>
  ///   Removes a temporary z-order for <paramref name="node"/>.
  /// </summary>
  public void RemoveTempZOrder(INode node) {
    var master = GetMasterNode(node);
    tempZOrders.Remove(master);
    tempParents.Remove(master);
  }

  /// <summary>
  ///   Transfers all temporary z-orders that have been set previously.
  /// </summary>
  public void ApplyTempZOrders(bool update = false) {
    foreach (var pair in tempZOrders.ToArray()) {
      SetZOrder(pair.Key, pair.Value);
      if (update) {
        Update(pair.Key);
      }
    }

    ClearTempZOrders();
  }

  /// <summary>
  ///   Removes all temporary z-orders.
  /// </summary>
  public void ClearTempZOrders() {
    tempZOrders.Clear();
    tempParents.Clear();
  }

  public void Raise(List<INode> nodes) {
    nodes.Sort(Compare);
    INode? previous = null;
    for (var i = nodes.Count - 1; i >= 0; i--) {
      var node = nodes[i];
      var renderElement = GraphComponent.GraphModelManager.GetMainRenderTreeElement(node);
      var nextElement = renderElement?.Next;
      if (nextElement != null) {
        var nextNode = GraphComponent.GraphModelManager.GetModelItem(nextElement) as INode;
        if (nextNode != null && !ReferenceEquals(nextNode, previous)) {
          SwapZOrder(node, nextNode);
          GraphComponent.GraphModelManager.Update(node);
        }
      }

      previous = node;
    }
  }

  public void Lower(List<INode> nodes) {
    nodes.Sort(Compare);
    INode? previous = null;
    foreach (var node in nodes) {
      var renderElement = GraphComponent.GraphModelManager.GetMainRenderTreeElement(node);
      var previousElement = renderElement?.Previous;
      if (previousElement != null) {
        var previousNode = GraphComponent.GraphModelManager.GetModelItem(previousElement) as INode;
        if (previousNode != null && !ReferenceEquals(previousNode, previous)) {
          SwapZOrder(node, previousNode);
          GraphComponent.GraphModelManager.Update(node);
        }
      }

      previous = node;
    }
  }

  public void ToFront(List<INode> nodes) => ToFrontOrBack(nodes, true);

  public void ToBack(List<INode> nodes) => ToFrontOrBack(nodes, false);

  private void ToFrontOrBack(List<INode> nodes, bool toFront) {
    foreach (var group in nodes.GroupBy(node => GraphComponent.Graph.GetParent(node))) {
      var groupNode = group.Key;
      var selected = group.ToList();
      var allChildren = GraphComponent.Graph.GetChildren(groupNode).ToList();
      if (selected.Count >= allChildren.Count) {
        continue;
      }

      allChildren.Sort(Compare);
      selected.Sort(Compare);
      var remaining = allChildren.Where(node => !selected.Contains(node)).ToList();
      var nextZOrder = toFront ? GetZOrder(remaining[^1]) + 1 : GetZOrder(remaining[0]) - 1;
      if (toFront) {
        foreach (var node in selected) {
          SetZOrder(node, nextZOrder++);
          Update(node);
        }
      } else {
        for (var i = selected.Count - 1; i >= 0; i--) {
          var node = selected[i];
          SetZOrder(node, nextZOrder--);
          Update(node);
        }
      }
    }
  }

  private void OnNodeCreated(INode newNode) {
    var undoEngine = GraphComponent.Graph.GetUndoEngine();
    if (!addZOrderForNewNodes || undoEngine?.PerformingUndo == true || undoEngine?.PerformingRedo == true) {
      // don't assign z-orders during undo/redo automatically
      return;
    }

    var parent = GetParent(newNode);
    var newZOrder = MasterGraph.GetChildren(parent).Select(node => GetZOrder(node) + 1).DefaultIfEmpty(0).Max();
    zOrders[newNode] = newZOrder;
    Update(newNode);
  }

  public void Clear() {
    zOrders.Clear();
    ClearTempZOrders();
  }

  /// <summary>
  ///   Returns the master item of <paramref name="node"/> if folding is enabled and the node itself otherwise.
  /// </summary>
  public INode GetMasterNode(INode node) {
    var view = GraphComponent.Graph.GetFoldingView();
    if (view == null) {
      return node;
    }

    if (view.Manager.MasterGraph.Contains(node)) {
      // node already part of the master graph
      return node;
    }

    // return master of node
    return view.GetMasterItem<INode>(node)!;
  }

  /// <summary>
  ///   Returns the view item of <paramref name="node"/> if folding is enabled and the node itself otherwise.
  /// </summary>
  public INode? GetViewNode(INode node) {
    if (GraphComponent.Graph.Contains(node)) {
      // node is already a view node
      return node;
    }

    var view = GraphComponent.Graph.GetFoldingView();
    if (view != null && view.Manager.MasterGraph.Contains(node)) {
      // return the view node of node
      return view.GetViewItem(node);
    }

    return null;
  }

  private INode? GetParent(INode masterNode) =>
      tempParents.TryGetValue(masterNode, out var parent) ? parent : MasterGraph.GetParent(masterNode);

  /// <summary>
  ///   Returns the temporary z-order of <paramref name="node"/> if one was set, and its stored z-order otherwise.
  /// </summary>
  private int ZOrderOf(INode node) {
    if (tempZOrders.TryGetValue(node, out var temporary)) {
      // temporary z-order has precedence over basic z-order
      return temporary;
    }

    return zOrders.GetValueOrDefault(node);
  }

  private void SwapZOrder(INode node1, INode node2) {
    var zOrder1 = GetZOrder(node1);
    var zOrder2 = GetZOrder(node2);
    SetZOrder(node1, zOrder2);
    SetZOrder(node2, zOrder1);
  }

  // Input mode configuration
  private void ConfigureInputMode(GraphEditorInputMode mode) {
    inputMode = mode;
    AddCommandBinding(GraphCommands.Raise, Raise);
    AddCommandBinding(GraphCommands.Lower, Lower);
    AddCommandBinding(GraphCommands.ToFront, ToFront);
    AddCommandBinding(GraphCommands.ToBack, ToBack);
    mode.DeletingSelection += BeforeDeleteSelection;
    mode.DeletedSelection += AfterDeleteSelection;
    mode.GroupingSelection += BeforeGrouping;
    mode.UngroupingSelection += BeforeUngrouping;
    mode.ReparentNodeHandler = new ZOrderReparentHandler(mode.ReparentNodeHandler, this);

    mode.MoveUnselectedItemsInputMode.DragStarting += MoveStarting;
    mode.MoveUnselectedItemsInputMode.DragFinished += MoveFinished;
    mode.MoveUnselectedItemsInputMode.DragCanceled += MoveCanceled;
  }

  private void AddCommandBinding(ICommand command, Action<List<INode>> action) {
    inputMode.KeyboardInputMode.AddCommandBinding(command, (_, args) => {
      var nodes = ResolveParameter(args.Parameter);
      if (nodes != null) {
        action(nodes);
        args.Handled = true;
      }
    }, (_, args) => {
      args.CanExecute = ResolveParameter(args.Parameter) != null;
      args.Handled = true;
    });
  }

  private List<INode>? ResolveParameter(object? parameter) {
    if (parameter == null) {
      var selection = GraphComponent.Selection.Nodes.ToList();
      return selection.Count > 0 ? selection : null;
    }

    if (parameter is INode node) {
      return [node];
    }

    if (parameter is IModelItem) {
      return [];
    }

    if (parameter is IEnumerable enumerable) {
      var nodes = enumerable.Cast<object>().OfType<INode>().ToList();
      return nodes.Count > 0 ? nodes : null;
    }

    return null;
  }

  // Grouping operations
  private void BeforeGrouping(object? sender, SelectionEventArgs<IModelItem> args) {
    // get all selected nodes and sort by their current z-order
    var nodes = args.Selection.OfType<INode>().ToList();
    nodes.Sort(Compare);
    // set increasing z-orders
    for (var i = 0; i < nodes.Count; i++) {
      SetZOrder(nodes[i], i);
    }
  }

  private void BeforeUngrouping(object? sender, SelectionEventArgs<IModelItem> args) {
    var graph = GraphComponent.Graph;
    // store all selected nodes that have a parent group
    var nodes = args.Selection.OfType<INode>().Where(node => graph.GetParent(node) != null).ToList();
    // sort selected nodes by their current z-order
    nodes.Sort(Compare);
    // collect top level nodes
    var topLevelNodes = graph.GetChildren(null).ToList();
    topLevelNodes.Sort(Compare);
    var newTopLevelNodes = new List<INode>();
    var topLevelIndex = 0;
    INode? nextTopLevelNode = null;
    foreach (var node in nodes) {
      var topLevelAncestor = masterGroupingSupport.GetAncestors(node).LastOrDefault();
      while (!ReferenceEquals(topLevelAncestor, nextTopLevelNode)) {
        nextTopLevelNode = topLevelNodes[topLevelIndex++];
        newTopLevelNodes.Add(nextTopLevelNode);
      }

      newTopLevelNodes.Add(node);
    }

    for (var i = topLevelIndex; i < topLevelNodes.Count; i++) {
      newTopLevelNodes.Add(topLevelNodes[i]);
    }

    for (var i = 0; i < newTopLevelNodes.Count; i++) {
      SetZOrder(newTopLevelNodes[i], i);
    }
  }

  private void BeforeDeleteSelection(object? sender, SelectionEventArgs<IModelItem> args) {
    var graph = GraphComponent.Graph;
    // collect absolute order of all view items
    var nodes = graph.Nodes.ToList();
    nodes.Sort(Compare);
    absoluteOrder.Clear();
    for (var i = 0; i < nodes.Count; i++) {
      absoluteOrder[nodes[i]] = i;
    }

    // Track the parents whose children may be reparented when selected groups are removed.
    deleteSelectionNewParents.Clear();
    foreach (var node in args.Selection.OfType<INode>()) {
      deleteSelectionNewParents.Add(graph.GetParent(node));
    }
  }

  private void AfterDeleteSelection(object? sender, ItemsEventArgs args) {
    var graph = GraphComponent.Graph;
    // for each new parent sort its children in previously stored absolute order
    foreach (var newParent in deleteSelectionNewParents) {
      if (newParent == null || graph.Contains(newParent)) {
        // newParent hasn't been removed as well, so sort its children
        var children = graph.GetChildren(newParent).ToList();
        children.Sort((node1, node2) =>
            absoluteOrder.GetValueOrDefault(node1).CompareTo(absoluteOrder.GetValueOrDefault(node2)));
        ArrangeNodes(children, 0);
      }
    }

    deleteSelectionNewParents.Clear();
  }

  // Move input mode
  private void MoveStarting(object? sender, InputModeEventArgs args) {
    var graph = GraphComponent.Graph;
    // store all selected nodes which might get reparented
    movedNodes = inputMode.MoveUnselectedItemsInputMode.AffectedItems.OfType<INode>().ToList();
    // sort this list by their relative z-order
    movedNodes.Sort(Compare);
    // calculate max z-order for all group nodes containing any moved node
    foreach (var node in movedNodes) {
      var parent = graph.GetParent(node);
      oldParents[node] = parent;
      GetOrCalculateMaxZOrder(parent);
    }

    // calculate max z-order of top-level nodes
    GetOrCalculateMaxZOrder(null);
  }

  /// <summary>
  ///   Returns the maximum z-order of the children of <paramref name="parent"/>.
  ///   If the maximum z-order isn't stored yet, it is calculated first.
  /// </summary>
  private int GetOrCalculateMaxZOrder(INode? parent) {
    var graph = GraphComponent.Graph;
    if (parent == null) {
      if (maxRootZOrder == int.MinValue) {
        maxRootZOrder = graph.GetChildren(null).Select(GetZOrder).DefaultIfEmpty(0).Max();
      }

      return maxRootZOrder;
    }

    if (!maxOldZOrders.TryGetValue(parent, out var maxZOrder)) {
      var children = graph.GetChildren(parent).ToList();
      maxZOrder = children.Count > 0 ? children.Select(GetZOrder).Max() : 0;
      maxOldZOrders[parent] = maxZOrder;
    }

    return maxZOrder;
  }

  /// <summary>
  ///   Returns a new z-order for <paramref name="node"/> in its new <paramref name="parent"/>.
  ///   As all moved nodes will be reparented to the same parent, nodes that had this parent initially keep their
  ///   old z-order. The new z-order accounts for moved nodes that were below <paramref name="node"/> and are reparented too.
  /// </summary>
  public int GetZOrderForNewParent(INode node, INode? parent) {
    // start the new z-order one after the old children's maximum
    var newZOrder = GetOrCalculateMaxZOrder(parent) + 1;
    foreach (var movedNode in movedNodes) {
      if (ReferenceEquals(movedNode, node)) {
        return newZOrder;
      }

      if (oldParents.GetValueOrDefault(movedNode) != parent) {
        // movedNode would be reparented and was below node, so increase the z-order
        return newZOrder++;
      }
    }

    return 0;
  }

  private void MoveFinished(object? sender, InputModeEventArgs args) {
    // Apply the temporary z-orders for all reparented nodes
    ApplyTempZOrders();
    CleanupMove();
  }

  private void MoveCanceled(object? sender, InputModeEventArgs args) {
    // clear temporary z-orders and keep the original ones.
    ClearTempZOrders();
    CleanupMove();
  }

  private void CleanupMove() {
    movedNodes.Clear();
    oldParents.Clear();
    maxOldZOrders.Clear();
    maxRootZOrder = int.MinValue;
  }

  public sealed class ZIndexChangedEventArgs : EventArgs
  {
    public IModelItem Item { get; }

    public int NewZIndex { get; }

    public int? OldZIndex { get; }

    public ZIndexChangedEventArgs(IModelItem item, int newZIndex, int? oldZIndex) {
      Item = item;
      NewZIndex = newZIndex;
      OldZIndex = oldZIndex;
    }
  }

  /// <summary>
  ///   An <see cref="IPositionHandler"/> for nodes that tries to keep the z-order of its moved node relative to other moved
  ///   nodes.
  ///   As the z-order doesn't change for normal move gestures, this class customizes only interactive reparent gestures.
  /// </summary>
  public sealed class ZOrderNodePositionHandler : GroupingNodePositionHandler
  {
    private readonly INode node;
    private readonly ZOrderSupport zOrderSupport;
    private INode? initialParent;
    private INode? currentParent;

    public ZOrderNodePositionHandler(INode node, ZOrderSupport zOrderSupport) : base(node) {
      this.node = node;
      this.zOrderSupport = zOrderSupport;
    }

    public ZOrderNodePositionHandler(INode node, ZOrderSupport zOrderSupport, IPositionHandler wrappedHandler) : base(
        node, wrappedHandler) {
      this.node = node;
      this.zOrderSupport = zOrderSupport;
    }

    public override void InitializeDrag(IInputModeContext context) {
      var graph = zOrderSupport.GraphComponent.Graph;
      // store initial parent of the node...
      initialParent = graph.GetParent(node);
      currentParent = initialParent;
      base.InitializeDrag(context);
    }

    /// <summary>
    ///   Customizes the temporary new parent of <paramref name="node"/> and its z-order in its new render tree group.
    /// </summary>
    protected override void SetCurrentParent(IInputModeContext context, INode node, INode? parent) {
      if (parent != initialParent) {
        // node is temporarily at a new parent
        // the move input mode knows all moved nodes and can provide the new z-order for this node
        var tempZOrder = zOrderSupport.GetZOrderForNewParent(node, parent);
        // 'parent' is only a temporary new parent so the old z-order should be kept but a new temporary one is set
        zOrderSupport.SetTempZOrder(node, parent, tempZOrder);
        base.SetCurrentParent(context, node, parent);
      } else if (parent != currentParent) {
        // node is reset to its initial parent: reset its temporary z-order so the original one is used again
        zOrderSupport.RemoveTempZOrder(node);
        base.SetCurrentParent(context, node, parent);
      }

      currentParent = parent;
    }

    public override void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      base.DragFinished(context, originalLocation, newLocation);
      CleanUp();
    }

    public override void CancelDrag(IInputModeContext context, PointD originalLocation) {
      base.CancelDrag(context, originalLocation);
      CleanUp();
    }

    private void CleanUp() {
      initialParent = null;
      currentParent = null;
    }
  }

  /// <summary>
  ///   An input handler that reads node z-orders from GraphML.
  /// </summary>
  private sealed class ZOrderInputHandler : InputHandlerBase<INode, int>
  {
    private readonly ZOrderSupport zOrderSupport;

    public ZOrderInputHandler(ZOrderSupport zOrderSupport) {
      this.zOrderSupport = zOrderSupport;
    }

    protected override int ParseDataCore(IParseContext context, XObject node) =>
        int.Parse(((XElement) node).Value, CultureInfo.InvariantCulture);

    protected override void SetValue(IParseContext context, INode? key, int data) {
      if (key != null) {
        zOrderSupport.SetZOrder(key, data);
        zOrderSupport.Update(key);
      }
    }

    public override void ApplyDefault(IParseContext context) {
      var key = context.GetCurrent<INode>();
      if (key != null) {
        SetValue(context, key, 0);
      }
    }
  }

  /// <summary>
  ///   An output handler that writes node z-orders to GraphML.
  /// </summary>
  private sealed class ZOrderOutputHandler : OutputHandlerBase<INode, int>
  {
    private readonly ZOrderSupport zOrderSupport;
    public const string ZOrderKeyName = "zOrder";

    /// <summary>
    ///   The namespace URI for z-order extensions to GraphML.
    /// </summary>
    public const string ZOrderNamespace = "http://www.yworks.com/xml/yfiles-z-order/1.0";

    public ZOrderOutputHandler(ZOrderSupport zOrderSupport) : base(KeyScope.Node, ZOrderKeyName, KeyType.Int) {
      this.zOrderSupport = zOrderSupport;
      WriteKeyDefault = false;
      Precedence = WritePrecedence.BeforeChildren;
      SetKeyDefinitionUri(ZOrderNamespace + "/" + ZOrderKeyName);
    }

    protected override void WriteValueCore(IWriteContext context, int data) =>
        context.Writer.WriteString(data.ToString(CultureInfo.InvariantCulture));

    protected override int GetValue(IWriteContext context, INode key) => zOrderSupport.GetZOrder(key);
  }

  /// <summary>
  ///   Delegates reparenting to a handler while performing z-order updates before and after the operation.
  /// </summary>
  private sealed class ZOrderReparentHandler : IReparentNodeHandler
  {
    private readonly IReparentNodeHandler handler;
    private readonly ZOrderSupport zOrderSupport;

    public ZOrderReparentHandler(IReparentNodeHandler handler, ZOrderSupport zOrderSupport) {
      this.handler = handler;
      this.zOrderSupport = zOrderSupport;
    }

    public bool IsReparentGesture(IInputModeContext context, INode node) => handler.IsReparentGesture(context, node);

    public bool ShouldReparent(IInputModeContext context, INode node) => handler.ShouldReparent(context, node);

    public bool IsValidParent(IInputModeContext context, INode node, INode? newParent) =>
        handler.IsValidParent(context, node, newParent);

    public void Reparent(IInputModeContext context, INode node, INode? newParent) {
      // Determine the node's and the parent's representatives in the master graph.
      // This has to be done prior to the reparenting operation, because if the new parent is a
      // folder node (i.e. a closed group node), the given node will be removed from the current
      // view graph as part of the reparenting operation. In this case, the corresponding master nodes
      // cannot be determined anymore after reparenting is done.
      var masterNode = zOrderSupport.GetMasterNode(node);
      var masterParent = newParent == null ? null : zOrderSupport.GetMasterNode(newParent);
      // reparent the node
      handler.Reparent(context, node, newParent);
      // update the node's z-order index
      var children = zOrderSupport.MasterGraph.GetChildren(masterParent).ToList();
      var zIndex = children.Count == 1
          ? 0
          : children.Where(child => child != masterNode).Select(zOrderSupport.GetZOrder).DefaultIfEmpty(-1).Max() + 1;
      zOrderSupport.SetZOrder(masterNode, zIndex);
      var viewNode = zOrderSupport.GetViewNode(masterNode);
      if (viewNode != null) {
        zOrderSupport.GraphComponent.GraphModelManager.Update(viewNode);
      }
    }
  }
}
