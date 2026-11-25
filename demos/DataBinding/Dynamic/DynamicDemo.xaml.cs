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
using System.Threading.Tasks;
using System.Windows.Input;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.DataBinding.Dynamic
{
  /// <summary>
  ///   Shows how to use a <see cref="TreeSource"/> bound to dynamic business data.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Note that this class does not change the graph maintained by the tree source directly.
  ///     It just acts on the business data to which the tree source is bound. Since the
  ///     <see cref="TreeSource"/> is configured to handle dynamic updates in
  ///     the XAML file, changes to the business data are translated to changes of the graph.
  ///   </para>
  ///   <para>
  ///     This class translates user gestures to updates of the business objects (see
  ///     <see cref="CreateCustomer"/> and <see cref="RemoveCustomer"/>), which are delegated to the
  ///     <see cref="CustomerRepository"/>. Moreover, it performs an animated layout once the graph
  ///     provided by the tree source has changed (see the region "Event Handling").
  ///   </para>
  /// </remarks>
  [Demo("dynamic", "data-binding", "Binding to Dynamic Business Data",
      Summary = "Demonstrates data binding using a TreeSource bound to dynamic business data.",
      Tags = ["data binding", "dynamic data"], Keywords = ["TreeSource", "tree", "business data"], RelatedDemos = [])]
  public sealed partial class DynamicDemo : Demo
  {
    private int customerCounter = 1;

    public DynamicDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      // add a mode that handles the user commands.
      var keyboardInputMode = graphViewerInputMode.KeyboardInputMode;
      keyboardInputMode.AddKeyBinding(Key.Insert, ModifierKeys.None, CreateCustomer);
      keyboardInputMode.AddKeyBinding(Key.Delete, ModifierKeys.None, RemoveCustomer);

      await DoLayout();
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
      if (selectedNode != null && TreeSource.GetBusinessObject(selectedNode) is Customer customer) {
        // Remove the customer which is represented by the selected node.
        CustomerRepository.RemoveCustomer(customer);
        await DoLayout();
      }
    }

    private async Task DoLayout() {
      await graphControl.ApplyLayoutAnimated(new HierarchicalLayout(), TimeSpan.FromMilliseconds(500));
    }

    #endregion

    #region Convenience Properties

    /// <summary>
    ///   Gets the customer repository defined in the XAML file.
    /// </summary>
    public CustomerRepository CustomerRepository => (CustomerRepository) FindResource("customerRepository");

    /// <summary>
    ///   Gets the tree source defined in the XAML file.
    /// </summary>
    public TreeSource TreeSource => (TreeSource) FindResource("treeSource");

    #endregion

    [STAThread]
    public static void Main() => Run<DynamicDemo>();
  }
}
