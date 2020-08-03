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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Windows.Threading;
using Demo.yFiles.Graph.NetworkMonitoring.Model;
using Demo.yFiles.Graph.NetworkMonitoring.Simulator;
using yWorks.Controls;
using yWorks.Layout;
using yWorks.Layout.Labeling;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

[assembly : XmlnsDefinition("NetworkMonitoring", "Demo.yFiles.Graph.NetworkMonitoring")]

namespace Demo.yFiles.Graph.NetworkMonitoring
{
  /// <summary>
  /// Interaction logic for NetworkMonitoringDemo.xaml.
  /// </summary>
  public partial class NetworkMonitoringDemo
  {
    private NetworkModel model;
    private NetworkSimulator simulator;
    private DispatcherTimer simulatorTimer;
    private ILabelStyle edgeLabelStyle;
    private ILabelStyle nodeLabelStyle;
    private Dictionary<ModelEdge, IEdge> modelEdgeToIEdge;
    private Dictionary<ModelNode, INode> modelNodeToINode;
    private Animator animator;


    public NetworkMonitoringDemo() {
      InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
      InitializeStyles();
      InitGraphAndModel();
      LayoutLabels();
      SetupSimulator();
      GraphControl.FitGraphBounds();
      animator = new Animator(GraphControl);
      Closing += (o, args) => animator.Stop();
      simulatorTimer.Start();
    }

    /// <summary>
    /// Initializes the styles for nodes, edges and labels.
    /// </summary>
    private void InitializeStyles() {
      edgeLabelStyle = new LabelControlLabelStyle("EdgeLabelStyle");
      nodeLabelStyle =
          new LevelOfDetailLabelStyleDecorator
          {
            HideThreshold = 0.5,
            WrappedStyle = new CalloutLabelStyleDecorator
            {
              WrappedStyle = new LabelControlLabelStyle("NodeLabelStyle")
            }
          };
    }

    /// <summary>
    /// Initializes the graph from the supplied GraphML file and creates the model from it.
    /// </summary>
    /// <remarks>
    /// While this reads the graph from a GraphML file and constructs the model from an already-finished graph, a
    /// real-world application would likely create the model from whichever data source is available and then create
    /// the graph from it.
    /// </remarks>
    private void InitGraphAndModel() {
      var graph = new DefaultGraph
      {
        NodeDefaults = { Style = new NodeControlNodeStyle("NodeStyle") { OutlineShape = new Ellipse() } },
        EdgeDefaults = { Style = new EdgeSegmentControlEdgeStyle("EdgeStyle") }
      };
      var ioh = new GraphMLIOHandler();

      // Parse node kinds and other info
      IMapper<INode, NodeKind> nodeKinds = new DictionaryMapper<INode, NodeKind>();
      IMapper<INode, NodeInfo> nodeInfos = new DictionaryMapper<INode, NodeInfo>();
      ioh.AddInputMapper("NetworkMonitoring.NodeKind", nodeKinds);
      ioh.AddInputMapper("NetworkMonitoring.NodeInfo", nodeInfos);

      ioh.Read(graph, @"Resources\network.graphml");

      foreach (var node in graph.Nodes) {
        // Create and attach the model node to the graph node.
        var modelNode = new ModelNode
        {
          Name = nodeInfos[node].Name,
          Ip = nodeInfos[node].Ip,
          Enabled = true,
          Kind = nodeKinds[node]
        };
        node.Tag = modelNode;

        // Add the label
        var label = graph.AddLabel(node, "", FreeLabelModel.Instance.CreateDefaultParameter(), nodeLabelStyle, tag: modelNode);
        // Attach event handler for changing label visibility, so that the graph redraws accordingly.
        // Since visibility can change via clicking on the node *and* from within the label, we have to use an event
        // handler on the model node here.
        modelNode.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
          if (args.PropertyName == "LabelVisible") {
            GraphControl.Invalidate();
          }
        };
      }

