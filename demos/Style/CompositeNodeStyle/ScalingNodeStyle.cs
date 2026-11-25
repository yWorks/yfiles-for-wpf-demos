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

using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CompositeNodeStyle
{
  /// <summary>
  ///   A custom node style that scales the visual representation of a node.
  ///   Extends the functionality of the <see cref="DelegatingNodeStyle"/> by applying a scaling transformation.
  /// </summary>
  public sealed class ScalingNodeStyle : DelegatingNodeStyle
  {
    private readonly double sy;
    private readonly double sx;
    private readonly INodeStyle style;
    private readonly SimpleNode scaledNode = new();

    /// <summary>
    ///   Initializes a new instance of the ScalingNodeStyle class.
    /// </summary>
    /// <param name="style">The wrapped node style to which the scaling transformation is applied.</param>
    /// <param name="sx">The horizontal scaling factor. Defaults to 1.</param>
    /// <param name="sy">The vertical scaling factor. Defaults to 1.</param>
    public ScalingNodeStyle(INodeStyle style, double sx = 1, double sy = 1) {
      this.style = style;
      this.sx = sx;
      this.sy = sy;
    }

    /// <summary>
    ///   Retrieves a scaled version of the node, applying the scaling transformation.
    ///   This method calculates the new layout of the node by scaling its width and height
    ///   according to the specified horizontal (sx) and vertical (sy) scaling factors.
    ///   It then creates a new <see cref="SimpleNode"/> with the updated layout.
    /// </summary>
    /// <remarks>
    ///   Creates the visual representation of the node, applying the scaling transformation.
    ///   Updates the visual representation to reflect any changes, preserving the scaling transformation.
    /// </remarks>
    /// <param name="node">The node for which the scaled version is created.</param>
    /// <returns>A new <see cref="INode"/> representing the scaled node with updated layout.</returns>
    protected override INode GetNode(INode node) {
      var center = node.Layout.GetCenter();
      var width = node.Layout.Width * sx;
      var height = node.Layout.Height * sy;
      scaledNode.Layout = new RectD(center.X - width / 2, center.Y - height / 2, width, height);
      return scaledNode;
    }

    /// <summary>
    ///   Gets the wrapped node style for the specified node.
    /// </summary>
    /// <param name="node">The node for which the wrapped style is retrieved.</param>
    /// <returns>The wrapped <see cref="INodeStyle"/> instance.</returns>
    protected override INodeStyle GetStyle(INode node) => style;
  }
}
