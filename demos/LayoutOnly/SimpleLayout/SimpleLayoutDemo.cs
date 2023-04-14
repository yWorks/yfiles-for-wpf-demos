/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Router.Polyline;

namespace Demo.Layout.BasicLayout
{
  /// <summary>
  /// This class shows how to use important layout and routing algorithms
  /// to calculate coordinates for a graph. 
  /// Featured layout and routing styles are Organic, Orthogonal (Router), 
  /// Orthogonal (Layout), Circular and Hierarchical.
  /// 
  /// The resulting diagrams will be displayed in a simple graph viewer. 
  /// Step to the next layout style by closing the window that was opened 
  /// for the current layout style.
  /// </summary>
  public class SimpleLayoutDemo
  {
    /// <summary>
    /// Creates a small graph and applies an hierarchic layout to it.
    /// Two different kinds of edge labeling mechanisms will be applied
    /// to the graph.
    /// The output of the calculated coordinates will be displayed in the
    /// console.
    /// </summary>
    [STAThread]
    public static void Main() {
      DefaultLayoutGraph graph = new DefaultLayoutGraph();

      //construct graph and assign sizes to its nodes
      Node[] nodes = new Node[16];
      for (int i = 0; i < 16; i++) {
        nodes[i] = graph.CreateNode();
        graph.SetSize(nodes[i], 30, 30);
      }
      graph.CreateEdge(nodes[0], nodes[1]);
      graph.CreateEdge(nodes[0], nodes[2]);
      graph.CreateEdge(nodes[0], nodes[3]);
      graph.CreateEdge(nodes[0], nodes[14]);
      graph.CreateEdge(nodes[2], nodes[4]);
      graph.CreateEdge(nodes[3], nodes[5]);
      graph.CreateEdge(nodes[3], nodes[6]);
      graph.CreateEdge(nodes[3], nodes[9]);
      graph.CreateEdge(nodes[4], nodes[7]);
      graph.CreateEdge(nodes[4], nodes[8]);
      graph.CreateEdge(nodes[5], nodes[9]);
      graph.CreateEdge(nodes[6], nodes[10]);
      graph.CreateEdge(nodes[7], nodes[11]);
      graph.CreateEdge(nodes[8], nodes[12]);
      graph.CreateEdge(nodes[8], nodes[15]);
      graph.CreateEdge(nodes[9], nodes[13]);
      graph.CreateEdge(nodes[10], nodes[13]);
      graph.CreateEdge(nodes[10], nodes[14]);
      graph.CreateEdge(nodes[12], nodes[15]);

      GraphViewer gv = new GraphViewer();

      //using organic layout style
      OrganicLayout organic = new OrganicLayout();
      organic.QualityTimeRatio = 1.0;
      organic.NodeOverlapsAllowed = false;
      organic.MinimumNodeDistance = 10;
      organic.PreferredEdgeLength = 40;
      new BufferedLayout(organic).ApplyLayout(graph);
      LayoutGraphUtilities.ClipEdgesOnBounds(graph);
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "Organic Layout Style");

      //using orthogonal edge router (node positions stay fixed)
      EdgeRouter router = new EdgeRouter();
      new BufferedLayout(router).ApplyLayout(graph);
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "Polyline Edge Router");


      //using orthogonal layout style
      OrthogonalLayout orthogonal = new OrthogonalLayout();
      orthogonal.GridSpacing = 15;
      orthogonal.OptimizePerceivedBends = true;
      new BufferedLayout(orthogonal).ApplyLayout(graph);
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "Orthogonal Layout Style");


      //using circular layout style
      CircularLayout circular = new CircularLayout();
      circular.BalloonLayout.MinimumEdgeLength = 20;
      circular.BalloonLayout.CompactnessFactor = 0.1;
      new BufferedLayout(circular).ApplyLayout(graph);
      LayoutGraphUtilities.ClipEdgesOnBounds(graph);
      gv.AddLayoutGraph(new CopiedLayoutGraph(graph), "Circular Layout Style");

      //using hierarchical layout style
      var hierarchic = new HierarchicLayout();
      new BufferedLayout(hierarchic).ApplyLayout(graph);
      gv.AddLayoutGraph(graph, "Hierarchical Layout Style");

      var application = new System.Windows.Application();
      application.Run(gv);
    }
  }
}
