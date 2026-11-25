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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Organic;

namespace yWorks.Demo.MetabolicPathways
{
  /// <summary>
  ///   Configures the organic layout and adds the needed constraints to handle the nodes that have to
  ///   be aligned horizontally or vertically.
  /// </summary>
  internal static class ConfigurePentoseLayout
  {
    internal static (OrganicLayout Layout, OrganicLayoutData<INode, IEdge, ILabel, ILabel> LayoutData) Create(
        IGraph graph) {
      // some basic configuration for the organic layout
      var organicLayout = new OrganicLayout {
          Deterministic = true,
          DefaultPreferredEdgeLength = 60,
          DefaultMinimumNodeDistance = 10,
          AvoidNodeEdgeOverlap = true
      };

      // create the layout data to assign different edge lengths for edges attached to enzymes
      // and to add margins to reaction nodes
      var organicLayoutData = organicLayout.CreateLayoutData(graph);
      organicLayoutData.PreferredEdgeLengths.MapperFunction = edge =>
          IsType(edge.GetSourceNode(), MetabolicPathwayData.NodeType.Enzyme) ||
          IsType(edge.GetTargetNode(), MetabolicPathwayData.NodeType.Enzyme)
              ? 5
              : 80;
      organicLayoutData.NodeMargins.MapperFunction = node =>
          IsType(node, MetabolicPathwayData.NodeType.Reaction) ? new InsetsD(15, 60, 60, 15) : InsetsD.Empty;

      // determine and align vertically the main pathway which is defined by a node with in-degree zero
      // and a node which is marked with vAlign = end in its tag
      var reactionPath = AlignReactionPath(graph, organicLayoutData);

      // determine the nodes that represent the reactions
      var reactions = graph.Nodes.Where(node => IsType(node, MetabolicPathwayData.NodeType.Reaction)).ToArray();
      // place the nodes in layers
      var layers = PlaceNodesOnLayers(graph, reactions, organicLayoutData);

      // sort the reaction nodes based on their layer
      Array.Sort(reactions, (node1, node2) => layers[node1].CompareTo(layers[node2]));

      // add the constraints for the co-reactant nodes to vertically align them and place them before
      // their associated reaction node
      AddCoReactantConstraints(graph, organicLayoutData);

      // add the constraints for the enzymes to align them after their associated reaction
      AddEnzymeConstraints(graph, organicLayoutData);

      // add the constraints for the reaction nodes to align them vertically on the main path
      AddReactionConstraints(reactions, graph, reactionPath, organicLayoutData);

      // add the constraints to reaction nodes that contain more than two reactants
      AddMultiProductReactionConstraints(reactions, graph, organicLayoutData);
      return (organicLayout, organicLayoutData);
    }

    /// <summary>
    ///   Defines the main reaction path and creates the constraints to vertically align the nodes on this path.
    ///   From the main path, reaction nodes that contain more than two reactants are excluded.
    /// </summary>
    private static INode[] AlignReactionPath(
        IGraph graph,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      // calculate the shortest path
      var pathNodes = CalculateMainReactionPath(graph);
      // exclude the reaction nodes that contain more than two reactants - these do not need to
      // be vertically aligned since they are attached to four nodes
      var alignedNodes = pathNodes.Where(node =>
          !IsType(node, MetabolicPathwayData.NodeType.Reaction) || !IsMultipleProductReaction(graph, node)).ToArray();

      // align the main path vertically and order its nodes based on their order in the path
      organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items = alignedNodes;
      AddOrderConstraint(organicLayoutData, ConstraintOrientation.Vertical, alignedNodes);
      return pathNodes;
    }

