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
using System.Threading.Tasks;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.HierarchicalIncremental
{
  /// <summary>
  ///   Demonstrates how to run a <see cref="HierarchicalLayout"/> on a predefined subset of nodes and edges.
  /// </summary>
  [Demo("layout-hierarchical-incremental", "layout-features", "Incremental Hierarchical Layout",
      Summary = "Shows how to run the hierarchical layout on a predefined set of nodes.",
      Tags = ["hierarchical", "incremental"],
      Keywords = ["layout", "subset", "nodes", "set", "node-set", "from sketch", "playground"])]
  public class HierarchicalIncrementalDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Build the graph from the JSON data
      BuildGraphFromJson("sample.json", typeof(NodeData));

      GraphControl.FitGraphBounds();

      // Add a button to run the incremental layout
      ToolBar.AddButton("Run Incremental Layout", async () => await ApplyIncrementalHierarchicalLayout());
    }

    /// <summary>
    ///   Applies the hierarchical layout incrementally to the marked nodes.
    /// </summary>
    private async Task ApplyIncrementalHierarchicalLayout() {
      // Configure the layout data to only include nodes marked as 'includeInLayout' in the tag
      var hierarchicalLayout = new HierarchicalLayout();
      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(Graph);
      hierarchicalLayoutData.IncrementalNodes.Predicate = IsIncremental;

      // Create a hierarchical layout with fromSketchMode enabled
      hierarchicalLayout.FromSketchMode = true;

      await GraphControl.ApplyLayoutAnimated(hierarchicalLayout, TimeSpan.FromMilliseconds(500),
          hierarchicalLayoutData);
    }

    private static bool IsIncremental(INode node) => node.Tag is NodeData data && data.IncludeInLayout;

    private record NodeData(bool IncludeInLayout);

    [STAThread]
    private static void Main() => Run<HierarchicalIncrementalDemo>();
  }
}
