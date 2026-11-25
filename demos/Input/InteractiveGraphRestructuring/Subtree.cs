/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.InteractiveGraphRestructuring
{
  /// <summary>
  ///   Holds the nodes and edges of a subtree rooted at a specific node.
  /// </summary>
  internal sealed class Subtree
  {
    private readonly IGraph graph;

    public INode Root { get; }

    public HashSet<INode> Nodes { get; } = [];

    public HashSet<IEdge> Edges { get; } = [];

    /// <summary>
    ///   Gets or sets the new parent of the subtree.
    /// </summary>
    public INode? NewParent { get; set; }

    /// <summary>
    ///   Returns the edge connecting the parent and the root.
    /// </summary>
    public IEdge? ParentToRootEdge => graph.InEdgesAt(Root).FirstOrDefault();

    /// <summary>
    ///   Returns the parent node of the subtree.
    /// </summary>
    public INode? Parent => ParentToRootEdge?.GetSourceNode();

    /// <summary>
    ///   Returns the bounds including the nodes of the subtree.
    /// </summary>
    public RectD Bounds => Nodes.Aggregate(RectD.Empty, (current, node) => current + node.Layout.ToRectD());

    /// <summary>
    ///   Initializes a subtree with the given root node.
    /// </summary>
    /// <param name="graph">The graph in which the subtree lives</param>
    /// <param name="root">The root of the subtree</param>
    public Subtree(IGraph graph, INode root) {
      this.graph = graph;
      Root = root;
      InitializeSubtree(root);
      NewParent = Parent;
    }

    /// <summary>
    ///   Sets the parent node of the subtree.
    /// </summary>
    /// <param name="parent">The parent node of the subtree</param>
    public void SetParent(INode? parent) {
      if (parent is null || ParentToRootEdge is not { } edge) {
        return;
      }
      graph.SetEdgePorts(edge, parent.Ports.First(), edge.TargetPort);
      graph.ClearBends(edge);
    }

    /// <summary>
    ///   Determines the nodes and edges of a subtree of a given root.
    /// </summary>
    /// <param name="root">The root node of the subtree</param>
    private void InitializeSubtree(INode root) {
      foreach (var edge in graph.OutEdgesAt(root)) {
        Edges.Add(edge);
        InitializeSubtree(edge.GetTargetNode());
      }
      Nodes.Add(root);
    }
  }
}
