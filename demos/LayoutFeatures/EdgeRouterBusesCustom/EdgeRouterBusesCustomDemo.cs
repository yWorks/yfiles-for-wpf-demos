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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.EdgeRouterBusesCustom
{
  /// <summary>
  ///   Demonstrates how to configure the EdgeRouter to generate orthogonal bus-style routes with manually defined backbone
  ///   points.
  /// </summary>
  [Demo("layout-edge-router-buses-custom", "layout-features", "Bus-Style Edge Routing (Custom)",
      Summary =
          "Shows how to configure the edge routing algorithm to produce orthogonal bus-style paths with custom backbone points.",
      Tags = ["routing", "orthogonal", "bus"],
      Keywords = [
          "layout", "edge router", "edgegroups", "bus", "bus structures", "backbone", "givenPoints",
          "EdgeRouterBusDescriptor", "playground"
      ])]
  public class EdgeRouterBusesCustomDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // Initialize layout data for the edge router algorithm
      var edgeRouter = new EdgeRouter();
      var edgeRouterData = edgeRouter.CreateLayoutData(Graph);

      // Prepare the bus for custom backbone points
      var customBusDescriptor = new BusDescriptor();

      // Add the bus to layout data, assign all edges to it
      edgeRouterData.Buses.Add(customBusDescriptor).Source = Graph.Edges;

      // Create a custom backbone path for the bus
      var backbonePoints = CreateBackbonePath();

      // Assign backbone points to the bus descriptor
      customBusDescriptor.BusPoints = backbonePoints;

      // Apply the configured edge router layout
      Graph.ApplyLayout(edgeRouter, edgeRouterData);
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a custom backbone path for the bus.
    /// </summary>
    private PointD[] CreateBackbonePath() {
      // Calculate bounding box of all graph nodes
      var bounds = GetBoundingBox(Graph.Nodes);

      // Constants for the backbone layout
      const double inset = 100;
      const double rowHeight = 50;
      const double nodeRowDistance = 70;
      var backboneLeftX = bounds.X - inset;

      var backbone = new List<PointD>();

      // Start left, go down to where the first "backbone row" should start
      backbone.Add(new PointD(backboneLeftX, bounds.Y - inset));
      var backboneFirstRowY = bounds.Y + rowHeight + nodeRowDistance * 0.5;
      backbone.Add(new PointD(backboneLeftX, backboneFirstRowY));

      // Go right and back again, creating the first row
      backbone.Add(new PointD(bounds.GetMaxX() + inset, backboneFirstRowY));
      backbone.Add(new PointD(backboneLeftX, backboneFirstRowY));

      // Go down to the second row
      var backboneSecondRowY = backboneFirstRowY + nodeRowDistance + rowHeight;
      backbone.Add(new PointD(backboneLeftX, backboneSecondRowY));

      // Go right and back again, creating the second backbone row
      backbone.Add(new PointD(bounds.GetMaxX() + inset, backboneSecondRowY));
      backbone.Add(new PointD(backboneLeftX, backboneSecondRowY));

      // Go down, prolonging the left vertical backbone a bit
      backbone.Add(new PointD(backboneLeftX, bounds.GetMaxY() + inset));

      return [.. backbone];
    }

    /// <summary>
    ///   Returns the bounding box of the given nodes.
    /// </summary>
    private static RectD GetBoundingBox(IEnumerable<INode> nodes) {
      var nodeBounds = RectD.Empty;
      foreach (var node in nodes) {
        nodeBounds += node.Layout.ToRectD();
      }
      return nodeBounds;
    }

    [STAThread]
    private static void Main() => Run<EdgeRouterBusesCustomDemo>();
  }
}
