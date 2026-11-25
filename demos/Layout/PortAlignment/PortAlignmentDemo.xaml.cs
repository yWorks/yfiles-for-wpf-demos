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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.PortAlignment
{
  /// <summary>
  ///   Displays the demo's graph.
  /// </summary>
  [Demo("port-alignment", "layout", "Port Alignment",
      Summary = "Shows how to implement port alignment in hierarchical layout to visualize paths.",
      Tags = ["port", "hierarchical", "path"], Keywords = ["layout", "portdata"])]
  public sealed partial class PortAlignmentDemo
  {
    private bool layoutRunning;
    private static readonly double[] PathThickness = [6, 7, 10, 14, 8, 20, 9];

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public PortAlignmentDemo() {
      InitializeComponent();
      InitializeUI();
    }

    protected override async void Initialize() {
      // initialize the graph component and input mode
      GraphControl.InputMode = new GraphViewerInputMode();

      // create nodes and edges of the graph
      BuildGraph();

      // apply the hierarchical layout with port alignment
      await RunLayout();
      await GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Create and configure port data for port alignment
    /// </summary>
    private void ConfigurePortAlignment(HierarchicalLayoutData<INode, IEdge, IModelItem, ILabel, ILabel> layoutData) {
      if (togglePortAlignment.IsChecked == true) {
        // align source and target ports with their edge paths
        layoutData.Ports.SourcePortAlignmentIds.MapperFunction = edge => ((EdgeData) edge.Tag!).Path;
        layoutData.Ports.TargetPortAlignmentIds.MapperFunction = edge => ((EdgeData) edge.Tag!).Path;
      }
    }

    /// <summary>
    ///   Arranges the demo's graph with <see cref="HierarchicalLayout"/>.
    /// </summary>
    private async Task RunLayout() {
      if (layoutRunning) {
        return;
      }
      layoutRunning = true;
      DisableUI(true);

      var layout = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
          MinimumLayerDistance = 250
      };
      // apply default node size
      layout.LayoutStages.Prepend(new NodeResizeStage(30, 50));

      var layoutData = layout.CreateLayoutData(Graph);
      layoutData.EdgeDescriptors.Constant = new HierarchicalLayoutEdgeDescriptor {
          RoutingStyleDescriptor = new RoutingStyleDescriptor { DefaultRoutingStyle = RoutingStyle.Curved }
      };

      layoutData.EdgeThickness.MapperFunction = edge => PathThickness[((EdgeData) edge.Tag!).Path];

      // configure port alignment
      ConfigurePortAlignment(layoutData);

      // configure minimum port distance
      // nodes will be automatically enlarged to meet the specified port distance if necessary
      layoutData.NodeDescriptors.Constant = new HierarchicalLayoutNodeDescriptor {
          MinimumPortDistance = minimumPortDistance.Value
      };

      try {
        await GraphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(700), layoutData);
      } finally {
        layoutRunning = false;
        DisableUI(false);
      }
    }

    /// <summary>
    ///   Binds commands and actions to the demo's UI controls.
    /// </summary>
    private void InitializeUI() {
      portDistanceLabel.Content = minimumPortDistance.Value.ToString("0");
      togglePortAlignment.Click += async (_, _) => await RunLayout();
      minimumPortDistance.ValueChanged += OnMinimumPortDistanceChanged;
      minimumPortDistance.AddHandler(Thumb.DragCompletedEvent,
          new DragCompletedEventHandler(OnMinimumPortDistanceDragCompleted));
    }

    private async void OnMinimumPortDistanceChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      portDistanceLabel.Content = e.NewValue.ToString("0");
      if (!minimumPortDistance.IsMouseCaptureWithin) {
        await RunLayout();
      }
    }

    private async void OnMinimumPortDistanceDragCompleted(object sender, DragCompletedEventArgs e) {
      if (IsLoaded) {
        await RunLayout();
      }
    }

    /// <summary>
    ///   Create nodes and edges according to the given data.
    ///   Apply the same color to edges that belong to the same path.
    /// </summary>
    public void BuildGraph() {
      var graph = Graph;

      // apply node style
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Rectangle,
          Brush = Themes.PaletteOrange.EdgeLabelFill,
          Pen = null
      };

      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle(Themes.PaletteOrange, false);

      // define node and edge sources
      BuildGraphFromJson("sample.json", graph, edgeTagType: typeof(EdgeData));

      // different colors for different edge paths
      Palette[] palettes = [
          Themes.PaletteOrange,
          Themes.PaletteBlue,
          Themes.PaletteRed,
          Themes.PaletteGreen,
          Themes.PalettePurple,
          Themes.PaletteLightblue,
          Themes.Palette14
      ];

      // apply colors to edges depending on the paths they belong to
      foreach (var edge in graph.Edges) {
        var path = ((EdgeData) edge.Tag!).Path;
        var palette = palettes[path];
        var style = DemoStyles.CreateDemoEdgeStyle(palette, false);
        style.Pen = new Pen(palette.Fill, PathThickness[path]);
        graph.SetStyle(edge, style);
      }
    }

    /// <summary>
    ///   Helper class to resize nodes during layout.
    /// </summary>
    private sealed class NodeResizeStage(double width, double height) : LayoutStageBase
    {
      protected override void ApplyLayoutImpl(LayoutGraph graph) {
        foreach (var node in graph.Nodes) {
          node.Layout.Width = width;
          node.Layout.Height = height;
        }

        CoreLayout?.ApplyLayout(graph);
      }
    }

    /// <summary>
    ///   Helper function to disable UI during layout animation
    /// </summary>
    private void DisableUI(bool disabled) {
      togglePortAlignment.IsEnabled = !disabled;
      minimumPortDistance.IsEnabled = !disabled;
    }

    private sealed record EdgeData(int Id, int Source, int Target, int Path);

    [STAThread]
    private static void Main() => Run<PortAlignmentDemo>();
  }
}
