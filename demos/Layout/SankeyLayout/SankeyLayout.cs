/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using SankeyLayout;
using yWorks.Controls;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Labeling;

namespace Demo.yFiles.Layout.Sankey
{
  public class SankeyLayout
  {
    private readonly GraphControl graphControl;

    public SankeyLayout(GraphControl graphControl) {
      this.graphControl = graphControl;
    }

    public HierarchicLayout ConfigureHierarchicLayout(bool fromSketchMode) {
      var hierarchicLayout = new HierarchicLayout
      {
        LayoutOrientation = LayoutOrientation.LeftToRight,
        LayoutMode = fromSketchMode ? LayoutMode.Incremental : LayoutMode.FromScratch,
        NodeToNodeDistance = 30,
        BackLoopRouting = true,
        EdgeLayoutDescriptor =
        {
          MinimumFirstSegmentLength = 120,
          MinimumLastSegmentLength = 120,
        },
      };

      // a port border gap ratio of zero means that ports can be placed directly on the corners of the nodes
      var portBorderRatio = 1;
      hierarchicLayout.NodeLayoutDescriptor.PortBorderGapRatios = portBorderRatio;
      // configures the generic labeling algorithm which produces more compact results, here
      var genericLabeling = (GenericLabeling) hierarchicLayout.Labeling;
      genericLabeling.ReduceAmbiguity = false;
      genericLabeling.PlaceNodeLabels = false;
      genericLabeling.PlaceEdgeLabels = true;
      hierarchicLayout.LabelingEnabled = true;

      // for Sankey diagrams, the nodes should be adjusted to the incoming/outgoing flow (enlarged if necessary)
      // -> use NodeResizingStage for that purpose
      var nodeResizingStage = new NodeResizingStage(hierarchicLayout);
      nodeResizingStage.LayoutOrientation = hierarchicLayout.LayoutOrientation;
      nodeResizingStage.PortBorderGapRatio = portBorderRatio;
      hierarchicLayout.PrependStage(nodeResizingStage);

      return hierarchicLayout;
    }

    public LayoutData CreateHierarchicLayoutData() {
      // create the layout data
      return new HierarchicLayoutData
      {
        // maps each edge with its thickness so that the layout algorithm takes the edge thickness under consideration
        EdgeThickness = { Delegate = edge => ((CustomTag)edge.Tag).thickness },
        // since orientation is LEFT_TO_RIGHT, we add port constraints so that the edges leave the source node at its
        // right side and enter the target node at its left side
        SourcePortConstraints = { Delegate = edge => PortConstraint.Create(PortSide.East, false) },
        TargetPortConstraints = { Delegate = edge => PortConstraint.Create(PortSide.West, false) },
        EdgeLabelPreferredPlacement =
        {
          Constant = new PreferredPlacementDescriptor
          {
            PlaceAlongEdge = LabelPlacements.AtSource
          }
        }
      };
    }
  }
}
