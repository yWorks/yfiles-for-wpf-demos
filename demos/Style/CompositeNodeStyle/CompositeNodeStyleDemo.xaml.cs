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
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CompositeNodeStyle
{
  /// <summary>
  ///   Bootstraps the demo.
  /// </summary>
  [Demo("composite-node-style", "style", "Composite Node Style",
      Summary = "Shows how to combine node visualizations from several styles.", Tags = ["style", "icon"],
      Keywords = ["composite"])]
  public sealed partial class CompositeNodeStyleDemo
  {
    /// <summary>
    ///   Stores the style definitions to be used with <see cref="CompositeNodeStyle"/>.
    /// </summary>
    private sealed class StyleDefinitions
    {
      public INodeStyle background = null!;
      public INodeStyle border = null!;
      public INodeStyle printer = null!;
      public INodeStyle router = null!;
      public INodeStyle scanner = null!;
      public INodeStyle server = null!;
      public INodeStyle switchStyle = null!;
      public INodeStyle workstation = null!;
    }

    public CompositeNodeStyleDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      // initialize graph component
      var graphComponent = graphControl;
      // configure user interaction
      graphComponent.InputMode = new GraphEditorInputMode();

      // create the style definitions from which to compose this demo's node visualizations
      var stylesDefinitions = CreateStyleDefinitions();

      // configures default styles for newly created graph elements
      ConfigureGraph(graphComponent.Graph, stylesDefinitions);

      // add a sample graph
      CreateGraph(graphComponent.Graph, stylesDefinitions);

      // center the sample graph in the visible area
      graphComponent.FitGraphBounds();

      // enable undo and redo
      ((DefaultGraph) graphComponent.Graph).UndoEngineEnabled = true;
    }

    /// <summary>
    ///   Sets defaults styles for the given graph.
    /// </summary>
    private static void ConfigureGraph(IGraph graph, StyleDefinitions styleDefinitions) {
      // specify a default node size that works well with the insets used in the given style definitions
      graph.NodeDefaults.Size = new SizeD(96, 96);

      graph.NodeDefaults.Style = CreateCompositeStyle(styleDefinitions, "workstation");

      graph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Bottom;

      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x61, 0x79, 0x84)), 2)
      };
    }

    /// <summary>
    ///   Creates a sample graph.
    /// </summary>
    private void CreateGraph(IGraph graph, StyleDefinitions stylesDefinitions) {
      var data = LoadGraphData();
      var builder = new GraphBuilder(graph);
      var nodesSource = builder.CreateNodesSource(data.Nodes, node => node.Id);
      nodesSource.NodeCreator.LayoutProvider = node =>
          new RectD(node.Layout[0], node.Layout[1], node.Layout[2], node.Layout[3]);
      nodesSource.NodeCreator.CreateLabelBinding(node => node.Label);
      nodesSource.NodeCreator.StyleProvider = node => CreateCompositeStyle(stylesDefinitions, node.Type);
      builder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target, edge => edge.Id);
      builder.BuildGraph();
    }

    /// <summary>
    ///   Creates a new <see cref="CompositeNodeStyle"/> instance with the default background and border from
    ///   the given style definitions as well as the requested icon from the given style definitions.
    /// </summary>
    /// <param name="styleDefinitions">The style definitions used to compose a new node style.</param>
    /// <param name="icon">The icon to show in the composite node style instance.</param>
    private static INodeStyle CreateCompositeStyle(StyleDefinitions styleDefinitions, string icon) {
      var iconStyle = icon switch {
          "printer" => styleDefinitions.printer,
          "router" => styleDefinitions.router,
          "scanner" => styleDefinitions.scanner,
          "server" => styleDefinitions.server,
          "switch" => styleDefinitions.switchStyle,
          "workstation" => styleDefinitions.workstation,
          _ => throw new ArgumentException($"Unknown node style icon '{icon}'.", nameof(icon))
      };
      return new Graph.Styles.CompositeNodeStyle(styleDefinitions.background, styleDefinitions.border, iconStyle);
    }

    /// <summary>
    ///   Creates several style definitions to use with <see cref="CompositeNodeStyle"/>.
    /// </summary>
    private StyleDefinitions CreateStyleDefinitions() =>
        new() {
            // the main style for the nodes in this demo
            // aside from background visualization, this style is used for all style related operations
            // such as hit testing and visibility testing
            background = new ShapeNodeStyle {
                Pen = null,
                Brush = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd)),
                Shape = ShapeNodeShape.Ellipse
            },
            // the border visualization for the nodes in this demo
            border = new ShapeNodeStyle {
                Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x61, 0x79, 0x84)), 3),
                Brush = null,
                Shape = ShapeNodeShape.Ellipse
            },
            printer =
                new ScalingNodeStyle(
                    new ImageNodeStyle((ImageSource) FindResource("PrinterIcon")) { KeepAspectRatio = true }, 0.7, 0.5),
            router =
                new ScalingNodeStyle(
                    new ImageNodeStyle((ImageSource) FindResource("RouterIcon")) { KeepAspectRatio = true }, 0.7, 0.7),
            scanner =
                new ScalingNodeStyle(
                    new ImageNodeStyle((ImageSource) FindResource("ScannerIcon")) { KeepAspectRatio = true }, 0.7, 0.4),
            server =
                new ScalingNodeStyle(
                    new ImageNodeStyle((ImageSource) FindResource("ServerIcon")) { KeepAspectRatio = true }, 0.4, 0.7),
            switchStyle =
                new ScalingNodeStyle(
                    new ImageNodeStyle((ImageSource) FindResource("SwitchIcon")) { KeepAspectRatio = true }, 0.8, 0.3),
            workstation =
                new ScalingNodeStyle(
                    new ImageNodeStyle((ImageSource) FindResource("WorkstationIcon")) { KeepAspectRatio = true }, 0.7,
                    0.6)
        };

    private static GraphData LoadGraphData() {
      using var stream = GetResourceStream("Resources.graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(stream)!;
    }

    private static Stream GetResourceStream(string suffix) {
      var assembly = typeof(CompositeNodeStyleDemo).Assembly;
      var resourceName = Array.Find(assembly.GetManifestResourceNames(),
          name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
      return resourceName is not null
          ? assembly.GetManifestResourceStream(resourceName)!
          : throw new InvalidOperationException($"Missing embedded resource ending in '{suffix}'.");
    }

    [STAThread]
    public static void Main() {
      Run<CompositeNodeStyleDemo>();
    }
  }
}
