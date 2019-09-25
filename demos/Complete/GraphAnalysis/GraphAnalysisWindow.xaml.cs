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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Analysis;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Controls;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;

namespace Demo.yFiles.Algorithms.GraphAnalysis
{
  /// <summary>
  /// This demo showcases a selection of algorithms to analyze the structure of a graph.
  /// </summary>
  public partial class GraphAnalysisWindow
  {
    private bool configOptionsValid;
    private bool inLayout;
    private bool inLoadSample;
    private bool directed;
    private bool useUniformWeights = true;
    private DictionaryMapper<INode, bool> incrementalNodesMapper;
    private bool preventLayout;
    private DictionaryMapper<INode, bool> incrementalElements;
    private readonly Random random = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphAnalysisWindow"/> class.
    /// </summary>
    public GraphAnalysisWindow() {
      InitializeComponent();
    }

    public Sample CurrentSample { get;set; }

    public AlgorithmConfiguration CurrentConfig { get; set; }

    private async void OnLoaded(object sender, EventArgs e) {
      InitializeInteraction();

      configOptionsValid = true;
      SetUiDisabled(false);

      incrementalNodesMapper = new DictionaryMapper<INode, bool> { DefaultValue = false };

      var samples = new[] {
          new Sample("Sample: Minimum Spanning Tree", "MinimumSpanningTree"),
          new Sample("Sample: Connected Components", "Connectivity"),
          new Sample("Sample: Biconnected Components", "Connectivity"),
          new Sample("Sample: Strongly Connected Components", "Connectivity"),
          new Sample("Sample: Reachability", "Connectivity"),
          new Sample("Sample: Shortest Paths", "Paths"),
          new Sample("Sample: All Paths", "Paths"),
          new Sample("Sample: All Chains", "Paths"),
          new Sample("Sample: Single Source", "Paths"),
          new Sample("Sample: Cycles", "Cycles"),
          new Sample("Sample: Degree Centrality", "Centrality"),
          new Sample("Sample: Weight Centrality", "Centrality"),
          new Sample("Sample: Graph Centrality", "Centrality"),
          new Sample("Sample: Node Edge Betweenness Centrality", "Centrality"),
          new Sample("Sample: Closeness Centrality", "Centrality"),
          new Sample("Sample: Independent Sets", "MinimumSpanningTree")
      };

      sampleComboBox.ItemsSource = samples;
      sampleComboBox.SelectedIndex = 0;

      directionComboBox.IsEnabled = false;
      directionComboBox.SelectedIndex = 0;
      uniformEdgeWeightsComboBox.IsEnabled = false;
      uniformEdgeWeightsComboBox.SelectedIndex = 0;

      InitializeGraph();

      UpdateGraphInformation();

      preventLayout = true;
      InitializeAlgorithms();

      await RunLayout(false, true, true);
    }

    private void InitializeGraph() {
      var graph = graphControl.Graph;

      // Enable undo support
      graph.SetUndoEngineEnabled(true);

      // set some nice defaults
      // use a node style renderer which highlights the node based on its tag.
      graph.NodeDefaults.Style = new ShapeNodeStyle(new AnalysisShapeNodeStyleRenderer()) {
          Shape = ShapeNodeShape.Ellipse
      };
      // use an ede style renderer which highlights the edge based on its tag.
      graph.EdgeDefaults.Style = new PolylineEdgeStyle(new AnalysisPolylineEdgeStyleRenderer());

      // use a special decorator for selection
      var selectionInstaller = new NodeStyleDecorationInstaller {
          NodeStyle = new ShapeNodeStyle
              { Shape = ShapeNodeShape.Ellipse, Pen = (Pen) new Pen(Brushes.Gray, 5).GetAsFrozen(), Brush = null }
      };

      // use a special decorator for focus
      var focusIndicatorInstaller = new NodeStyleDecorationInstaller {
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.Ellipse,
              Pen = (Pen) new Pen(Brushes.LightGray, 3) { DashStyle = DashStyles.Dash }.GetAsFrozen(),
              Brush = null
          }
      };
      var decorator = graph.GetDecorator();
      decorator.NodeDecorator.SelectionDecorator.SetImplementation(selectionInstaller);
      decorator.NodeDecorator.FocusIndicatorDecorator.SetImplementation(focusIndicatorInstaller);

