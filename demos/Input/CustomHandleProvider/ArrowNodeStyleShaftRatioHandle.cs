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
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CustomHandleProvider
{
  /// <summary>
  ///   An <see cref="IHandle"/> for nodes with an <see cref="ArrowNodeStyle"/> to change the
  ///   <see cref="ArrowNodeStyle.ShaftRatio"/> interactively.
  /// </summary>
  internal sealed class ArrowNodeStyleShaftRatioHandle : IHandle, IPoint
  {
    private readonly Action shaftRatioChanged;
    private readonly INode node;
    private readonly ArrowNodeStyle style;
    private double xFactor;
    private double yFactor;
    private double initialShaftRatio;

    /// <summary>
    ///   Creates a new instance for the given node.
    /// </summary>
    /// <param name="node">The node whose style shall be changed.</param>
    /// <param name="shaftRatioChanged">An action that is called when the handle has been moved.</param>
    public ArrowNodeStyleShaftRatioHandle(INode node, Action shaftRatioChanged) {
      this.node = node;
      this.shaftRatioChanged = shaftRatioChanged;
      style = (ArrowNodeStyle) node.Style;
    }

    /// <summary>
    ///   Gets a live view of the handle location. The handle is placed on the shaft border half-way
    ///   along the shaft.
    /// </summary>
    public IPoint Location => this;

    /// <summary>Initializes the drag gesture.</summary>
    /// <param name="context">The current input mode context.</param>
    public void InitializeDrag(IInputModeContext context) {
      switch (style.Direction) {
        case ArrowNodeDirection.Right:
        case ArrowNodeDirection.Left:
          xFactor = 0;
          yFactor = -2 / node.Layout.Height;
          break;
        case ArrowNodeDirection.Up:
        case ArrowNodeDirection.Down:
          xFactor = -2 / node.Layout.Width;
          yFactor = 0;
          break;
      }
      initialShaftRatio = style.ShaftRatio;
    }

    /// <summary>
    ///   Calculates the new shaft ratio depending on the new mouse location and updates the node
    ///   style.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    /// <param name="newLocation">The new mouse location.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // Determine delta for the shaft ratio.
      var delta = xFactor * (newLocation.X - originalLocation.X) + yFactor * (newLocation.Y - originalLocation.Y);
      // Clamp the shaft ratio to the valid range.
      style.ShaftRatio = Math.Clamp(initialShaftRatio + delta, 0, 1);
      shaftRatioChanged();
    }

    /// <summary>Resets the initial shaft ratio.</summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      style.ShaftRatio = initialShaftRatio;
    }

    /// <summary>Sets the shaft ratio for the new location and triggers the callback.</summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    /// <param name="newLocation">The new mouse location.</param>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      HandleMove(context, originalLocation, newLocation);
    }

    /// <summary>
    ///   Returns <see cref="HandleType.Custom4"/> as handle type that determines the visualization
    ///   of the handle.
    /// </summary>
    public HandleType Type => HandleType.Custom4;

    /// <summary>Returns an optional tag object associated with the handle.</summary>
    public object? Tag => null;

    /// <summary>
    ///   Returns a double-arrow cursor that shall be used during the drag gesture. If the style's
    ///   direction is left or right, a vertical resize cursor is returned; otherwise, a horizontal
    ///   resize cursor is returned.
    /// </summary>
    public Cursor Cursor =>
        style.Direction is ArrowNodeDirection.Left or ArrowNodeDirection.Right ? Cursors.SizeNS : Cursors.SizeWE;

    /// <summary>This implementation does nothing special when clicked.</summary>
    public void HandleClick(ClickEventArgs eventArgs) { }

    /// <summary>The handle's x coordinate.</summary>
    public double X {
      get {
        var layout = node.Layout;
        if (style.Direction is ArrowNodeDirection.Up or ArrowNodeDirection.Down) {
          return layout.X + layout.Width * (1 - style.ShaftRatio) / 2;
        }
        if (style.Shape == ArrowStyleShape.DoubleArrow) {
          return layout.X + layout.Width / 2;
        }
        var headLength = ArrowNodeStyleAngleHandle.GetArrowHeadLength(layout, style);
        return style.Direction == ArrowNodeDirection.Right
            ? layout.X + (layout.Width - headLength) / 2
            : layout.X + headLength + (layout.Width - headLength) / 2;
      }
    }

    /// <summary>The handle's y coordinate.</summary>
    public double Y {
      get {
        var layout = node.Layout;
        if (style.Direction is ArrowNodeDirection.Left or ArrowNodeDirection.Right) {
          return layout.Y + layout.Height * (1 - style.ShaftRatio) / 2;
        }
        if (style.Shape == ArrowStyleShape.DoubleArrow) {
          return layout.Y + layout.Height / 2;
        }
        var headLength = ArrowNodeStyleAngleHandle.GetArrowHeadLength(layout, style);
        return style.Direction == ArrowNodeDirection.Down
            ? layout.Y + (layout.Height - headLength) / 2
            : layout.Y + headLength + (layout.Height - headLength) / 2;
      }
    }
  }
}
