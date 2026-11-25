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
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ReshapeHandleProvider
{
  internal sealed class PortReshapeHandle : IHandle
  {
    private readonly SizeD minimumSize;
    private readonly HandlePositions position;
    private readonly ShapePortStyle portStyle;
    private readonly IPort port;
    private readonly IInputModeContext context;

    /// <summary>
    ///   The margins the handle is placed from the port visualization bounds.
    ///   The margins are applied in view coordinates. Default is <c>4</c>.
    /// </summary>
    public double Margins { get; set; }

    /// <summary>
    ///   The initial render size (i.e. the size of the port visualization). Used to reset the visualization size on cancel.
    /// </summary>
    private SizeD initialRenderSize = new(0, 0);

    /// <summary>
    ///   Creates a new instance for port and its style.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="port">The port whose visualization shall be resized.</param>
    /// <param name="portStyle">The style whose render size shall be changed.</param>
    /// <param name="position">The position of the handle.</param>
    /// <param name="minimumSize">The minimum render size for the given port.</param>
    public PortReshapeHandle(
        IInputModeContext context,
        IPort port,
        ShapePortStyle portStyle,
        HandlePositions position,
        SizeD minimumSize) {
      this.context = context;
      this.port = port;
      this.portStyle = portStyle;
      this.position = position;
      this.minimumSize = minimumSize;
      Margins = 4;
    }

    /// <summary>
    ///   Returns the current location of the handle.
    /// </summary>
    public IPoint Location => CalculateLocation();

    /// <summary>
    ///   Calculates the location of the handle considering the port location,
    ///   <see cref="ShapePortStyle.RenderSize" render size/> and <see cref="Margins"/>.
    /// </summary>
    private PointD CalculateLocation() {
      var portLocation = port.GetLocation();
      var handleX = portLocation.X;
      var handleY = portLocation.Y;
      var marginsInViewCoordinates = Margins / context.Zoom;
      if (position is HandlePositions.TopLeft or HandlePositions.Left or HandlePositions.BottomLeft) {
        handleX -= portStyle.RenderSize.Width / 2 + marginsInViewCoordinates;
      } else if (position is HandlePositions.TopRight or HandlePositions.Right or HandlePositions.BottomRight) {
        handleX += portStyle.RenderSize.Width / 2 + marginsInViewCoordinates;
      }
      if (position is HandlePositions.TopLeft or HandlePositions.Top or HandlePositions.TopRight) {
        handleY -= portStyle.RenderSize.Height / 2 + marginsInViewCoordinates;
      } else if (position is HandlePositions.BottomLeft or HandlePositions.Bottom or HandlePositions.BottomRight) {
        handleY += portStyle.RenderSize.Height / 2 + marginsInViewCoordinates;
      }
      return new PointD(handleX, handleY);
    }

    /// <summary>
    ///   Stores the initial <see cref="ShapePortStyle.RenderSize" render size/>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    public void InitializeDrag(IInputModeContext context) {
      initialRenderSize = portStyle.RenderSize;
    }

    /// <summary>
    ///   Calculates and applies the new <see cref="ShapePortStyle.RenderSize" render size/>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="originalLocation">
    ///   The value of the <see cref="Location"/> property at the time of
    ///   <see cref="InitializeDrag"/>.
    /// </param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // calculate the size delta implied by the originalLocation and newLocation
      var delta = CalculateDelta(originalLocation, newLocation);
      // calculate and apply the new render size
      portStyle.RenderSize = CalculateNewSize(delta);
    }

    /// <summary>
    ///   Calculates the size adjustment for the port's render size from the previous and current
    ///   mouse location.
    /// </summary>
    /// <param name="originalLocation">
    ///   The value of the <see cref="Location"/> property at the time of
    ///   <see cref="InitializeDrag"/>.
    /// </param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.</param>
    private SizeD CalculateDelta(PointD originalLocation, PointD newLocation) {
      // calculate the delta the mouse has been moved since initializeDrag
      var mouseDelta = newLocation - originalLocation;
      // depending on the handle position this mouse delta shall increase or decrease the render size
      return position switch {
          HandlePositions.TopLeft => new SizeD(-2 * mouseDelta.X, -2 * mouseDelta.Y),
          HandlePositions.Top => new SizeD(0, -2 * mouseDelta.Y),
          HandlePositions.TopRight => new SizeD(2 * mouseDelta.X, -2 * mouseDelta.Y),
          HandlePositions.Left => new SizeD(-2 * mouseDelta.X, 0),
          HandlePositions.Right => new SizeD(2 * mouseDelta.X, 0),
          HandlePositions.BottomLeft => new SizeD(-2 * mouseDelta.X, 2 * mouseDelta.Y),
          HandlePositions.Bottom => new SizeD(0, 2 * mouseDelta.Y),
          HandlePositions.BottomRight => new SizeD(2 * mouseDelta.X, 2 * mouseDelta.Y),
          _ => new SizeD(0, 0)
      };
    }

    /// <summary>
    ///   Calculates the new render size for the port from the current render size and the given
    ///   size adjustment while respecting the <see cref="minimumSize" minimum size/>.
    /// </summary>
    /// <param name="delta">The size adjustment for the port's render size.</param>
    private SizeD CalculateNewSize(SizeD delta) {
      var newWidth = Math.Max(minimumSize.Width, initialRenderSize.Width + delta.Width);
      var newHeight = Math.Max(minimumSize.Height, initialRenderSize.Height + delta.Height);
      return new SizeD(newWidth, newHeight);
    }

    /// <summary>
    ///   Resets the <see cref="ShapePortStyle.RenderSize">render size</see> to its initial value.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="originalLocation">
    ///   The value of the <see cref="Location"/> property at the time of
    ///   <see cref="InitializeDrag"/>.
    /// </param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      portStyle.RenderSize = initialRenderSize;
    }

    /// <summary>
    ///   Calculates and applies the final <see cref="ShapePortStyle.RenderSize">render size</see>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="originalLocation">
    ///   The value of the <see cref="Location"/> property at the time of
    ///   <see cref="InitializeDrag"/>.
    /// </param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = CalculateDelta(originalLocation, newLocation);
      portStyle.RenderSize = CalculateNewSize(delta);
    }

    /// <summary>
    ///   Returns <see cref="HandleType.Resize"/>.
    /// </summary>
    public HandleType Type => HandleType.Resize;

    /// <summary>
    ///   Gets an optional tag object associated with the handle.
    /// </summary>
    public object? Tag => null;

    /// <summary>
    ///   Returns a resize cursor matching the handle position.
    /// </summary>
    public Cursor Cursor =>
        position switch {
            HandlePositions.Bottom or HandlePositions.Top => Cursors.SizeNS,
            HandlePositions.Right or HandlePositions.Left => Cursors.SizeWE,
            HandlePositions.TopLeft or HandlePositions.BottomRight => Cursors.SizeNWSE,
            HandlePositions.TopRight or HandlePositions.BottomLeft => Cursors.SizeNESW,
            _ => Cursors.Arrow
        };

    /// <summary>
    ///   This implementation does nothing special when clicked.
    /// </summary>
    public void HandleClick(ClickEventArgs eventArgs) { }
  }
}
