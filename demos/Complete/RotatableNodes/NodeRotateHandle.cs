/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  ///   A custom <see cref="IHandle"/> implementation needed for rotating a label.
  /// </summary>
  public class NodeRotateHandle : IHandle, IPoint
  {
    private static readonly Cursor cursor;

    private PointD rotationCenter;
    private double initialAngle;

    private readonly INode node;
    private readonly IReshapeHandler reshapeHandler;
    private readonly IInputModeContext inputModeContext;
    private readonly List<IHandle> portHandles;

    private ICompoundEdit compoundEdit;

    private IInputModeContext context;

    static NodeRotateHandle() {
      // Load the custom rotation cursor
      var cursorStream = Application.GetResourceStream(new Uri("Resources/rotate.cur", UriKind.Relative));
      cursor = cursorStream != null ? new Cursor(cursorStream.Stream) : Cursors.Cross;
    }

    public NodeRotateHandle(INode node, IReshapeHandler reshapeHandler, IInputModeContext inputModeContext) {
      this.node = node;
      this.reshapeHandler = reshapeHandler;
      this.inputModeContext = inputModeContext;
      portHandles = new List<IHandle>();
    }

    /// <summary>
    ///   Returns the current oriented rectangle for the given node.
    /// </summary>
    private static CachingOrientedRectangle GetOrientedRectangle(INode node) {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      return wrapper != null ? wrapper.GetRotatedLayout(node) : new CachingOrientedRectangle();
    }

    /// <summary>
    ///   Gets or sets the threshold value that specifies whether the angle should snap to
    ///   the next multiple of <see cref="SnapStep"/> in degrees.
    /// </summary>
    /// <remarks>
    ///   Set a value less than or equal to zero to disable this feature.
    /// </remarks>
    public double SnapDelta { get; set; }

    /// <summary>
    ///   Gets or sets the steps in degrees to which rotation should snap to.
    /// </summary>
    public double SnapStep { get; set; }

    /// <summary>
    ///   Gets or sets the snapping distance (in degrees) for snapping to the same angle as other visible nodes.
    /// </summary>
    /// <remarks>
    ///   Rotation will snap to another node's rotation angle if the current angle differs from the other one by less than
    ///   this.
    ///   The default is 5.
    ///   <para>
    ///     Setting this to a non-positive value will disable same angle snapping.
    ///   </para>
    /// </remarks>
    public double SnapToSameAngleDelta { get; set; }

    /// <summary>
    ///   The type of handle that is used.
    /// </summary>
    /// <remarks>
    ///   Always returns <see cref="HandleType.Move"/>.
    /// </remarks>
    public HandleType Type => HandleType.Move;

    /// <summary>
    ///   Gets the optional tag object associated with the handle.
    /// </summary>
    public object Tag => null;

    /// <summary>
    ///   The cursor that is shown when using this handle.
    /// </summary>
    public Cursor Cursor => cursor;

    /// <summary>
    ///   The location of the handle.
    /// </summary>
    /// <remarks>
    ///   Since this instance also implements <see cref="IPoint"/>, we can simply return this.
    /// </remarks>
    public IPoint Location => this;

    /// <summary>
    ///   Initializes the drag.
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      this.context = context;
      var imc = context.Lookup<IModelItemCollector>();
      if (imc != null) {
        imc.Add(node);
      }

      // initialize snapping
      if (context.Lookup<GraphSnapContext>() is { IsInitializing: true } snapContext) {
        snapContext.CollectSnapReferences += CollectSnapReferences;
        snapContext.CollectSnapResults += CollectSnapResults;
      }

      rotationCenter = node.Layout.GetCenter();
      initialAngle = GetAngle();

      var graph = context.Lookup<IGraph>();
      if (graph != null) {
        compoundEdit = graph.BeginEdit("Change Rotation Angle", "Change Rotation Angle");
      }

      portHandles.Clear();
      var portContext = new DelegatingContext(context);
      foreach (var port in node.Ports) {
        var portHandle = new DummyPortLocationModelParameterHandle(port);
        portHandle.InitializeDrag(portContext);
        portHandles.Add(portHandle);
      }
      if (reshapeHandler != null) {
        reshapeHandler.InitializeReshape(context);
      }
    }

    /// <summary>
    ///   Updates the node according to the moving handle.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // calculate the angle
      var vector = (newLocation - rotationCenter).Normalized;
      var angle = CalculateAngle(vector);
      SetAngle(context, angle);

      var portContext = new DelegatingContext(context);
      foreach (var portHandle in portHandles) {
        portHandle.HandleMove(portContext, originalLocation, newLocation);
      }
      if (reshapeHandler != null) {
        reshapeHandler.HandleReshape(context, node.Layout.ToRectD(), node.Layout.ToRectD());
      }
    }

    /// <summary>
    ///   Returns the 'snapped' vector for the given up vector.
    /// </summary>
    /// <remarks>
    ///   If the vector is almost horizontal or vertical, this method returns the exact horizontal
    ///   or vertical up vector instead.
    /// </remarks>
    private static double CalculateAngle(PointD upVector) {
      return CachingOrientedRectangle.NormalizeAngle(Math.Atan2(upVector.Y, upVector.X) / Math.PI * 180 + 90);
    }

    private static PointD Rotate(PointD vector, double angleInDegree) {
      var angle = angleInDegree / 180 * Math.PI;

      var rotatedDx = Math.Cos(angle) * vector.X - Math.Sin(angle) * vector.Y;
      var rotatedDy = Math.Sin(angle) * vector.X + Math.Cos(angle) * vector.Y;
      return new PointD(rotatedDx, rotatedDy);
    }

    /// <summary>
    ///   Cancels the drag and cleans up.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      SetAngle(context, initialAngle);

      var portContext = new DelegatingContext(context);
      foreach (var portHandle in portHandles) {
        portHandle.CancelDrag(portContext, originalLocation);
      }
      portHandles.Clear();
      if (reshapeHandler != null) {
        reshapeHandler.CancelReshape(context, node.Layout.ToRectD());
      }
      if (compoundEdit != null) {
        compoundEdit.Cancel();
      }
      RemoveFromContext(context);
    }

    /// <summary>
    ///   Finishes the drag and updates the angle of the rotated node.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var vector = (newLocation - rotationCenter).Normalized;
      var angle = CalculateAngle(vector);
      SetAngle(context, angle);

      RemoveFromContext(context);

      // Switch width / height for 'vertical' rotations
      // Note that other parts of the application need support for this feature, too.
      var graph = context.GetGraph();
      if (graph == null) {
        return;
      }

      var portContext = new DelegatingContext(context);
      foreach (var portHandle in portHandles) {
        portHandle.DragFinished(portContext, originalLocation, newLocation);
      }
      portHandles.Clear();

      // Workaround: if the OrthogonalEdgeEditingContext is used to keep the edges orthogonal, it is not allowed
      // to change that edges manually. Therefore, we explicitly finish the OrthogonalEdgeEditingContext here and
      // then call the edge router.
      var edgeEditingContext = context.Lookup<OrthogonalEdgeEditingContext>();
      if (edgeEditingContext != null && edgeEditingContext.IsInitialized) {
        edgeEditingContext.DragFinished();
      }

      if (reshapeHandler != null) {
        reshapeHandler.ReshapeFinished(context, node.Layout.ToRectD(), node.Layout.ToRectD());
      }

      if (compoundEdit != null) {
        compoundEdit.Commit();
      }
    }

    /// <summary>
    ///   Sets the angle to the node style if the style supports this.
    /// </summary>
    private void SetAngle(IInputModeContext context, double angle) {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      if (wrapper != null) {
        var oldAngle = wrapper.Angle;
        context.GetGraph().AddUndoUnit("Change Angle", "Change Angle", () => wrapper.Angle = oldAngle,
            () => wrapper.Angle = angle);
        wrapper.Angle = angle;
      }
    }

    /// <summary>
    ///   Reads the angle from the node style if the style supports this.
    /// </summary>
    private double GetAngle() {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      return wrapper != null ? wrapper.Angle : 0;
    }

    #region IPoint members

    /// <summary>
    ///   Returns the x-coordinate of the handle's location.
    /// </summary>
    public double X => GetLocation().X;

    /// <summary>
    ///   Returns the y-coordinate of the handle's location.
    /// </summary>
    public double Y => GetLocation().Y;

    /// <summary>
    ///   Returns the handle's location.
    /// </summary>
    private PointD GetLocation() {
      var orientedRectangle = GetOrientedRectangle(node);
      var anchor = orientedRectangle.GetAnchor();
      var size = orientedRectangle.ToSizeD();
      var up = orientedRectangle.GetUpVector();
      // calculate the location of the handle from the anchor, the size and the orientation
      var offset = inputModeContext != null ? 20 / inputModeContext.CanvasControl.Zoom : 20;
      var location = anchor + up * (size.Height + offset) + new PointD(-up.Y, up.X) * (size.Width * 0.5);
      return location;
    }

    #endregion

    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }

    #region Angle Snapping

    private void RemoveFromContext(IInputModeContext inputModeContext) {
      var snapContext = inputModeContext.Lookup<GraphSnapContext>();
      if (snapContext != null) {
        snapContext.CollectSnapReferences -= CollectSnapReferences;
        snapContext.CollectSnapResults -= CollectSnapResults;
      }
    }

    private void CollectSnapReferences(object sender, CollectSnapReferencesEventArgs e) {
      // Collect other visible nodes and their angles
      if (SnapToSameAngleDelta > 0 && context.CanvasControl is GraphControl canvas) {
        // only collect nodes
        var rotatedNodes = canvas.Graph.Nodes
            // ... that are in the viewport
            .Where(n => canvas.Viewport.Intersects(n.Layout.ToRectD()))
            // ... and can be rotated
            .Where(n => n.Style is RotatableNodeStyleDecorator)
            // ... and are not *this* node
            .Where(n => n != node);
        // Group nodes by identical angles
        var nodeAngles = rotatedNodes.GroupBy(n => ((RotatableNodeStyleDecorator) n.Style).Angle)
            .Select(nodes => Tuple.Create(nodes.Key, nodes.ToList().AsEnumerable())).ToList();
        if (nodeAngles.Count == 0) {
          return;
        }
        foreach (var (angle, nodes) in nodeAngles) {
          e.AddSnapReference(new SnapRotationAngle(angle, nodes, 10, GraphItemTypes.Node, null));
        }
      }

      // collect snap step angles
      if (SnapDelta > 0 && SnapStep > 0) {
        for (var a = 0.0; a < 360; a += SnapStep) {
          e.AddSnapReference(new SnapRotationAngle(a, Enumerable.Empty<INode>(), 10, GraphItemTypes.Node, null));
        }
      }
    }

    private void CollectSnapResults(object sender, CollectSnapResultsEventArgs e) {
      var graphSnapContext = sender as GraphSnapContext;

      // calculate the angle
      var centerToLocation = e.NewLocation - rotationCenter;
      var vector = centerToLocation.Normalized;
      var angle = CalculateAngle(vector);

      var closestAngleDelta = 1000D;
      SnapRotationAngle closestSnapRotationAngle = null;

      var snapReferences = graphSnapContext.SnapReferences.Where(snapLine =>
          (snapLine.SnappableItems & GraphItemTypesSupport.GetItemType(node)) != GraphItemTypes.None);
      foreach (var angleSnapReference in snapReferences.OfType<SnapRotationAngle>()) {
        // Find the closest angle that is in SnapToSameAngleDelta range or in SnapDelta range to an axis
        var angleDelta = AngleDifference(angleSnapReference.angle, angle);
        var isAxisReference = !angleSnapReference.nodes.Any();
        if (angleDelta <= 10) {
        }
        if (angleDelta < closestAngleDelta && (!isAxisReference && angleDelta < SnapToSameAngleDelta ||
            isAxisReference && angleDelta < SnapDelta)) {
          closestAngleDelta = angleDelta;
          closestSnapRotationAngle = angleSnapReference;
        }
      }
      if (closestSnapRotationAngle != null) {
        var snappedAngle = closestSnapRotationAngle.angle;
        var dAngle = snappedAngle - angle;

        var centerToNewLocation = Rotate(centerToLocation, dAngle);
        var newLocation = rotationCenter + centerToNewLocation;

        var constraint = SnapConstraint.CreatePointConstraint(newLocation);
        var rotationSnapResult = SnapResults.CreateSnapResult(constraint, closestSnapRotationAngle.Weight, snappedAngle,
            node, null, closestSnapRotationAngle, RotatedNodesSnapRenderer.Instance);
        e.AddSnapResult(rotationSnapResult);
      }
    }

    private static double AngleDifference(double angle1, double angle2) {
      return Math.Min(Math.Abs(angle1 - angle2), 360 - Math.Abs(angle1 - angle2));
    }

    internal class SnapRotationAngle : SnapReference
    {
      /// <summary>
      ///   The angle to snap to.
      /// </summary>
      public readonly double angle;

      /// <summary>
      ///   The nodes that are rotated by <see cref="angle"/>.
      /// </summary>
      public readonly IEnumerable<INode> nodes;

      public SnapRotationAngle(
          double angle,
          IEnumerable<INode> nodes,
          double weight,
          GraphItemTypes snappableItems,
          object tag) : base(weight, snappableItems, tag) {
        this.angle = angle;
        this.nodes = nodes;
      }
    }

    #endregion
  }
}
