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
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Hierarchical
{
  [Demo("layout-hierarchical", "layout-features", "Hierarchical Layout",
      Summary = "Shows common configuration options for hierarchical layout.", Tags = ["hierarchical"],
      Keywords = ["layout", "orthogonal", "octilinear", "port-constraints", "critical-path", "playground"])]
  public class HierarchicalDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize the Hierarchical Layout algorithm
      var layout = new HierarchicalLayout {
          DefaultEdgeDescriptor = {
              // The first segment length can be used to reserve "space" for edge source decorations (typically arrowheads)
              // Reduce it here to reduce the overall height of the diagram
              MinimumFirstSegmentLength = 5,
              // Increase the last segment length to prevent bends within target arrowheads.
              MinimumLastSegmentLength = 20
          },
          DefaultNodeDescriptor = {
              // Set vertical alignment for nodes within a layer (0 = top-aligned, useful for varying node heights).
              // See e.g., the layer with nodes 1, 6, 8, 9, 23, and 25
              LayerAlignment = 0
          },
          // The main layout orientation
          LayoutOrientation = LayoutOrientation.BottomToTop,
          // Set a maximum duration to prevent very long runtimes for LARGE graphs
          StopDuration = TimeSpan.FromSeconds(5),
          // Minimum vertical spacing between nodes in different layers
          MinimumLayerDistance = 30,
          // Minimum horizontal distance between nodes and multi-layer edges.
          // See e.g., node 1 and the edge connecting node 0 and node 4
          NodeToEdgeDistance = 50,
          // Minimum horizontal spacing between nodes in the same layer
          NodeDistance = 10
      };
      // Reduce bends for edges spanning layers
      layout.CoordinateAssigner.MaximumPortDeviation = 50;
      // Disable barycenter symmetry optimization
      layout.CoordinateAssigner.SymmetryOptimizationStrategy = SymmetryOptimizationStrategy.None;

      // Initialize layout data for the hierarchical layout algorithm
      var layoutData = layout.CreateLayoutData(Graph);

      // It is possible to specify routing options for specific edges only
      // The following code defines a custom descriptor for octilinear edge routing
      var octilinearEdgeDescriptor = new HierarchicalLayoutEdgeDescriptor {
          MinimumFirstSegmentLength = 5,
          MinimumLastSegmentLength = 20,
          RoutingStyleDescriptor = new RoutingStyleDescriptor { DefaultRoutingStyle = RoutingStyle.Octilinear }
      };
      // Assign octilinear descriptor to edges tagged 'octilinear', otherwise use default (orthogonal)
      layoutData.EdgeDescriptors.MapperFunction = edge =>
          (edge.Tag as EdgeData)?.Type == "octilinear" ? octilinearEdgeDescriptor : layout.DefaultEdgeDescriptor;

      // Prioritize edges tagged 'criticalPath' for straighter routing
      // See the edges connecting nodes 19, 0, 1, and 2
      layoutData.CriticalEdgePriorities.MapperFunction = edge =>
          (edge.Tag as EdgeData)?.Type == "criticalPath" ? 10 : 0;

      // Force the edge from node 20 to node 21 to exit its source node from the RIGHT side
      layoutData.Ports.SourcePortCandidates.MapperFunction = edge =>
          (edge.Tag as EdgeData)?.Type == "rightOut"
              ? new EdgePortCandidates().AddFreeCandidate(PortSides.Right)
              : null;

      // Force the edge from node 9 to node 7 to enter its target node from the RIGHT side
      layoutData.Ports.TargetPortCandidates.MapperFunction = edge =>
          (edge.Tag as EdgeData)?.Type == "rightIn" ? new EdgePortCandidates().AddFreeCandidate(PortSides.Right) : null;

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", edgeTagType: typeof(EdgeData));

      // Apply edge colors from sample.json.
      foreach (var edge in Graph.Edges) {
        var cssClass = (edge.Tag as EdgeData)?.CssClass;
        var color = cssClass switch {
            "orange" => Color.FromRgb(0xf2, 0x64, 0x19),
            "green" => Color.FromRgb(0x76, 0xb0, 0x41),
            "pink" => Color.FromRgb(0xe0, 0x1a, 0x4f),
            "blue" => Color.FromRgb(0x01, 0xba, 0xff),
            _ => (Color?) null
        };
        if (color.HasValue) {
          var brush = new SolidColorBrush(color.Value);
          Graph.SetStyle(edge, new PolylineEdgeStyle {
              Pen = new Pen(brush, 2.5),
              TargetArrow = new Arrow(ArrowType.Stealth, brush)
          });
        }
      }

      // Apply the configured hierarchical layout
      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    private record EdgeData(string? Type, string? CssClass);

    [STAThread]
    private static void Main() => Run<HierarchicalDemo>();
  }
}
