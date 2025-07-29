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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Demo.yFiles.Layout.Tree.Configuration;
using Demo.yFiles.Toolkit;
using yWorks.Analysis;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Tree;
using yWorks.Utils;

namespace Demo.yFiles.Layout.Tree
{
  /// <summary>
  ///   This demo demonstrates the different <see cref="ISubtreePlacer"/> implementations for the
  ///   <see cref="TreeLayout"/>.
  /// </summary>
  /// <remarks>
  ///   An <see cref="ISubtreePlacer"/> can be assigned to each node separately. It arranges the node it is
  ///   assigned to and its subtrees. In this demo all nodes of the same layer have the same subtree placer.
  /// </remarks>
  public partial class TreeDemo
  {
    public TreeDemo() {
      InitializeComponent();
      subtreePlacerPanel.ReloadConfiguration += delegate { UpdatePlacerList(); };
      subtreePlacerPanel.ApplyConfiguration += async delegate { await ApplyConfigurations(); };
    }

    /// <summary>
    ///   Apply the current settings to the graph.
    /// </summary>
    private async Task ApplyConfigurations() {
      var graph = graphControl.Graph;
      // Get the root node of the tree.
      var graphAdapter = new StructureGraphAdapter(graph);
      var root = graphAdapter.GetOriginalNode(LayoutGraphAlgorithms.FindRoot(graphAdapter.StructureGraph));
      // apply the current setting to all nodes of the same layer.
      ApplySettingsRecursively(graph, root, 0);
      await ApplyLayout();
    }

    /// <summary>
    ///   Apply the new settings to the selected layer and descend recursively
    /// </summary>
    private void ApplySettingsRecursively(IGraph graph, INode root, int layer) {
      var placer = subtreePlacerPanel.SubtreePlacers[layer].Configuration.CreateSubtreePlacer();
      placers[root] = placer;
      foreach (var edge in graph.EdgesAt(root, AdjacencyTypes.Outgoing)) {
        var target = (INode) edge.TargetPort.Owner;
        ApplySettingsRecursively(graph, target, layer + 1);
      }
    }

    private void OnLoaded(object sender, EventArgs e) {
      InitializeGraphDefaults();
      InitializeInputModes();
    }

    private IMapper<INode, ISubtreePlacer> placers;
    private IMapper<INode, bool> assistants;


    private void InitializeGraphDefaults() {
      var graph = graphControl.Graph;

      DemoStyles.InitDemoStyles(graph, Themes.Palette58);
      graph.NodeDefaults.Size = new SizeD(60, 40);

      graph.SetUndoEngineEnabled(true);

      // The map for the subtree placers
      placers = new DictionaryMapper<INode, ISubtreePlacer>();
      // The map for "assistant" nodes. This map has only an effect
      // if an AssistantPlacer is chosen as subtree placer
      assistants = new DictionaryMapper<INode, bool>();

      sampleComboBox.SelectedIndex = 0;
    }

    private void InitializeInputModes() {
      var geim = new GraphEditorInputMode();
      graphControl.InputMode = geim;

      // forbid node and edge creation
      geim.AllowCreateEdge = true;
      geim.AllowCreateNode = false;
      // Allow only node deletion
      geim.DeletableItems = GraphItemTypes.Node;

      geim.ContextMenuItems = GraphItemTypes.Node;
      geim.PopulateItemContextMenu += ContextMenuInputModePopulateContextMenu;
      geim.DeletingSelection += GeimDeletingSelection;
      geim.DeletedSelection += async delegate {
        // And trigger new layout
        await ApplyLayout();
      };
      // Disallow multi selection
      geim.MarqueeSelectionInputMode.Enabled = false;
      geim.MultiSelectionRecognizer = EventRecognizers.Never;

      graphControl.Selection.ItemAdded += ItemSelectionChanged;

      // Modify edge creation to always create a new target node

      var createEdgeInputMode = geim.CreateEdgeInputMode;

      // never search for target ports
      createEdgeInputMode.EndHitTestable = HitTestables.Never;
      // any location is a valid target location
      createEdgeInputMode.PrematureEndHitTestable = HitTestables.Always;

      // ignore port candidates and don't show highlights:
      // we don't want to connect to existing nodes
      createEdgeInputMode.ForceSnapToCandidate = false;
      createEdgeInputMode.SnapToEndCandidate = false;
      createEdgeInputMode.ShowPortCandidates = ShowPortCandidates.None;
      createEdgeInputMode.AllowSelfLoops = false;
      graphControl.Graph.GetDecorator().Nodes.HighlightRenderer.Hide();

      // display the new target node during edge creation
      // provide the default size
      createEdgeInputMode.PreviewGraph.NodeDefaults.Size = graphControl.Graph.NodeDefaults.Size;
      // set the style according to the layer
      createEdgeInputMode.GestureStarted += (sender, args) => {
        var layer = GetLayer((INode) createEdgeInputMode.StartPortCandidate.Owner) + 1;
        createEdgeInputMode.PreviewGraph.SetStyle(createEdgeInputMode.PreviewEndNode, NewNodeStyle(layer));
      };

      // let the EdgeCreator create a new target node and connect the new edge to it
      createEdgeInputMode.EdgeCreator = (context, graph, sourcePortCandidate, targetPortCandidate, dummyEdge) => {
        // copy the style from the dummy node
        var dummyTargetNode = createEdgeInputMode.PreviewEndNode;
        var node = graph.CreateNode(dummyTargetNode.Layout.ToRectD(), dummyTargetNode.Style, dummyTargetNode.Tag);
        // create a port at the center
        var targetPort = graph.AddPort(node, createEdgeInputMode.PreviewEndNodePort.LocationParameter);
        // create the edge from the source port candidate to the new node
        return graph.CreateEdge(sourcePortCandidate.CreatePort(context), targetPort, dummyEdge.Style);
      };

      // run a new layout after the edge has been created
      createEdgeInputMode.EdgeCreated += InteractiveEdgeCreated;
    }

