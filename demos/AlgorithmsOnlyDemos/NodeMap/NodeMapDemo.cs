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
using System.Collections.Generic;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;

namespace Demo.Base.NodeMap
{
  /// <summary>
  /// This class compares the performance of different 
  /// mechanisms to bind extra data to the nodes of a graph.
  /// In scenarios where the indices of the nodes do not change
  /// while the extra node data is needed, it is best to use array based
  /// mechanisms that use the index of a node to access the data.
  /// In scenarios where the indices of the nodes will change
  /// while the extra node data is needed, it is necessary to
  /// use node maps that do not depend on the indices of the nodes
  /// (see {@link Node#index()}) or Map implementations like Dictionary
  /// provided by the collections framework.
  /// </summary>
  public class NodeMapDemo
  {
    private static readonly Timer t1 = new Timer();
    private static readonly Timer t2 = new Timer();
    private static readonly Timer t3 = new Timer();
    private static readonly Timer t4 = new Timer();
    private static readonly Timer t5 = new Timer();
    private static readonly Timer t6 = new Timer();

    public static void Main() {
      Graph graph = new Graph();

      const int nodes = 30000;
      const int loops = 10;

      const int outerLoops = 20;

      for (int i = 0; i < nodes; i++) {
        graph.CreateNode();
      }

      for (int loop = 0; loop < outerLoops; loop++) {
        Console.Write(".");

        t1.Start();
        INodeMap map = graph.CreateNodeMap();
        for (int i = 0; i < loops; i++) {
          for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
            Node v = nc.Node;
            map.SetInt(v, i);
            i = map.GetInt(v);
          }
        }
        graph.DisposeNodeMap(map);
        t1.Stop();


        t2.Start();
        map = Maps.CreateIndexNodeMap(new int[graph.N]);
        for (int i = 0; i < loops; i++) {
          for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
            Node v = nc.Node;
            map.SetInt(v, i);
            map.GetInt(v);
          }
        }
        t2.Stop();


        t3.Start();
        map = Maps.CreateHashedNodeMap();
        for (int i = 0; i < loops; i++) {
          for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
            Node v = nc.Node;
            map.SetInt(v, i);
            i = map.GetInt(v);
          }
        }
        t3.Stop();

        t4.Start();
        int[] array = new int[graph.N];
        for (int i = 0; i < loops; i++) {
          for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
            int vid = nc.Node.Index;
            array[vid] = i;
            i = array[vid];
          }
        }
        t4.Stop();


        t5.Start();
        IDictionary<Node, int> dictionary = new Dictionary<Node, int>(2*graph.N + 1); //use map with good initial size
        for (int i = 0; i < loops; i++) {
          for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
            Node v = nc.Node;
            dictionary[v] = i;
            i = dictionary[v];
          }
        }
        t5.Stop();

        t6.Start();
        IDictionary<Node, object> objectDictionary = new Dictionary<Node, object>(2*graph.N + 1);
          //use map with good initial size
        for (int i = 0; i < loops; i++) {
          for (INodeCursor nc = graph.GetNodeCursor(); nc.Ok; nc.Next()) {
            Node v = nc.Node;
            objectDictionary[v] = i;
            i = (int) objectDictionary[v];
          }
        }
        t6.Stop();
      }

      Console.WriteLine("");
      Console.WriteLine("TIME:  standard NodeMap  : " + t1);
      Console.WriteLine("TIME:  index    NodeMap  : " + t2);
      Console.WriteLine("TIME:  hashed   NodeMap  : " + t3);
      Console.WriteLine("TIME:  plain array       : " + t4);
      Console.WriteLine("TIME:  Dictionary        : " + t5);
      Console.WriteLine("TIME:  object Dictionary : " + t6);

      Console.WriteLine("\nPress key to end demo.");
      Console.ReadKey();
    }
  }

  internal sealed class Timer
  {
    private TimeSpan startTime;
    private TimeSpan sum = TimeSpan.Zero;

    public void Start() {
      startTime = DateTime.Now.TimeOfDay;
    }

    public void Stop() {
      sum += DateTime.Now.TimeOfDay - startTime;
    }

    public override string ToString() {
      return sum.ToString();
    }
  }
}