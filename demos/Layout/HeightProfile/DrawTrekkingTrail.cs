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

using System.Collections.Generic;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;

namespace yWorks.Demo.HeightProfile
{
  internal static class DrawTrekkingTrail
  {
    /// <summary>
    ///   Adds a trekking trail visual to the background group of the given graph control.
    /// </summary>
    public static void Draw(GraphControl graphControl, IReadOnlyList<PointD> scaledTrail) {
      graphControl.RenderTree.CreateElement(graphControl.RenderTree.BackgroundGroup, new TrailVisual(scaledTrail));
    }

    /// <summary>
    ///   Creates the trail based on a given set of points.
    /// </summary>
    private sealed class TrailVisual(IReadOnlyList<PointD> trailPoints) : IVisualCreator
    {
      /// <summary>
      ///   Creates a path that represents the trail based on the given set of points.
      /// </summary>
      public Visual CreateVisual(IRenderContext context) {
        var path = GetTrailPath();

        // creates a background path that will be used to color the background of the trail
        var backgroundPath = new GeneralPath();
        backgroundPath.MoveTo(trailPoints[0].X, 0);
        backgroundPath.Append(path, true);
        backgroundPath.LineTo(trailPoints[^1].X, 0);
        backgroundPath.Close();

        var visual = new DrawingVisual();
        using (var drawingContext = visual.RenderOpen()) {
          backgroundPath.Render(drawingContext, new SolidColorBrush(Color.FromRgb(0xC0, 0xE1, 0xE9)), null!);
          path.Render(drawingContext, null!, new Pen(new SolidColorBrush(Color.FromRgb(0x0B, 0x71, 0x89)), 1.5));
        }
        return visual;
      }

      /// <summary>
      ///   Creates a general path formed by the trail points.
      /// </summary>
      private GeneralPath GetTrailPath() {
        var path = new GeneralPath();
        path.MoveTo(trailPoints[0]);
        foreach (var point in trailPoints) {
          path.LineTo(point);
        }
        return path.CreateSmoothedPath(100);
      }

      /// <summary>
      ///   Delegates the call to the <see cref="CreateVisual"/> method.
      /// </summary>
      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisual(context);
    }
  }
}
