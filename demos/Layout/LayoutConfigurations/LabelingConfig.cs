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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Labeling;
using yWorks.Utils;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the <see cref="GenericLabeling"/> algorithm.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("Labeling")]
  public class LabelingConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public LabelingConfig() {
      PlaceNodeLabelsItem = true;
      PlaceEdgeLabelsItem = true;
      ConsiderSelectedFeaturesOnlyItem = false;

      OptimizationStrategyItem = LabelingOptimizationStrategy.Balanced;

      ReduceAmbiguityItem = true;

      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      if (!PlaceEdgeLabelsItem && !PlaceNodeLabelsItem) {
        return RecursiveGroupLayout.FixContentLayout;
      }
      var labeling = new GenericLabeling();
      var labelingPenalties = new LabelingCosts(OptimizationStrategyItem == null
          ? LabelingOptimizationStrategy.Balanced
          : OptimizationStrategyItem);
      labeling.DefaultEdgeLabelingCosts = labelingPenalties;
      labeling.DefaultNodeLabelingCosts = labelingPenalties;
      labeling.QualityTimeRatio = QualityTimeRatioItem;
      if (PlaceEdgeLabelsItem && PlaceNodeLabelsItem) {
        labeling.Scope = LabelingScope.All;
      } else {
        labeling.Scope = PlaceEdgeLabelsItem ? LabelingScope.EdgeLabels : LabelingScope.NodeLabels;
      }
      if (ReduceAmbiguityItem) {
        labeling.DefaultNodeLabelingCosts.AmbiguousPlacementCost = 1.0;
        labeling.DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
      }
      return labeling;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new GenericLabelingData<INode, IEdge, ILabel, ILabel>();

      var selection = graphControl.Selection;
      var selectionOnly = ConsiderSelectedFeaturesOnlyItem;
      if (selection != null && selectionOnly) {
        layoutData.Scope.NodeLabels.Predicate = label =>
            label.Owner is INode && (selection.Contains(label) || selection.Contains(label.Owner));
        layoutData.Scope.EdgeLabels.Predicate = label =>
            label.Owner is IEdge && (selection.Contains(label) || selection.Contains(label.Owner));
      }

      if (PlaceEdgeLabelsItem) {
        SetupEdgeLabelModels(graphControl);
        return layoutData.CombineWith(CreateLabelingLayoutData(graphControl.Graph, LabelPlacementAlongEdgeItem,
            LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem));
      }

      return layoutData;
    }


    private void SetupEdgeLabelModels(GraphControl graphControl) {
      var model = new FreeEdgeLabelModel();

      var selectionOnly = ConsiderSelectedFeaturesOnlyItem;
      var placeEdgeLabels = PlaceEdgeLabelsItem;
      if (!placeEdgeLabels) {
        return;
      }

      var graph = graphControl.Graph;
      foreach (var label in graph.GetEdgeLabels()) {
        var parameterFinder = model.GetContext(label).Lookup<ILabelModelParameterFinder>()!;
        if (selectionOnly) {
          if (graphControl.Selection.Contains(label)) {
            graph.SetLabelLayoutParameter(label, parameterFinder.FindBestParameter(label.GetLayout()));
          }
        } else {
          graph.SetLabelLayoutParameter(label, parameterFinder.FindBestParameter(label.GetLayout()));
        }
      }
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

    [Label("Quality")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object QualityGroup;

    [Label("Preferred Edge Label Placement")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object PreferredPlacementGroup;

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText =>
        "<Paragraph>This algorithm finds good positions for the labels of nodes and edges. " +
        "Typically, a label should be placed near the item it belongs to and it should not overlap with other labels. " +
        "Optionally, overlaps with nodes and edges can be avoided as well.</Paragraph>";

    [Label("Place Node Labels")]
    [OptionGroup("GeneralGroup", 10)]
    [DefaultValue(true)]
    public bool PlaceNodeLabelsItem { get; set; }

    [Label("Place Edge Labels")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(true)]
    public bool PlaceEdgeLabelsItem { get; set; }

    [Label("Consider Selected Features Only")]
    [OptionGroup("GeneralGroup", 30)]
    [DefaultValue(false)]
    public bool ConsiderSelectedFeaturesOnlyItem { get; set; }

    [Label("Quality Time Ratio")]
    [OptionGroup("QualityGroup", 10)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 0.0d, Max = 1.0d, Step = 0.01d)]
    [ComponentType(ComponentTypes.Slider)]
    public double QualityTimeRatioItem { get; set; } = 1.0;


    [Label("Reduce overlaps")]
    [OptionGroup("QualityGroup", 40)]
    [DefaultValue(LabelingOptimizationStrategy.Balanced)]
    [EnumValue("Balanced", LabelingOptimizationStrategy.Balanced)]
    [EnumValue("With Nodes", LabelingOptimizationStrategy.NodeOverlaps)]
    [EnumValue("Between Labels", LabelingOptimizationStrategy.LabelOverlaps)]
    [EnumValue("With Edges", LabelingOptimizationStrategy.EdgeOverlaps)]
    [EnumValue("Layout Grid Overlap", LabelingOptimizationStrategy.LayoutGridViolations)]
    [EnumValue("Don't optimize", null)]
    public LabelingOptimizationStrategy OptimizationStrategyItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("QualityGroup", 50)]
    [DefaultValue(true)]
    public bool ReduceAmbiguityItem { get; set; }

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

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

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem =>
        LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge;
  }
}
