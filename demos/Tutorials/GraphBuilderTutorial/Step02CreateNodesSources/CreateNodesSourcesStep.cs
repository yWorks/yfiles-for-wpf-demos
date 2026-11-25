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
using yWorks.Controls;
using yWorks.Graph.DataBinding;
using GraphBuilderApi = yWorks.Graph.DataBinding.GraphBuilder;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step02CreateNodesSources
{
  /// <summary>
  ///   Tutorial step 02: Loading data from different sources.
  /// </summary>
  /// <remarks>
  ///   Shows how to use different data sources for nodes.
  /// </remarks>
  public class CreateNodesSourcesStep : TutorialStepBase
  {
    public override string Title => "02 Loading data from different sources";

    public override string DescriptionSource => "Step02CreateNodesSources.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);
      CreateGraph(graphControl);
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void CreateGraph(GraphControl graphControl) {
      var graphBuilder = new GraphBuilderApi(graphControl.Graph);

      var nodesSource0 = CreateNodesSourceFromList(graphBuilder);
      var nodesSource1 = CreateNodesSourceFromDictionary(graphBuilder);

      GraphBuilderTutorial.ConfigureStyles(nodesSource0, nodesSource1);

      graphBuilder.BuildGraph();
    }

    private static NodesSource<NodeData> CreateNodesSourceFromList(GraphBuilderApi graphBuilder) {
      var nodeData = new List<NodeData> { new("00"), new("01"), new("02") };

      // nodes source for the turquoise nodes
      return graphBuilder.CreateNodesSource(nodeData, item => item.Id);
    }

    private static NodesSource<KeyValuePair<string, NodeData>> CreateNodesSourceFromDictionary(
        GraphBuilderApi graphBuilder) {
      var nodeData = new Dictionary<string, NodeData> {
          ["node1"] = new("10"), ["node2"] = new("11"), ["node3"] = new("12")
      };

      // nodes source for the blue nodes
      return graphBuilder.CreateNodesSource(nodeData, entry => entry.Value.Id);
    }

    private sealed record NodeData(string Id);
  }
}
