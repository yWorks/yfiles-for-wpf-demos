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
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.PositionHandler
{
  /// <summary>
  ///   A position handler that constrains the movement of a node to one axis
  ///   (for each gesture) and delegates for other aspects to another (the
  ///   original) handler.
  ///   Note that the simpler solution for this use case is subclassing
  ///   <see cref="ConstrainedPositionHandler"/>, however the interface is
  ///   completely implemented for illustration purposes, here.
  /// </summary>
  public sealed class GreenPositionHandler : IPositionHandler
  {
    private readonly IPositionHandler handler;
    private PointD lastLocation;

    /// <summary>
    ///   Creates a new instance of <see cref="GreenPositionHandler"/>.
    /// </summary>
    /// <param name="handler">The default handler.</param>
    public GreenPositionHandler(IPositionHandler handler) {
      this.handler = handler;
    }

    /// <summary>Returns the location of the item.</summary>
    public IPoint Location => handler.Location;

    /// <summary>
    ///   Stores the initial location of the movement for reference and calls the base method.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    public void InitializeDrag(IInputModeContext context) {
      handler.InitializeDrag(context);
      lastLocation = handler.Location.ToPointD();
    }

    /// <summary>
    ///   Constrains the movement to one axis. This is done by calculating the
    ///   constrained location for the given new location, and invoking the
    ///   original handler with the constrained location.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The location at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The new location of the handler.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // The larger difference in coordinates specifies whether this is
      // a horizontal or vertical movement.
      var delta = newLocation - originalLocation;
      var isHorizontalMovement = Math.Abs(delta.X) > Math.Abs(delta.Y);

      var updatedNewLocation = isHorizontalMovement
          ? new PointD(newLocation.X, originalLocation.Y)
          : new PointD(originalLocation.X, newLocation.Y);

      if (updatedNewLocation.Equals(lastLocation)) {
        return;
      }

      handler.HandleMove(context, originalLocation, updatedNewLocation);
      lastLocation = updatedNewLocation;
    }

    /// <summary>
    ///   Called when dragging has been canceled by the user.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The location at the time of <see cref="InitializeDrag"/>.</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      handler.CancelDrag(context, originalLocation);
    }

    /// <summary>
    ///   Called when dragging has finished.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The location at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The last location supplied to <see cref="HandleMove"/>.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      handler.DragFinished(context, originalLocation, lastLocation);
    }
  }
}
