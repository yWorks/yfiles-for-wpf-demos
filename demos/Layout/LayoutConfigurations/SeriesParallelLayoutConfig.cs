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

using System.ComponentModel;
using System.Reflection;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Layout;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.SeriesParallel;
using RoutingStyle = yWorks.Layout.SeriesParallel.RoutingStyle;

namespace Demo.yFiles.Layout.Configurations
{
  /// <summary>
  /// Configuration options for the layout algorithm of the same name.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  [Label("SeriesParallelLayout")]
  public class SeriesParallelLayoutConfig : LayoutConfiguration
  {
    public SeriesParallelLayoutConfig() {
      var layout = new SeriesParallelLayout();
      var edgeLayoutDescriptor = layout.DefaultEdgeLayoutDescriptor;

      OrientationItem = LayoutOrientation.TopToBottom;
      VerticalAlignmentItem = 0.5;
      UseDrawingAsSketchItem = layout.FromSketchMode;
      MinimumNodeToNodeDistanceItem = 30;
      MinimumNodeToEdgeDistanceItem = 15;
      MinimumEdgeToEdgeDistanceItem = 15;
      ConsiderNodeLabelsItem = true;
      PlaceEdgeLabelsItem = true;

      PortStyleItem = PortAssignmentMode.Center;
      RoutingStyleItem = RoutingStyle.Orthogonal;
      PreferredOctilinearSegmentLengthItem = layout.PreferredOctilinearSegmentLength;
      MinimumPolylineSegmentLengthItem = layout.MinimumPolylineSegmentLength;
      MinimumSlopeItem = layout.MinimumSlope;
      RoutingStyleNonSeriesParallelItem = NonSeriesParallelRoutingStyle.Orthogonal;
      RouteEdgesInFlowDirectionItem = true;
      MinimumFirstSegmentLengthItem = edgeLayoutDescriptor.MinimumFirstSegmentLength;
      MinimumLastSegmentLengthItem = edgeLayoutDescriptor.MinimumLastSegmentLength;
      MinimumEdgeLengthItem = 20;
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      var layout = new SeriesParallelLayout();
      layout.GeneralGraphHandling = true;

      layout.LayoutOrientation = OrientationItem;

      layout.VerticalAlignment = VerticalAlignmentItem;
      layout.FromSketchMode = UseDrawingAsSketchItem;

      layout.MinimumNodeToNodeDistance = MinimumNodeToNodeDistanceItem;
      layout.MinimumNodeToEdgeDistance = MinimumNodeToEdgeDistanceItem;
      layout.MinimumEdgeToEdgeDistance = MinimumEdgeToEdgeDistanceItem;

      layout.ConsiderNodeLabels = ConsiderNodeLabelsItem;
      layout.IntegratedEdgeLabeling = PlaceEdgeLabelsItem;

      var portAssignment = (DefaultPortAssignment) layout.DefaultPortAssignment;
      portAssignment.Mode = PortStyleItem;
      portAssignment.ForkStyle = RouteEdgesInFlowDirectionItem ? ForkStyle.OutsideNode : ForkStyle.AtNode;

      layout.RoutingStyle = RoutingStyleItem;
      if (RoutingStyleItem == RoutingStyle.Octilinear) {
        layout.PreferredOctilinearSegmentLength = PreferredOctilinearSegmentLengthItem;
      } else if (RoutingStyleItem == RoutingStyle.Polyline) {
        layout.MinimumPolylineSegmentLength = MinimumPolylineSegmentLengthItem;
        layout.MinimumSlope = MinimumSlopeItem;
      }

      if (RoutingStyleNonSeriesParallelItem == NonSeriesParallelRoutingStyle.Orthogonal) {
        var edgeRouter = new EdgeRouter {
            Rerouting = true,
            Scope = Scope.RouteAffectedEdges
        };
        layout.NonSeriesParallelEdgeRouter = edgeRouter;
        layout.NonSeriesParallelEdgesDpKey = edgeRouter.AffectedEdgesDpKey;
      } else if (RoutingStyleNonSeriesParallelItem == NonSeriesParallelRoutingStyle.Organic) {
        var edgeRouter = new OrganicEdgeRouter();
        layout.NonSeriesParallelEdgeRouter = edgeRouter;
        layout.NonSeriesParallelEdgesDpKey = OrganicEdgeRouter.AffectedEdgesDpKey;
      } else if (RoutingStyleNonSeriesParallelItem == NonSeriesParallelRoutingStyle.Straight) {
        var edgeRouter = new StraightLineEdgeRouter { Scope = Scope.RouteAffectedEdges };
        layout.NonSeriesParallelEdgeRouter = edgeRouter;
        layout.NonSeriesParallelEdgesDpKey = edgeRouter.AffectedEdgesDpKey;
      }

      var edgeLayoutDescriptor = layout.DefaultEdgeLayoutDescriptor;
      edgeLayoutDescriptor.MinimumFirstSegmentLength = MinimumFirstSegmentLengthItem;
      edgeLayoutDescriptor.MinimumLastSegmentLength = MinimumLastSegmentLengthItem;
      edgeLayoutDescriptor.MinimumLength = MinimumEdgeLengthItem;

      return layout;
    }

