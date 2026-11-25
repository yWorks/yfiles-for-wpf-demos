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
using yWorks.Layout.Circular;
using yWorks.Layout.Labeling;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("CircularLayout")]
  public class CircularLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public CircularLayoutConfig() {
      var layout = new CircularLayout();
      var treeLayout = layout.BackboneLayout;

      PartitioningPolicyItem = PartitioningPolicy.BccCompact;
      ActOnSelectionOnlyItem = false;
      FromSketchItem = false;

      PartitionStyleItem = PartitionStyle.Cycle;
      MinimumNodeDistanceItem = 30;
      ChooseRadiusAutomaticallyItem = true;
      FixedRadiusItem = 200;

      DefaultBetweenCirclesRoutingItem = RoutingStyle.StraightLine;
      DefaultInCircleRoutingStyleItem = RoutingStyle.StraightLine;
      DefaultOnCircleRoutingStyleItem = OnCircleRoutingStyle.StraightLine;

      EdgeRoutingPolicyItem = EdgeRoutingPolicy.Interior;
      CircleDistanceItem = 20;
      EdgeDistanceItem = 10;
      PreferredCurveLengthItem = 20;
      PreferredAngleItem = 10;
      SmoothnessItem = 0.7;

      EdgeBundlingItem = false;
      EdgeBundlingStrengthItem = 1;

      PreferredChildSectorAngleItem = treeLayout.PreferredChildSectorAngle;
      MinimumEdgeLengthItem = treeLayout.MinimumEdgeLength;
      MaximumDeviationAngleItem = layout.MaximumDeviationAngle;
      CompactnessFactorItem = treeLayout.CompactnessFactor;
      MinimumTreeNodeDistanceItem = treeLayout.MinimumNodeDistance;
      AllowOverlapsItem = treeLayout.AllowOverlaps;
      PlaceChildrenOnCommonRadiusItem = true;

      StarSubstructureItem = StarSubstructureStyle.None;
      StarSubstructureSizeItem = layout.StarSubstructureSize;

      EdgeLabelingItem = layout.EdgeLabelPlacement != RadialEdgeLabelPlacement.Ignore;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;

      NodeLabelingStyleItem = layout.NodeLabelPlacement;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new CircularLayout();
      var radialTreeLayout = layout.BackboneLayout;

      layout.PartitioningPolicy = GetPartitioningPolicy(PartitioningPolicyItem);
      layout.EdgeRoutingPolicy = GetEdgeRoutingPolicy(EdgeRoutingPolicyItem);
      layout.ExteriorEdgeDescriptor.CircleDistance = CircleDistanceItem;
      layout.ExteriorEdgeDescriptor.EdgeDistance = EdgeDistanceItem;
      layout.ExteriorEdgeDescriptor.PreferredAngle = PreferredAngleItem;
      layout.ExteriorEdgeDescriptor.PreferredCurveLength = PreferredCurveLengthItem;
      layout.ExteriorEdgeDescriptor.Smoothness = SmoothnessItem;
      var subgraphLayoutEnabled = ActOnSelectionOnlyItem;
      var subgraphLayout = layout.LayoutStages.Get<SubgraphLayoutStage>();
      subgraphLayout.Enabled = subgraphLayoutEnabled;
      layout.MaximumDeviationAngle = MaximumDeviationAngleItem;
      layout.FromSketchMode = FromSketchItem;

      layout.PartitionDescriptor.Style = PartitionStyleItem;

      layout.PartitionDescriptor.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.PartitionDescriptor.AutomaticRadius = ChooseRadiusAutomaticallyItem;
      layout.PartitionDescriptor.FixedRadius = FixedRadiusItem;

      radialTreeLayout.PreferredChildSectorAngle = PreferredChildSectorAngleItem;
      radialTreeLayout.MinimumEdgeLength = MinimumEdgeLengthItem;
      radialTreeLayout.CompactnessFactor = CompactnessFactorItem;
      radialTreeLayout.AllowOverlaps = AllowOverlapsItem;
      layout.PlaceChildrenOnCommonRadius = PlaceChildrenOnCommonRadiusItem;
      radialTreeLayout.MinimumNodeDistance = MinimumTreeNodeDistanceItem;

      if (EdgeLabelingItem) {
        layout.EdgeLabelPlacement = RadialEdgeLabelPlacement.Generic;
        if (ReduceAmbiguityItem) {
          layout.LayoutStages.Get<GenericLabeling>().DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
        }
      } else {
        layout.EdgeLabelPlacement = RadialEdgeLabelPlacement.Ignore;
      }

      layout.EdgeDescriptor.BetweenCirclesRoutingStyle = DefaultBetweenCirclesRoutingItem;
      layout.EdgeDescriptor.InCircleRoutingStyle = DefaultInCircleRoutingStyleItem;
      layout.EdgeDescriptor.OnCircleRoutingStyle = DefaultOnCircleRoutingStyleItem;

      var ebc = layout.EdgeBundling;
      var bundlingDescriptor = new EdgeBundleDescriptor();
      bundlingDescriptor.Bundled = EdgeBundlingItem;
      ebc.BundlingStrength = EdgeBundlingStrengthItem;
      ebc.DefaultBundleDescriptor = bundlingDescriptor;

      layout.NodeLabelPlacement = NodeLabelingStyleItem;

      layout.StarSubstructureStyle = StarSubstructureItem;
      layout.StarSubstructureSize = StarSubstructureSizeItem;

      return layout;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var circularData = new CircularLayoutData<INode, IEdge, ILabel, ILabel>();

      if (PartitioningPolicyItem == PartitioningPolicy.CustomGroups) {
        var graph = graphControl.Graph;
        circularData.Partitions.MapperFunction = node => graph.GetParent(node);
      }

      if (EdgeRoutingPolicyItem == EdgeRoutingPolicy.MarkedExterior) {
        circularData.ExteriorEdges.Source = graphControl.Selection.Edges;
      }

      var resultData = circularData.CombineWith(CreateLabelingLayoutData(graphControl.Graph,
          LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem,
          LabelPlacementDistanceItem));
      if (ActOnSelectionOnlyItem) {
        resultData = resultData.CombineWith(CreateSubgraphLayoutData(graphControl));
      }
      return resultData;
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

    [Label("Partition")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object CycleGroup;

    [Label("Edges")]
    [OptionGroup("RootGroup", 25)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgesGroup;

    [Label("Default edges")]
    [OptionGroup("EdgesGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DefaultEdgesGroup;

    [Label("Exterior edges")]
    [OptionGroup("EdgesGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object ExteriorEdgesGroup;

    [Label("Tree")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object TreeGroup;

    [Label("Substructure Layout")]
    [OptionGroup("RootGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object SubstructureLayoutGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 60)]
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
        "<Paragraph>The circular layout style emphasizes group and tree structures within a network. It creates node partitions " +
        "by analyzing the connectivity structure of the network, and arranges the partitions as separate circles. The circles " +
        "themselves are arranged in a radial tree layout fashion.</Paragraph>" +
        "<Paragraph>This layout style portraits interconnected ring and star topologies and is excellent for " +
        "applications in:</Paragraph>" + "<List>" +
        "<ListItem><Paragraph>Social networking (criminology, economics, fraud detection, etc.)</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Network management</Paragraph></ListItem>" +
        "<ListItem><Paragraph>WWW visualization</Paragraph></ListItem>" +
        "<ListItem><Paragraph>eCommerce</Paragraph></ListItem>" + "</List>";

    [Label("Partitioning Policy")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(PartitioningPolicy.BccCompact)]
    [EnumValue("BCC Compact", PartitioningPolicy.BccCompact)]
    [EnumValue("BCC Isolated", PartitioningPolicy.BccIsolated)]
    [EnumValue("Custom Groups", PartitioningPolicy.CustomGroups)]
    [EnumValue("Single Cycle", PartitioningPolicy.SingleCycle)]
    public PartitioningPolicy PartitioningPolicyItem { get; set; }

    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }

    [Label("Use Drawing as Sketch")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(false)]
    public bool FromSketchItem { get; set; }

    [Label("Partition Layout Style")]
    [OptionGroup("CycleGroup", 20)]
    [DefaultValue(PartitionStyle.Cycle)]
    [EnumValue("Circle", PartitionStyle.Cycle)]
    [EnumValue("Disk", PartitionStyle.Disk)]
    [EnumValue("Compact Disk", PartitionStyle.CompactDisk)]
    [EnumValue("Organic", PartitionStyle.Organic)]
    public PartitionStyle PartitionStyleItem { get; set; }

    [Label("Minimum Node Distance")]
    [OptionGroup("CycleGroup", 30)]
    [DefaultValue(30)]
    [MinMax(Min = 0, Max = 999)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }

    public bool ShouldDisableMinimumNodeDistanceItem => !ChooseRadiusAutomaticallyItem;

    [Label("Choose Radius Automatically")]
    [OptionGroup("CycleGroup", 40)]
    [DefaultValue(true)]
    public bool ChooseRadiusAutomaticallyItem { get; set; }

    [Label("Fixed Radius")]
    [OptionGroup("CycleGroup", 50)]
    [DefaultValue(200)]
    [MinMax(Min = 1, Max = 800)]
    [ComponentType(ComponentTypes.Slider)]
    public int FixedRadiusItem { get; set; }

    public bool ShouldDisableFixedRadiusItem => ChooseRadiusAutomaticallyItem;

    [Label("Routing Style")]
    [OptionGroup("EdgesGroup", 30)]
    [DefaultValue(EdgeRoutingPolicy.Interior)]
    [EnumValue("Inside", EdgeRoutingPolicy.Interior)]
    [EnumValue("Outside", EdgeRoutingPolicy.Exterior)]
    [EnumValue("Automatic", EdgeRoutingPolicy.Automatic)]
    [EnumValue("Selected edges outside", EdgeRoutingPolicy.MarkedExterior)]
    public EdgeRoutingPolicy EdgeRoutingPolicyItem { get; set; }


    [Label("Routing Style Between Circles")]
    [OptionGroup("DefaultEdgesGroup", 10)]
    [DefaultValue(RoutingStyle.StraightLine)]
    [EnumValue("Straight", RoutingStyle.StraightLine)]
    [EnumValue("Curved", RoutingStyle.Curved)]
    public RoutingStyle DefaultBetweenCirclesRoutingItem { get; set; }

    public bool ShouldDisableDefaultBetweenCirclesRoutingItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Exterior;

    [Label("Routing Style Within Partitions")]
    [OptionGroup("DefaultEdgesGroup", 20)]
    [DefaultValue(RoutingStyle.StraightLine)]
    [EnumValue("Straight", RoutingStyle.StraightLine)]
    [EnumValue("Curved", RoutingStyle.Curved)]
    public RoutingStyle DefaultInCircleRoutingStyleItem { get; set; }

    public bool ShouldDisableDefaultInCircleRoutingStyleItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Exterior;

    [Label("Routing Style Between Neighbors")]
    [OptionGroup("DefaultEdgesGroup", 30)]
    [DefaultValue(RoutingStyle.StraightLine)]
    [EnumValue("Straight", OnCircleRoutingStyle.StraightLine)]
    [EnumValue("Curved", OnCircleRoutingStyle.Curved)]
    [EnumValue("On Circle", OnCircleRoutingStyle.OnCircle)]
    public OnCircleRoutingStyle DefaultOnCircleRoutingStyleItem { get; set; }

    public bool ShouldDisableDefaultOnCircleRoutingStyleItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Exterior;

    [Label("Distance to circle")]
    [OptionGroup("ExteriorEdgesGroup", 10)]
    [DefaultValue(20d)]
    [MinMax(Min = 10.0d, Max = 100.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CircleDistanceItem { get; set; }

    public bool ShouldDisableCircleDistanceItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior;

    [Label("Edge distance")]
    [OptionGroup("ExteriorEdgesGroup", 20)]
    [DefaultValue(10d)]
    [MinMax(Min = 5.0d, Max = 50.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeDistanceItem { get; set; }

    public bool ShouldDisableEdgeDistanceItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior;

    [Label("Corner radius")]
    [OptionGroup("ExteriorEdgesGroup", 30)]
    [DefaultValue(20d)]
    [MinMax(Min = 0.0d, Max = 100.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredCurveLengthItem { get; set; }

    public bool ShouldDisablePreferredCurveLengthItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior;

    [Label("Angle")]
    [OptionGroup("ExteriorEdgesGroup", 40)]
    [DefaultValue(10d)]
    [MinMax(Min = 0.0d, Max = 45.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredAngleItem { get; set; }

    public bool ShouldDisablePreferredAngleItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior;

    [Label("Smoothness")]
    [OptionGroup("ExteriorEdgesGroup", 50)]
    [DefaultValue(0.7d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.1d)]
    [ComponentType(ComponentTypes.Slider)]
    public double SmoothnessItem { get; set; }

    public bool ShouldDisableSmoothnessItem => EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior;

    [Label("Enable Edge Bundling")]
    [OptionGroup("EdgesGroup", 10)]
    [DefaultValue(false)]
    public bool EdgeBundlingItem { get; set; }

    public bool ShouldDisableEdgeBundlingItem =>
        PartitionStyleItem != PartitionStyle.Cycle || PartitioningPolicyItem == PartitioningPolicy.BccIsolated;

    [Label("Bundling Strength")]
    [OptionGroup("EdgesGroup", 20)]
    [DefaultValue(1)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem =>
        !EdgeBundlingItem || PartitionStyleItem != PartitionStyle.Cycle ||
        PartitioningPolicyItem == PartitioningPolicy.BccIsolated;

    [Label("Preferred Child Sector Angle")]
    [OptionGroup("TreeGroup", 10)]
    [DefaultValue(340)]
    [MinMax(Min = 1, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredChildSectorAngleItem { get; set; }

    [Label("Minimum Edge Length")]
    [OptionGroup("TreeGroup", 20)]
    [DefaultValue(40)]
    [MinMax(Min = 5, Max = 400)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeLengthItem { get; set; }

    [Label("Maximum Deviation Angle")]
    [OptionGroup("TreeGroup", 30)]
    [DefaultValue(90)]
    [MinMax(Min = 1, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public double MaximumDeviationAngleItem { get; set; }

    [Label("Compactness Factor")]
    [OptionGroup("TreeGroup", 40)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.1d, Max = 0.9d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CompactnessFactorItem { get; set; }

    [Label("Minimum Node Distance")]
    [OptionGroup("TreeGroup", 50)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumTreeNodeDistanceItem { get; set; }

    [Label("Allow Overlaps")]
    [OptionGroup("TreeGroup", 60)]
    [DefaultValue(false)]
    public bool AllowOverlapsItem { get; set; }

    [Label("Place Children on Common Radius")]
    [OptionGroup("TreeGroup", 70)]
    [DefaultValue(true)]
    public bool PlaceChildrenOnCommonRadiusItem { get; set; }

    public bool ShouldDisableTreeGroupItem => PartitioningPolicyItem == PartitioningPolicy.SingleCycle;

    [Label("Star")]
    [OptionGroup("SubstructureLayoutGroup", 10)]
    [DefaultValue(StarSubstructureStyle.None)]
    [EnumValue("Ignore", StarSubstructureStyle.None)]
    [EnumValue("Radial", StarSubstructureStyle.Radial)]
    [EnumValue("Separated Radial", StarSubstructureStyle.SeparatedRadial)]
    public StarSubstructureStyle StarSubstructureItem { get; set; }

    [Label("Minimum Star Size")]
    [OptionGroup("SubstructureLayoutGroup", 15)]
    [MinMax(Min = 4, Max = 20)]
    [DefaultValue(4)]
    public int StarSubstructureSizeItem { get; set; }

    public bool ShouldDisableStarSubstructureSizeItem => StarSubstructureItem == StarSubstructureStyle.None;

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(RadialNodeLabelPlacement.Consider)]
    [EnumValue("Ignore Labels", RadialNodeLabelPlacement.Ignore)]
    [EnumValue("Consider Labels", RadialNodeLabelPlacement.Consider)]
    [EnumValue("Horizontal", RadialNodeLabelPlacement.Horizontal)]
    [EnumValue("Generic", RadialNodeLabelPlacement.Generic)]
    [EnumValue("Ray-like at Leaves", RadialNodeLabelPlacement.RayLikeLeaves)]
    [EnumValue("Ray-like", RadialNodeLabelPlacement.RayLike)]
    public RadialNodeLabelPlacement NodeLabelingStyleItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(true)]
    public bool EdgeLabelingItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
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

    public yWorks.Layout.Circular.PartitioningPolicy GetPartitioningPolicy(PartitioningPolicy style) {
      switch (style) {
        case PartitioningPolicy.BccCompact:
          return yWorks.Layout.Circular.PartitioningPolicy.BccCompact;
        case PartitioningPolicy.BccIsolated:
          return yWorks.Layout.Circular.PartitioningPolicy.BccIsolated;
        case PartitioningPolicy.CustomGroups:
        case PartitioningPolicy.SingleCycle:
          return yWorks.Layout.Circular.PartitioningPolicy.SingleCycle;
        default:
          throw new ArgumentOutOfRangeException(nameof(style), style, null);
      }
    }

    public enum PartitioningPolicy
    {
      BccCompact = 0,
      BccIsolated = 1,
      CustomGroups = 2,
      SingleCycle = 3
    }

    public enum EdgeRoutingPolicy : sbyte
    {
      Interior = 0,
      Automatic = 1,
      Exterior = 2,
      MarkedExterior = 3
    }

    private yWorks.Layout.Circular.EdgeRoutingPolicy GetEdgeRoutingPolicy(EdgeRoutingPolicy policy) {
      switch (policy) {
        case EdgeRoutingPolicy.MarkedExterior:
        case EdgeRoutingPolicy.Interior:
          return yWorks.Layout.Circular.EdgeRoutingPolicy.Interior;
        case EdgeRoutingPolicy.Automatic:
          return yWorks.Layout.Circular.EdgeRoutingPolicy.Automatic;
        case EdgeRoutingPolicy.Exterior:
          return yWorks.Layout.Circular.EdgeRoutingPolicy.Exterior;
        default:
          throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
      }
    }
  }
}
