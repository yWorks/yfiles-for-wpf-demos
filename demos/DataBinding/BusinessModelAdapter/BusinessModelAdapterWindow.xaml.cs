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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Organic;

namespace Demo.yFiles.Graph.BusinessModelAdapter
{
  /// <summary>
  ///   Shows how to write a simple adapter that adapts an existing observable
  ///   business model to an IGraph which can be visualized and edited in a view.
  /// </summary>
  public partial class BusinessModelAdapterWindow
  {
    // keeps the mappings
    private readonly Dictionary<Customer, INode> nodeMapping = new();

    // holds the style for the customer objects
    private readonly NodeControlNodeStyle customerNodeStyle;

    // holds the actual "business model"
    private readonly ICollection<Customer> customers;

    // one dummy customer for inserting new customers.
    private readonly Customer dummyCustomer;

    private readonly OrganicLayout layout;

    public BusinessModelAdapterWindow() {
      InitializeComponent();

      // create a sample list of customers
      // create the template Customer
      dummyCustomer = new Customer("Enter A Name", 0);

      customers = new ObservableCollection<Customer>();

      Resources.Add("DummyCustomer", dummyCustomer);

      var customer1 = new Customer("Mr. Tester", 42);
      var customer2 = new Customer("Mrs. Tester", 43);
      var customer3 = new Customer("Baby Tester", 2);
      var customer4 = new Customer("Aunt Tester", 34);

      customers.Add(customer1);
      customers.Add(customer2);
      customers.Add(customer3);
      customers.Add(customer4);

      // add some relationships between customers
      customer2.RelatedCustomers.Add(customer3);
      customer2.RelatedCustomers.Add(customer4);

      customer1.RelatedCustomers.Add(customer2);
      customer2.RelatedCustomers.Add(customer1);

      // publish the list so that the listview can pick it up
      Resources.Add("Customers", customers);

      // configure the GraphControl
      var editorInputMode = new GraphEditorInputMode();

      // configure some structural edits
      editorInputMode.CreateBendInputMode.Enabled = false;
      editorInputMode.ShowHandleItems = GraphItemTypes.Edge;
      editorInputMode.DeletableItems = GraphItemTypes.None;
      editorInputMode.CreateEdgeInputMode.Enabled = true;
      editorInputMode.CreateEdgeInputMode.EdgeCreator = EdgeCreator;

      // register custom code for the creation of new entities
      editorInputMode.NodeCreator = NodeCreator;

      // set the mode
      graphControl.InputMode = editorInputMode;

      // define the default edge style
      var style = new ArcEdgeStyle {
          Pen = new Pen(Brushes.Black, 2),
          Height = 30,
          TargetArrow = new Arrow(ArrowType.Stealth),
          ProvideHeightHandle = false
      };
      Graph.EdgeDefaults.Style = style;

      // fetch the node style from the xaml templates
      customerNodeStyle =
          new NodeControlNodeStyle("CustomerNodeStyle") { OutlineShape = new Rectangle { RadiusX = 3, RadiusY = 3 } };

      // now create the graph from the bindinglist
      CreateGraph(customers);

      // register for change notification
      ((INotifyCollectionChanged) customers).CollectionChanged += customers_CollectionChanged;

      layout = CreateLayout();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
      DoLayout();
    }

    public static readonly RoutedUICommand RemoveCustomerCommand =
        new("Remove Customer", "RemoveCustomer", typeof(BusinessModelAdapterWindow));

    public static readonly RoutedUICommand RemoveRelationCommand =
        new("Remove Relation", "RemoveRelation", typeof(BusinessModelAdapterWindow));

    /// <summary>
    ///   custom creation code that uses the model to actually trigger node creation
    /// </summary>
    private INode NodeCreator(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      // create the customer in the model
      var customer = new Customer("New Customer", 42);
      customers.Add(customer);
      // see if it has been added
      var newNode = nodeMapping[customer];
      if (newNode != null) {
        // adjust the location
        graph.SetNodeCenter(newNode, location);
      }
      return newNode;
    }

