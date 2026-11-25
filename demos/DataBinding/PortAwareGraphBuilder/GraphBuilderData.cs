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

namespace yWorks.Demo.DataBinding.PortAwareGraphBuilder
{
  /// <summary>
  ///   Sample business data used by the port-aware graph builder.
  /// </summary>
  internal sealed class GraphBuilderData
  {
    public List<NodeData> Gates { get; set; } = [];

    public List<EdgeData> Connections { get; set; } = [];

    public List<NodeData> UpdateGates { get; set; } = [];

    public List<EdgeData> UpdateConnections { get; set; } = [];
  }

  /// <summary>
  ///   the ID for the node, which must be unique for all nodes in the graph
  /// </summary>
  internal sealed class NodeData
  {
    public string Id { get; init; } = string.Empty;

    /// <summary>
    ///   the node type. Determines appearance and ports
    /// </summary>
    public string Type { get; init; } = string.Empty;
  }

  /// <summary>
  ///   the ID for the edge, which must be unique for all edges in the graph
  /// </summary>
  internal sealed class EdgeData
  {
    public string Id { get; init; } = string.Empty;

    /// <summary>
    ///   the source port ID as 'nodeID;portID'
    /// </summary>
    public string From { get; init; } = string.Empty;

    /// <summary>
    ///   the target port ID as 'nodeID;portID'
    /// </summary>
    public string To { get; init; } = string.Empty;
  }
}
