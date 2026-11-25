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
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.InteractiveGraphRestructuring
{
  /// <summary>
  ///   A demo that shows how to interactively relocate subtrees from one parent to another.
  /// </summary>
  [Demo("interactive-graph-restructuring", "input", "Interactive Graph Restructuring",
      Summary = "Shows how to interactively relocate subtrees from one parent to another.",
      Tags = ["interactive layout", "animation"], Keywords = [
          "layoutfeatures", "tree", "sub-tree", "re-parent", "fill area layout", "create space", "space maker",
          "hierarchical"
      ])]
  public sealed partial class InteractiveGraphRestructuringDemo
  {
    private Subtree? subtree;

    public InteractiveGraphRestructuringDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      var graph = graphControl.Graph;
      DemoStyles.InitDemoStyles(graph, orthogonalEdgeEditing: true);
      graph.NodeDefaults.ShareStyleInstance = false;

      InitializeHighlightDecorator(graph);
      InitializeInputMode(graph);
      LoadGraph(graph);
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Initializes the input mode.
    /// </summary>
    private void InitializeInputMode(IGraph graph) {
      var mode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.None,
          AllowCreateBend = false,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          MarqueeSelectionInputMode = { Enabled = false },
          MoveUnselectedItemsInputMode = { Enabled = true },
          MoveViewportInputMode = { BeginRecognizer = EventRecognizers.MouseDrag },
          // enable the ItemHoverInputMode and let it handle edges and nodes
          ItemHoverInputMode = {
              Enabled = true,
              HoverItems = GraphItemTypes.Node
          }
      };
      graphControl.InputMode = mode;

      // node style that is applied by SubtreePositionHandler while moving subtree nodes
      var movingNodeStyle = DemoStyles.CreateDemoNodeStyle(Themes.Palette12);
      movingNodeStyle.Pen = CreatePen(movingNodeStyle.Pen!.Brush, 3.5);
      // adds the position handler that will relocate the selected node along with the subtree rooted at it
      graph.GetDecorator().Nodes.PositionHandler
          .AddWrapperFactory((node, handler) => new SubtreePositionHandler(node, handler, movingNodeStyle));

      var defaultStyle = (RectangleNodeStyle) graph.NodeDefaults.Style;
      var defaultBrush = defaultStyle.Pen!.Brush;
      // normal and thicker stroke that will be set by the hovered item change listener
      var normalPen = CreatePen(defaultBrush, 1.5);
      var hoveredPen = CreatePen(defaultBrush, 3.5);

      // handle changes on the hovered items
      mode.ItemHoverInputMode.HoveredItemChanged += (_, args) => {
        if (subtree is not null) {
          foreach (var node in subtree.Nodes) {
            ((RectangleNodeStyle) node.Style).Pen = normalPen;
          }
        }

        subtree = args.Item is INode hoveredNode ? new Subtree(graph, hoveredNode) : null;
        if (subtree is not null) {
          foreach (var node in subtree.Nodes) {
            ((RectangleNodeStyle) node.Style).Pen = hoveredPen;
          }
        }
        graphControl.Invalidate();
      };
    }

    /// <summary>
    ///   Installs a different highlight decorator visual.
    /// </summary>
    private static void InitializeHighlightDecorator(IGraph graph) {
      var highlightStyle = new RectangleNodeStyle {
          CornerStyle = CornerStyle.Round,
          Brush = null,
          Pen = CreatePen(Brushes.DeepSkyBlue, 5)
      };
      graph.GetDecorator().Nodes.HighlightRenderer.AddConstant(
          new NodeStyleIndicatorRenderer {
              NodeStyle = highlightStyle,
              Margins = new InsetsD(8)
          });
    }

    private static Pen CreatePen(Brush brush, double thickness) {
      var pen = new Pen(brush, thickness);
      pen.Freeze();
      return pen;
    }

    /// <summary>
    ///   Loads the graph.
    /// </summary>
    private void LoadGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("GraphData.json") ??
          throw new InvalidOperationException("The graph data resource is missing.");
      var data =
          JsonSerializer.Deserialize<GraphData>(stream,
              new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
          throw new InvalidOperationException("The graph data resource is invalid.");

      var nodes = new Dictionary<int, INode>();
      foreach (var nodeData in data.NodesSource) {
        nodes.Add(nodeData.Id,
            graph.CreateNode(new RectD(nodeData.X, nodeData.Y, graph.NodeDefaults.Size.Width,
                graph.NodeDefaults.Size.Height)));
      }

      foreach (var edgeData in data.EdgesSource) {
        var edge = graph.CreateEdge(nodes[edgeData.Source], nodes[edgeData.Target]);
        // adds the bends
        foreach (var bend in edgeData.Bends) {
          graph.AddBend(edge, new PointD(bend.X, bend.Y));
        }
      }
    }

    private sealed class GraphData
    {
      public List<NodeData> NodesSource { get; init; } = [];

      public List<EdgeData> EdgesSource { get; init; } = [];
    }

    private sealed class NodeData
    {
      public int Id { get; init; }

      public double X { get; init; }

      public double Y { get; init; }
    }

    private sealed class EdgeData
    {
      public int Source { get; init; }

      public int Target { get; init; }

      public List<BendData> Bends { get; } = [];
    }

    private sealed class BendData
    {
      public double X { get; init; }

      public double Y { get; init; }
    }

    [STAThread]
    public static void Main() => Run<InteractiveGraphRestructuringDemo>();
  }
}
