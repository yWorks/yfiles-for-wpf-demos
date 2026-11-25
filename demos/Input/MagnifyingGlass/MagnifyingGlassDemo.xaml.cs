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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;

namespace yWorks.Demo.MagnifyingGlass
{
  /// <summary>
  ///   Shows a floating magnifying glass that magnifies the cursor's surroundings.
  /// </summary>
  [Demo("magnifying-glass", "input", "Magnifying Glass",
      Summary = "Shows a floating lens that magnifies the cursor's surroundings.", Tags = ["interaction", "input"],
      Keywords = ["magnifier", "lens", "zoom", "input mode"])]
  public partial class MagnifyingGlassDemo
  {
    private LensInputMode lensInputMode = null!;

    public MagnifyingGlassDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      var graphEditorInputMode = new GraphEditorInputMode {
          // Some configurations for a better user experience in this demo.
          // All these settings can be changed.
          FocusableItems = GraphItemTypes.None,
          ShowHandleItems = GraphItemTypes.Bend,
          AllowCreateNode = false
      };

      // Create the input mode that implements the magnifying glass and add it to the input mode
      // of the graph component
      lensInputMode = new LensInputMode();
      graphEditorInputMode.Add(lensInputMode);
      GraphControl.InputMode = graphEditorInputMode;

      // Decrease the zoom of the graphControl to make sure the magnifying glass is visible
      GraphControl.Zoom = 0.5;

      DemoStyles.InitDemoStyles(Graph, edgeTheme: Themes.PaletteGreen, extraCropLength: 0);
      PopulateGraph(Graph);
      GraphControl.FitGraphBounds();

      InitializeUI();
    }

    /// <summary>
    ///   Creates the sample graph.
    /// </summary>
    /// <param name="graph">The graph of the graphControl</param>
    private void PopulateGraph(IGraph graph) {
      // Set default graph item styling
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(Themes.PaletteGreen.Fill, 5) {
              StartLineCap = PenLineCap.Round,
              EndLineCap = PenLineCap.Round
          }
      };
      graph.GetDecorator().Ports.EdgePathCropper.AddConstant(new EdgePathCropper { ExtraCropLength = 10.0 });

      // Create shared image node styles
      var deviceStyles = CreateDeviceStyles();

      // Build the graph
      var data = LoadGraphData();
      var graphBuilder = new GraphBuilder(graph);
      var nodesSource = graphBuilder.CreateNodesSource(data.NodeList, node => node.Data.Id);
      nodesSource.NodeCreator.LayoutProvider = node =>
          RectD.FromCenter(new PointD(node.Layout.X, node.Layout.Y), graph.NodeDefaults.Size);
      nodesSource.NodeCreator.StyleProvider = node => deviceStyles[node.Data.Type];
      nodesSource.NodeCreator.TagProvider = node => node.Data;

      graphBuilder.CreateEdgesSource(data.EdgeList, edge => edge.Source, edge => edge.Target);
      graphBuilder.BuildGraph();
    }

    /// <summary>
    ///   Loads and deserializes the sample graph data embedded as <c>network-sample.json</c>.
    /// </summary>
    private GraphData LoadGraphData() {
      using var stream = LoadAssemblyResource("network-sample.json");
      return JsonSerializer.Deserialize<GraphData>(stream,
              new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
          throw new InvalidDataException("The embedded network sample is empty.");
    }

    /// <summary>
    ///   Creates shared image node styles for each kind of network device.
    /// </summary>
    private Dictionary<string, ImageNodeStyle> CreateDeviceStyles() {
      var iconTypes = new[] { "database", "notebook", "server", "smartphone", "switch", "wlan", "workstation" };
      var sprite = LoadDeviceIcons();
      return iconTypes.Select((type, index) => {
        var image = new CroppedBitmap(sprite, new Int32Rect(index * 70, 0, 70, 70));
        image.Freeze();
        return new KeyValuePair<string, ImageNodeStyle>(type,
            new ImageNodeStyle(image) { OutlineShape = new Ellipse() });
      }).ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
    }

    /// <summary>
    ///   Loads the embedded sprite containing the sample's device icons.
    /// </summary>
    private BitmapSource LoadDeviceIcons() {
      using var stream = LoadAssemblyResource("network-icons.png");
      var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
      var sprite = decoder.Frames[0];
      sprite.Freeze();
      return sprite;
    }

    /// <summary>
    ///   Initializes the UI.
    /// </summary>
    private void InitializeUI() {
      GraphControl.ZoomChanged += (_, _) => { zoomLevelLabel.Content = GraphControl.Zoom.ToString("0.00"); };
      zoomLevelLabel.Content = GraphControl.Zoom.ToString("0.00");
      LensZoomSelector.SelectedIndex = 1;
    }

    private void LensZoomSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (lensInputMode == null || LensZoomSelector.SelectedItem is not ComboBoxItem item ||
          !double.TryParse(item.Tag?.ToString(), out var zoomFactor)) {
        return;
      }
      lensInputMode.ZoomFactor = zoomFactor;
    }

    [STAThread]
    public static void Main() {
      Run<MagnifyingGlassDemo>();
    }
  }

  public sealed class GraphData
  {
    public List<NodeObject> NodeList { get; set; } = [];

    public List<EdgeData> EdgeList { get; set; } = [];
  }

  public sealed class NodeObject
  {
    public NodeData Data { get; set; } = new();

    public NodeLayout Layout { get; set; } = new();
  }

  public sealed class NodeData
  {
    public string Name { get; set; } = string.Empty;

    public string Ip { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Id { get; set; }
  }

  public sealed class NodeLayout
  {
    public double X { get; set; }

    public double Y { get; set; }
  }

  public sealed class EdgeData
  {
    public int Source { get; set; }

    public int Target { get; set; }
  }
}
