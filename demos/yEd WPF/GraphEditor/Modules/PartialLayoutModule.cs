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

using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Organic;
using yWorks.Layout.Orthogonal;
using yWorks.Layout.Partial;
using yWorks.Layout.Router;
using LayoutOrientation = yWorks.Layout.Partial.LayoutOrientation;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="BusRouter"/>.
  /// </summary>
  public class PartialLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string NAME = "PARTIAL";
    private const string GENERAL = "GENERAL";
    private const string SUBGRAPH_LAYOUT = "SUBGRAPH_LAYOUT";
    private const string SUBGRAPH_LAYOUT_IHL = "SUBGRAPH_LAYOUT_IHL";
    private const string SUBGRAPH_LAYOUT_ORGANIC = "SUBGRAPH_LAYOUT_ORGANIC";
    private const string SUBGRAPH_LAYOUT_CIRCULAR = "SUBGRAPH_LAYOUT_CIRCULAR";
    private const string SUBGRAPH_LAYOUT_ORTHOGONAL = "SUBGRAPH_LAYOUT_ORTHOGONAL";
    private const string ROUTING_TO_SUBGRAPH_OCTILINEAR = "ROUTING_TO_SUBGRAPH_OCTILINEAR";
    private const string SUBGRAPH_LAYOUT_NO_LAYOUT = "SUBGRAPH_LAYOUT_NO_LAYOUT";
    private const string MIN_NODE_DIST = "MIN_NODE_DIST";

    private const string SUBGRAPH_POSITION_STRATEGY = "SUBGRAPH_POSITION_STRATEGY";
    private const string SUBGRAPH_POSITIONING_STRATEGY_BARYCENTER = "SUBGRAPH_POSITION_STRATEGY_BARYCENTER";
    private const string SUBGRAPH_POSITIONING_STRATEGY_FROM_SKETCH = "SUBGRAPH_POSITION_STRATEGY_FROM_SKETCH";

    private const string ROUTING_TO_SUBGRAPH = "ROUTING_TO_SUBGRAPH";
    private const string ROUTING_TO_SUBGRAPH_STRAIGHT_LINE = "ROUTING_TO_SUBGRAPH_STRAIGHT_LINE";
    private const string ROUTING_TO_SUBGRAPH_ORTHOGONALLY = "ROUTING_TO_SUBGRAPH_ORTHOGONALLY";
    private const string ROUTING_TO_SUBGRAPH_ORGANIC = "ROUTING_TO_SUBGRAPH_ORGANIC";
    private const string ROUTING_TO_SUBGRAPH_AUTO = "ROUTING_TO_SUBGRAPH_AUTO";

    private const string MODE_COMPONENT_ASSIGNMENT = "MODE_COMPONENT_ASSIGNMENT";
    private const string MODE_COMPONENT_CLUSTERING = "MODE_COMPONENT_CLUSTERING";
    private const string MODE_COMPONENT_CONNECTED = "MODE_COMPONENT_CONNECTED";
    private const string MODE_COMPONENT_CUSTOMIZED = "MODE_COMPONENT_CUSTOMIZED";
    private const string MODE_COMPONENT_SINGLE = "MODE_COMPONENT_SINGLE";

    private const string ORIENTATION_MAIN_GRAPH = "ORIENTATION_MAIN_GRAPH";
    private const string ORIENTATION_MAIN_GRAPH_NONE = "ORIENTATION_MAIN_GRAPH_NONE";
    private const string ORIENTATION_MAIN_GRAPH_AUTO_DETECT = "ORIENTATION_MAIN_GRAPH_AUTO_DETECT";
    private const string ORIENTATION_MAIN_GRAPH_TOP_TO_BOTTOM = "ORIENTATION_MAIN_GRAPH_TOP_TO_BOTTOM";
    private const string ORIENTATION_MAIN_GRAPH_BOTTOM_TO_TOP = "ORIENTATION_MAIN_GRAPH_BOTTOM_TO_TOP";
    private const string ORIENTATION_MAIN_GRAPH_LEFT_TO_RIGHT = "ORIENTATION_MAIN_GRAPH_LEFT_TO_RIGHT";
    private const string ORIENTATION_MAIN_GRAPH_RIGHT_TO_LEFT = "ORIENTATION_MAIN_GRAPH_RIGHT_TO_LEFT";

    private const string CONSIDER_SNAPLINES = "CONSIDER_SNAPLINES";
    private const string CONSIDER_EDGE_DIRECTION = "CONSIDER_EDGE_DIRECTION";


    private static readonly IDictionary<string, EdgeRoutingStrategy> routingStrategies =
      new Dictionary<string, EdgeRoutingStrategy>();

    private static readonly IDictionary<string, SubgraphPlacement> subgraphPlacementStrategies =
      new Dictionary<string, SubgraphPlacement>();

    private static readonly IDictionary<string, ComponentAssignmentStrategy> componentAssignment =
      new Dictionary<string, ComponentAssignmentStrategy>();

    private static readonly IDictionary<string, LayoutOrientation> layoutOrientation =
      new Dictionary<string, LayoutOrientation>();

    private static readonly string[] subgraphLayouts = {
                                                           SUBGRAPH_LAYOUT_IHL, SUBGRAPH_LAYOUT_ORGANIC,
                                                           SUBGRAPH_LAYOUT_CIRCULAR, SUBGRAPH_LAYOUT_ORTHOGONAL,
                                                           SUBGRAPH_LAYOUT_NO_LAYOUT
                                                         };

    static PartialLayoutModule() {
      routingStrategies[ROUTING_TO_SUBGRAPH_AUTO] = EdgeRoutingStrategy.Automatic;
      routingStrategies[ROUTING_TO_SUBGRAPH_STRAIGHT_LINE] = EdgeRoutingStrategy.Straightline;
      routingStrategies[ROUTING_TO_SUBGRAPH_ORTHOGONALLY] = EdgeRoutingStrategy.Orthogonal;
      routingStrategies[ROUTING_TO_SUBGRAPH_ORGANIC] = EdgeRoutingStrategy.Organic;
      routingStrategies[ROUTING_TO_SUBGRAPH_OCTILINEAR] = EdgeRoutingStrategy.Octilinear;

      subgraphPlacementStrategies[SUBGRAPH_POSITIONING_STRATEGY_FROM_SKETCH] = SubgraphPlacement.FromSketch;
      subgraphPlacementStrategies[SUBGRAPH_POSITIONING_STRATEGY_BARYCENTER] = SubgraphPlacement.Barycenter;

      componentAssignment[MODE_COMPONENT_CONNECTED] = ComponentAssignmentStrategy.Customized;
      componentAssignment[MODE_COMPONENT_SINGLE] = ComponentAssignmentStrategy.Single;
      componentAssignment[MODE_COMPONENT_CUSTOMIZED] = ComponentAssignmentStrategy.Customized;
      componentAssignment[MODE_COMPONENT_CLUSTERING] = ComponentAssignmentStrategy.Clustering;

      layoutOrientation[ORIENTATION_MAIN_GRAPH_AUTO_DETECT] = LayoutOrientation.AutoDetect;
      layoutOrientation[ORIENTATION_MAIN_GRAPH_TOP_TO_BOTTOM] = LayoutOrientation.TopToBottom;
      layoutOrientation[ORIENTATION_MAIN_GRAPH_BOTTOM_TO_TOP] = LayoutOrientation.BottomToTop;
      layoutOrientation[ORIENTATION_MAIN_GRAPH_LEFT_TO_RIGHT] = LayoutOrientation.LeftToRight;
      layoutOrientation[ORIENTATION_MAIN_GRAPH_RIGHT_TO_LEFT] = LayoutOrientation.RightToLeft;
      layoutOrientation[ORIENTATION_MAIN_GRAPH_NONE] = LayoutOrientation.None;
    }

    #endregion

    private SelectedNodesDP selectedNodesDp;
    private IDataProvider selectedEdgesDp;
    private IDataProvider pnDp;
    private IDataProvider peDp;

    /// <summary>
    /// Create new instance
    /// </summary>
    public PartialLayoutModule() : base(NAME) {}


    ///<inheritdoc/>
    protected override void SetupHandler() {
      OptionGroup layoutGroup = Handler.AddGroup(GENERAL);
      layoutGroup.Attributes[TableEditorFactory.RenderingHintsAttribute] = TableEditorFactory.RenderingHints.Invisible;
      layoutGroup.Attributes[DefaultEditorFactory.RenderingHintsAttribute] =
        DefaultEditorFactory.RenderingHints.Invisible;

      layoutGroup.AddList(ROUTING_TO_SUBGRAPH, routingStrategies.Keys, ROUTING_TO_SUBGRAPH_AUTO);
      layoutGroup.AddList(MODE_COMPONENT_ASSIGNMENT, componentAssignment.Keys, MODE_COMPONENT_CONNECTED);
      layoutGroup.AddList(SUBGRAPH_LAYOUT, subgraphLayouts, subgraphLayouts[0]);
      layoutGroup.AddList(SUBGRAPH_POSITION_STRATEGY, subgraphPlacementStrategies.Keys,
                          SUBGRAPH_POSITIONING_STRATEGY_FROM_SKETCH);
      layoutGroup.AddInt(MIN_NODE_DIST, 30, 1, 100);
      layoutGroup.AddList(ORIENTATION_MAIN_GRAPH, layoutOrientation.Keys, ORIENTATION_MAIN_GRAPH_AUTO_DETECT);
      layoutGroup.AddBool(CONSIDER_SNAPLINES, true);
      layoutGroup.AddBool(CONSIDER_EDGE_DIRECTION, false);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      OptionGroup layoutGroup = Handler.GetGroupByName(GENERAL);
      var partialLayout = new PartialLayout
                              {
                                MinimumNodeDistance = (int) layoutGroup[MIN_NODE_DIST].Value,
                                ConsiderNodeAlignment = (bool) layoutGroup[CONSIDER_SNAPLINES].Value,
                                SubgraphPlacement =
                                  subgraphPlacementStrategies[
                                    (string) layoutGroup[SUBGRAPH_POSITION_STRATEGY].Value]
                              };

      string componentAssignmentStr = (string) layoutGroup[MODE_COMPONENT_ASSIGNMENT].Value;
      partialLayout.ComponentAssignmentStrategy =
        componentAssignment[componentAssignmentStr];

      partialLayout.LayoutOrientation =
        layoutOrientation[(string) layoutGroup[ORIENTATION_MAIN_GRAPH].Value];      
      
      partialLayout.EdgeRoutingStrategy =
        routingStrategies[(string) layoutGroup[ROUTING_TO_SUBGRAPH].Value];

      ILayoutAlgorithm subgraphLayout = null;
      if (componentAssignmentStr != MODE_COMPONENT_SINGLE) {
        var subGraphLayoutStr = (string) layoutGroup[SUBGRAPH_LAYOUT].Value;
        switch (subGraphLayoutStr) {
          case SUBGRAPH_LAYOUT_IHL:
            subgraphLayout = new HierarchicLayout();
            break;
          case SUBGRAPH_LAYOUT_ORGANIC:
            subgraphLayout = new OrganicLayout();
            break;
          case SUBGRAPH_LAYOUT_CIRCULAR:
            subgraphLayout = new CircularLayout();
            break;
          case SUBGRAPH_LAYOUT_ORTHOGONAL:
            subgraphLayout = new OrthogonalLayout();
            break;
          default:
            break;
        }
      }
      partialLayout.CoreLayout = subgraphLayout;

      LayoutAlgorithm = partialLayout;
    }

    ///<inheritdoc/>
    protected override void PerformPreLayout() {
      base.PerformPreLayout();
      pnDp = CurrentLayoutGraph.GetDataProvider(PartialLayout.AffectedNodesDpKey);
      if (pnDp == null) {
        selectedNodesDp = new SelectedNodesDP(this);
        CurrentLayoutGraph.AddDataProvider(PartialLayout.AffectedNodesDpKey, selectedNodesDp);
      }
      peDp = CurrentLayoutGraph.GetDataProvider(PartialLayout.AffectedEdgesDpKey);
      if (peDp == null) {
        selectedEdgesDp = new SelectedEdgesDP(this);
        CurrentLayoutGraph.AddDataProvider(PartialLayout.AffectedEdgesDpKey, selectedEdgesDp);
      }
    }

    ///<inheritdoc/>
    protected override void PerformPostLayout() {
      if (selectedNodesDp != null) {
        CurrentLayoutGraph.RemoveDataProvider(PartialLayout.AffectedNodesDpKey);
      }
      if (pnDp != null) {
        CurrentLayoutGraph.AddDataProvider(PartialLayout.AffectedNodesDpKey, pnDp);
      }
      if (selectedEdgesDp != null) {
        CurrentLayoutGraph.RemoveDataProvider(PartialLayout.AffectedEdgesDpKey);
      }
      if (peDp != null) {
        CurrentLayoutGraph.AddDataProvider(PartialLayout.AffectedEdgesDpKey, peDp);
      }
      base.PerformPostLayout();
    }

    internal class SelectedEdgesDP : DataProviderAdapter
    {
      private readonly LayoutModule module;

      public SelectedEdgesDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected((Edge) o);
      }
    }

    internal class SelectedNodesDP : DataProviderAdapter
    {
      private readonly LayoutModule module;

      public SelectedNodesDP(LayoutModule module) {
        this.module = module;
      }

      public override bool GetBool(object o) {
        return module.IsSelected((Node) o);
      }
    }

    protected override async Task StartWithIGraph(IGraph graph, ILookup newContext) {
      bool considerDirection = (bool) Handler.GetValue(GENERAL, CONSIDER_EDGE_DIRECTION);
      if (considerDirection) {
        graph.MapperRegistry.CreateDelegateMapper<IEdge,bool>(PartialLayout.DirectedEdgesDpKey, edge => {
          var style = edge.Style as IArrowOwner;
          if (style != null) {
            // directed => exactly one endpoint has an arrow
            return (style.SourceArrow == Arrows.None) == (style.TargetArrow != Arrows.None);
          }
          return false;
        });
      }
      try {
        await base.StartWithIGraph(graph, newContext);
      } finally {
        if (considerDirection) graph.MapperRegistry.RemoveMapper(PartialLayout.DirectedEdgesDpKey);
      }
    }
  }
}
