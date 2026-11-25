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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ArrowNodeStyle
{
  /// <summary>
  ///   Shows the features of the <see cref="ArrowNodeStyle"/> class.
  /// </summary>
  [Demo("arrow-node-style", "style", "Arrow Node Style", Summary = "Shows the features of the ArrowNodeStyle class.",
      Tags = ["style", "nodes", "arrows"], Keywords = ["library style", "style options"], RelatedDemos = [
          "shape-node-style", "rectangle-node-style", "group-node-style",
          "tutorial-style-implementation-node-create-a-rectangle"
      ])]
  public sealed partial class ArrowNodeStyleDemo
  {
    private readonly NodeStyleSettings settings = new();

    public ArrowNodeStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      InitializeGraph();
      InitializeInteraction();
      InitializeUI();
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Initializes defaults for the graph and creates a small sample with different node style
    ///   settings.
    /// </summary>
    private void InitializeGraph() {
      var defaultLabelParameter = FreeNodeLabelModel.Instance.CreateParameter(new PointD(0.5, 0), new PointD(0, -10),
          new PointD(0.5, 1), new PointD(0, 0));

      // initialize the graph defaults
      Graph.NodeDefaults.Style = CreateDefaultStyle();
      Graph.NodeDefaults.Size = new SizeD(200, 100);
      Graph.NodeDefaults.ShareStyleInstance = false;
      Graph.NodeDefaults.Labels.Style = CreateLabelStyle(Themes.PaletteOrange);
      Graph.NodeDefaults.Labels.LayoutParameter = defaultLabelParameter;

      // create nodes with different shapes, angles and shaft ratios
      CreateNodes(Graph, 0, ArrowStyleShape.Arrow, Themes.PaletteOrange);
      CreateNodes(Graph, 300, ArrowStyleShape.DoubleArrow, Themes.PaletteBlue);
      CreateNodes(Graph, 600, ArrowStyleShape.NotchedArrow, Themes.PaletteRed);
      CreateNodes(Graph, 900, ArrowStyleShape.Parallelogram, Themes.PaletteGreen);
      CreateNodes(Graph, 1200, ArrowStyleShape.Trapezoid, Themes.PalettePurple);
    }

    /// <summary>Creates a new node label style with colors from the given palette.</summary>
    /// <param name="palette">The palette to use for background and text color.</param>
    private static LabelStyle CreateLabelStyle(Palette palette) {
      var style = DemoStyles.CreateDemoNodeLabelStyle(palette);
      style.TextAlignment = TextAlignment.Left;
      style.Padding = new InsetsD(4, 8, 4, 8);
      style.TextSize = 14;
      return style;
    }

    /// <summary>
    ///   Creates several nodes with the given shape and different angles as well as shaft ratios.
    /// </summary>
    /// <param name="graph">The graph in which to create nodes.</param>
    /// <param name="xOffset">The x-location where to place the nodes.</param>
    /// <param name="shape">The shape to use for the arrow.</param>
    /// <param name="palette">The color palette to use for nodes and labels.</param>
    private void CreateNodes(IGraph graph, double xOffset, ArrowStyleShape shape, Palette palette) {
      var angleFactor = shape is ArrowStyleShape.Parallelogram or ArrowStyleShape.Trapezoid ? 0.5 : 1;

      // create a couple of ArrowNodeStyle instances that demonstrate various configuration options
      var styles = new[] {
          CreateArrowNodeStyle(shape, ArrowNodeDirection.Left, angleFactor * Math.PI / 8, 0.25, palette),
          CreateArrowNodeStyle(shape, ArrowNodeDirection.Up, angleFactor * Math.PI / 4, 1.0 / 3, palette),
          CreateArrowNodeStyle(shape, ArrowNodeDirection.Right, angleFactor * Math.PI * 3 / 8, 0.75, palette),
          CreateArrowNodeStyle(shape, ArrowNodeDirection.Right, angleFactor * -Math.PI / 8, 1, palette)
      };

      var labelParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(30) }.CreateParameter(ExteriorNodeLabelModel.Position
              .Bottom);
      var labelStyle = CreateLabelStyle(palette);

      // create a sample node for each sample style instance
      var y = 0.0;
      for (var i = 0; i < styles.Length; i++) {
        var x = xOffset + (i == 1 ? 50 : 0);
        var width = i == 1 ? 100 : 200;
        var height = i == 1 ? 200 : 100;
        var style = styles[i];
        var node = graph.CreateNode(new RectD(x, y, width, height), style);
        graph.AddLabel(node, StyleToText(style), labelParameter, labelStyle);
        y += height + 250;
      }
    }

    private static Graph.Styles.ArrowNodeStyle CreateArrowNodeStyle(
        ArrowStyleShape shape,
        ArrowNodeDirection direction,
        double angle,
        double shaftRatio,
        Palette palette) =>
        new() {
            Shape = shape,
            Direction = direction,
            Angle = angle,
            ShaftRatio = shaftRatio,
            Brush = palette.Fill,
            Pen = new Pen(palette.Stroke, 1)
        };

    private static Graph.Styles.ArrowNodeStyle CreateDefaultStyle() =>
        new() {
            Brush = Themes.PaletteOrange.Fill,
            Pen = new Pen(Themes.PaletteOrange.Stroke, 1)
        };

    /// <summary>Sets up an input mode for the GraphControl.</summary>
    private void InitializeInteraction() {
      var inputMode = new GraphEditorInputMode { SelectableItems = GraphItemTypes.Node };
      inputMode.NodeCreated += (_, e) => {
        if (e.Item.Style is Graph.Styles.ArrowNodeStyle style) {
          Graph.AddLabel(e.Item, StyleToText(style));
        }
      };
      GraphControl.InputMode = inputMode;
    }

    /// <summary>Binds actions to the toolbar and style property editor.</summary>
    private void InitializeUI() {
      StylePropertiesPanel.DataContext = settings;
      settings.SettingChanged += (_, _) => ApplyStyleSetting();

      // adjust option panel when the selection has been changed
      GraphControl.Selection.ItemAdded += (_, e) => {
        if (e.Item is INode node && node.Style is Graph.Styles.ArrowNodeStyle) {
          AdjustOptionPanel(node);
        }
      };
      GraphControl.Selection.ItemRemoved += (_, _) => {
        if (GraphControl.Selection.Nodes.Count == 0) {
          settings.IsEnabled = false;
          InfoMessage.Visibility = Visibility.Visible;
          StylePropertiesPanel.Visibility = Visibility.Collapsed;
        }
      };
    }

    /// <summary>Applies changes in the property editor to the selected nodes and default style.</summary>
    private void ApplyStyleSetting() {
      foreach (var node in GraphControl.Selection.Nodes) {
        if (node.Style is not Graph.Styles.ArrowNodeStyle style) {
          continue;
        }

        settings.ApplyTo(style);
        if (node.Labels.Count == 0) {
          Graph.AddLabel(node, StyleToText(style));
        } else {
          Graph.SetLabelText(node.Labels[0], StyleToText(style));
        }
      }

      // adjust also the default style applied to newly created nodes
      if (Graph.NodeDefaults.Style is Graph.Styles.ArrowNodeStyle defaultStyle) {
        settings.ApplyTo(defaultStyle);
      }

      GraphControl.Invalidate();
    }

    /// <summary>Returns a text description of the style configuration.</summary>
    private static string StyleToText(Graph.Styles.ArrowNodeStyle style) =>
        $"Shape: {GetShapeName(style.Shape)}\n" + $"Direction: {style.Direction}\n" +
        $"Angle: {ToDegrees(style.Angle).ToString("0", CultureInfo.CurrentCulture)}\n" +
        $"Shaft Ratio: {style.ShaftRatio.ToString("0.0", CultureInfo.CurrentCulture)}";

    /// <summary>Adjusts the option panel to show the settings of the selected node.</summary>
    /// <param name="node">The node whose style settings should be shown.</param>
    private void AdjustOptionPanel(INode node) {
      var style = (Graph.Styles.ArrowNodeStyle) node.Style;
      settings.UpdateFrom(style, true);

      // update defaultArrowNodeStyle to correspond to the option panel
      var defaultStyle = CreateDefaultStyle();
      settings.ApplyTo(defaultStyle);
      Graph.NodeDefaults.Style = defaultStyle;
      Graph.NodeDefaults.Size = new SizeD(node.Layout.Width, node.Layout.Height);
      InfoMessage.Visibility = Visibility.Collapsed;
      StylePropertiesPanel.Visibility = Visibility.Visible;
    }

    private static string GetShapeName(ArrowStyleShape shape) {
      return shape switch {
          ArrowStyleShape.Arrow => "Arrow",
          ArrowStyleShape.DoubleArrow => "Double Arrow",
          ArrowStyleShape.NotchedArrow => "Notched Arrow",
          ArrowStyleShape.Trapezoid => "Trapezoid",
          ArrowStyleShape.Parallelogram => "Parallelogram",
          _ => shape.ToString()
      };
    }

    /// <summary>Returns the given angle in degrees.</summary>
    private static double ToDegrees(double radians) => radians * 180 / Math.PI;

    [STAThread]
    public static void Main() {
      Run<ArrowNodeStyleDemo>();
    }
  }
}
