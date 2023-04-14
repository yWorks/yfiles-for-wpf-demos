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

namespace Demo.yFiles.Graph.ArrowNodeStyle
{
  /// <summary>
  /// An <see cref="IHandle"/> for nodes with a <see cref="yWorks.Graph.Styles.ArrowNodeStyle"/> to change the
  /// <see cref="ArrowNodeStyle.Angle"/> interactively.
  /// </summary>
  public class ArrowNodeStyleAngleHandle : IHandle, IPoint, IVisualCreator
  {
    private const double HandleOffset = 15.0;
    private readonly INode node;
    private readonly Action angleChanged;
    private readonly yWorks.Graph.Styles.ArrowNodeStyle style;

    // x and y factors that are used to translate the mouse delta to the relative handle movement
    private double xFactor;
    private double yFactor;

    private double arrowSideWidth;
    private double initialAngle;
    private double initialHandleOffset;
    private double handleOffsetToHeadLengthForPositiveAngles;
    private double handleOffsetToHeadLengthForNegativeAngles;

    // minimum and maximum handle offsets that result in the minimum and maximum allowed angles
    private double handleOffsetForMinAngle;
    private double handleOffsetForMaxAngle;

    private ICanvasObject angleLineCanvasObject;

    /// <summary>
    /// Creates a new instance for the given node.
    /// </summary>
    /// <param name="node">The node whose style shall be changed.</param>
    /// <param name="angleChanged">An action that is called when the handle has been moved.</param>
    public ArrowNodeStyleAngleHandle(INode node, Action angleChanged) {
      this.node = node;
      this.angleChanged = angleChanged;
      this.style = node.Style as yWorks.Graph.Styles.ArrowNodeStyle;
    }

    /// <summary>
    /// Gets a live view of the handle location.
    /// </summary>
    /// <remarks>
    /// The handle is placed with an offset to the node bounds on the line from the arrow head tip along the arrow blade.
    /// </remarks>
    public IPoint Location { get { return this; } }

    /// <summary>
    /// Initializes the drag gesture and adds a line from the arrow head tip along the arrow blade to the handle to the view.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    public void InitializeDrag(IInputModeContext context) {
      var nodeLayout = node.Layout;
      var isParallelogram = style.Shape == ArrowStyleShape.Parallelogram;
      var isTrapezoid = style.Shape == ArrowStyleShape.Trapezoid;

      // negative angles are only allowed for trapezoids, parallelograms or arrows with shaft ratio = 1
      var negativeAngleAllowed = style.ShaftRatio >= 1 || isTrapezoid || isParallelogram;

      arrowSideWidth = GetArrowSideWidth(node.Layout, style);

      // calculate the factors to convert the handle offset to the new length of the arrow head
      // note that for positive angles the angle rotates around the arrow tip while for negative ones it rotates around
      // a node corner
      handleOffsetToHeadLengthForPositiveAngles = arrowSideWidth / (HandleOffset + arrowSideWidth);
      handleOffsetToHeadLengthForNegativeAngles = arrowSideWidth / HandleOffset;

      initialAngle = GetClampedAngle(style);
      initialHandleOffset = GetArrowHeadLength(node.Layout, style) / (initialAngle < 0
          ? -handleOffsetToHeadLengthForNegativeAngles
          : handleOffsetToHeadLengthForPositiveAngles);

      // the maximum length of the arrow head depends on the direction and shape
      var maxHeadLength = GetMaxArrowHeadLength(nodeLayout, style);

      // calculate handle offsets for the current node size that correspond to the minimum and maximum allowed angle
      handleOffsetForMaxAngle = maxHeadLength / handleOffsetToHeadLengthForPositiveAngles;
      handleOffsetForMinAngle = negativeAngleAllowed ? -maxHeadLength / handleOffsetToHeadLengthForNegativeAngles : 0;