      foreach (var edge in graph.Edges) {
        // Create and attach the model edge to the graph edge
        var modelEdge = new ModelEdge
        {
          Source = (ModelNode) edge.GetSourceNode().Tag,
          Target = (ModelNode) edge.GetTargetNode().Tag
        };
        edge.Tag = modelEdge;

        // Add the edge label
        var label = graph.AddLabel(edge, "", NinePositionsEdgeLabelModel.CenterCentered, edgeLabelStyle, tag: modelEdge);
      }

      // Create the mappings from model items to graph elements.
      modelNodeToINode = graph.Nodes.ToDictionary(node => (ModelNode) node.Tag);
      modelEdgeToIEdge = graph.Edges.ToDictionary(edge => (ModelEdge) edge.Tag);

      model = new NetworkModel(modelNodeToINode.Keys, modelEdgeToIEdge.Keys);
      GraphControl.Graph = graph;
    }

    /// <summary>
    /// Moves the labels so that they don't overlap.
    /// </summary>
    private void LayoutLabels() {
      // We use a MinimumNodeSizeStage here to temporarily enlarge the nodes so that the labeling algorithm won't consider
      // placements too close to the node. This is just done for esthetic reasons, as the callout style doesn't
      // work so well too close to the node.
      var labeler = new MinimumNodeSizeStage(new GenericLabeling
      {
        PlaceNodeLabels = true,
        PlaceEdgeLabels = false,
      }, 90, 90);

      GraphControl.Graph.ApplyLayout(labeler);
    }

    /// <summary>
    /// Prepares the simulator that moves packets through the network.
    /// </summary>
    private void SetupSimulator() {
      simulator = new NetworkSimulator(model);
      simulatorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
      simulatorTimer.Tick += delegate { simulator.Tick(); };
      simulator.SomethingBroke += OnNetworkFailure;
    }

    /// <summary>
    /// Event handler for clicks on the “Enable failures” button.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">Event arguments.</param>
    private void OnEnableFailuresClicked(object sender, RoutedEventArgs args) {
      simulator.FailuresEnabled = !simulator.FailuresEnabled;
      enableFailuresButton.Content = simulator.FailuresEnabled ? "Disable Failures" : "Enable Failures";
    }

    /// <summary>
    /// Event handler for clicks on nodes to show or hide the node's label.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">Event arguments.</param>
    private void OnNodeClicked(object sender, ItemClickedEventArgs<IModelItem> args) {
      var graphNode = args.Item as INode;
      if (graphNode == null) {
        return;
      }
      var modelNode = graphNode.Tag as ModelNode;
      if (modelNode == null) {
        return;
      }
      modelNode.LabelVisible = !modelNode.LabelVisible;
    }

    /// <summary>
    /// Event handler for failures in the network during the simulation.
    /// </summary>
    /// <remarks>
    /// The effect is a viewport animation to bring the failed object into view, if it is not visible already.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">Event arguments.</param>
    private async void OnNetworkFailure(object sender, EventArgs args) {
      IRectangle rect = null;

      if (sender is ModelNode) {
        // For nodes just include the node itself in the viewport
        var graphNode = modelNodeToINode[sender as ModelNode];
        rect = graphNode.Layout;
      } else if (sender is ModelEdge) {
        var modelEdge = sender as ModelEdge;
        var graphEdge = modelEdgeToIEdge[modelEdge];
        
        // For edges we need to get the bounding box of the end points
        // We don't need to consider bends in this demo as there are none.
        rect = new RectD(graphEdge.SourcePort.GetLocation(), graphEdge.TargetPort.GetLocation());
      }

      // Don't do anything if the failing element is visible already
      if (GraphControl.Viewport.Contains(rect)) {
        return;
      }

      // Enlarge the viewport so that we get an overview of the neighborhood as well
      var rectD = rect.ToRectD().GetEnlarged(new InsetsD(200));

      // Animated the transition to the failed element
      await animator.Animate(new ViewportAnimation(GraphControl, rectD, TimeSpan.FromMilliseconds(1000)).CreateEasedAnimation());
    }
  }
}
