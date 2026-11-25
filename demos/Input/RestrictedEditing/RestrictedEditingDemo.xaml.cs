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
using System.IO;
using System.Linq;
using System.Text.Json;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;

namespace yWorks.Demo.RestrictedEditing
{
  /// <summary>
  ///   Shows how to restrict interactive editing while still using <see cref="GraphEditorInputMode"/>
  ///   for its non-editing features like click-selecting items.
  /// </summary>
  [Demo("restricted-editing", "input", "Restricted Editing",
      Summary = "Shows how to restrict interactive editing with GraphEditorInputMode.", Tags = ["interaction"],
      Keywords = ["editing", "viewing"], RelatedDemos = [])]
  public sealed partial class RestrictedEditingDemo
  {
    private readonly Settings settings = new();
    private GraphEditorInputMode graphEditorInputMode = null!;

    public RestrictedEditingDemo() {
      InitializeComponent();
    }

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override void Initialize() {
      // initially disable interactive editing
      ConfigureUserInteraction(GraphControl);

      // configure default styles for the demo's graph
      DemoStyles.InitDemoStyles(Graph);

      // create the demo's sample graph
      CreateSampleGraph(Graph);

      // center the demo's graph in the demo's visible area
      GraphControl.FitGraphBounds();

      // enable undo and redo
      Graph.SetUndoEngineEnabled(true);

      // bind the demo's UI controls to their respective actions
      InitializeUI();
    }

    /// <summary>
    ///   Creates the sample graph for this demo.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      var data = LoadGraphData();
      var builder = new GraphBuilder(graph);

      var nodesSource = builder.CreateNodesSource(data.Nodes.Where(node => !node.IsGroup), node => node.Id);
      nodesSource.ParentIdProvider = node => node.Parent;
      nodesSource.NodeCreator.LayoutProvider = node => ToRect(node.Layout);

      var groupNodesSource = builder.CreateGroupNodesSource(data.Nodes.Where(node => node.IsGroup), node => node.Id);
      groupNodesSource.ParentIdProvider = node => node.Parent;
      groupNodesSource.NodeCreator.LayoutProvider = node => ToRect(node.Layout);

      var edgesSource =
          builder.CreateEdgesSource(data.Edges, edge => edge.Source, edge => edge.Target, edge => edge.Id);
      edgesSource.EdgeCreator.BendsProvider = edge => edge.Bends.Select(bend => bend.ToPoint());

      builder.BuildGraph();
    }

    /// <summary>
    ///   Loads and deserializes the sample graph data embedded as <c>graph-data.json</c>.
    /// </summary>
    private GraphData LoadGraphData() {
      using var stream = LoadAssemblyResource("Resources.graph-data.json") ??
          throw new InvalidOperationException("Could not load the embedded graph data.");
      return JsonSerializer.Deserialize<GraphData>(stream,
              new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
          throw new InvalidDataException("The embedded graph data is empty.");
    }

    private static RectD ToRect(double[] layout) => new(layout[0], layout[1], layout[2], layout[3]);

    /// <summary>
    ///   Registers a <see cref="GraphEditorInputMode"/> instance that does not allow interactive editing of
    ///   the demo's graph initially.
    /// </summary>
    /// <param name="graphControl">The demo's main graph view.</param>
    private void ConfigureUserInteraction(GraphControl graphControl) {
      graphEditorInputMode = new GraphEditorInputMode {
          AllowAddLabel = false,
          AllowAdjustGroupNodeSize = false,
          AllowClipboardOperations = false,
          AllowCreateBend = false,
          AllowCreateEdge = false,
          AllowCreateNode = false,
          AllowDuplicate = false,
          AllowEditLabel = false,
          AllowEditLabelOnDoubleClick = false,
          AllowGroupingOperations = false,
          AllowGroupSelection = false,
          AllowPaste = false,
          AllowReparentNodes = false,
          AllowReparentToNonGroupNodes = false,
          AllowReverseEdge = false,
          AllowUndoOperations = false,
          AllowUngroupSelection = false,
          DeletableItems = GraphItemTypes.None,
          // suppressing handles effectively turns off
          // - node resizing
          // - edge reconnection
          // - bend movement
          ShowHandleItems = GraphItemTypes.None,
          MoveSelectedItemsInputMode = { Enabled = false },
          MoveUnselectedItemsInputMode = { Enabled = false }
      };
      graphControl.InputMode = graphEditorInputMode;

      // register position handlers on edges so that they can be moved
      graphControl.Graph.GetDecorator().Edges.PositionHandler.AddFactory(edge => new EdgePositionHandler(edge));
    }

    /// <summary>
    ///   Configures interactive editing according to the given operations value.
    /// </summary>
    /// <param name="operations">The editing operations to enable or disable.</param>
    /// <remarks>
    ///   <see cref="EditingOperations.None"/> disables all interactive editing operations;
    ///   <see cref="EditingOperations.MovingItems"/> enables moving items only; and
    ///   <see cref="EditingOperations.All"/> enables all interactive editing operations.
    /// </remarks>
    private void ConfigureEditing(EditingOperations operations) {
      if (operations == EditingOperations.None) {
        AllowEditing(graphEditorInputMode, false);
      } else if (operations == EditingOperations.MovingItems) {
        AllowEditing(graphEditorInputMode, false);
        graphEditorInputMode.ShowHandleItems = GraphItemTypes.Bend;
        graphEditorInputMode.MoveSelectedItemsInputMode.Enabled = true;
        graphEditorInputMode.MoveUnselectedItemsInputMode.Enabled = true;
      } else {
        AllowEditing(graphEditorInputMode, true);
      }
    }

    /// <summary>
    ///   Turns interactive editing on or off.
    /// </summary>
    /// <param name="inputMode">The input mode responsible for interactive editing.</param>
    /// <param name="enabled">If true, interactive editing is enabled; otherwise it is disabled.</param>
    private static void AllowEditing(GraphEditorInputMode inputMode, bool enabled) {
      inputMode.AllowAddLabel = enabled;
      inputMode.AllowAdjustGroupNodeSize = enabled;
      inputMode.AllowClipboardOperations = enabled;
      inputMode.AllowCreateBend = enabled;
      inputMode.AllowCreateEdge = enabled;
      inputMode.AllowCreateNode = enabled;
      inputMode.AllowDuplicate = enabled;
      inputMode.AllowEditLabel = enabled;
      inputMode.AllowEditLabelOnDoubleClick = enabled;
      inputMode.AllowGroupingOperations = enabled;
      inputMode.AllowGroupSelection = enabled;
      inputMode.AllowPaste = enabled;
      inputMode.AllowReparentNodes = enabled;
      inputMode.AllowReparentToNonGroupNodes = enabled;
      inputMode.AllowReverseEdge = enabled;
      inputMode.AllowUndoOperations = enabled;
      inputMode.AllowUngroupSelection = enabled;

      inputMode.DeletableItems = enabled ? GraphItemTypes.All : GraphItemTypes.None;
      inputMode.ShowHandleItems = enabled ? GraphItemTypes.All : GraphItemTypes.None;

      inputMode.MoveSelectedItemsInputMode.Enabled = enabled;
      inputMode.MoveUnselectedItemsInputMode.Enabled = enabled;
    }

    /// <summary>
    ///   Binds actions to the demo's UI controls.
    /// </summary>
    private void InitializeUI() {
      demoToolBar.DataContext = settings;
      settings.SettingChanged += (_, _) => ConfigureEditing(settings.EditingOperation);
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<RestrictedEditingDemo>();
    }
  }
}
