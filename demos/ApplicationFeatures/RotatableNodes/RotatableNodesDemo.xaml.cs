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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Radial;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;

namespace yWorks.Demo.RotatableNodes
{
  /// <summary>
  ///   Demo code that shows how support for rotated node visualizations can be implemented on top of the yFiles library.
  ///   A custom <see cref="INodeStyle"/> implementation is used to encapsulate most of the added functionality.
  /// </summary>
  [Demo("rotatable-nodes", "application-features", "Rotatable Nodes",
      Summary = "Shows nodes that can be rotated with the mouse.", Tags = ["style", "layout", "interaction"],
      Keywords = [
          "handles", "input", "rotation", "labels", "ports", "hierarchical", "organic", "orthogonal", "circular",
          "tree", "radial tree", "balloon", "radial", "edge router", "polyline router", "organic router", "curves"
      ])]
  public partial class RotatableNodesDemo
  {
    [STAThread]
    public static void Main() {
      Run<RotatableNodesDemo>();
    }

    private GraphMLSupport graphMLSupport;

    #region Initialization

    public RotatableNodesDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      InitializeInputMode();
      InitializeGraphML();
      InitializeGraph();
      LoadGraph("sine");
    }

    /// <summary>
    ///   Initializes the interaction with the graph.
    /// </summary>
    private void InitializeInputMode() {
      var graphEditorInputMode = new GraphEditorInputMode {
          SnapContext = new GraphSnapContext {
              Enabled = true,
              SnappableItems = SnappableItems.Node,
              CollectNodePairSegmentSnapLines = false,
              CollectNodePairSnapLines = false,
              CollectEdgeSnapReferences = false,
              CollectNodeSnapReferences = false,
              CollectPortSnapReferences = false,
              CollectNodeSizes = false
          }
      };
      graphControl.InputMode = graphEditorInputMode;

      var handleInputMode = graphEditorInputMode.HandleInputMode;
      handleInputMode.Dragged += (sender, args) => {
        if (args.Context.InputMode is not HandleInputMode handleInputMode) {
          return;
        }
        if (handleInputMode.CurrentHandle is not NodeRotateHandle) {
          return;
        }
        var node = handleInputMode.AffectedItems.FirstOrDefault(item => item is INode);
        if (node is INode {
                Style: RotatableNodeStyleDecorator rotatableNodeStyleDecorator, Labels: { Count: 1 }
            } rotatedNode && rotatedNode.Labels.First()!.Text.EndsWith("°")) {
          args.Context.GetGraph()!.SetLabelText(rotatedNode.Labels.First()!,
              $"{rotatableNodeStyleDecorator.Angle:F0}°");
        }
      };
    }

    /// <summary>
    ///   Initialize loading from and saving to graphml-files.
    /// </summary>
    private void InitializeGraphML() {
      // initialize (de-)serialization for load/save commands
      graphMLSupport = new GraphMLSupport(graphControl);
      graphMLSupport.RegisterOpenSaveCommandBindings();
      graphMLSupport.GraphMLIOHandler.Parsed += (sender, e) => {
        // after loading apply wrap node styles, node label models and port location models in rotatable decorators
        var graph = graphControl.Graph;
        foreach (var node in graph.Nodes.Where(node => !graph.IsGroupNode(node))) {
          if (!(node.Style is RotatableNodeStyleDecorator)) {
            graph.SetStyle(node, new RotatableNodeStyleDecorator(node.Style));
          }
          foreach (var label in node.Labels.Where(label =>
              !(label.LayoutParameter.Model is RotatableNodeLabelModelDecorator))) {
            graph.SetLabelLayoutParameter(label,
                new RotatableNodeLabelModelDecorator(label.LayoutParameter.Model).CreateWrappingParameter(
                    label.LayoutParameter));
          }
          foreach (var port in node.Ports.Where(port =>
              !(port.LocationParameter.Model is RotatablePortLocationModelDecorator))) {
            graph.SetPortLocationParameter(port,
                RotatablePortLocationModelDecorator.Instance.CreateWrappingParameter(port.LocationParameter));
          }
        }
      };
    }

