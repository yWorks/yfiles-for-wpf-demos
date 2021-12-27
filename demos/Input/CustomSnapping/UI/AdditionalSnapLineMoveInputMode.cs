/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace Demo.yFiles.Graph.Input.CustomSnapping.UI
{

  /// <summary>
  /// This input mode allows moving <see cref="AdditionalSnapLineVisualCreator"/>s using a drag gesture.
  /// </summary>
  public class AdditionalSnapLineMoveInputMode : MoveInputMode
  {
    private IPositionHandler handler;
    private readonly CustomSnappingWindow window;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="window">The <see cref="CustomSnappingWindow"/> the 
    /// <see cref="CustomSnappingWindow.AdditionalSnapLineVisualCreators"/> can be received from.</param>
    public AdditionalSnapLineMoveInputMode(CustomSnappingWindow window) {
      this.window = window;
      PositionHandler = null;
      HitTestable = HitTestables.Create(IsValidHit);
    }

    // Returns true if an AdditionalSnapLine can be found in a close surrounding of the given location.
    private bool IsValidHit(IInputModeContext context, PointD location) {
      AdditionalSnapLineVisualCreator line = TryGetAdditionalSnapLineAt(location);
      if (line != null) {
        handler = new AdditionalSnapLinePositionHandler(line, location);
        return true;
      }
      handler = null;
      return false;
    }

    // Returns the first AdditionalSnapLine found in a close surrounding of the given location
    // or null if none can be found.
    private AdditionalSnapLineVisualCreator TryGetAdditionalSnapLineAt(PointD location) {
      RectD surrounding = new RectD(location - new PointD(3, 3), new SizeD(6, 6));

      return window.AdditionalSnapLineVisualCreators.FirstOrDefault(line => surrounding.IntersectsLine(line.From, line.To));
    }

    /// <summary>
    /// Sets the <see cref="MoveInputMode.PositionHandler"/> property.
    /// </summary>
    /// <param name="inputModeEventArgs"></param>
    protected override void OnDragStarting(InputModeEventArgs inputModeEventArgs) {
      PositionHandler = handler;
      base.OnDragStarting(inputModeEventArgs);
    }

    /// <summary>
    /// Clears the <see cref="MoveInputMode.PositionHandler"/> property.
    /// </summary>
    /// <param name="inputModeEventArgs"></param>
    protected override void OnDragCanceled(InputModeEventArgs inputModeEventArgs) {
      base.OnDragCanceled(inputModeEventArgs);
      PositionHandler = null;
    }

    /// <summary>
    /// Clears the <see cref="MoveInputMode.PositionHandler"/> property.
    /// </summary>
    protected override void OnDragFinished(InputModeEventArgs inputModeEventArgs) {
      base.OnDragFinished(inputModeEventArgs);
      PositionHandler = null;
    }

  }

  /// <summary>
  /// An <see cref="IPositionHandler"/> used to move <see cref="AdditionalSnapLineVisualCreator"/> instances.
  /// </summary>
  internal class AdditionalSnapLinePositionHandler : IPositionHandler
  {
    private readonly AdditionalSnapLineVisualCreator line;
    private PointD startFrom;
    private readonly PointD mouseDeltaFromStart;

    /// <summary>
    /// Creates a new handler for the given <paramref name="line">AdditionalSnapLine</paramref>.
    /// </summary>
    /// <param name="line">The additional snap line to move.</param>
    /// <param name="mouseLocation">The mouse location at the beginning of a move gesture.</param>
    public AdditionalSnapLinePositionHandler(AdditionalSnapLineVisualCreator line, PointD mouseLocation) {
      this.line = line;
      mouseDeltaFromStart = mouseLocation - line.From;
    }

    /// <summary>
    /// Returns a view of the location of the item.
    /// </summary>
    /// <remarks>
    /// The point describes the current world coordinate of the <see cref="AdditionalSnapLineVisualCreator.From"/> property
    /// of the moved <see cref="AdditionalSnapLineVisualCreator"/>.
    /// </remarks>
    public IPoint Location {
      get { return line.From; }
    }

    /// <summary>
    /// Called by clients to indicate that the element is going to be dragged.
    /// </summary>
    /// <remarks>
    /// This call will be followed by one or more calls to <see cref="IDragHandler.HandleMove"/>,
    /// and a final <see cref="IDragHandler.DragFinished"/> or <see cref="IDragHandler.CancelDrag"/>.
    /// </remarks>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    public void InitializeDrag(IInputModeContext context) {
      startFrom = line.From;
    }

    /// <summary>
    /// Called by clients to indicate that the element has been dragged and its position
    /// should be updated.
    /// </summary>
    /// <remarks>
    /// This method may be called more than once after an initial <see cref="IDragHandler.InitializeDrag"/>
    /// and will the final call will be followed by either one 
    /// <see cref="IDragHandler.DragFinished"/> or one <see cref="IDragHandler.CancelDrag"/> call.
    /// </remarks>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The value of the <see cref="Location"/> property at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.
    ///   Depending on the implementation the <see cref="Location"/> may or may not be modified to reflect the new value.
    /// </param>
    /// <returns>Whether the move had any visual effect. This is a hint to the engine to optimize invalidation.</returns>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      SetPosition(newLocation - mouseDeltaFromStart);
    }

    /// <summary>
    /// Called by clients to indicate that the dragging has been canceled by the user.
    /// </summary>
    /// <remarks>
    /// This method may be called after the initial <see cref="InitializeDrag"/> and zero or
    /// more invocations of <see cref="HandleMove"/>.
    /// Alternatively to this method the <see cref="DragFinished"/> method might be called.
    /// </remarks>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The value of the coordinate of the <see cref="Location"/> property at the time of <see cref="InitializeDrag"/>.</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      SetPosition(startFrom);
    }

    /// <summary>
    /// Called by clients to indicate that the repositioning has just been finished.
    /// </summary>
    /// <remarks>
    /// This method may be called after the initial <see cref="InitializeDrag"/> and zero or
    /// more invocations of <see cref="IDragHandler.HandleMove"/>.
    /// Alternatively to this method the <see cref="IDragHandler.CancelDrag"/> method might be called.
    /// </remarks>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.
    /// This is the same value as delivered in the last invocation of <see cref="HandleMove"/>
    /// </param>
    /// <param name="originalLocation">The value of the <see cref="Location"/> property at the time of <see cref="InitializeDrag"/>.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      SetPosition(newLocation - mouseDeltaFromStart);
    }

    /// <summary>
    /// Called by clients to set the position to the given coordinates.
    /// </summary>
    /// <remarks>
    /// The given position are interpreted to be the new position of the <see cref="AdditionalSnapLineVisualCreator.From"/> property
    /// of the moved <see cref="AdditionalSnapLineVisualCreator"/>.
    /// </remarks>
    /// <param name="location">The new location for the <see cref="AdditionalSnapLineVisualCreator.From"/> property.</param>
    /// <seealso cref="IDragHandler.Location"/>
    public void SetPosition(PointD location) {
      PointD delta = location - line.From;
      line.From += delta;
      line.To += delta;
    }
  }
}
