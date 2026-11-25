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
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;

namespace yWorks.Demo.LabelHandleProvider
{
  /// <summary>
  ///   Shows how to implement custom handles that allow interactive resizing and rotation of labels.
  /// </summary>
  [Demo("label-handle-provider", "input", "Label Handle Provider",
      Summary = "Shows how to implement custom handles that allow interactive resizing and rotation of labels.",
      Tags = ["interaction", "label"], Keywords = ["handles"])]
  public sealed partial class LabelHandleProviderDemo
  {
    public LabelHandleProviderDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      InitializeGraph(graphControl.Graph);
      InitializeInputMode();
    }

    /// <summary>
    ///   Sets the default styles and creates the graph.
    /// </summary>
    private void InitializeGraph(IGraph graph) {
      // add a custom handle provider for labels
      graph.GetDecorator().Labels.HandleProvider.AddFactory(GetLabelHandleProvider);

      var labelDecorationRenderer = new LabelStyleIndicatorRenderer {
          LabelStyle = new RotatableLabelSelectionStyle(),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      };

      graph.GetDecorator().Labels.SelectionRenderer.AddConstant(
          label => label.LayoutParameter.Model is FreeNodeLabelModel or FreeEdgeLabelModel or FreeLabelModel,
          labelDecorationRenderer);

      DemoStyles.InitDemoStyles(graph);
      graph.NodeDefaults.Size = new SizeD(100, 50);

      // Our resize logic does not work together with all label models resp. label model parameters
      // for simplicity, we just use a centered label for nodes
      var compositeLabelModel = new CompositeLabelModel();
      compositeLabelModel.AddParameter(InteriorNodeLabelModel.Center);
      graph.NodeDefaults.Labels.LayoutParameter = compositeLabelModel.Parameters.First();

      var edgeLabelModel = new EdgeSegmentLabelModel { Distance = 10 };
      graph.EdgeDefaults.Labels.LayoutParameter =
          edgeLabelModel.CreateParameterFromSource(0, 0.5, EdgeSides.RightOfEdge);

      // create sample graph
      var n1 = graph.CreateNode(new PointD(100, 100));
      graph.AddLabel(n1, "Centered Node Label. Resizes symmetrically.");

      var rotatingLabelStyle = DemoStyles.CreateDemoNodeLabelStyle(Themes.Palette13);
      rotatingLabelStyle.AutoFlip = false;
      var n2 = graph.CreateNode(new PointD(500, 0));
      graph.AddLabel(n2, "Free Node Label.\nSupports rotation and asymmetric resizing",
          new FreeNodeLabelModel().CreateParameter(new PointD(0.5, 0.5), new PointD(0, 0), new PointD(0.5, 0.5)),
          rotatingLabelStyle);
      var edge = graph.CreateEdge(n2, n1);
      graph.AddLabel(edge, "Rotated Edge Label");

      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Returns the LabelHandleProvider for the given label.
    /// </summary>
    /// <param name="label">The given label.</param>
    private LabelHandleProvider GetLabelHandleProvider(ILabel label) => new(graphControl, label);

    /// <summary>
    ///   Initializes the input mode.
    /// </summary>
    private void InitializeInputMode() {
      var mode = new GraphEditorInputMode {
          ClickHitTestOrder = [
              GraphItemTypes.EdgeLabel,
              GraphItemTypes.NodeLabel,
              GraphItemTypes.Bend,
              GraphItemTypes.Edge,
              GraphItemTypes.Node,
              GraphItemTypes.Port,
              GraphItemTypes.All
          ]
      };

      // add a label to each created node
      mode.NodeCreated += (_, args) => {
        var graph = graphControl.Graph;
        graph.AddLabel(args.Item, $"Node {graph.Nodes.Count}");
      };

      graphControl.InputMode = mode;
    }

    [STAThread]
    public static void Main() {
      Run<LabelHandleProviderDemo>();
    }
  }
}
