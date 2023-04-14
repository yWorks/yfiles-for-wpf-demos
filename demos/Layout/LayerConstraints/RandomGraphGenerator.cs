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
using yWorks.Graph;

namespace Demo.yFiles.Layout.LayerConstraints
{
  ///<summary>
  /// A class that creates random graphs. The size of the graph and other options
  /// may be specified. These options influence the properties of the created
  /// graph.
  ///</summary>
  public class RandomGraphGenerator
  {
    private readonly Random random;

    /// <summary>
    /// Constructs a new random graph generator
    /// </summary>
    public RandomGraphGenerator() : this(DateTime.Now.Millisecond) {}

    public Func<IGraph, INode> NodeCreator { get; set; }

    /// <summary>
    /// Constructs a new random graph generator that uses
    /// the given random seed to initialize.
    /// </summary> 
    public RandomGraphGenerator(int seed) {
      NodeCreator = graph => graph.CreateNode();
      random = new Random(seed);
      NodeCount = 30;
      EdgeCount = 40;
      AllowSelfLoops = false;
      AllowCycles = true;
      AllowMultipleEdges = false;
    }

    ///<summary>
    /// Gets or sets the node count of the graph to be generated.
    /// The default value is 30.
    ///</summary>
    public int NodeCount { get; set; }

    ///<summary>
    /// Gets or sets the edge count of the graph to be generated.
    /// The default value is 40. If the edge count is 
    /// higher than it is theoretically possible by the 
    /// generator options set, then the highest possible
    /// edge count is applied instead.
    ///</summary>
    public int EdgeCount { get; set; }

    ///<summary>
    /// Whether or not to allow the generation of cyclic graphs, i.e. 
    /// graphs that contain directed cyclic paths. If allowed 
    /// it still could happen by chance that the generated
    /// graph is acyclic. By default allowed.
    ///</summary>
    public bool AllowCycles { get; set; }

    ///<summary>
    /// Whether or not to allow the generation of selfloops, i.e.
    /// edges with same source and target nodes.
    /// If allowed it still could happen by chance that
    /// the generated graph contains no selfloops.
    /// By default disallowed.
    ///</summary>
    public bool AllowSelfLoops { get; set; }

    ///<summary>
    /// Whether or not to allow the generation of graphs that contain multiple
    /// edges, i.e. graphs that has more than one edge that connect the same pair
    /// of nodes. If allowed it still could happen by chance that
    /// the generated graph does not contain multiple edges.
    /// By default disallowed.
    ///</summary>
    public bool AllowMultipleEdges { get; set; }

    ///<summary>
    /// Returns a newly created random graph that obeys the specified settings.
    ///</summary>
    public IGraph Generate() {
      IGraph graph = new DefaultGraph();
      Generate(graph);
      return graph;
    }

    ///<summary>
    ///Clears the given graph and generates new nodes and edges for it,
    /// so that the specified settings are obeyed.
    ///</summary>
    public void Generate(IGraph graph) {
      if (AllowMultipleEdges) {
        GenerateMultipleGraph(graph);
      } else if (NodeCount > 1 && EdgeCount > 10 && Math.Log(NodeCount)*NodeCount < EdgeCount) {
        GenerateDenseGraph(graph);
      } else {
        GenerateSparseGraph(graph);
      }
    }

    ///<summary>
    /// Random graph generator in case multiple edges are allowed.
    ///</summary>
    private void GenerateMultipleGraph(IGraph graph) {
      int n = NodeCount;
      int m = EdgeCount;
      IMapper<IPortOwner, int> index = new DictionaryMapper<IPortOwner, int>();

      int[] deg = new int[n];
      INode[] nodes = new INode[n];
      for (int i = 0; i < n; i++) {
        nodes[i] = CreateNode(graph);
        index[nodes[i]] = i;
      }

      for (int i = 0; i < m; i++) {
        deg[random.Next(n)]++;
      }

      for (int i = 0; i < n; i++) {
        INode v = nodes[i];
        int d = deg[i];
        while (d > 0) {
          int j = random.Next(n);
          if (j == i && (!AllowCycles || !AllowSelfLoops)) {
            continue;
          }
          graph.CreateEdge(v, nodes[j]);
          d--;
        }
      }

      if (!AllowCycles) {
        foreach (IEdge edge in graph.Edges) {
          IPort sourcePort = edge.SourcePort;
          IPort targetPort = edge.TargetPort;
          if (index[sourcePort.Owner] > index[targetPort.Owner]) {
            graph.SetEdgePorts(edge, targetPort, sourcePort);
          }
        }
      }
    }

