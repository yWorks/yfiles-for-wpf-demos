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
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.EdgeToEdge
{
  /// <summary>
  ///   Interaction logic for EdgeToEdgeDemo.xaml
  /// </summary>
  [Demo("edge-to-edge", "view", "Edge To Edge", Summary = "Shows edge-to-edge connections.",
      Tags = ["edge creation", "interaction"], Keywords = ["layoutfeatures", "port candidate providers"])]
  public sealed partial class EdgeToEdgeDemo
  {
    /// <summary>
    ///   This application demonstrates the use of edge-to-edge connections. Edges can be created interactively
    ///   between nodes, nodes and edges and between two edges. Also, this application enables moving the source or
    ///   target of an edge to another owner.
    ///   Connecting the source or target of an edge to itself is prohibited since this is conceptually forbidden.
    ///   Edge-to-edge connections have to be enabled explicitly using the property
    ///   <see cref="CreateEdgeInputMode.AllowEdgeToEdgeConnections"/>.
    ///   This demo also includes customized implementations of <see cref="IPortCandidateProvider"/>,
    ///   <see cref="IEdgeReconnectionPortCandidateProvider"/>, <see cref="IHitTestable"/>,
    ///   <see cref="IEdgePortHandleProvider"/> and <see cref="IPortLocationModel"/> to enable custom behavior like
    ///   reconnecting an existing edge to another edge, starting edge creation from an edge etc.
    /// </summary>
    protected override void Initialize() {
      InitializeInputMode();
      InitializeGraph();
      CreateSampleGraph();
      InitializeUI();
    }

    /// <summary>
    ///   Initializes the graph instance setting default styles and customizing behavior.
    /// </summary>
    private void InitializeGraph() {
      var graph = graphControl.Graph;

      graph.SetUndoEngineEnabled(true);

      DemoStyles.InitDemoStyles(graph);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2b, 0x00)), 1.5),
          OrthogonalEditing = true
      };
      graph.EdgeDefaults.ShareStyleInstance = false;

      // assign a port style for the ports at the edges
      graph.EdgeDefaults.Ports.Style = new ShapePortStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = Brushes.Black,
          Pen = null!,
          RenderSize = new SizeD(3, 3)
      };

      // enable edge port candidates
      graph.GetDecorator().Edges.PortCandidateProvider.AddFactory(edge => new EdgePathPortCandidateProvider(edge));

      // set IEdgeReconnectionPortCandidateProvider to allow re-connecting edges to other edges
      graph.GetDecorator().Edges.ReconnectionPortCandidateProvider.AddFactory(edge =>
          EdgeReconnectionPortCandidateProviders.FromAllNodeAndEdgeCandidates(edge));

      graph.GetDecorator().Edges.PortHandleProvider.AddFactory(edge => {
        var portRelocationHandleProvider = new PortRelocationHandleProvider(graph, edge) {
            Visualization = Visualization.Live
        };
        return portRelocationHandleProvider;
      });
    }

    /// <summary>
    ///   Creates the <see cref="GraphSnapContext"/>.
    /// </summary>
    private static GraphSnapContext CreateSnapContext() {
      var snapContext = new GraphSnapContext {
          Enabled = false,
          // disable grid snapping
          GridSnapType = GridSnapTypes.None
      };
      // add constraint provider for nodes, bends, and ports
      var gridInfo = new GridInfo(50, 50, new PointD(0, 0));
      snapContext.NodeGridConstraintProvider = new GridConstraintProvider(gridInfo);
      snapContext.BendGridConstraintProvider = new GridConstraintProvider(gridInfo);
      snapContext.PortGridConstraintProvider = new GridConstraintProvider(gridInfo);
      return snapContext;
    }

    /// <summary>
    ///   Initializes the input mode and enables edge-to-edge connections on the input mode.
    /// </summary>
    private void InitializeInputMode() {
      var mode = new GraphEditorInputMode {
          SnapContext = CreateSnapContext(),
          OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext { Enabled = false }
      };

      mode.CreateEdgeInputMode.AllowEdgeToEdgeConnections = true;

      // create bends only when shift is pressed
      mode.CreateBendInputMode.BeginRecognizer = (source, evt) =>
          EventRecognizers.MouseLeftDown(source, evt) && EventRecognizers.ShiftIsDown(source, evt);

      mode.CreateEdgeInputMode.EdgeCreationStarted += (_, evt) => SetRandomEdgeColor(evt.Item);

      // disable directional constraint recognizer because Shift is used for bend creation
      mode.HandleInputMode.DirectionalConstraintRecognizer = (source, evt) =>
          EventRecognizers.ShiftDown(source, evt) && !mode.HandleInputMode.AffectedItems.OfType<IBend>().Any();

      graphControl.InputMode = mode;
    }

    /// <summary>
    ///   Creates a small sample graph containing edge to edge connections.
    /// </summary>
    private void CreateSampleGraph() {
      var graph = graphControl.Graph;

      var n1 = graph.CreateNode(new PointD(0, 0));
      var n2 = graph.CreateNode(new PointD(500, 0));
      var n3 = graph.CreateNode(new PointD(0, 300));
      var n4 = graph.CreateNode(new PointD(500, 300));

      var e1 = graph.CreateEdge(n1, n3);
      var e2 = graph.CreateEdge(n2, n4);
      var e3 = graph.CreateEdge(n3, n4);

      graph.AddBends(e3, [
          new PointD(100, 300),
          new PointD(100, 450),
          new PointD(400, 450),
          new PointD(400, 300)
      ]);

      var p1 = graph.AddPort(e1, EdgePathPortLocationModel.Instance.CreateRatioParameter(0.4));
      var p2 = graph.AddPort(e2, EdgePathPortLocationModel.Instance.CreateRatioParameter(0.4));
      var e4 = graph.CreateEdge(p1, p2);
      var p3 = graph.AddPort(e4, EdgePathPortLocationModel.Instance.CreateRatioParameter());
      var p4 = graph.AddPort(e3, EdgePathPortLocationModel.Instance.CreateRatioParameter(0.8));
      var e5 = graph.CreateEdge(p3, p4);
      graph.AddBend(e5, new PointD(250, 360));

      graphControl.FitGraphBounds();

      graph.GetUndoEngine()!.Clear();
    }

    /// <summary>
    ///   Wires up the UI.
    /// </summary>
    private void InitializeUI() {
      var settings = new EdgeToEdgeSettings();
      demoToolBar.DataContext = settings;

      var inputMode = (GraphEditorInputMode) graphControl.InputMode!;
      settings.SettingChanged += (_, _) => {
        inputMode.SnapContext.Enabled = settings.SnappingEnabled;
        inputMode.OrthogonalEdgeEditingContext.Enabled = settings.OrthogonalEditingEnabled;
      };

      graphControl.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (_, _) => {
        var graph = graphControl.Graph;
        graph.Clear();
        graph.GetUndoEngine()?.Clear();
        graphControl.FitGraphBounds();
      }));
    }

    /// <summary>
    ///   Creates a random colored pen and uses that one for the style.
    /// </summary>
    private static void SetRandomEdgeColor(IEdge edge) {
      if (edge.Style is PolylineEdgeStyle polylineEdgeStyle) {
        polylineEdgeStyle.Pen = new Pen(
            new SolidColorBrush(Color.FromRgb((byte) Random.Shared.Next(256), (byte) Random.Shared.Next(256),
                (byte) Random.Shared.Next(256))), 1.5);
      }
    }

    [STAThread]
    private static void Main() {
      Run<EdgeToEdgeDemo>();
    }

    public EdgeToEdgeDemo() {
      InitializeComponent();
    }
  }
}
