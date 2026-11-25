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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.MazeRouting
{
  /// <summary>
  ///   Shows how the <see cref="EdgeRouter"/> can be used for finding routes through a maze.
  /// </summary>
  [Demo("maze-routing", "layout", "Maze Routing",
      Summary = "Shows how the automatic edge routing finds routes through a maze.",
      Tags = ["layout", "edge routing", "polyline"],
      Keywords = ["layoutfeatures", "edge router", "polyline router", "background", "layout", "data analysis"],
      RelatedDemos = [])]
  public sealed partial class MazeRoutingDemo
  {
    private readonly Settings settings = new();

    /// <summary>
    ///   Holds whether a layout is currently running.
    /// </summary>
    private bool inLayout;

    /// <summary>
    ///   Holds the filtered graph, which contains the maze nodes and normal graph nodes. Maze nodes
    ///   are visible only during layout to simulate the maze.
    /// </summary>
    private FilteredGraphWrapper? filteredGraph;

    private GraphEditorInputMode inputMode = null!;

    public MazeRoutingDemo() {
      InitializeComponent();
      SettingsPanel.DataContext = settings;
    }

    private IGraph Graph => graphControl.Graph;

    protected override async void Initialize() {
      // Set some default styles.
      DemoStyles.InitDemoStyles(Graph, Themes.Palette31, edgeTheme: Themes.Palette31);

      // Create the input mode.
      CreateEditorInputMode();

      // Create the sample graph.
      CreateSampleGraph(Graph);

      // Create the visualization for the edge path routing obstacles.
      await CreateMazeVisual();

      // Route edges.
      await RouteEdges();

      // Initialize undo and redo for the demo's graph.
      InitializeUndoEngine(Graph);
    }

    /// <summary>
    ///   Initializes the input mode.
    /// </summary>
    private void CreateEditorInputMode() {
      inputMode = new GraphEditorInputMode();

      // Route the newly created edge.
      inputMode.CreateEdgeInputMode.EdgeCreated += async (_, evt) => await RouteEdges([evt.Item]);

      // Add the listeners to route the edges affected by a node movement or node resize.
      inputMode.MoveSelectedItemsInputMode.DragFinished += async (_, _) =>
          await OnDragFinished(inputMode.MoveSelectedItemsInputMode.AffectedItems);
      inputMode.MoveUnselectedItemsInputMode.DragFinished += async (_, _) =>
          await OnDragFinished(inputMode.MoveUnselectedItemsInputMode.AffectedItems);
      inputMode.HandleInputMode.DragFinished += async (_, _) =>
          await OnDragFinished(inputMode.HandleInputMode.AffectedItems);

      graphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Routes the edges affected by a node movement or resize.
    /// </summary>
    private async Task OnDragFinished(IEnumerable<IModelItem> affectedItems) {
      var affectedEdges = affectedItems.OfType<IEdge>().ToArray();
      // Route the affected edges.
      await RouteEdges(affectedEdges);
    }

    /// <summary>
    ///   Routes only the affected edges.
    /// </summary>
    /// <param name="affectedEdges">The array of edges to be routed.</param>
    /// <returns>A task that completes after the layout is applied without errors.</returns>
    private async Task RouteEdges(IEnumerable<IEdge>? affectedEdges = null) {
      // Prevent starting another layout calculation.
      inLayout = true;
      SetUIDisabled(true);

      try {
        var graph = filteredGraph?.WrappedGraph ?? Graph;

        // Configure the routing style and the edge distances.
        var router = new EdgeRouter {
            DefaultEdgeDescriptor = {
                RoutingStyle = settings.SelectedRoutingStyle,
                MinimumEdgeDistance = settings.EdgeDistance
            }
        };

        // Configure the edge ports allowed to be used by the edge router.
        var edgeRouterData = router.CreateLayoutData(graph);
        var allowedPorts = GetAllowedPorts();
        if (allowedPorts != null) {
          edgeRouterData.Ports.SourcePortCandidates.Constant = allowedPorts;
          edgeRouterData.Ports.TargetPortCandidates.Constant = allowedPorts;
        }

        // If there are edges marked as affected as a result of edge creation, node movement, or
        // node resize, route only them.
        var affectedEdgeArray = affectedEdges?.ToArray();
        if (affectedEdgeArray is { Length: > 0 }) {
          edgeRouterData.Scope.Edges.Source = affectedEdgeArray;
        } else if (settings.Scope == RoutingScope.RouteEdgesAtSelectedNodes) {
          edgeRouterData.Scope.IncidentNodes.Items = graphControl.Selection.Nodes;
        } else if (settings.Scope == RoutingScope.RouteSelectedEdges) {
          edgeRouterData.Scope.Edges.Items = graphControl.Selection.Edges;
        }

        // Apply the layout.
        var layoutExecutor = new LayoutExecutor(graphControl, graph, router) {
            LayoutData = edgeRouterData,
            AnimationDuration = TimeSpan.FromMilliseconds(500)
        };
        await layoutExecutor.Start();
      } catch {
        // Keep the demo responsive if the layout cannot be calculated.
      } finally {
        inLayout = false;
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Checks whether the <see cref="EdgeRouter"/> should use specific ports,
    ///   and creates <see cref="EdgePortCandidates"/> for them.
    /// </summary>
    private EdgePortCandidates? GetAllowedPorts() =>
        settings.PortSides switch {
            PortSideSelection.LeftRight => new EdgePortCandidates().AddFreeCandidate(PortSides.Left)
                .AddFreeCandidate(PortSides.Right),
            PortSideSelection.TopBottom => new EdgePortCandidates().AddFreeCandidate(PortSides.Top)
                .AddFreeCandidate(PortSides.Bottom),
            _ => null
        };

    /// <summary>
    ///   Enables or disables the UI elements.
    /// </summary>
    /// <param name="disabled">True if the elements should be disabled, false otherwise.</param>
    private void SetUIDisabled(bool disabled) {
      settings.IsRouting = disabled;
      // Enable or disable input so that no user interactions occur on the GraphControl when a layout is running.
      inputMode.WaitInputMode.Waiting = disabled;
    }

    /// <summary>
    ///   Enables and configures undo and redo for the given graph.
    /// </summary>
    private static void InitializeUndoEngine(IGraph graph) {
      // Enable the undo engine and merge undo units that occur within a specific time window
      // (e.g., 2 seconds).
      graph.SetUndoEngineEnabled(true);
      var undoEngine = graph.GetUndoEngine();
      if (undoEngine != null) {
        undoEngine.MergeUnits = true;
      }
    }

    /// <summary>
    ///   Creates the sample graph. The graph consists of maze graph nodes, which form the maze,
    ///   and normal graph nodes. The maze nodes are only live during layout calculations to serve
    ///   as obstacles for edge path routing (thereby simulating the maze).
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      // Build the graph from the sample data and apply the style of the maze obstacles.
      BuildGraphFromJson("maze.json", graph, typeof(MazeNodeTag));
      var mazeNodeStyle = new ShapeNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0x24, 0x22, 0x65)),
          Pen = null
      };
      foreach (var node in graph.Nodes) {
        if (node.Tag is MazeNodeTag { Maze: true }) {
          graph.SetStyle(node, mazeNodeStyle);
        }
      }
    }

    /// <summary>
    ///   Creates the visualization for the obstacles that affect edge path routing.
    /// </summary>
    private async Task CreateMazeVisual() {
      var graph = Graph;

      // Determine the nodes that model the obstacles.
      var mazeNodes = graph.Nodes.Where(node => node.Tag is MazeNodeTag { Maze: true }).ToArray();
      // Add the maze visualization for the obstacle nodes.
      foreach (var node in mazeNodes) {
        var renderTree = graphControl.RenderTree;
        renderTree.CreateElement(renderTree.BackgroundGroup, node, GraphModelManager.DefaultNodeRenderer);
      }

      // Center the graph in the visible area.
      await graphControl.FitGraphBounds();

      // Route the edges according to the configured EdgeRouter.
      await RouteEdges();

      // Hide the obstacle nodes from the current view to prevent users from interacting with them.
      filteredGraph = new FilteredGraphWrapper(graphControl.Graph,
          node => inLayout || node.Tag is not MazeNodeTag { Maze: true });
      graphControl.Graph = filteredGraph;
    }

    private void RouteEdgesButtonClick(object sender, RoutedEventArgs e) => _ = RouteEdges();

    [STAThread]
    private static void Main() => Run<MazeRoutingDemo>();
  }

  public sealed record MazeNodeTag(bool Maze);
}
