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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Folding
{
  /// <summary>
  ///   Shows how to enable collapsing and expanding of group nodes.
  /// </summary>
  [Demo("folding", "application-features", "Folding",
      Summary = "Shows how to enable collapsing and expanding of group nodes.", Tags = ["folding", "grouping"],
      Keywords = [
          "masterGraph", "wrappedGraph", "FoldingManager", "expand", "nesting", "hide", "collapse", "playground"
      ],
      RelatedDemos = [
          "folding-with-merged-edges", "folding-with-layout", "filtering-with-folding", "hierarchical-nesting",
          "hierarchical-nesting-incremental"
      ])]
  public sealed class FoldingDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Set up the default style for group nodes
      var defaultPalette = Themes.Palette12;
      var groupNodeStyle = DemoStyles.CreateDemoGroupStyle(defaultPalette);
      groupNodeStyle.GroupIcon = GroupNodeStyleIconType.ChevronDown;
      groupNodeStyle.FolderIcon = GroupNodeStyleIconType.ChevronUp;
      groupNodeStyle.IconBackgroundShape = GroupNodeStyleIconBackgroundShape.Circle;
      groupNodeStyle.IconBackgroundBrush = defaultPalette.NodeLabelFill;
      groupNodeStyle.IconForegroundBrush = defaultPalette.NodeLabelFill;
      Graph.GroupNodeDefaults.Style = groupNodeStyle;

      // Build demo graph from JSON data
      BuildGraphFromJson("graph-data.json");

      // Enable folding with the original graph as "masterGraph"
      var foldingManager = new FoldingManager(Graph);
      GraphControl.Graph = foldingManager.CreateFoldingView().Graph;

      // Do an initial layout
      GraphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      GraphControl.FitGraphBounds();

      // Enable graph editing
      var graphEditorInputMode = new GraphEditorInputMode();
      GraphControl.InputMode = graphEditorInputMode;

      // Toggle whether to fix the group node location when collapsing/expanding groups
      ToolBar.AddToggleButton("Automatic Group Node Alignment", pressed => {
        graphEditorInputMode.NavigationInputMode.AutoGroupNodeAlignmentPolicy = pressed
            ? NodeAlignmentPolicy.TopRight // Fix the top right location of a group node when toggling collapse/expand
            : NodeAlignmentPolicy.None;    // Do not fix any specific point
      });

      // Enable undo only after the initial graph was created and laid out
      // since we don't want to allow undoing that
      foldingManager.MasterGraph.SetUndoEngineEnabled(true);
    }

    [STAThread]
    private static void Main() => Run<FoldingDemo>();
  }
}
