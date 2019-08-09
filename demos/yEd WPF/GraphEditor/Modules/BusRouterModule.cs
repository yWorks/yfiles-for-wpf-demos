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
using System.Linq;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout.Router;

// ReSharper disable InconsistentNaming

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="BusRouter"/>.
  /// </summary>
  public class BusRouterModule : LayoutModule
  {
    #region configuration constants

    private const string NAME = "BUS_ROUTER";


    private const string GROUP_LAYOUT = "GROUP_LAYOUT";
    private const string GROUP_SELECTION = "GROUP_SELECTION";
    private const string GROUP_ROUTING = "GROUP_ROUTING";

    private const string ALL = "ALL";
    private const string BUSES = "BUSES";
    private const string LABEL = "LABEL";
    private const string TAG = "TAG";
    private const string CUSTOM = "CUSTOM";

    private const string CROSSING_COST = "CROSSING_COST";
    private const string CROSSING_REROUTING = "CROSSING_REROUTING";
    private const string GRID_ENABLED = "GRID_ENABLED";
    private const string GRID_SPACING = "GRID_SPACING";
    private const string MINIMUM_CONNECTIONS_COUNT = "MINIMUM_CONNECTIONS_COUNT";
    private const string MINIMUM_BACKBONE_LENGTH = "MINIMUM_BACKBONE_LENGTH";
    private const string MIN_DISTANCE_TO_EDGES = "MIN_DISTANCE_TO_EDGES";
    private const string MIN_DISTANCE_TO_NODES = "MIN_DISTANCE_TO_NODES";
    private const string PREFERRED_BACKBONE_COUNT = "PREFERRED_BACKBONE_COUNT";
    private const string PARTIAL = "PARTIAL";
    private const string SCOPE = "SCOPE";
    private const string SINGLE = "SINGLE";
    private const string SUBSET = "SUBSET";
    private const string SUBSET_BUS = "SUBSET_BUS";

    #endregion

    private BusRouter router;

    /// <summary>
    /// Create new instance
    /// </summary>
    public BusRouterModule() : base(NAME) {}


    ///<inheritdoc/>
    protected override void SetupHandler() {
      createRouter();
      OptionGroup layoutGroup = Handler.AddGroup(GROUP_LAYOUT);

      ConstraintManager cm = new ConstraintManager(Handler);

      //Layout options
      layoutGroup.AddList(SCOPE, new[] { ALL, SUBSET_BUS, SUBSET, PARTIAL }, ALL);
      layoutGroup.AddList(BUSES, new[] {SINGLE, LABEL, TAG, CUSTOM}, SINGLE);
      IOptionItem gridEnabledItem = layoutGroup.AddBool(GRID_ENABLED, router.GridRouting);
      IOptionItem gridSpacingItem = layoutGroup.AddInt(GRID_SPACING, router.GridSpacing, 1, int.MaxValue);
      layoutGroup.AddInt(MIN_DISTANCE_TO_NODES, router.MinimumDistanceToNode, 1, int.MaxValue);
      layoutGroup.AddInt(MIN_DISTANCE_TO_EDGES, router.MinimumDistanceToEdge, 1, int.MaxValue);
      cm.SetEnabledOnValueEquals(gridEnabledItem, true, gridSpacingItem);

      //Selection options
      OptionGroup selectionGroup = Handler.AddGroup(GROUP_SELECTION);
      selectionGroup.AddInt(PREFERRED_BACKBONE_COUNT, router.PreferredBackboneSegmentCount, 1, int.MaxValue);
      selectionGroup.AddDouble(MINIMUM_BACKBONE_LENGTH, router.MinimumBackboneSegmentLength, 1, double.MaxValue);

      //Routing options
      OptionGroup routingGroup = Handler.AddGroup(GROUP_ROUTING);
      routingGroup.AddDouble(CROSSING_COST, router.CrossingCost, 0, double.MaxValue);
      routingGroup.AddBool(CROSSING_REROUTING, router.Rerouting);
      routingGroup.AddInt(MINIMUM_CONNECTIONS_COUNT, router.MinimumBusConnectionsCount, 0, int.MaxValue);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      OptionGroup layoutGroup = Handler.GetGroupByName(GROUP_LAYOUT);
      router.Scope = (string) layoutGroup[SCOPE].Value == ALL ? Scope.RouteAllEdges : Scope.RouteAffectedEdges;
      router.GridRouting = (bool) layoutGroup[GRID_ENABLED].Value;
      router.GridSpacing = (int) layoutGroup[GRID_SPACING].Value;
      router.MinimumDistanceToNode = (int) layoutGroup[MIN_DISTANCE_TO_NODES].Value;
      router.MinimumDistanceToEdge = (int) layoutGroup[MIN_DISTANCE_TO_EDGES].Value;

      OptionGroup selectionGroup = Handler.GetGroupByName(GROUP_SELECTION);
      router.PreferredBackboneSegmentCount = (int)selectionGroup[PREFERRED_BACKBONE_COUNT].Value;
      router.MinimumBackboneSegmentLength = (double)selectionGroup[MINIMUM_BACKBONE_LENGTH].Value;

      OptionGroup routingGroup = Handler.GetGroupByName(GROUP_ROUTING);
      router.MinimumBusConnectionsCount = (int)routingGroup[MINIMUM_CONNECTIONS_COUNT].Value;
      router.CrossingCost = (double)routingGroup[CROSSING_COST].Value;
      router.Rerouting = (bool)routingGroup[CROSSING_REROUTING].Value;

      LayoutAlgorithm = router;
    }

    private void createRouter() {
      if (router != null) {
        return;
      }
      router = new BusRouter();
    }

    protected override void StartWithIGraph(IGraph graph, ILookup newContext) {
      OptionGroup layoutGroup = Handler.GetGroupByName(GROUP_LAYOUT);
      string busDetermination = (string)layoutGroup[BUSES].Value;

      ISelectionModel<IModelItem> selectionModel = newContext.Lookup<ISelectionModel<IModelItem>>();

      var mapperRegistry = graph.MapperRegistry;
      var originalBusIds = mapperRegistry.GetMapper(BusRouter.EdgeDescriptorDpKey);

      IMapper<IEdge, BusDescriptor> busIds;
      if (busDetermination != CUSTOM) {
        mapperRegistry.RemoveMapper(BusRouter.EdgeDescriptorDpKey);
        busIds = mapperRegistry.CreateMapper<IEdge, BusDescriptor>(BusRouter.EdgeDescriptorDpKey);

        var scopePartial = (string) layoutGroup[SCOPE].Value == PARTIAL;
        foreach (var edge in graph.Edges) {
          bool isFixed = scopePartial 
            && !IsSelected(selectionModel, edge.GetSourceNode())
            && !IsSelected(selectionModel, edge.GetTargetNode());
          busIds[edge] = new BusDescriptor(GetBusId(edge, busDetermination), isFixed);
        }
      }
      else {
        busIds = originalBusIds 
          ?? mapperRegistry.CreateConstantMapper(BusRouter.EdgeDescriptorDpKey, new BusDescriptor(SingleBusId));
      }

      
      var originalEdgeSubsetMapper = mapperRegistry.GetMapper(BusRouter.DefaultAffectedEdgesDpKey);
      if (originalEdgeSubsetMapper != null) {
        mapperRegistry.RemoveMapper(BusRouter.DefaultAffectedEdgesDpKey);
      }
      var selectedIds = new System.Collections.Generic.HashSet<object>();
      switch((string)layoutGroup[SCOPE].Value) {
        case SUBSET:
          mapperRegistry.CreateDelegateMapper(BusRouter.DefaultAffectedEdgesDpKey, e => IsSelected(selectionModel, e));
          break;
        case SUBSET_BUS:
          foreach (var edge in graph.Edges.Where(edge => IsSelected(selectionModel, edge))) {
            selectedIds.Add(busIds[edge].BusId);
          }
          mapperRegistry.CreateDelegateMapper(BusRouter.DefaultAffectedEdgesDpKey, e => selectedIds.Contains(busIds[e].BusId));
          break;
        case PARTIAL:
          foreach (var edge in graph.Nodes.Where(node => IsSelected(selectionModel, node)).SelectMany((node)=>graph.EdgesAt(node))) {
            selectedIds.Add(busIds[edge].BusId);
          }
          mapperRegistry.CreateDelegateMapper(BusRouter.DefaultAffectedEdgesDpKey, e => selectedIds.Contains(busIds[e].BusId));
          break;
      }

      try {
        base.StartWithIGraph(graph, newContext);
      } finally {
        mapperRegistry.RemoveMapper(BusRouter.EdgeDescriptorDpKey);
        if (originalBusIds != null) {
          mapperRegistry.AddMapper(BusRouter.EdgeDescriptorDpKey, originalBusIds);
        }
        mapperRegistry.RemoveMapper(BusRouter.DefaultAffectedEdgesDpKey);
        if (originalEdgeSubsetMapper != null) {
          mapperRegistry.AddMapper(BusRouter.DefaultAffectedEdgesDpKey, originalEdgeSubsetMapper);
        }
      }
    }

    /// <summary>
    /// Check whether the specified model item has been selected in the underlying
    /// IGraph.
    /// </summary>
    private static bool IsSelected(ISelectionModel<IModelItem> selectionModel, IModelItem item) {
      return selectionModel != null && selectionModel.IsSelected(item);
    }

    private static object GetBusId(IEdge e, string busDetermination) {
      switch (busDetermination) {
        case LABEL:
          return e.Labels.Count > 0 ? e.Labels[0].Text : String.Empty;
        case TAG:
          return e.Tag;
        default:
          return SingleBusId;
      }
    }

    private static readonly object SingleBusId = new object();
  }

}