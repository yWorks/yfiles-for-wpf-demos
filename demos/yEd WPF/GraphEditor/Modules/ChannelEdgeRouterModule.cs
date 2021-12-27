/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="ChannelEdgeRouter"/>.
  /// </summary>
  public class ChannelEdgeRouterModule : LayoutModule
  {
    #region configuration constants

    private const string TOP_LEVEL = "TOP_LEVEL";
    private const string NAME = "CHANNEL_EDGE_ROUTER";
    private const string PATHFINDER = "PATHFINDER";
    private const string SCOPE = "SCOPE";
    private const string SCOPE_ALL_EDGES = "SCOPE_ALL_EDGES";
    private const string SCOPE_SELECTED_EDGES = "SCOPE_SELECTED_EDGES";
    private const string SCOPE_AT_SELECTED_NODES = "SCOPE_AT_SELECTED_NODES";

    private const string LAYOUT_OPTIONS = "LAYOUT_OPTIONS";
    private const string COST = "COST";
    private const string EDGE_CROSSING_COST = "EDGE_CROSSING_COST";
    private const string NODE_CROSSING_COST = "NODE_CROSSING_COST";
    private const string BEND_COST = "BEND_COST_FACTOR";
    private const string MINIMUM_DISTANCE = "MINIMUM_DISTANCE";
    private const string ACTIVATE_GRID_ROUTING = "ACTIVATE_GRID_ROUTING";
    private const string GRID_SPACING = "GRID_SPACING";
    private const string ORTHOGONAL_PATTERN_PATH_FINDER = "ORTHOGONAL_PATTERN_PATH_FINDER";
    private const string ORTHOGONAL_SHORTESTPATH_PATH_FINDER = "ORTHOGONAL_SHORTESTPATH_PATH_FINDER";

    private static readonly List<string> scopes = new List<string>();
    private static readonly List<string> pathFinderList = new List<string>();

    static ChannelEdgeRouterModule() {
      scopes.Add(SCOPE_ALL_EDGES);
      scopes.Add(SCOPE_SELECTED_EDGES);
      scopes.Add(SCOPE_AT_SELECTED_NODES);
      pathFinderList.Add(ORTHOGONAL_PATTERN_PATH_FINDER);
      pathFinderList.Add(ORTHOGONAL_SHORTESTPATH_PATH_FINDER);
    }

    #endregion

    private ChannelEdgeRouter router;

    /// <summary>
    /// Create new instance
    /// </summary>
    public ChannelEdgeRouterModule() : base(NAME) {}

    ///<inheritdoc/>
    protected override void SetupHandler() {
      createRouter();
      OptionGroup toplevelGroup = Handler.AddGroup(TOP_LEVEL);
      //the toplevel group will show neither in Table view nor in dialog view explicitely
      toplevelGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      toplevelGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] = DefaultEditorFactory.RenderingHints.Invisible;
      OptionGroup layoutGroup = toplevelGroup.AddGroup(LAYOUT_OPTIONS);
      OptionGroup costGroup = toplevelGroup.AddGroup(COST);
      if(router.PathFinderStrategy is OrthogonalPatternEdgeRouter) {
        OrthogonalPatternEdgeRouter oper = (OrthogonalPatternEdgeRouter)router.PathFinderStrategy;
        layoutGroup.AddList(PATHFINDER, pathFinderList, ORTHOGONAL_PATTERN_PATH_FINDER);
        layoutGroup.AddList(SCOPE, scopes, SCOPE_ALL_EDGES);
        layoutGroup.AddDouble(MINIMUM_DISTANCE, oper.MinimumDistance);
        layoutGroup.AddBool(ACTIVATE_GRID_ROUTING, oper.GridRouting);
        layoutGroup.AddDouble(GRID_SPACING, oper.GridSpacing, 2.0, Double.MaxValue);

        ConstraintManager cm = new ConstraintManager(Handler);
        cm.SetEnabledOnValueEquals(layoutGroup[ACTIVATE_GRID_ROUTING], true, layoutGroup[GRID_SPACING]);

        costGroup.AddDouble(BEND_COST, oper.BendCost);
        cm.SetEnabledOnValueEquals(layoutGroup[PATHFINDER], ORTHOGONAL_PATTERN_PATH_FINDER, costGroup[BEND_COST]);
        costGroup.AddDouble(EDGE_CROSSING_COST, oper.EdgeCrossingCost);
        cm.SetEnabledOnValueEquals(layoutGroup[PATHFINDER], ORTHOGONAL_PATTERN_PATH_FINDER, costGroup[EDGE_CROSSING_COST]);
        costGroup.AddDouble(NODE_CROSSING_COST, oper.NodeCrossingCost);
        cm.SetEnabledOnValueEquals(layoutGroup[PATHFINDER], ORTHOGONAL_PATTERN_PATH_FINDER, costGroup[NODE_CROSSING_COST]);
      }
      else {
        layoutGroup.AddList(PATHFINDER, pathFinderList, ORTHOGONAL_PATTERN_PATH_FINDER);
        layoutGroup.AddList(SCOPE, scopes, SCOPE_ALL_EDGES);
        layoutGroup.AddDouble(MINIMUM_DISTANCE, 10);
        layoutGroup.AddBool(ACTIVATE_GRID_ROUTING, true);
        layoutGroup.AddDouble(GRID_SPACING, 20);

        ConstraintManager cm = new ConstraintManager(Handler);
        cm.SetEnabledOnValueEquals(layoutGroup[ACTIVATE_GRID_ROUTING], true, layoutGroup[GRID_SPACING]);

        costGroup.AddDouble(BEND_COST, 1);
        cm.SetEnabledOnValueEquals(layoutGroup[PATHFINDER], ORTHOGONAL_PATTERN_PATH_FINDER, costGroup[BEND_COST]);
        costGroup.AddDouble(EDGE_CROSSING_COST, 5);
        cm.SetEnabledOnValueEquals(layoutGroup[PATHFINDER], ORTHOGONAL_PATTERN_PATH_FINDER, costGroup[EDGE_CROSSING_COST]);
        costGroup.AddDouble(NODE_CROSSING_COST, 50);
        cm.SetEnabledOnValueEquals(layoutGroup[PATHFINDER], ORTHOGONAL_PATTERN_PATH_FINDER, costGroup[NODE_CROSSING_COST]);
      }
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      OptionGroup layoutGroup = (OptionGroup) Handler.GetGroupByName(TOP_LEVEL).GetGroupByName(LAYOUT_OPTIONS);
      OptionGroup costGroup = (OptionGroup) Handler.GetGroupByName(TOP_LEVEL).GetGroupByName(COST);
      ILayoutAlgorithm pathFinder;
        OrthogonalPatternEdgeRouter orthogonalPatternEdgeRouter = new OrthogonalPatternEdgeRouter();
        orthogonalPatternEdgeRouter.AffectedEdgesDpKey = ChannelEdgeRouter.AffectedEdgesDpKey;
        orthogonalPatternEdgeRouter.MinimumDistance = (double)layoutGroup[MINIMUM_DISTANCE].Value;
        orthogonalPatternEdgeRouter.GridRouting = (bool) layoutGroup[ACTIVATE_GRID_ROUTING].Value;
        orthogonalPatternEdgeRouter.GridSpacing = (double) layoutGroup[GRID_SPACING].Value;

        orthogonalPatternEdgeRouter.BendCost = (double) costGroup[BEND_COST].Value;
        orthogonalPatternEdgeRouter.EdgeCrossingCost = (double) costGroup[EDGE_CROSSING_COST].Value;
        orthogonalPatternEdgeRouter.NodeCrossingCost = (double) costGroup[NODE_CROSSING_COST].Value;

        //disable edge overlap costs when Edge distribution will run afterwards anyway
        orthogonalPatternEdgeRouter.EdgeOverlapCost = 0.0;
        pathFinder = orthogonalPatternEdgeRouter;
     
      router.PathFinderStrategy = pathFinder;

      OrthogonalSegmentDistributionStage segmentDistributionStage = new OrthogonalSegmentDistributionStage();
      segmentDistributionStage.AffectedEdgesDpKey = ChannelEdgeRouter.AffectedEdgesDpKey;
      segmentDistributionStage.PreferredDistance = (double) layoutGroup[MINIMUM_DISTANCE].Value;
      segmentDistributionStage.GridRouting = (bool) layoutGroup[ACTIVATE_GRID_ROUTING].Value;
      segmentDistributionStage.GridSpacing = (double) layoutGroup[GRID_SPACING].Value;

      router.EdgeDistributionStrategy = segmentDistributionStage;

      LayoutAlgorithm = router;
    }

    private IDataProvider scopeDP;
    
    private class PredicateDP: DataProviderAdapter
    {
      private readonly Predicate<object> predicate;


      public PredicateDP(Predicate<object> predicate) {
        this.predicate = predicate;
      }

      public override bool GetBool(object dataHolder) {
        return predicate(dataHolder);
      }
    }

    protected override void PerformPreLayout() {
      base.PerformPreLayout();
      OptionGroup layoutGroup = (OptionGroup) Handler.GetGroupByName(TOP_LEVEL).GetGroupByName(LAYOUT_OPTIONS);
      if(layoutGroup[SCOPE].Value.Equals(SCOPE_ALL_EDGES)) {
        scopeDP = new PredicateDP(delegate { return true; });
      }
      else if (layoutGroup[SCOPE].Value.Equals(SCOPE_SELECTED_EDGES)) {
        scopeDP = new PredicateDP(delegate(object dataholder) { return IsSelected((Edge)dataholder); });
      }
      else {
        scopeDP = new PredicateDP(delegate(object dataholder) {
                                    Edge edge = (Edge) dataholder;                          
          return IsSelected(edge.Source) || IsSelected(edge.Target);
                                  });
      }
      CurrentLayoutGraph.AddDataProvider(ChannelEdgeRouter.AffectedEdgesDpKey, scopeDP);
    }

    protected override void PerformPostLayout() {
      base.PerformPostLayout();
      CurrentLayoutGraph.RemoveDataProvider(ChannelEdgeRouter.AffectedEdgesDpKey);
    }

    private void createRouter() {
      if (router != null) {
        return;
      }
      router = new ChannelEdgeRouter();
    }
  }
}
