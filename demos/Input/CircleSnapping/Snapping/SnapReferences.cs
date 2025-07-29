/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Graph.Input.CircleSnapping.Snapping
{
  /// <summary>
  ///   A circular snap reference around a given node with a given radius.
  /// </summary>
  public sealed class NodeSnapCircle : SnapCircle
  {
    public INode Node { get; }

    public NodeSnapCircle(INode node, PointD center, double radius, int weight, string tag) : this(node, center, radius,
        0, 0, weight, tag) { }

    public NodeSnapCircle(
        INode node,
        PointD center,
        double radius,
        double startAngle,
        double endAngle,
        int weight,
        string tag) : base(center, radius, startAngle, endAngle, weight, GraphItemTypes.All, tag) {
      Node = node;
    }
  }

  /// <summary>
  ///   A snap line that radiates out from a given node.
  /// </summary>
  public sealed class NodeSnapLine : SnapLine
  {
    public INode Node { get; }

    public NodeSnapLine(
        INode node,
        PointD coordinates,
        PointD from,
        PointD to,
        int weight,
        SnapReferenceVisualizationType visualizationType) : base(visualizationType, coordinates, from, to, weight) {
      Node = node;
    }
  }

  /// <summary>
  ///   A snap line that radiates out from a parent node between two neighboring child nodes bisecting
  ///   the angle between those child nodes.
  /// </summary>
  public sealed class EqualAngleSnapLine : SnapLine
  {
    public INode Parent { get; }

    public INode FirstSibling { get; }

    public INode SecondSibling { get; }

    public PointD CenteredPoint { get; }

    public EqualAngleSnapLine(INode parent, INode firstSibling, INode secondSibling, PointD centeredPoint, int weight)
        : base(SnapReferenceVisualizationType.BlankVariableLine, parent.Layout.GetCenter(), parent.Layout.GetCenter(),
            centeredPoint, weight, GraphItemTypes.Node, "NodeDistanceSnapLine") {
      Parent = parent;
      FirstSibling = firstSibling;
      SecondSibling = secondSibling;
      CenteredPoint = centeredPoint;
    }
  }
}
