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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Demo.Tutorial.GraphBuilder.Step01CreateGraph;
using yWorks.Demo.Tutorial.GraphBuilder.Step02CreateNodesSources;
using yWorks.Demo.Tutorial.GraphBuilder.Step03CreateEdgesSources;
using yWorks.Demo.Tutorial.GraphBuilder.Step04CreateGroupNodes;
using yWorks.Demo.Tutorial.GraphBuilder.Step05ImplicitGroupNodes;
using yWorks.Demo.Tutorial.GraphBuilder.Step06ConfigureStyles;
using yWorks.Demo.Tutorial.GraphBuilder.Step07CreateLabelsSources;
using yWorks.Demo.Tutorial.GraphBuilder.Step08ConfigureLabels;
using yWorks.Demo.Tutorial.GraphBuilder.Step09ConfigureTags;
using yWorks.Demo.Tutorial.GraphBuilder.Step10ConfigureLayout;
using yWorks.Demo.Tutorial.GraphBuilder.Step11UpdateGraph;
using yWorks.Demo.Tutorial.GraphBuilder.Step12AdjacencyGraphBuilder;
using yWorks.Demo.Tutorial.GraphBuilder.Step13TreeBuilder;
using yWorks.Geometry;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Tutorial.GraphBuilder
{
  [Demo("tutorial-graph-builder", "Tutorials", "Tutorial: Graph Builder",
      Summary = "Learn how to convert business data into a graph using the GraphBuilder class.",
      Tags = ["business data", "graph", "import"],
      Keywords = ["tutorial", "graphbuilder", "business data", "json", "data binding"])]
  public class GraphBuilderTutorial : TutorialBase
  {
    /// <summary>Registers the tutorial steps that have been migrated so far.</summary>
    protected override void AddTutorialSteps() {
      AddTutorialStep(new CreateGraphStep());
      AddTutorialStep(new CreateNodesSourcesStep());
      AddTutorialStep(new CreateEdgesSourcesStep());
      AddTutorialStep(new CreateGroupNodesStep());
      AddTutorialStep(new ImplicitGroupNodesStep());
      AddTutorialStep(new ConfigureStylesStep());
      AddTutorialStep(new CreateLabelsSourcesStep());
      AddTutorialStep(new ConfigureLabelsStep());
      AddTutorialStep(new ConfigureTagsStep());
      AddTutorialStep(new ConfigureLayoutStep());
      AddTutorialStep(new UpdateGraphStep());
      AddTutorialStep(new AdjacencyGraphBuilderStep());
      AddTutorialStep(new TreeBuilderStep());
    }

    [STAThread]
    public static void Main() {
      RunTutorial<GraphBuilderTutorial>();
    }

    /// <summary>Initializes the default styles for nodes, edges, and labels.</summary>
    public static void InitializeTutorialDefaults(GraphControl graphControl) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);

      graph.GroupNodeDefaults.Style = new GroupNodeStyle {
          TabBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#111d4a")),
          ContentAreaPadding = new InsetsD(10)
      };
    }

    /// <summary>Configures colors for nodes created by the supplied data sources.</summary>
    public static void ConfigureStyles<TDataItem>(IEnumerable<NodesSource<TDataItem>> nodesSources) {
      var palettes = new[] { Themes.Palette12, Themes.Palette11 };
      var index = 0;
      foreach (var nodesSource in nodesSources) {
        ConfigureStyle(nodesSource, palettes[index++ % palettes.Length]);
      }
    }

    /// <summary>Configures two sources whose data items have different types.</summary>
    public static void ConfigureStyles<TFirstDataItem, TSecondDataItem>(
        NodesSource<TFirstDataItem> firstSource,
        NodesSource<TSecondDataItem> secondSource) {
      ConfigureStyle(firstSource, Themes.Palette12);
      ConfigureStyle(secondSource, Themes.Palette11);
    }

    private static void ConfigureStyle<TDataItem>(NodesSource<TDataItem> nodesSource, Palette palette) {
      var defaults = nodesSource.NodeCreator.Defaults;
      defaults.Style = DemoStyles.CreateDemoNodeStyle(palette);
      defaults.Labels.Style = DemoStyles.CreateDemoNodeLabelStyle(palette);
    }

    /// <summary>Applies the tutorial"s preconfigured hierarchical layout.</summary>
    public static void RunLayout(GraphControl graphControl, bool animated) {
      graphControl.LimitFitContentZoom = false;
      var layout = new HierarchicalLayout();
      layout.ComponentLayout.Enabled = true;

      var layoutExecutor = new LayoutExecutor(graphControl, layout) {
          TargetBoundsPadding = new InsetsD(250),
          AnimationDuration = TimeSpan.FromMilliseconds(animated ? 700 : 0),
          AnimateViewport = true,
          PortAdjustmentPolicies = { Constant = PortAdjustmentPolicy.Always }
      };
      _ = layoutExecutor.Start();
    }
  }
}
