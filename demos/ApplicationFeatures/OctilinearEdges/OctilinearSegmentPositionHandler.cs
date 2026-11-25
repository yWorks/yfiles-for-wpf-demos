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

using System;
using System.Collections.Generic;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   A position handler for resizing and moving octilinear segments.
  ///   Depending on the <see cref="MoveBendRecognizer"/> and <see cref="ResizeAllRecognizer"/> dragging on the octilinear
  ///   segment
  ///   either moves the bend, or resizes the segment, or resizes all segments similarly in the graph.
  ///   Moving the segment is delegated to an <see cref="OctilinearSegmentHandle"/> that considers the
  ///   <see cref="OrthogonalEdgeEditingContext"/> and implicitly keeps the bends orthogonal to each other.
  /// </summary>
  internal sealed class OctilinearSegmentPositionHandler : IPositionHandler
  {
    private readonly OctilinearSegmentHandle bendHandle;
    private readonly IBend bend;
    private readonly Dictionary<IBend, double> initialCuttingLengths = new();
    private bool isMove = true;
    private bool wasResizeAll;

    /// <summary>
    ///   Whether the bend should be moved.
    /// </summary>
    internal static readonly EventRecognizer MoveBendRecognizer = EventRecognizers.ShiftIsDown;

    /// <summary>
    ///   Whether all segments in the graph should be resized.
    /// </summary>
    private static readonly EventRecognizer ResizeAllRecognizer = EventRecognizers.CtrlIsDown;

    internal OctilinearSegmentPositionHandler(IBend bend) {
      this.bend = bend;

      // moving the segment is handled by the octilinear handle
      var coreHandle = (IHandle) bend.Lookup(typeof(IHandle))!;
      bendHandle = new OctilinearSegmentHandle(bend, coreHandle, true);
    }

    /// <summary>
    ///   Returns the translated location of the bend handle.
    /// </summary>
    public IPoint Location => bendHandle.Location;

    public void InitializeDrag(IInputModeContext context) {
      foreach (var edgeBend in bend.Owner.Bends) {
        initialCuttingLengths[edgeBend] = Utils.GetCuttingLength(edgeBend);
      }

      isMove = MoveBendRecognizer(context.CanvasControl, context.CanvasControl.LastPointerEvent);

      bendHandle.InitializeDrag(context);
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      if (isMove) {
        bendHandle.HandleMove(context, originalLocation, newLocation);
      } else {
        var isResizeAll = ResizeAllRecognizer(context.CanvasControl, context.CanvasControl.LastPointerEvent);
        HandleResizeSegment(originalLocation, newLocation, isResizeAll);
        wasResizeAll = isResizeAll;
      }
    }

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      if (isMove) {
        bendHandle.DragFinished(context, originalLocation, newLocation);
      }
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      if (isMove) {
        bendHandle.CancelDrag(context, originalLocation);
        return;
      }

      var bendsToReset = wasResizeAll ? bend.Owner.Bends : [bend];
      foreach (var edgeBend in bendsToReset) {
        Utils.UpdateCuttingLength(edgeBend, initialCuttingLengths[edgeBend]);
      }
    }

    /// <summary>
    ///   Resizes the segment by adjusting the cutting length of the bend.
    /// </summary>
    /// <param name="originalLocation">The original location of the drag gesture.</param>
    /// <param name="newLocation">The new location resulting from the drag gesture.</param>
    /// <param name="resizeAll">Whether all segments in the graph should be resized.</param>
    private void HandleResizeSegment(PointD originalLocation, PointD newLocation, bool resizeAll) {
      if (!resizeAll && wasResizeAll) {
        // reset all other bends' cutting lengths
        foreach (var edgeBend in bend.Owner.Bends) {
          if (!ReferenceEquals(edgeBend, bend)) {
            Utils.UpdateCuttingLength(edgeBend, initialCuttingLengths[edgeBend]);
          }
        }
      }

      var locationDelta = newLocation - originalLocation;
      var handleCorner = Utils.GetCornerType(bend);
      var deltaL = DeltaCuttingLength(locationDelta, handleCorner);
      var maxCuttingLength = Utils.GetMaxCuttingLength(bend);
      var newCuttingLength = Math.Max(0, Math.Min(initialCuttingLengths[bend] + deltaL, maxCuttingLength));

      if (resizeAll) {
        foreach (var edgeBend in bend.Owner.Bends) {
          Utils.UpdateCuttingLength(edgeBend, Math.Min(newCuttingLength, Utils.GetMaxCuttingLength(edgeBend)));
        }
      } else {
        Utils.UpdateCuttingLength(bend, newCuttingLength);
      }
    }

    /// <summary>
    ///   Calculates the delta for the cutting length depending on the drag vector and the corner of the edge.
    /// </summary>
    /// <param name="locationDelta">The drag vector resulting from the user interaction.</param>
    /// <param name="handleCorner">The corner of the edge where the bend is located.</param>
    private static double DeltaCuttingLength(PointD locationDelta, Corner handleCorner) {
      return handleCorner switch {
          Corner.TopLeft => locationDelta.X + locationDelta.Y,
          Corner.TopRight => -locationDelta.X + locationDelta.Y,
          Corner.BottomLeft => locationDelta.X - locationDelta.Y,
          _ => -locationDelta.X - locationDelta.Y
      };
    }
  }
}