      graph.EdgeDefaults.Labels.LayoutParameter = FreeEdgeLabelModel.Instance.CreateDefaultParameter();

      graph.EdgeDefaults.Labels.Style = new DefaultLabelStyle {
          TextSize = 10,
          BackgroundBrush = Brushes.AliceBlue,
          BackgroundPen = (Pen) new Pen(Brushes.LightSkyBlue, 2).GetAsFrozen(),
          AutoFlip = false
      };
    }

    /// <summary>
    /// Creates the list with the available algorithms to be set as items of the algorithm combo box.
    /// </summary>
    private void InitializeAlgorithms() {
      var algorithms = new[] {
          new Algorithm("Algorithm: Minimum Spanning Tree", new MinimumSpanningTreeConfig()),
          new Algorithm("Algorithm: Connected Components", new ConnectivityConfig(ConnectivityMode.ConnectedComponents)),
          new Algorithm("Algorithm: Biconnected Components", new ConnectivityConfig(ConnectivityMode.BiconnectedComponents)),
          new Algorithm("Algorithm: Strongly Connected Components", new ConnectivityConfig(ConnectivityMode.StronglyConnectedComponents)),
          new Algorithm("Algorithm: Reachability", new ReachabilityConfig()),
          new Algorithm("Algorithm: Shortest Paths", new PathsConfig(PathsMode.ShortestPaths)),
          new Algorithm("Algorithm: All Paths", new PathsConfig(PathsMode.AllPaths)),
          new Algorithm("Algorithm: All Chains", new PathsConfig(PathsMode.AllChains)),
          new Algorithm("Algorithm: Single Source", new PathsConfig(PathsMode.SingleSource)),
          new Algorithm("Algorithm: Cycles", new CyclesConfig()),
          new Algorithm("Algorithm: Degree Centrality", new CentralityConfig(CentralityMode.Degree)),
          new Algorithm("Algorithm: Weight Centrality", new CentralityConfig(CentralityMode.Weight)),
          new Algorithm("Algorithm: Graph Centrality", new CentralityConfig(CentralityMode.Graph)),
          new Algorithm("Algorithm: Node Edge Betweenness Centrality", new CentralityConfig(CentralityMode.NodeEdgeBetweenness)),
          new Algorithm("Algorithm: Closeness Centrality", new CentralityConfig(CentralityMode.Closeness)),
          new Algorithm("Algorithm: Independent Sets", new IndependentSetConfig())
      };

      algorithmComboBox.ItemsSource = algorithms;
    }

