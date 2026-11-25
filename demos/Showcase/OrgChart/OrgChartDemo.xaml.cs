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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Tree;
using yWorks.Utils;
using LineSegment = yWorks.Geometry.LineSegment;

namespace yWorks.Demo.OrgChart
{
  /// <summary>
  ///   Interaction logic for OrgChartWindow.xaml
  /// </summary>
  [Demo("organization-chart", "showcase", "Organization Chart",
      Summary = "An interactive viewer for organization charts with adaptive styles and automatic layout.",
      Tags = ["style", "layout", "interaction"],
      Keywords = [
          "orgchart", "animation", "filtering", "highlight", "templates", "data panel", "hide", "detail",
          "notable style", "data management", "level of detail"
      ])]
  public partial class OrgChartDemo
  {
    [STAThread]
    public static void Main() {
      Run<OrgChartDemo>();
    }


    /// <summary>
    ///   The command that can be used by the buttons to show the parent node.
    /// </summary>
    /// <remarks>
    ///   This command requires the corresponding <see cref="INode"/> as the
    ///   <see cref="System.Windows.Input.ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand ShowParentCommand = new("Show Parent", "ShowParent", typeof(OrgChartDemo));

    /// <summary>
    ///   The command that can be used by the buttons to hide the parent node.
    /// </summary>
    /// <remarks>
    ///   This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand HideParentCommand = new("Hide Parent", "HideParent", typeof(OrgChartDemo));

    /// <summary>
    ///   The command that can be used by the buttons to show the child nodes.
    /// </summary>
    /// <remarks>
    ///   This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand ShowChildrenCommand = new("Show Children", "ShowChildren",
        typeof(OrgChartDemo));

