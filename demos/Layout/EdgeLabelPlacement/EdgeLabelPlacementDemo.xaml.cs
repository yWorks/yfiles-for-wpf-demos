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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;

namespace yWorks.Demo.EdgeLabelPlacement
{
  /// <summary>
  ///   This demo shows how to place edge labels using <see cref="EdgeLabelPreferredPlacement"/> together
  ///   with a generic labeling algorithm or a layout algorithm that supports integrated edge labeling.
  /// </summary>
  [Demo("edge-label-placement", "layout", "Edge Label Placement",
      Summary = "Shows how to place edge labels at the preferred location with a labeling algorithm.",
      Tags = ["label placement"], Keywords = [
          "layoutfeatures", "integrated", "texts", "generic labeling", "tree", "hierarchical",
          "orthogonal", "edge router", "move", "padding"
      ])]
  public sealed partial class EdgeLabelPlacementDemo
  {
    /// <summary>
    ///   The mapper which provides a PreferredPlacementDescriptor for each edge label.
    /// </summary>
    /// <remarks>
    ///   This mapper is used by the layout algorithms to consider the preferred placement for the edge labels.
    /// </remarks>
    private readonly Dictionary<ILabel, EdgeLabelPreferredPlacement> descriptorMapper = new();

    private bool initialized;

    public Settings Settings { get; } = new();

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      InitializeGraph();
      InitializeStyles();
      InitializeInputMode();
      InitializeOptions();

      // build the graph from the data set
      BuildGraphFromJson("graph-data.json", Graph);

      // layout and center the graph
      Graph.ApplyLayout(CreateHierarchicalLayout(LayoutOrientation.TopToBottom));
      GraphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      Graph.SetUndoEngineEnabled(true);
      initialized = true;
    }

