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
using yWorks.Geometry;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step13TreeBuilder
{
  /// <summary>
  ///   Tutorial step 13: Building a graph with TreeBuilder.
  /// </summary>
  /// <remarks>
  ///   Shows how to build a graph from data that already has a tree structure.
  /// </remarks>
  public class TreeBuilderStep : TutorialStepBase
  {
    public override string Title => "13 Building a graph with TreeBuilder";

    public override string DescriptionSource => "Step13TreeBuilder.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);
      graph.NodeDefaults.Size = new SizeD(200, 80);

      var orgChartData = CreateOrgChartData();
      var treeBuilder = new TreeBuilder(graph);
      var rootNodesSource = treeBuilder.CreateRootNodesSource(orgChartData);

      // the childDataProvider identifies the property of a node object that contains its child nodes
      rootNodesSource.AddChildNodesSource(data => data.Colleagues ?? [], rootNodesSource);

      rootNodesSource.NodeCreator.CreateLabelBinding(dataItem => dataItem.Name);

      treeBuilder.BuildGraph();
      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static List<OrgChartEntry> CreateOrgChartData() {
      return [
          new("Chief Executive Officer", "Eric Joplin",
              new List<OrgChartEntry> {
                  new("Chief Executive Assistant", "Gary Roberts",
                      new List<OrgChartEntry> {
                          new("Senior Executive Assistant", "Alexander Burns", null),
                          new("Junior Executive Assistant", "Linda Newland", null)
                      }),
                  new("Vice President of Production", "Amy Kain",
                      new List<OrgChartEntry> { new("Production Supervisor", "Kathy Maxwell", null) })
              })
      ];
    }

    private sealed record OrgChartEntry(string Position, string Name, IEnumerable<OrgChartEntry>? Colleagues);
  }
}
