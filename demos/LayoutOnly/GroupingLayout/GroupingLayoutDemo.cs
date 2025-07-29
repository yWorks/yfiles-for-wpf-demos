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

namespace Demo.Layout.GroupingLayout
{
  /// <summary>
  ///   This class shows how to use layout and grouping algorithms.
  ///   This demo displays the calculated coordinates and group bounds
  ///   in a simple graph viewer.
  ///   In this demo HierarchicalLayout is used to layout a small graph.
  /// </summary>
  public class GroupingLayoutDemo
  {
    [STAThread]
    public static void Main() {
      new GroupingLayoutDemo().Run();
    }


    /// <summary>
    ///   Creates a small graph and applies a hierarchical group layout to it.
    ///   The output of the calculated coordinates will be displayed in the
    ///   console.
    /// </summary>
    public void Run() {
      var graph = new LayoutGraph();

      // construct graph. assign sizes to nodes
      var groupNode = graph.CreateGroupNode(null, new RectD(0, 0, 100, 100));

      var v1 = graph.CreateNode(new RectD(0, 0, 30, 30));
      var v2 = graph.CreateNode(groupNode, new RectD(0, 0, 30, 30));
      var v3 = graph.CreateNode(groupNode, new RectD(0, 0, 30, 30));
      var v4 = graph.CreateNode(new RectD(0, 0, 30, 30));

      var e1 = graph.CreateEdge(v1, v2);
      var e2 = graph.CreateEdge(v4, groupNode);
      var e3 = graph.CreateEdge(v1, v3);
      var e4 = graph.CreateEdge(v1, v1);
      var e5 = graph.CreateEdge(v2, groupNode);
      var e6 = graph.CreateEdge(groupNode, v2);

      var layout = new HierarchicalLayout {
          MinimumLayerDistance = 0, DefaultEdgeDescriptor = { MinimumDistance = 10 }
      };

      // optionally setup some edge groups
      var layoutData = layout.CreateLayoutData(graph);
      layoutData.SourceGroupIds.MapperFunction = e => e == e1 || e == e3 ? "SGroup1" : null;
      layoutData.TargetGroupIds.MapperFunction = e => e == e1 || e == e3 ? "TGroup1" : null;

      graph.ApplyLayout(layout, layoutData);

      Console.WriteLine("\n\nGRAPH LAID OUT USING HIERARCHICALLAYOUT");
      Console.WriteLine("v1 center position = " + v1.Layout.Center);
      Console.WriteLine("v2 center position = " + v2.Layout.Center);
      Console.WriteLine("v3 center position = " + v3.Layout.Center);
      Console.WriteLine("v4 center position = " + v4.Layout.Center);
      Console.WriteLine("group center position = " + groupNode.Layout.Center);
      Console.WriteLine("group size = " + new SizeD(groupNode.Layout.Width, groupNode.Layout.Height));
      Console.WriteLine("e1 path = " + string.Join(",", e1.PathPoints.ToList()));
      Console.WriteLine("e2 path = " + string.Join(",", e2.PathPoints.ToList()));
      Console.WriteLine("e3 path = " + string.Join(",", e3.PathPoints.ToList()));
      Console.WriteLine("e4 path = " + string.Join(",", e4.PathPoints.ToList()));
      Console.WriteLine("e5 path = " + string.Join(",", e5.PathPoints.ToList()));
      Console.WriteLine("e6 path = " + string.Join(",", e6.PathPoints.ToList()));

      // display the result in a simple viewer
      var viewer = new GraphViewer();
      viewer.AddLayoutGraph(graph, "Hierarchical Group Layout");
      var application = new Application();
      application.Run(viewer);
    }
  }
}
