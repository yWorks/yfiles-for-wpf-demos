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
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step01CreateARectangle;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step02CreateACustomShape;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step03RenderPerformance;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step04ConfigurableStyle;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step05DataFromTag;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step06RenderText;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step07HitTesting;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step08EdgeCropping;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step09Visibility;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step10RenderBoundaries;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step11GroupNodeStyle;
using yWorks.Demo.Tutorial.StyleImplementationNode.Step12GroupNodeStyleBehavior;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Tutorial.StyleImplementationNode
{
  [Demo("tutorial-style-implementation-node", "Tutorials", "Tutorial: Node Style Implementation",
      Summary = "Learn how to implement a custom node style using DrawingVisual.",
      Tags = ["nodestyle", "DrawingVisual"], Keywords = ["tutorial", "custom", "styling"])]
  public class CustomNodeStyleTutorial : TutorialBase
  {
    /// <summary>Registers the tutorial steps that have been migrated so far.</summary>
    protected override void AddTutorialSteps() {
      AddTutorialStep(new CreateARectangleStep());
      AddTutorialStep(new CreateACustomShapeStep());
      AddTutorialStep(new RenderPerformanceStep());
      AddTutorialStep(new ConfigurableStyleStep());
      AddTutorialStep(new DataFromTagStep());
      AddTutorialStep(new RenderTextStep());
      AddTutorialStep(new HitTestingStep());
      AddTutorialStep(new EdgeCroppingStep());
      AddTutorialStep(new VisibilityStep());
      AddTutorialStep(new RenderBoundariesStep());
      AddTutorialStep(new GroupNodeStyleStep());
      AddTutorialStep(new GroupNodeStyleBehaviorStep());
    }

    /// <summary>Initializes the default styles for nodes, edges, and labels.</summary>
    public static void InitializeTutorialDefaults(GraphControl graphControl) {
      DemoStyles.InitTutorialStyles(graphControl.Graph);
    }

    /// <summary>Creates the single rectangular node used in the first step.</summary>
    public static void CreateSimpleGraph(IGraph graph) {
      graph.CreateNode(new RectD(0, 0, 100, 50));
    }

    /// <summary>Disables graph-item creation for this introductory visualization step.</summary>
    public static void EnableGraphEditing(GraphControl graphControl) {
      graphControl.InputMode = new GraphEditorInputMode {
          AllowCreateEdge = false, AllowCreateNode = false, AllowAddLabel = false
      };
    }

    /// <summary>Fits the graph after the control has completed its initial layout.</summary>
    public static void FitGraphBounds(GraphControl graphControl, double minimumZoom = 3) {
      graphControl.Dispatcher.InvokeAsync(() => {
        graphControl.LimitFitContentZoom = false;
        graphControl.FitGraphBounds();
        graphControl.Zoom = Math.Min(graphControl.Zoom, minimumZoom);
      }, DispatcherPriority.Loaded);
    }

    /// <summary>Creates and freezes a solid brush for custom node styles.</summary>
    internal static Brush CreateBrush(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }

    /// <summary>Creates and freezes a one-pixel pen for custom node styles.</summary>
    internal static Pen CreatePen(Color color) {
      var pen = new Pen(CreateBrush(color), 1);
      pen.Freeze();
      return pen;
    }

    [STAThread]
    public static void Main() {
      RunTutorial<CustomNodeStyleTutorial>();
    }
  }
}
