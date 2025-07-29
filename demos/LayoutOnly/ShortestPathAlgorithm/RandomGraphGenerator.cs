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
using System.Collections.Generic;
using System.Linq;
using yWorks.Layout.Graph;

namespace Demo.Base.ShortestPath
{
  /// <summary>
  ///   A class that creates random graphs. The size of the graph and other options
  ///   may be specified. These options influence the properties of the created
  ///   graph.
  /// </summary>
  public class RandomGraphGenerator
  {
    private readonly Random random;

    /// <summary>
    ///   Constructs a new random graph generator
    /// </summary>
    public RandomGraphGenerator() : this(DateTime.Now.Millisecond) { }

    /// <summary>
    ///   Constructs a new random graph generator that uses
    ///   the given random seed to initialize.
    /// </summary>
    public RandomGraphGenerator(int seed) {
      random = new Random(seed);
      NodeCount = 30;
      EdgeCount = 40;
      AllowSelfLoops = false;
      AllowCycles = true;
      AllowMultipleEdges = false;
    }

    /// <summary>
    ///   Gets or sets the node count of the graph to be generated.
    ///   The default value is 30.
    /// </summary>
    public int NodeCount { set; get; }

    /// <summary>
    ///   Gets or sets the edge count of the graph to be generated.
    ///   The default value is 40. If the edge count is
    ///   higher than it is theoretically possible by the
    ///   generator options set, then the highest possible
    ///   edge count is applied instead.
    /// </summary>
    public int EdgeCount { set; get; }

    /// <summary>
    ///   Whether or not to allow the generation of cyclic graphs, i.e.
    ///   graphs that contain directed cyclic paths. If allowed,
    ///   it could still happen by chance that the generated
    ///   graph is acyclic. By default, allowed.
    /// </summary>
    public bool AllowCycles { set; get; }

    /// <summary>
    ///   Whether or not to allow the generation of self-loops, i.e.
    ///   edges with the same source and target nodes.
    ///   If allowed, it still could happen by chance that
    ///   the generated graph contains no self-loops.
    ///   By default, disallowed.
    /// </summary>
    public bool AllowSelfLoops { set; get; }

    /// <summary>
    ///   Whether or not to allow the generation of graphs that contain multiple
    ///   edges, i.e. graphs that have more than one edge that connect the same pair
    ///   of nodes. If allowed, it still could happen by chance that
    ///   the generated graph does not contain multiple edges.
    ///   By default, disallowed.
    /// </summary>
    public bool AllowMultipleEdges { set; get; }

    /// <summary>
    ///   Returns a newly created random graph that obeys the specified settings.
    /// </summary>
    public LayoutGraph Generate() {
      var graph = new LayoutGraph();
      Generate(graph);
      return graph;
    }

    /// <summary>
    ///   Clears the given graph and generates new nodes and edges for it,
    ///   so that the specified settings are obeyed.
    /// </summary>
    public void Generate(LayoutGraph graph) {
      if (AllowMultipleEdges) {
        GenerateMultipleGraph(graph);
      } else if (NodeCount > 1 && EdgeCount > 10 && Math.Log(NodeCount) * NodeCount < EdgeCount) {
        GenerateDenseGraph(graph);
      } else {
        GenerateSparseGraph(graph);
      }
    }

    /// <summary>
    ///   Random graph generator in case multiple edges are allowed.
    /// </summary>
    private void GenerateMultipleGraph(LayoutGraph G) {
      var n = NodeCount;
      var m = EdgeCount;
      var index = new Dictionary<Node, int>();

      var deg = new int[n];
      var V = new Node[n];
      for (var i = 0; i < n; i++) {
        V[i] = G.CreateNode();
        index[V[i]] = i;
      }

      for (var i = 0; i < m; i++) {
        deg[random.Next(n)]++;
      }

      for (var i = 0; i < n; i++) {
        var v = V[i];
        var d = deg[i];
        while (d > 0) {
          var j = random.Next(n);
          if (j == i && (!AllowCycles || !AllowSelfLoops)) {
            continue;
          }
          G.CreateEdge(v, V[j]);
          d--;
        }
      }

      if (!AllowCycles) {
        foreach (var edge in G.Edges) {
          if (index[edge.Source] > index[edge.Target]) {
            G.ChangeEdge(edge, edge.Target, edge.Source);
          }
        }
      }
    }

    /// <summary>
    ///   Random graph generator for dense graphs.
    /// </summary>
    private void GenerateDenseGraph(LayoutGraph g) {
      g.Clear();
      var nodes = new Node[NodeCount];

      for (var i = 0; i < NodeCount; i++) {
        nodes[i] = g.CreateNode();
      }

      RandomSupport.Permutate(random, nodes);

      var m = Math.Min(GetMaxEdges(), EdgeCount);
      var n = NodeCount;

      var adder = AllowSelfLoops && AllowCycles ? 0 : 1;

      var edgeWanted = RandomSupport.GetBoolArray(random, GetMaxEdges(), m);
      for (int i = 0, k = 0; i < n; i++) {
        for (var j = i + adder; j < n; j++, k++) {
          if (edgeWanted[k]) {
            if (AllowCycles && random.NextDouble() > 0.5f) {
              g.CreateEdge(nodes[j], nodes[i]);
            } else {
              g.CreateEdge(nodes[i], nodes[j]);
            }
          }
        }
      }
    }

    /// <summary>
    ///   Random graph generator for sparse graphs.
    /// </summary>
    private void GenerateSparseGraph(LayoutGraph G) {
      G.Clear();
      var index = new Dictionary<Node, int>();

      var n = NodeCount;

      var m = Math.Min(GetMaxEdges(), EdgeCount);

      var V = new Node[n];

      for (var i = 0; i < n; i++) {
        V[i] = G.CreateNode();
        index[V[i]] = i;
      }

      RandomSupport.Permutate(random, V);

      var count = m;
      while (count > 0) {
        var vi = random.Next(n);
        var v = V[vi];
        var w = V[random.Next(n)];

        if (G.GetEdgesBetween(v, w).Any() || (v == w && (!AllowSelfLoops || !AllowCycles))) {
          continue;
        }
        G.CreateEdge(v, w);
        count--;
      }

      if (!AllowCycles) {
        foreach (var edge in G.Edges) {
          if (index[edge.Source] > index[edge.Target]) {
            G.ChangeEdge(edge, edge.Target, edge.Source);
          }
        }
      }
    }

    /// <summary>
    ///   Helper method that returns the maximum number of edges
    ///   of a graph that still obeys the set structural constraints.
    /// </summary>
    private int GetMaxEdges() {
      if (AllowMultipleEdges) {
        return int.MaxValue;
      }
      var maxEdges = NodeCount * (NodeCount - 1) / 2;
      if (AllowCycles && AllowSelfLoops) {
        maxEdges += NodeCount;
      }
      return maxEdges;
    }
  }
}