      // xFactor and yFactor are used later to translate the mouse delta to the relative handle movement
      var direction = style.Direction;
      xFactor = direction == ArrowNodeDirection.Left ? 1 : direction == ArrowNodeDirection.Right ? -1 : 0;
      yFactor = direction == ArrowNodeDirection.Up ? 1 : direction == ArrowNodeDirection.Down ? -1 : 0;
      if (isParallelogram) {
        // for parallelograms the slope of the arrow blade is in the opposite direction
        xFactor *= -1;
        yFactor *= -1;
      }

      // add a line from the arrow tip along the arrow blade to the handle location to the view
      // this line is created and updated in the CreateVisual and UpdateVisual methods
      angleLineCanvasObject =
          context.CanvasControl.InputModeGroup.AddChild(this, CanvasObjectDescriptors.AlwaysDirtyInstance);
    }

    /// <summary>
    /// Calculates the new angle depending on the new mouse location and updates the node style and angle visualization.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    /// <param name="newLocation">The new mouse location.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // determine delta of the handle
      var handleDelta = xFactor * (newLocation.X - originalLocation.X) + yFactor * (newLocation.Y - originalLocation.Y);

      // determine handle offset from the location that corresponds to angle = 0
      var handleOffset = initialHandleOffset + handleDelta;
      // ... and clamp to valid values
      handleOffset = Math.Max(handleOffsetForMinAngle, Math.Min(handleOffset, handleOffsetForMaxAngle));

      // calculate the new arrow head length based on the offset of the handle
      var newHeadLength = handleOffset < 0
          ? handleOffset * handleOffsetToHeadLengthForNegativeAngles
          : handleOffset * handleOffsetToHeadLengthForPositiveAngles;

      var newAngle = Math.Atan(newHeadLength / arrowSideWidth);
      style.Angle = newAngle;

