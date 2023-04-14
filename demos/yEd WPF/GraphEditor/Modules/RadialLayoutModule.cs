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
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Layout;
using yWorks.Layout.Radial;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{

  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="RadialLayout"/>.
  /// </summary>
  public class RadialLayoutModule : LayoutModule
  {

    #region configuration constants

    // option handler title
    private const String RADIAL = "RADIAL";

    // layout settings
    private const int MAXIMUM_SMOOTHNESS = 10;
    private const int MINIMUM_SMOOTHNESS = 1;
    private const int SMOOTHNESS_ANGLE_FACTOR = 4;

    private const String GENERAL = "GENERAL";
    private const String MINIMAL_NODE_DISTANCE = "MINIMAL_NODE_DISTANCE";
    private const String MINIMAL_LAYER_DISTANCE = "MINIMAL_LAYER_DISTANCE";
    private const String MAXIMAL_CHILD_SECTOR_SIZE = "MAXIMAL_CHILD_SECTOR_SIZE";
    private const String EDGE_SMOOTHNESS = "EDGE_SMOOTHNESS";
    private const String EDGE_BUNDLING_STRENGTH = "EDGE_BUNDLING_STRENGTH";
    private const String CONSIDER_NODE_LABELS = "CONSIDER_NODE_LABELS";

    private const String EDGE_ROUTING_STRATEGY = "EDGE_ROUTING_STRATEGY";
    private const String EDGE_POLYLINE = "EDGE_POLYLINE";
    private const String EDGE_ARC = "EDGE_ARC";
    private const String EDGE_BUNDLES = "EDGE_BUNDLES";
    private static readonly List<String> edgeRoutingStrategies = new List<string>(3);

    private const String CENTER_STRATEGY = "CENTER_STRATEGY";
    private const String CENTER_DIRECTED = "CENTER_DIRECTED";
    private const String CENTER_CENTRAL = "CENTER_CENTRAL";
    private const String CENTER_WEIGHTED_CENTRAL = "CENTER_WEIGHTED_CENTRAL";
    private const String CENTER_SELECTED = "CENTER_SELECTED";

    private static readonly Dictionary<String, CenterNodesPolicy> centerNodeStrategies = new Dictionary<string, CenterNodesPolicy>(4);

    private const String LAYERING_STRATEGY = "LAYERING_STRATEGY";
    private const String LAYERING_BFS = "LAYERING_BFS";
    private const String LAYERING_HIERARCHICAL = "LAYERING_HIERARCHICAL";
    private static readonly Dictionary<String, LayeringStrategy> layeringStrategies = new Dictionary<string, LayeringStrategy>(2);
    private RadialLayout layout;

    static RadialLayoutModule()  {
      edgeRoutingStrategies.Add(EDGE_POLYLINE);
      edgeRoutingStrategies.Add(EDGE_ARC);
      edgeRoutingStrategies.Add(EDGE_BUNDLES);

      centerNodeStrategies.Add(CENTER_DIRECTED, CenterNodesPolicy.Directed);
      centerNodeStrategies.Add(CENTER_CENTRAL, CenterNodesPolicy.Centrality);
      centerNodeStrategies.Add(CENTER_WEIGHTED_CENTRAL, CenterNodesPolicy.WeightedCentrality);
      centerNodeStrategies.Add(CENTER_SELECTED, CenterNodesPolicy.Custom);

      layeringStrategies.Add(LAYERING_BFS, LayeringStrategy.Bfs);
      layeringStrategies.Add(LAYERING_HIERARCHICAL, LayeringStrategy.Hierarchical);
    }

    #endregion
    
    public RadialLayoutModule() : base(RADIAL) {}

    protected override void SetupHandler() {
      CreateLayout();

      OptionGroup generalGroup = Handler.AddGroup(GENERAL);

      generalGroup.AddList(CENTER_STRATEGY, centerNodeStrategies.Keys, CENTER_WEIGHTED_CENTRAL);
      generalGroup.AddList(LAYERING_STRATEGY, layeringStrategies.Keys, LAYERING_BFS);
      generalGroup.AddDouble(MINIMAL_LAYER_DISTANCE, (int) layout.MinimumLayerDistance, 1, 1000);
      generalGroup.AddDouble(MINIMAL_NODE_DISTANCE, (int)layout.MinimumNodeToNodeDistance, 0, 300);
      generalGroup.AddDouble(MAXIMAL_CHILD_SECTOR_SIZE, (int)layout.MaximumChildSectorAngle, 15, 360);

      OptionItem routingStrategyItem = generalGroup.AddList(EDGE_ROUTING_STRATEGY, edgeRoutingStrategies, EDGE_ARC);

      int smoothness =
        (int)
          Math.Min(MAXIMUM_SMOOTHNESS,
            (1 + MAXIMUM_SMOOTHNESS*SMOOTHNESS_ANGLE_FACTOR - layout.MinimumBendAngle)/SMOOTHNESS_ANGLE_FACTOR);
      IOptionItem smoothnessItem = generalGroup.AddInt(EDGE_SMOOTHNESS, smoothness, MINIMUM_SMOOTHNESS, MAXIMUM_SMOOTHNESS);
      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(routingStrategyItem, EDGE_ARC, smoothnessItem);
      var bundlingStrength = generalGroup.AddDouble(EDGE_BUNDLING_STRENGTH, 0.99, 0, 1);
      cm.SetEnabledOnValueEquals(routingStrategyItem, EDGE_BUNDLES, bundlingStrength);
      generalGroup.AddBool(CONSIDER_NODE_LABELS, layout.ConsiderNodeLabels);
    }

    private void CreateLayout() {
      if (layout == null) {
        layout = new RadialLayout();
      }
    }

    protected override void ConfigureLayout() {
      OptionGroup generalGroup = Handler.GetGroupByName(GENERAL);

      RadialLayout radialLayout = new RadialLayout();
      radialLayout.MinimumNodeToNodeDistance = (double) generalGroup[MINIMAL_NODE_DISTANCE].Value;
      String strategy = (string) generalGroup[EDGE_ROUTING_STRATEGY].Value;
      switch (strategy) {
        case EDGE_POLYLINE:
          radialLayout.EdgeRoutingStrategy = EdgeRoutingStrategy.Polyline;
          break;
        case EDGE_ARC:
          radialLayout.EdgeRoutingStrategy = EdgeRoutingStrategy.Arc;
          break;
        case EDGE_BUNDLES:
          EdgeBundling ebc = radialLayout.EdgeBundling;
          EdgeBundleDescriptor descriptor = new EdgeBundleDescriptor();
          descriptor.Bundled = true;
          ebc.DefaultBundleDescriptor = descriptor;
          ebc.BundlingStrength = (double) Handler.GetValue(GENERAL, EDGE_BUNDLING_STRENGTH);
          break;

      }

      double minimumBendAngle = 1 +
                                (MAXIMUM_SMOOTHNESS - (int) generalGroup[EDGE_SMOOTHNESS].Value)*
                                SMOOTHNESS_ANGLE_FACTOR;
      radialLayout.MinimumBendAngle = minimumBendAngle;
      radialLayout.MinimumLayerDistance = (double) generalGroup[MINIMAL_LAYER_DISTANCE].Value;
      radialLayout.MaximumChildSectorAngle = (double) generalGroup[MAXIMAL_CHILD_SECTOR_SIZE].Value;

      String centerStrategy = (string) generalGroup[CENTER_STRATEGY].Value;
      radialLayout.CenterNodesPolicy = centerNodeStrategies[centerStrategy];

      radialLayout.LayeringStrategy = layeringStrategies[(string) generalGroup[LAYERING_STRATEGY].Value];

      radialLayout.ConsiderNodeLabels = (bool) generalGroup[CONSIDER_NODE_LABELS].Value;

      LayoutAlgorithm = radialLayout;
    }
  }
}
