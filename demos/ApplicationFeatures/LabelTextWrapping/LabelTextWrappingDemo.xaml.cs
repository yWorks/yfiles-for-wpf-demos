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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LabelTextWrapping
{
  /// <summary>
  ///   Shows different wrapping, trimming, and clipping options that are available for <see cref="LabelStyle"/>.
  /// </summary>
  [Demo("label-text-wrapping", "application-features", "Label Text Wrapping",
      Summary = "Shows how to enable label text wrapping and trimming.", Tags = ["label"],
      Keywords = ["TextWrapping", "linebreaks", "characters", "words", "ellipsis", "padding", "margin"])]
  public partial class LabelTextWrappingDemo
  {
    [STAThread]
    public static void Main() {
      Run<LabelTextWrappingDemo>();
    }

    /// <summary>
    ///   Initializes the demo.
    /// </summary>
    public LabelTextWrappingDemo() {
      InitializeComponent();

      // initialize the settings used when creating new graph items
      InitializeGraphDefaults();

      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputMode();
    }

    /// <summary>
    ///   Sets up defaults used when creating new graph items.
    /// </summary>
    private void InitializeGraphDefaults() {
      DemoStyles.InitDemoStyles(graphControl.Graph);

      var nodeDefaults = graphControl.Graph.NodeDefaults;
      nodeDefaults.Size = new SizeD(100, 80);

      // Use a label model that stretches the label over the full node layout, with small padding. The label style
      // is responsible for drawing the label in the given space. Depending on its implementation, it can either
      // ignore the given space, clip the label at the width or wrapping the text.
      // See the InitializeGraph function where labels are added with different style options.
      var centerLabelModel = new StretchNodeLabelModel { Padding = new InsetsD(5) };
      nodeDefaults.Labels.LayoutParameter = centerLabelModel.CreateParameter(StretchNodeLabelModel.Position.Center);

      var edgeDefaults = graphControl.Graph.EdgeDefaults;
      edgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(0.5, EdgeSides.BelowEdge);
    }

    /// <summary>
    ///   Initialize the graph with several nodes with labels using different text wrapping settings.
    /// </summary>
    private void InitializeGraph() {
      // label model and style for the description labels
      var topLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(10) };
      var topParameter = topLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Top);
      var topLabelStyle = new LabelStyle { TextAlignment = TextAlignment.Center };
      var graph = graphControl.Graph;
      var defaultNodeStyle = (RectangleNodeStyle) graph.NodeDefaults.Style;

      // create nodes
      var node1 = graph.CreateNode(new RectD(0, -450, 190, 200));
      var node2 = graph.CreateNode(new RectD(0, -150, 190, 200));
      var node3 = graph.CreateNode(new RectD(0, 150, 190, 200));
      var node4 = graph.CreateNode(new RectD(250, -150, 190, 200));
      var node5 = graph.CreateNode(new RectD(250, 250, 190, 200));
      var node6 = graph.CreateNode(new RectD(500, -150, 190, 200));
      var node7 = graph.CreateNode(new RectD(500, 150, 190, 200));
      var node8 = graph.CreateNode(new RectD(750, -150, 190, 200), new ShapeNodeStyle {
          Shape = ShapeNodeShape.Hexagon,
          Brush = defaultNodeStyle.Brush,
          Pen = defaultNodeStyle.Pen
      });
      var node9 = graph.CreateNode(new RectD(750, 150, 190, 200), new ShapeNodeStyle {
          Shape = ShapeNodeShape.Triangle,
          Brush = defaultNodeStyle.Brush,
          Pen = defaultNodeStyle.Pen
      });
      var node10 = graph.CreateNode(new RectD(1000, -150, 190, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Octagon,
              Brush = defaultNodeStyle.Brush,
              Pen = defaultNodeStyle.Pen
          });
      var node11 = graph.CreateNode(new RectD(1000, 150, 190, 200), new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = defaultNodeStyle.Brush,
          Pen = defaultNodeStyle.Pen
      });

      var node12 = graph.CreateNode(new RectD(1250, 150, 190, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Pentagon,
              Brush = defaultNodeStyle.Brush,
              Pen = defaultNodeStyle.Pen
          });
      var node13 = graph.CreateNode(new RectD(1500, 150, 321, 200),
          new ShapeNodeStyle {
              Shape = ShapeNodeShape.Squircle,
              Brush = defaultNodeStyle.Brush,
              Pen = defaultNodeStyle.Pen
          });

      // use a label model that stretches the label over the full node layout, with small padding
      var centerLabelModel = new StretchNodeLabelModel();
      var centerParameter = centerLabelModel.CreateParameter(StretchNodeLabelModel.Position.Center);
      var centerLabelModelWithPadding = new StretchNodeLabelModel { Padding = new InsetsD(5) };
      var centerParameterWithPadding =
          centerLabelModelWithPadding.CreateParameter(StretchNodeLabelModel.Position.Center);

      // A label that does not wrap or clip at all. As the ClipText property is set to false and no wrapping and
      // trimming is used, the label extends the bounds of its owner node.
      var noWrapNoTrimNoClipStyle = new LabelStyle {
          TextSize = 16,
          ClipText = false,
          TextWrapping = TextWrapping.NoWrap,
          TextTrimming = TextTrimming.None,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node1, Text, centerParameterWithPadding, noWrapNoTrimNoClipStyle);
      graph.AddLabel(node1, "No Wrapping\nNo Trimming\nNo Clipping", topParameter, topLabelStyle);

      // A label that does not wrap or clip at all. By default, ClipText is true, so it is clipped at the given bounds.
      var noWrapNoTrimStyle = new LabelStyle {
          TextSize = 16,
          TextWrapping = TextWrapping.NoWrap,
          TextTrimming = TextTrimming.None,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node2, Text, centerParameterWithPadding, noWrapNoTrimStyle);
      graph.AddLabel(node2, "No Wrapping\nNo Trimming\nClipping", topParameter, topLabelStyle);

      // A label that is not wrapped but trimmed with ellipsis at the given bounds if there is not enough space.
      var noWrapCharTrimStyle = new LabelStyle {
          TextSize = 16,
          TextWrapping = TextWrapping.NoWrap,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node3, Text, centerParameterWithPadding, noWrapCharTrimStyle);
      graph.AddLabel(node3, "No Wrapping\nCharacter Trimming\nClipping", topParameter, topLabelStyle);

      // A label that is wrapped at word boundaries but not trimmed. If there is not enough space, the wrapped lines
      // are placed according to the chosen vertical alignment. With 'VerticalAlignment.Center' the top and bottom part
      // of the label are clipped.
      var wrapNoTrimStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.None,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node4, Text, centerParameterWithPadding, wrapNoTrimStyle);
      graph.AddLabel(node4, "Wrapping\nNoTrimming\nClipping", topParameter, topLabelStyle);

      // A label that is wrapped at word boundaries but not trimmed or clipped.
      // Due to the label exceeding the node bounds vertically, we place the description label further away from the node
      var furtherTopLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(110) };
      var furtherTopParameter = furtherTopLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Top);
      var wrapNoTrimNoClipStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.None,
          ClipText = false,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node5, Text, centerParameterWithPadding, wrapNoTrimNoClipStyle);
      graph.AddLabel(node5, "Wrapping\nNoTrimming\nNoClipping", furtherTopParameter, topLabelStyle);

      // A label that is wrapped and trimmed at characters at the end.
      var wrapCharTrimStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node7, Text, centerParameterWithPadding, wrapCharTrimStyle);
      graph.AddLabel(node7, "Wrapping\nCharacter Trimming\nClipping", topParameter, topLabelStyle);

      // A label that is wrapped and trimmed at word boundaries.
      var wrapWordTrimStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.WordEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      graph.AddLabel(node6, Text, centerParameterWithPadding, wrapWordTrimStyle);
      graph.AddLabel(node6, "Wrapping\nWord Trimming\nClipping", topParameter, topLabelStyle);

      // A label that is wrapped but uses a hexagon shape to fit the text inside.
      // The TextWrappingShape can be combined with the TextWrappingPadding that keeps empty paddings inside this shape.
      var wrapHexagonShapeStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.WordEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Hexagon,
          TextWrappingPadding = 5
      };
      graph.AddLabel(node8, Text, centerParameter, wrapHexagonShapeStyle);
      graph.AddLabel(node8, "Wrapping\nat Hexagon Shape\nWord Trimming", topParameter, topLabelStyle);

      // A label that is wrapped inside a triangular shape.
      var wrapTriangleShapeStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Triangle,
          TextWrappingPadding = 5
      };
      graph.AddLabel(node9, Text, centerParameter, wrapTriangleShapeStyle);
      graph.AddLabel(node9, "Wrapping\nat Triangle Shape\nCharacterTrimming", topParameter, topLabelStyle);

      // A label that is wrapped inside an elliptic shape.
      // In addition to the TextWrappingPadding some padding are defined for the top and bottom side
      // to keep the upper and lower part of the ellipse empty.
      var wrapEllipseShapeStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Ellipse,
          TextWrappingPadding = 5,
          Padding = new InsetsD(0, 40, 0, 40)
      };
      graph.AddLabel(node11, Text, centerParameter, wrapEllipseShapeStyle);
      graph.AddLabel(node11, "Wrapping\nat Ellipse Shape\nwith Top/Bottom Padding\nCharacter Trimming", topParameter,
          topLabelStyle);

      // A label that is wrapped inside a pentagon shape.
      var pentagonLabelStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Pentagon,
          TextWrappingPadding = 5
      };
      graph.AddLabel(node12, Text, centerParameter, pentagonLabelStyle);
      graph.AddLabel(node12, "Wrapping\nat Pentagon Shape\nCharacter Ellipsis Trimming", topParameter, topLabelStyle);
      var squircleLabelStyle = new LabelStyle {
          Shape = LabelShape.Squircle,
          TextSize = 16,
          TextTrimming = TextTrimming.CharacterEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Squircle,
          TextWrappingPadding = 5
      };
      graph.AddLabel(node13, Text, centerParameter, squircleLabelStyle);
      graph.AddLabel(node13, "Wrapping\nat Squircle Shape\nCharacter Ellipsis Trimming", topParameter, topLabelStyle);

      // A label that is wrapped inside an octagon shape.
      // In addition to the TextWrappingPadding some padding is defined for the top and bottom side
      // to keep the upper and lower part of the octagon empty.
      var wrapOctagonShapeStyle = new LabelStyle {
          TextSize = 16,
          TextTrimming = TextTrimming.WordEllipsis,
          VerticalTextAlignment = VerticalAlignment.Center,
          TextWrappingShape = TextWrappingShape.Octagon,
          TextWrappingPadding = 5,
          Padding = new InsetsD(0, 40, 0, 40)
      };
      graph.AddLabel(node10, Text, centerParameter, wrapOctagonShapeStyle);
      graph.AddLabel(node10, "Wrapping\nat Octagon Shape\nwith Top/Bottom Padding\nWord Trimming", topParameter,
          topLabelStyle);
    }

    /// <summary>
    ///   Allow all kind of user interactions including node resizing.
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
    ///   Center the sample graph in the visible area.
    /// </summary>
    protected override void Initialize() {
      // center the sample graph in the visible area
      graphControl.FitGraphBounds();
    }
  }
}
