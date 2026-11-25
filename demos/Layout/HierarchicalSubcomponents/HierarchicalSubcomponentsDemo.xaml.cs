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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Tree;

namespace yWorks.Demo.HierarchicalSubcomponents
{
  /// <summary>
  ///   Demonstrates how hierarchical layout can arrange selected subcomponents of a graph with different layout styles.
  /// </summary>
  [Demo("hierarchical-subcomponents", "layout", "Hierarchical Subcomponents",
      Summary = "The hierarchical layout can arrange subcomponents with different layout styles.",
      Tags = ["layout", "hierarchical"], Keywords = ["layoutfeatures", "tree", "organic", "orthogonal"])]
  public sealed partial class HierarchicalSubcomponentsDemo
  {
    private readonly HierarchicalSubcomponentsSettings settings = new();

    /// <summary>
    ///   The collection of subcomponents contains all currently assigned subcomponents.
    /// </summary>
    private readonly List<Subcomponent> subcomponents = [];

    /// <summary>
    ///   The collection of node styles that are assigned to nodes that are members of the subcomponents.
    /// </summary>
    private static readonly INodeStyle[] nodeStyles = [
        DemoStyles.CreateDemoNodeStyle(Themes.PaletteBlue),
        DemoStyles.CreateDemoNodeStyle(Themes.PaletteRed),
        DemoStyles.CreateDemoNodeStyle(Themes.PalettePurple),
        DemoStyles.CreateDemoNodeStyle(Themes.PaletteGreen),
        DemoStyles.CreateDemoNodeStyle(Themes.PaletteLightblue)
    ];

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public HierarchicalSubcomponentsDemo() {
      InitializeComponent();
      DataContext = settings;
    }

    protected override async void Initialize() {
      ConfigureUserInteraction(GraphControl);
      InitializeGraph(Graph);
      CreateSampleGraph(Graph);
      InitializeSubcomponents(GraphControl);

      await RunLayout(GraphControl);
      await GraphControl.FitGraphBounds();
      RegisterSelectionListener(GraphControl);
    }

    /// <summary>
    ///   Arranges the graph in the given graph component.
    /// </summary>
    private Task RunLayout(GraphControl graphControl) {
      // initialize a hierarchical layout
      var hierarchicalLayout = new HierarchicalLayout();

      // assign subcomponents with their own layout algorithm and placement policy
      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(graphControl.Graph);
      foreach (var component in subcomponents) {
        // create a subcomponent descriptor that specifies the layout algorithm
        // and placement policy for the subcomponent
        var descriptor = new SubcomponentDescriptor(component.Layout, settings.SelectedPlacementPolicy);
        // specify a subcomponent with the descriptor
        var subcomponent = hierarchicalLayoutData.Subcomponents.Add(descriptor);
        // and assign the nodes to this subcomponent
        subcomponent.Items = component.Nodes;
      }

      return graphControl.ApplyLayoutAnimated(hierarchicalLayout, TimeSpan.FromMilliseconds(700),
          hierarchicalLayoutData);
    }

    /// <summary>
    ///   Creates a new subcomponent that gets a specific layout from the given nodes.
    /// </summary>
    private void CreateSubcomponent(IGraph graph, IEnumerable<INode> nodes, ILayoutAlgorithm layout) {
      var nodeList = nodes.ToList();
      if (nodeList.Count > 0) {
        // find the next free subcomponent index
        Subcomponent newSubcomponent;
        var selectedNodes = new HashSet<INode>(nodeList);
        var newSubcomponentIndex = subcomponents.FindIndex(component =>
            component.Nodes.Count == 0 || component.Nodes.All(selectedNodes.Contains));
        if (newSubcomponentIndex < 0) {
          // add a new subcomponent
          newSubcomponent = new Subcomponent([], layout);
          subcomponents.Add(newSubcomponent);
          newSubcomponentIndex = subcomponents.Count - 1;
        } else {
          // reuse the former subcomponent
          newSubcomponent = subcomponents[newSubcomponentIndex];
          newSubcomponent.Nodes.Clear();
          newSubcomponent.Layout = layout;
        }

        // update the subcomponents from which the nodes are taken as well as the new subcomponent
        foreach (var node in nodeList) {
          var oldSubcomponentIndex = GetSubcomponentIndex(node);
          if (oldSubcomponentIndex is { } oldIndex && oldIndex != newSubcomponentIndex &&
              oldIndex < subcomponents.Count) {
            subcomponents[oldIndex].Nodes.Remove(node);
          }
          newSubcomponent.Nodes.Add(node);
          node.Tag = newSubcomponentIndex;
          graph.SetStyle(node, nodeStyles[newSubcomponentIndex % nodeStyles.Length]);
        }
      }
    }

    /// <summary>
    ///   Removes the given nodes from every subcomponent.
    /// </summary>
    private void RemoveSubcomponent(IGraph graph, IEnumerable<INode> nodes) {
      foreach (var node in nodes) {
        if (GetSubcomponentIndex(node) is { } index && index < subcomponents.Count) {
          var subcomponentNodes = subcomponents[index].Nodes;
          subcomponentNodes.Remove(node);
          node.Tag = null;
          graph.SetStyle(node, graph.NodeDefaults.GetStyleInstance());
        }
      }
    }

    /// <summary>
    ///   Enables interactive editing for the given graph component.
    ///   Restricts marquee selection to nodes.
    /// </summary>
    private static void ConfigureUserInteraction(GraphControl graphControl) {
      graphControl.InputMode = new GraphEditorInputMode { MarqueeSelectableItems = GraphItemTypes.Node };
    }

