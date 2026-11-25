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

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.PositionHandler
{
  /// <summary>
  ///   A <see cref="ConstrainedPositionHandler"/> that limits the movement of a
  ///   node to be within a rectangle and delegates for other aspects to
  ///   another (the original) handler.
  /// </summary>
  public sealed class OrangePositionHandler : ConstrainedPositionHandler
  {
    private readonly INode node;
    private readonly MutableRectangle boundaryRectangle;
    private RectD boundaryPositionRectangle;

    /// <summary>
    ///   Creates a new instance of <see cref="OrangePositionHandler"/>.
    /// </summary>
    /// <param name="boundaryRectangle">The boundary rectangle.</param>
    /// <param name="node">The given node.</param>
    /// <param name="delegateHandler">The default handler.</param>
    public OrangePositionHandler(
        MutableRectangle boundaryRectangle,
        INode node,
        IPositionHandler delegateHandler) : base(delegateHandler) {
      this.boundaryRectangle = boundaryRectangle;
      this.node = node;
    }

    /// <summary>
    ///   Prepares the rectangle that is actually used to limit the node
    ///   position (and calls the base implementation). Since a position handler
    ///   works on points, the actual rectangle must be a limit for the upper
    ///   left corner of the node and not for the node's bounding box.
    /// </summary>
    /// <param name="context">The input mode context.</param>
    /// <param name="originalLocation">The original location.</param>
    protected override void OnInitialized(IInputModeContext context, PointD originalLocation) {
      base.OnInitialized(context, originalLocation);
      // Shrink the rectangle by the node size to get the limits for the upper left node corner
      boundaryPositionRectangle = new RectD(boundaryRectangle.X, boundaryRectangle.Y,
          boundaryRectangle.Width - node.Layout.Width, boundaryRectangle.Height - node.Layout.Height);
    }

    /// <summary>
    ///   Returns the position that is constrained by the rectangle.
    /// </summary>
    /// <param name="context">The context in which the drag will be performed.</param>
    /// <param name="originalLocation">The value of the location property at the time of InitializeDrag.</param>
    /// <param name="newLocation">The new location of the handler.</param>
    protected override PointD ConstrainNewLocation(
        IInputModeContext context,
        PointD originalLocation,
        PointD newLocation) =>
        newLocation.GetConstrained(boundaryPositionRectangle);
  }
}
