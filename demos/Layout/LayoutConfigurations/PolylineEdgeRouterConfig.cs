/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Demo.Toolkit.OptionHandler;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Router;
using BusDescriptor = yWorks.Layout.Router.BusDescriptor;

namespace yWorks.Demo.Layout.Configurations
{
  /// <summary>
  ///   Configuration options for the <see cref="EdgeRouter"/> algorithm.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("EdgeRouter")]
  public class PolylineEdgeRouterConfig : LayoutConfiguration
  {
    /// <summary>
    ///   Setup default values for various configuration parameters.
    /// </summary>
    public PolylineEdgeRouterConfig() {
      var router = new EdgeRouter();

      ScopeItem = Scope.RouteAllEdges;
      OptimizationStrategyItem = EnumStrategies.Balanced;
      MonotonicRestrictionItem = EnumMonotonyFlags.None;
      EnableReroutingItem = router.Rerouting;
      StopDurationItem = 30;

      var descriptor = router.DefaultEdgeDescriptor;
      MinimumEdgeDistanceItem = descriptor.MinimumEdgeDistance;
      MinimumNodeToEdgeDistanceItem = router.MinimumNodeToEdgeDistance;
      MinimumNodeCornerDistanceItem = descriptor.MinimumNodeCornerDistance;
      MinimumFirstSegmentLengthItem = descriptor.MinimumFirstSegmentLength;
      MinimumLastSegmentLengthItem = descriptor.MinimumLastSegmentLength;
      RoutingItem = EdgeRouterScope.Path;

      var gridSpacing = router.GridSpacing;
      GridEnabledItem = gridSpacing > 0;
      GridSpacingItem = gridSpacing < 2 ? 2 : 100 < gridSpacing ? 100 : gridSpacing;

      EdgeRoutingStyleItem = RoutingStyle.Orthogonal;
      PreferredOctilinearSegmentLengthItem = router.DefaultEdgeDescriptor.PreferredOctilinearSegmentLength;
      MaximumOctilinearSegmentRatioItem = router.DefaultEdgeDescriptor.MaximumOctilinearSegmentRatio;
      SourceConnectionStyleItem = descriptor.SourceCurveConnectionStyle;
      TargetConnectionStyleItem = descriptor.TargetCurveConnectionStyle;
      CurveUTurnSymmetryItem = descriptor.CurveUTurnSymmetry;
      CurveShortcutsItem = descriptor.CurveShortcuts;
      PortSidesItem = PortSides.Any;

      NodeLabelingItem = router.NodeLabelPlacement;
      EdgeLabelingItem = router.EdgeLabelPlacement;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;

      AutomaticEdgeGroupingItem = true;
      MinimumBackboneSegmentLengthItem = 100;
      AllowMultipleBackboneSegmentsItem = true;
    }

    /// <inheritdoc/>
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var router = new EdgeRouter();

      router.MinimumNodeToEdgeDistance = MinimumNodeToEdgeDistanceItem;

      if (GridEnabledItem) {
        router.GridSpacing = GridSpacingItem;
      } else {
        router.GridSpacing = 0;
      }

      router.NodeLabelPlacement = NodeLabelingItem;
      router.Rerouting = EnableReroutingItem;

      // Note that CreateConfiguredLayoutData replaces the settings on the DefaultEdgeLayoutDescriptor
      // by providing a custom one for each edge.
      router.DefaultEdgeDescriptor.RoutingStyle = EdgeRoutingStyleItem;
      router.DefaultEdgeDescriptor.PreferredOctilinearSegmentLength = PreferredOctilinearSegmentLengthItem;
      router.DefaultEdgeDescriptor.MaximumOctilinearSegmentRatio = MaximumOctilinearSegmentRatioItem;
      router.DefaultEdgeDescriptor.SourceCurveConnectionStyle = SourceConnectionStyleItem;
      router.DefaultEdgeDescriptor.TargetCurveConnectionStyle = TargetConnectionStyleItem;

      if (OptimizationStrategyItem == EnumStrategies.LowQuality) {
        router.StopDuration = TimeSpan.Zero;
      } else {
        router.StopDuration = TimeSpan.FromSeconds(StopDurationItem);
      }

      var layout = new SequentialLayout(router);

