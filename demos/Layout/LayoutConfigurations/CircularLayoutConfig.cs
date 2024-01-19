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
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Labeling;

namespace Demo.yFiles.Layout.Configurations 
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("CircularLayout")]
  public class CircularLayoutConfig : LayoutConfiguration
  {

    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public CircularLayoutConfig() {
      var layout = new CircularLayout();
      var treeLayout = layout.BalloonLayout;

      LayoutStyleItem = LayoutStyle.BccCompact;
      ActOnSelectionOnlyItem = false;
      FromSketchItem = false;

      PartitionStyleItem = PartitionStyle.Cycle;
      MinimumNodeDistanceItem = 30;
      ChooseRadiusAutomaticallyItem = true;
      FixedRadiusItem = 200;

      DefaultBetweenCirclesRoutingItem = RoutingStyle.Straight;
      DefaultInCircleRoutingStyleItem = RoutingStyle.Straight;
      DefaultOnCircleRoutingStyleItem = OnCircleRoutingStyle.Straight;
      
      EdgeRoutingPolicyItem = EdgeRoutingPolicy.Interior;
      CircleDistanceItem = 20;
      EdgeToEdgeDistanceItem = 10;
      PreferredCurveLengthItem = 20;
      PreferredAngleItem = 10;
      SmoothnessItem = 0.7;
      
      EdgeBundlingItem = false;
      EdgeBundlingStrengthItem = 0.95;

      PreferredChildWedgeItem = treeLayout.PreferredChildWedge;
      MinimumEdgeLengthItem = treeLayout.MinimumEdgeLength;
      MaximumDeviationAngleItem = layout.MaximumDeviationAngle;
      CompactnessFactorItem = treeLayout.CompactnessFactor;
      MinimumTreeNodeDistanceItem = treeLayout.MinimumNodeDistance;
      AllowOverlapsItem = treeLayout.AllowOverlaps;
      PlaceChildrenOnCommonRadiusItem = true;

      StarSubstructureItem = StarSubstructureStyle.None;
      StarSubstructureSizeItem = layout.StarSubstructureSize;

      EdgeLabelingItem = false;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;

      NodeLabelingStyleItem = EnumNodeLabelingPolicies.ConsiderCurrentPosition;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new CircularLayout();
      var balloonLayout = layout.BalloonLayout;

      layout.LayoutStyle = LayoutStyleItem;
      layout.EdgeRoutingPolicy = EdgeRoutingPolicyItem;
      layout.ExteriorEdgeLayoutDescriptor.CircleDistance = CircleDistanceItem;
      layout.ExteriorEdgeLayoutDescriptor.EdgeToEdgeDistance = EdgeToEdgeDistanceItem;
      layout.ExteriorEdgeLayoutDescriptor.PreferredAngle = PreferredAngleItem;
      layout.ExteriorEdgeLayoutDescriptor.PreferredCurveLength = PreferredCurveLengthItem;
      layout.ExteriorEdgeLayoutDescriptor.Smoothness = SmoothnessItem;
      layout.SubgraphLayoutEnabled = ActOnSelectionOnlyItem;
      layout.MaximumDeviationAngle = MaximumDeviationAngleItem;
      layout.FromSketchMode = FromSketchItem;

      layout.PartitionStyle = PartitionStyleItem;

      layout.SingleCycleLayout.MinimumNodeDistance = MinimumNodeDistanceItem;
      layout.SingleCycleLayout.AutomaticRadius = ChooseRadiusAutomaticallyItem;
      layout.SingleCycleLayout.FixedRadius = FixedRadiusItem;

      balloonLayout.PreferredChildWedge = PreferredChildWedgeItem;
      balloonLayout.MinimumEdgeLength = MinimumEdgeLengthItem;
      balloonLayout.CompactnessFactor = CompactnessFactorItem;
      balloonLayout.AllowOverlaps = AllowOverlapsItem;
      layout.PlaceChildrenOnCommonRadius = PlaceChildrenOnCommonRadiusItem;
      balloonLayout.MinimumNodeDistance = MinimumTreeNodeDistanceItem;

      if (EdgeLabelingItem) {
        var genericLabeling = new GenericLabeling();
        genericLabeling.PlaceEdgeLabels = true;
        genericLabeling.PlaceNodeLabels = false;
        genericLabeling.ReduceAmbiguity = ReduceAmbiguityItem;
        layout.LabelingEnabled = true;
        layout.Labeling = genericLabeling;
      }

      layout.DefaultEdgeLayoutDescriptor.BetweenCirclesRoutingStyle = DefaultBetweenCirclesRoutingItem;
      layout.DefaultEdgeLayoutDescriptor.InCircleRoutingStyle = DefaultInCircleRoutingStyleItem;
      layout.DefaultEdgeLayoutDescriptor.OnCircleRoutingStyle = DefaultOnCircleRoutingStyleItem;
      
      var ebc = layout.EdgeBundling;
      var bundlingDescriptor = new EdgeBundleDescriptor();
      bundlingDescriptor.Bundled = EdgeBundlingItem;
      ebc.BundlingStrength = EdgeBundlingStrengthItem;
      ebc.DefaultBundleDescriptor = bundlingDescriptor;

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

      if (NodeLabelingStyleItem == EnumNodeLabelingPolicies.RaylikeLeaves ||
          NodeLabelingStyleItem == EnumNodeLabelingPolicies.Horizontal) {
        foreach (var label in graphControl.Graph.Labels) {
          if (label.Owner is INode) {
            graphControl.Graph.SetLabelLayoutParameter(
                label,
                FreeNodeLabelModel.Instance.FindBestParameter(
                    label,
                    FreeNodeLabelModel.Instance,
                    label.GetLayout()
                )
            );
          }
        }
      }

      layout.StarSubstructureStyle = StarSubstructureItem;
      layout.StarSubstructureSize = StarSubstructureSizeItem;

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new CircularLayoutData();
      
      if (LayoutStyleItem == LayoutStyle.CustomGroups) {
        var graph = graphControl.Graph;
        layoutData.CustomGroups.Delegate = node => graph.GetParent(node);
      }

      if (EdgeRoutingPolicyItem == EdgeRoutingPolicy.MarkedExterior) {
        layoutData.ExteriorEdges.Source = graphControl.Selection.SelectedEdges;
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
    public string DescriptionText {
      get {
        return "<Paragraph>The circular layout style emphasizes group and tree structures within a network. It creates node partitions "
               + "by analyzing the connectivity structure of the network, and arranges the partitions as separate circles. The circles "
               + "themselves are arranged in a radial tree layout fashion.</Paragraph>"
               + "<Paragraph>This layout style portraits interconnected ring and star topologies and is excellent for "
               + "applications in:</Paragraph>"
               + "<List>"
               + "<ListItem><Paragraph>Social networking (criminology, economics, fraud detection, etc.)</Paragraph></ListItem>"
               + "<ListItem><Paragraph>Network management</Paragraph></ListItem>"
               + "<ListItem><Paragraph>WWW visualization</Paragraph></ListItem>"
               + "<ListItem><Paragraph>eCommerce</Paragraph></ListItem>"
               + "</List>";
      }
    }

    [Label("Layout Style")]
    [OptionGroup("GeneralGroup",10)]
    [DefaultValue(LayoutStyle.BccCompact)]
    [EnumValue("BCC Compact", LayoutStyle.BccCompact)]
    [EnumValue("BCC Isolated",LayoutStyle.BccIsolated)]
    [EnumValue("Custom Groups",LayoutStyle.CustomGroups)]
    [EnumValue("Single Cycle",LayoutStyle.SingleCycle)]
    public LayoutStyle LayoutStyleItem { get; set; }
    
    [Label("Act on Selection Only")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(false)]
    public bool ActOnSelectionOnlyItem { get; set; }
    
    [Label("Use Drawing as Sketch")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(false)]
    public bool FromSketchItem { get; set; }    
    
    [Label("Partition Layout Style")]
    [OptionGroup("CycleGroup", 10)]
    [DefaultValue(PartitionStyle.Cycle)]
    [EnumValue("Circle", PartitionStyle.Cycle)]
    [EnumValue("Disk",PartitionStyle.Disk)]
    [EnumValue("Compact Disk",PartitionStyle.CompactDisk)]
    [EnumValue("Organic",PartitionStyle.Organic)]
    public PartitionStyle PartitionStyleItem { get; set; }
    
    [Label("Minimum Node Distance")]
    [OptionGroup("CycleGroup", 20)]
    [DefaultValue(30)]
    [MinMax(Min = 0, Max = 999)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumNodeDistanceItem { get; set; }

    public bool ShouldDisableMinimumNodeDistanceItem {
      get { return ChooseRadiusAutomaticallyItem == false; }
    }
    
    [Label("Choose Radius Automatically")]
    [OptionGroup("CycleGroup", 30)]
    [DefaultValue(true)]
    public bool ChooseRadiusAutomaticallyItem { get; set; }
    
    [Label("Fixed Radius")]
    [OptionGroup("CycleGroup", 40)]
    [DefaultValue(200)]
    [MinMax(Min = 1, Max = 800)]
    [ComponentType(ComponentTypes.Slider)]
    public int FixedRadiusItem { get; set; }

    public bool ShouldDisableFixedRadiusItem {
      get { return ChooseRadiusAutomaticallyItem; }
    }

    [Label("Edge Routing")]
    [OptionGroup("EdgesGroup", 10)]
    [DefaultValue(EdgeRoutingPolicy.Interior)]
    [EnumValue("Inside", EdgeRoutingPolicy.Interior)]
    [EnumValue("Outside", EdgeRoutingPolicy.Exterior)]
    [EnumValue("Automatic", EdgeRoutingPolicy.Automatic)]
    [EnumValue("Selected edges outside", EdgeRoutingPolicy.MarkedExterior)]
    public EdgeRoutingPolicy EdgeRoutingPolicyItem { get; set; }


    [Label("Routing Style Between Circles")]
    [OptionGroup("DefaultEdgesGroup", 10)]
    [DefaultValue(RoutingStyle.Straight)]
    [EnumValue("Straight", RoutingStyle.Straight)]
    [EnumValue("Curved", RoutingStyle.Curved)]
    public RoutingStyle DefaultBetweenCirclesRoutingItem { get; set; }

    public bool ShouldDisableDefaultBetweenCirclesRoutingItem {
      get { return this.EdgeRoutingPolicyItem == EdgeRoutingPolicy.Exterior; }
    }

    [Label("Routing Style Within Partitions")]
    [OptionGroup("DefaultEdgesGroup", 20)]
    [DefaultValue(RoutingStyle.Straight)]
    [EnumValue("Straight", RoutingStyle.Straight)]
    [EnumValue("Curved", RoutingStyle.Curved)]
    public RoutingStyle DefaultInCircleRoutingStyleItem { get; set; }

    public bool ShouldDisableDefaultInCircleRoutingStyleItem {
      get { return this.EdgeRoutingPolicyItem == EdgeRoutingPolicy.Exterior; }
    }

    [Label("Routing Style Between Neighbors")]
    [OptionGroup("DefaultEdgesGroup", 30)]
    [DefaultValue(RoutingStyle.Straight)]
    [EnumValue("Straight", OnCircleRoutingStyle.Straight)]
    [EnumValue("Curved", OnCircleRoutingStyle.Curved)]
    [EnumValue("On Circle", OnCircleRoutingStyle.OnCircle)]
    public OnCircleRoutingStyle DefaultOnCircleRoutingStyleItem { get; set; }

    public bool ShouldDisableDefaultOnCircleRoutingStyleItem {
      get { return this.EdgeRoutingPolicyItem == EdgeRoutingPolicy.Exterior; }
    }

    [Label("Distance to circle")]
    [OptionGroup("ExteriorEdgesGroup", 10)]
    [DefaultValue(20d)]
    [MinMax(Min = 10.0d, Max = 100.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double CircleDistanceItem { get; set; }

    public bool ShouldDisableCircleDistanceItem {
      get { return EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior; }
    }

    [Label("Edge to edge distance")]
    [OptionGroup("ExteriorEdgesGroup", 20)]
    [DefaultValue(10d)]
    [MinMax(Min = 5.0d, Max = 50.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeToEdgeDistanceItem { get; set; }

    public bool ShouldDisableEdgeToEdgeDistanceItem {
      get { return EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior; }
    }

    [Label("Corner radius")]
    [OptionGroup("ExteriorEdgesGroup", 30)]
    [DefaultValue(20d)]
    [MinMax(Min = 0.0d, Max = 100.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredCurveLengthItem { get; set; }

    public bool ShouldDisablePreferredCurveLengthItem {
      get { return EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior; }
    }

    [Label("Angle")]
    [OptionGroup("ExteriorEdgesGroup", 40)]
    [DefaultValue(10d)]
    [MinMax(Min = 0.0d, Max = 45.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredAngleItem { get; set; }

    public bool ShouldDisablePreferredAngleItem {
      get { return EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior; }
    }

    [Label("Smoothness")]
    [OptionGroup("ExteriorEdgesGroup", 50)]
    [DefaultValue(0.7d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.1d)]
    [ComponentType(ComponentTypes.Slider)]
    public double SmoothnessItem { get; set; }

    public bool ShouldDisableSmoothnessItem {
      get { return EdgeRoutingPolicyItem == EdgeRoutingPolicy.Interior; }
    }

    [Label("Enable Edge Bundling")]
    [OptionGroup("EdgesGroup", 20)]
    [DefaultValue(false)]
    public bool EdgeBundlingItem { get; set; }

    public bool ShouldDisableEdgeBundlingItem {
      get { return PartitionStyleItem != PartitionStyle.Cycle || LayoutStyleItem == LayoutStyle.BccIsolated; }
    }

    [Label("Bundling Strength")]
    [OptionGroup("EdgesGroup", 50)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem {
      get { return PartitionStyleItem != PartitionStyle.Cycle || LayoutStyleItem == LayoutStyle.BccIsolated; }
    }
    
    [Label("Preferred Child Wedge")]
    [OptionGroup("TreeGroup", 10)]
    [DefaultValue(340)]
    [MinMax(Min = 1, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public int PreferredChildWedgeItem { get; set; }
    
    [Label("Minimum Edge Length")]
    [OptionGroup("TreeGroup", 20)]
    [DefaultValue(40)]
    [MinMax(Min = 5, Max = 400)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumEdgeLengthItem { get; set; }
    
    [Label("Maximum Deviation Angle")]
    [OptionGroup("TreeGroup", 30)]
    [DefaultValue(90)]
    [MinMax(Min = 1, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public int MaximumDeviationAngleItem { get; set; }
    
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
    public int MinimumTreeNodeDistanceItem { get; set; }

    [Label("Allow Overlaps")]
    [OptionGroup("TreeGroup", 60)]
    [DefaultValue(false)]
    public bool AllowOverlapsItem { get; set; }

    [Label("Place Children on Common Radius")]
    [OptionGroup("TreeGroup", 70)]
    [DefaultValue(true)]
    public bool PlaceChildrenOnCommonRadiusItem { get; set; }

    public bool ShouldDisableTreeGroupItem {
      get { return LayoutStyleItem == LayoutStyle.SingleCycle; }
    }

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

    public bool ShouldDisableStarSubstructureSizeItem {
      get { return StarSubstructureItem == StarSubstructureStyle.None; }
    }

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Ignore Labels", EnumNodeLabelingPolicies.None)]
    [EnumValue("Consider Labels", EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Horizontal", EnumNodeLabelingPolicies.Horizontal)]
    [EnumValue("Ray-like at Leaves", EnumNodeLabelingPolicies.RaylikeLeaves)]
    public EnumNodeLabelingPolicies NodeLabelingStyleItem { get; set; }
        
    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool EdgeLabelingItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem {
      get { return !EdgeLabelingItem; }
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
      get { return !EdgeLabelingItem; }
    }

    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source",EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Source Port",EnumLabelPlacementAlongEdge.AtSourcePort)]
    [EnumValue("At Target",EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("At Target Port",EnumLabelPlacementAlongEdge.AtTargetPort)]
    [EnumValue("Centered",EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementAlongEdgeItem {
      get { return !EdgeLabelingItem; }
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
      get { return !EdgeLabelingItem; }
    }

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem {
      get { return !EdgeLabelingItem || LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }
  }
}
