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
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ThemeVariants
{
  /// <summary>
  ///   Shows various interaction visualization themes simultaneously.
  /// </summary>
  [Demo("theme-variants", "style", "Theme Variants",
      Summary = "Shows various interaction visualization themes simultaneously.", Tags = ["theme", "handle", "color"])]
  public sealed partial class ThemeVariantsDemo
  {
    private readonly GraphControl[] graphControls;

    private IGraph sharedGraph = null!;

    private bool changingViewport;

    public ThemeVariantsDemo() {
      InitializeComponent();
      graphControls = [graphControl, roundHatchedGraphControl, squareGraphControl, squareHatchedGraphControl];
      scaleSlider.ValueChanged += OnScaleChanged;
      handleOffsetSlider.ValueChanged += OnHandleOffsetChanged;
      indicatorOffsetSlider.ValueChanged += OnIndicatorOffsetChanged;
    }

    protected override void Initialize() {
      sharedGraph = CreateSampleGraph();
      InitializeGraphComponents();
      GraphControl.FitGraphBounds();
      SelectSampleItems();
      InitializeUI();
    }

    private GraphControl GraphControl => graphControl;

    /// <summary>
    ///   Configures the GraphComponents with the different theme variants and connects the shared graph.
    /// </summary>
    private void InitializeGraphComponents() {
      // one of each variant
      CreateGraphComponent(graphControl, ThemeVariant.Round);
      CreateGraphComponent(roundHatchedGraphControl, ThemeVariant.RoundHatched);
      CreateGraphComponent(squareGraphControl, ThemeVariant.Square);
      CreateGraphComponent(squareHatchedGraphControl, ThemeVariant.SquareHatched);

      SynchronizeGraphComponents();
    }

    /// <summary>
    ///   Configures a GraphComponent with the given theme and the shared graph.
    /// </summary>
    private void CreateGraphComponent(GraphControl graphComponent, ThemeVariant themeVariant) {
      graphComponent.Resources[CanvasControl.ThemeVariantKey] = themeVariant;
      graphComponent.Graph = sharedGraph;
      graphComponent.InputMode = new GraphEditorInputMode {
          AllowReparentNodes = true,
          SnapContext = new GraphSnapContext()
      };
    }

    /// <summary>
    ///   Synchronizes the graph components so that changes in one will be mirrored in the others.
    /// </summary>
    private void SynchronizeGraphComponents() {
      foreach (var graphComponent in graphControls) {
        graphComponent.ViewportChanged += (_, _) => SynchronizeViewport(graphComponent);
        graphComponent.Selection.ItemAdded += (_, e) => SynchronizeSelection(graphComponent, e.Item, true);
        graphComponent.Selection.ItemRemoved += (_, e) => SynchronizeSelection(graphComponent, e.Item, false);
      }
    }

    private void SynchronizeViewport(GraphControl source) {
      if (changingViewport) {
        return;
      }

      changingViewport = true;
      try {
        foreach (var otherComponent in graphControls) {
          if (otherComponent != source) {
            otherComponent.ZoomTo(source.Viewport);
          }
        }
      } finally {
        changingViewport = false;
      }
    }

    private void SynchronizeSelection(GraphControl source, IModelItem item, bool selected) {
      foreach (var otherComponent in graphControls) {
        if (otherComponent == source) {
          continue;
        }

        var containsItem = otherComponent.Selection.Contains(item);
        if (selected && !containsItem) {
          otherComponent.Selection.Add(item);
        } else if (!selected && containsItem) {
          otherComponent.Selection.Remove(item);
        }
      }
    }

    /// <summary>
    ///   Creates a sample graph.
    /// </summary>
    private IGraph CreateSampleGraph() {
      var graph = new DefaultGraph();

      var lightGray = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
      var lightGrayPen = new Pen(Brushes.Black, 1);
      var groupGray = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
      var edgeGray = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));

      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Brush = lightGray,
          Pen = lightGrayPen
      };
      graph.NodeDefaults.Labels.Style = new LabelStyle { Padding = new InsetsD(2) };
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.GroupNodeDefaults.Style = new ShapeNodeStyle {
          Brush = groupGray,
          Pen = new Pen(Brushes.Transparent, 0)
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(ExteriorNodeLabelModel.Position.Top);
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle { Padding = new InsetsD(2) };
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(edgeGray, 1),
          TargetArrow = new Arrow(ArrowType.Triangle, edgeGray, lengthScale: 0.5, widthScale: 0.5)
      };
      graph.EdgeDefaults.Labels.Style = new LabelStyle { Padding = new InsetsD(2) };

      var node1 = graph.CreateNode(new PointD(20, 110));
      var node2 = graph.CreateNode(new PointD(120, 145));
      var node3 = graph.CreateNode(new PointD(120, 75));
      var node4 = graph.CreateNode(new PointD(220, 30));
      var node5 = graph.CreateNode(new PointD(220, 100));
      graph.AddLabel(node4, "Node");
      graph.AddLabel(node5, "Node");

      var groupNode = graph.GroupNodes(node1, node2, node3);
      graph.AddLabel(groupNode, "Group");

      var edge1 = graph.CreateEdge(node1, node2);
      var edge2 = graph.CreateEdge(node1, node3);
      var edge3 = graph.CreateEdge(node3, node4);
      var edge4 = graph.CreateEdge(node3, node5);
      var edge5 = graph.CreateEdge(node1, node5);
      graph.AddLabel(edge1, "Edge",
          new NinePositionsEdgeLabelModel().CreateParameter(NinePositionsEdgeLabelModel.Position.TargetBelow));
      graph.SetPortLocation(edge1.SourcePort, new PointD(40, 123.33));
      graph.SetPortLocation(edge1.TargetPort, new PointD(100, 145));
      graph.SetPortLocation(edge2.SourcePort, new PointD(40, 96.67));
      graph.SetPortLocation(edge2.TargetPort, new PointD(100, 75));
      graph.SetPortLocation(edge3.SourcePort, new PointD(140, 65));
      graph.SetPortLocation(edge3.TargetPort, new PointD(200, 30));
      graph.SetPortLocation(edge4.SourcePort, new PointD(140, 85));
      graph.SetPortLocation(edge4.TargetPort, new PointD(200, 90));
      graph.SetPortLocation(edge5.SourcePort, new PointD(40, 110));
      graph.SetPortLocation(edge5.TargetPort, new PointD(200, 110));
      graph.AddBends(edge1, [new PointD(70, 123.33), new PointD(70, 145)]);
      graph.AddBends(edge2, [new PointD(70, 96.67), new PointD(70, 75)]);
      graph.AddBends(edge3, [new PointD(170, 65), new PointD(170, 30)]);
      graph.AddBends(edge4, [new PointD(170, 85), new PointD(170, 90)]);

      return graph;
    }

    /// <summary>
    ///   Selects some items for illustration, but only if no elements are currently selected.
    /// </summary>
    private void SelectSampleItems() {
      var selection = graphControls[0].Selection;
      if (selection.Count > 0) {
        // Something is already selected, don't change it
        return;
      }

      var node = sharedGraph.Nodes.FirstOrDefault();
      if (node == null) {
        return;
      }

      selection.Add(node);
      foreach (var edge in sharedGraph.EdgesAt(node)) {
        selection.Add(edge);
        var opposite = (INode) edge.Opposite(node);
        var label = opposite.Labels.FirstOrDefault();
        if (label != null) {
          selection.Add(label);
        }
      }
    }

    /// <summary>
    ///   Binds actions to the buttons in the tutorial's toolbar.
    /// </summary>
    private void InitializeUI() {
      SetGraphControlResource(CanvasControl.ThemeScaleKey, scaleSlider.Value);
      SetGraphControlResource(CanvasControl.ThemeHandleOffsetKey, handleOffsetSlider.Value);
      SetGraphControlResource(CanvasControl.ThemeIndicatorOffsetKey, indicatorOffsetSlider.Value);
      SetPaletteResources(Themes.PaletteLightblue);

      SetSliderLabel(scaleLabel, scaleSlider.Value);
      SetSliderLabel(handleOffsetLabel, handleOffsetSlider.Value);
      SetSliderLabel(indicatorOffsetLabel, indicatorOffsetSlider.Value);

      foreach (var button in new[] { blueButton, orangeButton, greenButton, purpleButton, yellowButton }) {
        var palette = GetColorPalette((string) button.Tag);
        button.Background = palette.NodeLabelFill;
        button.Foreground = Themes.PaletteBlue.Text;
      }
    }

    private void OnScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      SetGraphControlResource(CanvasControl.ThemeScaleKey, e.NewValue);
      SetSliderLabel(scaleLabel, e.NewValue);
      InvalidateGraphControls();
    }

    private void OnHandleOffsetChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      SetGraphControlResource(CanvasControl.ThemeHandleOffsetKey, e.NewValue);
      SetSliderLabel(handleOffsetLabel, e.NewValue);
      InvalidateGraphControls();
    }

    private void OnIndicatorOffsetChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      SetGraphControlResource(CanvasControl.ThemeIndicatorOffsetKey, e.NewValue);
      SetSliderLabel(indicatorOffsetLabel, e.NewValue);
      InvalidateGraphControls();
    }

    private void SetPaletteResources(Palette palette) {
      SetGraphControlResource(CanvasControl.ThemePrimaryBrushKey, palette.Stroke);
      SetGraphControlResource(CanvasControl.ThemeSecondaryBrushKey, palette.NodeLabelFill);
      SetGraphControlResource(CanvasControl.ThemeBackgroundBrushKey, palette.Fill);
    }

    private void SetGraphControlResource(ResourceKey key, object value) {
      foreach (var control in graphControls) {
        control.Resources[key] = value;
        // Just replacing application resources doesn't trigger a redraw. We have to do this by resetting the input mode and the managers.
        var oldInputMode = control.InputMode;
        control.InputMode = null;
        control.InputMode = oldInputMode;
        control.SelectionIndicatorManager.Enabled = false;
        control.SelectionIndicatorManager.Enabled = true;
        control.HighlightIndicatorManager.Enabled = false;
        control.HighlightIndicatorManager.Enabled = true;
        control.FocusIndicatorManager.Enabled = false;
        control.FocusIndicatorManager.Enabled = true;
      }
    }

    private void InvalidateGraphControls() {
      foreach (var control in graphControls) {
        control.Invalidate();
      }
    }

    private static void SetSliderLabel(Label label, double value) {
      label.Content = value.ToString("0.##");
    }

    private void OnColorPaletteButtonClick(object sender, RoutedEventArgs e) {
      if (sender is Button { Tag: string paletteName }) {
        var palette = GetColorPalette(paletteName);
        SetPaletteResources(palette);
        InvalidateGraphControls();
      }
    }

    private static Palette GetColorPalette(string paletteName) =>
        paletteName switch {
            "Blue" => Themes.PaletteLightblue,
            "Orange" => Themes.PaletteOrange,
            "Green" => Themes.PaletteGreen,
            "Purple" => Themes.PalettePurple,
            "Yellow" => Themes.Palette22,
            _ => throw new ArgumentOutOfRangeException(nameof(paletteName), paletteName, "Unknown color palette.")
        };

    [STAThread]
    public static void Main() {
      Run<ThemeVariantsDemo>();
    }
  }
}
