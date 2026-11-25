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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.InteractiveGraphRestructuring
{
  /// <summary>
  ///   An IPositionHandler that moves the bends of an edge.
  /// </summary>
  internal sealed class EdgePositionHandler : IPositionHandler
  {
    private readonly IEdge edge;
    private readonly List<HandleStructure> handles = [];

    public IPoint Location => PointD.Origin;

    /// <summary>
    ///   Creates a new instance of the <see cref="EdgePositionHandler"/>.
    /// </summary>
    /// <param name="edge">The edge which should be moved</param>
    public EdgePositionHandler(IEdge edge) {
      this.edge = edge;
    }

    /// <summary>
    ///   The edge is about to be dragged.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    public void InitializeDrag(IInputModeContext context) {
      handles.Clear();
      foreach (var bend in edge.Bends) {
        var handle = bend.Lookup<IHandle>();
        if (handle != null) {
          handles.Add(new HandleStructure(handle, handle.Location.ToPointD()));
        }
      }
    }

    /// <summary>
    ///   The edge is dragged.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    /// <param name="originalLocation">The value of the location property at the time of InitializeDrag</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handles to be at</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      MoveHandles(context, originalLocation, newLocation, false);
    }

    /// <summary>
    ///   The edge is canceled being dragged.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    /// <param name="originalLocation">The value of the location property at the time of InitializeDrag</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      foreach (var handle in handles) {
        handle.Handle.CancelDrag(context, handle.OriginalLocation);
      }
    }

    /// <summary>
    ///   The edge is canceled being dragged.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from</param>
    /// <param name="originalLocation">The value of the location property at the time of InitializeDrag</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handles to be at</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      MoveHandles(context, originalLocation, newLocation, true);
    }

    private void MoveHandles(IInputModeContext context, PointD originalLocation, PointD newLocation, bool finished) {
      var delta = newLocation - originalLocation;
      foreach (var handle in handles) {
        var target = handle.OriginalLocation + delta;
        if (finished) {
          handle.Handle.DragFinished(context, handle.OriginalLocation, target);
        } else {
          handle.Handle.HandleMove(context, handle.OriginalLocation, target);
        }
      }
    }

    /// <summary>
    ///   Helper class that represents a handle structure.
    /// </summary>
    private sealed record HandleStructure(IHandle Handle, PointD OriginalLocation);
  }
}
