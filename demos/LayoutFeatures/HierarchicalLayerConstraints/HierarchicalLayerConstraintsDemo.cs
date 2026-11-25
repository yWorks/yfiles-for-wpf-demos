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
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalLayerConstraints
{
  /// <summary>
  ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> with configured layer constraints.
  /// </summary>
  [Demo("layout-layer-constraints", "layout-features", "Hierarchical Layout with Layer Constraints",
      Summary = "Shows how to use constraints to control layering in hierarchical layout.",
      Tags = ["hierarchical", "constraints"], Keywords = ["layout", "layering", "playground"])]
  public class HierarchicalLayerConstraintsDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from the JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Run an initial layout with layer constraints
      ApplyHierarchicalLayoutWithLayerConstraints();

      // Add a button to toggle the constraints on and off
      ToolBar.AddToggleButton("Apply Layer Constraints", pressed => {
        if (pressed) {
          ApplyHierarchicalLayoutWithLayerConstraints();
        } else {
          Graph.ApplyLayout(new HierarchicalLayout());
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    /// <summary>
    ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> with configured layer constraints.
    /// </summary>
    private void ApplyHierarchicalLayoutWithLayerConstraints() {
      // Get the nodes for which we want to define layer constraints
      var node0 = Graph.Nodes.First(node => (node.Tag as NodeData)?.Layer == 0);
      var node2 = Graph.Nodes.First(node => (node.Tag as NodeData)?.Layer == 2);
      var node7 = Graph.Nodes.First(node => (node.Tag as NodeData)?.Layer == 7);
      var node9 = Graph.Nodes.First(node => (node.Tag as NodeData)?.Layer == 9);

      var hierarchicalLayout = new HierarchicalLayout();
      var layoutData = hierarchicalLayout.CreateLayoutData(Graph);
      // Place node9 in the topmost layer
      layoutData.LayerConstraints.PlaceAtTop(node9);
      // Place node7 in the bottommost layer
      layoutData.LayerConstraints.PlaceAtBottom(node7);
      // Place node0 at least one layer below node9
      layoutData.LayerConstraints.PlaceInOrder(node9, node0);
      // Place node2 in the same layer as node0
      layoutData.LayerConstraints.PlaceInSameLayer(node0, node2);

      Graph.ApplyLayout(hierarchicalLayout, layoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(int Layer);

    [STAThread]
    private static void Main() => Run<HierarchicalLayerConstraintsDemo>();
  }
}
