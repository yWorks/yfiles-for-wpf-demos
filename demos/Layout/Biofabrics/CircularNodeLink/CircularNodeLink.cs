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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Utils;
using LayoutEdge = yWorks.Layout.Graph.Edge;
using LayoutEdgeLabel = yWorks.Layout.Graph.EdgeLabel;
using LayoutNode = yWorks.Layout.Graph.Node;
using LayoutNodeLabel = yWorks.Layout.Graph.NodeLabel;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Configuration options for the circular node-link diagram.
  /// </summary>
  public sealed class CircularNodeLinkOptions
  {
    /// <summary>The property key in the node's tag used to group nodes together.</summary>
    public string? NodeGroupDataKey { get; init; }

    /// <summary>The property key in the edge's tag used to group edges together.</summary>
    public string? EdgeGroupDataKey { get; init; }

    /// <summary>The strategy used to sort nodes around the circle.</summary>
    public NodeOrderingKey? NodeOrderingKey { get; init; }

    /// <summary>The strategy used to order edges when assigning edge-group colors.</summary>
    public EdgeOrderingKey? EdgeOrderingKey { get; init; }

    /// <summary>The strategy used to sort node groups.</summary>
    public NodeGroupOrderingKey? NodeGroupOrderingKey { get; init; }

    /// <summary>The strategy used to sort edge groups.</summary>
    public EdgeGroupOrderingKey? EdgeGroupOrderingKey { get; init; }

    /// <summary>The strategy used to color edges; the default is <see cref="EdgeColorMode.None"/>.</summary>
    public EdgeColorMode? EdgeColorMode { get; init; }

    /// <summary>The thickness of edges in graph coordinate units; the default is 2.</summary>
    public double? EdgeThickness { get; init; }

    /// <summary>Palette assigned cyclically to groups in group-coloring modes.</summary>
    public IReadOnlyList<string>? EdgeColors { get; init; }

    /// <summary>
    ///   Callback invoked when a graph item or node group is highlighted. The Boolean argument is
    ///   <see langword="true"/> for an item highlighted as adjacent to the primary item.
    /// </summary>
    public Action<object?, bool>? HighlightItemCallback { get; init; }

    /// <summary>Callback invoked after all item and group highlights are cleared.</summary>
    public Action? ClearItemCallback { get; init; }
  }

  /// <summary>
  ///   Creates and manages a circular node-link visualization, including group arcs,
  ///   edge coloring, layout, and hover highlighting.
  /// </summary>
  public sealed class CircularNodeLink
  {
    /// <summary>Gets the graph control that displays the circular node-link diagram.</summary>
    public GraphControl GraphComponent { get; }

    /// <summary>Gets the graph being arranged and styled by this visualization.</summary>
    public IGraph Graph { get; }

    /// <summary>Gets the tag-property key used to read node IDs for lexicographical ordering.</summary>
    public string NodeIDKey { get; }

    /// <summary>Gets the tag-property key used to read edge IDs for lexicographical ordering.</summary>
    public string EdgeIDKey { get; }

    /// <summary>Gets or sets the ordering strategy for nodes around the circle.</summary>
    public NodeOrderingKey NodeOrderingKey { get; set; } = NodeOrderingKey.Degree;

    /// <summary>Gets or sets the ordering strategy used when ordering edges.</summary>
    public EdgeOrderingKey EdgeOrderingKey { get; set; } = EdgeOrderingKey.EdgeLength;

    /// <summary>Gets or sets the tag-property key containing each node's group ID.</summary>
    public string? NodeGroupDataKey { get; set; }

    /// <summary>Gets or sets the tag-property key containing each edge's group ID.</summary>
    public string? EdgeGroupDataKey { get; set; }

    /// <summary>Gets or sets the ordering strategy for node groups.</summary>
    public NodeGroupOrderingKey NodeGroupOrderingKey { get; set; } = NodeGroupOrderingKey.CardinalityDescending;

    /// <summary>Gets or sets the ordering strategy for edge groups.</summary>
    public EdgeGroupOrderingKey EdgeGroupOrderingKey { get; set; } = EdgeGroupOrderingKey.CardinalityDescending;

    /// <summary>Gets or sets the strategy used to assign colors to edges and groups.</summary>
    public EdgeColorMode EdgeColorMode { get; set; } = EdgeColorMode.None;

    /// <summary>Gets the current node groups, indexed by group ID.</summary>
    public Dictionary<string, NodeGroup> NodeGroups { get; private set; } = new();

    /// <summary>Gets the current edge groups, indexed by group ID.</summary>
    public Dictionary<string, EdgeGroup> EdgeGroups { get; private set; } = new();

    /// <summary>
    ///   Gets or sets the callback invoked when an item or node group is highlighted. Its Boolean
    ///   argument indicates whether an item is adjacent to the primary highlight.
    /// </summary>
    public Action<object?, bool>? HighlightItemCallback { get; set; }

    /// <summary>Gets or sets the callback invoked when all highlights are cleared.</summary>
    public Action? ClearItemCallback { get; set; }

    // Visual appearance.

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

    private Dictionary<string, string> edgeColorMap = new();
    private readonly Dictionary<string, CircularNodeGroupRenderer> nodeGroupRenderers = new();
    private readonly Dictionary<string, IRenderTreeElement> nodeGroupVisuals = new();
    private readonly List<IModelItem> highlightedItems = [];
    private readonly Dictionary<IModelItem, object> originalStyles = new();

    /// <summary>
    ///   Creates a new circular node-link visualization.
    /// </summary>
    /// <param name="graphComponent">The graph control to visualize.</param>
    /// <param name="nodeIDKey">The property key in a node tag used as its identifier.</param>
    /// <param name="edgeIDKey">The property key in an edge tag used as its identifier.</param>
    /// <param name="options">Optional grouping, ordering, and appearance settings.</param>
    public CircularNodeLink(
        GraphControl graphComponent,
        string nodeIDKey,
        string edgeIDKey,
        CircularNodeLinkOptions? options = null) {
      options ??= new CircularNodeLinkOptions();
      GraphComponent = graphComponent;
      Graph = GraphComponent.Graph;
      NodeIDKey = nodeIDKey;
      EdgeIDKey = edgeIDKey;
      NodeGroupDataKey = options.NodeGroupDataKey;
      EdgeGroupDataKey = options.EdgeGroupDataKey;
      NodeOrderingKey = options.NodeOrderingKey ?? NodeOrderingKey;
      EdgeOrderingKey = options.EdgeOrderingKey ?? EdgeOrderingKey;
      NodeGroupOrderingKey = options.NodeGroupOrderingKey ?? NodeGroupOrderingKey;
      EdgeGroupOrderingKey = options.EdgeGroupOrderingKey ?? EdgeGroupOrderingKey;
      EdgeColorMode = options.EdgeColorMode ?? EdgeColorMode;
      EdgeThickness = options.EdgeThickness ?? EdgeThickness;
      if (options.EdgeColors is not null) {
        edgeColors.Clear();
        edgeColors.AddRange(options.EdgeColors);
      }
      HighlightItemCallback = options.HighlightItemCallback;
      ClearItemCallback = options.ClearItemCallback;

      // Build group data before styling and interaction need to query it.
      CreateNodeGroups();
      CreateEdgeGroups();
      UpdateEdgeColorMap();
      ConfigureInteraction();
      ConfigureStyles();
      // Start the initial layout and add the group arcs that will follow its node positions.
      _ = ApplyLayout();
      AddGroupVisuals();
    }

    /// <summary>
    ///   Gets or sets the edge thickness in graph coordinate units.
    /// </summary>
    /// <value>A non-negative thickness used by the edge styles.</value>
    public double EdgeThickness { get; set; } = 2;

    /// <summary>
    ///   Recreates the node groups from the current graph.
    /// </summary>
    private void CreateNodeGroups() {
      NodeGroups = new Dictionary<string, NodeGroup>();
      if (string.IsNullOrEmpty(NodeGroupDataKey)) {
        return;
      }
      // Group nodes by the configured tag property and order members for the circular layout.
      foreach (var grouping in Graph.Nodes.GroupBy(GetNodeGroupName).Where(group => group.Key is not null)) {
        var nodes = grouping.ToList();
        var ordering = CreateNodeOrdering(NodeOrderingKey);
        nodes.Sort((a, b) => ordering(a, b));
        NodeGroups[grouping.Key!] = new NodeGroup { Id = grouping.Key! };
        NodeGroups[grouping.Key!].Nodes.AddRange(nodes);
      }
    }

    /// <summary>
    ///   Recreates the edge groups from the current graph.
    /// </summary>
    private void CreateEdgeGroups() {
      EdgeGroups = new Dictionary<string, EdgeGroup>();
      if (string.IsNullOrEmpty(EdgeGroupDataKey)) {
        return;
      }
      // Edge groups are used for edge coloring; their order does not change the node-link layout.
      foreach (var grouping in Graph.Edges.GroupBy(GetEdgeGroupName).Where(group => group.Key is not null)) {
        var group = new EdgeGroup { Id = grouping.Key! };
        group.Edges.AddRange(grouping);
        EdgeGroups[grouping.Key!] = group;
      }
    }

    /// <summary>
    ///   Updates the edge color map based on the current edge color mode.
    /// </summary>
    private void UpdateEdgeColorMap() {
      edgeColorMap.Clear();
      // Assign palette colors in the configured edge-group order.
      if (EdgeColorMode == EdgeColorMode.EdgeGroups && !string.IsNullOrEmpty(EdgeGroupDataKey)) {
        var groups = EdgeGroups.Keys.ToList();
        var ordering = CreateEdgeGroupOrdering(EdgeGroupOrderingKey);
        groups.Sort((a, b) => ordering(a, b));
        for (var i = 0; i < groups.Count; i++) {
          edgeColorMap[groups[i]] = edgeColors[i % edgeColors.Count];
        }
      }
      // Node-group modes color the two ends of an edge by their node groups.
      if (EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted &&
          !string.IsNullOrEmpty(NodeGroupDataKey)) {
        var groups = NodeGroups.Keys.ToList();
        var ordering = CreateNodeGroupOrdering(NodeGroupOrderingKey);
        groups.Sort((a, b) => ordering(a, b));
        edgeColorMap = new Dictionary<string, string>();
        for (var i = 0; i < groups.Count; i++) {
          edgeColorMap[groups[i]] = edgeColors[i % edgeColors.Count];
        }
      }
    }

    /// <summary>Registers pointer-move handling for group and graph-item highlighting.</summary>
    private void ConfigureInteraction() {
      IReadOnlyList<IRenderTreeElement> previousElements = [];
      GraphComponent.PointerMove += PointerDebounce.WrapPointerWithMomentumAnalysis((evt, sender, movingFast) => {
        // Skip hit testing during fast movement and avoid reprocessing unchanged hit results.
        var hitElements = movingFast
            ? new List<IRenderTreeElement>()
            : sender.RenderTree.HitElementsAt(evt.Location).ToList();
        if (hitElements.Count == previousElements.Count && hitElements.SequenceEqual(previousElements)) {
          return;
        }
        previousElements = hitElements;
        ClearHighlights();
        if (hitElements.Count == 0) {
          return;
        }

        // Group visuals are hit-tested before graph items so hovering an arc highlights its members.
        var renderTag = hitElements[0].Tag;
        if (renderTag is CircularNodeGroupRenderTag groupTag) {
          if (HighlightItemCallback is not null) {
            HighlightItemCallback(groupTag.NodeGroup, false);
          }
          foreach (var node in groupTag.NodeGroup.Nodes) {
            AddHighlight(node);
            foreach (var label in node.Labels) {
              AddHighlight(label);
            }
          }
          groupTag.NodeGroup.Highlighted = true;
          InvalidateGroupVisuals();
          return;
        }

        // Hovering a label highlights its owning node or edge, not the label in isolation.
        if (renderTag is not IModelItem modelItem) {
          return;
        }
        if (modelItem is ILabel labelItem) {
          modelItem = labelItem.Owner;
        }
        if (modelItem is INode nodeItem) {
          // Highlight the node and its labels.
          AddHighlight(nodeItem);
          foreach (var label in nodeItem.Labels) {
            AddHighlight(label);
          }
          // Iterate over all incident edges.
          foreach (var edge in Graph.EdgesAt(nodeItem)) {
            // Highlight the edge and its labels.
            AddHighlight(edge);
            foreach (var label in edge.Labels) {
              AddHighlight(label, muteCallback: true);
            }
            var source = edge.GetSourceNode();
            var target = edge.GetTargetNode();
            // Ensure the source node is not the current item; highlight it and its labels as adjacent.
            if (source != nodeItem) {
              AddHighlight(source, true);
              foreach (var label in source.Labels) {
                AddHighlight(label, true);
              }
            }
            // Ensure the target node is not the current item; highlight it and its labels as adjacent.
            if (target != nodeItem) {
              AddHighlight(target, true);
              foreach (var label in target.Labels) {
                AddHighlight(label, true);
              }
            }
          }
        } else if (modelItem is IEdge edgeItem) {
          // Highlight the edge, its incident nodes, and their labels.
          AddHighlight(edgeItem);
          var source = edgeItem.GetSourceNode();
          var target = edgeItem.GetTargetNode();
          AddHighlight(source);
          AddHighlight(target);
          foreach (var label in source.Labels) {
            AddHighlight(label);
          }
          foreach (var label in target.Labels) {
            AddHighlight(label);
          }
        }

        // bring highlights to front
        foreach (var item in highlightedItems) {
          GraphComponent.GraphModelManager.GetRenderTreeElement(item)?.ToFront();
        }
      }, 1000);
    }

    /// <summary>Applies node and label styles, then configures edge styles for the selected color mode.</summary>
    private void ConfigureStyles() {
      foreach (var node in Graph.Nodes) {
        var groupName = GetNodeGroupName(node);
        var brush = groupName is not null && edgeColorMap.TryGetValue(groupName, out var color)
            ? BiofabricRenderUtils.CreateBrush(color, Brushes.Transparent)
            : BiofabricRenderUtils.DefaultEdge;
        Graph.SetStyle(node, new ShapeNodeStyle {
            Shape = ShapeNodeShape.Pill,
            Brush = brush,
            Pen = BiofabricRenderUtils.CreatePen(BiofabricRenderUtils.DefaultEdge, 1),
            KeepIntrinsicAspectRatio = false
        });
      }

      var labelBackground = BiofabricRenderUtils.CreateBrush("#202739");
      // The circular demo styles node labels; edge labels are left to the graph's existing configuration.
      foreach (var label in Graph.Labels) {
        if (label.Owner is not INode) {
          continue;
        }
        Graph.SetLabelLayoutParameter(label, FreeNodeLabelModel.Center);
        Graph.SetStyle(label, new LabelStyle {
            TextBrush = BiofabricRenderUtils.DefaultEdge,
            TextSize = 30,
            Shape = LabelShape.Pill,
            BackgroundBrush = labelBackground,
            BackgroundPen = BiofabricRenderUtils.CreatePen(BiofabricRenderUtils.DefaultBackground, 0),
            TextWrapping = TextWrapping.NoWrap
        });
      }
      ConfigureEdgeStyle();
    }

    /// <summary>
    ///   Arranges the graph on one circular cycle and updates the group visuals.
    /// </summary>
    /// <param name="animationDuration">Optional animation duration; a missing or zero duration applies the layout immediately.</param>
    /// <returns>A task that completes when the layout has finished.</returns>
    public async Task ApplyLayout(TimeSpan? animationDuration = null) {
      // Create a layout that does not arrange disconnected components separately.
      var circularLayout = new CircularLayout {
          PartitioningPolicy = PartitioningPolicy.SingleCycle,
          NodeLabelPlacement = RadialNodeLabelPlacement.RayLikeLeaves,
          FromSketchMode = false
      };
      circularLayout.ComponentLayout.Enabled = false;

      // Configure routing for edges inside and between partitions.
      var defaultEdgeDescriptor = circularLayout.EdgeDescriptor;
      defaultEdgeDescriptor.InCircleRoutingStyle = RoutingStyle.Curved;
      defaultEdgeDescriptor.OnCircleRoutingStyle = OnCircleRoutingStyle.Curved;
      defaultEdgeDescriptor.CreateControlPoints = true;

      var layout = new CircleGroupSpacerStage(circularLayout, CreateLayoutNodeOrdering(NodeOrderingKey),
          CreateNodeGroupOrdering(NodeGroupOrderingKey));
      var layoutData = CreateLayoutData();
      if (animationDuration is null || animationDuration == TimeSpan.Zero) {
        // Apply synchronously and fit the graph bounds after its positions have been updated.
        Graph.ApplyLayout(layout, layoutData);
        await GraphComponent.FitGraphBounds();
      } else {
        // Animate both the layout and viewport to the new circular arrangement.
        await new LayoutExecutor(GraphComponent, layout) {
            AnimationDuration = animationDuration.Value,
            LayoutData = layoutData,
            AnimateViewport = false
        }.Start();
      }
      InvalidateGroupVisuals();
    }

    /// <summary>
    ///   Adds visuals that represent node groups to the graph control's render tree.
    /// </summary>
    private void AddGroupVisuals() {
      UninstallGroupVisuals();
      if (NodeGroups.Count == 0 || NodeGroups.Count == 1 && NodeGroups.ContainsKey("Ungrouped")) {
        return;
      }

      // Calculate the width of the longest node label to determine the group arc radius.
      var longestNodeLabelWidth = 0d;
      foreach (var nodeGroup in NodeGroups.Values) {
        foreach (var node in nodeGroup.Nodes) {
          var labelValue = GetTagField(node.Tag, "label")?.ToString() ?? string.Empty;
          longestNodeLabelWidth = Math.Max(longestNodeLabelWidth,
              BiofabricRenderUtils.FormatText(labelValue, 40, Brushes.White).Width);
        }
      }

      // Add node group visuals.
      foreach (var (groupName, nodeGroup) in NodeGroups) {
        // Keep each arc outside the widest node label so group text does not overlap node labels.
        var color = EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted &&
            edgeColorMap.TryGetValue(groupName, out var mappedColor)
                ? mappedColor
                : null;
        var renderer = new CircularNodeGroupRenderer(8, longestNodeLabelWidth + 40, color);
        nodeGroupRenderers[groupName] = renderer;
        var renderTag = new CircularNodeGroupRenderTag(
            groupName, nodeGroup, nodeGroup.Nodes.Count > Graph.Nodes.Count * 0.5);
        nodeGroupVisuals[groupName] = GraphComponent.RenderTree.CreateElement(
            GraphComponent.RenderTree.ContentGroup, renderTag, renderer);
      }
    }

    private void UninstallGroupVisuals() {
      // Remove old render-tree elements before replacing the group renderers.
      foreach (var visual in nodeGroupVisuals.Values) {
        GraphComponent.RenderTree.Remove(visual);
      }
      nodeGroupVisuals.Clear();
      nodeGroupRenderers.Clear();
    }

    /// <summary>
    ///   Adds a highlight to the specified graph item.
    ///   If adjacent is true, the adjacent highlight appearance is applied.
    ///   The callback can be muted when the highlight is synchronized with another view.
    /// </summary>
    /// <param name="item">The graph item to highlight.</param>
    /// <param name="adjacent">Whether to use the adjacent-item appearance.</param>
    /// <param name="muteCallback">Whether to suppress the highlight callback.</param>
    public void AddHighlight(IModelItem item, bool adjacent = false, bool muteCallback = false) {
      if (originalStyles.ContainsKey(item)) {
        return;
      }
      var style = GetStyle(item);
      if (style is null) {
        return;
      }
      var highlightedStyle = CreateHighlightedStyle(style, adjacent);
      if (highlightedStyle is null) {
        return;
      }
      originalStyles[item] = style;
      SetStyle(item, highlightedStyle);
      highlightedItems.Add(item);
      if (!muteCallback) {
        HighlightItemCallback?.Invoke(item, adjacent);
      }

      // Bring the highlighted item to the front of the render tree for better visibility
      GraphComponent.GraphModelManager.GetRenderTreeElement(item)?.ToFront();
    }

    /// <summary>
    ///   Removes all highlights from graph items and restores their original styles.
    /// </summary>
    /// <param name="muteCallback">Whether to suppress the clear callback.</param>
    public void ClearHighlights(bool muteCallback = false) {
      // Restore saved styles before dropping the per-item highlight state.
      foreach (var item in highlightedItems) {
        if (originalStyles.TryGetValue(item, out var style)) {
          SetStyle(item, style);
        }
      }
      highlightedItems.Clear();
      originalStyles.Clear();
      if (!muteCallback) {
        ClearItemCallback?.Invoke();
      }
      foreach (var group in NodeGroups.Values) {
        group.Highlighted = false;
      }
      GraphComponent.Invalidate();
      InvalidateGroupVisuals();
    }

    /// <summary>
    ///   Creates generic layout data with a node-to-group mapper for the spacer stage.
    /// </summary>
    private GenericLayoutData<INode, IEdge, ILabel, ILabel> CreateLayoutData() {
      var layoutData = Graph.CreateGenericLayoutData();
      // Supply a group ID for every node; nodes without a configured tag share the ungrouped group.
      layoutData.AddItemMapping(CircleGroupSpacerStage.NodeGroupDataKey).MapperFunction = node =>
          GetNodeGroupName(node) ?? "Ungrouped";
      return layoutData;
    }

    /// <summary>Gets the group ID from the configured property in a node's tag.</summary>
    private string? GetNodeGroupName(INode node) => GetTagField(node.Tag, NodeGroupDataKey)?.ToString();

    /// <summary>Gets the group ID from the configured property in an edge's tag.</summary>
    private string? GetEdgeGroupName(IEdge edge) => GetTagField(edge.Tag, EdgeGroupDataKey)?.ToString();

    /// <summary>Chooses a solid or gradient edge style according to the active group-color mode.</summary>
    private void ConfigureEdgeStyle() {
      foreach (var edge in Graph.Edges) {
        // In edge-group mode, every edge uses the color assigned to its edge group.
        if (EdgeColorMode == EdgeColorMode.EdgeGroups && edgeColorMap.Count > 0) {
          var groupName = GetEdgeGroupName(edge);
          if (groupName is not null && edgeColorMap.TryGetValue(groupName, out var groupColor)) {
            Graph.SetStyle(edge, CreateBezierStyle(BiofabricRenderUtils.CreateBrush(groupColor)));
          } else {
            Graph.SetStyle(edge, CreateBezierStyle(BiofabricRenderUtils.DefaultEdge));
          }
          continue;
        }

        if (EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted && edgeColorMap.Count > 0) {
          // In node-group modes, color the edge from its source and target node groups.
          var sourceGroup = GetNodeGroupName(edge.GetSourceNode());
          var targetGroup = GetNodeGroupName(edge.GetTargetNode());
          if (sourceGroup is not null && targetGroup is not null &&
              edgeColorMap.TryGetValue(sourceGroup, out var sourceColor) &&
              edgeColorMap.TryGetValue(targetGroup, out var targetColor)) {
            if (EdgeColorMode == EdgeColorMode.NodeGroupsInverted) {
              (sourceColor, targetColor) = (targetColor, sourceColor);
            }
            if (sourceColor == targetColor) {
              // If both endpoint groups have the same color, use a regular Bezier edge style.
              Graph.SetStyle(edge, CreateBezierStyle(BiofabricRenderUtils.CreateBrush(sourceColor)));
            } else {
              // Interpolate between different endpoint colors along the edge path.
              Graph.SetStyle(edge,
                  new GradientDelegatingEdgeStyle(CreateBezierStyle(BiofabricRenderUtils.DefaultEdge), sourceColor,
                      targetColor, 250));
            }
            continue;
          }
        }
        // Fall back to the default edge color when group colors are unavailable.
        Graph.SetStyle(edge, CreateBezierStyle(BiofabricRenderUtils.DefaultEdge));
      }
    }

    /// <summary>
    ///   Updates the group collections, style configuration, and group arc visuals.
    /// </summary>
    public void UpdateVisualization() {
      // Recreate node and edge groupings.
      CreateNodeGroups();
      CreateEdgeGroups();
      // Update edge colors and configure graph item styles.
      UpdateEdgeColorMap();
      ConfigureStyles();
      // Add visuals that indicate node group membership.
      AddGroupVisuals();
      // Reflect any newly assigned colors in the recreated group renderers.
      foreach (var (groupName, renderer) in nodeGroupRenderers) {
        renderer.Color = EdgeColorMode is EdgeColorMode.NodeGroups or EdgeColorMode.NodeGroupsInverted &&
            edgeColorMap.TryGetValue(groupName, out var color)
                ? color
                : null;
      }
    }

    /// <summary>Creates a Bezier edge style with the configured edge thickness.</summary>
    private BezierEdgeStyle CreateBezierStyle(Brush brush) =>
        new() { Pen = BiofabricRenderUtils.CreatePen(brush, EdgeThickness) };

    /// <summary>Gets the current style for a supported graph item.</summary>
    private static object? GetStyle(IModelItem item) =>
        item switch {
            INode node => node.Style,
            IEdge edge => edge.Style,
            ILabel label => label.Style,
            _ => null
        };

    private void SetStyle(IModelItem item, object style) {
      // Route the style to the graph API overload that matches the model-item kind.
      switch (item) {
        case INode node when style is INodeStyle nodeStyle:
          Graph.SetStyle(node, nodeStyle);
          break;
        case IEdge edge when style is IEdgeStyle edgeStyle:
          Graph.SetStyle(edge, edgeStyle);
          break;
        case ILabel label when style is ILabelStyle labelStyle:
          Graph.SetStyle(label, labelStyle);
          break;
      }
    }

    private static object? CreateHighlightedStyle(object style, bool adjacent) {
      // Create a separate style copy so clearing highlights can restore the original instance.
      var brush = adjacent ? BiofabricRenderUtils.DefaultEdge : BiofabricRenderUtils.DefaultHighlight;
      if (style is ShapeNodeStyle nodeStyle) {
        var copy = (ShapeNodeStyle) nodeStyle.Clone();
        copy.Brush = brush;
        copy.Pen = BiofabricRenderUtils.CreatePen(brush, adjacent ? 0 : nodeStyle.Pen?.Thickness ?? 1);
        return copy;
      }
      if (style is GradientDelegatingEdgeStyle gradientStyle) {
        return new GradientDelegatingEdgeStyle(gradientStyle.WrappedStyle, gradientStyle.StartColor,
            gradientStyle.EndColor, gradientStyle.SubdivisionTargetLength) {
            HighlightBrush = adjacent ? BiofabricRenderUtils.DefaultEdge : BiofabricRenderUtils.DefaultHighlight
        };
      }
      if (style is BezierEdgeStyle bezierStyle) {
        var copy = (BezierEdgeStyle) bezierStyle.Clone();
        copy.Pen = BiofabricRenderUtils.CreatePen(
            adjacent ? BiofabricRenderUtils.DefaultEdge : BiofabricRenderUtils.DefaultHighlight,
            bezierStyle.Pen?.Thickness ?? 1);
        return copy;
      }

      if (style is LabelStyle labelStyle) {
        var copy = (LabelStyle) labelStyle.Clone();
        copy.BackgroundBrush = adjacent ? BiofabricRenderUtils.DefaultEdge : BiofabricRenderUtils.DefaultHighlight;
        copy.TextBrush = BiofabricRenderUtils.DefaultBackground;
        return copy;
      }
      return null;
    }

    private void InvalidateGroupVisuals() {
      // Group arcs draw their own highlighted state and must be repainted when it changes.
      foreach (var visual in nodeGroupVisuals.Values) {
        visual.Invalidate();
      }
    }

    /// <summary>Creates a comparator for graph nodes using the selected ordering strategy.</summary>
    private NodeOrderingFunction<INode> CreateNodeOrdering(NodeOrderingKey key) =>
        key switch {
            NodeOrderingKey.LexicographicalAscending => (a, b) => CompareTagLexicographical(a, b, NodeIDKey, true),
            NodeOrderingKey.LexicographicalDescending => (a, b) => CompareTagLexicographical(a, b, NodeIDKey, false),
            _ => (a, b) => GetDegree(b) - GetDegree(a)
        };

    /// <summary>Creates a comparator for layout nodes using the selected ordering strategy.</summary>
    private NodeOrderingFunction<LayoutNode> CreateLayoutNodeOrdering(NodeOrderingKey key) =>
        key switch {
            NodeOrderingKey.LexicographicalAscending => (a, b) => CompareTagLexicographical(a, b, NodeIDKey, true),
            NodeOrderingKey.LexicographicalDescending => (a, b) => CompareTagLexicographical(a, b, NodeIDKey, false),
            _ => (a, b) => GetDegree(b) - GetDegree(a)
        };

    /// <summary>Creates a comparator for node-group IDs using the selected ordering strategy.</summary>
    private NodeGroupOrderingFunction CreateNodeGroupOrdering(NodeGroupOrderingKey key) =>
        key switch {
            NodeGroupOrderingKey.CardinalityDescending => (a, b) =>
                CompareGroupCardinality(a, b, NodeGroups, group => group.Nodes.Count, false),
            NodeGroupOrderingKey.LexicographicalAscending => (a, b) => CompareLexicographical(a, b),
            NodeGroupOrderingKey.LexicographicalDescending => (a, b) => CompareLexicographical(b, a),
            _ => (a, b) => CompareGroupCardinality(a, b, NodeGroups, group => group.Nodes.Count, true)
        };

    /// <summary>Creates a comparator for edge-group IDs using the selected ordering strategy.</summary>
    private EdgeGroupOrderingFunction CreateEdgeGroupOrdering(EdgeGroupOrderingKey key) =>
        key switch {
            EdgeGroupOrderingKey.CardinalityDescending => (a, b) =>
                CompareGroupCardinality(a, b, EdgeGroups, group => group.Edges.Count, false),
            EdgeGroupOrderingKey.LexicographicalAscending => (a, b) => CompareLexicographical(a, b),
            EdgeGroupOrderingKey.LexicographicalDescending => (a, b) => CompareLexicographical(b, a),
            _ => (a, b) => CompareGroupCardinality(a, b, EdgeGroups, group => group.Edges.Count, true)
        };

    /// <summary>Returns the number of edges incident to a graph node.</summary>
    private int GetDegree(INode node) => Graph.EdgesAt(node).Count;

    /// <summary>Maps a layout node back to its original graph node to determine its degree.</summary>
    private int GetDegree(LayoutNode node) {
      var nodeMapper = node.Graph?.Context.GetItemData(LayoutGraphAdapter.OriginalNodeDataKey);
      if (nodeMapper is null) {
        throw new InvalidOperationException("No node mapper is defined for the layout graph.");
      }
      return nodeMapper[node] is INode originalNode ? GetDegree(originalNode) : 0;
    }

    private static int CompareTagLexicographical(ITagOwner first, ITagOwner second, string? key, bool ascending) {
      // Missing IDs sort consistently at either end of the requested ordering.
      if (string.IsNullOrEmpty(key)) {
        throw new InvalidOperationException("Ordering requires a valid ID key.");
      }
      var valueA = GetTagField(first.Tag, key)?.ToString();
      var valueB = GetTagField(second.Tag, key)?.ToString();
      if (string.IsNullOrEmpty(valueA) && string.IsNullOrEmpty(valueB)) {
        return 0;
      }
      if (string.IsNullOrEmpty(valueA)) {
        return ascending ? -1 : 1;
      }
      if (string.IsNullOrEmpty(valueB)) {
        return ascending ? 1 : -1;
      }
      return ascending ? CompareLexicographical(valueA, valueB) : CompareLexicographical(valueB, valueA);
    }

    private static int CompareGroupCardinality<T>(
        string first,
        string second,
        IDictionary<string, T> groups,
        Func<T, int> count,
        bool ascending) {
      // Compare group sizes first and use the IDs as a stable tie-breaker.
      var countA = groups.TryGetValue(first, out var groupA) ? count(groupA) : -1;
      var countB = groups.TryGetValue(second, out var groupB) ? count(groupB) : -1;
      if (countA != countB) {
        return ascending ? countA - countB : countB - countA;
      }
      return ascending ? CompareLexicographical(first, second) : CompareLexicographical(second, first);
    }

    private static int CompareLexicographical(string first, string second) {
      // Compare digit runs by numeric value.
      var i = 0;
      var j = 0;
      while (i < first.Length && j < second.Length) {
        if (char.IsDigit(first[i]) && char.IsDigit(second[j])) {
          var firstEnd = i;
          var secondEnd = j;
          while (firstEnd < first.Length && char.IsDigit(first[firstEnd])) {
            firstEnd++;
          }
          while (secondEnd < second.Length && char.IsDigit(second[secondEnd])) {
            secondEnd++;
          }
          var firstDigits = first.AsSpan(i, firstEnd - i).TrimStart('0');
          var secondDigits = second.AsSpan(j, secondEnd - j).TrimStart('0');
          var comparison = firstDigits.Length.CompareTo(secondDigits.Length);
          if (comparison == 0) {
            comparison = firstDigits.CompareTo(secondDigits, StringComparison.Ordinal);
          }
          if (comparison != 0) {
            return comparison;
          }
          i = firstEnd;
          j = secondEnd;
        } else {
          var comparison = char.ToUpperInvariant(first[i]).CompareTo(char.ToUpperInvariant(second[j]));
          if (comparison != 0) {
            return comparison;
          }
          i++;
          j++;
        }
      }
      return (first.Length - i).CompareTo(second.Length - j);
    }

    private static object? GetTagField(object? tag, string? key) {
      if (tag is null || string.IsNullOrEmpty(key)) {
        return null;
      }
      // Support the JSON tags used by demo data as well as dictionaries and regular CLR objects.
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
      return tag.GetType().GetProperty(key)?.GetValue(tag);
    }
  }
}
