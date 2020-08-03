/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Router.Polyline;

[assembly : XmlnsDefinition("http://www.yworks.com/yfilesWPF/2.2/demos/LogicGateWindow", "Demo.yFiles.Layout.LogicGate")]
[assembly : XmlnsPrefix("http://www.yworks.com/yfilesWPF/2.2/demos/LogicGateWindow", "demo")]

namespace Demo.yFiles.Layout.LogicGate
{
  /// <summary>
  /// This demo shows how ports can be used in a read-world example by modeling wires, inputs, and outputs of digital logic elements.
  /// </summary>
  public partial class LogicGateWindow
  {
    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public LogicGateWindow() {
      InitializeComponent();

      InitializeLayout();

      PopulateNodesList();
    }

    #region Initialization

    /// <summary>
    /// The default style
    /// </summary>
    private readonly LogicGateNodeStyle defaultStyle = new LogicGateNodeStyle();

    /// <summary>
    /// The orthogonal edge router
    /// </summary>
    private ILayoutAlgorithm orthogonalEdgeRouter;

    private LayoutData oerData;

    private ILayoutAlgorithm hl;

    private LayoutData hlData;

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected async void OnLoaded(object source, RoutedEventArgs e) {
      // initialize the graph
      await InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual async Task InitializeGraph() {
      IGraph graph = graphControl.Graph;

      // set the default style for all new nodes
      graph.NodeDefaults.Style = defaultStyle;
      graph.NodeDefaults.Size = new SizeD(50, 30);

      // set the default style for all new node labels
      graph.NodeDefaults.Labels.Style = new DefaultLabelStyle {Typeface = new Typeface("Arial")};

      // set the default style for all new edge labels
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
                                     SourceArrow = new Arrow {Type = ArrowType.None},
                                     TargetArrow = new Arrow {Type = ArrowType.None},
                                     Pen = new Pen(Brushes.Black, 3) { EndLineCap = PenLineCap.Square, StartLineCap = PenLineCap.Square}
                                   };

      // disable edge cropping
      graph.GetDecorator().PortDecorator.EdgePathCropperDecorator.HideImplementation();

      // don't delete ports a removed edge was connected to
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      // set a custom port candidate provider
      graphControl.Graph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetImplementation(new DescriptorDependentPortCandidateProvider());

      // read initial graph from embedded resource
      graphControl.ImportFromGraphML("Resources\\defaultGraph.graphml");

      // do the layout
      await ApplyLayout(hl, hlData, true);
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
      var mode = new GraphEditorInputMode
               {
                 // don't allow nodes to be created using a mouse click
                 AllowCreateNode = false,
                 // don't allow bends to be created using a mouse drag on an edge
                 AllowCreateBend = false,
                 // disable node resizing
                 ShowHandleItems = GraphItemTypes.Bend | GraphItemTypes.Edge,
                 // enable orthogonal edge creation and editing
                 OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext(),
                 // enable drag and drop
                 NodeDropInputMode = { Enabled = true },
                 // disable moving labels
                 MoveLabelInputMode = { Enabled = false },
                 // enable snapping for easier orthogonal edge editing
                 SnapContext = new GraphSnapContext { Enabled = true },
                 CreateEdgeInputMode = {
                     // only allow starting an edge creation over a valid port candidate
                     StartOverCandidateOnly = true,
                     // show all port candidates when hovering over a node
                     ShowPortCandidates = ShowPortCandidates.All
                 }
               };
      // wrap the original node creator so it copies the ports and labels from the dragged node
      var originalNodeCreator = mode.NodeDropInputMode.ItemCreator;
      mode.NodeDropInputMode.ItemCreator =
        (context, graph, draggedNode, dropTarget, layout) => {
          if (draggedNode != null) {
            var newNode = originalNodeCreator(context, graph, new SimpleNode { Style = draggedNode.Style, Layout = draggedNode.Layout }, dropTarget, layout);
            // copy the ports
            foreach (var port in draggedNode.Ports) {
              var descriptor = (PortDescriptor) port.Tag;
              var portStyle = new NodeStylePortStyleAdapter(new ShapeNodeStyle {
                Brush = descriptor.EdgeDirection == EdgeDirection.In ? Brushes.Green : Brushes.DodgerBlue,
                Pen = null,
                Shape = ShapeNodeShape.Rectangle
              }) { RenderSize = new SizeD(5, 5) };
              var newPort = graph.AddPort(newNode, port.LocationParameter, portStyle, port.Tag);
              // create the port labels
              var parameter = new InsideOutsidePortLabelModel().CreateOutsideParameter();
              graph.AddLabel(newPort, descriptor.LabelText, parameter, tag: descriptor);
            }
            // copy the labels
            foreach (var label in draggedNode.Labels) {
              graph.AddLabel(newNode, label.Text, label.LayoutParameter, label.Style, tag: label.Tag);
            }
            return newNode;
          }
          // fallback
          return originalNodeCreator(context, graph, draggedNode, dropTarget, layout);
        };

      mode.CreateEdgeInputMode.EdgeCreated += (sender, args) => {
        var sourcePortLabel = args.SourcePort.Labels.FirstOrDefault();
        var targetPortLabel = args.TargetPort.Labels.FirstOrDefault();
        if (sourcePortLabel != null) {
          ReplaceLabelModel(args.SourcePort, sourcePortLabel);
        }
        if (targetPortLabel != null) {
          ReplaceLabelModel(args.TargetPort, targetPortLabel);
        }
      };

      graphControl.Graph.EdgeRemoved += (sender, args) => {
        var sourcePortLabel = args.SourcePort.Labels.FirstOrDefault();
        var targetPortLabel = args.TargetPort.Labels.FirstOrDefault();
        if (sourcePortLabel != null && !graphControl.Graph.EdgesAt(args.SourcePort).Any()) {
          graphControl.Graph.SetLabelLayoutParameter(sourcePortLabel,
            new InsideOutsidePortLabelModel().CreateOutsideParameter());
        }
        if (targetPortLabel != null && !graphControl.Graph.EdgesAt(args.TargetPort).Any()) {
          graphControl.Graph.SetLabelLayoutParameter(targetPortLabel,
            new InsideOutsidePortLabelModel().CreateOutsideParameter());
        }
      };

      return mode;
    }