    /// <summary>
    ///   The command that can be used by the buttons to hide the child nodes.
    /// </summary>
    /// <remarks>
    ///   This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly RoutedUICommand HideChildrenCommand = new("Hide Children", "HideChildren",
        typeof(OrgChartDemo));

    /// <summary>
    ///   The command that can be used by the buttons to expand all collapsed nodes.
    /// </summary>
    public static readonly RoutedUICommand ShowAllCommand = new("Show All", "ShowAll", typeof(OrgChartDemo));

    /// <summary>
    ///   Used by the predicate function to determine which nodes should not be shown.
    /// </summary>
    private readonly HashSet<INode> hiddenNodesSet;

    /// <summary>
    ///   The filtered graph instance that hides nodes from the to create smaller graphs for easier navigation.
    /// </summary>
    private FilteredGraphWrapper filteredGraphWrapper;

    public OrgChartDemo() {
      InitializeComponent();
      hiddenNodesSet = [];
    }

    protected override void Initialize() {
      // register command bindings
      graphControl.CommandBindings.Add(new CommandBinding(HideChildrenCommand, HideChildrenExecuted,
          CanExecuteHideChildren));
      graphControl.CommandBindings.Add(new CommandBinding(ShowChildrenCommand, ShowChildrenExecuted,
          CanExecuteShowChildren));
      graphControl.CommandBindings.Add(new CommandBinding(HideParentCommand, HideParentExecuted, CanExecuteHideParent));
      graphControl.CommandBindings.Add(new CommandBinding(ShowParentCommand, ShowParentExecuted, CanExecuteShowParent));
      graphControl.CommandBindings.Add(new CommandBinding(ShowAllCommand, ShowAllExecuted, CanExecuteShowAll));

      // disable selection, focus and highlight rendering
      GraphControl.SelectionIndicatorManager.Enabled = false;
      GraphControl.FocusIndicatorManager.Enabled = false;
      GraphControl.HighlightIndicatorManager.Enabled = false;

      BuildGraph();
    }

    private void BuildGraph() {
      using var stream = LoadAssemblyResource("orgchartmodel.xml");
      var document = new XmlDocument();
      document.Load(stream);

      var dataProvider = new XmlDataProvider {
          Document = document,
          XPath = "/staff/employee[1]"
      };
      dataProvider.DataChanged += (sender, _) => {
        var treeSource = (TreeSource) TryFindResource("TreeSource");
        treeSource.NodesSource = (IEnumerable) ((XmlDataProvider) sender!).Data;
      };
    }

    /// <summary>
    ///   Called when an item has been double-clicked.
    /// </summary>
    /// <param name="o">The source of the event.</param>
    /// <param name="itemClickedEventArgs">The event data instance containing the event data.</param>
    private async void OnItemDoubleClicked(object o, ItemClickedEventArgs<IModelItem> itemClickedEventArgs) {
      graphControl.CurrentItem = itemClickedEventArgs.Item;
      await ZoomToCurrentItem();
    }

    /// <summary>
    ///   The predicate used for the FilterGraphWrapper
    /// </summary>
    /// <param name="obj"></param>
    private bool ShouldShowNode(INode obj) => !hiddenNodesSet.Contains(obj);

    private void LimitViewport() {
      GraphControl.UpdateContentBounds();
      GraphControl.ViewportLimiter.Policy = ViewportLimitingPolicy.TowardsLimit;
    }

    /// <summary>
    ///   Gets the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl => graphControl;

    #region Tree Layout Configuration and initial execution

    /// <summary>
    ///   Does a tree layout of the graph.
    ///   The layout and assistant attributes from the business data of the employees are used to
    ///   guide the layout.
    /// </summary>
    public void DoLayout() {
      var tree = graphControl.Graph;
      var layoutData = CreateLayoutData(tree, null);
      tree.ApplyLayout(new BendDuplicatorStage(new TreeLayout()), layoutData);
    }

    private static LayoutData<INode, IEdge, ILabel, ILabel> CreateLayoutData(IGraph tree, INode centerNode) {
      var data = new TreeLayoutData<INode, IEdge, ILabel, ILabel> {
          SubtreePlacers = {
              MapperFunction = delegate(INode node) {
                var employee = node.Tag as XmlElement;
                if (tree.OutDegree(node) == 0 || employee == null) {
                  return null;
                }
                var layout = employee.GetAttribute("layout");
                switch (layout) {
                  case "rightHanging":
                    return new AssistantSubtreePlacer {
                        ChildSubtreePlacer = new SingleLayerSubtreePlacer(SubtreeTransform.RotateLeftFlipY,
                            SingleLayerSubtreePlacerRootAlignment.LeadingOnBus,
                            SingleLayerSubtreePlacerRoutingStyle.OrthogonalAtRoot, 30, 30)
                    };
                  case "leftHanging":
                    return new AssistantSubtreePlacer {
                        ChildSubtreePlacer = new SingleLayerSubtreePlacer(SubtreeTransform.RotateRight,
                            SingleLayerSubtreePlacerRootAlignment.LeadingOnBus,
                            SingleLayerSubtreePlacerRoutingStyle.OrthogonalAtRoot, 30, 30)
                    };
                  case "bothHanging":
                    return new AssistantSubtreePlacer {
                        ChildSubtreePlacer = new LeftRightSubtreePlacer { PlaceLastOnBottom = false }
                    };
                  default:
                    return new AssistantSubtreePlacer {
                        ChildSubtreePlacer = new SingleLayerSubtreePlacer(SubtreeTransform.None,
                            SingleLayerSubtreePlacerRootAlignment.Median, verticalDistance: 30, horizontalDistance: 30)
                    };
                }
              }
          },
          AssistantNodes = {
              Predicate = delegate(INode node) {
                var employee = node.Tag as XmlElement;
                var assistant = employee != null ? employee.Attributes["assistant"] : null;
                return assistant != null && assistant.Value == "true";
              }
          }
      };

      var layoutAnchoringStageData = new LayoutAnchoringStageData<INode, IEdge, ILabel, ILabel>();
      layoutAnchoringStageData.NodeAnchoringPolicies.Mapper[centerNode] = LayoutAnchoringPolicy.Center;
      return data.CombineWith(layoutAnchoringStageData);
    }

    #endregion

    #region Command Binding Helper methods

    /// <summary>
    ///   Helper method that determines whether the <see cref="ShowParentCommand"/> can be executed.
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
    ///   Handler for the <see cref="ShowChildrenCommand"/>
    /// </summary>
    public async void ShowChildrenExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        var count = hiddenNodesSet.Count;
        foreach (var childEdge in filteredGraphWrapper.WrappedGraph.OutEdgesAt(node)) {
          var child = childEdge.GetTargetNode();
          if (hiddenNodesSet.Remove(child)) {
            filteredGraphWrapper.WrappedGraph.SetNodeCenter(child, node.Layout.GetCenter());
            filteredGraphWrapper.WrappedGraph.ClearBends(childEdge);
          }
        }
        await RefreshLayout(count, node);
      }
    }

    /// <summary>
    ///   Helper method that determines whether the <see cref="ShowParentCommand"/> can be executed.
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
    ///   Handler for the <see cref="ShowParentCommand"/>
    /// </summary>
    private async void ShowParentExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        var count = hiddenNodesSet.Count;
        foreach (var parentEdge in filteredGraphWrapper.WrappedGraph.InEdgesAt(node)) {
          var parent = parentEdge.GetSourceNode();
          if (hiddenNodesSet.Remove(parent)) {
            filteredGraphWrapper.WrappedGraph.SetNodeCenter(parent, node.Layout.GetCenter());
            filteredGraphWrapper.WrappedGraph.ClearBends(parentEdge);
          }
        }
        await RefreshLayout(count, node);
      }
    }

    /// <summary>
    ///   Helper method that determines whether the <see cref="HideParentCommand"/> can be executed.
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
    ///   Handler for the <see cref="HideParentCommand"/>
    /// </summary>
    private async void HideParentExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        var count = hiddenNodesSet.Count;

        foreach (var testNode in filteredGraphWrapper.WrappedGraph.Nodes) {
          if (testNode != node && filteredGraphWrapper.Contains(testNode) &&
              filteredGraphWrapper.InDegree(testNode) == 0) {
            // this is a root node - remove it and all children unless
            HideAllExcept(testNode, node);
          }
        }
        await RefreshLayout(count, node);
      }
    }

    /// <summary>
    ///   Helper method that determines whether the <see cref="HideChildrenCommand"/> can be executed.
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
    ///   Handler for the <see cref="HideChildrenCommand"/>
    /// </summary>
    private async void HideChildrenExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null && !doingLayout) {
        var count = hiddenNodesSet.Count;
        foreach (var child in filteredGraphWrapper.Successors(node)) {
          HideAllExcept(child, node);
        }
        await RefreshLayout(count, node);
      }
    }

    /// <summary>
    ///   Helper method that determines whether the <see cref="ShowParentCommand"/> can be executed.
    /// </summary>
    private void CanExecuteShowAll(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = filteredGraphWrapper != null && hiddenNodesSet.Count != 0 && !doingLayout;
      e.Handled = true;
    }

    /// <summary>
    ///   Handler for the <see cref="ShowAllCommand"/>
    /// </summary>
    private async void ShowAllExecuted(object sender, ExecutedRoutedEventArgs e) {
      if (!doingLayout) {
        hiddenNodesSet.Clear();
        await RefreshLayout(-1, graphControl.CurrentItem as INode);
      }
    }

    #endregion

    /// <summary>
    ///   Help method that hides all nodes and its descendants except for a given node
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
    ///   Helper method that refreshes the layout after children or parent nodes have been added
    ///   or removed.
    /// </summary>
    private async Task RefreshLayout(int count, INode centerNode) {
      if (doingLayout) {
        return;
      }
      if (count != hiddenNodesSet.Count) {
        doingLayout = true;
        // tell our filter to refresh the graph
        filteredGraphWrapper.NodePredicateChanged();
        // the commands CanExecute state might have changed - suggest a requery.
        CommandManager.InvalidateRequerySuggested();

        // now layout the graph in animated fashion
        var tree = graphControl.Graph;

        // configure the layout data
        var layoutData = CreateLayoutData(tree, centerNode);

        // create the layout algorithm (with a stage that fixes the center node in the coordinate system
        var layout = new BendDuplicatorStage(new LayoutAnchoringStage(new TreeLayout()));

        // configure a LayoutExecutor
        var executor = new LayoutExecutor(graphControl, layout) {
            AnimateViewport = centerNode == null,
            EasedAnimation = true,
            RunInThread = true,
            UpdateContentBounds = true,
            LayoutData = layoutData,
            AnimationDuration = TimeSpan.FromMilliseconds(500)
        };
        await executor.Start();
        doingLayout = false;
        LimitViewport();
      }
    }

    private async Task ZoomToCurrentItem() {
      var currentItem = GraphControl.CurrentItem as INode;
      // visible current item
      if (GraphControl.Graph.Contains(currentItem)) {
        GraphCommands.ZoomToCurrentItem.Execute(currentItem, GraphControl);
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
          await RefreshLayout(-1, null);
        }
      }
    }

    private void TreeSource_GraphRebuilt(object sender, EventArgs e) {
      // we wrap the graph instance by a filtered graph wrapper
      filteredGraphWrapper = new FilteredGraphWrapper(((TreeSource) sender).Graph, ShouldShowNode);
      GraphControl.Graph = filteredGraphWrapper;

      // now calculate the initial layout
      DoLayout();

      GraphControl.FitGraphBounds();
      LimitViewport();
    }
  }

  /// <summary>
  ///   LayoutStage that duplicates bends that share a common bus.
  /// </summary>
  internal class BendDuplicatorStage : LayoutStageBase
  {
    /// <summary>
    ///   Represents a small tolerance value used for floating-point comparisons.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The <c>Epsilon</c> constant is used to account for minor inaccuracies in floating-point arithmetic when performing
    ///     geometric calculations involving line segments. It provides a threshold for determining approximate equality or
    ///     intersections, helping to mitigate precision issues that can arise from the limitations of floating-point
    ///     representation.
    ///   </para>
    ///   <para>
    ///     This constant is typically used in methods that involve checking for intersections, containment, or equality of
    ///     line segments where exact precision is not feasible. The default value of <c>1e-8</c> is chosen to balance
    ///     precision with practical considerations of computational accuracy.
    ///   </para>
    /// </remarks>
    private const double Epsilon = 1e-8;

    public BendDuplicatorStage() { }

    public BendDuplicatorStage(ILayoutAlgorithm coreLayout) : base(coreLayout) { }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      CoreLayout?.ApplyLayout(graph);

      foreach (var n in graph.Nodes) {
        foreach (var e in n.OutEdges) {
          var lastSegmentOverlap = false;
          if (e.Bends.Count > 0) {
            // last bend point
            var bendPoint = e.Bends.Last.Location;

            var ecc = n.OutEdges.GetEnumerator();
          loop:
            while (ecc.MoveNext()) {
              var eccEdge = ecc.Current;
              if (eccEdge != e) {
                var path = eccEdge.PathPoints.ToArray();
                for (var i = 1; i < path.Length; i++) {
                  var seg = new LineSegment(path[i - 1].Location, path[i].Location);
                  if (seg.Contains(bendPoint, Epsilon)) {
                    lastSegmentOverlap = true;
                    goto loop;
                  }
                }
              }
            }
          }

          var bendPoints = new YList<PointD>();
          foreach (var eBend in e.Bends) {
            bendPoints.Add(eBend.Location);
          }
          for (var c = bendPoints.FirstCell; c != null; c = c.Next) {
            var p = c.Info;
            if (c.Next == null && !lastSegmentOverlap) {
              break;
            }

            var p0 = c.Previous == null ? e.SourcePortLocation : c.Previous.Info;
            PointD p2;
            if (Math.Abs(p0.X - p.X) < 0.01) {
              p2 = new PointD(p.X, p.Y - 0.001);
            } else {
              p2 = new PointD(p.X - 0.001, p.Y);
            }

            bendPoints.InsertBefore(p2, c);
          }
          e.ResetPath(false);
          foreach (var bendPoint in bendPoints) {
            var point = bendPoint;
            graph.AddBend(e, point.X, point.Y);
          }
        }
      }
    }
  }

  public class NameToShortNameConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var name = value.ToString();
      var names = name.Split(' ');
      string shortName;
      if (names.Length > 1) {
        shortName = names[0].Substring(0, 1) + ". " + names[names.Length - 1];
      } else {
        shortName = names[0];
      }
      return shortName;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;

    #endregion
  }

  public class StatusToSolidColorBrushConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var c = Colors.White;
      if (value != null) {
        var s = (string) value;
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;

    #endregion
  }

  public class NodeToEmployeeConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var node = value as INode;
      if (node != null) {
        return node.Tag;
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;

    #endregion
  }
}