    /// <summary>
    ///   Sets default styles for nodes and edges.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle();
      graph.NodeDefaults.ShareStyleInstance = false;
      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle();
    }

    /// <summary>
    ///   Creates a sample graph.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      BuildGraphFromJson("sample.json", graph);
    }

    /// <summary>
    ///   Creates initial subcomponents in the demo's sample graph.
    /// </summary>
    private void InitializeSubcomponents(GraphControl graphControl) {
      var graph = graphControl.Graph;

      var hierarchicalLayout = new HierarchicalLayout { LayoutOrientation = LayoutOrientation.LeftToRight };
      CreateSubcomponent(graph, graph.Nodes.Where(node => GetSubcomponentIndex(node) == 0), hierarchicalLayout);
      var treeLayout = CreateTreeLayout();
      CreateSubcomponent(graph, graph.Nodes.Where(node => GetSubcomponentIndex(node) == 1), treeLayout);
      var organicLayout = CreateOrganicLayout();
      CreateSubcomponent(graph, graph.Nodes.Where(node => GetSubcomponentIndex(node) == 2), organicLayout);
      CreateSubcomponent(graph, graph.Nodes.Where(node => GetSubcomponentIndex(node) == 3), hierarchicalLayout);
      var treeLayout2 = CreateTreeLayout();
      treeLayout2.LayoutOrientation = LayoutOrientation.RightToLeft;
      CreateSubcomponent(graph, graph.Nodes.Where(node => GetSubcomponentIndex(node) == 4), treeLayout2);
    }

    /// <summary>
    ///   Returns a new layout algorithm instance for the layout type that is specified in the
    ///   layout combo box.
    /// </summary>
    private ILayoutAlgorithm GetLayoutAlgorithm() {
      return settings.SelectedLayout switch {
          LayoutKind.Tree => CreateTreeLayout(),
          LayoutKind.Organic => CreateOrganicLayout(),
          LayoutKind.Orthogonal => new OrthogonalLayout(),
          LayoutKind.Hierarchical => new HierarchicalLayout(),
          _ => CreateTreeLayout()
      };
    }

    /// <summary>
    ///   Returns a new tree layout algorithm instance.
    /// </summary>
    private static TreeLayout CreateTreeLayout() {
      var tree = new TreeLayout();
      ((SingleLayerSubtreePlacer) tree.DefaultSubtreePlacer).EdgeRoutingStyle =
          SingleLayerSubtreePlacerRoutingStyle.Polyline;
      return tree;
    }

    /// <summary>
    ///   Returns a new organic layout algorithm instance.
    /// </summary>
    private static OrganicLayout CreateOrganicLayout() =>
        new() {
            Deterministic = true,
            DefaultPreferredEdgeLength = 70
        };

    private static void SetLayoutOrientation(ILayoutAlgorithm layout, LayoutOrientation orientation) {
      switch (layout) {
        case TreeLayout treeLayout:
          treeLayout.LayoutOrientation = orientation;
          break;
        case OrganicLayout organicLayout:
          organicLayout.LayoutOrientation = orientation;
          break;
        case OrthogonalLayout orthogonalLayout:
          orthogonalLayout.LayoutOrientation = orientation;
          break;
        case HierarchicalLayout hierarchicalLayout:
          hierarchicalLayout.LayoutOrientation = orientation;
          break;
      }
    }

    /// <summary>
    ///   Enables/disables some UI elements depending on the current selection.
    /// </summary>
    private void RegisterSelectionListener(GraphControl graphControl) {
      UpdateSelectionSettings();
      graphControl.Selection.ItemAdded += (_, _) => UpdateSelectionSettings();
      graphControl.Selection.ItemRemoved += (_, _) => UpdateSelectionSettings();
    }

    private void UpdateSelectionSettings() {
      settings.HasSelectedNodes = GraphControl.Selection.Nodes.Count > 0;
    }

    private async void CreateSubcomponent_Click(object sender, RoutedEventArgs e) {
      var selectedNodes = GraphControl.Selection.Nodes.ToList();
      if (selectedNodes.Count == 0) {
        return;
      }

      // configure the layout algorithm that is assigned to the new subcomponent
      var layout = GetLayoutAlgorithm();
      SetLayoutOrientation(layout, settings.SelectedOrientation);

      // create the subcomponent from all selected nodes with the chosen layout algorithm.
      CreateSubcomponent(Graph, selectedNodes, layout);
      await ApplyLayoutWithBusyState();
    }

    private async void RemoveSubcomponent_Click(object sender, RoutedEventArgs e) {
      var selectedNodes = GraphControl.Selection.Nodes.ToList();
      if (selectedNodes.Count == 0) {
        return;
      }
      RemoveSubcomponent(Graph, selectedNodes);
      await ApplyLayoutWithBusyState();
    }

    private async void RunLayout_Click(object sender, RoutedEventArgs e) {
      await ApplyLayoutWithBusyState();
    }

    private async Task ApplyLayoutWithBusyState() {
      settings.IsBusy = true;
      try {
        await RunLayout(GraphControl);
      } finally {
        settings.IsBusy = false;
      }
    }

    private static int? GetSubcomponentIndex(INode node) {
      return node.Tag switch {
          int index => index,
          double index => (int) index,
          _ => null
      };
    }

    private sealed class Subcomponent(List<INode> nodes, ILayoutAlgorithm layout)
    {
      public List<INode> Nodes { get; } = nodes;

      public ILayoutAlgorithm Layout { get; set; } = layout;
    }

    [STAThread]
    public static void Main() {
      Run<HierarchicalSubcomponentsDemo>();
    }
  }
}
