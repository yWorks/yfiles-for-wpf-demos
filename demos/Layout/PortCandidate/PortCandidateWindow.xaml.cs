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

using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Graph.LabelModels;

[assembly : XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.2/demos/PortCandidateWindow", "Demo.yFiles.Layout.PortCandidateDemo")]
[assembly : XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.2/demos/PortCandidateWindow", "demo")]

namespace Demo.yFiles.Layout.PortCandidateDemo
{
  /// <summary>
  /// This demo shows how to use PortCandidateSets in conjunction with <see cref="HierarchicLayout"/>.
  /// </summary>
  public partial class PortCandidateWindow
  {
    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public PortCandidateWindow() {
      InitializeComponent();

      PopulateNodesList();
    }

    #region Graph creation and layout

    /// <summary>
    /// Perform the layout operation
    /// </summary>
    private async void ApplyLayout() {
      // layout starting, disable button
      layoutButton.IsEnabled = false;

      // create the layout algorithm
      var layout = new HierarchicLayout
      {
        OrthogonalRouting = true,
        LayoutOrientation = LayoutOrientation.TopToBottom
      };

      // do the layout
      await graphControl.MorphLayout(layout,
        TimeSpan.FromSeconds(1),
        new HierarchicLayoutData {
          //Automatically determine port constraints for source and target
          NodePortCandidateSets = {
            Delegate = node => {
              var candidateSet = new PortCandidateSet();
              // iterate the port descriptors
              var descriptors = PortDescriptor.CreatePortDescriptors(((FlowChartNodeStyle)node.Style).FlowChartType);
              foreach (var portDescriptor in descriptors) {
                PortCandidate candidate;
                // isn't a fixed port candidate (location is variable)
                if (portDescriptor.X == int.MaxValue) {
                  // create a port candidate at the specified side (east, west, north, south) and apply a cost to it
                  candidate = PortCandidate.CreateCandidate(portDescriptor.Side, portDescriptor.Cost);
                } else {
                  // create a candidate at a fixed location and side
                  var x = portDescriptor.X - node.Layout.Width / 2;
                  var y = portDescriptor.Y - node.Layout.Height / 2;
                  candidate = PortCandidate.CreateCandidate(x, y, portDescriptor.Side, portDescriptor.Cost);
                }
                candidateSet.Add(candidate, portDescriptor.Capacity);
              }
              return candidateSet;
            }
          },
          SourceGroupIds = { Delegate = edge => {
            // create bus-like edge groups for outgoing edges at Start nodes
            var sourceNode = edge.SourcePort.Owner as INode;
            if (sourceNode != null && ((((FlowChartNodeStyle)sourceNode.Style).FlowChartType)) == FlowChartType.Start) {
              return sourceNode;
            }
            return null;
          }},

          TargetGroupIds = { Delegate = edge => {
            // create bus-like edge groups for incoming edges at Branch nodes
            var targetNode = edge.TargetPort.Owner as INode;
            if (targetNode != null && (((FlowChartNodeStyle)targetNode.Style).FlowChartType) == FlowChartType.Branch) {
              return targetNode;
            }
            return null;
          }}
        });

      // enable button again
      layoutButton.IsEnabled = true;
    }

    #endregion


    #region Initialization

    /// <summary>
    /// The default style
    /// </summary>
    private readonly FlowChartNodeStyle defaultStyle = new FlowChartNodeStyle();

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected void OnLoaded(object source, RoutedEventArgs e) {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      IGraph graph = graphControl.Graph;
      // set the style as the default for all new nodes
      graph.NodeDefaults.Style = defaultStyle;

      // set the default style for all new node labels
      graph.NodeDefaults.Labels.Style = new DefaultLabelStyle {Typeface = new Typeface("Arial")};

      // set the default style for all new edge labels
      graph.EdgeDefaults.Style = new PolylineEdgeStyle
                                   {
                                     SourceArrow = new Arrow {Type = ArrowType.None},
                                     TargetArrow = new Arrow {Type = ArrowType.None},
                                     Pen = new Pen(Brushes.DarkSlateGray, 1)
                                   };

      // edges should be painted last - be at the highest layer
      graphControl.GraphModelManager.EdgeGroup.ToFront();

      // don't delete ports a removed edge was connected to
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      // newly created edges will always connect to the node's center
      graph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetFactory(
        PortCandidateProviders.FromNodeCenter);

      //Read initial graph from embedded resource
      graphControl.ImportFromGraphML("Resources\\defaultGraph.graphml");

      // make sure the graph fits
      graphControl.FitGraphBounds();

      // do the layout
      ApplyLayout();
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <remarks>
    /// The control uses a custom node creation callback that creates business objects for newly
    /// created nodes.
    /// </remarks>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      GraphEditorInputMode mode = new GraphEditorInputMode
                                    {
                                      // don't allow nodes to be created using a mouse click
                                      AllowCreateNode = false,
                                      // disable node resizing
                                      ShowHandleItems = GraphItemTypes.Bend | GraphItemTypes.Edge,
                                      // edge creation - uncomment to allow edges only to be created orthogonally
                                      // CreateEdgeInputMode = {OrthogonalEdgeCreation = true},
                                      // OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext(),
                                      // enable drag and drop
                                      NodeDropInputMode = {Enabled = true}
                                    };
      // wrap the original node creator so it removes the label from the dragged node
      var originalNodeCreator = mode.NodeDropInputMode.ItemCreator;
      mode.NodeDropInputMode.ItemCreator =
        (context, graph, draggedNode, dropTarget, layout) =>
        {
          return originalNodeCreator(context, graph, new SimpleNode { Style = draggedNode.Style, Layout = draggedNode.Layout }, dropTarget, layout);
        };
      return mode;
    }

    /// <summary>
    /// Fill the node list that acts as a source for nodes.
    /// </summary>
    private void PopulateNodesList() {
      // Create a new Graph in which the palette nodes live
      // Copy all relevant settings
      IGraph nodeContainer = new DefaultGraph();
      nodeContainer.NodeDefaults.Style = defaultStyle;

      // Create some nodes
      CreateNode(nodeContainer, PointD.Origin, FlowChartType.Start);
      CreateNode(nodeContainer, PointD.Origin, FlowChartType.Operation);
      CreateNode(nodeContainer, PointD.Origin, FlowChartType.Branch);
      CreateNode(nodeContainer, PointD.Origin, FlowChartType.End);

      // Add nodes to listview
      foreach (INode n in nodeContainer.Nodes) {
        var type = ((FlowChartNodeStyle) n.Style).FlowChartType;
        nodeContainer.AddLabel(n, type.ToString(), ExteriorLabelModel.South);
        styleListBox.Items.Add(n);
      }
    }

    /// <summary>
    /// Method that creates a node of the specified type. The method will specify the ports
    /// that the node should have based on its type.
    /// </summary>
    private void CreateNode(IGraph graph, PointD location, FlowChartType type) {
      var size = FlowChartNodeStyle.GetNodeTypeSize(type);
      RectD newBounds = new RectD(location, size);
      graph.CreateNode(newBounds, new FlowChartNodeStyle {FlowChartType = type});
    }

    #endregion

    #region Event handler implementation

    /// <summary>
    /// Exits the demo.
    /// </summary>
    private void ExitMenuItemClick(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    /// <summary>
    /// Formats the current graph.
    /// </summary>
    private void OnLayoutClick(object sender, EventArgs e) {
      ApplyLayout();
    }

    /// <summary>
    /// Initializes drag and drop behavior on the node list to the left
    /// </summary>
    private void OnTemplateMouseDown(object sender, RoutedEventArgs e) {
      // Initialize the information for a drag & drop from the node style list.
      FrameworkElement o = e.OriginalSource as FrameworkElement;
      if (o != null) {
        if (o.DataContext is INode) {
          var node = o.DataContext as INode;

          DataObject dao = new DataObject();
          dao.SetData(typeof (INode), node);
          DragDrop.DoDragDrop(o, dao, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
        }
      }
    }

    #endregion
  }

  #region Business logic

  /// <summary>
  /// Specifies the type of the node.
  /// </summary>
  public enum FlowChartType
  {
    Start,
    Operation,
    Branch,
    End
  }

  #endregion
}
