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
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;

namespace yWorks.Demo.NodeAlignment
{
  /// <summary>
  ///   Stores the minimum and maximum coordinate for a snap column or snap row.
  /// </summary>
  internal readonly record struct Range(double Min, double Max);

  /// <summary>
  ///   Shows areas in which dropping or moving a node will trigger a re-alignment of the graph.
  /// </summary>
  internal sealed class SnapDistanceVisualCreator : IVisualCreator
  {
    private static readonly Brush DefaultBrush = CreateBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
    private static readonly Brush InsideBrush = Brushes.LightGray;

    private bool initialized;
    private GraphControl? graphControl;
    private List<Range> snapColumns = [];
    private List<Range> snapRows = [];
    private Range highlightColumn;
    private Range highlightRow;

    /// <summary>
    ///   Creates a visualization of snap columns and snap rows during a drag gesture.
    ///   Dropping a node inside one of those areas causes the graph nodes to be re-aligned.
    /// </summary>
    public Visual CreateVisual(IRenderContext context) => CreateVisualization();

    /// <summary>
    ///   Updates a previously created visualization after the highlighted area changed.
    /// </summary>
    public Visual UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateVisualization();

    private DrawingVisual CreateVisualization() {
      var visual = new DrawingVisual();
      if (!initialized || graphControl == null) {
        return visual;
      }

      // Extend the viewport slightly so the snap columns and rows span the visible graph area.
      var viewport = graphControl.Viewport.GetEnlarged(4);

      using var drawing = visual.RenderOpen();
      foreach (var range in snapColumns) {
        drawing.DrawRectangle(DefaultBrush, null,
            new Rect(range.Min, viewport.Y, range.Max - range.Min, viewport.Height));
      }
      foreach (var range in snapRows) {
        drawing.DrawRectangle(DefaultBrush, null,
            new Rect(viewport.X, range.Min, viewport.Width, range.Max - range.Min));
      }

      DrawHighlight(drawing, highlightColumn, true, viewport);
      DrawHighlight(drawing, highlightRow, false, viewport);
      return visual;
    }

    /// <summary>
    ///   Clears the internal state of this creator.
    /// </summary>
    public void Clear() {
      snapColumns = [];
      snapRows = [];
      highlightColumn = default;
      highlightRow = default;
      initialized = false;
    }

    /// <summary>
    ///   Initializes the snap columns and rows for the graph and the current settings.
    /// </summary>
    public void Initialize(GraphControl graphControl, IPoint nodeCenter, Settings settings) {
      this.graphControl = graphControl;
      initialized = true;
      (snapColumns, snapRows) = CollectSnapRanges(graphControl.Graph, settings.AlignmentPolicy, settings.SnapDistance);
      UpdateNodeCenter(nodeCenter);
    }

    /// <summary>
    ///   Updates which snap column or row is highlighted for the given node center.
    /// </summary>
    public bool UpdateNodeCenter(IPoint nodeCenter) {
      var nextColumn = FindHighlightRange(snapColumns, nodeCenter.X);
      var nextRow = FindHighlightRange(snapRows, nodeCenter.Y);
      var changed = !SameRange(highlightColumn, nextColumn) || !SameRange(highlightRow, nextRow);
      highlightColumn = nextColumn;
      highlightRow = nextRow;
      return changed;
    }

    /// <summary>
    ///   Calculates snap columns and rows for the given graph. Each node center defines an area
    ///   extending the snap distance in both directions, and overlapping areas are merged.
    /// </summary>
    private static (List<Range> Columns, List<Range> Rows) CollectSnapRanges(
        IGraph graph,
        AlignmentPolicy alignmentPolicy,
        double snapDistance) {
      var collectColumns = alignmentPolicy is AlignmentPolicy.SnapXY or AlignmentPolicy.SnapX;
      var collectRows = alignmentPolicy is AlignmentPolicy.SnapXY or AlignmentPolicy.SnapY;
      var columns = new List<Range>();
      var rows = new List<Range>();
      var knownCenterX = new HashSet<double>();
      var knownCenterY = new HashSet<double>();

      foreach (var node in graph.Nodes) {
        var centerX = node.Layout.GetCenterX();
        if (collectColumns && knownCenterX.Add(centerX)) {
          columns.Add(new Range(centerX - snapDistance, centerX + snapDistance));
        }

        var centerY = node.Layout.GetCenterY();
        if (collectRows && knownCenterY.Add(centerY)) {
          rows.Add(new Range(centerY - snapDistance, centerY + snapDistance));
        }
      }

      return (MergeSnapRanges(columns), MergeSnapRanges(rows));
    }

    /// <summary>
    ///   Merges overlapping snap columns or rows into their union.
    /// </summary>
    private static List<Range> MergeSnapRanges(List<Range> ranges) {
      if (ranges.Count < 2) {
        return ranges;
      }

      var orderedRanges = ranges.OrderBy(range => range.Min).ToArray();
      var merged = new List<Range>();
      var current = orderedRanges[0];
      foreach (var range in orderedRanges.Skip(1)) {
        if (range.Min <= current.Max + 0.1) {
          current = new Range(current.Min, Math.Max(current.Max, range.Max));
        } else {
          merged.Add(current);
          current = range;
        }
      }
      merged.Add(current);
      return merged;
    }

    /// <summary>
    ///   Returns the range containing the given value, or the union if ranges overlap there.
    /// </summary>
    private static Range FindHighlightRange(IEnumerable<Range> ranges, double value) {
      var matching = ranges.Where(range => range.Min <= value && value <= range.Max).ToArray();
      return matching.Length == 0
          ? default
          : new Range(matching.Min(range => range.Min), matching.Max(range => range.Max));
    }

    /// <summary>
    ///   Determines whether two ranges are equal.
    /// </summary>
    private static bool SameRange(Range first, Range second) => first.Min == second.Min && first.Max == second.Max;

    /// <summary>
    ///   Updates the highlighted snap column or row.
    /// </summary>
    private static void DrawHighlight(DrawingContext drawing, Range range, bool columns, RectD viewport) {
      var size = range.Max - range.Min;
      if (size <= 0) {
        return;
      }

      var rectangle = columns
          ? new Rect(range.Min, viewport.Y, size, viewport.Height)
          : new Rect(viewport.X, range.Min, viewport.Width, size);
      drawing.DrawRectangle(InsideBrush, null, rectangle);
    }

    private static Brush CreateBrush(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }
  }
}
