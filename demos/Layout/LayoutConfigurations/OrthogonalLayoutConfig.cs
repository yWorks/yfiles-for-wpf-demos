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

using System.ComponentModel;
using System.Linq;
using System.Reflection;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Labeling;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Router;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("OrthogonalLayout")]
  public class OrthogonalLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public OrthogonalLayoutConfig() {
      ModeItem = OrthogonalLayoutMode.Strict;
      GridSpacingItem = 15;
      QualityTimeRatioItem = 0.6;
      UseExistingDrawingAsSketchItem = false;

      NodeLabelingItem = NodeLabelPlacement.Consider;
      EdgeLabelingItem = EdgeLabelPlacement.Integrated;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10.0d;

      MinimumFirstSegmentLengthItem = 15.0d;
      MinimumSegmentLengthItem = 15.0d;
      MinimumLastSegmentLengthItem = 15.0d;
      ConsiderEdgeDirectionItem = false;

      ChainSubstructureStyleItem = ChainSubstructureStyle.None;
      ChainSubstructureSizeItem = 2;
      CycleSubstructureStyleItem = CycleSubstructureStyle.None;
      CycleSubstructureSizeItem = 4;
      TreeSubstructureStyleItem = TreeSubstructureStyle.None;
      TreeSubstructureSizeItem = 3;
      TreeSubstructureOrientationItem = SubstructureOrientation.AutoDetect;

      GroupLayoutPolicyItem = EnumGroupPolicy.LayoutGroups;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new OrthogonalLayout();
      if (GroupLayoutPolicyItem == EnumGroupPolicy.FixGroups) {
        var rgl = new RecursiveGroupLayout { InterEdgeRouter = CreateInterEdgeRouter() };
        layout.LayoutStages.Prepend(rgl);
      } else if (GroupLayoutPolicyItem == EnumGroupPolicy.IgnoreGroups) {
        layout.LayoutStages.Get<GroupHidingStage>().Enabled = true;
      }

      layout.LayoutMode = ModeItem;
      layout.GridSpacing = GridSpacingItem;
      layout.QualityTimeRatio = QualityTimeRatioItem;
      layout.UniformPortAssignment = UniformPortAssignmentItem;
      layout.FromSketchMode = UseExistingDrawingAsSketchItem;
      layout.DefaultEdgeDescriptor.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
      layout.DefaultEdgeDescriptor.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;
      layout.DefaultEdgeDescriptor.MinimumSegmentLength = MinimumSegmentLengthItem;

      // set edge labeling options
      var isStrictMode = layout.LayoutMode == OrthogonalLayoutMode.Strict;
      if (EdgeLabelingItem == EdgeLabelPlacement.Integrated && isStrictMode) {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Integrated;
      } else if (EdgeLabelingItem == EdgeLabelPlacement.Generic) {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Generic;

        if (ReduceAmbiguityItem) {
          var labeling = layout.LayoutStages.Get<GenericLabeling>();
          labeling.DefaultNodeLabelingCosts.AmbiguousPlacementCost = 1.0;
          labeling.DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
        }
      } else {
        layout.EdgeLabelPlacement = EdgeLabelPlacement.Ignore;
      }

      if (isStrictMode) {
        layout.NodeLabelPlacement = NodeLabelingItem;
      } else {
        layout.NodeLabelPlacement = NodeLabelPlacement.Ignore;
      }

      layout.ChainSubstructureStyle = ChainSubstructureStyleItem;
      layout.ChainSubstructureSize = ChainSubstructureSizeItem;
      layout.CycleSubstructureStyle = CycleSubstructureStyleItem;
      layout.CycleSubstructureSize = CycleSubstructureSizeItem;
      layout.TreeSubstructureStyle = TreeSubstructureStyleItem;
      layout.TreeSubstructureSize = TreeSubstructureSizeItem;
      layout.TreeSubstructureOrientation = TreeSubstructureOrientationItem;

      return layout;
    }

    private ILayoutAlgorithm CreateInterEdgeRouter() =>
        new EdgeRouter {
            DefaultEdgeDescriptor = { MinimumEdgeDistance = 4 },
            MinimumNodeToEdgeDistance = 0
        };

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var orthogonalLayoutData = new OrthogonalLayoutData<INode, IEdge, ILabel, ILabel>();
      if (ConsiderEdgeDirectionItem) {
        orthogonalLayoutData.EdgeOrientation.MapperFunction =
            edge => Enumerable.Contains(graphControl.Selection.Edges, edge) ? 1 : 0;
      } else {
        orthogonalLayoutData.EdgeOrientation.Constant = 0;
      }

      var recursiveGroupLayoutData = new RecursiveGroupLayoutData<INode, IEdge, ILabel, ILabel> {
          GroupNodeLayouts = { Constant = RecursiveGroupLayout.FixGroupLayout }
      };

      var labelingLayoutData = CreateLabelingLayoutData(graphControl.Graph, LabelPlacementAlongEdgeItem,
          LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem);

      return orthogonalLayoutData.CombineWith(recursiveGroupLayoutData).CombineWith(labelingLayoutData);
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

    [Label("Edges")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgesGroup;

    [Label("Grouping")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GroupingGroup;

    [Label("Substructure Layout")]
    [OptionGroup("RootGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object SubstructureLayoutGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    public enum EnumGroupPolicy
    {
      LayoutGroups,
      FixGroups,
      IgnoreGroups
    }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>The orthogonal layout style is a multi-purpose layout style for undirected graphs. " +
        "It is well suited for medium-sized sparse graphs, and produces compact drawings with no overlaps, " +
        "few crossings, and few bends.</Paragraph>" +
        "<Paragraph>It is especially fitted for application domains such as</Paragraph>" + "<List>" +
        "<ListItem><Paragraph>Software engineering</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Database schema</Paragraph></ListItem>" +
        "<ListItem><Paragraph>System management</Paragraph></ListItem>" +
        "<ListItem><Paragraph>Knowledge representation</Paragraph></ListItem>" + "</List>";

    [Label("Layout Mode")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(OrthogonalLayoutMode.Strict)]
    [EnumValue("Strict", OrthogonalLayoutMode.Strict)]
    [EnumValue("Relaxed", OrthogonalLayoutMode.Relaxed)]
    [EnumValue("Forced Straight-Line", OrthogonalLayoutMode.ForcedStraightLine)]
    public OrthogonalLayoutMode ModeItem { get; set; }

    public bool ShouldDisableModeItem => UseExistingDrawingAsSketchItem;

    [Label("Grid Spacing")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(15)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int GridSpacingItem { get; set; }

    [Label("Quality")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(0.6d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double QualityTimeRatioItem { get; set; }

    [Label("Use Drawing as Sketch")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(false)]
    public bool UseExistingDrawingAsSketchItem { get; set; }

    [Label("Uniform Port Assignment")]
    [OptionGroup("GeneralGroup", 50)]
    public bool UniformPortAssignmentItem { get; set; }

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(NodeLabelPlacement.Consider)]
    [EnumValue("Consider", NodeLabelPlacement.Consider)]
    [EnumValue("Generic", NodeLabelPlacement.Generic)]
    [EnumValue("Ignore", NodeLabelPlacement.Ignore)]
    public NodeLabelPlacement NodeLabelingItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EdgeLabelPlacement.Integrated)]
    [EnumValue("Ignore", EdgeLabelPlacement.Ignore)]
    [EnumValue("Integrated", EdgeLabelPlacement.Integrated)]
    [EnumValue("Generic", EdgeLabelPlacement.Generic)]
    public EdgeLabelPlacement EdgeLabelingItem { get; set; }

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

    public bool ShouldDisableLabelPlacementOrientationItem => EdgeLabelingItem == EdgeLabelPlacement.Ignore;

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

    public bool ShouldDisableLabelPlacementAlongEdgeItem => EdgeLabelingItem == EdgeLabelPlacement.Ignore;

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
        EdgeLabelingItem == EdgeLabelPlacement.Ignore ||
        LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge;

    [Label("Minimum First Segment Length")]
    [OptionGroup("EdgesGroup", 10)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumFirstSegmentLengthItem { get; set; }

    [Label("Minimum Segment Length")]
    [OptionGroup("EdgesGroup", 20)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumSegmentLengthItem { get; set; }

    [Label("Minimum Last Segment Length")]
    [OptionGroup("EdgesGroup", 30)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLastSegmentLengthItem { get; set; }

    [Label("Route selected Edges Downwards")]
    [OptionGroup("EdgesGroup", 40)]
    [DefaultValue(false)]
    public bool ConsiderEdgeDirectionItem { get; set; }

    [Label("Group Layout Policy")]
    [OptionGroup("GroupingGroup", 10)]
    [DefaultValue(EnumGroupPolicy.LayoutGroups)]
    [EnumValue("Layout Groups", EnumGroupPolicy.LayoutGroups)]
    [EnumValue("Fix Contents of Groups", EnumGroupPolicy.FixGroups)]
    [EnumValue("Ignore Groups", EnumGroupPolicy.IgnoreGroups)]
    public EnumGroupPolicy GroupLayoutPolicyItem { get; set; }

    [Label("Cycles")]
    [OptionGroup("SubstructureLayoutGroup", 10)]
    [EnumValue("Ignore", CycleSubstructureStyle.None)]
    [EnumValue("Circular with Nodes at Corners", CycleSubstructureStyle.CircularWithNodesAtCorners)]
    [EnumValue("Circular with Bends at Corners", CycleSubstructureStyle.CircularWithBendsAtCorners)]
    public CycleSubstructureStyle CycleSubstructureStyleItem { get; set; }

    [Label("Minimum Cycle Size")]
    [OptionGroup("SubstructureLayoutGroup", 20)]
    [MinMax(Min = 4, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int CycleSubstructureSizeItem { get; set; }

    public bool ShouldDisableCycleSubstructureSizeItem => CycleSubstructureStyleItem == CycleSubstructureStyle.None;

    [Label("Chains")]
    [OptionGroup("SubstructureLayoutGroup", 30)]
    [EnumValue("Ignore", ChainSubstructureStyle.None)]
    [EnumValue("Straight", ChainSubstructureStyle.Straight)]
    [EnumValue("Wrapped with Nodes at Turns", ChainSubstructureStyle.WrappedWithNodesAtTurns)]
    [EnumValue("Wrapped with Bends at Turns", ChainSubstructureStyle.WrappedWithBendsAtTurns)]
    public ChainSubstructureStyle ChainSubstructureStyleItem { get; set; }

    [Label("Minimum Chain Length")]
    [OptionGroup("SubstructureLayoutGroup", 40)]
    [MinMax(Min = 2, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int ChainSubstructureSizeItem { get; set; }

    public bool ShouldDisableChainSubstructureSizeItem => ChainSubstructureStyleItem == ChainSubstructureStyle.None;

    [Label("Tree Style")]
    [OptionGroup("SubstructureLayoutGroup", 50)]
    [EnumValue("Ignore", TreeSubstructureStyle.None)]
    [EnumValue("Default", TreeSubstructureStyle.Default)]
    [EnumValue("Integrated", TreeSubstructureStyle.Integrated)]
    [EnumValue("Compact", TreeSubstructureStyle.Compact)]
    [EnumValue("Aspect Ratio", TreeSubstructureStyle.AspectRatio)]
    public TreeSubstructureStyle TreeSubstructureStyleItem { get; set; }

    [Label("Minimum Tree Size")]
    [OptionGroup("SubstructureLayoutGroup", 60)]
    [MinMax(Min = 3, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int TreeSubstructureSizeItem { get; set; }

    public bool ShouldDisableTreeSubstructureSizeItem => TreeSubstructureStyleItem == TreeSubstructureStyle.None;

    [Label("Tree Orientation")]
    [OptionGroup("SubstructureLayoutGroup", 70)]
    [EnumValue("Automatic", SubstructureOrientation.AutoDetect)]
    [EnumValue("Top to Bottom", SubstructureOrientation.TopToBottom)]
    [EnumValue("Bottom to Top", SubstructureOrientation.BottomToTop)]
    [EnumValue("Left to Right", SubstructureOrientation.LeftToRight)]
    [EnumValue("Right to Left", SubstructureOrientation.RightToLeft)]
    public SubstructureOrientation TreeSubstructureOrientationItem { get; set; }

    public bool ShouldDisableTreeSubstructureOrientationItem => TreeSubstructureStyleItem == TreeSubstructureStyle.None;

    public void EnableSubstructures() {
      ChainSubstructureStyleItem = ChainSubstructureStyle.WrappedWithNodesAtTurns;
      ChainSubstructureSizeItem = 2;
      CycleSubstructureStyleItem = CycleSubstructureStyle.CircularWithBendsAtCorners;
      CycleSubstructureSizeItem = 4;
      TreeSubstructureStyleItem = TreeSubstructureStyle.Integrated;
      TreeSubstructureSizeItem = 3;
      TreeSubstructureOrientationItem = SubstructureOrientation.AutoDetect;
    }
  }
}