    private IEdge EdgeCreator(
        IInputModeContext context,
        IGraph graph,
        IPortCandidate sourcePort,
        IPortCandidate targetPort,
        IEdge templateEdge) {
      // We perform the edge creation indirectly through the model
      var sourceCustomer = (Customer) sourcePort.Owner.Tag;
      var targetCustomer = (Customer) targetPort.Owner.Tag;
      var relatedCustomers = sourceCustomer.RelatedCustomers;
      if (sourceCustomer != targetCustomer && !sourceCustomer.RelatedCustomers.Contains(targetCustomer)) {
        relatedCustomers.Add(targetCustomer);
        var edge = Graph.GetEdge(sourcePort.Owner, targetPort.Owner);
        // Return the newly created edge...
        return edge;
      }
      return null;
    }

    /// <summary>
    ///   callback for the insert button
    /// </summary>
    private void OnInsertClicked(object src, EventArgs args) {
      // create and add a new customer to the collection, this will trigger an update
      // in the graph
      customers.Add(new Customer(dummyCustomer.Name, dummyCustomer.Age));
    }

    /// <summary>
    ///   callback for the relate button
    /// </summary>
    private void OnRelateClicked(object src, EventArgs args) {
      // determine relationships to be established
      var firstCustomer = customerList.SelectedItem as Customer;
      if (firstCustomer != null) {
        var relationsToAdd = new List<Customer>(customerList.SelectedItems.Count);
        foreach (Customer relatedCustomer in customerList.SelectedItems) {
          if (relatedCustomer != firstCustomer && !firstCustomer.RelatedCustomers.Contains(relatedCustomer)) {
            relationsToAdd.Add(relatedCustomer);
          }
        }
        // create the relations - this will trigger edge creations in the graph
        foreach (var relatedCustomer in relationsToAdd) {
          firstCustomer.RelatedCustomers.Add(relatedCustomer);
        }
      }
    }

    /// <summary>
    ///   callback for the remove button
    /// </summary>
    private void OnRemoveClicked(object src, EventArgs args) {
      // works on the model - triggers update in the graph
      IList selectedItems = new ArrayList(customerList.SelectedItems);
      foreach (Customer item in selectedItems) {
        customers.Remove(item);
      }
    }

    /// <summary>
    ///   callback for removal of relationship
    /// </summary>
    private void OnRemoveRelatedClicked(object src, EventArgs args) {
      // see if something is being displayed
      var list = relatedCustomersList.ItemsSource as IList<Customer>;
      IList selectedRelations = new ArrayList(relatedCustomersList.SelectedItems);
      if (list != null && relatedCustomersList.SelectedItem is Customer) {
        foreach (Customer selectedRelation in selectedRelations) {
          list.Remove(selectedRelation);
        }
      }
    }

    /// <summary>
    ///   Called when the arrange button is clicked.
    ///   Performs a simple organic layout.
    /// </summary>
    private void OnArrangeClicked(object src, EventArgs e) {
      DoLayout();
    }

    private void DoLayout() {
      Graph.ApplyLayout(layout);
      graphControl.FitGraphBounds();
    }

    private OrganicLayout CreateLayout() {
      var organicLayout = new OrganicLayout {
          DefaultPreferredEdgeLength = 100, DefaultMinimumNodeDistance = 30, Deterministic = true
      };
      return organicLayout;
    }

    /// <summary>
    ///   callback that synchronized changes to the customer model
    /// </summary>
    private void customers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          foreach (Customer customer in e.NewItems) {
            InsertCustomer(customer);
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (Customer customer in e.OldItems) {
            RemoveCustomer(customer);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (Customer c in e.OldItems) {
            RemoveCustomer(c);
          }
          foreach (Customer customer in e.NewItems) {
            InsertCustomer(customer);
          }
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Reset:
          customers.Clear();
          Graph.Clear();
          CreateGraph(customers);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }


    /// <summary>
    ///   helper method to create the graph
    /// </summary>
    private void CreateGraph(IEnumerable<Customer> customers) {
      // for all customers
      foreach (var customer in customers) {
        InsertCustomer(customer);
      }
    }

    /// <summary>
    ///   Inserts a new customer.
    /// </summary>
    /// <param name="customer">The customer.</param>
    private void InsertCustomer(Customer customer) {
      // create or get a previously created node
      var sourceNode = GetOrCreateCustomerNode(customer);

      // for each related customer
      foreach (var relatedCustomer in customer.RelatedCustomers) {
        InsertRelation(sourceNode, relatedCustomer);
      }

      if (customer.RelatedCustomers is INotifyCollectionChanged) {
        var ncc = (INotifyCollectionChanged) customer.RelatedCustomers;
        ncc.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e) {
          RelationshipChanged(customer, e);
        };
      }
    }

