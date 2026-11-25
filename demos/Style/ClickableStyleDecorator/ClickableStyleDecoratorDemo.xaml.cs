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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ClickableStyleDecorator
{
  [Demo("clickable-style-decorator", "style", "Clickable Style Decorator",
      Summary = "Illustrates an approach for handling clicks on specific areas of the style.",
      Tags = ["interaction", "icon"],
      Keywords = ["NodeDecorator", "ILookupDecorator", "NodeStyleBase", "images", "events", "regions", "input mode"])]
  public sealed partial class ClickableStyleDecoratorDemo
  {
    /// <summary>
    ///   A cancelable timer to control the toast fadeout animation.
    /// </summary>
    private readonly DispatcherTimer hideTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    public ClickableStyleDecoratorDemo() {
      InitializeComponent();
      hideTimer.Tick += (_, _) => HideToast();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // initialize graph component
      GraphControl.InputMode = new GraphEditorInputMode();

      // configures default styles for newly created graph elements
      InitTutorialDefaults(Graph);

      // build the graph from the given data set
      BuildGraph(Graph, LoadGraphData());

      // layout and center the graph
      var layout = new HierarchicalLayout { MinimumLayerDistance = 35 };
      Graph.ApplyLayout(layout);
      GraphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      ((DefaultGraph) Graph).UndoEngineEnabled = true;

      // register a click listener that handles clicks on the decorator
      InitializeDecorationClickListener();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph, GraphData graphData) {
      var graphBuilder = new GraphBuilder(graph);

      var nodesSource = graphBuilder.CreateNodesSource(graphData.NodeList, item => item.Id);
      nodesSource.NodeCreator.StyleProvider = item => item.Tag is { } tag
          ? new NodeStyleDecorator(graph.NodeDefaults.GetStyleInstance(), LoadImage(tag))
          : graph.NodeDefaults.GetStyleInstance();

      graphBuilder.CreateEdgesSource(graphData.EdgeList, item => item.Source, item => item.Target);

      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Registers a click listener that handles clicks on a specific node area. In this case, we listen for clicks
    ///   on the decorator icon.
    /// </summary>
    private void InitializeDecorationClickListener() {
      if (GraphControl.InputMode is not GraphEditorInputMode inputMode) {
        return;
      }

      inputMode.ItemClicked += (_, args) => {
        if (args.Item is not INode node) {
          return;
        }
        if (node.Style is not NodeStyleDecorator nodeStyle ||
            !nodeStyle.GetDecorationLayout(node.Layout).Contains(args.Location)) {
          return;
        }

        // The decorator was clicked.
        // Handle the click if it should do nothing else than what is defined in the decorator click listener.
        // Otherwise the click will be handled by other input modes, too. For instance, a node may be created or the
        // clicked node may be selected.
        args.Handled = true;

        // Shows a toast to indicate the successful click, and hides it again.
        hideTimer.Stop();
        ShowToast();
        hideTimer.Start();
      };
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this tutorial.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitTutorialDefaults(IGraph graph) {
      // set styles that are the same for all tutorials
      DemoStyles.InitDemoStyles(graph);

      // set sizes and locations specific for this tutorial
      graph.NodeDefaults.Size = new SizeD(40, 40);
      var nodeLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(5) };
      graph.NodeDefaults.Labels.LayoutParameter =
          nodeLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    private void ShowToast() {
      toast.Visibility = Visibility.Visible;
      toastTransform.BeginAnimation(TranslateTransform.YProperty,
          new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(200)));
    }

    private void HideToast() {
      var animation = new DoubleAnimation(toastTransform.Y, 50, TimeSpan.FromMilliseconds(200));
      animation.Completed += (_, _) => toast.Visibility = Visibility.Collapsed;
      toastTransform.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    private GraphData LoadGraphData() {
      using var stream = GetResourceStream("Resources.graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(stream)!;
    }

    private ImageSource LoadImage(string name) {
      var resourceName = name.ToLowerInvariant() switch {
          "printer" => "PrinterIcon",
          "scanner" => "ScannerIcon",
          "switch" => "SwitchIcon",
          "workstation" => "WorkstationIcon",
          _ => throw new InvalidOperationException($"Unknown device icon '{name}'.")
      };
      return (ImageSource) FindResource(resourceName);
    }

    private static Stream GetResourceStream(string suffix) {
      var assembly = typeof(ClickableStyleDecoratorDemo).Assembly;
      var resourceName = Array.Find(assembly.GetManifestResourceNames(),
          name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
      return resourceName is not null
          ? assembly.GetManifestResourceStream(resourceName)!
          : throw new InvalidOperationException($"Missing embedded resource ending in '{suffix}'.");
    }

    [STAThread]
    public static void Main() {
      Run<ClickableStyleDecoratorDemo>();
    }
  }
}
