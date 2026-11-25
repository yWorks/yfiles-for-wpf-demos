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
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Utils;
using LayoutEdge = yWorks.Layout.Graph.Edge;
using LayoutNode = yWorks.Layout.Graph.Node;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Configuration options for the Biofabric visualization.
  /// </summary>
  public sealed class BiofabricOptions
  {
    /// <summary>The property key in the node's tag used to group nodes together.</summary>
    public string? NodeGroupDataKey { get; init; }

    /// <summary>The property key in the edge's tag used to group edges together.</summary>
    public string? EdgeGroupDataKey { get; init; }

    /// <summary>The strategy used to sort nodes vertically; defaults to degree descending.</summary>
    public NodeOrderingKey? NodeOrderingKey { get; init; }

    /// <summary>The strategy used to sort edges horizontally; defaults to edge length.</summary>
    public EdgeOrderingKey? EdgeOrderingKey { get; init; }

    /// <summary>The strategy used to sort node groups vertically; defaults to largest group first.</summary>
    public NodeGroupOrderingKey? NodeGroupOrderingKey { get; init; }

    /// <summary>The strategy used to sort edge groups horizontally; defaults to largest group first.</summary>
    public EdgeGroupOrderingKey? EdgeGroupOrderingKey { get; init; }

    /// <summary>The absolute width of the biofabric and the maximum width of a node row.</summary>
    public double? BiofabricWidth { get; init; }

    /// <summary>The absolute height of the biofabric and the maximum length of an edge.</summary>
    public double? BiofabricHeight { get; init; }

    /// <summary>
    ///   The optional smallest vertical distance between nodes. If omitted, it is calculated from the
    ///   number of visual elements and the requested layout height.
    /// </summary>
    public double? UnitHeight { get; init; }

    /// <summary>
    ///   The optional smallest horizontal distance between edges. If omitted, it is calculated from
    ///   the number of visual elements and the requested layout width.
    /// </summary>
    public double? UnitWidth { get; init; }

    /// <summary>
    ///   The empty horizontal space on each side of a node row. The left padding also separates node
    ///   labels from the first edge.
    /// </summary>
    public double? HorizontalPadding { get; init; }

    /// <summary>The vertical gap between node groups, measured in unit heights.</summary>
    public double? NodeGroupSpacing { get; init; }

    /// <summary>The height of each node row, measured in unit heights; must be greater than 0 and less than 1.</summary>
    public double? NodeSpacing { get; init; }

    /// <summary>The width of each edge column, measured in unit widths; must be greater than 0 and less than 1.</summary>
    public double? EdgeSpacing { get; init; }

    /// <summary>The horizontal gap between edge groups, measured in unit widths.</summary>
    public double? EdgeGroupSpacing { get; init; }

    /// <summary>
    ///   Optional callback invoked when a graph item or node/edge group is highlighted. The argument
    ///   is the highlighted item or group, and can be <see langword="null"/> when no item is available.
    /// </summary>
    public Action<object?>? HighlightItemCallback { get; init; }

    /// <summary>Optional callback invoked when the Biofabric's highlights are cleared.</summary>
    public Action? ClearItemCallback { get; init; }

    /// <summary>The color mode to use for edges; by default, edges use no group-based coloring.</summary>
    public EdgeColorMode? EdgeColorMode { get; init; }

    /// <summary>Palette used to color nodes or edges according to their group association.</summary>
    public IReadOnlyList<string>? EdgeColors { get; init; }

    /// <summary>The mode used to determine node widths; by default, every node spans the full width.</summary>
    public NodeWidthMode? NodeWidthMode { get; init; }

    /// <summary>Whether edges are first sorted by the topmost node they connect; defaults to <see langword="true"/>.</summary>
    public bool? FormStaircases { get; init; }
  }

  /// <summary>
  ///   Creates and manages a Biofabric visualization, including its layout, styles, grouping, and interaction.
  /// </summary>
  public sealed class Biofabric
  {
    /// <summary>Gets the graph control whose graph is rendered as a biofabric.</summary>
    public GraphControl GraphComponent { get; }

    /// <summary>Gets the tag key used to read node IDs for lexicographical ordering.</summary>
    public string NodeIdKey { get; }

    /// <summary>Gets the tag key used to read edge IDs for lexicographical ordering.</summary>
    public string EdgeIdKey { get; }

    /// <summary>Gets or sets how much of the horizontal biofabric width each node row spans.</summary>
    public NodeWidthMode NodeWidthMode { get; set; } = NodeWidthMode.Full;

    /// <summary>Gets or sets whether edges are ordered by their upper endpoint before other criteria.</summary>
    public bool FormStaircases { get; set; } = true;

    private readonly List<IModelItem> highlightedItems = [];

    public bool AnythingHighlighted => highlightedItems.Count > 0;

    public bool IsHighlighted(IModelItem item) => highlightedItems.Contains(item);

    private readonly Dictionary<IModelItem, object> originalStyles = new();

    /// <summary>Gets the node groups currently present in the graph, indexed by group ID.</summary>
    public Dictionary<string, NodeGroup> NodeGroups { get; private set; } = new();

    /// <summary>Gets the edge groups currently present in the graph, indexed by group ID.</summary>
    public Dictionary<string, EdgeGroup> EdgeGroups { get; private set; } = new();

    /// <summary>Gets or sets the ordering strategy for individual nodes.</summary>
    public NodeOrderingKey NodeOrderingKey { get; set; } = NodeOrderingKey.Degree;

    /// <summary>Gets or sets the ordering strategy for individual edges.</summary>
    public EdgeOrderingKey EdgeOrderingKey { get; set; } = EdgeOrderingKey.EdgeLength;

    /// <summary>Gets or sets the ordering strategy for node groups.</summary>
    public NodeGroupOrderingKey NodeGroupOrderingKey { get; set; } = NodeGroupOrderingKey.CardinalityDescending;

    /// <summary>Gets or sets the ordering strategy for edge groups.</summary>
    public EdgeGroupOrderingKey EdgeGroupOrderingKey { get; set; } = EdgeGroupOrderingKey.CardinalityDescending;

    /// <summary>Gets the layout algorithm used to arrange the graph as a biofabric.</summary>
    public BiofabricLayout BiofabricLayout { get; } = null!;

    /// <summary>Gets or sets the target width of the biofabric layout.</summary>
    public double BiofabricWidth { get; set; } = 2500;

    /// <summary>Gets or sets the target height of the biofabric layout.</summary>
    public double BiofabricHeight { get; set; } = 500;

    /// <summary>Gets the horizontal padding left blank at both ends of each node row.</summary>
    public double HorizontalPadding { get; } = 50;

    private readonly double nodeGroupSpacing = 2;
    private readonly double nodeSpacing = 0.1;
    private readonly double edgeGroupSpacing = 4;

    /// <summary>Gets or sets the callback invoked when an item is highlighted.</summary>
    public Action<object?, bool>? HighlightItemCallback { get; set; }

    /// <summary>Gets or sets the callback invoked when all highlights are cleared.</summary>
    public Action? ClearItemCallback { get; set; }

    /// <summary>Gets or sets the coloring strategy used for edges and groups.</summary>
    public EdgeColorMode EdgeColorMode { get; set; } = EdgeColorMode.None;

    private readonly List<string> edgeColors = [
        "#8dd3c7",
        "#fdb462",
        "#80b1d3",
        "#fb8072",
        "#bebada",
        "#b3de69",
        "#fccde5",
        "#d9d9d9",
        "#bc80bd",
        "#ffffb3"
    ];

    /// <summary>Gets or sets the node tag key that contains each node's group ID.</summary>
    public string? NodeGroupDataKey { get; set; }

    /// <summary>Gets or sets the edge tag key that contains each edge's group ID.</summary>
    public string? EdgeGroupDataKey { get; set; }

    private Dictionary<string, string> edgeColorMap = new();
    private readonly Dictionary<string, NodeGroupRenderer> nodeGroupRenderers = new();
    private readonly Dictionary<string, EdgeGroupRenderer> edgeGroupRenderers = new();
    private readonly Dictionary<string, IRenderTreeElement> nodeGroupVisuals = new();
    private readonly Dictionary<string, IRenderTreeElement> edgeGroupVisuals = new();
    private GenericLayoutData<INode, IEdge, ILabel, ILabel> layoutData;
    private double unitHeight = 10;
    private double unitWidth = 10;
    private double edgeLabelHeight = 10;
    private readonly Dictionary<INode, List<IEdge>> sortedEdgesPerNode = new();

    /// <summary>
    ///   Gets whether the layout dimensions were explicitly provided through
    ///   <see cref="BiofabricOptions.BiofabricWidth"/> and <see cref="BiofabricOptions.BiofabricHeight"/>.
    /// </summary>
    public bool FromTargetDimensions { get; }

    /// <summary>
    ///   Creates a new Biofabric manager instance.
    /// </summary>
    /// <param name="graphComponent">The graph control in which to create the visualization.</param>
    /// <param name="nodeIdKey">The key in a node's tag that denotes its ID.</param>
    /// <param name="edgeIdKey">The key in an edge's tag that denotes its ID.</param>
    /// <param name="options">Configuration for grouping and visual appearance.</param>
    public Biofabric(
        GraphControl graphComponent,
        string nodeIdKey,
        string edgeIdKey,
        BiofabricOptions? options = null) {
      options ??= new BiofabricOptions();
      GraphComponent = graphComponent;
      NodeIdKey = nodeIdKey;
      EdgeIdKey = edgeIdKey;
      NodeOrderingKey = options.NodeOrderingKey ?? NodeOrderingKey;
      EdgeOrderingKey = options.EdgeOrderingKey ?? EdgeOrderingKey;
      NodeGroupOrderingKey = options.NodeGroupOrderingKey ?? NodeGroupOrderingKey;
      EdgeGroupOrderingKey = options.EdgeGroupOrderingKey ?? EdgeGroupOrderingKey;
      BiofabricWidth = options.BiofabricWidth ?? BiofabricWidth;
      BiofabricHeight = options.BiofabricHeight ?? BiofabricHeight;
      unitHeight = options.UnitHeight ?? unitHeight;
      unitWidth = options.UnitWidth ?? unitWidth;
      nodeGroupSpacing = options.NodeGroupSpacing ?? nodeGroupSpacing;
      nodeSpacing = options.NodeSpacing ?? nodeSpacing;
      EdgeSpacing = options.EdgeSpacing ?? EdgeSpacing;
      edgeGroupSpacing = options.EdgeGroupSpacing ?? edgeGroupSpacing;
      NodeGroupDataKey = options.NodeGroupDataKey;
      EdgeGroupDataKey = options.EdgeGroupDataKey;
      EdgeColorMode = options.EdgeColorMode ?? EdgeColorMode;
      NodeWidthMode = options.NodeWidthMode ?? NodeWidthMode;
      FormStaircases = options.FormStaircases ?? FormStaircases;
      HighlightItemCallback = options.HighlightItemCallback is null
          ? null
          : (item, _) => options.HighlightItemCallback(item);
      ClearItemCallback = options.ClearItemCallback;
      if (options.EdgeColors is not null) {
        edgeColors.Clear();
        edgeColors.AddRange(options.EdgeColors);
      }
      GraphComponent.GraphModelManager.PortLayerPolicy = PortLayerPolicy.SeparateLayer;
      SetPortLocationModel();

      // Initialize the Biofabric
      UpdateBiofabric();
      ConfigureInteraction();
      FromTargetDimensions = options.BiofabricHeight is not null && options.BiofabricWidth is not null;
      DetermineUnitHeightAndWidth(FromTargetDimensions);
      SetStyles();
      layoutData = GraphComponent.Graph.CreateGenericLayoutData();

      // Create a new Biofabric layout
      BiofabricLayout = new BiofabricLayout(new BiofabricLayoutOptions {
          NodeComparator = BiofabricOrdering.CreateLayoutNodeOrdering(NodeOrderingKey, this),
          EdgeComparator = BiofabricOrdering.CreateLayoutEdgeOrdering(EdgeOrderingKey, this),
          NodeGroupComparator = BiofabricOrdering.CreateNodeGroupOrdering(NodeGroupOrderingKey, this),
          EdgeGroupComparator = BiofabricOrdering.CreateEdgeGroupOrdering(EdgeGroupOrderingKey, this),
          NodeSpacings = new NodeSpacingOptions {
              Node = nodeSpacing,
              NodeGroup = nodeGroupSpacing
          },
          EdgeSpacings = new EdgeSpacingOptions {
              Edge = EdgeSpacing,
              EdgeGroup = edgeGroupSpacing
          },
          LayoutWidth = BiofabricWidth,
          LayoutHeight = BiofabricHeight,
          UnitHeight = unitHeight,
          UnitWidth = unitWidth,
          NodeWidthMode = NodeWidthMode,
          FormStaircases = FormStaircases,
          Paddings = new PaddingOptions { XPadding = HorizontalPadding },
          EdgeLabelHeight = edgeLabelHeight
      });
      UpdateLayoutProperties();

      // register Biofabric in lookup to use them in the renderers
      var lookupDecorator = GraphComponent.Lookup<ILookupDecorator>();
      lookupDecorator?.AddLookup(typeof(GraphControl), new BiofabricLookup(this));
    }

    /// <summary>
    ///   Installs an absolute port location model on all node ports.
    /// </summary>
    public void SetPortLocationModel() {
      var absoluteFreePortLocationModel = new AbsoluteFreePortLocationModel();
      foreach (var node in GraphComponent.Graph.Nodes) {
        foreach (var port in node.Ports) {
          GraphComponent.Graph.SetPortLocationParameter(port,
              absoluteFreePortLocationModel.CreateParameter(node, port.GetLocation()));
        }
      }
    }

    /// <summary>Gets or sets the width of each edge column in calculated unit widths.</summary>
    /// <value>A value greater than zero and less than one.</value>
    public double EdgeSpacing { get; set; } = 0.3;

    /// <summary>
    ///   Updates the Biofabric's grouping and color data from the graph.
    /// </summary>
    public void UpdateBiofabric() {
      // Create the Node Group -> Node mapping
      SetNodeGroups();
      // Create the Edge Group -> Edge mapping
      SetEdgeGroups();
      // Set the Edge Colormap as a function of the selected mode
      UpdateEdgeColorMap();
    }

    /// <summary>
    ///   Updates the layout comparators and data mappings after ordering or grouping settings change.
    /// </summary>
    /// <param name="resort">Whether the current graph should be sorted again.</param>
    public void UpdateLayoutProperties(bool resort = false) {
      SetLayoutProperties(resort);
      SetLayoutData();
    }

    /// <summary>
    ///   Initialize/clear all data and visuals related to edge and node groups.
    /// </summary>
    private void UninstallGroupVisuals() {
      // Remove all Node Group Visuals and clear the map
      foreach (var nodeGroupVisual in nodeGroupVisuals.Values) {
        GraphComponent.RenderTree.Remove(nodeGroupVisual);
      }
      nodeGroupVisuals.Clear();

      // Remove all Edge Group Visuals and clear the map
      foreach (var edgeGroupVisual in edgeGroupVisuals.Values) {
        GraphComponent.RenderTree.Remove(edgeGroupVisual);
      }
      edgeGroupVisuals.Clear();

      // Reset the renderer objects
      nodeGroupRenderers.Clear();
      edgeGroupRenderers.Clear();
    }

    /// <summary>
    ///   Determines the unit height and width from the graph and requested dimensions.
    /// </summary>
    /// <param name="fromTargetDimensions">
    ///   Whether to derive the unit spacing from the configured overall dimensions or derive the
    ///   overall dimensions from the current unit spacing.
    /// </param>
    public void DetermineUnitHeightAndWidth(bool fromTargetDimensions) {
      SortEdges();
      DetermineUnitHeight(fromTargetDimensions);
      DetermineUnitWidth(fromTargetDimensions);
    }

    /// <summary>
    ///   Determine the unit height as a function of the node groups and overall height.
    /// </summary>
    /// <param name="fromTargetDimensions">
    ///   Whether the unit height is calculated from the requested layout height. When <see langword="false"/>,
    ///   the overall height is calculated from the unit height instead.
    /// </param>
    private void DetermineUnitHeight(bool fromTargetDimensions) {
      // Count the rows that will actually be visible after collapsed groups are represented as one row.
      var nNodeGroups = NodeGroups.Count;
      var unCollapsedNodes = NodeGroups.Values.Where(group => !group.Collapsed).Sum(group => group.Nodes.Count);
      var nUncollapsedGroups = NodeGroups.Values.Count(group => !group.Collapsed);
      // Each expanded group contributes one fewer row than nodes, with spacing added between groups.
      var yUnits = unCollapsedNodes - nUncollapsedGroups + (nNodeGroups - 1) * nodeGroupSpacing;

      // Either fit the unit height into the requested canvas or derive the canvas from the unit height.
      if (fromTargetDimensions) {
        unitHeight = yUnits == 0 ? 0 : BiofabricHeight / yUnits;
      } else {
        BiofabricHeight = yUnits * unitHeight;
      }
    }

    /// <summary>
    ///   Determines the unit width as a function of edges, edge groups, node sets, and total width.
    /// </summary>
    /// <param name="fromTargetDimensions">
    ///   Whether the unit width is calculated from the requested layout width. When <see langword="false"/>,
    ///   the overall width is calculated from the unit width instead.
    /// </param>
    private void DetermineUnitWidth(bool fromTargetDimensions) {
      // Collapsed edge groups occupy one column; group gaps are measured in unit widths.
      var nUncollapsedEdges = EdgeGroups.Values.Where(group => !group.Collapsed).Sum(group => group.Edges.Count);
      var nUncollapsedEdgeGroups = EdgeGroups.Values.Count(group => !group.Collapsed);
      var xUnits = nUncollapsedEdges - nUncollapsedEdgeGroups + (EdgeGroups.Count - 1) * edgeGroupSpacing;
      // Labels still need room for every edge, including those in collapsed groups.
      var labelUnits = GraphComponent.Graph.Edges.Count - EdgeGroups.Count + (EdgeGroups.Count - 1) * edgeGroupSpacing;

      // Either fit the unit width into the requested canvas or derive the canvas from the unit width.
      if (fromTargetDimensions) {
        unitWidth = xUnits == 0 ? 0 : (BiofabricWidth - 2 * HorizontalPadding) / xUnits;
        edgeLabelHeight = labelUnits == 0 ? unitWidth : (BiofabricWidth - 2 * HorizontalPadding) / labelUnits;
      } else {
        BiofabricWidth = xUnits * unitWidth + 2 * HorizontalPadding;
      }
    }

    /// <summary>
    ///   Determine the edges' overall order across edge groups and node sets.
    /// </summary>
    private void SortEdges() {
      // Assign every edge to exactly one node: the first node in the configured node ordering that touches it.
      var assignedEdges = new HashSet<IEdge>();
      sortedEdgesPerNode.Clear();
      var sortedNodes = GraphComponent.Graph.Nodes.ToList();
      sortedNodes.Sort((a, b) => BiofabricOrdering.CreateGraphNodeOrdering(NodeOrderingKey, this)(a, b));
      foreach (var node in sortedNodes) {
        var edgesToDraw = new List<IEdge>();
        // Ignore edges already assigned to the other endpoint.
        foreach (var edge in GraphComponent.Graph.EdgesAt(node)) {
          if (assignedEdges.Add(edge)) {
            edgesToDraw.Add(edge);
          }
        }
        // Order the edges assigned to this node before storing them for layout.
        edgesToDraw.Sort((a, b) => BiofabricOrdering.CreateGraphEdgeOrdering(EdgeOrderingKey, this)(a, b));
        sortedEdgesPerNode[node] = edgesToDraw;
      }
    }

    /// <summary>
    ///   Returns the nodes in their group order and configured node order.
    /// </summary>
    public IReadOnlyList<INode> SortNodes() {
      // Preserve the already sorted group order and sort members within each group.
      var sorted = new List<INode>();
      var ordering = BiofabricOrdering.CreateGraphNodeOrdering(NodeOrderingKey, this);
      foreach (var nodeGroup in NodeGroups.Values) {
        var nodes = nodeGroup.Nodes.ToList();
        nodes.Sort((a, b) => ordering(a, b));
        sorted.AddRange(nodes);
      }
      return sorted;
    }

    /// <summary>
    ///   Creates the renderers and visuals for enabled node and edge groups, and stores them for later
    ///   updates and hit testing.
    /// </summary>
    private void AddGroupVisuals() {
      UninstallGroupVisuals();
      AddNodeGroupVisuals();
      AddEdgeGroupVisuals();
    }

    private void AddEdgeGroupVisuals() {
      if (EdgeGroups.Count == 0 || (EdgeGroups.Count == 1 && EdgeGroups.ContainsKey("Ungrouped"))) {
        return;
      }
      // Calculate the vertical position from the visualized edge labels.
      var edgeLabelOffsets = GraphComponent.Graph.Edges.SelectMany(edge => edge.Labels)
          .Select(label => label.GetLayout().GetBounds().Y).ToList();
      var edgeGroupOffset = edgeLabelOffsets.Count > 0 ? edgeLabelOffsets.Min() : 0;

      foreach (var (groupName, edgeGroup) in EdgeGroups) {
        // Only edge-group coloring applies to these group markers.
        var color = EdgeColorMode == EdgeColorMode.EdgeGroups &&
            edgeColorMap.TryGetValue(groupName, out var mappedColor)
                ? mappedColor
                : null;
        var renderer = new EdgeGroupRenderer(edgeGroupOffset != 0 ? edgeGroupOffset * 1.25 : -2 * unitHeight,
            Math.Min(0.3 * unitHeight, 20), color);
        edgeGroupRenderers[groupName] = renderer;
        var visual = GraphComponent.RenderTree.CreateElement(GraphComponent.RenderTree.ContentGroup,
            new EdgeGroupRendererRenderTag(groupName, edgeGroup), renderer);
        edgeGroupVisuals[groupName] = visual;
      }
    }

    private void AddNodeGroupVisuals() {
      if (NodeGroups.Count == 0 || (NodeGroups.Count == 1 && NodeGroups.ContainsKey("Ungrouped"))) {
        return;
      }
      // Calculate the horizontal position from the visualized node labels.
      var nodeLabelOffsets = GraphComponent.Graph.Nodes.SelectMany(node => node.Labels)
          .Select(label => label.GetLayout().AnchorX).ToList();
      var nodeGroupOffset = nodeLabelOffsets.Count > 0 ? nodeLabelOffsets.Min() : 0;

      foreach (var (groupName, nodeGroup) in NodeGroups) {
        // Node-group markers use the palette only in node-group color modes.
        var useNodeGroups = EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted;
        var color = useNodeGroups && edgeColorMap.TryGetValue(groupName, out var mappedColor) ? mappedColor : null;
        var renderer = new NodeGroupRenderer(nodeGroupOffset - 50, Math.Min(0.3 * unitHeight, 20), color);
        nodeGroupRenderers[groupName] = renderer;
        var visual = GraphComponent.RenderTree.CreateElement(GraphComponent.RenderTree.ContentGroup,
            new NodeGroupRendererRenderTag(groupName, nodeGroup), renderer);
        nodeGroupVisuals[groupName] = visual;
      }
    }

    /// <summary>
    ///   If <see cref="NodeGroupDataKey"/> is specified, creates a map from each node-group ID to its
    ///   group data: member nodes, ID, collapsed state, and highlighted state. Otherwise, all nodes
    ///   are placed in the ungrouped group.
    /// </summary>
    private void SetNodeGroups() {
      NodeGroups = new Dictionary<string, NodeGroup>();
      var nodeOrdering = BiofabricOrdering.CreateGraphNodeOrdering(NodeOrderingKey, this);
      if (!string.IsNullOrEmpty(NodeGroupDataKey)) {
        // Collect graph nodes by the group ID stored in each node's tag.
        foreach (var node in GraphComponent.Graph.Nodes) {
          var groupName = GetNodeGroupName(node) ?? "Ungrouped";
          if (!NodeGroups.TryGetValue(groupName, out var group)) {
            group = new NodeGroup { Id = groupName };
            NodeGroups[groupName] = group;
          }
          group.Nodes.Add(node);
        }
        // Sort group members, then sort the groups themselves for stable layout order.
        foreach (var group in NodeGroups.Values) {
          group.Nodes.Sort((a, b) => nodeOrdering(a, b));
        }
        var groupOrdering = BiofabricOrdering.CreateNodeGroupOrdering(NodeGroupOrderingKey, this);
        NodeGroups = SortGroups(NodeGroups, (a, b) => groupOrdering(a, b));
      } else {
        // Without a group key, treat the entire graph as one ungrouped node group.
        var group = new NodeGroup { Id = "Ungrouped" };
        group.Nodes.AddRange(GraphComponent.Graph.Nodes);
        group.Nodes.Sort((a, b) => nodeOrdering(a, b));
        NodeGroups[group.Id] = group;
      }
    }

    /// <summary>
    ///   If <see cref="EdgeGroupDataKey"/> is specified, creates a map from each edge-group ID to its
    ///   group data: member edges, ID, collapsed state, and highlighted state. Otherwise, all edges
    ///   are placed in the ungrouped group.
    /// </summary>
    private void SetEdgeGroups() {
      EdgeGroups = new Dictionary<string, EdgeGroup>();
      var edgeOrdering = BiofabricOrdering.CreateGraphEdgeOrdering(EdgeOrderingKey, this);
      if (!string.IsNullOrEmpty(EdgeGroupDataKey)) {
        // Collect graph edges by the group ID stored in each edge's tag.
        foreach (var edge in GraphComponent.Graph.Edges) {
          var groupName = GetEdgeGroupName(edge) ?? "Ungrouped";
          if (!EdgeGroups.TryGetValue(groupName, out var group)) {
            group = new EdgeGroup { Id = groupName };
            EdgeGroups[groupName] = group;
          }
          group.Edges.Add(edge);
        }
        // Sort group members, then sort the groups themselves for stable layout order.
        foreach (var group in EdgeGroups.Values) {
          group.Edges.Sort((a, b) => edgeOrdering(a, b));
        }
        var groupOrdering = BiofabricOrdering.CreateEdgeGroupOrdering(EdgeGroupOrderingKey, this);
        EdgeGroups = SortGroups(EdgeGroups, (a, b) => groupOrdering(a, b));
      } else {
        // Without a group key, treat the entire graph as one ungrouped edge group.
        var group = new EdgeGroup { Id = "Ungrouped" };
        group.Edges.AddRange(GraphComponent.Graph.Edges);
        group.Edges.Sort((a, b) => edgeOrdering(a, b));
        EdgeGroups[group.Id] = group;
      }
    }

    /// <summary>Returns the group map ordered by its keys using the provided comparison.</summary>
    /// <typeparam name="T">The group data type.</typeparam>
    /// <param name="groups">The map of group IDs to group data.</param>
    /// <param name="comparison">The comparison used to order group IDs.</param>
    /// <returns>A new dictionary whose entries are inserted in comparison order.</returns>
    private static Dictionary<string, T> SortGroups<T>(Dictionary<string, T> groups, Comparison<string> comparison) {
      var entries = groups.ToList();
      entries.Sort((a, b) => comparison(a.Key, b.Key));
      return entries.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    /// <summary>
    ///   Specify the edge color map, based on either node or edge groups.
    /// </summary>
    private void UpdateEdgeColorMap() {
      edgeColorMap.Clear();
      // Assign palette colors in the configured order when coloring by edge group.
      if (EdgeColorMode == EdgeColorMode.EdgeGroups && !string.IsNullOrEmpty(EdgeGroupDataKey)) {
        var edgeGroupOrdering = BiofabricOrdering.CreateEdgeGroupOrdering(EdgeGroupOrderingKey, this);
        var groups = EdgeGroups.Keys.ToList();
        groups.Sort((a, b) => edgeGroupOrdering(a, b));
        for (var i = 0; i < groups.Count; i++) {
          edgeColorMap[groups[i]] = edgeColors[i % edgeColors.Count];
        }
      }
      // Node-group modes replace the edge-group map because edge ends are colored by endpoint group.
      if (EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted &&
          !string.IsNullOrEmpty(NodeGroupDataKey)) {
        var nodeGroupOrdering = BiofabricOrdering.CreateNodeGroupOrdering(NodeGroupOrderingKey, this);
        var groups = NodeGroups.Keys.ToList();
        groups.Sort((a, b) => nodeGroupOrdering(a, b));
        edgeColorMap = new Dictionary<string, string>();
        for (var i = 0; i < groups.Count; i++) {
          edgeColorMap[groups[i]] = edgeColors[i % edgeColors.Count];
        }
      }
    }

    /// <summary>
    ///   Create the mapping of nodes and edges to their respective groups.
    /// </summary>
    private void SetLayoutData() {
      layoutData = GraphComponent.Graph.CreateGenericLayoutData();
      // Create a mapping of edge to its edge group
      layoutData.AddItemMapping(BiofabricLayout.EdgeGroupDataKey).MapperFunction = edge => {
        var edgeGroupName = GetEdgeGroupName(edge);
        // Pass each edge's group ID and collapsed state to the layout algorithm.
        return edgeGroupName is not null && GetEdgeGroupData(edge) is { } edgeGroup
            ? (edgeGroupName, edgeGroup.Collapsed)
            : ("Ungrouped", false);
      };

      // Create a mapping of node to its node group
      layoutData.AddItemMapping(BiofabricLayout.NodeGroupDataKey).MapperFunction = node => {
        var nodeGroupName = GetNodeGroupName(node);
        // Pass each node's group ID and collapsed state to the layout algorithm.
        return nodeGroupName is not null && GetNodeGroupData(node) is { } nodeGroup
            ? (nodeGroupName, nodeGroup.Collapsed)
            : ("Ungrouped", false);
      };
    }

    /// <summary>
    ///   Layout and visualize the nodes, edges, and groups as a biofabric.
    /// </summary>
    /// <param name="animationDuration">
    ///   The duration of the layout animation. A missing or zero duration applies the layout immediately.
    /// </param>
    public async Task RunLayout(TimeSpan? animationDuration = null) {
      // Reinitialize edge labels so the custom edge-label styling can position them freely.
      foreach (var edgeLabel in GraphComponent.Graph.GetEdgeLabels()) {
        if (edgeLabel.LayoutParameter.Model is not FreeLabelModel) {
          GraphComponent.Graph.SetLabelLayoutParameter(edgeLabel,
              FreeLabelModel.Instance.CreateAnchored(PointD.Origin));
        }
      }

      Task layoutTask;
      if (animationDuration is null || animationDuration.Value == TimeSpan.Zero) {
        // Apply the Biofabric layout using the created layout data.
        GraphComponent.Graph.ApplyLayout(BiofabricLayout, layoutData);
        layoutTask = GraphComponent.FitGraphBounds();
      } else {
        // Animate the layout when a non-zero duration was requested.
        layoutTask = new LayoutExecutor(GraphComponent, BiofabricLayout) {
            LayoutData = layoutData,
            AnimationDuration = animationDuration.Value,
            AnimateViewport = false
        }.Start();
      }

      // Apply Biofabric styles and create the group markers alongside the layout operation.
      SetStyles();
      AddGroupVisuals();
      await layoutTask;
    }

    /// <summary>
    ///   Update the biofabric visualization without re-running the layout stage.
    /// </summary>
    public void UpdateVisualization() {
      // Refresh colors and styles after visualization settings change without recalculating positions.
      UpdateEdgeColorMap();
      SetStyles();
      AddGroupVisuals();
      UpdateGroupVisualColors();
    }

    /// <summary>
    ///   Updates the colors of node and edge group visuals using the selected edge-color mode, group
    ///   data keys, and color palette.
    /// </summary>
    private void UpdateGroupVisualColors() {
      // Color node-group markers and clear edge-group marker colors in node-group modes.
      if (EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted &&
          !string.IsNullOrEmpty(NodeGroupDataKey)) {
        foreach (var (groupName, renderer) in nodeGroupRenderers) {
          renderer.Color = edgeColorMap.TryGetValue(groupName, out var color) ? color : null;
        }
        foreach (var renderer in edgeGroupRenderers.Values) {
          renderer.Color = null;
        }
      } else if (EdgeColorMode == EdgeColorMode.EdgeGroups && !string.IsNullOrEmpty(EdgeGroupDataKey)) {
        // Color edge-group markers and clear node-group marker colors in edge-group mode.
        foreach (var (groupName, renderer) in edgeGroupRenderers) {
          renderer.Color = edgeColorMap.TryGetValue(groupName, out var color) ? color : null;
        }
        foreach (var renderer in nodeGroupRenderers.Values) {
          renderer.Color = null;
        }
      } else {
        // No group-based color mode is active, so clear colors from both marker types.
        foreach (var renderer in nodeGroupRenderers.Values) {
          renderer.Color = null;
        }
        foreach (var renderer in edgeGroupRenderers.Values) {
          renderer.Color = null;
        }
      }
      foreach (var visual in nodeGroupVisuals.Values.Concat(edgeGroupVisuals.Values)) {
        // Renderers are retained; invalidate their visuals to show the new colors.
        visual.Invalidate();
      }
    }

    /// <summary>
    ///   Highlight a node, edge, label, or group and execute the configured callback.
    /// </summary>
    /// <param name="item">The graph item to highlight.</param>
    /// <param name="adjacent">Whether the item is adjacent or incident to the primary highlighted item.</param>
    /// <param name="muteCallback">Whether to suppress the highlight callback.</param>
    public void AddHighlight(IModelItem item, bool adjacent = false, bool muteCallback = false) {
      if (highlightedItems.Contains(item)) {
        return;
      }
      var style = GetStyle(item);
      if (style is null) {
        return;
      }
      originalStyles[item] = style;
      // Build a highlighted copy so clearing highlights can restore the original style unchanged.
      var highlightedStyle = CreateHighlightedStyle(style, adjacent);
      if (highlightedStyle is not null) {
        SetStyle(item, highlightedStyle);
        highlightedItems.Add(item);
        var renderTreeElement = GraphComponent.GraphModelManager.GetRenderTreeElement(item);
        if (renderTreeElement is not null) {
          GraphComponent.RenderTree.SetParent(renderTreeElement, GraphComponent.RenderTree.HighlightGroup);
        }
        if (!muteCallback) {
          HighlightItemCallback?.Invoke(item, adjacent);
        }
      }
    }

    private static object? CreateHighlightedStyle(object style, bool adjacent) {
      // Use a subtler color for adjacent items, preserving the original geometry and line widths.
      var brush = adjacent ? BiofabricRenderUtils.DefaultEdge : BiofabricRenderUtils.DefaultHighlight;
      if (style is ShapeNodeStyle nodeStyle) {
        var copy = (ShapeNodeStyle) nodeStyle.Clone();
        copy.Brush = brush;
        copy.Pen = BiofabricRenderUtils.CreatePen(brush, adjacent ? 0 : nodeStyle.Pen?.Thickness ?? 1);
        return copy;
      }
      if (style is CompositeEdgeStyle composite) {
        // Composite edges need each child style highlighted independently.
        var styles = composite.Styles.Select(child => CreateHighlightedStyle(child, adjacent) as IEdgeStyle ?? child)
            .ToArray();
        return new CompositeEdgeStyle(styles[0], [.. styles.Skip(1)]);
      }
      if (style is TransparencyDelegatingEdgeStyle transparencyStyle) {
        var copy = (PolylineEdgeStyle) transparencyStyle.Style.Clone();
        copy.Pen = BiofabricRenderUtils.CreatePen(brush, copy.Pen?.Thickness ?? 1);
        return new TransparencyDelegatingEdgeStyle(copy);
      }
      if (style is PolylineEdgeStyle polylineStyle) {
        var copy = (PolylineEdgeStyle) polylineStyle.Clone();
        copy.Pen = BiofabricRenderUtils.CreatePen(brush, polylineStyle.Pen?.Thickness ?? 1);
        return copy;
      }
      if (style is BiofabricEdgeEndsStyle edgeEndsStyle) {
        // Delegate endpoint markers and gradients to their styles' highlight-aware rendering.
        var copy = (BiofabricEdgeEndsStyle) edgeEndsStyle.Clone();
        copy.IsHighlighted = true;
        return copy;
      }
      if (style is SimpleGradientDelegatingEdgeStyle gradientStyle) {
        var copy = (SimpleGradientDelegatingEdgeStyle) gradientStyle.Clone();
        copy.IsHighlighted = true;
        return copy;
      }
      if (style is ViewportLockedLabelStyle viewportStyle) {
        // Preserve viewport-locking behavior while replacing the wrapped label style.
        var copy = new ViewportLockedLabelStyle(
            (LabelStyle) CreateHighlightedStyle(viewportStyle.WrappedStyle, adjacent)!, viewportStyle.Direction) {
            IsHidden = viewportStyle.IsHidden
        };
        return copy;
      }
      if (style is LabelStyle labelStyle) {
        var copy = (LabelStyle) labelStyle.Clone();
        copy.BackgroundBrush =
            adjacent ? BiofabricRenderUtils.DefaultLabelBackground : BiofabricRenderUtils.DefaultHighlight;
        // copy.BackgroundBrush = BiofabricRenderUtils.DefaultHighlight;
        copy.TextBrush = BiofabricRenderUtils.DefaultBackground;
        return copy;
      }
      return null;
    }

    /// <summary>
    ///   Clear the stored list of highlighted items and restore their styles.
    /// </summary>
    /// <param name="muteCallback">Whether to suppress the clear-highlight callback.</param>
    public void ClearHighlights(bool muteCallback = false) {
      // Restore each item's original style and its normal render-tree layer.
      foreach (var item in highlightedItems) {
        if (originalStyles.TryGetValue(item, out var originalStyle)) {
          SetStyle(item, originalStyle);
          var renderTreeElement = GraphComponent.GraphModelManager.GetRenderTreeElement(item);
          if (renderTreeElement is not null) {
            GraphComponent.RenderTree.SetParent(renderTreeElement, GetDefaultRenderTreeGroup(item));
          }
        }
      }
      // Forget the saved styles after they have been restored.
      highlightedItems.Clear();
      originalStyles.Clear();
      if (!muteCallback) {
        ClearItemCallback?.Invoke();
      }
      foreach (var item in NodeGroups.Values) {
        item.ResetHighlight();
      }
      foreach (var item in EdgeGroups.Values) {
        item.ResetHighlight();
      }
      ;
      // Also reset the visible group-marker state and redraw the control.
      foreach (var renderer in nodeGroupRenderers.Values) {
        renderer.ResetHighlight();
      }
      ;
      foreach (var renderer in edgeGroupRenderers.Values) {
        renderer.ResetHighlight();
      }
      ;
      GraphComponent.Invalidate();
    }

    private IRenderTreeGroup GetDefaultRenderTreeGroup(IModelItem item) {
      // Return the model-manager layer that normally contains each item kind.
      if (item is INode) {
        return GraphComponent.GraphModelManager.NodeGroup;
      }
      if (item is IEdge) {
        return GraphComponent.GraphModelManager.EdgeGroup;
      }
      if (item is ILabel label) {
        return label.Owner is INode
            ? GraphComponent.GraphModelManager.NodeLabelGroup
            : GraphComponent.GraphModelManager.EdgeLabelGroup;
      }
      return GraphComponent.RenderTree.ContentGroup;
    }

    private object? GetStyle(IModelItem item) =>
        item switch {
            INode node => node.Style,
            IEdge edge => edge.Style,
            ILabel label => label.Style,
            _ => null
        };

    private void SetStyle(IModelItem item, object style) {
      // Dispatch to the graph's typed style API for the supported model item kinds.
      switch (item) {
        case INode node when style is INodeStyle nodeStyle:
          GraphComponent.Graph.SetStyle(node, nodeStyle);
          break;
        case IEdge edge when style is IEdgeStyle edgeStyle:
          GraphComponent.Graph.SetStyle(edge, edgeStyle);
          break;
        case ILabel label when style is ILabelStyle labelStyle:
          GraphComponent.Graph.SetStyle(label, labelStyle);
          break;
      }
    }

    /// <summary>
    ///   (Un-)collapse an edge group that has been clicked.
    /// </summary>
    /// <param name="edgeGroup">The edge group that was clicked.</param>
    private async Task ClickEdgeGroup(EdgeGroup edgeGroup) {
      if (edgeGroupRenderers.Count <= 1) {
        return;
      }
      // Toggle collapse, hide its labels, recompute dimensions, and animate the updated layout.
      edgeGroup.Collapsed = !edgeGroup.Collapsed;
      // Labels are viewport-locked, so hide them explicitly while their edge group is collapsed.
      foreach (var edge in edgeGroup.Edges) {
        foreach (var label in edge.Labels) {
          if (label.Style is ViewportLockedLabelStyle style) {
            style.IsHidden = edgeGroup.Collapsed;
          }
        }
      }
      DetermineUnitHeightAndWidth(FromTargetDimensions);
      UpdateLayoutProperties();
      await new LayoutExecutor(GraphComponent, BiofabricLayout) {
          AnimationDuration = TimeSpan.FromSeconds(1),
          LayoutData = layoutData,
          AnimateViewport = false
      }.Start();
    }

    /// <summary>
    ///   (Un-)collapse a node group that has been clicked.
    /// </summary>
    /// <param name="nodeGroup">The node group that was clicked.</param>
    private async Task ClickNodeGroup(NodeGroup nodeGroup) {
      if (nodeGroupRenderers.Count <= 1) {
        return;
      }
      // Toggle collapse, hide its labels, recompute dimensions, and animate the updated layout.
      nodeGroup.Collapsed = !nodeGroup.Collapsed;
      // Labels are viewport-locked, so hide them explicitly while their node group is collapsed.
      foreach (var node in nodeGroup.Nodes) {
        foreach (var label in node.Labels) {
          if (label.Style is ViewportLockedLabelStyle style) {
            style.IsHidden = nodeGroup.Collapsed;
          }
        }
      }
      DetermineUnitHeightAndWidth(FromTargetDimensions);
      UpdateLayoutProperties();
      await new LayoutExecutor(GraphComponent, BiofabricLayout) {
          AnimationDuration = TimeSpan.FromSeconds(1),
          LayoutData = layoutData,
          AnimateViewport = false
      }.Start();
    }

    private static int GetHighlightZIndex(IModelItem item) =>
        item switch {
            INode => 1,
            IEdge => 2,
            ILabel => 3,
            _ => 4
        };

    /// <summary>
    ///   Add pointer move and click handlers that provide the biofabric's interactivity.
    /// </summary>
    private void ConfigureInteraction() {
      var previousElements = new HashSet<IRenderTreeElement>();
      GraphComponent.PointerMove += PointerDebounce.WrapPointerWithMomentumAnalysis((evt, sender, fastMovement) => {
        // Skip hit testing during fast pointer motion to avoid flickering highlights.
        var elements = fastMovement
            ? new HashSet<IRenderTreeElement>()
            : GraphComponent.RenderTree.HitElementsAt(evt.Location, root: GraphComponent.RenderTree.RootGroup)
                .ToHashSet();
        if (elements.SetEquals(previousElements)) {
          return;
        }
        previousElements = elements;
        // Highlights describe the current pointer location, so clear the previous set first.
        ClearHighlights();

        // Give edges priority when an edge overlaps a group marker or another item.
        var edgeElements = elements.Where(element => element.Tag is IEdge).ToList();
        IEnumerable<IRenderTreeElement> prioritizedElements = edgeElements.Count > 0 ? edgeElements : elements;
        var anyNodeGroupHighlighted = false;
        var anyEdgeGroupHighlighted = false;
        foreach (var renderTreeElement in prioritizedElements) {
          if (renderTreeElement.Tag is NodeGroupRendererRenderTag nodeGroupTag) {
            var groupName = nodeGroupTag.GroupName;
            if (NodeGroups.TryGetValue(groupName, out var nodeGroup)) {
              // Highlight all members; labels stay hidden when the group is collapsed.
              HighlightItemCallback?.Invoke(nodeGroup, false);
              foreach (var node in nodeGroup.Nodes) {
                AddHighlight(node);
              }
              if (!nodeGroupTag.NodeGroup.Collapsed) {
                foreach (var node in nodeGroup.Nodes) {
                  foreach (var label in node.Labels) {
                    AddHighlight(label);
                  }
                }
              }
              nodeGroup.Highlighted = true;
              anyNodeGroupHighlighted = true;
              if (nodeGroupRenderers.TryGetValue(groupName, out var renderer)) {
                renderer.IsHighlighted = true;
              }
            }
          }

          if (renderTreeElement.Tag is EdgeGroupRendererRenderTag edgeGroupTag) {
            var groupName = edgeGroupTag.GroupName;
            if (EdgeGroups.TryGetValue(groupName, out var edgeGroup)) {
              // Highlight all members; labels stay hidden when the group is collapsed.
              HighlightItemCallback?.Invoke(edgeGroup, false);
              foreach (var edge in edgeGroup.Edges) {
                AddHighlight(edge);
              }
              if (!edgeGroupTag.EdgeGroup.Collapsed) {
                foreach (var edge in edgeGroup.Edges) {
                  foreach (var label in edge.Labels) {
                    AddHighlight(label);
                  }
                }
              }
              edgeGroup.Highlighted = true;
              anyEdgeGroupHighlighted = true;
              if (edgeGroupRenderers.TryGetValue(groupName, out var renderer)) {
                renderer.IsHighlighted = true;
              }
            }
          }

          if (renderTreeElement.Tag is IModelItem modelItem) {
            // Labels select their owner so hovering one highlights the graph item itself.
            var item = modelItem is ILabel label ? label.Owner : modelItem;
            if (item is IEdge edge) {
              // Highlight an edge and its incident nodes before returning from this hit.
              HighlightEdge(edge);
              SortHighlights();
              return;
            }
            if (item is INode node) {
              // Highlight a node and its incident/adjacent items before returning from this hit.
              HighlightNode(node);
              SortHighlights();
              return;
            }
          }
        }

        if (anyNodeGroupHighlighted) {
          foreach (var group in NodeGroups.Values) {
            group.OtherHighlighted = !group.Highlighted;
          }
        }
        if (anyEdgeGroupHighlighted) {
          foreach (var group in EdgeGroups.Values) {
            group.OtherHighlighted = !group.Highlighted;
          }
        }
        SortHighlights();
      }, 1000);

      GraphComponent.PointerClick += async (_, evt) => {
        // Clicking a group marker toggles that group's collapsed state.
        var renderTreeElements = GraphComponent.RenderTree.HitElementsAt(evt.Location).ToList();
        foreach (var element in renderTreeElements) {
          if (element.Tag is NodeGroupRendererRenderTag nodeTag &&
              NodeGroups.TryGetValue(nodeTag.GroupName, out var nodeGroup)) {
            await ClickNodeGroup(nodeGroup);
          } else if (element.Tag is EdgeGroupRendererRenderTag edgeTag &&
              EdgeGroups.TryGetValue(edgeTag.GroupName, out var edgeGroup)) {
            await ClickEdgeGroup(edgeGroup);
          }
        }
      };
    }

    private void SortHighlights() {
      // Keep nodes behind edges and labels, then raise them in that order.
      highlightedItems.Sort((a, b) => GetHighlightZIndex(a).CompareTo(GetHighlightZIndex(b)));
      foreach (var item in highlightedItems) {
        GraphComponent.GraphModelManager.GetRenderTreeElement(item)?.ToFront();
      }
      foreach (var visual in nodeGroupVisuals.Values.Concat(edgeGroupVisuals.Values)) {
        // Group markers render their own highlighted state and need repainting too.
        visual.Invalidate();
      }
    }

    /// <summary>
    ///   Returns the node group name for the given node.
    /// </summary>
    /// <param name="node">The node whose group ID is requested.</param>
    /// <returns>The node's group ID, or <see langword="null"/> when no group ID is configured or present.</returns>
    private string? GetNodeGroupName(INode node) =>
        string.IsNullOrEmpty(NodeGroupDataKey) ? null : GetTagField(node.Tag, NodeGroupDataKey)?.ToString();

    /// <summary>Returns the node-group data for the given node.</summary>
    /// <param name="node">The node whose group data is requested.</param>
    /// <returns>The node's group, or <see langword="null"/> if it is not present in the group map.</returns>
    private NodeGroup? GetNodeGroupData(INode node) {
      var name = GetNodeGroupName(node);
      return name is not null && NodeGroups.TryGetValue(name, out var group) ? group : null;
    }

    /// <summary>
    ///   Returns the edge group name for the given edge.
    /// </summary>
    /// <param name="edge">The edge whose group ID is requested.</param>
    /// <returns>The edge's group ID, or <c>Ungrouped</c> when no group ID is available.</returns>
    private string? GetEdgeGroupName(IEdge edge) =>
        string.IsNullOrEmpty(EdgeGroupDataKey)
            ? "Ungrouped"
            : GetTagField(edge.Tag, EdgeGroupDataKey)?.ToString() ?? "Ungrouped";

    /// <summary>Returns the edge-group data for the given edge.</summary>
    /// <param name="edge">The edge whose group data is requested.</param>
    /// <returns>The edge's group, or <see langword="null"/> if it is not present in the group map.</returns>
    private EdgeGroup? GetEdgeGroupData(IEdge edge) {
      var name = GetEdgeGroupName(edge);
      return name is not null && EdgeGroups.TryGetValue(name, out var group) ? group : null;
    }

    /// <summary>Reads a named property from a graph item's tag.</summary>
    /// <param name="tag">The tag object to inspect.</param>
    /// <param name="key">The property name to read.</param>
    /// <returns>The property value, or <see langword="null"/> if the tag or property is absent.</returns>
    internal static object? GetTagField(object? tag, string key) {
      if (tag is null) {
        return null;
      }
      if (tag is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object &&
          jsonElement.TryGetProperty(key, out var property)) {
        return property.ValueKind switch {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => property.GetRawText()
        };
      }
      if (tag is IDictionary<string, object?> dictionary && dictionary.TryGetValue(key, out var value)) {
        return value;
      }
      var propertyInfo = tag.GetType().GetProperty(key,
          BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
      return propertyInfo?.GetValue(tag);
    }

    /// <summary>
    ///   Highlight an edge and its incident nodes.
    /// </summary>
    /// <param name="edge">The edge to highlight.</param>
    private void HighlightEdge(IEdge edge) {
      // Highlight every edge when its group is collapsed; otherwise treat the hovered edge alone.
      var edgeGroup = EdgeGroups.Count > 0 && GetEdgeGroupData(edge)?.Collapsed == true
          ? GetEdgeGroupData(edge)
          : new EdgeGroup {
              Id = GetEdgeGroupName(edge) ?? string.Empty,
              Collapsed = false
          };
      if (edgeGroup is null) {
        return;
      }
      if (edgeGroup.Edges.Count == 0) {
        edgeGroup.Edges.Add(edge);
      }
      // Include the edge label and both endpoint groups, which may themselves be collapsed.
      foreach (var currentEdge in edgeGroup.Edges) {
        AddHighlight(currentEdge);
        if (currentEdge.Labels.Count > 0) {
          AddHighlight(currentEdge.Labels[0]);
        }

        // Highlight all source-group members if collapsed; otherwise only the incident source node and label.
        var sourceGroup = NodeGroups.Count > 0 && GetNodeGroupData(currentEdge.GetSourceNode())?.Collapsed == true
            ? GetNodeGroupData(currentEdge.GetSourceNode())
            : new NodeGroup {
                Id = string.Empty,
                Collapsed = false
            };
        if (sourceGroup is not null) {
          if (sourceGroup.Nodes.Count == 0) {
            sourceGroup.Nodes.Add(currentEdge.GetSourceNode());
          }
          foreach (var node in sourceGroup.Nodes) {
            AddHighlight(node);
          }
          if (currentEdge.GetSourceNode().Labels.Count > 0 && !sourceGroup.Collapsed) {
            AddHighlight(currentEdge.GetSourceNode().Labels[0]);
          }
        }

        // Apply the same rule to the target endpoint and its node group.
        var targetGroup = NodeGroups.Count > 0 && GetNodeGroupData(currentEdge.GetTargetNode())?.Collapsed == true
            ? GetNodeGroupData(currentEdge.GetTargetNode())
            : new NodeGroup {
                Id = string.Empty,
                Collapsed = false
            };
        if (targetGroup is not null) {
          if (targetGroup.Nodes.Count == 0) {
            targetGroup.Nodes.Add(currentEdge.GetTargetNode());
          }
          foreach (var node in targetGroup.Nodes) {
            AddHighlight(node);
          }
          if (currentEdge.GetTargetNode().Labels.Count > 0 && !targetGroup.Collapsed) {
            AddHighlight(currentEdge.GetTargetNode().Labels[0]);
          }
        }
      }
    }

    /// <summary>
    ///   Highlight a node, its incident edges, and its adjacent nodes.
    /// </summary>
    /// <param name="highlightedNode">The node to highlight.</param>
    private void HighlightNode(INode highlightedNode) {
      // Expand the highlight to the whole group only when the selected node's group is collapsed.
      var nodeGroup = GetNodeGroupData(highlightedNode)?.Collapsed == true
          ? GetNodeGroupData(highlightedNode)
          : new NodeGroup {
              Id = string.Empty,
              Collapsed = false
          };
      if (nodeGroup is null) {
        return;
      }
      if (nodeGroup.Nodes.Count == 0) {
        nodeGroup.Nodes.Add(highlightedNode);
      }
      foreach (var node in nodeGroup.Nodes) {
        // Highlight the selected group members and their labels.
        AddHighlight(node);
        foreach (var label in node.Labels) {
          AddHighlight(label);
        }

        // Highlight incident edges and the adjacent endpoint nodes and labels.
        foreach (var edge in GraphComponent.Graph.EdgesAt(node)) {
          AddHighlight(edge);
          AddHighlight(edge.GetSourceNode(), true);
          foreach (var label in edge.GetSourceNode().Labels) {
            AddHighlight(label, true);
          }
          AddHighlight(edge.GetTargetNode(), true);
          foreach (var label in edge.GetTargetNode().Labels) {
            AddHighlight(label, true);
          }
          // An edge label is only visible when its edge group is expanded.
          var edgeGroup = EdgeGroups.Count > 0 ? GetEdgeGroupData(edge) : new EdgeGroup { Id = string.Empty };
          if (edge.Labels.Count > 0 && edgeGroup is not null && !edgeGroup.Collapsed) {
            AddHighlight(edge.Labels[0]);
          }
        }
      }
    }

    /// <summary>Gets the colors assigned to the source and target ends of an edge.</summary>
    /// <param name="edge">The edge whose endpoint colors are requested.</param>
    /// <returns>The source-end and target-end colors, which may be <see langword="null"/>.</returns>
    private (string? ColorA, string? ColorB) GetColorsFromEdge(IEdge edge) {
      string? colorA = null;
      string? colorB = null;
      var edgeGroupName = GetEdgeGroupName(edge);
      // Edge-group mode gives the whole edge one color.
      if (EdgeColorMode == EdgeColorMode.EdgeGroups && edgeGroupName is not null) {
        edgeColorMap.TryGetValue(edgeGroupName, out colorA);
      } else if (EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted &&
          !string.IsNullOrEmpty(NodeGroupDataKey)) {
        // Node-group modes color each endpoint; the inverted mode swaps source and target colors.
        var sourceGroup = GetNodeGroupName(edge.GetSourceNode());
        var targetGroup = GetNodeGroupName(edge.GetTargetNode());
        if (sourceGroup is not null && targetGroup is not null) {
          if (sourceGroup == targetGroup) {
            // Edges within one node group use a single color.
            edgeColorMap.TryGetValue(sourceGroup, out colorA);
          } else {
            // Edges between groups use the colors of their two endpoint groups.
            var inverted = EdgeColorMode == EdgeColorMode.NodeGroupsInverted;
            edgeColorMap.TryGetValue(inverted ? targetGroup : sourceGroup, out colorA);
            edgeColorMap.TryGetValue(inverted ? sourceGroup : targetGroup, out colorB);
          }
        }
      }
      return (colorA, colorB);
    }

    /// <summary>
    ///   Set the styles of nodes, node labels, edges, and edge labels.
    /// </summary>
    public void SetStyles() {
      var graph = GraphComponent.Graph;
      // Use narrow pill-shaped node rows with labels locked to the viewport for legibility.
      var nodeBrush = BiofabricRenderUtils.CreateBrush("#89919c");
      var labelBackground = BiofabricRenderUtils.DefaultLabelBackground;
      var lineNodeStyle = new ShapeNodeStyle {
          Brush = nodeBrush,
          Pen = BiofabricRenderUtils.CreatePen(nodeBrush, 1),
          Shape = ShapeNodeShape.Pill,
          KeepIntrinsicAspectRatio = false
      };

      foreach (var node in graph.Nodes) {
        // Scale each node's row height to the calculated unit height.
        graph.SetNodeLayout(node, new RectD(node.Layout.X, node.Layout.Y, node.Layout.Width, unitHeight * nodeSpacing));
        graph.SetStyle(node, (INodeStyle) lineNodeStyle.Clone());

        foreach (var label in node.Labels) {
          // Keep node labels outside the row and lock their text size to the viewport.
          graph.SetLabelLayoutParameter(label, ExteriorNodeLabelModel.Left);
          graph.SetStyle(label,
              new ViewportLockedLabelStyle(
                  new LabelStyle {
                      TextSize = Math.Min(Math.Min(unitHeight * 0.6, 40), unitHeight * 0.75),
                      Shape = LabelShape.Pill,
                      BackgroundBrush = labelBackground,
                      TextBrush = BiofabricRenderUtils.DefaultEdge,
                      TextWrapping = TextWrapping.NoWrap
                  }, LockingOrientation.Horizontal));
        }
      }

      var lineThickness = EdgeSpacing * unitWidth;
      var markerRadius = Math.Min(EdgeSpacing * unitWidth, 0.9 * unitWidth / 2);
      SetEdgeLabelStyle();
      var gradientMap = new Dictionary<string, LinearGradientBrush>();

      foreach (var edge in graph.Edges) {
        var (colorA, colorB) = GetColorsFromEdge(edge);
        var edgeBrush = BiofabricRenderUtils.CreateBrush(colorA, Brushes.Black);
        // Use a solid line for one color; use the delegating style to render different endpoint colors as a gradient.
        if (colorB is null) {
          graph.SetStyle(edge,
              new CompositeEdgeStyle(
                  new TransparencyDelegatingEdgeStyle(new PolylineEdgeStyle {
                      Pen = BiofabricRenderUtils.CreatePen(edgeBrush, lineThickness)
                  }), new BiofabricEdgeEndsStyle(lineThickness, markerRadius, colorA, colorB ?? colorA)));
        } else {
          graph.SetStyle(edge,
              new CompositeEdgeStyle(
                  new SimpleGradientDelegatingEdgeStyle(
                      new PolylineEdgeStyle {
                          Pen = BiofabricRenderUtils.CreatePen(BiofabricRenderUtils.DefaultEdge, lineThickness)
                      }, currentEdge => GetColorsFromEdge(currentEdge)),
                  new BiofabricEdgeEndsStyle(lineThickness, markerRadius, colorA, colorB)));
        }
      }

      GraphComponent.GraphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.Nodes;
      // Keep edge group visuals above node group visuals in the render tree.
      GraphComponent.GraphModelManager.EdgeGroup.Above(GraphComponent.GraphModelManager.NodeGroup);
    }

    private void SetEdgeLabelStyle() {
      // Fix edge labels to the viewport so their text remains readable at every zoom level.
      foreach (var edge in GraphComponent.Graph.Edges) {
        foreach (var label in edge.Labels) {
          GraphComponent.Graph.SetStyle(label,
              new ViewportLockedLabelStyle(
                  new LabelStyle {
                      MaximumSize = new SizeD(double.PositiveInfinity, edgeLabelHeight * 0.5),
                      MinimumSize = new SizeD(0, edgeLabelHeight * 0.5),
                      VerticalTextAlignment = VerticalAlignment.Center,
                      TextAlignment = TextAlignment.Left,
                      TextSize = edgeLabelHeight * 0.5,
                      BackgroundBrush = BiofabricRenderUtils.CreateBrush("#202739"),
                      TextBrush = BiofabricRenderUtils.DefaultEdge,
                      TextWrapping = TextWrapping.NoWrap,
                      Shape = LabelShape.Pill,
                      Padding = new InsetsD(0)
                  }, LockingOrientation.Vertical));
        }
      }
    }

    /// <summary>
    ///   Sets the node, edge, and group comparators and the selected node-width mode on the layout.
    /// </summary>
    /// <param name="resort">Whether to apply the configured edge ordering again instead of preserving the current order.</param>
    private void SetLayoutProperties(bool resort = false) {
      // Preserve the current edge order unless the caller explicitly requests a resort.
      BiofabricLayout.NodeComparator = BiofabricOrdering.CreateLayoutNodeOrdering(NodeOrderingKey, this);
      BiofabricLayout.EdgeComparator = BiofabricOrdering.CreateLayoutEdgeOrdering(
          resort ? EdgeOrderingKey : EdgeOrderingKey.AsIs, this);
      BiofabricLayout.EdgeGroupComparator = BiofabricOrdering.CreateEdgeGroupOrdering(EdgeGroupOrderingKey, this);
      BiofabricLayout.NodeGroupComparator = BiofabricOrdering.CreateNodeGroupOrdering(NodeGroupOrderingKey, this);
      BiofabricLayout.NodeWidthMode = NodeWidthMode;
      BiofabricLayout.UnitWidth = unitWidth;
      BiofabricLayout.UnitHeight = unitHeight;
      BiofabricLayout.EdgeLabelHeight = edgeLabelHeight;
    }

    private class BiofabricLookup : IContextLookupChainLink
    {
      private IContextLookup next;
      private readonly Biofabric biofabric;

      public BiofabricLookup(Biofabric biofabric) {
        this.biofabric = biofabric;
      }

      public object? Lookup(object item, Type type) => type == typeof(Biofabric) ? biofabric : next?.Lookup(item, type);

      public void SetNext(IContextLookup next) {
        this.next = next;
      }
    }
  }

  /// <summary>
  ///   Ordering functions used by Biofabric and its layout algorithm.
  /// </summary>
  internal static class BiofabricOrdering
  {
    /// <summary>Creates an ordering function for layout nodes using the selected strategy.</summary>
    /// <param name="type">The node ordering strategy.</param>
    /// <param name="context">The visualization supplying graph data for comparisons.</param>
    /// <returns>A comparator for layout nodes.</returns>
    public static NodeOrderingFunction<LayoutNode> CreateLayoutNodeOrdering(NodeOrderingKey type, Biofabric context) =>
        (a, b) => type switch {
            NodeOrderingKey.LexicographicalDescending => CompareTagLexicographical(a, b, context.NodeIdKey, false),
            NodeOrderingKey.LexicographicalAscending => CompareTagLexicographical(a, b, context.NodeIdKey, true),
            _ => b.Degree.CompareTo(a.Degree)
        };

    /// <summary>Creates an ordering function for graph nodes using the selected strategy.</summary>
    /// <param name="type">The node ordering strategy.</param>
    /// <param name="context">The visualization supplying graph data for comparisons.</param>
    /// <returns>A comparator for graph nodes.</returns>
    public static NodeOrderingFunction<INode> CreateGraphNodeOrdering(NodeOrderingKey type, Biofabric context) =>
        (a, b) => type switch {
            NodeOrderingKey.LexicographicalDescending => CompareTagLexicographical(a, b, context.NodeIdKey, false),
            NodeOrderingKey.LexicographicalAscending => CompareTagLexicographical(a, b, context.NodeIdKey, true),
            _ => context.GraphComponent.Graph.EdgesAt(b).Count.CompareTo(context.GraphComponent.Graph.EdgesAt(a).Count)
        };

    /// <summary>Creates an ordering function for layout edges using the selected strategy.</summary>
    /// <param name="type">The edge ordering strategy.</param>
    /// <param name="context">The visualization supplying graph data for comparisons.</param>
    /// <returns>A comparator for layout edges.</returns>
    public static EdgeOrderingFunction<LayoutEdge> CreateLayoutEdgeOrdering(EdgeOrderingKey type, Biofabric context) =>
        (a, b) => type switch {
            EdgeOrderingKey.LexicographicalDescending => CompareTagLexicographical(a, b, context.EdgeIdKey, false),
            EdgeOrderingKey.LexicographicalAscending => CompareTagLexicographical(a, b, context.EdgeIdKey, true),
            EdgeOrderingKey.AsIs => b.Source.Layout.X.CompareTo(a.Source.Layout.X),
            _ => Math.Abs(a.Source.Layout.Y - a.Target.Layout.Y)
                .CompareTo(Math.Abs(b.Source.Layout.Y - b.Target.Layout.Y))
        };

    /// <summary>Creates an ordering function for graph edges using the selected strategy.</summary>
    /// <param name="type">The edge ordering strategy.</param>
    /// <param name="context">The visualization supplying graph data for comparisons.</param>
    /// <returns>A comparator for graph edges.</returns>
    public static EdgeOrderingFunction<IEdge> CreateGraphEdgeOrdering(EdgeOrderingKey type, Biofabric context) =>
        (a, b) => type switch {
            EdgeOrderingKey.LexicographicalDescending => CompareTagLexicographical(a, b, context.EdgeIdKey, false),
            EdgeOrderingKey.LexicographicalAscending => CompareTagLexicographical(a, b, context.EdgeIdKey, true),
            EdgeOrderingKey.AsIs => b.GetSourceNode().Layout.X.CompareTo(a.GetSourceNode().Layout.X),
            _ => Math.Abs(a.GetSourceNode().Layout.Y - a.GetTargetNode().Layout.Y)
                .CompareTo(Math.Abs(b.GetSourceNode().Layout.Y - b.GetTargetNode().Layout.Y))
        };

    /// <summary>Creates an ordering function for node-group IDs using the selected strategy.</summary>
    /// <param name="type">The node-group ordering strategy.</param>
    /// <param name="context">The visualization containing the node groups being ordered.</param>
    /// <returns>A comparator for node-group IDs.</returns>
    public static NodeGroupOrderingFunction CreateNodeGroupOrdering(NodeGroupOrderingKey type, Biofabric context) =>
        (a, b) => type switch {
            NodeGroupOrderingKey.CardinalityAscending => CompareGroupCardinality(a, b, context.NodeGroups,
                group => group.Nodes.Count, true),
            NodeGroupOrderingKey.CardinalityDescending => CompareGroupCardinality(a, b, context.NodeGroups,
                group => group.Nodes.Count, false),
            NodeGroupOrderingKey.LexicographicalDescending => CompareLexicographical(b, a),
            _ => CompareLexicographical(a, b)
        };

    /// <summary>Creates an ordering function for edge-group IDs using the selected strategy.</summary>
    /// <param name="type">The edge-group ordering strategy.</param>
    /// <param name="context">The visualization containing the edge groups being ordered.</param>
    /// <returns>A comparator for edge-group IDs.</returns>
    public static EdgeGroupOrderingFunction CreateEdgeGroupOrdering(EdgeGroupOrderingKey type, Biofabric context) =>
        (a, b) => type switch {
            EdgeGroupOrderingKey.CardinalityAscending => CompareGroupCardinality(a, b, context.EdgeGroups,
                group => group.Edges.Count, true),
            EdgeGroupOrderingKey.CardinalityDescending => CompareGroupCardinality(a, b, context.EdgeGroups,
                group => group.Edges.Count, false),
            EdgeGroupOrderingKey.LexicographicalDescending => CompareLexicographical(b, a),
            _ => CompareLexicographical(a, b)
        };

    /// <summary>Compares two tagged graph items by the values stored under the specified tag key.</summary>
    /// <param name="first">The first tagged graph item.</param>
    /// <param name="second">The second tagged graph item.</param>
    /// <param name="key">The tag property key containing each item's ID.</param>
    /// <param name="ascending">Whether to compare the values in ascending order.</param>
    /// <returns>A negative value, zero, or a positive value according to the selected order.</returns>
    /// <exception cref="InvalidOperationException">The ID key is missing.</exception>
    private static int CompareTagLexicographical(ITagOwner first, ITagOwner second, string? key, bool ascending) {
      if (string.IsNullOrEmpty(key)) {
        throw new InvalidOperationException("Ordering requires a valid ID key.");
      }
      var firstValue = Biofabric.GetTagField(first.Tag, key)?.ToString();
      var secondValue = Biofabric.GetTagField(second.Tag, key)?.ToString();
      if (firstValue is null && secondValue is null) {
        return 0;
      }
      if (firstValue is null) {
        return ascending ? -1 : 1;
      }
      if (secondValue is null) {
        return ascending ? 1 : -1;
      }
      return ascending
          ? CompareLexicographical(firstValue, secondValue)
          : CompareLexicographical(secondValue, firstValue);
    }

    /// <summary>Compares two groups by their member counts, using their IDs to break ties.</summary>
    /// <typeparam name="T">The group data type.</typeparam>
    /// <param name="first">The first group ID.</param>
    /// <param name="second">The second group ID.</param>
    /// <param name="data">The map containing the group data.</param>
    /// <param name="cardinality">Gets the member count for a group.</param>
    /// <param name="ascending">Whether to compare counts in ascending order.</param>
    /// <returns>A negative value, zero, or a positive value according to the selected order.</returns>
    private static int CompareGroupCardinality<T>(
        string first,
        string second,
        IReadOnlyDictionary<string, T> data,
        Func<T, int> cardinality,
        bool ascending) {
      var firstLength = data.TryGetValue(first, out var firstGroup) ? cardinality(firstGroup) : -1;
      var secondLength = data.TryGetValue(second, out var secondGroup) ? cardinality(secondGroup) : -1;
      if (firstLength != secondLength) {
        return ascending ? firstLength.CompareTo(secondLength) : secondLength.CompareTo(firstLength);
      }
      return ascending ? CompareLexicographical(first, second) : CompareLexicographical(second, first);
    }

    /// <summary>Compares two strings lexicographically, treating digit runs as numeric values.</summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <returns>A negative value, zero, or a positive value according to the strings' order.</returns>
    private static int CompareLexicographical(string first, string second) =>
        CultureInfo.CurrentCulture.CompareInfo.Compare(first, second, CompareOptions.NumericOrdering);
  }
}
