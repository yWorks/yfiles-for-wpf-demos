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
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout;
using yWorks.Layout.Organic;

namespace yWorks.Demo.BoundaryLabeling
{
  /// <summary>
  ///   Provides the layout configuration and helper methods used by the demo.
  /// </summary>
  internal static class ConfigureLayout
  {
    /// <summary>
    ///   Returns a layout configuration of the organic layout with constraints so that:
    ///   (i) Label nodes associated with points on the left side of the image should be placed before their
    ///   associated points but not necessarily horizontally aligned with them.
    ///   (ii) Label nodes associated with points on the right side of the image should be placed after their
    ///   associated points but not necessarily horizontally aligned with them.
    ///   (iii) Label nodes associated with points on the top side of the image should be placed above their
    ///   associated points but not necessarily vertically aligned with them.
    ///   (iv) Label nodes on the left or on the right side should be vertically aligned.
    ///   Labels associated with points on the top are excluded from this rule.
    /// </summary>
    public static (OrganicLayout Layout, OrganicLayoutData<INode, IEdge, ILabel, ILabel> LayoutData) Create(
        IGraph graph,
        RectD imageRect) {
      // positions all label nodes at the position of their associated points
      ResetLabelPositions(graph);
      // define the label models based on the position of the point
      AssignLabelModels(graph, imageRect);

      // some basic configuration for the organic layout
      // scope SUBSET is used because the locations of the points may not change i.e., the algorithm may
      // arrange only label nodes
      var organicLayout = new OrganicLayout {
          Deterministic = true,
          DefaultMinimumNodeDistance = 5
      };

      // specifies desired edge lengths and the set of nodes to be arranged, i.e., the label nodes
      var organicLayoutData = organicLayout.CreateLayoutData(graph);
      organicLayoutData.PreferredEdgeLengths.MapperFunction =
          edge => IsLeftPoint(edge.GetSourceNode(), imageRect) ? 50 : 25;
      organicLayoutData.Scope.Nodes.Source = graph.Nodes.Where(DataTypes.IsLabel);

      // create the constraints for the layout algorithm
      foreach (var edge in graph.Edges) {
        var point = edge.GetSourceNode();
        var labelNode = edge.GetTargetNode();

        if (IsVerticalPoint(point, imageRect)) {
          // create the constraints for the nodes on the top/bottom part
          AddVerticalConstraints(point, labelNode, organicLayoutData, imageRect);
        } else {
          AddHorizontalConstraints(point, labelNode, organicLayoutData, imageRect);
        }
      }

      // create the constraints to vertically align the label nodes, excluding the ones on the top part
      AddVerticalLabelToLabelConstraints(graph, imageRect, organicLayoutData);

      return (organicLayout, organicLayoutData);
    }

    /// <summary>
    ///   Creates horizontal constraints so that:
    ///   (i) Label nodes associated with points on the left side of the image are placed to the left of
    ///   their associated point nodes.
    ///   (ii) Label nodes associated with points on the right side of the image are placed to the right
    ///   of their associated point nodes.
    /// </summary>
    private static void AddHorizontalConstraints(
        INode point,
        INode labelNode,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData,
        RectD imageRect) {
      var pointLeft = IsLeftPoint(point, imageRect);

      // determine which node should be on the left/right based on the location of the point
      var left = pointLeft ? labelNode : point;
      var right = pointLeft ? point : labelNode;

      // calculate the distance of the point to the left/right boundary of the image to make sure
      // that the label node is placed outside the image
      var distanceToBorder = pointLeft
          ? Math.Abs(point.Layout.X - imageRect.X)
          : Math.Abs(point.Layout.X - imageRect.X - imageRect.Width);

      // enforce a minimum distance between the two nodes
      var separationConstraint = organicLayoutData.Constraints.AddSeparationConstraint(
          ConstraintOrientation.Horizontal, distanceToBorder + 20);
      separationConstraint.FirstSet.Item = left;
      separationConstraint.SecondSet.Item = right;

      // create a constraint to enforce the desired order of the point and label node
      var orderConstraint = organicLayoutData.Constraints.AddOrderConstraint(ConstraintOrientation.Horizontal);
      orderConstraint.Dictionary = new Dictionary<INode, IComparable> {
          [left] = 1,
          [right] = 2
      };
    }

