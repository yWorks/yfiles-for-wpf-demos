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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Layout.Hierarchic;
using yWorks.Graph.DataBinding;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.DataBinding.InteractiveNodesGraphSource
{
  /// <summary>
  /// This class implements the main logic for this demo.
  /// </summary>
  /// <remarks>
  /// This demo shows how to use <see cref="AdjacentNodesGraphSource"/> to build a graph from business data. 
  /// Each business data item defines a successors and a predecessors list.
  /// </remarks>
  public partial class InteractiveNodesGraphSourceWindow
  {
    private AdjacentNodesGraphSource graphSource;

    public InteractiveNodesGraphSourceWindow() {
      InitializeComponent();
    }

    private void GraphSourceWindow_OnLoaded(object sender, RoutedEventArgs e) {
      // create new input mode
      GraphViewerInputMode inputMode = new GraphViewerInputMode { SelectableItems = GraphItemTypes.None };
      // add a custom input mode that allows dragging nodes from the graph to the lists
      inputMode.Add(new NodeDragInputMode {Priority = -1});
      graphControl.InputMode = inputMode;

      graphControl.FocusIndicatorManager.ShowFocusPolicy = ShowFocusPolicy.Always;

      graphSource = ((AdjacentNodesGraphSource)Application.Current.MainWindow.Resources["GraphSource"]);
      graphSource.NodesSource = CreateInitialBusinessData();

      ApplyLayout(false);
    }

    private static IEnumerable<BusinessData> CreateInitialBusinessData() {
      var nameDataMap = new Dictionary<string, BusinessData>
                          {
                            {"Jenny", new BusinessData("Jenny")},
                            {"Julia", new BusinessData("Julia")},
                            {"Marc", new BusinessData("Marc")},
                            {"Martin", new BusinessData("Martin")},
                            {"Natalie", new BusinessData("Natalie")},
                            {"Nicole", new BusinessData("Nicole")},
                            {"Petra", new BusinessData("Petra")},
                            {"Stephen", new BusinessData("Stephen")},
                            {"Tim", new BusinessData("Tim")},
                            {"Tom", new BusinessData("Tom")},
                            {"Tony", new BusinessData("Tony")}
                          };

      nameDataMap["Julia"].Predecessors.Add(nameDataMap["Jenny"]);
      nameDataMap["Julia"].Successors.Add(nameDataMap["Petra"]);
      nameDataMap["Marc"].Predecessors.Add(nameDataMap["Julia"]);
      nameDataMap["Marc"].Successors.Add(nameDataMap["Tim"]);
      nameDataMap["Martin"].Predecessors.Add(nameDataMap["Julia"]);
      nameDataMap["Martin"].Successors.Add(nameDataMap["Natalie"]);
      nameDataMap["Martin"].Successors.Add(nameDataMap["Nicole"]);
      nameDataMap["Nicole"].Successors.Add(nameDataMap["Petra"]);
      nameDataMap["Tim"].Successors.Add(nameDataMap["Tom"]);
      nameDataMap["Tom"].Successors.Add(nameDataMap["Tony"]);
      nameDataMap["Tony"].Successors.Add(nameDataMap["Tim"]);
      nameDataMap["Tony"].Predecessors.Add(nameDataMap["Julia"]);
      nameDataMap["Stephen"].Successors.Add(nameDataMap["Tom"]);

      return new ObservableCollection<BusinessData>
               {
                 nameDataMap["Marc"],
                 nameDataMap["Martin"],
                 nameDataMap["Stephen"]
               };
    }

    /// <summary>
    /// Returns the business data object from the current item, or 
    /// <see langword="null"/> if no current item is set
    /// </summary>
    private BusinessData GetCurrentItemData() {
      return graphControl.CurrentItem != null
               ? graphSource.GetBusinessObject(graphControl.CurrentItem) as BusinessData
               : null;
    }

    /// <summary>
    /// Applies the layout.
    /// </summary>
    /// <remarks>
    /// Uses an <see cref="HierarchicLayout"/>. If single graph
    /// items are created or removed, the incremental mode of this layout
    /// algorithm is used to keep most of the current layout of the graph
    /// unchanged.
    /// </remarks>
    /// <param name="incremental">if set to <see langword="true"/> [incremental].</param>
    /// <param name="incrementalNodes">The incremental nodes.</param>
    private async void ApplyLayout(bool incremental, params BusinessData[] incrementalNodes) {
      var layout = new HierarchicLayout();
      HierarchicLayoutData layoutData = null;
      if (!incremental) {
        layout.LayoutMode = LayoutMode.FromScratch;
      } else {
        layout.LayoutMode = LayoutMode.Incremental;

        if (incrementalNodes.Any()) {
          // we need to add hints for incremental nodes
          layoutData = new HierarchicLayoutData {
            IncrementalHints = { IncrementalLayeringNodes = { Source = incrementalNodes.Select(graphSource.GetNode) } }
          };
        }
      }
      await graphControl.MorphLayout(layout, TimeSpan.FromSeconds(1), layoutData);
    }

    #region Custom commands

    public static readonly ICommand AddNodeCommand = new RoutedUICommand("New Node Data", "AddNode",
                                                                                  typeof(InteractiveNodesGraphSourceWindow));

    public static readonly ICommand RemoveNodeCommand = new RoutedUICommand("Remove Node Data", "RemoveNode",
                                                                                  typeof(InteractiveNodesGraphSourceWindow));

    public static readonly ICommand AddPredecessorCommand = new RoutedUICommand("New Predecessor Data", "AddPredecessor",
                                                                                  typeof(InteractiveNodesGraphSourceWindow));

    public static readonly ICommand RemovePredecessorCommand = new RoutedUICommand("Remove Predecessor Data", "RemovePredecessor",
                                                                                  typeof(InteractiveNodesGraphSourceWindow));

    public static readonly ICommand AddSuccessorCommand = new RoutedUICommand("New Successor Data", "AddSuccessor",
                                                                                  typeof(InteractiveNodesGraphSourceWindow));

    public static readonly ICommand RemoveSuccessorCommand = new RoutedUICommand("Remove Successor Data", "RemoveSuccessor",
                                                                                  typeof(InteractiveNodesGraphSourceWindow));

    private void Remove_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
      var listBox = e.Parameter as ListBox;
      e.CanExecute = listBox != null && listBox.SelectedItems.Count > 0;
      e.Handled = true;
    }

    private void AddNeighbor_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = GetCurrentItemData() != null;
      e.Handled = true;
    }

    private void AddDataExecuted(object sender, ExecutedRoutedEventArgs e) {
      var listBox = e.Parameter as ListBox;
      ICollection<BusinessData> collection;
      if (listBox != null && listBox.TryGetItemsSourceCollection(out collection)) {
        AddData(collection, e);
      }
    }

    private void RemoveSelectionExecuted(object sender, ExecutedRoutedEventArgs e) {
      var listBox = e.Parameter as ListBox;
      ICollection<BusinessData> collection;
      if (listBox != null && listBox.TryGetItemsSourceCollection(out collection)) {
        RemoveItems(collection, new ArrayList(listBox.SelectedItems));
        if (listBox.Items.Count > 0) {
          listBox.SelectedIndex = 0;
        }
      }
    }

    private void AddData(ICollection<BusinessData> collection, ExecutedRoutedEventArgs e) {
      var command = e.Command as RoutedUICommand;
      var dialog = new StringInputDialog
                     {
                       Title = command == null ? "New Data" : command.Text,
                       Label = "Name"
                     };
      if (dialog.ShowDialog() == true) {
        var data = new BusinessData(dialog.Value);
        collection.Add(data);
        ApplyLayout(true, data);
      }
    }


    private void RemoveItems(ICollection<BusinessData> collection, IEnumerable itemsToRemove) {
      foreach (var item in itemsToRemove) {
        collection.Remove(item as BusinessData);
      }
      ApplyLayout(true);
    }

    #endregion

    #region UI event handlers

    /// <summary>
    /// Called when an item is dropped on the <see cref="ListView"/> of NodesSource.
    /// </summary>
    private void NodesSourceListBox_OnDrop(object sender, DragEventArgs e) {
      OnDropOnListBox(e, graphSource.NodesSource as ICollection<BusinessData>);
    }

    /// <summary>
    /// Called when an item is dropped on the <see cref="ListView"/> of Predecessors.
    /// </summary>
    private void PredecessorsListBox_OnDrop(object sender, DragEventArgs e) {
      BusinessData currentData = GetCurrentItemData();
      if (currentData != null) {
        OnDropOnListBox(e, currentData.Predecessors);
      }
    }

    /// <summary>
    /// Called when an item is dropped  on the <see cref="ListView"/> of Successors.
    /// </summary>
    private void SuccessorsListBox_OnDrop(object sender, DragEventArgs e) {
      BusinessData currentData = GetCurrentItemData();
      if (currentData != null) {
        OnDropOnListBox(e, currentData.Successors);
      }
    }

    private void OnDropOnListBox(DragEventArgs e, ICollection<BusinessData> collection) {
      var draggedData = e.Data.GetData(typeof(BusinessData)) as BusinessData;
      if (draggedData == null) {
        var node = e.Data.GetData(typeof(INode)) as INode;
        draggedData = graphSource.GetBusinessObject(node) as BusinessData;
      }
      if (draggedData != null && !collection.Contains(draggedData)) {
        collection.Add(draggedData);
        ApplyLayout(true, draggedData);
      }
    }

    private void CurrentNode_OnMouseLeave(object sender, MouseEventArgs e) {
      // initilialize drag when mouse is dragged out
      if (e.LeftButton == MouseButtonState.Pressed) {
        FrameworkElement o = e.OriginalSource as FrameworkElement;
        if (o != null) {
          var data = o.DataContext as BusinessData;
          if (data != null) {
            StartDrag(data, o, ListType.None);
          }
        }
      }
    }
    
    private void TemplateNode_OnMouseLeave(object sender, MouseEventArgs e) {
      // initilialize drag when mouse is dragged out to ensure MouseUp works
      if (e.LeftButton == MouseButtonState.Pressed) {
        FrameworkElement o = e.OriginalSource as FrameworkElement;
        if (o != null) {
          var data = o.DataContext as BusinessData;
          if (data != null) {
            // start drag with clone to get new instance every time
            StartDrag((BusinessData) data.Clone(), o, ListType.None);
          }
        }
      }
    }

    private void TemplateNode_OnMouseUp(object sender, MouseButtonEventArgs e) {
      // get data from control
      BusinessData data = templateNodeControl.Content as BusinessData;
      if (data != null) {
        // show textbox
        templateNodeTextBox.Text = data.NodeName;
        templateNodeTextBox.Visibility = Visibility.Visible;
        templateNodeTextBox.Focus();
        templateNodeTextBox.SelectAll();
      }
    }

    private void TemplateNodeTextBox_OnKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Return || e.Key == Key.Enter) {
        string text = ((TextBox) sender).Text;
        // create new business object with input text 
        templateNodeControl.DataContext = new BusinessData(text);
        templateNodeTextBox.Visibility = Visibility.Collapsed;
      }
      if (e.Key == Key.Escape) {
        // hide textbox
        templateNodeTextBox.Visibility = Visibility.Collapsed;
      } 
    }

    private void TemplateNodeTextBox_OnLostFocus(object sender, RoutedEventArgs e) {
      // hide textbox
      templateNodeTextBox.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Called when an item is dropped over the trashcan visual.
    /// </summary>
    private void Trashcan_OnDrop(object sender, DragEventArgs e) {
      object listTypeData = e.Data.GetData(typeof(ListType));
      // nodes which are dragged from the GraphControl don't have a ListType
      ListType type = listTypeData != null ? (ListType)listTypeData : ListType.NodesSource;
      var draggedData = e.Data.GetData(typeof(BusinessData)) as BusinessData;
      if (draggedData == null) {
        var node = e.Data.GetData(typeof(INode)) as INode;
        draggedData = graphSource.GetBusinessObject(node) as BusinessData;
      }
      if (draggedData != null) {
        var currentData = GetCurrentItemData();
        switch (type) {
          case ListType.NodesSource:
            // an item from NodesSource has been dragged
            ((ObservableCollection<BusinessData>)graphSource.NodesSource).Remove(draggedData);
            ApplyLayout(true);
            break;
          case ListType.Predecessors:
            // an item from Predecessors has been dragged
            if (currentData != null) {
              currentData.Predecessors.Remove(draggedData);
              ApplyLayout(true);
            }
            break;
          case ListType.Successors:
            // an item from Successors has been dragged
            if (currentData != null) {
              currentData.Successors.Remove(draggedData);
              ApplyLayout(true);
            }
            break;
          case ListType.None:
            // do nothing
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    private Point startPoint;
    private void OnListMouseDown(object sender, MouseButtonEventArgs e) {
      // remember the mouse position for drag start
      startPoint = e.GetPosition(null);
      e.Handled = true;
    }

    private void OnListMouseMove(object sender, MouseEventArgs e) {
      // Get the current mouse position
      Point mousePos = e.GetPosition(null);
      Vector diff = startPoint - mousePos;

      if (e.LeftButton == MouseButtonState.Pressed &&
          (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
          Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) {
        // Get the dragged ListViewItem
        ListView listView = sender as ListView;
        ListViewItem listViewItem =
            FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

        if (listViewItem == null) {
          // exit if for whatever reason we didn't find the ListViewItem
          return;
        }

        // get the list that the event occurred on
        var listType = listView == nodesSourceListBox ? ListType.NodesSource
                     : listView == predecessorsListBox ? ListType.Predecessors
                     : listView == successorsListBox ? ListType.Successors 
                     : ListType.None;

        FrameworkElement o = e.OriginalSource as FrameworkElement;
        if (o != null) {
          var data = o.DataContext as BusinessData;
          if (data != null) {
            StartDrag(data, o, listType);
          }
        }
      }
    }

    private void OnListMouseUp(object sender, MouseButtonEventArgs e) {
      // select the ListViewItem on mouse up to keep items from being selected at drag start
      ListViewItem listViewItem =
          FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
      if (listViewItem != null) {
        listViewItem.IsSelected = true;
        ((ListView)sender).Focus();
      }
    }

    private void ListBox_KeyDown(object sender, KeyEventArgs e) {
      switch (e.Key) {
        case Key.Delete:
          if (sender == nodesSourceListBox) {
            ((RoutedUICommand)RemoveNodeCommand).Execute(nodesSourceListBox, graphControl);
          } else if (sender == predecessorsListBox) {
            ((RoutedUICommand)RemovePredecessorCommand).Execute(predecessorsListBox, graphControl);
          } else if (sender == successorsListBox) {
            ((RoutedUICommand)RemoveSuccessorCommand).Execute(successorsListBox, graphControl);
          }
          break;
        case Key.Insert:
          if (sender == nodesSourceListBox) {
            ((RoutedUICommand)AddNodeCommand).Execute(nodesSourceListBox, graphControl);
          } else if (sender == predecessorsListBox) {
            ((RoutedUICommand)AddPredecessorCommand).Execute(predecessorsListBox, graphControl);
          } else if (sender == successorsListBox) {
            ((RoutedUICommand)AddSuccessorCommand).Execute(successorsListBox, graphControl);
          }
          break;
      }
    }

    private enum ListType
    {
      NodesSource, Predecessors, Successors, None
    }

    #endregion

    #region UI helper methods

    private static void StartDrag([NotNull]BusinessData data, FrameworkElement o, ListType listType) {
      // initialize the drag operation of a business data object
      if (o != null) {
        DataObject dao = new DataObject();
        dao.SetData(typeof (BusinessData), data);
        dao.SetData(typeof (ListType), listType);
        DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
      }
    }

    private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject {
      do {
        if (current is T) {
          return (T) current;
        }
        current = VisualTreeHelper.GetParent(current);
      } while (current != null);
      return null;
    }

    #endregion

  }

  #region Business data

  /// <summary>
  /// Represents an object of the business data.
  /// </summary>
  public class BusinessData : ICloneable
  {
    public BusinessData() :this("Unnamed") {}

    public BusinessData(string name) {
      NodeName = name;
      Successors = new ObservableCollection<BusinessData>();
      Predecessors = new ObservableCollection<BusinessData>();
    }

    public String NodeName { get; set; }
    public ObservableCollection<BusinessData> Successors { get; set; }
    public ObservableCollection<BusinessData> Predecessors { get; set; }

    public override string ToString() {
      return string.Format("Name: {0}", NodeName);
    }

    public object Clone() {
      BusinessData clone = new BusinessData(NodeName);
      foreach (var successor in Successors) {
        clone.Successors.Add(successor);
      }
      foreach (var predecessor in Predecessors) {
        clone.Predecessors.Add(predecessor);
      }
      return clone;
    }
  }

  #endregion

  #region Converters

  public class NodeToBusinessObjectConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public AdjacentNodesGraphSource GraphSource { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var node = value as INode;
      return node == null ? null : node.Tag;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return GraphSource == null ? DependencyProperty.UnsetValue : GraphSource.GetNode(value);
    }

    #endregion
  }

  public class ObjectToBoolConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is BusinessData;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }

    #endregion
  }

  #endregion

  #region Internal extension methods

  internal static class Helpers
  {
    public static bool TryGetItemsSourceCollection(this ListBox listBox, out ICollection<BusinessData> itemsCollection) {
      if (listBox == null) {
        itemsCollection = null;
        return false;
      }
      itemsCollection = listBox.ItemsSource as ICollection<BusinessData>;
      return itemsCollection != null;
    }
  }

  #endregion

}
