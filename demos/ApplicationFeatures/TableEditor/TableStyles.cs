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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace yWorks.Demo.TableEditor
{
  /// <summary>
  ///   Registers the custom style types used by the table editor's GraphML sample.
  /// </summary>
  internal static class TableStyles
  {
    internal const string GraphMLNamespace = "yWorks.Demo.TableEditor";

    public static void RegisterGraphMLTypes(GraphMLIOHandler ioHandler) {
      ioHandler.AddXamlNamespaceMapping(GraphMLNamespace, "DemoTableStyle", typeof(DemoTableStyle));
      ioHandler.AddXamlNamespaceMapping(GraphMLNamespace, "DemoStripeStyle", typeof(DemoStripeStyle));
      ioHandler.AddXamlNamespaceMapping(GraphMLNamespace, "TableBackgroundStyle", typeof(TableBackgroundStyle));
    }
  }

  /// <summary>
  ///   Custom table node style which defines a clickable area on the table's headers and delegates
  ///   to <see cref="TableNodeStyle"/>.
  /// </summary>
  public sealed class DemoTableStyle : DelegatingNodeStyle
  {
    private readonly TableNodeStyle tableNodeStyle;

    public DemoTableStyle() : this(new Table()) { }

    public DemoTableStyle(ITable table) {
      tableNodeStyle = new TableNodeStyle(table) {
          TableRenderingOrder = TableRenderingOrder.RowsFirst,
          BackgroundStyle = new TableBackgroundStyle()
      };
    }

    protected override INodeStyle GetStyle(INode node) => tableNodeStyle;

    public TableRenderingOrder TableRenderingOrder {
      get => tableNodeStyle.TableRenderingOrder;
      set => tableNodeStyle.TableRenderingOrder = value;
    }

    public INodeStyle? BackgroundStyle {
      get => tableNodeStyle.BackgroundStyle;
      set => tableNodeStyle.BackgroundStyle = value;
    }

    public ITable Table {
      get => tableNodeStyle.Table;
      set => tableNodeStyle.Table = value;
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      if (!base.IsHit(context, location, node)) {
        return false;
      }

      var table = node.Lookup(typeof(ITable)) as ITable;
      if (table == null) {
        return true;
      }

      if (context.InputMode is CreateEdgeInputMode) {
        var accPadding = table.GetAccumulatedPadding();
        // during edge creation - the inside of the table is not considered a hit
        var nodeLayout = node.Layout.ToRectD();
        var innerRect = nodeLayout.GetEnlarged(new InsetsD(
            -accPadding.Left, -accPadding.Top, -accPadding.Right, -accPadding.Bottom));
        return !innerRect.Contains(location);
      }

      if (context.InputMode is MoveInputMode { IsDragging: true }) {
        // during movement of the node - the whole area is considered a hit
        return true;
      }

      if (context.InputMode is ClickInputMode) {
        var accPadding = table.GetAccumulatedPadding();
        // clicking will only work in the corners
        var nodeLayout = node.Layout.ToRectD();
        var tableBody = nodeLayout.GetEnlarged(
            new InsetsD(-accPadding.Left, -accPadding.Top, -accPadding.Right, -accPadding.Bottom));
        if (tableBody.Contains(location)) {
          return false;
        }
      }
      return true;
    }

    public override INodeStyle Clone() {
      var clone = new DemoTableStyle {
          TableRenderingOrder = TableRenderingOrder,
          BackgroundStyle = BackgroundStyle,
          Table = CopyTable(Table)
      };
      return clone;
    }

    private static ITable CopyTable(ITable table) => table is ICloneable cloneable ? (ITable) cloneable.Clone() : table;
  }

  /// <summary>
  ///   Custom table background style that uses a flat style.
  /// </summary>
  public sealed class TableBackgroundStyle : NodeStyleBase<Canvas>
  {
    private static readonly Brush AccentBrush = new SolidColorBrush(Color.FromRgb(0x0b, 0x71, 0x89));

    protected override Canvas? CreateVisual(IRenderContext context, INode node) {
      var table = node.Lookup(typeof(ITable)) as ITable;
      if (table == null) {
        return null;
      }

      var layout = node.Layout.ToRectD();
      var canvas = new Canvas {
          Width = Math.Max(0, layout.Width),
          Height = Math.Max(0, layout.Height),
          Background = Brushes.Transparent
      };
      DrawBackground(canvas, table.GetAccumulatedPadding(), layout.Width, layout.Height);
      canvas.Tag = new BackgroundVisualCache(layout.X, layout.Y, layout.Width, layout.Height);
      canvas.SetCanvasArrangeRect(layout.ToRect());
      return canvas;
    }

    /// <summary>
    ///   Creates and updates the inner rectangles for the headings.
    /// </summary>
    private static void DrawBackground(Canvas canvas, InsetsD padding, double width, double height) {
      Rectangle border;
      if (canvas.Children.Count == 0) {
        border = new Rectangle {
            Stroke = AccentBrush,
            StrokeThickness = 4,
            RadiusX = 1,
            RadiusY = 1,
            Fill = Brushes.Transparent
        };
        Canvas.SetLeft(border, 0);
        Canvas.SetTop(border, 0);
        canvas.Children.Add(border);
      } else {
        border = (Rectangle) canvas.Children[0];
      }
      border.Width = Math.Max(0, width);
      border.Height = Math.Max(0, height);

      var rectangles = new List<(double X, double Y, double Width, double Height, Brush Fill)>();

      if (padding.Top > 2 && padding.Left > 2) {
        AddRectangle(rectangles, 1, 1, padding.Left - 2, padding.Top - 2);
      }
      if (padding.Top > 2 && padding.Right > 2) {
        AddRectangle(rectangles, width - padding.Right + 1, 1, padding.Right - 2, padding.Top - 2);
      }
      if (padding.Bottom > 2 && padding.Left > 2) {
        AddRectangle(rectangles, 1, height - padding.Bottom + 1, padding.Left - 2, padding.Bottom - 2);
      }
      if (padding.Bottom > 2 && padding.Right > 2) {
        AddRectangle(rectangles, width - padding.Right + 1, height - padding.Bottom + 1, padding.Right - 2,
            padding.Bottom - 2);
      }

      for (var i = 0; i < rectangles.Count; i++) {
        var spec = rectangles[i];
        Rectangle rectangle;
        var childIndex = i + 1;
        if (childIndex < canvas.Children.Count) {
          rectangle = (Rectangle) canvas.Children[childIndex];
        } else {
          rectangle = new Rectangle();
          canvas.Children.Add(rectangle);
        }
        rectangle.Width = spec.Width;
        rectangle.Height = spec.Height;
        rectangle.Fill = spec.Fill;
        Canvas.SetLeft(rectangle, spec.X);
        Canvas.SetTop(rectangle, spec.Y);
      }
      while (canvas.Children.Count > rectangles.Count + 1) {
        canvas.Children.RemoveAt(canvas.Children.Count - 1);
      }
    }

    private static void AddRectangle(
        ICollection<(double X, double Y, double Width, double Height, Brush Fill)> rectangles,
        double x,
        double y,
        double width,
        double height) {
      if (width <= 0 || height <= 0) {
        return;
      }
      rectangles.Add((x, y, width, height, AccentBrush));
    }

    protected override Canvas? UpdateVisual(IRenderContext context, Canvas oldVisual, INode node) {
      var table = node.Lookup(typeof(ITable)) as ITable;
      var cache = oldVisual.Tag as BackgroundVisualCache;
      if (table == null || cache == null || oldVisual.Children.Count == 0) {
        return CreateVisual(context, node);
      }

      var layout = node.Layout.ToRectD();
      oldVisual.Width = Math.Max(0, layout.Width);
      oldVisual.Height = Math.Max(0, layout.Height);
      DrawBackground(oldVisual, table.GetAccumulatedPadding(), layout.Width, layout.Height);
      if (cache.X != layout.X || cache.Y != layout.Y || cache.Width != layout.Width || cache.Height != layout.Height) {
        cache.X = layout.X;
        cache.Y = layout.Y;
        cache.Width = layout.Width;
        cache.Height = layout.Height;
        oldVisual.SetCanvasArrangeRect(layout.ToRect());
      }
      return oldVisual;
    }

    private sealed class BackgroundVisualCache(double x, double y, double width, double height)
    {
      public double X { get; set; } = x;

      public double Y { get; set; } = y;

      public double Width { get; set; } = width;

      public double Height { get; set; } = height;
    }
  }

  /// <summary>
  ///   Custom style for the Stripes in a table.
  /// </summary>
  public sealed class DemoStripeStyle : NodeStyleBase<Canvas>
  {
    private static readonly Brush InsetBrush = new SolidColorBrush(Color.FromRgb(0x0b, 0x71, 0x89));
    private static readonly Brush LineBrush = new SolidColorBrush(Color.FromRgb(0xf0, 0xf0, 0xf0));

    protected override Canvas? CreateVisual(IRenderContext context, INode node) {
      var stripe = node.Lookup(typeof(IStripe)) as IStripe;
      if (stripe == null) {
        return null;
      }

      var layout = node.Layout.ToRectD();
      var canvas = new Canvas {
          Width = Math.Max(0, layout.Width),
          Height = Math.Max(0, layout.Height),
          Background = Brushes.Transparent
      };
      DrawStripe(canvas, stripe, layout.Width, layout.Height);
      canvas.Tag = new StripeVisualCache(layout.X, layout.Y, layout.Width, layout.Height);
      canvas.SetCanvasArrangeRect(layout.ToRect());
      return canvas;
    }

    /// <summary>
    ///   Creates and updates the inner rectangles for the headings and the table lines.
    /// </summary>
    private static void DrawStripe(Canvas canvas, IStripe stripe, double width, double height) {
      var rectangles = new List<(double X, double Y, double Width, double Height, Brush Fill)>();
      var isColumn = stripe is IColumn;
      var hasChildren = GetChildStripes(stripe).Any();
      var isFirst = hasChildren ? false : IsFirstStripe(stripe);
      var totalPadding = stripe.GetTotalPadding();
      var padding = hasChildren ? stripe.Padding :
          isColumn ? new InsetsD(0, totalPadding.Top, 0, totalPadding.Bottom) :
          new InsetsD(totalPadding.Left, 0, totalPadding.Right, 0);

      var x = 1d;
      var y = 1d;
      var w = Math.Max(0, width - 2);
      var h = Math.Max(0, height - 2);

      if (padding.Top > 2) {
        AddRectangle(rectangles, x, y, w, padding.Top - 2, InsetBrush);
      }
      y += padding.Top;
      h -= padding.Top;
      if (padding.Bottom > 2) {
        AddRectangle(rectangles, x, height - padding.Bottom + 1, w, padding.Bottom - 2, InsetBrush);
      }
      h -= padding.Bottom;

      if (padding.Left > 2) {
        AddRectangle(rectangles, x, y, padding.Left - 2, h, InsetBrush);
      }
      x += padding.Left;
      w -= padding.Left;
      if (padding.Right > 2) {
        AddRectangle(rectangles, width - padding.Right + 1, y, padding.Right - 2, h, InsetBrush);
      }
      w -= padding.Right;

      if (isColumn && !isFirst && !hasChildren) {
        AddRectangle(rectangles, -1, y, 2, h - 1, LineBrush);
      } else if (!isColumn && !isFirst && !hasChildren) {
        AddRectangle(rectangles, x, -1, w - 1, 2, LineBrush);
      }

      for (var i = 0; i < rectangles.Count; i++) {
        var spec = rectangles[i];
        Rectangle rectangle;
        if (i < canvas.Children.Count) {
          rectangle = (Rectangle) canvas.Children[i];
        } else {
          rectangle = new Rectangle();
          canvas.Children.Add(rectangle);
        }
        rectangle.Width = spec.Width;
        rectangle.Height = spec.Height;
        rectangle.Fill = spec.Fill;
        Canvas.SetLeft(rectangle, spec.X);
        Canvas.SetTop(rectangle, spec.Y);
      }
      while (canvas.Children.Count > rectangles.Count) {
        canvas.Children.RemoveAt(canvas.Children.Count - 1);
      }
    }

    protected override Canvas? UpdateVisual(IRenderContext context, Canvas oldVisual, INode node) {
      var stripe = node.Lookup(typeof(IStripe)) as IStripe;
      var cache = oldVisual.Tag as StripeVisualCache;
      if (stripe == null || cache == null) {
        return CreateVisual(context, node);
      }

      var layout = node.Layout.ToRectD();
      oldVisual.Width = Math.Max(0, layout.Width);
      oldVisual.Height = Math.Max(0, layout.Height);
      DrawStripe(oldVisual, stripe, layout.Width, layout.Height);
      if (cache.X != layout.X || cache.Y != layout.Y || cache.Width != layout.Width || cache.Height != layout.Height) {
        cache.X = layout.X;
        cache.Y = layout.Y;
        cache.Width = layout.Width;
        cache.Height = layout.Height;
        oldVisual.SetCanvasArrangeRect(layout.ToRect());
      }
      return oldVisual;
    }

    private static bool IsFirstStripe(IStripe stripe) {
      var root = stripe switch {
          IRow { Table: { } table } => (IStripe) table.RootRow,
          IColumn { Table: { } table } => table.RootColumn,
          _ => null
      };
      if (root == null) {
        return false;
      }
      var walker = root;
      while (!ReferenceEquals(walker, stripe)) {
        var firstChild = GetChildStripes(walker).FirstOrDefault();
        if (firstChild == null) {
          return false;
        }
        walker = firstChild;
      }
      return true;
    }

    private static IEnumerable<IStripe> GetChildStripes(IStripe stripe) =>
        stripe switch {
            IRow row => row.ChildRows,
            IColumn column => column.ChildColumns,
            _ => Enumerable.Empty<IStripe>()
        };

    private static void AddRectangle(
        ICollection<(double X, double Y, double Width, double Height, Brush Fill)> rectangles,
        double x,
        double y,
        double width,
        double height,
        Brush fill) {
      if (width <= 0 || height <= 0) {
        return;
      }
      rectangles.Add((x, y, width, height, fill));
    }

    private sealed class StripeVisualCache(double x, double y, double width, double height)
    {
      public double X { get; set; } = x;

      public double Y { get; set; } = y;

      public double Width { get; set; } = width;

      public double Height { get; set; } = height;
    }
  }
}
