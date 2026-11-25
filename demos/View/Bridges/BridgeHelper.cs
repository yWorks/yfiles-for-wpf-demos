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

using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Bridges
{
  /// <summary>
  ///   A custom delegating callback that implements <see cref="CustomCallback.CreateCustomBridge"/> differently.
  /// </summary>
  internal sealed class CustomCallback : IBridgeCreator
  {
    private readonly IBridgeCreator fallback;

    /// <summary>
    ///   Creates a new instance of <see cref="CustomCallback"/>.
    /// </summary>
    public CustomCallback(IBridgeCreator fallback) {
      this.fallback = fallback;
    }

    /// <summary>
    ///   Returns the CrossingStyle to be used.
    /// </summary>
    public BridgeCrossingStyle GetCrossingStyle(IRenderContext context) => fallback.GetCrossingStyle(context);

    /// <summary>
    ///   Returns the BridgeOrientationStyle to be used.
    /// </summary>
    public BridgeOrientationStyle GetOrientationStyle(IRenderContext context) => fallback.GetOrientationStyle(context);

    /// <summary>
    ///   Returns the width of the bridge to be used.
    /// </summary>
    public double GetBridgeWidth(IRenderContext context) => fallback.GetBridgeWidth(context);

    /// <summary>
    ///   Returns the height of the bridge to be used.
    /// </summary>
    public double GetBridgeHeight(IRenderContext context) => fallback.GetBridgeHeight(context);

    /// <summary>
    ///   Called by the BridgeManager if the GetCrossingStyle method returns BridgeCrossingStyle.Custom to
    ///   insert a bridge into the given general path.
    /// </summary>
    /// <param name="context">The given render context</param>
    /// <param name="path">The general path to be used</param>
    /// <param name="startPoint">The coordinates of the starting point of the bridge.</param>
    /// <param name="endPoint">The coordinates of the ending point of the bridge.</param>
    /// <param name="gapLength">The distance between the starting point and the end point.</param>
    public void CreateCustomBridge(
        IRenderContext context,
        GeneralPath path,
        PointD startPoint,
        PointD endPoint,
        double gapLength) {
      // first finish the last segment
      path.LineTo(startPoint);
      // then draw our custom bridge, if the gap is large enough
      if (gapLength > 1) {
        // some helper vectors first
        var delta = endPoint - startPoint;
        var rightVector = delta * (1 / gapLength);
        var upVector = new PointD(rightVector.Y, -rightVector.X);

        // get the height from the context
        var height = GetBridgeHeight(context);
        // determine bending for our arc
        const double arc = 3;
        // now draw two arcs at the end and the start of the segment
        path.MoveTo(startPoint + upVector * height - rightVector * arc);

        path.QuadTo(startPoint + rightVector * arc, startPoint - upVector * height - rightVector * arc);
        path.MoveTo(endPoint + rightVector * arc + upVector * height);
        path.QuadTo(endPoint - rightVector * arc, endPoint - upVector * height + rightVector * arc);
      }
      // finally make sure that the edge continues at the right location
      path.MoveTo(endPoint);
    }
  }

  /// <summary>
  ///   Custom <see cref="IObstacleProvider"/> implementation that returns the node style's outline
  ///   as an obstacle.
  /// </summary>
  /// <seealso cref="IShapeGeometry.GetOutline"/>
  internal sealed class GroupNodeObstacleProvider : IObstacleProvider
  {
    private readonly INode groupNode;

    /// <summary>
    ///   Creates a new instance of <see cref="GroupNodeObstacleProvider"/>.
    /// </summary>
    public GroupNodeObstacleProvider(INode groupNode) {
      this.groupNode = groupNode;
    }

    /// <summary>
    ///   Returns an obstacle for the node style's outline.
    /// </summary>
    /// <param name="context">The given render context</param>
    /// <remarks>
    ///   Specified by <see cref="IObstacleProvider.GetObstacles"/>.
    /// </remarks>
    public GeneralPath? GetObstacles(IRenderContext context) {
      var style = groupNode.Style;
      var visible = style.Renderer.GetVisibilityTestable(groupNode, style).IsVisible(context, context.Clip);
      if (visible) {
        return CreatePath();
      }
      // If the node is invisible, don't return anything (won't be painted anyway...)
      return null;
    }

    /// <summary>
    ///   Uses the node style's outline as obstacle.
    ///   For node style renderers that don't provide a <see cref="IShapeGeometry"/>, no bridges will be created.
    /// </summary>
    public GeneralPath? CreatePath() {
      var style = groupNode.Style;
      if (style is GroupNodeStyle) {
        var outline = new GeneralPath();
        outline.AppendRectangle(groupNode.Layout, false);
        return outline;
      }
      var geometry = style.Renderer.GetShapeGeometry(groupNode, style);
      return geometry.GetOutline();
    }
  }
}
