/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Input.ReshapeHandleProvider {

  /// <summary>
  /// An <see cref="IReshapeHandleProvider"/> implementation for <see cref="IPort"/>s using a <see cref="NodeStylePortStyleAdapter"/>.
  /// </summary>
  /// <remarks>
  /// The provided <see cref="PortReshapeHandle"/> modify the <see cref="NodeStylePortStyleAdapter.RenderSize"/> of the
  /// port style.
  /// </remarks>
  public class PortReshapeHandlerProvider : IReshapeHandleProvider {
    private readonly IPort port;
    private readonly NodeStylePortStyleAdapter adapter;

    /// <summary>
    /// The minimum size the <see cref="NodeStylePortStyleAdapter.RenderSize"/> may have.
    /// </summary>
    public SizeD MinimumSize { get; set; }
   
    /// <summary>
    /// Creates a new instance for <paramref name="port"/> and its <paramref name="adapter"/>.
    /// </summary>
    /// <param name="port">The port whose visualization shall be resized.</param>
    /// <param name="adapter">The adapter whose render size shall be changed.</param>
    public PortReshapeHandlerProvider(IPort port, NodeStylePortStyleAdapter adapter) {
      this.port = port;
      this.adapter = adapter;
    }

    /// <summary>
    /// Returns <see cref="HandlePositions.Corners"/> or <see cref="HandlePositions.Border"/> as available handle
    /// positions depending on the modifier state of <c>Ctrl</c>.
    /// </summary>
    /// <param name="context">The context the handles are created in.</param>
    /// <returns><see cref="HandlePositions.Corners"/> or <see cref="HandlePositions.Border"/></returns>
    public HandlePositions GetAvailableHandles(IInputModeContext context) {
      var ctrlPressed = KeyEventRecognizers.CtrlPressed(this, context.CanvasControl.LastInputEvent);
      // when Ctrl is pressed, all border positions are returned, otherwise only the corner positions
      return ctrlPressed ? HandlePositions.Border : HandlePositions.Corners;
    }

    /// <summary>
    /// Returns a <see cref="PortReshapeHandle"/> for the port at the <paramref name="position"/> and
    /// sets its <see cref="PortReshapeHandle.MinimumSize"/> to <see cref="MinimumSize"/>.
    /// </summary>
    /// <param name="context">The context the handles are created in.</param>
    /// <param name="position">The position the handle shall be created for.</param>
    /// <returns>A <see cref="PortReshapeHandle"/> for the port at the <paramref name="position"/>.</returns>
    public IHandle GetHandle(IInputModeContext context, HandlePositions position) {
      return new PortReshapeHandle(context, port, adapter, position) { MinimumSize = MinimumSize };
    }
  }
}
