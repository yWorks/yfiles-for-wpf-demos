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
using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.BezierEdgeStyle
{
  public sealed class BezierGraphEditorInputMode : GraphEditorInputMode
  {
    public BezierGraphEditorInputMode(BezierSettings config) {
      var bezierBendInputMode = new BezierCreateBendInputMode();
      bezierBendInputMode.Priority = CreateBendInputMode.Priority;
      CreateBendInputMode = bezierBendInputMode;

      var bezierEdgeInputMode = new BezierCreateEdgeInputMode();
      bezierEdgeInputMode.Priority = CreateEdgeInputMode.Priority;
      bezierEdgeInputMode.CreateSmoothSplines = config.SmoothSegments;
      CreateEdgeInputMode = bezierEdgeInputMode;
    }

    /// <summary>
    ///   Ensures that deleting Bézier bends removes the correct number of points. Selecting a middle control point
    ///   selects its complete triple; selecting only an outer point removes it from the selection. When only two control
    ///   points remain, both are deleted together.
    /// </summary>
    protected override void OnDeletingSelection(SelectionEventArgs<IModelItem> args) {
      var selectedCurveBends = args.Selection.OfType<IBend>().Where(bend =>
          bend.Owner.Style is Graph.Styles.BezierEdgeStyle && bend.Owner.Bends.Count % 3 == 2 &&
          bend.GetIndex() % 3 == 2).ToList();
      foreach (var curveBend in selectedCurveBends) {
        var allBends = curveBend.Owner.Bends;
        var index = curveBend.GetIndex();
        args.Selection.Add(allBends[index - 1]);
        args.Selection.Add(allBends[index + 1]);
      }

      // Remove remaining single control points from the list.
      var singularControlPoints = args.Selection.OfType<IBend>().Where(bend => {
        var edge = bend.Owner;
        var allBends = edge.Bends;
        var bendIndex = bend.GetIndex();
        return edge.Style is Graph.Styles.BezierEdgeStyle && allBends.Count % 3 == 2 && (bendIndex == 0 ||
            bendIndex == allBends.Count - 1 ||
            (bendIndex % 3 == 1 && !args.Selection.Contains(allBends[bendIndex + 1])) ||
            (bendIndex % 3 == 0 && !args.Selection.Contains(allBends[bendIndex - 1])));
      }).ToList();
      foreach (var singularControlPoint in singularControlPoints) {
        var owner = singularControlPoint.Owner;
        if (owner.Bends.Count > 2) {
          args.Selection.Remove(singularControlPoint);
        } else {
          // Special case: remove both of the last control points.
          args.Selection.Add(owner.Bends[0]);
          args.Selection.Add(owner.Bends[1]);
        }
      }

      base.OnDeletingSelection(args);
    }

    protected override void OnCreateBendInputModeDragSegmentFinished(
        object sender,
        InputModeItemEventArgs<IBend> eventArgs) {
      var bend = eventArgs.Item;
      var edge = bend?.Owner;
      var mode = sender;
      if (bend != null && mode is BezierCreateBendInputMode bezierBendMode &&
          edge?.Style is Graph.Styles.BezierEdgeStyle && edge.Bends.Count % 3 == 2 && bend.GetIndex() % 3 == 2) {
        if (bezierBendMode.WasExistingBend) {
          // Do not remove an existing bend when the gesture is canceled; simply drag it.
          DragBend(bend);
        } else {
          // Remove a newly created bend and restore its neighbors if the gesture is canceled.
          var handler = new BendCreationHandler(bend, GraphControl!.Graph, bezierBendMode);
          handler.Register(HandleInputMode);
          try {
            DragBend(bend);
          } finally {
            handler.Dragged();
          }
        }
      } else {
        base.OnCreateBendInputModeDragSegmentFinished(sender, eventArgs);
      }
    }
  }

  /// <summary>Remembers bend locations of an affected edge while a newly created bend is dragged.</summary>
  internal sealed class BezierCreateBendInputMode : CreateBendInputMode
  {
    private readonly Dictionary<IBend, PointD> locationMementos = new();

    /// <summary>
    ///   Remembers whether a new bend was created or an existing one is being dragged. This determines whether to remove
    ///   the dragged bend in <see cref="BezierGraphEditorInputMode.OnCreateBendInputModeDragSegmentFinished"/>.
    /// </summary>
    public bool WasExistingBend { get; private set; }

    public IReadOnlyDictionary<IBend, PointD> LocationMementos => locationMementos;

    protected override IBend? CreateBend(IEdge edge, PointD location) {
      locationMementos.Clear();
      foreach (var existingBend in edge.Bends) {
        locationMementos[existingBend] = existingBend.Location.ToPointD();
      }
      var oldBends = edge.Bends.ToList();
      var bend = base.CreateBend(edge, location);
      WasExistingBend = bend != null && edge.Bends.Count == oldBends.Count && oldBends.Contains(bend);
      return bend;
    }
  }

  internal sealed class BendCreationHandler
  {
    private readonly IBend bend;
    private readonly IGraph graph;
    private readonly BezierCreateBendInputMode bendInputMode;
    private bool initialized;
    private HandleInputMode? inputMode;
    private EventHandler<InputModeEventArgs>? dragStartedListener;
    private EventHandler<InputModeEventArgs>? dragCanceledListener;
    private EventHandler<InputModeEventArgs>? dragFinishedListener;

    internal BendCreationHandler(IBend bend, IGraph graph, BezierCreateBendInputMode bendInputMode) {
      this.bend = bend;
      this.graph = graph;
      this.bendInputMode = bendInputMode;
    }

    public void Register(HandleInputMode inputMode) {
      this.inputMode = inputMode;
      dragStartedListener = (_, _) => InputModeOnDragStarted();
      dragCanceledListener = (_, _) => InputModeOnDragCanceled();
      dragFinishedListener = (_, _) => InputModeOnDragFinished();
      inputMode.DragStarted += dragStartedListener;
      inputMode.DragCanceled += dragCanceledListener;
      inputMode.DragFinished += dragFinishedListener;
    }

    private void InputModeOnDragFinished() => Unregister();

    private void InputModeOnDragCanceled() {
      Unregister();
      if (!graph.Contains(bend)) {
        return;
      }
      var edge = bend.Owner;
      var bendIndex = bend.GetIndex();
      var previousBend = bendIndex > 0 ? edge.Bends[bendIndex - 1] : null;
      var previousPreviousBend = bendIndex > 1 ? edge.Bends[bendIndex - 2] : null;
      var nextBend = bendIndex < edge.Bends.Count - 1 ? edge.Bends[bendIndex + 1] : null;
      var nextNextBend = bendIndex < edge.Bends.Count - 2 ? edge.Bends[bendIndex + 2] : null;

      graph.Remove(bend);
      if (previousBend != null) {
        graph.Remove(previousBend);
      }
      if (nextBend != null) {
        graph.Remove(nextBend);
      }
      if (previousPreviousBend != null &&
          bendInputMode.LocationMementos.TryGetValue(previousPreviousBend, out var previousLocation)) {
        graph.SetBendLocation(previousPreviousBend, previousLocation);
      }
      if (nextNextBend != null && bendInputMode.LocationMementos.TryGetValue(nextNextBend, out var nextLocation)) {
        graph.SetBendLocation(nextNextBend, nextLocation);
      }
    }

    private void InputModeOnDragStarted() => initialized = true;

    public void Dragged() {
      if (!initialized) {
        Unregister();
      }
    }

    private void Unregister() {
      if (inputMode == null) {
        return;
      }
      if (dragStartedListener != null) {
        inputMode.DragStarted -= dragStartedListener;
      }
      if (dragCanceledListener != null) {
        inputMode.DragCanceled -= dragCanceledListener;
      }
      if (dragFinishedListener != null) {
        inputMode.DragFinished -= dragFinishedListener;
      }
      inputMode = null;
    }
  }
}
