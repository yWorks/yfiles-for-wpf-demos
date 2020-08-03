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
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Labeling;
using yWorks.Layout.Orthogonal;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("OrthogonalLayout")]
  public class OrthogonalLayoutConfig : LayoutConfiguration
  {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public OrthogonalLayoutConfig() {
      StyleItem = LayoutStyle.Normal;
      GridSpacingItem = 15;
      EdgeLengthReductionItem = true;
      UseExistingDrawingAsSketchItem = false;
      CrossingReductionItem = true;
      PerceivedBendsPostprocessingItem = true;
      UseRandomizationItem = true;
      UseFaceMaximizationItem = false;

      ConsiderNodeLabelsItem = false;
      EdgeLabelingItem = EnumEdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10.0d;

      MinimumFirstSegmentLengthItem = 15.0d;
      MinimumSegmentLengthItem = 15.0d;
      MinimumLastSegmentLengthItem = 15.0d;
      ConsiderEdgeDirectionItem = false;

      ChainSubstructureStyleItem = ChainLayoutStyle.None;
      ChainSubstructureSizeItem = 2;
      CycleSubstructureStyleItem = CycleLayoutStyle.None;
      CycleSubstructureSizeItem = 4;
      TreeSubstructureStyleItem = TreeLayoutStyle.None;
      TreeSubstructureSizeItem = 3;
      TreeSubstructureOrientationItem = SubstructureOrientation.AutoDetect;

      GroupLayoutPolicyItem = EnumGroupPolicy.LayoutGroups;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new OrthogonalLayout();
      if (GroupLayoutPolicyItem == EnumGroupPolicy.FixGroups) {
        var fgl = new FixGroupLayoutStage { InterEdgeRoutingStyle = InterEdgeRoutingStyle.Orthogonal };
        layout.PrependStage(fgl);
      } else if (GroupLayoutPolicyItem == EnumGroupPolicy.IgnoreGroups) {
        layout.HideGroupsStageEnabled = true;
      }

      layout.LayoutStyle = StyleItem;
      layout.GridSpacing = GridSpacingItem;
      layout.EdgeLengthReduction = EdgeLengthReductionItem;
      layout.OptimizePerceivedBends = PerceivedBendsPostprocessingItem;
      layout.UniformPortAssignment = UniformPortAssignmentItem;
      layout.CrossingReduction = CrossingReductionItem;
      layout.Randomization = UseRandomizationItem;
      layout.FaceMaximization = UseFaceMaximizationItem;
      layout.FromSketchMode = UseExistingDrawingAsSketchItem;
      layout.EdgeLayoutDescriptor.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
      layout.EdgeLayoutDescriptor.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;
      layout.EdgeLayoutDescriptor.MinimumSegmentLength = MinimumSegmentLengthItem;

      //set edge labeling options
      var normalStyle = layout.LayoutStyle == LayoutStyle.Normal;
      layout.IntegratedEdgeLabeling = EdgeLabelingItem == EnumEdgeLabeling.Integrated && normalStyle;
      layout.ConsiderNodeLabels = ConsiderNodeLabelsItem && normalStyle;

      if (EdgeLabelingItem == EnumEdgeLabeling.Generic ||
          (EdgeLabelingItem == EnumEdgeLabeling.Integrated && normalStyle)) {
        layout.LabelingEnabled = true;
        if (EdgeLabelingItem == EnumEdgeLabeling.Generic) {
          ((GenericLabeling) layout.Labeling).ReduceAmbiguity = ReduceAmbiguityItem;
        }
      } else if (!ConsiderNodeLabelsItem || !normalStyle) {
        layout.LabelingEnabled = false;
      }

      layout.ChainStyle = ChainSubstructureStyleItem;
      layout.ChainSize = ChainSubstructureSizeItem;
      layout.CycleStyle = CycleSubstructureStyleItem;
      layout.CycleSize = CycleSubstructureSizeItem;
      layout.TreeStyle = TreeSubstructureStyleItem;
      layout.TreeSize = TreeSubstructureSizeItem;
      layout.TreeOrientation = TreeSubstructureOrientationItem;

      AddPreferredPlacementDescriptor(graphControl.Graph, LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem);

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new OrthogonalLayoutData();
      if (ConsiderEdgeDirectionItem) {
        layoutData.DirectedEdges.Source = graphControl.Selection.SelectedEdges;
      } else {
        layoutData.DirectedEdges.Delegate = edge => false;
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

    public enum EnumEdgeLabeling {
      None, Integrated, Generic
    }

    public enum EnumGroupPolicy
    {
      LayoutGroups, FixGroups, IgnoreGroups
    }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>The orthogonal layout style is a multi-purpose layout style for undirected graphs. "
               + "It is well suited for medium-sized sparse graphs, and produces compact drawings with no overlaps, "
               + "few crossings, and few bends.</Paragraph>"
               + "<Paragraph>It is especially fitted for application domains such as</Paragraph>"
               + "<List>"
               + "<ListItem><Paragraph>Software engineering</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Database schema</Paragraph></ListItem>"
               + "<ListItem><Paragraph>System management</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Knowledge representation</Paragraph></ListItem>"
               + "</List>";
      }
    }

    [Label("Style")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(LayoutStyle.Normal)]
    [EnumValue("Normal", LayoutStyle.Normal)]
    [EnumValue("Uniform Node Sizes",LayoutStyle.Uniform)]
    [EnumValue("Node Boxes",LayoutStyle.Box)]
    [EnumValue("Mixed",LayoutStyle.Mixed)]
    [EnumValue("Node Boxes (Fixed Node Size)",LayoutStyle.FixedBox)]
    [EnumValue("Mixed (Fixed Node Size)",LayoutStyle.FixedMixed)]
    public LayoutStyle StyleItem { get; set; }

    public bool ShouldDisableStyleItem {
      get { return UseExistingDrawingAsSketchItem; }
    }

    [Label("Grid Spacing")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(15)]
    [MinMax(Min = 1, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int GridSpacingItem { get; set; }

    [Label("Edge Length Reduction")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(true)]
    public bool EdgeLengthReductionItem { get; set; }

    [Label("Use Drawing as Sketch")]
    [OptionGroup("LayoutGroup", 40)]
    [DefaultValue(false)]
    public bool UseExistingDrawingAsSketchItem { get; set; }

    [Label("Crossing Reduction")]
    [OptionGroup("LayoutGroup", 50)]
    [DefaultValue(true)]
    public bool CrossingReductionItem { get; set; }

    public bool ShouldDisableCrossingReductionItem {
      get { return UseExistingDrawingAsSketchItem; }
    }

    [Label("Minimize Perceived Bends")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(true)]
    public bool PerceivedBendsPostprocessingItem { get; set; }

    public bool ShouldDisablePerceivedBendsPostprocessingItem {
      get { return UseExistingDrawingAsSketchItem; }
    }

    [Label("Uniform Port Assignment")]
    [OptionGroup("LayoutGroup", 65)]
    public bool UniformPortAssignmentItem { get; set; }

    [Label("Randomization")]
    [OptionGroup("LayoutGroup", 70)]
    [DefaultValue(true)]
    public bool UseRandomizationItem { get; set; }

    public bool ShouldDisableUseRandomizationItem {
      get { return UseExistingDrawingAsSketchItem; }
    }

    [Label("Face Maximization")]
    [OptionGroup("LayoutGroup", 80)]
    [DefaultValue(false)]
    public bool UseFaceMaximizationItem { get; set; }

    [Label("Consider Node Labels")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool ConsiderNodeLabelsItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EnumEdgeLabeling.None)]
    [EnumValue("None", EnumEdgeLabeling.None)]
    [EnumValue("Integrated",EnumEdgeLabeling.Integrated)]
    [EnumValue("Generic",EnumEdgeLabeling.Generic)]
    public EnumEdgeLabeling EdgeLabelingItem { get; set; }

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
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
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
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
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
      get { return EdgeLabelingItem == EnumEdgeLabeling.None || LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }

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
    [EnumValue("Fix Contents of Groups",EnumGroupPolicy.FixGroups)]
    [EnumValue("Ignore Groups",EnumGroupPolicy.IgnoreGroups)]
    public EnumGroupPolicy GroupLayoutPolicyItem { get; set; }

    [Label("Cycles")]
    [OptionGroup("SubstructureLayoutGroup", 10)]
    [EnumValue("Ignore", CycleLayoutStyle.None)]
    [EnumValue("Circular with Nodes at Corners", CycleLayoutStyle.CircularWithNodesAtCorners)]
    [EnumValue("Circular with Bends at Corners", CycleLayoutStyle.CircularWithBendsAtCorners)]
    public CycleLayoutStyle CycleSubstructureStyleItem { get; set; }

    [Label("Minimum Cycle Size")]
    [OptionGroup("SubstructureLayoutGroup", 20)]
    [MinMax(Min = 4, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int CycleSubstructureSizeItem { get; set; }

    public bool ShouldDisableCycleSubstructureSizeItem {
      get { return CycleSubstructureStyleItem == CycleLayoutStyle.None; }
    }

    [Label("Chains")]
    [OptionGroup("SubstructureLayoutGroup", 30)]
    [EnumValue("Ignore", ChainLayoutStyle.None)]
    [EnumValue("Straight", ChainLayoutStyle.Straight)]
    [EnumValue("Wrapped with Nodes at Turns", ChainLayoutStyle.WrappedWithNodesAtTurns)]
    [EnumValue("Wrapped with Bends at Turns", ChainLayoutStyle.WrappedWithBendsAtTurns)]
    public ChainLayoutStyle ChainSubstructureStyleItem { get; set; }

    [Label("Minimum Chain Length")]
    [OptionGroup("SubstructureLayoutGroup", 40)]
    [MinMax(Min = 2, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int ChainSubstructureSizeItem { get; set; }

    public bool ShouldDisableChainSubstructureSizeItem {
      get { return ChainSubstructureStyleItem == ChainLayoutStyle.None; }
    }

    [Label("Tree Style")]
    [OptionGroup("SubstructureLayoutGroup", 50)]
    [EnumValue("Ignore", TreeLayoutStyle.None)]
    [EnumValue("Default", TreeLayoutStyle.Default)]
    [EnumValue("Integrated", TreeLayoutStyle.Integrated)]
    [EnumValue("Compact", TreeLayoutStyle.Compact)]
    [EnumValue("Aspect Ratio", TreeLayoutStyle.AspectRatio)]
    public TreeLayoutStyle TreeSubstructureStyleItem { get; set; }

    [Label("Minimum Tree Size")]
    [OptionGroup("SubstructureLayoutGroup", 60)]
    [MinMax(Min = 3, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int TreeSubstructureSizeItem { get; set; }

    public bool ShouldDisableTreeSubstructureSizeItem {
      get { return TreeSubstructureStyleItem == TreeLayoutStyle.None; }
    }

    [Label("Tree Orientation")]
    [OptionGroup("SubstructureLayoutGroup", 70)]
    [EnumValue("Automatic", SubstructureOrientation.AutoDetect)]
    [EnumValue("Top to Bottom", SubstructureOrientation.TopToBottom)]
    [EnumValue("Bottom to Top", SubstructureOrientation.BottomToTop)]
    [EnumValue("Left to Right", SubstructureOrientation.LeftToRight)]
    [EnumValue("Right to Left", SubstructureOrientation.RightToLeft)]
    public SubstructureOrientation TreeSubstructureOrientationItem { get; set; }

    public bool ShouldDisableTreeSubstructureOrientationItem {
      get { return TreeSubstructureStyleItem == TreeLayoutStyle.None; }
    }

    public void EnableSubstructures() {
      ChainSubstructureStyleItem = ChainLayoutStyle.WrappedWithNodesAtTurns;
      ChainSubstructureSizeItem = 2;
      CycleSubstructureStyleItem = CycleLayoutStyle.CircularWithBendsAtCorners;
      CycleSubstructureSizeItem = 4;
      TreeSubstructureStyleItem = TreeLayoutStyle.Integrated;
      TreeSubstructureSizeItem = 3;
      TreeSubstructureOrientationItem = SubstructureOrientation.AutoDetect;
    }
  }
}
