/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using yWorks.Geometry;
using yWorks.Layout.Graph;

namespace Demo.Layout.LayoutGraphViewer
{
  /// <summary>
  ///   Interaction logic for GraphViewer.xaml
  /// </summary>
  public partial class GraphViewer : Window
  {
    public GraphViewer() {
      InitializeComponent();
    }

    public void AddLayoutGraph(LayoutGraph graph, string hierarchicalLayoutStyle) {
      var tab = new TabItem { Header = hierarchicalLayoutStyle };
      tab.Content = new GraphCanvas(graph);
      TC_Graphs.Items.Add(tab);
    }

    /// <summary>
    ///   Helper class that displays a LayoutGraph
    /// </summary>
    private class GraphCanvas : Canvas
    {
      private static readonly Brush NodeFill = (Brush) new SolidColorBrush(Color.FromRgb(255, 108, 0)).GetAsFrozen();
      private static readonly Brush NodeStroke = (Brush) new SolidColorBrush(Color.FromRgb(102, 43, 0)).GetAsFrozen();

      private static readonly Brush GroupNodeStroke =
          (Brush) new SolidColorBrush(Color.FromRgb(19, 112, 136)).GetAsFrozen();

      private static readonly Brush LabelFill = (Brush) new SolidColorBrush(Color.FromRgb(255, 195, 152)).GetAsFrozen();
      private static readonly Brush EdgeStroke = (Brush) new SolidColorBrush(Color.FromRgb(102, 47, 1)).GetAsFrozen();

      private readonly int _padding = 50;
      private double scale = 1;

      public double Scale {
        get => scale;
        set {
          scale = value;

          var scaleTransform = new ScaleTransform { ScaleX = value, ScaleY = value };
          var translateTransform = new TranslateTransform(_padding, _padding);

          var transformGroup = new TransformGroup();
          transformGroup.Children.Add(scaleTransform);
          transformGroup.Children.Add(translateTransform);
          RenderTransform = transformGroup;
        }
      }

      public GraphCanvas(LayoutGraph graph) {
        RenderTransform = new TranslateTransform(_padding, _padding);
        var grouping = LayoutGraphGrouping.CreateReadOnlyView(graph);

        // Add all edges
        foreach (var edge in graph.Edges) {
          var l = new Polyline { Stroke = EdgeStroke, StrokeThickness = 1.5 };
          l.Points.Add(new Point(edge.SourcePortLocation.X, edge.SourcePortLocation.Y));
          foreach (var edgeBend in edge.Bends) {
            l.Points.Add(new Point(edgeBend.X, edgeBend.Y));
          }
          l.Points.Add(new Point(edge.TargetPortLocation.X, edge.TargetPortLocation.Y));
          Children.Add(l);
          Children.Add(GetArrowhead(l));

          // edge labels
          foreach (var label in edge.Labels) {
            Children.Add(GetPolygon(label.Layout));
          }
        }

        // add all nodes
        foreach (var node in graph.Nodes) {
          var nl = node.Layout;

          var isGroupNode = grouping.IsGroupNode(node);
          var stroke = isGroupNode ? GroupNodeStroke : NodeStroke;
          var rect = new Rectangle {
              RadiusX = 3.5,
              RadiusY = 3.5,
              StrokeThickness = isGroupNode ? 5 : 1.5,
              Stroke = stroke,
              Fill = isGroupNode ? null : NodeFill,
              Width = nl.Width,
              Height = nl.Height
          };
          Children.Add(rect);
          SetTop(rect, nl.Y);
          SetLeft(rect, nl.X);

          // display the node index
          var text = new TextBlock {
              Text = string.Empty + node.Index,
              HorizontalAlignment = HorizontalAlignment.Center,
              VerticalAlignment = VerticalAlignment.Center,
              Foreground = stroke
          };

          Children.Add(text);
          text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
          SetTop(text, nl.Y + nl.Height / 2 - text.DesiredSize.Height / 2);
          SetLeft(text, nl.X + nl.Width / 2 - text.DesiredSize.Width / 2);
        }
      }

      private Polygon GetArrowhead(Polyline l) {
        var lastPoint = l.Points[l.Points.Count - 1];
        var previousPoint = l.Points[l.Points.Count - 2];
        var lastSegmentVector = lastPoint - previousPoint;
        lastSegmentVector.Normalize();
        var perpendicular = new Vector(lastSegmentVector.Y, -lastSegmentVector.X);

        return new Polygon {
            Fill = EdgeStroke,
            Points = {
                lastPoint,
                lastPoint - 8 * lastSegmentVector + 4 * perpendicular,
                lastPoint - 8 * lastSegmentVector - 4 * perpendicular
            }
        };
      }

      private static Polygon GetPolygon(IOrientedRectangle orientedRectangle) {
        var poly = new Polygon {
            Fill = LabelFill,
            Points = {
                new Point(orientedRectangle.AnchorX, orientedRectangle.AnchorY),
                new Point(orientedRectangle.AnchorX + orientedRectangle.Height * orientedRectangle.UpX,
                    orientedRectangle.AnchorY + orientedRectangle.Height * orientedRectangle.UpY),
                new Point(
                    orientedRectangle.AnchorX + orientedRectangle.Height * orientedRectangle.UpX +
                    orientedRectangle.Width * -orientedRectangle.UpY,
                    orientedRectangle.AnchorY + orientedRectangle.Height * orientedRectangle.UpY +
                    orientedRectangle.Width * orientedRectangle.UpX),
                new Point(orientedRectangle.AnchorX + orientedRectangle.Width * -orientedRectangle.UpY,
                    orientedRectangle.AnchorY + orientedRectangle.Width * orientedRectangle.UpX)
            }
        };
        return poly;
      }
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
      graphCanvas.Scale = 1.0d;
    }

    private void MenuItemSave_OnClick(object sender, RoutedEventArgs e) {
      var saveFileDialog = new SaveFileDialog();

      saveFileDialog.Filter = "PNG File|*.png";

      saveFileDialog.Title = "Save as PNG";

      saveFileDialog.ShowDialog();

      // If the file name is not an empty string open it for saving.
      if (saveFileDialog.FileName != "") {
        var canvas = (Canvas) TC_Graphs.SelectedContent;

        var bounds = VisualTreeHelper.GetDescendantBounds(canvas);
        var dpi = 96d;

        var rtb = new RenderTargetBitmap((int) bounds.Width, (int) bounds.Height, dpi, dpi, PixelFormats.Default);

        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen()) {
          var vb = new VisualBrush(canvas);
          dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        }

        rtb.Render(dv);
        BitmapEncoder pngEncoder = new PngBitmapEncoder();
        pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

        using (var fs = File.OpenWrite(saveFileDialog.FileName)) {
          pngEncoder.Save(fs);
        }
      }
    }

    private void MenuItemExit_OnClick(object sender, RoutedEventArgs e) {
      Close();
    }
  }
}
