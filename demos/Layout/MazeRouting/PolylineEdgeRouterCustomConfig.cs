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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;

namespace Demo.yFiles.Layout.MazeRouting
{
  /// <summary>
  /// Configuration options for the <see cref="EdgeRouter"/> algorithm.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("PolylineEdgeRouter")]
  public class PolylineEdgeRouterCustomConfig
  {
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public PolylineEdgeRouterCustomConfig() {
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

      EnablePolylineRoutingItem = true;
      PreferredPolylineSegmentLengthItem = router.PreferredPolylineSegmentLength;
    }

    public EdgeRouter CreateConfiguredLayout(GraphControl graphControl) {
      var router = new EdgeRouter(); 
      var descriptor = router.DefaultEdgeLayoutDescriptor;

      router.Scope = ScopeItem;

      if (OptimizationStrategyItem == EnumStrategies.Balanced) {
        descriptor.PenaltySettings = PenaltySettings.OptimizationBalanced;
      }
      else if (OptimizationStrategyItem == EnumStrategies.MinimizeBends) {
        descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeBends;
      }
      else if (OptimizationStrategyItem == EnumStrategies.MinimizeEdgeLength) {
        descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeLengths;
      }
      else {
        descriptor.PenaltySettings = PenaltySettings.OptimizationEdgeCrossings;
      }

      if (MonotonicRestrictionItem == EnumMonotonyFlags.Horizontal) {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Horizontal;
      }
      else if (MonotonicRestrictionItem == EnumMonotonyFlags.Vertical) {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Vertical;
      }
      else if (MonotonicRestrictionItem == EnumMonotonyFlags.Both) {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.Both;
      }
      else {
        descriptor.MonotonicPathRestriction = MonotonicPathRestriction.None;
      }

      descriptor.MinimumEdgeToEdgeDistance = MinimumEdgeToEdgeDistanceItem;
      router.MinimumNodeToEdgeDistance = MinimumNodeToEdgeDistanceItem;
      descriptor.MinimumNodeCornerDistance = MinimumNodeCornerDistanceItem;
      descriptor.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
      descriptor.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;

      if (GridEnabledItem) {
        router.Grid = new Grid(0, 0, GridSpacingItem);
      }
      else {
        router.Grid = null;
      }

      router.Rerouting = EnableReroutingItem;

      router.PolylineRouting = EnablePolylineRoutingItem;
      router.PreferredPolylineSegmentLength = PreferredPolylineSegmentLengthItem;
      router.MaximumDuration = MaximumDurationItem * 1000;

      return router;
    }

   
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

    [Label("Minimum Distances")]
    [OptionGroup("RootGroup", 20)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DistancesGroup;

    [Label("Grid")]
    [OptionGroup("RootGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GridGroup;

    [Label("Octilinear Routing")]
    [OptionGroup("RootGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object PolylineGroup;

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
    [EnumValue("Less Bends",EnumStrategies.MinimizeBends)]
    [EnumValue("Less Crossings",EnumStrategies.MinimizeCrossings)]
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

    [Label("Last Segment Length")]
    [OptionGroup("DistancesGroup", 50)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLastSegmentLengthItem { get; set; }

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

    [Label("Octilinear Routing")]
    [OptionGroup("PolylineGroup", 10)]
    [DefaultValue(true)]
    public bool EnablePolylineRoutingItem { get; set; }

    [Label("Preferred Polyline Segment Length")]
    [OptionGroup("PolylineGroup", 20)]
    [DefaultValue(30.0d)]
    [MinMax(Min = 5, Max = 500)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredPolylineSegmentLengthItem { get; set; }

    public bool ShouldDisablePreferredPolylineSegmentLengthItem {
      get { return EnablePolylineRoutingItem == false; }
    }
  }
}
