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

using System.Linq;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.CustomCopyAndPaste
{
  internal static class BuildGraph
  {
    /// <summary>
    ///   Builds a graph from the given data.
    /// </summary>
    /// <param name="graph">The graph to build.</param>
    /// <param name="data">The JSON data to build the graph from.</param>
    internal static void Build(IGraph graph, GraphData data) {
      // Set different default label models for nodes and edges.
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorNodeLabelModel.Top;
      graph.EdgeDefaults.Labels.LayoutParameter =
          new EdgeSegmentLabelModel().CreateParameterFromCenter(sideOfEdge: EdgeSides.AboveEdge);

      var graphBuilder = new GraphBuilder(graph);

      var nodesSource = graphBuilder.CreateNodesSource(data.Nodes, node => node.Id);
      nodesSource.NodeCreator.TagProvider = node => node.Tag;
      nodesSource.NodeCreator.LayoutProvider = node => {
        // Make raw material nodes slightly larger than other nodes.
        var height = node.Tag == "raw-material" ? 60 : 30;
        return new RectD(0, 0, 120, height);
      };
      nodesSource.NodeCreator.StyleProvider = node => DemoStyles.CreateDemoNodeStyle(GetPalette(node.Tag));
      var nodeLabelCreator = nodesSource.NodeCreator.CreateLabelBinding(node => node.Label);
      nodeLabelCreator.StyleProvider = node => DemoStyles.CreateDemoNodeLabelStyle(GetPalette(node.Tag));
      nodeLabelCreator.LayoutParameterProvider = _ => InteriorNodeLabelModel.Center;

      var edgesSource =
          graphBuilder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target, edge => edge.Id);
      edgesSource.EdgeCreator.StyleProvider = edge => {
        var targetTag = data.Nodes.First(node => node.Id == edge.Target).Tag;
        var palette = GetPalette(targetTag);
        var edgeStyle = DemoStyles.CreateDemoEdgeStyle(palette);
        if (edge.Label == "refine") {
          // Make refine edges slightly thicker than other edges.
          edgeStyle.Pen = new Pen(palette.Stroke, 3);
        }
        return edgeStyle;
      };
      var edgeLabelCreator = edgesSource.EdgeCreator.CreateLabelBinding(edge => edge.Label);
      edgeLabelCreator.StyleProvider = edge => {
        var targetTag = data.Nodes.First(node => node.Id == edge.Target).Tag;
        return DemoStyles.CreateDemoEdgeLabelStyle(GetPalette(targetTag));
      };
      edgeLabelCreator.LayoutParameterProvider = _ => new EdgeSegmentLabelModel().CreateParameterFromCenter();

      graphBuilder.BuildGraph();

      // Reset the style of some graph items to whom style can be copied.
      foreach (var node in
          graph.Nodes.Where(node => node.Labels.First().Text is "Bauxite" or "Aluminum" or "Housing")) {
        graph.SetNodeLayout(node, RectD.FromCenter(node.Layout.GetCenter(), graph.NodeDefaults.Size));
        graph.SetStyle(node, graph.NodeDefaults.Style);
        var nodeLabel = node.Labels.First();
        graph.SetStyle(nodeLabel, graph.NodeDefaults.Labels.Style);
        graph.SetLabelLayoutParameter(nodeLabel, graph.NodeDefaults.Labels.LayoutParameter);
        foreach (var edge in graph.OutEdgesAt(node)) {
          graph.SetStyle(edge, graph.EdgeDefaults.Style);
          var edgeLabel = edge.Labels.First();
          graph.SetStyle(edgeLabel, graph.EdgeDefaults.Labels.Style);
          graph.SetLabelLayoutParameter(edgeLabel, graph.EdgeDefaults.Labels.LayoutParameter);
        }
      }
    }

    /// <summary>
    ///   Returns different colors for different categories.
    /// </summary>
    /// <param name="category">The category to get the color for.</param>
    private static Palette GetPalette(string category) =>
        category switch {
            "raw-material" => Themes.PaletteOrange,
            "refined-material" => Themes.PaletteLightblue,
            "intermediate-components" => Themes.PaletteBlue,
            "product" => Themes.PaletteGreen,
            _ => Themes.PaletteOrange
        };
  }
}
