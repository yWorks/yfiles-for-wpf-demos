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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Layout.IncrementalHierarchicalLayout
{
  /// <summary>
  ///   Helper class that moves a node and uses the location of the mouse
  ///   to determine the layer where the nodes should be moved to
  /// </summary>
  public sealed class LayerPositionHandler : ConstrainedPositionHandler
  {
    private readonly LayerVisualCreator layerVisualCreator;
    private readonly INode node;
    private readonly IMapper<INode, int> newLayerMapper;
    private readonly MutableRectangle targetBounds = new();
    private IRenderTreeElement renderTreeElement;

    public LayerPositionHandler(
        LayerVisualCreator layerVisualCreator,
        INode node,
        IPositionHandler positionHandler,
        IMapper<INode, int> newLayerMapper) : base(positionHandler) {
      this.layerVisualCreator = layerVisualCreator;
      this.node = node;
      this.newLayerMapper = newLayerMapper;
    }

    protected override void OnInitialized(IInputModeContext context, PointD originalLocation) {
      base.OnInitialized(context, originalLocation);
      var canvasControl = context.CanvasControl;
      // create the visual indicator
      UpdateTargetBounds(canvasControl.LastPointerEvent.Location);
      CreateIndicatorRect(canvasControl);
    }

    private void CreateIndicatorRect(CanvasControl canvasControl) {
      var renderTree = canvasControl.RenderTree;
      if (renderTreeElement != null) {
        renderTree.Remove(renderTreeElement);
      }
      // visualize it
      var rectangleRenderer = new RectangleRenderer((Pen) new Pen(Brushes.DarkGray, 1).GetAsFrozen(),
          (Brush) new SolidColorBrush(new Color { A = 36, R = 128, G = 128, B = 128 }).GetAsFrozen());
      renderTreeElement = renderTree.CreateElement(renderTree.BackgroundGroup, targetBounds, rectangleRenderer);
    }

    protected override void OnCanceled(IInputModeContext context, PointD originalLocation) {
      base.OnCanceled(context, originalLocation);
      if (renderTreeElement != null) {
        // clean up
        context.CanvasControl!.RenderTree.Remove(renderTreeElement);
        renderTreeElement = null;
      }
    }

    protected override void OnFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      base.OnFinished(context, originalLocation, newLocation);
      if (renderTreeElement != null) {
        var canvasControl = context.CanvasControl;
        // calculate the target layer
        var newLayer = UpdateTargetBounds(canvasControl.LastPointerEvent.Location);
        // clean up
        canvasControl.RenderTree.Remove(renderTreeElement);
        renderTreeElement = null;
        // and register the new layer for the node
        newLayerMapper[node] = newLayer;
      }
    }

    protected override void OnMoved(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      base.OnMoved(context, originalLocation, newLocation);
      if (renderTreeElement != null) {
        // update the bounds to highlight
        UpdateTargetBounds(context.CanvasControl.LastPointerEvent.Location);
      }
    }

    /// <summary>
    ///   Updates the target bounds of the currently hit layer.
    /// </summary>
    private int UpdateTargetBounds(PointD location) {
      var lastLayer = layerVisualCreator.GetLayer(location);
      var indicatedBounds = layerVisualCreator.GetLayerBounds(lastLayer);
      if (double.IsPositiveInfinity(indicatedBounds.Width) || double.IsPositiveInfinity(indicatedBounds.Height)) {
        indicatedBounds = RectD.Empty;
      }
      targetBounds.SetShape(indicatedBounds);
      return lastLayer;
    }

    protected override PointD ConstrainNewLocation(
        IInputModeContext context,
        PointD originalLocation,
        PointD newLocation) {
      // do not constrain...
      return newLocation;
    }
  }
}
