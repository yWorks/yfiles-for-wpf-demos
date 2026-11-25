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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.EditablePathStyle
{
  /// <summary>
  ///   Creates a <see cref="GeneralPath"/> that describes an octagonal shape.
  /// </summary>
  internal static class EditablePathNodeStyleHelpers
  {
    internal static GeneralPath CreateDefaultPath() {
      var generalPath = new GeneralPath();
      generalPath.MoveTo(0.3, 0);
      generalPath.LineTo(0.7, 0);
      generalPath.LineTo(1, 0.3);
      generalPath.LineTo(1, 0.7);
      generalPath.LineTo(0.7, 1);
      generalPath.LineTo(0.3, 1);
      generalPath.LineTo(0, 0.7);
      generalPath.LineTo(0, 0.3);
      generalPath.Close();
      return generalPath;
    }

    /// <summary>
    ///   Normalizes the location to the node layout.
    /// </summary>
    internal static (double x, double y) Normalize(PointD location, INode node) {
      var layout = node.Layout;
      var newX = (location.X - layout.X) / layout.Width;
      var newY = (location.Y - layout.Y) / layout.Height;
      return (newX, newY);
    }

    /// <summary>
    ///   Updates the handle positions to the current path.
    /// </summary>
    internal static void UpdateHandles(INode? node, HandleInputMode handleInputMode) {
      handleInputMode.Handles.Clear();
      if (node?.Style is EditablePathNodeStyle style) {
        foreach (var handle in style.GetHandles(node)) {
          handleInputMode.Handles.Add(handle);
        }
      }
    }
  }

  /// <summary>
  ///   A custom implementation of an <see cref="INodeStyle"/> that wraps a <see cref="GeneralPathNodeStyle"/> and
  ///   adds the option to change the <see cref="GeneralPath"/>.
  /// </summary>
  public sealed class EditablePathNodeStyle : NodeStyleBase<Visual>
  {
    private GeneralPathNodeStyle pathStyle;

    public EditablePathNodeStyle(GeneralPath? path = null, Brush? brush = null, Pen? pen = null) {
      this.Path = path ?? EditablePathNodeStyleHelpers.CreateDefaultPath();
      pathStyle = new GeneralPathNodeStyle(this.Path) {
          Brush = brush,
          Pen = pen
      };
    }

    public Brush? Brush {
      get => pathStyle.Brush;
      set => pathStyle.Brush = value;
    }

    public Pen? Pen {
      get => pathStyle.Pen;
      set => pathStyle.Pen = value;
    }

    /// <summary>
    ///   Returns and specifies the path that describes the outline rendered in this <see cref="INodeStyle"/>.
    /// </summary>
    public GeneralPath Path { get; set; }

    public static IMementoSupport MementoSupport { get; } = new EditablePathNodeStyleMementoSupport();

    protected override Visual CreateVisual(IRenderContext context, INode node) {
      if (!Path.IsEquivalentTo(pathStyle.Path)) {
        // the current path differs from the path in the wrapped style
        // -> configure a copy with the new path
        pathStyle = (GeneralPathNodeStyle) pathStyle.Clone();
        pathStyle.Path = (GeneralPath) Path.Clone();
      }
      return pathStyle.Renderer.GetVisualCreator(node, pathStyle).CreateVisual(context)!;
    }

    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, INode node) {
      if (Path.IsEquivalentTo(pathStyle.Path)) {
        // the path didn't change -> update the old visual
        return pathStyle.Renderer.GetVisualCreator(node, pathStyle).UpdateVisual(context, oldVisual)!;
      }
      // create a new visual that renders the new path
      return CreateVisual(context, node);
    }

    /// <summary>
    ///   Gets the outline of the node that is the current path provided by the wrapped <see cref="GeneralPathNodeStyle"/>.
    ///   This allows for correct edge path intersection calculation, among others.
    /// </summary>
    protected override GeneralPath? GetOutline(INode node) =>
        pathStyle.Renderer.GetShapeGeometry(node, pathStyle).GetOutline();

    /// <summary>
    ///   Get the bounding box of the node which is the smallest box that contains the complete current path.
    /// </summary>
    protected override RectD GetBounds(ICanvasContext context, INode node) =>
        pathStyle.Renderer.GetBoundsProvider(node, pathStyle).GetBounds(context);

    /// <summary>
    ///   Returns whether or not the rendered node is currently visible. Nodes outside of the viewport
    ///   do not need to be rendered.
    /// </summary>
    protected override bool IsVisible(ICanvasContext context, RectD clip, INode node) =>
        pathStyle.Renderer.GetVisibilityTestable(node, pathStyle).IsVisible(context, clip);

    /// <summary>
    ///   Returns whether or not the given point is inside of the current path considering the
    ///   <see cref="IInputModeContext.HitTestRadius"/>.
    /// </summary>
    protected override bool IsHit(IInputModeContext context, PointD point, INode node) =>
        pathStyle.Renderer.GetHitTestable(node, pathStyle).IsHit(context, point);

    /// <summary>
    ///   Checks if a node is inside a certain box. Considers HitTestRadius.
    /// </summary>
    protected override bool IsInBox(IInputModeContext context, RectD box, INode node) =>
        pathStyle.Renderer.GetMarqueeTestable(node, pathStyle).IsInBox(context, box);

    /// <summary>
    ///   Returns whether or not the given point lies inside the node. This is important for
    ///   intersection calculation, among others.
    /// </summary>
    protected override bool IsInside(INode node, PointD point) =>
        pathStyle.Renderer.GetShapeGeometry(node, pathStyle).IsInside(point);

    /// <summary>
    ///   Returns the intersection point of the line from inner to outer point with the nodes shape or
    ///   <see langword="null"/> if there is no intersection.
    /// </summary>
    protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) =>
        pathStyle.Renderer.GetShapeGeometry(node, pathStyle).GetIntersection(inner, outer);

    /// <summary>
    ///   Splits the outline path at the given point and inserts a segment and bend point.
    /// </summary>
    public bool SplitPath(INode node, PointD point) {
      var (x, y) = EditablePathNodeStyleHelpers.Normalize(point, node);
      var splitPoint = new PointD(x, y);

      var stylePath = Path;
      var newPath = new GeneralPath(stylePath.Count + 2);
      var cursor = stylePath.CreateCursor();

      // checks if the split point lies on a path segment and then splits this segment
      bool Check(PointD from, PointD to) {
        var intersection = splitPoint.GetProjectionOnSegment(from, to);
        if (intersection.DistanceTo(splitPoint) < 0.1) {
          newPath.Append(stylePath, 0, cursor.Index, false);
          newPath.LineTo(splitPoint);
          newPath.Append(stylePath, cursor.Index, stylePath.Count, true);
          stylePath.Clear();
          stylePath.Append(newPath, false);
          return true;
        }
        return false;
      }

      if (stylePath.PathContains(splitPoint, 0.1)) {
        var coords = new double[6];
        var lastMove = PointD.Origin;
        var lastLocation = PointD.Origin;
        while (cursor.MoveNext()) {
          var type = cursor.GetCurrent(coords);

          switch (type) {
            case PathType.Close:
              if (Check(lastLocation, lastMove)) {
                return true;
              }
              lastLocation = lastMove;
              break;
            case PathType.LineTo:
              if (Check(lastLocation, lastLocation = cursor.CurrentEndPoint)) {
                return true;
              }
              break;
            case PathType.CubicTo:
              if (Check(lastLocation, lastLocation = cursor.CurrentEndPoint)) {
                return true;
              }
              break;
            case PathType.QuadTo:
              if (Check(lastLocation, lastLocation = cursor.CurrentEndPoint)) {
                return true;
              }
              break;
            case PathType.MoveTo:
              lastLocation = lastMove = cursor.CurrentEndPoint;
              break;
          }
        }
      }
      return false;
    }

    public override object Clone() {
      var style = (EditablePathNodeStyle) base.Clone();
      style.Path = (GeneralPath) Path.Clone();
      style.pathStyle = (GeneralPathNodeStyle) pathStyle.Clone();
      return style;
    }

    /// <summary>
    ///   Returns a set of handles consisting of a handle for each control point in the <see cref="GeneralPath"/>.
    /// </summary>
    public IEnumerable<IHandle> GetHandles(INode node) {
      var cursor = Path.CreateCursor();
      var handles = new List<IHandle>();
      var coords = new double[6];
      var index = 0;
      while (cursor.MoveNext()) {
        var type = cursor.GetCurrent(coords);
        const HandleType handleType = HandleType.Move;
        const HandleType innerHandleType = HandleType.Move2;
        switch (type) {
          case PathType.Close:
            break;
          case PathType.LineTo:
            handles.Add(new PathHandle(this, node, index++, coords[0], coords[1], handleType));
            break;
          case PathType.CubicTo:
            handles.Add(new PathHandle(this, node, index++, coords[0], coords[1], innerHandleType));
            handles.Add(new PathHandle(this, node, index++, coords[2], coords[3], innerHandleType));
            handles.Add(new PathHandle(this, node, index++, coords[4], coords[5], handleType));
            break;
          case PathType.QuadTo:
            handles.Add(new PathHandle(this, node, index++, coords[0], coords[1], innerHandleType));
            handles.Add(new PathHandle(this, node, index++, coords[2], coords[3], handleType));
            break;
          case PathType.MoveTo:
            handles.Add(new PathHandle(this, node, index++, coords[0], coords[1], handleType));
            break;
        }
      }
      return handles;
    }

    /// <summary>
    ///   Normalizes the path and adjusts the size of the given node so that when the resulting path
    ///   extends beyond the node's layout it will be fully contained, again without changing the path
    ///   visibly.
    /// </summary>
    /// <param name="node">The node to adjust its size.</param>
    /// <param name="graph">The graph to use for setting the layout of the node.</param>
    public void NormalizePath(INode node, IGraph graph) {
      var nodeBounds = Renderer.GetShapeGeometry(node, this).GetOutline()!.Flatten(0.1).GetBounds();
      var pathBounds = Path.Flatten(0.01).GetBounds();
      if (!pathBounds.IsEmpty() && (Math.Abs(pathBounds.Width - 1) > 0.01 || Math.Abs(pathBounds.Height - 1) > 0.01)) {
        Path.Transform(new Matrix2D(1 / pathBounds.Width, 0, 0, 1 / pathBounds.Height, -pathBounds.X / pathBounds.Width,
            -pathBounds.Y / pathBounds.Height));
        graph.SetNodeLayout(node, nodeBounds);
      }
    }

    private sealed record PathStyleState(GeneralPath Path, Brush? Brush, Pen? Pen);

    private sealed class EditablePathNodeStyleMementoSupport : IMementoSupport
    {
      public void ApplyState(object subject, object state) {
        var style = (EditablePathNodeStyle) subject;
        var pathStyleState = (PathStyleState) state;
        style.Path.Clear();
        style.Path.Append(pathStyleState.Path, false);
        style.Brush = pathStyleState.Brush;
        style.Pen = pathStyleState.Pen;
      }

      public object GetState(object subject) {
        var style = (EditablePathNodeStyle) subject;
        return new PathStyleState((GeneralPath) style.Path.Clone(), style.Brush, style.Pen);
      }

      public bool StateEquals(object state1, object state2) {
        var first = (PathStyleState) state1;
        var second = (PathStyleState) state2;
        return first.Path.IsEquivalentTo(second.Path) && ReferenceEquals(first.Brush, second.Brush) &&
            ReferenceEquals(first.Pen, second.Pen);
      }
    }
  }

  /// <summary>
  ///   A <see cref="IHandle"/> that allows for changing the path of a <see cref="EditablePathNodeStyle"/>.
  /// </summary>
  public sealed class PathHandle : IHandle, IPoint
  {
    private readonly INode node;
    private readonly int index;
    private double x;
    private double y;
    private readonly EditablePathNodeStyle style;
    private double origX;
    private double origY;
    private GeneralPath? copy;
    private ICompoundEdit? edit;

    public PathHandle(EditablePathNodeStyle style, INode node, int index, double x, double y, HandleType type) {
      this.x = x;
      this.y = y;
      this.Type = type;
      this.node = node;
      this.style = style;
      this.index = index;
    }

    /// <summary>
    ///   Removes the control point associated with this handle from the <see cref="EditablePathNodeStyle"/>'s path.
    /// </summary>
    public void RemoveSegment() {
      var path = style.Path;
      var pathCopy = (GeneralPath) path.Clone();
      path.Clear();
      var cursor = pathCopy.CreateCursor();

      var coords = new double[6];
      var index = 0;
      var hadMoveTo = false;
      while (cursor.MoveNext()) {
        var type = cursor.GetCurrent(coords);
        switch (type) {
          case PathType.Close:
            path.Close();
            break;
          case PathType.LineTo:
            if (index++ != this.index) {
              if (hadMoveTo) {
                path.LineTo(coords[0], coords[1]);
              } else {
                path.MoveTo(coords[0], coords[1]);
              }
              hadMoveTo = true;
            }
            break;
          case PathType.CubicTo:
            if (index++ == this.index) {
              if (hadMoveTo) {
                path.QuadTo(coords[2], coords[3], coords[4], coords[5]);
              } else {
                path.MoveTo(coords[2], coords[3]);
                path.LineTo(coords[4], coords[5]);
                hadMoveTo = true;
              }
            } else if (index++ == this.index) {
              if (hadMoveTo) {
                path.QuadTo(coords[0], coords[1], coords[4], coords[5]);
              } else {
                path.MoveTo(coords[0], coords[1]);
                path.LineTo(coords[4], coords[5]);
                hadMoveTo = true;
              }
            } else if (index++ == this.index) {
              path.QuadTo(coords[0], coords[1], coords[4], coords[5]);
            } else {
              path.CubicTo(coords[0], coords[1], coords[2], coords[3], coords[4], coords[5]);
            }
            break;
          case PathType.QuadTo:
            if (index++ == this.index) {
              if (hadMoveTo) {
                path.LineTo(coords[2], coords[3]);
              } else {
                path.MoveTo(coords[2], coords[3]);
                hadMoveTo = true;
              }
            } else if (index++ == this.index) {
              path.LineTo(coords[2], coords[3]);
            } else {
              path.QuadTo(coords[0], coords[1], coords[2], coords[3]);
            }
            break;
          case PathType.MoveTo:
            if (index++ != this.index) {
              path.MoveTo(coords[0], coords[1]);
              hadMoveTo = true;
            }
            break;
        }
      }
    }

    /// <summary>
    ///   Updates the coordinates of the control point in the <see cref="EditablePathNodeStyle"/>'s path associated
    ///   with this handle.
    /// </summary>
    private void UpdateXY() {
      var path = style.Path;
      var cursor = path.CreateCursor();
      var coords = new double[6];
      var index = 0;
      while (cursor.MoveNext()) {
        var type = cursor.GetCurrent(coords);
        switch (type) {
          case PathType.Close:
            path.Close();
            break;
          case PathType.LineTo:
            if (index++ == this.index) {
              x = coords[0];
              y = coords[1];
              return;
            }
            break;
          case PathType.CubicTo:
            if (index++ == this.index) {
              x = coords[0];
              y = coords[1];
              return;
            }
            if (index++ == this.index) {
              x = coords[2];
              y = coords[3];
              return;
            }
            if (index++ == this.index) {
              x = coords[4];
              y = coords[5];
              return;
            }
            break;
          case PathType.QuadTo:
            if (index++ == this.index) {
              x = coords[0];
              y = coords[1];
              return;
            }
            if (index++ == this.index) {
              x = coords[2];
              y = coords[3];
              return;
            }
            break;
          case PathType.MoveTo:
            if (index++ == this.index) {
              x = coords[0];
              y = coords[1];
              return;
            }
            break;
        }
      }
    }

    public double X {
      get {
        var layout = node.Layout;
        return x * layout.Width + layout.X;
      }
    }

    public double Y {
      get {
        var layout = node.Layout;
        return y * layout.Height + layout.Y;
      }
    }

    public HandleType Type { get; }

    public Cursor Cursor => Cursors.Hand;

    public IPoint Location => this;

    public object? Tag => null;

    public void InitializeDrag(IInputModeContext context) {
      copy = (GeneralPath) style.Path.Clone();
      origX = x;
      origY = y;

      // this might resize the node and change the paths so make sure that this gets recorded by the undo engine
      var graph = context.GetGraph()!;
      edit = graph.BeginEdit("Change Path", "Change Path", new object[] { node.Style, node },
          item => item is INode modelItem
              ? modelItem.Lookup(typeof(IMementoSupport)) as IMementoSupport
              : EditablePathNodeStyle.MementoSupport);
    }

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var (newX, newY) = EditablePathNodeStyleHelpers.Normalize(newLocation, node);

      // create a copy of the path to be able to reset the path when the drag is canceled
      var path = style.Path;
      var pathCopy = (GeneralPath) path.Clone();
      path.Clear();
      var cursor = pathCopy.CreateCursor();

      // update the handle position and adjust the path
      var coords = new double[6];
      var index = 0;
      while (cursor.MoveNext()) {
        var type = cursor.GetCurrent(coords);
        switch (type) {
          case PathType.Close:
            path.Close();
            break;
          case PathType.LineTo:
            if (index++ == this.index) {
              path.LineTo(x = newX, y = newY);
            } else {
              path.LineTo(coords[0], coords[1]);
            }
            break;
          case PathType.CubicTo:
            if (index++ == this.index) {
              path.CubicTo(x = newX, y = newY, coords[2], coords[3], coords[4], coords[5]);
            } else if (index++ == this.index) {
              path.CubicTo(coords[0], coords[1], x = newX, y = newY, coords[4], coords[5]);
            } else if (index++ == this.index) {
              path.CubicTo(coords[0], coords[1], coords[2], coords[3], x = newX, y = newY);
            } else {
              path.CubicTo(coords[0], coords[1], coords[2], coords[3], coords[4], coords[5]);
            }
            break;
          case PathType.QuadTo:
            if (index++ == this.index) {
              path.QuadTo(x = newX, y = newY, coords[2], coords[3]);
            } else if (index++ == this.index) {
              path.QuadTo(coords[0], coords[1], x = newX, y = newY);
            } else {
              path.QuadTo(coords[0], coords[1], coords[2], coords[3]);
            }
            break;
          case PathType.MoveTo:
            if (index++ == this.index) {
              path.MoveTo(x = newX, y = newY);
            } else {
              path.MoveTo(coords[0], coords[1]);
            }
            break;
        }
      }
    }

    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      style.Path.Clear();
      x = origX;
      y = origY;
      style.Path.Append(copy!, false);

      edit!.Cancel();
      edit = null;
    }

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      HandleMove(context, originalLocation, newLocation);
      var inputMode = context.InputMode as HandleInputMode ??
          throw new InvalidOperationException("The handle input mode is not installed.");

      void FinishHandler(object? sender, InputModeEventArgs args) {
        inputMode.DragFinished -= FinishHandler;

        // adjust node layout and update the path with the final handle coordinates
        style.NormalizePath(node, context.GetGraph()!);
        UpdateXY();
        EditablePathNodeStyleHelpers.UpdateHandles(node, inputMode);

        edit!.Commit();
        edit = null;
      }

      inputMode.DragFinished += FinishHandler;
    }

    /// <summary>
    ///   This implementation does nothing special when clicked.
    /// </summary>
    public void HandleClick(ClickEventArgs eventArgs) { }
  }
}
