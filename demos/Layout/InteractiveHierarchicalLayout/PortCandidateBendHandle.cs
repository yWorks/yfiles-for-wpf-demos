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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;

namespace yWorks.Demo.InteractiveHierarchical
{
  /// <summary>
  ///   Helper class that provides a handle for the first and last bend of an edge
  ///   that interactively determines the port candidate.
  /// </summary>
  internal sealed class PortCandidateBendHandle : ConstrainedHandle, IVisualCreator
  {
    private const double MinDistance = 12;
    private const double ArrowOffset = 11;

    private static readonly PointD[] ArrowPath = [
        new(-15, 0), new(-5, 10), new(-2, 7), new(-5, 4), new(8, 4), new(8, -4),
        new(-5, -4), new(-2, -7), new(-5, -10)
    ];

    private readonly IDictionary<IEdge, EdgePortCandidates> portCandidates;
    private readonly IBend bend;
    private readonly bool sourceEnd;
    private IRenderTreeElement? renderTreeElement;
    private CanvasControl? canvasControl;

    /// <summary>
    ///   Creates a new handle that wraps the base handle.
    /// </summary>
    internal PortCandidateBendHandle(
        IHandle baseHandle,
        bool sourceEnd,
        IBend bend,
        IDictionary<IEdge, EdgePortCandidates> portCandidates) : base(baseHandle) {
      this.sourceEnd = sourceEnd;
      this.bend = bend;
      this.portCandidates = portCandidates;
    }

    /// <summary>
    ///   Called when a drag of the handle is initialized.
    ///   To indicate in which direction the port candidate will be assigned, an arrow visual is added.
    /// </summary>
    protected override void OnInitialized(IInputModeContext inputModeContext, PointD originalLocation) {
      base.OnInitialized(inputModeContext, originalLocation);
      // render the indicator
      canvasControl = inputModeContext.CanvasControl;
      renderTreeElement = canvasControl.RenderTree.CreateElement(canvasControl.RenderTree.RootGroup, this);
    }

    /// <summary>
    ///   Called when a drag of the handle is canceled.
    ///   The arrow visual is removed.
    /// </summary>
    protected override void OnCanceled(IInputModeContext inputModeContext, PointD originalLocation) {
      base.OnCanceled(inputModeContext, originalLocation);
      // remove the indicator
      RemoveIndicator(inputModeContext);
    }

    /// <summary>
    ///   Called when a drag of the handle is finished.
    ///   The port candidates are assigned and the arrow visual is removed.
    /// </summary>
    protected override void OnFinished(
        IInputModeContext inputModeContext,
        PointD originalLocation,
        PointD newLocation) {
      base.OnFinished(inputModeContext, originalLocation, newLocation);
      // remove the indicator
      RemoveIndicator(inputModeContext);

      // calculate the direction
      var node = GetNode();
      var nodeLayout = node.Layout;
      var portLocation = nodeLayout.GetCenter();
      var bendLocation = bend.Location.ToPointD();
      var delta = bendLocation - portLocation;
      EdgePortCandidates? candidates = null;
      if (delta.VectorLength > MinDistance && !nodeLayout.Contains(bendLocation)) {
        var direction = delta.Normalized;
        var side = direction.IsHorizontalVector ? direction.X > 0 ? PortSides.Right : PortSides.Left :
            direction.Y > 0 ? PortSides.Bottom : PortSides.Top;
        candidates = new EdgePortCandidates().AddFreeCandidate(side);
      }

      // and set the port candidate
      if (candidates == null) {
        portCandidates.Remove(bend.Owner);
      } else {
        portCandidates[bend.Owner] = candidates;
      }
    }

    /// <summary>
    ///   Returns the unconstrained location.
    /// </summary>
    protected override PointD ConstrainNewLocation(
        IInputModeContext context,
        PointD originalLocation,
        PointD newLocation) =>
        newLocation;

    /// <summary>
    ///   Creates a visual that contains an arrow visualization for the direction of port candidates.
    /// </summary>
    public Visual CreateVisual(IRenderContext context) => CreateArrowVisual();

    /// <summary>
    ///   Updates the arrow visualization for the direction of port candidates.
    /// </summary>
    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => CreateArrowVisual();

    private DrawingVisual CreateArrowVisual() {
      var visual = new DrawingVisual();
      var nodeLayout = GetNode().Layout;
      var center = nodeLayout.GetCenter();
      var bendLocation = new PointD(bend.Location.X, bend.Location.Y);
      var delta = bendLocation - center;
      if (delta.VectorLength <= MinDistance || nodeLayout.Contains(bendLocation)) {
        return visual;
      }

      var direction = delta.Normalized;
      var anchor = center;
      PointD xAxis;
      PointD yAxis;
      if (direction.IsHorizontalVector) {
        if (direction.X > 0) {
          anchor = new PointD(nodeLayout.GetMaxX() + ArrowOffset, center.Y);
          xAxis = sourceEnd ? new PointD(-1, 0) : new PointD(1, 0);
          yAxis = new PointD(0, 1);
        } else {
          anchor = new PointD(nodeLayout.X - ArrowOffset, center.Y);
          xAxis = sourceEnd ? new PointD(1, 0) : new PointD(-1, 0);
          yAxis = sourceEnd ? new PointD(0, -1) : new PointD(0, 1);
        }
      } else if (direction.Y < 0) {
        anchor = new PointD(center.X, nodeLayout.Y - ArrowOffset);
        xAxis = new PointD(0, 1);
        yAxis = sourceEnd ? new PointD(1, 0) : new PointD(-1, 0);
      } else {
        anchor = new PointD(center.X, nodeLayout.GetMaxY() + ArrowOffset);
        xAxis = new PointD(0, -1);
        yAxis = sourceEnd ? new PointD(-1, 0) : new PointD(1, 0);
      }

      PointD Transform(PointD point) =>
          new(anchor.X + xAxis.X * point.X + yAxis.X * point.Y, anchor.Y + xAxis.Y * point.X + yAxis.Y * point.Y);

      var geometry = new StreamGeometry();
      using (var path = geometry.Open()) {
        var first = Transform(ArrowPath[0]);
        path.BeginFigure(new Point(first.X, first.Y), true, true);
        foreach (var point in ArrowPath.Skip(1)) {
          var transformed = Transform(point);
          path.LineTo(new Point(transformed.X, transformed.Y), true, false);
        }
      }
      geometry.Freeze();
      using var drawing = visual.RenderOpen();
      drawing.DrawGeometry(Brushes.Green, null, geometry);
      return visual;
    }

    private void RemoveIndicator(IInputModeContext context) {
      if (renderTreeElement != null) {
        context.CanvasControl.RenderTree.Remove(renderTreeElement);
        renderTreeElement = null;
      }
      canvasControl = null;
    }

    private INode GetNode() => (INode) (sourceEnd ? bend.Owner.SourcePort.Owner : bend.Owner.TargetPort.Owner);
  }
}
