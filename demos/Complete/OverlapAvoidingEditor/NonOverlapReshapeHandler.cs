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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.OverlapAvoidingEditor
{
  /// <summary>
  /// An <see cref="IReshapeHandler"/> that resizes a node and creates space if it grows.
  /// </summary>
  internal class NonOverlapReshapeHandler : IReshapeHandler
  {
    /// <summary>
    /// The node we are currently resizing.
    /// </summary>
    private readonly INode node;

    /// <summary>
    /// The original <see cref="IReshapeHandler"/>.
    /// </summary>
    private readonly IReshapeHandler handler;

    /// <summary>
    /// Creates space if the node grows.
    /// </summary>
    private LayoutHelper layoutHelper;

    public NonOverlapReshapeHandler(INode node, IReshapeHandler handler) {
      this.node = node;
      this.handler = handler;
    }

    /// <summary>
    /// Returns the bounds of the node.
    /// </summary>
    public IRectangle Bounds { get { return handler.Bounds; } }

    #region Resize handling

    /// <summary>
    /// The node is upon to be resized.
    /// </summary>
    public void InitializeReshape(IInputModeContext context) {
      var graphControl = context.CanvasControl as GraphControl;
      layoutHelper = new LayoutHelper(graphControl, node);
      layoutHelper.InitializeLayout();
      handler.InitializeReshape(context);
    }

    /// <summary>
    /// The node is resized.
    /// </summary>
    public void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      handler.HandleReshape(context, originalBounds, newBounds);
      layoutHelper.RunLayout();
    }

    /// <summary>
    /// The resize gesture is canceled.
    /// </summary>
    public void CancelReshape(IInputModeContext context, RectD originalBounds) {
      handler.CancelReshape(context, originalBounds);
      layoutHelper.CancelLayout();
    }

    /// <summary>
    /// The resize gesture is finished.
    /// </summary>
    public void ReshapeFinished(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      handler.ReshapeFinished(context, originalBounds, newBounds);
      layoutHelper.FinishLayout();
    }

    #endregion
  }
}
