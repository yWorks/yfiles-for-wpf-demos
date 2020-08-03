/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Crops adjacent edges at the nodes rotated bounds for internal ports.
  /// </summary>
  class AdjustOutlinePortInsidenessEdgePathCropper : DefaultEdgePathCropper {
    
    /// <summary>
    /// Checks whether or not the given location is inside the nodes rotated shape.
    /// </summary>
    protected override bool IsInside(PointD location, INode node, IShapeGeometry nodeShapeGeometry, IEdge edge) {
      return GetScaledOutline(node, nodeShapeGeometry).AreaContains(location);
    }

    /// <summary>
    /// Returns the intersection point of the segment between the outer and inner point and the node's rotated shape.
    /// </summary>
    /// <remarks>
    /// If there is no intersection point, the result is null.
    /// </remarks>
    protected override PointD? GetIntersection(INode node, IShapeGeometry nodeShapeGeometry, IEdge edge, PointD inner, PointD outer) {
      var a = GetScaledOutline(node, nodeShapeGeometry).FindLineIntersection(inner, outer);
      if (a < Double.PositiveInfinity) {
        return inner + (outer - inner) * a;
      }
      return null;
    }

    /// <summary>
    /// Returns a slightly enlarged outline of the shape to ensure that ports that lie exactly on the shape's outline
    /// are always considered inside.
    /// </summary>
    private GeneralPath GetScaledOutline(INode node, IShapeGeometry nodeShapeGeometry) {
      var outline = nodeShapeGeometry.GetOutline();
      if (outline == null) {
        outline = new GeneralPath(4);
        outline.AppendRectangle(node.Layout.ToRectD(), false);
      }
      const double factor = 1.001;
      var center = node.Layout.GetCenter();
      var matrix = new Matrix2D();
      matrix.Translate(new PointD(-center.X * (factor - 1), -center.Y * (factor - 1)));
      matrix.Scale(factor, factor);
      outline.Transform(matrix);
      return outline;
    }
  }
}
