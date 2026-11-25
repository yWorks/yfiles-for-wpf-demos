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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.LabelStyle
{
  /// <summary>
  ///   Shows the most important configuration options for the built-in <see cref="LabelStyle"/> class.
  /// </summary>
  [Demo("label-style", "style", "Label Style", Summary = "Shows the features of the LabelStyle class.",
      Keywords = ["library style", "style options", "padding"], Tags = ["style", "labels"],
      RelatedDemos = ["tutorial-style-implementation-label-render-label-text"])]
  public sealed partial class LabelStyleDemo
  {
    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public LabelStyleDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      DemoStyles.InitDemoStyles(Graph);

      // Create node and edge labels using different label style settings
      CreateSampleNodeLabels(Graph);
      CreateSampleEdgeLabels(Graph);

      // Configure the interaction features and tool tips
      ConfigureInteraction(GraphControl);

      // Initialize the UI and show the demo
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates some sample node labels with different background styles.
    /// </summary>
    /// <param name="graph">The graph to add node labels to.</param>
    private static void CreateSampleNodeLabels(IGraph graph) {
      var n1 = graph.CreateNode(new RectD(-25, -100, 50, 200), tag: true);
      // Add sample node labels to the first node, distributed evenly on the side and with different
      // background shapes
      graph.AddLabel(n1, "Rectangle Node Label", CreateNodeLabelParameter(new PointD(1, 0.1), new PointD(100, 0)),
          CreateNodeLabelStyle(shape: LabelShape.Rectangle));
      graph.AddLabel(n1, "Rounded Node Label", CreateNodeLabelParameter(new PointD(1, 0.3), new PointD(100, 0)),
          CreateNodeLabelStyle(shape: LabelShape.RoundRectangle, padding: new InsetsD(2)));
      graph.AddLabel(n1, "Hexagon Node Label", CreateNodeLabelParameter(new PointD(1, 0.5), new PointD(100, 0)),
          // The hexagon background needs slightly larger insets at the sides
          CreateNodeLabelStyle(shape: LabelShape.Hexagon));
      graph.AddLabel(n1, "Pill Node Label", CreateNodeLabelParameter(new PointD(1, 0.7), new PointD(100, 0)),
          CreateNodeLabelStyle(shape: LabelShape.Pill));
      graph.AddLabel(n1, "Squircle Node Label", CreateNodeLabelParameter(new PointD(1, 0.9), new PointD(100, 0)),
          CreateNodeLabelStyle(shape: LabelShape.Squircle));

      // Create two more nodes, the bottom one and the right one
      var n2 = graph.CreateNode(new RectD(375, 700, 50, 50));
      graph.SetStyle(n2, DemoStyles.CreateDemoNodeStyle(Themes.Palette14));
      var n3 = graph.CreateNode(new RectD(525, -100, 50, 200));
      graph.SetStyle(n3, DemoStyles.CreateDemoNodeStyle(Themes.Palette12));

      // Add three node labels to the right node showing different text clipping and text wrapping options
      graph.AddLabel(n3, "Wrapped and clipped label text",
          CreateNodeLabelParameter(new PointD(1, 0.2), new PointD(120, 0)),
          CreateNodeLabelStyle(Themes.Palette12, LabelShape.Pill, wrapping: TextWrapping.Wrap,
              textTrimming: TextTrimming.WordEllipsis), new SizeD(140, 25));

      graph.AddLabel(n3, "Un-wrapped but clipped label text",
          CreateNodeLabelParameter(new PointD(1, 0.5), new PointD(120, 0)),
          CreateNodeLabelStyle(Themes.Palette12, LabelShape.Pill, wrapping: TextWrapping.NoWrap), new SizeD(140, 25));

      // For the last label, disable text clipping
      graph.AddLabel(n3, "Un-wrapped and un-clipped label text",
          CreateNodeLabelParameter(new PointD(1, 0.8), new PointD(120, 0)),
          CreateNodeLabelStyle(Themes.Palette12, LabelShape.Pill, wrapping: TextWrapping.NoWrap, clipText: false),
          new SizeD(140, 25));
    }

    /// <summary>
    ///   Creates some sample edge labels with different background styles.
    /// </summary>
    /// <param name="graph">The graph to add edge labels to.</param>
    private static void CreateSampleEdgeLabels(IGraph graph) {
      var edgeLabelModel = new SmartEdgeLabelModel { Angle = Math.PI / 2 };

      graph.EdgeDefaults.Labels.LayoutParameter = edgeLabelModel.CreateParameterFromSource(0);

      var edge1 = graph.CreateEdge(graph.Nodes[0], graph.Nodes[1]);
      graph.AddBend(edge1, new PointD(0, 400));

      // Add sample edge labels on the first edge segment, distributed evenly on the path and with different
      // background shapes
      graph.AddLabel(edge1, "Rectangle Edge Label\nA second line of sample text.",
          edgeLabelModel.CreateParameterFromSource(0, 0, 0.1), CreateEdgeLabelStyle(shape: LabelShape.Rectangle));
      graph.AddLabel(edge1, "Rounded Edge Label\nA second line of sample text.",
          edgeLabelModel.CreateParameterFromSource(0, 0, 0.3),
          // For the round rectangle, we can manually increase the padding around the text
          // using the insets property. By default, this would be just as tight as for
          // LabelShape.RECTANGLE, but in order to make sure that text is less likely to touch
          // the stroke of the round rectangle, we add 2 extra pixels.
          CreateEdgeLabelStyle(shape: LabelShape.RoundRectangle, padding: new InsetsD(2)));
      graph.AddLabel(edge1, "Hexagon Edge Label\nA second line of sample text.",
          edgeLabelModel.CreateParameterFromSource(0), CreateEdgeLabelStyle(shape: LabelShape.Hexagon));
      graph.AddLabel(edge1, "Pill Edge Label\nA second line of sample text.",
          edgeLabelModel.CreateParameterFromSource(0, 0, 0.7), CreateEdgeLabelStyle(shape: LabelShape.Pill));
      graph.AddLabel(edge1, "Squircle Edge Label\nA second line of sample text.",
          edgeLabelModel.CreateParameterFromSource(0, 0, 0.9), CreateEdgeLabelStyle(shape: LabelShape.Squircle));

      // Add rotated edge labels on the second edge segment, distributed evenly and with different
      // background shapes
      graph.AddLabel(edge1, "Rotated Rectangle", edgeLabelModel.CreateParameterFromSource(1, 0, 0.35),
          CreateEdgeLabelStyle(Themes.Palette15, LabelShape.Rectangle, CreateTypeface("Monospace"), 16));
      graph.AddLabel(edge1, "Rotated Rounded Rectangle", edgeLabelModel.CreateParameterFromSource(1),
          CreateEdgeLabelStyle(Themes.Palette15, LabelShape.RoundRectangle,
              // For the round rectangle, we can manually increase the padding around the text
              // using the insets property. By default, this would be just as tight as for
              // LabelShape.RECTANGLE, but in order to make sure that text is less likely to touch
              // the stroke of the round rectangle, we add 2 extra pixels.
              padding: new InsetsD(2), typeface: CreateTypeface("Monospace"), textSize: 16));
      graph.AddLabel(edge1, "Rotated Hexagon", edgeLabelModel.CreateParameterFromSource(1, 0, 0.6),
          CreateEdgeLabelStyle(Themes.Palette15, LabelShape.Hexagon, CreateTypeface("Monospace"), 16));
      graph.AddLabel(edge1, "Rotated Pill", edgeLabelModel.CreateParameterFromSource(1, 0, 0.7),
          CreateEdgeLabelStyle(Themes.Palette15, LabelShape.Pill, CreateTypeface("Monospace"), 16));
      graph.AddLabel(edge1, "Rotated Squircle", edgeLabelModel.CreateParameterFromSource(1, 0, 0.85),
          CreateEdgeLabelStyle(Themes.Palette15, LabelShape.Squircle, CreateTypeface("Monospace"), 16));

      var edge2 = graph.CreateEdge(graph.Nodes[2], graph.Nodes[1]);
      graph.AddBend(edge2, new PointD(550, 725));
      graph.SetStyle(edge2, DemoStyles.CreateDemoEdgeStyle(Themes.Palette12));

      // Add larger edge labels with different vertical and horizontal text alignment settings to the second edge
      graph.AddLabel(edge2, "Edge Label\nwith vertical text\nalignment at bottom",
          edgeLabelModel.CreateParameterFromSource(0, -20, 0.2),
          CreateEdgeLabelStyle(Themes.Palette12, LabelShape.RoundRectangle, padding: new InsetsD(2),
              typeface: CreateTypeface("sans-serif", FontWeights.Bold), textSize: 14,
              verticalTextAlignment: VerticalAlignment.Bottom),
          // Explicitly specify a preferred size for the label that is much larger than needed for the label's text
          new SizeD(150, 120));
      graph.AddLabel(edge2, "Edge Label\nwith vertical text\nalignment at top",
          edgeLabelModel.CreateParameterFromSource(0, 20, 0.2),
          CreateEdgeLabelStyle(Themes.Palette12, LabelShape.RoundRectangle, padding: new InsetsD(2),
              typeface: CreateTypeface("sans-serif", FontWeights.Bold), textSize: 14,
              verticalTextAlignment: VerticalAlignment.Top),
          // Explicitly specify a preferred size for the label that is much larger than needed for the label's text
          new SizeD(150, 120));
      graph.AddLabel(edge2, "Edge Label\nwith vertical center\nand horizontal left\ntext alignment",
          edgeLabelModel.CreateParameterFromSource(0, 20),
          CreateEdgeLabelStyle(Themes.Palette12, LabelShape.RoundRectangle, padding: new InsetsD(2),
              typeface: CreateTypeface("sans-serif", FontWeights.Bold), textSize: 14,
              verticalTextAlignment: VerticalAlignment.Center, horizontalTextAlignment: TextAlignment.Left),
          // Explicitly specify a preferred size for the label that is much larger than needed for the label's text
          new SizeD(150, 120));
      graph.AddLabel(edge2, "Edge Label\nwith vertical bottom\nand horizontal right\ntext alignment",
          edgeLabelModel.CreateParameterFromSource(0, -20),
          CreateEdgeLabelStyle(Themes.Palette12, LabelShape.RoundRectangle, padding: new InsetsD(2),
              typeface: CreateTypeface("sans-serif", FontWeights.Bold), textSize: 14,
              verticalTextAlignment: VerticalAlignment.Bottom, horizontalTextAlignment: TextAlignment.Right),
          // Explicitly specify a preferred size for the label that is much larger than needed for the label's text
          new SizeD(150, 120));
    }

    /// <summary>
    ///   Creates and configures a node label style.
    /// </summary>
    /// <param name="theme">The color palette to use for the style's fills and stroke.</param>
    /// <param name="shape">The label shape for the background.</param>
    /// <param name="typeface">The typeface for the label text.</param>
    /// <param name="textSize">The size of the label text.</param>
    /// <param name="padding">Optional padding to account for special background shapes.</param>
    /// <param name="wrapping">The text wrapping mode.</param>
    /// <param name="clipText">Whether to clip text at the label bounds.</param>
    /// <param name="verticalTextAlignment">The vertical text alignment.</param>
    /// <param name="horizontalTextAlignment">The horizontal text alignment.</param>
    /// <param name="textTrimming">The text trimming behavior.</param>
    private static Graph.Styles.LabelStyle CreateNodeLabelStyle(
        Palette? theme = null,
        LabelShape shape = LabelShape.Rectangle,
        Typeface? typeface = null,
        double textSize = 14,
        InsetsD? padding = null,
        TextWrapping wrapping = TextWrapping.NoWrap,
        bool clipText = true,
        VerticalAlignment verticalTextAlignment = VerticalAlignment.Center,
        TextAlignment horizontalTextAlignment = TextAlignment.Center,
        TextTrimming textTrimming = TextTrimming.None) {
      theme ??= Themes.Palette13;
      return new Graph.Styles.LabelStyle {
          Shape = shape,
          BackgroundBrush = theme.NodeLabelFill,
          BackgroundPen = null,
          Typeface = typeface ?? CreateTypeface("Arial"),
          TextSize = textSize,
          TextBrush = theme.Text,
          Padding = padding ?? new InsetsD(0),
          TextWrapping = wrapping,
          TextTrimming = textTrimming,
          ClipText = clipText,
          VerticalTextAlignment = verticalTextAlignment,
          TextAlignment = horizontalTextAlignment
      };
    }

    /// <summary>
    ///   Creates and configures an edge label style.
    /// </summary>
    /// <param name="theme">The color palette to use for the style's fills and stroke.</param>
    /// <param name="shape">The label shape for the background.</param>
    /// <param name="typeface">The typeface for the label text.</param>
    /// <param name="textSize">The size of the label text.</param>
    /// <param name="padding">Optional padding to account for special background shapes.</param>
    /// <param name="wrapping">The text wrapping mode.</param>
    /// <param name="clipText">Whether to clip text at the label bounds.</param>
    /// <param name="verticalTextAlignment">The vertical text alignment.</param>
    /// <param name="horizontalTextAlignment">The horizontal text alignment.</param>
    /// <param name="textTrimming">The text trimming behavior.</param>
    private static Graph.Styles.LabelStyle CreateEdgeLabelStyle(
        Palette? theme = null,
        LabelShape shape = LabelShape.Rectangle,
        Typeface? typeface = null,
        double textSize = 14,
        InsetsD? padding = null,
        TextWrapping wrapping = TextWrapping.NoWrap,
        bool clipText = false,
        VerticalAlignment verticalTextAlignment = VerticalAlignment.Center,
        TextAlignment horizontalTextAlignment = TextAlignment.Center,
        TextTrimming textTrimming = TextTrimming.None) {
      theme ??= Themes.Palette13;
      return new Graph.Styles.LabelStyle {
          Shape = shape,
          BackgroundBrush = theme.EdgeLabelFill,
          BackgroundPen = null,
          Typeface = typeface ?? CreateTypeface("Arial"),
          TextSize = textSize,
          TextBrush = theme.Text,
          Padding = padding ?? new InsetsD(0),
          TextWrapping = wrapping,
          TextTrimming = textTrimming,
          ClipText = clipText,
          VerticalTextAlignment = verticalTextAlignment,
          TextAlignment = horizontalTextAlignment
      };
    }

    /// <summary>
    ///   Creates a node label at the specified vertical ratio.
    /// </summary>
    /// <param name="layoutRatio">The ratio that describes the point on the node's layout relative to its upper-left corner.</param>
    /// <param name="layoutOffset">The absolute offset to apply to the point on the node after the ratio has been determined.</param>
    private static ILabelModelParameter CreateNodeLabelParameter(PointD layoutRatio, PointD layoutOffset) =>
        FreeNodeLabelModel.Instance.CreateParameter(layoutRatio, layoutOffset, new PointD(0.5, 0.5));

    private static Typeface CreateTypeface(string family, FontWeight? weight = null) {
      var fontFamily = family switch {
          "Monospace" => "Consolas",
          "sans-serif" => "Arial",
          _ => family
      };
      return new Typeface(new FontFamily(fontFamily), FontStyles.Normal, weight ?? FontWeights.Normal,
          FontStretches.Normal);
    }

    /// <summary>
    ///   Configures the interaction behavior and the tooltips.
    ///   Since this demo is about the styles of the labels, not their placement, only interaction with
    ///   them is allowed.
    /// </summary>
    private static void ConfigureInteraction(GraphControl graphControl) {
      var inputMode = new GraphEditorInputMode {
          AllowCreateNode = false,
          AllowCreateEdge = false,
          AllowCreateBend = false,
          ShowHandleItems = GraphItemTypes.None,
          SelectableItems = GraphItemTypes.Label,
          MovableSelectedItems = GraphItemTypes.Label,
          DeletableItems = GraphItemTypes.None
      };
      inputMode.EditLabelInputMode.QueryLabelEditing += (_, evt) => {
        // only the labels of the orange node are editable
        if (evt.Owner?.Tag is not true) {
          evt.Cancel = true;
        }
      };
      ToolTipHelper.ConfigureToolTips(inputMode);

      graphControl.InputMode = inputMode;
    }

    [STAThread]
    public static void Main() {
      Run<LabelStyleDemo>();
    }
  }
}
