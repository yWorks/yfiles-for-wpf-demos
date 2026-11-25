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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomHandleProvider
{
  /// <summary>
  ///   An <see cref="IHandle"/> for nodes with an <see cref="ArrowNodeStyle"/> to change the
  ///   <see cref="ArrowNodeStyle.Angle"/> interactively.
  /// </summary>
  internal sealed class ArrowNodeStyleAngleHandle : IHandle, IPoint, IVisualCreator
  {
    private readonly GraphControl graphControl;
    private readonly Action angleChanged;
    private readonly INode node;
    private readonly double handleOffset = 15;
    private readonly ArrowNodeStyle style;

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
    private IRenderTreeElement? angleLineRenderTreeElement;

    /// <summary>Creates a new instance for the given node.</summary>
    /// <param name="graphControl">The graph control whose render tree visualizes the angle.</param>
    /// <param name="node">The node whose style shall be changed.</param>
    /// <param name="angleChanged">An action that is called when the handle has been moved.</param>
    public ArrowNodeStyleAngleHandle(GraphControl graphControl, INode node, Action angleChanged) {
      this.graphControl = graphControl;
      this.node = node;
      this.angleChanged = angleChanged;
      style = (ArrowNodeStyle) node.Style;
    }

    /// <summary>
    ///   Gets a live view of the handle location.
    ///   The handle is placed with an offset to the node bounds on the line from the arrow head tip
    ///   along the arrow blade.
    /// </summary>
    public IPoint Location => this;

    /// <summary>
    ///   Initializes the drag gesture and adds a line from the arrow head tip along the arrow blade to
    ///   the handle to the view.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    public void InitializeDrag(IInputModeContext context) {
      var layout = node.Layout;
      var isParallelogram = style.Shape == ArrowStyleShape.Parallelogram;
      var isTrapezoid = style.Shape == ArrowStyleShape.Trapezoid;
      // negative angles are only allowed for trapezoids, parallelograms or arrows with shaft ratio = 1
      var negativeAngleAllowed = style.ShaftRatio >= 1 || isTrapezoid || isParallelogram;
      arrowSideWidth = GetArrowSideWidth(layout, style);
      // Calculate conversion factors from handle offset to arrowhead length. For positive angles
      // the angle rotates around the arrow tip; for negative ones it rotates around a node corner.
      handleOffsetToHeadLengthForPositiveAngles = arrowSideWidth / (handleOffset + arrowSideWidth);
      handleOffsetToHeadLengthForNegativeAngles = arrowSideWidth / handleOffset;
      initialAngle = GetClampedAngle(style);
      initialHandleOffset = GetArrowHeadLength(layout, style) / (initialAngle < 0
          ? -handleOffsetToHeadLengthForNegativeAngles
          : handleOffsetToHeadLengthForPositiveAngles);

      // The maximum length of the arrow head depends on the direction and shape.
      var maxHeadLength = GetMaxArrowHeadLength(layout, style);
      // Calculate the handle offsets for this node size that correspond to the minimum and maximum allowed angles.
      handleOffsetForMaxAngle = maxHeadLength / handleOffsetToHeadLengthForPositiveAngles;
      handleOffsetForMinAngle = negativeAngleAllowed ? -maxHeadLength / handleOffsetToHeadLengthForNegativeAngles : 0;

      // xFactor and yFactor are used later to translate the mouse delta to the relative handle movement.
      xFactor = style.Direction == ArrowNodeDirection.Left ? 1 : style.Direction == ArrowNodeDirection.Right ? -1 : 0;
      yFactor = style.Direction == ArrowNodeDirection.Up ? 1 : style.Direction == ArrowNodeDirection.Down ? -1 : 0;
      if (isParallelogram) {
        // For parallelograms the slope of the arrow blade is in the opposite direction.
        xFactor *= -1;
        yFactor *= -1;
      }

      // Add a line from the arrow tip along the arrow blade to the handle location to the view.
      // This line is created and updated in the CreateVisual and UpdateVisual methods.
      angleLineRenderTreeElement = graphControl.RenderTree.CreateElement(graphControl.RenderTree.InputModeGroup, this);
    }

    /// <summary>
    ///   Calculates the new angle depending on the new mouse location and updates the node style and
    ///   angle visualization.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    /// <param name="newLocation">The new mouse location.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // Determine delta of the handle.
      var handleDelta = xFactor * (newLocation.X - originalLocation.X) + yFactor * (newLocation.Y - originalLocation.Y);
      // Determine handle offset from the location that corresponds to angle = 0.
      // Clamp it to the valid range.
      var offset = Math.Clamp(initialHandleOffset + handleDelta, handleOffsetForMinAngle, handleOffsetForMaxAngle);
      // Calculate the new arrow head length based on the offset of the handle.
      var newHeadLength = offset < 0
          ? offset * handleOffsetToHeadLengthForNegativeAngles
          : offset * handleOffsetToHeadLengthForPositiveAngles;
      style.Angle = Math.Atan(newHeadLength / arrowSideWidth);
      angleChanged();
    }

    /// <summary>Resets the initial angle and removes the angle visualization.</summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      style.Angle = initialAngle;
      RemoveAngleLine();
    }

    /// <summary>Sets the angle at the new location, removes the angle visualization, and triggers the callback.</summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    /// <param name="newLocation">The new mouse location.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      HandleMove(context, originalLocation, newLocation);
      RemoveAngleLine();
    }

    private void RemoveAngleLine() {
      if (angleLineRenderTreeElement != null) {
        graphControl.RenderTree.Remove(angleLineRenderTreeElement);
        angleLineRenderTreeElement = null;
      }
    }

    /// <summary>
    ///   Returns <see cref="HandleType.Custom3"/> as handle type that determines the visualization of
    ///   the handle.
    /// </summary>
    public HandleType Type => HandleType.Custom3;

    /// <summary>Returns an optional tag object associated with the handle.</summary>
    public object? Tag => null;

    /// <summary>Returns a crosshair cursor that shall be used during the drag gesture.</summary>
    public Cursor Cursor =>
        style.Direction is ArrowNodeDirection.Left or ArrowNodeDirection.Right ? Cursors.SizeWE : Cursors.SizeNS;

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) { }

    /// <summary>The handle's x coordinate.</summary>
    public double X => GetLocation().X;

    /// <summary>The handle's y coordinate.</summary>
    public double Y => GetLocation().Y;

    private PointD GetLocation() {
      var layout = node.Layout;
      switch (style.Direction) {
        case ArrowNodeDirection.Right: {
          var offset = CalculateHandleInDirectionOffset();
          return new PointD(
              style.Shape == ArrowStyleShape.Parallelogram ? layout.X + offset : layout.GetMaxX() - offset,
              layout.Y - handleOffset);
        }
        case ArrowNodeDirection.Up: {
          var offset = CalculateHandleInDirectionOffset();
          return new PointD(layout.X - handleOffset,
              style.Shape == ArrowStyleShape.Parallelogram ? layout.GetMaxY() - offset : layout.Y + offset);
        }
        case ArrowNodeDirection.Left: {
          var offset = CalculateHandleInDirectionOffset();
          return new PointD(
              style.Shape == ArrowStyleShape.Parallelogram ? layout.GetMaxX() - offset : layout.X + offset,
              style.Shape == ArrowStyleShape.Trapezoid ? layout.GetMaxY() + handleOffset : layout.Y - handleOffset);
        }
        case ArrowNodeDirection.Down: {
          var offset = CalculateHandleInDirectionOffset();
          return new PointD(
              style.Shape == ArrowStyleShape.Trapezoid ? layout.GetMaxX() + handleOffset : layout.X - handleOffset,
              style.Shape == ArrowStyleShape.Parallelogram ? layout.Y + offset : layout.GetMaxY() - offset);
        }
        default:
          return PointD.Origin;
      }
    }

    /// <summary>
    ///   Clamps the <see cref="ArrowNodeStyle.Angle"/> of the given style to a valid value. A valid
    ///   angle is less than π / 2. For parallelograms, trapezoids, or styles with a shaft ratio of 1,
    ///   the angle also has to be greater than -π / 2; otherwise it has to be non-negative.
    /// </summary>
    /// <param name="style">The style to return the clamped angle for.</param>
    /// <returns>The angle of the style clamped to a valid value.</returns>
    private static double GetClampedAngle(ArrowNodeStyle style) {
      // Clamp angle to be <= Math.PI/2.
      var angle = Math.Min(Math.PI * 0.5, style.Angle);
      if (angle < 0) {
        // If a negative angle is set, check if the effective shaft ratio is 1.
        if (style.ShaftRatio >= 1 || style.Shape is ArrowStyleShape.Parallelogram or ArrowStyleShape.Trapezoid) {
          // Negative angle is allowed but has to be > -Math.PI/2.
          angle = Math.Max(-Math.PI * 0.5, angle);
        } else {
          angle = 0;
        }
      }
      return angle;
    }

    /// <summary>Returns the width of one arrow side for the given node layout and style.</summary>
    /// <param name="layout">The node layout whose size shall be used.</param>
    /// <param name="style">The style whose shape and direction shall be used.</param>
    /// <returns>The width of one arrow side for the given node layout and style.</returns>
    private static double GetArrowSideWidth(IRectangle layout, ArrowNodeStyle style) {
      var isParallelogram = style.Shape == ArrowStyleShape.Parallelogram;
      var isTrapezoid = style.Shape == ArrowStyleShape.Trapezoid;
      var size = style.Direction is ArrowNodeDirection.Up or ArrowNodeDirection.Down ? layout.Width : layout.Height;
      // For parallelogram and trapezoid, one side of the arrow fills the full against-direction size;
      // for other shapes, it fills half of that size.
      return size * (isParallelogram || isTrapezoid ? 1 : 0.5);
    }

    /// <summary>Returns the maximum possible arrow head length for the given node layout and style.</summary>
    /// <param name="layout">The node layout whose size shall be used.</param>
    /// <param name="style">The style whose shape and direction shall be used.</param>
    /// <returns>The maximum possible arrow head length for the given node layout and style.</returns>
    private static double GetMaxArrowHeadLength(IRectangle layout, ArrowNodeStyle style) {
      var limited = style.Shape is ArrowStyleShape.DoubleArrow or ArrowStyleShape.Trapezoid;
      var size = style.Direction is ArrowNodeDirection.Up or ArrowNodeDirection.Down ? layout.Height : layout.Width;
      // For double-arrow and trapezoid the arrow may only fill half the in-direction size.
      return size * (limited ? 0.5 : 1);
    }

    /// <summary>Calculates the length of the arrow head for the given node layout and style.</summary>
    /// <param name="layout">The layout of the node.</param>
    /// <param name="style">The style whose shape and angle shall be considered.</param>
    /// <returns>The length of the arrow head for the given style and node layout.</returns>
    internal static double GetArrowHeadLength(IRectangle layout, ArrowNodeStyle style) {
      var maxArrowLength = GetMaxArrowHeadLength(layout, style);
      var sideWidth = GetArrowSideWidth(layout, style);
      var angle = GetClampedAngle(style);
      // Limit the arrow head length to the maximum allowed by the node size and shape.
      var maxHeadLength = sideWidth * Math.Tan(Math.Abs(angle));
      return Math.Min(maxHeadLength, maxArrowLength);
    }

    /// <summary>
    ///   Calculates the offset of the current handle location to the location corresponding to an
    ///   angle of 0.
    /// </summary>
    private double CalculateHandleInDirectionOffset() {
      var headLength = GetArrowHeadLength(node.Layout, style);
      var sideWidth = GetArrowSideWidth(node.Layout, style);
      // Scale the head length to account for the handle offset.
      var scaledHeadLength = headLength * (handleOffset + sideWidth) / sideWidth;
      return GetClampedAngle(style) >= 0 ? scaledHeadLength : headLength - scaledHeadLength;
    }

    /// <summary>Creates the line that visualizes the angle during the drag.</summary>
    public Visual CreateVisual(IRenderContext context) => CreateLineVisual(context);

    /// <summary>Updates the line that visualizes the angle during the drag.</summary>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateLineVisual(context);

    private DrawingVisual CreateLineVisual(IRenderContext context) {
      var visual = new DrawingVisual();
      using var drawing = visual.RenderOpen();
      // Get the first line point with the handle location.
      var from = GetLocation().ToPoint();
      // Get the second line point with the arrow tip.
      var to = GetArrowTip().ToPoint();
      // Draw the line from the handle to the arrow tip.
      drawing.DrawLine(new Pen(Brushes.Goldenrod, 2), from, to);
      return visual;
    }

    /// <summary>Calculates the arrow tip location used as the end of the angle visualization.</summary>
    private PointD GetArrowTip() {
      var layout = node.Layout;
      var isParallelogram = style.Shape == ArrowStyleShape.Parallelogram;
      var isTrapezoid = style.Shape == ArrowStyleShape.Trapezoid;
      var againstDirectionRatio = isParallelogram || isTrapezoid ? 1 : 0.5;
      // For negative angles, the arrow tip is moved.
      var arrowTipOffset = style.Angle < 0 ? GetArrowHeadLength(layout, style) : 0;
      return style.Direction switch {
          ArrowNodeDirection.Right => new PointD(
              isParallelogram ? layout.X + arrowTipOffset : layout.GetMaxX() - arrowTipOffset,
              layout.Y + layout.Height * againstDirectionRatio),
          ArrowNodeDirection.Left => new PointD(
              isParallelogram ? layout.GetMaxX() - arrowTipOffset : layout.X + arrowTipOffset,
              isTrapezoid ? layout.Y : layout.Y + layout.Height * againstDirectionRatio),
          ArrowNodeDirection.Up => new PointD(layout.X + layout.Width * againstDirectionRatio,
              isParallelogram ? layout.GetMaxY() - arrowTipOffset : layout.Y + arrowTipOffset),
          ArrowNodeDirection.Down => new PointD(
              isTrapezoid ? layout.X : layout.X + layout.Width * againstDirectionRatio,
              isParallelogram ? layout.Y + arrowTipOffset : layout.GetMaxY() - arrowTipOffset),
          _ => PointD.Origin
      };
    }
  }
}
