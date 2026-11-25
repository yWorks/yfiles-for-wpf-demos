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

using System.Linq;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SankeyLayout
{
  public class SankeyLayout
  {
    public HierarchicalLayout ConfigureHierarchicalLayout(bool fromSketchMode) {
      var hierarchicalLayout = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
          FromSketchMode = fromSketchMode,
          NodeDistance = 30,
          DefaultEdgeDescriptor = {
              MinimumFirstSegmentLength = 120,
              MinimumLastSegmentLength = 120,
              RoutingStyleDescriptor = new RoutingStyleDescriptor(RoutingStyle.Polyline),
              BackLoopRouting = true
          },
          DefaultNodeDescriptor = { BorderToPortGapRatio = 1 }
      };

      // a border gap to port gap ratio of zero means that ports can be placed directly on the corners of the nodes
      var borderToPortGapRatio = 1;
      hierarchicalLayout.DefaultNodeDescriptor.BorderToPortGapRatio = borderToPortGapRatio;
      // configures the hierarchical layout to use the generic labeling algorithm which produces more compact results
      hierarchicalLayout.EdgeLabelPlacement = EdgeLabelPlacement.Generic;

      // for Sankey diagrams, the nodes should be adjusted to the incoming/outgoing flow (enlarged if necessary)
      // -> use NodeResizingStage for that purpose
      var nodeResizingStage = new NodeResizingStage(hierarchicalLayout);
      nodeResizingStage.LayoutOrientation = hierarchicalLayout.LayoutOrientation;
      nodeResizingStage.PortBorderGapRatio = borderToPortGapRatio;
      hierarchicalLayout.LayoutStages.Prepend(nodeResizingStage);

      return hierarchicalLayout;
    }

    public LayoutData<INode, IEdge, ILabel, ILabel> CreateHierarchicalLayoutData(IGraph graph, bool fromSketchMode) {
      // create the layout data
      var hierarchicalLayoutData = new HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> {
          // maps each edge with its thickness so that the layout algorithm takes the edge thickness under consideration
          EdgeThickness = { MapperFunction = edge => ((CustomTag) edge.Tag).thickness },
          // since orientation is LEFT_TO_RIGHT, we add port constraints so that the edges leave the source node at its
          // right side and enter the target node at its left side
          Ports = {
              SourcePortCandidates = {
                  MapperFunction = edge => new EdgePortCandidates().AddFreeCandidate(PortSides.Right)
              },
              TargetPortCandidates = {
                  MapperFunction = edge => new EdgePortCandidates().AddFreeCandidate(PortSides.Left)
              }
          },
          EdgeLabelPreferredPlacements = {
              Constant = new EdgeLabelPreferredPlacement { PlacementAlongEdge = LabelAlongEdgePlacements.AtSource }
          }
      };

      if (!fromSketchMode) {
        // In this demo, the nodes are ordered in the layers based on their size to show how the party
        // strength gets modified along the elections.
        // From this ordering the non-voters are excluded and are placed at the bottom of the layer
        hierarchicalLayoutData.SequenceConstraints.ItemComparables.MapperFunction = item =>
            item is INode node && !IsNonVoter(node)
                ? graph.EdgesAt(node).Sum(edge => ((CustomTag) edge.Tag).thickness)
                : 0;
      }
      return hierarchicalLayoutData;
    }

    /// <summary>
    ///   Returns whether the given node represents a non-voter.
    /// </summary>
    private static bool IsNonVoter(INode node) => node.Labels.Count > 0 && node.Labels[0]!.Text == "Non-voter";
  }
}
