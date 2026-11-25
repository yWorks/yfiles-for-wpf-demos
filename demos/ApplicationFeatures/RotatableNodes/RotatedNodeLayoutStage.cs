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
using yWorks.Geometry;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Organic;
using yWorks.Utils;

namespace yWorks.Demo.RotatableNodes
{
  /// <summary>
  ///   Layout Stage which handles <see cref="RotatableNodeStyleDecorator">rotated nodes</see>.
  /// </summary>
  /// <remarks>
  ///   During the <see cref="LayoutStageBase.CoreLayout"/> the layout is calculated with the rotated node's bounds,
  ///   i.e. a rectangular box which is large enough to fully include the rotated node.
  ///   The edges are connected with the actual rotated shape of the node according to the <see cref="EdgeRoutingMode"/>.
  /// </remarks>
  public class RotatedNodeLayoutStage : LayoutStageBase
  {
    /// <summary>
    ///   Creates a new instance with an optional core layout algorithm.
    /// </summary>
    public RotatedNodeLayoutStage(ILayoutAlgorithm coreLayout = null) : base(coreLayout) {
      EdgeRoutingMode = RoutingMode.ShortestStraightPathToBorder;
    }

    /// <summary>
    ///   The <see cref="IMapper{K,V}"/> key to register a data provider that provides the outline and oriented
    ///   layout to this stage.
    /// </summary>
    public static readonly NodeDataKey<RotatedNodeShape> RotatedNodeLayoutDataKey =
        new("yWorks.Demo.RotatableNodes.RotatedNodeLayoutStage.RotatedNodeLayoutDataKey");

    /// <summary>
    ///   Gets or sets the mode to use to connect edges from the bounds to the actual shape.
    /// </summary>
    public RoutingMode EdgeRoutingMode { get; set; }

