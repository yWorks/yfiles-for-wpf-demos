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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;

namespace yWorks.Demo.NodeAlignment
{
  /// <summary>
  ///   Shows how to automatically align nodes in rows and columns.
  /// </summary>
  [Demo("node-alignment", "layout", "Node Alignment",
      Summary = "Shows how to align nodes on horizontal and/or vertical lines.",
      Tags = ["layout", "interactive layout"],
      Keywords = ["layoutfeatures", "node alignment", "snapping", "interactive", "drag", "drop"], RelatedDemos = [])]
  public sealed partial class NodeAlignmentDemo
  {
    private readonly Settings settings = ConfigureLayout.CreateDefaultSettings();
    private readonly DispatcherTimer settingsLayoutTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private SnapDistanceVisualCreator snapDistanceVisualCreator = null!;
    private IRenderTreeElement snapDistanceRenderTreeElement = null!;
    private bool initialized;
    private bool layoutRunning;
    private bool layoutPending;

    public NodeAlignmentDemo() {
      InitializeComponent();
      interactionPanel.DataContext = settings;
      settings.SettingChanged += (_, _) => ScheduleAlignment();
      settingsLayoutTimer.Tick += async (_, _) => {
        settingsLayoutTimer.Stop();
        await AlignNodes();
      };
    }

    private IGraph Graph => graphControl.Graph;

