/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Threading.Tasks;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Graph.LabelModels;
using FreeEdgeLabelModel=yWorks.Graph.LabelModels.FreeEdgeLabelModel;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  ///  Common base class for modules that launch labeling algorithms.
  /// </summary>
  public class LabelingModule : LayoutModule
  {
    #region configuration constants

    private const string ALLOW_NODE_OVERLAPS = "ALLOW_NODE_OVERLAPS";
    private const string AS_IS = "AS_IS";
    private const string INPUT = "INPUT";
    private const string SIDE_SLIDER = "SIDE_SLIDER";
    private const string ALLOW_EDGE_OVERLAPS = "ALLOW_EDGE_OVERLAPS";
    private const string DIVERSE_LABELING = "DIVERSE_LABELING";
    private const string QUALITY = "QUALITY";
    private const string USE_OPTIMIZATION = "USE_OPTIMIZATION";
    private const string USE_POSTPROCESSING = "USE_POSTPROCESSING";
    private const string CONSIDER_SELECTED_FEATURES_ONLY = "CONSIDER_SELECTED_FEATURES_ONLY";
    private const string NINE_POS = "NINE_POS";
    private const string SCOPE = "SCOPE";
    private const string PLACE_EDGE_LABELS = "PLACE_EDGE_LABELS";
    private const string CENTER_SLIDER = "CENTER_SLIDER";
    private const string MODEL = "MODEL";
    private const string EDGE_LABEL_MODEL = "EDGE_LABEL_MODEL";
    private const string UNKNOWN_MODEL_VALUE = "UNKNOWN_MODEL_VALUE";
    private const string FREE = "FREE";
    private const string BEST = "BEST";
    private const string PLACE_NODE_LABELS = "PLACE_NODE_LABELS";
    private const string OPTIMIZATION_STRATEGY = "OPTIMIZATION_STRATEGY";
    private const string AUTO_ROTATE = "AUTO_ROTATE";

    private const string LabelSelectionDpKey = "LabelSelection";

    private static readonly string[] edgeLabelModel = {BEST, AS_IS, CENTER_SLIDER, SIDE_SLIDER, NINE_POS, FREE};

    #endregion

    private GenericLabeling labeler;

    /// <summary>
    /// Create a new instance.
    /// </summary>
    public LabelingModule() : base(DIVERSE_LABELING) {}

    #region LayoutModule interface

    ///<inheritdoc/>
    protected override void SetupHandler() {
      OptionGroup scopeGroup = Handler.AddGroup(SCOPE);
      scopeGroup.AddBool(PLACE_NODE_LABELS, true);
      scopeGroup.AddBool(PLACE_EDGE_LABELS, true);
      scopeGroup.AddBool(CONSIDER_SELECTED_FEATURES_ONLY, false);

      OptionGroup qualityGroup = Handler.AddGroup(QUALITY);
      qualityGroup.AddBool(USE_OPTIMIZATION, false);
      qualityGroup.AddOptionItem(new OptionItem(OPTIMIZATION_STRATEGY)
                                   {Value = OptimizationStrategy.Balanced, Type = typeof (OptimizationStrategy)});
      qualityGroup.AddBool(ALLOW_NODE_OVERLAPS, false);
      qualityGroup.AddBool(ALLOW_EDGE_OVERLAPS, true);
      qualityGroup.AddBool(USE_POSTPROCESSING, false);

      var edgeLabelGroup = Handler.AddGroup(MODEL);
      var labelModelItem = edgeLabelGroup.AddList(EDGE_LABEL_MODEL, edgeLabelModel, BEST);
      var autoRotationItem = edgeLabelGroup.AddBool(AUTO_ROTATE, true);
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnCondition(cm.CreateValueIsOneOfCondition(labelModelItem, CENTER_SLIDER, SIDE_SLIDER), autoRotationItem);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
        labeler = new GenericLabeling();
      if (!(bool) Handler.GetValue(QUALITY, USE_OPTIMIZATION)) {
        labeler.MaximumDuration = 0;
      }
      labeler.AutoFlipping = true;
      labeler.OptimizationStrategy =
          (OptimizationStrategy) Handler.GetValue(QUALITY, OPTIMIZATION_STRATEGY);
      if (labeler.OptimizationStrategy == OptimizationStrategy.None) {
        labeler.ProfitModel = new SimpleProfitModel();
      }
      labeler.RemoveNodeOverlaps = !(bool) Handler.GetValue(QUALITY, ALLOW_NODE_OVERLAPS);
      labeler.RemoveEdgeOverlaps = !(bool) Handler.GetValue(QUALITY, ALLOW_EDGE_OVERLAPS);
      labeler.PlaceEdgeLabels = (bool) Handler.GetValue(SCOPE, PLACE_EDGE_LABELS);
      labeler.PlaceNodeLabels = (bool) Handler.GetValue(SCOPE, PLACE_NODE_LABELS);
      bool selectionOnly = (bool)Handler.GetValue(SCOPE, CONSIDER_SELECTED_FEATURES_ONLY);

      labeler.AffectedLabelsDpKey = null;
      LayoutAlgorithm = labeler;

      var graph = this.Context.SafeLookup<IGraph>();
      string edgeLabelingName = (string)Handler.GetValue(MODEL, EDGE_LABEL_MODEL);
      SetupEdgeLabelModels(graph, edgeLabelingName);

      var graphSelection = Context.Lookup<ISelectionModel<IModelItem>>();
      if (graphSelection != null && selectionOnly) {
        labeler.AffectedLabelsDpKey = SelectedLabelsStage.ProviderKey;
        LayoutAlgorithm = new SelectedLabelsStage(labeler);
      }
    }

    protected override async Task StartWithIGraph(IGraph graph, ILookup newContext) {
      var graphSelection = newContext.Lookup<ISelectionModel<IModelItem>>();
      if (graphSelection != null) {
        var selectedLabels = graph.MapperRegistry.CreateDelegateMapper<ILabel, bool>(LabelSelectionDpKey, graphSelection.IsSelected);
        graph.MapperRegistry.CreateDelegateMapper(
          SelectedLabelsStage.SelectedLabelsAtItemKey,
          delegate(ILabelOwner item) {
            var bools = new bool[item.Labels.Count];
            for (int i = 0; i < item.Labels.Count; i++) {
              bools[i] = selectedLabels[item.Labels[i]];
            }
            return bools;
          });
      }
      await base.StartWithIGraph(graph, newContext);
    }

    ///<inheritdoc/>
    protected override void PerformPostLayout() {
      CurrentLayoutGraph.RemoveDataProvider(INPUT);
      base.PerformPostLayout();
      var graph = this.Context.SafeLookup<IGraph>();
      graph.MapperRegistry.RemoveMapper(LabelSelectionDpKey);
      graph.MapperRegistry.RemoveMapper(SelectedLabelsStage.SelectedLabelsAtItemKey);
    }

    #endregion

    #region private helpers

    private void SetupEdgeLabelModels(IGraph coreGraph, string edgeLabelingName) {
      if(edgeLabelingName == AS_IS) {
        return;
      }

      ILabelModel model;

      bool selectionOnly = (bool) Handler.GetValue(SCOPE, CONSIDER_SELECTED_FEATURES_ONLY);
      bool placeEdgeLabels = (bool) Handler.GetValue(SCOPE, PLACE_EDGE_LABELS);
      bool autoRotate = (bool) Handler.GetValue(MODEL, AUTO_ROTATE);
      if (!placeEdgeLabels) {
        return;
      }

      switch(edgeLabelingName) {
        case CENTER_SLIDER:
          model = new EdgeSegmentLabelModel { AutoRotation = autoRotate, SideOfEdge = EdgeSides.OnEdge };  
          break;
        case SIDE_SLIDER:
          model = new EdgeSegmentLabelModel { AutoRotation = autoRotate, Distance = 10, SideOfEdge = EdgeSides.LeftOfEdge | EdgeSides.RightOfEdge}; 
          break;
        case NINE_POS:
          model = new NinePositionsEdgeLabelModel();
          break;
        case FREE:
        case BEST:
          model = new FreeEdgeLabelModel();
          break;
        default:
          throw new ArgumentException(UNKNOWN_MODEL_VALUE + edgeLabelingName, "edgeLabelingName");
      }

      foreach (ILabel label in coreGraph.Labels) {
        if (label.Owner is IEdge) {
          if (selectionOnly) {
            if (base.IsSelected(label)) {
              coreGraph.SetLabelLayoutParameter(label, model.CreateDefaultParameter());
            }
          } else {
            coreGraph.SetLabelLayoutParameter(label, model.CreateDefaultParameter());
          }
        }
      }
    }


    #endregion
  }

  public sealed class SelectedLabelsStage : LayoutStageBase
  {
    public static readonly string ProviderKey = "YetAnotherKey";
    public static readonly string SelectedLabelsAtItemKey = "SelectedLabelsAtItem";

    public SelectedLabelsStage(ILayoutAlgorithm layout) :base(layout){}

    public override void ApplyLayout(LayoutGraph graph) {
      var dataProvider = graph.GetDataProvider(SelectedLabelsAtItemKey);
      graph.AddDataProvider(ProviderKey, new MyDataProviderAdapter(dataProvider, graph));
      ApplyLayoutCore(graph);
      graph.RemoveDataProvider(ProviderKey);
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
            bool[] selectedLabels = selectedLabelsAtItemProvider.Get(node) as bool[];
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
          var edge = layoutGraph.GetOwner((IEdgeLabelLayout)dataHolder);
          if (layoutGraph is CopiedLayoutGraph) {
            bool[] selectedLabels = selectedLabelsAtItemProvider.Get(edge) as bool[];
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
  }
}
