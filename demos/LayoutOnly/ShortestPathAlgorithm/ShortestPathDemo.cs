/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Algorithms;

namespace Demo.Base.ShortestPath
{
  /// <summary>
  /// Demonstrates the usage of the ShortestPaths class that
  /// provides easy to use algorithms for finding shortest paths 
  /// within weighted graphs.
  /// </summary>
  public class ShortestPathDemo
  {
    /// <summary>
    /// Main method:
    /// </summary>
    public static void Main() {
      // Create a random graph with the given edge and node count
      RandomGraphGenerator randomGraph = new RandomGraphGenerator(0);
      randomGraph.NodeCount = 30;
      randomGraph.EdgeCount = 60;
      Graph graph = randomGraph.Generate();

      // Create an edgemap and assign random double weights to 
      // the edges of the graph.
      IEdgeMap weightMap = graph.CreateEdgeMap();
      Random random = new Random(0);
      for (IEdgeCursor ec = graph.GetEdgeCursor(); ec.Ok; ec.Next()) {
        Edge e = ec.Edge;
        weightMap.SetDouble(e, 100.0*random.NextDouble());
      }

      // Calculate the shortest path from the first to the last node
      // within the graph
      if (!graph.Empty) {
        Node from = graph.FirstNode;
        Node to = graph.LastNode;
        double sum = 0.0;

        // The undirected case first, i.e. edges of the graph and the
        // resulting shortest path are considered to be undirected  
        EdgeList path = ShortestPaths.SingleSourceSingleSink(graph, from, to, false, weightMap);
        for (IEdgeCursor ec = path.Edges(); ec.Ok; ec.Next()) {
          Edge e = ec.Edge;
          double weight = weightMap.GetDouble(e);
          Console.WriteLine(e + " weight = " + weight);
          sum += weight;
        }
        if (sum == 0.0) {
          Console.WriteLine("NO UNDIRECTED PATH");
        } else {
          Console.WriteLine("UNDIRECTED PATH LENGTH = " + sum);
        }


        // Next the directed case, i.e. edges of the graph and the
        // resulting shortest path are considered to be directed.
        // Note that this shortest path can't be shorter then the one
        // for the undirected case

        path = ShortestPaths.SingleSourceSingleSink(graph, from, to, true, weightMap);
        sum = 0.0;
        for (IEdgeCursor ec = path.Edges(); ec.Ok; ec.Next()) {
          Edge e = ec.Edge;
          double weight = weightMap.GetDouble(e);
          Console.WriteLine(e + " weight = " + weight);
          sum += weight;
        }
        if (sum == 0.0) {
          Console.WriteLine("NO DIRECTED PATH");
        } else {
          Console.WriteLine("DIRECTED PATH LENGTH = " + sum);
        }


        Console.WriteLine("\nAuxiliary distance test\n");

        INodeMap distanceMap = graph.CreateNodeMap();
        INodeMap predMap = graph.CreateNodeMap();
        ShortestPaths.SingleSource(graph, from, true, weightMap, distanceMap, predMap);
        if (distanceMap.GetDouble(to) == double.PositiveInfinity) {
          Console.WriteLine("Distance from first to last node is infinite");
        } else {
          Console.WriteLine("Distance from first to last node is " + distanceMap.GetDouble(to));
        }

        // Dispose distanceMap since it is not needed anymore
        graph.DisposeNodeMap(distanceMap);
      }

      // Dispose weightMap since it is not needed anymore
      graph.DisposeEdgeMap(weightMap);

      Console.WriteLine("\nPress key to end demo.");
      Console.ReadKey();
    }
  }
}