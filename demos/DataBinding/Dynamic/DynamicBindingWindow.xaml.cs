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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout.Hierarchical;

namespace Demo.yFiles.DataBinding.Dynamic
{
  /// <summary>
  /// Shows how to use a <see cref="yWorks.Graph.DataBinding.TreeSource"/> bound to dynamic business data.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Note that this class does not change the graph maintained by the tree source directly.
  /// It just acts on the business data to which the tree source is bound. Since the
  /// <see cref="TreeSource"/> is configured to handle dynamic updates in
  /// the XAML file, changes to the business data are translated to changes of the graph.
  /// </para>
  /// <para>
  /// This class translates user gestures to updates of the business objects (see
  /// <see cref="CreateCustomer"/> and <see cref="RemoveCustomer"/>), which are delegated to the
  /// <see cref="CustomerRepository"/>. Moreover, it performs an animated layout once the graph
  /// provided by the tree source has changed (see the region "Event Handling").
  /// </para>
  /// </remarks>
  public partial class DynamicBindingWindow
  {
    #region Private Fields

    private int customerCounter = 1;

    #endregion

    public DynamicBindingWindow() {
      InitializeComponent();
      // add a mode that handles the user commands.
      var keyboardInputMode = graphViewerInputMode.KeyboardInputMode;
      keyboardInputMode.AddKeyBinding(Key.Insert, ModifierKeys.None, CreateCustomer);
      keyboardInputMode.AddKeyBinding(Key.Delete, ModifierKeys.None, RemoveCustomer);
    }

    #region Creating and Removing Customer Business Objects

    private async void CreateCustomer() {
      var selectedNode = graphControl.Selection.Nodes.FirstOrDefault();

      // Get parent customer.
      var parentCustomer = selectedNode == null ? null : TreeSource.GetBusinessObject(selectedNode) as Customer;

      // Create the new customer.
      CustomerRepository.CreateCustomer("Customer " + customerCounter++, parentCustomer);

      await DoLayout();
    }


    private async void RemoveCustomer() {
      var selectedNode = graphControl.Selection.Nodes.FirstOrDefault();
      if (selectedNode != null) {
        // Remove the customer which is represented by the selected node.
        if (TreeSource.GetBusinessObject(selectedNode) is Customer customer) {
          CustomerRepository.RemoveCustomer(customer);
          await DoLayout();
        }
      }
    }

    #endregion

    #region Event Handling

    private async void OnLoaded(object sender, RoutedEventArgs e) {

      // Perform an initial layout when the window is loaded.
      await DoLayout();
    }

    private async Task DoLayout() {
      await graphControl.ApplyLayoutAnimated(new HierarchicalLayout(), TimeSpan.FromMilliseconds(500));
    }

    #endregion

    #region Convenience Properties

    /// <summary>
    /// Gets the customer repository defined in the XAML file.
    /// </summary>
    public CustomerRepository CustomerRepository => (CustomerRepository) FindResource("customerRepository");

    /// <summary>
    /// Gets the tree source defined in the XAML file.
    /// </summary>
    public TreeSource TreeSource => (TreeSource) FindResource("treeSource");

    #endregion
  }
}
