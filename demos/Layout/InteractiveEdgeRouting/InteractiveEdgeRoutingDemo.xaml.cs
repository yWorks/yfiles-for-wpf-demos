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
using System.Threading.Tasks;
using System.Windows;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.GraphML;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.InteractiveEdgeRouting
{
  /// <summary>
  ///   Demonstrates the <see cref="EdgeRouter"/>'s ability to find and re-route edge paths which are not yet good.
  /// </summary>
  [Demo("interactive-edge-routing", "layout", "Interactive Edge Routing",
      Summary = "After each edit the edge paths are re-routed if necessary.", Tags = ["layout", "edge routing"],
      Keywords = ["layoutfeatures", "interaction", "edge router"], RelatedDemos = [])]
  public sealed partial class InteractiveEdgeRoutingDemo
  {
    private readonly RoutingSettings routingSettings = new();
    private GraphMLSupport graphMLSupport = null!;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public InteractiveEdgeRoutingDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      // enable undo and folding
      var masterGraph = new DefaultGraph { UndoEngineEnabled = true };
      var foldingManager = new FoldingManager(masterGraph);
      ((FoldingEdgeConverter) foldingManager.FoldingEdgeConverter).ReuseMasterPorts = true;
      var foldingView = foldingManager.CreateFoldingView();
      foldingView.EnqueueNavigationalUndoUnits = true;
      GraphControl.Graph = foldingView.Graph;

      // configure interaction
      GraphControl.InputMode = CreateInputMode();

      // configures default styles for newly created graph elements
      DemoStyles.InitDemoStyles(masterGraph, foldingEnabled: true, orthogonalEdgeEditing: true);
      masterGraph.NodeDefaults.ShareStyleInstance = false;
      masterGraph.NodeDefaults.Size = new SizeD(125, 100);

      // bind the demo buttons to their functionality
      graphMLSupport = new GraphMLSupport(GraphControl) {
          GraphMLIOHandler = new GraphMLIOHandler { ClearGraphBeforeRead = true }
      };
      graphMLSupport.RegisterOpenSaveCommandBindings();
      routingScopeComboBox.DataContext = routingSettings;

      LoadSampleGraph();
    }

    /// <summary>
    ///   Loads the sample graph.
    /// </summary>
    private void LoadSampleGraph() {
      BuildGraphFromGraphML("sample.graphml", Graph);
      // when done - fit the bounds
      GraphControl.FitGraphBounds();
      // the sample graph bootstrapping should not be undoable
      Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Configure interaction.
    /// </summary>
    private GraphEditorInputMode CreateInputMode() {
      var mode = new GraphEditorInputMode();
      // disable bend handling: the edge path will be routed
      mode.CreateBendInputMode.Enabled = false;
      mode.ShowHandleItems = GraphItemTypes.Node;
      mode.SelectableItems = GraphItemTypes.Node;
      mode.MarqueeSelectableItems = GraphItemTypes.Node;

      // register listener which trigger a re-routing after each
      mode.MoveSelectedItemsInputMode.DragFinished += async (_, _) => await ReRouteEdges();
      mode.MoveUnselectedItemsInputMode.DragFinished += async (_, _) => await ReRouteEdges();
      mode.HandleInputMode.DragFinished += async (_, _) => await ReRouteEdges();
      mode.CreateEdgeInputMode.EdgeCreated += async (_, _) => await ReRouteEdges();
      mode.NodeCreated += async (_, _) => await ReRouteEdges();
      mode.DeletedSelection += async (_, _) => await ReRouteEdges();
      mode.NavigationInputMode.GroupCollapsed += async (_, _) => await ReRouteEdges();
      mode.NavigationInputMode.GroupExpanded += async (_, _) => await ReRouteEdges();
      mode.ItemsPasted += async (_, _) => await ReRouteEdges();

      // Fix the top right location of a group node when toggling collapse/expand
      mode.NavigationInputMode.AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.TopRight;
      return mode;
    }

    /// <summary>
    ///   Re-Route the edges.
    ///   The EdgeRouter will be used with its routing policy set to either PATH_AS_NEEDED or SEGMENTS_AS_NEEDED.
    ///   That way, the EdgeRouter itself will determine which edges need to be re-routed.
    ///   The different policies are selected with the Routing Scope ComboBox in the tool bar:
    ///   PATH_AS_NEEDED: re-routes the entire path of a "dirty" edge.
    ///   SEGMENTS_AS_NEEDED: re-routes a dirty edge in a way that the
    ///   existing path is kept as much as possible.
    /// </summary>
    private async Task ReRouteEdges() {
      var router = new EdgeRouter();

      // keep existing edge groups
      var data = router.CreateLayoutData(Graph);
      data.SourceGroupIds.MapperFunction = edge =>
          $"s: {edge.GetSourceNode().Layout.GetCenter()} - {edge.SourcePort.GetLocation()}";
      data.TargetGroupIds.MapperFunction = edge =>
          $"t: {edge.GetTargetNode().Layout.GetCenter()} - {edge.TargetPort.GetLocation()}";
      data.Scope.EdgeMapping.MapperFunction = _ => routingSettings.SelectedScope;

      await GraphControl.ApplyLayoutAnimated(router, TimeSpan.FromMilliseconds(500), data);
    }

    private void OnReload(object sender, RoutedEventArgs e) => LoadSampleGraph();

    [STAThread]
    private static void Main() => Run<InteractiveEdgeRoutingDemo>();
  }
}
