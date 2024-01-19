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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Router;
using yWorks.Controls;

namespace Demo.yFiles.Layout.Configurations 
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("BusRouter")]
  public class BusEdgeRouterConfig : LayoutConfiguration {

    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public BusEdgeRouterConfig() {
      var router = new BusRouter();
      
      ScopeItem = EnumScope.All;
      BusesItem = EnumBuses.Label;
      GridEnabledItem = router.GridRouting;
      GridSpacingItem = router.GridSpacing;
      MinimumDistanceToNodesItem = router.MinimumDistanceToNode;
      MinimumDistanceToEdgesItem = router.MinimumDistanceToEdge;

      PreferredBackboneSegmentCountItem = 1;
      MinimumBackboneSegmentLengthItem = router.MinimumBackboneSegmentLength;

      CrossingCostItem = router.CrossingCost;
      CrossingReroutingItem = router.Rerouting;
      MinimumBusConnectionsCountItem = 6;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var router = new BusRouter();
      
      switch (ScopeItem) {
        case EnumScope.All:
          router.Scope = Scope.RouteAllEdges;
          break;
        case EnumScope.Partial:
        case EnumScope.Subset:
        case EnumScope.SubsetBus:
          router.Scope = Scope.RouteAffectedEdges;
          break;
        default:
          router.Scope = Scope.RouteAllEdges;
          break;
      }
      router.GridRouting = GridEnabledItem;
      router.GridSpacing = GridSpacingItem;
      router.MinimumDistanceToNode = MinimumDistanceToNodesItem;
      router.MinimumDistanceToEdge = MinimumDistanceToEdgesItem;
      router.PreferredBackboneSegmentCount = PreferredBackboneSegmentCountItem;
      router.MinimumBackboneSegmentLength = MinimumBackboneSegmentLengthItem;
      router.MinimumBusConnectionsCount = MinimumBusConnectionsCountItem;
      router.CrossingCost = CrossingCostItem;
      router.Rerouting = CrossingReroutingItem;

      if (ScopeItem == EnumScope.Partial) {
        return new HideNonOrthogonalEdgesStage(router);
      }

      return router;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new BusRouterData();
      var graph = graphControl.Graph;
      var graphSelection = graphControl.Selection;
      var scopePartial = ScopeItem == EnumScope.Partial;

      var busIds = layoutData.EdgeDescriptors.Mapper;

      foreach (var edge in graph.Edges) {
        var isFixed = scopePartial &&
                      !graphSelection.IsSelected(edge.GetSourceNode()) &&
                      !graphSelection.IsSelected(edge.GetTargetNode());
        var id = GetBusId(edge, BusesItem);
        var descriptor = new BusDescriptor(id, isFixed) { RoutingPolicy = RoutingPolicyItem };
        busIds[edge] = descriptor;
      }

      HashSet<object> selectedIds;

      switch (ScopeItem) {
        case EnumScope.Subset:
          layoutData.AffectedEdges.Delegate = graphSelection.IsSelected;
          break;
        case EnumScope.SubsetBus:
          selectedIds = new HashSet<object>(graphSelection
                                            .SelectedEdges
                                            .Select(edge => busIds[edge].BusId));
          layoutData.AffectedEdges.Delegate = edge => selectedIds.Contains(busIds[edge].BusId);
          break;
        case EnumScope.Partial:
          selectedIds = new HashSet<object>(graphSelection
                                            .SelectedNodes
                                            .SelectMany(node => graph.EdgesAt(node))
                                            .Select(edge => busIds[edge].BusId));

          layoutData.AffectedEdges.Delegate = edge => selectedIds.Contains(busIds[edge].BusId);

          var hideNonOrthogonalEdgesLayoutData = new GenericLayoutData();
          hideNonOrthogonalEdgesLayoutData.AddItemCollection(HideNonOrthogonalEdgesStage.SelectedNodesDpKey).Source =
              graphSelection.SelectedNodes;

          return layoutData.CombineWith(hideNonOrthogonalEdgesLayoutData);
      }

      return layoutData;
    }

    private static object GetBusId(IEdge e, EnumBuses busDetermination) {
      switch (busDetermination) {
        case EnumBuses.Label:
          return e.Labels.Count > 0 ? e.Labels[0].Text : String.Empty;
        default:
          return SingleBusId;
      }
    }

    private static readonly object SingleBusId = new object();

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;       
    
    [Label("Layout")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LayoutGroup;

    [Label("Backbone Selection")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object SelectionGroup;

    [Label("Routing and Recombination")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object RoutingGroup;
    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming

    public enum EnumScope {
      All, Subset, SubsetBus, Partial  
    }

    public enum EnumBuses {
      Single, Label, Tag
    }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionText {
      get {
        return "<Paragraph>Orthogonal bus-style edge routing combines the (likely confusing) mass of edges in parts "
               +"of a diagram where each node is connected to each other node in a concise, orthogonal tree-like structure. "
               + "This algorithm does not change the positions of the nodes.</Paragraph>"
               +"<Paragraph>The algorithm aims to find routes where all edge paths share preferably long segments. On those long line segments "
               +"ideally all but the first and last segments of all edge paths are drawn on top of each other, with short "
               +"connections branching off to the nodes. The short connections bundle the respective first or last segments of a "
               +"node's incident edges.</Paragraph>";
      }
    } 

    [Label("Scope")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(EnumScope.All)]
    [EnumValue("All Edges", EnumScope.All)]
    [EnumValue("Reroute to Selected Nodes",EnumScope.Partial)]
    [EnumValue("Buses of Selected Edges",EnumScope.SubsetBus)]
    [EnumValue("Selected Edges",EnumScope.Subset)]
    public EnumScope ScopeItem { get; set; }

    [Label("Bus Membership")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(EnumBuses.Label)]
    [EnumValue("Single Bus", EnumBuses.Single)]
    [EnumValue("Defined by First Label", EnumBuses.Label)]
    [EnumValue("Defined by User Tag", EnumBuses.Tag)]
    public EnumBuses BusesItem { get; set; }

    [Label("Route on Grid")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(false)]
    public bool GridEnabledItem { get; set; }

    [Label("Grid Spacing")]
    [OptionGroup("LayoutGroup", 40)]
    [DefaultValue(10)]
    [MinMax(Min = 2, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int GridSpacingItem { get; set; }

    public bool ShouldDisableGridSpacingItem {
      get { return !GridEnabledItem; }
    }

    [Label("Minimum Node Distance")]
    [OptionGroup("LayoutGroup", 50)]
    [DefaultValue(10)]
    [MinMax(Min = 0, Max = 100)]    
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumDistanceToNodesItem { get; set; }

    [Label("Minimum Edge Distance")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(5)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumDistanceToEdgesItem { get; set; }
    
    [Label("Preferred Segment Count")]
    [OptionGroup("SelectionGroup", 10)]
    [DefaultValue(1)]
    [MinMax(Min = 1, Max = 10)]    
    [ComponentType(ComponentTypes.Slider)]
    public int PreferredBackboneSegmentCountItem { get; set; }

    [Label("Minimum Segment Length")]
    [OptionGroup("SelectionGroup", 20)]
    [DefaultValue(100.0)]
    [MinMax(Min = 1, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumBackboneSegmentLengthItem { get; set; }

    [Label("Routing Policy")]
    [OptionGroup("RoutingGroup", 5)]
    [DefaultValue(RoutingPolicy.Always)]
    [EnumValue("Always", RoutingPolicy.Always)]
    [EnumValue("Path As Needed", RoutingPolicy.PathAsNeeded)]
    public RoutingPolicy RoutingPolicyItem { get; set; }
    
    [Label("Crossing Cost")]
    [OptionGroup("RoutingGroup", 10)]
    [DefaultValue(1.0d)]
    [MinMax(Min = 1, Max = 50)]
    [ComponentType(ComponentTypes.Slider)]
    public double CrossingCostItem { get; set; }

    [Label("Reroute Crossing Edges")]
    [OptionGroup("RoutingGroup", 20)]
    [DefaultValue(false)]
    public bool CrossingReroutingItem { get; set; }

    [Label("Minimum Bus Connections Count")]
    [OptionGroup("RoutingGroup", 30)]
    [DefaultValue(6)]
    [MinMax(Min = 1, Max = 20)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumBusConnectionsCountItem { get; set; }    

    private sealed class HideNonOrthogonalEdgesStage : LayoutStageBase
    {
      public HideNonOrthogonalEdgesStage(ILayoutAlgorithm layout) : base(layout) { }

      public static readonly NodeDpKey<bool> SelectedNodesDpKey =
          new NodeDpKey<bool>(typeof(HideNonOrthogonalEdgesStage), "BusEdgeRouterConfig.SELECTED_NODES_DP_KEY");

      public override void ApplyLayout(LayoutGraph graph) {
        var affectedEdges = graph.GetDataProvider(BusRouter.DefaultAffectedEdgesDpKey);
        var selectedNodes = graph.GetDataProvider(SelectedNodesDpKey);
        var hider = new LayoutGraphHider(graph);
        var hiddenEdges = new HashSet<Edge>();
        foreach (var edge in graph.Edges) {
          if (affectedEdges.GetBool(edge) &&
              selectedNodes != null &&
              !selectedNodes.GetBool(edge.Source) &&
              !selectedNodes.GetBool(edge.Target)) {
            var path = graph.GetPath(edge).ToArray();
            for (var i = 1; i < path.Length; i++) {
              var p1 = path[i - 1];
              var p2 = path[i];
              if (Math.Abs(p1.X - p2.X) >= 0.0001 && Math.Abs(p1.Y - p2.Y) >= 0.0001) {
                hiddenEdges.Add(edge);
              }
            }
          }
        }
        foreach (var edge in hiddenEdges) {
          hider.Hide(edge);
        }

        ApplyLayoutCore(graph);

        hider.UnhideEdges();
      }
    }
  }
}
