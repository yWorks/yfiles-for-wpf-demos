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

namespace yWorks.Demo.NodeSelectionResizing
{
  /// <summary>
  ///   Shows how to reshape a selection of nodes as one unit.
  /// </summary>
  [Demo("node-selection-resizing", "input", "Node Selection Resizing",
      Summary = "Shows how to reshape a selection of nodes as one unit.", Tags = ["interaction", "resizing"],
      Keywords = ["input mode", "scale"], RelatedDemos = [])]
  public sealed partial class NodeSelectionResizingDemo
  {
    private readonly NodeSelectionResizingSettings settings = new();
    private NodeSelectionResizingInputMode nodeSelectionResizingInputMode = null!;
    private GraphEditorInputMode graphEditorInputMode = null!;

    public NodeSelectionResizingDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      // Enable undo so group resizes can be reverted as one operation.
      Graph.SetUndoEngineEnabled(true);

      InitializeInputMode();

      // Set minimum and maximum sizes for all non-group nodes. Group nodes should be able to grow larger so they can
      // contain arbitrary numbers of nodes.
      var sizeConstraintProvider = new NodeSizeConstraintProvider(new SizeD(10, 10), new SizeD(100, 100));
      Graph.GetDecorator().Nodes.SizeConstraintProvider.AddFactory(
          node => !Graph.IsGroupNode(node), _ => sizeConstraintProvider);

      InitializeGraph();

      InitializeUI();
    }

    private IGraph Graph => graphControl.Graph;

    private void InitializeInputMode() {
      var graphSnapContext = new GraphSnapContext { Enabled = settings.SnappingEnabled };
      graphEditorInputMode = new GraphEditorInputMode { SnapContext = graphSnapContext };

      // Add a custom input mode that shows a single set of handles when multiple nodes are selected.
      nodeSelectionResizingInputMode = new NodeSelectionResizingInputMode(settings.ResizeMode, new InsetsD(10));
      graphEditorInputMode.Add(nodeSelectionResizingInputMode);
      graphControl.InputMode = graphEditorInputMode;
      graphEditorInputMode.OrthogonalEdgeEditingContext.Enabled = settings.OrthogonalEditingEnabled;
    }

    private void InitializeGraph() {
      DemoStyles.InitDemoStyles(Graph, orthogonalEdgeEditing: true);

      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(Graph, json);

      graphControl.FitGraphBounds();
    }

    private void InitializeUI() {
      demoToolBar.DataContext = settings;
      settings.PropertyChanged += (_, args) => {
        switch (args.PropertyName) {
          case nameof(NodeSelectionResizingSettings.SnappingEnabled):
            graphEditorInputMode.SnapContext.Enabled = settings.SnappingEnabled;
            break;
          case nameof(NodeSelectionResizingSettings.OrthogonalEditingEnabled):
            graphEditorInputMode.OrthogonalEdgeEditingContext.Enabled = settings.OrthogonalEditingEnabled;
            break;
          case nameof(NodeSelectionResizingSettings.ResizeMode):
            nodeSelectionResizingInputMode.Mode = settings.ResizeMode;
            break;
        }
      };
    }

    [STAThread]
    private static void Main() {
      Run<NodeSelectionResizingDemo>();
    }
  }
}
