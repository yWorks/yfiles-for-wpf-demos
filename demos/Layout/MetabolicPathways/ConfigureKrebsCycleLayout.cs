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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;
using LayoutEdge = yWorks.Layout.Graph.Edge;
using LayoutEdgeLabel = yWorks.Layout.Graph.EdgeLabel;
using LayoutGraph = yWorks.Layout.Graph.LayoutGraph;
using LayoutNode = yWorks.Layout.Graph.Node;
using LayoutNodeLabel = yWorks.Layout.Graph.NodeLabel;

namespace yWorks.Demo.MetabolicPathways
{
  /// <summary>
  ///   Creates the custom Krebs cycle layout and configures selective placement for co-reactant and other node labels.
  /// </summary>
  /// <remarks>
  ///   The custom algorithm runs two <see cref="OrganicLayout"/> passes on one layout graph. A
  ///   <see cref="GenericLabeling"/> stage wraps those passes to place the selected node labels afterward.
  ///   It reads each layout node's metabolic data through <see cref="LayoutGraphAdapter.OriginalNodeDataKey"/>.
  /// </remarks>
  internal static class ConfigureKrebsCycleLayout
  {
    /// <summary>
    ///   Creates an <see cref="ILayoutAlgorithm"/> that runs both organic layout phases and then places selected labels.
    /// </summary>
    /// <param name="graph">The graph whose node tags define the metabolic pathway data.</param>
    /// <returns>The custom Krebs cycle layout wrapped by <see cref="GenericLabeling"/> and its label-placement data.</returns>
    internal static (GenericLabeling Layout, GenericLabelingData<INode, IEdge, ILabel, ILabel> LayoutData) Create(
        IGraph graph) {
      // Wrap the two organic passes in one algorithm and place the selected labels afterward.
      var labeling = new GenericLabeling {
          CoreLayout = new KrebsCycleLayout(),
          Scope = LabelingScope.NodeLabels
      };
      var labelingData = labeling.CreateLayoutData(graph);
      labelingData.Scope.NodeLabels.Predicate = label => label.Owner is INode node &&
          (IsType(node, MetabolicPathwayData.NodeType.CoReactant) || IsType(node, MetabolicPathwayData.NodeType.Other));
      return (labeling, labelingData);
    }

    /// <summary>
    ///   Checks whether an original graph node has the specified metabolic type.
    /// </summary>
    /// <param name="node">The original graph node to inspect.</param>
    /// <param name="type">The metabolic node type to match.</param>
    /// <returns><see langword="true"/> if the node has the specified type.</returns>
    private static bool IsType(INode node, MetabolicPathwayData.NodeType type) =>
        MetabolicPathwayData.GetType(node) == type;

    /// <summary>
    ///   Performs both organic layout passes on the layout graph as a single layout algorithm.
    /// </summary>
    private sealed class KrebsCycleLayout : ILayoutAlgorithm
    {
      /// <summary>
      ///   Aligns the top pathway nodes and cycle nodes in the first organic pass, then arranges the attached
      ///   enzymes and co-reactants in the second pass.
      /// </summary>
      /// <param name="graph">The layout graph on which both organic passes are executed.</param>
      public void ApplyLayout(LayoutGraph graph) {
        // Find the nodes to align and the nodes that form the cycle.
        var alignedNodes = graph.Nodes.Where(node => GetAlignmentOrder(node) != null).OrderBy(GetAlignmentOrder)
            .ToArray();
        var circleNodes = GetCircleNodes(graph);

        // First place all nodes and establish the cycle geometry, then arrange the attached nodes.
        ApplyFirstPhase(graph, alignedNodes, circleNodes);
        ApplySecondPhase(graph, alignedNodes, circleNodes);
      }

      /// <summary>
      ///   Returns the marked nodes that belong on the circle and their associated reaction nodes.
      /// </summary>
      /// <param name="graph">The layout graph to inspect.</param>
      /// <returns>The circle nodes and the reactions adjacent to them.</returns>
      private LayoutNode[] GetCircleNodes(LayoutGraph graph) {
        var circleNodes = new List<LayoutNode>();
        foreach (var node in graph.Nodes) {
          if (GetData(node).Circle) {
            circleNodes.Add(node);
            // get the reaction nodes that are connected with these reactant nodes
            foreach (var neighbor in GetNeighbors(node)) {
              if (IsType(neighbor, MetabolicPathwayData.NodeType.Reaction) && !circleNodes.Contains(neighbor)) {
                circleNodes.Add(neighbor);
              }
            }
          }
        }
        return [.. circleNodes];
      }

