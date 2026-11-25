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

namespace yWorks.Demo.Tutorial.GraphBuilder.Step03CreateEdgesSources
{
  /// <summary>
  ///   Tutorial step 03: Creating edges sources.
  /// </summary>
  /// <remarks>
  ///   Shows how to use business data to create nodes and edges.
  /// </remarks>
  public class CreateEdgesSourcesStep : TutorialStepBase
  {
    public override string Title => "03 Creating edges sources";

    public override string DescriptionSource => "Step03CreateEdgesSources.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);
      CreateGraph(graphControl);
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void CreateGraph(GraphControl graphControl) {
      var graphBuilder = new Graph.DataBinding.GraphBuilder(graphControl.Graph);
      CreateEdgesSourceFromList(graphBuilder);
      graphBuilder.BuildGraph();
    }

    private static void CreateEdgesSourceFromList(Graph.DataBinding.GraphBuilder graphBuilder) {
      // add node data including ids
      var nodeData = new List<NodeData> { new(0), new(1), new(2) };
      graphBuilder.CreateNodesSource(nodeData, item => item.Id);

      // data for some edges that connect to the nodes using their ids
      var edgeData = new List<EdgeData> { new(0, 0, 1), new(1, 0, 2) };

      // create an edges source with id providers for sources and targets
      graphBuilder.CreateEdgesSource(edgeData, item => item.SourceId, item => item.TargetId);
    }

    private sealed record NodeData(int Id);

    private sealed record EdgeData(int Id, int SourceId, int TargetId);
  }
}
