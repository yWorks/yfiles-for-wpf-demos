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
using System.Linq;
using yWorks.Controls;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step11UpdateGraph
{
  /// <summary>
  ///   Tutorial step 11: Updating the graph.
  /// </summary>
  /// <remarks>
  ///   Shows how to update the graph structure when the underlying business data changes.
  /// </remarks>
  public class UpdateGraphStep : TutorialStepBase
  {
    private Graph.DataBinding.GraphBuilder? graphBuilder;
    private NodesSource<EntityData>? nodesSource;
    private List<EntityData> allNodes = [];
    private HashSet<string> nodeTypes = [];

    public override string Title => "11 Updating the graph";

    public override string DescriptionSource => "Step11UpdateGraph.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);

      allNodes = [
          new("0", "Investment Capital", "Corporation"),
          new("1", "Limited Trust", "Trust"),
          new("2", "Capital", "Branch"),
          new("3", "International Group", "PE_Risk"),
          new("4", "Monster Inc", "Corporation"),
          new("5", "Micro Group", "Trust"),
          new("6", "Melissa Barner", "PE_Risk")
      ];
      nodeTypes = ["Corporation", "Trust"];

      graphBuilder = new Graph.DataBinding.GraphBuilder(graphControl.Graph);

      // create nodes source from dynamic data
      nodesSource = CreateDynamicNodesSource(graphBuilder, allNodes);
      graphBuilder.CreateEdgesSource(
          new List<ConnectionData> {
              new("4", "2"),
              new("5", "0"),
              new("0", "2"),
              new("6", "3"),
              new("3", "2"),
              new("3", "1"),
              new("1", "2")
          }, item => item.SourceId, item => item.TargetId);

      nodesSource.NodeCreator.CreateLabelsSource(data => new[] { data.Name });

      graphBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);

      AddButtonListener(description, "updateGraphButton", () => {
        UpdateGraph();
        GraphBuilderTutorial.RunLayout(graphControl, true);
      });
      AddButtonListener(description, "resetGraphButton", () => {
        ResetGraph();
        GraphBuilderTutorial.RunLayout(graphControl, true);
      });
    }

    private NodesSource<EntityData> CreateDynamicNodesSource(
        Graph.DataBinding.GraphBuilder graphBuilder,
        IEnumerable<EntityData> data) {
      // For dynamic data, we can use a custom IEnumerable or collection that reflects the current state.
      // In this example, we use a List that we update manually, but it could also be a custom implementation.
      var filteredData = data.Where(entity => entity.Type != null && nodeTypes.Contains(entity.Type)).ToList();

      // create nodes source from dynamic data
      return graphBuilder.CreateNodesSource(filteredData, item => item.Id);
    }

    private void UpdateGraph() {
      // update displayed node types
      nodeTypes.Remove("Corporation");
      nodeTypes.Add("Branch");
      nodeTypes.Add("PE_Risk");

      // update the data in the nodesSource
      var filteredData = allNodes.Where(entity => entity.Type != null && nodeTypes.Contains(entity.Type)).ToList();
      graphBuilder!.SetData(nodesSource!, filteredData);

      // calling UpdateGraph is enough to update the graph structure
      graphBuilder.UpdateGraph();
    }

    private void ResetGraph() {
      nodeTypes = ["Corporation", "Trust"];
      var filteredData = allNodes.Where(entity => entity.Type != null && nodeTypes.Contains(entity.Type)).ToList();
      graphBuilder!.SetData(nodesSource!, filteredData);
      graphBuilder.UpdateGraph();
    }

    private sealed record EntityData(string Id, string Name, string Type);

    private sealed record ConnectionData(string SourceId, string TargetId);
  }
}
