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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.RenderingOrder;

public enum RenderingOrder
{
  Default,
  AtOwner,
  EdgesOnTop,
  GroupNodes,
  None
}

[Demo("rendering-order", "view", "Rendering Order", Summary = "Shows different rendering order settings.",
    Tags = ["rendering", "z-order", "grouping"], Keywords = ["hierarchical nesting", "ports", "labels", "groups"])]
public sealed partial class RenderingOrderDemo
{
  private bool initialized;

  public RenderingOrderDemo() {
    InitializeComponent();
  }

  /// <summary>
  ///   Bootstraps the demo.
  /// </summary>
  protected override void Initialize() {
    // initialize graph component
    graphControl.InputMode = new GraphEditorInputMode();
    graphControl.Graph.SetUndoEngineEnabled(true);

    // configures default styles for newly created graph elements
    InitDemoDefaults();

    // add a sample graph
    CreateGraph();

    initialized = true;
    InitializeUI();
  }

  /// <summary>
  ///   Switches between pre-defined rendering order settings for common use cases.
  ///   Note: The settings may also be combined in different ways, too.
  /// </summary>
  private void SelectRenderingOrder(RenderingOrder order) {
    var graphModelManager = graphControl.GraphModelManager;

    // set to default first
    graphModelManager.NodeLabelLayerPolicy = LabelLayerPolicy.SeparateLayer;
    graphModelManager.EdgeLabelLayerPolicy = LabelLayerPolicy.SeparateLayer;
    graphModelManager.PortLayerPolicy = PortLayerPolicy.SeparateLayer;
    graphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.NodesAndEdges;
    graphModelManager.EdgeGroup.Below(graphModelManager.NodeGroup);

    switch (order) {
      case RenderingOrder.AtOwner:
        graphModelManager.NodeLabelLayerPolicy = LabelLayerPolicy.AtOwner;
        graphModelManager.EdgeLabelLayerPolicy = LabelLayerPolicy.AtOwner;
        graphModelManager.PortLayerPolicy = PortLayerPolicy.AtOwner;
        break;
      case RenderingOrder.EdgesOnTop:
        graphModelManager.NodeLabelLayerPolicy = LabelLayerPolicy.AtOwner;
        graphModelManager.EdgeLabelLayerPolicy = LabelLayerPolicy.AtOwner;
        graphModelManager.PortLayerPolicy = PortLayerPolicy.AtOwner;
        graphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.Nodes;
        graphModelManager.EdgeGroup.Above(graphModelManager.NodeGroup);
        break;
      case RenderingOrder.GroupNodes:
        graphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.GroupNodes;
        break;
      case RenderingOrder.None:
        graphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.None;
        break;
    }
  }

  /// <summary>
  ///   Initializes the defaults for the styles in this demo.
  /// </summary>
  private void InitDemoDefaults() {
    var graph = graphControl.Graph;
    DemoStyles.InitDemoStyles(graph);
    graph.NodeDefaults.Ports.Style = new ShapePortStyle {
        Brush = new SolidColorBrush(Color.FromRgb(0x11, 0x1D, 0x4A)),
        Shape = ShapeNodeShape.Ellipse
    };
  }

  /// <summary>
  ///   Creates the sample graph.
  /// </summary>
  private void CreateGraph() {
    CreateOverlappingLabelSample(new PointD(-290, 0));
    CreateOverlappingNodeSample(new PointD(10, 0));
    CreateOverlappingEdgeSample(new PointD(370, 0));
    CreateNestedGroupSample(new PointD(800, 0));
    graphControl.FitGraphBounds();
    graphControl.Graph.GetUndoEngine()?.Clear();
  }

