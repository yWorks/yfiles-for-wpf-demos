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

using System;
using System.Collections.Generic;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.ControlStyles
{
  /// <summary>
  /// A simple form that shows how to use the <see cref="NodeControlNodeStyle"/> and <see cref="LabelControlLabelStyle"/>
  /// to style a graph that displays business objects appropriately.
  /// </summary>
  public partial class ControlStylesDemo
  {
    /// <summary>
    /// A reference to the static, shared node style instance that is used for customers.
    /// </summary>
    private NodeControlNodeStyle customerNodeStyle;
    /// <summary>
    /// A reference to the static, shared node style instance that is used for products.
    /// </summary>
    private NodeControlNodeStyle productNodeStyle;
    /// <summary>
    /// A reference to the static, shared node style instance that is used for the labels of the edges.
    /// </summary>
    private LabelControlLabelStyle labelStyle;
    /// <summary>
    /// A reference to the static, shared edge style instance that is used for the edges.
    /// </summary>
    private EdgeSegmentControlEdgeStyle edgeStyle;
    /// <summary>
    /// A reference to the static, shared port style instance that is used for the ports.
    /// </summary>
    private PortControlPortStyle portStyle;

    public ControlStylesDemo() {
      InitializeComponent();
    }

    /// <summary>
    /// Run the layout after the size of the control has been determined.
    /// </summary>
    protected virtual void OnLayoutUpdated(object source, EventArgs args) {
      // avoid double registration
      LayoutUpdated -= OnLayoutUpdated;

      // disable all default adorners - the style visualize these states themselves
      graphControl.HighlightIndicatorManager.Enabled = false;
      graphControl.SelectionIndicatorManager.Enabled = false;
      graphControl.FocusIndicatorManager.Enabled = false;


      // Initialize input mode for navigation
      InitializeInputModes();
      // initialize default style instances.
      InitializeStyles();

      // load the sample model from GraphML
      graphControl.ImportFromGraphML("Resources\\model.graphml");

      // alternatively create the sample model in code
      // CreateModel();
    }


    /// <summary>
    /// Initializes the input modes.
    /// </summary>
    protected virtual void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode();
      graphEditorInputMode.NodeCreator = CreateNode;
      graphEditorInputMode.LabelEditableItems = GraphItemTypes.Edge | GraphItemTypes.EdgeLabel;
      graphEditorInputMode.CreateEdgeInputMode.EdgeCreator = CreateEdge;
      graphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    /// Callback used by <see cref="CreateEdgeInputMode"/> to create an edge.
    /// </summary>
    /// <remarks>
    /// This implementation adds a label to the newly created edge and sets up the correct business object.
    /// Note that only edges from Customers to Products are allowed.
    /// </remarks>
    private IEdge CreateEdge(IInputModeContext context, IGraph graph, IPortCandidate sourcePortCandidate, IPortCandidate targetPortCandidate, IEdge templateEdge)
    {
      var sourcePort = sourcePortCandidate.Port ?? sourcePortCandidate.CreatePort(context);
      var targetPort = targetPortCandidate.Port ?? targetPortCandidate.CreatePort(context);
      var customer = sourcePort.Owner.Tag as Customer;
      var product  = targetPort.Owner.Tag as Product;
      if (customer != null && product != null) {
        var edge = graph.CreateEdge(sourcePort, targetPort, edgeStyle);
        var relation = new Relation() {Customer = customer, Product = product};
        var label = graph.AddLabel(edge, relation.ToString());
        label.Tag = relation;
        return edge;
      } else {
        return null;
      }
    }

    /// <summary>
    /// Callback used by <see cref="GraphEditorInputMode"/> to actually create a node upon a click.
    /// </summary>
    /// <remarks>
    /// This method creates a dummy business object and associated it with a newly created node.
    /// </remarks>
    private INode CreateNode(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      Customer c = new Customer("Sample Customer", "Your Computer", new Random((int) DateTime.Now.TimeOfDay.TotalMilliseconds).Next(99999));
      var simpleNode = new SimpleNode { Tag = c, Style = customerNodeStyle, Layout = new MutableRectangle(0, 0, 10, 10) };
      var preferredSize = customerNodeStyle.GetPreferredSize(graphControl.CreateRenderContext(), simpleNode);
      return graph.CreateNode(RectD.FromCenter(location, preferredSize), customerNodeStyle, c);
    }

    /// <summary>
    /// Initializes the styles.
    /// </summary>
    protected virtual void InitializeStyles() {
      customerNodeStyle = new NodeControlNodeStyle("CustomerNodeTemplate");
      productNodeStyle = new NodeControlNodeStyle("ProductNodeTemplate");
      labelStyle = new LabelControlLabelStyle("LabelTemplate");
      edgeStyle = new EdgeSegmentControlEdgeStyle("EdgeSegmentTemplate") { SegmentThickness = 5, PathStyle = new PolylineEdgeStyle { Pen = new Pen(Brushes.Black, 5) } };
      portStyle = new PortControlPortStyle("PortTemplate") { RenderSize = new SizeD(5, 5) };

      // set an initial default size for all nodes
      Graph.NodeDefaults.Size = new SizeD(150, 80);

      // and set the customer node style as the default
      Graph.NodeDefaults.Style = customerNodeStyle;

      // the same for the edges labels....
      Graph.EdgeDefaults.Labels.Style = labelStyle;
      // edges....
      Graph.EdgeDefaults.Style = edgeStyle;

      // and the ports
      Graph.NodeDefaults.Ports.Style = portStyle;
    }

    /// <summary>
    /// Creates a simple model - this method is not used in this demo - instead the graph is loaded from 
    /// the included GraphML file.
    /// </summary>
    private void CreateModel() {

      // remember the mapping for each object to the created node
      var objectMapping = new Dictionary<object, INode>();

      // set the default node style..
      Graph.NodeDefaults.Style = customerNodeStyle;

      // and create some nodes - and associate them with a tag.

      var customers = new[]
                        {
                          new Customer("Lucy Osbourne", "Arizona", 13413),
                          new Customer("Stephanie Cornwell", "Oregon", 13414),
                          new Customer("Mark Wright", "Michigan", 13415),
                          new Customer("Ruby Turner", "South Carolina", 13416),
                          new Customer("Norman Johnson", "Montana", 13417)
                        };


      foreach (var c in customers) {
        // calculate the preferred size before creating the node
        var simpleNode = new SimpleNode { Tag = c, Style = customerNodeStyle, Layout = new MutableRectangle(0, 0, 10, 10) };
        var preferredSize = customerNodeStyle.GetPreferredSize(graphControl.CreateRenderContext(), simpleNode);

        // then create the node
        objectMapping[c] = Graph.CreateNode(new RectD(new PointD(), preferredSize), customerNodeStyle, c);
      }

      // create the products
      Graph.NodeDefaults.Style = productNodeStyle;

      // and the nodes....

      var products = new[]
                       {
                         new Product("Donut Maker", 8971, true), 
                         new Product("Snow Boots", 8972, true), 
                         new Product("Cowboy Hat", 8973, false),
                       };

      foreach (var p in products) {
        // calculate the preferred size before creating the node
        var simpleNode = new SimpleNode { Tag = p, Style = productNodeStyle, Layout = new MutableRectangle(0, 0, 10, 10) };
        var preferredSize = productNodeStyle.GetPreferredSize(graphControl.CreateRenderContext(), simpleNode);

        // then create the node
        objectMapping[p] = Graph.CreateNode(new RectD(new PointD(), preferredSize), productNodeStyle, p);
      }

      var relations = new[] {
        new Relation {Customer = customers[0], Product = products[0]},
        new Relation {Customer = customers[0], Product = products[1]},
        new Relation {Customer = customers[0], Product = products[2]},
        new Relation {Customer = customers[1], Product = products[0]},
        new Relation {Customer = customers[1], Product = products[2]},
        new Relation {Customer = customers[2], Product = products[1]},
        new Relation {Customer = customers[2], Product = products[2]},
        new Relation {Customer = customers[3], Product = products[1]},
        new Relation {Customer = customers[4], Product = products[2]},
      };

      // now add the edges using the stored mapping between products/customers and INodes
      foreach (var relation in relations) {
        var edge = Graph.CreateEdge(objectMapping[relation.Customer], objectMapping[relation.Product]);

        // and add a label
        var label = Graph.AddLabel(edge, relation.ToString());
        label.Tag = relation;
      }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    public IGraph Graph {
      get { return graphControl.Graph; }
    }
  }
}