      /// <summary>
      ///   Arranges all nodes, aligns the upper path, and places the cycle nodes on an ellipse.
      /// </summary>
      /// <param name="graph">The layout graph to arrange.</param>
      /// <param name="alignedNodes">The nodes ordered and aligned vertically.</param>
      /// <param name="circleNodes">The nodes constrained to the cycle ellipse.</param>
      private void ApplyFirstPhase(LayoutGraph graph, LayoutNode[] alignedNodes, LayoutNode[] circleNodes) {
        // Create the layout for the Krebs cycle.
        var organicLayout = new OrganicLayout {
            Deterministic = true,
            DefaultPreferredEdgeLength = 290,
            DefaultMinimumNodeDistance = 5
        };

        // Create the layout data for the node placement constraints.
        var layoutData = organicLayout.CreateLayoutData(graph);
        if (circleNodes.Length > 0) {
          // Create the ellipse constraint so that the nodes are placed on the boundary of a circle.
          layoutData.Constraints.AddEllipse(true, 1).Items = circleNodes;
        }

        if (alignedNodes.Length > 0) {
          // Order the nodes based on their vertical alignment data.
          AddOrderConstraint(layoutData, ConstraintOrientation.Vertical, alignedNodes);

          // Align the nodes vertically.
          layoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items = alignedNodes;

          // Make the edges between aligned nodes a bit shorter.
          var alignedSet = new HashSet<LayoutNode>(alignedNodes);
          layoutData.PreferredEdgeLengths.MapperFunction = edge =>
              alignedSet.Contains(edge.Source) && alignedSet.Contains(edge.Target) ? 200 : 290;
        }

        graph.ApplyLayout(organicLayout, layoutData);
      }

      /// <summary>
      ///   Arranges enzymes, co-reactants, and other nodes around the cycle while preserving the first pass geometry.
      /// </summary>
      /// <param name="graph">The layout graph to arrange.</param>
      /// <param name="alignedNodes">The vertically aligned nodes from the first pass.</param>
      /// <param name="circleNodes">The nodes that form the cycle.</param>
      private void ApplySecondPhase(LayoutGraph graph, LayoutNode[] alignedNodes, LayoutNode[] circleNodes) {
        var organicLayout = new OrganicLayout {
            // Specially handle star substructures.
            StarSubstructureStyle = StarSubstructureStyle.Radial,
            Deterministic = true,
            DefaultPreferredEdgeLength = 200,
            DefaultMinimumNodeDistance = 12
        };

        var layoutData = organicLayout.CreateLayoutData(graph);
        var constraints = layoutData.Constraints;

        // Track nodes that are connected to the aligned path and already have constraints.
        var handledNodes = new HashSet<LayoutNode>();
        var circleNodeSet = new HashSet<LayoutNode>(circleNodes);

        if (circleNodes.Length >= 3) {
          // Place enzymes connected to the cycle inside the cycle bounds.
          var bounds = CalculateCircleBounds(circleNodes);
          var enzymesOnCircle = graph.Nodes.Where(node => IsType(node, MetabolicPathwayData.NodeType.Enzyme) &&
              ConnectsToCircle(node, circleNodeSet)).ToList();
          constraints.AddPinnedBounds(bounds).Items = enzymesOnCircle;

          // Constrain nodes connected to the vertically aligned path.
          AddConstraintsToVerticallyAlignedPath(alignedNodes, circleNodeSet, handledNodes, constraints);

          // Constrain the co-reactants, enzymes, and other nodes attached to cycle reactions.
          AddConstraintsToCircle(graph, circleNodes, handledNodes, constraints);
        }

        // Limit this pass to enzymes, other nodes, and co-reactants so the cycle remains in place.
        var affectedNodes = graph.Nodes.Where(node => IsType(node, MetabolicPathwayData.NodeType.Enzyme) ||
                IsType(node, MetabolicPathwayData.NodeType.Other) ||
                IsType(node, MetabolicPathwayData.NodeType.CoReactant))
            .ToArray();
        layoutData.Scope.Nodes.Source = affectedNodes;
        // Add some node margins to the nodes in the organic layout scope.
        var affectedSet = new HashSet<LayoutNode>(affectedNodes);
        layoutData.NodeMargins.MapperFunction = node => affectedSet.Contains(node) ? new InsetsD(10) : InsetsD.Empty;

        // Assign preferred edge lengths around the constrained nodes.
        layoutData.PreferredEdgeLengths.MapperFunction = edge => {
          if (handledNodes.Contains(edge.Source) || handledNodes.Contains(edge.Target)) {
            return 30;
          }
          if (IsType(edge.Source, MetabolicPathwayData.NodeType.Enzyme) ||
              IsType(edge.Target, MetabolicPathwayData.NodeType.Enzyme)) {
            return 10;
          }
          return 80;
        };

        graph.ApplyLayout(organicLayout, layoutData);
      }

