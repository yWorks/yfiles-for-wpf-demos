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
using System.Collections.Generic;
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.CircleSnapping
{
  /// <summary>
  ///   Provides snap references for circles around the node with radii to its children, for lines
  ///   with fixed angles, for lines through siblings, and for lines with equal angles to neighbor nodes.
  /// </summary>
  internal sealed class SnapCircleProvider : NodeSnapReferenceProvider
  {
    private readonly PointD[] anglePoints = [
        new(0, -1), // 0°
        new(1, 0),  // 90°
        new(0, 1),  // 180°
        new(-1, 0)  // 360°
    ];

    public SnapCircleProvider(INode node) : base(node) { }

    public override void AddSnapReferences(GraphSnapContext context, CollectSnapReferencesEventArgs args) {
      if (!(args.Context.CanvasControl is GraphControl graphControl)) {
        return;
      }

      var centerNode = node;
      var graph = graphControl.Graph;
      var center = centerNode.Layout.GetCenter();
      var outEdges = graph.OutEdgesAt(centerNode).ToArray();
      var radii = outEdges.Select(edge => PointD.Distance(center, edge.GetTargetNode().Layout.GetCenter())).Distinct()
          .ToArray();
      var siblings = outEdges.Select(edge => edge.GetTargetNode()).ToArray();
      var fullViewportSize = Math.Max(graphControl.Viewport.Width, graphControl.Viewport.Height);

      AddCircleSnapReferences(args, radii, centerNode, center);
      AddAngleSnapReferences(args, centerNode, center, fullViewportSize);
      AddOppositeSiblingSnapReferences(args, siblings, center, fullViewportSize, centerNode);
      AddEqualAngleSnapReferences(context, args, siblings, center, fullViewportSize, centerNode);
    }

    /// <summary>
    ///   Adds circular snap references around the given node at the given radii.
    /// </summary>
    private static void AddCircleSnapReferences(
        CollectSnapReferencesEventArgs args,
        IEnumerable<double> radii,
        INode centerNode,
        PointD center) {
      foreach (var radius in radii) {
        args.AddSnapReference(new NodeSnapCircle(centerNode, center, radius, 0, 0, 10, $"SnapCircle {radius}"));
      }
    }

    /// <summary>
    ///   Adds snap lines radiating out from the given node at predetermined angles.
    /// </summary>
    private void AddAngleSnapReferences(
        CollectSnapReferencesEventArgs args,
        INode centerNode,
        PointD center,
        double maxRadius) {
      foreach (var anglePoint in anglePoints) {
        args.AddSnapReference(new NodeSnapLine(centerNode, SnapReferenceVisualizationType.BlankVariableLine, center,
            center + anglePoint * 10, center + anglePoint * maxRadius, 1));
      }
    }

    /// <summary>
    ///   Adds snap lines radiating out from the given center node opposite each given sibling node.
    /// </summary>
    private static void AddOppositeSiblingSnapReferences(
        CollectSnapReferencesEventArgs args,
        IEnumerable<INode> siblings,
        PointD center,
        double maxRadius,
        INode centerNode) {
      foreach (var sibling in siblings) {
        var siblingCenter = sibling.Layout.GetCenter();
        var opposite = center + (center - siblingCenter).Normalized * maxRadius;
        args.AddSnapReference(new NodeSnapLine(centerNode, SnapReferenceVisualizationType.ExtendedVariableLine,
            siblingCenter, siblingCenter, opposite, 10));
      }
    }

    /// <summary>
    ///   Adds snap lines radiating out from the given center node bisecting the angles between any two
    ///   consecutive nodes of the given sibling nodes.
    /// </summary>
    private static void AddEqualAngleSnapReferences(
        GraphSnapContext context,
        CollectSnapReferencesEventArgs args,
        IEnumerable<INode> siblings,
        PointD center,
        double maxRadius,
        INode centerNode) {
      var movedNodes = context.AffectedItems.OfType<INode>().ToArray();

      // calculate the angle for each sibling and sort the siblings according to their angles
      var fixedSiblings = siblings.Where(sibling => !movedNodes.Contains(sibling))
          .Select(sibling => new {
              Node = sibling,
              Angle = GetAngle(sibling.Layout.GetCenter(), center)
          }).OrderBy(item => item.Angle).ToArray();

      for (var i = 0; i < fixedSiblings.Length; i++) {
        var first = fixedSiblings[i == 0 ? fixedSiblings.Length - 1 : i - 1];
        var second = fixedSiblings[i];

        // calculate the angle for the bisecting line between the two siblings
        var angleOffset = i == 0 ? 2 * Math.PI : 0;
        var centeredAngle = (first.Angle - angleOffset + second.Angle) * 0.5;
        var centeredPoint = new PointD(Math.Cos(centeredAngle) * maxRadius + center.X,
            Math.Sin(centeredAngle) * maxRadius + center.Y);
        args.AddSnapReference(new EqualAngleSnapLine(centerNode, first.Node, second.Node, centeredPoint,
            SnapReferenceVisualizationType.BlankVariableLine, center, center, centeredPoint, 10, GraphItemTypes.Node,
            "NodeDistanceSnapLine"));
      }
    }

    private static double GetAngle(PointD point, PointD center) {
      var vector = point - center;
      return Math.Atan2(vector.Y + 1, vector.X) + 2 * Math.PI;
    }
  }
}
