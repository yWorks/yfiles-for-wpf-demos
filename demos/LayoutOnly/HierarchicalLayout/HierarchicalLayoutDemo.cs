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
using yWorks.Utils;

namespace Demo.Layout.HL
{
  /// <summary>
  ///   This demo shows how to use the hierarchical layout algorithm.
  ///   In this demo, first a graph will be laid out from scratch. Then new graph elements
  ///   will be added to the graph structure. Finally, an updated layout for the
  ///   grown graph structure will be calculated. The updated layout
  ///   will still look similar to the original layout. This feature
  ///   is called incremental layout.
  ///   This demo
  ///   displays the calculated coordinates in a simple graph viewer.
  ///   Additionally, it outputs the calculated coordinates of the graph layout to
  ///   the console.
  /// </summary>
  public class HierarchicalLayoutDemo
  {
    /// <summary>
    ///   Uses HierarchicalLayout to perform an incremental layout of a graph.
    /// </summary>
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

      // add a label to one node
      var nodeLabel = graph.AddLabel(v1, new OrientedRectangle(0, 0, 80, 20));

      var e1 = graph.CreateEdge(v1, v2);
      var e2 = graph.CreateEdge(v1, v3);

      // add a label to an edge
      var edgeLabel = graph.AddLabel(e1, new OrientedRectangle(0, 0, 80, 20));

      var layout = new HierarchicalLayout();
      layout.EdgeLabelPlacement = EdgeLabelPlacement.Integrated;
      layout.NodeLabelPlacement = NodeLabelPlacement.Consider;
      layout.FromSketchMode = false;

      var layoutData = layout.CreateLayoutData(graph);

      layoutData.EdgeLabelPreferredPlacements.Constant =
          new EdgeLabelPreferredPlacement { EdgeSide = LabelEdgeSides.LeftOfEdge };

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
      layoutData.Ports.SourcePortCandidates.Mapper = spc;
      layoutData.Ports.TargetPortCandidates.Mapper = tpc;
      graph.ApplyLayout(layout, layoutData);

      Console.WriteLine("\n\nGRAPH LAID OUT HIERARCHICALLY FROM SCRATCH");
      Console.WriteLine("v1 center position = " + v1.Layout.Center);
      Console.WriteLine("v2 center position = " + v2.Layout.Center);
      Console.WriteLine("v3 center position = " + v3.Layout.Center);
      Console.WriteLine("e1 path = " + e1.PathPoints.ToList());
      Console.WriteLine("e2 path = " + e2.PathPoints.ToList());

      // display the graph in a simple viewer
      var gv = new GraphViewer();
      gv.AddLayoutGraph(LayoutGraph.CreateCopy(graph), "Before Addition");

      // now add a node and two edges incrementally...
      var v4 = graph.CreateNode();
      v4.Layout.Size = new SizeD(30, 30);

      var e4 = graph.CreateEdge(v4, v2);
      var e3 = graph.CreateEdge(v1, v4);

      // mark elements as newly added so that the layout algorithm can place
      // them nicely.
      layoutData.IncrementalNodes.Items.Add(v4);
      layoutData.IncrementalEdges.Predicate = edge => edge == e3 || edge == e4;
      layout.FromSketchMode = true;

      graph.ApplyLayout(layout, layoutData);

      Console.WriteLine("\n\nGRAPH AFTER ELEMENTS HAVE BEEN ADDED INCREMENTALLY");
      Console.WriteLine("v1 center position = " + v1.Layout.Center);
      Console.WriteLine("v2 center position = " + v2.Layout.Center);
      Console.WriteLine("v3 center position = " + v3.Layout.Center);
      Console.WriteLine("v4 center position = " + v4.Layout.Center);
      Console.WriteLine("e1 path = " + string.Join(",", e1.PathPoints.ToList()));
      Console.WriteLine("e2 path = " + string.Join(",", e2.PathPoints.ToList()));
      Console.WriteLine("e3 path = " + string.Join(",", e3.PathPoints.ToList()));
      Console.WriteLine("e4 path = " + string.Join(",", e4.PathPoints.ToList()));

      // display the graph in a simple viewer
      gv.AddLayoutGraph(LayoutGraph.CreateCopy(graph), "After Addition");
      var application = new Application();
      application.Run(gv);
    }
  }
}
