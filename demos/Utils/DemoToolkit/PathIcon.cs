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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace yWorks.Demo
{
  public sealed class PathIcon : Control
  {
    private static readonly Rect Viewbox = new(new Size(1024, 1024));

    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(nameof(Geometry),
        typeof(System.Windows.Media.Geometry), typeof(PathIcon),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public System.Windows.Media.Geometry Geometry {
      get => (System.Windows.Media.Geometry) GetValue(GeometryProperty);
      set => SetValue(GeometryProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext) {
      base.OnRender(drawingContext);

      var geometry = Geometry;
      var foreground = Foreground;
      var width = RenderSize.Width;
      var height = RenderSize.Height;

      if (geometry == null || foreground == null || width <= 0.0 || height <= 0.0 || !IsFinite(width) ||
          !IsFinite(height)) {
        return;
      }

      var viewbox = Viewbox;

      var scale = Math.Min(width / viewbox.Width, height / viewbox.Height);
      var offsetX = ((width - (viewbox.Width * scale)) * 0.5) - (viewbox.X * scale);
      var offsetY = ((height - (viewbox.Height * scale)) * 0.5) - (viewbox.Y * scale);

      drawingContext.PushClip(new RectangleGeometry(new Rect(RenderSize)));
      drawingContext.PushTransform(new MatrixTransform(scale, 0.0, 0.0, scale, offsetX, offsetY));
      drawingContext.DrawGeometry(foreground, null, geometry);
      drawingContext.Pop();
      drawingContext.Pop();
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
  }
}
