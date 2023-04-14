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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchic;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.DataBinding.InteractiveEdgesGraphSource
{
  /// <summary>
  /// This class implements the main logic for this demo.
  /// </summary>
  /// <remarks>
  /// This demo shows how to use <see cref="AdjacentEdgesGraphSource"/> to build a graph from business data. 
  /// Each business data item defines a list of ingoing and outgoing edges. The business data contained in 
  /// these lists defines the source or target node of the edge.
  /// </remarks>
  public partial class InteractiveEdgesGraphSourceWindow
  {
    private AdjacentEdgesGraphSource graphSource;

    public InteractiveEdgesGraphSourceWindow() {
      InitializeComponent();
    }

    #region custom commands

    public static readonly ICommand AddNodeCommand = new RoutedUICommand("New Class", "AddNode",
                                                                         typeof(InteractiveEdgesGraphSourceWindow));

    public static readonly ICommand RemoveNodeCommand = new RoutedUICommand("Delete Class", "RemoveNode",
                                                                            typeof(InteractiveEdgesGraphSourceWindow));

    public static readonly ICommand AddEventRegistrationCommand = new RoutedUICommand("Register New Event", "AddInEdge",
                                                                                      typeof(InteractiveEdgesGraphSourceWindow));

    public static readonly ICommand RemoveEventRegistrationCommand = new RoutedUICommand("Delete Registered Event", "RemoveInEdge",
                                                                                         typeof(InteractiveEdgesGraphSourceWindow));

    public static readonly ICommand AddMethodCallCommand = new RoutedUICommand("New Method Call", "AddOutEdge",
                                                                               typeof(InteractiveEdgesGraphSourceWindow));

    public static readonly ICommand RemoveMethodCallCommand = new RoutedUICommand("Delete Method Call", "RemoveOutEdge",
                                                                                  typeof(InteractiveEdgesGraphSourceWindow));

    private void RemoveCanExecute(object sender, CanExecuteRoutedEventArgs e) {
      var listBox = e.Parameter as ListBox;
      e.CanExecute = listBox != null && listBox.SelectedItems.Count > 0;
      e.Handled = true;
    }

    private void AddConnectionCanExecute(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = GetCurrentItemData() != null;
      e.Handled = true;
    }

    public async void AddNodeExecuted(object sender, ExecutedRoutedEventArgs e) {
      CreateNodeDialog createNodeDialog = new CreateNodeDialog();
      if (createNodeDialog.ShowDialog() == true) {
        EntityData data;
        switch (createNodeDialog.Type) {
          case CreateNodeDialog.ItemType.Class:
            data = new ClassData(createNodeDialog.NodeName, createNodeDialog.MethodsList, createNodeDialog.EventsList);
            break;
          case CreateNodeDialog.ItemType.Interface:
            data = new InterfaceData(createNodeDialog.NodeName, createNodeDialog.MethodsList, createNodeDialog.EventsList);
            break;
          default:
            return;
        }
        ((ObservableCollection<EntityData>) graphSource.NodesSource).Add(data);
        await ApplyLayout(true, data);
      }
    }

    public async void RemoveNodeExecuted(object sender, ExecutedRoutedEventArgs e) {
      foreach (var item in new ArrayList(nodesSourceListBox.SelectedItems)) {
        EntityData data = item as EntityData;
        if (data != null) {
          ((ObservableCollection<EntityData>)graphSource.NodesSource).Remove(data);
        }
      }
      await ApplyLayout(true);
    }

    public async void AddEventRegistrationExecuted(object sender, ExecutedRoutedEventArgs e) {
      AddConnectionDialog addConnectionDialog = new AddConnectionDialog(graphSource, AddConnectionDialog.CreationMode.EventRegistration);
      if (addConnectionDialog.ShowDialog() == true) {
        await AddEventRegistration(addConnectionDialog.DataElement, addConnectionDialog.ConnectionName);
      }
    }

    public async void RemoveEventRegistrationExecuted(object sender, ExecutedRoutedEventArgs e) {
      var currentData = GetCurrentItemData();
      if (currentData != null) {
        foreach (var item in new ArrayList(eventsListBox.SelectedItems)) {
          EventRegistrationData data = item as EventRegistrationData;
          if (data != null) {
            currentData.EventRegistrations.Remove(data);
          }
        }
        await ApplyLayout(true);
      }
    }

    public async void AddMethodCallExecuted(object sender, ExecutedRoutedEventArgs e) {
      AddConnectionDialog addConnectionDialog = new AddConnectionDialog(graphSource, AddConnectionDialog.CreationMode.MethodCall);
      if (addConnectionDialog.ShowDialog() == true) {
        await AddMethodCall(addConnectionDialog.DataElement, addConnectionDialog.ConnectionName);
      }
    }

    public async void RemoveMethodCallExecuted(object sender, ExecutedRoutedEventArgs e) {
      var currentData = GetCurrentItemData();
      if (currentData != null) {
        foreach (var item in new ArrayList(methodsListBox.SelectedItems)) {
          MethodCallData data = item as MethodCallData;
          if (data != null) {
            currentData.MethodCalls.Remove(data);
          }
        }
        await ApplyLayout(true);
      }
    }

    #endregion

    private async void GraphSourceWindow_OnLoaded(object sender, RoutedEventArgs e) {
      graphSource = ((AdjacentEdgesGraphSource)Application.Current.MainWindow.Resources["GraphSource"]);

      // always show focus indicator
      graphControl.FocusIndicatorManager.ShowFocusPolicy = ShowFocusPolicy.Always;

      graphSource.EdgeDefaults.Labels.LayoutParameter = new FreeEdgeLabelModel().CreateDefaultParameter();

      // use custom PolylineEdgeStyleRenderer wrapper that switches the edge color 
      // based on the attached business data
      graphSource.EdgeDefaults.Style = new PolylineEdgeStyle(new MyPolylineEdgeStyleRenderer(graphSource));
      
      // create inital business data
      CreateSampleData();

      await ApplyLayout(false);
    }

    private void CreateSampleData() {
      // create the sample nodes, defining a set of methods and events
      var collectionView = new ClassData("CollectionView", 
        new List<string> {"GetEnumerator()"},
        new List<string> {"CollectionChanged"});
      var adjacentEdgesGraphSource = new ClassData("AdjacentEdgesGraphSource",
        new List<string> {"GetBusinessObject()", "GetNode()", "GetGroup()", "GetNode()"},
        new List<string>());
      var iGraph = new InterfaceData("IGraph",
        new List<string> {"Contains()", "Remove()", "CreateEdge()", "AddLabel()", "CreateNode()"},
        new List<string> {"NodeRemoved", "NodeCreated"});
      var iGroupedGraph = new ClassData("IGroupedGraph", 
        new List<string> {"CreateNode()", "CreateGroupNode()"},
        new List<string>());
      var interactiveEdgesGraphSourceWindow = new ClassData("InteractiveEdgesGraphSourceWindow", 
        new List<string>(),
        new List<string>());

      // add connections between the nodes
      adjacentEdgesGraphSource.EventRegistrations.Add(new EventRegistrationData(collectionView, "CollectionChanged"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(collectionView, "GetEnumerator()"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(iGraph, "Contains()"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(iGraph, "Remove()"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(iGraph, "CreateEdge()"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(iGraph, "AddLabel()"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(iGroupedGraph, "CreateNode()"));
      adjacentEdgesGraphSource.MethodCalls.Add(new MethodCallData(iGroupedGraph, "CreateGroupNode()"));
      iGroupedGraph.MethodCalls.Add(new MethodCallData(iGraph, "CreateNode()"));
      iGroupedGraph.EventRegistrations.Add(new EventRegistrationData(iGraph, "NodeRemoved"));
      iGroupedGraph.EventRegistrations.Add(new EventRegistrationData(iGraph, "NodeCreated"));
      interactiveEdgesGraphSourceWindow.MethodCalls.Add(new MethodCallData(adjacentEdgesGraphSource, "GetBusinessObject()"));

      graphSource.NodesSource = new ObservableCollection<EntityData>
                                  {
                                    collectionView, adjacentEdgesGraphSource, iGraph, iGroupedGraph, interactiveEdgesGraphSourceWindow
                                  };
    }

    /// <summary>
    /// Adds a new event registration to the current item
    /// </summary>
    /// <param name="source">The event source</param>
    /// <param name="name">The event name</param>
    private async Task AddEventRegistration(EntityData source, string name) {
      var currentData = GetCurrentItemData();
      if (currentData != null) {
        currentData.EventRegistrations.Add(new EventRegistrationData(source, name));
        await ApplyLayout(true);
      }
    }

    /// <summary>
    /// Adds a new method call to the current item
    /// </summary>
    /// <param name="target">The method call target</param>
    /// <param name="name">The method name</param>
    private async Task AddMethodCall(EntityData target, string name) {
      var currentData = GetCurrentItemData();
      if (currentData != null) {
        currentData.MethodCalls.Add(new MethodCallData(target, name));
        await ApplyLayout(true);
      }
    }

    /// <summary>
    /// Returns the business data object from the current item, or <see langword="null"/> if no current item is set
    /// </summary>
    private EntityData GetCurrentItemData() {
      return graphControl.CurrentItem != null ? graphSource.GetBusinessObject(graphControl.CurrentItem) as EntityData : null;
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
    private async Task ApplyLayout(bool incremental, params EntityData[] incrementalNodes) {
      var layout = new HierarchicLayout {IntegratedEdgeLabeling = true, OrthogonalRouting = true};
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

    #region UI event handlers

    private Point startPoint;
    private void OnListMouseDown(object sender, MouseButtonEventArgs e) {
      // remember the mouse position for drag start
      startPoint = e.GetPosition(null);
      e.Handled = true;
    }

    private void CurrentNode_OnMouseLeave(object sender, MouseEventArgs e) {
      // initilialize drag when mouse is dragged out
      if (e.LeftButton == MouseButtonState.Pressed) {
        FrameworkElement o = e.OriginalSource as FrameworkElement;
        if (o != null) {
          var data = o.DataContext as EntityData;
          if (data != null) {
            DataObject dao = new DataObject();
            dao.SetData(typeof (EntityData), data);
            DragDrop.DoDragDrop((FrameworkElement) sender, dao,
                                DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          }
        }
      }
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
            FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

        if (listViewItem == null) {
          return;
        }

        // Find the data behind the ListViewItem
        object data = listViewItem.DataContext;
        if (data != null) {
          DataObject dao = new DataObject();
          if (listView == nodesSourceListBox) {
            dao.SetData(typeof(EntityData), data);
          } else if (listView == eventsListBox) {
            dao.SetData(typeof(EventRegistrationData), data);
          } else if (listView == methodsListBox) {
            dao.SetData(typeof(MethodCallData), data);
          }

          DragDrop.DoDragDrop(listViewItem, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        }
      }
    }

    private void OnListMouseUp(object sender, MouseButtonEventArgs e) {
      // select the ListViewItem on mouse up to keep items from being selected at drag start
      ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
      if (listViewItem != null) {
        listViewItem.IsSelected = true;
        ((ListView)sender).Focus();
      }
    }

    /// <summary>
    /// Called when a dragged item has been dropped over the NodesSource list.
    /// </summary>
    private async void NodesSourceListBox_OnDrop(object sender, DragEventArgs e) {
      object draggedData = e.Data.GetData(typeof(EntityData)) ??
                           e.Data.GetData(typeof(EventRegistrationData)) ??
                           e.Data.GetData(typeof(MethodCallData));
      if (draggedData != null) {
        // get the data item
        EntityData entityData = null;
        if (draggedData is EntityData) {
          entityData = (EntityData)draggedData;
        } else if (draggedData is EventRegistrationData) {
          entityData = ((EventRegistrationData)draggedData).Source;
        } else if (draggedData is MethodCallData) {
          entityData = ((MethodCallData)draggedData).Target;
        }
        if (entityData != null) {
          // add the data item to the NodesSource
          ObservableCollection<EntityData> collection = graphSource.NodesSource as ObservableCollection<EntityData>;
          if (collection != null && !collection.Contains(entityData)) {
            collection.Add(entityData);
            await ApplyLayout(true, entityData);
          }
        }
      }
    }

    /// <summary>
    /// Called when a dragged item has been dropped over the Events list.
    /// </summary>
    private async void EventsListBox_OnDrop(object sender, DragEventArgs e) {
      // only items dragged from NodesSource can be dropped on Events
      await CreateConnection_OnDrop(e, AddConnectionDialog.CreationMode.EventRegistration, AddEventRegistration);
    }

    /// <summary>
    /// Called when a dragged item has been dropped over the Methods list.
    /// </summary>
    private async void MethodsListBox_OnDrop(object sender, DragEventArgs e) {
      // only items dragged from NodesSource can be dropped on Methods
      await CreateConnection_OnDrop(e, AddConnectionDialog.CreationMode.MethodCall, AddMethodCall);
    }

    private async Task CreateConnection_OnDrop(DragEventArgs e, AddConnectionDialog.CreationMode creationMode,
                                         CreateConnectionCommand createConnectionCommand) {
      EntityData draggedData = e.Data.GetData(typeof (EntityData)) as EntityData;
      EntityData currentData = GetCurrentItemData();
      if (draggedData != null && currentData != null) {
        // show the dialog to choose which method to add a call to
        AddConnectionDialog addConnectionDialog = new AddConnectionDialog(graphSource, creationMode);
        // select the dragged item
        addConnectionDialog.Preselect(draggedData);
        if (addConnectionDialog.ShowDialog() == true) {
          await createConnectionCommand(addConnectionDialog.DataElement, addConnectionDialog.ConnectionName);
        }
      }
    }

    private delegate Task CreateConnectionCommand(EntityData data, string name);

    /// <summary>
    /// Deletes the dragged data
    /// </summary>
    private async void Trashcan_OnDrop(object sender, DragEventArgs e) {
      object draggedData = e.Data.GetData(typeof (EntityData)) ??
                           e.Data.GetData(typeof (EventRegistrationData)) ??
                           e.Data.GetData(typeof (MethodCallData));
      if (draggedData != null) {
        var currentData = GetCurrentItemData();
        if (draggedData is EntityData) {
          // node from NodesSource has been dragged
          ((ObservableCollection<EntityData>)graphSource.NodesSource).Remove((EntityData) draggedData);
        } else if (draggedData is EventRegistrationData) {
          // event registration has been dragged
          currentData.EventRegistrations.Remove((EventRegistrationData) draggedData);
        } else if (draggedData is MethodCallData) {
          // method call has been dragged
          currentData.MethodCalls.Remove((MethodCallData) draggedData);
        }
        await ApplyLayout(true);
      }
    }

    private void ListBox_KeyDown(object sender, KeyEventArgs e) {
      switch (e.Key) {
        case Key.Delete:
          if (sender == nodesSourceListBox) {
            ((RoutedUICommand)RemoveNodeCommand).Execute(null, graphControl);
          } else if (sender == eventsListBox) {
            ((RoutedUICommand)RemoveEventRegistrationCommand).Execute(null, graphControl);
          } else if (sender == methodsListBox) {
            ((RoutedUICommand)RemoveMethodCallCommand).Execute(null, graphControl);
          }
          break;
        case Key.Insert:
          if (sender == nodesSourceListBox) {
            ((RoutedUICommand)AddNodeCommand).Execute(null, graphControl);
          } else if (sender == eventsListBox) {
            ((RoutedUICommand)AddEventRegistrationCommand).Execute(null, graphControl);
          } else if (sender == methodsListBox) {
            ((RoutedUICommand)AddMethodCallCommand).Execute(null, graphControl);
          }
          break;
      }
    }

    #endregion

    #region UI helper methods

    private static T FindAnchestor<T>(DependencyObject current)
      where T : DependencyObject {
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

  /// <summary>
  /// A customized edge style renderer that changes the edge color based on the business data
  /// </summary>
  internal class MyPolylineEdgeStyleRenderer : PolylineEdgeStyleRenderer
  {
    private readonly AdjacentEdgesGraphSource graphSource;
    private readonly IArrow methodArrow = new Arrow(Colors.DimGray) { Type = ArrowType.Short }.GetAsFrozen();
    private readonly IArrow eventArrow = new Arrow(Colors.DarkRed) { Type = ArrowType.Short }.GetAsFrozen();

    public MyPolylineEdgeStyleRenderer([NotNull] AdjacentEdgesGraphSource graphSource) {
      this.graphSource = graphSource;
    }

    protected override Pen GetPen() {
      var businessObject = graphSource.GetBusinessObject(Edge);
      if (businessObject is MethodCallData) {
        // return gray pen for method call edges
        return Pens.DimGray;
      } if (businessObject is EventRegistrationData) {
        // return red pen for event edges
        return Pens.DarkRed;
      }
      return base.GetPen();
    }

    protected override IArrow GetTargetArrow() {
      // return colored arrow
      var businessObject = graphSource.GetBusinessObject(Edge);
      if (businessObject is MethodCallData) {
        return methodArrow;
      } if (businessObject is EventRegistrationData) {
        return eventArrow;
      }
      return base.GetTargetArrow();
    }
  }

  #region Business Data

  /// <summary>
  /// An entity which represents a node.
  /// </summary>
  /// <remarks>Base for <see cref="ClassData"/> and <see cref="InterfaceData"/></remarks>
  public abstract class EntityData
  {
    private readonly List<string> methods;
    private readonly List<string> events;

    protected EntityData() : this(new List<string>(), new List<string>()) { }

    protected EntityData(List<string> methods, List<string> events) {
      this.methods = methods;
      this.events = events;
      EventRegistrations = new ObservableCollection<EventRegistrationData>();
      MethodCalls = new ObservableCollection<MethodCallData>();
    }

    public ObservableCollection<EventRegistrationData> EventRegistrations { get; private set; }

    public ObservableCollection<MethodCallData> MethodCalls { get; private set; }

    public string Name { get; protected set; }

    public List<string> Methods {
      get { return methods; }
    }

    public List<string> Events {
      get { return events; }
    }
  }

  /// <summary>
  /// Represents a class.
  /// </summary>
  public class ClassData : EntityData 
  {
    public ClassData(string name, List<string> methods, List<string> events) : base(methods, events) {
      Name = name;
    }

    public override string ToString() {
      return string.Format("Class {0}", Name);
    }
  }

  /// <summary>
  /// Represents an interface.
  /// </summary>
  public class InterfaceData : EntityData
  {
    public InterfaceData(string name, List<string> methods, List<string> events) : base(methods, events) {
        Name = name;
    }

    public override string ToString() {
      return string.Format("Interface {0}", Name);
    }
  }

  /// <summary>
  /// Represents a method call.
  /// </summary>
  public class MethodCallData
  {
    public MethodCallData(EntityData target, string methodName) {
      Target = target;
      Name = methodName;
    }

    public EntityData Target { get; private set; }

    public string Name { get; private set; }
  }

  /// <summary>
  /// Represents an event registration.
  /// </summary>
  public class EventRegistrationData
  {
    public EventRegistrationData(EntityData source, string eventName) {
      Source = source;
      Name = eventName;
    }

    public EntityData Source { get; private set; }

    public string Name { get; private set; }
  }

  #endregion

  #region Converters

  public class NodeToBusinessObjectConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public AdjacentEdgesGraphSource GraphSource { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      INode node = value as INode;
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
      return value is EntityData;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }

    #endregion
  }

  #endregion
}