    private void ReplaceLabelModel(IPort port, ILabel label) {
      var descriptor = (PortDescriptor) port.Tag;
      graphControl.Graph.SetLabelLayoutParameter(label, descriptor.LabelPlacementWithEdge);
    }

    /// <summary>
    /// Initializes the layout algorithm and its layout data.
    /// </summary>
    private void InitializeLayout() {
      orthogonalEdgeRouter = new EdgeRouter {
        ConsiderNodeLabels = true
      };

      hl = new HierarchicLayout {
        OrthogonalRouting = true,
        LayoutOrientation = LayoutOrientation.LeftToRight,
        ConsiderNodeLabels = true
      };

      // outgoing edges must be routed to the right of the node
      // we use the same value for all edges, which is a strong port constraint that forces
      // the edge to leave at the east (right) side
      var east = PortConstraint.Create(PortSide.East, true);
      // incoming edges must be routed to the left of the node
      // we use the same value for all edges, which is a strong port constraint that forces
        // the edge to enter at the west (left) side
      var west = PortConstraint.Create(PortSide.West, true);

      MapperDelegate<IEdge, PortConstraint> sourceDelegate = edge => ((PortDescriptor) edge.SourcePort.Tag).X == 0 ? west : east;
      MapperDelegate<IEdge, PortConstraint> targetDelegate = edge => ((PortDescriptor) edge.TargetPort.Tag).X == 0 ? west : east;
      oerData = new PolylineEdgeRouterData {
        SourcePortConstraints = { Delegate = sourceDelegate},
        TargetPortConstraints = { Delegate = targetDelegate }
      };

      hlData = new HierarchicLayoutData {
        SourcePortConstraints = { Delegate = sourceDelegate },
        TargetPortConstraints = { Delegate = targetDelegate }
      };
    }


