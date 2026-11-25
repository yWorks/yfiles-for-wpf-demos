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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ArcDiagram
{
  /// <summary>
  ///   Interaction logic for ArcDiagramDemo.xaml
  /// </summary>
  [Demo("arc-diagram", "layout", "Arc Diagram", Summary = "Shows how to visualize a graph as an arc diagram.",
      Tags = ["layout", "curve"], Keywords = ["layoutfeatures", "arcs", "bezier", "networks", "curves"])]
  public sealed partial class ArcDiagramDemo
  {
    public ArcDiagramDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps this demo.
    /// </summary>
    protected override async void Initialize() {
      // Initially disable interactive editing.
      ConfigureUserInteraction();

      // Configure default styles for the demo's graph.
      ConfigureGraph();

      // Create the demo's sample graph.
      CreateSampleGraph();

      // Bind actions to the demo's UI controls.
      nodeOrderComboBox.SelectionChanged += OnNodeOrderSelectionChanged;

      // Calculate and animate an initial layout for the demo's sample graph.
      await ArrangeAsync();

      // Enable undo and redo after the initial graph has been created and laid out.
      graphControl.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Configures default styles for the graph.
    /// </summary>
    private void ConfigureGraph() {
      var graph = graphControl.Graph;
      graph.NodeDefaults.Size = new SizeD(30, 30);
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x6C, 0x00)),
          Shape = ShapeNodeShape.Ellipse,
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 1.5)
      };

      graph.EdgeDefaults.Style = new BezierEdgeStyle {
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)), 4)
      };

      graph.NodeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC4, 0x99)),
          TextBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x2B, 0x00)),
          Shape = LabelShape.Pill,
          Padding = new InsetsD(2, 4, 2, 4)
      };
    }

    /// <summary>
    ///   Creates the sample graph for this demo.
    /// </summary>
    private void CreateSampleGraph() {
      var nodeTagType = typeof(NodeData);
      BuildGraphFromJson("graph-data.json", graphControl.Graph, nodeTagType);

      foreach (var node in graphControl.Graph.Nodes) {
        graphControl.Graph.AddLabel(node, ((NodeData) node.Tag!).Id.ToString());
      }
    }

    /// <summary>
    ///   Enables interactive editing for the graph view.
    /// </summary>
    private void ConfigureUserInteraction() {
      var inputMode = new GraphEditorInputMode {
          // Do not show handles for edge reconnection or port movement,
          // because active port handles make it difficult to move the small default nodes of this demo.
          ShowHandleItems = GraphItemTypes.Bend | GraphItemTypes.EdgeLabel | GraphItemTypes.Node |
              GraphItemTypes.NodeLabel | GraphItemTypes.PortLabel
      };
      graphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Arranges the graph displayed in the graph view.
    /// </summary>
    private async Task ArrangeAsync() {
      nodeOrderComboBox.IsEnabled = false;
      var algorithm = new ArcDiagramLayout { NodeOrder = (NodeOrder) nodeOrderComboBox.SelectedValue };
      await graphControl.ApplyLayoutAnimated(algorithm);
      // Center the demo's graph in its visible area.
      await graphControl.FitGraphBounds();
      nodeOrderComboBox.IsEnabled = true;
    }

    private async void OnArrangeClick(object sender, RoutedEventArgs e) {
      await ArrangeAsync();
    }

    private async void OnNodeOrderSelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (graphControl.Graph.Nodes.Any()) {
        await ArrangeAsync();
      }
    }

    internal class NodeData
    {
      public int Id { get; init; }
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<ArcDiagramDemo>();
    }
  }
}
