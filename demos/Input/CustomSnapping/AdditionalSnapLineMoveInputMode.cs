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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;

namespace yWorks.Demo.CustomSnapping
{
  /// <summary>
  ///   This input mode allows moving free snap lines using a drag gesture.
  /// </summary>
  internal sealed class AdditionalSnapLineMoveInputMode : MoveInputMode
  {
    private readonly IReadOnlyList<AdditionalSnapLineVisualCreator> snapLineCreators;
    private readonly GraphControl graphControl;
    private AdditionalSnapLinePositionHandler? handler;

    /// <summary>
    ///   Creates a new instance of <see cref="AdditionalSnapLineMoveInputMode"/>.
    /// </summary>
    public AdditionalSnapLineMoveInputMode(
        IReadOnlyList<AdditionalSnapLineVisualCreator> snapLineCreators,
        GraphControl graphControl) {
      this.snapLineCreators = snapLineCreators;
      this.graphControl = graphControl;
      PositionHandler = null;
      HitTestable = HitTestables.Create(IsValidHit);
    }

    /// <summary>
    ///   Returns true if an AdditionalSnapLine can be found in a close surrounding of the given location.
    /// </summary>
    public bool IsValidHit(IInputModeContext context, PointD location) {
      var line = TryGetAdditionalSnapLineCreatorAt(location);
      if (line is not null) {
        handler = new AdditionalSnapLinePositionHandler(line, location, graphControl);
        return true;
      }

      handler = null;
      return false;
    }

    /// <summary>
    ///   Returns the first AdditionalSnapLine found in a close surrounding of the given location
    ///   or null if none can be found.
    /// </summary>
    public AdditionalSnapLineVisualCreator? TryGetAdditionalSnapLineCreatorAt(PointD location) {
      var surrounding = new RectD(location + new PointD(-3, -3), new SizeD(6, 6));
      return snapLineCreators.FirstOrDefault(line => surrounding.IntersectsLine(line.From, line.To));
    }

    /// <summary>
    ///   Sets the <see cref="MoveInputMode.PositionHandler"/> property.
    /// </summary>
    /// <remarks>Overrides <see cref="MoveInputMode.OnDragStarting"/>.</remarks>
    protected override void OnDragStarting(InputModeEventArgs inputModeEventArgs) {
      PositionHandler = handler;
      base.OnDragStarting(inputModeEventArgs);
    }

    /// <summary>
    ///   Clears the <see cref="MoveInputMode.PositionHandler"/> property.
    /// </summary>
    /// <remarks>Overrides <see cref="MoveInputMode.OnDragCanceled"/>.</remarks>
    protected override void OnDragCanceled(InputModeEventArgs inputModeEventArgs) {
      base.OnDragCanceled(inputModeEventArgs);
      PositionHandler = null;
    }

    /// <summary>
    ///   Clears the <see cref="MoveInputMode.PositionHandler"/> property.
    /// </summary>
    /// <remarks>Overrides <see cref="MoveInputMode.OnDragFinished"/>.</remarks>
    protected override void OnDragFinished(InputModeEventArgs inputModeEventArgs) {
      base.OnDragFinished(inputModeEventArgs);
      PositionHandler = null;
    }
  }
}
