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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step08ConfigureLabels
{
  /// <summary>
  ///   Tutorial step 08: Configure label styles and placement.
  /// </summary>
  /// <remarks>
  ///   Shows how to configure label placement, styles, and sizes.
  /// </remarks>
  public class ConfigureLabelsStep : TutorialStepBase
  {
    public override string Title => "08 Configure label styles and placement";

    public override string DescriptionSource => "Step08ConfigureLabels.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      InitializeDefaults(graph);

      var graphBuilder = new Graph.DataBinding.GraphBuilder(graph);
      var nodeData = new List<EntityData> {
          new("0", "Investment Capital", "Corporation"),
          new("1", "Melissa Barner", "Trust"),
          new("2", "Monster Inc", "Corporation"),
          new("3", "International Group", "Trust")
      };
      var edgeData = new List<ConnectionData> { new("0", "1", "0", 30), new("1", "2", "0", 60), new("2", "3", "0", 5) };

      var nodesSource = graphBuilder.CreateNodesSource(nodeData, item => item.Id);
      var nameLabelCreator = CreateLabelsForName(nodesSource);
      var typeLabelCreator = CreateLabelsForType(nodesSource);

      ConfigureDefaultPlacement(typeLabelCreator);
      ConfigureNodeLabelParameter(nameLabelCreator);
      ConfigureLabelDefaultStyles(nameLabelCreator);
      ConfigureLabelDefaultStyles(typeLabelCreator);
      ConfigureLabelStylingWithProvider(nameLabelCreator);
      ConfigureLabelSizeWithProvider(typeLabelCreator);

      var edgesSource = graphBuilder.CreateEdgesSource(
          edgeData, item => item.SourceId, item => item.TargetId, item => item.Id);
      ConfigureEdgeLabels(edgesSource);

      graphBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void InitializeDefaults(IGraph graph) {
      var lightGray = new SolidColorBrush(Color.FromRgb(238, 238, 238));
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Rectangle, Brush = lightGray, Pen = new Pen(lightGray, 1)
      };
      graph.NodeDefaults.Size = new SizeD(135, 60);

      var edgeGray = Color.FromRgb(170, 170, 170);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(edgeGray), 1), TargetArrow = new Arrow(ArrowType.Triangle, edgeGray)
      };
    }

    private static void ConfigureNodeLabelParameter(LabelCreator<EntityData> nameLabelCreator) {
      // position the label on the top of the node
      nameLabelCreator.LayoutParameterProvider = _ => StretchNodeLabelModel.Top;
    }

    private static void ConfigureDefaultPlacement(LabelCreator<EntityData> typeLabelCreator) {
      typeLabelCreator.Defaults.LayoutParameter = InteriorNodeLabelModel.Center;
    }

    private static void ConfigureLabelDefaultStyles(LabelCreator<EntityData> labelCreator) {
      labelCreator.Defaults.Style = new LabelStyle { TextAlignment = TextAlignment.Center };
    }

    private void ConfigureLabelStylingWithProvider(LabelCreator<EntityData> nameLabelCreator) {
      // disable the sharing of the label style
      nameLabelCreator.Defaults.ShareStyleInstance = false;

      // create a provider that will assign a new style, based on the type property
      nameLabelCreator.StyleProvider = data => {
        if (data.Type == "Corporation") {
          return new LabelStyle { BackgroundBrush = Brushes.Orange, TextAlignment = TextAlignment.Center };
        }

        var iconStyle = new IconLabelStyle(LoadPersonIcon()) {
            IconSize = new SizeD(14, 14),
            IconPlacement = InteriorNodeLabelModel.Left,
            WrappedStyle = new LabelStyle { BackgroundBrush = Brushes.Pink, TextAlignment = TextAlignment.Center }
        };
        return iconStyle;
      };
    }

    private static void ConfigureLabelSizeWithProvider(LabelCreator<EntityData> typeLabelCreator) {
      // set a new size for the labels with type 'Trust'
      typeLabelCreator.PreferredSizeProvider = data => data.Type == "Trust" ? new SizeD(70, 15) : new SizeD(100, 15);
    }

    private static void ConfigureEdgeLabels(EdgesSource<ConnectionData> edgesSource) {
      // bind the label text data and add some more text information
      var edgeLabelCreator = edgesSource.EdgeCreator.CreateLabelBinding(data =>
          data.Ownership is > 0 ? $"Owns {data.Ownership:0.0}%" : "");
      var red = new SolidColorBrush(Color.FromRgb(171, 35, 70));
      var gray = new SolidColorBrush(Color.FromRgb(26, 52, 66));

      // configure the position of the label
      edgeLabelCreator.LayoutParameterProvider = _ => new SmartEdgeLabelModel().CreateParameterFromSource(0, 0, 0);

      // configure its style
      edgeLabelCreator.Defaults.ShareStyleInstance = false;
      edgeLabelCreator.StyleProvider = data => new LabelStyle {
          TextAlignment = TextAlignment.Center, TextBrush = data.Ownership > 50 ? red : gray
      };
    }

    private BitmapImage LoadPersonIcon() {
      return LoadImageResource("Step08ConfigureLabels.resources.person.png");
    }

    private static LabelCreator<EntityData> CreateLabelsForName(NodesSource<EntityData> nodesSource) {
      return nodesSource.NodeCreator.CreateLabelBinding(data => data.Name);
    }

    private static LabelCreator<EntityData> CreateLabelsForType(NodesSource<EntityData> nodesSource) {
      var labelCreator = nodesSource.NodeCreator.CreateLabelBinding();
      labelCreator.TextProvider = data => data.Type;
      return labelCreator;
    }

    private sealed record EntityData(string Id, string Name, string Type);

    private sealed record ConnectionData(string Id, string SourceId, string TargetId, double Ownership);
  }
}
