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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Algorithms.Geometry;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using BusDescriptor = yWorks.Layout.Router.Polyline.BusDescriptor;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the <see cref="EdgeRouter"/> algorithm.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("PolylineEdgeRouter")]
  public class PolylineEdgeRouterConfig : LayoutConfiguration
  {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public PolylineEdgeRouterConfig() {
      var router = new EdgeRouter();

      ScopeItem = router.Scope;
      OptimizationStrategyItem = EnumStrategies.Balanced;
      MonotonicRestrictionItem = EnumMonotonyFlags.None;
      EnableReroutingItem = router.Rerouting;
      MaximumDurationItem = 30;

      var descriptor = router.DefaultEdgeLayoutDescriptor;
      MinimumEdgeToEdgeDistanceItem = descriptor.MinimumEdgeToEdgeDistance;
      MinimumNodeToEdgeDistanceItem = router.MinimumNodeToEdgeDistance;
      MinimumNodeCornerDistanceItem = descriptor.MinimumNodeCornerDistance;
      MinimumFirstSegmentLengthItem = descriptor.MinimumFirstSegmentLength;
      MinimumLastSegmentLengthItem = descriptor.MinimumLastSegmentLength;

      var grid = router.Grid;
      GridEnabledItem = grid != null;
      GridSpacingItem = grid != null ? grid.Spacing : 10;

      EdgeRoutingStyleItem = EdgeRoutingStyle.Orthogonal;
      PreferredOctilinearSegmentLengthItem = router.DefaultEdgeLayoutDescriptor.PreferredOctilinearSegmentLength;
      MaximumOctilinearSegmentRatioItem = router.DefaultEdgeLayoutDescriptor.MaximumOctilinearSegmentRatio;

      ConsiderNodeLabelsItem = router.ConsiderNodeLabels;
      ConsiderEdgeLabelsItem = router.ConsiderEdgeLabels; 
      EdgeLabelingItem = EnumEdgeLabeling.None;
      LabelPlacementAlongEdgeItem = EnumLabelPlacementAlongEdge.Centered;
      LabelPlacementSideOfEdgeItem = EnumLabelPlacementSideOfEdge.OnEdge;
      LabelPlacementOrientationItem = EnumLabelPlacementOrientation.Horizontal;
      LabelPlacementDistanceItem = 10;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var router = new EdgeRouter(); 

      router.Scope = ScopeItem;
      router.MinimumNodeToEdgeDistance = MinimumNodeToEdgeDistanceItem;

      if (GridEnabledItem) {
        router.Grid = new Grid(0, 0, GridSpacingItem);
      } else {
        router.Grid = null;
      }

      router.ConsiderNodeLabels = ConsiderNodeLabelsItem;
      router.ConsiderEdgeLabels = ConsiderEdgeLabelsItem;
      router.Rerouting = EnableReroutingItem;

      // Note that CreateConfiguredLayoutData replaces the settings on the DefaultEdgeLayoutDescriptor
      // by providing a custom one for each edge.
      router.DefaultEdgeLayoutDescriptor.RoutingStyle = EdgeRoutingStyleItem;
      router.DefaultEdgeLayoutDescriptor.PreferredOctilinearSegmentLength = PreferredOctilinearSegmentLengthItem;
      router.DefaultEdgeLayoutDescriptor.MaximumOctilinearSegmentRatio = MaximumOctilinearSegmentRatioItem;
      router.DefaultEdgeLayoutDescriptor.SourceCurveConnectionStyle = SourceConnectionStyleItem;
      router.DefaultEdgeLayoutDescriptor.TargetCurveConnectionStyle = TargetConnectionStyleItem;

      router.MaximumDuration = MaximumDurationItem * 1000;

      var layout = new SequentialLayout();
      layout.AppendLayout(router);

      if (EdgeLabelingItem == EnumEdgeLabeling.None) {
        router.IntegratedEdgeLabeling = false;
      } else if (EdgeLabelingItem == EnumEdgeLabeling.Integrated) {
        router.IntegratedEdgeLabeling = true;
      } else if (EdgeLabelingItem == EnumEdgeLabeling.Generic) {
        var genericLabeling = new GenericLabeling();
        genericLabeling.PlaceEdgeLabels = true;
        genericLabeling.PlaceNodeLabels = false;
        genericLabeling.ReduceAmbiguity = ReduceAmbiguityItem;
        layout.AppendLayout(genericLabeling);
      }

      AddPreferredPlacementDescriptor(graphControl.Graph, LabelPlacementAlongEdgeItem, LabelPlacementSideOfEdgeItem, LabelPlacementOrientationItem, LabelPlacementDistanceItem);

      return layout;
    }

    protected override LayoutData CreateConfiguredLayoutData(GraphControl graphControl, ILayoutAlgorithm layout) {
      var layoutData = new PolylineEdgeRouterData();

      layoutData.EdgeLayoutDescriptors.Delegate = edge => {
        var descriptor = new EdgeLayoutDescriptor {
            RoutingStyle = EdgeRoutingStyleItem,
            PreferredOctilinearSegmentLength = PreferredOctilinearSegmentLengthItem, 
            MaximumOctilinearSegmentRatio = MaximumOctilinearSegmentRatioItem,
            SourceCurveConnectionStyle = SourceConnectionStyleItem,
            TargetCurveConnectionStyle = TargetConnectionStyleItem
        };
        if (OptimizationStrategyItem == EnumStrategies.Balanced) {
          descriptor.PenaltySettings = PenaltySettings.OptimizationBalanced;
        } else if (OptimizationStrategyItem == EnumStrategies.MinimizeBends) {
          descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeBends;
        } else if (OptimizationStrategyItem == EnumStrategies.MinimizeEdgeLength) {
          descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeLengths;
        } else {
          descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeCrossings;
        }

        if (MonotonicRestrictionItem == EnumMonotonyFlags.Horizontal) {
          descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Horizontal;
        } else if (MonotonicRestrictionItem == EnumMonotonyFlags.Vertical) {
          descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Vertical;
        } else if (MonotonicRestrictionItem == EnumMonotonyFlags.Both) {
          descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Both;
        } else {
          descriptor.MonotonicPathRestriction = MonotonicPathRestriction.None;
        }

        descriptor.MinimumEdgeToEdgeDistance = MinimumEdgeToEdgeDistanceItem;
        descriptor.MinimumNodeCornerDistance = MinimumNodeCornerDistanceItem;
        descriptor.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
        descriptor.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;

        if (UseIntermediatePointsItem) {
          descriptor.IntermediateRoutingPoints =
              edge.Bends
                  .Select(b => new YPoint(b.Location.X, b.Location.Y))
                  .Cast<object>()
                  .ToList();
        }

        return descriptor;
      };

      var selection = graphControl.Selection;
      if (ScopeItem == Scope.RouteEdgesAtAffectedNodes) {
        layoutData.AffectedNodes.Source = selection.SelectedNodes;
      } else if (ScopeItem == Scope.RouteAffectedEdges) {
        layoutData.AffectedEdges.Source = selection.SelectedEdges;
      } else {
        layoutData.AffectedEdges.Delegate = edge => true;
        layoutData.AffectedNodes.Delegate = node => true;
      }

      switch (BusRoutingItem) {
        case EnumBusRouting.SingleBus:
          // All edges in a single bus
          layoutData.Buses.Add(new BusDescriptor()).Delegate = edge => true;
          break;
        case EnumBusRouting.ByLabel:
          var byLabel = new Dictionary<string, List<IEdge>>();
          foreach (var edge in graphControl.Graph.Edges) {
            if (edge.Labels.Count > 0) {
              var label = edge.Labels[0].Text;
              List<IEdge> list;
              if (!byLabel.TryGetValue(label, out list)) {
                list = new List<IEdge>();
                byLabel[label] = list;
              }
              list.Add(edge);
            }
          }
          foreach (var edges in byLabel.Values) {
            // Add a bus per label. Unlabeled edges don't get grouped into a bus
            layoutData.Buses.Add(new BusDescriptor()).Source = edges;
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
                list = new List<IEdge>();
                byColor[brush] = list;
              }
              list.Add(edge);
            }
          }
          foreach (var edges in byColor.Values) {
            // Add a bus per color. Black edges don't get grouped into a bus
            layoutData.Buses.Add(new BusDescriptor()).Source = edges;
          }
          break;
      }

      return layoutData;
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
    public object LayoutGroup;

    [Label("Minimum Distances")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DistancesGroup;

    [Label("Grid")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GridGroup;

    [Label("Routing Style")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object RoutingStyleGroup;

    [Label("Labeling")]
    [OptionGroup("RootGroup", 50)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup;

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
    public string DescriptionText {
      get {
        return "<Paragraph>Polyline edge routing calculates polyline edge paths for a diagram's edges. "
               + "The positions of the nodes are not changed by this algorithm.</Paragraph>"
               + "<Paragraph>Edges will be routed orthogonally, that is each edge path consists of horizontal and vertical segments, or octilinear."
               + " Octilinear means that the slope of each segment of an edge path is a multiple of 45 degrees.</Paragraph>"
               + "<Paragraph>This type of edge routing is especially well suited for technical diagrams.</Paragraph>";
      }
    } 

    public enum EnumStrategies
    {
      Balanced, MinimizeBends, MinimizeCrossings, MinimizeEdgeLength
    }

    public enum EnumMonotonyFlags
    {
      None,Horizontal,Vertical,Both
    }

    public enum EnumBusRouting
    {
      None, SingleBus, ByLabel, ByColor
    }

    [Label("Scope")]
    [OptionGroup("LayoutGroup", 10)]
    [DefaultValue(Scope.RouteAllEdges)]
    [EnumValue("All Edges", Scope.RouteAllEdges)]
    [EnumValue("Selected Edges",Scope.RouteAffectedEdges)]
    [EnumValue("Edges at Selected Nodes",Scope.RouteEdgesAtAffectedNodes)]
    public Scope ScopeItem { get; set; }

    [Label("Optimization Strategy")]
    [OptionGroup("LayoutGroup", 20)]
    [DefaultValue(EnumStrategies.Balanced)]
    [EnumValue("Balanced", EnumStrategies.Balanced)]
    [EnumValue("Fewer Bends",EnumStrategies.MinimizeBends)]
    [EnumValue("Fewer Crossings",EnumStrategies.MinimizeCrossings)]
    [EnumValue("Shorter Edges",EnumStrategies.MinimizeEdgeLength)]
    public EnumStrategies OptimizationStrategyItem { get; set; }

    [Label("Monotonic Restriction")]
    [OptionGroup("LayoutGroup", 30)]
    [DefaultValue(EnumMonotonyFlags.None)]
    [EnumValue("None", EnumMonotonyFlags.None)]
    [EnumValue("Horizontal",EnumMonotonyFlags.Horizontal )]
    [EnumValue("Vertical",EnumMonotonyFlags.Vertical )]
    [EnumValue("Both",EnumMonotonyFlags.Both )]
    public EnumMonotonyFlags MonotonicRestrictionItem { get; set; }

    [Label("Reroute Crossing Edges")]
    [OptionGroup("LayoutGroup", 60)]
    [DefaultValue(false)]
    public bool EnableReroutingItem { get; set; }

    [Label("Keep Bends as Intermediate Points")]
    [OptionGroup("LayoutGroup", 65)]
    public bool UseIntermediatePointsItem { get; set; }

    [Label("Maximum Duration")]
    [OptionGroup("LayoutGroup", 70)]
    [DefaultValue(30)]
    [MinMax(Min = 0, Max = 150)]
    [ComponentType(ComponentTypes.Slider)]
    public int MaximumDurationItem { get; set; }

    [Label("Edge to Edge")]
    [OptionGroup("DistancesGroup", 10)]
    [DefaultValue(3.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeToEdgeDistanceItem { get; set; }

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

    public bool ShouldDisableMinimumFirstSegmentLengthItem {
      get { return EdgeRoutingStyleItem == EdgeRoutingStyle.Curved && SourceConnectionStyleItem == CurveConnectionStyle.Organic; }
    }

    [Label("Last Segment Length")]
    [OptionGroup("DistancesGroup", 50)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLastSegmentLengthItem { get; set; }

    public bool ShouldDisableMinimumLastSegmentLengthItem {
      get { return EdgeRoutingStyleItem == EdgeRoutingStyle.Curved && TargetConnectionStyleItem == CurveConnectionStyle.Organic; }
    }

    [Label("Route on Grid")]
    [OptionGroup("GridGroup", 10)]
    [DefaultValue(false)]
    public bool GridEnabledItem { get; set; }

    [Label("Grid Spacing")]
    [OptionGroup("GridGroup", 20)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 2, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double GridSpacingItem { get; set; }

    public bool ShouldDisableGridSpacingItem {
      get { return GridEnabledItem == false; }
    }

    [Label("Routing Style")]
    [OptionGroup("RoutingStyleGroup", 10)]
    [DefaultValue(EdgeRoutingStyle.Orthogonal)]
    [EnumValue("Orthogonal", EdgeRoutingStyle.Orthogonal)]
    [EnumValue("Octilinear", EdgeRoutingStyle.Octilinear)]
    [EnumValue("Curved", EdgeRoutingStyle.Curved)]
    public EdgeRoutingStyle EdgeRoutingStyleItem { get; set; }

    [Label("Preferred Octilinear Corner Length")]
    [OptionGroup("RoutingStyleGroup", 20)]
    [DefaultValue(30.0d)]
    [MinMax(Min = 5, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredOctilinearSegmentLengthItem { get; set; }

    public bool ShouldDisablePreferredOctilinearSegmentLengthItem {
      get { return EdgeRoutingStyleItem != EdgeRoutingStyle.Octilinear; }
    }

    [Label("Maximum Octilinear Segment Length Ratio")]
    [OptionGroup("RoutingStyleGroup", 25)]
    [DefaultValue(0.3d)]
    [MinMax(Min = 0, Max = 0.5, Step = 0.05)]
    [ComponentType(ComponentTypes.Slider)]
    public double MaximumOctilinearSegmentRatioItem { get; set; }

    public bool ShouldDisableMaximumOctilinearSegmentRatioItem {
      get { return EdgeRoutingStyleItem != EdgeRoutingStyle.Octilinear; }
    }

    [Label("Curved connection at source")]
    [OptionGroup("RoutingStyleGroup", 30)]
    [DefaultValue(CurveConnectionStyle.KeepPort)]
    [EnumValue("Straight", CurveConnectionStyle.KeepPort)]
    [EnumValue("Organic", CurveConnectionStyle.Organic)]
    public CurveConnectionStyle SourceConnectionStyleItem { get; set; }

    public bool ShouldDisableSourceConnectionStyleItem {
      get { return EdgeRoutingStyleItem != EdgeRoutingStyle.Curved; }
    }

    [Label("Curved connection at target")]
    [OptionGroup("RoutingStyleGroup", 40)]
    [DefaultValue(CurveConnectionStyle.KeepPort)]
    [EnumValue("Straight", CurveConnectionStyle.KeepPort)]
    [EnumValue("Organic", CurveConnectionStyle.Organic)]
    public CurveConnectionStyle TargetConnectionStyleItem { get; set; }

    public bool ShouldDisableTargetConnectionStyleItem {
      get { return EdgeRoutingStyleItem != EdgeRoutingStyle.Curved; }
    }

    [Label("Bus routing")]
    [OptionGroup("RoutingStyleGroup", 50)]
    [EnumValue("No Buses", EnumBusRouting.None)]
    [EnumValue("Single Bus", EnumBusRouting.SingleBus)]
    [EnumValue("By Edge Color", EnumBusRouting.ByColor)]
    [EnumValue("By Edge Label", EnumBusRouting.ByLabel)]
    public EnumBusRouting BusRoutingItem { get; set; }

    [Label("Consider Node Labels")]
    [OptionGroup("NodePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool ConsiderNodeLabelsItem { get; set; }

    [Label("Consider Fixed Edges' Labels")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(false)]
    public bool ConsiderEdgeLabelsItem { get; set; }

    public enum EnumEdgeLabeling {
      None, Integrated, Generic
    }

    [Label("Edge Labeling")]
    [OptionGroup("EdgePropertiesGroup", 10)]
    [DefaultValue(EnumEdgeLabeling.None)]
    [EnumValue("None", EnumEdgeLabeling.None)] 
    [EnumValue("Integrated", EnumEdgeLabeling.Integrated)]
    [EnumValue("Generic", EnumEdgeLabeling.Generic)]
    public EnumEdgeLabeling EdgeLabelingItem { get; set; }    

    [Label("Orientation")]
    [OptionGroup("PreferredPlacementGroup", 10)]
    [DefaultValue(EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Parallel", EnumLabelPlacementOrientation.Parallel)]
    [EnumValue("Orthogonal",EnumLabelPlacementOrientation.Orthogonal)]
    [EnumValue("Horizontal",EnumLabelPlacementOrientation.Horizontal)]
    [EnumValue("Vertical",EnumLabelPlacementOrientation.Vertical)]
    public EnumLabelPlacementOrientation LabelPlacementOrientationItem { get; set; }

    public bool ShouldDisableLabelPlacementOrientationItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }

    [Label("Reduce Ambiguity")]
    [OptionGroup("EdgePropertiesGroup", 30)]
    public bool ReduceAmbiguityItem { get; set; }

    public bool ShouldDisableReduceAmbiguityItem {
      get { return EdgeLabelingItem != EnumEdgeLabeling.Generic; }
    }

    [Label("Along Edge")]
    [OptionGroup("PreferredPlacementGroup", 20)]
    [DefaultValue(EnumLabelPlacementAlongEdge.Centered)]
    [EnumValue("Anywhere", EnumLabelPlacementAlongEdge.Anywhere)]
    [EnumValue("At Source",EnumLabelPlacementAlongEdge.AtSource)]
    [EnumValue("At Target",EnumLabelPlacementAlongEdge.AtTarget)]
    [EnumValue("Centered",EnumLabelPlacementAlongEdge.Centered)]
    public EnumLabelPlacementAlongEdge LabelPlacementAlongEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementAlongEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }

    [Label("Side of Edge")]
    [OptionGroup("PreferredPlacementGroup", 30)]
    [DefaultValue(EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Anywhere", EnumLabelPlacementSideOfEdge.Anywhere)]
    [EnumValue("On Edge",EnumLabelPlacementSideOfEdge.OnEdge)]
    [EnumValue("Left",EnumLabelPlacementSideOfEdge.Left)]
    [EnumValue("Right",EnumLabelPlacementSideOfEdge.Right)]
    [EnumValue("Left or Right",EnumLabelPlacementSideOfEdge.LeftOrRight)]
    public EnumLabelPlacementSideOfEdge LabelPlacementSideOfEdgeItem { get; set; }

    public bool ShouldDisableLabelPlacementSideOfEdgeItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None; }
    }

    [Label("Distance")]
    [OptionGroup("PreferredPlacementGroup", 40)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0.0d, Max = 40.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double LabelPlacementDistanceItem { get; set; }

    public bool ShouldDisableLabelPlacementDistanceItem {
      get { return EdgeLabelingItem == EnumEdgeLabeling.None || LabelPlacementSideOfEdgeItem == EnumLabelPlacementSideOfEdge.OnEdge; }
    }

    private class ColorComparer : IEqualityComparer<Brush>
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
  }
}
