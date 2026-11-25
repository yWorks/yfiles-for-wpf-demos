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

using System.Collections.Generic;
using yWorks.Graph;
using yWorks.Layout.Graph;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   Determines the order of nodes in the biofabric's vertical node sequence.
  /// </summary>
  public enum NodeOrderingKey
  {
    /// <summary>Places nodes with the highest degree first.</summary>
    Degree,

    /// <summary>Orders nodes by their configured ID in descending lexicographical order.</summary>
    LexicographicalDescending,

    /// <summary>Orders nodes by their configured ID in ascending lexicographical order.</summary>
    LexicographicalAscending
  }

  /// <summary>
  ///   Determines the order of edges within the biofabric's horizontal edge sequence.
  /// </summary>
  public enum EdgeOrderingKey
  {
    /// <summary>Orders edges by the distance between their endpoint nodes, shortest first.</summary>
    EdgeLength,

    /// <summary>Orders edges by their configured ID in descending lexicographical order.</summary>
    LexicographicalDescending,

    /// <summary>Orders edges by their configured ID in ascending lexicographical order.</summary>
    LexicographicalAscending,

    /// <summary>Preserves the order in which edges occur in the graph.</summary>
    AsIs
  }

  /// <summary>Determines the order of node groups in the biofabric.</summary>
  public enum NodeGroupOrderingKey
  {
    /// <summary>Orders groups by node count, largest first.</summary>
    CardinalityDescending,

    /// <summary>Orders groups by node count, smallest first.</summary>
    CardinalityAscending,

    /// <summary>Orders groups by their ID in descending lexicographical order.</summary>
    LexicographicalDescending,

    /// <summary>Orders groups by their ID in ascending lexicographical order.</summary>
    LexicographicalAscending
  }

  /// <summary>Determines the order of edge groups in the biofabric.</summary>
  public enum EdgeGroupOrderingKey
  {
    /// <summary>Orders groups by edge count, largest first.</summary>
    CardinalityDescending,

    /// <summary>Orders groups by edge count, smallest first.</summary>
    CardinalityAscending,

    /// <summary>Orders groups by their ID in descending lexicographical order.</summary>
    LexicographicalDescending,

    /// <summary>Orders groups by their ID in ascending lexicographical order.</summary>
    LexicographicalAscending
  }

  /// <summary>Determines the horizontal extent used to draw each node row.</summary>
  public enum NodeWidthMode
  {
    /// <summary>Draws every node across the full width of the biofabric.</summary>
    Full,

    /// <summary>Starts every node at the same x-coordinate and ends it at its last incident edge.</summary>
    Stairs,

    /// <summary>Draws each node only between its first and last incident edges.</summary>
    Compact
  }

  /// <summary>Determines how the biofabric assigns colors to edges.</summary>
  public enum EdgeColorMode
  {
    /// <summary>Colors node groups and colors each edge with a gradient between its endpoint groups.</summary>
    NodeGroups,

    /// <summary>Colors node groups and reverses the endpoint colors used for each edge gradient.</summary>
    NodeGroupsInverted,

    /// <summary>Colors edges categorically according to their edge group.</summary>
    EdgeGroups,

    /// <summary>Does not assign colors based on node or edge groups.</summary>
    None
  }

  /// <summary>
  ///   Compares two nodes or layout nodes to determine their relative vertical order in a biofabric.
  /// </summary>
  /// <typeparam name="T">The node type being compared.</typeparam>
  /// <param name="first">The first node to compare.</param>
  /// <param name="second">The second node to compare.</param>
  /// <returns>
  ///   A negative value when <paramref name="first"/> precedes <paramref name="second"/>,
  ///   zero when they are equivalent, or a positive value when it follows.
  /// </returns>
  public delegate int NodeOrderingFunction<in T>(T first, T second);

  /// <summary>Compares two edges or layout edges to determine their relative horizontal order.</summary>
  /// <typeparam name="T">The edge type being compared.</typeparam>
  /// <param name="first">The first edge to compare.</param>
  /// <param name="second">The second edge to compare.</param>
  /// <returns>
  ///   A negative value when <paramref name="first"/> precedes <paramref name="second"/>,
  ///   zero when they are equivalent, or a positive value when it follows.
  /// </returns>
  public delegate int EdgeOrderingFunction<in T>(T first, T second);

  /// <summary>Compares node-group IDs to determine their relative order.</summary>
  /// <param name="first">The first node-group ID to compare.</param>
  /// <param name="second">The second node-group ID to compare.</param>
  /// <returns>
  ///   A negative value when <paramref name="first"/> precedes <paramref name="second"/>,
  ///   zero when they are equivalent, or a positive value when it follows.
  /// </returns>
  public delegate int NodeGroupOrderingFunction(string first, string second);

  /// <summary>Compares edge-group IDs to determine their relative order.</summary>
  /// <param name="first">The first edge-group ID to compare.</param>
  /// <param name="second">The second edge-group ID to compare.</param>
  /// <returns>
  ///   A negative value when <paramref name="first"/> precedes <paramref name="second"/>,
  ///   zero when they are equivalent, or a positive value when it follows.
  /// </returns>
  public delegate int EdgeGroupOrderingFunction(string first, string second);

  /// <summary>Describes the nodes in a group and the group's interactive state.</summary>
  public sealed class NodeGroup
  {
    /// <summary>Gets the graph nodes that belong to this group.</summary>
    public List<INode> Nodes { get; } = [];

    /// <summary>Gets the ID that identifies and labels this group.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets or sets whether this group is collapsed.</summary>
    public bool Collapsed { get; set; }

    /// <summary>Gets or sets whether this group is highlighted.</summary>
    public bool Highlighted { get; set; }

    /// <summary>Gets or sets whether another group is highlighted.</summary>
    public bool OtherHighlighted { get; set; }

    public void ResetHighlight() {
      Highlighted = false;
      OtherHighlighted = false;
    }
  }

  /// <summary>Describes the edges in a group and the group's interactive state.</summary>
  public sealed class EdgeGroup
  {
    /// <summary>Gets the graph edges that belong to this group.</summary>
    public List<IEdge> Edges { get; } = [];

    /// <summary>Gets the ID that identifies and labels this group.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets or sets whether this group is collapsed.</summary>
    public bool Collapsed { get; set; }

    /// <summary>Gets or sets whether this group is highlighted.</summary>
    public bool Highlighted { get; set; }

    /// <summary>Gets or sets whether another group is highlighted.</summary>
    public bool OtherHighlighted { get; set; }

    public void ResetHighlight() {
      Highlighted = false;
      OtherHighlighted = false;
    }
  }

  /// <summary>Stores the layout nodes and collapsed state for one node group during layout.</summary>
  internal sealed class LayoutNodeGroup
  {
    /// <summary>Gets the layout nodes that belong to this group.</summary>
    public List<Node> Nodes { get; } = [];

    /// <summary>Gets whether this group is collapsed.</summary>
    public bool Collapsed { get; init; }
  }

  /// <summary>Stores the layout edges and collapsed state for one edge group during layout.</summary>
  internal sealed class LayoutEdgeGroup
  {
    /// <summary>Gets the layout edges that belong to this group.</summary>
    public List<Edge> Edges { get; } = [];

    /// <summary>Gets whether this group is collapsed.</summary>
    public bool Collapsed { get; init; }
  }

  /// <summary>Provides the node-group name and data used to render a node group.</summary>
  /// <param name="GroupName">The unique ID displayed for the group.</param>
  /// <param name="NodeGroup">The group data, including its nodes and interactive state.</param>
  public sealed record NodeGroupRendererRenderTag(string GroupName, NodeGroup NodeGroup);

  /// <summary>Provides the edge-group name and data used to render an edge group.</summary>
  /// <param name="GroupName">The unique ID displayed for the group.</param>
  /// <param name="EdgeGroup">The group data, including its edges and interactive state.</param>
  public sealed record EdgeGroupRendererRenderTag(string GroupName, EdgeGroup EdgeGroup);

  internal static class BiofabricTypes
  {
    /// <summary>Checks whether an object is a node-group renderer tag.</summary>
    /// <param name="item">The object to test.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> is a node-group renderer tag.</returns>
    public static bool IsNodeGroupRenderTag(object? item) => item is NodeGroupRendererRenderTag;

    /// <summary>Checks whether an object is an edge-group renderer tag.</summary>
    /// <param name="item">The object to test.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> is an edge-group renderer tag.</returns>
    public static bool IsEdgeGroupRenderTag(object? item) => item is EdgeGroupRendererRenderTag;
  }
}
