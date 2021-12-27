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

using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("BalloonLayout")]
  public class BalloonLayoutConfig : LayoutConfiguration {

    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public BalloonLayoutConfig() {
      var layout = new BalloonLayout();

      RootNodePolicyItem = RootNodePolicy.DirectedRoot;
      RoutingStyleForNonTreeEdgesItem = EnumRoute.Orthogonal;
      ActOnSelectionOnlyItem = false;
      EdgeBundlingStrengthItem = 1;
      PreferredChildWedgeItem = layout.PreferredChildWedge;
      PreferredRootWedgeItem = layout.PreferredRootWedge;
      MinimumEdgeLengthItem = layout.MinimumEdgeLength;
      CompactnessFactorItem = layout.CompactnessFactor;
      AllowOverlapsItem = layout.AllowOverlaps;
      FromSketchItem = layout.FromSketchMode;
      PlaceChildrenInterleavedItem = layout.InterleavedMode == InterleavedMode.AllNodes;
      StraightenChainsItem = layout.ChainStraighteningMode;

      NodeLabelingStyleItem = EnumNodeLabelingPolicies.ConsiderCurrentPosition;
      EdgeLabelingItem = EnumEdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new BalloonLayout();

      ((ComponentLayout)layout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;

      layout.RootNodePolicy = RootNodePolicyItem;
      layout.PreferredChildWedge = PreferredChildWedgeItem;
      layout.PreferredRootWedge = PreferredRootWedgeItem;
      layout.MinimumEdgeLength = MinimumEdgeLengthItem;
      layout.CompactnessFactor = 1-CompactnessFactorItem;
      layout.AllowOverlaps = AllowOverlapsItem;
      layout.FromSketchMode = FromSketchItem;
      layout.ChainStraighteningMode = StraightenChainsItem;
      layout.InterleavedMode = PlaceChildrenInterleavedItem ? InterleavedMode.AllNodes : InterleavedMode.Off;

      switch (NodeLabelingStyleItem) {
        case EnumNodeLabelingPolicies.None:
          layout.ConsiderNodeLabels = false;
          break;
        case EnumNodeLabelingPolicies.RaylikeLeaves:
          layout.IntegratedNodeLabeling = true;
          layout.NodeLabelingPolicy = NodeLabelingPolicy.RayLikeLeaves;
          break;
        case EnumNodeLabelingPolicies.ConsiderCurrentPosition:
          layout.ConsiderNodeLabels = true;
          break;
        case EnumNodeLabelingPolicies.Horizontal:
          layout.IntegratedNodeLabeling = true;
          layout.NodeLabelingPolicy = NodeLabelingPolicy.Horizontal;
          break;
        default:
          layout.ConsiderNodeLabels = false;
          break;
      }

      // configures tree reduction stage and non-tree edge routing.
      layout.SubgraphLayoutEnabled = ActOnSelectionOnlyItem;
      MultiStageLayout multiStageLayout = layout;

      var treeReductionStage = new TreeReductionStage();
      multiStageLayout.AppendStage(treeReductionStage);
      if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Organic) {
        var organic = new OrganicEdgeRouter();
        treeReductionStage.NonTreeEdgeRouter = organic;
        treeReductionStage.NonTreeEdgeSelectionKey = OrganicEdgeRouter.AffectedEdgesDpKey;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Orthogonal) {
        var edgeRouter = new EdgeRouter {
            Rerouting = true,
            Scope = Scope.RouteAffectedEdges
        };
        treeReductionStage.NonTreeEdgeSelectionKey = edgeRouter.AffectedEdgesDpKey;
        treeReductionStage.NonTreeEdgeRouter = edgeRouter;
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.StraightLine) {
        treeReductionStage.NonTreeEdgeRouter = treeReductionStage.CreateStraightLineRouter();
      } else if (RoutingStyleForNonTreeEdgesItem == EnumRoute.Bundled) {
        var ebc = treeReductionStage.EdgeBundling;
        var bundleDescriptor = new EdgeBundleDescriptor { Bundled = true };
        ebc.BundlingStrength = EdgeBundlingStrengthItem;
        ebc.DefaultBundleDescriptor = bundleDescriptor;
      }

      if (EdgeLabelingItem == EnumEdgeLabeling.Generic) {
        layout.IntegratedEdgeLabeling = false;
        var genericLabeling = new GenericLabeling {
            PlaceEdgeLabels = true,
            PlaceNodeLabels = false,
            ReduceAmbiguity = ReduceAmbiguityItem
        };
        layout.LabelingEnabled = true;
        layout.Labeling = genericLabeling;
      } else if (EdgeLabelingItem == EnumEdgeLabeling.Integrated) {
        layout.IntegratedEdgeLabeling = true;
        treeReductionStage.NonTreeEdgeLabelingAlgorithm = new GenericLabeling();
      }

      if (NodeLabelingStyleItem == EnumNodeLabelingPolicies.RaylikeLeaves || NodeLabelingStyleItem == EnumNodeLabelingPolicies.Horizontal) {
        foreach (var label in graphControl.Graph.GetNodeLabels()) {
          graphControl.Graph.SetLabelLayoutParameter(label, FreeNodeLabelModel.Instance.FindBestParameter(label, FreeNodeLabelModel.Instance, label.GetLayout()));
        }
      }

      return layout;
    }

    /// <inheritdoc/>
    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new BalloonLayoutData();

      if (RootNodePolicyItem == RootNodePolicy.SelectedRoot) {
        var selection = graphControl.Selection.SelectedNodes;

        if (selection.Any()) {
          layoutData.TreeRoot.Item = selection.First();
        }
      }

      return layoutData.CombineWith(
          CreateLabelingLayoutData(
              graphControl.Graph,
              LabelPlacementAlongEdgeItem,
              LabelPlacementSideOfEdgeItem,
              LabelPlacementOrientationItem,
              LabelPlacementDistanceItem
          )
      );
    }

    public enum EnumRoute {
      Orthogonal, Organic, StraightLine, Bundled
    }

    public enum EnumEdgeLabeling {
      None, Integrated, Generic
    }

    public enum EnumNodeLabelingPolicies {
      None, Horizontal, RaylikeLeaves, ConsiderCurrentPosition
    };

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
    public string DescriptionText {
      get {
        return "<Paragraph>The balloon layout is a tree layout that positions the subtrees in a radial " +
               "fashion around their root nodes. It is ideally suited for larger trees.</Paragraph>";
      }
    }

    [Label("Root Node Policy")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(RootNodePolicy.DirectedRoot)]
    [EnumValue("Directed Root",RootNodePolicy.DirectedRoot)]
    [EnumValue("Center Root",RootNodePolicy.CenterRoot)]
    [EnumValue("Weighted Center Root",RootNodePolicy.WeightedCenterRoot)]
    [EnumValue("Selected Node", RootNodePolicy.SelectedRoot)]
    public RootNodePolicy RootNodePolicyItem { get; set; }

    [Label("Routing Style for Non-Tree Edges")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(EnumRoute.Orthogonal)]
    [EnumValue("Orthogonal",EnumRoute.Orthogonal)]
    [EnumValue("Organic",EnumRoute.Organic)]
    [EnumValue("Straight-Line",EnumRoute.StraightLine)]
    [EnumValue("Bundled",EnumRoute.Bundled)]
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

    public bool ShouldDisableEdgeBundlingStrengthItem {
      get { return RoutingStyleForNonTreeEdgesItem != EnumRoute.Bundled; }
    }

    [Label("Preferred Child Wedge")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(210)]
    [MinMax(Min = 1, Max = 359)]
    [ComponentType(ComponentTypes.Slider)]
    public int PreferredChildWedgeItem { get; set; }

    [Label("Preferred Root Wedge")]
    [OptionGroup("GeneralGroup", 60)]
    [DefaultValue(360)]
    [MinMax(Min = 1, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public int PreferredRootWedgeItem { get; set; }

    [Label("Minimum Edge Length")]
    [OptionGroup("GeneralGroup", 70)]
    [DefaultValue(40)]
    [MinMax(Min = 10, Max = 400)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumEdgeLengthItem { get; set; }

    [Label("Compactness Factor")]
    [OptionGroup("GeneralGroup", 80)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.1d, Max = 0.9d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CompactnessFactorItem { get; set; }

    [Label("Allow Overlaps")]
    [OptionGroup("GeneralGroup", 90)]
    [DefaultValue(false)]
    public bool AllowOverlapsItem { get; set; }

    [Label("Use Drawing as Sketch")]
    [OptionGroup("GeneralGroup", 100)]
    [DefaultValue(false)]
    public bool FromSketchItem { get; set; }

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
    [DefaultValue(EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Ignore Labels", EnumNodeLabelingPolicies.None)]
    [EnumValue("Consider Labels",EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Horizontal",EnumNodeLabelingPolicies.Horizontal)]
    [EnumValue("Ray-like at Leaves",EnumNodeLabelingPolicies.RaylikeLeaves)]
    public EnumNodeLabelingPolicies NodeLabelingStyleItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EnumEdgeLabeling.None)]
    [EnumValue("None", EnumEdgeLabeling.None)]
    [EnumValue("Integrated",EnumEdgeLabeling.Integrated)]
    [EnumValue("Generic",EnumEdgeLabeling.Generic)]
    public EnumEdgeLabeling EdgeLabelingItem {
      get { return edgeLabelingItem; }
      set {
        edgeLabelingItem = value;
        if (value == EnumEdgeLabeling.Integrated) {
          LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Parallel;
          LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.AtTarget;
          LabelPlacementDistanceItem = 0;
        }
      }
    }
    private EnumEdgeLabeling edgeLabelingItem;

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem {
      get { return EdgeLabelingItem != EnumEdgeLabeling.Generic; }
    }

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal",EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal",EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical",EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None || EdgeLabelingItem == EnumEdgeLabeling.Integrated; }
    }

    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source",EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Target",EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("Centered",EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementAlongEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None || EdgeLabelingItem == EnumEdgeLabeling.Integrated; }
    }

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge",EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left",EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right",EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right",EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementSideOfEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None ||
                   EdgeLabelingItem == EnumEdgeLabeling.Integrated ||
                   LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }

  }
}