    [Label("Description")]
    [OptionGroup("RootGroup", 5)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DescriptionGroup { get; set; }

    [Label("General")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object GeneralGroup { get; set; }

    [Label("Edges")]
    [OptionGroup("RootGroup", 10)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object EdgesGroup { get; set; }

    [OptionGroup("DescriptionGroup", 10)]
    [ComponentType(ComponentTypes.FormattedText)]
    public string DescriptionTextItem {
      get {
        return 
          "<Paragraph>The series-parallel layout algorithm highlights the main direction or flow of a graph, similar to the" +
          " hierarchic style. In comparison, this algorithm is usually faster but can be used only on special graphs," + 
          " namely series-parallel graphs.</Paragraph>";
      }
    }

    [Label("Orientation")]
    [DefaultValue(LayoutOrientation.TopToBottom)]
    [OptionGroup("GeneralGroup", 10)]
    [EnumValue("Top to Bottom", LayoutOrientation.TopToBottom )]
    [EnumValue("Left to Right", LayoutOrientation.LeftToRight )]
    [EnumValue("Bottom to Top", LayoutOrientation.BottomToTop )]
    [EnumValue("Right to Left", LayoutOrientation.RightToLeft )]
    public LayoutOrientation OrientationItem { get; set; }

    [Label("Vertical Alignment")]
    [DefaultValue(0.5d)]
    [OptionGroup("GeneralGroup", 20)]
    [EnumValue("Top", 0.0 )]
    [EnumValue("Center", 0.5 )]
    [EnumValue("Bottom", 1.0 )]
    public double VerticalAlignmentItem { get; set; }

    [Label("Use Drawing as Sketch")]
    [OptionGroup("GeneralGroup", 20)]
    [DefaultValue(false)]
    public bool UseDrawingAsSketchItem { get; set; }

    [Label("Minimum Distances")]
    [OptionGroup("GeneralGroup", 30)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object DistanceGroup { get; set; }

    [Label("Node to Node Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(30.0d)]
    [OptionGroup("DistanceGroup", 10)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeToNodeDistanceItem { get; set; }

    [Label("Node to Edge Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(15.0d)]
    [OptionGroup("DistanceGroup", 20)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumNodeToEdgeDistanceItem { get; set; }

    [Label("Edge to Edge Distance")]
    [MinMax(Min = 0, Max = 100)]
    [DefaultValue(15.0d)]
    [OptionGroup("DistanceGroup", 30)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeToEdgeDistanceItem { get; set; }

    [Label("Labeling")]
    [OptionGroup("GeneralGroup", 40)]
    [ComponentType(ComponentTypes.OptionGroup)]
    public object LabelingGroup { get; set; }

    [Label("Consider Node Labels")]
    [OptionGroup("LabelingGroup", 10)]
    [DefaultValue(true)]
    public bool ConsiderNodeLabelsItem { get; set; }

    [Label("Place Edge Labels")]
    [OptionGroup("LabelingGroup", 20)]
    [DefaultValue(true)]
    public bool PlaceEdgeLabelsItem { get; set; }

    [Label("Port Style")]
    [OptionGroup("EdgesGroup", 10)]
    [DefaultValue(PortAssignmentMode.Center)]
    [EnumValue("Centered", PortAssignmentMode.Center)]
    [EnumValue("Distributed", PortAssignmentMode.Distributed )]
    public PortAssignmentMode PortStyleItem { get; set; }

    [Label("Routing Style")]
    [OptionGroup("EdgesGroup", 20)]
    [DefaultValue(RoutingStyle.Orthogonal)]
    [EnumValue("Orthogonal", RoutingStyle.Orthogonal)]
    [EnumValue("Octilinear", RoutingStyle.Octilinear )]
    [EnumValue("Polyline", RoutingStyle.Polyline )]
    public RoutingStyle RoutingStyleItem { get; set; }

    [Label("Preferred Octilinear Segment Length")]
    [OptionGroup("EdgesGroup", 30)]
    [DefaultValue(10.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double PreferredOctilinearSegmentLengthItem { get; set; }

    public bool ShouldDisablePreferredOctilinearSegmentLengthItem {
      get { return RoutingStyleItem != RoutingStyle.Octilinear; }
    }

    [Label("Minimum Polyline Segment Length")]
    [OptionGroup("EdgesGroup", 40)]
    [DefaultValue(30.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumPolylineSegmentLengthItem { get; set; }

    public bool ShouldDisableMinimumPolylineSegmentLengthItem {
      get { return RoutingStyleItem != RoutingStyle.Polyline; }
    }

    [MinMax(Min = 0.0d, Max = 5.0d, Step = 0.01d)]
    [Label("Minimum Slope")]
    [DefaultValue(0.25d)]
    [OptionGroup("EdgesGroup", 50)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumSlopeItem { get; set; }

    public bool ShouldDisableMinimumSlopeItem {
      get { return RoutingStyleItem != RoutingStyle.Polyline; }
    }

    [Label("Routing Style (Non-Series-Parallel Edges)")]
    [OptionGroup("EdgesGroup", 60)]
    [DefaultValue(NonSeriesParallelRoutingStyle.Orthogonal)]
    [EnumValue("Orthogonal", NonSeriesParallelRoutingStyle.Orthogonal)]
    [EnumValue("Organic", NonSeriesParallelRoutingStyle.Organic)]
    [EnumValue("Straight-Line", NonSeriesParallelRoutingStyle.Straight)]
    public NonSeriesParallelRoutingStyle RoutingStyleNonSeriesParallelItem { get; set; }

    [Label("Route Edges in Flow Direction")]
    [OptionGroup("EdgesGroup", 70)]
    [DefaultValue(true)]
    public bool RouteEdgesInFlowDirectionItem { get; set; }

    [Label("Minimum First Segment Length")]
    [OptionGroup("EdgesGroup", 80)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumFirstSegmentLengthItem { get; set; }

    [Label("Minimum Last Segment Length")]
    [OptionGroup("EdgesGroup", 90)]
    [DefaultValue(15.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumLastSegmentLengthItem { get; set; }

    [Label("Minimum Edge Length")]
    [OptionGroup("EdgesGroup", 100)]
    [DefaultValue(20.0d)]
    [MinMax(Min = 0, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public double MinimumEdgeLengthItem { get; set; }

    public enum NonSeriesParallelRoutingStyle {
      Orthogonal, Organic, Straight
    }
  }
}