/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Tutorial.CustomStyles
{
  /// <summary>
  ///   A label style that renders labels always at the same size regardless of the zoom level.
  /// </summary>
  /// <remarks>
  ///   The style is implemented as a wrapper for an existing label style.
  /// </remarks>
  public class ZoomInvariantLabelStyle : LabelStyleBase<VisualGroup>
  {
    /// <summary>
    ///   A <see cref="ResourceKey"/> that will be used to find the <see cref="DataTemplate"/>
    ///   for drawing the selection indicator.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static readonly ResourceKey SelectionTemplateKey =
        new ComponentResourceKey(typeof(ZoomInvariantLabelStyle), "SelectionTemplateKey");

    private readonly SimpleLabel dummyLabel;
    private readonly OrientedRectangle rectangle;

    /// <summary>
    ///   Instantiates a new label style.
    /// </summary>
    public ZoomInvariantLabelStyle(ILabelStyle innerLabelStyle) {
      InnerLabelStyle = innerLabelStyle;
      rectangle = new OrientedRectangle();
      dummyLabel = new SimpleLabel { LayoutParameter = new FreeLabelModel().CreateDynamic(rectangle) };
    }

    /// <summary>
    ///   The inner style to use for the rendering.
    /// </summary>
    public ILabelStyle InnerLabelStyle { get; set; }

    /// <summary>
    ///   Returns the preferred size calculated by the <see cref="InnerLabelStyle"/>.
    /// </summary>
    protected override SizeD GetPreferredSize(ILabel label) {
      return InnerLabelStyle.Renderer.GetPreferredSize(label, InnerLabelStyle);
    }

    /// <summary>
    ///   Creates the visual for the label.
    /// </summary>
    /// <remarks>
    ///   Implementation of LabelStyleBase.CreateVisual.
    /// </remarks>
    /// <param name="context">The render context.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The visual as required by the <see cref="IVisualCreator.CreateVisual"/> interface.</returns>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      // Updates the dummy label which is internally used for rendering with the properties of the given label.
      UpdateDummyLabel(context, label);
      // creates the container for the visual and sets a transform for view coordinates
      var container = new VisualGroup();
      // ReSharper disable once PossibleUnintendedReferenceComparison
      if (container.Transform != context.IntermediateTransform) {
        container.Transform = context.IntermediateTransform;
      }

      var cache = CreateRenderDataCache(context, label);
      container.SetRenderDataCache(cache);

      var creator = InnerLabelStyle.Renderer.GetVisualCreator(dummyLabel, InnerLabelStyle);

      // create a new IRenderContext with a zoom of 1
      var cc = new ContextConfigurator(context.CanvasControl.ContentBounds);
      var innerContext = cc.CreateRenderContext(context.CanvasControl);

      // The wrapped style should always think it's rendering with zoom level 1
      var visual = creator.CreateVisual(innerContext);
      if (visual == null) {
        return container;
      }

      // add the created visual to the container
      container.Children.Add(visual);
      // if the label is selected, add the selection visualization, too.
      if (cache.Selected) {
        var selectionVisual = CreateSelectionVisual(innerContext, dummyLabel.GetLayout()) as UIElement;
        if (selectionVisual != null) {
          selectionVisual.IsHitTestVisible = false;
        }
        container.Children.Add(selectionVisual);
      }
      return container;
    }

    public Visual CreateSelectionVisual(IRenderContext context, IOrientedRectangle layout) {
      var template = context.CanvasControl.TryFindResource(SelectionTemplateKey) as DataTemplate;
      var container = new VisualGroup();
      if (template?.LoadContent() is not FrameworkElement frameworkElement) {
        return null;
      }
      container.Children.Add(frameworkElement);
      return UpdateSelectionVisual(context, container, layout);
    }

    /// <summary>
    ///   Update the visual previously created by <see cref="CreateVisual"/>.
    /// </summary>
    /// <remarks>
    ///   Implementation of LabelStyleBase.UpdateVisual.
    /// </remarks>
    /// <param name="context">The render context.</param>
    /// <param name="oldVisual">The visual that has been created in the call to <see cref="CreateVisual"/>.</param>
    /// <param name="label">The label to which this style instance is assigned.</param>
    /// <returns>The visual as required by the <see cref="IVisualCreator.CreateVisual"/> interface.</returns>
    /// <seealso cref="CreateVisual"/>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      // get the data with which the old visual was created
      var oldCache = oldVisual.GetRenderDataCache<RenderDataCache>();
      // get the data for the new visual
      var newCache = CreateRenderDataCache(context, label);
      oldVisual.SetRenderDataCache(newCache);

      // create a new visual if the cache has been changed or the container's contents seem to be wrong
      var cacheChanged = !newCache.Equals(oldCache);
      if (cacheChanged) {
        return CreateVisual(context, label);
      }

      if (oldVisual == null || oldVisual.Children.Count != (newCache.Selected ? 2 : 1)) {
        return CreateVisual(context, label);
      }

      var visual = oldVisual.Children[0];
      if (visual == null) {
        return CreateVisual(context, label);
      }

      // Updates the dummy label which is internally used for rendering with the properties of the given label.
      UpdateDummyLabel(context, label);

      // create a new IRenderContext with a zoom of 1
      var cc = new ContextConfigurator(context.CanvasControl.ContentBounds);
      var innerContext = cc.CreateRenderContext(context.CanvasControl);

      // ReSharper disable once PossibleUnintendedReferenceComparison
      if (oldVisual.Transform != context.IntermediateTransform) {
        oldVisual.Transform = context.IntermediateTransform;
      }

      // update the visual created by the inner style renderer
      var creator = InnerLabelStyle.Renderer.GetVisualCreator(dummyLabel, InnerLabelStyle);
      var updatedVisual = creator.UpdateVisual(innerContext, visual);
      if (updatedVisual == null) {
        // nothing to display -> return nothing
        return null;
      }

      // ReSharper disable once PossibleUnintendedReferenceComparison
      if (updatedVisual != visual) {
        oldVisual.Remove(visual);
        oldVisual.Add(updatedVisual);
      }

      // if selected: update the selection visual, too.
      if (newCache.Selected) {
        var oldSelectionVisual = oldVisual.Children[1];

        var selectionVisual = UpdateSelectionVisual(innerContext, oldSelectionVisual, dummyLabel.GetLayout());
        // ReSharper disable once PossibleUnintendedReferenceComparison
        if (oldSelectionVisual != selectionVisual) {
          oldVisual.Children.Remove(oldSelectionVisual);
          oldVisual.Children.Add(selectionVisual);
        }
      }
      return oldVisual;
    }


    public Visual UpdateSelectionVisual(IRenderContext context, Visual oldVisual, IOrientedRectangle layout) {
      var container = oldVisual as VisualGroup;
      if (container != null && container.Children.Count == 1) {
        var visual = container.Children[0] as FrameworkElement;
        if (visual != null) {
          var transform = context.IntermediateTransform;
          container.Transform = transform;
          var anchor = layout.GetAnchor();
          var anchorAndUp = anchor + layout.GetUpVector();
          anchor = context.WorldToIntermediateCoordinates(anchor);
          anchorAndUp = context.WorldToIntermediateCoordinates(anchorAndUp);

          var or = new OrientedRectangle();
          or.SetUpVector((anchorAndUp - anchor).Normalized);
          or.Anchor = anchor;
          or.Width = layout.Width * context.Zoom;
          or.Height = layout.Height * context.Zoom;
          visual.Width = or.Width;
          visual.Height = or.Height;
          visual.SetCanvasArrangeRect(new Rect(0, 0, or.Width, or.Height));
          ArrangeByLayout(context, visual, or, false);

          if (!container.IsMeasureValid) {
            container.Arrange(new Rect(0, 0, or.Width, or.Height));
          }

          return container;
        }
      }
      return CreateSelectionVisual(context, layout);
    }

    /// <summary>
    ///   Updates the internal label to match the given original label.
    /// </summary>
    private void UpdateDummyLabel(ICanvasContext context, ILabel original) {
      dummyLabel.Owner = original.Owner;
      dummyLabel.Style = original.Style;
      dummyLabel.Tag = original.Tag;
      dummyLabel.Text = original.Text;

      var location = original.GetLayout();
      rectangle.SetShape(location);
      rectangle.Width = location.Width / context.Zoom;
      rectangle.Height = location.Height / context.Zoom;

      dummyLabel.PreferredSize = rectangle.ToSizeD();
      rectangle.SetShape(original.LayoutParameter.Model.GetGeometry(dummyLabel, original.LayoutParameter));
      dummyLabel.PreferredSize = location.ToSizeD();
      WorldToIntermediateCoordinates(context, rectangle);
    }

    /// <inheritdoc/>
    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      UpdateDummyLabel(context, label);
      return IntermediateToWorldCoordinates(context,
          InnerLabelStyle.Renderer.GetBoundsProvider(dummyLabel, InnerLabelStyle).GetBounds(context));
    }

    /// <inheritdoc/>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      return GetBounds(context, label).Intersects(rectangle);
    }

    /// <inheritdoc/>
    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      return GetBounds(context, label).Contains(location);
    }

    /// <inheritdoc/>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) {
      return GetBounds(context, label).Intersects(rectangle);
    }

    /// <summary>
    ///   This implementation of the look up provides a custom implementation of the
    ///   <see cref="ISelectionRenderer"/> interface that better suits to this style.
    /// </summary>
    /// <remarks>
    ///   This implementation uses the actual visual label bounds to render the selection
    /// </remarks>
    protected override object Lookup(ILabel label, Type type) {
      return type == typeof(ISelectionRenderer) ? new VoidSelectionRenderer() : base.Lookup(label, type);
    }

    /// <summary>
    ///   Creates an object containing all necessary data to create an edge visual
    /// </summary>
    private static RenderDataCache CreateRenderDataCache(IRenderContext context, ILabel edge) {
      var selection = context.CanvasControl.Lookup<IGraphSelection>();
      var selected = selection != null && selection.Contains(edge);
      return new RenderDataCache(selected);
    }

    private sealed class VoidSelectionRenderer : ISelectionRenderer
    {
      public IVisualCreator GetVisualCreator(object renderTag) {
        return VoidVisualCreator.Instance;
      }

      public IBoundsProvider GetBoundsProvider(object renderTag) {
        return BoundsProviders.Empty;
      }

      public IVisibilityTestable GetVisibilityTestable(object renderTag) {
        return VisibilityTestables.Never;
      }

      public IHitTestable GetHitTestable(object renderTag) {
        return HitTestables.Never;
      }
    }

    /// <summary>
    ///   Saves the data which is necessary for the creation of an edge
    /// </summary>
    private sealed class RenderDataCache
    {
      public RenderDataCache(bool selected) {
        Selected = selected;
      }

      public bool Selected { get; }

      /// <summary>
      ///   Check if this instance is equals to another <see cref="RenderDataCache"/> object
      /// </summary>
      public bool Equals(RenderDataCache other) {
        return other.Selected == Selected;
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(RenderDataCache)) {
          return false;
        }
        return Equals((RenderDataCache) obj);
      }

      public override int GetHashCode() {
        return Selected.GetHashCode();
      }
    }


    #region Conversion between view and world coordinates

    /// <summary>
    ///   Converts the given <see cref="OrientedRectangle"/> from the world into the view coordinate space.
    /// </summary>
    internal static void WorldToIntermediateCoordinates(ICanvasContext context, OrientedRectangle rect) {
      var anchor = rect.Anchor.ToPointD();
      var anchorAndUp = anchor + rect.GetUpVector();

      var renderContext = context as IRenderContext ?? context.Lookup(typeof(IRenderContext)) as IRenderContext;
      if (renderContext != null) {
        anchor = renderContext.WorldToIntermediateCoordinates(anchor);
        anchorAndUp = renderContext.WorldToIntermediateCoordinates(anchorAndUp);
      } else {
        anchor = context.CanvasControl.WorldToIntermediateCoordinates(anchor);
        anchorAndUp = context.CanvasControl.WorldToIntermediateCoordinates(anchorAndUp);
      }

      rect.SetUpVector((anchorAndUp - anchor).Normalized);
      rect.Anchor = anchor;
      rect.Width *= context.Zoom;
      rect.Height *= context.Zoom;
    }

    /// <summary>
    ///   Converts the given rectangle from the view into the world coordinate space.
    /// </summary>
    internal static RectD IntermediateToWorldCoordinates(ICanvasContext context, RectD rect) {
      var renderContext = context as IRenderContext ?? context.Lookup(typeof(IRenderContext)) as IRenderContext;
      if (renderContext != null) {
        return IntermediateToWorldCoordinates(renderContext.CanvasControl, rect);
      }
      return IntermediateToWorldCoordinates(context.CanvasControl, rect);
    }

    /// <summary>
    ///   Converts the given rectangle from the view into the world coordinate space.
    /// </summary>
    internal static RectD IntermediateToWorldCoordinates(CanvasControl canvas, RectD rect) {
      var p1 = GetRounded(canvas.IntermediateToWorldCoordinates(rect.GetTopLeft()));
      var p2 = GetRounded(canvas.IntermediateToWorldCoordinates(rect.GetBottomRight()));
      return new RectD(p1.X, p1.Y, (int) Math.Max(0, p2.X - p1.X), (int) Math.Max(0, p2.Y - p1.Y));
    }

    private static PointD GetRounded(PointD p) {
      return new PointD(Math.Round(p.X), Math.Round(p.Y));
    }

    #endregion
  }
}