    /// <summary>
    ///   Executes the layout algorithm.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Enlarges the node layout to fully encompass the rotated layout (the rotated layout's bounds).
    ///     If the <see cref="EdgeRoutingMode"/> is set to <see cref="RoutingMode.FixedPort"/>,
    ///     port candidates are created to keep the ports at their current location.
    ///     Existing port candidates are adjusted to the rotation.
    ///   </para>
    ///   <para>
    ///     Then, the <see cref="LayoutStageBase.CoreLayout"/> is executed.
    ///   </para>
    ///   <para>
    ///     After the core layout, the original node sizes are restored.
    ///     If the <see cref="EdgeRoutingMode"/> is set to <see cref="RoutingMode.ShortestStraightPathToBorder"/>,
    ///     the last edge segment is extended from the bounds to the rotated layout.
    ///   </para>
    /// </remarks>
    protected override void ApplyLayoutImpl(LayoutGraph graph) {
      if (CoreLayout == null) {
        return;
      }

      var boundsProvider = graph.Context.GetItemData(RotatedNodeLayoutDataKey);
      if (boundsProvider == null) {
        // no provider: this stage adds nothing to the core layout
        CoreLayout.ApplyLayout(graph);
        return;
      }

      var addedSourcePortCandidates = false;
      var addedTargetPortCandidates = false;
      var sourcePortCandidates = graph.Context.GetItemData(EdgePortCandidates.SourcePortCandidatesDataKey);
      var targetPortCandidates = graph.Context.GetItemData(EdgePortCandidates.TargetPortCandidatesDataKey);
      if (EdgeRoutingMode == RoutingMode.FixedPort) {
        // Fixed port: create port constraints to keep the ports at position
        // in this case: create data providers if there are none yet
        if (sourcePortCandidates == null) {
          sourcePortCandidates = graph.CreateEdgeDataMap<EdgePortCandidates>();
          graph.Context.AddItemData(EdgePortCandidates.SourcePortCandidatesDataKey, sourcePortCandidates);
          addedSourcePortCandidates = true;
        }
        if (targetPortCandidates == null) {
          targetPortCandidates = graph.CreateEdgeDataMap<EdgePortCandidates>();
          graph.Context.AddItemData(EdgePortCandidates.TargetPortCandidatesDataKey, targetPortCandidates);
          addedTargetPortCandidates = true;
        }
      }
      try {
        var originalDimensions = new Dictionary<Node, OldDimensions>();
        foreach (var node in graph.Nodes) {
          var nodeShape = boundsProvider[node];
          var orientedLayout = nodeShape != null ? nodeShape.OrientedLayout : null;
          var outline = nodeShape != null ? nodeShape.Outline : null;
          if (orientedLayout != null) {
            // if the current node is rotated: apply fixes
            // remember old layout and size
            var oldLayout = node.Layout;
            var newLayout = orientedLayout.GetBounds();
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
              var tl = new PointD(orientedLayout.AnchorX + orientedLayout.UpX * orientedLayout.Height,
                  orientedLayout.AnchorY + orientedLayout.UpY * orientedLayout.Height);
              var tr = new PointD(
                  orientedLayout.AnchorX + orientedLayout.UpX * orientedLayout.Height -
                  orientedLayout.UpY * orientedLayout.Width,
                  orientedLayout.AnchorY + orientedLayout.UpY * orientedLayout.Height +
                  orientedLayout.UpX * orientedLayout.Width);
              var bl = new PointD(orientedLayout.AnchorX, orientedLayout.AnchorY);
              var br = new PointD(orientedLayout.AnchorX - orientedLayout.UpY * orientedLayout.Width,
                  orientedLayout.AnchorY + orientedLayout.UpX * orientedLayout.Width);

              // for each out edge
              foreach (var edge in node.OutEdges) {
                // create a fixed port candidate for the side which is closest to the port location (without rotation)
                var constraint = sourcePortCandidates[edge];
                if (constraint == null) {
                  var point = edge.SourcePortLocation;
                  var side = FindBestSide(point, bl, br, tl, tr);
                  sourcePortCandidates[edge] = new EdgePortCandidates().AddFixedCandidate(side);
                }
              }
              foreach (var edge in node.InEdges) {
                // create a fixed port candidate for the side which is closest to the port location (without rotation)
                var constraint = targetPortCandidates[edge];
                if (constraint == null) {
                  var point = edge.TargetPortLocation;
                  var side = FindBestSide(point, bl, br, tl, tr);
                  targetPortCandidates[edge] = new EdgePortCandidates().AddFixedCandidate(side);
                }
              }
            }

            // For source and target port candidates: fix the PortSide according to the rotation
            var angle = Math.Atan2(orientedLayout.UpY, orientedLayout.UpX);
            if (sourcePortCandidates != null) {
              foreach (var edge in node.OutEdges) {
                FixPortConstraintSide(sourcePortCandidates, edge, angle);
              }
            }
            if (targetPortCandidates != null) {
              foreach (var edge in node.InEdges) {
                FixPortConstraintSide(targetPortCandidates, edge, angle);
              }
            }

            // enlarge the node layout
            var position = new PointD(newLayout.X, newLayout.Y);
            oldDimensions.location = position;
            originalDimensions.Add(node, oldDimensions);
            node.Layout.TopLeft = position;
            node.Layout.Width = newLayout.Width;
            node.Layout.Height = newLayout.Height;
          }
        }

        // ===============================================================

        CoreLayout.ApplyLayout(graph);

        // ===============================================================

        foreach (var node in graph.Nodes) {
          if (graph.IsGroupNode(node)) {
            // groups don't need to be adjusted to their former size and location because their bounds are entirely
            // calculated by the layout algorithm and they are not rotated
            continue;
          }

          // for each node which has been corrected: undo the correction
          var oldDimensions = originalDimensions[node];
          var offset = oldDimensions.offset;
          var originalSize = oldDimensions.size;
          var newLayout = node.Layout;

          // create a general path representing the new roated layout
          var path = oldDimensions.outline;
          var transform = new Matrix2D();
          transform.Translate(
              new PointD(newLayout.X - oldDimensions.location.X, newLayout.Y - oldDimensions.location.Y));
          path.Transform(transform);

          // restore the original size
          node.Layout.X = newLayout.X - offset.X;
          node.Layout.Y = newLayout.Y - offset.Y;
          node.Layout.Width = originalSize.Width;
          node.Layout.Height = originalSize.Height;

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
          // handling in and out edges separately will automatically cause self-loops to be handled correctly
          foreach (var edge in node.InEdges) {
            FixPorts(graph, edge, path, false);
          }
          foreach (var edge in node.OutEdges) {
            FixPorts(graph, edge, path, true);
          }
        }
      } finally {
        // if data provider for the port candidates have been added
        // remove and dispose them
        if (addedSourcePortCandidates) {
          graph.Context.Remove(EdgePortCandidates.SourcePortCandidatesDataKey);
          graph.DisposeDataMap(sourcePortCandidates);
        }
        if (addedTargetPortCandidates) {
          graph.Context.Remove(EdgePortCandidates.TargetPortCandidatesDataKey);
          graph.DisposeDataMap(targetPortCandidates);
        }
      }
    }


