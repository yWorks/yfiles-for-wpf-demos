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

using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Grouping;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;
using yWorks.Utils;
using RoutingStyle = yWorks.Layout.Hierarchical.RoutingStyle;

namespace Demo.yFiles.Layout.MixedLayout
{
  /// <summary>
  ///   Demonstrates how to use the recursive group layout to realize different layouts of elements assigned
  ///   to different tiers.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Each group node can be assigned to the left, right or middle tier.
  ///   </para>
  ///   <para>
  ///     All group nodes mapped to <see cref="TierType.LeftTreeGroupNode"/> are placed on the left side. Their content is
  ///     drawn using a <see cref="TreeLayout"/> with layout orientation left-to-right.
  ///     Analogously, all group nodes mapped to <see cref="TierType.RightTreeGroupNode"/> are placed on the right side.
  ///     Their content is drawn using a <see cref="TreeLayout"/> with layout orientation right-to-left.
  ///     Elements not assigned to left or right group nodes are always lay out in the middle using the
  ///     <see cref="HierarchicalLayout"/> with layout orientation left-to-right. Note that group nodes of type
  ///     <see cref="TierType.CommonNode"/> are handled non-recursive.
  ///   </para>
  /// </remarks>
  internal class ThreeTierLayout : LayoutStageBase
  {
    public enum TierType
    {
      CommonNode,
      LeftTreeGroupNode,
      RightTreeGroupNode
    }

    /// <summary>
    ///   Creates a new instance of <c>ThreeTierLayout</c>.
    /// </summary>
    /// <param name="fromSketch"/>
    /// Whether the
    /// <see cref="HierarchicalLayout"/>
    /// shall be run in incremental layout mode.
    public ThreeTierLayout(bool fromSketch) {
      var hl = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
          FromSketchMode = fromSketch,
          DefaultEdgeDescriptor = { RoutingStyleDescriptor = new RoutingStyleDescriptor(RoutingStyle.Polyline) }
      };
      var rgl = new RecursiveGroupLayout(hl) { AutoAssignPortCandidates = true, FromSketchMode = true };

      CoreLayout = rgl;
    }

    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      CoreLayout?.ApplyLayout(graph);
    }

    /// <summary>
    ///   The layout data that shall be used for the ThreeTierLayout with the specified graph.
    /// </summary>
    public static LayoutData<INode, IEdge, ILabel, ILabel> CreateLayoutData(IGraph graph, bool fromSketch) {
      // set up port candidates for edges (edges should be attached to the left/right side of the corresponding node
      var candidates = new EdgePortCandidates().AddFreeCandidate(PortSides.Left).AddFreeCandidate(PortSides.Right);

      // configure the different sub group layout settings
      var leftToRightTreeLayout = new TreeReductionStage();
      leftToRightTreeLayout.NonTreeEdgeRouter = new StraightLineEdgeRouter();
      leftToRightTreeLayout.CoreLayout = new TreeLayout { LayoutOrientation = LayoutOrientation.LeftToRight };

      var rightToLeftTreeLayout = new TreeReductionStage();
      rightToLeftTreeLayout.NonTreeEdgeRouter = new StraightLineEdgeRouter();
      rightToLeftTreeLayout.CoreLayout = new TreeLayout { LayoutOrientation = LayoutOrientation.RightToLeft };

      var view = graph.Lookup<IFoldingView>();

      var rglData = new RecursiveGroupLayoutData<INode, IEdge, ILabel, ILabel> {
          Ports = {
              SourcePortCandidates = { Constant = candidates }, TargetPortCandidates = { Constant = candidates }
          },

          // map each group node to the layout algorithm that should be used for its content
          GroupNodeLayouts = {
              MapperFunction = node => {
                switch (GetTierType(node, view, graph)) {
                  case TierType.LeftTreeGroupNode:
                    return leftToRightTreeLayout;
                  case TierType.RightTreeGroupNode:
                    return rightToLeftTreeLayout;
                  default:
                    return null;
                }
              }
          }
      };

      var hlData = new HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> {
          NodeDescriptors = {
              MapperFunction = node => {
                // align tree group nodes within their layer
                switch (GetTierType(node, view, graph)) {
                  case TierType.LeftTreeGroupNode:
                    return new HierarchicalLayoutNodeDescriptor { LayerAlignment = 1 };
                  case TierType.RightTreeGroupNode:
                    return new HierarchicalLayoutNodeDescriptor { LayerAlignment = 0 };
                  default:
                    return null;
                }
              }
          }
      };

      if (!fromSketch) {
        // insert layer constraints to guarantee the desired placement for "left" and "right" group nodes
        var layerConstraintData = hlData.LayerConstraints;
        foreach (var node in graph.Nodes) {
          // align tree group nodes within their layer
          var type = GetTierType(node, view, graph);
          if (type == TierType.LeftTreeGroupNode) {
            layerConstraintData.PlaceAtTop(node);
          } else if (type == TierType.RightTreeGroupNode) {
            layerConstraintData.PlaceAtBottom(node);
          }
        }
      }

      return new CompositeLayoutData<INode, IEdge, ILabel, ILabel>(rglData, hlData);
    }

    internal static TierType GetTierType(INode node, IFoldingView foldingView, IGraph graph) {
      if (graph.IsGroupNode(node)
          // node is an expanded group node
          || foldingView.IsInFoldingState(node)) {
        var labelText = node.Labels.Count > 0 ? node.Labels[0].Text : "";
        switch (labelText) {
          case "left":
            return TierType.LeftTreeGroupNode;
          case "right":
            return TierType.RightTreeGroupNode;
          default:
            return TierType.CommonNode;
        }
      }
      return TierType.CommonNode;
    }
  }
}
