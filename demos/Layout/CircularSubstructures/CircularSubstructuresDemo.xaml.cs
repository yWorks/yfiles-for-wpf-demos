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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout.Circular;

namespace yWorks.Demo.CircularSubstructures
{
  /// <summary>
  ///   Shows circular layout, and its substructures and node types features.
  /// </summary>
  [Demo("circular-substructures", "layout", "Circular Substructures",
      Summary = "Shows circular layout, and its substructures and node types features.",
      Tags = ["layout", "circular", "substructure", "node type"], Keywords = ["layoutfeatures"])]
  public sealed partial class CircularSubstructuresDemo
  {
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly Settings settings = new();
    private GraphEditorInputMode? graphEditorInputMode;
    private bool updatingSettings;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public CircularSubstructuresDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      graphEditorInputMode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge,
          // Disable interactive label creation - labels are simply not in the focus of this demo.
          AllowAddLabel = false,
          // Disable interactive label editing - labels are simply not in the focus of this demo.
          AllowEditLabel = false
      };
      GraphControl.InputMode = graphEditorInputMode;

      // Set some default styles for the nodes and edges.
      InitializeStyles(Graph);

      // Enable undo and redo.
      Graph.SetUndoEngineEnabled(true);

      // Initialize the palette for changing the type of selected nodes.
      TypesPopup.InitializeTypePalette(GraphControl, typePalette, () => _ = RunLayoutAsync(true));

      InitializeUI();
    }

    /// <summary>
    ///   Calculates a new graph layout and optionally applies the new layout in an animated fashion.
    ///   This method also takes care of disabling the UI during layout calculations.
    /// </summary>
    private async Task RunLayoutAsync(bool animate) {
      if (graphEditorInputMode is null || graphEditorInputMode.WaitInputMode.Waiting) {
        return;
      }

      graphEditorInputMode.WaitInputMode.Waiting = true;
      settings.IsBusy = true;
      try {
        await ConfigureLayout.RunLayoutCoreAsync(GraphControl, settings, animate);
      } finally {
        graphEditorInputMode.WaitInputMode.Waiting = false;
        settings.IsBusy = false;
      }
    }

    /// <summary>
    ///   Configures default visualizations for the given graph.
    /// </summary>
    private static void InitializeStyles(IGraph graph) {
      // Use the first type color for all interactively created nodes.
      graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle(TypesPopup.NodeTypeColors[0]);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Size = new SizeD(40, 40);

      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: false);
    }

    /// <summary>
    ///   Loads a sample graph for testing the star substructure and node types support of the circular
    ///   layout algorithm.
    /// </summary>
    private async Task LoadSampleAsync(string sample) {
      if (graphEditorInputMode is null || graphEditorInputMode.WaitInputMode.Waiting) {
        return;
      }

      graphEditorInputMode.WaitInputMode.Waiting = true;
      settings.IsBusy = true;
      try {
        Graph.Clear();

        // Load sample data.
        var sampleData = LoadGraphData(sample == "singleStar" ? "single-star-data.json" : "multiple-stars-data.json");
        var builder = new GraphBuilder(Graph);
        var nodesSource = builder.CreateNodesSource(sampleData.Nodes, node => node.Id);
        nodesSource.NodeCreator.TagProvider = node => node.Tag;
        nodesSource.NodeCreator.StyleProvider = node =>
            DemoStyles.CreateDemoNodeStyle(TypesPopup.NodeTypeColors[GetNodeType(node.Tag)]);
        builder.CreateEdgesSource(sampleData.Edges, edge => edge.Source, edge => edge.Target);
        builder.BuildGraph();

        var layoutSettings = LoadLayoutSettings(
            sample == "singleStar" ? "single-star-settings.json" : "multiple-stars-settings.json");
        UpdateLayoutSettings(layoutSettings);

        // Calculate an initial arrangement for the new sample.
        await ConfigureLayout.RunLayoutCoreAsync(GraphControl, settings, false);

        // Clear the undo engine when a new sample is loaded.
        Graph.GetUndoEngine()?.Clear();
      } finally {
        graphEditorInputMode.WaitInputMode.Waiting = false;
        settings.IsBusy = false;
      }
    }

    private GraphData LoadGraphData(string resourceName) {
      using var stream = LoadAssemblyResource(resourceName);
      return JsonSerializer.Deserialize<GraphData>(stream, JsonOptions) ??
          throw new InvalidOperationException($"Could not load graph data from {resourceName}.");
    }

    private SampleSettingsData LoadLayoutSettings(string resourceName) {
      using var stream = LoadAssemblyResource(resourceName);
      return JsonSerializer.Deserialize<SampleSettingsData>(stream, JsonOptions) ??
          throw new InvalidOperationException($"Could not load layout settings from {resourceName}.");
    }

    /// <summary>
    ///   Updates the settings UI to match the given sample's default layout settings.
    /// </summary>
    private void UpdateLayoutSettings(SampleSettingsData sampleSettings) {
      updatingSettings = true;
      try {
        settings.StarSubstructureStyle = ParseStarStyle(sampleSettings.StarSubstructureStyle);
        settings.ConsiderNodeTypes = sampleSettings.ConsiderNodeTypes;
        settings.StarSubstructureTypeSeparation = sampleSettings.StarSubstructureTypeSeparation;
      } finally {
        updatingSettings = false;
      }
    }

    private static StarSubstructureStyle ParseStarStyle(string starStyle) {
      return starStyle switch {
          "RADIAL" => StarSubstructureStyle.Radial,
          "SEPARATED_RADIAL" => StarSubstructureStyle.SeparatedRadial,
          _ => StarSubstructureStyle.None
      };
    }

    private static int GetNodeType(NodeTag? tag) => tag?.Type ?? 0;

    private void InitializeUI() {
      settingsPanel.DataContext = settings;
      starStyleComboBox.ItemsSource = Enum.GetValues<StarSubstructureStyle>();
      settings.SettingChanged += Settings_SettingChanged;
      settings.PropertyChanged += (_, e) => {
        if (e.PropertyName == nameof(Settings.IsBusy)) {
          interactionPanel.IsEnabled = !settings.IsBusy;
          demoToolBar.IsEnabled = !settings.IsBusy;
        }
      };

      sampleComboBox.SelectionChanged += SampleComboBox_SelectionChanged;
      applyLayoutButton.Click += async (_, _) => await RunLayoutAsync(true);

      // Selecting the first item loads the initial sample graph.
      sampleComboBox.SelectedIndex = 0;
    }

    private void Settings_SettingChanged(object? sender, EventArgs e) {
      if (!updatingSettings) {
        _ = RunLayoutAsync(true);
      }
    }

    private void SampleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (sampleComboBox.SelectedItem is ComboBoxItem { Tag: string sample }) {
        _ = LoadSampleAsync(sample);
      }
    }

    [STAThread]
    private static void Main() {
      Run<CircularSubstructuresDemo>();
    }
  }
}
