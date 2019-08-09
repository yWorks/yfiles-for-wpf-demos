/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Input.SingleSelection
{
  /// <summary>
  /// This demo shows how to configure <see cref="GraphEditorInputMode"/> to enable single selection mode
  /// for interaction. All default gestures that result in more than one item selected at a time are
  /// either switched off or changed so only one item gets selected. This requires some configuration that
  /// is done in <see cref="EnableSingleSelection"/>. This method also shows how to restore the default selection
  /// behavior.
  /// </summary>
  public partial class SingleSelectionWindow
  {
    // the previously set multiselection recognizer
    private EventRecognizer oldMultiSelectionRecognizer;
    //the previously set select pasted items
    private GraphItemTypes oldPasteItems;

    // dummy bindings for the extend selection commands
    private readonly CommandBinding extendSelectionLeftBinding = new EmptyCommandBinding(ComponentCommands.ExtendSelectionLeft);
    private readonly CommandBinding extendSelectionRightBinding = new EmptyCommandBinding(ComponentCommands.ExtendSelectionRight);
    private readonly CommandBinding extendSelectionUpBinding = new EmptyCommandBinding(ComponentCommands.ExtendSelectionUp);
    private readonly CommandBinding extendSelectionDownBinding = new EmptyCommandBinding(ComponentCommands.ExtendSelectionDown);
    // custom command binding for 'toggle item selection'
    private readonly CommandBinding customToggleSelectionBinding;

    #region Initialization

    public SingleSelectionWindow() {
      InitializeComponent();
      // initialize command binding
      customToggleSelectionBinding = new CommandBinding(GraphCommands.ToggleItemSelection,
                                                        ToggleItemSelectionExecuted, ToggleItemSelectionCanExecute);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      graphControl.Graph.NodeDefaults.Style = new ShinyPlateNodeStyle { Brush = Brushes.Orange };
      var graphEditorInputMode = new GraphEditorInputMode();
      graphControl.InputMode = graphEditorInputMode;
      oldPasteItems = graphEditorInputMode.PasteSelectableItems;

      EnableSingleSelection(true);

      LoadGraph();
      graphControl.FitGraphBounds();
    }

    #endregion

    private void EnableSingleSelection(bool enable) {
      GraphEditorInputMode mode = (GraphEditorInputMode) graphControl.InputMode;
      if (enable) {
        // remember old recognizer so we can restore it later
        oldMultiSelectionRecognizer = mode.MultiSelectionRecognizer;

        // disable marquee selection
        mode.MarqueeSelectionInputMode.Enabled = false;
        // disable multi selection with Ctrl-Click
        mode.MultiSelectionRecognizer = EventRecognizers.Never;

        // deactivate commands that can lead to multi selection
        mode.AvailableCommands.Remove(GraphCommands.ToggleItemSelection);
        mode.AvailableCommands.Remove(ApplicationCommands.SelectAll);

        mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionLeft);
        mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionUp);
        mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionDown);
        mode.NavigationInputMode.AvailableCommands.Remove(ComponentCommands.ExtendSelectionRight);

        // add dummy command bindings that do nothing in order to prevent WPF default behavior
        graphControl.CommandBindings.Add(extendSelectionLeftBinding);
        graphControl.CommandBindings.Add(extendSelectionRightBinding);
        graphControl.CommandBindings.Add(extendSelectionUpBinding);
        graphControl.CommandBindings.Add(extendSelectionDownBinding);

        // add custom binding for toggle item selection
        graphControl.CommandBindings.Add(customToggleSelectionBinding);
        
        //Disable selection of (possibly multiple) items
        oldPasteItems = mode.PasteSelectableItems;
        mode.PasteSelectableItems = GraphItemTypes.None;

        //Also clear the selection - even though the setup works when more than one item is selected, it looks a bit strange
        graphControl.Selection.Clear();
      } else {
        // restore old settings
        mode.MarqueeSelectionInputMode.Enabled = true;
        mode.MultiSelectionRecognizer = oldMultiSelectionRecognizer;
        mode.PasteSelectableItems = oldPasteItems;

        // re-activate commands
        mode.AvailableCommands.Add(GraphCommands.ToggleItemSelection);
        mode.AvailableCommands.Add(ApplicationCommands.SelectAll);

        mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionLeft);
        mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionUp);
        mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionDown);
        mode.NavigationInputMode.AvailableCommands.Add(ComponentCommands.ExtendSelectionRight);

        graphControl.CommandBindings.Remove(extendSelectionLeftBinding);
        graphControl.CommandBindings.Remove(extendSelectionRightBinding);
        graphControl.CommandBindings.Remove(extendSelectionUpBinding);
        graphControl.CommandBindings.Remove(extendSelectionDownBinding);

        // remove custom binding for toggle item selection
        graphControl.CommandBindings.Remove(customToggleSelectionBinding);
      }
    }

    #region custom toggle item selection behavior

    private void ToggleItemSelectionCanExecute(object sender, CanExecuteRoutedEventArgs e) {
      // if we have an item, the command can be executed
      var modelItem = (e.Parameter as IModelItem) ?? graphControl.CurrentItem;
      e.CanExecute = modelItem != null;
      e.Handled = true;
    }

    /// <summary>
    /// Custom command handler that allows toggling the selection state of an item 
    /// respecting the single selection policy.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleItemSelectionExecuted(object sender, ExecutedRoutedEventArgs e) {
      // get the item
      var modelItem = (e.Parameter as IModelItem) ?? graphControl.CurrentItem;
      var inputMode = (GraphEditorInputMode) graphControl.InputMode;

      // check if it allowed to be selected
      if (modelItem != null && graphControl.Graph.Contains(modelItem) && inputMode.SelectableItems.Is(modelItem)) {
        var isSelected = inputMode.GraphSelection.IsSelected(modelItem);
        if (isSelected) {
          // the item is selected and needs to be unselected - just clear the selection
          inputMode.GraphSelection.Clear();
        } else {
          // the items is unselected - unselect all other items and select the currentItem
          inputMode.GraphSelection.Clear();
          inputMode.SetSelected(modelItem, true);
        }
        e.Handled = true;
        return;
      } else {
        return;
      }
    }

    #endregion

    private void ToggleSingleSelection_Click(object sender, RoutedEventArgs e) {
      bool isChecked = ((ToggleButton) sender).IsChecked ?? false;
      EnableSingleSelection(isChecked);
    }

    private void LoadGraph() {
      graphControl.ImportFromGraphML("Resources/SingleSelection.graphml");
    }

    /// <summary>
    /// Dummy <see cref="CommandBinding"/> that never allows to execute the command
    /// </summary>
    private class EmptyCommandBinding : CommandBinding {

      public EmptyCommandBinding(ICommand command) : base(command, OnExecutedHandler, OnCanExecuteHandler) {
        this.Command = command;
      }

      private static void OnCanExecuteHandler(object sender, CanExecuteRoutedEventArgs args) {
        args.CanExecute = false;
        args.Handled = true;
      }

      private static void OnExecutedHandler(object sender, ExecutedRoutedEventArgs args) { 
        // do nothing 
      }
    }
  }
}
