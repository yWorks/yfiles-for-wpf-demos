/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Partial;
using yWorks.Utils;
using LayoutOrientation = yWorks.Layout.Partial.LayoutOrientation;

namespace Demo.yFiles.Layout.PartialLayout
{
  /// <summary>
  /// This demo shows how to apply <see cref="Demo.yFiles.Layout.PartialLayout"/> to orthogonal, organic, hierarchic or
  /// circular layouts and the effect of some of its configuration settings.
  /// </summary>
  /// <remarks>The partial layout algorithm changes the coordinates of a given set of graph elements
  /// (called partial elements) and leaves the location and size of all other elements
  /// (called fixed elements) unchanged. The algorithm aims to place the partial elements
  /// such that the resulting drawing (including the fixed elements) has a good quality
  /// with respect to common graph drawing aesthetics.
  /// </remarks>
  public partial class PartialLayoutDemo
  {
    private const string HierarchicScenario = "Hierarchic";
    private const string OrthogonalScenario = "Orthogonal";
    private const string OrganicScenario = "Organic";
    private const string CircularScenario = "Circular";

    // define colors for fixed/partial nodes/edges
    private static readonly Color ColorFixedNode = Colors.Gray;
    private static readonly Color ColorPartialNode = Color.FromRgb(255, 151, 0);
    private static readonly Color ColorFixedEdge = Colors.Black;
    private static readonly Color ColorPartialEdge = ColorPartialNode;

    private static readonly CollapsibleNodeStyleDecorator PartialGroupNodeStyle =
      new CollapsibleNodeStyleDecorator(new PanelNodeStyle
                                          {
                                            Color = Color.FromArgb(255, 202, 220, 255),
                                            LabelInsetsColor = ColorPartialNode,
                                            Insets = new InsetsD(5, 21, 5, 5)
                                          });

    private static readonly CollapsibleNodeStyleDecorator FixedGroupNodeStyle =
      new CollapsibleNodeStyleDecorator(new PanelNodeStyle
                                          {
                                            Color = Color.FromArgb(255, 202, 220, 255),
                                            LabelInsetsColor = ColorFixedNode,
                                            Insets = new InsetsD(5, 21, 5, 5)
                                          });

