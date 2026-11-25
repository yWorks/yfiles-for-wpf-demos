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
using System.Threading.Tasks;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.SmartClickNavigation
{
  [Demo("smart-click-navigation", "application-features", "Smart Click Navigation",
      Summary = "Shows the how to scroll and zoom to the area of interest by single edge-clicks.",
      Keywords = ["navigation", "zoom", "move", "interaction"], Tags = ["exploration"])]
  public sealed partial class SmartClickNavigationDemo
  {
    private IGraph Graph => graphControl.Graph;

    public SmartClickNavigationDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.SelectionIndicatorManager.Enabled = false;
      graphControl.FocusIndicatorManager.Enabled = false;

      // Initializes the input mode
      InitializeInputMode();

      // Initializes the highlight style
      InitializeHighlightingStyle(Graph);

      // Configures default styles for newly created graph elements
      InitializeGraph(Graph);

      // Create an initial sample graph
      CreateSampleGraph();
      graphControl.FitGraphBounds();

      // Enable the undo engine. This prevents undoing the graph creation.
      Graph.SetUndoEngineEnabled(true);

      // Zoom to a node that has the "Sport" label
      _ = graphControl.ZoomToAnimated(1, new PointD(1637.468, 1828));
    }

    /// <summary>
    ///   Creates the sample graph.
    /// </summary>
    private void CreateSampleGraph() {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var graphData = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(Graph, graphData);
    }

    /// <summary>
    ///   Configures the highlight style used for clicked graph items.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeHighlightingStyle(IGraph graph) {
      var highlightPen = new Pen(new SolidColorBrush(Color.FromRgb(0x62, 0x1B, 0x00)), 3);

      var nodeHighlightStyle = new NodeStyleIndicatorRenderer {
          // We choose a shape node style
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.Rectangle,
              Pen = highlightPen,
              Brush = Brushes.Transparent
          },
          // With padding for the decoration
          Margins = new InsetsD(7)
      };

      var edgeHighlightStyle = new EdgeStyleIndicatorRenderer {
          // Use a thick line for the edge highlight.
          EdgeStyle = new PolylineEdgeStyle { Pen = highlightPen }
      };
      graph.GetDecorator().Nodes.HighlightRenderer.AddConstant(nodeHighlightStyle);
      graph.GetDecorator().Edges.HighlightRenderer.AddConstant(edgeHighlightStyle);
    }

    /// <summary>
    ///   Initializes the input mode for this component.
    /// </summary>
    private void InitializeInputMode() {
      var inputMode = new GraphViewerInputMode { ClickableItems = GraphItemTypes.Node | GraphItemTypes.Edge };
      // Implements the smart click navigation
      inputMode.ItemLeftClicked += async (_, evt) => {
        // Zooms to the suitable point
        await ZoomToLocation(evt.Item, evt.Location);
        // Highlights the concerned objects (node or edge with target and source node)
        UpdateHighlight(evt.Item);
      };
      graphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Zooms to the suitable point.
    /// </summary>
    /// <param name="item">The element that we clicked.</param>
    /// <param name="currentMouseClickLocation">The world-coordinate click location.</param>
    private async Task ZoomToLocation(IModelItem item, PointD currentMouseClickLocation) {
      // Get the point to where we should zoom in
      var location = GetFocusPoint(item);

      // Check the type of zooming
      if (GraphChooserBox.SelectedIndex == 1) {
        // The distance between where we clicked and the viewport center
        var offset = PointD.Subtract(currentMouseClickLocation, graphControl.Center);
        // Zooms to the new location of the mouse
        await graphControl.ZoomToAnimated(graphControl.Zoom, PointD.Subtract(location, offset));
      } else {
        await graphControl.ZoomToAnimated(graphControl.Zoom, location);
      }
    }

    /// <summary>
    ///   Gets the focus point.
    /// </summary>
    /// <param name="item">The clicked graph item.</param>
    /// <returns>The point that we should zoom to.</returns>
    private PointD GetFocusPoint(IModelItem item) {
      if (item is IEdge edge) {
        // If the source and target node are in the viewport, then zoom to the middle of the edge.
        var targetNodeCenter = edge.GetTargetNode().Layout.GetCenter();
        var sourceNodeCenter = edge.GetSourceNode().Layout.GetCenter();
        var viewport = graphControl.Viewport;
        if (viewport.Contains(targetNodeCenter) && viewport.Contains(sourceNodeCenter)) {
          return new PointD((sourceNodeCenter.X + targetNodeCenter.X) / 2,
              (sourceNodeCenter.Y + targetNodeCenter.Y) / 2);
        }
        if (PointD.Distance(graphControl.Center, targetNodeCenter) <
            PointD.Distance(graphControl.Center, sourceNodeCenter)) {
          // If the source node is outside the viewport, then zoom to it.
          return sourceNodeCenter;
        }

        // Otherwise zoom to the target node.
        return targetNodeCenter;
      }

      if (item is INode node) {
        return node.Layout.GetCenter();
      }

      // Since the clickable items are only nodes and edges, we should never reach this point.
      return PointD.Origin;
    }

    /// <summary>
    ///   Highlights the clicked item and its connected graph items briefly.
    /// </summary>
    private void UpdateHighlight(IModelItem item) {
      var highlights = graphControl.Highlights;
      if (item is IEdge edge) {
        highlights.Add(edge);
        highlights.Add(edge.GetSourceNode());
        highlights.Add(edge.GetTargetNode());
      } else if (item is INode node) {
        highlights.Add(node);
      }

      // clear highlights after one second
      _ = ClearHighlightsLater();
    }

    private async Task ClearHighlightsLater() {
      await Task.Delay(TimeSpan.FromSeconds(1));
      graphControl.Highlights.Clear();
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // Set styles for this demo
      DemoStyles.InitDemoStyles(graph);
    }

    [STAThread]
    private static void Main() {
      Run<SmartClickNavigationDemo>();
    }
  }
}
