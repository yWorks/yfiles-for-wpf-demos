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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step06ConfigureStyles
{
  /// <summary>
  ///   Tutorial step 06: Configure Styles.
  /// </summary>
  /// <remarks>
  ///   Shows how to configure default styles and style providers for nodes and edges.
  /// </remarks>
  public class ConfigureStylesStep : TutorialStepBase
  {
    public override string Title => "06 Configure Styles";

    public override string DescriptionSource => "Step06ConfigureStyles.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var nodes = new List<EntityData> {
          new("0", "Investment Capital", "Corporation", "EUR"),
          new("1", "Limited Trust", "Trust", "USD"),
          new("2", "Capital", "Branch", "USD"),
          new("3", "International Group", "PE_Risk", "EUR"),
          new("4", "Monster Inc", "Corporation", "EUR"),
          new("5", "Micro Group", "Trust", "EUR"),
          new("6", "Melissa Barner", "PE_Risk", "EUR")
      };

      var edges = new List<ConnectionData> {
          new("4", "2", 0),
          new("5", "0", 40),
          new("0", "2", 0),
          new("6", "3", 30),
          new("3", "2", 60),
          new("3", "1", 51),
          new("1", "2", 30)
      };

      var graphBuilder = new Graph.DataBinding.GraphBuilder(graphControl.Graph);

      var nodesSource = graphBuilder.CreateNodesSource(nodes, item => item.Id);
      nodesSource.NodeCreator.CreateLabelBinding(item => item.Name);

      ConfigureStylesWithDefaults(nodesSource);
      ConfigureStylesWithProvider(nodesSource);

      var edgesSource = graphBuilder.CreateEdgesSource(edges, item => item.SourceId, item => item.TargetId);
      ConfigureEdgeStylesWithProvider(edgesSource);

      graphBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void ConfigureStylesWithDefaults(NodesSource<EntityData> nodesSource) {
      var blue = new SolidColorBrush(Color.FromRgb(0x42, 0x81, 0xA4));
      nodesSource.NodeCreator.Defaults.Size = new SizeD(150, 90);
      nodesSource.NodeCreator.Defaults.Style = new ShapeNodeStyle { Shape = ShapeNodeShape.Ellipse, Brush = blue };
    }

    private static void ConfigureStylesWithProvider(NodesSource<EntityData> nodesSource) {
      var gold = new SolidColorBrush(Color.FromRgb(0xF0, 0xC8, 0x08));
      var green = new SolidColorBrush(Color.FromRgb(0x56, 0x92, 0x6E));
      nodesSource.NodeCreator.StyleProvider = entityData => {
        if (entityData.Type == "Branch") {
          return new ShapeNodeStyle { Shape = ShapeNodeShape.RoundRectangle, Brush = gold };
        }
        if (entityData.Type == "Corporation") {
          return new ShapeNodeStyle { Shape = ShapeNodeShape.Octagon, Brush = green };
        }
        return null!;
      };
    }

    private static void ConfigureEdgeStylesWithProvider(EdgesSource<ConnectionData> edgesSource) {
      var red = new SolidColorBrush(Color.FromRgb(0xDB, 0x3A, 0x34));
      var gray = new SolidColorBrush(Color.FromRgb(0xC1, 0xC1, 0xC1));
      edgesSource.EdgeCreator.StyleProvider = connectionData => {
        if (connectionData.Ownership > 0) {
          var pen = new Pen(connectionData.Ownership > 50 ? red : Brushes.Black, 3);
          return new PolylineEdgeStyle { Pen = pen };
        }

        var dashedPen = new Pen(gray, 3) { DashStyle = DashStyles.Dash };
        return new PolylineEdgeStyle { Pen = dashedPen };
      };
    }

    private sealed record EntityData(string Id, string Name, string Type, string Currency);

    private sealed record ConnectionData(string SourceId, string TargetId, double Ownership);
  }
}
