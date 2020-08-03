/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Aggregation;
using yWorks.Analysis;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Demo.yFiles.Complete.LargeGraphAggregation
{
  /// <summary>
  /// A helper class that provides methods to aggregate and separate nodes according to a <see cref="NodeAggregation.Result"/>.
  /// </summary>
  /// <remarks>
  /// Delegates most of it's work to an <see cref="AggregateGraphWrapper"/>. Implements some functionality on top:
  /// <list type="bullet">
  /// <item>When separating a node, this class creates a new aggregation node as replacement. This node represents the
  /// hierarchy also in separated state and allows the user to aggregate its children again.</item>
  /// <item>Creates additional "hierarchy" edges between such nodes and its children.</item>
  /// <item>Since <see cref="NodeAggregation.Aggregate"/>s are allowed to both have children as well as represent an
  /// original node, the aggregation nodes for such aggregates are special placeholder nodes that adopt its visual
  /// appearance, its labels, as well as its edges to other nodes.</item>
  /// </list>
  /// </remarks>
  public class AggregationHelper
  {
    /// <summary>
    /// The <see cref="AggregateGraphWrapper"/> this instance uses.
    /// </summary>
    public AggregateGraphWrapper AggregateGraph { get; }

    /// <summary>
    /// The edge style to be used for the artificial hierarchy edges.
    /// </summary>
    public IEdgeStyle HierarchyEdgeStyle { get; set; }

    /// <summary>
    /// The style for labels that are created for aggregation nodes that don't directly represent an original node. Such
    /// labels show the text of the most important descendant node.
    /// </summary>
    public ILabelStyle DescendantLabelStyle { get; set; }

    /// <summary>
    /// The style used for aggregation nodes (no matter which state). Should adapt to the <see cref="AggregationNodeInfo"/>
    /// in the node's tag.
    /// </summary>
    public INodeStyle AggregationNodeStyle { get; set; }

    /// <summary>
    /// The aggregation result.
    /// </summary>
    private readonly NodeAggregation.Result aggregationResult;

    /// <summary>
    /// Maps <see cref="NodeAggregation.Aggregate"/>s to aggregation nodes. 
    /// </summary>
    private readonly IDictionary<NodeAggregation.Aggregate, INode> aggregateToNode =
        new Dictionary<NodeAggregation.Aggregate, INode>();

    /// <summary>
    /// A map for placeholder nodes that maps original nodes to their aggregation placeholder node.
    /// </summary>
    private readonly IDictionary<INode, INode> placeholderMap = new Dictionary<INode, INode>();

    /// <summary>
    /// Returns the number of original nodes (or their placeholders) that are currently visible.
    /// </summary>
    public int VisibleNodes {
      get {
        return AggregateGraph?.Nodes.Count(IsOriginalNodeOrPlaceHolder) ?? 0;
      }
    }

    public bool IsOriginalNodeOrPlaceHolder(INode node) {
      return !AggregateGraph.IsAggregationItem(node) || ((AggregationNodeInfo) node.Tag).Aggregate.Node != null;
    }
    
    public bool IsHierarchyEdge(IEdge edge) {
      return edge.Tag != null;
    }

    /// <summary>
    /// Returns the number of original edges (or their placeholders) that are currently visible.
    /// </summary>
    public int VisibleEdges {
      get {
        return AggregateGraph?.Edges.Count(e => e.Tag == null) ?? 0;
      }
    }

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    public AggregationHelper(NodeAggregation.Result aggregationResult, AggregateGraphWrapper aggregateGraph) {
      AggregateGraph = aggregateGraph;
      this.aggregationResult = aggregationResult;
    }

    /// <summary>
    /// Returns the <see cref="NodeAggregation.Aggregate"/> for a node.
    /// </summary>
    public NodeAggregation.Aggregate GetAggregateForNode(INode node) {
      if (AggregateGraph.IsAggregationItem(node)) {
        return ((AggregationNodeInfo) node.Tag).Aggregate;
      } else {
        return aggregationResult.AggregateMap[node];
      }
    }

    /// <summary>
    /// Returns the placeholder node for an original node or the original node itself if there is no placeholder.
    /// </summary>
    public INode GetPlaceholder(INode originalNode) {
      return placeholderMap.TryGetValue(originalNode, out var placeHolder) ? placeHolder : originalNode;
    }

    /// <summary>
    /// If a node is aggregated, calls <see cref="Separate"/>, if not calls <see cref="Aggregate"/>.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The nodes affected by this operation. The created aggregation node is always the first item.</returns>
    public IListEnumerable<INode> ToggleAggregation(INode node) {
      var aggregationNodeInfo = (AggregationNodeInfo) node.Tag;
      return aggregationNodeInfo.IsAggregated ? Separate(node) : Aggregate(node);
    }

    /// <summary>
    /// Replaces a separated node and its hierarchy children with a new aggregation node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The nodes affected by this operation. The created aggregation node is always the first item.</returns>
    public IListEnumerable<INode> Aggregate(INode node) {
      var aggregationInfo = (AggregationNodeInfo) node.Tag;
      var aggregate = aggregationInfo.Aggregate;
      var aggregationNode = AggregateRecursively(aggregate);

      var affectedNodes = new List<INode> { aggregationNode };
      if (aggregate.Parent != null && aggregateToNode.TryGetValue(aggregate.Parent, out var parentNode)) {
        AggregateGraph.CreateEdge(parentNode, aggregationNode, HierarchyEdgeStyle, true);
        affectedNodes.Add(parentNode);
      }

      if (aggregate.Node != null) {
        ReplaceEdges(aggregationNode);
      }

      return new ListEnumerable<INode>(affectedNodes);
    }

    /// <summary>
    /// Aggregates the <paramref name="aggregate"/> as well as all its children recursively.
    /// </summary>
    /// <remarks>
    /// Can be used to apply the initial aggregation. If this is not the initial aggregation run, it will reuse existing
    /// aggregation nodes.
    /// </remarks>
    /// <param name="aggregate">The "root" aggregate.</param>
    /// <returns>The aggregation node representing the passed <paramref name="aggregate"/></returns>
    public INode AggregateRecursively(NodeAggregation.Aggregate aggregate) {
      if (aggregate.Children.Count == 0) {
        return aggregate.Node;
      }

      PointD originalCenter;
      if (aggregateToNode.TryGetValue(aggregate, out var node)) {
        originalCenter = node.Layout.GetCenter();
        var aggregationInfo = (AggregationNodeInfo) node.Tag;
        if (aggregationInfo.IsAggregated) {
          return node;
        } else {
          AggregateGraph.Separate(node);
        }
      } else {
        originalCenter = PointD.Origin;
      }

      var nodesToAggregate = aggregate.Children.Select(AggregateRecursively).ToList();
      if (aggregate.Node != null) {
        nodesToAggregate.Add(aggregate.Node);
      }

      var size = 30 + Math.Sqrt(aggregate.DescendantWeightSum) * 4;
      var layout = RectD.FromCenter(originalCenter, new SizeD(size, size));
      var aggregationNode =
          AggregateGraph.Aggregate(new ListEnumerable<INode>(nodesToAggregate), layout, AggregationNodeStyle);

      aggregateToNode[aggregate] = aggregationNode;
      aggregationNode.Tag = new AggregationNodeInfo(aggregate, true);

      if (aggregate.Node != null) {
        placeholderMap[aggregate.Node] = aggregationNode;
        CopyLabels(aggregate.Node, aggregationNode);
      } else {
        var maxChild = GetMostImportantDescendant(aggregate);
        if (maxChild.Node != null && maxChild.Node.Labels.Any()) {
          AggregateGraph.AddLabel(aggregationNode, $"({maxChild.Node.Labels[0].Text}, …)",
              FreeNodeLabelModel.Instance.CreateDefaultParameter(), DescendantLabelStyle);
        }
      }

      return aggregationNode;
    }

    /// <summary>
    /// Gets the descendant <see cref="NodeAggregation.Aggregate"/> with the highest
    /// <see cref="NodeAggregation.Aggregate.DescendantWeightSum"/>.
    /// </summary>
    private static NodeAggregation.Aggregate GetMostImportantDescendant(NodeAggregation.Aggregate aggregate) {
      while (true) {
        var maxChild = aggregate.Children.Aggregate((max, child) =>
            child.DescendantWeightSum > max.DescendantWeightSum ? child : max);
        if (maxChild.Node != null) return maxChild;
        aggregate = maxChild;
      }
    }

    /// <summary>
    /// Copies the labels from <paramref name="source"/> to <paramref name="target"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    private void CopyLabels(INode source, INode target) {
      foreach (var label in source.Labels) {
        AggregateGraph.AddLabel(target, label.Text, FreeNodeLabelModel.Instance.CreateDefaultParameter(), label.Style);
      }
    }

    /// <summary>
    /// Separates an aggregated aggregation node and replaces it by a new aggregation node.
    /// </summary>
    /// <remarks>
    /// Creates hierarchy edges between the new aggregation node and its children.
    /// </remarks>
    /// <param name="node">The node.</param>
    /// <returns>The nodes affected by this operation. The created aggregation node is always the first item.</returns>
    public IListEnumerable<INode> Separate(INode node) {
      var aggregationInfo = (AggregationNodeInfo) node.Tag;
      var aggregate = aggregationInfo.Aggregate;
      var aggregatedItems = AggregateGraph.GetAggregatedItems(node)
                                          .Where(n => n != aggregate.Node)
                                          .Cast<INode>().ToList();
      AggregateGraph.Separate(node);

      var nodesToAggregate = aggregate.Node != null
          ? new ListEnumerable<INode>(new List<INode> { aggregate.Node })
          : ListEnumerable<INode>.Empty;
      var aggregationNode = AggregateGraph.Aggregate(nodesToAggregate, node.Layout.ToRectD(), AggregationNodeStyle);

      foreach (var child in aggregatedItems) {
        AggregateGraph.CreateEdge(aggregationNode, child, HierarchyEdgeStyle, true);
        AggregateGraph.SetNodeLayout(child,
            RectD.FromCenter(aggregationNode.Layout.GetCenter(), child.Layout.ToSizeD()));
        ReplaceEdges(child);
      }

      aggregationInfo.IsAggregated = false;
      aggregateToNode[aggregate] = aggregationNode;
      aggregationNode.Tag = aggregationInfo;

      var affectedNodes = new List<INode> { aggregationNode };
      affectedNodes.AddRange(aggregatedItems);

      if (aggregate.Parent != null && aggregateToNode.TryGetValue(aggregate.Parent, out var parentNode)) {
        AggregateGraph.CreateEdge(parentNode, aggregationNode, HierarchyEdgeStyle, true);
        affectedNodes.Add(parentNode);
      }

      if (aggregate.Node != null) {
        placeholderMap[aggregate.Node] = aggregationNode;
        CopyLabels(aggregate.Node, aggregationNode);
        ReplaceEdges(aggregationNode);
      }

      return new ListEnumerable<INode>(affectedNodes);
    }

    /// <summary>
    /// Replaces original edges adjacent to a placeholder node with aggregation edges when source and target are
    /// currently visible.
    /// </summary>
    private void ReplaceEdges(INode node) {
      INode originalNode;
      if (node.Tag is AggregationNodeInfo aggregationInfo) {
        originalNode = aggregationInfo.Aggregate.Node;
      } else {
        originalNode = node;
      }

      if (originalNode == null) {
        return;
      }

      foreach (var edge in AggregateGraph.WrappedGraph.EdgesAt(originalNode).ToList()) {
        if (edge.TargetPort.Owner == originalNode) {
          var sourceNode = (INode) edge.SourcePort.Owner;
          if (AggregateGraph.Contains(sourceNode)) {
            CreateReplacementEdge(sourceNode, node, edge);
          } else if (placeholderMap.TryGetValue(sourceNode, out var placeholderSource) &&
                     AggregateGraph.Contains(placeholderSource)) {
            CreateReplacementEdge(placeholderSource, node, edge);
          }
        } else {
          var targetNode = (INode) edge.TargetPort.Owner;
          if (AggregateGraph.Contains(targetNode)) {
            CreateReplacementEdge(node, targetNode, edge);
          } else if (placeholderMap.TryGetValue(targetNode, out var placeholderTarget) &&
                     AggregateGraph.Contains(placeholderTarget)) {
            CreateReplacementEdge(node, placeholderTarget, edge);
          }
        }
      }
    }

    private void CreateReplacementEdge(INode sourceNode, INode targetNode, IEdge edge) {
      if ((AggregateGraph.IsAggregationItem(sourceNode) || AggregateGraph.IsAggregationItem(targetNode)) &&
          AggregateGraph.GetEdge(sourceNode, targetNode) == null &&
          AggregateGraph.GetEdge(targetNode, sourceNode) == null) {
        AggregateGraph.CreateEdge(sourceNode, targetNode, edge.Style);
      }
    }
  }

  /// <summary>
  /// The class for the tag of aggregation nodes.
  /// </summary>
  public sealed class AggregationNodeInfo
  {
    public NodeAggregation.Aggregate Aggregate { get; }

    public bool IsAggregated { get; set; }

    public AggregationNodeInfo(NodeAggregation.Aggregate aggregate, bool isAggregated) {
      this.Aggregate = aggregate;
      this.IsAggregated = isAggregated;
    }
  }
}