    private async void InteractiveEdgeCreated(object sender, InputModeItemEventArgs<IEdge> e) {
      var node = (INode) e.Item.TargetPort.Owner;
      var layer = GetLayer(node);
      var graph = graphControl.Graph;

      // set the correct color for the layer
      graph.SetStyle(node, NewNodeStyle(layer));
      // and the correct placer for the layer
      ISubtreePlacer placer = null;
      if (layer >= subtreePlacerPanel.SubtreePlacers.Count) {
        subtreePlacerPanel.SubtreePlacers.Add(SubtreePlacerConfigurations.None);
      } else {
        var subtreePlacerConfiguration = subtreePlacerPanel.SubtreePlacers[layer].Configuration;
        placer = subtreePlacerConfiguration.CreateSubtreePlacer();
      }
      placers[node] = placer ?? new SingleLayerSubtreePlacer();
      // execute the layout
      await ApplyLayout();
      // set the placer panel to the node's layer
      subtreePlacerPanel.Level = layer;
    }

    private void ItemSelectionChanged(object source, ItemEventArgs<IModelItem> evt) {
      var selectedNode = evt.Item as INode;
      if (selectedNode != null) {
        var layer = GetLayer(selectedNode);
        subtreePlacerPanel.Level = layer;
      }
    }

    private int GetLayer(INode n) {
      var layer = 0;
      var node = n;
      while (true) {
        var inEdges = graphControl.Graph.EdgesAt(node, AdjacencyTypes.Incoming);
        if (inEdges.Count == 0) {
          break;
        }
        node = (INode) inEdges.First().SourcePort.Owner;
        layer++;
      }
      return layer;
    }

    /// <summary>
    ///   Before deleting the selection, we mark the whole subtree as selected, too. This will ensure the whole subtree will be
    ///   deleted
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GeimDeletingSelection(object sender, SelectionEventArgs<IModelItem> e) {
      var selectedNodes = e.Selection.OfType<INode>();
      var nodesToDelete = new List<INode>();
      foreach (var selectedNode in selectedNodes) {
        CollectSubtreeNodes(selectedNode, nodesToDelete);
      }
      foreach (var node in nodesToDelete) {
        e.Selection.Add(node);
      }
    }

    private void CollectSubtreeNodes(INode selectedNode, List<INode> nodesToDelete) {
      nodesToDelete.Add(selectedNode);
      var outEdges = graphControl.Graph.EdgesAt(selectedNode, AdjacencyTypes.Outgoing);
      foreach (var outEdge in outEdges) {
        var target = (INode) outEdge.TargetPort.Owner;
        CollectSubtreeNodes(target, nodesToDelete);
      }
    }

