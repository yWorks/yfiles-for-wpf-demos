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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.RectangleNodeStyle
{
  /// <summary>
  ///   An <see cref="IHandleProvider"/> for nodes using a <see cref="RectangleNodeStyle"/> that provides
  ///   a single <see cref="CornerSizeHandle"/> to change the <see cref="RectangleNodeStyle.CornerSize"/> interactively.
  /// </summary>
  internal sealed class CornerSizeHandleProvider : IHandleProvider
  {
    private readonly IHandleProvider? delegateProvider;
    private readonly INode node;

    /// <summary>
    ///   Initializes a new instance of the provider with an optional <paramref name="delegateProvider"/>
    ///   whose handles are also returned.
    /// </summary>
    /// <param name="node">The node to provide handles for.</param>
    /// <param name="delegateProvider">The wrapped <see cref="IHandleProvider"/> implementation.</param>
    public CornerSizeHandleProvider(INode node, IHandleProvider? delegateProvider = null) {
      this.node = node;
      this.delegateProvider = delegateProvider;
    }

    /// <summary>
    ///   Returns the corner size handle, as well as all handles provided by the <see cref="delegateProvider"/>.
    /// </summary>
    public IEnumerable<IHandle> GetHandles(IInputModeContext context) {
      var handles = new List<IHandle>();
      if (delegateProvider is not null) {
        handles.AddRange(delegateProvider.GetHandles(context));
      }
      if (node.Style is Graph.Styles.RectangleNodeStyle) {
        handles.Add(new CornerSizeHandle(node));
      }
      return handles;
    }

    /// <summary>
    ///   An <see cref="IHandle"/> for nodes with a <see cref="RectangleNodeStyle"/> to change the
    ///   <see cref="RectangleNodeStyle.CornerSize"/> interactively.
    /// </summary>
    private sealed class CornerSizeHandle : IHandle, IPoint, IVisualCreator
    {
      private static readonly Pen OverlayPen = CreateOverlayPen();

      private readonly INode node;
      private readonly Graph.Styles.RectangleNodeStyle style;
      private double initialCornerSize;
      private double currentCornerSize;
      private IRenderTreeElement? cornerRectRenderTreeElement;

      /// <summary>
      ///   Initializes a new instance for the given node.
      /// </summary>
      /// <param name="node">The node whose style is changed.</param>
      public CornerSizeHandle(INode node) {
        this.node = node;
        style = (Graph.Styles.RectangleNodeStyle) node.Style;
      }

      /// <summary>Returns the handle's location.</summary>
      public IPoint Location => this;

      /// <summary>This only affects the handle's visualization.</summary>
      public HandleType Type => HandleType.Move2;

      /// <summary>Returns the desired mouse pointer when interacting with the handle.</summary>
      public Cursor Cursor => Cursors.SizeNS;

      /// <summary>Gets an optional tag object associated with the handle.</summary>
      public object? Tag => null;

      /// <summary>
      ///   Initializes the drag gesture and adds a rectangle representing the top-left corner of the node
      ///   using the absolute <see cref="RectangleNodeStyle.CornerSize"/> to the view.
      /// </summary>
      /// <param name="context">The current input mode context.</param>
      public void InitializeDrag(IInputModeContext context) {
        initialCornerSize = GetCornerSize();
        currentCornerSize = initialCornerSize;
        var canvasControl = context.CanvasControl;
        cornerRectRenderTreeElement =
            canvasControl.RenderTree.CreateElement(canvasControl.RenderTree.InputModeGroup, this);
      }

      /// <summary>
      ///   Calculates the new corner size, depending on the new mouse location and updates the node
      ///   style and corner visualization.
      /// </summary>
      /// <param name="context">The current input mode context.</param>
      /// <param name="originalLocation">The original handle location.</param>
      /// <param name="newLocation">The new mouse location.</param>
      public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
        // determine delta for the corner size
        var dy = newLocation.Y - originalLocation.Y;
        // ... and clamp to valid values
        currentCornerSize = Math.Max(0, Math.Min(initialCornerSize + dy, GetMaximumCornerSize()));
        SetCornerSize(currentCornerSize);
        context.InvalidateDisplays();
      }

      /// <summary>
      ///   Sets the corner size for the new location and removes the corner visualization.
      /// </summary>
      /// <param name="context">The current input mode context.</param>
      /// <param name="originalLocation">The original handle location.</param>
      /// <param name="newLocation">The new mouse location.</param>
      public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
        SetCornerSize(currentCornerSize);
        RemoveCornerVisualization(context);
      }

      /// <summary>
      ///   Resets the initial corner size and removes the corner visualization.
      /// </summary>
      /// <param name="context">The current input mode context.</param>
      /// <param name="originalLocation">The original handle location.</param>
      public void CancelDrag(IInputModeContext context, PointD originalLocation) {
        SetCornerSize(initialCornerSize);
        context.InvalidateDisplays();
        RemoveCornerVisualization(context);
      }

      /// <summary>
      ///   Returns the absolute corner size of the current node's style.
      ///   This reflects the <see cref="RectangleNodeStyle.ScaleCornerSize"/> property of the style and clamps
      ///   the size to where the style would restrict it as well. This ensures that the handle always appears
      ///   where the corner ends visually.
      /// </summary>
      private double GetCornerSize() {
        var layout = node.Layout;
        var smallerSize = Math.Min(layout.Width, layout.Height);
        var cornerSize = style.ScaleCornerSize ? style.CornerSize * smallerSize : style.CornerSize;
        return Math.Min(GetMaximumCornerSize(), cornerSize);
      }

      /// <summary>
      ///   Sets the <see cref="RectangleNodeStyle.CornerSize"/> considering whether the
      ///   <see cref="RectangleNodeStyle.ScaleCornerSize"/> is set.
      /// </summary>
      private void SetCornerSize(double cornerSize) {
        if (style.ScaleCornerSize) {
          var layout = node.Layout;
          style.CornerSize = cornerSize / Math.Min(layout.Height, layout.Width);
        } else {
          style.CornerSize = cornerSize;
        }
      }

      /// <summary>Determines the maximum corner size based on the style's current settings.</summary>
      private double GetMaximumCornerSize() {
        var corners = style.Corners;
        var layout = node.Layout;
        // if two corners can meet, the maximum size is half the height/width instead
        var maxHeight = (corners & Corners.Left) == Corners.Left || (corners & Corners.Right) == Corners.Right
            ? layout.Height * 0.5
            : layout.Height;
        var maxWidth = (corners & Corners.Top) == Corners.Top || (corners & Corners.Bottom) == Corners.Bottom
            ? layout.Width * 0.5
            : layout.Width;
        return Math.Min(maxWidth, maxHeight);
      }

      /// <summary>This implementation does nothing special when clicked.</summary>
      public void HandleClick(ClickEventArgs eventArgs) { }

      // IPoint for the handle's location

      /// <summary>The handle's x coordinate.</summary>
      public double X => node.Layout.X;

      /// <summary>The handle's y coordinate.</summary>
      public double Y => node.Layout.Y + GetCornerSize();

      // IVisualCreator implementation for the rectangle overlay

      /// <summary>Creates the rectangle overlay during the drag operation.</summary>
      public Visual CreateVisual(IRenderContext context) {
        var group = new VisualGroup();
        group.Add(new DrawingVisual());
        return UpdateVisual(context, group)!;
      }

      /// <summary>Updates the rectangle overlay during the drag operation.</summary>
      public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) {
        if (oldVisual is not VisualGroup { Children.Count: 1 } group ||
            group.Children[0] is not DrawingVisual rectangleVisual) {
          if (oldVisual is not null) {
            context.ChildVisualRemoved(oldVisual);
          }
          return CreateVisual(context);
        }

        group.Transform = context.ViewTransform;
        var topLeftView = context.WorldToViewCoordinates(new PointD(node.Layout.X, node.Layout.Y));
        var cornerSizeView = GetCornerSize() * context.Zoom;
        using var drawingContext = rectangleVisual.RenderOpen();
        drawingContext.DrawRectangle(null, OverlayPen,
            new Rect(topLeftView.X, topLeftView.Y, cornerSizeView, cornerSizeView));
        return group;
      }

      private void RemoveCornerVisualization(IInputModeContext context) {
        if (cornerRectRenderTreeElement is not null) {
          context.CanvasControl.RenderTree.Remove(cornerRectRenderTreeElement);
          cornerRectRenderTreeElement = null;
        }
      }

      private static Pen CreateOverlayPen() {
        var pen = new Pen(Brushes.CornflowerBlue, 2);
        pen.Freeze();
        return pen;
      }
    }
  }
}
