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
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Grouping;

namespace yWorks.Demo.RadialGroupLayout
{
  /// <summary>
  ///   Demonstrates the RadialGroupLayout algorithm for arranging grouped graphs.
  /// </summary>
  [Demo("layout-radial-group-layout", "layout-features", "Radial Group Layout",
      Summary = "Shows how to configure the radial group layout to arrange grouped graphs.",
      Tags = ["radial", "groups"],
      Keywords = ["layout", "hierarchy", "round", "nesting", "bundling", "fractal", "cactus", "playground"])]
  public class RadialGroupLayoutDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Configure radial group layout
      var radialGroupLayout = new Layout.Grouping.RadialGroupLayout {
          // Use default group sizing for compact drawings
          GroupSizePolicy = GroupSizePolicy.Adaptive,
          // Place labels at leaf nodes ray-like
          NodeLabelPlacement = RadialNodeLabelPlacement.RayLikeLeaves,
          // Increase child distribution angle
          PreferredRootSectorAngle = 200
      };
      radialGroupLayout.EdgeBundling.DefaultBundleDescriptor = new EdgeBundleDescriptor {
          // Enable bundled edges
          Bundled = true,
          // Enable smoother curves in conjunction with BezierEdgeStyle
          BezierFitting = true
      };

      // Configure radial group layout data
      var radialGroupLayoutData = radialGroupLayout.CreateLayoutData(Graph);
      // Allow more overlap with parent nodes (except those tagged "avoidParentOverlap") for compact drawings
      radialGroupLayoutData.ParentOverlapRatios.MapperFunction = node =>
          (node.Tag as NodeData)?.AvoidParentOverlap == true ? 0 : 0.5;

      // Use round nodes that are typical for radial layout
      if (Graph.NodeDefaults.Style is ShapeNodeStyle nodeStyle) {
        nodeStyle.Shape = ShapeNodeShape.Ellipse;
      }
      Graph.GroupNodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = new SolidColorBrush(Color.FromRgb(0xce, 0xd6, 0xd7))
      };

      // Use BezierEdgeStyle for edges for smoother curves
      Graph.EdgeDefaults.Style = new BezierEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x09, 0x32, 0x37)), 1.5)
      };

      // Use FreeNodeLabelModel for maximum layout flexibility
      Graph.NodeDefaults.Labels.LayoutParameter = FreeNodeLabelModel.Center;

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Apply the configured radial layout to the graph
      Graph.ApplyLayout(radialGroupLayout, radialGroupLayoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(bool? AvoidParentOverlap);

    [STAThread]
    private static void Main() => Run<RadialGroupLayoutDemo>();
  }
}