    private static readonly ShinyPlateNodeStyle PartialNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidColorBrush(ColorPartialNode) };
    private static readonly ShinyPlateNodeStyle FixedNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidColorBrush(ColorFixedNode) };

    private static readonly PolylineEdgeStyle PartialEdgeStyle = new PolylineEdgeStyle
                                                                   {
                                                                     TargetArrow = Arrows.Default,
                                                                     Pen =
                                                                       new Pen(new SolidColorBrush(ColorPartialEdge), 1)
                                                                   };

    private static readonly PolylineEdgeStyle FixedEdgeStyle = new PolylineEdgeStyle
                                                                 {
                                                                   TargetArrow = Arrows.Default,
                                                                   Pen = new Pen(new SolidColorBrush(ColorFixedEdge), 1)
                                                                 };

    // the mapper storing if a node/edge is fixed or shall be moved by the partial layout

    private static readonly IDictionary<string, ILayoutAlgorithm> SubGraphLayouts = new Dictionary<string, ILayoutAlgorithm>(4);
    private MasterViewConversionMapper<IEdge, bool> partialEdgesMapper;
    private MasterViewConversionMapper<INode, bool> partialNodesMapper;

    static PartialLayoutDemo() {
      SubGraphLayouts[LayoutIncremental] = new HierarchicLayout();
      SubGraphLayouts[LayoutOrganic] = new OrganicLayout();
      SubGraphLayouts[LayoutOrthogonal] = new OrthogonalLayout();
      SubGraphLayouts[LayoutCircular] = new CircularLayout();
      SubGraphLayouts[LayoutUnchanged] = null;
    }

    public PartialLayoutDemo() {
      InitializeComponent();
    }

    private void OnLoaded(object sender, EventArgs e) {
      scenarioComboBox.Items.Add(HierarchicScenario);
      scenarioComboBox.Items.Add(OrthogonalScenario);
      scenarioComboBox.Items.Add(OrganicScenario);
      scenarioComboBox.Items.Add(CircularScenario);
      InitializeGraphDefaults();
      InitializeInputModes();
      SetupOptions();
      scenarioComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Activates folding, sets the defaults for new graph elements and registers mappers
    /// </summary>
    private void InitializeGraphDefaults() {
      var fm = new FoldingManager();
      graphControl.Graph = fm.CreateFoldingView().Graph;

      IGraph graph = graphControl.Graph;
      graphControl.NavigationCommandsEnabled = true;

      //Create graph
      graph.NodeDefaults.Size = new SizeD(60, 30);

      // set the style as the default for all new nodes
      graph.NodeDefaults.Style = PartialNodeStyle;

      // set the style as the default for all new edges
      graph.EdgeDefaults.Style = PartialEdgeStyle;

      CollapsibleNodeStyleDecorator groupNodeStyle = PartialGroupNodeStyle;
      var groupNodeDefaults = graph.GroupNodeDefaults;
      groupNodeDefaults.Labels.LayoutParameter = InteriorStretchLabelModel.North;
      groupNodeDefaults.Labels.Style = new DefaultLabelStyle { TextAlignment = TextAlignment.Right };
      groupNodeDefaults.Style = groupNodeStyle;

      //Create and register mappers that specifiy partial graph elements
      partialNodesMapper = new MasterViewConversionMapper<INode, bool>(graph) { DefaultValue = true };
      partialEdgesMapper = new MasterViewConversionMapper<IEdge, bool>(graph) { DefaultValue = true };

      graphControl.GraphMLIOHandler.AddInputMapper(yWorks.Layout.Partial.PartialLayout.AffectedNodesDpKey.Name, partialNodesMapper);
      graphControl.GraphMLIOHandler.AddInputMapper(yWorks.Layout.Partial.PartialLayout.AffectedEdgesDpKey.Name, partialEdgesMapper);
    }

    /// <summary>
    /// Configures input modes to interact with the graph structure.
    /// </summary>
    private void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode();
      graphEditorInputMode.ClickInputMode.DoubleClicked += ClickInputModeDoubleClicked;
      // add a label to newly created nodes and mark the node as non-fixed
      graphEditorInputMode.NodeCreated +=
          delegate(object sender, ItemEventArgs<INode> args) {
            var node = args.Item;
            if (graphControl.Graph.IsGroupNode(node)) {
              graphControl.Graph.AddLabel(node, "Group");
            } else {
              graphControl.Graph.AddLabel(node,
                graphControl.Graph.Nodes.Count.ToString());
            }
            SetFixed(node, false);
          };
      graphEditorInputMode.CreateEdgeInputMode.EdgeCreated +=
        (sender, args) => SetFixed(args.Item, false);
      graphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    /// If an <c>INode</c> or <c>IEdge</c> was double clicked, its fixed/partial state gets toggled.
    /// </summary>
    private void ClickInputModeDoubleClicked(object sender, ClickEventArgs e) {
      var geim = e.Context.Lookup<GraphEditorInputMode>();
      if (geim != null) {
        IModelItem modelItem = geim.FindItems(e.Location, new[] { GraphItemTypes.Node | GraphItemTypes.Edge }, null).FirstOrDefault();
        if (modelItem is INode) {
          var node = (INode) modelItem;
          SetFixed(node, !IsFixed(node));
        } else if (modelItem is IEdge) {
          var edge = (IEdge) modelItem;
          SetFixed(edge, !IsFixed(edge));
        }
      }
    }

    /// <summary>
    /// Sets the given node as fixed or movable and changes its color to indicate its new state.
    /// </summary>
    private void SetFixed(INode node, bool isFixed) {
      partialNodesMapper[node] = !isFixed;
      SetNodeStyle(node, isFixed);
    }

    private void SetNodeStyle(INode node, bool isFixed) {
      if (graphControl.Graph.IsGroupNode(node)) {
        graphControl.Graph.SetStyle(node, isFixed ? FixedGroupNodeStyle : PartialGroupNodeStyle);
      } else {
        graphControl.Graph.SetStyle(node, isFixed ? FixedNodeStyle : PartialNodeStyle);
      }
    }

    private void SetFixed(IEdge edge, bool isFixed) {
      partialEdgesMapper[edge] = !isFixed;
      SetEdgeStyle(edge, isFixed);
    }

    private void SetEdgeStyle(IEdge edge, bool isFixed) {
      graphControl.Graph.SetStyle(edge, isFixed ? FixedEdgeStyle : PartialEdgeStyle);
    }

    /// <summary>
    /// Returns if a given edge is considered fixed or shall be rerouted by the layout algorithm.
    /// Note that an edge always gets rerouted if any of its end nodes may be moved.
    /// </summary>
    private bool IsFixed(IEdge edge) {
      return !partialEdgesMapper[edge];
    }

    /// <summary>
    /// Returns if a given node is considered fixed or shall be moved by the layout algorithm.
    /// </summary>
    private bool IsFixed(INode node) {
      return !partialNodesMapper[node];
    }

    private void ReadSampleGraph(string baseName) {
      partialNodesMapper.Clear();
      partialEdgesMapper.Clear();
      graphControl.ImportFromGraphML(baseName);
      foreach (var node in graphControl.Graph.Nodes) {
        var isFixed = IsFixed(node);
        SetNodeStyle(node, isFixed);
      }
      foreach (var edge in graphControl.Graph.Edges) {
        SetEdgeStyle(edge, IsFixed(edge));
      }
    }

    private void SetSelectionFixed(bool setFixed) {
      foreach (INode n in graphControl.Selection.SelectedNodes) {
        SetFixed(n, setFixed);
      }
      foreach (IEdge e in graphControl.Selection.SelectedEdges) {
        SetFixed(e, setFixed);
      }
    }

    private void ScenarioComboBoxSelectedValueChanged(object sender, EventArgs e) {
      LoadScenario();
    }

    private void SetOptions(ComponentAssignmentStrategy componentAssignmentStrategy, string coreLayout,
                            SubgraphPlacement subgraphPlacement,
                            EdgeRoutingStrategy edgeRoutingStrategy, LayoutOrientation layoutOrientation,
                            int minNodeDistance, bool allowMirroring, bool nodeSnapping) {
      handler.GetItemByName(ComponentAssignment).Value = componentAssignmentStrategy;
      handler.GetItemByName(SubgraphLayout).Value = coreLayout;
      handler.GetItemByName(SubgraphPositioning).Value = subgraphPlacement;
      handler.GetItemByName(EdgeRouting).Value = edgeRoutingStrategy;
      handler.GetItemByName(LayoutOrientationStrategy).Value = layoutOrientation;
      handler.GetItemByName(MinimumNodeDistance).Value = minNodeDistance;
      handler.GetItemByName(AllowMirroring).Value = allowMirroring;
      handler.GetItemByName(NodeSnapping).Value = nodeSnapping;
    }


    private void LoadScenario() {
      string scenario = (string) scenarioComboBox.SelectedItem;
      ReadSampleGraph("Resources\\" + scenario + ".graphml");
      switch (scenario) {
        case HierarchicScenario:
          SetOptions(ComponentAssignmentStrategy.Connected, LayoutIncremental, SubgraphPlacement.Barycenter,
                     EdgeRoutingStrategy.Orthogonal, LayoutOrientation.TopToBottom, 5, true, true);
          break;
        case OrthogonalScenario:
          SetOptions(ComponentAssignmentStrategy.Single, LayoutOrthogonal, SubgraphPlacement.Barycenter,
                     EdgeRoutingStrategy.Orthogonal, LayoutOrientation.None, 20, false, true);
          break;
        case OrganicScenario:
          SetOptions(ComponentAssignmentStrategy.Single, LayoutOrganic, SubgraphPlacement.Barycenter,
                     EdgeRoutingStrategy.Automatic, LayoutOrientation.None, 30, true, false);
          break;
        case CircularScenario:
          SetOptions(ComponentAssignmentStrategy.Connected, LayoutCircular, SubgraphPlacement.Barycenter,
                     EdgeRoutingStrategy.Automatic, LayoutOrientation.None, 10, true, false);
          break;
      }
    }

    private void OnRunButtonClicked(object sender, EventArgs e) {
      RunLayout();
    }

    private void OnRefreshButtonClicked(object sender, RoutedEventArgs e) {
      LoadScenario();
    }

    private void OnLockSelectionButtonClicked(object sender, RoutedEventArgs e) {
      SetSelectionFixed(true);
    }

    private void OnUnlockSelectionButtonClicked(object sender, RoutedEventArgs e) {
      SetSelectionFixed(false);
    }

    #region Option Handler for layout options    

    private const string LayoutIncremental = "Hierarchic";
    private const string LayoutOrganic = "Organic";
    private const string LayoutOrthogonal = "Orthogonal";
    private const string LayoutCircular = "Circular";
    private const string LayoutUnchanged = "Unchanged";
    private const string LayoutOptions = "LAYOUT_OPTIONS";
    private const string ComponentAssignment = "Component Assignment";
    private const string SubgraphLayout = "Subgraph Layout";
    private const string SubgraphPositioning = "Subgraph Positioning Strategy";
    private const string EdgeRouting = "Edge Routing Style";
    private const string LayoutOrientationStrategy = "Layout Orientation";
    private const string MinimumNodeDistance = "Minimum Node Distance";
    private const string AllowMirroring = "Allow Mirroring";
    private const string NodeSnapping = "Node Snapping";
    private OptionHandler handler;

    private OptionHandler Handler {
      get { return handler; }
    }

    private void SetupOptions() {
      // create the options
      SetupHandler();
      // populate the control to visualize them
      editorControl.IsAutoAdopt = true;
      editorControl.IsAutoCommit = true;
      editorControl.OptionHandler = Handler;
    }

    /// <summary>
    /// Initializes the option handler for the export
    /// </summary>
    private void SetupHandler() {
      handler = new OptionHandler(LayoutOptions);
      OptionItem componentItem = handler.AddList(ComponentAssignment,
                                                 new[]
                                                   {
                                                     ComponentAssignmentStrategy.Single,
                                                     ComponentAssignmentStrategy.Connected
                                                   },
                                                 ComponentAssignmentStrategy.Single);
      OptionItem subgraphItem = handler.AddList(SubgraphLayout, SubGraphLayouts.Keys, LayoutIncremental);
      handler.Add(SubgraphPositioning, SubgraphPlacement.Barycenter);
      handler.Add(EdgeRouting, EdgeRoutingStrategy.Automatic);
      handler.Add(LayoutOrientationStrategy, LayoutOrientation.TopToBottom);
      handler.AddInt(MinimumNodeDistance, 5, 0, int.MaxValue);
      handler.AddBool(AllowMirroring, true);
      handler.AddBool(NodeSnapping, true);
      var cm = new ConstraintManager(Handler);

      cm.SetEnabledOnValueEquals(componentItem, ComponentAssignmentStrategy.Connected, subgraphItem);
    }

    #endregion

    #region Layout configuration

    ///<summary>Create a <c>PartialLayout</c> using the selected demo settings</summary>
    private yWorks.Layout.Partial.PartialLayout CreateConfiguredPartialLayout() {
      var subGraphLayout = SubGraphLayouts[(string) handler.GetItemByName(SubgraphLayout).Value];
      ConfigureCoreLayout(subGraphLayout);
      var partialLayout = new yWorks.Layout.Partial.PartialLayout
                              {
                                ComponentAssignmentStrategy =
                                  (ComponentAssignmentStrategy) handler.GetItemByName(ComponentAssignment).Value,
                                CoreLayout = subGraphLayout,
                                SubgraphPlacement =
                                  (SubgraphPlacement) handler.GetItemByName(SubgraphPositioning).Value,
                                EdgeRoutingStrategy = (EdgeRoutingStrategy) handler.GetItemByName(EdgeRouting).Value,
                                LayoutOrientation =
                                  (LayoutOrientation) handler.GetItemByName(LayoutOrientationStrategy).Value,
                                MinimumNodeDistance = (int) handler.GetItemByName(MinimumNodeDistance).Value,
                                AllowMirroring = (bool) handler.GetItemByName(AllowMirroring).Value,
                                ConsiderNodeAlignment = (bool) handler.GetItemByName(NodeSnapping).Value
                              };

      return partialLayout;
    }

    ///<summary> Uses some of the demo settings to further configure the core layout algorithm </summary>
    private void ConfigureCoreLayout(ILayoutAlgorithm coreLayout) {
      int minNodeDistance = (int) handler.GetItemByName(MinimumNodeDistance).Value;
      if (coreLayout is OrthogonalLayout) {
        ((OrthogonalLayout) coreLayout).GridSpacing = minNodeDistance;
      } else if (coreLayout is HierarchicLayout) {
        ((HierarchicLayout) coreLayout).MinimumLayerDistance = minNodeDistance;
      } else if (coreLayout is CircularLayout) {
        ((CircularLayout) coreLayout).SingleCycleLayout.MinimumNodeDistance = minNodeDistance;
        ((CircularLayout) coreLayout).BalloonLayout.MinimumNodeDistance = minNodeDistance;
      }
    }

    ///<summary>
    /// Runs either the table or the three tiers layout depending on the selected scenario.
    ///</summary>
    private async void RunLayout() {
      DisableButtons();
      var layoutData = new PartialLayoutData {
        AffectedEdges = {Mapper = partialEdgesMapper},
        AffectedNodes = {Mapper = partialNodesMapper}
      };
      var executor = new LayoutExecutor(
        graphControl, CreateConfiguredPartialLayout())
                       {
                         Duration = TimeSpan.FromMilliseconds(500),
                         AnimateViewport = true,
                         LayoutData = layoutData
                       };

      try {
        await executor.Start();
      } catch (Exception e) {
        MessageBox.Show(this,
          "Layout did not complete successfully.\n" + e.Message);
      }

      EnableButtons();
    }

    private void DisableButtons() {
      scenarioComboBox.IsEnabled = false;
      lockSelectionButton.IsEnabled = false;
      unlockSelectionButton.IsEnabled = false;
      runButton.IsEnabled = false;
      reloadButton.IsEnabled = false;
    }

    private void EnableButtons() {
      scenarioComboBox.IsEnabled = true;
      lockSelectionButton.IsEnabled = true;
      unlockSelectionButton.IsEnabled = true;
      runButton.IsEnabled = true;
      reloadButton.IsEnabled = true;
    }

    #endregion
  }

  /// <summary>
  /// <see cref="IMapper{K,V}"/> implementation, backed by a <see cref="DictionaryMapper{K,V}"/>,
  /// that transparently accepts either master or view items.
  /// </summary>
  /// <typeparam name="K">The key type.</typeparam>
  /// <typeparam name="V">The value type.</typeparam>
  class MasterViewConversionMapper<K, V> : IMapper<K, V> where K : IModelItem
  {
    private readonly IGraph viewGraph;
    private readonly IGraph masterGraph;
    private readonly IFoldingView foldingView;
    private readonly DictionaryMapper<IModelItem, V> backingMapper = new DictionaryMapper<IModelItem, V>();

    public MasterViewConversionMapper(IGraph viewGraph) {
      this.viewGraph = viewGraph;
      foldingView = viewGraph.GetFoldingView();
      masterGraph = foldingView.Manager.MasterGraph;
    }

    public V this[K key] {
      get { return backingMapper[GetMasterItem(key)]; }
      set { backingMapper[GetMasterItem(key)] = value; }
    }

    public V DefaultValue {
      get { return backingMapper.DefaultValue; }
      set { backingMapper.DefaultValue = value; }
    }

    private IModelItem GetMasterItem(IModelItem item) {
      if (masterGraph.Contains(item)) {
        return item;
      }
      if (viewGraph.Contains(item)) {
        return foldingView.GetMasterItem(item);
      }
      return null;
    }

    public void Clear() {
      backingMapper.Clear();
    }
  }
}
