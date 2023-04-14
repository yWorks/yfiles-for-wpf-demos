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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;
using Condition = System.Windows.Automation.Condition;

// Some features in the demo require low-level input. Since this is not provided natively by UI Automation (which
// instead focuses on handling controls and patterns), we have to include a 3rd-party solution. The demo is written
// against two of the more popular frameworks for doing so: Microsoft.TestApi and TestStack.White. Add either of those
// as references to the project (e.g. via nuget) and define either the 'TestApi' or 'White' symbol in the project 
// properties for the compiler:

#if TestApi
using Microsoft.Test;
using Microsoft.Test.Input;
using System.Threading;
using Point = System.Drawing.Point;
#endif

#if White
using TestStack.White.Configuration;
using TestStack.White.Factory;
using TestStack.White.InputDevices;
using TestStack.White.UIA;
using TestStack.White.UIItems;
using System.Threading;
#endif

namespace Demo.yFiles.Graph.UIAutomation
{
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  // ReSharper disable once InconsistentNaming
  public partial class UIAutomationDemoWindow : Window
  {
    public UIAutomationDemoWindow() {
      InitializeComponent();
    }

    #region Initialization

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      InitializeGraph();
      graphControl.InputMode = new GraphEditorInputMode { AllowGroupingOperations = true };

      graphControl.Graph.NodeCreated += RefreshUiaInterface;
      graphControl.Graph.NodeStyleChanged += RefreshUiaInterface;
      graphControl.Graph.NodeTagChanged += RefreshUiaInterface;
      graphControl.Graph.NodeRemoved += RefreshUiaInterface;
      graphControl.Graph.EdgeCreated += RefreshUiaInterface;
      graphControl.Graph.EdgePortsChanged += RefreshUiaInterface;
      graphControl.Graph.EdgeStyleChanged += RefreshUiaInterface;
      graphControl.Graph.EdgeRemoved += RefreshUiaInterface;
      graphControl.Graph.LabelAdded += RefreshUiaInterface;
      graphControl.Graph.LabelLayoutParameterChanged += RefreshUiaInterface;
      graphControl.Graph.LabelStyleChanged += RefreshUiaInterface;
      graphControl.Graph.LabelPreferredSizeChanged += RefreshUiaInterface;
      graphControl.Graph.LabelTagChanged += RefreshUiaInterface;
      graphControl.Graph.LabelTextChanged += RefreshUiaInterface;
      graphControl.Graph.LabelRemoved += RefreshUiaInterface;

      RefreshTree();
      RefreshPatternInterface();
      DisableUnavailableFeatures();
    }

    private void DisableUnavailableFeatures() {
      rtbInteractionHint.Visibility = Visibility.Visible;
      grpDrawEdge.Visibility = Visibility.Collapsed;
#if TestApi
      grpDrawEdge.Visibility = Visibility.Visible;
      rtbInteractionHint.Visibility = Visibility.Collapsed;
#endif
#if White
      grpDrawEdge.Visibility = Visibility.Visible;
      rtbInteractionHint.Visibility = Visibility.Collapsed;
#endif
    }

    /// <summary>
    ///   Initializes the graph we use in this demo.
    /// </summary>
    private void InitializeGraph() {
      // Allow grouping and folding
      IGraph graph = new FoldingManager().CreateFoldingView().Graph;

      // Initialize defaults
      DemoStyles.InitDemoStyles(graph, foldingEnabled: true);

      // Mapper for using custom AutomationIds. This is just a sample that shows how to add a mapper that returns
      // an automation ID from the model item's tag. In practice you might return a property of the tag object if that
      // identifies your model item uniquely.
      graph.MapperRegistry.CreateDelegateMapper(GraphControl.AutomationIdKey,
          (IModelItem key) => {
            if (key.Tag == null) {
              return null;
            }
            if (key.Tag is string) {
              return key.Tag as string;
            }
            return null;
          });

      // create sample graph
      var node1 = graph.CreateNode(new RectD(0, 0, 100, 30));
      graph.AddLabel(node1, "Node with label");
      // Use a custom AutomationId for that node. We just set the tag to a string here, which then gets picked up by
      // the mapper we added above.
      node1.Tag = "Custom Automation ID";

      var node2 = graph.CreateNode(new PointD(90, 70));
      graph.CreateEdge(node2, graph.CreateNode(new PointD(90, 120)));

      graph.CreateEdge(node2, graph.CreateNode(new PointD(200, 30)));

      // Node with a templated style – the controls within the template do appear in the UIA tree as well and can be
      // interacted with.
      graph.CreateNode(new RectD(100, 150, 150, 150), new NodeControlNodeStyle("NodeStyle"));

      graph.GroupNodes(node1, node2);

      graphControl.Graph = graph;
      graphControl.FitGraphBounds();
    }

