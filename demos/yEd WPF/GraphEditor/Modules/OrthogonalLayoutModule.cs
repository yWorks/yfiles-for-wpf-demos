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
using System.Collections.Generic;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Labeling;
using yWorks.Layout.Orthogonal;
using yWorks.Graph.LabelModels;
using FreeEdgeLabelModel = yWorks.Graph.LabelModels.FreeEdgeLabelModel;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for 
  /// <see cref="OrthogonalLayout"/> and <see cref="OrthogonalGroupLayout"/>, 
  /// respectively.
  /// </summary>
  public class OrthogonalLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string ORTHOGONAL = "ORTHOGONAL_LAYOUTER";

    private const string LENGTH_REDUCTION = "LENGTH_REDUCTION";
    private const string PERCEIVED_BENDS_POSTPROCESSING = "PERCEIVED_BENDS_POSTPROCESSING";
    private const string STYLE = "STYLE";
    private const string USE_FACE_MAXIMIZATION = "USE_FACE_MAXIMIZATION";
    private const string USE_RANDOMIZATION = "USE_RANDOMIZATION";
    private const string USE_EXISTING_DRAWING_AS_SKETCH = "USE_EXISTING_DRAWING_AS_SKETCH";
    private const string CROSSING_POSTPROCESSING = "CROSSING_POSTPROCESSING";
    private const string GRID = "GRID";
    private const string NORMAL = "NORMAL";
    private const string NORMAL_TREE = "NORMAL_TREE";
    private const string UNIFORM_NODES = "UNIFORM_NODES";
    private const string BOX_NODES = "BOX_NODES";
    private const string FIXED_BOX_NODES = "FIXED_BOX_NODES";
    private const string MIXED = "MIXED";
    private const string FIXED_MIXED = "FIXED_MIXED";

    private const string LAYOUT = "LAYOUT";
    private const string EDGES = "EDGES";
    private const string ORIENTATION = "ORIENTATION";
    private const string RIGHT_TO_LEFT = "RIGHT_TO_LEFT";
    private const string BOTTOM_TO_TOP = "BOTTOM_TO_TOP";
    private const string LEFT_TO_RIGHT = "LEFT_TO_RIGHT";
    private const string TOP_TO_BOTTOM = "TOP_TO_BOTTOM";

    private const string UPWARD_EDGE_BUS_ROUTING = "UPWARD_EDGE_BUS_ROUTING";
    private const string DRAW_SELECTED_EDGES_UPWARDS = "DRAW_SELECTED_EDGES_UPWARDS";
    private const string MINIMUM_FIRST_SEGMENT_LENGTH = "MINIMUM_FIRST_SEGMENT_LENGTH";
    private const string MINIMUM_SEGMENT_LENGTH = "MINIMUM_SEGMENT_LENGTH";
    private const string MINIMUM_LAST_SEGMENT_LENGTH = "MINIMUM_LAST_SEGMENT_LENGTH";
    private const string EDGE_LABEL_MODEL = "EDGE_LABEL_MODEL";
    private const string EDGE_LABELING = "EDGE_LABELING";
    private const string LABELING = "LABELING";
    private const string GENERIC = "GENERIC";
    private const string NONE = "NONE";
    private const string INTEGRATED = "INTEGRATED";
    private const string FREE = "FREE";
    private const string SIDE_SLIDER = "SIDE_SLIDER";
    private const string CENTER_SLIDER = "CENTER_SLIDER";
    private const string AS_IS = "AS_IS";
    private const string BEST = "BEST";
    private const string CONSIDER_NODE_LABELS = "CONSIDER_NODE_LABELS";

    private const string GROUPING = "GROUPING";
    private const string GROUP_LAYOUT_POLICY = "GROUP_LAYOUT_POLICY";
    private const string LAYOUT_GROUPS = "LAYOUT_GROUPS";
    private const string FIX_GROUPS = "FIX_GROUPS";
    private const string IGNORE_GROUPS = "IGNORE_GROUPS";

    private static readonly Dictionary<string, LayoutStyle> styles = new Dictionary<string, LayoutStyle>(5);
    private static readonly Dictionary<string, LayoutOrientation> orientEnum = new Dictionary<string, LayoutOrientation>();
    private static readonly List<string> edgeLabeling = new List<string>();
    private static readonly List<string> edgeLabelModel = new List<string>();
    private static readonly List<string> groupPolicy = new List<string>();

    static OrthogonalLayoutModule() {
      styles.Add(NORMAL, LayoutStyle.Normal);
      styles.Add(UNIFORM_NODES, LayoutStyle.Uniform);
      styles.Add(BOX_NODES, LayoutStyle.Box);
      styles.Add(MIXED, LayoutStyle.Mixed);
      styles.Add(FIXED_BOX_NODES, LayoutStyle.FixedBox);
      styles.Add(FIXED_MIXED, LayoutStyle.FixedMixed);

      orientEnum.Add(TOP_TO_BOTTOM, LayoutOrientation.TopToBottom);
      orientEnum.Add(LEFT_TO_RIGHT, LayoutOrientation.LeftToRight);
      orientEnum.Add(BOTTOM_TO_TOP, LayoutOrientation.BottomToTop);
      orientEnum.Add(RIGHT_TO_LEFT, LayoutOrientation.RightToLeft);

      edgeLabeling.Add(NONE);
      edgeLabeling.Add(INTEGRATED);
      edgeLabeling.Add(GENERIC);

      edgeLabelModel.Add(BEST);
      edgeLabelModel.Add(AS_IS);
      edgeLabelModel.Add(CENTER_SLIDER);
      edgeLabelModel.Add(SIDE_SLIDER);
      edgeLabelModel.Add(FREE);

      groupPolicy.Add(LAYOUT_GROUPS);
      groupPolicy.Add(FIX_GROUPS);
      groupPolicy.Add(IGNORE_GROUPS);
    }

    #endregion

    private ILayoutStage preStage;
    private IDataProvider sgDPOrig;
    private IDataProvider tgDPOrig;
    private IEdgeMap sourceGroupMap;
    private IEdgeMap tgMap;
    private IMapper<ILabel, PreferredPlacementDescriptor> originalMapper;

    /// <summary>
    /// Create a new instance
    /// </summary>
    public OrthogonalLayoutModule()
      : base(ORTHOGONAL) {}

    /// <summary>
    /// sets up the option handler for specifying the layout parameters.
    /// </summary>
    protected override void SetupHandler() {
      // global layout options
      OptionGroup layoutGroup = Handler.AddGroup(LAYOUT);
      layoutGroup.AddList(STYLE, styles.Keys, NORMAL);
      layoutGroup.AddInt(GRID, 25, 1, int.MaxValue);
      layoutGroup.AddBool(LENGTH_REDUCTION, true);
      var fromSketchItem = layoutGroup.AddBool(USE_EXISTING_DRAWING_AS_SKETCH, false);
      layoutGroup.AddBool(CROSSING_POSTPROCESSING, true);
      layoutGroup.AddBool(PERCEIVED_BENDS_POSTPROCESSING, true);
      layoutGroup.AddBool(USE_RANDOMIZATION, true);
      layoutGroup.AddBool(USE_FACE_MAXIMIZATION, false);

      //edge label options
      OptionGroup labelingGroup = Handler.AddGroup(LABELING);
      labelingGroup.AddList(EDGE_LABELING, edgeLabeling, NONE);
      labelingGroup.AddList(EDGE_LABEL_MODEL, edgeLabelModel, BEST);
      labelingGroup.AddBool(CONSIDER_NODE_LABELS, false);

      ConstraintManager cm = new ConstraintManager(Handler);

      //edge options
      OptionGroup edgeGroup = Handler.AddGroup(EDGES);
      var selection = edgeGroup.AddBool(DRAW_SELECTED_EDGES_UPWARDS, false);
      var busRouting = edgeGroup.AddBool(UPWARD_EDGE_BUS_ROUTING, true);
      var directionEnum = edgeGroup.AddList(ORIENTATION, orientEnum.Keys, TOP_TO_BOTTOM);
      cm.SetEnabledOnValueEquals(selection, true, busRouting);
      cm.SetEnabledOnValueEquals(selection, true, directionEnum);

      edgeGroup.AddDouble(MINIMUM_FIRST_SEGMENT_LENGTH, 15, 1, 500);
      edgeGroup.AddDouble(MINIMUM_SEGMENT_LENGTH, 15, 1, 500);
      edgeGroup.AddDouble(MINIMUM_LAST_SEGMENT_LENGTH, 15, 1, 500);

      // node grouping options
      OptionGroup groupingGroup = Handler.AddGroup(GROUPING);
      groupingGroup.AddList(GROUP_LAYOUT_POLICY, groupPolicy, LAYOUT_GROUPS);

      var c = cm.CreateValueEqualsCondition(fromSketchItem, false);
      cm.SetEnabledOnCondition(c, layoutGroup[CROSSING_POSTPROCESSING]);
      cm.SetEnabledOnCondition(c, layoutGroup[PERCEIVED_BENDS_POSTPROCESSING]);
      cm.SetEnabledOnCondition(c, layoutGroup[STYLE]);
      cm.SetEnabledOnCondition(c, layoutGroup[USE_RANDOMIZATION]);
    }

    /// <summary>
    /// configures the layout algorithm according to the settings of the option handler.
    /// </summary>
    protected override void ConfigureLayout() {
      OrthogonalLayout orthogonal = new OrthogonalLayout();
      ((ComponentLayout)orthogonal.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;

      SetGlobalOptions(orthogonal);
      SetEdgeLabelingOptions(orthogonal);

      LayoutAlgorithm = orthogonal;
    }

    /// <summary>
    /// sets the global layout options.
    /// </summary>
    /// <param name="orthogonal"></param>
    private void SetGlobalOptions(OrthogonalLayout orthogonal) {
      OptionGroup layoutGroup = Handler.GetGroupByName(LAYOUT);
      OptionGroup edgeGroup = Handler.GetGroupByName(EDGES);
      orthogonal.LayoutStyle = styles[(string) layoutGroup[STYLE].Value];

      if ((bool) edgeGroup[DRAW_SELECTED_EDGES_UPWARDS].Value) {
        var ol = (OrientationLayout)orthogonal.OrientationLayout;
        string orientationChoice = (string)edgeGroup[ORIENTATION].Value;
        ol.Orientation = orientEnum[orientationChoice];
      }

      orthogonal.GridSpacing = ((int) layoutGroup[GRID].Value);
      orthogonal.EdgeLengthReduction = ((bool) layoutGroup[LENGTH_REDUCTION].Value);
      orthogonal.OptimizePerceivedBends = ((bool) layoutGroup[PERCEIVED_BENDS_POSTPROCESSING].Value);
      orthogonal.CrossingReduction =
        (bool) layoutGroup[CROSSING_POSTPROCESSING].Value;
      orthogonal.Randomization = (bool) layoutGroup[USE_RANDOMIZATION].Value;
      orthogonal.FaceMaximization = (bool) layoutGroup[USE_FACE_MAXIMIZATION].Value;
      orthogonal.FromSketchMode = (bool) layoutGroup[USE_EXISTING_DRAWING_AS_SKETCH].Value;
      orthogonal.EdgeLayoutDescriptor.MinimumFirstSegmentLength = (double) edgeGroup[MINIMUM_FIRST_SEGMENT_LENGTH].Value;
      orthogonal.EdgeLayoutDescriptor.MinimumLastSegmentLength = (double) edgeGroup[MINIMUM_LAST_SEGMENT_LENGTH].Value;
      orthogonal.EdgeLayoutDescriptor.MinimumSegmentLength = (double) edgeGroup[MINIMUM_SEGMENT_LENGTH].Value;
    }

    /// <summary>
    /// sets the options for edge labeling.
    /// </summary>
    /// <param name="orthogonal"></param>
    private void SetEdgeLabelingOptions(OrthogonalLayout orthogonal) {
      String edgeLabelingName = (String) Handler.GetValue(LABELING, EDGE_LABELING);

      bool normalStyle = (orthogonal.LayoutStyle == LayoutStyle.Normal);
      orthogonal.IntegratedEdgeLabeling = (edgeLabelingName == INTEGRATED && normalStyle);
      bool considerNodeLabels = (bool) Handler.GetValue(LABELING, CONSIDER_NODE_LABELS);

      orthogonal.ConsiderNodeLabels = considerNodeLabels && normalStyle;

      if (edgeLabelingName == GENERIC || (edgeLabelingName == INTEGRATED && normalStyle)) {
        orthogonal.LabelingEnabled = true;
      } else if (!considerNodeLabels || !normalStyle) {
        orthogonal.LabelingEnabled = false;
      }
    }

    private void SetupPreferredLabelPlacement(IGraph graph) {
      IMapperRegistry registry = graph.MapperRegistry;
      originalMapper = registry.GetMapper<ILabel, PreferredPlacementDescriptor>(
        LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey);
      registry.CreateDelegateMapper(LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey,
                         delegate(ILabel label) {
                           var oldDescriptor = originalMapper != null ? originalMapper[label] : null;
                           var newDescriptor = oldDescriptor != null
                                                 ? new PreferredPlacementDescriptor(oldDescriptor)
                                                 : new PreferredPlacementDescriptor
                                                     {
                                                       PlaceAlongEdge = LabelPlacements.Anywhere,
                                                       SideOfEdge = LabelPlacements.Anywhere
                                                     };
                           SetPreferredSide(newDescriptor, oldDescriptor, label.LayoutParameter.Model);
                           return newDescriptor;
                         });
    }

    private static void SetPreferredSide(PreferredPlacementDescriptor newDescriptor,
                                         PreferredPlacementDescriptor oldDescriptor, ILabelModel model) {
      if (model is EdgeSegmentLabelModel) {
        var rotatedModel = model as EdgeSegmentLabelModel;
        var onEdge = (rotatedModel.SideOfEdge & EdgeSides.OnEdge) == EdgeSides.OnEdge;
        newDescriptor.SideOfEdge = onEdge ? LabelPlacements.OnEdge : LabelPlacements.RightOfEdge | LabelPlacements.LeftOfEdge;
        newDescriptor.DistanceToEdge = rotatedModel.Distance;
      }
    }


    ///<inheritdoc/>
    protected override void PerformPreLayout() {
      String selGroupPolicy = (String) Handler.GetValue(GROUPING, GROUP_LAYOUT_POLICY);
      String edgeLabelingName = (String) Handler.GetValue(LABELING, EDGE_LABELING);
      MultiStageLayout multiStageLayout = LayoutAlgorithm as MultiStageLayout;

      IDataProvider upwardDP = null;
      if ((upwardDP = CurrentLayoutGraph.GetDataProvider(OrthogonalLayout.DirectedEdgeDpKey)) == null) {
        //determine upward edges if not already marked.
        if ((bool)Handler.GetValue(EDGES, DRAW_SELECTED_EDGES_UPWARDS)) {
          CurrentLayoutGraph.AddDataProvider(OrthogonalLayout.DirectedEdgeDpKey, upwardDP = new UpwardEdgeDP(this));
        } 
      }

//      if ((bool)Handler.GetValue(EDGES, UPWARD_EDGE_BUS_ROUTING) && upwardDP != null) {
//        sgDPOrig = CurrentLayoutGraph.GetDataProvider(PortConstraintKeys.SourceGroupIdDpKey);
//        tgDPOrig = CurrentLayoutGraph.GetDataProvider(PortConstraintKeys.TargetGroupIdDpKey);
//        sourceGroupMap = CurrentLayoutGraph.CreateEdgeMap();
//        tgMap = CurrentLayoutGraph.CreateEdgeMap();
//        CurrentLayoutGraph.AddDataProvider(PortConstraintKeys.SourceGroupIdDpKey, sourceGroupMap);
//        CurrentLayoutGraph.AddDataProvider(PortConstraintKeys.TargetGroupIdDpKey, tgMap);
//        AutoGroupEdges(CurrentLayoutGraph, sourceGroupMap, tgMap, upwardDP);
//      }


      if ((selGroupPolicy != IGNORE_GROUPS) && ContainsGroupNodes()) {
        multiStageLayout.HideGroupsStageEnabled = false;
        if (FIX_GROUPS == selGroupPolicy) {
          var fgl = new FixGroupLayoutStage();
          fgl.InterEdgeRoutingStyle = InterEdgeRoutingStyle.Orthogonal;

          if (multiStageLayout != null) {
            multiStageLayout.PrependStage(fgl);
            preStage = fgl;
          }
        }
      } else {
        multiStageLayout.HideGroupsStageEnabled = true;
      }
      if (edgeLabelingName == GENERIC && multiStageLayout != null) {
        var la = new GenericLabeling();
        la.MaximumDuration = 0;
        la.PlaceNodeLabels = false;
        la.PlaceEdgeLabels = true;
        la.AutoFlipping = true;
        la.ProfitModel = new SimpleProfitModel();
        multiStageLayout.Labeling = la;
      }
    }

    ///<inheritdoc/>
    protected override void PerformPostLayout() {
      MultiStageLayout multiStageLayout = LayoutAlgorithm as MultiStageLayout;
      if ((preStage != null) && (multiStageLayout != null)) {
        multiStageLayout.RemoveStage(preStage);
      }
      if (sgDPOrig != null) {
        CurrentLayoutGraph.AddDataProvider(PortConstraintKeys.SourceGroupIdDpKey, sgDPOrig);
        sgDPOrig = null;
      }
      if (tgDPOrig != null) {
        CurrentLayoutGraph.AddDataProvider(PortConstraintKeys.TargetGroupIdDpKey, tgDPOrig);
        tgDPOrig = null;
      }
      if (sourceGroupMap != null) {
        CurrentLayoutGraph.DisposeEdgeMap(sourceGroupMap);
        sourceGroupMap = null;
      }
      if (tgMap != null) {
        CurrentLayoutGraph.DisposeEdgeMap(tgMap);
        tgMap = null;
      }
      CurrentLayoutGraph.RemoveDataProvider(OrthogonalLayout.DirectedEdgeDpKey);
      IGraph graph = Context.Lookup<IGraph>();
      if (graph != null) {
        ClearPreferredLabelPlacement(graph);
      }
    }

    private void ClearPreferredLabelPlacement(IGraph graph) {
      IMapperRegistry registry = graph.MapperRegistry;
      registry.RemoveMapper(LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey);
      if (originalMapper != null) {
        registry.AddMapper(LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey, originalMapper);
        originalMapper = null;
      }
    }

    protected override void StartWithIGraph(IGraph graph, ILookup newContext) {
      string edgeLabelingName = (string) Handler.GetValue(LABELING, EDGE_LABELING);
      string edgeLabelModelName = (string) Handler.GetValue(LABELING, EDGE_LABEL_MODEL);

      if (edgeLabelingName != NONE) {
        if (edgeLabelModelName != AS_IS) {
          ApplyModelToIGraph(graph, CreateModelParameters(edgeLabelingName, edgeLabelModelName));
        }
        SetupPreferredLabelPlacement(graph);
      }
      base.StartWithIGraph(graph, newContext);
    }

    private static ILabelModelParameter CreateModelParameters(string edgeLabelingName, string edgeLabelModelName) {
      if (edgeLabelingName == NONE || edgeLabelModelName == AS_IS) {
        return null; //nothing to do
      }

      switch (edgeLabelModelName) {
        case CENTER_SLIDER:
          return new EdgeSegmentLabelModel() { AutoRotation = false }.CreateParameterFromSource(0, 0.5, EdgeSides.OnEdge);
        case SIDE_SLIDER:
          return new EdgeSegmentLabelModel() { AutoRotation = false }.CreateParameterFromSource(0, 0.5, EdgeSides.LeftOfEdge);
        case FREE:
        case BEST:
          return new FreeEdgeLabelModel().CreateDefaultParameter();
        default:
          return new EdgeSegmentLabelModel() { AutoRotation = false }.CreateParameterFromSource(0, 0.5, EdgeSides.OnEdge);
      }
    }

//    private void AutoGroupEdges(LayoutGraph graph, IEdgeMap sgMap, IEdgeMap targetGroupMap, IDataProvider positiveDP) {
//      for (IEdgeCursor ec = graph.GetEdgeCursor(); ec.Ok; ec.Next()) {
//        sgMap.Set(ec.Edge, null);
//        targetGroupMap.Set(ec.Edge, null);
//      }
//
//      BHeapIntNodePQ sourceGroupPQ = new BHeapIntNodePQ(graph);
//      BHeapIntNodePQ targetGroupPQ = new BHeapIntNodePQ(graph);
//      foreach (Node n in graph.Nodes) {
//        int outDegree = 0;
//        foreach (Edge e in n.OutEdges) {
//          if (positiveDP.GetBool(e) && !e.SelfLoop) {
//            outDegree++;
//          }
//        }
//        sourceGroupPQ.Add(n, -outDegree);
//        int inDegree = 0;
//        foreach (Edge e in n.InEdges) {
//          if (positiveDP.GetBool(e) && !e.SelfLoop) {
//            inDegree++;
//          }
//        }
//        targetGroupPQ.Add(n, -inDegree);
//      }
//
//      while (!sourceGroupPQ.Empty && !targetGroupPQ.Empty) {
//        int bestIn = 0, bestOut = 0;
//        if (!sourceGroupPQ.Empty) {
//          bestOut = -sourceGroupPQ.MinPriority;
//        }
//        if (!targetGroupPQ.Empty) {
//          bestIn = -targetGroupPQ.MinPriority;
//        }
//        if (bestIn > bestOut) {
//          Node n = targetGroupPQ.RemoveMin();
//          foreach (Edge e in n.InEdges) {
//            if (sgMap.Get(e) == null && positiveDP.GetBool(e) && !e.SelfLoop) {
//              targetGroupMap.Set(e, n);
//              sourceGroupPQ.ChangePriority(e.Source, sourceGroupPQ.GetPriority(e.Source) + 1);
//            }
//          }
//        } else {
//          Node n = sourceGroupPQ.RemoveMin();
//          foreach (Edge e in n.OutEdges) {
//            if (targetGroupMap.Get(e) == null && positiveDP.GetBool(e) && !e.SelfLoop) {
//              sgMap.Set(e, n);
//              targetGroupPQ.IncreasePriority(e.Target, targetGroupPQ.GetPriority(e.Target) + 1);
//            }
//          }
//        }
//      }
//    }

    private class UpwardEdgeDP : DataProviderAdapter
    {
      private readonly LayoutModule module;

      public UpwardEdgeDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected((Edge)o);
      }
    }
  }
}
