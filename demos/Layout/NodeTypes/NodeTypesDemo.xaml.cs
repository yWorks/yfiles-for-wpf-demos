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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Radial;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;
using OrganicCycleSubstructureStyle = yWorks.Layout.Organic.CycleSubstructureStyle;
using OrganicParallelSubstructureStyle = yWorks.Layout.Organic.ParallelSubstructureStyle;
using OrganicStarSubstructureStyle = yWorks.Layout.Organic.StarSubstructureStyle;

namespace yWorks.Demo.NodeTypes
{
  /// <summary>
  ///   Describes a sample graph and the layout algorithm to run on it.
  /// </summary>
  internal sealed record Sample(
      string Name,
      ILayoutAlgorithm Layout,
      LayoutData<INode, IEdge, ILabel, ILabel> LayoutData,
      string SampleDataResource,
      bool Directed = false);

  /// <summary>
  ///   Shows how different layout algorithms handle nodes with types.
  /// </summary>
  [Demo("node-types", "layout", "Node Types",
      Summary = "Shows how different layout algorithms handle nodes with types.", Tags = ["layout", "node type"],
      Keywords = [
          "layoutfeatures", "types", "tree", "hierarchical", "organic", "components", "circular", "balloon",
          "compact disk", "data analysis"
      ])]
  public sealed partial class NodeTypesDemo
  {
    /// <summary>
    ///   The samples that can be selected in this demo.
    /// </summary>
    private readonly List<Sample> samples = [];

    /// <summary>
    ///   The color sets for the three different node types.
    /// </summary>
    private static readonly Palette[] typeColors = [Themes.Palette21, Themes.Palette22, Themes.Palette23];

    private GraphEditorInputMode? graphEditorInputMode;
    private bool isBusy;

    /// <summary>
    ///   The graph control holding the graph shown in this demo.
    /// </summary>
    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public NodeTypesDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Runs this demo.
    /// </summary>
    protected override void Initialize() {
      ConfigureGraphComponent();
      CreateSamples();
      PrepareSampleList();
      InitializeTypePanel();
      InitializeUI();

      // Load the initial sample without the comparison preview.
      _ = LoadSampleAsync(false);
    }

    /// <summary>
    ///   Gets the type of the given node from its tag.
    /// </summary>
    /// <param name="node">The node to query the type for.</param>
    private static int GetNodeType(INode node) {
      // The implementation for this demo assumes that the node tag has a type property
      // (an integer). Note though that for the layout's node type feature arbitrary objects from
      // arbitrary sources may be used. In other applications one could, for example, use the color
      // or shape of the node's style as type or its label text, please see the following snippets:

      // Use the fill color of the node assuming that its style is an instance of ShapeNodeStyle
      // (does not work in this demo!)
      // return ((ShapeNodeStyle) node.Style).Brush;

      // Use the label text of the node as type
      // return node.Labels.Count > 0 ? node.Labels[0].Text : null;

      if (node.Tag is NodeData { Tag: { } dataTag }) {
        return dataTag.Type;
      }
      return 0;
    }

    /// <summary>
    ///   Creates and configures the <see cref="HierarchicalLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateHierarchicalSample(IGraph graph) {
      // Create hierarchical layout - no further settings on the algorithm are necessary to support types.
      var layout = new HierarchicalLayout();

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Hierarchical", layout, layoutData, "hierarchical.json", true);
    }