      router.EdgeLabelPlacement = EdgeLabelingItem;
      if (EdgeLabelingItem == EdgeRouterEdgeLabelPlacement.Generic && ReduceAmbiguityItem) {
        router.GenericLabeling!.DefaultEdgeLabelingCosts.AmbiguousPlacementCost = 1.0;
      }

      return layout;
    }

    protected override LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutData(
        GraphControl graphControl,
        ILayoutAlgorithm layout) {
      var layoutData = new EdgeRouterData<INode, IEdge, ILabel, ILabel>();

      layoutData.EdgeDescriptors.MapperFunction = edge => {
        var descriptor = new EdgeRouterEdgeDescriptor {
            MinimumEdgeDistance = MinimumEdgeDistanceItem,
            MinimumNodeCornerDistance = MinimumNodeCornerDistanceItem,
            MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem,
            MinimumLastSegmentLength = MinimumLastSegmentLengthItem,
            PreferredOctilinearSegmentLength = PreferredOctilinearSegmentLengthItem,
            MaximumOctilinearSegmentRatio = MaximumOctilinearSegmentRatioItem,
            SourceCurveConnectionStyle = SourceConnectionStyleItem,
            TargetCurveConnectionStyle = TargetConnectionStyleItem,
            CurveUTurnSymmetry = CurveUTurnSymmetryItem,
            CurveShortcuts = CurveShortcutsItem,
            RoutingStyle = EdgeRoutingStyleItem
        };
        if (OptimizationStrategyItem == EnumStrategies.Balanced) {
          descriptor.EdgeRouterCosts = EdgeRouterCosts.BalancedOptimization;
        } else if (OptimizationStrategyItem == EnumStrategies.MinimizeBends) {
          descriptor.EdgeRouterCosts = EdgeRouterCosts.EdgeBendsOptimization;
        } else if (OptimizationStrategyItem == EnumStrategies.MinimizeEdgeLength) {
          descriptor.EdgeRouterCosts = EdgeRouterCosts.EdgeLengthsOptimization;
        } else if (OptimizationStrategyItem == EnumStrategies.LowQuality) {
          descriptor.EdgeRouterCosts = EdgeRouterCosts.LowQuality;
        } else {
          descriptor.EdgeRouterCosts = EdgeRouterCosts.EdgeCrossingsOptimization;
        }

        if (MonotonicRestrictionItem == EnumMonotonyFlags.Horizontal) {
          descriptor.MonotonicPathRestriction = MonotonicPathRestrictions.Horizontal;
        } else if (MonotonicRestrictionItem == EnumMonotonyFlags.Vertical) {
          descriptor.MonotonicPathRestriction = MonotonicPathRestrictions.Vertical;
        } else if (MonotonicRestrictionItem == EnumMonotonyFlags.Both) {
          descriptor.MonotonicPathRestriction = MonotonicPathRestrictions.Both;
        } else {
          descriptor.MonotonicPathRestriction = MonotonicPathRestrictions.None;
        }

        descriptor.MinimumEdgeDistance = MinimumEdgeDistanceItem;
        descriptor.MinimumNodeCornerDistance = MinimumNodeCornerDistanceItem;
        descriptor.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
        descriptor.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;

        if (UseIntermediatePointsItem) {
          descriptor.IntermediateRoutingPoints = edge.Bends.Select(b => new MutablePoint(b.Location.X, b.Location.Y));
        }

        return descriptor;
      };

      var selection = graphControl.Selection;
      if (ScopeItem == Scope.RouteEdgesAtAffectedNodes) {
        layoutData.Scope.IncidentNodeMapping.MapperFunction = node =>
            selection.Contains(node) ? RoutingItem : EdgeRouterScope.Ignore;
      } else if (ScopeItem == Scope.RouteAffectedEdges) {
        layoutData.Scope.EdgeMapping.MapperFunction = edge =>
            selection.Contains(edge) ? RoutingItem : EdgeRouterScope.Ignore;
      } else {
        layoutData.Scope.EdgeMapping.Constant = RoutingItem;
      }

      if (PortSidesItem != PortSides.Any) {
        EdgePortCandidates candidates;
        if (PortSidesItem == PortSides.LeftRight) {
          candidates = new EdgePortCandidates().AddFreeCandidate(yWorks.Layout.PortSides.Right)
              .AddFreeCandidate(yWorks.Layout.PortSides.Left);
        } else {
          candidates = new EdgePortCandidates().AddFreeCandidate(yWorks.Layout.PortSides.Top)
              .AddFreeCandidate(yWorks.Layout.PortSides.Bottom);
        }
        layoutData.Ports.SourcePortCandidates.Constant = candidates;
        layoutData.Ports.TargetPortCandidates.Constant = candidates;
      }

      switch (BusMembershipItem) {
        case EnumBusRouting.SingleBus:
          // All edges in a single bus
          layoutData.Buses.Add(CreateBusDescriptor()).Predicate = edge => true;
          break;
        case EnumBusRouting.ByLabel:
          var byLabel = new Dictionary<string, List<IEdge>>();
          foreach (var edge in graphControl.Graph.Edges) {
            if (edge.Labels.Count > 0) {
              var label = edge.Labels[0].Text;
              List<IEdge> list;
              if (!byLabel.TryGetValue(label, out list)) {
                list = [];
                byLabel[label] = list;
              }
              list.Add(edge);
            }
          }
          foreach (var edges in byLabel.Values) {
            // Add a bus per label. Unlabeled edges don't get grouped into a bus
            layoutData.Buses.Add(CreateBusDescriptor()).Source = edges;
          }
          break;
        case EnumBusRouting.ByColor:
          var comparer = new ColorComparer();
          var byColor = new Dictionary<Brush, List<IEdge>>(comparer);
          foreach (var edge in graphControl.Graph.Edges) {
            var brush = ((PolylineEdgeStyle) edge.Style).Pen.Brush;
            if (!comparer.Equals(brush, Brushes.Black)) {
              List<IEdge> list;
              if (!byColor.TryGetValue(brush, out list)) {
                list = [];
                byColor[brush] = list;
              }
              list.Add(edge);
            }
          }
          foreach (var edges in byColor.Values) {
            // Add a bus per color. Black edges don't get grouped into a bus
            layoutData.Buses.Add(CreateBusDescriptor()).Source = edges;
          }
          break;
      }

      return layoutData.CombineWith(CreateLabelingLayoutData(graphControl.Graph, LabelPlacementAlongEdgeItem,
          LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem));
    }

