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
using System.Windows;
using System.Windows.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ContextMenu
{
  [Demo("context-menu", "input", "Context Menu",
      Summary = "Shows how to add a context menu to the nodes of a graph and to the canvas background.",
      Keywords = ["context menu", "copy", "playground"], Tags = ["interaction"])]
  public class ContextMenuDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Create a default editor input mode
      var inputMode = new GraphEditorInputMode { ContextMenuItems = GraphItemTypes.Node };
      GraphControl.InputMode = inputMode;

      // Add event listeners that populate the context menu according to the hit elements, or cancel showing a menu.
      inputMode.PopulateItemContextMenu += (_, args) => {
        if (args.Handled) {
          return;
        }

        // Create the context menu items
        if (GraphControl.Selection.Nodes.Count > 0) {
          args.Menu.Items.Add(CreateMenuItem("Cut", "✂", inputMode.Cut));
          args.Menu.Items.Add(CreateMenuItem("Copy", "▢", inputMode.Copy));
          args.Menu.Items.Add(CreateMenuItem("Delete", "×", inputMode.DeleteSelection));
        } else {
          // No node has been hit
          args.Menu.Items.Add(CreateMenuItem("Select all", null, inputMode.SelectAll));
          var pasteItem = CreateMenuItem("Paste", "▣", () => inputMode.PasteAtLocation(args.QueryLocation));
          pasteItem.IsEnabled = GraphControl.Clipboard.ClipboardGraph.Nodes.Count > 0 ||
              GraphControl.Clipboard.ClipboardGraph.Edges.Count > 0;
          args.Menu.Items.Add(pasteItem);
        }

        args.ShowMenu = true;
      };

      Graph.NodeDefaults.Size = new SizeD(50, 50);
      BuildGraphFromJson("graph-data.json");

      Graph.ApplyLayout(new HierarchicalLayout());
      GraphControl.FitGraphBounds();
    }

    private static MenuItem CreateMenuItem(string label, string? icon, Action action) {
      var menuItem = new MenuItem { Header = label };
      if (icon != null) {
        menuItem.Icon = new TextBlock {
            Text = icon,
            Width = 18,
            TextAlignment = TextAlignment.Center
        };
      }
      menuItem.Click += (_, _) => action();
      return menuItem;
    }

    [STAThread]
    private static void Main() => Run<ContextMenuDemo>();
  }
}
