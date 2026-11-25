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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.CustomLayoutStage
{
  /// <summary>
  ///   Demonstrates how custom layout stages can be used to solve layout problems.
  /// </summary>
  [Demo("custom-layout-stage", "layout", "Custom Layout Stage",
      Summary =
          "Custom layout stages can be used to solve unique layout problems that are not adequately covered by existing layout algorithms.",
      Tags = ["layout", "layout stage"], Keywords = [
          "layoutfeatures", "ILayoutStage", "LayoutStageBase", "CoreLayout", "IDataProvider", "data provider",
          "LayoutGraph", "LayoutGraphHider", "dummy edges", "temporary elements", "temporary edges",
          "layout post-processing", "DataProviderBase", "GenericLayoutData", "LayoutData"
      ], RelatedDemos = [])]
  public sealed partial class CustomLayoutStageDemo
  {
    /// <summary>
    ///   The graph component in which the graph is displayed.
    /// </summary>
    private GraphControl GraphComponent => graphControl;

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      GraphComponent.InputMode = new GraphEditorInputMode();

      // Create the sample graph
      CreateGraph(GraphComponent.Graph);
      GraphComponent.FitGraphBounds();

      // Finally, enable the undo engine. This prevents undoing of the graph creation
      GraphComponent.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Moves the nodes marked “aside” to the side of the graph.
    /// </summary>
    private async Task RunMoveAsideLayoutAsync() {
      // Create the custom stage including core layout
      var layout = new MoveNodesAsideStage(CreateCoreLayout());

      // Execute the layout
      await GraphComponent.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(0.5), CreateHierarchicalLayoutData());
    }

    /// <summary>
    ///   Aligns the green nodes vertically in the graph.
    /// </summary>
    private async Task RunAlignNodesLayoutAsync() {
      // Create the custom stage including core layout
      var layout = new AlignmentStage(CreateCoreLayout());

      // create the layout data for the custom alignment stage
      LayoutData<INode, IEdge, ILabel, ILabel> layoutData = CreateAlignmentStageLayoutData();
      // combine this with the layoutData of the hierarchical layout
      layoutData = layoutData.CombineWith(CreateHierarchicalLayoutData());

      // Execute the layout
      await GraphComponent.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(0.5), layoutData);
    }

    /// <summary>
    ///   Changes edge paths to a zig-zag shape.
    /// </summary>
    private async Task RunZigZagLayoutAsync() {
      // Create the custom stage including core layout
      var layout = new ZigZagEdgesStage(CreateCoreLayout());

      // Execute the layout
      await GraphComponent.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(0.5), CreateHierarchicalLayoutData());
    }

    /// <summary>
    ///   Applies all custom layout stages at once. Typically, layout stages can be chained like this to
    ///   divide responsibilities for different parts of the layout customization into separate stages.
    /// </summary>
    private async Task RunAllLayoutsAsync() {
      // In this case, however, the custom alignment stage must come last, since it explicitly checks
      // for the core layout to be a hierarchical layout. In many cases, the order of stages matters, as
      // the graph is changed for the next stage, but some stages with changes that don't interfere with
      // each other can also be applied in any order.
      var layout = new ZigZagEdgesStage(new MoveNodesAsideStage(new AlignmentStage(CreateCoreLayout())));

      // create the layout data for the custom alignment stage
      LayoutData<INode, IEdge, ILabel, ILabel> layoutData = CreateAlignmentStageLayoutData();
      // combine this with the layoutData of the hierarchical layout
      layoutData = layoutData.CombineWith(CreateHierarchicalLayoutData());

      await GraphComponent.ApplyLayoutAnimated(layout, TimeSpan.FromSeconds(0.5), layoutData);
    }

    private async void RunMoveAsideLayout(object sender, RoutedEventArgs e) =>
        await RunWithDisabledButtons(RunMoveAsideLayoutAsync);

    private async void RunAlignNodesLayout(object sender, RoutedEventArgs e) =>
        await RunWithDisabledButtons(RunAlignNodesLayoutAsync);

    private async void RunZigZagLayout(object sender, RoutedEventArgs e) =>
        await RunWithDisabledButtons(RunZigZagLayoutAsync);

    private async void RunAllLayouts(object sender, RoutedEventArgs e) =>
        await RunWithDisabledButtons(RunAllLayoutsAsync);

    private async Task RunWithDisabledButtons(Func<Task> runLayout) {
      runStage1Button.IsEnabled = false;
      runStage2Button.IsEnabled = false;
      runStage3Button.IsEnabled = false;
      runAllStagesButton.IsEnabled = false;
      try {
        await runLayout();
      } finally {
        runStage1Button.IsEnabled = true;
        runStage2Button.IsEnabled = true;
        runStage3Button.IsEnabled = true;
        runAllStagesButton.IsEnabled = true;
      }
    }

    /// <summary>
    ///   Creates custom layout data for the alignment stage.
    /// </summary>
    private GenericLayoutData<INode, IEdge, ILabel, ILabel> CreateAlignmentStageLayoutData() {
      // GenericLayoutData offers a way to attach custom properties to graph items for
      // layout stages in a similar manner as the yFiles built-in layout algorithms
      var layoutData = GraphComponent.Graph.CreateGenericLayoutData();
      layoutData.AddItemCollection(AlignmentStage.AlignedNodesDataKey).Predicate = node =>
          node.Style is ShapeNodeStyle style && style.Shape == ShapeNodeShape.Ellipse;
      return layoutData;
    }

    /// <summary>
    ///   Creates the layout data for the hierarchical layout algorithm. Sequence and layering
    ///   constraints are used to constrain the placement of the green nodes for this sample.
    /// </summary>
    private HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> CreateHierarchicalLayoutData() {
      var layout = new HierarchicalLayout();
      var layoutData = layout.CreateLayoutData(GraphComponent.Graph);
      foreach (var edge in GraphComponent.Graph.Edges) {
        if (edge.Tag is HorizontalEdgeTag { Horizontal: true }) {
          layoutData.LayerConstraints.PlaceInSameLayer(edge.GetSourceNode(), edge.GetTargetNode());
          layoutData.SequenceConstraints.PlaceAtHead(edge.GetTargetNode());
        }
      }
      return layoutData;
    }

    /// <summary>
    ///   Creates the core layout for all the other layouts: A simple hierarchical layout with orthogonal
    ///   edges and a slightly increased layer distance.
    /// </summary>
    private static HierarchicalLayout CreateCoreLayout() => new() { MinimumLayerDistance = 50 };

    /// <summary>
    ///   Creates an initial sample graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private void CreateGraph(IGraph graph) {
      graph.NodeDefaults.Size = new SizeD(40, 40);
      DemoStyles.InitDemoStyles(graph, nodeLabelTheme: Themes.Palette21, shape: ShapeNodeShape.RoundRectangle);
      graph.EdgeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Labels.Style = DemoStyles.CreateDemoNodeLabelStyle(Themes.Palette21);

      var nodeLocations = new PointD[] {
          new(150, 0),
          new(100, 50),
          new(200, 50),
          new(75, 100),
          new(125, 100),
          new(175, 100),
          new(225, 100),
          new(75, 150),
          new(225, 150),
          new(100, 0),
          new(50, 50),
          new(25, 100)
      };
      var nodes = new INode[nodeLocations.Length];
      for (var i = 0; i < nodeLocations.Length; i++) {
        var location = nodeLocations[i];
        nodes[i] = graph.CreateNode(new RectD(location.X, location.Y, 40, 40));
      }

      // We have a few special nodes in this sample
      var greenNodeStyle = DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse, Themes.Palette25);
      graph.SetStyle(nodes[9], greenNodeStyle);
      graph.SetStyle(nodes[10], greenNodeStyle);
      graph.SetStyle(nodes[11], greenNodeStyle);

      var blueNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.Palette21);
      graph.SetStyle(nodes[7], blueNodeStyle);
      graph.SetStyle(nodes[8], blueNodeStyle);
      nodes[7].Tag = nodes[8].Tag = new MoveAsideTag(true);
      graph.AddLabel(nodes[7], "Aside");
      graph.AddLabel(nodes[8], "Aside");

      graph.CreateEdge(nodes[0], nodes[1]);
      graph.CreateEdge(nodes[0], nodes[2]);
      graph.CreateEdge(nodes[1], nodes[3]);
      graph.CreateEdge(nodes[1], nodes[4]);
      graph.CreateEdge(nodes[2], nodes[5]);
      graph.CreateEdge(nodes[2], nodes[6]);
      graph.CreateEdge(nodes[3], nodes[7]);
      graph.CreateEdge(nodes[6], nodes[8]);
      var dashedEdgeStyle = new PolylineEdgeStyle {
          Pen = new Pen(Brushes.DarkGray, 1.5) { DashStyle = DashStyles.Dash }
      };
      var e1 = graph.CreateEdge(nodes[0], nodes[9], dashedEdgeStyle);
      var e2 = graph.CreateEdge(nodes[1], nodes[10], dashedEdgeStyle);
      var e3 = graph.CreateEdge(nodes[3], nodes[11], dashedEdgeStyle);
      e1.Tag = e2.Tag = e3.Tag = new HorizontalEdgeTag(true);

      graph.ApplyLayout(CreateCoreLayout(), CreateHierarchicalLayoutData());
    }

    public CustomLayoutStageDemo() {
      InitializeComponent();
    }

    [STAThread]
    private static void Main() => Run<CustomLayoutStageDemo>();
  }

  internal sealed record MoveAsideTag(bool MoveAside);

  internal sealed record HorizontalEdgeTag(bool Horizontal);
}
