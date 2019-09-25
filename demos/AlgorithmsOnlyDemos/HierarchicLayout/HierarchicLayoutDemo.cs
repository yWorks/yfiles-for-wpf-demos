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
using Demo.Layout.LayoutGraphViewer;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;

namespace Demo.Layout.HL
{
  /// <summary>
  /// This demo shows how to use the hierarchical layout algorithm.
  /// In this demo, first a graph will be laid out from scratch. Then new graph elements 
  /// will be added to the graph structure. Finally, an updated layout for the 
  /// grown graph structure will be calculated. The updated layout 
  /// will still look similar to the original layout. This feature
  /// is called incremental layout.
  /// This demo
  /// displays the calculated coordinates in a simple graph viewer.
  /// Additionally it outputs the calculated coordinates of the graph layout to 
  /// the console.
  /// </summary>
  public class HierarchicLayoutDemo
  {
    /// <summary>
    /// Uses HierarchicLayout to perform an incremental layout of a graph.
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

      // add a label to one node
      var nodeLabelLayoutModel = new DiscreteNodeLabelLayoutModel(DiscreteNodeLabelPositions.InternalMask);
      var labelLayoutFactory = LayoutGraphUtilities.GetLabelFactory(graph);
      labelLayoutFactory.AddLabelLayout(v1, labelLayoutFactory.CreateLabelLayout(v1,
          new YOrientedRectangle(0, 0, 80, 20), nodeLabelLayoutModel));

      Edge e1 = graph.CreateEdge(v1, v2);
      Edge e2 = graph.CreateEdge(v1, v3);

      // add a label to an edge
      var edgeLabelLayoutModel = new SliderEdgeLabelLayoutModel(SliderMode.Side);

      labelLayoutFactory.AddLabelLayout(e1, labelLayoutFactory.CreateLabelLayout(e1,
          new YOrientedRectangle(0, 0, 80, 20), edgeLabelLayoutModel,
          PreferredPlacementDescriptor.NewSharedInstance(LabelPlacements.LeftOfEdge)));

      //optionally setup some port constraints for HierarchicLayout
      IEdgeMap spc = graph.CreateEdgeMap();
      IEdgeMap tpc = graph.CreateEdgeMap();
      //e1 shall leave and enter the node on the right side
      spc.Set(e1, PortConstraint.Create(PortSide.East));
      //additionally set a strong port constraint on the target side. 
      tpc.Set(e1, PortConstraint.Create(PortSide.East, true));
      //ports with strong port constraints will not be reset by the 
      //layout algorithm.  So we specify the target port right now to connect 
      //to the upper left corner of the node 
      graph.SetTargetPointRel(e1, new YPoint(15, -15));

      //e2 shall leave and enter the node on the top side
      spc.Set(e2, PortConstraint.Create(PortSide.North));
      tpc.Set(e2, PortConstraint.Create(PortSide.North));

      graph.AddDataProvider(PortConstraintKeys.SourcePortConstraintDpKey, spc);
      graph.AddDataProvider(PortConstraintKeys.TargetPortConstraintDpKey, tpc);

      HierarchicLayout layout = new HierarchicLayout();
      layout.IntegratedEdgeLabeling = true;
      layout.ConsiderNodeLabels = true;
      layout.LayoutMode = LayoutMode.FromScratch;

      new BufferedLayout(layout).ApplyLayout(graph);

      Console.WriteLine("\n\nGRAPH LAID OUT HIERARCHICALLY FROM SCRATCH");
      Console.WriteLine("v1 center position = " + graph.GetCenter(v1));
      Console.WriteLine("v2 center position = " + graph.GetCenter(v2));
      Console.WriteLine("v3 center position = " + graph.GetCenter(v3));
      Console.WriteLine("e1 path = " + graph.GetPath(e1));
      Console.WriteLine("e2 path = " + graph.GetPath(e2));

      //display the graph in a simple viewer
      GraphViewer gv = new GraphViewer();
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "Before Addition");

      // now add a node and two edges incrementally...
      Node v4 = graph.CreateNode();
      graph.SetSize(v4, 30, 30);

      Edge e4 = graph.CreateEdge(v4, v2);
      Edge e3 = graph.CreateEdge(v1, v4);

      //mark elements as newly added so that the layout algorithm can place 
      //them nicely.
      IIncrementalHintsFactory ihf = layout.CreateIncrementalHintsFactory();
      IDataMap map = Maps.CreateHashedDataMap();
      map.Set(v4, ihf.CreateLayerIncrementallyHint(v4));
      map.Set(e3, ihf.CreateSequenceIncrementallyHint(e3));
      map.Set(e4, ihf.CreateSequenceIncrementallyHint(e4));
      graph.AddDataProvider(HierarchicLayout.IncrementalHintsDpKey, map);
      layout.LayoutMode = LayoutMode.Incremental;

      new BufferedLayout(layout).ApplyLayout(graph);

      Console.WriteLine("\n\nGRAPH AFTER ELEMENTS HAVE BEEN ADDED INCREMENTALLY");
      Console.WriteLine("v1 center position = " + graph.GetCenter(v1));
      Console.WriteLine("v2 center position = " + graph.GetCenter(v2));
      Console.WriteLine("v3 center position = " + graph.GetCenter(v3));
      Console.WriteLine("v4 center position = " + graph.GetCenter(v4));
      Console.WriteLine("e1 path = " + graph.GetPath(e1));
      Console.WriteLine("e2 path = " + graph.GetPath(e2));
      Console.WriteLine("e3 path = " + graph.GetPath(e3));
      Console.WriteLine("e4 path = " + graph.GetPath(e4));

      //clean up data maps
      graph.RemoveDataProvider(HierarchicLayout.IncrementalHintsDpKey);

      //display the graph in a simple viewer
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "After Addition");
      var application = new System.Windows.Application();
      application.Run(gv);
    }
  }
}
