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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.BoundaryLabeling
{
  /// <summary>
  ///   Interaction logic for BoundaryLabelingDemo.xaml
  /// </summary>
  [Demo("boundary-labeling", "layout", "Boundary Labeling",
      Summary = "Shows how to configure organic layout for annotating points on a diagram.",
      Tags = ["layout", "organic", "constraint"], Keywords = [
          "layoutfeatures", "align", "horizontal", "vertical", "labeling", "boundary", "map", "technical drawing",
          "image"
      ])]
  public sealed partial class BoundaryLabelingDemo
  {
    private static readonly RectD imageRect = new(0, 0, 350, 477);

    public BoundaryLabelingDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      var graph = graphControl.Graph;

      // configure user interaction, disable selection and focus
      graphControl.InputMode = new GraphViewerInputMode();

      // assign the default style and size for the points
      InitializeDefaultStyles(graph);

      // read the points from the dataset and create the associated label nodes
      InitializeGraph(graph);

      // add the image to be annotated to the demo's graph control
      AddBackgroundImage(graphControl);

      // configure and run OrganicLayout with constraints for aligning points and their associated labels
      await RunLayoutAsync();

      InitializeUI();
    }

    /// <summary>
    ///   Creates the graph from the given dataset.
    ///   For each data item in the dataset, a point node and an associated label node are created.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      foreach (var data in PointsData.Points) {
        // create the point node with the x,y coordinates given in the dataset and the default node size
        var point = graph.CreateNode(
            new RectD(data.Location.X, data.Location.Y, graph.NodeDefaults.Size.Width, graph.NodeDefaults.Size.Height),
            tag: data);

        // create the label node and define its tag based on the tag of the associated point
        var labelNode = graph.CreateNode(point.Layout.ToRectD(), VoidNodeStyle.Instance,
            data with { Type = NodeType.Label });

        // add the label to the new label node and use a LabelStyle for its display
        graph.AddLabel(labelNode, GetPlainLabelText(data.Label), ExteriorNodeLabelModel.Top, GetLabelStyle(data.Label));

        // ... create an edge between the point and the label node
        graph.CreateEdge(point, labelNode);
      }
    }

    /// <summary>
    ///   Configures the default node size and style as well as the default style for the edges for this demo.
    /// </summary>
    private static void InitializeDefaultStyles(IGraph graph) {
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Rectangle,
          Brush = Brushes.Black
      };
      graph.NodeDefaults.Size = new SizeD(3, 3);
      graph.NodeDefaults.Labels.Style = GetLabelStyle(string.Empty);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle();
    }

    /// <summary>
    ///   Adds the image to the background group of the given graph control.
    /// </summary>
    private void AddBackgroundImage(GraphControl graphComponent) {
      using var stream = LoadAssemblyResource("earth-layers.png");
      var image = new BitmapImage();
      image.BeginInit();
      image.CacheOption = BitmapCacheOption.OnLoad;
      image.StreamSource = stream;
      image.EndInit();
      image.Freeze();

      graphComponent.RenderTree.CreateElement(graphComponent.RenderTree.BackgroundGroup,
          new ImageVisualCreator(imageRect, image));
    }

    /// <summary>
    ///   Configures and runs an OrganicLayout with the constraints for aligning points and their associated labels.
    /// </summary>
    private async Task RunLayoutAsync(bool animated = false) {
      var (layout, layoutData) = ConfigureLayout.Create(graphControl.Graph, imageRect);
      if (animated) {
        await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(200), layoutData);
      } else {
        graphControl.Graph.ApplyLayout(layout, layoutData);
        await graphControl.FitGraphBounds();
      }
    }

    /// <summary>
    ///   Binds the action to the toolbar slider.
    /// </summary>
    private void InitializeUI() {
      fontSizeSlider.ValueChanged += OnFontSizeChanged;
      fontSizeSlider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnFontSizeDragCompleted));
    }

    private async void OnFontSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (!IsLoaded) {
        return;
      }

      var fontSize = (int) e.NewValue;
      fontSizeLabel.Content = fontSize.ToString();
      var graph = graphControl.Graph;

      foreach (var label in graph.Labels) {
        var style = (LabelStyle) label.Style.Clone();
        style.TextSize = fontSize;
        graph.SetStyle(label, style);
      }

      if (!fontSizeSlider.IsMouseCaptureWithin) {
        await RunLayoutAsync(true);
      }
    }

    private async void OnFontSizeDragCompleted(object sender, DragCompletedEventArgs e) {
      if (IsLoaded) {
        await RunLayoutAsync(true);
      }
    }

    /// <summary>
    ///   Returns a new LabelStyle with the given font size and formatting from the source label text.
    /// </summary>
    private static LabelStyle GetLabelStyle(string text, int fontSize = 12) {
      var isBold = text.Contains("font-weight: bold", StringComparison.OrdinalIgnoreCase);
      var isCentered = text.Contains("text-align: center", StringComparison.OrdinalIgnoreCase);
      return new LabelStyle {
          Typeface =
              new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal,
                  isBold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal),
          TextSize = fontSize,
          TextWrapping = TextWrapping.Wrap,
          TextTrimming = TextTrimming.CharacterEllipsis,
          TextAlignment = isCentered ? TextAlignment.Center : TextAlignment.Left,
          MaximumSize = new SizeD(400, 100),
          Padding = new InsetsD(4),
          BackgroundPen = new Pen(Brushes.Black, 1),
          BackgroundBrush = Brushes.White
      };
    }

    private static string GetPlainLabelText(string text) =>
        WebUtility.HtmlDecode(Regex.Replace(text, "<[^>]+>", string.Empty));

    [STAThread]
    private static void Main() => Run<BoundaryLabelingDemo>();
  }
}
