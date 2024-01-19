/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Algorithms.Geometry;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Labeling;
using yWorks.Layout.Organic;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("OrganicLayout")]
  public class OrganicLayoutConfig : LayoutConfiguration {    
    private ILayoutStage preStage;
    
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public OrganicLayoutConfig() {
      var layout = new OrganicLayout();
      ScopeItem = Scope.All;
      PreferredEdgeLengthItem = layout.PreferredEdgeLength;
      AllowNodeOverlapsItem = layout.NodeOverlapsAllowed;
      MinimumNodeDistanceItem = 10;
      AvoidNodeEdgeOverlapsItem = layout.NodeEdgeOverlapAvoided;
      CompactnessItem = layout.CompactnessFactor;
      OrientationItem = Orientation.None;
      ClusteringPolicyItem = layout.ClusteringPolicy;
      ClusteringQualityItem = layout.ClusteringQuality;

      RestrictOutputItem = EnumOutputRestrictions.None;
      RectCageUseViewItem = true;
      CageXItem = 0;
      CageYItem = 0;
      CageWidthItem = 1000;
      CageHeightItem = 1000;
      ArCageUseViewItem = true;
      CageRatioItem = 1;

      GroupLayoutPolicyItem = EnumGroupLayoutPolicy.LayoutGroups;

      QualityTimeRatioItem = layout.QualityTimeRatio;
      MaximumDurationItem = (int) (layout.MaximumDuration / 1000);
      ActivateDeterministicModeItem = layout.Deterministic;

      CycleSubstructureStyleItem = CycleSubstructureStyle.None;
      CycleSubstructureSizeItem = layout.CycleSubstructureSize;
      ChainSubstructureStyleItem = ChainSubstructureStyle.None;
      ChainSubstructureSizeItem = layout.ChainSubstructureSize;
      StarSubstructureStyleItem = StarSubstructureStyle.None;
      StarSubstructureSizeItem = layout.StarSubstructureSize;
      ParallelSubstructureStyleItem = ParallelSubstructureStyle.None;
      ParallelSubstructureSizeItem = layout.ParallelSubstructureSize;
      TreeSubstructureItem = TreeSubstructureStyle.None;
      TreeSubstructureSizeItem = layout.TreeSubstructureSize;
      GroupSubstructureScopeItem = GroupSubstructureScope.NoGroups;
      GroupSubstructureSizeItem = layout.GroupSubstructureSize;
      ClusterAsGroupSubstructureItem = false;

      ConsiderNodeLabelsItem = layout.ConsiderNodeLabels;
      EdgeLabelingItem = EdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new OrganicLayout();
      layout.PreferredEdgeLength = PreferredEdgeLengthItem;
      layout.ConsiderNodeLabels = ConsiderNodeLabelsItem;
      layout.NodeOverlapsAllowed = AllowNodeOverlapsItem;
      layout.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.Scope = ScopeItem;
      layout.CompactnessFactor = CompactnessItem;
      layout.ConsiderNodeSizes = true;
      layout.ClusteringPolicy = ClusteringPolicyItem;
      layout.ClusteringQuality = ClusteringQualityItem;
      layout.NodeEdgeOverlapAvoided = AvoidNodeEdgeOverlapsItem;
      layout.Deterministic = ActivateDeterministicModeItem;
      layout.MaximumDuration = 1000 * MaximumDurationItem;
      layout.QualityTimeRatio = QualityTimeRatioItem;

      var orientation = GetLayoutOrientation(OrientationItem);
      if (orientation != null) {
        layout.LayoutOrientation = orientation.Value;
      }

      if (EdgeLabelingItem != EdgeLabeling.None) {
        if (EdgeLabelingItem == EdgeLabeling.Generic) {
          layout.IntegratedEdgeLabeling = false;
  
          var genericLabeling = new GenericLabeling
          {
              PlaceEdgeLabels = true,
              PlaceNodeLabels = false,
              ReduceAmbiguity = ReduceAmbiguityItem
          };
          layout.LabelingEnabled = true;
          layout.Labeling = genericLabeling;
        } else if (EdgeLabelingItem == EdgeLabeling.Integrated) {
          layout.IntegratedEdgeLabeling = true;
        }
      } else {
        layout.IntegratedEdgeLabeling = false;
      }

      ((ComponentLayout)layout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;

      ConfigureOutputRestrictions(graphControl, layout);

      layout.ChainSubstructureStyle = ChainSubstructureStyleItem;
      layout.ChainSubstructureSize = ChainSubstructureSizeItem;
      layout.CycleSubstructureSize = CycleSubstructureSizeItem;
      layout.CycleSubstructureStyle = CycleSubstructureStyleItem;
      layout.StarSubstructureStyle = StarSubstructureStyleItem;
      layout.StarSubstructureSize = StarSubstructureSizeItem;
      layout.ParallelSubstructureStyle = ParallelSubstructureStyleItem;
      layout.ParallelSubstructureSize = ParallelSubstructureSizeItem;
      layout.TreeSubstructureStyle = TreeSubstructureItem;
      layout.TreeSubstructureSize = TreeSubstructureSizeItem;
      layout.GroupSubstructureScope = GroupSubstructureScopeItem;
      layout.GroupSubstructureSize = GroupSubstructureSizeItem;
      layout.ClusterAsGroupSubstructureAllowed = ClusterAsGroupSubstructureItem;

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new OrganicLayoutData();

      switch (GroupLayoutPolicyItem) {
        case EnumGroupLayoutPolicy.IgnoreGroups:
          preStage = new HideGroupsStage();
          ((MultiStageLayout) layout).PrependStage(preStage);
          break;
        case EnumGroupLayoutPolicy.LayoutGroups:
          //do nothing...
          break;
        case EnumGroupLayoutPolicy.FixGroupBounds:
          layoutData.GroupNodeModes.Delegate =
              node => graphControl.Graph.IsGroupNode(node)
                  ? GroupNodeMode.FixBounds
                  : GroupNodeMode.Normal;
          break;
        case EnumGroupLayoutPolicy.FixGroupContents:
          layoutData.GroupNodeModes.Delegate =
              node => graphControl.Graph.IsGroupNode(node)
                  ? GroupNodeMode.FixContents
                  : GroupNodeMode.Normal;
          break;
        default:
          preStage = new HideGroupsStage();
          ((MultiStageLayout) layout).PrependStage(preStage);
          break;
      }

      layoutData.AffectedNodes.Source = graphControl.Selection.SelectedNodes;

      if (EdgeDirectednessItem) {
        layoutData.EdgeDirectedness.Delegate = edge => {
          if (edge.Style is IArrowOwner && !Equals(((IArrowOwner) edge.Style).TargetArrow, Arrows.None)) {
            return 1;
          }
          return 0;
        };
      }
      
      if (UseEdgeGroupingItem) {
        layoutData.SourceGroupIds.Constant = "Group";
        layoutData.TargetGroupIds.Constant = "Group";
      }

      if (OrientationItem != Orientation.None) {
        layoutData.EdgeOrientations.Delegate = edge => {
          if (edge.Style is PolylineEdgeStyle pes &&
              pes.TargetArrow != null &&
              pes.TargetArrow != Arrows.None
             ) {
            return 1;
          }
          return 0;
        };
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

    public void EnableSubstructures() {
      CycleSubstructureStyleItem = CycleSubstructureStyle.Circular;
      ChainSubstructureStyleItem = ChainSubstructureStyle.StraightLine;
      StarSubstructureStyleItem = StarSubstructureStyle.Radial;
      ParallelSubstructureStyleItem = ParallelSubstructureStyle.StraightLine;
    }

    private void ConfigureOutputRestrictions(GraphControl graphControl, OrganicLayout layout) {
      var viewInfoIsAvailable = false;
      var visibleRect = GetVisibleRectangle(graphControl);
      double x = 0, y = 0, w = 0, h = 0;
      if (visibleRect != null) {
        viewInfoIsAvailable = true;
        x = visibleRect[0];
        y = visibleRect[1];
        w = visibleRect[2];
        h = visibleRect[3];
      }
      switch (RestrictOutputItem) {
        case EnumOutputRestrictions.None:
          layout.ComponentLayoutEnabled = true;
          layout.OutputRestriction = OutputRestriction.None;
          break;
        case EnumOutputRestrictions.OutputCage:
          if (!viewInfoIsAvailable || !RectCageUseViewItem) {
            x = CageXItem;
            y = CageYItem;
            w = CageWidthItem;
            h = CageHeightItem;
          }
          layout.OutputRestriction = OutputRestriction.CreateRectangularCageRestriction(x, y, w, h);
          layout.ComponentLayoutEnabled = false;
          break;
        case EnumOutputRestrictions.OutputAr:
          double ratio;
          if (viewInfoIsAvailable && ArCageUseViewItem) {
            ratio = w / h;
          }
          else {
            ratio = CageRatioItem;
          }
          layout.OutputRestriction = OutputRestriction.CreateAspectRatioRestriction(ratio);
          layout.ComponentLayoutEnabled = true;
          ((ComponentLayout)layout.ComponentLayout).PreferredSize = new YDimension(ratio * 100, 100);
          break;
        case EnumOutputRestrictions.OutputEllipticalCage:
          if (!viewInfoIsAvailable || !RectCageUseViewItem) {
            x = CageXItem;
            y = CageYItem;
            w = CageWidthItem;
            h = CageHeightItem;
          }
          layout.OutputRestriction = OutputRestriction.CreateEllipticalCageRestriction(x, y, w, h);
          layout.ComponentLayoutEnabled = false;
          break;
      }
    }

    private static double[] GetVisibleRectangle(GraphControl graphControl) {
      double[] visibleRect = new double[4];
      if (graphControl != null) {
        RectD viewPort = graphControl.Viewport;
        visibleRect[0] = viewPort.X;
        visibleRect[1] = viewPort.Y;
        visibleRect[2] = viewPort.Width;
        visibleRect[3] = viewPort.Height;
        return visibleRect;
      }
      return null;
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
    public object VisualGroup;

    [Label("Restrictions")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object RestrictionsGroup;    

    [Label("Bounds")]
    [OptionGroup("RestrictionsGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object CageGroup;

    [Label("Aspect Ratio")]
    [OptionGroup("RestrictionsGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object ARGroup;

    [Label("Grouping")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GroupingGroup;

    [Label("Algorithm")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object AlgorithmGroup;

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

    public enum EnumOutputRestrictions
    {
      None, OutputCage, OutputAr, OutputEllipticalCage  
    }

    public enum EnumGroupLayoutPolicy
    {
      LayoutGroups, FixGroupBounds, FixGroupContents, IgnoreGroups   
    }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>The organic layout style is based on the force-directed layout paradigm. This algorithm simulates physical forces "
               + "and rearranges the positions of the nodes in such a way that the sum of the forces emitted by the nodes and the edges "
               + "reaches a (local) minimum.</Paragraph>"
               + "<Paragraph>This style is well suited for the visualization of highly connected backbone regions with attached peripheral "
               + "ring or tree structures. In a diagram arranged by this algorithm, these regions of a network can be easily identified.</Paragraph>"
               + "<Paragraph>The organic layout style is a multi-purpose layout for undirected graphs. It produces clear representations of "
               + "complex networks and is especially fitted for application domains such as:</Paragraph>"
               + "<List>"
               + "<ListItem><Paragraph>Bioinformatics</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Enterprise networking</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Knowledge representation</Paragraph></ListItem>"
               + "<ListItem><Paragraph>System management</Paragraph></ListItem>"
               + "<ListItem><Paragraph>WWW visualization</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Mesh visualization</Paragraph></ListItem>"
               + "</List>";
      }
    }

    [Label("Scope")]
    [OptionGroup("VisualGroup", 10)]
    [DefaultValue(Scope.All)]
    [EnumValue("All", Scope.All)]
    [EnumValue("Selection and Connected Nodes",Scope.MainlySubset)]
    [EnumValue("Selection and Nearby Nodes",Scope.MainlySubsetGeometric)]
    [EnumValue("Selection",Scope.Subset)]
    public Scope ScopeItem { get; set; }

    [Label("Preferred Edge Length")]
    [OptionGroup("VisualGroup", 20)]
    [DefaultValue(40.0d)]
    [MinMax(Min = 5, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredEdgeLengthItem { get; set; }

    [Label("Allow Overlapping Nodes")]
    [OptionGroup("VisualGroup", 40)]
    [DefaultValue(false)]
    public bool AllowNodeOverlapsItem { get; set; }

    public bool ShouldDisableAllowNodeOverlapsItem {
      get { return ConsiderNodeLabelsItem; }
    }

    [Label("Minimum Node Distance")]
    [OptionGroup("VisualGroup", 30)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeDistanceItem { get; set; }

    public bool ShouldDisableMinimumNodeDistanceItem {
      get { return AllowNodeOverlapsItem && !ConsiderNodeLabelsItem; }
    }

    [Label("Avoid Node/Edge Overlaps")]
    [OptionGroup("VisualGroup", 60)]
    [DefaultValue(false)]
    public bool AvoidNodeEdgeOverlapsItem { get; set; }

    [Label("Compactness Factor")]
    [OptionGroup("VisualGroup", 70)]
    [DefaultValue(0.5d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.1d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CompactnessItem { get; set; }


    [Label("Orientation")]
    [OptionGroup("VisualGroup", 80)]
    [DefaultValue(null)]
    [EnumValue("None", Orientation.None)]
    [EnumValue("Top to Bottom", Orientation.TopToBottom)]
    [EnumValue("Left to Right", Orientation.LeftToRight)]
    [EnumValue("Bottom to Top", Orientation.BottomToTop)]
    [EnumValue("Right to Left", Orientation.RightToLeft)]
    public Orientation OrientationItem { get; set; }
    
    [Label("Clustering")]
    [OptionGroup("VisualGroup", 100)]
    [DefaultValue(ClusteringPolicy.None)]
    [EnumValue("None", ClusteringPolicy.None)]
    [EnumValue("Edge Betweenness",ClusteringPolicy.EdgeBetweenness)]
    [EnumValue("Label Propagation",ClusteringPolicy.LabelPropagation)]
    [EnumValue("Louvain Modularity",ClusteringPolicy.LouvainModularity)]
    public ClusteringPolicy ClusteringPolicyItem { get; set; }

    [Label("Edge Betweenness Clustering Quality")]
    [OptionGroup("VisualGroup", 110)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double ClusteringQualityItem { get; set; }

    public bool ShouldDisableClusteringQualityItem {
      get { return ClusteringPolicyItem != ClusteringPolicy.EdgeBetweenness; } 
    }

    [Label("Output Area")]
    [OptionGroup("RestrictionsGroup", 10)]
    [DefaultValue(EnumOutputRestrictions.None)]
    [EnumValue("Unrestricted", EnumOutputRestrictions.None)]
    [EnumValue("Rectangular",EnumOutputRestrictions.OutputCage)]
    [EnumValue("Aspect Ratio",EnumOutputRestrictions.OutputAr)]
    [EnumValue("Elliptical",EnumOutputRestrictions.OutputEllipticalCage)]
    public EnumOutputRestrictions RestrictOutputItem { get; set; }

    public bool ShouldDisableCageGroup {
      get {
        return RestrictOutputItem != EnumOutputRestrictions.OutputCage &&
               RestrictOutputItem != EnumOutputRestrictions.OutputEllipticalCage;
      }
    }

    [Label("Use Visible Area")]
    [OptionGroup("CageGroup", 10)]
    [DefaultValue(true)]
    public bool RectCageUseViewItem { get; set; }

    [Label("Top Left X")]
    [OptionGroup("CageGroup", 20)]
    [DefaultValue(0.0d)]
    [ComponentType(ComponentTypes.Spinner)]
    public double CageXItem { get; set; }

    public bool ShouldDisableCageXItem {
      get { return RectCageUseViewItem; }
    }

    [Label("Top Left Y")]
    [OptionGroup("CageGroup", 30)]
    [DefaultValue(0.0d)]
    [ComponentType(ComponentTypes.Spinner)]
    public double CageYItem { get; set; }

    public bool ShouldDisableCageYItem {
      get { return RectCageUseViewItem; }
    }

    [Label("Width")]
    [OptionGroup("CageGroup", 40)]
    [DefaultValue(1000.0d)]
    [MinMax(Min = 1, Max = 100000)]
    [ComponentType(ComponentTypes.Spinner)]
    public double CageWidthItem { get; set; }

    public bool ShouldDisableCageWidthItem {
      get { return RectCageUseViewItem; }
    }

    [Label("Height")]
    [OptionGroup("CageGroup", 50)]
    [DefaultValue(1000.0d)]
    [MinMax(Min = 1, Max = 100000)]
    [ComponentType(ComponentTypes.Spinner)]
    public double CageHeightItem { get; set; }

    public bool ShouldDisableCageHeightItem {
      get { return RectCageUseViewItem; }
    }

    [Label("Use Ratio of View")]
    [OptionGroup("ARGroup", 10)]
    [DefaultValue(true)]
    public bool ArCageUseViewItem { get; set; }

    [Label("Aspect Ratio")]
    [OptionGroup("ARGroup", 20)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0.2d, Max = 5.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CageRatioItem { get; set; }

    public bool ShouldDisableCageRatioItem {
      get { return ArCageUseViewItem; }
    }

    [Label("Group Layout Policy")]
    [OptionGroup("GroupingGroup", 10)]
    [DefaultValue(EnumGroupLayoutPolicy.LayoutGroups)]
    [EnumValue("Layout Groups", EnumGroupLayoutPolicy.LayoutGroups)]
    [EnumValue("Fix Bounds of Groups",EnumGroupLayoutPolicy.FixGroupBounds)]
    [EnumValue("Fix Contents of Groups",EnumGroupLayoutPolicy.FixGroupContents)]
    [EnumValue("Ignore Groups",EnumGroupLayoutPolicy.IgnoreGroups)]
    public EnumGroupLayoutPolicy GroupLayoutPolicyItem { get; set; }

    [Label("Quality")]
    [OptionGroup("AlgorithmGroup", 10)]
    [DefaultValue(0.6d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double QualityTimeRatioItem { get; set; }

    [Label("Maximum Duration (sec)")]
    [OptionGroup("AlgorithmGroup", 20)]
    [DefaultValue(30)]
    [MinMax(Min = 0, Max = 150)]
    [ComponentType(ComponentTypes.Slider)]
    public int MaximumDurationItem { get; set; }

    [Label("Deterministic Mode")]
    [OptionGroup("AlgorithmGroup", 30)]
    [DefaultValue(false)]
    public bool ActivateDeterministicModeItem { get; set; }

    [Label("Consider Node Labels")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool ConsiderNodeLabelsItem { get; set; }

    [Label("Cycles")]
    [OptionGroup("SubstructureLayoutGroup", 10)]
    [DefaultValue(CycleSubstructureStyle.None)]
    [EnumValue("Ignore", CycleSubstructureStyle.None)]
    [EnumValue("Circular", CycleSubstructureStyle.Circular)]
    [EnumValue("Circular, also within other structures", CycleSubstructureStyle.CircularNested)]
    public CycleSubstructureStyle CycleSubstructureStyleItem { get; set; }

    [Label("Minimum Cycle Size")]
    [OptionGroup("SubstructureLayoutGroup", 15)]
    [DefaultValue(4)]
    [MinMax(Min = 4, Max = 20)]
    [ComponentType(ComponentTypes.Spinner)]
    public int CycleSubstructureSizeItem { get; set; }
    
    public bool ShouldDisableCycleSubstructureSizeItem {
      get { return CycleSubstructureStyleItem == CycleSubstructureStyle.None; }
    }
    
    [Label("Chains")]
    [OptionGroup("SubstructureLayoutGroup", 20)]
    [DefaultValue(ChainSubstructureStyle.None)]
    [EnumValue("Ignore", ChainSubstructureStyle.None)]
    [EnumValue("Rectangular", ChainSubstructureStyle.Rectangular)]
    [EnumValue("Rectangular, also within other structures", ChainSubstructureStyle.RectangularNested)]
    [EnumValue("Straight-Line", ChainSubstructureStyle.StraightLine)]
    [EnumValue("Straight-Line, also within other structures", ChainSubstructureStyle.StraightLineNested)]
    [EnumValue("Disk", ChainSubstructureStyle.Disk)]
    [EnumValue("Disk, also within other structures", ChainSubstructureStyle.DiskNested)]
    public ChainSubstructureStyle ChainSubstructureStyleItem { get; set; }

    [Label("Minimum Chain Size")]
    [OptionGroup("SubstructureLayoutGroup", 25)]
    [DefaultValue(4)]
    [MinMax(Min = 4, Max = 20)]
    [ComponentType(ComponentTypes.Spinner)]
    public int ChainSubstructureSizeItem { get; set; }
    
    public bool ShouldDisableChainSubstructureSizeItem {
      get { return ChainSubstructureStyleItem == ChainSubstructureStyle.None; }
    }

    [Label("Stars")]
    [OptionGroup("SubstructureLayoutGroup", 30)]
    [DefaultValue(StarSubstructureStyle.None)]
    [EnumValue("Ignore", StarSubstructureStyle.None)]
    [EnumValue("Circular", StarSubstructureStyle.Circular)]
    [EnumValue("Circular, also within other structures", StarSubstructureStyle.CircularNested)]
    [EnumValue("Radial", StarSubstructureStyle.Radial)]
    [EnumValue("Radial, also within other structures", StarSubstructureStyle.RadialNested)]
    [EnumValue("Separated Radial", StarSubstructureStyle.SeparatedRadial)]
    public StarSubstructureStyle StarSubstructureStyleItem { get; set; }

    [Label("Minimum Star Size")]
    [OptionGroup("SubstructureLayoutGroup", 35)]
    [DefaultValue(4)]
    [MinMax(Min = 4, Max = 20)]
    [ComponentType(ComponentTypes.Spinner)]
    public int StarSubstructureSizeItem { get; set; }
    
    public bool ShouldDisableStarSubstructureSizeItem {
      get { return StarSubstructureStyleItem == StarSubstructureStyle.None; }
    }

    [Label("Parallel Structures")]
    [OptionGroup("SubstructureLayoutGroup", 40)]
    [DefaultValue(ParallelSubstructureStyle.None)]
    [EnumValue("Ignore", ParallelSubstructureStyle.None)]
    [EnumValue("Rectangular", ParallelSubstructureStyle.Rectangular)]
    [EnumValue("Radial", ParallelSubstructureStyle.Radial)]
    [EnumValue("Straight-Line", ParallelSubstructureStyle.StraightLine)]
    public ParallelSubstructureStyle ParallelSubstructureStyleItem { get; set; }

    [Label("Minimum size for parallel structures")]
    [OptionGroup("SubstructureLayoutGroup", 45)]
    [DefaultValue(3)]
    [MinMax(Min = 3, Max = 20)]
    [ComponentType(ComponentTypes.Spinner)]
    public int ParallelSubstructureSizeItem { get; set; }
    
    public bool ShouldDisableParallelSubstructureSizeItem {
      get { return ParallelSubstructureStyleItem == ParallelSubstructureStyle.None; }
    }

    [Label("Tree")]
    [OptionGroup("SubstructureLayoutGroup", 50)]
    [DefaultValue(TreeSubstructureStyle.None)]
    [EnumValue("Ignore", TreeSubstructureStyle.None)]
    [EnumValue("Radial", TreeSubstructureStyle.Radial)]
    [EnumValue("Balloon", TreeSubstructureStyle.Balloon)]
    [EnumValue("Oriented", TreeSubstructureStyle.Oriented)]
    public TreeSubstructureStyle TreeSubstructureItem { get; set; }

    
    [Label("Minimum size for tree structures")]
    [OptionGroup("SubstructureLayoutGroup", 55)]
    [DefaultValue(4)]
    [MinMax(Min = 3, Max = 20)]
    [ComponentType(ComponentTypes.Spinner)]
    public int TreeSubstructureSizeItem { get; set; }
    
    public bool ShouldDisableTreeSubstructureSizeItem {
      get { return TreeSubstructureItem == TreeSubstructureStyle.None; }
    }
    
    
    [Label("Group Substructures")]
    [OptionGroup("SubstructureLayoutGroup", 60)]
    [DefaultValue(GroupSubstructureScope.NoGroups)]
    [EnumValue("Ignore", GroupSubstructureScope.NoGroups)]
    [EnumValue("All Groups", GroupSubstructureScope.AllGroups)]
    [EnumValue("Groups Without Intra-Edges", GroupSubstructureScope.GroupsWithoutEdges)]
    [EnumValue("Groups Without Inter-Edges", GroupSubstructureScope.GroupsWithoutInterEdges)]
    public GroupSubstructureScope GroupSubstructureScopeItem { get; set; }

    
    [Label("Minimum size for group structures")]
    [OptionGroup("SubstructureLayoutGroup", 65)]
    [DefaultValue(4)]
    [MinMax(Min = 2, Max = 20)]
    [ComponentType(ComponentTypes.Spinner)]
    public int GroupSubstructureSizeItem { get; set; }
    
    public bool ShouldDisableGroupSubstructureSizeItem {
      get { return GroupSubstructureScopeItem == GroupSubstructureScope.NoGroups; }
    }
        
    [Label("Clusters With Group Substructures")]
    [OptionGroup("SubstructureLayoutGroup", 70)]
    [DefaultValue(false)]
    public bool ClusterAsGroupSubstructureItem { get; set; }

    public bool ShouldDisableClusterAsGroupSubstructureItem {
      get {
        return GroupSubstructureScopeItem != GroupSubstructureScope.AllGroups &&
                                             GroupSubstructureScopeItem !=
            GroupSubstructureScope.GroupsWithoutInterEdges;
      }
    }
    
    [Label("Arrows Define Edge Direction")]
    [OptionGroup("SubstructureLayoutGroup", 80)]
    public bool EdgeDirectednessItem { get; set; }

    [Label("Use Edge Grouping")]
    [OptionGroup("SubstructureLayoutGroup", 90)]
    public bool UseEdgeGroupingItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EdgeLabeling.None)]
    [EnumValue("None", EdgeLabeling.None)]
    [EnumValue("Integrated", EdgeLabeling.Integrated)]
    [EnumValue("Generic", EdgeLabeling.Generic)]
    public EdgeLabeling EdgeLabelingItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem {
      get { return EdgeLabelingItem != EdgeLabeling.Generic; }
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
      get { return EdgeLabelingItem == EdgeLabeling.None; }
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
      get { return EdgeLabelingItem == EdgeLabeling.None; }
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
      get { return EdgeLabelingItem == EdgeLabeling.None; }
    }

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem {
      get { return EdgeLabelingItem == EdgeLabeling.None || LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }

    private LayoutOrientation? GetLayoutOrientation(Orientation orientation) {
      switch (orientation) {
        case Orientation.None:
          return null;
        case Orientation.TopToBottom:
          return LayoutOrientation.TopToBottom;
        case Orientation.BottomToTop:
          return LayoutOrientation.BottomToTop;
        case Orientation.LeftToRight:
          return LayoutOrientation.LeftToRight;
        case Orientation.RightToLeft:
          return LayoutOrientation.RightToLeft;
      }
      return null;
    }
    public enum Orientation
    {
      None, TopToBottom, LeftToRight, BottomToTop, RightToLeft
    }
  }
}
