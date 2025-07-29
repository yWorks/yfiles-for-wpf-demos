/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Demo.yFiles.Toolkit;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Router;
using yWorks.Layout.Tree;
using RoutingStyle = yWorks.Layout.Router.RoutingStyle;

namespace Demo.yFiles.Layout.BasicLayout
{
  /// <summary>
  ///   This class shows how to use important layout and routing algorithms to calculate coordinates for a graph.
  ///   Featured layout and routing styles are Hierarchical, Organic, Orthogonal, Circular, Tree and Polyline Edge
  ///   Router.
  /// </summary>
  public partial class BasicLayoutDemo
  {
    public Dictionary<string, ILayoutAlgorithm> Layouts { get; } = new();

    public ILayoutAlgorithm CurrentLayout { get; set; }


    private async Task ApplyLayout() {
      // launch the layout in a separate thread and animate the result
      try {
        await graphControl.ApplyLayoutAnimated(CurrentLayout, TimeSpan.FromSeconds(1));
      } catch (Exception e) {
        MessageBox.Show(this, "Layout did not complete successfully.\n" + e.Message);
      }
    }

    public BasicLayoutDemo() {
      InitializeComponent();
    }

    private void SetupLayouts() {
      // using hierarchical layout style
      var hierarchicalLayout = new HierarchicalLayout();

      CurrentLayout = hierarchicalLayout;
      Layouts.Add("Hierarchical", hierarchicalLayout);

      // using organic layout style
      var organic = new OrganicLayout {
          QualityTimeRatio = 1.0,
          AllowNodeOverlaps = false,
          AvoidNodeEdgeOverlap = true,
          DefaultMinimumNodeDistance = 10,
          DefaultPreferredEdgeLength = 50
      };
      Layouts.Add("Organic", organic);

      // using orthogonal layout style
      var orthogonal = new OrthogonalLayout { GridSpacing = 15 };
      Layouts.Add("Orthogonal", orthogonal);

      // using circular layout style
      var circular = new CircularLayout();
      circular.BackboneLayout.MinimumEdgeLength = 50;
      circular.BackboneLayout.CompactnessFactor = 0.95;
      Layouts.Add("Circular", circular);

      // a tree layout algorithm
      var treeLayout = new TreeLayout {
          NodeLabelPlacement = NodeLabelPlacement.Consider,
          TreeReductionStage = { NonTreeEdgeRouter = new OrganicEdgeRouter() }
      };
      Layouts.Add("Tree", treeLayout);

      // using Polyline Router
      var polylineRouter = new EdgeRouter {
          GridSpacing = 10, DefaultEdgeDescriptor = { RoutingStyle = RoutingStyle.Orthogonal }, Rerouting = true
      };
      polylineRouter.DefaultEdgeDescriptor.EdgeRouterCosts.BendCost = 3;
      polylineRouter.DefaultEdgeDescriptor.EdgeRouterCosts.EdgeCrossingCost = 5;
      Layouts.Add("Polyline Edge Router", polylineRouter);
    }

    private void Demo_Load(object sender, EventArgs e) {
      SetupLayouts();
      layoutComboBox.ItemsSource = Layouts.Keys;
      InitializeGraph();
      InitializeInputModes();

      layoutComboBox.SelectedIndex = 0;
    }

    /// <summary>
    ///   Creates a small graph and various layouts to it.
    /// </summary>
    private void InitializeGraph() {
      var graph = graphControl.Graph;

      graphControl.FileOperationsEnabled = true;
      graphControl.NavigationCommandsEnabled = true;

      DemoStyles.InitDemoStyles(graph);

      ReadSampleGraph();

      // enable undo...
      graph.SetUndoEngineEnabled(true);
    }

    private void InitializeInputModes() {
      graphControl.InputMode = new GraphEditorInputMode();
    }

    private void ReadSampleGraph() {
      graphControl.ImportFromGraphML("Resources\\sample-flat.graphml");
      var i = 0;
      foreach (var node in graphControl.Graph.Nodes) {
        graphControl.Graph.AddLabel(node, (++i).ToString());
      }
    }

    private async void layoutComboBox_SelectedValueChanged(object sender, EventArgs e) {
      CurrentLayout = Layouts[(string) layoutComboBox.SelectedItem];
      await ApplyLayout();
    }

    private async void OnRunButtonClicked(object sender, EventArgs e) {
      await ApplyLayout();
    }
  }
}
