/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.DataBinding.Dynamic
{
  /// <summary>
  /// This class is a simple example for a business object. Each customer has a
  /// <see cref="CustomerName">name</see> and a <see cref="RelatedCustomers">list of related customers</see>.
  /// </summary>
  [ContentProperty("RelatedCustomers")]
  public class Customer
  {
    private readonly ObservableCollection<Customer> relatedCustomers = new ObservableCollection<Customer>();

    public string CustomerName { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<Customer> RelatedCustomers {
      get { return relatedCustomers; }
    }
  }

  /// <summary>
  /// This class is a container for <see cref="Customer">customers</see>. It maintains a list of root
  /// <see cref="Customers"/> and provides methods to <see cref="CreateCustomer">create new customers</see>
  /// and to <see cref="RemoveCustomer">remove customers</see>.
  /// </summary>
  [ContentProperty("Customers")]
  public class CustomerRepository
  {
    private readonly ObservableCollection<Customer> customers = new ObservableCollection<Customer>();

    /// <summary>
    /// Gets the root customers, i.e. customers which are not related customers of other customers.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<Customer> Customers {
      get { return customers; }
    }

    /// <summary>
    /// Creates the customer and adds it to the root <see cref="Customers"/> or the related
    /// customers of an existing parent customer.
    /// </summary>
    /// <param name="name">The name of the new customer.</param>
    /// <param name="parent">The parent of the new customer.</param>
    /// <returns>The new customer.</returns>
    public Customer CreateCustomer([NotNull] string name, [CanBeNull] Customer parent) {
      // Create a new customer business object.
      var newCustomer = new Customer { CustomerName = name };
      if (parent == null) {
        // No parent -> add new customer at the root level.
        customers.Add(newCustomer);
      } else {
        // Add new customer as a related customer of an existing customer.
        parent.RelatedCustomers.Add(newCustomer);
      }
      return newCustomer;
    }

    /// <summary>
    /// Removes a customer from the root <see cref="Customers"/> or the related customers
    /// reachable from a root customer.
    /// </summary>
    /// <param name="customer">The customer.</param>
    /// <returns><see langword="true"/> if the customer was found and removed</returns>
    public bool RemoveCustomer([NotNull] Customer customer) {
      if (customers.Contains(customer)) {
        // Remove customer at root level if possible.
        customers.Remove(customer);
        return true;
      } else {
        // The customer is not a root customer. Try to remove it from the related
        // customers of one of the root customers.
        foreach (var rootCustomer in customers) {
          if(RemoveRelatedCustomer(rootCustomer, customer)) {
            return true;
          }
        }
        return false;
      }
    }

    private bool RemoveRelatedCustomer(Customer customer, Customer customerToBeRemoved) {
      if (customer.RelatedCustomers.Contains(customerToBeRemoved)) {
        customer.RelatedCustomers.Remove(customerToBeRemoved);
        return true;
      } else {
        foreach (var relatedCustomer in customer.RelatedCustomers) {
          if (RemoveRelatedCustomer(relatedCustomer, customerToBeRemoved)) {
            return true;
          }
        }
      }
      return false;
    }
  }
}
