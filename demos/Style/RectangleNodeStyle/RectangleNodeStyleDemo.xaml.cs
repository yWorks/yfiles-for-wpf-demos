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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.RectangleNodeStyle
{
  /// <summary>
  ///   Shows the different node shapes that can be implemented with the <see cref="RectangleNodeStyle"/> class.
  /// </summary>
  [Demo("rectangle-node-style", "style", "Rectangle Node Style",
      Summary = "Shows the different node shapes that can be implemented with the RectangleNodeStyle class.",
      Tags = ["style", "interaction"],
      Keywords = [
          "handles", "rectangles", "rounded rectangles", "round rectangles", "pills", "hexagons", "octagons",
          "library style", "style options"
      ],
      RelatedDemos = [
          "shape-node-style", "group-node-style", "arrow-node-style",
          "tutorial-style-implementation-node-create-a-rectangle"
      ])]
  public sealed partial class RectangleNodeStyleDemo
  {
    private static readonly Palette yellow = Themes.Palette71;
    private static readonly Palette orange = Themes.Palette72;
    private static readonly Palette green = Themes.Palette73;
    private static readonly Palette blue = Themes.Palette74;
    private static readonly Palette gray = Themes.Palette75;

    private readonly RectangleNodeStyleSettings settings = new();

    public RectangleNodeStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      InitializeGraph(Graph);
      InitializeInteraction(GraphControl);
      InitializeUI();
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Initializes defaults for the given graph and creates a small sample with different node style settings.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      // Set defaults for new nodes
      graph.NodeDefaults.Style = new Graph.Styles.RectangleNodeStyle {
          Brush = gray.Fill,
          Pen = new Pen(gray.Stroke, 1)
      };
      graph.NodeDefaults.Size = new SizeD(300, 100);
      graph.NodeDefaults.ShareStyleInstance = false;

      // Create nodes with round corners with different resizing behaviors
      CreateNode(graph, new PointD(0, 0), yellow, CornerStyle.Round, false, 10);
      CreateNode(graph, new PointD(0, 200), orange, CornerStyle.Round, true, 0.2);
      CreateNode(graph, new PointD(0, 400), green, CornerStyle.Round, true, 0.5);
      CreateNode(graph, new PointD(0, 600), blue, CornerStyle.Round, true, 0.8, Corners.Bottom);

      // Create nodes with cut-off corners with different resizing behaviors
      CreateNode(graph, new PointD(400, 0), yellow, CornerStyle.Cut, false, 10);
      CreateNode(graph, new PointD(400, 200), orange, CornerStyle.Cut, true, 0.2);
      CreateNode(graph, new PointD(400, 400), green, CornerStyle.Cut, true, 0.5);
      CreateNode(graph, new PointD(400, 600), blue, CornerStyle.Cut, true, 0.8, Corners.Bottom);

      // Create nodes with squircle corners with different resizing behaviors
      CreateNode(graph, new PointD(800, 0), yellow, CornerStyle.Squircle, false, 10);
      CreateNode(graph, new PointD(800, 200), orange, CornerStyle.Squircle, true, 0.2);
      CreateNode(graph, new PointD(800, 400), green, CornerStyle.Squircle, true, 0.5);
      CreateNode(graph, new PointD(800, 600), blue, CornerStyle.Squircle, true, 0.8, Corners.Bottom);
    }

    /// <summary>
    ///   Creates a node with a label that describes the configuration of the <see cref="RectangleNodeStyle"/>.
    /// </summary>
    /// <param name="graph">The graph to which the node belongs.</param>
    /// <param name="location">The location of the node.</param>
    /// <param name="color">The color set of the node and label.</param>
    /// <param name="cornerStyle">Whether corners should be round or a line.</param>
    /// <param name="scaleCornerSize">Whether the corner size should be used as absolute value or be scaled with the node size.</param>
    /// <param name="cornerSize">The corner size.</param>
    /// <param name="corners">Which corners are drawn with the given corner style.</param>
    private static void CreateNode(
        IGraph graph,
        PointD location,
        Palette color,
        CornerStyle cornerStyle,
        bool scaleCornerSize,
        double cornerSize,
        Corners corners = Corners.All) {
      var style = new Graph.Styles.RectangleNodeStyle {
          Brush = color.Fill,
          Pen = new Pen(color.Stroke, 1),
          CornerStyle = cornerStyle,
          ScaleCornerSize = scaleCornerSize,
          CornerSize = cornerSize,
          Corners = corners
      };

      var node = graph.CreateNode(new RectD(location.X, location.Y, 300, 100), style);
      AddLabel(graph, node, color);
    }

    /// <summary>
    ///   Adds a label that describes the owner's style configuration.
    /// </summary>
    /// <param name="graph">The graph to which the label belongs.</param>
    /// <param name="node">The owner of the label.</param>
    /// <param name="color">The color set of the label.</param>
    private static void AddLabel(IGraph graph, INode node, Palette color) {
      graph.AddLabel(node, StyleToText((Graph.Styles.RectangleNodeStyle) node.Style), style: CreateLabelStyle(color));
    }

    /// <summary>
    ///   Sets up an input mode for the GraphControl, and adds a custom handle that allows to change the corner size.
    /// </summary>
    private void InitializeInteraction(GraphControl graphControl) {
      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      SingleSelectionHelper.EnableSingleSelection(inputMode, graphControl);
      graphControl.Selection.Clear();

      // add a label to newly created node that shows the current style settings
      inputMode.NodeCreated += (_, evt) => AddLabel(graphControl.Graph, evt.Item, gray);

      var nodeDecorator = graphControl.Graph.GetDecorator().Nodes;

      // add handle that enables the user to change the corner size of a node
      nodeDecorator.HandleProvider.AddWrapperFactory(node => node.Style is Graph.Styles.RectangleNodeStyle,
          (node, delegateProvider) => new CornerSizeHandleProvider(node, delegateProvider));

      // only provide reshape handles for the right, bottom and bottom-right sides, so they don't clash with the corner size handle
      nodeDecorator.ReshapeHandleProvider.AddFactory(node => new NodeReshapeHandleProvider(node,
          node.Lookup<IReshapeHandler>()!,
          HandlePositions.Right | HandlePositions.Bottom | HandlePositions.BottomRight));

      graphControl.Graph.GetDecorator().Nodes.SelectionRenderer.Hide();
    }

    /// <summary>
    ///   Updates the style properties editor when a different node is selected.
    /// </summary>
    private void OnSelectionChanged() {
      var selectedNode = GraphControl.Selection.Nodes.FirstOrDefault();
      if (selectedNode?.Style is Graph.Styles.RectangleNodeStyle style) {
        settings.UpdateFrom(style, true);
        SetPropertiesViewState(false);
        return;
      }

      settings.IsEnabled = false;
      SetPropertiesViewState(true);
    }

    /// <summary>
    ///   Sets the style properties when they have been changed in the editor.
    /// </summary>
    private void UpdateStyleProperties() {
      var node = GraphControl.Selection.Nodes.FirstOrDefault();
      if (node?.Style is not Graph.Styles.RectangleNodeStyle style) {
        return;
      }

      style.CornerStyle = settings.CornerStyle;
      style.ScaleCornerSize = settings.CornerSizeScaling == CornerSizeScaling.Relative;

      var corners = Corners.None;
      if (settings.TopLeftCornerAffected) {
        corners |= Corners.TopLeft;
      }
      if (settings.TopRightCornerAffected) {
        corners |= Corners.TopRight;
      }
      if (settings.BottomLeftCornerAffected) {
        corners |= Corners.BottomLeft;
      }
      if (settings.BottomRightCornerAffected) {
        corners |= Corners.BottomRight;
      }
      style.Corners = corners;

      var label = node.Labels.FirstOrDefault();
      var text = StyleToText(style);
      if (label is null) {
        Graph.AddLabel(node, text);
      } else {
        Graph.SetLabelText(label, text);
      }

      Graph.InvalidateDisplays();
    }

    /// <summary>
    ///   Shows the properties of the selected node if a node is selected. Otherwise, it shows an information message to select
    ///   a node.
    /// </summary>
    private void SetPropertiesViewState(bool disabled) {
      StylePropertiesPanel.Visibility = disabled ? Visibility.Collapsed : Visibility.Visible;
      InfoMessage.Visibility = disabled ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///   Returns a text description of the style configuration.
    /// </summary>
    private static string StyleToText(Graph.Styles.RectangleNodeStyle style) =>
        $"Corner Style: {CornerStyleValueToText(style.CornerStyle)}\n" +
        $"Corner Size Scaling: {(style.ScaleCornerSize ? "relative" : "absolute")}\n" +
        $"Affected Corners: {CornersToText(style.Corners)}";

    /// <summary>
    ///   Returns a text description of the given corner configuration.
    /// </summary>
    private static string CornersToText(Corners corners) {
      var affected = new[] {
          Corners.All,
          Corners.Top,
          Corners.Bottom,
          Corners.Right,
          Corners.Left,
          Corners.TopLeft,
          Corners.TopRight,
          Corners.BottomLeft,
          Corners.BottomRight
      }.Where(corner => {
        if ((corners & corner) == corner) {
          corners &= ~corner;
          return true;
        }
        return false;
      }).Select(CornerValueToText);

      var text = string.Join(" & ", affected);
      return text.Length > 0 ? text : "none";
    }

    /// <summary>
    ///   Returns the display text for the given corner value.
    /// </summary>
    private static string CornerValueToText(Corners corner) =>
        corner switch {
            Corners.All => "all",
            Corners.Top => "top",
            Corners.Bottom => "bottom",
            Corners.Right => "right",
            Corners.Left => "left",
            Corners.TopLeft => "top-left",
            Corners.TopRight => "top-right",
            Corners.BottomLeft => "bottom-left",
            Corners.BottomRight => "bottom-right",
            _ => corner.ToString().ToLowerInvariant()
        };

    /// <summary>
    ///   Returns the display text for the given corner style value.
    /// </summary>
    private static string CornerStyleValueToText(CornerStyle style) => style.ToString().ToLowerInvariant();

    /// <summary>
    ///   Binds actions to the toolbar and style property input elements.
    /// </summary>
    private void InitializeUI() {
      StylePropertiesPanel.DataContext = settings;
      settings.SettingChanged += (_, _) => UpdateStyleProperties();

      // Update the values of the input elements when the selected element changes
      GraphControl.Selection.ItemAdded += (_, _) => OnSelectionChanged();
      GraphControl.Selection.ItemRemoved += (_, _) => OnSelectionChanged();
      OnSelectionChanged();
    }

    private static LabelStyle CreateLabelStyle(Palette color) =>
        new() {
            TextBrush = color.Text,
            BackgroundBrush = color.NodeLabelFill,
            TextSize = 12,
            Padding = new InsetsD(5)
        };

    [STAThread]
    public static void Main() {
      Run<RectangleNodeStyleDemo>();
    }
  }
}
