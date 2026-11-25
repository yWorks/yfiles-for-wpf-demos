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

using System.Collections.Generic;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;

namespace yWorks.Demo.NodeOverlapAvoiding
{
  /// <summary>
  ///   Enables single-selection behavior for the input mode and commands of a graph control.
  /// </summary>
  internal static class SingleSelectionHelper
  {
    private static readonly List<KeyboardInputModeBinding> commandBindings = [];

    /// <summary>
    ///   The previously set multi-selection recognizer.
    /// </summary>
    private static EventRecognizer? previousMultiSelectionRecognizer;

    /// <summary>
    ///   Enables single selection behavior for the input mode and commands of the given control.
    /// </summary>
    public static void EnableSingleSelection(GraphControl graphControl) {
      var mode = (GraphInputMode) graphControl.InputMode!;
      // Remember the previous recognizer so it can be restored later.
      previousMultiSelectionRecognizer = mode.MultiSelectionRecognizer;

      // Disable marquee selection.
      mode.MarqueeSelectionInputMode.Enabled = false;
      // Disable multi-selection with Ctrl-click.
      mode.MultiSelectionRecognizer = EventRecognizers.Never;

      // Deactivate commands that can lead to multiple selected items.
      mode.AvailableCommands.Remove(GraphCommands.ToggleItemSelection);
      mode.AvailableCommands.Remove(ApplicationCommands.SelectAll);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionLeft);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionUp);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionDown);
      mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionRight);

      // Add command bindings that do nothing to prevent the default behavior.
      commandBindings.Add(
          mode.KeyboardInputMode.AddCommandBinding(ComponentCommands.ExtendSelectionLeft, (_, _) => { }));
      commandBindings.Add(mode.KeyboardInputMode.AddCommandBinding(ComponentCommands.ExtendSelectionUp, (_, _) => { }));
      commandBindings.Add(
          mode.KeyboardInputMode.AddCommandBinding(ComponentCommands.ExtendSelectionDown, (_, _) => { }));
      commandBindings.Add(
          mode.KeyboardInputMode.AddCommandBinding(ComponentCommands.ExtendSelectionRight, (_, _) => { }));

      // Add a custom binding for toggling an item selection.
      commandBindings.Add(mode.KeyboardInputMode.AddCommandBinding(GraphCommands.ToggleItemSelection,
          (_, evt) => ToggleItemSelectionExecuted(graphControl, evt),
          (_, evt) => ToggleItemSelectionCanExecute(graphControl, evt)));

      // Clear the selection so the initial state is consistent.
      graphControl.Selection.Clear();
    }

    /// <summary>
    ///   Checks whether toggling the selection state of an item is allowed.
    /// </summary>
    private static void ToggleItemSelectionCanExecute(GraphControl graphControl, CanExecuteRoutedEventArgs evt) {
      var modelItem = evt.Parameter as IModelItem ?? graphControl.CurrentItem;
      evt.CanExecute = modelItem != null;
      evt.Handled = true;
    }

    /// <summary>
    ///   Handles toggle selection while respecting the single-selection policy.
    /// </summary>
    private static void ToggleItemSelectionExecuted(GraphControl graphControl, ExecutedRoutedEventArgs evt) {
      var modelItem = evt.Parameter as IModelItem ?? graphControl.CurrentItem;
      var inputMode = (GraphInputMode) graphControl.InputMode!;

      if (modelItem != null && graphControl.Graph.Contains(modelItem) && inputMode.SelectableItems.Is(modelItem)) {
        var selection = inputMode.GraphSelection;
        if (selection?.Contains(modelItem) == true) {
          // The item is selected, so clear the selection.
          selection.Clear();
        } else {
          // The item is unselected, so clear the other selection and select this item.
          selection?.Clear();
          inputMode.SetSelected(modelItem, true);
        }
      }
      evt.Handled = true;
    }

    /// <summary>
    ///   Restores normal multi-selection behavior for the input mode and commands.
    /// </summary>
    public static void DisableSingleSelection(GraphControl graphControl) {
      var mode = (GraphInputMode) graphControl.InputMode!;
      mode.MarqueeSelectionInputMode.Enabled = true;
      if (previousMultiSelectionRecognizer != null) {
        mode.MultiSelectionRecognizer = previousMultiSelectionRecognizer;
      }

      mode.AvailableCommands.Add(GraphCommands.ToggleItemSelection);
      mode.AvailableCommands.Add(ApplicationCommands.SelectAll);
      mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionLeft);
      mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionUp);
      mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionDown);
      mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionRight);

      foreach (var binding in commandBindings) {
        binding.Remove();
      }
      commandBindings.Clear();
    }
  }
}
