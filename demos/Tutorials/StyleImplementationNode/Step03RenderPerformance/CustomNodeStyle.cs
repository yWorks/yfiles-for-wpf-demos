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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step03RenderPerformance
{
  /// <summary>
  ///   A node style that renders a card shape with a tab and reuses its visual between render passes.
  /// </summary>
  internal sealed class CustomNodeStyle : NodeStyleBase<CustomNodeStyle.CustomNodeVisual>
  {
    private static readonly Brush Fill = CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(11, 113, 137));
    private static readonly Pen Border = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(4, 45, 55));

    protected override CustomNodeVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new CustomNodeVisual();
      var layout = node.Layout;
      visual.UpdatePath(layout.Width, layout.Height);
      visual.UpdateLocation(layout.X, layout.Y);
      return visual;
    }

    protected override CustomNodeVisual UpdateVisual(IRenderContext context, CustomNodeVisual oldVisual, INode node) {
      var layout = node.Layout;
      oldVisual.UpdatePath(layout.Width, layout.Height);
      oldVisual.UpdateLocation(layout.X, layout.Y);
      return oldVisual;
    }

    /// <summary>The visual that renders and caches the node's path.</summary>
    internal sealed class CustomNodeVisual : DrawingVisual
    {
      private const double TabWidth = 50;
      private const double TabHeight = 10;

      private double width;
      private double height;
      private StreamGeometry? path;
      private readonly TranslateTransform translation = new();

      public CustomNodeVisual() {
        Transform = translation;
      }

      private static StreamGeometry CreatePath(double width, double height) {
        var path = new StreamGeometry();
        using (var pathContext = path.Open()) {
          pathContext.BeginFigure(new Point(0, 0), true, true);
          pathContext.LineTo(new Point(TabWidth, 0), true, false);
          pathContext.LineTo(new Point(TabWidth, TabHeight), true, false);
          pathContext.LineTo(new Point(width, TabHeight), true, false);
          pathContext.LineTo(new Point(width, height), true, false);
          pathContext.LineTo(new Point(0, height), true, false);
        }
        path.Freeze();
        return path;
      }

      public void UpdatePath(double width, double height) {
        // only update the path if the size has changed
        if (path == null || this.width != width || this.height != height) {
          this.width = width;
          this.height = height;
          path = CreatePath(width, height);
          Render();
        }
      }

      public void UpdateLocation(double x, double y) {
        if (translation.X != x) {
          translation.X = x;
        }
        if (translation.Y != y) {
          translation.Y = y;
        }
      }

      private void Render() {
        using var drawingContext = RenderOpen();
        drawingContext.DrawGeometry(Fill, Border, path!);
      }
    }
  }
}
