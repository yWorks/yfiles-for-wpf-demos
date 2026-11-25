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
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;

namespace yWorks.Demo.ArrowEdgeStyle
{
  /// <summary>
  ///   Shows the various rendering options of <see cref="ArrowEdgeStyle"/>.
  /// </summary>
  [Demo("arrow-edge-style", "style", "Arrow Edge Style",
      Summary = "Shows the various rendering options of the ArrowEdgeStyle.")]
  public partial class ArrowEdgeStyleDemo
  {
    private readonly EdgeStyleSettings settings = new();

    public ArrowEdgeStyleDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      var graphEditorInputMode = new GraphEditorInputMode {
          MarqueeSelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge
      };
      // Creating new edges with bends is disabled in this demo since ArrowEdgeStyle always draws
      // straight-line shafts and ignores bends. Note that ArrowEdgeStyle prevents the creation of
      // bends for existing edges that use it, so we just need to disable it for edge creation.
      graphEditorInputMode.CreateEdgeInputMode.AllowCreateBend = false;
      GraphControl.InputMode = graphEditorInputMode;

      // Initialize style defaults
      DemoStyles.InitDemoStyles(Graph, Themes.Palette58, extraCropLength: 0.0);
      Graph.EdgeDefaults.Style = CreateStyleFromSettings(Themes.PaletteLightblue);
      Graph.EdgeDefaults.ShareStyleInstance = false;

      // Create the sample graph
      CreateSampleGraph(Graph);

      // Initialize the style property editor
      ShapeComboBox.ItemsSource = Enum.GetValues(typeof(ArrowStyleShape));
      StylePropertiesPanel.DataContext = settings;
      settings.SettingChanged += (_, _) => ApplyStyleSetting();

      // adjust the property editor when the selection has been changed
      GraphControl.Selection.ItemAdded += (_, e) => {
        if (e.Item is IEdge edge && edge.Style is Graph.Styles.ArrowEdgeStyle style) {
          settings.UpdateFrom(style, true);
          Graph.EdgeDefaults.Style = CreateStyleFromSettings(Themes.PaletteLightblue);
        }
      };

      GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates a sample graph with different style settings, read from an embedded JSON resource.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      var data = LoadGraphData();

      var builder = new GraphBuilder(graph);

      var nodesSource = builder.CreateNodesSource(data.Nodes, node => node.Id);
      nodesSource.NodeCreator.LayoutProvider = node =>
          new RectD(node.Layout.X, node.Layout.Y, node.Layout.Width, node.Layout.Height);

      var edgesSource = builder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target);
      edgesSource.EdgeCreator.StyleProvider = edge =>
          edge.Style != null ? CreateStyleFromEdgeData(edge.Style) : CreateStyleFromSettings(Themes.PaletteLightblue);

      builder.BuildGraph();
    }

    /// <summary>
    ///   Loads and deserializes the sample graph data embedded as <c>graph-data.json</c>.
    /// </summary>
    private GraphData LoadGraphData() => JsonSerializer.Deserialize<GraphData>(LoadAssemblyResource("graph-data.json"));

    /// <summary>
    ///   Creates an <see cref="ArrowEdgeStyle"/> from the style settings stored for a sample edge.
    /// </summary>
    /// <remarks>
    ///   The sample data has no per-edge cropping value, so - just like the original demo - the cropping currently
    ///   shown in the style property editor is used for every edge of the sample graph.
    /// </remarks>
    private Graph.Styles.ArrowEdgeStyle CreateStyleFromEdgeData(EdgeStyleData data) {
      var palette = Themes.ForName(data.Color) ?? Themes.PaletteLightblue;
      return new Graph.Styles.ArrowEdgeStyle {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1),
          Shape = ParseShape(data.Shape),
          Thickness = data.Thickness,
          Angle = EdgeStyleSettings.ToRadians(data.Angle),
          ShaftRatio = data.ShaftRatio,
          SourceCropping = settings.Cropping,
          TargetCropping = settings.Cropping
      };
    }

    /// <summary>
    ///   Creates an <see cref="ArrowEdgeStyle"/> for the current settings of the style property editor.
    /// </summary>
    private Graph.Styles.ArrowEdgeStyle CreateStyleFromSettings(Palette palette) {
      var style = new Graph.Styles.ArrowEdgeStyle {
          Brush = palette.Fill,
          Pen = new Pen(palette.Stroke, 1)
      };
      settings.ApplyTo(style);
      return style;
    }

    /// <summary>
    ///   Applies changes in the style property editor to the selected edges and the default style.
    /// </summary>
    private void ApplyStyleSetting() {
      foreach (var edge in GraphControl.Selection.Edges) {
        if (edge.Style is Graph.Styles.ArrowEdgeStyle style) {
          settings.ApplyTo(style);
        }
      }

      // adjust also the default style applied to newly created edges
      if (Graph.EdgeDefaults.Style is Graph.Styles.ArrowEdgeStyle defaultStyle) {
        settings.ApplyTo(defaultStyle);
      }

      GraphControl.Invalidate();
    }

    /// <summary>
    ///   Parses a kebab-case shape name from the sample JSON data (e.g. <c>"double-arrow"</c>) into an
    ///   <see cref="ArrowStyleShape"/> value using <see cref="Enum.Parse{TEnum}(string,bool)"/>.
    /// </summary>
    private static ArrowStyleShape ParseShape(string value) =>
        Enum.Parse<ArrowStyleShape>(value.Replace("-", ""), true);

    [STAThread]
    public static void Main() {
      Run<ArrowEdgeStyleDemo>();
    }
  }
}
