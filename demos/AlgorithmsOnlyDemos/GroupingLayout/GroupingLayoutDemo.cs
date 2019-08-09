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
using System.Windows.Forms;

using yWorks.Algorithms;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Hierarchic;

namespace Demo.Layout.GroupingLayout
{
  /// <summary>
  /// This class shows how to use layout and grouping algorithms.
  /// This demo displays the calculated coordinates and group bounds
  /// in a simple graph viewer.
  /// In this demo HierarchicLayout is used to layout a small graph. 
  /// </summary>
  public class GroupingLayoutDemo
  {
  
    [STAThread]
    public static void Main()
    {
      new GroupingLayoutDemo().Run();
    }

   
    /// <summary>
    /// Creates a small graph and applies a hierarchic group layout to it.
    /// The output of the calculated coordinates will be displayed in the
    /// console.
    /// </summary>
    public void Run()
    {
      DefaultLayoutGraph graph = new DefaultLayoutGraph();
    
      //construct graph. assign sizes to nodes
      Node v1 = graph.CreateNode();
      graph.SetSize(v1,30,30);
      Node v2 = graph.CreateNode();
      graph.SetSize(v2,30,30);
      Node v3 = graph.CreateNode();
      graph.SetSize(v3,30,30);
      Node v4 = graph.CreateNode();
      graph.SetSize(v4,30,30);
    
      Node groupNode = graph.CreateNode();
      graph.SetSize(groupNode, 100,100);
    
      Edge e1 = graph.CreateEdge(v1,v2);
      Edge e2 = graph.CreateEdge(v4, groupNode);
      Edge e3 = graph.CreateEdge(v1,v3);
      Edge e4 = graph.CreateEdge(v1, v1);
      Edge e5 = graph.CreateEdge(v2, groupNode);
      Edge e6 = graph.CreateEdge(groupNode, v2);
 
      //optionally setup some edge groups
      IEdgeMap spg = graph.CreateEdgeMap();
      IEdgeMap tpg = graph.CreateEdgeMap();
    
      graph.AddDataProvider(PortConstraintKeys.SourceGroupIdDpKey, spg);
      graph.AddDataProvider(PortConstraintKeys.TargetGroupIdDpKey, tpg);
    
      spg.Set(e1, "SGroup1");
      spg.Set(e3, "SGroup1");
      tpg.Set(e1, "TGroup1");
      tpg.Set(e3, "TGroup1");

      //optionally setup the node grouping
      INodeMap nodeId = graph.CreateNodeMap();
      INodeMap parentNodeId = graph.CreateNodeMap();
      INodeMap groupKey = graph.CreateNodeMap();
    
      graph.AddDataProvider(GroupingKeys.NodeIdDpKey, nodeId);
      graph.AddDataProvider(GroupingKeys.ParentNodeIdDpKey, parentNodeId);
      graph.AddDataProvider(GroupingKeys.GroupDpKey, groupKey);
    
      //mark a node as a group node
      groupKey.SetBool(groupNode, true);
    
      // add ids for each node
      nodeId.Set(v1, "v1");
      nodeId.Set(v2, "v2");
      nodeId.Set(v3, "v3");
      nodeId.Set(v4, "v4");
      nodeId.Set(groupNode, "groupNode");
    
      // set the parent for each grouped node
      parentNodeId.Set(v2, "groupNode");
      parentNodeId.Set(v3, "groupNode");
    
      HierarchicLayout layout = new HierarchicLayout();
    
      layout.MinimumLayerDistance = 0;
      layout.EdgeLayoutDescriptor.MinimumDistance = 10;
  
      new BufferedLayout(layout).ApplyLayout(graph);

      Console.WriteLine("\n\nGRAPH LAID OUT USING HIERARCHICLAYOUT");
      Console.WriteLine("v1 center position = " + graph.GetCenter(v1));
      Console.WriteLine("v2 center position = " + graph.GetCenter(v2));
      Console.WriteLine("v3 center position = " + graph.GetCenter(v3));
      Console.WriteLine("v4 center position = " + graph.GetCenter(v4));
      Console.WriteLine("group center position = " + graph.GetCenter(groupNode));
      Console.WriteLine("group size = " + graph.GetSize(groupNode));
      Console.WriteLine("e1 path = " + graph.GetPath(e1));
      Console.WriteLine("e2 path = " + graph.GetPath(e2));
      Console.WriteLine("e3 path = " + graph.GetPath(e3));
      Console.WriteLine("e4 path = " + graph.GetPath(e4));
      Console.WriteLine("e5 path = " + graph.GetPath(e5));
      Console.WriteLine("e6 path = " + graph.GetPath(e4));
      
      //display the result in a simple viewer
      Application.Run(new Demo.yWorks.LayoutGraphViewer.GraphViewer(graph, "Hierarchical Group Layout"));
    }
  }
}
