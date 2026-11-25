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
using yWorks.Geometry;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step12AdjacencyGraphBuilder
{
  /// <summary>
  ///   Tutorial step 12: Building a graph with the AdjacencyGraphBuilder.
  /// </summary>
  /// <remarks>
  ///   Shows how to build a graph when the relationship information is part of the node data.
  /// </remarks>
  public class AdjacencyGraphBuilderStep : TutorialStepBase
  {
    public override string Title => "12 Building a graph with the AdjacencyGraphBuilder";

    public override string DescriptionSource => "Step12AdjacencyGraphBuilder.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);
      graph.NodeDefaults.Size = new SizeD(200, 80);

      var orgChartData = CreateOrgChartData();
      var adjacencyGraphBuilder = new AdjacencyGraphBuilder(graph);
      var adjacencyNodesSource = adjacencyGraphBuilder.CreateNodesSource(orgChartData, item => item.Id);

      adjacencyNodesSource.AddSuccessorIds(data => data.Colleagues?.Cast<object>() ?? [],
          new EdgeCreator<OrgChartEntry>());

      adjacencyNodesSource.NodeCreator.CreateLabelBinding(dataItem => dataItem.Name);

      adjacencyGraphBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static List<OrgChartEntry> CreateOrgChartData() {
      return [
          new("0", "Chief Executive Officer", "Eric Joplin", ["0_0", "0_1"]),
          new("0_0", "Chief Executive Assistant", "Gary Roberts", ["0_0_0", "0_0_1"]),
          new("0_0_0", "Senior Executive Assistant", "Alexander Burns", null),
          new("0_0_1", "Junior Executive Assistant", "Linda Newland", null),
          new("0_1", "Vice President of Production", "Amy Kain", ["0_1_0"]),
          new("0_1_0", "Production Supervisor", "Kathy Maxwell", null)
      ];
    }

    private sealed record OrgChartEntry(string Id, string Position, string Name, List<string>? Colleagues);
  }
}