    /// <summary>
    ///   Find the best <see cref="PortSides">port side</see> according to the position of the port.
    /// </summary>
    /// <remarks>
    ///   The orientation is not rotated, i.e. <paramref name="bottomLeft"/> is always the anchor
    ///   of the oriented rectangle.
    /// </remarks>
    /// <param name="point">The port position.</param>
    /// <param name="bottomLeft">The bottom left corner of the oriented rectangle.</param>
    /// <param name="bottomRight">The bottom right corner.</param>
    /// <param name="topLeft">The top left corner.</param>
    /// <param name="topRight">The top right corner.</param>
    /// <returns>The side to which the given port is closest.</returns>
    private static PortSides FindBestSide(
        PointD point,
        PointD bottomLeft,
        PointD bottomRight,
        PointD topLeft,
        PointD topRight) {
      // determine the distances to the sides of the oriented rectangle
      // with a small penalty to the left and right side.
      var distToBottom = point.DistanceToSegment(bottomLeft, bottomRight);
      var distToTop = point.DistanceToSegment(topLeft, topRight);
      var distToLeft = point.DistanceToSegment(topLeft, bottomLeft) * 1.05;
      var distToRight = point.DistanceToSegment(topRight, bottomRight) * 1.05;
      PortSides side;
      if (distToTop <= distToBottom) {
        if (distToTop <= distToLeft) {
          side = distToTop <= distToRight ? PortSides.Top : PortSides.Right;
        } else {
          side = distToLeft < distToRight ? PortSides.Left : PortSides.Right;
        }
      } else if (distToBottom <= distToLeft) {
        side = distToBottom <= distToRight ? PortSides.Bottom : PortSides.Right;
      } else {
        side = distToLeft < distToRight ? PortSides.Left : PortSides.Right;
      }
      return side;
    }

    /// <summary>
    ///   Fix the <see cref="PortSides">port side</see> of the given edge's port candidates
    ///   for the oriented rectangles rotation.
    /// </summary>
    /// <remarks>
    ///   If the oriented rectangle is rotated 180° the port sides will be flipped, e.g.
    ///   The port candidates will be replaced.
    /// </remarks>
    /// <param name="portCandidates">The data provider for source or target candidates.</param>
    /// <param name="edge">The edge to fix the port candidates for.</param>
    /// <param name="angle">
    ///   The angle as obtained by applying <see cref="Math.Atan2"/>
    ///   to the oriented rectangle's upX and upY vectors.
    /// </param>
    private static void FixPortConstraintSide(
        IMapper<Edge, EdgePortCandidates> portCandidates,
        Edge edge,
        double angle) {
      var candidate = portCandidates[edge]?.Candidates.First();
      if (candidate != null && !candidate.IsOnAnySide()) {
        var direction = candidate.Side;
        if (angle < Math.PI / 4 && angle > -Math.PI / 4) {
          // top is rotated 90 deg left
          switch (direction) {
            case PortSides.Left:
              direction = PortSides.Top;
              break;
            case PortSides.Bottom:
              direction = PortSides.Left;
              break;
            case PortSides.Right:
              direction = PortSides.Bottom;
              break;
            case PortSides.Top:
              direction = PortSides.Right;
              break;
          }
        } else if (angle > Math.PI / 4 && angle < Math.PI * 0.75 && angle > 0) {
          // 180 deg
          switch (direction) {
            case PortSides.Left:
              direction = PortSides.Right;
              break;
            case PortSides.Bottom:
              direction = PortSides.Top;
              break;
            case PortSides.Right:
              direction = PortSides.Left;
              break;
            case PortSides.Top:
              direction = PortSides.Bottom;
              break;
          }
        } else if (angle > Math.PI * 0.75 || angle < -Math.PI * 0.75) {
          // top is rotated 90 deg right
          switch (direction) {
            case PortSides.Left:
              direction = PortSides.Bottom;
              break;
            case PortSides.Bottom:
              direction = PortSides.Right;
              break;
            case PortSides.Right:
              direction = PortSides.Top;
              break;
            case PortSides.Top:
              direction = PortSides.Left;
              break;
          }
        } else {
          // no rotation
          return;
        }
        // Side is not writable, so set new candidate
        portCandidates[edge] = candidate.Type == PortCandidateType.Free
            ? new EdgePortCandidates().AddFreeCandidate(direction)
            : new EdgePortCandidates().AddFixedCandidate(direction);
      }
    }

