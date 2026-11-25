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

namespace yWorks.Demo.Tutorial.GraphBuilder.Step04CreateGroupNodes
{
  /// <summary>
  ///   Tutorial step 04: Grouping nodes.
  /// </summary>
  /// <remarks>
  ///   Shows how to create group nodes to represent hierarchy information in business data.
  /// </remarks>
  public class CreateGroupNodesStep : TutorialStepBase
  {
    public override string Title => "04 Grouping nodes";

    public override string DescriptionSource => "Step04CreateGroupNodes.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);

      var graphBuilder = new Graph.DataBinding.GraphBuilder(graphControl.Graph);
      CreateGroupNodes(graphBuilder);
      graphBuilder.BuildGraph();

      GraphBuilderTutorial.RunLayout(graphControl, false);
    }

    private static void CreateGroupNodes(Graph.DataBinding.GraphBuilder graphBuilder) {
      var nodeData = new List<Item> {
          new("item0", null),
          new("item1", "group0"),
          new("item2", "group1"),
          new("group0", null),
          new("group1", "group0")
      };

      var nodes = nodeData.Where(item => item.Id.StartsWith("item")).ToList();
      graphBuilder.CreateNodesSource(nodes, item => item.Id).ParentIdProvider = item => item.ParentId;

      var groups = nodeData.Where(item => item.Id.StartsWith("group")).ToList();
      graphBuilder.CreateGroupNodesSource(groups, item => item.Id).ParentIdProvider = item => item.ParentId;
    }

    private sealed record Item(string Id, string? ParentId);
  }
}
