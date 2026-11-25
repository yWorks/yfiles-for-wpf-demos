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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.TabularGroups
{
  /// <summary>
  ///   Demonstrates how to configure the tabular groups feature of the hierarchical layout.
  /// </summary>
  [Demo("tabular-groups", "layout", "Tabular Groups",
      Summary = "Shows how to configure the tabular groups feature of the hierarchical layout.",
      Tags = ["hierarchical", "tabular", "groups"],
      Keywords = ["layoutfeatures", "layout", "column", "compact", "padding"], RelatedDemos = [])]
  public sealed partial class TabularGroupsDemo
  {
    private readonly Settings settings = new();
    private bool suppressSettingsEvents;

    public TabularGroupsDemo() {
      InitializeComponent();
      DataContext = settings;
      settings.PropertyChanged += Settings_PropertyChanged;
      childDistanceSlider.AddHandler(Thumb.DragCompletedEvent,
          new DragCompletedEventHandler((_, _) => UpdateChildDistanceBinding()), true);
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override async void Initialize() {
      GraphControl.InputMode = new GraphViewerInputMode();

      // Load the initial sample graph and run the layout.
      await LoadSampleGraph(settings.SelectedSample);
      await RunHierarchicalLayoutWithTabularGroups();
      await GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Runs a <see cref="HierarchicalLayout"/> configured with tabular groups on the graph.
    /// </summary>
    private Task RunHierarchicalLayoutWithTabularGroups() {
      // Create the configured hierarchical layout with the tabular groups feature.
      var (layout, layoutData) = HierarchicalLayoutTabularGroups.CreateTabularGroupsHierarchicalLayout(
          Graph, settings.SortingEnabled, settings.ChildDistance);

      // Apply the layout to the graph.
      return GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(500), layoutData);
    }

    /// <summary>
    ///   Loads the sample selected in the combo box and populates the graph.
    /// </summary>
    private Task LoadSampleGraph(string sampleName) {
      // For the nested sample, add some more distance to the children nodes.
      if (sampleName == "uml") {
        settings.ChildDistance = 5;
      }

      var isSimple = sampleName == "simple";

      // Initialize the style of the graph.
      InitializeGraph(Graph, isSimple);

      // Build the graph from the embedded sample data.
      BuildGraphFromJson($"{sampleName}.json", Graph);

      // Customize the style of nested group nodes.
      foreach (var node in Graph.Nodes) {
        if (Graph.IsGroupNode(node) && Graph.GetParent(node) is not null) {
          Graph.SetStyle(node, CreateNestedGroupStyle());
        }
      }

      return Task.CompletedTask;
    }

    /// <summary>
    ///   Initializes the style defaults for the graph.
    /// </summary>
    private static void InitializeGraph(IGraph graph, bool isSimple = false) {
      var palette = Themes.Palette56;

      // Initialize the basic style of the graph items.
      DemoStyles.InitDemoStyles(graph, palette, palette, palette, palette, palette, palette);

      graph.NodeDefaults.Style = new RectangleNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD1, 0xB3)),
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x71, 0x73, 0x45)), 1.5),
          CornerStyle = CornerStyle.Round,
          CornerSize = 3.5
      };

      graph.NodeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xE7, 0xE8, 0xD9)),
          Shape = LabelShape.RoundRectangle,
          Padding = new InsetsD(4, 2, 4, 2)
      };

      // Customize the group node style and its label to get nice tabular groups.
      graph.GroupNodeDefaults.Style = CreateGroupStyle(palette, palette.Fill, true);
      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          VerticalTextAlignment = VerticalAlignment.Center,
          TextAlignment = TextAlignment.Left,
          TextWrapping = TextWrapping.Wrap,
          TextTrimming = TextTrimming.CharacterEllipsis,
          TextBrush = Brushes.White,
          Padding = new InsetsD(4)
      };
      graph.GroupNodeDefaults.Labels.LayoutParameter = StretchNodeLabelModel.Top;

      // Customize the node label background color based on the selected sample.
      if (!isSimple && graph.NodeDefaults.Labels.Style is LabelStyle nodeLabelStyle) {
        nodeLabelStyle.BackgroundBrush = null;
      }
    }

    /// <summary>
    ///   Creates a group node style for nested groups.
    /// </summary>
    private static GroupNodeStyle CreateNestedGroupStyle() =>
        CreateGroupStyle(Themes.Palette56, new SolidColorBrush(Color.FromRgb(0x9E, 0xA0, 0x2C)), false);

    private static GroupNodeStyle CreateGroupStyle(Palette palette, Brush tabBackground, bool drawShadow) =>
        new() {
            TabBrush = palette.Stroke,
            TabBackgroundBrush = tabBackground,
            ContentAreaBrush = Brushes.White,
            TabSlope = 0.5,
            DrawShadow = drawShadow,
            ContentAreaPadding = new InsetsD(8),
            TabPosition = GroupNodeStyleTabPosition.TopLeading,
            Pen = new Pen(palette.Stroke, 1),
            TabHeight = 20,
            TabWidth = 80,
            TabPadding = 2
        };

    private async void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
      if (suppressSettingsEvents) {
        return;
      }

      switch (e.PropertyName) {
        case nameof(Settings.SelectedSample):
          await LoadSelectedSample();
          break;
        case nameof(Settings.TabularGroupsEnabled):
          if (settings.TabularGroupsEnabled) {
            await RunHierarchicalLayoutWithTabularGroups();
          } else {
            // Run a hierarchical layout without tabular groups.
            await GraphControl.ApplyLayoutAnimated(HierarchicalLayoutTabularGroups.CreateDefaultHierarchicalLayout(),
                TimeSpan.FromMilliseconds(500));
          }
          break;
        case nameof(Settings.SortingEnabled):
        case nameof(Settings.ChildDistance):
          if (settings.TabularGroupsEnabled) {
            await RunHierarchicalLayoutWithTabularGroups();
          }
          break;
      }
    }

    private void ChildDistanceSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      childDistanceSlider.Dispatcher.BeginInvoke(UpdateChildDistanceBinding);
    }

    private void ChildDistanceSlider_KeyUp(object sender, KeyEventArgs e) {
      UpdateChildDistanceBinding();
    }

    private void UpdateChildDistanceBinding() {
      BindingOperations.GetBindingExpression(childDistanceSlider, Slider.ValueProperty)?.UpdateSource();
    }

    private async Task LoadSelectedSample() {
      settings.IsBusy = true;
      suppressSettingsEvents = true;
      try {
        // Reset and disable the toolbar elements while the sample is being loaded.
        settings.SortingEnabled = false;
        settings.TabularGroupsEnabled = true;
        settings.ChildDistance = 0;

        // Load the selected sample and arrange it with tabular groups.
        await LoadSampleGraph(settings.SelectedSample);
        await RunHierarchicalLayoutWithTabularGroups();
        await GraphControl.FitGraphBounds();
      } finally {
        suppressSettingsEvents = false;
        settings.IsBusy = false;
      }
    }

    [STAThread]
    public static void Main() {
      Run<TabularGroupsDemo>();
    }
  }
}
