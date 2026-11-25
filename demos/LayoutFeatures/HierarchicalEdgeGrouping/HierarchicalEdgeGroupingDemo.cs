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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalEdgeGrouping
{
  /// <summary>
  ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> with edge grouping.
  /// </summary>
  [Demo("layout-hierarchical-edge-grouping", "layout-features", "Hierarchical Layout with Edge Grouping",
      Summary = "Shows how to configure edge grouping for hierarchical layout.", Tags = ["hierarchical", "edgegroups"],
      Keywords = ["layout", "groups", "playground"])]
  public class HierarchicalEdgeGroupingDemo : SimpleDemo
  {
    protected override void Initialize() {
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Run an initial layout with edge grouping
      ApplyHierarchicalLayoutWithEdgeGrouping();

      // Add a button to toggle the edge grouping on and off
      ToolBar.AddToggleButton("Apply Edge Grouping", pressed => {
        if (pressed) {
          ApplyHierarchicalLayoutWithEdgeGrouping();
        } else {
          var layout = new HierarchicalLayout();
          Graph.ApplyLayout(layout);
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    private void ApplyHierarchicalLayoutWithEdgeGrouping() {
      var layout = new HierarchicalLayout();
      var layoutData = layout.CreateLayoutData(Graph);

      // Use LayoutData to configure edge grouping
      // Group edges at their source side if they start from node 0
      layoutData.SourceGroupIds.MapperFunction = edge =>
          (edge.GetSourceNode().Tag as NodeData)?.Type == 0 ? "edgesFromNode0" : null;
      // Group edges at their target side if they end at node 5
      layoutData.TargetGroupIds.MapperFunction = edge =>
          (edge.GetTargetNode().Tag as NodeData)?.Type == 5 ? "edgesToNode5" : null;

      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(int Type);

    [STAThread]
    private static void Main() => Run<HierarchicalEdgeGroupingDemo>();
  }
}