    /// <summary>
    ///   Creates constraints that force label nodes associated with points on the top/bottom side of
    ///   the image to be placed above/below their associated points but not necessarily vertically
    ///   aligned with them.
    /// </summary>
    private static void AddVerticalConstraints(
        INode point,
        INode labelNode,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData,
        RectD imageRect) {
      var isTop = IsTopPoint(point, imageRect);
      // determine the top-to-bottom order of the point node and its label node depending on the
      // location of the point node
      var top = isTop ? labelNode : point;
      var bottom = isTop ? point : labelNode;

      // calculate the distance of the point to the top/bottom boundary of the image to make sure
      // that the label node is placed outside the image
      var distanceToBorder = isTop
          ? Math.Abs(point.Layout.Y - imageRect.Y)
          : Math.Abs(point.Layout.Y - imageRect.Y - imageRect.Height);

      // enforce a minimum distance between the two nodes
      var separationConstraint = organicLayoutData.Constraints.AddSeparationConstraint(
          ConstraintOrientation.Vertical, distanceToBorder + 20);
      separationConstraint.FirstSet.Item = top;
      separationConstraint.SecondSet.Item = bottom;

      // create a constraint to enforce the desired ordering of the point and label node
      // based on whether the point node should lie above or below its label node
      var orderConstraint = organicLayoutData.Constraints.AddOrderConstraint(ConstraintOrientation.Vertical);
      orderConstraint.Dictionary = new Dictionary<INode, IComparable> {
          [top] = 1,
          [bottom] = 2
      };
    }

    /// <summary>
    ///   Create constraints to vertically align the label nodes excluding the ones on the top/bottom part.
    /// </summary>
    private static void AddVerticalLabelToLabelConstraints(
        IGraph graph,
        RectD imageRect,
        OrganicLayoutData<INode, IEdge, ILabel, ILabel> organicLayoutData) {
      // get all label nodes that do not lie on the topmost/bottommost part
      var nonVerticalPoints = graph.Nodes.Where(node => DataTypes.IsLabel(node) && !IsVerticalPoint(node, imageRect))
          .ToArray();

      // get the labels on the left side and create a constraint to align them vertically
      var leftAlignmentConstraint =
          organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical);
      leftAlignmentConstraint.Items = [.. nonVerticalPoints.Where(node => IsLeftPoint(node, imageRect))];

      // get the labels on the right side and create a constraint to align them vertically
      var rightAlignmentConstraint =
          organicLayoutData.Constraints.AddAlignmentConstraint(ConstraintOrientation.Vertical);
      rightAlignmentConstraint.Items = [.. nonVerticalPoints.Where(node => !IsLeftPoint(node, imageRect))];
    }

    /// <summary>
    ///   Returns whether the point lies on the left side of the image.
    /// </summary>
    private static bool IsLeftPoint(INode point, RectD imageRect) =>
        point.Layout.X < imageRect.X + imageRect.Width * 0.5;

    /// <summary>
    ///   Returns whether the point lies on the top of the image, i.e., the distance from the top border is smaller
    ///   than 30.
    /// </summary>
    private static bool IsTopPoint(INode point, RectD imageRect) => Math.Abs(point.Layout.Y - imageRect.Y) < 30;

    /// <summary>
    ///   Returns whether the point lies on the bottom of the image, i.e., the distance from the bottom border is smaller
    ///   than 30.
    /// </summary>
    private static bool IsBottomPoint(INode point, RectD imageRect) =>
        Math.Abs(point.Layout.Y - imageRect.Y - imageRect.Height) < 30;

    /// <summary>
    ///   Returns whether the connection between the point and its label node should be vertical,
    ///   i.e., the point lies on the topmost/bottommost part of the image.
    /// </summary>
    private static bool IsVerticalPoint(INode point, RectD imageRect) =>
        IsTopPoint(point, imageRect) || IsBottomPoint(point, imageRect);

    /// <summary>
    ///   Defines the positions of the labels based on the location of the point.
    /// </summary>
    private static void AssignLabelModels(IGraph graph, RectD imageRect) {
      foreach (var labelNode in graph.Nodes.Where(DataTypes.IsLabel)) {
        var label = labelNode.Labels.First();

        if (IsVerticalPoint(labelNode, imageRect)) {
          var isTop = IsTopPoint(labelNode, imageRect);
          // for labels that should go vertically, i.e., point is on the top or at the bottom
          // the label should go above the node or below the node, respectively
          graph.SetLabelLayoutParameter(label, isTop ? ExteriorNodeLabelModel.Top : ExteriorNodeLabelModel.Bottom);
        } else {
          var isLeft = IsLeftPoint(labelNode, imageRect);
          // for labels that should go horizontally, (i.e., point is on the left or on the right)
          // the label should go before the node or after the node, respectively
          graph.SetLabelLayoutParameter(label, isLeft ? ExteriorNodeLabelModel.Left : ExteriorNodeLabelModel.Right);
        }
      }
    }

    /// <summary>
    ///   For better layout results when the graph is re-arranged after font changes, the label
    ///   nodes are placed initially on the positions of the point nodes.
    /// </summary>
    private static void ResetLabelPositions(IGraph graph) {
      foreach (var labelNode in graph.Nodes.Where(DataTypes.IsLabel)) {
        var point = graph.EdgesAt(labelNode).First().GetSourceNode();
        graph.SetNodeLayout(labelNode,
            new RectD(point.Layout.X, point.Layout.Y, labelNode.Layout.Width, labelNode.Layout.Height));
      }
    }
  }
}
