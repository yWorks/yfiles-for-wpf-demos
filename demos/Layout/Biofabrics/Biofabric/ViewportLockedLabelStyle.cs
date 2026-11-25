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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>Specifies the viewport axis along which a label is kept visible.</summary>
  public enum LockingOrientation
  {
    /// <summary>Moves the label horizontally to keep it within the viewport.</summary>
    Horizontal,

    /// <summary>Moves the label vertically to keep it within the viewport.</summary>
    Vertical
  }

  /// <summary>
  ///   A label style which keeps its labels within the viewport as long as the edge/node it is
  ///   attached to is still in view. The style keeps track of direction, wrapped style, and padding.
  /// </summary>
  public sealed class ViewportLockedLabelStyle : DelegatingLabelStyle
  {
    /// <summary>Gets the label style used to draw the label.</summary>
    public LabelStyle WrappedStyle { get; }

    /// <summary>Gets the axis along which the label is kept within the viewport.</summary>
    public LockingOrientation Direction { get; }

    /// <summary>Gets or sets whether the label is hidden, for example when its group is collapsed.</summary>
    public bool IsHidden { get; set; }

    private readonly InsetsD padding = new(10);
    private readonly SimpleLabel standInLabel;
    private readonly OrientedRectangle dynamicLayout;

    private readonly Brush originalBrush;
    private readonly Brush transparentBrush;


    /// <summary>
    ///   Creates a new label style instance.
    /// </summary>
    /// <param name="wrappedStyle">The base style to be kept in the viewport.</param>
    /// <param name="direction">Whether the label is horizontal or vertical.</param>
    public ViewportLockedLabelStyle(LabelStyle wrappedStyle, LockingOrientation direction) {
      WrappedStyle = wrappedStyle;
      Direction = direction;
      dynamicLayout = new OrientedRectangle();
      standInLabel = new SimpleLabel(null, string.Empty, FreeLabelModel.Instance.CreateDynamic(dynamicLayout));
      originalBrush = wrappedStyle.BackgroundBrush!;
      transparentBrush = BiofabricRenderUtils.CreateTransparent(originalBrush);
    }

    /// <summary>
    ///   Gets the style's wrapped style.
    /// </summary>
    protected override ILabelStyle GetStyle(ILabel label) => WrappedStyle;

    protected override ILabel GetLabel(ILabel label) {
      standInLabel.Style = label.Style;
      standInLabel.Text = label.Text;
      standInLabel.Tag = label.Tag;
      standInLabel.Owner = label.Owner;
      return standInLabel;
    }

    protected override Visual? CreateVisual(IRenderContext context, ILabel label) {
      UpdateOffset(context, label);
      UpdateBrush(context, label);
      return base.CreateVisual(context, label);
    }

    /// <summary>
    ///   Updates the current label's visual.
    /// </summary>
    protected override Visual? UpdateVisual(IRenderContext context, Visual oldVisual, ILabel label) {
      UpdateOffset(context, label);
      UpdateBrush(context, label);
      return base.UpdateVisual(context, oldVisual, label);
    }

    /// <summary>
    ///   Calculates the offset of the label based on the viewport's position and size.
    /// </summary>
    private void UpdateOffset(ICanvasContext context, ILabel label) {
      var viewport = context.CanvasControl.Viewport.GetReduced(padding);
      var labelBounds = WrappedStyle.Renderer.GetBoundsProvider(label, WrappedStyle).GetBounds(context);
      var dx = 0d;
      var dy = 0d;
      if (Direction == LockingOrientation.Horizontal) {
        if (labelBounds.X < viewport.X) {
          dx = viewport.X - labelBounds.X;
        } else if (labelBounds.GetMaxX() > viewport.GetMaxX()) {
          dx = viewport.GetMaxX() - labelBounds.GetMaxX();
        }
      } else {
        if (labelBounds.Y < viewport.Y) {
          dy = viewport.Y - labelBounds.Y;
        } else if (labelBounds.GetMaxY() > viewport.GetMaxY()) {
          dy = viewport.GetMaxY() - labelBounds.GetMaxY();
        }
      }
      dynamicLayout.SetShape(label.GetLayout());
      dynamicLayout.X += dx;
      dynamicLayout.Y += dy;
    }

    private void UpdateBrush(IRenderContext context, ILabel label) {
      var biofabric = context.CanvasControl.Lookup<Biofabric>()!;
      if (biofabric.AnythingHighlighted && !biofabric.IsHighlighted(label.Owner)) {
        WrappedStyle.BackgroundBrush = transparentBrush;
      } else {
        WrappedStyle.BackgroundBrush = originalBrush;
      }
    }

    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      dynamicLayout.SetShape(label.GetLayout());
      return base.GetBounds(context, label);
    }

    /// <summary>
    ///   Checks whether the label is visible.
    /// </summary>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      if (IsHidden) {
        return false;
      }
      var ownerVisible = false;
      if (label.Owner is INode node) {
        ownerVisible = node.Style.Renderer.GetVisibilityTestable(node, node.Style).IsVisible(context, rectangle);
      } else if (label.Owner is IEdge edge) {
        ownerVisible = edge.Style.Renderer.GetVisibilityTestable(edge, edge.Style).IsVisible(context, rectangle);
      }
      // We are visible if the original layout is in the viewport or the original label location is visible.
      return ownerVisible || label.GetLayout().GetBounds().Intersects(rectangle);
    }
  }
}
