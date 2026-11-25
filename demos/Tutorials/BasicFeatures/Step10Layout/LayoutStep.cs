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

using System;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step10Layout
{
  /// <summary>
  ///   Tutorial step 10: Automatic Graph Layout.
  /// </summary>
  /// <remarks>
  ///   Shows how to use yFiles WPF layout algorithms to automatically place graph elements,
  ///   both instantly and with animation.
  /// </remarks>
  public class LayoutStep : TutorialStepBase
  {
    public override string Title => "10 Automatic Graph Layout";

    public override string DescriptionSource => "Step10Layout.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitializeSimpleDemoStyles(graph);
      CreateSampleGraph(graph);
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      AddButtonListener(description, "layoutButton", () => ApplyLayout(graphControl));
      AddButtonListener(description, "layoutAnimatedButton", () => RunLayoutAnimated(graphControl));
    }

    /// <summary>Runs a hierarchical layout on the graph.</summary>
    private static void ApplyLayout(GraphControl graphControl) {
      var hierarchicalLayout = new HierarchicalLayout();
      graphControl.Graph.ApplyLayout(hierarchicalLayout);

      // Fit the graph bounds since they changed for the new layout
      graphControl.FitGraphBounds();
    }

    /// <summary>Runs an organic layout with animation.</summary>
    private static void RunLayoutAnimated(GraphControl graphControl) {
      // Run the layout and animate the result
      _ = graphControl.ApplyLayoutAnimated(new OrganicLayout(), TimeSpan.FromMilliseconds(500));
    }

    /// <summary>Creates a larger sample graph for layout and analysis from an embedded JSON resource.</summary>
    private void CreateSampleGraph(IGraph graph) {
      BuildGraphFromJson(graph, "Step10Layout.graph-data.json");
    }
  }
}