    /// <summary>
    ///   Initializes styles and decorators for the graph.
    /// </summary>
    private void InitializeGraph() {
      var foldingManager = new FoldingManager();
      var graph = foldingManager.CreateFoldingView().Graph;

      var decorator = graph.GetDecorator();

      // For rotated nodes, need to provide port candidates that are backed by a rotatable port location model
      // If you want to support non-rotated port candidates, you can just provide undecorated instances here
      decorator.Nodes.PortCandidateProvider.AddFactory(node => node.Style is RotatableNodeStyleDecorator,
          CreatePortCandidateProvider);

      decorator.Ports.EdgePathCropper.AddConstant(new AdjustOutlinePortInsidenessEdgePathCropper());
      decorator.Nodes.GroupBoundsCalculator.AddFactory(node => new RotationAwareGroupBoundsCalculator(node));

      graph.NodeDefaults.Style = new RotatableNodeStyleDecorator(DemoStyles.CreateDemoNodeStyle());
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Size = new SizeD(100, 50);

      var coreLabelModel = new InteriorNodeLabelModel();
      graph.NodeDefaults.Labels.Style = DemoStyles.CreateDemoNodeLabelStyle();
      graph.NodeDefaults.Labels.LayoutParameter =
          new RotatableNodeLabelModelDecorator(coreLabelModel).CreateWrappingParameter(InteriorNodeLabelModel.Center);

      // Make ports visible
      graph.NodeDefaults.Ports.Style = new ShapePortStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)), 1.5)
      };
      // Use a rotatable port model as default
      graph.NodeDefaults.Ports.LocationParameter =
          new RotatablePortLocationModelDecorator().CreateWrappingParameter(FreeNodePortLocationModel.Top);

      graph.GroupNodeDefaults.Style = DemoStyles.CreateDemoGroupStyle(foldingEnabled: true);

      // enable orthogonal edge editing
      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(orthogonalEditing: true);
      graph.EdgeDefaults.Labels.Style = DemoStyles.CreateDemoEdgeLabelStyle();
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel { Distance = 10 }.CreateRatioParameter();

      // enable undo
      foldingManager.MasterGraph.SetUndoEngineEnabled(true);

      graphControl.Graph = graph;
    }

    /// <summary>
    ///   Creates an <see cref="IPortCandidateProvider"/> that considers the node's shape and rotation.
    /// </summary>
    private static IPortCandidateProvider CreatePortCandidateProvider(INode node) {
      var rotatedPortModel = RotatablePortLocationModelDecorator.Instance;
      var freeModel = FreeNodePortLocationModel.Instance;

      var rnsd = (RotatableNodeStyleDecorator) node.Style;
      var wrapped = rnsd.Wrapped;
      var sns = wrapped as ShapeNodeStyle;

      if (sns != null && sns.Shape == ShapeNodeShape.RoundRectangle) {
        return PortCandidateProviders.Combine(
            // Take all existing ports - these are assumed to have the correct port location model
            PortCandidateProviders.FromUnoccupiedPorts(node),
            // Provide explicit candidates - these are all backed by a rotatable port location model
            PortCandidateProviders.FromCandidates(
                // Port candidates at the corners that are slightly inset
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter(freeModel.CreateParameter(new PointD(0, 0),
                        new PointD(5, 5)))),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter(freeModel.CreateParameter(new PointD(0, 1),
                        new PointD(5, -5)))),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter(freeModel.CreateParameter(new PointD(1, 0),
                        new PointD(-5, 5)))),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter(freeModel.CreateParameter(new PointD(1, 1),
                        new PointD(-5, -5)))),
                // Port candidates at the sides and the center
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Left)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Bottom)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Center)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Top)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Right))));
      }
      if (wrapped is RectangleNodeStyle rns) {
        // Rectangle: create ports in the corners and
        var cornerSize = rns.CornerSize * (rns.CornerStyle == CornerStyle.Cut ? 0.5 : 0.3);
        return PortCandidateProviders.Combine(
            // Take all existing ports - these are assumed to have the correct port location model
            PortCandidateProviders.FromUnoccupiedPorts(node),
            // Provide explicit candidates - these are all backed by a rotatable port location model
            PortCandidateProviders.FromCandidates(
                // Port candidates at the corners
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter((rns.Corners & Corners.TopLeft) == Corners.TopLeft
                        ? freeModel.CreateParameter(new PointD(0, 0), new PointD(cornerSize, cornerSize))
                        : FreeNodePortLocationModel.TopLeft)),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter((rns.Corners & Corners.BottomLeft) == Corners.BottomLeft
                        ? freeModel.CreateParameter(new PointD(0, 1), new PointD(cornerSize, -cornerSize))
                        : FreeNodePortLocationModel.BottomLeft)),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter((rns.Corners & Corners.TopRight) == Corners.TopRight
                        ? freeModel.CreateParameter(new PointD(1, 0), new PointD(-cornerSize, cornerSize))
                        : FreeNodePortLocationModel.TopRight)),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter((rns.Corners & Corners.BottomRight) == Corners.BottomRight
                        ? freeModel.CreateParameter(new PointD(1, 1), new PointD(-cornerSize, -cornerSize))
                        : FreeNodePortLocationModel.BottomRight)),
                // Port candidates at the sides and the center
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Left)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Bottom)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Center)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Top)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Right))));
      }
      if (sns != null && sns.Shape == ShapeNodeShape.Rectangle) {
        return PortCandidateProviders.Combine(PortCandidateProviders.FromUnoccupiedPorts(node),
            PortCandidateProviders.FromCandidates(
                // Port candidates at the corners
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.TopLeft)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.TopRight)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.BottomLeft)),
                new PortCandidate(node,
                    rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.BottomRight)),
                // Port candidates at the sides and the center
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Left)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Bottom)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Center)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Top)),
                new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(FreeNodePortLocationModel.Right))));
      }
      // Can be an arbitrary style
      var dummyNode = new SimpleNode {
          Style = wrapped,
          Layout = node.Layout
      };
      // candidates at the corners and in the middle of each side
      var shapeProvider = PortCandidateProviders.FromShapeGeometry(dummyNode, 0, 0.5);
      var shapeCandidates = shapeProvider.GetTargetPortCandidates(null);
      var rotatingCandidates = shapeCandidates.Select(c =>
          new PortCandidate(node, rotatedPortModel.CreateWrappingParameter(c.LocationParameter)));
      return PortCandidateProviders.Combine(PortCandidateProviders.FromUnoccupiedPorts(node),
          PortCandidateProviders.FromCandidates(rotatingCandidates));
    }

    /// <summary>
    ///   Loads the graph from the 'Resources' folder.
    /// </summary>
    private void LoadGraph(string graphName) {
      var stream = LoadAssemblyResource(graphName + ".graphml");
      graphControl.Graph.Clear();
      graphMLSupport.ImportFromGraphML(stream);
    }

    #endregion

    #region Layout

    private async void LayoutChooserBoxSelectedIndexChanged(object sender, SelectionChangedEventArgs e) {
      if (IsInitialized) {
        await ApplyLayout();
      }
    }

    private async void OnLayoutClick(object sender, RoutedEventArgs e) {
      await ApplyLayout();
    }

    /// <summary>
    ///   Runs a layout algorithm which is configured to consider node rotations.
    /// </summary>
    public async Task ApplyLayout() {
      var graph = graphControl.Graph;

      // provide the rotated outline and layout for the layout algorithm
      var layoutData = graph.CreateGenericLayoutData();
      layoutData.AddItemMapping(RotatedNodeLayoutStage.RotatedNodeLayoutDataKey).MapperFunction = node => {
        var style = node.Style;
        var outline = style.Renderer.GetShapeGeometry(node, style).GetOutline();
        if (outline == null) {
          outline = new GeneralPath(4);
          outline.AppendRectangle(node.Layout.ToRectD(), false);
        }
        return new RotatedNodeLayoutStage.RotatedNodeShape(outline,
            style is RotatableNodeStyleDecorator
                ? ((RotatableNodeStyleDecorator) style).GetRotatedLayout(node)
                : new OrientedRectangle(node.Layout));
      };

      // get the selected layout algorithm
      var layout = GetLayoutAlgorithm();

      // wrap the algorithm in RotatedNodeLayoutStage to make it aware of the node rotations
      var rotatedLayout = new RotatedNodeLayoutStage(layout) { EdgeRoutingMode = GetRoutingMode() };

      // apply the layout (disable RunInThread because PortAdjustmentStage wouldn't work with custom RotatableNodeStyle)
      var executor = new LayoutExecutor(graphControl, rotatedLayout) {
          RunInThread = false,
          AnimationDuration = TimeSpan.FromMilliseconds(700),
          LayoutData = layoutData
      };
      await executor.Start();
    }

    /// <summary>
    ///   Gets the layout algorithm selected by the user.
    /// </summary>
    private ILayoutAlgorithm GetLayoutAlgorithm() {
      var graph = graphControl.Graph;
      var item = layoutChooserBox.SelectedItem as ComboBoxItem;
      var layoutName = item != null ? item.Tag as string : null;
      ILayoutAlgorithm layout = new HierarchicalLayout();
      if (layoutName != null) {
        if (layoutName == "hierarchical") {
          layout = new HierarchicalLayout();
        } else if (layoutName == "organic") {
          layout = new OrganicLayout {
              DefaultPreferredEdgeLength = 1.5 * Math.Max(graph.NodeDefaults.Size.Width, graph.NodeDefaults.Size.Height)
          };
        } else if (layoutName == "orthogonal") {
          layout = new OrthogonalLayout();
        } else if (layoutName == "circular") {
          layout = new CircularLayout();
        } else if (layoutName == "tree") {
          layout = new TreeLayout { TreeReductionStage = { NonTreeEdgeRouter = new OrganicEdgeRouter() } };
        } else if (layoutName == "radial-tree") {
          layout = new RadialTreeLayout { TreeReductionStage = { NonTreeEdgeRouter = new OrganicEdgeRouter() } };
        } else if (layoutName == "radial") {
          layout = new RadialLayout();
        } else if (layoutName == "router-polyline") {
          layout = new EdgeRouter();
        } else if (layoutName == "router-organic") {
          layout = new OrganicEdgeRouter { AllowEdgeNodeOverlaps = false };
        }
      }
      return layout;
    }

    /// <summary>
    ///   Get the routing mode that suits the selected layout algorithm.
    /// </summary>
    /// <remarks>
    ///   Layout algorithm that place edge ports in the center of the node don't need to add a routing step.
    /// </remarks>
    private RotatedNodeLayoutStage.RoutingMode GetRoutingMode() {
      var item = layoutChooserBox.SelectedItem as ComboBoxItem;
      var layoutName = item != null ? item.Tag as string : null;
      if (layoutName != null && (layoutName == "hierarchical" || layoutName == "orthogonal" || layoutName == "tree" ||
          layoutName == "router-polyline")) {
        return RotatedNodeLayoutStage.RoutingMode.ShortestStraightPathToBorder;
      }
      return RotatedNodeLayoutStage.RoutingMode.NoRouting;
    }

    #endregion

    /// <summary>
    ///   Toggle snapping.
    /// </summary>
    private void SnappingButtonClick(object sender, EventArgs e) {
      ((GraphEditorInputMode) graphControl.InputMode!).SnapContext.Enabled = ((ToggleButton) sender).IsChecked == true;
    }

    /// <summary>
    ///   Toggle orthogonal edge editing.
    /// </summary>
    private void OrthogonalEditingButtonClick(object sender, EventArgs e) {
      ((GraphEditorInputMode) graphControl.InputMode!).OrthogonalEdgeEditingContext.Enabled =
          ((ToggleButton) sender).IsChecked == true;
    }

    /// <summary>
    ///   Load the selected graph.
    /// </summary>
    private void GraphChooserBoxSelectedIndexChanged(object sender, SelectionChangedEventArgs e) {
      if (IsInitialized && graphMLSupport != null &&
          graphChooserBox.SelectedItem is ComboBoxItem { Tag: string graphName }) {
        LoadGraph(graphName);
      }
    }
  }
}
