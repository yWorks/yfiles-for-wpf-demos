/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Algorithms;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Graph.Collapse
{
  /// <summary>
  /// A form that demonstrates the wrapping and decorating of <see cref="IGraph"/> instances.
  /// </summary>
  /// <remarks>
  /// This demo shows a collapsible tree structure. Subtrees can be collapsed or expanded by clicking on 
  /// their root nodes.
  /// </remarks>
  /// <seealso cref="FilteredGraphWrapper"/>
  public partial class GraphCollapseWindow
  {
    // list that stores collapsed nodes
    private readonly ICollection<INode> collapsedNodes = new List<INode>();
    // graph that contains visible nodes
    private FilteredGraphWrapper filteredGraph;
    // graph containing all nodes
    private IGraph fullGraph;
    private readonly INodeStyle leafNodeStyle = new NodeControlNodeStyle("LeafNodeStyleTemplate");
    // currently selected layout algorithm
    private ILayoutAlgorithm currentLayout;
    // list of all layout algorithms
    private readonly List<ILayoutAlgorithm> layouts = new List<ILayoutAlgorithm>();
    // mapper for mapping layout algorithms to their string representation in the combobox
    private IMapper<string, ILayoutAlgorithm> layoutMapper = new DictionaryMapper<string, ILayoutAlgorithm>();
    // the node that has just been toggled and should stay fixed.
    private INode toggledNode;

    /// <summary>
    /// Returns all available layout algorithms.
    /// </summary>
    public List<ILayoutAlgorithm> Layouts {
      get { return layouts; }
    }

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public GraphCollapseWindow() {
      InitializeComponent();
    }

    #region Handling Expand/Collapse Clicks

    /// <summary>
    /// The command that can be used by the node buttons to toggle the visibilit of the child nodes.
    /// </summary>
    /// <remarks>
    /// This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedRoutedEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly ICommand ToggleChildrenCommand = new RoutedUICommand("Toggle Children", "ToggleChildren",
                                                                                  typeof(GraphCollapseWindow));

    
    /// <summary>
    /// Called when the ToggleChildren command has been executed. 
    /// </summary>
    /// <remarks>
    /// Toggles the visiblity of the node's children.
    /// </remarks>
    public async void ToggleChildrenExecuted(object sender, ExecutedRoutedEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null) {
        bool canExpand = filteredGraph.OutDegree(node) != filteredGraph.WrappedGraph.OutDegree(node);
        if (canExpand) {
          await Expand(node);
        } else {
          await Collapse(node);
        }
      }
    }

    /// <summary>
    /// Show the children of a collapsed node.
    /// </summary>
    /// <param name="node">The node that should be expanded</param>
    private async Task Expand(INode node) {
      if (collapsedNodes.Contains(node)) {
        toggledNode = node;
        SetCollapsedTag(node, false);
        AlignChildren(node);
        collapsedNodes.Remove(node);
        filteredGraph.NodePredicateChanged();
        await RunLayout(false);
      }
    }

    /// <summary>
    /// Hide the children of a expanded node.
    /// </summary>
    /// <param name="node">The node that should be collapsed</param>
    private async Task Collapse(INode node) {
      if (!collapsedNodes.Contains(node)) {
        toggledNode = node;
        SetCollapsedTag(node, true);
        collapsedNodes.Add(node);
        filteredGraph.NodePredicateChanged();
        await RunLayout(false);
      }
    }

    private void AlignChildren(INode node) {
      // This method is used to set the initial positions of the children
      // of a node which gets expanded to the position of the expanded node.
      // This looks nicer in the following animated layout. Try commenting
      // out the method body to see the difference.
      PointD center = node.Layout.GetCenter();
      foreach (IEdge edge in fullGraph.EdgesAt(node)) {
        if (edge.SourcePort.Owner == node) {
          fullGraph.ClearBends(edge);
          INode child = (INode)edge.TargetPort.Owner;
          fullGraph.SetNodeCenter(child, center);
          AlignChildren(child);
        }
      }
    }

    private void SetCollapsedTag(INode node, bool collapsed) {
      var style = node.Style as NodeControlNodeStyle;
      if (style != null) {
        style.StyleTag = collapsed;
      }
    }
    
    #endregion

    /// <summary>
    /// Builds a sample graph
    /// </summary>
    private void BuildTree(IGraph graph, int children, int levels, int collapseLevel) {
      INode root = graph.CreateNode(new PointD(20, 20));
      SetCollapsedTag(root, false);
      AddChildren(levels, graph, root, children, collapseLevel);
    }

    private readonly Random random = new Random(666);

    /// <summary>
    /// Recusively add children to the tree
    /// </summary>
    private void AddChildren(int level, IGraph graph, INode root, int childCount, int collapseLevel) {
      int actualChildCount = random.Next(1, childCount + 1);
      for (int i = 0; i < actualChildCount; i++) {
        INode child = graph.CreateNode(new PointD(20, 20));
        graph.CreateEdge(root, child);
        if (level < collapseLevel) {
          collapsedNodes.Add(child);
          SetCollapsedTag(child, true);
        }
        else {
          SetCollapsedTag(child, false);
        }
        if (level > 0) {
          AddChildren(level - 1, graph, child, 4, 2);
        } else {
          graph.SetStyle(child, leafNodeStyle);
        }
      }
    }

    /// <summary>
    /// Predicate for the filtered graph wrapper that 
    /// indicates whether a node should be visible
    /// </summary>
    /// <returns> true if the node should be visible</returns>
    private bool NodePredicate(INode node) {
      // return true if none of the parent nodes is collapsed
      foreach (IEdge edge in fullGraph.InEdgesAt(node)) {
        INode parent = (INode)edge.SourcePort.Owner;
        return !collapsedNodes.Contains(parent) && NodePredicate(parent);
      }
      return true;
    }

    #region Initialization

    /// <summary>
    /// Called upon the loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <param name="e"></param>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoaded(object source, EventArgs e) {
      // initialize the input mode
      InitializeInputModes();

      // initialize the graph
      InitializeGraph();
    }

    /// <summary>
    /// Initializes the graph instance, setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      // Create the graph instance that will hold the complete graph.
      fullGraph = new DefaultGraph();

      // Create a nice default style for the nodes
      fullGraph.NodeDefaults.Style = new NodeControlNodeStyle("InnerNodeStyleTemplate");
      fullGraph.NodeDefaults.Size = new SizeD(60, 30);
      fullGraph.NodeDefaults.ShareStyleInstance = false;


      // and a style for the labels
      DefaultLabelStyle labelStyle = new DefaultLabelStyle();
      fullGraph.NodeDefaults.Labels.Style = labelStyle;


      // now build a simple sample tree
      BuildTree(fullGraph, 3, 3, 3);

      // create a view of the graph that contains only non-collapsed subtrees.
      // use a predicate method to decide what nodes should be part of the graph.
      filteredGraph = new FilteredGraphWrapper(fullGraph, NodePredicate);

      // display the filtered graph in our control.
      graphControl.Graph = filteredGraph;
      // center the graph to prevent the initial layout fading in from the top left corner
      graphControl.FitGraphBounds();

      // create layout algorithms
      SetupLayouts();
    }

    /// <summary>
    /// Creates a mode and registers it as the
    /// <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      // Create a multiplexing input mode and that uses a MoveViewportInputMode and add a 
      // wait input mode that will be used by the animator to block user input during 
      // animations automatically.
      MultiplexingInputMode multiplexingInputMode = new MultiplexingInputMode();
      multiplexingInputMode.Add(new WaitInputMode{Priority = 0});
      multiplexingInputMode.Add(new MoveViewportInputMode{Priority = 5});
      graphControl.InputMode = multiplexingInputMode;
    }

    private void SetupLayouts() {
      // create TreeLayout
      var treeLayout = new TreeLayout {LayoutOrientation = LayoutOrientation.LeftToRight};
      treeLayout.PrependStage(new FixNodeLayoutStage());
      layouts.Add(treeLayout);
      layoutMapper["Tree"] = treeLayout;
      layoutComboBox.Items.Add("Tree");

      // create BalloonLayout
      var balloonLayout = new BalloonLayout
      {
        FromSketchMode = true,
        CompactnessFactor = 1.0,
        AllowOverlaps = true
      };
      balloonLayout.PrependStage(new FixNodeLayoutStage());
      layouts.Add(balloonLayout);
      layoutMapper["Balloon"] = balloonLayout;
      layoutComboBox.Items.Add("Balloon");

      // create OrganicLayout
      var organicLayout = new OrganicLayout {
        MinimumNodeDistance = 40,
        Deterministic = true
      };
      organicLayout.PrependStage(new FixNodeLayoutStage());
      layouts.Add(organicLayout);
      layoutMapper["Organic"] = organicLayout;
      layoutComboBox.Items.Add("Organic");

      // create OrthogonalLayout
      var orthogonalLayout = new OrthogonalLayout();
      orthogonalLayout.PrependStage(new FixNodeLayoutStage());
      layouts.Add(orthogonalLayout);
      layoutMapper["Orthogonal"] = orthogonalLayout;
      layoutComboBox.Items.Add("Orthogonal");

      // set it as initial value
      currentLayout = treeLayout;
      layoutComboBox.SelectedIndex = 0;
    }

    #endregion

    #region Actions

    /// <summary>
    /// Exit the demo
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>            
    private void ExitMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion
    
    private async Task RunLayout(bool animateViewport) {
      if (currentLayout != null) {
        // provide additional data to configure the FixNodeLayoutStage
        FixNodeLayoutData fixNodeLayoutData = new FixNodeLayoutData();
        // specify the node whose position is to be fixed during layout
        fixNodeLayoutData.FixedNodes.Item = toggledNode;

        // run the layout and animate the result
        var layoutExecutor = new LayoutExecutor(graphControl, currentLayout)
        {
          UpdateContentRect = true,
          AnimateViewport = animateViewport,
          Duration = TimeSpan.FromSeconds(0.3d),
          LayoutData = fixNodeLayoutData
        };
        await layoutExecutor.Start();
        toggledNode = null;
      }
    }

    private async void layoutComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      currentLayout = layoutMapper[layoutComboBox.SelectedItem as string];
      await RunLayout(true);
    }

  }

  internal class XCoordComparer : IComparer<object>
  {
    /// <summary>Compares two edges by the x-coordinates of the centers of their target nodes.</summary>
    /// <param name="edge1">the first edge</param>
    /// <param name="edge2">the second edge</param>
    /// <returns>a negative value if the first target node is left of the second target node, a positive value if it's the
    /// other way round and <c>0</c> if both target nodes are at the same x-coordinate</returns>
    public virtual int Compare(object edge1, object edge2) {
      Node va = ((Edge) edge1).Target;
      Node vb = ((Edge) edge2).Target;
      LayoutGraph graph = (LayoutGraph) va.Graph;
      return (int) ((100.0 * (graph.GetCenterX(va) - graph.GetCenterX(vb))));
    }
  }

}
