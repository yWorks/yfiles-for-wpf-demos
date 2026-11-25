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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Organic;

namespace yWorks.Demo.OrganicSubstructures
{
  /// <summary>
  ///   Type information associated with a node.
  /// </summary>
  internal sealed class NodeTag
  {
    public int Type { get; set; }

    public string? Image { get; set; }
  }

  /// <summary>
  ///   Settings stored with each sample graph.
  /// </summary>
  internal sealed class SampleSettingsData
  {
    public string CycleSubstructureStyle { get; set; } = "NONE";

    public string ChainSubstructureStyle { get; set; } = "NONE";

    public string StarSubstructureStyle { get; set; } = "NONE";

    public string ParallelSubstructureStyle { get; set; } = "NONE";

    public string TreeSubstructureStyle { get; set; } = "NONE";

    public string GroupSubstructures { get; set; } = "NO_GROUPS";

    public bool UseEdgeGrouping { get; set; }

    public bool ConsiderNodeTypes { get; set; }

    public bool ParallelSubstructureTypeSeparation { get; set; }

    public bool StarSubstructureTypeSeparation { get; set; }

    public bool OverrideStyles { get; set; }

    public bool RoundNodes { get; set; }

    public bool AllowItemCreation { get; set; }

    public bool AllowItemModification { get; set; }

    public bool AllowTypeChanges { get; set; }
  }

  /// <summary>
  ///   A sample graph and its displayed name.
  /// </summary>
  internal sealed record Sample(string Name, string Text);

  /// <summary>
  ///   A choice shown in an enum-backed combo box.
  /// </summary>
  internal sealed record EnumChoice<T>(T Value, string Text);

  /// <summary>
  ///   Shows how the organic layout algorithm handles substructures and node types.
  /// </summary>
  [Demo("organic-substructures", "layout", "Organic Substructures",
      Summary = "Shows organic layout, and its substructures and node types features.",
      Tags = ["layout", "organic", "substructure", "node type"],
      Keywords = ["layoutfeatures", "clustering", "grouping", "similar", "data management", "margin", "padding"])]
  public sealed partial class OrganicSubstructuresDemo
  {
    /// <summary>
    ///   The color sets for the eight different node types.
    /// </summary>
    private static readonly Palette[] nodeTypeColors = [
        Themes.Palette21,
        Themes.Palette22,
        Themes.Palette23,
        Themes.Palette15,
        Themes.Palette25,
        Themes.Palette11,
        Themes.Palette12,
        Themes.Palette14
    ];

    private static readonly Sample[] samples = [
        new("mixed_large", "Simple Mixed, Large"),
        new("mixed_small", "Simple Mixed, Small"),
        new("parallel", "Simple Parallel"),
        new("star", "Simple Star"),
        new("groups", "Simple Groups"),
        new("computer_network", "Computer Network")
    ];

    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly Settings settings = new();
    private readonly Dictionary<string, ImageSource> nodeImages = new(StringComparer.OrdinalIgnoreCase);
    private GraphEditorInputMode? graphEditorInputMode;
    private bool layoutRunning;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public OrganicSubstructuresDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphEditorInputMode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge,
          // Disable interactive label creation - labels are simply not in the focus of this demo.
          AllowAddLabel = false,
          // Disable interactive label editing - labels are simply not in the focus of this demo.
          AllowEditLabel = false
      };
      GraphControl.InputMode = graphEditorInputMode;

      // Enable undo/redo.
      Graph.SetUndoEngineEnabled(true);

      // Configure default visualizations for the graph.
      ConfigureGraph(Graph);

      // Initializes the context menu for changing a node's type.
      InitializeTypePanel();

