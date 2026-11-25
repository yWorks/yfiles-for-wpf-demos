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
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.GraphCopy
{
  /// <summary>
  ///   Shows how to use the <see cref="GraphCopier"/> to copy a graph or part of it to another graph.
  /// </summary>
  [Demo("graph-copy", "application-features", "Graph Copy", Summary = "Shows how to copy a graph or parts of a graph.",
      Tags = ["clipboard"], Keywords = ["GraphCopier", "copy", "cut", "paste"])]
  public sealed partial class GraphCopyDemo
  {
    public GraphCopyDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode();
      copyGraphControl.InputMode = new GraphViewerInputMode();

      // configures default styles for newly created original graph and the copy graph elements
      InitializeGraph(graphControl.Graph);
      InitializeGraph(copyGraphControl.Graph);

      // then build the graph from the given data set
      BuildGraph(graphControl.Graph);

      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);

      // bind the buttons to their functionality
      InitializeUI();
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
    ///   Determines if the <em>copy</em> command created in <see cref="InitializeUI"/> may be executed.
    /// </summary>
    private bool CanCopyGraph() => graphControl.Selection.Count > 0;

    /// <summary>
    ///   Copy the selected part of the original graph to another graph.
    /// </summary>
    private void CopyGraph() {
      var graphCopier = new GraphCopier();
      copyGraphControl.Graph.Clear();

      graphCopier.Copy(graphControl.Graph, copyGraphControl.Graph, item => {
        var selection = graphControl.Selection;
        if (item is INode or IEdge) {
          // copy selected node or edge
          // note that only edges for which both source and edge are actually in the graph
          // are ultimately copied by the copier
          return selection.Contains(item);
        }
        if (item is IPort port) {
          // ports, bends, and labels are copied if they belong to a selected item
          // note that edges are not copied if their ports are not copied also
          return selection.Contains(port.Owner);
        }
        if (item is IBend bend) {
          return selection.Contains(bend.Owner);
        }
        if (item is ILabel label) {
          return selection.Contains(label.Owner);
        }
        return false;
      });
      copyGraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph);

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

    /// <summary>
    ///   Clear the graph in the given graph control.
    /// </summary>
    private static void ClearGraph(GraphControl graphControl) {
      graphControl.Graph.Clear();
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Binds actions to the buttons in the tutorial's toolbar.
    /// </summary>
    private void InitializeUI() {
      var geim = (GraphEditorInputMode) graphControl.InputMode!;
      // disable edit on typing not to interfere with our key-binding
      geim.AllowEditLabelOnTyping = false;

      var kim = geim.KeyboardInputMode;
      kim.AddCommandBinding(ApplicationCommands.Copy, (_, executeArgs) => {
        CopyGraph();
        executeArgs.Handled = true;
      }, (_, canExecuteArgs) => {
        canExecuteArgs.CanExecute = CanCopyGraph();
        canExecuteArgs.Handled = true;
      });
      kim.AddKeyBinding(Key.C, ModifierKeys.None, () => {
        if (CanCopyGraph()) {
          CopyGraph();
        }
      });
      kim.AddCommandBinding(ApplicationCommands.New, (sender, _) => ClearGraph((GraphControl) sender));

      copyGraphControl.CommandBindings.Add(new CommandBinding(ApplicationCommands.New,
          (_, _) => ClearGraph(copyGraphControl)));
    }

    [STAThread]
    public static void Main() {
      Run<GraphCopyDemo>();
    }
  }
}