    /// <summary>
    /// Set up the input mode and other aspects of interaction.
    /// </summary>
    private void InitializeInteraction() {
      incrementalElements = new DictionaryMapper<INode, bool> { DefaultValue = false };

      var inputMode = new GraphEditorInputMode {
          ShowHandleItems = GraphItemTypes.Bend | GraphItemTypes.Edge | GraphItemTypes.Label | GraphItemTypes.Port,
          AllowAddLabel = false
      };

      // deletion: notify configuration about removed elements and remove the elements from the maps, too
      inputMode.DeletingSelection += (sender, e) => {
        var selection = (IGraphSelection) e.Selection;
        CurrentConfig.EdgeRemoved = true;
        var graph = e.Context.GetGraph();
        foreach (var node in selection.SelectedNodes) {
          foreach (var edge in graph.EdgesAt(node)) {
            if (!selection.IsSelected(edge.Opposite(node))) {
              incrementalNodesMapper[(INode) edge.Opposite(node)] = true;
            }
          }
        }
        foreach (var edge in selection.SelectedEdges) {
          if (!selection.IsSelected(edge.GetSourceNode())) {
            incrementalNodesMapper[edge.GetSourceNode()] = true;
            incrementalElements[edge.GetSourceNode()] = true;
          }
          if (!selection.IsSelected(edge.GetTargetNode())) {
            incrementalNodesMapper[edge.GetTargetNode()] = true;
            incrementalElements[edge.GetTargetNode()] = true;
          }
        }

        CurrentConfig.IncrementalElements = incrementalElements;
      };

      // after deletion: update the graph information panel
      // and run a layout and the current algorithm
      inputMode.DeletedSelection += async (sender, e) => {
        UpdateGraphInformation();
        await RunLayout(true, false, true);
      };

      // edge creation: update the graph information panel
      // and add the new edge to the elements to be updated;
      // run a new layout and a new algorithm
      inputMode.CreateEdgeInputMode.EdgeCreated += async (sender, e) => {
        UpdateGraphInformation();
        var edge = e.Item;
        incrementalNodesMapper[edge.GetSourceNode()] = true;
        incrementalNodesMapper[edge.GetTargetNode()] = true;

        incrementalElements[edge.GetSourceNode()] = true;
        incrementalElements[edge.GetTargetNode()] = true;

        CurrentConfig.IncrementalElements = incrementalElements;

        await RunLayout(true, false, true);
      };

      // same for new nodes
      inputMode.NodeCreated += (sender, e) => {
        UpdateGraphInformation();

        incrementalElements[e.Item] = true;
        CurrentConfig.IncrementalElements = incrementalElements;

        // prevent a new layout from starting and disable the UI's buttons
        inLayout = true;
        SetUiDisabled(true);

        ApplyAlgorithm();

        // permit a new layout to start and enable the UI's buttons
        ReleaseLocks();
        SetUiDisabled(false);
      };

      inputMode.LabelTextChanged += (sender, e) => {
        // prevent a new layout from starting and disable the UI's buttons
        inLayout = true;
        SetUiDisabled(true);

        ApplyAlgorithm();

        // permit a new layout to start and enable the UI's buttons
        ReleaseLocks();
        SetUiDisabled(false);
      };

      // also we add a context menu
      inputMode.ContextMenuItems = GraphItemTypes.Node;
      inputMode.ContextMenuInputMode.ClearMenu = true;
      inputMode.PopulateItemContextMenu += (o, args) => {
        if (CurrentConfig != null && configOptionsValid && !inLayout) {
          CurrentConfig.PopulateContextMenu(args);
        }
      };
      graphControl.InputMode = inputMode;
    }


    private void ApplyAlgorithm() {
      if (CurrentConfig == null || !configOptionsValid) {
        return;
      }

      CurrentConfig.Directed = directed;
      CurrentConfig.UseUniformWeights = useUniformWeights;

      // apply the algorithm
      CurrentConfig.Apply(graphControl);
    }

    /// <summary>
    /// Button handler to run a layout algorithm.
    /// </summary>
    /// <remarks>
    /// Runs a layout without re-executing the algorithm.
    /// </remarks>
    private async void RunLayout(object sender, EventArgs e) {
      await RunLayout(false, false, false);
    }

    /// <summary>
    /// Run a layout and an analysis algorithm if wanted.
    /// </summary>
    /// <param name="incremental">Whether to run in incremental mode, i.e. only moving new items.</param>
    /// <param name="clearUndo">Whether to clear the undo queue after the layout.</param>
    /// <param name="runAlgorithm">Whether to apply the <see cref="CurrentConfig">current analysis algorithm</see>, too.</param>
    /// <returns></returns>
    private async Task RunLayout(bool incremental, bool clearUndo, bool runAlgorithm) {
      // the actual organic layout
      var layout = new OrganicLayout {
          Deterministic = true,
          ConsiderNodeSizes = true,
          Scope = incremental ? Scope.MainlySubset : Scope.All,
          LabelingEnabled = false,
          PreferredEdgeLength = 100,
          MinimumNodeDistance = 10
      };
      ((ComponentLayout) layout.ComponentLayout).Style = ComponentArrangementStyles.None | ComponentArrangementStyles.ModifierNoOverlap;

      OrganicLayoutData layoutData = null;
      if (incremental && incrementalNodesMapper != null) {
        layoutData = new OrganicLayoutData();
        layoutData.AffectedNodes.Mapper = incrementalNodesMapper;
      }
      inLayout = true;
      SetUiDisabled(true);
      // run the layout in an asynchronous, animated fashion
      await graphControl.MorphLayout(layout, TimeSpan.FromSeconds(0.5), layoutData);

      // run algorithm
      if (runAlgorithm) {
        // apply graph algorithms after layout
        ApplyAlgorithm();
        var algorithm = algorithmComboBox.SelectedItem as Algorithm;
        if (algorithm != null && algorithm.DisplayName.EndsWith("Centrality")) {
          // since centrality changes the node sizes, node overlaps need to be removed
          await graphControl.MorphLayout(new OrganicRemoveOverlapsStage(), TimeSpan.FromSeconds(0.2));
        }
      }

      // labeling: place labels after the layout and the analysis (which might have generated or changed labels)
      var labeling = new GenericLabeling {
          PlaceEdgeLabels = true,
          PlaceNodeLabels = false,
          Deterministic = true
      };
      var labelingData = new LabelingData {
          EdgeLabelPreferredPlacement = {
              Delegate = label => {
                var preferredPlacementDescriptor = new PreferredPlacementDescriptor();
                if ("Centrality".Equals(label.Tag)) {
                  preferredPlacementDescriptor.SideOfEdge = LabelPlacements.OnEdge;
                } else {
                  preferredPlacementDescriptor.SideOfEdge = LabelPlacements.RightOfEdge | LabelPlacements.LeftOfEdge;
                  preferredPlacementDescriptor.DistanceToEdge = 5;
                }
                preferredPlacementDescriptor.Freeze();
                return preferredPlacementDescriptor;
              }
          }
      };
      await graphControl.MorphLayout(labeling, TimeSpan.FromSeconds(0.2), labelingData);

      // cleanup
      if (clearUndo) {
        graphControl.Graph.GetUndoEngine().Clear();
      }
      // clean up data provider
      incrementalNodesMapper.Clear();

      // enable the UI's buttons
      ReleaseLocks();
      SetUiDisabled(false);
      UpdateUiState();
    }

