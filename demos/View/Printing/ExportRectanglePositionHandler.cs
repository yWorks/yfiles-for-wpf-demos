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

using yWorks.Controls.Input;
using yWorks.Geometry;

namespace Demo.yFiles.Printing
{
  /// <summary>
  /// Simple implementation of an <see cref="IPositionHandler"/> that moves an 
  /// <see cref="IMutablePoint"/> by the dragged distance.
  /// </summary>
  public class ExportRectanglePositionHandler : IPositionHandler
  {

    private readonly IMutablePoint position;
    private PointD startPosition;

    /// <summary>
    /// Creates a position handler that delegates to a mutable position.
    /// </summary>
    /// <param name="position">The position that will be read and changed.</param>
    public ExportRectanglePositionHandler(IMutablePoint position) {
      this.position = position;
    }

    public IPoint Location {
      get { return position; }
    }

    /// <summary>
    /// Stores the initial position of the <see cref="IMutablePoint"/>.
    /// </summary>
    public virtual void InitializeDrag(IInputModeContext context) {
      startPosition = position.ToPointD();
    }

    /// <summary>
    /// Moves the <see cref="IMutablePoint"/> away from the start position by the difference
    /// beteween <paramref name="newLocation"/> and <paramref name="originalLocation"/>.
    /// </summary>
    public virtual void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var currentPosition = startPosition + (newLocation - originalLocation);
      if (position.X != currentPosition.X || position.Y != currentPosition.Y) {
        position.X = currentPosition.X;
        position.Y = currentPosition.Y;
      }
    }

    /// <summary>
    /// Moves the <see cref="IMutablePoint"/> back to the start position.
    /// </summary>
    public virtual void CancelDrag(IInputModeContext context, PointD originalLocation) {
      if (position.X != startPosition.X || position.Y != startPosition.Y) {
        position.X = startPosition.X;
        position.Y = startPosition.Y;
      }
    }

    public virtual void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
    }
  }
}
