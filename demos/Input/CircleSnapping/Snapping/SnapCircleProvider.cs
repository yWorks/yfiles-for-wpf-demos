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

using System;
using System.Collections.Generic;
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Input.CircleSnapping.Snapping
{
  /// <summary>
  ///   Provides <see cref="SnapReference"/>s for <see cref="AddCircleSnapReferences">circles</see> around the node
  ///   with radii to its children, for <see cref="AddAngleSnapReferences">lines with fixed angles</see>,
  ///   for <see cref="AddOppositeSiblingSnapReferences">lines through siblings</see> to snap to their opposite side,
  ///   and for <see cref="AddEqualAngleSnapReferences">lines with equal angles</see> to neighbor nodes.
  /// </summary>
  public sealed class SnapCircleProvider : NodeSnapReferenceProvider
  {
    private static readonly List<PointD> AnglePoints = new() {
        new PointD(0, -1), // 0°
        new PointD(1, 0),  // 90°
        new PointD(0, 1),  // 180°
        new PointD(-1, 0), // 360°

        new PointD(0.707, -0.707), // 45°
        new PointD(0.707, 0.707),  // 135°
        new PointD(-0.707, 0.707), // 225°
        new PointD(-0.707, -0.707) // 315°

        // new PointD(0.5, -0.866),  // 30°
        // new PointD(0.866, -0.5),  // 60°
        // new PointD(0.866, 0.5),   // 120°
        // new PointD(0.5, 0.866),   // 150°
        // new PointD(-0.5, 0.866),  // 210°
        // new PointD(-0.866, 0.5),  // 240°
        // new PointD(-0.866, -0.5), // 300°
        // new PointD(-0.5, -0.866), // 330°
    };

    public SnapCircleProvider(INode node) : base(node) { }

    public override void AddSnapReferences(GraphSnapContext context, CollectSnapReferencesEventArgs args) {
      if (args.Context.CanvasControl is not GraphControl graphControl) {
        return;
      }

      var centerNode = node;
      var graph = graphControl.Graph;
      var center = centerNode.Layout.GetCenter();
      var outEdges = graph.OutEdgesAt(centerNode);
      var radii = outEdges.Select(outEdge => {
        var targetNode = outEdge.GetTargetNode();
        return PointD.Distance(center, targetNode.Layout.GetCenter());
      }).Distinct().ToList();
      var siblings = outEdges.Select(outEdge => outEdge.GetTargetNode()).ToList();
      var fullViewportSize = Math.Max(graphControl.Viewport.Width, graphControl.Viewport.Height);

      AddCircleSnapReferences(args, radii, centerNode, center);
      AddAngleSnapReferences(args, centerNode, center, fullViewportSize);
      AddOppositeSiblingSnapReferences(args, siblings, center, fullViewportSize, centerNode);
      AddEqualAngleSnapReferences(context, args, siblings, center, fullViewportSize, centerNode);
    }

    private static void AddCircleSnapReferences(
        CollectSnapReferencesEventArgs args,
        List<double> radii,
        INode centerNode,
        PointD center) {
      foreach (var radius in radii) {
        args.AddSnapReference(new NodeSnapCircle(centerNode, center, radius, 10, $"SnapCircle {radius}"));
      }
    }

    /// <summary>
    ///   Adds snap lines radiating out from the given node at pre-determined <see cref="AnglePoints">angles</see>.
    /// </summary>
    private static void AddAngleSnapReferences(
        CollectSnapReferencesEventArgs args,
        INode centerNode,
        PointD center,
        double maxRadius) {
      foreach (var point in AnglePoints) {
        args.AddSnapReference(new NodeSnapLine(centerNode, center, center + (point * 10), center + (point * maxRadius),
            1, SnapReferenceVisualizationType.BlankVariableLine));
      }
    }

    /// <summary>
    ///   Adds snap lines radiating out from the given center node opposite each given sibling node.
    /// </summary>
    private static void AddOppositeSiblingSnapReferences(
        CollectSnapReferencesEventArgs args,
        List<INode> siblings,
        PointD center,
        double maxRadius,
        INode centerNode) {
      foreach (var sibling in siblings) {
        var siblingCenter = sibling.Layout.GetCenter();
        var opposite = center + ((center - siblingCenter).Normalized * maxRadius);
        args.AddSnapReference(new NodeSnapLine(centerNode, siblingCenter, siblingCenter, opposite, 10,
            SnapReferenceVisualizationType.ExtendedVariableLine));
      }
    }

    /// <summary>
    ///   Adds snap lines radiating out from the given center node bisecting the angles between any two
    ///   consecutive nodes of the given sibling nodes.
    /// </summary>
    private static void AddEqualAngleSnapReferences(
        GraphSnapContext context,
        CollectSnapReferencesEventArgs args,
        List<INode> siblings,
        PointD center,
        double maxRadius,
        INode centerNode) {
      var angles = new Dictionary<INode, double>();
      var movedNodes = context.AffectedItems.OfType<INode>();

      // calculate the angle for each sibling and sort the siblings according to their angles
      var fixedSiblings = siblings.Where(sibling => !movedNodes.Contains(sibling)).Select(fixedSibling => {
        var fixedSiblingVec = fixedSibling.Layout.GetCenter() - center;
        var angle = Math.Atan2(fixedSiblingVec.Y + 1, fixedSiblingVec.X - 0) + (2 * Math.PI);
        angles.Add(fixedSibling, angle);
        return fixedSibling;
      }).OrderBy(fixedSibling => angles[fixedSibling]).ToList();

      INode firstSibling = null;
      INode lastSibling = null;
      foreach (var fixedSibling in fixedSiblings) {
        if (firstSibling == null) {
          firstSibling = fixedSibling;
        } else {
          var centeredAngle = (angles[fixedSibling] + angles[lastSibling]) * 0.5;
          var centeredPoint = (new PointD(Math.Cos(centeredAngle), Math.Sin(centeredAngle)) * maxRadius) + center;
          args.AddSnapReference(new EqualAngleSnapLine(centerNode, fixedSibling, lastSibling, centeredPoint, 10));
        }
        lastSibling = fixedSibling;
      }

      if (firstSibling != null && lastSibling != null) {
        var centeredAngle = (angles[lastSibling] - (2 * Math.PI) + angles[firstSibling]) * 0.5;
        var centeredPoint = (new PointD(Math.Cos(centeredAngle), Math.Sin(centeredAngle)) * maxRadius) + center;
        args.AddSnapReference(new EqualAngleSnapLine(centerNode, firstSibling, lastSibling, centeredPoint, 10));
      }
    }
  }
}
