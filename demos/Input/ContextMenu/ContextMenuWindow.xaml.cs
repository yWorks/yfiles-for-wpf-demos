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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Demo.yFiles.Toolkit;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.Input.ContextMenu
{
  /// <summary>
  /// Shows how to implement a dynamic context menu for the nodes and for the
  /// background of a <see cref="GraphControl"/>.
  /// </summary>
  public partial class ContextMenuWindow
  {
    /// <summary>
    /// Registers the callback method that populates the dynamic context menu, 
    /// when needed.
    /// </summary>
    /// <remarks>
    /// Note that this is the only place that interfaces with the Context Menu
    /// API. The remainder of the implementation shows how this can be
    /// customized, but for simple scenarios, all that needs to be done is shown
    /// in this method.
    /// </remarks>
    private void RegisterContextMenuCallback() {
      graphEditorInputMode.ContextMenuItems = GraphItemTypes.Node | GraphItemTypes.Edge;

      // Simple implementations with static context menus could just assign 
      // a static ContextMenu instance here.
      // Instead, use a dynamic context menu:
      graphEditorInputMode.PopulateItemContextMenu += OnPopulateItemContextMenu;
    }

    /// <summary>
    /// Fills the context menu with menu items based on the clicked node.
    /// </summary>
    private void OnPopulateItemContextMenu(object sender, PopulateItemContextMenuEventArgs<IModelItem> e) {
      // first update the selection
      INode node = e.Item as INode;
      // if the cursor is over a node select it, else clear selection
      UpdateSelection(node);
        
      // Create the context menu items
      if (graphControl.Selection.SelectedNodes.Count > 0) {
        // at least one node is selected
        var cutItem = new MenuItem { Command = ApplicationCommands.Cut};
        e.Menu.Items.Add(cutItem);
        var copyItem = new MenuItem { Command = ApplicationCommands.Copy };
        e.Menu.Items.Add(copyItem);
        var deleteItem = new MenuItem { Command = ApplicationCommands.Delete };
        e.Menu.Items.Add(deleteItem);
      } else {
        // no node has been hit
        var selectAllItem = new MenuItem { Header = "Select all"};
        selectAllItem.Click += (o, args) => { graphEditorInputMode.SelectAll(); };
        e.Menu.Items.Add(selectAllItem);
        var pasteItem = new MenuItem { Command = ApplicationCommands.Paste, CommandParameter = e.QueryLocation };
        e.Menu.Items.Add(pasteItem);
      }
      // open the menu
      e.ShowMenu = true;
      // mark the event as handled
      e.Handled = true;
    }

    /// <summary>
    /// Updates the node selection state when the context menu is opened on a node.
    /// </summary>
    /// <param name="node">The node or <see langword="null"/>.</param>
    private void UpdateSelection(INode node) {
      // see if no node was hit
      if (node == null) {
        // clear the whole selection
        graphControl.Selection.Clear();
      } else {
        // see if the node was selected, already
        if (!graphControl.Selection.SelectedNodes.IsSelected(node)) {
          // no - clear the remaining selection
          graphControl.Selection.Clear();
          // select the node
          graphControl.Selection.SelectedNodes.SetSelected(node, true);
          // also update the current item
          graphControl.CurrentItem = node;
        }
      }
    }

    #region Initialization

    public ContextMenuWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      RegisterContextMenuCallback();

      // Set a nicer node style and create the sample graph
      DemoStyles.InitDemoStyles(graphControl.Graph);
      CreateSampleGraph(graphControl.Graph);
    }

    #endregion

    #region Sample Graph Creation

    private void CreateSampleGraph(IGraph graph) {
      graph.AddLabel(graph.CreateNode(new PointD(100, 100)), "1");
      graph.AddLabel(graph.CreateNode(new PointD(200, 100)), "2");
      graph.AddLabel(graph.CreateNode(new PointD(300, 100)), "3");
    }

    #endregion
  }
}
