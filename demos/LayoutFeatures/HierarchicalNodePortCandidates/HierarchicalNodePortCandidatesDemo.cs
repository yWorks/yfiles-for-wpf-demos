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

namespace yWorks.Demo.HierarchicalNodePortCandidates
{
  /// <summary>
  ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> with a configured set of <see cref="NodePortCandidates"/>.
  /// </summary>
  [Demo("layout-hierarchical-node-port-candidates", "layout-features", "Hierarchical Layout with Node Port Candidates",
      Summary = "Shows how to use node port candidates with hierarchical layout.", Tags = ["hierarchical", "ports"],
      Keywords = ["layout", "ports", "candidates", "nodeportcandidates", "keyword", "playground"])]
  public class HierarchicalNodePortCandidatesDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      // Run an initial layout with port candidates
      ApplyHierarchicalLayoutWithPortCandidates();

      // Add a button to toggle the port candidates on and off
      ToolBar.AddToggleButton("Use Node Port Candidates", pressed => {
        if (pressed) {
          ApplyHierarchicalLayoutWithPortCandidates();
        } else {
          Graph.ApplyLayout(new HierarchicalLayout());
          GraphControl.FitGraphBounds();
        }
      }, true);
    }

    private void ApplyHierarchicalLayoutWithPortCandidates() {
      // Add a LayoutPortCandidate for each side of the node. Allow only one connection to each candidate
      var nodePortCandidates = new NodePortCandidates();
      nodePortCandidates.AddFreeCandidate(PortSides.Top);
      nodePortCandidates.AddFreeCandidate(PortSides.Bottom);
      nodePortCandidates.AddFreeCandidate(PortSides.Right);
      nodePortCandidates.AddFreeCandidate(PortSides.Left);

      // Only use the candidate set for the terminal node
      var layout = new HierarchicalLayout();
      var layoutData = layout.CreateLayoutData(Graph);
      layoutData.Ports.NodePortCandidates.MapperFunction = node =>
          (node.Tag as NodeData)?.Terminal == true ? nodePortCandidates : null;

      Graph.ApplyLayout(layout, layoutData);
      GraphControl.FitGraphBounds();
    }

    private record NodeData(bool Terminal);

    [STAThread]
    private static void Main() => Run<HierarchicalNodePortCandidatesDemo>();
  }
}
