/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Threading.Tasks;
using System.Windows;
using Demo.yFiles.Toolkit;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Tree;
using yWorks.Utils;

namespace Demo.yFiles.Layout.MixedLayout
{
  /// <summary>
  /// This demo shows how to use the recursive group layout to apply different layouts to the contents of group nodes.
  /// </summary>
  /// <remarks>The demo shows the following two use cases:
  /// <list type="bullet">
  /// <item>Table Sample: demonstrates how to realize a table node structure, i.e., each group node in the drawing represents a
  /// table and the nodes within the groups the table rows. Edges are connected to specific rows.
  /// The rows are sorted according to their y-coordinate in the initial drawing.
  /// </item>
  /// <item>
  /// Three-Tier Sample: demonstrates how to use the recursive group layout to realize different layouts of elements
  /// assigned to different tiers. Each group node can be assigned to the left, right or middle tier (depending on the
  /// group node label). All group nodes labeled "left" are placed on the left side. Their content is drawn using a
  /// <see cref="TreeLayout"/> with layout orientation left-to-right. Analogously, all group nodes labeled "right" are placed on the
  /// right side. Their content is drawn using a <see cref="TreeLayout"/> with layout orientation right-to-left. Elements not assigned
  /// to "left" or "right" group nodes are always lay out in the middle using the <see cref="HierarchicLayout"/> with layout
  /// orientation left-to-right. Note that group nodes not labeled "left" or "right" are handled non-recursive.
  /// </item>
  /// </list>
  /// </remarks>
  public partial class MixedLayoutDemo
  {
    private const string TableSample = "Table Sample";
    private const string ThreeTierSample = "Three-Tier Sample";

    public MixedLayoutDemo() {
      InitializeComponent();
    }

    private void DemoLoad(object sender, EventArgs e) {
      scenarioComboBox.Items.Add(TableSample);
      scenarioComboBox.Items.Add(ThreeTierSample);
      InitializeGraphDefaults();
      InitializeInputModes();

      scenarioComboBox.SelectedIndex = 0;
    }

    private void InitializeGraphDefaults() {
      FoldingManager fm = new FoldingManager();
      graphControl.Graph = fm.CreateFoldingView().Graph;
      var folderNodeConverter = fm.FolderNodeConverter as DefaultFolderNodeConverter;
      if (folderNodeConverter != null) {
        folderNodeConverter.CopyFirstLabel = true;
        folderNodeConverter.FolderNodeSize = new SizeD(80, 60);
      }

      IGraph graph = graphControl.Graph;

      graphControl.NavigationCommandsEnabled = true;

      //Create graph
      graph.NodeDefaults.Size = new SizeD(60, 30);

      DemoStyles.InitDemoStyles(graph);
    }

    private void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode
                                   {
                                     ShowHandleItems = GraphItemTypes.None,
                                     NavigationInputMode = {AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.TopLeft}
                                   };
      graphEditorInputMode.NavigationInputMode.GroupCollapsed += NavigationInputModeGroupStateToggled;
      graphEditorInputMode.NavigationInputMode.GroupExpanded += NavigationInputModeGroupStateToggled;
      graphControl.InputMode = graphEditorInputMode;
    }

    private async void NavigationInputModeGroupStateToggled(object source, ItemEventArgs<INode> evt) {
      // run a new layout after expanding/collapsing a group node
      await RunLayout();
    }

    private void ReadSampleGraph(string baseName) {
      graphControl.ImportFromGraphML("Resources\\" + baseName + ".graphml");
      //Adjust size and style from the first leaf in the loaded graph
      var firstLeaf = graphControl.Graph.GetGroupingSupport().GetDescendants(null).FirstOrDefault(node => !graphControl.Graph.IsGroupNode(node));
      if (firstLeaf != null) {
        graphControl.Graph.NodeDefaults.Size = firstLeaf.Layout.GetSize();
        graphControl.Graph.NodeDefaults.Style = firstLeaf.Style;
      }
    }

    private async void ScenarioComboBoxSelectedValueChanged(object sender, EventArgs e) {
      ReadSampleGraph((string) scenarioComboBox.SelectedItem);
      await RunLayout();
    }

    private async void OnRunButtonClicked(object sender, EventArgs e) {
      await RunLayout();
    }

    private async void OnRefreshButtonClicked(object sender, RoutedEventArgs e) {
      ReadSampleGraph((string) scenarioComboBox.SelectedItem);
      await RunLayout();
    }

    #region Layout configuration

    ///<summary>
    /// Runs either the table or the three tiers layout depending on the selected scenario.
    ///</summary>
    private async Task RunLayout() {
      object selectedLayout = scenarioComboBox.SelectedItem;
      DisableButtons();

      if ((string) selectedLayout == TableSample) {
        await RunTableLayout();
      } else if ((string) selectedLayout == ThreeTierSample) {
        await RunThreeTierLayout();
      }
    }

    private void DisableButtons() {
      scenarioComboBox.IsEnabled = false;
      useSketchCheckBox.IsEnabled = false;
      reloadButton.IsEnabled = false;
      runLayoutButton.IsEnabled = false;
    }

    private void EnableButtons() {
      scenarioComboBox.IsEnabled = true;
      useSketchCheckBox.IsEnabled = true;
      reloadButton.IsEnabled = true;
      runLayoutButton.IsEnabled = true;
    }

    ///<summary>
    /// Run a new table layout.
    ///</summary>
    private async Task RunTableLayout() {
      await RunLayout(new TableLayout(useSketchCheckBox.IsChecked.Value), TableLayout.LayoutData);
    }

    ///<summary>
    /// Run a new three tier layout.
    ///</summary>
    private async Task RunThreeTierLayout() {
      var threeTierLayout = new ThreeTierLayout(useSketchCheckBox.IsChecked.Value);
      var layoutData = new ThreeTierLayout.LayoutData(graphControl.Graph, useSketchCheckBox.IsChecked.Value);
      await RunLayout(threeTierLayout, layoutData);
    }

    private async Task RunLayout(ILayoutAlgorithm layout, LayoutData layoutData) {
      var executor = new LayoutExecutor(graphControl, layout) {
        LayoutData = layoutData,
        Duration = TimeSpan.FromMilliseconds(500),
        AnimateViewport = true
      };
      try {
        await executor.Start();
      } catch (Exception e) {
        MessageBox.Show(this, "Layout did not complete successfully.\n" + e.Message);
      }
      EnableButtons();
    }

    #endregion
  }
}