    /// <summary>
    ///   Creates and configures the <see cref="OrganicLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateOrganicSample(IGraph graph) {
      // Create an organic layout wrapped by an organic edge router.
      var organicLayout = new OrganicLayout {
          Deterministic = true,
          DefaultMinimumNodeDistance = 30,
          StarSubstructureStyle = OrganicStarSubstructureStyle.Circular,
          StarSubstructureTypeSeparation = false,
          ParallelSubstructureStyle = OrganicParallelSubstructureStyle.Rectangular,
          ParallelSubstructureTypeSeparation = false,
          CycleSubstructureStyle = OrganicCycleSubstructureStyle.Circular
      };
      var layout = new OrganicEdgeRouter(organicLayout);

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = organicLayout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Organic", layout, layoutData, "organic.json");
    }

    /// <summary>
    ///   Creates and configures the <see cref="TreeLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateTreeSample(IGraph graph) {
      // Create a tree layout.
      var layout = new TreeLayout { DefaultSubtreePlacer = new CompactSubtreePlacer() };
      layout.TreeReductionStage.NonTreeEdgeRouter = new EdgeRouter();

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Tree", layout, layoutData, "resources.tree.json", true);
    }

    /// <summary>
    ///   Creates and configures the <see cref="CircularLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateCircularSample(IGraph graph) {
      // Create a circular layout. Its layout data specifies node types for the ordering on the circle.
      var layout = new CircularLayout();

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Circular", layout, layoutData, "circular.json");
    }

    /// <summary>
    ///   Creates and configures the <see cref="ComponentLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateComponentSample(IGraph graph) {
      // Create a component layout with default settings.
      var layout = new ComponentLayout();

      // Note that with the default component arrangement style the types of nodes have an influence
      // already - however, if in a row only components with nodes of the same type should be
      // allowed, this can be achieved by specifying a type-separated component arrangement style.
      // layout.Style = ComponentArrangementStyle.MultiRowsTypeSeparated;

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Component", layout, layoutData, "component.json");
    }

    /// <summary>
    ///   Creates and configures the <see cref="RadialTreeLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateRadialTreeSample(IGraph graph) {
      // Create a radial tree layout.
      var layout = new RadialTreeLayout();

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Radial Tree", layout, layoutData, "radial-tree.json", true);
    }

    /// <summary>
    ///   Creates and configures the <see cref="CompactDiskLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateCompactDiskSample(IGraph graph) {
      // Create a compact disk layout with a little additional node distance (since the nodes
      // are not circles and this algorithm treats them as such).
      var layout = new CompactDiskLayout { MinimumNodeDistance = 20 };

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Compact Disk", layout, layoutData, "compact-disk.json");
    }

    /// <summary>
    ///   Creates and configures the <see cref="RadialLayout"/> and its layout data
    ///   such that node types are considered.
    /// </summary>
    private static Sample CreateRadialSample(IGraph graph) {
      // Create a radial layout with a little additional layer distance.
      var layout = new RadialLayout { MinimumLayerDistance = 150 };

      // The node types are specified as a delegate on the node types mapping of the layout data.
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeTypes.MapperFunction = node => GetNodeType(node);

      return new Sample("Radial Layout", layout, layoutData, "radial.json");
    }

    private void CreateSamples() {
      samples.Add(CreateHierarchicalSample(Graph));
      samples.Add(CreateOrganicSample(Graph));
      samples.Add(CreateTreeSample(Graph));
      samples.Add(CreateRadialTreeSample(Graph));
      samples.Add(CreateCircularSample(Graph));
      samples.Add(CreateRadialSample(Graph));
      samples.Add(CreateComponentSample(Graph));
      samples.Add(CreateCompactDiskSample(Graph));
    }

    /// <summary>
    ///   Applies the current layout style to the current graph.
    /// </summary>
    private async Task ApplyCurrentLayoutAsync(bool animate, bool considerTypes) {
      var sample = samples[sampleComboBox.SelectedIndex];
      if (animate) {
        if (considerTypes) {
          await GraphControl.ApplyLayoutAnimated(sample.Layout, TimeSpan.FromMilliseconds(1000), sample.LayoutData);
        } else {
          await GraphControl.ApplyLayoutAnimated(sample.Layout, TimeSpan.FromMilliseconds(1000));
        }
      } else {
        Graph.ApplyLayout(sample.Layout);
      }
    }

    /// <summary>
    ///   Loads the sample currently selected in the combo box, populates the graph and applies
    ///   the respective layout algorithm.
    /// </summary>
    private async Task LoadSampleAsync(bool previewWithoutNodeTypes) {
      if (isBusy || sampleComboBox.SelectedIndex < 0) {
        return;
      }

      isBusy = true;
      SetUIDisabled(true);
      try {
        var sample = samples[sampleComboBox.SelectedIndex];
        var edgeStyle = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: sample.Directed);
        Graph.EdgeDefaults.Style = edgeStyle;

        // Use the Demo helper to load sample data from the embedded JSON resource.
        BuildGraphFromJson(sample.SampleDataResource, Graph, typeof(NodeData));
        foreach (var node in Graph.Nodes) {
          // Create node style depending on the type tag.
          if (node.Tag is NodeData { Tag : null }) {
            Graph.SetStyle(node, DemoStyles.CreateDemoNodeStyle());
          } else {
            Graph.SetStyle(node, DemoStyles.CreateDemoNodeStyle(typeColors[GetNodeType(node)]));
          }
        }

        await ArrangeGraphCoreAsync(false, previewWithoutNodeTypes);

        // Make the sample change non-undoable.
        Graph.GetUndoEngine()?.Clear();
      } finally {
        SetUIDisabled(false);
        isBusy = false;
      }
    }

    /// <summary>
    ///   Arranges the graph with an optional preview arrangement that shows the layout result when
    ///   ignoring the node types.
    /// </summary>
    /// <param name="animate">Whether the layout should be animated.</param>
    /// <param name="previewWithoutNodeTypes">
    ///   Whether the layout should first run ignoring node types to make
    ///   the difference easily visible.
    /// </param>
    private async Task ArrangeGraphAsync(bool animate, bool previewWithoutNodeTypes) {
      if (isBusy || sampleComboBox.SelectedIndex < 0) {
        return;
      }

      isBusy = true;
      SetUIDisabled(true);
      try {
        await ArrangeGraphCoreAsync(animate, previewWithoutNodeTypes);
      } finally {
        SetUIDisabled(false);
        isBusy = false;
      }
    }

    private async Task ArrangeGraphCoreAsync(bool animate, bool previewWithoutNodeTypes) {
      // Run a layout without considering node types.
      if (previewWithoutNodeTypes) {
        UpdateLayoutPopup(true, "Node types are not considered");
        // Apply the current layout that is associated with the newly loaded sample.
        await ApplyCurrentLayoutAsync(animate, false);
        await GraphControl.FitGraphBounds();
        // Add some delay.
        await Task.Delay(2000);
        UpdateLayoutPopup(true, "Node types are considered");
      }

      // Run a layout when considering node types.
      await ApplyCurrentLayoutAsync(true, true);

      await GraphControl.FitGraphBounds();
      UpdateLayoutPopup(false);
    }

    /// <summary>
    ///   Sets up a <see cref="GraphEditorInputMode"/> and configures the defaults for the graph control.
    /// </summary>
    private void ConfigureGraphComponent() {
      graphEditorInputMode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge,
          NodeCreator = (_, graph, location, parent) => {
            var node = graph.CreateNode(parent, RectD.FromCenter(location, graph.NodeDefaults.Size));
            SetNodeType(node, 0);
            return node;
          }
      };
      GraphControl.InputMode = graphEditorInputMode;

      Graph.NodeDefaults.ShareStyleInstance = false;
      Graph.NodeDefaults.Size = new SizeD(40, 40);
      Graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle();
      Graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(showTargetArrow: false);

      Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Specifies the type of the given node by storing it in the node's tag.
    ///   This function is used as a callback by the type panel when changing the type.
    /// </summary>
    private void SetNodeType(INode node, int type) {
      // Set a new tag and style so this change is easily undoable.
      node.Tag = new NodeData { Tag = new NodeTag { Type = type } };
      Graph.SetStyle(node, DemoStyles.CreateDemoNodeStyle(typeColors[type]));
    }

    /// <summary>
    ///   Initializes the type panel that allows for changing a node's type.
    /// </summary>
    private void InitializeTypePanel() {
      var buttons = new[] { type0Button, type1Button, type2Button };
      for (var type = 0; type < buttons.Length; type++) {
        var currentType = type;
        var palette = typeColors[currentType];
        buttons[currentType].Background = palette.Fill;
        buttons[currentType].BorderBrush = palette.Stroke;
        buttons[currentType].Click += async (_, _) => await ChangeSelectedNodesTypeAsync(currentType);
      }

      GraphControl.Selection.ItemAdded += (_, _) => UpdateTypePanel();
      GraphControl.Selection.ItemRemoved += (_, _) => UpdateTypePanel();
      GraphControl.ViewportChanged += (_, _) => UpdateTypePanelLocation();
    }

    private async Task ChangeSelectedNodesTypeAsync(int type) {
      var selectedNodes = GraphControl.Selection.Nodes.ToArray();
      foreach (var node in selectedNodes) {
        if (node.Tag is NodeData { Tag: { } dataTag } && dataTag.Type == type) {
          continue;
        }
        SetNodeType(node, type);
      }

      typePopup.IsOpen = false;
      if (selectedNodes.Length > 0) {
        await ArrangeGraphAsync(true, false);
      }
    }

    private void UpdateTypePanel() {
      var selectedNodes = GraphControl.Selection.Nodes.ToArray();
      if (selectedNodes.Length == 0) {
        typePopup.IsOpen = false;
        return;
      }

      var buttons = new[] { type0Button, type1Button, type2Button };
      for (var type = 0; type < buttons.Length; type++) {
        var currentType = type;
        buttons[type].BorderThickness = selectedNodes.Any(node => GetNodeType(node) == currentType)
            ? new Thickness(3)
            : new Thickness(1);
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
      typePopup.HorizontalOffset = Math.Max(4, point.X - 57);
      typePopup.VerticalOffset = point.Y < 86 ? point.Y + 18 : point.Y - 86;
    }

    /// <summary>
    ///   Initializes the sample combo box.
    /// </summary>
    private void PrepareSampleList() {
      foreach (var sample in samples) {
        sampleComboBox.Items.Add(sample.Name);
      }
      sampleComboBox.SelectedIndex = 0;
    }

    /// <summary>
    ///   Binds actions to the buttons in the toolbar.
    /// </summary>
    private void InitializeUI() {
      layoutButton.Click += async (_, _) => await ArrangeGraphAsync(true, true);
      resetButton.Click += async (_, _) => await LoadSampleAsync(true);
      sampleComboBox.SelectionChanged += async (_, _) => {
        if (sampleComboBox.SelectedIndex >= 0) {
          await LoadSampleAsync(true);
        }
      };
    }

    /// <summary>
    ///   Disables the graph and toolbar UI while a layout is running.
    /// </summary>
    private void SetUIDisabled(bool disabled) {
      resetButton.IsEnabled = !disabled;
      layoutButton.IsEnabled = !disabled;
      sampleComboBox.IsEnabled = !disabled;
      typePalette.IsEnabled = !disabled;
      if (graphEditorInputMode is not null) {
        graphEditorInputMode.WaitInputMode.Waiting = disabled;
      }
    }

    /// <summary>
    ///   Updates the layout popup visibility and text.
    /// </summary>
    /// <param name="visible">Whether the popup should be visible.</param>
    /// <param name="text">The desired text.</param>
    private void UpdateLayoutPopup(bool visible, string? text = null) {
      loadingPopup.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
      if (text is null) {
        return;
      }

      loadingMessage.Inlines.Clear();
      const string notConsidered = "Node types are not considered";
      const string considered = "Node types are considered";
      if (text == notConsidered) {
        loadingMessage.Inlines.Add(new Run("Node types are "));
        loadingMessage.Inlines.Add(new Run("not considered") { TextDecorations = TextDecorations.Underline });
      } else if (text == considered) {
        loadingMessage.Inlines.Add(new Run("Node types "));
        loadingMessage.Inlines.Add(new Run("are considered") { TextDecorations = TextDecorations.Underline });
      } else {
        loadingMessage.Text = text;
      }
    }

    [STAThread]
    private static void Main() {
      Run<NodeTypesDemo>();
    }
  }

  /// <summary>
  ///   Type information associated with a node.
  /// </summary>
  internal sealed class NodeTag
  {
    public int Type { get; set; }
  }

  /// <summary>
  ///   Data read from the node objects in the sample graph resources.
  /// </summary>
  internal sealed class NodeData
  {
    public NodeTag? Tag { get; set; }
  }
}
