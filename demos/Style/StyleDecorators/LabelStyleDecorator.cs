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
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.StyleDecorators
{
  /// <summary>
  ///   This label style decorator shows how to decorate another label style.
  /// </summary>
  public sealed class LabelStyleDecorator : LabelStyleBase<VisualGroup>
  {
    private static readonly Brush DecorationBrush = new SolidColorBrush(Color.FromRgb(0x22, 0x45, 0x56));
    private readonly ILabelStyle baseStyle;

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="baseStyle">The base style.</param>
    public LabelStyleDecorator(ILabelStyle baseStyle) {
      this.baseStyle = baseStyle;
    }

    /// <summary>
    ///   Creates a new visual as combination of the base label visualization and the decoration.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The created visual.</returns>
    /// <see cref="LabelStyleBase{TVisual}.CreateVisual(IRenderContext, ILabel)"/>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      var group = new VisualGroup();

      // create the base visualization
      var baseVisual = baseStyle.Renderer.GetVisualCreator(label, baseStyle).CreateVisual(context);
      group.Add(baseVisual!);

      // create the decoration
      var layout = label.GetLayout();
      var decoration = CreateDecoration(layout);
      ArrangeByLayout(context, decoration, layout, true);
      group.Add(decoration);
      group.SetRenderDataCache(new LabelVisualCache(layout.Width, layout.Height));
      return group;
    }

    /// <summary>
    ///   Updates the provided visual.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">
    ///   The visual that has been created in the call to
    ///   <see cref="LabelStyleBase{TVisual}.CreateVisual(IRenderContext, ILabel)"/>.
    /// </param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The updated visual.</returns>
    /// <see cref="LabelStyleBase{TVisual}.UpdateVisual(IRenderContext, TVisual, ILabel)"/>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      // check whether the elements are as expected
      if (oldVisual.Children.Count != 2 || oldVisual.Children[1] is not VisualGroup decoration) {
        return CreateVisual(context, label);
      }

      // update the base visual
      var baseVisual = baseStyle.Renderer.GetVisualCreator(label, baseStyle)
          .UpdateVisual(context, oldVisual.Children[0]);
      // Check whether the UpdateVisual method created a new element and replace the old one if needed.
      if (!ReferenceEquals(baseVisual, oldVisual.Children[0])) {
        oldVisual.Children[0] = baseVisual!;
      }

      var layout = label.GetLayout();
      var cache = oldVisual.GetRenderDataCache<LabelVisualCache>();
      if (cache.Width != layout.Width || cache.Height != layout.Height) {
        UpdateDecoration(decoration, layout);
        oldVisual.SetRenderDataCache(new LabelVisualCache(layout.Width, layout.Height));
      }

      // The base style visual already applies the label layout. Arrange only the decoration
      // to avoid applying the label location and orientation to the base visual a second time.
      ArrangeByLayout(context, decoration, layout, true);
      return oldVisual;
    }

    /// <summary>
    ///   Creates the visualization of the decoration.
    /// </summary>
    /// <param name="layout">The label layout.</param>
    /// <returns>The visual that provides the decoration.</returns>
    private static VisualGroup CreateDecoration(IOrientedRectangle layout) {
      const double xPadding = 3;
      const double yPadding = 5;

      var line1 = new Rectangle { Fill = DecorationBrush };
      line1.SetCanvasArrangeRect(new Rect(-xPadding, -yPadding + 1, layout.Width + 2 * xPadding, 2));

      var line2 = new Rectangle { Fill = DecorationBrush };
      line2.SetCanvasArrangeRect(new Rect(-xPadding, layout.Height + yPadding - 3, layout.Width + 2 * xPadding, 2));

      var group = new VisualGroup();
      group.Add(line1);
      group.Add(line2);
      return group;
    }

    /// <summary>
    ///   Updates the visualization of the decoration.
    /// </summary>
    /// <param name="visualGroup">The visual that provides the decoration.</param>
    /// <param name="layout">The label layout.</param>
    private static void UpdateDecoration(VisualGroup visualGroup, IOrientedRectangle layout) {
      const double padding = 3;
      var line1 = (Rectangle) visualGroup.Children[0]!;
      var line2 = (Rectangle) visualGroup.Children[1]!;
      line1.SetCanvasArrangeRect(new Rect(-padding, -padding, layout.Width + 2 * padding, 2));
      line2.SetCanvasArrangeRect(new Rect(-padding, layout.Height + padding - 1, layout.Width + 2 * padding, 2));
    }

    /// <summary>
    ///   Returns the preferred size of the base style for the provided label.
    /// </summary>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The preferred size.</returns>
    /// <see cref="LabelStyleBase{TVisual}.GetPreferredSize(ILabel)"/>
    protected override SizeD GetPreferredSize(ILabel label) => baseStyle.Renderer.GetPreferredSize(label, baseStyle);

    /// <summary>
    ///   Returns the bounds provided by the base style for the label.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The visual bounds.</returns>
    /// <see cref="LabelStyleBase{TVisual}.GetBounds(ICanvasContext, ILabel)"/>
    protected override RectD GetBounds(ICanvasContext context, ILabel label) =>
        baseStyle.Renderer.GetBoundsProvider(label, baseStyle).GetBounds(context);

    /// <summary>
    ///   Returns whether the base visualization is visible.
    /// </summary>
    /// <param name="context">The canvas context.</param>
    /// <param name="rectangle">The clipping rectangle.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization is visible.</returns>
    /// <see cref="LabelStyleBase{TVisual}.IsVisible(ICanvasContext, RectD, ILabel)"/>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) =>
        baseStyle.Renderer.GetVisibilityTestable(label, baseStyle).IsVisible(context, rectangle);

    /// <summary>
    ///   Returns whether the base visualization is hit, we don't want the decoration to be hit testable.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="location">The point to test.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization is hit.</returns>
    /// <see cref="LabelStyleBase{TVisual}.IsHit(IInputModeContext, PointD, ILabel)"/>
    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) =>
        baseStyle.Renderer.GetHitTestable(label, baseStyle).IsHit(context, location);

    /// <summary>
    ///   Returns whether the base visualization is in the box, we don't want the decoration to be marquee selectable.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="rectangle">The marquee selection box.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns><see langword="true"/> if the base visualization is in the box.</returns>
    /// <see cref="LabelStyleBase{TVisual}.IsInBox(IInputModeContext, RectD, ILabel)"/>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) =>
        // return only box containment test of baseStyle - we don't want the decoration to be marquee selectable
        baseStyle.Renderer.GetMarqueeTestable(label, baseStyle).IsInBox(context, rectangle);

    /// <summary>
    ///   Delegates the lookup to the base style.
    /// </summary>
    /// <param name="label">The label to use for the context lookup.</param>
    /// <param name="type">The type to query.</param>
    /// <returns>An implementation of the <paramref name="type"/> or <see langword="null"/>.</returns>
    /// <see cref="LabelStyleBase{TVisual}.Lookup(ILabel, System.Type)"/>
    protected override object? Lookup(ILabel label, Type type) =>
        baseStyle.Renderer.GetContext(label, baseStyle).Lookup(type);

    private readonly record struct LabelVisualCache(double Width, double Height);
  }
}