    #endregion

    #region UI Automation tree handling

    private void RefreshTree() {
      TreeNode node = null;
      RunInBackground(
          delegate { node = GetCurrentTree(); },
          delegate {
            if (node == null) {
              RefreshTree();
              return;
            }
            uiaTree.Items.Clear();
            uiaTree.Items.Add(node.ToTreeViewItem());
            RefreshComboboxes();
          });
    }

    private static TreeNode GetCurrentTree() {
      return GetTreeNode(GetGraphControlAutomationElement());
    }

    private static TreeNode GetTreeNode(AutomationElement element) {
      if (element == null) {
        return null;
      }
      var itemName = element.Current.Name;
      var className = element.Current.ClassName;
      var id = element.Current.AutomationId;
      var name = string.Format("{0} ({1}, Id: {2})", itemName, className, id);

      var node = new TreeNode
      {
        Header = name,
        Tag = element
      };

      // As per the documentation and various assorted advice on the web this is suboptimal. You're supposed to use
      // a TreeWalker or at least cache a subtree you're going to walk anyway. However, that approach has reliability
      // issues when adding or editing labels. Thus the simple, dumb (but less performant) FindAll is used here to
      // prevent problems where all of a sudden 90 % of the UIA tree would be missing.
      // If your application does not have to handle a changing graph or added/changed labels, then the other, usual
      // options are likely faster.
      foreach (AutomationElement child in element.FindAll(TreeScope.Children, Condition.TrueCondition)) {
        node.Items.Add(GetTreeNode(child));
      }

      return node;
    }

    private static IEnumerable<AutomationElement> GetAutomationElements(TreeViewItem tvi) {
      if (tvi == null) {
        return new AutomationElement[0];
      }

      var l = new List<AutomationElement> { (AutomationElement) tvi.Tag };
      foreach (var i in tvi.Items) {
        l.AddRange(GetAutomationElements(i as TreeViewItem));
      }
      return l;
    }

    #endregion

    #region Helper methods

    /// <summary>
    ///   Helper method around a <see cref="BackgroundWorker" /> to run an action on a separate thread and another
    ///   action on the originating thread after completion.
    /// </summary>
    /// <remarks>
    ///   UI Automation mandates that when accessing your own UI you have to do all UIA calls from a separate thread to
    ///   prevent deadlocks and slowness (see <see href="https://docs.microsoft.com/en-us/dotnet/articles/framework/ui-automation/ui-automation-threading-issues" />).
    ///   This method eases that process a bit by wrapping a <see cref="BackgroundWorker" />. If you automate a different
    ///   application this is not necessary.
    /// </remarks>
    /// <param name="action">The action to run on a separate thread.</param>
    /// <param name="completed">The action to run after completion on the originating thread.</param>
    private static void RunInBackground(Action action, Action completed) {
      var worker = new BackgroundWorker();
      worker.DoWork += delegate {
        try {
          action();
        } catch (Exception exception) {
          Console.WriteLine(exception);
        }
      };
      worker.RunWorkerCompleted += delegate { completed(); };
      worker.RunWorkerAsync();
    }

