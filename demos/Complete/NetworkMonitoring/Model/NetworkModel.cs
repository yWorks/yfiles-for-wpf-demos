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

using System.Collections.Generic;
using System.Linq;

namespace Demo.yFiles.Graph.NetworkMonitoring.Model
{
  /// <summary>
  /// Class modeling the network as a separate graph.
  /// </summary>
  public class NetworkModel
  {
    /// <summary>Nodes in the network.</summary>
    private readonly List<ModelNode> nodes;

    /// <summary>Edges in the network.</summary>
    private readonly List<ModelEdge> edges;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkModel"/> class with the given nodes and edges.
    /// </summary>
    /// <param name="nodes">The nodes in the network.</param>
    /// <param name="edges">The edges in the network.</param>
    public NetworkModel(IEnumerable<ModelNode> nodes, IEnumerable<ModelEdge> edges) {
      this.nodes = nodes.ToList();
      this.edges = edges.ToList();
    }

    /// <summary>
    /// Returns the edges having the given node as either source or target.
    /// </summary>
    /// <param name="node">The node to find connected edges of.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of the edges that are connected to the node.</returns>
    public IEnumerable<ModelEdge> GetAdjacentEdges(ModelNode node) {
      return edges.Where(e => e.Source == node || e.Target == node);
    }

    /// <summary>
    /// Returns the nodes that are neighbors of the given node, that is, nodes that are directly connected to the
    /// given node via an edge.
    /// </summary>
    /// <param name="node">The node to find neighbors of.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of the neighboring nodes.</returns>
    public IEnumerable<ModelNode> GetNeighbors(ModelNode node) {
      return edges.Where(e => e.Source == node).Select(e => e.Target)
                  .Union(edges.Where(e => e.Target == node).Select(e => e.Source));
    }

    /// <summary>Gets an <see cref="IEnumerable{T}"/> of nodes in the model.</summary>
    public IEnumerable<ModelNode> Nodes {
      get { return nodes; }
    }

    /// <summary>Gets an <see cref="IEnumerable{T}"/> of edges in the model.</summary>
    public IEnumerable<ModelEdge> Edges {
      get { return edges; }
    }
  }
}