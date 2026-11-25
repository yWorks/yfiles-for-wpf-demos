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

using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   The custom handle that sits in the middle of an octilinear segment. It wraps the original handle
  ///   of the bend and delegates the operations with an offset to the original handle.
  /// </summary>
  internal sealed class OctilinearSegmentHandle : IHandle
  {
    private readonly IHandle coreHandle;
    private readonly IBend bend;
    private readonly bool draggedOnSegment;
    private PointD initialBendLocation;
    private PointD initialHandleOffset;
    private PointD cursorHandleOffset;
    private bool isMultiSelection;

    /// <summary>
    ///   Initializes a new instance of the <see cref="OctilinearSegmentHandle"/> class.
    /// </summary>
    /// <param name="bend">The bend that is being manipulated.</param>
    /// <param name="coreHandle">The original handle being decorated.</param>
    /// <param name="draggedOnSegment">
    ///   Whether this handle is controlled by the
    ///   <see cref="OctilinearSegmentPositionHandler"/> and responsible for dragging on the segment rather
    ///   than the actual handle. This is relevant for the degeneration compensation.
    /// </param>
    internal OctilinearSegmentHandle(IBend bend, IHandle coreHandle, bool draggedOnSegment) {
      this.bend = bend;
      this.coreHandle = coreHandle;
      this.draggedOnSegment = draggedOnSegment;
    }

    public IPoint Location => coreHandle.Location.ToPointD() + Utils.GetHandleOffset(bend);

    public object? Tag => coreHandle.Tag;

    public HandleType Type => coreHandle.Type;

    public Cursor Cursor => Cursors.SizeAll;

    public void HandleClick(ClickEventArgs eventArgs) {
      coreHandle.HandleClick(eventArgs);
    }

    /// <summary>
    ///   Captures the initial state of the bend, the offset of the handle to the bend, and the offset of
    ///   the cursor to the handle before a drag operation starts.
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      initialBendLocation = Utils.GetLocation(bend);
      initialHandleOffset = Utils.GetHandleOffset(bend);
      var graphControl = (GraphControl) context.CanvasControl;
      // if the handle is not dragged via the segment, it is directly moved so use its center location
      var grabLocation = draggedOnSegment ? graphControl.LastPointerEvent.Location : Location.ToPointD();
      cursorHandleOffset = grabLocation - initialBendLocation;
      isMultiSelection = graphControl.Selection.Bends.Count > 1;

      coreHandle.InitializeDrag(context);
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var locationDelta = newLocation - originalLocation;
      HandleBendMove(context, newLocation, locationDelta);
    }

    /// <summary>
    ///   Performs the move of the bend by calculating its new location and delegating to the core handle.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="newLocation">The new location of the cursor.</param>
    /// <param name="locationDelta">The delta the cursor has moved since <see cref="InitializeDrag"/>.</param>
    private void HandleBendMove(IInputModeContext context, PointD newLocation, PointD locationDelta) {
      if (initialHandleOffset.X == 0 && initialHandleOffset.Y == 0) {
        // if there is no offset between the handle and its bend, this means that the cutting length
        // is 0 and the move can simply be delegated to the core handle
        coreHandle.HandleMove(context, initialBendLocation, newLocation);
        return;
      }

      var currentHandleOffset = Utils.GetHandleOffset(bend);
      // If an octilinear segment is forced to shrink because it hits an obstacle (e.g., a node or
      // another octilinear segment) during a bend move, the bend must be moved closer to the cursor
      // than the actual mouse delta to keep the handle attached to the cursor. This compensation
      // is not supported with multi-selection.
      var degenerationCompensation = isMultiSelection
          ? new PointD(0, 0)
          : new PointD(cursorHandleOffset.X * (1 - currentHandleOffset.X / initialHandleOffset.X),
              cursorHandleOffset.Y * (1 - currentHandleOffset.Y / initialHandleOffset.Y));
      var adjustedLocation = initialBendLocation + locationDelta + degenerationCompensation;
      coreHandle.HandleMove(context, initialBendLocation, adjustedLocation);
    }

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      coreHandle.DragFinished(context, initialBendLocation, newLocation);
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      coreHandle.CancelDrag(context, initialBendLocation);
    }
  }
}