    private BusDescriptor CreateBusDescriptor() =>
        new() {
            AutomaticEdgeGrouping = AutomaticEdgeGroupingItem,
            MinimumBackboneSegmentLength = MinimumBackboneSegmentLengthItem,
            MultipleBackboneSegments = AllowMultipleBackboneSegmentsItem
        };

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup;

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LayoutGroup;

    [Label("Minimum Distances")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DistancesGroup;

    [Label("Grid")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GridGroup;

    [Label("Routing Style")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object PolylineGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup;

    [Label("Bus Routing")]
    [OptionGroup("PolylineGroup", 80)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object BusGroup;

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
    public string DescriptionText =>
        "<Paragraph>Polyline edge routing calculates polyline edge paths for a diagram's edges. " +
        "The positions of the nodes are not changed by this algorithm.</Paragraph>" +
        "<Paragraph>Edges will be routed orthogonally, that is each edge path consists of horizontal and vertical segments, or octilinear." +
        " Octilinear means that the slope of each segment of an edge path is a multiple of 45 degrees.</Paragraph>" +
        "<Paragraph>This type of edge routing is especially well suited for technical diagrams.</Paragraph>";

    public enum EnumStrategies
    {
      Balanced,
      MinimizeBends,
      MinimizeCrossings,
      MinimizeEdgeLength,
      LowQuality
    }

    public enum EnumMonotonyFlags
    {
      None,
      Horizontal,
      Vertical,
      Both
    }

    public enum EnumBusRouting
    {
      None,
      SingleBus,
      ByLabel,
      ByColor
    }

    public enum PortSides
    {
      Any,
      LeftRight,
      TopBottom
    }

    public enum Scope
    {
      RouteAllEdges,
      RouteAffectedEdges,
      RouteEdgesAtAffectedNodes
    }

    [Label("Scope")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(Scope.RouteAllEdges)]
    [EnumValue("All Edges", Scope.RouteAllEdges)]
    [EnumValue("Selected Edges", Scope.RouteAffectedEdges)]
    [EnumValue("Edges at Selected Nodes", Scope.RouteEdgesAtAffectedNodes)]
    public Scope ScopeItem { get; set; }

    [Label("Optimization Strategy")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(EnumStrategies.Balanced)]
    [EnumValue("Balanced", EnumStrategies.Balanced)]
    [EnumValue("Fewer Bends", EnumStrategies.MinimizeBends)]
    [EnumValue("Fewer Crossings", EnumStrategies.MinimizeCrossings)]
    [EnumValue("Shorter Edges", EnumStrategies.MinimizeEdgeLength)]
    [EnumValue("Low Quality and Fast", EnumStrategies.LowQuality)]
    public EnumStrategies OptimizationStrategyItem { get; set; }

    [Label("Monotonic Restriction")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(EnumMonotonyFlags.None)]
    [EnumValue("None", EnumMonotonyFlags.None)]
    [EnumValue("Horizontal", EnumMonotonyFlags.Horizontal)]
    [EnumValue("Vertical", EnumMonotonyFlags.Vertical)]
    [EnumValue("Both", EnumMonotonyFlags.Both)]
    public EnumMonotonyFlags MonotonicRestrictionItem { get; set; }

    [Label("Reroute Crossing Edges")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(false)]
    public bool EnableReroutingItem { get; set; }

    [Label("Keep Bends as Intermediate Points")]
    [OptionGroup("LayoutGroup", 65)]
    public bool UseIntermediatePointsItem { get; set; }

    public bool ShouldDisableUseIntermediatePointsItem =>
        BusMembershipItem != EnumBusRouting.None || OptimizationStrategyItem == EnumStrategies.LowQuality;

    [Label("Stop Duration (sec)")]
    [OptionGroup("LayoutGroup", 70)]
    [DefaultValue(30)]
    [MinMax(Min = 0, Max = 150)]
    [ComponentType(ComponentTypes.Slider)]
    public int StopDurationItem { get; set; }

    public bool ShouldDisableMaximumDurationItem => OptimizationStrategyItem == EnumStrategies.LowQuality;

    [Label("Routing Policy")]
    [OptionGroup("LayoutGroup", 80)]
    [DefaultValue(EdgeRouterScope.Path)]
    [EnumValue("Always", EdgeRouterScope.Path)]
    [EnumValue("Path As Needed", EdgeRouterScope.PathAsNeeded)]
    [EnumValue("Segments As Needed", EdgeRouterScope.SegmentsAsNeeded)]
    [EnumValue("Never", EdgeRouterScope.Ignore)]
    public EdgeRouterScope RoutingItem { get; set; }

    [Label("Allowed port sides")]
    [OptionGroup("LayoutGroup", 90)]
    [DefaultValue(PortSides.Any)]
    [EnumValue("Any", PortSides.Any)]
    [EnumValue("Left or Right", PortSides.LeftRight)]
    [EnumValue("Top or Bottom", PortSides.TopBottom)]
    public PortSides PortSidesItem { get; set; }

    [Label("Edge to Edge")]
    [OptionGroup("DistancesGroup", 10)]
    [DefaultValue(3.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeDistanceItem { get; set; }

    [Label("Node to Edge")]
    [OptionGroup("DistancesGroup", 20)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeToEdgeDistanceItem { get; set; }

    [Label("Port to Node Corner")]
    [OptionGroup("DistancesGroup", 30)]
    [DefaultValue(3.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeCornerDistanceItem { get; set; }

    [Label("First Segment Length")]
    [OptionGroup("DistancesGroup", 40)]
    [DefaultValue(5.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumFirstSegmentLengthItem { get; set; }

    public bool ShouldDisableMinimumFirstSegmentLengthItem =>
        (EdgeRoutingStyleItem == RoutingStyle.Curved && SourceConnectionStyleItem == CurveConnectionStyle.Organic) ||
        OptimizationStrategyItem == EnumStrategies.LowQuality;

    [Label("Last Segment Length")]
    [OptionGroup("DistancesGroup", 50)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLastSegmentLengthItem { get; set; }

    public bool ShouldDisableMinimumLastSegmentLengthItem =>
        (EdgeRoutingStyleItem == RoutingStyle.Curved && TargetConnectionStyleItem == CurveConnectionStyle.Organic) ||
        OptimizationStrategyItem == EnumStrategies.LowQuality;

    [Label("Route on Grid")]
    [OptionGroup("GridGroup", 10)]
    [DefaultValue(false)]
    public bool GridEnabledItem { get; set; }

    [Label("Grid Spacing")]
    [OptionGroup("GridGroup", 20)]
    [DefaultValue(2)]
    [MinMax(Min = 2, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double GridSpacingItem { get; set; }

    public bool ShouldDisableGridSpacingItem => !GridEnabledItem;

    [Label("Routing Style")]
    [OptionGroup("PolylineGroup", 10)]
    [DefaultValue(RoutingStyle.Orthogonal)]
    [EnumValue("Orthogonal", RoutingStyle.Orthogonal)]
    [EnumValue("Octilinear", RoutingStyle.Octilinear)]
    [EnumValue("Curved", RoutingStyle.Curved)]
    public RoutingStyle EdgeRoutingStyleItem { get; set; }

    [Label("Preferred Octilinear Corner Length")]
    [OptionGroup("PolylineGroup", 20)]
    [DefaultValue(30.0d)]
    [MinMax(Min = 5, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredOctilinearSegmentLengthItem { get; set; }

    public bool ShouldDisablePreferredOctilinearSegmentLengthItem => EdgeRoutingStyleItem != RoutingStyle.Octilinear;

    [Label("Maximum Octilinear Segment Ratio")]
    [OptionGroup("PolylineGroup", 30)]
    [DefaultValue(0.3d)]
    [MinMax(Min = 0, Max = 0.5, Step = 0.01)]
    [ComponentType(ComponentTypes.Slider)]
    public double MaximumOctilinearSegmentRatioItem { get; set; }

    public bool ShouldDisableMaximumOctilinearSegmentRatioItem => EdgeRoutingStyleItem != RoutingStyle.Octilinear;

    [Label("Curved Connection at Source")]
    [OptionGroup("PolylineGroup", 40)]
    [DefaultValue(CurveConnectionStyle.KeepPort)]
    [EnumValue("Straight", CurveConnectionStyle.KeepPort)]
    [EnumValue("Organic", CurveConnectionStyle.Organic)]
    public CurveConnectionStyle SourceConnectionStyleItem { get; set; }

    public bool ShouldDisableSourceConnectionStyleItem => EdgeRoutingStyleItem != RoutingStyle.Curved;

    [Label("Curved Connection at Target")]
    [OptionGroup("PolylineGroup", 50)]
    [DefaultValue(CurveConnectionStyle.KeepPort)]
    [EnumValue("Straight", CurveConnectionStyle.KeepPort)]
    [EnumValue("Organic", CurveConnectionStyle.Organic)]
    public CurveConnectionStyle TargetConnectionStyleItem { get; set; }

    public bool ShouldDisableTargetConnectionStyleItem => EdgeRoutingStyleItem != RoutingStyle.Curved;

    [Label("U-turn Symmetry")]
    [OptionGroup("PolylineGroup", 60)]
    [DefaultValue(0d)]
    [MinMax(Min = 0, Max = 1, Step = 0.1)]
    [ComponentType(ComponentTypes.Slider)]
    public double CurveUTurnSymmetryItem { get; set; }

    public bool ShouldDisableCurveUTurnSymmetryItem => EdgeRoutingStyleItem != RoutingStyle.Curved;

    [Label("Allow shortcuts")]
    [OptionGroup("PolylineGroup", 70)]
    [DefaultValue(false)]
    public bool CurveShortcutsItem { get; set; }

    public bool ShouldDisableCurveShortcutsItem => EdgeRoutingStyleItem != RoutingStyle.Curved;

    [Label("Bus Membership")]
    [OptionGroup("BusGroup", 10)]
    [EnumValue("No Buses", EnumBusRouting.None)]
    [EnumValue("Single Bus", EnumBusRouting.SingleBus)]
    [EnumValue("By Edge Color", EnumBusRouting.ByColor)]
    [EnumValue("By Edge Label", EnumBusRouting.ByLabel)]
    public EnumBusRouting BusMembershipItem { get; set; }

    public bool ShouldDisableBusRoutingItem => OptimizationStrategyItem == EnumStrategies.LowQuality;

    [Label("Automatic Edge Grouping")]
    [OptionGroup("BusGroup", 20)]
    [DefaultValue(true)]
    public bool AutomaticEdgeGroupingItem { get; set; }

    public bool ShouldDisableAutomaticEdgeGroupingItem => BusMembershipItem == EnumBusRouting.None;

    [Label("Minimum Backbone Segment Length")]
    [OptionGroup("BusGroup", 30)]
    [DefaultValue(100)]
    [MinMax(Min = 1, Max = 1000)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumBackboneSegmentLengthItem { get; set; }

    public bool ShouldDisableMinimumBackboneSegmentLengthItem => BusMembershipItem == EnumBusRouting.None;

    [Label("Multiple Backbone Segments")]
    [OptionGroup("BusGroup", 40)]
    [DefaultValue(true)]
    public bool AllowMultipleBackboneSegmentsItem { get; set; }

    public bool ShouldDisableAllowMultipleBackboneSegmentsItem => BusMembershipItem == EnumBusRouting.None;

    [Label("Node Labeling")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(EdgeRouterNodeLabelPlacement.Consider)]
    [EnumValue("Consider", EdgeRouterNodeLabelPlacement.Consider)]
    [EnumValue("Generic", EdgeRouterNodeLabelPlacement.Generic)]
    [EnumValue("Ignore", EdgeRouterNodeLabelPlacement.Ignore)]
    [EnumValue("Ignore Group Labels", EdgeRouterNodeLabelPlacement.IgnoreGroupLabels)]
    public EdgeRouterNodeLabelPlacement NodeLabelingItem { get; set; }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 20)]
    [DefaultValue(EdgeRouterEdgeLabelPlacement.Generic)]
    [EnumValue("Ignore", EdgeRouterEdgeLabelPlacement.Ignore)]
    [EnumValue("Integrated", EdgeRouterEdgeLabelPlacement.Integrated)]
    [EnumValue("Generic", EdgeRouterEdgeLabelPlacement.Generic)]
    [EnumValue("Consider Fixed Edge Labels", EdgeRouterEdgeLabelPlacement.ConsiderUnaffectedEdgeLabels)]
    public EdgeRouterEdgeLabelPlacement EdgeLabelingItem { get; set; }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 30)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem => EdgeLabelingItem != EdgeRouterEdgeLabelPlacement.Generic;

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal", EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal", EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical", EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem => EdgeLabelingItem == EdgeRouterEdgeLabelPlacement.Ignore;

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

    public bool ShouldDisableLabelPlacementAlongEdgeItem => EdgeLabelingItem == EdgeRouterEdgeLabelPlacement.Ignore;

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge", EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left", EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right", EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right", EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementSideOfEdgeItem => EdgeLabelingItem == EdgeRouterEdgeLabelPlacement.Ignore;

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem =>
        EdgeLabelingItem == EdgeRouterEdgeLabelPlacement.Ignore ||
        LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge;

    private sealed class ColorComparer : IEqualityComparer<Brush>
    {
      public bool Equals(Brush x, Brush y) {
        if (x is SolidColorBrush && y is SolidColorBrush) {
          return EqualityComparer<Color>.Default.Equals(((SolidColorBrush) x).Color, ((SolidColorBrush) y).Color);
        }
        return object.Equals(x, y);
      }

      public int GetHashCode(Brush obj) {
        if (obj is SolidColorBrush) {
          return ((SolidColorBrush) obj).Color.GetHashCode();
        }
        return obj.GetHashCode();
      }
    }

    public ILayoutAlgorithm CreateConfiguredLayoutPublic(GraphControl graphControl) =>
        CreateConfiguredLayout(graphControl);

    public LayoutData<INode, IEdge, ILabel, ILabel> CreateConfiguredLayoutDataPublic(
        GraphControl graphControl,
        ILayoutAlgorithm layout) =>
        CreateConfiguredLayoutData(graphControl, layout);
  }
}
