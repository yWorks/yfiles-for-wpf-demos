/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Input.DragAndDrop
{
  /// <summary>
  ///   This demo creates a simple style chooser that shows how to use class <see cref="NodeDropInputMode"/> for drag and
  ///   drop.
  ///   In contrast to <see cref="DropInputMode"/>, <see cref="NodeDropInputMode"/> shows a preview of the node
  ///   while dragging, leverages snapping, and allows for dropping nodes into group nodes.
  /// </summary>
  /// <remarks>
  ///   To create a node, drag the desired node style from the left panel onto the canvas. See the dragged node
  ///   snap to the grid positions and to other nodes.
  /// </remarks>
  public partial class DragAndDropWindow
  {
    private readonly string[] functionOptions = { "Snapping & Preview", "Preview", "None" };


    /// <summary>
    ///   Enables support for dropping nodes on the given <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <param name="editorInputMode">The GraphEditorInputMode for this application.</param>
    private void ConfigureNodeDropInputMode(GraphEditorInputMode editorInputMode) {
      // Obtain an input mode for handling dropped nodes for the GraphEditorInputMode.
      var nodeDropInputMode = editorInputMode.NodeDropInputMode;
      // by default the mode available in GraphEditorInputMode is disabled, so first enable it
      nodeDropInputMode.Enabled = true;

      // we want nodes that have a GroupNodeStyle assigned to be created as group nodes.
      nodeDropInputMode.IsGroupNodePredicate = draggedNode => draggedNode.Style is GroupNodeStyle;

      // we enable dropping nodes onto leaf nodes ...
      nodeDropInputMode.AllowNonGroupNodeAsParent = true;
      // ... but we restrict that feature to the third nodes (and group nodes).
      nodeDropInputMode.IsValidParentPredicate = node =>
          graphControl.Graph.IsGroupNode(node) || (node.Labels.Count == 2 && node.Labels[1].Text.Contains("convert"));

      var labelDropInputMode = editorInputMode.LabelDropInputMode;
      labelDropInputMode.Enabled = true;
    }


    public DragAndDropWindow() {
      InitializeComponent();
    }

    private async void OnLoaded(object sender, EventArgs eventArgs) {
      InitializeStylesList();

      // add the visual grid
      const int gridWidth = 80;
      var gridInfo = new GridInfo(gridWidth);

      var renderTree = graphControl.RenderTree;
      renderTree.CreateElement(renderTree.BackgroundGroup, gridInfo, new GridRenderer());

      // Create and configure a GraphSnapContext to enable snapping
      var context = new GraphSnapContext {
          SnappableItems = SnappableItems.Node | SnappableItems.Edge | SnappableItems.Label,
          NodeDistance = 30,
          NodeToEdgeDistance = 20,
          SnapDistance = 10,
          NodeGridConstraintProvider = new GridConstraintProvider(gridInfo),
          BendGridConstraintProvider = new GridConstraintProvider(gridInfo),
          GridSnapType = GridSnapTypes.All
      };

      // Create and register a graph editor input mode for editing the graph
      // in the canvas.
      var editorInputMode = new GraphEditorInputMode { SnapContext = context };
      // use the style, size and labels of the currently selected palette node for newly created nodes
      editorInputMode.NodeCreator = GetNodeCreator(editorInputMode.NodeCreator);

      ConfigureNodeDropInputMode(editorInputMode);

      // use the mode in our control
      graphControl.InputMode = editorInputMode;

      // Enable undo
      graphControl.Graph.SetUndoEngineEnabled(true);

      DemoStyles.InitDemoStyles(graphControl.Graph, orthogonalEdgeEditing : true);

      // populate the control with some nodes
      CreateSampleGraph();

      await graphControl.FitGraphBounds();
      graphControl.Zoom = 2;

      featuresComboBox.ItemsSource = functionOptions;
      featuresComboBox.SelectedIndex = 0;
    }

    private NodeCreationCallback GetNodeCreator(NodeCreationCallback nodeCreator) {
      return (context, graph, location, parent) => {
        var paletteNode = styleListBox.SelectedItem as INode;
        if (paletteNode != null) {
          if (paletteNode.Tag == null || !paletteNode.Tag.ToString().EndsWith("Label Container")) {
            var newNode = nodeCreator(context, graph, location, parent);
            graph.SetStyle(newNode, paletteNode.Style);
            graph.SetNodeLayout(newNode, RectD.FromCenter(location, paletteNode.Layout.ToSizeD()));
            graph.SetIsGroupNode(newNode, paletteNode.Style is GroupNodeStyle);
            foreach (var label in paletteNode.Labels) {
              graph.AddLabel(newNode, label.Text, label.LayoutParameter, label.Style);
            }
            return newNode;
          }
        }

        return null;
      };
    }

    private void CreateSampleGraph() {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Size = new SizeD(60, 40);
      var edgeStyle = DemoStyles.CreateDemoEdgeStyle();
      edgeStyle.Pen = new Pen(edgeStyle.Pen!.Brush, 3);
      graph.EdgeDefaults.Style = edgeStyle;

      var n1 = graph.CreateNode(new PointD(0, -50));
      var n2 = graph.CreateNode(new PointD(-100, 100));
      var n3 = graph.CreateNode(new PointD(100, 100));
      var n4 = graph.CreateNode(new RectD(75, 200, 50, 50), DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse));
      graph.CreateEdge(n1, n2);
      graph.CreateEdge(n1, n3);
      graph.CreateEdge(n3, n4);
      graph.AddLabel(n4, "label");
    }

    #region Standard Actions

    /// <summary>
    ///   Callback action that is triggered when the user exits the application.
    /// </summary>
    protected virtual void ExitMenuItemClick(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion

    #region Initialize node palette

    /// <summary>
    ///   Initializes the style panel of this demo.
    /// </summary>
    private void InitializeStylesList() {
      // Create a new Graph in which the palette nodes live
      IGraph nodeContainer = new DefaultGraph();
      DemoStyles.InitDemoStyles(nodeContainer);

      var defaultLabelStyle = new LabelStyle {
          BackgroundPen = (Pen) new Pen(new SolidColorBrush(Color.FromRgb(101, 152, 204)), 1).GetAsFrozen(),
          BackgroundBrush = Brushes.White,
          Padding = new InsetsD(3, 5, 3, 5)
      };

      nodeContainer.NodeDefaults.Labels.Style = defaultLabelStyle;
      nodeContainer.EdgeDefaults.Labels.Style = defaultLabelStyle;

      // Create some nodes
      var node = nodeContainer.CreateGroupNode(layout : new RectD(0, 0, 80, 80));
      nodeContainer.AddLabel(node, "Group Node");

      nodeContainer.CreateNode(new RectD(0, 0, 60, 40),
          DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.RoundRectangle));
      nodeContainer.CreateNode(new RectD(0, 0, 50, 50), DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse));

      var nodeLabelContainer =
          nodeContainer.CreateNode(new RectD(0, 0, 80, 20), VoidNodeStyle.Instance, "Node Label Container");
      nodeContainer.AddLabel(nodeLabelContainer, "Node Label", FreeNodeLabelModel.Center,
          DemoStyles.CreateDemoNodeLabelStyle());

      var edgeLabelContainer =
          nodeContainer.CreateNode(new RectD(0, 0, 80, 20), VoidNodeStyle.Instance, "Edge Label Container");
      nodeContainer.AddLabel(edgeLabelContainer, "Edge Label", FreeNodeLabelModel.Center,
          DemoStyles.CreateDemoEdgeLabelStyle());

      // Add nodes to listview
      foreach (var n in nodeContainer.Nodes) {
        styleListBox.Items.Add(n);
      }
    }

    #endregion

    #region Toolbar events

    private void FeatureSelectionChanged(object sender, EventArgs e) {
      var nodeDropInputMode = ((GraphEditorInputMode) graphControl.InputMode).NodeDropInputMode;
      var labelDropInputMode = ((GraphEditorInputMode) graphControl.InputMode).LabelDropInputMode;
      switch (featuresComboBox.SelectedIndex) {
        case 0:
          nodeDropInputMode.SnappingEnabled = true;
          nodeDropInputMode.ShowPreview = true;
          labelDropInputMode.ShowPreview = true;
          break;
        case 1:
          nodeDropInputMode.SnappingEnabled = false;
          nodeDropInputMode.ShowPreview = true;
          labelDropInputMode.ShowPreview = true;
          break;
        case 2:
          nodeDropInputMode.SnappingEnabled = false;
          nodeDropInputMode.ShowPreview = false;
          labelDropInputMode.ShowPreview = false;
          break;
      }
    }

    private void UseDropLocationForParameterChanged(object sender, EventArgs eventArgs) {
      (graphControl.InputMode as GraphEditorInputMode)!.LabelDropInputMode.UseLocationForParameter =
          UseDragLocationToggle.IsChecked.Value;
    }

    #endregion

    private void OnTemplateMouseDown(object sender, RoutedEventArgs e) {
      // Initialize the information for a drag&drop from the node style list.
      var o = e.OriginalSource as FrameworkElement;
      if (o != null) {
        var node = o.DataContext as INode;
        if (node != null) {
          if ("Node Label Container".Equals(node.Tag)) {
            var dao = new DataObject();
            dao.SetData(typeof(ILabel), node.Labels.First());
            DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          } else if ("Edge Label Container".Equals(node.Tag)) {
            var labelTemplate = node.Labels.First();
            // Not all label models return a valid geometry when the path is empty
            var p1 = new SimplePort(new SimpleNode { Layout = new RectD(PointD.Origin, new SizeD(1, 1)) },
                FreeNodePortLocationModel.Center);
            var p2 = new SimplePort(new SimpleNode { Layout = new RectD(new PointD(0, 100), new SizeD(1, 1)) },
                FreeNodePortLocationModel.Center);
            var edge = new SimpleEdge(p1, p2);
            var dummyLabel = new SimpleLabel(edge, labelTemplate.Text, FreeEdgeLabelModel.Instance.CreateParameter()) {
                Style = labelTemplate.Style, Tag = labelTemplate.Tag, PreferredSize = labelTemplate.PreferredSize
            };

            var dao = new DataObject();
            dao.SetData(typeof(ILabel), dummyLabel);
            DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          } else {
            // use as defaults for newly created nodes
            graphControl.Graph.NodeDefaults.Style = node.Style;
            graphControl.Graph.NodeDefaults.Size = node.Layout.ToSizeD();

            var dao = new DataObject();
            dao.SetData(typeof(INode), node);
            DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          }
        }
      }
    }
  }

  /// <summary>
  ///   Convert from a node style to an image that shows the visual representation of the style
  /// </summary>
  [ValueConversion(typeof(INode), typeof(DrawingImage))]
  public class NodeImageConverter : IValueConverter
  {
    private readonly GraphControl graphControl = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is not INode listBoxNode) {
        return null;
      }

      var graph = graphControl.Graph;
      graph.Clear();

      var node = graph.CreateNode(listBoxNode.Layout.ToRectD(), listBoxNode.Style, listBoxNode.Tag);
      foreach (var label in listBoxNode.Labels) {
        graph.AddLabel(node, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
      }
      graphControl.FitGraphBounds();
      var cc = new ContextConfigurator(graphControl.ContentBounds);

      var renderContext = cc.CreateRenderContext(graphControl);
      Transform transform = cc.CreateWorldToIntermediateTransform();
      var clip = cc.CreateClip();

      var visualContent = graphControl.ExportContent(renderContext);
      var container = new VisualGroup { Transform = transform, Clip = clip, Children = { visualContent } };
      var brush = new VisualBrush(container);
      return new DrawingImage(new GeometryDrawing(brush, null,
          new RectangleGeometry(new Rect(new Point(0, 0), listBoxNode.Layout.ToSize()))));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
