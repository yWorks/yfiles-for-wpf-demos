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
using Demo.Layout.LayoutGraphViewer;
using yWorks.Algorithms;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace Demo.Layout.SwimLaneLayout
{
  /// <summary>
  /// This demo shows how to use the swim lane feature of the hierarchical layout algorithm.
  /// In this demo, nodes will be assigned to certain regions of the diagram, 
  /// the so-called swim lanes. The diagram will be arranged using hierarchical layout 
  /// style, while nodes remain within the bounds of their lanes. 
  /// This demo displays the calculated coordinates in a simple graph viewer.
  /// Additionally it outputs the calculated coordinates of the graph layout to 
  /// the console.
  /// </summary>
  public class SwimLaneLayoutDemo
  {
    /// <summary>
    /// Creates a small graph and applies a swim lane layout to it.
    /// </summary>   
    [STAThread]
    public static void Main() {
      DefaultLayoutGraph graph = new DefaultLayoutGraph();

      //construct graph. assign sizes to nodes
      Node v1 = graph.CreateNode();
      graph.SetSize(v1, 30, 30);
      Node v2 = graph.CreateNode();
      graph.SetSize(v2, 30, 30);
      Node v3 = graph.CreateNode();
      graph.SetSize(v3, 30, 30);
      Node v4 = graph.CreateNode();
      graph.SetSize(v4, 30, 30);

      // create some edges...
      Edge e1 = graph.CreateEdge(v1, v2);
      Edge e2 = graph.CreateEdge(v1, v3);
      Edge e3 = graph.CreateEdge(v2, v4);

      // create swim lane descriptors for two lanes
      var sl1 = new SwimlaneDescriptor(1);
      var sl2 = new SwimlaneDescriptor(2);

      // create a map to store the swim lane descriptors
      INodeMap slMap = graph.CreateNodeMap();

      // assign nodes to lanes 
      slMap.Set(v1, sl1);
      slMap.Set(v2, sl2);
      slMap.Set(v3, sl2);
      slMap.Set(v4, sl1);

      // register the information
      graph.AddDataProvider(HierarchicLayout.SwimlaneDescriptorDpKey, slMap);

      // create the layout algorithm
      HierarchicLayout layout = new HierarchicLayout();

      // start the layout
      new BufferedLayout(layout).ApplyLayout(graph);


      Console.WriteLine("\n\nGRAPH LAID OUT HIERARCHICALLY IN SWIMLANES");
      Console.WriteLine("v1 center position = " + graph.GetCenter(v1));
      Console.WriteLine("v2 center position = " + graph.GetCenter(v2));
      Console.WriteLine("v3 center position = " + graph.GetCenter(v3));
      Console.WriteLine("v4 center position = " + graph.GetCenter(v4));
      Console.WriteLine("e1 path = " + graph.GetPath(e1));
      Console.WriteLine("e2 path = " + graph.GetPath(e2));
      Console.WriteLine("e3 path = " + graph.GetPath(e3));
      Console.WriteLine("SwimLane 1 index = " + sl1.ComputedLaneIndex);
      Console.WriteLine("SwimLane 1 position = " + sl1.ComputedLanePosition);
      Console.WriteLine("SwimLane 1 width = " + sl1.ComputedLaneWidth);
      Console.WriteLine("SwimLane 2 index = " + sl2.ComputedLaneIndex);
      Console.WriteLine("SwimLane 2 position = " + sl2.ComputedLanePosition);
      Console.WriteLine("SwimLane 2 width = " + sl2.ComputedLaneWidth);

      //clean up data maps
      graph.DisposeNodeMap(slMap);
      graph.RemoveDataProvider(HierarchicLayout.SwimlaneDescriptorDpKey);

      //display the graph in a simple viewer
      var viewer = new GraphViewer();
      viewer.AddLayoutGraph(graph, "Swimlane Demo");
      var application = new System.Windows.Application();
      application.Run(viewer);
    }
  }
}
