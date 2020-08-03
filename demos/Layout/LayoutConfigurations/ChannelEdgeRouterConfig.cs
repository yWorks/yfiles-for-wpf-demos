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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("ChannelEdgeRouter")]
  public class ChannelEdgeRouterConfig : LayoutConfiguration {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public ChannelEdgeRouterConfig() {
      ScopeItem = Scope.RouteAllEdges;
      MinimumDistanceItem = 10;
      ActivateGridRoutingItem = true;
      GridSpacingItem = 20;

      BendCostItem = 1;
      EdgeCrossingCostItem = 5;
      NodeCrossingCostItem = 50;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var router = new ChannelEdgeRouter();

      var orthogonalPatternEdgeRouter = new OrthogonalPatternEdgeRouter();

      orthogonalPatternEdgeRouter.AffectedEdgesDpKey = ChannelEdgeRouter.AffectedEdgesDpKey;
      orthogonalPatternEdgeRouter.MinimumDistance = MinimumDistanceItem;
      orthogonalPatternEdgeRouter.GridRouting = ActivateGridRoutingItem;
      orthogonalPatternEdgeRouter.GridSpacing = GridSpacingItem;

      orthogonalPatternEdgeRouter.BendCost = BendCostItem;
      orthogonalPatternEdgeRouter.EdgeCrossingCost = EdgeCrossingCostItem;
      orthogonalPatternEdgeRouter.NodeCrossingCost = NodeCrossingCostItem;

      // disable edge overlap costs when Edge distribution will run afterwards anyway
      orthogonalPatternEdgeRouter.EdgeOverlapCost = 0;

      router.PathFinderStrategy = orthogonalPatternEdgeRouter;

      var segmentDistributionStage = new OrthogonalSegmentDistributionStage();
      segmentDistributionStage.AffectedEdgesDpKey = ChannelEdgeRouter.AffectedEdgesDpKey;
      segmentDistributionStage.PreferredDistance = MinimumDistanceItem;
      segmentDistributionStage.GridRouting = ActivateGridRoutingItem;
      segmentDistributionStage.GridSpacing = GridSpacingItem;

      router.EdgeDistributionStrategy = segmentDistributionStage;

      return router;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new ChannelEdgeRouterData();
      var selection = graphControl.Selection;
      if (ScopeItem == Scope.RouteEdgesAtAffectedNodes) {
        layoutData.AffectedEdges.Delegate = edge =>
            selection.IsSelected(edge.GetSourceNode()) || selection.IsSelected(edge.GetTargetNode());
      } else if (ScopeItem == Scope.RouteAffectedEdges) {
        layoutData.AffectedEdges.Source = selection.SelectedEdges;
      } else {
        layoutData.AffectedEdges.Source = graphControl.Graph.Edges;
      }
      return layoutData;
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;
    
    [Label("Layout")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LayoutGroup;

    [Label("Costs")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object CostsGroup;
    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>Channel edge router uses a rather fast but "
               + "simple algorithm for finding orthogonal edge routes. As other routing algorithms it does not change the positions of the nodes. "
               + "Compared to polyline and orthogonal edge router, edge segments can be very "
               + "close to each other and edges may also overlap with nodes. However, this algorithm is faster in many situations.</Paragraph>";
      }
    } 

    [Label("Scope")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(Scope.RouteAllEdges)]
    [EnumValue("All Edges", Scope.RouteAllEdges)]
    [EnumValue("Selected Edges",Scope.RouteAffectedEdges)]
    [EnumValue("Edges at Selected Nodes",Scope.RouteEdgesAtAffectedNodes)]
    public Scope ScopeItem { get; set; }

    [Label("Minimum Distance")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(10)]
    [MinMax(Min = 0.0d, Max = 100.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumDistanceItem { get; set; }

    [Label("Route on Grid")]
    [OptionGroup("LayoutGroup", 40)]
    [DefaultValue(true)]
    public bool ActivateGridRoutingItem { get; set; }

    [Label("Grid Spacing")]
    [OptionGroup("LayoutGroup", 50)]
    [DefaultValue(20)]
    [MinMax(Min = 2, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int GridSpacingItem { get; set; }

    public bool ShouldDisableGridSpacingItem {
      get { return !ActivateGridRoutingItem; }
    }

    [Label("Bend Cost")]
    [OptionGroup("CostsGroup", 10)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double BendCostItem { get; set; }

    [Label("Edge Crossing Cost")]
    [OptionGroup("CostsGroup", 20)]
    [DefaultValue(5.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeCrossingCostItem { get; set; }

    [Label("Node Overlap Cost")]
    [OptionGroup("CostsGroup", 30)]
    [DefaultValue(50.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double NodeCrossingCostItem { get; set; }

  }
}