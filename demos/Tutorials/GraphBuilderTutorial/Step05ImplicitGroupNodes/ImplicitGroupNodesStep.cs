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
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step05ImplicitGroupNodes
{
  /// <summary>
  ///   Tutorial step 05: Implicit grouping.
  /// </summary>
  /// <remarks>
  ///   Shows how to create group nodes from implicit hierarchy information in business data.
  /// </remarks>
  public class ImplicitGroupNodesStep : TutorialStepBase
  {
    public override string Title => "05 Implicit grouping";

    public override string DescriptionSource => "Step05ImplicitGroupNodes.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);

      var graph = graphControl.Graph;
      var graphBuilder = new Graph.DataBinding.GraphBuilder(graph);
      CreateGroupNodes(graphBuilder, graph);
      graphBuilder.BuildGraph();

      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void CreateGroupNodes(Graph.DataBinding.GraphBuilder graphBuilder, IGraph graph) {
      var nodeData = CreateData();
      var edgeData = CreateEdgeData();

      var nodesSource = graphBuilder.CreateNodesSource(nodeData, item => item.Id);
      nodesSource.NodeCreator.Defaults.Labels.LayoutParameter = InteriorNodeLabelModel.Top;
      nodesSource.NodeCreator.Defaults.Size = new SizeD(60, 40);

      var parentsSource = nodesSource.CreateParentNodesSource<string>(item => item.Path!);
      parentsSource.NodeCreator.CreateLabelBinding(data => data);

      Provider<string, string> parentDataProvider = path => {
        var separator = path.LastIndexOf('/');
        return separator <= 0 ? null! : path.Substring(0, separator);
      };
      var ancestorSource = parentsSource.CreateParentNodesSource(parentDataProvider);
      // Enable recursive processing higher up in the container hierarchy
      ancestorSource.AddParentNodesSource(parentDataProvider, ancestorSource);
      ancestorSource.NodeCreator.CreateLabelBinding(data => data);

      // Enable processing of the contents of the nodes in the NodeData
      var childNodesSource = nodesSource.CreateChildNodesSource(item => item.Children, item => item.Id);
      // Enable processing of the contents of the child nodes
      var descendantsSource = childNodesSource.CreateChildNodesSource(item => item.Children, item => item.Id);
      // Enable recursive processing of the contents
      descendantsSource.AddChildNodesSource(item => item.Children, descendantsSource);

      // Declare edges between all different kinds of entities
      graphBuilder.CreateEdgesSource(edgeData, item => item.From, item => item.To);

      // Styling for the group nodes
      // We style the different levels differently
      // the "path" containers are pale blue
      var pathStyle = new GroupNodeStyle {
          TabBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#9dc6d0")),
          TabHeight = 20,
          ContentAreaPadding = new InsetsD(10)
      };
      parentsSource.NodeCreator.Defaults.Style = pathStyle;

      // whereas the entities in the NodeData and all other group nodes have their respective
      // default styles
      nodesSource.NodeCreator.StyleProvider = item => item.Children is { Count: > 0 }
          ? graph.GroupNodeDefaults.Style
          : graph.NodeDefaults.Style;
      childNodesSource.NodeCreator.StyleProvider = item => item.Children is { Count: > 0 }
          ? graph.GroupNodeDefaults.Style
          : graph.NodeDefaults.Style;
      descendantsSource.NodeCreator.StyleProvider = item => item.Children is { Count: > 0 }
          ? graph.GroupNodeDefaults.Style
          : graph.NodeDefaults.Style;
    }

    private static List<ItemData> CreateData() {
      return [
          new("item0", "/root/dir1/dir1", null),
          new("item1", "/root/dir1/dir2", [
              new("child1", null, [
                  new("child11", null, [
                      new("child14", null, null),
                      new("child15", null, null)
                  ]),
                  new("child12", null, null)
              ]),

              new("child2", null, [
                  new("child13", null, null)
              ])
          ]),

          new("item2", "/root/dir1/dir2", null),
          new("item3", "/root/dir2/dir1", null),
          new("item4", "/root/dir2/dir1", [
              new("child3", null, null),
              new("child4", null, null),
              new("child5", null, null)
          ]),

          new("item5", "/root/dir2/dir2", [
              new("child6", null, null)
          ]),

          new("item6", "/root/dir2/dir2", [
              new("child7", null, [
                  new("child16", null, null)
              ]),

              new("child8", null, null)
          ]),

          new("item7", "/root/dir1", [
              new("child9", null, null),
              new("child10", null, null)
          ]),

          new("item8", "/root/dir1", null)
      ];
    }

    private static List<EdgeData> CreateEdgeData() {
      return [
          new("item8", "child16"),
          new("item8", "child9"),
          new("item8", "child10"),
          new("item2", "child14"),
          new("item2", "child15"),
          new("item2", "child13"),
          new("item2", "child12"),
          new("child12", "child13"),
          new("item0", "item2"),
          new("item8", "child8"),
          new("child16", "child6"),
          new("item3", "child5"),
          new("item3", "child3"),
          new("/root/dir1", "/root/dir2")
      ];
    }

    private sealed record ItemData(string Id, string? Path, List<ItemData>? Children);

    private sealed record EdgeData(string From, string To);
  }
}
