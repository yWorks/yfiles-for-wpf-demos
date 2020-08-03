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
using System.Linq;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Labeling;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the <see cref="GenericLabeling"/> algorithm.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("Labeling")]
  public class LabelingConfig : LayoutConfiguration
  {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public LabelingConfig() {
      PlaceNodeLabelsItem = true;
      PlaceEdgeLabelsItem = true;
      ConsiderSelectedFeaturesOnlyItem = false;      

      OptimizationStrategyItem = OptimizationStrategy.Balanced;

      AllowNodeOverlapsItem = false;
      AllowEdgeOverlapsItem = true;
      ReduceAmbiguityItem = true;

      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var labeling = new GenericLabeling();

      labeling.AutoFlipping = true;
      labeling.OptimizationStrategy = OptimizationStrategyItem;
      if (labeling.OptimizationStrategy == OptimizationStrategy.None) {
        labeling.ProfitModel = new SimpleProfitModel();
      }

      labeling.RemoveNodeOverlaps = !AllowNodeOverlapsItem;
      labeling.RemoveEdgeOverlaps = !AllowEdgeOverlapsItem;
      labeling.PlaceEdgeLabels = PlaceEdgeLabelsItem;
      labeling.PlaceNodeLabels = PlaceNodeLabelsItem;
      labeling.ReduceAmbiguity = ReduceAmbiguityItem;

      var selectionOnly = ConsiderSelectedFeaturesOnlyItem;
      labeling.AffectedLabelsDpKey = null;
      ILayoutAlgorithm layout = labeling;

      if (graphControl.Selection != null && selectionOnly) {
        labeling.AffectedLabelsDpKey = SelectedLabelsStage.ProviderKey;
        layout = new SelectedLabelsStage(labeling);
      }

      AddPreferredPlacementDescriptor(graphControl.Graph, LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem);
      SetupEdgeLabelModels(graphControl);

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new LabelingData();

      var selection = graphControl.Selection;
      if (selection != null) {
        layoutData.AffectedLabels.Source = selection.SelectedLabels;

        return new CompositeLayoutData {
            Items = {
                layoutData,
                new SelectedLabelsLayoutData {
                    SelectedLabelsAtItem = {
                        Delegate =
                            item => item.Labels
                                        .Select(label => selection.IsSelected(label) || selection.IsSelected(item))
                                        .ToArray()
                    }
                }
            }
        };
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

      var parameterFinder = model.Lookup<ILabelModelParameterFinder>();
      var graph = graphControl.Graph;
      foreach (var label in graph.GetEdgeLabels()) {
        if (selectionOnly) {
          if (graphControl.Selection.IsSelected(label)) {
            graph.SetLabelLayoutParameter(
                label,
                parameterFinder.FindBestParameter(label, model, label.GetLayout()));
          }
        } else {
          graph.SetLabelLayoutParameter(
              label,
              parameterFinder.FindBestParameter(label, model, label.GetLayout()));
        }
      }
    }

    /// <summary>
    /// A layout stage that takes care to convert the selected labels mapper into the respective data provider.
    /// Unfortunately, mappers for labels are not converted into working data providers for labels automatically.
    /// </summary>
    public sealed class SelectedLabelsStage : LayoutStageBase {
      public static readonly string ProviderKey = "YetAnotherKey";
      public static readonly string SelectedLabelsAtItemKey = "SelectedLabelsAtItem";

      public SelectedLabelsStage(ILayoutAlgorithm layout) : base(layout) { }

      public override void ApplyLayout(LayoutGraph graph) {
        var dataProvider = graph.GetDataProvider(SelectedLabelsAtItemKey);
        graph.AddDataProvider(ProviderKey, new MyDataProviderAdapter(dataProvider, graph));
        ApplyLayoutCore(graph);
        graph.RemoveDataProvider(ProviderKey);
      }
    }

    public class MyDataProviderAdapter : DataProviderAdapter
    {
      private readonly IDataProvider selectedLabelsAtItemProvider;
      private readonly LayoutGraph layoutGraph;

      public MyDataProviderAdapter(IDataProvider selectedLabelsAtItemProvider, LayoutGraph layoutGraph) {
        this.selectedLabelsAtItemProvider = selectedLabelsAtItemProvider;
        this.layoutGraph = layoutGraph;
      }

      public override bool GetBool(object dataHolder) {
        if (dataHolder is INodeLabelLayout) {
          var node = layoutGraph.GetOwner((INodeLabelLayout) dataHolder);
          if (layoutGraph is CopiedLayoutGraph) {
            var selectedLabels = (bool[]) selectedLabelsAtItemProvider.Get(node);
            if (selectedLabels != null) {
              var nodeLabelLayouts = layoutGraph.GetLabelLayout(node);
              for (int i = 0; i < nodeLabelLayouts.Length; i++) {
                var nodeLabelLayout = nodeLabelLayouts[i];
                if (nodeLabelLayout == dataHolder && selectedLabels.Length > i) {
                  return selectedLabels[i];
                }
              }
            }
          }
        } else if (dataHolder is IEdgeLabelLayout) {
          var edge = layoutGraph.GetOwner((IEdgeLabelLayout) dataHolder);
          if (layoutGraph is CopiedLayoutGraph) {
            var selectedLabels = (bool[]) selectedLabelsAtItemProvider.Get(edge);
            if (selectedLabels != null) {
              var edgeLabelLayouts = layoutGraph.GetLabelLayout(edge);
              for (int i = 0; i < edgeLabelLayouts.Length; i++) {
                var edgeLabelLayout = edgeLabelLayouts[i];
                if (edgeLabelLayout == dataHolder && selectedLabels.Length > i) {
                  return selectedLabels[i];
                }
              }
            }
          }
        }
        return false;
      }
    }

    public class SelectedLabelsLayoutData : LayoutData
    {
      private ItemMapping<ILabelOwner, bool[]> selectedLabelsAtItem;

      public ItemMapping<ILabelOwner, bool[]> SelectedLabelsAtItem {
        get { return selectedLabelsAtItem ?? (selectedLabelsAtItem = new ItemMapping<ILabelOwner, bool[]>()); }
        set { selectedLabelsAtItem = value; }
      }

      protected override void Apply(LayoutGraphAdapter layoutGraphAdapter, ILayoutAlgorithm layout, CopiedLayoutGraph layoutGraph) {
        layoutGraphAdapter.AddDataProvider(SelectedLabelsStage.SelectedLabelsAtItemKey, SelectedLabelsAtItem.ProvideMapper(layoutGraphAdapter, layout));
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
    public string DescriptionText {
      get {
        return "<Paragraph>This algorithm finds good positions for the labels of nodes and edges. "
              + "Typically, a label should be placed near the item it belongs to and it should not overlap with other labels. "
              + "Optionally, overlaps with nodes and edges can be avoided as well.</Paragraph>";
      }
    } 

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

    [Label("Allow Node Overlaps")]
    [OptionGroup("QualityGroup", 10)]
    [DefaultValue(false)]
    public bool AllowNodeOverlapsItem { get; set; }

    [Label("Allow Edge Overlaps")]
    [OptionGroup("QualityGroup", 20)]
    [DefaultValue(true)]
    public bool AllowEdgeOverlapsItem { get; set; }

    [Label("Reduce overlaps")]
    [OptionGroup("QualityGroup", 40)]
    [DefaultValue(OptimizationStrategy.Balanced)]
    [EnumValue("Balanced", OptimizationStrategy.Balanced)]
    [EnumValue("With Nodes",OptimizationStrategy.NodeOverlap)]
    [EnumValue("Between Labels",OptimizationStrategy.LabelOverlap)]
    [EnumValue("With Edges",OptimizationStrategy.EdgeOverlap)]
    [EnumValue("Don't optimize",OptimizationStrategy.None)]
    public OptimizationStrategy OptimizationStrategyItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("QualityGroup", 50)]
    [DefaultValue(true)]
    public bool ReduceAmbiguityItem { get; set; }
    
    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal",EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal",EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical",EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source",EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Target",EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("Centered",EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge",EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left",EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right",EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right",EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem {
      get { return LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }
            
  }
}
