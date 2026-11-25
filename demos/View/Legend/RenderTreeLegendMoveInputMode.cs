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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.Legend;

/// <summary>
///   A move input mode for a render-tree legend.
/// </summary>
internal sealed class RenderTreeLegendMoveInputMode : MoveInputMode
{
  private readonly GraphControl graphControl;
  private readonly Func<RectD?> getBounds;
  private readonly Func<PointD> getPosition;
  private readonly Action<PointD> setPosition;
  private readonly bool useViewTransform;
  private RenderTreeLegendPositionHandler? handler;

  public RenderTreeLegendMoveInputMode(
      GraphControl graphControl,
      Func<RectD?> getBounds,
      Func<PointD> getPosition,
      Action<PointD> setPosition,
      bool useViewTransform) {
    this.graphControl = graphControl;
    this.getBounds = getBounds;
    this.getPosition = getPosition;
    this.setPosition = setPosition;
    this.useViewTransform = useViewTransform;
    HitTestable = new LegendHitTestable(this);
    PositionHandler = new DelegatingPositionHandler(() => handler);
    Priority = 10;
  }

  private bool IsValidHit(IInputModeContext context, PointD location) {
    var bounds = getBounds();
    // input location to test is in world coordinates that we might need to transform
    var hitLocation = useViewTransform ? graphControl.WorldToViewCoordinates(location) : location;
    // apply the hit-test radius in view coordinates
    var hitTestRadius = useViewTransform ? context.HitTestRadius * graphControl.Zoom : context.HitTestRadius;
    handler = bounds is { } rect && rect.Contains(hitLocation, hitTestRadius)
        ? handler = new RenderTreeLegendPositionHandler(graphControl, getPosition, setPosition, location,
            useViewTransform)
        : null;
    return handler != null;
  }

  private sealed class LegendHitTestable(RenderTreeLegendMoveInputMode owner) : IHitTestable
  {
    public bool IsHit(IInputModeContext context, PointD location) => owner.IsValidHit(context, location);
  }

  private sealed class DelegatingPositionHandler(Func<RenderTreeLegendPositionHandler?> getHandler) : IPositionHandler
  {
    public IPoint Location => getHandler()?.Location ?? PointD.Origin;

    public void InitializeDrag(IInputModeContext context) => getHandler()?.InitializeDrag(context);

    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
        getHandler()?.HandleMove(context, originalLocation, newLocation);

    public void CancelDrag(IInputModeContext context, PointD originalLocation) =>
        getHandler()?.CancelDrag(context, originalLocation);

    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
        getHandler()?.DragFinished(context, originalLocation, newLocation);
  }
}

internal sealed class RenderTreeLegendPositionHandler : IPositionHandler
{
  private readonly GraphControl graphControl;
  private readonly Func<PointD> getPosition;
  private readonly Action<PointD> setPosition;
  private readonly PointD mouseDeltaFromStart;
  private readonly bool useViewTransform;
  private PointD startPosition;

  public RenderTreeLegendPositionHandler(
      GraphControl graphControl,
      Func<PointD> getPosition,
      Action<PointD> setPosition,
      PointD mouseLocation,
      bool useViewTransform) {
    this.graphControl = graphControl;
    this.getPosition = getPosition;
    this.setPosition = setPosition;
    this.useViewTransform = useViewTransform;
    mouseDeltaFromStart = ToLegendCoordinates(mouseLocation) - getPosition();
  }

  public IPoint Location => ToWorldCoordinates(getPosition());

  public void InitializeDrag(IInputModeContext context) => startPosition = getPosition();

  public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
    setPosition(ToLegendCoordinates(newLocation) - mouseDeltaFromStart);
  }

  public void CancelDrag(IInputModeContext context, PointD originalLocation) => setPosition(startPosition);

  public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) =>
      setPosition(ToLegendCoordinates(newLocation) - mouseDeltaFromStart);

  private PointD ToLegendCoordinates(PointD location) =>
      useViewTransform ? graphControl.WorldToViewCoordinates(location) : location;

  private PointD ToWorldCoordinates(PointD location) =>
      useViewTransform ? graphControl.ViewToWorldCoordinates(location) : location;
}
