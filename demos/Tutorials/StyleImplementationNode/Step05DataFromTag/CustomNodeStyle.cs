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

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step05DataFromTag
{
  /// <summary>
  ///   A node style that reads its fill color and badge visibility from the node's tag.
  /// </summary>
  public sealed class CustomNodeStyle : NodeStyleBase<CustomNodeStyle.CustomNodeVisual>
  {
    protected override CustomNodeVisual CreateVisual(IRenderContext context, INode node) {
      var visual = new CustomNodeVisual();
      UpdateCustomNodeVisual(node, visual);
      return visual;
    }

    protected override CustomNodeVisual UpdateVisual(IRenderContext context, CustomNodeVisual oldVisual, INode node) {
      UpdateCustomNodeVisual(node, oldVisual);
      return oldVisual;
    }

    private static void UpdateCustomNodeVisual(INode node, CustomNodeVisual visual) {
      var layout = node.Layout;
      var pathChanged = visual.UpdatePath(layout.Width, layout.Height);
      visual.UpdateLocation(layout.X, layout.Y);

      // get the fill color and badge visibility from the node's tag
      var color = Color.FromRgb(11, 113, 137);
      var showBadge = false;
      if (node.Tag is NodeData nodeData) {
        if (nodeData.Color != null) {
          color = (Color) ColorConverter.ConvertFromString(nodeData.Color)!;
        }
        showBadge = nodeData.ShowBadge;
      }

      var fillChanged = visual.UpdateFillColor(color);
      var badgeChanged = visual.UpdateBadge(showBadge);
      if (pathChanged || fillChanged || badgeChanged) {
        visual.Render();
      }
    }

    /// <summary>The visual that renders and caches the card and optional badge.</summary>
    public sealed class CustomNodeVisual : DrawingVisual
    {
      private const double TabWidth = 50;
      private const double TabHeight = 14;

      private static readonly Pen Border = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(51, 51, 51));
      private static readonly Brush BadgeFill = CustomNodeStyleTutorial.CreateBrush(Color.FromRgb(108, 159, 68));
      private static readonly Pen BadgeBorder = CustomNodeStyleTutorial.CreatePen(Color.FromRgb(73, 108, 46));
      private static readonly EllipseGeometry BadgeGeometry = CreateBadgeGeometry();

      private double width;
      private double height;
      private StreamGeometry? path;
      private Color? fillColor;
      private Brush? fillBrush;
      private bool showBadge;
      private readonly TranslateTransform translation = new();

      public CustomNodeVisual() {
        Transform = translation;
      }

      private static EllipseGeometry CreateBadgeGeometry() {
        var geometry = new EllipseGeometry(new Point(0, 0), 8, 8);
        geometry.Freeze();
        return geometry;
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

      public void UpdateLocation(double x, double y) {
        if (translation.X != x) {
          translation.X = x;
        }
        if (translation.Y != y) {
          translation.Y = y;
        }
      }

      public bool UpdateFillColor(Color color) {
        if (fillColor != color) {
          fillColor = color;
          fillBrush = CustomNodeStyleTutorial.CreateBrush(color);
          return true;
        }
        return false;
      }

      public bool UpdateBadge(bool showBadge) {
        if (this.showBadge != showBadge) {
          this.showBadge = showBadge;
          return true;
        }
        return false;
      }

      public void Render() {
        using var drawingContext = RenderOpen();
        drawingContext.DrawGeometry(fillBrush, Border, path!);
        if (showBadge) {
          drawingContext.DrawGeometry(BadgeFill, BadgeBorder, BadgeGeometry);
        }
      }
    }
  }
}
