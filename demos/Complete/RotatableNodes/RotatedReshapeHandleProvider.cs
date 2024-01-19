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
using yWorks.Graph;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Provides reshape handles for rotated nodes.
  /// </summary>
  public class RotatedReshapeHandleProvider : IReshapeHandleProvider
  {
    private readonly INode node;
    private readonly IReshapeHandler reshapeHandler;

    /// <summary>
    /// Creates a new instance for a given node.
    /// </summary>
    public RotatedReshapeHandleProvider(INode node) {
      this.node = node;
      // use a reshape handler to properly handle 
      // implicit resizing of parent group nodes
      reshapeHandler = node.Lookup<IReshapeHandler>();
    }

    /// <summary>
    /// Returns all eight positions around a node.
    /// </summary>
    public HandlePositions GetAvailableHandles(IInputModeContext inputModeContext) {
      return HandlePositions.Border;
    }

    /// <summary>
    /// Returns a <see cref="RotatedNodeResizeHandle"/> for the given position and node.
    /// </summary>
    public IHandle GetHandle(IInputModeContext inputModeContext, HandlePositions position) {
      return new RotatedNodeResizeHandle(position, node, reshapeHandler, false);
    }
  }
}
