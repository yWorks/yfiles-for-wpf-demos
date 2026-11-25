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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using GraphBuilderApi = yWorks.Graph.DataBinding.GraphBuilder;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step07CreateLabelsSources
{
  /// <summary>
  ///   Tutorial step 07: Adding Labels.
  /// </summary>
  /// <remarks>
  ///   Shows how to add labels from bindings, providers, and label sources.
  /// </remarks>
  public class CreateLabelsSourcesStep : TutorialStepBase
  {
    public override string Title => "07 Adding Labels";

    public override string DescriptionSource => "Step07CreateLabelsSources.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      InitializeDefaults(graph);

      var graphBuilder = new GraphBuilderApi(graph);
      CreateNodeLabelsWithBinding(graphBuilder);
      CreateNodeLabelsWithProvider(graphBuilder);
      CreateNodeLabelsWithSources(graphBuilder);
      CreateEdgeLabelsWithProvider(graphBuilder);

      graphBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void InitializeDefaults(IGraph graph) {
      var lightGray = new SolidColorBrush(Color.FromRgb(238, 238, 238));
      graph.NodeDefaults.Style = new ShapeNodeStyle {
          Shape = ShapeNodeShape.Rectangle, Brush = lightGray, Pen = new Pen(lightGray, 1)
      };
      graph.NodeDefaults.Size = new SizeD(135, 60);

      graph.NodeDefaults.Labels.Style = new LabelStyle { TextAlignment = TextAlignment.Center };
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Center;

      var edgeGray = Color.FromRgb(170, 170, 170);
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(new SolidColorBrush(edgeGray), 1), TargetArrow = new Arrow(ArrowType.Triangle, edgeGray)
      };
    }

    private static void CreateNodeLabelsWithBinding(GraphBuilderApi graphBuilder) {
      var nodeData = new List<NodeData> { new("0", "Investment Capital"), new("1", "Melissa Barner") };
      var nodesSource = graphBuilder.CreateNodesSource(nodeData, item => item.Id);

      // create the label binding to the name property
      nodesSource.NodeCreator.CreateLabelBinding(data => data.Name);
    }

    private static void CreateNodeLabelsWithProvider(GraphBuilderApi graphBuilder) {
      var nodeData = new List<NodeData> { new("2", "Monster Inc") };
      var nodesSource = graphBuilder.CreateNodesSource(nodeData, item => item.Id);

      // create the text provider that will return the name of each node
      var labelCreator = nodesSource.NodeCreator.CreateLabelBinding();
      labelCreator.TextProvider = data => data.Name.ToUpperInvariant();
    }

    private static void CreateNodeLabelsWithSources(GraphBuilderApi graphBuilder) {
      var nodeData = new List<NodeWithOwnersData> {
          new("3", ["Local Group", "Germany"]), new("4", ["International Group"])
      };
      var nodesSource = graphBuilder.CreateNodesSource(nodeData, item => item.Id);

      // create the label sources based on the `owners` property
      var labelsSource = nodesSource.NodeCreator.CreateLabelsSource(data => data.Owners);
      labelsSource.LabelCreator.LayoutParameterProvider = data =>
          data.EndsWith("Group", StringComparison.Ordinal)
              ? InteriorNodeLabelModel.Center
              : InteriorNodeLabelModel.Bottom;
    }

    private static void CreateEdgeLabelsWithProvider(GraphBuilderApi graphBuilder) {
      var edgeData = new List<EdgeData> {
          new("0", "1", "0", 30), new("1", "0", "2", 60), new("2", "4", "0", 5), new("3", "3", "0", 5)
      };
      var edgesSource = graphBuilder.CreateEdgesSource(
          edgeData, item => item.SourceId, item => item.TargetId, item => item.Id);

      // bind the label text data and add some more text information
      edgesSource.EdgeCreator.CreateLabelBinding(data =>
          $"Owns {data.Ownership.ToString("0.0", CultureInfo.InvariantCulture)}%");
    }

    private sealed record NodeData(string Id, string Name);

    private sealed record NodeWithOwnersData(string Id, List<string> Owners);

    private sealed record EdgeData(string Id, string SourceId, string TargetId, double Ownership);
  }
}