    /// <summary>
    ///   Helper method that creates nodes and stores the mapping
    /// </summary>
    private INode GetOrCreateCustomerNode(Customer customer) {
      // see if we already created the node
      INode node = null;
      nodeMapping.TryGetValue(customer, out node);
      if (node == null) {
        // create the node at a default location using a default size
        node = Graph.CreateNode(new RectD(10, 40 * Graph.Nodes.Count, 100, 50), customerNodeStyle, customer);

        // now update the size to the desired size
        var preferredSize = customerNodeStyle.GetPreferredSize(graphControl.CreateRenderContext(), node);
        Graph.SetNodeLayout(node, new RectD(node.Layout.GetTopLeft(), preferredSize));

        // update the mapping
        nodeMapping[customer] = node;
      }
      return node;
    }

    /// <summary>
    ///   Helper method that inserts a new relation.
    /// </summary>
    /// <param name="sourceNode">The source node.</param>
    /// <param name="relatedCustomer">The related customer.</param>
    private void InsertRelation(INode sourceNode, Customer relatedCustomer) {
      // create or get the related customer node
      var targetNode = GetOrCreateCustomerNode(relatedCustomer);
      // create an edge
      Graph.CreateEdge(sourceNode, targetNode);
    }

    /// <summary>
    ///   Helper method that removes a customer.
    /// </summary>
    /// <param name="customer">The customer.</param>
    private void RemoveCustomer(Customer customer) {
      var node = nodeMapping[customer];
      if (node != null) {
        if (Graph.Contains(node)) {
          Graph.Remove(node);
        }
        nodeMapping.Remove(customer);
      }
    }

    /// <summary>
    ///   Helper method that removes relations from a customer.
    /// </summary>
    private void RemoveRelations(INode node, Customer customer) {
      var targetNode = nodeMapping[customer];
      if (targetNode != null) {
        var toDelete = new List<IEdge>();
        foreach (var edge in Graph.EdgesAt(node)) {
          if (edge.SourcePort.Owner == node && edge.TargetPort.Owner == targetNode) {
            toDelete.Add(edge);
          }
        }
        foreach (var edge in toDelete) {
          Graph.Remove(edge);
        }
      }
    }

    /// <summary>
    ///   callback that synchronizes changes in the relationships models.
    /// </summary>
    private void RelationshipChanged(Customer customer, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add: {
          var sourceNode = GetOrCreateCustomerNode(customer);
          foreach (Customer relatedCustomer in e.NewItems) {
            InsertRelation(sourceNode, relatedCustomer);
          }
        }
          break;
        case NotifyCollectionChangedAction.Remove: {
          var sourceNode = nodeMapping[customer];
          if (sourceNode != null) {
            foreach (Customer relatedCustomer in e.OldItems) {
              RemoveRelations(sourceNode, relatedCustomer);
            }
          }
        }
          break;
        case NotifyCollectionChangedAction.Replace: {
          var sourceNode = GetOrCreateCustomerNode(customer);
          foreach (Customer relatedCustomer in e.NewItems) {
            InsertRelation(sourceNode, relatedCustomer);
          }
          foreach (Customer relatedCustomer in e.OldItems) {
            RemoveRelations(sourceNode, relatedCustomer);
          }
        }
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Reset: {
          var sourceNode = nodeMapping[customer];
          if (sourceNode != null) {
            foreach (var edge in new List<IEdge>(Graph.EdgesAt(sourceNode))) {
              if (Graph.Contains(edge)) {
                Graph.Remove(edge);
              }
            }
            foreach (var relatedCustomer in customer.RelatedCustomers) {
              InsertRelation(sourceNode, relatedCustomer);
            }
          }
        }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private IGraph Graph => graphControl.Graph;
  }
}