    private static AutomationElement GetGraphControlAutomationElement() {
      try {
        var root = AutomationElement.RootElement;
        // Look for the graph control in our own process
        var processIdCondition = new PropertyCondition(AutomationElement.ProcessIdProperty,
            Process.GetCurrentProcess().Id);

        var window = root.FindFirst(TreeScope.Children, processIdCondition);

        var idCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, "DemoGraphControl");
        return window.FindFirst(TreeScope.Descendants, idCondition);
      } catch {
        return null;
      }
    }

    #endregion

    #region UI helper methods

    private void RefreshSelection() {
      string value = null;
      RunInBackground(
          delegate {
            object p;
            var elem = GetGraphControlAutomationElement();
            if (elem == null) {
              return;
            }
            if (elem.TryGetCurrentPattern(SelectionPattern.Pattern, out p)) {
              var pattern = (SelectionPattern) p;
              var selectionElements = pattern.Current.GetSelection();
              value = string.Join(", ", selectionElements.Select(ae => ae.Current.Name));
            }
          },
          delegate {
            if (value != null) {
              lbCurrentSelection.Content = value;
            }
          });
    }

    private void RefreshPatternInterface() {
      RefreshSelection();

      pnlSelectionItem.IsEnabled = false;
      grpInvoke.IsEnabled = false;
      grpToggle.IsEnabled = false;
      grpExpandCollapse.IsEnabled = false;
      grpValue.IsEnabled = false;
      grpScrollItem.IsEnabled = false;

      lbCurrentSelection.Content = "";
      lbCurrentToggleValue.Content = "";
      lbCurrentExpandValue.Content = "";
      tbCurrentValue.Text = "";

      var item = uiaTree.SelectedItem as TreeViewItem;
      if (item == null) {
        return;
      }
      var elem = item.Tag as AutomationElement;
      if (elem == null) {
        return;
      }

      bool selectionItemEnabled = false;
      bool invokeEnabled = false;
      bool toggleEnabled = false;
      bool expandCollapseEnabled = false;
      bool valueEnabled = false;
      bool scrollItemEnabled = false;
      string toggleValue = "";
      string expandCollapseValue = "";
      string valueValue = "";
      bool valueReadOnly = true;
      RunInBackground(
          delegate {
            var patterns = elem.GetSupportedPatterns();
            selectionItemEnabled = patterns.Contains(SelectionItemPattern.Pattern);
            invokeEnabled = patterns.Contains(InvokePattern.Pattern);
            toggleEnabled = patterns.Contains(TogglePattern.Pattern);
            expandCollapseEnabled = patterns.Contains(ExpandCollapsePattern.Pattern);
            valueEnabled = patterns.Contains(ValuePattern.Pattern);
            scrollItemEnabled = patterns.Contains(ScrollItemPattern.Pattern);

            if (toggleEnabled) {
              var p = elem.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
              if (p != null) {
                toggleValue = p.Current.ToggleState.ToString();
              }
            }

            if (expandCollapseEnabled) {
              var p = elem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
              if (p != null) {
                expandCollapseValue = p.Current.ExpandCollapseState.ToString();
              }
            }

            if (valueEnabled) {
              var p = elem.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
              if (p != null) {
                valueValue = p.Current.Value;
                valueReadOnly = p.Current.IsReadOnly;
              }
            }
          },
          delegate {
            pnlSelectionItem.IsEnabled = selectionItemEnabled;
            grpInvoke.IsEnabled = invokeEnabled;
            grpToggle.IsEnabled = toggleEnabled;
            grpExpandCollapse.IsEnabled = expandCollapseEnabled;
            grpValue.IsEnabled = valueEnabled;
            grpScrollItem.IsEnabled = scrollItemEnabled;

            lbCurrentToggleValue.Content = toggleValue;
            lbCurrentExpandValue.Content = expandCollapseValue;
            tbCurrentValue.Text = valueValue;
            tbCurrentValue.IsReadOnly = valueReadOnly;
            btnSetValue.IsEnabled = !valueReadOnly;
          });
    }

    private void RefreshComboboxes() {
      var elements = new List<AutomationElement>();
      foreach (var tvi in uiaTree.Items) {
        elements.AddRange(GetAutomationElements(tvi as TreeViewItem));
      }

      var items = new List<Tuple<string, string>>();

      RunInBackground(
          delegate {
            var nodes = elements.Where(e => e.Current.ClassName == "Node").ToList();
            items.AddRange(nodes.Select(e => Tuple.Create(e.Current.Name, e.Current.AutomationId)));
          },
          delegate {
            fromNode.ItemsSource = items;
            toNode.ItemsSource = items;
          });
    }

    private AutomationElement GetSelectedAutomationElement() {
      var selectedItem = uiaTree.SelectedItem as TreeViewItem;
      if (selectedItem == null) {
        return null;
      }
      var element = selectedItem.Tag as AutomationElement;
      return element;
    }

    #endregion

    #region UI event handlers

    private void RefreshUiaInterface<T>(object sender, ItemEventArgs<T> args) {
      RefreshTree();
      RefreshPatternInterface();
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
      RefreshOverlay();
      RefreshPatternInterface();
    }

    private void OnGraphControlViewportChanged(object sender, PropertyChangedEventArgs e) {
      RefreshOverlay();
    }

    private void RefreshOverlay() {
      AutomationElement ae = GetSelectedAutomationElement();
      if (ae == null)
      {
        return;
      }

      Rect bounds = new Rect();

      RunInBackground(
          delegate { bounds = ae.Current.BoundingRectangle; },
          delegate {
            if (bounds.IsEmpty) {
              return;
            }
            overlay.Width = bounds.Width;
            overlay.Height = bounds.Height;
            var topLeft = overlayContainer.PointFromScreen(bounds.TopLeft);
            Canvas.SetLeft(overlay, topLeft.X);
            Canvas.SetTop(overlay, topLeft.Y);
            overlayContainer.Visibility = Visibility.Visible;
          });
    }

    private void OnRefreshTreeClicked(object sender, RoutedEventArgs e) {
      RefreshTree();
    }

    private void OnRefreshSelectionClicked(object sender, RoutedEventArgs e) {
      RefreshPatternInterface();
    }

    private void OnSelectClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
            if (pattern != null) {
              pattern.Select();
            }
          },
          RefreshPatternInterface);
    }

    private void OnAddToSelectionClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
            if (pattern != null) {
              pattern.AddToSelection();
            }
          },
          RefreshPatternInterface);
    }

    private void OnRemoveFromSelectionClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
            if (pattern != null) {
              pattern.RemoveFromSelection();
            }
          },
          RefreshPatternInterface);
    }

    private void OnToggleClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
            if (pattern != null) {
              pattern.Toggle();
            }
          },
          RefreshPatternInterface);
    }

    private void OnCollapseClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }
      // Since the expanding/collapsing changes the automation element, and things are asynchronous, we would still
      // show the expand/collapse pattern as enabled, even though it then points to a stale instance.
      // Just deselecting the selected item is an easy way of resolving that.
      var tvi = uiaTree.SelectedItem as TreeViewItem;
      if (tvi != null) {
        tvi.IsSelected = false;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            if (pattern != null) {
              pattern.Collapse();
            }
          },
          RefreshPatternInterface);
    }

    private void OnExpandClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }
      // Since the expanding/collapsing changes the automation element, and things are asynchronous, we would still
      // show the expand/collapse pattern as enabled, even though it then points to a stale instance.
      // Just deselecting the selected item is an easy way of resolving that.
      var tvi = uiaTree.SelectedItem as TreeViewItem;
      if (tvi != null) {
        tvi.IsSelected = false;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            if (pattern != null) {
              pattern.Expand();
            }
          },
          RefreshPatternInterface);
    }

    private void OnInvokeClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            if (pattern != null) {
              pattern.Invoke();
            }
          },
          RefreshPatternInterface);
    }

    private void OnSetValueClicked(object sender, RoutedEventArgs e) {
      var elem = GetSelectedAutomationElement();
      if (elem == null) {
        return;
      }

      var text = tbCurrentValue.Text;

      RunInBackground(
          delegate {
            var pattern = elem.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
            if (pattern != null) {
              pattern.SetValue(text);
            }
          },
          RefreshPatternInterface);
    }

    private void OnScrollIntoViewClicked(object sender, RoutedEventArgs e)
    {
      var elem = GetSelectedAutomationElement();
      if (elem == null)
      {
        return;
      }

      RunInBackground(
          delegate
          {
            var pattern = elem.GetCurrentPattern(ScrollItemPattern.Pattern) as ScrollItemPattern;
            if (pattern != null)
            {
              pattern.ScrollIntoView();
            }
          },
          RefreshOverlay);
    }

    private void OnDrawEdgeClicked(object sender, RoutedEventArgs e) {
#if !TestApi && !White
      MessageBox.Show("Please add either Microsoft.TestApi or TestStack.White to the project to test this.");
#else
      var fromId = fromNode.SelectedValue as string;
      var toId = toNode.SelectedValue as string;

#if TestApi
      DrawEdgeTestApi(fromId, toId);
#else
      DrawEdgeWhite(fromId, toId);
#endif
#endif
    }

