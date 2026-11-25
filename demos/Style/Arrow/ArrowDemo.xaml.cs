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
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Arrow
{
  /// <summary>
  ///   Shows all arrow types of the <see cref="Arrow"/> class.
  /// </summary>
  [Demo("arrow", "style", "Arrow", Summary = "Shows all arrow types of the Arrow class.", Tags = ["arrows", "edges"],
      Keywords = [
          "arrow type", "arrowtype", "chevron", "cross", "deltoid", "diamond", "ellipse", "kite", "stealth", "triangle"
      ])]
  public sealed partial class ArrowDemo
  {
    private readonly Settings settings = new();

    public ArrowDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // create nodes, edges and arrows to demonstrate all arrow types
      InitializeGraph();

      InitializeUI();
      InitializeInteraction();
      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a new edge style with the provided arrow type and color.
    /// </summary>
    private static PolylineEdgeStyle CreateEdgeStyleWithArrow(ArrowType arrowType, Palette colorSet) {
      // create a new arrow
      var arrow = new Graph.Styles.Arrow(arrowType, pen: new Pen(colorSet.Fill, 4), lengthScale: 2, widthScale: 2);

      // create a new edge style with the above arrow
      return new PolylineEdgeStyle {
          Pen = new Pen(colorSet.Fill, 4),
          TargetArrow = arrow
      };
    }

    /// <summary>
    ///   Creates nodes, edges, and arrows to demonstrate all arrow types.
    /// </summary>
    private void InitializeGraph() {
      var graph = Graph;
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Brush = Themes.PaletteBlue.EdgeLabelFill,
          Pen = null
      };

      // all available arrows types
      var arrowTypes = new[] {
          ArrowType.Chevron,
          ArrowType.Cross,
          ArrowType.Deltoid,
          ArrowType.Diamond,
          ArrowType.Ellipse,
          ArrowType.Kite,
          ArrowType.Open,
          ArrowType.Stealth,
          ArrowType.Triangle
      };

      var colorSets = new[] {
          Themes.PaletteOrange,
          Themes.PaletteBlue,
          Themes.PaletteRed,
          Themes.PaletteGreen,
          Themes.PalettePurple,
          Themes.PaletteLightblue,
          Themes.Palette22,
          Themes.Palette12,
          Themes.Palette14
      };

      /* iterate over all available arrows types and create arrows */
      var x = 0.0;
      var y = 0.0;
      var i = 1;
      INode? currNode = null;
      var width = 80.0;
      var height = 80.0;
      for (var arrowIndex = arrowTypes.Length; arrowIndex > 0;) {
        var lastNode = currNode;
        currNode = graph.CreateNode(new RectD(x, y, width, height));
        // create an edge with the current arrow type
        if (i % 4 != 1) {
          var arrowType = arrowTypes[--arrowIndex];
          var color = colorSets[arrowIndex];
          // create edge style with the current arrow type
          var edge = graph.CreateEdge(lastNode!, currNode, CreateEdgeStyleWithArrow(arrowType, color));
          // add arrow type text as edge label
          graph.AddLabel(edge, arrowType.ToString().ToLowerInvariant(),
              FreeEdgeLabelModel.Instance.CreateParameter(distance: 40), DemoStyles.CreateDemoEdgeLabelStyle(color));
        }
        // display 4 nodes (3 edges) in every row
        if (i % 4 == 0) {
          x = 0.0;
          y += height + 150.0;
        } else {
          x += width + 150.0;
        }
        i++;
      }
    }

    /// <summary>
    ///   Updates the edge thickness and arrow scale of all edges in the graph.
    /// </summary>
    private void UpdateEdgeThickness(double thickness) {
      foreach (var edge in Graph.Edges) {
        // obtain the current edge style and target arrow
        if (edge.Style is PolylineEdgeStyle { TargetArrow: Graph.Styles.Arrow arrow } edgeStyle) {
          // update edge thickness, arrow stroke and scale
          Graph.SetStyle(edge, new PolylineEdgeStyle {
              Pen = new Pen(edgeStyle.Pen!.Brush, thickness),
              TargetArrow = new Graph.Styles.Arrow(arrow.Type, arrow.Brush, new Pen(arrow.Pen!.Brush, thickness),
                  Math.Max(thickness / 2, 1), Math.Max(thickness / 2, 1))
          });
        }
      }
    }

    /// <summary>
    ///   Updates the fill of all arrows in the graph.
    /// </summary>
    private void UpdateArrowStyle(ArrowFillStyle fillStyle) {
      foreach (var edge in Graph.Edges) {
        // obtain the current edge style, stroke and target arrow
        if (edge.Style is PolylineEdgeStyle { TargetArrow: Graph.Styles.Arrow arrow } edgeStyle) {
          var pen = edgeStyle.Pen!;
          // update the arrow fill
          Graph.SetStyle(edge, new PolylineEdgeStyle {
              Pen = pen,
              TargetArrow = new Graph.Styles.Arrow(arrow.Type, fillStyle == ArrowFillStyle.Filled ? pen.Brush : null,
                  pen,
                  arrow.LengthScale, arrow.WidthScale)
          });
        }
      }
    }

    /// <summary>
    ///   Registers event listeners to the toolbar editor.
    /// </summary>
    private void InitializeUI() {
      ThicknessSlider.DataContext = settings;
      ThicknessLabel.DataContext = settings;
      ArrowStyleSelector.DataContext = settings;
      settings.SettingChanged += (_, _) => {
        UpdateEdgeThickness(settings.Thickness);
        UpdateArrowStyle(settings.FillStyle);
      };
    }

    /// <summary>
    ///   Sets up an input mode that assigns a random existing edge style to newly created edges.
    /// </summary>
    private void InitializeInteraction() {
      var inputMode = new GraphEditorInputMode {
          AllowCreateNode = false,
          SelectableItems = GraphItemTypes.None,
          FocusableItems = GraphItemTypes.None,
          MovableUnselectedItems = GraphItemTypes.None
      };
      inputMode.CreateEdgeInputMode.EdgeCreationStarted += (_, evt) => {
        var edges = Graph.Edges;
        if (edges.Count > 0) {
          var randomEdgeStyle = edges[Random.Shared.Next(edges.Count)].Style;
          inputMode.CreateEdgeInputMode.PreviewGraph.SetStyle(evt.Item, randomEdgeStyle);
        }
      };
      GraphControl.InputMode = inputMode;
    }

    [STAThread]
    public static void Main() {
      Run<ArrowDemo>();
    }
  }
}
