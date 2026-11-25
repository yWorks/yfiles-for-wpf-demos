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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step01CreateARectangle
{
  /// <summary>A basic node style that renders a filled rectangle.</summary>
  internal sealed class CustomNodeStyle : NodeStyleBase<DrawingVisual>
  {
    private static readonly Brush Fill = CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(11, 113, 137));
    private static readonly Pen Border = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(4, 45, 55));

    protected override DrawingVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new DrawingVisual();
      Render(visual, node);
      return visual;
    }

    private static void Render(DrawingVisual visual, INode node) {
      var layout = node.Layout;
      using var drawingContext = visual.RenderOpen();
      drawingContext.DrawRectangle(Fill, Border, new Rect(layout.X, layout.Y, layout.Width, layout.Height));
    }
  }
}
