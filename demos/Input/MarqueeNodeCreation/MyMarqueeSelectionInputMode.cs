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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.MarqueeNodeCreation
{
  public class MyMarqueeSelectionInputMode : MarqueeSelectionInputMode
  {
    private readonly SimpleNode dummyNode;

    // determines in which quadrant the marquee is dragged
    private HandlePositions currentReshapePosition;

    public MyMarqueeSelectionInputMode(INodeStyle nodeStyle) {
      Priority = 50;
      dummyNode = new SimpleNode { Style = nodeStyle };

      currentReshapePosition = HandlePositions.None;

      UseViewCoordinates = false;

      // customizing the marquee rect visualization to look like the current node
      MarqueeRenderer = new MyMarqueeTemplate(nodeStyle);
    }

    /// <summary>
    ///   Creates a rectangle that incorporates the snap result
    /// </summary>
    protected override RectD CalculateMarqueeRectangle(PointD startDragLocation, PointD currentDragLocation) {
      currentReshapePosition = HandlePositions.None;
      if (startDragLocation.X > currentDragLocation.X) {
        currentReshapePosition |= HandlePositions.Left;
      } else {
        currentReshapePosition |= HandlePositions.Right;
      }
      if (startDragLocation.Y > currentDragLocation.Y) {
        currentReshapePosition |= HandlePositions.Top;
      } else {
        currentReshapePosition |= HandlePositions.Bottom;
      }

      var snapState = GetSnapContext().HandleMove(currentDragLocation, false);
      var rect = base.CalculateMarqueeRectangle(startDragLocation, snapState);

      dummyNode.Layout = rect;

      GetSnapContext().Dragged(currentDragLocation, snapState);
      return rect;
    }

    /// <summary>
    ///   Called when a drag is started. Prepares the SnapContext for collecting results that the marquee can be snapped to.
    /// </summary>
    protected override void OnDragStarted(MarqueeSelectionEventArgs evt) {
      base.OnDragStarted(evt);

      var snapContext = GetSnapContext();

      snapContext.InitializeDrag(new InputModeContext(ParentInputModeContext, this), SelectionRectangle.GetTopLeft());

      snapContext.AddItemToBeReshaped(dummyNode);

      snapContext.DragInitialized();

      dummyNode.Layout = evt.Rectangle;

      var initialBounds = evt.Rectangle;

      var lastEventLocation = ParentInputModeContext!.CanvasControl!.LastPointerEvent.Location;
      currentReshapePosition = GetReshapePosition(lastEventLocation, evt.Rectangle);

      GetSnapContext().AddItemToBeReshaped(dummyNode);

      EventHandler<CollectSnapResultsEventArgs> collectSnapResultsEventListener = (_, args) => {
        var reshapePosition = currentReshapePosition;
        var topLeftChangeFactorX = (reshapePosition & HandlePositions.Left) == HandlePositions.Left ? 1 : 0;
        var topLeftChangeFactorY = (reshapePosition & HandlePositions.Top) == HandlePositions.Top ? 1 : 0;
        var bottomRightChangeFactorX = (reshapePosition & HandlePositions.Left) == HandlePositions.Left ? 0 : 1;
        var bottomRightChangeFactorY = (reshapePosition & HandlePositions.Top) == HandlePositions.Top ? 0 : 1;
        var sizeChangeFactorX = (reshapePosition & HandlePositions.Left) == HandlePositions.Left ? -1 : 1;
        var sizeChangeFactorY = (reshapePosition & HandlePositions.Top) == HandlePositions.Top ? -1 : 1;

        NodeReshapeSnapResultProvider.Instance.CollectSnapResults(snapContext, args, dummyNode,
            new ReshapeRectangleContext(initialBounds, SizeD.Zero, SizeD.Infinite, RectD.Empty, RectD.Infinite,
                reshapePosition, new PointD(topLeftChangeFactorX, topLeftChangeFactorY),
                new PointD(bottomRightChangeFactorX, bottomRightChangeFactorY),
                new SizeD(sizeChangeFactorX, sizeChangeFactorY), ReshapePolicy.None, 1));
      };
      snapContext.CollectSnapResults += collectSnapResultsEventListener;

      snapContext.CleanedUp += (_, _) => snapContext.CollectSnapResults -= collectSnapResultsEventListener;
    }

    /// <summary>
    ///   Signals the SnapContext that the drag is finished.
    /// </summary>
    protected override void OnDragFinished(MarqueeSelectionEventArgs evt) {
      var x = (currentReshapePosition & HandlePositions.Left) == HandlePositions.Left
          ? SelectionRectangle.X
          : SelectionRectangle.GetMaxX();
      var y = (currentReshapePosition & HandlePositions.Top) == HandlePositions.Top
          ? SelectionRectangle.Y
          : SelectionRectangle.GetMaxY();
      GetSnapContext().DragFinished(new PointD(x, y), false);
      base.OnDragFinished(evt);
    }

    /// <summary>
    ///   Signals the SnapContext that the drag has been canceled.
    /// </summary>
    protected override void OnDragCanceled(MarqueeSelectionEventArgs evt) {
      GetSnapContext().CancelDrag();
    }

    private GraphSnapContext? snapContext;

    /// <summary>
    ///   Looks up the current SnapContext or creates a new one
    /// </summary>
    private GraphSnapContext GetSnapContext() {
      if (snapContext != null) {
        return snapContext;
      }

      snapContext = ParentInputModeContext.Lookup<GraphSnapContext>();
      if (snapContext == null) {
        return new GraphSnapContext();
      }

      return snapContext;
    }

    /// <summary>
    ///   Determines the reshape position
    /// </summary>
    private HandlePositions GetReshapePosition(PointD pointer, RectD rect) {
      var result = HandlePositions.None;
      if (pointer.X < rect.GetCenterX()) {
        currentReshapePosition |= HandlePositions.Left;
      } else {
        currentReshapePosition |= HandlePositions.Right;
      }
      if (pointer.Y < rect.GetCenterY()) {
        currentReshapePosition |= HandlePositions.Top;
      } else {
        currentReshapePosition |= HandlePositions.Bottom;
      }
      return result;
    }
  }

  public class MyMarqueeTemplate : ObjectRendererBase<MarqueeRenderTag, Visual>
  {
    private readonly INodeStyle nodeStyle;
    private readonly SimpleNode dummyNode;

    public MyMarqueeTemplate(INodeStyle nodeStyle) {
      this.nodeStyle = nodeStyle;
      dummyNode = new SimpleNode { Style = nodeStyle };
    }

    protected override Visual CreateVisual(IRenderContext context, MarqueeRenderTag renderTag) {
      dummyNode.Layout = renderTag.SelectionRectangle;
      var visual = nodeStyle.Renderer.GetVisualCreator(dummyNode, nodeStyle).CreateVisual(context);
      if (visual == null) {
        return null!;
      }

      var preview = new VisualGroup { Opacity = 0.3 };
      preview.Children.Add(visual);
      return preview;
    }

    protected override Visual UpdateVisual(IRenderContext context, Visual oldVisual, MarqueeRenderTag renderTag) =>
        CreateVisual(context, renderTag);
  }
}