    /// <summary>
    /// Load the previous sample graph in the list.
    /// </summary>
    private void PreviousSample(object sender, RoutedEventArgs e) {
      sampleComboBox.SelectedIndex--;
    }

    /// <summary>
    /// Load the next sample graph in the list.
    /// </summary>
    private void NextSample(object sender, RoutedEventArgs e) {
      sampleComboBox.SelectedIndex++;
    }

    /// <summary>
    /// The selected sample in the combo box has changed.
    /// </summary>
    /// <remarks>
    /// Load the sample and apply the corresponding analysis algorithm.
    /// </remarks>
    private async void OnSampleChanged(object sender, RoutedEventArgs e) {
      if (inLayout || inLoadSample) {
        return;
      }

      var sampleSelectedIndex = sampleComboBox.SelectedIndex;

      if (CurrentConfig != null) {
        directed = CurrentConfig.SupportsDirectedAndUndirected ? directionComboBox.SelectedIndex == 1 : CurrentConfig.Directed;
      }

      var sample = (Sample) sampleComboBox.SelectedItem;
      var graph = graphControl.Graph;
      if (sampleComboBox.SelectedItem == null) {
        // no specific item - just clear the graph
        graph.Clear();
        // and fit the content
        graphControl.FitGraphBounds();
        return;
      }
      inLoadSample = true;
      SetUiDisabled(true);
      var fileName = string.Format("Resources/{0}.graphml", sample.FileName);
      // load the sample graph and start the layout algorithm in the done handler
      graphControl.ImportFromGraphML(fileName);
      await ApplyAlgorithmForKey(sampleSelectedIndex);
    }

    /// <summary>
    /// Apply the algorithm which is appropriate for the selected graph.
    /// </summary>
    /// <param name="sampleSelectedIndex">The index to use.</param>
    private async Task ApplyAlgorithmForKey(int sampleSelectedIndex) {
      ResetStyles();

      if (CurrentConfig != null && CurrentConfig.IncrementalElements != null &&
          CurrentConfig.IncrementalElements.Entries.Any()) {
        incrementalElements.Clear();
        CurrentConfig.IncrementalElements = incrementalElements;
        CurrentConfig.EdgeRemoved = false;
      }

      // run the layout if the layout combo box is already correct
      var algorithmSelectedIndex = algorithmComboBox.SelectedIndex;
      if (algorithmSelectedIndex != sampleSelectedIndex) {
        preventLayout = true;
        // otherwise, change the selection and indirectly trigger the layout
        algorithmComboBox.SelectedIndex = sampleSelectedIndex; // changing the algorithm will trigger a layout run
      } else {
        UpdateGraphInformation();
      }

      preventLayout = false;
      // run a layout from scratch (i.e. for a new graph), clear the undo queue and apply an analysis algorithm
      await RunLayout(false, true, true);
    }

