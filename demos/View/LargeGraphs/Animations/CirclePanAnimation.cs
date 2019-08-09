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
using yWorks.Geometry;

namespace Demo.yFiles.Graph.LargeGraphs.Animations
{
  /// <summary>
  ///   An animation that pans the viewport in a circular motion.
  /// </summary>
  /// <remarks>
  ///   The animation pans the viewport in a circle with a diameter of half the viewport's width.
  /// </remarks>
  public class CirclePanAnimation : IAnimation
  {
    /// <summary>The <see cref="CanvasControl" /> whose viewport will be animated.</summary>
    private readonly CanvasControl canvas;

    /// <summary>The number of rotations during the animation.</summary>
    private readonly double revolutions;

    /// <summary>The rotation angle during the last frame.</summary>
    /// <remarks>This is needed for correct interaction with a simultaneous zoom animation.</remarks>
    private double lastAngle;

    /// <summary>The circle radius during the last frame.</summary>
    /// <remarks>This is needed for correct interaction with a simultaneous zoom animation.</remarks>
    private double lastRadius;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CirclePanAnimation" /> class with the given number of revolutions and
    ///   animation time.
    /// </summary>
    /// <param name="canvas">The <see cref="CanvasControl" /> whose viewport will be animated.</param>
    /// <param name="revolutions">The number of rotations during the animation.</param>
    /// <param name="duration">The duration of the animation.</param>
    public CirclePanAnimation(CanvasControl canvas, double revolutions, TimeSpan duration) {
      this.canvas = canvas;
      this.revolutions = revolutions;
      PreferredDuration = duration;
    }

    /// <inheritdoc />
    public void Initialize() {
      lastAngle = 0;
      lastRadius = canvas.Viewport.Width / 4;
    }

    /// <inheritdoc />
    public void Animate(double time) {
      // The circle radius depends on the viewport size to be zoom-invariant
      var radius = canvas.Viewport.Width / 4;
      var totalAngle = 2 * Math.PI * revolutions;
      var currentAngle = totalAngle * time;

      // Undo the last frame's movement first
      PointD undo = new PointD(Math.Cos(lastAngle) * lastRadius, Math.Sin(lastAngle) * lastRadius);
      // Then apply the current one. This is needed to play well with a simultaneous zoom animation.
      canvas.ViewPoint = canvas.ViewPoint - undo +
                         new PointD(Math.Cos(currentAngle) * radius, Math.Sin(currentAngle) * radius);

      lastRadius = radius;
      lastAngle = currentAngle;
    }

    /// <inheritdoc />
    public void CleanUp() {}

    /// <inheritdoc />
    public TimeSpan PreferredDuration { get; private set; }
  }
}