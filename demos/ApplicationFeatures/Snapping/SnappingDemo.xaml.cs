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
using System.Text.Json;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.Snapping
{
  /// <summary>
  ///   This demo shows how to enable snapping (guide lines) for graph elements.
  /// </summary>
  [Demo("snapping", "application-features", "Snapping",
      Summary = "Shows how to enable snapping (guide lines) for interactive changes.",
      Tags = ["interaction", "snapping"], Keywords = ["move", "resize", "resizing"])]
  public sealed partial class SnappingDemo
  {
    public SnappingDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      // initialize graph component
      graphControl.InputMode = new GraphEditorInputMode();

      // configures default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // enable snapping for labels and other graph items
      InitializeSnapping();

      // build the graph from the given data set
      BuildGraph(graphControl.Graph);

      // layout and center the graph
      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json);
    }

    /// <summary>
    ///   Initializes snapping for labels and other graph items. The default snapping behavior can easily be enabled by
    ///   setting the according snap context. Those snap contexts provide many options to fine tune their behavior. Please
    ///   see the documentation of <see cref="GraphSnapContext"/> for more information.
    /// </summary>
    private void InitializeSnapping() {
      var inputMode = (GraphEditorInputMode) graphControl.InputMode!;
      inputMode.SnapContext = new GraphSnapContext();
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph);

      var groupNodeStyle = (GroupNodeStyle) graph.GroupNodeDefaults.Style;
      groupNodeStyle.TabPosition = GroupNodeStyleTabPosition.Right;
      groupNodeStyle.CornerRadius = 8;

      // set sizes and locations specific for this demo
      graph.NodeDefaults.Size = new SizeD(40, 40);
      graph.NodeDefaults.Labels.LayoutParameter =
          new ExteriorNodeLabelModel { Margins = new InsetsD(5) }.CreateParameter(
              ExteriorNodeLabelModel.Position.Bottom);
      graph.EdgeDefaults.Labels.LayoutParameter = new EdgePathLabelModel {
          Distance = 5,
          AutoRotation = true
      }.CreateRatioParameter(sideOfEdge: EdgeSides.BelowEdge);
    }

    [STAThread]
    public static void Main() {
      Run<SnappingDemo>();
    }
  }
}