  /// <summary>
  ///   Creates a sample graph with overlapping exterior node labels.
  /// </summary>
  private void CreateOverlappingLabelSample(PointD origin) {
    var graph = graphControl.Graph;
    var firstNode = graph.CreateNode(new RectD(origin.X, origin.Y + 50, 50, 50));
    graph.AddLabel(firstNode, "External Node Label 1", ExteriorNodeLabelModel.Bottom);
    var secondNode = graph.CreateNode(new RectD(origin.X + 60, origin.Y + 80, 50, 50));
    graph.AddLabel(secondNode, "External Node Label 2", ExteriorNodeLabelModel.Bottom);

    graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup,
        new RectangleBorder(new PointD(origin.X - 50, origin.Y - 20), new SizeD(210, 250),
            new PointD(origin.X + 5, origin.Y - 30), "Try 'Default'"));
  }

  /// <summary>
  ///   Creates a sample graph with overlapping nodes that have interior node labels.
  /// </summary>
  private void CreateOverlappingNodeSample(PointD origin) {
    var graph = graphControl.Graph;

    // overlapping nodes
    var back1 = graph.CreateNode(new RectD(origin.X, origin.Y + 20, 50, 50));
    graph.AddLabel(back1, "Back");
    var middle1 = graph.CreateNode(new RectD(origin.X + 20, origin.Y + 35, 50, 50));
    graph.AddLabel(middle1, "Middle");
    var front1 = graph.CreateNode(new RectD(origin.X + 40, origin.Y + 50, 50, 50));
    graph.AddLabel(front1, "Front");

    // overlapping nodes with ports
    var back2 = graph.CreateNode(new RectD(origin.X + 120, origin.Y + 20, 50, 50));
    var middle2 = graph.CreateNode(new RectD(origin.X + 140, origin.Y + 35, 50, 50));
    var front2 = graph.CreateNode(new RectD(origin.X + 160, origin.Y + 50, 50, 50));
    var nodes = new[] { back2, middle2, front2 };
    foreach (var node in nodes) {
      graph.AddPort(node, FreeNodePortLocationModel.Bottom);
      graph.AddPort(node, FreeNodePortLocationModel.Top);
      graph.AddPort(node, FreeNodePortLocationModel.Left);
      graph.AddPort(node, FreeNodePortLocationModel.Right);
    }

    var edge1 = graph.CreateEdge(back1, back2);
    graph.AddBend(edge1, new PointD(origin.X + 25, origin.Y + 185));
    graph.AddBend(edge1, new PointD(origin.X + 145, origin.Y + 185));
    graph.AddLabel(edge1, "Edge Label", new SmartEdgeLabelModel().CreateParameterFromSource(1));
    graph.SetPortLocationParameter(edge1.SourcePort,
        FreeNodePortLocationModel.Instance.CreateParameter(new PointD(0.5, 0.5), new PointD(0, 25)));
    var edge2 = graph.CreateEdge(front1, front2);
    graph.AddBend(edge2, new PointD(origin.X + 65, origin.Y + 190));
    graph.AddBend(edge2, new PointD(origin.X + 185, origin.Y + 190));
    graph.SetPortLocationParameter(edge2.SourcePort,
        FreeNodePortLocationModel.Instance.CreateParameter(new PointD(0.5, 0.5), new PointD(0, 25)));

    graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup,
        new RectangleBorder(new PointD(origin.X - 50, origin.Y - 20), new SizeD(310, 250),
            new PointD(origin.X - 15, origin.Y - 30), "Try 'Labels/Ports At Owner'"));
  }

  /// <summary>
  ///   Creates a sample graph with an edge that crosses a group node.
  /// </summary>
  private void CreateOverlappingEdgeSample(PointD origin) {
    var graph = graphControl.Graph;

    var srcNode = graph.CreateNode(new RectD(origin.X, origin.Y + 60, 50, 50));
    var tgtNode1 = graph.CreateNode(new RectD(origin.X + 250, origin.Y + 60, 50, 50));
    var tgtNode2 = graph.CreateNode(new RectD(origin.X + 122.5, origin.Y + 130, 50, 50));
    var groupNode = graph.CreateGroupNode(null, new RectD(origin.X + 85, origin.Y, 125, 200));
    graph.AddLabel(groupNode, "Group Node");
    graph.SetParent(tgtNode2, groupNode);

    var edge = graph.CreateEdge(srcNode, tgtNode1);
    graph.AddLabel(edge, "Edge Label");
    var innerEdge = graph.CreateEdge(srcNode, tgtNode2);
    graph.AddBend(innerEdge, new PointD(origin.X + 25, origin.Y + 155));

    graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup,
        new RectangleBorder(new PointD(origin.X - 20, origin.Y - 20), new SizeD(340, 250),
            new PointD(origin.X - 5, origin.Y - 30), "'Edges on top' or 'Group Nodes'"));
  }

  /// <summary>
  ///   Creates a sample graph with nested group nodes and edges.
  /// </summary>
  private void CreateNestedGroupSample(PointD origin) {
    var graph = graphControl.Graph;

    var root = graph.CreateGroupNode(null, new RectD(origin.X, origin.Y, 230, 220));
    graph.AddLabel(root, "Outer Group Node");

    var outerChild1 = graph.CreateNode(root, new RectD(origin.X + 145, origin.Y + 30, 50, 50));
    graph.AddLabel(outerChild1, "Outer\nChild");
    var outerChild2 = graph.CreateNode(root, new RectD(origin.X + 40, origin.Y + 140, 50, 50));
    graph.AddLabel(outerChild2, "Outer\nChild");
    var outerEdge = graph.CreateEdge(outerChild1, outerChild2);
    graph.AddBend(outerEdge, new PointD(origin.X + 65, origin.Y + 55));

    var childGroup = graph.CreateGroupNode(root, new RectD(origin.X + 20, origin.Y + 50, 150, 150));
    graph.AddLabel(childGroup, "Inner Group Node");
    var innerNode1 = graph.CreateNode(childGroup, new RectD(origin.X + 40, origin.Y + 80, 50, 50));
    graph.AddLabel(innerNode1, "Inner\nChild");
    var innerNode2 = graph.CreateNode(childGroup, new RectD(origin.X + 100, origin.Y + 140, 50, 50));
    graph.AddLabel(innerNode2, "Inner\nChild");
    var innerEdge = graph.CreateEdge(innerNode1, innerNode2);
    graph.AddBend(innerEdge, new PointD(origin.X + 125, origin.Y + 105));

    graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup,
        new RectangleBorder(new PointD(origin.X - 20, origin.Y - 20), new SizeD(280, 250),
            new PointD(origin.X + 20, origin.Y - 30), "Try different settings"));
  }

  /// <summary>
  ///   Binds the actions to the buttons in the demo's toolbar.
  /// </summary>
  private void InitializeUI() {
    if (renderingOrderComboBox.SelectedValue is RenderingOrder order) {
      SelectRenderingOrder(order);
    }
  }

  private void OnNewGraph(object sender, RoutedEventArgs args) {
    var graph = graphControl.Graph;
    graph.Clear();
    graph.GetUndoEngine()?.Clear();
    graphControl.FitGraphBounds();
  }

  private void OnRenderingOrderSelectionChanged(object sender, SelectionChangedEventArgs args) {
    if (initialized && renderingOrderComboBox.SelectedValue is RenderingOrder order) {
      SelectRenderingOrder(order);
    }
  }

  [STAThread]
  private static void Main() {
    Run<RenderingOrderDemo>();
  }

  /// <summary>
  ///   Creates a boundary rectangle with a title for a sample graph.
  /// </summary>
  private sealed class RectangleBorder : IVisualCreator
  {
    private readonly string title;
    private readonly PointD titleOrigin;
    private readonly SizeD size;
    private readonly PointD rectOrigin;

    /// <param name="rectOrigin">the position where to draw the rectangle</param>
    /// <param name="size">the size of the rectangle</param>
    /// <param name="titleOrigin">the position where to draw the title</param>
    /// <param name="title">the content of the label above the rectangle</param>
    public RectangleBorder(PointD rectOrigin, SizeD size, PointD titleOrigin, string title) {
      this.rectOrigin = rectOrigin;
      this.size = size;
      this.titleOrigin = titleOrigin;
      this.title = title;
    }

    public Visual CreateVisual(IRenderContext context) {
      const double fontSize = 18;
      var text = new TextBlock {
          Text = title,
          FontFamily = new FontFamily("Arial"),
          FontSize = fontSize,
          FontWeight = FontWeights.Bold,
          Foreground = Brushes.Gray,
          TextWrapping = TextWrapping.NoWrap
      };
      text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      var textSize = text.DesiredSize;

      var left = Math.Min(rectOrigin.X, titleOrigin.X);
      var top = Math.Min(rectOrigin.Y, titleOrigin.Y - fontSize);
      var right = Math.Max(rectOrigin.X + size.Width, titleOrigin.X + textSize.Width);
      var bottom = Math.Max(rectOrigin.Y + size.Height, titleOrigin.Y + fontSize * 0.25);
      var group = new VisualGroup();
      group.SetCanvasArrangeRect(new Rect(left, top, right - left, bottom - top));

      var rectangle = new Rectangle {
          Fill = Brushes.Transparent,
          Stroke = Brushes.Gray,
          StrokeThickness = 4,
          StrokeDashArray = [5, 5]
      };
      rectangle.SetCanvasArrangeRect(new Rect(rectOrigin.X - left, rectOrigin.Y - top, size.Width, size.Height));
      group.Add(rectangle);

      text.SetCanvasArrangeRect(new Rect(titleOrigin.X - left, titleOrigin.Y - fontSize - top, textSize.Width,
          textSize.Height));
      group.Add(text);
      return group;
    }

    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) => oldVisual ?? CreateVisual(context);
  }
}