    private void ContextMenuInputModePopulateContextMenu(
        object sender,
        PopulateItemContextMenuEventArgs<IModelItem> e) {
      var node = e.Item as INode;
      // create a menu item to toggle the assistant status
      var assistant = assistants[node];

      var menuItem = new MenuItem { Header = "Set as " + (assistant ? "normal" : "assistant") };
      menuItem.Click += async delegate {
        // Toggle the value in the map and set the style to indicate an assistant node
        assistants[node] = !assistant;
        var layer = GetLayer(node);
        graphControl.Graph.SetStyle(node, assistant ? NewNodeStyle(layer) : NewAssistantStyle(layer));
        await ApplyLayout();
      };
      e.Menu.Items.Add(menuItem);
      e.ShowMenu = true;
      e.Handled = true;
    }

    private void OnReloadSampleButtonClicked(object sender, RoutedEventArgs e) {
      var messageBoxResult = MessageBox.Show(this,
          "This will reload the current sample graph and reset all custom subtree placer configurations. Do you really want to continue?",
          "Reload Sample", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);
      if (messageBoxResult == MessageBoxResult.OK) {
        SampleComboBoxSelectedValueChanged(this, null);
      }
    }

    private async void SampleComboBoxSelectedValueChanged(object sender, SelectionChangedEventArgs e) {
      switch ((string) sampleComboBox.SelectedItem) {
        case "Example Tree":
          CreateSampleGraph(graphControl.Graph);
          break;
        case "Organization Chart":
          CreateOrgChart(graphControl.Graph);
          break;
      }
      // Update the placer configuration
      UpdatePlacerList();
      // Center the graph to prevent the initial layout fading in from the top left corner
      await graphControl.FitGraphBounds();
      // And trigger new layout
      await ApplyLayout();
      // Clear the undo engine afterwards
      var undoEngine = graphControl.Graph.GetUndoEngine();
      if (undoEngine != null) {
        undoEngine.Clear();
      }
      subtreePlacerPanel.SetLevel(0);
    }

    /// <summary>
    ///   Update the subtree placer configurations in the <see cref="SubtreePlacerPanel"/>
    /// </summary>
    public void UpdatePlacerList() {
      var graph = graphControl.Graph;

      // Determine the root node of the tree:
      var graphAdapter = new StructureGraphAdapter(graph);
      var root = graphAdapter.GetOriginalNode(LayoutGraphAlgorithms.FindRoot(graphAdapter.StructureGraph));

      subtreePlacerPanel.SubtreePlacers.Clear();
      // Fill the Array recursively
      UpdatePlacerListRecursively(graph, placers, subtreePlacerPanel.SubtreePlacers, root, 0);
    }

    /// <summary>
    ///   Fill the placer Array recursively.
    /// </summary>
    /// <param name="graph">The current graph</param>
    /// <param name="map">The subtree placer map</param>
    /// <param name="placers">The placer Array to fill.</param>
    /// <param name="root">The root node of the subtree</param>
    /// <param name="layer">The index of the current layer.</param>
    private static void UpdatePlacerListRecursively(
        IGraph graph,
        IMapper<INode, ISubtreePlacer> map,
        IList<SubtreePlacerDescriptor> placers,
        INode root,
        int layer) {
      // create and set the holder for the current root node
      var placer = map[root];
      if (layer >= placers.Count) {
        placers.Add(SubtreePlacerConfigurations.GetDescriptor(placer));
      } else {
        placers[layer] = SubtreePlacerConfigurations.GetDescriptor(placer);
      }
      // do the same for the subtrees
      foreach (var edge in graph.EdgesAt(root, AdjacencyTypes.Outgoing)) {
        UpdatePlacerListRecursively(graph, map, placers, edge.TargetPort.Owner as INode, layer + 1);
      }
    }

    /// <summary>
    ///   Execute the layout algorithm.
    /// </summary>
    private async Task ApplyLayout() {
      var treeLayout = new TreeLayout();
      var treeLayoutData = treeLayout.CreateLayoutData(graphControl.Graph);
      treeLayoutData.SubtreePlacers.Mapper = placers;
      treeLayoutData.AssistantNodes.Mapper = assistants;
      treeLayoutData.ChildOrder.OutEdgeComparables.MapperFunction = e => GetOrdinal(e);

      var executor = new LayoutExecutor(graphControl, treeLayout) {
          AnimationDuration = TimeSpan.FromMilliseconds(500),
          LayoutData = treeLayoutData
      };

      try {
        await executor.Start();
      } catch (Exception e) {
        MessageBox.Show(this, "Layout did not complete successfully.\n" + e.Message);
      }
    }

    private static int GetOrdinal(IEdge edge) {
      var value = 0;
      var targetLabels = ((INode) edge.TargetPort.Owner).Labels;
      if (targetLabels.Count > 0) {
        int.TryParse(targetLabels[0].Text, out value);
      }
      return value;
    }

