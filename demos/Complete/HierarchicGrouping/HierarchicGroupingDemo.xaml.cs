/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.yFiles.Toolkit;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;
using RoutingStyle = yWorks.Layout.Hierarchic.RoutingStyle;

namespace Demo.yFiles.Graph.HierarchicGrouping
{
  /// <summary>
  /// A demo that demonstrates how to automatically trigger an incremental layout when opening or closing groups.
  /// </summary>
  public partial class HierarchicGroupingDemo
  {
    private HashSet<INode> incrementalNodes;
    private HashSet<IEdge> incrementalEdges;
    private IFoldingView foldingView;
    private DictionaryMapper<INode, RectD?> fixedGroupNodeLayout;

    public HierarchicGroupingDemo() {
      InitializeComponent();
    }

    private async Task ApplyLayout() {
      // create a pre-configured HierarchicLayout
      var hl = CreateHierarchicLayout();
      // rearrange only the incremental graph elements, the
      // remaining elements are not, or only slightly, changed
      hl.LayoutMode = LayoutMode.Incremental;

      // provide additional data to configure the HierarchicLayout
      var hlData = new HierarchicLayoutData();
      // specify the nodes to rearrange
      hlData.IncrementalHints.IncrementalLayeringNodes.Source = incrementalNodes;
      // specify the edges to rearrange
      hlData.IncrementalHints.IncrementalSequencingItems.Source = incrementalEdges;

      // append the FixNodeLocationStage to fix the position of the upper right corner
      // of the currently expanded or collapsed group node so that the mouse cursor
      // remains on the expand/collapse button during layout
      var fixNodeLocationStage = new FixNodeLocationStage();
      hl.AppendStage(fixNodeLocationStage);
      
      // run the layout calculation and animate the result
      var executor = new LayoutExecutor(graphControl, hl) {
        AnimateViewport = false,
        EasedAnimation = true,
        RunInThread = true,
        UpdateContentRect = true,
        Duration = TimeSpan.FromMilliseconds(500),
        LayoutData = hlData
      };
      // compose layout data from HierarchicLayoutData and FixNodeLayoutData
      await executor.Start();
    }


    /// <summary>
    /// An <see cref="LayoutStageBase"/> that uses a <see cref="IDataProvider"/>/<see cref="IMapper{K,V}"/>
    /// to determine a node whose location should not be changed during the layout.
    /// </summary>
    internal sealed class FixNodeLocationStage : LayoutStageBase
    {
      public override void ApplyLayout(LayoutGraph graph) {
        // determine the single node to keep at the center.
        var provider = graph.GetDataProvider("NodeLayouts");
        Node centerNode = null;
        if (provider != null) {
          centerNode = graph.Nodes.FirstOrDefault(n => provider.Get(n) != null);
        }
        if (CoreLayout != null) {
          if (centerNode != null) {
            // remember old center
            RectD oldLayout = (RectD) provider.Get(centerNode);
            var fixedLocation = new YPoint(graph.GetX(centerNode) + graph.GetWidth(centerNode), graph.GetY(centerNode));
            //Set to saved size (this is important for collapsed nodes to ensure correct size)
            graph.SetSize(centerNode, oldLayout.Width, oldLayout.Height);
            // run layout
            CoreLayout.ApplyLayout(graph);
            // obtain new center
            var newFixedLocation = new YPoint(graph.GetX(centerNode) + graph.GetWidth(centerNode),
                                              graph.GetY(centerNode));
            // and adjust the layout
            LayoutGraphUtilities.MoveSubgraph(graph, graph.GetNodeCursor(), fixedLocation.X - newFixedLocation.X,
                                    fixedLocation.Y - newFixedLocation.Y);
          } else {
            CoreLayout.ApplyLayout(graph);
          }
        }
      }
    }

    private HierarchicLayout CreateHierarchicLayout() {
      // create and HierarchicLayout
      return new HierarchicLayout
      {
        RecursiveGroupLayering = false,
        LayoutMode = LayoutMode.Incremental,
        EdgeLayoutDescriptor = { RoutingStyle = new RoutingStyle(EdgeRoutingStyle.Orthogonal) }
      };
    }

    /// <summary>
    /// Creates a mode and registers it as the
    /// <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      var inputMode = new GraphViewerInputMode();
      inputMode.NavigationInputMode.AllowCollapseGroup = true;
      inputMode.NavigationInputMode.AllowExpandGroup = true;
      // FitContent interferes with our viewport animation setup
      inputMode.NavigationInputMode.FitContentAfterGroupActions = false;
      inputMode.NavigationInputMode.GroupExpanded += NavigationInputMode_GroupExpanded;
      inputMode.NavigationInputMode.GroupCollapsed += NavigationInputMode_GroupCollapsed;
      graphControl.InputMode = inputMode;
    }

