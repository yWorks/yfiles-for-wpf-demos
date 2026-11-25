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

using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.RectangleNodeStyle
{
  internal static class SingleSelectionHelper
  {
    /// <summary>
    ///   Enables single selection behavior for the input mode of the given graph control.
    /// </summary>
    public static void EnableSingleSelection(GraphEditorInputMode mode, GraphControl graphControl) {
      // Disable marquee and lasso selection
      mode.MarqueeSelectionInputMode.Enabled = false;
      mode.LassoSelectionInputMode.Enabled = false;

      // Disable multi-selection with Ctrl-Click
      mode.MultiSelectionRecognizer = EventRecognizers.Never;

      // Deactivate commands that can lead to multi-selection
      mode.AvailableCommands.Remove(GraphCommands.ToggleItemSelection);
      mode.AvailableCommands.Remove(ApplicationCommands.SelectAll);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionLeft);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionUp);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionDown);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionRight);

      // Add a custom binding for item selection to maintain the single-selection policy
      mode.KeyboardInputMode.AddCommandBinding(GraphCommands.ToggleItemSelection, (_, evt) => {
        var item = evt.Parameter as IModelItem ?? graphControl.CurrentItem;
        if (item is not null && graphControl.Graph.Contains(item)) {
          if (graphControl.Selection.Contains(item)) {
            graphControl.Selection.Clear();
          } else {
            graphControl.Selection.Clear();
            graphControl.Selection.Add(item);
          }
        }
        evt.Handled = true;
      }, (_, evt) => {
        var item = evt.Parameter as IModelItem ?? graphControl.CurrentItem;
        evt.CanExecute = item is not null;
        evt.Handled = true;
      });
    }
  }
}
