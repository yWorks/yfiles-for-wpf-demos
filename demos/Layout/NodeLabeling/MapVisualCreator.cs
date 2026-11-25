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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using yWorks.Controls;
using WpfGeometry = System.Windows.Media.Geometry;

namespace yWorks.Demo.NodeLabeling
{
  /// <summary>
  ///   This class renders the map used as background in this demo.
  /// </summary>
  internal sealed class MapVisualCreator : IVisualCreator
  {
    private const double ImageWidth = 640;
    private const double ImageHeight = 480;
    private static readonly Brush MapBrush = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc));
    private readonly WpfGeometry mapGeometry;
    private readonly Rect mapBounds;

    public MapVisualCreator(Stream svgStream) {
      var document = XDocument.Load(svgStream);
      var pathData = document.Descendants().First(element => element.Name.LocalName == "path").Attribute("d")!.Value;
      mapGeometry = WpfGeometry.Parse(pathData);
      mapGeometry.Freeze();
      mapBounds = mapGeometry.Bounds;
      MapBrush.Freeze();
    }

    /// <summary>
    ///   Creates the visual for the background map.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used</param>
    /// <returns>The visual for the background map</returns>
    public Visual CreateVisual(IRenderContext context) {
      var visual = new DrawingVisual();
      using (var drawingContext = visual.RenderOpen()) {
        var scale = Math.Min(ImageWidth / mapBounds.Width, ImageHeight / mapBounds.Height);
        var offsetX = (ImageWidth - mapBounds.Width * scale) / 2 - mapBounds.X * scale;
        var offsetY = (ImageHeight - mapBounds.Height * scale) / 2 - mapBounds.Y * scale;
        drawingContext.PushTransform(new MatrixTransform(new Matrix(scale, 0, 0, scale, offsetX, offsetY)));
        drawingContext.DrawGeometry(MapBrush, null, mapGeometry);
        drawingContext.Pop();
      }
      return visual;
    }

    /// <summary>
    ///   Updates the visual for the background map.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used</param>
    /// <param name="oldVisual">The old visual</param>
    /// <returns>The old visual</returns>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => oldVisual ?? CreateVisual(context);
  }
}
