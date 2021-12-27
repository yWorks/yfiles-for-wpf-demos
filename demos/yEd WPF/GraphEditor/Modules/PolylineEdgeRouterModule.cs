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
using Demo.yFiles.Option.Handler;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="EdgeRouter"/>.
  /// </summary>
  public class PolylineEdgeRouterModule : LayoutModule
  {
    #region configuration constants

    // option handler title
    private const string POLYLINE_EDGE_ROUTER = "POLYLINE_EDGE_ROUTER";

    // layout settings
    private const string LAYOUT = "LAYOUT";
    private const string SCOPE = "SCOPE";
    private const string SCOPE_ALL_EDGES = "SCOPE_ALL_EDGES";
    private const string SCOPE_SELECTED_EDGES = "SCOPE_SELECTED_EDGES";
    private const string SCOPE_EDGES_AT_SELECTED_NODES = "SCOPE_EDGES_AT_SELECTED_NODES";

    private static readonly string[] scopeTypes = {
                                                   SCOPE_ALL_EDGES,
                                                   SCOPE_SELECTED_EDGES,
                                                   SCOPE_EDGES_AT_SELECTED_NODES
                                                 };

    private const string OPTIMIZATION_STRATEGY = "OPTIMIZATION_STRATEGY";
    private const string STRATEGY_BALANCED = "STRATEGY_BALANCED";
    private const string STRATEGY_MINIMIZE_BENDS = "STRATEGY_MINIMIZE_BENDS";
    private const string STRATEGY_MINIMIZE_CROSSINGS = "STRATEGY_MINIMIZE_CROSSINGS";
    private const string STRATEGY_MINIMIZE_EDGE_LENGTH = "STRATEGY_MINIMIZE_EDGE_LENGTH";

    private static readonly string[] strategies = {
                                                  STRATEGY_BALANCED,
                                                  STRATEGY_MINIMIZE_BENDS,
                                                  STRATEGY_MINIMIZE_CROSSINGS,
                                                  STRATEGY_MINIMIZE_EDGE_LENGTH
                                                };

    private const string MONOTONIC_RESTRICTION = "MONOTONIC_RESTRICTION";
    private const string MONOTONIC_NONE = "MONOTONIC_NONE";
    private const string MONOTONIC_VERTICAL = "MONOTONIC_VERTICAL";
    private const string MONOTONIC_HORIZONTAL = "MONOTONIC_HORIZONTAL";
    private const string MONOTONIC_BOTH = "MONOTONIC_BOTH";

    private static readonly Object[] monotonyFlags = {
                                                      MONOTONIC_NONE,
                                                      MONOTONIC_HORIZONTAL,
                                                      MONOTONIC_VERTICAL,
                                                      MONOTONIC_BOTH,
                                                    };

    private const string MINIMAL_DISTANCES = "MINIMAL_DISTANCES";
    private const string MINIMAL_EDGE_TO_EDGE_DISTANCE = "MINIMAL_EDGE_TO_EDGE_DISTANCE";
    private const string MINIMAL_NODE_TO_EDGE_DISTANCE = "MINIMAL_NODE_TO_EDGE_DISTANCE";
    private const string MINIMAL_NODE_CORNER_DISTANCE = "MINIMAL_NODE_CORNER_DISTANCE";
    private const string MINIMAL_FIRST_SEGMENT_LENGTH = "MINIMAL_FIRST_SEGMENT_LENGTH";
    private const string MINIMAL_LAST_SEGMENT_LENGTH = "MINIMAL_LAST_SEGMENT_LENGTH";
    private const string GRID_SETTINGS = "GRID_SETTINGS";
    private const string GRID_ENABLED = "GRID_ENABLED";
    private const string GRID_SPACING = "GRID_SPACING";
    private const string CONSIDER_NODE_LABELS = "CONSIDER_NODE_LABELS";
    private const string CONSIDER_EDGE_LABELS = "CONSIDER_EDGE_LABELS";
    private const string ENABLE_REROUTING = "ENABLE_REROUTING";
    private const string MAXIMUM_DURATION = "MAXIMUM_DURATION";

    // polyline routing settings
    private const string POLYLINE_ROUTING = "POLYLINE_ROUTING";
    private const string ENABLE_POLYLINE_ROUTING = "ENABLE_POLYLINE_ROUTING";
    private const string PREFERRED_POLYLINE_SEGMENT_LENGTH = "PREFERRED_POLYLINE_SEGMENT_LENGTH";

    private static readonly IDictionary<string, MonotonicPathRestriction> monotonyEnum =
      new Dictionary<string, MonotonicPathRestriction>();

    static PolylineEdgeRouterModule() {
      monotonyEnum[MONOTONIC_NONE] = MonotonicPathRestriction.None;
      monotonyEnum[MONOTONIC_VERTICAL] = MonotonicPathRestriction.Vertical;
      monotonyEnum[MONOTONIC_HORIZONTAL] = MonotonicPathRestriction.Horizontal;
      monotonyEnum[MONOTONIC_BOTH] = MonotonicPathRestriction.Both;
    }

    #endregion

    private EdgeRouter router;

    /// <summary>
    /// Create new instance
    /// </summary>
    public PolylineEdgeRouterModule() : base(POLYLINE_EDGE_ROUTER) {}

    ///<inheritdoc/>
    protected override void SetupHandler() {
      CreateRouter();

      // tab layout
      var layoutGroup = Handler.AddGroup(LAYOUT);
      Scope scope = router.Scope;
      layoutGroup.AddList(SCOPE, scopeTypes, scopeTypes[scope == Scope.RouteAllEdges ? 0 : 1]);
      layoutGroup.AddList(OPTIMIZATION_STRATEGY, strategies, strategies[0]);
      layoutGroup.AddList(MONOTONIC_RESTRICTION, monotonyFlags, monotonyFlags[0]);
      layoutGroup.AddBool(CONSIDER_NODE_LABELS, router.ConsiderNodeLabels);
      layoutGroup.AddBool(CONSIDER_EDGE_LABELS, router.ConsiderEdgeLabels);
      layoutGroup.AddBool(ENABLE_REROUTING, router.Rerouting);
      layoutGroup.AddInt(MAXIMUM_DURATION, 30);

      // tab distances
      var distancesGroup = Handler.AddGroup(MINIMAL_DISTANCES);
      
      EdgeLayoutDescriptor descriptor = router.DefaultEdgeLayoutDescriptor;

      distancesGroup.AddDouble(MINIMAL_EDGE_TO_EDGE_DISTANCE, descriptor.MinimumEdgeToEdgeDistance);
      distancesGroup.AddDouble(MINIMAL_NODE_TO_EDGE_DISTANCE, router.MinimumNodeToEdgeDistance);
      distancesGroup.AddDouble(MINIMAL_NODE_CORNER_DISTANCE, descriptor.MinimumNodeCornerDistance);
      distancesGroup.AddDouble(MINIMAL_FIRST_SEGMENT_LENGTH, descriptor.MinimumFirstSegmentLength);
      distancesGroup.AddDouble(MINIMAL_LAST_SEGMENT_LENGTH, descriptor.MinimumLastSegmentLength);

      // tab grid
      var gridGroup = Handler.AddGroup(GRID_SETTINGS);
      Grid grid = router.Grid;
      
      var gridEnabledItem = gridGroup.AddBool(GRID_ENABLED, grid != null);
      var gridSpacingItem = gridGroup.AddDouble(GRID_SPACING, grid != null ? grid.Spacing : 10);

      
      // tab polyline routing
      var polylineGroup = Handler.AddGroup(POLYLINE_ROUTING);
      var polyLineItem = polylineGroup.AddBool(ENABLE_POLYLINE_ROUTING, true);
      var polyLineSegmentLengthItem = polylineGroup.AddDouble(PREFERRED_POLYLINE_SEGMENT_LENGTH,
                                                              router.PreferredPolylineSegmentLength);

      // some constraints to enable/disable values that depends on other values
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(gridEnabledItem, true, gridSpacingItem);
      cm.SetEnabledOnValueEquals(polyLineItem, true, polyLineSegmentLengthItem);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      EdgeLayoutDescriptor descriptor = router.DefaultEdgeLayoutDescriptor;
      OptionGroup layoutGroup = Handler.GetGroupByName(LAYOUT);
      OptionGroup gridGroup = Handler.GetGroupByName(GRID_SETTINGS);
      OptionGroup polylineGroup = Handler.GetGroupByName(POLYLINE_ROUTING);
      OptionGroup distancesGroup = Handler.GetGroupByName(MINIMAL_DISTANCES);

      string scope = (string) layoutGroup[SCOPE].Value;

      if (scope.Equals(SCOPE_ALL_EDGES)) {
        router.Scope = Scope.RouteAllEdges;
      } else {
        router.Scope = Scope.RouteAffectedEdges;
      }


      string strategy = (string) layoutGroup[OPTIMIZATION_STRATEGY].Value;
      if (strategy == STRATEGY_BALANCED) {
        descriptor.PenaltySettings = PenaltySettings.OptimizationBalanced;
      } else if (strategy == STRATEGY_MINIMIZE_BENDS) {
        descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeBends;
      } else if (strategy == STRATEGY_MINIMIZE_EDGE_LENGTH) {
        descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeLengths;
      } else {
        descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeCrossings;
      }

      string monotonyFlag = (string) layoutGroup[MONOTONIC_RESTRICTION].Value;
      if (monotonyFlag == MONOTONIC_HORIZONTAL) {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Horizontal;
      } else if (monotonyFlag == MONOTONIC_VERTICAL) {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Vertical;
      } else {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.None;
      }

      descriptor.MinimumEdgeToEdgeDistance = (double) distancesGroup[MINIMAL_EDGE_TO_EDGE_DISTANCE].Value;
      router.MinimumNodeToEdgeDistance = (double) distancesGroup[MINIMAL_NODE_TO_EDGE_DISTANCE].Value;
      descriptor.MinimumNodeCornerDistance = (double) distancesGroup[MINIMAL_NODE_CORNER_DISTANCE].Value;
      descriptor.MinimumFirstSegmentLength = (double) distancesGroup[MINIMAL_FIRST_SEGMENT_LENGTH].Value;
      descriptor.MinimumLastSegmentLength = (double) distancesGroup[MINIMAL_LAST_SEGMENT_LENGTH].Value;

      if ((bool) gridGroup[GRID_ENABLED].Value) {
        double gridSpacing = (double) gridGroup[GRID_SPACING].Value;
        router.Grid = new Grid(0, 0, gridSpacing);
      } else {
        router.Grid = null;
      }

      router.ConsiderNodeLabels = (bool) layoutGroup[CONSIDER_NODE_LABELS].Value;
      router.ConsiderEdgeLabels = (bool) layoutGroup[CONSIDER_EDGE_LABELS].Value;
      router.Rerouting = (bool) layoutGroup[ENABLE_REROUTING].Value;

      router.PolylineRouting = (bool) polylineGroup[ENABLE_POLYLINE_ROUTING].Value;
      router.PreferredPolylineSegmentLength = (double) polylineGroup[PREFERRED_POLYLINE_SEGMENT_LENGTH].Value;
      router.MaximumDuration =  (int) layoutGroup[MAXIMUM_DURATION].Value*1000;
      LayoutAlgorithm = router;
    }

    private void CreateRouter() {
      if (router != null) {
        return;
      }
      router = new EdgeRouter();
      //ConfigureLayout();
    }
  }
}
