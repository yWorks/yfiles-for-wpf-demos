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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.SelectionStyling
{
  /// <summary>
  ///   Shows customized selection painting of nodes, edges and labels by decorating these items with a corresponding style.
  /// </summary>
  [Demo("selection-styling", "style", "Selection Styling",
      Summary =
          "Shows customized selection painting of nodes, edges and labels by decorating these items with a corresponding style.",
      Keywords = ["selection styling", "labels", "notable style"], Tags = ["style", "interaction"])]
  public sealed partial class SelectionStylingDemo
  {
    private NodeStyleIndicatorRenderer? selectionNodeStyle;

    private bool useCustomNodeSelectionIndicator = true;

    private EdgeStyleIndicatorRenderer? selectionEdgeStyle;

    private bool useCustomEdgeSelectionIndicator = true;

    private LabelStyleIndicatorRenderer? selectionLabelStyle;

    private bool useCustomLabelSelectionIndicator = true;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public SelectionStylingDemo() {
      // Initialize UI elements.
      InitializeComponent();
    }

    protected override void Initialize() {
      // Initialize the selection decorations.
      InitializeDecoration();

      // Initialize default graph styles, the graph and input modes.
      InitializeGraph();

      // Initialize the zoom mode decoration.
      UpdateZoomModeDecoration();

      InitializeUI();
    }

    /// <summary>
    ///   Initializes the styles for the graph nodes, edges, and labels.
    /// </summary>
    private void InitializeGraph() {
      var graph = Graph;

      graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle();
      graph.NodeDefaults.Size = new SizeD(50, 30);

      // Defaults for labels.
      var simpleLabelStyle = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 195, 152)),
          TextBrush = new SolidColorBrush(Color.FromRgb(102, 43, 0)),
          Padding = new InsetsD(3, 5, 3, 5)
      };

      // Nodes...
      graph.NodeDefaults.Labels.Style = simpleLabelStyle;
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(15) }.CreateParameter(ExteriorNodeLabelModel.Position.Top);

      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle();
      graph.EdgeDefaults.Labels.Style = simpleLabelStyle;

      // Labels.
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel().CreateRatioParameter();

      // Create a simple sample graph.
      var n1 = graph.CreateNode(new RectD(0, 0, 50, 30));
      var n2 = graph.CreateNode(new RectD(250, 20, 50, 30));
      graph.AddLabel(n1, "Node 1");
      graph.AddLabel(n2, "Node 2");

      var edge = graph.CreateEdge(n1, n2);
      graph.AddBend(edge, new PointD(100, 35));
      graph.AddLabel(edge, "Edge Label");

      // Center the graph on the screen.
      GraphControl.FitGraphBounds();

      // Select all elements to show the effect.
      SelectAllNodes();
      SelectAllEdges();
      SelectAllLabels();

      // Initialize the input mode to enable editing and selecting nodes, edges, and labels at once with the marquee.
      GraphControl.InputMode = new GraphEditorInputMode {
          MarqueeSelectableItems = GraphItemTypes.LabelOwner | GraphItemTypes.Label
      };
    }

    /// <summary>
    ///   Initializes the selection decorations.
    /// </summary>
    private void InitializeDecoration() {
      var selectionBrush = new SolidColorBrush(Color.FromRgb(1, 186, 255));

      selectionNodeStyle = new NodeStyleIndicatorRenderer {
          // For nodes...
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.RoundRectangle,
              Pen = new Pen(selectionBrush, 1),
              Brush = Brushes.Transparent
          },
          // With a margin for the decoration.
          Margins = new InsetsD(8)
      };

      // for edges...
      selectionEdgeStyle = new EdgeStyleIndicatorRenderer {
          // Just a thick polyline edge style.
          EdgeStyle = new PolylineEdgeStyle { Pen = new Pen(selectionBrush, 3) }
      };

      // ... and for labels
      selectionLabelStyle = new LabelStyleIndicatorRenderer {
          // We use a rectangular label style for the decoration and declare a margin around it.
          LabelStyle = new LabelStyle {
              Shape = LabelShape.Rectangle,
              BackgroundPen = new Pen(selectionBrush, 1),
              BackgroundBrush = Brushes.Transparent,
              TextBrush = Brushes.Transparent
          },
          Margins = new InsetsD(5)
      };

      Graph.GetDecorator().Nodes.SelectionRenderer.AddWrapperFactory((_, original) =>
          useCustomNodeSelectionIndicator ? selectionNodeStyle : original);

      // For edges...
      Graph.GetDecorator().Edges.SelectionRenderer.AddWrapperFactory((_, original) =>
          useCustomEdgeSelectionIndicator ? selectionEdgeStyle : original);

      // ... and for labels.
      Graph.GetDecorator().Labels.SelectionRenderer.AddWrapperFactory((_, original) =>
          useCustomLabelSelectionIndicator ? selectionLabelStyle : original);

      // Hide focus indication.
      Graph.GetDecorator().Nodes.FocusRenderer.Hide();
    }

    /// <summary>
    ///   Sets, removes and updates the custom selection decoration for nodes,
    ///   edges, and labels according to the current settings.
    /// </summary>
    private void UpdateZoomModeDecoration() {
      if (ZoomModeComboBox.SelectedValue is not StyleIndicatorZoomPolicy selectedZoomMode) {
        return;
      }

      selectionNodeStyle?.ZoomPolicy = selectedZoomMode;
      selectionEdgeStyle?.ZoomPolicy = selectedZoomMode;
      selectionLabelStyle?.ZoomPolicy = selectedZoomMode;
    }

    private void SelectAllNodes() {
      var nodeSelection = GraphControl.Selection.Nodes;
      nodeSelection.Clear();
      foreach (var node in Graph.Nodes) {
        nodeSelection.Add(node);
      }
    }

    private void SelectAllEdges() {
      var edgeSelection = GraphControl.Selection.Edges;
      edgeSelection.Clear();
      foreach (var edge in Graph.Edges) {
        edgeSelection.Add(edge);
      }
    }

    private void SelectAllLabels() {
      var labelSelection = GraphControl.Selection.Labels;
      labelSelection.Clear();
      foreach (var label in Graph.Labels) {
        labelSelection.Add(label);
      }
    }

    /// <summary>
    ///   Wires up the UI.
    /// </summary>
    private void InitializeUI() {
      NodeDecorationToggle.Checked += (_, _) => CustomNodeDecorationChanged(true);
      NodeDecorationToggle.Unchecked += (_, _) => CustomNodeDecorationChanged(false);
      EdgeDecorationToggle.Checked += (_, _) => CustomEdgeDecorationChanged(true);
      EdgeDecorationToggle.Unchecked += (_, _) => CustomEdgeDecorationChanged(false);
      LabelDecorationToggle.Checked += (_, _) => CustomLabelDecorationChanged(true);
      LabelDecorationToggle.Unchecked += (_, _) => CustomLabelDecorationChanged(false);
      ZoomModeComboBox.SelectionChanged += (_, _) => ZoomModeChanged();
    }

    private void CustomNodeDecorationChanged(bool value) {
      useCustomNodeSelectionIndicator = value;

      // De-select and re-select all nodes to refresh the selection visualization.
      SelectAllNodes();
    }

    private void CustomEdgeDecorationChanged(bool value) {
      useCustomEdgeSelectionIndicator = value;

      // De-select and re-select all edges to refresh the selection visualization.
      SelectAllEdges();
    }

    private void CustomLabelDecorationChanged(bool value) {
      useCustomLabelSelectionIndicator = value;

      // Deselect and re-select all labels to refresh the selection visualization.
      SelectAllLabels();
    }

    private void ZoomModeChanged() {
      UpdateZoomModeDecoration();
      GraphControl.Invalidate();
    }

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    [STAThread]
    public static void Main() {
      Run<SelectionStylingDemo>();
    }
  }
}
