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
using yWorks.Graph;

namespace Demo.Base.TreeGenerator
{
  ///<summary>
  /// A class that creates tree structures. The size of the graph and other options
  /// may be specified. These options influence the properties of the created
  /// tree.
  ///</summary>
  public class TreeGenerator
  {
    /// <summary>
    /// The maximum depth of the tree, i.e. the number of layers.
    /// </summary>
    /// <remarks>This value need not actually be reached, depending on the other properties.</remarks>
    public int MaxDepth {
      get { return maxDepth; }
      set { maxDepth = value; }
    }

    /// <summary>
    /// The maximum number of direct children of each node.
    /// </summary>
    public int MaxChildCount {
      get { return maxChildCount; }
      set { maxChildCount = value; }
    }

    /// <summary>
    /// The maximum number of nodes in the resulting tree.
    /// </summary>
    public int NodeCount {
      get { return nodeCount; }
      set { nodeCount = value; }
    }

    private int maxDepth = 5;
    private int maxChildCount = 4;
    private int nodeCount = 30;
    private readonly Random random;

    public TreeGenerator() : this(DateTime.Now.Millisecond) {}

    public TreeGenerator(int seed) {
      random = new Random(seed);
    }

    public IGraph Generate() {
      IGraph g = new DefaultGraph();
      Generate(g);
      return g;
    }

    public void Generate(IGraph g) {
      g.Clear();

      INode[] nodes = new INode[NodeCount];
      int[] depth = new int[NodeCount];

      // create root
      nodes[0] = g.CreateNode();
      depth[0] = 0;

      int saturatedCount = (maxDepth <= 0 || maxChildCount <= 0) ? 1 : 0;

      // create more nodes; create edges
      for (int childID = 1; childID < nodeCount && saturatedCount < childID; childID++) {
        int parentID = random.Next(saturatedCount, childID);
        INode parent = nodes[parentID];
        INode child = nodes[childID] = g.CreateNode();
        depth[childID] = depth[parentID] + 1;

        g.CreateEdge(parent, child);


        if (g.OutDegree(parent) >= maxChildCount) {
          //parent is saturated
          int parentDepth = depth[parentID];
          depth[parentID] = depth[saturatedCount];
          depth[saturatedCount] = parentDepth;

          nodes[parentID] = nodes[saturatedCount];
          nodes[saturatedCount] = parent;
          saturatedCount++;
        }
        if (depth[childID] >= maxDepth) {
          //child is saturated
          int childDepth = depth[childID];
          depth[childID] = depth[saturatedCount];
          depth[saturatedCount] = childDepth;

          nodes[childID] = nodes[saturatedCount];
          nodes[saturatedCount] = child;
          saturatedCount++;
        }
      }
    }
  }
}