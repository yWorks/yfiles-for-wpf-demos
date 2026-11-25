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
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step01CreateAPolyline;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step02CropThePolyline;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step03CreateParallelPolylines;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step04RenderPerformance;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step05ConfigurableStyle;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step06DataFromTag;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step07HitTesting;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step08Visibility;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step09ItemBounds;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step10BridgeSupport;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step11AddingArrows;
using yWorks.Demo.Tutorial.StyleImplementationEdge.Step12CustomArrowVisualization;
using yWorks.Geometry;

namespace yWorks.Demo.Tutorial.StyleImplementationEdge
{
  [Demo("tutorial-style-implementation-edge", "Tutorials", "Tutorial: Edge Style Implementation",
      Summary = "Learn how to implement a custom edge style using DrawingVisual.",
      Tags = ["edgestyle", "DrawingVisual"], Keywords = ["tutorial", "custom", "styling"])]
  public class CustomEdgeStyleTutorial : TutorialBase
  {
    /// <summary>Registers the tutorial steps that have been migrated so far.</summary>
    protected override void AddTutorialSteps() {
      AddTutorialStep(new CreateAPolylineStep());
      AddTutorialStep(new CropThePolylineStep());
      AddTutorialStep(new CreateParallelPolylinesStep());
      AddTutorialStep(new RenderPerformanceStep());
      AddTutorialStep(new ConfigurableStyleStep());
      AddTutorialStep(new DataFromTagStep());
      AddTutorialStep(new HitTestingStep());
      AddTutorialStep(new VisibilityStep());
      AddTutorialStep(new ItemBoundsStep());
      AddTutorialStep(new BridgeSupportStep());
      AddTutorialStep(new AddingArrowsStep());
      AddTutorialStep(new CustomArrowVisualizationStep());
    }

    /// <summary>Creates the two-node graph used by the first step.</summary>
    public static void CreateSimpleGraph(GraphControl graphControl, bool opaque = false) {
      var graph = graphControl.Graph;

      var nodeStyle = DemoStyles.CreateTutorialNodeStyle();
      nodeStyle.Pen = CreatePen(Color.FromRgb(4, 45, 55));
      nodeStyle.Brush = CreateBrush(Color.FromArgb(opaque ? (byte) 255 : (byte) 136, 11, 113, 137));
      graph.NodeDefaults.Style = nodeStyle;

      var node1 = graph.CreateNode();
      var node2 = graph.CreateNode(new RectD(100, 200, 30, 30));
      var edge = graph.CreateEdge(node1, node2);
      graph.AddBend(edge, new PointD(90, 60));
      graph.AddBend(edge, new PointD(10, 170));
    }

    /// <summary>Disables graph-item creation for this visualization step.</summary>
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

    private static Brush CreateBrush(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }

    private static Pen CreatePen(Color color) {
      var pen = new Pen(CreateBrush(color), 1);
      pen.Freeze();
      return pen;
    }

    [STAThread]
    public static void Main() {
      RunTutorial<CustomEdgeStyleTutorial>();
    }
  }
}
