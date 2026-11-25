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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.FoldingWithLayout
{
  /// <summary>
  ///   A demo that demonstrates how to automatically trigger a layout that clears or fills the space
  ///   when opening or closing groups.
  /// </summary>
  [Demo("folding-with-layout", "layout", "Folding With Layout",
      Summary =
          "Shows how an automatic layout makes space for opening groups and reclaims the space of closing groups.",
      Tags = ["layout", "hierarchical", "grouping"],
      Keywords = ["folding", "hide", "create", "space", "clear area layout", "fill area layout"], RelatedDemos = [])]
  public sealed partial class FoldingWithLayoutDemo
  {
    /// <summary>
    ///   Initializes input modes, folding, and the sample graph.
    /// </summary>
    protected override void Initialize() {
      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      // enable folding
      var foldingManager = new FoldingManager();
      var foldingView = foldingManager.CreateFoldingView();
      graphControl.Graph = foldingView.Graph;

      var navigationInputMode = inputMode.NavigationInputMode;
      new ExpandCollapseNavigationHelper(navigationInputMode, foldingView);

      // Assign the default demo styles
      DemoStyles.InitDemoStyles(foldingManager.MasterGraph, foldingEnabled: true);

      // managing the appearance of folder nodes
      foldingManager.FolderNodeConverter = new FolderNodeConverter {
          FolderNodeDefaults = {
              CopyLabels = true,
              Size = new SizeD(110, 60)
          }
      };

      // read sample graph
      BuildGraph(foldingManager.MasterGraph, foldingView);

      // applying hierarchical layout with recursive edges
      var hierarchicalLayout = new HierarchicalLayout {
          FromSketchMode = true,
          DefaultEdgeDescriptor = { RecursiveEdgePolicy = RecursiveEdgePolicy.Directed }
      };

      // apply a layout and move it to the top of the graph component
      foldingView.Graph.ApplyLayout(hierarchicalLayout);
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Builds the graph using the JSON data.
    /// </summary>
    /// <param name="masterGraph">The master graph to populate.</param>
    /// <param name="foldingView">The folding view used to collapse the groups.</param>
    private void BuildGraph(IGraph masterGraph, IFoldingView foldingView) {
      // Builds the master graph from the JSON data
      BuildGraphFromJson("graph-data.json", masterGraph, typeof(NodeData), edgeTagType: typeof(EdgeData));

      // apply port locations and bends stored in the edge tags
      var viewGraph = foldingView.Graph;
      foreach (var edge in viewGraph.Edges) {
        if (edge.Tag is not EdgeData edgeData) {
          continue;
        }
        if (edgeData.Tag is { } edgeTag) {
          if (edgeTag.SourcePort != null) {
            viewGraph.SetPortLocation(edge.SourcePort, edgeTag.SourcePort.ToPointD());
          }
          if (edgeTag.TargetPort != null) {
            viewGraph.SetPortLocation(edge.TargetPort, edgeTag.TargetPort.ToPointD());
          }
          if (edgeTag.Bends != null) {
            foreach (var bendData in edgeTag.Bends) {
              viewGraph.AddBend(edge, bendData.ToPointD());
            }
          }
        } else {
          if (edgeData.SourcePort != null) {
            viewGraph.SetPortLocation(edge.SourcePort, edgeData.SourcePort.ToPointD());
          }
          if (edgeData.TargetPort != null) {
            viewGraph.SetPortLocation(edge.TargetPort, edgeData.TargetPort.ToPointD());
          }
        }
      }
      // collapsing the groups whose tags are collapsed
      masterGraph.Nodes.Where(node => masterGraph.IsGroupNode(node) && node.Tag is NodeData { Collapsed: true })
          .ToList().ForEach(foldingView.Collapse);
    }

    private record NodeData(bool Collapsed);

    private record EdgeData(EdgeTagData Tag, PointData SourcePort, PointData TargetPort);

    private record EdgeTagData(PointData? SourcePort, PointData? TargetPort, PointData[]? Bends);

    private record PointData(double X, double Y)
    {
      public PointD ToPointD() => new(X, Y);
    }

    [STAThread]
    private static void Main(string[] args) => Run<FoldingWithLayoutDemo>();

    public FoldingWithLayoutDemo() {
      InitializeComponent();
    }
  }
}
