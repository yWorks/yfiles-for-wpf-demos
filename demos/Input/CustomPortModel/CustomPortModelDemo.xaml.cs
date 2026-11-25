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
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace yWorks.Demo.CustomPortModel
{
  /// <summary>
  ///   This demo shows how to create and use a custom port model.
  /// </summary>
  [Demo("custom-port-location-model", "input", "Custom Port Location Model",
      Summary = "Shows how to implement and use a custom port location model.", Tags = ["interaction", "port"],
      Keywords = ["port candidate providers", "placements"],
      RelatedDemos = ["border-aligned-label-model", "offset-wrapper-label-model"])]
  public sealed partial class CustomPortModelDemo
  {
    public CustomPortModelDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;
      // Initialize the input mode.
      graphControl.InputMode = new GraphEditorInputMode();

      // initialize the graph
      InitializeGraph(graph);

      // Center the graph in the graph component.
      graphControl.FitGraphBounds();

      // For selected nodes show the handles.
      graph.GetDecorator().Nodes.HandleProvider.AddFactory(node => new PortsHandleProvider(node));
      // For nodes add a custom port candidate provider implementation which uses our model.
      graph.GetDecorator().Nodes.PortCandidateProvider.AddFactory(GetPortCandidateProvider);
    }

    /// <summary>
    ///   Callback used by the node decorator to provide the custom port candidates.
    /// </summary>
    private static IPortCandidateProvider GetPortCandidateProvider(INode node) {
      var model = new CustomNodePortLocationModel(10);
      return PortCandidateProviders.FromCandidates(
          new PortCandidate(node, model.CreateCustomParameter(PortLocation.Center)),
          new PortCandidate(node, model.CreateCustomParameter(PortLocation.Top)),
          new PortCandidate(node, model.CreateCustomParameter(PortLocation.Right)),
          new PortCandidate(node, model.CreateCustomParameter(PortLocation.Bottom)),
          new PortCandidate(node, model.CreateCustomParameter(PortLocation.Left)));
    }

    /// <summary>
    ///   Enables loading and saving the graph from/to GraphML.
    /// </summary>
    private GraphMLIOHandler CreateGraphMLIOHandler() {
      // Enable GraphML support with a handler for save and load operations.
      var graphMLIOHandler = new GraphMLIOHandler();
      graphMLIOHandler.ClearGraphBeforeRead = true;
      graphMLIOHandler.HandleSerialization += CustomNodePortLocationModelParameter.SerializationHandler;
      graphMLIOHandler.HandleDeserialization += CustomNodePortLocationModelParameter.DeserializationHandler;
      return graphMLIOHandler;
    }

    private void OnNewGraph(object sender, RoutedEventArgs e) {
      graphControl.Graph.Clear();
    }

    private void OnOpenGraphML(object sender, RoutedEventArgs e) {
      var dialog = new OpenFileDialog { Filter = "GraphML files (*.graphml)|*.graphml|All files (*.*)|*.*" };
      if (dialog.ShowDialog() == true) {
        CreateGraphMLIOHandler().Read(graphControl.Graph, dialog.FileName);
        graphControl.FitGraphBounds();
      }
    }

    private void OnSaveGraphML(object sender, RoutedEventArgs e) {
      var dialog = new SaveFileDialog {
          FileName = "customPortLocationModel.graphml",
          DefaultExt = ".graphml",
          Filter = "GraphML files (*.graphml)|*.graphml|All files (*.*)|*.*"
      };
      if (dialog.ShowDialog() == true) {
        CreateGraphMLIOHandler().Write(graphControl.Graph, dialog.FileName);
      }
    }

    /// <summary>
    ///   Set a custom node port model parameter for new ports, create sample nodes with custom ports,
    ///   and connect them with an edge.
    /// </summary>
    /// <param name="graph"></param>
    private static void InitializeGraph(IGraph graph) {
      DemoStyles.InitDemoStyles(graph, Themes.Palette58, edgeTheme: Themes.Palette58);
      var edgeBrush = new SolidColorBrush(Color.FromRgb(0x4e, 0x4e, 0x4e));
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(edgeBrush, 3),
          TargetArrow = new Arrow(ArrowType.Triangle, edgeBrush)
      };
      graph.NodeDefaults.Size = new SizeD(100, 100);
      // Set the default port location parameter (and thus implicitly the model as well).
      graph.NodeDefaults.Ports.LocationParameter =
          new CustomNodePortLocationModel(10).CreateCustomParameter(PortLocation.Center);
      // Set the default port style and size for this demo.
      graph.NodeDefaults.Ports.Style = new ShapePortStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = new SolidColorBrush(Color.FromRgb(0xff, 0x6c, 0x00)),
          Pen = null,
          RenderSize = new SizeD(10, 10)
      };

      var source = graph.CreateNode(new RectD(90, 90, 100, 100));
      var target = graph.CreateNode(new RectD(250, 90, 100, 100));

      // Creates a port using the default declared above.
      var sourcePort = graph.AddPort(source);
      // Creates a port using a custom model introduced here.
      var targetPort = graph.AddPort(
          target, new CustomNodePortLocationModel(10).CreateCustomParameter(PortLocation.Top));

      // Create an edge.
      graph.CreateEdge(sourcePort, targetPort);
    }

    [STAThread]
    public static void Main() => Run<CustomPortModelDemo>();
  }
}
