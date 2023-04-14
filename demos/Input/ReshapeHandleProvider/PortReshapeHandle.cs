/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Graph.Input.ReshapeHandleProvider {

  /// <summary>
  /// An <see cref="IHandle"/> implementation for an <see cref="IPort"/> using a <see cref="NodeStylePortStyleAdapter"/>
  /// that can be used to reshape the <see cref="NodeStylePortStyleAdapter.RenderSize"/>.
  /// </summary>
  public class PortReshapeHandle : IHandle {
    private readonly IInputModeContext context;
    private readonly IPort port;
    private readonly NodeStylePortStyleAdapter adapter;
    private readonly HandlePositions position;

    // the initial RenderSize used to reset the size on Cancel
    private SizeD initialRenderSize;

    /// <summary>
    /// The margins the handle is placed form the port visualization bounds.
    /// </summary>
    /// <remarks>
    /// The margins are applied in view coordinates. Default is <c>4</c>.
    /// </remarks>
    public double Margins { get; set; }
    
    /// <summary>
    /// The minimum size the <see cref="NodeStylePortStyleAdapter.RenderSize"/> may have.
    /// </summary>
    public SizeD MinimumSize { get; set; }

    /// <summary>
    /// Creates a new instance for <paramref name="port"/> and its <paramref name="adapter"/>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="port">The port whose visualization shall be resized.</param>
    /// <param name="adapter">The adapter whose render size shall be changed.</param>
    /// <param name="position">The position of the handle.</param>
    public PortReshapeHandle(IInputModeContext context, IPort port, NodeStylePortStyleAdapter adapter, HandlePositions position) {
      this.context = context;
      this.position = position;
      this.adapter = adapter;
      this.port = port;
      this.Margins = 4;
    }

    /// <summary>
    /// Returns the current location of the handle.
    /// </summary>
    public IPoint Location {
      get { return CalculateLocation(); }
    }

    /// <summary>
    /// Calculates the location of the handle considering the <see cref="GraphExtensions.GetLocation">port location</see>,
    /// <see cref="NodeStylePortStyleAdapter.RenderSize"/> and <see cref="Margins"/>.
    /// </summary>
    private PointD CalculateLocation() {
      var portLocation = port.GetLocation();
      var handleX = portLocation.X;
      var handleY = portLocation.Y;
      var marginsInViewCoordinates = Margins / context.Zoom;
      if (position == HandlePositions.NorthWest || position == HandlePositions.West || position == HandlePositions.SouthWest) {
        handleX -= adapter.RenderSize.Width / 2 + marginsInViewCoordinates;
      } else if (position == HandlePositions.NorthEast || position == HandlePositions.East || position == HandlePositions.SouthEast) {
        handleX += adapter.RenderSize.Width / 2 + marginsInViewCoordinates;
      }
      if (position == HandlePositions.NorthWest || position == HandlePositions.North || position == HandlePositions.NorthEast) {
        handleY -= adapter.RenderSize.Height / 2 + marginsInViewCoordinates;
      } else if (position == HandlePositions.SouthWest || position == HandlePositions.South || position == HandlePositions.SouthEast) {
        handleY += adapter.RenderSize.Height / 2 + marginsInViewCoordinates;
      }
      return new PointD(handleX, handleY);
    }

    /// <summary>
    /// Stores the initial <see cref="NodeStylePortStyleAdapter.RenderSize"/>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    public void InitializeDrag(IInputModeContext context) {
      initialRenderSize = adapter.RenderSize;
    }

    /// <summary>
    /// Calculates and applies the new <see cref="NodeStylePortStyleAdapter.RenderSize"/>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="originalLocation">The value of the <see cref="Location"/> property at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // calculate the size delta implied by the originalLocation and newLocation
      var delta = CalculateDelta(originalLocation, newLocation);
      // calculate and apply the new render size
      adapter.RenderSize = CalculateNewSize(delta);
    }

    private SizeD CalculateDelta(PointD originalLocation, PointD newLocation) {
      // calculate the delta the mouse has been moved since InitializeDrag
      var mouseDelta = newLocation - originalLocation;
      // depending on the handle position this mouse delta shall increase or decrease the render size
      switch (position) {
        case HandlePositions.NorthWest:
          return new SizeD(-2 * mouseDelta.X, -2 * mouseDelta.Y);
        case HandlePositions.North:
          return new SizeD(0, -2 * mouseDelta.Y);
        case HandlePositions.NorthEast:
          return new SizeD(2 * mouseDelta.X, -2 * mouseDelta.Y);
        case HandlePositions.West:
          return new SizeD(-2 * mouseDelta.X, 0);
        case HandlePositions.East:
          return new SizeD(2 * mouseDelta.X, 0);
        case HandlePositions.SouthWest:
          return new SizeD(-2 * mouseDelta.X, 2 * mouseDelta.Y);
        case HandlePositions.South:
          return new SizeD(0, 2 * mouseDelta.Y);
        case HandlePositions.SouthEast:
          return new SizeD(2 * mouseDelta.X, 2 * mouseDelta.Y);
      }
      return SizeD.Empty;
    }

    private SizeD CalculateNewSize(SizeD delta) {
      var newWidth = Math.Max(MinimumSize.Width, initialRenderSize.Width + delta.Width);
      var newHeight = Math.Max(MinimumSize.Height, initialRenderSize.Height + delta.Height);
      return new SizeD(newWidth, newHeight);
    }

    /// <summary>
    /// Resets <see cref="NodeStylePortStyleAdapter.RenderSize"/> to its initial value.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="originalLocation">The value of the <see cref="Location"/> property at the time of <see cref="InitializeDrag"/>.</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      adapter.RenderSize = initialRenderSize;
    }

    /// <summary>
    /// Calculates and applies the final <see cref="NodeStylePortStyleAdapter.RenderSize"/>.
    /// </summary>
    /// <param name="context">The context of the reshape gesture.</param>
    /// <param name="originalLocation">The value of the <see cref="Location"/> property at the time of <see cref="InitializeDrag"/>.</param>
    /// <param name="newLocation">The coordinates in the world coordinate system that the client wants the handle to be at.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = CalculateDelta(originalLocation, newLocation);
      adapter.RenderSize = CalculateNewSize(delta);
    }

    /// <summary>
    /// Returns <see cref="HandleTypes.Resize"/>.
    /// </summary>
    public HandleTypes Type {
      get { return HandleTypes.Resize; }
    }

    /// <summary>
    /// Returns a resize cursor matching the handle position.
    /// </summary>
    public Cursor Cursor {
      get {
        switch (position) {
          case HandlePositions.South:
          case HandlePositions.North:
            return Cursors.SizeNS;
          case HandlePositions.East:
          case HandlePositions.West:
            return Cursors.SizeWE;
          case HandlePositions.NorthWest:
          case HandlePositions.SouthEast:
            return Cursors.SizeNWSE;
          case HandlePositions.NorthEast:
          case HandlePositions.SouthWest:
            return Cursors.SizeNESW;
        }
        return Cursors.None;
      }
    }

    /// <summary>
    /// Ignore clicking the handle.
    /// </summary>
    /// <param name="eventArgs">Arguments describing the click.</param>
    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }
  }
}
