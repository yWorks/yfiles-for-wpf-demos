/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.RenderPolicies
{
  /// <summary>
  /// This demo shows the effect of different render policies on the z-order of nodes, edges, labels and ports.
  /// </summary>
  public partial class RenderPoliciesWindow
  {
    /// <summary>
    /// Initializes the components of this demo.
    /// </summary>
    public RenderPoliciesWindow() {
      InitializeComponent();
    }

    /// <summary>
    /// Initializes the application.
    /// </summary>
    protected virtual void OnLoaded(object src, RoutedEventArgs e) {
      // Initialize:
      // ... the input mode
      graphControl.InputMode = new GraphEditorInputMode { AllowGroupingOperations = true };

      // ...the defaults for new graph items
      SetGraphDefaults();

      // ... the default layer policies for labels and ports
      graphControl.GraphModelManager.LabelLayerPolicy = LabelLayerPolicy.SeparateLayer;
      graphControl.GraphModelManager.PortLayerPolicy = PortLayerPolicy.SeparateLayer;
      graphControl.GraphModelManager.HierarchicNestingPolicy = HierarchicNestingPolicy.NodesAndEdges;
      graphControl.GraphModelManager.EdgeGroup.Below(graphControl.GraphModelManager.NodeGroup);

      // ... the undo engine
      graphControl.Graph.SetUndoEngineEnabled(true);

      // load the sample
      CreateGraph();
    }

    /// <summary>
    /// Switches between pre-defined rendering order settings for common use cases.
    /// </summary>
    /// <remarks>
    /// The settings may also be combined in different ways.
    /// </remarks>
    private void RenderingOrderBoxChanged(object sender, SelectionChangedEventArgs e) {
      if (graphControl == null) {
        return;
      }

      var selectedItem = renderingOrderBox.SelectedIndex;

      // set to default first
      graphControl.GraphModelManager.LabelLayerPolicy = LabelLayerPolicy.SeparateLayer;
      graphControl.GraphModelManager.PortLayerPolicy = PortLayerPolicy.SeparateLayer;
      graphControl.GraphModelManager.HierarchicNestingPolicy = HierarchicNestingPolicy.NodesAndEdges;
      graphControl.GraphModelManager.EdgeGroup.Below(graphControl.GraphModelManager.NodeGroup);

      switch (selectedItem) {
        case 0:
          // default, do nothing
          break;
        case 1:
          // at owner
          graphControl.GraphModelManager.LabelLayerPolicy = LabelLayerPolicy.AtOwner;
          graphControl.GraphModelManager.PortLayerPolicy = PortLayerPolicy.AtOwner;
          break;
        case 2:
          // edges on top
          graphControl.GraphModelManager.LabelLayerPolicy = LabelLayerPolicy.AtOwner;
          graphControl.GraphModelManager.PortLayerPolicy = PortLayerPolicy.AtOwner;
          graphControl.GraphModelManager.HierarchicNestingPolicy = HierarchicNestingPolicy.Nodes;
          graphControl.GraphModelManager.EdgeGroup.Above(graphControl.GraphModelManager.NodeGroup);
          break;
        case 3:
          // group nodes
          graphControl.GraphModelManager.HierarchicNestingPolicy = HierarchicNestingPolicy.GroupNodes;
          break;
        case 4:
          // none
          graphControl.GraphModelManager.HierarchicNestingPolicy = HierarchicNestingPolicy.None;
          break;
      }
    }

    /// <summary>
    /// Initializes the node, label and port defaults used in this demo.
    /// </summary>
    private void SetGraphDefaults() {
      var graph = graphControl.Graph;

      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Pen = Pens.White,
          Brush = (Brush) new SolidColorBrush(Color.FromArgb(238, 255, 140, 0)).GetAsFrozen()
      };
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.Style = new DefaultLabelStyle {
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrapping = TextWrapping.Wrap
      };
      graph.NodeDefaults.Labels.LayoutParameter = InteriorLabelModel.Center;
      graph.NodeDefaults.Ports.Style = new NodeStylePortStyleAdapter(new ShapeNodeStyle {
          Brush = Brushes.DarkBlue,
          Pen = Pens.DarkBlue,
          Shape = ShapeNodeShape.Ellipse
      });
      graph.GroupNodeDefaults.Style = new PanelNodeStyle {
          Color = Color.FromArgb(229, 214, 229, 248),
          Insets = new InsetsD(18, 5, 5, 5),
          LabelInsetsColor = Color.FromRgb(214, 229, 248)
      };
      graph.GroupNodeDefaults.Labels.Style = new DefaultLabelStyle {
          TextAlignment = TextAlignment.Center
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter = InteriorStretchLabelModel.North;
      graph.EdgeDefaults.Labels.LayoutParameter = new SmartEdgeLabelModel().CreateParameterFromSource(0, 5, 0);
      graph.EdgeDefaults.Labels.Style = new DefaultLabelStyle {
          BackgroundBrush = Brushes.White,
          BackgroundPen = Pens.LightGray
      };
    }

    /// <summary>
    /// Loads the sample graph for this demo.
    /// </summary>
    /// <remarks>
    /// It contains several different demo cases so that overlapping can be tested for different layer policies.
    /// </remarks>
    private void CreateGraph() {
      CreateOverlappingLabelSample(new PointD(-290, 0));
      CreateOverlappingNodeSample(new PointD(10, 0));
      CreateOverlappingEdgeSample(new PointD(370, 0));
      CreateNestedGroupSample(new PointD(800, 0));

      graphControl.FitGraphBounds();
      graphControl.Graph.GetUndoEngine().Clear();
    }

    /// <summary>
    /// creates a graph example with a nested group
    /// </summary>
    private void CreateNestedGroupSample(PointD origin) {
      var graph = graphControl.Graph;

      var root = graph.CreateGroupNode(null, new RectD(origin.X, origin.Y, 230, 220));
      graph.AddLabel(root, "Outer Group Node");

      var outerChild1 = graph.CreateNode(new RectD(origin.X + 145, origin.Y + 30, 50, 50));
      graph.AddLabel(outerChild1, "Outer\nChild");
      graph.SetParent(outerChild1, root);

      var outerChild2 = graph.CreateNode(new RectD(origin.X + 40, origin.Y + 140, 50, 50));
      graph.AddLabel(outerChild2, "Outer\nChild");
      graph.SetParent(outerChild2, root);

      var edge1 = graph.CreateEdge(outerChild1, outerChild2);
      graph.AddBend(edge1, new PointD(origin.X + 65, origin.Y + 55));

      var childGroup = graph.CreateGroupNode(root, new RectD(origin.X + 20, origin.Y + 50, 150, 150));
      graph.AddLabel(childGroup, "Inner Group Node");

      var innerNode1 = graph.CreateNode(childGroup, new RectD(origin.X + 40, origin.Y + 80, 50, 50));
      graph.AddLabel(innerNode1, "Inner\nChild");

      var innerNode2 = graph.CreateNode(childGroup, new RectD(origin.X + 100, origin.Y + 140, 50, 50));
      graph.AddLabel(innerNode2, "Inner\nChild");

      var edge2 = graph.CreateEdge(innerNode1, innerNode2);
      graph.AddBend(edge2, new PointD(origin.X + 125, origin.Y + 105));

      AddBorderVisual(new RectD(origin.X - 20, origin.Y - 20, 280, 250), "Try different settings");
    }

    /// <summary>
    /// creates a graph example with overlapping edges
    /// </summary>
    private void CreateOverlappingEdgeSample(PointD origin) {
      var graph = graphControl.Graph;

      var source = graph.CreateNode(new RectD(origin.X, origin.Y + 60, 50, 50));
      var target1 = graph.CreateNode(new RectD(origin.X + 250, origin.Y + 60, 50, 50));
      var target2 = graph.CreateNode(new RectD(origin.X + 122.5, origin.Y + 130, 50, 50));
      var groupNode = graph.CreateGroupNode(null, new RectD(origin.X + 85, origin.Y, 125, 200));
      graph.AddLabel(groupNode, "Group Node");
      graph.SetParent(target2, groupNode);
      var edge1 = graph.CreateEdge(source, target1);
      graph.AddLabel(edge1, "Edge Label");
      var edge2 = graph.CreateEdge(source, target2);
      graph.AddBend(edge2, new PointD(origin.X + 25, origin.Y + 155));

      AddBorderVisual(new RectD(origin.X - 20, origin.Y - 20, 340, 250), "Try 'Edges on top' or 'Group Nodes'");
    }

    /// <summary>
    /// creates a graph example with overlapping nodes
    /// </summary>
    private void CreateOverlappingNodeSample(PointD origin) {
      var graph = graphControl.Graph;

      // overlapping nodes
      var back1 = graph.CreateNode(new RectD(origin.X, origin.Y + 20, 50, 50), graph.NodeDefaults.Style);
      graph.AddLabel(back1, "Back");

      var middle1 = graph.CreateNode(new RectD(origin.X + 20, origin.Y + 35, 50, 50), graph.NodeDefaults.Style);
      graph.AddLabel(middle1, "Middle");

      var front1 = graph.CreateNode(new RectD(origin.X + 40, origin.Y + 50, 50, 50), graph.NodeDefaults.Style);
      graph.AddLabel(front1, "Front");

      // overlapping nodes with ports
      var back2 = graph.CreateNode(new RectD(origin.X + 120, origin.Y + 20, 50, 50), graph.NodeDefaults.Style);
      var middle2 =
          graph.CreateNode(new RectD(origin.X + 140, origin.Y + 35, 50, 50), graph.NodeDefaults.Style);
      var front2 = graph.CreateNode(new RectD(origin.X + 160, origin.Y + 50, 50, 50), graph.NodeDefaults.Style);

      var nodeList = new List<INode> { back2, middle2, front2 };

      foreach (var node in nodeList) {
        graph.AddPort(node, FreeNodePortLocationModel.NodeBottomAnchored);
        graph.AddPort(node, FreeNodePortLocationModel.NodeTopAnchored);
        graph.AddPort(node, FreeNodePortLocationModel.NodeLeftAnchored);
        graph.AddPort(node, FreeNodePortLocationModel.NodeRightAnchored);
      }

      var edge1 = graph.CreateEdge(back1, back2);
      graph.AddBend(edge1, new PointD(origin.X + 25, origin.Y + 185));
      graph.AddBend(edge1, new PointD(origin.X + 145, origin.Y + 185));
      graph.AddLabel(edge1, "Edge Label", new SmartEdgeLabelModel().CreateParameterFromSource(1));
      graph.SetRelativePortLocation(edge1.SourcePort, new PointD(0, 25));
      var edge2 = graph.CreateEdge(front1, front2);
      graph.AddBend(edge2, new PointD(origin.X + 65, origin.Y + 190));
      graph.AddBend(edge2, new PointD(origin.X + 185, origin.Y + 190));
      graph.SetRelativePortLocation(edge2.SourcePort, new PointD(0, 25));

      AddBorderVisual(new RectD(origin.X - 50, origin.Y - 20, 310, 250), "Try 'Labels/Ports At Owner'");
    }

    /// <summary>
    /// creates a graph example with overlapping labels
    /// </summary>
    private void CreateOverlappingLabelSample(PointD origin) {
      var graph = graphControl.Graph;

      var firstNode = graph.CreateNode(new RectD(origin.X, origin.Y + 50, 50, 50), graph.NodeDefaults.Style);
      var secondNode =
          graph.CreateNode(new RectD(origin.X + 60, origin.Y + 80, 50, 50), graph.NodeDefaults.Style);

      graph.AddLabel(firstNode, "External Node Label 1", ExteriorLabelModel.South);
      graph.AddLabel(secondNode, "External Node Label 2", ExteriorLabelModel.South);

      AddBorderVisual(new RectD(origin.X - 50, origin.Y - 20, 210, 250), "Try 'Default'");
    }

    /// <summary>
    /// Adds a border with the given size and description.
    /// </summary>
    /// <remarks>Used to describe the different portions of the graph.</remarks>
    private void AddBorderVisual(RectD bounds, string text) {
      var rect = new SimpleNode {
          Layout = bounds,
          Style = new ShapeNodeStyle {
              Shape = ShapeNodeShape.Rectangle,
              Brush = null,
              Pen = new Pen(Brushes.Gray, 4) {
                  DashStyle = DashStyles.Dot
              }
          }
      };
      var label = new SimpleLabel(rect, text, ExteriorLabelModel.North) {
          Style = new DefaultLabelStyle {
              TextBrush = Brushes.Gray,
              TextAlignment = TextAlignment.Center,
              TextSize = 18,
              Typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold,
                  FontStretches.Normal)
          },
          PreferredSize = new SizeD(bounds.Width, 30)
      };
      graphControl.BackgroundGroup.AddChild(rect, GraphModelManager.DefaultNodeDescriptor);
      graphControl.BackgroundGroup.AddChild(label, GraphModelManager.DefaultLabelDescriptor);
    }

    /// <summary>
    /// Reset the graph to its initial state.
    /// </summary>
    private void ResetGraph(object sender, RoutedEventArgs e) {
      graphControl.Graph.Clear();
      CreateGraph();
    }
  }
}