    /// <summary>
    /// The selected analysis algorithm changed.
    /// </summary>
    private async void OnAlgorithmChanged(object sender, RoutedEventArgs e) {
      CurrentConfig = ((Algorithm) algorithmComboBox.SelectedItem).Configuration;

      // if the algorithm doesn't support to chose between directed and undirected edges
      // set the combobox to the only mode the algorithm supports
      if (!CurrentConfig.SupportsDirectedAndUndirected) {
        directionComboBox.SelectedIndex = CurrentConfig.Directed ? 1 : 0;
      }
      directed = directionComboBox.SelectedIndex == 1;

      ResetStyles();
      UpdateGraphInformation();

      if (!preventLayout) {
        await RunLayout(false, false, true);
      }

      preventLayout = false;
    }

    /// <summary>
    /// Whether the graph is directed has been toggled.
    /// </summary>
    private async void OnDirectedChanged(object sender, RoutedEventArgs e) {
      if (directionComboBox.IsEnabled) {
        directed = directionComboBox.SelectedIndex == 1;
        await RunLayout(true, false, true);
      }
    }

    /// <summary>
    /// Whether to use uniform edge weights has been changed.
    /// </summary>
    private async void OnUniformEdgeWeightsChanged(object sender, RoutedEventArgs e) {
      if (uniformEdgeWeightsComboBox.IsEnabled) {
        useUniformWeights = uniformEdgeWeightsComboBox.SelectedIndex == 0;
        await GenerateEdgeLabels();
      }
    }

    /// <summary>
    /// Updates the table that holds information about the graph.
    /// </summary>
    private void UpdateGraphInformation() {
      // clear table
      var table = graphInformationTable;
      var rows = table.RowGroups[0];
      rows.Rows.Clear();

      // fill table with updated information
      var graph = graphControl.Graph;

      var captions = new[] {
          "Number of Nodes",
          "Number of Edges",
          "Acyclic",
          "Bipartite",
          "Connected",
          "Biconnected",
          "Strongly Connected",
           "Planar",
          "Tree",
          "Self-Loops",
          "Parallel Edges"
      };
      var analyzer = graph.GetStructureAnalyzer();
      foreach (var caption in captions) {
        var row = new TableRow();
        rows.Rows.Add(row);
        var graphInformation = GetGraphInformation(graph, analyzer, caption);
        if (graphInformation.isBooleanValue) {
          var tableCell = new TableCell();
          row.Cells.Add(tableCell);
          tableCell.ColumnSpan = 2;
          var para = new Paragraph();
          tableCell.Blocks.Add(para);
          var textBlock = new TextBlock();
          para.Inlines.Add(textBlock);
          var isTrue = (bool) graphInformation.value;
          // boolean information is presented green (true) or grey (false)
          textBlock.Foreground = isTrue ? Brushes.Green : Brushes.Gray;
          textBlock.TextDecorations = isTrue ? null : TextDecorations.Strikethrough;
          textBlock.FontWeight = isTrue ? FontWeights.Bold : FontWeights.Normal;
          textBlock.FontSize = 12;
          textBlock.Text = caption;
          para.Margin = new Thickness(0);
        } else {
          var tableCell = new TableCell();
          row.Cells.Add(tableCell);
          var para = new Paragraph {
              Margin = new Thickness(0)
          };
          tableCell.Blocks.Add(para);
          var textBlock = new TextBlock();
          para.Inlines.Add(textBlock);
          textBlock.FontWeight = FontWeights.Bold;
          textBlock.Text = caption;

          var tableCell2 = new TableCell();
          row.Cells.Add(tableCell2);
          var para2 = new Paragraph {
              Margin = new Thickness(0)
          };
          tableCell2.Blocks.Add(para2);
          var textBlock2 = new TextBlock();
          para2.Inlines.Add(textBlock2);
          textBlock2.FontWeight = FontWeights.Bold;
          textBlock2.Text = graphInformation.value.ToString();
        }
      }
    }

