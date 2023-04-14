/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Input.DragAndDrop
{

  /// <summary>
  /// This demo creates a simple style chooser that shows how to use class <see cref="NodeDropInputMode"/> for drag and drop.
  /// In contrast to <see cref="DropInputMode"/>, <see cref="NodeDropInputMode"/> shows a preview of the node
  /// while dragging, leverages snapping and allows for dropping nodes into group nodes.
  /// </summary>
  /// <remarks>To create a node, drag the desired node style from the left panel onto the canvas. See the dragged node
  /// snap to the grid positions and to other nodes.</remarks>
  public partial class DragAndDropWindow 
  {
    private string[] functionOptions = { "Snapping & Preview", "Preview", "None" };

    
    /// <summary>
    /// Enables support for dropping nodes on the given <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <param name="editorInputMode">The GraphEditorInputMode for this application.</param>
    private void ConfigureNodeDropInputMode(GraphEditorInputMode editorInputMode) {
      // Obtain an input mode for handling dropped nodes for the GraphEditorInputMode.
      var nodeDropInputMode = editorInputMode.NodeDropInputMode;
      // by default the mode available in GraphEditorInputMode is disabled, so first enable it
      nodeDropInputMode.Enabled = true;

      // we want nodes that have a PanelNodeStyle assigned to be created as group nodes.
      nodeDropInputMode.IsGroupNodePredicate = draggedNode => draggedNode.Style is GroupNodeStyle;

      // we enable dropping nodes onto leaf nodes ...
      nodeDropInputMode.AllowNonGroupNodeAsParent = true;
      // ... but we restrict that feature to the third nodes (and group nodes).
      nodeDropInputMode.IsValidParentPredicate =
        node =>
          graphControl.Graph.IsGroupNode(node) ||
          (node.Labels.Count == 2 && node.Labels[1].Text.Contains("convert"));

      var labelDropInputMode = editorInputMode.LabelDropInputMode;
      labelDropInputMode.Enabled = true;
      labelDropInputMode.AutoEditLabel = true;
      labelDropInputMode.UseBestMatchingParameter = true;
    }


    public DragAndDropWindow() {
      InitializeComponent();

      InitializeStylesList();

      // add the visual grid
      const int gridWidth = 80;
      GridInfo gridInfo = new GridInfo(gridWidth);

      var grid = new GridVisualCreator(gridInfo);
      graphControl.BackgroundGroup.AddChild(grid);

      // Create and configure a GraphSnapContext to enable snapping
      var context = new GraphSnapContext
      {
        NodeToNodeDistance = 30,
        NodeToEdgeDistance = 20,
        SnapOrthogonalMovement = false,
        SnapDistance = 10,
        SnapSegmentsToSnapLines = true,
        NodeGridConstraintProvider = new GridConstraintProvider<INode>(gridInfo),
        BendGridConstraintProvider = new GridConstraintProvider<IBend>(gridInfo),
        SnapBendsToSnapLines = true,
        GridSnapType = GridSnapTypes.All
      };

      // Create and register a graph editor input mode for editing the graph
      // in the canvas.
      var editorInputMode = new GraphEditorInputMode{AllowGroupingOperations = true};
      editorInputMode.SnapContext = context;
      editorInputMode.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();
      // use the style, size and labels of the currently selected palette node for newly created nodes
      editorInputMode.NodeCreator = GetNodeCreator(editorInputMode.NodeCreator);

      ConfigureNodeDropInputMode(editorInputMode);

      // use the mode in our control
      graphControl.InputMode = editorInputMode;

      // Enable undo
      graphControl.Graph.SetUndoEngineEnabled(true);
      
      DemoStyles.InitDemoStyles(graphControl.Graph);

      // populate the control with some nodes
      CreateSampleGraph();

      featuresComboBox.ItemsSource = functionOptions;
      featuresComboBox.SelectedIndex = 0;
    }

    private NodeCreationCallback GetNodeCreator(NodeCreationCallback nodeCreator) {
      return (context, graph, location, parent) => {
        INode paletteNode = styleListBox.SelectedItem as INode;
        if (paletteNode != null) {
          if (paletteNode.Tag == null || !paletteNode.Tag.ToString().EndsWith("Label Container")) {
            INode newNode = nodeCreator(context, graph, location, parent);
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
      // Create a group node in which the dragged node can be dropped
      var graph = graphControl.Graph;
      INode groupNode = graph.CreateGroupNode(null, new RectD(100, 100, 70, 70));
      graph.AddLabel(groupNode, "Group Node");
      graph.AddLabel(groupNode, "Drop a node over me!", ExteriorLabelModel.South);

      // Create a node to which the dragged node can snap
      INode node1 = graph.CreateNode(new RectD(300, 100, 30, 30));
      graph.AddLabel(node1, "Sample Node", ExteriorLabelModel.North);
      graph.AddLabel(node1, "Drag a node near me!", ExteriorLabelModel.South);

      // Create a node which can be converted to a group node automatically, if a node is dropped onto it
      INode node2 = graph.CreateNode(new RectD(450, 200, 30, 30), DemoStyles.CreateDemoNodeStyle(Themes.PaletteGreen));
      graph.AddLabel(node2, "Sample Node", ExteriorLabelModel.North);
      graph.AddLabel(node2, "Drag a node onto me to convert me to a group node!", ExteriorLabelModel.South);
    }

    #region Standard Actions

    /// <summary>
    /// Callback action that is triggered when the user exits the application.
    /// </summary>
    protected virtual void ExitMenuItemClick(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion

    #region Initialize node palette

    /// <summary>
    /// Initializes the style panel of this demo.
    /// </summary>
    private void InitializeStylesList() {
      const int nodeWidth = 60;
      const int nodeHeight = 40;

      // Create a new Graph in which the palette nodes live
      IGraph nodeContainer = new DefaultGraph();
      DemoStyles.InitDemoStyles(nodeContainer);
      
      var defaultLabelStyle = new DefaultLabelStyle{
          BackgroundPen = (Pen) new Pen(new SolidColorBrush(Color.FromRgb(101, 152, 204)), 1).GetAsFrozen(),
          BackgroundBrush = Brushes.White,
          Insets = new InsetsD(3, 5, 3, 5)
      };

      nodeContainer.NodeDefaults.Labels.Style = defaultLabelStyle;
      nodeContainer.EdgeDefaults.Labels.Style = defaultLabelStyle;
      
      // Create some nodes
      nodeContainer.CreateNode(new RectD(0, 0, nodeWidth, nodeHeight), DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Rectangle));
      nodeContainer.CreateNode(new RectD(0, 0, nodeWidth, nodeHeight));

      INode node = nodeContainer.CreateGroupNode(layout:new RectD(0, 0, 70, 70));
      nodeContainer.AddLabel(node, "Group Node");

      var nodeLabelContainer = nodeContainer.CreateNode(new RectD(0, 0, 70, 70), VoidNodeStyle.Instance, "Node Label Container");
      var nodeLabel = nodeContainer.AddLabel(nodeLabelContainer, "Node Label", InteriorLabelModel.Center);

      var edgeLabelContainer = nodeContainer.CreateNode(new RectD(0, 0, 70, 70), VoidNodeStyle.Instance, "Edge Label Container");
      var edgeLabelTemplate = nodeContainer.AddLabel(edgeLabelContainer, "Edge Label", FreeNodeLabelModel.Instance.CreateDefaultParameter());

      // Add nodes to listview
      foreach (INode n in nodeContainer.Nodes) {
        styleListBox.Items.Add(n);
      }
    }

    #endregion

    #region Combobox events

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

    #endregion

    private void OnTemplateMouseDown(object sender, RoutedEventArgs e) {
      // Initialize the information for a drag&drop from the node style list.
      FrameworkElement o = e.OriginalSource as FrameworkElement;
      if (o != null) {
        var node = o.DataContext as INode;
        if (node != null) {
          if ("Node Label Container".Equals(node.Tag)) {
            DataObject dao = new DataObject();
            dao.SetData(typeof(ILabel), node.Labels.First());
            DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          }
          else if ("Edge Label Container".Equals(node.Tag)) {
            var labelTemplate = node.Labels.First();
            //Not all label models return a valid geometry when the path is empty
            var p1 = new SimplePort(new SimpleNode() {Layout = new RectD(PointD.Origin, new Size(1,1))}, FreeNodePortLocationModel.NodeCenterAnchored);
            var p2 = new SimplePort(new SimpleNode(){Layout = new RectD(new PointD(0, 100), new Size(1,1))}, FreeNodePortLocationModel.NodeCenterAnchored);
            var edge = new SimpleEdge(p1, p2);
            var dummyLabel = new SimpleLabel(edge, labelTemplate.Text,
              FreeEdgeLabelModel.Instance.CreateDefaultParameter()) {
              Style = labelTemplate.Style,
              Tag = labelTemplate.Tag,
              PreferredSize = labelTemplate.PreferredSize
            };

            DataObject dao = new DataObject();
            dao.SetData(typeof(ILabel), dummyLabel);
            DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          } else {
            // use as defaults for newly created nodes
            graphControl.Graph.NodeDefaults.Style = node.Style;
            graphControl.Graph.NodeDefaults.Size = node.Layout.ToSizeD();

            DataObject dao = new DataObject();
            dao.SetData(typeof(INode), node);
            DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
          }
        }
      }
    }
  }

  /// <summary>
  /// Convert from a node style to an image that shows the visual representation of the style
  /// </summary>
  [ValueConversion(typeof(INode), typeof(DrawingImage))]
  public class NodeImageConverter : IValueConverter
  {
    private readonly GraphControl graphControl = new GraphControl();
    private SizeD size = new SizeD(40, 40);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      INode listBoxNode = value as INode;
      if (listBoxNode == null) {
        return null;
      }
      graphControl.Graph.Clear();

      var node = graphControl.Graph.CreateNode(listBoxNode.Layout.ToRectD(), listBoxNode.Style, listBoxNode.Tag);
      foreach (var label in listBoxNode.Labels) {
        graphControl.Graph.AddLabel(node, label.Text, label.LayoutParameter,
            label.Style, label.PreferredSize, label.Tag);
      }
      graphControl.FitGraphBounds();
      ContextConfigurator cc = new ContextConfigurator(graphControl.ContentRect);
      cc.Scale = Math.Min(cc.CalculateScaleForWidth(size.Width), cc.CalculateScaleForHeight(size.Height));

      var renderContext = cc.CreateRenderContext(graphControl);
      Transform transform = cc.CreateWorldToIntermediateTransform();
      Geometry clip = cc.CreateClip();

      var visualContent = graphControl.ExportContent(renderContext);
      VisualGroup container = new VisualGroup
      {
        Transform = transform,
        Clip = clip,
        Children = { visualContent }
      };
      VisualBrush brush = new VisualBrush(container);
      return
          new DrawingImage(new GeometryDrawing(brush, null,
              new RectangleGeometry(new Rect(0, 0, size.Width, size.Height))));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
