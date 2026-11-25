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
using System.Windows.Threading;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout.Organic;

namespace yWorks.Demo.InteractiveOrganic
{
  /// <summary>
  ///   Demonstrates interactive layout updates with <see cref="InteractiveOrganicLayout"/>.
  /// </summary>
  [Demo("interactive-organic-layout", "layout", "Interactive Organic Layout",
      Summary = "Shows the 'interactive organic' layout algorithm.", Tags = ["layout", "interactive layout"],
      Keywords = ["layoutfeatures", "organic", "move", "worker"])]
  public sealed partial class InteractiveOrganicDemo
  {
    private InteractiveOrganicLayoutHelper interactionHelper = null!;

    /// <summary>
    ///   Wires up the UI components.
    /// </summary>
    public InteractiveOrganicDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    ///   Calls <see cref="CreateEditorMode"/> and registers
    ///   the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    private void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    ///   Creates the default input mode for the GraphControl,
    ///   a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    private IInputMode CreateEditorMode() {
      // create default interaction with a number of disabled input modes.
      var mode = new GraphEditorInputMode {
          // no bend creation
          CreateBendInputMode = { Enabled = false },
          // only nodes can be selected
          SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge,
          // only nodes may be marquee selected
          MarqueeSelectableItems = GraphItemTypes.Node,
          // only nodes and edges can be selected via clicks
          ClickSelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge,
          // clicks will be reported for nodes, only
          ClickableItems = GraphItemTypes.Node,
          // handles won't be shown for any items
          ShowHandleItems = GraphItemTypes.None,
          // when creating new edges, bends and self-loops are not allowed
          CreateEdgeInputMode = {
              AllowCreateBend = false,
              AllowSelfLoops = false
          }
      };

      // prepare the move selected items input mode for interacting with the layout algorithm
      InitMoveMode(mode.MoveSelectedItemsInputMode);
      InitMoveMode(mode.MoveUnselectedItemsInputMode);

      // We could also allow direct moving of nodes, without requiring selection of the nodes, first.
      // However by default this conflicts with the edge creation gesture, which we will simply disable, here.
      // uncomment the following lines to be able to move nodes without selecting them first
      //
      // var unselected = mode.CreateMoveUnselectedInputMode(EventRecognizers.Create(EventRecognizers.Always));
      // unselected.Priority = mode.MoveInputMode.Priority + 1;
      // mode.Add(unselected);
      // mode.CreateEdgeInputMode.Enabled = false;
      // InitMoveMode(unselected);

      return mode;
    }

    /// <summary>
    ///   Registers the listeners to the given move input mode in order to tell the organic layout what
    ///   nodes are moved interactively.
    /// </summary>
    /// <param name="moveInputMode">The input mode that should be observed</param>
    private void InitMoveMode(MoveInputMode moveInputMode) {
      // register callbacks to notify the organic layout of changes
      moveInputMode.DragStarted += OnMoveInitialized;
      moveInputMode.DragCanceled += OnMoveCanceled;
      moveInputMode.Dragged += OnMoving;
      moveInputMode.DragFinished += OnMovedFinished;
    }

    /// <summary>
    ///   Called once the move operation has been initialized
    /// </summary>
    /// <remarks>
    ///   Calculates which components stay fixed and which nodes will be moved by the user.
    /// </remarks>
    private void OnMoveInitialized(object? sender, EventArgs eventArgs) {
      movedNodes.Clear();
      movedNodes.AddRange(((MoveInputMode) sender!).AffectedItems.OfType<INode>());
      var connectedComponents = new ConnectedComponents().Run(Graph);
      var movedComponents = new HashSet<int>();
      var selectedNodes = new HashSet<INode>(movedNodes);
      foreach (var node in movedNodes) {
        // remember that we are moving this component
        movedComponents.Add(connectedComponents.NodeComponentIds[node]);
        interactionHelper.FixNode(node);
      }

      // there are components that won't be moved - nail the nodes down so that they don't spread apart infinitely
      foreach (var node in Graph.Nodes) {
        if (!movedComponents.Contains(connectedComponents.NodeComponentIds[node])) {
          interactionHelper.SetInertia(node, 1);
        } else if (!selectedNodes.Contains(node)) {
          // make it float freely
          interactionHelper.SetInertia(node, 0);
        }
      }

      // Notify the layout algorithm that there is new work to do...
      interactionHelper.WakeUp();
    }