    /**
     * Returns the graph information according to the given type.
     *
     * @param graph the given graph
     * @param type the algorithm type
     *
     * @returns {boolean, Object, string}
     */
    SingleGraphInfo GetGraphInformation(IGraph graph, GraphStructureAnalyzer analyzer, string type) {
      switch (type) {
        case "Number of Nodes":
          return new SingleGraphInfo(false, graph.Nodes.Count, null);
        case "Number of Edges":
          return new SingleGraphInfo(false, graph.Edges.Count, null);
        case "Acyclic":
          return new SingleGraphInfo(true, analyzer.IsAcyclic(true),
              "https://en.wikipedia.org/wiki/Cycle_(graph_theory)");
        case "Bipartite":
          return new SingleGraphInfo(true, analyzer.IsBipartite(),
              "https://en.wikipedia.org/wiki/Bipartite_graph");
        case "Connected":
          return new SingleGraphInfo(true, analyzer.IsConnected(),
              "https://en.wikipedia.org/wiki/Connectivity_(graph_theory)");
        case "Biconnected":
          return new SingleGraphInfo(true, analyzer.IsBiconnected(),
              "https://en.wikipedia.org/wiki/Biconnected_graph");
        case "Strongly Connected":
          return new SingleGraphInfo(true, analyzer.IsStronglyConnected(),
              "https://en.wikipedia.org/wiki/Strongly_connected_component");
        case "Planar":
          return new SingleGraphInfo(true, analyzer.IsPlanar(),
            "https://en.wikipedia.org/wiki/Planar_graph");
        case "Tree":
          return new SingleGraphInfo(true, analyzer.IsTree(false),
              "https://en.wikipedia.org/wiki/Tree_(graph_theory)");
        case "Self-Loops":
          return new SingleGraphInfo(true, analyzer.HasSelfLoops(),
              "https://en.wikipedia.org/wiki/Loop_(graph_theory)");
        case "Parallel Edges":
          return new SingleGraphInfo(true, analyzer.HasMultipleEdges(),
              "https://en.wikipedia.org/wiki/Multiple_edges");
        default:
          return new SingleGraphInfo(false, graph.Nodes.Count, null);
      }
    }

    /// <summary>
    /// Resets the styles of the nodes and edges to the default style.
    /// </summary>
    private void ResetStyles() {
      var graph = graphControl.Graph;
      var defaultEdgeStyle = graph.EdgeDefaults.Style;
      ((PolylineEdgeStyle) defaultEdgeStyle).TargetArrow = directed ? Arrows.Default : Arrows.None;

      foreach (var edge in graph.Edges) {
        graph.SetStyle(edge, defaultEdgeStyle);
      }

      DeleteEdgeLabels();
      DeleteNodeLabels();

      foreach (var node in graph.Nodes) {
        graph.SetStyle(node, graph.NodeDefaults.Style);
        graph.SetNodeLayout(node, new RectD(node.Layout.GetTopLeft(), graph.NodeDefaults.Size));
      }
    }

    private void DeleteEdgeLabels() {
      var graph = graphControl.Graph;
      foreach (var label in graph.GetEdgeLabels().ToList()) {
        graph.Remove(label);
      }
    }

    private void DeleteNodeLabels() {
      var graph = graphControl.Graph;
      foreach (var label in graph.GetNodeLabels().ToList()) {
        graph.Remove(label);
      }
    }

    private void DeleteCustomEdgeLabels() {
      var graph = graphControl.Graph;
      foreach (var label in graph.GetEdgeLabels().Where(l => Equals(l.Tag, "Weight") || Equals(l.Tag, "Centrality")).ToList()) {
        graph.Remove(label);
      }
    }

    /// <summary>
    /// Remove Edge Labels button has been pressed.
    /// </summary>
    private async void RemoveEdgeLabels(object sender, RoutedEventArgs e) {
      DeleteCustomEdgeLabels();
      await RunLayout(true, false, true);
    }

    /// <summary>
    /// Generate Edge Label button has been pressed.
    /// </summary>
    private async void GenerateEdgeLabels(object sender, RoutedEventArgs e) {
      await GenerateEdgeLabels();
    }

