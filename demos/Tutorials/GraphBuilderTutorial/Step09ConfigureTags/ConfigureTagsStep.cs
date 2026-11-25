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

namespace yWorks.Demo.Tutorial.GraphBuilder.Step09ConfigureTags
{
  /// <summary>
  ///   Tutorial step 09: Customizing data stored in the graph.
  /// </summary>
  /// <remarks>
  ///   Shows how to provide custom tag data for graph nodes.
  /// </remarks>
  public class ConfigureTagsStep : TutorialStepBase
  {
    public override string Title => "09 Customizing data stored in the graph";

    public override string DescriptionSource => "Step09ConfigureTags.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);

      var nodes = new List<EntityData> {
          new("0", "Investment Capital", "Corporation", "EUR", null),
          new("1", "Limited Trust", "Trust", "USD", null),
          new("2", "Capital", "Branch", "USD", null),
          new("3", "International Group", "PE_Risk", "EUR", ["Germany"]),
          new("4", "Monster Inc", "Corporation", "EUR", null),
          new("5", "Micro Group", "Trust", "EUR", ["Spain"]),
          new("6", "Melissa Barner", "PE_Risk", "EUR", null)
      };

      var edges = new List<ConnectionData> {
          new("0", "4", "2"),
          new("1", "5", "0"),
          new("2", "0", "2"),
          new("3", "6", "3"),
          new("4", "3", "2"),
          new("5", "3", "1"),
          new("6", "1", "2")
      };

      var graphBuilder = new Graph.DataBinding.GraphBuilder(graphControl.Graph);
      var nodesSource = graphBuilder.CreateNodesSource(nodes, item => item.Id);
      CreateNodeTags(nodesSource);

      graphBuilder.CreateEdgesSource(edges, item => item.SourceId, item => item.TargetId, item => item.Id);
      nodesSource.NodeCreator.CreateLabelsSource(data => new[] { data.Name });

      graphBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void CreateNodeTags(NodesSource<EntityData> nodesSource) {
      // configure the provider that returns an object with the name and the type property of the nodes
      nodesSource.NodeCreator.TagProvider = data => new OwnerData(data.Name, data.Type);
    }

    private sealed record EntityData(string Id, string Name, string Type, string Currency, List<string>? Jurisdiction);

    private sealed record OwnerData(string Name, string Type);

    private sealed record ConnectionData(string Id, string SourceId, string TargetId);
  }
}
