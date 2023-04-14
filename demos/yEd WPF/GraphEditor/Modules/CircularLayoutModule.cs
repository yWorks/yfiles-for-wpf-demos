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

using System.Collections.Generic;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Circular;
using yWorks.Layout.Grouping;
using yWorks.Layout.Tree;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="CircularLayout"/>.
  /// </summary>
  /// Author: yFiles Layout Team
  public class CircularLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string CIRCULAR = "CIRCULAR";

    private const string ALLOW_OVERLAPS = "ALLOW_OVERLAPS";
    private const string COMPACTNESS_FACTOR = "COMPACTNESS_FACTOR";
    private const string MAXIMAL_DEVIATION_ANGLE = "MAXIMAL_DEVIATION_ANGLE";
    private const string MINIMAL_EDGE_LENGTH = "MINIMAL_EDGE_LENGTH";
    private const string PREFERRED_CHILD_WEDGE = "PREFERRED_CHILD_WEDGE";
    private const string TREE = "TREE";
    private const string FIXED_RADIUS = "FIXED_RADIUS";
    private const string CHOOSE_RADIUS_AUTOMATICALLY = "CHOOSE_RADIUS_AUTOMATICALLY";
    private const string MINIMAL_NODE_DISTANCE = "MINIMAL_NODE_DISTANCE";
    private const string MINIMAL_TREE_NODE_DISTANCE = "MINIMAL_TREE_NODE_DISTANCE";
    private const string CYCLE = "CYCLE";
    private const string ACT_ON_SELECTION_ONLY = "ACT_ON_SELECTION_ONLY";
    private const string LAYOUT_STYLE = "LAYOUT_STYLE";
    private const string GENERAL = "GENERAL";
    private const string SINGLE_CYCLE = "SINGLE_CYCLE";
    private const string BCC_ISOLATED = "BCC_ISOLATED";
    private const string BCC_COMPACT = "BCC_COMPACT";
    private const string CUSTOM_GROUPS = "CUSTOM_GROUPS";
    private const string FROM_SKETCH = "FROM_SKETCH";
    private const string HANDLE_NODE_LABELS = "HANDLE_NODE_LABELS";
    private const string PLACE_CHILDREN_ON_COMMON_RADIUS = "PLACE_CHILDREN_ON_COMMON_RADIUS";
    private const string EDGE_BUNDLING = "EDGE_BUNDLING";
    private const string EDGE_BUNDLING_ENABLED = "EDGE_BUNDLING_ENABLED";
    private const string EDGE_BUNDLING_STRENGTH = "EDGE_BUNDLING_STRENGTH";

    private const string PARTITION_LAYOUT_STYLE = "PARTITION_LAYOUT_STYLE";
    private const string PARTITION_LAYOUTSTYLE_CYCLIC = "PARTITION_LAYOUTSTYLE_CYCLIC";
    private const string PARTITION_LAYOUTSTYLE_DISK = "PARTITION_LAYOUTSTYLE_DISK";
    private const string PARTITION_LAYOUTSTYLE_ORGANIC = "PARTITION_LAYOUTSTYLE_ORGANIC";

    private static readonly Dictionary<string, LayoutStyle> globalLayoutStyles = new Dictionary<string, LayoutStyle>(3);
    private static readonly Dictionary<string, PartitionStyle> partitionLayoutStyles = new Dictionary<string, PartitionStyle>(3);

    static CircularLayoutModule() {
      globalLayoutStyles.Add(BCC_COMPACT, LayoutStyle.BccCompact);
      globalLayoutStyles.Add(BCC_ISOLATED, LayoutStyle.BccIsolated);
      globalLayoutStyles.Add(CUSTOM_GROUPS, LayoutStyle.CustomGroups);
      globalLayoutStyles.Add(SINGLE_CYCLE, LayoutStyle.SingleCycle);
      partitionLayoutStyles.Add(PARTITION_LAYOUTSTYLE_CYCLIC, PartitionStyle.Cycle);
      partitionLayoutStyles.Add(PARTITION_LAYOUTSTYLE_DISK, PartitionStyle.Disk);
      partitionLayoutStyles.Add(PARTITION_LAYOUTSTYLE_ORGANIC, PartitionStyle.Organic);
    }

    #endregion
    /// <summary>
    /// Create a new instance
    /// </summary>
    public CircularLayoutModule() : base(CIRCULAR) {}

    #region LayoutModule interface

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      CircularLayout layout = new CircularLayout();
      ((ComponentLayout) layout.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;

      BalloonLayout treeLayout = layout.BalloonLayout;

      layout.LayoutStyle = globalLayoutStyles[(string) Handler.GetValue(GENERAL, LAYOUT_STYLE)];
      layout.SubgraphLayoutEnabled = (bool) Handler.GetValue(GENERAL, ACT_ON_SELECTION_ONLY);
      layout.MaximumDeviationAngle = (int) Handler.GetValue(TREE, MAXIMAL_DEVIATION_ANGLE);
      layout.FromSketchMode = (bool) Handler.GetValue(GENERAL, FROM_SKETCH);
      layout.ConsiderNodeLabels = (bool) Handler.GetValue(GENERAL, HANDLE_NODE_LABELS);

      layout.PartitionStyle = partitionLayoutStyles[(string) Handler.GetValue(CYCLE, PARTITION_LAYOUT_STYLE)];

      layout.SingleCycleLayout.MinimumNodeDistance = (int) Handler.GetValue(CYCLE, MINIMAL_NODE_DISTANCE);
      layout.SingleCycleLayout.AutomaticRadius = (bool) Handler.GetValue(CYCLE, CHOOSE_RADIUS_AUTOMATICALLY);
      layout.SingleCycleLayout.FixedRadius = (int) Handler.GetValue(CYCLE, FIXED_RADIUS);

      treeLayout.PreferredChildWedge = ((int) Handler.GetValue(TREE, PREFERRED_CHILD_WEDGE));
      treeLayout.MinimumEdgeLength = (int) Handler.GetValue(TREE, MINIMAL_EDGE_LENGTH);
      treeLayout.CompactnessFactor = (double) Handler.GetValue(TREE, COMPACTNESS_FACTOR);
      treeLayout.AllowOverlaps = (bool) Handler.GetValue(TREE, ALLOW_OVERLAPS);
      layout.PlaceChildrenOnCommonRadius = (bool)Handler.GetValue(TREE, PLACE_CHILDREN_ON_COMMON_RADIUS);
      treeLayout.MinimumNodeDistance = (int) Handler.GetValue(TREE, MINIMAL_TREE_NODE_DISTANCE);
      
      // Edge Bundling
      EdgeBundling ebc = layout.EdgeBundling;
      EdgeBundleDescriptor descriptor = new EdgeBundleDescriptor();
      descriptor.Bundled = (bool) Handler.GetValue(EDGE_BUNDLING, EDGE_BUNDLING_ENABLED);
      ebc.DefaultBundleDescriptor = descriptor;
      ebc.BundlingStrength = (double) Handler.GetValue(EDGE_BUNDLING, EDGE_BUNDLING_STRENGTH);

      LayoutAlgorithm = layout;
    }

    ///<inheritdoc/>
    protected override void SetupHandler() {
      CircularLayout layout = new CircularLayout();
      BalloonLayout treeLayout = layout.BalloonLayout;

      OptionGroup generalGroup = Handler.AddGroup(GENERAL);
      var styleItem = generalGroup.AddList( LAYOUT_STYLE, globalLayoutStyles.Keys, BCC_COMPACT);
      generalGroup.AddBool( ACT_ON_SELECTION_ONLY, false);
      generalGroup.AddBool( FROM_SKETCH, false);
      generalGroup.AddBool( HANDLE_NODE_LABELS, false);

      OptionGroup cycleGroup = Handler.AddGroup(CYCLE);
      cycleGroup.AddList( PARTITION_LAYOUT_STYLE, partitionLayoutStyles.Keys, PARTITION_LAYOUTSTYLE_CYCLIC);
      IOptionItem minDistItem = cycleGroup.AddInt( MINIMAL_NODE_DISTANCE, 30, 0, int.MaxValue);
      IOptionItem autoRadiusItem = cycleGroup.AddBool(CHOOSE_RADIUS_AUTOMATICALLY, true);
      IOptionItem fixedRadiusItem = cycleGroup.AddInt(FIXED_RADIUS, 200, 1, int.MaxValue);

      ConstraintManager cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(autoRadiusItem, true, minDistItem);
      cm.SetEnabledOnValueEquals(autoRadiusItem, false, fixedRadiusItem);

      OptionGroup bundlingGroup = Handler.AddGroup(EDGE_BUNDLING);
      IOptionItem bundlingEnabled = bundlingGroup.AddBool(EDGE_BUNDLING_ENABLED, false);
      IOptionItem bundlingStrength = bundlingGroup.AddDouble(EDGE_BUNDLING_STRENGTH, 0.95, 0, 1);
      cm.SetEnabledOnValueEquals(bundlingEnabled, true, bundlingStrength);

      OptionGroup treeGroup = Handler.AddGroup(TREE);
      treeGroup.AddInt( PREFERRED_CHILD_WEDGE, treeLayout.PreferredChildWedge, 1, 360);
      treeGroup.AddInt(MINIMAL_EDGE_LENGTH, treeLayout.MinimumEdgeLength, 1, int.MaxValue);
      treeGroup.AddInt( MAXIMAL_DEVIATION_ANGLE, layout.MaximumDeviationAngle, 1, 360);
      treeGroup.AddDouble( COMPACTNESS_FACTOR, treeLayout.CompactnessFactor, 0.1, 0.9);
      treeGroup.AddInt(MINIMAL_TREE_NODE_DISTANCE, treeLayout.MinimumNodeDistance, 0, int.MaxValue);
      treeGroup.AddBool( ALLOW_OVERLAPS, treeLayout.AllowOverlaps);
      treeGroup.AddBool(PLACE_CHILDREN_ON_COMMON_RADIUS, true);

      cm.SetEnabledOnCondition(ConstraintManager.LogicalCondition.Not(cm.CreateValueEqualsCondition(styleItem, SINGLE_CYCLE)), treeGroup);
    }

    ///<inheritdoc/>
    protected override void PerformPreLayout() {
      base.PerformPreLayout();
      string layoutStyle = (string) Handler.GetValue(GENERAL, LAYOUT_STYLE);
      IDataProvider customGroupDP = CurrentLayoutGraph.GetDataProvider(CircularLayout.CustomGroupsDpKey);
      if (layoutStyle.Equals(CUSTOM_GROUPS) && customGroupDP == null) {
        IDataProvider groupInfoDP = CurrentLayoutGraph.GetDataProvider(GroupingKeys.ParentNodeIdDpKey);
        if (groupInfoDP == null) {
          //Set up dummy data provider for custom group layout style.
          CurrentLayoutGraph.AddDataProvider(CircularLayout.CustomGroupsDpKey, DataProviders.CreateConstantDataProvider(null));
        } else {
          // use existing group info
          CurrentLayoutGraph.AddDataProvider(CircularLayout.CustomGroupsDpKey, groupInfoDP);
        }
      }
    }

    #endregion
  }
}
