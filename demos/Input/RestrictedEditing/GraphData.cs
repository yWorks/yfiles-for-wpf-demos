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
using yWorks.Geometry;

namespace yWorks.Demo.RestrictedEditing
{
  /// <summary>
  ///   The sample graph data loaded from the embedded JSON resource.
  /// </summary>
  public sealed class GraphData
  {
    public List<NodeData> Nodes { get; set; } = [];

    public List<EdgeData> Edges { get; set; } = [];
  }

  /// <summary>
  ///   Data for a sample graph node.
  /// </summary>
  public sealed class NodeData
  {
    public int Id { get; set; }

    public double[] Layout { get; set; } = [];

    public bool IsGroup { get; set; }

    public int? Parent { get; set; }
  }

  /// <summary>
  ///   Data for a sample graph edge.
  /// </summary>
  public sealed class EdgeData
  {
    public int Id { get; set; }

    public int Source { get; set; }

    public int Target { get; set; }

    public List<PointData> Bends { get; set; } = [];
  }

  /// <summary>
  ///   Coordinates of a sample edge bend.
  /// </summary>
  public sealed class PointData
  {
    public double X { get; set; }

    public double Y { get; set; }

    public PointD ToPoint() => new(X, Y);
  }
}
