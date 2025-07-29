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
using yWorks.Analysis;
using yWorks.Layout.Graph;

namespace Demo.Base.ShortestPath
{
  /// <summary>
  ///   Demonstrates the usage of the ShortestPaths class that
  ///   provides easy-to-use algorithms for finding shortest paths
  ///   within weighted graphs.
  /// </summary>
  public class ShortestPathDemo
  {
    /// <summary>
    ///   Main method:
    /// </summary>
    public static void Main() {
      // Create a random graph with the given edge and node count
      var randomGraph = new RandomGraphGenerator(0);
      randomGraph.NodeCount = 30;
      randomGraph.EdgeCount = 60;
      var graph = randomGraph.Generate();

      // Create an edgemap and assign random double weights to
      // the edges of the graph.
      var weightMap = graph.CreateEdgeDataMap<double>();
      var random = new Random(0);
      foreach (var e in graph.Edges) {
        weightMap[e] = 100.0 * random.NextDouble();
      }

      // Calculate the shortest path from the first to the last node
      // within the graph
      if (!graph.IsEmpty) {
        var from = graph.Nodes.First;
        var to = graph.Nodes.Last;
        var sum = 0.0;

        // The undirected case first, i.e. edges of the graph and the
        // resulting shortest path are considered to be undirected
        var path = LayoutGraphAlgorithms.ShortestPath(graph, from, to, false, weightMap);
        foreach (var e in path) {
          var weight = weightMap[e];
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

        path = LayoutGraphAlgorithms.ShortestPath(graph, from, to, true, weightMap);
        sum = 0.0;
        foreach (var e in path) {
          var weight = weightMap[e];
          Console.WriteLine(e + " weight = " + weight);
          sum += weight;
        }
        if (sum == 0.0) {
          Console.WriteLine("NO DIRECTED PATH");
        } else {
          Console.WriteLine("DIRECTED PATH LENGTH = " + sum);
        }

        Console.WriteLine("\nAuxiliary distance test\n");

        var distanceMap = graph.CreateNodeDataMap<double>();
        var predMap = graph.CreateNodeDataMap<Edge>();
        LayoutGraphAlgorithms.SingleSourceShortestPath(graph, from, true, weightMap, distanceMap, predMap);
        if (distanceMap[to] == double.PositiveInfinity) {
          Console.WriteLine("Distance from first to last node is infinite");
        } else {
          Console.WriteLine("Distance from first to last node is " + distanceMap[to]);
        }

        // Dispose distanceMap since it is not needed anymore
        graph.DisposeDataMap(distanceMap);
      }

      // Dispose weightMap since it is not needed anymore
      graph.DisposeDataMap(weightMap);

      Console.WriteLine("\nPress key to end demo.");
      Console.ReadKey();
    }
  }
}