      angleChanged?.Invoke();
    }

    /// <summary>
    /// Resets the initial angle and removes the angle visualization.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      style.Angle = initialAngle;
      angleLineCanvasObject.Remove();
    }

    /// <summary>
    /// Sets the angle for the new location, removes the angle visualization and triggers the angleChanged callback.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      HandleMove(context, originalLocation, newLocation);
      angleLineCanvasObject.Remove();
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

    double IPoint.X {
      get {
        switch (style.Direction) {
          case ArrowNodeDirection.Right: {
            var offset = CalculateHandleInDirectionOffset();
            return style.Shape == ArrowStyleShape.Parallelogram
                ? node.Layout.X + offset
                : node.Layout.GetMaxX() - offset;
          }
          case ArrowNodeDirection.Up:
            return node.Layout.X - HandleOffset;
          case ArrowNodeDirection.Left: {
            var offset = CalculateHandleInDirectionOffset();
            return style.Shape == ArrowStyleShape.Parallelogram
                ? node.Layout.GetMaxX() - offset
                : node.Layout.X + offset;
          }
          case ArrowNodeDirection.Down:
            return style.Shape == ArrowStyleShape.Trapezoid
                ? node.Layout.GetMaxX() + HandleOffset
                : node.Layout.X - HandleOffset;
        }
        return 0;
      }
    }

    double IPoint.Y {
      get {
        switch (style.Direction) {
          case ArrowNodeDirection.Right:
            return node.Layout.Y - HandleOffset;
          case ArrowNodeDirection.Up: {
            var offset = CalculateHandleInDirectionOffset();
            return style.Shape == ArrowStyleShape.Parallelogram
                ? node.Layout.GetMaxY() - offset
                : node.Layout.Y + offset;
          }
          case ArrowNodeDirection.Left:
            return style.Shape == ArrowStyleShape.Trapezoid
                ? node.Layout.GetMaxY() + HandleOffset
                : node.Layout.Y - HandleOffset;
          case ArrowNodeDirection.Down: {
            var offset = CalculateHandleInDirectionOffset();
            return style.Shape == ArrowStyleShape.Parallelogram
                ? node.Layout.Y + offset
                : node.Layout.GetMaxY() - offset;
          }
        }
        return 0;
      }
    }

    #endregion

    #region Utility methods

    /// <summary>
    /// Clamps the <see cref="ArrowNodeStyle.Angle"/> of the given style to a valid value.
    /// </summary>
    /// <remarks>
    /// A valid angle is less then <c>&#x03c0; / 2</c>.
    /// For styles using <see cref="ArrowStyleShape.Parallelogram"/> or <see cref="ArrowStyleShape.Trapezoid"/> shape
    /// or having <see cref="ArrowNodeStyle.ShaftRatio"/> <c>1</c>, the angle also has to be bigger then <c>-&#x03c0; / 2</c>,
    /// otherwise it has to be <c>&gt;= 0</c>
    /// </remarks>
    /// <param name="style">The style to return the clamped angle for.</param>
    /// <returns>The angle of the style clamped to a valid value.</returns>
    private static double GetClampedAngle(yWorks.Graph.Styles.ArrowNodeStyle style) {
      // clamp angle to be <= Math.PI/2
      var angle = Math.Min(Math.PI * 0.5, style.Angle);
      if (angle < 0) {
        // if a negative angle is set, check if the effective shaft ratio is 1
        if (style.ShaftRatio >= 1 || style.Shape == ArrowStyleShape.Parallelogram ||
            style.Shape == ArrowStyleShape.Trapezoid) {
          // negative angle allowed but has to be > -Math.PI/2
          angle = Math.Max(-Math.PI * 0.5, angle);
        } else {
          angle = 0;
        }
      }
      return angle;
    }

    /// <summary>
    /// Returns the width of one arrow side for the given node layout and style.
    /// </summary>
    /// <param name="nodeLayout">The node layout whose size shall be used.</param>
    /// <param name="style">The style whose shape and direction shall be used.</param>
    /// <returns>The width of one arrow side for the given node layout and style.</returns>
    private static double GetArrowSideWidth(IRectangle nodeLayout, yWorks.Graph.Styles.ArrowNodeStyle style) {
      var shape = style.Shape;
      var isParallelogram = shape == ArrowStyleShape.Parallelogram;
      var isTrapezoid = shape == ArrowStyleShape.Trapezoid;
      var againstDirectionSize = style.Direction == ArrowNodeDirection.Up || style.Direction == ArrowNodeDirection.Down
          ? nodeLayout.Width
          : nodeLayout.Height;
      // for parallelogram and trapezoid, one side of the arrow fills the full againstDirectionSize
      return againstDirectionSize * (isParallelogram || isTrapezoid ? 1 : 0.5);
    }

    /// <summary>
    /// Returns the maximum possible arrow head length for the given node layout and style.
    /// </summary>
    /// <param name="nodeLayout">The node layout whose size shall be used.</param>
    /// <param name="style">The style whose shape and direction shall be used.</param>
    /// <returns>The maximum possible arrow head length for the given node layout and style.</returns>
    private static double GetMaxArrowHeadLength(IRectangle nodeLayout, yWorks.Graph.Styles.ArrowNodeStyle style) {
      var shape = style.Shape;
      var isTrapezoid = shape == ArrowStyleShape.Trapezoid;
      var isDoubleArrow = shape == ArrowStyleShape.DoubleArrow;
      var inDirectionSize = style.Direction == ArrowNodeDirection.Up || style.Direction == ArrowNodeDirection.Down
          ? nodeLayout.Height
          : nodeLayout.Width;
      // for double arrow and trapezoid the arrow may only fill half the inDirectionSize
      var maxArrowLength = inDirectionSize * (isDoubleArrow || isTrapezoid ? 0.5 : 1);
      return maxArrowLength;
    }

    /// <summary>
    /// Calculates the length of the arrow head for the given node layout and style.
    /// </summary>
    /// <param name="nodeLayout">The layout of the node.</param>
    /// <param name="style">The style whose shape and angle shall be considered.</param>
    /// <returns>The length of the arrow head for the given style and node layout.</returns>
    public static double GetArrowHeadLength(IRectangle nodeLayout, yWorks.Graph.Styles.ArrowNodeStyle style) {
      var maxArrowLength = GetMaxArrowHeadLength(nodeLayout, style);
      var arrowSideWidth = GetArrowSideWidth(nodeLayout, style);
      var angle = GetClampedAngle(style);
      var maxHeadLength = arrowSideWidth * Math.Tan(Math.Abs(angle));

      return Math.Min(maxHeadLength, maxArrowLength);
    }

    /// <summary>
    /// Calculates the offset of the current handle location to the location corresponding to an angle of 0. 
    /// </summary>
    /// <returns>The offset of the current handle location to the location corresponding to an angle of 0.</returns>
    private double CalculateHandleInDirectionOffset() {
      var headLength = GetArrowHeadLength(node.Layout, style);
      var arrowSideWidth = GetArrowSideWidth(node.Layout, style);
      var scaledHeadLength = headLength * (HandleOffset + arrowSideWidth) / (arrowSideWidth);
      var angle = GetClampedAngle(style);
      var offset = angle >= 0 ? scaledHeadLength : (headLength - scaledHeadLength);
      return offset;
    }

    #endregion

    #region IVisualCreator implementation for the angle line overlay during drag

    Visual IVisualCreator.CreateVisual(IRenderContext context) {
      var group = new VisualGroup {
          Transform = context.ViewTransform,
          Children = { new Line() { Stroke = Brushes.Goldenrod, StrokeThickness = 2 } }
      };
      return ((IVisualCreator) this).UpdateVisual(context, group);
    }

    Visual IVisualCreator.UpdateVisual(IRenderContext context, Visual oldVisual) {
      var group = (VisualGroup) oldVisual;
      group.Transform = context.ViewTransform;

      // line shall point from handle to arrow tip
      var line = (Line) group.Children[0];

      // synchronize first line point with handle location
      var fromWorld = Location.ToPointD();
      var fromView = context.ToViewCoordinates(fromWorld);
      line.X1 = fromView.X;
      line.Y1 = fromView.Y;

      // synchronize second line point with arrow tip
      var nodeLayout = node.Layout;
      var isParallelogram = style.Shape == ArrowStyleShape.Parallelogram;
      var isTrapezoid = style.Shape == ArrowStyleShape.Trapezoid;
      var againstDirectionRatio = isParallelogram || isTrapezoid ? 1 : 0.5;

      double toWorldX = 0;
      double toWorldY = 0;
      // for negative angles, the arrow tip is moved
      var arrowTipOffset = style.Angle < 0 ? GetArrowHeadLength(node.Layout, style) : 0;
      switch (style.Direction) {
        case ArrowNodeDirection.Right: {
          toWorldX = isParallelogram ? nodeLayout.X + arrowTipOffset : nodeLayout.GetMaxX() - arrowTipOffset;
          toWorldY = nodeLayout.Y + nodeLayout.Height * againstDirectionRatio;
          break;
        }
        case ArrowNodeDirection.Left: {
          toWorldX = isParallelogram ? nodeLayout.GetMaxX() - arrowTipOffset : nodeLayout.X + arrowTipOffset;
          toWorldY = isTrapezoid ? nodeLayout.Y : nodeLayout.Y + nodeLayout.Height * againstDirectionRatio;
          break;
        }
        case ArrowNodeDirection.Up: {
          toWorldX = nodeLayout.X + nodeLayout.Width * againstDirectionRatio;
          toWorldY = isParallelogram ? nodeLayout.GetMaxY() - arrowTipOffset : nodeLayout.Y + arrowTipOffset;
          break;
        }
        case ArrowNodeDirection.Down: {
          toWorldX = isTrapezoid ? nodeLayout.X : nodeLayout.X + nodeLayout.Width * againstDirectionRatio;
          toWorldY = isParallelogram ? nodeLayout.Y + arrowTipOffset : nodeLayout.GetMaxY() - arrowTipOffset;
          break;
        }
      }

      var toView = context.ToViewCoordinates(new PointD(toWorldX, toWorldY));
      line.X2 = toView.X;
      line.Y2 = toView.Y;
      return group;
    }

    #endregion
  }
}
