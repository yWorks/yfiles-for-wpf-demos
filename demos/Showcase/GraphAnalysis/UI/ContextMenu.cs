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
using yWorks.Controls.Input;
using yWorks.Graph;
using static yWorks.Demo.GraphAnalysis.DemoTypes;

namespace yWorks.Demo.GraphAnalysis
{
  public sealed partial class GraphAnalysisDemo
  {
    /// <summary>
    ///   Initializes the context menu.
    /// </summary>
    private void InitializeContextMenu(GraphEditorInputMode inputMode) {
      inputMode.PopulateItemContextMenu += PopulateContextMenu;
    }

    /// <summary>
    ///   Populates the context menu based on the item the mouse hovers over.
    /// </summary>
    private void PopulateContextMenu(object? sender, PopulateItemContextMenuEventArgs<IModelItem> e) {
      if (e.Item is not INode node || GetCurrentAlgorithm() is not { } algorithm) {
        return;
      }

      void Add(string title, Action action) {
        var item = new MenuItem { Header = title };
        item.Click += (_, _) => action();
        e.Menu.Items.Add(item);
      }

      UpdateSelection(node);

      var selected = graphControl.Selection.Nodes.ToList();
      if (algorithm.NeedsStartNodes) {
        Add("Mark as Start Node", () => {
          using (Graph.BeginEdit("Mark as Start Node", "Mark as Start Node")) {
            // clear previous start nodes
            foreach (var n in Graph.Nodes) {
              if (GetTag(n)?.Type == NodeType.Start) {
                ResetType(n);
              }
            }

            foreach (var n in algorithm.NeedsEndNodes ? selected : [node]) {
              SetType(n, NodeType.Start);
            }

            ApplyAlgorithm();
          }
        });
      }

      if (algorithm.NeedsEndNodes) {
        Add("Mark as End Node", () => {
          using (Graph.BeginEdit("Mark as End Node", "Mark as End Node")) {
            // clear previous end nodes
            foreach (var n in Graph.Nodes) {
              if (GetTag(n)?.Type == NodeType.End) {
                ResetType(n);
              }
            }

            foreach (var n in selected) {
              SetType(n, NodeType.End);
            }

            ApplyAlgorithm();
          }
        });
      }

      // finally, if there is at least one menu item set them to the context menu
      if (e.Menu.Items.Count > 0) {
        e.ShowMenu = true;
        e.Handled = true;
      }
    }

    /// <summary>
    ///   Updates the selection when an item is right-clicked for a context menu.
    /// </summary>
    private void UpdateSelection(INode node) {
      // A right click on an unselected item replaces the previous selection.
      if (!graphControl.Selection.Nodes.Contains(node)) {
        graphControl.Selection.Clear();
        graphControl.Selection.Nodes.Add(node);
      }
    }
  }
}
