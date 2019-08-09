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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using yWorks.Graph.DataBinding;

namespace Demo.yFiles.DataBinding.InteractiveEdgesGraphSource
{
  /// <summary>
  /// Interaction logic for AddConnectionDialog.xaml.
  /// </summary>
  public partial class AddConnectionDialog
  {
    private readonly List<EntityData> dataItems;
    private readonly CreationMode mode;

    public AddConnectionDialog(AdjacentEdgesGraphSource graphSource, CreationMode mode) {
      InitializeComponent();

      this.mode = mode;
      ApplyMode();

      dataItems = new List<EntityData>();
      // fill data items list with all the nodes available in the graph
      foreach (var node in graphSource.Graph.Nodes) {
        EntityData data = graphSource.GetBusinessObject(node) as EntityData;
        if (data != null) {
          dataItems.Add(data);
        }
      }
      targetCombobox.SelectionChanged += TargetComboboxOnSelectionChanged;
      targetCombobox.ItemsSource = dataItems;
      targetCombobox.SelectedIndex = 0;
    }

    private void TargetComboboxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs) {
      EntityData entityData = targetCombobox.SelectedItem as EntityData;
      if (entityData != null) {
        // put the events/methods of the selected data item into the combobox
        memberCombobox.ItemsSource = mode == CreationMode.EventRegistration ? entityData.Events : entityData.Methods;
        memberCombobox.SelectedIndex = 0;
      } else {
        memberCombobox.ItemsSource = null;
      }
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) {
      ConnectionName = (string) memberCombobox.SelectedItem;
      DataElement = targetCombobox.SelectedItem as EntityData;
      this.DialogResult = !string.IsNullOrWhiteSpace(ConnectionName) && DataElement != null;
      this.Close();
    }

    public string ConnectionName { get; private set; }

    public EntityData DataElement { get; private set; }

    /// <summary>
    /// Selects an item in the source/target combobox
    /// </summary>
    /// <param name="draggedData"></param>
    public void Preselect(EntityData draggedData) {
      targetCombobox.SelectedItem = draggedData;
    }

    private void ApplyMode() {
      switch(mode) {
        case CreationMode.EventRegistration:
          Title = "Add Event Registration";
          dataElementLabel.Content = "Source";
          memberLabel.Content = "Event";
          break;
        case CreationMode.MethodCall:
          Title = "Add Method Call";
          dataElementLabel.Content = "Target";
          memberLabel.Content = "Method";
          break;
      }
    }

    public enum CreationMode { EventRegistration, MethodCall }
  }
}
