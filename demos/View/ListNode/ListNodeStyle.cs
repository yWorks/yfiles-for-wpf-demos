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
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ListNode;

/// <summary>
///   A node style with a header, a thick border, and row separators.
///   The row contents are rendered as ports and port labels.
///   The header text is rendered as a node label.
/// </summary>
public sealed class ListNodeStyle : NodeStyleBase<VisualGroup>
{
  private const double BorderThickness = 4;
  private const double HeaderHeight = 20;
  private const double RowHeight = 20;

  private static readonly Brush BorderBrush = CreateBrush(0x2C, 0x4B, 0x52);
  private static readonly Brush BackgroundBrush = CreateBrush(0x9C, 0xC5, 0xCF);
  private static readonly Brush HighlightBrush = CreateBrush(0x6A, 0xA7, 0xB0);
  private static readonly Pen LinePen = new(Brushes.White, 1);

  private static Brush CreateBrush(byte red, byte green, byte blue) {
    var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
    brush.Freeze();
    return brush;
  }

  /// <summary>
  ///   Creates the visual for a node and caches the data used to render it.
  /// </summary>
  protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
    var group = new VisualGroup();
    var cache = CreateRenderDataCache(node);
    Render(group, cache);
    group.SetRenderDataCache(cache);
    group.SetCanvasArrangeRect(node.Layout.ToRect());
    return group;
  }

  /// <summary>
  ///   Updates the visual when the node size or its row data changes.
  /// </summary>
  protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
    var oldCache = oldVisual.GetRenderDataCache<ListNodeRenderDataCache>();
    var newCache = CreateRenderDataCache(node);
    if (oldCache != newCache) {
      Clear(oldVisual);
      Render(oldVisual, newCache);
      oldVisual.SetRenderDataCache(newCache);
    }
    oldVisual.SetCanvasArrangeRect(node.Layout.ToRect());
    return oldVisual;
  }

  private static ListNodeRenderDataCache CreateRenderDataCache(INode node) {
    var info = (NodeInfo) node.Tag!;
    return new ListNodeRenderDataCache(new SizeD(node.Layout.Width, node.Layout.Height), info.Rows.Count,
        info.DraggingIndex, BorderBrush, BackgroundBrush, HighlightBrush, LinePen);
  }

  private static void Render(VisualGroup group, ListNodeRenderDataCache cache) {
    var width = cache.NodeSize.Width;
    var height = cache.NodeSize.Height;
    var border = new Rectangle {
        Width = width,
        Height = height,
        RadiusX = 2,
        RadiusY = 2,
        Fill = cache.BackgroundBrush,
        Stroke = cache.BorderBrush,
        StrokeThickness = BorderThickness
    };
    border.SetCanvasArrangeRect(new Rect(0, 0, width, height));
    group.Add(border);

    if (height < HeaderHeight) {
      return;
    }

    var header = new Rectangle {
        Width = width,
        Height = HeaderHeight,
        Fill = cache.BorderBrush
    };
    header.SetCanvasArrangeRect(new Rect(0, 0, width, HeaderHeight));
    group.Add(header);

    var borderInset = BorderThickness / 2;
    for (var i = 0; i < cache.RowCount; i++) {
      var y = HeaderHeight + RowHeight * i;
      if (cache.DraggingIndex == i) {
        var highlight = new Rectangle {
            Width = width - borderInset * 2,
            Height = RowHeight,
            Fill = cache.HighlightBrush
        };
        highlight.SetCanvasArrangeRect(new Rect(borderInset, y, width - borderInset * 2, RowHeight));
        group.Add(highlight);
      }
      if (i > 0) {
        var separator = new Rectangle {
            Width = width - borderInset * 2,
            Height = 1,
            Fill = cache.LinePen.Brush
        };
        separator.SetCanvasArrangeRect(new Rect(borderInset, y, width - borderInset * 2, 1));
        group.Add(separator);
      }
    }
  }

  private static void Clear(VisualGroup group) {
    group.Children.Clear();
  }

  /// <summary>
  ///   Modifies the node lookup to provide port candidates and a minimum node height.
  /// </summary>
  protected override object? Lookup(INode node, Type type) {
    if (type == typeof(IPortCandidateProvider)) {
      return new ExistingAndFreePortCandidateProvider(node);
    }
    if (type == typeof(INodeSizeConstraintProvider)) {
      var info = (NodeInfo) node.Tag!;
      return new NodeSizeConstraintProvider(new SizeD(100, GetMinimumHeight(info)), SizeD.Infinite, RectD.Empty);
    }
    return base.Lookup(node, type);
  }

  /// <summary>
  ///   Returns the index of the row at the given location, or <c>-1</c> when no row is hit.
  /// </summary>
  public int GetRowIndex(INode node, PointD location) {
    var layout = node.Layout;
    var y = location.Y - layout.Y;
    if (location.X < layout.X || location.X > layout.X + layout.Width || y <= HeaderHeight || y > layout.Height) {
      return -1;
    }
    var index = (int) ((y - HeaderHeight) / RowHeight);
    return index < ((NodeInfo) node.Tag!).Rows.Count ? index : -1;
  }

  /// <summary>
  ///   Determines whether the location lies inside the node header.
  /// </summary>
  public bool IsHeaderHit(INode node, PointD location) {
    var layout = node.Layout;
    return layout.X <= location.X && location.X <= layout.X + layout.Width && layout.Y <= location.Y &&
        location.Y <= layout.Y + HeaderHeight;
  }

  /// <summary>
  ///   Returns the y-coordinate of the center of the row at the given index, relative to the node.
  /// </summary>
  public double GetRowCenterY(int rowIndex) => HeaderHeight + RowHeight / 2 + rowIndex * RowHeight;

  /// <summary>
  ///   Returns the minimum height required to display the header and all rows.
  /// </summary>
  public double GetMinimumHeight(NodeInfo info) => HeaderHeight + info.Rows.Count * RowHeight;

  private sealed class ExistingAndFreePortCandidateProvider(INode node) : IPortCandidateProvider
  {
    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) =>
        GetCandidates(context, false);

    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context, IPortCandidate target) =>
        GetCandidates(context, false);

    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) =>
        GetCandidates(context, true);

    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context, IPortCandidate source) =>
        GetCandidates(context, true);

    private IEnumerable<IPortCandidate> GetCandidates(IInputModeContext context, bool incoming) {
      var graph = context.GetGraph();
      if (graph is null) {
        return [];
      }

      return [
          .. node.Ports.Where(port => ((RowPortInfo) port.Tag!).Incoming == incoming).Select(port =>
              new PortCandidate(port) {
                  Validity = graph.Degree(port) == 0 ? PortCandidateValidity.Valid : PortCandidateValidity.Invalid
              })
      ];
    }
  }

  private readonly record struct ListNodeRenderDataCache(
      SizeD NodeSize,
      int RowCount,
      int? DraggingIndex,
      Brush BorderBrush,
      Brush BackgroundBrush,
      Brush HighlightBrush,
      Pen LinePen);
}
