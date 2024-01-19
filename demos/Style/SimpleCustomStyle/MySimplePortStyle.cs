/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.SimpleCustomStyle
{
  /// <summary>
  /// This class is an example of a custom port style based on the <see cref="PortStyleBase{TVisual}"/> class.
  /// The port is rendered as a circle.
  /// </summary>
  public class MySimplePortStyle : PortStyleBase<Ellipse> {
    private static readonly SolidColorBrush ellipseStroke;

    static MySimplePortStyle() {
      ellipseStroke = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255));
      ellipseStroke.Freeze();
    }

    // the size of the port rendering - immutable
    private const int Width = 4;
    private const int Height = 4;

    protected override Ellipse CreateVisual(IRenderContext context, IPort port) {
      // create the ellipse
      var visual = new Ellipse
                            {
                              Stroke = ellipseStroke,
                              Width = Width,
                              Height = Height,
                            };

      // and arrange it
      visual.SetCanvasArrangeRect(new Rect(GetLocation(port) + new PointD(-Width * 0.5, -Height * 0.5), new SizeD(Width, Height)));
      return visual;
    }

    protected override Ellipse UpdateVisual(IRenderContext context, Ellipse oldVisual, IPort port) {
      // arrange the old ellipse
      oldVisual.SetCanvasArrangeRect(new Rect(GetLocation(port) + new PointD(-Width * 0.5, -Height * 0.5), new SizeD(Width, Height)));
      return oldVisual;
    }

    /// <summary>
    /// Calculates the bounds of this port.
    /// </summary>
    /// <remarks>
    /// These are also used for arranging the visual, hit testing, visibility testing, and marquee box tests.
    /// </remarks>
    protected override RectD GetBounds(ICanvasContext context, IPort port) {
      return RectD.FromCenter(GetLocation(port), new SizeD(Width, Height));
    }

    /// <summary>
    /// This method returns the static current location of the port.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns>The current location of the given port.</returns>
    private static PointD GetLocation(IPort port) {
      var param = port.LocationParameter;
      return param.Model.GetLocation(port, param);
    }
  }
}
