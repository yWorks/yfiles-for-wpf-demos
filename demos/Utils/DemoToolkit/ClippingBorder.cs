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
  /// <summary>
  ///   A <see cref="Border"/> that automatically clips its contents to the (uniform) corner radius.
  /// </summary>
  public class ClippingBorder : Border
  {
    protected override Size ArrangeOverride(Size finalSize) {
      var size = base.ArrangeOverride(finalSize);
      if (Child == null) {
        return size;
      }
      var thickness = BorderThickness.Left;
      var cr = CornerRadius;
      var radius = Math.Max(cr.TopLeft, Math.Max(cr.TopRight, Math.Max(cr.BottomLeft, cr.BottomRight))) -
          thickness * 0.5;
      var childSize = Child is FrameworkElement uie ? new Size(uie.ActualWidth, uie.ActualHeight) : default;
      var clipGeometry = Child.Clip as RectangleGeometry ?? new();
      clipGeometry.Rect = new Rect(childSize);
      clipGeometry.RadiusX = clipGeometry.RadiusY = radius;
      Child.Clip = clipGeometry;
      return size;
    }
  }
}