    /// <summary>
    ///   Fix the ports for <see cref="RoutingMode.ShortestStraightPathToBorder"/>
    ///   by enlarging the adjacent segment to the rotated layout.
    /// </summary>
    /// <param name="graph">The layout graph to work on.</param>
    /// <param name="edge">The edge to fix.</param>
    /// <param name="path">A <see cref="GeneralPath"/> which represents the rotated layout.</param>
    /// <param name="atSource">Whether to fix the source or target port of the edge.</param>
    private static void FixPorts(LayoutGraph graph, Edge edge, GeneralPath path, bool atSource) {
      var bendCount = edge.Bends.Count;
      // find the opposite point of the port at the adjacent segment
      var firstBend = atSource ? bendCount > 0 ? edge.Bends.First.Location : edge.TargetPortLocation :
          bendCount > 0 ? edge.Bends.Last.Location : edge.SourcePortLocation;
      // The port itself
      var port = atSource ? edge.SourcePortLocation : edge.TargetPortLocation;
      // The adjacent segment as vector pointing from the opposite point to the port
      var direction = port - firstBend;
      // find the intersection (there is always one)
      var intersection = path.FindRayIntersection(firstBend, direction);
      var point = port;
      if (intersection < double.PositiveInfinity) {
        // found an intersection: extend the adjacent segment
        point = firstBend + (direction * intersection);
      } else {
        // no intersection: connect to the original port's nearest point
        var cursor = path.CreateCursor();
        var minDistance = double.PositiveInfinity;
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
        edge.SourcePortLocation = point;
      } else {
        edge.TargetPortLocation = point;
      }
    }

    /// <summary>
    ///   Remember some aspects of the original layout (before fixing size and before the layout).
    /// </summary>
    private sealed class OldDimensions
    {
      // the offset the position is moved while fixing the size
      public PointD offset;

      // the original size
      public SizeD size;

      // the original location
      public PointD location;

      // the original outline of the node
      public GeneralPath outline;
    }

    /// <summary>
    ///   The mode which determines how to route the edges from the bounds to the actual layout.
    /// </summary>
    public enum RoutingMode
    {
      /// <summary>
      ///   Does nothing.
      /// </summary>
      /// <remarks>
      ///   This is ideally suited for layout algorithms which connect to the center of the nodes, e.g.
      ///   <see cref="OrganicLayout"/>.
      /// </remarks>
      NoRouting,

      /// <summary>
      ///   Prolongs the last edge segment until it connects with the actual node shape.
      /// </summary>
      ShortestStraightPathToBorder,

      /// <summary>
      ///   Keeps the ports at the position they had before the layout.
      /// </summary>
      FixedPort
    }

    /// <summary>
    ///   Data holder used by <see cref="RotatedNodeLayoutStage.RotatedNodeLayoutDataKey"/>.
    /// </summary>
    public class RotatedNodeShape
    {
      /// <summary>
      ///   The <see cref="IShapeGeometry"/> of a node.
      /// </summary>
      public GeneralPath Outline { get; }

      /// <summary>
      ///   The rotated layout of a node.
      /// </summary>
      public IOrientedRectangle OrientedLayout { get; }

      /// <summary>
      ///   Creates a new instance.
      /// </summary>
      public RotatedNodeShape(GeneralPath outline, IOrientedRectangle orientedLayout) {
        Outline = outline;
        OrientedLayout = orientedLayout;
      }
    }
  }
}
