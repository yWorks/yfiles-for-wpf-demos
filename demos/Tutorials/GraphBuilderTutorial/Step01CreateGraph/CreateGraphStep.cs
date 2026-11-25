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
using GraphBuilderApi = yWorks.Graph.DataBinding.GraphBuilder;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step01CreateGraph
{
  /// <summary>
  ///   Tutorial step 01: Create Graph.
  /// </summary>
  /// <remarks>
  ///   Shows how to use a GraphBuilder to create a graph from node and edge business data.
  /// </remarks>
  public class CreateGraphStep : TutorialStepBase
  {
    public override string Title => "01 Create Graph";

    public override string DescriptionSource => "Step01CreateGraph.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);
      CreateGraph(graphControl);
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void CreateGraph(GraphControl graphControl) {
      var persons = new List<Person> { new("0", "Erik Joplin"), new("1", "Melissa Barner") };
      var relationships = new List<Relationship> { new("0", "0", "1") };

      // Use the graph from the GraphControl, for example.
      var graph = graphControl.Graph;

      // Instantiate the graph builder.
      var graphBuilder = new GraphBuilderApi(graph);

      // Add node and edge data sources.
      graphBuilder.CreateNodesSource(persons, person => person.Id);
      graphBuilder.CreateEdgesSource(relationships, relationship => relationship.SourceId,
          relationship => relationship.TargetId);

      // Create the graph.
      graphBuilder.BuildGraph();
    }

    private sealed record Person(string Id, string Name);

    private sealed record Relationship(string Id, string SourceId, string TargetId);
  }
}
