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

using System.Linq;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomDragAndDrop
{
  /// <summary>
  ///   A <see cref="DropInputMode"/> specialized to drag and drop colors onto <see cref="INode"/>s and
  ///   <see cref="IEdge"/>s to change their current color.
  /// </summary>
  public sealed class ColorDropInputMode : DropInputMode
  {
    /// <summary>
    ///   Constructs a new instance of class <see cref="ColorDropInputMode"/>.
    /// </summary>
    public ColorDropInputMode() : base(typeof(Color)) { }

    /// <summary>
    ///   Gets the currently dragged color from the drop data.
    /// </summary>
    public Color? DraggedColor => DropData is Color color ? color : null;

    /// <summary>
    ///   Gets the drop target.
    /// </summary>
    /// <returns>The node or edge at the mouse location, or null.</returns>
    private IModelItem? DropTarget { get; set; }

    /// <summary>
    ///   Sets the drop target.
    /// </summary>
    private void SetDropTarget(IModelItem? value) {
      if (ReferenceEquals(DropTarget, value)) {
        return;
      }
      if (DropTarget is { } oldTarget) {
        // Remove the old highlight, if present
        HighlightIndicatorManager?.Items.Remove(oldTarget);
      }
      DropTarget = value;
    }

    protected override void OnDraggedOver(InputModeEventArgs eventArgs) {
      base.OnDraggedOver(eventArgs);
      UpdateDropTarget(new PointD(PointerPosition.X, PointerPosition.Y));
    }

    protected override void OnDragLeft(InputModeEventArgs eventArgs) {
      base.OnDragLeft(eventArgs);
      SetDropTarget(null);
    }

    protected override void OnDragDropped(InputModeEventArgs eventArgs) {
      base.OnDragDropped(eventArgs);
      if (DropTarget is { } target && DraggedColor is { } color) {
        ApplyColor(eventArgs.Context.GetGraph()!, target, color);
      }
      SetDropTarget(null);
    }

    /// <summary>
    ///   Updates the drop target at the given location.
    /// </summary>
    private void UpdateDropTarget(PointD dragLocation) {
      var context = ParentInputModeContext;
      var color = DraggedColor;

      if (context is not null && color is { } draggedColor) {
        var item = FindEdgeOrNode(context, dragLocation);
        if (item is not null && Accept(item, draggedColor)) {
          SetDropTarget(item);
          return;
        }
      }
      SetDropTarget(null);
    }

    /// <summary>
    ///   Finds the first edge or node that contains the given location.
    /// </summary>
    private IModelItem? FindEdgeOrNode(IInputModeContext context, PointD location) {
      var hitTester = context.Lookup(typeof(IHitTester)) as IHitTester;
      // hit testing needs to be done with a context whose parent input mode is this mode,
      // because hit-testables may behave differently depending on context
      // this is e.g., the case for GroupNodeStyle
      return hitTester?.EnumerateHits(new InputModeContext(context, this), location).FirstOrDefault(IsEdgeOrNode);
    }

    /// <summary>
    ///   Checks whether the color of the specified item can be changed.
    /// </summary>
    private static bool Accept(IModelItem item, Color color) {
      if (item is INode { Style: ShapeNodeStyle style }) {
        return style.Brush is not SolidColorBrush current || current.Color != color;
      }

      if (item is IEdge { Style: PolylineEdgeStyle { Pen: { } pen } }) {
        return pen.Brush is null || pen.Brush is not SolidColorBrush current || current.Color != color;
      }

      return false;
    }

    /// <summary>
    ///   Changes the color of the given node or edge to the given color.
    /// </summary>
    private void ApplyColor(IGraph targetGraph, IModelItem item, Color color) {
      if (item is INode { Style: ShapeNodeStyle nodeStyle }) {
        ApplyNodeColor(targetGraph, nodeStyle, color);
      }

      if (item is IEdge { Style: PolylineEdgeStyle { Pen: { } pen } } edge &&
          edge.Style is PolylineEdgeStyle edgeStyle) {
        ApplyEdgeColor(targetGraph, edgeStyle, pen, color);
      }
    }

    /// <summary>
    ///   Changes the color of the given node to the given color.
    /// </summary>
    private static void ApplyNodeColor(IGraph graph, ShapeNodeStyle style, Color color) {
      var oldBrush = style.Brush;
      var newBrush = new SolidColorBrush(color);
      graph.AddUndoUnit("Change Node Color", "Change Node Color", () => style.Brush = oldBrush,
          () => style.Brush = newBrush);
      style.Brush = newBrush;
    }

    /// <summary>
    ///   Changes the color of the given edge to the given color.
    /// </summary>
    private static void ApplyEdgeColor(IGraph graph, PolylineEdgeStyle style, Pen oldPen, Color color) {
      var newPen = oldPen.CloneCurrentValue();
      newPen.Brush = new SolidColorBrush(color);
      graph.AddUndoUnit("Change Edge Color", "Change Edge Color", () => style.Pen = oldPen, () => style.Pen = newPen);
      style.Pen = newPen;
    }

    /// <summary>
    ///   Sets the drop effect to Copy if the current drop target is valid or to None otherwise.
    ///   Depending on the current drop effect, WPF displays the corresponding copy or no-drop cursor.
    /// </summary>
    protected override void AdjustEffect(DragEventArgs evt) {
      base.AdjustEffect(evt);

      var dropAllowed = evt.Effects != DragDropEffects.None && DropTarget is not null;
      evt.Effects = dropAllowed ? DragDropEffects.Copy : DragDropEffects.None;
      // Highlight the target item if dropping is allowed
      if (dropAllowed && DropTarget is { } target) {
        HighlightIndicatorManager?.Items.Add(target);
      }
    }

    /// <summary>
    ///   Determines if the given item is an <see cref="IEdge"/> or an <see cref="INode"/> instance.
    /// </summary>
    private static bool IsEdgeOrNode(IModelItem item) => item is IEdge or INode;

    private HighlightIndicatorManager<IModelItem>? HighlightIndicatorManager =>
        ParentInputModeContext?.Lookup(typeof(HighlightIndicatorManager<IModelItem>)) as
            HighlightIndicatorManager<IModelItem>;
  }
}
