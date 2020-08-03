/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Graph.BezierEdgeStyle {
  internal sealed class BezierGraphEditorInputMode : GraphEditorInputMode
  {
    private readonly BezierEdgeStyleWindow bezierEdgeStyleWindow;

    public BezierGraphEditorInputMode(BezierEdgeStyleWindow bezierEdgeStyleWindow) {
      this.bezierEdgeStyleWindow = bezierEdgeStyleWindow;
    }

    /// <summary>
    /// Overridden to ensure when deleting bezier bends, the correct number is actually removed.
    /// </summary>
    /// <remarks>
    /// This method doe the following:
    /// - for each middle control point of a bezier control triple, it also selects both other control points
    /// - if there are bezier control points selected where the middle control point is NOT selected, they are deselected.
    /// So in effect, either a complete triple is removed (when the middle point is selected), or nothing (when ONLY one of the outer points is selected)
    /// Exception: When only two control points are left, both are deleted together
    /// </remarks>
    protected override void OnDeletingSelection(SelectionEventArgs<IModelItem> args) {
      var selectedCurveBends = args.Selection.OfType<IBend>()
                                   .Where(b => b.Owner.Style is yWorks.Graph.Styles.BezierEdgeStyle && b.Owner.Bends.Count % 3 == 2 &&
                                               b.GetIndex() % 3 == 2).ToList();
      foreach (var selectedCurveBend in selectedCurveBends) {
        args.Selection.SetSelected(selectedCurveBend.Owner.Bends[selectedCurveBend.GetIndex() - 1], true);
        args.Selection.SetSelected(selectedCurveBend.Owner.Bends[selectedCurveBend.GetIndex() + 1], true);
      }
      //Remove remaining single control points from the list...
      var singularControlPoints
          = args.Selection.OfType<IBend>()
                .Where(b => b.Owner.Style is yWorks.Graph.Styles.BezierEdgeStyle && b.Owner.Bends.Count % 3 == 2 &&
                            (
                                (b.GetIndex() == 0) ||
                                (b.GetIndex() == b.Owner.Bends.Count - 1) ||
                                (b.GetIndex() % 3 == 1 &&
                                 !args.Selection.IsSelected(b.Owner.Bends[b.GetIndex() + 1])) ||
                                (b.GetIndex() % 3 == 0 && !args.Selection.IsSelected(b.Owner.Bends[b.GetIndex() + -1]))
                            )
                )
                .ToList();
      foreach (var singularControlPoint in singularControlPoints) {
        if (singularControlPoint.Owner.Bends.Count > 2) {
          args.Selection.SetSelected(singularControlPoint, false);
        } else {
          //Special case: Remove both of the last control points
          args.Selection.SetSelected(singularControlPoint.Owner.Bends[0], true);
          args.Selection.SetSelected(singularControlPoint.Owner.Bends[1], true);
        }
      }

      base.OnDeletingSelection(args);
    }

    protected override void OnCreateBendInputModeBendCreated(object sender, ItemEventArgs<IBend> e) {
      var bend = e.Item;
      if (bend != null) {
        var edge = bend.Owner;
        var mode = sender as BezierCreateBendInputMode;

        if (mode != null && edge.Style is yWorks.Graph.Styles.BezierEdgeStyle && edge.Bends.Count % 3 == 2 && bend.GetIndex() % 3 == 2) {
          // we need to remove the bend when the gesture is canceled.
          var handler = new BendCreationHandler(bend, Graph, mode);
          handler.Register(HandleInputMode);
          try {
            DragBend(bend);
          } finally {
            handler.Dragged();
          }
        } else {
          base.OnCreateBendInputModeBendCreated(sender, e);
        }
      }
    }

    /// <summary>
    /// This class removes the dragged bend when the handle input mode is canceled.
    /// </summary>
    /// <remarks>
    /// This is to support the <see cref="GraphEditorInputMode.OnCreateBendInputModeBendCreated"/>
    /// implementation for bends that are created during the gesture and the user wants to cancel
    /// the gesture, not only moving the bend back to the original location, but also removing
    /// the newly created bend.
    ///
    /// This implementation also removes the additionally created bends for a bezier edge and resets the positions of the control points.
    /// </remarks>
    private sealed class BendCreationHandler 
    {
      private readonly IBend bend;
      private readonly IGraph graph;
      private readonly BezierCreateBendInputMode bendInputMode;
      private bool initialized;
      private HandleInputMode inputMode;

      public BendCreationHandler(IBend bend, IGraph graph, BezierCreateBendInputMode bendInputMode) {
        this.bend = bend;
        this.graph = graph;
        this.bendInputMode = bendInputMode;
      }

      public void Register(HandleInputMode inputMode) {
        this.inputMode = inputMode;
        inputMode.DragStarted += InputModeOnDragStarted;
        inputMode.DragCanceled += InputModeOnDragCanceled;
        inputMode.DragFinished += InputModeOnDragFinished;
      }

      private void InputModeOnDragFinished(object sender, InputModeEventArgs inputModeEventArgs) {
        Unregister();
      }

      private void InputModeOnDragCanceled(object sender, InputModeEventArgs inputModeEventArgs) {
        Unregister();
        if (graph.Contains(bend)) {
          var edge = bend.Owner;
          var bendIndex = bend.GetIndex();
          IBend previousBend = null;
          IBend nextBend = null;
          if (bendIndex > 0) {
            previousBend = edge.Bends[bendIndex - 1];
          }
          IBend prevPrevBend = null;
          if (bendIndex > 1) {
            prevPrevBend = edge.Bends[bendIndex - 2];
          }
          if (bendIndex < edge.Bends.Count - 1) {
            nextBend = edge.Bends[bendIndex + 1];
          }
          IBend nextNextBend = null;
          if (bendIndex < edge.Bends.Count - 2) {
            nextNextBend = edge.Bends[bendIndex + 2];
          }
          graph.Remove(bend);
          //Also remove the additional bends
          if (previousBend != null) {
            graph.Remove(previousBend);
          }
          if (nextBend != null) {
            graph.Remove(nextBend);
          }
          //And roll back the position change of the adjacent bends
          if (prevPrevBend != null) {
            PointD oldLocation;
            if (bendInputMode.LocationMementos.TryGetValue(prevPrevBend, out oldLocation)) {
              graph.SetBendLocation(prevPrevBend, oldLocation);
            }
          }
          if (nextNextBend != null) {
            PointD oldLocation;
            if (bendInputMode.LocationMementos.TryGetValue(nextNextBend, out oldLocation)) {
              graph.SetBendLocation(nextNextBend, oldLocation);
            }
          }
        }
      }

      private void InputModeOnDragStarted(object sender, InputModeEventArgs inputModeEventArgs) {
        initialized = true;
      }

      public void Dragged() {
        if (!initialized) {
          Unregister();
        }
      }

      private void Unregister() {
        inputMode.DragStarted -= InputModeOnDragStarted;
        inputMode.DragCanceled -= InputModeOnDragCanceled;
        inputMode.DragFinished -= InputModeOnDragFinished;
      }
    }

    protected override CreateBendInputMode CreateCreateBendInputMode() {
      return new BezierCreateBendInputMode{Priority = 42};
    }

    protected override CreateEdgeInputMode CreateCreateEdgeInputMode() {
      return new BezierCreateEdgeInputMode {Priority = 45, CreateSmoothSplines = bezierEdgeStyleWindow.SmoothEditing};
    }

    /// <summary>
    /// Custom input mode implementation that temporarily remembers all bend locations of the affected edge.
    /// </summary>
    /// <remarks>This is to make it easier to unroll the bend creation when the drag is canceled</remarks>
    private sealed class BezierCreateBendInputMode : CreateBendInputMode
    {
      private readonly IDictionary<IBend, PointD> locationMementos = new Dictionary<IBend, PointD>();
        
      public IDictionary<IBend, PointD> LocationMementos {
        get {
          return locationMementos;
        }
      }

      protected override IBend CreateBend(IEdge edge, PointD location) {
        LocationMementos.Clear();
        foreach (var existingBend in edge.Bends) {
          LocationMementos[existingBend] = existingBend.Location.ToPointD();
        }
        return base.CreateBend(edge, location);
      }
    }

  }
}
