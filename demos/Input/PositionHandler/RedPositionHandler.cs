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

namespace yWorks.Demo.PositionHandler
{
  /// <summary>
  ///   A position handler that prevents node movements. This implementation is
  ///   very simple since most methods do nothing at all.
  /// </summary>
  public sealed class RedPositionHandler : IPositionHandler
  {
    /// <summary>Returns the location of the item.</summary>
    public IPoint Location => PointD.Origin;

    /// <summary>
    ///   Stores the initial location of the movement for reference, and calls the base method.
    /// </summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    public void InitializeDrag(IInputModeContext context) { }

    /// <summary>Prevents node movements.</summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The location at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The new location of the handler.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) { }

    /// <summary>Called when dragging has been canceled by the user.</summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The location at the time of <see cref="InitializeDrag"/>.</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) { }

    /// <summary>Called when dragging has finished.</summary>
    /// <param name="context">The context to retrieve information about the drag from.</param>
    /// <param name="originalLocation">The location at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The new location of the handler.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) { }
  }
}
