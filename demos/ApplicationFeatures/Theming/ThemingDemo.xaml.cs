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
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Theming
{
  /// <summary>
  ///   Demonstrates a custom theme for graph interaction and overview controls.
  /// </summary>
  [Demo("simple-theming", "application-features", "Theming",
      Summary = "Shows how to use a theme to change the look-and-feel of an application.",
      Tags = ["interaction", "theme"], Keywords = ["dark mode", "light mode", "colors", "theming"])]
  public sealed partial class ThemingDemo
  {
    public ThemingDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // Configure input mode and snapping.
      var inputMode = new GraphEditorInputMode {
          SnapContext = new GraphSnapContext(),
          AllowGroupingOperations = true
      };
      inputMode.KeyboardInputMode.AddCommandBinding(ApplicationCommands.New,
          (sender, _) => ((GraphControl) sender).Graph.Clear());
      graphControl.InputMode = inputMode;

      // Configure default styles for newly created graph elements.
      var graph = graphControl.Graph;
      InitializeGraph(graph);

      // Build the graph from the given data set.
      BuildGraph(graph);

      // Layout and center the graph.
      graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 50 });
      graphControl.FitGraphBounds();

      // Initial selection of a single node to make the handles visible.
      graphControl.Selection.Nodes.Add(graph.Nodes.ElementAt(2));

      // Enable undo after populating the graph so the initial setup cannot be undone.
      graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var jsonDocument = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, jsonDocument);
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      // Define default styling for nodes, group nodes and edges.
      var stroke = BrushFrom("#605003");
      var nodeBrush = BrushFrom("#f0c808");
      graph.NodeDefaults.Style = new RectangleNodeStyle {
          Brush = nodeBrush,
          Pen = new Pen(stroke, 1.5),
          CornerStyle = CornerStyle.Round,
          CornerSize = 3.5
      };
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(stroke, 1.5),
          TargetArrow = new Arrow(ArrowType.Triangle, stroke)
      };
      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabBrush = nodeBrush,
          Pen = new Pen(nodeBrush, 2),
          ContentAreaPadding = new InsetsD(15)
      };

      // Define default label styles for nodes, group nodes and edges.
      var nodeLabelStyle = new LabelStyle();
      nodeLabelStyle.Shape = LabelShape.RoundRectangle;
      nodeLabelStyle.BackgroundBrush = BrushFrom("#f9e99c");
      nodeLabelStyle.TextBrush = stroke;
      nodeLabelStyle.VerticalTextAlignment = VerticalAlignment.Center;
      nodeLabelStyle.TextAlignment = TextAlignment.Center;
      nodeLabelStyle.Padding = new InsetsD(4, 2, 4, 1);
      graph.NodeDefaults.Labels.Style = nodeLabelStyle;

      graph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          VerticalTextAlignment = VerticalAlignment.Center,
          TextBrush = Brushes.White
      };

      var edgeLabelStyle = (LabelStyle) nodeLabelStyle.Clone();
      edgeLabelStyle.BackgroundBrush = BrushFrom("#bfb99a");
      graph.EdgeDefaults.Labels.Style = edgeLabelStyle;

      // Set sizes and label locations specific for this demo.
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = NinePositionsEdgeLabelModel.TargetAbove;
      graph.GroupNodeDefaults.Labels.LayoutParameter = new GroupNodeLabelModel().CreateTabBackgroundParameter();
    }

    private static SolidColorBrush BrushFrom(string color) => new((Color) ColorConverter.ConvertFromString(color));

    [STAThread]
    public static void Main() {
      Run<ThemingDemo>();
    }
  }
}
