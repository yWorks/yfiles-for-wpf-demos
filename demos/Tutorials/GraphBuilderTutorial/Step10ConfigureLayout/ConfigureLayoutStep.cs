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
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.Tutorial.GraphBuilder.Step10ConfigureLayout
{
  /// <summary>
  ///   Tutorial step 10: Using layout information provided by business data.
  /// </summary>
  /// <remarks>
  ///   Shows how to read node layouts and edge bends from business data.
  /// </remarks>
  public class ConfigureLayoutStep : TutorialStepBase
  {
    public override string Title => "10 Using layout information provided by business data";

    public override string DescriptionSource => "Step10ConfigureLayout.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      GraphBuilderTutorial.InitializeTutorialDefaults(graphControl);

      var graphBuilder = new Graph.DataBinding.GraphBuilder(graphControl.Graph);
      var nodesSource = ConfigureNodeLayoutWithProvider(graphBuilder);
      ConfigureBends(graphBuilder);

      GraphBuilderTutorial.ConfigureStyles([nodesSource]);

      graphBuilder.BuildGraph();
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);
    }

    private static NodesSource<NodeLayoutData> ConfigureNodeLayoutWithProvider(
        Graph.DataBinding.GraphBuilder graphBuilder) {
      var nodeData = new List<NodeLayoutData> {
          new("00", new RectD(110, 20, 30, 30)),
          new("01", new RectD(145, 95, 30, 30)),
          new("02", new RectD(75, 95, 30, 30))
      };

      // create the node using the id property
      var nodesSource = graphBuilder.CreateNodesSource(nodeData, item => item.Id);
      // configure the layout provider that returns the layout information
      nodesSource.NodeCreator.LayoutProvider = data => data.Layout;

      return nodesSource;
    }

    private static void ConfigureBends(Graph.DataBinding.GraphBuilder graphBuilder) {
      var edgeData = new List<EdgeLayoutData> {
          new("00", "01", [new(125, 68), new(160, 68)]), new("00", "02", [new(125, 68), new(90, 68)])
      };

      // create the edges using the sourceId/targetId
      var edgesSource = graphBuilder.CreateEdgesSource(edgeData, item => item.SourceId, item => item.TargetId);
      // configure the bend provider to return the location of each bend point
      edgesSource.EdgeCreator.BendsProvider = data => data.Bends;
    }

    private sealed record NodeLayoutData(string Id, RectD Layout);

    private sealed record EdgeLayoutData(string SourceId, string TargetId, List<PointD> Bends);
  }
}