    /// <summary>
    ///   Places the nodes in layers using breadth-first search and creates constraints so that nodes in each layer
    ///   are horizontally aligned and consecutive layers are ordered vertically.
    /// </summary>
    private static ResultItemMapping<INode, int> PlaceNodesOnLayers(
        IGraph graph,
        INode[] reactions,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      // we apply a breadth-first search to calculate the layers of the nodes
      var firstBfs = CalculateLayers(graph);

      // Since we want the products/reactants to be placed below their corresponding reaction,
      // we check whether there exist reactions that contain multiple reactants, and if yes,
      // for this reaction we consider only the edges that connect to the product with the maximum layer
      var excludedEdges = new HashSet<IEdge>();
      foreach (var reaction in reactions.Where(reaction => IsMultipleProductReaction(graph, reaction))) {
        // get the edges attached to product/reactant nodes
        var productEdges = graph.InEdgesAt(reaction).Where(edge => IsProductOrReactant(edge.GetSourceNode())).ToArray();
        var reactionLayer = firstBfs.NodeLayerIds[reaction];

        // get the maximum layer of these edges
        var maxLayer = productEdges.Select(edge => {
          var node = edge.GetSourceNode();
          return firstBfs.NodeLayerIds[node];
        }).DefaultIfEmpty(reactionLayer).Max();

        if (maxLayer != reactionLayer) {
          // use the first edge that does not belong to the maximum layer of these edges
          var edgeToExclude = productEdges.FirstOrDefault(edge => {
            var node = edge.GetSourceNode();
            return firstBfs.NodeLayerIds[node] != maxLayer;
          });
          if (edgeToExclude != null) {
            excludedEdges.Add(edgeToExclude);
          }
        }
      }

      var bfs = excludedEdges.Count > 0 ? CalculateLayers(graph, excludedEdges) : firstBfs;

      // create the constraints for each layer
      foreach (var bfsLayer in bfs.Layers) {
        var layerNodes = bfsLayer.Nodes;
        // nodes that belong in the same layer have to be horizontally aligned
        organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Horizontal).Source = layerNodes;

        if (bfsLayer.Index > 0) {
          // nodes that belong to consecutive layers have to be ordered based on their bfs layering
          var orderedNodes = bfs.Layers[bfsLayer.Index - 1].Nodes.Concat(bfsLayer.Nodes).ToArray();
          AddOrderConstraint(organicLayoutData, ConstraintOrientation.Vertical, orderedNodes);
        }
      }

      return bfs.NodeLayerIds;
    }

    /// <summary>
    ///   Creates the constraints for the nodes representing the co-reactants. Namely, co-reactants
    ///   have to be vertically aligned and placed before their associated reaction.
    /// </summary>
    private static void AddCoReactantConstraints(
        IGraph graph,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      // find the nodes representing the co-reactants
      var coReactants = graph.Nodes.Where(node => IsType(node, MetabolicPathwayData.NodeType.CoReactant)).ToArray();

      // align vertically the co-reactants
      organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items = coReactants;

      foreach (var coReactant in coReactants) {
        if (graph.OutEdgesAt(coReactant).Count > 0) {
          var reaction = graph.OutEdgesAt(coReactant).First().GetTargetNode();
          // for each co-reactant, find the associated reaction and place:
          // (i) the reaction after the co-reactant in the horizontal axis
          // (ii) the reaction below the co-reactant
          AddOrderConstraint(organicLayoutData, ConstraintOrientation.Horizontal, coReactant, reaction);
          AddOrderConstraint(organicLayoutData, ConstraintOrientation.Vertical, coReactant, reaction);
        } else if (graph.InEdgesAt(coReactant).Count > 0) {
          var reaction = graph.InEdgesAt(coReactant).First().GetSourceNode();
          // for each co-reactant, find the associated reaction and place,
          // (i) the reaction after the co-reactant in the horizontal axis
          // (ii) the reaction above the co-reactant
          AddOrderConstraint(organicLayoutData, ConstraintOrientation.Horizontal, coReactant, reaction);
          AddOrderConstraint(organicLayoutData, ConstraintOrientation.Vertical, reaction, coReactant);
        }
      }
    }

