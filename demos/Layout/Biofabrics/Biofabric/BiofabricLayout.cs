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
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Utils;
using LayoutEdge = yWorks.Layout.Graph.Edge;
using LayoutEdgeLabel = yWorks.Layout.Graph.EdgeLabel;
using LayoutGraph = yWorks.Layout.Graph.LayoutGraph;
using LayoutNode = yWorks.Layout.Graph.Node;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Configuration options for the Biofabric layout algorithm.
  /// </summary>
  public sealed class BiofabricLayoutOptions
  {
    /// <summary>Function defining the vertical sort order of individual nodes.</summary>
    public required NodeOrderingFunction<LayoutNode> NodeComparator { get; init; }

    /// <summary>Function defining the horizontal sort order of individual edges.</summary>
    public required EdgeOrderingFunction<LayoutEdge> EdgeComparator { get; init; }

    /// <summary>Function defining the sort order of node groups.</summary>
    public required NodeGroupOrderingFunction NodeGroupComparator { get; init; }

    /// <summary>Function defining the sort order of edge groups.</summary>
    public required EdgeGroupOrderingFunction EdgeGroupComparator { get; init; }

    /// <summary>Horizontal spacing configuration.</summary>
    public EdgeSpacingOptions? EdgeSpacings { get; init; }

    /// <summary>Vertical spacing configuration.</summary>
    public NodeSpacingOptions? NodeSpacings { get; init; }

    /// <summary>Padding configuration for the layout boundaries.</summary>
    public PaddingOptions? Paddings { get; init; }

    /// <summary>The selected node width mode.</summary>
    public NodeWidthMode? NodeWidthMode { get; init; }

    /// <summary>Whether edges are first sorted by the topmost node of the source/target pair.</summary>
    public bool? FormStaircases { get; init; }

    /// <summary>The absolute height of the layout.</summary>
    public double LayoutHeight { get; init; }

    /// <summary>The absolute width of the layout.</summary>
    public double LayoutWidth { get; init; }

    /// <summary>The unit height between nodes.</summary>
    public double? UnitHeight { get; init; }

    /// <summary>The unit width between edges.</summary>
    public double? UnitWidth { get; init; }

    /// <summary>The width of each edge label.</summary>
    public double? EdgeLabelHeight { get; init; }
  }

  /// <summary>Configures horizontal spacing for edges and edge groups, in unit widths.</summary>
  public sealed class EdgeSpacingOptions
  {
    /// <summary>The width reserved for an edge column; greater than zero and less than one.</summary>
    public double? Edge { get; init; }

    /// <summary>The gap between edges with different source nodes.</summary>
    public double? Node { get; init; }

    /// <summary>The gap between adjacent edge groups.</summary>
    public double? EdgeGroup { get; init; }
  }

  /// <summary>Configures the height of node rows and the gaps between node groups, in unit heights.</summary>
  public sealed class NodeSpacingOptions
  {
    /// <summary>The height reserved for each node row; greater than zero and less than one.</summary>
    public double? Node { get; init; }

    /// <summary>The gap between nodes in different node groups.</summary>
    public double? NodeGroup { get; init; }
  }

  /// <summary>Configures the padding around the layout.</summary>
  public sealed class PaddingOptions
  {
    /// <summary>The empty horizontal space at both ends of each node row.</summary>
    public double? XPadding { get; init; }
  }

  /// <summary>
  ///   The BiofabricLayout object responsible for laying out a given graph as a biofabric visualization.
  /// </summary>
  public sealed class BiofabricLayout : ILayoutAlgorithm
  {
    /// <summary>Gets or sets the comparator that orders individual nodes vertically.</summary>
    public NodeOrderingFunction<LayoutNode> NodeComparator { get; set; } = (a, b) => b.Degree - a.Degree;

    /// <summary>Gets or sets the comparator that orders individual edges horizontally.</summary>
    public EdgeOrderingFunction<LayoutEdge> EdgeComparator { get; set; } = (a, b) =>
        Math.Abs(a.Source.Layout.Y - a.Target.Layout.Y).CompareTo(Math.Abs(b.Source.Layout.Y - b.Target.Layout.Y));

    /// <summary>Gets or sets the comparator that orders node groups.</summary>
    public NodeGroupOrderingFunction NodeGroupComparator { get; set; } = (_, _) => 0;

    /// <summary>Gets or sets the comparator that orders edge groups.</summary>
    public EdgeGroupOrderingFunction EdgeGroupComparator { get; set; } = (_, _) => 0;

    // Edge/Node Group sorted by corresponding comparator
    private Dictionary<string, LayoutEdgeGroup> EdgeGroups { get; set; } = new();

    private Dictionary<string, LayoutNodeGroup> NodeGroups { get; set; } = new();

    // Intra-Edge Group Node Sorting
    private readonly Dictionary<string, List<LayoutNode>> nodesInEdgeGroup = new();
    private readonly Dictionary<LayoutNode, List<LayoutEdge>> sortedEdgesPerNode = new();

    // Globally sorted nodes
    private readonly List<LayoutNode> sortedNodes = [];

    // Intermediate Data Structures
    private readonly Dictionary<NodePlacement, List<EdgePlacement>> edgesPerNode = new();
    private readonly Dictionary<LayoutNode, NodePlacement> nodeNodeMap = new();

    // Layout Data Mappers
    private IMapper<LayoutEdge, (string, bool)>? edgeGroupMapper;
    private IMapper<LayoutNode, (string, bool)>? nodeGroupMapper;

    // Aesthetics
    /// <summary>Gets or sets how much of the layout width each node row spans.</summary>
    public NodeWidthMode NodeWidthMode { get; set; } = NodeWidthMode.Full;

    /// <summary>Gets or sets whether edges are first ordered by the upper node they connect.</summary>
    public bool FormStaircases { get; set; }

    private readonly Dictionary<EdgeSpacingTypes, double> edgeSpacings;
    private readonly Dictionary<NodeSpacingTypes, double> nodeSpacings;
    private readonly Dictionary<PaddingTypes, double> paddings;

    /// <summary>Gets or sets the absolute width of the biofabric layout.</summary>
    public double LayoutWidth { get; set; } = 2500;

    /// <summary>Gets or sets the horizontal unit distance between edge columns.</summary>
    public double UnitWidth { get; set; } = 10;

    /// <summary>Gets or sets the vertical unit distance between node rows.</summary>
    public double UnitHeight { get; set; } = 10;

    /// <summary>Gets or sets the height reserved for each edge label.</summary>
    public double EdgeLabelHeight { get; set; } = 10;

    // The mapping of edge and node to edge group and node group respectively
    /// <summary>Layout-data key that maps each edge to its group ID and collapsed state.</summary>
    public static readonly EdgeDataKey<(string, bool)> EdgeGroupDataKey = new("BiofabricLayout.EdgeGroupDataKey");

    /// <summary>Layout-data key that maps each node to its group ID and collapsed state.</summary>
    public static readonly NodeDataKey<(string, bool)> NodeGroupDataKey = new("BiofabricLayout.NodeGroupDataKey");

    /// <summary>
    ///   Initializes a new BiofabricLayout algorithm instance.
    /// </summary>
    /// <param name="options">The layout configuration including comparators and spacing values.</param>
    public BiofabricLayout(BiofabricLayoutOptions options) {
      NodeComparator = options.NodeComparator;
      EdgeComparator = options.EdgeComparator;
      NodeGroupComparator = options.NodeGroupComparator;
      EdgeGroupComparator = options.EdgeGroupComparator;
      NodeWidthMode = options.NodeWidthMode ?? NodeWidthMode.Full;
      FormStaircases = options.FormStaircases ?? false;
      LayoutWidth = options.LayoutWidth;
      UnitHeight = options.UnitHeight ?? 10;
      UnitWidth = options.UnitWidth ?? 10;
      EdgeLabelHeight = options.EdgeLabelHeight ?? UnitWidth;
      edgeSpacings = new Dictionary<EdgeSpacingTypes, double> {
          [EdgeSpacingTypes.Edge] = options.EdgeSpacings?.Edge ?? 0.5,
          [EdgeSpacingTypes.Node] = options.EdgeSpacings?.Node ?? 1,
          [EdgeSpacingTypes.EdgeGroup] = options.EdgeSpacings?.EdgeGroup ?? 2
      };
      nodeSpacings = new Dictionary<NodeSpacingTypes, double> {
          [NodeSpacingTypes.Node] = options.NodeSpacings?.Node ?? 0.5,
          [NodeSpacingTypes.NodeGroup] = options.NodeSpacings?.NodeGroup ?? 2
      };
      paddings = new Dictionary<PaddingTypes, double> { [PaddingTypes.XPadding] = options.Paddings?.XPadding ?? 50 };
    }

    /// <summary>
    ///   Wrapper function with which to (re-)initialize/clear all layout-specific objects.
    /// </summary>
    private void ClearLayout() {
      sortedNodes.Clear();
      edgesPerNode.Clear();
      nodeNodeMap.Clear();
      sortedEdgesPerNode.Clear();
      nodesInEdgeGroup.Clear();
      NodeGroups = new Dictionary<string, LayoutNodeGroup>();
      EdgeGroups = new Dictionary<string, LayoutEdgeGroup>();
    }

    /// <summary>
    ///   Wrapper function that lays out the provided graph as a biofabric visualization.
    /// </summary>
    /// <param name="graph">The LayoutGraph to be visualized as a biofabric.</param>
    public void ApplyLayout(LayoutGraph graph) {
      // Clear the layout of all objects
      ClearLayout();

      // Create and sort edge and node groups
      CreateNodeGroups(graph);
      CreateEdgeGroups(graph);

      // Sort Nodes across node groups
      SortNodes();

      // Layout nodes as a function of calculated unit height
      LayoutNodes();

      // Check if Staircases are to be formed by edges
      if (FormStaircases) {
        // Sort edges across edge groups
        SortEdges();

        // Assign each edge to a particular source node
        AssignEdgesToNodes();
      }

      // Layout edges as a function of the calculated unit width
      LayoutEdges();

      // Transfer calculated coordinates from the internal to the layout edges
      ApplyPositions();

      // Style the edge labels (as a function of calculated edge positions)
      LayoutEdgeLabels(graph);
    }

    /// <summary>
    ///   Create the edge group mapper and edge group object, which stores each group's sorted edges and collapsed state.
    /// </summary>
    private void CreateEdgeGroups(LayoutGraph graph) {
      // Create Edge Group Mapper (LayoutEdge -> Group ID and its collapsed state)
      edgeGroupMapper = graph.Context.GetItemData(EdgeGroupDataKey);

      // Create Initial Edge Group Object (Group ID -> Edge Group type)
      foreach (var edge in graph.Edges) {
        // If an edge maps to no group, create a dummy 'Ungrouped' group
        var edgeGroup = edgeGroupMapper is not null ? edgeGroupMapper[edge] : ("Ungrouped", false);
        // Create a new keyed entry for the extracted group if it does not yet exist
        if (!EdgeGroups.TryGetValue(edgeGroup.Item1, out var group)) {
          group = new LayoutEdgeGroup { Collapsed = edgeGroup.Item2 };
          EdgeGroups.Add(edgeGroup.Item1, group);
        }
        // Push the current edge into the current group edge
        group.Edges.Add(edge);
      }

      // (Re-)create Edge Groups Object, this time sorted
      EdgeGroups = SortGroups(EdgeGroups, (a, b) => EdgeGroupComparator(a, b));
    }

    /// <summary>
    ///   Create the node group mapper and node group object, which stores each group's sorted nodes and collapsed state.
    /// </summary>
    private void CreateNodeGroups(LayoutGraph graph) {
      // Create Node Group Mapper (LayoutNode -> Group ID and its collapsed state)
      nodeGroupMapper = graph.Context.GetItemData(NodeGroupDataKey);

      // Create Initial Node Group Object (Group ID -> Node Group type)
      foreach (var node in graph.Nodes) {
        // If a node maps to no group, create a dummy 'Ungrouped' group
        var nodeGroup = nodeGroupMapper is not null ? nodeGroupMapper[node] : ("Ungrouped", false);
        // Create a new keyed entry for the extracted group if it does not yet exist
        if (!NodeGroups.TryGetValue(nodeGroup.Item1, out var group)) {
          group = new LayoutNodeGroup { Collapsed = nodeGroup.Item2 };
          NodeGroups.Add(nodeGroup.Item1, group);
        }
        // Push the current node into the current group node
        group.Nodes.Add(node);
      }

      // Recreate Node Groups Object, this time sorted
      NodeGroups = SortGroups(NodeGroups, (a, b) => NodeGroupComparator(a, b));
    }

    private static Dictionary<string, TGroup> SortGroups<TGroup>(
        Dictionary<string, TGroup> groups,
        Comparison<string> comparison) {
      var entries = groups.ToList();
      entries.Sort((a, b) => comparison(a.Key, b.Key));
      return entries.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    /// <summary>
    ///   Given a particular node and edge ordering, assign each edge to a particular node, i.e., the
    ///   first and top-most node that an edge connects to.
    /// </summary>
    private void AssignEdgesToNodes() {
      // Create an empty nodesInEdgeGroup Object
      foreach (var edgeGroupId in EdgeGroups.Keys) {
        nodesInEdgeGroup[edgeGroupId] = [];
      }

      // Iterate over globally sorted nodes
      foreach (var node in sortedNodes) {
        // Initialize an empty set describing all the edge groups that a node maps to
        var edgeGroupsPerNode = new HashSet<string>();

        // Register the edge groups of each incident edge to the node
        if (sortedEdgesPerNode.TryGetValue(node, out var sortedEdges)) {
          foreach (var edge in sortedEdges) {
            edgeGroupsPerNode.Add(edgeGroupMapper is not null ? edgeGroupMapper[edge].Item1 : "Ungrouped");
          }
        }

        // Add the node's edge set to nodesInEdgeGroup
        foreach (var group in edgeGroupsPerNode) {
          if (nodesInEdgeGroup.TryGetValue(group, out var nodes)) {
            nodes.Add(node);
          }
        }
      }
    }

    /// <summary>
    ///   Determine the x and y positions of all nodes as a function of their global ordering, the
    ///   calculated unit height, the collapsed states of nodes and groups, and the previous node.
    /// </summary>
    /// <param name="origin">The origin where the first node will be drawn.</param>
    private void LayoutNodes(PointD? origin = null) {
      var actualOrigin = origin ?? PointD.Origin;
      // Initialize the previous node
      string? previousGroup = null;
      var previousY = actualOrigin.Y;

      // Iterate over each node in the globally sorted nodes array
      for (var nodeIndex = 0; nodeIndex < sortedNodes.Count; nodeIndex++) {
        var node = sortedNodes[nodeIndex];
        // Collect the current node group and its collapsed state
        var nodeGroup = nodeGroupMapper is not null ? nodeGroupMapper[node] : ("", false);
        var currentGroup = nodeGroupMapper is null ? null : nodeGroup.Item1;
        var collapsed = nodeGroupMapper is not null && nodeGroup.Item2;

        /*
        Calculate the current node's y coordinate dependent on the previous node's y coordinate and
        dependent on whether the current group is collapsed and the previous group and current
        one differ
        */
        var yCoordinate = previousY;
        if (nodeIndex != 0) {
          if (!collapsed) {
            yCoordinate += currentGroup != previousGroup
                ? nodeSpacings[NodeSpacingTypes.NodeGroup] * UnitHeight
                : UnitHeight;
          } else if (currentGroup != previousGroup) {
            yCoordinate += nodeSpacings[NodeSpacingTypes.NodeGroup] * UnitHeight;
          }
        }

        // Create a new internal node object
        var newNode = new NodePlacement {
            X = actualOrigin.X,
            Y = yCoordinate,
            Width = LayoutWidth,
            Height = UnitHeight * nodeSpacings[NodeSpacingTypes.Node],
            LayoutNode = node
        };

        // Store current x and y positions in the current 'layoutNode' as well
        node.Layout.X = actualOrigin.X;
        node.Layout.Y = yCoordinate;

        // Update the 'node' -> 'layoutNode' map
        nodeNodeMap[node] = newNode;

        // Update the previous group and y coordinate for the next iteration
        previousGroup = currentGroup;
        previousY = newNode.Y;
      }
    }

    /// <summary>
    ///   Determine the x and y coordinates of the graph's edges as a function of the number of
    ///   (un)collapsed edge groups, total number of edges, and the calculated unit width.
    /// </summary>
    /// <param name="edgeOrigin">The position at which the first edge will be drawn.</param>
    /// <returns>The x coordinate of the last edge.</returns>
    private double LayoutEdges(double? edgeOrigin = null) {
      // Set the first node's x position
      var xEndPoint = edgeOrigin ?? paddings[PaddingTypes.XPadding];

      // Precalculate the (user-defined) node and edge-group padding
      var edgeGroupPadding = UnitWidth * edgeSpacings[EdgeSpacingTypes.EdgeGroup];

      // Iterate over all groups
      var edgeGroupIndex = 0;

      // Ensure there is at least one edge group
      if (EdgeGroups.Count > 0) {
        // Iterate over all edge groups
        foreach (var (edgeGroupId, layoutEdgeGroup) in EdgeGroups) {
          if (FormStaircases) {
            // Get all nodes that fall within the current edge group
            var nodes = nodesInEdgeGroup[edgeGroupId];

            // Get all edges in the current edge group
            var layoutGroupEdges = layoutEdgeGroup.Edges;

            // Iterate over all nodes and render its incident edges in order
            for (var nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++) {
              var layoutNode = nodes[nodeIndex];
              var edges = sortedEdgesPerNode.TryGetValue(layoutNode, out var nodeEdges)
                  ? nodeEdges.Where(layoutGroupEdges.Contains).ToList()
                  : new List<LayoutEdge>();
              xEndPoint = LayoutEdgeSet(edges, xEndPoint);

              // Add padding between edges that map to different nodes
              if (nodeIndex != nodes.Count - 1) {
                xEndPoint += layoutEdgeGroup.Collapsed ? 0 : UnitWidth;
              }
            }
          } else {
            // Render all edges in the group in the requested order
            var layoutGroupEdges = layoutEdgeGroup.Edges.ToList();
            layoutGroupEdges.Sort((a, b) => EdgeComparator(a, b));
            xEndPoint = LayoutEdgeSet(layoutGroupEdges, xEndPoint);
          }

          // Add padding between different edge groups
          if (edgeGroupIndex != EdgeGroups.Count - 1) {
            xEndPoint += edgeGroupPadding;
          }

          // Increment the group index
          edgeGroupIndex++;
        }
      } else {
        // Get all nodes that have at least one edge mapped to them
        var nonEmptyChildren = sortedNodes.Where(node =>
            sortedEdgesPerNode.TryGetValue(node, out var edges) && edges.Count > 0).ToList();

        // Iterate over the non-empty nodes
        for (var nodeIndex = 0; nodeIndex < nonEmptyChildren.Count; nodeIndex++) {
          var node = nonEmptyChildren[nodeIndex];
          xEndPoint = LayoutEdgeSet(sortedEdgesPerNode[node], xEndPoint);

          // Add padding between nodes
          if (nodeIndex != nonEmptyChildren.Count - 1) {
            xEndPoint += UnitWidth;
          }
        }
      }

      // Return the final x coordinate of the final edge
      return xEndPoint;
    }

    /// <summary>
    ///   Determines the x and y coordinates of edges that fall within a particular node's set.
    /// </summary>
    /// <param name="edgesToDraw">The edges that map to a particular node set.</param>
    /// <param name="origin">The x coordinate of the first edge to be rendered.</param>
    /// <returns>The final edge's x coordinate.</returns>
    private double LayoutEdgeSet(IReadOnlyList<LayoutEdge> edgesToDraw, double origin) {
      // Set the next edge's x coordinate to the given origin
      var endPoint = origin;

      // Iterate over all given edges
      for (var edgeIndex = 0; edgeIndex < edgesToDraw.Count; edgeIndex++) {
        var edge = edgesToDraw[edgeIndex];
        // Extract source and target nodes
        var sourceNode = nodeNodeMap[edge.Source];
        var targetNode = nodeNodeMap[edge.Target];

        // Create new internal edge object
        var newEdge = new EdgePlacement(endPoint, sourceNode, targetNode, edge);

        // Update the edgesPerNode with the source
        if (!edgesPerNode.TryGetValue(sourceNode, out var sourceEdges)) {
          sourceEdges = [];
          edgesPerNode[sourceNode] = sourceEdges;
        }
        sourceEdges.Add(newEdge);

        // Update the edgesPerNode with the target
        if (!edgesPerNode.TryGetValue(targetNode, out var targetEdges)) {
          targetEdges = [];
          edgesPerNode[targetNode] = targetEdges;
        }
        targetEdges.Add(newEdge);

        // Update the origin/end-point
        if (edgeIndex != edgesToDraw.Count - 1) {
          var collapsed = edgeGroupMapper is not null && edgeGroupMapper[edge].Item2;
          endPoint += collapsed ? 0 : UnitWidth;
        }
      }

      // Return the final edge's x coordinate
      return endPoint;
    }

    /// <summary>
    ///   Determine the nodes' overall order across node groups.
    /// </summary>
    private void SortNodes() {
      // (Re-)initialize the sorted nodes array
      sortedNodes.Clear();

      // Iterate over each node group to append its sorted node subset
      foreach (var layoutNodeGroup in NodeGroups.Values) {
        var groupNodes = layoutNodeGroup.Nodes.ToList();
        groupNodes.Sort((a, b) => NodeComparator(a, b));
        sortedNodes.AddRange(groupNodes);
      }
    }

    /// <summary>
    ///   Determine the edges' overall order across edge groups and node-sets.
    /// </summary>
    private void SortEdges() {
      // Keep track of those edges that have already been assigned to a node
      var assignedEdges = new HashSet<LayoutEdge>();

      // Iterate over the globally sorted nodes and initialize an empty edge list
      foreach (var node in sortedNodes) {
        var edgesToDraw = new List<LayoutEdge>();

        // Assign all not-yet-assigned edges to the current node
        foreach (var edge in node.Edges) {
          if (assignedEdges.Add(edge)) {
            edgesToDraw.Add(edge);
          }
        }

        // Update the sorted edges per node
        edgesToDraw.Sort((a, b) => EdgeComparator(a, b));
        sortedEdgesPerNode[node] = edgesToDraw;
      }
    }

    /// <summary>
    ///   Map the x and y coordinates of the 'edge' objects to the actual 'layoutEdge' objects and the
    ///   width of each 'node' object to each actual 'layoutNode' object.
    /// </summary>
    private void ApplyPositions() {
      // Set the width of each 'layoutNode' object
      foreach (var layoutNode in sortedNodes) {
        SetNodeWidth(layoutNode);
      }

      // Set the x and y coordinates of each 'layoutEdge' object
      foreach (var layoutNode in sortedNodes) {
        var node = nodeNodeMap[layoutNode];
        if (!edgesPerNode.TryGetValue(node, out var edges)) {
          continue;
        }
        foreach (var edge in edges) {
          edge.LayoutEdge.SourcePortLocation = new PointD(edge.X, edge.LayoutEdge.Source.Layout.CenterY);
          edge.LayoutEdge.TargetPortLocation = new PointD(edge.X, edge.LayoutEdge.Target.Layout.CenterY);
        }
      }
    }

    /// <summary>
    ///   Set each node's width dependent on the user-specified node width mode.
    /// </summary>
    /// <param name="layoutNode">The LayoutNode object whose width is to be set.</param>
    private void SetNodeWidth(LayoutNode layoutNode) {
      var node = nodeNodeMap[layoutNode];
      var edges = edgesPerNode.TryGetValue(node, out var nodeEdges) ? nodeEdges : [];
      double min;
      double max;

      if (edges.Count == 0) {
        min = node.X;
        max = NodeWidthMode == NodeWidthMode.Full ? LayoutWidth : node.X;
      } else if (NodeWidthMode == NodeWidthMode.Compact) {
        // Compact: each node starts at its first edge and ends at its last edge
        min = edges.Min(edge => edge.X) - paddings[PaddingTypes.XPadding];
        max = edges.Max(edge => edge.X) + paddings[PaddingTypes.XPadding];
      } else if (NodeWidthMode == NodeWidthMode.Stairs) {
        // Stairs: each node starts at the same x origin and ends at its last edge
        min = node.X;
        max = edges.Max(edge => edge.X) + paddings[PaddingTypes.XPadding];
      } else {
        // Full: each node starts and ends at the same x position, taking up the drawing's full width
        min = node.X;
        max = LayoutWidth;
      }
      node.Width = max - min;
      node.X = min;
      layoutNode.Layout.X = min;
      layoutNode.Layout.Width = max - min;
    }

    /// <summary>
    ///   Determine each edge label's x and y positions as a function of the already determined
    ///   nodes' and edges' coordinates. Label positioning differs based on node width mode.
    /// </summary>
    /// <param name="graph">The layout graph containing the edges and their labels.</param>
    private void LayoutEdgeLabels(LayoutGraph graph) {
      if (NodeWidthMode == NodeWidthMode.Full) {
        var topY = graph.GetBounds(includeNodeLabels: false, includeEdgeLabels: false).Y;
        foreach (var edgeLabel in graph.EdgeLabels) {
          if (edgeLabel.Owner is not { } edge) {
            continue;
          }
          edgeLabel.Layout = new OrientedRectangle(edge.SourcePortLocation.X + EdgeLabelHeight * 0.25,
              topY - EdgeLabelHeight, edgeLabel.Layout.Width, EdgeLabelHeight, -1, 0);
        }
      } else {
        foreach (var edgeLabel in graph.EdgeLabels) {
          if (edgeLabel.Owner is not { } edge) {
            continue;
          }
          var topY = Math.Min(edge.SourcePortLocation.Y, edge.TargetPortLocation.Y);
          edgeLabel.Layout = new OrientedRectangle(edge.SourcePortLocation.X + EdgeLabelHeight * 0.25,
              topY - EdgeLabelHeight, edgeLabel.Layout.Width, EdgeLabelHeight, -1, 0);
        }
      }
    }

    private enum EdgeSpacingTypes
    {
      Edge,
      Node,
      EdgeGroup
    }

    private enum NodeSpacingTypes
    {
      Node,
      NodeGroup
    }

    private enum PaddingTypes
    {
      XPadding
    }

    /// <summary>Stores a node's calculated position and dimensions during layout.</summary>
    private sealed class NodePlacement
    {
      /// <summary>Gets or sets the node's x-coordinate.</summary>
      public double X { get; set; }

      /// <summary>Gets or sets the node's y-coordinate.</summary>
      public double Y { get; set; }

      /// <summary>Gets or sets the node's calculated width.</summary>
      public double Width { get; set; }

      /// <summary>Gets or sets the node's calculated height.</summary>
      public double Height { get; set; }

      /// <summary>Gets the layout node represented by this placement.</summary>
      public LayoutNode LayoutNode { get; init; } = null!;
    }

    /// <summary>Stores an edge's calculated x-coordinate and its endpoint placements.</summary>
    private sealed record EdgePlacement(
        double X,
        NodePlacement SourceNode,
        NodePlacement TargetNode,
        LayoutEdge LayoutEdge);
  }
}
