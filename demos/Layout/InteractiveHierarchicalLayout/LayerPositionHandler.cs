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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.InteractiveHierarchical
{
  /// <summary>
  ///   Helper class that moves a node and uses the location of the mouse
  ///   to determine the layer where the nodes should be moved to.
  /// </summary>
  internal sealed class LayerPositionHandler : ConstrainedPositionHandler
  {
    private readonly Dictionary<INode, int> newLayerMapper;
    private readonly INode node;
    private readonly LayerVisual layerVisual;
    private IRenderTreeElement? renderTreeElement;
    private readonly LayerIndicatorVisual layerIndicatorVisual = new();
    private CanvasControl? canvasControl;

    /// <summary>
    ///   Creates a new instance that wraps the base handler.
    /// </summary>
    internal LayerPositionHandler(
        IPositionHandler baseHandler,
        LayerVisual layerVisual,
        INode node,
        Dictionary<INode, int> newLayerMapper) : base(baseHandler) {
      this.layerVisual = layerVisual;
      this.node = node;
      this.newLayerMapper = newLayerMapper;
    }

    /// <summary>
    ///   Called when a node drag started.
    ///   This adds a rectangle which highlights the layer into which the node would be currently dropped.
    /// </summary>
    protected override void OnInitialized(IInputModeContext inputModeContext, PointD originalLocation) {
      base.OnInitialized(inputModeContext, originalLocation);

      // create the visual indicator
      canvasControl = inputModeContext.CanvasControl;
      renderTreeElement = canvasControl.RenderTree.CreateElement(
          canvasControl.RenderTree.BackgroundGroup, layerIndicatorVisual);
      UpdateTargetBounds(canvasControl.LastPointerEvent.Location);
    }

    /// <summary>
    ///   Called when a node drag was canceled.
    ///   The highlighting rectangle is removed.
    /// </summary>
    protected override void OnCanceled(IInputModeContext context, PointD originalLocation) {
      base.OnCanceled(context, originalLocation);
      // clean up
      if (renderTreeElement != null) {
        context.CanvasControl.RenderTree.Remove(renderTreeElement);
        renderTreeElement = null;
      }
      canvasControl = null;
    }

    /// <summary>
    ///   Called when a node drag was finished.
    ///   The layer is updated for the moved node and the highlighting rectangle is removed.
    /// </summary>
    protected override void OnFinished(
        IInputModeContext inputModeContext,
        PointD originalLocation,
        PointD newLocation) {
      base.OnFinished(inputModeContext, originalLocation, newLocation);
      // calculate the target layer
      var newLayer = UpdateTargetBounds(inputModeContext.CanvasControl.LastPointerEvent.Location);
      // clean up
      if (renderTreeElement != null) {
        inputModeContext.CanvasControl.RenderTree.Remove(renderTreeElement);
        renderTreeElement = null;
      }
      canvasControl = null;
      newLayerMapper[node] = newLayer;
    }

    /// <summary>
    ///   Updates the highlighting rectangle while the node is moved.
    /// </summary>
    protected override void OnMoved(IInputModeContext inputModeContext, PointD originalLocation, PointD newLocation) {
      base.OnMoved(inputModeContext, originalLocation, newLocation);
      // update the bounds to highlight
      UpdateTargetBounds(inputModeContext.CanvasControl.LastPointerEvent.Location);
    }

    /// <summary>
    ///   Does nothing because the location should not be constrained.
    /// </summary>
    protected override PointD ConstrainNewLocation(
        IInputModeContext context,
        PointD originalLocation,
        PointD newLocation) =>
        newLocation;

    /// <summary>
    ///   Updates the target bounds of the currently hit layer.
    /// </summary>
    private int UpdateTargetBounds(PointD location) {
      var lastLayer = layerVisual.GetLayer(location);
      layerIndicatorVisual.Bounds = layerVisual.GetLayerBounds(lastLayer);
      canvasControl?.Invalidate();
      return lastLayer;
    }
  }

  /// <summary>
  ///   Visual that presents a rectangle which marks the current layer to drop a node.
  /// </summary>
  internal sealed class LayerIndicatorVisual : IVisualCreator
  {
    internal RectD Bounds { get; set; } = RectD.Empty;

    public Visual CreateVisual(IRenderContext context) => Create();

    public Visual? UpdateVisual(IRenderContext context, Visual? oldVisual) => Create();

    private DrawingVisual Create() {
      var visual = new DrawingVisual();
      using var drawing = visual.RenderOpen();
      var fill = new SolidColorBrush(Color.FromArgb(26, 80, 80, 80));
      fill.Freeze();
      var pen = new Pen(Brushes.DarkGray, 1);
      pen.Freeze();
      drawing.DrawRectangle(fill, pen, new Rect(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height));
      return visual;
    }
  }
}
