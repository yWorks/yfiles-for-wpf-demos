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

using System.Collections.Generic;
using System.Linq;
using yWorks.Graph;

namespace Demo.yFiles.Graph.RelocateSubtree
{
  /// <summary>
  /// Holds the nodes and edges of a subtree.
  /// </summary>
  internal class Subtree
  {
    /// <summary>
    /// Initializes a subtree with the given root node.
    /// </summary>
    /// <param name="graph">The graph in which the subtree lives.</param>
    /// <param name="root">The root of the subtree.</param>
    public Subtree(IGraph graph, INode root) {
      this.graph = graph;
      Root = root;
      Nodes = new HashSet<INode>();
      Edges = new HashSet<IEdge>();
      InitializeSubtree(Root);
      NewParent = Parent;
    }

    /// <summary>
    /// The graph containing the subtree.
    /// </summary>
    private readonly IGraph graph;

    /// <summary>
    /// The root of the subtree.
    /// </summary>
    public INode Root { get; private set; }

    /// <summary>
    /// The nodes of the subtree.
    /// </summary>
    public ICollection<INode> Nodes { get; private set; }

    /// <summary>
    /// The edges of the subtree.
    /// </summary>
    public ICollection<IEdge> Edges { get; private set; }

    /// <summary>
    /// The edge connecting <see cref="Parent"/> and <see cref="Root"/>.
    /// </summary>
    public IEdge ParentToRootEdge {
      get {
        return graph.InEdgesAt(Root).FirstOrDefault();
      }
    }

    /// <summary>
    /// The parent node of the subtree.
    /// </summary>
    public INode Parent {
      get {
        return ParentToRootEdge != null ? ParentToRootEdge.GetSourceNode() : null;
      }
      set {
        if (value != null && ParentToRootEdge != null) {
          graph.SetEdgePorts(ParentToRootEdge, value.Ports.FirstOrDefault(), ParentToRootEdge.TargetPort);
          graph.ClearBends(ParentToRootEdge);
        }
      }
    }

    /// <summary>
    /// The node that will become the new parent when the relocation is finished.
    /// </summary>
    public INode NewParent { get; set; }

    /// <summary>
    /// Determines the nodes and edges of a subtree of a given root.
    /// </summary>
    /// <param name="root">The root node of the subtree.</param>
    private void InitializeSubtree(INode root) {
      var outEdges = graph.OutEdgesAt(root);
      foreach (var outEdge in outEdges) {
        Edges.Add(outEdge);
        InitializeSubtree(outEdge.GetTargetNode());
      }
      Nodes.Add(root);
    }
  }
}