    /// <summary>
    ///   Notifies the layout of the new positions of the interactively moved nodes.
    /// </summary>
    private void OnMoving(object? sender, InputModeEventArgs inputModeEventArgs) {
      foreach (var node in movedNodes) {
        interactionHelper.MoveNode(node);
      }
      // Notify the layout algorithm that there is new work to do...
      interactionHelper.WakeUp();
    }

    /// <summary>
    ///   Resets the state in the layout when the user cancels the move operation.
    /// </summary>
    private void OnMoveCanceled(object? sender, InputModeEventArgs inputModeEventArgs) {
      foreach (var node in graphControl.Graph.Nodes) {
        interactionHelper.FixNode(node, 1, 0);
      }
      // We don't want to restart the layout (since we canceled the drag anyway...)
    }


    /// <summary>
    ///   Called once the interactive move is finished.
    /// </summary>
    /// <remarks>
    ///   Updates the state of the interactive layout.
    /// </remarks>
    private void OnMovedFinished(object? sender, InputModeEventArgs inputModeEventArgs) {
      // we make all nodes freely movable
      foreach (var node in graphControl.Graph.Nodes) {
        interactionHelper.FixNode(node, 1, 0);
      }
    }

    /// <summary>
    ///   Initializes the graph instance setting default styles
    ///   and creating a small sample graph.
    /// </summary>
    private void InitializeGraph() {
      var graph = Graph;

      // initialize default styles
      DemoStyles.InitDemoStyles(graph);
      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: false);

      // load a sample graph
      BuildGraphFromGraphML("sample.graphml", graph);

      // we start with a simple run of OrganicLayout to get a good starting result
      // the algorithm is optimized to "unfold" graphs quicker than
      // interactive organic, so we use this result as a starting solution
      var initialLayout = new OrganicLayout { DefaultMinimumNodeDistance = 50 };
      graph.ApplyLayout(initialLayout);

      // center the initial graph
      GraphControl.FitGraphBounds();

      interactionHelper = new InteractiveOrganicLayoutHelper(graphControl, graph);

      // create and start the layout algorithm
      interactionHelper.StartLayout();
      interactionHelper.StartAnimator();

      WakeUp();

      // register a listener so that structure updates are handled automatically
      graph.NodeCreated += (_, args) => {
        interactionHelper.AddNode(args.Item);
        interactionHelper.Synchronize();
      };
      graph.NodeRemoved += OnStructureChanged;
      graph.EdgeCreated += (_, args) => {
        interactionHelper.IncreaseStress(args.Item.GetSourceNode());
        interactionHelper.SetInertia(args.Item.GetSourceNode(), 0.1);
        interactionHelper.IncreaseStress(args.Item.GetTargetNode());
        interactionHelper.SetInertia(args.Item.GetTargetNode(), 0.1);
        interactionHelper.Synchronize();
      };
      graph.EdgeRemoved += OnStructureChanged;
    }

    /// <summary>
    ///   If the graph structure has been changed interactively, synchronize the structure with the layout algorithm.
    /// </summary>
    private void OnStructureChanged(object? source, EventArgs eventArgs) {
      interactionHelper.Synchronize();
    }

    #region Properties

    /// <summary>
    ///   Returns the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl => graphControl;

    /// <summary>
    ///   Gets the currently registered <see cref="IGraph"/> instance from the GraphControl.
    /// </summary>
    public IGraph Graph => GraphControl.Graph;

    private readonly List<INode> movedNodes = [];

    /// <summary>
    ///   Wakes the layout algorithm up to calculate an initial layout.
    /// </summary>
    private void WakeUp() {
      // we make all nodes freely movable
      foreach (var node in graphControl.Graph.Nodes) {
        interactionHelper.SetInertia(node, 0);
      }
      // then wake up the layout
      interactionHelper.WakeUp();
      // and after two second we freeze the nodes again...
      DispatcherTimer timer = null!;
      timer = new DispatcherTimer(TimeSpan.FromSeconds(2), DispatcherPriority.Input, (_, _) => {
        // we make all nodes freely movable
        foreach (var node in graphControl.Graph.Nodes) {
          interactionHelper.SetInertia(node, 1);
        }
        timer.Stop();
      }, graphControl.Dispatcher);

      timer.Start();
    }

    [STAThread]
    private static void Main() => Run<InteractiveOrganicDemo>();

    #endregion
  }
}
