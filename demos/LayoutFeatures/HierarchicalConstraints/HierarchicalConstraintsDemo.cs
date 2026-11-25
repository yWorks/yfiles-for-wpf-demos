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

namespace yWorks.Demo.HierarchicalConstraints
{
  [Demo("layout-constraints", "layout-features", "Hierarchical Layout with Constraints",
      Summary = "Shows how to use constraints to control layering and sequencing in the hierarchical layout.",
      Tags = ["hierarchical", "constraints"], Keywords = ["layout", "playground"])]
  public class HierarchicalConstraintsDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Run an initial layout with constraints
      ApplyHierarchicalLayoutWithConstraints();

      // Add a button to toggle the constraints on and off
      ToolBar.AddToggleButton("Apply Constraints", pressed => {
        if (pressed) {
          ApplyHierarchicalLayoutWithConstraints();
        } else {
          Graph.ApplyLayout(new HierarchicalLayout());
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    /// <summary>
    ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> with configured constraints.
    /// </summary>
    private void ApplyHierarchicalLayoutWithConstraints() {
      // Get the nodes for which we want to define constraints
      var node4 = Graph.Nodes.First(node => (node.Tag as NodeData)?.Type == 4);
      var node5 = Graph.Nodes.First(node => (node.Tag as NodeData)?.Type == 5);

      // Initialize a layout data object to define constraints
      var hierarchicalLayout = new HierarchicalLayout();
      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(Graph);

      // Define a sequence constraint: Node 5 must follow Node 4 in the layout order
      hierarchicalLayoutData.SequenceConstraints.PlaceInOrder(node4, node5);

      // Define a layer constraint: Node 4 and 5 must be placed in the same layer
      // Otherwise, HierarchicalLayout normally places nodes so that edges point in the layout direction (top to bottom by default)
      hierarchicalLayoutData.LayerConstraints.PlaceInSameLayer(node4, node5);

      Graph.ApplyLayout(hierarchicalLayout, hierarchicalLayoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(int? Type);

    [STAThread]
    private static void Main() => Run<HierarchicalConstraintsDemo>();
  }
}
