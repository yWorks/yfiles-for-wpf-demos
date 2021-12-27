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
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Algorithms.Util;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Organic;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents an interactive configurator and launcher for
  /// <see cref="OrganicLayout"/>.
  /// </summary>
  public class SmartOrganicLayoutModule : LayoutModule
  {
    #region configuration constants

    private const string ACTIVATE_DETERMINISTIC_MODE = "ACTIVATE_DETERMINISTIC_MODE";
    private const string ALLOW_MULTI_THREADING = "ALLOW_MULTI_THREADING";
    private const string VISUAL = "VISUAL";
    private const string ALGORITHM = "ALGORITHM";
    private const string COMPACTNESS = "COMPACTNESS";
    private const string MAXIMAL_DURATION = "MAXIMAL_DURATION";
    private const string CONSIDER_NODE_LABELS = "CONSIDER_NODE_LABELS";
    private const string ALLOW_NODE_OVERLAPS = "ALLOW_NODE_OVERLAPS";
    private const string MINIMAL_NODE_DISTANCE = "MINIMAL_NODE_DISTANCE";
    private const string SCOPE = "SCOPE";
    private const string PREFERRED_EDGE_LENGTH = "PREFERRED_EDGE_LENGTH";
    private const string SMARTORGANIC = "SMARTORGANIC";
    private const string SCOPE_SUBSET = "SUBSET";
    private const string SCOPE_MAINLY_SUBSET = "MAINLY_SUBSET";
    private const string SCOPE_ALL = "ALL";
    private const string QUALITY_TIME_RATIO = "QUALITY_TIME_RATIO";
    private const string RESTRICT_OUTPUT = "RESTRICT_OUTPUT";
    private const string NONE = "NONE";
    private const string OUTPUT_CAGE = "OUTPUT_CAGE";
    private const string BOUNDS = "BOUNDS";
    private const string OUTPUT_ELLIPTICAL_CAGE = "OUTPUT_ELLIPTICAL_CAGE";
    private const string OUTPUT_AR = "OUTPUT_AR";
    private const string CAGE_X = "CAGE_X";
    private const string CAGE_Y = "CAGE_Y";
    private const string CAGE_WIDTH = "CAGE_WIDTH";
    private const string CAGE_HEIGHT = "CAGE_HEIGHT";
    private const string CAGE_RATIO = "CAGE_RATIO";
    private const string AR_CAGE_USE_VIEW = "AR_CAGE_USE_VIEW";
    private const string RECT_CAGE_USE_VIEW = "RECT_CAGE_USE_VIEW";
    private const string RESTRICTIONS = "RESTRICTIONS";
    private const string AVOID_NODE_EDGE_OVERLAPS = "AVOID_NODE_EDGE_OVERLAPS";

    private const string GROUPING = "GROUPING";
    private const string GROUP_LAYOUT_POLICY = "GROUP_LAYOUT_POLICY";
    private const string IGNORE_GROUPS = "IGNORE_GROUPS";
    private const string LAYOUT_GROUPS = "LAYOUT_GROUPS";
    private const string FIX_GROUP_BOUNDS = "FIX_GROUP_BOUNDS";
    private const string FIX_GROUP_CONTENTS = "FIX_GROUP_CONTENTS";
    private const string USE_AUTO_CLUSTERING = "USE_AUTO_CLUSTERING";
    private const string AUTO_CLUSTERING_QUALITY = "AUTO_CLUSTERING_QUALITY";

    private static readonly Dictionary<string, Scope> scopes = new Dictionary<string, Scope>(5);
    private static readonly List<string> outputRestrictions = new List<string>();
    private static readonly List<string> groupModes = new List<string>(4);

    static SmartOrganicLayoutModule() {
      scopes.Add(SCOPE_ALL, Scope.All);
      scopes.Add(SCOPE_MAINLY_SUBSET, Scope.MainlySubset);
      scopes.Add(SCOPE_SUBSET, Scope.Subset);

      outputRestrictions.Add(NONE);
      outputRestrictions.Add(OUTPUT_CAGE);
      outputRestrictions.Add(OUTPUT_AR);
      outputRestrictions.Add(OUTPUT_ELLIPTICAL_CAGE);
      groupModes.Add(LAYOUT_GROUPS);
      groupModes.Add(FIX_GROUP_BOUNDS);
      groupModes.Add(FIX_GROUP_CONTENTS);
      groupModes.Add(IGNORE_GROUPS);
    }

    #endregion

    #region private members

    private OrganicLayout organic;
    private ILayoutStage preStage;
    private INodeMap groupNodeContentDP;

    #endregion

    /// <summary>
    /// Create a new instance.
    /// </summary>
    public SmartOrganicLayoutModule() : base(SMARTORGANIC) {}

    #region LayoutModule interface

    ///<inheritdoc/>
    protected override void SetupHandler() {
      CreateOrganic();

      ConstraintManager cm = new ConstraintManager(Handler);
      OptionGroup visualGroup = Handler.AddGroup(VISUAL);
      visualGroup.AddList(SCOPE, scopes.Keys, SCOPE_ALL);
      visualGroup.AddInt(PREFERRED_EDGE_LENGTH, (int) organic.PreferredEdgeLength, 5, int.MaxValue);
      IOptionItem considerNodeLabelsItem = visualGroup.AddBool(CONSIDER_NODE_LABELS, organic.ConsiderNodeLabels);
      IOptionItem allowNodeOverlapsItem = visualGroup.AddBool(ALLOW_NODE_OVERLAPS, organic.NodeOverlapsAllowed);

      IOptionItem minDistItem = visualGroup.AddDouble(MINIMAL_NODE_DISTANCE, 10, 0, double.MaxValue);
      ICondition condition = ConstraintManager.LogicalCondition.Or(cm.CreateValueEqualsCondition(allowNodeOverlapsItem, false),
                                                            cm.CreateValueEqualsCondition(considerNodeLabelsItem, true));
      cm.SetEnabledOnCondition(condition, minDistItem);
      
      visualGroup.AddBool(AVOID_NODE_EDGE_OVERLAPS, organic.NodeEdgeOverlapAvoided);
      cm.SetEnabledOnValueEquals(considerNodeLabelsItem, false, allowNodeOverlapsItem);

      visualGroup.AddDouble(COMPACTNESS, organic.CompactnessFactor, 0, 1);
      visualGroup.AddBool(USE_AUTO_CLUSTERING, organic.ClusterNodes);
      visualGroup.AddDouble(AUTO_CLUSTERING_QUALITY, organic.ClusteringQuality, 0, 1);
      cm.SetEnabledOnValueEquals(visualGroup[USE_AUTO_CLUSTERING], true, visualGroup[AUTO_CLUSTERING_QUALITY]);

      OptionGroup outputRestrictionsGroup = Handler.AddGroup(RESTRICTIONS);

      OptionItem outputRestrictionsItem =
        outputRestrictionsGroup.AddList(RESTRICT_OUTPUT, outputRestrictions, NONE);

      OptionGroup outputCageGroup = outputRestrictionsGroup.AddGroup(BOUNDS);

      ICondition cond = cm.CreateValueIsOneOfCondition(outputRestrictionsItem, OUTPUT_CAGE, OUTPUT_ELLIPTICAL_CAGE);
      cm.SetEnabledOnCondition(cond, outputCageGroup);

      //IOptionItem rectCageUseViewItem = new BoolOptionItem(RECT_CAGE_USE_VIEW, true);
      IOptionItem rectCageUseViewItem = outputCageGroup.AddBool(RECT_CAGE_USE_VIEW, true);
      IOptionItem cageXItem = outputCageGroup.AddDouble(CAGE_X, 0.0d);
      IOptionItem cageYItem = outputCageGroup.AddDouble(CAGE_Y, 0.0d);
      IOptionItem cageWidthItem = outputCageGroup.AddDouble(CAGE_WIDTH, 1000.0d, 1, double.MaxValue);
      IOptionItem cageHeightItem = outputCageGroup.AddDouble(CAGE_HEIGHT, 1000.0d, 1, double.MaxValue);

      cm.SetEnabledOnValueEquals(rectCageUseViewItem, false, cageXItem);
      cm.SetEnabledOnValueEquals(rectCageUseViewItem, false, cageYItem);
      cm.SetEnabledOnValueEquals(rectCageUseViewItem, false, cageWidthItem);
      cm.SetEnabledOnValueEquals(rectCageUseViewItem, false, cageHeightItem);

      OptionGroup outputARGroup = outputRestrictionsGroup.AddGroup(OUTPUT_AR);
      cm.SetEnabledOnValueEquals(outputRestrictionsItem, OUTPUT_AR, outputARGroup);
      IOptionItem arCageUseViewItem = outputARGroup.AddBool(AR_CAGE_USE_VIEW, true);
      IOptionItem cageRatioItem = outputARGroup.AddDouble(CAGE_RATIO, 1.0d);
      cm.SetEnabledOnValueEquals(arCageUseViewItem, false, cageRatioItem);

      OptionGroup groupingGroup = Handler.AddGroup(GROUPING);
      groupingGroup.AddList(GROUP_LAYOUT_POLICY, groupModes, LAYOUT_GROUPS);

      OptionGroup algorithmGroup = Handler.AddGroup(ALGORITHM);
      algorithmGroup.AddDouble(QUALITY_TIME_RATIO, organic.QualityTimeRatio, 0, 1);
      algorithmGroup.AddInt(MAXIMAL_DURATION, (int) (organic.MaximumDuration/1000), 0, int.MaxValue);
      algorithmGroup.AddBool(ACTIVATE_DETERMINISTIC_MODE, organic.Deterministic);
      algorithmGroup.AddBool(ALLOW_MULTI_THREADING, true);
    }

    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      CreateOrganic();
      OptionGroup visualGroup = Handler.GetGroupByName(VISUAL);
      OptionGroup algorithmGroup = Handler.GetGroupByName(ALGORITHM);

      organic.PreferredEdgeLength = (int) visualGroup[PREFERRED_EDGE_LENGTH].Value;
      bool considerNodeLabels = (bool) visualGroup[CONSIDER_NODE_LABELS].Value;
      organic.ConsiderNodeLabels = considerNodeLabels;

      organic.NodeOverlapsAllowed = (bool) visualGroup[ALLOW_NODE_OVERLAPS].Value && !considerNodeLabels;
      organic.MinimumNodeDistance = (double) visualGroup[MINIMAL_NODE_DISTANCE].Value;
      string scopeChoice = (string) visualGroup[SCOPE].Value;
      organic.Scope = scopes[scopeChoice];
      organic.CompactnessFactor = (double) visualGroup[COMPACTNESS].Value;
      //Doesn't really make sense to ignore node sizes (for certain configurations, this setting
      //doesn't have an effect anyway)
      organic.ConsiderNodeSizes = true;
      organic.ClusterNodes = (bool) visualGroup[USE_AUTO_CLUSTERING].Value;
      organic.ClusteringQuality = (double) visualGroup[AUTO_CLUSTERING_QUALITY].Value;

      organic.NodeEdgeOverlapAvoided = (bool) visualGroup[AVOID_NODE_EDGE_OVERLAPS].Value;

      organic.Deterministic = (bool) algorithmGroup[ACTIVATE_DETERMINISTIC_MODE].Value;
      organic.MaximumDuration = 1000*((int) algorithmGroup[MAXIMAL_DURATION].Value);
      organic.QualityTimeRatio = (double) algorithmGroup[QUALITY_TIME_RATIO].Value;
      organic.MultiThreadingAllowed = (bool) algorithmGroup[ALLOW_MULTI_THREADING].Value;
      ((ComponentLayout)organic.ComponentLayout).Style = ComponentArrangementStyles.MultiRows;

      ConfigureOutputRestrictions();

      LayoutAlgorithm = organic;
    }

    private void ConfigureGrouping() {
      IOptionItem groupingItem = Handler.GetItemByName("GROUPING.GROUP_LAYOUT_POLICY");
      switch ((string) groupingItem.Value) {
        case IGNORE_GROUPS:
          preStage = new HideGroupsStage();
          organic.PrependStage(preStage);
          break;
        case LAYOUT_GROUPS:
          //do nothing...
          break;
        case FIX_GROUP_BOUNDS:
          IDataProvider groupDP = CurrentLayoutGraph.GetDataProvider(GroupingKeys.GroupDpKey);
          if (groupDP != null) {
            groupNodeContentDP = Maps.CreateHashedNodeMap();
            foreach (Node node in CurrentLayoutGraph.Nodes) {
              if (groupDP.GetBool(node)) {
                groupNodeContentDP.Set(node, GroupNodeMode.FixBounds);
              }
            }
            CurrentLayoutGraph.AddDataProvider(OrganicLayout.GroupNodeModeDpKey, groupNodeContentDP);
          }
          break;
        case FIX_GROUP_CONTENTS:
          groupDP = CurrentLayoutGraph.GetDataProvider(GroupingKeys.GroupDpKey);
          if (groupDP != null) {
            groupNodeContentDP = Maps.CreateHashedNodeMap();
            foreach (Node node in CurrentLayoutGraph.Nodes) {
              if (groupDP.GetBool(node)) {
                groupNodeContentDP.Set(node, GroupNodeMode.FixContents);
              }
            }
            CurrentLayoutGraph.AddDataProvider(OrganicLayout.GroupNodeModeDpKey, groupNodeContentDP);
          }
          break;
      }
    }

    ///<inheritdoc/>
    protected override void PerformPreLayout() {
      base.PerformPreLayout();

      CreateOrganic();
      ConfigureGrouping();
      IDataProvider affectedNodeProvider = CurrentLayoutGraph.GetDataProvider(LayoutKeys.AffectedNodesDpKey);

      CurrentLayoutGraph.AddDataProvider(OrganicLayout.AffectedNodesDpKey, affectedNodeProvider);
    }

    ///<inheritdoc/>
    protected override void PerformPostLayout() {
      CurrentLayoutGraph.RemoveDataProvider(OrganicLayout.AffectedNodesDpKey);
      if (preStage != null) {
        organic.RemoveStage(preStage);
        preStage = null;
      }
      if (groupNodeContentDP != null) {
        CurrentLayoutGraph.RemoveDataProvider(OrganicLayout.GroupNodeModeDpKey);
        groupNodeContentDP = null;
      }
      base.PerformPostLayout();
    }

    #endregion

    #region private helpers

    private void CreateOrganic() {
      if (organic == null) {
        organic = new OrganicLayout();
        organic.PrependStage(new PortCalculator());
      }
    }

    private void ConfigureOutputRestrictions() {
      bool viewInfoIsAvailable = false;
      double[] visibleRect = GetVisibleRectangle();
      double x = 0, y = 0, w = 0, h = 0;
      if (visibleRect != null) {
        viewInfoIsAvailable = true;
        x = visibleRect[0];
        y = visibleRect[1];
        w = visibleRect[2];
        h = visibleRect[3];
      }
      OptionGroup restrictionGroup = Handler.GetGroupByName(RESTRICTIONS);
      string restrictionType = (string) restrictionGroup[RESTRICT_OUTPUT].Value;
      OptionGroup currentGroup = null;
      if (restrictionType != NONE) {
        if (restrictionType.Equals(OUTPUT_CAGE) || restrictionType.Equals(OUTPUT_ELLIPTICAL_CAGE)) {
          currentGroup = (OptionGroup) restrictionGroup.GetGroupByName(BOUNDS);
        } else {
          currentGroup = (OptionGroup) restrictionGroup.GetGroupByName(restrictionType);
        }
      }
      switch (restrictionType) {
        case NONE: {
          organic.ComponentLayoutEnabled = true;
          organic.OutputRestriction = OutputRestriction.None;
          break;
        }
        case OUTPUT_CAGE: {
          if (!viewInfoIsAvailable || !(bool) currentGroup[RECT_CAGE_USE_VIEW].Value) {
            x = (double) currentGroup[CAGE_X].Value;
            y = (double) currentGroup[CAGE_Y].Value;
            w = (double) currentGroup[CAGE_WIDTH].Value;
            h = (double) currentGroup[CAGE_HEIGHT].Value;
          }
          organic.OutputRestriction = 
            OutputRestriction.CreateRectangularCageRestriction(x, y, w, h);
          organic.ComponentLayoutEnabled = false;
          break;
        }
        case OUTPUT_AR: {
          double ratio;
          if ((bool) currentGroup[AR_CAGE_USE_VIEW].Value && viewInfoIsAvailable) {
            ratio = w/h;
          } else {
            ratio = (double) currentGroup[CAGE_RATIO].Value;
          }
          organic.OutputRestriction = OutputRestriction.CreateAspectRatioRestriction(ratio);
          organic.ComponentLayoutEnabled = true;
          ((ComponentLayout) organic.ComponentLayout).PreferredSize = new YDimension(ratio*100, 100);
          break;
        }
        case OUTPUT_ELLIPTICAL_CAGE: {
          if (!viewInfoIsAvailable || !((bool) currentGroup[RECT_CAGE_USE_VIEW].Value)) {
            x = (double) currentGroup[CAGE_X].Value;
            y = (double) currentGroup[CAGE_Y].Value;
            w = (double) currentGroup[CAGE_WIDTH].Value;
            h = (double) currentGroup[CAGE_HEIGHT].Value;
          }
          organic.OutputRestriction =
            OutputRestriction.CreateEllipticalCageRestriction(x, y, w, h);
          organic.ComponentLayoutEnabled = false;
          break;
        }
      }
    }

    private double[] GetVisibleRectangle() {
      double[] visibleRect = new double[4];
      CanvasControl cv = Context.Lookup<CanvasControl>();
      if (cv != null) {
        RectD viewPort = cv.Viewport;
        visibleRect[0] = viewPort.X;
        visibleRect[1] = viewPort.Y;
        visibleRect[2] = viewPort.Width;
        visibleRect[3] = viewPort.Height;
        return visibleRect;
      }
      return null;
    }

    #endregion
  }
}
