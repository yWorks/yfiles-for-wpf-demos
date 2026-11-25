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
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;

namespace yWorks.Demo.OrganicEdgeLabeling
{
  [Demo("layout-organic-edge-labeling", "layout-features", "Organic Layout with Edge Labeling",
      Summary = "Shows how to configure automatic label placement of organic layout.", Tags = ["organic", "labeling"],
      Keywords = [
          "layout", "integrated-labeling", "label-placement", "auto-flipping", "playground", "margin", "nodeMargin"
      ])]
  public class OrganicEdgeLabelingDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Configure the organic layout
      var organicLayout = new OrganicLayout {
          // Enable the automatic edge label placement
          EdgeLabelPlacement = EdgeLabelPlacement.Integrated,
          // Set the preferred length for the edges
          DefaultPreferredEdgeLength = 150,
          Deterministic = true
      };

      /*
        Note: Integrated edge labeling produces satisfying results for a lot of cases by default.
        Additionally, the algorithm offers control over how to place individual labels (EdgeLabelPreferredPlacement).
        For better results, specify both the side and angle of the labels relative to the edge.
      */

      // Place label close to the edge's source node, on the left and parallel to the edge flow
      var closeToSourcePlacement = new EdgeLabelPreferredPlacement {
          PlacementAlongEdge = LabelAlongEdgePlacements.AtSourcePort,
          SideReference = LabelSideReferences.RelativeToEdgeFlow,
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow,
          EdgeSide = LabelEdgeSides.LeftOfEdge,
          DistanceToEdge = 4
      };

      // Place label close to the edge's target node, on the right and parallel to the edge flow
      var closeToTargetPlacement = new EdgeLabelPreferredPlacement {
          PlacementAlongEdge = LabelAlongEdgePlacements.AtTargetPort,
          SideReference = LabelSideReferences.RelativeToEdgeFlow,
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow,
          EdgeSide = LabelEdgeSides.RightOfEdge,
          DistanceToEdge = 4
      };

      // Default: Place label parallel to edge flow
      var defaultPlacement = new EdgeLabelPreferredPlacement {
          SideReference = LabelSideReferences.RelativeToEdgeFlow,
          AngleReference = LabelAngleReferences.RelativeToEdgeFlow
      };

      // Use layout data to pass the info about the placement of individual edges
      var organicLayoutData = organicLayout.CreateLayoutData(Graph);
      // Assign preferred label placements based on the label text
      organicLayoutData.EdgeLabelPreferredPlacements.MapperFunction = label => label.Text switch {
          "Source" or "Parallel Source" => closeToSourcePlacement,
          "Target" or "Parallel Target" => closeToTargetPlacement,
          _ => defaultPlacement
      };

      /*
        Note: Labels on parallel edges or self-loops are routed by specialized routers (ParallelEdgeRouter
        and SelfLoopRouter), not the organic layout itself.
        For these, we must use a generic labeling algorithm.
      */

      // Setup GenericLabeling
      var layout = new GenericLabeling {
          CoreLayout = organicLayout,
          // Place only the edge labels
          Scope = LabelingScope.EdgeLabels,
          Deterministic = true
      };

      // Specify that GenericLabeling should only arrange labels on self-loops,
      // all other labels are placed by the layout algorithm
      var labelingData = layout.CreateLayoutData(Graph);
      labelingData.Scope.EdgeLabels.Predicate = label => ((IEdge) label.Owner).IsSelfLoop();

      // Prevent labels overlap with edge arrows by reserving space above nodes
      labelingData.NodeMargins.Constant = new InsetsD(5);

      // Use unrestricted FreeEdgeLabelModel for this demo so that labels can rotate freely
      Graph.EdgeDefaults.Labels.LayoutParameter = FreeEdgeLabelModel.Instance.CreateParameter();

      // Combine all layout data to pass all info to the algorithm
      var layoutData = organicLayoutData.CombineWith(labelingData);

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // TODO: remove
      new HierarchicalLayout();

      // Apply the configured layout and layout data
      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    private static void Main() => Run<OrganicEdgeLabelingDemo>();
  }
}
