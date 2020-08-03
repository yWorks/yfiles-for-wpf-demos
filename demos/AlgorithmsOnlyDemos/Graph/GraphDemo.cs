/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.Base.Graph
{
  /// <summary>
  /// Demonstrates how to use the directed graph data type Graph.
  /// </summary>
  public class GraphDemo
  {
    public static void Main() {
      //instantiates an empty graph
      yWorks.Algorithms.Graph graph = new yWorks.Algorithms.Graph();

      //create a temporary node array for fast lookup
      Node[] tmpNodes = new Node[5];

      //create some nodes in the graph and store references in the array
      for (int i = 0; i < 5; i++) {
        tmpNodes[i] = graph.CreateNode();
      }

      //create some edges in the graph
      for (int i = 0; i < 5; i++) {
        for (int j = i + 1; j < 5; j++) {
          //create an edge from node at index i to node at index j
          graph.CreateEdge(tmpNodes[i], tmpNodes[j]);
        }
      }


      //output the nodes of the graph 
      Console.WriteLine("The nodes of the graph");
      for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
        Node node = nc.Node;
        Console.WriteLine(node);
        Console.WriteLine("in edges #" + node.InDegree);
        for (IEdgeCursor ec = node.GetInEdgeCursor(); ec.Ok; ec.Next()) {
          Console.WriteLine(ec.Edge);
        }
        Console.WriteLine("out edges #" + node.OutDegree);
        for (IEdgeCursor ec = node.GetOutEdgeCursor(); ec.Ok; ec.Next()) {
          Console.WriteLine(ec.Edge);
        }
      }


      //output the edges of the graph 
      Console.WriteLine("\nThe edges of the graph");
      for (IEdgeCursor ec = graph.GetEdgeCursor(); ec.Ok; ec.Next()) {
        Console.WriteLine(ec.Edge);
      }

      //reverse edges that have consecutive neighbors in graph
      //reversing means switching source and target node
      for (IEdgeCursor ec = graph.GetEdgeCursor(); ec.Ok; ec.Next()) {
        if (Math.Abs(ec.Edge.Source.Index - ec.Edge.Target.Index) == 1) {
          graph.ReverseEdge(ec.Edge);
        }
      }

      Console.WriteLine("\nthe edges of the graph after some edge reversal");
      for (IEdgeCursor ec = graph.GetEdgeCursor(); ec.Ok; ec.Next()) {
        Console.WriteLine(ec.Edge);
      }

      ///////////////////////////////////////////////////////////////////////////
      // Node- and EdgeMap handling   ///////////////////////////////////////////
      ///////////////////////////////////////////////////////////////////////////

      //create a nodemap for the graph
      INodeMap nodeMap = graph.CreateNodeMap();
      foreach (var node in graph.Nodes) {
        //associate descriptive String to the node via a nodemap 
        nodeMap.Set(node, "this is node " + node.Index);
      }

      //create an edgemap for the graph
      IEdgeMap edgeMap = graph.CreateEdgeMap();
      foreach (var edge in graph.Edges) {
        //associate descriptive String to the edge via an edgemap
        edgeMap.Set(edge, "this is edge [" +
                          nodeMap.Get(edge.Source) + "," +
                          nodeMap.Get(edge.Target) + "]");
      }

      //output the nodemap values of the nodes
      Console.WriteLine("\nThe node map values of the graph");
      foreach (var node in graph.Nodes) {
        Console.WriteLine(nodeMap.Get(node));
      }

      //output the edges of the graph 
      Console.WriteLine("\nThe edge map values of the graph");
      foreach (var edge in graph.Edges) {
        Console.WriteLine(edgeMap.Get(edge));
      }

      //cleanup unneeded node and edge maps again (free resources)
      graph.DisposeNodeMap(nodeMap);
      graph.DisposeEdgeMap(edgeMap);

      ///////////////////////////////////////////////////////////////////////////
      // removing elements from the graph  //////////////////////////////////////
      ///////////////////////////////////////////////////////////////////////////

      for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
        //remove node that has a edge degree > 2
        if (nc.Node.Degree > 2) {
          //removed the node and all of its adjacent edges from the graph
          graph.RemoveNode(nc.Node);
        }
      }
      Console.WriteLine("\ngraph after some node removal");
      Console.WriteLine(graph);

      Console.WriteLine("\nPress key to end demo.");
      Console.ReadKey();
    }
  }
}