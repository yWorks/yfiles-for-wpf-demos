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
using System.Windows.Media;
using yWorks.Controls;

namespace yWorks.Demo.ViewportLimiter
{
  /// <summary>
  ///   Animates the projection between two alpha and scale values.
  /// </summary>
  internal sealed class ProjectionAnimation : IAnimation
  {
    private readonly double toScale;
    private readonly double fromScale;
    private readonly double toAlpha;
    private readonly double fromAlpha;
    private readonly GraphControl graphControl;

    public ProjectionAnimation(
        GraphControl graphControl,
        double fromAlpha,
        double toAlpha,
        double fromScale,
        double toScale) {
      this.graphControl = graphControl;
      this.fromAlpha = fromAlpha;
      this.toAlpha = toAlpha;
      this.fromScale = fromScale;
      this.toScale = toScale;
    }

    public void CleanUp() {
      // NOP
    }

    public void Initialize() {
      // NOP
    }

    public void Animate(double time) {
      var currentAlpha = (1 - time) * fromAlpha + time * toAlpha;
      var currentScale = (1 - time) * fromScale + time * toScale;

      var cos = Math.Cos(currentAlpha);
      var sin = Math.Sin(currentAlpha);

      graphControl.Projection = new MatrixTransform(cos, -currentScale * sin, sin, currentScale * cos, 0, 0);
    }

    public TimeSpan PreferredDuration => TimeSpan.FromMilliseconds(1000);
  }
}
