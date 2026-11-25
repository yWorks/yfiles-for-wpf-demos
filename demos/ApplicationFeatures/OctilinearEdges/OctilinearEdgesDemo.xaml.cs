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
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.OctilinearEdges
{
  /// <summary>
  ///   Demonstrates interactive editing of octilinear edge routes.
  /// </summary>
  [Demo("octilinear-edge-editing", "application-features", "Octilinear Edge Editing",
      Summary = "Shows how to implement interactive octilinear edge editing.", Tags = ["style", "interaction", "bends"],
      Keywords = ["OrthogonalEdgeEditingContext", "PositionHandler", "handles", "IHandle", "selection"],
      RelatedDemos = ["orthogonal-edge-editing", "layout-styles"])]
  public sealed partial class OctilinearEdgesDemo
  {
    public OctilinearEdgesDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Configures the graph, loads the sample data, applies the initial octilinear layout, and enables editing.
    /// </summary>
    protected override void Initialize() {
      // Enable graph editing
      graphControl.InputMode = new GraphEditorInputMode { SnapContext = new GraphSnapContext() };

      // Register octilinear segment interaction handlers
      RegisterOctilinearSegmentHandler.Register(graphControl);

      // Configure the OctilinearEdgeStyle that renders bends as octilinear segments
      InitializeGraph(graphControl.Graph);

      // Load sample graph
      BuildGraph(graphControl.Graph);

      // Make sure the graph is centered in the view before arranging it
      graphControl.FitGraphBounds();

      // Apply an octilinear routed layout
      ApplyOctilinearLayout.ApplyInitialOctilinearLayout(graphControl, Utils.CreateHierarchicalLayout());

      // Enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);

      ToolbarSupport.EnableGraphML(graphControl);
    }

    private static void InitializeGraph(IGraph graph) {
      var nodeStroke = (Brush) new SolidColorBrush(Color.FromRgb(0x4d, 0x4d, 0x4d)).GetAsFrozen();
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Rectangle,
          Brush = new SolidColorBrush(Color.FromRgb(0xc1, 0xc1, 0xc1)),
          Pen = new Pen(nodeStroke, 1)
      };

      var edgeColor = (Brush) new SolidColorBrush(Color.FromRgb(0xdb, 0x3a, 0x34)).GetAsFrozen();
      graph.EdgeDefaults.Style = new OctilinearEdgeStyle {
          Pen = (Pen) new Pen(edgeColor, 2).GetAsFrozen(),
          TargetArrow = new Arrow(ArrowType.Triangle, edgeColor),
          PreferredOctilinearSegmentLength = 30
      };
    }

    /// <summary>
    ///   Builds the graph from the embedded JSON sample.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var jsonDocument = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, jsonDocument);
    }

    private void OnSegmentLengthChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      ToolbarSupport.OnSegmentLengthChanged(graphControl, e.NewValue);
    }

    private async void OnApplyLayoutClicked(object sender, RoutedEventArgs e) {
      await ToolbarSupport.ApplyLayout(graphControl);
    }

    private async void OnApplyRouterClicked(object sender, RoutedEventArgs e) {
      await ToolbarSupport.ApplyRouter(graphControl);
    }

    [STAThread]
    public static void Main() {
      Run<OctilinearEdgesDemo>();
    }
  }
}
