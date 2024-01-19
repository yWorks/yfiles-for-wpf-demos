/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.BezierEdgeStyle
{
  /// <summary>
  /// Handle for an outer control point bend of a bezier curve
  /// </summary>
  /// <remarks>The outer control point bends are those that are not located on the path, but determine the slope of the segments.
  /// If the control point triple is currently collinear, this implementation moves the <b>other</b> outer control point
  /// of the triple so that this invariant is kept. Otherwise, the other control point is not adjusted.
  /// </remarks>
  internal class OuterControlPointHandle : IHandle
  {
    private const double EPS = 1e-6;

    /// <summary>
    /// The core bend handle that performs the actual bend movement.
    /// </summary>
    private readonly IHandle coreHandle;

    /// <summary>
    /// The bend that belongs to this handle instance
    /// </summary>
    private readonly IBend bend;

    /// <summary>
    /// The other handle that is controlled indirectly from us.
    /// </summary>
    private IHandle slaveHandle;

    /// <summary>
    /// The original location ot the slave handle, required to perform the actual movement of the other handle.
    /// </summary>
    private PointD slaveOrigin;

    /// <summary>
    /// The location of the middle bend of a control point triple.
    /// </summary>
    /// <remarks>This is used as the axis of rotation to keep the collinearity invariant.</remarks>
    private PointD middleLocation;

    /// <summary>
    /// Creates a new instance that wraps the original <paramref name="coreHandle"/> for the given <paramref name="bend"/>.
    /// </summary>
    public OuterControlPointHandle([NotNull] IHandle coreHandle, [NotNull] IBend bend) {
      this.coreHandle = coreHandle;
      this.bend = bend;
    }

    /// <summary>
    /// Initializes our own drag and determines whether we are a slave or the master and if there are actual slave handles in that case
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      coreHandle.InitializeDrag(context);
      if (context.ParentInputMode is MoveInputMode) {
        //If we are moved via MoveInputMode (happens when the whole edge is dragged)
        //We only delegate to the core handle
        return;
      }

      //If we are indirectly controlled from the other control point or the bend curve point
      //those implementations put a marker in the lookup
      //If such a marker is present, we DON'T delegate to the other handle and just move ourselves.
      var bcph = context.Lookup<InnerControlPointHandle>();
      var cph = context.Lookup<OuterControlPointHandle>();

      if (bcph == null && cph == null) {
        //We are the master handle and so we control the other one
        var index = bend.GetIndex();

        //Whether this is the first or the last bend in such a control point triple
        var isFirstInTriplet = index % 3 == 1;

        IBend otherBend = null;
        IBend middleBend = null;
        if (isFirstInTriplet && index < bend.Owner.Bends.Count - 1) {
          //We are the first of the triple and there is a potential slave handle
          //So get the slave and the middle bend
          otherBend = bend.Owner.Bends[index + 2];
          middleBend = bend.Owner.Bends[index + 1];
        } else if (index >= 3) {
          //We are the last of the triple and there is a potential slave handle
          //So get the slave and the middle bend
          otherBend = bend.Owner.Bends[index - 2];
          middleBend = bend.Owner.Bends[index - 1];
        }
        if (otherBend != null && AreCollinear(bend.Location, middleBend.Location, otherBend.Location)) {
          slaveHandle = otherBend.Lookup<IHandle>();
          middleLocation = middleBend.Location.ToPointD();
        }

        if (slaveHandle != null) {
          //There not only a bend, but actually a handle to control
          //notify it that it is the slave
          //We just put ourselves in the context, so our presence serves as flag to the other handle
          //And from now on control its actions.
          var childContext = Contexts.CreateInputModeContext(context.ParentInputMode, context, Lookups.Single(this));
          slaveHandle.InitializeDrag(childContext);
          slaveOrigin = slaveHandle.Location.ToPointD();
        }
      }
    }

    /// <summary>
    /// Checks whether three points are collinear.
    /// </summary>
    /// <returns>true iff all three points are (approximately) collinear.</returns>
    internal static bool AreCollinear([NotNull] IPoint p1, [NotNull] IPoint p2, [NotNull] IPoint p3) {
      //Use the cross product to check whether we are collinear
      return Math.Abs(0.5 * (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y))) < EPS;
    }

    /// <summary>
    /// Move the core handle and if present also the slave handle in a way that the control point triple is collinear.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      coreHandle.HandleMove(context, originalLocation, newLocation);

      if (slaveHandle != null) {
        //If necessary, rotate the slave handle
        //Move the other one by the point reflection of our move delta, keeping its distance, though
        var delta = newLocation - middleLocation;
        //The distance of the slave handle - we keep this
        var otherDist = (slaveOrigin - middleLocation).VectorLength;
        //We can use the original context since we have already done all decisions in InitializeDrag
        slaveHandle.HandleMove(context, slaveOrigin, middleLocation - delta.Normalized * otherDist);
      }
    }

    /// <summary>
    /// Cancel the movement on the core handle and if present also on the slave handle
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      coreHandle.CancelDrag(context, originalLocation);

      if (slaveHandle != null) {
        //If necessary, cancel the movement of the slave handle, using its old origin
        //We can use the original context since we have already done all decisions in InitializeDrag
        slaveHandle.CancelDrag(context, slaveOrigin);
        slaveHandle = null;
      }
    }

    /// <summary>
    /// Finish the movement on the core handle and if present also on the slave handle
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      coreHandle.DragFinished(context, originalLocation, newLocation);
      if (slaveHandle != null) {
        //If necessary, rotate the slave handle
        //Move the other one by the point reflection of our move delta, keeping its distance, though
        var delta = newLocation - middleLocation;
        //The distance of the slave handle - we keep this
        var otherDist = (slaveOrigin - middleLocation).VectorLength;
        slaveHandle.DragFinished(context, slaveOrigin, middleLocation - delta.Normalized * otherDist);
        CleanUp();
      }
    }

    /// <summary>
    /// Clean up work at the end of the gesture, either by canceling or finishing
    /// </summary>
    private void CleanUp() {
      slaveHandle = null;
    }

    /// <summary>
    /// We use a slightly different visualization
    /// </summary>
    public HandleTypes Type {
      get { return HandleTypes.Default | HandleTypes.Variant1; }
    }

    /// <summary>
    /// Use the core handle's cursor
    /// </summary>
    public Cursor Cursor {
      get { return coreHandle.Cursor; }
    }

    /// <summary>
    /// Use the core handle's location
    /// </summary>
    public IPoint Location {
      get { return coreHandle.Location; }
    }

    /// <summary>
    /// Use the core handle's click handling.
    /// </summary>
    /// <param name="eventArgs">Arguments describing the click.</param>
    public void HandleClick(ClickEventArgs eventArgs) {
      coreHandle.HandleClick(eventArgs);
    }
  }

  /// <summary>
  /// Handle for an inner control point bend of a bezier curve
  /// </summary>
  /// <remarks>The inner control point bends are those that are actually located on the path.
  /// If the control point triple is currently collinear, this implementation moves the  outer control points
  /// of the triple so that this invariant is kept. Otherwise, the other control points are not adjusted.
  /// </remarks>
  internal class InnerControlPointHandle : IHandle
  {
    /// <summary>
    /// The core bend handle that performs the actual bend movement.
    /// </summary>
    private readonly IHandle coreHandle;

    /// <summary>
    /// The bend that belongs to this handle instance
    /// </summary>
    private readonly IBend bend;

    /// <summary>
    /// The first (slave) handle in a control point triple
    /// </summary>
    private IHandle firstSlaveHandle;

    /// <summary>
    /// The last (slave) handle in a control point triple
    /// </summary>
    private IHandle lastSlaveHandle;

    /// <summary>
    /// The original location ot the first slave handle, required to perform the actual movement of the that handle.
    /// </summary>
    private PointD firstOrigin;

    /// <summary>
    /// The original location ot the last slave handle, required to perform the actual movement of the that handle.
    /// </summary>
    private PointD lastOrigin;

    /// <summary>
    /// Creates a new instance that wraps the original <paramref name="coreHandle"/> for the given <paramref name="bend"/>.
    /// </summary>
    public InnerControlPointHandle([NotNull] IHandle coreHandle, [NotNull] IBend bend) {
      this.coreHandle = coreHandle;
      this.bend = bend;
    }

    /// <summary>
    /// Initializes our own drag and determines whether we are a slave or the master and if there are actual slave handles in that case
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      coreHandle.InitializeDrag(context);
      if (context.ParentInputMode is MoveInputMode) {
        //If we are moved via MoveInputMode (happens when the whole edge is dragged)
        //We only delegate to the core handle
        return;
      }
      var index = bend.GetIndex();

      IBend firstBend = index > 0 ? bend.Owner.Bends[index - 1] : null;
      IBend lastBend = index < bend.Owner.Bends.Count - 1 ? bend.Owner.Bends[index + 1] : null;

      if (firstBend != null && lastBend != null &&
          OuterControlPointHandle.AreCollinear(firstBend.Location, bend.Location, lastBend.Location)) {
        //Put a marker in the context so that the slave handles can distinguish whether they are moved dependent from us, or are dragged directly
        var childContext = Contexts.CreateInputModeContext(context.ParentInputMode, context, Lookups.Single(this));
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

    /// <summary>
    /// Move the core handle and if present also the slave handles in a way that the control point triple is collinear.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = newLocation - originalLocation;
      coreHandle.HandleMove(context, originalLocation, newLocation);
      if (firstSlaveHandle != null) {
        firstSlaveHandle.HandleMove(context, firstOrigin, firstOrigin + delta);
      }
      if (lastSlaveHandle != null) {
        lastSlaveHandle.HandleMove(context, lastOrigin, lastOrigin + delta);
      }
    }

    /// <summary>
    /// Cancel the movement on the core handle and if present also on the slave handles
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      var childContext = Contexts.CreateInputModeContext(context.ParentInputMode, context, Lookups.Single(this));
      coreHandle.CancelDrag(context, originalLocation);
      if (firstSlaveHandle != null) {
        firstSlaveHandle.CancelDrag(childContext, firstOrigin);
      }
      if (lastSlaveHandle != null) {
        lastSlaveHandle.CancelDrag(childContext, lastOrigin);
      }
    }

    /// <summary>
    /// Finish the movement on the core handle and if present also on the slave handles
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var delta = newLocation - originalLocation;
      coreHandle.DragFinished(context, originalLocation, newLocation);
      if (firstSlaveHandle != null) {
        firstSlaveHandle.DragFinished(context, firstOrigin, firstOrigin + delta);
      }
      if (lastSlaveHandle != null) {
        lastSlaveHandle.DragFinished(context, lastOrigin, lastOrigin + delta);
      }
      CleanUp();
    }

    /// <summary>
    /// Clean up work at the end of the gesture, either by canceling or finishing
    /// </summary>
    private void CleanUp() {
      firstSlaveHandle = null;
      lastSlaveHandle = null;
    }

    /// <summary>
    /// Use the core handle's type
    /// </summary>
    public HandleTypes Type {
      get { return coreHandle.Type; }
    }

    /// <summary>
    /// Use the core handle's cursor
    /// </summary>
    public Cursor Cursor {
      get { return coreHandle.Cursor; }
    }

    /// <summary>
    /// Use the core handle's location
    /// </summary>
    public IPoint Location {
      get { return coreHandle.Location; }
    }

    /// <summary>
    /// Use the core handle's click handling.
    /// </summary>
    /// <param name="eventArgs">Arguments describing the click.</param>
    public void HandleClick(ClickEventArgs eventArgs) {
      coreHandle.HandleClick(eventArgs);
    }
  }
}
