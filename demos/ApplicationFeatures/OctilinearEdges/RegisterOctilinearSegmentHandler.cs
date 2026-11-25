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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>Provides interactive editing support for octilinear edges.</summary>
  internal static class RegisterOctilinearSegmentHandler
  {
    private readonly struct EdgePathPoint
    {
      public PointD Location { get; }

      public IModelItem Owner { get; }

      public EdgePathPoint(PointD location, IModelItem owner) {
        Location = location;
        Owner = owner;
      }
    }

    /// <summary>
    ///   <para>
    ///     Registers all handlers for interactive editing of edges with <see cref="OctilinearEdgeStyle"/>.
    ///   </para>
    ///   <para>
    ///     It installs a separate <see cref="MoveInputMode"/> that handles dragging directly on octilinear segments by either
    ///     moving
    ///     the bend of the segment or resizing the cutting length; and registers a handle provider that shows an explicit
    ///     handle on the octilinear segment's center.
    ///   </para>
    ///   <para>
    ///     Additionally, registers a selection renderer wrapper to visualize bend indicators in the center of the octilinear
    ///     segments.
    ///   </para>
    /// </summary>
    /// <param name="graphControl">The graph component on which the interaction is registered.</param>
    internal static void Register(GraphControl graphControl) {
      var graphEditorInputMode = (GraphEditorInputMode) graphControl.InputMode!;
      var octilinearSegmentInputMode = RegisterSegmentInteractions(graphEditorInputMode);
      var decorator = graphControl.Graph.GetDecorator();

      // provide octilinear bend handles for octilinear segments
      decorator.Bends.Handle.AddWrapperFactory(
          bend => graphControl.Selection.Bends.Contains(bend) && bend.Owner.Style is OctilinearEdgeStyle,
          (bend, handle) => new OctilinearSegmentHandle(bend, handle!, false));

      // indicate the bend selection on the octilinear segments
      decorator.Edges.SelectionRenderer.AddWrapperFactory(edge => edge.Style is OctilinearEdgeStyle,
          (_, renderer) => new OctilinearSelectionRenderer(renderer!));

      // Ensure when clicking the handle position that the appearing OctilinearHandle is synced with the
      // input mode state even without an additional input event (e.g., mouse move).
      graphControl.Selection.ItemAdded += (_, _) => RequeryInputMode(graphEditorInputMode);

      RegisterCuttingLengthSynchronization(graphControl, graphEditorInputMode, octilinearSegmentInputMode);
    }

    /// <summary>
    ///   Triggers a re-evaluation of the last input event.
    /// </summary>
    private static void RequeryInputMode(GraphEditorInputMode graphEditorInputMode) {
      graphEditorInputMode.Enabled = false;
      graphEditorInputMode.Enabled = true;
    }

    /// <summary>
    ///   <para>
    ///     Synchronizes implicit changes of the cutting length to ensure that the stored data on the tag corresponds to the
    ///     visual representation in the graph.
    ///   </para>
    ///   <para>
    ///     For example, when dragging a node, attached edge paths may change such that the cutting length of a bend cannot be
    ///     rendered entirely (due to space limitations wrt. the next bend). After the gesture is finished, the actual cutting
    ///     length is evaluated and stored on the bend's tag.
    ///   </para>
    /// </summary>
    private static void RegisterCuttingLengthSynchronization(
        GraphControl graphControl,
        GraphEditorInputMode graphEditorInputMode,
        MoveInputMode octilinearSegmentInputMode) {
      // dragging on an octilinear segment
      octilinearSegmentInputMode.DragFinished += (_, _) =>
          SynchronizeAffectedBends(graphControl.Graph, octilinearSegmentInputMode.AffectedItems);

      // moving selected nodes
      graphEditorInputMode.MoveSelectedItemsInputMode.DragFinished += (_, _) =>
          SynchronizeAffectedBends(graphControl.Graph, graphEditorInputMode.MoveSelectedItemsInputMode.AffectedItems);

      // moving unselected nodes
      graphEditorInputMode.MoveUnselectedItemsInputMode.DragFinished += (_, _) =>
          SynchronizeAffectedBends(graphControl.Graph, graphEditorInputMode.MoveUnselectedItemsInputMode.AffectedItems);

      // resizing nodes or dragging on the octilinear segment's handle
      graphEditorInputMode.HandleInputMode.DragFinishing += (_, _) =>
          SynchronizeAffectedBends(graphControl.Graph, graphEditorInputMode.HandleInputMode.AffectedItems);

      // orthogonal edge editing creates an implicit bend during the gesture whose cutting length must be ignored to avoid
      // style rendering artifacts
      graphEditorInputMode.HandleInputMode.DragStarting += (_, _) => {
        if (graphControl.Selection.Bends.Count == 1) {
          var temporaryBend = graphControl.Selection.Bends.First();
          var bendLocation = Utils.GetLocation(temporaryBend);
          if (GeometryUtilities.AreCollinear(Utils.GetLocation(Utils.GetPreviousEdgeItem(temporaryBend)), bendLocation,
              Utils.GetLocation(Utils.GetNextEdgeItem(temporaryBend)))) {
            Utils.UpdateCuttingLength(temporaryBend, 0);
          }
        }
      };
    }

    /// <summary>
    ///   Updates the bend tags of the affected items with the current cutting length.
    /// </summary>
    /// <param name="graph">The owner graph of the affected items.</param>
    /// <param name="affectedItems">The items whose bends' cutting lengths should be updated with the actual cutting length.</param>
    private static void SynchronizeAffectedBends(IGraph graph, IEnumerable<IModelItem> affectedItems) {
      var newCuttingLengths = new Dictionary<IBend, double>();
      foreach (var affectedItem in affectedItems) {
        if (affectedItem is INode node) {
          foreach (var edge in graph.EdgesAt(node)) {
            if (edge.Style is OctilinearEdgeStyle) {
              foreach (var bend in edge.Bends) {
                newCuttingLengths[bend] = Utils.GetVisualCuttingLength(bend);
              }
            }
          }
        } else if (affectedItem is IBend bend && graph.Contains(bend) && bend.Owner.Style is OctilinearEdgeStyle) {
          foreach (var edgeBend in bend.Owner.Bends) {
            newCuttingLengths[edgeBend] = Utils.GetVisualCuttingLength(edgeBend);
          }
        }
      }

      foreach (var (bend, newCuttingLength) in newCuttingLengths) {
        Utils.UpdateCuttingLength(bend, newCuttingLength);
      }
    }

    /// <summary>
    ///   Registers a separate <see cref="MoveInputMode"/> on the <see cref="GraphEditorInputMode"/> that manages drag gestures
    ///   on octilinear edge segments.
    /// </summary>
    /// <param name="graphEditorInputMode">The input mode to which the additional <see cref="MoveInputMode"/> is registered.</param>
    private static MoveInputMode RegisterSegmentInteractions(GraphEditorInputMode graphEditorInputMode) {
      var octilinearSegmentInputMode = new MoveInputMode {
          Priority = graphEditorInputMode.HandleInputMode.Priority + 1,
          // Disable directional constraints because we use Shift for toggling orthogonal segment interaction
          DirectionalConstraintRecognizer = EventRecognizers.Never
      };

      // Trigger the OrthogonalEdgeEditingContext when moving the octilinear segment
      RegisterOrthogonalEditing(octilinearSegmentInputMode);

      // Arm the MoveInputMode when hovering over an octilinear segment
      octilinearSegmentInputMode.HitTestable = HitTestables.Create((context, location) => {
        var bend = GetBendOfHitOctilinearSegment(context, location);
        if (bend != null) {
          octilinearSegmentInputMode.ValidBeginCursor = GetCursor(context, Utils.GetCornerType(bend));
          return true;
        }
        return false;
      });

      // When dragging on an octilinear segment, provide a custom position handler that manages the interaction
      octilinearSegmentInputMode.QueryPositionHandler += (_, args) => {
        if (args.Handled) {
          return;
        }

        var bendOfHitSegment = GetBendOfHitOctilinearSegment(args.Context, args.QueryLocation);
        if (bendOfHitSegment != null) {
          octilinearSegmentInputMode.MoveCursor = GetCursor(args.Context, Utils.GetCornerType(bendOfHitSegment));
          args.PositionHandler = new OctilinearSegmentPositionHandler(bendOfHitSegment);
          args.Handled = true;
        }
      };

      // Do not move other elements while moving edge segments
      octilinearSegmentInputMode.DragStarting += (_, args) =>
          ((GraphControl) args.Context.CanvasControl).Selection.Clear();

      graphEditorInputMode.Add(octilinearSegmentInputMode);
      return octilinearSegmentInputMode;
    }

    /// <summary>
    ///   Registers the orthogonal edge editing functionality for the given octilinear input mode.
    /// </summary>
    /// <param name="octilinearSegmentInputMode">The input mode to which orthogonal editing is added.</param>
    private static void RegisterOrthogonalEditing(MoveInputMode octilinearSegmentInputMode) {
      var initializedOrthogonalDrag = false;

      octilinearSegmentInputMode.DragStarting += (_, args) => {
        if (OctilinearSegmentPositionHandler.MoveBendRecognizer(args.Context.CanvasControl,
            args.Context.CanvasControl.LastPointerEvent)) {
          (args.Context.Lookup(typeof(OrthogonalEdgeEditingContext)) as OrthogonalEdgeEditingContext)
              ?.InitializeDrag(args.Context);
          initializedOrthogonalDrag = true;
        }
      };
      octilinearSegmentInputMode.DragStarted += (_, args) => {
        if (initializedOrthogonalDrag) {
          (args.Context.Lookup(typeof(OrthogonalEdgeEditingContext)) as OrthogonalEdgeEditingContext)
              ?.DragInitialized();
        }
      };
      octilinearSegmentInputMode.DragFinished += (_, args) => {
        if (initializedOrthogonalDrag) {
          (args.Context.Lookup(typeof(OrthogonalEdgeEditingContext)) as OrthogonalEdgeEditingContext)?.DragFinished();
        }
        initializedOrthogonalDrag = false;
      };
      octilinearSegmentInputMode.DragCanceled += (_, args) => {
        if (initializedOrthogonalDrag) {
          (args.Context.Lookup(typeof(OrthogonalEdgeEditingContext)) as OrthogonalEdgeEditingContext)?.CancelDrag();
        }
        initializedOrthogonalDrag = false;
      };
    }

    /// <summary>
    ///   Returns the bend of an octilinear segment when the given location hits an octilinear segment.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="queryLocation">The location to test for an octilinear segment hit.</param>
    private static IBend? GetBendOfHitOctilinearSegment(IInputModeContext context, PointD queryLocation) {
      var hitTester = (IHitTester) context.Lookup(typeof(IHitTester))!;
      // remark: we don't test for other graph items like labels for performance reasons
      var hits = hitTester.EnumerateHits(context, queryLocation, GraphItemTypes.Edge | GraphItemTypes.Node);

      var hitItem = hits.FirstOrDefault();
      if (hitItem is IEdge edge && edge.Style is OctilinearEdgeStyle) {
        return GetBendOfHitOctilinearSegmentForEdge(context, edge, queryLocation);
      }

      return null;
    }

    /// <summary>
    ///   <para>
    ///     Determines the actual hit segment and its bend for the hit edge.
    ///   </para>
    ///   <para>
    ///     To determine the hit segment, a point-based representation of the edge's octilinear path is created where each
    ///     segment is tested for a hit of the query location considering the context's hit-test radius.
    ///   </para>
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="edge">The hit edge.</param>
    /// <param name="queryLocation">The location of the hit.</param>
    private static IBend? GetBendOfHitOctilinearSegmentForEdge(
        IInputModeContext context,
        IEdge edge,
        PointD queryLocation) {
      var visualPathPoints = GetVisualPathPoints(edge);

      // Check which segment is hit and return its owner bend
      for (var i = 0; i < visualPathPoints.Count - 1; i++) {
        var current = visualPathPoints[i];
        var next = visualPathPoints[i + 1];

        // Octilinear segments are only between two path points belonging to the same owner bend
        if (!ReferenceEquals(current.Owner, next.Owner)) {
          continue;
        }

        // If an octilinear segment has length zero, there is no segment to hit-test, so check proximity to the neighboring
        // path points instead to be able to resize it
        var distanceToCurrent = current.Location.DistanceTo(queryLocation);
        var distanceToNext = next.Location.DistanceTo(queryLocation);
        if (distanceToCurrent < context.HitTestRadius && distanceToNext < context.HitTestRadius) {
          return current.Owner as IBend;
        }

        var edgeThickness = edge.Style is OctilinearEdgeStyle style ? style.Pen.Thickness : 1;
        var hitTestRadius = context.HitTestRadius + 0.5 * edgeThickness;
        var hitsLineSegment = queryLocation.HitsLineSegment(current.Location, next.Location, hitTestRadius);
        if (hitsLineSegment) {
          // Must be a bend because this is an octilinear segment
          return current.Owner as IBend;
        }
      }

      return null;
    }

    /// <summary>
    ///   Creates a point-based representation of the edge's octilinear path.
    /// </summary>
    /// <param name="edge">The edge for which to create the visual path points.</param>
    /// <returns>An array of path points with their respective owners (bends or ports).</returns>
    private static List<EdgePathPoint> GetVisualPathPoints(IEdge edge) {
      var visualPathPoints = new List<EdgePathPoint> { new(edge.SourcePort.GetLocation(), edge.SourcePort) };

      for (var i = 0; i < edge.Bends.Count; i++) {
        var bend = edge.Bends[i];
        var bendLocation = Utils.GetLocation(bend);

        IModelItem prevItem = i == 0 ? edge.SourcePort : edge.Bends[i - 1];
        var prevLocation = Utils.GetLocation(prevItem);

        var currentCuttingLength = Utils.GetVisualCuttingLength(bend);
        var cornerType = Utils.GetCornerType(bend);

        void AddPoint(double x1, double y1, double x2, double y2) {
          var p1 = new PointD(bendLocation.X + x1, bendLocation.Y + y1);
          var p2 = new PointD(bendLocation.X + x2, bendLocation.Y + y2);

          // Ensure insertion order along the path
          if (p1.X == prevLocation.X || p1.Y == prevLocation.Y) {
            visualPathPoints.Add(new EdgePathPoint(p1, bend));
            visualPathPoints.Add(new EdgePathPoint(p2, bend));
          } else {
            visualPathPoints.Add(new EdgePathPoint(p2, bend));
            visualPathPoints.Add(new EdgePathPoint(p1, bend));
          }
        }

        switch (cornerType) {
          case Corner.TopRight:
            AddPoint(-currentCuttingLength, 0, 0, currentCuttingLength);
            break;
          case Corner.BottomRight:
            AddPoint(0, -currentCuttingLength, -currentCuttingLength, 0);
            break;
          case Corner.BottomLeft:
            AddPoint(0, -currentCuttingLength, currentCuttingLength, 0);
            break;
          default:
            AddPoint(currentCuttingLength, 0, 0, currentCuttingLength);
            break;
        }
      }

      visualPathPoints.Add(new EdgePathPoint(edge.TargetPort.GetLocation(), edge.TargetPort));
      return visualPathPoints;
    }

    /// <summary>Returns a resize cursor matching the given octilinear corner segment.</summary>
    private static Cursor GetCursor(IInputModeContext context, Corner cornerType) {
      if (OctilinearSegmentPositionHandler.MoveBendRecognizer(context.CanvasControl,
          context.CanvasControl.LastPointerEvent)) {
        return Cursors.SizeAll;
      }

      return cornerType is Corner.TopLeft or Corner.BottomRight ? Cursors.SizeNWSE : Cursors.SizeNESW;
    }
  }
}
