/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.TextWrapping
{
  /// <summary>
  /// Shows different wrapping, trimming and clipping options that are available for <see cref="DefaultLabelStyle"/>.
  /// </summary>
  public partial class TextWrappingWindow
  {
    /// <summary>
    /// Initializes the demo.
    /// </summary>
    public TextWrappingWindow() {
      InitializeComponent();

      // initialize the settings used when creating new graph items
      InitializeGraphDefaults();

      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputMode();
    }

    /// <summary>
    /// Sets up defaults used when creating new graph items.
    /// </summary>
    private void InitializeGraphDefaults() {
      DemoStyles.InitDemoStyles(graphControl.Graph);

      var nodeDefaults = graphControl.Graph.NodeDefaults;
      nodeDefaults.Size = new Size(100, 80);

      // Use a label model that stretches the label over the full node layout, with small insets. The label style
      // is responsible for drawing the label in the given space. Depending on its implementation, it can either
      // ignore the given space, clip the label at the width or wrapping the text.
      // See the InitializeGraph function where labels are added with different style options.
      var centerLabelModel = new InteriorStretchLabelModel { Insets = new InsetsD(5) };
      nodeDefaults.Labels.LayoutParameter = centerLabelModel.CreateParameter(
          InteriorStretchLabelModel.Position.Center
      );

      var edgeDefaults = graphControl.Graph.EdgeDefaults;
      edgeDefaults.Labels.LayoutParameter =
          new EdgePathLabelModel { Distance = 5, AutoRotation = true }.CreateRatioParameter(0.5, EdgeSides.BelowEdge);
    }

    /// <summary>
    /// Initialize the graph with several nodes with labels using different text wrapping settings.
    /// </summary>
    private void InitializeGraph() {
      // label model and style for the description labels north of the nodes
      var northLabelModel = new ExteriorLabelModel { Insets = new InsetsD(10) };
      var northParameter = northLabelModel.CreateParameter(ExteriorLabelModel.Position.North);
      var northLabelStyle = new DefaultLabelStyle { TextAlignment = TextAlignment.Center };
      var graph = graphControl.Graph;
      var defaultNodeStyle = graph.NodeDefaults.Style as RectangleNodeStyle;

      // create nodes
      var node1 = graph.CreateNode(new RectD(0, -450, 190, 200));
      var node2 = graph.CreateNode(new RectD(0, -150, 190, 200));
      var node3 = graph.CreateNode(new RectD(0, 150, 190, 200));
      var node4 = graph.CreateNode(new RectD(250, -150, 190, 200));
      var node5 = graph.CreateNode(new RectD(250, 250, 190, 200));
      var node6 = graph.CreateNode(new RectD(500, -150, 190, 200));
      var node7 = graph.CreateNode(new RectD(500, 150, 190, 200));
      var node8 = graph.CreateNode(
          new RectD(750, -150, 190, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Hexagon, Brush = defaultNodeStyle.Brush, Pen = defaultNodeStyle.Pen
          }
      );
      var node9 = graph.CreateNode(
          new RectD(750, 150, 190, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Triangle2, Brush = defaultNodeStyle.Brush, Pen = defaultNodeStyle.Pen
          });
      var node10 = graph.CreateNode(
          new RectD(1000, -150, 190, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Octagon, Brush = defaultNodeStyle.Brush, Pen = defaultNodeStyle.Pen
          });
      var node11 = graph.CreateNode(
          new RectD(1000, 150, 190, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Ellipse, Brush = defaultNodeStyle.Brush, Pen = defaultNodeStyle.Pen
          });

      // use a label model that stretches the label over the full node layout, with small insets
      var centerLabelModel = new InteriorStretchLabelModel { Insets = new InsetsD(5) };
      var centerParameter = centerLabelModel.CreateParameter(InteriorStretchLabelModel.Position.Center);

      // A label that does not wrap or clip at all. As the ClipText property is set to false and no wrapping and
      // trimming is used, the label extends the bounds of its owner node.
      var noWrapNoTrimNoClipStyle = new DefaultLabelStyle {
          TextSize = 16,
          ClipText = false,
          TextWrapping = System.Windows.TextWrapping.NoWrap,
          TextTrimming = TextTrimming.None,
          VerticalTextAlignment = VerticalAlignment.Center,
      };
      graph.AddLabel(node1, Text, centerParameter, noWrapNoTrimNoClipStyle);
      graph.AddLabel(node1, "No Wrapping\nNo Trimming\nNo Clipping", northParameter, northLabelStyle);

      // A label that does not wrap or clip at all. By default, ClipText is true, so it is clipped at the given bounds.
      var noWrapNoTrimStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextWrapping = System.Windows.TextWrapping.NoWrap,
          TextTrimming = TextTrimming.None,
          VerticalTextAlignment = VerticalAlignment.Center,
      };
      graph.AddLabel(node2, Text, centerParameter, noWrapNoTrimStyle);
      graph.AddLabel(node2, "No Wrapping\nNo Trimming\nClipping", northParameter, northLabelStyle);

      // A label that is not wrapped but trimmed with ellipsis at the given bounds if there is not enough space.
      var noWrapCharTrimStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextWrapping = System.Windows.TextWrapping.NoWrap,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
      };
      graph.AddLabel(node3, Text, centerParameter, noWrapCharTrimStyle);
      graph.AddLabel(node3, "No Wrapping\nCharacter Trimming\nClipping", northParameter, northLabelStyle);

      // A label that is wrapped at word boundaries but not trimmed. If there is not enough space, the wrapped lines
      // are placed according to the chosen vertical alignment. With 'VerticalAlignment.Center' the top and bottom part
      // of the label are clipped.
      var wrapNoTrimStyle = new DefaultLabelStyle {
          TextSize = 16, TextTrimming = TextTrimming.None, VerticalTextAlignment = VerticalAlignment.Center,
      };
      graph.AddLabel(node4, Text, centerParameter, wrapNoTrimStyle);
      graph.AddLabel(node4, "Wrapping\nNoTrimming\nClipping", northParameter, northLabelStyle);

      // A label that is wrapped at word boundaries but not trimmed or clipped.
      // Due to the label exceeding the node bounds vertically, we place the description label even further north 
      var furtherNorthLabelModel = new ExteriorLabelModel { Insets = new InsetsD(110) };
      var furtherNorthParameter = furtherNorthLabelModel.CreateParameter(ExteriorLabelModel.Position.North);
      var wrapNoTrimNoClipStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.None,
          ClipText = false,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node5, Text, centerParameter, wrapNoTrimNoClipStyle);
      graph.AddLabel(node5, "Wrapping\nNoTrimming\nNoClipping", furtherNorthParameter, northLabelStyle);

      // A label that is wrapped and trimmed at characters at the end.
      var wrapCharTrimStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
      };
      graph.AddLabel(node7, Text, centerParameter, wrapCharTrimStyle);
      graph.AddLabel(node7, "Wrapping\nCharacter Trimming\nClipping", northParameter, northLabelStyle);

      // A label that is wrapped and trimmed at word boundaries.
      var wrapWordTrimStyle = new DefaultLabelStyle {
          TextSize = 16, TextTrimming = TextTrimming.WordEllipsis, VerticalTextAlignment = VerticalAlignment.Center,
      };
      graph.AddLabel(node6, Text, centerParameter, wrapWordTrimStyle);
      graph.AddLabel(node6, "Wrapping\nWord Trimming\nClipping", northParameter, northLabelStyle);

      // A label that is wrapped but uses a hexagon shape to fit the text inside.
      // The TextWrappingShape can be combined with the TextWrappingPadding that keeps empty paddings inside this shape.
      var wrapHexagonShapeStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.WordEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Hexagon,
          TextWrappingPadding = 5
      };
      graph.AddLabel(node8, Text, centerParameter, wrapHexagonShapeStyle);
      graph.AddLabel(node8, "Wrapping\nat Hexagon Shape\nWord Trimming", northParameter, northLabelStyle);

      // A label that is wrapped inside a triangular shape.
      var wrapTriangleShapeStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Triangle2,
          TextWrappingPadding = 5
      };
      graph.AddLabel(node9, Text, centerParameter, wrapTriangleShapeStyle);
      graph.AddLabel(node9, "Wrapping\nat Triangle Shape\nCharacterTrimming", northParameter, northLabelStyle);

      // A label that is wrapped inside an elliptic shape.
      // In addition to the TextWrappingPadding some insets are defined for the top and bottom side
      // to keep the upper and lower part of the ellipse empty.
      var wrapEllipseShapeStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Ellipse,
          TextWrappingPadding = 5,
          Insets = new InsetsD(0, 40, 0, 40)
      };
      graph.AddLabel(node11, Text, centerParameter, wrapEllipseShapeStyle);
      graph.AddLabel(
          node11,
          "Wrapping\nat Ellipse Shape\nwith Top/Bottom Insets\nCharacter Trimming",
          northParameter,
          northLabelStyle
      );

      // A label that is wrapped inside an octagon shape.
      // In addition to the TextWrappingPadding some insets are defined for the top and bottom side
      // to keep the upper and lower part of the octagon empty.
      var wrapOctagonShapeStyle = new DefaultLabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.WordEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Octagon,
          TextWrappingPadding = 5,
          Insets = new InsetsD(0, 40, 0, 40)
      };
      graph.AddLabel(node10, Text, centerParameter, wrapOctagonShapeStyle);
      graph.AddLabel(
          node10,
          "Wrapping\nat Octagon Shape\nwith Top/Bottom Insets\nWord Trimming",
          northParameter,
          northLabelStyle
      );
    }

    /// <summary>
    /// Allow all kind of user interactions including node resizing.
    /// </summary>
    private void InitializeInputMode() {
      graphControl.InputMode = new GraphEditorInputMode();
    }

    private const string Text =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\n" +
        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.\n\n" +
        "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.\n" +
        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

    /// <summary>
    /// Center the sample graph in the visible area.
    /// </summary>
    private void OnLoaded(object source, EventArgs e) {
      // center the sample graph in the visible area
      graphControl.FitGraphBounds();
    }
  }
}
