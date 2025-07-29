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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Demo.yFiles.Toolkit;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.ThemeVariants
{
  public sealed partial class ThemeVariantsWindow
  {
    private readonly GraphControl[] graphControls = new GraphControl[4];

    private readonly IGraph sharedGraph;

    public ThemeVariantsWindow() {
      InitializeComponent();
      sharedGraph = CreateSampleGraph();
      InitGraphControls();
    }

    public void OnLoaded(object source, EventArgs args) {
      graphControls[0].FitGraphBounds();

      SelectSampleItems();

      InitializeUI();
      InitColorButtons();
    }

    /// <summary>
    /// Creates the sample Graph to display.
    /// </summary>
    private static IGraph CreateSampleGraph() {
      var graph = new DefaultGraph();

      var defaultNodeBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#CCCCCC")!);
      graph.NodeDefaults.Style = new ShapeNodeStyle { Brush = defaultNodeBrush, Pen = Pens.Black };
      graph.NodeDefaults.Labels.Style = new LabelStyle { Padding = new InsetsD(2) };
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      var defaultGroupNodeBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#EEEEEE")!);
      graph.GroupNodeDefaults.Style = new ShapeNodeStyle { Brush = defaultGroupNodeBrush, Pen = null };
      graph.GroupNodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(ExteriorNodeLabelModel.Position.Top);
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle { Padding = new InsetsD(2) };

      var defaultEdgeBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#AAAAAA")!);
      var defaultEdgePen = new Pen(defaultEdgeBrush, 1);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = defaultEdgePen, TargetArrow = new Arrow(ArrowType.Triangle, defaultEdgeBrush, null, 0.75, 0.75)
      };
      graph.EdgeDefaults.Labels.Style = new LabelStyle { Padding = new InsetsD(2) };

      var node1 = graph.CreateNode(new PointD(20, 110));
      var node2 = graph.CreateNode(new PointD(120, 145));
      var node3 = graph.CreateNode(new PointD(120, 75));
      var node4 = graph.CreateNode(new PointD(220, 30));
      var node5 = graph.CreateNode(new PointD(220, 100));
      graph.AddLabel(node4, "Node");
      graph.AddLabel(node5, "Node");

      var group = graph.GroupNodes(node1, node2, node3);
      graph.AddLabel(group, "Group");

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
      graph.AddBends(edge1, new[] { new PointD(70, 123.33), new PointD(70, 145) });
      graph.AddBends(edge2, new[] { new PointD(70, 96.67), new PointD(70, 75) });
      graph.AddBends(edge3, new[] { new PointD(170, 65), new PointD(170, 30) });
      graph.AddBends(edge4, new[] { new PointD(170, 85), new PointD(170, 90) });

      // We use ghost here so we can also see the IsCurrentCandidate variant of the label candidate visual.
      graph.GetDecorator().Labels.PositionHandler
          .AddFactory(label => new LabelPositionHandler(label) { Visualization = Visualization.Ghost });

      return graph;
    }

    /// <summary>
    /// Initializes the graph controls for each theme variant.
    /// </summary>
    /// <remarks>
    /// Synchronizes the controls so that they all display the same graph and selection.
    /// </remarks>
    private void InitGraphControls() {
      var themeVariants = Enum.GetValues(typeof(ThemeVariant));
      for (var i = 0; i < themeVariants.Length; i++) {
        var themeVariant = (ThemeVariant) themeVariants.GetValue(i);
        var gc = CreateGraphControl(themeVariant);
        graphControls[i] = gc;

        if (i > 0) {
          // Synchronize all other controls to the first one
          gc.Selection = graphControls[0].Selection;
          gc.HighlightIndicatorManager.Items = graphControls[0].HighlightIndicatorManager.Items;
          gc.SetBinding(GraphControl.CurrentItemProperty,
              new Binding("CurrentItem") { Source = graphControls[0], Mode = BindingMode.TwoWay });
          gc.SetBinding(CanvasControl.ContentBoundsProperty,
              new Binding("ContentBounds") { Source = graphControls[0], Mode = BindingMode.TwoWay });
          gc.SetBinding(CanvasControl.ViewPointProperty,
              new Binding("ViewPoint") { Source = graphControls[0], Mode = BindingMode.TwoWay });
          gc.SetBinding(CanvasControl.ZoomProperty,
              new Binding("Zoom") { Source = graphControls[0], Mode = BindingMode.TwoWay });
        }
        // Propagate redraws to all other controls
        gc.UpdatedVisual += (_, _) => {
          if (gc.IsKeyboardFocusWithin) {
            foreach (var otherGc in graphControls) {
              if (otherGc != gc) {
                otherGc.Invalidate();
              }
            }
          }
        };
      }
      SetColors(ColorSchemes[0]);
    }

    /// <summary>
    /// Creates a GraphControl which uses the given theme variant.
    /// </summary>
    private GraphControl CreateGraphControl(ThemeVariant themeVariant) {
      var gcContainer = new Grid();
      var label = new TextBlock {
          Text = themeVariant.ToString(),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Top,
          Padding = new Thickness(4),
          Background = (Brush) new BrushConverter().ConvertFromString("#ebeef0"),
          IsHitTestVisible = false
      };
      var graphControl = new GraphControl {
          // Always show focus, so we can observe the theme changes
          FocusIndicatorManager = { ShowFocusPolicy = ShowFocusPolicy.Always },
          InputMode = new GraphEditorInputMode { AllowReparentNodes = true, SnapContext = new GraphSnapContext() },
          Graph = sharedGraph,
          // Leave a small gap around the controls so we get grid lines
          Margin = new Thickness(1),
          Background = Brushes.White
      };
      SetThemeVariant(graphControl, themeVariant);
      gcContainer.Children.Add(graphControl);
      gcContainer.Children.Add(label);
      GraphControlsGrid.Children.Add(gcContainer);
      return graphControl;
    }

    private void SelectSampleItems() {
      var selection = graphControls[0].Selection;
      if (selection.Any()) {
        // Something is already selected, don't change it
        return;
      }

      var node = sharedGraph.Nodes[0];
      if (node == null) {
        return;
      }

      selection.Add(node);
      foreach (var edge in graphControls[0].Graph.EdgesAt(node)) {
        selection.Add(edge);
        var opposite = (INode) edge.Opposite(node);
        if (opposite.Labels.Any()) {
          selection.Add(opposite.Labels[0]);
        }
      }
    }

    private void InitializeUI() {
      IncreaseZoomButton.CommandTarget = graphControls[0];
      DecreaseZoomButton.CommandTarget = graphControls[0];
      FitBoundsButton.CommandTarget = graphControls[0];
      Application.Current.Resources[CanvasControl.ThemeScaleKey] = 1.4;
      ScaleSlider.Value = 1.4;
      var handleOffset = (double?) Application.Current.TryFindResource(CanvasControl.ThemeHandleOffsetKey);
      var indicatorOffset = (double?) Application.Current.TryFindResource(CanvasControl.ThemeIndicatorOffsetKey);
      HandleOffsetSlider.Value = handleOffset ?? 2;
      IndicatorOffsetSlider.Value = indicatorOffset ?? 2;
    }

    private void InitColorButtons() {
      foreach (var colorScheme in ColorSchemes) {
        var button = new Button {
            Content = colorScheme,
            Padding = new Thickness { Top = 2, Bottom = 2, Left = 5, Right = 5 },
            Margin = new Thickness { Left = 5 }
        };
        button.Click += (_, _) => {
          SetColors(colorScheme);
          Rerender();
        };
        ToolBar.Items.Add(button);
      }
    }

    private static void SetThemeVariant(GraphControl graphControl, ThemeVariant themeVariant) {
      if (graphControl == null) {
        return;
      }
      graphControl.Resources[CanvasControl.ThemeVariantKey] = themeVariant;
    }

    private static void SetColors(ColorScheme palette) {
      Application.Current.Resources[CanvasControl.ThemePrimaryBrushKey] = palette.Primary;
      Application.Current.Resources[CanvasControl.ThemeSecondaryBrushKey] = palette.Secondary;
      Application.Current.Resources[CanvasControl.ThemeBackgroundBrushKey] = palette.Background;
    }

    private void OnScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      Application.Current.Resources[CanvasControl.ThemeScaleKey] = ScaleSlider.Value;
      Rerender();
    }

    private void OnHandleOffsetChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      Application.Current.Resources[CanvasControl.ThemeHandleOffsetKey] = HandleOffsetSlider.Value;
      Rerender();
    }

    private void OnIndicatorOffsetChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      Application.Current.Resources[CanvasControl.ThemeIndicatorOffsetKey] = IndicatorOffsetSlider.Value;
      Rerender();
    }

    private void Rerender() {
      // Just replacing application resources doesn't trigger a redraw. We have to do this by resetting the input mode and the managers.
      foreach (var graphControl in graphControls) {
        if (graphControl != null) {
          var oldInputMode = graphControl.InputMode;
          graphControl.InputMode = null;
          graphControl.InputMode = oldInputMode;
          graphControl.SelectionIndicatorManager.Enabled = false;
          graphControl.SelectionIndicatorManager.Enabled = true;
          graphControl.HighlightIndicatorManager.Enabled = false;
          graphControl.HighlightIndicatorManager.Enabled = true;
          graphControl.FocusIndicatorManager.Enabled = false;
          graphControl.FocusIndicatorManager.Enabled = true;
        }
      }
    }

    public void OnSelectAllClicked(object sender, RoutedEventArgs routedEventArgs) {
      var graphSelection = graphControls[0].Selection;
      foreach (var node in sharedGraph.Nodes) {
        graphSelection.Add(node);
      }
      foreach (var edge in sharedGraph.Edges) {
        graphSelection.Add(edge);
        foreach (var bend in edge.Bends) {
          graphSelection.Add(bend);
        }
      }
      foreach (var label in sharedGraph.Labels) {
        graphSelection.Add(label);
      }
      foreach (var port in sharedGraph.Ports) {
        graphSelection.Add(port);
      }
    }

    public void OnHighlightAllClicked(object sender, RoutedEventArgs routedEventArgs) {
      var highlightedItems = graphControls[0].HighlightIndicatorManager.Items;
      if (HighlightAllButton.IsChecked == true) {
        foreach (var node in sharedGraph.Nodes) {
          highlightedItems.Add(node);
        }
        foreach (var edge in sharedGraph.Edges) {
          highlightedItems.Add(edge);
        }
        foreach (var label in sharedGraph.Labels) {
          highlightedItems.Add(label);
        }
        foreach (var port in sharedGraph.Ports) {
          highlightedItems.Add(port);
        }
      } else {
        highlightedItems.Clear();
      }
    }

    public void OnFocusNodeClicked(object sender, RoutedEventArgs routedEventArgs) {
      graphControls[0].CurrentItem = sharedGraph.Nodes.FirstOrDefault();
    }

    public void OnFocusLabelClicked(object sender, RoutedEventArgs routedEventArgs) {
      graphControls[0].CurrentItem = sharedGraph.Labels.FirstOrDefault();
    }

    public void OnFocusEdgeClicked(object sender, RoutedEventArgs routedEventArgs) {
      graphControls[0].CurrentItem = sharedGraph.Edges.FirstOrDefault();
    }

    public void OnClearFocusClicked(object sender, RoutedEventArgs routedEventArgs) {
      graphControls[0].CurrentItem = null;
    }

    public static List<ColorScheme> ColorSchemes =>
        new() {
            new("Blue", Themes.PaletteLightblue.Stroke, Themes.PaletteLightblue.NodeLabelFill,
                Themes.PaletteLightblue.Fill),
            new("Orange", Themes.PaletteOrange.Stroke, Themes.PaletteOrange.NodeLabelFill, Themes.PaletteOrange.Fill),
            new("Green", Themes.PaletteGreen.Stroke, Themes.PaletteGreen.NodeLabelFill, Themes.PaletteGreen.Fill),
            new("Purple", Themes.PalettePurple.Stroke, Themes.PalettePurple.NodeLabelFill, Themes.PalettePurple.Fill),
            new("Yellow", Themes.Palette22.Stroke, Themes.Palette22.NodeLabelFill, Themes.Palette22.Fill),
            new("Gradient", new LinearGradientBrush(Color.FromRgb(0, 131, 178), Color.FromRgb(103, 215, 150), -30),
                new LinearGradientBrush(Color.FromRgb(173, 214, 229), Color.FromRgb(130, 147, 161), -30),
                new LinearGradientBrush(Color.FromRgb(0, 188, 255), Color.FromRgb(115, 239, 167), -30))
        };
  }

  public class ColorScheme
  {
    public ColorScheme(string name, Brush primary, Brush secondary, Brush background) {
      Name = name;
      Primary = primary;
      Secondary = secondary;
      Background = background;
    }

    public string Name { get; }

    public Brush Primary { get; }

    public Brush Secondary { get; }

    public Brush Background { get; }
  }
}
