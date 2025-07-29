/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Partial;
using yWorks.Utils;

namespace Demo.yFiles.Graph.InteractiveClearArea
{
  /// <summary>
  ///   A demo that shows how to interactively move graph elements within a rectangular area in a given graph
  ///   layout so that the modifications in the graph are minimal. The rectangular area can be freely moved or
  ///   resized.
  /// </summary>
  public partial class InteractiveClearAreaDemo
  {
    /// <summary>
    ///   Options to control the layout behavior.
    /// </summary>
    private readonly LayoutOptions options;

    /// <summary>
    ///   The rectangular area that can be freely moved or resized.
    /// </summary>
    private MutableRectangle clearRect;

    /// <summary>
    ///   The group node we are currently inside.
    /// </summary>
    private INode groupNode;

    /// <summary>
    ///   Performs layout and animation while dragging the rectangle.
    /// </summary>
    private ClearAreaLayoutHelper layoutHelper;

    /// <summary>
    ///   A <see cref="IHitTester"/> to determine the group node we are currently hovering.
    /// </summary>
    private IHitTester nodeHitTester;

    /// <summary>
    ///   Returns the GraphControl instance used in the form.
    /// </summary>
    private GraphControl GraphControl => graphControl;

    /// <summary>
    ///   Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    private IGraph Graph => GraphControl.Graph;

    #region Initialization

    public InteractiveClearAreaDemo() {
      InitializeComponent();
      options = new LayoutOptions();

      ClearAreaStrategyComboBox.ItemsSource = new[] {
          new NamedEntry("Local", ClearAreaStrategy.Local),
          new NamedEntry("LocalUniform", ClearAreaStrategy.LocalUniform),
          new NamedEntry("PreserveShapes", ClearAreaStrategy.PreserveShapes),
          new NamedEntry("PreserveShapesUniform", ClearAreaStrategy.PreserveShapesUniform),
          new NamedEntry("Global", ClearAreaStrategy.Global)
      };

      SampleGraphComboBox.ItemsSource = new[] {
          new NamedEntry("Hierarchical", "hierarchical"), new NamedEntry("Grouping", "grouping"),
          new NamedEntry("Organic", "organic"), new NamedEntry("Orthogonal", "orthogonal"),
          new NamedEntry("Circular", "circular"), new NamedEntry("Tree", "tree"),
          new NamedEntry("Radial Tree", "radialtree"), new NamedEntry("Series-Parallel", "seriesparallel"),
          new NamedEntry("Components", "components")
      };

      ComponentAssignmentStrategyComboBox.ItemsSource = new[] {
          new NamedEntry("Single", ComponentAssignmentStrategy.Single),
          new NamedEntry("Connected", ComponentAssignmentStrategy.Connected),
          new NamedEntry("Clustering", ComponentAssignmentStrategy.Clustering)
      };
    }

    private void OnLoad(object sender, EventArgs e) {
      InitializeInputModes();
      InitializeGraph();

      SampleGraphComboBox.SelectedIndex = 0;
      ClearAreaStrategyComboBox.SelectedIndex = 2;
      ComponentAssignmentStrategyComboBox.SelectedIndex = 0;
      ConsiderEdgesToggleButton.IsChecked = true;
    }

    /// <summary>
    ///   Registers the <see cref="GraphEditorInputMode"/> as the <see cref="CanvasControl.InputMode"/>
    ///   and initializes the rectangular area so that it is drawn and can be moved and resized.
    /// </summary>
    private void InitializeInputModes() {
      // create a GraphEditorInputMode instance
      var editMode = new GraphEditorInputMode();

      // and install the edit mode into the canvas.
      GraphControl.InputMode = editMode;

      // create the model for the rectangular area
      clearRect = new MutableRectangle(0, 0, 100, 100);

      // visualize it
      var pen = new Pen(new SolidColorBrush(new Color { A = 166, R = 77, G = 121, B = 153 }), 3);
      var brush = new SolidColorBrush(new Color { A = 166, R = 0, G = 167, B = 255 });
      var rectangleRenderer = new RectangleRenderer((Pen) pen.GetAsFrozen(), (Brush) brush.GetAsFrozen());
      var renderTree = GraphControl.RenderTree;
      renderTree.CreateElement(renderTree.HighlightGroup, clearRect, rectangleRenderer);

      AddClearRectInputModes(GraphControl.InputModeContext, editMode);
    }

