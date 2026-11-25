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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomSnapping
{
  /// <summary>
  ///   Shows how the snapping feature can be customized.
  /// </summary>
  [Demo("custom-snapping", "input", "Custom Snapping", Summary = "Shows how the snapping feature can be customized.",
      Tags = ["interaction", "snapping"], Keywords = ["guides", "lines", "labels", "move"])]
  public partial class CustomSnappingDemo
  {
    /// <summary>
    ///   Returns a list of the free <see cref="AdditionalSnapLineVisualCreator"/>s used in this demo.
    ///   This property is used by <see cref="AdditionalSnapLineMoveInputMode"/> to access the
    ///   <see cref="AdditionalSnapLineVisualCreator"/>s used in this demo.
    /// </summary>
    private readonly List<AdditionalSnapLineVisualCreator> additionalSnapLineVisualCreators = [];

    public CustomSnappingDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;

      DecorateModelItemLookupForCustomSnappingBehaviour(graph);

      var graphSnapContext = CreateGraphSnapContext();

      InitializeGrid(graphSnapContext);

      // initialize two free snap lines that are also visualized in the demo's GraphControl
      AddAdditionalSnapLineVisualCreator(new PointD(0, -70), new PointD(500, -70));
      AddAdditionalSnapLineVisualCreator(new PointD(-230, -50), new PointD(-230, 400));

      // initialize the input mode for this demo
      var graphEditorInputMode = new GraphEditorInputMode { SnapContext = graphSnapContext };

      // add an input mode that allows to move the additional snap lines
      var additionalSnapLineMoveInputMode =
          new AdditionalSnapLineMoveInputMode(additionalSnapLineVisualCreators, graphControl);
      // ensure the new mode is the first to process mouse events
      additionalSnapLineMoveInputMode.Priority = -50;
      graphEditorInputMode.Add(additionalSnapLineMoveInputMode);

      graphControl.InputMode = graphEditorInputMode;

      graph.SetUndoEngineEnabled(true);

      InitializeGraphDefaults(graph);

      // create a sample graph for the demo
      CreateSampleGraph(graph);
      // center the sample graph in the demo's GraphControl
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a pre-configured <see cref="GraphSnapContext"/>.
    /// </summary>
    private GraphSnapContext CreateGraphSnapContext() {
      var context = new GraphSnapContext {
          SnappableItems = SnappableItems.All & ~(SnappableItems.Bend | SnappableItems.OrthogonalMovement),
          GridSnapType = GridSnapTypes.Lines,
          SnapDistance = 10
      };
      // use the free additional snap lines
      context.CollectSnapReferences += (_, args) => {
        // Creates and adds snap lines for the free AdditionalSnapLineVisualCreator to a GraphSnapContext.
        // While the AdditionalSnapLineVisualCreators are used to visualize free snap lines, corresponding
        // OrthogonalSnapLines have to be added to the snapping mechanism to provide the snapping behavior.
        foreach (var creator in additionalSnapLineVisualCreators) {
          foreach (var snapLine in creator.CreateSnapLines()) {
            args.AddSnapReference(snapLine);
          }
        }
      };
      return context;
    }

    /// <summary>
    ///   Registers the demo's custom snap line providers for nodes and edges.
    /// </summary>
    /// <param name="graph">The demo's graph</param>
    private static void DecorateModelItemLookupForCustomSnappingBehaviour(IGraph graph) {
      // add additional snap lines for orthogonal labels of nodes
      var decorator = graph.GetDecorator();
      decorator.Nodes.SnapReferenceProvider.AddWrapperFactory((node, wrappedProvider) =>
          new OrthogonalLabelSnapLineProviderWrapper(node, wrappedProvider!));

      // add additional snap lines for orthogonal labels of edges
      decorator.Edges.SnapReferenceProvider.AddWrapperFactory((edge, wrappedProvider) =>
          new OrthogonalLabelSnapLineProviderWrapper(edge, wrappedProvider!));

      // for nodes using ShapeNodeStyle use a customized grid snapping behavior based on their shape
      decorator.Nodes.SnapResultProvider.AddConstant(new ShapeBasedGridNodeSnapResultProvider());
    }

    /// <summary>
    ///   Configures grid visualization and grid snapping for the given graph control.
    /// </summary>
    /// <param name="graphSnapContext">The snap context to set up for grid snapping</param>
    private void InitializeGrid(GraphSnapContext graphSnapContext) {
      var gridInfo = new GridInfo {
          HorizontalSpacing = 200,
          VerticalSpacing = 200
      };

      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, gridInfo, new GridRenderer());

      graphControl.Invalidate();
      graphControl.ZoomChanged += (_, _) => graphControl.Invalidate();
      graphControl.ViewportChanged += (_, _) => graphControl.Invalidate();

      graphSnapContext.NodeGridConstraintProvider = new GridConstraintProvider(gridInfo);
      graphSnapContext.BendGridConstraintProvider = new GridConstraintProvider(gridInfo);
    }

    /// <summary>
    ///   Configures default styles for the given graph.
    /// </summary>
    /// <param name="graph">The graph for which default styles are configured</param>
    private static void InitializeGraphDefaults(IGraph graph) {
      var labelStyle = new LabelStyle();
      graph.NodeDefaults.Labels.Style = labelStyle;
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Instance.CreateParameter(new PointD(0.5, 0.0),
          new PointD(0, -10), new PointD(0.5, 1.0), PointD.Origin);

      graph.EdgeDefaults.Labels.Style = labelStyle;
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0);

      DemoStyles.InitDemoStyles(graph);
      graph.NodeDefaults.Size = new SizeD(50, 50);
    }

    /// <summary>
    ///   Adds a new <see cref="AdditionalSnapLineVisualCreator"/> to the given graph control that spans between
    ///   <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The start location of the snap line.</param>
    /// <param name="to">The end location of the snap line.</param>
    private void AddAdditionalSnapLineVisualCreator(PointD from, PointD to) {
      var lineVisualCreator = new AdditionalSnapLineVisualCreator(from, to);
      additionalSnapLineVisualCreators.Add(lineVisualCreator);
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, lineVisualCreator);
    }

    /// <summary>
    ///   Creates the demo's sample graph.
    /// </summary>
    /// <param name="graph">The graph to populate</param>
    private static void CreateSampleGraph(IGraph graph) {
      graph.Clear();

      var starShape = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Star5,
          Brush = new SolidColorBrush(Color.FromRgb(0x46, 0xA8, 0xD5)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x22, 0x45, 0x56)), 1)
      };

      graph.CreateNode(new RectD(-100, 260, 75, 75), starShape);
      graph.CreateNode(new RectD(40, 260, 75, 75), starShape);

      var n1 = graph.CreateNode(new RectD(-80, 60, 50, 50));
      graph.AddLabel(n1,
          "This node label has an extra extended super long text\nso you can see how nodes snap all along its border.");
      var n2 = graph.CreateNode(new RectD(360, 280, 50, 50));
      graph.CreateNode(new RectD(50, -90, 50, 50));

      var edge = graph.CreateEdge(n1, n2);
      graph.AddBend(edge, new PointD(130, 85), 0);
      graph.AddBend(edge, new PointD(130, 210), 1);
      graph.AddBend(edge, new PointD(280, 210), 2);
      graph.AddBend(edge, new PointD(320, 305), 3);
      var smartEdgeLabelModel = new SmartEdgeLabelModel();
      graph.AddLabel(edge, "Other graph items can snap\nto this edge label as long as\nit is orthogonal.",
          smartEdgeLabelModel.CreateParameterFromSource(2, 10));
    }

    [STAThread]
    public static void Main() {
      Run<CustomSnappingDemo>();
    }
  }
}
