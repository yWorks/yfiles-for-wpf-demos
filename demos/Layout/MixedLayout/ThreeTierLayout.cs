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

using System.Collections.Generic;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.MixedLayout
{
  /// <summary>
  /// Demonstrates how to use the recursive group layout to realize different layouts of elements assigned
  /// to different tiers.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each group node can be assigned to the left, right or middle tier.
  /// </para>
  /// <para>
  /// All group nodes mapped to <see cref="TierType.LeftTreeGroupNode"/> are placed on the left side. Their content is
  /// drawn using a <see cref="ClassicTreeLayout"/> with layout orientation left-to-right.
  /// Analogously, all group nodes mapped to <see cref="TierType.RightTreeGroupNode"/> are placed on the right side.
  /// Their content is drawn using a <see cref="ClassicTreeLayout"/> with layout orientation right-to-left.
  /// Elements not assigned to left or right group nodes are always lay out in the middle using the
  /// <see cref="HierarchicLayout"/> with layout orientation left-to-right. Note that group nodes of type
  /// <see cref="TierType.CommonNode"/> are handled non-recursive.
  /// </para>
  /// </remarks>
  internal class ThreeTierLayout : LayoutStageBase
  {
    public enum TierType
    {
      CommonNode,
      LeftTreeGroupNode,
      RightTreeGroupNode
    }

    ///<summary>
    /// Creates a new instance of <c>ThreeTierLayout</c>.
    ///</summary>
    /// <param name="fromSketch"/> Whether the <see cref="HierarchicLayout"/> shall be run in incremental layout mode.
    public ThreeTierLayout(bool fromSketch) {
      HierarchicLayout hl = new HierarchicLayout
      {
        LayoutOrientation = LayoutOrientation.LeftToRight,
        LayoutMode = fromSketch ? LayoutMode.Incremental : LayoutMode.FromScratch,
      };
      var rgl = new RecursiveGroupLayout(hl) {AutoAssignPortCandidates = true, FromSketchMode = true};

      CoreLayout = rgl;
    }

    public override void ApplyLayout(LayoutGraph graph) {
      ApplyLayoutCore(graph);
    }

    /// <summary>
    /// The layout data that shall be used for the ThreeTierLayout with the specified graph.
    /// </summary>
    internal class LayoutData : CompositeLayoutData {
      /// <summary>
      /// Creates a new instance configured for the specified graph.
      /// </summary>
      /// <param name="graph">The graph to configure the layout data for</param>
      /// <param name="fromSketch"/> Whether the <see cref="HierarchicLayout"/> shall be run in incremental layout mode.
      public LayoutData(IGraph graph, bool fromSketch) {

        // set up port candidates for edges (edges should be attached to the left/right side of the corresponding node
        var candidates = new List<PortCandidate>
          {
        PortCandidate.CreateCandidate(PortDirections.West),
        PortCandidate.CreateCandidate(PortDirections.East)
      };

        // configure the different sub group layout settings
        var leftToRightTreeLayout = new TreeReductionStage();
        leftToRightTreeLayout.NonTreeEdgeRouter = leftToRightTreeLayout.CreateStraightLineRouter();
        leftToRightTreeLayout.CoreLayout = new TreeLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
        };

        var rightToLeftTreeLayout = new TreeReductionStage();
        rightToLeftTreeLayout.NonTreeEdgeRouter = rightToLeftTreeLayout.CreateStraightLineRouter();
        rightToLeftTreeLayout.CoreLayout = new TreeLayout {
          LayoutOrientation = LayoutOrientation.RightToLeft,
        };

        var view = graph.Lookup<IFoldingView>();

        Items.Add(new RecursiveGroupLayoutData {
          SourcePortCandidates = { Constant = candidates },
          TargetPortCandidates = { Constant = candidates },

          // map each group node to the layout algorithm that should be used for its content
          GroupNodeLayouts = {
            Delegate = node => {
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
        });

        var hlData = new HierarchicLayoutData {
          NodeLayoutDescriptors = {
            Delegate = node => {
              // align tree group nodes within their layer
              switch (GetTierType(node, view, graph)) {
                case TierType.LeftTreeGroupNode:
                  return new NodeLayoutDescriptor { LayerAlignment = 1 };
                case TierType.RightTreeGroupNode:
                  return new NodeLayoutDescriptor { LayerAlignment = 0 };
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
            TierType type = GetTierType(node, view, graph);
            if (type == TierType.LeftTreeGroupNode) {
              layerConstraintData.PlaceAtTop(node);
            } else if (type == TierType.RightTreeGroupNode) {
              layerConstraintData.PlaceAtBottom(node);
            }
          }
        }
        Items.Add(hlData);
      }
    }

    internal static TierType GetTierType(INode node, IFoldingView foldingView, IGraph graph) {
      if (graph.IsGroupNode(node)
        // node is an expanded group node
          || foldingView.IsInFoldingState(node)) {
        string labelText = (node.Labels.Count > 0)
          ? node.Labels[0].Text
          : "";
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
