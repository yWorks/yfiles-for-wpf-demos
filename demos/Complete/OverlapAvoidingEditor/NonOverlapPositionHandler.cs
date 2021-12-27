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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.OverlapAvoidingEditor
{
  /// <summary>
  /// An <see cref="IPositionHandler"/> that moves a node and creates space at the new location.
  /// </summary>
  internal class NonOverlapPositionHandler : IPositionHandler
  {
    /// <summary>
    /// The node we are currently moving.
    /// </summary>
    private readonly INode node;

    /// <summary>
    /// The original <see cref="IPositionHandler"/>.
    /// </summary>
    private readonly IPositionHandler handler;

    /// <summary>
    /// Creates space at the new location.
    /// </summary>
    private LayoutHelper layoutHelper;

    /// <summary>
    /// To check whether re-parenting is taking place.
    /// </summary>
    private IReparentNodeHandler reparentHandler;

    public NonOverlapPositionHandler(INode node, IPositionHandler handler) {
      this.node = node;
      this.handler = handler;
    }

    /// <summary>
    /// Returns the location of the node.
    /// </summary>
    public IPoint Location { get { return handler.Location; } }

    #region Drag handling

    /// <summary>
    /// The node is upon to be dragged.
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      reparentHandler = context.Lookup<IReparentNodeHandler>();
      var graphControl = context.CanvasControl as GraphControl;
      layoutHelper = new LayoutHelper(graphControl, node);
      layoutHelper.InitializeLayout();
      handler.InitializeDrag(context);
    }

    /// <summary>
    /// The node is dragged.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      handler.HandleMove(context, originalLocation, newLocation);
      // stop changing the graph layout during re-parenting is taking place
      if (reparentHandler == null || !reparentHandler.IsReparentGesture(context, node)) {
        layoutHelper.RunLayout();
      }
    }

    /// <summary>
    /// The drag is canceled.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      handler.CancelDrag(context, originalLocation);
      layoutHelper.CancelLayout();
    }

    /// <summary>
    /// The drag is finished.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      handler.DragFinished(context, originalLocation, newLocation);
      layoutHelper.FinishLayout();
    }

    #endregion
  }
}
