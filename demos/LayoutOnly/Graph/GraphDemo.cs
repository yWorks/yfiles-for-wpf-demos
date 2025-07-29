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
using yWorks.Layout.Graph;

namespace Demo.Base.Graph
{
  /// <summary>
  ///   Demonstrates how to use the directed graph data type Graph.
  /// </summary>
  public class GraphDemo
  {
    public static void Main() {
      // instantiates an empty graph
      var graph = new LayoutGraph();

      // create a temporary node array for fast lookup
      var tmpNodes = new Node[5];

      // create some nodes in the graph and store references in the array
      for (var i = 0; i < 5; i++) {
        tmpNodes[i] = graph.CreateNode();
      }

      // create some edges in the graph
      for (var i = 0; i < 5; i++) {
        for (var j = i + 1; j < 5; j++) {
          // create an edge from node at index i to node at index j
          graph.CreateEdge(tmpNodes[i], tmpNodes[j]);
        }
      }

      // output the nodes of the graph
      Console.WriteLine("The nodes of the graph");
      foreach (var node in graph.Nodes) {
        Console.WriteLine(node);
        Console.WriteLine("in edges #" + node.InDegree);
        foreach (var inEdge in node.InEdges) {
          Console.WriteLine(inEdge);
        }
        Console.WriteLine("out edges #" + node.OutDegree);
        foreach (var outEdge in node.OutEdges) {
          Console.WriteLine(outEdge);
        }
      }

      // output the edges of the graph
      Console.WriteLine("\nThe edges of the graph");
      foreach (var edge in graph.Edges) {
        Console.WriteLine(edge);
      }

      // reverse edges that have consecutive neighbors in graph
      // reversing means switching source and target node
      foreach (var edge in graph.Edges) {
        if (Math.Abs(edge.Source.Index - edge.Target.Index) == 1) {
          graph.ReverseEdge(edge);
        }
      }

      Console.WriteLine("\nthe edges of the graph after some edge reversal");
      foreach (var edge in graph.Edges) {
        Console.WriteLine(edge);
      }

      ///////////////////////////////////////////////////////////////////////////
      // Node- and EdgeMap handling   ///////////////////////////////////////////
      ///////////////////////////////////////////////////////////////////////////

      // create a nodemap for the graph
      var nodeMap = graph.CreateNodeDataMap<string>();
      foreach (var node in graph.Nodes) {
        // associate descriptive String to the node via a nodemap
        nodeMap[node] = "this is node " + node.Index;
      }

      // create an edgemap for the graph
      var edgeMap = graph.CreateEdgeDataMap<string>();
      foreach (var edge in graph.Edges) {
        // associate descriptive String to the edge via an edgemap
        edgeMap[edge] = "this is edge [" + nodeMap[edge.Source] + "," + nodeMap[edge.Target] + "]";
      }

      // output the nodemap values of the nodes
      Console.WriteLine("\nThe node map values of the graph");
      foreach (var node in graph.Nodes) {
        Console.WriteLine(nodeMap[node]);
      }

      // output the edges of the graph
      Console.WriteLine("\nThe edge map values of the graph");
      foreach (var edge in graph.Edges) {
        Console.WriteLine(edgeMap[edge]);
      }

      // cleanup unneeded node and edge maps again (free resources)
      graph.DisposeDataMap(nodeMap);
      graph.DisposeDataMap(edgeMap);

      ///////////////////////////////////////////////////////////////////////////
      // removing elements from the graph  //////////////////////////////////////
      ///////////////////////////////////////////////////////////////////////////

      foreach (var node in graph.Nodes) {
        // remove node that has a edge degree > 2
        if (node.Degree > 2) {
          // removed the node and all of its adjacent edges from the graph
          graph.Remove(node);
        }
      }
      Console.WriteLine("\ngraph after some node removal");
      Console.WriteLine(graph);

      Console.WriteLine("\nPress key to end demo.");
      Console.ReadKey();
    }
  }
}