    /// <summary>
    /// Generates custom edge labels with a random value which denotes the edge weight.
    /// </summary>
    private async Task GenerateEdgeLabels() {
      var graph = graphControl.Graph;
      
      DeleteCustomEdgeLabels();

      foreach (var edge in graph.Edges) {
        var weightLabelStyle = new DefaultLabelStyle {
            TextSize = 10,
            TextBrush = Brushes.Gray
        };

        // select a weight from 1 to 20
        var weight = useUniformWeights ? 1 : random.Next(1, 21);
        graph.AddLabel(edge, weight.ToString(), FreeEdgeLabelModel.Instance.CreateDefaultParameter(), weightLabelStyle, tag : "Weight");
      }

      await RunLayout(true, false, true);
    }

    private void ReleaseLocks() {
      inLoadSample = false;
      inLayout = false;
    }

    private void SetUiDisabled(bool disabled) {
      var enabled = !disabled;
      sampleComboBox.IsEnabled = enabled;
      nextButton.IsEnabled = enabled;
      previousButton.IsEnabled = enabled;
      directionComboBox.IsEnabled = enabled;
      algorithmComboBox.IsEnabled = enabled;
      uniformEdgeWeightsComboBox.IsEnabled = enabled;
      generateEdgeLabelsButton.IsEnabled = enabled;
      removeEdgeLabelsButton.IsEnabled = enabled;
      layoutButton.IsEnabled = enabled;
      graphControl.InputMode.InputModeContext.Lookup<WaitInputMode>().Waiting = disabled;
    }

    private void UpdateUiState() {
      sampleComboBox.IsEnabled = true;
      algorithmComboBox.IsEnabled = true;
      nextButton.IsEnabled = sampleComboBox.SelectedIndex < sampleComboBox.Items.Count - 1;
      previousButton.IsEnabled = sampleComboBox.SelectedIndex > 0;
      directionComboBox.IsEnabled = configOptionsValid && !inLayout && CurrentConfig.SupportsDirectedAndUndirected;
      uniformEdgeWeightsComboBox.IsEnabled = configOptionsValid && !inLayout && CurrentConfig.SupportsWeights;
      generateEdgeLabelsButton.IsEnabled = configOptionsValid && !inLayout && CurrentConfig.SupportsWeights;
      removeEdgeLabelsButton.IsEnabled = configOptionsValid && !inLayout && CurrentConfig.SupportsWeights;
      layoutButton.IsEnabled = true;
    }

    /// <summary>
    /// Execute the new command.
    /// </summary>
    private void ExecuteNew(object sender, ExecutedRoutedEventArgs e) {
      graphControl.Graph.Clear();
      graphControl.Graph.GetUndoEngine().Clear();
    }

    /// <summary>
    /// Whether the new command can be executed.
    /// </summary>
    /// <remarks>
    /// Enables/Disables the new button according to the state of the <see cref="WaitInputMode"/>.
    /// </remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CanExecuteNew(object sender, CanExecuteRoutedEventArgs e) {
      var mode = graphControl != null ? graphControl.InputMode as GraphEditorInputMode : null;
      e.CanExecute = mode != null && !mode.WaitInputMode.Waiting;
    }
    
    /// <summary>
    /// Represents a sample graph.
    /// </summary>
    /// <remarks>
    /// Has a display name and the actual file name of the graph.
    /// </remarks>
    public sealed class Sample
    {
      public Sample(string displayName, string fileName) {
        DisplayName = displayName;
        FileName = fileName;
      }

      public string DisplayName { get; set; }
      public string FileName { get; set; }
    }

    /// <summary>
    /// Represents an analysis algorithm.
    /// </summary>
    /// <remarks>
    /// Has a display name and the <see cref="AlgorithmConfiguration"/> for the algorithm.
    /// </remarks>
    private sealed class Algorithm
    {
      public Algorithm(string displayName, AlgorithmConfiguration configuration) {
        DisplayName = displayName;
        Configuration = configuration;
      }

      public string DisplayName { get; set; }
      public AlgorithmConfiguration Configuration { get; set; }
    }

    private sealed class SingleGraphInfo
    {
      public readonly bool isBooleanValue;
      public readonly object value;
      public readonly string url;

      public SingleGraphInfo(bool isBooleanValue, object value, string url) {
        this.isBooleanValue = isBooleanValue;
        this.value = value;
        this.url = url;
      }
    }

  }
}
