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
using yWorks.Controls.Input;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step01RenderLabelText;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step02UsingTextUtilities;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step03AddBackgroundShape;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step04PreferredSize;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step05RenderPerformance;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step06TextAlignment;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step07LineWrapping;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step08ItemSpecificCustomization;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step09HitTesting;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step10Visibility;
using yWorks.Demo.Tutorial.StyleImplementationLabel.Step11CustomBounds;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.Tutorial.StyleImplementationLabel
{
  [Demo("tutorial-style-implementation-label", "Tutorials", "Tutorial: Label Style Implementation",
      Summary = "Learn how to implement a custom label style using WPF drawing APIs.",
      Tags = ["labelstyle", "text", "FormattedText"], Keywords = ["tutorial", "custom", "label", "styling"])]
  public class CustomLabelStyleTutorial : TutorialBase
  {
    /// <summary>Registers the tutorial steps that have been migrated so far.</summary>
    protected override void AddTutorialSteps() {
      AddTutorialStep(new RenderLabelTextStep());
      AddTutorialStep(new UsingTextUtilitiesStep());
      AddTutorialStep(new AddBackgroundShapeStep());
      AddTutorialStep(new PreferredSizeStep());
      AddTutorialStep(new RenderPerformanceStep());
      AddTutorialStep(new TextAlignmentStep());
      AddTutorialStep(new LineWrappingStep());
      AddTutorialStep(new ItemSpecificCustomizationStep());
      AddTutorialStep(new HitTestingStep());
      AddTutorialStep(new VisibilityStep());
      AddTutorialStep(new CustomBoundsStep());
    }

    /// <summary>Initializes the default layout parameters for node and edge labels.</summary>
    public static void InitializeLabelModel(GraphControl graphControl) {
      var graph = graphControl.Graph;
      var smartEdgeLabelModel = new SmartEdgeLabelModel();
      graph.EdgeDefaults.Labels.LayoutParameter = smartEdgeLabelModel.CreateParameterFromSource(0, -5);

      var exteriorNodeLabelModel = new ExteriorNodeLabelModel { Margins = new InsetsD(5) };
      graph.NodeDefaults.Labels.LayoutParameter =
          exteriorNodeLabelModel.CreateParameter(ExteriorNodeLabelModel.Position.Top);
    }

    /// <summary>Creates the two nodes and labels used in the first step.</summary>
    public static void CreateSimpleGraph(IGraph graph) {
      var node1 = graph.CreateNode();
      graph.AddLabel(node1, "Node Label 1");

      var node2 = graph.CreateNode(new RectD(100, 200, 30, 30));
      graph.AddLabel(node2, "Node Label 2");

      var edge = graph.CreateEdge(node1, node2);
      graph.AddLabel(edge, "Edge Label");
    }

    /// <summary>Disables graph-item creation for this visualization step.</summary>
    public static void EnableGraphEditing(GraphControl graphControl) {
      graphControl.InputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false
      };
    }

    /// <summary>Fits the graph after the control has completed its initial layout.</summary>
    public static void FitGraphBounds(GraphControl graphControl) {
      graphControl.Dispatcher.InvokeAsync(() => {
        graphControl.LimitFitContentZoom = false;
        graphControl.FitGraphBounds();
      }, DispatcherPriority.Loaded);
    }

    [STAThread]
    public static void Main() {
      RunTutorial<CustomLabelStyleTutorial>();
    }
  }
}
