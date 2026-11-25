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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SimpleHighlightDecorator
{
  /// <summary>
  ///   This demo shows how to highlight a node or an edge when the mouse hovers over it.
  /// </summary>
  [Demo("simple-highlight", "application-features", "Simple Highlight Decorator",
      Summary = "Shows how to highlight nodes and edges when the mouse hovers over them.",
      Tags = ["highlight", "hover", "interaction"],
      Keywords = ["GraphDecorator", "NodeDecorator", "HighlightDecorator", "playground", "margin"])]
  public sealed class SimpleHighlightDecoratorDemo : SimpleDemo
  {
    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // Enable hover effects on nodes and edges
      var inputMode = new GraphEditorInputMode();
      inputMode.ItemHoverInputMode.Enabled = true;
      inputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Node | GraphItemTypes.Edge;

      // Specify hover effect: the hovered item is added to the graph's highlight collection
      inputMode.ItemHoverInputMode.HoveredItemChanged += (_, args) => {
        GraphControl.Highlights.Clear();
        if (args.Item != null) {
          GraphControl.Highlights.Add(args.Item);
        }
      };
      GraphControl.InputMode = inputMode;

      // Define the visual styles for node and edge highlights
      var highlightBrush = (Brush) new SolidColorBrush(Color.FromRgb(0x64, 0xa8, 0xbe)).GetAsFrozen();
      var highlightPen = (Pen) new Pen(highlightBrush, 3).GetAsFrozen();
      Graph.GetDecorator().Nodes.HighlightRenderer.AddConstant(new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              Shape = ShapeNodeShape.Rectangle,
              Pen = highlightPen,
              Brush = Brushes.Transparent
          },
          // Padding between the actual node and its highlight visualization
          Margins = new InsetsD(4)
      });
      Graph.GetDecorator().Edges.HighlightRenderer.AddConstant(new EdgeStyleIndicatorRenderer {
          EdgeStyle = new PolylineEdgeStyle {
              SmoothingLength = 50,
              TargetArrow = new Arrow(ArrowType.Triangle, highlightBrush,
                  (Pen) new Pen(highlightBrush, 2).GetAsFrozen()),
              Pen = highlightPen
          }
      });

      // Populate the graph with data from the JSON dataset and apply layout
      BuildGraphFromJson("graph-data.json");
      Graph.ApplyLayout(new HierarchicalLayout());
      GraphControl.FitGraphBounds();
    }

    [STAThread]
    public static void Main() => Run<SimpleHighlightDecoratorDemo>();
  }
}
