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
using System.Linq;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Grouping;
using yWorks.Layout.Organic;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace yWorks.Demo.CompactDiskGroups
{
  [Demo("layout-compact-disk-groups", "layout-features", "Compact Disk Groups",
      Summary = "Shows how to configure the compact disk layout to arrange children of group nodes.",
      Tags = ["compact", "disk", "groups"],
      Keywords = ["layout", "round", "nesting", "node types", "recursive", "RecursiveGroupLayout", "playground"])]
  public class CompactDiskGroupsDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize compact disk layout for grouping node contents
      var compactDiskLayout = new CompactDiskLayout {
          MinimumNodeDistance = 4 // Slightly increase node spacing
      };

      // Configure layout data to use node tags as types
      var compactDiskLayoutData = compactDiskLayout.CreateLayoutData(Graph);
      compactDiskLayoutData.NodeTypes.MapperFunction = node => node.Tag;

      // Setup recursive layout to apply compact disk layout separately in each group
      var recursiveGroupLayout = new RecursiveGroupLayout {
          CoreLayout = new OrganicLayout {
              Deterministic = true,
              DefaultMinimumNodeDistance = 20
          }
      };

      // Make recursive layout use compact disk layout for group nodes
      var recursiveGroupLayoutData = recursiveGroupLayout.CreateLayoutData(Graph);
      recursiveGroupLayoutData.GroupNodeLayouts.MapperFunction =
          node => Graph.IsGroupNode(node) ? compactDiskLayout : null;

      // Set circular style for standard nodes
      Graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Pen = Pens.Black,
          Brush = new SolidColorBrush(Color.FromRgb(0x0b, 0x71, 0x89))
      };

      // Configure circular style and appearance for group nodes
      Graph.GroupNodeDefaults.Style = new ShapeNodeStyle {
          Brush = Brushes.White,
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x09, 0x32, 0x37)), 5),
          Shape = ShapeNodeShape.Ellipse
      };

      // Add padding to group nodes to keep contents inside bounds
      Graph.GetDecorator().Nodes.GroupPaddingProvider.AddConstant(
          node => Graph.IsGroupNode(node), new GroupPaddingProvider(new InsetsD(20)));

      // Improve edge cropping at nodes
      Graph.GetDecorator().Ports.EdgePathCropper.AddConstant(new EdgePathCropper { ExtraCropLength = 2 });

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));
      ApplyNodeTypeStyles();

      // Apply the configured layout to the graph
      Graph.ApplyLayout(recursiveGroupLayout, compactDiskLayoutData.CombineWith(recursiveGroupLayoutData));
      GraphControl.FitGraphBounds();
    }

    private void ApplyNodeTypeStyles() {
      foreach (var node in Graph.Nodes.Where(node => !Graph.IsGroupNode(node))) {
        if (node.Tag is not NodeData type) {
          continue;
        }

        var (nodeColor, labelColor) = type.Type switch {
            "A" => (Color.FromRgb(0x24, 0x22, 0x65), Color.FromRgb(0xa7, 0xa7, 0xc1)),
            "B" => (Color.FromRgb(0x61, 0xa0, 0x44), Color.FromRgb(0xc0, 0xd9, 0xb4)),
            "C" => (Color.FromRgb(0xa3, 0x7a, 0xb3), Color.FromRgb(0xda, 0xca, 0xe1)),
            _ => (Color.FromRgb(0x24, 0x22, 0x65), Color.FromRgb(0xa7, 0xa7, 0xc1))
        };

        Graph.SetStyle(node, new ShapeNodeStyle {
            Brush = new SolidColorBrush(nodeColor),
            Pen = Pens.Black,
            Shape = ShapeNodeShape.Ellipse
        });
        foreach (var label in node.Labels) {
          Graph.SetStyle(label, new LabelStyle {
              BackgroundBrush = new SolidColorBrush(labelColor),
              Padding = new InsetsD(2),
              Shape = LabelShape.RoundRectangle
          });
        }
      }
    }

    private record NodeData(string Type);

    [STAThread]
    private static void Main() => Run<CompactDiskGroupsDemo>();
  }
}