    /// <summary>
    /// Event handler that is triggered when a group is closed interactively.
    /// </summary>
    /// <remarks>This method performs an incremental layout on the newly collapsed group node.</remarks>
    /// <param name="source"></param>
    /// <param name="evt"></param>
    private async void NavigationInputMode_GroupCollapsed(object source, ItemEventArgs<INode> evt) {
      incrementalNodes.Clear();
      incrementalEdges.Clear();
      // we mark the group node and its adjacent edges as incremental
      incrementalNodes.Add(evt.Item);
      incrementalEdges.UnionWith(graphControl.Graph.EdgesAt(evt.Item));
      // rescue the original node bounds which are needed for a correct layout
      INode groupNode = evt.Item;
      fixedGroupNodeLayout.Clear();
      fixedGroupNodeLayout[groupNode] = groupNode.Layout.ToRectD();

      // retrieve the state of the view node *before* the grouping operation
      INode master = foldingView.GetMasterItem(groupNode);
      // and set these bounds so that the animation will move it to the correct location
      graphControl.Graph.SetNodeLayout(groupNode, master.Layout.ToRectD());

      // reset adjacent edge paths to get smoother layout transitions
      foreach (var edge in graphControl.Graph.EdgesAt(groupNode)) {
        graphControl.Graph.ClearBends(edge);
      }

      await ApplyLayout();
    }

    /// <summary>
    /// Event handler that is triggered when a group is expanded interactively.
    /// </summary>
    /// <remarks>This method performs an incremental layout on the newly expanded group node and its descendants.</remarks>
    /// <param name="source"></param>
    /// <param name="evt"></param>
    private async void NavigationInputMode_GroupExpanded(object source, ItemEventArgs<INode> evt) {
      incrementalNodes.Clear();
      incrementalEdges.Clear();
      // we mark the group node and its descendants as incremental
      incrementalNodes.Add(evt.Item);
      var descendants = graphControl.Graph.GetGroupingSupport().GetDescendants(evt.Item);
      incrementalNodes.UnionWith(descendants);
      INode groupNode = evt.Item;
      fixedGroupNodeLayout.Clear();
      // rescue the original node bounds which are needed for a correct layout
      fixedGroupNodeLayout[groupNode] = groupNode.Layout.ToRectD();

      INode master = foldingView.GetMasterItem(groupNode);

      // retrieve the state of the view node *before* the grouping operation
      var state = foldingView.Manager.GetFolderNodeState(master);
      // and set these bounds so that the animation will move it to the correct location
      graphControl.Graph.SetNodeLayout(groupNode, state.Layout.ToRectD());

      // reset the paths and the centers of the child nodes so that morphing looks smoother
      foreach (var childNode in descendants) {
        foreach (var edge in graphControl.Graph.EdgesAt(childNode)) {
          graphControl.Graph.ClearBends(edge);
        }
        graphControl.Graph.SetNodeCenter(childNode, groupNode.Layout.GetCenter());
      }

      // reset adjacent edge paths to get smoother layout transitions
      foreach (var edge in graphControl.Graph.EdgesAt(groupNode)) {
        graphControl.Graph.ClearBends(edge);
      }

      await ApplyLayout();
    }

    /// <summary>
    /// Initializes the graph instance, setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      // create the manager for the folding operations
      FoldingManager foldingManager = new FoldingManager();

      // create a view 
      foldingView = foldingManager.CreateFoldingView();

      // and set it to the GraphControl
      graphControl.Graph = foldingView.Graph;
      
      DemoStyles.InitDemoStyles(graphControl.Graph, foldingEnabled:true);

      // decorate the behavior of nodes
      NodeDecorator nodeDecorator = graphControl.Graph.GetDecorator().NodeDecorator;
      
      // adjust the insets so that labels are considered
      nodeDecorator.InsetsProviderDecorator.SetImplementationWrapper(
        delegate(INode node, INodeInsetsProvider insetsProvider) {
          if (insetsProvider != null) {
            InsetsD insets = insetsProvider.GetInsets(node);
            return new LabelInsetsProvider(insets);
          } else {
            return new LabelInsetsProvider();
          }
        });

      //Constrain group nodes to at least the size of their labels
      nodeDecorator.SizeConstraintProviderDecorator.SetImplementationWrapper(
        (node, oldImpl) => new LabelSizeConstraintProvider(oldImpl));

      fixedGroupNodeLayout = graphControl.Graph.MapperRegistry.CreateMapper<INode, RectD?>("NodeLayouts");
      ReadSampleGraph();
      incrementalNodes.UnionWith(graphControl.Graph.Nodes);
    }


    private void ReadSampleGraph() {
      graphControl.ImportFromGraphML("Resources\\sample.graphml");
    }

    private void OnLoad(object sender, EventArgs e) {
      incrementalNodes = new HashSet<INode>();
      incrementalEdges = new HashSet<IEdge>();

      // initialize the input mode
      InitializeInputModes();

      InitializeGraph();

      graphControl.FitGraphBounds();
      graphControl.Graph.ApplyLayout(CreateHierarchicLayout());
      // top align the graph
      graphControl.ViewPoint = new PointD(graphControl.ViewPoint.X, graphControl.ContentRect.MinY - 50);
    }
  }
}
