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
using System.Windows.Media;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Router;

namespace yWorks.Demo.EdgeRouter
{
  /// <summary>
  ///   Demonstrates common configuration options for the EdgeRouter algorithm.
  /// </summary>
  [Demo("layout-edgerouter", "layout-features", "Edge Router",
      Summary = "Shows common configuration options for the edge routing algorithm.", Tags = ["router"],
      Keywords = [
          "layout", "orthogonal", "octilinear", "port-candidates", "edge-grouping", "scope", "routing-style",
          "EdgeRouterEdgeDescriptor", "playground"
      ])]
  public class EdgeRouterDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize the Edge Router algorithm
      var edgeRouter = new Layout.Router.EdgeRouter {
          DefaultEdgeDescriptor = {
              // Distance between edges
              MinimumEdgeDistance = 5,
              // Minimum length of the first and last segments
              MinimumFirstSegmentLength = 15,
              MinimumLastSegmentLength = 15
          },
          // Minimum distance between nodes and edges
          MinimumNodeToEdgeDistance = 5,
          // Make the router aware of the fixed labels (i.e., try avoiding overlaps with them)
          EdgeLabelPlacement = EdgeRouterEdgeLabelPlacement.ConsiderUnaffectedEdgeLabels,
          // Edge routes must be on a grid with spacing of 10 pixels
          GridSpacing = 10,
          // Set a maximum duration to prevent very long runtimes for LARGE graphs
          StopDuration = TimeSpan.FromSeconds(5)
      };

      // Create the layout data for the edge routing algorithm
      var edgeRouterData = edgeRouter.CreateLayoutData(Graph);
      edgeRouterData.Scope.Edges.Predicate = edge =>
          // Do not route edges with "skipEdgeRouting" tag
          (edge.Tag as EdgeData)?.Type != "skipEdgeRouting";

      // Configure default (orthogonal) routing style
      var defaultDescriptor = edgeRouter.DefaultEdgeDescriptor;
      defaultDescriptor.RoutingStyle = RoutingStyle.Orthogonal;

      // Configure octilinear routing style
      var octilinearDescriptor = defaultDescriptor.CreateCopy();
      octilinearDescriptor.RoutingStyle = RoutingStyle.Octilinear;

      // Use octilinear routing for edges tagged 'octilinear', and default for the rest
      edgeRouterData.EdgeDescriptors.MapperFunction = edge =>
          (edge.Tag as EdgeData)?.Type == "octilinear" ? octilinearDescriptor : defaultDescriptor;

      // Group edges tagged 'groupAtTarget' by giving them the same target group ID
      const string groupId = "goldenGroup";
      edgeRouterData.TargetGroupIds.MapperFunction = edge =>
          (edge.Tag as EdgeData)?.Type == "groupAtTarget" ? groupId : null;

      // Define ports candidates for edges at node 5 and 7 (on the left or right side)
      edgeRouterData.Ports.SourcePortCandidates.MapperFunction = edge =>
          IsRestrictedPortNode(edge.GetSourceNode())
              ? new EdgePortCandidates().AddFreeCandidate(PortSides.Right).AddFreeCandidate(PortSides.Left)
              : null;
      edgeRouterData.Ports.TargetPortCandidates.MapperFunction = edge =>
          IsRestrictedPortNode(edge.GetTargetNode())
              ? new EdgePortCandidates().AddFreeCandidate(PortSides.Right).AddFreeCandidate(PortSides.Left)
              : null;

      BuildGraphFromJson("sample.json", typeof(NodeData), edgeTagType: typeof(EdgeData));

      // Apply edge colors from sample.json.
      foreach (var edge in Graph.Edges) {
        var edgeType = (edge.Tag as EdgeData)?.Type;
        var color = edgeType switch {
            "skipEdgeRouting" => Color.FromRgb(0xf2, 0x64, 0x19),
            "octilinear" => Color.FromRgb(0x01, 0xba, 0xff),
            "groupAtTarget" => Color.FromRgb(0xe0, 0x11, 0xa4),
            _ => (Color?) null
        };
        if (color.HasValue) {
          Graph.SetStyle(edge, new PolylineEdgeStyle { Pen = new Pen(new SolidColorBrush(color.Value), 2.5) });
        }
      }

      // Apply the configured edge router layout
      Graph.ApplyLayout(edgeRouter, edgeRouterData);
      GraphControl.FitGraphBounds();
    }

    private bool IsRestrictedPortNode(INode node) => node.Tag is NodeData data && (data.Type == 5 || data.Type == 7);

    private record NodeData(int Type);

    private record EdgeData(string Type);

    [STAThread]
    private static void Main() => Run<EdgeRouterDemo>();
  }
}
