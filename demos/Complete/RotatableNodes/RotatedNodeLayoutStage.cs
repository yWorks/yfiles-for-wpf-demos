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
using System.Collections.Generic;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Geometry;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Organic;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Layout Stage which handles <see cref="RotatableNodeStyleDecorator">rotated nodes</see>.
  /// </summary>
  /// <remarks>
  /// During the <see cref="LayoutStageBase.CoreLayout"/> the layout is calculated with the rotated node's bounding box,
  /// i.e. a rectangular box which is large enought to fully include the rotated node.
  /// The edges are connected with the actual rotated shape of the node according to the <see cref="EdgeRoutingMode"/>.
  /// </remarks>
  public class RotatedNodeLayoutStage : LayoutStageBase
  {
    /// <summary>
    /// Creates a new instance with an optional core layout algorithm.
    /// </summary>
    public RotatedNodeLayoutStage(ILayoutAlgorithm coreLayout = null) : base(coreLayout) {
      EdgeRoutingMode = RoutingMode.ShortestStraightPathToBorder;
    }

    /// <summary>
    /// The <see cref="IDataProvider"/> key to register a data provider that provides the outline and oriented
    /// layout to this stage.
    /// </summary>
    public static readonly NodeDpKey<RotatedNodeShape> RotatedNodeLayoutDpKey = new NodeDpKey<RotatedNodeShape>(typeof(RotatedNodeLayoutStage), "RotatedNodeLayoutDpKey");

    /// <summary>
    /// Gets or sets the mode to use to connect edges from the bounding box to the actual shape.
    /// </summary>
    public RoutingMode EdgeRoutingMode { get; set; }

    /// <summary>
    /// Executes the layout algorithm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enlarges the node layout to fully encompass the rotated layout (the rotated layout's bounding box). 
    /// If the <see cref="EdgeRoutingMode"/> is set to <see cref="RoutingMode.FixedPort"/> 
    /// port constraints are created to keep the ports at their current location.
    /// Existing port constraints are adjusted to the rotation.
    /// </para>
    /// <para>
    /// Then, the <see cref="LayoutStageBase.CoreLayout"/> is executed.
    /// </para>
    /// <para>
    /// After the core layout the original node sizes are restored.
    /// If the <see cref="EdgeRoutingMode"/> is set to <see cref="RoutingMode.ShortestStraightPathToBorder"/>
    /// the last edge segment is extended from the bounding box to the rotated layout.
    /// </para>
    /// </remarks>
    public override void ApplyLayout(LayoutGraph graph) {
      if (CoreLayout == null) return;

      var boundsProvider = graph.GetDataProvider(RotatedNodeLayoutDpKey);
      if (boundsProvider == null) {
        // no provider: this stage adds nothing to the core layout
        CoreLayout.ApplyLayout(graph);
        return;
      }

      bool addedSourcePortConstraints = false;
      bool addedTargetPortContstraints = false;
      IDataMap sourcePortConstraints = (IDataMap)graph.GetDataProvider(PortConstraintKeys.SourcePortConstraintDpKey);
      IDataMap targetPortConstraints = (IDataMap)graph.GetDataProvider(PortConstraintKeys.TargetPortConstraintDpKey);
      if (EdgeRoutingMode == RoutingMode.FixedPort) {
        // Fixed port: create port constraints to keep the ports at position
        // in this case: create data providers if there are none yet
        if (sourcePortConstraints == null) {
          sourcePortConstraints = graph.CreateEdgeMap();
          graph.AddDataProvider(PortConstraintKeys.SourcePortConstraintDpKey, sourcePortConstraints);
          addedSourcePortConstraints = true;
        }
        if (targetPortConstraints == null) {
          targetPortConstraints = graph.CreateEdgeMap();
          graph.AddDataProvider(PortConstraintKeys.TargetPortConstraintDpKey, targetPortConstraints);
          addedTargetPortContstraints = true;
        }
      }
      try {
        var originalDimensions = new Dictionary<Node, OldDimensions>();
        foreach (var node in graph.Nodes) {
          var nodeShape = (RotatedNodeShape)boundsProvider.Get(node);
          var orientedLayout = nodeShape != null ? nodeShape.OrientedLayout : null;
          var outline = nodeShape != null ? nodeShape.Outline : null;
          if (orientedLayout != null) {
            // if the current node is rotated: apply fixes
            // remember old layout and size
            var oldLayout = graph.GetLayout(node);
            var newLayout = orientedLayout.GetBounds().ToYRectangle();
            var offset = new PointD(newLayout.X - oldLayout.X, newLayout.Y - oldLayout.Y);
            var originalSize = new SizeD(oldLayout.Width, oldLayout.Height);
            var oldDimensions = new OldDimensions {
              offset = offset,
              size = originalSize,
              outline = outline
            };
            if (EdgeRoutingMode == RoutingMode.FixedPort) {
              // EdgeRoutingMode: FixedPort: keep the ports at their current location

              // The oriented layout's corners to find the best PortSide
              var tl = new PointD(orientedLayout.AnchorX + orientedLayout.UpX * orientedLayout.Height, orientedLayout.AnchorY + orientedLayout.UpY * orientedLayout.Height);
              var tr = new PointD(orientedLayout.AnchorX + orientedLayout.UpX * orientedLayout.Height - orientedLayout.UpY * orientedLayout.Width, 
                orientedLayout.AnchorY + orientedLayout.UpY * orientedLayout.Height + orientedLayout.UpX * orientedLayout.Width);
              var bl = new PointD(orientedLayout.AnchorX, orientedLayout.AnchorY);
              var br = new PointD(orientedLayout.AnchorX - orientedLayout.UpY * orientedLayout.Width, orientedLayout.AnchorY + orientedLayout.UpX * orientedLayout.Width);

              // for each out edge
              foreach (var edge in node.OutEdges) {
                // create a strong port constraint for the side which is closest to the port location (without rotation)
                var constraint = sourcePortConstraints.Get(edge);
                if (constraint == null) {
                  var point = graph.GetSourcePointAbs(edge).ToPointD();
                  var side = FindBestSide(point, bl, br, tl, tr);
                  sourcePortConstraints.Set(edge, PortConstraint.Create(side, true));
                }
              }
              foreach (var edge in node.InEdges) {
                // create a strong port constraint for the side which is closest to the port location (without rotation)
                var constraint = targetPortConstraints.Get(edge);
                if (constraint == null) {
                  var point = graph.GetTargetPointAbs(edge).ToPointD();
                  var side = FindBestSide(point, bl, br, tl, tr);
                  targetPortConstraints.Set(edge, PortConstraint.Create(side, true));
                }
              }
            }

            // For source and target port constraints: fix the PortSide according to the rotation
            var angle = Math.Atan2(orientedLayout.UpY, orientedLayout.UpX);
            if (sourcePortConstraints != null) {
              foreach (var edge in node.OutEdges) {
                FixPortConstraintSide(sourcePortConstraints, edge, angle);
              }
            }
            if (targetPortConstraints != null) {
              foreach (var edge in node.InEdges) {
                FixPortConstraintSide(targetPortConstraints, edge, angle);
              }
            }

            // enlarge the node layout
            var position = new YPoint(newLayout.X, newLayout.Y);
            oldDimensions.location = position;
            originalDimensions.Add(node, oldDimensions);
            graph.SetLocation(node, position);
            graph.SetSize(node, newLayout);
          }
        }

        // ===============================================================

        CoreLayout.ApplyLayout(graph);

        // ===============================================================

        var groups = graph.GetDataProvider(GroupingKeys.GroupDpKey);
        foreach (var node in graph.Nodes) {
          if (groups != null && groups.GetBool(node)) {
            // groups don't need to be adjusted to their former size and location because their bounds are entirely
            // calculated by the layout algorithm and they are not rotated
            continue;
          }

          // for each node which has been corrected: undo the correction
          var oldDimensions = originalDimensions[node];
          var offset = oldDimensions.offset;
          var originalSize = oldDimensions.size;
          var newLayout = graph.GetLayout(node);

          // create a general path representing the new roated layout
          var path = oldDimensions.outline;
          var transform = new Matrix2D();
          transform.Translate(new PointD(newLayout.X - oldDimensions.location.X, newLayout.Y - oldDimensions.location.Y));
          path.Transform(transform);

          // restore the original size
          graph.SetLocation(node, new YPoint(newLayout.X - offset.X, newLayout.Y - offset.Y));
          graph.SetSize(node, originalSize.ToYDimension());

          if (EdgeRoutingMode == RoutingMode.NoRouting) {
            // NoRouting still needs fix for self-loops
            foreach (var edge in node.Edges) {
              if (edge.SelfLoop) {
                FixPorts(graph, edge, path, false);
                FixPorts(graph, edge, path, true);
              }
            }
            continue;
          }

          if (EdgeRoutingMode != RoutingMode.ShortestStraightPathToBorder) {
            continue;
          }

          // enlarge the adjacent segment to the oriented rectangle (represented by the path)
          // handling in and out edges separately will automatically cause selfloops to be handled correctly
          foreach (var edge in node.InEdges) {
            FixPorts(graph, edge, path, false);
          }
          foreach (var edge in node.OutEdges) {
            FixPorts(graph, edge, path, true);
          }
        }
      } finally {
        // if data provider for the port constraints have been added
        // remove and dispose them
        if (addedSourcePortConstraints) {
          graph.RemoveDataProvider(PortConstraintKeys.SourcePortConstraintDpKey);
          graph.DisposeEdgeMap((IEdgeMap)sourcePortConstraints);
        }
        if (addedTargetPortContstraints) {
          graph.RemoveDataProvider(PortConstraintKeys.TargetPortConstraintDpKey);
          graph.DisposeEdgeMap((IEdgeMap)targetPortConstraints);
        }
      }
    }


    /// <summary>
    /// Find the best <see cref="PortSide"/> according to the position of the port.
    /// </summary>
    /// <remarks>
    /// The orientation is not rotated, i.e. <paramref name="bottomLeft"/> is always the anchor
    /// of the oriented rectangle.
    /// </remarks>
    /// <param name="point">The port position.</param>
    /// <param name="bottomLeft">The bottom left corner of the oriented rectangle.</param>
    /// <param name="bottomRight">The bottom right corner.</param>
    /// <param name="topLeft">The top left corner.</param>
    /// <param name="topRight">The top right corner.</param>
    /// <returns>The side to which the given port is closest.</returns>
    private static PortSide FindBestSide(PointD point, PointD bottomLeft, PointD bottomRight, PointD topLeft, PointD topRight) {
      // determine the distances to the sides of the oriented rectangle
      // with a small penalty to the left and right side.
      var distToBottom = point.DistanceToSegment(bottomLeft, bottomRight);
      var distToTop = point.DistanceToSegment(topLeft, topRight);
      var distToLeft = point.DistanceToSegment(topLeft, bottomLeft) * 1.05;
      var distToRight = point.DistanceToSegment(topRight, bottomRight) * 1.05;
      PortSide side;
      if (distToTop <= distToBottom) {
        if (distToTop <= distToLeft) {
          side = distToTop <= distToRight ? PortSide.North : PortSide.East;
        } else {
          side = distToLeft < distToRight ? PortSide.West : PortSide.East;
        }
      } else if (distToBottom <= distToLeft) {
        side = distToBottom <= distToRight ? PortSide.South : PortSide.East;
      } else {
        side = distToLeft < distToRight ? PortSide.West : PortSide.East;
      }
      return side;
    }

    /// <summary>
    /// Fix the <see cref="PortSide"/> of the given edge's port constraints
    /// for the oriented rectangles rotation.
    /// </summary>
    /// <remarks>
    /// If the oriented rectangle is rotated 180° the port sides will be flipped, e.g.
    /// The port constraints will be replaced.
    /// </remarks>
    /// <param name="portConstraints">The data provider for source or target constraints.</param>
    /// <param name="edge">The edge to fix the port constraints for.</param>
    /// <param name="angle">The angle as obtained by applying <see cref="Math.Atan2"/>
    /// to the oriented rectangle's upX and upY vectors.</param>
    private static void FixPortConstraintSide(IDataMap portConstraints, Edge edge, double angle) {
      var constraint = (PortConstraint)portConstraints.Get(edge);
      if (constraint != null && !constraint.AtAnySide) {
        var side = constraint.Side;
        if (angle < Math.PI / 4 && angle > -Math.PI / 4) {
          // top is rotated 90 deg left
          switch (side) {
            case PortSide.West:
              side = PortSide.North;
              break;
            case PortSide.South:
              side = PortSide.West;
              break;
            case PortSide.East:
              side = PortSide.South;
              break;
            case PortSide.North:
              side = PortSide.East;
              break;
          }
        } else if (angle > Math.PI / 4 && angle < Math.PI * 0.75 && angle > 0) {
          // 180 deg
          switch (side) {
            case PortSide.West:
              side = PortSide.East;
              break;
            case PortSide.South:
              side = PortSide.North;
              break;
            case PortSide.East:
              side = PortSide.West;
              break;
            case PortSide.North:
              side = PortSide.South;
              break;
          }
        } else if (angle > Math.PI * 0.75 || angle < -Math.PI * 0.75) {
          // top is rotated 90 deg right
          switch (side) {
            case PortSide.West:
              side = PortSide.South;
              break;
            case PortSide.South:
              side = PortSide.East;
              break;
            case PortSide.East:
              side = PortSide.North;
              break;
            case PortSide.North:
              side = PortSide.West;
              break;
          }

        } else {
          // no rotation
          return;
        }
        // Side is not writable, so set new constraint
        portConstraints.Set(edge, PortConstraint.Create(side, constraint.Strong));
      }
    }

    /// <summary>
    /// Fix the ports for <see cref="RoutingMode.ShortestStraightPathToBorder"/>
    /// by enlarging the adjacent segment to the rotated layout.
    /// </summary>
    /// <param name="graph">The layout graph to work on.</param>
    /// <param name="edge">The edge to fix.</param>
    /// <param name="path">A <see cref="GeneralPath"/> which represents the rotated layout.</param>
    /// <param name="atSource">Whether to fix the source or target port of the edge.</param>
    private static void FixPorts(LayoutGraph graph, Edge edge, GeneralPath path, bool atSource) {
      var el = graph.GetLayout(edge);
      var pointCount = el.PointCount();
      // find the opposite point of the port at the adjacent segment
      PointD firstBend = atSource
        ? (pointCount > 0 ? el.GetPoint(0) : graph.GetTargetPointAbs(edge)).ToPointD()
        : (pointCount > 0 ? el.GetPoint(pointCount - 1) : graph.GetSourcePointAbs(edge)).ToPointD();
      // The port itself
      PointD port = (atSource ? graph.GetSourcePointAbs(edge) : graph.GetTargetPointAbs(edge)).ToPointD();
      // The adjacent segment as vector pointing from the opposite point to the port
      var direction = port - firstBend;
      // find the intersection (there is always one)
      var intersection = path.FindRayIntersection(firstBend.ToPointD(), direction);
      PointD point = port;
      if (intersection < Double.PositiveInfinity) {
        // found an intersection: extend the adjacent segment
        point = firstBend + (direction * intersection);
      } else {
        // no intersection: connect to the original port's nearest point
        var cursor = path.CreateCursor();
        double minDistance = Double.PositiveInfinity;
        while (cursor.MoveNext()) {
          var distance = port.DistanceTo(cursor.CurrentEndPoint);
          if (distance < minDistance) {
            minDistance = distance;
            point = cursor.CurrentEndPoint;
          }
        }
      }
      // set the port position
      if (atSource) {
        graph.SetSourcePointAbs(edge, point.ToYPoint());
      } else {
        graph.SetTargetPointAbs(edge, point.ToYPoint());
      }
    }

    /// <summary>
    /// Remember some aspects of the original layout (before fixing size and before the layout).
    /// </summary>
    private sealed class OldDimensions
    {
      // the offset the position is moved while fixing the size
      public PointD offset;

      // the original size
      public SizeD size;

      // the original location
      public YPoint location;

      // the original outline of the node
      public GeneralPath outline;
    }

    /// <summary>
    /// The mode which determines how to route the edges from the bounding box to the actual layout.
    /// </summary>
    public enum RoutingMode
    {
      /// <summary>
      /// Does nothing.
      /// </summary>
      /// <remarks>
      /// This is ideally suited for layout algorithms which connect to the center of the nodes, e.g. <see cref="OrganicLayout"/>.
      /// </remarks>
      NoRouting,

      /// <summary>
      /// Prolongs the last edge segment until it connects with the actual node shape.
      /// </summary>
      ShortestStraightPathToBorder,

      /// <summary>
      /// Keeps the ports at the position they had before the layout.
      /// </summary>
      FixedPort
    }

    /// <summary>
    /// Data holder used by <see cref="RotatedNodeLayoutDpKey"/>.
    /// </summary>
    public class RotatedNodeShape
    {
      /// <summary>
      /// The <see cref="IShapeGeometry"/> of a node.
      /// </summary>
      public GeneralPath Outline { get; private set; }

      /// <summary>
      /// The rotated layout of a node.
      /// </summary>
      public IOrientedRectangle OrientedLayout { get; private set; }

      /// <summary>
      /// Creates a new instance.
      /// </summary>
      public RotatedNodeShape(GeneralPath outline, IOrientedRectangle orientedLayout) {
        Outline = outline;
        OrientedLayout = orientedLayout;
      }
    }

  }
}