    #region sample graph creation

    private static readonly int[] NumChildren = { 1, 2, 3, 5 };

    /// <summary>
    ///   Create a sample graph: a simple tree with <see cref="SimpleSubtreePlacer"/>s assigned to each node.
    ///   This implementation creates three layers with three children each.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      graph.Clear();
      var root = graph.CreateNode();
      graph.SetStyle(root, NewNodeStyle(0));
      var placer = new SingleLayerSubtreePlacer { Transformation = SubtreeTransform.None };
      placers[root] = placer;
      CreateSubtree(graph, root, 1, 2);
    }

    /// <summary>
    ///   Recursively builds a subtree
    /// </summary>
    private void CreateSubtree(IGraph graph, INode root, int layer, int layers) {
      var children = NumChildren[layer];

      for (var i = 0; i < children; i++) {
        var child = graph.CreateNode();
        graph.CreateEdge(root, child);
        graph.SetStyle(child, NewNodeStyle(layer));

        var placer = new SingleLayerSubtreePlacer { Transformation = SubtreeTransform.None };
        placers[child] = placer;

        if (layers > 0) {
          CreateSubtree(graph, child, layer + 1, layers - 1);
        }
      }
    }

    private static INodeStyle NewAssistantStyle(int layer) {
      return SubtreePlacerPanel.NewAssistantStyle(GetLayerPalette(layer));
    }

    private static INodeStyle NewNodeStyle(int layer) {
      return DemoStyles.CreateDemoNodeStyle(GetLayerPalette(layer));
    }

    private static Palette GetLayerPalette(int layer) {
      return SubtreePlacerPanel.LayerPalettes[layer % SubtreePlacerPanel.LayerPalettes.Length];
    }

    /////////////////////////// Org Chart ///////////////////////////////////

    /// <summary>
    ///   Create the organization chart using an XML file.
    /// </summary>
    private void CreateOrgChart(IGraph graph) {
      graph.Clear();
      var orgChartModel = XDocument.Load("Resources\\orgchartmodel.xml");
      CreateOrgChartSubtree(graph, null, orgChartModel.Root, 0);
    }

    /// <summary>
    ///   Create a subtree of the organization chart
    /// </summary>
    private void CreateOrgChartSubtree(IGraph graph, INode parent, XElement xml, int layer) {
      // The root of the subtree
      var node = graph.CreateNode();
      // Whether it is an assistant
      var assistantAttribute = xml.Attribute("assistant");
      var assistant = assistantAttribute == null ? false : bool.Parse(assistantAttribute.Value);
      assistants[node] = assistant;

      // Set a style according to the layer and the assistant status
      graph.SetStyle(node, assistant ? NewAssistantStyle(layer) : NewNodeStyle(layer));
      // Connect to the parent
      if (parent != null) {
        graph.CreateEdge(parent, node);
      }
      // Create a placer with the layout which is defined in the xml
      var layoutAttribute = xml.Attribute("layout");
      var layout = layoutAttribute == null ? "" : layoutAttribute.Value;
      var placer = new AssistantSubtreePlacer();
      switch (layout) {
        case "left_below":
          placer.ChildSubtreePlacer = new SingleLayerSubtreePlacer {
              Transformation = SubtreeTransform.RotateRight,
              RootAlignment = SingleLayerSubtreePlacerRootAlignment.LeadingOnBus,
              EdgeRoutingStyle = SingleLayerSubtreePlacerRoutingStyle.OrthogonalAtRoot
          };
          break;
        case "right_below":
          placer.ChildSubtreePlacer = new SingleLayerSubtreePlacer {
              Transformation = SubtreeTransform.RotateLeftFlipY,
              RootAlignment = SingleLayerSubtreePlacerRootAlignment.LeadingOnBus,
              EdgeRoutingStyle = SingleLayerSubtreePlacerRoutingStyle.OrthogonalAtRoot
          };
          break;
        case "below":
          placer.ChildSubtreePlacer = new LeftRightSubtreePlacer { PlaceLastOnBottom = false };
          break;
        default:
          placer.ChildSubtreePlacer = new SingleLayerSubtreePlacer {
              Transformation = SubtreeTransform.None, RootAlignment = SingleLayerSubtreePlacerRootAlignment.Median
          };
          break;
      }
      placers[node] = placer;
      foreach (var xElement in xml.Elements()) {
        CreateOrgChartSubtree(graph, node, xElement, layer + 1);
      }
    }

    #endregion
  }
}
