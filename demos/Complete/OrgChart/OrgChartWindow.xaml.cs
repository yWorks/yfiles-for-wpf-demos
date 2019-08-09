/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using yWorks.Controls;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;
using yWorks.Layout.Tree;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Application = System.Windows.Application;
using LineSegment = yWorks.Algorithms.Geometry.LineSegment;

namespace Demo.yFiles.Graph.OrgChart
{
  /// <summary>
  /// Interaction logic for OrgChartWindow.xaml
  /// </summary>
  public partial class OrgChartWindow
  {
    public OrgChartWindow() {
      InitializeComponent();
      hiddenNodesSet = new System.Collections.Generic.HashSet<INode>();
    }


    /// <summary>
    /// The command that can be used by the buttons to show the parent node.
    /// </summary>
    /// <remarks>
    /// This command requires the corresponding <see cref="INode"/> as the <see cref="System.Windows.Input.ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand ShowParentCommand = new RoutedUICommand("Show Parent", "ShowParent",
                                                                            typeof(OrgChartWindow));

    /// <summary>
    /// The command that can be used by the buttons to hide the parent node.
    /// </summary>
    /// <remarks>
    /// This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand HideParentCommand = new RoutedUICommand("Hide Parent", "HideParent",
                                                                            typeof(OrgChartWindow));

    /// <summary>
    /// The command that can be used by the buttons to show the child nodes.
    /// </summary>
    /// <remarks>
    /// This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand ShowChildrenCommand = new RoutedUICommand("Show Children", "ShowChildren",
                                                                            typeof(OrgChartWindow));

    /// <summary>
    /// The command that can be used by the buttons to hide the child nodes.
    /// </summary>
    /// <remarks>
    /// This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand HideChildrenCommand = new RoutedUICommand("Hide Children", "HideChildren",
                                                                            typeof(OrgChartWindow));

    /// <summary>
    /// The command that can be used by the buttons to expand all collapsed nodes.
    /// </summary>
    public static readonly RoutedUICommand ShowAllCommand = new RoutedUICommand("Show All", "ShowAll",
                                                                            typeof(OrgChartWindow));

    /// <summary>
    /// Used by the predicate function to determine which nodes should not be shown.
    /// </summary>
    private readonly System.Collections.Generic.HashSet<INode> hiddenNodesSet;

    /// <summary>
    /// The filtered graph instance that hides nodes from the to create smaller graphs for easier navigation.
    /// </summary>
    private FilteredGraphWrapper filteredGraphWrapper;

    private void OnLoaded(object src, EventArgs eventArgs) {
      InitializeGraph();

      // register command bindings
      graphControl.CommandBindings.Add(new CommandBinding(HideChildrenCommand, HideChildrenExecuted, CanExecuteHideChildren));
      graphControl.CommandBindings.Add(new CommandBinding(ShowChildrenCommand, ShowChildrenExecuted, CanExecuteShowChildren));
      graphControl.CommandBindings.Add(new CommandBinding(HideParentCommand, HideParentExecuted, CanExecuteHideParent));
      graphControl.CommandBindings.Add(new CommandBinding(ShowParentCommand, ShowParentExecuted, CanExecuteShowParent));
      graphControl.CommandBindings.Add(new CommandBinding(ShowAllCommand, ShowAllExecuted, CanExecuteShowAll));

      // disable selection, focus and highlight painting
      GraphControl.SelectionIndicatorManager.Enabled = false;
      GraphControl.FocusIndicatorManager.Enabled = false;
      GraphControl.HighlightIndicatorManager.Enabled = false;

      // we wrap the graph instance by a filtered graph wrapper
      filteredGraphWrapper = new FilteredGraphWrapper(GraphControl.Graph, ShouldShowNode, edge => true);
      GraphControl.Graph = filteredGraphWrapper;

      // now calculate the initial layout
      DoLayout();

      GraphControl.FitGraphBounds();
      LimitViewport();
    }

    /// <summary>
    /// Called when an item has been double clicked.
    /// </summary>
    /// <param name="o">The source of the event.</param>
    /// <param name="itemClickedEventArgs">The event argument instance containing the event data.</param>
    private void OnItemDoubleClicked(object o, ItemClickedEventArgs<IModelItem> itemClickedEventArgs) {
      graphControl.CurrentItem = itemClickedEventArgs.Item;
      ZoomToCurrentItem();
    }

    private void InitializeGraph() {
      // create new nodestyle that delegates to other 
      // styles for different zoom ranges
      var nodeStyle = new NodeControlNodeStyle("EmployeeNodeControlStyle");

      graphControl.CurrentItemChanged += graphControl_CurrentItemChanged;
      
      graphControl.Graph.NodeDefaults.Style = nodeStyle;
      graphControl.Graph.NodeDefaults.Size = new SizeD(250, 100);

      graphControl.Graph.EdgeDefaults.Style = new PolylineEdgeStyle { SmoothingLength = 10 };
    }

    /// <summary>
    /// The predicate used for the FilterGraphWrapper
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool ShouldShowNode(INode obj) {
      return !hiddenNodesSet.Contains(obj);
    }

    private void LimitViewport() {
      GraphControl.UpdateContentRect();
      ViewportLimiter limiter = GraphControl.ViewportLimiter;
      limiter.HonorBothDimensions = false;
      limiter.Bounds = GraphControl.ContentRect.GetEnlarged(100);
    }

    /// <summary>
    /// Gets the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl {
      get { return graphControl; }
    }

    #region Tree Layout Configuration and initial execution

    /// <summary>
    /// Does a tree layout of the graph.
    /// The layout and assistant attributes from the business data of the employees are used to
    /// guide the the layout.
    /// </summary>
    public void DoLayout() {
      IGraph tree = graphControl.Graph;
      var layoutData = CreateLayoutData(tree);
      tree.ApplyLayout(new BendDuplicatorStage(new TreeLayout()), layoutData);
    }

    private static LayoutData CreateLayoutData(IGraph tree) {
      var data = new TreeLayoutData
      {
        NodePlacers = { Delegate = delegate(INode node) {
          var employee = node.Tag as XmlElement;
          if (tree.OutDegree(node) == 0 || employee == null) {
            return null;
          }
          var layout = employee.GetAttribute("layout");
          switch (layout) {
            case "rightHanging":
              return new AssistantNodePlacer() {
                ChildNodePlacer = new DefaultNodePlacer(ChildPlacement.VerticalToRight, RootAlignment.LeadingOnBus, 30, 30) {RoutingStyle = RoutingStyle.ForkAtRoot}
              };
            case "leftHanging":
              return new AssistantNodePlacer() {
                ChildNodePlacer = new DefaultNodePlacer(ChildPlacement.VerticalToLeft, RootAlignment.LeadingOnBus, 30, 30) {RoutingStyle = RoutingStyle.ForkAtRoot}
              };
            case "bothHanging":
              return new AssistantNodePlacer() {
                ChildNodePlacer = new LeftRightNodePlacer() {PlaceLastOnBottom = false}};
            default:
              return new AssistantNodePlacer() {
                ChildNodePlacer = new DefaultNodePlacer(ChildPlacement.HorizontalDownward, RootAlignment.Median, 30, 30)};
          }
        }},
        AssistantNodes = { Delegate = delegate(INode node) {
          var employee = node.Tag as XmlElement;
          var assistant = employee != null ? employee.Attributes["assistant"] : null;
          return assistant != null && assistant.Value == "true";
        }}
      };

      return data;
    }

    #endregion

    #region Command Binding Helper methods

    /// <summary>
    /// Helper method that determines whether the <see cref="ShowParentCommand"/> can be executed.
    /// </summary>
    public void CanExecuteShowChildren(object sender, CanExecuteRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout && filteredGraphWrapper != null) {
        e.CanExecute = filteredGraphWrapper.OutDegree(node) != filteredGraphWrapper.WrappedGraph.OutDegree(node);
      } else {
        e.CanExecute = false;
      }
      e.Handled = true;
    }

    /// <summary>
    /// Handler for the <see cref="ShowChildrenCommand"/>
    /// </summary>
    public void ShowChildrenExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        int count = hiddenNodesSet.Count;
        foreach (var childEdge in filteredGraphWrapper.WrappedGraph.OutEdgesAt(node)) {
          var child = childEdge.GetTargetNode();
          if (hiddenNodesSet.Remove(child)) {
            filteredGraphWrapper.WrappedGraph.SetNodeCenter(child, node.Layout.GetCenter());
            filteredGraphWrapper.WrappedGraph.ClearBends(childEdge);
          }
        }
        RefreshLayout(count, node);
      }
    }

    /// <summary>
    /// Helper method that determines whether the <see cref="ShowParentCommand"/> can be executed.
    /// </summary>
    private void CanExecuteShowParent(object sender, CanExecuteRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout && filteredGraphWrapper != null) {
        e.CanExecute = filteredGraphWrapper.InDegree(node) == 0 && filteredGraphWrapper.WrappedGraph.InDegree(node) > 0;
      } else {
        e.CanExecute = false;
      }
      e.Handled = true;
    }

    /// <summary>
    /// Handler for the <see cref="ShowParentCommand"/>
    /// </summary>
    private void ShowParentExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        int count = hiddenNodesSet.Count;
        foreach (var parentEdge in filteredGraphWrapper.WrappedGraph.InEdgesAt(node)) {
          var parent = parentEdge.GetSourceNode();
          if (hiddenNodesSet.Remove(parent)) {
            filteredGraphWrapper.WrappedGraph.SetNodeCenter(parent, node.Layout.GetCenter());
            filteredGraphWrapper.WrappedGraph.ClearBends(parentEdge);
          }
        }
        RefreshLayout(count, node);
      }
    }

    /// <summary>
    /// Helper method that determines whether the <see cref="HideParentCommand"/> can be executed.
    /// </summary>
    private void CanExecuteHideParent(object sender, CanExecuteRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout && filteredGraphWrapper != null) {
        e.CanExecute = filteredGraphWrapper.InDegree(node) > 0;
      } else {
        e.CanExecute = false;
      }
      e.Handled = true;
    }

    /// <summary>
    /// Handler for the <see cref="HideParentCommand"/>
    /// </summary>
    private void HideParentExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        int count = hiddenNodesSet.Count;

        foreach (var testNode in filteredGraphWrapper.WrappedGraph.Nodes) {
          if (testNode != node && filteredGraphWrapper.Contains(testNode) &&
              filteredGraphWrapper.InDegree(testNode) == 0) {
            // this is a root node - remove it and all children unless 
            HideAllExcept(testNode, node);
          }
        }
        RefreshLayout(count, node);
      }
    }

    /// <summary>
    /// Helper method that determines whether the <see cref="HideChildrenCommand"/> can be executed.
    /// </summary>
    private void CanExecuteHideChildren(object sender, CanExecuteRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout && filteredGraphWrapper != null) {
        e.CanExecute = filteredGraphWrapper.OutDegree(node) > 0;
      } else {
        e.CanExecute = false;
      }
      e.Handled = true;
    }

    /// <summary>
    /// Handler for the <see cref="HideChildrenCommand"/>
    /// </summary>
    private void HideChildrenExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        int count = hiddenNodesSet.Count;
        foreach (var child in filteredGraphWrapper.Successors(node)) {
          HideAllExcept(child, node);
        }
        RefreshLayout(count, node);
      }
    }

    /// <summary>
    /// Helper method that determines whether the <see cref="ShowParentCommand"/> can be executed.
    /// </summary>
    private void CanExecuteShowAll(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute =  filteredGraphWrapper != null && hiddenNodesSet.Count != 0 && !doingLayout;
      e.Handled = true;
    }

    /// <summary>
    /// Handler for the <see cref="ShowAllCommand"/>
    /// </summary>
    private void ShowAllExecuted(object sender, ExecutedRoutedEventArgs e) {
      if (!doingLayout) {
        hiddenNodesSet.Clear();
        RefreshLayout(-1, graphControl.CurrentItem as INode);
      }
    }

    #endregion

    /// <summary>
    /// Help method that hides all nodes and its descendants except for a given node
    /// </summary>
    private void HideAllExcept(INode nodeToHide, INode exceptNode) {
      hiddenNodesSet.Add(nodeToHide);
      foreach (var child in filteredGraphWrapper.WrappedGraph.Successors(nodeToHide)) {
        if (exceptNode != child) {
          HideAllExcept(child, exceptNode);
        }
      }
    }

    // indicates whether a layout is calculated at the moment
    private bool doingLayout;

    /// <summary>
    /// Helper method that refreshes the layout after children or parent nodes have been added
    /// or removed.
    /// </summary>
    private async void RefreshLayout(int count, INode centerNode) {
      if (doingLayout) {
        return;
      }
      doingLayout = true;
      if (count != hiddenNodesSet.Count) {
        // tell our filter to refresh the graph
        filteredGraphWrapper.NodePredicateChanged();
        // the commands CanExecute state might have changed - suggest a requery.
        CommandManager.InvalidateRequerySuggested();

        // now layout the graph in animated fashion
        IGraph tree = graphControl.Graph;

        // we mark a node as the center node
        graphControl.Graph.MapperRegistry.CreateDelegateMapper<INode, bool>("CenterNode", node => node == centerNode);

        // configure the layout data
        var layoutData = CreateLayoutData(tree);

        // create the layout algorithm (with a stage that fixes the center node in the coordinate system
        var layout = new BendDuplicatorStage(new FixNodeLocationStage(new TreeLayout()));

        // configure a LayoutExecutor
        var executor = new LayoutExecutor(graphControl, layout)
        {
          AnimateViewport = centerNode == null,
          EasedAnimation = true,
          RunInThread = true,
          UpdateContentRect = true,
          LayoutData = layoutData,
          Duration = TimeSpan.FromMilliseconds(500)
        };
        await executor.Start();
        graphControl.Graph.MapperRegistry.RemoveMapper("CenterNode");
        doingLayout = false;
        LimitViewport();
      }
    }

    private void ZoomToCurrentItem() {
      var currentItem = GraphControl.CurrentItem as INode;
      // visible current item
      if (GraphControl.Graph.Contains(currentItem)) {
        GraphControl.ZoomToCurrentItemCommand.Execute(null, GraphControl);
      } else {
        // see if it can be made visible
        if (filteredGraphWrapper.WrappedGraph.Nodes.Contains(currentItem)) {
          // uhide all nodes...
          hiddenNodesSet.Clear();
          // except the node to be displayed and all its descendants
          foreach (var testNode in filteredGraphWrapper.WrappedGraph.Nodes) {
            if (testNode != currentItem && filteredGraphWrapper.WrappedGraph.InDegree(testNode) == 0) {
              HideAllExcept(testNode, currentItem);
            }
          }
          // reset the layout to make the animation nicer
          foreach (var n in filteredGraphWrapper.Nodes) {
            filteredGraphWrapper.SetNodeCenter(n, PointD.Origin);
          }
          foreach (var edge in filteredGraphWrapper.Edges) {
            filteredGraphWrapper.ClearBends(edge);
          }
          RefreshLayout(-1, null);
        }
      }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #region TreeView related

    private void TreeMouseDoubleClick(object sender, MouseButtonEventArgs e) {
      XmlLinkedNode clickedItem = ((TreeView)e.Source).SelectedItem as XmlLinkedNode;
      INode selectedNode = filteredGraphWrapper.WrappedGraph.Nodes.FirstOrDefault(node => node.Tag == clickedItem);
      graphControl.CurrentItem = selectedNode;
      ZoomToCurrentItem();
    }

    private void TreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
      ZoomToTreeItem();
    }

    private void ZoomToTreeItem() {
      XmlLinkedNode clickedItem = treeView.SelectedItem as XmlLinkedNode;
      // get the correspondent node in the graph
      INode selectedNode = filteredGraphWrapper.WrappedGraph.Nodes.FirstOrDefault(node => node.Tag == clickedItem);
      if (selectedNode != null && graphControl.Graph.Contains(selectedNode)) {
        // select the node in the GraphControl
        GraphControl.CurrentItem = selectedNode;
        graphControl.EnsureVisible(selectedNode.Layout.ToRectD());
      }
    }

    void graphControl_CurrentItemChanged(object sender, RoutedPropertyChangedEventArgs<IModelItem> e) {
      foreach (var item in treeView.Items) {
        SelectCurrentItemInTreeRec(treeView, item);
      }
    }

    private void SelectCurrentItemInTreeRec(ItemsControl parent, object item) {
      var currentItem = graphControl.CurrentItem;
      if (currentItem == null) {
        return;
      }
      XmlLinkedNode employee = currentItem.Tag as XmlLinkedNode;
      
      if (item == null || parent == null || employee == null) {
        return;
      }

      var treeViewItem = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
      if (treeViewItem != null) {
        if (item == employee) {
          treeViewItem.IsSelected = true;
          return;
        }
        foreach (var child in treeViewItem.Items) {
          SelectCurrentItemInTreeRec(treeViewItem, child);
        }
      }
    }

    private void TreeSource_GraphRebuilt(object sender, EventArgs e) {
    }

    private void TreeViewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Return || e.Key == Key.Enter) {
        ZoomToCurrentItem();
      }
    }

    #endregion
  }

  /// <summary>
  /// An <see cref="AbstractLayoutStage"/> that uses a <see cref="IDataProvider"/>/<see cref="IMapper{K,V}"/>
  /// to determine a node whose location should not be changed during the layout.
  /// </summary>
  internal sealed class FixNodeLocationStage : LayoutStageBase
  {
    public FixNodeLocationStage(ILayoutAlgorithm layout) : base(layout) { }

    public override void ApplyLayout(LayoutGraph graph) {
      // determine the single node to keep at the center.
      var provider = graph.GetDataProvider("CenterNode");
      Node centerNode = null;
      if (provider != null) {
        centerNode = graph.Nodes.FirstOrDefault(provider.GetBool);
      }
      if (CoreLayout != null) {
        if (centerNode != null) {
          // remember old center
          var center = graph.GetCenter(centerNode);
          // run layout
          CoreLayout.ApplyLayout(graph);
          // obtain new center
          var newCenter = graph.GetCenter(centerNode);
          // and adjust the layout
          LayoutGraphUtilities.MoveSubgraph(graph, graph.GetNodeCursor(), center.X - newCenter.X, center.Y - newCenter.Y);
        } else {
          CoreLayout.ApplyLayout(graph);
        }
      }
    }
  }

  /// <summary>
  /// LayoutStage that duplicates bends that share a common bus.
  /// </summary>
  class BendDuplicatorStage : LayoutStageBase
  {

    public BendDuplicatorStage()
      : base() {
    }

    public BendDuplicatorStage(ILayoutAlgorithm coreLayout)
      : base(coreLayout) {
    }

    public override void ApplyLayout(LayoutGraph graph) {

      ApplyLayoutCore(graph);

      foreach (Node n in graph.Nodes) {
        foreach (Edge e in n.OutEdges) {
          bool lastSegmentOverlap = false;
          IEdgeLayout er = graph.GetLayout(e);
          if (er.PointCount() > 0) {
            // last bend point
            YPoint bendPoint = er.GetPoint(er.PointCount() - 1);

            IEnumerator<Edge> ecc = n.OutEdges.GetEnumerator();
          loop: while (ecc.MoveNext()) {
              Edge eccEdge = ecc.Current;
              if (eccEdge != e) {
                YPointPath path = graph.GetPath(eccEdge);
                for (ILineSegmentCursor lc = path.LineSegments(); lc.Ok; lc.Next()) {
                  LineSegment seg = lc.LineSegment;
                  if (seg.Contains(bendPoint)) {
                    lastSegmentOverlap = true;
                    goto loop;
                  }
                }
              }
            }
          }


          YList points = graph.GetPointList(e);
          for (ListCell c = points.FirstCell; c != null; c = c.Succ()) {
            YPoint p = (YPoint)c.Info;
            if (c.Succ() == null && !lastSegmentOverlap) {
              break;
            }

            YPoint p0 = (YPoint)(c.Pred() == null ? graph.GetSourcePointAbs(e) : c.Pred().Info);
            YPoint p2;
            if (Math.Abs(p0.X - p.X) < 0.01) {
              p2 = new YPoint(p.X, p.Y - 0.001);
            } else {
              p2 = new YPoint(p.X - 0.001, p.Y);
            }

            points.InsertBefore(p2, c);
          }
          graph.SetPoints(e, points);
        }
      }
    }
  }

  public class NameToShortNameConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      String name = value.ToString();
      String[] names = name.Split(' ');
      String shortName;
      if (names.Length > 1) {
        shortName = names[0].Substring(0, 1) + ". " + names[names.Length - 1];
      } else {
        shortName = names[0];
      }
      return shortName;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }

    #endregion
  }

  public class StatusToSolidColorBrushConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      Color c = Colors.White;
      if (value != null) {
        string s = (string)value;
        if (s.Equals("present")) {
          c = Colors.Green;
        } else if (s.Equals("unavailable")) {
          c = Colors.Red;
        } else if (s.Equals("travel")) {
          c = Colors.Purple;
        } 
      }
      return new SolidColorBrush(c);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }

    #endregion
  }

  public class NodeToEmployeeConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      INode node = value as INode;
      if (node != null) {
        return node.Tag;
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }

    #endregion
  }

}
