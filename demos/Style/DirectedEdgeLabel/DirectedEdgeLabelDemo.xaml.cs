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
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.DirectedEdgeLabel
{
  /// <summary>
  ///   Shows a custom label style which visualizes an edge label with an additional arrow that points in the
  ///   direction of the edge label flow.
  /// </summary>
  [Demo("directed-edge-label-style", "style", "Directed Edge Label Style",
      Summary = "Shows label styles displaying arrows that always point to the source or target port.",
      Tags = ["style", "label", "edge"], Keywords = ["edges", "directed", "labels"])]
  public sealed partial class DirectedEdgeLabelDemo
  {
    private static readonly DirectedEdgeLabelStyle ToSourceStyle = new(true) { ArrowFill = Brushes.Red };
    private static readonly DirectedEdgeLabelStyle ToTargetStyle = new() { ArrowFill = Brushes.Green };

    public DirectedEdgeLabelDemo() {
      InitializeComponent();
    }

    private IGraph Graph => graphControl.Graph;

    protected override void Initialize() {
      DemoStyles.InitDemoStyles(Graph, Themes.Palette31, edgeTheme: Themes.Palette31, edgeLabelTheme: Themes.Palette31,
          orthogonalEdgeEditing: true);
      var labelStyle = (LabelStyle) Graph.EdgeDefaults.Labels.Style;
      labelStyle.MinimumSize = new SizeD(0, 22);

      // Initialize the input mode
      graphControl.InputMode = new GraphEditorInputMode {
          AllowEditLabel = true,
          AllowAddLabel = false,
          DeletableItems = GraphItemTypes.All & ~GraphItemTypes.Label
      };

      Graph.EdgeCreated += (_, args) => AddLabels(Graph, args.Item);

      // Create some graph elements
      CreateSampleGraph(Graph);
      graphControl.FitGraphBounds();
    }

    private static void AddLabels(IGraph graph, IEdge edge) {
      var labelModel = new EdgePathLabelModel(0, 0, 0, false, EdgeSides.OnEdge);
      graph.AddLabel(edge, "To Source", labelModel.CreateRatioParameter(0), ToSourceStyle);

      graph.AddLabel(edge, "Center", labelModel.CreateRatioParameter());

      graph.AddLabel(edge, "To Target", labelModel.CreateRatioParameter(1), ToTargetStyle);
    }

    /// <summary>
    ///   Creates the initial sample graph.
    /// </summary>
    private static void CreateSampleGraph(IGraph graph) {
      var firstEdge = graph.CreateEdge(
          graph.CreateNode(new PointD(-100, -100)), graph.CreateNode(new PointD(-200, 100)));
      graph.AddBend(firstEdge, new PointD(-100, 0));
      graph.AddBend(firstEdge, new PointD(-200, 0));

      var secondEdge = graph.CreateEdge(
          graph.CreateNode(new PointD(100, -100)), graph.CreateNode(new PointD(200, 100)));
      graph.AddBend(secondEdge, new PointD(100, 0));
      graph.AddBend(secondEdge, new PointD(200, 0));

      graph.CreateEdge(graph.CreateNode(new PointD(-200, 150)), graph.CreateNode(new PointD(200, 150)));
    }

    [STAThread]
    public static void Main() {
      Run<DirectedEdgeLabelDemo>();
    }
  }
}
