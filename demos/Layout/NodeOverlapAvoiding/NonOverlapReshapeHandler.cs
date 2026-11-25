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
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.NodeOverlapAvoiding
{
  /// <summary>
  ///   An <see cref="IReshapeHandler"/> that resizes a node and creates space if it grows.
  /// </summary>
  internal sealed class NonOverlapReshapeHandler : IReshapeHandler
  {
    /// <summary>
    ///   The node we are currently resizing.
    /// </summary>
    private readonly INode node;

    /// <summary>
    ///   The original <see cref="IReshapeHandler"/>.
    /// </summary>
    private readonly IReshapeHandler handler;

    /// <summary>
    ///   Creates space if the node grows.
    /// </summary>
    private LayoutHelper? layoutHelper;

    private readonly DispatcherTimer timer;

    public NonOverlapReshapeHandler(INode node, IReshapeHandler handler) {
      this.node = node;
      this.handler = handler;
      timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
      timer.Tick += async (_, _) => {
        timer.Stop();
        if (layoutHelper != null) {
          await layoutHelper.RunLayoutAsync();
        }
      };
    }

    public IRectangle Bounds => handler.Bounds;

    /// <summary>
    ///   The node is about to be resized.
    /// </summary>
    public void InitializeReshape(IInputModeContext context) {
      if (context.InputMode is HandleInputMode) {
        layoutHelper = new LayoutHelper((GraphControl) context.CanvasControl, node);
        layoutHelper.InitializeLayout();
      } else {
        layoutHelper = null;
      }
      handler.InitializeReshape(context);
    }

    /// <summary>
    ///   The node is resized.
    /// </summary>
    public void HandleReshape(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      timer.Stop();
      handler.HandleReshape(context, originalBounds, newBounds);
      timer.Start();
    }

    /// <summary>
    ///   The resize gesture is canceled.
    /// </summary>
    public async void CancelReshape(IInputModeContext context, RectD originalBounds) {
      timer.Stop();
      handler.CancelReshape(context, originalBounds);

      var waitInputMode = context.Lookup<WaitInputMode>();
      if (waitInputMode != null) {
        // Disable user interaction while the cancel layout is running.
        waitInputMode.Waiting = true;
      }

      if (layoutHelper != null) {
        await layoutHelper.CancelLayoutAsync();
      }

      if (waitInputMode != null) {
        waitInputMode.Waiting = false;
      }
    }

    /// <summary>
    ///   The resize gesture is finished.
    /// </summary>
    public async void ReshapeFinished(IInputModeContext context, RectD originalBounds, RectD newBounds) {
      timer.Stop();
      handler.ReshapeFinished(context, originalBounds, newBounds);

      var waitInputMode = context.Lookup<WaitInputMode>();
      if (waitInputMode != null) {
        // Disable user interaction while the finish layout is running.
        waitInputMode.Waiting = true;
      }

      if (layoutHelper != null) {
        await layoutHelper.FinishLayoutAsync();
      }

      if (waitInputMode != null) {
        waitInputMode.Waiting = false;
      }
    }
  }
}
