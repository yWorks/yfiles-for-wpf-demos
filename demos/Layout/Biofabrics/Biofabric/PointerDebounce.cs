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
using System.Diagnostics;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;

namespace yWorks.Demo.Biofabrics
{
  internal static class PointerDebounce
  {
    /// <summary>
    ///   Creates a debounced pointer listener that annotates movement events and signals fast movement.
    /// </summary>
    public static EventHandler<PointerEventArgs> WrapPointerWithMomentumAnalysis(
        Action<PointerEventArgs, GraphControl, bool> listener,
        double speedThreshold = 300,
        TimeSpan? stopDelay = null,
        TimeSpan? sampleInterval = null) {
      // Maximum speed in view coordinates per millisecond to consider stopped.
      var stopAfter = stopDelay ?? TimeSpan.FromMilliseconds(200);
      var sampleEvery = sampleInterval ?? TimeSpan.FromMilliseconds(50);
      const double smoothingFactor = 0.2;

      PointerEventArgs? lastEvent = null;
      GraphControl? lastSender = null;
      var lastTimestamp = 0L;
      PointD? lastPosition = null;
      var isMovingFast = false;
      double? smoothedSpeed = null;

      var stopTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = stopAfter };
      var sampleTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = sampleEvery };

      double CalculateSpeed(PointD first, PointD second, double timeDelta) {
        var distance = first.DistanceTo(second);
        var zoom = lastSender?.Zoom ?? 1;
        return zoom * distance / timeDelta;
      }

      double UpdateSmoothedSpeed(double currentSpeed) {
        smoothedSpeed = smoothedSpeed is null
            ? currentSpeed
            : smoothingFactor * currentSpeed + (1 - smoothingFactor) * smoothedSpeed.Value;
        return smoothedSpeed.Value;
      }

      void CheckSpeedAndTrigger() {
        if (lastEvent is null || lastPosition is null || lastSender is null) {
          return;
        }
        var now = Stopwatch.GetTimestamp();
        var timeDelta = (now - lastTimestamp) / (double) Stopwatch.Frequency;
        if (timeDelta > 0) {
          var speed = UpdateSmoothedSpeed(CalculateSpeed(lastPosition.Value, lastEvent.Location, timeDelta));
          if (speed <= speedThreshold && isMovingFast) {
            // Smoothed speed dropped below the threshold.
            isMovingFast = false;
          } else if (speed > speedThreshold) {
            isMovingFast = true;
          }
          listener(lastEvent, lastSender, isMovingFast);
          lastPosition = lastEvent.Location;
          lastTimestamp = now;
        }
      }

      sampleTimer.Tick += (_, _) => {
        sampleTimer.Stop();
        CheckSpeedAndTrigger();
      };
      stopTimer.Tick += (_, _) => {
        stopTimer.Stop();
        if (lastEvent is not null && lastSender is not null) {
          isMovingFast = false;
          smoothedSpeed = null;
          listener(lastEvent, lastSender, isMovingFast);
          lastPosition = lastEvent.Location;
          lastTimestamp = Stopwatch.GetTimestamp();
        }
      };

      return (sender, evt) => {
        if (sender is not GraphControl graphControl) {
          return;
        }
        lastEvent = evt;
        lastSender = graphControl;
        stopTimer.Stop();
        sampleTimer.Stop();

        if (lastPosition is null) {
          lastPosition = evt.Location;
          lastTimestamp = Stopwatch.GetTimestamp();
          isMovingFast = false;
          smoothedSpeed = null;
          return;
        }

        CheckSpeedAndTrigger();
        sampleTimer.Start();
        stopTimer.Start();
      };
    }
  }
}
