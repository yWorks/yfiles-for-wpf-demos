/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.RectangleNodeStyle
{
  /// <summary>
  /// An <see cref="IHandle"/> for nodes with a <see cref="yWorks.Graph.Styles.RectangleNodeStyle"/> to change the
  /// <see cref="RectangleNodeStyle.CornerSize"/> interactively.
  /// </summary>
  public class CornerSizeHandle : IHandle, IPoint, IVisualCreator
  {
    private readonly INode node;
    private readonly Action cornerSizeChanged;
    private readonly yWorks.Graph.Styles.RectangleNodeStyle style;

    private double initialCornerSize;
    private double currentCornerSize;
    private ICanvasObject cornerRectCanvasObject;

    /// <summary>
    /// Creates a new instance for the given node.
    /// </summary>
    /// <param name="node">The node whose style shall be changed.</param>
    /// <param name="cornerSizeChanged">An action that is called when the handle has been moved.</param>
    public CornerSizeHandle(INode node, Action cornerSizeChanged) {
      this.node = node;
      this.cornerSizeChanged = cornerSizeChanged;
      style = node.Style as yWorks.Graph.Styles.RectangleNodeStyle;
    }

    /// <summary>
    /// Returns the absolute corner size of the current node's style.
    /// </summary>
    /// <remarks>
    /// This reflects the <see cref="RectangleNodeStyle.ScaleCornerSize"/> property of the style and clamps the
    /// size to where the style would restrict it as well.
    /// This ensures that the handle always appears where the corner ends visually.
    /// </remarks>
    internal double GetCornerSize() {
      var layout = node.Layout;
      var smallerSize = Math.Min(layout.Width, layout.Height);
      var cornerSize = style.ScaleCornerSize ? style.CornerSize * smallerSize : style.CornerSize;
      return Math.Min(GetMaximumCornerSize(), cornerSize);
    }

    /// <summary>
    /// Determines the maximum corner size based on the style's current settings.
    /// </summary>
    private double GetMaximumCornerSize() {
      // if two corners can meet, the maximum size is half the height/width instead
      double maxHeight =
        (style.Corners & Corners.Left) == Corners.Left ||
        (style.Corners & Corners.Right) == Corners.Right
          ? node.Layout.Height * 0.5
          : node.Layout.Height;
      double maxWidth =
        (style.Corners & Corners.Top) == Corners.Top ||
        (style.Corners & Corners.Bottom) == Corners.Bottom
          ? node.Layout.Width * 0.5
          : node.Layout.Width;

      return Math.Min(maxWidth, maxHeight);
    }

    /// <summary>
    /// Gets a live view of the handle location.
    /// </summary>
    /// <remarks>
    /// The handle is placed at the top-left of the node with a vertical offset of the node styles
    /// <see cref="RectangleNodeStyle.CornerSize"/>.
    /// </remarks>
    public IPoint Location { get { return this; } }

    /// <summary>
    /// Initializes the drag gesture and adds a rectangle representing the top-left corner of the node using the
    /// absolute <see cref="RectangleNodeStyle.CornerSize"/> to the view.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    public void InitializeDrag(IInputModeContext context) {
      initialCornerSize = GetCornerSize();
      currentCornerSize = initialCornerSize;
      cornerRectCanvasObject = context.CanvasControl.InputModeGroup.AddChild(this, CanvasObjectDescriptors.AlwaysDirtyInstance);
    }

    /// <summary>
    /// Calculates the new corner size depending on the new mouse location and updates the node style and corner
    /// visualization.
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
    }

    /// <summary>
    /// Sets the corner size to <see cref="RectangleNodeStyle.CornerSize"/> considering whether the
    /// <see cref="RectangleNodeStyle.ScaleCornerSize">corner size is scaled</see>.
    /// </summary>
    /// <param name="cornerSize">The new absolute corner size.</param>
    private void SetCornerSize(double cornerSize) {
      if (style.ScaleCornerSize) {
        var layout = node.Layout;
        style.CornerSize = cornerSize / Math.Min(layout.Height, layout.Width);
      } else {
        style.CornerSize = cornerSize;
      }
    }

    /// <summary>
    /// Resets the initial corner size and removes the corner visualization.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      SetCornerSize(initialCornerSize);
      cornerRectCanvasObject.Remove();
    }

    /// <summary>
    /// Sets the corner size for the new location, removes the corner visualization and triggers the cornerSizeChanged
    /// callback.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      SetCornerSize(currentCornerSize);
      cornerRectCanvasObject.Remove();
      if (cornerSizeChanged != null) {
        cornerSizeChanged();
      }
    }

    /// <summary>
    /// Returns <see cref="HandleTypes.Rotate"/> as handle type that determines the visualization of the handle.
    /// </summary>
    public HandleTypes Type { get { return HandleTypes.Rotate; } }

    /// <summary>
    /// Returns <see cref="Cursors.Cross"/> as cursor that shall be used during the drag gesture.
    /// </summary>
    public Cursor Cursor { get { return Cursors.Cross; } }

    /// <summary>
    /// Ignore clicks.
    /// </summary>
    /// <param name="eventArgs">Arguments describing the click.</param>
    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }

    #region IPoint implementation to ensure the handle position is always up-to-date

    double IPoint.X { get { return node.Layout.X; } }

    double IPoint.Y { get { return node.Layout.Y + GetCornerSize(); } }

    #endregion

    #region IVisualCreator implementation for the rectangle overlay during drag

    Visual IVisualCreator.CreateVisual(IRenderContext context) {
      var group = new VisualGroup
      {
        Transform = context.ViewTransform,
        Children = {
          new Rectangle
          {
            Stroke = Brushes.CornflowerBlue,
            StrokeThickness = 2
          }
        }
      };
      return ((IVisualCreator) this).UpdateVisual(context, group);
    }

    Visual IVisualCreator.UpdateVisual(IRenderContext context, Visual oldVisual) {
      var group = (VisualGroup) oldVisual;
      group.Transform = context.ViewTransform;
      var rectangle = (Rectangle) group.Children[0];
      var cornerSize = GetCornerSize();
      var topLeftView = context.ToViewCoordinates(node.Layout.GetTopLeft());
      var bottomRightView = context.ToViewCoordinates(node.Layout.GetTopLeft() + new PointD(cornerSize, cornerSize));
      var arrangeRect = new RectD(topLeftView, bottomRightView).GetEnlarged(1);
      rectangle.SetCanvasArrangeRect(arrangeRect);
      return group;
    }

    #endregion
  }
}
