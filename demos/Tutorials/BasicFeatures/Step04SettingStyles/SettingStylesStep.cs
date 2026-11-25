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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step04SettingStyles
{
  /// <summary>
  ///   Tutorial step 04: Setting Styles.
  /// </summary>
  /// <remarks>
  ///   Shows how to configure the visual appearance of graph elements using styles:
  ///   default styles, per-item styles at creation time, and changing styles at runtime.
  /// </remarks>
  public class SettingStylesStep : TutorialStepBase
  {
    public override string Title => "04 Setting Styles";

    public override string DescriptionSource => "Step04SettingStyles.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      SetDefaultStyles(graph);
      BasicFeaturesTutorial.CreateSampleGraph(graph);
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      AddButtonListener(description, "createGraphItemsWithStylesButton", () => CreateGraphItemsWithStyles(graph));
      AddButtonListener(description, "changeStylesButton", () => SetStyles(graph));
    }

    /// <summary>
    ///   Sets up default styles for graph elements.
    ///   Default styles apply only to elements created after the default style has been set,
    ///   so typically you'd set these as early as possible in your application.
    /// </summary>
    private static void SetDefaultStyles(IGraph graph) {
      // Create a ShapeNodeStyle instance, using an orange fill.
      // Set this style as the default for all nodes that don't have another
      // style assigned explicitly.
      var nodeStyle = new ShapeNodeStyle {
          Shape = ShapeNodeShape.RoundRectangle,
          Brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ff6c00")),
          Pen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#662f01")), 1.5)
      };
      graph.NodeDefaults.Style = nodeStyle;

      // Also assign the default node size
      graph.NodeDefaults.Size = new SizeD(40, 40);

      // Create a PolylineEdgeStyle which will be used as default for all edges
      // that don't have another style assigned explicitly
      var edgeStyle = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#662f01")), 1.5),
          TargetArrow = new Arrow(ArrowType.Triangle, (Color) ColorConverter.ConvertFromString("#662f01"))
      };
      graph.EdgeDefaults.Style = edgeStyle;

      // Create a label style using Tahoma as the label font and a black text color
      var defaultLabelStyle = new LabelStyle {
          Typeface = new Typeface("Tahoma"),
          TextSize = 12,
          TextBrush = Brushes.Black,
          BackgroundBrush = new SolidColorBrush(Color.FromArgb(135, 255, 255, 255)), // #ffffff87
          Shape = LabelShape.RoundRectangle,
          Padding = new InsetsD(2, 5, 2, 5)
      };

      // Set the defined style as the default for both edge and node labels
      graph.EdgeDefaults.Labels.Style = defaultLabelStyle;
      graph.NodeDefaults.Labels.Style = defaultLabelStyle;
    }

    /// <summary>
    ///   Creates a node, an edge, and a label using specific styles,
    ///   i.e., styles different from the defaults.
    /// </summary>
    private void CreateGraphItemsWithStyles(IGraph graph) {
      var nodes = graph.Nodes.ToList();
      if (nodes.Count == 0) {
        return;
      }
      var sourceNode = nodes[0];

      var nodeStyle = new ImageNodeStyle(LoadStarImage());
      var node = graph.CreateNode(new RectD(30, 215, 40, 40), nodeStyle);

      var edgeStyle = new PolylineEdgeStyle {
          TargetArrow = new Arrow(ArrowType.Triangle, (Color) ColorConverter.ConvertFromString("#224556")),
          Pen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#224556")), 2)
      };
      graph.CreateEdge(sourceNode, node, edgeStyle);

      var labelStyle = new LabelStyle {
          BackgroundBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#a6a6c0"))
      };
      graph.AddLabel(node, "New Label", null, labelStyle);
    }

    /// <summary>Changes the styles of some of the graph items.</summary>
    private static void SetStyles(IGraph graph) {
      var nodes = graph.Nodes.ToList();
      var edges = graph.Edges.ToList();
      var nodeLabels = graph.Labels.Where(l => l.Owner is INode).ToList();
      if (nodes.Count < 3 || edges.Count == 0 || nodeLabels.Count < 3) {
        return;
      }
      var node1 = nodes[1];
      var node2 = nodes[2];
      var edge = edges[0];
      var label = nodeLabels[2];

      var dashPen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#224556")), 2) {
          DashStyle = DashStyles.Dash
      };
      var edgeStyle = new PolylineEdgeStyle {
          Pen = dashPen,
          SourceArrow = new Arrow(ArrowType.Ellipse, (Color) ColorConverter.ConvertFromString("#224556")),
          TargetArrow = new Arrow(ArrowType.Triangle, (Color) ColorConverter.ConvertFromString("#224556"))
      };
      graph.SetStyle(edge, edgeStyle);

      // Creates a different style for the label with black text and a blue border
      var labelStyle = new LabelStyle {
          BackgroundPen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#46A8D5")), 2),
          BackgroundBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#b4dbed")),
          Padding = new InsetsD(3, 5, 3, 5)
      };
      graph.SetStyle(label, labelStyle);

      var nodeStyle1 = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Ellipse,
          Brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#a37ab3")),
          Pen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#433449")), 2)
      };
      graph.SetStyle(node1, nodeStyle1);

      var nodeStyle2 = new RectangleNodeStyle {
          Brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#46a8d5")),
          Pen = new Pen(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#224556")), 2),
          CornerStyle = CornerStyle.Cut
      };
      graph.SetStyle(node2, nodeStyle2);
    }

    /// <summary>
    ///   Loads the star icon from the embedded assembly resources.
    /// </summary>
    private BitmapImage LoadStarImage() {
      return LoadImageResource("star-16.png");
    }
  }
}