    /// <summary>
    /// Fill the node list that acts as a source for nodes.
    /// </summary>
    private void PopulateNodesList() {
      // Create a new Graph in which the palette nodes live and copy all relevant settings
      IGraph nodeContainer = new DefaultGraph();
      nodeContainer.NodeDefaults.Style = defaultStyle;
      nodeContainer.NodeDefaults.Size = new SizeD(50, 30);

      // Create some nodes
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.And, "AND");
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.Nand, "NAND");
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.Or, "OR");
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.Nor, "NOR");
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.Not, "NOT");
      // Create an IC
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.Timer, "555", new SizeD(70, 120));
      CreateNode(nodeContainer, PointD.Origin, LogicGateType.ADConverter, "2-bit A/D\nConverter", new SizeD(70, 120));

      // Add nodes to listview
      foreach (var n in nodeContainer.Nodes) {
        styleListBox.Items.Add(n);
      }
    }

    /// <summary>
    /// Creates a node of the specified type.
    /// </summary>
    /// <remarks>
    /// The method will specify the ports that the node should have based on its type.
    /// </remarks>
    private void CreateNode(IGraph graph, PointD location, LogicGateType type, string label, SizeD? size = null) {
      RectD newBounds = RectD.FromCenter(location, graph.NodeDefaults.Size);
      INode node;
      if (type >= LogicGateType.Timer) {
        node = graph.CreateNode(RectD.FromCenter(location, (SizeD)size), new ShapeNodeStyle {
          Pen = new Pen(Brushes.Black, 2)
        });
      } else {
        node = graph.CreateNode(newBounds, new LogicGateNodeStyle { GateType = type });
      }

      graph.AddLabel(node, label, InteriorLabelModel.Center);

      var portDescriptors = PortDescriptor.CreatePortDescriptors(type);

      // use relative port locations
      var model = new FreeNodePortLocationModel();

      // add ports for all descriptors using the descriptor as the tag of the port
      foreach (var descriptor in portDescriptors) {
        // use the descriptor's location as offset 
        var portLocationModelParameter = model.CreateParameter(PointD.Origin, new PointD(descriptor.X, descriptor.Y));
        graph.AddPort(node, portLocationModelParameter, tag : descriptor);
      }
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
    private async void OnHLLayoutClick(object sender, EventArgs e) {
      await ApplyLayout(hl, hlData, true);
    }

    private async void OnOrthoEdgeRouterButtonClick(object sender, RoutedEventArgs e) {
      await ApplyLayout(orthogonalEdgeRouter, oerData, false);
    }

    /// <summary>
    /// Initializes drag and drop behavior on the node list to the left
    /// </summary>
    private void OnTemplateMouseDown(object sender, RoutedEventArgs e) {
      // Initialize the information for a drag&drop from the node style list.
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

    #region Graph creation and layout

    /// <summary>
    /// Perform the layout operation
    /// </summary>
    private async Task ApplyLayout(ILayoutAlgorithm layout, LayoutData layoutData, bool animateViewport) {
      // layout starting, disable button
      hlLayoutButton.IsEnabled = false;
      orthoEdgeRouterButton.IsEnabled = false;
      // do the layout
      var executor = new LayoutExecutor(graphControl, layout) {
          LayoutData = layoutData,
          Duration = TimeSpan.FromSeconds(1),
          AnimateViewport = animateViewport
      };
      await executor.Start();

      // layout finished, enable layout button again
      hlLayoutButton.IsEnabled = true;
      orthoEdgeRouterButton.IsEnabled = true;
    }

    #endregion

    private void EdgeCreationPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (graphControl != null && graphControl.InputMode != null) {
        ((GraphEditorInputMode) graphControl.InputMode).CreateEdgeInputMode.EdgeDirectionPolicy =
            (EdgeDirectionPolicy) edgeCreationPolicyComboBox.SelectedValue;
      }
    }
  }

  #region PortCandidateProvider implementation

  /// <summary>
  /// <see cref="IPortCandidateProvider"/> implementation. Provides all available ports with the specified
  /// edge direction.
  /// </summary>
  public class DescriptorDependentPortCandidateProvider : IPortCandidateProvider
  {
    /// <summary>
    /// not queried
    /// </summary>
    /// <returns>no elements</returns>
    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context, IPortCandidate target) {
      return GetCandidatesForDirection(EdgeDirection.Out, context);
    }

    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context, IPortCandidate source) {
      return GetCandidatesForDirection(EdgeDirection.In, context);
    }

    public IEnumerable<IPortCandidate> GetSourcePortCandidates(IInputModeContext context) {
      return GetCandidatesForDirection(EdgeDirection.Out, context);
    }

    public IEnumerable<IPortCandidate> GetTargetPortCandidates(IInputModeContext context) {
      return GetCandidatesForDirection(EdgeDirection.In, context);
    }

    /// <summary>
    /// Returns the suitable candidates based on the specified <see cref="EdgeDirection"/>.
    /// </summary>
    private IEnumerable<IPortCandidate> GetCandidatesForDirection(EdgeDirection direction, IInputModeContext context) {
      // If EdgeDirectionPolicy.DetermineFromPortCandidates is used, CreateEdgeInputMode queries GetSourcePortCandidates
      // as well as GetTargetPortCandidates to collect possible port candidates to start the edge creation.
      // In this case this method is called twice (with EdgeDirection.In and EdgeDirection.Out) so for each call we
      // should only return the *valid* port candidates of a port as otherwise for each port a valid as well as an invalid
      // candidate is returned.
      var provideAllCandidates = true;
      var ceim = context.ParentInputMode as CreateEdgeInputMode;
      if (ceim != null) {
        // check the edge direction policy as well as whether candidates are collected for starting or ending the edge creation
        provideAllCandidates = ceim.EdgeDirectionPolicy != EdgeDirectionPolicy.DetermineFromPortCandidates
                               || ceim.IsCreationInProgress;
      }

      var candidates = new List<IPortCandidate>();
      // iterate over all available ports
      foreach (var port in context.GetGraph().Ports) {
        // create a port candidate, invalidate it (so it is visible but not usable)
        var candidate = new DefaultPortCandidate(port) {Validity = PortCandidateValidity.Invalid};
        // get the port descriptor
        var portDescriptor = port.Tag as PortDescriptor;
        // make the candidate valid if the direction is the same as the one supplied
        if (portDescriptor != null && portDescriptor.EdgeDirection == direction) {
          candidate.Validity = PortCandidateValidity.Valid;
        }
        // add the candidate to the list
        if (provideAllCandidates || candidate.Validity == PortCandidateValidity.Valid) {
          candidates.Add(candidate);
        }
      }
      // and return the list
      return candidates;
    }
  }

  #endregion

  #region Business logic

  /// <summary>
  /// Specifies the type of a logical gate node.
  /// </summary>
  public enum LogicGateType
  {
    And,
    Nand,
    Or,
    Nor,
    Not,
    Timer,
    ADConverter
  }

  #endregion
}
