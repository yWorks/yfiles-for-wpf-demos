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
using System.IO;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Utils;

namespace Demo.yFiles.Graph.ComponentDragAndDrop
{
  /// <summary>
  /// A demo that shows how to make space for a subgraph that ia dragged and dropped from a palette
  /// onto the canvas.
  /// </summary>
  public partial class ComponentDragAndDropDemo
  {
    /// <summary>
    /// Performs layout and animation during the drag and drop operation.
    /// </summary>
    private ClearAreaLayoutHelper layoutHelper;

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    private IGraph Graph {
      get { return GraphControl.Graph; }
    }

    /// <summary>
    /// Components that should not be modified by the layout.
    /// </summary>
    private readonly DictionaryMapper<INode, object> components;


    #region Initialization

    /// <summary>
    /// Wires up the UI components.
    /// </summary>
    public ComponentDragAndDropDemo() {
      InitializeComponent();
      components = new DictionaryMapper<INode, object>();
    }

    /// <summary>
    /// Initializes the graph and the input mode.
    /// </summary>
    private void OnLoad(object sender, EventArgs e) {
      InitializeInputModes();
      InitializeGraph();
      PopulatePalette();
    }

    /// <summary>
    /// Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    /// and initializes the input mode for dropping components.
    /// </summary>
    private void InitializeInputModes() {
      // create a GraphEditorInputMode instance
      var editMode = new GraphEditorInputMode();
      GraphControl.InputMode = editMode;

      // add the input mode to drop components
      var graphDropInputMode = new ComponentDropInputMode();
      graphDropInputMode.DragEntered += OnDragStarting;
      graphDropInputMode.DragOver += OnDragged;
      graphDropInputMode.ItemCreated += OnDragFinished;
      graphDropInputMode.DragLeft += OnDragCanceled;
      editMode.Add(graphDropInputMode);
    }

    /// <summary>
    /// Enables undo/redo support and initializes the default styles.
    /// </summary>
    protected virtual void InitializeGraph() {
      GraphControl.Graph.SetUndoEngineEnabled(true);
      
      Graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = Brushes.DarkGray,
          Pen = null
      };

      Graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(Brushes.DarkGray, 5)
      };
    }

    /// <summary>
    /// Populates the palette with the graphs stored in the resources folder.
    /// </summary>
    private void PopulatePalette() {
      var ioHandler = new GraphMLIOHandler();
      foreach (var file in Directory.GetFiles("Resources")) {
        var graph = new DefaultGraph();
        ioHandler.Read(graph, file);
        PaletteListBox.Items.Add(graph);
      }
    }

    #endregion

    #region Dragging the component

    /// <summary>
    /// The component is upon to be moved or resized.
    /// </summary>
    private void OnDragStarting(object sender, InputModeEventArgs e) {
      var graphDropInputMode = sender as ComponentDropInputMode;
      if (graphDropInputMode != null) {
        var component = graphDropInputMode.DropData as IGraph;
        layoutHelper = new ClearAreaLayoutHelper(GraphControl, component, ConsiderComponents.IsChecked == true ? components : null);
        layoutHelper.InitializeLayout();
      }
    }

    /// <summary>
    /// The component is currently be moved or resized.
    /// For each drag a new layout is calculated and applied if the previous one is completed.
    /// </summary>
    private void OnDragged(object sender, InputModeEventArgs e) {
      var graphDropInputMode = sender as ComponentDropInputMode;
      if (graphDropInputMode != null) {
        layoutHelper.Location = graphDropInputMode.MousePosition.ToPointD();
        layoutHelper.RunLayout();
      }
    }

    /// <summary>
    /// Dragging the component has been canceled.
    /// The state before the gesture must be restored.
    /// </summary>
    private void OnDragCanceled(object sender, InputModeEventArgs e) {
      var graphDropInputMode = sender as ComponentDropInputMode;
      if (graphDropInputMode != null) {
        layoutHelper.CancelLayout();
      }
    }

    /// <summary>
    /// The component has been dropped.
    /// We execute the layout to the final state.
    /// </summary>
    private void OnDragFinished(object sender, ItemEventArgs<IGraph> itemEventArgs) {
      var graphDropInputMode = sender as ComponentDropInputMode;
      if (graphDropInputMode != null) {
        layoutHelper.Location = graphDropInputMode.DropLocation;
        layoutHelper.Component = itemEventArgs.Item;
        // specify the dropped nodes as a single component
        var componentId = new object();
        foreach (var node in itemEventArgs.Item.Nodes) {
          components[node] = componentId;
        }
        layoutHelper.FinishLayout();
      }
    }

    #endregion

    /// <summary>
    /// Initializes the information for a drag and drop operation from the component list.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTemplateMouseDown(object sender, RoutedEventArgs e) {
      var element = e.OriginalSource as FrameworkElement;
      if (element != null) {
        var graph = element.DataContext as IGraph;
        if (graph != null) {
          var dao = new DataObject();
          dao.SetData(typeof(IGraph), graph);
          DragDrop.DoDragDrop(element, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        }
      }
    }
  }
}
