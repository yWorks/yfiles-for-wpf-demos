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
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.GraphDecorator
{
  /// <summary>
  ///   Shows how to use the graph decorator concept to customize the behavior and the visualization of graph items.
  /// </summary>
  [Demo("graph-decorator", "application-features", "Graph Decorator",
      Summary = "Shows how to decorate graph items to change their behavior or visualization.", Tags = ["interaction"],
      Keywords = [
          "GraphDecorator", "NodeDecorator", "ILookupDecorator", "ports", "IPortCandidate", "port candidate providers"
      ])]
  public sealed partial class GraphDecoratorDemo
  {
    public GraphDecoratorDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override void Initialize() {
      graphControl.InputMode = new GraphEditorInputMode();

      // configures default styles for newly created graph elements
      InitializeGraph(graphControl.Graph);

      // then build the graph with the given data set
      BuildGraph(graphControl.Graph);

      graphControl.Graph.ApplyLayout(new HierarchicalLayout { MinimumLayerDistance = 35 });
      graphControl.FitGraphBounds();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);

      // configure port candidate provider
      ConfigurePortCandidateProvider(graphControl.Graph);
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
    ///   Configures custom port candidates by utilizing the portCandidateProvider of the <see cref="NodeDecorator"/>.
    /// </summary>
    private void ConfigurePortCandidateProvider(IGraph graph) {
      // Don't remove unoccupied ports.
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      // First, get the GraphDecorator from the IGraph.
      // GraphDecorator is a utility class that aids in decorating
      // functionality to model items from a graph instance.
      var decorator = graph.GetDecorator();

      // Here, we obtain the nodeDecorator.portCandidateProvider
      // to access the lookup decorator that handles ports candidates at nodes.
      // Basically this means that if any INode instance in the graph is queried for the IPortCandidateProvider interface, the query will be handled by our code below.
      var portCandidateProvider = decorator.Nodes.PortCandidateProvider;

      // One way to decorate the graph is to use the factory design pattern.
      // For each INode in the graph that will be queried for the IPortCandidateProvider interface, the below
      // factory method will be called with the node instance as argument.
      // We set the factory to a function expression which
      // returns a custom instance that implements the IPortCandidateProvider interface.

      // PortCandidateProviders.Combine combines various port candidate providers.
      // PortCandidateProviders.FromExistingPorts provides port candidates at the locations of already existing ports.
      // PortCandidateProviders.FromNodeCenter provides a single port candidate at the center of the node.
      // PortCandidateProviders.FromShapeGeometry provides several port candidates based on the shape of the node's style.
      portCandidateProvider.AddFactory(node =>
          graph.IsGroupNode(node)
              ? PortCandidateProviders.Combine(PortCandidateProviders.FromShapeGeometry(node, 0.5))
              : PortCandidateProviders.Combine(PortCandidateProviders.FromExistingPorts(node),
                  PortCandidateProviders.FromNodeCenter(node), PortCandidateProviders.FromShapeGeometry(node, 0.5)));
    }

    /// <summary>
    ///   Initializes the defaults for the styling in this demo.
    /// </summary>
    /// <param name="graph">The graph.</param>
    private static void InitializeGraph(IGraph graph) {
      // set styles for this demo
      DemoStyles.InitDemoStyles(graph, Themes.Palette12, orthogonalEdgeEditing: true);
    }

    private void OnNewGraphExecuted(object sender, ExecutedRoutedEventArgs e) {
      graphControl.Graph.Clear();
      graphControl.Graph.GetUndoEngine()?.Clear();
      graphControl.FitGraphBounds();
      e.Handled = true;
    }

    [STAThread]
    private static void Main() {
      Run<GraphDecoratorDemo>();
    }
  }
}