    /// <summary>
    ///   Creates the constraints for the nodes representing the enzymes.
    ///   These should be horizontally aligned with their associated reaction and placed after it.
    /// </summary>
    private static void AddEnzymeConstraints(
        IGraph graph,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      var enzymes = graph.Nodes.Where(node => IsType(node, MetabolicPathwayData.NodeType.Enzyme));
      foreach (var enzyme in enzymes) {
        var reaction = graph.Neighbors(enzyme).First();
        // align horizontally the reaction and the enzyme
        organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Horizontal).Items =
            [reaction, enzyme];
        // place the reaction before the enzyme
        AddOrderConstraint(organicLayoutData, ConstraintOrientation.Horizontal, reaction, enzyme);
        organicLayoutData.Constraints.AddMaximumDistance(reaction, enzyme, 0, ConstraintOrientation.Horizontal);
      }
    }

    /// <summary>
    ///   Creates the constraints for the nodes representing reactions that contain two reactants.
    ///   These should be vertically aligned on the main path.
    /// </summary>
    private static void AddReactionConstraints(
        IEnumerable<INode> reactions,
        IGraph graph,
        INode[] reactionPath,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      foreach (var reaction in reactions) {
        foreach (var edge in graph.EdgesAt(reaction)) {
          var opposite = edge.Opposite(reaction) as INode;
          if (IsProductOrReactant(opposite) && !IsMultipleProductReaction(graph, reaction) &&
              !reactionPath.Contains(opposite)) {
            // align vertically the reactions with the reactants on the path
            organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items =
                [reaction, opposite];
          }
        }
      }
    }

    /// <summary>
    ///   Creates the constraints for the nodes representing reactions that contain more than two reactants.
    ///   In this case, the four products involved in the reaction should be pairwise aligned vertically.
    /// </summary>
    private static void AddMultiProductReactionConstraints(
        IEnumerable<INode> reactions,
        IGraph graph,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      foreach (var reaction in reactions.Where(reaction => IsMultipleProductReaction(graph, reaction))) {
        var incoming = graph.InEdgesAt(reaction).Select(edge => edge.GetSourceNode()).Where(IsProductOrReactant)
            .ToArray();
        var outgoing = graph.OutEdgesAt(reaction).Select(edge => edge.GetTargetNode()).Where(IsProductOrReactant)
            .ToArray();
        if (incoming.Length < 2 || outgoing.Length < 2) {
          continue;
        }

        // align vertically pairwise the reactants
        organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items =
            [incoming[1], outgoing[0]];
        // align vertically pairwise the reactants
        organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical).Items =
            [incoming[0], outgoing[1]];

        organicLayoutData.Constraints.AddMaximumDistance(incoming[0], reaction, 8, ConstraintOrientation.Horizontal);
        organicLayoutData.Constraints.AddMaximumDistance(incoming[1], reaction, 8, ConstraintOrientation.Horizontal);
        organicLayoutData.Constraints.AddMaximumDistance(outgoing[0], reaction, 8, ConstraintOrientation.Horizontal);
        organicLayoutData.Constraints.AddMaximumDistance(outgoing[1], reaction, 8, ConstraintOrientation.Horizontal);
      }
    }

    /// <summary>
    ///   Applies the shortest path algorithm starting from a node with no incoming edges and ending at a node
    ///   which is marked with vAlign = 'end' in its tag.
    /// </summary>
    private static INode[] CalculateMainReactionPath(IGraph graph) {
      var start = graph.Nodes.FirstOrDefault(node => graph.InEdgesAt(node).Count == 0);
      var end = graph.Nodes.FirstOrDefault(MetabolicPathwayData.IsEndAlignment);
      if (start == null || end == null) {
        return [];
      }

      var shortestPath = new ShortestPath {
          Source = { Item = start },
          Sink = { Item = end }
      };
      shortestPath.SubgraphNodes.Excludes.Predicate = node =>
          IsType(node, MetabolicPathwayData.NodeType.Enzyme) || IsType(node, MetabolicPathwayData.NodeType.CoReactant);
      return [.. shortestPath.Run(graph).Nodes];
    }

    /// <summary>
    ///   Calculates the layer of each node that represents a product or a reactant starting from a node with
    ///   no incoming edges. For the breadth-first search, nodes representing enzymes and co-reactants are excluded.
    /// </summary>
    private static Bfs.Result CalculateLayers(IGraph graph, ISet<IEdge>? excludedEdges = null) {
      var start = graph.Nodes.FirstOrDefault(node => graph.InEdgesAt(node).Count == 0 && IsProductOrReactant(node));

      var bfs = new Bfs {
          CoreNodes = { Item = start },
          TraversalDirection = TraversalDirection.Successor
      };
      bfs.SubgraphNodes.Excludes.Predicate = node =>
          IsType(node, MetabolicPathwayData.NodeType.Enzyme) || IsType(node, MetabolicPathwayData.NodeType.CoReactant);
      bfs.SubgraphEdges.Excludes.Predicate = edge => excludedEdges?.Contains(edge) == true;
      return bfs.Run(graph);
    }

    private static void AddOrderConstraint(
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> data,
        ConstraintOrientation orientation,
        params INode[] nodes) {
      var orderConstraint = data.Constraints.AddOrderConstraint(orientation);
      orderConstraint.Dictionary = nodes.Select((node, index) => new KeyValuePair<INode, IComparable>(node, index))
          .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private static bool IsProductOrReactant(INode node) =>
        IsType(node, MetabolicPathwayData.NodeType.Product) || IsType(node, MetabolicPathwayData.NodeType.Reactant);

    private static bool IsMultipleProductReaction(IGraph graph, INode node) =>
        graph.Neighbors(node).Count(IsProductOrReactant) > 2;

    private static bool IsType(INode node, MetabolicPathwayData.NodeType type) =>
        MetabolicPathwayData.GetType(node) == type;
  }
}