    /// <summary>
    ///   Does the label placement using the selected labeling/layout algorithm. Before this, the
    ///   preferred placement descriptor of the labels is set according to the settings.
    ///   <see cref="UpdateLabelValues"/>
    /// </summary>
    /// <param name="fitViewToContent">Whether to animate the viewport.</param>
    private async Task DoLayout(bool fitViewToContent) {
      var inputMode = (GraphEditorInputMode) GraphControl.InputMode!;
      if (inputMode.WaitInputMode.Waiting) {
        return;
      }

      SetUIDisabled(true);

      try {
        var layout = Settings.SelectedLayout?.Algorithm ?? CreateHierarchicalLayout(LayoutOrientation.TopToBottom);

        // provide preferred placement data to the layout algorithm
        var labeling = new GenericLabeling();
        var labelingData = labeling.CreateLayoutData(Graph);
        labelingData.EdgeLabelPreferredPlacements.Dictionary = descriptorMapper;

        // fix node port stage is used to keep the bounding box of the graph in the view port
        var layoutStage = new LayoutAnchoringStage(layout);
        var anchoringData = layoutStage.CreateLayoutData(Graph);
        anchoringData.NodeAnchoringPolicies.Constant = LayoutAnchoringPolicy.Center;
        var layoutData = anchoringData.CombineWith(labelingData);

        // initialize layout executor
        var layoutExecutor = new LayoutExecutor(GraphControl, layoutStage) {
            LayoutData = layoutData,
            AnimationDuration = TimeSpan.FromMilliseconds(500),
            AnimateViewport = fitViewToContent
        };

        // apply layout
        await layoutExecutor.Start();
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Disables the layout controls and input mode while the layout is running.
    /// </summary>
    /// <param name="disabled">true if the controls should be disabled, false otherwise.</param>
    private void SetUIDisabled(bool disabled) {
      ((GraphEditorInputMode) GraphControl.InputMode!).WaitInputMode.Waiting = disabled;
      Settings.IsLayoutEnabled = !disabled;
      layoutComboBox.IsEnabled = !disabled;
    }

    /// <summary>
    ///   Called whenever a property of the settings has changed.
    /// </summary>
    private async void Settings_SettingChanged(object? sender, SettingsChangedEventArgs e) {
      if (!initialized) {
        return;
      }

      if (e.PropertyName == nameof(Settings.SelectedLayout)) {
        await DoLayout(true);
        return;
      }

      if (e.PropertyName == nameof(Settings.DistanceToEdgeText) &&
          double.TryParse(Settings.DistanceToEdgeText, out var distance) && distance > 200) {
        MessageBox.Show("Distance cannot be larger than 200.", "Edge Label Placement", MessageBoxButton.OK,
            MessageBoxImage.Information);
        Settings.DistanceToEdgeText = "-1";
        return;
      }
      if (e.PropertyName == nameof(Settings.AngleText) && double.TryParse(Settings.AngleText, out var enteredAngle) &&
          (enteredAngle <= -360 || enteredAngle >= 360)) {
        Settings.AngleText = (enteredAngle % 360).ToString(CultureInfo.CurrentCulture);
        return;
      }

      UpdateLabelValues(GetAffectedLabels(), e.PropertyName);
      await DoLayout(false);
    }

    /// <summary>
    ///   Updates the preferred placement descriptors for all affected labels.
    ///   Affected labels are part of the selection or all labels if no label is selected.
    /// </summary>
    /// <param name="labels">The affected labels.</param>
    /// <param name="propertyName">The setting that changed.</param>
    private void UpdateLabelValues(IEnumerable<ILabel> labels, string propertyName) {
      foreach (var edgeLabel in labels) {
        if (!descriptorMapper.TryGetValue(edgeLabel, out var descriptor)) {
          descriptor = new EdgeLabelPreferredPlacement();
          descriptorMapper[edgeLabel] = descriptor;
        }

        switch (propertyName) {
          case nameof(Settings.LabelText):
            Graph.SetLabelText(edgeLabel, Settings.LabelText);
            break;
          case nameof(Settings.Placement) when Settings.Placement.HasValue:
            descriptor.PlacementAlongEdge = Settings.Placement.Value;
            break;
          case nameof(Settings.Side) when Settings.Side.HasValue:
            descriptor.EdgeSide = Settings.Side.Value;
            break;
          case nameof(Settings.SideReference) when Settings.SideReference.HasValue:
            descriptor.SideReference = Settings.SideReference.Value;
            break;
          case nameof(Settings.AngleReference) when Settings.AngleReference.HasValue:
            descriptor.AngleReference = Settings.AngleReference.Value;
            break;
          case nameof(Settings.AngleRotation) when Settings.AngleRotation.HasValue:
            descriptor.AngleRotationOnRightSide = Settings.AngleRotation.Value;
            break;
          case nameof(Settings.AddHalfRotationOnRightSide):
            descriptor.AddHalfRotationOnRightSide = Settings.AddHalfRotationOnRightSide;
            break;
          case nameof(Settings.AngleText) when double.TryParse(Settings.AngleText, out var angle):
            descriptor.Angle = ToRadians(angle);
            break;
          case nameof(Settings.DistanceToEdgeText) when double.TryParse(Settings.DistanceToEdgeText, out var distance):
            descriptor.DistanceToEdge = distance;
            break;
        }
      }
    }

    /// <summary>
    ///   Updates the settings when the selection has changed. The options show the descriptor values
    ///   for all selected labels or an undefined value if the selected labels do not agree.
    /// </summary>
    private void UpdateLabelProperties(IEnumerable<ILabel> labels) {
      var affected = labels.ToList();
      if (affected.Count == 0) {
        return;
      }

      var descriptors = affected.Select(label => descriptorMapper.TryGetValue(label, out var descriptor)
          ? descriptor
          : new EdgeLabelPreferredPlacement()).ToArray();
      Settings.UpdateFrom(affected, descriptors);
    }

    /// <summary>
    ///   Adds preferred placement information to each new label.
    /// </summary>
    private void InitializeGraph() {
      Graph.LabelAdded += (_, e) => descriptorMapper[e.Item] = new EdgeLabelPreferredPlacement();
      Graph.LabelRemoved += (_, e) => descriptorMapper.Remove(e.Item);
    }

    /// <summary>
    ///   Initializes node, edge and label styles that are applied when the graph is created.
    /// </summary>
    private void InitializeStyles() {
      DemoStyles.InitDemoStyles(Graph);
      Graph.NodeDefaults.Size = new SizeD(50, 30);
      Graph.EdgeDefaults.Labels.LayoutParameter = FreeEdgeLabelModel.Instance.CreateParameter();

      // allow full 360 degrees rotation for edge labels
      ((LabelStyle) Graph.EdgeDefaults.Labels.Style).AutoFlip = false;
    }

    /// <summary>
    ///   Configures the input mode such that only labels can be added or removed from the graph.
    ///   Also, selecting labels updates the settings panel.
    /// </summary>
    private void InitializeInputMode() {
      var inputMode = new GraphEditorInputMode {
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowCreateBend = false,
          DeletableItems = GraphItemTypes.Label,
          LabelEditableItems = GraphItemTypes.Edge,
          MarqueeSelectableItems = GraphItemTypes.Label,
          SelectableItems = GraphItemTypes.Label | GraphItemTypes.Edge,
          FocusableItems = GraphItemTypes.None
      };
      inputMode.MultiSelectionFinished += (_, _) => UpdateLabelProperties(GetAffectedLabels());
      GraphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Initializes the properties in the settings panel with options and default values.
    /// </summary>
    private void InitializeOptions() {
      Settings.SettingChanged += Settings_SettingChanged;
      GraphControl.Selection.ItemAdded += (_, _) => UpdateLabelProperties(GetAffectedLabels());
      GraphControl.Selection.ItemRemoved += (_, _) => UpdateLabelProperties(GetAffectedLabels());
    }

    /// <summary>
    ///   Creates a configured <see cref="HierarchicalLayout"/>.
    /// </summary>
    public static HierarchicalLayout CreateHierarchicalLayout(LayoutOrientation orientation) =>
        new() { LayoutOrientation = orientation };

    public static ILayoutAlgorithm CreateTreeLayout() {
      var layout = new TreeLayout();
      layout.ParallelEdgeRouter.Enabled = false;
      layout.TreeReductionStage.NonTreeEdgeRouter = new EdgeRouter();
      layout.TreeReductionStage.NonTreeEdgeLabeling = new GenericLabeling();
      return layout;
    }

    public static ILayoutAlgorithm CreateOrthogonalLayout() => new OrthogonalLayout();

    public static ILayoutAlgorithm CreateOrganicLayout() =>
        new OrganicLayout {
            EdgeLabelPlacement = Layout.EdgeLabelPlacement.Integrated,
            Deterministic = true,
            DefaultPreferredEdgeLength = 60,
            DefaultMinimumNodeDistance = 20
        };

    private static double ToRadians(double degrees) => degrees / 180 * Math.PI;

    private async void LayoutButton_OnClick(object sender, RoutedEventArgs e) => await DoLayout(true);

    /// <summary>
    ///   Retrieves a collection of labels affected by option changes. These are selected labels or,
    ///   if none are selected, all edge labels in the graph.
    /// </summary>
    private IEnumerable<ILabel> GetAffectedLabels() {
      var selectedLabels = GraphControl.Selection.Labels.ToList();
      return selectedLabels.Count > 0 ? selectedLabels : [.. Graph.Edges.SelectMany(edge => edge.Labels)];
    }

    public EdgeLabelPlacementDemo() {
      InitializeComponent();
      DataContext = Settings;
    }

    [STAThread]
    private static void Main() => Run<EdgeLabelPlacementDemo>();
  }
}
