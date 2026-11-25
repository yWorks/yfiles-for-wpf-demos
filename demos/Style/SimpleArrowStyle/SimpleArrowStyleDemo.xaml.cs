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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace yWorks.Demo.SimpleArrowStyle
{
  /// <summary>
  ///   Shows how to create a simple custom arrow for edges.
  /// </summary>
  [Demo("simple-arrow-style", "style", "Simple Arrow Style",
      Summary = "Shows how to create a simple custom arrow for edges.", Tags = ["style", "arrow"],
      Keywords = ["styling", "arrow", "edge", "custom"], RelatedDemos = ["tutorial-style-implementation-edge"])]
  public sealed partial class SimpleArrowStyleDemo
  {
    private GraphMLSupport graphMLSupport = null!;

    public SimpleArrowStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      // initialize graph component
      GraphControl.InputMode = new GraphEditorInputMode();

      // initialize demo styles
      InitializeStyleDefaults(Graph);

      // create the sample graph
      CreateSampleGraph(GraphControl);

      InitializeUI();
    }

    /// <summary>
    ///   Configure GraphML so that custom arrows can be serialized and deserialized to this format.
    /// </summary>
    private GraphMLIOHandler CreateGraphMLIOHandler() {
      var graphMLIOHandler = new GraphMLIOHandler();
      graphMLIOHandler.AddXamlNamespaceMapping("http://www.yworks.com/yFilesWPF/demos/TaperedArrow/1.0",
          typeof(TaperedArrow));
      return graphMLIOHandler;
    }

    /// <summary>
    ///   Initializes the style defaults.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private void InitializeStyleDefaults(IGraph graph) {
      DemoStyles.InitDemoStyles(graph);

      // default edge style with the following arrow configuration
      var taperedArrow = new TaperedArrow(5, 10, new SolidColorBrush(Color.FromRgb(0xab, 0x23, 0x46)));
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0xab, 0x23, 0x46)), 5),
          SourceArrow = taperedArrow,
          TargetArrow = taperedArrow
      };
    }

    /// <summary>
    ///   Creates the sample graph for this demo.
    /// </summary>
    /// <param name="graphControl">The graphControl to add the nodes to</param>
    private void CreateSampleGraph(GraphControl graphControl) {
      var graph = graphControl.Graph;

      graph.Clear();
      var node1 = graph.CreateNode(new PointD(0, 0));
      var node2 = graph.CreateNode(new PointD(300, 0));

      var heights = new[] { -75.0, -50, -25, 0, 25, 50, 95 };
      var edgeThicknesses = new[] { 1.0, 2, 3, 4, 5, 8, 10 };
      var edgeColor = Themes.PaletteOrange.Stroke;

      for (var i = 0; i < heights.Length; i++) {
        var height = heights[i];
        var width = edgeThicknesses[i];
        var taperedArrow = new TaperedArrow(width, 20, edgeColor);
        graph.CreateEdge(node1, node2, new BridgeEdgeStyle {
            Pen = new Pen(edgeColor, width),
            Height = height,
            FanLength = 0.4,
            SourceArrow = taperedArrow,
            TargetArrow = taperedArrow
        });
      }

      _ = graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Binds actions to the demo's UI controls.
    /// </summary>
    private void InitializeUI() {
      graphMLSupport = new GraphMLSupport(GraphControl) { GraphMLIOHandler = CreateGraphMLIOHandler() };
      graphMLSupport.GraphMLIOHandler.ClearGraphBeforeRead = true;
      graphMLSupport.RegisterOpenSaveCommandBindings();

      reloadButton.Click += (_, _) => CreateSampleGraph(GraphControl);
    }

    [STAThread]
    public static void Main(string[] args) {
      Run<SimpleArrowStyleDemo>();
    }
  }
}
