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
using Demo.yFiles.Complete.IsometricDrawing.Model;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Complete.IsometricDrawing
{
  /// <summary>
  /// A node handle that can be used to change the <see cref="Model.Geometry.Height"/> of the nodes <see cref="NodeData"/>.
  /// </summary>
  public class HeightHandle : IHandle
  {
    private readonly INode node;
    private readonly IInputModeContext inputModeContext;
    private readonly double minimumHeight;

    private double originalHeight = 0;

    public HeightHandle(INode node, IInputModeContext inputModeContext, double minimumHeight) {
      this.node = node;
      this.inputModeContext = inputModeContext;
      this.minimumHeight = minimumHeight;
    }

    public IPoint Location {
      get {
        var cc = this.inputModeContext.CanvasControl;
        var vp = cc.ToViewCoordinates(this.node.Layout.GetCenter());
        var up = PointD.Add(vp, new PointD(0, -Geometry.Height * this.inputModeContext.Zoom));
        return cc.ToWorldCoordinates(up);
      }
    }

    /// <summary>
    /// Initializes the drag.
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      this.originalHeight = Geometry.Height;
    }

    /// <summary>
    /// Updates the node according to the moving handle.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      this.AdjustNodeHeight(inputModeContext, originalLocation, newLocation);
    }

    /// <summary>
    /// Cancels the drag and cleans up.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      Geometry.Height = this.originalHeight;
    }

    /// <summary>
    /// Finishes the drag an applies changes.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      this.AdjustNodeHeight(context, originalLocation, newLocation);
    }

    /// <summary>
    /// Adjusts the node height according to how much the handle was moved.
    /// </summary>
    private void AdjustNodeHeight(IInputModeContext context, PointD oldLocation, PointD newLocation) {
      var newY = context.CanvasControl.ToViewCoordinates(newLocation).Y;
      var oldY = context.CanvasControl.ToViewCoordinates(oldLocation).Y;
      var delta = (newY - oldY) / context.Zoom;
      var newHeight = this.originalHeight - delta;
      Geometry.Height = Math.Max(this.minimumHeight, newHeight);
    }

    public HandleTypes Type {
      get { return HandleTypes.Resize; }
    }

    public Cursor Cursor {
      get { return Cursors.Hand; }
    }

    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }

    private Geometry Geometry {
      get {
        return ((NodeData) this.node.Tag).Geometry;
      }
    }
  }
}
