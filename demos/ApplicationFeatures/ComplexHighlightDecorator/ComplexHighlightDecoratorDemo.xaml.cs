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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.ComplexHighlightDecorator
{
  /// <summary>
  ///   Shows how to highlight a node when the mouse hovers over it. The shape of the highlight
  ///   effect differs depending on the data stored in the node's tag.
  /// </summary>
  [Demo("complex-highlight", "application-features", "Complex Highlight Decorator",
      Summary = "Shows how to highlight nodes with different effects based on data stored in their tags.",
      Tags = ["highlight", "hover", "interaction"],
      Keywords = ["GraphDecorator", "NodeDecorator", "HighlightDecorator", "HighlightIndicatorManager", "margin"])]
  public sealed partial class ComplexHighlightDecoratorDemo
  {
    public ComplexHighlightDecoratorDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // create graph component
      graphControl.InputMode = new GraphEditorInputMode();

      var graph = graphControl.Graph;

      // configure default styles for newly created graph elements
      InitializeGraph(graph);

      // enable mouse hover effects for nodes and edges
      ConfigureHoverHighlight(graphControl);

      // build the graph from the given data set
      BuildGraph(graph);

      // layout and center the graph
      graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable now the undo engine to prevent undoing of the graph creation
      graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json, typeof(ShapeTag));

      foreach (var node in graph.Nodes) {
        if (node.Style is ShapeNodeStyle style) {
          style.Shape = GetShape(node.Tag);
        }
      }
    }

    /// <summary>
    ///   Registers highlight styles for the nodes and edges of the given graph.
    /// </summary>
    private static void ConfigureHoverHighlight(GraphControl graphControl) {
      var inputMode = (GraphInputMode) graphControl.InputMode!;

      // enable hover effects for nodes and edges
      inputMode.ItemHoverInputMode.Enabled = true;
      inputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Node;

      // specify the hover effect: highlight a node whenever the mouse hovers over the respective node
      inputMode.ItemHoverInputMode.HoveredItemChanged += (_, args) => {
        var highlights = graphControl.Highlights;
        highlights.Clear();
        if (args.Item != null) {
          highlights.Add(args.Item);
        }
      };

      // configure the node highlight style -> each node highlight should have the shape of the node
      var highlightPen = (Pen) new Pen(new SolidColorBrush(Color.FromRgb(0xf0, 0xc8, 0x08)), 3).GetAsFrozen();
      graphControl.Graph.GetDecorator().Nodes.HighlightRenderer.AddFactory(node => new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              // the tag of each node contains information about the appropriate shape for the highlight
              Shape = GetShape(node.Tag),
              Pen = highlightPen,
              Brush = Brushes.Transparent
          },
          // the margin from the actual node to its highlight visualization
          Margins = new InsetsD(4)
      });
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph, Themes.Palette12, shape: ShapeNodeShape.Ellipse);
      graph.NodeDefaults.ShareStyleInstance = false;
    }

    /// <summary>
    ///   Determines a suitable highlight shape depending on the given tag data.
    /// </summary>
    /// <param name="tag">The tag of the node to be highlighted.</param>
    private static ShapeNodeShape GetShape(object? tag) {
      var shape = tag is ShapeTag shapeTag ? shapeTag.Shape : null;
      return "ellipse" == shape ? ShapeNodeShape.Ellipse :
          "triangle" == shape ? ShapeNodeShape.Triangle : ShapeNodeShape.Rectangle;
    }

    [STAThread]
    public static void Main() {
      Run<ComplexHighlightDecoratorDemo>();
    }
  }
}
