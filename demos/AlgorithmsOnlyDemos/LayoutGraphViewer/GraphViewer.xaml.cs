/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Size = System.Windows.Size;


namespace Demo.Layout.LayoutGraphViewer
{
  /// <summary>
  /// Interaction logic for GraphViewer.xaml
  /// </summary>
  public partial class GraphViewer : Window
  {
    public GraphViewer() {
      InitializeComponent();
    }

    public void AddLayoutGraph(LayoutGraph graph, string hierarchicalLayoutStyle) {
      TabItem tab = new TabItem { Header = hierarchicalLayoutStyle};
      tab.Content = new GraphCanvas(graph);
      TC_Graphs.Items.Add(tab);
    }

    /// <summary>
    /// Helper class that displays a LayoutGraph
    /// </summary>
    class GraphCanvas : Canvas
    {
      private int _padding = 50;
      private double scale = 1;
      public double Scale {
        get {return scale;}
        set {
          scale = value;

          var scaleTranform = new ScaleTransform() {
            ScaleX = value,
            ScaleY = value
          };
          var translateTransform = new TranslateTransform(_padding, _padding);

          var transformGroup = new TransformGroup();
          transformGroup.Children.Add(scaleTranform);
          transformGroup.Children.Add(translateTransform);
          this.RenderTransform = transformGroup;
        }
      }

      public GraphCanvas(LayoutGraph graph) {
        this.RenderTransform = new TranslateTransform(_padding,_padding);
        var grouping = new GroupingSupport(graph);

        // Add all edges
        foreach (var edge in graph.Edges) {
          IEdgeLayout el = graph.GetLayout(edge);
          var l = new Polyline();
          l.Stroke = Brushes.Black;
          l.Points.Add(new Point(graph.GetSourcePointAbs(edge).X, graph.GetSourcePointAbs(edge).Y));
          for (int i = 0; i < el.PointCount(); i++) {
            Point p = new Point(el.GetPoint(i).X, el.GetPoint(i).Y);
            l.Points.Add(p);
          }
          l.Points.Add(new Point(graph.GetTargetPointAbs(edge).X, graph.GetTargetPointAbs(edge).Y));
          this.Children.Add(l);

          // edge labels
          var edgeLabelLayout = graph.GetLabelLayout(edge);
          foreach (var labelLayout in edgeLabelLayout) {
            var orientedRectangle = labelLayout.LabelModel.GetLabelPlacement(
                labelLayout.BoundingBox,
                graph.GetLayout(edge),
                graph.GetLayout(edge.Source),
                graph.GetLayout(edge.Target),
                labelLayout.ModelParameter);
            this.Children.Add(GetPolygon(orientedRectangle));

          }
        }

        // add all nodes
        foreach (var node in graph.Nodes) {
          INodeLayout nl = graph.GetLayout(node);
          Color color = grouping.IsGroupNode(node) ? Color.FromArgb(60, 255, 60, 0) : Color.FromArgb(255, 255, 255, 0);


          var rect = new Rectangle();
          this.Children.Add(rect);
          rect.Stroke = new SolidColorBrush() {Color = Colors.Black};
          rect.Fill = new SolidColorBrush() { Color = color };
          rect.Width = nl.Width;
          rect.Height = nl.Height;
          Canvas.SetTop(rect, nl.Y);
          Canvas.SetLeft(rect, nl.X);

          // display the node index 
          var text = new TextBlock() {
              Text = String.Empty + node.Index,
              HorizontalAlignment = HorizontalAlignment.Center,
              VerticalAlignment = VerticalAlignment.Center
          }; 
          
          this.Children.Add(text);
          text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
          Canvas.SetTop(text, nl.Y + nl.Height/2 - text.DesiredSize.Height/2);
          Canvas.SetLeft(text, nl.X + nl.Width/2 - text.DesiredSize.Width/2);
        }
      }
    }

    private static Polygon GetPolygon(YOrientedRectangle orientedRectangle) {
      var poly = new Polygon();
      poly.Stroke = Brushes.Red;
      poly.Points.Add(new Point((float) orientedRectangle.AnchorX, (float) orientedRectangle.AnchorY));
      poly.Points.Add(new Point((float) (orientedRectangle.AnchorX + orientedRectangle.Height * orientedRectangle.UpX), (float) (orientedRectangle.AnchorY + orientedRectangle.Height * orientedRectangle.UpY)));
      poly.Points.Add(new Point((float) (orientedRectangle.AnchorX + orientedRectangle.Height * orientedRectangle.UpX + orientedRectangle.Width * -orientedRectangle.UpY), (float) (orientedRectangle.AnchorY + orientedRectangle.Height * orientedRectangle.UpY + orientedRectangle.Width * orientedRectangle.UpX)));
      poly.Points.Add(new Point((float) (orientedRectangle.AnchorX + orientedRectangle.Width * -orientedRectangle.UpY), (float) (orientedRectangle.AnchorY + orientedRectangle.Width * orientedRectangle.UpX)));
      return poly;
    }

    private void Btn_zoomIn(object sender, RoutedEventArgs e) {
      var graphCanvas = TC_Graphs.SelectedContent as GraphCanvas;
      graphCanvas.Scale += 0.2d;
    }

    private void Btn_zoomOut(object sender, RoutedEventArgs e) {
      var graphCanvas = TC_Graphs.SelectedContent as GraphCanvas;
      graphCanvas.Scale -= 0.2d;
    }

    private void Btn_actualSize(object sender, RoutedEventArgs e) {
      var graphCanvas = TC_Graphs.SelectedContent as GraphCanvas;
      graphCanvas.Scale =1.0d;
    }

    private void MenuItemSave_OnClick(object sender, RoutedEventArgs e) {
      SaveFileDialog saveFileDialog = new SaveFileDialog();

      saveFileDialog.Filter = "PNG File|*.png";

      saveFileDialog.Title = "Save as PNG";

      saveFileDialog.ShowDialog();

      // If the file name is not an empty string open it for saving.
      if (saveFileDialog.FileName != "") {
        var canvas = (Canvas) TC_Graphs.SelectedContent;

        var bounds = VisualTreeHelper.GetDescendantBounds(canvas);
        var dpi = 96d;

        var rtb = new RenderTargetBitmap((int) bounds.Width, (int) bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);

        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen()) {
          var vb = new VisualBrush(canvas);
          dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        }

        rtb.Render(dv);
        BitmapEncoder pngEncoder = new PngBitmapEncoder();
        pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

        using (var fs = System.IO.File.OpenWrite(saveFileDialog.FileName)) {
          pngEncoder.Save(fs);
        }
      }
    }
    private void MenuItemExit_OnClick(object sender, RoutedEventArgs e) {
      this.Close();
    }
  }
}
