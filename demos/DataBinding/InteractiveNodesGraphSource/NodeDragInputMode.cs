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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace Demo.yFiles.DataBinding.InteractiveNodesGraphSource {

  /// <summary>
  /// A custom input mode that starts a drag and drag operation when dragging from a node.
  /// </summary>
  internal class NodeDragInputMode : InputModeBase {

    private INode node;

    public override void Install(IInputModeContext context, ConcurrencyController concurrencyController) {
      base.Install(context, concurrencyController);
      // register the relevant events
      var canvas = context.CanvasControl;
      canvas.Mouse2DPressed += CanvasOnMouse2DPressed;
      canvas.Mouse2DReleased += CanvasOnMouse2DReleased;
      canvas.Mouse2DDragged += CanvasOnMouse2DDragged;
    }

    public override void Uninstall(IInputModeContext context) {
      base.Uninstall(context);
      // de-register the relevant events
      var canvas = context.CanvasControl;
      canvas.Mouse2DPressed -= CanvasOnMouse2DPressed;
      canvas.Mouse2DReleased -= CanvasOnMouse2DReleased;
      canvas.Mouse2DDragged -= CanvasOnMouse2DDragged;
    }

    private void CanvasOnMouse2DPressed(object sender, Mouse2DEventArgs me) {
      // get hitTester from context
      var hitTest = InputModeContext.Lookup<IHitTester<INode>>();
      // take first node and request mutex
      node = hitTest.EnumerateHits(InputModeContext, me.Location).FirstOrDefault();
    }

    private void CanvasOnMouse2DDragged(object sender, Mouse2DEventArgs me) {
      if (node != null) {
        RequestMutex();
        // start drag and drop
        DataObject dao = new DataObject();
        dao.SetData(typeof (INode), node);
        DragDrop.DoDragDrop(InputModeContext.CanvasControl, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        node = null;
        ReleaseMutex();
      }
    }

    private void CanvasOnMouse2DReleased(object sender, Mouse2DEventArgs me) {
      Cancel();
    }

    protected override void OnConcurrencyControllerDeactivated() {
      base.OnConcurrencyControllerDeactivated();
      node = null;
    }

    public override bool TryStop() {
      node = null;
      ReleaseMutex();
      return HasMutex();
    }
  }
}