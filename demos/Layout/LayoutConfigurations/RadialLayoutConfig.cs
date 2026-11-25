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
using System.ComponentModel;
using System.Reflection;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Radial;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("RadialLayout")]
  public class RadialLayoutConfig : LayoutConfiguration
  {
    private const int MaximumSmoothness = 10;
    private const int MinimumSmoothness = 1;
    private const int SmoothnessAngleFactor = 4;

    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public RadialLayoutConfig() {
      var layout = new RadialLayout();

      CenterStrategyItem = CenterNodesPolicy.WeightedCentrality;
      LayeringStrategyItem = RadialLayeringStrategy.Bfs;
      MinimumLayerDistanceItem = (int) layout.MinimumLayerDistance;
      MinimumNodeDistanceItem = (int) layout.MinimumNodeDistance;
      MaximumChildSectorAngleItem = (int) layout.MaximumChildSectorAngle;
      EdgeRoutingStyleItem = EdgeRoutingStyle.Arc;
      EdgeSmoothnessItem = MaximumSmoothness;
      EdgeBundlingStrengthItem = 0.95;

      EdgeLabelingItem = false;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
      NodeLabelingStyleItem = layout.NodeLabelPlacement;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new RadialLayout();
      layout.MinimumNodeDistance = MinimumNodeDistanceItem;
      if (EdgeRoutingStyleItem != EdgeRoutingStyle.Bundled) {
        layout.EdgeRoutingStyle = (RoutingStyle) EdgeRoutingStyleItem;
      }

      var minimumBendAngle = 1 + (MaximumSmoothness - EdgeSmoothnessItem) * SmoothnessAngleFactor;
      layout.MinimumBendAngle = minimumBendAngle;
      layout.MinimumLayerDistance = MinimumLayerDistanceItem;
      layout.MaximumChildSectorAngle = MaximumChildSectorAngleItem;
      layout.CenterNodesPolicy = GetCenterStrategy(CenterStrategyItem);
      layout.LayeringStrategy = LayeringStrategyItem;

      var ebc = layout.EdgeBundling;
      ebc.BundlingStrength = EdgeBundlingStrengthItem;
      ebc.DefaultBundleDescriptor = new EdgeBundleDescriptor {
          Bundled = EdgeRoutingStyleItem == EdgeRoutingStyle.Bundled
      };

      if (EdgeLabelingItem) {
        layout.EdgeLabelPlacement = RadialEdgeLabelPlacement.Generic;
        if (ReduceAmbiguityItem) {
          layout.LayoutStages.Get<GenericLabeling>().DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
        }
      }

      layout.NodeLabelPlacement = NodeLabelingStyleItem;

      return layout;
    }

    private yWorks.Layout.Radial.CenterNodesPolicy GetCenterStrategy(CenterNodesPolicy strategy) {
      switch (strategy) {
        case CenterNodesPolicy.Directed:
          return yWorks.Layout.Radial.CenterNodesPolicy.Directed;
        case CenterNodesPolicy.Centrality:
          return yWorks.Layout.Radial.CenterNodesPolicy.Centrality;
        case CenterNodesPolicy.WeightedCentrality:
        case CenterNodesPolicy.Custom:
          return yWorks.Layout.Radial.CenterNodesPolicy.WeightedCentrality;
        default:
          throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
      }
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new RadialLayoutData<INode, IEdge, ILabel, ILabel>();

      if (CenterStrategyItem == CenterNodesPolicy.Custom) {
        layoutData.CenterNodes.Source = graphControl.Selection.Nodes;
      }

      return layoutData.CombineWith(CreateLabelingLayoutData(graphControl.Graph, LabelPlacementAlongEdgeItem,
          LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem));
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup;

    [Label("Node Settings")]
    [OptionGroup("LabelingGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object NodePropertiesGroup;

    [Label("Edge Settings")]
    [OptionGroup("LabelingGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgePropertiesGroup;

    [Label("Preferred Edge Label Placement")]
    [OptionGroup("LabelingGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object PreferredPlacementGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The radial layout arranges the nodes of a graph on concentric circles. Similar to hierarchical layouts, " +
        "the overall flow of the graph is nicely visualized.</Paragraph>" +
        "<Paragraph>This style is well suited for the visualization of directed graphs and tree-like structures.</Paragraph>";

    [Label("Minimum Circle Distance")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(100)]
    [MinMax(Min = 1, Max = 1000)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumLayerDistanceItem { get; set; }

    [Label("Minimum Node Distance")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 300)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }

    [Label("Maximum Child Sector Angle")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(180)]
    [MinMax(Min = 15, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public int MaximumChildSectorAngleItem { get; set; }

    [Label("Routing Style")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(EdgeRoutingStyle.Arc)]
    [EnumValue("Straight", EdgeRoutingStyle.Polyline)]
    [EnumValue("Arc", EdgeRoutingStyle.Arc)]
    [EnumValue("Curved", EdgeRoutingStyle.Curved)]
    [EnumValue("Radial Polyline", EdgeRoutingStyle.RadialPolyline)]
    [EnumValue("Bundled", EdgeRoutingStyle.Bundled)]
    public EdgeRoutingStyle EdgeRoutingStyleItem { get; set; }

    [Label("Arc Smoothness")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(10)]
    [MinMax(Min = MinimumSmoothness, Max = MaximumSmoothness)]
    [ComponentType(ComponentTypes.Slider)]
    public int EdgeSmoothnessItem { get; set; }

    public bool ShouldDisableEdgeSmoothnessItem => EdgeRoutingStyleItem != EdgeRoutingStyle.Arc;

    [Label("Bundling Strength")]
    [OptionGroup("GeneralGroup", 55)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem => EdgeRoutingStyleItem != EdgeRoutingStyle.Bundled;

    [Label("Center Allocation Strategy")]
    [OptionGroup("GeneralGroup", 60)]
    [DefaultValue(CenterNodesPolicy.WeightedCentrality)]
    [EnumValue("Directed", CenterNodesPolicy.Directed)]
    [EnumValue("Centrality", CenterNodesPolicy.Centrality)]
    [EnumValue("Weighted Centrality", CenterNodesPolicy.WeightedCentrality)]
    [EnumValue("Selected Nodes", CenterNodesPolicy.Custom)]
    public CenterNodesPolicy CenterStrategyItem { get; set; }

    [Label("Circle Assignment Strategy")]
    [OptionGroup("GeneralGroup", 70)]
    [DefaultValue(RadialLayeringStrategy.Bfs)]
    [EnumValue("Distance From Center", RadialLayeringStrategy.Bfs)]
    [EnumValue("Hierarchical", RadialLayeringStrategy.Hierarchical)]
    [EnumValue("Dendrogram", RadialLayeringStrategy.Dendrogram)]
    public RadialLayeringStrategy LayeringStrategyItem { get; set; }

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(RadialNodeLabelPlacement.Consider)]
    [EnumValue("Ignore Labels", RadialNodeLabelPlacement.Ignore)]
    [EnumValue("Consider Labels", RadialNodeLabelPlacement.Consider)]
    [EnumValue("Generic", RadialNodeLabelPlacement.Generic)]
    [EnumValue("Horizontal", RadialNodeLabelPlacement.Horizontal)]
    [EnumValue("Ray-like at Leaves", RadialNodeLabelPlacement.RayLikeLeaves)]
    [EnumValue("Ray-like", RadialNodeLabelPlacement.RayLike)]
    public RadialNodeLabelPlacement NodeLabelingStyleItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool EdgeLabelingItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    [DefaultValue(false)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem => !EdgeLabelingItem;

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem => !EdgeLabelingItem;

    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source", EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Source Port", EnumLabelPlacementAlongEdge.AtSourcePort)]
    [EnumValue("At Target", EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("At Target Port", EnumLabelPlacementAlongEdge.AtTargetPort)]
    [EnumValue("Centered", EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementAlongEdgeItem => !EdgeLabelingItem;

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementSideOfEdgeItem => !EdgeLabelingItem;

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem =>
        !EdgeLabelingItem || LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge;

    public enum EdgeRoutingStyle
    {
      Polyline = RoutingStyle.Polyline,
      Arc = RoutingStyle.Arc,
      RadialPolyline = RoutingStyle.RadialPolyline,
      Curved = RoutingStyle.Curved,
      Bundled
    }

    public enum CenterNodesPolicy : sbyte
    {
      Directed = 0,
      Centrality = 1,
      WeightedCentrality = 2,
      Custom = 3
    }
  }
}
