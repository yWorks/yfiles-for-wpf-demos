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

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("RadialTreeLayout")]
  public class RadialTreeLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public RadialTreeLayoutConfig() {
      var layout = new RadialTreeLayout();

      RootSelectionPolicyItem = RadialTreeRootNodePolicy.DirectedRoot;
      RoutingStyleForNonTreeEdgesItem = EnumRoute.Orthogonal;
      ActOnSelectionOnlyItem = false;
      EdgeBundlingStrengthItem = 1;
      PreferredChildSectorAngleItem = layout.PreferredChildSectorAngle;
      PreferredRootSectorAngleItem = layout.PreferredRootSectorAngle;
      MinimumEdgeLengthItem = layout.MinimumEdgeLength;
      CompactnessFactorItem = layout.CompactnessFactor;
      AllowOverlapsItem = layout.AllowOverlaps;
      ChildOrderingPolicyItem = ChildOrderingPolicy.Compact;
      PlaceChildrenInterleavedItem = false;
      StraightenChainsItem = layout.ChainStraighteningMode;

      NodeLabelingStyleItem = layout.NodeLabelPlacement;
      EdgeLabelingItem = layout.EdgeLabelPlacement;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new RadialTreeLayout();

      layout.ComponentLayout.Style = ComponentArrangementStyle.MultiRows;

      layout.RootSelectionPolicy = GetRootNodePolicy(RootSelectionPolicyItem);
      layout.PreferredChildSectorAngle = PreferredChildSectorAngleItem;
      layout.PreferredRootSectorAngle = PreferredRootSectorAngleItem;
      layout.MinimumEdgeLength = MinimumEdgeLengthItem;
      layout.CompactnessFactor = CompactnessFactorItem;
      layout.AllowOverlaps = AllowOverlapsItem;
      layout.ChildOrderingPolicy = ChildOrderingPolicyItem;
      layout.ChainStraighteningMode = StraightenChainsItem;
      layout.NodeLabelPlacement = NodeLabelingStyleItem;

      var subgraphLayout = layout.LayoutStages.Get<SubgraphLayoutStage>();
      subgraphLayout.Enabled = ActOnSelectionOnlyItem;

      // configures tree reduction stage and non-tree edge routing.
      var treeReductionStage = layout.TreeReductionStage;
      if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Organic) {
        var organic = new OrganicEdgeRouter();
        treeReductionStage.NonTreeEdgeRouter = organic;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Orthogonal) {
        var edgeRouter = new EdgeRouter { Rerouting = true };
        treeReductionStage.NonTreeEdgeRouter = edgeRouter;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.StraightLine) {
        treeReductionStage.NonTreeEdgeRouter = new StraightLineEdgeRouter();
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Bundled) {
        var ebc = treeReductionStage.EdgeBundling;
        var bundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
        ebc.BundlingStrength = EdgeBundlingStrengthItem;
        ebc.DefaultBundleDescriptor = bundleDescriptor;
      }

      if (EdgeLabelingItem == EdgeLabelPlacement.Generic) {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Generic;
        if (ReduceAmbiguityItem) {
          layout.LayoutStages.Get<GenericLabeling>()!.DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
        }
      } else if (EdgeLabelingItem == EdgeLabelPlacement.Integrated) {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Integrated;
        treeReductionStage.NonTreeEdgeLabeling = new GenericLabeling();
      }

      return layout;
    }

    /// <inheritdoc/>
    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var radialTreeData = new RadialTreeLayoutData<INode, IEdge, ILabel, ILabel>();

      if (RootSelectionPolicyItem == RadialTreeRootNodePolicy.SelectedRoot) {
        var selection = graphControl.Selection.Nodes;

        if (selection.Any()) {
          radialTreeData.TreeRoot.Item = selection.First();
        }
      }

      if (PlaceChildrenInterleavedItem) {
        radialTreeData.InterleavedNodes.Predicate = _ => true;
      }

      var resultData = radialTreeData.CombineWith(CreateLabelingLayoutData(graphControl.Graph,
          LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem,
          LabelPlacementDistanceItem));
      if (ActOnSelectionOnlyItem) {
        resultData = resultData.CombineWith(CreateSubgraphLayoutData(graphControl));
      }
      return resultData;
    }

    public enum EnumRoute
    {
      Orthogonal,
      Organic,
      StraightLine,
      Bundled
    }

    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
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
        "<Paragraph>The radial tree layout is a tree layout that positions the subtrees in a radial " +
        "fashion around their root nodes. It is ideally suited for larger trees.</Paragraph>";

    [Label("Root Selection Policy")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(RadialTreeRootNodePolicy.DirectedRoot)]
    [EnumValue("Directed Root", RadialTreeRootNodePolicy.DirectedRoot)]
    [EnumValue("Center Root", RadialTreeRootNodePolicy.CenterRoot)]
    [EnumValue("Weighted Center Root", RadialTreeRootNodePolicy.WeightedCenterRoot)]
    [EnumValue("Selected Node", RadialTreeRootNodePolicy.SelectedRoot)]
    public RadialTreeRootNodePolicy RootSelectionPolicyItem { get; set; }

    [Label("Routing Style for Non-Tree Edges")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(EnumRoute.Orthogonal)]
    [EnumValue("Orthogonal", EnumRoute.Orthogonal)]
    [EnumValue("Organic", EnumRoute.Organic)]
    [EnumValue("Straight-Line", EnumRoute.StraightLine)]
    [EnumValue("Bundled", EnumRoute.Bundled)]
    public EnumRoute RoutingStyleForNonTreeEdgesItem { get; set; }

    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }

    [Label("Bundling Strength")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(1.0)]
    [MinMax(Min = 0, Max = 1.0, Step = 0.01)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem => RoutingStyleForNonTreeEdgesItem != EnumRoute.Bundled;

    [Label("Preferred Child Sector Angle")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(210)]
    [MinMax(Min = 1, Max = 359)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredChildSectorAngleItem { get; set; }

    [Label("Preferred Root Sector Angle")]
    [OptionGroup("GeneralGroup", 60)]
    [DefaultValue(360)]
    [MinMax(Min = 1, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredRootSectorAngleItem { get; set; }

    [Label("Minimum Edge Length")]
    [OptionGroup("GeneralGroup", 70)]
    [DefaultValue(40)]
    [MinMax(Min = 10, Max = 400)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeLengthItem { get; set; }

    [Label("Compactness Factor")]
    [OptionGroup("GeneralGroup", 80)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CompactnessFactorItem { get; set; }

    [Label("Allow Overlaps")]
    [OptionGroup("GeneralGroup", 90)]
    [DefaultValue(false)]
    public bool AllowOverlapsItem { get; set; }

    [Label("Child Ordering Policy")]
    [OptionGroup("GeneralGroup", 100)]
    [DefaultValue(RadialTreeRootNodePolicy.DirectedRoot)]
    [EnumValue("Compact", ChildOrderingPolicy.Compact)]
    [EnumValue("From Sketch", ChildOrderingPolicy.FromSketch)]
    [EnumValue("Symmetric", ChildOrderingPolicy.Symmetric)]
    public ChildOrderingPolicy ChildOrderingPolicyItem { get; set; }

    [Label("Place Children Interleaved")]
    [OptionGroup("GeneralGroup", 110)]
    [DefaultValue(false)]
    public bool PlaceChildrenInterleavedItem { get; set; }

    [Label("Straighten Chains")]
    [OptionGroup("GeneralGroup", 120)]
    [DefaultValue(false)]
    public bool StraightenChainsItem { get; set; }

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 20)]
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
    [DefaultValue(EdgeLabelPlacement.Integrated)]
    [EnumValue("Ignore", EdgeLabelPlacement.Ignore)]
    [EnumValue("Integrated", EdgeLabelPlacement.Integrated)]
    [EnumValue("Generic", EdgeLabelPlacement.Generic)]
    public EdgeLabelPlacement EdgeLabelingItem {
      get => edgeLabelingItem;
      set {
        edgeLabelingItem = value;
        if (value == EdgeLabelPlacement.Integrated) {
          LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Parallel;
          LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.AtTarget;
          LabelPlacementDistanceItem = 0;
        }
      }
    }

    private EdgeLabelPlacement edgeLabelingItem;

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem => EdgeLabelingItem != EdgeLabelPlacement.Generic;

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem =>
        EdgeLabelingItem == EdgeLabelPlacement.Ignore || EdgeLabelingItem == EdgeLabelPlacement.Integrated;

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

    public bool ShouldDisableLabelPlacementAlongEdgeItem =>
        EdgeLabelingItem == EdgeLabelPlacement.Ignore || EdgeLabelingItem == EdgeLabelPlacement.Integrated;

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementSideOfEdgeItem => EdgeLabelingItem == EdgeLabelPlacement.Ignore;

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem =>
        EdgeLabelingItem == EdgeLabelPlacement.Ignore || EdgeLabelingItem == EdgeLabelPlacement.Integrated ||
        LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge;

    private static RootSelectionPolicy GetRootNodePolicy(RadialTreeRootNodePolicy policy) {
      switch (policy) {
        case RadialTreeRootNodePolicy.CenterRoot:
          return RootSelectionPolicy.CenterRoot;
        case RadialTreeRootNodePolicy.WeightedCenterRoot:
          return RootSelectionPolicy.WeightedCenterRoot;
        case RadialTreeRootNodePolicy.SelectedRoot:
        case RadialTreeRootNodePolicy.DirectedRoot:
          return RootSelectionPolicy.DirectedRoot;
        default:
          throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
      }
    }

    public enum RadialTreeRootNodePolicy : sbyte
    {
      DirectedRoot = 0,
      CenterRoot = 1,
      WeightedCenterRoot = 2,
      SelectedRoot = 3
    }
  }
}