    /// <summary>
    ///   Enables undo/redo support and initializes the default styles.
    /// </summary>
    protected virtual void InitializeGraph() {
      GraphControl.Graph.SetUndoEngineEnabled(true);
      DemoStyles.InitDemoStyles(GraphControl.Graph);
    }

    /// <summary>
    ///   Adds the input modes that handle the resizing and movement of the rectangular area.
    /// </summary>
    /// <param name="graphControlInputModeContext"></param>
    /// <param name="inputMode"></param>
    private void AddClearRectInputModes(
        IInputModeContext graphControlInputModeContext,
        MultiplexingInputMode inputMode) {
      // create handles for interactively resizing the rectangle
      var rectangleHandles = new RectangleReshapeHandleProvider(clearRect) { MinimumSize = new SizeD(10, 10) };

      // create a mode that deals with the handles
      var handleInputMode = new HandleInputMode { Priority = 1 };

      // add it to the graph editor mode
      inputMode.Add(handleInputMode);

      // now the handles
      var inputModeContext = new InputModeContext(graphControlInputModeContext);
      handleInputMode.Handles = new ObservableCollection<IHandle> {
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.TopRight),
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.TopLeft),
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.BottomRight),
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.BottomLeft)
      };

      // create a mode that allows for dragging the rectangle at the sides
      var moveInputMode = new MoveInputMode {
          PositionHandler = new RectanglePositionHandler(clearRect),
          HitTestable = HitTestables.Create((context, location) => clearRect.Contains(location)),
          Priority = 41,
          DirectionalConstraintRecognizer = EventRecognizers.Never
      };

      // handle dragging the rectangle
      moveInputMode.DragStarting += OnDragStarting;
      moveInputMode.Dragged += OnDragged;
      moveInputMode.DragCanceled += OnDragCanceled;
      moveInputMode.DragFinished += OnDragFinished;

      // handle resizing the rectangle
      handleInputMode.DragStarting += OnDragStarting;
      handleInputMode.Dragged += OnDragged;
      handleInputMode.DragCanceled += OnDragCanceled;
      handleInputMode.DragFinished += OnDragFinished;

      // add it to the edit mode
      inputMode.Add(moveInputMode);
    }

    #endregion

    #region Dragging the rectangular area

    /// <summary>
    ///   The rectangular area is upon to be moved or resized.
    /// </summary>
    private void OnDragStarting(object sender, InputModeEventArgs e) {
      nodeHitTester = e.Context.Lookup<IHitTester>();
      layoutHelper = new ClearAreaLayoutHelper(GraphControl, clearRect, options);
      layoutHelper.InitializeLayout();
    }

    /// <summary>
    ///   The rectangular area is currently be moved or resized.
    ///   For each drag a new layout is calculated and applied if the previous one is completed.
    /// </summary>
    private void OnDragged(object sender, InputModeEventArgs e) {
      if (IsShiftPressed(e)) {
        // We do not change the layout now, instead we check if we are hovering a group node.
        // If so, we use that group node inside which the cleared area should be located.
        // In addition, the group node is highlighted to better recognize him.
        if (nodeHitTester != null) {
          var hitGroupNode = GetHitGroupNode(e.Context);
          if (hitGroupNode != groupNode) {
            if (groupNode != null) {
              GraphControl.Highlights.Remove(groupNode);
            }
            if (hitGroupNode != null) {
              GraphControl.Highlights.Add(hitGroupNode);
            }
            groupNode = hitGroupNode;
          }
        }
      } else {
        if (IsShiftChanged(e) && groupNode != null) {
          // now we remove the highlight of the group
          GraphControl.Highlights.Remove(groupNode);
        }

        // invoke the layout calculation and animation
        layoutHelper.GroupNode = groupNode;
        layoutHelper.RunLayout();
      }
    }

    /// <summary>
    ///   Moving or resizing the rectangular area has been canceled.
    ///   The state before the gesture must be restored.
    /// </summary>
    private void OnDragCanceled(object sender, InputModeEventArgs e) {
      layoutHelper.CancelLayout();
      groupNode = null;
    }

    /// <summary>
    ///   Moving or resizing the rectangular area has been finished.
    ///   We execute the layout to the final state.
    /// </summary>
    private void OnDragFinished(object sender, InputModeEventArgs e) {
      layoutHelper.StopLayout();
      groupNode = null;
    }

    /// <summary>
    ///   Determines the group node on that the mouse is currently hovering. If there is no
    ///   group node <code>null</code> is returned.
    /// </summary>
    private INode GetHitGroupNode(IInputModeContext context) {
      return nodeHitTester.EnumerateHits(context, context.CanvasControl.LastPointerEvent.Location, GraphItemTypes.Node)
          .Cast<INode>().FirstOrDefault(n => Graph.IsGroupNode(n));
    }

    /// <summary>
    ///   Determines whether <see cref="ModifierKeys.Shift"/> is currently is pressed.
    /// </summary>
    private static bool IsShiftPressed(InputModeEventArgs e) {
      return (e.Context.CanvasControl.LastPointerEvent.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
    }

    /// <summary>
    ///   Determines whether <see cref="ModifierKeys.Shift"/> state has been changed.
    /// </summary>
    private static bool IsShiftChanged(InputModeEventArgs e) {
      return (e.Context.CanvasControl.LastPointerEvent.ChangedModifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
    }

    #endregion

    #region User interface event handling

    /// <summary>
    ///   Applies a new <see cref="ClearAreaStrategy"/>.
    /// </summary>
    private void ClearAreaStrategyChanged(object sender, SelectionChangedEventArgs e) {
      if (ClearAreaStrategyComboBox.SelectedItem is NamedEntry selectedItem) {
        options.ClearAreaStrategy = (ClearAreaStrategy) selectedItem.Value;
      }
    }

    /// <summary>
    ///   Applies a new <see cref="ComponentAssignmentStrategy"/>.
    /// </summary>
    private void ComponentAssignmentStrategyChanged(object sender, SelectionChangedEventArgs e) {
      if (ComponentAssignmentStrategyComboBox.SelectedItem is NamedEntry selectedItem) {
        options.ComponentAssignmentStrategy = (ComponentAssignmentStrategy) selectedItem.Value;
      }
    }

    /// <summary>
    ///   Whether or not to allow edges in the rectangular area.
    /// </summary>
    private void ToggleConsiderEdges(object sender, RoutedEventArgs e) {
      if (ConsiderEdgesToggleButton.IsChecked != null) {
        options.ConsiderEdges = ConsiderEdgesToggleButton.IsChecked.Value;
      }
    }

    /// <summary>
    ///   Loads a new sample graph.
    /// </summary>
    private void SampleGraphChanged(object sender, SelectionChangedEventArgs e) {
      if (SampleGraphComboBox.SelectedItem is NamedEntry selectedItem) {
        GraphControl.ImportFromGraphML("Resources\\" + selectedItem.Value + ".graphml");
        clearRect.Location = GraphControl.ViewPoint + new PointD(50, 50);
        CheckSampleButtonStates();
      }
    }

    /// <summary>
    ///   Loads the previous sample graph.
    /// </summary>
    private void LoadPreviousSampleGraph(object sender, RoutedEventArgs e) {
      if (SampleGraphComboBox.SelectedIndex <= 0) {
        return;
      }
      SampleGraphComboBox.SelectedIndex--;
    }

    /// <summary>
    ///   Loads the next sample graph.
    /// </summary>
    private void LoadNextSampleGraph(object sender, RoutedEventArgs e) {
      if (SampleGraphComboBox.SelectedIndex >= SampleGraphComboBox.Items.Count - 1) {
        return;
      }
      SampleGraphComboBox.SelectedIndex++;
    }

    /// <summary>
    ///   Updates the enabled state of the next and previous buttons.
    /// </summary>
    private void CheckSampleButtonStates() {
      var maxReached = SampleGraphComboBox.SelectedIndex == SampleGraphComboBox.Items.Count - 1;
      var minReached = SampleGraphComboBox.SelectedIndex == 0;
      NextSample.IsEnabled = true;
      PreviousSample.IsEnabled = true;
      if (maxReached) {
        NextSample.IsEnabled = false;
      } else if (minReached) {
        PreviousSample.IsEnabled = false;
      }
    }

    #endregion

    /// <summary>
    ///   Name-value struct for combo box entries.
    /// </summary>
    private readonly struct NamedEntry
    {
      public object Value { get; }

      private string DisplayName { get; }

      public NamedEntry(string displayName, object value) {
        DisplayName = displayName;
        Value = value;
      }

      public override string ToString() {
        return DisplayName;
      }
    }

    /// <summary>
    ///   Options to control the layout behavior.
    /// </summary>
    internal sealed class LayoutOptions
    {
      public ClearAreaStrategy ClearAreaStrategy { get; set; }

      public ComponentAssignmentStrategy ComponentAssignmentStrategy { get; set; }

      public bool ConsiderEdges { get; set; }
    }
  }
}
