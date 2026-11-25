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
using yWorks.Analysis;
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ArcDiagram
{
  /// <summary>
  ///   Arranges graphs in a manner that is suitable for arc diagrams.
  ///   I.e., nodes are placed from left to right on a horizontal line and edges are routed as "arcs"
  ///   from the source's node center to the target's node center.
  ///   The actual left-to-right order of nodes may be from sketch, in topological sort order, or such
  ///   that the number of edge crossings is minimized.
  ///   Edges may be routed such that their bends are control points for cubic bezier curves that
  ///   approximate semicircles. Alternatively, edges may be routed with a single bend such that
  ///   source node center, target node center, and bend location define a unique semi circle.
  ///   <seealso cref="NodeOrder"/>
  ///   <seealso cref="CreateBezierControlPoints"/>
  /// </summary>
  public sealed class ArcDiagramLayout : ILayoutAlgorithm
  {
    /// <summary>
    ///   The minimum distance between two subsequent nodes.
    /// </summary>
    public double MinimumNodeDistance { get; set; } = 30;

    /// <summary>
    ///   Specifies if edge path bends should be calculated as cubic bezier control points.
    ///   If this property is set to <see langword="true"/>, edges are routed in such a way that their
    ///   bends are control points for cubic bezier curves that approximate semi-circles.
    ///   If this property is set to <see langword="false"/>, edges are routed with a single bend such that
    ///   source node center, target node center, and bend location define a unique semi circle.
    /// </summary>
    public bool CreateBezierControlPoints { get; set; } = true;

    /// <summary>
    ///   Specifies the left-to-right order of nodes.
    /// </summary>
    public NodeOrder NodeOrder { get; set; } = NodeOrder.FromSketch;

    /// <summary>
    ///   Arranges the given graph.
    /// </summary>
    /// <param name="graph">The graph to be arranged.</param>
    public void ApplyLayout(LayoutGraph graph) {
      if (graph.Nodes.Any()) {
        PlaceNodes(graph);
      }

      if (graph.Edges.Any()) {
        RouteEdges(graph);
      }
    }

    /// <summary>
    ///   Places the nodes of the given graph on a horizontal line from left to right.
    ///   The order in which the nodes are placed is determined by property <see cref="NodeOrder"/>.
    /// </summary>
    /// <param name="graph">The graph to be arranged.</param>
    private void PlaceNodes(LayoutGraph graph) {
      var nodes = graph.Nodes.ToArray();
      var maxWidth = nodes.Max(node => node.Layout.Width);
      var distance = maxWidth + MinimumNodeDistance;
      var order = CalculateNodeOrder(graph, NodeOrder);
      foreach (var node in nodes) {
        var position = order[node.Index];
        node.Layout.Center = new PointD(position * distance, 0);
      }
    }

    /// <summary>
    ///   Routes the edges in the given graph.
    /// </summary>
    /// <param name="graph">The graph to be arranged.</param>
    private void RouteEdges(LayoutGraph graph) {
      var bezier = CreateBezierControlPoints;
      var bezierFactor = 4 * (Math.Sqrt(2) - 1) / 3;

      foreach (var edge in graph.Edges) {
        edge.ResetPath();
        if (edge.SelfLoop) {
          continue;
        }

        var source = edge.Source;
        var target = edge.Target;

        var centerXSource = source.Layout.CenterX;
        var centerXTarget = target.Layout.CenterX;
        var circleCenterX = (centerXSource + centerXTarget) * 0.5;
        var radius = circleCenterX - Math.Min(centerXSource, centerXTarget);

        if (bezier) {
          var sign = centerXSource > centerXTarget ? -1 : 1;

          var d = radius * bezierFactor;
          graph.AddBend(edge, centerXSource, -d);
          graph.AddBend(edge, circleCenterX - sign * d, -radius);
          graph.AddBend(edge, circleCenterX, -radius);
          graph.AddBend(edge, circleCenterX + sign * d, -radius);
          graph.AddBend(edge, centerXTarget, -d);
        } else {
          graph.AddBend(edge, circleCenterX, -radius);
        }
      }
    }

    /// <summary>
    ///   Calculates the left-to-right order of nodes for the given graph according to the given node order
    ///   policy.
    /// </summary>
    /// <param name="graph">The graph to be arranged.</param>
    /// <param name="nodeOrder">The node order policy that determines the calculated order.</param>
    private static int[] CalculateNodeOrder(LayoutGraph graph, NodeOrder nodeOrder) {
      var nodes = graph.Nodes.ToArray();
      switch (nodeOrder) {
        case NodeOrder.FromSketch: {
          var order = nodes.OrderBy(node => node.Layout.Center.X).ToArray();
          return [.. nodes.Select(orderNode => Array.IndexOf(order, orderNode))];
        }
        case NodeOrder.MinimizeCrossings:
          return MinimizeCrossings(graph, nodes);
        case NodeOrder.Topological: {
          if (LayoutGraphAlgorithms.IsAcyclic(graph)) {
            var topologicalOrder = LayoutGraphAlgorithms.TopologicalNodeOrder(graph).ToArray();
            return [.. nodes.Select(node => Array.IndexOf(topologicalOrder, node))];
          }
          return [.. nodes.Select((_, index) => index)];
        }
        default:
          // The order in which they were created in the graph.
          return [.. nodes.Select((_, index) => index)];
      }
    }

    /// <summary>
    ///   Calculates a node order with few crossings for the given graph.
    ///   This implementation uses <see cref="HierarchicalLayout"/>'s sequencing phase towards this end.
    ///   Note, crossing minimization is an NP-hard problem. For this reason, <see cref="HierarchicalLayout"/>
    ///   uses an approximation when calculating a sequence.
    /// </summary>
    /// <param name="graph">The graph for which a node order is calculated.</param>
    /// <param name="nodes">The graph's nodes in their original order.</param>
    /// <returns>An array containing the new node order for the given graph.</returns>
    private static int[] MinimizeCrossings(LayoutGraph graph, IReadOnlyList<Node> nodes) {
      // Run hierarchical layout's sequencing phase to calculate a node order with few crossings.
      var hierarchicalLayout = new HierarchicalLayout { FromScratchLayeringStrategy = LayeringStrategy.UserDefined };
      hierarchicalLayout.Core.StopAfterSequencing = false;

      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(graph);
      hierarchicalLayoutData.GivenLayersIndices.MapperFunction = _ => 0;
      graph.ApplyLayout(hierarchicalLayout, hierarchicalLayoutData);

      // Write the node sequence to the order array.
      return [
          .. nodes.Select(node =>
              hierarchicalLayoutData.SequenceIndicesResult[node] ??
              throw new InvalidOperationException("The hierarchical layout did not return a node sequence."))
      ];
    }
  }

  /// <summary>
  ///   Specifies policies for calculating the node order in an arc diagram.
  ///   With <see cref="FromSketch"/>, nodes will be placed from left to right in the order of their x-coordinates.
  ///   With <see cref="MinimizeCrossings"/>, nodes will be placed from left to right such that the number of
  ///   crossings between edges is reduced as much as possible.
  ///   With <see cref="Topological"/>, if the graph is acyclic, nodes will be placed from left to right in the order
  ///   calculated by a topological sorting. Otherwise, nodes will be placed from left to right in the order they
  ///   were created in the graph.
  /// </summary>
  public enum NodeOrder
  {
    FromSketch,
    MinimizeCrossings,
    Topological
  }
}
