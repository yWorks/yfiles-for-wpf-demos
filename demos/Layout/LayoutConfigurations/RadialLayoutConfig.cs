/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Radial;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("RadialLayout")]
  public class RadialLayoutConfig : LayoutConfiguration
  {
    private const int MaximumSmoothness = 10;
    private const int MinimumSmoothness = 1;
    private const int SmoothnessAngleFactor = 4;

    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public RadialLayoutConfig() {
      var layout = new RadialLayout();

      CenterStrategyItem = CenterNodesPolicy.WeightedCentrality;
      LayeringStrategyItem = LayeringStrategy.Bfs;
      MinimumLayerDistanceItem = (int) layout.MinimumLayerDistance;
      MinimumNodeToNodeDistanceItem = (int) layout.MinimumNodeToNodeDistance;
      MaximumChildSectorSizeItem = (int) layout.MaximumChildSectorAngle;
      EdgeRoutingStrategyItem = EdgeRoutingStyle.Arc;
      EdgeSmoothnessItem = (int) Math.Min(MaximumSmoothness,
          (1 + MaximumSmoothness * SmoothnessAngleFactor - layout.MinimumBendAngle) / SmoothnessAngleFactor);
      EdgeBundlingStrengthItem = 0.95;

      EdgeLabelingItem = false;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
      NodeLabelingStyleItem = EnumNodeLabelingPolicies.ConsiderCurrentPosition;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new RadialLayout();
      layout.MinimumNodeToNodeDistance = MinimumNodeToNodeDistanceItem;
      if (EdgeRoutingStrategyItem != EdgeRoutingStyle.Bundled) {
        layout.EdgeRoutingStrategy = (EdgeRoutingStrategy) EdgeRoutingStrategyItem;
      }

      var minimumBendAngle = 1 + (MaximumSmoothness - EdgeSmoothnessItem) * SmoothnessAngleFactor;
      layout.MinimumBendAngle = minimumBendAngle;
      layout.MinimumLayerDistance = MinimumLayerDistanceItem;
      layout.MaximumChildSectorAngle = MaximumChildSectorSizeItem;
      layout.CenterNodesPolicy = CenterStrategyItem;
      layout.LayeringStrategy = LayeringStrategyItem;

      var ebc = layout.EdgeBundling;
      ebc.BundlingStrength = EdgeBundlingStrengthItem;
      ebc.DefaultBundleDescriptor = new EdgeBundleDescriptor {
          Bundled = EdgeRoutingStrategyItem == EdgeRoutingStyle.Bundled
      };

      if (EdgeLabelingItem) {
        var labeling = new GenericLabeling {
            PlaceEdgeLabels = true, PlaceNodeLabels = false, ReduceAmbiguity = ReduceAmbiguityItem
        };
        layout.LabelingEnabled = true;
        layout.Labeling = labeling;
      }

      switch (NodeLabelingStyleItem) {
        case EnumNodeLabelingPolicies.None:
          layout.ConsiderNodeLabels = false;
          break;
        case EnumNodeLabelingPolicies.RaylikeLeaves:
          layout.IntegratedNodeLabeling = true;
          layout.NodeLabelingPolicy = NodeLabelingPolicy.RayLikeLeaves;
          break;
        case EnumNodeLabelingPolicies.Raylike:
          layout.IntegratedNodeLabeling = true;
          layout.NodeLabelingPolicy = NodeLabelingPolicy.RayLike;
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

      if (this.NodeLabelingStyleItem == EnumNodeLabelingPolicies.RaylikeLeaves ||
          NodeLabelingStyleItem == EnumNodeLabelingPolicies.Raylike ||
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

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new RadialLayoutData();

      if (CenterStrategyItem == CenterNodesPolicy.Custom) {
        layoutData.CenterNodes.Source = graphControl.Selection.SelectedNodes;
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
        return
            "<Paragraph>The radial layout arranges the nodes of a graph on concentric circles. Similar to hierarchic layouts, "
            + "the overall flow of the graph is nicely visualized.</Paragraph>"
            + "<Paragraph>This style is well suited for the visualization of directed graphs and tree-like structures.</Paragraph>";
      }
    }

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
    public int MinimumNodeToNodeDistanceItem { get; set; }

    [Label("Maximum Child Sector Size")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(180)]
    [MinMax(Min = 15, Max = 360)]
    [ComponentType(ComponentTypes.Slider)]
    public int MaximumChildSectorSizeItem { get; set; }

    [Label("Routing Style")]
    [OptionGroup("GeneralGroup", 40)]
    [DefaultValue(EdgeRoutingStyle.Arc)]
    [EnumValue("Straight", EdgeRoutingStyle.Polyline)]
    [EnumValue("Arc", EdgeRoutingStyle.Arc)]
    [EnumValue("Curved", EdgeRoutingStyle.Curved)]
    [EnumValue("Radial Polyline", EdgeRoutingStyle.RadialPolyline)]
    [EnumValue("Bundled", EdgeRoutingStyle.Bundled)]
    public EdgeRoutingStyle EdgeRoutingStrategyItem { get; set; }

    [Label("Arc Smoothness")]
    [OptionGroup("GeneralGroup", 50)]
    [DefaultValue(9)]
    [MinMax(Min = MinimumSmoothness, Max = MaximumSmoothness)]
    [ComponentType(ComponentTypes.Slider)]
    public int EdgeSmoothnessItem { get; set; }

    public bool ShouldDisableEdgeSmoothnessItem {
      get { return EdgeRoutingStrategyItem != EdgeRoutingStyle.Arc; }
    }

    [Label("Bundling Strength")]
    [OptionGroup("GeneralGroup", 55)]
    [DefaultValue(0.95d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double EdgeBundlingStrengthItem { get; set; }

    public bool ShouldDisableEdgeBundlingStrengthItem {
      get { return EdgeRoutingStrategyItem != EdgeRoutingStyle.Bundled; }
    }

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
    [DefaultValue(LayeringStrategy.Bfs)]
    [EnumValue("Distance From Center", LayeringStrategy.Bfs)]
    [EnumValue("Hierarchic", LayeringStrategy.Hierarchical)]
    [EnumValue("Dendrogram", LayeringStrategy.Dendrogram)]
    public LayeringStrategy LayeringStrategyItem { get; set; }

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Ignore Labels", EnumNodeLabelingPolicies.None)]
    [EnumValue("Consider Labels", EnumNodeLabelingPolicies.ConsiderCurrentPosition)]
    [EnumValue("Horizontal", EnumNodeLabelingPolicies.Horizontal)]
    [EnumValue("Ray-like at Leaves", EnumNodeLabelingPolicies.RaylikeLeaves)]
    [EnumValue("Ray-like", EnumNodeLabelingPolicies.Raylike)]
    public EnumNodeLabelingPolicies NodeLabelingStyleItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool EdgeLabelingItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    [DefaultValue(false)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem {
      get { return !EdgeLabelingItem; }
    }

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem {
      get { return !EdgeLabelingItem; }
    }

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

    public bool ShouldDisableLabelPlacementAlongEdgeItem {
      get { return !EdgeLabelingItem; }
    }

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
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

    public enum EdgeRoutingStyle
    {
      Polyline = 1, Arc = 5, RadialPolyline = 6, Curved = 7, Bundled
    }
  }
}
