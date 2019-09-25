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
using System.Threading.Tasks;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Graph;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Labeling;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using DataProviderAdapter = yWorks.Algorithms.Util.DataProviderAdapter;
using FreeEdgeLabelModel = yWorks.Graph.LabelModels.FreeEdgeLabelModel;
using Maps = yWorks.Algorithms.Util.Maps;
using RoutingStyle = yWorks.Layout.Hierarchic.RoutingStyle;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  ///<summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="HierarchicLayout"/>.
  /// </summary>
  public class HierarchicLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string INCREMENTAL_HIERARCHIC = "INCREMENTAL_HIERARCHIC";

    private const string GENERAL = "GENERAL";
    private const string INTERACTION = "INTERACTION";
    private const string SELECTED_ELEMENTS_INCREMENTALLY = "SELECTED_ELEMENTS_INCREMENTALLY";
    private const string USE_DRAWING_AS_SKETCH = "USE_DRAWING_AS_SKETCH";
    private const string ORIENTATION = "ORIENTATION";
    private const string RIGHT_TO_LEFT = "RIGHT_TO_LEFT";
    private const string BOTTOM_TO_TOP = "BOTTOM_TO_TOP";
    private const string LEFT_TO_RIGHT = "LEFT_TO_RIGHT";
    private const string TOP_TO_BOTTOM = "TOP_TO_BOTTOM";
    private const string LAYOUT_COMPONENTS_SEPARATELY = "LAYOUT_COMPONENTS_SEPARATELY";
    private const string SYMMETRIC_PLACEMENT = "SYMMETRIC_PLACEMENT";
    private const string MINIMUM_DISTANCES = "MINIMUM_DISTANCES";
    private const string NODE_TO_NODE_DISTANCE = "NODE_TO_NODE_DISTANCE";
    private const string NODE_TO_EDGE_DISTANCE = "NODE_TO_EDGE_DISTANCE";
    private const string EDGE_TO_EDGE_DISTANCE = "EDGE_TO_EDGE_DISTANCE";
    private const string MINIMUM_LAYER_DISTANCE = "MINIMUM_LAYER_DISTANCE";
    private const string MAXIMUM_DURATION = "MAXIMUM_DURATION";

    private const string EDGE_SETTINGS = "EDGE_SETTINGS";
    private const string EDGE_ROUTING = "EDGE_ROUTING";
    private const string EDGE_ROUTING_OCTILINEAR = "EDGE_ROUTING_OCTILINEAR";
    private const string EDGE_ROUTING_ORTHOGONAL = "EDGE_ROUTING_ORTHOGONAL";
    private const string EDGE_ROUTING_POLYLINE = "EDGE_ROUTING_POLYLINE";
    private const string BACKLOOP_ROUTING = "BACKLOOP_ROUTING";
    private const string MINIMUM_FIRST_SEGMENT_LENGTH = "MINIMUM_FIRST_SEGMENT_LENGTH";
    private const string MINIMUM_LAST_SEGMENT_LENGTH = "MINIMUM_LAST_SEGMENT_LENGTH";
    private const string MINIMUM_EDGE_LENGTH = "MINIMUM_EDGE_LENGTH";
    private const string MINIMUM_EDGE_DISTANCE = "MINIMUM_EDGE_DISTANCE";
    private const string MINIMUM_SLOPE = "MINIMUM_SLOPE";
    private const string PC_OPTIMIZATION_ENABLED = "PC_OPTIMIZATION_ENABLED";
    private const string AUTOMATIC_EDGE_GROUPING_ENABLED = "AUTOMATIC_EDGE_GROUPING_ENABLED"; 
    private const string EDGE_STRAIGHTENING_OPTIMIZATION_ENABLED = "EDGE_STRAIGHTENING_OPTIMIZATION_ENABLED";
    private const string CONSIDER_EDGE_THICKNESS = "CONSIDER_EDGE_THICKNESS"; 
    private const string CONSIDER_EDGE_DIRECTION = "CONSIDER_EDGE_DIRECTION"; 
    private const string RECURSIVE_EDGE_ROUTING = "RECURSIVE_EDGE_ROUTING"; 
    private const string RECURSIVE_EDGE_ROUTING_OFF = "RECURSIVE_EDGE_ROUTING_OFF"; 
    private const string RECURSIVE_EDGE_ROUTING_DIRECTED = "RECURSIVE_EDGE_ROUTING_DIRECTED"; 
    private const string RECURSIVE_EDGE_ROUTING_UNDIRECTED = "RECURSIVE_EDGE_ROUTING_UNDIRECTED"; 

    private const string RANKS = "RANKS";
    private const string RANKING_POLICY = "RANKING_POLICY";
    private const string HIERARCHICAL_OPTIMAL = "HIERARCHICAL_OPTIMAL";
    private const string HIERARCHICAL_TIGHT_TREE_HEURISTIC = "HIERARCHICAL_TIGHT_TREE_HEURISTIC";
    private const string HIERARCHICAL_TOPMOST = "HIERARCHICAL_TOPMOST";
    private const string BFS_LAYERS = "BFS_LAYERS";
    private const string FROM_SKETCH = "FROM_SKETCH";
    private const string LAYER_ALIGNMENT = "LAYER_ALIGNMENT";
    private const string TOP = "TOP";
    private const string CENTER = "CENTER";
    private const string BOTTOM = "BOTTOM";
    private const string FROM_SKETCH_PROPERTIES = "FROM_SKETCH_PROPERTIES";
    private const string SCALE = "SCALE";
    private const string HALO = "HALO";
    private const string MINIMUM_SIZE = "MINIMUM_SIZE";
    private const string MAXIMUM_SIZE = "MAXIMUM_SIZE";
    private const string COMPONENT_ARRANGEMENT_POLICY = "COMPONENT_ARRANGEMENT_POLICY";
    private const string POLICY_TOPMOST = "POLICY_TOPMOST";
    private const string POLICY_COMPACT = "POLICY_COMPACT";


    private const string LABELING = "LABELING";
    private const string NODE_PROPERTIES = "NODE_PROPERTIES";
    private const string CONSIDER_NODE_LABELS = "CONSIDER_NODE_LABELS";
    private const string LABELING_EDGE_PROPERTIES = "EDGE_PROPERTIES";
    private const string EDGE_LABELING = "EDGE_LABELING";
    private const string EDGE_LABELING_NONE = "EDGE_LABELING_NONE";
    private const string EDGE_LABELING_HIERARCHIC = "EDGE_LABELING_HIERARCHIC";
    private const string EDGE_LABELING_GENERIC = "EDGE_LABELING_GENERIC";
    private const string EDGE_LABEL_MODEL = "EDGE_LABEL_MODEL";
    private const string EDGE_LABEL_MODEL_FREE = "EDGE_LABEL_MODEL_FREE";
    private const string EDGE_LABEL_MODEL_BEST = "EDGE_LABEL_MODEL_BEST";
    private const string EDGE_LABEL_MODEL_AS_IS = "EDGE_LABEL_MODEL_AS_IS";
    private const string EDGE_LABEL_MODEL_SIDE_SLIDER = "EDGE_LABEL_MODEL_SIDE_SLIDER";
    private const string EDGE_LABEL_MODEL_CENTER_SLIDER = "EDGE_LABEL_MODEL_CENTER_SLIDER";
    private const string COMPACT_EDGE_LABEL_PLACEMENT = "COMPACT_EDGE_LABEL_PLACEMENT";

    private const string GROUPING = "GROUPING";
    private const string GROUP_LAYERING_STRATEGY = "GROUP_LAYERING_STRATEGY";
    private const string GLOBAL_LAYERING = "GLOBAL_LAYERING";
    private const string RECURSIVE_LAYERING = "RECURSIVE_LAYERING";
    private const string GROUP_ALIGNMENT = "GROUP_ALIGNMENT";
    private const string GROUP_ALIGN_TOP = "GROUP_ALIGN_TOP";
    private const string GROUP_ALIGN_CENTER = "GROUP_ALIGN_CENTER";
    private const string GROUP_ALIGN_BOTTOM = "GROUP_ALIGN_BOTTOM";

    private const string GROUP_ENABLE_COMPACTION = "GROUP_ENABLE_COMPACTION";
    private const string GROUP_HORIZONTAL_COMPACTION = "GROUP_HORIZONTAL_COMPACTION";
    private const string GROUP_HORIZONTAL_COMPACTION_NONE = "GROUP_HORIZONTAL_COMPACTION_NONE";
    private const string GROUP_HORIZONTAL_COMPACTION_MAX = "GROUP_HORIZONTAL_COMPACTION_MAX";

    private const string SWIMLANES = "SWIMLANES";
    private const string TREAT_ROOT_GROUPS_AS_SWIMLANES = "TREAT_ROOT_GROUPS_AS_SWIMLANES";
    private const string USE_ORDER_FROM_SKETCH = "USE_ORDER_FROM_SKETCH";
    private const string SWIMLANE_SPACING = "SWIMLANE_SPACING";


    private const string GRID = "GRID";
    private const string GRID_ENABLED = "GRID_ENABLED";
    private const string GRID_SPACING = "GRID_SPACING";
    private const string GRID_PORT_ASSIGNMENT = "GRID_PORT_ASSIGNMENT";
    private const string GRID_PORT_ASSIGNMENT_DEFAULT = "GRID_PORT_ASSIGNMENT_DEFAULT";
    private const string GRID_PORT_ASSIGNMENT_ON_GRID = "GRID_PORT_ASSIGNMENT_ON_GRID";
    private const string GRID_PORT_ASSIGNMENT_ON_SUBGRID = "GRID_PORT_ASSIGNMENT_ON_SUBGRID";
    
    private static readonly Dictionary<string, LayoutOrientation> orientEnum = new Dictionary<string, LayoutOrientation>();
    private static readonly Dictionary<string, double> alignmentEnum = new Dictionary<string, double>();
    private static readonly Dictionary<string, ComponentArrangementPolicy> componentAlignmentEnum = new Dictionary<string, ComponentArrangementPolicy>();
    private static readonly Dictionary<string, LayeringStrategy> rankingPolicies = new Dictionary<string, LayeringStrategy>();
    private static readonly Dictionary<string, RoutingStyle> edgeRoutingEnum = new Dictionary<string, RoutingStyle>();
    private static readonly List<string> edgeLabeling = new List<string>();
    private static readonly List<string> edgeLabelModel = new List<string>();

    private static readonly Dictionary<string, bool> groupStrategyEnum = new Dictionary<string, bool>();
    private static readonly Dictionary<string, GroupAlignmentPolicy> groupAlignmentEnum = new Dictionary<string, GroupAlignmentPolicy>();
    private static readonly Dictionary<string, GroupCompactionPolicy> groupHorizCompactionEnum = new Dictionary<string, GroupCompactionPolicy>();


    static HierarchicLayoutModule() {
      orientEnum.Add(TOP_TO_BOTTOM, LayoutOrientation.TopToBottom);
      orientEnum.Add(LEFT_TO_RIGHT, LayoutOrientation.LeftToRight);
      orientEnum.Add(BOTTOM_TO_TOP, LayoutOrientation.BottomToTop);
      orientEnum.Add(RIGHT_TO_LEFT, LayoutOrientation.RightToLeft);

      alignmentEnum.Add(TOP, 0.0);
      alignmentEnum.Add(CENTER, 0.5);
      alignmentEnum.Add(BOTTOM, 1.0);

      componentAlignmentEnum.Add(POLICY_COMPACT, ComponentArrangementPolicy.Compact);
      componentAlignmentEnum.Add(POLICY_TOPMOST, ComponentArrangementPolicy.Topmost);

      rankingPolicies.Add(HIERARCHICAL_OPTIMAL, LayeringStrategy.HierarchicalOptimal);
      rankingPolicies.Add(HIERARCHICAL_TIGHT_TREE_HEURISTIC,
                          LayeringStrategy.HierarchicalTightTree);
      rankingPolicies.Add(BFS_LAYERS, LayeringStrategy.Bfs);
      rankingPolicies.Add(FROM_SKETCH, LayeringStrategy.FromSketch);
      rankingPolicies.Add(HIERARCHICAL_TOPMOST, LayeringStrategy.HierarchicalTopmost);

      edgeLabeling.Add(EDGE_LABELING_NONE);
      edgeLabeling.Add(EDGE_LABELING_HIERARCHIC);
      edgeLabeling.Add(EDGE_LABELING_GENERIC);

      edgeLabelModel.Add(EDGE_LABEL_MODEL_BEST);
      edgeLabelModel.Add(EDGE_LABEL_MODEL_AS_IS);
      edgeLabelModel.Add(EDGE_LABEL_MODEL_CENTER_SLIDER);
      edgeLabelModel.Add(EDGE_LABEL_MODEL_SIDE_SLIDER);
      edgeLabelModel.Add(EDGE_LABEL_MODEL_FREE);

      edgeRoutingEnum.Add(EDGE_ROUTING_OCTILINEAR, new RoutingStyle(EdgeRoutingStyle.Octilinear));
      edgeRoutingEnum.Add(EDGE_ROUTING_ORTHOGONAL, new RoutingStyle(EdgeRoutingStyle.Orthogonal));
      edgeRoutingEnum.Add(EDGE_ROUTING_POLYLINE, new RoutingStyle(EdgeRoutingStyle.Polyline));


      groupStrategyEnum.Add(RECURSIVE_LAYERING, true);
      groupStrategyEnum.Add(GLOBAL_LAYERING, false);

      groupAlignmentEnum.Add(GROUP_ALIGN_TOP, GroupAlignmentPolicy.Top);
      groupAlignmentEnum.Add(GROUP_ALIGN_CENTER, GroupAlignmentPolicy.Center);
      groupAlignmentEnum.Add(GROUP_ALIGN_BOTTOM, GroupAlignmentPolicy.Bottom);

      groupHorizCompactionEnum.Add(GROUP_HORIZONTAL_COMPACTION_MAX, GroupCompactionPolicy.Maximal);
      groupHorizCompactionEnum.Add(GROUP_HORIZONTAL_COMPACTION_NONE, GroupCompactionPolicy.None);
    }

    #endregion

    #region private members

    private OrientationLayout ol;
    private IMapper<ILabel, PreferredPlacementDescriptor> originalMapper;

    #endregion

    /// <summary>
    /// Create a new instance.
    /// </summary>
    public HierarchicLayoutModule() : base(INCREMENTAL_HIERARCHIC) { }

    #region LayoutModule interface

    ///<inheritdoc/>
    protected override void SetupHandler() {
      OptionGroup generalGroup = Handler.AddGroup(GENERAL);
      OptionGroup interactionGroup = generalGroup.AddGroup(INTERACTION);
      interactionGroup.AddBool(SELECTED_ELEMENTS_INCREMENTALLY, false);
      IOptionItem useDrawingItem = interactionGroup.AddBool(USE_DRAWING_AS_SKETCH, false);
      interactionGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;

      generalGroup.AddList(ORIENTATION, orientEnum.Keys, TOP_TO_BOTTOM);
      generalGroup.AddBool(LAYOUT_COMPONENTS_SEPARATELY, false);
      IOptionItem symmetricPlacement = generalGroup.AddBool(SYMMETRIC_PLACEMENT, true);
      generalGroup.AddInt(MAXIMUM_DURATION, 5);

      OptionGroup distanceGroup = generalGroup.AddGroup(MINIMUM_DISTANCES);
      distanceGroup.AddDouble(NODE_TO_NODE_DISTANCE, 30.0d);
      distanceGroup.AddDouble(NODE_TO_EDGE_DISTANCE, 15.0d);
      distanceGroup.AddDouble(EDGE_TO_EDGE_DISTANCE, 15.0d);
      distanceGroup.AddDouble(MINIMUM_LAYER_DISTANCE, 10.0d);

      OptionGroup edgeSettingsGroup = Handler.AddGroup(EDGE_SETTINGS);
      OptionItem eoi = edgeSettingsGroup.AddList(EDGE_ROUTING, edgeRoutingEnum.Keys, EDGE_ROUTING_ORTHOGONAL);
      edgeSettingsGroup.AddBool(BACKLOOP_ROUTING, false);
      edgeSettingsGroup.AddBool(AUTOMATIC_EDGE_GROUPING_ENABLED, false);
      IOptionItem edgeStraightening = edgeSettingsGroup.AddBool(EDGE_STRAIGHTENING_OPTIMIZATION_ENABLED, false);
      edgeSettingsGroup.AddBool(CONSIDER_EDGE_THICKNESS, false);
      edgeSettingsGroup.AddBool(CONSIDER_EDGE_DIRECTION, false);
      edgeSettingsGroup.AddDouble(MINIMUM_FIRST_SEGMENT_LENGTH, 10.0d);
      edgeSettingsGroup.AddDouble(MINIMUM_LAST_SEGMENT_LENGTH, 15.0d);
      edgeSettingsGroup.AddDouble(MINIMUM_EDGE_LENGTH, 20.0d);
      edgeSettingsGroup.AddDouble(MINIMUM_EDGE_DISTANCE, 15.0d);

      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(eoi, EDGE_ROUTING_POLYLINE,
                                 edgeSettingsGroup.AddDouble(MINIMUM_SLOPE, 0.25d, 0.0d, 5.0d));
      cm.SetEnabledOnValueEquals(symmetricPlacement, false, edgeStraightening);

      edgeSettingsGroup.AddBool(PC_OPTIMIZATION_ENABLED, false);
      edgeSettingsGroup.AddList(RECURSIVE_EDGE_ROUTING, new[] {RECURSIVE_EDGE_ROUTING_OFF, RECURSIVE_EDGE_ROUTING_DIRECTED, RECURSIVE_EDGE_ROUTING_UNDIRECTED}, RECURSIVE_EDGE_ROUTING_OFF);

      OptionGroup rankGroup = Handler.AddGroup(RANKS);
      rankGroup.AddList(RANKING_POLICY, rankingPolicies.Keys, HIERARCHICAL_OPTIMAL);
      rankGroup.AddList(LAYER_ALIGNMENT, alignmentEnum.Keys, BOTTOM);
      rankGroup.AddList(COMPONENT_ARRANGEMENT_POLICY, componentAlignmentEnum.Keys, POLICY_TOPMOST);

      OptionGroup sketchGroup = rankGroup.AddGroup(FROM_SKETCH_PROPERTIES);
      sketchGroup.AddDouble(SCALE, 1.0d, 0.0d, 5.0d);
      sketchGroup.AddDouble(HALO, 0.0d);
      sketchGroup.AddDouble(MINIMUM_SIZE, 0.0d, 0, Double.MaxValue);
      sketchGroup.AddDouble(MAXIMUM_SIZE, 1000.0d, 0, Double.MaxValue);
      cm.SetEnabledOnValueEquals(Handler.GetItemByName(RANKS + "." + RANKING_POLICY), FROM_SKETCH,
                                 Handler.GetItemByName(RANKS + "." + FROM_SKETCH_PROPERTIES));

      OptionGroup labelingGroup = Handler.AddGroup(LABELING);
      OptionGroup npGroup = labelingGroup.AddGroup(NODE_PROPERTIES);
      npGroup.AddBool(CONSIDER_NODE_LABELS, true);
      npGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      //npGroup.SetAttribute(DefaultEditorFactory.RenderingHintsAttribute,
      //  DefaultEditorFactory.RenderingHints.Invisible);
      OptionGroup epGroup = labelingGroup.AddGroup(LABELING_EDGE_PROPERTIES);
      CollectionOptionItem<string> edgeLabelingEnumItem = epGroup.AddList(EDGE_LABELING, edgeLabeling, EDGE_LABELING_NONE);
      CollectionOptionItem<string> labelModelItem = epGroup.AddList(EDGE_LABEL_MODEL, edgeLabelModel, EDGE_LABEL_MODEL_BEST);
      epGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      ICondition cond = ConstraintManager.LogicalCondition.Not(cm.CreateValueEqualsCondition(edgeLabelingEnumItem, EDGE_LABELING_NONE));
      cm.SetEnabledOnCondition(cond, labelModelItem);
      IOptionItem compactEdgeLabel = epGroup.AddBool(COMPACT_EDGE_LABEL_PLACEMENT, true);
      cm.SetEnabledOnValueEquals(edgeLabelingEnumItem, EDGE_LABELING_HIERARCHIC, compactEdgeLabel);
      OptionGroup groupingGroup = Handler.AddGroup(GROUPING);
      CollectionOptionItem<string> groupStrategyItem = groupingGroup.AddList(GROUP_LAYERING_STRATEGY, groupStrategyEnum.Keys, RECURSIVE_LAYERING);
      CollectionOptionItem<string> groupAlignItem = groupingGroup.AddList(GROUP_ALIGNMENT, groupAlignmentEnum.Keys, GROUP_ALIGN_TOP);
      IOptionItem groupCompactionItem = groupingGroup.AddBool(GROUP_ENABLE_COMPACTION, false);
      groupingGroup.AddList(GROUP_HORIZONTAL_COMPACTION, groupHorizCompactionEnum.Keys, GROUP_HORIZONTAL_COMPACTION_NONE);
      cm.SetEnabledOnValueEquals(groupStrategyItem, RECURSIVE_LAYERING, groupCompactionItem);
      cm.SetEnabledOnValueEquals(groupStrategyItem, RECURSIVE_LAYERING, groupAlignItem);

      cm.SetEnabledOnValueEquals(useDrawingItem, false, groupStrategyItem);
      cm.SetEnabledOnCondition(ConstraintManager.LogicalCondition.And(cm.CreateValueEqualsCondition(groupStrategyItem, RECURSIVE_LAYERING),
        cm.CreateValueEqualsCondition(groupCompactionItem, false)), groupAlignItem);

      cm.SetEnabledOnCondition(ConstraintManager.LogicalCondition.And(cm.CreateValueEqualsCondition(groupStrategyItem, RECURSIVE_LAYERING),
          cm.CreateValueEqualsCondition(useDrawingItem, false)), groupCompactionItem);

      OptionGroup swimGroup = Handler.AddGroup(SWIMLANES);
      IOptionItem swimlaneOption = swimGroup.AddBool(TREAT_ROOT_GROUPS_AS_SWIMLANES, false);
      IOptionItem fromSketchOption = swimGroup.AddBool(USE_ORDER_FROM_SKETCH, false);
      IOptionItem spacingOption = swimGroup.AddDouble(SWIMLANE_SPACING, 0.0d, 0, Double.MaxValue);
      cm.SetEnabledOnValueEquals(swimlaneOption, true, fromSketchOption);
      cm.SetEnabledOnValueEquals(swimlaneOption, true, spacingOption);

      OptionGroup gridGroup = Handler.AddGroup(GRID);
      IOptionItem gridEnabled = gridGroup.AddBool(GRID_ENABLED, false);
      IOptionItem gridSpacing = gridGroup.AddDouble(GRID_SPACING, 10);
      IOptionItem gridPortAssignment = gridGroup.AddList(GRID_PORT_ASSIGNMENT, new[]{GRID_PORT_ASSIGNMENT_DEFAULT, GRID_PORT_ASSIGNMENT_ON_GRID, GRID_PORT_ASSIGNMENT_ON_SUBGRID}, GRID_PORT_ASSIGNMENT_DEFAULT);
      cm.SetEnabledOnValueEquals(gridEnabled, true, gridSpacing);
      cm.SetEnabledOnValueEquals(gridEnabled, true, gridPortAssignment);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      LayoutGraph graph = CurrentLayoutGraph;
      HierarchicLayout hl = new HierarchicLayout();
      LayoutAlgorithm = hl;

      //  mark incremental elements if required
      IDataMap incrementalElements;
      OptionGroup generalGroup = (OptionGroup) Handler.GetGroupByName(GENERAL);
      OptionGroup currentGroup = (OptionGroup) generalGroup.GetGroupByName(INTERACTION);
      OptionGroup groupingGroup = (OptionGroup) Handler.GetGroupByName(GROUPING);

      bool fromSketch = (bool)currentGroup[USE_DRAWING_AS_SKETCH].Value;
      bool incrementalLayout = (bool)currentGroup[SELECTED_ELEMENTS_INCREMENTALLY].Value;
      bool selectedElements = !IsEdgeSelectionEmpty() || !IsNodeSelectionEmpty();

      if (incrementalLayout && selectedElements) {
        // create storage for both nodes and edges
        incrementalElements = Maps.CreateHashedDataMap();
        // configure the mode
        hl.LayoutMode = LayoutMode.Incremental;
        IIncrementalHintsFactory ihf = hl.CreateIncrementalHintsFactory();

        foreach (Node node in graph.Nodes) {
          if (IsSelected(node)) {
            incrementalElements.Set(node, ihf.CreateLayerIncrementallyHint(node));
          }
        }

        foreach (Edge edge in graph.Edges) {
          if (IsSelected(edge)) {
            incrementalElements.Set(edge, ihf.CreateSequenceIncrementallyHint(edge));
          }
        }
        graph.AddDataProvider(HierarchicLayout.IncrementalHintsDpKey, incrementalElements);
      } else if (fromSketch) {
        hl.LayoutMode = LayoutMode.Incremental;
      } else {
        hl.LayoutMode = LayoutMode.FromScratch;
      }


      // cast to implementation simplex
      var np = (SimplexNodePlacer)hl.NodePlacer;
      np.BarycenterMode = (bool)generalGroup[SYMMETRIC_PLACEMENT].Value;
      np.StraightenEdges = (bool) Handler.GetValue(EDGE_SETTINGS, EDGE_STRAIGHTENING_OPTIMIZATION_ENABLED);

      hl.ComponentLayoutEnabled = (bool)generalGroup[LAYOUT_COMPONENTS_SEPARATELY].Value;

      currentGroup = (OptionGroup) generalGroup.GetGroupByName(MINIMUM_DISTANCES);

      hl.MinimumLayerDistance = (double)currentGroup[MINIMUM_LAYER_DISTANCE].Value;
      hl.NodeToEdgeDistance = (double)currentGroup[NODE_TO_EDGE_DISTANCE].Value;
      hl.NodeToNodeDistance = (double)currentGroup[NODE_TO_NODE_DISTANCE].Value;
      hl.EdgeToEdgeDistance = (double)currentGroup[EDGE_TO_EDGE_DISTANCE].Value;

      NodeLayoutDescriptor nld = hl.NodeLayoutDescriptor;
      EdgeLayoutDescriptor eld = hl.EdgeLayoutDescriptor;

      currentGroup = (OptionGroup) Handler.GetGroupByName(EDGE_SETTINGS);

      hl.AutomaticEdgeGrouping = (bool)currentGroup[AUTOMATIC_EDGE_GROUPING_ENABLED].Value;

      string edgeRoutingChoice = (string)currentGroup[EDGE_ROUTING].Value;
      eld.RoutingStyle = edgeRoutingEnum[edgeRoutingChoice];
      eld.MinimumFirstSegmentLength = (double)currentGroup[MINIMUM_FIRST_SEGMENT_LENGTH].Value;
      eld.MinimumLastSegmentLength = (double)currentGroup[MINIMUM_LAST_SEGMENT_LENGTH].Value;

      eld.MinimumDistance = (double)currentGroup[MINIMUM_EDGE_DISTANCE].Value;
      eld.MinimumLength = (double)currentGroup[MINIMUM_EDGE_LENGTH].Value;

      eld.MinimumSlope = (double)currentGroup[MINIMUM_SLOPE].Value;

      eld.SourcePortOptimization = (bool)currentGroup[PC_OPTIMIZATION_ENABLED].Value;
      eld.TargetPortOptimization = (bool)currentGroup[PC_OPTIMIZATION_ENABLED].Value;
      
      var isIncrementalModeEnabled = (fromSketch || (incrementalLayout && selectedElements));
      var recursiveRoutingMode = Handler.GetValue(EDGE_SETTINGS, RECURSIVE_EDGE_ROUTING);
      if (!isIncrementalModeEnabled && recursiveRoutingMode == RECURSIVE_EDGE_ROUTING_DIRECTED) {
        eld.RecursiveEdgeStyle = RecursiveEdgeStyle.Directed;
      } else if (!isIncrementalModeEnabled && recursiveRoutingMode == RECURSIVE_EDGE_ROUTING_UNDIRECTED) {
        eld.RecursiveEdgeStyle = RecursiveEdgeStyle.Undirected;
      } else {
        eld.RecursiveEdgeStyle = RecursiveEdgeStyle.Off;
      }

      nld.MinimumDistance = Math.Min(hl.NodeToNodeDistance, hl.NodeToEdgeDistance);
      nld.MinimumLayerHeight = 0;

      OptionGroup rankGroup = (OptionGroup) Handler.GetGroupByName(RANKS);
      string layerAlignmentChoice = (string) rankGroup[LAYER_ALIGNMENT].Value;
      nld.LayerAlignment = alignmentEnum[layerAlignmentChoice];

      ol = (OrientationLayout)hl.OrientationLayout;
      string orientationChoice = (string)generalGroup[ORIENTATION].Value;
      ol.Orientation = orientEnum[orientationChoice];

      OptionGroup labelingGroup = (OptionGroup) Handler.GetGroupByName(LABELING);
      currentGroup = (OptionGroup) labelingGroup.GetGroupByName(LABELING_EDGE_PROPERTIES);
      string el = (string) currentGroup[EDGE_LABELING].Value;

      if (!el.Equals(EDGE_LABELING_NONE)) {
        if (el.Equals(EDGE_LABELING_GENERIC)) {
          var la = new GenericLabeling();
          la.MaximumDuration = 0;
          la.PlaceNodeLabels = false;
          la.PlaceEdgeLabels = true;
          la.AutoFlipping = true;
          la.ProfitModel = new SimpleProfitModel();
          hl.PrependStage(la);
        } else if (el.Equals(EDGE_LABELING_HIERARCHIC)) {
          bool copactEdgeLabelPlacement = (bool) currentGroup[COMPACT_EDGE_LABEL_PLACEMENT].Value;
          if (hl.NodePlacer is SimplexNodePlacer) {
            np.LabelCompaction = copactEdgeLabelPlacement;
          }
          hl.IntegratedEdgeLabeling = true;
        }
      } else {
        hl.IntegratedEdgeLabeling = false;
      }

      currentGroup = (OptionGroup)labelingGroup.GetGroupByName(NODE_PROPERTIES);
      if ((bool)currentGroup[CONSIDER_NODE_LABELS].Value) {
        hl.ConsiderNodeLabels = true;
        hl.NodeLayoutDescriptor.NodeLabelMode = NodeLabelMode.ConsiderForDrawing;
      } else {
        hl.ConsiderNodeLabels = false;
      }

      string rp = (string)rankGroup[RANKING_POLICY].Value;
      hl.FromScratchLayeringStrategy = rankingPolicies[rp];
      if (rp.Equals(BFS_LAYERS)) {
        CurrentLayoutGraph.AddDataProvider(BFSLayerer.CoreNodesDpKey, new SelectedNodesDP(this));
      }

      hl.ComponentArrangementPolicy = componentAlignmentEnum[(string) rankGroup[COMPONENT_ARRANGEMENT_POLICY].Value];

      //configure AsIsLayerer
      Object layerer = (hl.LayoutMode == LayoutMode.FromScratch)
                         ? hl.FromScratchLayerer
                         : hl.FixedElementsLayerer;
//      if (layerer is OldLayererWrapper) {
//        layerer = ((OldLayererWrapper)layerer).OldLayerer;
//      }
      if (layerer is AsIsLayerer) {
        AsIsLayerer ail = (AsIsLayerer)layerer;
        currentGroup = (OptionGroup)rankGroup.GetGroupByName(FROM_SKETCH_PROPERTIES);
        ail.NodeHalo = (double)currentGroup[HALO].Value;
        ail.NodeScalingFactor = (double)currentGroup[SCALE].Value;
        ail.MinimumNodeSize = (double)currentGroup[MINIMUM_SIZE].Value;
        ail.MaximumNodeSize = (double)currentGroup[MAXIMUM_SIZE].Value;
      }

      //configure grouping
      np.GroupCompactionStrategy = groupHorizCompactionEnum[(string)groupingGroup[GROUP_HORIZONTAL_COMPACTION].Value];

      if (!fromSketch && groupStrategyEnum[(string)groupingGroup[GROUP_LAYERING_STRATEGY].Value]) {
        GroupAlignmentPolicy alignmentPolicy = groupAlignmentEnum[(string)groupingGroup[GROUP_ALIGNMENT].Value];
        hl.GroupAlignmentPolicy = alignmentPolicy;
        hl.CompactGroups = (bool)groupingGroup[GROUP_ENABLE_COMPACTION].Value;
        hl.RecursiveGroupLayering = true;
      } else {
        hl.RecursiveGroupLayering = false;
      }

      OptionGroup swimGroup = (OptionGroup) Handler.GetGroupByName(SWIMLANES);
      if ((bool) swimGroup[TREAT_ROOT_GROUPS_AS_SWIMLANES].Value) {
        TopLevelGroupToSwimlaneStage stage = new TopLevelGroupToSwimlaneStage();
        stage.OrderSwimlanesFromSketch = (bool)swimGroup[USE_ORDER_FROM_SKETCH].Value;
        stage.Spacing = (double)swimGroup[SWIMLANE_SPACING].Value;
        hl.AppendStage(stage);
      }

      hl.BackLoopRouting = (bool)Handler.GetValue(EDGE_SETTINGS,BACKLOOP_ROUTING);
      hl.MaximumDuration = ((int)Handler.GetValue(GENERAL, MAXIMUM_DURATION)) * 1000;


      bool gridEnabled = (bool) Handler.GetValue(GRID, GRID_ENABLED);
      if (gridEnabled) {
        hl.GridSpacing = (double) Handler.GetValue(GRID, GRID_SPACING);
        String portAssignment = (string) Handler.GetValue(GRID, GRID_PORT_ASSIGNMENT);
        PortAssignmentMode gridPortAssignment;
        switch (portAssignment) {
          case GRID_PORT_ASSIGNMENT_ON_GRID:
            gridPortAssignment = PortAssignmentMode.OnGrid;
            break;
          case GRID_PORT_ASSIGNMENT_ON_SUBGRID:
            gridPortAssignment = PortAssignmentMode.OnSubgrid;
            break;
          default:
            gridPortAssignment = PortAssignmentMode.Default;
            break;
        }
        graph.AddDataProvider(HierarchicLayoutCore.NodeLayoutDescriptorDpKey,
            new NodeLayoutDescriptorAdapter(hl.NodeLayoutDescriptor, graph, gridPortAssignment));
      }
    }

    private class NodeLayoutDescriptorAdapter : DataProviderAdapter
    {
      NodeLayoutDescriptor nld;
      private readonly LayoutGraph graph;
      private readonly PortAssignmentMode gridPortAssignment;

      public NodeLayoutDescriptorAdapter(NodeLayoutDescriptor nld, LayoutGraph graph, PortAssignmentMode gridPortAssignment) {
        this.nld = nld;
        this.graph = graph;
        this.gridPortAssignment = gridPortAssignment;
      }


      public override object Get(Object dataHolder) {
          Node node = dataHolder as Node;
          if (node != null) {
            // copy descriptor to keep all settings for this node
            NodeLayoutDescriptor descriptor = new NodeLayoutDescriptor();
            descriptor.LayerAlignment = nld.LayerAlignment;
            descriptor.MinimumDistance = nld.MinimumDistance;
            descriptor.MinimumLayerHeight = nld.MinimumLayerHeight;
            descriptor.NodeLabelMode = nld.NodeLabelMode;
            // anchor nodes on grid according to their alignment within the layer
            descriptor.GridReference = new YPoint(0.0, (nld.LayerAlignment - 0.5) * graph.GetHeight(node));
            descriptor.PortAssignment = gridPortAssignment;
            return descriptor;
          }
          return null;
        }
      
      
    }

    ///<inheritdoc/>
    protected override void PerformPostLayout() {
      // remove the registered DataProvider instances
      CurrentLayoutGraph.RemoveDataProvider(HierarchicLayout.IncrementalHintsDpKey);
      CurrentLayoutGraph.RemoveDataProvider(PortConstraintKeys.SourcePortConstraintDpKey);
      CurrentLayoutGraph.RemoveDataProvider(PortConstraintKeys.TargetPortConstraintDpKey);
      IGraph graph = Context.Lookup<IGraph>();
      if (graph != null) {
        ClearPreferredLabelPlacement(graph);
      }
      base.PerformPostLayout();
    }

    #endregion

    #region private helpers

    protected override async Task StartWithIGraph(IGraph graph, ILookup newContext) {
      OptionGroup epGroup = (OptionGroup)Handler.GetGroupByName(LABELING).GetGroupByName(LABELING_EDGE_PROPERTIES);
      string edgeLabelModelName = (string)epGroup[EDGE_LABEL_MODEL].Value;
      string edgeLabelingName = (string)epGroup[EDGE_LABELING].Value;

      if (edgeLabelingName != EDGE_LABELING_NONE) {
        if (edgeLabelModelName != EDGE_LABEL_MODEL_AS_IS) {
          ApplyModelToIGraph(graph, CreateModelParameters(edgeLabelingName, edgeLabelModelName));
        }
        SetupPreferredLabelPlacement(graph);
      }
      bool useDirectedness = (bool) Handler.GetValue(EDGE_SETTINGS, CONSIDER_EDGE_DIRECTION);
      bool useThickness = (bool) Handler.GetValue(EDGE_SETTINGS, CONSIDER_EDGE_THICKNESS);

      if (useDirectedness) {
        graph.MapperRegistry.CreateDelegateMapper<IEdge, double>(HierarchicLayout.EdgeDirectednessDpKey, edge => {
          IArrowOwner style = edge.Style as IArrowOwner;
          if (style == null) {
            return 0;
          }
          if (style.SourceArrow == Arrows.None && style.TargetArrow != Arrows.None) {
            return 1;
          } else if (style.SourceArrow != Arrows.None && style.TargetArrow == Arrows.None) {
            return 1;
          } else {
            return 0;
          }
        });
      }
      if (useThickness) {
        graph.MapperRegistry.CreateDelegateMapper<IEdge, double>(HierarchicLayout.EdgeThicknessDpKey, edge => {
          var pes = edge.Style as PolylineEdgeStyle;
          if (pes != null) {
            return pes.Pen.Thickness;
          }
          var aes = edge.Style as ArcEdgeStyle;
          if (aes != null) {
            return aes.Pen.Thickness;
          }
          return 1;
        });
      }
      var foldingView = graph.GetFoldingView();
      bool recursiveRouting = foldingView != null &&
                              Handler.GetValue(EDGE_SETTINGS, RECURSIVE_EDGE_ROUTING) != RECURSIVE_EDGE_ROUTING_OFF;
      if (recursiveRouting) {
        graph.MapperRegistry.CreateDelegateMapper(HierarchicLayout.FolderNodesDpKey, node => foldingView.IsInFoldingState(node));
      }
      try {
        await base.StartWithIGraph(graph, newContext);
      } finally {
        if (useDirectedness) graph.MapperRegistry.RemoveMapper(HierarchicLayout.EdgeDirectednessDpKey);
        if (useThickness) graph.MapperRegistry.RemoveMapper(HierarchicLayout.EdgeThicknessDpKey);
        if (recursiveRouting) graph.MapperRegistry.RemoveMapper(HierarchicLayout.FolderNodesDpKey);
      }
    }
    
    private ILabelModelParameter CreateModelParameters(string edgeLabelingName, string edgeLabelModelName) {
      if (edgeLabelModelName == EDGE_LABEL_MODEL_BEST) {
        if (edgeLabelingName == EDGE_LABELING_GENERIC) {
          edgeLabelModelName = EDGE_LABEL_MODEL_SIDE_SLIDER;
        } else if (edgeLabelingName == EDGE_LABELING_HIERARCHIC) {
          edgeLabelModelName = EDGE_LABEL_MODEL_FREE;
        }
      }
      switch (edgeLabelModelName) {
        case EDGE_LABEL_MODEL_CENTER_SLIDER:
          return new EdgeSegmentLabelModel { AutoRotation = false }
            .CreateParameterFromSource(0, 0.5, EdgeSides.OnEdge);
        case EDGE_LABEL_MODEL_SIDE_SLIDER:
          return new EdgeSegmentLabelModel { AutoRotation = false }
            .CreateParameterFromSource(0, 0.5, EdgeSides.LeftOfEdge);
        case EDGE_LABEL_MODEL_FREE:
        default:
          return new FreeEdgeLabelModel().CreateDefaultParameter();
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

    private void ClearPreferredLabelPlacement(IGraph graph) {
      IMapperRegistry registry = graph.MapperRegistry;
      registry.RemoveMapper(LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey);
      if (originalMapper != null) {
        registry.AddMapper(LayoutGraphAdapter.EdgeLabelLayoutPreferredPlacementDescriptorDpKey, originalMapper);
        originalMapper = null;
      }
    }


    internal class SelectedNodesDP : DataProviderAdapter
    {
      private readonly LayoutModule module;

      public SelectedNodesDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected((Node)o);
      }
    }

    #endregion
  }
}
