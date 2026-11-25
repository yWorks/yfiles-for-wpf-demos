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
using yWorks.Layout.Tree;

namespace yWorks.Demo.TreeLayout
{
  [Demo("layout-tree", "layout-features", "Basic Tree Layout",
      Summary = "Shows common configuration options for the tree layout.", Tags = ["tree"],
      Keywords = ["layout", "outedgecomparers", "portassignment", "subtreeplacer", "nodeplacer", "playground"])]
  public class TreeLayoutDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Initialize the tree layout algorithm
      var treeLayout = new Layout.Tree.TreeLayout {
          // Set the orientation to left-to-right (horizontal flow)
          LayoutOrientation = LayoutOrientation.LeftToRight,
          // Configure the single layer subtree placer
          DefaultSubtreePlacer = new SingleLayerSubtreePlacer {
              // Increase the distances between elements vertically and horizontally
              VerticalDistance = 30,
              HorizontalDistance = 30,
              // Increase the minimum length of the first and last edge segment
              MinimumFirstSegmentLength = 30,
              MinimumLastSegmentLength = 30,
              // Set alignment for nodes within a layer, useful for varying node sizes (1 = right-aligned)
              VerticalAlignment = 1,
              // Increase the spacing between orthogonally routed edge segments
              MinimumChannelSegmentDistance = 6
          },
          // Distribute the ports evenly on the side of nodes
          DefaultPortAssigner = new TreeLayoutPortAssigner(PortAssignmentMode.Distributed)
      };

      // Use layout data object to specify item-individual settings
      var treeLayoutData = treeLayout.CreateLayoutData(Graph);
      treeLayoutData.ChildOrder.OutEdgeComparisons.MapperFunction = _ => (edge1, edge2) => {
        // Sort out-edges by the label text (if any) of their target nodes
        var targetLabels1 = edge1.GetTargetNode().Labels;
        var targetLabels2 = edge2.GetTargetNode().Labels;
        if (targetLabels1.Count > 0 && targetLabels2.Count > 0) {
          return string.Compare(targetLabels1[0].Text, targetLabels2[0].Text, StringComparison.CurrentCulture);
        }
        return 0;
      };

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json");

      // Apply the configured tree layout
      Graph.ApplyLayout(treeLayout, treeLayoutData);
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    private static void Main() => Run<TreeLayoutDemo>();
  }
}