    protected override async void Initialize() {
      if (initialized) {
        return;
      }
      initialized = true;

      // Initialize the graph component and populate the sample graph.
      DemoStyles.InitDemoStyles(Graph, Themes.PaletteLightblue);
      BuildGraphFromJson("node-alignment-data.json", Graph);

      // For new nodes, use colors different from the colors used for nodes in the sample graph.
      Graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle(Themes.PaletteOrange);
      Graph.NodeDefaults.Size = new SizeD(30, 30);

      // Enable undo and redo for this demo.
      Graph.SetUndoEngineEnabled(true);

      graphControl.FocusIndicatorManager.Enabled = false;

      // Create and populate a palette component from which new nodes may be dragged and dropped
      // into the demo's graph component.
      CreateDragPanel();

      // Enable interactive editing and show hints for areas that will result in node alignment.
      ConfigureInteraction();

      await graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates and populates a palette that provides templates for new nodes.
    ///   Dragging a template from the palette and dropping it in the demo's graph component creates
    ///   a new node that looks like the dropped template.
    /// </summary>
    private void CreateDragPanel() {
      var panel = new DragAndDropPanel(palettePanel) { MaxItemWidth = 160 };
      panel.PopulatePanel(CreateNodeTemplates());
    }

    /// <summary>
    ///   Creates the set of templates for the demo's palette.
    /// </summary>
    private static IEnumerable<IModelItem> CreateNodeTemplates() => [
        CreateNodeTemplate(30, 30, ShapeNodeShape.RoundRectangle),
        CreateNodeTemplate(90, 30, ShapeNodeShape.RoundRectangle),
        CreateNodeTemplate(30, 30, ShapeNodeShape.Diamond),
        CreateNodeTemplate(90, 30, ShapeNodeShape.Diamond),
        CreateNodeTemplate(30, 30, ShapeNodeShape.Ellipse),
        CreateNodeTemplate(90, 30, ShapeNodeShape.Ellipse)
    ];

    private static INode CreateNodeTemplate(double width, double height, ShapeNodeShape shape) =>
        new SimpleNode {
            Layout = new RectD(0, 0, width, height),
            Style = DemoStyles.CreateDemoShapeNodeStyle(shape, Themes.PaletteOrange)
        };

    /// <summary>
    ///   Configures interactive editing and the visual hints shown while nodes are moved or dropped.
    /// </summary>
    private void ConfigureInteraction() {
      snapDistanceVisualCreator = new SnapDistanceVisualCreator();
      snapDistanceRenderTreeElement = graphControl.RenderTree.CreateElement(
          graphControl.RenderTree.BackgroundGroup, snapDistanceVisualCreator);

      // Create an input mode for interactive editing.
      var inputMode = new GraphEditorInputMode {
          MovableSelectedItems = GraphItemTypes.Node,
          ShowHandleItems = GraphItemTypes.None
      };

      // Run the alignment layout calculation whenever a new node is created by clicking on free
      // space or by dropping a template from the palette.
      inputMode.NodeCreated += async (_, _) => await AlignNodes();

      // Configure the visual hints displayed during drag-and-drop operations.
      var nodeDropInputMode = inputMode.NodeDropInputMode;
      nodeDropInputMode.DragEntered += (_, _) => InitializeHints(GetNodeCenter(nodeDropInputMode));
      nodeDropInputMode.DragLeft += (_, _) => DisposeHints();
      nodeDropInputMode.DragDropped += (_, _) => DisposeHints();
      nodeDropInputMode.DragOver += (_, _) => UpdateHints(GetNodeCenter(nodeDropInputMode));
      nodeDropInputMode.Enabled = true;

      // Display the same visual hints when moving an existing node.
      var moveInputModes = new[] { inputMode.MoveSelectedItemsInputMode, inputMode.MoveUnselectedItemsInputMode };
      foreach (var moveInputMode in moveInputModes) {
        moveInputMode.DragStarted += (_, _) => {
          InitializeHints(GetNodeCenter(moveInputMode));

          // Force the graph component to render the initial state of the hints visualization.
          graphControl.UpdateVisual();
        };
        moveInputMode.Dragged += (_, _) => UpdateHints(GetNodeCenter(moveInputMode));
        moveInputMode.DragFinished += async (_, _) => {
          DisposeHints();

          // Run the alignment layout calculation after a node has been moved.
          await AlignNodes();
        };
      }

      graphControl.InputMode = inputMode;

      // Only one graph item may be selected at a time so multiple nodes cannot be moved together.
      SingleSelectionHelper.EnableSingleSelection(graphControl);
    }

    /// <summary>
    ///   Starts displaying hints for areas that will result in new node positions after a drop or move.
    /// </summary>
    private void InitializeHints(IPoint nodeCenter) {
      snapDistanceVisualCreator.Initialize(graphControl, nodeCenter, settings);
      snapDistanceRenderTreeElement.Invalidate();
    }

    /// <summary>
    ///   Updates the visual hints for a new node position during drag-and-drop or node movement.
    /// </summary>
    private void UpdateHints(IPoint nodeCenter) {
      if (snapDistanceVisualCreator.UpdateNodeCenter(nodeCenter)) {
        snapDistanceRenderTreeElement.Invalidate();
      }
    }

    /// <summary>
    ///   Stops displaying visual hints after dropping a node or ending a node movement.
    /// </summary>
    private void DisposeHints() {
      snapDistanceVisualCreator.Clear();
      snapDistanceRenderTreeElement.Invalidate();
    }

    /// <summary>
    ///   Determines the node center for updating the hints during a node movement.
    /// </summary>
    private static IPoint GetNodeCenter(MoveInputMode mode) {
      var node = mode.AffectedItems.OfType<INode>().FirstOrDefault();
      return node == null ? PointD.Origin : node.Layout.GetCenter();
    }

    /// <summary>
    ///   Determines the node center for updating the hints during a palette drop.
    /// </summary>
    private static IPoint GetNodeCenter(NodeDropInputMode mode) => mode.PointerPosition;

    /// <summary>
    ///   Runs the alignment layout calculation for the graph.
    /// </summary>
    private async Task AlignNodes() {
      if (layoutRunning) {
        layoutPending = true;
        return;
      }

      layoutRunning = true;
      try {
        do {
          layoutPending = false;
          var algorithm = ConfigureLayout.CreateConfiguredLayoutAlgorithm(settings);
          await new LayoutExecutor(graphControl, algorithm) {
              AnimationDuration = TimeSpan.FromMilliseconds(500),
              AnimateViewport = false
          }.Start();
        } while (layoutPending);
      } finally {
        layoutRunning = false;
      }
    }

    /// <summary>
    ///   Schedules the alignment layout calculation after a setting has changed.
    /// </summary>
    private void ScheduleAlignment() {
      settingsLayoutTimer.Stop();
      settingsLayoutTimer.Start();
    }

    private async void AlignNodesButtonClick(object sender, RoutedEventArgs e) => await AlignNodes();

    [STAThread]
    public static void Main() => Run<NodeAlignmentDemo>();
  }
}
