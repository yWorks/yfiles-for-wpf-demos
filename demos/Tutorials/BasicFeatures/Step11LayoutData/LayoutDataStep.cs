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
using System.Linq;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step11LayoutData
{
  /// <summary>
  ///   Tutorial step 11: Layout Data.
  /// </summary>
  /// <remarks>
  ///   Shows how to configure a hierarchical layout using alignment values stored in node labels.
  /// </remarks>
  public class LayoutDataStep : TutorialStepBase
  {
    public override string Title => "11 Layout Data";

    public override string DescriptionSource => "Step11LayoutData.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;
      DemoStyles.InitTutorialStyles(graph);
      CreateSampleGraph(graph);

      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);

      var inputMode = new GraphEditorInputMode();
      inputMode.LabelEdited += (_, _) => RunLayout(graphControl);
      graphControl.InputMode = inputMode;

      AddButtonListener(description, "layoutButton", () => RunLayout(graphControl));
    }

    /// <summary>Runs a hierarchical layout using alignment data from the node labels.</summary>
    private static void RunLayout(GraphControl graphControl) {
      var layout = new HierarchicalLayout();
      var layoutData = layout.CreateLayoutData(graphControl.Graph);
      layoutData.NodeDescriptors.MapperFunction = node => new HierarchicalLayoutNodeDescriptor {
          LayerAlignment = GetAlignment(node)
      };

      _ = graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(1000), layoutData);
    }

    /// <summary>Returns the layer alignment specified by the first label of the node.</summary>
    private static double GetAlignment(INode node) {
      var text = node.Labels.FirstOrDefault()?.Text.ToLowerInvariant() ?? "center";
      return text switch {
          "top" => 0.0,
          "bottom" => 1.0,
          _ => 0.5
      };
    }

    /// <summary>Creates the sample graph used to demonstrate layout data.</summary>
    private void CreateSampleGraph(IGraph graph) {
      BuildGraphFromJson(graph, "Step11LayoutData.graph-data.json");
    }
  }
}
