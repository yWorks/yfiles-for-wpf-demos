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

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.OrthogonalEdgeEditingCustomization
{
  /// <summary>
  ///   Creates one new bend if the first or last segment of an edge is moved.
  ///   If the old first (last) segment was a horizontal segment, the new first (last) segment will be
  ///   a vertical segment and vice versa.
  ///   The new first (last) segment may lie completely inside the edge's source (target) node.
  /// </summary>
  public sealed class YellowOrthogonalEdgeHelper(IEdge edge) : OrthogonalEdgeHelper(edge)
  {
    /// <summary>
    ///   Stores if a new bend has been added when moving a segment of an orthogonal edge.
    /// </summary>
    private BendAddedState bendAddedState = BendAddedState.None;

    /// <summary>
    ///   Prevents edge ports from being moved.
    ///   As a side effect, this method adds a new bend at the source or target end of the edge.
    /// </summary>
    /// <param name="inputModeContext">The input mode context in which the segment is edited.</param>
    /// <param name="sourceEnd">
    ///   <see langword="true"/> if the source end of the edge is queried and <see langword="false"/>
    ///   otherwise.
    /// </param>
    public override bool ShouldMoveEndImplicitly(IInputModeContext inputModeContext, bool sourceEnd) {
      if (sourceEnd) {
        bendAddedState = BendAddedState.AtSource;
        inputModeContext.GetGraph()!.AddBend(Edge, Edge.SourcePort.GetLocation(), 0);
      } else {
        bendAddedState = BendAddedState.AtTarget;
        inputModeContext.GetGraph()!.AddBend(Edge, Edge.TargetPort.GetLocation());
      }

      return false;
    }

    /// <see cref="OrthogonalEdgeHelper.CleanUpEdge"/>
    /// <see cref="IOrthogonalEdgeHelper.CleanUpEdge"/>
    public override void CleanUpEdge(IInputModeContext inputModeContext, IGraph graph) {
      CleanUpEdgeImpl(graph);
      base.CleanUpEdge(inputModeContext, graph);
    }

    private void CleanUpEdgeImpl(IGraph graph) {
      var state = bendAddedState;
      bendAddedState = BendAddedState.None;

      if (state == BendAddedState.None || Edge.Bends.Count < 1) {
        return;
      }

      CleanUpEdgeEnd(graph, Edge, state);
    }

    /// <summary>
    ///   Removes unnecessary bends inside the source or target node of the given edge, if
    ///   <see cref="ShouldMoveEndImplicitly"/> added a new bend at that end of the edge in the first place.
    /// </summary>
    /// <summary>
    ///   Removes unnecessary bends inside the source or target node of the given edge, if
    ///   <see cref="ShouldMoveEndImplicitly"/> added a new bend at that end of the edge in the first place.
    /// </summary>
    /// <param name="graph">The graph to use for modifying the edge instance.</param>
    /// <param name="edge">The edge whose path needs to be cleaned up.</param>
    /// <param name="state">Specifies which end of the given edge has to be cleaned up.</param>
    private static void CleanUpEdgeEnd(IGraph graph, IEdge edge, BendAddedState state) {
      var atSource = state == BendAddedState.AtSource;

      var port = atSource ? edge.SourcePort : edge.TargetPort;
      var geometry = port.Owner.Lookup<IShapeGeometry>()!;

      // remove all bend inside the respective node
      var bends = edge.Bends;
      while (bends.Count > 0) {
        var bend = atSource ? bends[0] : bends[bends.Count - 1];
        if (geometry.IsInside(bend.Location.ToPointD())) {
          graph.Remove(bend);
        } else {
          break;
        }
      }

      var portLocation = port.GetLocation();
      var portX = portLocation.X;
      var portY = portLocation.Y;

      var otherLocation = bends.Count > 0
          ? (atSource ? bends[0] : bends[bends.Count - 1]).Location.ToPointD()
          : (atSource ? edge.TargetPort : edge.SourcePort).GetLocation();
      var otherX = otherLocation.X;
      var otherY = otherLocation.Y;

      // ensure the first (last) two segments are orthogonal segments
      if (portX != otherX && portY != otherY) {
        var candidate = new PointD(otherX, portY);
        if (geometry.IsInside(candidate)) {
          graph.AddBend(edge, candidate, state == BendAddedState.AtSource ? 0 : -1);
        } else {
          graph.AddBend(edge, new PointD(portX, otherY), state == BendAddedState.AtSource ? 0 : -1);
        }
      }
    }

    private enum BendAddedState
    {
      None,
      AtTarget,
      AtSource
    }
  }
}
