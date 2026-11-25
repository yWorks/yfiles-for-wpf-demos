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
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.DragAndDrop
{
  /// <summary>
  ///   This demo shows how to enable drag and drop functionality for nodes,
  ///   edges, labels, and ports.
  /// </summary>
  /// <remarks>
  ///   It uses the yFiles library classes <see cref="NodeDropInputMode"/>, <see cref="LabelDropInputMode"/>,
  ///   <see cref="PortDropInputMode"/>, and the custom class <see cref="EdgeDropInputMode"/>.
  /// </remarks>
  [Demo("drag-and-drop", "input", "Drag and Drop", Summary = "Shows drag and drop of nodes, groups and labels.",
      Keywords = ["palette", "ports", "labels", "groups", "move", "dnd"], Tags = ["interaction", "drag and drop"],
      RelatedDemos = ["graph-drag-and-drop", "drag-from-component", "custom-drag-and-drop", "component-drag-and-drop"])]
  public sealed partial class DragAndDropDemo
  {
    private NodeDropInputMode nodeDropInputMode = null!;

    public DragAndDropDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      // initialize the GraphControl
      InitializeInteraction(GraphControl);
      var yFilesLogo = (ImageSource) FindResource("yfiles-logo");
      DragAndDrop.InitializeDnDPanel(dndPanel, yFilesLogo);

      // init graph default styles and visual decorators
      InitializeGraph(Graph);

      // build the graph from the given data set
      BuildGraph(Graph, LoadGraphData(), yFilesLogo);

      // layout and center the graph
      Graph.ApplyLayout(new HierarchicalLayout {
          DefaultEdgeDescriptor = {
              MinimumFirstSegmentLength = 25,
              MinimumLastSegmentLength = 25
          },
          MinimumLayerDistance = 50
      });
      GraphControl.FitGraphBounds();
      GraphControl.Zoom = 1.5;

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      Graph.SetUndoEngineEnabled(true);

      InitializeUI();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private static void BuildGraph(IGraph graph, GraphData graphData, ImageSource yFilesLogo) {
      var graphBuilder = new GraphBuilder(graph);

      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodeList, item => item.Id);
      nodesSource.NodeCreator.StyleProvider = item => item.Tag == "icon"
          ? new ImageNodeStyle(yFilesLogo) { KeepAspectRatio = true }
          : graph.NodeDefaults.GetStyleInstance();
      nodesSource.NodeCreator.LayoutProvider = item => item.Tag == "icon"
          ? new RectD(0, 0, 50, 50)
          : new RectD(0, 0, 80, 40);
      nodesSource.NodeCreator.CreateLabelBinding(item => item.Label).LayoutParameterProvider =
          _ => InteriorNodeLabelModel.Center;

      graphBuilder.CreateEdgesSource(graphData.EdgeList, item => item.Source, item => item.Target, item => item.Id);

      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Initializes the graph.
    /// </summary>
    /// <remarks>
    ///   Sets up styles and visual decorations of graph elements.
    /// </remarks>
    private static void InitializeGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);

      graph.NodeDefaults.Size = new SizeD(80, 40);
      // draw a port with an elliptical shape
      graph.NodeDefaults.Ports.Style = new ShapePortStyle {
          Brush = Brushes.LightGray,
          Pen = new Pen(Brushes.Gray, 1),
          Shape = ShapeNodeShape.Ellipse
      };
      graph.NodeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xff, 0xc4, 0x99)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)),
          Padding = new InsetsD(5),
          Shape = LabelShape.RoundRectangle
      };
      graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Center;
      graph.EdgeDefaults.Ports.Style = new ShapePortStyle {
          Brush = Brushes.LightGray,
          Pen = new Pen(Brushes.Gray, 1),
          Shape = ShapeNodeShape.Ellipse
      };
      graph.EdgeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xc2, 0xaa, 0x99)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)),
          Padding = new InsetsD(5),
          Shape = LabelShape.RoundRectangle
      };
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0);

      // initialize edge highlight when an edge element from dnd panel is dragged on a valid edge target
      graph.GetDecorator().Edges.HighlightRenderer.AddConstant(new EdgeStyleIndicatorRenderer {
          EdgeStyle = new PolylineEdgeStyle { Pen = new Pen(Brushes.CornflowerBlue, 2) },
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      });
    }

    /// <summary>
    ///   Creates and activates a <see cref="GraphEditorInputMode"/> and configures it to enable drag and drop.
    /// </summary>
    private void InitializeInteraction(GraphControl graphControl) {
      // configure the snapping context
      var mode = new GraphEditorInputMode {
          SnapContext = new GraphSnapContext {
              SnappableItems = SnappableItems.Node | SnappableItems.Edge,
              NodeDistance = 30,
              NodeToEdgeDistance = 20,
              SnapDistance = 10,
              GridSnapType = GridSnapTypes.All
          }
      };

      // create a new NodeDropInputMode to configure the drag and drop operation
      nodeDropInputMode = new NodeDropInputMode {
          // enables the display of the dragged element during the drag
          ShowPreview = true,
          // initially disables snapping for the dragged element to existing elements
          SnappingEnabled = false,
          // by default, the mode available in GraphEditorInputMode is disabled, so first enable it
          Enabled = true,
          // nodes that have a GroupNodeStyle assigned have to be created as group nodes
          IsGroupNodePredicate = draggedNode => draggedNode.Style is GroupNodeStyle
      };
      mode.NodeDropInputMode = nodeDropInputMode;

      // create a new LabelDropInputMode to configure the drag and drop operation
      mode.LabelDropInputMode = new MatchingLabelDropInputMode {
          ShowPreview = true,
          SnappingEnabled = false,
          Enabled = true,
          UseLocationForParameter = true
      };

      // create a new PortDropInputMode to configure the drag and drop operation
      mode.PortDropInputMode = new PortDropInputMode {
          ShowPreview = true,
          SnappingEnabled = false,
          Enabled = true,
          UseLocationForParameter = true,
          // allow only for nodes to be the new port owner
          IsValidPortOwnerPredicate = portOwner => portOwner is INode
      };

      // add the edge drop input mode
      mode.Add(new EdgeDropInputMode());

      graphControl.InputMode = mode;
    }

    /// <summary>
    ///   Registers event listeners for the snapping checkbox.
    /// </summary>
    private void InitializeUI() {
      previewSnappingCheckBox.Checked += (_, _) => nodeDropInputMode.SnappingEnabled = true;
      previewSnappingCheckBox.Unchecked += (_, _) => nodeDropInputMode.SnappingEnabled = false;
    }

    private static GraphData LoadGraphData() {
      using var stream = GetResourceStream("Resources.graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(stream)!;
    }

    private static Stream GetResourceStream(string suffix) {
      var assembly = typeof(DragAndDropDemo).Assembly;
      var resourceName = Array.Find(assembly.GetManifestResourceNames(),
          name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
      return resourceName is not null
          ? assembly.GetManifestResourceStream(resourceName)!
          : throw new InvalidOperationException($"Missing embedded resource ending in '{suffix}'.");
    }

    private sealed class MatchingLabelDropInputMode : LabelDropInputMode
    {
      public MatchingLabelDropInputMode() {
        // allow labels to be dropped only on owners of the same kind
        IsValidLabelOwnerPredicate = IsMatchingLabelOwner;
      }

      private bool IsMatchingLabelOwner(ILabelOwner labelOwner) {
        // check whether the labelOwner matches the dragged label's owner,
        // so that a dragged edge-label is only dropped on an edge
        return DraggedItem?.Owner switch {
            INode => labelOwner is INode,
            IEdge => labelOwner is IEdge,
            IPort => labelOwner is IPort,
            _ => false
        };
      }
    }

    [STAThread]
    private static void Main() => Run<DragAndDropDemo>();
  }
}
