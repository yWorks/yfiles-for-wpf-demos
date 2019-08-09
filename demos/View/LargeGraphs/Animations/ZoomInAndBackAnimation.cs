/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Controls;

namespace Demo.yFiles.Graph.LargeGraphs.Animations
{
  /// <summary>
  ///   Animation that zooms in and out again.
  /// </summary>
  /// <remarks>
  ///   Half the animation duration is spent zooming in from the initial zoom level to a given target zoom level. The
  ///   other half of the animation duration is spent zooming out again.
  /// </remarks>
  public class ZoomInAndBackAnimation : IAnimation
  {
    /// <summary>The <see cref="CanvasControl" /> whose viewport will be animated.</summary>
    private readonly CanvasControl canvas;

    /// <summary>Binary logarithm of the target zoom level.</summary>
    private readonly double targetZoomLog;

    /// <summary>The zoom level difference between the initial and the target zoom level.</summary>
    private double delta;

    /// <summary>Binary logarithm of the initial zoom level.</summary>
    private double initialZoomLog;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ZoomInAndBackAnimation" /> class with the given target zoom level and
    ///   duration.
    /// </summary>
    /// <param name="canvas">The <see cref="CanvasControl" /> whose viewport will be animated.</param>
    /// <param name="targetZoom">The target zoom level.</param>
    /// <param name="duration">The duration of the animation.</param>
    public ZoomInAndBackAnimation(CanvasControl canvas, double targetZoom, TimeSpan duration) {
      this.canvas = canvas;
      targetZoomLog = Math.Log(targetZoom, 2);
      PreferredDuration = duration;
    }

    /// <inheritdoc />
    public void Initialize() {
      initialZoomLog = Math.Log(canvas.Zoom, 2);
      delta = targetZoomLog - initialZoomLog;
    }

    /// <inheritdoc />
    public void Animate(double time) {
      var newZoom = time < 0.5
          ? initialZoomLog + delta * (time * 2)
          : targetZoomLog - delta * (time - 0.5) * 2;
      canvas.Zoom = Math.Pow(2, newZoom);
    }

    /// <inheritdoc />
    public void CleanUp() {}

    /// <inheritdoc />
    public TimeSpan PreferredDuration { get; private set; }
  }
}