      /// <summary>
      ///   Adds alignment, ordering, and separation constraints for nodes attached to cycle reactions.
      /// </summary>
      /// <param name="graph">The layout graph containing the pathway.</param>
      /// <param name="circleNodes">The nodes that form the cycle.</param>
      /// <param name="handledNodes">The nodes already constrained by the top pathway.</param>
      /// <param name="constraints">The organic constraint data to update.</param>
      private void AddConstraintsToCircle(
          LayoutGraph graph,
          LayoutNode[] circleNodes,
          HashSet<LayoutNode> handledNodes,
          OrganicConstraintData<LayoutNode> constraints) {
        // calculate the center of the circle
        var circleCenter = CalculateCircleCenter(circleNodes);
        var visited = new HashSet<LayoutNode>();
        foreach (var coReactant in graph.Nodes.Where(node =>
            IsType(node, MetabolicPathwayData.NodeType.CoReactant) && !handledNodes.Contains(node))) {
          if (visited.Contains(coReactant)) {
            continue;
          }

          // Get the reaction node for this co-reactant and find its other co-reactant.
          var reaction = GetNeighbors(coReactant).FirstOrDefault();
          if (reaction == null) {
            continue;
          }
          var otherReactant = GetNeighbors(reaction).FirstOrDefault(neighbor =>
              neighbor != coReactant && IsType(neighbor, MetabolicPathwayData.NodeType.CoReactant));
          if (otherReactant == null) {
            continue;
          }

          // Align and order the pair based on the reaction's position on the circle.
          var orientation = GetOrientation(reaction.Layout.CenterX, reaction.Layout.CenterY, circleCenter);

          // Order the co-reactant pair based on whether either node has incoming edges.
          var coReactantHasIncomingEdges = coReactant.InDegree > 0;
          AddOrderConstraint(constraints, orientation, coReactantHasIncomingEdges ? otherReactant : coReactant,
              coReactantHasIncomingEdges ? coReactant : otherReactant);
          constraints.AddAlignmentConstraint(orientation).Items = coReactantHasIncomingEdges
              ? [otherReactant, coReactant]
              : [coReactant, otherReactant];
          visited.Add(otherReactant);
        }

        // Keep each reaction separated from its attached node of type 'other'.
        foreach (var node in graph.Nodes.Where(node => IsType(node, MetabolicPathwayData.NodeType.Other))) {
          var reaction = GetNeighbors(node).FirstOrDefault();
          if (reaction == null) {
            continue;
          }
          // Require a minimum distance between the reaction and its attached 'other' node.
          var separation = constraints.AddSeparationConstraint(
              GetOrientation(reaction.Layout.CenterX, reaction.Layout.CenterY, circleCenter), 0);
          separation.FirstSet.Item = node;
          separation.SecondSet.Item = reaction;
        }
      }

