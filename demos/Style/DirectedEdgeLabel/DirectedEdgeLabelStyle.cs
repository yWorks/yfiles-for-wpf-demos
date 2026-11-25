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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DirectedEdgeLabel
{
  /// <summary>
  ///   A label style for edges which renders a flow indicator. The indicator points to the edge's source or target,
  ///   depending on the <see cref="ToSource"/> setting. A <see cref="VisualGroup"/> stores the render data cache
  ///   used when the visual is updated.
  /// </summary>
  public sealed class DirectedEdgeLabelStyle : LabelStyleBase<VisualGroup>
  {
    private const double HorizontalInset = 3;
    private const double VerticalInset = 2;
    private const double ArrowSize = 18;

    /// <summary>
    ///   Whether the indicator points to the source
    /// </summary>
    public bool ToSource { get; set; }

    /// <summary>
    ///   The text color
    /// </summary>
    public Brush TextFill { get; set; } = Themes.Palette31.Text;

    /// <summary>
    ///   The background color
    /// </summary>
    public Brush BackgroundFill { get; set; } = Themes.Palette31.EdgeLabelFill;

    /// <summary>
    ///   The fill for the indicator
    /// </summary>
    public Brush ArrowFill { get; set; } = Brushes.Orange;

    /// <summary>
    ///   The stroke for the indicator
    /// </summary>
    public Pen ArrowStroke { get; set; } = new(Brushes.Black, 1);

    /// <summary>
    ///   The text font
    /// </summary>
    private readonly FontFamily font = new("Arial");

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="toSource">If set to true the indicator points to the source.</param>
    public DirectedEdgeLabelStyle(bool toSource = false) {
      ToSource = toSource;
    }

    /// <summary>
    ///   Creates the visual for a label to be drawn.
    /// </summary>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      var group = new VisualGroup();

      // Get the necessary data for rendering of the label
      var cache = CreateRenderDataCache(label, font, ToSource, GetDirection(label), TextFill, BackgroundFill,
          ArrowStroke, ArrowFill);
      // Render the label
      group.Add(new Rectangle());
      group.Add(new TextBlock());
      group.Add(CreateArrow(cache.Direction, cache.ArrowStroke, cache.ArrowFill));
      Render(group, label.GetLayout(), cache);
      // Arrange the group at the correct location and orientation
      ArrangeByLayout(context, group, label.GetLayout(), false);

      // Store the information used to create the rendering on the visual group
      group.SetRenderDataCache(cache);
      return group;
    }

    /// <summary>
    ///   Determines the direction for the indicator.
    ///   Calculates it from the direction to the edge's port of interest relative to the center of the label layout.
    ///   The port of interest is the source port if <see cref="ToSource"/> is true, the target port otherwise.
    /// </summary>
    /// <param name="label">The label to get the direction for.</param>
    private ArrowDirection GetDirection(ILabel label) {
      if (label.Owner is not IEdge edge) {
        // Fallback if the label has been removed or is not at an edge
        return ArrowDirection.Up;
      }
      var center = label.GetLayout().GetCenter();
      var target = (ToSource ? edge.SourcePort : edge.TargetPort).GetLocation();
      var dx = center.X - target.X;
      var dy = center.Y - target.Y;
      // The preferred direction (vertical or horizontal)
      var topDown = Math.Abs(dx) < Math.Abs(dy);
      if (topDown) {
        return dy > 0 ? ArrowDirection.Up : ArrowDirection.Down;
      }
      return dx > 0 ? ArrowDirection.Left : ArrowDirection.Right;
    }

    /// <summary>
    ///   Re-renders the label using the old visual for performance reasons.
    ///   <see cref="LabelStyleBase{VisualGroup}.UpdateVisual"/>.
    /// </summary>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      if (oldVisual.Children.Count != 3 || oldVisual.Children[0] is not Rectangle ||
          oldVisual.Children[1] is not TextBlock || oldVisual.Children[2] is not Path) {
        return CreateVisual(context, label);
      }

      // Get the data with which the old visual was created
      var oldCache = oldVisual.GetRenderDataCache<LabelRenderDataCache>();
      // Get the data for the new visual
      var newCache = CreateRenderDataCache(label, font, ToSource, GetDirection(label), TextFill, BackgroundFill,
          ArrowStroke, ArrowFill);
      if (!newCache.Equals(oldCache)) {
        // Something changed - re-render the visual
        Render(oldVisual, label.GetLayout(), newCache);
        oldVisual.SetRenderDataCache(newCache);
      }
      // Nothing changed, return the old visual
      // Arrange because the layout might have changed
      ArrangeByLayout(context, oldVisual, label.GetLayout(), false);
      return oldVisual;
    }

    /// <summary>
    ///   Creates the visual appearance of a label.
    /// </summary>
    private void Render(VisualGroup group, IOrientedRectangle labelLayout, LabelRenderDataCache cache) {
      // The background rectangle
      var rect = (Rectangle) group.Children[0]!;
      rect.Width = labelLayout.Width;
      rect.Height = labelLayout.Height;
      rect.RadiusX = 3;
      rect.RadiusY = 3;
      rect.StrokeThickness = 1;
      rect.Fill = cache.BackgroundFill;
      rect.SetCanvasArrangeRect(new Rect(0, 0, labelLayout.Width, labelLayout.Height));

      // The text
      var text = (TextBlock) group.Children[1]!;
      text.Text = cache.Text;
      text.FontFamily = cache.Font;
      text.FontSize = cache.FontSize;
      text.Foreground = cache.TextFill;
      text.TextWrapping = TextWrapping.NoWrap;

      // Calculate the size of the text element
      text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      var textSize = text.DesiredSize;

      // If the indicator points to the source it is placed left, then
      // move the text to the right
      var translateX = cache.ToSource ? 2 * HorizontalInset + ArrowSize : HorizontalInset;

      // Calculate vertical offset for centered alignment
      var translateY = (labelLayout.Height - textSize.Height) * 0.5;

      text.SetCanvasArrangeRect(new Rect(translateX, translateY, textSize.Width, textSize.Height));

      var button = (Path) group.Children[2]!;
      button.Data = CreateArrowGeometry(cache.Direction);
      button.Fill = cache.ArrowFill;
      button.SetCanvasArrangeRect(new Rect(
          cache.ToSource ? HorizontalInset : labelLayout.Width - HorizontalInset - ArrowSize, VerticalInset + 1, 16,
          16));
    }

    /// <summary>
    ///   Creates an object containing all necessary data to create a label visual.
    /// </summary>
    private static LabelRenderDataCache CreateRenderDataCache(
        ILabel label,
        FontFamily font,
        bool toSource,
        ArrowDirection direction,
        Brush textFill,
        Brush backgroundFill,
        Pen arrowStroke,
        Brush arrowFill) =>
        // Visibility of button changes dependent on the zoom level
        new(label.Text, toSource, direction, font, textFill, backgroundFill, arrowStroke, arrowFill);

    /// <summary>
    ///   Calculates the preferred size for the given label if this style is used for the rendering.
    ///   The size is calculated from the label's text.
    ///   <see cref="LabelStyleBase{VisualGroup}.GetPreferredSize"/>.
    /// </summary>
    protected override SizeD GetPreferredSize(ILabel label) {
      // First measure
      var text = new TextBlock {
          Text = label.Text,
          FontFamily = font,
          FontSize = LabelRenderDataCache.FontSizeValue,
          TextWrapping = TextWrapping.NoWrap
      };
      text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      var size = text.DesiredSize;

      // Then use the desired size - plus rounding and insets, as well as space for the button
      return new SizeD(Math.Ceiling(0.5 + size.Width) + HorizontalInset * 3 + ArrowSize,
          2 * VerticalInset + Math.Max(ArrowSize, Math.Ceiling(0.5 + size.Height)));
    }

    private static Path CreateArrow(ArrowDirection direction, Pen arrowStroke, Brush arrowFill) =>
        new() {
            Data = CreateArrowGeometry(direction),
            Fill = arrowFill
        };

    private static System.Windows.Media.Geometry CreateArrowGeometry(ArrowDirection direction) {
      var pathData = direction switch {
          ArrowDirection.Down => "M 6,0 L 6,8 L 0,8 L 8,16 L 16,8 L 10,8 L 10,0 Z",
          ArrowDirection.Up => "M 6,16 L 6,8 L 0,8 L 8,0 L 16,8 L 10,8 L 10,16 Z",
          ArrowDirection.Left => "M 16,6 L 8,6 L 8,0 L 0,8 L 8,16 L 8,10 L 16,10 Z",
          ArrowDirection.Right => "M 0,6 L 8,6 L 8,0 L 16,8 L 8,16 L 8,10 L 0,10 Z",
          _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
      };
      return System.Windows.Media.Geometry.Parse(pathData);
    }

    private enum ArrowDirection
    {
      Up,
      Right,
      Down,
      Left
    }

    /// <summary>
    ///   The cache containing the rendering information for a label visual.
    /// </summary>
    private readonly record struct LabelRenderDataCache
    {
      public const double FontSizeValue = 12;

      public Brush ArrowFill { get; }

      public Pen ArrowStroke { get; }

      public Brush BackgroundFill { get; }

      public Brush TextFill { get; }

      public FontFamily Font { get; }

      public double FontSize => FontSizeValue;

      public ArrowDirection Direction { get; }

      public bool ToSource { get; }

      public string Text { get; }

      public LabelRenderDataCache(
          string text,
          bool toSource,
          ArrowDirection direction,
          FontFamily font,
          Brush textFill,
          Brush backgroundFill,
          Pen arrowStroke,
          Brush arrowFill) {
        Text = text;
        ToSource = toSource;
        Direction = direction;
        Font = font;
        TextFill = textFill;
        BackgroundFill = backgroundFill;
        ArrowStroke = arrowStroke;
        ArrowFill = arrowFill;
      }
    }
  }
}
