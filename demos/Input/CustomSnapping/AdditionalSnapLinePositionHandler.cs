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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.CustomSnapping
{
  /// <summary>
  ///   An <see cref="IPositionHandler"/> used to move <see cref="AdditionalSnapLineVisualCreator"/> instances.
  /// </summary>
  internal sealed class AdditionalSnapLinePositionHandler : IPositionHandler
  {
    private readonly AdditionalSnapLineVisualCreator line;
    private readonly GraphControl graphControl;
    private readonly PointD mouseDeltaFromStart;
    private PointD startFrom;

    /// <summary>
    ///   Creates a new handler for the given <paramref name="line"/>.
    /// </summary>
    /// <param name="line">The additional snap line to move.</param>
    /// <param name="mouseLocation">The mouse location at the beginning of a move gesture.</param>
    public AdditionalSnapLinePositionHandler(
        AdditionalSnapLineVisualCreator line,
        PointD mouseLocation,
        GraphControl graphControl) {
      this.line = line;
      this.graphControl = graphControl;
      mouseDeltaFromStart = mouseLocation - line.From;
    }

    /// <summary>
    ///   Returns a view of the location of the item.
    ///   The point describes the current world coordinate of the <see cref="AdditionalSnapLineVisualCreator.From"/>
    ///   property of the moved <see cref="AdditionalSnapLineVisualCreator"/>.
    /// </summary>
    public IPoint Location => line.From;

    /// <summary>
    ///   Called by clients to indicate that the element is going to be dragged.
    ///   This call will be followed by one or more calls to <see cref="HandleMove"/>,
    ///   and a final <see cref="DragFinished"/> or <see cref="CancelDrag"/>.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <remarks>
    ///   The value of <see cref="Location"/> at this point will be used for the original-location argument of
    ///   <see cref="HandleMove"/>, <see cref="CancelDrag"/>, and <see cref="DragFinished"/>.
    /// </remarks>
    /// <seealso cref="IPositionHandler.InitializeDrag"/>
    public void InitializeDrag(IInputModeContext context) {
      startFrom = line.From;
    }

    /// <summary>
    ///   Called by clients to indicate that the element has been dragged and its position
    ///   should be updated.
    ///   This method may be called more than once after an initial <see cref="InitializeDrag"/>.
    ///   The final call will be followed by either one <see cref="DragFinished"/> or one
    ///   <see cref="CancelDrag"/> call.
    /// </summary>
    /// <param name="context">The context providing information about the drag.</param>
    /// <param name="originalLocation">
    ///   The value of <see cref="Location"/> at the time of <see cref="InitializeDrag"/>.
    /// </param>
    /// <param name="newLocation">
    ///   The coordinates in the world coordinate system that the client wants the handle to be at.
    ///   Depending on the implementation, <see cref="Location"/> may or may not be modified to reflect the new value.
    /// </param>
    /// <seealso cref="IPositionHandler.HandleMove"/>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      SetPosition(newLocation.X - mouseDeltaFromStart.X, newLocation.Y - mouseDeltaFromStart.Y);
    }

    /// <summary>
    ///   Called by clients to indicate that the drag gesture has been canceled by the user.
    ///   This method may be called after the initial <see cref="InitializeDrag"/> and zero or more invocations of
    ///   <see cref="HandleMove"/>. Alternatively, <see cref="DragFinished"/> might be called.
    /// </summary>
    /// <param name="context">The context providing information about the drag.</param>
    /// <param name="originalLocation">
    ///   The value of <see cref="Location"/> at the time of <see cref="InitializeDrag"/>.
    /// </param>
    /// <seealso cref="IPositionHandler.CancelDrag"/>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      SetPosition(startFrom.X, startFrom.Y);
    }

    /// <summary>
    ///   Called by clients to indicate that the repositioning has just been finished.
    ///   This method may be called after the initial <see cref="InitializeDrag"/> and zero or more invocations of
    ///   <see cref="HandleMove"/>. Alternatively, <see cref="CancelDrag"/> might be called.
    /// </summary>
    /// <param name="context">The context providing information about the drag.</param>
    /// <param name="originalLocation">
    ///   The value of <see cref="Location"/> at the time of <see cref="InitializeDrag"/>.
    /// </param>
    /// <param name="newLocation">
    ///   The coordinates in the world coordinate system that the client wants the handle to be at.
    ///   This is the same value as delivered in the last invocation of <see cref="HandleMove"/>.
    /// </param>
    /// <seealso cref="IPositionHandler.DragFinished"/>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      SetPosition(newLocation.X - mouseDeltaFromStart.X, newLocation.Y - mouseDeltaFromStart.Y);
    }

    /// <summary>
    ///   Sets the position to the given coordinates.
    ///   The given coordinates are interpreted to be the new position of the
    ///   <see cref="AdditionalSnapLineVisualCreator.From"/> property of the moved
    ///   <see cref="AdditionalSnapLineVisualCreator"/>.
    /// </summary>
    /// <param name="x">The new x-coordinate for the <see cref="AdditionalSnapLineVisualCreator.From"/> property.</param>
    /// <param name="y">The new y-coordinate for the <see cref="AdditionalSnapLineVisualCreator.From"/> property.</param>
    /// <seealso cref="IPositionHandler.Location"/>
    private void SetPosition(double x, double y) {
      // ensure the line stays an orthogonal line by moving its end point in the same way as its start point
      var delta = new PointD(x - line.From.X, y - line.From.Y);
      line.To += delta;

      line.From = new PointD(x, y);
      graphControl.Invalidate();
    }
  }
}