      // Bind the buttons to their commands.
      InitializeUI();
    }

    /// <summary>
    ///   Calculates a new graph layout and optionally applies the new layout in an animated fashion.
    ///   This method also takes care of disabling the UI during layout calculations.
    /// </summary>
    private async Task RunLayoutAsync(bool animate) {
      if (layoutRunning || graphEditorInputMode is null) {
        return;
      }

      layoutRunning = true;
      SetUIDisabled(true);
      try {
        // The actual layout calculation.
        await RunLayoutCoreAsync(animate);
      } finally {
        layoutRunning = false;
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Calculates a new graph layout and optionally applies the new layout in an animated fashion.
    ///   This method creates and configures a new organic layout algorithm for this purpose.
    /// </summary>
    private async Task RunLayoutCoreAsync(bool animate) {
      // Configure the organic layout algorithm.
      var currentSample = sampleComboBox.SelectedItem as Sample;
      var layout = new OrganicLayout {
          // Configure some basic settings.
          Deterministic = true,
          DefaultMinimumNodeDistance = currentSample?.Name == "groups" ? 0 : 70,
          DefaultPreferredEdgeLength = 60,
          // Configure substructure styles (cycles, chains, parallel structures, star, tree).
          CycleSubstructureStyle = settings.CycleStyle,
          ChainSubstructureStyle = settings.ChainStyle,
          ParallelSubstructureStyle = settings.ParallelStyle,
          StarSubstructureStyle = settings.StarStyle,
          TreeSubstructureStyle = settings.TreeStyle,
          GroupSubstructureScope = settings.GroupScope,
          // Configure type separation for parallel and star substructures.
          ParallelSubstructureTypeSeparation = settings.ParallelTypeSeparation,
          StarSubstructureTypeSeparation = settings.StarTypeSeparation
      };

      // Configure data-driven features for the organic layout algorithm by using OrganicLayoutData.
      var layoutData = layout.CreateLayoutData(Graph);

      if (settings.UseEdgeGrouping) {
        // If desired, define edge grouping on the organic layout data.
        layoutData.SubstructureSourceGroupIds.Constant = "groupAll";
        layoutData.SubstructureTargetGroupIds.Constant = "groupAll";
      }

      if (settings.ConsiderNodeTypes) {
        // If types should be considered define a delegate on the respective layout data property
        // that queries the type from the node's tag.
        layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);
      }

      // Runs the layout algorithm and applies the result...
      if (animate) {
        // ...with a morph animation.
        await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(500), layoutData);
      } else {
        // ...without an animation.
        Graph.ApplyLayout(layout, layoutData);
        await GraphControl.FitGraphBounds();
      }
    }

    /// <summary>
    ///   Gets the type of the given node by querying it from the node's tag.
    /// </summary>
    private static int GetNodeType(INode node) {
      if (node.Tag is NodeTag tag) {
        return tag.Type;
      }
      return 0;
    }

    /// <summary>
    ///   Configures default visualizations for the given graph.
    /// </summary>
    /// <param name="graph">The demo's graph.</param>
    private static void ConfigureGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);

      // Use first type color for all interactively created nodes.
      graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle(nodeTypeColors[0]);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.GetDecorator().Nodes.GroupPaddingProvider.AddConstant(
          graph.IsGroupNode, new GroupPaddingProvider(new InsetsD(40)));

      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: false);
    }

    /// <summary>
    ///   Initializes the context menu for changing a node's type.
    /// </summary>
    private void InitializeTypePanel() {
      for (var type = 0; type < nodeTypeColors.Length; type++) {
        var currentType = type;
        var palette = nodeTypeColors[currentType];
        var button = new Button {
            Width = 28,
            Height = 26,
            Margin = new Thickness(2),
            Background = palette.Fill,
            BorderBrush = palette.Stroke,
            BorderThickness = new Thickness(1),
            ToolTip = $"Set selected nodes to type {currentType + 1}"
        };
        button.Click += async (_, _) => await ChangeSelectedNodesTypeAsync(currentType);
        typePalette.Children.Add(button);
      }

      GraphControl.Selection.ItemAdded += (_, _) => UpdateTypePanel();
      GraphControl.Selection.ItemRemoved += (_, _) => UpdateTypePanel();
      GraphControl.ViewportChanged += (_, _) => UpdateTypePanelLocation();
    }

    private async Task ChangeSelectedNodesTypeAsync(int type) {
      var selectedNodes = GraphControl.Selection.Nodes.Where(node => !Graph.IsGroupNode(node)).ToArray();
      foreach (var node in selectedNodes) {
        if (GetNodeType(node) != type) {
          SetNodeType(node, type);
        }
      }

      typePopup.IsOpen = false;
      if (selectedNodes.Length > 0) {
        await RunLayoutAsync(true);
      }
    }

    /// <summary>
    ///   Sets the type for the given node by updating the node's tag and the according style.
    ///   This function is invoked when the type of node is changed via the type panel.
    /// </summary>
    private void SetNodeType(INode node, int type) {
      // Set a new tag and style so that this change is easily undo-able.
      node.Tag = new NodeTag { Type = type };
      if (node.Style is ShapeNodeStyle shapeNodeStyle) {
        Graph.SetStyle(node, DemoStyles.CreateDemoShapeNodeStyle(shapeNodeStyle.Shape, nodeTypeColors[type]));
      } else {
        Graph.SetStyle(node, DemoStyles.CreateDemoNodeStyle(nodeTypeColors[type]));
      }
    }

    private void UpdateTypePanel() {
      var selectedNodes = GraphControl.Selection.Nodes.Where(node => !Graph.IsGroupNode(node)).ToArray();
      if (!settings.AllowTypeChanges || selectedNodes.Length == 0) {
        typePopup.IsOpen = false;
        return;
      }

      for (var type = 0; type < typePalette.Children.Count; type++) {
        if (typePalette.Children[type] is Button button) {
          button.BorderThickness = selectedNodes.Any(node => GetNodeType(node) == type)
              ? new Thickness(3)
              : new Thickness(1);
        }
      }

      typePopup.IsOpen = true;
      UpdateTypePanelLocation();
    }

    private void UpdateTypePanelLocation() {
      if (!typePopup.IsOpen || GraphControl.Selection.Nodes.Count == 0) {
        return;
      }

      var node = GraphControl.Selection.Nodes.First();
      var layout = node.Layout;
      var point = GraphControl.WorldToViewCoordinates(new PointD(layout.X + layout.Width / 2, layout.Y));
      typePopup.HorizontalOffset = Math.Max(4, point.X - 92);
      typePopup.VerticalOffset = point.Y < 112 ? point.Y + 18 : point.Y - 112;
    }

    /// <summary>
    ///   Loads a sample graph for testing the substructure and node types support of the organic layout
    ///   algorithm.
    /// </summary>
    private async Task LoadSampleAsync(Sample sample) {
      if (graphEditorInputMode is null || graphEditorInputMode.WaitInputMode.Waiting) {
        return;
      }

      SetUIDisabled(true);
      try {
        Graph.Clear();

        // Load sample data.
        BuildGraphFromJson($"{sample.Name}_sample.json", Graph, typeof(NodeTag));
        var sampleSettings = LoadSampleSettings(sample.Name);
        UpdateLayoutSettings(sampleSettings);

        // If required for the sample, override and set the node styles.
        if (sampleSettings.OverrideStyles) {
          UpdateNodeStyles(Graph, sampleSettings.RoundNodes);
        } else {
          UpdateImageNodeStyles(Graph);
        }

        // Update input mode setting depending on whether we are allowed to change the graph structure.
        graphEditorInputMode.AllowCreateNode = sampleSettings.AllowItemCreation;
        graphEditorInputMode.AllowCreateEdge = sampleSettings.AllowItemCreation;
        graphEditorInputMode.AllowDuplicate = sampleSettings.AllowItemCreation;
        graphEditorInputMode.AllowClipboardOperations = sampleSettings.AllowItemCreation;
        graphEditorInputMode.MoveSelectedItemsInputMode.Enabled = sampleSettings.AllowItemModification;
        graphEditorInputMode.DeletableItems = sampleSettings.AllowItemCreation
            ? GraphItemTypes.All
            : GraphItemTypes.None;
        graphEditorInputMode.ShowHandleItems = sampleSettings.AllowItemModification
            ? GraphItemTypes.All
            : GraphItemTypes.None;

        if (sampleSettings.AllowItemCreation) {
          // Update the default node style depending on the style of the first node.
          var refStyle = Graph.Nodes.FirstOrDefault()?.Style;
          if (refStyle is ShapeNodeStyle shapeNodeStyle) {
            Graph.NodeDefaults.Style = DemoStyles.CreateDemoShapeNodeStyle(shapeNodeStyle.Shape, nodeTypeColors[0]);
          }
        }

        // Calculate an initial arrangement for the new sample.
        await RunLayoutCoreAsync(false);

        // Enable undo/redo.
        Graph.SetUndoEngineEnabled(true);
        Graph.GetUndoEngine()?.Clear();
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Updates the node styles in the given graph depending on the type of each node.
    /// </summary>
    /// <param name="graph">The graph to update.</param>
    private static void UpdateNodeStyles(IGraph graph, bool roundNodes) {
      foreach (var node in graph.Nodes) {
        if (roundNodes && graph.IsGroupNode(node)) {
          var pen = new Pen(Themes.Palette23.Stroke, 5) {
              StartLineCap = PenLineCap.Square,
              EndLineCap = PenLineCap.Square
          };
          graph.SetStyle(node, new ShapeNodeStyle {
              Shape = ShapeNodeShape.Ellipse,
              Brush = Brushes.Transparent,
              Pen = (Pen) pen.GetAsFrozen()
          });
        } else if (node.Tag is NodeTag { Type: >= 0 } tag && tag.Type < nodeTypeColors.Length) {
          graph.SetStyle(node,
              roundNodes
                  ? DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse, nodeTypeColors[tag.Type])
                  : DemoStyles.CreateDemoNodeStyle(nodeTypeColors[tag.Type]));
        }
      }
    }

    private void UpdateImageNodeStyles(IGraph graph) {
      foreach (var node in graph.Nodes) {
        if (node.Tag is not NodeTag { Image: { Length: > 0 } imageName }) {
          continue;
        }

        if (!nodeImages.TryGetValue(imageName, out var image)) {
          using var stream = LoadAssemblyResource(imageName);
          var bitmap = new BitmapImage();
          bitmap.BeginInit();
          bitmap.CacheOption = BitmapCacheOption.OnLoad;
          bitmap.StreamSource = stream;
          bitmap.EndInit();
          bitmap.Freeze();
          image = bitmap;
          nodeImages.Add(imageName, image);
        }

        graph.SetStyle(node, new ImageNodeStyle(image) { KeepAspectRatio = true });
      }
    }

    /// <summary>
    ///   Updates the settings UI to match the given sample's default layout settings.
    /// </summary>
    private void UpdateLayoutSettings(SampleSettingsData sampleSettings) {
      settings.UpdateFrom(new SampleLayoutSettings(
          ParseSubstructureStyle<CycleSubstructureStyle>(sampleSettings.CycleSubstructureStyle),
          ParseSubstructureStyle<ChainSubstructureStyle>(sampleSettings.ChainSubstructureStyle),
          ParseSubstructureStyle<StarSubstructureStyle>(sampleSettings.StarSubstructureStyle),
          ParseSubstructureStyle<ParallelSubstructureStyle>(sampleSettings.ParallelSubstructureStyle),
          ParseSubstructureStyle<TreeSubstructureStyle>(sampleSettings.TreeSubstructureStyle),
          ParseGroupScope(sampleSettings.GroupSubstructures), sampleSettings.UseEdgeGrouping,
          sampleSettings.ConsiderNodeTypes, sampleSettings.ParallelSubstructureTypeSeparation,
          sampleSettings.StarSubstructureTypeSeparation, sampleSettings.AllowTypeChanges));
    }

    private static T ParseSubstructureStyle<T>(string value) where T : struct, Enum {
      var enumName = value.Equals("IGNORE", StringComparison.OrdinalIgnoreCase)
          ? "None"
          : value.Replace("_", string.Empty, StringComparison.Ordinal);
      return Enum.Parse<T>(enumName, true);
    }

    private static GroupSubstructureScope ParseGroupScope(string value) =>
        value switch {
            "ALL" => GroupSubstructureScope.AllGroups,
            "WITHOUT_EDGES" => GroupSubstructureScope.GroupsWithoutEdges,
            "WITHOUT_INTER_EDGES" => GroupSubstructureScope.GroupsWithoutInterEdges,
            _ => GroupSubstructureScope.NoGroups
        };

    private SampleSettingsData LoadSampleSettings(string sampleName) {
      using var stream = LoadAssemblyResource($"{sampleName}.json");
      return JsonSerializer.Deserialize<SampleSettingsData>(stream, jsonOptions) ??
          throw new InvalidOperationException($"Could not load settings for sample {sampleName}.");
    }

    /// <summary>
    ///   Configures the settings choices and binds actions to the demo's UI controls.
    /// </summary>
    private void InitializeUI() {
      foreach (var sample in samples) {
        sampleComboBox.Items.Add(sample);
      }

      cycleStyleComboBox.ItemsSource = CreateEnumChoices(CycleSubstructureStyle.None, CycleSubstructureStyle.Circular);
      chainStyleComboBox.ItemsSource = CreateEnumChoices(ChainSubstructureStyle.None, ChainSubstructureStyle.Disk,
          ChainSubstructureStyle.Rectangular, ChainSubstructureStyle.StraightLine);
      starStyleComboBox.ItemsSource = CreateEnumChoices(StarSubstructureStyle.None, StarSubstructureStyle.Circular,
          StarSubstructureStyle.Radial, StarSubstructureStyle.SeparatedRadial);
      parallelStyleComboBox.ItemsSource = CreateEnumChoices(ParallelSubstructureStyle.None,
          ParallelSubstructureStyle.Radial, ParallelSubstructureStyle.Rectangular,
          ParallelSubstructureStyle.StraightLine);
      treeStyleComboBox.ItemsSource = CreateEnumChoices(TreeSubstructureStyle.None, TreeSubstructureStyle.RadialTree,
          TreeSubstructureStyle.Oriented, TreeSubstructureStyle.Radial);
      groupScopeComboBox.ItemsSource = CreateEnumChoices(GroupSubstructureScope.NoGroups,
          GroupSubstructureScope.AllGroups, GroupSubstructureScope.GroupsWithoutEdges,
          GroupSubstructureScope.GroupsWithoutInterEdges);
      settingsPanel.DataContext = settings;

      settings.SettingChanged += (_, _) => _ = RunLayoutAsync(true);
      settings.PropertyChanged += Settings_PropertyChanged;
      sampleComboBox.SelectionChanged += async (_, _) => {
        if (sampleComboBox.SelectedItem is Sample sample) {
          await LoadSampleAsync(sample);
        }
      };
      applyLayoutButton.Click += async (_, _) => await RunLayoutAsync(true);

      // Selecting the first item loads the initial sample graph.
      sampleComboBox.SelectedIndex = 0;
    }

    private static EnumChoice<T>[] CreateEnumChoices<T>(params T[] values) where T : struct, Enum => [
        .. values.Select(value => new EnumChoice<T>(value, GetEnumText(value.ToString())))
    ];

    private static string GetEnumText(string value) =>
        value switch {
            "None" or "NoGroups" => "Ignore",
            "StraightLine" => "Straight-Line",
            "SeparatedRadial" => "Separated Radial",
            "RadialTree" => "Radial Tree",
            "AllGroups" => "All Groups",
            "GroupsWithoutEdges" => "Groups without edges",
            "GroupsWithoutInterEdges" => "Groups without inter-edges",
            _ => value
        };

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof(Settings.IsBusy)) {
        interactionPanel.IsEnabled = !settings.IsBusy;
        demoToolBar.IsEnabled = !settings.IsBusy;
      }
      if (e.PropertyName == nameof(Settings.AllowTypeChanges)) {
        UpdateTypePanel();
      }
    }

    /// <summary>
    ///   Disables the UI while a layout or sample load is running.
    /// </summary>
    /// <param name="disabled">The disabled state to set.</param>
    private void SetUIDisabled(bool disabled) {
      if (graphEditorInputMode is not null) {
        graphEditorInputMode.WaitInputMode.Waiting = disabled;
      }
      settings.IsBusy = disabled;
    }

    [STAThread]
    private static void Main() {
      Run<OrganicSubstructuresDemo>();
    }
  }
}