    ///<summary>
    /// Random graph generator for dense graphs.
    ///</summary>
    private void GenerateDenseGraph(IGraph g) {
      g.Clear();
      INode[] nodes = new INode[NodeCount];

      for (int i = 0; i < NodeCount; i++) {
        nodes[i] = CreateNode(g);
      }

      RandomSupport.Permutate(random, nodes);

      int m = Math.Min(GetMaxEdges(), EdgeCount);
      int n = NodeCount;

      int adder = (AllowSelfLoops && AllowCycles) ? 0 : 1;

      bool[] edgeWanted = RandomSupport.GetBoolArray(random, GetMaxEdges(), m);
      for (int i = 0, k = 0; i < n; i++) {
        for (int j = i + adder; j < n; j++, k++) {
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

    ///<summary>
    /// Random graph generator for sparse graphs.
    ///</summary>
    private void GenerateSparseGraph(IGraph graph) {
      graph.Clear();
      IMapper<IPortOwner, int> index = new DictionaryMapper<IPortOwner, int>();

      int n = NodeCount;

      int m = Math.Min(GetMaxEdges(), EdgeCount);

      INode[] nodes = new INode[n];

      for (int i = 0; i < n; i++) {
        nodes[i] = CreateNode(graph);
        index[nodes[i]] = i;
      }

      RandomSupport.Permutate(random, nodes);

      int count = m;
      while (count > 0) {
        int vi = random.Next(n);
        INode v = nodes[vi];
        INode w = nodes[random.Next(n)];

        if (graph.GetEdge(v, w) != null || (v == w && (!AllowSelfLoops || !AllowCycles))) {
          continue;
        }
        graph.CreateEdge(v, w);
        count--;
      }

      if (!AllowCycles) {
        foreach (IEdge edge in graph.Edges) {
          IPort sourcePort = edge.SourcePort;
          IPort targetPort = edge.TargetPort;
          if (index[sourcePort.Owner] > index[targetPort.Owner]) {
            graph.SetEdgePorts(edge, targetPort, sourcePort);
          }
        }
      }
    }

    protected virtual INode CreateNode(IGraph graph) {
      return NodeCreator(graph);
    }

    ///<summary>
    /// Helper method that returns the maximum number of edges
    /// of a graph that still obeys the set structural constraints.
    ///</summary>
    private int GetMaxEdges() {
      if (AllowMultipleEdges) {
        return int.MaxValue;
      }
      int maxEdges = NodeCount*(NodeCount - 1)/2;
      if (AllowCycles && AllowSelfLoops) {
        maxEdges += NodeCount;
      }
      return maxEdges;
    }
  }

  internal static class RandomSupport
  {
    ///<summary>
    /// Permutates the positions of the elements within the given array.
    ///</summary>
    public static void Permutate(Random rnd, object[] a) {
      // forth...
      for (int i = 0; i < a.Length; i++) {
        int j = rnd.Next(a.Length);
        Object tmp = a[i];
        a[i] = a[j];
        a[j] = tmp;
      }
      // back...
      for (int i = a.Length - 1; i >= 0; i--) {
        int j = rnd.Next(a.Length);
        Object tmp = a[i];
        a[i] = a[j];
        a[j] = tmp;
      }
    }

    ///<summary>
    /// Returns an array of <paramref name="n"/> unique random integers that 
    /// lie within the range <paramref name="min"/> (inclusive) and
    /// <paramref name="max"/> (exclusive). If <c>max - min &lt; n</c>
    /// then <see langword="null"/> is returned.
    ///</summary>
    public static int[] GetUniqueArray(Random rnd, int n, int min, int max) {
      max--;

      int[] ret = null;
      int l = max - min + 1;
      if (l >= n && n > 0) {
        int[] accu = new int[l];
        ret = new int[n];
        for (int i = 0, j = min; i < l; i++, j++) {
          accu[i] = j;
        }
        for (int j = 0, m = l - 1; j < n; j++, m--) {
          int r = rnd.Next(m + 1);
          ret[j] = accu[r];
          if (r < m) {
            accu[r] = accu[m];
          }
        }
      }
      return ret;
    }

    ///<summary>
    /// Returns an array of <paramref name="n"/> randomly chosen boolean values
    /// of which <paramref name="trueCount"/> of them are <see langword="true"/>.
    /// If the requested numbers of <see langword="true"/> values is bigger than the number
    /// of requested boolean values, an Exception is raised.
    ///</summary>
    public static bool[] GetBoolArray(Random rnd, int n, int trueCount) {
      if (trueCount > n) {
        throw new ArgumentException("RandomSupport.GetBoolArray( " + n + ", " + trueCount + " )");
      }

      int[] a = GetUniqueArray(rnd, trueCount, 0, n);
      bool[] b = new bool[n];
      if (a != null) {
        for (int i = 0; i < a.Length; i++) {
          b[a[i]] = true;
        }
      }
      return b;
    }
  }
}