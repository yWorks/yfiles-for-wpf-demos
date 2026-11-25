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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.BezierEdgeStyle
{
  /// <summary>
  ///   Handle for an outer control point bend of a Bézier curve. The outer control point bends are not located on the
  ///   path, but determine the slope of the segments. If the control point triple is currently collinear, this
  ///   implementation moves the other outer control point of the triple so that this invariant is kept.
  /// </summary>
  public sealed class OuterControlPointHandle : IHandle
  {
    /// <summary>The core bend handle that performs the actual bend movement.</summary>
    private readonly IHandle coreHandle;

    /// <summary>The original location of this handle.</summary>
    private PointD origin;

    /// <summary>The other handle that is controlled indirectly from this handle.</summary>
    private IHandle? slaveHandle;

    /// <summary>The original location of the slave handle, required to move it.</summary>
    private PointD slaveOrigin;

    /// <summary>The location of the middle bend, used as the axis of rotation to keep the triple collinear.</summary>
    private PointD middleLocation;

    /// <summary>The bend that belongs to this handle instance.</summary>
    public IBend Bend { get; }

    /// <summary>Creates a new instance that wraps the original core handle for the given bend.</summary>
    public OuterControlPointHandle(IHandle coreHandle, IBend bend) {
      this.coreHandle = coreHandle;
      Bend = bend;
      slaveOrigin = new PointD(0, 0);
      middleLocation = new PointD(0, 0);
    }

    /// <summary>Initializes our drag and determines whether this handle controls another handle.</summary>
    public void InitializeDrag(IInputModeContext context) {
      origin = Bend.Location.ToPointD();
      coreHandle.InitializeDrag(context);
      if (context.InputMode is MoveInputMode) {
        // If we are moved via MoveInputMode (happens when the whole edge is dragged), delegate only to the core handle.
        return;
      }

      // If we are indirectly controlled from the other control point or the bend curve point, those implementations
      // put a marker in the lookup. If such a marker is present, we do not delegate to the other handle.
      var innerHandle = context.Lookup<InnerControlPointHandle>();
      var outerHandle = context.Lookup<OuterControlPointHandle>();
      if (innerHandle != null || outerHandle != null) {
        return;
      }

      // We are the master handle so we control the other one.
      var index = Bend.GetIndex();
      var isFirstInTriplet = index % 3 == 1;
      IBend? otherBend = null;
      IBend? middleBend = null;
      var allBends = Bend.Owner.Bends;
      if (isFirstInTriplet && index < allBends.Count - 1) {
        // We are the first of the triple and there is a potential slave handle.
        // Get the slave and the middle bend.
        otherBend = allBends[index + 2];
        middleBend = allBends[index + 1];
      } else if (index >= 3) {
        // We are the last of the triple and there is a potential slave handle.
        // Get the slave and the middle bend.
        otherBend = allBends[index - 2];
        middleBend = allBends[index - 1];
      }
      if (otherBend != null && middleBend != null &&
          BezierHandleUtil.AreCollinear(Bend.Location, middleBend.Location, otherBend.Location)) {
        slaveHandle = otherBend.Lookup<IHandle>();
        middleLocation = middleBend.Location.ToPointD();
      }
      if (slaveHandle == null) {
        return;
      }

      var childContext = new InputModeContext(context);
      childContext.GetDecoratorFor<OuterControlPointHandle>().AddConstant(this);
      slaveHandle.InitializeDrag(childContext);
      slaveOrigin = slaveHandle.Location.ToPointD();
    }

    /// <summary>Moves this handle and, if present, its slave handle while keeping the triple collinear.</summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      coreHandle.HandleMove(context, originalLocation, newLocation);
      if (slaveHandle == null) {
        return;
      }

      var delta = newLocation - middleLocation;
      var otherDistance = (slaveOrigin - middleLocation).VectorLength;
      var direction = delta.VectorLength == 0 ? PointD.Origin : delta.Normalized;
      slaveHandle.HandleMove(context, slaveOrigin, middleLocation - direction * otherDistance);
    }

    /// <summary>Cancels the movement on the core handle and, if present, on the slave handle.</summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      coreHandle.CancelDrag(context, origin);
      if (slaveHandle == null) {
        return;
      }
      // We can use the original context since all decisions were already made in InitializeDrag.
      slaveHandle.CancelDrag(context, slaveOrigin);
      slaveHandle = null;
    }

    /// <summary>Finishes the movement on the core handle and, if present, on the slave handle.</summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      coreHandle.DragFinished(context, originalLocation, newLocation);
      if (slaveHandle == null) {
        return;
      }
      var delta = newLocation - middleLocation;
      var otherDistance = (slaveOrigin - middleLocation).VectorLength;
      var direction = delta.VectorLength == 0 ? PointD.Origin : delta.Normalized;
      slaveHandle.DragFinished(context, slaveOrigin, middleLocation - direction * otherDistance);
      CleanUp();
    }

    /// <summary>Cleans up work at the end of the gesture, either by canceling or finishing.</summary>
    private void CleanUp() => slaveHandle = null;

    /// <summary>Uses a slightly different visualization.</summary>
    public HandleType Type => HandleType.Move2;

    /// <summary>Uses the core handle's cursor.</summary>
    public Cursor Cursor => coreHandle.Cursor;

    /// <summary>Uses the core handle's location.</summary>
    public IPoint Location => coreHandle.Location;

    public object? Tag => null;

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) { }
  }

  /// <summary>
  ///   Handle for an inner control point bend of a Bézier curve. The inner control point bends are located on the path.
  ///   If the control point triple is currently collinear, this implementation moves the outer control points so that
  ///   this invariant is kept.
  /// </summary>
  public sealed class InnerControlPointHandle : IHandle
  {
    /// <summary>The core bend handle that performs the actual bend movement.</summary>
    private readonly IHandle coreHandle;

    /// <summary>The bend that belongs to this handle instance.</summary>
    private readonly IBend bend;

    /// <summary>The original location of this handle.</summary>
    private PointD origin;

    /// <summary>The first (slave) handle in a control point triple.</summary>
    private IHandle? firstSlaveHandle;

    /// <summary>The last (slave) handle in a control point triple.</summary>
    private IHandle? lastSlaveHandle;

    /// <summary>The original location of the first slave handle, required to move it.</summary>
    private PointD firstOrigin;

    /// <summary>The original location of the last slave handle, required to move it.</summary>
    private PointD lastOrigin;

    /// <summary>Creates a new instance that wraps the original core handle for the given bend.</summary>
    public InnerControlPointHandle(IHandle coreHandle, IBend bend) {
      this.coreHandle = coreHandle;
      this.bend = bend;
      firstOrigin = new PointD(0, 0);
      lastOrigin = new PointD(0, 0);
    }

    /// <summary>Initializes our drag and determines whether this handle controls other handles.</summary>
    public void InitializeDrag(IInputModeContext context) {
      origin = bend.Location.ToPointD();
      coreHandle.InitializeDrag(context);
      if (context.InputMode is MoveInputMode) {
        // If we are moved via MoveInputMode (happens when the whole edge is dragged), delegate only to the core handle.
        return;
      }

      var index = bend.GetIndex();
      var firstBend = index > 0 ? bend.Owner.Bends[index - 1] : null;
      var lastBend = index < bend.Owner.Bends.Count - 1 ? bend.Owner.Bends[index + 1] : null;
      if (firstBend != null && lastBend != null &&
          BezierHandleUtil.AreCollinear(firstBend.Location, bend.Location, lastBend.Location)) {
        // Put a marker in the context so slave handles can distinguish dependent movement from a direct drag.
        var childContext = new InputModeContext(context);
        childContext.GetDecoratorFor<InnerControlPointHandle>().AddConstant(this);
        firstSlaveHandle = firstBend.Lookup<IHandle>();
        lastSlaveHandle = lastBend.Lookup<IHandle>();
        if (firstSlaveHandle != null) {
          firstSlaveHandle.InitializeDrag(childContext);
          firstOrigin = firstSlaveHandle.Location.ToPointD();
        }
        if (lastSlaveHandle != null) {
          lastSlaveHandle.InitializeDrag(childContext);
          lastOrigin = lastSlaveHandle.Location.ToPointD();
        }
      }
    }

    /// <summary>Moves the core handle and, if present, the slave handles with the same delta.</summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = newLocation - originalLocation;
      coreHandle.HandleMove(context, originalLocation, newLocation);
      firstSlaveHandle?.HandleMove(context, firstOrigin, firstOrigin + delta);
      lastSlaveHandle?.HandleMove(context, lastOrigin, lastOrigin + delta);
    }

    /// <summary>Cancels the movement on the core handle and, if present, on the slave handles.</summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      var childContext = new InputModeContext(context);
      childContext.GetDecoratorFor<InnerControlPointHandle>().AddConstant(this);
      coreHandle.CancelDrag(context, origin);
      firstSlaveHandle?.CancelDrag(childContext, firstOrigin);
      lastSlaveHandle?.CancelDrag(childContext, lastOrigin);
    }

    /// <summary>Finishes the movement on the core handle and, if present, on the slave handles.</summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = newLocation - originalLocation;
      coreHandle.DragFinished(context, originalLocation, newLocation);
      firstSlaveHandle?.DragFinished(context, firstOrigin, firstOrigin + delta);
      lastSlaveHandle?.DragFinished(context, lastOrigin, lastOrigin + delta);
      CleanUp();
    }

    /// <summary>Cleans up work at the end of the gesture, either by canceling or finishing.</summary>
    private void CleanUp() {
      firstSlaveHandle = null;
      lastSlaveHandle = null;
    }

    public object? Tag => coreHandle.Tag;

    /// <summary>Uses the core handle's type.</summary>
    public HandleType Type => coreHandle.Type;

    /// <summary>Uses the core handle's cursor.</summary>
    public Cursor Cursor => coreHandle.Cursor;

    /// <summary>Uses the core handle's location.</summary>
    public IPoint Location => coreHandle.Location;

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) => coreHandle.HandleClick(eventArgs);
  }

  internal static class BezierHandleUtil
  {
    private const double Eps = 1e-6;

    /// <summary>Checks whether three points are collinear.</summary>
    /// <returns><see langword="true"/> if all three points are approximately collinear.</returns>
    internal static bool AreCollinear(IPoint p1, IPoint p2, IPoint p3) =>
        Math.Abs(0.5 * (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y))) < Eps;
  }

  /// <summary>
  ///   Handle for control points of a Bézier curve if smooth editing is disabled. It stores the corresponding bend
  ///   and whether it belongs to an outer control point so the renderer can draw it differently.
  /// </summary>
  public sealed class SimpleControlPointHandle : IHandle
  {
    private readonly IHandle coreHandle;

    public IBend Bend { get; }

    public bool IsOuterControlPoint { get; }

    public SimpleControlPointHandle(IHandle coreHandle, IBend bend, bool isOuterControlPoint) {
      this.coreHandle = coreHandle;
      Bend = bend;
      IsOuterControlPoint = isOuterControlPoint;
    }

    public IPoint Location => coreHandle.Location;

    public void InitializeDrag(IInputModeContext context) => coreHandle.InitializeDrag(context);

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
        coreHandle.HandleMove(context, originalLocation, newLocation);

    public void CancelDrag(IInputModeContext context, PointD originalLocation) =>
        coreHandle.CancelDrag(context, originalLocation);

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
        coreHandle.DragFinished(context, originalLocation, newLocation);

    /// <summary>Inner control point handles are round and outer ones quadratic.</summary>
    public HandleType Type => IsOuterControlPoint ? HandleType.Move2 : coreHandle.Type;

    public object? Tag => coreHandle.Tag;

    public Cursor Cursor => coreHandle.Cursor;

    public void HandleClick(ClickEventArgs eventArgs) => coreHandle.HandleClick(eventArgs);
  }
}
