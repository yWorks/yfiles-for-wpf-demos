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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using yWorks.Annotations;

namespace yWorks.Demo.DataBinding.Dynamic
{
  /// <summary>
  ///   This class is a simple business object. Each customer has a name and related customers.
  /// </summary>
  [ContentProperty("RelatedCustomers")]
  public class Customer
  {
    public string CustomerName { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<Customer> RelatedCustomers { get; } = [];
  }

  /// <summary>
  ///   This class contains root customers and provides methods to create and remove customers.
  /// </summary>
  [ContentProperty("Customers")]
  public class CustomerRepository
  {
    /// <summary>
    ///   Gets the root customers, which are not related to other customers.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<Customer> Customers { get; } = [];

    /// <summary>
    ///   Creates the customer and adds it to the root customers or to an existing parent's related customers.
    /// </summary>
    public Customer CreateCustomer([NotNull] string name, [CanBeNull] Customer? parent) {
      // Create a new customer business object.
      var newCustomer = new Customer { CustomerName = name };
      if (parent == null) {
        // No parent -> add new customer at the root level.
        Customers.Add(newCustomer);
      } else {
        // Add new customer as a related customer of an existing customer.
        parent.RelatedCustomers.Add(newCustomer);
      }
      return newCustomer;
    }

    /// <summary>
    ///   Removes a customer from the root customers or from the related customers reachable from a root customer.
    /// </summary>
    public bool RemoveCustomer([NotNull] Customer customer) {
      if (Customers.Contains(customer)) {
        // Remove customer at root level if possible.
        Customers.Remove(customer);
        return true;
      }
      // The customer is not a root customer. Try to remove it from the related
      // customers of one of the root customers.
      foreach (var rootCustomer in Customers) {
        if (RemoveRelatedCustomer(rootCustomer, customer)) {
          return true;
        }
      }
      return false;
    }

    private bool RemoveRelatedCustomer(Customer customer, Customer customerToBeRemoved) {
      if (customer.RelatedCustomers.Contains(customerToBeRemoved)) {
        customer.RelatedCustomers.Remove(customerToBeRemoved);
        return true;
      }
      foreach (var relatedCustomer in customer.RelatedCustomers) {
        if (RemoveRelatedCustomer(relatedCustomer, customerToBeRemoved)) {
          return true;
        }
      }
      return false;
    }
  }
}
