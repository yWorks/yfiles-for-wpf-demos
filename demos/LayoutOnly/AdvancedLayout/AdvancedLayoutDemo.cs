/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using Demo.Layout.LayoutGraphViewer;
using yWorks.Geometry;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Labeling;
using yWorks.Utils;

namespace Demo.Layout.AdvancedLayout
{
  /// <summary>
  ///   This class shows how to use layout and labeling algorithms. This demo
  ///   displays the calculated coordinates in a simple graph viewer.
  ///   Additionally, it outputs the calculated coordinates of the graph layout to
  ///   the console.
  ///   In this demo, HierarchicalLayout is used to layout a small graph.
  ///   First, the edge labels of the graph will be laid out using a general labeling
  ///   approach that effectively positions the labels after the node and edge positions
  ///   have already been fixed.
  ///   Second, a special edge labeling mechanism will be used that is currently only
  ///   available in conjunction with HierarchicalLayout. While laying out the graph, the
  ///   edge labels will be considered as well. Therefore, the node and edge positions
  ///   can be chosen in such a way that the labeling does not
  ///   introduce overlaps between labels and other entities in the graph.
  /// </summary>
  public class AdvancedLayoutDemo
  {
    [STAThread]
    public static void Main() {
      var graph = new LayoutGraph();

      // construct graph. assign sizes to nodes
      var v1 = graph.CreateNode();
      v1.Layout.Size = new SizeD(30, 30);
      var v2 = graph.CreateNode();
      v2.Layout.Size = new SizeD(30, 30);
      var v3 = graph.CreateNode();
      v3.Layout.Size = new SizeD(30, 30);

      var e1 = graph.CreateEdge(v1, v2);
      var e2 = graph.CreateEdge(v2, v3);
      var e3 = graph.CreateEdge(v1, v3);

      var layout = new HierarchicalLayout { EdgeLabelPlacement = EdgeLabelPlacement.Generic };
      var hierarchicalLayoutData = layout.CreateLayoutData(graph);

      // optionally set some port candidates for HierarchicalLayout
      var spc = new DictionaryMapper<Edge, EdgePortCandidates>();
      var tpc = new DictionaryMapper<Edge, EdgePortCandidates>();
      // e1 shall leave and enter the node on the right side
      spc[e1] = new EdgePortCandidates().AddFreeCandidate(PortSides.Right);
      // additionally set a fixed port candidate on the target side.
      PointD? offset = new PointD(15, -15);
      tpc[e1] = new EdgePortCandidates().AddFixedCandidate(PortSides.Right, offset);

      // e2 shall leave and enter the node on the top side
      spc[e2] = new EdgePortCandidates().AddFreeCandidate(PortSides.Top);
      tpc[e2] = new EdgePortCandidates().AddFreeCandidate(PortSides.Top);
      // e3 uses no port constraints, i.e. layout will choose best side
      hierarchicalLayoutData.Ports.SourcePortCandidates.Mapper = spc;
      hierarchicalLayoutData.Ports.TargetPortCandidates.Mapper = tpc;

      // prepare adding labels with preferred placements
      var preferredPlacementMap = new DictionaryMapper<EdgeLabel, EdgeLabelPreferredPlacement>();
      hierarchicalLayoutData.EdgeLabelPreferredPlacements.Mapper = preferredPlacementMap;

      // setup two edge labels for edge e1. The size of the edge labels will be set to
      // 80x20. Usually the size of the labels will be determined by
      // calculating the bounds of a piece of text that is displayed
      // with a specific font.
      var labelFactoryMap = new DictionaryMapper<EdgeLabel, EdgeLabelCandidates>();
      var label1 = AddEdgeLabel(graph, e1);
      preferredPlacementMap[label1] =
          EdgeLabelPreferredPlacement.CreateSharedPlacement(LabelAlongEdgePlacements.AtCenter, LabelEdgeSides.Anywhere);
      labelFactoryMap[label1] = new EdgeLabelCandidates().AddSliderCandidates(EdgeLabelSliderMode.Center);
      var label2 = AddEdgeLabel(graph, e1);
      preferredPlacementMap[label2] =
          EdgeLabelPreferredPlacement.CreateSharedPlacement(LabelAlongEdgePlacements.Anywhere,
              LabelEdgeSides.LeftOfEdge);
      labelFactoryMap[label2] = new EdgeLabelCandidates().AddSliderCandidates(EdgeLabelSliderMode.Side);
      preferredPlacementMap[label2] =
          EdgeLabelPreferredPlacement.CreateSharedPlacement(LabelAlongEdgePlacements.Anywhere,
              LabelEdgeSides.LeftOfEdge);

      var labelingData = layout.LayoutStages.Get<GenericLabeling>().CreateLayoutData(graph);
      labelingData.EdgeLabelCandidates.Mapper = labelFactoryMap;

      var layoutData = graph.CreateCompositeLayoutData(hierarchicalLayoutData, labelingData);
      graph.ApplyLayout(layout, layoutData);

      Console.WriteLine("\n\nGRAPH LAID OUT USING GENERIC EDGE LABELING");
      Console.WriteLine("v1 center position = " + v1.Layout.Center);
      Console.WriteLine("v2 center position = " + v2.Layout.Center);
      Console.WriteLine("v3 center position = " + v3.Layout.Center);
      Console.WriteLine("e1 path = " + string.Join(",", e1.PathPoints.ToList()));
      Console.WriteLine("e2 path = " + string.Join(",", e2.PathPoints.ToList()));
      Console.WriteLine("e3 path = " + string.Join(",", e3.PathPoints.ToList()));
      Console.WriteLine("ell1 upper left location = " + GetEdgeLabelLocation(e1.Labels[0]));
      Console.WriteLine("ell2 upper left location = " + GetEdgeLabelLocation(e1.Labels[1]));

      var gv = new GraphViewer();
      gv.AddLayoutGraph(LayoutGraph.CreateCopy(graph), "Layout with Generic Labeling");

      // Remove slider labels and add to 'free' labels
      graph.Remove(e1.Labels[0]);
      graph.Remove(e1.Labels[0]);
      preferredPlacementMap[AddEdgeLabel(graph, e1)] = new EdgeLabelPreferredPlacement();
      preferredPlacementMap[AddEdgeLabel(graph, e1)] =
          new EdgeLabelPreferredPlacement { EdgeSide = LabelEdgeSides.LeftOfEdge };

      layout.EdgeLabelPlacement = EdgeLabelPlacement.Integrated;

      graph.ApplyLayout(layout, layoutData);

      Console.WriteLine("\n\nGRAPH LAID OUT USING HIERARCHICAL LAYOUT SPECIFIC EDGE LABELING");
      Console.WriteLine("v1 center position = " + v1.Layout.Center);
      Console.WriteLine("v2 center position = " + v2.Layout.Center);
      Console.WriteLine("v3 center position = " + v3.Layout.Center);
      Console.WriteLine("e1 path = " + string.Join(",", e1.PathPoints.ToList()));
      Console.WriteLine("e2 path = " + string.Join(",", e2.PathPoints.ToList()));
      Console.WriteLine("e3 path = " + string.Join(",", e3.PathPoints.ToList()));
      Console.WriteLine("ell1 upper left location = " + GetEdgeLabelLocation(e1.Labels[0]));
      Console.WriteLine("ell2 upper left location = " + GetEdgeLabelLocation(e1.Labels[1]));

      // display the result in a simple viewer
      gv.AddLayoutGraph(graph, "Layout with Integrated Labeling");
      var application = new Application();
      application.Run(gv);
    }

    private static EdgeLabel AddEdgeLabel(LayoutGraph graph, Edge edge) {
      return graph.AddLabel(edge, new OrientedRectangle(0, 0, 80, 20));
    }

    /// <summary>
    ///   Returns the calculated location of the edge label. Note that the labeling
    ///   machinery can, depending on the layout algorithm, only find ideal label positions
    ///   without constraints on the labels. This method demonstrates how to retrieve the
    ///   best label positions in compliance with the constraints, based on that result.
    /// </summary>
    private static PointD GetEdgeLabelLocation(EdgeLabel label) {
      return new PointD(label.Layout.AnchorX, label.Layout.AnchorY - label.Layout.Height);
    }
  }
}
