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
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.ZOrder;

/// <summary>
///   Interaction logic for ZOrderDemo.xaml
/// </summary>
[Demo("z-order", "view", "Z-Order",
    Summary = "Shows how to adjust the z-order of graph elements and to keep this z-order consistent.",
    Tags = ["rendering", "z-order"],
    Keywords = ["raise", "lower", "to Front", "to Back", "clipboard", "graphml", "z-index"])]
public sealed partial class ZOrderDemo
{
  private ZOrderSupport zOrderSupport = null!;
  private GraphMLSupport graphMLSupport = null!;

  public ZOrderDemo() {
    InitializeComponent();
  }

  /// <summary>
  ///   Bootstraps the demo.
  /// </summary>
  protected override void Initialize() {
    // initialize the graph
    InitializeGraph();

    // initialize consistent z-order support
    zOrderSupport = new ZOrderSupport(graphControl);

    EnableGroupingOperations();

    // use a custom GraphMLIOHandler that supports writing and parsing node z-orders to/from GraphML
    graphMLSupport = new GraphMLSupport(graphControl);
    zOrderSupport.ConfigureGraphMLIOHandler(graphMLSupport.GraphMLIOHandler);
    graphMLSupport.RegisterOpenSaveCommandBindings();

    var inputMode = (GraphEditorInputMode) graphControl.InputMode!;
    inputMode.FocusableItems = GraphItemTypes.None;
    // prevent interactive label changes since they display the z-index in this demo
    inputMode.SelectableItems = GraphItemTypes.Node | GraphItemTypes.Edge;
    inputMode.AllowEditLabel = false;
    inputMode.AllowAddLabel = false;
    inputMode.AllowEditLabelOnDoubleClick = false;

    inputMode.NodeCreated += (_, args) => {
      var node = args.Item;
      UpdateLabel(node, zOrderSupport.GetZOrder(node));
    };

    // if graph structure is changed, update all labels,
    // otherwise the copied labels erroneously show the z-index
    // of the source nodes. (this is for demo purposes only)
    inputMode.ItemsDuplicated += (_, _) => UpdateLabels();
    inputMode.ItemsPasted += (_, _) => UpdateLabels();
    inputMode.NavigationInputMode.GroupExpanded += (_, _) => UpdateLabels();

    zOrderSupport.AddZIndexChangedListener((_, args) => {
      if (args.Item is INode node) {
        UpdateLabel(node, args.NewZIndex);
      }
    });

    CreateGraph();
    UpdateLabels();
    ClearUndoQueue();
    InitializeUI();
  }

  /// <summary>
  ///   Initializes folding and sets default styles.
  /// </summary>
  private void InitializeGraph() {
    // Enable folding
    var view = new FoldingManager().CreateFoldingView();
    graphControl.Graph = view.Graph;

    // Get the master graph instance and enable undo-ability support.
    view.Manager.MasterGraph.SetUndoEngineEnabled(true);
    // add undo support for expand/collapse operations
    view.EnqueueNavigationalUndoUnits = true;

    // set default demo styles
    DemoStyles.InitDemoStyles(graphControl.Graph, foldingEnabled: true);

    graphControl.Graph.NodeDefaults.Size = new SizeD(70, 40);

    var folderNodeConverter = (FolderNodeConverter) view.Manager.FolderNodeConverter;
    folderNodeConverter.FolderNodeDefaults.CopyLabels = true;
  }

  private void CreateGraph() {
    var graph = graphControl.Graph;
    var g1 = graph.CreateGroupNode(null, new RectD(350, 300, 250, 150));
    var n1 = graph.CreateNode(new PointD(400, 400));
    var n2 = graph.CreateNode(new PointD(460, 380));
    var n3 = graph.CreateNode(new PointD(520, 360));
    graph.SetParent(n1, g1);
    graph.SetParent(n2, g1);
    graph.SetParent(n3, g1);

    var g2 = graph.CreateGroupNode(null, new RectD(500, 150, 250, 200));
    var n4 = graph.CreateNode(new PointD(560, 200));
    var n5 = graph.CreateNode(new PointD(590, 230));
    var n6 = graph.CreateNode(new PointD(620, 260));
    graph.SetParent(n4, g2);
    graph.SetParent(n5, g2);
    graph.SetParent(n6, g2);

    var g3 = graph.CreateGroupNode(null, new RectD(650, 225, 350, 300));
    var n7 = graph.CreateNode(new PointD(700, 280));
    var n8 = graph.CreateNode(new PointD(760, 300));
    graph.SetParent(n7, g3);
    graph.SetParent(n8, g3);

    var g4 = graph.CreateGroupNode(g3, new RectD(775, 310, 170, 190));
    var n9 = graph.CreateNode(new PointD(830, 375));
    var n10 = graph.CreateNode(new PointD(880, 405));
    var n11 = graph.CreateNode(new PointD(840, 440));
    graph.SetParent(n9, g4);
    graph.SetParent(n10, g4);
    graph.SetParent(n11, g4);

    // normalize all z orders starting from 0
    zOrderSupport.SetTempNormalizedZOrders(null);
    zOrderSupport.ApplyTempZOrders();

    // clear undo queue to prevent the possibility of undoing the sample graph creation
    graph.GetFoldingView()!.Manager.MasterGraph.GetUndoEngine()?.Clear();
    graphControl.FitGraphBounds();
  }

  /// <summary>
  ///   Updates the labels of all nodes. Called after duplicate or paste
  /// </summary>
  private void UpdateLabels() {
    foreach (var node in graphControl.Graph.Nodes) {
      UpdateLabel(node, zOrderSupport.GetZOrder(node));
    }
  }

  /// <summary>
  ///   Updates the label text to show the current z-index of the node.
  ///   z-Order labels are for demo purposes only and not necessary for
  ///   the z-Order mechanism itself
  /// </summary>
  private void UpdateLabel(INode node, int zIndex) {
    var graph = graphControl.Graph.Contains(node)
        ? graphControl.Graph
        : graphControl.Graph.GetFoldingView()!.Manager.MasterGraph;

    var label = node.Labels.FirstOrDefault(candidate => candidate.Text.StartsWith("Level: ", StringComparison.Ordinal));
    if (label != null) {
      graph.SetLabelText(label, $"Level: {zIndex}");
    } else {
      graph.AddLabel(node, $"Level: {zIndex}");
    }
  }

  /// <summary>
  ///   Enables interactive grouping operations.
  /// </summary>
  private void EnableGroupingOperations() {
    var inputMode = (GraphEditorInputMode) graphControl.InputMode!;
    inputMode.NavigationInputMode.AutoGroupNodeAlignmentPolicy = NodeAlignmentPolicy.TopRight;
  }

  /// <summary>
  ///   Binds actions to the buttons in the toolbar.
  /// </summary>
  private void InitializeUI() {
    graphControl.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (_, _) => {
      var graph = graphControl.Graph;
      zOrderSupport.Clear();
      graph.Clear();
      ClearUndoQueue();
      graphControl.FitGraphBounds();
    }));
  }

  private void ClearUndoQueue() => graphControl.Graph.GetFoldingView()!.Manager.MasterGraph.GetUndoEngine()?.Clear();

  [STAThread]
  private static void Main() {
    Run<ZOrderDemo>();
  }
}
