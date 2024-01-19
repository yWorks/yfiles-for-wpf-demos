/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Layout;
using yWorks.Layout.Router;
using yWorks.Layout.Router.Polyline;
using yWorks.Layout.Tree;

// ReSharper disable once CheckNamespace
namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="BalloonLayout"/>.
  /// </summary>
  public class BalloonLayoutModule : LayoutModule
  {
    #region configuration constants
// ReSharper disable InconsistentNaming
    private const string PREFERRED_CHILD_WEDGE = "PREFERRED_CHILD_WEDGE";
    private const string DIRECTED_ROOT = "DIRECTED_ROOT";
    private const string ALLOW_OVERLAPS = "ALLOW_OVERLAPS";

    private const string BALLOON = "BALLOON";
    private const string GENERAL = "GENERAL";
    private const string LABELING = "LABELING";

    private const string ALLOW_NON_TREE_EDGES = "ALLOW_NON_TREES";
    private const string ROUTING_STYLE_FOR_NON_TREE_EDGES = "ROUTING_STYLE_FOR_NON_TREE_EDGES";
    private const string ROUTE_ORGANIC = "ROUTE_ORGANIC";
    private const string ROUTE_ORTHOGONAL = "ROUTE_ORTHOGONAL";
    private const string ROUTE_STRAIGHTLINE = "ROUTE_STRAIGHTLINE";
    private const string ROUTE_BUNDLED = "ROUTE_BUNDLED";

    private const string COMPACTNESS_FACTOR = "COMPACTNESS_FACTOR";
    private const string ACT_ON_SELECTION_ONLY = "ACT_ON_SELECTION_ONLY";
    private const string PREFERRED_ROOT_WEDGE = "PREFERRED_ROOT_WEDGE";
    private const string MINIMAL_EDGE_LENGTH = "MINIMAL_EDGE_LENGTH";
    private const string ROOT_NODE_POLICY = "ROOT_NODE_POLICY";
    private const string CENTER_ROOT = "CENTER_ROOT";
    private const string WEIGHTED_CENTER_ROOT = "WEIGHTED_CENTER_ROOT";
    private const string SELECTED_ROOT = "SELECTED_ROOT";
    private const string STRAIGHTEN_CHAINS = "STRAIGHTEN_CHAINS";
    private const string PLACE_CHILDREN_INTERLEAVED = "PLACE_CHILDREN_INTERLEAVED";
    private const string BALLOON_FROM_SKETCH = "FROM_SKETCH";
    private const string EDGE_BUNDLING_STRENGTH = "EDGE_BUNDLING_STRENGTH";
    
    private const string NODE_LABELING_STYLE = "NODE_LABELING_STYLE";
    private const string NODE_LABELING_STYLE_NONE = "NODE_LABELING_STYLE_NONE";
    private const string NODE_LABELING_STYLE_HORIZONTAL = "NODE_LABELING_STYLE_HORIZONTAL";
    private const string NODE_LABELING_STYLE_RAYLIKE_LEAVES = "NODE_LABELING_STYLE_RAYLIKE_LEAVES";
    private const string NODE_LABELING_STYLE_CONSIDER_CURRENT_POSITION = "NODE_LABELING_STYLE_CONSIDER_CURRENT_POSITION";
    private const string INTEGRATED_EDGE_LABELING = "INTEGRATED_EDGE_LABELING";

    private static readonly List<string> enumRoute = new List<string>();
    private static readonly Dictionary<string, RootNodePolicy> enumRoot = new Dictionary<string, RootNodePolicy>();

    private static readonly String[] nodeLabelingPolicies =
    {
      NODE_LABELING_STYLE_NONE, NODE_LABELING_STYLE_HORIZONTAL,
      NODE_LABELING_STYLE_RAYLIKE_LEAVES, NODE_LABELING_STYLE_CONSIDER_CURRENT_POSITION
    };

    static BalloonLayoutModule()
    {
      enumRoute.Add(ROUTE_ORTHOGONAL);
      enumRoute.Add(ROUTE_ORGANIC);
      enumRoute.Add(ROUTE_STRAIGHTLINE);
      enumRoute.Add(ROUTE_BUNDLED);
      
      enumRoot.Add(DIRECTED_ROOT, RootNodePolicy.DirectedRoot);
      enumRoot.Add(CENTER_ROOT, RootNodePolicy.CenterRoot);
      enumRoot.Add(WEIGHTED_CENTER_ROOT, RootNodePolicy.WeightedCenterRoot);
      enumRoot.Add(SELECTED_ROOT, RootNodePolicy.SelectedRoot);

    }
    #endregion

    #region private members
    private MultiStageLayout multiStageLayout;
    private TreeReductionStage additionalStage;
    #endregion

    /// <summary>
    /// Create new instance
    /// </summary>
    public BalloonLayoutModule() : base(BALLOON) { }

    #region LayoutModule interface
    ///<inheritdoc/>
    protected override void SetupHandler()
    {
      BalloonLayout balloonLayout = new BalloonLayout();

      OptionGroup generalGroup = Handler.AddGroup(GENERAL);
      generalGroup.AddList(ROOT_NODE_POLICY, enumRoot.Keys, DIRECTED_ROOT);
      IOptionItem allowNonTreeItem = generalGroup.AddBool(ALLOW_NON_TREE_EDGES, true);
      IOptionItem nonTreeStyleItem = generalGroup.AddList(ROUTING_STYLE_FOR_NON_TREE_EDGES, enumRoute, ROUTE_ORTHOGONAL);
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(allowNonTreeItem, true, nonTreeStyleItem);
      IOptionItem ebs = generalGroup.AddDouble(EDGE_BUNDLING_STRENGTH, 0.99, 0, 1);
      cm.SetEnabledOnValueEquals(nonTreeStyleItem, ROUTE_BUNDLED, ebs);
      generalGroup.AddBool(ACT_ON_SELECTION_ONLY, false);
      generalGroup.AddInt(PREFERRED_CHILD_WEDGE, balloonLayout.PreferredChildWedge, 1, 359);
      generalGroup.AddInt(PREFERRED_ROOT_WEDGE, balloonLayout.PreferredRootWedge, 1, 360);
      generalGroup.AddInt(MINIMAL_EDGE_LENGTH, balloonLayout.MinimumEdgeLength, 10, int.MaxValue);
      generalGroup.AddDouble(COMPACTNESS_FACTOR, balloonLayout.CompactnessFactor, 0.1, 0.9);
      generalGroup.AddBool(ALLOW_OVERLAPS, balloonLayout.AllowOverlaps);
      generalGroup.AddBool(BALLOON_FROM_SKETCH, balloonLayout.FromSketchMode);
      generalGroup.AddBool(PLACE_CHILDREN_INTERLEAVED, balloonLayout.InterleavedMode == InterleavedMode.AllNodes);
      generalGroup.AddBool(STRAIGHTEN_CHAINS, balloonLayout.ChainStraighteningMode);

      OptionGroup labelingGroup = Handler.AddGroup(LABELING);
      labelingGroup.AddBool(INTEGRATED_EDGE_LABELING, true);
      labelingGroup.AddList(NODE_LABELING_STYLE, nodeLabelingPolicies, nodeLabelingPolicies[3]);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout()
    {
      BalloonLayout balloon = new BalloonLayout();
      ((ComponentLayout)balloon.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;
      
      string rootNodePolicyChoice = (string)Handler.GetValue(GENERAL, ROOT_NODE_POLICY);
      balloon.RootNodePolicy = enumRoot[rootNodePolicyChoice];

      balloon.PreferredChildWedge = (int)Handler.GetValue(GENERAL, PREFERRED_CHILD_WEDGE);
      balloon.PreferredRootWedge = (int)Handler.GetValue(GENERAL, PREFERRED_ROOT_WEDGE);
      balloon.MinimumEdgeLength = (int) Handler.GetValue(GENERAL, MINIMAL_EDGE_LENGTH);
      balloon.CompactnessFactor = (double)Handler.GetValue(GENERAL, COMPACTNESS_FACTOR);
      balloon.AllowOverlaps = (bool)Handler.GetValue(GENERAL, ALLOW_OVERLAPS);
      balloon.FromSketchMode = (bool)Handler.GetValue(GENERAL, BALLOON_FROM_SKETCH);

      balloon.ChainStraighteningMode = (bool)Handler.GetValue(GENERAL, STRAIGHTEN_CHAINS);
      balloon.InterleavedMode = (bool)Handler.GetValue(GENERAL, PLACE_CHILDREN_INTERLEAVED) ? InterleavedMode.AllNodes : InterleavedMode.Off;

      balloon.IntegratedEdgeLabeling = (bool) Handler.GetValue(LABELING, INTEGRATED_EDGE_LABELING);
      switch ((string)Handler.GetValue(LABELING, NODE_LABELING_STYLE)) {
        case NODE_LABELING_STYLE_RAYLIKE_LEAVES:
          balloon.IntegratedNodeLabeling = true;
          balloon.NodeLabelingPolicy = NodeLabelingPolicy.RayLikeLeaves;
          break;
        case NODE_LABELING_STYLE_CONSIDER_CURRENT_POSITION:
          balloon.ConsiderNodeLabels = true;
          break;
        case NODE_LABELING_STYLE_HORIZONTAL:
          balloon.IntegratedEdgeLabeling = true;
          balloon.NodeLabelingPolicy = NodeLabelingPolicy.Horizontal;
          break;
      }
      
      balloon.SubgraphLayoutEnabled = (bool)Handler.GetValue(GENERAL, ACT_ON_SELECTION_ONLY);
      multiStageLayout = balloon;
      LayoutAlgorithm = balloon;
    }

    /// <summary>
    /// configures tree reduction state and non-tree edge routing.
    /// </summary>
    protected override void PerformPreLayout()
    {
      if ((bool)Handler.GetValue(GENERAL, ALLOW_NON_TREE_EDGES))
      {
        additionalStage = new TreeReductionStage();
        multiStageLayout.AppendStage(additionalStage);
        string routingStyleChoice = (string)Handler.GetValue(GENERAL, ROUTING_STYLE_FOR_NON_TREE_EDGES);
        if (ROUTE_ORGANIC.Equals(routingStyleChoice))
        {
          OrganicEdgeRouter organic = new OrganicEdgeRouter();
          additionalStage.NonTreeEdgeRouter = organic;
          additionalStage.NonTreeEdgeSelectionKey = OrganicEdgeRouter.AffectedEdgesDpKey;
        }
        if (ROUTE_ORTHOGONAL.Equals(routingStyleChoice))
        {
          EdgeRouter orthogonal = new EdgeRouter()
          {
            Scope = Scope.RouteAffectedEdges
          };

          additionalStage.NonTreeEdgeSelectionKey = orthogonal.AffectedEdgesDpKey;
          additionalStage.NonTreeEdgeRouter = orthogonal;
        }
        if (ROUTE_STRAIGHTLINE.Equals(routingStyleChoice)) {
          additionalStage.NonTreeEdgeRouter = additionalStage.CreateStraightLineRouter();
        } else if (ROUTE_BUNDLED.Equals(routingStyleChoice)) {
          // Edge Bundling
          EdgeBundling ebc = additionalStage.EdgeBundling;
          EdgeBundleDescriptor descriptor = new EdgeBundleDescriptor();
          descriptor.Bundled = true;
          ebc.DefaultBundleDescriptor = descriptor;
          ebc.BundlingStrength = (double) Handler.GetValue(GENERAL, EDGE_BUNDLING_STRENGTH);
          // Sets a new straight-line router in case some edges are not bundled, e.g. self-loops
          additionalStage.NonTreeEdgeRouter = additionalStage.CreateStraightLineRouter();
        }
      }
    }

    /// <summary>
    /// removes additional layout stages and data providers.
    /// </summary>
    protected override void PerformPostLayout()
    {
      if (additionalStage != null)
      {
        multiStageLayout.RemoveStage(additionalStage);
        additionalStage = null;
      }
      CurrentLayoutGraph.RemoveDataProvider(AspectRatioTreeLayout.SubtreeRoutingPolicyDpKey);
    }
    #endregion

  }
}
