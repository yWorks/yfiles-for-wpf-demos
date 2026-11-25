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

namespace yWorks.Demo.CircleSnapping
{
  /// <summary>
  ///   A circular snap reference around a given node with a given radius.
  /// </summary>
  internal sealed class NodeSnapCircle : SnapCircle
  {
    public INode Node { get; }

    public NodeSnapCircle(
        INode node,
        PointD center,
        double radius,
        double startAngle,
        double endAngle,
        double weight,
        object? tag) : base(center, radius, startAngle, endAngle, weight, GraphItemTypes.Node, tag) {
      Node = node;
    }
  }

  /// <summary>
  ///   A snap line that radiates out from a given node.
  /// </summary>
  internal class NodeSnapLine : SnapLine
  {
    public INode Node { get; }

    public NodeSnapLine(
        INode node,
        SnapReferenceVisualizationType visualizationType,
        PointD coordinates,
        PointD from,
        PointD to,
        double weight,
        GraphItemTypes snappableItems = GraphItemTypes.All,
        object? tag = null) : base(visualizationType, coordinates, from, to, weight, snappableItems, tag) {
      Node = node;
    }
  }

  /// <summary>
  ///   A snap line that radiates out from a parent node between two neighboring child nodes bisecting
  ///   the angle between those child nodes.
  /// </summary>
  internal sealed class EqualAngleSnapLine : SnapLine
  {
    public INode Parent { get; }

    public INode FirstSibling { get; }

    public INode SecondSibling { get; }

    public PointD CenteredPoint { get; }

    public EqualAngleSnapLine(
        INode parent,
        INode firstSibling,
        INode secondSibling,
        PointD centeredPoint,
        SnapReferenceVisualizationType visualizationType,
        PointD coordinates,
        PointD from,
        PointD to,
        double weight,
        GraphItemTypes snappableItems,
        object? tag) : base(visualizationType, coordinates, from, to, weight, snappableItems, tag) {
      Parent = parent;
      FirstSibling = firstSibling;
      SecondSibling = secondSibling;
      CenteredPoint = centeredPoint;
    }
  }
}
