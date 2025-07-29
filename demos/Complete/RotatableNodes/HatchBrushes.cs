/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  ///   Static helper class that provides convenience access to commonly used
  ///   <see cref="Brush"/> instances.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  internal static class HatchBrushes
  {
    static HatchBrushes() {
      var pixels = new byte[8];
      for (var i = 0; i < pixels.Length; i++) {
        pixels[i] = (i & 1) == 0 ? (byte) 0xAA : (byte) 0x55;
      }

      // Try creating a new image with a custom palette.
      var colors = new List<Color> { Colors.Black, Colors.White };
      var myPalette = new BitmapPalette(colors);

      var source = BitmapSource.Create(8, 8, 96, 96, PixelFormats.Indexed1, myPalette, pixels, 1);
      var brush = new ImageBrush(source) {
          TileMode = TileMode.Tile,
          ViewportUnits = BrushMappingMode.Absolute,
          Viewport = new Rect(0, 0, 8, 8),
          ViewboxUnits = BrushMappingMode.Absolute,
          Viewbox = new Rect(0, 0, 8, 8)
      };

      brush.Freeze();
      Hatch50 = brush;
    }

    /// <summary>
    ///   Gets a 50 percent hatch brush.
    /// </summary>
    /// <value>The frozen brush instance.</value>
    public static Brush Hatch50 { get; }
  }
}
