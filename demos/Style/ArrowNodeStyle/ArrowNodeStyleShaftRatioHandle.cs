/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Graph.ArrowNodeStyle
{
  /// <summary>
  /// An <see cref="IHandle"/> for nodes with a <see cref="yWorks.Graph.Styles.ArrowNodeStyle"/> to change the
  /// <see cref="yWorks.Graph.Styles.ArrowNodeStyle.ShaftRatio"/> interactively.
  /// </summary>
  public class ArrowNodeStyleShaftRatioHandle : IHandle, IPoint
  {
    private readonly INode node;
    private readonly yWorks.Graph.Styles.ArrowNodeStyle style;
    private readonly Action shaftRatioChanged;
    private double xFactor;
    private double yFactor;
    private double initialShaftRatio;

    /// <summary>
    /// Creates a new instance for the given node.
    /// </summary>
    /// <param name="node">The node whose style shall be changed.</param>
    /// <param name="shaftRatioChanged">An action that is called when the handle has been moved.</param>
    public ArrowNodeStyleShaftRatioHandle(INode node, Action shaftRatioChanged) {
      this.node = node;
      this.shaftRatioChanged = shaftRatioChanged;
      this.style = node.Style as yWorks.Graph.Styles.ArrowNodeStyle;
    }

    /// <summary>
    /// Gets a live view of the handle location.
    /// </summary>
    /// <remarks>
    /// The handle is placed on the shaft border half-way along the shaft.
    /// </remarks>
    public IPoint Location { get { return this; } }

    /// <summary>
    /// Initializes the drag gesture.
    /// </summary>
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
          ;
          yFactor = 0;
          break;
      }
      initialShaftRatio = style.ShaftRatio;
    }

    /// <summary>
    /// Calculates the new shaft ratio depending on the new mouse location and updates the node style.
    /// </summary>
    /// <param name="context">The current input mode context.</param>
    /// <param name="originalLocation">The original handle location.</param>
    /// <param name="newLocation">The new mouse location.</param>
    public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      // determine delta for the shaft ratio
      var delta = xFactor * (newLocation.X - originalLocation.X) + yFactor * (newLocation.Y - originalLocation.Y);
      // ... and clamp to valid values
      var newShaftRatio = Math.Max(0, Math.Min(initialShaftRatio + delta, 1));
      style.ShaftRatio = newShaftRatio;

      shaftRatioChanged?.Invoke();
    }

    /// <summary>
    /// Resets the initial shaft ratio.
    /// </summary>
    public void CancelDrag(IInputModeContext context, PointD originalLocation) {
      style.ShaftRatio = initialShaftRatio;
    }

    /// <summary>
    /// Sets the shaft ratio for the new location, and triggers the shaftRatioChanged action.
    /// </summary>
    public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      HandleMove(context, originalLocation, newLocation);
    }

    /// <summary>
    /// Returns <see cref="HandleTypes.Shear"/> as handle type that determines the visualization of the handle.
    /// </summary>
    public HandleTypes Type { get { return HandleTypes.Shear; } }

    /// <summary>
    /// Returns an double-arrow cursor as cursor that shall be used during the drag gesture.
    /// </summary>
    /// <remarks>
    /// If the styles <see cref="ArrowNodeStyle.Direction"/> is <see cref="ArrowNodeDirection.Left"/> or
    /// <see cref="ArrowNodeDirection.Right"/>, <see cref="Cursors.SizeNS"/> is returned, otherwise <see cref="Cursors.SizeWE"/>.
    /// </remarks>
    public Cursor Cursor {
      get {
        return (style.Direction == ArrowNodeDirection.Left || style.Direction == ArrowNodeDirection.Right)
            ? Cursors.SizeNS
            : Cursors.SizeWE;
      }
    }

    /// <summary>
    /// Ignore clicks.
    /// </summary>
    /// <param name="eventArgs">Arguments describing the click.</param>
    public void HandleClick(ClickEventArgs eventArgs) {
      // ignore clicks
    }

    #region IPoint implementation to ensure the handle position is always up-to-date

    double IPoint.X {
      get {
        var nodeLayout = node.Layout;
        if (style.Direction == ArrowNodeDirection.Up || style.Direction == ArrowNodeDirection.Down) {
          return nodeLayout.X + nodeLayout.Width * (1 - style.ShaftRatio) / 2;
        }
        if (style.Shape == ArrowStyleShape.DoubleArrow) {
          return nodeLayout.X + nodeLayout.Width / 2;
        }

        var headLength = ArrowNodeStyleAngleHandle.GetArrowHeadLength(nodeLayout, style);
        if (style.Direction == ArrowNodeDirection.Right) {
          return nodeLayout.X + (nodeLayout.Width - headLength) / 2;
        }
        return nodeLayout.X + headLength + (nodeLayout.Width - headLength) / 2;
      }
    }

    double IPoint.Y {
      get {
        var nodeLayout = node.Layout;
        if (style.Direction == ArrowNodeDirection.Left || style.Direction == ArrowNodeDirection.Right) {
          return nodeLayout.Y + nodeLayout.Height * (1 - style.ShaftRatio) / 2;
        }
        if (style.Shape == ArrowStyleShape.DoubleArrow) {
          return nodeLayout.Y + nodeLayout.Height / 2;
        }

        var headLength = ArrowNodeStyleAngleHandle.GetArrowHeadLength(nodeLayout, style);
        if (style.Direction == ArrowNodeDirection.Down) {
          return nodeLayout.Y + (nodeLayout.Height - headLength) / 2;
        }
        return nodeLayout.Y + headLength + (nodeLayout.Height - headLength) / 2;
      }
    }

    #endregion
  }
}
