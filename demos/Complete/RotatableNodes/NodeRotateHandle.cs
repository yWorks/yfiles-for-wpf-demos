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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// A custom <see cref="IHandle"/> implementation needed for rotating a label.
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

    /// <summary>
    /// A cache of angles and nodes with those angles.
    /// </summary>
    /// <remarks>
    /// Used for same angle snapping.
    /// </remarks>
    private IEnumerable<Tuple<double, IEnumerable<INode>>> nodeAngles;

    /// <summary>
    /// The currently highlighted nodes for same angle snapping.
    /// </summary>
    private IEnumerable<INode> sameAngleHighlightedNodes;

    static NodeRotateHandle() {
      // Load the custom rotation cursor
      var cursorStream = Application.GetResourceStream(new Uri("Resources/rotate.cur", UriKind.Relative));
      cursor = cursorStream != null ? new Cursor(cursorStream.Stream) : Cursors.Cross;
    }

    public NodeRotateHandle(INode node, IReshapeHandler reshapeHandler, IInputModeContext inputModeContext) {
      this.node = node;
      this.reshapeHandler = reshapeHandler;
      this.inputModeContext = new DelegatingContext(inputModeContext);
      portHandles = new List<IHandle>();
    }

    /// <summary>
    /// Returns the current oriented rectangle for the given node.
    /// </summary>
    private static CachingOrientedRectangle GetOrientedRectangle(INode node) {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      return wrapper != null ? wrapper.GetRotatedLayout(node) : new CachingOrientedRectangle();
    }

    /// <summary>
    /// Gets or sets the threshold value the specifies whether the angle should snap to 
    /// the next multiple of <see cref="SnapStep"/> in degrees.
    /// </summary>
    /// <remarks>
    /// Set a value less than or equal to zero to disable this feature.
    /// </remarks>
    public double SnapDelta { get; set; }
      
    /// <summary>
    /// Gets or sets the steps in degrees to which rotation should snap to.
    /// </summary>
    public double SnapStep { get; set; }

    /// <summary>
    /// Gets or sets the snapping distance (in degrees) for snapping to the same angle as other visible nodes.
    /// </summary>
    /// <remarks>
    /// Rotation will snap to another node's rotation angle if the current angle differs from the other one by less than this.
    /// The default is 5.
    /// <para>
    /// Setting this to a non-positive value will disable same angle snapping.
    /// </para>
    /// </remarks>
    public double SnapToSameAngleDelta { get; set; }

    /// <summary>
    /// The type of handle which is used.
    /// </summary>
    /// <remarks>
    /// Always returns <see cref="HandleTypes.Move"/>.
    /// </remarks>
    public HandleTypes Type
    {
      get { return HandleTypes.Move; }
    }

    /// <summary>
    /// The cursor that is shown when using this handle.
    /// </summary>
    public Cursor Cursor
    {
      get { return cursor; }
    }

    /// <summary>
    /// The location of the handle.
    /// </summary>
    /// <remarks>
    /// Since this instance also implements <see cref="IPoint"/>, we can simply return this.
    /// </remarks>
    public IPoint Location
    {
      get { return this; }
    }

    /// <summary>
    /// Initializes the drag.
    /// </summary>
    public void InitializeDrag(IInputModeContext context) {
      var imc = context.Lookup<IModelItemCollector>();
      if (imc != null) {
        imc.Add(node);
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
      // Collect other visible nodes and their angles
      if (SnapToSameAngleDelta > 0) {
        var canvas = context.CanvasControl;
        var rotatedNodes =
            canvas.GetCanvasObjects()
                  .Select(co => co.UserObject)
                  // only collect nodes
                  .OfType<INode>()
                  // ... that are in the viewport
                  .Where(n => canvas.Viewport.Intersects(n.Layout.ToRectD()))
                  // ... and can be rotated
                  .Where(n => n.Style is RotatableNodeStyleDecorator)
                  // ... and are not *this* node
                  .Where(n => n != node);
        // Group nodes by identical angles
        nodeAngles =
            rotatedNodes.GroupBy(n => ((RotatableNodeStyleDecorator) n.Style).Angle)
                        .Select(nodes => Tuple.Create(nodes.Key, nodes.ToList().AsEnumerable())).ToList();
      }
    }

    /// <summary>
    /// Updates the node according to the moving handle.
    /// </summary>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // calculate the angle
      var vector = (newLocation - rotationCenter).Normalized;
      var angle = CalculateAngle(vector);
      if (ShouldSnap(context)) {
        angle = SnapAngle(context, angle);
      }
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
    /// Returns the 'snapped' vector for the given up vector.
    /// </summary>
    /// <remarks>
    /// If the vector is almost horizontal or vertical, this method returns the exact horizontal
    /// or vertical up vector instead.
    /// </remarks>
    private static double CalculateAngle(PointD upVector) {
      return CachingOrientedRectangle.NormalizeAngle(-(Math.Atan2(upVector.Y, upVector.X) / Math.PI * 180 + 90));
    }

    /// <summary>
    /// Snaps the angle to the rotation angles of other nodes and the coordinate axes.
    /// </summary>
    /// <remarks>
    /// Angles near such an angle are replaced with this angle.
    /// </remarks>
    private double SnapAngle(IInputModeContext context, double angle) {
      // Check for disabled snapping
      var snapContext = context.Lookup<SnapContext>();
      if (snapContext != null && !snapContext.Enabled) {
        return angle;
      }
      // Same angle snapping
      if (SnapToSameAngleDelta > 0 && nodeAngles != null) {
        // Find the first angle that is sufficiently similar
        var candidate = nodeAngles.Where(na => CachingOrientedRectangle.NormalizeAngle(Math.Abs(na.Item1 - angle)) < SnapToSameAngleDelta).OrderBy(na => na.Item1).FirstOrDefault();
        if (candidate != null) {
          // Add highlight to every matching node
          var canvas = (GraphControl) context.CanvasControl;
          if (sameAngleHighlightedNodes != candidate.Item2) {
            ClearSameAngleHighlights(context);
          }
          foreach (var matchingNode in candidate.Item2) {
            canvas.HighlightIndicatorManager.AddHighlight(matchingNode);
          }
          sameAngleHighlightedNodes = candidate.Item2;
          return candidate.Item1;
        }
        ClearSameAngleHighlights(context);
      }
      if (SnapDelta <= 0.0 || SnapStep == 0) {
        return angle;
      }
      var mod = Math.Abs(angle % SnapStep);
      return (mod < SnapDelta || mod > SnapStep - SnapDelta) 
        ? SnapStep * Math.Round(angle / SnapStep)
        : angle;
    }

    /// <summary>
    /// Cancels the drag and cleans up.
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
      nodeAngles = null;
      ClearSameAngleHighlights(context);
    }

    /// <summary>
    /// Finishes the drag and updates the angle of the rotated node.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      var vector = (newLocation - rotationCenter).Normalized;

      var angle = CalculateAngle(vector);
      if (ShouldSnap(context)) {
        angle = SnapAngle(context, angle);
      }
      SetAngle(context, angle);

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

      nodeAngles = null;
      ClearSameAngleHighlights(context);
    }

    /// <summary>
    /// Removes highlights for same angle snapping.
    /// </summary>
    private void ClearSameAngleHighlights(IInputModeContext context) {
      if (sameAngleHighlightedNodes != null) {
        foreach (var highlightedNode in sameAngleHighlightedNodes) {
          ((GraphControl) context.CanvasControl).HighlightIndicatorManager.RemoveHighlight(highlightedNode);
        }
        sameAngleHighlightedNodes = null;
      }
    }

    /// <summary>
    /// Sets the angle to the node style if the style supports this.
    /// </summary>
    private void SetAngle(IInputModeContext context, double angle) {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      if (wrapper != null) {
        var oldAngle = wrapper.Angle;
        context.GetGraph().AddUndoUnit("Change Angle", "Change Angle",
            () => wrapper.Angle = oldAngle,
            () => wrapper.Angle = angle);
        wrapper.Angle = angle;
      }
    }
      
    /// <summary>
    /// Reads the angle from the node style if the style supports this.
    /// </summary>
    /// <returns></returns>
    private double GetAngle() {
      var wrapper = node.Style as RotatableNodeStyleDecorator;
      return wrapper != null ? wrapper.Angle : 0;
    }

    /// <summary>
    /// Whether the current gesture does not disable snapping.
    /// </summary>
    /// <returns>true if snapping is not temporarily disabled.</returns>
    private bool ShouldSnap(IInputModeContext context) {
      var shouldSnap = (context.CanvasControl.LastMouse2DEvent.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift;
      if (!shouldSnap && sameAngleHighlightedNodes != null) {
        ClearSameAngleHighlights(context);
      }
      return shouldSnap;
    }

    #region IPoint members

    /// <summary>
    /// Returns the x-coordinate of the handle's location.
    /// </summary>
    public double X {
      get { return GetLocation().X; }
    }

    /// <summary>
    /// Returns the y-coordinate of the handle's location.
    /// </summary>
    public double Y {
      get { return GetLocation().Y; }
    }

    /// <summary>
    /// Returns the handle's location.
    /// </summary>
    private PointD GetLocation() {
      var orientedRectangle = GetOrientedRectangle(node);
      var anchor = orientedRectangle.GetAnchorLocation();
      var size = orientedRectangle.ToSizeD();
      var up = orientedRectangle.GetUp();
      // calculate the location of the handle from the anchor, the size and the orientation
      var offset = inputModeContext != null ? 20/inputModeContext.CanvasControl.Zoom : 20;
      var location = anchor + up*(size.Height + offset) +
                     new PointD(-up.Y, up.X)*(size.Width*0.5);
      return location;
    }

    #endregion

    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }
  }
}
