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
using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Input
{
  /// <summary>
  /// This class is used for orthogonal edges in yEd and allows orthogonally edited edges to move their ports.
  /// </summary>
  /// <remarks>
  /// Also this implementation removes bends inside the bounds of the nodes and relocates the ports to the last
  /// bend inside the node.
  /// </remarks>
  public class CustomOrthogonalEdgeHelper : OrthogonalEdgeHelper
  {
    public override bool ShouldEditOrthogonally(IInputModeContext context, IEdge edge) {
      // arc edges are not edited orthogonally
      return !(edge.Style is ArcEdgeStyle);
    }

    public override bool ShouldMoveEndImplicitly(IInputModeContext context, IEdge edge, bool sourceEnd) {
      // enable moving of source/target of the edge to other ports
      return true;
    }

    public override void CleanUpEdge(IInputModeContext context, IGraph graph, IEdge edge) {
      base.CleanUpEdge(context, graph, edge);
      // now check bends which lie inside the node bounds and remove them...
      var sourceNode = edge.GetSourceNode();
      if (sourceNode != null && graph.EdgesAt(edge.SourcePort).Count == 1) {
        var sourceContainsTest = sourceNode.SafeLookup<IShapeGeometry>();
        while (edge.Bends.Count > 0 && sourceContainsTest.IsInside(edge.Bends[0].Location.ToPointD())) {
          var bendLocation = edge.Bends[0].Location.ToPointD();
          // we try to move to port to the bend location so that the edge shape stays the same
          graph.SetPortLocation(edge.SourcePort, bendLocation);
          if (edge.SourcePort.GetLocation() != bendLocation) {
            break; // does not work - bail out
          }
          graph.Remove(edge.Bends[0]);
        }
      }
      var targetNode = edge.GetTargetNode();
      if (targetNode != null && graph.EdgesAt(edge.TargetPort).Count == 1) {
        var targetContainsTest = targetNode.SafeLookup<IShapeGeometry>();
        while (edge.Bends.Count > 0 && targetContainsTest.IsInside(edge.Bends.Last().Location.ToPointD())) {
          var lastBend = edge.Bends.Last();
          var bendLocation = lastBend.Location.ToPointD();
          // we try to move to port to the bend location so that the edge shape stays the same
          graph.SetPortLocation(edge.TargetPort, bendLocation);
          if (edge.TargetPort.GetLocation() != bendLocation) {
            break; // does not work - bail out
          }
          graph.Remove(lastBend);
        }
      }
    }
  }
}