#if TestApi
    private void DrawEdgeTestApi(string fromId, string toId) {
      RunInBackground(
          delegate {
            var fromElement = AutomationUtilities.FindElementsById(GetGraphControlAutomationElement(), fromId);
            var toElement = AutomationUtilities.FindElementsById(GetGraphControlAutomationElement(), toId);
            // Normally we would use GetClickablePoint here. However, the AutomationPeer takes the value returned and
            // then looks whether there really is either the element or a descendant at that point. And that goes
            // wrong as soon as there is an edge overlapping a node. The AutomationPeer sees the edge and determines
            // that the clickable point is invalid and throws an exception.
            // To work around that we use the same default logic AutomationPeers have for determining a clickable
            // point: Just take the center of the bounding box. It should work most of the time, and in this case we
            // don't particularly care whether an edge overlays the *target* node, for example. The edge doesn't react
            // in that case anyway. And even if it overlays the source node there's still a good chance that our
            // initial drag point will work.
            var fromBounds = fromElement[0].Current.BoundingRectangle;
            var toBounds = toElement[0].Current.BoundingRectangle;
            var fromPoint = new Point((int) (fromBounds.X + fromBounds.Width / 2),
                (int) (fromBounds.Y + fromBounds.Height / 2));
            var toPoint = new Point((int) (toBounds.X + toBounds.Width / 2),
                (int) (toBounds.Y + toBounds.Height / 2));

            Mouse.MoveTo(fromPoint);
            Thread.Sleep(50);
            Mouse.Down(MouseButton.Left);
            Thread.Sleep(50);
            // If we're trying to create a self-loop we have to drag outside the node first, otherwise it's just a
            // click that selects the node.
            if (fromPoint == toPoint) {
              Mouse.MoveTo(new Point((int) fromBounds.X - 10, (int) fromBounds.Y - 10));
              Thread.Sleep(50);
              // Note: The following mini-move is necessary due to how yFiles does state transitions while finding a
              // suitable target for drawing the edge. It just generates one additional MouseEvent on the node.
              // This is also only necessary when drawing self-loops, so in many real-world cases it can be left out.
              Mouse.MoveTo(new Point(fromPoint.X + 1, fromPoint.Y + 1));
              Thread.Sleep(50);
            }
            Mouse.MoveTo(toPoint);
            Thread.Sleep(50);
            Mouse.Up(MouseButton.Left);
          },
          delegate { });
    }