      /// <summary>
      ///   Constrains enzymes and co-reactants attached to the vertically aligned pathway at the top.
      /// </summary>
      /// <param name="alignedNodes">The nodes in the aligned pathway.</param>
      /// <param name="circleNodes">The nodes that belong to the cycle.</param>
      /// <param name="handledNodes">The set of nodes already constrained by this method.</param>
      /// <param name="constraints">The organic constraint data to update.</param>
      private void AddConstraintsToVerticallyAlignedPath(
          LayoutNode[] alignedNodes,
          HashSet<LayoutNode> circleNodes,
          HashSet<LayoutNode> handledNodes,
          OrganicConstraintData<LayoutNode> constraints) {
        foreach (var reaction in alignedNodes.Where(node =>
            IsType(node, MetabolicPathwayData.NodeType.Reaction) && !circleNodes.Contains(node))) {
          var coReactants = new List<LayoutNode>();

          // Sort the neighbors so shared co-reactants with incoming or outgoing edges are ordered consistently.
          var neighbors = GetNeighbors(reaction).ToArray();
          Array.Sort(neighbors, (node1, node2) => {
            var node1IsCoReactant = IsType(node1, MetabolicPathwayData.NodeType.CoReactant);
            if (node1IsCoReactant && node1.InDegree > 1 && node2.InDegree == 0) {
              return 1;
            }
            if (node1IsCoReactant && node1.OutDegree > 1 && node2.OutDegree == 0) {
              return -1;
            }
            return 0;
          });

          foreach (var neighbor in neighbors) {
            if (IsType(neighbor, MetabolicPathwayData.NodeType.Enzyme)) {
              // create a constraint to place the enzymes before their associated reaction
              AddOrderConstraint(constraints, ConstraintOrientation.Horizontal, neighbor, reaction);

              // create a constraint to align the enzymes horizontally with their associated reaction
              constraints.AddAlignmentConstraint(ConstraintOrientation.Horizontal).Items = [neighbor, reaction];
              handledNodes.Add(neighbor);
            } else if (IsType(neighbor, MetabolicPathwayData.NodeType.CoReactant) && !coReactants.Contains(neighbor)) {
              coReactants.Add(neighbor);

              // create a constraint to place the co-reactants after their associated reaction
              AddOrderConstraint(constraints, ConstraintOrientation.Horizontal, reaction, neighbor);
              handledNodes.Add(neighbor);
            }
          }

          // Align all attached co-reactants vertically.
          if (coReactants.Count > 0) {
            constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items = [.. coReactants];
          }

          // Order the pair based on their incoming and outgoing edges.
          if (coReactants.Count > 1) {
            AddOrderConstraint(constraints, ConstraintOrientation.Vertical, coReactants[0], coReactants[1]);
          }
          handledNodes.Add(reaction);
        }
      }

      /// <summary>
      ///   Calculates the cycle bounds and insets them so enzymes connected to the cycle are placed inside it.
      /// </summary>
      /// <param name="circleNodes">The nodes that form the cycle.</param>
      /// <returns>The inset bounds of the cycle.</returns>
      private static RectD CalculateCircleBounds(IEnumerable<LayoutNode> circleNodes) {
        var bounds = circleNodes.Select(node => node.Layout).ToArray();
        var left = bounds.Min(rect => rect.X);
        var top = bounds.Min(rect => rect.Y);
        var right = bounds.Max(rect => rect.X + rect.Width);
        var bottom = bounds.Max(rect => rect.Y + rect.Height);
        return new RectD(left, top, right - left, bottom - top).GetReduced(200);
      }

      /// <summary>
      ///   Checks whether a node is connected to any node on the cycle.
      /// </summary>
      /// <param name="node">The node to check.</param>
      /// <param name="circleNodes">The nodes that belong to the cycle.</param>
      /// <returns><see langword="true"/> if an edge of <paramref name="node"/> touches the cycle.</returns>
      private static bool ConnectsToCircle(LayoutNode node, HashSet<LayoutNode> circleNodes) =>
          node.Edges.Any(edge => circleNodes.Contains(edge.Source) || circleNodes.Contains(edge.Target));

      /// <summary>Returns the angle of a point around the cycle center in degrees.</summary>
      /// <param name="x">The point's x-coordinate.</param>
      /// <param name="y">The point's y-coordinate.</param>
      /// <param name="center">The center of the cycle.</param>
      /// <returns>An angle in the range from 0 to 360 degrees.</returns>
      private static double GetAngle(double x, double y, PointD center) {
        var angle = Math.Atan2(y - center.Y, x - center.X);
        if (angle < 0) {
          angle += 2 * Math.PI;
        }
        return angle * 180 / Math.PI;
      }

      /// <summary>Returns the alignment orientation for a point based on its position around the cycle.</summary>
      /// <param name="x">The point's x-coordinate.</param>
      /// <param name="y">The point's y-coordinate.</param>
      /// <param name="circleCenter">The center of the cycle.</param>
      /// <returns>The horizontal or vertical constraint orientation.</returns>
      private static ConstraintOrientation GetOrientation(double x, double y, PointD circleCenter) {
        var angle = GetAngle(x, y, circleCenter);
        return (angle > 40 && angle < 120) || (angle > 240 && angle < 250)
            ? ConstraintOrientation.Horizontal
            : ConstraintOrientation.Vertical;
      }

