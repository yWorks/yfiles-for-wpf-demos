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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Radial;

namespace yWorks.Demo.GraphSearch
{
  /// <summary>
  ///   This demo shows an implementation of the search functionality on the nodes of a graph.
  /// </summary>
  [Demo("graph-search", "application-features", "Graph Search",
      Summary = "Shows how to search for specific nodes in a graph.", Tags = ["highlight"],
      Keywords = ["query", "queries", "match", "matches", "find"])]
  public sealed partial class GraphSearchDemo
  {
    public GraphSearchDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // configure default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // configure the highlight style for the nodes that match the searched query
      InitializeSearchHighlightingStyle(graphControl);

      // then build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new RadialLayout());
      graphControl.FitGraphBounds();

      // bind the buttons to their functionality
      InitializeUI();
    }

    /// <summary>
    ///   Creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Configures the highlight style that will be used for the nodes that match the searched query.
    /// </summary>
    /// <param name="graphControl">The control containing the graph.</param>
    private static void InitializeSearchHighlightingStyle(GraphControl graphControl) {
      var searchHighlightStyle = new NodeStyleIndicatorRenderer {
          // we choose a shape node style
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.RoundRectangle,
              Pen = new Pen(new SolidColorBrush(Color.FromRgb(0x0B, 0x71, 0x89)), 3),
              Brush = Brushes.Transparent
          },
          // with a margin for the decoration
          Margins = new InsetsD(7)
      };
      graphControl.Graph.GetDecorator().Nodes.HighlightRenderer.AddConstant(searchHighlightStyle);
    }

    /// <summary>
    ///   Updates the search results by using the given string.
    /// </summary>
    /// <param name="searchText">The text to be queried</param>
    private void UpdateSearch(string searchText) {
      // we use the search highlight manager to highlight matching items
      var highlights = graphControl.Highlights;

      // first remove previous highlights
      highlights.Clear();
      if (searchText.Trim() != string.Empty) {
        foreach (var node in graphControl.Graph.Nodes) {
          if (Matches(node, searchText)) {
            // if the node is a match, highlight it
            highlights.Add(node);
          }
        }
      }
    }

    /// <summary>
    ///   Returns whether the given node is a match when searching for the given text in the label of the node.
    /// </summary>
    /// <param name="node">The node to be examined</param>
    /// <param name="text">The text to be queried</param>
    /// <returns>True if the node matches the text, false otherwise</returns>
    private static bool Matches(INode node, string text) {
      return node.Labels.Any(label => label.Text.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph);

      // set sizes and locations specific for this demo
      graph.NodeDefaults.Size = new SizeD(80, 40);
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;
    }

    /// <summary>
    ///   Binds actions to the buttons in the tutorial's toolbar.
    /// </summary>
    private void InitializeUI() {
      // adds the listener to the search box
      searchBox.TextChanged += (_, _) => UpdateSearch(searchBox.Text);
    }

    [STAThread]
    public static void Main() {
      Run<GraphSearchDemo>();
    }
  }
}