#endif
#if White
    void DrawEdgeWhite(string fromId, string toId) {
      RunInBackground(
          delegate {
            // Note: TestStack.White is *not* supposed to be used from the same process that is tested. In practice
            // UI tests are done from out of process anyway. For the purposes of this demo it's just more convenient
            // to have both in the same process. In any case, the following *goes directly against* what the White
            // developers suggest doing. Keep that in mind when blindly copying this code, please.

            // TestStack.White by default will only search two levels deep, which isn't necessarily enough, especially
            // when groups are involved.
            CoreAppXmlConfiguration.Instance.RawElementBasedSearch = true;
            CoreAppXmlConfiguration.Instance.MaxElementSearchDepth = 100;

            // You can convert AutomationElements to UIItems for interoperability of raw UI Automation and White, but
            // in this case it's not worth the trouble, so we just find everything from scratch.
            var app = TestStack.White.Application.Attach(Process.GetCurrentProcess());
            var window = app.GetWindow("MainWindow", InitializeOption.NoCache);
            var fromElement = window.Lookup<IUIItem>(fromId);
            var toElement = window.Lookup<IUIItem>(toId);

            if (fromElement.AutomationElement != toElement.AutomationElement) {
              // If we don't have to draw a self-loop this is the case made very easy by White.
              window.Mouse.DragAndDrop(fromElement, toElement);
            } else {
              // If we're trying to create a self-loop we have to drag outside the node first, otherwise it's just a
              // click that selects the node.
              var center = fromElement.Bounds.Center();
              var exterior = new Point(toElement.Bounds.X - 10, toElement.Bounds.Y - 10);
              var offcenter = new Point(center.X + 1, center.Y + 1);

              window.Mouse.Location = center;
              Thread.Sleep(100);
              Mouse.LeftDown();
              Thread.Sleep(100);
              window.Mouse.Location = exterior;
              Thread.Sleep(100);
              // Note: The following mini-move is necessary due to how yFiles does state transitions while finding a
              // suitable target for drawing the edge. It just generates one additional MouseEvent on the node.
              // This is also only necessary when drawing self-loops, so in many real-world cases it can be left out.
              window.Mouse.Location = offcenter;
              Thread.Sleep(100);
              window.Mouse.Location = center;
              Thread.Sleep(100);
              Mouse.LeftUp();
            }
          },
          delegate { });
    }
#endif

    #endregion
  }

  /// <summary>
  /// Helper class used in the three visualization of the UI Elements.
  /// </summary>
  internal class TreeNode
  {
    private readonly List<TreeNode> items = new List<TreeNode>();

    public string Header { get; set; }
    public object Tag { get; set; }

    public List<TreeNode> Items {
      get { return items; }
    }

    public TreeViewItem ToTreeViewItem() {
      var item = new TreeViewItem
      {
        Header = Header,
        Tag = Tag,
        IsExpanded = true
      };
      foreach (var nodes in Items) {
        item.Items.Add(nodes.ToTreeViewItem());
      }
      return item;
    }
  }
}