      /// <summary>Calculates the center of the circle through three cycle nodes.</summary>
      /// <param name="circleNodes">The nodes that form the cycle; at least three are required.</param>
      /// <returns>The center point of the circle.</returns>
      private static PointD CalculateCircleCenter(IReadOnlyList<LayoutNode> circleNodes) {
        var p1x = circleNodes[0].Layout.CenterX;
        var p1y = circleNodes[0].Layout.CenterY;
        var p2x = circleNodes[1].Layout.CenterX;
        var p2y = circleNodes[1].Layout.CenterY;
        var p3x = circleNodes[2].Layout.CenterX;
        var p3y = circleNodes[2].Layout.CenterY;
        var determinant = 2 * (p1x * p2y - p2x * p1y - p1x * p3y + p3x * p1y + p2x * p3y - p3x * p2y);
        var a = p1x * p1x + p1y * p1y;
        var b = p2x * p2x + p2y * p2y;
        var c = p3x * p3x + p3y * p3y;
        var centerX = (a * (p2y - p3y) + b * (p3y - p1y) + c * (p1y - p2y)) / determinant;
        var centerY = (a * (p3x - p2x) + b * (p1x - p3x) + c * (p2x - p1x)) / determinant;
        return new PointD(centerX, centerY);
      }

      /// <summary>Adds an order constraint for the given layout nodes.</summary>
      /// <param name="data">The organic layout data to update.</param>
      /// <param name="orientation">The direction in which the nodes should be ordered.</param>
      /// <param name="nodes">The nodes in the requested order.</param>
      private static void AddOrderConstraint(
          OrganicLayoutData<LayoutNode, LayoutEdge, LayoutNodeLabel, LayoutEdgeLabel> data,
          ConstraintOrientation orientation,
          params LayoutNode[] nodes) =>
          AddOrderConstraint(data.Constraints, orientation, nodes);

      /// <summary>Adds an order constraint by assigning an increasing rank to each node.</summary>
      /// <param name="constraints">The organic constraint data to update.</param>
      /// <param name="orientation">The direction in which the nodes should be ordered.</param>
      /// <param name="nodes">The nodes in the requested order.</param>
      private static void AddOrderConstraint(
          OrganicConstraintData<LayoutNode> constraints,
          ConstraintOrientation orientation,
          params LayoutNode[] nodes) {
        var orderConstraint = constraints.AddOrderConstraint(orientation);
        orderConstraint.Dictionary = nodes
            .Select((node, index) => new KeyValuePair<LayoutNode, IComparable>(node, index))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
      }

      /// <summary>Returns all nodes adjacent to the given node.</summary>
      /// <param name="node">The node whose neighbors are requested.</param>
      /// <returns>The source and target nodes of all incident edges.</returns>
      private IEnumerable<LayoutNode> GetNeighbors(LayoutNode node) =>
          node.InEdges.Select(edge => edge.Source).Concat(node.OutEdges.Select(edge => edge.Target));

      /// <summary>Gets the metabolic data associated with a layout node's original graph node.</summary>
      /// <param name="node">The layout node to inspect.</param>
      /// <returns>The node's metabolic pathway data.</returns>
      private MetabolicPathwayData.NodeData GetData(LayoutNode node) => (MetabolicPathwayData.NodeData) node.Tag!;

      /// <summary>Gets the node's explicit vertical alignment order, including start and end markers.</summary>
      /// <param name="node">The layout node to inspect.</param>
      /// <returns>The order, or <see langword="null"/> if the node is not vertically aligned.</returns>
      private int? GetAlignmentOrder(LayoutNode node) {
        var data = GetData(node);
        return data.AlignmentOrder ?? data.AlignmentMarker switch {
            "start" => int.MinValue,
            "end" => int.MaxValue,
            _ => null
        };
      }

      /// <summary>Checks whether a layout node's original graph node has the specified metabolic type.</summary>
      /// <param name="node">The layout node to inspect.</param>
      /// <param name="type">The metabolic node type to match.</param>
      /// <returns><see langword="true"/> if the node has the specified type.</returns>
      private bool IsType(LayoutNode node, MetabolicPathwayData.NodeType type) => GetData(node).Type == type;
    }
  }
}
