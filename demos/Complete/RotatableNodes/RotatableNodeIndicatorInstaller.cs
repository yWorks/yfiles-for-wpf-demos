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
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// An extension of <see cref="OrientedRectangleIndicatorInstaller"/> that uses the rotated layout 
  /// of nodes using a <see cref="RotatableNodeStyleDecorator"/>.
  /// </summary>
  /// <remarks>
  /// The indicator will be rotated to fit the rotated bounds of the node.
  /// </remarks>
  public class RotatableNodeIndicatorInstaller : OrientedRectangleIndicatorInstaller
  {
    /// <summary>
    /// A <see cref="ResourceKey"/> that will be used to find the <see cref="DataTemplate"/>
    /// for drawing the selection indicator.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ResourceKey NodeSelectionTemplateKey =
      new ComponentResourceKey(typeof(LabelPositionHandler), "NodeSelectionTemplateKey");

    /// <summary>
    /// Create a new instance with the specified template key and no fixed node layout.
    /// </summary>
    /// <param name="templateKey">The key to lookup the visualization template.</param>
    public RotatableNodeIndicatorInstaller(ResourceKey templateKey) : base(null, templateKey) { }

    /// <summary>
    /// Returns the rotated layout of the specified node.
    /// </summary>
    protected override IOrientedRectangle GetRectangle(object item) {
      var node = item as INode;
      var styleWrapper = node.Style as RotatableNodeStyleDecorator;
      if (styleWrapper != null) {
        return styleWrapper.GetRotatedLayout(node);
      }
      return new OrientedRectangle(node.Layout);
    }
  }

  /// <summary>
  /// Static helper class that provides convenience access to commonly used
  /// <see cref="Brush"/> instances.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  internal static class HatchBrushes
  {
    private static readonly Brush hatchBrush;

    static HatchBrushes() {
      byte[] pixels = new byte[8];
      for (int i = 0; i < pixels.Length; i++) {
        pixels[i] = ((i & 1) == 0) ? (byte) 0xAA : (byte) 0x55;
      }


      // Try creating a new image with a custom palette.
      List<Color> colors = new List<Color> {Colors.Black, Colors.White};
      BitmapPalette myPalette = new BitmapPalette(colors);

      BitmapSource source = BitmapSource.Create(8, 8, 96, 96, PixelFormats.Indexed1, myPalette, pixels, 1);
      ImageBrush brush = new ImageBrush(source) {
        TileMode = TileMode.Tile,
        ViewportUnits = BrushMappingMode.Absolute,
        Viewport = new Rect(0, 0, 8, 8),
        ViewboxUnits = BrushMappingMode.Absolute,
        Viewbox = new Rect(0, 0, 8, 8)
      };

      brush.Freeze();
      hatchBrush = brush;
    }

    /// <summary>
    /// Gets a 50 percent hatch brush.
    /// </summary>
    /// <value>The frozen brush instance.</value>
    public static Brush Hatch50 {
      get { return hatchBrush; }
    }
  }
}
