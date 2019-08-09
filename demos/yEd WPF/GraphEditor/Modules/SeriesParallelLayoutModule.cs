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
using yWorks.Layout;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.SeriesParallel;
using EdgeLayoutDescriptor = yWorks.Layout.SeriesParallel.EdgeLayoutDescriptor;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{

  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="SeriesParallelLayout"/>.
  /// </summary>
  public class SeriesParallelLayoutModule : LayoutModule
  {

    #region configuration constants

    //// Module 'Series-Parallel Layout'
    private const string SERIES_PARALLEL = "SERIES_PARALLEL";

    //// Section 'General'
    private const string GENERAL = "GENERAL";
    // Section 'General' items
    private const string ORIENTATION = "ORIENTATION";
    private const string RIGHT_TO_LEFT = "RIGHT_TO_LEFT";
    private const string BOTTOM_TO_TOP = "BOTTOM_TO_TOP";
    private const string LEFT_TO_RIGHT = "LEFT_TO_RIGHT";
    private const string TOP_TO_BOTTOM = "TOP_TO_BOTTOM";
    private const string VERTICAL_ALIGNMENT = "VERTICAL_ALIGNMENT";
    private const string ALIGNMENT_TOP = "ALIGNMENT_TOP";
    private const string ALIGNMENT_CENTER = "ALIGNMENT_CENTER";
    private const string ALIGNMENT_BOTTOM = "ALIGNMENT_BOTTOM";
    private const string FROM_SKETCH_MODE = "FROM_SKETCH_MODE";
    private const string ROUTING_STYLE_FOR_NON_SERIES_PARALLEL_EDGES = "ROUTING_STYLE_FOR_NON_SERIES_PARALLEL_EDGES";
    private const string ROUTE_ORGANIC = "ROUTE_ORGANIC";
    private const string ROUTE_ORTHOGONAL = "ROUTE_ORTHOGONAL";
    private const string ROUTE_STRAIGHTLINE = "ROUTE_STRAIGHTLINE";
    private const string TITLE_MINIMUM_DISTANCES = "MINIMUM_DISTANCES";
    private const string NODE_TO_NODE_DISTANCE = "NODE_TO_NODE_DISTANCE";
    private const string NODE_TO_EDGE_DISTANCE = "NODE_TO_EDGE_DISTANCE";
    private const string EDGE_TO_EDGE_DISTANCE = "EDGE_TO_EDGE_DISTANCE";
    private const string TITLE_LABELING = "LABELING";
    private const string CONSIDER_NODE_LABELS = "CONSIDER_NODE_LABELS";
    private const string INTEGRATED_EDGE_LABELING = "INTEGRATED_EDGE_LABELING";


    //// Section 'Edge Settings'
    private const string EDGE_SETTINGS = "EDGE_SETTINGS";
    //// Section 'Edge Settings' items
    private const string PORT_STYLE = "PORT_STYLE";
    private const string CENTER_PORTS = "CENTER_PORTS";
    private const string DISTRIBUTED_PORTS = "DISTRIBUTED_PORTS";
    private const string MINIMUM_FIRST_SEGMENT_LENGTH = "MINIMUM_FIRST_SEGMENT_LENGTH";
    private const string MINIMUM_LAST_SEGMENT_LENGTH = "MINIMUM_LAST_SEGMENT_LENGTH";
    private const string MINIMUM_EDGE_LENGTH = "MINIMUM_EDGE_LENGTH";
    private const string ROUTING_STYLE = "ROUTING_STYLE";
    private const string ROUTING_STYLE_ORTHOGONAL = "ROUTING_STYLE_ORTHOGONAL";
    private const string ROUTING_STYLE_OCTILINEAR = "ROUTING_STYLE_OCTILINEAR";
    private const string ROUTING_STYLE_POLYLINE = "ROUTING_STYLE_POLYLINE";
    private const string PREFERRED_OCTILINEAR_SEGMENT_LENGTH = "PREFERRED_OCTILINEAR_SEGMENT_LENGTH";
    private const string POLYLINE_DISTANCE = "POLYLINE_DISTANCE";
    private const string MINIMUM_SLOPE = "MINIMUM_SLOPE";
    private const string ROUTE_IN_FLOW = "ROUTE_IN_FLOW";

    private static readonly Dictionary<string, LayoutOrientation> orientations =
        new Dictionary<string, LayoutOrientation>(4);

    private static readonly Dictionary<string, double> verticalAlignments = new Dictionary<string, double>(3);
    private static readonly Dictionary<string, RoutingStyle> routingStyles = new Dictionary<string, RoutingStyle>(3);

    private SeriesParallelLayout layout;

    static SeriesParallelLayoutModule() {
      orientations.Add(TOP_TO_BOTTOM, LayoutOrientation.TopToBottom);
      orientations.Add(LEFT_TO_RIGHT, LayoutOrientation.LeftToRight);
      orientations.Add(BOTTOM_TO_TOP, LayoutOrientation.BottomToTop);
      orientations.Add(RIGHT_TO_LEFT, LayoutOrientation.RightToLeft);

      verticalAlignments.Add(ALIGNMENT_TOP, 0);
      verticalAlignments.Add(ALIGNMENT_CENTER, 0.5);
      verticalAlignments.Add(ALIGNMENT_BOTTOM, 1);

      routingStyles.Add(ROUTING_STYLE_ORTHOGONAL, RoutingStyle.Orthogonal);
      routingStyles.Add(ROUTING_STYLE_OCTILINEAR, RoutingStyle.Octilinear);
      routingStyles.Add(ROUTING_STYLE_POLYLINE, RoutingStyle.Polyline);
    }

    #endregion

    public SeriesParallelLayoutModule() : base(SERIES_PARALLEL) {}

    protected override void SetupHandler() {
      CreateLayout();

      OptionGroup generalGroup = Handler.AddGroup(GENERAL);
      generalGroup.AddList(ORIENTATION, orientations.Keys, TOP_TO_BOTTOM);
      generalGroup.AddList(VERTICAL_ALIGNMENT, verticalAlignments.Keys, ALIGNMENT_TOP);
      generalGroup.AddBool(FROM_SKETCH_MODE, layout.FromSketchMode);
      generalGroup.AddList(ROUTING_STYLE_FOR_NON_SERIES_PARALLEL_EDGES,
          new[] {ROUTE_ORGANIC, ROUTE_ORTHOGONAL, ROUTE_STRAIGHTLINE}, ROUTE_ORGANIC);

      generalGroup.AddDouble(NODE_TO_NODE_DISTANCE, 30.0d);
      generalGroup.AddDouble(NODE_TO_EDGE_DISTANCE, 15.0d);
      generalGroup.AddDouble(EDGE_TO_EDGE_DISTANCE, 15.0d);


      generalGroup.AddBool(CONSIDER_NODE_LABELS, true);
      generalGroup.AddBool(INTEGRATED_EDGE_LABELING, true);

      OptionGroup edgesGroup = Handler.AddGroup(EDGE_SETTINGS);
      edgesGroup.AddList(PORT_STYLE, new[] {CENTER_PORTS, DISTRIBUTED_PORTS}, CENTER_PORTS);
      edgesGroup.AddDouble(MINIMUM_FIRST_SEGMENT_LENGTH, layout.DefaultEdgeLayoutDescriptor.MinimumFirstSegmentLength);
      edgesGroup.AddDouble(MINIMUM_LAST_SEGMENT_LENGTH, layout.DefaultEdgeLayoutDescriptor.MinimumLastSegmentLength);
      edgesGroup.AddDouble(MINIMUM_EDGE_LENGTH, 20);
      var routingStyle = edgesGroup.AddList(ROUTING_STYLE, routingStyles.Keys, ROUTING_STYLE_ORTHOGONAL);
      var segmentLength = edgesGroup.AddDouble(PREFERRED_OCTILINEAR_SEGMENT_LENGTH,
          layout.PreferredOctilinearSegmentLength);
      var distance = edgesGroup.AddDouble(POLYLINE_DISTANCE, layout.MinimumPolylineSegmentLength);
      var minimumSlope = edgesGroup.AddDouble(MINIMUM_SLOPE, layout.MinimumSlope, 0, 5);
      edgesGroup.AddBool(ROUTE_IN_FLOW, true);

      var cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(routingStyle, ROUTING_STYLE_OCTILINEAR, segmentLength);
      cm.SetEnabledOnValueEquals(routingStyle, ROUTING_STYLE_POLYLINE, distance);
      cm.SetEnabledOnValueEquals(routingStyle, ROUTING_STYLE_POLYLINE, minimumSlope);
    }

    private void CreateLayout() {
      if (layout == null) {
        layout = new SeriesParallelLayout();
      }
    }

    protected override void ConfigureLayout() {
      OptionGroup generalGroup = Handler.GetGroupByName(GENERAL);

      SeriesParallelLayout series = new SeriesParallelLayout {GeneralGraphHandling = true};

      series.LayoutOrientation = orientations[(string) Handler.GetValue(GENERAL, ORIENTATION)];

      series.VerticalAlignment = verticalAlignments[(string) Handler.GetValue(GENERAL, VERTICAL_ALIGNMENT)];

      RoutingStyle routingStyle = routingStyles[(string) Handler.GetValue(EDGE_SETTINGS, ROUTING_STYLE)];
      series.RoutingStyle = routingStyle;
      switch (routingStyle) {
        case RoutingStyle.Octilinear:
          series.PreferredOctilinearSegmentLength =
              (double) Handler.GetValue(EDGE_SETTINGS, PREFERRED_OCTILINEAR_SEGMENT_LENGTH);
          break;
        case RoutingStyle.Polyline:
          series.MinimumPolylineSegmentLength = (double) Handler.GetValue(EDGE_SETTINGS, POLYLINE_DISTANCE);
          series.MinimumSlope = (double) Handler.GetValue(EDGE_SETTINGS, MINIMUM_SLOPE);
          break;
      }


      ((DefaultPortAssignment) series.DefaultPortAssignment).ForkStyle =
          (bool) Handler.GetValue(EDGE_SETTINGS, ROUTE_IN_FLOW)
              ? ForkStyle.OutsideNode
              : ForkStyle.AtNode;
      series.FromSketchMode = (bool) Handler.GetValue(GENERAL, FROM_SKETCH_MODE);


      string nonSeriesParallelRoutingStyle =
          (string) Handler.GetValue(GENERAL, ROUTING_STYLE_FOR_NON_SERIES_PARALLEL_EDGES);
      switch (nonSeriesParallelRoutingStyle) {
        case ROUTE_ORGANIC:
          series.NonSeriesParallelEdgeRouter = new OrganicEdgeRouter();
          series.NonSeriesParallelEdgesDpKey = OrganicEdgeRouter.AffectedEdgesDpKey;
          break;
        case ROUTE_ORTHOGONAL:
          var orthogonal = new EdgeRouter();
          orthogonal.Rerouting = true;
          orthogonal.Scope = Scope.RouteAffectedEdges;
          series.NonSeriesParallelEdgeRouter = orthogonal;
          series.NonSeriesParallelEdgesDpKey = orthogonal.AffectedEdgesDpKey;
          break;
        case ROUTE_STRAIGHTLINE:
          var straightLine = new StraightLineEdgeRouter();
          straightLine.Scope = Scope.RouteAffectedEdges;
          series.NonSeriesParallelEdgeRouter = straightLine;
          series.NonSeriesParallelEdgesDpKey = straightLine.AffectedEdgesDpKey;
          break;
      }

      series.MinimumNodeToNodeDistance = (double) Handler.GetValue(GENERAL, NODE_TO_NODE_DISTANCE);
      series.MinimumNodeToEdgeDistance = (double) Handler.GetValue(GENERAL, NODE_TO_EDGE_DISTANCE);
      series.MinimumEdgeToEdgeDistance = (double) Handler.GetValue(GENERAL, EDGE_TO_EDGE_DISTANCE);

      Object portStyle = Handler.GetValue(EDGE_SETTINGS, PORT_STYLE);
      if (CENTER_PORTS == portStyle) {
        ((DefaultPortAssignment) series.DefaultPortAssignment).Mode = PortAssignmentMode.Center;
      } else {
        ((DefaultPortAssignment) series.DefaultPortAssignment).Mode = PortAssignmentMode.Distributed;
      }
      EdgeLayoutDescriptor eld = series.DefaultEdgeLayoutDescriptor;
      eld.MinimumFirstSegmentLength = (double) Handler.GetValue(EDGE_SETTINGS, MINIMUM_FIRST_SEGMENT_LENGTH);
      eld.MinimumLastSegmentLength = (double) Handler.GetValue(EDGE_SETTINGS, MINIMUM_LAST_SEGMENT_LENGTH);
      eld.MinimumLength = (double) Handler.GetValue(EDGE_SETTINGS, MINIMUM_EDGE_LENGTH);

      series.ConsiderNodeLabels = (bool) Handler.GetValue(GENERAL, CONSIDER_NODE_LABELS);
      series.IntegratedEdgeLabeling = (bool) Handler.GetValue(GENERAL, INTEGRATED_EDGE_LABELING);
      LayoutAlgorithm = series;
    }
  }
}
