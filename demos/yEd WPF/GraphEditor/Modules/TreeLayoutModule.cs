/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.Tree;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="ClassicTreeLayout"/>, <see cref="BalloonLayout"/>, and <see cref="AspectRatioTreeLayout"/>.
  /// </summary>
  public class TreeLayoutModule : LayoutModule
  {
    #region configuration constants
    private const string LAYOUT_STYLE = "LAYOUT_STYLE";
    private const string LEFT_TO_RIGHT = "LEFT_TO_RIGHT";
    private const string GENERAL = "GENERAL";

    private const string ALLOW_NON_TREE_EDGES = "ALLOW_NON_TREES";
    private const string ROUTING_STYLE_FOR_NON_TREE_EDGES = "ROUTING_STYLE_FOR_NON_TREE_EDGES";
    private const string ROUTE_ORGANIC = "ROUTE_ORGANIC";
    private const string ROUTE_ORTHOGONAL = "ROUTE_ORTHOGONAL";
    private const string ROUTE_STRAIGHTLINE = "ROUTE_STRAIGHTLINE";
    private const string ROUTE_BUNDLED = "ROUTE_BUNDLED";
    private const string EDGE_BUNDLING_STRENGTH = "EDGE_BUNDLING_STRENGTH";

    private const string MINIMAL_NODE_DISTANCE = "MINIMAL_NODE_DISTANCE";
    private const string ACT_ON_SELECTION_ONLY = "ACT_ON_SELECTION_ONLY";
    private const string BOTTOM_TO_TOP = "BOTTOM_TO_TOP";
    private const string MINIMAL_LAYER_DISTANCE = "MINIMAL_LAYER_DISTANCE";
    private const string ORIENTATION = "ORIENTATION";
    private const string RIGHT_TO_LEFT = "RIGHT_TO_LEFT";
    private const string HV = "HV";
    private const string VERTICAL_SPACE = "VERTICAL_SPACE";
    private const string AR = "AR";
    private const string HORIZONTAL_SPACE = "HORIZONTAL_SPACE";
    private const string TREE = "TREE";
    private const string TOP_TO_BOTTOM = "TOP_TO_BOTTOM";

    private const string BEND_DISTANCE = "BEND_DISTANCE";
    private const string ASPECT_RATIO = "ASPECT_RATIO";
    private const string USE_VIEW_ASPECT_RATIO = "USE_VIEW_ASPECT_RATIO";

    private const string DIRECTED = "DIRECTED";
    private const string ORTHOGONAL_EDGE_ROUTING = "ORTHOGONAL_EDGE_ROUTING";

    private const string CHILD_PLACEMENT_POLICY = "CHILD_PLACEMENT_POLICY";
    private const string SIBLINGS_ON_SAME_LAYER = "SIBLINGS_ON_SAME_LAYER";
    private const string ALL_LEAVES_ON_SAME_LAYER = "ALL_LEAVES_ON_SAME_LAYER";
    private const string LEAVES_STACKED = "LEAVES_STACKED";
    private const string LEAVES_STACKED_LEFT = "LEAVES_STACKED_LEFT";
    private const string LEAVES_STACKED_RIGHT = "LEAVES_STACKED_RIGHT";
    private const string LEAVES_STACKED_LEFT_AND_RIGHT = "LEAVES_STACKED_LEFT_AND_RIGHT";

    private const string ENFORCE_GLOBAL_LAYERING = "ENFORCE_GLOBAL_LAYERING";


    private const string INTEGRATED_EDGE_LABELING = "INTEGRATED_EDGE_LABELING";
    private const string INTEGRATED_NODE_LABELING = "INTEGRATED_NODE_LABELING";

    private const string VERTICAL_ALIGNMENT = "VERTICAL_ALIGNMENT";
    private const string BUS_ALIGNMENT = "BUS_ALIGNMENT";

    private const string PORT_STYLE = "PORT_STYLE";
    private const string NODE_CENTER_PORTS = "NODE_CENTER";
    private const string BORDER_CENTER_PORTS = "BORDER_CENTER";
    private const string BORDER_DISTRIBUTED_PORTS = "BORDER_DISTRIBUTED";

    private static readonly List<string> enumRoute = new List<string>();
    private static readonly List<string> enumStyle = new List<string>();
    private static readonly Dictionary<string, LayoutOrientation> enumOrient = new Dictionary<string, LayoutOrientation>();
    private static readonly Dictionary<string, PortStyle> enumPortStyle = new Dictionary<string, PortStyle>();
    private static readonly Dictionary<string, LeafPlacement> childPlacementPolicies = new Dictionary<string, LeafPlacement>();

    static TreeLayoutModule() {
      enumRoute.Add(ROUTE_ORTHOGONAL);
      enumRoute.Add(ROUTE_ORGANIC);
      enumRoute.Add(ROUTE_STRAIGHTLINE);
      enumRoute.Add(ROUTE_BUNDLED);

      enumStyle.Add(DIRECTED);
      enumStyle.Add(AR);

      enumOrient.Add(TOP_TO_BOTTOM, LayoutOrientation.TopToBottom);
      enumOrient.Add(LEFT_TO_RIGHT, LayoutOrientation.LeftToRight);
      enumOrient.Add(BOTTOM_TO_TOP, LayoutOrientation.BottomToTop);
      enumOrient.Add(RIGHT_TO_LEFT, LayoutOrientation.RightToLeft);

      enumPortStyle.Add(NODE_CENTER_PORTS, PortStyle.NodeCenter);
      enumPortStyle.Add(BORDER_CENTER_PORTS, PortStyle.BorderCenter);
      enumPortStyle.Add(BORDER_DISTRIBUTED_PORTS, PortStyle.BorderDistributed);

      childPlacementPolicies[SIBLINGS_ON_SAME_LAYER] = LeafPlacement.SiblingsOnSameLayer;
      childPlacementPolicies[ALL_LEAVES_ON_SAME_LAYER] = LeafPlacement.AllLeavesOnSameLayer;
      childPlacementPolicies[LEAVES_STACKED] = LeafPlacement.LeavesStacked;
      childPlacementPolicies[LEAVES_STACKED_LEFT] = LeafPlacement.LeavesStackedLeft;
      childPlacementPolicies[LEAVES_STACKED_RIGHT] = LeafPlacement.LeavesStackedRight;
      childPlacementPolicies[LEAVES_STACKED_LEFT_AND_RIGHT] = LeafPlacement.LeavesStackedLeftAndRight;
    }
    #endregion

    #region private members
    private MultiStageLayout multiStageLayout;
    private TreeReductionStage additionalStage;
    #endregion

    /// <summary>
    /// Create new instance
    /// </summary>
    public TreeLayoutModule() : base(TREE) {
    }

    #region LayoutModule interface
    ///<inheritdoc/>
    protected override void SetupHandler() {
      OptionGroup generalGroup = Handler.AddGroup(GENERAL);
      generalGroup.AddList(LAYOUT_STYLE, enumStyle, DIRECTED);
      IOptionItem allowNonTreeItem = generalGroup.AddBool(ALLOW_NON_TREE_EDGES, true);
      IOptionItem nonTreeStyleItem = generalGroup.AddList(ROUTING_STYLE_FOR_NON_TREE_EDGES, enumRoute, ROUTE_ORTHOGONAL);
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(allowNonTreeItem, true, nonTreeStyleItem);

      generalGroup.AddBool(ACT_ON_SELECTION_ONLY, false);
      var bundlingStrength = generalGroup.AddDouble(EDGE_BUNDLING_STRENGTH, 0.99, 0, 1);
      cm.SetEnabledOnValueEquals(nonTreeStyleItem, ROUTE_BUNDLED, bundlingStrength);

      ClassicTreeLayout treeLayout = new ClassicTreeLayout();
      OptionGroup directedGroup = Handler.AddGroup(DIRECTED);
      directedGroup.AddInt( MINIMAL_NODE_DISTANCE, (int)treeLayout.MinimumNodeDistance, 1, int.MaxValue);
      directedGroup.AddInt( MINIMAL_LAYER_DISTANCE, (int)treeLayout.MinimumLayerDistance, 10, int.MaxValue);
      directedGroup.AddList( ORIENTATION, enumOrient.Keys, TOP_TO_BOTTOM);
      directedGroup.AddList( PORT_STYLE, enumPortStyle.Keys, NODE_CENTER_PORTS);

      directedGroup.AddBool( INTEGRATED_NODE_LABELING, false);
      directedGroup.AddBool( INTEGRATED_EDGE_LABELING, false);

      IOptionItem edgeRoutingOption = directedGroup.AddBool( ORTHOGONAL_EDGE_ROUTING, false);
      IOptionItem busAlignmentOption = directedGroup.AddDouble(BUS_ALIGNMENT, 0.5, 0, 1);
      
      directedGroup.AddDouble(VERTICAL_ALIGNMENT, 0.5, 0, 1);
      var childPolicyItem = directedGroup.AddList(CHILD_PLACEMENT_POLICY, childPlacementPolicies.Keys, SIBLINGS_ON_SAME_LAYER);
      var globalLayeringItem = directedGroup.AddBool(ENFORCE_GLOBAL_LAYERING, false);

      cm.SetEnabledOnCondition(ConstraintManager.LogicalCondition.And(cm.CreateValueEqualsCondition(edgeRoutingOption, true),
        ConstraintManager.LogicalCondition.Or(cm.CreateValueEqualsCondition(globalLayeringItem, true), 
        cm.CreateValueEqualsCondition(childPolicyItem, ALL_LEAVES_ON_SAME_LAYER))), 
        busAlignmentOption);
      
      var ar = new AspectRatioTreeLayout();
      OptionGroup arGroup = Handler.AddGroup(AR);
      arGroup.AddInt( HORIZONTAL_SPACE, (int)ar.HorizontalDistance);
      arGroup.AddInt( VERTICAL_SPACE, (int)ar.VerticalDistance);
      arGroup.AddInt( BEND_DISTANCE, (int)ar.BendDistance);
      IOptionItem ratioItem = arGroup.AddDouble( ASPECT_RATIO, ar.AspectRatio);
      IOptionItem useViewItem = arGroup.AddBool( USE_VIEW_ASPECT_RATIO, true);
      cm.SetEnabledOnValueEquals(useViewItem, false, ratioItem);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      string style = (string) Handler.GetValue(GENERAL, LAYOUT_STYLE);
      if (style.Equals(DIRECTED)) {
        multiStageLayout = ConfigureDirectedRouter();
      } else if (style.Equals(AR)) {
        multiStageLayout = ConfigureARRouter();
      }

      ((ComponentLayout)multiStageLayout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;
      multiStageLayout.SubgraphLayoutEnabled = (bool) Handler.GetValue(GENERAL, ACT_ON_SELECTION_ONLY);
      LayoutAlgorithm = multiStageLayout;
    }

    /// <summary>
    /// configures tree reduction state and non-tree edge routing.
    /// </summary>
    protected override void PerformPreLayout() {
      if ((bool)Handler.GetValue(GENERAL, ALLOW_NON_TREE_EDGES)) {
        additionalStage = new TreeReductionStage();
        multiStageLayout.AppendStage(additionalStage);
        string routingStyleChoice = (string)Handler.GetValue(GENERAL, ROUTING_STYLE_FOR_NON_TREE_EDGES);
        switch (routingStyleChoice) {
          case ROUTE_ORGANIC:
            OrganicEdgeRouter organic = new OrganicEdgeRouter();
            additionalStage.NonTreeEdgeRouter = organic;
            additionalStage.NonTreeEdgeSelectionKey = OrganicEdgeRouter.AffectedEdgesDpKey;
            break;
          case ROUTE_ORTHOGONAL:
            EdgeRouter orthogonal = new EdgeRouter {
              Rerouting = true,
              Scope = Scope.RouteAffectedEdges
            };

            additionalStage.NonTreeEdgeSelectionKey = orthogonal.AffectedEdgesDpKey;
            additionalStage.NonTreeEdgeRouter = orthogonal;
            break;
          case ROUTE_STRAIGHTLINE:
            additionalStage.NonTreeEdgeRouter = additionalStage.CreateStraightLineRouter();
            break;
          case ROUTE_BUNDLED:
            EdgeBundling ebc = additionalStage.EdgeBundling;
            EdgeBundleDescriptor descriptor = new EdgeBundleDescriptor();
            descriptor.Bundled = true;
            ebc.DefaultBundleDescriptor = descriptor;
            ebc.BundlingStrength = (double) Handler.GetValue(GENERAL, EDGE_BUNDLING_STRENGTH);

            // Sets a new straight-line router in case some edges are not bundled, e.g. self-loops
            OrganicEdgeRouter oer = new OrganicEdgeRouter();
            additionalStage.NonTreeEdgeRouter = oer;
            additionalStage.NonTreeEdgeSelectionKey = OrganicEdgeRouter.AffectedEdgesDpKey;
            break;
        }
      }
    }

    /// <summary>
    /// removes additional layout stages and data providers.
    /// </summary>
    protected override void PerformPostLayout() {
      if (additionalStage != null) {
        multiStageLayout.RemoveStage(additionalStage);
        additionalStage = null;
      }
      CurrentLayoutGraph.RemoveDataProvider(AspectRatioTreeLayout.SubtreeRoutingPolicyDpKey);
    }
    #endregion

    #region private helpers
    private MultiStageLayout ConfigureDirectedRouter() {
      ClassicTreeLayout tree = new ClassicTreeLayout
                            {
                              MinimumNodeDistance = (int) Handler.GetValue(DIRECTED, MINIMAL_NODE_DISTANCE),
                              MinimumLayerDistance = (int) Handler.GetValue(DIRECTED, MINIMAL_LAYER_DISTANCE)
                            };

      OrientationLayout ol = (OrientationLayout)tree.OrientationLayout;
      string orientationChoice = (string)Handler.GetValue(DIRECTED, ORIENTATION);
      ol.Orientation = enumOrient[orientationChoice];

      if ((bool)Handler.GetValue(DIRECTED, ORTHOGONAL_EDGE_ROUTING)) {
        tree.EdgeRoutingStyle = yWorks.Layout.Tree.EdgeRoutingStyle.Orthogonal;
      } else {
        tree.EdgeRoutingStyle = yWorks.Layout.Tree.EdgeRoutingStyle.Plain;
      }

      tree.LeafPlacement = childPlacementPolicies[(string) Handler.GetValue(DIRECTED, CHILD_PLACEMENT_POLICY)];
      tree.EnforceGlobalLayering = (bool) Handler.GetValue(DIRECTED, ENFORCE_GLOBAL_LAYERING);

      string portStyleChoice = (string)Handler.GetValue(DIRECTED, PORT_STYLE);
      tree.PortStyle = enumPortStyle[portStyleChoice];

      tree.ConsiderNodeLabels = (bool)Handler.GetValue(DIRECTED, INTEGRATED_NODE_LABELING);
      tree.IntegratedEdgeLabeling = (bool)Handler.GetValue(DIRECTED, INTEGRATED_EDGE_LABELING);

      tree.VerticalAlignment = (double)Handler.GetValue(DIRECTED, VERTICAL_ALIGNMENT);
      tree.BusAlignment = (double)Handler.GetValue(DIRECTED, BUS_ALIGNMENT);

      return tree;
    }
    
    private MultiStageLayout ConfigureARRouter() {
      var ar = new AspectRatioTreeLayout();

      CanvasControl cv = Context.Lookup<CanvasControl>();
      if (cv != null) {
        var size = cv.InnerSize;
        if ((bool)Handler.GetValue(AR, USE_VIEW_ASPECT_RATIO)) {
          ar.AspectRatio = size.Width / size.Height;
        } else {
          ar.AspectRatio = (double) Handler.GetValue(AR, ASPECT_RATIO);
        }
      }

      ar.HorizontalDistance = (int) Handler.GetValue(AR, HORIZONTAL_SPACE);
      ar.VerticalDistance = (int) Handler.GetValue(AR, VERTICAL_SPACE);
      ar.BendDistance = (int) Handler.GetValue(AR, BEND_DISTANCE);

      DataProviderAdapter dp = new ARRoutingDataProviderAdapter(this);
      CurrentLayoutGraph.AddDataProvider(AspectRatioTreeLayout.SubtreeRoutingPolicyDpKey, dp);
      
      return ar;
    }

    private class ARRoutingDataProviderAdapter : DataProviderAdapter
    {
      private readonly LayoutModule module;
      public ARRoutingDataProviderAdapter(LayoutModule module) {
        this.module = module;
      }
      public override object Get(object node) {
        if (module.IsSelected((Node)node)) {
          return SubtreeArrangement.Vertical;
        } else {
          return SubtreeArrangement.Horizontal;
        }
      }
    }
    #endregion
  }
}
