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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalEdgeLabeling
{
  [Demo("layout-hierarchical-edge-labeling", "layout-features", "Hierarchical Layout with Edge Labeling",
      Summary = "Shows how to configure automatic label placement of hierarchical layout.",
      Tags = ["hierarchical", "labeling"],
      Keywords = [
          "layout", "integrated-labeling", "label-placement", "auto-flipping", "playground", "margin", "padding",
          "nodeMargin"
      ])]
  public class HierarchicalEdgeLabelingDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Default label placement
      var defaultPlacement = new EdgeLabelPreferredPlacement();

      // Place label near source node, on left of edge (relative to the edge direction)
      var closeToSourcePlacement = new EdgeLabelPreferredPlacement {
          PlacementAlongEdge = LabelAlongEdgePlacements.AtSourcePort,
          SideReference = LabelSideReferences.AbsoluteWithLeftAbove,
          EdgeSide = LabelEdgeSides.LeftOfEdge,
          DistanceToEdge = 4
      };

      // Place label near target node, on right of edge (relative to the edge direction)
      var closeToTargetPlacement = new EdgeLabelPreferredPlacement {
          PlacementAlongEdge = LabelAlongEdgePlacements.AtTargetPort,
          EdgeSide = LabelEdgeSides.RightOfEdge,
          DistanceToEdge = 4
      };

      // Rotate label parallel to edge, placed near target node
      var downwardsPlacement = new EdgeLabelPreferredPlacement {
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow,
          PlacementAlongEdge = LabelAlongEdgePlacements.AtTarget,
          EdgeSide = LabelEdgeSides.LeftOfEdge,
          DistanceToEdge = 4
      };

      // Rotate label parallel to edge
      var parallelPlacement = new EdgeLabelPreferredPlacement {
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow,
          EdgeSide = LabelEdgeSides.LeftOfEdge,
          DistanceToEdge = 4
      };

      // Rotate label by a fixed angle
      var rotatedPlacement = new EdgeLabelPreferredPlacement {
          Angle = Math.PI * 0.225,
          EdgeSide = LabelEdgeSides.LeftOfEdge
      };

      // Place label upside down
      var upsideDownPlacement = new EdgeLabelPreferredPlacement { Angle = Math.PI };

      // Place label parallel to edge but flipped
      var upwardsPlacement = new EdgeLabelPreferredPlacement {
          Angle = Math.PI,
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow,
          PlacementAlongEdge = LabelAlongEdgePlacements.AtTarget,
          EdgeSide = LabelEdgeSides.RightOfEdge,
          DistanceToEdge = 4
      };

      // Set up layout data with custom label placements based on the label text
      var layout = new HierarchicalLayout();
      var layoutData = layout.CreateLayoutData(Graph);
      layoutData.EdgeLabelPreferredPlacements.MapperFunction = label => label.Text switch {
          "Close to Source" => closeToSourcePlacement,
          "Close to Target" => closeToTargetPlacement,
          "Downwards" => downwardsPlacement,
          "Parallel" => parallelPlacement,
          "Rotated" => rotatedPlacement,
          "Upside down" => upsideDownPlacement,
          "Upwards" => upwardsPlacement,
          _ => defaultPlacement
      };
      // Reserve extra space above nodes to avoid label overlap with target arrows
      layoutData.NodeMargins.Constant = new InsetsD(15, 0, 0, 0);

      // Use unrestricted FreeEdgeLabelModel for this demo so that labels can rotate freely
      Graph.EdgeDefaults.Labels.LayoutParameter = FreeEdgeLabelModel.Instance.CreateParameter();

      // Auto-flip prevents upside-down labels, turn it off for this demo so that labels can be upside-down
      ((LabelStyle) Graph.EdgeDefaults.Labels.Style).AutoFlip = false;

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // Apply the configured hierarchical layout
      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    private static void Main() => Run<HierarchicalEdgeLabelingDemo>();
  }
}
