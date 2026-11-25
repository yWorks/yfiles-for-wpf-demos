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
using System.Windows.Controls;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Orthogonal;

namespace yWorks.Demo.ClipboardDeferredCut
{
  /// <summary>
  ///   Demonstrates how to defer removing cut elements until they are pasted.
  /// </summary>
  [Demo("clipboard-deferred-cut", "view", "Deferred Cut Clipboard",
      Summary =
          "Shows a clipboard which grays elements out upon cut and only removes them when they are finally pasted.",
      Tags = ["interaction", "copy", "paste"], Keywords = ["labels"])]
  public sealed class DeferredCutClipboardDemo : SimpleDemo
  {
    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      // get the graph
      var graph = Graph;

      // set the styles and create a sample graph
      InitializeGraph(graph);

      // configure the clipboard itself
      var clipboard = new DeferredCutClipboard();
      // trigger a repaint after copy since copy removed the "marked for cut" mark from the elements
      clipboard.ItemsCopied += (_, _) => GraphControl.Invalidate();
      clipboard.ItemsCut += (_, _) => GraphControl.Invalidate();
      GraphControl.Clipboard = clipboard;
      ClipboardStyles.SetClipboardStyles(graph);

      // build the graph from the given data set
      BuildGraphFromJson("graph-data.json");

      // layout and center the graph
      graph.ApplyLayout(new OrthogonalLayout { GridSpacing = 30 });
      GraphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graph.SetUndoEngineEnabled(true);

      // set up the input mode
      var inputMode = new GraphEditorInputMode { MarqueeSelectableItems = GraphItemTypes.Node | GraphItemTypes.Bend };
      GraphControl.InputMode = inputMode;

      InitializeUI();

      // for demonstration purposes we configure a context menu
      // to make it possible to paste to an arbitrary location
      ConfigureContextMenu(inputMode);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set sizes and locations specific for this demo
      graph.NodeDefaults.Size = new SizeD(50, 50);

      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;
    }

    /// <summary>
    ///   Binds actions to the toolbar.
    /// </summary>
    private void InitializeUI() {
      var toolbar = ToolBar.Container;
      toolbar.Items.Add(CreateCommandButton(ApplicationCommands.Cut));
      toolbar.Items.Add(CreateCommandButton(ApplicationCommands.Copy));
      toolbar.Items.Add(CreateCommandButton(ApplicationCommands.Paste));
      ToolBar.AddSeparator();
      toolbar.Items.Add(CreateCommandButton(ApplicationCommands.Undo));
      toolbar.Items.Add(CreateCommandButton(ApplicationCommands.Redo));
    }

    private Button CreateCommandButton(ICommand command) =>
        new() {
            Command = command,
            CommandTarget = GraphControl
        };

    /// <summary>
    ///   Configures a context menu.
    ///   This is to provide the ability to paste graph elements to an arbitrary location.
    ///   Developers who want to know more about using context menus should look at the contextmenu demo in the input folder.
    /// </summary>
    private void ConfigureContextMenu(GraphEditorInputMode inputMode) {
      inputMode.ContextMenuItems = GraphItemTypes.All;
      inputMode.ContextMenuInputMode.PopulateMenu += (_, args) => {
        if (args.Handled) {
          return;
        }

        var item = inputMode.FindItems(args.QueryLocation, [GraphItemTypes.All]).FirstOrDefault();
        if (item is INode node) {
          if (!GraphControl.Selection.Nodes.Contains(node)) {
            GraphControl.Selection.Clear();
            GraphControl.Selection.Nodes.Add(node);
          }
        } else {
          GraphControl.Selection.Clear();
        }

        args.Menu.Items.Clear();
        if (GraphControl.Selection.Nodes.Count > 0) {
          args.Menu.Items.Add(CreateMenuItem("Cut", inputMode.Cut));
          args.Menu.Items.Add(CreateMenuItem("Copy", inputMode.Copy));
          args.Menu.Items.Add(CreateMenuItem("Delete", inputMode.DeleteSelection));
        } else {
          var queryLocation = args.QueryLocation;
          args.Menu.Items.Add(CreateMenuItem("Paste", () => inputMode.PasteAtLocation(queryLocation)));
        }

        args.ShowMenu = true;
        args.Handled = true;
      };
    }

    private static MenuItem CreateMenuItem(string label, Action action) {
      var menuItem = new MenuItem { Header = label };
      menuItem.Click += (_, _) => action();
      return menuItem;
    }

    [STAThread]
    private static void Main() => Run<DeferredCutClipboardDemo>();
  }
}
