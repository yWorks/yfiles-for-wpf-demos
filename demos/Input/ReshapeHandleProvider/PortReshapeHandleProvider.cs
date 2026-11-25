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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ReshapeHandleProvider
{
  /// <summary>
  ///   An <see cref="IReshapeHandleProvider"/> implementation for <see cref="IPort"/>s using <see cref="ShapePortStyle"/>.
  ///   The provided <see cref="PortReshapeHandle"/> modifies the <see cref="ShapePortStyle.RenderSize" render size/>.
  /// </summary>
  internal sealed class PortReshapeHandleProvider : IReshapeHandleProvider
  {
    private readonly SizeD minimumSize;
    private readonly ShapePortStyle portStyle;
    private readonly IPort port;

    /// <summary>
    ///   Creates a new instance for port and its style.
    /// </summary>
    /// <param name="port">The port whose visualization shall be resized.</param>
    /// <param name="portStyle">The style whose render size shall be changed.</param>
    /// <param name="minimumSize">The minimum size the <see cref="ShapePortStyle.RenderSize"/> may have.</param>
    public PortReshapeHandleProvider(IPort port, ShapePortStyle portStyle, SizeD minimumSize) {
      this.port = port;
      this.portStyle = portStyle;
      this.minimumSize = minimumSize;
    }

    /// <summary>
    ///   Returns <see cref="HandlePositions.Corners"/> or <see cref="HandlePositions.Border"/> as available handle
    ///   positions depending on the modifier state of <c>Shift</c>.
    /// </summary>
    /// <param name="context">The context the handles are created in.</param>
    /// <returns>Either <see cref="HandlePositions.Corners"/> or <see cref="HandlePositions.Border"/></returns>
    public HandlePositions GetAvailableHandles(IInputModeContext context) {
      var shiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
      return shiftPressed ? HandlePositions.Border : HandlePositions.Corners;
    }

    /// <summary>
    ///   Returns a <see cref="PortReshapeHandle"/> for the port at the given position and
    ///   sets its <see cref="PortReshapeHandle.minimumSize"/> to <see cref="minimumSize"/>.
    /// </summary>
    /// <param name="context">The context the handles are created in.</param>
    /// <param name="position">The position the handle shall be created for.</param>
    /// <returns>A <see cref="PortReshapeHandle"/> for the port at the given position.</returns>
    public IHandle GetHandle(IInputModeContext context, HandlePositions position) =>
        new PortReshapeHandle(context, port, portStyle, position, minimumSize);
  }
}
