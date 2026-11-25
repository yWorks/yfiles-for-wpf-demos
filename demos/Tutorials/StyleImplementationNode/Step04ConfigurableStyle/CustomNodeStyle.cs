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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step04ConfigurableStyle
{
  /// <summary>
  ///   A node style that renders a card shape with a tab and allows configuring its fill brush.
  /// </summary>
  public sealed class CustomNodeStyle : NodeStyleBase<CustomNodeStyle.CustomNodeVisual>
  {
    private static readonly Pen Border = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(51, 51, 51));

    private Brush fillBrush;

    /// <summary>Creates a style that uses the specified fill brush.</summary>
    public CustomNodeStyle(Brush fillBrush) {
      this.fillBrush = fillBrush ?? throw new ArgumentNullException(nameof(fillBrush));
    }

    /// <summary>Creates a style that uses the default fill brush.</summary>
    public CustomNodeStyle() : this(CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(11, 113, 137))) { }

    /// <summary>Gets or sets the brush used to fill nodes that use this style.</summary>
    public Brush FillBrush {
      get => fillBrush;
      set {
        if (value == null) {
          throw new ArgumentNullException(nameof(value));
        }
        if (!ReferenceEquals(fillBrush, value)) {
          fillBrush = value;
        }
      }
    }

    protected override CustomNodeVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new CustomNodeVisual();
      UpdateCustomNodeVisual(node, visual);
      return visual;
    }

    protected override CustomNodeVisual UpdateVisual(IRenderContext context, CustomNodeVisual oldVisual, INode node) {
      UpdateCustomNodeVisual(node, oldVisual);
      return oldVisual;
    }

    private void UpdateCustomNodeVisual(INode node, CustomNodeVisual visual) {
      var layout = node.Layout;
      var pathChanged = visual.UpdatePath(layout.Width, layout.Height);
      var fillChanged = visual.UpdateFillBrush(fillBrush);
      visual.UpdateLocation(layout.X, layout.Y);
      if (pathChanged || fillChanged) {
        visual.Render();
      }
    }

    /// <summary>The visual that renders and caches the node's path.</summary>
    public sealed class CustomNodeVisual : DrawingVisual
    {
      private const double TabWidth = 50;
      private const double TabHeight = 14;

      private double width;
      private double height;
      private StreamGeometry? path;
      private Brush? fillBrush;
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

      public bool UpdatePath(double width, double height) {
        if (path == null || this.width != width || this.height != height) {
          this.width = width;
          this.height = height;
          path = CreatePath(width, height);
          return true;
        }
        return false;
      }

      public bool UpdateFillBrush(Brush fillBrush) {
        if (!ReferenceEquals(this.fillBrush, fillBrush)) {
          this.fillBrush = fillBrush;
          return true;
        }
        return false;
      }

      public void UpdateLocation(double x, double y) {
        if (translation.X != x) {
          translation.X = x;
        }
        if (translation.Y != y) {
          translation.Y = y;
        }
      }

      public void Render() {
        using var drawingContext = RenderOpen();
        drawingContext.DrawGeometry(fillBrush, Border, path!);
      }
    }
  }
}
