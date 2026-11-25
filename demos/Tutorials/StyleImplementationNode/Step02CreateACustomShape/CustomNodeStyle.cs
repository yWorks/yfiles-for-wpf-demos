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

using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step02CreateACustomShape
{
  /// <summary>A node style that renders a simple card shape with a tab.</summary>
  internal sealed class CustomNodeStyle : NodeStyleBase<DrawingVisual>
  {
    private const double TabWidth = 50;
    private const double TabHeight = 10;

    private static readonly Brush Fill = CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(11, 113, 137));
    private static readonly Pen Border = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(4, 45, 55));

    protected override DrawingVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new DrawingVisual();
      Render(visual, node);
      return visual;
    }

    private static void Render(DrawingVisual visual, INode node) {
      var layout = node.Layout;
      var path = CreatePath(layout.X, layout.Y, layout.Width, layout.Height);
      using var drawingContext = visual.RenderOpen();
      drawingContext.DrawGeometry(Fill, Border, path);
    }

    /// <summary>Creates the path for the card shape.</summary>
    private static StreamGeometry CreatePath(double x, double y, double width, double height) {
      var path = new StreamGeometry();
      using (var pathContext = path.Open()) {
        pathContext.BeginFigure(new Point(x, y), true, true);
        pathContext.LineTo(new Point(x + TabWidth, y), true, false);
        pathContext.LineTo(new Point(x + TabWidth, y + TabHeight), true, false);
        pathContext.LineTo(new Point(x + width, y + TabHeight), true, false);
        pathContext.LineTo(new Point(x + width, y + height), true, false);
        pathContext.LineTo(new Point(x, y + height), true, false);
      }
      path.Freeze();
      return path;
    }
  }
}
