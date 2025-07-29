/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Graph.RelocateSubtree
{
  internal sealed class EdgePositionHandler: IPositionHandler
  {
    private readonly IEdge edge;
    private List<(IHandle Handle, PointD OriginalLocation)> handles;

    public EdgePositionHandler(IEdge edge) {
      this.edge = edge;
    }

    public IPoint Location { get; } = new MutablePoint();

    public void InitializeDrag(IInputModeContext context) {
      handles = new List<(IHandle, PointD)>();
      foreach (var bend in edge.Bends) {
        var handle = bend.Lookup<IHandle>();
        if (handle != null) {
          handles.Add((handle, handle.Location.ToPointD()));
        }
      }
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = newLocation - originalLocation;
      foreach (var tuple in handles) {
        tuple.Handle.HandleMove(context, tuple.OriginalLocation, tuple.OriginalLocation + delta);
      }
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      foreach (var tuple in handles) {
        tuple.Handle.CancelDrag(context, tuple.OriginalLocation);
      }
    }

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = newLocation - originalLocation;
      foreach (var tuple in handles) {
        tuple.Handle.DragFinished(context, tuple.OriginalLocation, tuple.OriginalLocation + delta);
      }
    }
  }
}
