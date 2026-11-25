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

namespace yWorks.Demo.TreeNodePlacers
{
  [Demo("layout-tree-node-placers", "layout-features", "Tree Layout with Subtree Placers",
      Summary = "Shows how to use different subtree placers in tree layout.",
      Tags = ["tree", "subtreeplacer", "nodeplacer"], Keywords = ["layout", "nodes", "playground"])]
  public class TreeNodePlacersDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Use layout data object to specify item-individual settings
      var treeLayout = new TreeLayout();
      var treeLayoutData = treeLayout.CreateLayoutData();
      // Assign a subtree placer depending on the number stored in the node's tag
      treeLayoutData.SubtreePlacers.MapperFunction = node => {
        if (node.Tag is not NodeData data) {
          return null;
        }
        return data.Id switch {
            1 =>
                // Place child nodes with the same depth in the tree in the same horizontal layer
                new LevelAlignedSubtreePlacer(),
            2 =>
                // Place child nodes left and right of a single vertical bus
                new LeftRightSubtreePlacer(),
            6 =>
                // Places child nodes in two horizontal lines
                new DoubleLayerSubtreePlacer(),
            _ => null
        };
      };

      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Apply the configured tree layout
      Graph.ApplyLayout(treeLayout, treeLayoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(int Id);

    [STAThread]
    private static void Main() => Run<TreeNodePlacersDemo>();
  }
}
