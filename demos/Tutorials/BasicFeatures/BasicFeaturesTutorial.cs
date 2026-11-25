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
using yWorks.Demo.Tutorial.BasicFeatures.Step01CreatingTheView;
using yWorks.Demo.Tutorial.BasicFeatures.Step02GraphElementCreation;
using yWorks.Demo.Tutorial.BasicFeatures.Step03ManagingViewport;
using yWorks.Demo.Tutorial.BasicFeatures.Step04SettingStyles;
using yWorks.Demo.Tutorial.BasicFeatures.Step05LabelPlacement;
using yWorks.Demo.Tutorial.BasicFeatures.Step06BasicInteraction;
using yWorks.Demo.Tutorial.BasicFeatures.Step07UndoClipboardSupport;
using yWorks.Demo.Tutorial.BasicFeatures.Step08Grouping;
using yWorks.Demo.Tutorial.BasicFeatures.Step09DataBinding;
using yWorks.Demo.Tutorial.BasicFeatures.Step10Layout;
using yWorks.Demo.Tutorial.BasicFeatures.Step11LayoutData;
using yWorks.Demo.Tutorial.BasicFeatures.Step12GraphAnalysis;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

namespace yWorks.Demo.Tutorial.BasicFeatures
{
  [Demo("tutorial-basic-features", "Tutorials", "Tutorial: Basic Features",
      Summary = "Learn about the most commonly used yFiles features.", Tags = new[] { "tutorial", "basic features" })]
  public class BasicFeaturesTutorial : TutorialBase
  {
    /// <summary>
    ///   Registers all tutorial steps.
    /// </summary>
    protected override void AddTutorialSteps() {
      AddTutorialStep(new CreatingTheViewStep());
      AddTutorialStep(new GraphElementCreationStep());
      AddTutorialStep(new ManagingViewportStep());
      AddTutorialStep(new SettingStylesStep());
      AddTutorialStep(new LabelPlacementStep());
      AddTutorialStep(new BasicInteractionStep());
      AddTutorialStep(new UndoClipboardSupportStep());
      AddTutorialStep(new GroupingStep());
      AddTutorialStep(new DataBindingStep());
      AddTutorialStep(new LayoutStep());
      AddTutorialStep(new LayoutDataStep());
      AddTutorialStep(new GraphAnalysisStep());
    }

    /// <summary>
    ///   Creates a sample graph and introduces all important graph elements present in yFiles.
    ///   Additionally, this method overrides the label placement for some specific labels.
    /// </summary>
    public static void CreateSampleGraph(IGraph graph) {
      var node1 = graph.CreateNode(new PointD(30, 30));
      var node2 = graph.CreateNode(new PointD(170, 30));
      var node3 = graph.CreateNode(new RectD(230, 200, 60, 30));

      graph.CreateEdge(node1, node2);
      var edge2 = graph.CreateEdge(node2, node3);

      graph.AddBend(edge2, new PointD(260, 30));

      var port1AtNode1 = graph.AddPort(node1, FreeNodePortLocationModel.Center);

      var port1AtNode3 = graph.AddPort(node3, new PointD(node3.Layout.X, node3.Layout.GetCenter().Y));

      var edgeAtPorts = graph.CreateEdge(port1AtNode1, port1AtNode3);

      // Adds labels to several graph elements
      graph.AddLabel(node1, "n1");
      graph.AddLabel(node2, "n2");
      graph.AddLabel(node3, "n3");
      graph.AddLabel(edgeAtPorts, "Edge at Ports");
    }

    [STAThread]
    public static void Main() {
      RunTutorial<BasicFeaturesTutorial>();
    }
  }
}
